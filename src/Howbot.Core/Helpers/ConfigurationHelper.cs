using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace Howbot.Core.Helpers;

public static class ConfigurationHelper
{
  public static IConfiguration? HostConfiguration { get; private set; }

  public static void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
  {
    try
    {
      var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
      var json = File.ReadAllText(filePath);
      dynamic? jsonObj = JsonConvert.DeserializeObject(json);

      if (jsonObj is null)
      {
        Log.Logger.Error("Error reading app settings");
        return;
      }

      SetValueRecursively(sectionPathKey, jsonObj, value);

      string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
      File.WriteAllText(filePath, output);
    }
    catch (Exception ex)
    {
      Log.Logger.Error(ex, "Error writing app settings");
    }
  }

  public static void SetHostConfiguration(IConfiguration configuration)
  {
    HostConfiguration = configuration;
  }

  private static void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
  {
    // split the string at the first ':' character
    var remainingSections = sectionPathKey.Split(":", 2);

    var currentSection = remainingSections[0];
    if (remainingSections.Length > 1)
    {
      // continue with the process, moving down the tree
      var nextSection = remainingSections[1];
      SetValueRecursively(nextSection, jsonObj[currentSection], value);
    }
    else
    {
      // we've got to the end of the tree, set the value
      jsonObj[currentSection] = value;
    }
  }
}
