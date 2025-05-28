using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private ContentValidator ContentValidator { get; set; }
    [Inject] private GameWorld GameState { get; set; }
    [Inject] private GameWorldManager GameManager { get; set; }

    private Timer _pollingTimer;
    private bool _previousLoadingState;
    private string _previousMessage = string.Empty;
    private int _previousProgress;

    public CurrentViews CurrentScreen { get; private set; } = CurrentViews.CharacterScreen;

    public Player PlayerState
    {
        get
        {
            return GameState.Player;
        }
    }

    [Inject] private LoadingStateService LoadingStateService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ContentValidationResult validationResult = ContentValidator.ValidateContent();
        bool missingReferences = validationResult.HasMissingReferences;

        if (missingReferences)
        {
            CurrentScreen = CurrentViews.MissingReferences;
        }
        else if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
        else
        {
            CurrentScreen = CurrentViews.LocationScreen;
        }

        _pollingTimer = new Timer(CheckLoadingState, null, 0, 100);
    }

    private void CheckLoadingState(object state)
    {
        // Check if loading state has changed
        bool hasChanged = _previousLoadingState != LoadingStateService.IsLoading ||
            _previousMessage != LoadingStateService.Message ||
            _previousProgress != LoadingStateService.Progress;

        if (hasChanged)
        {
            _previousLoadingState = LoadingStateService.IsLoading;
            _previousMessage = LoadingStateService.Message;
            _previousProgress = LoadingStateService.Progress;

            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        _pollingTimer?.Dispose();
    }

    private async Task ResolvedMissingReferences()
    {
        if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
        else
        {
            CurrentScreen = CurrentViews.LocationScreen;
            StateHasChanged();
        }
    }

    private async Task InitializeGame()
    {
        CurrentScreen = CurrentViews.CharacterScreen;
        StateHasChanged();
    }

    private async Task HandleCharacterCreated(Player playerState)
    {
        CurrentScreen = CurrentViews.LocationScreen;
        await GameManager.StartGame();
        StateHasChanged();
    }
}