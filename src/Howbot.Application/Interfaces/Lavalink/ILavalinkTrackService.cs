using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Application.Enums;
using Lavalink4NET.Tracks;

namespace Howbot.Application.Interfaces.Lavalink;
public interface ILavalinkTrackService
{
  ValueTask<List<LavalinkTrack>> SearchBySearchQueryAsync(string searchQuery, SearchProviderTypes searchProvider, CancellationToken token = default);
}
