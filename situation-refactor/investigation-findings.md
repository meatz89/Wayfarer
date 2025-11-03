# SCENE/SITUATION REFACTORING - INVESTIGATION FINDINGS

## DATE: 2025-11-02

## INVESTIGATION MANDATE

Comprehensive codebase analysis to verify assumptions in architectural-reasoning.md before implementation. Applied HIGHLANDER principle: check for existing code with same responsibility before writing duplicates.

---

## CRITICAL FINDING #1: PLACEMENT DUPLICATION (CONFIRMED ARCHITECTURAL VIOLATION)

### THE VIOLATION

**Scene ALREADY has placement:**
- `PlacementType` (enum: Location/NPC/Route)
- `PlacementId` (string: concrete entity ID)
- Pattern C (ID only, GameWorld lookup on demand)

**Situation DUPLICATES placement:**
- `PlacementLocation` (Location object)
- `PlacementNpc` (NPC object)
- `PlacementRouteId` (string)
- `ParentScene` (Scene object)

**This violates HIGHLANDER Pattern A:** Scene should be single source of truth for placement. Situation should QUERY Scene, not STORE duplicate.

### IMPACT RADIUS (9 Files Will Break)

**WRITES (Set placement on Situation):**
1. `SituationParser.cs` (lines 89-93) - Resolves PlacementLocationId/PlacementNpcId from DTO
2. `SceneInstantiator.cs` (lines 237-241) - Inherits placement from Scene context
3. `SpawnFacade.cs` (lines 263-314) - Spawns situations with placement from parent
4. `PackageLoader.cs` (lines 736-753) - Adds situation IDs to entity ActiveSituationIds

**READS (Query placement from Situation):**
5. `GameFacade.cs` (lines 1506-1508, 1521-1523) - Filters situations by placement
6. `SituationCompletionHandler.cs` (lines 136-274) - Grants cubes to placement entities
7. `RewardApplicationService.cs` (lines 174-188) - Builds SceneSpawnContext from placement
8. `DifficultyCalculationService.cs` (lines 72-80) - Checks familiarity/tokens at placement
9. `SituationFacade.cs` (lines 156-195) - Passes placement ID to Intent constructors

### EXISTING CORRECT PATTERN (Scene-Based)

**SceneFacade.IsSceneAtLocation()** (lines 63-101):
- ✅ Queries `scene.PlacementType` and `scene.PlacementId`
- ✅ For NPC placement: Resolves NPC, checks NPC.Location
- ✅ For Route placement: Resolves Route, checks Route origin
- **THIS IS THE CANONICAL PATTERN ALL CODE SHOULD FOLLOW**

**LocationFacade.FindParentScene()** (lines 975-983):
- ✅ Queries `scene.PlacementType == PlacementType.Location`
- ✅ Queries `scene.PlacementId == spot.Id`
- **ALREADY USES SCENE AS SOURCE OF TRUTH**

### REFACTORING STRATEGY

**DELETE from Situation:**
- `PlacementLocation` (object)
- `PlacementNpc` (object)
- `PlacementRouteId` (string)

**KEEP on Situation:**
- `ParentScene` (object) - Navigation reference for inherited context

**ADD to Situation:**
```csharp
public string GetPlacementId(PlacementType placementType)
{
    return ParentScene?.PlacementType == placementType
        ? ParentScene.PlacementId
        : null;
}
```

**REFACTOR ALL 9 FILES:**
- Change `situation.PlacementLocation?.Id` to `situation.GetPlacementId(PlacementType.Location)`
- Change `situation.PlacementNpc?.ID` to `situation.GetPlacementId(PlacementType.NPC)`
- Change `situation.PlacementRouteId` to `situation.GetPlacementId(PlacementType.Route)`

---

## CRITICAL FINDING #2: STATUS REDUNDANCY (CONFIRMED ARCHITECTURAL VIOLATION)

### THE VIOLATION

**THREE PARALLEL STATUS SYSTEMS:**
1. `Status` (SituationStatus enum) - 5 states: Dormant/Available/Active/Completed/Failed
2. `IsAvailable` (bool) - Availability flag
3. `IsCompleted` (bool) - Completion flag

