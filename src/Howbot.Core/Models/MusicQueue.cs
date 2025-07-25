using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Models;
public class MusicQueue
{
  public List<ExtendedLavalinkTrack> Tracks { get; set; } = [];
  public int CurrentIndex { get; set; }
  public bool IsShuffled { get; set; }
  public TrackRepeatMode RepeatMode { get; set; } = TrackRepeatMode.Queue;
}
