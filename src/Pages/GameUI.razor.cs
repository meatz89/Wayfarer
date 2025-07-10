using Microsoft.AspNetCore.Components;
using Wayfarer.UIHelpers;

namespace Wayfarer.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private ContentValidator ContentValidator { get; set; }
    [Inject] private GameWorld GameWorld { get; set; }
    [Inject] private GameWorldManager GameWorldManager { get; set; }

    public CurrentViews CurrentScreen { get; private set; } = CurrentViews.CharacterScreen;

    public Player PlayerState
    {
        get
        {
            return GameWorld.GetPlayer();
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

    private async Task HandleCharacterCreated(Player player)
    {
        CurrentScreen = CurrentViews.LocationScreen;
        await GameWorldManager.StartGame();
        StateHasChanged();
    }
}