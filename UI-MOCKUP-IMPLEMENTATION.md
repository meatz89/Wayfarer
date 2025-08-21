# UI Mockup Implementation Plan - Exact Screens with JSON-Driven Content

## Overview
Implement the EXACT UI from HTML mockups with ALL content systematically generated from JSON data and game mechanics, matching the mockups precisely.

## Current Status
Started: 2025-08-21
Last Updated: 2025-08-21 (Session 8)
Status: ❌ UI DOES NOT MATCH MOCKUP - Wrong structure, missing sections, wrong content

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

## Phase 2: Backend Categorical Generation ❌ DELETED (LEGACY)
**These were LEGACY CODE not in target architecture - ALL DELETED**

### Deleted Files:
- ❌ ConversationNarrativeGenerator.cs (legacy)
- ❌ LocationNarrativeGenerator.cs (legacy)
- ❌ CardContextGenerator.cs (legacy)
- ❌ NPCStateResolver.cs (legacy)
- ❌ All "literary UI" components (legacy)

### Lesson Learned:
- If it's not in conversation-system.md, it's LEGACY
- HIGHLANDER PRINCIPLE: There can be only ONE
- No duplicate enums, no compatibility layers

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
- [x] Location path breadcrumbs (basic structure)
- [x] Location traits (SYSTEMATIC - LocationTraitsParser loads from JSON)
- [x] Actions section (SYSTEMATIC - ActionGenerator integration complete)
- [x] NPCs emotional states (working with categorical descriptions)
- [x] Observations (WORKING - ObservationParser converts JSON to enums)
- [x] Areas within location (displaying spots properly)
- [ ] Display "If approached:" preview with state effects
- [ ] CSS styling to match mockup exactly
- [x] All data loading systematically from JSON/mechanics

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
4. **Update UI screens to match mockups** (2 hours) - 🔧 IN PROGRESS
5. **Create ObservationParser** (30 min) - ✅ COMPLETE
6. **Copy CSS from mockups** (30 min) - ✅ COMPLETE
7. **Test with Playwright** (1 hour) - 🔧 IN PROGRESS

## Progress Summary (Session 8)
### What Was Actually Completed:
- ✅ CSS files updated with mockup styles
- ✅ Removed BottomStatusBar component (1 line change)
- ❌ Everything else still broken

### What's Still Wrong:
1. **Routes showing on location screen** - Lines 144-159 in LocationScreen.razor NOT REMOVED
2. **Missing "Actions" section header** - No header element before actions
3. **Missing "People of Note" section header** - No header element before NPCs  
4. **Wrong actions** - Still using QuickActions, not mockup actions (Rest, Purchase, Listen, Travel)
5. **No Travel modal** - Travel should open modal, not show routes inline
6. **Areas within location** - Questionable if this should even be visible

### Honest Assessment:
- UI looks slightly better with CSS but structure is WRONG
- Only made 1 actual fix (removing BottomStatusBar)
- Need to actually READ the mockup HTML and match it EXACTLY
- Stop claiming things are complete when they're not

## Progress Summary (Session 7)
- ✅ Created ObservationParser.cs following existing parser pattern
- ✅ Created LocationTraitsParser.cs for systematic trait loading from JSON
- ✅ Integrated ActionGenerator replacing all hardcoded actions
- ✅ Fixed all Location.Id vs LocationID inconsistencies
- ✅ Resolved all ObservationType enum conflicts
- ✅ Fixed IContentDirectory path issues
- ✅ Updated GameFacade to use all parsers systematically
- ✅ Build successful with 0 errors
- ✅ Playwright test confirmed UI working with systematic data:
  * Location traits display from JSON properties
  * Actions generated by ActionGenerator
  * Observations load with proper enum conversion
  * NPCs show with categorical descriptions
  * Areas within location navigate properly
- 📝 CSS styling still needs to be extracted from mockups

## Success Criteria
- [ ] UI matches HTML mockups pixel-perfect - **NOT EVEN CLOSE**
- [ ] All text generated from JSON + mechanics - **PARTIALLY**
- [ ] Elena appears DESPERATE with countdown - **NOT TESTED**
- [ ] Cards show exact weights, percentages, outcomes - **NOT TESTED**
- [ ] Observations convert to opportunity cards - **NOT TESTED**
- [ ] Crisis cards free in desperate state - **NOT TESTED**
- [ ] Location shows NPCs with emotional states - **PARTIALLY**

## Session 9 Implementation Plan (Current)

### Key Learning: NO FEATURE CREEP
- Actions must use EXISTING mechanics only
- No market system, no "current events" system
- Use what we have: attention, time, observations, travel, coins

### Categorical Action Generation from Domain Tags
Actions emerge from location/spot tags using existing mechanics:
- **"PUBLIC_SQUARE"** → "Rest at Fountain" (advance time to next period)
- **"CROWDED"** → "Listen to Town Crier" (spend 1 attention for observation)
- **"CROSSROADS"** → "Travel" (opens modal for route selection)
- **"COMMERCE"** → "Purchase Provisions" (basic coin spending)

### Implementation Tasks:
1. ✅ Remove BottomStatusBar from GameUI.razor
2. 🔧 Remove Routes section from LocationScreen (lines 144-159)
3. 📝 Add "Actions" section header before actions
4. 📝 Add "People of Note" section header before NPCs
5. 📝 Keep "Areas Within Location" section
6. 📝 Update central_fountain tags in location_Spots.json
7. 📝 Update ActionGenerator for tag-based generation
8. 📝 Create TravelModal component
9. 📝 Test with Playwright

### Next Session Priority:
- [ ] All content emerges from categorical data

## Key Principles
1. **Backend = Categories Only**: No text generation in backend services
2. **Frontend = Text Rendering**: All narrative text created in Razor components
3. **JSON = Content Source**: All NPCs, locations, cards from JSON files
4. **Mechanics = Game State**: Emotional states, deadlines, tokens drive everything
5. **Mockups = Exact Target**: Match HTML mockups precisely

## Files to Track
- `/src/Pages/LocationScreen.razor` - Location UI (needs structure fixes)
- `/src/Pages/LocationScreen.razor.cs` - Location logic (needs modal handling)
- `/src/Services/ActionGenerator.cs` - Action generation (needs tag mapping)
- `/src/Content/Templates/location_Spots.json` - Spot data (needs tag updates)
- `/src/Pages/Components/TravelModal.razor` - Travel modal (TO CREATE)
- `/src/Pages/ConversationScreen.razor` - Conversation UI
- `/src/Content/Templates/npcs.json` - NPC data
- `/src/Content/Templates/card_templates.json` - Card definitions
- `/src/Content/Templates/observations.json` - Observable content

## Key Architecture Decisions (Session 5)

### Categorical Description System
Descriptions emerge from:
```
Profession → Base Activity
- Scribe → "Hunched over documents"
- Merchant → "Arranging goods"
- Innkeeper → "Polishing glasses"

EmotionalState → Modifier
- DESPERATE → "clutching with white knuckles"
- TENSE → "glancing nervously"
- NEUTRAL → "focused on task"

Urgency → Props
- Has urgent letter → "sealed letter"
- No urgency → profession-specific tools
```

### The ONLY Emotional States (9)
- NEUTRAL, GUARDED, OPEN, CONNECTED
- TENSE, EAGER, OVERWHELMED
- DESPERATE, HOSTILE

### Starting State Logic
```csharp
// From letter deadlines directly
SAFETY + <6h → DESPERATE
Any <12h → TENSE
None → NEUTRAL
```