# HANDOFF: Scene Spawning Context Building & Dynamic Location Actions

## Problem & Motivation

### What Are We Solving?

The game was unplayable from start with two critical issues preventing basic tutorial flow:

**Issue 1: Starter scenes never spawned**. Elena the innkeeper had no interaction options. The private room that should be generated as part of the tutorial didn't exist. Players could load the game but had no path forward.

**Issue 2: Dependent locations lacked travel actions**. Even when scenes did spawn and generate dependent locations, those locations appeared on the map but were unreachable - no intra-venue travel actions existed to navigate to them.

Root cause analysis revealed interconnected architectural problems:
1. Context building logic duplicated across six locations with inconsistent implementations
2. NPC location resolution attempted to use runtime properties that didn't exist, rather than parse-time resolved object references
3. LocationActions only generated at initial package load, never regenerated when dynamic content added locations at runtime

### Why Does It Matter?

This represents critical PLAYABILITY OVER COMPILATION failures. A game that compiles but prevents the first five minutes of gameplay is worse than a crash - players experience confusion and frustration rather than clear error messages.

The HIGHLANDER principle violation (six duplicate context builders) creates maintenance burden: fixing bugs requires updating six locations, implementations drift over time, testing complexity multiplies, onboarding friction increases. The action generation gap represents an incomplete feature implementation - the system generates dependent locations with routes but fails the final step of making them accessible.

From player experience, the tutorial is the critical first impression. Unreachable content regardless of reason makes all that content worthless. Forward progress must be guaranteed from every game state.

---

## Architectural Discovery

### The Parser-JSON-Entity Triangle Pattern

Content flows through three distinct layers with clear phase separation:

**JSON Layer**: Hand-authored data files define entities with string IDs for cross-references. locationId references locations, npcId references NPCs.

**Parser Layer**: Translates JSON into domain entities once at initialization. Resolves ID strings to object references through GameWorld lookups. This phase separation is critical - parsers run once, runtime code runs thousands of times.

**Entity Layer**: Domain models with strong typing and direct object navigation. Properties hold object references resolved during parsing.

The dual nature of location references exists because some scenarios need parse-time resolution (static associations) while others need runtime resolution (dynamic placement). The NPC entity has both:
- **Location property** (object reference): Resolved at parse time from locationId in JSON, immutable after parsing
- **WorkLocationId/HomeLocationId properties** (string IDs): For runtime dynamic resolution when location might change or not exist yet

The mistake was attempting runtime resolution when parse-time resolution already provided the needed data.

### Scene Spawning Architecture

Scenes are the primary gameplay content delivery mechanism. They follow a two-phase lifecycle:

**Provisional Phase**: Scene template instantiated with basic structure but no generated content. Placement determined, no situations created yet.

**Finalization Phase**: Situations instantiated with AI-generated narrative. Dependent resources (locations, items) generated if specified. Scene becomes active and visible to player.

The finalization phase handles dependent resource generation. A scene can specify it needs a private room. The system must:
- Generate LocationDTO with unique ID
- Determine venue ID (SameAsBase copies from context.CurrentLocation.VenueId)
- Find adjacent unoccupied hex position
- Create dynamic JSON package wrapping the location
- Load package into GameWorld via PackageLoader
- Generate hex routes connecting to existing locations
- Make location accessible through actions

The critical invariant: ALL locations must have hex positions. The travel system, route generation, and action placement all depend on spatial presence.

### SceneSpawnContext Pattern

SceneSpawnContext carries information about WHERE and HOW a scene spawns. Contains:
- **Player**: Always required
- **CurrentLocation**: Where scene takes place (required for dependent location generation)
- **CurrentNPC**: NPC involved (optional, depends on placement type)
- **CurrentRoute**: Travel route where scene occurs (optional, for journey encounters)
- **CurrentSituation**: Parent situation that triggered spawn (optional, null for starter/procedural)

The context must be fully populated before finalization because dependent resource generation needs CurrentLocation to determine venue ID through VenueIdSource.SameAsBase. A null CurrentLocation causes crashes during VenueIdSource resolution.

### Orchestration vs Domain Logic Separation

The codebase follows clear architectural layers:

**Domain Services**: Pure logic operating on entities. SceneInstantiator creates/finalizes scenes. SpawnConditionsEvaluator checks eligibility. These receive fully populated contexts.

**Facades**: Orchestration coordinating multiple services. GameFacade is SOLE orchestrator for cross-subsystem operations. SceneFacade handles scene-specific workflows. ContentGenerationFacade manages dynamic content creation.

