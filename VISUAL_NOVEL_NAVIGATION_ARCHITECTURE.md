# Visual Novel Scene Navigation Architecture

## Executive Summary

Transform LocationContent from an "Everything Screen" anti-pattern (30-50 simultaneous options) into a visual novel-style scene controller with progressive disclosure through layered navigation. Players navigate through focused sub-scenes (3-5 choices per screen) that shift focus or go deeper, creating immersion and strategic clarity. This leverages existing GameScreen navigation architecture and mirrors the proven pattern already working in TravelContent.

---

## The Problem: Everything Screen Anti-Pattern

### Current State
LocationContent.razor displays ALL content simultaneously:
- Location atmosphere
- ALL NPCs at spot with ALL their actions
- ALL Social goals (placed on NPCs)
- ALL Mental goals (ambient at location)
- ALL Physical goals (ambient at location)
- ALL discovered investigations
- ALL spots within venue
- ALL location-specific actions

**Result:** 30-50 clickable options on ONE screen. Player overwhelmed. Zero immersion. No strategic focus.

### Root Cause
LocationContent treats itself like a challenge screen (focused, tactical) but it's actually a world screen (exploratory, strategic). World screens need NAVIGATION between focused views, not everything displayed at once.

### Why This Matters
This isn't a CSS problem. This isn't a layout problem. This is a FUNDAMENTAL ARCHITECTURAL MISMATCH between:
- **What we're building**: Visual novel-style narrative RPG
- **What we have**: Traditional RPG menu system

---

## The Solution: Visual Novel Scene Navigation

### VN Navigation Pattern
Visual novels use scene-based navigation with progressive disclosure:

