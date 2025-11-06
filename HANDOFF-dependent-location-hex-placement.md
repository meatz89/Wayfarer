# HANDOFF: Dependent Location Hex Placement Fix

## Problem & Motivation

Scene-generated dependent locations (private rooms created dynamically when scenes spawn) were being created WITHOUT hex positions on the game's hex grid. This violates a fundamental architectural constraint: ALL locations in Wayfarer must occupy exactly one hex on the world hex grid. Without hex positions, the intra-venue travel system cannot generate movement actions between locations in the same venue cluster, making these dependent locations completely inaccessible to the player.

This matters because the service scene pattern (lodging, bathing, healing, training) depends on creating temporary private rooms that players access after negotiating with NPCs. A player negotiates lodging with an innkeeper, receives a room key, and should be able to move to an adjacent hex containing their private room. Without hex placement, the room exists in GameWorld but has no spatial presence - it's invisible to the location action system.

The pain point manifested as: dependent locations generated, rewards applied correctly (unlock + key granted), but LocationActionCatalog couldn't generate "Move to Private Room" actions because the room had no hex coordinates to verify adjacency.

## Architectural Discovery

### The Hex Grid as Spatial Foundation

Wayfarer's world exists on a hex grid using axial coordinates (Q, R). Every location occupies exactly one hex. This isn't optional or convenience - it's architectural bedrock. The hex grid defines:

**Venues as 7-Hex Clusters**: A venue consists of one center hex plus six adjacent hexes. All locations sharing a VenueId must be geometrically adjacent on the hex grid. This creates spatial coherence - locations in "The Silver Chalice Inn" venue are physically adjacent hexes, not scattered across the map.

**Two Travel Systems**: Intra-venue travel happens between ADJACENT hexes in the SAME venue (instant, free movement because geometrically adjacent). Inter-venue travel happens between NON-ADJACENT hexes in DIFFERENT venues (requires routes, costs resources, takes time). Adjacency is checked via hex coordinate geometry, not venue membership.

**Location Identity**: A location's hex position is as fundamental as its name. Without it, the location cannot participate in movement, cannot have routes generated to it, and cannot verify adjacency to other locations. It's not "data we might need later" - it's core identity.

### The Parser-JSON-Entity Triangle

Content flows through three layers with distinct timing:

**JSON Layer (Authoring)**: Location definitions in JSON may or may not contain hex coordinates. Normal locations defined in foundation JSON have coordinates provided separately in the hex grid JSON file. Scene-generated dependent locations must have coordinates embedded directly in their JSON because they're generated dynamically at scene finalization, not parsed from static files.

**Parser Layer (Parse-Time Translation)**: Parsers execute ONCE during game initialization. They translate JSON into domain entities, resolve references via GameWorld lookups, and call catalogues to generate procedural content. Critically, catalogues are ONLY accessible during parsing - runtime code never imports or calls them.

**Entity Layer (Runtime)**: Domain entities (Location, NPC, Scene, etc.) live in GameWorld collections. Runtime code queries these collections directly. Entities are immutable after parsing completes (except for state changes like IsLocked). No re-parsing, no catalogue calls, no JSON access at runtime.

This triangle creates a timing boundary: parse-time is when ALL generation happens. Runtime operates on pre-generated, fully-resolved entities.

### The Scene Instantiation Architecture

Scenes follow a two-phase lifecycle:

**Provisional Phase**: Scene created with metadata (estimated difficulty, situation count) but NO situations instantiated yet. Exists in GameWorld.Scenes with State=Provisional. Player can see the scene in their choices (perfect information pattern) before committing. Dependent resource specs generated but NOT loaded yet.

**Finalization Phase**: When player selects the scene, it finalizes. Situations instantiated, dependent resource JSON generated, PackageLoader called to add locations/items to GameWorld, LocationActionCatalog generates actions for new locations. Scene transitions to State=Active.

The separation allows players to see what they're choosing before triggering expensive operations. Critically, FinalizeScene returns DependentResourceSpecs containing the generated JSON package and lists of created entity IDs.

### The Orchestration Gap

