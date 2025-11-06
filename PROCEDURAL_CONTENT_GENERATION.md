# Procedural Content Generation Architecture

## Problem & Motivation

### The Scaling Crisis

Wayfarer's narrative RPG model creates a content volume problem. A scene like "Secure Lodging" encompasses: the innkeeper conversation, payment or persuasion, receiving a room key, navigating to the locked upper floor, resting overnight with resource restoration, time passing to morning, waking up, leaving the room, and the room locking again. This entire multi-situation arc with location state changes, item lifecycle, resource restoration, and time advancement is ONE scene template.

The naive solution - hand-author every scene in JSON - doesn't scale. Writing explicit situations and choices for every service location, every checkpoint, every transaction creates exponential authoring burden. Tutorial lodging at Elena's tavern needs JSON defining: negotiate situation with 3 choices, access situation, rest situation, depart situation. Then bathhouse with Marcus needs separate JSON. Then healing temple with Thalia needs more JSON. Each mechanically similar but requiring separate authoring.

The insight: **entities have categorical properties that drive procedural generation, and scenes grant/require pre-authored tags for dependency-inverted progression**. Elena has Personality=Innkeeper and Demeanor=Friendly. Marcus has Personality=Attendant and Demeanor=Professional. Thalia has Personality=Priest and Demeanor=Compassionate. These properties determine how archetype generates situations and choices. Scenes grant player tags on completion (EstablishedContact with Innkeepers at Vallenmarch region) and require tags for spawning (requires EstablishedContact with Innkeepers at current region). No direct scene-to-scene ID dependencies - only state-based spawning. Tutorial JSON needs only: archetype ID + concrete entity references. Generation logic queries entity properties and produces contextually appropriate complete arc.

The traditional solution - procedural generation - fails because these complex multi-situation arcs require careful mechanical orchestration. Generated scenes produce broken progression (situations spawn out of order), unbalanced economics (rest costs too little or too much), narrative incoherence (morning departure happens before sleeping), and broken state management (keys granted but never removed).

### The AI Limitation

Large language models can generate compelling narrative for individual moments but cannot orchestrate complex multi-situation arcs with mechanical coherence. AI can write engaging dialogue for innkeeper negotiation but has no concept of "this grants an item that unlocks a location spot which enables a rest situation which restores resources using formulas and advances time which triggers morning situation which removes the key and re-locks the spot". 

Scene structure requires understanding: situation sequencing (negotiation BEFORE access BEFORE rest BEFORE departure), resource flow (key granted then consumed, energy depleted then restored, coins spent never returned), world state causality (time advances causing morning, rest causes restoration, departure causes lock), and economic balance (rest benefit must justify cost across progression curve).

### The Core Insight

Separate scene archetype patterns from entity context, with entity categorical properties driving generation. Game logic defines multi-situation arc structure: "service with location access" contains negotiation situation (challenge or payment grants access item), access situation (consume item to unlock spot), service situation (consume time/resources for benefit), and departure situation (remove item, re-lock spot).

**Entity properties control generation specifics:** NPC Personality=Innkeeper + Demeanor=Friendly generates easier challenge difficulty and conversational tone. Location Services=Lodging determines service provides rest benefit. Spot Privacy=Medium sets moderate cost premium. Same archetype, different entities, contextually appropriate mechanical variation.

**Tutorial and procedural use identical generation path:** Tutorial JSON specifies archetype + concrete entity IDs (Elena, Tavern, Upper Floor). Procedural templates specify archetype + categorical filters (any Innkeeper at any Urban location with Lodging service). Both paths invoke same generation logic receiving entity objects. Generation queries properties and produces complete arc.

**System reliability is non-negotiable:** Pure generation core with zero game dependencies. Parse-time validation catches all structural errors. Explicit contracts enforce dependencies. Comprehensive automated testing (1000+ tests) prevents regressions. Debug tools provide complete traceability. This is mission-critical infrastructure requiring bulletproof implementation.

AI applies narrative texture using entity properties: Elena generates "the innkeeper greets you warmly" because Demeanor=Friendly. Marcus generates "the attendant regards you professionally" because Demeanor=Professional. Thalia generates "the priestess offers a compassionate smile" because Demeanor=Compassionate. Same situation position, different entity properties, contextually appropriate narrative.

The architecture enables: minimal JSON authoring (just archetype + entity references), tutorial reproducibility (same entities always generate same scene), infinite procedural variation (different entities generate appropriate variations), mechanical consistency (same formulas balance all instances), narrative appropriateness (AI receives property-derived hints).

### Why This Matters

Without procedural scene generation, every service location needs hand-authored multi-situation arcs. With naive procedural generation, scenes break mechanically (wrong situation ordering, unbalanced costs, broken state). With AI-only generation, scenes lack mechanical coherence (made-up costs, arbitrary progression, broken economy). The solution must generate mechanically sound multi-situation arcs while allowing AI to provide rich narrative at each beat.

The system must be bulletproof. Scene generation is mission-critical infrastructure. Parse-time validation catches structural errors before runtime. Pure generation core with deterministic output enables exhaustive testing. Explicit contracts and audit trails provide complete traceability. Comprehensive test suite (1000+ unit tests, 100+ integration tests, 20+ end-to-end tests) catches regressions automatically. See System Robustness Architecture section for complete reliability strategy.

**LET IT CRASH philosophy:** Succeed completely or fail with full diagnostic information. No graceful degradation, no partial states, no fallbacks hiding problems. Invalid template at parse? Crash with validation report. Location creation fails during spawn? Crash and rollback. Situation references non-existent location? Crash with stack trace. Force developers to fix root causes rather than papering over issues with fallbacks that mask systemic problems.

### Three-Tier Timing Model

The architecture separates concerns across three distinct timing phases. Each tier has specific responsibilities and constraints.

**Parse Time (Tier 1)**: JSON files loaded, DTOs converted to domain entities, object references resolved. Catalogues generate complete entity structures from categorical specifications. Templates created and stored immutably in GameWorld collections. All structural validation occurs here. Invalid content prevents game launch.

**Instantiation Time (Tier 2)**: When scenes spawn, templates copied into runtime instances. Markers like "generated:private_room" resolved to actual IDs. Dependent resources created through facade pipeline. Situations linked to parent scenes. Actions remain templates until query time.

**Query Time (Tier 3)**: When UI requests data, facades query GameWorld and create view models. Actions instantiated from ChoiceTemplates on demand. Challenge outcomes determined. Conditional rewards evaluated. No entity creation at this tier - only queries and instantiation of transient UI elements.

The conditional transition system operates at Tier 2 (scene advancement logic) but requires Tier 3 data (which choice selected, challenge outcome). This separation enables pure domain logic while accommodating runtime outcomes.

### Design vs Configuration Separation

The architecture enforces strict separation between what is DESIGNED (reusable patterns defined in code) and what is CONFIGURED (where/when patterns appear, defined in JSON).

**Design Space (Catalogue Code)**:
- Scene structure: how many situations, in what order
- Situation content: which situation archetypes for each position
- Flow patterns: transition logic (Linear, Branching, Standalone)
- Resource dependencies: what locations/items scenes create
- Choice patterns: 4-choice structure (stat-gated, money, challenge, fallback)

Design decisions live in SceneArchetypeCatalog and SituationArchetypeCatalog. Content authors never make structural choices.

**Configuration Space (JSON Data)**:
- Placement filters: where scenes spawn (NPC personality, location properties, route terrain)
- Spawn conditions: when scenes become eligible (player state, world state, time windows)
- Entity targeting: which specific NPCs/locations (concrete IDs or categorical filters)
- Spawn rules: frequency, cooldowns, exclusivity

Configuration specifies WHERE and WHEN pre-designed patterns appear, never WHAT those patterns contain.

**The Boundary**: If JSON authors need to make design decisions, the catalogue is incomplete. Create new archetypes rather than parameterizing existing ones.

### Hex Grid Spatial Foundation

The world exists on a hex grid using axial coordinates (Q, R). Every location occupies exactly one hex. This is architectural bedrock, not optional.

**Venue Geometry**: A venue consists of one center hex plus six adjacent hexes (7-hex cluster). All locations sharing VenueId must be geometrically adjacent on the hex grid. This creates spatial coherence - locations in "The Silver Chalice Inn" venue are physically adjacent hexes forming a cluster.

**Two Travel Systems**:
- **Intra-venue**: Movement between adjacent hexes in same venue. Instant and free BECAUSE geometrically adjacent. LocationActionCatalog generates "Move to X" actions for adjacent locations.
- **Inter-venue**: Movement between non-adjacent hexes in different venues. Requires routes, costs resources, takes time. Handled by route system.

**Location Identity**: Hex position is as fundamental as location name. Without it, location cannot participate in movement, cannot have routes generated to it, cannot verify adjacency. Not "optional metadata" - core identity.

**No Exceptions**: There are NO locations without hex positions in Wayfarer. Every location occupies one hex. "Intra-venue instant travel" describes movement BEHAVIOR (free, instant) not location STRUCTURE (no hex position). Movement is instant BECAUSE hexes are adjacent, not INSTEAD OF having hex positions.

---

## Architecture Implementation

The procedural content generation system implements property-driven generation philosophy with robust reliability guarantees.

**Critical Success Factor:** Scene generation system operates as isolated, thoroughly tested subsystem. Pure generation core with zero game dependencies enables independent testing. Parse-time validation prevents invalid templates from reaching runtime. Comprehensive test suite (unit, integration, end-to-end) provides confidence in mechanical correctness. See System Robustness Architecture section for complete implementation strategy ensuring bulletproof reliability.

### Two-Level Archetype System

Procedural generation operates through two distinct archetype layers that compose to create complete scenes.

**Level 1: Scene Archetypes** (SceneArchetypeCatalog)
- Purpose: Generate multi-situation STRUCTURES with transitions
- Granularity: 1-4 situations per scene
- Examples: `service_with_location_access` (4 situations), `transaction_sequence` (3 situations), `single_negotiation` (1 situation)
- Responsibility: Defines HOW situations connect and flow
- Decides: Which situation archetypes to use, transition patterns, dependent resources

**Level 2: Situation Archetypes** (SituationArchetypeCatalog)
- Purpose: Generate 4-CHOICE PATTERNS for individual situations
- Granularity: 1 situation = 4 choices (stat-gated, money, challenge, fallback)
- Examples: `negotiation`, `confrontation`, `investigation`, `social_maneuvering`, `crisis`, `service_transaction`
- Responsibility: Defines HOW players interact with a single situation
- Returns: Choice structure with costs, requirements, action types

**Composition Pattern**: Scene archetypes call situation archetypes. SceneArchetypeCatalog.Generate() invokes SituationArchetypeCatalog.GetArchetype() to produce choices for each situation. The scene archetype DEFINES which situation archetypes appear and in what configuration.

Example: `service_with_location_access` scene archetype internally decides:
- Situation 1 uses `social_maneuvering` OR `service_transaction` (based on NPC personality)
- Situation 2-4 are auto-progress (no choice archetype needed)
- Linear transitions connect them

The archetype ID encodes the complete pattern. No parameters. Scene identity IS its structure.

### Scene Archetype Catalog Detail

Twelve scene archetypes implemented. Six multi-situation, six single-situation. Each generates complete structure with mechanical orchestration.

UI layer supports multiple concurrent scenes at single NPC. Query pattern changed from FirstOrDefault (single scene) to Where (all scenes). Each active scene renders as separate interaction button. Navigation routing includes (npcId, sceneId) tuple for direct scene lookup. Physical NPC presence always visible, interaction buttons conditional on scene availability.

### Tag-Based Scene Spawning

Player owns Tags list ("EstablishedContact_Innkeepers_Vallenmarch", "DefeatedBandit_ForestRoad"). Scenes specify RequiredTags (what tags player must have to spawn scene) and GrantedTags (what tags player receives on scene completion). Scene templates match against player tags at spawn - no scene-to-scene ID dependencies.

SceneTemplate.CanSpawn(player, location) checks: player has all RequiredTags, player at correct location, not conflicting with existing active scene of same archetype. Returns SpawnEligibility result with diagnostic reason code.

SceneSpawnService.SpawnProcedural() queries eligible templates, passes to SpawnService with target entity references. SpawnService creates Scene instance with template + entities, marks Active.

Completing scene calls GrantTagsToPlayer(scene.GrantedTags). Player.Tags list updated. Future scene spawns now match new tags. State-based progression without hardcoded chains.

### PlacementFilter: Pure Configuration

PlacementFilter determines WHERE scene templates spawn without affecting scene structure or content. Filters operate as pure eligibility checks examining entity properties.

**NPC Filters**:
- Personality types (Innkeeper, Merchant, Guard, Scholar, etc.)
- Bond ranges (relationship thresholds)
- Regional affiliation (Vallenmarch, Thornwood, etc.)
- NPC-specific tags or states

**Location Filters**:
- Property types (Commercial, Residential, Wilderness, Sacred)
- Settlement tiers (Urban, Rural, Remote)
- Danger levels (Safe, Low, Medium, High, Extreme)
- Available services (Lodging, Trade, Healing, etc.)
- District or region membership

**Route Filters**:
- Terrain types (Road, Forest, Mountain, Desert)
- Danger ratings (Peaceful, Contested, Hostile)
- Travel tiers (Local, Regional, Continental)
- Route state (Open, Restricted, Blocked)

**Player State Filters**:
- Required/forbidden states (Wounded, Energized, etc.)
- Achievement gates (completed tutorials, story milestones)
- Scale positions (reputation, faction standing)
- Inventory requirements (must have/must not have items)

**Critical Distinction**: PlacementFilter never affects what a scene IS, only where it appears. A `single_negotiation` scene has identical structure whether spawning at Mercantile NPC in city or Diplomatic NPC in wilderness. Only narrative hints and stat requirements adjust based on context properties.

**JSON Authoring**: Content authors specify placement filters in scene template JSON. This is configuration space - choosing where pre-designed patterns appear.

### SpawnConditions: Temporal Gates

SpawnConditions determine WHEN scenes become eligible without affecting structure. These are pure eligibility checks examining temporal and state conditions.

**Player State Conditions**:
- Completed scenes (requires prior scene finished)
- Choice history (player made specific past choices)
- Stat thresholds (minimum/maximum stat values)
- Inventory possession (specific items held)
- Tag requirements (player has/lacks specific tags)

**World State Conditions**:
- Day ranges (scenes available days 5-10 only)
- Time blocks (morning/afternoon/evening/night)
- Weather conditions (clear/rain/storm)
- Seasonal timing (spring/summer/autumn/winter)

**Entity State Conditions**:
- NPC bond levels (relationship must exceed threshold)
- Location reputation (faction standing requirements)
- Route travel counts (first visit vs repeat visits)
- Entity-specific flags (NPC injured, location damaged)

**Cooldown Management**:
- Time since last spawn (prevent immediate respawning)
- Completion count limits (once per playthrough vs repeatable)
- Exclusivity constraints (cannot spawn if conflicting scene active)

SpawnConditions gate access without changing what happens when scene spawns. They answer "is this the right time?" not "what should this scene do?"

### Entity Property-Driven Difficulty

Difficulty modifier calculation queries entity properties and combines factors into total challenge difficulty.

**NPC Demeanor:** Friendly = -1, Neutral = 0, Hostile = +1, Aggressive = +2. Innkeeper with Demeanor=Friendly makes easier challenge.

**Location Danger:** Safe = -1, Low = 0, Medium = +1, High = +2, Extreme = +3. Urban tavern (Safe) versus wilderness camp (Medium) affects encounter difficulty.

**Service Cost Premium:** Basic = 0, Standard = +1, Premium = +2, Luxury = +3. Fancy spa bath costs more than public bathhouse.

**Time Pressure:** None = 0, Low = +1, Medium = +2, High = +3. Urgent checkpoint crossing harder than casual rest.

Total difficulty modifier = NPC + Location + ServiceCost + TimePressure. Base challenge cost modified by total. Ensures contextually appropriate difficulty while maintaining mechanical consistency.

### Spawn Context: Scenes Adapt to Current Location

SceneTemplate defines TargetArchetype (Location, NPC, Spot) + filter properties. For tutorial: concrete ID "elena_npc". For procedural: categorical filter "any Innkeeper at Urban location with Lodging service".