1. **Landing Scene**: 3-5 high-level choices representing FOCUS SHIFTS
   - "Look around" (shift focus → who's present)
   - "Check notice board" (shift focus → ambient challenges)
   - "Move to another spot" (shift focus → spatial navigation)
   - "Leave tavern" (shift focus → travel)

2. **Focused Sub-Scenes**: Each choice leads to NEW focused screen with 3-5 options
   - "Look around" → See NPCs present → Pick ONE NPC → See THAT NPC's goals
   - Pick NPC → See full NPC detail + their goals → Commit to ONE goal
   - Pick goal → See commitment details → BEGIN or BACK

3. **Navigation Types**:
   - **LATERAL**: Shifting focus at same depth ("Look around" ↔ "Check notice board")
   - **VERTICAL**: Going deeper into one area ("Look around" → "Approach Gedren" → "Speak with him")
   - **BACK**: Return to previous scene using history stack

4. **Information as Inventory**: Each sub-scene reveals new information progressively, not all at once

---

## Architectural Decision: Option 1 - Internal Sub-Screens

### The Chosen Approach
LocationContent becomes a **scene controller** with internal navigation state machine and conditional view rendering. Multiple sub-scenes rendered within the Location screen context, managed by internal navigation state.

### Why This Is Correct

#### 1. PRECEDENT EXISTS
**TravelContent already uses this exact pattern successfully:**
- Internal state: `CurrentTravelContext`
- Conditional rendering: Route Selection View vs Path Cards View
- Two distinct sub-scenes within ScreenMode.Travel
- Works seamlessly with GameScreen navigation
- Zero issues in production

#### 2. SCREEN HIERARCHY PRESERVED
```
GameScreen (Top Level - Screen Switching)
├─ ScreenMode.Location → LocationContent (Scene Controller)
│  ├─ Landing View (Sub-scene)
│  ├─ Looking Around View (Sub-scene)
│  ├─ NPC Detail View (Sub-scene)
│  └─ Goal Detail View (Sub-scene)
├─ ScreenMode.Travel → TravelContent (Scene Controller) ✅ PROVEN PATTERN
│  ├─ Route Selection View ✅ EXISTS
│  └─ Path Cards View ✅ EXISTS
├─ ScreenMode.SocialChallenge → ConversationContent (Single Focused Screen)
├─ ScreenMode.MentalChallenge → MentalContent (Single Focused Screen)
└─ ScreenMode.PhysicalChallenge → PhysicalContent (Single Focused Screen)
```

**Semantic Clarity:**
- `ScreenMode.Location` = "player is at a venue" (semantic state)
- `ScreenMode.Travel` = "player is traveling" (semantic state)
- Internal view states = implementation details of that semantic state

#### 3. STATE MANAGEMENT CLARITY
- **GameScreen manages**: Top-level screen switching (Location ↔ Travel ↔ Challenge)
- **Scene controllers manage**: Internal sub-scene navigation (Landing → LookingAround → NPC Detail)
- **Challenge screens**: No sub-scenes, already focused (one tactical screen)

#### 4. NO SCREENSMODE ENUM POLLUTION
**WRONG:**
- `ScreenMode.LocationLanding`
- `ScreenMode.LocationLookingAround`
- `ScreenMode.LocationApproachNPC`
- `ScreenMode.LocationNoticeBoard`
- etc.

**Pollutes top-level enum with implementation details. Violates separation of concerns.**

**RIGHT:**
- `ScreenMode.Location` (one semantic state)
- LocationContent internally manages Landing, LookingAround, ApproachNPC, etc.
- Same as TravelContent managing RouteSelection vs PathCards internally

#### 5. MINIMAL CHANGES
- GameScreen: **ZERO changes**
- GameScreen.razor.cs: **ZERO changes**
- ScreenMode enum: **ZERO changes**
- Navigation system: **ZERO changes**
- LocationContent: Add internal navigation (same pattern as TravelContent)

### Rejected Alternatives

**Option 2: Elevate to ScreenMode Level**
- Bloats ScreenMode enum with implementation details
- Violates separation of concerns
- Creates navigation system duplication
- Makes GameScreen aware of Location-specific navigation states
- WRONG

**Option 3: Component Composition (Router Pattern)**
- Same as Option 1 but with unnecessary indirection layer
- No benefits over direct conditional rendering
- More complexity for zero gain

**Option 4: Modal/Overlay Pattern**
- Modals are for interruptions, not primary navigation flow
- Wrong UX pattern for scene transitions
- Doesn't match VN navigation feel

---

## Existing Architecture Analysis

### What We Already Have (DO NOT REBUILD)

#### 1. GameScreen Navigation System (GameScreen.razor.cs)
- **ScreenMode enum** (line 797-805): Top-level screen types
- **NavigateToScreen()** (line 200-249): Screen switching with transitions
- **Navigation stack** (line 38): `Stack<ScreenContext>` for back button
- **ScreenContext** (line 807-812): Screen state preservation
- **ScreenStateData** (line 817-825): Strongly-typed context data
- **Transition management** (line 36): IsTransitioning flag
- **State serialization** (line 251-264): Save/restore screen state

#### 2. GameScreen Rendering (GameScreen.razor)
- **CascadingValue pattern** (line 147): Parent provides itself to children
- **Switch statement** (line 152-184): Render current screen
- **ContentVersion keying** (line 148): Force re-render on screen change
- **Consistent screen structure**: All screens rendered inside game-container

#### 3. TravelContent Pattern (REFERENCE IMPLEMENTATION)
- **Internal state**: `CurrentTravelContext` determines active view
- **Conditional rendering**: `@if (HasActiveTravelSession())` switches views
- **Two sub-scenes**: Route Selection vs Path Cards (TravelPathContent component)
- **State preservation**: Context maintained during view switches
- **Clean separation**: Each view is focused on one task

#### 4. Challenge Screen Pattern (ALREADY PERFECT)
- ConversationContent: One focused tactical screen
- MentalContent: One focused tactical screen
- PhysicalContent: One focused tactical screen
- **NO sub-scenes needed** - already at correct focus level

---

## Design Patterns: VN Architecture

### How Visual Novels Work

#### 1. Scene Controller
Central component that manages scene state and view rendering:
- Maintains current scene/view state
- Handles navigation between views
- Preserves context during transitions
- Provides navigation methods to child views

#### 2. View Rendering
Each view is a complete UI state with:
- Background/context display
- Focused information set (3-5 key items)
- Navigation choices (3-5 options)
- Clear purpose and focus

#### 3. Navigation Flow
```
Landing Scene (WHERE AM I?)
  ├─> Look Around (WHO IS HERE?) → LATERAL shift
  │     └─> Approach NPC (WHAT CAN THEY DO?) → VERTICAL deeper
  │           └─> View Goal Detail (COMMIT?) → VERTICAL deeper
  │                 └─> Begin Challenge (EXECUTE) → Screen switch to ScreenMode.Challenge
  ├─> Check Notice Board (WHAT CHALLENGES?) → LATERAL shift
  │     └─> View Goal Detail → VERTICAL deeper
  ├─> Move to Spot (WHERE ELSE?) → LATERAL shift
  └─> Leave Location → Screen switch to ScreenMode.Travel
```

#### 4. Progressive Disclosure
Information revealed layer by layer:
- **Landing**: High-level options only (what can I focus on?)
- **Layer 1**: Category details (who's here? what challenges exist?)
- **Layer 2**: Specific entity details (this NPC's goals, this challenge's requirements)
- **Layer 3**: Commitment decision (full information before challenge begins)
- **Layer 4**: Challenge execution (tactical screen)

#### 5. History Stack
Back button returns to previous view:
- Push current view state before navigating
- Pop to restore previous view
- Preserve context (which NPC was I looking at?)
- Clear stack when leaving scene controller (return to Location from Challenge)

---

## Implementation Overview

### What Gets Built

#### 1. LocationContent Scene Controller
Transform LocationContent from single view to scene controller:
- Add internal view state enum (Landing, LookingAround, ApproachNPC, etc.)
- Add navigation state management (current view, history stack, selected entities)
- Add navigation methods (NavigateToView, NavigateBack, ResetNavigation)
- Conditional view rendering based on current view state

#### 2. Location Sub-Scene Views
Create focused view components OR inline conditional blocks:

**Landing View:**
- Show 3-5 high-level navigation choices
- Each choice represents focus shift or category
- Atmosphere/flavor text
- Clean, minimal presentation

**Looking Around View:**
- List NPCs present at current spot (3-7 NPCs typically)
- Basic info per NPC (name, personality, connection state)
- Click NPC → Navigate to NPC Detail View
- Back button → Return to Landing

**NPC Detail View:**
- Full NPC information (description, tokens, connection state)
- List THIS NPC's available goals (Social goals placed on this NPC)
- List THIS NPC's direct actions (active goal IDs, exchange if available)
- Click goal → Navigate to Goal Detail View
- Back button → Return to Looking Around

**Notice Board View:**
- List investigations available at location
- List ambient Mental goals at location
- List ambient Physical goals at location
- Click goal → Navigate to Goal Detail View
- Back button → Return to Landing

**Goal Detail View:**
- Full goal information (description, difficulty, costs, rewards)
- System type badge (Social/Mental/Physical)
- Commitment decision: BEGIN CHALLENGE or CANCEL
- BEGIN → Launch challenge (screen switch to appropriate ScreenMode)
- CANCEL (Back button) → Return to previous view

**Spots View:**
- List other spots in current venue
- Show NPCs at each spot
- Click spot → Execute MoveToSpot (instant, free)
- Back button → Return to Landing

#### 3. Navigation State Management
Add to LocationContentBase:
- `LocationViewState` enum (Landing, LookingAround, ApproachNPC, NoticeBoard, GoalDetail, Spots)
- `Stack<LocationViewState>` for back button history
- `SelectedNpcId` for NPC Detail context
- `SelectedGoal` for Goal Detail context
- Navigation methods following TravelContent pattern

#### 4. CSS/Styling for Scene Views
Create scene-specific CSS:
- `.location-landing-view` - Landing scene styles
- `.location-npc-list-view` - NPC list styles
- `.location-npc-detail-view` - NPC detail styles
- `.location-notice-board-view` - Notice board styles
- `.location-goal-detail-view` - Goal detail styles
- `.location-spots-view` - Spots navigation styles

**Styling Principles:**
- Each view has dedicated container class
- Shared components (action cards, goal cards, NPC cards) reuse existing styles
- Scene-specific layouts in location.css
- No duplication of existing styles
- Focus on layout/composition, not recreating components

---

## Component Architecture

### LocationContent Scene Controller

#### Responsibilities
- Manage internal view state (Landing, LookingAround, etc.)
- Handle navigation between views (lateral and vertical)
- Preserve context during navigation (which NPC? which goal?)
- Provide navigation stack for back button
- Query GameWorld for view-specific data
- Launch challenges when goal committed

#### View State Machine
```
Landing (Entry Point)
  ├─> LookingAround (lateral)
  ├─> NoticeBoard (lateral)
  ├─> Spots (lateral)
  └─> [Leave via Travel] (screen switch)

LookingAround
  ├─> ApproachNPC (vertical with context: npcId)
  └─> [Back to Landing]

ApproachNPC
  ├─> GoalDetail (vertical with context: goal)
  └─> [Back to LookingAround]

NoticeBoard
  ├─> GoalDetail (vertical with context: goal)
  └─> [Back to Landing]

GoalDetail
  ├─> [Begin Challenge] (screen switch to appropriate ScreenMode)
  └─> [Back to previous view]

Spots
  ├─> [Move to Spot] (refresh LocationContent, stay on Landing)
  └─> [Back to Landing]
```

#### Context Preservation
When navigating between views:
- **LookingAround → ApproachNPC**: Store which NPC was selected
- **ApproachNPC → GoalDetail**: Store which NPC we came from + which goal selected
- **NoticeBoard → GoalDetail**: Store which goal selected
- **Back navigation**: Restore previous view state and context

When launching challenges:
- **Clear navigation state**: Reset to Landing on challenge end
- **Challenge screens don't need to know**: Where player was before challenge
- **GameScreen handles**: Challenge end → Return to ScreenMode.Location → LocationContent resets to Landing

---

## View Hierarchy and Data Requirements

### Landing View
**Purpose:** High-level navigation hub showing available focus areas

**Data Displayed:**
- Location atmosphere (already rendered above in location header)
- High-level navigation options (NOT detailed data)

**Navigation Options:**
- "Look Around" - IF NPCs present at spot
- "Check Notice Board" - IF investigations OR ambient goals exist
- "Move to Another Spot" - IF multiple spots in venue
- Travel actions - IF location has travel actions (e.g., "Leave Tavern")

**Data Requirements:**
- `AvailableNpcs.Any()` - check if NPCs present
- `DiscoveredInvestigationsAtLocation.Any()` - check if investigations
- `AvailableMentalGoals.Any() || AvailablePhysicalGoals.Any()` - check ambient goals
- `AvailableSpots.Any()` - check if multiple spots
- `LocationActions` - get travel/venue actions

**Navigation:**
- Click option → Navigate to corresponding view
- No back button (this IS the root)

---

### Looking Around View
**Purpose:** Show who is present at current spot

**Data Displayed:**
- List of NPCs at current spot
- Per NPC: Name, personality type, connection state
- Simple card/list presentation

**Data Requirements:**
- `AvailableNpcs` - list of NPCs at current spot
- Per NPC: `Name`, `PersonalityType`, `ConnectionState`

**Navigation:**
- Click NPC → Navigate to NPC Detail View (store NpcId context)
- Back button → Return to Landing

---

### NPC Detail View
**Purpose:** Show ONE NPC's full details and available interactions

**Data Displayed:**
- NPC header: Name, personality, connection state
- Token counts: Trust, Diplomacy, Status, Shadow (with effects)
- NPC description
- Social Goals placed on this NPC (from location)
- NPC Active Goals (from NPC's ActiveGoalIds)
- Exchange option (if NPC has exchange cards)

**Data Requirements:**
- `SelectedNpcId` - which NPC to display
- NPC from `AvailableNpcs.FirstOrDefault(n => n.Id == SelectedNpcId)`
- Actual NPC from `NPCsAtSpot.FirstOrDefault(n => n.ID == SelectedNpcId)` for ActiveGoalIds
- `AvailableSocialGoals.Where(g => g.PlacementNpcId == SelectedNpcId)` - goals on this NPC
- `GameWorld.Goals` - lookup NPC's ActiveGoalIds
- Token counts and effects from existing methods

**Navigation:**
- Click goal → Navigate to Goal Detail View (store Goal context)
- Click "Quick Exchange" → Launch exchange (screen switch to ScreenMode.Exchange)
- Click NPC Active Goal → Launch conversation with request (screen switch to ScreenMode.SocialChallenge)
- Back button → Return to Looking Around

---

### Notice Board View
**Purpose:** Show ambient challenges and investigations at location

**Data Displayed:**
- **Investigations Section:**
  - Discovered investigations at this location
  - Investigation intro action text and narrative
  - "Begin Investigation" button per investigation
- **Mental Challenges Section:**
  - Mental goals at this location (not on NPCs)
  - Goal name, description, exposure (difficulty)
- **Physical Challenges Section:**
  - Physical goals at this location
  - Goal name, description, danger (difficulty)

**Data Requirements:**
- `DiscoveredInvestigationsAtLocation` - investigations here
- `AvailableMentalGoals` - mental goals at location
- `AvailablePhysicalGoals` - physical goals at location
- Per goal: Name, Description, Difficulty calculation, InvestigationId if applicable

**Navigation:**
- Click investigation "Begin" → Launch investigation intro directly
- Click goal → Navigate to Goal Detail View (store Goal context)
- Back button → Return to Landing

---

### Goal Detail View
**Purpose:** Show full goal information before commitment decision

**Data Displayed:**
- Goal name (large, prominent)
- System type badge (Social/Mental/Physical)
- Full description
- Difficulty metric (Doubt Rate / Exposure / Danger based on system)
- Costs (Focus/Stamina if any)
- Commitment decision: BEGIN CHALLENGE or Cancel

**Data Requirements:**
- `SelectedGoal` - which goal to display
- `SelectedGoal.Name`, `Description`, `SystemType`
- `GetGoalDifficulty(SelectedGoal)` - calculated difficulty
- `SelectedGoal.Costs.Focus`, `SelectedGoal.Costs.Stamina` if any

**Navigation:**
- Click "BEGIN CHALLENGE" → Launch appropriate challenge:
  - Social: `StartSocialGoal(SelectedGoal)` → ScreenMode.SocialChallenge
  - Mental: `StartMentalGoal(SelectedGoal)` → ScreenMode.MentalChallenge
  - Physical: `StartPhysicalGoal(SelectedGoal)` → ScreenMode.PhysicalChallenge
- Click "Cancel" (Back button) → Return to previous view (NPC Detail or Notice Board)

---

### Spots View
**Purpose:** Show all spots in current venue for movement

**Data Displayed:**
- Grid of spots in current venue
- Per spot: Name, NPCs present at that spot, "current spot" indicator
- Move action or "You are here" indicator

**Data Requirements:**
- `AvailableSpots` - spots in current venue
- Per spot: `Id`, `Name`
- `GetNPCsAtSpot(spot.Id)` - NPCs at each spot
- `CurrentSpot?.Name` - identify current spot

**Navigation:**
- Click non-current spot → Execute `MoveToSpot(spot.Id)` (instant, free)
  - This refreshes LocationContent
  - Player stays on Landing view (movement resets navigation)
- Back button → Return to Landing

---

## Data Flow and State Management

### State Ownership

#### GameScreen Owns
- Current top-level screen (ScreenMode enum)
- Navigation stack for screen switching
- Screen context preservation (ScreenStateData)
- Resource display state (Health, Coins, etc.)
- Time display state

#### LocationContent Owns
- Current view state (Landing, LookingAround, etc.)
- Navigation history within Location (Stack<LocationViewState>)
- Selected entity context (NpcId, Goal)
- View-specific computed data (AvailableNpcs, DiscoveredInvestigations, etc.)

#### GameWorld Owns (Single Source of Truth)
- All entities (NPCs, Goals, Locations, etc.)
- All game state
- Player data
- Relationship between entities

### State Transitions

#### Entering Location Screen
```
User clicks "Back to Location" OR Challenge ends
→ GameScreen.NavigateToScreen(ScreenMode.Location)
→ LocationContent renders
→ LocationContent.OnInitializedAsync()
→ ViewState = Landing (always start at Landing)
→ Load data from GameWorld
→ Render Landing View
```

#### Navigating Within Location
```
User clicks "Look Around"
→ LocationContent.NavigateToView(LocationViewState.LookingAround)
→ Push current view state to history stack
→ Set ViewState = LookingAround
→ Query AvailableNpcs from GameWorld
→ StateHasChanged() → Render LookingAround View

User clicks NPC
→ LocationContent.NavigateToView(LocationViewState.ApproachNPC, npcId)
→ Push current view state to history stack
→ Store SelectedNpcId = npcId
→ Set ViewState = ApproachNPC
→ Query NPC data from GameWorld
→ StateHasChanged() → Render NPC Detail View

User clicks Back
→ LocationContent.NavigateBack()
→ Pop previous view state from history stack
→ Restore previous ViewState
→ Clear context if leaving that view type
→ StateHasChanged() → Render previous view
```

#### Launching Challenge
```
User clicks "BEGIN CHALLENGE" on Goal Detail
→ LocationContent determines goal system type
→ Call appropriate challenge start method:
   - StartSocialGoal() → GameScreen.StartConversationSession()
   - StartMentalGoal() → GameScreen.StartMentalSession()
   - StartPhysicalGoal() → GameScreen.StartPhysicalSession()
→ GameScreen creates challenge context
→ GameScreen.NavigateToScreen(ScreenMode.SocialChallenge/Mental/Physical)
→ Challenge screen renders
→ LocationContent navigation state preserved (but not visible)
```

#### Returning from Challenge
```
Challenge ends
→ GameScreen.HandleConversationEnd() (or Mental/Physical equivalent)
→ GameScreen.NavigateToScreen(ScreenMode.Location)
→ LocationContent renders
→ LocationContent.OnInitializedAsync() OR OnParametersSetAsync()
→ RESET navigation state:
   - ViewState = Landing
   - Clear navigation history
   - Clear selected entity context
→ Render Landing View
```

**Why reset:** Player completed a challenge, returning to high-level navigation. Don't deep-link back to where they were - clean slate.

### Context Preservation Rules

#### PRESERVE During Internal Navigation
- Which NPC is selected (ApproachNPC view)
- Which goal is selected (GoalDetail view)
- Navigation history for back button

#### RESET When Leaving Location Screen
- On challenge launch: Preserve state (hidden but intact)
- On challenge return: Reset to Landing
- On screen switch to Travel: Reset to Landing
- On spot movement: Reset to Landing (location context changed)

#### NEVER PRESERVE Across Sessions
- View state doesn't persist to save file
- Always start at Landing when entering Location screen
- Fresh navigation each time player arrives at location

---

## CSS/Styling Strategy

### Scene-Based Architecture

#### Container Classes per View
Each view gets dedicated container class in location.css:
```
.location-landing-view { }
.location-npc-list-view { }
.location-npc-detail-view { }
.location-notice-board-view { }
.location-goal-detail-view { }
.location-spots-view { }
```

#### Layout Composition, Not Component Duplication
**Reuse existing component styles:**
- `.action-card` (already exists) - for navigation choices
- `.npc-card` (already exists) - for NPC displays
- `.goal-action-btn` (already exists) - for goal displays
- `.section-header` (already exists) - for section titles
- `.back-button` (already exists in common.css) - for back navigation

**Create NEW layout styles only:**
- Grid layouts for view-specific arrangements
- Spacing/padding for scene composition
- View-specific containers
- Navigation choice layouts

**Example:**
```
Landing View uses:
  - .location-landing-view (NEW - layout container)
  - .action-card (EXISTS - for each navigation choice)
  - .section-header (EXISTS - for "What do you do?")

NPC Detail View uses:
  - .location-npc-detail-view (NEW - layout container)
  - .npc-card (EXISTS - for NPC info display)
  - .goal-action-btn (EXISTS - for goals on this NPC)
  - .back-button (EXISTS - for back navigation)
```

### Styling Principles
1. **Scene containers define layout** - How views are composed
2. **Component styles already exist** - Don't recreate cards, buttons, headers
3. **location.css owns all styles** - Scene containers and layouts live here
4. **Minimal additions** - Only add what's needed for scene composition
5. **Consistent with existing patterns** - Match parchment theme, serif fonts, warm colors

---

## Success Criteria

### What "Done" Looks Like

#### 1. Functional Requirements
✅ LocationContent has internal view state machine
✅ Landing view shows 3-5 high-level navigation choices
✅ Each sub-view focuses on ONE category (NPCs, Goals, Spots)
✅ NPC Detail view shows ONE NPC's complete information
✅ Goal Detail view shows full goal information before commitment
✅ Back button returns to previous view using history stack
✅ Navigation resets to Landing when returning from challenges
✅ All existing functionality preserved (no features lost)

#### 2. User Experience
✅ Player sees 3-5 options per screen (never 30-50)
✅ Progressive disclosure: information revealed layer by layer
✅ Clear navigation: forward into focused views, back to broader views
✅ Immersion: focused scenes feel like VN progression
✅ No confusion: purpose of each screen is immediately clear

#### 3. Technical Quality
✅ Zero changes to GameScreen or ScreenMode enum
✅ Pattern matches TravelContent (proven, working)
✅ CSS added to location.css, reuses existing component styles
✅ No code duplication
✅ All navigation state owned by LocationContent
✅ Build succeeds with zero errors
✅ Application runs without crashes

#### 4. Architectural Integrity
✅ Single source of truth: GameWorld owns entities
✅ Clear separation: GameScreen manages screen switching, LocationContent manages internal views
✅ No state duplication
✅ Context preserved correctly during navigation
✅ Reset behavior correct when returning from challenges
✅ Follows existing patterns (TravelContent precedent)

---

## Anti-Patterns to Avoid

### DO NOT DO THESE THINGS

#### 1. DO NOT Elevate to ScreenMode Level
❌ Adding ScreenMode.LocationLanding, ScreenMode.LocationLookingAround, etc.
❌ Making GameScreen aware of Location-specific navigation
❌ Polluting top-level enum with implementation details

**Why:** Violates separation of concerns. Location navigation is LocationContent's responsibility.

#### 2. DO NOT Create New Navigation System
❌ Building separate navigation infrastructure for LocationContent
❌ Duplicating GameScreen's navigation patterns at different level
❌ Creating new state management systems

**Why:** TravelContent pattern already works. Reuse it exactly.

#### 3. DO NOT Focus on CSS First
❌ Starting implementation with CSS before Razor views exist
❌ Creating CSS for components that don't exist yet
❌ Designing styles without understanding data flow

**Why:** This was my mistake in previous session. Build views with data flow first, style last.

#### 4. DO NOT Assume - Read Code
❌ Assuming navigation systems don't exist
❌ Assuming patterns aren't established
❌ Guessing at architecture without checking actual code

**Why:** Cost me an entire session making wrong assumptions. Read GameScreen.razor.cs, TravelContent.razor, etc. FIRST.

#### 5. DO NOT Create Components Prematurely
❌ Creating LocationLandingView.razor as separate component before proving pattern
❌ Over-engineering component hierarchy too early
❌ Splitting into components before understanding full view structure

**Why:** Start with inline conditional rendering (like TravelContent). Extract to components ONLY if complexity warrants.

#### 6. DO NOT Duplicate Component Styles
❌ Creating new CSS for cards/buttons/headers that already exist
❌ Recreating .action-card, .npc-card, .goal-action-btn styles
❌ Adding !important hacks to override existing styles

**Why:** Existing component styles work. Only add LAYOUT/COMPOSITION styles for scenes.

#### 7. DO NOT Lose Existing Functionality
❌ Removing any current Location features
❌ Breaking existing goal launching
❌ Breaking exchange launching
❌ Breaking investigation starts

**Why:** This is refactoring presentation, not changing features. All current actions must still work.

#### 8. DO NOT Forget Reset Behavior
❌ Preserving view state when returning from challenge
❌ Deep-linking back to NPC Detail view after conversation
❌ Maintaining navigation history across screen switches

**Why:** Clean slate on return. Always Landing view when entering LocationContent.

---

## Implementation Sequence

### Phase 1: Study Existing Patterns
1. Read TravelContent.razor completely - understand conditional rendering pattern
2. Read TravelContent.razor.cs completely - understand state management
3. Read GameScreen.razor.cs completely - understand screen switching
4. Read LocationContent.razor.cs completely - understand current data queries
5. Document existing data properties and methods used in LocationContent

### Phase 2: Design View State Machine
1. Define LocationViewState enum (Landing, LookingAround, ApproachNPC, NoticeBoard, GoalDetail, Spots)
2. Map out navigation transitions (which views lead to which)
3. Identify context needed per view (NpcId for NPC Detail, Goal for Goal Detail)
4. Design navigation methods (NavigateToView, NavigateBack, ResetNavigation)
5. Design state preservation rules (what gets stored, when it gets cleared)

### Phase 3: Implement Scene Controller Infrastructure
1. Add LocationViewState enum to LocationContentBase
2. Add navigation state properties (ViewState, NavigationStack, SelectedNpcId, SelectedGoal)
3. Add navigation methods following TravelContent pattern exactly
4. Add reset logic to OnInitializedAsync / OnParametersSetAsync
5. Test navigation methods compile and state transitions work

### Phase 4: Build Landing View
1. Create conditional block for Landing view in LocationContent.razor
2. Query data needed: check if NPCs exist, if goals exist, if spots exist
3. Render 3-5 high-level navigation options based on available data
4. Wire navigation calls to NavigateToView()
5. Test Landing view renders and navigation works

### Phase 5: Build Sub-Views One at a Time
1. Looking Around View: List NPCs, navigate to NPC Detail
2. NPC Detail View: Show one NPC's info and goals, navigate to Goal Detail or launch actions
3. Notice Board View: List investigations and ambient goals
4. Goal Detail View: Show full goal info, commitment decision
5. Spots View: List spots in venue, execute movement
6. Test each view in isolation before moving to next

### Phase 6: Add CSS for Scene Layouts
1. Add scene container classes to location.css
2. Define layouts for each view (grid arrangements, spacing)
3. Reuse existing component styles (no duplication)
4. Test visual presentation matches design intent
5. Verify styles work with actual game data (not just empty states)

### Phase 7: Integration Testing
1. Test full navigation flow: Landing → Looking Around → NPC Detail → Goal Detail → Challenge
2. Test back button at each level
3. Test returning from challenge (resets to Landing)
4. Test spot movement (resets to Landing)
5. Test all existing actions still work (exchange, investigations, goal launching)
6. Test with real game data from JSON
7. Test edge cases (no NPCs, no goals, empty location, etc.)

### Phase 8: Polish and Verification
1. Build application - verify zero errors
2. Run application - verify no crashes
3. Navigate through all views - verify UX feels right
4. Verify no functionality lost compared to original
5. Verify progressive disclosure working (3-5 options per screen)
6. Document any discovered issues or architectural decisions made during implementation

---

## Reference Implementation: TravelContent Pattern

### Key Elements to Mirror

#### State Management
```
TravelContent has:
- CurrentTravelContext (internal state determining active view)
- HasActiveTravelSession() (query method for conditional rendering)
- LoadTravelState() (initialization method)

LocationContent needs:
- CurrentViewState (enum determining active view)
- IsViewState(LocationViewState state) helper
- InitializeViewState() (reset to Landing)
```

#### Conditional Rendering
```
TravelContent pattern:
@if (HasActiveTravelSession())
{
    <TravelPathContent />
}
else
{
    <!-- Route Selection View -->
}

LocationContent pattern:
@switch (CurrentViewState)
{
    case LocationViewState.Landing:
        <!-- Landing View -->
        break;
    case LocationViewState.LookingAround:
        <!-- NPC List View -->
        break;
    // etc.
}
```

#### Navigation Actions
```
TravelContent:
- TravelRoute() launches path cards → sets CurrentTravelContext → view switches
- ReturnToLocation() clears context → returns to route selection

LocationContent:
- NavigateToView(LocationViewState.LookingAround) → pushes history, sets state
- NavigateBack() → pops history, restores previous state
- NavigateToView(LocationViewState.ApproachNPC, npcId) → stores context, sets state
```

#### Context Preservation
```
TravelContent:
- Preserves CurrentTravelContext during path cards session
- Clears context when returning to route selection

LocationContent:
- Preserves SelectedNpcId during NPC Detail view
- Preserves SelectedGoal during Goal Detail view
- Clears all context when resetting to Landing
```

---

## Final Notes

### This Is The Way Forward
This architecture:
- ✅ Solves the Everything Screen problem
- ✅ Mirrors proven VN navigation patterns
- ✅ Reuses existing GameScreen infrastructure
- ✅ Follows established TravelContent precedent
- ✅ Requires minimal changes
- ✅ Preserves all existing functionality
- ✅ Creates immersive, focused experience
- ✅ Enables progressive disclosure
- ✅ Maintains architectural integrity

### Questions Answered
- **How do VNs work?** Scene controllers with sub-scenes and progressive disclosure
- **What pattern to use?** Option 1 - Internal Sub-Screens (proven by TravelContent)
- **Where does state live?** LocationContent owns internal navigation, GameScreen owns screen switching
- **How to style?** Scene containers in location.css, reuse component styles
- **When to reset?** Always reset to Landing when entering LocationContent from outside
- **What about GameScreen?** Zero changes needed - existing navigation works perfectly

### Implementation Ready
Next session should:
1. Read this document completely
2. Study TravelContent implementation
3. Follow implementation sequence exactly
4. Mirror TravelContent pattern precisely
5. Test incrementally (view by view)
6. Add CSS last (after views work)

**DO NOT:**
- Assume anything
- Start with CSS
- Change GameScreen
- Modify ScreenMode enum
- Create new navigation infrastructure
- Duplicate component styles

**DO:**
- Follow the pattern
- Build incrementally
- Test continuously
- Ask questions if unclear
- Reference TravelContent constantly

This architecture is correct. Implementation is straightforward. Execute methodically.
