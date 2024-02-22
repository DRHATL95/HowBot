using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Howbot.Infrastructure.Services;

public class MessageQueueConsumerService(IConnectionFactory connectionFactory, ILoggerAdapter<MessageQueueConsumerService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    stoppingToken.ThrowIfCancellationRequested();

    using var connection = connectionFactory.CreateConnection();
    using var channel = connection.CreateModel();
    
    var consumer = new EventingBasicConsumer(channel);
    
    consumer.Received += ConsumerOnReceived;
    
    channel.BasicConsume("CommandQueue", autoAck: false, consumer);
    
    await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
  }

  private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
  {
    var consumer = sender as EventingBasicConsumer;
    var channel = consumer?.Model;
    
    var body = e.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    logger.LogDebug("Received message: {message}", message);
    
    // Process the message
    channel?.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
  }
}
