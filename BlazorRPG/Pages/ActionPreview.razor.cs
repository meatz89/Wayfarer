using Microsoft.AspNetCore.Components;

public partial class ActionPreviewBase : ComponentBase
{
    [Parameter] public UserActionOption CurrentAction { get; set; }
    [Parameter] public ApproachDefinition CurrentApproach { get; set; }
    [Parameter] public GameWorld GameState { get; set; }
    [Parameter] public EventCallback<bool> OnActionConfirmed { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }

    protected string GetCardCostClass(ActionTypes costType)
    {
        return costType switch
        {
            ActionTypes.Physical => "physical",
            ActionTypes.Intellectual => "intellectual",
            ActionTypes.Social => "social",
            _ => ""
        };
    }
    public string GetActionName()
    {
        LocationAction action = CurrentAction?.locationAction;

        string name = $"{action?.RequiredCardType} - {action?.Name}";
        return string.IsNullOrWhiteSpace(name) ? "No Action" : name;
    }

    public string GetActionDescription()
    {
        LocationAction action = CurrentAction?.locationAction;

        string name = $"{action?.ObjectiveDescription}";
        return string.IsNullOrWhiteSpace(name) ? "No Action" : name;
    }

    public bool GetRequirementsMet()
    {
        List<string> descriptions = new();
        LocationAction basicAction = CurrentAction.locationAction;
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
        LocationAction basicAction = CurrentAction.locationAction;
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

    public async Task HandleConfirm()
    {
        await OnActionConfirmed.InvokeAsync(true);
    }

    public async Task HandleBack()
    {
        await OnBack.InvokeAsync();
    }
}