using Howbot.Core.Models.Commands;

namespace Howbot.Core.Interfaces;

public interface IHttpService
{
  Task<int> GetUrlResponseStatusCodeAsync(string url, CancellationToken cancellationToken = default);

  Task<string> CreateWatchTogetherRoomAsync(string url, CancellationToken cancellationToken = default);

  Task<List<ActivityApplication>> GetCurrentApplicationIdsAsync(CancellationToken cancellationToken = default);

  Task<string> StartDiscordActivityAsync(string channelId, string activityId,
    CancellationToken cancellationToken = default);

  Task<Tuple<string, string, int>?> GetTarkovMarketPriceByItemNameAsync(string itemName,
    CancellationToken cancellationToken = default);

  Task<string> GetTarkovTaskByTaskNameAsync(string taskName, CancellationToken cancellationToken = default);

  Task<string> GetRandomCatImageUrlAsync(int limit = 1, CancellationToken cancellationToken = default);

  Task<string> GetRandomDogImageUrlAsync(int limit = 1, CancellationToken cancellationToken = default);
}
