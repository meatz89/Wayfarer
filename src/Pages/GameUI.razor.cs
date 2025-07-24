using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

public class GameUIBase : ComponentBase, IDisposable
{
    [Inject] public ContentValidator ContentValidator { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Inject] public NavigationService NavigationService { get; set; }

    public CurrentViews CurrentScreen => NavigationService.CurrentScreen;

    public Player PlayerState => GameWorld.GetPlayer();

    [Inject] public LoadingStateService LoadingStateService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ContentValidationResult validationResult = ContentValidator.ValidateContent();
        bool missingReferences = validationResult.HasMissingReferences;

        if (missingReferences)
        {
            NavigationResult result = NavigationService.NavigateTo(CurrentViews.MissingReferences);
            if (result.Changed) StateHasChanged();
        }
        else if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
        else
        {
            NavigationResult result = NavigationService.NavigateTo(NavigationService.GetDefaultView());
            if (result.Changed) StateHasChanged();
        }

        // Events removed per architecture guidelines - handle navigation results directly
    }

    // Navigation changes now handled by checking NavigationResult directly

    public void Dispose()
    {
        // Events removed - no subscription cleanup needed
    }

    public async Task ResolvedMissingReferences()
    {
        if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
        else
        {
            NavigationResult result = NavigationService.NavigateTo(NavigationService.GetDefaultView());
            if (result.Changed) StateHasChanged();
        }
    }

    public async Task InitializeGame()
    {
        NavigationResult result = NavigationService.NavigateTo(CurrentViews.CharacterScreen);
        if (result.Changed) StateHasChanged();
    }

    public async Task HandleCharacterCreated(Player player)
    {
        await GameWorldManager.StartGame();

        // Navigate to Letter Queue Screen as the primary gameplay screen
        NavigationResult result = NavigationService.NavigateTo(CurrentViews.LetterQueueScreen);
        if (result.Changed) StateHasChanged();
    }
}