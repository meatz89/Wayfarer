using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates NPC JSON content files.
/// </summary>
public class NPCValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "name", "profession", "locationId"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("npcs.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_npcs.json", StringComparison.OrdinalIgnoreCase);
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
                    "NPCs file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            int index = 0;
            foreach (JsonElement npcElement in root.EnumerateArray())
            {
                ValidateNPC(npcElement, index, fileName, errors);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate NPCs: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateNPC(JsonElement npc, int index, string fileName, List<ValidationError> errors)
    {
        string npcId = GetStringProperty(npc, "id") ?? $"NPC[{index}]";

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!npc.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{npcId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate profession
        if (npc.TryGetProperty("profession", out JsonElement profession) &&
            profession.ValueKind == JsonValueKind.String)
        {
            string? profStr = profession.GetString();
            if (!string.IsNullOrEmpty(profStr) &&
                !EnumParser.TryParse<Professions>(profStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{npcId}",
                    $"Invalid profession: '{profStr}'",
                    ValidationSeverity.Critical));
            }
        }

        // Validate services
        if (npc.TryGetProperty("services", out JsonElement services) &&
            services.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement service in services.EnumerateArray())
            {
                if (service.ValueKind == JsonValueKind.String)
                {
                    string? serviceStr = service.GetString();
                    if (!string.IsNullOrEmpty(serviceStr) &&
                        !EnumParser.TryParse<ServiceTypes>(serviceStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{npcId}",
                            $"Invalid service type: '{serviceStr}'",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }

        // Validate token types
        if (npc.TryGetProperty("tokenTypes", out JsonElement tokenTypes) &&
            tokenTypes.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement tokenType in tokenTypes.EnumerateArray())
            {
                if (tokenType.ValueKind == JsonValueKind.String)
                {
                    string? tokenStr = tokenType.GetString();
                    if (!string.IsNullOrEmpty(tokenStr) &&
                        !EnumParser.TryParse<ConnectionType>(tokenStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{npcId}",
                            $"Invalid token type: '{tokenStr}'",
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