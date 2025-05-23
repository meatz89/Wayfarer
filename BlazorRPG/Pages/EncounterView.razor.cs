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
        EncounterOption choice = choiceOption.Choice;
        NarrativeResult narrativeResult = Model.EncounterResult.NarrativeResult;
        Dictionary<string, ChoiceNarrative> choiceDescriptions = narrativeResult?.ChoiceDescriptions;
        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(choice.Id))
            choiceNarrative = choiceDescriptions[choice.Id];

        string name = $"{choice.Name}";
        if (choiceNarrative != null)
        {
            name = choiceNarrative.ShorthandName;
        }
        return name;
    }

    protected int GetCurrentFocusPoints()
    {
        return EncounterManager?.encounterState?.FocusPoints ?? 0;
    }

    protected int GetMaxFocusPoints()
    {
        return EncounterManager?.encounterState?.MaxFocusPoints ?? 0;
    }

    protected Dictionary<AspectTokenTypes, int> GetAspectTokenCounts()
    {
        return EncounterManager?.encounterState?.AspectTokens?.GetAllTokenCounts() ??
               new Dictionary<AspectTokenTypes, int>();
    }

    protected int GetCurrentProgress()
    {
        return EncounterManager?.encounterState?.CurrentProgress ?? 0;
    }

    protected int GetProgressThreshold()
    {
        return EncounterManager?.encounterState?.EncounterInfo?.TotalProgress ?? 0;
    }

    protected string GetStageTitle()
    {
        int currentStage = (EncounterManager?.encounterState?.CurrentStageIndex ?? 0) + 1;
        int totalStages = EncounterManager?.encounterState?.EncounterInfo?.Stages?.Count ?? 0;
        return $"Stage {currentStage} of {totalStages}";
    }

    protected bool CanAffordChoice(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            return EncounterManager?.encounterState?.CanAffordFocusCost(option.FocusCost) ?? false;
        }
        return true;
    }

    protected bool HasRequiredTokens(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            foreach (KeyValuePair<AspectTokenTypes, int> requirement in option.TokenCosts)
            {
                if (!EncounterManager.encounterState.HasAspectTokens(requirement.Key, requirement.Value))
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
                UniversalActionTypes.Recovery => "tier-1",
                UniversalActionTypes.GenerationA => "tier-2",
                UniversalActionTypes.GenerationB => "tier-2",
                UniversalActionTypes.ConversionA => "tier-3",
                UniversalActionTypes.ConversionB => "tier-3",
                UniversalActionTypes.Hybrid => "tier-4",
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