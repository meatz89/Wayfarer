# Handoff: Location Locking, Hex Grid Integration, and Scene State Machine

## Problem & Motivation

### Primary Issues
Players reported that runtime-generated dependent locations (Elena's private room) appeared unlocked when they should be locked, and clicking the locked room action caused crashes with NullReferenceException. Additionally, after unlocking and entering the room, the scene failed to progress to subsequent situations, leaving the game in an unplayable soft-lock state.

### Business Impact
This breaks the fundamental tutorial flow and makes procedurally-generated scenes completely non-functional. Players cannot experience multi-situation scenes where dependent locations gate progression. The game becomes unplayable past the first situation of any scene that generates dependent locations.

### Core Pain Points
- Dependent locations spawn with InitialState="Locked" in DTO but this categorical string never translates to the concrete IsLocked boolean property
- The hex grid spatial index lacks entries for runtime-created locations, causing navigation queries to return null and crash
- Scene lifecycle state machine uses exact ID matching for transitions, but templates define transitions with template IDs while runtime situations have instance IDs, causing transition matching to fail and scenes to despawn prematurely

## Architectural Discovery

### Parser-JSON-Entity Triangle
The codebase follows a three-phase data transformation pattern for all content:

**Phase 1: JSON Authoring**
Content authors write categorical, human-readable properties. Example: InitialState can be "Locked", "Hidden", "Discovered". These are descriptive strings chosen for authoring clarity, not runtime efficiency.

**Phase 2: Parse-Time Translation**
Parsers act as translation layers. They read DTOs and convert categorical strings into concrete, strongly-typed runtime properties. This happens once during content load. The parser is responsible for consulting catalogues that translate relative/categorical values into concrete game state.

**Phase 3: Runtime Domain Entities**
Domain entities use only concrete types - booleans, enums, integers, object references. No string matching, no dictionary lookups, no catalogue calls. All decisions use direct property access.

**Critical Rule**: Translation must be COMPLETE. If a DTO has InitialState, the parser MUST translate it. If the parser doesn't translate, the property defaults to its C# initialization value, breaking game logic.

### Hex Grid as Spatial Index
The WorldHexGrid is a lookup structure, not an ownership structure. Locations own their HexPosition as source of truth. Hexes in the grid act as a bidirectional spatial index - given coordinates, find location; given location, verify it's in the grid.

**Two-Phase Sync Pattern:**
1. Initial world load: Hex JSON defines hexes with LocationIds → SyncLocationHexPositions() reads these and sets Location.HexPosition
2. Runtime location creation: Location gets HexPosition assigned → EnsureHexGridCompleteness() reads this and updates Hex.LocationId

The critical invariant: Every positioned location must have a corresponding hex with matching LocationId. Without this, GetPlayerCurrentLocation() queries the hex grid and returns null.

### HIGHLANDER Principle in Practice
The codebase rigidly enforces "one concept, one representation" with three patterns:

**Pattern A: Persistence IDs with Runtime Navigation**
Used when properties come from JSON and need frequent runtime access. Store BOTH ID (string, immutable after parsing) and object reference (resolved once during parsing, used everywhere at runtime). Example: Location has VenueId and Venue object. Runtime code ONLY uses the object reference, never looks up by ID.

**Pattern B: Runtime-Only Navigation**
Used for runtime state that changes during gameplay. Store ONLY the object reference, NO ID property. Adding an ID alongside the object creates desync risk. Example: Scene.CurrentSituation - this is runtime state, so only the object exists.

**Pattern C: Lookup on Demand**
Used for infrequent access from JSON. Store ONLY the ID, no cached object. Perform GameWorld lookups when needed.

**Pattern D: Template ID + Instance ID (for spawned entities)**
Used for entities created from templates at runtime. Store BOTH TemplateId (references the template used to create this) and Id (unique runtime instance identifier). Template ID enables matching against template-defined rules while Instance ID ensures uniqueness. This is a specialization of Pattern A where the "persistence ID" is actually the template ID.

### Scene Lifecycle State Machine
Scenes progress through a Linear pattern with situation transitions. Each transition connects a source situation to a destination situation via a condition. When a situation completes, the scene's AdvanceToNextSituation method queries these transitions to find the next situation. If a matching transition exists, the scene updates CurrentSituationId and persists. If no matching transition exists, the scene marks itself Completed and despawns.

**Critical Design**: Transition matching uses TemplateId, not instance Id. Templates define transitions with template IDs because templates are authored before instances exist. Runtime situations have both TemplateId (references the template) and Id (unique instance identifier). Transition matching MUST use TemplateId to connect template-defined rules to runtime instances.

## Domain Model Understanding

### Location Lifecycle
Locations exist in three forms: authored static locations, template-defined dependent locations, and runtime-instantiated dependent locations.

**Static Locations**: Defined in location JSON, parsed once at startup, exist for entire game session.

**Dependent Location Templates**: Defined within scene archetypes as DependentLocationSpec objects. These are NOT locations - they're blueprints specifying how to create a location when needed.

**Dependent Location Instances**: Created at runtime when a scene spawns. Each instance gets a unique ID combining scene instance ID and template ID. Properties come from the template, hex position comes from spatial placement logic.

### Lock/Unlock Mechanics
IsLocked is a boolean domain property on Location. The game has multiple systems that check this:
- Location actions check IsLocked and set IsAvailable=false if locked
- UI displays locked actions with grayed styling and lock icon
- RewardApplicationService sets IsLocked=false when LocationsToUnlock rewards apply

The InitialState DTO property acts as a categorical authoring convenience. Content authors write "Locked" instead of setting a boolean property. The parser translates this during load.

### Scene-Situation Relationship
Scenes own situations via ID references, not inline collections. A scene has CurrentSituationId and SpawnRules.Transitions. Situations live in GameWorld.Situations as a flat list. This enables situations to be queried independently while scenes manage lifecycle.

**Situation Identity Duality**: Each runtime situation has TWO identifiers - Id (unique instance) and TemplateId (references source template). Template ID is essential for transition matching because transitions are defined at template authoring time using template IDs.

### Hex Coordinate System
Uses axial coordinates with Q and R. Each hex can have at most one location. Hexes without locations represent wilderness/travel spaces. The HexMap maintains both a List of all hexes and a lookup dictionary for O(1) coordinate queries.

**Hex-Location Binding**: When a location spawns at runtime, it receives hex coordinates. The hex at those coordinates may already exist (defined in initial world) with LocationId=null. EnsureHexGridCompleteness updates this existing hex's LocationId to point to the new location, maintaining the spatial index.

## Current State Analysis

### Existing Location Parsing
LocationParser.ConvertDTOToLocation handles most properties correctly - ID, name, description, venue reference. It stores InitialState as a string but performs no translation. The pattern exists elsewhere: the parser translates Properties (string array) to LocationProperties (enum flags) by consulting the LocationPropertyCatalog. The same pattern should apply to InitialState.

### Existing Hex Grid Sync
SyncLocationHexPositions works in one direction: reads Hex.LocationId from the hex grid and sets Location.HexPosition. This works for initial world load where hexes are defined first. For runtime location creation, the inverse is needed: read Location.HexPosition and update Hex.LocationId.

### Existing Scene Transition Matching
Scene.GetTransitionForCompletedSituation queries transitions using exact ID comparison against the completed situation's Id property. This worked historically when situations were authored directly with stable IDs. With procedurally-generated situations, instance IDs are unique GUIDs while transitions reference template IDs. The property already exists on Situation (TemplateId), but the matching logic doesn't use it.

## Design Approach & Rationale

### Location Locking: Catalogue Pattern Application
The InitialState translation follows the established CATALOGUE PATTERN. During parsing, check if InitialState equals "Locked" and set IsLocked=true. This is parse-time translation from categorical to concrete, exactly like how Properties translates.

**Why this approach**: Maintains consistency with existing patterns. The catalogue pattern explicitly exists to translate categorical author-friendly values into concrete runtime properties. InitialState="Locked" is categorical, IsLocked=true is concrete. The translation is deterministic and stateless, perfect for parse-time execution.

**Rejected alternative**: Could check InitialState at runtime wherever IsLocked is used. This violates the architecture - runtime code should never do string matching or interpret categorical values. Parse-time translation is mandatory.

### Hex Grid: Inverse Sync Operation
The EnsureHexGridCompleteness method mirrors SyncLocationHexPositions but operates in the opposite direction. Instead of "hex has LocationId, set location's HexPosition", it's "location has HexPosition, set hex's LocationId".

**Why this approach**: Maintains the HIGHLANDER principle that Location.HexPosition is source of truth. The hex grid is a derived index. When a location knows its position but the hex doesn't know about the location, the index is incomplete. Update the index to match reality.

**Critical insight from discovery**: The hex at (1, -1) already exists in initial world JSON with LocationId=null. We're not creating a new hex, we're updating an existing hex's LocationId. The method handles both cases - update existing hex if found, create new hex if coordinates have no hex.

**Rejected alternative**: Could create hexes during scene instantiation as dependent resources. This couples scene generation to hex grid management. By keeping it in PackageLoader after location loading, we maintain separation - location system doesn't know about hex grid, hex grid system updates itself when new locations appear.

### Scene Transitions: Use Existing TemplateId
The TemplateId property already exists on Situation for this exact purpose. The transition matching just needs to use it. Change one comparison from completedSituation.Id to completedSituation.TemplateId.

**Why this works**: Template transitions are defined with template IDs. Runtime situations store the template ID they came from. Matching template ID to template ID connects template-defined rules to runtime instances. This is HIGHLANDER Pattern D in action.

**Critical verification needed**: Ensure TemplateId is populated during situation instantiation. If the property exists but stays null, matching still fails. The instantiation code must copy template.Id into situation.TemplateId when creating the situation from template.

## Implementation Strategy

### Location Lock Translation (Parse Time)
After creating the Location entity in the parser, before returning it, check if InitialState is non-empty and equals "Locked". If so, set location.IsLocked = true. This executes once during content load, translating the categorical value to concrete boolean.

Place this check immediately after the Location object is created but before any HIGHLANDER Pattern A resolutions. This ensures the property is set before any other code might query it.

### Hex Grid Completeness (Post-Load)
In PackageLoader, after calling SyncLocationHexPositions (which handles initial world), call EnsureHexGridCompleteness. This method iterates all locations, finds those with HexPosition values, checks if a hex exists at those coordinates, and either updates the existing hex's LocationId or creates a new hex.

The method must rebuild the lookup dictionary after any modifications to ensure GetHex() queries work correctly. Place the call in the same location loading section where SyncLocationHexPositions is called, maintaining the pattern of "load locations, then sync hex grid, then ensure completeness".

### Hex Update vs Create Logic
For each positioned location:
- Use hexMap.GetHex(coordinates) to retrieve hex at location's position
- If hex exists and LocationId is null or different, update it and increment modified counter
- If hex doesn't exist, create new hex with terrain, danger defaults suitable for safe settlement areas
- After loop, if counter > 0, rebuild lookup dictionary

The rebuild is critical - the dictionary caches coordinate-to-hex mappings, and any hex modifications invalidate it.

### Scene Transition Fix (State Machine)
In Scene.GetTransitionForCompletedSituation, change the LINQ Where clause from comparing with completedSituation.Id to comparing with completedSituation.TemplateId. This is a single-property change in one method.

Verify that situation instantiation code sets TemplateId when creating situations from templates. Search for where Situation objects are instantiated from SituationTemplate and ensure template.Id is assigned to situation.TemplateId.

## Critical Constraints

### Parse-Time vs Runtime Resolution
ALL categorical translations must happen at parse time. Runtime code must NEVER do string comparisons, dictionary lookups to interpret categorical values, or call catalogue methods. This is enforced in CLAUDE.md under CATALOGUE PATTERN. Violations cause performance issues and violate single responsibility.

### HIGHLANDER Principle Compliance
One concept, one representation. If both template ID and instance ID are needed, BOTH must be stored (Pattern D). Never compute one from the other at query time. Never mix patterns - if using Pattern A for most properties, don't suddenly use Pattern C for one property. Consistency prevents desync bugs.

### Source of Truth Respect
Location.HexPosition is source of truth, Hex.LocationId is derived. Never set hex position by reading from hex - only update hex by reading from location. This prevents circular dependencies and ensures one-way data flow.

### No Backwards Compatibility
CLAUDE.md explicitly forbids backwards compatibility. If InitialState needs translation, add it. Don't check "does it work without translation" or "what if old code doesn't expect this". Fix it right, completeness over compatibility.

### Immutability of Parse-Time State
Properties set during parsing are immutable during that parsing phase. TemplateId is set once when situation is instantiated and never changes. This enables reliable matching throughout the situation's lifetime.

## Key Files & Their Roles

### LocationParser.cs
Translates LocationDTO to Location entity. This is where InitialState="Locked" must become IsLocked=true. The parser is responsible for ALL DTO-to-entity translation, consulting catalogues for categorical property translation. Adding InitialState translation maintains pattern consistency with existing Properties translation.

### HexParser.cs
Manages bidirectional sync between Location.HexPosition and Hex.LocationId. Contains SyncLocationHexPositions for initial world load. Add EnsureHexGridCompleteness to handle runtime-created locations. Both methods maintain the invariant that the hex grid spatial index stays complete.

### PackageLoader.cs
Orchestrates content loading across all systems. Calls parsers in dependency order. After loading locations, calls hex sync methods to update spatial index. This is the integration point where parse-time hex grid completeness gets invoked.

### Scene.cs
Implements scene lifecycle state machine. Contains AdvanceToNextSituation (decides whether to continue or complete scene) and GetTransitionForCompletedSituation (matches completed situation to next transition). The transition matching is where TemplateId must be used instead of Id.

### Situation.cs
Domain entity representing individual situations. Has both Id (unique instance) and TemplateId (source template reference). Both properties are already defined. TemplateId enables template-based matching while Id ensures uniqueness. This is HIGHLANDER Pattern D in action.

### location.css
Provides UI styling for locked location actions. The locked class applies grayed background, dashed border, disabled cursor, lock icon styling. No changes needed here - CSS already handles visual feedback correctly.

### LocationActionManager.cs
Generates location actions and checks Location.IsLocked to set IsAvailable property and LockReason text. This provides backend enforcement of lock state. No changes needed here - manager already checks the property correctly.

### Landing.razor
Renders location actions and applies locked CSS class when IsAvailable is false. Shows lock icon and LockReason text. No changes needed here - UI already responds to backend lock state correctly.

## Critical Discovery: Scene Soft-Lock Bug

While verifying the location locking fix, discovered that multi-situation scenes despawn after the first situation completes. This is a SEPARATE critical bug from location locking. Root cause: transition matching compares instance IDs instead of template IDs, finds no matching transition, marks scene Completed, scene despawns.

**Severity**: Game-breaking soft-lock. Player completes first situation, scene despawns, remaining situations never spawn, player cannot progress.

**Fix Complexity**: Low - change one comparison from Id to TemplateId. High impact, low risk.

**Verification**: Run existing LodgingSceneFlowTest unit tests - they validate scene structure but don't test runtime lifecycle. Need to verify TemplateId is populated during instantiation and transition matching uses it.
