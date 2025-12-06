
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
    /// Referenced by Consequence.ScenesToSpawn and Obligation phase rewards
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
    /// Scene archetype type - the mechanical pattern for this scene.
    /// HIGHLANDER: ONE enum for ALL scene archetypes (encounter + narrative patterns).
    /// Used for anti-repetition tracking and narrative generation context.
    ///
    /// PRINCIPLE: This is a TYPE discriminator, not an ID (arc42 §8.3).
    /// All scenes have an archetype - it defines structure (situation count, transitions).
    /// Archetype is selected either explicitly (authored) or categorically (procedural).
    ///
    /// Distinct from SpawnPattern "Archetype" property which describes transition patterns.
    /// </summary>
    public SceneArchetypeType SceneArchetype { get; init; }

    /// <summary>
    /// Display name template for this Scene
    /// May contain placeholders like {NPCName}, {LocationName}
    /// Replaced at spawn time with actual entity names
    /// Example: "The Plea of {NPCName}", "Investigating {LocationName}"
    /// </summary>
    public string DisplayNameTemplate { get; init; }

    // ==================== ACTIVATION FILTERS (TRIGGER CONDITIONS) ====================
    // Determines WHEN scene activates (Deferred → Active transition)
    // Separate from situation placement filters (which determine WHERE situations happen)
    // Copied to Scene instance at spawn time, evaluated BEFORE entity resolution

    /// <summary>
    /// Location activation filter - categorical properties that trigger scene activation
    /// Scene activates when player enters location matching these categorical properties
    /// null = no location-based activation (use NPC or other trigger)
    /// Evaluated before entity resolution (categorical matching: Privacy, Safety, Activity, Purpose)
    /// Separate from Situation.LocationFilter which determines WHERE situation happens (always explicit per-situation)
    /// Copied to Scene.LocationActivationFilter at spawn time
    /// </summary>
    public PlacementFilter LocationActivationFilter { get; init; }

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
    /// Intro narrative template with placeholders
    /// Generated narrative shown when Scene first appears
    /// Example: "As you enter {LocationName}, you notice {NPCName} looks distressed..."
    /// Placeholders replaced at spawn time with actual entity properties
    /// If null/empty, SceneInstantiator may use AI generation with NarrativeHints
    /// </summary>
    public string IntroNarrativeTemplate { get; init; }

    /// <summary>
    /// Story category classification for narrative role
    /// MainStory = A-story progression (sequential A1-A10, then procedural A11+)
    /// SideStory = B-story content (reward threads from A-story success)
    /// Encounter = C-story content (natural emergence from journey)
    /// Defaults to SideStory for backward compatibility with existing content
    /// </summary>
    public StoryCategory Category { get; init; } = StoryCategory.SideStory;

    /// <summary>
    /// Main story sequence number for A-story scenes
    /// 1-10 = Authored tutorial scenes (A1-A10, sequential progression)
    /// 11+ = Procedural continuation (A11, A12, A13... infinity)
    /// null = Not part of A-story (SideStory or Encounter content)
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
    /// Whether this scene is created as Deferred at game start
    /// true = Scene created as Deferred during SpawnStarterScenes (e.g., A1 tutorial)
    /// false = Scene only created when ScenesToSpawn reward fires (e.g., A2, A3)
    /// Defaults to false - most scenes are NOT created at game start
    /// Only A1 (isStarter: true) should be created at startup; other A-story scenes
    /// remain as Templates until triggered by ScenesToSpawn rewards
    /// </summary>
    public bool IsStarter { get; init; } = false;

    /// <summary>
    /// Sir Brante-style narrative rhythm classification.
    /// Determines choice generation pattern and consequence polarity.
    /// Building = All positive outcomes, stat grants (A1 tutorial, recovery)
    /// Crisis = Damage mitigation, fallback has penalty (A3 crisis, high stakes)
    /// Mixed = Standard trade-offs (normal gameplay)
    /// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
    /// </summary>
    public RhythmPattern RhythmPattern { get; init; }

}
