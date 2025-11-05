# Self-Contained Scene Package Architecture

## Executive Summary

This document describes the architectural pattern for **self-contained scenes** - narrative packages that dynamically create their own dependent resources (locations, items) without hardcoded references to pre-existing world entities.

**Core Principle:** Scenes define resource specifications → Generate JSON packages at finalization → Load through standard pipeline → Parsers create entities in GameWorld → Scene tracks created IDs for cleanup.

This maintains architectural uniformity: ALL content flows through the JSON → DTO → Domain Entity pipeline, whether static (parse time) or dynamic (runtime).

---

## Problem Statement

### Fragility of External Dependencies

**Current Pattern (Fragile):**
- Scene references "tavern_upper_floor" location by hardcoded ID
- Location must pre-exist in static JSON packages
- Broken if location missing, renamed, or modified
- Scene cannot control location lifecycle or properties
- Tutorial flow depends on pre-authored content existing

**Desired Pattern (Robust):**
- Scene declares: "I need a private room adjacent to base location"
- Scene creates that location when spawned
- Scene controls all properties (locked state, description, hex placement)
- Scene cleans up location when completed
- Zero external dependencies except NPC + base location from PlacementFilter

### Self-Containment Goal

Scenes become **true packages** that:
1. Accept minimal external context (NPC matching personality, base location matching services)
2. Create all dependent resources at spawn time
3. Manage complete lifecycle (creation → usage → cleanup)
4. Leave no orphaned resources
5. Work identically for tutorial and procedural content

---

## Architecture Overview

### Three-Phase Content Pipeline

**Wayfarer's Content Model:**

```
Phase 1: JSON (Human/AI Authoring)
   ↓
Phase 2: DTO (Deserialization)
   ↓
Phase 3: Domain Entity (Parsing & Validation)
   ↓
GameWorld Collections (Single Source of Truth)
```

**Critical Insight:** Dynamic content MUST follow this same pipeline, not bypass it with direct C# entity creation.

### Package-Based Resource Creation

**Flow:**
1. **Specification** - Scene defines what resources it needs
2. **Generation** - Build valid JSON package with those resources
3. **Loading** - PackageLoader processes JSON via standard pipeline
4. **Parsing** - Existing parsers convert DTOs to domain entities
5. **Registration** - Entities added to GameWorld collections
6. **Tracking** - Scene records created entity IDs
7. **Usage** - Scene situations reference created entities
8. **Cleanup** - Explicit removal/locking via ChoiceRewards

---

## Domain Model: Resource Specifications

### DependentLocationSpec

**Purpose:** Declarative specification for a location the scene needs to create.

**Properties:**
- **TemplateId** - Type identifier for this resource (e.g., "private_room", "bath_chamber")
- **NamePattern** - Template with placeholders (e.g., "{NPC.Name}'s Private Room")
- **VenueIdSource** - Where to get parent venue ("BaseLocation.VenueId")
- **HexPlacement** - Strategy for hex grid placement (Adjacent, SpecificDirection, Distance)
- **Properties** - Categorical location properties (sleepingSpace, restful, indoor)
- **IsLockedInitially** - Whether location starts locked

**Design Philosophy:**
- Declarative, not imperative
- Categorical properties (leverages existing property system)
- Context-aware (uses placeholders resolved at finalization)
- Flexible placement (multiple strategies for hex positioning)

### DependentItemSpec

**Purpose:** Declarative specification for an item the scene needs to create.

**Properties:**
- **TemplateId** - Type identifier for this resource (e.g., "room_key", "entry_token")
- **NamePattern** - Template with placeholders (e.g., "Key to {DependentLocation[0].Name}")
- **Categories** - Item categories (Key, Consumable, Valuable)
- **Weight** - Item weight for inventory management
- **BuyPrice/SellPrice** - Optional economic properties

**Design Philosophy:**
- Minimal required properties
- Category-based classification
- Cross-reference support (can reference created locations)
- Optional economic integration

