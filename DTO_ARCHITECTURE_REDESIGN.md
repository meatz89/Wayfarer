# DTO ARCHITECTURE REDESIGN

**STATUS**: PAUSED FOR STABILIZATION

**Problem**: DTOs use entity instance IDs (LocationId, NpcId, VenueId) instead of categorical properties. This breaks procedural content generation - same template can't work across different procedural worlds.

**Solution**: Use PlacementFilterDTO with categorical properties everywhere. EntityResolver.FindOrCreate queries existing entities first, generates new from categories if no match.

**STABILIZATION COMPLETED** (2025-11-17):
1. ✅ Planning bloat deleted (8 files, ~120KB)
2. ✅ Venue logic restored (LocationDTO.VenueId, LocationParser venue resolution)
3. ✅ DTO migration path restored (NPCDTO.LocationId/VenueId temporarily kept for parsing existing JSON)
4. ⚠️ **REMAINING**: String lookup methods elimination

**REMAINING WORK BEFORE DTO REDESIGN**:

### String Lookup Methods to DELETE (Not Rename, COMPLETELY REMOVE):

**GameWorld.cs**:
- `GetLocation(string locationName)` - DELETE + fix 40+ callers with 9-step pattern
- `GetDistrictForLocation(string venueName)` - DELETE + trace callers
- `AddStrangerToLocation(string locationName, NPC stranger)` - DELETE + fix callers
- `GetJobByLocations(string originLocationName, string destinationLocationName)` - DELETE + fix callers
- `GetLocationCubes(string locationName)` - DELETE + fix callers
- `GrantLocationCubes(string locationName, int amount)` - DELETE + fix callers
- `GetLocationCountInVenue(string venueName)` - DELETE + fix callers
- `GetAvailableStrangers(string locationName, TimeBlocks currentTimeBlock)` - DELETE + fix callers
- `GetStrangerByName(string strangerName)` - DELETE + fix callers
- `GetNPCCubes(string npcName)` - DELETE + fix callers
- `GrantNPCCubes(string npcName, int amount)` - DELETE + fix callers

**NPCRepository.cs**:
- `GetById(string id)` - DELETE + fix callers
- `GetByName(string name)` - DELETE + fix callers

**9-Step Holistic Pattern for Each Method**:
1. Find method with string parameter
2. Grep ALL callers
3. Read caller files completely
4. Trace where caller got the string (object.Name extraction)
5. Change caller to pass object instead
6. Change method signature to accept object
7. Delete lookup from method body
8. Repeat up entire chain to UI
9. Verify: 0 .Name lookups for entity resolution

After string lookup elimination complete, THEN proceed with DTO layer redesign.

---

## 1. CORRECT DTO PATTERNS

### PlacementFilterDTO (Already Correct)

PlacementFilterDTO enables entity matching via categorical properties rather than hardcoded IDs. The DTO contains lists of categorical dimensions for NPCs, Locations, and Routes.

**NPC Categorical Dimensions**: PersonalityTypes, Professions, SocialStandings, StoryRoles, KnowledgeLevels

**Location Categorical Dimensions**: LocationTypes, LocationProperties, PrivacyLevels, SafetyLevels, ActivityLevels, Purposes

**Route Categorical Dimensions**: TerrainTypes, MinDifficulty, MaxDifficulty

**Selection Strategy**: Specifies how to resolve when multiple matches exist (Random, Closest, LeastRecent).

### Entity Reference Pattern (Wrong vs Correct)

**❌ WRONG - Entity Instance IDs**:

NPCDTO using VenueId and LocationId properties to reference specific entity instances. This violates the architectural mandate prohibiting entity instance IDs and breaks procedural content generation.

**✅ CORRECT - Categorical Placement**:

NPCDTO containing an Id property for template identification (immutable archetype, acceptable) and a SpawnLocation property containing categorical properties. EntityResolver uses these categorical properties to find existing matching entities or generate new ones dynamically.

### Template ID Exception

**ACCEPTABLE**: Template IDs (SceneTemplate.Id, SituationTemplate.Id, NPCTemplate.Id)
- Templates are immutable archetypes (content library)
- Not mutable entity instances (runtime game state)
- Resolved ONCE at boundary, then domain uses objects

**VIOLATION**: Entity instance IDs (locationId, npcId, venueId)
- Should use PlacementFilters with categorical properties
- EntityResolver finds matching entity OR creates new

---

