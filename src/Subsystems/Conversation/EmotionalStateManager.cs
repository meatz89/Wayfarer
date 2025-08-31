using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages emotional state transitions and state-based rules.
/// Handles weight limits, draw counts, and transition logic.
/// </summary>
public class EmotionalStateManager
{
    private readonly ObligationQueueManager _queueManager;

    public EmotionalStateManager(ObligationQueueManager queueManager)
    {
        _queueManager = queueManager;
    }

    /// <summary>
    /// Process state transition based on comfort threshold
    /// </summary>
    public EmotionalState ProcessStateTransition(EmotionalState currentState, bool positiveDirection)
    {
        if (positiveDirection)
        {
            // Moving towards positive states
            return currentState switch
            {
                EmotionalState.DESPERATE => EmotionalState.TENSE,
                EmotionalState.HOSTILE => EmotionalState.TENSE,
                EmotionalState.TENSE => EmotionalState.GUARDED,
                EmotionalState.GUARDED => EmotionalState.NEUTRAL,
                EmotionalState.NEUTRAL => EmotionalState.OPEN,
                EmotionalState.OPEN => EmotionalState.EAGER,
                EmotionalState.EAGER => EmotionalState.CONNECTED,
                EmotionalState.CONNECTED => EmotionalState.CONNECTED, // Can't go higher
                _ => currentState
            };
        }
        else
        {
            // Moving towards negative states
            return currentState switch
            {
                EmotionalState.CONNECTED => EmotionalState.EAGER,
                EmotionalState.EAGER => EmotionalState.OPEN,
                EmotionalState.OPEN => EmotionalState.NEUTRAL,
                EmotionalState.NEUTRAL => EmotionalState.GUARDED,
                EmotionalState.GUARDED => EmotionalState.TENSE,
                EmotionalState.TENSE => EmotionalState.HOSTILE,
                EmotionalState.HOSTILE => EmotionalState.DESPERATE,
                EmotionalState.DESPERATE => EmotionalState.DESPERATE, // Can't go lower
                _ => currentState
            };
        }
    }

