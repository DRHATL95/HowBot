using System;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Core.Interfaces;

// Purpose: Interface for the ServiceLocator
public interface IServiceLocator : IDisposable
{
  /// <summary>
  ///   Creates a scope used to resolve dependencies that are scoped
  /// </summary>
  /// <returns></returns>
  IServiceScope CreateScope();

  /// <summary>
  ///   Gets a service of type T
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  T Get<T>();
}
