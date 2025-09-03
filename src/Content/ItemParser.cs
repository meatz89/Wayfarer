using System;
using System.Collections.Generic;
using System.Text.Json;

public static class ItemParser
{
    /// <summary>
    /// Convert an ItemDTO to an Item domain model
    /// </summary>
    public static Item ConvertDTOToItem(ItemDTO dto)
    {
        Item item = new Item
        {
            Id = dto.Id ?? "",
            Name = dto.Name ?? "",
            Focus = dto.Focus,
            BuyPrice = dto.BuyPrice,
            SellPrice = dto.SellPrice,
            InventorySlots = dto.InventorySlots,
            Description = dto.Description ?? ""
        };

        // Parse item categories
        if (dto.Categories != null)
        {
            foreach (string categoryStr in dto.Categories)
            {
                if (EnumParser.TryParse<ItemCategory>(categoryStr, out ItemCategory category))
                {
                    item.Categories.Add(category);
                }
            }
        }

        // Parse enhanced categorical properties
        if (!string.IsNullOrEmpty(dto.SizeCategory))
        {
            if (EnumParser.TryParse<SizeCategory>(dto.SizeCategory, out SizeCategory size))
            {
                item.Size = size;
            }
        }

        // Parse token generation modifiers
        if (dto.TokenGenerationModifiers != null)
        {
            foreach (var kvp in dto.TokenGenerationModifiers)
            {
                if (Enum.TryParse<ConnectionType>(kvp.Key, out var connectionType))
                {
                    item.TokenGenerationModifiers[connectionType] = kvp.Value;
                }
            }
        }

        // Parse enabled token generation
        if (dto.EnablesTokenGeneration != null)
        {
            foreach (var tokenType in dto.EnablesTokenGeneration)
            {
                if (Enum.TryParse<ConnectionType>(tokenType, out var connectionType))
                {
                    item.EnablesTokenGeneration.Add(connectionType);
                }
            }
        }

        return item;
    }
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
            Focus = GetIntProperty(root, "focus", 1),
            BuyPrice = GetIntProperty(root, "buyPrice", 0),
            SellPrice = GetIntProperty(root, "sellPrice", 0),
            InventorySlots = GetIntProperty(root, "inventorySlots", 1),
            LocationId = GetStringProperty(root, "locationId", ""),
            SpotId = GetStringProperty(root, "spotId", ""),
            Description = GetStringProperty(root, "description", ""),
        };

        // Parse item categories
        List<string> categoryStrings = GetStringArray(root, "categories");
        foreach (string categoryStr in categoryStrings)
        {
            if (EnumParser.TryParse<ItemCategory>(categoryStr, out ItemCategory category))
            {
                item.Categories.Add(category);
            }
        }


        // Parse enhanced categorical properties
        string sizeStr = GetStringProperty(root, "size", "Medium");
        if (EnumParser.TryParse<SizeCategory>(sizeStr, out SizeCategory size))
        {
            item.Size = size;
        }

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