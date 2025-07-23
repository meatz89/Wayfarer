# Wayfarer Tutorial Implementation - Remaining TODOs

This document provides a comprehensive list of all remaining implementation tasks needed to complete the tutorial user stories. Each item has been triple-checked against the current codebase implementation.

## Critical Integration Issues (Blocking Tutorial from Running)

### 1. Narrative System Integration ❌ CRITICAL
**Problem**: The narrative system exists but is NOT connected to the game flow.
**Current State**: 
- `NarrativeManager` class exists with all functionality
- Tutorial narrative fully defined in `NarrativeBuilder`
- But `LocationActionManager` doesn't check narrative state
**Required**:
- Inject `NarrativeManager` into `LocationActionManager`
- Call `NarrativeManager.FilterActions()` before returning available actions
- Ensure narrative overrides are applied to conversations

### 2. NarrativeOverlay Component Missing ❌ CRITICAL
**Problem**: Players can't see tutorial guidance without the overlay.
**Current State**: 
- Referenced in `MainGameplayView.razor` but file doesn't exist
- No way for players to see current objectives or hints
**Required**:
- Create `NarrativeOverlay.razor` component
- Display current step name, description, and guidance
- Show progress (step X of Y)
- Make it minimizable but always visible during tutorial

### 3. Tutorial Auto-Start Missing ❌ CRITICAL
**Problem**: Tutorial doesn't start automatically on new game.
**Current State**:
- Can manually start via debug panel
- New games don't trigger tutorial
**Required**:
- Modify `GameWorldManager.StartGame()` to call `NarrativeManager.StartNarrative("wayfarer_tutorial")`
- Ensure tutorial starts before any other game actions

### 4. Save/Load Integration Missing ❌ CRITICAL
**Problem**: Tutorial progress is lost on reload.
**Current State**:
- `NarrativeManager` state not included in `GameWorld` serialization
- `FlagService` state not persisted
**Required**:
- Add narrative state to `GameWorld` save/load
- Include `FlagService` data in persistence
- Test tutorial continues correctly after reload

## Epic 1: Tutorial Framework - Remaining TODOs

### Story 1.1: Tutorial Mode Initialization
**Status**: ⚠️ Partially Implemented

**TODO 1.1.1**: Hide UI Elements During Tutorial
- [ ] Add `IsInTutorial` check to `NavigationBar.razor`
- [ ] Hide Queue button until `tutorial_first_letter_accepted` flag
- [ ] Hide Relations button until `first_token_earned` flag  
- [ ] Hide Obligations button until `tutorial_patron_met` flag
- [ ] Add tutorial stage tracking to control UI visibility

**TODO 1.1.2**: Disable Save/Load During Tutorial
- [ ] Check for active tutorial in save/load buttons
- [ ] Show message "Cannot save during tutorial"
- [ ] Re-enable after `tutorial_complete` flag

### Story 1.2: Forced Action System
**Status**: ✅ Implemented (once integration is complete)

### Story 1.3: Tutorial Progress Tracking  
**Status**: ⚠️ Partially Implemented

**TODO 1.3.1**: Expose Step Progress in NarrativeManager
- [ ] Add `GetCurrentStepIndex()` method to `NarrativeManager`
- [ ] Add `GetTotalSteps()` method to `NarrativeManager`
- [ ] Update `NarrativeOverlay` to use real data instead of hardcoded values

## Epic 2: Tutorial Locations - Remaining TODOs

### Story 2.1: Limited Location Access
**Status**: ❌ Not Implemented

**TODO 2.1.1**: Create Tutorial Locations
- [ ] Add to `locations.json`:
  - Lower Ward (Millbrook slum district)
  - Docks (Millbrook waterfront)  
  - Merchant's Rest (Millbrook inn)
- [ ] Define proper routes between locations
- [ ] Set appropriate domain tags

**TODO 2.1.2**: Implement Location Visibility Control
- [ ] Add `IsVisible` property to `Location` class
- [ ] Add `VisibilityFlag` property for conditional visibility
- [ ] Modify `LocationRepository.GetAllLocations()` to filter by visibility
- [ ] Hide non-tutorial locations during tutorial

### Story 2.2: Tutorial-Specific Location Spots
**Status**: ❌ Not Implemented  

