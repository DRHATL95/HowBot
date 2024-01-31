namespace Howbot.Core.Models;

public struct ActivityApplication
{
  public string Version { get; set; }
  
  public string IconUrl { get; set; }
  
  public ulong Id { get; set; }
  
  public string Name { get; set; }
  
  public string MaxParticipants { get; set; }
}
