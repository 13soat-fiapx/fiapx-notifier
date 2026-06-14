namespace FiapX.Tests.Integration.Mocks;

public static class VideoProcessingMessageMocks
{
    public static string SucceededEvent => """
        {
          "Payload": {
            "ProcessingJobId": "job-test-123",
            "Status": "succeeded",
            "User": {
              "Id": "user-1",
              "Name": "João da Silva",
              "Email": "joao@example.com"
            },
            "Messages": [],
            "CompletedAt": "2024-01-01T00:00:00+00:00"
          },
          "Headers": {
            "EventId": "evt-001",
            "EventType": "VideoProcessingCompletedEvent",
            "Traceparent": "00-trace-001-00",
            "OccurredAt": "2024-01-01T00:00:00+00:00",
            "Source": "video-processor"
          }
        }
        """;

    public static string FailedEvent => """
        {
          "Payload": {
            "ProcessingJobId": "job-test-456",
            "Status": "failed",
            "User": {
              "Id": "user-1",
              "Name": "João da Silva",
              "Email": "joao@example.com"
            },
            "Messages": [
              {
                "Code": "ERR001",
                "Message": "Codec não suportado",
                "Severity": "error"
              }
            ],
            "CompletedAt": "2024-01-01T00:00:00+00:00"
          },
          "Headers": {
            "EventId": "evt-002",
            "EventType": "VideoProcessingCompletedEvent",
            "Traceparent": "00-trace-002-00",
            "OccurredAt": "2024-01-01T00:00:00+00:00",
            "Source": "video-processor"
          }
        }
        """;
}
