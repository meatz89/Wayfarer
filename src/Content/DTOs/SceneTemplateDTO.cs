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

    // ==================== ACTIVATION FILTERS (TRIGGER CONDITIONS) ====================
    // Determines WHEN scene activates (Deferred → Active transition)
    // Separate from situation placement filters (each situation specifies explicit categorical properties)
    // Parsed into SceneTemplate, copied to Scene instance at spawn time

    /// <summary>
    /// Location activation filter - categorical properties that trigger scene activation
    /// Scene activates when player enters location matching these categorical properties
    /// null = no location-based activation (use NPC or other trigger)
    /// Example: Activate when player enters any location with Purpose=Dwelling, Privacy=SemiPublic
    /// Separate from Situation.LocationFilter which determines WHERE situation happens (always explicit per-situation)
    /// </summary>
    public PlacementFilterDTO LocationActivationFilter { get; set; }

    /// <summary>
    /// Temporal eligibility conditions for scene spawning
    /// null = always eligible (no temporal filtering)
    /// Defines player state, world state, and entity state requirements
    /// </summary>
    public SpawnConditionsDTO SpawnConditions { get; set; }

    /// <summary>
    /// Optional time limit before Scene expires
    /// null = no expiration
    /// </summary>
    public int? ExpirationDays { get; set; }

    /// <summary>
    /// Intro narrative template with {placeholders}
    /// Example: "As you approach {LocationName}, {NPCName} appears..."
    /// </summary>
    public string IntroNarrativeTemplate { get; set; }

    /// <summary>
    /// Complexity tier (0-4) - REQUIRED, no default (FAIL-FAST)
    /// Tier 0: Safety net, Tier 1: Low, Tier 2: Standard, Tier 3: High, Tier 4: Climactic
    /// Missing tier = parse-time exception (content authoring error)
    /// </summary>
    public int? Tier { get; set; }

    /// <summary>
    /// Story category for narrative role classification
    /// Values: "MainStory", "SideStory", "Service"
    /// MainStory = A-story progression (sequential A1-A10, then procedural A11+)
    /// SideStory = B-story content (optional, unlocked by A-story)
    /// Service = C-story content (repeatable transactional scenes)
    /// Defaults to "SideStory" if not specified
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Main story sequence number for A-story scenes
    /// 1-10 = Authored tutorial scenes (sequential progression)
    /// 11+ = Procedural continuation (infinite)
    /// null = Not part of A-story (SideStory or Service content)
    /// Parser validation: Non-null value requires Category = "MainStory"
    /// </summary>
    public int? MainStorySequence { get; set; }

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

    /// <summary>
    /// Scene archetype type for procedural scene generation.
    /// String value validated at parse-time against SceneArchetypeType enum (PascalCase).
    /// HIGHLANDER: ONE SceneArchetypeCatalog generates SituationTemplates at PARSE TIME.
    ///
    /// PRINCIPLE: This is a TYPE discriminator, not an ID (arc42 §8.3).
    /// All scenes have an archetype - it defines structure (situation count, transitions).
    ///
    /// Scene archetype defines WHAT the scene contains (design).
    /// PlacementFilter defines WHERE/WHEN it appears (configuration).
    /// Valid values: InnLodging, ConsequenceReflection, DeliveryContract, RouteSegmentTravel,
    ///               SeekAudience, InvestigateLocation, GatherTestimony, ConfrontAntagonist,
    ///               MeetOrderMember, DiscoverArtifact, UncoverConspiracy, UrgentDecision, MoralCrossroads
    /// </summary>
    public string SceneArchetype { get; set; }

    /// <summary>
    /// Archetype category for procedural selection (CATALOGUE PATTERN).
    /// Used when specific SceneArchetype is unknown - parser calls Catalogue to resolve.
    /// Values: "Investigation", "Social", "Confrontation", "Crisis"
    /// null = use explicit SceneArchetype (authored content)
    /// PARSE-TIME RESOLUTION: Parser calls SceneArchetypeCatalog.ResolveFromCategory at parse-time.
    /// Combined with ExcludedArchetypes for anti-repetition.
    /// </summary>
    public string ArchetypeCategory { get; set; }

    /// <summary>
    /// Archetypes to exclude when resolving ArchetypeCategory (anti-repetition).
    /// Only used when ArchetypeCategory is set.
    /// Values: SceneArchetypeType enum names (PascalCase)
    /// Example: ["InvestigateLocation", "GatherTestimony"] = avoid recently used archetypes
    /// null or empty = no exclusions (any archetype from category valid)
    /// </summary>
    public List<string> ExcludedArchetypes { get; set; }

    /// <summary>
    /// Service type for service_with_location_access archetype
    /// Values: "lodging", "bathing", "healing", "storage", "training"
    /// Ignored if SceneArchetype is null or not service-related
    /// Used to generate service-specific rewards and narrative hints
    /// </summary>
    public string ServiceType { get; set; }

    /// <summary>
    /// Whether this scene is created as Deferred at game start
    /// true = Scene created as Deferred during SpawnStarterScenes (e.g., A1 tutorial)
    /// false = Scene only created when ScenesToSpawn reward fires (e.g., A2, A3)
    /// Defaults to false - most scenes are NOT created at game start
    /// </summary>
    public bool IsStarter { get; set; } = false;

}