**Context Builders**: Pure functions translating placement information into populated contexts. Stateless utilities with no external dependencies beyond GameWorld. Accessible from any layer.

GameFacade sits atop the orchestration hierarchy. When spawning scenes with dependent resources, it coordinates: scene instantiation, content generation, package loading, route generation. This prevents circular dependencies and maintains clean separation.

### The LocationAction System

Locations have two types of actions generated through different mechanisms:

**Property-Based Actions**: Generated by LocationActionCatalog based on LocationProperties enum flags. Crossroads property generates inter-venue "Travel to another location" action. Commercial property generates Work and Job Board actions. These are categorical - properties gate access to action types.

**Hex-Based Intra-Venue Actions**: Generated by examining hex grid adjacency relationships. For each location, find all adjacent hexes with locations in the same venue. Create IntraVenueMove action for each. Property-independent - spatial relationships alone determine generation.

The key insight: Property-based actions are feature gates (you need Crossroads to access long-distance travel). Intra-venue actions are navigation primitives (you can always walk to adjacent locations in the same venue). Mixing these two concerns was the source of confusion.

### Parse-Time vs Runtime Action Generation

LocationActions are generated once at parse time in PackageLoader.LoadAllPackages after all JSON packages loaded. The method GenerateLocationActionsFromCatalogue iterates all locations and generates both action types. This happens ONCE during initialization.

The gap: When dependent locations are added at runtime through scene finalization, they're added to GameWorld.Locations but LocationActions are never generated. The system assumed all locations come from JSON at startup. Dynamic content broke this assumption.

The routes exist (HexRouteGenerator creates them during dependent resource loading) but routes and actions are separate concepts. Routes enable pathfinding. Actions enable player interaction. You need both.

---

## Domain Model Understanding

### NPC Location References

NPCs have three location-related properties serving different purposes:

**Location (object reference)**: Resolved at parse time from locationId in JSON. Static association - this NPC works at/lives at this location. Immutable after parsing. Always populated if locationId present in JSON.

**WorkLocationId (string)**: Runtime property for dynamic context resolution. Where the NPC is currently working. Used when spawning scenes that need to know the NPC's spatial context. Optional - can be null.

**HomeLocationId (string)**: Fallback for runtime context resolution. The NPC's home base. Used when WorkLocationId null or when seeking the NPC outside work hours. Optional - can be null.

The pattern discovery: The initial implementation assumed WorkLocationId/HomeLocationId would always be populated and attempted runtime resolution. But the JSON never populated these fields - only locationId existed. The Location object reference was already available from parse-time resolution, making runtime resolution redundant.

The correct approach: Use npc.Location directly. It's already resolved, already populated, already points to the correct location. No need for string ID lookups at runtime.

### Scene Placement Types

PlacementRelation enum defines how scenes spawn:

**SpecificNPC**: Scene spawns at a concrete named NPC. Used for tutorial patterns with known NPCs. Requires resolving NPC entity and NPC's location for context.

**SpecificLocation**: Scene spawns at a concrete named location. Simpler - just resolve Location entity.

**SpecificRoute**: Scene spawns on a specific travel route. Resolve Route entity, optionally set CurrentLocation from Route.OriginLocation.

**SameLocation/SameNPC/SameRoute**: Context-relative placement. Scene spawns relative to current situation's context. Copy context properties from parent.

**Generic**: Categorical placement. Scene spawns at first entity matching PlacementFilter criteria (personality type, bond level, location properties). Runtime evaluation of filters.

The distinction matters for context building. Specific* placements need entity resolution from IDs. Same* placements inherit from parent context. Generic relies on filter evaluation.

### DependentResourceSpecs

When a scene needs to generate dependent resources, it produces a DependentResourceSpecs object containing:

**LocationDTOs**: List of LocationDTO definitions with unique IDs. Each specifies name, description, venue determination strategy, hex placement strategy, properties, locked state.

**Item Definitions**: Generated items like keys that unlock locations. Tied to specific locations.

**Package JSON Structure**: Complete JSON wrapping all generated content in Package format. Includes metadata.

**Created ID Lists**: Tracking which locations/items were generated for later reference and cleanup.

The generation process uses catalogues for property translation. VenueIdSource enum determines venue assignment:
- **SameAsBase**: Use CurrentLocation.VenueId from context - dependent location in same venue as base
- **GenerateNew**: Create new venue (not yet implemented)

