using EventBus;
using EventBus.RabbitMQ;
using OfferService.Users;

namespace OfferService.Configuration;

internal static class EventBusConfiguration
{
    internal static void ConfigureRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var host = configuration["RabbitMQ:Host"];
        var port = configuration["RabbitMQ:Port"];
        var client = configuration["RabbitMQ:Client"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(client))
        {
            throw new ArgumentException("RabbitMQ host, port or queue name is empty.");
        }

        int portAsInt = 0;
        if (!int.TryParse(port, out portAsInt))
        {
            throw new ArgumentException("Provided RabbitMQ port is not valid.");
        }

        services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        {
            var eventBusLogger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
            var connectionManagerLogger = sp.GetRequiredService<ILogger<RabbitMQConnectionManager>>();
            var connectionManager = new RabbitMQConnectionManager(host, portAsInt, client, connectionManagerLogger);

            return new EventBusRabbitMQ(connectionManager, eventBusLogger, client);
        });
    }

    internal static void RegisterEventHandlers(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope()) 
        {
            var eventBus = scope.ServiceProvider.GetService<IEventBus>();

            if (eventBus != null)
            {
                eventBus.Subscribe<CreateUser.CreatedUserEvent, CreateUser.EventHandler>();
            }
            else
            {
                throw new InvalidOperationException("Can't register event handler, there is no EventBus service.");
            }
        }
    }
}
