using System.Text.Json;

/// <summary>
/// Validates game configuration JSON files.
/// </summary>
public class GameConfigValidator : IContentValidator
{
    public bool CanValidate(string fileName)
    {
        return fileName.Equals("game-config.json", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<ValidationError> Validate(string content, string fileName)
    {
        List<ValidationError> errors = new List<ValidationError>();

        using JsonDocument doc = JsonDocument.Parse(content);
        JsonElement root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            errors.Add(new ValidationError(
                fileName,
                "Game config file must contain a JSON object",
                ValidationSeverity.Critical));
            return errors;
        }

        // Validate main sections
        ValidateTimeConfig(root, fileName, errors);
        ValidateStaminaConfig(root, fileName, errors);
        ValidateTokenEconomyConfig(root, fileName, errors);
        ValidateDayTransitionConfig(root, fileName, errors);
        ValidateWorkRewardsConfig(root, fileName, errors);

        return errors;
    }

    private void ValidateTimeConfig(JsonElement root, string fileName, List<ValidationError> errors)
    {
        if (!root.TryGetProperty("time", out JsonElement time))
        {
            errors.Add(new ValidationError(
                fileName,
                "Missing required section: time",
                ValidationSeverity.Critical));
            return;
        }

    }

    private void ValidateStaminaConfig(JsonElement root, string fileName, List<ValidationError> errors)
    {
        if (!root.TryGetProperty("stamina", out JsonElement stamina))
        {
            errors.Add(new ValidationError(
                fileName,
                "Missing required section: stamina",
                ValidationSeverity.Critical));
            return;
        }

        // Validate maxStamina
        if (stamina.TryGetProperty("maxStamina", out JsonElement max) &&
            max.ValueKind == JsonValueKind.Number)
        {
            int maxStamina = max.GetInt32();
            if (maxStamina < 1 || maxStamina > 100)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:stamina",
                    $"maxStamina must be between 1 and 100 (got {maxStamina})",
                    ValidationSeverity.Critical));
            }
        }
    }

    private void ValidateTokenEconomyConfig(JsonElement root, string fileName, List<ValidationError> errors)
    {
        if (!root.TryGetProperty("tokenEconomy", out JsonElement tokenEconomy))
        {
            errors.Add(new ValidationError(
                fileName,
                "Missing required section: tokenEconomy",
                ValidationSeverity.Critical));
            return;
        }

        // Validate token thresholds
        if (tokenEconomy.TryGetProperty("basicLetterThreshold", out JsonElement basic) &&
            tokenEconomy.TryGetProperty("qualityLetterThreshold", out JsonElement quality) &&
            tokenEconomy.TryGetProperty("premiumLetterThreshold", out JsonElement premium) &&
            basic.ValueKind == JsonValueKind.Number &&
            quality.ValueKind == JsonValueKind.Number &&
            premium.ValueKind == JsonValueKind.Number)
        {
            int basicThreshold = basic.GetInt32();
            int qualityThreshold = quality.GetInt32();
            int premiumThreshold = premium.GetInt32();

            if (basicThreshold > qualityThreshold || qualityThreshold > premiumThreshold)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:tokenEconomy",
                    $"Token thresholds must be in ascending order: basic ({basicThreshold}) < quality ({qualityThreshold}) < premium ({premiumThreshold})",
                    ValidationSeverity.Critical));
            }
        }
    }

    private void ValidateDayTransitionConfig(JsonElement root, string fileName, List<ValidationError> errors)
    {
        if (!root.TryGetProperty("dayTransition", out JsonElement dayTransition))
        {
            errors.Add(new ValidationError(
                fileName,
                "Missing required section: dayTransition",
                ValidationSeverity.Warning));
            return;
        }

        // Validate percentages sum to 100 (optional but useful)
        if (dayTransition.TryGetProperty("percentages", out JsonElement percentages) &&
            percentages.ValueKind == JsonValueKind.Object)
        {
            int totalPercentage = 0;
            foreach (JsonProperty pct in percentages.EnumerateObject())
            {
                if (pct.Value.ValueKind == JsonValueKind.Number)
                {
                    totalPercentage += pct.Value.GetInt32();
                }
            }

            if (totalPercentage != 100)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:dayTransition.percentages",
                    $"Percentages should sum to 100 (got {totalPercentage})",
                    ValidationSeverity.Warning));
            }
        }
    }

    private void ValidateWorkRewardsConfig(JsonElement root, string fileName, List<ValidationError> errors)
    {
        if (!root.TryGetProperty("workRewards", out JsonElement workRewards))
        {
            errors.Add(new ValidationError(
                fileName,
                "Missing required section: workRewards",
                ValidationSeverity.Warning));
            return;
        }

        if (workRewards.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty reward in workRewards.EnumerateObject())
            {
                // Validate profession
                if (!EnumParser.TryParse<Professions>(reward.Name, out _))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:workRewards",
                        $"Invalid profession: '{reward.Name}'",
                        ValidationSeverity.Warning));
                }

                // Validate reward structure
                if (reward.Value.ValueKind == JsonValueKind.Object)
                {
                    ValidateWorkReward(reward.Value, reward.Name, fileName, errors);
                }
            }
        }
    }

    private void ValidateWorkReward(JsonElement reward, string profession, string fileName, List<ValidationError> errors)
    {
        // Validate coinRange
        if (reward.TryGetProperty("coinRange", out JsonElement coinRange) &&
            coinRange.ValueKind == JsonValueKind.Object)
        {
            ValidateRange(coinRange, $"{fileName}:workRewards.{profession}.coinRange", errors, "min", "max");
        }

        // Validate staminaCost
        if (reward.TryGetProperty("staminaCost", out JsonElement stamina) &&
            stamina.ValueKind == JsonValueKind.Number)
        {
            int cost = stamina.GetInt32();
            if (cost < 0)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:workRewards.{profession}",
                    $"staminaCost must be non-negative (got {cost})",
                    ValidationSeverity.Warning));
            }
        }
    }

    private void ValidateRange(JsonElement range, string path, List<ValidationError> errors, string minField, string maxField)
    {
        if (range.TryGetProperty(minField, out JsonElement min) &&
            range.TryGetProperty(maxField, out JsonElement max) &&
            min.ValueKind == JsonValueKind.Number &&
            max.ValueKind == JsonValueKind.Number)
        {
            int minValue = min.GetInt32();
            int maxValue = max.GetInt32();

            if (minValue > maxValue)
            {
                errors.Add(new ValidationError(
                    path,
                    $"{minField} ({minValue}) cannot be greater than {maxField} ({maxValue})",
                    ValidationSeverity.Warning));
            }
        }
    }
}