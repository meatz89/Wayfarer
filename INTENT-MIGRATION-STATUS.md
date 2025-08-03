# Intent-Based Architecture Migration Status

## Overview
This document captures the current state of migrating from the old CommandDiscoveryService to a clean intent-based architecture where commands only capture player intent and backend services execute using GameWorld as the single source of truth.

## Migration Context

### Original Problem
- "Move Here" button in tutorial was broken due to command ID mismatch
- UI expected: `move_lower_ward_square`
- Backend generated: `TravelToSpotCommand_12345-guid`
- Root cause: CommandDiscoveryService pre-created all commands with 30+ dependencies

### Architectural Issues Addressed
1. **CommandDiscoveryService was backwards** - pre-validated all possible commands
2. **GameWorldManager was legacy** - had 20+ service dependencies 
3. **Commands weren't pure intent** - contained logic and fetched context
4. **Overly complex chains** - UI → GameFacade → CommandDiscovery → Command → GameWorld

## Current Implementation Status

### ✅ COMPLETED

#### 1. Intent System Created
- **File**: `/src/GameState/Intents/PlayerIntent.cs`
- **Intents**: MoveIntent, TalkIntent, RestIntent, ObserveLocationIntent, TravelIntent, etc.
- **Pattern**: Simple data objects with only player intent, no logic

#### 2. GameFacade Updated
- **File**: `/src/Services/GameFacade.cs`
- **Key Methods**:
  - `ExecuteIntent(PlayerIntent intent)` - pattern matching dispatcher
  - `ExecuteMove()`, `ExecuteTalk()`, `ExecuteRest()`, `ExecuteObserve()`, `ExecuteTravel()` - implemented
  - `GetLocationActions()` - generates actions directly without discovery
  - `ExecuteLocationActionAsync()` - converts action IDs to intents

#### 3. Legacy Code Removed
- **Deleted Files**:
  - `/src/GameState/Commands/` - entire directory
  - `/src/GameState/GameWorldManager.cs`
  - `/src/GameState/Commands/CommandExecutor.cs`
  - `/src/GameState/Commands/CommandDiscoveryService.cs`
  - `/src/GameState/NarrativeManager.cs`
  - `/src/GameState/GameStateManager.cs`
  - `/src/Services/ActionExecutionService.cs`
  - `/src/GameState/PendingCommand.cs`
  - `/src/GameState/NarrativeRequirement.cs`

#### 4. UI Updated
- **MainGameplayView.HandleSpotSelection()** - uses MoveIntent
- **LocationActions.razor** - uses ExecuteLocationActionAsync with action IDs

### ❌ CURRENT ISSUES

#### Build Errors
The project doesn't build due to:
1. UI components reference deleted GameWorldManager
2. Components expect old types (IGameCommand, CommandResult, CommandExecutor)
3. MarketItem type is missing (was only in ViewModels)

#### Specific Files With Errors
- `/src/Pages/LocationSpotMap.razor.cs` - references GameWorldManager
- `/src/Pages/PlayerStatusView.razor.cs` - references GameWorldManager
- `/src/Pages/GameUI.razor.cs` - references GameWorldManager
- `/src/Pages/AreaMap.razor` - references GameWorldManager
- `/src/Pages/ConversationChoiceTooltip.razor.cs` - references GameWorldManager
- `/src/Game/ConversationSystem/DeterministicNarrativeProvider.cs` - uses IGameCommand
- `/src/Pages/Components/GuildInteractionView.razor` - references CommandExecutor

## Next Steps for Continuation

### 1. Fix UI Component References
Replace GameWorldManager references with GameFacade calls:
```csharp
// OLD: @inject GameWorldManager GameWorldManager
// NEW: @inject GameFacade GameFacade

// OLD: GameWorldManager.CanMoveToSpot(spot.SpotID)
// NEW: !spot.IsClosed && player.CurrentLocationSpot?.LocationId == spot.LocationId
```

### 2. Remove Command Type References
For files expecting IGameCommand, CommandResult, etc:
- Either delete the file if it's part of the old system
- Or rewrite to use the intent system directly

### 3. Add Missing Types
If types like MarketItem are genuinely needed:
- Check if they exist in ViewModels
- Or create minimal versions in appropriate locations

### 4. Complete Intent Implementations
In GameFacade, implement remaining intent executors:
- ExecuteDeliverLetter()
- ExecuteCollectLetter() 
- ExecuteExplore()
- ExecutePatronFunds()
- ExecuteAcceptOffer()
- ExecuteDiscoverRoute()
- ExecuteConvertEndorsements()

## Key Design Principles

### From CLAUDE.md
- **NO SILENT BACKEND ACTIONS** - Player must initiate everything
- **GameWorld as Single Source of Truth** - No duplicate state
- **No Special Rules** - Use categorical mechanics
- **Delete Legacy Code Entirely** - No compatibility layers
- **Never use Task.CompletedTask** - Make methods sync if no async work

### Intent Architecture Rules
1. Intents are pure data - no logic, no dependencies
2. GameFacade executes intents by fetching context from GameWorld
3. UI creates intents directly - no discovery needed
4. Stable action IDs like `talk_{npcId}`, `rest_{hours}`, `move_{spotId}`

## Important Context

### User Directive
"it doesnt matter if it is 'critical'. we dont care about keeping anything playable. we want the target architecture asap"

This means:
- Aggressively delete legacy code
- Don't create compatibility layers
- Don't worry about breaking gameplay
- Focus on clean architecture

### Documentation Created
- `/INTENT-BASED-ARCHITECTURE.md` - Full design documentation
- `/MOVEMENT-SYSTEM-ANALYSIS.md` - Original problem analysis

### Key Code Locations
- Intent definitions: `/src/GameState/Intents/PlayerIntent.cs`
- Intent execution: `/src/Services/GameFacade.cs` (ExecuteIntent method)
- Action generation: `/src/Services/GameFacade.cs` (GetLocationActions method)

## Current Working Directory
`/mnt/c/git/wayfarer`

## Git Status
- Branch: `letters-ledgers`
- Modified files include GameFacade.cs, ServiceConfiguration.cs, MainGameplayView.razor.cs
- Many deleted files from removing command system

## Build Command
```bash
cd /mnt/c/git/wayfarer/src && dotnet build
```

Currently fails with ~39 errors related to missing types from deleted command system.