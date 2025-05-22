using BlazorRPG.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }

    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public EncounterManager EncounterManager { get; set; }
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }
    private IJSObjectReference _tooltipModule;
    public PlayerState PlayerState
    {
        get
        {
            return GameState.PlayerState;
        }
    }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double tooltipX;
    public double tooltipY;

    public bool IsLoading = true;

    public EncounterResult EncounterResult { get; private set; }
    public List<UserEncounterChoiceOption> CurrentChoices { get; set; } = new();

    public bool IsChoiceDisabled(UserEncounterChoiceOption userEncounterChoiceOption)
    {
        return userEncounterChoiceOption.Choice.IsDisabled;
    }

    public EncounterViewModel Model;

    protected override async Task OnInitializedAsync()
    {
        Model = GetModel();
        GetChoices();

        if (EncounterManager != null && Model != null)
        {
            IsLoading = false;
        }
        else
        {
            IsLoading = true;
        }

        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _tooltipModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/tooltipInterop.js");
        }
    }

    public async Task ShowTooltip(UserEncounterChoiceOption choice, string elementId)
    {
        hoveredChoice = choice;
        showTooltip = true;

        if (_tooltipModule != null)
        {
            TooltipPosition position = await _tooltipModule.InvokeAsync<TooltipPosition>(
                "getTooltipPositionRelativeToElement", $"{elementId}");

            tooltipX = position.TooltipX;
            tooltipY = position.TooltipY;
        }

        StateHasChanged();
    }

    public void HideTooltip()
    {
        hoveredChoice = null;
        showTooltip = false;
    }

    private EncounterViewModel GetModel()
    {
        EncounterViewModel? encounterViewModel = GameManager.GetEncounterViewModel();

        if (encounterViewModel == null)
        {
            encounterViewModel = CreateGameOverModel();
        }

        return encounterViewModel;
    }

    private EncounterViewModel CreateGameOverModel()
    {
        EncounterViewModel encounterViewModel = new EncounterViewModel()
        {
            ChoiceSetName = "None",
            CurrentChoices = new List<UserEncounterChoiceOption>(),
            CurrentEncounter = null,
            State = EncounterState.PreviousEncounterState,
            EncounterResult = new EncounterResult()
            {
                ActionImplementation = null,
                ActionResult = ActionResults.GameOver,
                EncounterEndMessage = "Game Over",
                NarrativeContext = null,
                PostEncounterEvolution = null,
                NarrativeResult = null
            }
        };

        return encounterViewModel;
    }

    public async Task HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        hoveredChoice = null;
        showTooltip = false;
        IsLoading = true;

        EncounterResult result = await GameManager.ExecuteEncounterChoice(choice);
        await CheckEncounterCompleted(result);

        Model = GetModel();
        GetChoices();

        HideTooltip();
        IsLoading = false;

        StateHasChanged();
    }

    private async Task CheckEncounterCompleted(EncounterResult result)
    {
        if (result.ActionResult == ActionResults.Ongoing)
        {
            EncounterResult = result;
        }
        else
        {
            await OnEncounterCompleted.InvokeAsync(result);
        }
    }

    public void GetChoices()
    {
        CurrentChoices = GameState.ActionStateTracker.UserEncounterChoiceOptions;
        StateHasChanged();
    }

    public string GetChoiceName(UserEncounterChoiceOption choiceOption)
    {
        EncounterOption card = choiceOption.Choice;
        NarrativeResult narrativeResult = Model.EncounterResult.NarrativeResult;
        Dictionary<EncounterOption, ChoiceNarrative> choiceDescriptions = narrativeResult?.ChoiceDescriptions;
        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(card))
            choiceNarrative = choiceDescriptions[card];

        string name = $"{card.Name}";
        if (choiceNarrative != null)
        {
            name = choiceNarrative.ShorthandName;
        }
        return name;
    }

    public List<PropertyDisplay> GetAvailableTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        return properties;
    }

    private string GetTagIcon(IEncounterTag tag)
    {
        // Determine icon based on tag type
        if (tag is NarrativeTag)
            return "📜"; // Narrative tag icon
        else if (tag is StrategicTag)
            return "⚙️"; // Strategic tag icon

        return "🏷️"; // Default tag icon
    }

    private string GetTagCssClass(IEncounterTag tag)
    {
        // Determine CSS class based on tag type
        if (tag is NarrativeTag)
            return "narrative-tag";
        else if (tag is StrategicTag)
            return "strategic-tag";

        return "";
    }

    private string GetTagTooltipText(IEncounterTag tag)
    {
        StringBuilder tooltip = new StringBuilder();
        tooltip.AppendLine(tag.NarrativeName);

        if (tag is NarrativeTag narrativeTag)
        {
            tooltip.AppendLine($"{narrativeTag.GetEffectDescription()}");
        }
        else if (tag is StrategicTag strategicTag)
        {
            tooltip.AppendLine(strategicTag.GetEffectDescription());
        }

        return tooltip.ToString();
    }

    public string GetTagEffectDescription(string tagName)
    {
        return "Affects encounter mechanics";
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

    public List<PropertyDisplay> GetActiveTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        if (EncounterManager == null)
            return properties;

        return properties;
    }

    protected int GetCurrentFocusPoints()
    {
        return EncounterManager?.EncounterState?.FocusPoints ?? 0;
    }

    protected int GetMaxFocusPoints()
    {
        return EncounterManager?.EncounterState?.MaxFocusPoints ?? 0;
    }

    protected Dictionary<AspectTokenTypes, int> GetAspectTokenCounts()
    {
        return EncounterManager?.EncounterState?.AspectTokens?.GetAllTokenCounts() ??
               new Dictionary<AspectTokenTypes, int>();
    }

    protected int GetCurrentProgress()
    {
        return EncounterManager?.EncounterState?.CurrentProgress ?? 0;
    }

    protected int GetProgressThreshold()
    {
        return EncounterManager?.EncounterState?.EncounterInfo?.TotalProgress ?? 0;
    }

    protected string GetStageTitle()
    {
        int currentStage = (EncounterManager?.EncounterState?.CurrentStageIndex ?? 0) + 1;
        int totalStages = EncounterManager?.EncounterState?.EncounterInfo?.Stages?.Count ?? 0;
        return $"Stage {currentStage} of {totalStages}";
    }

    protected bool CanAffordChoice(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            return EncounterManager?.EncounterState?.CanAffordFocusCost(option.FocusCost) ?? false;
        }
        return true;
    }

    protected bool HasRequiredTokens(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            foreach (KeyValuePair<AspectTokenTypes, int> requirement in option.TokenCosts)
            {
                if (!EncounterManager.EncounterState.HasAspectTokens(requirement.Key, requirement.Value))
                {
                    return false;
                }
            }
        }
        return true;
    }

    protected int GetFocusCost(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            return option.FocusCost;
        }
        return 0;
    }

    protected List<string> GetTokenRequirements(UserEncounterChoiceOption choice)
    {
        List<string> requirements = new List<string>();

        if (choice.Choice is EncounterOption option)
        {
            foreach (KeyValuePair<AspectTokenTypes, int> requirement in option.TokenCosts)
            {
                if (requirement.Value > 0)
                {
                    requirements.Add($"{requirement.Value} {GetTokenDisplayName(requirement.Key)}");
                }
            }
        }

        return requirements;
    }

    protected string GetChoiceCssClass(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            return option.ActionType switch
            {
                UniversalActionType.SafetyOption => "tier-1",
                UniversalActionType.GenerateForce => "tier-2",
                UniversalActionType.GenerateFlow => "tier-2",
                UniversalActionType.GenerateFocus => "tier-2",
                UniversalActionType.GenerateFortitude => "tier-3",
                UniversalActionType.BasicConversion => "tier-3",
                UniversalActionType.SpecializedConversion => "tier-4",
                UniversalActionType.PremiumConversion => "tier-5",
                _ => "tier-1"
            };
        }
        return "tier-1";
    }

    protected string GetTokenIcon(AspectTokenTypes tokenType)
    {
        return tokenType switch
        {
            AspectTokenTypes.Force => "🔴",
            AspectTokenTypes.Flow => "🔵",
            AspectTokenTypes.Focus => "🟡",
            AspectTokenTypes.Fortitude => "🟢",
            _ => "⚪"
        };
    }

    private string GetTokenDisplayName(AspectTokenTypes tokenType)
    {
        return tokenType switch
        {
            AspectTokenTypes.Force => "Force",
            AspectTokenTypes.Flow => "Flow",
            AspectTokenTypes.Focus => "Focus",
            AspectTokenTypes.Fortitude => "Fortitude",
            _ => tokenType.ToString()
        };
    }
}


public class TooltipPosition
{
    public double TooltipX { get; set; }
    public double TooltipY { get; set; }
}