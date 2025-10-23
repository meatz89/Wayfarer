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
    /// Screen navigation target if intent triggers navigation (null if no navigation)
    /// Backend decides: CheckBelongings → Equipment, Travel action → TravelScreen
    /// </summary>
    public ScreenNavigation? Navigation { get; set; }

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
    /// Create result that triggers screen navigation
    /// Navigation-only actions don't require location refresh
    /// </summary>
    public static IntentResult NavigateTo(ScreenNavigation target)
    {
        return new IntentResult
        {
            Success = true,
            Navigation = target,
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

/// <summary>
/// Screen navigation targets that intents can trigger.
/// Backend authority: Backend decides navigation targets, UI just navigates.
/// </summary>
public enum ScreenNavigation
{
    /// <summary>
    /// Navigate to equipment/inventory view
    /// Triggered by: CheckBelongingsIntent
    /// </summary>
    Equipment,

    /// <summary>
    /// Navigate to travel/routes screen
    /// Triggered by: TravelIntent (opens travel UI)
    /// </summary>
    TravelScreen
}
