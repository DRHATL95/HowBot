namespace Howbot.Core.Models.Commands;

public enum CommandTypes
{
  Unknown = 0,
  SendMessage = 1,
  SendEmbed = 2,
  JoinVoiceChannel = 3,
  LeaveVoiceChannel = 4,
  Play = 5,
  Stop = 6,
  Skip = 7,
  Pause = 8,
  Resume = 9,
  Queue = 10,
  IsPlaying = 11,
  Session = 12,
  Guild = 13,
  Guilds = 14
}
