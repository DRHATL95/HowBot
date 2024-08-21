using System.ComponentModel.DataAnnotations;
using Howbot.Core.Models;
using Howbot.Core.Models.Enums;

namespace Howbot.Core.Entities;

public class Guild : BaseEntity
{
  [Required] [StringLength(10)] public string Prefix { get; set; } = Constants.DefaultPrefix;

  [Required] public float Volume { get; set; } = 100.0f;

  [Required] public SearchProviderTypes SearchProvider { get; set; } = SearchProviderTypes.YouTubeMusic;
  
  public string EncryptedSessionId { get; set; } = string.Empty;
}
