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

### 12. Scene Auto-Cascade Bug (FIXED - 2025-11-27)

**Bug Description:** Scene 2 (a2_morning) and Scene 3 (a3_route_travel) start immediately after the previous scene completes, bypassing player navigation. Player has no agency to explore between scenes.

**Root Cause (IDENTIFIED):**
`SpawnStarterScenes()` in `GameFacade.cs:1687-1689` filtered by `Category == StoryCategory.MainStory` instead of `IsStarter == true`. This caused ALL MainStory scenes (A1, A2, A3) to be created as Deferred at game start, instead of just A1.

**Fix Applied:**
1. Added `IsStarter` property to `SceneTemplate.cs` (domain entity)
2. Added `IsStarter` property to `SceneTemplateDTO.cs` (DTO)
3. Updated `SceneTemplateParser.cs` to copy `IsStarter` from DTO
4. Changed `SpawnStarterScenes()` filter from `Category == MainStory` to `IsStarter == true`

**Verification:** Console logs now show only A1 created at game start:
```
[GameFacade] Created deferred initial scene 'a1_secure_lodging' (State=Deferred, no dependent resources yet)
```
A2 and A3 are NOT created until their ScenesToSpawn rewards fire.

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

**Correct A-Story Tutorial Flow (UPDATED 2025-11-27):**

**Scene A1 (InnLodging - 3 Situations):**
1. Game Start → A1 created as Deferred (isStarter: true)
2. Player at Common Room → "Look Around" → A1 Situation 1 activates with Elena
3. Player completes Situation 1 → ExitToWorld → must navigate to Private Room
4. Player at Private Room → A1 Situation 2 activates
5. Player completes Situation 2 → A1 Situation 3 activates
6. Player completes Situation 3 → ScenesToSpawn creates A2 as Deferred

**Scene A2 (DeliveryContract):**
7. Player navigates back to Common Room (SemiPublic + Commercial)
8. A2 activates when location filter matches
9. Player completes A2 → ScenesToSpawn creates A3 as Deferred

**Scene A3 (RouteSegmentTravel):**
10. A3 activates IMMEDIATELY (same filter as A2 - player already at Common Room)
11. A3 creates/finds route to destination
12. Player takes route travel action → A3 Situation 1 starts

**Key Validation Points:**
- A2 activates at Common Room (SemiPublic + Commercial) - filter was FIXED from "Public"
- A3 activates immediately after A2 (same filter) - filter was FIXED from "Quiet + Outdoor"
- Player navigates to Private Room during A1 (teaches spatial model)

**Files to Investigate:**
- `src/Content/GameWorldInitializer.cs` - Scene startup logic (line 52: SpawnInitialScenes)
- `src/Content/SceneInstantiator.cs` - Scene activation logic
- `src/Services/SituationCompletionHandler.cs` - Scene routing decisions
- `src/Subsystems/Consequence/RewardApplicationService.cs` - ScenesToSpawn reward application

---

## Session 2025-11-27: Playwright Session Cleanup Protocol

### 16. Console Log Pollution from Zombie Servers (DOCUMENTED - 2025-11-27)

**Issue:** Browser console shows hundreds of errors from previous sessions, making it impossible to spot real errors in the current test. Errors include:
```
WebSocket closed with status code: 1006 (no reason given)
Failed to complete negotiation with the server: TypeError: Failed to fetch
ERR_CONNECTION_REFUSED
ERR_ADDRESS_IN_USE
```

**Root Cause:** Multiple dotnet servers spawned on different ports (8100-9300) during previous Playwright sessions that either:
1. Timed out without cleanup
2. Were orphaned when Playwright session ended
3. Continue running in background after context window ran out

The browser maintains WebSocket connections and logs errors when servers become unavailable.

**Impact:** Critical - Cannot distinguish new errors from stale errors. Blocks effective testing.

**MANDATORY CLEANUP PROTOCOL FOR PLAYWRIGHT TESTS:**

Before starting ANY new Playwright test session, execute this cleanup sequence:

