# Venue-First Spatial Architecture - Complete Refactoring Plan

## HIGHLANDER Violation Identified

**DUAL PLACEMENT SYSTEM (WRONG):**
1. Authored locations with Q,R hardcoded in JSON
2. Procedural locations with Q,R calculated by SceneInstantiator at runtime

**SINGLE CORRECT PATTERN (HIGHLANDER):**
ALL locations (authored + procedural) placed using identical procedural algorithm.

---

## Core Architectural Principle

**Venue-First Spatial Model:**
- Venues are PRIMARY spatial partitions that claim hex territory FIRST
- Venues define centerHex + hexAllocation strategy (ClusterOf7 = 7 hexes)
- Locations are SECONDARY entities placed procedurally within venue territories SECOND
- JSON NEVER contains location Q,R coordinates
- Placement happens in post-parse initialization phase (after entity loading, before hex sync)

**Procedural Placement Algorithm:**
```
For each venue:
  Sort locations by name (deterministic order)
  First location → venue.CenterHex
  Subsequent locations → FindAdjacentHex(previousLocation, sameVenue)
    - Find unoccupied neighbor hex
    - Validate venue separation (no adjacency to other venues)
    - Return first valid hex
```

---

## System Layers

### Layer 1: JSON (Input Boundary)
**Venues** define spatial territory:
```json
{
  "name": "Town Square",
  "centerHex": { "q": 0, "r": 0 },
  "hexAllocation": "ClusterOf7",
  "maxLocations": 20
}
```

**Locations** described CONTEXTUALLY (no coordinates):
```json
{
  "name": "Town Square Center",
  "venueId": "Town Square",  // Temporary - will become spatial
  "description": "...",
  "properties": { "base": ["crossroads", "public"] }
}
```

### Layer 2: DTO (Data Boundary)
- VenueDTO has CenterHex, HexAllocation, MaxLocations
- LocationDTO has NO Q,R properties (deleted)
- LocationDTO has venueId temporarily (for backward compatibility)

### Layer 3: Parser (Transformation)
- VenueParser creates Venue with centerHex and hexAllocation
- LocationParser creates Location with NO hex position
- NO venue assignment in parser (deferred to placement phase)

### Layer 4: Placement Service (Domain Logic)
- LocationPlacementService.PlaceLocationsInVenue(venue, locations)
- Extracted from SceneInstantiator (HIGHLANDER: single implementation)
- Sets Location.HexPosition procedurally
- Validates venue separation, hex availability, capacity budget

### Layer 5: PackageLoader (Orchestration)
```
Load Venues → Load Locations → PlaceAllLocations() → SyncLocationHexPositions()
```

PlaceAllLocations():
- Groups locations by venue
- Calls LocationPlacementService for each venue
- Sets all Location.HexPosition values before hex sync

---

## Implementation Phases

### Phase 1: Create LocationPlacementService ✅ COMPLETE
- Extract FindAdjacentHex from SceneInstantiator
- Extract IsAdjacentToOtherVenue from SceneInstantiator
- New method: PlaceLocationsInVenue(venue, locations)
- Registers in DI container

### Phase 2: Delete Q,R from LocationDTO ✅ COMPLETE
- Remove Q and R properties entirely
- Add comment explaining HIGHLANDER principle
- Hex coordinates NEVER flow through DTO layer

### Phase 3: Update LocationParser ✅ COMPLETE (partially)
- Delete hex assignment code (was lines 33-41)
- Parser creates Location with NO hex position
- Venue assignment temporarily via venueId (will be spatial later)

### Phase 4: Update PackageLoader ✅ COMPLETE
- Inject LocationPlacementService
- Add PlaceAllLocations() method
- Call BEFORE SyncLocationHexPositions()
- Groups locations by venue, calls placement service

### Phase 5: Refactor SceneInstantiator ⏳ PENDING
- Use LocationPlacementService instead of inline logic
- Delete FindAdjacentHex method (moved to service)
- Delete IsAdjacentToOtherVenue method (moved to service)
- Don't set Q,R in LocationDTO (no longer exists)
- Call placement service directly after creating Location

### Phase 6: Update JSON Content ⏳ PENDING
- Delete Q,R from ALL locations in 01_foundation.json
- Keep venueId temporarily (spatial assignment comes later)
- Add venueId to locations that don't have it

### Phase 7: Service Registration ⏳ PENDING
- Register LocationPlacementService in ServiceConfiguration.cs
- Update PackageLoader registration (already injected in constructor)

