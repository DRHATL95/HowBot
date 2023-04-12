using System;
using System.IO;

namespace Howbot.Core.Helpers;

/// <summary>
/// Class of static helpers used for handling configuration.
/// </summary>
public class ConfigurationHelper
{
  /// <summary>
  /// Adds or updates configuration settings in appsettings.json
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="sectionPathKey"></param>
  /// <param name="value"></param>
  public static void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
  {
    try
    {
      var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
      string json = File.ReadAllText(filePath);
      dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

      SetValueRecursively(sectionPathKey, jsonObj, value);

      string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
      File.WriteAllText(filePath, output);
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error writing app settings | {0}", ex.Message);
    }
  }

  /// <summary>
  /// Used to update a section of a jsonObjection, specifically used for <see cref="AddOrUpdateAppSetting{T}(string, T)"/>
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
      // continue with the procress, moving down the tree
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
