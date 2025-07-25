using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of INavigationHandler for non-UI contexts.
/// In the actual game, MainGameplayView implements this interface.
/// </summary>
public class NavigationHandler : INavigationHandler
{
    private readonly ILogger<NavigationHandler> _logger;

    public NavigationHandler(ILogger<NavigationHandler> logger)
    {
        _logger = logger;
    }

    public void HandleNavigationChange(CurrentViews previousScreen, CurrentViews newScreen)
    {
        _logger.LogInformation("Navigation changed from {PreviousScreen} to {NewScreen}", 
            previousScreen, newScreen);
    }
}