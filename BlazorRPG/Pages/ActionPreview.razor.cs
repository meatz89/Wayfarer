using Microsoft.AspNetCore.Components;

public partial class ActionPreviewBase : ComponentBase
{
    [Parameter] public UserActionOption CurrentAction { get; set; }
    [Parameter] public GameState GameState { get; set; }
    [Parameter] public EventCallback<bool> OnActionConfirmed { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }

    protected string GetCardCostClass(CardTypes costType)
    {
        return costType switch
        {
            CardTypes.Physical => "physical",
            CardTypes.Intellectual => "intellectual",
            CardTypes.Social => "social",
            _ => ""
        };
    }
    public string GetActionName()
    {
        ActionImplementation action = CurrentAction.ActionImplementation;

        string name = $"{action.ActionType} - {action.Name}";
        return name;
    }

    public string GetActionDescription()
    {
        ActionImplementation action = CurrentAction.ActionImplementation;

        string name = $"{action.Description}";
        return name;
    }

    public List<Outcome> GetCosts()
    {
        return CurrentAction.ActionImplementation.Costs;
    }

    public List<Outcome> GetRewards()
    {
        return CurrentAction.ActionImplementation.Yields;
    }

    public bool GetRequirementsMet()
    {

        List<string> descriptions = new();
        ActionImplementation basicAction = CurrentAction.ActionImplementation;
        foreach (IRequirement req in basicAction.Requirements)
        {
            string description = req.GetDescription();
            bool isSatisfied = req.IsMet(GameState);

            if (!isSatisfied) return false;
        }

        return true;
    }

    public List<string> GetRequirements()
    {
        List<string> descriptions = new();
        ActionImplementation basicAction = CurrentAction.ActionImplementation;
        foreach (IRequirement req in basicAction.Requirements)
        {
            string description = req.GetDescription();
            bool isSatisfied = req.IsMet(GameState);

            string color = isSatisfied ? "positive" : "negative";
            descriptions.Add($"<span class='{color}'>{description}</span>");
        }
        return descriptions;
    }

    public MarkupString GetValueTypeIcon(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Momentum => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Pressure => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Health => new MarkupString("<i class='value-icon physical-icon'>⚡</i>"),
            ValueTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            _ => new MarkupString("")
        };
    }

    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        return outcome switch
        {
            // Existing outcomes
            EnergyOutcome => new MarkupString("<i class='value-icon energy-icon'>⚡</i>"),
            HealthOutcome => new MarkupString("<i class='value-icon health-icon'>❤️</i>"),
            ConcentrationOutcome => new MarkupString("<i class='value-icon focus-icon'>🌀</i>"),
            CoinOutcome => new MarkupString("<i class='value-icon coins-icon'>💰</i>"),
            FoodOutcome => new MarkupString("<i class='value-icon food-icon'>🍖</i>"),
            ActionPointOutcome => new MarkupString("<i class='value-icon ap-icon'>🔹</i>"),

            // Recovery outcomes
            EnergyRecoveryOutcome => new MarkupString("<i class='value-icon energy-recovery-icon'>🔋</i>"),

            _ => new MarkupString("")
        };
    }

    public async Task HandleConfirm()
    {
        await OnActionConfirmed.InvokeAsync(true);
    }

    public async Task HandleBack()
    {
        await OnBack.InvokeAsync();
    }
}