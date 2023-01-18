using Howbot.Infrastructure.Messaging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Howbot.UnitTests.Infrastructure;

public class InMemoryQueueReceiverGetMessageFromQueue
{
  private readonly InMemoryQueueReceiver _receiver = new InMemoryQueueReceiver();
  [Fact]
  public async Task ThrowsNullExceptionGivenNullQueuename()
  {
    var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _receiver.GetMessageFromQueue(null));
  }

  [Fact]
  public async Task ThrowsArgumentExceptionGivenEmptyQueuename()
  {
    var ex = await Assert.ThrowsAsync<ArgumentException>(() => _receiver.GetMessageFromQueue(String.Empty));
  }
}
