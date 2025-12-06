
/// <summary>
/// Scene - persistent narrative container spawned from templates
/// ARCHITECTURE: Scenes are PERSISTENT (stored in GameWorld.Scenes)
/// Contains embedded Situations organized by spawn rules defining cascades
/// </summary>
public class Scene
{
    // ==================== IDENTITY PROPERTIES ====================

    // HIGHLANDER: NO Id property - Scene is identified by object reference
    // Name property provides display identifier if needed

    /// <summary>
    /// Template identifier - which SceneTemplate spawned this instance
    /// References SceneTemplate in GameWorld.SceneTemplates
    /// null for manually-authored Scenes (not template-spawned)
    /// Used for save/load persistence
    /// EXCEPTION: Template IDs are acceptable (immutable archetypes)
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Template reference - composition pattern for runtime access
    /// SceneInstantiator sets this when spawning Scene from SceneTemplate
    /// Enables access to template data without dictionary lookup
    /// NOT serialized - populated from TemplateId during load
    /// </summary>
    public SceneTemplate Template { get; set; }

    // ==================== PLACEMENT MOVED TO SITUATION ====================
    // ARCHITECTURAL CHANGE: Placement is per-situation, not per-scene
    // Multi-situation scenes require each situation to have its own location/NPC/route
    // Example: "Inn Service" scene has three situations at different locations:
    //   - Situation 1: "Negotiate" at Common Room with Innkeeper
    //   - Situation 2: "Rest" at Private Room with no NPC
    //   - Situation 3: "Depart" at Exit with no NPC
    // Scene is narrative container with no specific placement
    // Each Situation has Location/Npc/Route properties

    // ==================== ACTIVATION FILTERS (TRIGGER CONDITIONS) ====================
    // Determines WHEN scene activates (Deferred → Active transition)
    // Evaluated BEFORE entity resolution using categorical matching only
    // Separate from situation placement filters (evaluated AFTER activation)

    /// <summary>
    /// Location activation filter - categorical properties that trigger scene activation
    /// CheckAndActivateDeferredScenes evaluates this filter against player's current location
    /// null = no location-based activation (use NPC or other trigger)
    /// Evaluated before entity resolution (categorical matching: Privacy, Safety, Activity, Purpose)
    /// Separate from Situation.LocationFilter which determines WHERE situation happens (always explicit per-situation)
    /// Copied from SceneTemplate.LocationActivationFilter at spawn time
    /// </summary>
    public PlacementFilter LocationActivationFilter { get; set; }

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
    /// Index of current situation in Situations list
    /// Tracks player progression through sequential situations
    /// 0-based index: 0 = first situation, Situations.Count = scene complete
    /// When CurrentSituationIndex >= Situations.Count, scene is complete
    /// NEW ARCHITECTURE: Index-based progression replaces CurrentSituation object pointer
    /// </summary>
    public int CurrentSituationIndex { get; set; } = 0;

    /// <summary>
    /// Current Situation derived from CurrentSituationIndex
    /// COMPUTED PROPERTY: Not stored, derived from index on every access
    /// Returns null if index out of bounds (scene complete or not started)
    /// Single source of truth is CurrentSituationIndex, this is convenience accessor
    /// </summary>
    public Situation CurrentSituation =>
        CurrentSituationIndex >= 0 && CurrentSituationIndex < Situations.Count
            ? Situations[CurrentSituationIndex]
            : null;

    // ==================== STATE PROPERTIES ====================

    /// <summary>
    /// Scene state in lifecycle
    /// Deferred: Created but not yet activated, dependent resources not spawned
    /// Active: Available for player interaction, dependent resources spawned
    /// Completed: All relevant Situations finished
    /// </summary>
    public SceneState State { get; set; } = SceneState.Deferred;

    /// <summary>
    /// Source Situation that spawned this deferred Scene (for cleanup tracking)
    /// Set during deferred Scene creation in SceneInstantiator
    /// When action selected, all deferred Scenes from same Situation are deleted (except activated ones)
    /// null for activated Scenes or manually-authored Scenes
    /// HIGHLANDER: Object reference ONLY, no SourceSituationId
    /// </summary>
    public Situation SourceSituation { get; set; }

