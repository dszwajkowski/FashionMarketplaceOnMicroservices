using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ;

public class RabbitMQConnectionManager : IRabbitMQConnectionManager
{
    private readonly ILogger<RabbitMQConnectionManager> _logger;
    private readonly IConnectionFactory _connectionFactory;
    private readonly RetryPolicy<IConnection> _retryPolicy;

    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQConnectionManager(
        string host,
        int port,
        string clientName,
        ILogger<RabbitMQConnectionManager> logger)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            ClientProvidedName = clientName
        };

        _logger = logger;

        _retryPolicy = RetryPolicy<IConnection>
            .Handle<BrokerUnreachableException>()
            .WaitAndRetry(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 2), (r, t) =>
            {
                _logger.LogError(r.Exception, "Could not connect to RabbitMQ after {Seconds}s.", t.TotalSeconds);
            });
    }

    public bool IsConnectionOpen => _connection != null && _connection.IsOpen;
    public bool IsChannelOpen => _channel != null && _channel.IsOpen;
    public string Client => _connectionFactory.ClientProvidedName;

    public bool CreateConnection()
    {
        if (IsConnectionOpen)
        {
            return true;
        }

        _logger.LogInformation("Trying to create RabbitMQ connection.");

        _retryPolicy.Execute(() => _connection = _connectionFactory.CreateConnection());

        if (IsConnectionOpen)
        {
            _connection!.ConnectionShutdown += ConnectionShutdownEventHandler;
            _connection.CallbackException += CallbackExceptionEventHandler;
            _connection.ConnectionBlocked += ConnectionBlockedEventHandler;
            return true;
        }
        else
        {
            _logger.LogError("Couldn't create RabbitMQ connection.");
            return false;
        }
    }

    public IModel? GetChannel()
    {
        if (IsChannelOpen)
        {
            return _channel;
        }

        if (!IsConnectionOpen && !CreateConnection())
        {
            _logger.LogTrace("Can't create channel because connection is not open and establishing new connection failed.");
            return null;
        }

        _channel = _connection!.CreateModel();

        return _channel;
    }


    public void Dispose()
    {
        if (IsChannelOpen)
        {
            _channel!.Close();
            _channel.Dispose();
        }

        if (IsConnectionOpen)
        {
            _connection!.ConnectionShutdown -= ConnectionShutdownEventHandler;
            _connection.CallbackException -= CallbackExceptionEventHandler;
            _connection.ConnectionBlocked -= ConnectionBlockedEventHandler;
            _connection.Close();
            _connection.Dispose();
        }
    }

    private void ConnectionShutdownEventHandler(object? sender, ShutdownEventArgs e)
    {
        if (e.Initiator is ShutdownInitiator.Peer or ShutdownInitiator.Library)
        {
            _logger.LogError("RabbitMQ was closed by {Initiator}. Trying to reconnect.", e.Initiator);

            CreateConnection();
        }
        else
        {
            _logger.LogInformation("RabbitMQ was closed by {Initiator}.", e.Initiator);
        }
    }

    private void CallbackExceptionEventHandler(object? sender, CallbackExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "RabbitMQ callback exception. Trying to reconnect.");

        CreateConnection();
    }

    private void ConnectionBlockedEventHandler(object? sender, ConnectionBlockedEventArgs e)
    {
        _logger.LogError("RabbitMQ blocked because {Reason}. Trying to reconnect.", e.Reason);

        CreateConnection();
    }
}
