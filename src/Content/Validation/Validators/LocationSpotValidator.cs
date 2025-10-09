using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates Venue spot JSON content files.
/// </summary>
public class LocationSpotValidator : IContentValidator
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
                    "Location spots file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> spotIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement spotElement in root.EnumerateArray())
            {
                ValidateLocationSpot(spotElement, index, fileName, errors, spotIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate Venue spots: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateLocationSpot(JsonElement spot, int index, string fileName, List<ValidationError> errors, HashSet<string> spotIds)
    {
        string spotId = GetStringProperty(spot, "id") ?? $"LocationSpot[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(spotId) && spotId != $"LocationSpot[{index}]")
        {
            if (!spotIds.Add(spotId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{spotId}",
                    $"Duplicate Venue spot ID: {spotId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!spot.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{spotId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Type field removed - no longer needed

        // Validate CurrentTimeBlocks array (capital C to match JSON)
        if (spot.TryGetProperty("CurrentTimeBlocks", out JsonElement timeBlocks) &&
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
                            $"{fileName}:{spotId}",
                            $"Invalid time block: '{timeStr}'",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }

        // Validate domainTags array
        if (spot.TryGetProperty("domainTags", out JsonElement domainTags) &&
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
                            $"{fileName}:{spotId}",
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