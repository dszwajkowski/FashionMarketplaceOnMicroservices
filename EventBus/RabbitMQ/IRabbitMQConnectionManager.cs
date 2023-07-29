using RabbitMQ.Client;

namespace EventBus.RabbitMQ;

public interface IRabbitMQConnectionManager : IDisposable
{
    bool IsConnectionOpen { get; }
    string Client { get; }
    bool CreateConnection();
    IModel? GetChannel();
}