HexPlacementStrategy determines spatial positioning:
- **Adjacent**: Find first unoccupied hex adjacent to base location
- **SameVenue**: Find adjacent hex in same venue (currently same as Adjacent)

The critical flow: DependentResourceSpecs → JSON Package → PackageLoader → GameWorld. The package becomes permanent content, indistinguishable from hand-authored JSON after loading.

### LocationActionCatalog Responsibilities

LocationActionCatalog has two distinct responsibilities that should NOT be confused:

**Property-Based Generation**: Examines LocationProperties enum flags. Generates actions based on property presence. Crossroads → inter-venue travel. Commercial → work and job board. Restful → rest action. These are FEATURE GATES - properties enable functionality.

**Hex-Based Generation**: Examines HexMap spatial relationships. Finds adjacent hexes with locations in same venue. Generates IntraVenueMove action for each. No properties involved - PURE SPATIAL NAVIGATION.

The method GenerateActionsForLocation does both. It checks properties, generates property-based actions. Then it checks hex adjacency, generates intra-venue actions. One method, two distinct responsibilities, different conceptual models.

The confusion arose because both property-based and hex-based actions affect travel. But they govern different travel types. Properties gate long-distance travel (Crossroads). Hex adjacency enables local movement (intra-venue). Don't conflate them.

---

## Current State Analysis

### Six Context Building Implementations

Investigation discovered context building logic duplicated across six locations with varying completeness:

**GameFacade.SpawnStarterScenes**: Builds context for tutorial scene spawning during initialization. Switch statement resolving Specific* placement types. Originally attempted npc.WorkLocationId resolution, causing null crashes.

**ObligationActivity.BuildContextForObligationReward**: Separate method building context for obligation reward scenes. Most complete implementation initially - resolved NPC → Location, handled all placement types. But still used WorkLocationId pattern.

**RewardApplicationService.FinalizeSceneSpawns**: Builds context when finalizing provisional scenes from action rewards. Extracts placement from parent Situation, resolves entities. Does NOT resolve NPC → Location (partial implementation).

**SceneFacade.BuildSpawnContext**: Builds context from parent Scene placement. Extracts placement from Scene, resolves entities. Does NOT resolve NPC → Location (partial implementation).

**SpawnFacade.BuildSpawnContext**: Minimal context builder for procedural spawning. Sets only Player, relies on SceneInstantiator to resolve placement from filters. Intentionally minimal - different pattern.

**LodgingSceneIntegrationTest**: Manual test construction. Directly sets all context properties without any resolution logic. Test-specific, not production code.

The pattern: Two implementations attempted full resolution with bugs. Two had partial implementation missing NPC location resolution. One was intentionally minimal with different purpose. One was test scaffolding.

This violates HIGHLANDER - one concept should have one implementation. Each implementation was a potential bug source. Fixing the bug required updating multiple locations or risking inconsistency.

### PackageLoader Action Generation Timing

PackageLoader.LoadAllPackages follows this sequence:
1. Load all JSON packages from directory
2. Parse each package through LoadPackageContent
3. Sync location hex positions via HexParser
4. Generate delivery jobs via DeliveryJobCatalog
5. Generate procedural routes via HexRouteGenerator
6. **Generate location actions via GenerateLocationActionsFromCatalogue**
7. Log initialization complete

The final step GenerateLocationActionsFromCatalogue iterates GameWorld.Locations and generates actions for each. This happens ONCE after ALL packages loaded.

The gap: PackageLoader.LoadDynamicPackageFromJson also exists for runtime content loading. It deserializes JSON, checks if already loaded, adds to tracking, calls LoadPackageContent, returns skeleton IDs. But it NEVER calls GenerateLocationActionsFromCatalogue. Dynamic locations get added to GameWorld.Locations but their actions never generate.

The assumption: All locations come from startup JSON packages. Dynamic content wasn't considered during initial implementation. The action generation step was tied to initialization rather than location addition.

---

## Design Approach & Rationale

### HIGHLANDER Principle for Context Building

The fundamental problem was six implementations of the same concept. The HIGHLANDER principle states: one concept, one implementation. Context building is a single concept - given placement information, construct a properly populated SceneSpawnContext.

The solution: Create shared static utility class SceneSpawnContextBuilder in Content folder. Single public static method BuildContext. All context building code calls this utility instead of implementing own resolution logic.

