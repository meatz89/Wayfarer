
/// <summary>
/// SceneTemplate - immutable archetype for procedural Scene generation
/// Defines Scene structure with categorical filters, embedded SituationTemplates, and spawn rules
/// Stored in GameWorld.SceneTemplates, used to instantiate Scene instances at runtime
/// Implements AI content generation pattern: Templates use categories, spawner resolves to concrete entities
/// </summary>
public class SceneTemplate
{
    /// <summary>
    /// Unique identifier for this SceneTemplate
    /// Used to spawn Scene instances from this archetype
    /// Referenced by ChoiceReward.SceneSpawnReward and Obligation phase rewards
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Archetype classification for this Scene pattern
    /// Linear, HubAndSpoke, Branching, Converging, Conditional, etc.
    /// Matches SpawnRules.Pattern for consistency
    /// Helps content creators understand Scene structure at a glance
    /// </summary>
    public SpawnPattern Archetype { get; init; }

    /// <summary>
    /// Scene archetype ID used for generation
    /// For A-story scenes: "seek_audience", "investigate_location", etc.
    /// Used for anti-repetition tracking and narrative generation context
    /// null for manually-authored scenes (not generated from catalogue)
    /// </summary>
    public string SceneArchetypeId { get; init; }

    /// <summary>
    /// Display name template for this Scene
    /// May contain placeholders like {NPCName}, {LocationName}
    /// Replaced at spawn time with actual entity names
    /// Example: "The Plea of {NPCName}", "Investigating {LocationName}"
    /// </summary>
    public string DisplayNameTemplate { get; init; }

    // ==================== HIERARCHICAL PLACEMENT (BASE FILTERS) ====================
    // CSS-style inheritance: SceneTemplate provides BASE filters for all situations
    // SituationTemplate can OVERRIDE selectively per-situation
    // Resolution: effectiveFilter = situationFilter ?? sceneBaseFilter

    /// <summary>
    /// Base location filter for all situations in this scene
    /// Applied to first situation by default, subsequent situations can override
    /// null = no base location (situations must specify their own)
    /// Enables multi-location scenes with shared default location
    /// </summary>
    public PlacementFilter BaseLocationFilter { get; init; }

    /// <summary>
    /// Base NPC filter for all situations in this scene
    /// Applied to first situation by default, subsequent situations can override
    /// null = no base NPC (situations must specify their own)
    /// Example: "Innkeeper" for all Inn Service situations unless overridden
    /// </summary>
    public PlacementFilter BaseNpcFilter { get; init; }

    /// <summary>
    /// Base route filter for all situations in this scene
    /// Applied to first situation by default, subsequent situations can override
    /// null = no base route (situations must specify their own)
    /// Rarely used - most situations don't involve routes
    /// </summary>
    public PlacementFilter BaseRouteFilter { get; init; }

    /// <summary>
    /// Temporal eligibility conditions for scene spawning
    /// null = always eligible (no temporal filtering)
    /// Non-null = Scene spawns only when conditions met (player state, world state, entity state)
    /// Three evaluation dimensions: PlayerState, WorldState, EntityState
    /// Evaluated by SpawnConditionsEvaluator at spawn time
    /// Enables dynamic narrative emergence based on game state
    /// </summary>
    public SpawnConditions SpawnConditions { get; init; }

    /// <summary>
    /// Embedded SituationTemplates defining Situations within this Scene
    /// Each SituationTemplate contains 2-4 ChoiceTemplates (Sir Brante pattern)
    /// NOT standalone entities - always embedded in SceneTemplate
    /// At spawn time, instantiated to Situation instances embedded in Scene
    /// COMPOSITION pattern - stored inline, not referenced by ID
    /// </summary>
    public List<SituationTemplate> SituationTemplates { get; init; } = new List<SituationTemplate>();

    /// <summary>
    /// Spawn rules defining how Situations lead into each other
    /// Creates cascade patterns: Linear, HubAndSpoke, Branching, Converging, etc.
    /// Transitions reference SituationTemplate IDs within this SceneTemplate
    /// Copied to Scene instance at spawn time
    /// </summary>
    public SituationSpawnRules SpawnRules { get; init; }

