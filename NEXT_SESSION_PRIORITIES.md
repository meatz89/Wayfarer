# Next Session Priorities - Wayfarer Core Loop Implementation

**Date Created:** 2025-10-31
**Session Goal:** Make the tutorial playable with complete daily delivery cycles

---

## Current State Summary

### What Works ✅
1. **Route System**: HexRouteGenerator is now the single source of truth for route generation
   - Bidirectional routes generated procedurally from hex pathfinding
   - Legacy 04_connections.json deleted (HIGHLANDER principle enforced)
   - 2 routes generated (common_room ↔ square_center)

2. **Starting State**: Player spawns correctly
   - Location: Common Room (The Brass Bell Inn) - CORRECT
   - Time: Evening, Day 1
   - Resources: 8 coins, 4 health, 3 stamina, 6 focus

3. **Intra-Venue Travel**: LocationActionCatalog generates instant movement actions
   - "Move to Upstairs Room" appears correctly
   - Free, instant travel within same venue

4. **Universal Actions**: All working
   - Look Around
   - Check Belongings
   - Wait
   - Sleep Outside

### Critical Bug 🔴

**Location Property-Based Actions Are Not Appearing in UI**

**Expected Behavior:**
Common Room has these properties:
- Crossroads → Should generate "Travel to Another Location" action ✅ WORKS
- Commercial → Should generate "Work for Coins" action ❌ MISSING
- Restful → Should generate "Rest" action ❌ MISSING
- Lodging → Should generate "Secure a Room (10 coins)" action ❌ MISSING

**Current Behavior:**
Only Travel action appears. Rest, Secure Room, and Work actions are missing despite LocationActionCatalog generating them at parse time.

**Why This Blocks Everything:**
The tutorial flow is: "Evening arrival at Brass Bell Inn, work for shelter"
- Player arrives evening with 8 coins
- Player MUST secure room for 10 coins (requires 2 more coins)
- Player CANNOT see "Secure Room" action
- Player CANNOT see "Work" action (would appear in Morning/Midday/Afternoon)
- Player has NO path to progression

**Tutorial becomes unplayable without this fix.**

---

## Investigation Required

### Priority 1: Diagnose Missing Location Actions Bug

**The Question:** Why are property-based actions (Rest, Work, Secure Room) not appearing when Travel action IS appearing?

**Data Flow to Trace:**
```
LocationActionCatalog.GeneratePropertyBasedActions() [Parse Time]
  → Actions stored in GameWorld.LocationActions
  → LocationActionManager.GetLocationActions() [Query Time]
  → Filter by MatchesLocation()
  → LocationFacade builds ViewModels
  → UI displays action cards
```

**Specific Investigation Points:**

1. **Are actions being generated at all?**
   - Add logging to LocationActionCatalog during package loading
   - Verify Rest/Work/Secure Room actions created for common_room
   - Check GameWorld.LocationActions count after initialization

2. **Is SourceLocationId causing rejection?**
   - Recent change added SourceLocationId check in MatchesLocation()
   - Verify SourceLocationId is correctly set on all generated actions
   - Verify MatchesLocation() checks location.Id == action.SourceLocationId

3. **Is time block filtering the issue?**
   - Rest: Available Morning/Midday/Afternoon/Evening
   - Secure Room: Available Evening ONLY
   - Work: Available Morning/Midday/Afternoon ONLY
   - Current time: Evening → Rest and Secure Room SHOULD appear

4. **Are required properties matching correctly?**
   - Common Room properties: Crossroads, Commercial, Public, Busy, Restful, Lodging
   - Rest requires: Restful ✅ (present)
   - Secure Room requires: Lodging ✅ (present)
   - Work requires: Commercial ✅ (present)
   - Property matching logic should pass for all

5. **Is LocationActionManager filtering actions out?**
   - Check GetLocationActions() implementation
   - Look for additional filters beyond MatchesLocation()
   - Verify actions reach LocationFacade

