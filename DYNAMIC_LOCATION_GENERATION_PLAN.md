# Dynamic Location Generation - Holistic Implementation Plan

## Executive Summary

**Status:** 90% architecture exists, completing final 10% to enable full functionality.

**Philosophy:** World materializes in response to narrative need, not pre-emptively.

**Pattern:** Extends Catalogue Pattern (categorical specs → concrete entities) to runtime world-building.

---

## Architecture Principles

### 1. Lazy Instantiation
Locations generated only when:
- PlacementFilter finds no matching existing location
- Venue has generation budget remaining
- Scene requires categorical spatial properties

### 2. Significance-Based Lifecycle
Location persistence determined emergently:
- **Temporary:** Never visited, single scene → Cleanup on scene completion
- **Persistent:** Visited OR multi-scene reference → Promote to permanent
- **Critical:** Authored content → Never cleanup

### 3. Bounded Infinity
Venues have `MaxGeneratedLocations` budget:
- Small towns: 5-10 locations (intimate, constrained)
- Large cities: 50-100 locations (expansive, variety)
- Wilderness: Unlimited (infinite procedural)

### 4. Match First, Generate Last
PlacementFilter evaluation order:
1. Try matching existing locations
2. If no match and budget available, generate new
3. If still no match, throw exception (fail-fast)

