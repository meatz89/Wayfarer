using System;
using Microsoft.AspNetCore.Components;
using Wayfarer.GameState;
using Wayfarer.UIHelpers;

namespace Wayfarer.Services;

/// <summary>
/// Centralized navigation management service for the UI
/// Ensures proper navigation flow based on letter queue paradigm
/// </summary>
public class NavigationService
{
    private readonly GameWorldManager _gameWorldManager;
    private CurrentViews _currentView = CurrentViews.LetterQueueScreen;
    private CurrentViews? _previousView = null;
    
    public NavigationService(GameWorldManager gameWorldManager)
    {
        _gameWorldManager = gameWorldManager;
    }
    
    /// <summary>
    /// Current active view
    /// </summary>
    public CurrentViews CurrentView => _currentView;
    
    /// <summary>
    /// Previous view for back navigation
    /// </summary>
    public CurrentViews? PreviousView => _previousView;
    
    /// <summary>
    /// Event raised when navigation occurs
    /// </summary>
    public event Action? OnNavigationChanged;
    
    /// <summary>
    /// Navigate to a specific view with validation
    /// </summary>
    public void NavigateTo(CurrentViews targetView)
    {
        // Validate navigation based on game state
        if (!CanNavigateTo(targetView))
        {
            return;
        }
        
        // Store previous view for back navigation
        _previousView = _currentView;
        _currentView = targetView;
        
        // Raise navigation event
        OnNavigationChanged?.Invoke();
    }
    
    /// <summary>
    /// Navigate back to previous view
    /// </summary>
    public void NavigateBack()
    {
        if (_previousView.HasValue)
        {
            var temp = _currentView;
            _currentView = _previousView.Value;
            _previousView = temp;
            OnNavigationChanged?.Invoke();
        }
    }
    
    /// <summary>
    /// Navigate to the primary gameplay screen (Letter Queue)
    /// </summary>
    public void NavigateToHome()
    {
        NavigateTo(CurrentViews.LetterQueueScreen);
    }
    
    /// <summary>
    /// Check if navigation to a specific view is allowed
    /// </summary>
    public bool CanNavigateTo(CurrentViews targetView)
    {
        var gameWorld = _gameWorldManager.GameWorld;
        
        // Special navigation rules based on game state
        return targetView switch
        {
            // Letter screens always accessible after character creation
            CurrentViews.LetterQueueScreen => gameWorld.GetPlayer() != null,
            CurrentViews.LetterBoardScreen => gameWorld.GetPlayer() != null,
            CurrentViews.RelationshipScreen => gameWorld.GetPlayer() != null,
            CurrentViews.ObligationsScreen => gameWorld.GetPlayer() != null,
            
            // Location screens require player to be at a location
            CurrentViews.LocationScreen => gameWorld.GetPlayer()?.CurrentLocation != null,
            CurrentViews.MarketScreen => gameWorld.GetPlayer()?.CurrentLocation != null,
            
            // Travel is always accessible if player exists
            CurrentViews.TravelScreen => gameWorld.GetPlayer() != null,
            
            // Always accessible if player exists
            CurrentViews.CharacterScreen => gameWorld.GetPlayer() != null,
            CurrentViews.PlayerStatusScreen => gameWorld.GetPlayer() != null,
            CurrentViews.MapScreen => gameWorld.GetPlayer() != null,
            CurrentViews.RestScreen => gameWorld.GetPlayer() != null,
            
            // Event screens are controlled by game events
            CurrentViews.EncounterScreen => true, // Managed by encounter system
            CurrentViews.NarrativeScreen => true, // Managed by narrative system
            
            // System screens
            CurrentViews.MissingReferences => true,
            
            _ => true
        };
    }
    
    /// <summary>
    /// Get appropriate default view based on game state
    /// </summary>
    public CurrentViews GetDefaultView()
    {
        var gameWorld = _gameWorldManager.GameWorld;
        
        // No player yet - character creation
        if (gameWorld.GetPlayer() == null)
        {
            return CurrentViews.CharacterScreen;
        }
        
        // Default to Letter Queue Screen if player has no location yet
        if (gameWorld.GetPlayer().CurrentLocation == null)
        {
            return CurrentViews.LetterQueueScreen;
        }
        
        // Default to Letter Queue Screen (primary gameplay)
        return CurrentViews.LetterQueueScreen;
    }
    
    /// <summary>
    /// Get navigation category for a view
    /// </summary>
    public static string GetViewCategory(CurrentViews view)
    {
        return view switch
        {
            CurrentViews.LetterQueueScreen or 
            CurrentViews.LetterBoardScreen or 
            CurrentViews.RelationshipScreen or 
            CurrentViews.ObligationsScreen => "Letter Management",
            
            CurrentViews.LocationScreen or 
            CurrentViews.MapScreen or 
            CurrentViews.TravelScreen => "Navigation",
            
            CurrentViews.MarketScreen or 
            CurrentViews.RestScreen => "Activities",
            
            CurrentViews.EncounterScreen or 
            CurrentViews.NarrativeScreen => "Events",
            
            CurrentViews.CharacterScreen or 
            CurrentViews.PlayerStatusScreen => "Character",
            
            _ => "System"
        };
    }
    
    /// <summary>
    /// Check if a view is a primary gameplay screen
    /// </summary>
    public static bool IsPrimaryGameplayScreen(CurrentViews view)
    {
        return view is CurrentViews.LetterQueueScreen or 
                      CurrentViews.LetterBoardScreen or 
                      CurrentViews.RelationshipScreen or 
                      CurrentViews.ObligationsScreen;
    }
}