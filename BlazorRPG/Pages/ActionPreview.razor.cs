using Microsoft.AspNetCore.Components;

public partial class ActionPreviewBase : ComponentBase
{
    [Parameter] public UserActionOption CurrentAction { get; set; }
    [Parameter] public ApproachDefinition CurrentApproach { get; set; }
    [Parameter] public GameState GameState { get; set; }
    [Parameter] public EventCallback<bool> OnActionConfirmed { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }

    protected string GetCardCostClass(SkillCategories costType)
    {
        return costType switch
        {
            SkillCategories.Physical => "physical",
            SkillCategories.Intellectual => "intellectual",
            SkillCategories.Social => "social",
            _ => ""
        };
    }
    public string GetActionName()
    {
        ActionImplementation action = CurrentAction?.ActionImplementation;

        string name = $"{action?.ActionType} - {action?.Name}";
        return string.IsNullOrWhiteSpace(name) ? "No Action" : name;
    }

    public string GetActionDescription()
    {
        ActionImplementation action = CurrentAction?.ActionImplementation;

        string name = $"{action?.Description}";
        return string.IsNullOrWhiteSpace(name) ? "No Action" : name;
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

    public async Task HandleConfirm()
    {
        await OnActionConfirmed.InvokeAsync(true);
    }

    public async Task HandleBack()
    {
        await OnBack.InvokeAsync();
    }
}