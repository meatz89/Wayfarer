# UI Mockup Implementation Plan - Exact Screens with JSON-Driven Content

## Overview
Implement the EXACT UI from HTML mockups with ALL content systematically generated from JSON data and game mechanics, matching the mockups precisely.

## Current Status
Started: 2025-08-21
Last Updated: 2025-08-21 (14:30)
Status: 🚧 IN PROGRESS - Refactoring UI screens to match mockups

## Phase 1: JSON Data Structure (POC Setup) ✅ COMPLETE
**Create complete JSON content for POC scenario**

### 1.1 Enhance npcs.json with POC state:
- [x] Created npcs_poc.json with Elena DESPERATE state, 8-minute deadline
- [x] Marcus has CALCULATING state with commerce letter
- [x] Lord Blackwood NEUTRAL, leaving at 5 minutes

### 1.2 Create card_templates.json:
- [x] Created with 15+ card templates
- [x] Includes COMFORT, STATE, and CRISIS types
- [x] Set bonuses and success formula defined
- [x] Persistence types and connection types specified

### 1.3 Create observations.json:
- [x] Created with observations for all locations
- [x] Market Square: guards, merchants, cart service
- [x] Tavern: noble schedule, Elena's distress
- [x] Noble District: Lord preparing, guard patterns
- [x] Each observation has type, cost, relevance

## Phase 2: Backend Categorical Generation ✅ COMPLETE
**Fix and complete the categorical generators**

### 2.1 Fix ConversationNarrativeGenerator.cs:
- [x] Fix type: NPC.PersonalityType not Personality
- [x] Fix NPCRelationshipTracker return types
- [x] Map emotional states to narrative categories
- [x] Generate context from NPC state + deadline

### 2.2 Fix LocationNarrativeGenerator.cs:
- [x] Use DomainTags instead of LocationType
- [x] Fix "Standing Watch" → "StandingWatch" enum
- [x] Calculate atmosphere from NPCs present
- [x] Generate location mood categories

### 2.3 Fix CardContextGenerator.cs:
- [x] Remove duplicate EmotionalWeight enum
- [x] Fix "ProposeDeaclass" → "ProposeDeal" typo
- [x] Fix CardContext initialization (init-only) - commented out
- [x] Map observations to card templates
- [x] Added missing CardTemplateType enum values

## Phase 3: Frontend Text Rendering ✅ COMPLETE
**Create components that map categories to text**

### 3.1 Create StateNarrativeRenderer.razor:
- [x] Map EmotionalState.DESPERATE → "Time is running short..."
- [x] Map personality + state → specific dialogue
- [x] Generate contextual narrative based on deadlines

### 3.2 Create CardDialogueRenderer.razor:
- [x] Map CardTemplateType + context → actual card text
- [x] Generate success percentages with formula
- [x] Handle special markers (Crisis, State, Comfort)
- [x] Elena desperate dialogue variations

### 3.3 Create LocationAtmosphereRenderer.razor:
- [x] Map atmosphere categories → descriptive text
- [x] Generate NPC presence descriptions
- [x] Create observation hints for each location
- [x] Time-of-day variations

## Phase 4: Update UI to Match Mockups EXACTLY 🚧 IN PROGRESS

### 4.1 ConversationScreen.razor updates:
- [x] Add exact div structure from mockup
- [x] Display cards with weight dots, percentages, outcomes
- [x] Show "Crisis Card" and "State Card" markers
- [x] Add LISTEN/SPEAK buttons with state effects
- [x] Display emotional state with rules
- [x] Integrated StateNarrativeRenderer for categorical text
- [x] Integrated CardDialogueRenderer for card text

### 4.2 LocationScreen.razor updates:
- [ ] Add location path breadcrumbs
- [ ] Show NPCs with emotional state badges
- [ ] Display "If approached:" preview
- [ ] Show observation opportunities with arrows
- [ ] Add area navigation options

### 4.3 CSS updates:
- [ ] Copy exact styles from mockups
- [ ] Card borders by type (comfort=green, state=brown, crisis=red)
- [ ] Weight display circles
- [ ] Success percentage colors
- [ ] Progress bars with thresholds

## Phase 5: Game State Integration ⏳ PENDING

### 5.1 Initialize POC scenario:
- [ ] Load Elena with DESPERATE state, 8-minute deadline
- [ ] Load Marcus with commerce letter in queue
- [ ] Set player resources (10 attention, 3 coins)
- [ ] Create observation about guards

### 5.2 Connect mechanics:
- [ ] Calculate success chances: 70% - (Weight × 10%) + (Status × 3%)
- [ ] Apply emotional state rules (draw counts, weight limits)
- [ ] Handle state transitions on LISTEN
- [ ] Process set bonuses for same-type cards

### 5.3 Test critical paths:
- [ ] Elena desperate conversation flow
- [ ] Observation becoming opportunity card
- [ ] Crisis card free in desperate state
- [ ] State progression desperate → tense → neutral

## Implementation Timeline
1. **Fix compilation errors** (30 min) - ✅ COMPLETE
2. **Create JSON data files** (30 min) - ✅ COMPLETE
3. **Create text renderer components** (1 hour) - ✅ COMPLETE
4. **Update UI screens to match mockups** (2 hours) - ⏳ NEXT
5. **Copy CSS from mockups** (30 min) - ⏳ PENDING
6. **Test with Playwright** (1 hour) - ⏳ PENDING

## Progress Summary
- Build now compiles successfully (0 errors)
- Created POC data files with Elena DESPERATE scenario  
- Created text rendering components for narrative generation
- Deleted unnecessary JSON files, kept only POC essentials
- Phase8_InitialLetters already creates Elena's DESPERATE state (1 min deadline, SAFETY stakes)
- Ready to refactor UI screens to match mockups exactly

## Success Criteria
- [ ] UI matches HTML mockups pixel-perfect
- [ ] All text generated from JSON + mechanics
- [ ] Elena appears DESPERATE with countdown
- [ ] Cards show exact weights, percentages, outcomes
- [ ] Observations convert to opportunity cards
- [ ] Crisis cards free in desperate state
- [ ] Location shows NPCs with emotional states
- [ ] All content emerges from categorical data

## Key Principles
1. **Backend = Categories Only**: No text generation in backend services
2. **Frontend = Text Rendering**: All narrative text created in Razor components
3. **JSON = Content Source**: All NPCs, locations, cards from JSON files
4. **Mechanics = Game State**: Emotional states, deadlines, tokens drive everything
5. **Mockups = Exact Target**: Match HTML mockups precisely

## Files to Track
- `/src/GameState/ConversationNarrativeGenerator.cs` - Categorical narrative
- `/src/GameState/LocationNarrativeGenerator.cs` - Location atmosphere
- `/src/GameState/CardContextGenerator.cs` - Card enrichment
- `/src/Pages/ConversationScreen.razor` - Conversation UI
- `/src/Pages/LocationScreen.razor` - Location UI
- `/src/Content/Templates/npcs.json` - NPC data
- `/src/Content/Templates/card_templates.json` - Card definitions
- `/src/Content/Templates/observations.json` - Observable content

## Current Compilation Errors
- 43 errors in narrative generators (fixing now)
- Type mismatches: PersonalityType, NPCRelationshipTokens
- Missing enum values in CardTemplateType
- CardContext initialization issues