**Expected Finding:**
Most likely cause is SourceLocationId check failing. If SourceLocationId is null or mismatched, MatchesLocation() returns false and action is filtered out.

**Fix Strategy:**
Once root cause identified, ensure LocationActionCatalog sets SourceLocationId correctly on ALL generated actions, or modify MatchesLocation() to handle null SourceLocationId (global actions).

---

## Priority 2: Complete Daily Cycle Testing

**Once location actions bug is fixed, test complete daily cycle:**

### Day 1 Evening (Starting State)
- Player at Common Room, 8 coins
- "Secure Room" action visible but not affordable (needs 10 coins)
- "Sleep Outside" action available (costs 2 health, no recovery)
- Player must choose: Sleep outside OR wait until morning to work

### Day 1 → Day 2 Transition
**Test both sleep paths:**

**Path A: Sleep Outside**
- Player executes "Sleep Outside" action
- Expected: -2 Health, no resource recovery
- Expected: CurrentDay advances to 2
- Expected: TimeBlock advances to Morning
- Expected: New actions appear (Work becomes available)

**Path B: Wait Until Morning**
- Player executes "Wait" 3 times (Evening → Night → Late Night → Morning)
- Expected: Hunger increases (+5 per Wait)
- Expected: Day 2, Morning reached
- Expected: Work action appears

### Day 2 Morning/Midday/Afternoon
- "Work for Coins" action appears
- Player executes Work action
- Expected: +8 coins (total 16 coins)
- Expected: -1 stamina cost
- Expected: Time advances 1 segment
- Player can now afford room for next evening

### Day 2 Evening
- "Secure Room" action appears again
- Player has 16 coins, can afford 10 coin cost
- Player executes "Secure Room"
- Expected: -10 coins (6 remaining)
- Expected: Full health/stamina/focus recovery
- Expected: Day advances to 3, Morning
- Expected: "Well rested" feeling (full resources)

### Day 3+ Cycle Repeats
- Morning: Work for coins
- Evening: Secure room
- Sustainable daily cycle established

**Validation Criteria:**
- Day counter increments correctly
- Time block advances through full cycle
- Resources change as expected (costs/rewards applied)
- Actions appear/disappear based on time blocks
- Player can sustain infinite cycles (work → earn → sleep → recover → repeat)

---

## Priority 3: Delivery Job System (Core Loop)

**Once daily cycle works, implement the ACTUAL core game loop from design document.**

### The Core Loop Design
```
Morning: Accept delivery job from job board
  ↓
Travel to destination (route with segments/encounters)
  ↓
Complete delivery at destination location
  ↓
Receive payment (coins + possible bonus)
  ↓
Evening: Secure room or sleep outside
  ↓
Next Morning: Accept new job → Repeat
```

### What Needs Implementation

**1. Job Board System**
- Job board entity/data structure
- Available jobs list (dynamically generated or from templates)
- Job acceptance flow (player selects job, becomes active)

**2. Delivery Job Entity**
- Origin location (where job accepted)
- Destination location (where delivery must be completed)
- Cargo description (narrative flavor)
- Base payment (coins)
- Deadline (days remaining)
- Danger level (affects route difficulty)

**3. Active Job Tracking**
- Player can have ONE active delivery job
- Active job displayed in UI (destination, payment, deadline)
- Active job persists through travel/rest
- Cannot accept new job while one is active

**4. Job Completion Mechanics**
- Player arrives at destination location
- "Complete Delivery" action appears
- Player executes action
- Rewards granted (coins, possible XP/reputation)
- Active job cleared
- Player can accept new job

**5. Job Board Location Action**
- New LocationActionType: AcceptDeliveryJob
- Appears at locations with "Commercial" property
- Only available if player has NO active job
- Opens job board UI or directly accepts random job

