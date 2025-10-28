# Scene-Situation System - Complete Integration Architecture

**CRITICAL: This document defines the CORRECT Scene-Situation architecture that replaces the old Obstacle/equipment-based Scene system.**

---

## ARCHITECTURAL OVERVIEW

### The Sir Brante Pattern in Wayfarer

Wayfarer implements the Sir Brante narrative progression pattern through three core concepts:

**Scene** = Persistent narrative container spawned from templates
**Situation** = Narrative moment offering 2-4 player choices
**Choice** = Player action with requirement, cost, and reward

### Information Flow

SceneTemplate defines structure using categorical filters (personality types, location properties, bond levels) without concrete entity IDs. At spawn time, procedural generation queries GameWorld for entities matching those filters and creates a Scene instance assigned to a concrete Location, NPC, or Route.

Scenes contain multiple Situations organized by spawn rules defining how Situations lead into each other. These rules create cascades: Linear progression, parallel exploration, branching consequences, convergent paths, and other patterns.

When player enters a Location, talks to an NPC, or travels a Route, GameWorld.Scenes is filtered by PlacementType and PlacementId to find active Scenes. The current Situation renders with its narrative text and 2-4 Choices. Player selects a Choice, requirements are checked, costs paid, and rewards applied. Rewards can spawn new Scenes at relative placements like SameLocation or SameNPC.

Spawn rules then determine the next Situation within the current Scene, creating the narrative cascade effect.

---

## CORE ENTITY CONCEPTS

### Scene (Persistent)

**Storage Location:** GameWorld.Scenes as List

**Purpose:** Orchestrate multiple Situations with spawn rules defining their relationships

**Identity Properties:**
- Unique identifier
- Template identifier tracking which SceneTemplate spawned this instance

**Placement Properties:**
- Placement type: Location, NPC, or Route
- Placement identifier: Concrete entity ID from GameWorld this Scene appears at

**Content Properties:**
- List of embedded Situations (not ID references)
- Spawn rules defining how Situations lead into each other
- Current situation identifier tracking player progress

**State Properties:**
- Scene state: Active, Completed, Expired
- Optional expiration day for time-limited content
- Generated intro narrative customized to placement

**Metadata:**
- Archetype classification: Linear, HubAndSpoke, Branching, Converging, etc.

### Lifecycle

Scenes are spawned from templates, registered in GameWorld, activated when player enters their placement, advanced through Situations via spawn rules, completed when all relevant Situations finish or expiration reached, then removed or archived.

---

### Situation (Embedded)

**Storage Location:** Embedded in Scene.Situations as List

**Purpose:** Present one narrative moment with 2-4 player choices (Sir Brante compliance)

**Identity:**
- Unique identifier within Scene
- Optional template identifier

**Content:**
- Narrative text player sees
- List of 2-4 Choices (never less, never more)

**State:**
- Situation state: Available, Completed, Locked
- Priority for display ordering when multiple Situations active

### Sir Brante Compliance Rules

Every Situation must have exactly 2-4 Choices. Each Choice represents a distinct player action. Requirements are visible to player before selection. Costs are visible before selection. Outcomes remain hidden until Choice selected.

---

### Choice (Embedded)

**Storage Location:** Embedded in Situation.Choices as List

**Purpose:** Define one player action with requirement, cost, and reward structure

**Content:**
- Action text player sees
- Compound requirement structure (may be null)
- Resource cost structure specifying coins, resolve, time segments
- Reward structure (hidden until selected)

**Action Classification:**
- Instant: Apply cost and reward immediately, advance situation
- Start Challenge: Enter tactical challenge (Social, Mental, or Physical)
- Navigate: Move player to new location, NPC, or route

**Challenge References:**
- Challenge identifier (if action starts challenge)
- Challenge type classification

---

### SituationSpawnRules (Embedded)

**Storage Location:** Embedded in Scene.SpawnRules

**Purpose:** Define rule cascade - how Situations within Scene lead into each other

**Pattern Classification:**
- Pattern type: Linear, HubAndSpoke, Branching, Converging, Conditional, and 10+ other documented patterns

**Structural Properties:**
- Initial situation identifier (what player sees first)
- List of transitions between situations
- Completion condition defining when Scene is done

