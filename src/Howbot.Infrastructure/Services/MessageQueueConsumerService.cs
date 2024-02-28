using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Howbot.Core.Models.Commands;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageQueueConstants = Howbot.Core.Models.Constants.RabbitMq;

namespace Howbot.Infrastructure.Services;

// TODO: Consider name change, this doesn't just consume, it also publishes.
public class MessageQueueConsumerService(
  IConnectionFactory connectionFactory,
  IHowbotService howbotService,
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

  private async void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
  {
    if (sender is not EventingBasicConsumer consumer) return;
    
    var channel = consumer.Model;
    var props = e.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    replyProps.CorrelationId = props?.CorrelationId;
    
    var body = e.Body.ToArray();
    
    try
    {
      var response = await HandleMessageRequestAsync(body);
      var responseAsJson = JsonConvert.SerializeObject(response);
      
      // Send the response back to the client
      var responseBytes = Encoding.UTF8.GetBytes(responseAsJson);
      channel.BasicPublish(string.Empty, props?.ReplyTo, replyProps, responseBytes);
      
      // Process the message
      channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ConsumerOnReceived));
    }
  }

  private async Task<CommandResponse> HandleMessageRequestAsync(byte[] bodyContent, CancellationToken cancellationToken = default)
  {
    try
    {
      var message = Encoding.UTF8.GetString(bodyContent);
      
      if (string.IsNullOrWhiteSpace(message))
      {
        return new CommandResponse();
      }

      return await howbotService.HandleCommandAsync(message, cancellationToken);
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(HandleMessageRequestAsync));
      return CommandResponse.Create(false, exception.Message);
    }
  }
}
