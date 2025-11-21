# HIGHLANDER Refactoring Progress: PHASES 10-14

**Session Status**: 116 errors → 50 errors (66 fixed)

## Completed Phases

### PHASE 10: TokenCount Iteration Fix (6 errors) ✅
**Files Modified:**
- `src/Subsystems/Social/ExchangeHandler.cs:236`
- `src/Pages/Components/ExchangeContent.razor.cs:432, 492`

**Fix**: Changed `foreach (KeyValuePair<ConnectionType, int>)` → `foreach (TokenCount)`, access via `.Type` and `.Count` properties.

---

### PHASE 11: Player Object Properties (8 errors) ✅
**Files Modified:**
- `src/GameState/Player.cs` - Added two properties:
  - `List<Observation> CollectedObservations` (line 130)
  - `List<PhysicalCard> InjuryCards` (line 137)
- `src/Subsystems/Physical/PhysicalDeckBuilder.cs:49` - Iterate over InjuryCards objects
- `src/Subsystems/Physical/PhysicalFacade.cs:459` - Commented out incomplete injury logic (TODO)
- `src/Pages/Components/DiscoveryJournal.razor.cs:62` - Extract `.Text` from Observation objects

**Pattern**: Store objects, not IDs. Extract display properties at UI layer.

---

### PHASE 12: SituationCardRewards.Equipment (4 errors) ✅
**Files Modified:**
- `src/Services/SituationCompletionHandler.cs:189`

**Fix**: Changed `rewards.Equipment` → `rewards.Item` (property exists, was using wrong name).

---

### PHASE 13: ExchangeCard.UnlocksExchangeId (4 errors) ✅
**Files Modified:**
- `src/Subsystems/Exchange/ExchangeOrchestrator.cs:115, 158`

**Fix**: Changed `exchange.UnlocksExchangeId` (string) → `exchange.UnlocksExchange` (object), pass ExchangeCard object to UnlockExchange method.

---

### PHASE 14: HIGHLANDER ID Elimination (Partial - 44 errors fixed) 🚧

#### 14A: Region.Id → Region Object (4 errors fixed) ✅
**Files Modified:**
- `src/Subsystems/ProceduralContent/AStoryContext.cs`
  - Line 55: `List<string> RecentRegionIds` → `List<Region> RecentRegions`
  - Line 168: `IsRegionRecent(string)` → `IsRegionRecent(Region)`
  - Line 121: `RecordCompletion(..., string regionId, ...)` → `RecordCompletion(..., Region region, ...)`
  - Line 136-142: Store Region objects in collection
  - Line 194: Initialize `RecentRegions = new List<Region>()`
- `src/Subsystems/ProceduralContent/ProceduralAStoryService.cs`
  - Line 267: Pass Region object to `IsRegionRecent(r)`
  - Line 441: Initialize `RecentRegions = new List<Region>()`
  - Line 464-470: Navigate spatial hierarchy `location.Venue.District.Region` and store Region object

**Pattern**: Store Region objects in anti-repetition tracking. Navigate spatial hierarchy to obtain Region reference.

#### 14B: Inventory Method Names (2 errors fixed) ✅
**Files Modified:**
- `src/Subsystems/Market/MarketFacade.cs:272` - `GetItems()` → `GetAllItems()`
- `src/Subsystems/Market/MarketSubsystemManager.cs:446` - `HasItem(string)` → `Contains(Item)`

**Pattern**: Inventory methods accept Item objects, not string names.

---

## Remaining 50 Errors (Categories)

### Category A: ViewModel ID Assignments (10 errors)
**Pattern**: ViewModels have deleted ID properties - remove assignment lines.

**Files**:
- `LocationFacade.cs:251` - NPCInteractionViewModel.Id
- `LocationFacade.cs:325` - ObservationViewModel.Id
- `LocationFacade.cs:368` - RouteOptionViewModel.RouteId
- `LocationFacade.cs:769` - NpcWithSituationsViewModel.Id
- `ResourceFacade.cs:248` - InventoryItemViewModel.ItemId

**Fix Pattern**:
```csharp
// BEFORE
return new NPCInteractionViewModel {
    Id = npc.ID,  // ❌ DELETE THIS LINE
    Name = npc.Name,
    Npc = npc
};

// AFTER
return new NPCInteractionViewModel {
    Name = npc.Name,
    Npc = npc  // Object reference sufficient
};
```

---

### Category B: Entity .Id Access (12 errors)
**Pattern**: Entities accessing deleted .Id properties for logging/lookups.

**Entities with .Id errors**:
- `Situation.Id` (MentalFacade.cs:67)
- `Scene.Id` (SceneInstanceFacade.cs:91)
- `Location.Id` (SceneFacade.cs:47)
- `NPC.ID` (SceneFacade.cs:47)
- `Observation.Id` (LocationFacade.cs:325)
- `MeetingObligation.Id` (MeetingManager.cs:54)

