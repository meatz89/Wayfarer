using Wayfarer.GameState.Enums;

/// <summary>
/// Scene - persistent narrative container spawned from templates
/// ARCHITECTURE: Scenes are PERSISTENT (stored in GameWorld.Scenes)
/// Contains embedded Situations organized by spawn rules defining cascades
/// </summary>
public class Scene
{
    // ==================== IDENTITY PROPERTIES ====================

    /// <summary>
    /// Unique identifier for this Scene instance
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Template identifier - which SceneTemplate spawned this instance
    /// References SceneTemplate in GameWorld.SceneTemplates
    /// null for manually-authored Scenes (not template-spawned)
    /// Used for save/load persistence
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Template reference - composition pattern for runtime access
    /// SceneInstantiator sets this when spawning Scene from SceneTemplate
    /// Enables access to template data without dictionary lookup
    /// NOT serialized - populated from TemplateId during load
    /// </summary>
    public SceneTemplate Template { get; set; }

    // ==================== PLACEMENT PROPERTIES ====================

    /// <summary>
    /// Placement type - where this Scene appears
    /// Location: Appears at specific location
    /// NPC: Appears when talking to specific NPC
    /// Route: Appears when traveling specific route
    /// </summary>
    public PlacementType PlacementType { get; set; }

    /// <summary>
    /// Concrete placement identifier - which entity this Scene is assigned to
    /// LocationId, NpcId, or RouteId depending on PlacementType
    /// Assigned by procedural generation during spawn
    /// </summary>
    public string PlacementId { get; set; }

    // ==================== PRESENTATION PROPERTIES ====================

    /// <summary>
    /// Presentation mode - how this Scene appears to the player
    /// Atmospheric: Scene appears as menu option (existing behavior)
    /// Modal: Scene takes over full screen on location entry (Sir Brante forced moment)
    /// Defaults to Atmospheric for backward compatibility
    /// </summary>
    public PresentationMode PresentationMode { get; set; } = PresentationMode.Atmospheric;

    /// <summary>
    /// Progression mode - how situations within this Scene flow after choices
    /// Breathe: Return to menu after each situation (player-controlled pacing)
    /// Cascade: Continue to next situation immediately (pressure and momentum)
    /// Defaults to Breathe for backward compatibility
    /// </summary>
    public ProgressionMode ProgressionMode { get; set; } = ProgressionMode.Breathe;

    /// <summary>
    /// IsForced - whether this Scene auto-starts on location entry (forced interruption)
    /// true: Scene triggers immediately when player enters location (emergency, critical story beat)
    /// false: Scene waits for player action to trigger (NPC engagement, player-initiated)
    /// Defaults to false - most Modal scenes are player-initiated, not forced
    /// Used by GameScreen.OnInitializedAsync() to filter GetModalSceneAtLocation() results
    /// </summary>
    public bool IsForced { get; set; } = false;

    // ==================== CONTENT PROPERTIES ====================

    /// <summary>
    /// Situation IDs within this Scene
    /// HIGHLANDER Pattern A: Scene stores IDs, GameWorld.Situations is single source of truth
    /// SceneInstantiator creates Situations and adds to GameWorld.Situations, stores IDs here
    /// Query time: Filter GameWorld.Situations.Where(s => scene.SituationIds.Contains(s.Id))
    /// SHALLOW PROVISIONAL: Empty list for provisional scenes (no Situations instantiated yet)
    /// </summary>
    public List<string> SituationIds { get; set; } = new List<string>();

    /// <summary>
    /// Number of situations in this Scene (metadata for provisional scenes)
    /// Populated from Template.SituationTemplates.Count during provisional creation
    /// Enables perfect information display without full Situation instantiation
    /// Used for UI display: "This spawns scene with N situations"
    /// </summary>
    public int SituationCount { get; set; } = 0;

    /// <summary>
    /// Estimated difficulty based on template tier and requirements
    /// Calculated during provisional creation from template properties
    /// Enables rough difficulty preview without full instantiation
    /// Used for UI display: "Estimated difficulty: Medium"
    /// </summary>
    public string EstimatedDifficulty { get; set; } = "Standard";

    /// <summary>
    /// Spawn rules defining how Situations lead into each other
    /// Creates cascade patterns: Linear, HubAndSpoke, Branching, Converging, etc.
    /// Transitions triggered by Choice selection and outcomes
    /// </summary>
    public SituationSpawnRules SpawnRules { get; set; }

    /// <summary>
    /// Current Situation identifier tracking player progress
    /// References Situation.Id in GameWorld.Situations (filtered by this Scene's SituationIds)
    /// Player sees this Situation when they enter placement
    /// </summary>
    public string CurrentSituationId { get; set; }

