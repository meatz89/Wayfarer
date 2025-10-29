using Wayfarer.GameState.Enums;

/// <summary>
/// Template for Choice - immutable archetype defining one player action option
/// Used by SituationTemplate to define 2-4 choice archetypes
/// At spawn time, instantiated to Choice instances with placeholders replaced
/// Implements composition pattern: Choice instances reference ChoiceTemplate, never clone
/// </summary>
public class ChoiceTemplate
{
    /// <summary>
    /// Unique identifier for this ChoiceTemplate
    /// Used to track which template spawned which Choice instance
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Action text TEMPLATE with placeholders
    /// Example: "Persuade {NPCName}", "Search {LocationName} for clues"
    /// Placeholders replaced at spawn time with actual entity names
    /// Enables one template to generate varied text based on placement
    /// </summary>
    public string ActionTextTemplate { get; init; }

    /// <summary>
    /// Requirement FORMULA structure (not calculated values)
    /// Uses categorical baseValue enums (CurrentPlayerBond, PlayerResolve, etc.)
    /// Offsets applied at spawn time based on tier/difficulty
    /// Example: "CurrentPlayerBond + 3" becomes actual threshold when instantiated
    /// </summary>
    public CompoundRequirement RequirementFormula { get; init; }

    /// <summary>
    /// Cost template structure
    /// May use categorical values that scale at spawn time
    /// Example: "Medium" cost becomes actual number based on tier
    /// Or concrete values if costs are fixed
    /// </summary>
    public ChoiceCost CostTemplate { get; init; } = new ChoiceCost();

    /// <summary>
    /// Reward template structure
    /// Defines what rewards to apply when Choice selected
    /// May include Scene spawning templates with PlacementRelation
    /// </summary>
    public ChoiceReward RewardTemplate { get; init; } = new ChoiceReward();

    /// <summary>
    /// Action classification - what happens when player selects this Choice
    /// Instant: Apply cost and reward immediately
    /// StartChallenge: Enter tactical challenge
    /// Navigate: Move to new location/NPC/route
    /// Same as runtime Choice.ActionType (no translation needed)
    /// </summary>
    public ChoiceActionType ActionType { get; init; } = ChoiceActionType.Instant;

    /// <summary>
    /// Challenge identifier (if ActionType is StartChallenge)
    /// References challenge definition for tactical gameplay
    /// null for Instant and Navigate action types
    /// Same as runtime Choice.ChallengeId (no translation needed)
    /// </summary>
    public string ChallengeId { get; init; }

    /// <summary>
    /// Challenge type classification (if ActionType is StartChallenge)
    /// Social, Mental, or Physical tactical system
    /// null for Instant and Navigate action types
    /// Same as runtime Choice.ChallengeType (no translation needed)
    /// </summary>
    public TacticalSystemType? ChallengeType { get; init; }

    /// <summary>
    /// Navigation payload (if ActionType is Navigate)
    /// Destination and auto-trigger configuration
    /// null for Instant and StartChallenge action types
    /// May contain placeholder destination IDs that resolve at spawn time
    /// </summary>
    public NavigationPayload NavigationPayload { get; init; }
}
