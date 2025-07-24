using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates token favor JSON content files.
/// </summary>
public class TokenFavorValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "npcId", "name", "favorType", "tokenCost"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("token_favors.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_token_favors.json", StringComparison.OrdinalIgnoreCase);
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
                    "Token favors file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> favorIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement favorElement in root.EnumerateArray())
            {
                ValidateTokenFavor(favorElement, index, fileName, errors, favorIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate token favors: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateTokenFavor(JsonElement favor, int index, string fileName, List<ValidationError> errors, HashSet<string> favorIds)
    {
        string favorId = GetStringProperty(favor, "id") ?? $"TokenFavor[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(favorId) && favorId != $"TokenFavor[{index}]")
        {
            if (!favorIds.Add(favorId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{favorId}",
                    $"Duplicate token favor ID: {favorId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!favor.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{favorId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate favorType
        if (favor.TryGetProperty("favorType", out JsonElement favorType) &&
            favorType.ValueKind == JsonValueKind.String)
        {
            string? typeStr = favorType.GetString();
            if (!string.IsNullOrEmpty(typeStr) &&
                !EnumParser.TryParse<TokenFavorType>(typeStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{favorId}",
                    $"Invalid favor type: '{typeStr}'",
                    ValidationSeverity.Critical));
            }
        }

        // Validate tokenCost
        if (favor.TryGetProperty("tokenCost", out JsonElement tokenCost) &&
            tokenCost.ValueKind == JsonValueKind.Number)
        {
            int cost = tokenCost.GetInt32();
            if (cost < 0)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{favorId}",
                    $"Token cost must be non-negative (got {cost})",
                    ValidationSeverity.Warning));
            }
        }

        // Validate minimumRelationshipLevel
        if (favor.TryGetProperty("minimumRelationshipLevel", out JsonElement minLevel) &&
            minLevel.ValueKind == JsonValueKind.Number)
        {
            int level = minLevel.GetInt32();
            if (level < 0)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{favorId}",
                    $"Minimum relationship level must be non-negative (got {level})",
                    ValidationSeverity.Warning));
            }
        }

        // Validate isOneTime boolean
        if (favor.TryGetProperty("isOneTime", out JsonElement isOneTime) &&
            isOneTime.ValueKind != JsonValueKind.True &&
            isOneTime.ValueKind != JsonValueKind.False)
        {
            errors.Add(new ValidationError(
                $"{fileName}:{favorId}",
                "isOneTime must be a boolean value",
                ValidationSeverity.Warning));
        }

        // Validate based on favor type - grantsId should exist for most types
        if (favor.TryGetProperty("favorType", out JsonElement typeElement) &&
            typeElement.ValueKind == JsonValueKind.String)
        {
            string? typeStr = typeElement.GetString();
            if (EnumParser.TryParse<TokenFavorType>(typeStr, out TokenFavorType parsedType))
            {
                // Most favor types require a grantsId
                if (parsedType != TokenFavorType.Custom &&
                    !favor.TryGetProperty("grantsId", out _))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{favorId}",
                        $"Favor type '{typeStr}' requires a grantsId field",
                        ValidationSeverity.Warning));
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