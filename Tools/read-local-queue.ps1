param(
    [int]$MaxMessages = 10,
    [string]$QueueName = "events-ingestion-local",
    [int]$ElasticMqPort = 9324,
    [switch]$Raw
)

$ErrorActionPreference = "Stop"

$queueUrl = "http://localhost:$ElasticMqPort/000000000000/$QueueName"
$body = "Action=ReceiveMessage&MaxNumberOfMessages=$MaxMessages&AttributeName.1=All&MessageAttributeName.1=All&Version=2012-11-05"

$response = Invoke-WebRequest -UseBasicParsing -Method Post -Uri $queueUrl -ContentType "application/x-www-form-urlencoded" -Body $body

if ($Raw) {
    $response.Content
    return
}

[xml]$xml = $response.Content
$messages = $xml.ReceiveMessageResponse.ReceiveMessageResult.Message

if (-not $messages) {
    Write-Host "No visible messages in '$QueueName'."
    Write-Host "If you read them recently, wait for the visibility timeout and try again."
    return
}

foreach ($message in $messages) {
    $decodedBody = [System.Net.WebUtility]::HtmlDecode($message.Body)

    Write-Host "MessageId: $($message.MessageId)"
    try {
        $json = $decodedBody | ConvertFrom-Json
        $json | ConvertTo-Json -Depth 20
    }
    catch {
        $decodedBody
    }
    Write-Host ""
}
