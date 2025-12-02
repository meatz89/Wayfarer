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
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameOrchestrator GameOrchestrator { get; set; }
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

        Console.WriteLine("[GameUI.OnInitializedAsync] Starting initialization");

        // Start the game (idempotent - checks IsGameStarted internally)
        await GameOrchestrator.StartGameAsync();
        CurrentView = CurrentViews.LocationScreen;
        StateHasChanged();
    }

    private CurrentViews GetDefaultView()
    {
        // Show Venue screen as the main gameplay view
        return CurrentViews.LocationScreen;
    }

    public async Task ResolvedMissingReferences()
    {
        // After resolving references, return to default view
        CurrentView = GetDefaultView();
        StateHasChanged();
    }

    public async Task HandleCharacterCreated(Player player)
    {
        await GameOrchestrator.StartGameAsync(); CurrentView = CurrentViews.LocationScreen;
        StateHasChanged();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}