    /// <summary>
    /// Day when this Scene expires (transitions to Expired state)
    /// null = no expiration (Scene remains until completed)
    /// Positive value = Scene expires on this day automatically
    /// Calculated at spawn time from SceneTemplate.ExpirationDays: CurrentDay + ExpirationDays
    /// Enforcement happens in GameOrchestrator.ProcessTimeAdvancement (HIGHLANDER sync point)
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
    /// SideStory = B-story content (reward threads from A-story success)
    /// Encounter = C-story content (natural emergence from journey)
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
    /// HIGHLANDER: All flow control through Consequence (see arc42 §8.30)
    /// - NextSituationTemplateId: explicit next situation
    /// - IsTerminal: explicit scene end
    /// - Sequential fallback: next in list if no explicit flow
    /// DOMAIN RESPONSIBILITY: Scene owns its state machine, not facades
    /// CONTEXT-AWARE: Compares contexts to determine routing (seamless cascade vs exit to world)
    /// </summary>
    /// <param name="completedSituation">Situation that was just completed</param>
    /// <param name="executedConsequence">Consequence that was executed (contains flow control)</param>
    /// <returns>Routing decision for UI (ContinueInScene, ExitToWorld, or SceneComplete)</returns>
    public SceneRoutingDecision AdvanceToNextSituation(Situation completedSituation, Consequence executedConsequence)
    {
        // HIGHLANDER: Scene has NO Id, Situation has NO Id - use TemplateId or Name
        Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{TemplateId}' advancing from situation '{completedSituation.Name}'");

        // CHOICE-DRIVEN FLOW: Check consequence for explicit flow control
        // Priority: 1. IsTerminal, 2. NextSituationTemplateId, 3. Sequential fallback

        // 1. IsTerminal - explicit scene end
        if (executedConsequence?.IsTerminal == true)
        {
            Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{TemplateId}' - consequence IsTerminal=true - marking as complete");
            CurrentSituationIndex = Situations.Count; // Out of bounds = complete
            State = SceneState.Completed;
            return SceneRoutingDecision.SceneComplete;
        }

        // 2. NextSituationTemplateId - explicit next situation
        if (!string.IsNullOrEmpty(executedConsequence?.NextSituationTemplateId))
        {
            Situation nextSituation = Situations
                .FirstOrDefault(s => s.TemplateId == executedConsequence.NextSituationTemplateId);

            // FAIL-FAST: If consequence references non-existent situation, this is data error
            if (nextSituation == null)
            {
                throw new InvalidOperationException(
                    $"Scene '{TemplateId}' consequence references NextSituationTemplateId '{executedConsequence.NextSituationTemplateId}' which does not exist in Situations collection");
            }

            // Update CurrentSituationIndex
            CurrentSituationIndex = Situations.IndexOf(nextSituation);
            Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{TemplateId}' advanced to explicit situation '{nextSituation.Name}' (index {CurrentSituationIndex})");

            // Compare contexts to determine routing
            SceneRoutingDecision decision = CompareContexts(completedSituation, nextSituation);
            Console.WriteLine($"[Scene.AdvanceToNextSituation] Scene '{TemplateId}' routing decision: {decision}");
            return decision;
        }

        // HIGHLANDER VIOLATION: No explicit flow control - this is a data/generation error (arc42 §8.30)
        // Every choice consequence MUST have either NextSituationTemplateId or IsTerminal=true
        // NO SEQUENTIAL FALLBACK - all flow must be explicit
        throw new InvalidOperationException(
            $"Scene '{TemplateId}' situation '{completedSituation.Name}' completed but consequence has no explicit flow control. " +
            "Every choice consequence must set NextSituationTemplateId (continue) or IsTerminal=true (end scene). " +
            "Check archetype generators for missing flow control. (arc42 §8.30)");
    }

