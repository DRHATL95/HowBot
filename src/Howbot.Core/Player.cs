using System.Collections.Generic;
using System.Collections.ObjectModel;
using Discord;
using JetBrains.Annotations;
using Victoria.Player;
using Victoria.WebSocket;

namespace Howbot.Core;

public class Player<T> : LavaPlayer<T> where T : LavaTrack
{
  // Constructor
  public Player(WebSocketClient socketClient, IVoiceChannel voiceChannel, ITextChannel textChannel) : base(socketClient,
    voiceChannel, textChannel)
  {
    RecentlyPlayed = new Collection<LavaTrack>();
  }

  public bool IsRadioMode { get; set; }
  public IGuildUser Author { get; set; }

  [CanBeNull] public LavaTrack LastPlayed { get; set; }

  // TODO: Message queue implementation
  public ICollection<LavaTrack> RecentlyPlayed { get; set; }

  public void ToggleRadioMode()
  {
    IsRadioMode = !IsRadioMode;
  }
}
