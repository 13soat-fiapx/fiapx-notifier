namespace FiapX.Domain.Events;

public class VideoProcessingCompletedEvent
{
    public required string ProcessingJobId { get; init; }
    public required NotificationTarget User { get; init; }
    public required string Status { get; init; }
    public required IReadOnlyList<ProcessingMessage> Messages { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
    public ProcessingResult? Result { get; init; }
    public DateTimeOffset? EstimatedCompletionTime { get; init; }
}

public class NotificationTarget
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
}

public class ProcessingMessage
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required string Severity { get; init; }
}

public class ProcessingResult
{
    public required S3ObjectRef ZipFile { get; init; }
    public long? SizeBytes { get; init; }
    public string? Checksum { get; init; }
}

public class S3ObjectRef
{
    public required string Bucket { get; init; }
    public required string Key { get; init; }
    public required string Region { get; init; }
}