    /// <summary>
    /// Get weight limit for emotional state
    /// </summary>
    public int GetWeightLimit(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => 1,
            EmotionalState.HOSTILE => 0,  // Can't play normal cards
            EmotionalState.TENSE => 2,
            EmotionalState.GUARDED => 1,
            EmotionalState.NEUTRAL => 3,
            EmotionalState.OPEN => 3,
            EmotionalState.EAGER => 3,
            EmotionalState.CONNECTED => 4,
            _ => 3
        };
    }

    /// <summary>
    /// Get draw count for emotional state
    /// </summary>
    public int GetDrawCount(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => 2,
            EmotionalState.HOSTILE => 1,  // Draws 1 burden card
            EmotionalState.TENSE => 1,
            EmotionalState.GUARDED => 1,
            EmotionalState.NEUTRAL => 2,
            EmotionalState.OPEN => 2,
            EmotionalState.EAGER => 2,
            EmotionalState.CONNECTED => 2,
            _ => 2
        };
    }

    /// <summary>
    /// Validate if state change is legal
    /// </summary>
    public bool ValidateStateChange(EmotionalState fromState, EmotionalState toState)
    {
        // Can only move one step at a time
        var validTransitions = GetValidTransitions(fromState);
        return validTransitions.Contains(toState);
    }

    /// <summary>
    /// Get valid transitions from current state
    /// </summary>
    private List<EmotionalState> GetValidTransitions(EmotionalState currentState)
    {
        var transitions = new List<EmotionalState> { currentState }; // Can always stay same
        
        switch (currentState)
        {
            case EmotionalState.DESPERATE:
                transitions.Add(EmotionalState.TENSE);
                break;
            case EmotionalState.HOSTILE:
                transitions.Add(EmotionalState.TENSE);
                transitions.Add(EmotionalState.DESPERATE);
                break;
            case EmotionalState.TENSE:
                transitions.Add(EmotionalState.HOSTILE);
                transitions.Add(EmotionalState.GUARDED);
                break;
            case EmotionalState.GUARDED:
                transitions.Add(EmotionalState.TENSE);
                transitions.Add(EmotionalState.NEUTRAL);
                break;
            case EmotionalState.NEUTRAL:
                transitions.Add(EmotionalState.GUARDED);
                transitions.Add(EmotionalState.OPEN);
                break;
            case EmotionalState.OPEN:
                transitions.Add(EmotionalState.NEUTRAL);
                transitions.Add(EmotionalState.EAGER);
                break;
            case EmotionalState.EAGER:
                transitions.Add(EmotionalState.OPEN);
                transitions.Add(EmotionalState.CONNECTED);
                break;
            case EmotionalState.CONNECTED:
                transitions.Add(EmotionalState.EAGER);
                break;
        }
        
        return transitions;
    }

    /// <summary>
    /// Get state characteristics for UI display
    /// </summary>
    public StateCharacteristics GetStateTraits(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => new StateCharacteristics
            {
                Name = "Desperate",
                Description = "In crisis, needs immediate help",
                WeightLimit = 1,
                DrawCount = 2,
                CanGenerateLetter = false,
                Color = "#FF0000"
            },
            EmotionalState.HOSTILE => new StateCharacteristics
            {
                Name = "Hostile",
                Description = "Angry and confrontational",
                WeightLimit = 0,
                DrawCount = 1,
                CanGenerateLetter = false,
                DrawsBurdenCards = true,
                Color = "#CC0000"
            },
            EmotionalState.TENSE => new StateCharacteristics
            {
                Name = "Tense",
                Description = "On edge and wary",
                WeightLimit = 2,
                DrawCount = 1,
                CanGenerateLetter = false,
                Color = "#FF6600"
            },
            EmotionalState.GUARDED => new StateCharacteristics
            {
                Name = "Guarded",
                Description = "Cautious but listening",
                WeightLimit = 1,
                DrawCount = 1,
                CanGenerateLetter = false,
                Color = "#FFAA00"
            },
            EmotionalState.NEUTRAL => new StateCharacteristics
            {
                Name = "Neutral",
                Description = "Calm and receptive",
                WeightLimit = 3,
                DrawCount = 2,
                CanGenerateLetter = false,
                Color = "#888888"
            },
            EmotionalState.OPEN => new StateCharacteristics
            {
                Name = "Open",
                Description = "Engaged and interested",
                WeightLimit = 3,
                DrawCount = 2,
                CanGenerateLetter = true,
                Color = "#66CC00"
            },
            EmotionalState.EAGER => new StateCharacteristics
            {
                Name = "Eager",
                Description = "Enthusiastic and trusting",
                WeightLimit = 3,
                DrawCount = 2,
                CanGenerateLetter = true,
                Color = "#00CC00"
            },
            EmotionalState.CONNECTED => new StateCharacteristics
            {
                Name = "Connected",
                Description = "Deep understanding achieved",
                WeightLimit = 4,
                DrawCount = 2,
                CanGenerateLetter = true,
                Color = "#00AA00"
            },
            _ => new StateCharacteristics
            {
                Name = "Unknown",
                Description = "",
                WeightLimit = 3,
                DrawCount = 2,
                CanGenerateLetter = false,
                Color = "#666666"
            }
        };
    }

    /// <summary>
    /// Process comfort-triggered state transition
    /// </summary>
    public EmotionalState ComfortTriggeredTransition(EmotionalState currentState, int comfort)
    {
        if (comfort >= 3)
        {
            // Positive transition
            return ProcessStateTransition(currentState, true);
        }
        else if (comfort <= -3)
        {
            // Negative transition
            return ProcessStateTransition(currentState, false);
        }
        
        return currentState;
    }

    /// <summary>
    /// Calculate state change based on card play
    /// </summary>
    public StateChangeResult CalculateStateChange(EmotionalState currentState, int comfortChange, int currentComfort)
    {
        var newComfort = currentComfort + comfortChange;
        EmotionalState newState = currentState;
        bool stateChanged = false;
        
        // Check if comfort triggers state change
        if (newComfort >= 3)
        {
            newState = ProcessStateTransition(currentState, true);
            stateChanged = newState != currentState;
            if (stateChanged)
            {
                newComfort = 0; // Reset on state change
            }
        }
        else if (newComfort <= -3)
        {
            newState = ProcessStateTransition(currentState, false);
            stateChanged = newState != currentState;
            if (stateChanged)
            {
                newComfort = 0; // Reset on state change
            }
        }
        
        return new StateChangeResult
        {
            NewState = newState,
            NewComfort = newComfort,
            StateChanged = stateChanged
        };
    }

    /// <summary>
    /// Determine initial state based on NPC and obligations
    /// </summary>
    public EmotionalState DetermineInitialState(NPC npc)
    {
        return ConversationRules.DetermineInitialState(npc, _queueManager);
    }

    /// <summary>
    /// Check if card can be played in state
    /// </summary>
    public bool CanPlayCard(CardInstance card, EmotionalState state)
    {
        // Check state restrictions on card
        if (card.Context?.ValidStates != null && card.Context.ValidStates.Any())
        {
            if (!card.Context.ValidStates.Contains(state))
                return false;
        }

        // HOSTILE can only play burden cards
        if (state == EmotionalState.HOSTILE)
        {
            return card.Category == CardCategory.Burden;
        }

        // Check weight limit
        var weightLimit = GetWeightLimit(state);
        if (card.Weight > weightLimit)
            return false;

        return true;
    }

    /// <summary>
    /// Check if NPC has valid goal card for state
    /// </summary>
    public bool HasValidGoalCard(NPC npc, EmotionalState state)
    {
        if (npc.GoalDeck == null || !npc.GoalDeck.HasCardsAvailable())
            return false;

        foreach (var goalCard in npc.GoalDeck.GetAllCards())
        {
            if (goalCard.Context?.ValidStates != null && goalCard.Context.ValidStates.Any())
            {
                if (goalCard.Context.ValidStates.Contains(state))
                    return true;
            }
            else
            {
                // No restrictions means valid
                return true;
            }
        }
        
        return false;
    }
}

/// <summary>
/// Characteristics of an emotional state
/// </summary>
public class StateCharacteristics
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int WeightLimit { get; set; }
    public int DrawCount { get; set; }
    public bool CanGenerateLetter { get; set; }
    public bool DrawsBurdenCards { get; set; }
    public string Color { get; set; }
}

/// <summary>
/// Result of state change calculation
/// </summary>
public class StateChangeResult
{
    public EmotionalState NewState { get; set; }
    public int NewComfort { get; set; }
    public bool StateChanged { get; set; }
}