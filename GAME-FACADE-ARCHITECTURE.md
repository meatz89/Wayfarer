# GameFacade Architecture Pattern

## Overview

The GameFacade pattern is THE ONLY way UI components and test controllers should communicate with the game backend. This architectural pattern provides a single, well-defined interface between the presentation layer and the domain layer, ensuring consistency, testability, and maintainability.

## Core Principles

### 1. Single Point of Entry
- **ALL** UI-to-backend communication goes through `IGameFacade`
- **NO** direct service injection in UI components
- **NO** direct access to GameWorld, managers, or repositories from UI
- Test controllers use the **exact same interface** as UI components

### 2. Clean Separation of Concerns
```
UI Components / Test Controllers
        ↓
    IGameFacade (Contract)
        ↓
    GameFacade (Implementation)
        ↓
    UIServices (Domain Translation)
        ↓
    Domain Layer (GameWorld, Managers, etc.)
```

### 3. ViewModels for Data Transfer
- All data returned from GameFacade uses ViewModels
- ViewModels are UI-specific DTOs that contain only what the UI needs
- Domain objects never leak to the UI layer

## Architecture Flow

### 1. UI Component Request Flow
```csharp
// In a Blazor component
@inject IGameFacade GameFacade

// Making a request
var travelOptions = await GameFacade.GetTravelDestinations();
var result = await GameFacade.TravelToDestinationAsync(destinationId, routeId);
```

### 2. GameFacade Processing
```csharp
public class GameFacade : IGameFacade
{
    private readonly TravelUIService _travelService;
    private readonly LetterQueueUIService _letterService;
    // ... other UI services

    public async Task<List<TravelDestinationViewModel>> GetTravelDestinations()
    {
        // Delegates to specialized UI service
        return await _travelService.GetAvailableDestinations();
    }

    public async Task<bool> TravelToDestinationAsync(string destinationId, string routeId)
    {
        // Validation and delegation
        if (string.IsNullOrEmpty(destinationId))
            return false;
            
        return await _travelService.ExecuteTravel(destinationId, routeId);
    }
}
```

### 3. UIService Domain Translation
```csharp
public class TravelUIService
{
    private readonly GameWorld _gameWorld;
    private readonly TravelManager _travelManager;
    private readonly CommandExecutor _commandExecutor;

    public async Task<List<TravelDestinationViewModel>> GetAvailableDestinations()
    {
        // Translate domain objects to ViewModels
        var player = _gameWorld.GetPlayer();
        var destinations = _travelManager.GetAvailableDestinations(player.CurrentLocation);
        
        return destinations.Select(d => new TravelDestinationViewModel
        {
            DestinationId = d.Id,
            Name = d.Name,
            Distance = d.Distance,
            IsAccessible = d.MeetsRequirements(player)
        }).ToList();
    }
}
```

## Benefits

### 1. Testability
```csharp
// Test controllers use the same interface
[ApiController]
public class GameTestController : ControllerBase
{
    private readonly IGameFacade _gameFacade;
    
    [HttpPost("travel")]
    public async Task<IActionResult> Travel(string destination)
    {
        // Same interface as UI - tests are realistic
        var result = await _gameFacade.TravelToDestinationAsync(destination, "walking");
        return Ok(result);
    }
}
```

### 2. Consistency
- All UI components interact with the game the same way
- No confusion about which service to inject
- Clear contract for what operations are available

### 3. Maintainability
- Changes to domain layer don't affect UI directly
- Can refactor backend without touching UI components
- Easy to add new features - just extend IGameFacade

### 4. Comprehensive Testing
```csharp
// E2E tests can use the facade
public class TutorialE2ETest
{
    private readonly IGameFacade _gameFacade;
    
    public async Task RunTutorial()
    {
        // Start game
        await _gameFacade.StartGameAsync();
        
        // Check tutorial state
        var tutorialActive = _gameFacade.IsTutorialActive();
        Assert.True(tutorialActive);
        
        // Execute tutorial actions
        var actions = _gameFacade.GetLocationActions();
        await _gameFacade.ExecuteLocationActionAsync(actions.Commands[0].Id);
        
        // All interactions through the same interface!
    }
}
```

## Implementation Examples

### Example 1: Location Actions
```csharp
// UI Component
@code {
    private LocationActionsViewModel actions;
    
    protected override async Task OnInitializedAsync()
    {
        actions = GameFacade.GetLocationActions();
    }
    
    private async Task ExecuteAction(string commandId)
    {
        var success = await GameFacade.ExecuteLocationActionAsync(commandId);
        if (success)
        {
            // Refresh view
            actions = GameFacade.GetLocationActions();
        }
    }
}
```

