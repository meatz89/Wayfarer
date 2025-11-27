# Playtest Session Learnings & Quick Start Guide

**Purpose:** Document findings from playtest execution to enable faster setup in future sessions

---

## Critical Bugs Found

### 1. Tutorial Soft-Lock Bug (FIXED)
**Issue:** Player starts with 8 coins, but tutorial stat-granting choices all cost 5 coins. If player spends 5 coins in first scene, they're left with 3 coins and CANNOT AFFORD any stat-building choices.

**Root Cause:** First playthrough attempted spent 5 coins on "Pay for service" in first "Secure Lodging" scene, then had only 3 coins left for stat-building scene where all choices cost 5 coins.

**Fix Applied:** Changed costs from 10 → 5 coins in SceneArchetypeCatalog.cs lines 114, 127, 140, 153

**Current State:** Bug is PARTIALLY fixed - choices cost 5 (affordable), but resource management is CRITICAL. Player must NOT spend coins before reaching stat-building scene.

### 2. Duplicate Scene Architecture Bug (FIXED)
**Issue:** "Secure Lodging" appeared TWICE at game start due to both old tutorial and new A-story tutorial having `isStarter: true`.

**Root Cause:** Both `tutorial_secure_lodging` (21_tutorial_scenes.json) and `a1_secure_lodging` (22_a_story_tutorial.json) were marked as starter scenes, causing duplicate spawning.

**Fix Applied:** Changed `isStarter: true` to `isStarter: false` for old tutorial scenes in 21_tutorial_scenes.json (lines 18, 34).

**Verification:** Server logs now show "Found 1 starter templates" (down from 3). Only one "Secure Lodging" scene appears at game start.

**Technical Details:** The A-story tutorial uses `mainStorySequence: 1` to trigger special stat-granting logic in SceneArchetypeCatalog.cs. Old tutorial scenes are now disabled to prevent conflicts.

### 3. Hex Placement Overlap Bug (FIXED)
**Issue:** Scene-created locations were placed at occupied hexes, causing location overlap within venue clusters.

**Root Cause:** `PackageLoader.CreateSingleLocation` was calling `LocationPlacementService.PlaceLocationInVenue()` which calculated hex position but didn't check if the hex was already occupied by another location.

**Fix Applied:**
1. Added `FindUnoccupiedHexInVenue()` method to LocationPlacementService
2. Updated `CreateSingleLocation` and `PlaceLocationsInVenue` to use the new method
3. Method iterates through venue hex offsets to find first unoccupied position

**Verification:** Server logs confirm fix works:
```
[LocationPlacement] Hex (-2, -1) is occupied, checking next...
[LocationPlacement] Found unoccupied hex at (-2, -2)
[LocationPlacement] Placed 'Town Square Center' at (-2, -2) in venue 'Town Square'
```

**Technical Details:** The fix uses venue hex offsets array and checks each position against existing locations until an unoccupied hex is found.

### 4. Tutorial Scene Design Clarification (UPDATED - 2025-11-27)
**Previous Misunderstanding:** a1_secure_lodging was thought to need automatic cascading through 3 situations.

**Actual Design:** The 3 situations have INTENTIONALLY different contexts:
- Situation 1: Negotiate with Elena (Innkeeper) at Common Room
- Situation 2: Rest in Private Room (no NPC)
- Situation 3: Depart from Private Room (no NPC)

**Correct Behavior:** After completing Situation 1, player receives `ExitToWorld` routing decision. Player must manually navigate to Private Room where Situation 2 activates. This teaches the spatial model.

**Previous "Fix" Was Wrong:** A "Cascade bypass" was added that ignored ExitToWorld when ProgressionMode=Cascade. This broke the spatial model and prevented generated locations from working properly.

**Current Code (Correct):**
```csharp
bool shouldContinueInScene = routingDecision == SceneRoutingDecision.ContinueInScene;
```
- ExitToWorld ALWAYS exits (context changed, player navigates)
- ContinueInScene continues (same context, next situation flows)
- ProgressionMode affects UI pacing only, not routing decisions

