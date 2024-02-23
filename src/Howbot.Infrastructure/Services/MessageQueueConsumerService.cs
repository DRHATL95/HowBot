using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageQueueConstants = Howbot.Core.Models.Constants.RabbitMq;

namespace Howbot.Infrastructure.Services;

// TODO: Consider name change, this doesn't just consume, it also publishes.
public class MessageQueueConsumerService(
  IConnectionFactory connectionFactory,
  ILoggerAdapter<MessageQueueConsumerService> logger)
  : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    stoppingToken.ThrowIfCancellationRequested();
    
    using var connection = connectionFactory.CreateConnection();
    using var channel = connection.CreateModel();
    
    channel.QueueDeclare(queue: MessageQueueConstants.RpcQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    
    var consumer = new EventingBasicConsumer(channel);

    channel.BasicConsume(MessageQueueConstants.RpcQueue, autoAck: false, consumer);
    
    consumer.Received += ConsumerOnReceived;
    
    await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
  }

  private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
  {
    if (sender is not EventingBasicConsumer consumer) return;
    
    var channel = consumer.Model;
    var props = e.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    replyProps.CorrelationId = props?.CorrelationId;
    
    var body = e.Body.ToArray();

    try
    {
      var message = Encoding.UTF8.GetString(body);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ConsumerOnReceived));
    }
    finally
    {
      var response = Encoding.UTF8.GetBytes("OK, I got it. This is my response.");
      channel.BasicPublish(string.Empty, props?.ReplyTo, replyProps, response);
      // Process the message
      channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
    }
    
    /*_logger.LogDebug("Received message: {message}", message);

    await Task.Run(() =>
    {
      var commandPayload = JsonConvert.DeserializeObject<CommandPayload>(message);
      
      _howbotService.ProcessCommand(commandPayload);
    });*/
  }
}
