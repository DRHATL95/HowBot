using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Howbot.Core.Helpers;

public static class ReflectionHelper
{
  [CanBeNull]
  public static Assembly GetAssemblyByName(string assemblyName)
  {
    if (string.IsNullOrEmpty(assemblyName))
    {
      throw new ArgumentNullException(nameof(assemblyName));
    }

    return AppDomain.CurrentDomain.GetAssemblies()
      .SingleOrDefault(assembly => assembly.GetName().Name == assemblyName);
  }
}
