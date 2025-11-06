# HANDOFF: Scene Spawn Context Building Architecture & HIGHLANDER Refactoring

## Problem & Motivation

### What Are We Solving?

The game was unplayable from start. Starter scenes never spawned, making the tutorial flow completely broken. Elena (the innkeeper NPC) showed no interaction options, and the private room that should be generated as part of the tutorial simply didn't exist. The player could load the game but had no path forward - a critical PLAYABILITY OVER COMPILATION failure.

The root cause analysis revealed two distinct but related problems:
1. Immediate technical issue: Missing data in NPC configuration causing runtime context resolution to fail
2. Deeper architectural flaw: Context building logic duplicated across six different locations in the codebase with inconsistent implementations

### Why Does It Matter?

This represents a pattern violation that affects game stability and maintainability. The HIGHLANDER principle states "one concept, one implementation" - but context building had six implementations with subtle behavioral differences. This creates:
- Maintenance burden (fix same bug six times)
- Inconsistency risk (implementations drift apart over time)
- Testing complexity (six code paths to verify)
- Onboarding friction (new developers see multiple patterns for same operation)

From a player experience perspective, the tutorial is the critical first impression. A broken tutorial means no player can experience the game, making all other content worthless regardless of quality.

---

## Architectural Discovery

### The Hex-Based Spatial Foundation

Wayfarer uses a hex grid as the fundamental spatial layer. Every location MUST occupy exactly one hex cell identified by axial coordinates (Q, R). This isn't optional - the travel system, route generation, and location actions all depend on hex positions being present.

Venues represent clusters of related locations (typically seven hexes: one center plus six adjacent). Locations within the same venue can be adjacent on the hex grid, enabling "intra-venue travel" which is instant and free. Locations in different venues are non-adjacent, requiring route-based "inter-venue travel" with resource costs.

### The Scene System Architecture

Scenes are the primary gameplay content delivery mechanism. A Scene is a collection of Situations (story beats) that present choices to the player. Scenes can be:
- **Starter scenes**: Spawned once during game initialization to provide tutorial content
- **Procedural scenes**: Spawned dynamically based on game state triggers (time, location, NPC interactions)
- **Reward scenes**: Spawned as consequences of completing obligations or other story events

### Provisional vs Finalized Scenes

Scenes follow a two-phase lifecycle:
1. **Provisional**: Scene template instantiated with basic structure but no generated content
2. **Finalized**: Situations instantiated, dependent resources (locations, items) generated, scene becomes active

The finalization phase is where dependent resource generation happens. A scene can specify that it needs additional locations (like a private room) or items (like a room key). These dependent resources must be:
- Generated dynamically with unique IDs
- Placed on the hex grid with valid coordinates
- Loaded into GameWorld so they're accessible
- Properly linked to the parent scene

### The Context Pattern

SceneSpawnContext is a data structure that carries information about WHERE and HOW a scene should spawn. It contains:
- **Player**: The player entity (always required)
- **CurrentLocation**: Location where scene takes place (required for dependent location generation)
- **CurrentNPC**: NPC involved in the scene (optional, depends on placement type)
- **CurrentRoute**: Travel route where scene occurs (optional, for journey encounters)
- **CurrentSituation**: Parent situation that triggered spawn (optional, null for starter/procedural scenes)

The context serves multiple purposes:
- Resolves placement ambiguity (which NPC? which location?)
- Provides spatial anchoring for dependent resource generation
- Enables situational validation (can this scene spawn here?)
- Carries state for narrative customization

### The Parser-JSON-Entity Triangle

Content flows through three distinct layers:
1. **JSON Layer**: Hand-authored data files define entities with string IDs for cross-references
2. **Parser Layer**: Translates JSON into domain entities, resolves ID references to object references
3. **Entity Layer**: Domain models with strong typing and object navigation

