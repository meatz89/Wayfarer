# Conversation and Action Flow Architecture

## Current Issues
1. **NPC Visibility**: Both Old Woman and Tam appear when only Tam should be visible in tutorial step 2
2. **Action Organization**: Actions grouped by type (Social, Economic) instead of by NPC
3. **Broken Click Handler**: Clicking "Talk with Tam" does nothing
4. **No Conversation Screen**: Conversation screen doesn't open when action is clicked

## Component Architecture

### 1. UI Components
- **GameUI.razor**: Root game component that manages screen transitions
- **MainGameplayView.razor**: Location screen showing NPCs and actions
- **ConversationView.razor**: Conversation screen for dialog interactions
- **LocationSpotMap.razor**: Displays NPCs at current location
- **LocationActions.razor**: Displays available actions

### 2. State Management
- **GameFacade**: Central service managing game state transitions
- **ConversationStateManager**: Manages conversation state
- **LocationActionsUIService**: Provides actions for UI display
- **CommandDiscoveryService**: Discovers available commands
- **PlayerManager**: Tracks player state and progress

### 3. Action/Conversation Flow

#### Current (Broken) Flow:
1. LocationSpotMap shows NPCs with "No actions available"
2. LocationActions shows actions grouped by type
3. Clicking action -> Nothing happens

#### Expected Flow:
1. LocationSpotMap shows NPCs with their actions nested underneath
2. Player clicks "Talk with Tam"
3. GameFacade.SetPendingConversation() called
4. CurrentScreen changes to ConversationScreen
5. ConversationView displays tutorial narrative
6. Player selects dialog option
7. Conversation ends, returns to MainGameplayView
8. Tutorial progresses to next step

## Key Problems to Fix

### Problem 1: NPC Visibility
- Tutorial manager should filter NPCs based on current step
- Only Tam should be visible in step 2

### Problem 2: Action Organization
- Actions need to be associated with NPCs
- UI should show: NPC Name -> Available Actions

### Problem 3: Click Handler
- ConverseCommand not properly wired to UI
- Need to ensure GameFacade.SetPendingConversation() is called

### Problem 4: Screen Transition
- GameUI needs to detect pending conversation
- Automatically switch to ConversationScreen when conversation starts

## Files to Investigate
1. `/src/Services/LocationActionsUIService.cs` - How actions are provided
2. `/src/GameState/Commands/ConverseCommand.cs` - Conversation command
3. `/src/Pages/MainGameplayView.razor` - Location screen UI
4. `/src/Pages/ConversationView.razor` - Conversation screen
5. `/src/Services/GameFacade.cs` - State management
6. `/src/Pages/GameUI.razor` - Screen transitions