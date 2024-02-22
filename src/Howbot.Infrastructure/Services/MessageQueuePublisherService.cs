using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Howbot.Core.Models;
using RabbitMQ.Client;

namespace Howbot.Infrastructure.Services;

public class MessageQueuePublisherService
{
  private readonly IModel _channel;

  public MessageQueuePublisherService(IConnectionFactory connectionFactory)
  {
    var connection = connectionFactory.CreateConnection();
    
    _channel = connection.CreateModel();
    _channel.ConfirmSelect();
  }

  public async Task PublishAsync(string routingKey, CommandPayload commandPayload)
  {
    var json = JsonSerializer.Serialize(commandPayload);
    var body = Encoding.UTF8.GetBytes(json);
    
    await Task.Run(() => _channel.BasicPublish(
      exchange: string.Empty,
      routingKey: routingKey,
      basicProperties: null,
      body: body));
    
    // TODO: Revisit this, maybe consider a constant too
    _channel.WaitForConfirmsOrDie(TimeSpan.FromMinutes(5));
  }
}
