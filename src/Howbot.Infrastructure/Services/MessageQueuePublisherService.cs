using System.Collections.Concurrent;
using System.Text;
using Howbot.Core.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageQueueConstants = Howbot.Core.Models.Constants.RabbitMq;

namespace Howbot.Infrastructure.Services;

// TODO: Consider name change, this doesn't just publish, it also consumes.
public class MessageQueuePublisherService : IDisposable
{
  private readonly IModel _channel;
  private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _completionSources = new();
  private readonly IConnection _connection;
  private readonly ILoggerAdapter<MessageQueuePublisherService> _logger;
  private readonly string _replyQueueName;

  public MessageQueuePublisherService(IConnectionFactory connectionFactory,
    ILoggerAdapter<MessageQueuePublisherService> logger)
  {
    _logger = logger;

    _connection = connectionFactory.CreateConnection();
    _channel = _connection.CreateModel();

    _logger.LogDebug("Message queue is {0}", IsMessageQueueOnline() ? "Online" : "Offline");

    // declare a server-named queue
    _replyQueueName = _channel.QueueDeclare().QueueName;

    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += (model, ea) =>
    {
      if (!_completionSources.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
      {
        return;
      }

      var body = ea.Body.ToArray();
      var response = Encoding.UTF8.GetString(body);
      tcs.TrySetResult(response);
    };

    _channel.BasicConsume(consumer: consumer, queue: _replyQueueName, autoAck: true);
  }

  public void Dispose()
  {
    _channel.Close();
    _connection.Close();

    GC.SuppressFinalize(this);
  }

  public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
  {
    var props = _channel.CreateBasicProperties();
    var correlationId = Guid.NewGuid().ToString();
    props.CorrelationId = correlationId;
    props.ReplyTo = _replyQueueName;

    var messageBytes = Encoding.UTF8.GetBytes(message);
    var tcs = new TaskCompletionSource<string>();
    _completionSources.TryAdd(correlationId, tcs);

    _channel.BasicPublish(string.Empty, MessageQueueConstants.RpcQueue, props, messageBytes);

    cancellationToken.Register(() => _completionSources.TryRemove(correlationId, out _));

    return tcs.Task;
  }

  public bool IsMessageQueueOnline()
  {
    return _connection.IsOpen && _channel.IsOpen;
  }
}
