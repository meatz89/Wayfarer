using System;
using System.Threading.Tasks;
using Wayfarer.Pages;

/// <summary>
/// Four-modal focus system coordinator to manage cognitive load
/// Enforces clear transitions between Map, Conversation, Queue, and Route Planning modes
/// </summary>
public class NavigationCoordinator
{
    private readonly GameFacade _gameFacade;
    private readonly TimeManager _timeManager;

    private CurrentViews _currentView = CurrentViews.LocationScreen; // Default to Map Mode
    private CurrentViews _previousView = CurrentViews.LocationScreen;
    private bool _isTransitioning = false;
    
    // Event to notify components of navigation changes
    public event Action OnNavigationChanged;

    // Modal state tracking
    public enum ModalState
    {
        MapMode,           // LocationScreen - city overview with NPCs
        ConversationMode,  // ConversationScreen - NPC interactions
        QueueMode,         // ObligationQueueScreen - obligation management
        RoutePlanningMode  // TravelScreen - travel decisions
    }

    // Context for certain screens
    private string _targetLocationId = null;

    public CurrentViews CurrentView => _currentView;
    public bool IsTransitioning => _isTransitioning;
    
    public ModalState CurrentModalState => GetModalState(_currentView);

    public NavigationCoordinator(GameFacade gameFacade, TimeManager timeManager)
    {
        _gameFacade = gameFacade;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Navigate to a screen with validation
    /// </summary>
    public async Task<bool> NavigateToAsync(CurrentViews targetView, object context = null)
    {
        Console.WriteLine($"[NavigationCoordinator] NavigateToAsync called: {targetView}, current: {_currentView}");
        
        if (_isTransitioning)
        {
            Console.WriteLine("[NavigationCoordinator] Already transitioning, rejecting");
            return false;
        }

        // Validate transition
        if (!CanNavigateTo(targetView, context))
        {
            Console.WriteLine($"[NavigationCoordinator] Cannot navigate from {_currentView} to {targetView}");
            return false;
        }

        _isTransitioning = true;
        try
        {
            // Store context for certain transitions
            if (targetView == CurrentViews.TravelScreen && context is string locationId)
            {
                _targetLocationId = locationId;
            }

            // Perform any cleanup for current screen
            await OnLeavingScreen(_currentView);

            // Update state
            CurrentViews previousView = _currentView;
            _previousView = previousView;
            _currentView = targetView;

            // Initialize new screen
            await OnEnteringScreen(targetView, context);
            
            // Notify listeners of navigation change
            OnNavigationChanged?.Invoke();

            return true;
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    /// <summary>
    /// Check if navigation to target is allowed from current state
    /// </summary>
    public bool CanNavigateTo(CurrentViews targetView, object context = null)
    {
        // Can't navigate while already transitioning
        if (_isTransitioning)
            return false;

        // Define allowed transitions
        switch (_currentView)
        {
            case CurrentViews.LocationScreen:
                // From location, can go to conversation, queue, travel, or letter board
                return targetView == CurrentViews.ConversationScreen ||
                       targetView == CurrentViews.ObligationQueueScreen ||
                       targetView == CurrentViews.TravelScreen ||
                       (targetView == CurrentViews.LetterBoardScreen && _timeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn);

            case CurrentViews.ConversationScreen:
                // From conversation, can only return to location
                return targetView == CurrentViews.LocationScreen;

            case CurrentViews.ObligationQueueScreen:
                // From queue, can return to location
                return targetView == CurrentViews.LocationScreen;

            case CurrentViews.TravelScreen:
                // From travel selection, can go to location (cancel or arrive)
                return targetView == CurrentViews.LocationScreen;

            case CurrentViews.LetterBoardScreen:
                // From letter board, return to location
                return targetView == CurrentViews.LocationScreen;

            default:
                return false;
        }
    }

    /// <summary>
    /// Get user-friendly reason why navigation is blocked
    /// </summary>
    public string GetBlockedReason(CurrentViews targetView)
    {
        if (_isTransitioning)
            return "Please wait...";

        if (_currentView == CurrentViews.ConversationScreen && targetView != CurrentViews.LocationScreen)
            return "Finish your conversation first";

        if (targetView == CurrentViews.LetterBoardScreen && _timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
            return "DeliveryObligation board only opens at dawn";

        if (targetView == CurrentViews.TravelScreen && _currentView == CurrentViews.ConversationScreen)
            return "Can't travel during a conversation";

        return "Can't go there from here";
    }

    private async Task OnLeavingScreen(CurrentViews screen)
    {
        switch (screen)
        {
            case CurrentViews.ConversationScreen:
                // End conversation if still active
                await _gameFacade.EndConversationAsync();
                break;

            case CurrentViews.TravelScreen:
                _targetLocationId = null;
                break;
        }
    }

    private async Task OnEnteringScreen(CurrentViews screen, object context)
    {
        switch (screen)
        {
            case CurrentViews.ConversationScreen:
                // Start conversation if NPC provided
                if (context is string npcId)
                {
                    await _gameFacade.CreateConversationContext(npcId);
                }
                break;

            case CurrentViews.LocationScreen:
                // Refresh location state
                _gameFacade.RefreshLocationState();
                break;
        }
    }

    /// <summary>
    /// Quick navigation helpers for common transitions
    /// </summary>
    public async Task<bool> ReturnToLocationAsync()
    {
        return await NavigateToAsync(CurrentViews.LocationScreen);
    }

    public async Task<bool> OpenObligationQueueAsync()
    {
        return await NavigateToAsync(CurrentViews.ObligationQueueScreen);
    }

    public async Task<bool> StartConversationAsync(string npcId)
    {
        return await NavigateToAsync(CurrentViews.ConversationScreen, npcId);
    }
    

    public async Task<bool> OpenTravelSelectionAsync(string targetLocationId = null)
    {
        return await NavigateToAsync(CurrentViews.TravelScreen, targetLocationId);
    }
    
    /// <summary>
    /// Convert CurrentViews to modal state for cognitive load management
    /// </summary>
    private ModalState GetModalState(CurrentViews view)
    {
        return view switch
        {
            CurrentViews.LocationScreen => ModalState.MapMode,
            CurrentViews.ConversationScreen => ModalState.ConversationMode,
            CurrentViews.ObligationQueueScreen => ModalState.QueueMode,
            CurrentViews.TravelScreen => ModalState.RoutePlanningMode,
            // All other screens default to map mode
            _ => ModalState.MapMode
        };
    }
    
    /// <summary>
    /// Check if we're in one of the four core modal states
    /// </summary>
    public bool IsInCoreModalState()
    {
        return _currentView == CurrentViews.LocationScreen ||
               _currentView == CurrentViews.ConversationScreen ||
               _currentView == CurrentViews.ObligationQueueScreen ||
               _currentView == CurrentViews.TravelScreen;
    }
}