SpawnService.Spawn(template, playerId) determines current context. If player at location_id_123, passes that Location entity to generator. Generator queries location.UrbanLevel, location.DangerLevel, location.AvailableServices. Same for NPC if template targets NPC.

Template specifies archetype + filters. Spawn provides concrete entities matching those filters from current context. Generator receives actual entity objects, queries properties, generates appropriate content. Tutorial (concrete ID) and procedural (filters) both invoke same generation path with entity objects.

### Property Query Examples

Generator receives NPC object. Queries:
- npc.Personality (Innkeeper, Merchant, Guard, etc)
- npc.Demeanor (Friendly, Neutral, Hostile)
- npc.RegionalAffiliation (Vallenmarch, Thornwood, etc)

Generator receives Location object. Queries:
- location.Settlement (Urban, Rural, Wilderness)
- location.DangerLevel (Safe, Low, Medium, High)
- location.AvailableServices (Lodging, Trade, Healing, etc)

Generator receives Spot object (if applicable). Queries:
- spot.AccessControl (Open, Locked, Restricted)
- spot.PrivacyLevel (Public, Semi-Private, Private)
- spot.Comfort (Basic, Standard, Premium, Luxury)

These properties drive: challenge difficulty, resource costs, granted items, narrative tone hints for AI. Same archetype + different properties = contextually appropriate variation.

### Scene Self-Containment Strategy

Scene creation must not depend on pre-existing world content beyond base locations. When procedurally spawning "lodging at any inn", scene cannot assume room location exists. Player might trigger scene at location never visited before. Dependency on pre-authored content breaks procedural scalability.

**Solution: Dynamic Location with Hex Placement**. Scene generation orchestrates multi-system pipeline creating spatially-positioned dependent locations:

1. SceneInstantiator generates DependentLocationSpec with HexPlacementStrategy (SameVenue, Adjacent)
2. BuildLocationDTO determines hex coordinates: queries base location hex position, finds unoccupied adjacent hex, embeds Q and R in LocationDTO
3. ContentGenerationFacade converts DTO to JSON with hex coordinates
4. PackageLoaderFacade parses JSON creating Location entity with HexPosition from embedded coordinates
5. LocationActionCatalog generates intra-venue movement actions based on hex adjacency

**Hex Placement Strategies**:
- **SameVenue**: Find unoccupied adjacent hex within same venue cluster (7-hex boundary). Used for service locations maintaining venue coherence.
- **Adjacent**: Find unoccupied adjacent hex regardless of venue boundary. Used when dependent location may cross venue edges.

Both strategies use identical algorithm (find adjacent hex) - distinction is semantic intent, not implementation.

**Critical Requirement**: Dependent locations MUST have hex positions. Cannot create locations without spatial presence. FindAdjacentHex validates unoccupied adjacent hex exists, throws exception if venue cluster is full. This enforces spatial constraint: venues have maximum 7 locations (center + 6 adjacent).

**Parser-JSON-Entity Triangle for Hex Coordinates**:
- **JSON**: LocationDTO includes Q and R properties (nullable integers) for embedded coordinates
- **Parser**: LocationParser reads Q/R from DTO (if present) and assigns Location.HexPosition
- **Entity**: Location has HexPosition (AxialCoordinates) enabling spatial queries and movement generation

Static locations (from foundation JSON) get hex positions via HexParser.SyncLocationHexPositions (separate hex grid file). Dynamic locations (from scenes) get hex positions embedded in DTO. Both converge to same Location.HexPosition property - HIGHLANDER principle.

**Three Facade Architecture**:
- **ContentGenerationFacade:** Transforms typed specs into JSON files, knows file structure
- **PackageLoaderFacade:** Parses JSON into GameWorld entities, knows entity creation
- **SceneInstanceFacade:** Creates scene instances with entity references, knows scene lifecycle

Facades isolated from each other. GameFacade orchestrates pipeline. Strongly-typed value objects (LocationCreationSpec, DynamicFileManifest, LocationCreationResult, CleanupResult) ensure compile-time verification. No dictionary baggage, no generic types, no shared context objects.

**Lifecycle:** Scene requests location via spec (pattern with resolution variables, hex coordinate, properties). ContentGenerationFacade produces JSON. PackageLoaderFacade creates entity, returns ID. Scene references final ID. At scene completion/cleanup, ContentGenerationFacade deletes JSON, PackageLoaderFacade removes entity. Clean creation and destruction.

**Validation:** Parse-time checks verify structural validity. Runtime pipeline uses atomic transactions - complete success or clean rollback. Type system prevents invalid specifications. No fallbacks, no partial states, no graceful degradation hiding issues.

### Tutorial Scene "Secure Lodging" Example

Template JSON: archetype="service_with_location_access", target_npc="elena_npc", target_location="Elena's tavern", target_spot="elena_private_room".

Spawn at game start. Current location = tutorial town. Generator receives: Elena entity (Personality=Innkeeper, Demeanor=Friendly), Tavern entity (Services=Lodging, DangerLevel=Safe), Room entity (PrivacyLevel=Private, Comfort=Standard).

Generator queries entity properties:
- Elena.Demeanor=Friendly → challenge difficulty -1, conversational tone
- Tavern.DangerLevel=Safe → location modifier -1
- Room.Comfort=Standard → cost modifier +1
- Total difficulty: -1 (friendly NPC offsets room cost)

Generator produces negotiate situation with 4 choices enriched with conditional rewards:

1. **Use Rapport** (stat-gated, requires Rapport≥3): Immediate reward grants room key. OnChoice transition to access situation.
2. **Pay 5 Coins** (money, costs 5 coins): Immediate reward grants room key. OnChoice transition to access situation.
3. **Haggle** (challenge, routes to Social): Time/energy spent immediately. OnSuccessReward grants room key (only if challenge won). OnSuccess transition to access situation. OnFailure transition to fallback scene end.
4. **Leave** (fallback): No rewards. OnChoice transition to scene end. Scene completes without service.

Remaining situations (access, rest, depart) follow linear Always transitions. Room key consumed at access, resources restored at rest, key removed at depart.

**Tutorial Value**: Players learn strategic choice system. Safe options (rapport/money) have prerequisites. Risky option (haggle) universally available but uncertain. Fallback prevents soft-lock. Different paths produce same outcome (room access) through different resource expenditure (rapport/money/risk).

**Hex Placement**: Private room created adjacent to common room on hex grid. FindAdjacentHex queries common_room.HexPosition (center of tavern venue cluster), selects unoccupied neighbor hex, embeds Q and R in LocationDTO. After creation, LocationActionCatalog generates "Move to Elena's Lodging Room" intra-venue movement action. Player movement between common_room and private_room instant and free because hexes geometrically adjacent.

Same archetype at different location with Marcus (Demeanor=Professional, Services=Bathing) would generate: challenge difficulty 0, different narrative tone, different service effect (cleanliness restoration), same conditional transition structure, same hex placement pattern (bath room adjacent to base location).

### Procedural Scene "Find Lodging" Example

Template JSON: archetype="service_with_location_access", target_npc_filter={Personality=Innkeeper, RegionalAffiliation=CurrentRegion}, target_location_filter={Settlement=Urban, Services=Lodging}, target_spot_filter={PrivacyLevel=Any}.

Player enters new city. SpawnService checks eligibility: player has "EstablishedContact_Innkeepers_CurrentRegion" tag, no conflicting active scene. Queries current location for NPCs matching filter (any Innkeeper). Finds "marcus_npc". Queries location for lodging service. Finds "Golden Rest Inn". 

Generator receives: Marcus entity (Personality=Innkeeper, Demeanor=Professional), Inn entity (Services=Lodging, DangerLevel=Low), queries Marcus properties:
- Marcus.Demeanor=Professional → difficulty 0, formal tone
- Inn.DangerLevel=Low → location modifier 0  
- Room.Comfort=Premium → cost modifier +2
- Total difficulty: +2 (premium room)

Generator produces 4 situations with +2 difficulty modifier. Challenge costs more. Rest effect greater. Narrative tone professional rather than warm.

Player completes scene. Scene grants "CompletedLodging_GoldenRestInn" tag. Future scenes requiring "lodging experience in current region" now match.

### Formula-Driven Resource Costs

Cost calculation queries entity properties and applies formulas rather than hardcoded values.

**Base Cost Formula:** BaseCost = ArchetypeBaseCost + (DifficultyModifier * DifficultyScalar). Service_with_location_access base=10. Difficulty +2. Scalar=2. Total base=14.

**Service Effect Formula:** RestoreAmount = BaseBenefit * ComfortMultiplier. Rest base stamina=80. Comfort=Premium (1.5x). Total=120 stamina restored.

**Time Cost Formula:** Duration = BaseTime * ServiceTier. Rest base=6 segments. Tier=Standard (1.0x). Total=6 segments consumed.

**Item Value Formula:** KeyItemWorth = LocationPrestige * PrivacyLevel. Prestige=2, Privacy=3. Key protected as valuable item.

Formulas ensure consistent economy. Same service at different locations costs appropriately. Premium comfort justifies higher coin cost. Procedural and tutorial use identical formulas - mechanical consistency guaranteed.

### Scene State Machine

Scene lifecycle through typed states with explicit transitions. No boolean flags, no ambiguous states.

**States:**
- **Dormant:** Scene exists in GameWorld but not yet active. Waiting for player to reach spawn context.
- **Active:** Scene ready for interaction. Current situation instantiated with choices. Player can engage.
- **Paused:** Scene in progress but player navigated away. Situation suspended. Resumes on return.
- **Completed:** All situations finished. Granted tags applied. Scene archives.
- **Abandoned:** Player chose to leave. Scene cleanup removes created resources. No tag grants.

**Transitions:**
- Dormant → Active: Player reaches PlacedLocation, triggers ActivateScene(). Current situation transitions Active.
- Active → Paused: Player navigates elsewhere. Current situation suspends. Scene remains in GameWorld.
- Paused → Active: Player returns to PlacedLocation. Scene resumes. Current situation re-activates.
- Active → Completed: Player completes all situations. GrantTags() called. Scene archives.
- Active → Abandoned: Player explicitly abandons. Cleanup() called. Scene removed.

State transitions logged for debugging. Audit trail shows: spawn → activate → pause → resume → complete. Diagnostic errors reference state history.

### Scene State Machine with Transition Matching

Scenes progress through situations using typed transitions with condition evaluation. When situation completes, Scene.AdvanceToNextSituation queries transitions to determine next situation. If matching transition exists, scene updates CurrentSituationId and continues. If no matching transition found, scene marks Completed and despawns.

**Situation Identity Duality (HIGHLANDER Pattern D)**: Each runtime situation has TWO identifiers serving distinct purposes:
- **Id**: Unique instance identifier (GUID). Ensures runtime uniqueness when same template spawns multiple times.
- **TemplateId**: References source SituationTemplate. Enables matching against template-defined rules.

Both stored on Situation entity. Template ID copied from template during instantiation, never changes. Instance ID generated at creation, ensures global uniqueness.

**Transition Matching Logic**: SituationTransition objects define SourceSituationId and DestinationSituationId using template IDs (not instance IDs). Template authors define transitions at authoring time using template identifiers. Runtime matching uses completedSituation.TemplateId to find transitions, connecting template-defined rules to runtime instances.

**Critical Requirement**: GetTransitionForCompletedSituation must compare against TemplateId, not Id. Using instance Id causes matching failure (transitions reference templates, not specific instances). Scene finds no matching transition, marks Completed, despawns prematurely creating soft-lock bug.

**Pattern Rationale**: Templates authored before instances exist. Transition rules defined in templates using template IDs. Runtime situations store both IDs enabling template-based matching (transition logic) and instance-based uniqueness (state tracking). This is HIGHLANDER Pattern D specialization where "persistence ID" is actually template ID.

**Scene Completion**: When GetTransitionForCompletedSituation returns null (no matching transition), scene interprets this as "no more situations" and marks itself Completed. Scene despawns, removes from active scenes list. This is correct terminal state for linear progression reaching end. Incorrect when transitions exist but matching fails due to wrong ID comparison.

### Conditional Transition Architecture

Scenes support branching narratives where player choices and challenge outcomes determine progression paths. The conditional transition system enables strategic decision-making with mechanical consequences.

**TransitionCondition Evaluation**: Each SituationTransition has a Condition property with four values:

- **Always**: Transition occurs unconditionally when source situation completes. Used for linear progression.
- **OnChoice**: Transition occurs only if specific choice was selected. Enables branching based on player's strategic decision (use rapport vs pay money vs attempt challenge).
- **OnSuccess**: Transition occurs only if challenge succeeded. Routes to success consequences.
- **OnFailure**: Transition occurs only if challenge failed. Routes to failure consequences.

**Evaluation Priority**: Scene.GetTransitionForCompletedSituation(situation) evaluates conditions in priority order:

1. OnChoice conditions (most specific) - matches SpecificChoiceId against Situation.LastChoiceId
2. OnSuccess/OnFailure conditions (outcome-based) - checks Situation.LastChallengeSucceeded
3. Always conditions (fallback) - unconditional progression

Multiple transitions can share the same SourceSituationId with different conditions. The first matching transition in priority order determines the next situation.

**Outcome Tracking**: Situation entity stores completion metadata enabling condition evaluation:

- **LastChoiceId**: string nullable. Set when player selects a choice. Used for OnChoice condition matching.
- **LastChallengeSucceeded**: bool nullable. Null means no challenge occurred. True/false indicates challenge outcome. Used for OnSuccess/OnFailure evaluation.

These properties live alongside LifecycleStatus as completion metadata. They're set by SituationCompletionHandler when the situation finishes.

**Architectural Boundary**: Scene.GetTransitionForCompletedSituation accepts Situation object (not just ID) to enable condition evaluation. This keeps Scene as data with pure domain logic. The caller (AdvanceToNextSituation) queries GameWorld to retrieve the Situation, then passes it to the evaluation method.

**Choice Differentiation Pattern**: Different choice types have different reward timing based on action mechanics:

- **Stat-gated choices** (Use rapport, Show credentials): Requirements checked, immediate reward granted on selection. Success guaranteed if requirements met.
- **Money choices** (Pay coins, Offer bribe): Cost deducted, immediate reward granted on selection. Transactional - payment equals service.
- **Challenge choices** (Haggle, Negotiate, Intimidate): Route to tactical system. Immediate effects applied (time passes, energy spent), but unlock rewards granted only on challenge success via OnSuccessReward.
- **Fallback choices** (Leave, Walk away): No requirements, no costs, no rewards. Always available exit option preventing soft-locks.

This pattern creates meaningful strategic decisions. Safe choices (stat-gated, money) have prerequisites but guarantee results. Risky choices (challenge) are universally available but outcome uncertain. Fallback preserves player agency without rewarding abandonment.

### Conditional Rewards System

ChoiceTemplate has three reward properties supporting different timing requirements:

**RewardTemplate**: Applied immediately when choice selected. Used for effects that occur BEFORE outcome determination:
- Time passage (challenge takes 2 segments whether you win or lose)
- Resource consumption (haggling costs energy regardless of success)
- State changes (conversation advances to negotiation phase)

**OnSuccessReward**: Applied when challenge succeeds. Used for success consequences:
- Location unlocks (room key granted only if persuasion succeeds)
- Item grants (merchant gives discount item only if negotiation wins)
- Reputation gains (NPC approves only if challenge successful)

**OnFailureReward**: Applied when challenge fails. Used for failure consequences:
- Reputation losses (NPC becomes hostile after failed intimidation)
- State degradation (checkpoint closes after failed breach attempt)
- Consolation items (NPC pities you, offers lesser alternative)

All three properties are nullable ChoiceReward objects. Null means "no rewards for this timing phase."

This tri-phase reward system enables nuanced choice mechanics. Immediate rewards apply unconditionally (representing the attempt itself), while conditional rewards apply based on tactical outcome (representing success/failure consequences).

### Catalogue Composition Pattern

The architecture separates generic situation patterns from scenario-specific rewards through catalogue composition.

**SituationArchetypeCatalog**: Generates reusable situation patterns. Method `GenerateChoiceTemplates(SituationArchetype)` returns four ChoiceTemplate objects with empty reward properties:
- Stat-gated choice (high stat requirement, no cost)
- Money choice (coin cost, moderate requirement)
- Challenge choice (low requirement, routes to tactical)
- Fallback choice (no requirement, no cost, no reward)

These generic choices define cost/requirement structure but not rewards. The archetype doesn't know what unlocks - that's scenario-specific.

