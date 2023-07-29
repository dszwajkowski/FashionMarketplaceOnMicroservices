using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text.Json;

namespace EventBus.RabbitMQ;

public class EventBusRabbitMQ : IEventBus, IDisposable
{
    private const string BrokerName = "fashionmarketplace_eventbus";

    private readonly IRabbitMQConnectionManager _connectionManager;
    private readonly ILogger<EventBusRabbitMQ> _logger;
    private readonly string? _queueName;

    private RetryPolicy _publishRetryPolicy;

    public EventBusRabbitMQ(
        IRabbitMQConnectionManager connectionManager,
        ILogger<EventBusRabbitMQ> logger,
        string? queueName = null)
    {
        _connectionManager = connectionManager;
        _queueName = queueName;
        _logger = logger;

        _publishRetryPolicy = Policy
            .Handle<BrokerUnreachableException>()
            .WaitAndRetry(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 2), (e, t) =>
            {
                _logger.LogError(e, "Could not publish event to RabbitMQ after {Seconds}s.", t.TotalSeconds);
            });
    }

    public bool Publish(IntegrationEvent integrationEvent)
    {
        var eventName = integrationEvent.GetType().Name;

        _logger.LogTrace("Received {EventType}:{EvenId}, trying to publish to RabbitMQ.", eventName, integrationEvent.Id);

        var channel = _connectionManager.GetChannel();
        if (channel is not null)
        {
            DeclareExchange(channel);

            var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, integrationEvent.GetType());

            _logger.LogInformation("Publishing {EventType}:{EvenId} to RabbitMQ.", eventName, integrationEvent.Id);

            _publishRetryPolicy.Execute(() => channel.BasicPublish(BrokerName, eventName, body: body));

            return true;
        }
        else
        {
            _logger.LogError("Couldn't get channel, event {EventType}:{EventId} not published.", eventName, integrationEvent.Id);
            return false;
        };
    }

    public bool Subscribe<T>()
        where T : IntegrationEvent
    {

        throw new NotImplementedException();
    }

    public bool Unsubscribe<T>()
        where T : IntegrationEvent
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _connectionManager.Dispose();
    }

    private void DeclareExchange(IModel channel)
    {
        _logger.LogTrace("Declaring RabbitMQ exchange.");
        channel.ExchangeDeclare(BrokerName, ExchangeType.Direct);
    }

    private void DeclareAndBindQueue(IModel channel, string routingKey)
    {
        channel.QueueDeclare(_queueName);
        channel.QueueBind(_queueName, BrokerName, "");
    }

    private void StartEventConsume()
    {
        var channel = _connectionManager.GetChannel();

        if (channel is not null)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (sender, e) =>
            {
                // todo implementation
                await Task.FromResult(1);
                channel.BasicAck(e.DeliveryTag, false);
            };
        }
        else
        {
            _logger.LogError("Can't start consuming events, could not get channel.");
        }
    }
}