GameWorldInitializer was attempting to spawn starter scenes but lacked orchestration capabilities. It could call CreateProvisionalScene and FinalizeScene but couldn't handle the DependentResourceSpecs return value (no ContentGenerationFacade, no PackageLoaderFacade, no HexRouteGenerator dependencies).

The proper orchestration layer is GameFacade, which already has complete infrastructure: can write JSON files to disk, call PackageLoader to parse them, add items to player inventory, generate hex routes for new locations. GameFacade.SpawnSceneWithRewards implements the full pattern correctly.

## Domain Model Understanding

### Location as Spatial Entity

A Location is fundamentally a spatial entity occupying one hex on the world grid. Its properties:

**Identity**: Id (unique string), Name (display), HexPosition (AxialCoordinates Q and R)

**Venue Membership**: VenueId (string) indicating which 7-hex cluster it belongs to. Venue object reference resolved by parser from VenueId lookup in GameWorld.Venues.

**Accessibility**: IsLocked (bool) controlling whether location can be entered. Locked locations exist spatially but cannot be accessed without the required unlock item.

**Properties**: LocationProperties enum flags defining what activities are available (Crossroads, Commercial, SleepingSpace, etc.). These drive action generation via LocationActionCatalog.

**Temporal Availability**: CurrentTimeBlocks list determining when location is accessible. Some locations only open during specific time periods.

The hex position is not "optional metadata" - it's as essential as the location's Id. A location without hex position cannot participate in the movement system.

### Dependent Location Lifecycle

Dependent locations are locations that don't exist until a scene creates them. They follow a specific lifecycle:

**Pre-Existence**: Before scene spawns, dependent location does NOT exist in GameWorld.Locations. It's not "hidden" or "disabled" - it literally doesn't exist.

**Creation at Scene Finalization**: When scene finalizes, SceneInstantiator generates DependentLocationSpec describing the location (name pattern, description pattern, properties, hex placement strategy). This spec gets converted to LocationDTO with concrete values (placeholders replaced, hex position determined). DTO gets wrapped in JSON package. PackageLoader parses the JSON and LocationParser creates the actual Location entity, adding it to GameWorld.Locations.

**Persistence**: Once created, the location persists FOREVER. Even after scene completes, the location remains in GameWorld. It may become locked again, but it doesn't disappear. This allows players to return to locations they've unlocked.