### Scene Tracking

**Scene Entity Extensions:**
- **CreatedLocationIds** - List of location IDs this scene created
- **CreatedItemIds** - List of item IDs this scene created
- **DependentPackageId** - ID of generated package for forensics

**SceneTemplate Extensions:**
- **DependentLocations** - List of location specifications
- **DependentItems** - List of item specifications

**Purpose:** Enable forensic visibility and explicit cleanup without automatic hooks.

---

## Content Generation: Catalogue Pattern

### SceneArchetypeCatalog Integration

**Where:** SceneArchetypeCatalog.GenerateServiceWithLocationAccess()

**What:** Add resource specifications to generated SceneTemplate.

**DependentLocationSpec for Private Room:**
- TemplateId: "private_room"
- NamePattern: "{NPC.Name}'s Private Room"
- VenueIdSource: "BaseLocation.VenueId"
- HexPlacement: Adjacent
- Properties: ["sleepingSpace", "restful", "indoor"]
- IsLockedInitially: true

**DependentItemSpec for Room Key:**
- TemplateId: "room_key"
- NamePattern: "Key to {DependentLocation[0].Name}"
- Categories: ["Key"]
- Weight: 1

**Situation Integration:**
- Situation 2/3 RequiredLocationId: "generated:private_room" (marker)
- Situation 4 ItemsToRemove: ["generated:room_key"] (marker)
- Markers resolved to actual IDs after package generation

**Design Philosophy:**
- Catalogue generates specifications, not concrete entities
- Parse-time generation (same as static content)
- Specifications flow through SceneTemplate → Scene → Instantiator
- Consistent with existing catalogue pattern (categorical → concrete)

---

## Runtime Package Generation

### Package Building Flow

**Trigger:** SceneInstantiator.FinalizeScene() - when player selects provisional scene

**Process:**

1. **Check Specifications**
   - Read scene.Template.DependentLocations
   - Read scene.Template.DependentItems
   - Skip if both empty

2. **Generate DTOs**
   - For each DependentLocationSpec: Build valid LocationDTO
   - For each DependentItemSpec: Build valid ItemDTO
   - Generate unique IDs: "{templateId}_{sceneId}"
   - Replace placeholders in names using scene context
   - Find hex positions using HexMap.GetNeighbors()
   - Set all required DTO fields per parser expectations

3. **Build Package Object**
   - PackageId: "scene_{sceneId}_resources"
   - Metadata: Name, timestamp, author="Scene System"
   - Content: LocationDTO list, ItemDTO list
   - No StartingConditions (ignored for dynamic packages)

4. **Serialize to JSON**
   - Standard System.Text.Json serialization
   - CamelCase naming policy (matches static packages)
   - Indented for forensic readability

5. **Load Through PackageLoader**
   - Call PackageLoader.LoadDynamicPackageFromJson(json, packageId)
   - Existing parsers process DTOs
   - LocationParser creates Location entities
   - ItemParser creates Item entities
   - Entities added to GameWorld.Locations, GameWorld.Items
   - Skeletons created for missing references (e.g., missing venue)
   - Returns list of skeleton IDs

6. **Track Created Resources**
   - Add location IDs to scene.CreatedLocationIds
   - Add item IDs to scene.CreatedItemIds
   - Store scene.DependentPackageId
   - Enables forensic analysis and explicit cleanup

7. **Generate Routes**
   - Call HexRouteGenerator.GenerateRoutesForNewLocation() per created location
   - Intra-venue instant travel routes auto-generated
   - Same logic as static location route generation

8. **Resolve Reference Markers**
   - Loop through scene situations
   - Replace "generated:private_room" with actual "{private_room}_{sceneId}"
   - Replace "generated:room_key" with actual "{room_key}_{sceneId}"
   - Enables situation templates to be specification-agnostic

### LocationDTO Building

**Input:** DependentLocationSpec + Scene context + GameWorld

