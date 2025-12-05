/// <summary>
/// Parser for PlayerAction entities with strong typing and enum validation.
/// Validates actionType against PlayerActionType enum - throws on unknown types.
/// PlayerActions are global actions available everywhere regardless of location.
/// HIGHLANDER: Consequence is the ONLY class for resource outcomes.
/// </summary>
public static class PlayerActionParser
{
    public static PlayerAction ParsePlayerAction(PlayerActionDTO dto)
    {
        ValidateRequiredFields(dto);

        // ENUM VALIDATION - throws if unknown action type
        if (!Enum.TryParse<PlayerActionType>(dto.ActionType, true, out PlayerActionType actionType))
        {
            string validTypes = string.Join(", ", Enum.GetNames(typeof(PlayerActionType)));
            throw new InvalidDataException(
                $"PlayerAction '{dto.Id}' has unknown actionType '{dto.ActionType}'. " +
                $"Valid types: {validTypes}");
        }

        PlayerAction action = new PlayerAction
        {
            Name = dto.Name,
            Description = dto.Description,
            ActionType = actionType,  // Strongly typed enum
            Consequence = ParseConsequence(dto.Consequence),
            TimeRequired = dto.TimeRequired,
            Priority = dto.Priority
        };

        return action;
    }

    /// <summary>
    /// HIGHLANDER: Parse unified Consequence (negative = cost, positive = reward)
    /// </summary>
    private static Consequence ParseConsequence(ConsequenceDTO dto)
    {
        if (dto == null)
            return Consequence.None();

        return new Consequence
        {
            Coins = dto.Coins,
            Health = dto.Health,
            Stamina = dto.Stamina,
            Focus = dto.Focus,
            Hunger = dto.Hunger,
            Resolve = dto.Resolve,
            FullRecovery = dto.FullRecovery,
            // Flow control (HIGHLANDER: all flow through choices)
            NextSituationTemplateId = dto.NextSituationTemplateId,
            IsTerminal = dto.IsTerminal
        };
    }

    private static void ValidateRequiredFields(PlayerActionDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("PlayerAction missing required field 'Id'");

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"PlayerAction '{dto.Id}' missing required field 'Name'");

        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"PlayerAction '{dto.Id}' missing required field 'Description'");

        if (string.IsNullOrEmpty(dto.ActionType))
            throw new InvalidDataException($"PlayerAction '{dto.Id}' missing required field 'ActionType'");
    }
}
