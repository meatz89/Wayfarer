using System;
using System.Threading.Tasks;
using Wayfarer.Pages;

namespace Wayfarer.Services
{
    /// <summary>
    /// Simple navigation coordinator that ensures valid screen transitions
    /// without over-engineering. Focuses on the 5 core screens actually used.
    /// </summary>
    public class NavigationCoordinator
    {
        private readonly GameFacade _gameFacade;
        private readonly ITimeManager _timeManager;
        
        private CurrentViews _currentView = CurrentViews.LocationScreen;
        private bool _isTransitioning = false;
        
        // Context for certain screens
        private string _currentNpcId = null;
        private string _targetLocationId = null;
        
        public CurrentViews CurrentView => _currentView;
        public bool IsTransitioning => _isTransitioning;
        
        public NavigationCoordinator(GameFacade gameFacade, ITimeManager timeManager)
        {
            _gameFacade = gameFacade;
            _timeManager = timeManager;
        }
        
        /// <summary>
        /// Navigate to a screen with validation
        /// </summary>
        public async Task<bool> NavigateToAsync(CurrentViews targetView, object context = null)
        {
            if (_isTransitioning)
                return false;
                
            // Validate transition
            if (!CanNavigateTo(targetView, context))
                return false;
            
            _isTransitioning = true;
            try
            {
                // Store context for certain transitions
                if (targetView == CurrentViews.ConversationScreen && context is string npcId)
                {
                    _currentNpcId = npcId;
                }
                else if (targetView == CurrentViews.TravelScreen && context is string locationId)
                {
                    _targetLocationId = locationId;
                }
                
                // Perform any cleanup for current screen
                await OnLeavingScreen(_currentView);
                
                // Update state
                var previousView = _currentView;
                _currentView = targetView;
                
                // Initialize new screen
                await OnEnteringScreen(targetView, context);
                
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
                           targetView == CurrentViews.LetterQueueScreen ||
                           targetView == CurrentViews.TravelScreen ||
                           (targetView == CurrentViews.LetterBoardScreen && _timeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn);
                
                case CurrentViews.ConversationScreen:
                    // From conversation, can only return to location
                    return targetView == CurrentViews.LocationScreen;
                
                case CurrentViews.LetterQueueScreen:
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
                return "Letter board only opens at dawn";
            
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
                    if (_currentNpcId != null)
                    {
                        await _gameFacade.EndConversationAsync();
                        _currentNpcId = null;
                    }
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
                        await _gameFacade.StartConversationAsync(npcId);
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
        
        public async Task<bool> OpenLetterQueueAsync()
        {
            return await NavigateToAsync(CurrentViews.LetterQueueScreen);
        }
        
        public async Task<bool> StartConversationAsync(string npcId)
        {
            return await NavigateToAsync(CurrentViews.ConversationScreen, npcId);
        }
        
        public async Task<bool> OpenTravelSelectionAsync(string targetLocationId = null)
        {
            return await NavigateToAsync(CurrentViews.TravelScreen, targetLocationId);
        }
    }
}