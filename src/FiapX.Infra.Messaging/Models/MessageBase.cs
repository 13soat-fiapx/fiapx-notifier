namespace FiapX.Infra.Messaging.Models;

public class MessageBase<T> where T : class
{
    public required T Payload { get; init; }
    public required EventHeaders Headers { get; init; }
}

public sealed class EventHeaders
{
    public required string EventId { get; init; }
    public required string EventType { get; init; }
    public required string Traceparent { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required string Source { get; init; }
    public string? Tracestate { get; init; }
    public string? Baggage { get; init; }
}