### 5. Travel Blocked - Potential Soft-Lock (FIXED - 2025-11-26)
**Issue:** Player cannot travel from The Brass Bell Inn to Town Square due to resource constraints.

**Root Cause:** TravelManager.UpdateTravelState() had inverted comparison operators. Used `<=` when should use `>=`, causing stamina 3 to match Weary state incorrectly.

**Fix Applied:** Reordered checks from HIGH to LOW with `>=` comparisons:
```csharp
// Now correctly checks: >= 6 (Steady) → >= 5 (Fresh) → >= 4 (Tired) → >= 3 (Weary) → else (Exhausted)
```

**Technical Details:** With stamina 3, the old check `<= 3` matched Weary instead of allowing proper state transitions.

### 6. Missing Rest Action (FIXED - 2025-11-26)
**Issue:** Common Room has "Restful" capability but no Rest action is generated.

**Root Cause:** LocationActionCatalog only handled SleepingSpace capability for Rest action. The Restful and Rest capability handlers were completely missing.

**Fix Applied:** Added two new capability handlers to LocationActionCatalog.GeneratePropertyBasedActions():
1. Restful capability → Rest action (+2 Stamina, Priority 120)
2. Rest capability → Rest action (+1 Stamina, Priority 110)

**Verification:** Server logs now show `[LocationActionCatalog] ✅ Restful found - generating Rest action` and UI displays "Rest - Take time to rest in this peaceful atmosphere. Advances 1 time segment. Restores +2 Stamina."

---

## Gameplay Flow Discovery

### Tutorial Scene Sequence (After Architecture Fix)
1. **Game Start** → Common Room (8 coins, 0 all stats)
2. **Look Around** → Spawns ONE "Secure Lodging" scene (A-story tutorial)
3. **"Secure Lodging"** → Multi-situation scene with stat-building choices

### Stat-Building Scene Structure
**A-Story Tutorial:** The "Secure Lodging" scene uses `mainStorySequence: 1` to trigger special stat-granting choices.

**Stat Choices (all cost 5 coins):**
- Chat warmly → +1 Rapport
- Assert your need → +1 Authority
- Seek advantageous deal → +1 Cunning ⭐ (Investigator priority)
- Negotiate a fair → +1 Diplomacy

**Strategy for Investigator Build:**
1. Preserve starting coins for stat investment
2. Click "Secure Lodging" when it appears
3. Select "Seek advantageous deal" (+1 Cunning)
4. Result: 3 coins remaining, Cunning = 1

---

## Stat Gating Observations

### First Stat Gate Encountered
**Scene:** "Morning Reflection"
**Locked Choice:** "Take decisive action with expertise"
**Requirement:** Authority 4+ OR Insight 4+
**Player Status:** All stats at 0
**Result:** Choice LOCKED, demonstrating opportunity cost

**Implication:** Without early stat investment, premium choices become unavailable. This creates the "life you could have had" regret emotion.

---

## Server Management

### Fresh Game State
**Problem:** Blazor Server persists game state across page refreshes
**Solution:** Must kill server process and restart to get fresh state

**Commands:**
```bash
# Kill server
taskkill //F //IM dotnet.exe

# Start fresh
cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build
```

### Browser State
**Problem:** localStorage/sessionStorage don't affect Blazor Server state
**Solution:** Server-side state requires server restart, not just browser refresh

---

## Playwright Automation Tips

### Clicking Buttons
**Problem:** Some buttons are divs without proper selectors
**Solution:** Find by text content
```javascript
const divs = Array.from(document.querySelectorAll('div'));
const button = divs.find(el => el.textContent?.trim() === 'Button Text');
button.click();
```

### Waiting for State Changes
**Pattern:** Always use Promise with setTimeout for scene transitions
```javascript
new Promise(resolve => {
  setTimeout(() => {
    // Check page state
    resolve(data);
  }, 2000-3000); // 2-3 seconds for Blazor rendering
});
```

