using Howbot.Core.Models.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Howbot.Core.Models.Commands;

public class ApiCommandResponse
{
  public bool IsSuccessful { get; init; }
  public object? Value { get; set; }
  public string ValueType { get; init; } = string.Empty;
  public ApiCommandRequestException? Exception { get; init; }

  public static ApiCommandResponse Create(bool isSuccessful, ApiCommandRequestException? exception = null,
    object? value = default)
  {
    return new ApiCommandResponse
    {
      IsSuccessful = isSuccessful,
      Exception = exception,
      Value = value,
      ValueType = value?.GetType().AssemblyQualifiedName ?? string.Empty
    };
  }
}

public class ApiCommandResponseConverter : JsonConverter
{
  public override bool CanConvert(Type objectType)
  {
    return objectType == typeof(ApiCommandResponse);
  }

  public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
  {
    var jObject = JObject.Load(reader);

    var response = new ApiCommandResponse
    {
      IsSuccessful = jObject["IsSuccessful"]?.Value<bool>() ?? false,
      Exception = jObject["Exception"]?.ToObject<ApiCommandRequestException>(serializer) ?? null,
      ValueType = jObject["ValueType"]?.Value<string>() ?? string.Empty
    };

    // Use the ValueType property to determine the type of Value
    var valueType = Type.GetType(response.ValueType);
    response.Value = jObject["Value"]?.ToObject(valueType, serializer) ?? null;

    return response;
  }

  public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
  {
    throw new NotImplementedException();
  }
}