Why static utility vs service class? The context building logic has no state and no external dependencies beyond GameWorld. It's a pure function: given inputs (GameWorld, placement details), produce output (SceneSpawnContext). Static utilities are appropriate for:
- No instance state required
- No external service dependencies
- Pure transformation logic
- Used across multiple layers (orchestration, services, tests)

Why Content folder? Content folder is architecture-neutral. Contains domain entities and associated infrastructure but no dependencies on higher-level services. Makes the utility accessible from anywhere: GameFacade (orchestration), ObligationActivity (service), tests. Placing it with SceneSpawnContext follows the principle: data structures and their construction logic belong together.

### Parse-Time Resolution Over Runtime Lookup

The bug was attempting runtime resolution (npc.WorkLocationId lookup) when parse-time resolution already provided the answer (npc.Location object reference).

The fix: Use npc.Location directly. It's already resolved during JSON parsing. LocationParser read locationId from NPCDTO, looked up Location entity in GameWorld.Locations, assigned to npc.Location property. This happens once during initialization. Runtime code should use the object reference, not re-resolve from string IDs.

Why this is correct architecturally: Parse-time resolution is cheaper (runs once vs thousands of times), safer (object references can't have typos), simpler (direct property access vs dictionary lookups), more maintainable (one resolution point vs scattered lookups).

The distinction between Location (object, parse-time) and WorkLocationId (string, runtime) exists for scenarios where location can change during gameplay or doesn't exist at parse time. For static NPCs with fixed locations, parse-time resolution is sufficient and preferred.

### Action Generation Tied to Location Addition

The realization: LocationActions aren't tied to package loading, they're tied to location existence. When a new location enters GameWorld.Locations, its actions need generation regardless of HOW it entered (JSON parse vs dynamic generation).

The solution: Call GenerateLocationActionsFromCatalogue after loading dynamic packages. This method already exists and is designed for this purpose. It iterates ALL locations in GameWorld.Locations. For each, it calls LocationActionCatalog.GenerateActionsForLocation which generates both property-based and hex-based actions.

Why regenerate for ALL locations? Because adjacency relationships may have changed. Adding a new location at hex (1, -1) affects the adjacent location at hex (0, -1) - it now has one more neighbor. Regenerating for all ensures consistency. The method has duplicate detection via Contains check before adding actions to GameWorld.LocationActions, so regenerating existing location actions is safe.

Alternative considered: Track which locations are new and only generate actions for them. Rejected because: More complex tracking, misses adjacency updates to existing locations, harder to maintain, optimization for problem that doesn't exist (action generation is fast).

### Fail Fast with Clear Errors

The context builder returns null if entity resolution fails. This enables fail-fast behavior - calling code checks for null and either logs warning or skips that spawn. Don't try to paper over missing data with defaults or empty contexts.

If an NPC referenced in placement doesn't exist, that's a DATA QUALITY ISSUE, not a runtime scenario to handle gracefully. Log it, skip the spawn, fix the data. Don't create complex fallback logic that hides the problem.

Same for locations and routes. Null return means "cannot build valid context from this placement information." Calling code decides whether to log, skip, or propagate error. Context builder's job is resolution, not policy.

---

## Implementation Strategy

### Phase 1: Create Shared Context Builder

Create SceneSpawnContextBuilder static class in Content folder. Single public static method BuildContext accepting GameWorld, Player, PlacementRelation, SpecificPlacementId, and optional CurrentSituation.

Method is pure switch statement on PlacementRelation:

**SpecificNPC case**: Find NPC entity by ID using FirstOrDefault on GameWorld.NPCs. Set context.CurrentNPC. Set context.CurrentLocation from npc.Location (the key fix - direct object reference, not WorkLocationId lookup). Return null if NPC not found.

**SpecificLocation case**: Find Location entity by ID using FirstOrDefault on GameWorld.Locations. Set context.CurrentLocation. Return null if location not found.

**SpecificRoute case**: Find Route entity by ID using FirstOrDefault on GameWorld.Routes. Set context.CurrentRoute. If route has OriginLocation, set context.CurrentLocation as well. Return null if route not found.

**SameLocation/SameNPC/SameRoute/Generic cases**: Return context with only Player and CurrentSituation set. These cases rely on different resolution strategies (inheritance or filtering) handled by calling code.

No logging in the builder itself. It's a pure transformation function. Calling code decides what to log based on null returns.

### Phase 2: Refactor GameFacade

Replace the switch statement in SpawnStarterScenes that duplicates resolution logic. Remove all debug logging from context building.

Change from manual entity resolution + context population to single call: context = SceneSpawnContextBuilder.BuildContext(gameWorld, player, placementRelation, specificPlacementId, null).

Add null check after builder call. If null, skip this template with continue statement. This handles missing entities gracefully during startup.

The result: SpawnStarterScenes becomes significantly simpler. Template iteration, placement determination, builder call, null check, spawn call. Clean orchestration without embedded resolution logic.

### Phase 3: Refactor ObligationActivity

Delete the entire BuildContextForObligationReward method. It's now redundant - shared builder provides same functionality.

Update call site in ProcessObligationReward. Replace BuildContextForObligationReward(sceneSpawn) with direct builder call: SceneSpawnContextBuilder.BuildContext(gameWorld, player, sceneSpawn.PlacementRelation, sceneSpawn.SpecificPlacementId, null).

Existing null check and warning message remain unchanged. They handle builder returning null for missing entities.

The result: Obligation activity becomes simpler, HIGHLANDER violation eliminated, one less method to maintain.

### Phase 4: Add Action Generation to Dynamic Package Loading

Modify PackageLoader.LoadDynamicPackageFromJson. After the LoadPackageContent call that adds locations to GameWorld, add call to GenerateLocationActionsFromCatalogue.

Three-line addition: Comment explaining why, GenerateLocationActionsFromCatalogue() call, blank line before return statement.

The comment is important: Explains that dynamic packages may add locations needing actions, notes that regeneration for all locations handles adjacency updates, establishes pattern for future dynamic content.

The placement matters: Must come AFTER LoadPackageContent (so new locations exist in GameWorld.Locations) but BEFORE return statement (so actions exist before method completes).

No conditional logic needed. Always regenerate actions after loading dynamic content. The duplicate check in action addition makes this safe.

### Phase 5: Verification Testing

Build project and verify zero warnings/errors. This confirms no compilation issues from refactoring.

Run game from fresh initialization. Expected flow:
1. GameWorld initializes from JSON files
2. StartGameAsync calls SpawnStarterScenes
3. SpawnStarterScenes uses shared builder to create context
4. Elena NPC found, npc.Location used for CurrentLocation
5. SpawnSceneWithDynamicContent creates provisional scene
6. FinalizeScene generates dependent location spec
7. DetermineVenueId uses context.CurrentLocation.VenueId successfully
8. BuildLocationDTO finds adjacent hex successfully
9. Dynamic package generated and loaded
10. **GenerateLocationActionsFromCatalogue called - KEY NEW STEP**
11. Dependent location gets intra-venue travel actions
12. Elena has active scene with situations
13. Player can interact with Elena
14. Player can navigate to private room via intra-venue travel action

Check browser UI: Elena should show "Talk" action card. Map should show private room. Private room location should show "Move to Common Room" action (or similar based on adjacent location name). Verify both directions of intra-venue travel work.

---

## Critical Constraints

### HIGHLANDER Enforcement

Once shared builder exists, NO new context building implementations should be added. Any future code needing SceneSpawnContext must call SceneSpawnContextBuilder.BuildContext. This is a hard architectural rule. Code reviews must reject new switch statements that duplicate this logic.

### No Runtime Catalogue Calls

Catalogues execute exclusively at parse time during game initialization. Runtime code never calls catalogues. LocationActionCatalog is NOT a catalogue despite the name - it's a generator called at parse time and after dynamic content loads. The naming is historical but the timing pattern is correct.

### Parse-Time vs Runtime Resolution Preserved

The distinction between parse-time resolution (locationId → Location object) and runtime resolution (workLocationId → lookup) must be preserved. They serve different purposes: static associations vs dynamic state changes.

Don't try to consolidate them. The NPC entity needs both patterns for different scenarios. Parse-time for fixed locations, runtime IDs for locations that change or don't exist yet.

Context building should prefer parse-time resolved references when available. Use runtime ID resolution only when parse-time resolution insufficient.

### Action Generation Atomicity

Action generation must be atomic with location addition for dynamic content. If a location enters GameWorld.Locations, its actions must be generated before the method that added it returns. Don't create gaps where locations exist but lack actions.

This means any future dynamic content loading mechanism must also call GenerateLocationActionsFromCatalogue. The pattern established in LoadDynamicPackageFromJson should be followed universally.

### Orchestration Layer Responsibility

Context building with entity resolution is orchestration-level logic. Domain services receive fully populated contexts, not null properties they have to resolve themselves.

SceneInstantiator should receive contexts with CurrentLocation already set. It shouldn't check if CurrentLocation is null and try to resolve from NPC. Separation of concerns: orchestration resolves entities, domain services operate on entities.

This keeps domain logic pure and testable. Tests can construct contexts with any entities without needing GameWorld lookups.

### Data Integrity Over Code Flexibility

If NPC data is missing required properties for a scene to spawn, the correct fix is updating the data, not adding fallback logic in code. Code that tries to gracefully handle missing data creates technical debt and hides data quality issues.

Fail fast, fix data, enforce quality. Let null returns from context builder surface data problems during testing. Fix the JSON, don't paper over gaps with defaults.

---

## Key Files & Their Roles

### Content/SceneSpawnContext.cs

Defines the context data structure containing Player, CurrentLocation, CurrentNPC, CurrentRoute, CurrentSituation. Pure data carrier with no behavior. Lives in Content folder as domain entity.

### Content/SceneSpawnContextBuilder.cs (NEW)

Static utility class with single method BuildContext. Centralizes all entity resolution and context population logic. Used by any code constructing SpawnContext from placement information. New file created during this refactoring.

### Content/Core/03_npcs.json

NPC definitions including Elena. Contains locationId for parse-time associations. The Location object reference resolved from this field is what context building uses. No need for additional runtime ID fields given current game mechanics.

### Services/GameFacade.cs

Orchestration layer coordinating scene spawning. Method SpawnStarterScenes builds contexts and calls SpawnSceneWithDynamicContent. Modified to use shared context builder instead of duplicate switch logic. Simplified by removal of manual resolution code.

### Services/ObligationActivity.cs

Service handling obligation phase completion rewards. Method ProcessObligationReward spawns scenes as rewards. Modified to use shared builder instead of separate BuildContextForObligationReward method. Method deleted entirely as redundant.

### Content/SceneInstantiator.cs

Domain service for scene lifecycle management. Methods CreateProvisionalScene and FinalizeScene use context to resolve placement and generate dependent resources. Uses context.CurrentLocation for VenueIdSource.SameAsBase resolution in DetermineVenueId. Consumer of context, not builder.

### Content/PackageLoader.cs

Handles loading content from JSON packages. Method LoadAllPackages loads startup content and calls GenerateLocationActionsFromCatalogue at end. Method LoadDynamicPackageFromJson loads runtime-generated content. Modified to also call GenerateLocationActionsFromCatalogue after loading, ensuring dynamic locations get actions.

### Content/Catalogues/LocationActionCatalog.cs

Generates LocationActions for locations based on properties and hex adjacency. Method GenerateActionsForLocation creates both property-based actions (Crossroads → inter-venue travel) and hex-based actions (adjacent locations → intra-venue movement). Called by PackageLoader at initialization and after dynamic content loads.

### Content/Parsers/HexParser.cs

Parser for hex grid data. Contains validation methods ensuring hex positions are unique and all locations have spatial presence. Serves as architectural model for data integrity validation at parse time.

### Subsystems/Spawn/SpawnFacade.cs

Service for procedural scene spawning. Method BuildSpawnContext intentionally minimal (Player only) because it relies on categorical filtering rather than concrete placement. Different pattern from GameFacade - doesn't need full resolution.

---

## Next Session Priorities

### Testing Verification

Confirm starter scene spawns successfully. Confirm dependent room has hex position and venue ID. Confirm Elena presents "Talk" action. Confirm tutorial flow is playable from start to room acquisition. Confirm intra-venue travel actions exist on both locations (common room and private room). Test bidirectional navigation.

### Pattern Documentation

Document the established pattern: When adding locations to GameWorld.Locations at runtime, always call GenerateLocationActionsFromCatalogue afterward. This pattern should be followed by any future dynamic content systems.

### Architecture Review

Consider whether other context-like patterns exist in the codebase that could benefit from similar HIGHLANDER enforcement. The shared builder pattern is reusable for any scenario where entity resolution from IDs needs centralization.

Look for other places where parse-time resolution exists but runtime code attempts redundant lookups. The npc.Location pattern may apply to other entities with similar dual reference patterns.

### Validation Enhancement

Consider adding parser validation for NPC location references similar to hex position validation. Parser could check that locationId in NPC JSON points to valid location. This catches data issues at startup rather than runtime.

Could also validate that scenes with dependent locations using VenueIdSource.SameAsBase have concrete placement (not Generic). Fail fast at parse time for impossible configurations.
