/// <summary>
/// Records the intensity of a completed A-story scene for rhythm tracking.
/// Stored in Player.SceneIntensityHistory for context-aware scene selection.
///
/// Used to calculate:
/// - Recent intensity balance (how many Recovery/Standard/Demanding recently)
/// - When Peaceful is contextually appropriate (based on intensity history)
/// - Rhythm phase (accumulating vs testing vs recovering)
///
/// CHALLENGE PHILOSOPHY: This tracks STORY structure, not player resources.
/// Peaceful is earned through intensity history, not granted when player struggles.
/// </summary>
public class SceneIntensityRecord
{
    /// <summary>
    /// The intensity category of the completed scene.
    /// Recovery = Peaceful scenes, Standard = Investigation/Social, Demanding = Crisis/Confrontation
    /// </summary>
    public ArchetypeIntensity Intensity { get; set; }

    /// <summary>
    /// The archetype category that was selected (Investigation, Social, Confrontation, Crisis, Peaceful).
    /// Used for anti-repetition and category balance tracking.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The A-story sequence number of this scene.
    /// Used for ordering and determining recency.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// The day this scene was completed on.
    /// Used for time-based rhythm calculations.
    /// </summary>
    public int CompletedDay { get; set; }

    /// <summary>
    /// Whether this scene had a Crisis rhythm pattern (tests player investments).
    /// Distinct from Crisis category - some non-Crisis categories can use Crisis rhythm.
    /// </summary>
    public bool WasCrisisRhythm { get; set; }

    /// <summary>
    /// Location Safety where scene occurred (from Location.Safety).
    /// Used for location context tracking in rhythm calculations.
    /// </summary>
    public LocationSafety LocationSafety { get; set; }

    /// <summary>
    /// Location Purpose where scene occurred (from Location.Purpose).
    /// Used for location context tracking in rhythm calculations.
    /// </summary>
    public LocationPurpose LocationPurpose { get; set; }
}
