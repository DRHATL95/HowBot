using System.ComponentModel.DataAnnotations;

namespace Howbot.Core.Models;

public enum LavalinkSourceNames
{
  [Display(Name = "Unknown")] Unknown = -1,
  [Display(Name = "YouTube")] Youtube = 0,
  [Display(Name = "Twitch")] Twitch = 1,
  [Display(Name = "SoundCloud")] SoundCloud = 2,
  [Display(Name = "BandCamp")] BandCamp = 3,
  [Display(Name = "Vimeo")] Vimeo = 4,
  [Display(Name = "Spotify")] Spotify = 5,
  [Display(Name = "Apple Music")] AppleMusic = 6,
  [Display(Name = "Deezer")] Deezer = 7,
  [Display(Name = "Yandex")] Yandex = 8,
  [Display(Name = "FlowerTTS")] FlowerTts = 9,
  [Display(Name = "HTTP")] Http = 10,
  [Display(Name = "Local")] Local = 11
}
