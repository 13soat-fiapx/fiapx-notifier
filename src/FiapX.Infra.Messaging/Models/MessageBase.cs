namespace FiapX.Infra.Messaging.Models;

public abstract class MessageBase<T>(T payload, int eventVersion = 1) where T : class, new()
{
    public T Payload { get; set; } = payload;
    public int EventVersion { get; } = eventVersion;

    protected MessageBase() : this(new T())
    {
    }
}
