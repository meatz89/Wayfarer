using Wayfarer.GameState.Enums;

/// <summary>
/// Situation - narrative moment embedded within Scene
/// Part of three-tier timing model: Templates → Scenes/Situations → Actions
/// Situations start Dormant, become Active when player enters context
/// Actions instantiated at query time by SceneFacade, NOT at Scene spawn
/// </summary>
public class Situation
{
    /// <summary>
    /// Unique identifier for the situation
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for the situation
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Narrative description of the situation
    /// </summary>
    public string Description { get; set; }

    // ==================== STATE MACHINE (QUERY-TIME ACTION INSTANTIATION) ====================

    /// <summary>
    /// Lifecycle state controlling action instantiation
    /// Dormant: Situation exists but player hasn't entered context, NO actions instantiated
    /// Active: Player entered location/conversation/route, ChoiceTemplates → Actions created
    /// State transition triggered by SceneFacade when player enters context
    /// </summary>
    public SituationState State { get; set; } = SituationState.Dormant;

    /// <summary>
    /// Indicates this Situation auto-advances without player input
    /// Used when parent Scene has Archetype = AutoAdvance
    /// Situation displays narrative then immediately applies AutoProgressRewards
    /// No player choices created - automatic progression
    /// Tutorial Night Rest scene uses this pattern
    /// </summary>
    public bool IsAutoAdvance { get; set; } = false;

