using Microsoft.AspNetCore.Components;
using Wayfarer.UIHelpers;
using Wayfarer.Services;

namespace Wayfarer.Pages;

public class GameUIBase : ComponentBase, IDisposable
{
    [Inject] public ContentValidator ContentValidator { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Inject] public NavigationService NavigationService { get; set; }

    public CurrentViews CurrentScreen => NavigationService.CurrentScreen;

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
            NavigationService.NavigateTo(CurrentViews.MissingReferences);
        }
        else if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
        else
        {
            NavigationService.NavigateTo(NavigationService.GetDefaultView());
        }
        
        // Subscribe to navigation changes
        NavigationService.OnNavigationChanged += OnNavigationChanged;
    }
    
    private void OnNavigationChanged(CurrentViews newView)
    {
        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        NavigationService.OnNavigationChanged -= OnNavigationChanged;
    }

    public async Task ResolvedMissingReferences()
    {
        if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
        else
        {
            NavigationService.NavigateTo(NavigationService.GetDefaultView());
        }
    }

    public async Task InitializeGame()
    {
        NavigationService.NavigateTo(CurrentViews.CharacterScreen);
    }

    public async Task HandleCharacterCreated(Player player)
    {
        await GameWorldManager.StartGame();
        // Navigate to Letter Queue Screen as the primary gameplay screen
        NavigationService.NavigateTo(CurrentViews.LetterQueueScreen);
    }
}