**6. Economic Balance**
- Job payment must cover: Travel costs + Room cost + Profit margin
- Example: Delivery pays 15 coins, route costs 3 coins, room costs 10 coins = 2 coin profit
- Ensure jobs are always available (sustainable loop)
- Vary job difficulty/payment (short cheap routes vs. long dangerous routes)

### Why This Is The Core Loop

**Without delivery jobs, the game has no structure:**
- Work action is generic (just earn coins)
- No goal-oriented travel (player wanders aimlessly)
- No risk/reward decision making (safe cheap jobs vs. dangerous high-pay jobs)
- No progression sense (just infinite work → sleep cycles)

**With delivery jobs, the game has PURPOSE:**
- Player has clear goal each cycle (deliver to X)
- Travel becomes meaningful (getting to destination efficiently)
- Economic decisions matter (accept high-pay dangerous route vs. safe low-pay route)
- Progression emerges (unlock new routes, better jobs, reputation)

---

## Priority 4: Expand World Content (If Time Permits)

**Once delivery loop works, expand to test route variety and economic balance.**

### Additional Locations Needed
Current state: 4 locations (2 venues)
- brass_bell_inn: common_room, upstairs_room
- town_square: square_center, fountain_plaza

**Minimum viable world:**
- 3-4 venues (towns/settlements)
- 2-3 locations per venue
- Routes connecting all venues
- Variety of route lengths/difficulties

**Why:** Test that delivery jobs can route to multiple destinations, routes have varied costs/dangers, economic balance holds across different route types.

### Hex Grid Expansion
- Add more hexes to 02_hex_grid.json
- Assign hex positions to new locations
- HexRouteGenerator will automatically create routes
- Test that procedural generation scales to larger world

---

## Architecture Context for Next Session

### Core Principles to Remember

**1. HIGHLANDER: One Concept, One Representation**
- HexRouteGenerator is ONLY source for routes
- Do NOT create parallel route systems
- Do NOT create JSON routes alongside procedural routes
- All routes come from hex pathfinding

**2. Parse-Time vs. Runtime**
- LocationActionCatalog generates actions at PARSE TIME
- Actions stored in GameWorld.LocationActions
- Runtime QUERIES GameWorld, never generates
- LocationActionManager filters actions based on current context

**3. Property-Based Action Generation**
- Actions tied to location properties (Crossroads, Commercial, Restful, Lodging)
- Same property → same action type across all locations
- Properties define availability, not specific location IDs
- SourceLocationId binds action to specific location instance

**4. Perfect Information**
- Player sees ALL costs before executing action
- No hidden consequences
- Action description explains what will happen
- Deterministic outcomes (same action always produces same result)

**5. Fail Fast**
- Missing required content should THROW exceptions at initialization
- Silent failures waste debugging time
- If player can't progress, game should crash at startup with clear error
- Better to fail during development than ship broken content

### Data Flow Reference

**Action Appearance Flow:**
```
Package Loading:
  LocationParser reads location JSON
    → LocationActionCatalog.GeneratePropertyBasedActions(location)
    → Actions added to GameWorld.LocationActions

Player Navigation:
  Player at location X, time block Y
    → LocationActionManager.GetLocationActions(location, timeBlock)
    → Filter: action.MatchesLocation(location, timeBlock)
    → LocationFacade builds ViewModels
    → UI renders action cards
```

**Sleep/Day Advancement Flow (Needs Verification):**
```
Player executes Sleep action:
  → GameFacade.ExecutePlayerAction(PlayerActionType.SecureRoom/SleepOutside)
    → ResourceFacade applies costs/rewards
    → ??? Does TimeModel.AdvanceDay() get called?
    → ??? Does CurrentDay increment?
    → ??? Does TimeBlock reset to Morning?
```

This flow is UNKNOWN and needs investigation. Sleep actions exist but day advancement mechanics unclear.

---

## Success Criteria