### 5. HIGHLANDER Enforcement
- Location.HexPosition = source of truth (Hex.LocationId is derived)
- Location.Provenance = creation metadata (Scene doesn't track created IDs)
- One cleanup service (no duplicate logic)
- One synchronization point per entity relationship

---

## Implementation Phases

### Phase 1: Cleanup & Lifecycle Management
**Files Created:**
- `src/Services/LocationSignificanceEvaluator.cs`
- `src/Services/DependentResourceCleanupService.cs`

**Files Modified:**
- `src/Services/SceneCompletionService.cs` (add cleanup trigger)

**Design:**
- Evaluate location significance based on visits and references
- Three-tier cleanup: Temporary (remove), Persistent (orphan), Critical (keep)
- Triggered when scene enters Completed/Expired state

---

### Phase 2: Venue Generation
**Files Created:**
- `src/GameState/VenueTemplate.cs`
- `src/Services/VenueGeneratorService.cs`

**Files Modified:**
- `src/GameState/Venue.cs` (add MaxGeneratedLocations, GeneratedLocationCount)
- `src/Content/SceneInstantiator.cs` (implement VenueIdSource.GenerateNew)

**Design:**
- VenueTemplate defines categorical venue requirements
- VenueGeneratorService allocates hex clusters and creates venues
- Generation budget prevents unlimited expansion

---

### Phase 3: Synchronization Systems
**Files Created:**
- `src/Services/HexSynchronizationService.cs`
- `src/Services/RouteCleanupService.cs`

**Files Modified:**
- `src/GameState/GameWorld.cs` (AddOrUpdateLocation, RemoveLocation sync hooks)
- `src/Content/LocationParser.cs` (call sync on parse)
- `src/Services/DependentResourceOrchestrationService.cs` (call sync on generation)

**Design:**
- Hex.LocationId synced when Location.HexPosition set
- Venue.LocationIds synced when location added/removed
- Routes cleaned when location deleted

---

### Phase 4: Validation & Playability
**Files Created:**
- `src/Content/Validation/GeneratedLocationValidator.cs`

**Files Modified:**
- `src/Services/VenueGeneratorService.cs` (validate before adding)
- `src/Content/SceneInstantiator.cs` (validate dependent locations)

**Design:**
- Validate hex reachability from player
- Validate unlock mechanisms for locked locations
- Validate required properties present
- Fail-fast on validation errors (throw exception)

---

### Phase 5: PlacementFilter Extensions
**Files Modified:**
- `src/Content/SceneInstantiator.cs` (complete location property filtering)
- `src/Services/LocationGeneratorService.cs` (NEW - generate from filter specs)

**Design:**
- Complete LocationProperties, LocationTags, DistrictId filtering
- Generation fallback when no match and budget available
- Prefer existing content, generate only as last resort

---

### Phase 6: Documentation
**Files Modified:**
- `design/03_world_architecture.md` (add Dynamic Location Generation section)
- `08_crosscutting_concepts.md` (add Dynamic World Building section)

**Design:**
- Document authoring patterns (DependentLocationSpec, PlacementFilter)
- Document lifecycle (significance evaluation, cleanup triggers)
- Document generation budgets and bounded infinity principle

---

## Critical Design Decisions

### Decision 1: Significance Over Configuration
**Chosen:** Evaluate significance emergently (visited, referenced)
**Rejected:** Static `IsPersistent` boolean flag
**Rationale:** Same location can become significant through gameplay

### Decision 2: Generation Budget Over Infinite
**Chosen:** Venues have MaxGeneratedLocations property
**Rejected:** Unlimited generation
**Rationale:** Maintains geographic coherence, prevents performance issues

### Decision 3: Match First, Generate Last
**Chosen:** PlacementFilter tries matching before generation
**Rejected:** Always generate new
**Rationale:** Prefer authored content when appropriate (bootstrap gradient)

### Decision 4: Fail-Fast Validation
**Chosen:** Validate before adding, throw on failure
**Rejected:** Silent fallback to defaults
**Rationale:** Unplayable content worse than crash (playability over compilation)

### Decision 5: Provenance Over Scene Tracking
**Chosen:** Location.Provenance tracks creation metadata
**Rejected:** Scene.CreatedLocationIds list
**Rationale:** Location knows own origin, survives scene deletion

---

## Existing Architecture (90% Complete)

### Already Implemented ✅
- DependentLocationSpec (complete authoring system)
- SceneProvenance (complete tracking)
- PlacementFilter (90% functional, missing location property filtering)
- Self-Contained Scene Pattern (fully functional)
- Hex Grid System (complete spatial positioning)
- Marker Resolution ("generated:{id}" translation)
- Package Generation Pipeline (JSON creation → loading)
- Adjacent Hex Finding (procedural placement)

### Missing (10%) ❌
- Automatic cleanup (provenance exists, logic missing)
- Venue generation (VenueIdSource.GenerateNew throws NotImplementedException)
- Synchronization (Hex/Venue/Route references not maintained)
- Validation gates (no playability checks)
- Complete location filtering (properties/tags partially done)

---

## Implementation Order & Dependencies

```
Phase 1 (Cleanup) - FOUNDATION
  └─ Must establish lifecycle before generating more content

Phase 2 (Venue Generation) - EXPANSION
  ├─ Requires: Phase 1 (cleanup for generated venues)
  └─ Enables: Unlimited procedural expansion

Phase 3 (Synchronization) - INTEGRITY
  ├─ Requires: Phase 1 (cleanup triggers sync)
  └─ Enables: Data consistency for Phases 1-2

Phase 4 (Validation) - QUALITY
  ├─ Requires: Phase 2 (validates generated content)
  └─ Enables: Playability assurance

Phase 5 (PlacementFilter) - COMPLETION
  ├─ Requires: Phase 2 (generation fallback)
  └─ Completes: Categorical filtering system

Phase 6 (Documentation) - KNOWLEDGE
  └─ Requires: All phases complete
```

---

## Success Criteria

### Functional Requirements
- [ ] Scenes spawn with PlacementFilter.Generic, location generated when no match
- [ ] Generated locations have valid hex positions adjacent to base location
- [ ] Generated locations validated as reachable before adding to GameWorld
- [ ] Generated venues allocated 7-hex clusters in unoccupied space
- [ ] Visited locations persist after scene completion
- [ ] Unvisited locations cleaned up when scene expires
- [ ] Hex.LocationId cleared when location deleted
- [ ] Routes removed when location deleted

### Non-Functional Requirements
- [ ] No performance regression (O(n) cleanup, not O(n²))
- [ ] No memory leaks (all references cleared on cleanup)
- [ ] Fail-fast on invalid generation (throw, don't silently degrade)
- [ ] HIGHLANDER compliance (no duplicate logic paths)

---

## Estimated Effort

| Phase | New Code | Modified Code | Risk | Priority |
|-------|----------|---------------|------|----------|
| 1: Cleanup | 300 lines | 50 lines | Low | Critical |
| 2: Venue Gen | 400 lines | 100 lines | Medium | High |
| 3: Sync | 150 lines | 150 lines | Low | Critical |
| 4: Validation | 250 lines | 50 lines | Low | High |
| 5: Filter | 100 lines | 100 lines | Low | Medium |
| 6: Docs | 0 lines | 500 lines | None | Low |
| **TOTAL** | **1200 lines** | **950 lines** | **Low-Medium** | - |

---

## HIGHLANDER Violations to Eliminate

### Current Violations
None identified - existing architecture follows HIGHLANDER strictly.

### Potential Violations to Prevent
- ❌ Don't add Scene.CreatedLocationIds (use Location.Provenance forensics)
- ❌ Don't duplicate cleanup logic (one service, one entry point)
- ❌ Don't sync Hex manually (one synchronization service)
- ❌ Don't validate in multiple places (one validator, called consistently)

---

## Bootstrap Gradient (Design Philosophy)

### Early Game (Day 1-30)
- World: 95% authored, 5% generated
- Locations: Hand-crafted starting town, initial quest venues
- Feel: Intentional, specific, character-driven
- PlacementFilter: Matches existing content

### Mid Game (Day 31-90)
- World: 60% authored, 40% generated
- Locations: Main path authored, side content generated
- Feel: Structured core with emergent edges
- PlacementFilter: Matches authored, starts generating

### Late Game (Day 91+)
- World: 20% authored, 80% generated
- Locations: Only narrative-critical locations authored
- Feel: Infinite procedural variety
- PlacementFilter: Mostly generates new content

### Transition Mechanism
Same PlacementFilter evaluation, different outcomes based on:
- Available authored content (decreases over time)
- Venue generation budgets (increases in late game)
- Player exploration patterns (creates demand for new content)

---

## End State

After implementation, system enables:
- ✅ AI-generated scenes specify categorical location requirements
- ✅ System finds existing match OR generates new location
- ✅ Generated locations integrate seamlessly with authored content
- ✅ Automatic cleanup maintains world hygiene
- ✅ Venues expand procedurally within budget constraints
- ✅ Validation ensures all content is functionally playable
- ✅ Bootstrap gradient transitions authored → procedural seamlessly
