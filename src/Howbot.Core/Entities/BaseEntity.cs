using System.ComponentModel.DataAnnotations;

namespace Howbot.Core.Entities;

public abstract class BaseEntity
{
  [Key] public ulong Id { get; set; }
}
