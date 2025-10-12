# Obstacle System Implementation Plan

## Executive Summary

Implementing refined obstacle system with distributed interaction pattern, 3-property gating, and rich consequence types. This refactor transforms obstacles from location-bound entities into location-agnostic strategic barriers with goals scattered across the world.

**Total Estimated Time**: 5-7 hours (Phases 1+2 only)

---

## Design Decisions (Confirmed)

### 1. Containment Pattern (KEEP)
Goals live inside obstacles as children, not separate entities. One obstacle JSON contains all its goals inline.

### 2. Three Properties (REVERT)
**Remove**: StaminaCost, TimeCost (these are challenge costs, not obstacle properties)
**Keep**: PhysicalDanger, SocialDifficulty, MentalComplexity (0-3 scale)

### 3. Knowledge Cards (OPTIONAL - Phase 3)
Deferred to Phase 3. Not blocking for core functionality.

### 4. Property Reduction (SIMPLIFIED)
Not literal goal swapping. Modify goals reduce properties, unlocking goals with stricter thresholds.

### 5. Full Design Always
Complete implementation of refined design, no half-measures.

### 6. Distributed Interaction Pattern (KEY FEATURE)
One obstacle creates goals at multiple locations/NPCs. Example:
- Gatekeeper obstacle spawned by investigation
- "Pay Fee" goal appears at North Gate (PlacementLocationId: north_gate)
- "Ask About Miller" goal appears at Town Square (PlacementLocationId: town_square)
- "Get Official Pass" goal appears at Town Hall (PlacementLocationId: town_hall)
- Player discovers connections organically through exploration

---

## Phase 1: Core Architecture (CRITICAL)

**Goal**: Location-agnostic obstacles with distributed goals
**Time Estimate**: 3-4 hours

### 1.1 Domain Entity Changes

**File**: `src/GameState/Obstacle.cs`
- ⚠️ **ADD** `Id` property (CRITICAL - needed for GameWorld.Obstacles dictionary key)
- Remove `StaminaCost` property (challenge cost, not obstacle property)
- Remove `TimeCost` property (challenge cost, not obstacle property)
- Remove `LocationId` property (if exists - obstacles are location-agnostic)
- Remove `RouteId` property (if exists - obstacles are location-agnostic)
- Remove `NpcId` property (if exists - obstacles are location-agnostic)
- Keep: `PhysicalDanger`, `SocialDifficulty`, `MentalComplexity` (the 3 core properties)
- **ADD** `ObstacleState State` (Active/Resolved/Transformed)
- **ADD** `ResolutionMethod ResolutionMethod` (Violence/Diplomacy/Stealth/etc)
- **ADD** `RelationshipOutcome RelationshipOutcome` (Hostile/Neutral/Friendly/etc)
- **ADD** `string TransformedDescription` (new description after Transform)

**File**: `src/GameState/Goal.cs`
- Rename `LocationId` → `PlacementLocationId` (semantic clarity: where button appears)
- Rename `NpcId` → `PlacementNpcId` (semantic clarity: where button appears)

**File**: `src/GameState/Location.cs`
- Remove `List<Obstacle> Obstacles` property
- Add `List<string> ObstacleIds` property (references to GameWorld.Obstacles)

**File**: `src/GameState/NPC.cs`
- Remove `List<Obstacle> Obstacles` property
- Add `List<string> ObstacleIds` property (references to GameWorld.Obstacles)

**File**: `src/GameState/GameWorld.cs`
- Add `List<Obstacle> Obstacles` property (single source of truth - list, not dictionary)

### 1.2 Parser Updates

**File**: `src/Parsers/LocationParser.cs`
- Parse `ObstacleIds` instead of inline obstacles
- Remove obstacle parsing logic (obstacles parsed separately)

**File**: `src/Parsers/NPCParser.cs`
- Parse `ObstacleIds` instead of inline obstacles
- Remove obstacle parsing logic (obstacles parsed separately)

