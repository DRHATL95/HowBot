using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Howbot.Core.Interfaces;
using Howbot.Core.Models;
using ContextType = Discord.Interactions.ContextType;
using RunMode = Discord.Interactions.RunMode;

namespace Howbot.Core.Modules;

public class GameModule(IHttpService httpService, ILoggerAdapter<GameModule> logger)
  : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand(Constants.Commands.RollCommandName, Constants.Commands.RollCommandDescription, true, RunMode.Async)]
  [Discord.Interactions.RequireContext(ContextType.Guild)]
  public async Task RollCommandAsync([Discord.Interactions.Summary("totalDice", "Amount of dice to roll. Max is 10.")] int amountOfDice = 1)
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
  [Discord.Interactions.RequireContext(ContextType.Guild)]
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

  [SlashCommand(Constants.Commands.ActivitiesCommandName, Constants.Commands.ActivitiesCommandDescription, true, RunMode.Async)]
  public async Task ActivitiesCommandAsync()
  {
    try
    {
      var activities = await httpService.GetCurrentApplicationIdsAsync();
      
      // Generate a select menu for user to select from to start activity
      var selectMenuBuilder = new SelectMenuBuilder()
        .WithPlaceholder("Select an activity")
        .WithCustomId("activity_select")
        .WithMinValues(1)
        .WithMaxValues(1);
      
      // Limit the amount of options to 25
      if (activities.Count > 25)
      {
        activities = activities.Take(25).ToList();
      }
      
      foreach (var activity in activities)
      {
        selectMenuBuilder.AddOption(new SelectMenuOptionBuilder().WithLabel(activity.Name).WithValue(activity.Id.ToString()));
      }
      
      var builder = new ComponentBuilder()
        .WithSelectMenu(selectMenuBuilder);
      
      await ReplyAsync(components: builder.Build());
    }
    catch (Exception exception)
    {
      logger.LogError(exception, nameof(ActivitiesCommandAsync));
      throw;
    }
  }

  [SlashCommand(Constants.Commands.EftCommandName, Constants.Commands.EftCommandDescription, true, RunMode.Async)]
  [Discord.Interactions.RequireOwner]
  public async Task EftCommandAsync([Discord.Interactions.Summary("itemName", "The name of the item to search for.")] string itemName)
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
