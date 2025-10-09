using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

/// <summary>
/// Root game UI component that manages initial game setup and view routing.
/// 
/// CRITICAL: BLAZOR SERVERPRERENDERED CONSEQUENCES
/// ================================================
/// This component renders TWICE due to ServerPrerendered mode:
/// 1. During server-side prerendering (static HTML generation)
/// 2. After establishing interactive SignalR connection
/// 
/// ARCHITECTURAL PRINCIPLES:
/// - OnInitializedAsync() runs TWICE - all initialization MUST be idempotent
/// - Use flags (like GameWorld.IsGameStarted) to prevent duplicate initialization
/// - NEVER add duplicate messages or perform duplicate side effects
/// - Services are Singletons and persist state across both renders
/// - Read-only operations are safe to run multiple times
/// - User actions only occur after interactive connection established
/// 
/// IMPLEMENTATION REQUIREMENTS:
/// - Check state flags before mutating game state
/// - Guard against duplicate system messages
/// - Protect resource modifications with idempotence checks
/// - Ensure event subscriptions don't duplicate
/// </summary>
public class GameUIBase : ComponentBase, IDisposable
{
    [Inject] public ContentValidator ContentValidator { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameFacade GameFacade { get; set; }
    [Inject] public TimeManager TimeManager { get; set; }
    [Inject] public LoadingStateService LoadingStateService { get; set; }
    public CurrentViews CurrentView { get; set; } = CurrentViews.LocationScreen;

    protected override async Task OnInitializedAsync()
    {
        // IMPORTANT: For testing purposes, this ALWAYS starts a fresh game
        // No save/load functionality is implemented or desired
        // The game state is never persisted between sessions

        // CRITICAL: This method runs TWICE due to ServerPrerendered mode:
        // 1. During prerendering (server-side HTML generation)
        // 2. After interactive SignalR connection established
        // All initialization MUST be idempotent to avoid duplicate side effects

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
                StateHasChanged();
            }
            else if (!GameWorld.GetPlayer().IsInitialized)
            {
                Console.WriteLine($"[GameUIBase.OnInitializedAsync] Player not initialized. Creating default player...");
                // Create a default player for the mockup
                Player player = GameWorld.GetPlayer();
                player.Name = "Wayfarer";
                player.IsInitialized = true;

                // Start the game to initialize location
                Console.WriteLine("[GameUIBase.OnInitializedAsync] Starting game...");
                await GameFacade.StartGameAsync();

                Console.WriteLine("[GameUIBase.OnInitializedAsync] Default player created. Showing Location...");
                CurrentView = CurrentViews.LocationScreen;
                StateHasChanged();
            }
            else
            {
                Console.WriteLine("[GameUIBase.OnInitializedAsync] Player already initialized. Starting game...");
                await GameFacade.StartGameAsync();

                Console.WriteLine("[GameUIBase.OnInitializedAsync] Showing Location...");
                CurrentView = CurrentViews.LocationScreen;
                StateHasChanged();
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
        // Show Venue screen as the main gameplay view
        return CurrentViews.LocationScreen;
    }


    public async Task ResolvedMissingReferences()
    {
        if (!GameWorld.GetPlayer().IsInitialized)
        {
            CurrentView = CurrentViews.CharacterScreen;
            StateHasChanged();
        }
        else
        {
            CurrentView = GetDefaultView();
            StateHasChanged();
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
        CurrentView = CurrentViews.LocationScreen;
        StateHasChanged();
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Navigation to LocationScreen completed.");
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}