using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Parser for PlayerAction entities with strong typing and enum validation.
/// Validates actionType against PlayerActionType enum - throws on unknown types.
/// PlayerActions are global actions available everywhere regardless of location.
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
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ActionType = actionType,  // Strongly typed enum
            Costs = ParseCosts(dto.Cost),
            TimeRequired = dto.TimeRequired,
            Priority = dto.Priority
        };

        return action;
    }

    private static ActionCosts ParseCosts(Dictionary<string, int> costDict)
    {
        if (costDict == null || costDict.Count == 0)
            return ActionCosts.None();

        ActionCosts costs = new ActionCosts();

        if (costDict.ContainsKey("coins"))
            costs.CoinCost = costDict["coins"];

        if (costDict.ContainsKey("focus"))
            costs.FocusCost = costDict["focus"];

        if (costDict.ContainsKey("stamina"))
            costs.StaminaCost = costDict["stamina"];

        if (costDict.ContainsKey("health"))
            costs.HealthCost = costDict["health"];

        return costs;
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
