using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Howbot.Core.Interfaces;

public interface IHowbotService
{
  Dictionary<ulong, string> SessionIds { get; set; }
  
  Task StartWorkerServiceAsync(CancellationToken cancellationToken);
  Task StopWorkerServiceAsync(CancellationToken cancellationToken);
  void Initialize();
}