**File**: `src/Parsers/ObstacleParser.cs` (NEW or UPDATE)
- Parse obstacles into GameWorld.Obstacles list
- Parse goals with `PlacementLocationId` and `PlacementNpcId`
- Add obstacle to list (lookup by Id property when needed)

**File**: `src/Parsers/InvestigationParser.cs`
- Update to spawn obstacles in GameWorld.Obstacles
- Add obstacle IDs to appropriate Location.ObstacleIds / NPC.ObstacleIds

### 1.3 DTO Updates

**File**: `src/Parsers/DTOs/LocationDTO.cs`
- Remove `Obstacles` list
- Add `ObstacleIds` list

**File**: `src/Parsers/DTOs/NPCDTO.cs`
- Remove `Obstacles` list
- Add `ObstacleIds` list

**File**: `src/Parsers/DTOs/ObstacleDTO.cs`
- Remove `StaminaCost`
- Remove `TimeCost`
- Remove `LocationId`/`RouteId`/`NpcId` (if present)

**File**: `src/Parsers/DTOs/GoalDTO.cs`
- Rename `LocationId` → `PlacementLocationId`
- Rename `NpcId` → `PlacementNpcId`

### 1.4 Service Layer Updates

**File**: `src/Services/ObstacleGoalFilter.cs`
**Current Pattern**:
```csharp
// Gets location.Obstacles directly
public List<Goal> GetVisibleLocationGoals(Location location)
{
    visibleGoals.AddRange(location.ActiveGoals); // Ambient
    foreach (Obstacle obstacle in location.Obstacles) // Direct
    {
        visibleGoals.AddRange(GetVisibleGoalsFromObstacle(obstacle));
    }
}
```

**New Pattern**:
```csharp
public List<Goal> GetVisibleLocationGoals(Location location, GameWorld gameWorld)
{
    visibleGoals.AddRange(location.ActiveGoals); // Ambient

    // Look up obstacles by ID from list
    foreach (string obstacleId in location.ObstacleIds)
    {
        Obstacle obstacle = gameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
        if (obstacle != null)
        {
            // Filter goals where PlacementLocationId matches this location
            var locationGoals = obstacle.Goals
                .Where(g => g.PlacementLocationId == location.Id)
                .ToList();

            visibleGoals.AddRange(GetVisibleGoalsFromObstacle(obstacle, locationGoals));
        }
    }
}
```

**Similar updates for**:
- `GetVisibleNPCGoals(NPC npc, GameWorld gameWorld)`
- `GetVisibleRouteGoals(RouteOption route, GameWorld gameWorld)`

### 1.5 UI Updates

**File**: `src/Pages/Components/LocationContent.razor.cs`
**Current Pattern** (line 106-111):
```csharp
CurrentObstacles.Clear();
if (CurrentSpot != null && CurrentSpot.Obstacles != null)
{
    CurrentObstacles = CurrentSpot.Obstacles;
}
```

**New Pattern**:
```csharp
CurrentObstacles.Clear();
if (CurrentSpot != null && CurrentSpot.ObstacleIds != null)
{
    foreach (string obstacleId in CurrentSpot.ObstacleIds)
    {
        Obstacle obstacle = GameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
        if (obstacle != null)
        {
            // Only add obstacles that have goals for THIS location
            bool hasLocalGoals = obstacle.Goals
                .Any(g => g.PlacementLocationId == CurrentSpot.Id);

            if (hasLocalGoals)
            {
                CurrentObstacles.Add(obstacle);
            }
        }
    }
}
```

**Update line 170**:
```csharp
// Pass GameWorld for lookup
List<Goal> allVisibleGoals = ObstacleGoalFilter.GetVisibleLocationGoals(CurrentSpot, GameWorld);
```

**File**: `src/Pages/Components/ObstacleDisplay.razor`
- Update to handle obstacles from GameWorld.Obstacles lookup
- Filter goals by PlacementLocationId in display

### 1.6 Content Updates

**File**: `src/Content/Core/01_locations.json` (or similar)
- Change obstacle inline objects to obstacleIds string arrays
- Move obstacle definitions to separate obstacle JSON files

