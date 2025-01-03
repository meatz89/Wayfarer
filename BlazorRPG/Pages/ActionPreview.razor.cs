using Microsoft.AspNetCore.Components;

public partial class ActionPreviewBase : ComponentBase
{
    [Parameter] public UserActionOption CurrentAction { get; set; }
    [Parameter] public PlayerState Player { get; set; }
    [Parameter] public EventCallback<bool> OnActionConfirmed { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }

    // Now we work directly with our strongly-typed classes
    public List<string> GetRequirementDescriptions()
    {
        List<string> descriptions = new();
        foreach (Requirement req in CurrentAction.BasicAction.Requirements)
        {
            string description = req.GetDescription();
            string color = req.IsSatisfied(Player) ? "positive" : "negative";
            descriptions.Add($"<span class='{color}'>{description}</span>");
        }
        return descriptions;
    }

    public List<string> GetOutcomeDescriptions()
    {
        List<string> descriptions = new();
        foreach (Outcome outcome in CurrentAction.BasicAction.Costs)
        {
            string description = outcome.GetDescription();
            string preview = outcome.GetPreview(Player);

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
        foreach (Outcome outcome in CurrentAction.BasicAction.Rewards)
        {
            string description = outcome.GetDescription();
            string preview = outcome.GetPreview(Player);

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