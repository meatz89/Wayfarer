using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

public class GameUIBase : ComponentBase, IDisposable
{
    [Inject] public ContentValidator ContentValidator { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Inject] public NavigationService NavigationService { get; set; }

    public CurrentViews CurrentScreen => NavigationService.CurrentScreen;

    [Inject] public LoadingStateService LoadingStateService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("[GameUIBase.OnInitializedAsync] Starting initialization...");

        Console.WriteLine("[GameUIBase.OnInitializedAsync] Calling ContentValidator.ValidateContent()...");
        ContentValidationResult validationResult = ContentValidator.ValidateContent();
        Console.WriteLine($"[GameUIBase.OnInitializedAsync] ContentValidator.ValidateContent() completed. HasMissingReferences: {validationResult.HasMissingReferences}");

        bool missingReferences = validationResult.HasMissingReferences;

        if (missingReferences)
        {
            Console.WriteLine("[GameUIBase.OnInitializedAsync] Missing references detected. Navigating to MissingReferences view...");
            NavigationService.NavigateTo(CurrentViews.MissingReferences);
        }
        else if (!GameWorld.GetPlayer().IsInitialized)
        {
            Console.WriteLine($"[GameUIBase.OnInitializedAsync] Player not initialized. Calling InitializeGame()...");
            await InitializeGame();
            Console.WriteLine("[GameUIBase.OnInitializedAsync] InitializeGame() completed.");
        }
        else
        {
            Console.WriteLine("[GameUIBase.OnInitializedAsync] Player already initialized. Navigating to default view...");
            NavigationService.NavigateTo(NavigationService.GetDefaultView());
        }

        // Subscribe to navigation changes
        Console.WriteLine("[GameUIBase.OnInitializedAsync] Subscribing to navigation changes...");
        NavigationService.OnNavigationChanged += OnNavigationChanged;

        Console.WriteLine("[GameUIBase.OnInitializedAsync] Initialization completed.");
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
        if (!GameWorld.GetPlayer().IsInitialized)
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
        Console.WriteLine("[GameUIBase.InitializeGame] Navigating to CharacterScreen...");
        NavigationService.NavigateTo(CurrentViews.CharacterScreen);
        Console.WriteLine("[GameUIBase.InitializeGame] Navigation to CharacterScreen completed.");
    }

    public async Task HandleCharacterCreated(Player player)
    {
        Console.WriteLine($"[GameUIBase.HandleCharacterCreated] Character created: {player?.Name ?? "null"}");
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Calling GameWorldManager.StartGame()...");
        await GameWorldManager.StartGame();
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] GameWorldManager.StartGame() completed.");
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Navigating to LocationScreen...");
        NavigationService.NavigateTo(CurrentViews.LocationScreen);
        Console.WriteLine("[GameUIBase.HandleCharacterCreated] Navigation to LocationScreen completed.");
    }
}