    /// <summary>
    /// Template reference for lazy action instantiation
    /// CRITICAL: Actions NOT created at Scene spawn (Tier 2)
    /// Actions created at query time (Tier 3) by SceneFacade from Template.ChoiceTemplates
    /// Template stored here for on-demand instantiation when State: Dormant → Active
    /// </summary>
    public SituationTemplate Template { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS - which tactical system this situation uses
    /// </summary>
    public TacticalSystemType SystemType { get; set; } = TacticalSystemType.Social;

    /// <summary>
    /// Semantic type marking narrative weight (Normal vs Crisis)
    /// Crisis situations are typically final situations in a Scene that test player preparation
    /// with higher stat requirements or expensive alternatives
    /// </summary>
    public SituationType Type { get; set; } = SituationType.Normal;

    /// <summary>
    /// The deck ID this situation uses for challenge generation
    /// </summary>
    public string DeckId { get; set; }


    /// <summary>
    /// Route ID where this situation's button appears in UI (semantic: placement, not ownership)
    /// Used for route-based situations (scouting, pathfinding) that grant ExplorationCubes
    /// </summary>
    public string PlacementRouteId { get; set; }

    // Scene-Situation Architecture additions (spawn tracking, completion tracking, template system)

    /// <summary>
    /// Template ID this situation was spawned from (for runtime instances)
    /// References the original situation definition used as template
    /// null for non-spawned situations (authored directly in JSON)
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Parent situation ID that spawned this situation (for cascade chains)
    /// null if this is a root situation (not spawned by another)
    /// Enables tracking of situation hierarchies and dependencies
    /// </summary>
    public string ParentSituationId { get; set; }

    /// <summary>
    /// Day when this situation was spawned
    /// null if not yet spawned (template definition)
    /// </summary>
    public int? SpawnedDay { get; set; }

    /// <summary>
    /// Time block when this situation was spawned
    /// null if not yet spawned (template definition)
    /// </summary>
    public TimeBlocks? SpawnedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when this situation was spawned (1-4)
    /// null if not yet spawned (template definition)
    /// </summary>
    public int? SpawnedSegment { get; set; }

    /// <summary>
    /// Day when this situation was completed
    /// null if not yet completed
    /// </summary>
    public int? CompletedDay { get; set; }

    /// <summary>
    /// Time block when this situation was completed
    /// null if not yet completed
    /// </summary>
    public TimeBlocks? CompletedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when this situation was completed (1-4)
    /// null if not yet completed
    /// </summary>
    public int? CompletedSegment { get; set; }

    /// <summary>
    /// Type of interaction when player selects this situation
    /// Determines resolution flow: instant, challenge (Mental/Physical/Social), or navigation
    /// Replaces implicit SystemType checks - explicit interaction type
    /// </summary>
    public SituationInteractionType InteractionType { get; set; } = SituationInteractionType.Instant;

    /// <summary>
    /// Navigation-specific payload for Navigation interaction type
    /// null for non-navigation situations
    /// Contains destination and auto-trigger scene flag
    /// </summary>
    public NavigationPayload NavigationPayload { get; set; }

    /// <summary>
    /// Compound requirement - multiple OR paths to unlock this situation
    /// null or empty = always available (no requirements)
    /// Player needs to satisfy at least ONE complete path
    /// </summary>
    public CompoundRequirement CompoundRequirement { get; set; }

    /// <summary>
    /// Projected bond changes shown to player before selection
    /// Transparent consequence display for relationship impacts
    /// Applied when situation resolves successfully
    /// </summary>
    public List<BondChange> ProjectedBondChanges { get; set; } = new List<BondChange>();

    /// <summary>
    /// Projected scale shifts shown to player before selection
    /// Transparent consequence display for behavioral reputation impacts
    /// Applied when situation resolves successfully
    /// </summary>
    public List<ScaleShift> ProjectedScaleShifts { get; set; } = new List<ScaleShift>();

    /// <summary>
    /// Projected state applications/removals shown to player before selection
    /// Transparent consequence display for temporary condition impacts
    /// Applied when situation resolves successfully
    /// </summary>
    public List<StateApplication> ProjectedStates { get; set; } = new List<StateApplication>();

    /// <summary>
    /// Spawn rules executed when situation succeeds
    /// Creates cascading chains: parent situation → child situations
    /// Each rule spawns new situation at specific placement with adjusted requirements
    /// DECLARATIVE DATA (not event handler - NO EVENTS principle)
    /// </summary>
    public List<SpawnRule> SuccessSpawns { get; set; } = new List<SpawnRule>();

    /// <summary>
    /// Spawn rules executed when situation fails
    /// Failure consequences: different situations spawn based on failure outcome
    /// Enables branching narrative based on success/failure
    /// DECLARATIVE DATA (not event handler - NO EVENTS principle)
    /// </summary>
    public List<SpawnRule> FailureSpawns { get; set; } = new List<SpawnRule>();

    /// <summary>
    /// Situation complexity tier (0-4)
    /// Tier 0: Safety net (zero requirements, infinite repeat, no Resolve cost)
    /// Tier 1: Low complexity (0-3 Resolve, simple requirements)
    /// Tier 2: Standard complexity (5-8 Resolve, moderate compound requirements)
    /// Tier 3: High complexity (10-15 Resolve, complex requirements, deep cascades)
    /// Tier 4: Climactic moments (18-25 Resolve, very complex, resolution content)
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    /// Whether this situation can be repeated after completion
    /// true = Remains available after completion (work, services)
    /// false = Deleted after completion (one-time progression events)
    /// Tier 0 safety net situations MUST be repeatable
    /// </summary>
    public bool Repeatable { get; set; } = false;

    /// <summary>
    /// AI-generated narrative cached for this situation instance
    /// Generated once when situation first appears in scene
    /// null = not yet generated (generate on first display)
    /// Cached to avoid regenerating same text multiple times
    /// </summary>
    public string GeneratedNarrative { get; set; }

    /// <summary>
    /// Hints for AI narrative generation
    /// Provides tone, theme, context, style guidance
    /// Used by AI service to generate appropriate narrative
    /// </summary>
    public NarrativeHints NarrativeHints { get; set; }

    /// <summary>
    /// Object reference to placement location (for runtime navigation)
    /// </summary>
    public Location PlacementLocation { get; set; }

    /// <summary>
    /// Object reference to placement NPC (for runtime navigation)
    /// </summary>
    public NPC PlacementNpc { get; set; }

    /// <summary>
    /// Object reference to parent obligation (for runtime navigation)
    /// </summary>
    public Obligation Obligation { get; set; }

    /// <summary>
    /// Object reference to parent scene (for runtime navigation)
    /// Populated at initialization time from scene's SituationIds
    /// </summary>
    public Scene ParentScene { get; set; }

    /// <summary>
    /// Whether this situation is an obligation intro action
    /// </summary>
    public bool IsIntroAction { get; set; } = false;

    /// <summary>
    /// Category that must match the challenge type's category (for Social system)
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Connection type (token type) for this situation (for Social system)
    /// </summary>
    public ConnectionType ConnectionType { get; set; } = ConnectionType.Trust;

    /// <summary>
    /// Current status of the situation
    /// </summary>
    public SituationStatus Status { get; set; } = SituationStatus.Available;

    /// <summary>
    /// Whether this situation is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Whether this situation has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Whether this situation should be deleted from ActiveSituations on successful completion.
    /// Obligation progression situations: true (one-time, remove after complete)
    /// Repeatable situations: false (persist for retry)
    /// Default: true (obligation progression pattern)
    /// </summary>
    public bool DeleteOnSuccess { get; set; } = true;

    /// <summary>
    /// Resources player must pay to attempt this situation
    /// Transparent costs create resource competition and strategic choices
    /// Board game pattern: Situation A costs 20 Focus, Situation B costs 30 Focus - choose wisely
    /// </summary>
    public SituationCosts Costs { get; set; } = new SituationCosts();

    /// <summary>
    /// Difficulty modifiers that reduce/increase difficulty based on player state
    /// Situation ALWAYS visible, difficulty varies transparently
    /// Multiple paths to reduce difficulty create strategic choices
    /// Example: Understanding 2 reduces Exposure by 3, Familiarity 2 reduces Exposure by 2
    /// No boolean gates: All situations always visible, modifiers just change difficulty
    /// </summary>
    public List<DifficultyModifier> DifficultyModifiers { get; set; } = new List<DifficultyModifier>();

    /// <summary>
    /// Situation cards (tactical layer) - inline victory conditions
    /// Each situation card defines a momentum threshold and rewards
    /// </summary>
    public List<SituationCard> SituationCards { get; set; } = new List<SituationCard>();

    /// <summary>
    /// What consequence occurs when situation succeeds
    /// Resolution: Scene permanently overcome, removed from play
    /// Bypass: Player passes, scene persists
    /// Transform: Scene fundamentally changed, properties set to 0
    /// Modify: Scene properties reduced, other situations may unlock
    /// Grant: Player receives knowledge/items, scene unchanged
    /// </summary>
    public ConsequenceType ConsequenceType { get; set; } = ConsequenceType.Grant;

    /// <summary>
    /// Resolution method this situation sets when completed (for AI narrative context)
    /// </summary>
    public ResolutionMethod SetsResolutionMethod { get; set; } = ResolutionMethod.Unresolved;

    /// <summary>
    /// Relationship outcome this situation sets when completed (affects future interactions)
    /// </summary>
    public RelationshipOutcome SetsRelationshipOutcome { get; set; } = RelationshipOutcome.Neutral;

    /// <summary>
    /// New description for scene if Transform consequence (replaces scene description)
    /// </summary>
    public string TransformDescription { get; set; }

    /// <summary>
    /// Check if this situation is available to attempt
    /// </summary>
    public bool IsAvailableToAttempt()
    {
        return Status == SituationStatus.Available && IsAvailable;
    }

    /// <summary>
    /// Mark this situation as completed
    /// </summary>
    public void Complete()
    {
        Status = SituationStatus.Completed;
        IsCompleted = true;
    }
}
