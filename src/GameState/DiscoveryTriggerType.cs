/// <summary>
/// Discovery trigger types determine WHEN an investigation becomes discoverable
/// Each type has different prerequisite evaluation logic
/// </summary>
public enum DiscoveryTriggerType
{
    /// <summary>
    /// Investigation visible immediately upon entering location/spot
    /// Prerequisites: at_location_spot(X)
    /// Example: Broken waterwheel visible in courtyard
    /// </summary>
    ImmediateVisibility,

    /// <summary>
    /// Investigation revealed through examining location features
    /// Prerequisites: location_familiarity(X) >= N, examining specific spot
    /// Example: Hidden damage noticed after thorough exploration
    /// </summary>
    EnvironmentalObservation,

    /// <summary>
    /// Investigation revealed through NPC dialogue
    /// Prerequisites: has_knowledge(key) from conversation
    /// Example: NPC mentions mystery, grants observation card that spawns investigation
    /// </summary>
    ConversationalDiscovery,

    /// <summary>
    /// Investigation triggered by acquiring specific item
    /// Prerequisites: has_item(X)
    /// Example: Finding torn letter spawns investigation about its sender
    /// </summary>
    ItemDiscovery,

    /// <summary>
    /// Investigation spawned when accepting specific NPC request/obligation
    /// Prerequisites: accepted_obligation(X)
    /// Example: Merchant asks you to investigate sabotage
    /// </summary>
    ObligationTriggered
}
