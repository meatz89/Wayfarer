using System.Text.Json;

public static class ItemParser
{
    public static Item ParseItem(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        Item item = new Item
        {
            Id = GetStringProperty(root, "id", ""),
            Name = GetStringProperty(root, "name", ""),
            Weight = GetIntProperty(root, "weight", 1),
            BuyPrice = GetIntProperty(root, "buyPrice", 0),
            SellPrice = GetIntProperty(root, "sellPrice", 0),
            InventorySlots = GetIntProperty(root, "inventorySlots", 1),
            IsContraband = GetBoolProperty(root, "isContraband", false),
            LocationId = GetStringProperty(root, "locationId", ""),
            SpotId = GetStringProperty(root, "spotId", ""),
            Description = GetStringProperty(root, "description", ""),
        };

        // Parse enabled route types
        item.EnabledRouteTypes = GetStringArray(root, "enabledRouteTypes");

        return item;
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