**SceneArchetypeCatalog**: Generates scenario-specific scenes. Method `GenerateServiceWithLocationAccess` calls SituationArchetypeCatalog to get generic choices, then enriches them with scenario rewards:

```
// Get generic negotiation choices
var genericChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(SituationArchetype.Negotiation);

// Enrich with lodging-specific rewards
var enrichedChoices = genericChoices.Select(choice => {
  if (choice.Id.EndsWith("_rapport") || choice.Id.EndsWith("_money")) {
    // Stat-gated and money choices grant unlock immediately
    return new ChoiceTemplate {
      ...choice properties...,
      RewardTemplate = new ChoiceReward {
        LocationsToUnlock = new List<string> { "generated:private_room" },
        ItemIds = new List<string> { "generated:room_key" }
      }
    };
  }
  else if (choice.Id.EndsWith("_challenge")) {
    // Challenge choice grants unlock only on success
    return new ChoiceTemplate {
      ...choice properties...,
      OnSuccessReward = new ChoiceReward {
        LocationsToUnlock = new List<string> { "generated:private_room" },
        ItemIds = new List<string> { "generated:room_key" }
      }
    };
  }
  else {
    // Fallback choice grants nothing
    return choice;
  }
}).ToList();
```

Enrichment creates NEW ChoiceTemplate instances with populated reward properties. Never mutates returned objects - catalogues generate immutable structures.

This pattern enables reuse. Negotiation archetype used by lodging, bathing, healing, checkpoint, and transaction scenarios. Each enriches with scenario-appropriate rewards. Generic pattern + specific context = complete template.

### Marker Resolution System

Dependent resources use "generated:" prefix markers enabling templates to reference resources that don't exist yet.

**At Parse Time**: SceneArchetypeCatalog creates DependentLocationSpec with TemplateId "private_room". Situation.RequiredLocationId set to marker "generated:private_room". ChoiceReward.LocationsToUnlock contains "generated:private_room". Template complete with markers unresolved.

**At Instantiation Time**: SceneInstantiator spawns scene. Calls ContentGenerationFacade to create locations from DependentLocationSpecs. Receives actual IDs ("scene_abc123_private_room"). Builds MarkerResolutionMap: `{"generated:private_room" → "scene_abc123_private_room"}`. Applies map to all situation and reward references. Situation.ResolvedRequiredLocationId populated with actual ID.

**At Query Time**: RewardApplicationService applies ChoiceReward. Finds "generated:private_room" in LocationsToUnlock. Queries MarkerResolutionMap for actual ID. Unlocks "scene_abc123_private_room". Marker fully resolved to concrete entity.

This enables template composition at parse time while deferring resource creation to instantiation time. Templates remain pure, reusable structures. Instances track concrete entity bindings.



### Multiple Concurrent Scenes

Player can have multiple active scenes simultaneously. UI queries GameWorld.Scenes.Where(s => s.State == Active). Each scene tracks independent progression through situations.

**Example:** Player has lodging scene at Elena's tavern (situation 2 of 4: Access), shopping scene at Merchant Guild (situation 1 of 3: Browse), checkpoint scene at North Gate (situation 1 of 2: Negotiate). All Active, all paused while player elsewhere. Returning to any location resumes that scene's current situation.

**Conflict Prevention:** SpawnService checks ArchetypeConflict. Cannot have two "lodging" scenes active simultaneously (one at Elena's tavern, one at Marcus's inn). Must complete or abandon first. Prevents: "you're already resting somewhere else" paradox.

**Priority System:** When multiple scenes active at same location, UI sorts by: (1) scene with active situation, (2) scene most recently interacted, (3) scene spawned first. First viable scene gets default interaction. Others available via "more options" expansion.

### Scene Spawning Service

**SceneSpawnService** evaluates eligibility and creates scene instances.

**EvaluateTutorialScenes(player):** Queries TutorialSceneTemplates. Checks each template.CanSpawn(player, currentLocation). Returns eligible templates with spawn priority. Tutorial scenes spawn immediately at game start if eligible.

**EvaluateProceduralScenes(player):** Queries ProceduralSceneTemplates. Matches RequiredTags against player.Tags. Matches location/NPC filters against current context. Returns eligible templates. Spawns highest priority eligible template if none conflicting.

**SpawnScene(template, player):** Invokes SceneArchetypeCatalog.Generate(template.Archetype, targetEntities). Receives generated structure (situations, choices, effects). Creates Scene instance in GameWorld. Sets State=Dormant initially. Scene activates when player enters PlacedLocation.

**CleanupCompleted(scene):** Scene completed. Calls GrantTags(player, scene.GrantedTags). Removes scene from GameWorld.Scenes. Archives scene metadata for replay/debugging.

**CleanupAbandoned(scene):** Scene abandoned. No tag grants. Removes scene from GameWorld. Calls ResourceCleanup() to remove dynamically-created locations/items if scene created them.

### Scene Spawning Context Architecture

SceneSpawnContext carries information about WHERE and HOW scenes spawn. Context building is orchestration-level logic centralized in SceneSpawnContextBuilder utility (HIGHLANDER: single shared implementation).

**Context Properties**:
- **Player**: Player entity (always required)
- **CurrentLocation**: Location where scene takes place (required for dependent location generation using VenueIdSource.SameAsBase)
- **CurrentNPC**: NPC involved in scene (optional, depends on placement type)
- **CurrentRoute**: Travel route where scene occurs (optional, for journey encounters)
- **CurrentSituation**: Parent situation that triggered spawn (optional, null for starter/procedural scenes)

**SceneSpawnContextBuilder.BuildContext()**: Static utility method in Content folder accepting GameWorld, Player, PlacementRelation, SpecificPlacementId, and optional CurrentSituation. Returns fully populated SceneSpawnContext through entity resolution:

**SpecificNPC Resolution**:
1. Find NPC entity by ID in GameWorld.NPCs
2. Use NPC.Location (object reference resolved at parse time from locationId in JSON)
3. Set both CurrentNPC and CurrentLocation in context
4. Return null if NPC not found or Location is null

**SpecificLocation Resolution**:
1. Find Location entity by ID in GameWorld.Locations
2. Set CurrentLocation in context
3. Return null if location not found

**SpecificRoute Resolution**:
1. Find Route entity by ID in GameWorld.Routes
2. Set CurrentRoute in context
3. If route has OriginLocation, set CurrentLocation as well
4. Return null if route not found

**SameLocation/SameNPC/SameRoute Resolution**:
- Inherit context properties from CurrentSituation's parent scene
- Copy relevant properties maintaining context chain

**Generic Resolution**:
- Return context with only Player set
- SceneInstantiator handles placement resolution from categorical filters
- Relies on PlacementFilter evaluation rather than concrete binding

**Parse-Time Object References**: NPCs use Location property (object reference) resolved during JSON parsing from locationId field. Context building uses direct object navigation (npc.Location) rather than runtime ID lookups. This pattern applies universally: prefer parse-time resolved object references over runtime string-based resolution. Parse-time resolution runs once (cheaper), uses object references (safer), enables direct navigation (simpler).

**HIGHLANDER Enforcement**: SceneSpawnContextBuilder is the SOLE implementation of context building logic. No duplicate implementations allowed anywhere in codebase. All context construction (GameFacade.SpawnStarterScenes, ObligationActivity.ProcessObligationReward, tests) calls this utility. Prevents inconsistency, reduces maintenance burden, simplifies testing. Code reviews reject new switch statements duplicating this logic.

**Fail-Fast Error Handling**: Builder returns null when entity resolution fails (NPC not found, Location not found). Calling code checks null and decides policy (log warning, skip spawn, propagate error). Don't paper over missing data with defaults or fallbacks. Null return signals DATA QUALITY ISSUE requiring JSON fixes, not runtime handling.

### LocationAction Generation for Dynamic Content

LocationActions enable player interaction with locations. Two distinct action types generated through different mechanisms, both triggered when locations enter GameWorld.

**Property-Based Actions** (feature gates):
- Generated by examining LocationProperties enum flags
- Crossroads property → inter-venue travel action
- Commercial property → work and job board actions
- Restful property → rest action
- SleepingSpace property → sleep action
- Properties gate access to functionality

**Hex-Based Intra-Venue Actions** (spatial navigation primitives):
- Generated by examining HexMap adjacency relationships
- For each location, find all adjacent hexes with locations in same venue
- Create IntraVenueMove action for each adjacent location
- Property-independent - spatial relationships alone determine generation
- Movement instant and free because geometrically adjacent

**Generation Timing**: LocationActionCatalog.GenerateActionsForLocation called when locations added to GameWorld. For static content, PackageLoader.LoadAllPackages calls GenerateLocationActionsFromCatalogue after all JSON loaded. For dynamic content, PackageLoader.LoadDynamicPackageFromJson calls GenerateLocationActionsFromCatalogue after loading package. This ensures dynamic locations get actions immediately upon creation.

**Regeneration for All Locations**: When dynamic content adds location, actions regenerated for ALL locations in GameWorld, not just new location. Reason: adjacency relationships change. Adding location at hex (1,-1) affects adjacent location at hex (0,-1) which now has additional neighbor. Regeneration ensures consistency. Duplicate detection via Contains check before adding actions makes regeneration safe.

**Action Generation Atomicity**: Actions must generate atomically with location addition. If location enters GameWorld.Locations, its actions must exist before method returns. No gaps where locations exist but lack accessibility. Any future dynamic content loading mechanism must call GenerateLocationActionsFromCatalogue following this pattern.

**Architectural Separation**: Routes and actions are separate concepts. Routes enable pathfinding (HexRouteGenerator creates route objects). Actions enable player interaction (LocationActionCatalog creates action objects). Dynamic locations need both. HexRouteGenerator runs during dependent resource loading creating routes. GenerateLocationActionsFromCatalogue runs after loading creating actions.

### Parse-Time Catalogue Pattern

Catalogues execute exclusively at parse time (game initialization). Runtime never calls catalogues. This timing separation is fundamental to performance and architecture clarity.

**Parse Time Execution**:
- JSON files loaded into DTOs
- SceneTemplateParser invokes SceneArchetypeCatalog.Generate() for each scene template
- SceneArchetypeCatalog internally calls SituationArchetypeCatalog.GetArchetype() as needed
- Generated templates stored in GameWorld collections
- All generation complete before first frame renders

**Runtime Prohibition**:
- Facades never import catalogue classes
- Managers never import catalogue classes  
- Services never import catalogue classes
- UI never imports catalogue classes
- Catalogues live in Content/Catalogues folder, isolated from runtime code

**Why This Matters**: Catalogue generation is computationally expensive (property queries, formula calculations, validation checks). Running once at startup is free. Running every scene spawn would be prohibitive. The architectural boundary enforces this performance requirement through import restrictions.

**Template Immutability**: Generated templates are immutable after parsing. Runtime never modifies them. They're shared across all instances of that template. This enables efficient reuse without defensive copying. Scene instances track mutable runtime state (current situation, granted items, time consumed). Templates remain pure, stateless patterns.

**Facade Pattern**: Runtime code queries GameWorld.SceneTemplates and GameWorld.SituationTemplates collections. These were populated at parse time. Facades instantiate Scene instances from templates without regenerating structure. The separation is complete: parse-time generates, runtime queries.

### Catalogue Composition Through Two-Level System

The two-level archetype system composes through explicit catalogue calls. Scene catalogues invoke situation catalogues, never vice versa.

**Composition Direction**: SceneArchetypeCatalog → SituationArchetypeCatalog (top-down only)

**Scene Catalogue Responsibilities**:
- Define complete scene structure (situation count, ordering, transitions)
- Select which situation archetypes to use for each situation
- Generate dependent resources (locations, items, spots)
- Enrich generic choices with scenario-specific rewards
- Return SceneArchetypeDefinition containing everything

**Situation Catalogue Responsibilities**:
- Define 4-choice patterns (stat-gated, money, challenge, fallback)
- Specify base costs and requirements (formulas, thresholds)
- Determine action types (Instant, StartChallenge, Navigate)
- Return SituationArchetype with empty reward templates
- Never know about scenes or multi-situation arcs

**Composition Example** (`service_with_location_access` archetype):
1. SceneArchetypeCatalog.Generate("service_with_location_access", entities) invoked
2. For negotiate situation: calls SituationArchetypeCatalog.GetArchetype(SituationArchetype.SocialManeuvering)
3. Receives 4 ChoiceTemplate objects with empty rewards
4. Creates NEW ChoiceTemplate instances enriching rewards (room key + location unlock)
5. Enriched choices become negotiate situation's ChoiceTemplates
6. Situations 2-4 generated without situation archetype (auto-progress)
7. Returns SceneArchetypeDefinition with 4 SituationTemplates and Linear transitions

**Never Mutate**: SceneArchetypeCatalog never modifies ChoiceTemplates returned from SituationArchetypeCatalog. It creates new instances with additional properties populated. This preserves catalogue purity and prevents hidden coupling.

**Identity Encoding**: Scene archetype name encodes which situation archetypes it uses. `single_negotiation` MEANS "1 situation using negotiation archetype." `service_with_location_access` MEANS "4 situations, first using social_maneuvering or service_transaction (based on NPC personality)." No parameterization - archetype IS the definition.

### Generator Architecture (Critical)

SceneGenerator receives: archetype ID, target entity objects, player state. Queries entity properties. Applies archetype-specific logic. Returns generated structure as typed objects.

**Pure Computation:** Generator has zero dependencies on GameWorld. Receives value objects, returns value objects. No side effects, no state mutation, no database queries. Deterministic output for given inputs. Enables isolated testing.

**Property Queries:** Generator queries: npc.Personality, npc.Demeanor, location.Settlement, location.Services, spot.PrivacyLevel, spot.Comfort. Uses properties to determine: challenge difficulty, resource costs, granted items, narrative tone.

**Formula Application:** Applies consistent formulas across all archetype instances. Cost = base + (difficulty * scalar). Benefit = base * comfort multiplier. Duration = base * tier. Same formulas for tutorial and procedural. Economic consistency guaranteed.

**Output Structure:** Returns GeneratedSceneStructure object containing: List<SituationTemplate> (ordered sequence), each with List<ChoiceTemplate>, each with List<Effect>. Templates contain mechanical data only - costs, items, formulas. Narrative content generated later by AI.

**Validation:** Structure validated at generation time. Checks: situations sequential, no orphaned effects, item lifecycle (granted before consumed), location references valid, formulas produce positive values. Invalid structure throws GenerationException with diagnostic details.

### SceneInstanceFacade

**CreateSceneInstance(generatedStructure, targetEntities):** Receives validated structure from generator. Creates Scene domain entity. Creates Situation entities from templates. Creates Choice entities with Action references. Links situations sequentially. Assigns to GameWorld.Scenes collection. Scene starts Dormant state. Returns Scene object.

**ActivateSituation(scene, situationIndex):** Transitions situation from Dormant to Active. Instantiates choices as interactable UI elements. Evaluates prerequisite effects (item checks, resource verification). Situation now ready for player selection.

**ExecuteChoice(choice):** Player selected choice. Applies effects in order: resource modifications, item grants/removes, location state changes, time advancement. Records choice in scene history. Transitions situation to Completed. If more situations remain, activates next. Otherwise completes scene.

**PauseScene(scene):** Player navigated away. Transitions current situation from Active to Paused. Preserves state. Scene remains in GameWorld. Resume on return.

**ResumeScene(scene):** Player returned. Transitions current situation from Paused to Active. Reactivates choices. Player continues.

**CompleteScene(scene):** All situations finished. Calls GrantTags(). Removes scene from active list. Archives metadata.

Facade handles scene lifecycle orchestration. Domain entities own their state. Generator remains pure computation.

### Template vs Instance (Critical Distinction)

**SceneTemplate** (JSON-serialized, parse-time):
- Archetype ID reference
- Target entity references (concrete IDs or filters)
- RequiredTags, GrantedTags
- Priority value
- Static, immutable after parse
- Lives in catalog, never mutates at runtime

**Scene** (Domain entity, runtime):
- Reference to originating template (for respawn)
- PlacedLocation, PlacedNPC (actual entity references)
- Current state (Dormant/Active/Paused/Completed)
- Current situation index
- Scene-specific runtime state (items granted, time consumed)
- Mutable, tracks player progression

