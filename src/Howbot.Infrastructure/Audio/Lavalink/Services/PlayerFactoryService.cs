using Discord.Interactions;
using Howbot.Application.Interfaces.Lavalink;
using Howbot.Application.Models.Lavalink.Players;
using Lavalink4NET;

namespace Howbot.Infrastructure.Audio.Lavalink.Services;

public class PlayerFactoryService(IAudioService audioService) : IPlayerFactoryService
{
  public ValueTask<HowbotPlayer> GetOrCreatePlayerAsync(ulong guildId, ulong textChannelId, ulong voiceChannelId, CancellationToken token = default)
  {
    try
    {
      HowbotPlayer? player = null;
      
      if (!audioService.Players.TryGetPlayer(guildId, out player))
      {
        player = new HowbotPlayer(new HowbotPlayerOptions(guildId, textChannelId));
      }
    }
    catch (Exception exception)
    {
      Console.WriteLine(exception);
      throw;
    }
    
    throw new NotImplementedException();
  }

  public ValueTask<HowbotPlayer?> GetOrCreatePlayerAsync(SocketInteractionContext context)
  {
    throw new NotImplementedException();
  }
}
