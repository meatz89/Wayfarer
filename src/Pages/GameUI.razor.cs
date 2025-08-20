using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

public class GameUIBase : ComponentBase
{
    [Inject] public ContentValidator ContentValidator { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameFacade GameFacade { get; set; }
    [Inject] public ITimeManager TimeManager { get; set; }
    [Inject] public LoadingStateService LoadingStateService { get; set; }
    [Inject] public FlagService FlagService { get; set; }
    [Inject] public Wayfarer.Services.NavigationCoordinator NavigationCoordinator { get; set; }

    // Navigation state now managed by NavigationCoordinator
    public CurrentViews CurrentView => NavigationCoordinator?.CurrentView ?? CurrentViews.LocationScreen;

    // Navigation method for child components to use
    public async void NavigateTo(CurrentViews view)
    {
        Console.WriteLine($"[GameUIBase.NavigateTo] Called with view: {view}");
        Console.WriteLine($"[GameUIBase.NavigateTo] Current view before: {CurrentView}");

        if (NavigationCoordinator == null)
        {
            Console.WriteLine($"[GameUIBase.NavigateTo] NavigationCoordinator is null, cannot navigate");
            return;
        }

        // Use NavigationCoordinator for validation and transition
        bool success = await NavigationCoordinator.NavigateToAsync(view);

        if (!success)
        {
            string reason = NavigationCoordinator.GetBlockedReason(view);
            Console.WriteLine($"[GameUIBase.NavigateTo] Navigation blocked: {reason}");
            // Could show this to user via MessageSystem
            return;
        }

        Console.WriteLine($"[GameUIBase.NavigateTo] Current view after: {CurrentView}");
        Console.WriteLine($"[GameUIBase.NavigateTo] Calling StateHasChanged...");
        StateHasChanged();
        Console.WriteLine($"[GameUIBase.NavigateTo] StateHasChanged completed");
    }

    protected override async Task OnInitializedAsync()
    {
        // IMPORTANT: For testing purposes, this ALWAYS starts a fresh game
        // No save/load functionality is implemented or desired
        // The game state is never persisted between sessions

        try
        {
            Console.WriteLine("[GameUIBase.OnInitializedAsync] Starting initialization...");
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] ContentValidator null? {ContentValidator == null}");
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] GameWorld null? {GameWorld == null}");
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] TimeManager null? {TimeManager == null}");
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] LoadingStateService null? {LoadingStateService == null}");

            Console.WriteLine("[GameUIBase.OnInitializedAsync] Calling ContentValidator.ValidateContent()...");
            ContentValidationResult validationResult = ContentValidator.ValidateContent();
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] ContentValidator.ValidateContent() completed. HasMissingReferences: {validationResult.HasMissingReferences}");

            bool missingReferences = validationResult.HasMissingReferences;

            if (missingReferences)
            {
                Console.WriteLine("[GameUIBase.OnInitializedAsync] Missing references detected. Navigating to MissingReferences view...");
                await NavigationCoordinator.NavigateToAsync(CurrentViews.MissingReferences);
            }
            else if (!GameWorld.GetPlayer().IsInitialized)
            {
                Console.WriteLine($"[GameUIBase.OnInitializedAsync] Player not initialized. Creating default player...");
                // Create a default player for the mockup
                Player player = GameWorld.GetPlayer();
                player.Name = "Wayfarer";
                player.IsInitialized = true;
                Console.WriteLine("[GameUIBase.OnInitializedAsync] Default player created. Showing Location...");
                await NavigationCoordinator.NavigateToAsync(CurrentViews.LocationScreen);
            }
            else
            {
                Console.WriteLine("[GameUIBase.OnInitializedAsync] Player already initialized. Showing Location...");
                await NavigationCoordinator.NavigateToAsync(CurrentViews.LocationScreen);
            }

            Console.WriteLine($"[GameUIBase.OnInitializedAsync] Initialization completed. CurrentView: {CurrentView}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] ERROR: {ex.Message}");
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] STACK: {ex.StackTrace}");
            throw;
        }
    }

    private CurrentViews GetDefaultView()
    {
        // Show location screen as the main gameplay view
        return CurrentViews.LocationScreen;
    }


    public async Task ResolvedMissingReferences()
    {
        if (!GameWorld.GetPlayer().IsInitialized)
        {
            await NavigationCoordinator.NavigateToAsync(CurrentViews.CharacterScreen);
        }
        else
        {
            await NavigationCoordinator.NavigateToAsync(GetDefaultView());
        }
        StateHasChanged();
    }

    public async Task HandleCharacterCreated(Player player)
    {
        Console.WriteLine($"[GameUIBase.HandleCharacterCreated] Character created: {player?.Name ?? "null"}");
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Calling GameFacade.StartGame()...");
        await GameFacade.StartGameAsync();
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] GameFacade.StartGame() completed.");
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Navigating to LocationScreen...");
        await NavigationCoordinator.NavigateToAsync(CurrentViews.LocationScreen);
        StateHasChanged();
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Navigation to LocationScreen completed.");
    }
}