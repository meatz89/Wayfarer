# GameFacade Migration Guide

*Created: 2025-01-28*

## Overview

This guide helps developers migrate UI components from direct service injection to the GameFacade pattern. This migration is MANDATORY for all UI components in the Wayfarer codebase.

## Why Migrate to GameFacade?

1. **Testability**: UI components can be tested with mock IGameFacade implementations
2. **Consistency**: All UI components interact with the backend the same way
3. **Maintainability**: Changes to backend services don't affect UI components
4. **Clean Architecture**: Clear separation between presentation and domain layers
5. **No Circular Dependencies**: Prevents the startup hangs that occur with direct service injection

## Migration Checklist

### Step 1: Identify Components Needing Migration

Look for these patterns in `.razor` files:

```razor
❌ BAD - Direct service injection:
@inject GameWorld GameWorld
@inject TravelManager TravelManager
@inject CommandExecutor CommandExecutor
@inject NPCRepository NPCRepository
@inject LetterQueueService LetterQueueService
```

### Step 2: Replace Service Injections

Replace ALL service injections with a single IGameFacade:

```razor
✅ GOOD - Single facade injection:
@inject IGameFacade GameFacade
```

### Step 3: Update Data Access Patterns

#### Before (Direct Domain Access):
```csharp
// ❌ Accessing domain objects directly
var player = GameWorld.GetPlayer();
var location = player.CurrentLocation;
var npcs = NPCRepository.GetNPCsAtLocation(location.Id);
```

#### After (Using ViewModels):
```csharp
// ✅ Using facade methods and ViewModels
var snapshot = GameFacade.GetGameSnapshot();
var locationActions = GameFacade.GetLocationActions();
var npcs = locationActions.AvailableNPCs;
```

### Step 4: Update Command Execution

#### Before (Direct Command Execution):
```csharp
// ❌ Creating and executing commands directly
var command = new TravelCommand(destination, route);
var result = await CommandExecutor.ExecuteAsync(command);
if (result.Success)
{
    // Update UI
}
```

#### After (Using Facade Methods):
```csharp
// ✅ Using facade methods
var success = await GameFacade.TravelToDestinationAsync(destinationId, routeId);
if (success)
{
    // Refresh state
    var newSnapshot = GameFacade.GetGameSnapshot();
}
```

### Step 5: Update State Management

#### Before (Direct State Queries):
```csharp
// ❌ Multiple service calls to get state
var player = GameWorld.GetPlayer();
var queue = LetterQueueService.GetQueue();
var obligations = ObligationService.GetActiveObligations();
var tokens = TokenService.GetPlayerTokens();
```

#### After (Single State Query):
```csharp
// ✅ Single facade call for all state
var gameState = GameFacade.GetGameSnapshot();
// Access everything through the snapshot
var health = gameState.Health;
var queue = gameState.LetterQueue;
var obligations = gameState.Obligations;
```

## Common Migration Patterns

### Pattern 1: Location Actions

**Before:**
```razor
@inject LocationService LocationService
@inject CommandExecutor CommandExecutor

@code {
    private async Task PerformAction(string actionId)
    {
        var action = LocationService.GetAction(actionId);
        var command = action.CreateCommand();
        await CommandExecutor.ExecuteAsync(command);
    }
}
```

**After:**
```razor
@inject IGameFacade GameFacade

@code {
    private async Task PerformAction(string actionId)
    {
        var success = await GameFacade.ExecuteLocationActionAsync(actionId);
        if (success)
        {
            // Refresh UI state
            StateHasChanged();
        }
    }
}
```

### Pattern 2: NPC Interactions

**Before:**
```razor
@inject NPCRepository NPCRepository
@inject ConversationManager ConversationManager

@code {
    private async Task StartConversation(string npcId)
    {
        var npc = NPCRepository.GetNPC(npcId);
        var conversation = await ConversationManager.StartConversation(npc);
        // Handle conversation UI
    }
}
```

**After:**
```razor
@inject IGameFacade GameFacade

@code {
    private async Task StartConversation(string npcId)
    {
        var conversation = await GameFacade.StartConversationAsync(npcId);
        if (conversation != null)
        {
            // Handle conversation UI with ViewModel
        }
    }
}
```

