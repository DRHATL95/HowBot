using System;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Howbot.UnitTests.Core.Services;

public class DiscordClientServiceTest
{
  private static Mock<IRepository> SetupCreateScope(Mock<IServiceLocator> serviceLocator)
  {
    var fakeScope = new Mock<IServiceScope>();
    serviceLocator.Setup(sl => sl.CreateScope())
      .Returns(fakeScope.Object);

    var serviceProvider = new Mock<IServiceProvider>();
    fakeScope.Setup(s => s.ServiceProvider)
      .Returns(serviceProvider.Object);

    return SetupCustomInjection(serviceProvider);
  }
  
  private static Mock<IRepository> SetupCustomInjection(Mock<IServiceProvider> serviceProvider)
  {
    // GetRequiredService is an extension method, but GetService is not
    var repository = new Mock<IRepository>();
    serviceProvider.Setup(sp => sp.GetService(typeof(IRepository)))
      .Returns(repository.Object);

    // return a tuple as you have more dependencies
    return repository;
  }
}