    /// <summary>
    /// Optional expiration duration in days
    /// null = no expiration (Scene remains until completed)
    /// Positive value = Scene expires after this many days from spawn
    /// At spawn time, calculated as: ExpirationDay = CurrentDay + ExpirationDays
    /// Enables time-limited content (rumors, opportunities, urgent requests)
    /// </summary>
    public int? ExpirationDays { get; init; }

    /// <summary>
    /// Starter content flag
    /// true = Spawn this Scene during initial game setup
    /// false = Spawn via triggers (Choice rewards, Obligation phases, events)
    /// GameWorld.SpawnInitialScenes() processes all IsStarter templates
    /// </summary>
    public bool IsStarter { get; init; } = false;

    /// <summary>
    /// Intro narrative template with placeholders
    /// Generated narrative shown when Scene first appears
    /// Example: "As you enter {LocationName}, you notice {NPCName} looks distressed..."
    /// Placeholders replaced at spawn time with actual entity properties
    /// If null/empty, SceneInstantiator may use AI generation with NarrativeHints
    /// </summary>
    public string IntroNarrativeTemplate { get; init; }

    /// <summary>
    /// Tier classification for this Scene
    /// 0 = Safety net (simple, repeatable, minimal requirements)
    /// 1 = Low complexity (0-3 Resolve, simple requirements)
    /// 2 = Standard complexity (5-8 Resolve, moderate requirements)
    /// 3 = High complexity (10-15 Resolve, complex requirements, deep cascades)
    /// 4 = Climactic moments (18-25 Resolve, very complex, resolution content)
    /// Used for difficulty scaling and requirement formula calculation
    /// </summary>
    public int Tier { get; init; } = 1;

    /// <summary>
    /// Story category classification for narrative role
    /// MainStory = A-story progression (sequential A1-A10, then procedural A11+)
    /// SideStory = B-story content (optional, unlocked by A-story progression)
    /// Service = C-story content (repeatable transactional scenes)
    /// Defaults to SideStory for backward compatibility with existing content
    /// </summary>
    public StoryCategory Category { get; init; } = StoryCategory.SideStory;

    /// <summary>
    /// Main story sequence number for A-story scenes
    /// 1-10 = Authored tutorial scenes (A1-A10, sequential progression)
    /// 11+ = Procedural continuation (A11, A12, A13... infinity)
    /// null = Not part of A-story (SideStory or Service content)
    /// CONSTRAINT: Non-null value requires Category = MainStory
    /// Used for A-story chain validation and sequence tracking
    /// </summary>
    public int? MainStorySequence { get; init; }

    /// <summary>
    /// Presentation mode - how Scenes spawned from this template appear to the player
    /// Atmospheric: Scene appears as menu option (existing behavior)
    /// Modal: Scene takes over full screen on location entry (Sir Brante forced moment)
    /// Defaults to Atmospheric for backward compatibility
    /// </summary>
    public PresentationMode PresentationMode { get; init; } = PresentationMode.Atmospheric;

    /// <summary>
    /// Progression mode - how situations within Scenes spawned from this template flow after choices
    /// Breathe: Return to menu after each situation (player-controlled pacing)
    /// Cascade: Continue to next situation immediately (pressure and momentum)
    /// Defaults to Breathe for backward compatibility
    /// </summary>
    public ProgressionMode ProgressionMode { get; init; } = ProgressionMode.Breathe;

    /// <summary>
    /// Dependent locations that this Scene creates dynamically at spawn time
    /// Self-contained pattern: Scene generates JSON package → PackageLoader → Standard parsing
    /// Each spec becomes LocationDTO, flows through normal JSON → DTO → Domain pipeline
    /// Scene tracks created location IDs for forensics and cleanup
    /// Empty list = Scene uses only pre-existing locations (traditional pattern)
    /// </summary>
    public List<DependentLocationSpec> DependentLocations { get; init; } = new List<DependentLocationSpec>();

    /// <summary>
    /// Dependent items that this Scene creates dynamically at spawn time
    /// Self-contained pattern: Scene generates JSON package → PackageLoader → Standard parsing
    /// Each spec becomes ItemDTO, flows through normal JSON → DTO → Domain pipeline
    /// Scene tracks created item IDs for forensics and cleanup
    /// Empty list = Scene uses only pre-existing items (traditional pattern)
    /// Example: Room keys, special access permits, quest items
    /// </summary>
    public List<DependentItemSpec> DependentItems { get; init; } = new List<DependentItemSpec>();
}
