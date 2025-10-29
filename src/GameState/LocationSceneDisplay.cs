namespace Wayfarer.GameState;

/// <summary>
/// Display object for location/NPC scene presentation
/// THREE-LAYER DISPLAY PATTERN (from UNIFIED_ACTION_ARCHITECTURE.md):
/// 1. Persistent entities (Scene, Situation, actions) in GameWorld
/// 2. Persistent wrappers (LocationSceneDisplay) - EPHEMERAL, generated on demand
/// 3. Ephemeral content (UI components) - render from wrapper
///
/// This is Layer 2 - ephemeral wrapper that aggregates data from persistent entities
/// Created fresh each time player views location/NPC
/// NOT stored in GameWorld - throwaway presentation object
/// </summary>
public class LocationSceneDisplay
{
    /// <summary>
    /// Location this display is for (null if NPC scene)
    /// </summary>
    public Location Location { get; set; }

    /// <summary>
    /// NPC this display is for (null if location scene)
    /// </summary>
    public NPC NPC { get; set; }

    /// <summary>
    /// Display name for this scene context
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Contextual intro narrative for current game state
    /// Generated dynamically based on time, NPCs present, etc.
    /// </summary>
    public string IntroNarrative { get; set; }

    /// <summary>
    /// Available situations player can attempt
    /// Extracted from active Scenes at this placement
    /// Requirements already evaluated - these are unlocked
    /// </summary>
    public List<Situation> AvailableSituations { get; set; } = new List<Situation>();

    /// <summary>
    /// Locked situations with detailed requirement explanations
    /// Perfect information pattern - player sees what they need
    /// </summary>
    public List<SituationWithLockReason> LockedSituations { get; set; } = new List<SituationWithLockReason>();

    /// <summary>
    /// Current time context
    /// </summary>
    public TimeBlocks CurrentTimeBlock { get; set; }

    /// <summary>
    /// Current day
    /// </summary>
    public int CurrentDay { get; set; }

    /// <summary>
    /// Current segment within time block
    /// </summary>
    public int CurrentSegment { get; set; }
}
