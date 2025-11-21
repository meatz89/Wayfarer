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

**Exception**: Templates can have IDs (SceneTemplate.Id, SituationTemplate.Id) because templates are immutable archetypes, not mutable entity instances. Templates are content definitions, not game state.

**Pattern**:
- Domain entities have NO ID properties
- Relationships use object references ONLY
- Parser uses categorical properties to find/create entities (EntityResolver.FindOrCreate)
- NEVER store both ID and object reference (violates Single Source of Truth)

**Correct Pattern Structure**:

Domain entities store direct object references to related entities rather than ID strings. NPC entities have a Location property containing the actual Location object, not a location ID string. Scene entities reference their CurrentSituation object directly, along with direct references to their Location and NPC placement. Situation entities maintain references to their Template (templates are allowed to have IDs as they're immutable archetypes), along with direct object references to Location and the parent Scene. This creates a clean object graph where entities hold actual references to other entities, enabling navigation through the domain model without lookup operations.

**Parser Pattern (EntityResolver.FindOrCreate)**:

The parser resolves entities using categorical properties through a FindOrCreate pattern. When resolving a Location, the method queries existing locations in GameWorld using filter criteria like Purpose and Safety properties. If a matching location exists (categorical properties align), the parser returns that existing object reference. If no match is found, the parser creates a new Location instance populated with categorical properties from the filter, adds it to GameWorld's Locations collection, and returns the newly created object reference. This pattern eliminates ID dependencies - the parser works entirely with categorical matching and direct object references, never generating or storing ID strings.

**FORBIDDEN Patterns**:

Several patterns violate the HIGHLANDER principle and must be avoided. Storing both an ID string and an object reference creates redundancy - for example, RouteOption should not have both OriginLocationId and OriginLocation properties, as this duplicates the same information in two forms and can desynchronize. Storing ID-only properties without object references forces runtime lookups - SceneSpawnReward should reference the actual SceneTemplate object rather than storing just a template ID string. Maintaining lists of ID strings instead of object lists violates the pattern - Player should maintain an ActiveObligations collection of Obligation objects, not an ActiveObligationIds collection of string identifiers. The correct pattern uses object references exclusively: collections contain entity objects, properties reference entity objects, and no ID strings exist in the domain layer.

**Why This Pattern**:
- **Single Source of Truth**: Object IS the truth, no ID to get out of sync
- **Compile-time Safety**: Object references checked by compiler, IDs fail at runtime
- **Domain Clarity**: Entities reference entities, not string lookups
- **Procedural Generation**: Categorical properties enable FindOrCreate pattern

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

Catalogues accept archetype definitions and generation context containing categorical properties. The method retrieves base numeric values from the archetype template, then applies scaling multipliers based on categorical context properties. For example, if the context indicates a friendly demeanor, the base threshold might be scaled by a 0.6 multiplier (making it easier). If the context indicates superior power dynamics, the base threshold might be scaled by a 1.4 multiplier (making it harder). These multipliers compound when multiple categorical properties apply. The method returns a collection of choice templates where all properties have been resolved to concrete integer values - stat thresholds, coin costs, and reward amounts are all deterministic numbers ready for runtime evaluation. No categorical properties remain in the output - translation happens once at parse-time, and the concrete results are stored permanently.

#### FORBIDDEN Forever

- ❌ Runtime catalogue calls (parse-time ONLY)
- ❌ String matching: `if (action.Id == "secure_room")`
- ❌ Dictionary lookups: `Cost["coins"]`, `Cost.ContainsKey("coins")`
- ❌ ID-based routing: Entity IDs are reference only, never control behavior

---

### 8.2.3 Three-Tier Timing Model

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

**Structure**: Template entities serve as immutable blueprints containing an identifier property and a collection of embedded child templates. Each template stores base values and categorical properties used during instantiation. Templates are created once from JSON during game initialization and remain unchanged throughout gameplay. The template collection stores reusable patterns that can spawn multiple runtime instances without modifying the original template definitions.

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

**WRONG**: The anti-pattern checks for null data and silently creates a default object instance to allow execution to continue. This hides the underlying problem by papering over missing data with manufactured defaults that may have incorrect state, making it difficult to identify why data is absent.

**CORRECT**: The proper pattern checks for null and immediately throws an exception with a descriptive message explaining what data was expected and where it should have come from. This fails fast at the point of detection, forcing immediate investigation and correction of the root cause rather than allowing silent degradation.

#### Application Areas

**1. Entity Initialization**: Entities initialize collection properties inline with empty collections as part of their declaration. Parsers trust this initialization exists and assign parsed data directly to the property without null-coalescing checks. If the parser receives null data, it assigns null and the code crashes. Runtime code similarly trusts initialization and accesses collection properties directly - querying count or iterating without defensive null checks. If initialization somehow failed, the code crashes immediately at the point of access with a clear null reference exception.

**2. Parse-Time Validation**: When querying for entities by identifier during parsing, the code searches the collection for a matching template. If the query returns null (no matching template found), the code immediately throws an exception with a message identifying which template identifier was being sought. This fails fast with precise error information rather than allowing null to propagate through the system.

**3. Missing Data**: The forbidden pattern uses null-coalescing to return a default threshold value of 3 when the property is null, hiding the fact that required data is missing. The correct pattern explicitly checks whether the threshold is set (comparing to zero for integer properties) and throws an exception identifying which choice is missing its threshold value. This forces the content author to provide the required value rather than silently substituting defaults.

#### When NOT to Let It Crash

- **User input validation**: Graceful error messages appropriate
- **External API failures**: Retry logic appropriate
- **Expected gameplay conditions**: Player not meeting requirements (show UI feedback, don't crash)

**Example**: For expected gameplay states like insufficient resources, the code should return a validation result object rather than throwing exceptions. When checking whether the player can afford an action, if coins are below the cost, the method returns a validation result indicating the check failed with a user-friendly message. This is normal gameplay state, not an error condition - throwing an exception would be inappropriate.

---

### 8.2.5 Sentinel Values Pattern

**Core Principle**: Never use null for domain logic. Create explicit sentinel objects with internal flags.

#### Problem with Null

Using null to represent default domain states creates ambiguity. A conditions property set to null could mean "always eligible", "never eligible", "not initialized", or "unspecified". This forces defensive null checks throughout the codebase. Every location checking eligibility must first verify the property isn't null before calling methods, creating repetitive defensive code. Forgetting a null check causes runtime exceptions. The meaning of null remains ambiguous - does absence of conditions mean permissive or restrictive?

#### Sentinel Solution

Instead of null, create explicit sentinel objects with descriptive static factory methods. The entity class contains a private boolean flag indicating whether it represents the sentinel case. A static factory property returns a sentinel instance with the flag set. The eligibility evaluation method checks this internal flag first - if the sentinel is active, it returns the appropriate default behavior immediately without evaluating any conditions. Otherwise, it proceeds with actual condition logic. Entities initialize properties to the sentinel value rather than null. Usage code calls methods directly without null checks - the sentinel handles the default case internally. The sentinel value has explicit semantic meaning - "AlwaysEligible" clearly communicates intent compared to ambiguous null.

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

Traditional progression systems use boolean flags to gate content creation. When the player completes a quest, the completion flag triggers creation of the next quest. The subsequent content doesn't exist in the world until the unlock condition evaluates to true. This creates several problems: content remains completely hidden from the player until unlocked (no perfect information), progression feels like checklist completion rather than strategic choice, players cannot plan ahead because they don't know what's coming, and boolean flags create hard gates rather than resource arithmetic.

#### Requirement Inversion - CORRECT

With requirement inversion, all content entities spawn into the world immediately at game start or early in gameplay. Subsequent content already exists in the game world's collection - it's not created dynamically when conditions are met. Spawn conditions are attached to each entity, filtering visibility and selectability rather than creation. When evaluating whether to show content to the player, the system queries the existing entity's eligibility conditions against current player state. The UI displays the entity with visual indicators showing whether it's currently accessible or locked, along with the exact requirements needed to unlock it. Players see future content and can plan resource allocation strategically.

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

**Example Display Structure**:

An NPC entity appears at a location (physical presence always shown). The first NPC has two active scenes simultaneously - one focused on securing lodging with its own button, and another focused on investigating trouble at the inn with a separate button. Each button is labeled descriptively based on its associated scene. A second NPC also appears at the same location (physical presence shown), but this NPC has no currently active scenes, so no interaction buttons appear beneath their listing. Physical presence and interactive opportunities are decoupled - NPCs are always visible when present, but interaction buttons only appear when narrative threads are active.

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

Scene state machine manages CurrentSituationId and AdvanceToNextSituation() for seamless narrative flow.

#### Perfect Information Requirement

Players must see ALL available interaction options for strategic decisions. Hiding scenes because they lack aesthetic labels violates perfect information principle. Architecture prioritizes functionality over cosmetics:

- Scene exists + active situation → Show button (even with placeholder label)
- No active scene → No button (nothing to engage)

#### Multi-Scene Display Pattern

The architecture transitioned from single-scene to multi-scene querying. Previously, queries used first-match operators to retrieve only one active scene per NPC, and viewmodels contained a single string property for the interaction label. This pattern assumed NPCs had at most one interactive opportunity at a time. The updated architecture queries for all active scenes matching the NPC and state criteria, returning a collection rather than a single result. Viewmodels now contain a list of scene descriptor objects rather than a single label string. Each descriptor in the list represents one available scene with its own button, label, and routing information. This structural change enables NPCs to serve as hubs for multiple concurrent narrative threads.

#### Label Derivation Hierarchy

When deriving button labels for scenes, use fallback hierarchy:

1. Scene.DisplayName (explicit authored label)
2. First Situation.Name in scene (derived from situation content)
3. Placeholder "Talk to [NPC Name]" (functional default)

Never hide functional scene because it lacks pretty label. Playability trumps aesthetics.

#### Navigation Routing

When players click scene interaction buttons, navigation must route to the specific scene represented by that button. The previous ambiguous pattern passed only the NPC identifier to the navigation handler. The navigation code then searched for any active scene at that NPC using filtering queries, retrieving the first match. This approach breaks when multiple scenes exist - it's ambiguous which scene the player intended to access. The correct explicit pattern passes both the NPC identifier and the scene identifier as a routing pair. The navigation handler receives both identifiers and uses the scene identifier for direct lookup, retrieving exactly the scene the player selected. This explicit routing eliminates ambiguity and ensures each button reliably accesses its associated scene regardless of how many concurrent scenes exist at the NPC.

#### Spawn Independence

Scenes spawn independently from different sources:

- Tutorial scenes: Spawn at parse-time (categorical filters, same as all content)
- Obligation scenes: Spawn at runtime (categorical filters)
- Multiple obligations: Can spawn scenes at same NPC simultaneously
- Each scene: Operates independently until completion

This architectural pattern supports rich narrative branching where NPCs serve as hubs for multiple concurrent story threads.

#### Implementation Requirements

1. **Query Pattern**: Use `.Where()` not `.FirstOrDefault()` when fetching NPC scenes
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

**Eligibility Checking Structure**:

The eligibility method accepts a template containing spawn conditions and the player state object. It first evaluates required tags - if the spawn conditions specify required tags that aren't present in the player's tag collection, eligibility fails immediately. Next it evaluates temporal requirements - if the player's current day is less than the minimum day specified in spawn conditions, eligibility fails. If all conditions pass, the method returns success, signaling that the scene can proceed to the spawning process. This evaluation happens before any scene instantiation occurs, preventing creation of ineligible scenes.

#### System 2: Scene Specification (Data Structure)

**Responsibility**: Store categorical requirements for spawning, NO concrete entity IDs

**Location**: SceneSpawnReward class, ChoiceReward property

**Data Structure Properties**:

The scene spawn reward entity contains only categorical requirements for spawning. It stores a template identifier string referencing which immutable template to instantiate. It may contain optional placement filter overrides that refine or replace the template's default placement criteria. The structure explicitly does NOT contain context binding concepts, placement relation enumerations, or specific placement identifiers. All placement needs are expressed via categorical filters - either inherited from the template or provided as overrides in the reward. This ensures the reward specification remains pure categorical data without concrete entity references.

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

**Package Generation Flow**:

The scene instantiator service receives a scene spawn reward containing a categorical template identifier. It retrieves the corresponding template from the game world's template collection. The method constructs a scene DTO object with a newly generated unique identifier and stores the template identifier for reference. It copies the placement filter from the template into the DTO structure - this includes location filters specifying categorical properties like location type and tags, NPC filters specifying demeanor and profession, and route filters specifying origin and destination criteria. All of these filters contain categorical properties expressed as enumerations and string tags, not concrete entity identifiers. The service generates child situation DTOs from the template's situation definitions. The completed package contains categorical specifications ready for entity resolution in the next system, with no concrete entity IDs written to the JSON structure.

**What This Does NOT Do**:
- Does NOT resolve entities (no FindOrCreate calls)
- Does NOT write concrete entity IDs ("elena", "common_room_inn")
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

**Resolution Process Structure**:

The entity resolver provides methods for each entity type that accept placement filters and the game world. Each method follows a two-step process. First, it queries existing entities in the game world's collections using categorical matching. The query filters entities based on whether their properties match the filter criteria - for locations, this might include matching location type, safety level, and tag requirements. If an existing entity matches all filter criteria, the method returns that entity object immediately, reusing content that already exists rather than generating duplicates. Second, if no existing entity matches, the method proceeds to generation. It invokes a generator method that creates a new entity instance populated with properties derived from the categorical filter criteria. The newly generated entity is added to the game world's appropriate collection, making it available for future reuse. The method returns the generated entity object. All resolution happens eagerly at this point - by the time System 5 executes, all entities are fully resolved and available as objects, never as string identifiers.

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

**Scene Construction Structure**:

The scene parser method accepts a DTO containing scene metadata along with pre-resolved entity objects passed as parameters - these objects were already resolved by System 4. The method constructs a new scene entity, copying the unique identifier and template identifier from the DTO. It assigns the pre-resolved entity objects directly to the scene's properties - the location property receives the location object, the NPC property receives the NPC object, and the route property receives the route object. No identifier strings are stored, no enumeration-based dispatch occurs. The scene holds direct object references enabling immediate navigation through the object graph. The parser invokes child parsing methods to create the scene's embedded situations from the DTO's situation collection. The completed scene entity is returned with all references fully resolved and ready for gameplay, requiring no further lookup operations.

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

**System 1 - Decision Flow**:

When a choice is executed, its reward contains a scene spawn reward specifying the template identifier for an investigation scene. The placement filter override is null, indicating the system should use the template's default filter. The facade checks template eligibility by evaluating spawn conditions against player state - required tags, minimum day, stat thresholds. If all conditions pass, the system proceeds to specification.

**System 2 - Specification Structure**:

The reward data structure contains only the categorical template identifier. It explicitly does NOT contain concrete entity IDs, placement relation enumerations, or context binding concepts. The specification is purely categorical - which template to spawn, with optional filter overrides.

**System 3 - Package Generation Output**:

The scene instantiator generates a DTO with a unique scene identifier and the template identifier. The DTO contains categorical filters read from the template - location filters specify indoor and private properties along with industrial and mill tags. NPC and route filters are null since this scene doesn't require those placement contexts. All filters contain categorical properties (enumerations, tags, property lists) rather than concrete entity identifiers.

**System 4 - Entity Resolution Execution**:

The entity resolver invokes the location resolution method with the location filter from the DTO. The method queries existing locations in the game world, filtering for entities that match the indoor and private properties and contain industrial and mill tags. If a matching mill location exists, it's returned immediately. If no match exists, a new mill location is generated with properties derived from the filter criteria and added to the game world. The result is a location object, not an identifier string.

**System 5 - Scene Instantiation Result**:

The scene parser receives the DTO along with the pre-resolved location object from System 4. It constructs a new scene entity and assigns the location object directly to the scene's location property. The scene now holds a direct object reference to the mill location, enabling immediate property access without lookup operations. The fully resolved scene is ready for gameplay.

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

**Categorical Throughout**: Filters used from reward → package → resolution. No concrete IDs until System 4 resolution

---

### 8.2.6 Dynamic World Building (Lazy Materialization Pattern)

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
- No locking needed: Blazor Server is single-threaded (07_deployment_view.md line 26)

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

**Template Specification Structure**:

The scene archetype JSON specifies a service that includes location access as part of its rewards. The dependent locations collection defines what locations must be generated when this scene spawns. Each dependent location specification includes a template identifier indicating which location template to use for generation. The name pattern contains a token that will be replaced with context-specific values at generation time (such as the NPC's name). The venue source indicates the generated location should appear in the same venue as the base scene location. Hex placement specifies the generated location should be placed adjacent to the base location on the hex grid. The properties collection lists categorical attributes the location should have (sleeping space for rest mechanics, restful for bonuses, indoor and private for atmosphere). The locked state indicates the location begins locked and requires player actions to unlock, preventing immediate access to the generated resource.

**Generation (Scene Spawn)**:
1. SceneInstantiator reads DependentLocationSpec
2. Checks venue capacity: Can add? → Yes (LocationIds.Count < MaxLocations)
3. Generates LocationDTO with NamePattern resolved ("Elena's Private Room")
4. Finds adjacent hex to base location (venue cluster)
5. Creates Package JSON with generated LocationDTO
6. PackageLoader parses → Location entity created (indistinguishable from authored)
7. Provenance stored: `SceneProvenance { SceneId = "scene_tutorial_001" }` (metadata)

**Gameplay**:
- Player negotiates with Elena → Receives room_key item
- Player unlocks private room → Location used during gameplay
- Player rests → Resource restoration based on room properties
- **Location persists forever** → Never deleted, becomes permanent world feature

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

**Ownership Structure**:

Correct ownership creates a clear hierarchy where GameWorld owns the scene collection, and each scene owns its embedded situations collection. This creates a single path from root to leaf. The problematic pattern creates parallel collections where GameWorld maintains both a scenes collection AND a separate situations collection. Parallel collections create desynchronization risk - adding a situation to one collection but forgetting to update the other causes inconsistent state.

#### Principle 2: Strong Typing as Design Enforcement

**Statement**: Strong typing and explicit relationships aren't constraints - they're filters that catch bad design before it propagates.

**Type Restrictions**:
- **ONLY**: `List<T>` where T is entity/enum, strongly-typed objects, `int` for numbers
- **FORBIDDEN**: Dictionary, HashSet, var, object, func, lambda (except LINQ), float/double/decimal, tuples

**Rationale**: Type restrictions enforce clear entity relationships, prevent ambiguity, force proper domain modeling.

**Type System Application**:

Dictionary types with string keys create runtime error vulnerability - accessing an incorrect key throws exceptions that the compiler cannot catch. Floating point types for game values introduce precision issues and fractional values when game mechanics are inherently discrete. The correct pattern uses explicit properties with descriptive names and integer types. A coin cost property clearly indicates its purpose through its name. A stat value property uses integer type because player stats are whole numbers. The compiler enforces correct usage at build time rather than discovering errors during gameplay.

#### Principle 3: Ownership vs Placement vs Reference

**Statement**: Distinguish between ownership (lifecycle), placement (context), and reference (lookup).

**Definitions**:
- **Ownership**: Parent creates/destroys child. If A destroyed, B destroyed.
- **Placement**: Entity appears at location (lifecycle independent).
- **Reference**: Entity A stores Entity B's ID (no lifecycle dependency).

**Relationship Pattern Examples**:

Ownership relationships mean the parent controls child lifecycle. A scene entity owns its situations collection - if the scene is deleted, all its situations are deleted as well. The parent's destruction cascades to children. Placement relationships indicate where an entity appears contextually without lifecycle coupling. A scene has placement properties indicating it appears at a specific location using placement type and identifier properties. The location continues to exist independently - deleting the scene doesn't destroy the location, and deleting the location doesn't automatically destroy scenes placed there. Reference relationships indicate one entity refers to another for navigation without lifecycle implications. A choice entity references a destination location via an identifier property. The choice doesn't own or control the location - it simply needs to know which location to navigate to when selected.

#### Principle 4: Inter-Systemic Rules Over Boolean Gates

**Statement**: Systems connect via typed rewards applied at completion, not continuous boolean flag evaluation.

**Architectural Implication**: Use typed reward objects (SceneReward, ChoiceReward) with explicit properties instead of boolean flags. This enforces resource arithmetic and prevents hidden unlocks.

**System Connection Patterns**:

Boolean gate systems check flag properties to enable features. If a player has completed a tutorial flag set to true, the system enables advanced features without cost. This creates free binary unlocks based on completion state rather than resource investment. The correct pattern uses numeric resource properties for threshold comparisons. If the player's understanding stat has reached at least 5 points, features become accessible. This accessibility is based on accumulated resources rather than boolean flags. Players must invest in the resource to reach the threshold, creating meaningful cost and trade-offs rather than automatic unlocks.

**For game design rationale and resource economy philosophy**, see [design/05_resource_economy.md](design/05_resource_economy.md).

#### Principle 5: Typed Rewards as System Boundaries

**Statement**: Systems connect via typed rewards applied at completion. One-time effect, not continuous state query.

**Typed Reward Application**:

Scene completion triggers a one-time reward application using a typed reward object. The reward class contains explicit properties for each type of benefit - a collection of location identifiers to unlock, an integer amount of coins to grant, and an integer amount of understanding to grant. The reward executor iterates through the unlock collection and sets each specified location's locked state to false. It increments the player's coin resource by the grant amount. It increments the player's understanding stat by the grant amount. This happens once when the scene completes, applying permanent state changes. The reward is not continuously evaluated - it's a one-time transformation of game state at the completion boundary.

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

**Entity Purpose Clarity**:

Correctly designed entities have single describable purposes. A scene entity contains a sequential collection of situations that progress toward a narrative goal - that's its complete responsibility. A situation entity presents multiple choices to the player with visible costs and rewards - that's its complete responsibility. Each description is clean and singular. Incorrectly designed entities mix multiple responsibilities. An entity that contains situations AND tracks reputation AND manages inventory violates single purpose. Such designs indicate the entity should be split into multiple specialized entities, each with clear singular responsibility.

#### Principle 8: Verisimilitude in Entity Relationships

**Statement**: Relationships match conceptual model. If explanation feels backwards, design is wrong.

**Relationship Direction Correctness**:

Natural relationships feel intuitive when described conceptually. Scenes appearing at locations matches how we think about narrative events - stories happen in places, places don't generate stories. The scene is placed contextually at the location. Backwards relationships feel wrong when explained. Locations owning scenes implies that locations create and destroy narrative events as part of their lifecycle. This doesn't match our mental model - a location is a place, not a story generator. When relationship direction feels awkward to explain, the design is inverted and should be corrected to match conceptual understanding.

#### Principle 9: Elegance Through Minimal Interconnection

**Statement**: Systems connect at explicit boundaries. One arrow per connection.

**Test**: Can you draw system diagram with one arrow per connection? If spaghetti, refactor.

**System Boundary Clarity**:

Clean system connections flow in one direction across explicit boundaries. Scene completion triggers reward application, which invokes the location facade's unlock method. The flow is unidirectional with a clear boundary at the reward application point. Tangled connections create circular dependencies where systems reach back and forth. A scene checks the location facade, the location facade updates scene state, the scene notifies the UI, and the UI calls back into the location facade. These circular flows create unpredictable behavior and make the system difficult to reason about. Minimize connections and make each connection explicit and unidirectional.

#### Principle 10: Perfect Information with Hidden Complexity

**Statement**: Strategic layer visible (costs, rewards, requirements). Tactical layer hidden (card draw, challenge flow).

**Architectural Implication**: All ChoiceTemplate properties (costs, requirements, rewards) must be concrete numeric values displayable in UI. Tactical layer sessions separate entity hierarchy, not exposed until entry.

**Test**: Can player decide WHETHER to attempt before entering? If no, violates principle.

**Information Layer Separation**:

The strategic layer displays all decision-relevant information before commitment. A negotiation choice shows its entry cost of 2 stamina, its success reward of unlocking a private room, and its failure consequence of paying 5 extra coins. Players can evaluate this complete information to decide whether attempting the negotiation is worthwhile given their current resources and goals. The tactical layer hides execution details until entry. Once the player commits to the challenge, they encounter a session where card draw order is randomized and exact challenge progression is unknown. This creates tension during execution while maintaining perfect information at the decision point.

**For player experience design rationale**, see [design/01_design_vision.md](design/01_design_vision.md).

#### Principle 11: Execution Context Entity Design

**Statement**: Design properties around WHERE they're checked (execution context), not implementation details.

**Process**:
1. Identify facade method checking property
2. Design property for that context
3. Decompose categorical properties to multiple semantic properties
4. Translate at parse-time (catalogues), use concrete at runtime

**Entity Design Flow**:

JSON content contains categorical properties expressing semantic characteristics - NPC demeanor and quality level as enumerations. At parse time, catalogues translate these categorical inputs into concrete numeric values using multipliers. A friendly demeanor might apply a 0.6 reduction multiplier to base stat thresholds, while premium quality might apply a 1.6 increase multiplier to costs. The multipliers combine to produce a concrete stat threshold integer. At runtime, the facade method checking requirements performs simple numeric comparisons. If the player's rapport stat is greater than or equal to the choice's stat threshold property, the requirement is satisfied. The facade doesn't know about demeanor or quality - it only knows the concrete threshold that resulted from parse-time translation.

#### Principle 12: Categorical Properties → Dynamic Scaling

**Statement**: AI generates categorical properties (AI doesn't know global state), catalogues translate with dynamic scaling.

**Why**: Enables infinite AI-generated content without balance knowledge.

**AI Content Generation Flow**:

AI systems generate content descriptions using categorical properties without knowing global game balance. An AI might generate a scenario description specifying a friendly innkeeper at a premium establishment. The catalogue translation system applies multipliers to base values based on these categories. A base stat threshold of 5 is multiplied by 0.6 for friendly demeanor, producing a threshold of 3 (making the interaction easier). A base coin cost of 8 is multiplied by 1.6 for premium quality, producing a cost of 13 (making the service expensive). The result is contextually appropriate difficulty and cost without the AI needing to understand global balance numbers or player progression curves. The categorical-to-concrete translation handles balance automatically.

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

### 8.4.3 Scene Lifecycle States

**States**: Provisional → Active → Completed/Expired

- **Provisional**: Created but player hasn't finalized (preview with rollback)
- **Active**: Player committed, scenes progresses through situations
- **Completed**: All situations finished, rewards applied
- **Expired**: ExpirationDays reached, scene removed

**Transitions**: Spawn → Finalize → Progress → Complete/Expire

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

**Lambda Restriction Application**:

The forbidden pattern uses a lambda expression in dependency injection registration. The service registration accepts a factory lambda that discards its parameter and invokes an initializer method, returning the result inline. This lambda hides the initialization logic and makes debugging harder. The correct pattern performs initialization explicitly in a named variable assignment. The game world is created by invoking the initializer directly and storing the result in a local variable. The registration then passes the already-constructed instance to the service collection. This makes initialization explicit, allows breakpoint debugging on the initialization line, and keeps the service registration simple.

### 8.5.2 Antipatterns (Strictly Forbidden)

#### ID Antipattern

**FORBIDDEN**:
- ❌ Encoding data in IDs: `Id = $"move_to_{destinationId}"`
- ❌ Parsing IDs: `Id.StartsWith("move_")`, `Id.Split('_')`
- ❌ ID-based routing: `if (action.Id == "secure_room")`

**CORRECT**:
- ✅ ActionType enum for routing: `switch (action.ActionType)`
- ✅ Strongly-typed properties: `action.DestinationLocationId`
- ✅ IDs for uniqueness only (dictionary keys, debugging)

#### Generic Property Modification Antipattern

**Forbidden Generic Property Pattern**:

The anti-pattern uses string-based property routing where a change object contains property name and new value as strings. Runtime code compares the property name string against known property names and parses the string value into the appropriate type. This creates multiple problems: property names are magic strings that fail silently if misspelled, type conversion happens at runtime without compiler safety, adding new properties requires adding new string comparison branches, and the intent is obscured behind generic infrastructure.

**Correct Explicit Property Pattern**:

The proper pattern uses explicit strongly-typed properties for each type of modification. A scene reward has separate collections for locations to unlock and locations to lock. Reward execution iterates through each collection and directly modifies the appropriate boolean property on the target entity. The property access is direct without string matching, the operation is clear from reading the code, and the compiler verifies property names exist at build time. Adding new modification types means adding new explicit properties to the reward class rather than adding string comparison branches.

### 8.5.3 Code Quality Standards

**NO Exception Handling** (unless explicitly requested):

The forbidden pattern wraps operations in try-catch blocks that log errors and return null or default values. This hides problems by converting exceptions into silent nulls that propagate through the system. The error occurs far from where the exception is eventually noticed. The correct pattern lets exceptions bubble up the call stack naturally. If a scene lookup fails, the method throws immediately, creating a clear stack trace pointing to the exact operation that failed. This makes debugging straightforward - the exception indicates precisely what went wrong and where.

**NO Logging** (unless explicitly requested):
- No Log.Info/Debug/Error unless debugging specific issues
- Don't pollute code with logging

**Avoid Comments**:
- Self-documenting code preferred
- Exception: Complex algorithms, non-obvious business rules (rare)

**No Defaults Unless Strictly Necessary**:

The forbidden pattern uses null-coalescing operators to provide default values when entities are missing. If a scene lookup returns null, the method creates and returns a new empty scene. This hides data problems by providing fallback values that may have incorrect state. The correct pattern explicitly checks for null and throws a descriptive exception with context about what was expected. The error message includes the identifier that was searched for, making diagnosis immediate. The method only returns valid scenes that actually exist - never manufactured defaults.

### 8.5.4 Semantic Honesty

**Requirement**: Method names MUST match return types exactly. Parameter types match parameter names. Property names describe actual data.

**Semantic Honesty Application**:

The dishonest pattern names a method GetVenueById but returns a Location type. This mismatch between name and return type creates confusion - callers expect a venue based on the name, but receive a location. Reading code becomes difficult because names lie about what actually happens. The honest pattern names the method GetLocationById and returns a Location type. The name accurately describes what the method does - it retrieves a location entity by identifier. Readers can trust that method names describe reality, making code comprehension straightforward.

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

**Always Propagate Async:**

The correct pattern makes methods async when they call async operations. A scene processing method is declared as returning Task and uses the async keyword. It awaits repository save operations and notification service calls. The async pattern propagates up the call stack naturally. The forbidden pattern declares a synchronous void method but calls async operations, blocking on them with Wait or Result methods. This creates deadlock risk in ASP.NET contexts where the synchronization context expects async operations to complete without blocking threads.

**Propagate to UI Layer:**

The correct pattern makes Blazor component event handlers async when they call async services. The click handler is declared as returning Task with the async keyword. It awaits the scene service call, allowing the async operation to complete without blocking. After the async operation finishes, it triggers state change notification. The forbidden pattern declares a synchronous void event handler and blocks on async service calls using Wait. This violates the async propagation principle and can cause deadlocks or performance issues.

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