## 2. ENTITY RESOLUTION ARCHITECTURE

### EntityResolver.FindOrCreate Algorithm

The EntityResolver implements the core algorithm for resolving categorical filters into concrete entities. The process follows three steps:

1. **Query Existing Entities** - Search the GameWorld collections for entities matching the filter's categorical properties. Use LINQ to filter by specified dimensions.

2. **Apply Selection Strategy** - If multiple matches exist, apply the filter's selection strategy (Random selects arbitrarily, Closest uses spatial proximity, LeastRecent picks the least-recently-used entity).

3. **Create or Return** - If matches exist, return selected entity. If no matches, create new entity from categorical properties, add to GameWorld, return.

### Matching Priority

Matching follows strict priority order:

1. **Exact Match** - Entity has ALL specified categorical properties matching the filter
2. **Partial Match** - Entity has SOME properties matching (only if filter allows loose matching)
3. **Create** - No match found, generate new entity from filter properties and add to GameWorld

### Property-Based Search

Entity matching uses categorical property queries. All specified properties must match using AND logic. Each categorical dimension is evaluated independently:

**Location Dimensional Matching**: LocationProperties (all must match), LocationTypes (must match one of), PrivacyLevels (must match one of), SafetyLevels, ActivityLevels, Purposes.

**NPC Dimensional Matching**: Professions (must match one of), PersonalityTypes, SocialStandings, StoryRoles, KnowledgeLevels.

Matching logic is simple LINQ filters on List<T> properties, with no hardcoded IDs involved.

---

## 3. DTO LAYER AUDIT

### High-Priority Violations (Most Used)

**NPCDTO.cs**
- Current: `VenueId`, `LocationId` (entity instance IDs)
- Target: `SpawnLocation` (PlacementFilterDTO)
- Impact: ~150 NPCs in tutorial

**LocationDTO.cs**
- Current: `VenueId` (entity instance ID)
- Target: `SpawnVenue` (PlacementFilterDTO for venue categorical matching)
- Impact: ~80 locations

**ObligationDTO.cs**
- Current: `TargetLocationId`, `TargetNpcId` (entity instance IDs)
- Target: `TargetLocation`, `TargetNpc` (PlacementFilterDTO)
- Impact: ~20 obligations

**ConversationTreeDTO.cs**
- Current: `ParticipantNpcId` (entity instance ID)
- Target: `ParticipantFilter` (PlacementFilterDTO)
- Impact: ~30 conversations

**ExchangeDTO.cs**
- Current: `ProviderNpcId`, `LocationId` (entity instance IDs)
- Target: `ProviderFilter`, `LocationFilter` (PlacementFilterDTO)
- Impact: ~15 exchanges

### Medium-Priority Violations

**ObservationSceneDTO.cs, NavigationPayloadDTO.cs, RouteDTO.cs, EmergencySituationDTO.cs, StrangerNPCDTO.cs, VenueDTO.cs, HexDTO.cs, SituationCardDTO.cs, BondChangeDTO.cs**
- Pattern: All reference entity instances via ID strings
- Target: PlacementFilterDTO for categorical matching

### Migration Path (Per DTO)

1. Add `PlacementFilterDTO` property (e.g., `SpawnLocation`, `TargetLocation`)
2. Remove entity instance ID property (e.g., `LocationId`, `NpcId`)
3. Update JSON to use categorical properties instead of IDs
4. Update parser to call `EntityResolver.FindOrCreate(filter)` instead of lookup
5. Pass resolved object (not ID) to domain entity constructor

---

## 4. PARSER REFACTORING STRATEGY

### Current Pattern (WRONG)

Parsers accept DTOs with entity instance ID properties (LocationId, VenueId, NPCId) and use LINQ lookups to find corresponding entities. This creates a dependency on specific entity IDs existing, breaking procedural content generation.

### Target Pattern (CORRECT)

Parsers accept DTOs with PlacementFilterDTO properties containing categorical dimensions. Parsers call EntityResolver.FindOrCreateLocation() or EntityResolver.FindOrCreateNPC() to resolve filters into object references. Entities are created from categorical properties if no match exists.

### EntityResolver Integration Points

**All Parsers Require EntityResolver Injection**:
- NPCParser resolves spawn locations from categorical filters
- ObligationParser resolves target locations and target NPCs from filters
- ConversationTreeParser resolves participants from categorical NPC filters
- ExchangeParser resolves providers (NPCs) and locations from filters
- SceneTemplateParser resolves placement filters for all situation placement