    // HIGHLANDER: GetTransitionForCompletedSituation method REMOVED (see arc42 §8.30)
    // All flow control through Consequence.NextSituationTemplateId and IsTerminal
    // Different choices can now lead to different situations within the same scene

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
    /// Check if this Scene should resume at given context (location + optional NPC)
    /// Used for multi-situation scene resumption after navigation
    /// Scene resumes if:
    /// - Scene is Active
    /// - CurrentSituation is set (scene has next situation waiting)
    /// - Current situation's Location matches location
    /// - Current situation's Npc matches npc (or both null)
    /// HIGHLANDER: Accept Location and NPC objects, compare objects directly
    /// </summary>
    /// <param name="location">Location player is currently at</param>
    /// <param name="npc">NPC player is currently interacting with (null if none)</param>
    /// <returns>True if scene should resume at this context</returns>
    public bool ShouldResumeAtContext(Location location, NPC npc)
    {
        // HIGHLANDER: Scene has NO Id - use TemplateId for logging
        // ZERO NULL TOLERANCE: location/npc can be null (player at location with no NPC), format accordingly
        string locationName = location != null ? location.Name : "nowhere";
        string npcName = npc != null ? npc.Name : "no-one";
        Console.WriteLine($"[Scene.ShouldResumeAtContext] Scene '{TemplateId}' checking resumption at location '{locationName}', npc '{npcName}'");

        if (State != SceneState.Active)
        {
            Console.WriteLine($"[Scene.ShouldResumeAtContext] Scene '{TemplateId}' rejected - State is {State}, not Active");
            return false;
        }

        if (CurrentSituation == null)
        {
            Console.WriteLine($"[Scene.ShouldResumeAtContext] Scene '{TemplateId}' rejected - CurrentSituation is null");
            return false;
        }

        // ZERO NULL TOLERANCE: CurrentSituation.Template guaranteed non-null for active situations
        if (CurrentSituation.Template == null)
        {
            throw new InvalidOperationException(
                $"Scene '{TemplateId}' has CurrentSituation with null Template - violates active situation architecture");
        }

        // HIGHLANDER: Direct object comparison
        Location requiredLocation = CurrentSituation.Location;
        NPC requiredNpc = CurrentSituation.Npc;

        string reqLocationName = requiredLocation != null ? requiredLocation.Name : "any";
        string reqNpcName = requiredNpc != null ? requiredNpc.Name : "any";
        Console.WriteLine($"[Scene.ShouldResumeAtContext] Scene '{TemplateId}' requires location '{reqLocationName}', npc '{reqNpcName}' | Player at '{locationName}', '{npcName}'");

        // Check location match - object equality
        if (requiredLocation != location)
        {
            Console.WriteLine($"[Scene.ShouldResumeAtContext] Scene '{TemplateId}' rejected - Location mismatch");
            return false;
        }

        // Check NPC match - object equality (both null = match, both non-null = compare objects)
        if (requiredNpc != npc)
        {
            Console.WriteLine($"[Scene.ShouldResumeAtContext] Scene '{TemplateId}' rejected - NPC mismatch");
            return false;
        }

        Console.WriteLine($"[Scene.ShouldResumeAtContext] Scene '{TemplateId}' RESUMED - All conditions met!");
        return true;
    }

    /// <summary>
    /// Compare contexts of two consecutive situations to determine routing
    /// Same context (location + NPC) = ContinueInScene (seamless cascade)
    /// Different context = ExitToWorld (player must navigate)
    /// PRIVATE: Called internally by AdvanceToNextSituation
    /// HIGHLANDER: Compare Location and NPC objects directly
    /// </summary>
    /// <param name="previousSituation">Situation that was just completed</param>
    /// <param name="nextSituation">Situation about to become current</param>
    /// <returns>Routing decision for UI</returns>
    private SceneRoutingDecision CompareContexts(Situation previousSituation, Situation nextSituation)
    {
        // ZERO NULL TOLERANCE: Both situations guaranteed non-null by caller (AdvanceToNextSituation validates)
        // Templates guaranteed non-null for spawned situations
        if (previousSituation.Template == null || nextSituation.Template == null)
        {
            throw new InvalidOperationException(
                $"Scene '{TemplateId}' comparing situations with null Templates - violates situation architecture");
        }

        // HIGHLANDER: Direct object references
        Location prevLocation = previousSituation.Location;
        Location nextLocation = nextSituation.Location;

        NPC prevNpc = previousSituation.Npc;
        NPC nextNpc = nextSituation.Npc;

        string prevLocName = prevLocation != null ? prevLocation.Name : "any";
        string prevNpcName = prevNpc != null ? prevNpc.Name : "any";
        string nextLocName = nextLocation != null ? nextLocation.Name : "any";
        string nextNpcName = nextNpc != null ? nextNpc.Name : "any";

        Console.WriteLine($"[Scene.CompareContexts] Previous: location='{prevLocName}', npc='{prevNpcName}'");
        Console.WriteLine($"[Scene.CompareContexts] Next: location='{nextLocName}', npc='{nextNpcName}'");

        // Compare location context - object equality
        bool sameLocation = prevLocation == nextLocation;

        // Compare NPC context - object equality
        bool sameNpc = prevNpc == nextNpc;

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
/// HIGHLANDER: Stores NPC object, not string IDs
/// </summary>
public class UnmetBondRequirement
{
    public NPC Npc { get; set; }
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
/// HIGHLANDER: Stores Achievement object, not string ID
/// </summary>
public class UnmetAchievementRequirement
{
    public Achievement Achievement { get; set; }
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
