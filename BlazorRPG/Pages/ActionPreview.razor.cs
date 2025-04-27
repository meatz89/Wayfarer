using Microsoft.AspNetCore.Components;

public partial class ActionPreviewBase : ComponentBase
{
    [Parameter] public UserActionOption CurrentAction { get; set; }
    [Parameter] public GameState GameState { get; set; }
    [Parameter] public EventCallback<bool> OnActionConfirmed { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }

    public string GetActionName()
    {
        ActionImplementation action = CurrentAction.ActionImplementation;

        string name = $"{action.ActionType} - {action.Id}";
        return name;
    }

    public string GetActionDescription()
    {
        ActionImplementation action = CurrentAction.ActionImplementation;

        string name = $"{action.Goal}";
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

    // Now we work directly with our strongly-typed classes
    public List<string> GetRequirementDescriptions()
    {
        List<string> descriptions = new();
        ActionImplementation basicAction = CurrentAction.ActionImplementation;
        foreach (IRequirement req in basicAction.Requirements)
        {
            string description = req.GetDescription();
            bool isSatisfied = req.IsMet(GameState);

            if (isSatisfied) { continue; }
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
            ValueTypes.Confidence => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        if (outcome is EnergyOutcome energyOutcome)
        {
            return new MarkupString("<i class='value-icon physical-icon'>⚡</i>");
        }
        return new MarkupString("");
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