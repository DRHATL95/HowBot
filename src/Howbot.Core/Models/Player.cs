using System.Collections.Generic;
using System.Collections.ObjectModel;
using Discord;
using JetBrains.Annotations;
using Victoria.Player;
using Victoria.WebSocket;

namespace Howbot.Core.Models;

public class Player<T> : LavaPlayer<T> where T : LavaTrack
{
  // Constructor
  public Player(WebSocketClient socketClient, IVoiceChannel voiceChannel, ITextChannel textChannel) : base(socketClient,
    voiceChannel, textChannel)
  {
    RecentlyPlayed = new Collection<LavaTrack>();
  }

  public bool Is247ModeEnabled { get; set; }
  
  public IGuildUser Author { get; set; }

  [CanBeNull] public LavaTrack LastPlayed { get; set; }

  public ICollection<LavaTrack> RecentlyPlayed { get; set; }

  public void Toggle247Mode() => Is247ModeEnabled = !Is247ModeEnabled;
}
