/// <summary>
/// Pure input DTO for scene archetype selection.
/// SIMPLIFIED: Only RhythmPattern + anti-repetition.
///
/// HIGHLANDER PRINCIPLE (arc42 §8.28):
/// - ALL scene generation uses the SAME selection logic
/// - RhythmPattern is THE ONLY driver for category selection
/// - LocationSafety, LocationPurpose, Tier REMOVED (legacy)
///
/// HISTORY-DRIVEN GENERATION (gdd/01 §1.8):
/// - RhythmPattern derived from intensity history
/// - Anti-repetition prevents immediate category/archetype repeats
/// - Current player state NEVER influences selection
///
/// This DTO makes selection deterministic and testable.
/// </summary>
public class SceneSelectionInputs
{
    /// <summary>
    /// Rhythm pattern for category selection.
    /// Building = Investigation/Social/Confrontation (accumulation phase)
    /// Crisis = Crisis/Confrontation (test phase)
    /// Mixed = Social/Investigation (recovery phase)
    /// </summary>
    public RhythmPattern RhythmPattern { get; set; }

    /// <summary>
    /// Categories used in last 2 scenes.
    /// Selection avoids these for variety.
    /// </summary>
    public List<string> RecentCategories { get; set; } = new List<string>();

    /// <summary>
    /// Archetypes used in recent scenes.
    /// Selection avoids these for variety.
    /// </summary>
    public List<string> RecentArchetypes { get; set; } = new List<string>();

    /// <summary>
    /// Create inputs with defaults (for testing or game start).
    /// </summary>
    public static SceneSelectionInputs CreateDefault()
    {
        return new SceneSelectionInputs
        {
            RhythmPattern = RhythmPattern.Building,
            RecentCategories = new List<string>(),
            RecentArchetypes = new List<string>()
        };
    }
}
