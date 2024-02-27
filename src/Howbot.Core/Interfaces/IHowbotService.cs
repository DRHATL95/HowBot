using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Howbot.Core.Models.Commands;

namespace Howbot.Core.Interfaces;

public interface IHowbotService
{
  ConcurrentDictionary<ulong, string> SessionIds { get; }
  
  void Initialize();
  Task StartWorkerServiceAsync(CancellationToken cancellationToken);
  Task StopWorkerServiceAsync(CancellationToken cancellationToken);
  Task<CommandResponse> HandleCommandAsync(string commandAsJson);
}