```bash
# Step 1: Kill all zombie dotnet processes
taskkill //F //IM dotnet.exe 2>/dev/null || true

# Step 2: Close browser to clear stale connections
mcp__playwright__playwright_close

# Step 3: Wait briefly for ports to release
sleep 2

# Step 4: Start fresh server on clean port
cd src && ASPNETCORE_URLS="http://localhost:6000" timeout 600 dotnet run --no-build

# Step 5: Navigate with fresh browser instance
mcp__playwright__playwright_navigate url=http://localhost:6000

# Step 6: Verify clean console (should have 0-2 entries only)
mcp__playwright__playwright_console_logs type=all
```

**Expected Clean Console Output:**
```
[info] Information: Normalizing '_blazor' to 'http://localhost:6000/_blazor'.
[info] Information: WebSocket connected to ws://localhost:6000/_blazor?id=...
```

**If Console Still Has Errors:** The Playwright MCP server maintains a single console log buffer that persists across browser sessions. Closing and reopening the browser does NOT clear the console log history.

**WORKAROUND - Filter by Timestamp:**
When checking console logs, note the timestamp of the successful WebSocket connection to your current port. Ignore all errors with timestamps BEFORE that connection. Only investigate errors AFTER the successful connection.

Example:
```
[info] [2025-11-27T18:45:28.901Z] Information: WebSocket connected to ws://localhost:5100/_blazor?id=...
```
Any errors before 18:45:28 are stale. Only errors AFTER 18:45:28 are relevant.

**Port Selection Strategy:**
- Use ports 5000-5999 range (safe ports, not blocked by Chrome)
- Port 6000 is BLOCKED by Chrome as "unsafe port" (ERR_UNSAFE_PORT)
- Increment by 100 for each new session
- Avoid ports 8000-9999 (heavily polluted from previous sessions)

---

**Last Updated:** 2025-11-27 18:45 UTC
**Current Phase:** A2 scene testing - critical bug found
**Issues Fixed This Session:**
- Z.Blazor.Diagrams MutationObserver error (dynamic script loading)
- CRITICAL: Procedural routes PathCards generation (HexRouteGenerator + GameWorld discovery handling)
- CRITICAL: RouteSegmentTravel arrival location resolution (PlacementProximity.RouteDestination)
- CRITICAL: Scene auto-cascade bug - SpawnStarterScenes() now filters by IsStarter (not Category)
- **CRITICAL: A2/A3 locationActivationFilter JSON fix** - A2 changed from "Public" to "SemiPublic", A3 changed to same filter as A2 (so it activates immediately at Common Room)

**Open Issues:**
- RECOMMENDED: Add validation to ensure at least one scene has IsStarter=true
- RECOMMENDED: Add validation for ScenesToSpawn reference integrity
- **CRITICAL: A2 Scene Choice Click Bug** - See Bug #14 below

---

## Session 2025-11-27: A2 Scene Testing - New Bugs Found

### 14. A2 Scene Choice Card Click Bug (INVESTIGATING - 2025-11-27)
**Issue:** Clicking scene choice cards in A2 "First Delivery" Situation 2 does NOT progress the game state. Multiple symptoms observed:

**Symptoms:**
1. **Header doesn't update:** Coins stays at 3 despite choice consequences being applied
2. **UI doesn't progress:** Same 4 choices remain visible after clicking
3. **Consequence calculations inflate:** "will have" values increase with each click
   - First view: "now 3, will have 11"
   - After clicks: "now 19, will have 27" (suggesting +8 coins applied multiple times internally)
4. **Stat-gating works correctly:** Rapport 2+ and insufficient coins paths correctly locked

**Test Path:**
1. Completed A1 (all 3 situations) with Cunning+Insight build
2. Stats after A1: Cunning=2, Insight=1, Coins=3
3. Navigate to Common Room
4. "Look Around" → "The Merchant" NPC shows "First Delivery" action
5. Click "First Delivery" → A2 Situation 1 appears (Accept/Decline)
6. Click "Accept the opportunity" → A2 Situation 2 appears with 4 contract options
7. **BUG:** Click any choice (e.g., "Politely decline") → Nothing happens, same screen persists