    // ==================== STATE PROPERTIES ====================

    /// <summary>
    /// Scene state in lifecycle
    /// Provisional: Created eagerly for perfect information, not yet finalized
    /// Active: Available for player interaction
    /// Completed: All relevant Situations finished
    /// </summary>
    public SceneState State { get; set; } = SceneState.Active;

    /// <summary>
    /// Source Situation that spawned this provisional Scene (for cleanup tracking)
    /// Set during provisional Scene creation in SceneInstantiator
    /// When action selected, all provisional Scenes from same Situation are deleted (except finalized ones)
    /// null for finalized Scenes or manually-authored Scenes
    /// </summary>
    public string SourceSituationId { get; set; }

    /// <summary>
    /// Day when this Scene expires (transitions to Expired state)
    /// null = no expiration (Scene remains until completed)
    /// Positive value = Scene expires on this day automatically
    /// Calculated at spawn time from SceneTemplate.ExpirationDays: CurrentDay + ExpirationDays
    /// Enforcement happens in GameFacade.ProcessTimeAdvancement (HIGHLANDER sync point)
    /// Enables time-limited content (rumors, opportunities, urgent requests)
    /// Expired scenes filtered out from SceneFacade queries
    /// </summary>
    public int? ExpiresOnDay { get; set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Generated intro narrative customized to placement
    /// Created during spawn using template + actual entity properties
    /// Example: "As you approach the mill, you notice..." (location-specific)
    /// </summary>
    public string IntroNarrative { get; set; }

    /// <summary>
    /// Archetype classification for this Scene
    /// Linear, HubAndSpoke, Branching, etc.
    /// Matches SpawnRules.Pattern for consistency
    /// </summary>
    public SpawnPattern Archetype { get; set; }

    /// <summary>
    /// Display name for this Scene
    /// Generated from template or manually authored
    /// </summary>
    public string DisplayName { get; set; }

    // ==================== DEPENDENT RESOURCE TRACKING ====================

    /// <summary>
    /// Location IDs created by this Scene through dynamic package generation
    /// Self-contained pattern: Scene generates resources, tracks IDs for forensics
    /// Used for cleanup and debugging - enables answering "which scene created this location?"
    /// Empty list = Scene created no locations (traditional pre-existing location pattern)
    /// </summary>
    public List<string> CreatedLocationIds { get; set; } = new List<string>();

    /// <summary>
    /// Item IDs created by this Scene through dynamic package generation
    /// Self-contained pattern: Scene generates resources, tracks IDs for forensics
    /// Used for cleanup and debugging - enables answering "which scene created this item?"
    /// Empty list = Scene created no items (traditional pre-existing item pattern)
    /// </summary>
    public List<string> CreatedItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Package ID of dynamically generated content package
    /// Forensic identifier enables tracing back to generated JSON
    /// Format: "scene_{sceneId}_package"
    /// null = Scene generated no dynamic package
    /// </summary>
    public string DependentPackageId { get; set; }

    /// <summary>
    /// Marker resolution map for self-contained scenes
    /// Maps template IDs to actual created resource IDs
    /// Key: "generated:{templateId}" marker format
    /// Value: actual resource ID after creation
    /// Example: {"generated:private_room" → "scene_abc123_private_room", "generated:room_key" → "scene_abc123_room_key"}
    /// Populated during FinalizeScene after resource creation
    /// Used by RewardApplicationService and action instantiation to resolve markers
    /// Empty dictionary = no marker resolution needed (traditional pattern)
    /// </summary>
    public Dictionary<string, string> MarkerResolutionMap { get; set; } = new Dictionary<string, string>();

    // ==================== STATE MACHINE METHODS ====================

    /// <summary>
    /// Advance scene to next situation after completing current situation
    /// Queries SpawnRules.Transitions for matching source, updates CurrentSituationId
    /// If no valid transitions, marks scene as complete
    /// DOMAIN RESPONSIBILITY: Scene owns its state machine, not facades
    /// CONTEXT-AWARE: Compares contexts to determine routing (seamless cascade vs exit to world)
    /// </summary>
    /// <param name="completedSituationId">Situation that was just completed</param>
    /// <param name="gameWorld">GameWorld for situation lookup</param>
    /// <returns>Routing decision for UI (ContinueInScene, ExitToWorld, or SceneComplete)</returns>
    public SceneRoutingDecision AdvanceToNextSituation(string completedSituationId, GameWorld gameWorld)
    {
        if (SpawnRules == null || SpawnRules.Transitions == null || SpawnRules.Transitions.Count == 0)
        {
            // No transitions defined - scene complete after first situation
            CurrentSituationId = null;
            State = SceneState.Completed;
            return SceneRoutingDecision.SceneComplete;
        }

        // Find transition from completed situation
        SituationTransition transition = GetTransitionForCompletedSituation(completedSituationId);

        if (transition != null)
        {
            // Valid transition found - get both situations for context comparison
            Situation completedSituation = gameWorld.Situations.FirstOrDefault(s => s.Id == completedSituationId);
            Situation nextSituation = gameWorld.Situations.FirstOrDefault(s => s.Id == transition.DestinationSituationId);

            // Update CurrentSituationId
            CurrentSituationId = transition.DestinationSituationId;

            // Compare contexts to determine routing
            return CompareContexts(completedSituation, nextSituation);
        }
        else
        {
            // No valid transition - scene complete
            CurrentSituationId = null;
            State = SceneState.Completed;
            return SceneRoutingDecision.SceneComplete;
        }
    }

    /// <summary>
    /// Get transition for completed situation
    /// Returns first matching SituationTransition or null if no match
    /// Helper for AdvanceToNextSituation()
    /// </summary>
    /// <param name="completedSituationId">Situation that was just completed</param>
    /// <returns>Matching SituationTransition or null</returns>
    public SituationTransition GetTransitionForCompletedSituation(string completedSituationId)
    {
        if (SpawnRules == null || SpawnRules.Transitions == null)
            return null;

        return SpawnRules.Transitions.FirstOrDefault(t => t.SourceSituationId == completedSituationId);
    }

    /// <summary>
    /// Check if scene is complete
    /// Scene complete when CurrentSituationId is null (no more situations) or State is Completed
    /// </summary>
    /// <returns>True if scene complete, false otherwise</returns>
    public bool IsComplete()
    {
        return CurrentSituationId == null || State == SceneState.Completed;
    }

    /// <summary>
    /// Check if this Scene should activate at given context (location + optional NPC)
    /// Used for multi-situation scene resumption after navigation
    /// Scene resumes if:
    /// - Scene is Active
    /// - CurrentSituationId is set (scene has next situation waiting)
    /// - Current situation's RequiredLocationId matches locationId
    /// - Current situation's RequiredNpcId matches npcId (or both null)
    /// </summary>
    /// <param name="locationId">Location player is currently at</param>
    /// <param name="npcId">NPC player is currently interacting with (null if none)</param>
    /// <param name="gameWorld">GameWorld for situation lookup</param>
    /// <returns>True if scene should resume at this context</returns>
    public bool ShouldActivateAtContext(string locationId, string npcId, GameWorld gameWorld)
    {
        if (State != SceneState.Active)
            return false;

        if (string.IsNullOrEmpty(CurrentSituationId))
            return false;

        Situation currentSituation = gameWorld.Situations.FirstOrDefault(s => s.Id == CurrentSituationId);
        if (currentSituation == null || currentSituation.Template == null)
            return false;

        // MARKER RESOLUTION: Use resolved IDs if present (self-contained scenes)
        // Resolved IDs populated during finalization for markers like "generated:private_room"
        // Fall back to template properties for non-self-contained scenes
        string requiredLocationId = currentSituation.ResolvedRequiredLocationId ?? currentSituation.Template.RequiredLocationId;
        string requiredNpcId = currentSituation.ResolvedRequiredNpcId ?? currentSituation.Template.RequiredNpcId;

        // Check location match
        if (requiredLocationId != locationId)
            return false;

        // Check NPC match (both null = match, both non-null = compare values)
        if (requiredNpcId != npcId)
            return false;

        return true;
    }

    /// <summary>
    /// Compare contexts of two consecutive situations to determine routing
    /// Same context (location + NPC) = ContinueInScene (seamless cascade)
    /// Different context = ExitToWorld (player must navigate)
    /// PRIVATE: Called internally by AdvanceToNextSituation
    /// </summary>
    /// <param name="previousSituation">Situation that was just completed</param>
    /// <param name="nextSituation">Situation about to become current</param>
    /// <returns>Routing decision for UI</returns>
    private SceneRoutingDecision CompareContexts(Situation previousSituation, Situation nextSituation)
    {
        if (previousSituation?.Template == null || nextSituation?.Template == null)
            return SceneRoutingDecision.ExitToWorld;

        // MARKER RESOLUTION: Use resolved IDs if present (self-contained scenes)
        // Compare actual resolved IDs, not template markers
        string prevLocationId = previousSituation.ResolvedRequiredLocationId ?? previousSituation.Template.RequiredLocationId;
        string nextLocationId = nextSituation.ResolvedRequiredLocationId ?? nextSituation.Template.RequiredLocationId;

        string prevNpcId = previousSituation.ResolvedRequiredNpcId ?? previousSituation.Template.RequiredNpcId;
        string nextNpcId = nextSituation.ResolvedRequiredNpcId ?? nextSituation.Template.RequiredNpcId;

        // Compare location context
        bool sameLocation = prevLocationId == nextLocationId;

        // Compare NPC context
        bool sameNpc = prevNpcId == nextNpcId;

        // Same context = seamless cascade, different context = exit to world
        return (sameLocation && sameNpc) ? SceneRoutingDecision.ContinueInScene : SceneRoutingDecision.ExitToWorld;
    }
}

/// <summary>
/// Routing decision for multi-situation scene progression
/// Determines UI behavior after situation completion
/// </summary>
public enum SceneRoutingDecision
{
    /// <summary>
    /// Continue in scene modal - next situation shares same context (location + NPC)
    /// UI reloads modal with next situation without exiting to world
    /// Example: Situations 2→3 both at upper_floor
    /// </summary>
    ContinueInScene,

