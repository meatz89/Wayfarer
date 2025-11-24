# Arc42 Section 8: Crosscutting Concepts

## 8.1 Overview

This section documents patterns, principles, and conventions that span multiple building blocks. These crosscutting concepts ensure architectural consistency and guide decision-making across the entire system.

**Organization:**
- **8.2 Architectural Patterns**: Structural patterns used throughout codebase
- **8.3 Design Principles**: Core design philosophy with priority hierarchy
- **8.4 Domain Concepts**: Gameplay-specific patterns and mechanics
- **8.5 Development Practices**: Coding standards and conventions
- **8.6 Pattern Relationships**: How concepts interact and reinforce each other

---

## 8.2 Architectural Patterns

### 8.2.1 HIGHLANDER Pattern (One Concept, One Representation)

**Core Principle**: "There can be only ONE." One concept gets one representation. Domain entities use object references ONLY. No IDs, no redundant storage, no duplicate paths.

**ENTITY INSTANCE IDs DO NOT EXIST. PERIOD.**

Domain entities (NPC, Location, Route, Scene, Situation) have **NO ID PROPERTIES**. All entity relationships use **DIRECT OBJECT REFERENCES**. This is architectural law, not a guideline.

**The Only Exception: Template IDs**

Templates can have IDs (SceneTemplate.Id, SituationTemplate.Id, ArchetypeId) because templates are **immutable archetypes**, not mutable entity instances:
- ✅ Template IDs acceptable: SceneTemplate.Id, SituationTemplate.Id, ArchetypeId
- ❌ Entity instance IDs FORBIDDEN: Scene.Id, Situation.Id, NPC.Id, Location.Id, Route.Id

**Why the distinction:** Templates are content definitions (like classes). Instances are game state (like objects). Templates don't change during gameplay. Instances do. IDs belong to immutable definitions, not mutable state.

**Pattern**:
- Domain entities have NO ID properties
- Relationships use object references ONLY
- Parser uses categorical properties to find/create entities (EntityResolver.FindOrCreate)
- NEVER store both ID and object reference (violates Single Source of Truth)

**Correct Implementation**:

Entity classes contain only object reference properties, never ID properties. NPC class has Location property holding direct reference to Location object, not LocationId string. Scene class contains direct Situation object reference via CurrentSituation property. All relationships modeled as direct object graph navigation without intermediate ID lookups.

**Parser Pattern (EntityResolver.FindOrCreate)**:

Parser receives categorical filters and queries existing entities by matching properties like Purpose, Safety, LocationProperties. If matching entity found, return existing object reference. If no match, create new entity from categorical properties, add to GameWorld collection, and return new object reference. No ID assignment or ID-based lookup occurs anywhere in resolution process.

**FORBIDDEN Patterns**:

Entity instance classes must never contain ID properties. NPC class with Id property or LocationId property violates architecture. RouteOption storing both OriginLocationId string and OriginLocation object creates redundancy violating HIGHLANDER. SceneSpawnReward with LocationId string for entity instance (not template) forbidden. Player class with ActiveObligationIds list of strings instead of ActiveObligations list of objects violates pattern.

Correct pattern uses only object references: NPC contains Location object property, WorkLocation object property. Player contains ActiveObligations list of Obligation objects. No ID properties exist on entity instances at all.

**Why This Pattern**:
- **Single Source of Truth**: Object IS the truth, no ID to get out of sync
- **Compile-time Safety**: Object references checked by compiler, IDs fail at runtime
- **Domain Clarity**: Entities reference entities, not string lookups
- **Procedural Generation**: Categorical properties enable FindOrCreate pattern

#### Package-Round Entity Tracking (HIGHLANDER Extension)

**Principle**: One entity exists in exactly one package. Initialize ONLY entities from THIS package load round.

Package-round tracking enforces HIGHLANDER principle during package loading and entity initialization. Each package load round creates a PackageLoadResult structure tracking all entities added during that specific load operation. Spatial initialization methods receive explicit entity lists as parameters instead of querying GameWorld collections. This architectural pattern ensures each entity initializes exactly once during its originating package round, making duplicate processing impossible by construction.

**Correct Pattern (Package-Round Tracking)**:
```csharp
// Loading returns result tracking THIS round's entities
PackageLoadResult result = LoadPackageContent(package);

// Spatial initialization receives ONLY new entities
PlaceLocations(result.LocationsAdded); // Process explicit list
PlaceVenues(result.VenuesAdded);       // Not GameWorld queries
```

**Forbidden Pattern (GameWorld Iteration)**:
```csharp
// FORBIDDEN: Queries all entities in GameWorld
private void PlaceAllLocations()
{
    List<Location> allLocations = _gameWorld.Locations.ToList(); // ❌ ALL entities
    List<Location> newLocations = allLocations.Where(l => l.HexPosition == null); // ❌ State check
}

// FORBIDDEN: Re-processes existing entities every package load
LoadPackageContent(package);
PlaceAllLocations(); // ❌ Iterates ALL locations including already-placed ones
```

**Why Package-Round Tracking**:
- **Performance**: O(m) where m = new entities vs O(n) where n = total GameWorld size
- **Architectural Purity**: No entity state checks (`HexPosition == null`) needed for deduplication
- **HIGHLANDER Enforcement**: One entity initialized exactly once, violation impossible
- **Explicit Data Flow**: Entities flow through parameters, not hidden queries
- **Package Isolation**: Each round processes only its own entities, no cross-contamination

**Two Loading Scenarios**:

**Static Loading (Startup)**: Multiple packages loaded, results accumulated, spatial initialization called ONCE with aggregated entity lists. Pattern aggregates via SelectMany collecting all entities across packages, then passes complete lists to initialization methods. Performance is O(n) where n equals total entities across all packages, executing spatial initialization single time.

**Dynamic Loading (Runtime)**: Single package loaded, result used directly, spatial initialization called IMMEDIATELY with current round's entity lists. Pattern passes result lists directly to initialization methods without accumulation. Performance is O(m) where m equals new entities from this package only, constant-time relative to existing GameWorld size.

**Architectural Enforcement**:

Methods cannot re-process existing entities because they never receive them as parameters. Spatial initialization signature requires explicit `List<Location>` parameter, making GameWorld queries impossible. PackageLoadResult contains only entities from current round, guaranteeing isolation. Entity state checks forbidden - if entity received as parameter, it's guaranteed uninitialized. This makes correct behavior the ONLY possible behavior.

**Related**: See ADR-015 (Package-Round Entity Tracking) and Section 6.2 (Runtime View - Package-Round Tracking Pattern) for complete implementation details.

---

### 8.2.2 Catalogue Pattern (Parse-Time Translation)

**Core Principle**: JSON has categorical properties, catalogues translate to concrete types at parse-time, runtime uses concrete types directly.

#### Three Phases

**Phase 1: JSON Authoring (Categorical Properties)**
- Authors/AI write descriptive: "Friendly" not 15, "Premium" not 1.6
- Categorical enums: `NPCDemeanor.Friendly`, `Quality.Premium`
- Why: AI can generate without knowing global game state, self-documenting, enables dynamic scaling

**Phase 2: Parsing (Translation - ONE TIME ONLY)**
- Catalogues translate: `NPCDemeanor.Friendly` → StatThreshold = 3 (scaled by 0.6×)
- Called from Parsers folder ONLY
- Translation deterministic, results stored in GameWorld collections

**Phase 3: Runtime (Concrete Values ONLY)**
- NO catalogue calls at runtime
- NO string matching, NO dictionary lookups
- Direct property access: `if (player.Stat >= choice.RequiredStat)`

#### Catalogue Generation Example

Catalogue methods receive archetype template and generation context containing categorical properties like NpcDemeanor and PowerDynamic. Scale base threshold by multipliers derived from categorical enums: Friendly demeanor applies 0.6 multiplier, Superior power dynamic applies 1.4 multiplier. Return ChoiceTemplate with concrete StatThreshold and CoinCost integers ready for runtime evaluation. No further catalogue calls needed once template generated.

#### FORBIDDEN Forever

- ❌ Runtime catalogue calls (parse-time ONLY)
- ❌ String matching: checking action ID equals specific string literal
- ❌ Dictionary lookups: accessing costs via string keys or ContainsKey checks
- ❌ ID-based routing: Entity IDs are reference only, never control behavior

---

### 8.2.3 Template vs Archetype Architecture

**Core Distinction**: Templates are DATA (stored), Archetypes are CODE (generators that create templates).

**The Critical Difference**:
- **Templates** = Immutable data structures loaded from JSON or generated at parse-time, stored in GameWorld collections, used at runtime
- **Archetypes** = Static C# catalog methods that procedurally GENERATE template structures, never stored, never called at runtime

#### Templates (Data Structures)

**What Templates Are**:
- SceneTemplate and SituationTemplate domain entity classes
- Immutable data representing specific content instances
- Loaded from JSON files OR generated by archetype code at parse-time
- Stored in GameWorld.SceneTemplates collection after parsing
- Used at runtime for scene spawning and situation progression

**Two Sources of Templates**:
1. **Authored Templates**: Directly defined in JSON files, parsed into template objects
2. **Generated Templates**: Created by archetype code during parsing, indistinguishable from authored after generation

**Template Lifecycle**: Parse-time creation → Store in GameWorld → Runtime usage (spawn scenes, progress situations)

#### Archetypes (Code Generators)

**What Archetypes Are**:
- Static catalog methods in SceneArchetypeCatalog.cs and SituationArchetypeCatalog.cs
- Pure C# code, not data structures
- Generate SceneArchetypeDefinition objects containing SituationTemplates at parse-time
- Invoked by parsers ONLY during content loading
- Never stored in GameWorld, never called at runtime

**Archetype Catalog Methods**:
SceneArchetypeCatalog contains static methods like GenerateInnLodging, GenerateDeliveryContract. Each method receives context parameters (tier, mainStorySequence, categorical properties). Method procedurally generates situation templates with calculated thresholds and rewards. Returns SceneArchetypeDefinition containing generated templates ready for storage.

