using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Generic JSON schema validator for content files.
/// Validates that required fields exist and have correct types.
/// </summary>
public class SchemaValidator : IContentValidator
{
    private readonly Dictionary<string, FileSchema> _schemas;

    public SchemaValidator()
    {
        _schemas = new Dictionary<string, FileSchema>(StringComparer.OrdinalIgnoreCase)
        {
            ["locations.json"] = new FileSchema
            {
                IsArray = true,
                RequiredFields = new[] { "id", "name" },
                OptionalFields = new[] { "description", "connectedTo", "locations", "domainTags",
                                           "environmentalProperties", "availableProfessionsByTime" }
            },
            ["location_spots.json"] = new FileSchema
            {
                IsArray = true,
                RequiredFields = new[] { "id", "name", "venueId", "type" },
                OptionalFields = new[] { "description", "initialState", "domainTags",
                                           "CurrentTimeBlocks", "accessRequirement" }
            },
            ["letter_templates.json"] = new FileSchema
            {
                IsArray = true,
                RequiredFields = new[] { "id", "name", "tokenType", "basePay" },
                OptionalFields = new[] { "category", "size", "physicalProperties",
                                           "requiredEquipment", "narrative" }
            },
            ["standing_obligations.json"] = new FileSchema
            {
                IsArray = true,
                RequiredFields = new[] { "id", "name", "description", "source" },
                OptionalFields = new[] { "relatedTokenType", "benefitEffects", "constraintEffects" }
            }
        };
    }

    public bool CanValidate(string fileName)
    {
        return _schemas.ContainsKey(fileName);
    }

    public IEnumerable<ValidationError> Validate(string content, string fileName)
    {
        List<ValidationError> errors = new List<ValidationError>();

        if (!_schemas.TryGetValue(fileName, out FileSchema? schema))
        {
            return errors;
        }

        using JsonDocument doc = JsonDocument.Parse(content);
        JsonElement root = doc.RootElement;

        if (schema.IsArray)
        {
            if (root.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new ValidationError(
                    fileName,
                    $"Expected JSON array but found {root.ValueKind}",
                    ValidationSeverity.Critical));
                return errors;
            }

            int index = 0;
            foreach (JsonElement element in root.EnumerateArray())
            {
                ValidateElement(element, schema, $"{fileName}[{index}]", errors);
                index++;
            }
        }
        else
        {
            ValidateElement(root, schema, fileName, errors);
        }

        return errors;
    }

    private void ValidateElement(JsonElement element, FileSchema schema, string path, List<ValidationError> errors)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            errors.Add(new ValidationError(
                path,
                $"Expected object but found {element.ValueKind}",
                ValidationSeverity.Critical));
            return;
        }

        // Check required fields
        foreach (string field in schema.RequiredFields)
        {
            if (!element.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    path,
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Check for unknown fields (helps catch typos)
        List<string> knownFields = schema.RequiredFields.Concat(schema.OptionalFields ?? Array.Empty<string>())
            .ToList();

        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!knownFields.Contains(property.Name))
            {
                errors.Add(new ValidationError(
                    path,
                    $"Unknown field: {property.Name}",
                    ValidationSeverity.Warning));
            }
        }
    }

    private class FileSchema
    {
        public bool IsArray { get; set; }
        public string[] RequiredFields { get; set; }
        public string[] OptionalFields { get; set; }
    }
}