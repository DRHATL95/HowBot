using System.Threading.Tasks;
using Discord;
using Howbot.Core.Entities;
using Victoria.Responses.Search;

namespace Howbot.Core.Interfaces;

public interface IMusicService
{
  public Task<CommandResponse> PlayBySearchTypeAsync(SearchType searchType, string searchRequest, IGuildUser user,
    IVoiceState voiceState, ITextChannel textChannel);

  public Task<CommandResponse> PlayByYouTubeSearch(string searchRequest, IGuildUser user, IVoiceState voiceState,
    ITextChannel textChannel);
}
