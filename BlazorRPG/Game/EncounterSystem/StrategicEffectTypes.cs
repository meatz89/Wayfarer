namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Types of strategic tag effects
    /// </summary>
    public enum StrategicEffectTypes
    {
        None,                      // Default/special case
        AddMomentumToApproach,     // Add momentum to specific approach choices
        AddMomentumToFocus,        // Add momentum to specific focus choices
        ReducePressureFromApproach, // Reduce pressure from specific approach choices
        ReducePressureFromFocus,    // Reduce pressure from specific focus choices
        ReducePressurePerTurn,      // Reduce pressure at end of each turn
        AddMomentumPerTurn,         // Add momentum at end of each turn
        ReduceMomentumFromApproach, // Negative tag: reduce momentum from approach
        ReduceMomentumFromFocus,    // Negative tag: reduce momentum from focus
        AddPressurePerTurn,          // Negative tag: add pressure each turn
        AddPressureFromApproach,
        AddPressureFromFocus,
        AddMomentumOnActivation,
        ReducePressureOnActivation
    }
}