**Observations:**
- A2 Situation 1 correctly shows two simple choices (Accept/Not right now) with "(None)" consequences
- A2 Situation 2 correctly shows stat-gated choices with perfect information
- Choice clicking works in A1 but fails in A2 Situation 2
- Browser console shows NO errors (only normal Blazor WebSocket messages)
- Server logs don't show new activity when clicking

**Root Cause Investigation Needed:**
- Compare SceneChoiceCard click handler between working (A1) and broken (A2 Sit2) scenarios
- Check if ChoiceTemplate structure differs between A1 and A2
- Check SituationCompletionHandler for A2-specific issues
- Check if DeliveryContract archetype choice execution path is different

**Player State at Bug:**
- Location: Common Room (The Brass Bell Inn)
- Time: Evening, Day 1
- Stats: Health 5, Stamina 4/6, Focus 6/6, Coins 3, Insight 1, Cunning 2
- Active Scene: a2_morning (First Delivery)
- Current Situation: Situation 2 (contract negotiation)

### A2 Scene Flow (Working Parts)
1. ✅ A2 created as Deferred when A1 Situation 3 completes (ScenesToSpawn reward)
2. ✅ A2 activates at Common Room (SemiPublic + Commercial filter matches)
3. ✅ A2 Situation 1 displays correctly ("Accept the opportunity" / "Not right now")
4. ✅ A2 Situation 2 displays correctly with 4 stat-gated choices
5. ❌ A2 Situation 2 choice execution fails - no progression

### 15. DeliveryContract Duplicate Fallback Semantics (PARTIALLY FIXED - 2025-11-27)
**Issue:** A2 (DeliveryContract) scene has semantically identical Fallback choices across both situations:
- Situation 1 Fallback: "Not right now" (decline opportunity)
- Situation 2 Fallback: "Politely decline" (decline... after already accepting?)

**Design Problem:** Both Fallbacks mean "decline" but the player's commitment context is DIFFERENT:
- Situation 1: Player has NO obligation - can freely exit
- Situation 2: Player ACCEPTED the opportunity - backing out breaks commitment

**Correct Design (Fallback Context Rules):**

| Context | Player State | Fallback Meaning | Requirements | Consequences |
|---------|-------------|------------------|--------------|--------------|
| Pre-commitment | No obligation | "Exit, return later" | NONE | NONE |
| Post-commitment | Obligated | "Break commitment" | NONE | YES (penalty) |

**Key Principle:** Fallback must ALWAYS exist (TIER 1: No Soft-Locks) but:
- Fallback NEVER has requirements (would create soft-locks)
- Fallback CAN have consequences (preserves scarcity)

**Fix Required:**
- Situation 1 Fallback: "Not right now" → No change (correct)
- Situation 2 Fallback: "Politely decline" → "Back out of the deal" with **-1 Rapport** penalty

**Documentation Added:**
- `arc42/08_crosscutting_concepts.md` §8.16: Fallback Context Rules (No Soft-Lock Guarantee)
- `gdd/04_systems.md` §4.5: Fallback Context Rules subsection

**Files Modified:**
- `src/Content/Catalogs/SceneArchetypeCatalog.cs` - `GenerateDeliveryContract()` method, Situation 2 Fallback enrichment

**Verification Results (2025-11-27):**
- ✅ **Action text changed**: "Politely decline" → "Back out of the deal" - VERIFIED in UI
- ✅ **-1 Rapport consequence**: Now displaying correctly (FIXED 2025-11-27)
- ✅ **Situation 1 Fallback unchanged**: "Not right now" with no consequences - VERIFIED

**Root Cause (FIXED 2025-11-27):** The "no consequences" check in `SceneContent.razor` (lines 79-87) didn't include the Five Stats rewards (InsightReward, RapportReward, AuthorityReward, DiplomacyReward, CunningReward). When a choice had ONLY a stat reward (like -1 Rapport) and nothing else, it passed the "no consequences" check and displayed "(None)" instead of showing the actual Rapport consequence.

