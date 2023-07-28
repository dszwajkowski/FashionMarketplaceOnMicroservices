using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text.Json;

namespace EventBus;

public class EventBusRabbitMQ : IEventBus, IDisposable
{
    private const string BrokerName = "fashionmarketplace_eventbus";

    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<EventBusRabbitMQ> _logger;
    private readonly string _queueName;

    private IConnection _connection;
    private IModel _channel;

    public EventBusRabbitMQ(string host, int port, string queueName, ILogger<EventBusRabbitMQ> logger)
    {
        _logger = logger;

        _connectionFactory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
        };

        _queueName = queueName;

        _connection = CreateConnection() ?? throw new ArgumentException("Could not create RabbitMQ connection for provided host.");
        _channel = CreateChannelAndQueue(_connection, _queueName);
    }

    public void Publish(IntegrationEvent integrationEvent)
    {
        var eventName = integrationEvent.GetType().Name;

        _logger.LogInformation("Received {EventType}:{EvenId}, trying to publish to RabbitMQ.", eventName, integrationEvent.Id);

        var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, integrationEvent.GetType());

        // todo retry policy
        _logger.LogInformation("Publishing {EventType}:{EvenId} to RabbitMQ.", eventName, integrationEvent.Id);
        _channel.BasicPublish(BrokerName, "", body: body);
    }

    public void Subscribe<T>() 
        where T : IntegrationEvent
    {
        throw new NotImplementedException();
    }

    public void Unsubscribe<T>() 
        where T : IntegrationEvent
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        if (_channel.IsOpen)
        {
            _channel.Close();
        }

        if (_connection.IsOpen)
        {
            _connection.Close();
        }
    }

    private IConnection? CreateConnection()
    {
        _logger.LogInformation("Trying to create RabbitMQ connection {Connection}.", _connectionFactory.Uri);

        IConnection? connection = null;

        // todo retry policy
        try
        {
            connection = _connectionFactory.CreateConnection();
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogError(ex, "Can't create RabbitMQ connection {@Connection}.", _connectionFactory);
            throw;
        }

        if (connection.IsOpen)
        {
            connection.ConnectionShutdown += ConnectionShutdownEventHandler;
            connection.CallbackException += CallbackExceptionEventHandler;
            connection.ConnectionBlocked += ConnectionBlockedEventHandler;

            return connection;
        }
        else
        {
            _logger.LogError("Couldn't create RabbitMQ connection {Connection}", _connectionFactory);
            return null;
        }
    }

    private IModel CreateChannelAndQueue(IConnection connection, string queueName)
    {
        var channel = connection.CreateModel();
        channel.ExchangeDeclare(BrokerName, ExchangeType.Fanout);
        channel.QueueDeclare(queueName);
        channel.QueueBind(queueName, BrokerName, "");

        return channel;
    }

    private void ConnectionShutdownEventHandler(object? sender, ShutdownEventArgs e)
    {
        if (e.Initiator is ShutdownInitiator.Peer or ShutdownInitiator.Library)
        {
            _logger.LogError("RabbitMQ {@Connection} was closed by {Initiator}. Trying to reconnect.", _connection.Endpoint, e.Initiator);

            CreateConnection();
        }
        else
        {
            _logger.LogInformation("RabbitMQ {@Connection} was closed by {Initiator}.", _connection.Endpoint, e.Initiator);
        }
    }

    private void CallbackExceptionEventHandler(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "RabbitMQ callback exception for {@Connection}. Trying to reconnect.", _connection.Endpoint);

        CreateConnection();
    }

    private void ConnectionBlockedEventHandler(object? sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogError("RabbitMQ {@Connection} blocked {Reason}. Trying to reconnect.", _connection.Endpoint, e.Reason);

        CreateConnection();
    }
}
