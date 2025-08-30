# 🎯 WAYFARER: Full Mockup Implementation Plan

## Overview
This document tracks the implementation of UI mockups into the actual game, ensuring 1:1 matching between mockups and implementation through JSON content and game mechanics.

## Critical Issues to Fix

### 🔴 PRIORITY 1: Core Mechanics (Blocking Gameplay)

#### 1.1 LISTEN Card Draw Counts
**Current**: Drawing 3-4 cards per LISTEN
**Target**: Draw 1-2 cards per LISTEN
**File**: `src/Game/ConversationSystem/Core/EmotionalState.cs`
**Status**: ⏳ IN PROGRESS

#### 1.2 Goal Card Mechanics  
**Current**: Goal card appears immediately in hand
**Target**: 
- Goal card shuffled from goal deck into conversation deck
- Only drawable when emotional state matches validStates
- Player must navigate from DESPERATE to better state using comfort cards
- Once drawable, goal card MUST be drawn on next LISTEN (priority draw)
- 3-turn urgency rule activates once drawn
**Specific for Elena**:
- Elena starts in DESPERATE state
- Her urgent letter goal should have validStates: ["TENSE", "OPEN"] (NOT DESPERATE!)
- Player MUST use comfort cards to navigate from DESPERATE → TENSE
- Only THEN the goal card becomes drawable
- This creates the 3-turn navigation gameplay shown in mockup
**Files**: 
- `src/Game/ConversationSystem/Models/ConversationSession.cs`
- `src/Game/ConversationSystem/Managers/ConversationManager.cs`
- `src/Content/Templates/npc_goal_decks.json`
**Status**: ⏳ IN PROGRESS

### 🟡 PRIORITY 2: Location Screen

#### 2.1 Spot Names
**Current**: "Central Fountain", "Bar Counter"
**Target**: "The Fountain", "The Bar"
**File**: `src/Content/Templates/location_spots.json`
**Status**: ⏳ PENDING

#### 2.2 NPC Display at Spots
**Current**: NPCs not showing at spots with states
**Target**: Show "Marcus (Calculating)" with queue markers
**File**: `src/Pages/Components/LocationContent.razor`
**Status**: ⏳ PENDING

#### 2.3 Location Actions
**Current**: Missing Travel and Work actions
**Target**: Show action cards for Travel (at Crossroads) and Work
**Files**:
- `src/Pages/Components/LocationContent.razor`
- `src/Services/GameFacade.cs`
**Status**: ⏳ PENDING

### 🟢 PRIORITY 3: Conversation Flow

#### 3.1 Observation Cards
**Current**: NPCs generating observation cards
**Target**: Player builds observation deck through location actions
**Files**:
- `src/Content/Templates/player_observation_cards.json`
- `src/GameState/ObservationSystem.cs`
**Status**: ⏳ PENDING

#### 3.2 Elena's Desperate State
**Current**: Not properly desperate
**Target**: Show desperate state, urgency, goal card after turn 3
**Files**:
- `src/Content/Templates/npcs.json`
- `src/Content/Templates/npc_goal_decks.json`
**Status**: ⏳ PENDING

### 🔵 PRIORITY 4: Exchange System

#### 4.1 Exchange Card Display
**Current**: Not showing as cards
**Target**: Display as selectable exchange cards with cost→reward
**File**: Create `src/Pages/Components/ExchangeContent.razor`
**Status**: ⏳ PENDING

## Implementation Phases

### Phase 1: Fix Core Mechanics ✅
- [✅] Fix LISTEN card draw counts (1-2 cards) - ALREADY FIXED in EmotionalState.cs
- [✅] Fix goal card shuffle mechanics - COMPLETED
  - Fixed validStates: ["TENSE", "OPEN"] (not DESPERATE)
  - Added DrawableStates property to goal cards
  - Goal cards now prioritized when drawable
- [✅] Fix initial conversation draw - goal card no longer appears early
- [✅] Implement 3-turn urgency rule - already exists, now activates properly

### Phase 2: Fix JSON Content ✅
- [✅] Update location spot names - COMPLETED
  - "Central Fountain" → "The Fountain"
  - "Bar Counter" → "The Bar"
- [✅] Add merchant route observation - COMPLETED
  - Added to player_observation_cards.json
  - Added to observations.json at market_square/central_fountain
  - 85% success rate, DESPERATE→OPEN transition
