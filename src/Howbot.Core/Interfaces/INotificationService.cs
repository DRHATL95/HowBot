using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Models;

namespace Howbot.Core.Interfaces;
public interface INotificationService
{
  ValueTask NotifyMusicStatusChangedAsync(ulong guildId, MusicStatus status);
  ValueTask NotifyMusicQueueChangedAsync(ulong guildId, MusicQueue queue);
  ValueTask NotifyMusicPlayerDisconnectedAsync(ulong guildId, string reason);
  ValueTask NotifyMusicPlayerConnectedAsync(ulong guildId, string channelName);
  ValueTask NotifyExceptionAsync(ulong guildId, Exception exception);

  // Event subscription for local consumers
  event Func<ulong, MusicStatus, ValueTask>? MusicStatusChanged;
  event Func<ulong, MusicQueue, ValueTask>? QueueUpdated;
  event Func<ulong, string, ValueTask>? PlayerConnected;
  event Func<ulong, Task>? PlayerDisconnected;
  event Func<ulong, Exception, ValueTask>? ExceptionOccured;
}
