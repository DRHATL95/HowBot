using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Howbot.Core.Models;

namespace Howbot.Core.Interfaces;

public interface IHttpService
{
  Task<int> GetUrlResponseStatusCodeAsync(string url);
  
  Task<string> CreateWatchTogetherRoomAsync(string url);
  
  Task<List<ActivityApplication>> GetCurrentApplicationIdsAsync(CancellationToken token = default);

  Task<string> StartDiscordActivity(string channelId, string activityId);
}