### Example 2: Conversations
```csharp
// Starting a conversation
var conversation = await GameFacade.StartConversationAsync(npcId);

// Making choices
var updatedConversation = await GameFacade.ContinueConversationAsync(choiceId);

// Checking state
var currentConversation = GameFacade.GetCurrentConversation();
if (currentConversation?.IsComplete ?? false)
{
    // Handle completion
}
```

### Example 3: Letter Queue Management
```csharp
// Get current queue state
var letterQueue = GameFacade.GetLetterQueue();

// Execute actions
await GameFacade.ExecuteLetterActionAsync("deliver", letterId);
await GameFacade.ExecuteLetterActionAsync("refuse", letterId);

// Accept new letters
var letterBoard = GameFacade.GetLetterBoard();
await GameFacade.AcceptLetterOfferAsync(offerId);
```

## Migration Guide

### Step 1: Identify Direct Service Usage
Look for patterns like:
```csharp
// OLD - Direct injection
@inject GameWorld GameWorld
@inject TravelManager TravelManager
@inject CommandExecutor CommandExecutor

// NEW - Single facade
@inject IGameFacade GameFacade
```

### Step 2: Replace Service Calls
```csharp
// OLD
var player = GameWorld.GetPlayer();
var destinations = TravelManager.GetDestinations(player.CurrentLocation);
var command = new TravelCommand(destination);
CommandExecutor.Execute(command);

// NEW
var destinations = await GameFacade.GetTravelDestinations();
await GameFacade.TravelToDestinationAsync(destinationId, routeId);
```

### Step 3: Update Data Models
```csharp
// OLD - Using domain objects
Location currentLocation = player.CurrentLocation;
List<NPC> npcs = locationService.GetNPCs();

// NEW - Using ViewModels
var snapshot = GameFacade.GetGameSnapshot();
var locationActions = GameFacade.GetLocationActions();
```

### Step 4: Update Tests
```csharp
// OLD - Complex setup with multiple services
var gameWorld = new GameWorld();
var travelManager = new TravelManager(gameWorld);
// ... setup continues

// NEW - Simple facade usage
var facade = serviceProvider.GetRequiredService<IGameFacade>();
await facade.StartGameAsync();
var result = await facade.TravelToDestinationAsync("town_square", "walking");
```

## Common Patterns

### 1. Query-Then-Act
```csharp
// First query available options
var restOptions = GameFacade.GetRestOptions();

// Then execute chosen action
if (restOptions.Options.Any())
{
    await GameFacade.ExecuteRestAsync(restOptions.Options[0].Id);
}
```

### 2. State Refresh After Actions
```csharp
private async Task RefreshGameState()
{
    // Get all relevant state
    var snapshot = GameFacade.GetGameSnapshot();
    var actions = GameFacade.GetLocationActions();
    var letterQueue = GameFacade.GetLetterQueue();
    
    // Update UI
    StateHasChanged();
}
```

### 3. Tutorial-Aware Actions
```csharp
private async Task<bool> CanExecuteAction(string actionId)
{
    if (GameFacade.IsTutorialActive())
    {
        var guidance = GameFacade.GetTutorialGuidance();
        return guidance.AllowedActions.Contains(actionId);
    }
    return true;
}
```

## Anti-Patterns to Avoid

### ❌ Direct Service Injection
```csharp
// NEVER DO THIS IN UI
@inject GameWorld GameWorld
@inject NPCRepository NPCRepository
@inject CommandExecutor CommandExecutor
```

### ❌ Bypassing the Facade
```csharp
// NEVER DO THIS
var gameWorld = serviceProvider.GetService<GameWorld>();
gameWorld.GetPlayer().Coins += 100; // Direct manipulation!
```

### ❌ Creating Domain Objects in UI
```csharp
// NEVER DO THIS
var command = new TravelCommand(destination); // Domain object in UI!
```

### ❌ Exposing Domain Objects
```csharp
// NEVER DO THIS
public interface IGameFacade
{
    Player GetPlayer(); // Returns domain object - BAD!
    GameWorld GetGameWorld(); // Exposes entire domain - BAD!
}
```

## Summary

The GameFacade pattern is not optional - it is THE architectural pattern for UI-backend communication in Wayfarer. By following this pattern:

1. **UI components** remain simple and focused on presentation
2. **Tests** use the same interface as production code
3. **Backend changes** don't break UI components
4. **New features** are easy to add and test
5. **Code quality** improves through clear separation of concerns

Remember: If you're in a UI component or test controller and need to interact with the game, you should ONLY be using `IGameFacade`. No exceptions.