namespace Wayfarer.GameState.Enums;

/// <summary>
/// Pattern types for how Situations within a Scene lead into each other
/// Defines the narrative cascade structure
/// See situation-spawn-patterns.md for detailed pattern catalog
/// </summary>
public enum SpawnPattern
{
    /// <summary>
    /// Sequential progression A → B → C
    /// Each Situation spawns single follow-up
    /// </summary>
    Linear,

    /// <summary>
    /// Central Situation spawns multiple parallel child Situations
    /// All available simultaneously
    /// </summary>
    HubAndSpoke,

    /// <summary>
    /// Success and failure paths diverge
    /// Creates different consequence chains
    /// </summary>
    Branching,

    /// <summary>
    /// Multiple independent paths lead to single finale Situation
    /// </summary>
    Converging,

    /// <summary>
    /// Different paths based on game state evaluation
    /// </summary>
    Conditional,

    /// <summary>
    /// Player chooses between multiple competing paths
    /// Selecting one locks out others
    /// </summary>
    Exclusive,

    /// <summary>
    /// Situations must be completed in specific order
    /// Later situations locked until prerequisites met
    /// </summary>
    Sequential,

    /// <summary>
    /// Loop back to previous Situations based on outcomes
    /// Creates repeated attempts or cycles
    /// </summary>
    Recursive,

    /// <summary>
    /// Mix of parallel and sequential elements
    /// Complex multi-stage progression
    /// </summary>
    Hybrid,

    /// <summary>
    /// Single Situation, no cascade
    /// Standalone encounter
    /// </summary>
    Standalone,

    /// <summary>
    /// AutoAdvance - no player choices, narrative displays then auto-executes
    /// Used for sleep, forced transitions, narrative progression
    /// Situation.AutoProgressRewards applied automatically
    /// Player sees narrative briefly, then rewards/time advancement execute
    /// Tutorial Night Rest uses this: restore resources, advance to morning
    /// </summary>
    AutoAdvance
}