**Example Before**:
```json
{
  "id": "north_gate",
  "name": "North Gate",
  "obstacles": [
    {
      "name": "Suspicious Gatekeeper",
      "physicalDanger": 2,
      "socialDifficulty": 1,
      "goals": [...]
    }
  ]
}
```

**Example After**:
```json
{
  "id": "north_gate",
  "name": "North Gate",
  "obstacleIds": ["gatekeeper_obstacle"]
}
```

**New File**: `src/Content/Core/XX_obstacles.json`
```json
{
  "obstacles": [
    {
      "id": "gatekeeper_obstacle",
      "name": "Suspicious Gatekeeper",
      "physicalDanger": 2,
      "socialDifficulty": 1,
      "mentalComplexity": 0,
      "goals": [
        {
          "id": "pay_gate_fee",
          "placementLocationId": "north_gate",
          "text": "Pay Gate Fee",
          ...
        },
        {
          "id": "ask_about_miller",
          "placementLocationId": "town_square",
          "text": "Ask About Miller",
          ...
        }
      ]
    }
  ]
}
```

**Note**: JSON uses strongly-typed object with `obstacles` array, not bare array. Never use Dictionary/HashSet - strongly typed domain entities only.

---

## Phase 2: Rich Consequences (CRITICAL)

**Goal**: State tracking and 5 consequence types
**Time Estimate**: 2-3 hours

### 2.1 New Enums

**File**: `src/GameState/Enums/ObstacleState.cs` (NEW)
```csharp
public enum ObstacleState
{
    Active,      // Currently blocking
    Resolved,    // Permanently overcome, removed from play
    Transformed  // Fundamentally changed, still exists but neutral
}
```

**File**: `src/GameState/Enums/ResolutionMethod.cs` (NEW)
```csharp
public enum ResolutionMethod
{
    Unresolved,   // Not yet overcome
    Violence,     // Forced, destroyed, attacked
    Diplomacy,    // Negotiated, befriended, persuaded
    Stealth,      // Sneaked, avoided, bypassed undetected
    Authority,    // Official channels, credentials, legal power
    Cleverness,   // Outsmarted, found workaround, exploited weakness
    Preparation   // Methodical reduction over multiple attempts
}
```

**File**: `src/GameState/Enums/RelationshipOutcome.cs` (NEW)
```csharp
public enum RelationshipOutcome
{
    Hostile,    // Made enemies, damaged relationships
    Neutral,    // No relationship established or maintained
    Friendly,   // Built positive relationship
    Allied,     // Deep alliance formed, strong bond
    Obligated   // Favors owed or owing, future expectations
}
```

**File**: `src/GameState/Enums/ConsequenceType.cs` (REPLACE GoalEffectType)
```csharp
public enum ConsequenceType
{
    Resolution,  // Obstacle permanently overcome, marked Resolved
    Bypass,      // Player passes, obstacle persists for world
    Transform,   // Obstacle fundamentally changed, properties → 0
    Modify,      // Obstacle properties reduced, other goals unlock
    Grant        // Player receives knowledge/items, obstacle unchanged
}
```

### 2.2 Domain Entity Updates

**File**: `src/GameState/Obstacle.cs`
- Add `ObstacleState State { get; set; } = ObstacleState.Active;`
- Add `ResolutionMethod ResolutionMethod { get; set; } = ResolutionMethod.Unresolved;`
- Add `RelationshipOutcome RelationshipOutcome { get; set; } = RelationshipOutcome.Neutral;`
- Add `string TransformedDescription { get; set; }` (new description after Transform)

**File**: `src/GameState/Goal.cs`
- Replace `GoalEffectType EffectType` with `ConsequenceType ConsequenceType`
- Add `ResolutionMethod SetsResolutionMethod { get; set; }` (what method this sets)
- Add `RelationshipOutcome SetsRelationshipOutcome { get; set; }` (what outcome this sets)
- Add `string TransformDescription { get; set; }` (new description if Transform)

### 2.3 Consequence Handling Logic