The parser layer is critical - it's responsible for taking string-based references (like locationId in JSON) and resolving them to actual object references. However, different properties serve different purposes:
- **locationId**: Sets the Location object reference (resolved at parse time, used for static associations)
- **workLocationId**: String ID used at runtime to resolve dynamic context (where is this NPC currently working?)
- **homeLocationId**: String ID used at runtime to resolve fallback context (where does this NPC live?)

This dual system exists because parse-time resolution can't handle dynamic scenarios. An NPC might reference a location that doesn't exist yet (because it will be generated by a scene). The runtime IDs enable late binding.

### Orchestration vs Domain Logic

The codebase follows a clear separation:
- **Domain Services**: Pure logic operating on entities (SceneInstantiator, SpawnConditionsEvaluator)
- **Facades**: Orchestration layer coordinating multiple services (GameFacade, SceneFacade, ContentGenerationFacade)
- **Instantiators**: Entity creation and lifecycle management (Scene creation, finalization)
- **Catalogues**: Parse-time translation from categorical JSON properties to concrete runtime properties

GameFacade sits at the top of the orchestration hierarchy. It's the SOLE orchestrator for operations that span multiple subsystems. When a scene spawns with dependent resources, GameFacade coordinates:
1. Scene instantiation via SceneInstanceFacade
2. Content generation via ContentGenerationFacade (creates dynamic JSON package)
3. Package loading via PackageLoaderFacade (loads generated content into GameWorld)
4. Route generation via HexRouteGenerator (creates travel routes to new locations)

This orchestration pattern prevents circular dependencies and ensures clean separation of concerns.

---

## Domain Model Understanding

### NPC Entity Structure

NPCs have three location-related properties that serve different purposes:

