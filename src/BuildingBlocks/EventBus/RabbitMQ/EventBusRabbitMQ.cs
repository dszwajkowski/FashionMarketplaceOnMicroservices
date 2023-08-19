using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ;

public class EventBusRabbitMQ : IEventBus, IDisposable
{
    private const string BrokerName = "fashionmarketplace_eventbus";

    private readonly IRabbitMQConnectionManager _connectionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBusRabbitMQ> _logger;
    private readonly IDictionary<string, IList<Type>> _eventHandlers;
    private readonly IList<Type> _eventTypes;
    private readonly string? _queueName;

    private RetryPolicy _publishRetryPolicy;

    public EventBusRabbitMQ(
        IRabbitMQConnectionManager connectionManager,
        IServiceProvider serviceProvider,
        ILogger<EventBusRabbitMQ> logger,
        string? queueName = null)
    {
        _connectionManager = connectionManager;
        _connectionManager.BrokerName = BrokerName;
        _queueName = queueName ?? "";
        _serviceProvider = serviceProvider;
        _logger = logger;

        _eventHandlers = new Dictionary<string, IList<Type>>();
        _eventTypes = new List<Type>();

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

        var channel = _connectionManager.GetOrCreateChannel();
        if (channel is not null)
        {
            // uncomment if you want to publish events to publisher's queue as well
            //DeclareAndBindQueue(channel, eventName);

            var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, integrationEvent.GetType());

            _logger.LogInformation("Publishing {EventType}:{EvenId} to RabbitMQ.", eventName, integrationEvent.Id);

            _publishRetryPolicy.Execute(() => channel.BasicPublish(BrokerName, eventName, null, body));

            return true;
        }
        else
        {
            _logger.LogError("Couldn't get channel, event {EventType}:{EventId} not published.", eventName, integrationEvent.Id);
            return false;
        };
    }

    public bool Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler
    {
        var eventType = typeof(T);
        var handlerType = typeof(TH);

        _logger.LogInformation("Subcribing to event {EventType} with {Handler}", eventType.Name, handlerType.Name);

        var channel = _connectionManager.GetOrCreateChannel();

        if (channel is null) 
        {
            _logger.LogError("Can't subscribe to event {EventType} with {Handler}. Couldn't get channel.", 
                eventType.Name, handlerType.Name);
            return false;
        }

        DeclareAndBindQueue(channel, eventType.Name);

        if (!_eventHandlers.ContainsKey(eventType!.Name))
        {
            _eventHandlers.Add(eventType.Name, new List<Type>());
        }

        if (_eventHandlers[eventType.Name].Contains(eventType))
        {
            _logger.LogTrace("{Handler} already registered for {EventType}. Exiting with success.", handlerType.Name, eventType.Name);
            return true;
        }

        _eventHandlers[eventType.Name].Add(handlerType);
        if (!_eventTypes.Contains(eventType))
        {
            _eventTypes.Add(eventType);
        }

        _logger.LogTrace("Added {Handler} for {EventType}.", handlerType.Name, eventType.Name);

        StartEventConsume(channel);

        return true;
    }

    public bool Unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler
    {
        var eventType = typeof(T);
        var handlerType = typeof(TH);

        _logger.LogInformation("Unsubcribing to event {EventType} with {Handler}", eventType.Name, handlerType.Name);
        
        if (!_eventHandlers.ContainsKey(eventType.Name))
        {
            _logger.LogTrace("No handlers registered for {EventType}. Exiting with success.",
                eventType.Name);
            return true;
        }

        _eventHandlers[eventType.Name].Remove(handlerType);

        if (_eventHandlers[eventType.Name].Count == 0)
        {
            var channel = _connectionManager.GetOrCreateChannel();

            if (channel is null)
            {
                _logger.LogError("Couldn't unsubscribe to {EventType}, couldn't get channel.", eventType.Name);
                return false;
            }
            
            UnbindQueue(channel, eventType.Name);

            _eventHandlers.Remove(eventType.Name);
            _eventTypes.Remove(eventType);

            _logger.LogTrace("There are no handlers left for {EventType}.", eventType.Name);
        }

        return true;
    }

    public void Dispose()
    {
        _connectionManager.Dispose();
    }

    private void DeclareAndBindQueue(IModel channel, string eventName)
    {
        channel.QueueDeclare(_queueName, true, false, false, null);
        channel.QueueBind(_queueName, BrokerName, eventName);
    }

    private void UnbindQueue(IModel channel, string eventName) 
    {
        channel.QueueUnbind(_queueName, BrokerName, eventName);
    }

    private void StartEventConsume(IModel channel)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += ProcessEvent;

        channel.BasicConsume(_queueName, false, consumer);
    }

    private async Task ProcessEvent(object sender, BasicDeliverEventArgs @event)
    {
        _logger.LogTrace("Received event {EventType} from RabbitMQ.", @event.RoutingKey);
        
        if (!_eventHandlers.ContainsKey(@event.RoutingKey))
        {
            _logger.LogWarning("Not subscibed to {EventType}.", @event.RoutingKey);
            return;
        }

        var handlers = _eventHandlers[@event.RoutingKey];
        if (handlers.Count == 0)
        {
            _logger.LogWarning("No handlers registered for {EventType}.", @event.RoutingKey);
            return;
        }

        var message = Encoding.UTF8.GetString(@event.Body.Span);
        var eventType = _eventTypes
            .Where(x => x.Name.Equals(@event.RoutingKey, StringComparison.InvariantCultureIgnoreCase))
            .First();

        var integrationEvent = JsonSerializer.Deserialize(message, eventType);
        if (integrationEvent is null)
        {
            _logger.LogWarning("Couldn't deserialize {EventType}.", @event.RoutingKey);
            return;
        }

        bool ack = true;

        foreach (var handler in handlers)
        {
            _logger.LogTrace("Consuming event {EventType} with {Handler}.", 
                @event.RoutingKey, handler.Name);
            
            using (var scope = _serviceProvider.CreateScope()) 
            try
            {
                var handlerObject = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handler);
                await (Task)handler
                    .GetMethod(nameof(IIntegrationEventHandler.Handle))!
                    .Invoke(handlerObject, new object[] { integrationEvent })!;

            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Handler exception for {EventType}.", @event.RoutingKey);
                ack = false;
            }  
        }

        if (ack) 
        {
            var channel = _connectionManager.GetOrCreateChannel();
            channel!.BasicAck(@event.DeliveryTag, false);
        }
    }
}