Generator operates on templates, produces structure. SceneInstanceFacade creates Scene instances. Templates reused for multiple spawns (tutorial scene respawns on new game). Scene instances track unique player progression through specific spawn.

### Scene-Created Resources (Locations, Items, Spots)

Scene may create resources during spawn. "Lodging" scene creates private room location, bed spot, room key item. These resources specific to scene instance. Must cleanup on scene completion/abandon.

**Creation:** Scene requests resources via ContentGenerationFacade. Facade creates JSON files. PackageLoaderFacade parses into entities. Scene stores resource IDs. Resources exist in GameWorld.

**Lifecycle Tracking:** Scene owns DynamicResourceManifest listing created resource IDs. On complete/abandon, calls ContentGenerationFacade.RemoveDynamicResources(manifest). Entities removed, JSON deleted. No orphaned content.

**Validation:** Pipeline validates resources at creation. Spot requires parent location exists. Item requires granted before consumed. Lock requires unlock action later. Validation prevents broken resource chains.

### AI Generation Call Structure

AI called per situation, not per scene. Each situation activation triggers context bundling and generation.

**Context Bundle:**
- Entity objects: NPC with all properties, Location with services, Spot if relevant
- Situation metadata: position in sequence (1 of 4), preceding outcomes
- Player state: current resources, time of day, held items
- Narrative hints derived from properties: "friendly tone", "formal setting", "luxury service"

**Generation Call:** AI receives context bundle. Returns:
- Situation description text (2-3 sentences introducing situation)
- Choice action texts (one per choice, 1 sentence each)
- Outcome narratives (one per choice, 2-3 sentences consequence)

**Sequential Coherence:** Situation 2 generates with Situation 1 outcome. Situation 3 sees both previous outcomes. AI maintains continuity across sequence because receives full context each time. No separate "remember previous" mechanism - context includes it.

**Storage:** Generated texts stored directly on Situation and Choice properties. Remain fixed for this playthrough. New spawn generates new text (same mechanics, different narrative).

### Scene Advancement Flow

When situation completes, scene evaluates transitions to determine next situation. The flow demonstrates conditional transition system in action.

**Completion Trigger**: Player selects choice. For Instant actions, SituationCompletionHandler.CompleteSituation executes immediately. For StartChallenge actions, tactical system calls CompleteSituation after challenge resolves.

**Outcome Recording**: CompleteSituation sets Situation.LastChoiceId to selected choice ID. If challenge occurred, sets Situation.LastChallengeSucceeded to true/false based on outcome. These properties enable condition evaluation.

**Transition Evaluation**: Scene.AdvanceToNextSituation queries GameWorld for completed Situation object. Calls Scene.GetTransitionForCompletedSituation(situation). Method evaluates:

1. Check OnChoice transitions. If situation.LastChoiceId matches transition.SpecificChoiceId, return that transition.
2. Check OnSuccess/OnFailure transitions. If situation.LastChallengeSucceeded == true, return OnSuccess transition. If false, return OnFailure transition.
3. Check Always transitions. Return first Always transition from source situation.
4. No matching transition found - complete scene.

**Next Situation Activation**: Returned transition specifies DestinationSituationId. Scene.CurrentSituationId updated to destination. New situation activated. If destination dormant, instantiate its choices. Player sees new situation options.

**Example Flow - Lodging Negotiate**:
- Player selects "Haggle" choice (challenge)
- Routes to Social challenge system
- Challenge resolves (player wins)
- CompleteSituation called with LastChoiceId="negotiate_challenge", LastChallengeSucceeded=true
- GetTransitionForCompletedSituation evaluates: OnSuccess transition matches (LastChallengeSucceeded==true)
- Scene advances to "Access" situation
- Player can now unlock room and enter

**Example Flow - Challenge Failure**:
- Player selects "Haggle" choice
- Routes to Social challenge
- Challenge resolves (player loses)
- CompleteSituation called with LastChoiceId="negotiate_challenge", LastChallengeSucceeded=false
- GetTransitionForCompletedSituation evaluates: OnFailure transition matches (LastChallengeSucceeded==false)
- Scene completes without granting room access
- Player must try different approach or leave location

This architecture enables true branching. Same situation, different outcomes, different paths. Player choices and tactical skill determine progression through scene graph.

### Separation of Concerns (Architecture Principle)

Each subsystem has single responsibility. Subsystems interact via explicit typed interfaces only.

**SceneGenerator:** Pure computation. Receives entity properties, returns structure. No GameWorld dependency.

**SceneInstanceFacade:** Lifecycle orchestration. Creates/activates/pauses/completes scenes. Coordinates domain entities.

**SceneSpawnService:** Eligibility evaluation. Matches templates against player state. Decides what spawns when.

**ContentGenerationFacade:** Resource creation. Generates JSON files for locations/items/spots. File system only.

**PackageLoaderFacade:** Resource parsing. Reads JSON, creates entities. GameWorld integration only.

**AI Generation Service:** Narrative generation. Receives context, returns text. No game logic.

Clear boundaries prevent tight coupling. SceneGenerator doesn't know about UI. SpawnService doesn't know about resource files. AI service doesn't know about mechanics. Each testable in isolation.

### Debug Visualization Tools

**SceneDebugView:** Shows all scenes with: template archetype, target entities, current state, situation index, elapsed time, created resources. Sortable, filterable. Identifies stuck scenes.

**TemplateDebugView:** Shows all templates with: eligibility criteria, required/granted tags, priority. Highlights eligible but not spawned. Diagnoses spawn issues.

**PropertyQueryLog:** Logs every entity property query during generation. Shows: NPC.Demeanor=Friendly retrieved 3 times, Location.DangerLevel=Safe retrieved 1 time. Identifies missing properties causing default fallbacks.

**StateTransitionLog:** Logs every scene state change with timestamp. Shows: Dormant→Active, Active→Paused, Paused→Active, Active→Completed. Audit trail for investigating progression issues.

**ResourceManifestView:** Shows dynamically-created resources per scene. Tracks: creation timestamp, parent scene, cleanup status. Identifies orphaned resources.

These tools are first-class development features, not afterthoughts. Comprehensive observability into generation and lifecycle enables rapid diagnosis of issues.

### Parse-Time Validation (Critical Quality Gate)

All structural validation happens at content load, not at runtime. Invalid templates rejected immediately with diagnostic error.

**Template Validation:**
- Archetype ID exists in catalog
- Entity references (if concrete IDs) exist in foundation data
- RequiredTags properly formatted ("verb_subject_context")
- GrantedTags properly formatted
- Priority value positive integer
- Target entity filters valid (Properties exist on entity type)

**Structure Validation:**
- Situations ordered sequentially (situation 1 before 2 before 3)
- Each situation has at least one choice
- Each choice has at least one effect
- Item effects properly ordered (grant before consume)
- Location/spot references resolve (spot has parent location)
- Formulas produce positive values given property ranges

**Validation Error Format:** "Template 'find_lodging' failed validation: Target NPC filter references non-existent property 'Charisma'. Valid properties: Personality, Demeanor, RegionalAffiliation." Precise diagnostic with fix guidance.

Invalid templates prevent game launch. No runtime fallbacks, no graceful degradation. Fix content or game doesn't start. Forces quality.

### Comprehensive Testing Strategy (1000+ Tests)

**Unit Tests (Core Generation):**
- Test each archetype generator in isolation
- Mock entity property queries with test data
- Verify correct structure output for all property combinations
- Test formula calculations with boundary values
- 300+ tests covering all generation paths

**Integration Tests (System Interaction):**
- Test spawn service with real templates and entity data
- Test scene lifecycle state transitions
- Test resource creation pipeline (facade coordination)
- Test AI context bundling with actual entities
- 150+ tests covering cross-system interaction

**End-to-End Tests (Full Flow):**
- Test complete scene from spawn to completion
- Test multiple concurrent scenes
- Test scene with dynamic location creation
- Test abandoned scene cleanup
- 30+ tests covering full player experience

**Property Tests (Randomized):**
- Generate random entity property combinations
- Verify generation never fails (always valid output)
- Verify formulas always positive
- Verify no orphaned resources
- 100+ randomized test runs per property test

**Regression Tests (Past Bugs):**
- Each bug becomes permanent test
- Test suite prevents bug reoccurrence
- Living documentation of edge cases
- 50+ tests from production issues

**Test Coverage Target:** 95%+ line coverage on generation core. 100% path coverage on state transitions. Zero untested error paths.

### System Robustness Architecture (Mission-Critical Infrastructure)

Scene generation must be bulletproof. This is load-bearing infrastructure where failure is not an option.

**Pure Generation Core:**
- Zero dependencies on GameWorld
- Receives value objects, returns value objects  
- Deterministic output for given inputs
- No I/O, no randomness, no side effects
- Enables exhaustive isolated testing
- 100% reproducible results

**Parse-Time Validation:**
- All structural errors caught at content load
- Templates validated before runtime
- Invalid content prevents game launch
- No runtime error paths from bad templates
- Forces content quality at source

**Explicit State Machines:**
- Scene lifecycle: Dormant → Active → Paused → Completed
- Situation lifecycle: Dormant → Active → Completed
- No boolean flags (isActive, isCompleted, isPaused)
- Single current state variable of enum type
- Transitions explicit with logged reasons
- Impossible to be in multiple states

**Audit Trails:**
- Every entity property query logged
- Every state transition logged with timestamp
- Every resource creation tracked in manifest
- Complete traceability for debugging
- Post-mortem analysis from logs alone

**Typed Contracts:**
- No Dictionary<string, object> baggage
- No generic "Context" objects
- Every input/output strongly typed
- Compiler enforces valid interactions
- Runtime type errors impossible

**Atomic Operations:**
- Scene creation: complete success or clean rollback
- Resource pipeline: all or nothing
- Effect application: atomic transaction
- No partial states, no "try to continue" logic
- Crash loudly on failure with full diagnostic

**Comprehensive Testing:**
- 1000+ unit tests (generation core)
- 150+ integration tests (system interaction)  
- 30+ end-to-end tests (full scenarios)
- 100+ property tests (randomized inputs)
- 50+ regression tests (past bugs)
- Continuous integration runs full suite
- Zero tolerance for failing tests

**Debug Observability:**
- SceneDebugView (all scenes, states, resources)
- TemplateDebugView (eligibility, tags, priority)
- PropertyQueryLog (entity queries during generation)
- StateTransitionLog (audit trail)
- ResourceManifestView (dynamic content tracking)
- First-class dev features, always available

**Let It Crash Philosophy:**
- No graceful degradation hiding problems
- No fallbacks papering over issues  
- No partial results "just in case"
- Invalid input → immediate crash with diagnostic
- Forces root cause fixes, not workarounds
- Better to fail visibly than succeed silently with corruption

This architecture ensures scene generation is rock-solid infrastructure. The system either works perfectly or fails immediately with complete diagnostic information forcing fixes. No middle ground where broken state propagates silently.

### Scene Archetype Catalog Detail

Six scene archetypes implemented. Each generates multi-situation arc with mechanical orchestration.

#### 1. Service With Location Access

**Purpose:** NPC provides service requiring entry to private location spot. Common pattern: lodging (inn room), bathing (bathhouse), healing (temple chamber).

**Structure:**
- Situation 1: Negotiate service. Multiple choices with different unlock mechanics.
- Situation 2: Navigate to spot, consume access item to unlock. Enter location.
- Situation 3: Consume service. Resources restored, time advances. Exit location.
- Situation 4: Departure cleanup. Remove access item, re-lock spot.

**Conditional Transition Mechanics**: Negotiate situation uses OnChoice transitions enabling branching:

- **Stat-gated choice** (Use rapport if Rapport≥3): OnChoice transition to access situation. Room key granted in RewardTemplate (immediate).
- **Money choice** (Pay 5 coins): OnChoice transition to access situation. Room key granted in RewardTemplate (immediate).
- **Challenge choice** (Haggle): OnSuccess transition to access situation, OnFailure transition to fallback situation. Room key granted in OnSuccessReward (conditional on winning negotiation challenge).
- **Fallback choice** (Leave): OnChoice transition to scene end. No rewards. Scene completes without service.

This creates strategic depth. Players with high rapport get free room. Players with money can buy directly. Players with neither must risk the challenge. Fallback prevents soft-locking while not rewarding abandonment.

**Property-Driven Generation:**
- NPC.Personality determines service type (Innkeeper→Lodging, Attendant→Bathing, Priest→Healing)
- NPC.Demeanor sets challenge difficulty (Friendly=-1, Hostile=+1)
- Location.Settlement influences base cost (Urban cheaper than Wilderness)
- Spot.Comfort sets service benefit (Basic/Standard/Premium restoration multiplier)
- Spot.PrivacyLevel determines access item protection (Private rooms need secure keys)

**Reward Enrichment**: SceneArchetypeCatalog receives generic negotiation choices from SituationArchetypeCatalog and enriches them:
- Stat-gated/money choices: RewardTemplate populated with room key and location unlock
- Challenge choice: OnSuccessReward populated with room key and location unlock
- Fallback choice: No rewards (left empty)

**Mechanical Guarantee:**
- Key granted only through successful negotiation (stat/money/challenge success)
- Key consumed Situation 2 (never orphaned)
- Spot locked initially, unlocked Situation 2, re-locked Situation 4 (state cycle)
- Resources consumed Situation 3, never before (payment happens first)
- Time advances Situation 3, situations adjust for morning context
- Access item lifecycle complete (grant → consume → remove)

**Example Instances:**
- Elena (Innkeeper, Friendly) + Tavern (Lodging) + Room (Standard) → easy challenge, affordable rest, warm tone
- Marcus (Attendant, Professional) + Bathhouse (Bathing) + Bath (Premium) → moderate challenge, luxury bath, formal tone
- Thalia (Priest, Compassionate) + Temple (Healing) + Chamber (Standard) → low challenge, healing rest, gentle tone

#### 2. Transaction Sequence

**Purpose:** NPC operates shop where player browses inventory, selects item, completes purchase transaction.

**Structure:**
- Situation 1: Browse shop inventory (list available items with costs). Player selects desired item.
- Situation 2: Negotiate price via challenge or straight purchase. Difficulty based on item value.
- Situation 3: Complete transaction. Remove coins, grant item. Update shop stock.

**Property-Driven Generation:**
- NPC.Personality=Merchant (required archetype filter)
- NPC.Demeanor affects negotiation difficulty (Greedy=+2, Fair=0, Generous=-1)
- Location.Settlement influences stock (Urban has more variety, Wilderness has basics)
- Shop.Specialization determines inventory type (Weapons, Provisions, General Goods)
- Item.Value sets base transaction cost

**Mechanical Guarantee:**
- Inventory state consistent (item removed from stock after purchase)
- Coin check before transaction (insufficient funds prevents purchase)
- Item granted only after successful payment
- Stock replenishment timer starts after purchase
- Multiple purchases possible (loop back to Situation 1 until player exits)

**Example Instances:**
- Garrick (Merchant, Fair) + General Store (Urban) → balanced prices, varied stock
- Helga (Merchant, Greedy) + Provisions (Wilderness) → higher prices, survival goods only
- Aldric (Merchant, Generous) + Weapon Shop (Urban) → slight discount, quality weapons

#### 3. Gatekeeper Sequence  

**Purpose:** NPC guards checkpoint/border requiring credentials or payment for passage.

**Structure:**
- Situation 1: Approach checkpoint. Gatekeeper states requirements (toll/papers/mission).
- Situation 2: Present credentials or negotiate passage. Challenge or payment.
- Situation 3: Pass through checkpoint. Unlock new location region or route.

**Property-Driven Generation:**
- NPC.Personality=Guard/Official (archetype filter)
- NPC.Demeanor sets negotiation difficulty (Strict=+2, Reasonable=0, Lenient=-1)
- Location.BorderSignificance affects passage cost (Major city gate expensive, rural checkpoint cheap)
- Checkpoint.SecurityLevel determines credential requirements (High needs papers, Low needs payment)

**Mechanical Guarantee:**
- Credentials checked before passage attempt
- Payment collected before route unlock
- Route/region unlock happens atomically with success
- Failed negotiation allows retry with different approach
- Successful passage grants "PassedCheckpoint_Location" tag for future reference

**Example Instances:**
- Captain Rodrik (Guard, Strict) + North Gate (High Security) → difficult passage, expensive toll
- Corporal Mills (Guard, Reasonable) + Trade Road (Medium Security) → moderate challenge, fair toll
- Scout Vessa (Guard, Lenient) + Forest Path (Low Security) → easy passage, low cost

