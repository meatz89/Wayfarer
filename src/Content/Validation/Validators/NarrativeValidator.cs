using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Validates narrative JSON content files.
/// </summary>
public class NarrativeValidator : IContentValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "name", "description"
        };

    public bool CanValidate(string fileName)
    {
        return fileName.Equals("narratives.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("narrative-", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_narratives.json", StringComparison.OrdinalIgnoreCase);
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
                    "Narratives file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> narrativeIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement narrativeElement in root.EnumerateArray())
            {
                ValidateNarrative(narrativeElement, index, fileName, errors, narrativeIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate narratives: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateNarrative(JsonElement narrative, int index, string fileName, List<ValidationError> errors, HashSet<string> narrativeIds)
    {
        string narrativeId = GetStringProperty(narrative, "id") ?? $"Narrative[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(narrativeId) && narrativeId != $"Narrative[{index}]")
        {
            if (!narrativeIds.Add(narrativeId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{narrativeId}",
                    $"Duplicate narrative ID: {narrativeId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields
        foreach (string field in _requiredFields)
        {
            if (!narrative.TryGetProperty(field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{narrativeId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate starting conditions
        if (narrative.TryGetProperty("startingConditions", out JsonElement conditions) &&
            conditions.ValueKind == JsonValueKind.Object)
        {
            // Validate numeric values
            ValidatePositiveNumber(conditions, narrativeId, fileName, errors, "playerCoins");
            ValidatePositiveNumber(conditions, narrativeId, fileName, errors, "playerStamina");

            // Validate booleans
            ValidateBoolean(conditions, narrativeId, fileName, errors, "clearInventory");
            ValidateBoolean(conditions, narrativeId, fileName, errors, "clearLetterQueue");
            ValidateBoolean(conditions, narrativeId, fileName, errors, "clearObligations");
        }

        // Validate steps array
        if (narrative.TryGetProperty("steps", out JsonElement steps) &&
            steps.ValueKind == JsonValueKind.Array)
        {
            HashSet<string> stepIds = new HashSet<string>();
            int stepIndex = 0;

            foreach (JsonElement step in steps.EnumerateArray())
            {
                ValidateNarrativeStep(step, stepIndex, narrativeId, fileName, errors, stepIds);
                stepIndex++;
            }
        }

        // Validate rewards
        if (narrative.TryGetProperty("rewards", out JsonElement rewards) &&
            rewards.ValueKind == JsonValueKind.Object)
        {
            ValidatePositiveNumber(rewards, narrativeId, fileName, errors, "coins");
            ValidatePositiveNumber(rewards, narrativeId, fileName, errors, "stamina");

            // Validate items array
            if (rewards.TryGetProperty("items", out JsonElement items) &&
                items.ValueKind == JsonValueKind.Array)
            {
                // Items will be validated for existence once all content is loaded
            }
        }
    }

    private void ValidateNarrativeStep(JsonElement step, int index, string narrativeId, string fileName, List<ValidationError> errors, HashSet<string> stepIds)
    {
        string stepId = GetStringProperty(step, "id") ?? $"Step[{index}]";
        string fullStepId = $"{narrativeId}:{stepId}";

        // Check for duplicate step IDs
        if (!string.IsNullOrEmpty(stepId) && stepId != $"Step[{index}]")
        {
            if (!stepIds.Add(stepId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{fullStepId}",
                    $"Duplicate step ID: {stepId}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate required fields for steps
        if (!step.TryGetProperty("name", out _))
        {
            errors.Add(new ValidationError(
                $"{fileName}:{fullStepId}",
                "Step missing required field: name",
                ValidationSeverity.Critical));
        }

        // Validate requiredAction
        if (step.TryGetProperty("requiredAction", out JsonElement action) &&
            action.ValueKind == JsonValueKind.String)
        {
            string? actionStr = action.GetString();
            if (!string.IsNullOrEmpty(actionStr) &&
                !IsValidCommandType(actionStr))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{fullStepId}",
                    $"Invalid required command type: '{actionStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate allowedActions array
        if (step.TryGetProperty("allowedActions", out JsonElement allowedActions) &&
            allowedActions.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement allowedAction in allowedActions.EnumerateArray())
            {
                if (allowedAction.ValueKind == JsonValueKind.String)
                {
                    string? actionStr = allowedAction.GetString();
                    if (!string.IsNullOrEmpty(actionStr) &&
                        !IsValidCommandType(actionStr))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{fullStepId}",
                            $"Invalid allowed command type: '{actionStr}'",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }

        // Validate obligation to create
        if (step.TryGetProperty("obligationToCreate", out JsonElement obligation) &&
            obligation.ValueKind == JsonValueKind.Object)
        {
            ValidateStepObligation(obligation, fullStepId, fileName, errors);
        }
    }

    private void ValidateStepObligation(JsonElement obligation, string stepId, string fileName, List<ValidationError> errors)
    {
        // Check required obligation fields
        if (!obligation.TryGetProperty("id", out _))
        {
            errors.Add(new ValidationError(
                $"{fileName}:{stepId}:obligation",
                "Obligation missing required field: id",
                ValidationSeverity.Warning));
        }

        // Validate token type
        if (obligation.TryGetProperty("relatedTokenType", out JsonElement tokenType) &&
            tokenType.ValueKind == JsonValueKind.String)
        {
            string? tokenStr = tokenType.GetString();
            if (!string.IsNullOrEmpty(tokenStr) &&
                !EnumParser.TryParse<ConnectionType>(tokenStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{stepId}:obligation",
                    $"Invalid token type: '{tokenStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate effect arrays
        ValidateEffectArray(obligation, $"{stepId}:obligation", fileName, errors, "benefitEffects");
        ValidateEffectArray(obligation, $"{stepId}:obligation", fileName, errors, "constraintEffects");
    }

    private void ValidateEffectArray(JsonElement element, string id, string fileName, List<ValidationError> errors, string arrayName)
    {
        if (element.TryGetProperty(arrayName, out JsonElement effects) &&
            effects.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement effect in effects.EnumerateArray())
            {
                if (effect.ValueKind == JsonValueKind.String)
                {
                    string? effectStr = effect.GetString();
                    if (!string.IsNullOrEmpty(effectStr) &&
                        !EnumParser.TryParse<ObligationEffect>(effectStr, out _))
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{id}",
                            $"Invalid {arrayName} value: '{effectStr}'",
                            ValidationSeverity.Warning));
                    }
                }
            }
        }
    }

    private bool IsValidCommandType(string commandType)
    {
        // Check against known command types
        return commandType switch
        {
            "Rest" => true,
            "Converse" => true,
            "Work" => true,
            "Socialize" => true,
            "CollectLetter" => true,
            "DeliverLetter" => true,
            "BorrowMoney" => true,
            "GatherResources" => true,
            "Browse" => true,
            "Observe" => true,
            "PatronFunds" => true,
            "TravelEncounter" => true,
            _ => false
        };
    }
    
    private void ValidatePositiveNumber(JsonElement element, string id, string fileName, List<ValidationError> errors, string fieldName)
    {
        if (element.TryGetProperty(fieldName, out JsonElement field) &&
            field.ValueKind == JsonValueKind.Number)
        {
            int value = field.GetInt32();
            if (value < 0)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{id}",
                    $"{fieldName} must be a positive number (got {value})",
                    ValidationSeverity.Warning));
            }
        }
    }

    private void ValidateBoolean(JsonElement element, string id, string fileName, List<ValidationError> errors, string fieldName)
    {
        if (element.TryGetProperty(fieldName, out JsonElement field) &&
            field.ValueKind != JsonValueKind.True &&
            field.ValueKind != JsonValueKind.False)
        {
            errors.Add(new ValidationError(
                $"{fileName}:{id}",
                $"{fieldName} must be a boolean value",
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