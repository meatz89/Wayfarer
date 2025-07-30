using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

public class GameUIBase : ComponentBase
{
    [Inject] public ContentValidator ContentValidator { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Inject] public ITimeManager TimeManager { get; set; }
    [Inject] public LoadingStateService LoadingStateService { get; set; }
    [Inject] public FlagService FlagService { get; set; }

    // Navigation state managed directly in this component
    public CurrentViews CurrentView { get; set; } = CurrentViews.LocationScreen;

    // Navigation method for child components to use
    public void NavigateTo(CurrentViews view)
    {
        Console.WriteLine($"[GameUIBase.NavigateTo] Called with view: {view}");
        Console.WriteLine($"[GameUIBase.NavigateTo] Current view before: {CurrentView}");
        
        // Simple time-based validation for Letter Board
        if (view == CurrentViews.LetterBoardScreen && 
            TimeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
        {
            Console.WriteLine($"[GameUIBase.NavigateTo] Blocking navigation to LetterBoardScreen - not Dawn");
            return; // Don't navigate
        }

        CurrentView = view;
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
                CurrentView = CurrentViews.MissingReferences;
            }
            else if (!GameWorld.GetPlayer().IsInitialized)
            {
                Console.WriteLine($"[GameUIBase.OnInitializedAsync] Player not initialized. Navigating to CharacterScreen...");
                CurrentView = CurrentViews.CharacterScreen;
            }
            else
            {
                Console.WriteLine("[GameUIBase.OnInitializedAsync] Player already initialized. Navigating to default view...");
                CurrentView = GetDefaultView();
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
        // During tutorial, always show location screen
        if (FlagService.HasFlag("tutorial_active"))
        {
            return CurrentViews.LocationScreen;
        }
        
        // Dawn = letter board available
        if (TimeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn)
        {
            return CurrentViews.LetterBoardScreen;
        }

        // Default to location screen (primary interface)
        return CurrentViews.LocationScreen;
    }


    public async Task ResolvedMissingReferences()
    {
        if (!GameWorld.GetPlayer().IsInitialized)
        {
            CurrentView = CurrentViews.CharacterScreen;
        }
        else
        {
            CurrentView = GetDefaultView();
        }
        StateHasChanged();
    }

    public async Task HandleCharacterCreated(Player player)
    {
        Console.WriteLine($"[GameUIBase.HandleCharacterCreated] Character created: {player?.Name ?? "null"}");
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Calling GameWorldManager.StartGame()...");
        await GameWorldManager.StartGame();
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] GameWorldManager.StartGame() completed.");
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Navigating to LocationScreen...");
        CurrentView = CurrentViews.LocationScreen;
        StateHasChanged();
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Navigation to LocationScreen completed.");
    }
}