#### 4. Consequence Reflection

**Purpose:** Player processes aftermath of significant event (battle, loss, discovery) with NPC guidance or solo reflection.

**Structure:**
- Situation 1: Encounter consequence scenario (wounded companion, destroyed location, revelation).
- Situation 2: Process emotionally. NPC provides guidance or player reflects solo. Resource cost (time/focus).
- Situation 3: Resolve forward. Accept consequence, grant tags for progression milestone.

**Property-Driven Generation:**
- NPC.Personality=Mentor/Friend/Confidant if NPC present (can be solo without NPC)
- ConsequenceType determines narrative framing (Loss, Victory, Discovery, Betrayal)
- Player.EmotionalState influences reflection difficulty (Shaken=+1, Composed=0, Resolved=-1)
- SignificanceLevel sets tag value (Minor grants regional tag, Major grants campaign tag)

**Mechanical Guarantee:**
- Consequence tied to prior scene completion (prerequisite tag)
- Resource cost prevents immediate continuation (forces pause)
- Tag grants unlock related future content
- No loop-back (one-way progression sequence)
- Narrative closure via AI-generated reflection text

**Example Instances:**
- Elder Miriam (Mentor, Wise) + Loss of Companion → processing grief, grants "Bereaved" tag
- Solo Reflection + Discovery of Artifact → realizing significance, grants "KnowsArtifactPurpose" tag
- Comrade Finn (Friend, Loyal) + Victory over Threat → celebrating success, grants "VeteranOfBattle" tag

#### 5. Inn Crisis Escalation

**Purpose:** Tutorial-specific branching emergency teaching consequence system. Starts with minor issue, escalates based on player choices, demonstrates permanent world changes.

**Structure:**
- Situation 1: Initial crisis (kitchen fire). 3 choices with different consequence paths.
- Situation 2 (Branch A): Controlled resolution if player acts decisively. Minor cost, minor consequence.
- Situation 2 (Branch B): Worsening crisis if player hesitates. Medium cost, medium consequence.
- Situation 2 (Branch C): Catastrophic escalation if player ignores. High cost, permanent damage.
- Situation 3: Aftermath processing. Different narrative/tags based on branch taken.

**Property-Driven Generation:**
- CrisisType determines initial scenario (Fire, Theft, Accident, Conflict)
- EscalationSpeed sets time pressure (Fast forces quick decision, Slow allows deliberation)
- Location.Importance determines consequence severity (Important location = worse damage)
- ConsequenceScale sets permanent world changes (Minor = NPC mood change, Major = location damaged)

**Mechanical Guarantee:**
- Branch paths mutually exclusive (taking Branch A prevents B and C)
- Consequences persist (damaged location remains damaged)
- Different tags granted per branch (showing divergent progression)
- Tutorial-only archetype (not used in procedural spawning)
- Teaches player: choices matter, hesitation has cost, world state changes permanently

**Example Instance:**
- Elena's Tavern Kitchen Fire: 
  - Choice 1 (Act): Douse fire with water barrel → controlled resolution, minor coin cost, grants "ActedDecisively" tag
  - Choice 2 (Delay): Fetch help from cellar → fire spreads, medium damage to kitchen, grants "Cautious" tag
  - Choice 3 (Ignore): Leave to someone else → tavern heavily damaged, Elena injured, grants "Negligent" tag, tavern state changes to "Damaged"

This archetype deliberately structured to teach consequences through branching rather than binary success/fail.

#### 6. Single Negotiation

**Purpose:** Standalone diplomatic interaction. One decision with 4-choice pattern. Used for merchant haggling, favor requests, information trading.

**Structure:**
- Situation 1: Negotiation with stat-gated, money, challenge, and fallback choices. Scene completes after choice.

**Property-Driven Generation:**
- NPC.Personality=Mercantile/Diplomatic (archetype filter)
- NPC.Demeanor affects requirements (Friendly lowers stat threshold, Greedy raises coin cost)
- Location.Settlement influences negotiation context (Urban = formal, Wilderness = pragmatic)
- Rewards specified by scene template (item grants, reputation changes, information unlocks)

**Scene Identity**: Uses `negotiation` situation archetype. Thematically diplomatic, trade-based, non-violent, rapport-focused. Distinct from confrontation or investigation.

**Example Instances:**
- Merchant haggle over rare item → challenge success grants discount
- Diplomat requesting favor → high rapport grants immediate agreement
- Information broker selling secrets → payment or persuasion unlocks knowledge

#### 7. Single Confrontation

**Purpose:** Standalone authority interaction. One decision with 4-choice pattern. Used for guard checkpoints, territorial disputes, intimidation.

**Structure:**
- Situation 1: Confrontation with stat-gated, money, challenge, and fallback choices. Scene completes after choice.

**Property-Driven Generation:**
- NPC.Personality=Guard/Authority (archetype filter)
- NPC.Demeanor affects difficulty (Strict increases challenge, Lenient decreases)
- Location.SecurityLevel influences requirements (High security needs credentials or force)
- Rewards: passage permissions, reputation impacts, escalation prevention

**Scene Identity**: Uses `confrontation` situation archetype. Thematically authoritative, dominance-based, potentially violent, power-focused. Distinct from negotiation.

**Example Instances:**
- Gate guard blocking passage → show papers or intimidate
- Territorial warlord demanding tribute → pay or fight
- Bouncer refusing entry → bribe or force way through

#### 8. Single Investigation

**Purpose:** Standalone analytical interaction. One decision with 4-choice pattern. Used for clue gathering, puzzle solving, research.

**Structure:**
- Situation 1: Investigation with stat-gated, money, challenge, and fallback choices. Scene completes after choice.

**Property-Driven Generation:**
- NPC.Personality=Scholar/Informant (archetype filter) OR solo investigation (no NPC)
- Location.Context provides environmental clues (Library = research, Crime Scene = forensics)
- Player.InsightStat determines stat-gated threshold
- Rewards: knowledge tags, investigation progress, artifact discoveries

**Scene Identity**: Uses `investigation` situation archetype. Thematically analytical, discovery-based, puzzle-solving, insight-focused. Distinct from negotiation or confrontation.

**Example Instances:**
- Library research on ancient artifact → high insight or coin for research access
- Crime scene examination → challenge to deduce clues
- Sage consultation about prophecy → payment or rapport unlocks wisdom

#### 9. Single Social Maneuvering

**Purpose:** Standalone subtle influence interaction. One decision with 4-choice pattern. Used for court intrigue, social manipulation, reputation management.

**Structure:**
- Situation 1: Social maneuvering with stat-gated, money, challenge, and fallback choices. Scene completes after choice.

**Property-Driven Generation:**
- NPC.Personality=Noble/Politician (archetype filter)
- Location.SocialTier influences complexity (High court = complex, Tavern = simple)
- Player.CharmStat determines stat-gated threshold
- Rewards: faction standing, political favors, social reputation

**Scene Identity**: Uses `social_maneuvering` situation archetype. Thematically subtle, influence-based, reputation-driven. Used when direct negotiation or confrontation inappropriate.

**Example Instances:**
- Court favor seeking → charm or gift secures ally
- Political alliance forming → challenge navigates competing interests
- Reputation damage control → payment or performance repairs standing

#### 10. Single Crisis

**Purpose:** Standalone emergency response. One decision with 4-choice pattern. Used for immediate danger, urgent decisions, split-second choices.

**Structure:**
- Situation 1: Crisis with stat-gated, money, challenge, and fallback choices. Scene completes after choice. Often has time pressure or consequence severity.

**Property-Driven Generation:**
- CrisisType determines urgency (Fire = immediate, Illness = gradual)
- Location.Importance affects consequence severity (Critical location = worse outcomes)
- Player.ReflexStat often used for stat-gated options
- Rewards: crisis mitigation, reputation changes, state preservation

**Scene Identity**: Uses `crisis` situation archetype. Thematically urgent, high-stakes, consequence-heavy. Time pressure distinguishes from other patterns.

**Example Instances:**
- Burning building with trapped child → act decisively or get help
- Ambush on caravan → fight or negotiate under pressure
- Poisoning attempt → antidote purchase or medical skill application

#### 11. Single Service Transaction

**Purpose:** Standalone service purchase. One decision with 4-choice pattern. Used for healers, bathhouses, stables when location access not required.

**Structure:**
- Situation 1: Service transaction with stat-gated, money, challenge, and fallback choices. Service effect applied immediately. Scene completes after choice.

**Property-Driven Generation:**
- NPC.Personality=Healer/Attendant (archetype filter)
- ServiceType determines effect (Healing = restore health, Bathing = restore stamina)
- ServiceTier affects cost and benefit (Basic cheap/weak, Premium expensive/strong)
- Rewards: resource restoration, status effect removal, temporary buffs

**Scene Identity**: Uses `service_transaction` situation archetype. Thematically economic, benefit-focused, immediate effect. Used when private location access not needed (vs service_with_location_access for inn rooms).

**Example Instances:**
- Healer treating wounds → payment or rapport for healing
- Stable boarding horse → coin cost for mount care
- Street vendor selling hot meal → quick stamina restoration

### Archetype as Complete Identity

A scene archetype IS the complete definition of structure and content, not a template accepting parameters. The archetype name encodes:
- Exact situation count and ordering
- Which situation archetypes used for each position
- Transition pattern connecting situations
- Dependent resources created (if any)
- Thematic identity and intended usage

`single_negotiation` and `single_confrontation` are distinct scene archetypes, not configuration variants of generic "standalone." They have different thematic identities, different situation archetypes, different contextual appropriateness.

**Why Not Parameterize**: Making archetype selection a parameter moves design decisions from code to data. This violates design/configuration separation. If JSON specifies "which situation archetype to use," content authors make structural choices that belong in catalogues. The correct solution is more specific archetypes, not flexible parameterization.

**Catalogue Composition**: SceneArchetypeCatalog defines scene structure. For single-situation archetypes, it calls SituationArchetypeCatalog.GetArchetype() with specific situation archetype, wraps result in scene structure (1 situation, Standalone transition pattern), returns SceneArchetypeDefinition. The scene archetype name determines which situation archetype gets used.

### Multi-Scene NPC Display
- Can grant tags for future content spawn
- No resource creation (uses existing locations/NPCs)

**Example Instances:**
- Ask Directions: Traveler NPC + 3 choices (persuade/pay/charm) → grants "KnowsRoute" tag
- Observe Ritual: Witness event + 2 choices (intervene/watch) → grants "SeenRitual" tag  
- Quick Trade: Merchant NPC + 3 choices (offer item/coin/service) → immediate transaction

#### 6. Inn Crisis Escalation

**Purpose:** Tutorial-specific branching emergency teaching consequence system. Starts with minor issue, escalates based on player choices, demonstrates permanent world changes.

**Structure:**
- Situation 1: Initial crisis (kitchen fire). 3 choices with different consequence paths.
- Situation 2 (Branch A): Controlled resolution if player acts decisively. Minor cost, minor consequence.
- Situation 2 (Branch B): Worsening crisis if player hesitates. Medium cost, medium consequence.
- Situation 2 (Branch C): Catastrophic escalation if player ignores. High cost, permanent damage.
- Situation 3: Aftermath processing. Different narrative/tags based on branch taken.

**Property-Driven Generation:**
- CrisisType determines initial scenario (Fire, Theft, Accident, Conflict)
- EscalationSpeed sets time pressure (Fast forces quick decision, Slow allows deliberation)
- Location.Importance determines consequence severity (Important location = worse damage)
- ConsequenceScale sets permanent world changes (Minor = NPC mood change, Major = location damaged)

**Mechanical Guarantee:**
- Branch paths mutually exclusive (taking Branch A prevents B and C)
- Consequences persist (damaged location remains damaged)
- Different tags granted per branch (showing divergent progression)
- Tutorial-only archetype (not used in procedural spawning)
- Teaches player: choices matter, hesitation has cost, world state changes permanently

**Example Instance:**
- Elena's Tavern Kitchen Fire: 
  - Choice 1 (Act): Douse fire with water barrel → controlled resolution, minor coin cost, grants "ActedDecisively" tag
  - Choice 2 (Delay): Fetch help from cellar → fire spreads, medium damage to kitchen, grants "Cautious" tag
  - Choice 3 (Ignore): Leave to someone else → tavern heavily damaged, Elena injured, grants "Negligent" tag, tavern state changes to "Damaged"

This archetype deliberately structured to teach consequences through branching rather than binary success/fail.

### Archetype Selection Logic

SceneSpawnService matches archetype to context using eligibility filters.

**Tutorial Scenes:** Explicitly authored templates with concrete entity IDs. Tutorial "Secure Lodging" always uses service_with_location_access archetype + Elena + Tavern + Room. Same mechanical structure every playthrough. Narrative may vary (AI-generated text), mechanics identical.

**Procedural Scenes:** Templates with entity filters. "Find Lodging" template specifies: service_with_location_access archetype + {NPC.Personality=Innkeeper, Location.Services=Lodging}. SpawnService queries current location for matching entities. Multiple matches possible (city with 3 inns) - selects highest priority or player choice.

**Archetype Constraints:** Some archetypes require specific entity configurations. Transaction_sequence requires NPC.Personality=Merchant + Shop inventory. Gatekeeper_sequence requires Location.Checkpoint=true. Service_with_location_access requires Spot.AccessControl=Locked. Template validation ensures requirements met.

**Priority System:** When multiple archetypes eligible at same context, priority determines spawn order. High priority: consequence_reflection (aftermath matters most). Medium priority: gatekeeper_sequence (checkpoints block progress). Low priority: single_situation (quick encounters less critical). Player never overwhelmed with simultaneous spawns.

### Dynamic Location Creation Details

Scenes requiring new locations (private rooms, special chambers, temporary camps) use multi-facade pipeline.

**SceneInstanceFacade.RequestLocation(spec):** Scene needs location during spawn. Creates LocationCreationSpec value object with: id pattern ("{sceneId}_location"), name pattern ("Private Room at {npcLocation}"), hexCoordinate (adjacent to parent location), properties (settlement type, danger level, services). Passes spec to GameFacade.CreateDynamicLocation(spec).

**GameFacade.CreateDynamicLocation(spec):** Orchestrates pipeline. Calls ContentGenerationFacade.CreateDynamicLocationFile(spec). Receives DynamicFileManifest on success. Calls PackageLoaderFacade.LoadDynamicContent(manifest.filepath). Receives LocationCreationResult with final locationId. Returns locationId to SceneInstanceFacade. Scene stores locationId reference.

**ContentGenerationFacade.CreateDynamicLocationFile(spec):** Transforms spec into JSON structure matching foundation.json format. Generates unique filename (scene_{sceneId}_location_{index}.json). Writes to /dynamic-content/locations/ directory. Returns DynamicFileManifest with filepath, timestamp, checksum. No knowledge of scenes or entities - pure JSON generation.

**PackageLoaderFacade.LoadDynamicContent(filepath):** Reads JSON file using same parser as static content loading. Creates Location entity with all properties. Adds to GameWorld.Locations. Integrates with hex grid system at specified coordinates. Spatial systems automatically generate intra-venue travel routes. Returns LocationCreationResult with final locationId or error enum (HexOccupied, DuplicateId, InvalidPlacement, ParseError).

**Scene Lifecycle Integration:** Scene spawns Provisional state until location creation completes. LocationId stored as nullable. Pipeline completes → locationId assigned → scene transitions Active. If pipeline fails → scene aborts → rollback all created resources → log failure diagnostic. No partial states.

**Cleanup Process:** Scene completion or abandon triggers GameFacade.RemoveDynamicLocation(locationId, manifest). ContentGenerationFacade deletes JSON file. PackageLoaderFacade removes Location entity from GameWorld. Hex grid updates automatically. Routes regenerated. Clean removal with no orphaned references.

**Type Safety:** All pipeline stages use strongly-typed value objects. LocationCreationSpec (input), DynamicFileManifest (creation proof), LocationCreationResult (output), CleanupResult (removal proof). Compiler enforces valid pipelines. No dictionary baggage, no generic types, no string-keyed data.

**Atomic Guarantees:** Location creation succeeds completely or fails cleanly. Pipeline failure at any stage triggers rollback: JSON created but parse failed → delete JSON. Location created but hex occupied → remove location and delete JSON. Situation references location but creation aborted → scene cleanup removes situation. Never partial state.

