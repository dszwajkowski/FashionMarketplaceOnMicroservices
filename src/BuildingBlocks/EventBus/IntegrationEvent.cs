namespace EventBus;
public record IntegrationEvent
{
    public IntegrationEvent() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreationDate { get; private set; } = DateTime.Now;
}
