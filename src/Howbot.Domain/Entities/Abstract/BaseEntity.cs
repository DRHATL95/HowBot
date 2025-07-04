namespace Howbot.Domain.Entities.Abstract;
public abstract class BaseEntity
{
  public Guid Id { get; set; } = Guid.NewGuid();
}
