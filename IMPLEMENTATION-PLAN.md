# Wayfarer Core System Overhaul - Implementation Plan

## Overview
Complete replacement of conversation mechanics to match the refined design specification. NO compatibility layers, NO legacy code, ALL content from JSON.

## Current State vs Target State

### 1. COMFORT SYSTEM
**CURRENT (WRONG)**:
- Range: 0-20, starts at 5
- Used for depth gating
- Fixed comfort gains
- Momentum system (-3 to +3) for state degradation

**TARGET (CORRECT)**:
- Range: -3 to +3, starts at 0
- Battery for state transitions
- Weight-based: Success +weight, Failure -weight
- At ±3: State transition, reset to 0
- NO momentum system

### 2. CARD DEPTH
**CURRENT**: Depth property exists, cards filtered by comfort vs depth
**TARGET**: NO depth at all, cards filtered by emotional state

### 3. WEIGHT SYSTEM  
**CURRENT**: Weight limits correct, but comfort changes not weight-based
**TARGET**: Weight ONLY determines playability, also drives comfort changes

### 4. CARD FILTERING
**CURRENT**: Cards filtered by depth threshold
**TARGET**: Cards explicitly list drawable states, LISTEN only draws matching

### 5. STATE TRANSITIONS
**CURRENT**: Momentum at -3 degrades state
**TARGET**: Comfort at ±3 triggers specific transitions per state

### 6. PATIENCE CARDS
**CURRENT**: Don't exist
**TARGET**: New card type that adds patience

### 7. GOAL CARDS
**CURRENT**: Only Promise/Resolution/Delivery conversations get goals
**TARGET**: Standard conversations get letter goals too

### 8. OBSERVATION DECK
**CURRENT**: No player deck
**TARGET**: Player has 20-card observation deck

## Implementation Phases

### PHASE 1: Core Comfort Overhaul ✅ COMPLETED
- [x] Change comfort range to -3 to +3
- [x] Start at 0, not 5
- [x] Weight-based comfort changes
- [x] State transitions at ±3
- [x] Delete momentum system entirely

### PHASE 2: Remove Depth System ✅ COMPLETED
- [x] Delete Depth property from ConversationCard
- [x] Remove all depth-based filtering
- [x] Remove depth from JSON files
- [x] Update card draw to pure state filtering

### PHASE 3: Card State Filtering ✅ COMPLETED
- [x] Add DrawableStates property to cards
- [x] Update card_templates.json with drawable states
- [x] Make LISTEN filter by current state only
- [x] Update UI to show drawable states (backend working, UI needs testing)

### PHASE 4: New Card Types ✅ COMPLETED
- [x] Add CardCategory.Patience
- [x] Create patience cards in JSON (5 cards added)
- [x] Implement patience modification mechanics
- [x] No double-duty with other mechanics (patience ONLY adds patience)

### PHASE 5: Fix Weight Limits ✅ COMPLETED
- [x] HOSTILE: 0 (cannot play)
- [x] GUARDED: 1  
- [x] CONNECTED: 4
- [x] Verify all states match spec

### PHASE 6: Goal Card System ✅ COMPLETED
- [x] Standard conversations shuffle letter goals
- [x] Exactly ONE goal per conversation type
- [x] Goal urgency: 3 turns to play
- [x] Letter cards ARE goal cards

### PHASE 7: Observation Deck ✅ COMPLETED
- [x] Create player observation deck system
- [x] Max 20 cards
- [x] Weight 1, 85% success
- [x] State changes only
- [x] 24-48 hour expiration

## Critical Principles
1. **DELETE, don't deprecate** - Remove old code entirely
2. **NO compatibility layers** - Clean break
3. **ALL content from JSON** - No hardcoded cards or text
4. **Single purpose mechanics** - Each mechanic does ONE thing
5. **Perfect information** - All mechanics visible to player

## Files to Modify

### Core System Files
- `/src/Game/ConversationSystem/Models/ConversationSession.cs` - Core session logic
- `/src/Game/ConversationSystem/Core/ConversationCard.cs` - Card model
- `/src/Game/ConversationSystem/Core/EmotionalState.cs` - State rules
- `/src/Game/ConversationSystem/Managers/CardSelectionManager.cs` - Card play logic

### Content Files
- `/src/Content/Templates/card_templates.json` - Card definitions
- `/src/Content/Templates/npc_conversation_decks.json` - NPC decks
- `/src/Content/Templates/npc_goal_decks.json` - Goal cards

### UI Files
- `/src/Pages/Components/ConversationContent.razor(.cs)` - Conversation UI
- `/src/Pages/ConversationScreen.razor(.cs)` - Screen logic

## Validation Checklist
- [ ] Comfort starts at 0, range -3 to +3
- [ ] No depth property anywhere
- [ ] No momentum system
- [ ] Cards show drawable states
- [ ] Weight determines comfort change
- [ ] State transitions at ±3 comfort
- [ ] Patience cards exist and work
- [ ] Goal cards in Standard conversations
- [ ] Player observation deck functional
- [ ] All content from JSON files