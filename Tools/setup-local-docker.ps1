param(
    [string]$NetworkName = "events-local",
    [string]$ElasticMqContainerName = "elasticmq",
    [string]$ElasticMqImage = "softwaremill/elasticmq-native",
    [int]$ElasticMqPort = 9324,
    [string]$QueueName = "events-ingestion-local",
    [string]$ServiceImageTag = "events-ingestion-service:local",
    [switch]$RunService
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$hostElasticMqUrl = "http://localhost:$ElasticMqPort"
$dockerElasticMqUrl = "http://${ElasticMqContainerName}:$ElasticMqPort"
$queueUrl = "$dockerElasticMqUrl/000000000000/$QueueName"

function Invoke-SqsAction {
    param(
        [string]$Uri,
        [string]$Body
    )

    Invoke-WebRequest -UseBasicParsing -Method Post -Uri $Uri -ContentType "application/x-www-form-urlencoded" -Body $Body
}

function Wait-ElasticMq {
    Write-Host "Waiting for ElasticMQ at $hostElasticMqUrl..."

    for ($i = 1; $i -le 30; $i++) {
        try {
            Invoke-SqsAction -Uri $hostElasticMqUrl -Body "Action=ListQueues&Version=2012-11-05" | Out-Null
            return
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    throw "ElasticMQ did not become reachable at $hostElasticMqUrl."
}

Write-Host "Ensuring Docker network '$NetworkName'..."
$networkExists = docker network ls --format "{{.Name}}" | Where-Object { $_ -eq $NetworkName }
if (-not $networkExists) {
    docker network create $NetworkName | Out-Null
}

Write-Host "Ensuring ElasticMQ container '$ElasticMqContainerName'..."
$containerExists = docker ps -a --format "{{.Names}}" | Where-Object { $_ -eq $ElasticMqContainerName }
if (-not $containerExists) {
    docker run -d --name $ElasticMqContainerName --network $NetworkName -p "${ElasticMqPort}:9324" $ElasticMqImage | Out-Null
}
else {
    $runningContainer = docker ps --format "{{.Names}}" | Where-Object { $_ -eq $ElasticMqContainerName }
    if (-not $runningContainer) {
        docker start $ElasticMqContainerName | Out-Null
    }

    $networkJson = docker inspect $ElasticMqContainerName --format "{{json .NetworkSettings.Networks}}"
    if ($networkJson -notmatch [regex]::Escape($NetworkName)) {
        docker network connect $NetworkName $ElasticMqContainerName
    }
}

Wait-ElasticMq

Write-Host "Creating queue '$QueueName' if needed..."
Invoke-SqsAction -Uri $hostElasticMqUrl -Body "Action=CreateQueue&QueueName=$QueueName&Version=2012-11-05" | Out-Null

Write-Host "Building service image '$ServiceImageTag'..."
Push-Location $root
try {
    docker build -f EventsIngestion.Service\Dockerfile -t $ServiceImageTag .
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Local Docker setup is ready."
Write-Host "ElasticMQ host URL: $hostElasticMqUrl"
Write-Host "Service queue URL: $queueUrl"
Write-Host "Service image: $ServiceImageTag"

if ($RunService) {
    & (Join-Path $PSScriptRoot "run-service-local.ps1") `
        -NetworkName $NetworkName `
        -ElasticMqContainerName $ElasticMqContainerName `
        -ElasticMqPort $ElasticMqPort `
        -QueueName $QueueName `
        -ServiceImageTag $ServiceImageTag
}
else {
    Write-Host ""
    Write-Host "Run the service with:"
    Write-Host "powershell -ExecutionPolicy Bypass -File Tools\run-service-local.ps1"
}
