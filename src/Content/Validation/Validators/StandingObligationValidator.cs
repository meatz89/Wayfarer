using System;
using System.Collections.Generic;
using System.Text.Json;
using Wayfarer.GameState.Enums;

/// <summary>
/// Validates standing obligation JSON content files.
/// </summary>
public class StandingObligationValidator : BaseValidator
{
    private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "name", "description", "source", "relatedTokenType",
            "benefitEffects", "constraintEffects"
        };

    public override bool CanValidate(string fileName)
    {
        return fileName.Equals("standing_obligations.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.Equals("scaling_obligations.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_standing_obligations.json", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_scaling_obligations.json", StringComparison.OrdinalIgnoreCase);
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
                    "Standing obligations file must contain a JSON array",
                    ValidationSeverity.Critical));
                return errors;
            }

            HashSet<string> obligationIds = new HashSet<string>();
            int index = 0;

            foreach (JsonElement obligationElement in root.EnumerateArray())
            {
                ValidateStandingObligation(obligationElement, index, fileName, errors, obligationIds);
                index++;
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError(
                fileName,
                $"Failed to validate standing obligations: {ex.Message}",
                ValidationSeverity.Critical));
        }

        return errors;
    }

    private void ValidateStandingObligation(JsonElement obligation, int index, string fileName, List<ValidationError> errors, HashSet<string> obligationIds)
    {
        string obligationId = GetStringProperty(obligation, "id") ?? $"StandingObligation[{index}]";

        // Check for duplicate IDs
        if (!string.IsNullOrEmpty(obligationId) && obligationId != $"StandingObligation[{index}]")
        {
            if (!obligationIds.Add(obligationId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{obligationId}",
                    $"Duplicate standing obligation ID: {obligationId}",
                    ValidationSeverity.Critical));
            }
        }

        // Check required fields using case-insensitive matching
        foreach (string field in _requiredFields)
        {
            if (!TryGetPropertyCaseInsensitive(obligation, field, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{obligationId}",
                    $"Missing required field: {field}",
                    ValidationSeverity.Critical));
            }
        }

        // Validate relatedTokenType
        if (TryGetPropertyCaseInsensitive(obligation, "relatedTokenType", out JsonElement tokenType) &&
            tokenType.ValueKind == JsonValueKind.String)
        {
            string? tokenStr = tokenType.GetString();
            if (!string.IsNullOrEmpty(tokenStr) &&
                !EnumParser.TryParse<ConnectionType>(tokenStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{obligationId}",
                    $"Invalid token type: '{tokenStr}'",
                    ValidationSeverity.Critical));
            }
        }

        // Validate benefitEffects array
        ValidateEffectArray(obligation, obligationId, fileName, errors, "benefitEffects");

        // Validate constraintEffects array
        ValidateEffectArray(obligation, obligationId, fileName, errors, "constraintEffects");

        // Validate letter template configuration
        if (TryGetPropertyCaseInsensitive(obligation, "letterTemplateConfig", out JsonElement templateConfig) &&
            templateConfig.ValueKind == JsonValueKind.Object)
        {
            // Validate frequency
            if (TryGetPropertyCaseInsensitive(templateConfig, "frequency", out JsonElement frequency) &&
                frequency.ValueKind == JsonValueKind.String)
            {
                string? freqStr = frequency.GetString();
                if (!string.IsNullOrEmpty(freqStr) &&
                    !EnumParser.TryParse<ObligationFrequency>(freqStr, out _))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{obligationId}",
                        $"Invalid obligation frequency: '{freqStr}'",
                        ValidationSeverity.Warning));
                }
            }

            // Validate numeric fields
            ValidatePositiveNumber(templateConfig, obligationId, fileName, errors, "daysBetweenLetters");
            ValidatePositiveNumber(templateConfig, obligationId, fileName, errors, "minimumTokensRequired");
        }

        // Validate scaling configuration
        ValidateScalingConfiguration(obligation, obligationId, fileName, errors);
    }

    private void ValidateEffectArray(JsonElement element, string id, string fileName, List<ValidationError> errors, string arrayName)
    {
        if (TryGetPropertyCaseInsensitive(element, arrayName, out JsonElement effects) &&
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

    private void ValidatePositiveNumber(JsonElement element, string id, string fileName, List<ValidationError> errors, string fieldName)
    {
        if (TryGetPropertyCaseInsensitive(element, fieldName, out JsonElement field) &&
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

    private void ValidateScalingConfiguration(JsonElement obligation, string id, string fileName, List<ValidationError> errors)
    {
        // Validate ScalingType
        if (TryGetPropertyCaseInsensitive(obligation, "scalingType", out JsonElement scalingType) &&
            scalingType.ValueKind == JsonValueKind.String)
        {
            string? scalingStr = scalingType.GetString();
            if (!string.IsNullOrEmpty(scalingStr) &&
                !EnumParser.TryParse<ScalingType>(scalingStr, out _))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{id}",
                    $"Invalid scaling type: '{scalingStr}'",
                    ValidationSeverity.Warning));
            }
        }

        // Validate MinValue < MaxValue if both present
        if (TryGetPropertyCaseInsensitive(obligation, "minValue", out JsonElement minValue) &&
            TryGetPropertyCaseInsensitive(obligation, "maxValue", out JsonElement maxValue) &&
            minValue.ValueKind == JsonValueKind.Number &&
            maxValue.ValueKind == JsonValueKind.Number)
        {
            float min = minValue.GetSingle();
            float max = maxValue.GetSingle();

            if (min > max)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{id}",
                    $"MinValue ({min}) cannot be greater than MaxValue ({max})",
                    ValidationSeverity.Warning));
            }
        }

        // Validate SteppedThresholds if present
        if (TryGetPropertyCaseInsensitive(obligation, "steppedThresholds", out JsonElement thresholds) &&
            thresholds.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty threshold in thresholds.EnumerateObject())
            {
                // Validate key is an integer
                if (!int.TryParse(threshold.Name, out _))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{id}",
                        $"SteppedThresholds key must be an integer: '{threshold.Name}'",
                        ValidationSeverity.Warning));
                }

                // Validate value is a number
                if (threshold.Value.ValueKind != JsonValueKind.Number)
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{id}",
                        $"SteppedThresholds value must be a number for key: '{threshold.Name}'",
                        ValidationSeverity.Warning));
                }
            }
        }
    }
}