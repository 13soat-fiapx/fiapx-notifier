using FiapX.Domain.Events;

namespace FiapX.Tests.Unit.Mocks;

public static class NotificationEventMocks
{
    public static VideoProcessingCompletedEvent BuildSucceededEvent(
        string jobId = "job-123",
        string userName = "João da Silva",
        string userEmail = "joao@example.com") => new()
    {
        ProcessingJobId = jobId,
        Status = "succeeded",
        User = new NotificationTarget { Id = "user-1", Name = userName, Email = userEmail },
        Messages = [],
        CompletedAt = DateTimeOffset.UtcNow
    };

    public static VideoProcessingCompletedEvent BuildFailedEvent(
        IReadOnlyList<ProcessingMessage>? messages = null,
        string userName = "João da Silva",
        string userEmail = "joao@example.com") => new()
    {
        ProcessingJobId = "job-456",
        Status = "failed",
        User = new NotificationTarget { Id = "user-1", Name = userName, Email = userEmail },
        Messages = messages ?? [new ProcessingMessage { Code = "ERR001", Message = "Codec não suportado", Severity = "error" }],
        CompletedAt = DateTimeOffset.UtcNow
    };

    public static VideoProcessingCompletedEvent BuildEventWithStatus(string status) => new()
    {
        ProcessingJobId = "job-789",
        Status = status,
        User = new NotificationTarget { Id = "user-1", Name = "João da Silva", Email = "joao@example.com" },
        Messages = [],
        CompletedAt = DateTimeOffset.UtcNow
    };
}
