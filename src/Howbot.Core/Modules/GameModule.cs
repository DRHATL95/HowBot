using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;

namespace Howbot.Core.Modules;

public class GameModule(IHttpService httpService, ILoggerAdapter<GameModule> logger)
  : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand(Constants.Commands.RollCommandName, Constants.Commands.RollCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  public async Task RollCommandAsync([Summary("totalDice", "Amount of dice to roll. Max is 10.")] int amountOfDice = 1)
  {
    await DeferAsync();

    try
    {
      await ModifyOriginalResponseAsync(properties => properties.Content = "Rolling dice..");

      var random = new Random();

      switch (amountOfDice)
      {
        case > 10:
          await ModifyOriginalResponseAsync(properties =>
            properties.Content = "You can't roll more than 10 dice at once!");
          return;

        case > 1:
          {
            var rolls = new int[amountOfDice];

            for (var i = 0; i < amountOfDice; i++)
            {
              rolls[i] = random.Next(1, 7);
            }

            await ModifyOriginalResponseAsync(
              properties => properties.Content = $"You rolled {string.Join(", ", rolls)}!");
            return;
          }
        default:
          {
            var roll = random.Next(1, 7);

            await ModifyOriginalResponseAsync(properties => properties.Content = $"You rolled a {roll}!");
            break;
          }
      }
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(RollCommandAsync));
      throw;
    }
  }

  [SlashCommand(Constants.Commands.FlipCommandName, Constants.Commands.FlipCommandDescription, true, RunMode.Async)]
  [RequireContext(ContextType.Guild)]
  public async Task FlipCommandAsync()
  {
    await DeferAsync();

    try
    {
      await ModifyOriginalResponseAsync(properties => properties.Content = "Flipping coin..");

      var random = new Random();
      var flip = random.Next(1, 3);

      await ModifyOriginalResponseAsync(properties =>
        properties.Content = $"You flipped a {(flip == 1 ? "heads" : "tails")}!");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(FlipCommandAsync));
      throw;
    }
  }

  /*[SlashCommand("test", "test", true, RunMode.Async)]
  [RequireOwner]
  public async Task TestCommandAsync()
  {
    await DeferAsync();

    try
    {
      var ids = await httpService.GetCurrentApplicationIdsAsync();

      if (ids.Any())
      {
        var yt = ids.FirstOrDefault(x => x.Id == 880218394199220334);
        var link = await httpService.StartDiscordActivityAsync(Context.Channel.Id.ToString(), yt.Id.ToString());
        if (!string.IsNullOrEmpty(link))
        {
          await ModifyOriginalResponseAsync(properties => properties.Content = link);
          return;
        }
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = $"Ids returned {ids.Count}");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(TestCommandAsync));
      throw;
    }
  }*/

  [SlashCommand(Constants.Commands.EftCommandName, Constants.Commands.EftCommandDescription, true, RunMode.Async)]
  public async Task EftCommandAsync([Summary("itemName", "The name of the item to search for.")] string itemName)
  {
    await DeferAsync();

    try
    {
      var priceTuple = await httpService.GetTarkovMarketPriceByItemNameAsync(itemName);

      if (priceTuple is null)
      {
        await ModifyOriginalResponseAsync(properties => properties.Content = "Failed to get Tarkov market price.");
        return;
      }

      await ModifyOriginalResponseAsync(properties => properties.Content = $"The item **{priceTuple.Item1}** is being bought by **{priceTuple.Item2}** for the highest price of **{priceTuple.Item3:N0}** \u20bd.");
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(EftCommandAsync));
      await DeleteOriginalResponseAsync();
    }
  }
}
