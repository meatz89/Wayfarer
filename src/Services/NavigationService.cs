/// <summary>
/// Centralized navigation management service for the letter queue game.
/// Manages screen transitions, navigation history, and context awareness.
/// </summary>
public class NavigationService
{
    private readonly GameWorld _gameWorld;
    private readonly Stack<CurrentViews> _navigationHistory = new();
    private CurrentViews _currentScreen = CurrentViews.LetterQueueScreen;

    public NavigationService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Current active screen
    /// </summary>
    public CurrentViews CurrentScreen => _currentScreen;

    /// <summary>
    /// Navigation history for back button support
    /// </summary>
    public IReadOnlyCollection<CurrentViews> NavigationHistory => _navigationHistory;

    // Events removed per architecture guidelines - use return values instead

    /// <summary>
    /// Navigate to a specific screen
    /// Returns the navigation result including the new screen if successful
    /// </summary>
    public NavigationResult NavigateTo(CurrentViews screen)
    {
        // Don't navigate to the same screen
        if (_currentScreen == screen)
            return NavigationResult.NoChange(_currentScreen);

        // Validate navigation is allowed
        if (!CanNavigateTo(screen))
            return NavigationResult.Blocked(_currentScreen, "Navigation not allowed to " + screen);

        CurrentViews previousScreen = _currentScreen;

        // Push current screen to history
        _navigationHistory.Push(_currentScreen);

        // Update current screen
        _currentScreen = screen;

        return NavigationResult.Success(previousScreen, _currentScreen);
    }

    /// <summary>
    /// Navigate back to previous screen
    /// Returns the navigation result including the new screen if successful
    /// </summary>
    public NavigationResult NavigateBack()
    {
        if (!CanNavigateBack())
            return NavigationResult.Blocked(_currentScreen, "No navigation history available");

        CurrentViews fromScreen = _currentScreen;
        CurrentViews previousScreen = _navigationHistory.Pop();
        _currentScreen = previousScreen;

        return NavigationResult.Success(fromScreen, _currentScreen);
    }

    /// <summary>
    /// Check if back navigation is available
    /// </summary>
    public bool CanNavigateBack()
    {
        return _navigationHistory.Count > 0;
    }

    /// <summary>
    /// Check if navigation to a specific screen is allowed
    /// </summary>
    public bool CanNavigateTo(CurrentViews screen)
    {
        // Always allow system screens
        if (screen == CurrentViews.MissingReferences) return true;

        // Check if player exists for game screens
        Player player = _gameWorld.GetPlayer();
        if (player == null || !player.IsInitialized)
        {
            return screen == CurrentViews.CharacterScreen;
        }

        // Location-based screens require a current location
        if (IsLocationBasedScreen(screen))
        {
            return player.CurrentLocation != null && !string.IsNullOrEmpty(player.CurrentLocation.Id);
        }

        // Letter board only available at dawn
        if (screen == CurrentViews.LetterBoardScreen)
        {
            return _gameWorld.TimeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn;
        }

        return true;
    }

    /// <summary>
    /// Get the navigation context for a screen
    /// </summary>
    public NavigationContext GetContext(CurrentViews screen)
    {
        return screen switch
        {
            CurrentViews.LetterQueueScreen or
            CurrentViews.LetterBoardScreen or
            CurrentViews.RelationshipScreen or
            CurrentViews.ObligationsScreen => NavigationContext.Queue,

            CurrentViews.LocationScreen or
            CurrentViews.MapScreen or
            CurrentViews.TravelScreen or
            CurrentViews.MarketScreen or
            CurrentViews.RestScreen => NavigationContext.Location,

            CurrentViews.CharacterScreen or
            CurrentViews.PlayerStatusScreen => NavigationContext.Character,

            _ => NavigationContext.System
        };
    }

    /// <summary>
    /// Get contextual screens available from current context
    /// </summary>
    public List<CurrentViews> GetContextualScreens()
    {
        NavigationContext context = GetContext(_currentScreen);

        return context switch
        {
            NavigationContext.Queue => new List<CurrentViews>(),  // Queue is the hub, no sub-screens

            NavigationContext.Location => new List<CurrentViews>
            {
                CurrentViews.MapScreen,
                CurrentViews.MarketScreen,
                CurrentViews.RestScreen,
                CurrentViews.TravelScreen
            },

            NavigationContext.Character => new List<CurrentViews>
            {
                CurrentViews.PlayerStatusScreen,
                CurrentViews.RelationshipScreen,
                CurrentViews.ObligationsScreen
            },

            _ => new List<CurrentViews>()
        };
    }

    /// <summary>
    /// Check if a screen is location-based
    /// </summary>
    private bool IsLocationBasedScreen(CurrentViews screen)
    {
        return screen == CurrentViews.LocationScreen ||
               screen == CurrentViews.MapScreen ||
               screen == CurrentViews.TravelScreen ||
               screen == CurrentViews.MarketScreen ||
               screen == CurrentViews.RestScreen;
    }

    /// <summary>
    /// Check if a screen is a primary gameplay screen
    /// </summary>
    public bool IsPrimaryGameplayScreen(CurrentViews screen)
    {
        return screen == CurrentViews.LetterQueueScreen ||
               screen == CurrentViews.LetterBoardScreen ||
               screen == CurrentViews.RelationshipScreen ||
               screen == CurrentViews.ObligationsScreen;
    }

    /// <summary>
    /// Get the default view for the current game state
    /// </summary>
    public CurrentViews GetDefaultView()
    {
        Player player = _gameWorld.GetPlayer();

        // No player = character creation
        if (player == null || !player.IsInitialized)
        {
            return CurrentViews.CharacterScreen;
        }

        // Dawn = letter board available
        if (_gameWorld.TimeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn)
        {
            return CurrentViews.LetterBoardScreen;
        }

        // Default to letter queue (primary interface)
        return CurrentViews.LetterQueueScreen;
    }
}

/// <summary>
/// Navigation context categories
/// </summary>
public enum NavigationContext
{
    Queue,      // Letter Queue management
    Location,   // Location-based activities
    Character,  // Character management
    System      // System/meta functions
}

/// <summary>
/// Result of a navigation attempt
/// </summary>
public class NavigationResult
{
    public bool IsSuccess { get; init; }
    public CurrentViews FromScreen { get; init; }
    public CurrentViews ToScreen { get; init; }
    public string Message { get; init; }
    public bool Changed => FromScreen != ToScreen;

    public static NavigationResult Success(CurrentViews from, CurrentViews to)
    {
        return new() { IsSuccess = true, FromScreen = from, ToScreen = to };
    }

    public static NavigationResult NoChange(CurrentViews current)
    {
        return new() { IsSuccess = true, FromScreen = current, ToScreen = current, Message = "Already on this screen" };
    }

    public static NavigationResult Blocked(CurrentViews current, string reason)
    {
        return new() { IsSuccess = false, FromScreen = current, ToScreen = current, Message = reason };
    }
}