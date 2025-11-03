/// <summary>
/// DTO for SceneTemplate - immutable archetype that spawns Scene instances
/// Orchestrates spawning of multiple related Situations with placement strategies
/// Maps to SceneTemplate domain entity
/// </summary>
public class SceneTemplateDTO
{
    /// <summary>
    /// Unique template identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Scene pattern classification (Linear, HubAndSpoke, Branching, Converging, Discovery, etc.)
    /// Maps to SpawnPattern enum
    /// </summary>
    public string Archetype { get; set; }

    /// <summary>
    /// Display name template with {placeholders} for entity names
    /// Example: "The Mystery of {LocationName}"
    /// </summary>
    public string DisplayNameTemplate { get; set; }

    /// <summary>
    /// Categorical filters for procedural entity selection
    /// Determines what NPCs/Locations/Routes match this Scene's context
    /// </summary>
    public PlacementFilterDTO PlacementFilter { get; set; }

    /// <summary>
    /// Temporal eligibility conditions for scene spawning
    /// null = always eligible (no temporal filtering)
    /// Defines player state, world state, and entity state requirements
    /// </summary>
    public SpawnConditionsDTO SpawnConditions { get; set; }

    /// <summary>
    /// Embedded situation templates (composition)
    /// Each defines a narrative context with 2-4 choices
    /// </summary>
    public List<SituationTemplateDTO> SituationTemplates { get; set; } = new List<SituationTemplateDTO>();

    /// <summary>
    /// Cascade pattern definitions
    /// Defines how Situations sequence/branch/converge
    /// </summary>
    public SituationSpawnRulesDTO SpawnRules { get; set; }

    /// <summary>
    /// Optional time limit before Scene expires
    /// null = no expiration
    /// </summary>
    public int? ExpirationDays { get; set; }

    /// <summary>
    /// Flag for initial game setup scenes
    /// </summary>
    public bool IsStarter { get; set; } = false;

    /// <summary>
    /// Intro narrative template with {placeholders}
    /// Example: "As you approach {LocationName}, {NPCName} appears..."
    /// </summary>
    public string IntroNarrativeTemplate { get; set; }

    /// <summary>
    /// Complexity tier (0-4)
    /// Tier 0: Safety net, Tier 1: Low, Tier 2: Standard, Tier 3: High, Tier 4: Climactic
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    /// Presentation mode - how this Scene appears to the player
    /// "Atmospheric": Scene appears as menu option (existing behavior)
    /// "Modal": Scene takes over full screen on location entry (Sir Brante forced moment)
    /// Defaults to "Atmospheric" if not specified
    /// </summary>
    public string PresentationMode { get; set; }

    /// <summary>
    /// Progression mode - how situations within this Scene flow after choices
    /// "Breathe": Return to menu after each situation (player-controlled pacing)
    /// "Cascade": Continue to next situation immediately (pressure and momentum)
    /// Defaults to "Breathe" if not specified
    /// </summary>
    public string ProgressionMode { get; set; }
}