**Process:**
- Generate ID from templateId + sceneId
- Replace name placeholders (NPC.Name, Location.Name)
- Query base location from scene placement
- Extract VenueId from base location
- Find adjacent hex via HexMap.GetNeighbors()
- Validate hex not occupied (throw if no valid placement)
- Set categorical properties from spec
- Set IsLocked from spec

**Output:** Fully populated LocationDTO matching parser expectations

### ItemDTO Building

**Input:** DependentItemSpec + Scene context

**Process:**
- Generate ID from templateId + sceneId
- Replace name placeholders (can reference DependentLocation[0].Name)
- Set categories from spec
- Set weight, buyPrice, sellPrice from spec
- Generate description mentioning scene origin (forensics)

**Output:** Fully populated ItemDTO matching parser expectations

### Placeholder Resolution

**Supported Placeholders:**
- {NPC.Name} - From scene placement NPC
- {NPC.Profession} - From scene placement NPC
- {Location.Name} - From scene base location
- {Venue.Name} - From base location's venue
- {DependentLocation[N].Name} - Cross-reference created locations
- {Player.Name} - Player character name

**Resolution Context:**
- SceneSpawnContext object contains all resolved entities
- PlaceholderReplacer utility handles substitution
- Circular references prevented (items can reference locations, not vice versa)

---

## Package Loading Infrastructure (Existing)

### PackageLoader.LoadDynamicPackageFromJson()

**Already Implemented** - No changes needed

**Capabilities:**
- Deserializes JSON string to Package object
- Checks packageId to prevent duplicate loading
- Calls LoadPackageContent(package, allowSkeletons: true)
- Dynamic packages allow skeletons (missing references create placeholders)
- Returns list of skeleton IDs for AI completion

**Difference from Static Loading:**
- Static: allowSkeletons=false, throws on missing references
- Dynamic: allowSkeletons=true, creates skeletons automatically
- Static: Catalogues run after all packages loaded
- Dynamic: Catalogues do NOT re-run (already executed)

### Parser Infrastructure (Existing)

**Already Implemented** - No changes needed

**Parsers:**
- LocationParser.ConvertDTOToLocation() - DTO → Location
- ItemParser.ConvertDTOToItem() - DTO → Item
- VenueParser.ConvertDTOToVenue() - DTO → Venue (if skeleton needed)

**Responsibilities:**
- Validate required fields (throw if missing)
- Parse categorical strings to enums
- Resolve object references via GameWorld lookups (HIGHLANDER Pattern A)
- Apply defaults for optional fields
- Return fully initialized domain entity

**Design Philosophy:**
- Stateless (no parser instance state)
- Fail fast (throw on invalid data)
- Trust entity initialization (collections initialized inline)
- Called only during package loading (parse-time equivalent)

### Skeleton System (Existing)

**Already Implemented** - No changes needed

**Purpose:** Handle missing references gracefully with mechanically-complete placeholders

**Capabilities:**
- SkeletonGenerator creates placeholder entities
- Deterministic (hash of ID for consistent randomness)
- Fully functional (game remains playable)
- Tracked in GameWorld.SkeletonRegistry
- Can be replaced when real content loads

**Usage in Dynamic Packages:**
- If created location references non-existent venue → skeleton venue created
- If created item references non-existent category (future) → skip gracefully
- Scene functionality preserved even with missing dependencies

---

## Hex Grid Integration

### Placement Strategies

**HexPlacementStrategy Enum:**
- **Adjacent** - Any of 6 adjacent hexes to base location
- **SpecificDirection** - Adjacent hex in specific direction (NE, E, SE, SW, W, NW)
- **Distance** - N hexes away in any direction
- **Random** - Random hex within radius

**Default:** Adjacent (simplest, most common use case)

### FindAdjacentHex() Logic

**Process:**
1. Get base location's HexPosition
2. Call HexMap.GetNeighbors(hexPosition)
3. Returns 0-6 adjacent hex coordinates
4. Filter to unoccupied hexes (no Location.HexPosition matches)
5. Select based on strategy:
   - Adjacent: First available
   - SpecificDirection: Match direction
   - Distance: Calculate distance, find match
   - Random: Random selection from valid set
