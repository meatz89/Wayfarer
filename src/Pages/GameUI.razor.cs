using Microsoft.AspNetCore.Components;
using Wayfarer.UIHelpers;

namespace Wayfarer.Pages;

public class GameUIBase : ComponentBase
{
    [Inject] public ContentValidator ContentValidator { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }

    public CurrentViews CurrentScreen { get; set; } = CurrentViews.CharacterScreen;

    public Player PlayerState
    {
        get
        {
            return GameWorld.GetPlayer();
        }
    }

    [Inject] public LoadingStateService LoadingStateService { get; set; }

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

    public async Task ResolvedMissingReferences()
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

    public async Task InitializeGame()
    {
        CurrentScreen = CurrentViews.CharacterScreen;
        StateHasChanged();
    }

    public async Task HandleCharacterCreated(Player player)
    {
        CurrentScreen = CurrentViews.LocationScreen;
        await GameWorldManager.StartGame();
        StateHasChanged();
    }
}