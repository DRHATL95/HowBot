using Howbot.Application.Models.Discord.Commands;
using Howbot.Application.Models.Tarkov;
using Task = Howbot.Application.Models.Tarkov.Task;

namespace Howbot.Application.Interfaces.Infrastructure;

public interface IHttpService
{
  Task<int> GetUrlResponseStatusCodeAsync(string url, CancellationToken cancellationToken = default);

  Task<string> CreateWatchTogetherRoomAsync(string url, CancellationToken cancellationToken = default);

  Task<List<ActivityApplication>> GetCurrentApplicationIdsAsync(CancellationToken cancellationToken = default);

  Task<string> StartDiscordActivityAsync(string channelId, string activityId,
    CancellationToken cancellationToken = default);

  Task<string> GetRandomCatImageUrlAsync(int limit = 1, CancellationToken cancellationToken = default);

  Task<string> GetRandomDogImageUrlAsync(int limit = 1, CancellationToken cancellationToken = default);

  Task<Item?> GetTarkovMarketPriceByItemNameAsync(string itemName,
    CancellationToken cancellationToken = default);

  Task<Task?> GetTarkovTaskByTaskNameAsync(string taskName, CancellationToken cancellationToken = default);
}
