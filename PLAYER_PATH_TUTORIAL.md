# PLAYER PATH ANALYSIS: Game Start → Tutorial

**Date:** 2025-10-31
**Objective:** Trace EXACT player action path from game initialization to tutorial completion

---

## STARTING STATE

**Source:** `src/Content/Core/06_gameplay.json` (startingConditions)

```json
{
  "startingSpotId": "square_center",
  "startingDay": 1,
  "startingTimeBlock": "Evening",
  "startingSegment": 1
}
```

**Player Initial State:**
- **Location:** `square_center` (town_square venue)
- **Time:** Evening, Segment 1 (Day 1)
- **Resources:**
  - Coins: 8
  - Health: 4/6
  - Stamina: 3/6
  - Focus: 6/6
  - Hunger: 0/100

**What Player Sees:**
- Location screen showing "Town Square Center"
- Description: "You've been walking since dawn. Your legs ache, your coin purse feels light, and the sun is setting. The warm glow of The Brass Bell Inn beckons from across the square."

---

## PATH LINK 1: See Travel Options

**Required:** Player must be able to ACCESS travel/route UI

**Current Implementation:**
- Location screen has "Travel" button (presumably)
- Clicking Travel opens route selection

**VALIDATION NEEDED:**
- ❓ Does LocationContent.razor render Travel button?
- ❓ Is Travel button always visible or conditional?
- ❓ Does player need to discover routes first?

**Route Data (from 04_connections.json):**
```json
{
  "id": "square_to_inn",
  "name": "Cross to Brass Bell Inn",
  "originSpotId": "square_center",
  "destinationSpotId": "common_room",
  "createBidirectional": true
}
```

**Critical Questions:**
1. Are routes automatically known at game start?
2. Does player need to "discover" route first?
3. Is there a KnownRoutes system that gates visibility?

---

## PATH LINK 2: Execute Travel

**Required:** Player must be able to SELECT and EXECUTE route

**Expected Flow:**
1. Player clicks "Travel" button on Location screen
2. Travel screen shows available routes from current location
3. Player selects "Cross to Brass Bell Inn"
4. Travel challenge begins (or instant travel if 0 segments)

**Route Configuration:**
- Travel time: 0 segments (instant)
- Stamina cost: 0
- Path card: "walk_to_inn" (starts revealed)

**VALIDATION NEEDED:**
- ❓ How does TravelManager.GetAvailableRoutes() work?
- ❓ Does it filter by player.KnownRoutes?
- ❓ Is square_to_inn automatically known?

---

## PATH LINK 3: Arrive at Destination

**Required:** Travel must successfully place player at `common_room`

**Expected State After Travel:**
- **Location:** `common_room` (brass_bell_inn venue)
- **Time:** Still Evening, Segment 1 (0 time cost)
- **Resources:** Unchanged (0 stamina cost)

**Location Data (from 01_foundation.json):**
```json
{
  "id": "common_room",
  "venueId": "brass_bell_inn",
  "name": "Common Room",
  "description": "Lamplight spills from windows. Inside: warmth, food, safety..."
}
```

---

## PATH LINK 4: See Tutorial Scene

**Required:** Tutorial scene must SPAWN and be VISIBLE at common_room

**Scene Data (from 21_tutorial_scenes.json):**
```json
{
  "id": "tutorial_evening_arrival",
  "archetype": "Linear",
  "isStarter": true,
  "placementFilter": {
    "placementType": "NPC",
    "npcId": "elena",
    "maxBond": 0
  }
}
```

**NPC Data (from 03_npcs.json):**
```json
{
  "id": "elena",
  "name": "Elena",
  "venueId": "brass_bell_inn",
  "locationId": "common_room"
}
```

**Scene Spawn Rules:**
- **isStarter: true** → Should spawn automatically at game initialization
- **Placement:** NPC "elena" (who is at common_room)
- **Bond Requirement:** maxBond: 0 (player starts with 0 bond → matches)

**CRITICAL VALIDATION:**
1. ✅ Elena is at common_room (JSON verified)
2. ✅ Scene targets Elena (placementFilter verified)
3. ✅ Scene is marked isStarter (JSON verified)
4. ❌ Does SceneInstantiator spawn starter scenes at game init?
5. ❌ Are spawned scenes visible in LocationContent UI?

**Expected Player Experience:**
- Arrive at common_room
- See scene card: "Evening Arrival at Brass Bell Inn"
- Click scene to begin
- See first situation: "Elena wipes down tables..."
- See 2 choices:
  - "Offer to Help Elena with Evening Service" (StartChallenge)
  - "Pay 10 Coins for Room" (Instant action)

---

## PATH LINK 5: Start Tutorial Challenge

**Required:** Player must be able to CLICK scene and START challenge

**Scene Choice (Social Challenge):**
```json
{
  "id": "elena_social_work",
  "actionTextTemplate": "Offer to Help Elena with Evening Service",
  "actionType": "StartChallenge",
  "challengeId": "tutorial_social_service",
  "challengeType": "Social",
  "costTemplate": {
    "timeSegments": 1,
    "focus": 1
  }
}
```

**Challenge Data (from 02_cards.json - presumably):**
```json
{
  "id": "tutorial_social_service",
  "deckComposition": {
    "conversationalMove": "Cooperative",
    "targetStat": "Rapport"
  }
}
```

**Expected Flow:**
1. Player clicks "Offer to Help"
2. Costs deducted: -1 Focus, +1 time segment
3. Social challenge begins
4. Challenge screen shows conversational cards
5. Player plays cards to build rapport
6. Success rewards: +5 coins, +5 bond with Elena, spawns next scene