**File**: `src/Subsystems/Mental/MentalFacade.cs` (lines 219-247)
**Current**:
```csharp
if (goal.EffectType == GoalEffectType.ReduceProperties)
{
    bool cleared = ObstacleRewardService.ApplyPropertyReduction(...);
    if (cleared && !parentObstacle.IsPermanent)
    {
        location.Obstacles.Remove(parentObstacle);
    }
}
```

**Replace with**:
```csharp
switch (goal.ConsequenceType)
{
    case ConsequenceType.Resolution:
        // Permanently overcome
        parentObstacle.State = ObstacleState.Resolved;
        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
        // Remove from active play (but keep in GameWorld for history)
        if (!parentObstacle.IsPermanent)
        {
            location.ObstacleIds.Remove(parentObstacle.Id);
        }
        break;

    case ConsequenceType.Bypass:
        // Player passes, obstacle persists
        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
        // Obstacle stays in world, player just got past it
        break;

    case ConsequenceType.Transform:
        // Fundamentally changed
        parentObstacle.State = ObstacleState.Transformed;
        parentObstacle.PhysicalDanger = 0;
        parentObstacle.SocialDifficulty = 0;
        parentObstacle.MentalComplexity = 0;
        parentObstacle.Description = goal.TransformDescription ?? parentObstacle.Description;
        parentObstacle.ResolutionMethod = goal.SetsResolutionMethod;
        parentObstacle.RelationshipOutcome = goal.SetsRelationshipOutcome;
        break;

    case ConsequenceType.Modify:
        // Properties reduced
        if (goal.PropertyReduction != null)
        {
            parentObstacle.PhysicalDanger = Math.Max(0,
                parentObstacle.PhysicalDanger - goal.PropertyReduction.PhysicalDanger);
            parentObstacle.SocialDifficulty = Math.Max(0,
                parentObstacle.SocialDifficulty - goal.PropertyReduction.SocialDifficulty);
            parentObstacle.MentalComplexity = Math.Max(0,
                parentObstacle.MentalComplexity - goal.PropertyReduction.MentalComplexity);
        }
        parentObstacle.ResolutionMethod = ResolutionMethod.Preparation;
        // Check if all properties are now 0 (fully modified)
        if (parentObstacle.PhysicalDanger == 0 &&
            parentObstacle.SocialDifficulty == 0 &&
            parentObstacle.MentalComplexity == 0)
        {
            parentObstacle.State = ObstacleState.Transformed;
        }
        break;

    case ConsequenceType.Grant:
        // Grant knowledge/items, no obstacle change
        // (Knowledge cards handled in Phase 3)
        // Items already handled by existing reward system
        break;
}
```

**Apply same pattern to**:
- `src/Subsystems/Physical/PhysicalFacade.cs`
- `src/Subsystems/Social/SocialFacade.cs`

### 2.4 UI Updates

**File**: `src/Pages/Components/ObstacleDisplay.razor`
- Display obstacle state (Active/Resolved/Transformed)
- Display resolution method (if resolved)
- Display relationship outcome (if set)
- Show transformed description if state is Transformed

---

## Phase 3: Knowledge Cards (OPTIONAL - DEFERRED)

**Goal**: Cards granted by goals, scoped to obstacles
**Time Estimate**: 3-4 hours
**Status**: Not blocking, implement later

### 3.1 Domain Entities
- `KnowledgeCard` class
- `KnowledgeCardGrant` structure in Goal
- Player knowledge tracking in GameWorld

### 3.2 Challenge Deck Augmentation
- Modify challenge deck builders to add knowledge cards
- Scope checking (obstacle-specific, universal)

### 3.3 AI Narrative Hooks
- Pass knowledge cards to AI for context
- Generate narrative reflecting player knowledge

---

## Phase 4: Templates (AI GENERATION ONLY)

**Goal**: Template system for AI obstacle generation
**Time Estimate**: 2-3 hours
**Status**: Not runtime entities, generation guidance only

### 4.1 Obstacle Archetypes
- Authority Gate (PhysicalDanger 1-2, SocialDifficulty 1-2, MentalComplexity 0-1)
- Physical Barrier (PhysicalDanger 1-3, SocialDifficulty 0, MentalComplexity 1-3)
- Hostile Entity (PhysicalDanger 2-3, SocialDifficulty 2-3, MentalComplexity 1-2)

