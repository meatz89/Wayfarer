# Wayfarer Session Handoff - Literary UI Implementation
## Session Date: 2025-01-06 (Last Updated: 2025-01-28)

# CURRENT STATE: UI Mockups NOT Showing with Systemic Generation

## 🚨 CRITICAL ISSUES IDENTIFIED (2025-01-28)

### The Problem
User wants to see the EXACT UI mockups from `/UI-MOCKUPS/` folder when the game starts, with ALL content dynamically generated from game systems. Currently:
- Game starts with old UI pages (CharacterCreation, MissingReferences)
- No initial letters in queue
- Conversations don't show systemic choices
- NPCs and locations aren't properly initialized

### What the Mockups Show

#### 1. **Conversation Screen (Elena at Copper Kettle)**
From `/UI-MOCKUPS/conversation-elena.html`:
- Attention bar with 3 dots (2 available, 1 spent)
- Peripheral awareness: "⚡ Lord B: 2h 15m"
- Elena DESPERATE: "leaning forward, fingers worrying her shawl"
- Dialogue: "The letter contains Lord Aldwin's marriage proposal. My refusal..."
- 5 systemic choices with mechanical effects:
  - Free: "I understand. Your letter is second in my queue."
  - 1 Att: "I'll prioritize your letter..." (⚠ Burns 1 Status)
  - 1 Att: "Tell me about the situation..." (ℹ Gain rumor)
  - 2 Att: "I swear I'll deliver..." (⛓ Creates obligation)
  - 3 Att: [Locked] "Let me investigate..."

#### 2. **Location Screen**
From `/UI-MOCKUPS/location-screens.html`:
- Time bar: "Morning (9:00 AM)" with "Next deadline: 2h 15m"
- NPCs present with emotional states
- Action cards with time/cost indicators
- Atmosphere tags

## 🔧 WORK COMPLETED THIS SESSION (2025-01-28)

### 1. Fixed Circular Dependencies
- **Problem**: NPCEmotionalStateCalculator → LetterQueueManager → ConversationFactory → DeterministicNarrativeProvider → VerbContextualizer → NPCEmotionalStateCalculator
- **Solution**: 
  - Removed ConversationFactory dependency from LetterQueueManager
  - Converted async methods to sync (PrepareSkipAction, PreparePurgeAction)
  - VerbContextualizer now receives NPCEmotionalStateCalculator as parameter

### 2. Refactored Type-Unsafe Metadata Dictionary
- **Problem**: GameWorld used Dictionary<string, object> for metadata (type-unsafe)
- **Solution**: Created strongly typed `PendingQueueState` class:
  ```csharp
  public class PendingQueueState {
      public QueueActionType? PendingAction { get; set; }
      public int? PendingSkipPosition { get; set; }
      public PurgeTokenSelection PendingPurgeTokens { get; set; }
      public HashSet<string> NPCsWithSecrets { get; set; }
  }
  ```
- Created `PurgeTokenSelection` with strongly typed properties (no dictionaries!)
- Updated all references to use typed properties

### 3. Fixed Build Issues
- Created `/src/Models/TokenChange.cs` to fix compilation errors
- Fixed port conflict (changed from 5011 to 5089)
- Game now builds and starts successfully

### 4. Literary UI System Implementation
The complete conversation-as-mechanics system is implemented:

#### VerbContextualizer.cs
- Generates ALL choices from queue state, tokens, NPCs, obligations
- 4 hidden verbs (PLACATE, EXTRACT, DEFLECT, COMMIT)
- Contextual filtering ensures max 5 choices
- Attention economy (0/1 points) gates discovery

#### ConversationEffects.cs (15+ effect classes)
- LetterReorderEffect, GainTokensEffect, BurnTokensEffect
- AcceptLetterEffect, ExtendDeadlineEffect, ShareInformationEffect
- CreateObligationEffect, UnlockRoutesEffect, UnlockNPCEffect
- UnlockLocationEffect, DiscoverRouteEffect

#### NPCConversationExtensions.cs
- Extension methods for dynamic content generation
- HasLetterToSend(), GenerateLetter(), GetContact()
- GetSecretRoute(), HasObligationTo()

## ❌ WHAT'S STILL NOT WORKING

### 1. Game Doesn't Start with Mockup UI
- **Current**: Starts with GameUI.razor showing CharacterCreation/MissingReferences
- **Needed**: Should start with LetterQueueScreen showing 5 letters

### 2. No Initial Game State
- **Current**: Empty queue, no NPCs initialized
- **Needed**: 
  - 5 letters (Elena/Marcus/Viktor/Aldwin/Garrett)
  - NPCs at Copper Kettle
  - Time at 9 AM
  - 3 attention points

