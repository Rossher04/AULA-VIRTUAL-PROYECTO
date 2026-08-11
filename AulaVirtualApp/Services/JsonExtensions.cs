using System.Text.Json.Nodes;

namespace AulaVirtualApp.Services;

public static class JsonExtensions
{
    public static string GetStr(this JsonObject obj, string key, string fallback = "")
    {
        if (obj == null) return fallback;
        if (obj.TryGetPropertyValue(key, out var value) && value != null)
            return value.ToString();
        return fallback;
    }

    public static int GetIntOrZero(this JsonObject obj, string key)
    {
        if (obj != null && obj.TryGetPropertyValue(key, out var value) && value != null)
        {
            if (value is JsonValue jv && jv.TryGetValue<int>(out var i))
                return i;
            if (int.TryParse(value.ToString(), out var parsed))
                return parsed;
        }
        return 0;
    }

    public static JsonArray AsJsonArray(this JsonNode? node)
    {
        return node as JsonArray ?? new JsonArray();
    }
}
