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

    public List<string> GetOutcomeCostsDescriptions()
    {
        List<string> descriptions = new();
        ActionImplementation basicAction = CurrentAction.ActionImplementation;
        foreach (Outcome outcome in basicAction.Costs)
        {
            string description = outcome.GetDescription();
            string preview = outcome.GetPreview(GameState.Player);

            // Special handling for DayChangeOutcome to make it stand out
            if (outcome is DayChangeOutcome)
            {
                descriptions.Add($"<strong>{description}</strong> {preview}");
            }
            else
            {
                descriptions.Add($"{description} {preview}");
            }
        }

        //if (basicAction.TimeInvestment > 0)
        //{
        //    string time =
        //        (basicAction.TimeInvestment > 1)
        //        ? $"{basicAction.TimeInvestment} hours"
        //        : $"{basicAction.TimeInvestment} hour";
        //    descriptions.Add($"{time} passes");
        //}
        return descriptions;
    }

    public List<string> GetOutcomeRewardsDescriptions()
    {
        List<string> descriptions = new();
        ActionImplementation basicAction = CurrentAction.ActionImplementation;
        foreach (Outcome outcome in basicAction.Rewards)
        {
            string description = outcome.GetDescription();
            string preview = outcome.GetPreview(GameState.Player);

            // Special handling for DayChangeOutcome to make it stand out
            if (outcome is DayChangeOutcome)
            {
                descriptions.Add($"<strong>{description}</strong> {preview}");
            }
            else
            {
                descriptions.Add($"{description} {preview}");
            }
        }
        return descriptions;
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