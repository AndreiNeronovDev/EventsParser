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

docker run --rm `
    --network $NetworkName `
    -e SOURCE_CODE=$SourceCode `
    -e Sqs__Region=$Region `
    -e Sqs__ServiceUrl=$serviceUrl `
    -e Sqs__QueueUrl=$queueUrl `
    -e AWS_ACCESS_KEY_ID=test `
    -e AWS_SECRET_ACCESS_KEY=test `
    $ServiceImageTag
