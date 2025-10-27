using System.Collections.Generic;

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
    /// Whether this situation is an obligation intro action
    /// </summary>
    public bool IsIntroAction { get; set; } = false;

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

    /// <summary>
    /// Property reduction to apply to parent obstacle (for Modify consequence)
    /// Reduces obstacle intensity, making other situations easier (NOT unlocking them)
    /// </summary>
    public ObstaclePropertyReductionDTO PropertyReduction { get; set; }
}