**Why Code Not Data**: Archetypes contain generation logic (conditional thresholds, context-based scaling, procedural narrative composition). This logic CANNOT be represented in static JSON - requires code execution. Archetypes enable dynamic content generation while templates represent static immutable content.

#### Generation Flow

**Parse-Time Generation Sequence**:

Phase 1 (JSON Declaration): JSON specifies sceneArchetypeId string identifying generator method. Example: sceneArchetypeId equals inn_lodging or delivery_contract string.

Phase 2 (Parser Invocation): SceneTemplateParser recognizes archetype identifier. Calls SceneArchetypeCatalog.Generate with archetype identifier, tier number, and generation context containing categorical properties.

Phase 3 (Archetype Execution): Archetype code method executes generating situation templates. Applies context-aware scaling (tutorial gets lower thresholds, higher tiers get harder challenges). Composes multiple situations into cohesive scene structure. Returns SceneArchetypeDefinition containing all generated templates.

Phase 4 (Template Storage): Parser creates SceneTemplate from archetype definition. SceneTemplate stored in GameWorld.SceneTemplates collection. Original archetype code never stored, only its generated output persists.

Phase 5 (Runtime Usage): Runtime spawns scenes from stored SceneTemplate. NO archetype code called during gameplay. Templates contain all data needed for execution.

**Critical Point**: Archetypes are procedural generation systems active ONLY at parse-time. Templates are the frozen generated data used at runtime.

#### Why This Architecture

**Separation of Generation from Usage**:
- Generation logic (code) separate from generated content (data)
- Complex procedural logic in archetypes, simple data access at runtime
- Parse-time complexity, runtime simplicity

**Reusable Patterns Without Duplication**:
- One archetype generates infinite template variations through context parameters
- Tutorial inn_lodging scene uses SAME archetype as endgame inn_lodging scene, different thresholds through tier parameter
- Avoids duplicating JSON for every difficulty variant

**Content Authoring Flexibility**:
- Authors can write direct JSON templates (full control, explicit definition)
- OR authors can reference archetypes (reusable patterns, context-driven variation)
- Both produce identical SceneTemplate structures in GameWorld after parsing

**Performance Optimization**:
- Archetype generation happens ONCE at parse-time (startup cost)
- Runtime NEVER calls catalog code (no generation overhead during gameplay)
- Templates are pre-computed data structures (direct property access)

#### Common Confusion

**Templates Are NOT Archetypes**: Documentation sometimes calls templates "immutable archetypes" - this is INCORRECT terminology. Templates are data GENERATED BY archetypes, not archetypes themselves. Archetype is the generator (code), template is the product (data).

**SpawnPattern Is NOT An Archetype**: SpawnPattern enum (Linear, Branching, Parallel) is a PROPERTY on SceneTemplate describing situation ordering, not an archetype. Archetype is the code that generates the template containing that property.

**Archetype Composition Pattern**: Scene archetypes call situation archetypes during generation. SceneArchetypeCatalog.Generate method invokes SituationArchetypeCatalog.GetArchetype to compose multi-situation structures. This is code composition at generation time, not data composition at runtime.

**EntityResolver Uses Templates Not Archetypes**: When situations need location or NPC binding, EntityResolver queries existing entities using PlacementFilter categorical properties. This operates on parsed template data (SceneTemplate, SituationTemplate), never calls archetype generation code. Runtime resolution uses stored data, not generation logic.

#### The Three Content Systems

**System 1: Templates (Data - Parsed and Stored)**
- SceneTemplate, SituationTemplate classes
- Source: JSON files or archetype generation
- Storage: GameWorld.SceneTemplates collection
- Usage: Runtime scene spawning and progression

**System 2: Archetypes (Code - Generation Logic)**
- SceneArchetypeCatalog, SituationArchetypeCatalog static methods
- Invocation: Parse-time ONLY by parsers
- Never stored, never called at runtime
- Output: SceneArchetypeDefinition containing generated templates

**System 3: PlacementFilter (Categorical Binding)**
- LocationFilter, NpcFilter properties on templates
- Resolution: EntityResolver queries entities by categorical properties
- Operates on template data at scene spawn time
- Binds template specifications to actual game world entities

**Integration**: Archetype generates template with PlacementFilter properties, template stored in GameWorld, runtime spawns scene using template and resolves filters to entities through EntityResolver.

**Rationale**: Clear separation between generation (code archetypes), storage (data templates), and binding (categorical resolution). Parse-time complexity isolated from runtime simplicity. Enables both authored control (direct JSON) and procedural variation (archetype generation) through unified template storage.

---

### 8.2.4 Three-Tier Timing Model

**Core Principle**: Content exists in three timing tiers to enable lazy instantiation and reduce memory.

#### Tier 1: Templates (Parse Time)

**When**: Game startup during JSON parsing

**What**: Immutable archetypes defining reusable patterns

**Characteristics**:
- SceneTemplate contains embedded SituationTemplates
- SituationTemplate contains embedded ChoiceTemplates
- Created ONCE from JSON
- Stored in GameWorld.SceneTemplates
- NEVER modified during gameplay

**Structure**: SceneTemplate class contains Id property and list of SituationTemplate objects representing embedded template hierarchy.

#### Tier 2: Scenes/Situations (Spawn Time)

**When**: Scene spawns from Obligation or trigger

**What**: Runtime instances with lifecycle and mutable state

**Characteristics**:
- Scene instance created with embedded Situations
- Situation.Template reference stored (ChoiceTemplates NOT instantiated yet)
- InstantiationState = Deferred
- NO actions created in GameWorld collections yet

**Why Deferred**: Creating actions prematurely bloats GameWorld with thousands of inaccessible actions. Player may never reach all situations.

#### Tier 3: Actions (Query Time)

**When**: Player enters context where situation becomes accessible

**What**: Concrete actions queryable by UI, placed in GameWorld collections

**Process**:
1. Check if situation's context met (player at required location/NPC)
2. If met, instantiate actions from Situation.Template.ChoiceTemplates
3. Create concrete Action entities (LocationAction, NPCAction, PathCard)
4. Add to GameWorld.LocationActions/NPCActions/PathCards
5. Set Situation.InstantiationState = Instantiated

**Cleanup**: When scene completes/expires, actions removed from GameWorld

#### Complete Flow

```
PARSE TIME:
JSON → SceneTemplateParser → SceneTemplate
                            → Stored in GameWorld.SceneTemplates

SPAWN TIME:
Obligation triggers → SceneInstantiator → Scene (with Situations)
                                        → InstantiationState = Deferred
                                        → Stored in GameWorld.Scenes

QUERY TIME:
Player at Location → SceneFacade queries → If Deferred + Context Met
                                         → Instantiate Actions
                                         → Add to GameWorld.LocationActions
                                         → Set InstantiationState = Instantiated
```

#### CRITICAL: Two Action Generation Patterns

**Actions exist in TWO architectural forms with different timing:**

**1. Atmospheric/Static LocationActions (Parse-Time, Tier 1):**
- Generated ONCE during package loading via LocationActionCatalog
- Stored PERMANENTLY in GameWorld.LocationActions
- Always available (Travel, Work, Rest, Intra-Venue Movement)
- NEVER deleted (persistent gameplay scaffolding)
- Purpose: Prevent dead ends, ensure player freedom

**Example Flow:**
```
PARSE TIME:
JSON Package Load → LocationActionCatalog.GenerateActionsForLocation()
                                        → Create LocationAction entities
                                        → Add to GameWorld.LocationActions
                                        → Stored permanently
```

**2. Scene-Based Ephemeral Actions (Query-Time, Tier 3):**
- Generated fresh when SceneFacade queries active Situation
- Passed by direct object reference to UI (NOT stored in GameWorld)
- Available only while parent Scene/Situation active
- Discarded after execution
- Purpose: Dynamic narrative content
- Types: LocationAction (scene at location), NPCAction (scene at NPC), PathCard (scene on route)

**Example Flow:**
```
QUERY TIME:
Player at Location → SceneFacade.GetActionsAtLocation()
                                         → Create ephemeral LocationAction from ChoiceTemplate
                                         → Return directly to UI (NO storage)
                                         → UI passes to GameFacade.ExecuteLocationAction()
                                         → Action discarded after execution
```

**Key Distinction:**
- LocationAction exists in BOTH forms (static atmospheric AND ephemeral scene-based)
- NPCAction and PathCard are ONLY ephemeral (no static equivalent)
- GameWorld.LocationActions stores ONLY static atmospheric actions
- Ephemeral actions passed by object reference, never stored

**Why This Matters:**
- Nearly deleted LocationActionCatalog generation thinking it was obsolete
- Static atmospheric actions are CORE GAMEPLAY (travel/work/rest)
- Deleting static actions would remove player's ability to earn money and progress
- Both patterns must coexist: persistent scaffolding + dynamic narrative

---

### 8.2.4 Let It Crash Pattern

**Core Principle**: Fail fast with descriptive errors. No graceful degradation, no defaults, no hiding problems.

#### Philosophy

**WRONG**: Detecting null data and creating default fallback object silently hides missing data problem. Code continues executing with incorrect state, bugs manifest far from root cause.

**CORRECT**: Detecting null data and throwing InvalidOperationException with descriptive message indicating expected data source forces fixing root cause immediately. Crash reveals problem location in stack trace.

#### Application Areas

**1. Entity Initialization**

Collection properties initialized to empty list in property declaration. Parser assigns directly to property trusting initialization never null. Runtime queries collection count trusting initialization. If somehow null, crash immediately revealing initialization bug rather than hiding with null check.

**2. Parse-Time Validation**

When searching GameWorld collections for referenced template, if FirstOrDefault returns null, throw InvalidDataException with template ID in message. Parse-time validation ensures all references resolve before gameplay starts. Runtime trusts parse-time validation completed successfully.

**3. Missing Data**

