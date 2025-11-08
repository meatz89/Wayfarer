/// <summary>
/// DTO for ChoiceTemplate - one player action option within a SituationTemplate
/// Sir Brante pattern: Requirements and costs VISIBLE, rewards HIDDEN until selected
/// Maps to ChoiceTemplate domain entity
/// </summary>
public class ChoiceTemplateDTO
{
    /// <summary>
    /// Unique identifier within parent SituationTemplate
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Action text template with {placeholders}
    /// Example: "Persuade {NPCName}", "Search {LocationName} for clues"
    /// </summary>
    public string ActionTextTemplate { get; set; }

    /// <summary>
    /// Compound requirement structure (may be null for always-available choices)
    /// Player must satisfy requirements to select this Choice
    /// VISIBLE to player before selection
    /// </summary>
    public CompoundRequirementDTO RequirementFormula { get; set; }

    /// <summary>
    /// Resource cost structure
    /// Coins, Resolve, Time that must be paid to select this Choice
    /// VISIBLE to player before selection
    /// </summary>
    public ChoiceCostDTO CostTemplate { get; set; }

    /// <summary>
    /// Reward structure
    /// Resources, relationships, states, Scene spawning
    /// HIDDEN until Choice is selected (Sir Brante pattern)
    /// </summary>
    public ChoiceRewardDTO RewardTemplate { get; set; }

    /// <summary>
    /// Action classification - what happens when player selects this Choice
    /// Values: "Instant", "StartChallenge", "Navigate"
    /// </summary>
    public string ActionType { get; set; } = "Instant";

    /// <summary>
    /// Challenge identifier (if ActionType is StartChallenge)
    /// References challenge definition for tactical gameplay
    /// null for Instant and Navigate action types
    /// </summary>
    public string ChallengeId { get; set; }

    /// <summary>
    /// Challenge type classification (if ActionType is StartChallenge)
    /// Values: "Social", "Mental", "Physical"
    /// null for Instant and Navigate action types
    /// </summary>
    public string ChallengeType { get; set; }

    /// <summary>
    /// Navigation payload (if ActionType is Navigate)
    /// Destination and auto-trigger configuration
    /// null for Instant and StartChallenge action types
    /// </summary>
    public NavigationPayloadDTO NavigationPayload { get; set; }
}