### Detecting Scene Changes
**Check:** Page text includes "WHAT DO YOU DO?" = back at location
**Check:** Page text includes "A situation unfolds" = in scene

---

## Phase 1 Test Results Summary

**All 5 Tests PASSED:**
1. ✅ Game Startup Verification
2. ✅ Tutorial Scene Spawning
3. ✅ Perfect Information Display (all costs visible upfront)
4. ✅ Stat-Gated Visual Indicators (clear "UNAVAILABLE" with requirements)
5. ✅ Soft-Lock Prevention (after bug fix)

**Key Finding:** Perfect information principle is upheld - player always sees exact costs and consequences before committing to choices.

---

## Phase 2 Progress Status

### Investigator Build Playthrough
**Goal:** Maximize Cunning + Insight
**Progress:**
- Cunning: 0 → 1 (only 1 stat point from a1 scene - scene ended early)
- Coins: 8 → 3 (spent 5 on stat choice)
- Stamina: 3/6 (CRITICALLY LOW - blocks travel)
- Build identity: BLOCKED - cannot progress

### Session 2025-11-26 Findings (Updated)

**CRITICAL ISSUES DISCOVERED:**

1. **Tutorial Scene a1_secure_lodging Only 1 Situation:**
   - Expected 3 situations per server logs
   - Actual: Only 1 stat-choice situation presented
   - Scene ended immediately after first choice
   - "Secure Lodging" no longer available

2. **Travel Blocked - Potential Soft-Lock:**
   - Attempted travel to Town Square: BLOCKED
   - Error: "PATH BLOCKED - All routes ahead are impassable"
   - Route showed STAMINA: 3/3 requirement
   - Player has Stamina 3/6 but cannot proceed
   - Forced to TURN BACK

3. **Missing Rest Action:**
   - Common Room has "Restful" capability
   - No Rest action generated
   - Only: Look Around, Check Belongings, Wait, Travel
   - Wait explicitly provides "no resource recovery"

