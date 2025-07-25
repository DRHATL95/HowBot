using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Core.Models.Enums;
using Lavalink4NET.Integrations.Lavasrc;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;

namespace Howbot.Core.Models;
public class MusicStatus
{
  public bool IsPlaying { get; set; }
  public bool IsPaused { get; set; }
  public TimeSpan Duration { get; set; }
  public TrackPosition? Position { get; set; }
  public ExtendedLavalinkTrack? CurrentTrack { get; set; }
  public int QueueCount { get; set; }
  public VolumeLevel Volume { get; set; }
  public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
