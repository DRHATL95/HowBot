using System;

namespace Howbot.Worker;

/// <summary>
///   A settings class used for background worker service
/// </summary>
public class WorkerSettings
{
  public TimeSpan DelayInMilliseconds { get; set; }
}
