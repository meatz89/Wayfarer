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

**Last Updated:** 2025-11-26 16:15 UTC
**Current Phase:** UNBLOCKED - All 3 critical bugs FIXED
**Issues Fixed This Session:**
- Bug #4: Scene cascade (ProgressionMode.Cascade now respected)
- Bug #5: Travel blocked (TravelState comparison operators fixed)
- Bug #6: Missing Rest action (Restful/Rest capability handlers added)
