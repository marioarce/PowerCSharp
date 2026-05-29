using System;
using System.Text.Json;

namespace PowerCSharp.Helpers;

/// <summary>
/// Helper class for JSON serialization and deserialization
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Safely serializes an object to JSON string
    /// </summary>
    public static string SafeSerialize<T>(T obj)
    {
        try
        {
            var result = JsonSerializer.Serialize(obj);
            return result;
        }
        catch (Exception)
        {
            return "{}";
        }
    }

    /// <summary>
    /// Safely deserializes JSON string to object
    /// </summary>
    public static T? SafeDeserialize<T>(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(json);
            return result;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// Pretty prints JSON with indentation
    /// </summary>
    public static string PrettyPrint(string json)
    {
        try
        {
            var element = JsonDocument.Parse(json);

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true
            };

            JsonSerializerOptions options = jsonSerializerOptions;

            var result = JsonSerializer.Serialize(element, options: options);
            return result;
        }
        catch (Exception)
        {
            return json;
        }
    }
}
