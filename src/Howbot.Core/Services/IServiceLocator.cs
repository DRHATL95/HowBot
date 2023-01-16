using System;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Core.Services;

public interface IServiceLocator : IDisposable
{
  IServiceScope CreateScope();
  T Get<T>();
}