**DESYNC VULNERABILITY FOUND:**
- `SituationFacade.cs` line 105: Sets `Status = SituationStatus.Active` WITHOUT updating `IsAvailable`
- `IsAvailableToAttempt()` line 331: Checks BOTH `Status == Available AND IsAvailable` (redundant if synchronized)

### CURRENT USAGE PATTERNS

**WRITES (6 locations):**
1. `Situation.cs:339-340` - Complete() method sets BOTH Status AND IsCompleted
2. `SituationFacade.cs:105` - Sets Status ONLY (desync risk!)
3. `SituationFacade.cs:133, 210` - Calls Complete() (dual write)
4. `SpawnFacade.cs:129-130, 148` - Sets all three together
5. `SituationParser.cs:51-52` - Copies from DTO

**READS (4 locations):**
1. `Situation.cs:331` - IsAvailableToAttempt() checks both Status AND IsAvailable
2. `LocationFacade.cs:802, 933` - Queries use ONLY boolean flags (ignores Status enum!)
3. `SituationCompletionHandler.cs:298` - Checks IsCompleted for obligation progress

**INCONSISTENCY:** LocationFacade uses boolean flags exclusively, IsAvailableToAttempt() uses both. No single source of truth.

### DTO/JSON LAYER

**SituationDTO (lines 92, 97):**
- HAS: `IsAvailable`, `IsCompleted`
- MISSING: `Status` enum property

**ANALYSIS:** JSON contains initial boolean values, Status is runtime-only. But this creates initialization inconsistency.

### REFACTORING STRATEGY

**DELETE from Situation:**
- `public bool IsAvailable { get; set; }`
- `public bool IsCompleted { get; set; }`

**REPLACE with computed properties:**
```csharp
public bool IsAvailable => Status == SituationStatus.Available || Status == SituationStatus.Dormant;
public bool IsCompleted => Status == SituationStatus.Completed || Status == SituationStatus.Failed;
```

**DELETE from SituationDTO:**
- `IsAvailable` property
- `IsCompleted` property

**REFACTOR ALL 4 READ LOCATIONS:**
- LocationFacade queries: `g.Status == SituationStatus.Available` instead of `g.IsAvailable && !g.IsCompleted`
- SituationCompletionHandler: `g.Status == SituationStatus.Completed` instead of `g.IsCompleted`
- IsAvailableToAttempt(): `Status == SituationStatus.Available` only (remove boolean check)

**FIX DESYNC:**
- Line 105 in SituationFacade: Status=Active is correct, IsAvailable computed property will handle it

---

## FINDING #3: SPAWN TRACKING (NO DUPLICATION - BUT OPTIMIZATION OPPORTUNITY)

### CURRENT STATE

**ONLY Situation has spawn tracking:**
- `SpawnedDay`, `SpawnedTimeBlock`, `SpawnedSegment` (when created)
- `CompletedDay`, `CompletedTimeBlock`, `CompletedSegment` (when finished)