**TODO 2.2.1**: Create Tutorial Spots
- [ ] Add to `spots.json`:
  - Abandoned Warehouse (Lower Ward)
  - Lower Ward Square (Lower Ward)
  - Martha's Wharf (Docks)
  - Private Room (Merchant's Rest)
- [ ] Ensure spots have appropriate actions available

## Epic 3: Tutorial NPCs - Remaining TODOs

### Story 3.1: Tutorial NPC Creation
**Status**: ❌ Not Implemented

**TODO 3.1.1**: Add Tutorial NPCs to Content
- [ ] Add to `npcs.json`:
  - `tam_beggar`: Tam the Beggar (Common type, info only)
  - `martha_docks`: Martha the Docker (Trade type, work provider)
  - `elena_scribe`: Elena the Scribe (Trust type, friend)
  - `fishmonger`: Fishmonger (Common type, simple letters)
  - `patron_intermediary`: Mysterious Intermediary (Noble type, patron)
- [ ] Set proper locations and spot IDs for each
- [ ] Configure appropriate token types

### Story 3.2: NPC Availability Schedule  
**Status**: ❌ Not Implemented

**TODO 3.2.1**: Implement NPC Scheduling
- [ ] Modify `NPC.IsAvailable()` to check narrative flags
- [ ] Add schedule data structure to NPCs (time blocks + narrative conditions)
- [ ] Update `LocationActionManager.IsNPCAvailable()` to respect schedules
- [ ] Test NPCs appear/disappear at correct times

## Epic 4: Day 1 - Movement and Survival - Remaining TODOs

### Story 4.1: Tutorial Start State
**Status**: ✅ Implemented (starting conditions work)

### Story 4.2: Forced Movement Tutorial
**Status**: ❌ Not Implemented Properly

**TODO 4.2.1**: Fix Movement Tutorial
- [ ] Replace `LocationAction.Rest` placeholder with actual movement
- [ ] Create proper "Leave Warehouse" action
- [ ] Implement location transition animation/feedback
- [ ] Show time advancement clearly

### Story 4.3: First NPC Interaction
**Status**: ✅ Implemented (once NPCs added)

### Story 4.4: Stamina Crisis
**Status**: ❌ Not Implemented

**TODO 4.4.1**: Implement Collapse Mechanic
- [ ] Add check for 0 stamina in `TimeManager.AdvanceTime()`
- [ ] Create collapse event that forces rest
- [ ] Implement "robbed while unconscious" mechanic
- [ ] Add narrative for waking up after collapse

## Epic 5: Day 2 - Work and Tokens - Remaining TODOs

### Story 5.1: First Work Action
**Status**: ✅ Implemented (work system exists)

### Story 5.2: First Token Earned  
**Status**: ✅ Implemented (token display works)

### Story 5.3: Elena Introduction
**Status**: ✅ Implemented (conversation system supports choices)

## Epic 6: Day 3 - Letter Discovery - Remaining TODOs

### Story 6.1: First Letter Offer
**Status**: ✅ Implemented (letter offering system works)

### Story 6.2: Queue Introduction
**Status**: ✅ Implemented (queue UI exists and works)

### Story 6.3: Collection and Delivery
**Status**: ✅ Implemented (two-step delivery works)

## Epic 7: Days 4-5 - Queue Pressure and Token Burning - Remaining TODOs

### Story 7.1: Multiple Letters
**Status**: ✅ Implemented (queue handles multiple letters)

### Story 7.2: Queue Crisis Tutorial  
**Status**: ✅ Implemented (token burning mechanics work)

### Story 7.3: Consequence Tracking
**Status**: ⚠️ Partially Implemented

**TODO 7.3.1**: Implement Child Death Consequence
- [ ] Add flag for `medicine_delivered_in_time`
- [ ] Make Martha hostile if medicine not delivered
- [ ] Change Martha's conversation based on outcome
- [ ] Update Elena's dialogue to reference the choice

## Epic 8: Days 6-7 - Token Debt - Remaining TODOs

### Story 8.1: Desperation Mechanics
**Status**: ✅ Implemented (can check for hostile NPCs)

### Story 8.2: Debt Implementation
**Status**: ✅ Implemented (token debt system works)

## Epic 9: Days 8-9 - Mystery Build-up - Remaining TODOs

### Story 9.1: Tam's Return
**Status**: ✅ Implemented (narrative step exists)

### Story 9.2: The Mysterious Letter
**Status**: ⚠️ Partially Implemented

**TODO 9.2.1**: Create Special Letter Item
- [ ] Add "Mysterious Letter" as special item (not queue letter)
- [ ] Create reading interface for special letters
- [ ] Show gold seal and fine parchment in UI
- [ ] Trigger patron meeting availability after reading

## Epic 10: Day 10 - Patron and Obligation - Remaining TODOs

### Story 10.1: Patron Meeting
**Status**: ✅ Implemented (conversation exists)

### Story 10.2: First Standing Obligation
**Status**: ❌ Not Properly Integrated

**TODO 10.2.1**: Create Obligation During Tutorial
- [ ] Remove auto-creation of patron obligation from `GameWorldManager`
- [ ] Add `ObligationToCreate` property to `NarrativeStep`
- [ ] Inject `StandingObligationManager` into `NarrativeManager`
- [ ] Create "patrons_expectation" obligation when accepting patron offer

### Story 10.3: First Patron Letter
**Status**: ✅ Implemented (patron letter system works)

### Story 10.4: Tutorial Completion
**Status**: ⚠️ Partially Implemented

**TODO 10.4.1**: Unlock Full Game
- [ ] Show "Tutorial Complete" message
- [ ] Remove all UI hiding restrictions
- [ ] Enable all locations and NPCs
- [ ] Enable save/load functionality
- [ ] Transition to "Chapter 1" or main game

## Epic 11: Tutorial Content Data - Remaining TODOs

### Story 11.1: Tutorial-Specific Letters
**Status**: ❌ Not Implemented

**TODO 11.1.1**: Create Tutorial Letter Templates
- [ ] Add to letter templates:
  - Martha's fish oil package (Common, 2 coins, 3 days)
  - Martha's urgent medicine (Trust, 0 coins, 1 day, high urgency)
  - Fishmonger's routine letter (Common, 3 coins, 3 days)
  - First patron letter (Noble, 0 coins, 3 days, Harbor Master)
- [ ] Ensure only tutorial letters appear during tutorial

### Story 11.2: Tutorial Conversation Scripts
**Status**: ✅ Implemented (conversations defined in narrative)

## Additional TODOs Not Covered in User Stories

### Equipment System
**TODO**: The tutorial mentions "ragged clothes" and "proper equipment" but no equipment system exists
- [ ] Create basic equipment system (or remove references from tutorial)
- [ ] Add equipment slots to Player
- [ ] Create "Letter Satchel" equipment item
- [ ] Show equipment benefits in UI

### Tutorial Testing
**TODO**: Comprehensive end-to-end testing needed
- [ ] Test complete 10-day flow
- [ ] Verify all branches (save money vs eat, burn token vs don't)
- [ ] Check save/load at each day
- [ ] Ensure no soft locks possible
- [ ] Verify UI elements appear at correct times

## Priority Order for Implementation

1. **CRITICAL**: Fix integration issues (1-4) - Tutorial won't run without these
2. **HIGH**: Create locations and NPCs (Epic 2-3) - Core content needed
3. **HIGH**: Fix movement tutorial and add collapse mechanic (Epic 4)
4. **MEDIUM**: Add UI visibility controls (Epic 1)
5. **MEDIUM**: Fix patron obligation creation (Epic 10)
6. **LOW**: Create tutorial-specific letters (Epic 11)
7. **LOW**: Add equipment system or remove references

## Estimated Effort

- Critical Integration: 2-3 days
- Content Creation (Locations/NPCs): 1-2 days  
- Mechanical Fixes: 2-3 days
- Polish and Testing: 2-3 days

**Total: 7-11 days for complete tutorial implementation**

## Note on Current State

The core systems (narrative, tokens, letters, obligations) are **excellently implemented**. The tutorial content is **fully defined** in the narrative system. The main gap is **integration and content creation**. Once the critical integration issues are resolved and the locations/NPCs are added, the tutorial should work with minimal additional effort.