### 3. Conversations Not Using Systemic Generation
- **Current**: DeterministicNarrativeProvider calls VerbContextualizer but choices may not display
- **Needed**: Choices should show in UI with mechanical effects

## 📋 IMPLEMENTATION PLAN TO FIX

### Step 1: Initialize Game with Proper State
Create or update GameWorldInitializer to:
```csharp
// Add 5 initial letters
queue.Add(new Letter {
    SenderId = "elena", SenderName = "Elena",
    TokenType = ConnectionType.Trust,
    Stakes = StakeType.REPUTATION,
    DeadlineInDays = 0.1f, // 2 hours
    Size = SizeCategory.Medium
});
// Add Marcus, Viktor, Aldwin, Garrett letters...

// Initialize NPCs
world.NPCs.Add(new NPC { 
    ID = "elena", Name = "Elena",
    Location = "copper_kettle"
});
// Add Marcus, Viktor...

// Set starting location
player.CurrentLocationSpot = copperKettleSpot;
player.CurrentTime = TimeBlocks.Morning;
```

### Step 2: Change Default UI
Update GameUI.razor:
```csharp
@if (CurrentView == CurrentViews.LetterQueue) // DEFAULT
{
    <LetterQueueScreen />
}
```

### Step 3: Create/Update LetterQueueScreen
Match the mockup exactly:
- Show 8 queue slots with letters
- Display peripheral awareness (deadlines)
- Show NPCs at current location
- Action buttons for each NPC

### Step 4: Fix ConversationScreen
Ensure it:
- Shows attention bar (3 dots)
- Displays choices from VerbContextualizer
- Shows mechanical effects for each choice
- Matches mockup styling

### Step 5: Wire Everything Together
- GameFacade.StartConversation() should use VerbContextualizer
- Choices should display with proper mechanical effects
- Navigation should flow: Queue → Conversation → Queue

## 🎯 SUCCESS CRITERIA

When starting the game at http://localhost:5089, user should see:
1. **Queue with 5 letters** (Elena, Marcus, Viktor, Aldwin, Garrett)
2. **Copper Kettle location** with NPCs present
3. **Click Elena** → See DESPERATE state conversation
4. **Choices generated from queue state**, not templates
5. **Mechanical effects visible** on each choice
6. **Attention economy working** (3 dots, costs shown)

## 🚀 QUICK TEST COMMANDS

```bash
# Build and run
cd /mnt/c/git/wayfarer/src
dotnet build
dotnet run

# Game should start at http://localhost:5089
# Should see queue with letters, not character creation
```

## 📊 Architecture Reminders

### CRITICAL Rules
1. **NO dictionaries** - User hates them, use strongly typed objects
2. **NO `new()` in constructors** - Everything through DI
3. **GameWorld has NO dependencies** - It's the root
4. **UI uses GameFacade only** - Never inject services directly
5. **Delete legacy code** - No compatibility layers

### Key Classes
- `VerbContextualizer` - Generates choices from game state
- `NPCEmotionalStateCalculator` - NPC states from letters
- `PendingQueueState` - Strongly typed queue actions
- `ConversationEffects` - All mechanical effect classes

## 📝 Next Session Priority

**MANDATORY: Make the game show the EXACT UI mockups with systemic generation**
1. Initialize game with 5 letters and NPCs
2. Show LetterQueueScreen by default
3. Generate choices from VerbContextualizer
4. Display mechanical effects
5. Test that everything is dynamic, not static

The backend systems are complete. The UI components exist. They just need to be connected and shown by default with proper initial state.

## Key Files Modified This Session
- `/src/GameState/PendingQueueState.cs` - Created (strongly typed)
- `/src/GameState/GameWorld.cs` - Removed metadata dictionary
- `/src/GameState/LetterQueueManager.cs` - Removed ConversationFactory dependency
- `/src/Services/GameFacade.cs` - Updated for typed state
- `/src/Game/ConversationSystem/VerbContextualizer.cs` - Fixed circular dependency
- `/src/Game/ConversationSystem/DeterministicNarrativeProvider.cs` - Pass stateCalculator
- `/src/Properties/launchSettings.json` - Changed port to 5089
- `/src/Models/TokenChange.cs` - Created for compilation fix

## Previous Session Work (Still Valid)
- Complete Literary UI backend implementation
- VerbContextualizer generates choices from queue state
- 15+ mechanical effect classes
- NPCEmotionalStateCalculator working
- Attention economy implemented

The systems are built. The UI exists. We just need to SHOW IT with proper initialization!