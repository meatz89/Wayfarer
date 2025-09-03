using System;

/// <summary>
/// Manages the flow battery system for conversations with strict -3 to +3 range,
/// automatic state transitions at extremes, and reset to 0 after each transition.
/// </summary>
public class FlowManager
{
    private int currentFlow = 0;
    private EmotionalState currentState;
    
    public int CurrentFlow => currentFlow;
    public EmotionalState CurrentState => currentState;
    
    public event Action<EmotionalState, EmotionalState>? StateTransitioned;
    public event Action? ConversationEnded;
    
    public FlowManager(EmotionalState initialState)
    {
        currentState = initialState;
        currentFlow = 0; // Always starts at 0
    }
    
    /// <summary>
    /// Apply card result and handle any state transitions.
    /// Returns (stateChanged, newState, conversationEnds)
    /// </summary>
    public (bool stateChanged, EmotionalState newState, bool conversationEnds) ApplyCardResult(bool success)
    {
        // Simple: +1 for success, -1 for failure
        int change = success ? 1 : -1;
        return ApplyFlowChange(change);
    }
    
    /// <summary>
    /// Apply a flow change and handle any state transitions.
    /// Returns (stateChanged, newState, conversationEnds)
    /// </summary>
    public (bool stateChanged, EmotionalState newState, bool conversationEnds) ApplyFlowChange(int change, AtmosphereType atmosphere = AtmosphereType.Neutral)
    {
        // Apply atmosphere modifiers
        int modifiedChange = ModifyByAtmosphere(change, atmosphere);
        
        // Apply the change
        int newFlow = currentFlow + modifiedChange;
        
        // Clamp to -3 to +3 range
        newFlow = Math.Clamp(newFlow, -3, 3);
        currentFlow = newFlow;
        
        // Check for state transition at exactly ±3
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
    
    private (bool, EmotionalState, bool) HandlePositiveTransition()
    {
        var oldState = currentState;
        var newState = TransitionUp(currentState);
        
        if (newState != oldState)
        {
            currentState = newState;
            currentFlow = 0; // Reset battery to 0
            StateTransitioned?.Invoke(oldState, newState);
            return (true, newState, false);
        }
        else
        {
            // Already at CONNECTED, can't go higher
            // Flow stays at 3
            return (false, currentState, false);
        }
    }
    
    private (bool, EmotionalState, bool) HandleNegativeTransition()
    {
        // Check if DESPERATE first
        if (currentState == EmotionalState.DESPERATE)
        {
            // DESPERATE at -3 ends conversation immediately
            ConversationEnded?.Invoke();
            return (false, currentState, true);
        }
        
        var oldState = currentState;
        var newState = TransitionDown(currentState);
        
        currentState = newState;
        currentFlow = 0; // Reset battery to 0
        StateTransitioned?.Invoke(oldState, newState);
        return (true, newState, false);
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
    
    private EmotionalState TransitionUp(EmotionalState current)
    {
        return current switch
        {
            EmotionalState.DESPERATE => EmotionalState.TENSE,
            EmotionalState.TENSE => EmotionalState.NEUTRAL,
            EmotionalState.NEUTRAL => EmotionalState.OPEN,
            EmotionalState.OPEN => EmotionalState.CONNECTED,
            EmotionalState.CONNECTED => EmotionalState.CONNECTED, // Max
            _ => current
        };
    }
    
    private EmotionalState TransitionDown(EmotionalState current)
    {
        return current switch
        {
            EmotionalState.CONNECTED => EmotionalState.OPEN,
            EmotionalState.OPEN => EmotionalState.NEUTRAL,
            EmotionalState.NEUTRAL => EmotionalState.TENSE,
            EmotionalState.TENSE => EmotionalState.DESPERATE,
            EmotionalState.DESPERATE => EmotionalState.DESPERATE, // Can't go lower
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
    /// Get the current flow level clamped to range
    /// </summary>
    public int GetFlow()
    {
        return Math.Clamp(currentFlow, -3, 3);
    }
    
    /// <summary>
    /// Set the current state (for initialization or special effects)
    /// </summary>
    public void SetState(EmotionalState newState)
    {
        if (newState != currentState)
        {
            var oldState = currentState;
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
            var nextState = TransitionUp(currentState);
            if (nextState != currentState)
                return $"One more positive to transition to {nextState}!";
            else
                return "At maximum positive flow";
        }
        else if (currentFlow == -2)
        {
            if (currentState == EmotionalState.DESPERATE)
                return "WARNING: One more negative will end conversation!";
            
            var nextState = TransitionDown(currentState);
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
        var display = "";
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
        string sign = currentFlow > 0 ? "+" : "";
        return $"{sign}{currentFlow}";
    }
}