using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates letter template JSON content files.
/// </summary>
public class LetterTemplateValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "description", "tokenType", "minDeadlineInSegments", "maxDeadlineInSegments",
            "minPayment", "maxPayment", "possibleSenders", "possibleRecipients"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("letter_templates.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_letter_templates.json", StringComparison.OrdinalIgnoreCase);
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
                    "DeliveryObligation templates file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> templateIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement templateElement in root.EnumerateArray())
            {
                ValidateLetterTemplate(templateElement, index, fileName, errors, templateIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate letter templates: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateLetterTemplate(JsonElement template, int index, string fileName, List<ValidationError> errors, HashSet<string> templateIds)
    {
        string templateId = GetStringProperty(template, "id") ?? $"LetterTemplate[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(templateId) && templateId != $"LetterTemplate[{index}]")
        {
            if (!templateIds.Add(templateId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{templateId}",
                    $"Duplicate letter template ID: {templateId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!template.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{templateId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate tokenType
        if (template.TryGetProperty("tokenType", out JsonElement tokenType) &&
            tokenType.ValueKind == JsonValueKind.String)
        {
            string? tokenStr = tokenType.GetString();
            if (!string.IsNullOrEmpty(tokenStr) &&
                !EnumParser.TryParse<ConnectionType>(tokenStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{templateId}",
                    $"Invalid token type: '{tokenStr}'",
                    ValidationSeverity.Critical));
            }
        }

        // Validate category
        if (template.TryGetProperty("category", out JsonElement category) &&
            category.ValueKind == JsonValueKind.String)
        {
            string? categoryStr = category.GetString();
            if (!string.IsNullOrEmpty(categoryStr) &&
                !EnumParser.TryParse<LetterCategory>(categoryStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{templateId}",
                    $"Invalid letter category: '{categoryStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate size
        if (template.TryGetProperty("size", out JsonElement size) &&
            size.ValueKind == JsonValueKind.String)
        {
            string? sizeStr = size.GetString();
            if (!string.IsNullOrEmpty(sizeStr) &&
                !EnumParser.TryParse<SizeCategory>(sizeStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{templateId}",
                    $"Invalid size category: '{sizeStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate physical properties
        if (template.TryGetProperty("physicalProperties", out JsonElement physicalProps) &&
            physicalProps.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement prop in physicalProps.EnumerateArray())
            {
                if (prop.ValueKind == JsonValueKind.String)
                {
                    string? propStr = prop.GetString();
                    if (!string.IsNullOrEmpty(propStr) &&
                        !EnumParser.TryParse<LetterPhysicalProperties>(propStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{templateId}",
                            $"Invalid physical property: '{propStr}'",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }

        // Validate requiredEquipment
        if (template.TryGetProperty("requiredEquipment", out JsonElement equipment) &&
            equipment.ValueKind == JsonValueKind.String)
        {
            string? equipStr = equipment.GetString();
            if (!string.IsNullOrEmpty(equipStr) &&
                !EnumParser.TryParse<ItemCategory>(equipStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{templateId}",
                    $"Invalid equipment category: '{equipStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate numeric ranges
        ValidateNumericRange(template, templateId, fileName, errors, "minDeadlineInSegments", "maxDeadlineInSegments", 2, 1440); // Min 2 segments, max 60 days (1440 segments at 24 per day)
        ValidateNumericRange(template, templateId, fileName, errors, "minPayment", "maxPayment", 0, 10000);
    }

    private void ValidateNumericRange(JsonElement element, string id, string fileName, List<ValidationError> errors,
        string minField, string maxField, int minValue, int maxValue)
    {
        if (element.TryGetProperty(minField, out JsonElement minProp) &&
            element.TryGetProperty(maxField, out JsonElement maxProp) &&
            minProp.ValueKind == JsonValueKind.Number &&
            maxProp.ValueKind == JsonValueKind.Number)
        {
            int min = minProp.GetInt32();
            int max = maxProp.GetInt32();

            if (min > max)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{id}",
                    $"{minField} ({min}) cannot be greater than {maxField} ({max})",
                    ValidationSeverity.Critical));
            }

            if (min < minValue || max > maxValue)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{id}",
                    $"{minField}/{maxField} must be between {minValue} and {maxValue}",
                    ValidationSeverity.Warning));
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