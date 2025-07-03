using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Application.Interfaces.Infrastructure;

public interface IServiceLocator : IDisposable
{
  IServiceScope CreateScope();

  T? Get<T>();
}
