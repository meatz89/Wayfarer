/// <summary>
/// Parser for LocationAction entities with strong typing and enum validation.
/// Validates actionType against LocationActionType enum - throws on unknown types.
/// Converts string-based JSON properties to strongly-typed LocationCapability flags.
/// </summary>
public static class LocationActionParser
{
    public static LocationAction ParseLocationAction(LocationActionDTO dto)
    {
        ValidateRequiredFields(dto);

        // ENUM VALIDATION - throws if unknown action type
        if (!Enum.TryParse<LocationActionType>(dto.ActionType, true, out LocationActionType actionType))
        {
            string validTypes = string.Join(", ", Enum.GetNames(typeof(LocationActionType)));
            throw new InvalidDataException(
                $"LocationAction '{dto.Id}' has unknown actionType '{dto.ActionType}'. " +
                $"Valid types: {validTypes}");
        }

        LocationAction action = new LocationAction
        {
            Name = dto.Name,
            Description = dto.Description,
            ActionType = actionType,  // Strongly typed enum
            Costs = ParseCosts(dto.Cost),
            Rewards = ParseRewards(dto.Reward),
            TimeRequired = dto.TimeRequired,
            Availability = ParseTimeBlocks(dto.Availability),
            Priority = dto.Priority,
            RequiredCapabilities = ParseLocationCapabilities(dto.RequiredProperties),
            OptionalCapabilities = ParseLocationCapabilities(dto.OptionalProperties),
            ExcludedCapabilities = ParseLocationCapabilities(dto.ExcludedProperties)
        };

        return action;
    }

    private static List<TimeBlocks> ParseTimeBlocks(List<string> timeBlockStrings)
    {
        List<TimeBlocks> result = new List<TimeBlocks>();

        if (timeBlockStrings == null || timeBlockStrings.Count == 0)
            return result;

        foreach (string blockStr in timeBlockStrings)
        {
            if (Enum.TryParse<TimeBlocks>(blockStr, true, out TimeBlocks timeBlock))
            {
                result.Add(timeBlock);
            }
            else
            {
                throw new InvalidDataException(
                    $"LocationAction has unknown availability time block '{blockStr}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames(typeof(TimeBlocks)))}");
            }
        }

        return result;
    }

    private static ActionCosts ParseCosts(ActionCostsDTO dto)
    {
        if (dto == null)
            return ActionCosts.None();

        return new ActionCosts
        {
            Coins = dto.Coins,
            Focus = dto.Focus,
            Stamina = dto.Stamina,
            Health = dto.Health
        };
    }

    private static ActionRewards ParseRewards(ActionRewardsDTO dto)
    {
        if (dto == null)
            return ActionRewards.None();

        return new ActionRewards
        {
            CoinReward = dto.Coins,
            HealthRecovery = dto.Health,
            FocusRecovery = dto.Focus,
            StaminaRecovery = dto.Stamina,
            FullRecovery = dto.FullRecovery
        };
    }

    private static void ValidateRequiredFields(LocationActionDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("LocationAction missing required field 'Id'");

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"LocationAction '{dto.Id}' missing required field 'Name'");

        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"LocationAction '{dto.Id}' missing required field 'Description'");

        if (string.IsNullOrEmpty(dto.ActionType))
            throw new InvalidDataException($"LocationAction '{dto.Id}' missing required field 'ActionType'");
    }

    private static LocationCapability ParseLocationCapabilities(List<string> propertyStrings)
    {
        LocationCapability result = LocationCapability.None;

        if (propertyStrings == null || propertyStrings.Count == 0)
            return result;

        foreach (string propStr in propertyStrings)
        {
            if (Enum.TryParse<LocationCapability>(propStr, true, out LocationCapability capability))
            {
                result |= capability;  // Bitwise OR to combine flags
            }
            else
            {
                // Log warning but don't throw - allows for graceful handling of unknown properties
                // In production, might want stricter validation
            }
        }

        return result;
    }
}
