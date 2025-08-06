# Intent-Based Architecture Design

## Overview

This document describes the target architecture for replacing the backwards CommandDiscoveryService with a clean intent-based system where commands only capture player intent, and backend services handle execution using GameWorld as the single source of truth.

## Current Problems with CommandDiscoveryService

1. **Pre-execution Discovery**: Creates and validates all possible commands upfront, even though 99% won't be executed
2. **Massive Dependencies**: Requires 30+ dependencies to construct commands that may never run
3. **ID Mismatch**: UI expects simple IDs (`move_lower_ward_square`) but gets complex generated IDs (`TravelToSpotCommand_12345-guid`)
4. **Double Discovery**: Commands are discovered twice - once for display, once for execution
5. **Context in Commands**: Commands contain execution logic and fetch their own context, violating single source of truth
6. **Legacy GameWorldManager**: GameWorldManager is legacy code with 20+ service dependencies that should be removed. Its responsibilities belong in GameFacade or as direct GameWorld methods

## Target Architecture Principles

### 1. Commands as Pure Intent

Commands should ONLY represent what the player wants to do, without any execution logic, validation, or context fetching.

```csharp
// WRONG - Command with dependencies and logic
public class TravelToSpotCommand : BaseGameCommand
{
    private readonly LocationRepository _locationRepository;
    private readonly LocationSpotRepository _spotRepository;
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Command fetching its own context - WRONG!
        var targetSpot = _spotRepository.GetAllLocationSpots().FirstOrDefault(s => s.SpotID == _targetSpotId);
        // ...
    }
}

// RIGHT - Pure intent object
public class MoveIntent : PlayerIntent
{
    public string TargetSpotId { get; }
    
    public MoveIntent(string targetSpotId)
    {
        TargetSpotId = targetSpotId ?? throw new ArgumentNullException(nameof(targetSpotId));
    }
}
```

### 2. GameWorld as Single Source of Truth

All game state and context must come from GameWorld during execution, not from repositories or services injected into commands.

```csharp
// Backend service handles execution
public async Task<bool> ExecuteIntent(MoveIntent intent)
{
    // Get ALL context from GameWorld
    var player = _gameWorld.GetPlayer();
    var currentSpot = player.CurrentLocationSpot;
    var targetSpot = _gameWorld.GetSpot(intent.TargetSpotId);
    
    // Validate using GameWorld context
    if (targetSpot == null || targetSpot.IsClosed)
    {
        _messageSystem.AddSystemMessage("Cannot move to that location", SystemMessageTypes.Warning);
        return false;
    }
    
    if (player.Stamina < 1)
    {
        _messageSystem.AddSystemMessage("Not enough stamina to move", SystemMessageTypes.Warning);
        return false;
    }
    
    // Execute by updating GameWorld
    _gameWorld.MovePlayer(intent.TargetSpotId);
    _gameWorld.SpendPlayerStamina(1);
    
    _messageSystem.AddSystemMessage($"Moved to {targetSpot.Name}", SystemMessageTypes.Success);
    return true;
}
```

### 3. Direct UI-to-Backend Communication

UI creates intent objects directly and passes them to GameFacade. No discovery needed.

```csharp
// UI component
async Task HandleMoveClick(LocationSpot targetSpot)
{
    var intent = new MoveIntent(targetSpot.SpotID);
    var success = await GameFacade.ExecuteIntent(intent);
    if (success) RefreshUI();
}

// GameFacade
public async Task<bool> ExecuteIntent(PlayerIntent intent)
{
    return intent switch
    {
        MoveIntent move => await ExecuteMove(move),
        TalkIntent talk => await ExecuteTalk(talk),
        RestIntent rest => await ExecuteRest(rest),
        _ => throw new NotSupportedException($"Unknown intent type: {intent.GetType()}")
    };
}
```

## Core Intent Types

### Movement
```csharp
public class MoveIntent : PlayerIntent
{
    public string TargetSpotId { get; }
}

public class TravelIntent : PlayerIntent
{
    public string RouteId { get; }
}
```

### Social
```csharp
public class TalkIntent : PlayerIntent
{
    public string NpcId { get; }
}

public class DiscoverRouteIntent : PlayerIntent
{
    public string NpcId { get; }
    public string RouteId { get; }
}
```

### Rest & Recovery
```csharp
public class RestIntent : PlayerIntent
{
    public int Hours { get; }
}
```

### Letters
```csharp
public class DeliverLetterIntent : PlayerIntent
{
    public string LetterId { get; }
}

public class CollectLetterIntent : PlayerIntent
{
    public string LetterId { get; }
}

public class AcceptLetterOfferIntent : PlayerIntent
{
    public string OfferId { get; }
}
```

### Exploration
```csharp
public class ObserveLocationIntent : PlayerIntent { }

public class ExploreAreaIntent : PlayerIntent { }
```

## Implementation Strategy (REVISED)

### Phase 1: ✅ Delete Legacy System (COMPLETED)
1. Created PlayerIntent base class and concrete intent types
2. Added ExecuteIntent method to GameFacade
3. Aggressively deleted CommandDiscoveryService, GameWorldManager, etc.
4. No compatibility layers - clean break

### Phase 2: 🔄 Fix Build Errors (IN PROGRESS)
1. Create missing types (MarketItem) or remove references
2. Update UI components to remove GameWorldManager
3. Replace NarrativeManager with ConversationRepository
4. Fix all compilation errors before proceeding