**Static vs Dynamic Content:** Static locations loaded at game start from /content/locations/. Dynamic locations created on-demand during scenes. Both types indistinguishable after creation - same Location entity type, same GameWorld.Locations collection, same routing system integration. Only difference: lifecycle (static permanent, dynamic temporary) tracked via DynamicContentManifest.

### Dependent Location Lifecycle

Dependent locations don't exist until scenes create them. They follow specific lifecycle with spatial placement requirements.

**Pre-Existence**: Before scene spawns, dependent location does NOT exist in GameWorld.Locations. Not hidden or disabled - literally doesn't exist.

**Creation at Scene Finalization**:
1. Scene finalizes, SceneInstantiator generates DependentLocationSpec (name pattern, properties, HexPlacementStrategy)
2. BuildLocationDTO calls FindAdjacentHex(baseLocation, strategy) to determine hex position
3. FindAdjacentHex queries baseLocation.HexPosition, gets neighboring hexes from HexMap, finds first unoccupied
4. DTO populated with concrete values: placeholders replaced, Q and R coordinates set
5. ContentGenerationFacade wraps DTO in JSON package
6. PackageLoaderFacade parses JSON, LocationParser creates Location entity with HexPosition from DTO
7. Location added to GameWorld.Locations, LocationActionCatalog generates intra-venue movement actions

**Persistence**: Once created, location persists after scene completes. May become locked again (IsLocked=true), but doesn't disappear. Enables returning to previously unlocked locations.

**Hex Placement Validation**: FindAdjacentHex throws exception if no unoccupied adjacent hex exists. Enforces spatial constraint: venues have maximum 7 locations (center + 6 neighbors). Scene cannot spawn if venue cluster full.

**Intra-Venue Movement**: After creation, player sees "Move to [Room Name]" action in LocationAction list (if unlocked). Action uses standard intra-venue movement system - instant and free because hexes adjacent. No special navigation handling needed.

### Location Locking System

Locations have lifecycle state beyond existence. IsLocked property represents whether location is accessible for player navigation. Distinct from visibility (locked locations appear on map) and routing (locked locations have movement actions generated).

**Location Entity State**: Location owns locked state through IsLocked boolean (HIGHLANDER: one owner, no duplicate tracking). Defaults false (open-world assumption), explicitly set true for locked locations.

**Parser Translation**: LocationParser translates InitialState string from DTO to IsLocked boolean during parsing (Parser-JSON-Entity Triangle completion). When InitialState equals "Locked", parser sets IsLocked to true. Translation happens once at parse time - runtime uses concrete boolean, never string matching.

**Dependent Location Locking**: Scene archetypes specify IsLockedInitially flag on dependent locations. SceneInstantiator sets InitialState="Locked" in LocationDTO during generation. Parser translates to IsLocked=true during package loading. Dependent locations integrate as locked, inaccessible until unlocked.

**Choice Rewards Unlock**: ChoiceReward has LocationsToUnlock and LocationsToLock lists containing location IDs. RewardApplicationService iterates lists, finds Location entities, directly modifies IsLocked properties. Player choices unlock locations through typed reward application.

**Movement Action Availability**: LocationActionCatalog generates intra-venue movement actions for all adjacent hex locations regardless of lock state. LocationActionManager queries Location.IsLocked at availability check, sets LocationAction.IsAvailable to false for locked destinations. Separates action existence (spatial relationships) from action executability (lock state).

**UI Rendering**: Landing.razor conditionally adds "locked" CSS class to action cards where IsAvailable is false. Renders lock icons with reason text. Visual feedback communicates lock state.

**Player Agency**: When location unlocks, player does NOT automatically move there. System unlocks access, player manually chooses to navigate. Preserves exploration mental model.

### Hex Grid Integration for Dynamic Locations

WorldHexGrid provides spatial indexing using two-level lookup: Hexes list containing all Hex objects, _hexLookup dictionary mapping AxialCoordinates to hexes. Both must be updated when dependent locations spawn.

**Bidirectional Sync Pattern**: Hex grid maintains invariant that Location.HexPosition and Hex.LocationId stay synchronized through two complementary operations:
- **SyncLocationHexPositions** (hex→location): Initial world load reads Hex.LocationId from hex grid, sets Location.HexPosition. Hex is source of truth during initial load.
- **EnsureHexGridCompleteness** (location→hex): Runtime location creation reads Location.HexPosition, updates Hex.LocationId. Location is source of truth for runtime modifications.

One-way data flow per operation prevents circular dependencies. Initial load uses hex grid as spatial definition. Runtime creation uses locations as spatial source.

**Dynamic Location Integration**: When location with HexPosition loads through dynamic package loading:
1. Query hexMap.GetHex(coordinates) to find hex at location's position
2. If hex exists with null or different LocationId, update Hex.LocationId to reference location
3. If hex doesn't exist at coordinates, create new Hex with appropriate terrain/danger for settlement
4. Rebuild _hexLookup dictionary after modifications (cache invalidation)

Maintains invariant: every location with HexPosition has corresponding hex in grid at exact coordinates. Updates existing hexes rather than always creating new ones (wilderness hexes exist without locations initially).

**Timing**: Hex completeness check occurs in PackageLoader after location loading but before action generation. Sequence: load locations → SyncLocationHexPositions (initial) → EnsureHexGridCompleteness (runtime) → generate actions → create routes. Integration boundary where dependent resources join world state.

**Navigation Dependency**: GetPlayerCurrentLocation queries WorldHexGrid.GetHex with player coordinates. Lookup must succeed for navigation. Without hex at coordinates, lookup returns null causing NullReferenceException when code accesses null.Venue. Hex grid is spatial index for ALL locations, not optional or parallel system.

**Architectural Consistency**: Dynamic locations use same hex grid systems as static locations. No special navigation paths, no bypass logic, no "virtual locations." One spatial index, one navigation system (HIGHLANDER principle for spatial systems).

**Integration Pattern**: Any system creating locations dynamically follows pattern: create DTO with coordinates, parse through standard pipeline, add to hex grid if positioned, generate actions. Dependent resources leverage same infrastructure as authored content.

### Value Objects for Pipeline Communication

**LocationCreationSpec:** Input specification for new location.
- IdPattern: string template with variable resolution ("{sceneId}_room")
- NamePattern: string template with context variables ("{npcName}'s Private Room")
- HexCoordinate: Coordinate value object (q, r, region)
- Properties: LocationProperties value object (settlement, danger, services, etc)
- ParentLocationId: nullable string reference if location child of another
- AccessControl: enum (Open, Restricted, Locked) for initial state

**DynamicFileManifest:** Proof of file creation.
- Filepath: absolute path to created JSON file
- CreatedTimestamp: when file written
- Checksum: content hash for verification
- ResourceType: enum (Location, Item, Spot) for cleanup categorization
- SceneId: which scene created this resource

**LocationCreationResult:** Output after entity creation.
- Success: bool indicating complete success
- LocationId: assigned GameWorld ID if success
- ErrorReason: enum (HexOccupied, DuplicateId, InvalidPlacement, ParseError) if failure
- DiagnosticMessage: detailed error explanation for debugging

**CleanupResult:** Outcome of resource removal.
- ResourcesRemoved: int count of successfully deleted resources
- FilesDeleted: int count of JSON files removed
- EntitiesRemoved: List<string> of GameWorld entity IDs removed
- FailedRemovals: List<string> of resources that failed removal with reasons
- CleanupTimestamp: when cleanup completed

