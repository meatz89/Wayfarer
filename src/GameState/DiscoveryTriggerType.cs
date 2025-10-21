/// <summary>
/// Discovery trigger types determine WHEN an obligation becomes discoverable
/// Each type has different prerequisite evaluation logic
/// </summary>
public enum DiscoveryTriggerType
{
    /// <summary>
    /// Obligation visible immediately upon entering location/location
    /// Prerequisites: at_location_spot(X)
    /// Example: Broken waterwheel visible in courtyard
    /// </summary>
    ImmediateVisibility,

    /// <summary>
    /// Obligation revealed through examining Venue features
    /// Prerequisites: location_familiarity(X) >= N, examining specific location
    /// Example: Hidden damage noticed after thorough exploration
    /// </summary>
    EnvironmentalObservation,

    /// <summary>
    /// Obligation revealed through NPC dialogue
    /// Prerequisites: has_knowledge(key) from conversation
    /// Example: NPC mentions mystery, grants observation card that spawns obligation
    /// </summary>
    ConversationalDiscovery,

    /// <summary>
    /// Obligation triggered by acquiring specific item
    /// Prerequisites: has_item(X)
    /// Example: Finding torn letter spawns obligation about its sender
    /// </summary>
    ItemDiscovery,

    /// <summary>
    /// Obligation spawned when accepting specific NPC request/obligation
    /// Prerequisites: accepted_obligation(X)
    /// Example: Merchant asks you to investigate sabotage
    /// </summary>
    ObligationTriggered,

    /// <summary>
    /// Obligation revealed after completing specific goal
    /// Prerequisites: goal_completed(X)
    /// Example: Completing "Gather Information" with Martha reveals her daughter's disappearance
    /// PROPER ARCHITECTURE: Checks actual game state (goal completion), not invisible knowledge tokens
    /// </summary>
    GoalCompletionTrigger
}
