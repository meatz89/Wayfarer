# ADR-015: Package-Round Entity Tracking

**Status:** Approved
**Date:** 2025-11-23
**Deciders:** Architecture Team
**Related:** ADR-001 (HIGHLANDER Principle), ADR-008 (Single Source of Truth)

## Context

Previous spatial initialization implementation violated the package-round principle by processing ALL entities in GameWorld whenever ANY package loaded. This caused performance degradation and architectural confusion.

**Violation Pattern (BEFORE):**
```csharp
private void PlaceAllLocations()
{
    // Iterates ENTIRE GameWorld.Locations collection
    List<Location> locations = _gameWorld.Locations.OrderBy(l => l.Name).ToList();

    // Filters for unplaced locations via entity state check
    List<Location> locationsToPlace = locations.Where(l => l.HexPosition == null).ToList();

    // Problem: Re-scans all entities every package load
}
```

**Problems:**
1. **Performance**: O(n) scan of all entities for every package load (quadratic when loading multiple packages)
2. **Architectural Impurity**: Entity state checks (`HexPosition == null`) used for deduplication instead of package-round tracking
3. **Violation of Package-Round Principle**: Methods should process ONLY entities from THIS package round, not ALL entities in GameWorld

## Decision

Implement explicit package-round entity tracking via `PackageLoadResult` structure. Spatial initialization methods accept explicit entity lists instead of querying GameWorld collections.

**Correct Pattern (AFTER):**
```csharp
// Loading phase returns result
private PackageLoadResult LoadPackageContent(Package package, bool allowSkeletons)
{
    PackageLoadResult result = new PackageLoadResult { PackageId = package.Id };

    // As entities are parsed and added to GameWorld, track them
    foreach (var locationDto in package.Locations)
    {
        Location location = ParseLocation(locationDto);
        _gameWorld.AddOrUpdateLocation(location.Name, location);
        result.LocationsAdded.Add(location); // Track for THIS round
    }

    return result;
}

// Spatial initialization receives explicit list
private void PlaceLocations(List<Location> locations)
{
    // Guaranteed to receive ONLY new locations from current package round
    // No GameWorld queries, no entity state checks
    foreach (Location location in locations)
    {
        _locationPlacementService.PlaceLocation(location, distanceHint, player);
    }
}
```

## Architectural Principles

### Package-Round Isolation

**Principle:** One entity exists in exactly one package. Initialize ONLY entities from THIS package load round.

**Implementation:**
- `PackageLoadResult` tracks entities added during single package load
- Spatial methods receive explicit `List<Entity>` parameters
- No GameWorld collection queries during initialization
- No entity state checks for deduplication

### Loading Scenarios

**Static Loading (Startup):**
```csharp
List<PackageLoadResult> allResults = new List<PackageLoadResult>();
foreach (var package in packages)
{
    allResults.Add(LoadPackageContent(package, allowSkeletons));
}

// Aggregate across all packages
var allLocations = allResults.SelectMany(r => r.LocationsAdded).ToList();

// Initialize ONCE with aggregated lists
PlaceLocations(allLocations);
```

**Dynamic Loading (Runtime):**
```csharp
PackageLoadResult result = LoadPackageContent(package, allowSkeletons);

// Initialize IMMEDIATELY with result lists
PlaceLocations(result.LocationsAdded);
```

### Forbidden Patterns

**NEVER:**
- Query `_gameWorld.*` collections in spatial initialization methods
- Check entity state for deduplication (`HexPosition == null`)
- Call methods like `PlaceAllLocations()` that iterate all entities
- Assume spatial methods are idempotent (they are NOT by design)

**ALWAYS:**
- Track entities in `PackageLoadResult` as they're added to GameWorld
- Pass explicit entity lists to spatial methods
- Process ONLY entities from current package round
- Fail fast if entity already initialized (indicates architecture violation)

## Consequences

### Positive

**Performance Improvement:**
- Static loading: O(n) scan of all entities once vs O(n×p) scans (where p = package count)
- Dynamic loading: O(m) processing where m = new entities vs O(n) processing where n = total entities
- Constant-time package loading (independent of GameWorld size)

**Architectural Clarity:**
- Explicit data flow (entities flow through parameters, not queries)
- Single responsibility (loading tracks entities, initialization receives entities)
- Package-round principle enforced by architecture (impossible to violate)

**Testability:**
- Unit test spatial methods with explicit entity lists
- Verify package isolation (no cross-contamination between rounds)
- Performance tests verify constant-time loading

### Negative

**Code Verbosity:**
- More explicit parameters (methods accept `List<Location>` instead of querying)
- More tracking code (accumulate entities in result during load)
- Aggregation logic needed for static loading (SelectMany pattern)

**Non-Idempotent Methods:**
- Spatial methods are NOT idempotent by design
- Calling twice causes incorrect state (intentional for fail-fast)
- Requires careful coordination between loading and initialization

### Neutral

**Memory Overhead:**
- PackageLoadResult holds references to entities (not copies)
- Overhead: ~0.8 KB per package (negligible)
- Lifetime: Short-lived (scope of loading operation)

## Alternatives Considered

### Alternative 1: Keep Entity State Checks

**Approach:** Continue using `HexPosition == null` to filter entities.

**Rejected because:**
- Violates package-round principle
- Hides architectural problem with tactical fix
- Requires O(n) scan of all entities every load
- Entity state checks are code smell (deduplication via filtering)

### Alternative 2: Mark Entities with PackageId

**Approach:** Add `PackageId` property to entities, filter by package.

**Rejected because:**
- Pollutes domain entities with loading metadata
- Violates separation of concerns (entities shouldn't know about packages)
- Still requires O(n) filtering of all entities
- Package-round tracking simpler and cleaner

### Alternative 3: Separate "New Entity" Collection

**Approach:** Maintain `_newEntities` collection separate from GameWorld.

**Rejected because:**
- Violates Single Source of Truth (entities in two collections)
- Risk of desynchronization between collections
- Adds complexity (when to clear `_newEntities`?)
- PackageLoadResult provides same benefit with local scope

## Implementation Checklist

- [x] Create `PackageLoadResult` class with entity lists
- [x] Refactor `LoadPackageContent` to return `PackageLoadResult`
- [x] Modify all `Load*()` methods to track entities in result
- [x] Change spatial method signatures to accept `List<Entity>` parameters
- [x] Update `LoadStaticPackages` to accumulate and aggregate results
- [x] Update `LoadDynamicPackageFromJson` to pass result lists directly
- [x] Delete entity state checks (`HexPosition == null` filters)
- [x] Delete obsolete comments about idempotent initialization
- [x] Update architecture documentation (runtime view, crosscutting concepts, building blocks)
- [x] Create this ADR
- [x] Build verification (0 errors)
- [x] Test static loading (game startup)
- [x] Test dynamic loading (scene activation)
- [x] Verify console logs (no duplicate placement)

## References

- **HIGHLANDER Principle:** One entity exists in exactly one package (ADR-001)
- **Single Source of Truth:** GameWorld owns entities (ADR-008)
- **Package-Round Principle:** Initialize ONLY entities from THIS package load round (this ADR)
- **Catalogue Pattern:** Generate actions for new entities using full GameWorld context (separate concern)

## Notes

Entity state checks for **domain validation** (e.g., checking if location is properly initialized before use) are ACCEPTABLE and CORRECT. This ADR forbids entity state checks for **deduplication** (filtering already-processed entities during initialization). The distinction is purpose: validation vs deduplication.
