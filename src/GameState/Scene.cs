
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

    // ==================== PLACEMENT PROPERTIES (SYSTEM 5 OUTPUT) ====================

    /// <summary>
    /// Location where this Scene appears
    /// DIRECT OBJECT REFERENCE - no PlacementType enum, no string ID dispatch
    /// Set by PackageLoader (System 5) after EntityResolver (System 4) returns concrete object
    /// Flow: Categories (System 2) → JSON spec (System 3) → FindOrCreate (System 4) → Object reference (System 5)
    /// Most scenes have Location (conversation, investigation, ambient)
    /// null only for abstract/conceptual scenes (rare)
    /// </summary>
    public Location Location { get; set; }

    /// <summary>
    /// NPC associated with this Scene (conversation/interaction partner)
    /// DIRECT OBJECT REFERENCE - set by PackageLoader after entity resolution
    /// null for location-only scenes (ambient discoveries, environmental events)
    /// Example: Conversation AT tavern WITH Elena → scene.Location = tavern, scene.Npc = Elena
    /// Enables direct navigation: scene.Npc.Name, scene.Npc.PersonalityType
    /// </summary>
    public NPC Npc { get; set; }

    /// <summary>
    /// Route associated with this Scene (travel/journey context)
    /// DIRECT OBJECT REFERENCE - set by PackageLoader after entity resolution
    /// null for most scenes (only route-specific events use this)
    /// Example: Bandit ambush ON mountain road → scene.Location = ambush site, scene.Route = mountain road
    /// Enables direct navigation: scene.Route.DangerRating, scene.Route.TerrainTypes
    /// </summary>
    public RouteOption Route { get; set; }

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

    // ==================== CONTENT PROPERTIES ====================

    /// <summary>
    /// Situations owned by this Scene
    /// Scene owns its Situations directly (like Car owns Wheels)
    /// SceneInstantiator creates Situations and adds to this collection
    /// SHALLOW PROVISIONAL: Empty list for provisional scenes (no Situations instantiated yet)
    /// </summary>
    public List<Situation> Situations { get; set; } = new List<Situation>();

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
    /// Current Situation tracking player progress
    /// Direct object reference (no ID lookup needed)
    /// Player sees this Situation when they enter placement
    /// null = scene not started or completed
    /// </summary>
    public Situation CurrentSituation { get; set; }

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

    /// <summary>
    /// Story category classification (copied from template)
    /// MainStory = A-story progression (sequential A1-A10, then procedural A11+)
    /// SideStory = B-story content (optional, unlocked by A-story)
    /// Service = C-story content (repeatable transactional)
    /// </summary>
    public StoryCategory Category { get; set; } = StoryCategory.SideStory;

    /// <summary>
    /// Main story sequence number (copied from template)
    /// 1-10 = Authored tutorial scenes
    /// 11+ = Procedural continuation (infinite)
    /// null = Not part of A-story
    /// </summary>
    public int? MainStorySequence { get; set; }

    // ==================== DEPENDENT RESOURCE DISCOVERY (QUERY-BASED) ====================

    // Scene does NOT track "what I created" via explicit lists
    // Query world state directly: gameWorld.Locations.Where(loc => loc.OwningSceneId == scene.Id)
    // Requires generated entities to have OwningSceneId property set during creation
    // Pattern: Generate → Set owner relationship → Query by relationship when needed
    // Benefits: No stale tracking lists, single source of truth (entity ownership property)

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
    public SceneRoutingDecision AdvanceToNextSituation(Situation completedSituation)
    {
        Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{Id}' advancing from situation '{completedSituation.Id}'");

        if (SpawnRules == null || SpawnRules.Transitions == null || SpawnRules.Transitions.Count == 0)
        {
            // No transitions defined - scene complete after first situation
            Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{Id}' has no transitions - marking as complete");
            CurrentSituation = null;
            State = SceneState.Completed;
            return SceneRoutingDecision.SceneComplete;
        }

        // Find transition from completed situation (evaluates conditions)
        SituationTransition transition = GetTransitionForCompletedSituation(completedSituation);

        if (transition != null)
        {
            // Valid transition found - find next situation by TemplateId match
            Situation nextSituation = Situations
                .FirstOrDefault(s => s.TemplateId == transition.DestinationSituationId);

            // Update CurrentSituation (direct object reference)
            CurrentSituation = nextSituation;
            Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{Id}' advanced to situation '{(nextSituation != null ? nextSituation.Id : "NULL")}'");

            // Compare contexts to determine routing
            SceneRoutingDecision decision = CompareContexts(completedSituation, nextSituation);
            Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{Id}' routing decision: {decision}");
            return decision;
        }
        else
        {
            // No valid transition - scene complete
            Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{Id}' has no valid transition - marking as complete");
            CurrentSituation = null;
            State = SceneState.Completed;
            return SceneRoutingDecision.SceneComplete;
        }
    }

    /// <summary>
    /// Get transition for completed situation
    /// Evaluates TransitionCondition to determine which transition applies
    /// Supports conditional branching based on choice selection and challenge outcome
    /// Helper for AdvanceToNextSituation()
    /// </summary>
    /// <param name="completedSituation">Situation that was just completed (with outcome tracking)</param>
    /// <returns>Matching SituationTransition based on evaluated conditions, or null if no match</returns>
    public SituationTransition GetTransitionForCompletedSituation(Situation completedSituation)
    {
        if (SpawnRules == null || SpawnRules.Transitions == null || completedSituation == null)
            return null;

        // Find all transitions from this source situation
        // CRITICAL: Use TemplateId for matching (HIGHLANDER Pattern D)
        // Template transitions reference template IDs, runtime situations have instance IDs
        // TemplateId bridges template-defined rules to runtime instances
        List<SituationTransition> candidateTransitions = SpawnRules.Transitions
            .Where(t => t.SourceSituationId == completedSituation.TemplateId)
            .ToList();

        if (candidateTransitions.Count == 0)
            return null;

        // Evaluate conditions in priority order:
        // 1. OnChoice (most specific)
        // 2. OnSuccess/OnFailure (outcome-based)
        // 3. Always (fallback)

        // Check OnChoice transitions first (most specific)
        if (completedSituation.LastChoiceId != null)
        {
            SituationTransition choiceTransition = candidateTransitions
                .FirstOrDefault(t => t.Condition == TransitionCondition.OnChoice
                                  && t.SpecificChoiceId == completedSituation.LastChoiceId);
            if (choiceTransition != null)
                return choiceTransition;
        }

        // Check OnSuccess/OnFailure transitions (challenge outcome)
        if (completedSituation.LastChallengeSucceeded.HasValue)
        {
            TransitionCondition targetCondition = completedSituation.LastChallengeSucceeded.Value
                ? TransitionCondition.OnSuccess
                : TransitionCondition.OnFailure;

            SituationTransition outcomeTransition = candidateTransitions
                .FirstOrDefault(t => t.Condition == targetCondition);
            if (outcomeTransition != null)
                return outcomeTransition;
        }

        // Fallback to Always transition
        return candidateTransitions.FirstOrDefault(t => t.Condition == TransitionCondition.Always);
    }

    /// <summary>
    /// Check if scene is complete
    /// Scene complete when CurrentSituationId is null (no more situations) or State is Completed
    /// </summary>
    /// <returns>True if scene complete, false otherwise</returns>
    public bool IsComplete()
    {
        return CurrentSituation == null || State == SceneState.Completed;
    }

    /// <summary>
    /// Check if this Scene should activate at given context (location + optional NPC)
    /// Used for multi-situation scene resumption after navigation
    /// Scene resumes if:
    /// - Scene is Active
    /// - CurrentSituation is set (scene has next situation waiting)
    /// - Current situation's RequiredLocationId matches locationId
    /// - Current situation's RequiredNpcId matches npcId (or both null)
    /// </summary>
    /// <param name="locationId">Location player is currently at</param>
    /// <param name="npcId">NPC player is currently interacting with (null if none)</param>
    /// <returns>True if scene should resume at this context</returns>
    public bool ShouldActivateAtContext(string locationId, string npcId)
    {
        Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' checking activation at location '{locationId}', npc '{npcId}'");

        if (State != SceneState.Active)
        {
            Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' rejected - State is {State}, not Active");
            return false;
        }

        if (CurrentSituation == null)
        {
            Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' rejected - CurrentSituation is null");
            return false;
        }

        if (CurrentSituation.Template == null)
        {
            Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' rejected - CurrentSituation template is null");
            return false;
        }

        // Use resolved IDs if present, fall back to template properties
        string requiredLocationId = CurrentSituation.ResolvedRequiredLocationId ?? CurrentSituation.Template.RequiredLocationId;
        string requiredNpcId = CurrentSituation.ResolvedRequiredNpcId ?? CurrentSituation.Template.RequiredNpcId;

        Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' requires location '{requiredLocationId}', npc '{requiredNpcId}' | Player at '{locationId}', '{npcId}'");

        // Check location match
        if (requiredLocationId != locationId)
        {
            Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' rejected - Location mismatch");
            return false;
        }

        // Check NPC match (both null = match, both non-null = compare values)
        if (requiredNpcId != npcId)
        {
            Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' rejected - NPC mismatch");
            return false;
        }

        Console.WriteLine($"[Scene.ShouldActivateAtContext] Scene '{Id}' ACTIVATED - All conditions met!");
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

        // Use resolved IDs if present, fall back to template properties
        string prevLocationId = previousSituation.ResolvedRequiredLocationId ?? previousSituation.Template.RequiredLocationId;
        string nextLocationId = nextSituation.ResolvedRequiredLocationId ?? nextSituation.Template.RequiredLocationId;

        string prevNpcId = previousSituation.ResolvedRequiredNpcId ?? previousSituation.Template.RequiredNpcId;
        string nextNpcId = nextSituation.ResolvedRequiredNpcId ?? nextSituation.Template.RequiredNpcId;

        Console.WriteLine($"[Scene.CompareContexts] Previous: location='{prevLocationId}', npc='{prevNpcId}'");
        Console.WriteLine($"[Scene.CompareContexts] Next: location='{nextLocationId}', npc='{nextNpcId}'");

        // Compare location context
        bool sameLocation = prevLocationId == nextLocationId;

        // Compare NPC context
        bool sameNpc = prevNpcId == nextNpcId;

        Console.WriteLine($"[Scene.CompareContexts] sameLocation={sameLocation}, sameNpc={sameNpc}");

        // Same context = seamless cascade, different context = exit to world
        SceneRoutingDecision decision = (sameLocation && sameNpc) ? SceneRoutingDecision.ContinueInScene : SceneRoutingDecision.ExitToWorld;
        Console.WriteLine($"[Scene.CompareContexts] Final decision: {decision}");
        return decision;
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
