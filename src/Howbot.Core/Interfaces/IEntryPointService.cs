using System.Threading;
using System.Threading.Tasks;

namespace Howbot.Core.Interfaces;

public interface IEntryPointService
{
  Task ExecuteAsync(CancellationToken cancellationToken);
}