### Pattern 3: Letter Queue Operations

**Before:**
```razor
@inject LetterQueueService QueueService
@inject TokenService TokenService

@code {
    private async Task PurgeBottomLetter()
    {
        var cost = QueueService.GetPurgeCost();
        if (TokenService.HasTokens(cost))
        {
            TokenService.SpendTokens(cost);
            QueueService.PurgeBottomLetter();
        }
    }
}
```

**After:**
```razor
@inject IGameFacade GameFacade

@code {
    private async Task PurgeBottomLetter()
    {
        var success = await GameFacade.PurgeBottomLetterAsync();
        if (success)
        {
            // Action succeeded, tokens were spent
            var updatedQueue = GameFacade.GetLetterQueue();
        }
    }
}
```

## Testing After Migration

### Unit Testing UI Components

```csharp
[Test]
public async Task LocationActions_ExecuteAction_UpdatesUI()
{
    // Arrange
    var mockFacade = new Mock<IGameFacade>();
    mockFacade.Setup(f => f.ExecuteLocationActionAsync(It.IsAny<string>()))
              .ReturnsAsync(true);
    
    var component = new LocationActionsComponent
    {
        GameFacade = mockFacade.Object
    };
    
    // Act
    await component.ExecuteAction("rest");
    
    // Assert
    mockFacade.Verify(f => f.ExecuteLocationActionAsync("rest"), Times.Once);
}
```

### Integration Testing

```csharp
[Test]
public async Task FullWorkflow_ThroughGameFacade_Works()
{
    // Use real GameFacade with test database
    var services = new ServiceCollection();
    services.AddGameServices();
    var provider = services.BuildServiceProvider();
    
    var facade = provider.GetRequiredService<IGameFacade>();
    
    // Start game
    await facade.StartGameAsync();
    
    // Get initial state
    var snapshot = facade.GetGameSnapshot();
    Assert.NotNull(snapshot);
    
    // Execute action
    var actions = facade.GetLocationActions();
    await facade.ExecuteLocationActionAsync(actions.Commands[0].Id);
}
```

## Validation Checklist

After migrating a component, verify:

- [ ] Component only has `@inject IGameFacade GameFacade`
- [ ] No direct domain object usage (Player, Location, NPC classes)
- [ ] All data comes from ViewModels
- [ ] All actions go through GameFacade methods
- [ ] Component can be unit tested with mock IGameFacade
- [ ] No compilation warnings about unused injections

## Components Requiring Migration

As of 2025-01-28, the following components still need migration:

1. **TravelScreen.razor** - Still uses TravelManager directly
2. **MarketScreen.razor** - Direct MarketService usage
3. **RestScreen.razor** - Multiple service injections
4. **NPCInteractionModal.razor** - Direct ConversationManager usage
5. **CharacterStatusHub.razor** - Direct GameWorld access
6. **ObligationScreen.razor** - Direct ObligationService usage

## Anti-Patterns to Avoid

### ❌ Partial Migration
Don't leave some services while adding GameFacade:
```razor
@inject IGameFacade GameFacade
@inject GameWorld GameWorld  // ❌ Remove this!
```

### ❌ Accessing Services Through DI
Don't get services from the service provider:
```csharp
// ❌ NEVER DO THIS
var gameWorld = ServiceProvider.GetService<GameWorld>();
```

### ❌ Creating Domain Objects
Don't create domain objects in UI:
```csharp
// ❌ Domain object in UI layer
var command = new TravelCommand(destination);
```

### ❌ Type Casting ViewModels
Don't cast ViewModels back to domain objects:
```csharp
// ❌ Breaking the abstraction
var location = (Location)locationViewModel;
```

## Getting Help

If you encounter scenarios not covered in this guide:

1. Check `IGameFacade` to see if a method already exists
2. If not, add the method to IGameFacade and implement in GameFacade
3. Create appropriate ViewModels for data transfer
4. Ensure the new method follows the same patterns as existing ones

Remember: The goal is complete separation between UI and domain layers. When in doubt, keep domain logic in the backend and UI logic in the components.