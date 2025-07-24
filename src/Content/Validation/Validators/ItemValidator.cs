using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Validates item JSON content files.
/// </summary>
public class ItemValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "name", "weight", "buyPrice", "sellPrice", "inventorySlots"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("items.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_items.json", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<ValidationError> Validate(string content, string fileName)
    {
        List<ValidationError> errors = new List<ValidationError>();

        try
        {
            using JsonDocument doc = JsonDocument.Parse(content);
            JsonElement root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new ValidationError(
                    fileName,
                    "Items file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            int index = 0;
            foreach (JsonElement itemElement in root.EnumerateArray())
            {
                ValidateItem(itemElement, index, fileName, errors);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate items: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateItem(JsonElement item, int index, string fileName, List<ValidationError> errors)
    {
        string itemId = GetStringProperty(item, "id") ?? $"Item[{index}]";

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!item.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{itemId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate numeric fields
        ValidateNumericField(item, "weight", itemId, fileName, errors, min: 0);
        ValidateNumericField(item, "buyPrice", itemId, fileName, errors, min: 0);
        ValidateNumericField(item, "sellPrice", itemId, fileName, errors, min: 0);
        ValidateNumericField(item, "inventorySlots", itemId, fileName, errors, min: 1);

        // Validate categories
        if (item.TryGetProperty("categories", out JsonElement categories) ||
            item.TryGetProperty("itemCategories", out categories))
        {
            if (categories.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement cat in categories.EnumerateArray())
                {
                    if (cat.ValueKind == JsonValueKind.String)
                    {
                        string? catStr = cat.GetString();
                        if (!string.IsNullOrEmpty(catStr) &&
                            !EnumParser.TryParse<ItemCategory>(catStr, out _))
                        {
                            errors.Add(new ValidationError(
                                $"{fileName}:{itemId}",
                                $"Invalid item category: '{catStr}'",
                                ValidationSeverity.Warning));
                        }
                    }
                }
            }
        }

        // Validate size
        if (item.TryGetProperty("size", out JsonElement size) &&
            size.ValueKind == JsonValueKind.String)
        {
            string? sizeStr = size.GetString();
            if (!string.IsNullOrEmpty(sizeStr) &&
                !EnumParser.TryParse<SizeCategory>(sizeStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{itemId}",
                    $"Invalid size category: '{sizeStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate price consistency
        if (item.TryGetProperty("buyPrice", out JsonElement buyPrice) &&
            item.TryGetProperty("sellPrice", out JsonElement sellPrice))
        {
            if (buyPrice.TryGetInt32(out int buy) && sellPrice.TryGetInt32(out int sell))
            {
                if (buy > sell * 2)
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{itemId}",
                        $"Buy price ({buy}) is more than double sell price ({sell})",
                        ValidationSeverity.Warning));
                }
            }
        }
    }

    private void ValidateNumericField(JsonElement item, string fieldName, string itemId,
        string fileName, List<ValidationError> errors, int? min = null, int? max = null)
    {
        if (item.TryGetProperty(fieldName, out JsonElement field))
        {
            if (field.ValueKind != JsonValueKind.Number || !field.TryGetInt32(out int value))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{itemId}",
                    $"Field '{fieldName}' must be a valid integer",
                    ValidationSeverity.Critical));
            }
            else
            {
                if (min.HasValue && value < min.Value)
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{itemId}",
                        $"Field '{fieldName}' value {value} is below minimum {min.Value}",
                        ValidationSeverity.Critical));
                }
                if (max.HasValue && value > max.Value)
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{itemId}",
                        $"Field '{fieldName}' value {value} is above maximum {max.Value}",
                        ValidationSeverity.Critical));
                }
            }
        }
    }

    private string GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }
        return null;
    }
}