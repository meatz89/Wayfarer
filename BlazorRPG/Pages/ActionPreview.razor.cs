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
        return CurrentAction.ActionImplementation.Rewards;
    }

    // Now we work directly with our strongly-typed classes
    public List<string> GetRequirementDescriptions()
    {
        List<string> descriptions = new();
        ActionImplementation basicAction = CurrentAction.ActionImplementation;
        foreach (Requirement req in basicAction.Requirements)
        {
            string description = req.GetDescription();
            bool isSatisfied = req.IsSatisfied(GameState);

            if (isSatisfied) { continue; }
            string color = isSatisfied ? "positive" : "negative";
            descriptions.Add($"<span class='{color}'>{description}</span>");
        }
        return descriptions;
    }

    public MarkupString GetValueTypeIcon(ChangeTypes valueType)
    {
        return valueType switch
        {
            ChangeTypes.Momentum => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ChangeTypes.Pressure => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ChangeTypes.PhysicalEnergy => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            ChangeTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ChangeTypes.Reputation => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        if (outcome is EnergyOutcome energyOutcome)
        {
            return energyOutcome.EnergyType switch
            {
                EnergyTypes.Physical => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
                EnergyTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
                _ => new MarkupString("")
            };
        }
        return new MarkupString("");
    }

    public MarkupString GetRequirementIcon(Requirement requirement)
    {
        // Add icons for each requirement type like in EncounterChoiceTooltip
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