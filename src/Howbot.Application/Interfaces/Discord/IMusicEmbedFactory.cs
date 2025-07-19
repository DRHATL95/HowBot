using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Howbot.Application.Interfaces.Discord;
public interface IMusicEmbedFactory
{
  Embed? CreateEmbed(string title, string description, string? footer = null, string? thumbnailUrl = null, string? imageUrl = null, Color? color = null);
  Embed? CreateEmbed(string title, string description, IList<EmbedField> embedFields, string? footer = null, string? thumbnailUrl = null, string? imageUrl = null, Color? color = null);
}