**NO OTHER ENTITIES have these properties** (Scene, Obligation, Challenges don't have spawn tracking)

### USAGE ANALYSIS

**WRITES (3 locations):**
1. `SituationParser.cs:65-70` - Copies from JSON DTO
2. `SpawnFacade.cs:153-155` - Sets spawn timestamps
3. `SituationFacade.cs:135-137, 212-214` - Sets completion timestamps

**READS:**
- **ZERO QUERIES FOUND** - Properties are WRITE-ONLY (dead data!)
- No expiration logic
- No analytics queries
- No filtering/sorting by spawn time

### ARCHITECTURAL PLAN (from architectural-reasoning.md)

**Scene expiration REQUIRES SpawnedDay:**
```
SceneTemplate.ExpirationDays (metadata) - ALREADY EXISTS
Scene.SpawnedDay (when created) - NOT IMPLEMENTED YET
```

When Scene expiration is implemented, BOTH Scene and Situation will need SpawnedDay → Duplication risk.

### REFACTORING STRATEGY

**CREATE SpawnTracking shared record:**
```csharp
public record SpawnTracking
{
    public int? SpawnedDay { get; init; }
    public TimeBlocks? SpawnedTimeBlock { get; init; }
    public int? SpawnedSegment { get; init; }
    public int? CompletedDay { get; set; }
    public TimeBlocks? CompletedTimeBlock { get; set; }
    public int? CompletedSegment { get; set; }
}
```

**REPLACE 6 properties on Situation with:**
```csharp
public SpawnTracking Lifecycle { get; set; } = new SpawnTracking();
```

**BENEFIT:**
- HIGHLANDER compliance (one definition of spawn tracking)
- Reusable when Scene needs SpawnedDay for expiration
- Semantic grouping (Lifecycle.SpawnedDay makes intent clear)
- Future-proof for other entities needing spawn tracking

**UPDATE 3 WRITE LOCATIONS:**
- SituationParser: `situation.Lifecycle = new SpawnTracking { SpawnedDay = dto.SpawnedDay, ... }`
- SpawnFacade: `Lifecycle = new SpawnTracking { SpawnedDay = currentDay, ... }`
- SituationFacade: `situation.Lifecycle.CompletedDay = currentDay`

---

## FINDING #4: REQUIREMENTS/CONSEQUENCES (ABSTRACTIONS ALREADY EXIST - NO EXTRACTION NEEDED)

### DISCOVERY: COMPLETE SHARED SYSTEM EXISTS

**CompoundRequirement class:**
- Shared by: Situation, ChoiceTemplate, LocationAction, NPCAction, PathCard (5+ entities)
- Service: `RequirementParser.ConvertDTOToCompoundRequirement()`
- Validation: `CompoundRequirement.IsAnySatisfied(player, gameWorld)`
- **ALREADY ARCHITECTED CORRECTLY**

**Consequence classes:**
- `BondChange`, `ScaleShift`, `StateApplication`
- Shared by: Situation (ProjectedX), ChoiceReward (X), all Actions via ChoiceTemplate
- Service: `ConsequenceFacade.ApplyConsequences()`
- **ALREADY CENTRALIZED**

**Cost classes:**
- `SituationCosts` (strategic layer - Situation directly)
- `ChoiceCost` (tactical layer - ChoiceTemplate)
- **INTENTIONALLY DIFFERENT** (strategic vs tactical information revelation)

### WHY NO EXTRACTION NEEDED

**REASON 1:** Abstractions already exist and are shared
- CompoundRequirement used across 5+ entity types
- BondChange/ScaleShift/StateApplication used across 2+ entity types
- ConsequenceFacade and RequirementParser centralize logic

**REASON 2:** Property placement is intentional architecture
- Situation has DIRECT properties (Pattern B - domain entity)
- Actions have COMPOSED properties via ChoiceTemplate (Pattern A - runtime ephemeral)
- Different patterns serve different purposes

**REASON 3:** Semantic differences matter
- SituationCosts ≠ ChoiceCost (strategic vs tactical)
- ProjectedBondChanges ≠ BondChanges (transparent vs hidden)
- Both serve distinct information revelation patterns (Sir Brante board game transparency)

### CONCLUSION: USE EXISTING SYSTEM

**DO NOT create new abstractions:**
- ❌ SituationRequirements class (CompoundRequirement already exists)
- ❌ SituationConsequence class (ConsequenceFacade already exists)
- ❌ Merge SituationCosts and ChoiceCost (intentionally different granularities)

**DO use existing services:**
- ✅ RequirementParser for DTO conversion
- ✅ ConsequenceFacade for consequence application
- ✅ CompoundRequirement.IsAnySatisfied() for validation

---

## REVISED IMPLEMENTATION PLAN

### PHASE 0.1: SpawnTracking Extraction

**Create:** `src/GameState/SpawnTracking.cs` (shared record)

**Refactor:** `Situation.cs` (replace 6 properties with Lifecycle)

**Update:** SituationParser, SpawnFacade, SituationFacade (3 write locations)

**Impact:** None (internal refactoring, no external API changes)

---

### PHASE 0.2: Placement Deduplication (CRITICAL)

**Delete:** PlacementLocation, PlacementNpc, PlacementRouteId from Situation

**Add:** GetPlacementId(PlacementType) helper method

**Refactor:** 9 files (SituationParser, SceneInstantiator, SpawnFacade, PackageLoader, GameFacade, SituationCompletionHandler, RewardApplicationService, DifficultyCalculationService, SituationFacade)

**Pattern:** Change all `situation.PlacementX` to `situation.GetPlacementId(PlacementType.X)`

**Impact:** HIGH (9 files, placement queries throughout system)

---

### PHASE 0.3: Status Deduplication (CRITICAL)

**Delete:** IsAvailable, IsCompleted properties

**Add:** IsAvailable, IsCompleted as computed properties from Status enum

**Delete:** IsAvailable, IsCompleted from SituationDTO

**Refactor:** 4 files (LocationFacade, SituationCompletionHandler, SituationParser remove DTO assignments, SituationFacade fix desync)

**Pattern:** Change `g.IsAvailable && !g.IsCompleted` to `g.Status == SituationStatus.Available`

**Impact:** MEDIUM (4 files, status queries in location/obligation systems)

---

## VERIFICATION CRITERIA

### Build Success
- ✅ No compilation errors
- ✅ All references resolved

### Architectural Compliance
- ✅ HIGHLANDER: Scene is single source of truth for placement
- ✅ HIGHLANDER: Status enum is single source of truth for status
- ✅ HIGHLANDER: SpawnTracking record is single definition
- ✅ NO duplicate code created (reused existing abstractions)

### Functional Correctness
- ✅ Placement queries return correct entities
- ✅ Status queries filter situations correctly
- ✅ Spawn tracking timestamps populate correctly
- ✅ No null reference exceptions (ParentScene always populated)

### Performance
- ✅ No additional GameWorld lookups (placement resolution already happened via Scene)
- ✅ Computed properties have no performance cost (simple comparisons)

---

## DECISION LOG

### DECISION 1: Extract SpawnTracking

**WHY:** Architectural document confirms Scene will need SpawnedDay for expiration. Creating shared record now prevents duplication later.

**ALTERNATIVES REJECTED:**
- Keep on entities (would duplicate when Scene adds SpawnedDay)
- Delete properties (currently write-only, but expiration feature planned)

### DECISION 2: Remove Situation Placement Properties

**WHY:** Scene already has PlacementType + PlacementId. Situation storing duplicate violates HIGHLANDER and creates desync risk.

**ALTERNATIVES REJECTED:**
- Keep both (creates dual source of truth and update burden)
- Make Situation query GameWorld directly (bypasses Scene, wrong hierarchy)

### DECISION 3: Make Status Booleans Computed Properties

**WHY:** Status enum is proper state machine with 5 states. Booleans are just boolean projections of enum state. Storing both creates desync (already found line 105 desync).

**ALTERNATIVES REJECTED:**
- Keep both (Status=Active desync with IsAvailable=true found)
- Delete Status enum (loses 5-state granularity, LocationFacade would need complex boolean logic)

### DECISION 4: Use Existing Requirement/Consequence Abstractions

**WHY:** CompoundRequirement, ConsequenceFacade, RequirementParser already exist and are shared. Creating new abstractions would duplicate existing code.

**ALTERNATIVES REJECTED:**
- Extract to new classes (duplicates existing ConsequenceFacade)
- Merge SituationCosts and ChoiceCost (intentionally different - strategic vs tactical)

---

## CONFIDENCE LEVEL: 9/10

**Evidence-based decisions:**
- ✅ Traced EXACT data flow (JSON → Parser → Entity → Facade)
- ✅ Found EXACT files affected (9 placement, 4 status, 3 spawn tracking)
- ✅ Identified EXACT violations (placement duplication, status redundancy, desync at line 105)
- ✅ Verified existing abstractions (CompoundRequirement, ConsequenceFacade already exist)

**Uncertainty (1/10):**
- ⚠️ Integration testing needed to verify Scene.PlacementId queries work correctly when Situation queries them
- ⚠️ Edge case: What if ParentScene is null? (Need null-safe navigation in GetPlacementId)

---

## NEXT STEPS

1. Mark investigation task complete ✅
2. Begin Phase 0.1 implementation (SpawnTracking extraction)
3. Proceed to Phase 0.2 (Placement deduplication - highest risk)
4. Complete Phase 0.3 (Status deduplication - medium risk)
5. Build and run integration tests
6. Validate architectural compliance
7. Document any surprises/adjustments
