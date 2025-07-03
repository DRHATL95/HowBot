using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Howbot.Application.Enums;

namespace Howbot.Application.Constants;
public class BotDefaults
{
  public const string DefaultPrefix = "!~";
  public const float DefaultVolume = 50f;

  public static readonly SearchProviderTypes DefaultSearchProvider = SearchProviderTypes.YouTube;
}
