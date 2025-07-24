using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates location JSON content files.
/// </summary>
public class LocationValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "name", "description", "connectedTo", "locationSpots"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("locations.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_locations.json", StringComparison.OrdinalIgnoreCase);
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
                    "Locations file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> locationIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement locationElement in root.EnumerateArray())
            {
                ValidateLocation(locationElement, index, fileName, errors, locationIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate locations: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateLocation(JsonElement location, int index, string fileName, List<ValidationError> errors, HashSet<string> locationIds)
    {
        string locationId = GetStringProperty(location, "id") ?? $"Location[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(locationId) && locationId != $"Location[{index}]")
        {
            if (!locationIds.Add(locationId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{locationId}",
                    $"Duplicate location ID: {locationId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!location.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{locationId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate connectedTo array
        if (location.TryGetProperty("connectedTo", out JsonElement connectedTo))
        {
            if (connectedTo.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{locationId}",
                    "connectedTo must be an array",
                    ValidationSeverity.Critical));
            }
        }

        // Validate locationSpots array
        if (location.TryGetProperty("locationSpots", out JsonElement locationSpots))
        {
            if (locationSpots.ValueKind != JsonValueKind.Array)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{locationId}",
                    "locationSpots must be an array",
                    ValidationSeverity.Critical));
            }
        }

        // Validate availableProfessionsByTime
        if (location.TryGetProperty("availableProfessionsByTime", out JsonElement professions) &&
            professions.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty timeBlock in professions.EnumerateObject())
            {
                // Validate time block
                if (!EnumParser.TryParse<TimeBlocks>(timeBlock.Name, out _))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{locationId}",
                        $"Invalid time block: '{timeBlock.Name}'",
                        ValidationSeverity.Warning));
                }

                // Validate professions array
                if (timeBlock.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement prof in timeBlock.Value.EnumerateArray())
                    {
                        if (prof.ValueKind == JsonValueKind.String)
                        {
                            string? profStr = prof.GetString();
                            if (!string.IsNullOrEmpty(profStr) &&
                                !EnumParser.TryParse<Professions>(profStr, out _))
                            {
                                errors.Add(new ValidationError(
                                    $"{fileName}:{locationId}",
                                    $"Invalid profession: '{profStr}'",
                                    ValidationSeverity.Warning));
                            }
                        }
                    }
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