/// <summary>
/// ActionExecutionPlan - Instructions for GameFacade on how to execute an action
/// PURE DATA - No logic, just validated instructions
/// Executors return this, GameFacade applies it via facades
/// </summary>
public class ActionExecutionPlan
{
    public bool IsValid { get; set; }
    public string FailureReason { get; set; }

    // Strategic costs to consume (GameFacade applies these)
    public int ResolveCoins { get; set; }
    public int CoinsCost { get; set; }
    public int TimeSegments { get; set; }

    // Tutorial resource costs (6-point pools and hunger)
    public int HealthCost { get; set; }
    public int StaminaCost { get; set; }
    public int FocusCost { get; set; }
    public int HungerCost { get; set; }  // Positive value increases hunger (exertion)

    // Consequences to apply (unified pattern for costs and rewards)
    public Consequence Consequence { get; set; }  // Scene-based actions (ChoiceTemplate)
    public ActionRewards DirectRewards { get; set; }  // Atmospheric actions (LocationActionCatalog)

    // Execution routing
    public ChoiceActionType ActionType { get; set; }
    public TacticalSystemType? ChallengeType { get; set; }
    public string ChallengeId { get; set; }
    public NavigationPayload NavigationPayload { get; set; }

    // Action metadata
    public string ActionName { get; set; }
    public bool IsAtmosphericAction { get; set; }  // Pattern discrimination flag

    public static ActionExecutionPlan Invalid(string reason)
    {
        return new ActionExecutionPlan
        {
            IsValid = false,
            FailureReason = reason
        };
    }

    public static ActionExecutionPlan Valid()
    {
        return new ActionExecutionPlan
        {
            IsValid = true
        };
    }
}
