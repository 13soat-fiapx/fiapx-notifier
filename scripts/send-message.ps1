param (
  [ValidateSet('succeeded', 'failed')]
  [string]$Status = 'succeeded',
  [string]$ProcessingJobId = '8f160f70-b26b-4eb2-8ff9-c69b4f8d2e0e'
)

$UserId = 'auth0|abc123'
$UserName = 'Fulano de Tal'
$UserEmail = 'fulano@example.com'
$QueueName = 'fiapx-dev-video-processing-completed'
$Container = 'fiapx-notifier-localstack-1'

# Build SQS message body following the VideoProcessingCompleted contract
$occurredAt = (Get-Date).ToUniversalTime().ToString('s')
$eventId = [System.Guid]::NewGuid().ToString()
$traceparent = '00-4bf92f3577b34da6a3ce929d0e0e4736-519aa4f8279bde01-01'

$messageHeaders = @{
  eventId = $eventId
  eventType = 'VideoProcessingCompleted'
  eventVersion = '1.0'
  traceparent = $traceparent
  occurredAt = $occurredAt
  source = 'fiapx-processor'
}

$messagePayload = @{
  processingJobId = $ProcessingJobId
  user = @{
    id = $UserId
    name = $UserName
    email = $UserEmail
  }
  status = $Status
  messages = @(
    @{
      code = 'PROC-1000'
      message = 'Processamento concluido com sucesso.'
      severity = 'info'
    }
  )
  completedAt = $occurredAt
}

if ($Status -eq 'succeeded')
{
  $messagePayload.result = @{
    zipFile = @{
      bucket = 'fiapx-media'
      key = "frames/$ProcessingJobId/frames.zip"
      region = 'us-east-1'
    }
    sizeBytes = 43888312
    checksum = '0f9c2f5e5085f9c1d24a5149f8089a7ab8be4a29173c9e5a87525737f27a4f87'
  }
}

$body = @{
  headers = $messageHeaders
  payload = $messagePayload
} | ConvertTo-Json -Depth 10 -Compress

Write-Host -ForegroundColor Yellow "Publishing VideoProcessingCompleted ($Status)..."

docker exec $Container `
    awslocal sqs send-message `
        --queue-url "http://localhost:4566/000000000000/$QueueName" `
        --message-body $body

if ($LASTEXITCODE -ne 0)
{
  throw "Failed to send message to '$QueueName'"
}

Write-Host -ForegroundColor Green "Message published successfully (eventId: $eventId)."
