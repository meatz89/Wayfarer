# Wayfarer Architecture Documentation - 2025-01-22

## Critical Architecture Issues and Flows

### Core Problems Identified

1. **NPCs Not Appearing**: NPCs have `spotId` in JSON and are loaded correctly, but `LocationActionManager.GetAvailableActions()` calls `GetNPCsForLocationSpotAndTime()` which filters by BOTH spotId AND current time. Need to verify NPCs are available at the current time block.

2. **Travel Not Working**: When travel button is clicked:
   - `HandleTravelRoute()` calls `GameManager.Travel()`
   - Travel() creates conversation and sets `GameWorld.ConversationPending = true`
   - BUT HandleTravelRoute immediately navigates back to LocationScreen
   - Race condition: polling may not catch the pending conversation before navigation

3. **Morning Summary Not Appearing**: 
   - `ProcessMorningActivities()` is called but may not have events
   - State changes aren't triggering UI updates reliably

## Critical File Map and Responsibilities

### UI Layer (Blazor Components)

#### `/src/Pages/MainGameplayView.razor` + `.razor.cs`
- **PURPOSE**: Main container for all game screens, handles navigation and state polling
- **KEY RESPONSIBILITIES**:
  - Polls game state every 50ms via `PollGameState()`
  - Checks `GameWorld.ConversationPending` and navigates to ConversationScreen
  - Checks for morning activities and shows summary dialog
  - Routes to different screens based on `CurrentScreen` enum
- **CRITICAL METHODS**:
  - `PollGameState()` - Lines 160-197: Checks for pending conversations and morning activities
  - `HandleTravelRoute()` - Lines 256-261: **PROBLEM** - Navigates immediately after Travel()
  - `OnConversationCompleted()` - Lines 327-402: Handles conversation outcomes

#### `/src/Pages/ConversationView.razor` + `.razor.cs`
- **PURPOSE**: Displays streaming narrative text and player choices
- **KEY RESPONSIBILITIES**:
  - Shows narrative from `ConversationManager.State.CurrentNarrative`
  - Gets streaming state from `GameWorldSnapshot` (not ConversationManager directly)
  - Displays choices and handles selection
- **STREAMING**: Uses `GameWorld.StreamingContentState` for text animation

#### `/src/Pages/LocationActions.razor`
- **PURPOSE**: Shows available actions at current location
- **KEY METHOD**: `RefreshActions()` calls `LocationActionManager.GetAvailableActions()`
- **PROBLEM**: No NPCs = no actions displayed

#### `/src/Pages/TravelSelection.razor`
- **PURPOSE**: Shows travel routes and handles route selection
- **KEY**: Travel button calls `OnTravelRoute.InvokeAsync(route)` → `HandleTravelRoute()`

### Game State Layer

#### `/src/GameState/GameWorld.cs`
- **PURPOSE**: Central game state container
- **KEY FIELDS**:
  - `ConversationPending` - Flag checked by polling
  - `PendingConversationManager` - The conversation to display
  - `StreamingContentState` - Shared streaming text state
  - `PendingAction` - Action that triggered conversation

#### `/src/GameState/GameWorldManager.cs`
- **PURPOSE**: Orchestrates all game actions and state changes
- **CRITICAL METHODS**:
  - `Travel()` - Lines 144-202: Creates travel encounter conversation
  - `StartConversation()` - Lines 115-141: Creates NPC conversations
  - `CompleteTravelAfterEncounter()` - Lines 241-247: Completes travel after conversation
- **KEY PATTERN**: Manager methods create conversations and set pending flags

#### `/src/GameState/LocationActionManager.cs`
- **PURPOSE**: Generates available actions based on NPCs and location
- **CRITICAL METHOD**: `GetAvailableActions()` - Line 98:
  ```csharp
  var npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTimeBlock);
  ```
- **PROBLEM**: If no NPCs match spot AND time, no actions appear

### Content Layer

#### `/src/Content/NPCRepository.cs`
- **PURPOSE**: Manages NPC data access from GameWorld
- **KEY METHOD**: `GetNPCsForLocationSpotAndTime()` - Lines 65-69:
  - Filters by `n.SpotId == locationSpotId`
  - AND filters by `n.IsAvailable(currentTime)`
- **DATA SOURCE**: `_gameWorld.WorldState.GetCharacters()`

#### `/src/Content/NPCFactory.cs`
- **PURPOSE**: Creates NPC instances from data
- **KEY**: `CreateNPCFromIds()` accepts `spotId` parameter (no default value)

#### `/src/Content/GameWorldInitializer.cs`
- **PURPOSE**: Loads all game data from JSON files
- **NPC LOADING**: `LoadNPCs()` - Lines 845-937:
  - Reads `/src/Content/Templates/npcs.json`
  - Passes `dto.SpotId` to factory (line 913)
  - NPCs have spotId like "workshop", "market", "tavern"

### Conversation System

