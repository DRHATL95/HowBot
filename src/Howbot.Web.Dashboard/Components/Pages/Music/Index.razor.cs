using Howbot.Core.Models;
using Howbot.Core.Models.Commands;
using Lavalink4NET.Tracks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace Howbot.Web.Dashboard.Components.Pages.Music;

public partial class Index
{
  private string SearchQuery { get; set; } = string.Empty;
  private LavalinkTrack? CurrentTrack { get; set; }
  private List<LavalinkTrack> Queue { get; } = [];

  private GuildDto? SelectedGuild { get; set; }

  [Parameter] public long GuildId { get; set; }

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();

    await LoadInitialDataAsync()
      .ConfigureAwait(false);
  }

  private async Task LoadInitialDataAsync()
  {
    try
    {
      var response = await HttpClient.GetAsync($"api/Bot");
      if (response.IsSuccessStatusCode)
      {
        var result = await response.Content.ReadFromJsonAsync<ApiCommandResponse>();

        if (result != null)
        {
          SelectedGuild = JsonConvert.DeserializeObject<GuildDto>((result.Value as string) ?? string.Empty);
        }
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }

  private void SearchSongs()
  {
    if (string.IsNullOrWhiteSpace(SearchQuery))
    {
      return;
    }

    var track = new LavalinkTrack
    {
      Title = "The Best Song",
      Identifier = "123",
      Author = "The Best Artist",
    };

    if (CurrentTrack != null)
    {
      Queue.Add(track);
    }
    else
    {
      CurrentTrack = track;
    }
  }
}
