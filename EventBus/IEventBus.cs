namespace EventBus;

public interface IEventBus
{
    bool Publish(IntegrationEvent integrationEvent);
    bool Subscribe<T>()
        where T : IntegrationEvent;
    bool Unsubscribe<T>()
        where T : IntegrationEvent;
}
