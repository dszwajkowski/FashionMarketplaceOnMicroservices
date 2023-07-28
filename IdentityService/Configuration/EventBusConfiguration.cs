using EventBus;

namespace IdentityService.Configuration;

internal static class EventBusConfiguration
{
    internal static void ConfigureRabbitMQ(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var host = configuration["RabbitMQ:Host"];
        var port = configuration["RabbitMQ:Port"];
        var queueName = configuration["RabbitMQ:QueueName"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(queueName)) 
        {
            throw new ArgumentException("RabbitMQ host, port or queue name is empty.");
        }

        services.AddSingleton<IEventBus, EventBus.EventBusRabbitMQ>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<EventBus.EventBusRabbitMQ>>();

            return new EventBusRabbitMQ(host, int.Parse(port), queueName, logger);
        });
    }
}