All value objects immutable, passed by value, no lifecycle. Serve as contracts between facades. Compiler enforces valid pipeline stages (can't pass CleanupResult to CreateLocation). No casting, no type checking, no runtime validation needed.

### Scene Template Spawning Workflow

Player reaches new context (enters city, meets NPC, completes prerequisite scene). System evaluates spawn eligibility.

**Step 1: Eligibility Query**
SceneSpawnService.EvaluateProceduralScenes(player) iterates all ProceduralSceneTemplates. For each template:
- Check player.Tags contains all template.RequiredTags
- Check template.ArchetypeConflict not already active (no duplicate lodging scenes)
- Check template target entity filters match current context (NPC.Personality=Innkeeper present?)
- Return SpawnEligibility result with: Eligible=true/false, Reason="player missing tag" / "archetype conflict" / "no matching entities" / "eligible"

**Step 2: Priority Selection**
Eligible templates sorted by Priority value. Higher priority spawns first. If multiple same priority, spawn order: consequence (aftermath urgent) > gatekeeper (blocks progress) > service (repeatable) > single_situation (minor).

**Step 3: Entity Resolution**
Highest priority template has entity filters: {NPC.Personality=Innkeeper, Location.Services=Lodging}. SpawnService queries current location GameWorld.NPCs where condition matches. Finds "marcus_npc". Queries GameWorld.Locations where coordinates match and services contain Lodging. Finds "Golden Rest Inn". Bundles entities as TargetEntities parameter.

**Step 4: Generation**
SceneSpawnService.SpawnScene(template, targetEntities, player). Calls SceneArchetypeCatalog.Generate(template.Archetype, targetEntities). Generator queries entity properties, applies formulas, returns GeneratedSceneStructure with situations/choices/effects.

**Step 5: Resource Creation (if needed)**
SceneInstanceFacade checks generated structure for dynamic resource requests (new location for private room). Calls GameFacade.CreateDynamicLocation(spec). Pipeline: ContentGenerationFacade creates JSON → PackageLoaderFacade creates entity → locationId returned. Scene stores locationId in PlacedLocation reference.

**Step 6: Instantiation**
SceneInstanceFacade.CreateSceneInstance(generatedStructure, finalEntityReferences). Creates Scene domain entity. Creates Situation entities. Creates Choice entities. Links situations sequentially. Adds to GameWorld.Scenes. Scene state = Provisional until resources finalize.

**Step 7: Finalization**
Pipeline completes successfully. All dynamic locations created, all items granted, all entity references validated. Scene state transitions Provisional → Dormant. Scene now exists in GameWorld, waiting for player to reach PlacedLocation.

**Step 8: Activation**
Player navigates to scene.PlacedLocation. LocationViewBuilder queries scenes at current location. Finds scene. Calls scene.ShouldActivateAtContext(currentLocationId, relevantNpcId). Returns true. Activates scene (Dormant → Active). Activates current situation (Dormant → Active). Instantiates choices as UI buttons. Player can now interact.

### Scene Resumption After Navigation

Player in middle of multi-situation scene. Navigates elsewhere (leaves location). Scene must pause and resume cleanly.

**Pause Trigger:** Player navigation away from scene.PlacedLocation. LocationChangeHandler queries active scenes at previous location. For each scene where State=Active: call Scene.Pause(). Transitions State Active → Paused. CurrentSituation transitions Active → Paused. Choices de-instantiate (no longer interactable). Scene remains in GameWorld.Scenes.

**State Preservation:** Paused scene maintains: current situation index, granted items, consumed resources, time elapsed, choice history. All state intact, just not actively interactable.

**Resume Trigger:** Player returns to scene.PlacedLocation. LocationViewBuilder queries scenes at location. Finds paused scene. Calls scene.ShouldActivateAtContext(currentLocationId, relevantNpcId). Returns true (player at correct place). Calls Scene.Resume(). Transitions State Paused → Active. CurrentSituation transitions Paused → Active. Choices re-instantiate with preserved state. Player continues exactly where left off.

**Multi-Location Scenes:** Some scenes span multiple locations (lodging: negotiate at lobby, rest at room). Scene tracks multiple PlacedLocations. Activation logic: if player at ANY PlacedLocation AND current situation relevant to that location, activate. Example: negotiate situation activates at lobby, rest situation activates at room. Player navigating lobby→room automatically transitions situations without pause/resume.

**Timeout Handling (Future):** Long-term paused scenes (player ignored for days) may have timeout logic. After 24 real-world hours paused, scene auto-abandons with cleanup. Prevents infinite accumulation of paused scenes. Not implemented yet - current implementation has no timeout.

### Multiple Scenes at Same NPC

Player can have multiple concurrent scenes with same NPC. Common case: ongoing lodging scene + new conversation scene + shopping scene all with same innkeeper.

**UI Display:** LocationViewBuilder.BuildNPCInteractions(npc) queries GameWorld.Scenes.Where(scene => scene.PlacedNpc.Id == npc.Id && scene.HasActiveOrSelectableSituations()). Returns list of all scenes at this NPC.

**Button Rendering:** Each scene becomes separate interaction button. Button label priority: (1) scene.DisplayName if set, (2) scene.CurrentSituation.Name, (3) fallback "Talk to [NPC Name]". Player sees: "Continue Lodging", "Ask About Local News", "Browse Shop Inventory" as three distinct buttons for same NPC.

**Navigation Routing:** Button click includes (npcId, sceneId) tuple. SceneNavigationHandler.NavigateToScene(npcId, sceneId) looks up specific scene directly. No ambiguity even with multiple scenes at same NPC.

**Scene Isolation:** Scenes don't interact. Progress in lodging scene doesn't affect shopping scene. Each tracks independent state. Only interaction: shared NPC relationship changes (completing major scene might change NPC.Demeanor for future spawns).

**Conflict Prevention:** Some archetypes prevent concurrent spawns. Can't have two lodging scenes at same NPC (already resting). Can't have two gatekeeper scenes at same checkpoint (already negotiating passage). Conflict check during spawn: if existing scene same archetype + same target NPC/location, reject new spawn.

**Priority Ordering:** When rendering multiple scene buttons, sort by: (1) scene with current active situation (in-progress scenes first), (2) scene most recently interacted with (resume preferred), (3) scene spawned first (older scenes priority). First scene gets primary UI position, others available via "More Interactions" expansion.

### Entity Property Extension System (Future Consideration)

Current design: entities have fixed property sets (NPC has Personality/Demeanor/etc, Location has Settlement/Danger/etc). Works for current archetypes. Future archetypes may need additional properties.

**Problem:** New archetype "Romantic Dinner" needs NPC.AttractionLevel property. Adding to NPC base class forces all NPCs to have this property even if irrelevant. Bloats entity with unused properties.

**Solution (not yet implemented):** Optional property extension system. Entity base class has fixed core properties. Optional extension properties stored in Dictionary<PropertyType, object> if needed. Generator queries TryGetProperty<AttractionLevel>() returns nullable. If present, uses value. If absent, uses default or skips generation aspect.

**Trade-off:** Violates strong typing preference (dictionary usage). Adds complexity (property presence checking). Enables flexible archetype evolution without entity class modification. NOT current design, future consideration if archetype proliferation demands it.

Current design: author explicitly enumerates needed properties on entity classes. Archetype uses only properties entity has. New archetype requiring new property means modifying entity class to add property. Keeps entity structure explicit and typed. No hidden properties, no runtime discovery, no dictionary baggage.

### Scene Completion Flow Detail

Player executes final choice in final situation. Scene transitions to Completed state and applies granted tags.

**Final Choice Execution:** SceneInstanceFacade.ExecuteChoice(finalChoice). Applies choice effects: remove consumed items, modify resources, record outcome. Increments situation index. Checks: moresituationsRemaining? No. Calls CompleteScene(scene).

**Tag Granting:** SceneInstanceFacade.CompleteScene(scene) queries scene.GrantedTags. For each tag: add to player.Tags if not present. Tags formatted as "verb_subject_context" ("EstablishedContact_Innkeepers_Vallenmarch"). Future scene spawns now match these tags for prerequisite checks.

**Resource Cleanup:** If scene created dynamic resources (locations/items/spots), calls GameFacade.CleanupSceneResources(scene.DynamicResourceManifest). ContentGenerationFacade deletes JSON files. PackageLoaderFacade removes entities from GameWorld. Clean removal.

**State Archive:** Scene transitions State Active → Completed. Removed from GameWorld.Scenes active list. Added to GameWorld.CompletedScenes archive list. Preserved for debugging/replay, but no longer interactable.

**Progression Unblock:** Completing scene with granted tags enables new scene spawns. "CompletedLodging_ElenasTavern" tag granted. Future scene requiring "lodging experience" now eligible. Progression tree advances via tag accumulation, not hardcoded ID chains.

**Respawn Prevention:** Completed scenes don't respawn at same entities. Template tracks CompletedContexts list (entity ID + location ID combinations already spawned). New spawn checks: if (entityId, locationId) in CompletedContexts, skip spawn. Prevents: secure lodging at Elena's tavern every time player visits.

**One-Time vs Repeatable:** Some scenes explicitly repeatable (shopping transaction can happen multiple times). Template property Repeatable=true allows respawn at same context. Default Repeatable=false (most scenes one-time experiences). Archetype determines repeatability: transaction_sequence repeatable, consequence_reflection one-time.

### Scene Abandonment Flow

Player chooses to abandon scene mid-progression. Scene removed without tag grants or completion credit.

**Abandon Trigger:** Player clicks "Leave" / "Cancel" button during scene. UI confirms: "Abandon this interaction? Progress will be lost." Player confirms. Calls SceneInstanceFacade.AbandonScene(scene).

**No Tag Grants:** Scene doesn't apply GrantedTags. Player gains no progression credit. Tags only granted on complete success, not partial progress.

**Resource Cleanup:** If scene created dynamic resources, cleanup pipeline executes. Locations removed, items removed, spots removed. No orphaned content in GameWorld.

**State Removal:** Scene transitions State Active → Abandoned. Removed from GameWorld.Scenes. NOT archived in CompletedScenes (didn't complete). Scene instance garbage collected.

**Respawn Eligibility:** Abandoned scene doesn't mark context as completed. Same template can respawn at same entities. Player can retry later. No penalty beyond lost time/resources already invested.

**Narrative Consistency:** If scene had state-changing effects already applied (item granted in situation 1, player abandons at situation 2), effects remain. Item not revoked on abandon. World state changes persist. Only future effects prevented. Reinforces consequence: partial engagement has partial effect.

### World State Changes from Scenes

Scenes modify world state through effects. Changes persist after scene completion.

**Location State:** Scene can change Location.State (Safe → Dangerous after crisis). Scene can add/remove Location.AvailableServices (Inn loses Lodging service after fire damage). State changes persist permanently. Future scenes at that location query new state, generate appropriately.

**NPC Relationship:** Scene can modify NPC.Demeanor (Neutral → Friendly after positive interaction). Scene can add/remove NPC.KnownFacts (player learned NPC secret). Relationship changes affect future scene generation. Friendly NPC generates easier challenges. Known facts unlock new conversation topics.

**Item Possession:** Scene grants items to player inventory. Items persist after scene. Items enable future scene access (room key enables private room scenes). Items consumed in other scenes. Full item lifecycle from grant to use to removal.

**Time Advancement:** Scene can advance world time (rest scene advances 6 segments to morning). Time change affects: time-of-day dependent scenes, scheduled events, NPC availability. Time flows consistently across all systems.

**Tag Accumulation:** Scene grants tags to player. Tags are permanent accumulation (never removed except explicit rare cases). Tags unlock future content via prerequisite matching. Tags represent player progression narrative (places visited, people met, milestones achieved).

**Dynamic Content Lifecycle:** Scene creates temporary locations/spots during execution. Content exists while scene active. Content cleaned up on scene completion/abandon. Temporary, scene-specific locations don't persist beyond scene lifecycle.

### Tag System Details

Tags enable dependency-inverted progression. Scenes grant/require tags instead of hardcoded ID references.

**Tag Format:** "verb_subject_context" structure. Examples: "EstablishedContact_Innkeepers_Vallenmarch", "DefeatedBandit_ForestRoad", "LearnedSecret_ElderMiriam", "CompletedLodging_ElenasTavern". Three part structure enables precise or flexible matching.

**Flexible Matching:** Template can require exact tag or pattern match. RequiredTags: ["EstablishedContact_Innkeepers_*"] matches any innkeeper contact in any region. RequiredTags: ["EstablishedContact_*_Vallenmarch"] matches any contact type in Vallenmarch. Enables general prerequisites without hardcoded specifics.

**Multiple Requirement Logic:** Template can specify multiple RequiredTags with AND logic. Requires ["EstablishedContact_Innkeepers_Vallenmarch", "CompletedTutorial_BasicCombat"] means player must have both tags. Enables complex prerequisite graphs.

**Tag Semantics:**
- "EstablishedContact" tags: player met NPC type in region, can spawn social scenes
- "Completed" tags: player finished specific scene, prevents respawn, enables next scenes
- "Learned" tags: player gained knowledge, unlocks investigation progress
- "Defeated" tags: player overcame challenge, enables victory consequence scenes
- "Discovered" tags: player found location/secret, spawns related content

**Tag Granting Timing:** Tags granted on scene completion only, never mid-scene. Ensures atomic progression (either scene fully completes granting tags, or abandons granting nothing). Prevents partial progression states.

**Tag Persistence:** Tags never removed (except explicit rare design cases). Accumulate throughout playthrough. Represent permanent player history. Enable content to spawn based on player's unique progression path rather than linear sequence.

**Debug Visualization:** TagDebugView shows all player tags with: tag string, when granted (scene ID + timestamp), what scenes now eligible due to this tag. Enables tracing progression chains: "player completed X granting tag Y enabling scenes Z1, Z2, Z3".

### Economic Balance Through Formulas

All resource costs calculated via consistent formulas. Prevents economic broken-ness.

**Base Cost Formula:** `Cost = ArchetypeBase + (DifficultyModifier * DifficultyScalar) + LocationPremium`. Service lodging base=10 coins. Difficulty +2 (premium room). Scalar=2. Location premium +3 (luxury inn). Total=10 + 4 + 3 = 17 coins.

**Restoration Formula:** `RestoreAmount = BaseRestore * ComfortMultiplier * DangerMultiplier`. Rest stamina base=80. Comfort premium=1.5x. Danger safe=1.0x. Total=80 * 1.5 * 1.0 = 120 stamina restored.

**Time Cost Formula:** `TimeCost = BaseTime * ServiceTier * UrgencyMultiplier`. Rest base=6 segments. Service standard=1.0x. Urgency none=1.0x. Total=6 segments. If urgent=2.0x, total=12 segments (rushing takes longer, worse quality).

**Item Value Formula:** `ItemWorth = BaseMaterial + CraftQualityBonus + Enchantment`. Simple key base=1. Quality standard+2. No enchant+0. Total=3 coin value. Lost key means replacing=3 coin cost.

**Difficulty Scaling:** Difficulty modifier ranges -2 (very easy) to +4 (very hard). Each point adds (DifficultyScalar * modifier) to cost. Ensures harder challenges cost more resources, easier challenges cost less. Player makes strategic choice: attempt hard challenge with high cost, or find easier alternative.

**Balance Validation:** Generation time, formulas verified to produce positive values. No negative costs, no zero costs (except free interactions). No overflow (max difficulty capped preventing astronomical costs). Comprehensive unit tests verify formula correctness across all difficulty ranges and property combinations.

### AI Context Bundling (Future Implementation)

When situation activates, AI needs context to generate appropriate narrative.

**Context Bundle Structure:**
```
NarrativeContext {
  Entities: {
    NPC: { Id, Name, Personality, Demeanor, RegionalAffiliation, KnownFacts }
    Location: { Id, Name, Settlement, DangerLevel, Services, State }
    Spot: { Id, Name, AccessControl, PrivacyLevel, Comfort } (if relevant)
  }
  SituationMeta: {
    Position: int (1 of 4)
    SequenceRole: enum (Opening, Middle, Climax, Resolution)
    PrecedingOutcome: string (what happened in previous situation)
  }
  PlayerState: {
    TimeOfDay: enum (Morning, Afternoon, Evening, Night)
    Resources: { Health, Stamina, Focus, Coins }
    HeldItems: List<ItemId>
    Conditions: List<string> (Wounded, Tired, Energized, etc)
  }
  NarrativeHints: List<string> (derived from properties)
}
```

**Hint Derivation:** Generator queries entity properties, produces narrative hints:
- NPC.Demeanor=Friendly → ["warm greeting", "helpful tone", "casual conversation"]
- Location.Settlement=Wilderness → ["isolated atmosphere", "natural sounds", "rustic setting"]
- Spot.Comfort=Luxury → ["plush furnishings", "elegant decor", "attention to detail"]
- Player.Conditions contains "Wounded" → ["pain evident", "injury limiting", "need rest"]

**AI Generation Call:** Send NarrativeContext to AI service. AI returns:
```
GeneratedNarrative {
  SituationDescription: string (2-3 sentences setting scene)
  ChoiceTexts: List<string> (one per choice, 1 sentence action)
  OutcomeNarratives: Dictionary<ChoiceId, string> (2-3 sentences consequence)
}
```

**Storage:** Store GeneratedNarrative directly on Situation entity properties. Description, choice action texts, outcome texts persist for playthrough. Player sees same narrative on resume (doesn't regenerate).

**Sequential Coherence:** Situation 2 bundle includes Situation 1 outcome. AI sees "player persuaded innkeeper, was given key warmly" and generates Situation 2 narrative acknowledging this. Situation 3 sees both previous outcomes. Natural continuity emerges from context bundling, no separate memory system needed.

**Regeneration on Replay:** New game spawns same template at same entities. Generation produces same mechanical structure. AI generates new narrative (different sentences, same meaning). Tutorial "Secure Lodging" mechanically identical every playthrough, narratively varied.

### GameFacade Orchestration Pattern

GameFacade coordinates multi-system operations without implementing domain logic. Acts as composition root for complex workflows.

**Resource Creation Pipeline Orchestration:**
```csharp
LocationCreationResult CreateDynamicLocation(LocationCreationSpec spec) {
  // Step 1: Create JSON file
  DynamicFileManifest manifest = contentGenerationFacade.CreateDynamicLocationFile(spec);
  if (!manifest.Success) {
    return LocationCreationResult.Failure(manifest.ErrorReason);
  }
  
  // Step 2: Parse and create entity
  LocationCreationResult result = packageLoaderFacade.LoadDynamicContent(manifest.Filepath);
  if (!result.Success) {
    contentGenerationFacade.DeleteFile(manifest.Filepath); // Rollback
    return result;
  }
  
  // Step 3: Return success with final ID
  return LocationCreationResult.Success(result.LocationId);
}
```

**Facade Isolation:** GameFacade only orchestrates. ContentGenerationFacade only creates files. PackageLoaderFacade only parses and instantiates. Each facade has single responsibility. No facade imports other facades. Only GameFacade imports all facades for coordination.

**Transaction Guarantees:** Pipeline fails at any step → rollback previous steps. File created but parse failed → delete file. Entity created but validation failed → remove entity and delete file. Atomic all-or-nothing operations.

**Type-Safe Contracts:** All parameters strongly typed value objects. LocationCreationSpec (input), DynamicFileManifest (stage 1 output), LocationCreationResult (final output). Compiler enforces valid pipeline sequences. Can't pass wrong object to wrong method.

**No Business Logic in Facade:** GameFacade doesn't know what a scene is, doesn't understand game rules, doesn't make strategic decisions. Only coordinates: "create this file, then parse that file, then return this ID". SceneInstanceFacade makes decisions (what location needed, when to create). GameFacade just executes.

### Resource Cleanup on Scene End

Scenes may create temporary resources. Cleanup must remove all traces.

**Cleanup Trigger:** Scene completion or abandonment calls GameFacade.CleanupSceneResources(scene.DynamicResourceManifest).

**Manifest Structure:**
```csharp
DynamicResourceManifest {
  SceneId: string
  CreatedResources: List<DynamicResource> {
    ResourceType: enum (Location, Item, Spot)
    ResourceId: string (GameWorld entity ID)
    Filepath: string (JSON file path)
    CreatedTimestamp: DateTime
  }
}
```

**Cleanup Process:**
1. For each resource in manifest:
2. Query GameWorld for entity by ResourceId
3. Remove entity from appropriate collection (Locations, Items, Spots)
4. Call ContentGenerationFacade.DeleteFile(resource.Filepath)
5. Update spatial systems (hex grid if location, item references if item)
6. Record cleanup timestamp

**Validation:** Verify entity actually removed before marking cleanup complete. If entity removal fails, log diagnostic but continue (don't abort cleanup of other resources). CleanupResult reports successful/failed removals.

**Orphan Prevention:** If scene aborts mid-spawn (player crashes game during scene creation), next game load checks for orphaned dynamic resources. Query: files in /dynamic-content/locations/ with no corresponding active scene. Delete orphans. Prevents accumulation of dead content files.

**Clean Slate Guarantee:** After cleanup, no evidence of temporary resources remains. GameWorld entity collections contain only permanent content and active dynamic content. File system contains only active resource JSON. No orphans, no dead references, no memory leaks.

### Pipeline Error Handling

Multi-stage pipelines need explicit error handling at each stage.

**Explicit Error Enums:** No generic Exception throwing. Each facade returns typed result with error enum.

**ContentGenerationFacade Errors:**
- FileSystemFailure (disk full, permissions issue)
- InvalidSpecification (spec violates JSON constraints)
- DuplicateFilepath (file already exists)
Each error has diagnostic message with fix guidance.

**PackageLoaderFacade Errors:**
- ParseError (malformed JSON, missing required fields)
- HexOccupied (coordinate already has location)
- DuplicateId (entity ID conflicts with existing)
- InvalidPlacement (location violates spatial rules)
Each error includes entity ID and specific constraint violated.

**SceneInstanceFacade Errors:**
- ResourceCreationFailed (pipeline stage failed)
- InvalidEntityReference (target NPC doesn't exist)
- ArchetypeConflict (duplicate scene type at context)
- RequirementNotMet (player missing prerequisite tag)
Each error references scene template ID and specific requirement.

**Error Propagation:** Errors propagate up call stack without suppression. Facade returns error result → GameFacade receives error result → SceneSpawnService receives error result → logs diagnostic, abandons spawn. No catching exceptions and continuing with partial state.

**Diagnostic Logging:** Each error logged with full context: which facade failed, what operation attempted, what entity involved, what constraint violated, what expected vs actual values were. Enables rapid debugging without needing reproducible scenario.

**No Fallbacks:** Errors don't trigger "use default" or "skip this part" logic. Error = operation failed, abort cleanly. LET IT CRASH philosophy: surface problems immediately rather than hiding them with workarounds.

### Scene Generation Parse-Time Validation

All template validation happens when content loads, not when scenes spawn.

**Template Structure Validation:**
- Archetype ID exists in SceneArchetypeCatalog
- Entity references (if concrete IDs) exist in GameWorld
- Entity filters (if categorical) use valid properties
- RequiredTags match tag format regex "verb_subject_context"
- GrantedTags match tag format regex
- Priority is positive integer
- Archetype matches entity requirements (transaction_sequence requires Merchant NPC)

**Template Semantic Validation:**
- RequiredTags reference actual game progression (no made-up tags)
- GrantedTags don't conflict with existing tags (no ambiguous tags)
- Entity filters matchable (properties exist on entity type)
- Target entity types compatible with archetype (service requires Location with spot access)

**Cross-Template Validation:**
- No duplicate template IDs
- No circular tag dependencies (Template A requires tag from Template B requires tag from Template A)
- No unreachable templates (requires tags never granted by any template)
- Progression graph has valid entry points (some templates require no tags for initial spawning)

**Archetype Structure Validation (per archetype):**
- Situations ordered sequentially (1, 2, 3, 4 no gaps)
- Each situation has at least one choice
- Each choice has at least one effect
- Item effects properly sequenced (grant before consume)
- Location/spot references complete (spot has parent location)
- Formulas use valid entity properties
- Formula results guaranteed positive (no division by zero, no negative costs)

**Validation Error Reporting:**
Format: "Template '{templateId}' failed validation: {specificError}. Expected {expected}, found {actual}. Fix by {actionableGuidance}."

Example: "Template 'secure_lodging' failed validation: Target NPC filter references non-existent property 'Charisma'. Expected one of: Personality, Demeanor, RegionalAffiliation. Fix by changing filter to use valid property or adding property to NPC entity class."

**Fail-Fast on Invalid Content:** Content load aborts if any template invalid. Game doesn't launch with broken templates. Console shows validation report listing all errors. Developer must fix content before game runs. No fallbacks, no partial loads, no "skip invalid templates".

At finalization when scene transitions Provisional to Active, situations remain dormant until player reaches matching context. Scene references all resources by final GameWorld IDs. Resources guaranteed to exist because pipeline completed successfully before instantiation.

Separation of concerns: SceneInstantiator creates scene domain entities. GameFacade orchestrates resource creation pipeline. ContentGenerationFacade creates JSON files. PackageLoaderFacade creates Location entities. Clean boundaries.

### ContentGenerationFacade

Transforms strongly-typed resource specifications into JSON files matching static content structure.

**CreateDynamicLocationFile(spec):** Receives LocationCreationSpec value object. Transforms spec fields into JSON structure matching foundation.json format: id from pattern, name from pattern with context resolution, hexCoordinate from spec with embedded Q and R values, properties object with typed fields. 

**Hex Coordinate Embedding**: Unlike static locations (hex positions from separate hex grid file), dynamic locations have Q and R coordinates embedded directly in JSON. BuildLocationDTO calls FindAdjacentHex to determine placement, stores coordinates in DTO properties, JSON generation includes these values. This enables PackageLoader to create spatially-positioned locations without requiring hex grid sync.

Writes JSON to /dynamic-content/locations/ directory with generated filename scene_{sceneId}_location_{index}.json. Returns DynamicFileManifest on success (filepath, timestamp, checksum) or throws FileSystemException on failure. Never imports GameWorld, SceneGenerationFacade, or PackageLoaderFacade. Only knows JSON structure and file operations.

**RemoveDynamicLocation(locationId, manifest):** Receives location ID and its creation manifest. Validates location is dynamic (in manifest). Removes from GameWorld.Locations collection. Deletes JSON file at manifest filepath. Updates DynamicContentManifest removing entry. Returns RemovalResult indicating success. Handles cleanup phase of resource lifecycle.

**GetDynamicContentManifest():** Returns current manifest listing all dynamically-created content files with metadata. Used for world reset and debugging.

Facade responsibility: JSON generation and file management only. Does not parse, does not create GameWorld entities, does not understand scenes or situations. Pure data transformation and file I/O.

### PackageLoaderFacade

Parses JSON content files and creates GameWorld entities. Treats dynamic content identically to static content except for hex coordinate source.

**LoadDynamicContent(filepath):** Receives filepath to JSON file. Parses JSON using identical parser logic as static foundation.json loading. Extracts location data, creates Location entity with all properties. 

**Hex Position Assignment**: LocationParser checks if DTO has Q and R values (embedded coordinates from dynamic generation). If present, creates AxialCoordinates and assigns to Location.HexPosition. If absent (static locations), hex position comes from HexParser.SyncLocationHexPositions later. Both paths converge to same HexPosition property.

Adds to GameWorld.Locations collection. Integrates with hex grid at specified coordinates. Spatial systems automatically generate intra-venue travel routes based on hex adjacency. Returns LocationCreationResult on success (locationId) or failure (errorReason enum: HexOccupied, DuplicateId, InvalidPlacement, ParseError). Never imports SceneGenerationFacade or ContentGenerationFacade. Only knows parsing and entity creation.

**LoadStaticContent(filepath):** Existing method for static content. Uses same parsing logic as LoadDynamicContent. No special dynamic handling - both paths identical after JSON parse.

Facade responsibility: JSON parsing and entity instantiation only. Does not generate JSON, does not create files, does not understand scenes. Pure parsing and GameWorld integration.

Dynamic and static content converge at this boundary: both become Location entities in GameWorld.Locations with HexPosition set, indistinguishable after creation except for lifecycle (static permanent, dynamic temporary).

### Critical Architectural Constraints

The architecture enforces specific constraints preventing entire classes of bugs and design problems.

**NO Mutable Catalogues**: Catalogues generate immutable structures and return them. Never modify objects received from other catalogues. SceneArchetypeCatalog receives choices from SituationArchetypeCatalog - it creates NEW ChoiceTemplate instances with enriched properties, never mutates the original choices. This preserves the parse-time contract and prevents hidden coupling.

**NO Dictionary or HashSet Types**: Dictionary and HashSet types forbidden entirely. Data lives directly on entities, not in separate lookup structures. Need to map situation IDs to outcomes? Store LastChoiceId on Situation itself. This eliminates desync bugs and enforces HIGHLANDER principle (one concept, one representation).

**NO Extension Methods**: Extension methods hide domain logic outside entities. All entity behavior lives directly on the entity class as regular methods. If Situation needs outcome tracking, methods go on Situation class, not as extensions. This keeps behavior co-located with data.

**LET IT CRASH Philosophy**: No null coalescing or defensive checks. Initialize collections inline on entities. Missing data means bug - crash with descriptive error. Don't mask problems with fallback values. If RewardTemplate.LocationsToUnlock is null, that's initialization bug to fix, not runtime condition to handle. Fail-fast surfaces issues immediately instead of allowing corrupted state to propagate.

**Parser-JSON-Entity Triangle Integrity**: JSON field names must match C# property names exactly. No JsonPropertyName attributes to translate. If property is "terrain", JSON field is "terrain". If adding JsonPropertyName, you're violating the triangle. This reduces translation layers and makes data path obvious.

**Catalogue Pattern Parse-Time Boundary**: Catalogues live in Content/Catalogues folder and are NEVER imported by runtime code (facades, managers, services). They execute at parse time generating templates. Runtime code queries GameWorld collections populated at parse time. Calling catalogue from facade violates timing model.

**Single Source of Truth**: When multiple properties could represent same data, choose ONE. Either LifecycleStatus enum OR IsCompleted boolean, not both. The enum is truth, boolean is computed property derived from it. This eliminates desync bugs and clarifies ownership.

**HIGHLANDER Principle**: One concept, one representation. Multiple ways to represent same thing = choose ONE and use consistently everywhere. Object reference pattern: BOTH ID + resolved object (Pattern A) OR just ID (Pattern C) OR just object (Pattern B) - pick one pattern, never mix. Transition evaluation: evaluate conditions OR return first match, not both depending on call site. Consistency prevents architectural drift.

**HIGHLANDER at Field Level**: Scene archetype identification requires exactly ONE field: `sceneArchetypeId`. No alternative fields for special cases. Never `sceneArchetypeId` OR `situationArchetypeId` depending on archetype type. The presence of alternative identification fields signals architecture confusion. If different scene types need different identification mechanisms, the archetype system is wrong. Fix catalogues, not JSON schema.

**HIGHLANDER for Context Building**: SceneSpawnContextBuilder is the SOLE implementation of context building logic. Never duplicate context resolution in facades, services, or tests. Any code needing SceneSpawnContext calls the shared utility. This prevents six duplicate implementations with inconsistent NPC→Location resolution behavior. Context building is orchestration-level logic (requires GameWorld queries) centralized in static utility accessible everywhere.

**HIGHLANDER Pattern D (Template + Instance IDs)**: Entities spawned from templates at runtime store BOTH TemplateId (references source template) AND Id (unique instance identifier). Template ID enables matching against template-defined rules. Instance ID ensures uniqueness when same template spawns multiple times. Never compute one from other at query time. Situation uses Pattern D: transitions match on TemplateId (template-defined rules), state tracking uses Id (instance uniqueness). Consistency prevents desync bugs.

**Why Parameterization Fails**: Making archetype selection a parameter (e.g., "standalone" archetype taking `situationArchetypeId` parameter) moves design decisions to configuration space. This creates:
- Configuration creep (structure choices in JSON)
- Catalogue bypass (parser calls SituationArchetypeCatalog directly)
- Identity loss ("standalone" meaningless without parameter)
- Impossible archetype-specific logic (catalogue doesn't know which situation archetype will be used)

Correct solution: More specific archetypes (`single_negotiation`, `single_confrontation`), not flexible parameterization.

**All Locations Have Hex Positions**: Every location occupies exactly one hex on the world grid. No exceptions. "Intra-venue instant travel" describes movement behavior (free/instant) not location structure (no hex position). Movement is instant BECAUSE hexes adjacent, not INSTEAD OF having positions. Dependent locations from scenes get hex coordinates embedded in DTO. Static locations get hex coordinates from separate hex grid sync. Both paths converge to Location.HexPosition property. Location without hex position cannot participate in movement system - architectural violation.

**Action Generation Atomicity**: LocationActions must generate atomically with location addition. When location enters GameWorld.Locations (whether from JSON parse or dynamic generation), actions must exist before method returns. No gaps where locations exist but lack accessibility. PackageLoader.LoadDynamicPackageFromJson calls GenerateLocationActionsFromCatalogue after LoadPackageContent. This pattern mandatory for any future dynamic content loading. Regeneration affects all locations (adjacency relationships change when new locations added), duplicate detection makes this safe.

**Parse-Time Object References Preferred**: When entity references exist as both parse-time objects and runtime IDs, prefer parse-time objects. NPC.Location (object reference from locationId) over WorkLocationId (runtime string). Parse-time resolution runs once (cheaper), uses object references (safer), enables direct navigation (simpler). Runtime string-based resolution only when parse-time insufficient (dynamic state, non-existent entities at parse time).

**Parser-JSON-Entity Triangle Completeness**: Every categorical property in JSON must translate to concrete typed property on domain entity during parsing. InitialState string translates to IsLocked boolean. Terrain string translates to TerrainType enum. Runtime code never does string matching, dictionary lookups, or catalogue calls. Ensures type safety, eliminates runtime parsing overhead, makes domain model self-contained.

**Hex Grid as Universal Spatial Index**: All locations with HexPosition must have corresponding Hex objects in WorldHexGrid at exact coordinates. Grid is not optional, not parallel to another system, not only for authored content. Dynamic locations integrate into same hex grid as static locations. Navigation, adjacency queries, route validation all depend on hex grid completeness. Without hex at coordinates, spatial queries fail with null reference crashes.

### AI Generation Service

Context bundling per situation: collect NPC/Location/Spot entity objects, situation position indicator, service type, player state, items held. Package as typed context.

Generation call per situation activation: AI receives context and narrative hints, returns situation description + all choice action texts + outcome narratives. Store directly on Situation properties.

Sequential generation: Situation 2 generates with updated context (situation 1 outcome, possibly advanced time). Situation 3 generates with further updates. Emergent coherent narrative from context-aware sequential generation.

### LocationViewBuilder / UI Layer

Queries active scenes at current location to build interaction options.

**Multi-Scene Query Pattern:** Calls GameWorld.Scenes.Where(scene => scene.PlacedLocation.Id == currentLocationId && scene.HasActiveOrSelectableSituations()). Returns ALL scenes with content at this location, not just first one.

**NPC Interaction Display:** For each NPC present at location, queries all active scenes where PlacedNpc matches. Builds list of scene interaction options. Each scene becomes separate button with derived label (DisplayName > CurrentSituation.Name > "Talk to [NPC Name]" placeholder).

**Context-Based Situation Activation:** For each scene at location, calls Scene.ShouldActivateAtContext(currentLocationId, relevantNpcId). If returns true and situation currently dormant, activates situation (transitions to Active, instantiates actions). Enables scene resumption after navigation.

**Navigation Routing:** Button actions include both npcId and sceneId. Click handler routes to specific scene via direct lookup. Eliminates ambiguity when multiple scenes active at same NPC.

**Physical Presence vs Interactions:** Always displays NPC cards when physically present. Conditionally renders interaction buttons only when scenes exist. Maintains fiction (NPC physically there) while showing only available gameplay content.

---

This architecture supports procedural generation of complete multi-situation narrative arcs while maintaining mechanical coherence and economic balance. Scene archetypes define reusable arc patterns. AI provides contextual narrative for each beat. Game logic orchestrates progression through situations with state modification, item lifecycle, and time advancement. Templates contain pure structure, instances track runtime state, entities own their lifecycle behavior.

**Dynamic location creation enables true scene self-containment with spatial integrity.** Scenes don't depend on pre-authored locations beyond base. GameFacade orchestrates multi-system pipeline: ContentGenerationFacade produces JSON files with embedded hex coordinates, PackageLoaderFacade creates Location entities with HexPosition from DTO, SceneInstanceFacade references final IDs. FindAdjacentHex determines hex placement using HexPlacementStrategy (SameVenue, Adjacent). All locations occupy exactly one hex with corresponding Hex object in WorldHexGrid - no exceptions. Parser translates InitialState string to IsLocked boolean (Parser-JSON-Entity Triangle). Dynamic locations integrate as locked, unlock through choice rewards, enable manual navigation preserving player agency. Static and dynamic locations indistinguishable after creation. Complete lifecycle from specification to JSON to spatially-positioned, lockable entity to cleanup.

**Two-level archetype system enables procedural variation within designed constraints.** Scene archetypes define structure (how many situations, which situation archetypes, what transitions). Situation archetypes define interaction patterns (4-choice structure with stat/money/challenge/fallback). Catalogues compose these layers at parse time. Scene archetype identity IS its structure - `single_negotiation` and `single_confrontation` are distinct archetypes, not configuration variants. No parameterization - more specific archetypes solve flexibility needs.

**Scene state machine with template-based transition matching.** Scenes progress through situations using SituationTransition objects with condition evaluation. Transitions defined at template authoring time using template IDs. Runtime situations have both Id (unique instance) and TemplateId (source template reference) - HIGHLANDER Pattern D. Transition matching uses TemplateId to connect template-defined rules to runtime instances. When GetTransitionForCompletedSituation finds matching transition, scene advances to next situation. When no matching transition found, scene marks Completed and despawns. Using instance Id instead of TemplateId causes matching failure, premature scene completion, soft-lock bugs.

**Hex grid provides spatial foundation for all locations.** Every location occupies one hex using axial coordinates (Q, R). Venues are 7-hex clusters (center + 6 adjacent). Intra-venue movement between adjacent hexes instant and free. Inter-venue movement between non-adjacent hexes uses routes with resource costs. Location.HexPosition is core identity enabling movement system participation. WorldHexGrid.Hexes and _hexLookup dictionary updated through bidirectional sync pattern: SyncLocationHexPositions (hex→location) for initial load, EnsureHexGridCompleteness (location→hex) for runtime creation. Navigation depends on hex grid completeness - missing hex at coordinates causes null reference crashes.

**Location locking system enables dynamic world expansion with access control.** Locations own IsLocked state (HIGHLANDER: one owner). Parser translates categorical InitialState to concrete IsLocked boolean. Dependent locations spawn locked. Choice rewards modify IsLocked directly through LocationsToUnlock/LocationsToLock lists. Action generation creates movement actions for all adjacent locations regardless of lock state. Availability checks query IsLocked at action execution time. UI renders locked state visually. Players unlock locations through scene choices, manually navigate to unlocked areas maintaining agency.

**Design vs configuration separation maintains architectural clarity.** Design (structure, patterns, content) lives exclusively in catalogue code. Configuration (placement, timing, eligibility) lives exclusively in JSON data. Never blur this boundary. If JSON authors need structural choices, catalogues are incomplete. PlacementFilter determines WHERE scenes spawn. SpawnConditions determine WHEN scenes become eligible. Neither affects WHAT scenes contain. SceneSpawnContextBuilder resolves placement into fully populated context using parse-time object references (HIGHLANDER: single shared implementation).

**LocationAction generation ensures dynamic content accessibility.** Property-based actions (Crossroads → inter-venue travel) gate features. Hex-based intra-venue actions enable spatial navigation between adjacent locations. GenerateLocationActionsFromCatalogue called after loading any content (static or dynamic). Regeneration affects all locations because adjacency relationships change. Action generation atomic with location addition - no gaps where locations exist but lack accessibility.

**Parse-time generation eliminates runtime performance cost.** All catalogue calls occur at game initialization. Templates immutable after parse. Runtime queries pre-generated collections. Facades never import catalogues. This timing separation is sacred - breaking it introduces performance problems and architectural confusion. Context building operates at runtime (entity resolution from GameWorld) using orchestration-level utility, distinct from parse-time catalogue pattern. LocationActionCatalog generates actions at initialization and after dynamic content loads, not continuously.

**Reliability is paramount.** Pure generation core with zero dependencies enables isolated testing. Parse-time validation prevents structural errors from reaching runtime. Parser-JSON-Entity Triangle completeness ensures all categorical properties translate to concrete typed properties. Explicit contracts and state machines provide complete traceability. Comprehensive automated test suite (1000+ unit, 100+ integration, 20+ end-to-end tests) catches regressions immediately. Debug visualization tools enable rapid diagnosis. Atomic pipeline guarantees complete success or clean failure. LET IT CRASH philosophy surfaces problems immediately rather than masking with fallbacks. SceneSpawnContextBuilder fails fast with null returns when entity resolution fails, forcing data quality fixes. Action generation validates all locations have hex positions. Hex grid integration prevents spatial system violations. This system architecture ensures rock-solid procedural content generation where failure is not an option.