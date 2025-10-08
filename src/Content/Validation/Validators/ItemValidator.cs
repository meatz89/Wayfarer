using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Validates item JSON content files.
/// </summary>
public class ItemValidator : BaseValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "name", "focus", "buyPrice", "sellPrice", "inventorySlots"
        };

    public override bool CanValidate(string fileName)
    {
        return fileName.Equals("items.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_items.json", StringComparison.OrdinalIgnoreCase);
    }

    public override IEnumerable<ValidationError> Validate(string content, string fileName)
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
            if (!TryGetPropertyCaseInsensitive(item, field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{itemId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate categories
        if (TryGetPropertyCaseInsensitive(item, "categories", out JsonElement categories) ||
            TryGetPropertyCaseInsensitive(item, "itemCategories", out categories))
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
        if (TryGetPropertyCaseInsensitive(item, "size", out JsonElement size) &&
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
        if (TryGetPropertyCaseInsensitive(item, "buyPrice", out JsonElement buyPrice) &&
            TryGetPropertyCaseInsensitive(item, "sellPrice", out JsonElement sellPrice))
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

    // ValidateNumericField is now inherited from BaseValidator

    // GetStringProperty is now inherited from BaseValidator
}