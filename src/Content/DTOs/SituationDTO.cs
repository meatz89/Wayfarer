/// <summary>
/// DTO for Situations - strategic layer that defines UI actions
/// Universal across all three challenge types (Social/Mental/Physical)
/// </summary>
public class SituationDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// THREE PARALLEL SYSTEMS - which tactical system this situation uses
    /// </summary>
    public string SystemType { get; set; }

    /// <summary>
    /// Which deck this situation uses for challenge generation
    /// </summary>
    public string DeckId { get; set; }

    /// <summary>
    /// Location ID where this situation's button appears (distributed interaction pattern)
    /// Semantic: WHERE the button is placed, not who owns the situation
    /// </summary>
    public string PlacementLocationId { get; set; }

    /// <summary>
    /// NPC ID where this situation's button appears (Social system situations)
    /// Semantic: WHERE the button is placed, not who owns the situation
    /// </summary>
    public string PlacementNpcId { get; set; }

    /// <summary>
    /// Obligation ID for UI grouping and label display
    /// </summary>
    public string ObligationId { get; set; }

    /// <summary>
    /// Template ID this situation was spawned from (for runtime instances)
    /// References the original situation definition used as template
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Parent situation ID that spawned this situation (for cascade chains)
    /// null if this is a root situation (not spawned by another)
    /// </summary>
    public string ParentSituationId { get; set; }

    /// <summary>
    /// Day when this situation was spawned
    /// </summary>
    public int? SpawnedDay { get; set; }

    /// <summary>
    /// Time block when this situation was spawned
    /// Values: "Morning", "Midday", "Afternoon", "Evening"
    /// </summary>
    public string SpawnedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when this situation was spawned
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
    public string CompletedTimeBlock { get; set; }

    /// <summary>
    /// Segment within time block when this situation was completed
    /// null if not yet completed
    /// </summary>
    public int? CompletedSegment { get; set; }

    /// <summary>
    /// Whether this situation is an obligation intro action
    /// </summary>
    public bool IsIntroAction { get; set; } = false;

    /// <summary>
    /// Whether this situation should be deleted from ActiveSituations on successful completion.
    /// Default: true (obligation progression pattern)
    /// </summary>
    public bool DeleteOnSuccess { get; set; } = true;

    /// <summary>
    /// Situation cards (tactical layer) - inline victory conditions
    /// </summary>
    public List<SituationCardDTO> SituationCards { get; set; } = new List<SituationCardDTO>();

    /// <summary>
    /// Resources player must pay to attempt this situation
    /// Transparent costs create resource competition and strategic choices
    /// </summary>
    public SituationCostsDTO Costs { get; set; }

    /// <summary>
    /// Difficulty modifiers that reduce/increase difficulty based on player state
    /// Multiple paths to reduce difficulty create strategic choices
    /// No boolean gates: All situations always visible, modifiers just change difficulty
    /// </summary>
    public List<DifficultyModifierDTO> DifficultyModifiers { get; set; } = new List<DifficultyModifierDTO>();

    /// <summary>
    /// What consequence this situation has when completed
    /// Values: "Resolution", "Bypass", "Transform", "Modify", "Grant"
    /// </summary>
    public string ConsequenceType { get; set; }

    /// <summary>
    /// Resolution method to set when situation is completed
    /// Values: "Violence", "Diplomacy", "Stealth", "Authority", "Cleverness", "Preparation"
    /// </summary>
    public string ResolutionMethod { get; set; }

    /// <summary>
    /// Relationship outcome to set when situation is completed
    /// Values: "Hostile", "Neutral", "Friendly", "Allied", "Obligated"
    /// </summary>
    public string RelationshipOutcome { get; set; }

    /// <summary>
    /// New description for obstacle after Transform consequence
    /// </summary>
    public string TransformDescription { get; set; }

    // ====================
    // SCENE-SITUATION ARCHITECTURE PROPERTIES
    // ====================

    /// <summary>
    /// Type of interaction when player selects this situation
    /// Values: "Instant", "Mental", "Physical", "Social", "Navigation"
    /// </summary>
    public string InteractionType { get; set; }

    /// <summary>
    /// Navigation-specific payload for Navigation interaction type
    /// null for non-navigation situations
    /// </summary>
    public NavigationPayloadDTO NavigationPayload { get; set; }

    /// <summary>
    /// Compound requirement - multiple OR paths to unlock this situation
    /// null or empty = always available (no requirements)
    /// </summary>
    public CompoundRequirementDTO CompoundRequirement { get; set; }

    /// <summary>
    /// Projected bond changes shown to player before selection
    /// Transparent consequence display for relationship impacts
    /// </summary>
    public List<BondChangeDTO> ProjectedBondChanges { get; set; } = new List<BondChangeDTO>();

    /// <summary>
    /// Projected scale shifts shown to player before selection
    /// Transparent consequence display for behavioral reputation impacts
    /// </summary>
    public List<ScaleShiftDTO> ProjectedScaleShifts { get; set; } = new List<ScaleShiftDTO>();

    /// <summary>
    /// Projected state applications/removals shown to player before selection
    /// Transparent consequence display for temporary condition impacts
    /// </summary>
    public List<StateApplicationDTO> ProjectedStates { get; set; } = new List<StateApplicationDTO>();

    /// <summary>
    /// Spawn rules executed when situation succeeds
    /// Creates cascading chains: parent situation → child situations
    /// DECLARATIVE DATA (not event handler - NO EVENTS principle)
    /// </summary>
    public List<SpawnRuleDTO> SuccessSpawns { get; set; } = new List<SpawnRuleDTO>();

    /// <summary>
    /// Spawn rules executed when situation fails
    /// Failure consequences: different situations spawn based on failure outcome
    /// DECLARATIVE DATA (not event handler - NO EVENTS principle)
    /// </summary>
    public List<SpawnRuleDTO> FailureSpawns { get; set; } = new List<SpawnRuleDTO>();

    /// <summary>
    /// Situation complexity tier (0-4)
    /// Tier 0: Safety net, Tier 1: Low, Tier 2: Standard, Tier 3: High, Tier 4: Climactic
    /// </summary>
    public int Tier { get; set; } = 1;

    /// <summary>
    /// Whether this situation can be repeated after completion
    /// </summary>
    public bool Repeatable { get; set; } = false;

    /// <summary>
    /// AI-generated narrative cached for this situation instance
    /// null = not yet generated
    /// </summary>
    public string GeneratedNarrative { get; set; }

    /// <summary>
    /// Hints for AI narrative generation
    /// </summary>
    public NarrativeHintsDTO NarrativeHints { get; set; }
}