**Transition Structure:**
- Source situation identifier
- Destination situation identifier
- Transition condition: OnSuccess, OnFailure, OnChoice, Always
- Specific choice identifier (if condition is OnChoice)

### Spawn Pattern Types

Linear: Sequential progression A → B → C where each Situation spawns single follow-up.

HubAndSpoke: Central Situation spawns multiple parallel child Situations, all available simultaneously.

Branching: Success and failure paths diverge, creating different consequence chains.

Converging: Multiple independent paths all lead to single finale Situation.

Conditional: Different paths based on game state evaluation.

See situation-spawn-patterns.md for complete catalog of 15+ pattern types.

---

### ChoiceReward (Embedded)

**Storage Location:** Embedded in Choice.Reward

**Purpose:** Define outcomes when player selects this Choice

**Resource Rewards:**
- Coin amount
- Resolve amount
- Bond change amount with target NPC identifier
- Time advancement in segments

**State Changes:**
- Scale shifts for player behavioral spectrum
- States granted and removed
- Achievements granted
- Items granted

**Scene Spawning (Critical Feature):**

List of Scenes to spawn, each specifying:
- Template identifier to instantiate
- Placement relation: SameLocation, SameNPC, SameRoute, SpecificLocation, SpecificNPC, SpecificRoute
- Specific placement identifier (if relation is Specific type)
- Delay in days (0 for immediate, positive for scheduled future spawn)

### Placement Relations

SameLocation: Spawn at same location where Choice was made
SameNPC: Spawn at same NPC where Choice was made
SameRoute: Spawn on same route
SpecificLocation: Spawn at provided location identifier
SpecificNPC: Spawn at provided NPC identifier
SpecificRoute: Spawn at provided route identifier

---

## TEMPLATE SYSTEM CONCEPTS

### SceneTemplate (Content Definition)

**Storage Location:** GameWorld.SceneTemplates as Dictionary keyed by template identifier

**Purpose:** Define Scene pattern with categorical filters, no concrete entity IDs

**Structure:**
- Template identifier
- Archetype classification
- Placement filter with categorical requirements
- List of situation templates
- Spawn rules structure
- Optional expiration days
- Starter flag for initial game content

### Placement Filter Concept

Filters specify entity selection criteria without naming specific entities:

**For NPC placement:**
- Personality type list: Mercantile, Commanding, Cunning, etc.
- Bond thresholds: minimum and maximum
- Tag requirements: Wealthy, UrbanLocation, etc.

**For Location placement:**
- Location property requirements
- Tag requirements
- District or region requirements

**For Route placement:**
- Terrain type requirements
- Distance requirements
- Connectivity requirements

**Player State Filters:**
- Required states player must have
- Required achievements player must have
- Scale position requirements

### Template Instantiation Concept

Parser reads JSON into SceneTemplate object stored in GameWorld. When spawning time arrives, SceneSpawner queries GameWorld for entities matching all filter criteria. If matches found, spawner selects best candidate using priority rules or randomization. Spawner creates Scene instance with concrete PlacementId assigned. If no matches found, template waits in pool or fails to spawn based on template configuration.

---

### SituationTemplate (Embedded in SceneTemplate)

**Purpose:** Define one Situation within Scene template

**Structure:**
- Situation identifier
- Narrative template with placeholder syntax
- List of choice templates

**Placeholder System:**

Narrative templates contain placeholders replaced at spawn time:
- NPC name placeholder replaced with actual NPC name
- Location name placeholder replaced with actual Location name
- Player name placeholder
- Custom property placeholders based on placement type

Enables one template to generate varied narrative based on which entity was selected during instantiation.

---

## PROCEDURAL GENERATION CONCEPT

### Scene Spawning Process

Template retrieval: Get SceneTemplate from GameWorld by identifier

Placement query: Query GameWorld for all entities matching template placement filter criteria. For NPC placement, query NPCs matching personality types, bond thresholds, and tags. For Location placement, query Locations matching property and tag requirements.

Candidate selection: From matching entities, select one using priority rules (highest bond, closest distance, random selection, etc.) or context-specific logic.

Scene instantiation: Create Scene object with generated identifier, assigned template identifier, concrete placement type and identifier from selected entity, initial state as Active.

