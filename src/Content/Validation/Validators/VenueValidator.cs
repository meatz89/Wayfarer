using System.Text.Json;

/// <summary>
/// Validates Venue JSON content files.
/// </summary>
public class VenueValidator : IContentValidator
{
private readonly List<string> _requiredFields = new List<string>
    {
        "id", "name", "description", "connectedTo", "locations"
    };

public bool CanValidate(string fileName)
{
    return fileName.Equals("locations.json", StringComparison.OrdinalIgnoreCase) ||
           fileName.EndsWith("_locations.json", StringComparison.OrdinalIgnoreCase);
}

public IEnumerable<ValidationError> Validate(string content, string fileName)
{
    List<ValidationError> errors = new List<ValidationError>();

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

    List<string> venueIds = new List<string>();
    int index = 0;

    foreach (JsonElement locationElement in root.EnumerateArray())
    {
        ValidateLocation(locationElement, index, fileName, errors, venueIds);
        index++;
    }

    return errors;
}

private void ValidateLocation(JsonElement location, int index, string fileName, List<ValidationError> errors, List<string> venueIds)
{
    // Use index as fallback identifier if id field is missing (for error reporting only)
    string venueId = GetStringProperty(location, "id") ?? $"Location[{index}]";

    // Check for duplicate IDs
    if (!string.IsNullOrEmpty(venueId) && venueId != $"Location[{index}]")
    {
        if (venueIds.Contains(venueId))
        {
            errors.Add(new ValidationError(
                $"{fileName}:{venueId}",
                $"Duplicate Venue ID: {venueId}",
                ValidationSeverity.Critical));
        }
        else
        {
            venueIds.Add(venueId);
        }
    }

    // Check required fields
    foreach (string field in _requiredFields)
    {
        if (!location.TryGetProperty(field, out _))
        {
            errors.Add(new ValidationError(
                $"{fileName}:{venueId}",
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
                $"{fileName}:{venueId}",
                "connectedTo must be an array",
                ValidationSeverity.Critical));
        }
    }

    // Validate locations array
    if (location.TryGetProperty("locations", out JsonElement locations))
    {
        if (locations.ValueKind != JsonValueKind.Array)
        {
            errors.Add(new ValidationError(
                $"{fileName}:{venueId}",
                "locations must be an array",
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
                    $"{fileName}:{venueId}",
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
                                $"{fileName}:{venueId}",
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