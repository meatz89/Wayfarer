# Obstacle System Refactoring - Remaining Tasks

## Completed Tasks (Phases 1-3)

### Phase 1: Domain Model Updates ✅
- Created `GoalEffectType` enum (None, ReduceProperties, RemoveObstacle)
- Created `ObstaclePropertyRequirements` class with `MeetsRequirements()` method
- Added `Goals` list property to `Obstacle` entity
- Updated `Goal` entity:
  - Added `EffectType` property
  - Added `PropertyRequirements` property
  - Added `PropertyReduction` property
  - **REMOVED** `TargetObstacle` property (replaced by containment)

### Phase 2: DTO and Parser Updates ✅
- Created `ObstaclePropertyRequirementsDTO`
- Updated `ObstacleDTO` with `Goals` list
- Updated `GoalDTO`:
  - Added `EffectType` property
  - Added `PropertyRequirements` property
  - Added `PropertyReduction` property
  - **REMOVED** `TargetObstacleIndex` property
- Updated `GoalParser`:
  - Parse `EffectType`, `PropertyRequirements`, `PropertyReduction`
  - **REMOVED** obstacle resolution logic (targetObstacleIndex)
  - Added `ParseEffectType()` and `ParsePropertyRequirements()` helper methods

### Phase 3: Investigation Entity Updates ✅
- **REMOVED** `GoalId` from `InvestigationPhaseDefinition`
- **REMOVED** `GoalId` from `InvestigationPhaseDTO`
- **REMOVED** goal validation from `InvestigationParser.ParsePhaseDefinition()`

---

## Remaining Tasks

### Phase 4: InvestigationActivity Refactoring (CRITICAL)

**File:** `src/Services/InvestigationActivity.cs`

**Issues:**
1. References removed `GoalId` property (lines 121, 124, 230)
2. Property name error: `RouteOptions` → should be `Routes` (line 570)
3. Goal spawning logic needs removal

**Tasks:**
1. **Remove goal spawning completely:**
   - Delete `SpawnGoalForPhase()` method (lines 118-163)
   - Delete `SpawnIntroGoalForInvestigation()` method (lines 467-505)
   - Remove goal spawning from `ActivateInvestigation()` (lines 102-108)
   - Remove goal unlocking from `CompleteGoal()` (lines 221-272)

2. **Fix RouteOptions bug:**
   - Line 570: Change `locationEntry.location.RouteOptions` → `locationEntry.location.Routes`

3. **Update CompleteGoal logic:**
   - Keep knowledge granting (lines 202-209) ✅
   - Keep obstacle spawning (lines 211-219) ✅
   - Remove goal spawning section entirely

**Expected Outcome:**
- InvestigationActivity only spawns obstacles
- Goals exist inline in location/NPC/obstacle content
- No goal ID references remain

---

### Phase 5: Facade Goal Completion Refactoring (CRITICAL)

**Files:**
- `src/Subsystems/Mental/MentalFacade.cs` (lines 205-240)
- `src/Subsystems/Physical/PhysicalFacade.cs` (lines 224-259)
- `src/Subsystems/Social/SocialFacade.cs` (lines 279-319)

**Current Problem:**
All three facades reference `Goal.TargetObstacle` which no longer exists. Goals are now children of obstacles (containment pattern).

**New Pattern:**
```csharp
if (card.CardType == CardTypes.Goal)
{
    // Find parent obstacle via containment
    Location location = _gameWorld.GetLocation(_currentSession.LocationId);
    Obstacle parentObstacle = location?.Obstacles
        .FirstOrDefault(o => o.Goals.Any(g => g.Id == card.GoalCardTemplate?.GoalId));

    if (parentObstacle != null)
    {
        Goal goal = parentObstacle.Goals.First(g => g.Id == card.GoalCardTemplate?.GoalId);

        if (goal.EffectType == GoalEffectType.ReduceProperties && goal.PropertyReduction != null)
        {
            // Preparation goal - reduce properties
            bool cleared = ObstacleRewardService.ApplyPropertyReduction(parentObstacle, goal.PropertyReduction);
            if (cleared)
            {
                location.Obstacles.Remove(parentObstacle);
            }
        }
        else if (goal.EffectType == GoalEffectType.RemoveObstacle)
        {
            // Resolution goal - remove obstacle
            location.Obstacles.Remove(parentObstacle);
        }
    }
    // Else: ambient goal with no obstacle parent

    EndSession();
    return success;
}
```