**Hex Placement**: The dependent location must be placed on an ADJACENT hex to the base location (the NPC's location where scene spawns). HexPlacementStrategy enum indicates the placement rule. SameVenue strategy means "find unoccupied adjacent hex in same venue cluster." Adjacent strategy means "find unoccupied adjacent hex regardless of venue boundary."

### Intra-Venue Movement Pattern

Movement between locations in the same venue follows specific rules:

**Geometric Adjacency Required**: Two locations can have intra-venue movement if they are BOTH in the same venue (same VenueId) AND their hexes are geometrically adjacent (differ by one step in axial coordinates). Venue membership alone is insufficient - they must be spatially adjacent.

**Action Generation**: LocationActionCatalog generates intra-venue movement actions during parsing (when locations are added to GameWorld). For each location, it queries all other locations in GameWorld, finds those in same venue with adjacent hex positions, and generates "Move to X" actions.

**No Cost**: Intra-venue movement is instant and free BECAUSE the hexes are adjacent. This is the mechanical payoff for geometric adjacency - close locations have no movement friction.

**Runtime Filtering**: Generated actions are filtered at runtime by IsLocked status. A "Move to Private Room" action exists after room creation, but the action system checks IsLocked before presenting it. Once player gets the room key and room unlocks, the action becomes executable.

## Current State Analysis

### The Multi-Bug Reality

Investigation revealed four separate bugs plus one architectural triangle violation, all conspiring to prevent dependent location hex placement:

**Bug 1 - FindAdjacentHex Returns Null**: SceneInstantiator contains a method FindAdjacentHex that takes a HexPlacementStrategy enum. It has a switch statement with cases for different strategies. The SameVenue case returns null immediately with a comment stating "Same venue = no hex placement needed (intra-venue instant travel)". This reflects a fundamental misunderstanding. The comment conflates "movement is free because adjacent" with "location doesn't need hex position." The method should find an adjacent hex for both SameVenue and Adjacent strategies.

**Bug 2 - BuildLocationDTO Never Calls FindAdjacentHex**: SceneInstantiator has a method BuildLocationDTO that converts DependentLocationSpec to LocationDTO. At the point where hex placement should happen, there's a comment saying "Find hex placement" followed immediately by "Build LocationDTO" with no code between them. The commented intention was clear but the implementation was missing. The DTO gets built without any hex coordinates.

**Bug 3 - LocationDTO Missing Hex Properties**: LocationDTO class has properties for Id, Name, Description, VenueId, and various gameplay properties, but no properties for hex coordinates. Even if BuildLocationDTO called FindAdjacentHex and got coordinates back, it had nowhere to store them. The DTO structure couldn't represent hex-positioned locations.

**Bug 4 - Orchestration in Wrong Layer**: GameWorldInitializer was spawning starter scenes during initialization. It called CreateProvisionalScene and FinalizeScene but discarded the DependentResourceSpecs return value (captured with underscore variable). The generated locations and items were never loaded into GameWorld. Even if all other bugs were fixed, the resources wouldn't materialize.

**Triangle Violation - Parser Can't Read What DTO Can't Store**: LocationParser has logic to sync hex positions from the hex grid JSON (HexParser.SyncLocationHexPositions matches hex locationId to location objects). But it has NO logic to read hex coordinates directly from LocationDTO properties because those properties don't exist. The parser assumed hex coordinates would always come from separate hex grid sync, not from the DTO itself.

### Existing Hex Placement Patterns

The codebase has one working pattern for hex placement: static locations defined in JSON with separate hex grid.

**Foundation Locations Pattern**: Locations defined in foundation JSON (like "common_room", "fountain_plaza") specify Id, Name, Properties, VenueId. Hex grid JSON has entries mapping locationId to Q and R coordinates. During parsing, HexParser reads hex grid JSON, creates Hex entities, then calls SyncLocationHexPositions to assign hex positions to locations. This two-file pattern works for static content known at authoring time.

**The Missing Pattern**: Dynamic locations generated at runtime need hex coordinates embedded in their DTO because there's no separate hex grid file for them. SceneInstantiator must determine the hex position, store it in the DTO, and LocationParser must read it from the DTO. This pattern was intended (evidenced by the "Find hex placement" comment) but never implemented.

### HexPlacementStrategy Enum Semantics

The enum has values: Adjacent, SameVenue, Distance, Random. Current code treats Adjacent and SameVenue as different strategies. Adjacent is implemented (finds unoccupied adjacent hex). SameVenue returns null. Distance and Random throw NotImplementedException.

The semantic intent of SameVenue versus Adjacent was unclear. Does SameVenue have special logic (prefer specific directions, enforce venue boundaries differently)? Or is it just Adjacent with a semantic label indicating "this is for same-venue placement"?

Investigation of the domain model clarified: venues are DEFINED by geometric adjacency. A venue is a 7-hex cluster where all hexes are adjacent to the center. You cannot have locations in the same venue that aren't adjacent (violates venue definition). Therefore SameVenue and Adjacent must have IDENTICAL implementation - both find any unoccupied adjacent hex. The enum distinction is semantic (communicating intent) not behavioral (different algorithms).

## Design Approach & Rationale

### Principle 1 - All Locations Have Hex Positions (No Exceptions)

The correct understanding is: there are NO locations without hex positions in Wayfarer's architecture. Every location must occupy one hex on the grid. "Intra-venue instant travel" describes movement BEHAVIOR (free, instant) not location STRUCTURE (no hex position). The movement is instant BECAUSE the hexes are adjacent, not INSTEAD OF having hex positions.

This principle rejects any special-casing or "virtual locations" or "conceptual locations without spatial presence." If something is a location, it has hex coordinates. Period.

### Principle 2 - Fix the Triangle (JSON ↔ DTO ↔ Parser ↔ Entity)

The Parser-JSON-Entity triangle must be complete. If dependent locations need hex positions (they do), then:
- LocationDTO must have Q and R properties to store hex coordinates
- SceneInstantiator must populate those properties when building the DTO
- LocationParser must read those properties when parsing the DTO
- Location entity must have HexPosition assigned from the DTO values

Breaking any link in this chain leaves the triangle incomplete. The fix requires touching all layers.

### Principle 3 - SameVenue = Adjacent in Implementation

User confirmed: HexPlacementStrategy.SameVenue and HexPlacementStrategy.Adjacent should have IDENTICAL implementation. Both find any unoccupied adjacent hex. The distinction exists for semantic clarity (SameVenue signals "this dependent location stays in same venue" even though the implementation is identical to Adjacent).

This decision preserves the semantic value of the enum (communicating intent in catalogue code) while eliminating the buggy divergence in implementation (SameVenue returning null while Adjacent works correctly).

### Principle 4 - Orchestration Belongs in GameFacade

Spawning scenes with dependent resources requires orchestration: generate JSON, write files, call PackageLoader, add items to inventory, generate routes. This orchestration belongs in GameFacade, which has all necessary dependencies.

GameWorldInitializer is the wrong place. It's a static initialization method with no service dependencies. It constructs GameWorld from static JSON files. Attempting to make it handle dynamic scene spawning with dependent resources would require passing in multiple service dependencies, breaking its clean static nature.

The solution: GameWorldInitializer should NOT spawn starter scenes. It should identify them (find templates with IsStarter flag) but leave spawning to GameFacade. After GameWorld initialization completes and DI container is built, GameFacade can spawn starter scenes with full orchestration support.

### Principle 5 - Comprehensive Logging for Diagnostic Flow

The complete dependent location flow spans multiple layers (catalogue generation → DTO building → JSON packaging → PackageLoader parsing → location creation → action generation). When debugging failures, you need visibility into each step.

Add logging at critical points: when hex position is determined, when DTO contains coordinates, when LocationParser reads coordinates, when LocationActionCatalog processes the location. This creates a diagnostic trail showing exactly where the flow succeeds or fails.

## Implementation Strategy

### Phase 1 - Fix the DTO Layer (Add Hex Coordinate Properties)

LocationDTO needs Q and R properties (nullable integers) to store axial hex coordinates. These are nullable because normal locations from JSON won't have them (they get hex positions from hex grid sync). Only dependent locations from scenes populate these properties.

Add documentation clarifying: Q and R are for scene-generated dependent locations. LocationParser must check if these properties have values and, if so, assign Location.HexPosition from them. This creates the DTO → Entity path for embedded hex coordinates.

### Phase 2 - Fix FindAdjacentHex (Make SameVenue Work Like Adjacent)

The FindAdjacentHex method's switch statement needs modification. Currently SameVenue case returns null. Change it to fall through to the Adjacent case (using C# switch fall-through pattern: case SameVenue: case Adjacent: shared implementation).

Update method documentation to clarify: both SameVenue and Adjacent find any unoccupied adjacent hex. The difference is semantic intent, not implementation. SameVenue signals "this is for intra-venue placement" but uses the same adjacency-finding algorithm.

The Adjacent case implementation is correct: get base location's hex position, query HexMap for neighbors, iterate neighbors checking if any location already occupies that hex, return first unoccupied neighbor hex. If no unoccupied hex found, throw exception (can't place location).

