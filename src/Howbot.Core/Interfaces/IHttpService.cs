using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IHttpService
{
  Task<int> GetUrlResponseStatusCodeAsync(string url);
  
  Task<string> CreateWatchTogetherRoomAsync(string url);
}