### Phase 3: Complete Intent Implementation
1. Implement remaining intent executors
2. Update GetLocationActions() for stable IDs
3. Ensure all UI actions use intents
4. Test with E2E suite

## Benefits

1. **Simplicity**: No complex discovery, just simple intent objects
2. **Performance**: No pre-creation of unused commands
3. **Maintainability**: Clear separation between intent and execution
4. **Testability**: Easy to test intent execution without complex setup
5. **Flexibility**: Easy to add new intent types without modifying discovery
6. **Single Source of Truth**: All context comes from GameWorld

## Example: Fixing the "Move Here" Button

### Current (Broken)
```csharp
// UI expects
commandId = "move_lower_ward_square"

// CommandDiscovery generates
commandId = "TravelToSpotCommand_12345-guid"

// Result: Command not found!
```

### New Intent-Based
```csharp
// UI creates intent
var intent = new MoveIntent("lower_ward_square");
await GameFacade.ExecuteIntent(intent);

// GameFacade handles it
private async Task<bool> ExecuteMove(MoveIntent intent)
{
    var player = _gameWorld.GetPlayer();
    var targetSpot = _gameWorld.GetSpot(intent.TargetSpotId);
    
    if (player.Stamina < 1)
    {
        _messageSystem.AddSystemMessage("Not enough stamina", SystemMessageTypes.Warning);
        return false;
    }
    
    _gameWorld.MovePlayer(intent.TargetSpotId);
    _gameWorld.SpendPlayerStamina(1);
    
    _messageSystem.AddSystemMessage($"Moved to {targetSpot.Name}", SystemMessageTypes.Success);
    return true;
}
```

## Migration Notes

- Start with high-impact, broken features (like "Move Here")
- Keep both systems running during migration
- Test extensively with E2E tests
- Document any special cases or gotchas discovered during migration

## Implementation Progress

### Completed
- ✅ Created PlayerIntent base class and concrete intent types
- ✅ Added ExecuteIntent method to GameFacade with pattern matching
- ✅ Implemented ExecuteMove, ExecuteTalk, ExecuteRest, ExecuteObserve, ExecuteTravel
- ✅ Updated MainGameplayView.HandleSpotSelection to use MoveIntent
- ✅ Deleted entire Commands directory and CommandDiscoveryService
- ✅ Deleted GameWorldManager
- ✅ Deleted CommandExecutor
- ✅ Deleted NarrativeManager, GameStateManager, ActionExecutionService
- ✅ Created MarketItem type to fix missing reference
- ✅ Repurposed Phase4_Narratives as Phase4_Conversations to load conversation JSON
- ✅ Created ConversationRepository to replace NarrativeManager dialogue functionality
- ✅ Updated DeterministicNarrativeProvider to use ConversationRepository

### In Progress (Build Errors - 36 remaining)
- 🔄 Removing NarrativeManager references from UI components
- 🔄 Removing GameWorldManager references from UI components
- 🔄 Fixing missing type references (NarrativeDefinition, NarrativeState, etc.)

### Todo - Intent Implementations
- ❌ ExecuteDeliverLetter(DeliverLetterIntent)
- ❌ ExecuteCollectLetter(CollectLetterIntent)
- ❌ ExecuteExplore(ExploreAreaIntent)
- ❌ ExecutePatronFunds(PatronFundsIntent)
- ❌ ExecuteAcceptOffer(AcceptLetterOfferIntent)
- ❌ ExecuteDiscoverRoute(DiscoverRouteIntent)
- ❌ ExecuteConvertEndorsements(ConvertEndorsementsIntent)

### Todo - UI Updates
- ❌ LocationSpotMap.razor.cs - remove GameWorldManager
- ❌ PlayerStatusView.razor.cs - remove GameWorldManager
- ❌ GameUI.razor.cs - remove GameWorldManager
- ❌ AreaMap.razor - remove GameWorldManager
- ❌ ConversationChoiceTooltip.razor.cs - remove GameWorldManager
- ❌ GuildInteractionView.razor - remove CommandExecutor
- ❌ NPCActionsView.razor - remove ActionExecutionService
- ❌ TravelSelection.razor.cs - remove ActionExecutionService

### Todo - Service Registration
- ❌ Register ConversationRepository in ServiceConfiguration
- ❌ Initialize ConversationRepository with data from Phase4_Conversations
- ❌ Remove NarrativeManager registration from ServiceConfiguration

## Key Discoveries

### NarrativeManager vs Conversations
- NarrativeManager was being used for tutorial dialogue overrides
- Tutorial conversations are now JSON-based (e.g., tam_tutorial_conversation.json)
- Created Phase4_Conversations to load these during initialization
- ConversationRepository replaces NarrativeManager's dialogue functionality

### Aggressive Deletion Strategy
- User directive: "we dont care about keeping anything playable. we want the target architecture asap"
- Delete legacy code entirely - no compatibility layers
- Fix build errors by either creating minimal types or removing references
- Don't worry about breaking gameplay during migration

### Pattern for UI Updates
Replace GameWorldManager with either:
1. Direct GameWorld access for state queries
2. GameFacade for action execution
3. Specific repositories (LocationRepository, NPCRepository) for data access

## Next Steps
1. Fix remaining 36 build errors
2. Complete intent executor implementations
3. Update GetLocationActions() to generate stable action IDs
4. Run E2E tests and fix any startup issues
5. Test basic gameplay with new intent system