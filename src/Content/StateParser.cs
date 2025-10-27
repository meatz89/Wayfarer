using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for converting StateDTO to State domain model
/// </summary>
public static class StateParser
{
    /// <summary>
    /// Convert a StateDTO to a State domain model
    /// </summary>
    public static State ConvertDTOToState(StateDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Type))
            throw new InvalidOperationException("State DTO missing required 'Type' field");
        if (string.IsNullOrEmpty(dto.Category))
            throw new InvalidOperationException($"State {dto.Type} missing required 'Category' field");

        // Parse state type
        if (!Enum.TryParse<StateType>(dto.Type, true, out StateType stateType))
        {
            throw new InvalidOperationException($"State has invalid Type value: '{dto.Type}'. Must be valid StateType enum value.");
        }

        // Parse category
        if (!Enum.TryParse<StateCategory>(dto.Category, true, out StateCategory category))
        {
            throw new InvalidOperationException($"State {dto.Type} has invalid Category value: '{dto.Category}'. Must be 'Physical', 'Mental', or 'Social'.");
        }

        State state = new State
        {
            Type = stateType,
            Category = category,
            Description = dto.Description ?? "",
            BlockedActions = dto.BlockedActions ?? new List<string>(),
            EnabledActions = dto.EnabledActions ?? new List<string>(),
            Duration = dto.Duration,
            // Parse-time translation: clearConditions → strongly-typed behavior object
            ClearingBehavior = StateClearConditionsCatalogue.GetClearingBehavior(dto.ClearConditions ?? new List<string>())
        };

        return state;
    }

    /// <summary>
    /// Parse all states from StateDTO list
    /// </summary>
    public static List<State> ParseStates(List<StateDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<State>();

        List<State> states = new List<State>();
        foreach (StateDTO dto in dtos)
        {
            try
            {
                State state = ConvertDTOToState(dto);
                states.Add(state);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse state: {ex.Message}", ex);
            }
        }

        return states;
    }

}
