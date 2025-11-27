
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
    /// Choice path type - for scene archetype reward routing (REPLACES ID string matching)
    /// Situation archetypes set this when generating choices
    /// Scene archetypes switch on this for reward enrichment
    /// FORBIDDEN: `if (choice.Id.EndsWith("_stat"))` (ID antipattern)
    /// CORRECT: `if (choice.PathType == ChoicePathType.InstantSuccess)`
    /// </summary>
    public ChoicePathType PathType { get; init; } = ChoicePathType.Fallback;

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
    /// Unified consequence - all costs and rewards in single structure
    /// DESIGN: Negative values = costs, positive values = rewards
    /// Example: Coins = -5 means pay 5 coins, Coins = 10 means gain 10 coins
    /// For Instant actions: Applied immediately on selection
    /// For StartChallenge actions: Applied BEFORE challenge (immediate effects)
    /// May include Scene spawning with categorical placement
    /// </summary>
    public Consequence Consequence { get; init; } = new Consequence();

    /// <summary>
    /// Conditional consequence applied if challenge SUCCEEDS
    /// Only used for StartChallenge action types
    /// Applied AFTER challenge completion when player wins
    /// null = no success-specific consequences (use base Consequence only)
    /// Example: Unlock location on challenge success, grant key item
    /// </summary>
    public Consequence OnSuccessConsequence { get; init; }

    /// <summary>
    /// Conditional consequence applied if challenge FAILS
    /// Only used for StartChallenge action types
    /// Applied AFTER challenge completion when player loses
    /// null = no failure-specific consequences
    /// Example: Apply negative outcomes, spawn follow-up situations
    /// </summary>
    public Consequence OnFailureConsequence { get; init; }

    /// <summary>
    /// Action classification - what happens when player selects this Choice
    /// Instant: Apply consequence immediately
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
    /// Challenge deck identifier (if ActionType is StartChallenge)
    /// Specifies which card deck to use within the tactical system
    /// Set from SituationArchetype.DeckId at parse-time
    /// Social: "friendly_chat" or "desperate_request"
    /// Mental: "mental_challenge"
    /// Physical: "physical_challenge"
    /// string.Empty for situations with no challenge path
    /// null for Instant and Navigate action types
    /// </summary>
    public string DeckId { get; init; }

    /// <summary>
    /// Navigation payload (if ActionType is Navigate)
    /// Destination and auto-trigger configuration
    /// null for Instant and StartChallenge action types
    /// May contain placeholder destination IDs that resolve at spawn time
    /// </summary>
    public NavigationPayload NavigationPayload { get; init; }
}
