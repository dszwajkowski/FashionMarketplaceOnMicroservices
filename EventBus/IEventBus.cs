namespace EventBus;

public interface IEventBus
{
    bool Publish(IntegrationEvent integrationEvent);
    bool Subscribe<T, TH>()
        where T : IntegrationEvent 
        where TH : IIntegrationEventHandler;
    bool Unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler;
}