6. Throw if no valid hex found (fail fast)

**Integration:**
- HexMap already provides GetNeighbors()
- Location.HexPosition is source of truth
- Hex.LocationId is derived (calculated lookup)
- HIGHLANDER: Location owns position, Hex derives it

### Route Generation

**HexRouteGenerator.GenerateRoutesForNewLocation()**

**Already Implemented** - No changes needed

**Process:**
1. Find all locations with different VenueId and HexPosition set
2. Use PathfindingService.FindPath() (A* hex pathfinding)
3. Create RouteOption with hexPath, segments, costs
4. Add to GameWorld.Routes

**Intra-Venue Travel:**
- Same venue = instant travel (no route needed)
- Automatically available when two locations share VenueId
- UI shows "Move to X" actions without cost
- Already implemented in LocationActionCatalog

**Design Philosophy:**
- Routes generated automatically for new locations
- No manual route authoring needed
- Consistent with static location route generation
- Called per created location (not batch)

---

## Reference Resolution System

### Marker Pattern

**Problem:** SituationTemplates can't reference specific generated IDs (unknown at catalogue generation time)

**Solution:** Use marker prefixes that resolve after package generation

**Marker Format:**
- "generated:{templateId}" - Resolves to actual generated entity ID
- Example: "generated:private_room" → "private_room_scene_42"

**Where Used:**
- SituationTemplate.RequiredLocationId
- SituationTemplate.RequiredNpcId
- ChoiceReward.ItemsToRemove
- ChoiceReward.LocationsToLock/Unlock

### Resolution Process

**Timing:** After package loading, before situation instantiation

**Process:**
1. Loop through scene.SituationIds
2. For each Situation, examine:
   - Template.RequiredLocationId
   - Template.RequiredNpcId
   - Template.ChoiceTemplates[].RewardTemplate fields
3. If value starts with "generated:":
   - Extract templateId from marker
   - Look up in scene.CreatedLocationIds (match templateId pattern)
   - Replace marker with actual generated ID
4. Situations now reference concrete entity IDs

