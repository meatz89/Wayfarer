/// <summary>
/// Interface for handling navigation changes in the application.
/// Replaces event-based navigation to maintain clean architecture.
/// </summary>
public interface INavigationHandler
{
    /// <summary>
    /// Called when navigation occurs between screens.
    /// </summary>
    /// <param name="previousScreen">The screen being navigated from</param>
    /// <param name="newScreen">The screen being navigated to</param>
    void HandleNavigationChange(CurrentViews previousScreen, CurrentViews newScreen);
}