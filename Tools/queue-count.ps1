param(
    [string]$QueueName = "events-ingestion-local",
    [int]$ElasticMqPort = 9324
)

$ErrorActionPreference = "Stop"

$queueUrl = "http://localhost:$ElasticMqPort/000000000000/$QueueName"
$body = "Action=GetQueueAttributes&AttributeName.1=ApproximateNumberOfMessages&AttributeName.2=ApproximateNumberOfMessagesNotVisible&Version=2012-11-05"

$response = Invoke-WebRequest -UseBasicParsing -Method Post -Uri $queueUrl -ContentType "application/x-www-form-urlencoded" -Body $body
[xml]$xml = $response.Content

$xml.GetQueueAttributesResponse.GetQueueAttributesResult.Attribute | ForEach-Object {
    [pscustomobject]@{
        Name = $_.Name
        Value = $_.Value
    }
}