### 4.2 Goal Templates
- Direct Confrontation (Physical, Resolution)
- Negotiate Passage (Social, Bypass)
- Build Relationship (Social, Transform)
- Gather Intelligence (Mental, Grant)
- Establish Authority (Social, Modify)
- Find Workaround (Mental, Bypass)

### 4.3 AI Generation Process
- Select archetype for narrative context
- Generate properties within ranges
- Select 3-6 goal templates
- Write contextual descriptions
- Generate knowledge card text

---

## Testing Checklist

### Phase 1 Tests
- [ ] Obstacles stored in GameWorld.Obstacles list (strongly-typed, no Dictionary/HashSet)
- [ ] Locations have ObstacleIds list (no direct Obstacles)
- [ ] NPCs have ObstacleIds list (no direct Obstacles)
- [ ] Goals have PlacementLocationId/PlacementNpcId
- [ ] ObstacleGoalFilter looks up obstacles by ID
- [ ] ObstacleGoalFilter filters goals by PlacementLocationId
- [ ] UI displays goals from distributed obstacle
- [ ] Goal at Town Square affects obstacle at North Gate
- [ ] Parsers register obstacles in GameWorld.Obstacles
- [ ] Parsers add obstacle IDs to Location.ObstacleIds
- [ ] No StaminaCost or TimeCost in obstacle properties
- [ ] Build succeeds with no compilation errors

### Phase 2 Tests
- [ ] Resolution consequence removes obstacle from location
- [ ] Bypass consequence leaves obstacle in world
- [ ] Transform consequence sets all properties to 0
- [ ] Transform consequence updates description
- [ ] Modify consequence reduces properties
- [ ] Modify consequence unlocks goals with stricter thresholds
- [ ] Grant consequence gives items (knowledge cards in Phase 3)
- [ ] ResolutionMethod set correctly for each consequence type
- [ ] RelationshipOutcome set correctly for each consequence type
- [ ] ObstacleState transitions correctly (Active → Resolved/Transformed)
- [ ] UI displays obstacle state and resolution method
- [ ] Build succeeds with no compilation errors

---

## File Change Summary

### Modified Files (Phase 1)
1. `src/GameState/Obstacle.cs` - Remove 2 properties
2. `src/GameState/Goal.cs` - Rename 2 properties
3. `src/GameState/Location.cs` - Change Obstacles → ObstacleIds
4. `src/GameState/NPC.cs` - Change Obstacles → ObstacleIds
5. `src/GameState/GameWorld.cs` - Add Obstacles dictionary
6. `src/Parsers/LocationParser.cs` - Parse ObstacleIds
7. `src/Parsers/NPCParser.cs` - Parse ObstacleIds
8. `src/Parsers/ObstacleParser.cs` - Register in GameWorld.Obstacles
9. `src/Parsers/InvestigationParser.cs` - Spawn in GameWorld.Obstacles
10. `src/Parsers/DTOs/LocationDTO.cs` - ObstacleIds list
11. `src/Parsers/DTOs/NPCDTO.cs` - ObstacleIds list
12. `src/Parsers/DTOs/ObstacleDTO.cs` - Remove 2 properties
13. `src/Parsers/DTOs/GoalDTO.cs` - Rename 2 properties
14. `src/Services/ObstacleGoalFilter.cs` - New lookup pattern
15. `src/Pages/Components/LocationContent.razor.cs` - New lookup pattern
16. `src/Pages/Components/ObstacleDisplay.razor` - Filter by PlacementLocationId

### Modified Files (Phase 2)
17. `src/Subsystems/Mental/MentalFacade.cs` - Consequence handling
18. `src/Subsystems/Physical/PhysicalFacade.cs` - Consequence handling
19. `src/Subsystems/Social/SocialFacade.cs` - Consequence handling
20. `src/Pages/Components/ObstacleDisplay.razor` - Display state