**Social Facade Variation:**
Search `NPC.Obstacles` instead of `Location.Obstacles`

---

### Phase 6: UI Code Updates (MINOR)

**Files:**
- `src/Pages/Components/DiscoveryJournal.razor.cs` (lines 164, 167)
- `src/Subsystems/Social/SocialFacade.cs` (lines 1235, 1238)

**Issue:**
References to removed `phaseDef.GoalId`

**Solution:**
Investigation phases no longer spawn goals directly. Update UI to:
- Show phase completion status
- Show obstacles spawned (not goals)
- Remove goal ID lookups

---

### Phase 7: ObstacleGoalFilter Service (NEW)

**File:** `src/Services/ObstacleGoalFilter.cs` (CREATE NEW)

**Purpose:**
Aggregate and filter goals from two sources:
1. Ambient goals (Location.Goals, NPC.Goals)
2. Obstacle-specific goals (Obstacle.Goals with property requirements)

**Methods:**
```csharp
public static List<Goal> GetVisibleLocationGoals(Location location)
public static List<Goal> GetVisibleNPCGoals(NPC npc)
public static List<Goal> GetVisibleRouteGoals(RouteOption route)
```

**Logic:**
- Add all ambient goals (always visible)
- For each obstacle, check each goal's PropertyRequirements
- If requirements met (or null), add goal to visible list

---

### Phase 8: ObstacleParser Update (CRITICAL)

**File:** `src/Content/Parsers/ObstacleParser.cs`

**Current State:**
Only parses obstacle properties, not inline goals

**Required:**
Update `ConvertDTOToObstacle()` to parse `dto.Goals`:
```csharp
if (dto.Goals != null && dto.Goals.Count > 0)
{
    foreach (GoalDTO goalDto in dto.Goals)
    {
        Goal goal = GoalParser.ConvertDTOToGoal(goalDto, gameWorld);
        obstacle.Goals.Add(goal);
    }
}
```

**Challenge:**
ObstacleParser is static, needs GameWorld reference for GoalParser.
- Add GameWorld parameter to ConvertDTOToObstacle()
- Update all callers

---

### Phase 9: UI Screen Updates (FUTURE)

**Files:**
- `src/Pages/Screens/LocationScreen.razor`
- `src/Pages/Screens/TravelScreen.razor`
- `src/Pages/Screens/ConversationScreen.razor`

**Required:**
Replace direct goal access with filtered aggregation:
```csharp
// OLD: currentLocation.ActiveGoals
// NEW: ObstacleGoalFilter.GetVisibleLocationGoals(currentLocation)
```

**Also:**
Display obstacles with properties and child goals

---

## Implementation Order

1. **Phase 8** - ObstacleParser (enables obstacle + goals parsing)
2. **Phase 4** - InvestigationActivity (remove goal spawning)
3. **Phase 5** - Three Facades (update goal completion)
4. **Phase 6** - UI Code (remove goalId references)
5. **Phase 7** - ObstacleGoalFilter (create service)
6. **Phase 9** - UI Screens (update displays)

---

## Expected Build Status After Completion

**0 Errors** - All compilation errors resolved
**Working Features:**
- Investigations spawn obstacles with inline goals
- Goals filtered by property requirements (80 Days pattern)
- Preparation goals reduce properties → unlock better options
- Resolution goals remove obstacles
- Ambient goals always available

**Architecture:**
```
Location
├─ Goals[] (ambient, always visible)
└─ Obstacles[]
    ├─ Properties (5 universal properties)
    └─ Goals[] (filtered by PropertyRequirements)

NPC
├─ Goals[] (ambient conversations)
└─ Obstacles[]
    └─ Goals[] (social challenges only)
```
