using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates route JSON content files.
/// </summary>
public class RouteValidator : BaseValidator
{
    private readonly List<string> _requiredFields = new List<string>
        {
            "id", "originLocationSpot", "destinationLocationSpot", "method", "travelTimeSegments", "baseCoinCost", "baseStaminaCost"
        };

    public override bool CanValidate(string fileName)
    {
        return fileName.Equals("routes.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_routes.json", StringComparison.OrdinalIgnoreCase);
    }

    public override IEnumerable<ValidationError> Validate(string content, string fileName)
    {
        List<ValidationError> errors = new List<ValidationError>();

        using JsonDocument doc = JsonDocument.Parse(content);
        JsonElement root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
        {
            errors.Add(new ValidationError(
                fileName,
                "Routes file must contain a JSON array",
                ValidationSeverity.Critical));
            return errors;
        }

        int index = 0;
        List<string> routeIds = new List<string>();

        foreach (JsonElement routeElement in root.EnumerateArray())
        {
            ValidateRoute(routeElement, index, fileName, errors, routeIds);
            index++;
        }

        return errors;
    }

    private void ValidateRoute(JsonElement route, int index, string fileName,
        List<ValidationError> errors, List<string> routeIds)
    {
        // Use index as fallback identifier if id field is missing (for error reporting only)
        string routeId = GetStringProperty(route, "id") ?? $"Route[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(routeId))
        {
            if (routeIds.Contains(routeId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{routeId}",
                    $"Duplicate route ID: {routeId}",
                    ValidationSeverity.Critical));
            }
            else
            {
                routeIds.Add(routeId);
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!TryGetPropertyCaseInsensitive(route, field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{routeId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate travel method
        if (TryGetPropertyCaseInsensitive(route, "method", out JsonElement method) &&
            method.ValueKind == JsonValueKind.String)
        {
            string? methodStr = method.GetString();
            if (!string.IsNullOrEmpty(methodStr) &&
                !EnumParser.TryParse<TravelMethods>(methodStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{routeId}",
                    $"Invalid travel method: '{methodStr}'",
                    ValidationSeverity.Critical));
            }
        }

        // Validate departure time (single value in DTO)
        if (TryGetPropertyCaseInsensitive(route, "departureTime", out JsonElement departureTime) &&
            departureTime.ValueKind == JsonValueKind.String)
        {
            string? timeStr = departureTime.GetString();
            if (!string.IsNullOrEmpty(timeStr) &&
                !EnumParser.TryParse<TimeBlocks>(timeStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{routeId}",
                    $"Invalid departure time: '{timeStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate terrain categories (RouteDTO uses TerrainCategories)
        if (TryGetPropertyCaseInsensitive(route, "terrainCategories", out JsonElement terrainCategories) &&
            terrainCategories.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement terrain in terrainCategories.EnumerateArray())
            {
                if (terrain.ValueKind == JsonValueKind.String)
                {
                    string? terrainStr = terrain.GetString();
                    if (!string.IsNullOrEmpty(terrainStr) &&
                        !EnumParser.TryParse<TerrainCategory>(terrainStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{routeId}",
                            $"Invalid terrain category: '{terrainStr}'",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }

        // Validate route consistency
        string fromId = GetStringProperty(route, "originLocationSpot");
        string toId = GetStringProperty(route, "destinationLocationSpot");

        if (!string.IsNullOrEmpty(fromId) && !string.IsNullOrEmpty(toId) && fromId == toId)
        {
            errors.Add(new ValidationError(
                $"{fileName}:{routeId}",
                "Route cannot have the same origin and destination location",
                ValidationSeverity.Critical));
        }
    }

}