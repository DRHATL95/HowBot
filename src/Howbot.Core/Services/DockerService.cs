using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Howbot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Howbot.Core.Services;

public class DockerService : IDockerService
{
  private readonly IServiceLocator _serviceLocator;
  public DockerService(IServiceLocator serviceLocator)
  {
    _serviceLocator = serviceLocator;
  }

#pragma warning disable CS1998
  public async Task<IList<ContainerListResponse>> ListAllContainers()
#pragma warning restore CS1998
  {
    using var scope = _serviceLocator.CreateScope();
    // var dockerClient = scope.ServiceProvider.GetService<IDockerClient>();

    return new List<ContainerListResponse>();

    // return await dockerClient.Containers.ListContainersAsync(new ContainersListParameters());
  }

#pragma warning disable CS1998
  public async Task<IList<ImagesListResponse>> ListAllImages()
#pragma warning restore CS1998
  {
    using var scope = _serviceLocator.CreateScope();
    // var dockerClient = scope.ServiceProvider.GetService<IDockerClient>();

    return new List<ImagesListResponse>();

    // return await dockerClient.Images.ListImagesAsync(new ImagesListParameters());
  }

#pragma warning disable CS1998
  public async Task<ImageInspectResponse> GetImageByImageName(string imageName)
#pragma warning restore CS1998
  {
    using var scope = _serviceLocator.CreateScope();
    // var dockerClient = scope.ServiceProvider.GetService<IDockerClient>();

    return new ImageInspectResponse();

    // return await dockerClient.Images.InspectImageAsync(imageName);
  }

#pragma warning disable CS1998
  public async Task<ContainerInspectResponse> GetContainerByContainerName(string containerId)
#pragma warning restore CS1998
  {
    using var scope = _serviceLocator.CreateScope();
    // var dockerClient = scope.ServiceProvider.GetService<IDockerClient>();

    return new ContainerInspectResponse();

    // return await dockerClient.Containers.InspectContainerAsync(containerId);
  }

  public async Task<bool> BuildImageByImageName(string imageName)
  {
    using var scope = _serviceLocator.CreateScope();
    // var dockerClient = scope.ServiceProvider.GetService<IDockerClient>();

    if (await DoesImageExist(imageName)) return true;

    /*await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = imageName },
      new AuthConfig(), new Progress<JSONMessage>());*/

    return true;
  }

  public async Task<bool> BuildImageByImageName(string imageName, string tagName)
  {
    using var scope = _serviceLocator.CreateScope();
    // var dockerClient = scope.ServiceProvider.GetService<IDockerClient>();

    if (await DoesImageExist(imageName)) return true;

    /*await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = imageName, Tag = tagName},
      new AuthConfig(), new Progress<JSONMessage>());*/

    return true;
  }

  private async Task<bool> DoesImageExist(string imageName, string tagName = "latest")
  {
    using var scope = _serviceLocator.CreateScope();
    // var dockerClient = scope.ServiceProvider.GetService<IDockerClient>();

    var images = await ListAllImages();

    return images.Any(i => i.RepoTags.Contains($"{imageName}:{tagName}"));
  }

  public void Initialize()
  {
    // TODO:
  }
}
