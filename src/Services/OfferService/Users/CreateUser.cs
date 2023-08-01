using EventBus;
using Mapster;
using Microsoft.EntityFrameworkCore.Design;
using OfferService.Data;
using OfferService.Models;

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

    public class CreatedUserEventHandler : IIntegrationEventHandler
    {
        private readonly ApplicationDbContext _dbContext;

        public CreatedUserEventHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(IntegrationEvent integrationEvent)
        {
            var user = integrationEvent.Adapt<User>();
            await _dbContext.Users.AddAsync(user);
            if (_dbContext.SaveChanges() == 0)
            {
                // todo custom exception for handler fails
                throw new OperationException("Couldn't save user.");
            }
        }
    }
}
