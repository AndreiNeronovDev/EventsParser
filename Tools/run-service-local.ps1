param(
    [string]$NetworkName = "events-local",
    [string]$ElasticMqContainerName = "elasticmq",
    [int]$ElasticMqPort = 9324,
    [string]$QueueName = "events-ingestion-local",
    [string]$ServiceImageTag = "events-ingestion-service:local",
    [string]$SourceCode = "muziekladder",
    [string]$Region = "eu-west-1"
)

$ErrorActionPreference = "Stop"

$serviceUrl = "http://${ElasticMqContainerName}:$ElasticMqPort"
$queueUrl = "$serviceUrl/000000000000/$QueueName"
$hostElasticMqUrl = "http://localhost:$ElasticMqPort"

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

Write-Host "Checking service image '$ServiceImageTag'..."
docker image inspect $ServiceImageTag | Out-Null

Write-Host "Ensuring Docker network '$NetworkName'..."
$networkExists = docker network ls --format "{{.Name}}" | Where-Object { $_ -eq $NetworkName }
if (-not $networkExists) {
    docker network create $NetworkName | Out-Null
}

Write-Host "Ensuring ElasticMQ container '$ElasticMqContainerName'..."
$containerExists = docker ps -a --format "{{.Names}}" | Where-Object { $_ -eq $ElasticMqContainerName }
if (-not $containerExists) {
    docker run -d --name $ElasticMqContainerName --network $NetworkName -p "${ElasticMqPort}:9324" softwaremill/elasticmq-native | Out-Null
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

Write-Host "Running service image '$ServiceImageTag'..."

docker run --rm `
    --network $NetworkName `
    -e SOURCE_CODE=$SourceCode `
    -e Sqs__Region=$Region `
    -e Sqs__ServiceUrl=$serviceUrl `
    -e Sqs__QueueUrl=$queueUrl `
    -e AWS_ACCESS_KEY_ID=test `
    -e AWS_SECRET_ACCESS_KEY=test `
    $ServiceImageTag