### New Files (Phase 2)
21. `src/GameState/Enums/ObstacleState.cs`
22. `src/GameState/Enums/ResolutionMethod.cs`
23. `src/GameState/Enums/RelationshipOutcome.cs`
24. `src/GameState/Enums/ConsequenceType.cs`

### Content Files (Phase 1)
25. Update all location JSON files (obstacleIds instead of inline)
26. Create obstacle JSON files (separate from locations)

---

## Risk Assessment

### Low Risk
- Property removal (StaminaCost, TimeCost) - clean deletion
- Property renames (LocationId → PlacementLocationId) - mechanical
- Enum additions - new types, no conflicts

### Medium Risk
- Storage pattern change (Obstacles → ObstacleIds) - affects many files
- Parser refactoring - must update all parsers consistently
- UI lookup pattern - must pass GameWorld correctly

### High Risk
- Distributed interaction pattern - new concept, must test thoroughly
- Consequence type handling - complex branching logic
- Content migration - must update all JSON files correctly

---

## Success Criteria

### Phase 1 Success
- ✅ Build succeeds with zero compilation errors
- ✅ Obstacles stored in GameWorld.Obstacles only
- ✅ Locations/NPCs have ObstacleIds lists only
- ✅ Goals have PlacementLocationId/PlacementNpcId
- ✅ UI displays distributed goals correctly
- ✅ Goal at one location affects obstacle visible elsewhere

### Phase 2 Success
- ✅ All 5 consequence types work correctly
- ✅ State tracking enums set properly
- ✅ Resolution removes obstacle from play
- ✅ Transform sets properties to 0
- ✅ Modify unlocks new goals
- ✅ UI displays obstacle state

---

## Implementation Order

**Phase 1** (3-4 hours):
1. Create new enum files (ConsequenceType preparation)
2. Update domain entities (Obstacle, Goal, Location, NPC, GameWorld)
3. Update DTOs (LocationDTO, NPCDTO, ObstacleDTO, GoalDTO)
4. Update parsers (LocationParser, NPCParser, ObstacleParser, InvestigationParser)
5. Update services (ObstacleGoalFilter)
6. Update UI (LocationContent.razor.cs, ObstacleDisplay.razor)
7. Build and fix compilation errors
8. Test distributed interaction pattern

**Phase 2** (2-3 hours):
1. Create state tracking enums (ObstacleState, ResolutionMethod, RelationshipOutcome)
2. Add enum properties to Obstacle and Goal
3. Implement consequence handling in facades
4. Update UI to display state
5. Build and fix compilation errors
6. Test all 5 consequence types

---

## Post-Implementation

### Immediate Next Steps
- Update existing content to use distributed pattern
- Create example obstacles with goals at multiple locations
- Test player experience with preparation mechanics

### Future Enhancements (Phase 3)
- Knowledge card system
- AI template selection
- Dynamic obstacle generation
- Narrative generation from state enums

---

## Notes

**Distributed Interaction Pattern is the KEY innovation**. This enables:
- Strategic preparation gameplay (improve situation before committing)
- Organic discovery (player finds connections through exploration)
- Verisimilitude (talk to townspeople to learn about guard's weakness)
- Emergent complexity (simple property reduction creates rich possibility space)

**State tracking enums provide AI context** without string matching:
- ResolutionMethod tells AI how obstacle was overcome (narrative generation)
- RelationshipOutcome tells AI social impact (future goal availability)
- ObstacleState tells AI current status (Active/Resolved/Transformed)

**Three properties only** because Stamina/Time are challenge costs, not obstacle properties:
- PhysicalDanger = how hard physical approaches are (intrinsic to obstacle)
- SocialDifficulty = how hard social approaches are (intrinsic to obstacle)
- MentalComplexity = how hard mental approaches are (intrinsic to obstacle)
- StaminaCost = what you spend during challenge (extrinsic, player state)
- TimeCost = what you spend during challenge (extrinsic, player state)

**Containment pattern** because obstacles are self-contained JSON objects with child goals. No separate goal entities, no complex targeting, simpler data model.