Each parser is constructed with EntityResolver dependency. During parsing, any DTO property containing PlacementFilterDTO is resolved through EntityResolver.FindOrCreate, returning object references, not ID strings.

### Procedural Route Generation

**Current (WRONG)**: RouteDTO contains hardcoded originLocationId and destinationLocationId, binding the route to specific entity instances.

**Target (CORRECT)**: Routes are generated procedurally at GameWorld initialization from hex grid spatial data. For each pair of locations, A* pathfinding generates the hex path from origin HexPosition to destination HexPosition. Routes store OriginLocation and DestinationLocation as object references, not IDs.

This eliminates RouteDTO entirely. Routes are runtime spatial data derived from Location hex coordinates, not authored content.

---

## 5. HEX SPATIAL SYSTEM

### Location Placement

Each Location has an HexPosition property containing AxialCoordinates (Q, R values). This hex position is the source of truth for spatial positioning. Locations also reference their containing Venue. Hex coordinates are mandatory - all locations must have spatial position.

### Venue Definition

Venues are defined as a center hex coordinate with a radius value representing hex distance. A Venue represents a region of the hex grid. Locations within the radius distance from the venue's center hex belong to that venue. Venue membership is derived from hex distance calculation, not hardcoded location-to-venue assignments.

### Route Generation (Procedural)

Routes are generated procedurally from hex spatial data, not authored in JSON. Route generation uses A* pathfinding on the hex grid. For each pair of locations, the pathfinder returns a list of AxialCoordinates from origin to destination. Routes store OriginLocation and DestinationLocation as object references, plus the HexPath list representing the spatial route through the hex grid.

**Routes are generated, not authored** - hex grid is the sole source of truth for spatial relationships.

### Distance Calculations

Hex distance calculation uses the axial coordinate system. Given two AxialCoordinates with (Q, R) values, distance is calculated as: (absolute difference of Q values + absolute difference of combined Q+R values + absolute difference of R values) divided by 2. This provides accurate Manhattan-style distance in hexagonal grids.

---

## IMPLEMENTATION PRIORITY

### Phase 1: Core Entity DTOs (Week 1)
1. NPCDTO - remove VenueId/LocationId, add SpawnLocation filter
2. LocationDTO - remove VenueId, add SpawnVenue filter (or derive from hex position)
3. Update NPCParser, LocationParser to use EntityResolver

### Phase 2: Relationship DTOs (Week 2)
4. ObligationDTO - remove target IDs, add target filters
5. ConversationTreeDTO - remove participant IDs, add participant filters
6. ExchangeDTO - remove provider/location IDs, add filters

### Phase 3: Remaining DTOs (Week 3)
7. All other DTOs with entity instance ID violations
8. Route generation system (replace RouteDTO entirely)

### Phase 4: Verification (Week 4)
9. Search codebase for all `.Id` property accesses in parsers
10. Verify 0 entity instance ID lookups remain
11. Test procedural content generation end-to-end

---

## SUCCESS CRITERIA

✅ **Zero entity instance IDs in DTOs** (only template IDs)
✅ **All parsers use EntityResolver.FindOrCreate**
✅ **Same scene template works in different procedural worlds**
✅ **Routes generated from hex grid, not authored**
✅ **All locations have hex positions**

---

## ARCHITECTURE VALIDATION

**Core Test**: Can a scene template spawn in three different procedurally-generated worlds without any modification to the template?

**Before (BROKEN - Hardcoded IDs)**:

Scene template contains references to specific entity IDs (elena_innkeeper, rusty_tankard). When template is instantiated in a second procedurally-generated world, those specific entities don't exist. EntityResolver.FindOrCreate searches for an entity with Id matching "elena_innkeeper", finds nothing, and either fails to spawn the scene or creates a broken entity. Result: Template is coupled to one specific world and cannot be reused.

**After (CORRECT - Categorical Filtering)**:

Scene template contains PlacementFilterDTO with categorical dimensions (professions: ["Innkeeper"], storyRoles: ["Facilitator"]). When template is instantiated in any world, EntityResolver.FindOrCreateNPC searches for NPCs matching the filter. In world 1, it finds Elena. In world 2, it finds a procedurally-generated Innkeeper NPC. In world 3, it creates a new Innkeeper if none exists. Same template works identically across infinite procedural worlds.

**This is DDR-006 (Categorical Scaling) - the foundation of the entire architecture.**