**Fix Applied:** Added Five Stats rewards to the "no consequences" condition in `SceneContent.razor`:
```razor
choice.InsightReward == 0 && choice.RapportReward == 0 && choice.AuthorityReward == 0 &&
choice.DiplomacyReward == 0 && choice.CunningReward == 0 &&
```

---

## Session 2025-11-27: Sleep Outside Environment Filter Fix

### 13. Sleep Outside Showing at Indoor Locations (FIXED - 2025-11-27)
**Issue:** "Sleep Outside" player action appeared at all locations including indoor locations like Common Room and Private Room. Should only appear at outdoor locations.

**Root Cause:** PlayerAction entity had `RequiredLocationRole` property but NO `RequiredEnvironment` property. The comment in PlayerActionCatalog.cs said "filtering happens at execution time based on Environment" but this filtering was NEVER implemented.

**Fix Applied (3 parts):**

1. **PlayerAction.cs** - Added `RequiredEnvironment` property:
   ```csharp
   /// <summary>
   /// Location environment required for this action to be available.
   /// Action only appears if location has this environment.
   /// null = available in any environment (default)
   /// </summary>
   public LocationEnvironment? RequiredEnvironment { get; set; } = null;
   ```

2. **PlayerActionCatalog.cs** - Set environment requirement for Sleep Outside:
   ```csharp
   RequiredEnvironment = LocationEnvironment.Outdoor  // Only available at outdoor locations
   ```

3. **LocationFacade.cs** - Added environment filtering in GetPlayerActions():
   ```csharp
   // Filter: Check if location has required environment for this action
   if (action.RequiredEnvironment != null)
   {
       if (spot.Environment != action.RequiredEnvironment.Value)
       {
           continue;  // Skip - location missing required environment
       }
   }
   ```

**Verification:** At Common Room (Indoor location), player actions now show:
- Look Around
- Check Belongings
- Wait
- Travel to Another Location
- **NO "Sleep Outside"** (correctly filtered)

**Technical Details:** The fix follows the existing pattern for `RequiredLocationRole` filtering, extending it to support environment-based filtering using the orthogonal `LocationEnvironment` enum (Indoor, Outdoor, Covered, Underground).

---

## Debugging Tools Reference

### Spawn Graph Visualizer (`/spawngraph`)

**What It Is:** Interactive graph visualization of all procedurally generated content. Shows scenes, situations, choices made, and entity dependencies as nodes with connections.

**Access:** Navigate to `/spawngraph` route while game is running

**Node Types:**
- **Scene nodes** - Color-coded by category (Main/Side/Service) and state (Active/Completed/Deferred)
- **Situation nodes** - Individual encounters, connected to parent scene
- **Choice nodes** - Decisions player made, shows which path was taken
- **Entity nodes** - NPCs, Locations, Routes referenced by content

**Connection Legend:**
| Line Style | Color | Meaning |
|------------|-------|---------|
| Solid | Gray | Contains (hierarchy) |
| Dashed | Green | Spawns scene |
| Dashed | Blue | Spawns situation |
| Dotted | Orange | References location |
| Dotted | Red | References NPC |
| Dotted | Brown | References route |

**Filters Available:**
- By type: Scenes, Situations, Choices, Entities
- By category: Main Story, Side Story, Service
- By state: Active, Completed, Deferred
- Search: Find nodes by name

**Key Features:**
- Click node to see detail panel with full information
- Double-click scene to zoom to its subtree
- "Fit to View" button to see entire graph
- "Refresh" button to update after game state changes
- "Back to Game" button to return to gameplay

**Debugging Use Cases:**
1. **Scene cascade verification:** Trace A1 → A2 → A3 flow via "Spawns scene" connections
2. **Activation debugging:** Check if scene is Deferred (waiting) vs Active
3. **Choice history:** See exactly which choices player made
4. **Entity assignment:** Verify which NPC/Location is assigned to which situation
5. **Missing content:** Search for expected scene/situation that isn't showing