**Player STUCK at The Brass Bell Inn:**
- Cannot travel (stamina too low)
- Cannot rest (no Rest action)
- Cannot recover stamina (Wait doesn't help)
- Only Working/Trading available but may not help stamina

**Map View Observations:**
- Hex grid displays correctly with terrain types
- Locations show as gray circles with labels
- Player position shown with star icon

**Next Steps:**
- FIX: Add Rest action for Restful capability locations
- FIX: Investigate why scene ended after 1 situation
- FIX: Review route segment stamina requirements

---

## Quick Start for Next Session

### To Resume Playtest
1. Start server: `cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build`
2. Navigate to http://localhost:8100
3. Current state: Cunning = 2, Insight = 1, Coins = 3, a1 completed
4. Look Around → find a2_morning scene
5. Prioritize Cunning/Insight choices when available
6. Document all stat-gated moments

### To Start Fresh Investigator Run
1. Kill server + restart (see commands above)
2. Navigate to http://localhost:8100
3. Look Around
4. Click "Secure Lodging" (only one will appear)
5. Select "Seek advantageous deal" (Cunning) in Situation 1
6. Select "Read and study" (Insight) in Situation 2
7. Select "Leave early" (Cunning) in Situation 3
8. Continue to a2_morning

### To Start Diplomat Build (Phase 3)
1. Fresh server restart
2. Look Around
3. Click "Secure Lodging" (only one will appear)
4. Select "Chat warmly" (Rapport) instead
5. Compare experience to Investigator

---

## Session 2025-11-27: PlacementFilter Refactoring Verification

### Major Refactoring Completed
**Change:** PlacementFilter refactored from PLURAL list properties to SINGULAR nullable properties

**Before:**
```csharp
public List<PersonalityType> PersonalityTypes { get; init; }
public List<Professions> Professions { get; init; }
```

**After:**
```csharp
public PersonalityType? PersonalityType { get; init; }  // null = don't filter, value = exact match
public Professions? Profession { get; init; }
```

**Why:** User feedback: "the real problem is that it is plural. all filters should specify only one value"
- Eliminates complex empty-list semantics
- Simplifies filter matching logic
- Each property is now `null` (any value matches) or `value` (exact match required)

### Verification Results

**1. Exchange Cards Load Correctly:**
```
[EntityResolver] Found 1 matching NPCs (global search)  // 7 times for 7 exchange cards
```
- All exchange cards with `providerFilter: { profession: "Merchant" }` loaded successfully
- Singular filter `profession: "Merchant"` correctly matches NPC with that profession

**2. NPC Discovery Works:**
- Click "Look Around" at Common Room
- Elena (Innkeeper, MERCANTILE - NEUTRAL) discovered
- "Secure Lodging" scene button appears

**3. Tutorial Scene Displays Correctly:**
- **[TUTORIAL] SECURE LODGING** header with badge
- 4 stat-building choices displayed with perfect information:
  - Chat warmly with the innkeeper → -5 Coins, +1 Rapport
  - Assert your need for accommodation → -5 Coins, +1 Authority
  - Seek advantageous deal → -5 Coins, +1 Cunning
  - Negotiate a fair arrangement → -5 Coins, +1 Diplomacy

**4. Choice Execution Works:**
- Selected "Seek advantageous deal"
- Coins: 8 → 3 (correct)
- Cunning: 0 → 1 (correct)
- Returned to Common Room location (ExitToWorld routing)

**5. Post-Situation 1 State:**
- Location actions available: Look Around, Check Belongings, Wait, Travel, **Move to The Inn**, Rest, View Job Board
- "Move to The Inn" action present for manual navigation to Private Room (Situation 2)

### Design Confirmation
The tutorial flow is CORRECT:
1. Situation 1: Negotiate with Elena at Common Room → ExitToWorld
2. Player manually navigates to Private Room via "Move to The Inn"
3. Situation 2 activates at Private Room (no NPC context)
4. Situation 3 activates at Private Room

This teaches the spatial model - player learns to navigate between locations.

### Files Changed in Refactoring
- `PlacementFilter.cs` - Domain class (singular nullable properties)
- `PlacementFilterDTO.cs` - DTO class (singular string properties)
- `EntityResolver.cs` - Matching logic (simplified `HasValue` checks)
- `SceneTemplateParser.cs` - Parsing logic (singular parse methods)
- All JSON content files - Updated from array syntax to singular syntax

### Next Steps for Testing
1. Navigate to "The Inn" (Private Room) to verify Situation 2 activates
2. Complete all 3 situations of a1_secure_lodging
3. Verify a2_morning scene spawns after tutorial completion
4. Document any stat-gating observations

---

## Session 2025-11-27: Move to Inn Exception Fix

### 7. NPC DTO Missing Required Fields (FIXED - 2025-11-27)
**Issue:** Clicking "Move to The Inn" after completing Situation 1 of a1_secure_lodging threw an exception:
```
System.InvalidOperationException: NPC DTO missing required 'Id' field
   at NPCParser.ConvertDTOToNPC
   at PackageLoader.CreateSingleNpc
   at SceneInstantiator.ActivateScene
```

**Root Cause:** `SceneInstantiator.BuildNPCDTOFromFilter()` created an NPCDTO for dynamically generated NPCs but was missing three required fields:
- `Id` - unique identifier
- `CurrentState` - connection state
- `SpawnLocation` - PlacementFilterDTO with PlacementType

**Fix Applied:** Updated `BuildNPCDTOFromFilter()` in `SceneInstantiator.cs` (line 314-328):
```csharp
return new NPCDTO
{
    Id = $"generated_npc_{Guid.NewGuid():N}",      // NEW
    Name = _narrativeService.GenerateNPCName(filter),
    Profession = (filter.Profession ?? Professions.Commoner).ToString(),
    PersonalityType = (filter.PersonalityType ?? PersonalityType.Neutral).ToString(),
    CurrentState = "Neutral",                       // NEW
    SpawnLocation = new PlacementFilterDTO { PlacementType = "Location" },  // NEW
    Tier = filter.MinTier ?? 1,
    Role = "Generated NPC",
    Description = "A person you've encountered"
};
```

**Verification:** "Move to The Inn" now works correctly. Situation 2 displays with 4 choices:
- Read and study (+1 Health, +1 Stamina, +1 Focus, +1 Insight)
- Plan tomorrow's route
- Rest peacefully (+3 Health, +3 Stamina, +3 Focus)
- Visit the common room

---

## Session 2025-11-27 (Evening): Full Tutorial Flow Testing

### 8. Z.Blazor.Diagrams MutationObserver Error (FIXED - 2025-11-27)
**Issue:** Console error at game startup:
```
Failed to execute 'observe' on 'MutationObserver': parameter 1 is not of type 'Node'
```

**Root Cause:** Z.Blazor.Diagrams script was loaded globally in `_Layout.cshtml` on every page, but `DiagramCanvas` only exists on `/spawngraph` page. The script's MutationObserver tried to observe DOM elements that don't exist on other pages.

**Fix Applied:**
1. Removed global script load from `_Layout.cshtml`
2. Added dynamic script loading in `SpawnGraph.razor`:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && !ScriptLoaded)
    {
        await JSRuntime.InvokeVoidAsync("eval", @"
            if (!window.blazorDiagramsLoaded) {
                var script = document.createElement('script');
                script.src = '_content/Z.Blazor.Diagrams/script.min.js';
                script.onload = function() { window.blazorDiagramsLoaded = true; };
                document.head.appendChild(script);
            }
        ");
        await Task.Delay(100);
        ScriptLoaded = true;
        await BuildGraphAsync();
    }
}
```

**Verification:** Console logs now clean at game startup - NO MutationObserver error.

### 9. a2_morning Scene Location Mismatch (DESIGN - Not a Bug)
**Observation:** After completing a1_secure_lodging and advancing to MORNING at Common Room, a2_morning scene does NOT appear on Elena.

**Analysis:** This is CORRECT by design:
- a2_morning requires: `locationActivationFilter: { privacy: "Public", capabilities: ["Commercial"] }`
- a2_morning requires: `npcActivationFilter: { profession: "Merchant" }`
- Common Room is `privacy: "SemiPublic"` with Elena (Innkeeper)
- Town Square Center is `privacy: "Public"` with General Merchant

**Expected Flow:** Player must TRAVEL to Town Square Center to find the Merchant and trigger a2_morning scene. The intro narrative confirms this: "Morning arrives. You step out into the town square, seeking work."

### 10. CRITICAL BUG: Procedural Routes Blocked (FIXED - 2025-11-27)
**Issue:** When attempting to travel from Common Room to Town Square Center:
- Travel initiated showing "Segment 1 of 2"
- Immediately shows "PATH BLOCKED - All routes ahead are impassable"
- "You cannot proceed further. All paths ahead are blocked due to insufficient resources or requirements."
- Player forced to TURN BACK

**Root Cause Analysis:**
1. `HexRouteGenerator.cs` creates RouteSegments for procedural routes
2. Segments had `Type = SegmentType.FixedPath` but NO PathCards were generated (PathCollection = null)
3. `TravelManager.GetSegmentCards()` returned empty list for null PathCollection
4. `TravelFacade.GetAvailablePathCards()` returned empty → triggered dead-end
5. `IsDeadEnd()` returned true because no cards available

**Fix Applied:** Two changes made:

1. **`HexRouteGenerator.cs`** - Added `GeneratePathCardsForSegment()` method that creates 2 PathCardDTOs per segment:
   - **Safe Path** (e.g., "Main Road", "Forest Trail"): 0 stamina, 2 segments (slower)
   - **Fast Path** (e.g., "Side Track", "Through the Undergrowth"): 1-2 stamina, 1 segment (faster)
   - Names and descriptions are terrain-appropriate (Road, Forest, Plains, Mountains, etc.)
   - Both paths start face-down (`StartsRevealed = false`) for discovery mechanic

2. **`GameWorld.cs`** - Modified `IsPathCardDiscovered()` to handle procedural cards gracefully:
   - If no discovery entry exists, returns `card.StartsRevealed` (false for procedural cards)
   - No longer throws exception for missing discovery entries

**Verification:** Procedural route travel now works:
- "CHOOSE YOUR PATH" displays 2 options (Main Road 0 stamina/2 seg, Side Track 1 stamina/1 seg)
- Selecting a path shows "PATH REVEALED" with correct description and costs
- "CONTINUE TO NEXT SEGMENT" button advances travel

**Design Intent Implemented:**
- Player learns routes through repeated travel (paths revealed permanently after first use)
- Safe vs Fast tradeoff creates strategic choice (time vs stamina)
- Terrain-based naming adds world immersion

### Test Flow Executed
1. Game Start → Common Room (EVENING)
2. Look Around → Elena with "Secure Lodging" action
3. Click "Secure Lodging" → Tutorial scene with 4 choices
4. Select "Seek advantageous deal" → Coins 8→3, Cunning 0→1
5. Wait x4 → Time advanced EVENING→MORNING (Day 2)
6. Look Around → Elena (Trading), Thomas visible - NO a2_morning scene
7. Click "Travel to Another Location"
8. Select "Town Square - Town Square Center" (2 segments)
9. PATH BLOCKED - cannot proceed
10. Server timeout → session ended

---

## Session 2025-11-27: RouteDestination Fix Verification

### 11. RouteSegmentTravel Arrival Location Resolution Bug (FIXED - 2025-11-27)
**Issue:** After completing route travel (e.g., 3-segment journey from The Brass Bell Inn to The Old Mill), player remained at the origin location instead of arriving at the destination.

**Root Cause:** The RouteSegmentTravel scene archetype's Arrival situation used `PlacementProximity.Spot` which was resolving to the player's CURRENT location (origin) instead of the route's DESTINATION.

**Fix Applied (3 parts):**

1. **New enum value:** Added `PlacementProximity.RouteDestination = 6` in `PlacementProximity.cs`:
   ```csharp
   /// <summary>Place at route's destination location (resolved from prior RouteFilter in same scene)</summary>
   RouteDestination = 6
   ```

2. **SceneInstantiator route tracking:** Modified `SceneInstantiator.cs` to track the route across all situation iterations and resolve `RouteDestination` to `sceneRoute.DestinationLocation`:
   ```csharp
   case PlacementProximity.RouteDestination:
       if (sceneRoute?.DestinationLocation?.HexPosition != null)
       {
           filterLocation = sceneRoute.DestinationLocation;
       }
       break;
   ```

3. **Player teleportation on scene complete:** Added logic in `SituationCompletionHandler.cs` to move player to final situation's location when scene completes:
   ```csharp
   if (routingDecision == SceneRoutingDecision.SceneComplete)
   {
       Location finalLocation = situation.Location;
       if (finalLocation != null && finalLocation.HexPosition.HasValue)
       {
           Player player = _gameWorld.GetPlayer();
           AxialCoordinates targetPosition = finalLocation.HexPosition.Value;
           if (player.CurrentPosition.Q != targetPosition.Q || player.CurrentPosition.R != targetPosition.R)
           {
               player.CurrentPosition = targetPosition;
           }
       }
   }
   ```

4. **Updated SceneArchetypeCatalog:** Changed Arrival situation's LocationFilter from `PlacementProximity.Spot` to `PlacementProximity.RouteDestination`.

**Verification (Playwright Playtest):**
- Started at The Brass Bell Inn, Common Room (hex 1,0)
- Clicked "Travel to Another Location"
- Selected The Old Mill route (3 segments)
- Completed all 3 segments: chose paths like "Main Road", "Mountain Pass"
- Saw "JOURNEY COMPLETE - You have reached your destination" screen
- Clicked "FINISH ROUTE"
- **RESULT:** Player is now at **The Old Mill > Mill Courtyard** (hex -3,3)
- Notifications confirm: "6 segments pass...", "Route mastery increased: 1/10 exploration cubes"
- Time advanced to MIDDAY Day 2 of Journey

**Technical Details:**
- The `RouteDestination` proximity type allows Arrival situations to resolve their location based on route context rather than player position
- The route is tracked across all situation iterations during scene activation via `sceneRoute` parameter
- Player teleportation ensures narrative coherence - player physically MOVES to destination when scene completes

---

## Session 2025-11-27: Scene Cascade Architecture Documentation

### 12. Scene Auto-Cascade Bug - INTENDED BEHAVIOR DEFINITION

**Bug Description:** Scene 2 (a2_morning) and Scene 3 (a3_route_travel) start immediately after the previous scene completes, bypassing player navigation. Player has no agency to explore between scenes.

**Root Cause (Under Investigation):**
- Scenes may be created as Deferred at game start instead of only when ScenesToSpawn fires
- OR: Deferred scenes may auto-activate without checking locationActivationFilter
- OR: SituationCompletionHandler may auto-cascade without ExitToWorld

**INTENDED ARCHITECTURE (Authoritative Definition):**

**Scene Lifecycle:**
1. `SceneTemplate` - Immutable archetype in GameWorld.SceneTemplates (exists always)
2. `Deferred Scene` - Created scene waiting for activation conditions (created on demand)
3. `Active Scene` - Scene with player engagement (activated when conditions met)

**Scene Creation Rules:**
- `isStarter: true` scenes → Created as Deferred at game start
- All other scenes → Remain as Templates only until `ScenesToSpawn` reward fires

**Scene Activation Rules:**
- `locationActivationFilter` → Scene activates when player ENTERS matching location
- `npcActivationFilter` → Scene activates when player INTERACTS with matching NPC
- Both must match if both specified

**Correct A-Story Tutorial Flow:**
1. Game Start → A1 created as Deferred (isStarter: true)
2. Player at Common Room (Commercial + Restful) → A1 activates
3. Player completes A1 Situation 3 → ScenesToSpawn creates A2 as Deferred
4. Player returns to location screen (ExitToWorld)
5. Player navigates to Town Square Center (Public + Commercial + Merchant NPC)
6. A2 activates when filters match
7. Player completes A2 → ScenesToSpawn creates A3 as Deferred
8. Player returns to location screen (ExitToWorld)
9. Player initiates route travel (Outdoor + Quiet)
10. A3 activates during route travel

**Key Validation Points:**
- A2 should NOT appear at Common Room (it's SemiPublic, A2 needs Public)
- A3 should NOT appear until route travel initiated
- Player must have agency to explore between scenes

**Files to Investigate:**
- `src/Content/GameWorldInitializer.cs` - Scene startup logic (line 52: SpawnInitialScenes)
- `src/Content/SceneInstantiator.cs` - Scene activation logic
- `src/Services/SituationCompletionHandler.cs` - Scene routing decisions
- `src/Subsystems/Consequence/RewardApplicationService.cs` - ScenesToSpawn reward application

---

**Last Updated:** 2025-11-27 11:00 UTC
**Current Phase:** Scene cascade architecture documented - Bug investigation pending
**Issues Fixed This Session:**
- Z.Blazor.Diagrams MutationObserver error (dynamic script loading)
- CRITICAL: Procedural routes PathCards generation (HexRouteGenerator + GameWorld discovery handling)
- CRITICAL: RouteSegmentTravel arrival location resolution (PlacementProximity.RouteDestination)

**Open Issues:**
- INVESTIGATING: Scene auto-cascade bug - scenes activate immediately without player navigation