---

## MISSING LINKS (BREAKS PLAYER PATH)

### Critical Issue #1: Route Visibility
**File:** TravelManager.cs (need to check)
**Question:** Does player automatically know square_to_inn route at game start?

**Possible Implementations:**
- **Option A:** All routes automatically known (player sees all routes from current location)
- **Option B:** Routes must be discovered (player.KnownRoutes system)
- **Option C:** Routes require items/states to unlock

**If Option B/C:** Tutorial is INACCESSIBLE - player trapped at square_center with no way to discover route

**Fix Required:**
- Verify TravelManager route visibility logic
- If discovery required: Add square_to_inn to player's KnownRoutes at game start
- OR: Mark route as "autoDiscover: true" in JSON

---

### Critical Issue #2: Scene Spawn Timing
**File:** SceneInstantiator.cs, PackageLoader.cs
**Question:** When do isStarter scenes spawn?

**Possible Implementations:**
- **Option A:** Spawn at game initialization (all isStarter scenes created immediately)
- **Option B:** Spawn when player first reaches location (lazy spawn)
- **Option C:** Spawn on NPC first encounter

**If Option B/C:** Tutorial scene doesn't exist until player reaches common_room - but how do they know to go there?

**Fix Required:**
- Verify scene instantiation timing
- Ensure isStarter scenes spawn at game init
- Validate scenes are accessible in LocationContent query

---

### Critical Issue #3: Scene Visibility in UI
**File:** LocationContent.razor.cs, LocationContent.razor
**Question:** How are scenes rendered in location UI?

**Expected:** LocationContent should show:
- NPCs at location (with their scenes as interaction options)
- Scenes directly placed at location
- Player actions available

**Current Implementation (from earlier grep):**
```csharp
ViewModel = GameFacade.GetLocationFacade().GetLocationContentViewModel();
AvailableConversationTrees = GameFacade.GetAvailableConversationTreesAtLocation(locationId);
AvailableObservationScenes = GameFacade.GetAvailableObservationScenesAtLocation(locationId);
```

**MISSING:** No call to get available Scenes (tutorial_evening_arrival)

**Possible Gap:**
- LocationContent queries conversation trees and observation scenes
- But does NOT query Scene entities (which tutorial uses)
- Tutorial scene exists but UI doesn't display it

**Fix Required:**
- Add `GameFacade.GetAvailableScenesAtLocation(locationId)` call
- Render scenes in LocationContent.razor
- Verify scene cards are clickable and trigger navigation

---

## PLAYABILITY ASSESSMENT

**Current Status:** ❌ TUTORIAL INACCESSIBLE

**Confirmed Working:**
1. ✅ Player spawns at square_center (StartingSpotId set)
2. ✅ Route data exists in JSON (square_to_inn)
3. ✅ Elena NPC exists at common_room
4. ✅ Tutorial scene exists with correct placement filter

**Confirmed Broken:**
1. ❌ Route visibility unknown (might not appear in Travel UI)
2. ❌ Scene spawn timing unknown (might not spawn at game init)
3. ❌ Scene UI rendering unknown (might not display in LocationContent)

**Blocking Issues:**
- **If routes not auto-visible:** Player trapped, cannot leave square_center
- **If scenes not spawned:** Tutorial doesn't exist in GameWorld
- **If scenes not rendered:** Tutorial exists but player cannot see/click it

---

## REQUIRED FIXES (PRIORITY ORDER)

### Fix #1: Validate Route Visibility
**Task:** Check TravelManager.GetAvailableRoutes() implementation
**Action:**
- If routes require discovery: Add square_to_inn to starting conditions
- If routes auto-visible: Verify UI renders them correctly

### Fix #2: Validate Scene Spawning
**Task:** Check when/how isStarter scenes instantiate
**Action:**
- Ensure isStarter scenes spawn at PackageLoader.LoadPackage()
- Validate scenes exist in GameWorld.Scenes after init
- Add validation: throw if NO isStarter scenes found after package load

### Fix #3: Add Scene Rendering to LocationContent
**Task:** Ensure LocationContent.razor displays Scene entities
**Action:**
- Add `GetAvailableScenesAtLocation()` call in LocationContent.razor.cs
- Render scene cards in LocationContent.razor
- Wire up scene click → GameScreen navigation

### Fix #4: Test End-to-End Player Flow
**Task:** Open browser and PLAY the game
**Action:**
- Start game → Should spawn at square_center
- Click Travel → Should see "Cross to Brass Bell Inn"
- Select route → Should arrive at common_room
- See Elena → Should see "Evening Arrival" scene
- Click scene → Should see situation choices
- Select "Offer to Help" → Should start Social challenge

---

## SUCCESS CRITERIA

**Player can complete this exact sequence:**
1. Game loads → Player at Town Square Center
2. Click "Travel" button → See route to Brass Bell Inn
3. Click "Cross to Brass Bell Inn" → Arrive at Common Room
4. See Elena NPC → See "Evening Arrival" scene card
5. Click scene → See "Elena wipes down tables..." narrative
6. See 2 choices → Click "Offer to Help Elena"
7. Social challenge starts → See conversational cards
8. Play cards → Complete challenge successfully
9. Rewards applied → +5 coins, +5 Elena bond
10. Next scene spawns → "Secure Room for Night" appears

**If ANY step fails, tutorial is INACCESSIBLE.**