**Location (object reference):**
- Resolved at parse time from locationId in JSON
- Used for static associations (this NPC is associated with this location)
- Not suitable for runtime context resolution (can't change dynamically)

**WorkLocationId (string):**
- Runtime property for dynamic context resolution
- Represents where the NPC is currently working
- Used when spawning scenes that need to know the NPC's spatial context

**HomeLocationId (string):**
- Fallback for runtime context resolution
- Represents the NPC's home base
- Used when WorkLocationId is null or when seeking the NPC outside work hours

The pattern is: resolve Location object at parse time for static associations, use WorkLocationId/HomeLocationId at runtime for dynamic context building.

### Scene Placement Types

Scenes can be placed in the world using different strategies:

**SpecificNPC**: Scene spawns at a specific named NPC (concrete binding for tutorial patterns)
**SpecificLocation**: Scene spawns at a specific named location
**SpecificRoute**: Scene spawns on a specific named travel route
**Generic**: Scene spawns wherever spawn conditions match (categorical search)

The placement type determines what context properties must be populated. SpecificNPC placement requires CurrentNPC AND CurrentLocation (the NPC's location). SpecificLocation requires only CurrentLocation. SpecificRoute requires CurrentRoute and potentially CurrentLocation (route's origin).

### Dependent Resource Generation

When a scene finalizes, it can generate dependent resources through DependentResourceSpecs. This specification contains:
- **LocationDTOs**: Generated location definitions with unique IDs
- **Item definitions**: Generated items (like keys) that unlock locations
- **Package JSON**: Complete JSON structure wrapping all generated content
- **Created IDs lists**: Tracking which locations/items were generated for later reference

The generation process uses VenueIdSource to determine how to assign venue IDs to dependent locations:
- **SameAsBase**: Use the same venue as the base location (context.CurrentLocation.VenueId)
- **GenerateNew**: Create a new venue (not yet implemented)

The hex placement strategy determines spatial positioning:
- **Adjacent**: Place on an unoccupied hex adjacent to the base location
- **SameVenue**: Place on an adjacent hex in the same venue (currently same as Adjacent)

The critical invariant: ALL locations must have hex positions. No exceptions. If a dependent location can't find an adjacent hex (all occupied), the system throws an exception rather than creating a position-less location.

---

## Current State Analysis

### Six Context Building Implementations

Investigation revealed context building logic duplicated across six locations:

**GameFacade.SpawnStarterScenes**: Builds context for tutorial scene spawning during initialization. Resolves SpecificNPC → Location, SpecificLocation → Location, SpecificRoute → Route.

**ObligationActivity.BuildContextForObligationReward**: Builds context for obligation reward scenes. Most complete implementation - resolves NPC → Location, Location directly, Route → Location (via Route.OriginLocation).

**RewardApplicationService.FinalizeSceneSpawns**: Builds context when finalizing provisional scenes from action rewards. Extracts placement from parent Situation, resolves entities, but does NOT resolve NPC → Location.

**SceneFacade.BuildSpawnContext**: Builds context from parent Scene placement. Extracts placement from Scene, resolves entities, but does NOT resolve NPC → Location.

**SpawnFacade.BuildSpawnContext**: Minimal context builder for procedural spawning. Sets only Player, relies on SceneInstantiator to resolve placement from filters.

**LodgingSceneIntegrationTest**: Manual test construction. Directly sets all context properties without any resolution logic.

The inconsistency is clear: two implementations resolve NPC → Location correctly (GameFacade, ObligationActivity), two don't (RewardApplicationService, SceneFacade), one is intentionally minimal (SpawnFacade), and one is test-specific.

### The Data Issue Pattern

NPCs in the JSON have locationId for static parse-time associations but lack workLocationId and homeLocationId for runtime dynamic resolution. This creates a gap: the Entity has the Location object reference from parsing, but runtime code trying to resolve CurrentLocation from WorkLocationId/HomeLocationId gets null.

This pattern appears to be an oversight during NPC schema design. The locationId was added for parse-time object graph construction, but the runtime context building was added later and assumed WorkLocationId/HomeLocationId would be present.

### Existing Validation Infrastructure

The hex positioning system has comprehensive validation:
- Parse-time validation prevents duplicate hex coordinates
- Parse-time validation prevents duplicate locationId references
- Sync-time validation ensures all locations have hex positions after parser completes

But there's NO validation for NPC location references. The parser doesn't check that WorkLocationId/HomeLocationId exist or point to valid locations. Runtime code fails silently with null when these IDs are missing, leading to downstream crashes in dependent resource generation.

---

## Design Approach & Rationale

### The HIGHLANDER Principle

The fundamental design principle guiding this refactoring is HIGHLANDER: "one concept, one implementation." Context building is a single concept - given placement information, construct a properly populated SceneSpawnContext. Having six implementations violates this principle.

The correct pattern is a shared static utility class that centralizes the logic. Any code that needs to build a context calls this utility instead of implementing its own resolution logic.

### Why Static Utility vs Service Class?

The context building logic has no state and no external dependencies beyond GameWorld. It's a pure function: given inputs (GameWorld, placement details), produce output (SceneSpawnContext). This makes it ideal for a static utility class rather than a service that needs dependency injection.

Static utilities are appropriate when:
- No instance state required
- No external service dependencies
- Pure transformation logic
- Used across multiple layers (orchestration, services, tests)

### Location Choice: Content Folder

The SceneSpawnContextBuilder utility should live in the Content folder alongside SceneSpawnContext itself. This follows the principle: "data structures and their construction logic belong together."

The Content folder is architecture-neutral. It contains domain entities and their associated infrastructure but has no dependencies on higher-level services or facades. This makes the utility accessible from anywhere: orchestration layer (GameFacade), service layer (ObligationActivity), tests.

### Resolution Strategy

The shared builder uses a switch on PlacementRelation:

**SpecificNPC case**: Most complex resolution
- Find NPC entity by ID
- Extract WorkLocationId ?? HomeLocationId (fallback pattern)
- Find Location entity by the resolved ID
- Set both CurrentNPC and CurrentLocation in context
- Log warning if NPC exists but has no location IDs

**SpecificLocation case**: Direct resolution
- Find Location entity by ID
- Set CurrentLocation in context

**SpecificRoute case**: Route with origin location
- Find Route entity by ID
- Set CurrentRoute in context
- If route has OriginLocation, set CurrentLocation as well
- This enables spawn contexts that need both route and spatial anchoring

**Generic/Same* cases**: Minimal context
- Return context with only Player set
- Let SceneInstantiator handle placement resolution from filters
- These cases rely on categorical filtering rather than concrete binding

### Data Fix Strategy

The NPC JSON must be updated to include workLocationId and homeLocationId. The pattern is:
- workLocationId: Where the NPC is currently working (context for work-related scenes)
- homeLocationId: Where the NPC lives (fallback, context for home-related scenes)

For tutorial NPCs that work and live in the same location (Elena at the inn), both properties can point to the same location ID. This provides runtime resolution without complex logic.

The fix is data-only. No parser changes needed - the properties already exist in the NPC entity class, they're just not populated from JSON.

---

## Implementation Strategy

### Phase 1: Create Shared Utility

Build the SceneSpawnContextBuilder static class in the Content folder. The class should be minimal - a single public static method and no instance state.

The method signature accepts all necessary inputs: GameWorld for entity lookups, Player for context population, PlacementRelation and SpecificPlacementId for resolution strategy, and optional CurrentSituation for inherited context.

The implementation is a straightforward switch statement. Each case performs entity resolution using LINQ FirstOrDefault queries against GameWorld collections. The pattern matches existing code in ObligationActivity but consolidates it into reusable form.

Logging should be added for edge cases: NPC exists but has no location IDs, Location ID exists but location not found, Route ID exists but route not found. These warnings help diagnose data issues without crashing the game.

### Phase 2: Update Data

Modify the NPC JSON to add workLocationId and homeLocationId to all NPCs. For Elena specifically, both should point to common_room since she works and lives at the inn.

This change is JSON-only. The parser already handles these properties - they're defined in NPCDTO and mapped to NPC entity properties. The parser just hasn't been receiving these values from JSON.

### Phase 3: Refactor GameFacade

Replace the switch statement in SpawnStarterScenes with a single call to SceneSpawnContextBuilder.BuildContext. Pass in the GameWorld, player, placementRelation, and specificPlacementId that were already being used.

Remove the debug logging statements that were added during investigation. The shared builder will have its own logging.

### Phase 4: Refactor ObligationActivity

The existing BuildContextForObligationReward method becomes redundant. Delete the entire method.

Update the call site in ProcessObligationReward to directly call SceneSpawnContextBuilder.BuildContext instead. The parameters are already available at the call site.

### Phase 5: Consider Other Call Sites

RewardApplicationService and SceneFacade currently have partial context building that doesn't resolve NPC → Location. Evaluate whether they need the fix:
- If they spawn scenes with dependent locations using VenueIdSource.SameAsBase, they MUST resolve NPC → Location
- If they only spawn simple scenes without dependent locations, the current minimal context is acceptable

SpawnFacade should remain minimal - its pattern is intentionally different (categorical spawning, not concrete binding).

### Phase 6: Verification

Build the game and start from fresh initialization. The expected flow:
1. GameWorld initializes from JSON files
2. StartGameAsync calls SpawnStarterScenes
3. SpawnStarterScenes builds context using shared builder
4. Elena NPC found, WorkLocationId="common_room" resolved, CurrentLocation set
5. SpawnSceneWithDynamicContent creates provisional scene
6. FinalizeScene generates dependent location spec
7. DetermineVenueId uses context.CurrentLocation.VenueId successfully
8. BuildLocationDTO finds adjacent hex successfully
9. Dependent location created with hex position
10. Dynamic package generated and loaded
11. Elena has active scene with situations
12. Player can interact with Elena

---

## Critical Constraints

### HIGHLANDER Enforcement

Once the shared builder exists, NO new context building implementations should be added. Any future code that needs a SceneSpawnContext must call the shared builder. This is a hard architectural rule.

### No Null Coalescing in Runtime Context

The pattern "resolve ID, return null if not found" is intentional. The system should fail fast with clear errors rather than silently continuing with null context properties. If an NPC is referenced but has no location, the warning should be logged but the context still returned. Let the downstream code (DetermineVenueId) crash with a specific error rather than trying to paper over the data issue.

### Parser-Time vs Runtime Resolution

The distinction between parse-time and runtime resolution must be preserved:
- locationId → Location object reference: Parse-time, for static associations
- workLocationId/homeLocationId → Location resolution: Runtime, for dynamic context

Don't try to consolidate these. They serve different purposes and operate at different phases of the system lifecycle.

### Orchestration Layer Responsibility

Context building with entity resolution is orchestration-level logic. Domain services (SceneInstantiator) should receive fully populated contexts, not null properties they have to resolve themselves. This keeps domain logic pure and orchestration concerns isolated.

### Data Integrity Over Code Flexibility

If NPC data is missing required properties, the correct fix is updating the data, not adding fallback logic in code. Code that tries to gracefully handle missing data creates technical debt and hides data quality issues. Fail fast, fix data, enforce quality.

---

## Key Files & Their Roles

### Content/SceneSpawnContext.cs
Defines the context data structure. Contains Player, CurrentLocation, CurrentNPC, CurrentRoute, CurrentSituation properties. This is a pure data carrier with no behavior.

### Content/SceneSpawnContextBuilder.cs (NEW)
Static utility class with single method BuildContext. Centralizes all entity resolution and context population logic. Used by any code that needs to construct a SpawnContext from placement information.

### Content/Core/03_npcs.json
NPC definitions including Elena. Contains locationId for parse-time associations and (after fix) workLocationId/homeLocationId for runtime resolution.

### Services/GameFacade.cs
Orchestration layer coordinating scene spawning. Method SpawnStarterScenes builds contexts and calls SpawnSceneWithDynamicContent. Modified to use shared context builder instead of duplicate switch logic.

### Services/ObligationActivity.cs
Service handling obligation phase completion rewards. Method ProcessObligationReward spawns scenes as rewards. Modified to use shared context builder instead of separate BuildContextForObligationReward method.

### Content/SceneInstantiator.cs
Domain service for scene lifecycle management. Methods CreateProvisionalScene and FinalizeScene use the context to resolve placement and generate dependent resources. Uses context.CurrentLocation for VenueIdSource.SameAsBase resolution in DetermineVenueId.

### Content/Parsers/HexParser.cs
Parser for hex grid data. Contains validation methods that ensure hex positions are unique and all locations have spatial presence. Serves as architectural model for data integrity validation.

### Subsystems/Spawn/SpawnFacade.cs
Service for procedural scene spawning. Method BuildSpawnContext intentionally minimal (Player only) because it relies on categorical filtering rather than concrete placement.

---

## Next Session Priorities

### Immediate Tasks

Implement the five-phase refactoring plan: create shared builder, fix data, refactor GameFacade, refactor ObligationActivity, verify end-to-end flow.

### Validation Points

Confirm starter scene spawns successfully. Confirm dependent room has hex position. Confirm Elena presents "Talk" action. Confirm tutorial flow is playable from start to room acquisition.

### Technical Debt Resolution

Document the pattern: workLocationId/homeLocationId must be present for all NPCs that appear in scenes with dependent locations. Consider adding parser validation to enforce this constraint.

### Architecture Evolution

Consider whether other context-like patterns exist in the codebase that could benefit from similar HIGHLANDER enforcement. The shared builder pattern is reusable for any scenario where entity resolution from IDs needs to be centralized.