#### `/src/Game/ConversationSystem/ConversationManager.cs`
- **PURPOSE**: Manages conversation flow and choices
- **STATE**: Tracks narrative, choices, and completion
- **PATTERN**: Narrative providers generate content

#### `/src/Game/ConversationSystem/DeterministicNarrativeProvider.cs`
- **PURPOSE**: Generates non-AI narrative for actions
- **STREAMING**: Uses `DeterministicStreamingService` to animate text
- **KEY**: Streams text through `StreamingContentStateWatcher`

#### `/src/Game/ConversationSystem/ConversationFactory.cs`
- **PURPOSE**: Creates appropriate conversation manager based on context
- **PATTERN**: Factory determines which narrative provider to use

### Streaming System

#### `/src/UIHelpers/StreamingContentState.cs`
- **PURPOSE**: Shared state for streaming text
- **USAGE**: Both AI and deterministic providers update this
- **CONSUMED BY**: ConversationView reads this for display

#### `/src/Game/ConversationSystem/DeterministicStreamingService.cs`
- **PURPOSE**: Simulates typing effect for deterministic text
- **CONFIG**: 50ms per word default delay

## Critical Architectural Patterns

### 1. Repository Pattern
- All data access goes through repositories
- Repositories read from `GameWorld.WorldState`
- Example: `NPCRepository` wraps `_gameWorld.WorldState.GetCharacters()`

### 2. No Optional Parameters
- All method parameters must be explicit
- Example: `CreateNPCFromIds(..., string spotId)` - NOT `string spotId = null`

### 3. Clean Architecture (DI)
- Use interfaces for behavior variations
- Example: `INarrativeProvider` implemented by AI and Deterministic providers
- ServiceConfiguration.cs controls which implementation is used

### 4. Polling-Based UI Updates
- MainGameplayView polls every 50ms
- Checks flags like `ConversationPending`
- Updates UI based on state changes

### 5. Manager Pattern
- Managers orchestrate complex operations
- Example: `GameWorldManager.Travel()` coordinates multiple systems

## Critical Flows

### Travel Flow (BROKEN)
1. User clicks travel button in TravelSelection.razor
2. `OnTravelRoute.InvokeAsync(route)` called
3. MainGameplayView.`HandleTravelRoute()` called
4. `GameManager.Travel(route)` called
5. Travel() creates conversation, sets `ConversationPending = true`
6. **PROBLEM**: HandleTravelRoute immediately navigates to LocationScreen
7. Polling may miss the pending conversation

### NPC Action Flow (BROKEN)
1. LocationActions.razor calls `LocationActionManager.GetAvailableActions()`
2. Gets current spot: `player.CurrentLocationSpot`
3. Gets NPCs: `GetNPCsForLocationSpotAndTime(spot.SpotID, currentTimeBlock)`
4. **PROBLEM**: No NPCs found if they're not available at current time
5. No NPCs = no actions displayed

### Morning Activities Flow
1. GameWorldManager.`StartNewDay()` called
2. Sets `_lastMorningResult` with morning events
3. MainGameplayView polling checks `HasPendingMorningActivities()`
4. Shows `MorningSummaryDialog` component
5. **ISSUE**: May not have events or state not updating

## Key Configuration Files

### `/src/ServiceConfiguration.cs`
- Registers all services for DI
- Controls which `INarrativeProvider` implementation is used
- Line 89: `services.AddSingleton<INarrativeProvider, DeterministicNarrativeProvider>();`

### `/src/Content/Templates/npcs.json`
- Defines all NPCs with:
  - `spotId`: "workshop", "market", "tavern", etc.
  - `locationId`: "millbrook", "thornwood", etc.
  - `availabilitySchedule`: When NPC is available

## Debugging Strategy

1. **Add logging to track flow**:
   - Log when Travel() is called
   - Log when ConversationPending is set
   - Log polling checks
   - Log navigation changes

2. **Verify NPC availability**:
   - Check current TimeBlock
   - Check NPC schedules
   - Log filtering in GetNPCsForLocationSpotAndTime

3. **Fix race conditions**:
   - Don't navigate immediately after async operations
   - Let polling handle navigation based on state

4. **Add state verification**:
   - Before actions, verify preconditions
   - After actions, verify expected state changes

## Next Session Quick Reference

**To fix NPCs not appearing**:
1. Check TimeManager.GetCurrentTimeBlock()
2. Verify NPC.IsAvailable(currentTime) returns true
3. Add logging to NPCRepository.GetNPCsForLocationSpotAndTime()

**To fix travel not working**:
1. Remove immediate navigation in HandleTravelRoute()
2. Let polling detect ConversationPending and navigate
3. Add logging to verify conversation creation

**To fix morning summary**:
1. Log when StartNewDay() is called
2. Verify _lastMorningResult has events
3. Check polling detects HasPendingMorningActivities()

**Key principle**: The UI should react to state changes via polling, not navigate immediately after actions.