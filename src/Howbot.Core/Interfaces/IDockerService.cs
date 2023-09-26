using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;

namespace Howbot.Core.Interfaces;

public interface IDockerService
{
  Task<IList<ContainerListResponse>> ListAllContainers();

  Task<IList<ImagesListResponse>> ListAllImages();

  Task<ImageInspectResponse> GetImageByImageName(string imageName);

  Task<ContainerInspectResponse> GetContainerByContainerName(string containerId);

  Task<bool> BuildImageByImageName(string imageName);

  Task<bool> BuildImageByImageName(string imageName, string tagName);
}
