using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

public partial class EncounterViewBase : ComponentBase
{
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Parameter] public EventCallback<BeatOutcome> OnEncounterCompleted { get; set; }
    [Parameter] public EncounterManager EncounterManager { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }
    private IJSObjectReference _tooltipModule;

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double tooltipX;
    public double tooltipY;

    public bool IsLoading = true;

    public BeatOutcome EncounterResult { get; private set; }
    public List<UserEncounterChoiceOption> CurrentChoices { get; set; } = new();

    private Timer pollingTimer;
    public GameWorldSnapshot currentSnapshot;

    public Player PlayerState
    {
        get
        {
            return GameWorld.Player;
        }
    }

    public bool IsChoiceDisabled(UserEncounterChoiceOption userEncounterChoiceOption)
    {
        return userEncounterChoiceOption.Choice.IsDisabled;
    }

    public EncounterViewModel Model;
    private string tooltipContent;

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

    protected override void OnInitialized()
    {
        NextEncounterBeat();

        // Set up polling timer - no events, just regular polling
        pollingTimer = new Timer(_ =>
        {
            InvokeAsync(() =>
            {
                PollGameWorld();
                StateHasChanged();
            });
        }, null, 0, 100); // Poll every 100ms
    }

    private async Task NextEncounterBeat()
    {
        await GameWorldManager.NextEncounterBeat();
    }

    private void PollGameWorld()
    {
        // Poll for current game state
        currentSnapshot = GameWorldManager.GetGameSnapshot();
    }

    public async Task ProcessPlayerChoice(EncounterChoice choice)
    {
        hoveredChoice = null;
        showTooltip = false;
        IsLoading = true;

        PlayerChoiceSelection playerChoice = new PlayerChoiceSelection()
        {
            Choice = choice,
            SelectedOption = choice.SkillOption
        };

        BeatOutcome result = await GameWorldManager.ProcessPlayerChoice(playerChoice);
        await CheckEncounterCompleted(result);

        Model = GetModel();
        GetChoices();

        HideTooltip();
        IsLoading = false;

        StateHasChanged();
    }

    public void Dispose()
    {
        pollingTimer?.Dispose();
    }

    public async Task ShowTooltip(UserEncounterChoiceOption choice, string elementId)
    {
        hoveredChoice = choice;
        showTooltip = true;

        if (choice.Choice is EncounterChoice encounterChoice)
        {
            // Include template information in tooltip
            string templateName = encounterChoice.TemplateUsed;
            string templatePurpose = encounterChoice.TemplatePurpose;

            // Set tooltip content
            tooltipContent = $"{templatePurpose}";
        }

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
        EncounterViewModel? encounterViewModel = GameWorldManager.GetEncounterViewModel();

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
            CurrentEncounterContext = null,
            State = null,
            EncounterResult = new EncounterResult()
            {
                locationAction = null,
                ActionResult = ActionResults.GameOver,
                EncounterEndMessage = "Game Over",
                EncounterContext = null,
                PostEncounterEvolution = null,
                AIResponse = null
            }
        };

        return encounterViewModel;
    }


    private async Task CheckEncounterCompleted(BeatOutcome result)
    {
        if (result.Outcome != BeatOutcomes.None)
        {
            await OnEncounterCompleted.InvokeAsync(result);
        }
        else
        {
            EncounterResult = result;
        }
    }

    public void GetChoices()
    {
        CurrentChoices = GameWorld.ActionStateTracker.UserEncounterChoiceOptions;
        StateHasChanged();
    }

    protected int GetCurrentFocusPoints()
    {
        return EncounterManager?.GetEncounterState()?.FocusPoints ?? 0;
    }

    protected int GetMaxFocusPoints()
    {
        return EncounterManager?.GetEncounterState()?.MaxFocusPoints ?? 0;
    }


    protected int GetFocusCost(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterChoice option)
        {
            return option.FocusCost;
        }
        return 0;
    }
}

public class TooltipPosition
{
    public double TooltipX { get; set; }
    public double TooltipY { get; set; }
}