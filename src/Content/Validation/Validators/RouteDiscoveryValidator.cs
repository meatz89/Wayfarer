using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates route discovery JSON content files.
/// </summary>
public class RouteDiscoveryValidator : IContentValidator
{
    private readonly List<string> _requiredFields = new List<string>
        {
            "routeId", "knownByNPCs"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("route_discovery.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_route_discovery.json", StringComparison.OrdinalIgnoreCase);
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
                "Route discovery file must contain a JSON array",
                ValidationSeverity.Critical));
            return errors;
        }

        List<string> routeIds = new List<string>();
        int index = 0;

        foreach (JsonElement discoveryElement in root.EnumerateArray())
        {
            ValidateRouteDiscovery(discoveryElement, index, fileName, errors, routeIds);
            index++;
        }

        return errors;
    }

    private void ValidateRouteDiscovery(JsonElement discovery, int index, string fileName, List<ValidationError> errors, List<string> routeIds)
    {
        string routeId = GetStringProperty(discovery, "routeId") ?? $"RouteDiscovery[{index}]";

        // Check for duplicate route IDs
        if (!string.IsNullOrEmpty(routeId) && routeId != $"RouteDiscovery[{index}]")
        {
            if (routeIds.Contains(routeId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:route-{routeId}",
                    $"Duplicate route discovery for route: {routeId}",
                    ValidationSeverity.Warning));
            }
            else
            {
                routeIds.Add(routeId);
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!discovery.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:route-{routeId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate knownByNPCs array
        if (discovery.TryGetProperty("knownByNPCs", out JsonElement npcs) &&
            npcs.ValueKind == JsonValueKind.Array)
        {
            if (npcs.GetArrayLength() == 0)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:route-{routeId}",
                    "knownByNPCs array cannot be empty",
                    ValidationSeverity.Warning));
            }
        }

        // Validate discoveryContexts
        if (discovery.TryGetProperty("discoveryContexts", out JsonElement contexts) &&
            contexts.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty context in contexts.EnumerateObject())
            {
                ValidateDiscoveryContext(context.Value, routeId, context.Name, fileName, errors);
            }
        }

        // Validate requiredTokensWithNPC
        if (discovery.TryGetProperty("requiredTokensWithNPC", out JsonElement tokenReqs) &&
            tokenReqs.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty req in tokenReqs.EnumerateObject())
            {
                if (req.Value.ValueKind == JsonValueKind.Number)
                {
                    int tokens = req.Value.GetInt32();
                    if (tokens < 0)
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:route-{routeId}",
                            $"Required tokens for NPC '{req.Name}' must be non-negative (got {tokens})",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }
    }

    private void ValidateDiscoveryContext(JsonElement context, string routeId, string npcId, string fileName, List<ValidationError> errors)
    {
        string contextId = $"route-{routeId}:npc-{npcId}";

        // Validate requiredEquipment array
        if (context.TryGetProperty("requiredEquipment", out JsonElement equipment) &&
            equipment.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement equip in equipment.EnumerateArray())
            {
                if (equip.ValueKind == JsonValueKind.String)
                {
                    string? equipStr = equip.GetString();
                    if (!string.IsNullOrEmpty(equipStr) &&
                        !EnumParser.TryParse<ItemCategory>(equipStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{contextId}",
                            $"Invalid equipment category: '{equipStr}'",
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