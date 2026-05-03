using System.Text.Json;

namespace VRChatTools.Common;

public static class JsonHelpers
{
    public static string GetRequiredStringProperty(JsonElement element, string propertyName)
    {
        var value = GetOptionalStringProperty(element, propertyName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"JSON property was not found: {propertyName}");
        }

        return value;
    }

    public static string? GetOptionalStringProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
    }

    public static string FormatJsonIfPossible(string json)
    {
        return json;
    }
}
