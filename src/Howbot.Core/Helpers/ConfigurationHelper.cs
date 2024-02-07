using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace Howbot.Core.Helpers;

/// <summary>
///   Class of static helpers used for handling configuration.
/// </summary>
public static class ConfigurationHelper
{
  public static IConfiguration HostConfiguration { get; private set; }

  /// <summary>
  ///   Adds or updates configuration settings in appsettings.json
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="sectionPathKey"></param>
  /// <param name="value"></param>
  public static void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
  {
    try
    {
      var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
      var json = File.ReadAllText(filePath);
      dynamic jsonObj = JsonConvert.DeserializeObject(json);

      SetValueRecursively(sectionPathKey, jsonObj, value);

      string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
      File.WriteAllText(filePath, output);
    }
    catch (Exception ex)
    {
      Log.Logger.Error(ex, "Error writing app settings");
    }
  }

  /// <summary>
  ///   Used to update a section of a jsonObjection, specifically used for <see cref="AddOrUpdateAppSetting{T}(string, T)" />
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="sectionPathKey"></param>
  /// <param name="jsonObj"></param>
  /// <param name="value"></param>
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

  /// <summary>
  ///   Allows for setting the host configuration globally.
  /// </summary>
  /// <param name="configuration">The configuration to assign to global variable.</param>
  public static void SetHostConfiguration(IConfiguration configuration)
  {
    HostConfiguration = configuration;
  }
}
