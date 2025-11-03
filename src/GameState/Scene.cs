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

    // ==================== STATE MACHINE METHODS ====================

    /// <summary>
    /// Advance scene to next situation after completing current situation
    /// Queries SpawnRules.Transitions for matching source, updates CurrentSituationId
    /// If no valid transitions, marks scene as complete
    /// DOMAIN RESPONSIBILITY: Scene owns its state machine, not facades
    /// </summary>
    /// <param name="completedSituationId">Situation that was just completed</param>
    public void AdvanceToNextSituation(string completedSituationId)
    {
        if (SpawnRules == null || SpawnRules.Transitions == null || SpawnRules.Transitions.Count == 0)
        {
            // No transitions defined - scene complete after first situation
            CurrentSituationId = null;
            State = SceneState.Completed;
            return;
        }

        // Find transition from completed situation
        SituationTransition transition = GetTransitionForCompletedSituation(completedSituationId);

        if (transition != null)
        {
            // Valid transition found - advance to destination situation
            CurrentSituationId = transition.DestinationSituationId;
        }
        else
        {
            // No valid transition - scene complete
            CurrentSituationId = null;
            State = SceneState.Completed;
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
