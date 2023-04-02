using System;
using System.Threading.Tasks;
using Howbot.Core.Models;
using Victoria.Player;

namespace Howbot.Core.Interfaces;

public interface ILavaNodeService : IServiceBase
{
  Task InitiateDisconnectLogicAsync(Player<LavaTrack> lavaPlayer, TimeSpan timeSpan);
}
