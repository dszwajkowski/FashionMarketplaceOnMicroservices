using RabbitMQ.Client;

namespace EventBus.RabbitMQ;

public interface IRabbitMQConnectionManager : IDisposable
{
    bool IsConnectionOpen { get; }
    string Client { get; }
    string BrokerName { get; set; }
    bool CreateConnection();
    IModel? GetChannel();
}
