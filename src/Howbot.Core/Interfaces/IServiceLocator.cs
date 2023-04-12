using System;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Core.Interfaces;

public interface IServiceLocator : IDisposable
{
  IServiceScope CreateScope();

  T Get<T>();
}
