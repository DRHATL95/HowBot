using Howbot.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Infrastructure.Services.Concrete;

/// <summary>
///   A wrapper around ServiceScopeFactory to make it easier to fake out with MOQ.
///   Entity Framework requires scope, hence why this class was created. This is only for scoped services though, not
///   singletons
/// </summary>
/// <see cref="https://stackoverflow.com/a/53509491/54288" />
public sealed class ServiceScopeFactoryLocator(IServiceScopeFactory factory) : IServiceLocator
{
  private IServiceScope? _scope;

  public T? Get<T>()
  {
    CreateScope();

    if (_scope is null)
    {
      throw new InvalidOperationException("Service scope is null");
    }

    return _scope.ServiceProvider.GetService<T>();
  }

  public IServiceScope CreateScope()
  {
    return _scope ??= factory.CreateScope();
  }

  public void Dispose()
  {
    _scope?.Dispose();
    _scope = null;
  }
}