Situation generation: For each SituationTemplate, create Situation object with placeholders replaced using actual entity properties from selected placement.

Spawn rules copying: Copy template spawn rules to Scene instance, set current situation to initial situation from rules.

Narrative generation: Generate intro narrative using template and actual placement entity properties.

Registration: Add completed Scene to GameWorld.Scenes.

Return: Provide Scene instance to calling code.

---

## RUNTIME FLOW CONCEPTS

### Scene Activation

When player enters Location: Query GameWorld.Scenes filtered by PlacementType equals Location AND PlacementId equals current location AND State equals Active.

When player talks to NPC: Query GameWorld.Scenes filtered by PlacementType equals NPC AND PlacementId equals current NPC AND State equals Active.

When player travels Route: Query GameWorld.Scenes filtered by PlacementType equals Route AND PlacementId equals current route AND State equals Active.

### Situation Rendering

From active Scenes, retrieve current Situation using Scene.CurrentSituationId.

Display Situation narrative text.

For each Choice in Situation:
- Display Choice action text
- Evaluate Choice requirement against player state and GameWorld
- Show locked or available visual state
- Display Choice cost (coins, resolve, time)
- Enable selection if available, disable if locked

### Choice Execution Flow

Player selects Choice from Situation.

Validation: Check Choice requirement is satisfied using player state and GameWorld. If not satisfied, reject and show requirement details.

Cost payment: Deduct coins from player, deduct resolve from player, advance time by specified segments.

Reward application: Add coins to player, add resolve to player, modify NPC bond strength, apply scale shifts, grant or remove states, grant achievements, grant items.

Scene spawning: For each SceneSpawnReward in Choice reward, determine actual placement identifier based on placement relation, create spawn context with forced placement and delay, schedule delayed spawn or spawn immediately based on delay value.

Situation completion: Mark current Situation as Completed.

Scene advancement: Find applicable transition from spawn rules where source matches completed Situation and condition matches selected Choice or outcome, set Scene.CurrentSituationId to destination Situation from transition, mark destination Situation as Available.

Challenge handling: If Choice action type is StartChallenge, initiate tactical challenge subsystem with specified challenge identifier and type.

---

### Situation Advancement Details

Transition evaluation examines spawn rules to find transition where:
- Source situation matches just-completed Situation
- Condition evaluates true given selected Choice and outcome

**Condition Types:**

Always: Transition always occurs regardless of choice
OnChoice: Transition occurs only if specific choice selected
OnSuccess: Transition occurs if choice succeeded (reward applied)
OnFailure: Transition occurs if choice failed

If matching transition found: Update Scene.CurrentSituationId to destination, mark destination Situation as Available.

If no matching transition found: Check Scene completion condition, mark Scene as Completed if condition met, remove from GameWorld.Scenes or archive for save system.

---

## INTEGRATION CONCEPTS

### PackageLoader Integration

During package loading, SceneTemplate DTOs are parsed into SceneTemplate objects and stored in GameWorld.SceneTemplates dictionary keyed by template identifier. Templates remain in memory throughout game session for spawning reference.

### GameWorld Initialization

After all packages loaded, GameWorld invokes SpawnInitialScenes method. This queries SceneTemplates for templates marked as starter content. For each starter template, SceneSpawner instantiates Scene using procedural generation. Initial Scenes populate game world with starting narrative content before player begins.

### Obligation Integration

Obligation phases can spawn Scenes as completion rewards. ObligationPhase contains list of SceneSpawnRewards defining which templates to spawn and where. When phase completes, ObligationFacade triggers Scene spawning for each reward. This creates dynamic narrative progression tied to multi-phase mystery structure.

Obligations track spawned Scene identifiers for save system and debugging purposes but do not own Scenes. Scene lifecycle controlled by GameWorld.

### UI Rendering Integration

LocationContent component queries active Scenes at current location. For each Scene, retrieves current Situation and renders narrative with Choice cards. Player clicks Choice card, triggering Choice execution flow through SceneFacade.

Locked Choices show requirement details using strongly-typed requirement classes. Available Choices show cost information. Selected Choice triggers requirement validation, cost deduction, reward application, and scene advancement.

### TravelManager Integration