- [✅] Fix Elena's initial state - COMPLETED
  - NPCs now use CurrentState from JSON
  - Elena has currentState: "DESPERATE" in npcs.json
  - DetermineInitialState now simply returns npc.CurrentState
- [✅] Add Marcus exchange cards - COMPLETED
  - Buy Provisions (3 coins → Hunger=0)
  - Buy Access Permit (15 coins → Noble District Permit)
  - Accept Quick Delivery (New obligation → 8 coins)
  - Buy Health Potion (8 coins → +2 health)

### Phase 3: Fix UI Components
- [ ] Update LocationContent.razor
- [ ] Fix ConversationContent.razor
- [ ] Create ExchangeContent.razor
- [ ] Add "You are here" indicator

### Phase 4: Implement Missing Features
- [ ] Work action (2 attention → 8 coins)
- [ ] Travel from Crossroads
- [ ] Observation deck building
- [ ] Queue position markers

### Phase 5: Testing & Validation
- [ ] Test Elena desperate letter scenario
- [ ] Test Marcus exchange
- [ ] Screenshot and compare with mockups
- [ ] Verify all mechanics working

## Mockup vs Implementation Checklist

### Location Screen (location-screens.html)
- [ ] Resources bar shows: Coins, Health, Hunger, Attention
- [ ] Time display: "Tuesday, 2:47 PM"
- [ ] Active Obligations queue with position markers
- [ ] Location path: "Lower Wards → Market District → Central Square"
- [ ] Current spot banner: "The Fountain"
- [ ] Atmosphere text in italic box
- [ ] Actions Available Here section
- [ ] People at This Spot with tokens
- [ ] Observations with attention costs
- [ ] Other Spots grid with "You are here"

### Conversation Screen (conversation-screen.html)
- [ ] Turn counter: "Turn 3/10"
- [ ] Patience: "8/10"
- [ ] Comfort dots visualization
- [ ] Cards show success percentages
- [ ] Goal card has "Goal Card" marker
- [ ] Observation card shows "24hr" expiry
- [ ] Weight limits per emotional state

### Exchange Screen (exchange-conversation.html)
- [ ] Exchange mode banner
- [ ] NPC tokens with bonuses
- [ ] Exchange cards with visual flow
- [ ] Cost → Reward display
- [ ] Success percentages
- [ ] Accept/Decline buttons

## Files to Modify

### JSON Content Files
1. `/src/Content/Templates/location_spots.json` - Fix spot names
2. `/src/Content/Templates/npcs.json` - Elena desperate state
3. `/src/Content/Templates/npc_goal_decks.json` - Goal card config
4. `/src/Content/Templates/npc_exchange_decks.json` - Marcus exchanges
5. `/src/Content/Templates/player_observation_cards.json` - Merchant route

### Code Files
1. `/src/Game/ConversationSystem/Core/EmotionalState.cs` - Card draw counts
2. `/src/Game/ConversationSystem/Models/ConversationSession.cs` - Goal mechanics
3. `/src/Pages/Components/LocationContent.razor` - Location display
4. `/src/Pages/Components/ConversationContent.razor` - Conversation UI
5. `/src/Services/GameFacade.cs` - Work/Travel actions

## Success Metrics

### Must Have (Game Breaking)
✅ LISTEN draws 1-2 cards only
✅ Goal cards require state navigation
✅ Elena starts desperate
✅ Observations from player actions only

### Should Have (Mockup Match)
✅ Exact spot names from mockup
✅ NPCs show at spots with states
✅ Work and Travel actions visible
✅ Exchange cards display properly

### Nice to Have (Polish)
✅ All text matches mockups exactly
✅ Visual styling matches
✅ Animations and transitions

## Testing Scenarios

### Scenario 1: Elena's Desperate Letter
1. Start new game
2. Navigate to Market Square
3. See "Elena (Desperate)" at Copper Kettle
4. Move to Corner Table
5. Start Letter Conversation (1 attention)
6. Turn 1: Draw 1-2 desperate cards
7. Turn 2: Play state change card
8. Turn 3: Goal card appears, play it
9. Letter added to queue position 1

### Scenario 2: Marcus Exchange
1. At Market Square
2. Move to Merchant Row
3. See Marcus with queue marker
4. Start Quick Exchange (0 attention)
5. See 3 exchange cards
6. "Buy Provisions" highlighted
7. Accept → Hunger set to 0

## Current Status: Phase 1 - Fixing Core Mechanics

Next Steps:
1. Fix LISTEN card draw counts ⏳
2. Test the changes
3. Move to goal card mechanics