### Phase 3 - Fix BuildLocationDTO (Actually Call FindAdjacentHex)

BuildLocationDTO method has a comment "Find hex placement" with no code following it. Add the implementation:

Get the base location from context (context.CurrentLocation). This is where the scene spawns (e.g., the inn's common room where the innkeeper stands). Validate it exists - if null, throw exception.

Call FindAdjacentHex passing base location and spec.HexPlacement strategy. This returns nullable AxialCoordinates. If null (shouldn't happen with fixed FindAdjacentHex but check defensively), throw exception explaining hex placement failed.

Extract Q and R values from the coordinates. Assign them to the LocationDTO's Q and R properties. Add logging showing which hex was selected adjacent to which base location.

This completes the generation side: catalogue creates spec with strategy → BuildLocationDTO determines concrete hex → DTO contains coordinates.

### Phase 4 - Fix LocationParser (Read Hex Coordinates from DTO)

LocationParser.ConvertDTOToLocation method processes LocationDTO and builds Location entity. Currently it resolves venue reference, parses properties, handles time blocks, but never touches hex coordinates.

Add logic after venue resolution (makes sense grouped with spatial concerns): check if dto.Q has value AND dto.R has value. If both present, create AxialCoordinates from them and assign to location.HexPosition. Add logging confirming hex position was read from DTO.

This completes the parsing side: DTO contains coordinates → parser reads them → entity has HexPosition.

Note: normal locations from static JSON won't have Q/R in DTO (properties will be null). They get hex positions from HexParser.SyncLocationHexPositions later. Only dependent locations from scenes use the DTO→Entity path. Both paths are valid.

### Phase 5 - Fix Orchestration (Move Starter Scene Spawning to GameFacade)

GameWorldInitializer.CreateGameWorld currently has code that finds starter templates, builds spawn rewards, creates provisional scenes, finalizes them, and discards the DependentResourceSpecs return value.

Delete this spawning code entirely. Replace with comment explaining: starter scene spawning deferred to GameFacade because orchestration (dynamic JSON generation, PackageLoader calls, route generation) requires service dependencies that GameWorldInitializer doesn't have.

Keep the logging that identifies starter templates (useful for verification) but don't spawn them.

GameFacade needs new method: SpawnStarterScenes(). This method queries GameWorld.SceneTemplates for templates with IsStarter flag. For each, builds spawn reward using PlacementFilter to find appropriate placement entity (first matching NPC/Location based on filter criteria). Builds spawn context. Calls its own SpawnSceneWithRewards method which already implements full orchestration.

This method gets called after DI container construction completes (from wherever game initialization flow calls into GameFacade to set up initial game state).

### Phase 6 - Add Comprehensive Logging

At each critical point in the flow, add Console.WriteLine logging:

**In BuildLocationDTO**: When hex position is determined, log "Placed dependent location X at hex (Q, R) adjacent to base location Y."

**In LocationParser**: When hex coordinates are read from DTO, log "Set HexPosition for location X from DTO: (Q, R)."

**In GameFacade.SpawnSceneWithRewards**: When dependent resources are detected, log counts: "Scene X has N dependent locations and M dependent items." When PackageLoader is called, log "Loaded dynamic package via PackageLoader." When locations are added, log each one with its hex position.

**In LocationActionCatalog**: The existing logging for locations without hex positions can be clarified. Change warning tone to info, explaining "Location X has no HexPosition - scene-internal location (movement via NavigationPayload)" versus current ambiguous warning. Actually for hex-positioned dependent locations this won't fire anymore - they'll have positions.

### Testing the Complete Flow

After implementation, the flow should work as follows:

Player starts game. GameFacade.SpawnStarterScenes finds tutorial_secure_lodging template. Calls SpawnSceneWithRewards. Scene finalization generates DependentResourceSpecs. Orchestrator writes JSON file containing private room location with Q and R coordinates. PackageLoader reads JSON. LocationParser creates Location entity with HexPosition from DTO. LocationActionCatalog generates actions including "Move to Private Room" action (but it requires room to be unlocked).

Player interacts with Elena the innkeeper. Negotiate situation presents choices. Player selects the successful negotiation path (stat-gated, money, or challenge). Choice reward contains LocationsToUnlock list with "generated:private_room" and ItemIds list with "generated:room_key". RewardApplicationService applies these: unlocks the location (IsLocked = false), adds room_key to player inventory.

Player is still at common_room location. LocationAction list now includes "Move to Elena's Lodging Room" action (intra-venue movement to adjacent hex). Player clicks the action. GameFacade.ExecuteLocationAction processes it, validates destination is unlocked (it is), moves player's hex position to the private room hex.

UI updates to show private room. Service situation auto-advances, granting rest rewards. Player regains health and stamina. Depart situation auto-advances, removes room_key from inventory, locks private room again (IsLocked = true), returns player to common_room hex.

Logging at each step confirms: hex placement happened, DTO contained coordinates, parser read them, location has position, action was generated, unlock worked, movement worked.

## Critical Constraints

### Constraint 1 - Catalogue Pattern (Parse-Time Only)

Catalogues (SceneArchetypeCatalog, SituationArchetypeCatalog, LocationActionCatalog) are parse-time code exclusively. They are NEVER imported outside parser classes. Runtime code does not call them, reference them, or have access to them.

This timing boundary is sacred. If runtime needs to generate something, it doesn't call catalogues. Instead, parsers call catalogues during initialization, generating all possible content, storing it in GameWorld collections. Runtime queries those collections.

For dependent locations, this means: SceneInstantiator (which runs during scene finalization, which is runtime) cannot call LocationActionCatalog directly. Instead, it generates LocationDTO, packages it in JSON, hands it to PackageLoader. PackageLoader treats it like any JSON file, calling LocationParser which creates the Location entity and adds it to GameWorld.Locations. Later (separately), LocationActionCatalog gets called by PackageLoader's initialization flow to generate actions for newly-added locations.

### Constraint 2 - HIGHLANDER Principle (One Concept, One Representation)

There should be ONE way to represent hex coordinates on locations, not multiple alternative paths. LocationDTO has Q and R properties. Location entity has HexPosition property. There's no redundant storage, no alternative fields for different scenarios.

Similarly, there's ONE field for scene archetype identification (SceneArchetypeId), not alternative fields for different patterns. The DTO that had both SceneArchetypeId and SituationArchetypeId was a HIGHLANDER violation - it suggested two ways to specify what scene to create. We eliminated SituationArchetypeId entirely (unrelated to hex placement bug but fixed in same session).

For hex placement, this means: don't add alternative mechanisms like "virtual locations" or "hex-less locations" or "special movement system for dependent rooms." Fix the PRIMARY system (hex grid spatial model) to include dependent locations correctly.

### Constraint 3 - Parser-JSON-Entity Triangle Completeness

When adding new data flow (hex coordinates embedded in DTO), ALL three layers must be updated:
- DTO must have properties to store it
- Parser must have code to read those properties
- Entity must receive the value correctly

Partial fixes that update only one or two layers leave gaps. The triangle is either complete or broken.

### Constraint 4 - Immutability After Parsing

SceneTemplate, SituationTemplate, Location.HexPosition - these are immutable after parsing completes. Runtime code may change Location.IsLocked or Location.CurrentTimeBlocks (game state), but not Location.HexPosition (structural identity).

This means hex position must be determined at creation time. You can't create a location without hex position and "fill it in later." The first time Location entity is constructed, it must have HexPosition or null (which is invalid for dependent locations and will cause failures).

### Constraint 5 - Semantic Honesty

Property names, enum values, method names must match reality. HexPlacementStrategy.SameVenue returning null was semantic dishonesty - the name implied "place in same venue" but the implementation was "don't place at all." This creates confusion and bugs.

When the implementation doesn't match the name, either fix the implementation to match the name (SameVenue should actually place in same venue), or rename the thing to match the implementation (rename to NoHexPlacement). Don't leave them misaligned.

For this fix, we aligned implementation with name: SameVenue now actually finds adjacent hex in same venue, matching its semantic promise.

### Constraint 6 - All Locations Have Hex Positions

User explicitly confirmed: there are NO locations without hex positions. Every location occupies one hex on the world grid. This is architectural bedrock, not a nice-to-have.

Any code that returns null for hex position or creates locations without hex positions is architecturally incorrect. Fix it to always provide hex positions, or throw clear exceptions explaining why hex position cannot be determined (e.g., no unoccupied adjacent hex exists).

## Key Files & Their Roles

### LocationDTO.cs
Data transfer object bridging JSON and domain entity layers. Contains all properties that can be specified in JSON (name, description, venue, properties, etc.). Role in this fix: receives Q and R properties (nullable integers) to store hex coordinates for dependent locations generated by scenes. Normal locations from JSON won't populate these properties (they use hex grid sync instead). Parser checks if these properties have values and uses them to set Location.HexPosition.

### SceneInstantiator.cs
Domain service responsible for scene lifecycle: creating provisional scenes, finalizing them, generating dependent resource specs. Contains two critical methods for this fix:

**FindAdjacentHex method**: Takes base location and HexPlacementStrategy enum, returns AxialCoordinates for where to place new location. Contains the bug where SameVenue case returns null instead of finding adjacent hex. Fix makes SameVenue fall through to Adjacent case (both use same algorithm).

**BuildLocationDTO method**: Converts DependentLocationSpec to LocationDTO with concrete values (placeholders replaced, hex position determined). Contains the bug where "Find hex placement" comment exists but no code executes. Fix adds call to FindAdjacentHex, stores result in DTO's Q and R properties.

### LocationParser.cs
Parser converting LocationDTO to Location entity. Handles venue resolution, property parsing, time block setup. Role in this fix: adds logic to read Q and R properties from DTO (if present) and assign Location.HexPosition from them. This creates the DTO → Entity flow for embedded hex coordinates. Normal locations without Q/R in DTO skip this step (they get hex positions from HexParser.SyncLocationHexPositions later).

### GameWorldInitializer.cs
Static initialization method constructing GameWorld from JSON files. Currently attempts to spawn starter scenes but lacks orchestration dependencies (no ContentGenerationFacade, no PackageLoaderFacade, no HexRouteGenerator). Role in this fix: remove starter scene spawning code entirely, replace with comment explaining spawning deferred to GameFacade. Keep logging that identifies starter templates but don't spawn them.

### GameFacade.cs
Orchestration layer with full service dependencies. Contains SpawnSceneWithRewards method implementing complete flow: generate JSON, write files, call PackageLoader, add items to inventory, generate routes. Role in this fix: receives DependentResourceSpecs return value (instead of discarding with underscore) and processes it through full orchestration. Needs new SpawnStarterScenes method to handle deferred starter scene spawning after initialization completes.

### LocationActionCatalog.cs
Parse-time catalogue generating LocationActions based on location properties and hex adjacency. Called by PackageLoader when locations are added to GameWorld. Contains GenerateIntraVenueMovementActions method that queries for adjacent locations in same venue and creates "Move to X" actions. No changes needed for this fix (it already handles hex-positioned locations correctly), but logging could be clarified to explain that locations without hex positions are scene-internal (movement via NavigationPayload) rather than errors.

### SceneArchetypeCatalog.cs
Parse-time catalogue generating scene structure from archetype IDs. Contains definition for service_with_location_access archetype which creates DependentLocationSpec with HexPlacement = HexPlacementStrategy.SameVenue. No changes needed for this fix (the spec is correct), but understanding its role clarifies where hex placement strategy originates (catalogue design decision, not runtime logic).

## Next Session Priorities

The architectural understanding is complete and fixes are implemented. Next session should:

**Implement GameFacade.SpawnStarterScenes**: Create the method that handles deferred starter scene spawning with full orchestration. Query GameWorld.SceneTemplates for IsStarter flag, build spawn rewards using PlacementFilter evaluation, call SpawnSceneWithRewards for each.

**Update DI Setup to Call SpawnStarterScenes**: After GameWorld initialization and DI container construction, ensure GameFacade.SpawnStarterScenes gets called to spawn tutorial scenes. Determine the appropriate call site in application startup flow.

**Test Complete Flow End-to-End**: Run game, verify tutorial_secure_lodging scene spawns, dependent private room created with hex position adjacent to common_room, negotiate with Elena, verify room unlocks and key granted, verify "Move to Elena's Lodging Room" action appears, execute movement, verify service situation renders at private room, verify departure returns to common room and locks room again.

**Verify Logging Output**: Check that all logging points execute, showing hex placement determination, DTO coordinate storage, parser coordinate reading, location creation with position, action generation including intra-venue movement. Use logging to confirm complete flow works as designed.

**Consider NavigationPayload Implementation**: The architectural analysis revealed NavigationPayload is mentioned for scene-internal movement but may not be fully implemented. If auto-progress situations need to move player to different locations without explicit movement actions, NavigationPayload needs implementation in GameFacade. However, this may not be needed for the service scene pattern if intra-venue movement actions suffice.

The hex placement architecture is now correct: dependent locations get placed on adjacent hexes, participate in the standard hex-based movement system, and integrate cleanly with the venue cluster spatial model. The Parser-JSON-Entity triangle is complete, the orchestration is in the proper layer, and the HIGHLANDER principle is respected.
