using Discord;
using JetBrains.Annotations;
using Victoria.Player;
using Victoria.WebSocket;

namespace Howbot.Core;

public class Player<T> : LavaPlayer<T> where T : LavaTrack
{
  private bool IsRadioMode { get; set; }
  public IGuildUser Author { get; set; }
  [CanBeNull] public LavaTrack LastPlayed { get; set; }
  
  public Player(WebSocketClient socketClient, IVoiceChannel voiceChannel, ITextChannel textChannel) : base(socketClient, voiceChannel, textChannel) { }

  public bool RadioMode() => IsRadioMode;

  public void EnableRadioMode()
  {
    IsRadioMode = true;
  }

  public void DisableRadioMode()
  {
    IsRadioMode = false;
  }
}
