using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates Venue location JSON content files.
/// </summary>
public class LocationValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "name", "venueId"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("location_spots.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_location_spots.json", StringComparison.OrdinalIgnoreCase);
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
                    "Location Locations file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> spotIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement spotElement in root.EnumerateArray())
            {
                ValidateLocation(spotElement, index, fileName, errors, spotIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate Venue Locations: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateLocation(JsonElement location, int index, string fileName, List<ValidationError> errors, HashSet<string> spotIds)
    {
        string LocationId = GetStringProperty(location, "id") ?? $"Location[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(LocationId) && LocationId != $"Location[{index}]")
        {
            if (!spotIds.Add(LocationId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{LocationId}",
                    $"Duplicate Venue location ID: {LocationId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!location.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{LocationId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Type field removed - no longer needed

        // Validate CurrentTimeBlocks array (capital C to match JSON)
        if (location.TryGetProperty("CurrentTimeBlocks", out JsonElement timeBlocks) &&
            timeBlocks.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement timeBlock in timeBlocks.EnumerateArray())
            {
                if (timeBlock.ValueKind == JsonValueKind.String)
                {
                    string? timeStr = timeBlock.GetString();
                    if (!string.IsNullOrEmpty(timeStr) &&
                        !EnumParser.TryParse<TimeBlocks>(timeStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{LocationId}",
                            $"Invalid time block: '{timeStr}'",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }

        // Validate domainTags array
        if (location.TryGetProperty("domainTags", out JsonElement domainTags) &&
            domainTags.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement tag in domainTags.EnumerateArray())
            {
                if (tag.ValueKind == JsonValueKind.String)
                {
                    string? tagStr = tag.GetString();
                    if (!string.IsNullOrEmpty(tagStr) &&
                        !EnumParser.TryParse<DomainTag>(tagStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{LocationId}",
                            $"Invalid domain tag: '{tagStr}'",
                            ValidationSeverity.Warning));
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