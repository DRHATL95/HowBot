using System.Threading.Tasks;
using Howbot.Core.Entities;

namespace Howbot.Core.Interfaces;

public interface IUrlStatusChecker
{
  Task<UrlStatusHistory> CheckUrlAsync(string url, string requestId);
}
