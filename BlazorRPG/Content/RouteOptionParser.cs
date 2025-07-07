using System.Text.Json;

public static class RouteOptionParser
{
    public static RouteOption ParseRouteOption(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        RouteOption route = new RouteOption
        {
            Id = GetStringProperty(root, "id", ""),
            Name = GetStringProperty(root, "name", ""),
            Origin = GetStringProperty(root, "origin", ""),
            Destination = GetStringProperty(root, "destination", ""),
            BaseCoinCost = GetIntProperty(root, "baseCoinCost", 0),
            BaseStaminaCost = GetIntProperty(root, "baseStaminaCost", 1),
            TimeBlockCost = GetIntProperty(root, "timeBlockCost", 1),
            IsDiscovered = GetBoolProperty(root, "isDiscovered", true),
            MaxItemCapacity = GetIntProperty(root, "maxItemCapacity", 3),
            Description = GetStringProperty(root, "description", "")
        };

        // Parse method
        string methodStr = GetStringProperty(root, "method", "Walking");
        if (Enum.TryParse<TravelMethods>(methodStr, true, out TravelMethods method))
        {
            route.Method = method;
        }
        else
        {
            route.Method = TravelMethods.Walking;
        }

        // Parse departure time
        string departureTimeStr = GetStringProperty(root, "departureTime", null);
        if (!string.IsNullOrEmpty(departureTimeStr))
        {
            if (Enum.TryParse<TimeBlocks>(departureTimeStr, true, out TimeBlocks depTime))
            {
                route.DepartureTime = depTime;
            }
        }

        // Parse required route types
        route.RequiredRouteTypes = GetStringArray(root, "requiredRouteTypes");

        return route;
    }

    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString() ?? defaultValue;
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
        return defaultValue;
    }

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
        }
        return defaultValue;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False))
        {
            return property.GetBoolean();
        }
        return defaultValue;
    }

    private static List<string> GetStringArray(JsonElement element, string propertyName)
    {
        List<string> results = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string value = item.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(value);
                    }
                }
            }
        }

        return results;
    }
}