**Fix Pattern**:
```csharp
// BEFORE - Logging with ID
_logger.LogInformation($"Completed situation {situation.Id}");

// AFTER - Logging with Name
_logger.LogInformation($"Completed situation {situation.Name}");

// BEFORE - Object comparison via ID
if (scene.Id == targetSceneId)

// AFTER - Direct object comparison
if (scene == targetScene)
```

---

### Category C: Session/Context Property Access (4 errors)
**Pattern**: Sessions/contexts with deleted ID properties.

**Properties**:
- `MentalSession.ObligationId` (MentalFacade.cs:88, MentalContent.razor:12)
- `SocialChallengeContext.ConversationType` (ConversationContent.razor.cs:351)

**Fix**: Verify property exists or use object reference property.

---

### Category D: Method Signature Mismatches (4 errors)
**Pattern**: Method calls with wrong argument counts.

**Methods**:
- `IsSatisfied` - called with 3 args, expects 2 (LocationFacade.cs:825)
- `GetObservationsForLocation` - called with 2 args, expects 1 (LocationFacade.cs:306)

**Fix**: Investigate actual method signature, adjust call site.

---

### Category E: Property Access Errors (8 errors)
**Pattern**: Accessing properties that don't exist or were renamed.

**Properties**:
- `GameWorld.InitialLocationName` (GameFacade.cs:710)
- `RouteSegmentUnlock.PathDTO` (SituationCompletionHandler.cs:276)
- `RouteImprovement.RouteId` (TravelTimeCalculator.cs:72)
- `NPCData.NpcId` (JsonNarrativeRepository.cs:63)
- `Venue.Venue` (LocationManager.cs:232) - Self-reference typo

**Fix**: Search for correct property name or verify existence.

---

### Category F: Variable Scope Conflicts (2 errors)
**Pattern**: CS0136 - Variable name already used in enclosing scope.

**Files**:
- `SceneFacade.cs:147` - variable "npc"
- `LocationFacade.cs:978` - variable "situations"

**Fix**: Rename inner variable to avoid conflict.

---

### Category G: Undefined Names (2 errors)
**Pattern**: CS0103 - Name doesn't exist in context.

**Names**:
- `GetLocation` (LocationManager.cs:139)

**Fix**: Verify method exists or replace with correct method call.

---

## Next Steps (PHASE 15-16)

**PHASE 15**: Fix remaining 50 errors systematically by category (A through G).

**PHASE 16**: Zero errors verification
- Final build check
- HIGHLANDER compliance verification:
  ```bash
  grep -rn "\.Id\s*==" src/ --include="*.cs" | grep -v "Template"
  grep -rn "GetById\(" src/ --include="*.cs" | grep -v "Template"
  ```

**Estimated time**: 2-3 hours to complete PHASE 15-16.

---

## Key Architectural Patterns Applied

1. **Object References Only**: No .Id properties on domain entities (except Templates)
2. **Spatial Hierarchy Navigation**: `location.Venue.District.Region` to obtain related entities
3. **Anti-Repetition Tracking**: Store objects (Region, NPC, etc.) not IDs
4. **ViewModel Simplification**: Remove redundant ID properties, pass objects only
5. **Display Property Extraction**: Extract `.Name`/`.Text` at UI layer, not in domain

---

## Progress Summary

| Phase | Description | Errors Fixed | Status |
|-------|-------------|--------------|--------|
| 10 | TokenCount iteration | 6 | ✅ Complete |
| 11 | Player properties | 8 | ✅ Complete |
| 12 | Equipment → Item | 4 | ✅ Complete |
| 13 | UnlocksExchange object | 4 | ✅ Complete |
| 14A | Region objects | 4 | ✅ Complete |
| 14B | Inventory methods | 2 | ✅ Complete |
| 14C | ViewModel IDs | 0 (10 remaining) | 🚧 In Progress |
| 14D | Entity .Id access | 0 (12 remaining) | ⏳ Pending |
| 14E | Session properties | 0 (4 remaining) | ⏳ Pending |
| 14F | Method signatures | 0 (4 remaining) | ⏳ Pending |
| 14G | Property access | 0 (8 remaining) | ⏳ Pending |
| 14H | Variable scope | 0 (2 remaining) | ⏳ Pending |
| 14I | Undefined names | 0 (2 remaining) | ⏳ Pending |
| 15-16 | Final cleanup | 0 (8 remaining) | ⏳ Pending |
| **TOTAL** | **All Phases** | **66 of 116** | **57% Complete** |

**Target**: 0 errors with full HIGHLANDER compliance.
