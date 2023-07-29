namespace EventBus;

public interface IIntegrationEventHandler
{
    Task Handle(IntegrationEvent integrationEvent);
}
