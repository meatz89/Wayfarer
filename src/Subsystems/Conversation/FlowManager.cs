using System;

/// <summary>
/// Manages the flow battery system for conversations with strict -3 to +3 range,
/// automatic state transitions at extremes, and reset to 0 after each transition.
/// </summary>
public class FlowManager
{
    private int currentFlow = 0;
    private ConnectionState currentState;

    public int CurrentFlow => currentFlow;
    public ConnectionState CurrentState => currentState;

    public event Action<ConnectionState, ConnectionState>? StateTransitioned;
    public event Action? ConversationEnded;

    public FlowManager(ConnectionState initialState, int initialFlow = 0)
    {
        currentState = initialState;
        currentFlow = initialFlow; // Can start at any persisted value (-2 to +2)
    }

    /// <summary>
    /// Apply card result and handle any state transitions.
    /// Returns (stateChanged, newState, conversationEnds)
    /// </summary>
    public (bool stateChanged, ConnectionState newState, bool conversationEnds) ApplyCardResult(bool success)
    {
        // Simple: +1 for success, -1 for failure
        int change = success ? 1 : -1;
        return ApplyFlowChange(change);
    }

    /// <summary>
    /// Apply a flow change and handle any state transitions.
    /// Returns (stateChanged, newState, conversationEnds)
    /// </summary>
    public (bool stateChanged, ConnectionState newState, bool conversationEnds) ApplyFlowChange(int change, AtmosphereType atmosphere = AtmosphereType.Neutral)
    {
        // Apply atmosphere modifiers
        int modifiedChange = ModifyByAtmosphere(change, atmosphere);

        // Apply the change
        int newFlow = currentFlow + modifiedChange;
        
        // Allow flow to reach ±3 (displays as ±4) before transition
        currentFlow = newFlow;

        // Check for state transition at ±3 (one more change triggers transition)
        if (currentFlow >= 3)
        {
            return HandlePositiveTransition();
        }
        else if (currentFlow <= -3)
        {
            return HandleNegativeTransition();
        }

        // No transition, flow stays in range
        return (false, currentState, false);
    }

    private (bool, ConnectionState, bool) HandlePositiveTransition()
    {
        // At +3, check if next success would trigger transition
        if (currentFlow > 3)
        {
            ConnectionState oldState = currentState;
            ConnectionState newState = TransitionUp(currentState);

            if (newState != oldState)
            {
                currentState = newState;
                currentFlow = -2; // Enter new state at -2 (displays as -3)
                StateTransitioned?.Invoke(oldState, newState);
                return (true, newState, false);
            }
            else
            {
                // Already at TRUSTING, can't go higher
                currentFlow = 3; // Cap at max
                return (false, currentState, false);
            }
        }
        
        return (false, currentState, false);
    }

    private (bool, ConnectionState, bool) HandleNegativeTransition()
    {
        // At -3, check if next failure would trigger transition
        if (currentFlow < -3)
        {
            // Check if DISCONNECTED first
            if (currentState == ConnectionState.DISCONNECTED)
            {
                // DISCONNECTED at -4 ends conversation immediately
                ConversationEnded?.Invoke();
                return (false, currentState, true);
            }

            ConnectionState oldState = currentState;
            ConnectionState newState = TransitionDown(currentState);

            currentState = newState;
            currentFlow = 2; // Enter new state at +2 (displays as +3)
            StateTransitioned?.Invoke(oldState, newState);
            return (true, newState, false);
        }
        
        return (false, currentState, false);
    }

    private int ModifyByAtmosphere(int baseChange, AtmosphereType atmosphere)
    {
        return atmosphere switch
        {
            AtmosphereType.Volatile => baseChange > 0 ? baseChange + 1 : (baseChange < 0 ? baseChange - 1 : 0),
            AtmosphereType.Exposed => baseChange * 2,
            AtmosphereType.Synchronized => baseChange * 2, // Handled by effect doubling
            _ => baseChange
        };
    }

    private ConnectionState TransitionUp(ConnectionState current)
    {
        return current switch
        {
            ConnectionState.DISCONNECTED => ConnectionState.GUARDED,
            ConnectionState.GUARDED => ConnectionState.NEUTRAL,
            ConnectionState.NEUTRAL => ConnectionState.RECEPTIVE,
            ConnectionState.RECEPTIVE => ConnectionState.TRUSTING,
            ConnectionState.TRUSTING => ConnectionState.TRUSTING, // Max
            _ => current
        };
    }

    private ConnectionState TransitionDown(ConnectionState current)
    {
        return current switch
        {
            ConnectionState.TRUSTING => ConnectionState.RECEPTIVE,
            ConnectionState.RECEPTIVE => ConnectionState.NEUTRAL,
            ConnectionState.NEUTRAL => ConnectionState.GUARDED,
            ConnectionState.GUARDED => ConnectionState.DISCONNECTED,
            ConnectionState.DISCONNECTED => ConnectionState.DISCONNECTED, // Can't go lower
            _ => current
        };
    }

    /// <summary>
    /// Reset flow to 0 (used by observation cards with ResetFlow effect)
    /// </summary>
    public void ResetToZero()
    {
        currentFlow = 0;
    }

    /// <summary>
    /// Get the current flow level (internal range -3 to +3)
    /// </summary>
    public int GetFlow()
    {
        return currentFlow; // Internal range -3 to +3 (displays as -4 to +4)
    }

    /// <summary>
    /// Set the current state (for initialization or special effects)
    /// </summary>
    public void SetState(ConnectionState newState)
    {
        if (newState != currentState)
        {
            ConnectionState oldState = currentState;
            currentState = newState;
            StateTransitioned?.Invoke(oldState, newState);
        }
    }

    /// <summary>
    /// Check if at transition threshold
    /// </summary>
    public bool IsAtTransitionThreshold()
    {
        return Math.Abs(currentFlow) == 3;
    }

    /// <summary>
    /// Check if one away from transition
    /// </summary>
    public bool IsNearTransition()
    {
        return Math.Abs(currentFlow) == 2;
    }

    /// <summary>
    /// Get warning message for near transitions
    /// </summary>
    public string GetTransitionWarning()
    {
        if (currentFlow == 2)
        {
            ConnectionState nextState = TransitionUp(currentState);
            if (nextState != currentState)
                return $"One more positive to transition to {nextState}!";
            else
                return "At maximum positive flow";
        }
        else if (currentFlow == -2)
        {
            if (currentState == ConnectionState.DISCONNECTED)
                return "WARNING: One more negative will end conversation!";

            ConnectionState nextState = TransitionDown(currentState);
            return $"One more negative to transition to {nextState}!";
        }
        return "";
    }

    /// <summary>
    /// Get a visual representation of the flow battery
    /// </summary>
    public string GetFlowDisplay()
    {
        // Visual representation: [-3][-2][-1][0][+1][+2][+3]
        string display = "";
        for (int i = -3; i <= 3; i++)
        {
            if (i == currentFlow)
                display += $"[{i:+0;-0;0}]";
            else if (i == 0)
                display += "·0·";
            else
                display += $" {i:+#;-#} ";
        }
        return display;
    }

    /// <summary>
    /// Get a compact display for UI
    /// </summary>
    public string GetCompactDisplay()
    {
        // Display flow as -3 to +3 (even though internally it's -2 to +2)
        int displayFlow = currentFlow + (currentFlow > 0 ? 1 : currentFlow < 0 ? -1 : 0);
        string sign = displayFlow > 0 ? "+" : "";
        return $"{sign}{displayFlow}";
    }
}