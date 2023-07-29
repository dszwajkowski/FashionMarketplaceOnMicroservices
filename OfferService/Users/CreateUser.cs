using EventBus;

namespace OfferService.Users;

public static class CreateUser
{
    public record CreatedUserEvent : IntegrationEvent
    {
        public string Email { get; init; } = null!;
        public string Username { get; init; } = null!;
        public string FirstName { get; init; } = null!;
        public string SecondName { get; init; } = null!;
        public string? PhoneNumber { get; init; }
    }

    public class EventHandler : IIntegrationEventHandler
    {
        public Task Handle(IntegrationEvent integrationEvent)
        {
            throw new NotImplementedException();
        }
    }
}