**Design Philosophy:**
- Late binding (resolve after creation)
- Template-agnostic (same template works for all instances)
- Type-safe (markers replaced with actual IDs)
- Fail-fast (throw if marker doesn't match created resource)

---

## Lifecycle Management

### Creation Phase

**When:** SceneInstantiator.FinalizeScene() - player selects provisional scene

**Order:**
1. Generate and load dependent resource package
2. Parsers create entities in GameWorld
3. Track created IDs in scene
4. Resolve reference markers
5. Generate routes for created locations
6. Instantiate situations (now references valid)

**Result:** Scene ready for play with all dependencies satisfied

### Usage Phase

**During:** Scene progression through 4 situations

**Flow:**
- Situation 1: Negotiate at base location with NPC
- Situation 2: Move to created location (route exists, auto-navigation)
- Situation 3: Interact at created location
- Situation 4: Return to base location, cleanup executes

**Properties:**
- Created locations fully functional (fog of war, time windows, actions)
- Created items in inventory, tradeable, useable
- Routes enable instant intra-venue travel
- No mechanical difference from static content

### Cleanup Phase

**When:** Situation 4 completion (departure choice)

**Mechanism:** Explicit ChoiceReward fields

**ItemsToRemove:**
- Lists item IDs to remove from player inventory
- RewardApplicationService processes list
- Removes items from Player.Inventory collection
- Items remain in GameWorld.Items (forensic visibility)

**LocationsToLock:**
- Lists location IDs to relock
- RewardApplicationService processes list
- Sets Location.IsLocked = true
- Location remains in GameWorld.Locations (forensic visibility)

**Design Philosophy:**
- **Explicit over automatic** (superior per design docs)
- Cleanup visible in JSON content (author-controlled)
- Debuggable (can trace exactly what cleanup executes)
- Forensic visibility (entities remain, just inaccessible)
- No automatic hooks (no OnSceneComplete magic)

---

## Integration Points

### Dependency Injection

**SceneInstantiator Constructor Changes:**
- Inject PackageLoader (already exists, use directly)
- Inject HexRouteGenerator (already exists, use directly)
- Store as readonly fields
- No new dependencies (leverage existing infrastructure)

### FinalizeScene() Extension

**New Calls:**
1. After creating provisional scene object
2. Before InstantiateSituations()
3. Call: GenerateAndLoadDependentResources(scene, context)
4. Call: ResolveGeneratedReferences(scene)
5. Proceed with normal instantiation

**Error Handling:**
- Throw exceptions on generation failures (fail fast)
- Skeleton IDs logged for AI completion tracking
- Invalid placement throws clear error messages

### SceneArchetypeCatalog Extension

**GenerateServiceWithLocationAccess() Changes:**
- Add dependentLocations parameter to SceneTemplate initialization
- Add dependentItems parameter to SceneTemplate initialization
- Set marker references in situation templates
- Set marker references in cleanup rewards
- No changes to situation logic itself

---

## Design Patterns Preserved

### HIGHLANDER Principle

**One Concept, One Representation:**
- Location.HexPosition is authoritative (Pattern A: ID + Object)
- Hex.LocationId is derived (calculated from all Location.HexPosition)
- Scene.CreatedLocationIds stores IDs (Pattern C: ID Only for tracking)
- No duplicate state (IDs tracked for forensics, not navigation)

### LET IT CRASH Philosophy

**Fail Fast, No Null Checks:**
- FindAdjacentHex() throws if no valid placement
- Parser throws if required DTO fields missing
- Reference resolution throws if marker doesn't match
- Trust entity initialization (collections pre-initialized)

### Explicit Over Implicit

**Manual Cleanup Pattern:**
- ItemsToRemove explicitly lists IDs in JSON
- LocationsToLock explicitly lists IDs in JSON
- Author sees exactly what cleanup happens
- No automatic scene completion hooks
- Superior to automatic cleanup (per design docs)

### Three-Phase Processing

**JSON → DTO → Domain:**
- All content through same pipeline
- No C# direct entity creation
- Parser validation consistent
- Static and dynamic paths unified

### Catalogue at Parse Time

**Generation, Not Runtime:**
- SceneArchetypeCatalog generates specifications at parse time
- SceneInstantiator generates packages at finalization (parse-time equivalent)
- No runtime catalogue calls
- Consistent with "parse-time catalogues" principle

---

## Benefits Analysis

### Self-Containment Achieved

**External Dependencies:**
- ONLY: NPC matching personality filter
- ONLY: Base location matching service filter
- Zero hardcoded location/item references

**Internal Creation:**
- Locations created on demand
- Items created on demand
- Routes generated automatically
- Situations reference created resources

**Result:** Scene works in any context matching filter criteria

### Tutorial and Procedural Unification

**Same Pattern:**
- Tutorial scenes use specifications
- Procedurally-generated scenes use specifications
- Both flow through package generation
- Identical execution path

**Benefits:**
- No special tutorial code paths
- Tutorial content proves procedural system works
- AI can generate specifications (no C# entity knowledge needed)
- Validation unified (parsers check both)

### Forensic Visibility

**Package JSON Preserved:**
- Generated packages logged with scene ID
- Exact entities created visible in JSON
- Debugging trivial (check package JSON)
- Cleanup explicit (see what ItemsToRemove contains)

**Entity Tracking:**
- Scene.CreatedLocationIds lists all created locations
- Scene.CreatedItemIds lists all created items
- Scene.DependentPackageId references package
- Full audit trail for each scene

### Architectural Integrity

**No Pipeline Bypass:**
- All content through JSON → DTO → Domain
- Parsers validate everything
- HIGHLANDER pattern preserved
- No direct C# entity creation

**Existing Infrastructure:**
- PackageLoader unchanged
- Parsers unchanged
- DTOs unchanged
- Skeleton system unchanged
- Route generator unchanged

**Minimal Changes:**
- New: 3 specification classes
- Extended: 2 entity classes (SceneTemplate, Scene)
- Extended: 1 catalogue method
- Extended: 1 instantiator class
- Total: ~7 changed files

---

## Example Flow: "Secure Lodging" Scene

### Authoring (Parse Time)

**SceneArchetypeCatalog generates:**
- SceneTemplate with archetype "service_with_location_access"
- 4 SituationTemplates with context requirements
- DependentLocationSpec for private room (adjacent, sleepingSpace, locked)
- DependentItemSpec for room key (Key category)
- Marker references in situations ("generated:private_room")
- Cleanup markers in situation 4 ("generated:room_key" in ItemsToRemove)

**PlacementFilter:**
- NPC personality: Innkeeper
- Base location service: Lodging

### Spawning (Runtime - Scene Eligible)

**SceneInstantiator.CreateProvisionalScene():**
- Evaluates SpawnConditions (time, player state)
- Matches PlacementFilter against GameWorld
- Finds Elena (Innkeeper) at Common Room (Lodging service)
- Creates SHALLOW Scene (no situations, no resources yet)
- Adds to GameWorld.Scenes with State=Provisional
- Player sees scene in perfect information display

### Selection (Runtime - Player Chooses Scene)

**SceneInstantiator.FinalizeScene():**
1. Reads scene.Template.DependentLocations (1 spec: private_room)
2. Reads scene.Template.DependentItems (1 spec: room_key)
3. Calls GenerateAndLoadDependentResources():
   - Builds LocationDTO: "private_room_scene_42"
   - Name: "Elena's Private Room" (placeholder replaced)
   - VenueId: "brass_bell_inn" (from base location)
   - HexPosition: Adjacent hex found via GetNeighbors()
   - Properties: ["sleepingSpace", "restful", "indoor"]
   - IsLocked: true
   - Builds ItemDTO: "room_key_scene_42"
   - Name: "Key to Elena's Private Room"
   - Categories: ["Key"]
   - Weight: 1
   - Creates Package object with packageId "scene_scene_42_resources"
   - Serializes to JSON
   - Calls PackageLoader.LoadDynamicPackageFromJson()
   - LocationParser creates Location entity
   - ItemParser creates Item entity
   - Entities added to GameWorld collections
   - Routes generated via HexRouteGenerator
   - Tracks: scene.CreatedLocationIds = ["private_room_scene_42"]
   - Tracks: scene.CreatedItemIds = ["room_key_scene_42"]
4. Calls ResolveGeneratedReferences():
   - Finds "generated:private_room" in situation 2/3 RequiredLocationId
   - Replaces with "private_room_scene_42"
   - Finds "generated:room_key" in situation 4 ItemsToRemove
   - Replaces with "room_key_scene_42"
5. Instantiates situations (now references valid)
6. Sets scene.State = Active

### Progression (Runtime - Player Plays Scene)

**Situation 1 (Negotiate):**
- Location: Common Room (base location)
- NPC: Elena
- Player selects choice, pays costs
- Situation completes
- Scene.AdvanceToNextSituation() called
- Context comparison: Common Room → Private Room (different)
- RoutingDecision: ExitToWorld
- Modal exits, scene persists with CurrentSituationId=Situation2

**Player Navigation:**
- Player sees "Move to Elena's Private Room" action (route exists)
- Player selects move
- Instant intra-venue travel (same venue)
- LocationContent.RefreshLocationData() called
- SceneFacade.GetResumableModalScenesAtContext() finds scene
- Modal auto-resumes with Situation 2

**Situation 2 (Access):**
- Location: Elena's Private Room (created location)
- Player selects choice, pays costs
- Situation completes
- Context comparison: Private Room → Private Room (same)
- RoutingDecision: ContinueInScene
- Modal reloads seamlessly with Situation 3

**Situation 3 (Service):**
- Location: Elena's Private Room (created location)
- Player rests, recovers resources
- Situation completes
- Context comparison: Private Room → Common Room (different)
- RoutingDecision: ExitToWorld
- Modal exits, scene persists with CurrentSituationId=Situation4

**Player Navigation:**
- Player moves back to Common Room
- Scene auto-resumes with Situation 4

**Situation 4 (Depart):**
- Location: Common Room (base location)
- NPC: Elena
- Player selects final choice
- ChoiceReward processed:
  - ItemsToRemove: ["room_key_scene_42"] → Key removed from inventory
  - LocationsToLock: ["private_room_scene_42"] → Location.IsLocked = true
- Situation completes
- Scene complete (no more situations)
- Scene.State = Completed

### Result

**Created Resources:**
- Location "private_room_scene_42" exists in GameWorld (locked)
- Item "room_key_scene_42" removed from inventory (still in GameWorld for forensics)
- Route between Common Room ↔ Private Room exists (instant travel if unlocked)
- Package JSON "scene_scene_42_resources" logged

**Player Experience:**
- Negotiated with Elena → Gained access to private room → Rested → Concluded business
- Seamless flow with context-aware routing
- No hardcoded dependencies (works with any Innkeeper at any Lodging location)
- Clean lifecycle (key removed, room locked)

---

## Future Extensions

### Procedural Scene Generation

**AI Integration Points:**
- AI generates DependentLocationSpec properties (name patterns, properties)
- AI generates DependentItemSpec properties (categories, descriptions)
- AI generates situation narrative (uses {DependentLocation[0].Name} placeholders)
- AI unaware of concrete IDs (works at specification level)

**Example:** AI generates "Merchant's Storeroom Access" scene
- Learns: Merchant profession, Warehouse service → PlacementFilter
- Generates: DependentLocationSpec for back_room
- Generates: DependentItemSpec for warehouse_key
- Generates: 4 situations with negotiation → access → inspect → conclude
- System creates location, item, routes automatically
- Zero C# entity knowledge required from AI

### Multiple Dependent Resources

**Supported:** Lists of specifications

**Example:** Bath House Scene
- DependentLocation[0]: private_bath (adjacent)
- DependentLocation[1]: changing_room (adjacent to bath)
- DependentItem[0]: bath_token
- DependentItem[1]: towel
- Cross-references: "{DependentLocation[1].Name}" in item descriptions

**Hex Placement:**
- First location: Adjacent to base
- Second location: Adjacent to first (chain)
- Validation: Throw if no valid hex arrangement

### Cleanup Strategies

**Future Enhancement:** CleanupStrategy enum on specs

**Strategies:**
- PermanentLock: Lock forever (current default)
- TemporaryRemoval: Remove from GameWorld entirely
- Reusable: Keep unlocked (for repeatable scenes)
- TransferOwnership: Grant to different NPC

**Implementation:** Process in RewardApplicationService based on strategy

**Design Philosophy:** Still explicit (strategy declared in spec, visible in JSON)

### Advanced Placement

**Future Enhancement:** More HexPlacementStrategy options

**Strategies:**
- WithinRadius: Any hex N distance away
- SpecificCoordinates: Absolute hex position
- RelativeToOtherLocation: Offset from different location
- NearestToPlayer: Closest unoccupied hex to player position

**Implementation:** Extend FindAdjacentHex() switch

---

## Technical Constraints

### Package Loading Rules

**Package Cohesion:**
- All generated entities in same package
- No split across multiple packages
- Package loaded atomically (all or nothing)

**Idempotency:**
- Same packageId won't reload
- PackageLoader tracks _loadedPackageIds
- Prevents duplicate creation on scene replay

**Skeletons:**
- Dynamic packages use allowSkeletons=true
- Missing venue → skeleton venue created
- Missing category → graceful skip
- Game remains playable

### DTO Field Matching

**Critical:** JSON field names MUST match C# DTO properties exactly

**No JsonPropertyName Attributes:**
- DTOs use exact C# property names
- Serialization uses camelCase policy
- Example: locationId, venueId, hexPosition (all camelCase)

**Validation:**
- Parser throws on missing required fields
- Parser throws on invalid enum values
- Fail fast (don't limp along with bad data)

### Hex Grid Limitations

**Finite Space:**
- Hex grid has boundaries
- Adjacent hexes may not exist at edges
- Throw if no valid placement found

**Occupied Hexes:**
- Only one location per hex
- Filter out occupied hexes before selection
- Throw if all adjacent hexes occupied

**Solution:** Scenes should use reasonable placement (Adjacent typically succeeds)

### Parser Dependencies

**Order Matters:**
- Venues must exist before Locations
- If generated location references non-existent venue → skeleton created
- Skeletons resolved when real content loads

**Validation:**
- LocationParser validates VenueId references
- Creates skeleton if missing (dynamic packages)
- Throws if missing (static packages)

---

## Debugging & Forensics

### Package JSON Logging

**Location:** Logs or file system (TBD)

**Contents:**
- Full JSON package generated for scene
- Timestamp of generation
- Scene ID, template ID
- All created entity DTOs

**Usage:**
- Debug generation issues
- Verify DTO structure
- Audit what scene created

### Skeleton Registry

**Query:** GameWorld.SkeletonRegistry

**Contents:**
- List of SkeletonRegistryEntry
- Each has: SkeletonKey (ID), EntityType (type name)

**Usage:**
- Check if venue skeleton created
- Queue for AI completion
- Validate package dependencies

### Scene Tracking

**Query:** Scene.CreatedLocationIds, Scene.CreatedItemIds

**Contents:**
- Exact IDs of entities this scene created

**Usage:**
- Verify resources created correctly
- Debug reference resolution
- Audit cleanup execution

### Route Validation

**Query:** GameWorld.Routes filtered by origin/destination

**Contents:**
- Routes between base location and created locations

**Usage:**
- Verify automatic route generation
- Debug navigation issues
- Validate intra-venue travel

---

## Comparison: Static vs Dynamic Content

### Static Content (Current)

**Authoring:** Hand-written JSON files (01_foundation.json)
**Loading:** Parse time via GameWorldInitializer
**Validation:** Strict (throws on missing references)
**Catalogues:** Run ONCE after all packages loaded
**Lifecycle:** Permanent (exists from game start)
**References:** Pre-authored IDs in JSON

### Dynamic Content (This System)

**Authoring:** Generated DTOs → Serialized JSON
**Loading:** Runtime via PackageLoader.LoadDynamicPackageFromJson()
**Validation:** Permissive (creates skeletons for missing references)
**Catalogues:** Do NOT re-run (already executed at startup)
**Lifecycle:** Scene-scoped (created at finalization, cleaned at completion)
**References:** Marker-based resolution after generation

### Unified Pipeline

**Both Use:**
- Same Package/PackageContent structure
- Same DTO classes
- Same parsers (LocationParser, ItemParser)
- Same GameWorld collections
- Same HIGHLANDER patterns
- Same validation rules

**Difference:**
- Static: allowSkeletons=false
- Dynamic: allowSkeletons=true

---

## Summary

**Self-contained scenes** transform narrative packages from fragile reference-based to robust specification-based architecture.

**Key Innovation:** Scenes declare what they need (specifications) → System generates valid JSON packages → Existing pipeline creates entities → Scene tracks and cleans up.

**Architectural Integrity:** No pipeline bypass, no C# direct entity creation, uniform content model, minimal infrastructure changes.

**Benefits:** Tutorial/procedural unification, AI-friendly specification format, forensic visibility, explicit cleanup, zero hardcoded dependencies.

**Result:** Scenes become true packages that work anywhere, anytime, with any matching context, leaving no orphaned resources.
