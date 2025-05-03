using System.Text;
using Discord;
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

  [SlashCommand(Constants.Commands.ActivitiesCommandName, Constants.Commands.ActivitiesCommandDescription, true,
    RunMode.Async)]
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
        selectMenuBuilder.AddOption(new SelectMenuOptionBuilder().WithLabel(activity.Name)
          .WithValue(activity.Id.ToString()));
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


  [Group("eft", "Escape from Tarkov commands.")]
  public class EscapeFromTarkovGroup(IHttpService httpService, ILoggerAdapter<EscapeFromTarkovGroup> logger)
    : InteractionModuleBase<SocketInteractionContext>
  {
    [SlashCommand("price", "Get all relevant pricing information on a given item.", false, RunMode.Async)]
    public async Task EftItemCommandAsync([Summary("itemName", "The name of the item to search for.")] string itemName)
    {
      await DeferAsync();

      try
      {
        var item = await httpService.GetTarkovMarketPriceByItemNameAsync(itemName);

        if (item is null)
        {
          await ModifyOriginalResponseAsync(properties => properties.Content = "Failed to get Tarkov market price.");
          return;
        }

        // Create an embed that displays the amount each vendor will give for the item
        var embedFields = item.SellFor.Select(price => new EmbedFieldBuilder()
            .WithName(price.Vendor.Name)
            .WithValue($"{price.Price:N0} \u20bd")
            .WithIsInline(true))
          .ToList();

        if (item.ChangeLast48HPercent is not null)
        {
          if (item.ChangeLast48HPercent > 0.0f)
          {
            embedFields.Add(new EmbedFieldBuilder()
              .WithName($" {Emojis.UpArrow} Price Change")
              .WithValue($"{item.ChangeLast48HPercent} %")
              .WithIsInline(true));
          }
          else
          {
            embedFields.Add(new EmbedFieldBuilder()
              .WithName($"{Emojis.DownArrow} Price Change")
              .WithValue($"{item.ChangeLast48HPercent} %")
              .WithIsInline(true));
          }
        }

        // Add 48hr price change percent to the embed
        embedFields.Add(new EmbedFieldBuilder()
          .WithName("Avg 24h Price")
          .WithValue($"{item.Avg24HPrice:N0} \u20bd")
          .WithIsInline(true));

        var embed = new EmbedBuilder();
        embed.WithTitle(item.Name)
          .WithUrl(item.WikiUrl ?? "https://escapefromtarkov.gamepedia.com/Escape_from_Tarkov_Wiki")
          .WithDescription(item.Description)
          .WithFields(embedFields)
          .WithImageUrl(item.IconUrl)
          .WithColor(Constants.ThemeColor)
          .WithFooter($"Updated at {item.Updated}");

        await ModifyOriginalResponseAsync(properties => properties.Embed = embed.Build());
      }
      catch (Exception exception)
      {
        logger.LogError(exception, nameof(EftItemCommandAsync));
        throw;
      }
    }

    [SlashCommand("task", "Lookup information on an eft task.", false, RunMode.Async)]
    public async Task EftTaskCommandAsync([Summary("taskName", "The name of the task to search for.")] string taskName)
    {
      await DeferAsync();

      try
      {
        var task = await httpService.GetTarkovTaskByTaskNameAsync(taskName);
        if (task is null)
        {
          await ModifyOriginalResponseAsync(properties =>
            properties.Content = "Failed to get Tarkov task information.");
          return;
        }

        var sb = new StringBuilder();

        sb.AppendLine(
          $"The task **{task.Name}** is given by **{task.Trader.Name}** for **{task.Map?.Name}** and requires you to:");
        sb.AppendLine();
        sb.AppendLine("**Objectives:**");
        foreach (var objective in task.Objectives)
        {
          sb.AppendLine($"- {objective.Description}");
        }

        await ModifyOriginalResponseAsync(properties =>
          properties.Content = sb.ToString());
      }
      catch (Exception exception)
      {
        logger.LogError(exception, nameof(EftTaskCommandAsync));
        throw;
      }
    }
  }
}
