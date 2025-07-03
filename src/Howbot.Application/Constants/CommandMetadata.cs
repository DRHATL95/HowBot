using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Application.Constants;
public class CommandMetadata
{
  public const string JoinCommandName = "join";
  public const string JoinCommandDescription = "Join a valid server voice channel, otherwise nothing.";

  public const string PlayCommandName = "play";

  public const string PlayCommandDescription =
    "Plays a track from a given search query. Optionally, can choose different search providers.";

  public const string PlaySearchRequestArgumentName = "search_request";
  public const string PlaySearchRequestArgumentDescription = "Search request used for search provider.";

  public const string PauseCommandName = "pause";
  public const string PauseCommandDescription = "Pauses a currently playing song, otherwise nothing.";

  public const string ResumeCommandName = "resume";
  public const string ResumeCommandDescription = "Resumes the currently paused or stopped track, otherwise nothing.";

  public const string SeekCommandName = "seek";
  public const string SeekCommandDescription = "Seek to a position on the current playing track.";

  public const string SkipCommandName = "skip";

  public const string SkipCommandDescription =
    "Skip either to the next track or a valid number of songs in the track queue.";

  public const string VolumeCommandName = "volume";

  public const string VolumeCommandDescription =
    "Change the volume of the current playing track. Leaving blank will show the current volume.";

  public const string NowPlayingCommandName = "nowplaying";
  public const string NowPlayingCommandDescription = "Gets an embed for the current playing/paused track.";

  public const string LeaveCommandName = "leave";

  public const string LeaveCommandDescription =
    "Leaves the current voice channel and removes any songs from the queue.";

  public const string PingCommandName = "ping";
  public const string PingCommandDescription = "Ping the Discord websocket API.";

  public const string HelpCommandName = "help";
  public const string HelpCommandDescription = "See all available commands for the bot.";

  public const string HelpCommandArgumentName = "command";
  public const string HelpCommandArgumentDescription = "The name of the command to get help for.";

  /*public const string RadioCommandName = "radio";
  public const string RadioCommandDescription = "Plays a radio station by a given genre or radio station name.";*/

  public const string ShuffleCommandName = "shuffle";
  public const string ShuffleCommandDescription = "Shuffles the current servers music queue.";

  public const string LyricsCommandName = "lyrics";
  public const string LyricsCommandDescription = "Gets lyrics from lyrics.ovh for the current playing track.";

  public const string QueueCommandName = "queue";
  public const string QueueCommandDescription = "Gets the current music queue for the server.";

  public const string PurgeCommandName = "purge";
  public const string PurgeCommandDescription = "Purge current text channel from up to 10,000 messages.";

  public const string BanCommandName = "ban";
  public const string BanCommandDescription = "Permanently ban a user from the server.";

  public const string UnbanCommandName = "unban";
  public const string UnbanCommandDescription = "Unban a user from the server.";

  public const string RollCommandName = "roll";
  public const string RollCommandDescription = "Rolls a dice.";

  public const string FlipCommandName = "flip";
  public const string FlipCommandDescription = "Flips a coin.";

  public const string EftCommandName = "eft";
  public const string EftCommandDescription = "Gets price information about Escape from Tarkov items.";

  public const string WatchTogetherCommandName = "w2g";
  public const string WatchTogetherCommandDescription = "Creates a Watch2gether room with a given URL.";

  public const string ClearCommandName = "clear";
  public const string ClearCommandDescription = "Clears the current music queue.";

  public const string CleanCommandName = "clean";
  public const string CleanCommandDescription = "Cleans the current text channel from bot messages.";

  public const string ActivitiesCommandName = "activities";
  public const string ActivitiesCommandDescription = "Gets the current activities for the bot.";

  public const string MuteCommandName = "mute";
  public const string MuteCommandDescription = "Mutes a user in the voice channel.";

  public const string UnmuteCommandName = "unmute";
  public const string UnmuteCommandDescription = "Unmutes a user in the voice channel.";

  public const string SettingsProviderArgumentName = "search_type";

  public const string SettingsProviderArgumentDescription =
    "Optional search type used for search request. (Default: YouTube Music)";

  public const string SettingsVolumeArgumentName = "volume";
  public const string SettingsVolumeArgumentDescription = "Optional volume level for the bot. (Default: 50)";

  public const string SettingsPrefixArgumentName = "prefix";
  public const string SettingsPrefixArgumentDescription = "Optional prefix for the bot. (Default: !~)";

  public const string AutoPlayCommandName = "autoplay";
  public const string AutoPlayCommandDescription = "Toggles autoplay for the bot on this Guild.";

  public const string KickCommandName = "kick";
  public const string KickCommandDescription = "Kicks a user from the server.";

  public const string SlowmodeCommandName = "slowmode";
  public const string SlowmodeCommandDescription = "Sets the slowmode for the current text channel.";

  public const string LockCommandName = "lock";
  public const string LockCommandDescription = "Locks the current text channel.";

  public const string UnlockCommandName = "unlock";
  public const string UnlockCommandDescription = "Unlocks the current text channel.";

  public const string SayCommandName = "say";
  public const string SayCommandDescription = "Repeats the message back to the user.";

  public const string DogCommandName = "dog";
  public const string DogCommandDescription = "Gets a random dog image or up to 10 random images.";

  public const string CatCommandName = "cat";
  public const string CatCommandDescription = "Gets a random cat image or up to 10 random images.";
}