**Minimum Viable Tutorial:**
1. Player spawns at Common Room, Evening, 8 coins ✅
2. Player can see "Secure Room" action (currently BLOCKED by bug)
3. Player cannot afford room yet (8 coins, needs 10)
4. Player sleeps outside (-2 health) or waits until morning
5. Day advances to Day 2, Morning
6. "Work" action appears
7. Player works, earns 8 coins (16 total)
8. Evening arrives, player secures room for 10 coins
9. Day advances to Day 3, Morning, player fully recovered
10. Cycle repeats sustainably (work → earn → sleep → recover)

**Stretch Goal:**
11. Job board action appears at Commercial locations
12. Player accepts delivery job (destination + payment shown)
13. Player travels to destination using generated route
14. Player completes delivery, earns payment
15. Player can sustain delivery job loop indefinitely

---

## Technical Debt / Future Work

**Not priorities for this session, but document for later:**

1. **Intra-Venue Movement UI**
   - "Move to Another Spot" parent card is confusing
   - Should just show "Move to X" actions directly
   - Current implementation works but UX could be clearer

2. **Route Narrative Descriptions**
   - HexRouteGenerator creates NarrativeDescription from terrain
   - Currently generic ("Plains terrain, moderate danger")
   - Could be enhanced with procedural narrative generation
   - Not critical for POC, but improves immersion

3. **Scene Spawning on Routes**
   - HexRouteGenerator.SpawnEncounterScenes() exists
   - Creates Scenes for Encounter segments
   - Needs SceneTemplates with PlacementType.Route to actually spawn
   - Currently no route scene templates exist
   - Routes work without scenes (just segment descriptions)

4. **PathCard System**
   - Legacy PathCollection/PathCard system still in codebase
   - HexRouteGenerator doesn't use it (self-sufficient)
   - Could be deleted if confirmed unused elsewhere
   - Or could be enhanced to support FixedPath segments

5. **Economic Tuning**
   - Work pays 8 coins
   - Room costs 10 coins
   - Routes cost stamina (varies)
   - Balance needs testing with full delivery loop
   - May need job payment adjustments

---

## Key Files Reference

**For Bug Investigation:**
- `src/Content/Catalogs/LocationActionCatalog.cs` - Generates actions
- `src/GameState/LocationAction.cs` - MatchesLocation() logic
- `src/Subsystems/Location/LocationActionManager.cs` - Filters actions for UI
- `src/Subsystems/Location/LocationFacade.cs` - Builds ViewModels

**For Day Advancement:**
- `src/Services/GameFacade.cs` - ExecutePlayerAction() handlers
- `src/Services/ResourceFacade.cs` - Resource costs/rewards
- `src/Subsystems/Time/TimeModel.cs` - Day/time advancement

**For Delivery Jobs (Future):**
- Need to create: `src/GameState/DeliveryJob.cs`
- Need to modify: `src/GameState/Player.cs` (add ActiveDeliveryJob)
- Need to create: Job board UI component
- Need to create: Job completion handler in GameFacade

**Content Files:**
- `src/Content/Core/01_foundation.json` - Locations
- `src/Content/Core/02_hex_grid.json` - Hex positions
- `src/Content/Core/06_gameplay.json` - Starting conditions

---

## Session Start Checklist

When starting next session:
1. ✅ Verify build compiles (should be clean)
2. ✅ Start server and navigate to game
3. ✅ Verify player at Common Room with 8 coins
4. 🔴 Check console logs for action generation during package loading
5. 🔴 Confirm only Travel and Move actions appear (bug still present)
6. 🔴 Begin investigation of missing actions bug

**Expected first investigation step:**
Add logging to LocationActionCatalog.GeneratePropertyBasedActions() to confirm actions ARE being generated for common_room with Restful/Lodging properties. This will narrow down whether bug is in generation or filtering.

---

**END OF DOCUMENT**

This document should provide complete context for the next session. Focus is on fixing the blocking bug, then testing daily cycle, then implementing core delivery loop.