Route obstacles represented as Scenes with PlacementType equals Route. When player travels route encountering obstacle, TravelManager queries GameWorld.Scenes for active Scenes on that route. If found, presents current Situation as route obstacle requiring resolution before travel continues.

Route Situations may have Choices like "Fight bandits" or "Pay toll" or "Find alternate path". Completing Choice resolves obstacle, advances Scene, allows travel to continue. Failed Choices may force player back to origin or create consequence Scenes.

---

## MIGRATION FROM OLD SYSTEM

### Deletion Targets

Old equipment-based Scene system files are completely removed:
- Equipment-context Scene facade
- Equipment intensity DTO and parser
- Scene property reduction DTO and domain model
- Scene reward service for equipment mechanics
- Scene intensity calculator
- Scene filtering service

These files implemented equipment-based challenge modification incompatible with Sir Brante narrative pattern.

### Entity Property Removal

NPC entities no longer have SceneIds list. Scenes reference NPCs via PlacementType and PlacementId instead. UI queries GameWorld.Scenes to find Scenes at specific NPC.

RouteOption entities no longer have SceneIds list. Route obstacles are Scenes with PlacementType equals Route and PlacementId equals route identifier.

Obligation entities replace SceneSpawnInfo list with simple spawned Scene identifier tracking for save system. Actual spawning uses SceneSpawnReward structures in phase completion.

### TravelManager Refactoring

Old code referenced Scene list on routes and checked equipment-based intensity clearance. New code queries GameWorld.Scenes for active Scenes with PlacementType equals Route and matching route identifier. Route obstacle presentation uses current Situation from Scene rather than equipment-intensity model.

---

## VALIDATION PRINCIPLES

### Architecture Compliance

Scenes must be persistent entities stored in GameWorld.Scenes, not ephemeral UI constructs generated per visit.

Situations must be embedded in Scene entity as list, not stored separately in GameWorld.Situations with ID references.

Situations must implement rule cascade through SituationSpawnRules, not simple linear list.

Sir Brante pattern must be enforced: Every Situation has exactly 2-4 Choices, no exceptions.

Choices must have visible requirements and costs, hidden outcomes until selected.

Choices must support spawning new Scenes via reward system.

SceneTemplates must use categorical filters for entity selection, never concrete entity IDs.

Scene spawner must implement procedural generation assigning concrete placements from GameWorld based on filter evaluation.

Placement relations must work correctly: SameLocation, SameNPC, Specific types all functional.

### Old System Removal

All equipment-based Scene system files deleted.

NPC.SceneIds property removed.

RouteOption.SceneIds property removed.

Obligation.ScenesSpawned replaced with proper spawning mechanism.

TravelManager migrated to use Scene-Situation system.

No remaining references to SceneIntensity, SceneContext enums, or equipment reduction concepts.

### Integration Verification

PackageLoader successfully loads SceneTemplates into GameWorld.

GameWorld.SpawnInitialScenes creates starter content.

SceneFacade implements correct methods for spawning, querying, and Choice execution.

UI components render Scenes with narrative, Choices, requirements, costs.

Choice execution validates requirements, deducts costs, applies rewards correctly.

Scene spawning from rewards functions with all placement relation types.

Situation advancement follows spawn rules creating proper cascades.

---

## CONCEPTUAL SUMMARY

The Scene-Situation system implements Sir Brante narrative pattern through persistent Scenes containing embedded Situations organized by spawn rules. Scenes are spawned from templates using procedural generation with categorical filters rather than hardcoded entity references. Situations present 2-4 Choices with requirements, costs, and rewards. Rewards can spawn new Scenes creating dynamic narrative chains. This architecture completely replaces the old equipment-based Scene system and integrates with existing Obligation, Location, NPC, and Route systems.

Key architectural principles:
- Persistence: Scenes stored in GameWorld, not generated per visit
- Embedding: Situations inside Scenes, not separate entities
- Cascades: Spawn rules create Situation progressions
- Filters: Templates use categories, spawner assigns concrete entities
- Spawning: Choices spawn Scenes dynamically with placement relations
- Sir Brante: 2-4 Choices per Situation, visible requirements, hidden outcomes

See situation-spawn-patterns.md for complete catalog of spawn pattern types including Linear, HubAndSpoke, Branching, Converging, Conditional, and 10+ additional patterns.
