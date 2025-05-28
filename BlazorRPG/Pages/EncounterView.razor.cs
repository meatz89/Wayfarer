using BlazorRPG.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }

    [Inject] public GameWorld GameState { get; set; }
    [Inject] public GameWorldManager GameManager { get; set; }
    [Parameter] public EncounterManager EncounterManager { get; set; }
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }
    private IJSObjectReference _tooltipModule;
    public Player PlayerState
    {
        get
        {
            return GameState.Player;
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

    protected int GetCurrentFocusPoints()
    {
        return EncounterManager?.state?.FocusPoints ?? 0;
    }

    protected int GetMaxFocusPoints()
    {
        return EncounterManager?.state?.MaxFocusPoints ?? 0;
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