Forbidden pattern: returning nullable StatThreshold with null-coalescing operator defaulting to hardcoded value. Correct pattern: checking if StatThreshold equals zero (unset sentinel) and throwing InvalidOperationException with choice identifier in message. Forces content author to set required property rather than masking missing data.

#### When NOT to Let It Crash

- **User input validation**: Graceful error messages appropriate
- **External API failures**: Retry logic appropriate
- **Expected gameplay conditions**: Player not meeting requirements (show UI feedback, don't crash)

**Expected Gameplay State**: When player lacks required coins for purchase, return ValidationResult with IsValid false and user-friendly message. This represents normal gameplay state, not architectural error. Don't crash on expected player resource shortages.

---

### 8.2.5 Sentinel Values Pattern

**Core Principle**: Never use null for domain logic. Create explicit sentinel objects with internal flags.

#### Problem with Null

Storing SpawnConditions property as null creates ambiguity: does null mean always eligible or never eligible? Requires null checks scattered throughout codebase. Easy to forget null check before calling methods on conditions object.

**Issues**: Ambiguous meaning, null checks everywhere, easy to forget checks

#### Sentinel Solution

SpawnConditions class contains private boolean flag _isAlwaysEligible. Static factory property AlwaysEligible returns new instance with flag set true. IsEligible method checks flag first, returning true immediately if always eligible flag set, otherwise evaluating actual conditions.

Property initialization sets SpawnConditions property to SpawnConditions.AlwaysEligible sentinel by default. No null checks needed in calling code, just call IsEligible method directly knowing object always exists.

#### Benefits

1. **Explicit intent**: SpawnConditions.AlwaysEligible is clear
2. **No null checks**: Safe to call methods
3. **Type safety**: Compiler enforces correct usage
4. **Self-documenting**: Code reads naturally

#### When to Use

- **Domain defaults**: "Always eligible", "No requirements", "Unlimited"
- **Optional complex objects**: Better than null for objects with methods
- **State machines**: Explicit "None" or "Initial" states

#### When NOT to Use

- **Simple optionals**: `int?`, `string?` are fine
- **Collections**: Empty `List<T>` is the sentinel (never null)
- **Strings**: Empty string `""` is the sentinel (never null)

---

### 8.2.6 Requirement Inversion Pattern

**Core Principle**: Entities spawn into world immediately, requirements filter visibility/selectability, not spawning.

#### Traditional (Boolean Gate) - WRONG

Traditional approach checks boolean flag on player (CompletedQuest), then creates new quest content only after check passes. Content doesn't exist until boolean unlock triggered. Player cannot see what's locked or requirements to unlock.

**Problems**: Hidden content, boolean gates, checklist completion, no strategic planning

#### Requirement Inversion - CORRECT

Phase 2 scene exists in GameWorld from game start. SpawnConditions property contains eligibility criteria. When querying for available scenes, check IsEligible with player state to filter visibility. UI displays scene with locked/unlocked visual indicator based on eligibility check.

Player sees phase 2 exists, sees exact requirements (Understanding >= 5, CompletedScene("phase1")), and can plan resource allocation to meet requirements. Perfect information enables strategic decision-making.

#### Benefits

- **Perfect Information**: Player sees what's locked and exact requirements
- **Resource Arithmetic**: Numeric comparisons (`player.Rapport >= 6`) instead of boolean flags
- **State-Based Visibility**: Content visible/locked based on game state, not creation flags
- **Resource Competition**: Shared resources force trade-offs, not checklist completion

#### Resource Layers

1. **Personal Stats**: Capability thresholds (Insight, Rapport, Authority, etc.)
2. **Per-Person Relationships**: Individual capital with each NPC
3. **Permanent Resources**: Health, Stamina, Focus, Resolve, Coins
4. **Time as Competition**: Calendar days, time blocks per day
5. **Ephemeral Context**: Current location, active scenes, NPC availability

**Full Documentation**: See ADR-002 in section 9 for comprehensive explanation.

---

### 8.2.7 Multi-Scene NPC Interaction Pattern

**Core Principle**: A single NPC can have multiple independent active scenes simultaneously. Each scene represents a distinct narrative thread with separate lifecycle, situations, and completion state.

#### Physical Presence vs Interactive Opportunities

**Physical Presence (Always Visible)**:
- NPCs exist in game world as physical entities
- When NPC present at location, player always sees them listed
- Represents fiction: "Elena is standing near the fireplace"

**Interactive Opportunities (Conditional)**:
- Interaction buttons appear only when NPC has active scenes
- Each active scene spawns separate button with descriptive label
- Represents available conversation topics or interaction contexts

**Example**:
```
Elena at Common Room (physical presence shown)
├─ Active Scene 1: "Secure Lodging" → Button: "Secure Lodging"
└─ Active Scene 2: "Inn Trouble Brewing" → Button: "Discuss Inn Trouble"

Thomas at Common Room (physical presence shown)
└─ No active scenes → No buttons
```

#### Scene Independence

Each scene maintains independent lifecycle:
- Scene A: "Secure Lodging" (4 situations cascading sequentially)
- Scene B: "Inn Trouble Brewing" (3 situations cascading sequentially)

Completing Scene A does not affect Scene B. Both remain visible and independently playable until each reaches completion criteria.

#### Sequential Situations Within Scenes

Within single scene, situations flow sequentially without interruption:

1. Player clicks "Secure Lodging" button
2. Scene activates, shows Situation 1
3. Player selects choice → Scene cascades to Situation 2 (no return to location view)
4. Player selects choice → Scene cascades to Situation 3
5. Player selects choice → Scene completes, returns to location view

Scene state machine manages CurrentSituation (object reference) and AdvanceToNextSituation() for seamless narrative flow.

#### Perfect Information Requirement

Players must see ALL available interaction options for strategic decisions. Hiding scenes because they lack aesthetic labels violates perfect information principle. Architecture prioritizes functionality over cosmetics:

- Scene exists + active situation → Show button (even with placeholder label)
- No active scene → No button (nothing to engage)

#### Multi-Scene Display Pattern

Architecture shift from single-scene to multi-scene:

**Before (Single Scene)**: Query used FirstOrDefault to return single active scene matching NPC. ViewModel contained single InteractionLabel property for button text.

**After (Multi-Scene)**: Query uses Where to return all active scenes matching NPC as list. ViewModel contains AvailableScenes property holding list of NpcSceneViewModel descriptors.

#### Label Derivation Hierarchy

When deriving button labels for scenes, use fallback hierarchy:

1. Scene.DisplayName (explicit authored label)
2. First Situation.Name in scene (derived from situation content)
3. Placeholder "Talk to [NPC Name]" (functional default)

Never hide functional scene because it lacks pretty label. Playability trumps aesthetics.

#### Navigation Routing

When player clicks scene button, navigation must route to SPECIFIC scene:

**Before (Ambiguous)**: Button click passed only npcId. Navigation searched for any active scene at NPC using FirstOrDefault. Ambiguous when multiple scenes exist - which scene should load?

**After (Explicit)**: Button click passes both npcId and sceneId as pair. Navigation uses sceneId for direct lookup via First, ensuring exact scene routing without ambiguity.

#### Spawn Independence

Scenes spawn independently from different sources:

- Tutorial scenes: Spawn at parse-time (categorical filters, same as all content)
- Obligation scenes: Spawn at runtime (categorical filters)
- Multiple obligations: Can spawn scenes at same NPC simultaneously
- Each scene: Operates independently until completion

This architectural pattern supports rich narrative branching where NPCs serve as hubs for multiple concurrent story threads.

#### Implementation Requirements

1. **Query Pattern**: Use Where not FirstOrDefault when fetching NPC scenes
2. **ViewModel Structure**: Support list of available scenes per NPC
3. **UI Rendering**: Loop through available scenes, render one button per scene
4. **Navigation**: Pass both npcId and sceneId for explicit routing
5. **Label Priority**: Prefer Scene.DisplayName, fallback to Situation.Name, fallback to placeholder

---

### 8.2.8 5-System Scene Spawning Architecture

**Core Principle**: Scene spawning flows through five distinct systems, each with one clear responsibility. Placement uses categorical filters at all stages. Entity resolution happens eagerly in System 4 before scene instantiation in System 5.

#### System 1: Scene Selection (Decision Logic)

**Responsibility**: Decide WHEN to spawn scene based on game state

**Location**: SceneFacade, SituationRewardExecutor

**Key Operations**:
- Evaluate SpawnConditions on SceneTemplate (RequiredTags, MinDay, StatThresholds)
- Check eligibility when choice executed with ScenesToSpawn reward
- Trigger System 2 if conditions met

**Eligibility Check**: SceneFacade method receives SceneTemplate and Player. Checks if any required tags missing from player tag collection, returns false if missing. Checks if current day less than minimum day requirement, returns false if too early. Returns true if all conditions met, proceeding to spawning.

#### System 2: Scene Specification (Data Structure)

**Responsibility**: Store categorical requirements for spawning, NO concrete entity IDs

**Location**: SceneSpawnReward class, ChoiceReward property

**Data Structure**: SceneSpawnReward class contains SceneTemplateId property (categorical template reference) and optional PlacementFilterOverride property for filter modifications. Contains NO ContextBinding, NO PlacementRelation enum, NO SpecificPlacementId. Categorical properties ONLY expressing spawn requirements.

**Key Principle**: Scene rewards contain ONLY categorical requirements. NO concrete entity IDs. All placement needs expressed via categorical filters on template or override.

#### System 3: Package Generator (SceneInstantiator)

**Responsibility**: Create JSON package specification with PlacementFilterDTO (categorical specs), does NOT resolve entities

**Location**: SceneInstantiator service

**Key Operations**:
- Receive SceneSpawnReward with categorical template ID
- Load SceneTemplate from GameWorld
- Write PlacementFilterDTO to JSON package (LocationFilter, NpcFilter, RouteFilter properties)
- Does NOT call FindOrCreate (no entity resolution here)
- Does NOT write concrete entity IDs to JSON
- Output: SceneDTO package with categorical filters

**Package Generation**: GenerateScenePackage method receives SceneSpawnReward, loads template from GameWorld by template ID. Creates SceneDTO with generated scene ID and template reference. Writes categorical filters from template directly to DTO LocationFilter, NpcFilter, RouteFilter properties without resolving to concrete entities. Generates embedded SituationDTOs from template situation collection. Returns SceneDTO package ready for System 4 resolution.

**What This Does NOT Do**:
- Does NOT resolve entities (no FindOrCreate calls)
- Does NOT write concrete entity IDs (like "elena" or "common_room_inn")
- Does NOT use PlacementRelation enum (deleted concept)
- Does NOT use ContextBinding (deleted concept)

**What This DOES**:
- Writes categorical requirements to JSON (PlacementFilterDTO)
- Packages complete scene specification
- Passes categorical specs to System 4 for resolution

#### System 4: Entity Resolver (EntityResolver in PackageLoader)

**Responsibility**: Resolve categorical filters to concrete entity objects using FindOrCreate pattern

**Location**: EntityResolver service, called by PackageLoader

**Key Operations**:
- Read PlacementFilterDTO from JSON package
- FindOrCreate pattern for each entity type:
  - FindOrCreateLocation(filter) → Location object
  - FindOrCreateNPC(filter) → NPC object
  - FindOrCreateRoute(filter) → RouteOption object
- Query existing entities FIRST (reuse via categorical matching)
- Generate new entities if no match (eager creation when needed)
- Return concrete entity OBJECTS (not IDs)

**Location Resolution**: FindOrCreateLocation method receives PlacementFilterDTO and GameWorld. Step 1 queries existing locations using Where to match filter properties. If existing match found, return existing object for reuse. Step 2 (if no match) generates new Location from filter categorical properties via GenerateLocation, adds to GameWorld, returns newly generated object. Result is Location object reference, never ID string.

NPC resolution follows identical pattern: query existing NPCs matching filter, return existing if found, otherwise generate and return new NPC object.

**Key Principle**: Entities resolved from categorical properties, returned as objects (NOT IDs). System 5 receives pre-resolved objects.

#### System 5: Scene Instantiator (SceneParser in PackageLoader)

**Responsibility**: Create Scene entity with direct object references from pre-resolved entities

**Location**: SceneParser, called by PackageLoader after System 4

**Key Operations**:
- Receive pre-resolved entity objects as parameters (Location, NPC, RouteOption)
- Create Scene with direct object references:
  - Scene.Location = resolvedLocation (object property)
  - Scene.Npc = resolvedNpc (object property)
  - Scene.Route = resolvedRoute (object property)
- NO resolution logic (objects already resolved by System 4)
- NO PlacementType enum dispatch (deleted concept)
- NO string ID lookups

**Scene Creation**: CreateScene method receives SceneDTO plus pre-resolved Location, NPC, and RouteOption objects from System 4. Creates Scene with ID and TemplateId from DTO. Assigns direct object references: Location property receives resolvedLocation parameter, Npc property receives resolvedNpc, Route property receives resolvedRoute. Parses embedded Situations from DTO. Returns complete Scene with all references resolved to objects.

**What This Does NOT Do**:
- Does NOT resolve entities (System 4 did that)
- Does NOT use PlacementType enum + PlacementId string (deleted pattern)
- Does NOT look up entity IDs (receives objects directly)
- Does NOT use MarkerResolutionMap (deleted concept)

**What This DOES**:
- Create Scene with direct object references
- Simple scene construction with pre-resolved dependencies
- Clean separation from resolution logic

#### Complete Data Flow Example

**Scenario**: Choice reward spawns "investigate_mill" scene

**System 1 - Decision**: Choice executed with SceneSpawnReward containing template ID "investigate_mill" and null filter override. SceneFacade checks eligibility against player state. If eligible, proceeds to System 2.

**System 2 - Specification**: Reward data structure contains only SceneTemplateId "investigate_mill". No concrete IDs, no PlacementRelation enum, no ContextBinding. Categorical specification only.

**System 3 - Package Generation**: SceneInstantiator writes SceneDTO to JSON. DTO contains generated scene GUID, template ID reference, and categorical filters from template. LocationFilter specifies LocationProperties array containing "Indoor" and "Private", plus LocationTags array containing "industrial" and "mill". No concrete location IDs written. NpcFilter and RouteFilter null for this scenario.

**System 4 - Entity Resolution**: EntityResolver reads LocationFilter from DTO. Calls FindOrCreateLocation which queries existing GameWorld locations matching Indoor property, Private property, and industrial/mill tags. Returns existing mill location if found, OR generates new mill location if no match. Result is Location object reference, not ID string.

**System 5 - Scene Instantiation**: SceneParser receives DTO and pre-resolved Location object from System 4. Creates Scene with direct object property assignment: scene.Location equals millLocation object received as parameter. No ID lookups, no PlacementType dispatch, direct object reference only.

**Result**: Scene created with categorical resolution → query/generate → direct object references. No PlacementRelation enum, no ContextBinding, no MarkerResolutionMap, no string ID lookups.

#### Why This Architecture

**HIGHLANDER Compliance**: One placement mechanism (categorical filters), one resolution pattern (FindOrCreate), one reference pattern (direct objects).

**Separation of Concerns**:
- System 1: WHEN (eligibility checking)
- System 2: WHAT (categorical specification)
- System 3: HOW (package generation)
- System 4: WHERE/WHO (entity resolution)
- System 5: ASSEMBLY (scene construction)

**No Enum Dispatch**: Scene has Location/Npc/Route object properties, NOT PlacementType enum + PlacementId string.

**Eager Resolution**: Entities resolved before scene construction, not lazily during scene usage.

**Categorical Throughout**: Filters used from reward → package → resolution. No concrete IDs until System 4 resolution.

---

### 8.2.9 Dynamic World Building (Lazy Materialization Pattern)

**Core Principle**: World expands in response to narrative need, not pre-emptively. Locations and venues materialize when scenes spawn, validated for playability. **All generated locations persist forever** - no cleanup system exists.

#### Pattern Structure

**Two-Phase Lifecycle**:
1. **Generation**: Scene spawns with DependentLocationSpec → Budget validation (fail-fast) → SceneInstantiator generates LocationDTO → PackageLoader parses → Location entity added to GameWorld
2. **Gameplay**: Location used during scene → Player may visit → Location becomes permanent world feature

**Data Flow**:
```
DependentLocationSpec (Template)
    ↓ Scene Spawn
Capacity Validation (fail-fast if venue full: LocationIds.Count >= MaxLocations)
    ↓ Pass
LocationDTO (JSON generated at runtime)
    ↓ PackageLoader
Location Entity (parsed into GameWorld, indistinguishable from authored)
    ↓ Gameplay
Location Persists Forever (no cleanup)
```

#### Components

**Generation**:
- `DependentLocationSpec`: Template defining location to generate (NamePattern, Properties, HexPlacement)
- `VenueTemplate`: Template for procedural venue generation (Type, Tier, District, MaxLocations)
- `SceneInstantiator.BuildLocationDTO()`: DTO generation with fail-fast capacity validation
- `VenueGeneratorService`: Generate venues with hex allocation and capacity budgets

**Matching**:
- `PlacementFilter`: Categorical property matching (LocationProperties, LocationTags, DistrictId)
- `SceneInstantiator.FindMatchingLocation()`: Query existing locations by categorical properties
- `PlacementSelectionStrategy`: Choose ONE from multiple matches (Closest, LeastRecent, WeightedRandom)

**Validation**:
- `LocationPlayabilityValidator`: Fail-fast validation of playability for ALL locations (hex position, reachability, venue, properties, unlock mechanism)
- Capacity validation: Generated checked BEFORE DTO creation (SceneInstantiator), authored checked AFTER parsing (PackageLoader)

**Synchronization**:
- `HexSynchronizationService`: Maintain HIGHLANDER (Location.HexPosition = source, Hex.LocationId = derived)

**Tracking**:
- `SceneProvenance`: Metadata tracking creation source (for debugging only, not lifecycle decisions)
- `Venue.MaxLocations`: Total capacity budget (counts ALL locations: authored + generated)
- `Venue.LocationIds`: Bidirectional relationship maintained by GameWorld.AddOrUpdateLocation
- Budget derived (LocationIds.Count) not tracked (Catalogue Pattern compliance)
- No locking needed: Blazor Server is single-threaded (07_deployment_view.md § Server Resources: CPU)

#### Design Decisions

**Match First, Generate Last**:
- PlacementFilter attempts categorical matching FIRST
- DependentLocationSpec triggers explicit generation (not fallback)
- No silent fallback from matching to generation
- Fail-fast if no match and no explicit generation spec

**Rationale**: Authored content priority. If filter can't find match, either author matching content OR relax filter constraints OR add explicit DependentLocationSpec. Never silently degrade.

**All Locations Persist Forever**:
- No cleanup system exists
- Generated locations become permanent world features
- Provenance tracks creation source (metadata only, not lifecycle)
- Budget enforcement critical since violations cannot be cleaned up

**Rationale**: Simplifies architecture. Locations represent player's narrative journey - deleting them erases history. Budget validation prevents unbounded growth instead of cleanup.

**Bounded Infinity Through Fail-Fast Capacity**:
- Venues have MaxLocations capacity (default 20)
- BuildLocationDTO checks capacity BEFORE DTO creation (LocationIds.Count < MaxLocations)
- Throws InvalidOperationException if venue at capacity
- Small venues (5), medium venues (20), large venues (100), wilderness (unlimited)

**Rationale**: Since locations persist forever, budget violations cannot be cleaned up. Prevention through fail-fast validation is essential. Forces spatial design decisions at authoring time.

**Fail-Fast Validation**:
- LocationPlayabilityValidator throws on unplayable content (ALL locations)
- Validation checks: hex position, reachability, venue, properties, unlock mechanism
- System crashes rather than creating inaccessible content
- Catalogue Pattern: No distinction between authored/generated during validation

**Rationale**: Unplayable content worse than crash. Forces fixing root cause in content authoring. Playability over compilation.

#### Integration with Catalogue Pattern

**Generation Flows Through Standard Pipeline**:
1. SceneInstantiator generates LocationDTO (same structure as authored JSON)
2. LocationDTO serialized to Package JSON
3. PackageLoader loads package (same path as authored content)
4. LocationParser parses LocationDTO → Location entity
5. Location added to GameWorld.Locations

**Rationale**: Generated content indistinguishable from authored content after parsing. Same validation, same resolution, same entity structure. Catalogue Pattern compliance.

#### Example: Self-Contained Scene with Private Room

**Template Specification**: Scene archetype JSON defines dependentLocations array with object specifying templateId "private_room", namePattern substitution "{NPCName}'s Private Room", venueIdSource "SameAsBase", hexPlacement "Adjacent", properties array containing "sleepingSpace", "restful", "indoor", "private", and isLockedInitially true.

**Generation (Scene Spawn)**:
1. SceneInstantiator reads DependentLocationSpec
2. Checks venue capacity: Can add? → Yes (LocationIds.Count < MaxLocations)
3. Generates LocationDTO with NamePattern resolved ("Elena's Private Room")
4. Finds adjacent hex to base location (venue cluster)
5. Creates Package JSON with generated LocationDTO
6. PackageLoader parses → Location entity created (indistinguishable from authored)
7. Provenance stored: SceneProvenance with SceneId "scene_tutorial_001" (metadata only)

**Gameplay**:
- Player negotiates with Elena → Receives room_key item
- Player unlocks private room → Location used during gameplay
- Player rests → Resource restoration based on room properties
- **Location persists forever** → Never deleted, becomes permanent world feature

#### Scene-Specific Dependent Location Binding (LocationTags System)

**Core Principle**: Situations must reference dependent locations created by their parent scene WITHOUT using entity instance IDs. LocationTags provides scene-specific binding through marker transformation.

**The Problem**: Scene creates private room, Situation 2 needs to be placed AT that private room. Cannot use LocationId (violates HIGHLANDER - no entity instance IDs). Cannot use generic categorical filter (might match wrong location from different scene). Need scene-specific binding through object references.

**Three-Phase Marker Transformation Pattern**:

**Phase 1: Authoring (Marker Declaration)**
- Situation template declares LocationTags with DEPENDENT_LOCATION marker prefix
- Marker format specifies which dependent location to bind: DEPENDENT_LOCATION:private_room references dependent location with templateId private_room
- Marker is placeholder resolved at scene instantiation time
- Authoring uses generic markers, not scene-specific values

**Phase 2: Scene Instantiation (Marker Transformation)**
- SceneInstantiator transforms marker to scene-specific tag
- Transformation adds unique scene identifier prefix: DEPENDENT_LOCATION:private_room becomes sceneId_private_room
- Scene creates dependent location with matching DomainTags containing sceneId_private_room
- Transformation binds situation to specific scene instance dependent location
- Multiple scenes with same template create different bindings through unique scene identifiers

**Phase 3: Entity Resolution (Tag Matching)**
- EntityResolver.FindMatchingLocation receives PlacementFilter with transformed LocationTags
- EntityResolver checks filter LocationTags against location DomainTags using ALL matching (location must have ALL specified tags)
- Finds dependent location created by specific scene through scene-specific tag
- Returns object reference to matched location
- Situation receives bound location through categorical resolution without entity IDs

**Why Tag-Based Not ID-Based**:
- Respects HIGHLANDER (no entity instance IDs on domain objects)
- Enables categorical resolution through EntityResolver standard pipeline
- Scene-specific binding through transformation maintains architectural consistency
- Object references flow through system without string ID lookups

**Critical EntityResolver Responsibility**:
EntityResolver.LocationMatchesFilter MUST check LocationTags against DomainTags. Missing this check breaks dependent location resolution entirely. Situations fail to find scene-created locations, causing resolution failures or incorrect generic location matching.

**DomainTags vs LocationTags Distinction**:
- Location.DomainTags: Tags assigned TO location (what tags does this location have)
- PlacementFilter.LocationTags: Tags REQUIRED by filter (what tags must location have)
- Matching uses ALL semantics: Location must possess ALL tags specified in filter
- Analogy: DomainTags are labels ON object, LocationTags are requirements FOR object

**Rationale**: Tag-based binding maintains architectural purity (no IDs), enables scene-specific resolution through transformation, respects categorical resolution pattern throughout system, preserves object reference flow without string lookups.

---

### 8.2.6 Location Accessibility (Query-Based Pattern)

**Core Principle**: Location accessibility determined through runtime queries, not stored state. Situations grant access to their OWN location through presence, not through unlocking OTHER locations.

**The Architectural Model**: Active situations make locations accessible by being placed AT those locations. Situation at Location X with GrantsLocationAccess true means Location X is accessible while that situation is active. This is query-based accessibility, not state mutation.

**GrantsLocationAccess Pattern**:

**Situation Placement Grants Access**:
- SituationTemplate has GrantsLocationAccess property defaulting to true
- Situation placed AT specific location through PlacementFilter resolution
- LocationAccessibilityService queries active scenes and checks current situation
- If current situation at target location has GrantsLocationAccess true, location is accessible
- Accessibility determined by situation presence, not stored unlock state

**Tutorial Example Flow**:
- Scene creates dependent private room location through DependentLocationSpec
- Situation 1 placed at Common Room (Elena location) through base scene filter
- Situation 2 placed AT private room through LocationTags DEPENDENT_LOCATION:private_room marker
- When Situation 2 becomes current, private room becomes accessible (Situation 2 is AT private room)
- Situation 2 grants access to its OWN location, not unlocking from elsewhere

**Why Situations Unlock Their Own Location**:
- Architectural simplicity: Situation presence at location determines accessibility
- Query-based pattern: No state mutation, accessibility derived from active situations
- Object reference integrity: Situation has direct Location property, checks situation location equals target location
- Avoids state synchronization: No unlock lists to maintain, no state clearing needed

**InitialState Property (Vestigial)**:
Location has InitialState property set to Locked for dependent locations but this property is NEVER CHECKED by any system. Property exists in domain model but has no behavioral impact. Actual accessibility determined entirely by GrantsLocationAccess query pattern. InitialState is vestigial remnant from deleted state-based unlocking architecture.

**Why Query-Based Not State-Based**:
- **No state mutation**: Accessibility derived from queries, never stored
- **Situation-driven**: Active situations control accessibility through placement
- **Temporal coupling**: Location accessible only while situation active
- **HIGHLANDER compliance**: No duplicate accessibility state, single query determines truth
- **Fail-fast validation**: LocationPlayabilityValidator catches inaccessible content at parse time

**Critical LocationAccessibilityService Query**:
Service queries all active scenes, finds current situations, checks if current situation located at target location AND has GrantsLocationAccess true. Returns boolean accessibility without consulting any stored state on Location entity. Query executes on every accessibility check, ensuring real-time accuracy based on active situations.

**Rationale**: Query-based accessibility eliminates state synchronization complexity, respects HIGHLANDER pattern (single query source of truth), enables temporal accessibility (situation presence controls access), maintains architectural consistency with categorical resolution throughout system.

#### Bootstrap Gradient

**Early Game (Act 1)**: 95% authored, 5% generated
- Core locations authored (villages, inns, major landmarks)
- Only scene-specific resources generated (private rooms, hideouts)
- Stability priority (authored content tested and validated)

**Mid Game (Act 2-3)**: 60% authored, 40% generated
- Major locations authored, minor locations generated
- Generated venues appear for side quests
- Variety increases while maintaining coherence

**Late Game (Act 4+)**: 20% authored, 80% generated
- Only critical story locations authored
- Procedural expansion dominates
- Infinite world growth enabled

**Rationale**: Authored content establishes baseline quality. Generated content provides infinite variety. Gradient manages transition from stability → variety.

#### Hexagonal Architecture Compliance

**Domain Independence**: Location generation services in `src/Services/` (domain), not `src/Content/` (parsing) or `src/Pages/` (UI)

**Catalogue Pattern**: Generated content flows through same pipeline as authored content (JSON → DTO → Parser → Entity)

**HIGHLANDER**: Location.HexPosition = source of truth, Hex.LocationId = derived lookup (single source, synchronized)

**Fail-Fast**: Validation throws on unplayable content (no silent degradation)

---

### 8.2.10 Venue-Scoped Categorical Resolution

**Core Principle**: Entity resolution during scene activation searches ONLY within the venue where the scene spawns, preventing cross-venue teleportation while maintaining categorical flexibility.

#### The Spatial Consistency Problem

Scenes activate when player enters a location (Deferred → Active transition). During activation, EntityResolver resolves categorical filters to concrete entities (NPCs, Locations) that situations will reference. Without spatial scoping, resolution could match entities from ANY venue in GameWorld, causing narrative inconsistency.

**Example Problem**: Scene spawns at "The Brass Bell Inn" (Tavern venue). Situation filter requires Innkeeper profession NPC. Without venue scoping, EntityResolver might match "Marcus" from "The Silver Hart" inn across town, teleporting Marcus to player's current venue for the scene.

**Fiction Break**: Player is at Brass Bell Inn, talks to Elena (local innkeeper), but scene references Marcus from different inn. Marcus cannot be in two venues simultaneously. Spatial consistency violated.

#### Venue-Scoped Resolution Pattern

**Dual-Mode Search**:
- **Global Mode** (CurrentVenue = null): Searches all entities in GameWorld.Locations/GameWorld.NPCs (used during static content loading)
- **Venue-Scoped Mode** (CurrentVenue provided): Searches ONLY entities where entity.Venue == CurrentVenue (used during scene activation)

**Implementation**:
EntityResolver receives optional CurrentVenue parameter passed through SceneSpawnContext. When CurrentVenue is null, resolution searches globally across all venues. When CurrentVenue is provided, resolution filters to entities located within that specific venue only.

**Activation Flow**:
```
Player enters Location "Common Room"
    ↓
LocationFacade.CheckAndActivateDeferredScenes(targetLocation)
    ↓
Create SceneSpawnContext:
  - CurrentVenue = targetLocation.Venue ("The Brass Bell Inn")
    ↓
SceneInstantiator.ResolveSceneEntityReferences(scene, context)
    ↓
For each Situation with NpcFilter:
  EntityResolver.FindOrCreateNPC(filter, context.CurrentVenue)
    ↓
Query: GameWorld.NPCs
  .Where(npc => npc.Venue == CurrentVenue)
  .Where(npc => NpcMatchesFilter(npc, filter))
    ↓
Returns: Elena (from Brass Bell Inn venue ONLY)
    ↓
Situation.Npc = Elena (object reference, venue-consistent)
```

**Context Structures**:
SceneSpawnContext class contains CurrentVenue property holding Venue object reference passed to all resolution methods. When CheckAndActivateDeferredScenes triggers, context populated with targetLocation.Venue before entity resolution begins. EntityResolver methods receive context parameter and extract CurrentVenue for spatial filtering.

**Resolution Logging**:
Console logs distinguish resolution modes for debugging. Global mode logs "Found X matching NPCs (global search, no venue constraint)". Venue-scoped mode logs "Found X matching NPCs within venue 'The Brass Bell Inn'". Logging visibility critical for verifying spatial consistency during scene activation.

#### Why Venue Scoping Matters

**Spatial Consistency**: Scenes reference NPCs and locations physically present in same venue, maintaining fictional coherence. No NPC teleportation across venues.

**Narrative Coherence**: Player at Brass Bell Inn interacts with Brass Bell Inn NPCs only. Situation dialogue references local context accurately.

**Procedural Content**: Same scene template works in ANY venue by matching entities procedurally. "inn_lodging" template activates at ANY tavern venue, resolving to that venue's specific innkeeper and rooms.

**Fail-Fast on Missing Entities**: If scene requires Innkeeper but venue has none, resolution returns no matches. Scene activation fails explicitly rather than silently teleporting NPC from elsewhere. Forces content authoring to ensure venues have required entity types.

#### Global vs Venue-Scoped Usage

**Global Resolution (CurrentVenue = null)**:
- Parse-time static content loading (NPCs placed in authored JSON locations)
- Initial world setup before gameplay begins
- Content validation and entity initialization
- Used by PackageLoader during game startup

**Venue-Scoped Resolution (CurrentVenue provided)**:
- Runtime scene activation (Deferred → Active transition)
- Dynamic dependent resource spawning within scenes
- Situation entity binding during activation
- Used by LocationFacade.CheckAndActivateDeferredScenes

**Critical Distinction**: Static content uses global resolution because entities authored with explicit venue assignments. Dynamic content uses venue-scoped resolution because scene activation location determines spatial context.

#### Architecture Integration

**HIGHLANDER Compliance**: One resolution pattern (FindOrCreate), two modes (global/venue-scoped), single CurrentVenue parameter controls behavior. No duplicate resolution logic.

**Catalogue Pattern Integration**: Categorical filters (profession, demeanor, properties) work identically in both modes. Venue scoping adds spatial filter layer on top of categorical matching.

**Two-Phase Spawning**: Phase 1 (Deferred creation) has NO venue context yet (scene not placed). Phase 2 (Activation) has venue context from player location, enabling venue-scoped resolution.

**Fail-Fast Validation**: Missing entity resolution fails explicitly during activation. Stack trace points to exact filter that couldn't resolve. No silent fallbacks or default entities.

#### Example: Inn Lodging Tutorial Scene

**Scenario**: Player starts at "Common Room" in "The Brass Bell Inn" venue. Scene "a1_secure_lodging" spawns as Deferred at game start.

**Phase 1 - Deferred Spawning** (Startup):
```
SceneInstantiator.CreateDeferredScene("a1_secure_lodging")
  - Creates Scene entity (State = Deferred)
  - Creates 3 Situations with null NPC/Location references
  - NO entity resolution yet (no venue context)
  - Scene stored in GameWorld.Scenes awaiting activation
```

**Phase 2 - Activation** (Player Movement):
```
Player clicks "Look Around" at Common Room
LocationFacade.MoveToSpot(commonRoom)
  ↓
CheckAndActivateDeferredScenes(commonRoom)
  ↓
Create context with CurrentVenue = commonRoom.Venue
  ("The Brass Bell Inn")
  ↓
SceneInstantiator.ResolveSceneEntityReferences(scene, context)
  ↓
Situation 1 NpcFilter: { Profession: Innkeeper }
  ↓
EntityResolver.FindOrCreateNPC(filter, "The Brass Bell Inn")
  ↓
Query: NPCs WHERE Venue == "The Brass Bell Inn"
              AND Profession == Innkeeper
  ↓
Found: Elena (Brass Bell Inn innkeeper)
  ↓
Situation.Npc = Elena (venue-consistent binding)
```

**Result**: Situation references Elena specifically because she's the innkeeper AT the venue where player currently is. If player were at different tavern, resolution would bind that venue's innkeeper instead.

#### Code References

**Implementation Locations**:
- EntityResolver.FindOrCreateLocation() - src/Content/EntityResolver.cs:101-127 (venue-scoped location search)
- EntityResolver.FindOrCreateNPC() - src/Content/EntityResolver.cs:146-172 (venue-scoped NPC search)
- SceneSpawnContext class - src/Content/DTOs/SceneSpawnContext.cs (CurrentVenue property)
- LocationFacade.CheckAndActivateDeferredScenes() - src/Subsystems/Location/LocationFacade.cs:130-190 (activation trigger with venue context)

**Related Patterns**:
- See 8.2.8 (5-System Scene Spawning Architecture) for complete entity resolution flow
- See 8.2.9 (Dynamic World Building) for categorical matching principles
- See 8.4.3 (Scene Lifecycle States) for Deferred → Active transition details

---

## 8.3 Design Principles

### 8.3.1 Principle Priority Hierarchy

When principles conflict, resolve via three-tier priority:

#### TIER 1: Non-Negotiable (Never Compromise)

1. **No Soft-Locks**: Always forward progress. If design creates unwinnable state, redesign completely.
2. **Single Source of Truth**: One owner per entity type. Redundant storage creates desync bugs.

**Rule**: If violating TIER 1, **STOP** - redesign completely.

#### TIER 2: Core Experience (Compromise Only with Clear Justification)

3. **Playability Over Compilation**: Game must be testable and playable. Unplayable code is worthless.
4. **Perfect Information at Strategic Layer**: Player can calculate strategic decisions.
5. **Resource Scarcity Creates Choices**: Shared resources force trade-offs.

**Rule**: TIER 2 beats TIER 3. Creative solutions preferred within same tier.

#### TIER 3: Architectural Quality (Prefer but Negotiable)

6. **HIGHLANDER - One Path**: One instantiation path per entity type.
7. **Elegance Through Minimal Interconnection**: Systems connect at explicit boundaries.
8. **Verisimilitude**: Relationships match conceptual model.

**Rule**: Within same tier, find creative solutions satisfying both.

### 8.3.2 Core Design Principles

#### Principle 1: Single Source of Truth + Explicit Ownership

**Statement**: Every piece of game state has exactly ONE canonical storage location.

**Application**:
- GameWorld owns all entities via flat lists
- Parent references children by ID (never inline at runtime)
- ONE owner per entity type
- Test: Can you name owner in one word?

**Correct Ownership**: GameWorld owns Scenes collection, Scene owns Situations collection. Situation entities stored inline within Scene.Situations list. GameWorld does NOT have separate Situations collection avoiding desync between parallel collections.

**Wrong Ownership**: GameWorld having both Scenes collection AND separate Situations collection creates desync risk. Situation could be modified in one collection without updating the other. Single owner (Scene.Situations) enforces truth.

#### Principle 2: Strong Typing as Design Enforcement

**Statement**: Strong typing and explicit relationships aren't constraints - they're filters that catch bad design before it propagates.

**Type Restrictions**:
- **ONLY**: `List<T>` where T is entity/enum, strongly-typed objects, `int` for numbers
- **FORBIDDEN**: Dictionary, HashSet, var, object, func, lambda (except LINQ), float/double/decimal, tuples

**Rationale**: Type restrictions enforce clear entity relationships, prevent ambiguity, force proper domain modeling.

**Forbidden Types**: Dictionary with string keys mapping to integer costs requires runtime string parsing, fails silently on typos, lacks compile-time validation. Float/double for game values introduces rounding errors and precision ambiguity in discrete game mechanics.

**Correct Types**: Explicit CoinCost property with int type provides compile-time checking, autocomplete support, clear semantic meaning. StatValue property as int represents discrete game mechanics without floating-point imprecision.

#### Principle 3: Ownership vs Placement vs Reference

**Statement**: Distinguish between ownership (lifecycle), placement (context), and reference (lookup).

**Definitions**:
- **Ownership**: Parent creates/destroys child. If A destroyed, B destroyed.
- **Placement**: Entity appears at location (lifecycle independent).
- **Reference**: Entity A stores Entity B's ID (no lifecycle dependency).

**Ownership Example**: Scene owns Situations collection. When Scene deleted from GameWorld, all Situation entities in Scene.Situations collection are destroyed. Lifecycle coupled: parent destruction cascades to children.

**Placement Example**: Scene has PlacementType Location and PlacementId "market_square". Scene lifecycle independent from Location lifecycle. Deleting market_square location doesn't destroy Scene, just invalidates placement requiring resolution. Location can be deleted without Scene deletion.

**Reference Example**: Choice has DestinationLocationId referencing "inn_room" location. Choice doesn't own inn_room location, just references it for navigation. Choice deletion doesn't affect location. Location deletion doesn't affect choice (though choice becomes invalid requiring validation).

#### Principle 4: Inter-Systemic Rules Over Boolean Gates

**Statement**: Systems connect via typed rewards applied at completion, not continuous boolean flag evaluation.

**Architectural Implication**: Use typed reward objects (SceneReward, ChoiceReward) with explicit properties instead of boolean flags. This enforces resource arithmetic and prevents hidden unlocks.

**Boolean Gate Antipattern**: Checking player CompletedTutorial boolean flag, then calling EnableAdvancedFeature method granting free unlock without resource cost. Player progresses via checklist completion, no trade-offs required.

**Resource Arithmetic Pattern**: Checking player Understanding numeric stat against threshold 5. Feature accessible based on earned resource requiring investment trade-offs. Player decides whether to spend Understanding building this capability versus alternative uses.

**For game design rationale and resource economy philosophy**, see [design/05_resource_economy.md](design/05_resource_economy.md).

#### Principle 5: Typed Rewards as System Boundaries

**Statement**: Systems connect via typed rewards applied at completion. One-time effect, not continuous state query.

**Typed Reward Structure**: SceneReward class contains strongly-typed properties: LocationsToUnlock list of location IDs, CoinsToGrant integer, UnderstandingToGrant integer. Each property represents explicit effect applied once at scene completion.

**One-Time Application**: When scene completes, reward executor iterates LocationsToUnlock list, setting location.IsLocked false for each ID. Adds CoinsToGrant to player.Coins. Adds UnderstandingToGrant to player.Understanding. Effects applied once, not recalculated continuously.

#### Principle 6: Resource Scarcity Creates Impossible Choices

**Statement**: Shared resources (Time, Focus, Stamina, Health) force player to accept one cost to avoid another.

**Architectural Implication**: Model resources as numeric properties on Player entity, use arithmetic comparison throughout codebase. All choice costs and rewards expressed as integer deltas.

**Resource Types**:
- **Shared**: Time, Focus, Stamina, Health, Coins (compete across systems)
- **System-Specific**: Momentum/Progress/Breakthrough (tactical only)

**Test**: Can player pursue all options without trade-offs? If yes, add scarcity.

**For resource economy design philosophy and impossible choices**, see [design/05_resource_economy.md](design/05_resource_economy.md).

#### Principle 7: One Purpose Per Entity

**Statement**: Each entity type serves exactly one purpose.

**Test**: Describe purpose in one sentence without "and"/"or".

**Correct Purpose**: Scene purpose: "Contains sequential situations progressing toward narrative goal." Situation purpose: "Presents 2-4 choices with visible costs/rewards." Each entity single clear responsibility.

**Wrong Purpose**: Scene described as "Contains situations AND tracks player reputation AND manages inventory." Multiple purposes indicate wrong entity design. Reputation and inventory belong to separate systems, not Scene responsibility.

#### Principle 8: Verisimilitude in Entity Relationships

**Statement**: Relationships match conceptual model. If explanation feels backwards, design is wrong.

**Natural Relationship**: Scenes appear at Locations. Conceptually, narrative scenes are placed contextually in physical locations. Relationship feels natural: Scene has Location placement.

**Backwards Relationship**: Locations own Scenes collection. Conceptually wrong: Location doesn't create or destroy Scenes based on location lifecycle. Scene lifecycle independent from Location. Relationship feels backwards, indicates wrong design.

#### Principle 9: Elegance Through Minimal Interconnection

**Statement**: Systems connect at explicit boundaries. One arrow per connection.

**Test**: Can you draw system diagram with one arrow per connection? If spaghetti, refactor.

**Explicit Boundary**: Scene completion triggers SceneReward application. LocationFacade.UnlockLocation method receives location ID from reward. One clear boundary: Scene → Reward → LocationFacade. Single arrow connection.

**Tangled Dependencies**: Scene checks LocationFacade state, LocationFacade updates Scene properties, Scene notifies UI observers, UI calls LocationFacade methods, LocationFacade queries Scene state. Circular dependencies, multiple arrows, spaghetti architecture requiring refactoring.

#### Principle 10: Perfect Information with Hidden Complexity

**Statement**: Strategic layer visible (costs, rewards, requirements). Tactical layer hidden (card draw, challenge flow).

**Architectural Implication**: All ChoiceTemplate properties (costs, requirements, rewards) must be concrete numeric values displayable in UI. Tactical layer sessions separate entity hierarchy, not exposed until entry.

**Test**: Can player decide WHETHER to attempt before entering? If no, violates principle.

**Strategic Layer Visibility**: Choice "Negotiate diplomatically" displays entry cost (Stamina -2), success outcome (Unlock private room), failure outcome (Pay extra 5 coins). All information visible before commitment. Player calculates affordability and risk before entering.

**Tactical Layer Hidden**: Challenge session card draw order unknown until play begins. Exact challenge flow hidden until player commits to attempt. Maintains tension while preserving strategic decision-making at entry point.

**For player experience design rationale**, see [design/01_design_vision.md](design/01_design_vision.md).

#### Principle 11: Execution Context Entity Design

**Statement**: Design properties around WHERE they're checked (execution context), not implementation details.

**Process**:
1. Identify facade method checking property
2. Design property for that context
3. Decompose categorical properties to multiple semantic properties
4. Translate at parse-time (catalogues), use concrete at runtime

**Categorical to Concrete Translation**: JSON contains categorical npcDemeanor "Friendly" and quality "Premium". Parse-time catalogue translates to concrete StatThreshold: base value 5 multiplied by DemeanorMultiplier 0.6 (Friendly) multiplied by QualityMultiplier 1.6 (Premium) equals final threshold 4.8 rounded to 5. Runtime ChoiceFacade.EvaluateRequirements checks player.Rapport >= choice.StatThreshold using concrete integer comparison, no catalogue calls at runtime.

#### Principle 12: Categorical Properties → Dynamic Scaling

**Statement**: AI generates categorical properties (AI doesn't know global state), catalogues translate with dynamic scaling.

**Why**: Enables infinite AI-generated content without balance knowledge.

**AI Generation Example**: AI generates NPC description "Friendly innkeeper at premium inn" without knowing global stat thresholds or economy balance. Catalogue receives categorical properties: demeanor Friendly, quality Premium. Translates to concrete values: StatThreshold 5 × 0.6 (Friendly multiplier) = 3 (easy interaction), CoinCost 8 × 1.6 (Premium multiplier) = 13 (expensive lodging). Result contextually appropriate without AI needing numeric knowledge.

#### Principle 13: Playability Over Compilation

**Statement**: Code that compiles but cannot be played/tested is unacceptable.

**Test**: Can QA tester reach this from game start? If no, incomplete.

**Application**:
- Every scene reachable via gameplay path
- Every challenge deck mathematically solvable
- All dependencies validated at parse-time (fail-fast)

---

## 8.4 Domain Concepts

> **Note**: For game design patterns, content archetypes, and player-facing design concepts, see [design/09_design_patterns.md](design/09_design_patterns.md). This section covers technical domain concepts only.

### 8.4.1 Two-Layer Architecture

**Separation**: Strategic layer (perfect information) vs Tactical layer (hidden complexity)

**Strategic Layer**:
- Flow: Obligation → Scene → Situation → Choice
- Perfect information (all costs/rewards visible)
- State machine progression
- Persistent entities

**Tactical Layer**:
- Flow: Challenge Session → Card Play → Resource Accumulation
- Hidden complexity (card draw order unknown)
- Victory thresholds
- Temporary sessions

**Bridge**: ChoiceTemplate.ActionType (Instant, Navigate, StartChallenge)

**See**: ADR-003 in section 9, section 4.1 in solution strategy

### 8.4.2 Four-Choice Archetype (Guaranteed Progression)

**Pattern**: Every A-story situation has 4 choice types ensuring no soft-locks.

**Choice Types**:
1. **Stat-Gated (Optimal)**: Requires stat threshold, best rewards if qualified
2. **Money-Gated (Reliable)**: Costs coins, good rewards if affordable
3. **Challenge (Risky)**: No requirements, variable outcome (success OR failure both progress)
4. **Guaranteed (Patient)**: Zero requirements, minimal rewards, ALWAYS progresses

**Purpose**: Player chooses HOW to progress (optimal/reliable/risky/patient), not IF.

**See**: ADR-001 in section 9 (Infinite A-Story), QS-001 in section 10

### 8.4.3 Scene Lifecycle States (Two-Phase Spawning with Activation Filters)

**States**: Deferred → Active → Completed/Expired

- **Deferred**: Scene and Situations created, dependent resources NOT spawned yet, entity references null (lightweight initialization)
- **Active**: Activation triggered, dependent resources spawned, entity references resolved, scene fully playable
- **Completed**: All situations finished, rewards applied
- **Expired**: ExpirationDays reached, scene removed

**Two-Phase Pattern**:
- **Phase 1 (Spawn Deferred)**: SceneInstantiator.CreateDeferredScene() generates Scene + Situations with NO dependent resources. Scene contains LocationActivationFilter/NpcActivationFilter (categorical trigger conditions). PackageLoader creates entities with State=Deferred.
- **Phase 2 (Activate)**: LocationFacade.CheckAndActivateDeferredScenes() matches player's current location against Scene.LocationActivationFilter using categorical properties (Privacy, Safety, Activity, Purpose). When match found, SceneInstantiator.ActivateScene() generates dependent resources, resolves entity references, transitions State to Active.

**Key Architectural Pattern - Activation vs Placement**:
- **Scene.LocationActivationFilter/NpcActivationFilter**: Determines WHEN scene activates (Deferred → Active transition). Evaluated BEFORE entity resolution using categorical matching. Checked repeatedly until activation occurs.
- **Situation.LocationFilter/NpcFilter/RouteFilter**: Determines WHERE/WHO situation happens. Resolved ONCE during activation using EntityResolver. Each situation MUST have explicit filters (NO inheritance, NO fallback).

**NO CSS-Style Inheritance**: Each Situation MUST specify explicit LocationFilter/NpcFilter in its template. No fallback to scene-level base filters. No inheritance pattern. Explicit filters required per situation.

**Categorical Enum Matching**: Activation filters use intentionally named enum properties (Privacy, Safety, Activity, Purpose), NOT generic tags or capabilities. Strong typing, compile-time verification, semantic clarity.

**Transitions**: CreateDeferred → WaitForTrigger → Activate (player context matches filter) → Progress → Complete/Expire

**See**: Section 6.2 for detailed flow

### 8.4.4 Marker Resolution (Self-Contained Resources)

**Problem**: Templates can't reference what doesn't exist yet (e.g., scene-generated locations).

**Solution**: Logical markers resolved at spawn.

**Pattern**:
```
Template: "generated:private_room" (marker)
  ↓ Spawn-time
Resolution: "location_guid_12345" (actual GUID)
  ↓ Runtime
Usage: Player navigates to location_guid_12345
```

**Benefits**: Reusable templates, instance isolation, no resource sharing between spawns.

**See**: Section 6.6 for complete lifecycle

---

## 8.5 Development Practices

### 8.5.1 Lambda Restrictions

**ALLOWED**:
- LINQ queries: `.Where()`, `.Select()`, `.FirstOrDefault()`
- Blazor event handlers: `@onclick="() => HandleClick()"`
- Framework configuration: `services.AddHttpClient<T>(client => { /* ... */ })`

**FORBIDDEN**:
- Backend event handlers: Use named methods
- DI registration: Explicit initialization, not lambdas
- Backend logic: Use named methods for debugging/testing

**Forbidden Lambda Example**: Services registration using AddSingleton with lambda function receiving underscore parameter and returning GameWorldInitializer.CreateGameWorld call. Lambda obscures initialization, hinders debugging, prevents testing initialization logic independently.

**Correct Named Initialization**: Call GameWorldInitializer.CreateGameWorld, store result in gameWorld variable. Call builder.Services.AddSingleton with gameWorld instance directly. Initialization explicit, debuggable, testable.

### 8.5.2 Antipatterns (Strictly Forbidden)

#### ID Antipattern

**FORBIDDEN**:
- ❌ Encoding data in IDs: Creating composite ID by string interpolation combining action type prefix with destination ID
- ❌ Parsing IDs: Checking if ID starts with specific prefix, splitting ID on underscore delimiter
- ❌ ID-based routing: Switch statement dispatching on action ID string literal matching

**CORRECT**:
- ✅ ActionType enum for routing: Switch statement on action.ActionType enum value
- ✅ Strongly-typed properties: action.DestinationLocationId explicit property
- ✅ IDs for uniqueness only (dictionary keys, debugging)

#### Generic Property Modification Antipattern

**FORBIDDEN Pattern**: PropertyChange class with PropertyName string ("IsLocked") and NewValue string ("true"). Conditional checks PropertyName equals "IsLocked", then parses NewValue to boolean and assigns to location.IsLocked. Runtime string matching, no compile-time validation, fails silently on typos.

**CORRECT Pattern**: SceneReward class with explicit strongly-typed properties: LocationsToUnlock list of strings, LocationsToLock list of strings. Reward executor iterates LocationsToUnlock list, setting location.IsLocked false directly via property access. Compile-time type checking, autocomplete support, clear semantics.

### 8.5.3 Code Quality Standards

**NO Exception Handling** (unless explicitly requested): Try-catch blocks wrapping logic, logging exceptions, returning null on error forbidden unless explicitly requested. Correct pattern: call GetSceneById, let exceptions bubble to caller revealing stack trace and failure location.

**NO Logging** (unless explicitly requested):
- No Log.Info/Debug/Error unless debugging specific issues
- Don't pollute code with logging

**Avoid Comments**:
- Self-documenting code preferred
- Exception: Complex algorithms, non-obvious business rules (rare)

**No Defaults Unless Strictly Necessary**: Returning scene null-coalescing operator defaulting to new empty Scene masks missing data problem. Correct pattern: check scene equals null, throw InvalidOperationException with scene ID in message, return scene only if non-null. Forces fixing root cause rather than masking.

### 8.5.4 Semantic Honesty

**Requirement**: Method names MUST match return types exactly. Parameter types match parameter names. Property names describe actual data.

**Semantic Mismatch**: Method named GetVenueById with Location return type creates confusion. Method name promises Venue, returns Location. Caller expects Venue entity, receives Location entity, requires mental translation.

**Semantic Honesty**: Method named GetLocationById with Location return type matches name to return type exactly. Clear expectation, no mental translation required, compile-time verification.

### 8.5.5 Formatting Standards

**FORBIDDEN**:
- ❌ Regions (#region/#endregion)
- ❌ Inline styles in HTML/Blazor
- ❌ Hungarian notation (strTitle, iCount, lstScenes)
- ❌ Labeling docs as "Revised"/"Refined"/"Updated"

**REQUIRED**:
- PascalCase for entities, properties, methods
- CSS in CSS files (separation of concerns)
- Explicit type names (no var)

### 8.5.6 Async Propagation Pattern

**Core Principle**: Async methods propagate upward through the call stack. If a method calls async code, it MUST be async. Never block on async code with `.Wait()` or `.Result`.

**Async Propagation**: ProcessSceneAsync method declared as async Task, awaits repository.SaveSceneAsync call, awaits notificationService.NotifyAsync call. Async propagates to caller who must also await ProcessSceneAsync. No blocking calls.

**Forbidden Blocking**: ProcessScene synchronous void method calls repository.SaveSceneAsync().Wait() blocking thread waiting for completion. Calls notificationService.NotifyAsync().Result blocking for result value. Creates deadlock risk in ASP.NET context, defeats async benefits.

**UI Async Propagation**: Blazor component HandleClickAsync method declared async Task. Awaits sceneService.ProcessSceneAsync call. Calls StateHasChanged to trigger rerender. Event handler async allowing UI thread to handle other work during async operation.

**Forbidden UI Blocking**: Blazor component HandleClick synchronous void method calls sceneService.ProcessSceneAsync().Wait() blocking UI thread. Creates deadlock potential, freezes UI during operation, violates async pattern.

**Rationale:**
- **Avoid Deadlocks**: `.Wait()` and `.Result` can deadlock in ASP.NET context
- **Better Performance**: Async allows thread pool to handle other work
- **Consistent Pattern**: All I/O operations are async throughout stack
- **Framework Expectation**: ASP.NET Core and Blazor designed for async

**Enforcement:**
- All database operations async
- All HTTP calls async
- All file I/O async
- UI event handlers async when calling async services
- Never use `.Wait()`, `.Result`, or `.GetAwaiter().GetResult()`

**Exception**: Framework configuration code that runs once at startup may use `.GetAwaiter().GetResult()` if truly required (rare).

---

## 8.6 Pattern Relationships

### 8.6.1 Reinforcing Patterns

**HIGHLANDER + Single Source of Truth**:
- HIGHLANDER enforces one representation
- Single Source of Truth enforces one storage location
- Together: One concept, one representation, one location

**Catalogue + Strong Typing**:
- Catalogue translates categorical → concrete
- Strong Typing requires explicit properties
- Together: Parse-time translation, runtime type safety

**Three-Tier Timing + Let It Crash**:
- Three-Tier requires clear timing boundaries
- Let It Crash validates references at parse-time
- Together: Fail-fast at load, safe at runtime

**Requirement Inversion + Perfect Information**:
- Requirement Inversion: Content exists, requirements filter
- Perfect Information: Show exact requirements
- Together: Player calculates affordability before commitment

### 8.6.2 Conflict Resolution

When patterns conflict, apply principle priority (ADR-006):

**TIER 1 beats all**: If No Soft-Locks or Single Source of Truth violated, redesign.

**TIER 2 beats TIER 3**: Playability, Perfect Information, Resource Scarcity override elegance concerns.

**Within same tier**: Creative solutions satisfying both principles.

**Example**:
- **Conflict**: HIGHLANDER (fail-fast) vs Playability (graceful degradation)
- **Resolution**: TIER 3 vs TIER 2 → Playability wins. Use pattern: authoritative ID + ephemeral cache.

### 8.6.3 Pattern Application Matrix

| Pattern | Parse Time | Spawn Time | Query Time | Runtime |
|---------|------------|------------|------------|---------|
| HIGHLANDER | ID from JSON | Object resolved | - | Object used |
| Catalogue | Translate categorical | - | - | Use concrete |
| Three-Tier Timing | Templates created | Scenes spawned | Actions created | Actions executed |
| Let It Crash | Validate references | Validate context | - | Trust initialization |
| Sentinel Values | Create sentinels | - | - | Check sentinels |
| Requirement Inversion | Content spawned | Conditions evaluated | Visibility filtered | Player selects |

---

## 8.7 Summary

The Wayfarer architecture rests on these crosscutting foundations:

**Architectural Patterns**: HIGHLANDER, Catalogue, Three-Tier Timing, Let It Crash, Sentinel Values, Requirement Inversion

**Design Priorities**: TIER 1 (No Soft-Locks, Single Source of Truth) → TIER 2 (Playability, Perfect Information, Resource Scarcity) → TIER 3 (Elegance, Verisimilitude)

**Core Principles**: Strong typing, explicit ownership, one purpose per entity, resource competition, perfect information at strategic layer

**Development Practices**: Semantic honesty, let it crash, no defaults, no abstraction over-engineering

**Common Theme**: Elegance through clarity. One concept, one representation. Perfect information with hidden complexity. Resource scarcity creates impossible choices.

---

## Related Documentation

- **04_solution_strategy.md** - Strategic decisions implementing these patterns
- **09_architecture_decisions.md** - ADRs documenting pattern adoption
- **02_constraints.md** - Constraints derived from these principles
- **10_quality_requirements.md** - Quality scenarios validating pattern application
