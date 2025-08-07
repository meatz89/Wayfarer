# Wayfarer Session Handoff - UI Mockup Implementation
## Session Date: 2025-01-28

# 🎯 OBJECTIVE: Show EXACT UI Mockups with Systemic Generation

## ❌ CURRENT STATUS: NOT WORKING
The game fails to start with `GameWorldInitializationException: 'Critical phase 2 (Location-Dependent Entities) failed'`

## 🔧 WORK COMPLETED THIS SESSION

### 1. Created Minimal JSON Content for Mockups
- **npcs.json**: Elena, Marcus, Bertram, Lord Aldwin, Viktor, Garrett, Lord Blackwood
- **locations.json**: Market Square, Noble District, Merchant Row, Riverside  
- **location_spots.json**: Copper Kettle, Central Fountain, merchant stalls, etc (removed legacy "type" field)
- **letter_templates.json**: 5 letter templates matching mockup scenario
- **gameWorld.json**: Start at Copper Kettle at 3:45 PM (15.75 hours)
- **standing_obligations.json**: Cleared to empty array

### 2. Fixed LocationSpotFactory
- Removed LocationSpotTypes type parameter from all methods
- `CreateLocationSpot()` no longer requires type
- `CreateLocationSpotFromIds()` no longer requires type
- `CreateMinimalSpot()` no longer sets Type property
- Phase2_LocationDependents updated to use fixed factory

### 3. Created Phase8_InitialLetters
- Adds 5 letters directly to Player.LetterQueue array:
  1. Elena's marriage refusal (pos 1, REPUTATION stakes, 2 day deadline)
  2. Lord Blackwood's urgent letter (pos 2, REPUTATION, 2 days)
  3. Marcus's trade deal (pos 3, WEALTH, 3 days)
  4. Viktor's security report (pos 5, SAFETY, 6 days)
  5. Garrett's mysterious package (pos 6, SECRET, 12 days)

### 4. Updated GameUI to Show Letter Queue
- Changed default view from LocationScreen to LetterQueueScreen
- Removed GetDefaultView() logic that checked for Dawn/tutorial
- GameUI.razor.cs now starts with `CurrentViews.LetterQueueScreen`

## ⚠️ REMAINING ISSUES

### 1. Build/Startup Errors
- Phase 2 initialization failing (even after removing type field)
- Need to verify all JSON validators work with new structure
- LocationSpotValidator may still check for "type" field

### 2. Missing UI Implementation
The following screens need to match EXACT mockups:
- **LetterQueueScreen**: Show 8 slots with letters, deadlines, peripheral awareness
- **ConversationScreen**: Attention dots, body language, systemic choices
- **LocationScreen**: NPCs with emotional states, action cards

### 3. Systemic Generation Not Connected
- VerbContextualizer exists but not wired to UI
- NPCEmotionalStateCalculator not calculating from letters
- Choices still using templates, not queue state

## 📋 WHAT NEEDS TO BE DONE

### STEP 1: Fix Initialization Error
```bash
# Debug Phase 2 error
dotnet run 2>&1 | grep -A 20 "PHASE 2"
```
- Check if LocationSpotValidator still requires "type" field
- Verify all JSON files are valid
- Fix any remaining type references

### STEP 2: Create/Update UI Components

#### LetterQueueScreen.razor
Must show:
- 8 queue slots with visual letters
- Deadline warnings ("⚡ Lord B: 2h 15m")
- NPCs at current location
- Click NPC → start conversation

#### ConversationScreen.razor  
Must show:
- Attention bar (3 dots, show spent/available)
- Character name and body language
- Dialogue from letter stakes
- 5 choices with mechanical effects
- Bottom status bar

### STEP 3: Wire Systemic Generation
```csharp
// In GameFacade.StartConversation()
var choices = _verbContextualizer.GenerateChoicesFromQueueState(
    npc, 
    _attentionManager, 
    _emotionalStateCalculator
);
```

### STEP 4: Test Full Flow
1. Game starts → LetterQueueScreen shows
2. See 5 letters in queue
3. Click Elena → ConversationScreen opens
4. Elena shows DESPERATE state
5. Choices generated from queue state
6. Mechanical effects visible

## 🚨 CRITICAL NOTES

### User Frustration Points
1. **"FUCKING USE SPOTFACTORY"** - Must use factory, not create spots directly
2. **"remove the fucking field as it is legacy"** - Type field completely removed
3. **"i hate dictionaries"** - All metadata replaced with strongly typed classes
4. **"when i start the fucking game in browser, i want to see THE EXACT UI PAGES FROM OUR MOCK-UPS!"**

### Architecture Rules (from CLAUDE.md)
- GameWorld has NO dependencies
- Everything through DI (no `new()` in constructors)
- No dictionaries, use strongly typed objects
- Delete legacy code completely
- ALWAYS test before claiming complete

## 🎯 SUCCESS CRITERIA

When running `dotnet run` at http://localhost:5089:

✅ Game starts without errors
✅ Shows LetterQueueScreen with 5 letters
✅ Player at Copper Kettle Tavern
✅ Time shows 3:45 PM (TUE)
✅ Elena present and clickable
✅ Conversation shows DESPERATE state
✅ Choices generated from queue, not templates
✅ Attention economy working (3 dots)
✅ Mechanical effects visible on choices

## 🔴 CURRENT BLOCKER
Phase 2 initialization error prevents game from starting. Must fix this first before any UI work can be tested.

## 📝 Next Session Priority
1. Fix Phase 2 initialization error
2. Verify game starts
3. Update UI components to match mockups
4. Test full flow end-to-end
5. Ensure systemic generation working

The backend systems are built. The JSON content exists. The initialization is failing. Fix that first, then connect the UI.