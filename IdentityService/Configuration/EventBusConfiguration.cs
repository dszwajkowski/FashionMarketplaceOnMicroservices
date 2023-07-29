using EventBus;
using EventBus.RabbitMQ;

namespace IdentityService.Configuration;

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

            return new EventBusRabbitMQ(connectionManager, eventBusLogger);
        });
    }
}
