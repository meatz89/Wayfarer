using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates route JSON content files.
/// </summary>
public class RouteValidator : BaseValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "origin", "destination", "method", "travelTimeMinutes", "baseCoinCost", "baseStaminaCost"
        };

    public override bool CanValidate(string fileName)
    {
        return fileName.Equals("routes.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_routes.json", StringComparison.OrdinalIgnoreCase);
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
                    "Routes file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            int index = 0;
            HashSet<string> routeIds = new HashSet<string>();

            foreach (JsonElement routeElement in root.EnumerateArray())
            {
                ValidateRoute(routeElement, index, fileName, errors, routeIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate routes: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateRoute(JsonElement route, int index, string fileName,
        List<ValidationError> errors, HashSet<string> routeIds)
    {
        string routeId = GetStringProperty(route, "id") ?? $"Route[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(routeId) && !routeIds.Add(routeId))
        {
            errors.Add(new ValidationError(
                $"{fileName}:{routeId}",
                $"Duplicate route ID: {routeId}",
                ValidationSeverity.Critical));
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

        // Validate numeric fields
        ValidateNumericField(route, "travelTimeMinutes", routeId, fileName, errors, min: 1);
        ValidateNumericField(route, "baseCoinCost", routeId, fileName, errors, min: 0);
        ValidateNumericField(route, "baseStaminaCost", routeId, fileName, errors, min: 0);

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
        string fromId = GetStringProperty(route, "Origin");
        string toId = GetStringProperty(route, "Destination");

        if (!string.IsNullOrEmpty(fromId) && !string.IsNullOrEmpty(toId) && fromId == toId)
        {
            errors.Add(new ValidationError(
                $"{fileName}:{routeId}",
                "Route cannot have the same origin and destination location",
                ValidationSeverity.Critical));
        }
    }

}