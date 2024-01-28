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
    await DeferAsync();
    
    try
    {
      var responseMessage =
        await ModifyOriginalResponseAsync(properties => properties.Content = "Rolling dice..");

      if (responseMessage is null) return;

      var random = new Random();
      var roll = random.Next(1, 7);

      await ModifyOriginalResponseAsync(properties => properties.Content = $"You rolled a {roll}!");
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error in RollCommandAsync");
    }
  }
}
