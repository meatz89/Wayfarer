using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates progression unlock (network unlock) JSON content files.
/// </summary>
public class ProgressionUnlockValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "unlockerNpcId", "tokensRequired", "unlockDescription", "unlocks"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("progression_unlocks.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_progression_unlocks.json", StringComparison.OrdinalIgnoreCase);
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
                    "Progression unlocks file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> unlockIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement unlockElement in root.EnumerateArray())
            {
                ValidateProgressionUnlock(unlockElement, index, fileName, errors, unlockIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate progression unlocks: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateProgressionUnlock(JsonElement unlock, int index, string fileName, List<ValidationError> errors, HashSet<string> unlockIds)
    {
        string unlockId = GetStringProperty(unlock, "id") ?? $"ProgressionUnlock[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(unlockId) && unlockId != $"ProgressionUnlock[{index}]")
        {
            if (!unlockIds.Add(unlockId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{unlockId}",
                    $"Duplicate progression unlock ID: {unlockId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!unlock.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{unlockId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate tokensRequired
        if (unlock.TryGetProperty("tokensRequired", out JsonElement tokens) &&
            tokens.ValueKind == JsonValueKind.Number)
        {
            int tokenCount = tokens.GetInt32();
            if (tokenCount < 0)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{unlockId}",
                    $"tokensRequired must be non-negative (got {tokenCount})",
                    ValidationSeverity.Warning));
            }
        }

        // Validate unlocks array
        if (unlock.TryGetProperty("unlocks", out JsonElement unlocks) &&
            unlocks.ValueKind == JsonValueKind.Array)
        {
            if (unlocks.GetArrayLength() == 0)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{unlockId}",
                    "unlocks array cannot be empty",
                    ValidationSeverity.Critical));
            }

            int unlockIndex = 0;
            foreach (JsonElement unlockTarget in unlocks.EnumerateArray())
            {
                ValidateUnlockTarget(unlockTarget, unlockIndex, unlockId, fileName, errors);
                unlockIndex++;
            }
        }
    }

    private void ValidateUnlockTarget(JsonElement target, int index, string parentId, string fileName, List<ValidationError> errors)
    {
        string targetId = $"{parentId}:unlock[{index}]";

        // Check required fields for unlock target
        if (!target.TryGetProperty("npcId", out _))
        {
            errors.Add(new ValidationError(
                $"{fileName}:{targetId}",
                "Unlock target missing required field: npcId",
                ValidationSeverity.Critical));
        }

        if (!target.TryGetProperty("introductionText", out _))
        {
            errors.Add(new ValidationError(
                $"{fileName}:{targetId}",
                "Unlock target missing required field: introductionText",
                ValidationSeverity.Warning));
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