### Phase 8: Fix VenueParser ⏳ PENDING
- Parse centerHex from VenueDTO → Venue.CenterHex
- Parse hexAllocation from VenueDTO → Venue.HexAllocation
- Parse maxLocations with default 20

### Phase 9: Restore VenueId to LocationDTO ⏳ PENDING
**TEMPORARY BACKWARD COMPATIBILITY:**
- Add VenueId property back to LocationDTO
- LocationParser assigns venue via venueId lookup
- This is TEMPORARY until spatial venue assignment implemented
- Will be deleted when venue assigned via hex containment

### Phase 10: Build and Test ⏳ PENDING
- Compile and fix any remaining errors
- Run tests
- Verify locations have hex positions
- Verify deterministic placement
- Check console logs for placement messages

---

## Future Work (Post-HIGHLANDER)

### Spatial Venue Assignment (Replaces VenueId)
After placement system stable, implement:
1. Delete VenueId from LocationDTO permanently
2. Delete venueId from all location JSON
3. Assign venues spatially: location.Venue = venues.First(v => v.ContainsHex(location.HexPosition))
4. Validate all locations within venue territories
5. Enforce venue territory validation (no overlaps, buffer enforcement)

This requires venue territories to be validated BEFORE location placement.

---

## Critical Constraints

### Order of Operations (STRICT)
1. Load and parse venues (establish territories)
2. Load and parse locations (NO hex positions yet)
3. PlaceAllLocations (set hex positions procedurally)
4. SyncLocationHexPositions (sync to hex grid)
5. Normal game initialization continues

### Determinism
- Locations sorted by name before placement
- Same input → same output (reproducible hex positions)
- No randomness in placement algorithm

### Venue Separation
- Adjacent hex check: IsAdjacentToOtherVenue
- Prevents venues from bleeding into each other
- Maintains clear spatial boundaries

### Capacity Budget
- Venue.MaxLocations enforced
- Throws if locations.Count > MaxLocations
- Prevents unbounded venue expansion

---

## Success Criteria

1. ✅ Build succeeds with no compilation errors
2. ⏳ All locations have Location.HexPosition after PlaceAllLocations
3. ⏳ Hex positions deterministic (same every load)
4. ⏳ Venue separation maintained (no adjacent venues)
5. ⏳ Console shows placement messages for each location
6. ⏳ Game loads and locations are accessible
7. ⏳ Tutorial locations work correctly
8. ⏳ Scene-generated locations work correctly
9. ⏳ All tests pass

---

## Files Modified

### NEW:
- LocationPlacementService.cs (domain service)

### MODIFIED:
- LocationDTO.cs (deleted Q,R properties)
- LocationParser.cs (deleted hex assignment)
- PackageLoader.cs (added PlaceAllLocations, injected service)
- VenueParser.cs (TODO: parse centerHex, hexAllocation)
- SceneInstantiator.cs (TODO: use placement service)
- ServiceConfiguration.cs (TODO: register service)
- 01_foundation.json (TODO: delete Q,R from locations)

### TO BE MODIFIED (Future):
- LocationDTO.cs (delete VenueId after spatial assignment)
- LocationParser.cs (assign venue spatially, not via venueId)
- All location JSON (delete venueId after spatial assignment)

---

## Rollback Plan

If implementation fails:
```bash
git checkout HEAD -- src/Content/DTOs/LocationDTO.cs
git checkout HEAD -- src/Content/LocationParser.cs
git checkout HEAD -- src/Content/PackageLoader.cs
git checkout HEAD -- src/Content/Core/01_foundation.json
rm src/Services/LocationPlacementService.cs
```

Restore Q,R to LocationDTO, restore hex assignment in LocationParser, remove PlaceAllLocations call.

---

## Current Status

**Completed:**
- Phase 1: LocationPlacementService created ✅
- Phase 2: Q,R deleted from LocationDTO ✅
- Phase 3: LocationParser updated (partial) ✅
- Phase 4: PackageLoader updated ✅

**In Progress:**
- Phase 5: Refactor SceneInstantiator
- Phase 6: Update JSON content
- Phase 7: Service registration
- Phase 8: Fix VenueParser
- Phase 9: Temporary VenueId restoration
- Phase 10: Build and test

**Next Step:**
Restore venueId to LocationDTO temporarily, then continue with remaining phases.
