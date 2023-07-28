namespace EventBus;

public interface IEventBus
{
    void Publish(IntegrationEvent integrationEvent);
    void Subscribe<T>()
        where T : IntegrationEvent;
    void Unsubscribe<T>()
        where T : IntegrationEvent;
}
