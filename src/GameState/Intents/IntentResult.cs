/// <summary>
/// Result of processing a PlayerIntent through GameFacade.ProcessIntent().
/// Backend authority: Backend determines success, navigation, and refresh requirements.
/// UI interprets result without making decisions.
/// Strongly typed: NO object properties, only concrete types.
/// </summary>
public class IntentResult
{
    /// <summary>
    /// Whether the intent executed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Screen-level navigation if intent triggers screen change (null if no screen navigation)
    /// Backend decides: TravelIntent → ScreenMode.Travel
    /// </summary>
    public ScreenMode? NavigateToScreen { get; set; }

    /// <summary>
    /// View-level navigation if intent triggers view change within Location screen (null if no view navigation)
    /// Backend decides: CheckBelongingsIntent → LocationViewState.Equipment
    /// </summary>
    public LocationViewState? NavigateToView { get; set; }

    /// <summary>
    /// Whether UI should refresh location data after execution
    /// True for most actions (Rest, Work, etc), false for navigation-only actions
    /// </summary>
    public bool RequiresLocationRefresh { get; set; }

    /// <summary>
    /// Create result for successful execution with optional refresh
    /// </summary>
    public static IntentResult Executed(bool requiresRefresh = true)
    {
        return new IntentResult
        {
            Success = true,
            RequiresLocationRefresh = requiresRefresh
        };
    }

    /// <summary>
    /// Create result that navigates to different screen
    /// Screen navigation doesn't require location refresh
    /// </summary>
    public static IntentResult NavigateScreen(ScreenMode screen)
    {
        return new IntentResult
        {
            Success = true,
            NavigateToScreen = screen,
            RequiresLocationRefresh = false
        };
    }

    /// <summary>
    /// Create result that navigates to different view within Location screen
    /// View navigation doesn't require location refresh (just view change)
    /// </summary>
    public static IntentResult NavigateView(LocationViewState view)
    {
        return new IntentResult
        {
            Success = true,
            NavigateToView = view,
            RequiresLocationRefresh = false
        };
    }

    /// <summary>
    /// Create result for failed execution
    /// </summary>
    public static IntentResult Failed()
    {
        return new IntentResult
        {
            Success = false,
            RequiresLocationRefresh = false
        };
    }
}