    /// <summary>
    /// Exit to world - next situation requires different context (location or NPC changed)
    /// Scene persists with updated CurrentSituationId, resumes when player navigates to required context
    /// Example: Situation 1 at common_room → Situation 2 at upper_floor (player must navigate)
    /// </summary>
    ExitToWorld,

    /// <summary>
    /// Scene complete - no more situations
    /// Scene marked as Completed, removed from active scenes
    /// </summary>
    SceneComplete
}

/// <summary>
/// STRONGLY-TYPED REQUIREMENT GAP CLASSES
/// Each class documents its UI execution context and enables type-specific rendering
/// </summary>

/// <summary>
/// Unmet bond requirement - for relationship-gated situations
/// UI Context: Render NPC portrait, bond progress bar, "Talk to X" guidance
/// </summary>
public class UnmetBondRequirement
{
    public string NpcId { get; set; }
    public string NpcName { get; set; }
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet scale requirement - for behavioral reputation gates
/// UI Context: Render scale spectrum visualization, current position marker
/// </summary>
public class UnmetScaleRequirement
{
    public ScaleType ScaleType { get; set; }
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet resolve requirement - for strategic resource gates
/// UI Context: Render progress bar with current/required resolve
/// </summary>
public class UnmetResolveRequirement
{
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet coins requirement - for economic gates
/// UI Context: Render coin amount with "Earn X more coins" guidance
/// </summary>
public class UnmetCoinsRequirement
{
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet situation count requirement - for progression gates
/// UI Context: Render completion counter "Complete X more situations"
/// </summary>
public class UnmetSituationCountRequirement
{
    public int Required { get; set; }
    public int Current { get; set; }
    public int Gap => Required - Current;
}

/// <summary>
/// Unmet achievement requirement - for milestone gates
/// UI Context: Render achievement badge, link to earning situation
/// </summary>
public class UnmetAchievementRequirement
{
    public string AchievementId { get; set; }
    public bool MustHave { get; set; }  // true = must have, false = must NOT have
}

/// <summary>
/// Unmet state requirement - for temporary condition gates
/// UI Context: Render state icon, show how to gain/remove state
/// </summary>
public class UnmetStateRequirement
{
    public StateType StateType { get; set; }
    public bool MustHave { get; set; }  // true = must have state, false = must NOT have state
}

/// <summary>
/// Situation with lock reason - for displaying why a situation is locked
/// Perfect information pattern: player sees what they need to unlock
/// </summary>
public class SituationWithLockReason
{
    /// <summary>
    /// The locked situation
    /// </summary>
    public Situation Situation { get; set; }

    /// <summary>
    /// Human-readable explanation of why this situation is locked
    /// Example: "Requires Bond 15+ with Martha OR Morality +8"
    /// </summary>
    public string LockReason { get; set; }

    /// <summary>
    /// CONTEXTUAL PROPERTIES - each enables different UI execution contexts
    /// UI renders type-specific components based on requirement type
    /// </summary>
    public List<UnmetBondRequirement> UnmetBonds { get; set; } = new List<UnmetBondRequirement>();
    public List<UnmetScaleRequirement> UnmetScales { get; set; } = new List<UnmetScaleRequirement>();
    public List<UnmetResolveRequirement> UnmetResolve { get; set; } = new List<UnmetResolveRequirement>();
    public List<UnmetCoinsRequirement> UnmetCoins { get; set; } = new List<UnmetCoinsRequirement>();
    public List<UnmetSituationCountRequirement> UnmetSituationCount { get; set; } = new List<UnmetSituationCountRequirement>();
    public List<UnmetAchievementRequirement> UnmetAchievements { get; set; } = new List<UnmetAchievementRequirement>();
    public List<UnmetStateRequirement> UnmetStates { get; set; } = new List<UnmetStateRequirement>();
}
