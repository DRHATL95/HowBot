using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Howbot.Core.Interfaces;

namespace Howbot.Core.Modules;

public class GameModule(ILoggerAdapter<GameModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
  private ILoggerAdapter<GameModule> Logger { get; } = logger;
  
  [SlashCommand("roll", "Rolls a dice")]
  [RequireContext(ContextType.Guild)]
  public async Task RollCommandAsync()
  {
    await DeferAsync().ConfigureAwait(false);
    
    try
    {
      var responseMessage =
        await ModifyOriginalResponseAsync(properties => properties.Content = $"Rolling dice..")
          .ConfigureAwait(false);

      if (responseMessage is null) return;

      var random = new Random();
      var roll = random.Next(1, 7);
      
      await ModifyOriginalResponseAsync(properties => properties.Content = $"You rolled a {roll}!")
        .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error in RollCommandAsync");
    }
  }
}
