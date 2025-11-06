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

---

## Implementation Status

The codebase implements the core property-driven generation philosophy while making pragmatic improvements to the original design. Implementation choices often prove superior to initial architectural proposals.

**Critical Success Factor:** Scene generation system operates as isolated, thoroughly tested subsystem. Pure generation core with zero game dependencies enables independent testing. Parse-time validation prevents invalid templates from reaching runtime. Comprehensive test suite (unit, integration, end-to-end) provides confidence in mechanical correctness. See System Robustness Architecture section for complete implementation strategy ensuring bulletproof reliability.

### Working Scene Archetypes (6 Total)

**SceneArchetypeCatalog.cs** generates complete multi-situation structures at parse time using property queries. Archetypes: `service_with_location_access` (lodging/bathing/healing), `transaction_sequence` (shopping), `gatekeeper_sequence` (checkpoints), `consequence_reflection` (aftermath), `single_situation` (quick interactions), `inn_crisis_escalation` (branching emergencies). All property-driven (query NPC.Demeanor, Location.Services, etc).

### Multi-Scene NPC Display

UI layer supports multiple concurrent scenes at single NPC. Query pattern changed from FirstOrDefault (single scene) to Where (all scenes). Each active scene renders as separate interaction button. Navigation routing includes (npcId, sceneId) tuple for direct scene lookup. Physical NPC presence always visible, interaction buttons conditional on scene availability.

### Tag-Based Scene Spawning

Dependency inversion pattern eliminates scene-ID dependencies. Scenes grant abstract tags through choice rewards (LodgingExperienceAcquired, TravelersGuildMember, PatronEstablished). Spawn conditions evaluate player tags, not completed scene IDs. Multiple scenes can grant same tag (tutorial lodging OR procedural lodging both unlock "experienced" tag). Sir Brante pattern: "CONDITIONS MET" displays which tags unlocked scene. No brittle references, complete flexibility.

### Superior Implementation Decisions

**Choice-Level Time Costs:** Implementation uses ChoiceCost.TimeSegments instead of situation-level costs. Superior because different approaches to same situation cost different time (bribe guard = instant, persuade = 2 segments, fight = 4 segments). More flexible than uniform situation timing proposed in design.

**Manual Cleanup:** Implementation uses explicit ItemsToRemove and LocationsToLock in departure situations instead of automatic Scene.ExecuteCleanup(). Superior because cleanup is visible in content authoring, debuggable, and author-controlled. Automatic tracking was hidden magic prone to silent failures.

**Direct Location Access:** Implementation has Location.IsLocked directly, no separate LocationSpot entity. Superior because player accesses Location, period. Design's Location/Spot hierarchy was unnecessary complexity (spots within locations within venues = three-level nesting for no benefit).

**Null Entity Handling:** Implementation gracefully handles categorical scenes without concrete entities at parse time. GenerateContextualHints provides generic hints when entities null, concrete hints when present. Enables procedural spawning without parse-time entity binding. Design assumed entities always present - implementation proved more flexible pattern needed.

### Domain Entity State Machine

Scene.AdvanceToNextSituation() and Scene.IsComplete() methods exist. Domain entity owns progression logic. Services orchestrate calls to entity methods, not scattered state manipulation.

### AI Integration Points Ready

SceneNarrativeService.cs architecture complete with context bundling (ScenePromptContext), NarrativeHints structure, integration point in SceneInstantiator.FinalizeScene(). AI calls currently stubbed returning fallback narrative. Proven async pattern exists in Social system's AINarrativeProvider for reference implementation.

### Template-Instance Separation

SituationTemplates embedded in SceneTemplate (composition). Runtime Situations in flat GameWorld.Situations list. Scene.SituationIds references only (HIGHLANDER). Immutable templates, mutable game state.

### Two-Entity Placement

PlacementFilter evaluates NPC (personality, bond, tags) and Location (services, properties, tags). Selection strategies: Closest, HighestBond, LeastRecent, WeightedRandom. Dual-mode: concrete binding (tutorial) + categorical filtering (procedural).

### Item Lifecycle & Time System

Player.Inventory with weight capacity. ChoiceReward.ItemIds grants items, ItemsToRemove cleans up. HasItem requirement gates content. GameWorld.CurrentDay/CurrentTimeBlock single source. ChoiceCost.TimeSegments, ChoiceReward time advancement. GameFacade.ProcessTimeAdvancement() HIGHLANDER sync point. Scene.ExpiresOnDay for time-limited content.

---

## Architectural Discovery

### Scene Scope Understanding

Scenes are NOT single conversations with 2-4 choices. Scenes are complete multi-situation narrative arcs with:

**Sequential Situation Progression:** Multiple situations flowing in defined order. Negotiation situation completes, spawns access situation. Access situation completes, spawns service situation. Service situation completes, spawns departure situation. Scene progression rules define this flow (linear, branching, hub-and-spoke, conditional).

**Context-Aware Progression:** Situations auto-advance seamlessly ONLY when consecutive situations share the same location/NPC context. When next situation requires different location or NPC interaction, player exits to world, navigates, and scene resumes at new context. Scene.CurrentSituationId persists across navigation - player can complete Situation 1, travel for days, return, and Situation 2 activates when context matches.

**Example Flow (Secure Lodging):** Situation 1 at Common Room completes → player exits to world → player navigates to Upper Floor → Situation 2 activates automatically → Situation 3 auto-advances seamlessly (same location) → player exits to world → player returns to Common Room → Situation 4 activates. Situations 2→3 cascade without interruption (shared context). Situations 1→2 and 3→4 require navigation (context change).

**Location State Modification:** Scenes change world state. Location.IsLocked toggles (locked becomes unlocked becomes locked again). NPC availability changes (merchant closes shop, guard changes post). Environmental properties shift (time of day advances, weather changes, danger levels update).

**Item Lifecycle Management:** Scenes grant items (room key, ticket, permit) that exist temporarily within scene scope. Item granted in situation 1, consumed in situation 2, removed in situation 4 via explicit cleanup. Items don't persist beyond scene unless intentionally permanent rewards.

**Resource Flow Orchestration:** Scenes consume resources (coins, time, energy) and produce benefits (restored stamina, gained knowledge, increased stats). Economic balance maintained across entire arc. Early situations cost resources, later situations provide benefits, net outcome justified by total cost.

**World State Causality:** Actions in one situation cause consequences in later situations. Paying coins in negotiation situation affects what happens in service situation (paid upfront versus pay later). Time advancement in rest situation triggers morning-specific narrative in departure situation. Prior choices ripple forward through arc.

**Explicit Cleanup Design:** Departure situations specify cleanup through authored rewards (ItemsToRemove, LocationsToLock). Author controls what gets restored, making cleanup visible and debuggable. More reliable than hidden automatic tracking.

### Scene as Self-Contained Package

Scenes define and create their own dependent resources rather than referencing pre-existing world entities. This eliminates broken references and ensures scenes control complete lifecycle of resources they introduce.

**Minimal External Dependencies:** Scene templates specify only essential external requirements through placement filters. Service archetype requires: NPC matching personality filter (Innkeeper/Priest/Attendant), base location matching service filter (Lodging/Healing/Bathing). These are selection criteria, not hardcoded references.

**Created Dependencies at Spawn:** When scene instantiates, it creates dependent resources from specifications in template. Service scenes create: private location adjacent to base location on hex grid (upper room, private chamber, secluded sanctuary), access item unique to this scene instance (room key, entry token, sacred seal). Created resources exist solely for this scene instance.

**Dynamic Location Creation Pipeline:** Scenes don't reference pre-existing locations beyond base. Instead, scene generation produces strongly-typed LocationCreationSpec objects describing locations to create. GameFacade orchestrates multi-system pipeline: ContentGenerationFacade transforms specs into JSON files matching foundation.json structure in /dynamic-content directory, PackageLoaderFacade parses JSON identically to static content creating Location entities in GameWorld, SceneInstanceFacade receives final location IDs for scene tracking. Facades never reference each other - only GameFacade orchestrates. Pipeline atomic: all locations created before scene instantiation proceeds, or entire spawn fails.

**Hex Grid Integration:** Dependent locations specify relative placement (adjacent to base location, specific distance, directional preference). GameFacade queries hex grid for valid placement coordinates, passes concrete coordinates to ContentGenerationFacade. Created locations added to hex grid at specified coordinates. Existing intra-venue travel logic automatically generates movement routes between adjacent locations. No special dynamic location handling needed. Player navigates between static and dynamic locations identically.

**Resource Naming and Identity:** Created resources use pattern-based naming incorporating context. Upper room becomes "Elena's Private Room" when spawned at Elena's inn, "Marcus's Bath Chamber" at Marcus's bathhouse. Item becomes "room_key_scene_{sceneId}" ensuring uniqueness per instance. Names derived from placement context, not pre-authored.

**Strongly-Typed Specifications:** Scene templates contain LocationCreationSpec objects (never dictionaries or generic objects). Each spec declares: NamePattern string, PlacementRule enum (Adjacent/Distance/Direction), HexOffsetFromBase coordinates, Properties object with typed fields (IsLocked boolean, Atmosphere enum, Services enum list), IntegrationStrategy enum (Permanent/Temporary/Reusable). Item specs similarly strongly-typed. No var, no dynamic, no object. Compiler verifies all specifications.

**Lifecycle Ownership:** Scene tracks all resources it created. Scene entity maintains lists: created location IDs, created item IDs. Scene owns complete lifecycle: creation at spawn, modification during progression, cleanup at completion. No orphaned resources when scene completes.

**Cleanup Strategies:** Template defines cleanup behavior per dependent resource. Location specifications declare IntegrationStrategy: Permanent (location permanently locked after scene, remains on grid), TemporaryRemove (location deleted from GameWorld and hex grid, JSON file removed from dynamic-content directory), Reusable (location restored to initial state for future scene spawns). Items typically scene-scoped (removed at completion) or permanent rewards (persist in player inventory). Dynamic content manifest tracks all created files enabling systematic cleanup.

**Static-Dynamic Equivalence:** After creation, dynamically-created locations behave identically to statically-authored locations. Same Location entity type, same properties, same query patterns. No code checking "is this dynamic?" needed. PackageLoader parses dynamic JSON identically to foundation.json. Spatial systems, travel systems, query systems treat all locations uniformly.

**Self-Containment Benefits:** Scene never references world entities that might not exist. Upper room location guaranteed to exist because scene creates it. Room key guaranteed to exist because scene creates it. Situations reference created resources through template indices (dependentLocations[0], dependentItems[0]) not brittle ID strings. Reproducibility guaranteed: same external context always creates identical dependent structure.

**Tutorial and Procedural Identical:** Tutorial scenes and procedural scenes use identical resource creation pipeline. Tutorial "Secure Lodging" spawns at Elena + Tavern Common Room (concrete), creates upper room adjacent, generates room key. Procedural "Lodging Service" spawns at any Innkeeper + Lodging location (categorical), creates upper room adjacent, generates room key. Same specifications, same creation logic, different placement context.

**Package Completeness:** Scene template is complete specification of everything scene needs. External dependencies declared through filters. Internal dependencies declared through specifications. Situations reference created resources. Archetype generation produces complete package from minimal external bindings. Scene can spawn anywhere matching filters without world modification.

### Tag-Based Progression Architecture

Scenes spawn based on player state tags rather than hardcoded scene ID dependencies. Pattern matches Sir Brante's consequence grouping: credentials (global unlocks), standing (local relationships), experience (portable skills), knowledge (contextual information).

**Tag Structure:** Each PlayerTag contains Name (from pre-authored library), Scope (personality type or null), Region (venue identifier or null). Grouping determines scope and region binding rules.

**CREDENTIALS Group (Global):**
Names: TravelersGuildMember, MerchantsGuildMember, PhysiciansCredential, NobleRecommendation, PilgrimStatus, CourierLicense. Scope null, Region null. Official documents and memberships travel everywhere. Grant once, apply universally. Example: Guild membership works at any inn in any region.

**STANDING Group (Region-Scoped):**
Names: EstablishedContact, ProvedReliable, ProvedUnreliable, CausedOffense, OwedFavor. Scope: personality type (Innkeepers, MerchantsGuild, CityGuard, Nobles, Clergy, Criminals). Region: venue where granted. Relationships are local - must rebuild in new regions. Example: EstablishedContact with Innkeepers at Vallenmarch doesn't transfer to Krossburg.

**EXPERIENCE Group (Global):**
Names: HandledCrisis, NegotiatedUnderPressure, SurvivedCombat, NavigatedWilderness, ManagedComplexDelivery. Scope null, Region null. Portable skills demonstrating capability. Grant through gameplay, apply everywhere. Example: Player who handled crisis at Vallenmarch has skill applicable anywhere.

**KNOWLEDGE Group (Mixed):**
Names: LearnedLocalCustoms (regional), DiscoveredSecretRoute (regional), UncoveredConspiracy (contextual), KnowsDangerousTruth (contextual). Scope and Region vary by knowledge type. Local information stays local, major discoveries may travel.

**Granting Tags:** Choice rewards include TagsToGrant with Name, Scope, Region. Tutorial lodging completion grants EstablishedContact scoped to Innkeepers in current region. Guild membership grants TravelersGuildMember with no scope or region. Tags added to Player.Tags collection.

**Requiring Tags:** Scene templates specify RequiredTags in spawn conditions. Patron meeting scene requires EstablishedContact with Innkeepers at current region. Advanced service scenes require relevant credentials or experience. SpawnConditionEvaluator matches player tags against requirements, resolving current region at evaluation time.

**Dependency Inversion:** Scenes don't reference other scene IDs. Producer scene grants tags, consumer scene requires tags, system matches through player state. Multiple scenes can grant same tag (tutorial lodging OR procedural lodging both grant EstablishedContact). Breaking scene A doesn't break scene B - just fewer ways to acquire necessary tags.

**Verisimilitude Through Scope:** Standing tags always region-scoped because relationships are local. Player established with Elena at Vallenmarch must rebuild relationship with Marcus at Krossburg. Credentials never region-scoped because official documents travel. Guild membership works everywhere. Experience never region-scoped because skills are portable. Knowledge varies by type.

**Sir Brante Consequence Display:** Scene completion shows grouped effects: CREDENTIALS (global unlocks), STANDING (relationship changes with scope and region), EXPERIENCE (skills acquired), KNOWLEDGE (information gained). Player sees exactly what changed and where it applies. Transparency enables strategic planning.

### The Three-Tier Timing Model

The codebase enforces strict separation between when content is defined, when it's instantiated, and when it becomes visible.

**Parse Time** occurs at game initialization. JSON files define immutable scene templates containing: complete situation sequences, spawn rules governing flow between situations, mechanical formulas for costs/benefits, item grant/consume/remove patterns, location state modification logic, and narrative hints for AI generation. Catalogues translate scene archetype IDs into these complete multi-situation structures during parsing exclusively.

**Instantiation Time** occurs when spawn conditions trigger. A scene template becomes runtime scene instance placed at concrete location/NPC. All situations within scene created as dormant entities simultaneously - they exist but none are active yet. First situation in sequence marked as current, others wait for spawn rules to activate them. No actions instantiated yet, just situation structure in dormant state.

**Query Time** occurs when player enters context matching scene's current situation. Scene.CurrentSituationId determines which situation is active. When player location/NPC context matches that situation's requirements, situation transitions from dormant to active and instantiates its choices into action entities. Player selects action, situation completes, spawn rules determine next situation. If next situation shares same context, auto-advances seamlessly. If next situation requires different context, player exits to world and scene remains in progress with updated CurrentSituationId until player reaches new context.

This three-tier separation prevents premature activation of later situations (rest situation doesn't activate before negotiation), ensures fresh evaluation at each beat (departure situation evaluates morning-specific conditions when player reaches that context), maintains scene persistence across navigation (can complete Situation 1, travel, return days later for Situation 2), and maintains efficient memory usage (actions only exist when player can see them).

### Template-Instance Composition for Scene Arcs

Scene templates contain embedded situation templates defining complete arc structure. SituationTemplates are NOT separate top-level entities in GameWorld - they're embedded within SceneTemplate as architectural definition of arc flow.

A "Secure Lodging" scene template contains four embedded SituationTemplates: negotiate, access, rest, depart. Each SituationTemplate has ChoiceTemplates defining player options at that beat. Scene template's SpawnRules define how situations activate sequentially (negotiate completes spawns access, access completes spawns rest, rest completes spawns depart).

At instantiation, Scene instance created with reference to template, all Situations instantiated simultaneously as dormant runtime entities, each Situation stores reference to its SituationTemplate for later action instantiation. Scene owns progression state (which situation is current), Situations own activation state (dormant versus active).

This composition enables: archetype-driven scene structure (template defines complete arc pattern), lazy action instantiation (situation dormant until player reaches that beat), template reusability (one scene template generates hundreds of contextual instances), and mechanical consistency (all lodging scenes follow same economic balance, just different narrative).

### The HIGHLANDER Principle

Every concept exists in exactly one authoritative location with no duplication.

Situations within scene live in GameWorld.Situations flat list, not nested in Scene.Situations property. Scene stores List<string> SituationIds referencing them. Querying "situations in this scene" filters flat list by IDs. This prevents situations existing in multiple locations simultaneously.

Spawn rules define situation flow but don't duplicate situation data. SpawnRule has SourceSituationTemplateId and DestinationSituationTemplateId (references), not embedded copies of situation definitions. Rule says "when situation A completes, spawn situation B", but doesn't contain duplicate copies of A or B.

Item grants stored in reward templates, item requirements stored in situation requirements, item lifecycle managed by scene progression, but item definition itself exists once in item catalog. No duplicating "room_key" definition across negotiation rewards, access requirements, and departure cleanup logic.

Location spot lock states stored once on LocationSpot entity. Access situation modifies spot.IsLocked property, departure situation restores it, but lock state isn't cached in scene. Query spot entity for current state, don't maintain duplicate tracking.

### Catalogue Constraint for Scene Archetypes

Scene archetype catalogues generate complete multi-situation structures at parse time exclusively. A scene archetype like "service_with_location_access" receives context (service type, tier, expected player resources, location properties) and outputs: complete sequence of SituationTemplates, ChoiceTemplates for each situation, cost/benefit formulas, item grant/consume patterns, SpawnRules governing flow, and cleanup logic.

The catalogue performs complex generation: calculates balanced economics (total cost versus total benefit), determines appropriate item types (temporary access tokens), defines sensible progression (negotiation before access before service), sets tier-appropriate costs (scales with player progression), and embeds default narrative hints (service type influences tone).

This generation happens ONCE per template load. Runtime never calls archetype catalogues. After parsing, Scene entities have concrete SituationTemplate lists with embedded ChoiceTemplates. The archetype ID becomes metadata for content tools. Runtime uses strongly-typed template structures, never regenerates from archetype.

This ensures fail-fast behavior (broken archetypes crash at load) and architectural boundaries (parsers import catalogues, game logic never does).

### Entity Properties as Generation Drivers

Scene templates specify required entity types categorically (needs NPC, needs Location, needs Spot), but the breakthrough is: **entity categorical properties drive archetype generation to produce contextually appropriate output**.

**At parse time for tutorial content:** JSON provides concrete entity IDs. Parser resolves IDs to entity objects (Elena NPC, Tavern Location, Upper Floor Spot). Archetype generation receives entity objects with all properties. Generation logic queries properties to determine: challenge difficulty (Demeanor=Friendly → easier), cost scaling (Services=Lodging + Privacy=Medium → moderate), benefit type (Functionality=Rest → stamina restoration), narrative hints (Personality=Innkeeper + Atmosphere=Casual → conversational).

**At instantiation time for procedural content:** Placement filter evaluates categorical requirements against game world. Selects entities matching filters (any Innkeeper at Urban location with Lodging service). Archetype generation receives selected entity objects. Identical generation logic queries identical properties. Produces contextually appropriate scene using different entities.

**Generation queries properties, not hardcoded values:** Cost formula doesn't specify "15 coins". Formula queries: npc.Demeanor (Friendly=0.8x, Gruff=1.2x multiplier) × location.CostModifier (Urban=1.0x, Remote=1.5x) × spot.Privacy (Shared=0.5x, Medium=1.0x, Private=2.0x) × tierBaseValue. Result: contextually appropriate cost that maintains economic balance.

**Properties ARE the content authoring system:** Designing NPC personalities (Friendly vs Professional vs Hostile), location properties (Urban vs Remote, Lodging vs Bathing), spot attributes (Privacy levels, Functionality types) IS designing the procedural content space. Same archetype generates infinite contextual variations determined by property combinations.

**Tutorial reproducibility guaranteed:** Same entities always produce same scene because generation is deterministic property query. Elena (Friendly Innkeeper) at Tavern (Urban Lodging) with Upper Floor (Medium privacy Rest functionality) ALWAYS generates identical negotiate difficulty, cost structure, rest benefit. Different playthroughs see identical tutorial.

### State Machine Orthogonality

Scenes track multiple independent state dimensions:

**Scene Lifecycle:** Provisional (not committed), Active (currently playing), Completed (fully resolved), Expired (time limit exceeded). Governs whether scene appears in world and accepts player interaction.

**Situation Activation:** Dormant (not yet reached), Active (player at this beat). Governs whether situation instantiates actions. Many situations dormant waiting for spawn rules to activate them.

**Situation Lifecycle:** Locked (requirements not met), Selectable (player can engage), InProgress (challenge executing), Completed (resolved successfully), Failed (attempt failed). Governs specific situation status independent of scene or activation state.

**Progression Tracking:** Which situation is current, which situations completed, what spawn rules triggered, what items granted/consumed, what world state modifications applied. Scene-level arc state.

These dimensions are genuinely orthogonal. A scene can be Active with current situation Completed waiting for spawn rule to trigger next situation which is still Dormant. Or situation can be Active (actions instantiated) but Locked (requirements not met yet) because player needs item from prior situation first.

Conflating these creates confusion. Original codebase had collision between activation state and lifecycle state both using "Active". Clear naming prevents bugs.

---

## Domain Model Understanding

### Entity Properties Control Generation

The categorical properties on entities (NPC, Location, LocationSpot) are the primary mechanism for contextual procedural generation. These properties query during archetype generation to produce contextually appropriate mechanical values and narrative hints.

**NPC Properties Affecting Generation:**

Personality (Innkeeper/Merchant/Guard/Priest/Attendant) determines: available service types, base relationship difficulty, negotiation patterns, authority level. Innkeepers provide lodging, Guards provide passage, Priests provide healing - same archetype generates different service benefits.

Demeanor (Friendly/Professional/Gruff/Hostile/Compassionate) scales: challenge difficulty (Friendly easier, Hostile harder), payment cost multipliers (Friendly cheaper, Gruff expensive), bond gain/loss magnitude, narrative tone hints (warm vs cold, formal vs casual).

Authority (Low/Medium/High) affects: consequence severity (high authority failures have bigger penalties), requirement strictness (high authority demands more proof), bypass option availability (can't always bribe high authority).

**Location Properties Affecting Generation:**

Services (Lodging/Bathing/Healing/Training/Storage) determines: which archetypes applicable (service_with_location_access requires service type), benefit type delivered (Lodging restores stamina, Healing restores health), spot functionality requirements (Lodging needs Rest spots).

CostModifier (Urban=1.0x, Remote=1.5x, Dangerous=2.0x) scales: base costs proportionally maintaining economic balance, affects accessibility perception in narrative hints.

Atmosphere (Casual/Formal/Rustic/Luxurious/Sacred) provides: narrative tone hints for AI generation, influences privacy expectations, affects appropriate demeanor combinations (Formal atmosphere mismatches Casual demeanor).

**Spot Properties Affecting Generation:**

Privacy (Shared/Semi-private/Private) determines: cost premium multipliers (Private costs 2x Shared), service quality scaling (Private spots provide better benefits), narrative intimacy level (Private enables personal conversations).

Functionality (Rest/Bathing/Healing/Storage/Training) must match: service type from location properties, determines mechanical benefit formulas (Rest uses stamina restoration, Bathing uses cleanliness stat).

Lockable (boolean) determines: whether access situation needs key item, affects security perception in narrative, required for service archetypes needing access control.

**Property-Driven Formula Example:**

Negotiation challenge difficulty = tierBaseDifficulty × npc.DemeanorMultiplier × npc.AuthorityMultiplier. Elena (Friendly=0.8x, Low=1.0x) at Tier 0 (base 15) = 12 Focus. Thorne (Hostile=1.4x, High=1.2x) at Tier 0 = 25 Focus. Same archetype, different difficulty from entity properties.

Payment cost = tierBaseCost × npc.DemeanorMultiplier × location.CostModifier × spot.PrivacyMultiplier. Tutorial: 10 × 0.8 × 1.0 × 1.0 = 8 coins. Luxury remote inn: 10 × 1.0 × 1.5 × 2.0 = 30 coins. Property combinations generate economic variety while maintaining balance.

**Properties Enable Contextual Reusability:**

Same archetype + different property combinations = infinite contextual variations. service_with_location_access archetype generates: friendly casual lodging (Elena), professional formal bathing (Marcus), compassionate sacred healing (Thalia), hostile military passage (Thorne). Mechanical coherence (same formulas) with contextual appropriateness (property-driven variation).

### Scene as Multi-Situation Arc

Scenes are self-contained narrative arcs with beginning, middle, end. Not single moments, but complete story beats spanning multiple player decisions and world state changes.

**Beginning:** Setup situation establishes premise. Innkeeper conversation, guard confrontation, merchant pitch. Player learns what scene offers and what it costs. Choice determines how arc progresses (challenge versus payment versus refusal).

**Middle:** Execution situations deliver on promise. Access locked area, perform service, achieve goal. May involve navigation (move to spot), time passage (wait for result), resource application (consume items/energy). Mechanical benefit applied here.

**End:** Conclusion situation wraps up arc. Return to normal, cleanup temporary state, receive final consequences. Departure from room, leaving checkpoint, completing transaction. World restored to coherent state for next player visit.

Situations within scene know their position. Template properties indicate: IsOpening (first situation), IsIntermediate (middle beats), IsConclusion (final situation). This affects narrative generation (opening introduces, conclusion wraps up) and mechanical behavior (openings may have requirements, conclusions may have cleanup).

### Multi-Scene NPC Interactions

Single NPC can have multiple independent active scenes simultaneously. Each scene represents distinct narrative thread with own lifecycle, situations, and completion state.

**Physical Presence vs Interactive Opportunities:** NPCs exist as physical entities (always visible when present). Interaction opportunities exist only when NPC has active scenes. Each active scene spawns separate interaction option with descriptive label. Elena at Common Room with 2 active scenes ("Secure Lodging" + "Inn Trouble Brewing") shows 2 buttons. Thomas at Common Room with 0 active scenes shows 0 buttons.

**Scene Independence:** Completing one scene doesn't affect others. "Secure Lodging" and "Inn Trouble Brewing" progress independently at same NPC. Both remain visible until each reaches own completion criteria.

**Navigation Routing:** Player selection routes to specific (npcId, sceneId) pair, not just npcId. Direct lookup prevents ambiguity when multiple scenes active.

**Spawn Independence Through Tags:** Tutorial scenes spawn at parse-time (concrete bindings), obligation scenes spawn at runtime when player acquires matching tags. Multiple obligations can spawn scenes at same NPC simultaneously. Scenes grant tags on completion (LodgingExperienceAcquired, PatronEstablished). Future scenes require tags, not scene IDs. No direct scene-to-scene dependencies.

**Perfect Information Principle:** Players must see ALL available interactions to make strategic decisions. Scene exists + has active situation = show button (even with placeholder label). No active scene = no button. Functionality trumps aesthetics.

### Situation Sequencing via Spawn Rules

Situations don't activate arbitrarily. SpawnRules on Scene template govern flow:

**Linear Progression:** Situation A completes, spawn rule activates situation B. Situation B completes, spawn rule activates situation C. Strict sequence, no branching. Used for: service arcs (negotiate → access → use → depart), tutorial flows, time-bound sequences.

**Conditional Branching:** Situation A completion spawns different situations based on outcome. Success spawns situation B, failure spawns situation C. Both valid continuations, different narrative tones. Used for: negotiation failures (success grants access, failure demands alternative), challenge outcomes (victory rewards, defeat consequences).

**Hub-and-Spoke:** Initial situation spawns multiple parallel situations, player chooses order, all must complete before finale. Used for: investigation scenes (gather clues at multiple locations), preparation sequences (acquire items from different sources), relationship building (multiple conversation topics).

**Recursive Spawning:** Situation completion can spawn additional situations within same scene dynamically. Investigation gathering evidence might spawn follow-up investigation based on what was found. Crisis situation might spawn escalation situations if player choices make things worse.

**Context-Based Auto-Advance:** When consecutive situations share identical location/NPC context, scene auto-advances seamlessly without returning player to world. Scene checks next situation's placement context against current context. Match = instant transition, player locked in scene flow. Mismatch = player exits to world, scene persists with updated CurrentSituationId, scene resumes when player reaches matching context. This enables: seamless cascades within single location (unlock door → rest in room), natural navigation between locations (negotiate at common room → travel to upper floor → rest), persistent scene state across time (complete situation, travel for days, return to continue).

Spawn rules are DECLARATIVE DATA defining flow, not imperative code. Scene template lists transitions (source situation → destination situation, with conditions). Scene instance executes rules by checking completion, evaluating conditions, activating next situations. Domain entity owns state machine.

### Item Lifecycle Within Scene Scope

Scenes grant temporary items enabling progression through explicit choice rewards:

**Grant Phase:** Early situation rewards player with item via ChoiceReward.ItemIds. Room key from innkeeper negotiation, permit from guard approval, token from merchant purchase. Item added to player inventory.

**Consumption Phase:** Middle situation requires item to proceed via HasItem requirement. Accessing locked location requires room key. Presenting permit allows checkpoint passage. Showing token enables service. Requirements check player inventory.

**Removal Phase:** Final situation explicitly removes temporary item via ChoiceReward.ItemsToRemove. Departure choice specifies "remove room_key from inventory". Author controls cleanup, making it visible in content design. More reliable than hidden automatic tracking.

Items can also be permanent rewards. Completing investigation grants knowledge_fragment that persists. Choice rewards distinguish: access tokens, permanent rewards, consumable items.

Scene manages item lifecycle through authored choice rewards, not automatic tracking. Author sees item lifecycle in situation definitions: grant in negotiate rewards, require in access requirements, remove in depart rewards. Debuggable and explicit.

### Resource Restoration Economics

Service scenes typically cost resources early, provide benefits later:

**Cost Phase:** Negotiation situation costs coins OR energy (challenge). Player spends resources upfront to access service.

**Benefit Phase:** Service situation restores different resources. Lodging restores stamina and reduces sleep need. Healing restores health. Training increases stats. Benefit justified by earlier cost.

**Economic Balance:** Total benefit must justify total cost across player progression. Early game lodging costs 15 coins and restores 30 stamina (good value for poor players). Late game lodging costs 60 coins and restores 80 stamina (good value for rich players maintaining same ratio). Formulas scale both cost and benefit by tier.

Not all scenes have restoration. Investigation scenes cost resources (time, energy) but grant knowledge (information, items, narrative progression) without resource restoration. Service scenes specifically characterized by resource exchange pattern.

### Location State Modification Patterns

Scenes modify world state temporarily or permanently:

**Temporary State Changes:** Spot locks toggle for scene duration. Upper floor unlocked when player has key, re-locked when scene completes. NPC availability shifts (shopkeeper busy during your transaction). These revert after scene.

**Permanent State Changes:** Location properties modified permanently. Completing investigation at location adds "Investigated" property preventing repeat. Causing scene failure might add "Hostile" property changing future interactions. These persist beyond scene.

**Time Advancement:** Service situations advance world clock. Resting advances to morning. Long travels advance multiple time blocks. Training advances by days. Time affects all world state (NPC schedules, shop availability, random events, spawn eligibility for time-gated scenes).

**State Restoration:** Cleanup situations ensure coherent world state. Can't leave spot permanently unlocked (breaks future lodging scenes). Can't leave NPC in "transaction active" state. Can't leave time frozen. Scene completion restores or advances appropriately.

Scenes don't modify structural entities (won't delete locations or NPCs), only modify state properties on those entities temporarily.

---

## Current State Analysis

### Tutorial Scene Structure

Tutorial "Secure Lodging" scene is minimally authored, maximally generated:

**Tutorial JSON (Complete):**
```
{
  "id": "tutorial_lodging",
  "archetypeId": "service_with_location_access", 
  "tier": 0,
  "isStarter": true,
  "concreteNpcId": "elena_innkeeper",
  "concreteLocationId": "tavern_common_room"
}
```

Only external dependencies specified: Elena (NPC) and Tavern Common Room (base location). No situations, choices, dependent locations, or items defined in JSON. Scene creates everything else from archetype specifications.

**Tutorial Sequence Context:**

Tutorial begins with separate Guild Recruiter scene (single situation, binary choice):
- **Join Traveler's Guild** (50 coins, grants guild_token item, **grants TravelersGuildMember tag (Universal)**)
- **Decline** (keep coins, no tag granted)

Guild membership tag unlocks bypass choices throughout game world. Universal scope means valid in all regions. Strategic trade-off: expensive upfront cost versus permanent benefit across all regions.

Both paths through negotiation (convince or pay) grant InnkeeperRapport tag (Regional, Northern Highlands). This regional tag enables future advanced lodging scenes in this region and serves as conditional requirement for innkeeper interactions here. Tag system enables scenes to require capabilities without knowing which scene granted them - dependency inversion eliminates brittle scene-to-scene ID references.

**Parse-Time Generation:** SceneArchetypeCatalog.GenerateServiceWithLocationAccess receives: archetype ID, tier 0, Elena entity object (Personality=Innkeeper, Demeanor=Friendly, Authority=Low), Tavern Common Room location object (Services=Lodging), player state benchmarks.

Generation queries properties and produces complete self-contained package:

**Dependent Resources Created at Spawn:**
- Upper Room location (adjacent hex to Tavern Common Room, pattern name "Elena's Private Room", IsLocked=true, Services=Lodging, lifecycle=PermanentLock)
- Room Key item (pattern name "Room Key", unique per scene instance, lifecycle=SceneScoped)

**Situation Structure (references created resources):**

**Situation 1 - Negotiation (Common Room + Elena):** Three choices demonstrating Sir Brante-style transparent choice design:

1. **Convince Elena** - Social challenge (15 Focus, Friendly demeanor = easier difficulty)
   - Visible consequences: +1 Bond with Elena, grants room key, **grants tag: InnkeeperRapport (Regional)**
   - Available (player has sufficient Focus)

2. **Pay Elena** - Direct transaction (15 coins, tier 0 base cost)
   - Visible consequences: Grants room key, no bond change, **grants tag: InnkeeperRapport (Regional)**
   - Available (player has sufficient coins)

3. **Show Traveler's Guild Token** - Free access
   - Visible consequences: Grants room key, no cost, no tag (already have TravelersGuildMember)
   - **LOCKED** - Requires: TravelersGuildMember (Universal)
   - Grayed presentation showing: "Join guild earlier to unlock this option"

All three choices visible simultaneously. Requirements shown upfront. Consequences displayed before selection including tag grants. Successful negotiation or payment grants InnkeeperRapport tag (Regional scope, Northern Highlands region). This tag enables advanced lodging scenes in this region and conditional bypass choices at other inns here. Guild token bypasses without granting regional tag (universal credential already covers all regions).

Player learns: building local relationships through challenge/payment creates regional advantages. Universal credentials bypass without relationship building. Multiple paths grant same regional tag - strategic flexibility.

Successful choices grant scene-created room_key item and unlock scene-created Upper Room location.

**Situation 2 - Access (Upper Room):** Navigate to created Upper Room location (now unlocked). Single choice (enter room) with HasItem requirement (scene-created room_key). Choice cost includes TimeSegments (time to walk upstairs). Location and item both created by scene at spawn, guaranteed to exist.

**Situation 3 - Rest (Upper Room):** Time cost 6 hours via ChoiceCost.TimeSegments (advances to morning). Benefit: restore 40 stamina (Tier 0 formula). Choice reward advances time to morning block.

**Situation 4 - Departure (Common Room + Elena):** Cleanup situation. ChoiceReward specifies: ItemsToRemove=[scene-created room_key], LocationsToLock=[scene-created Upper Room], GrantPlayerTags=["LodgingExperienceAcquired"]. Explicit author-visible cleanup. Single automatic choice (leave room). Upper Room permanently locked after scene completion (lifecycle=PermanentLock). Room key removed from inventory. Player gains tag enabling future lodging-dependent scenes.

**Complete 4-situation arc generated from properties + archetype pattern.** Situation flow (SpawnRules), choice structures, costs, benefits, item lifecycle all procedurally determined by querying Elena/Upper Floor properties and applying archetype formulas.

**Runtime Context-Aware Flow:**
1. Scene spawns → Creates Upper Room location adjacent to Common Room, creates room_key item
2. Player at Common Room clicks "Secure Lodging" → Situation 1 activates
3. Player completes Situation 1 → Scene.AdvanceToNextSituation checks context
4. Next situation (Sit 2) requires created Upper Room (different location) → **Exit to world**
5. Player sees Upper Room on hex grid (adjacent to Common Room), navigates there
6. LocationViewBuilder calls Scene.ShouldActivateAtContext → Context matches Situation 2 requirements
7. Situation 2 activates automatically
8. Player completes Situation 2 → Scene.AdvanceToNextSituation checks context
9. Next situation (Sit 3) also at created Upper Room (same location) → **Auto-advance seamlessly**
10. Player completes Situation 3 → Scene.AdvanceToNextSituation checks context
11. Next situation (Sit 4) requires Common Room (different location) → **Exit to world**
12. Player navigates to Common Room → Situation 4 activates when context matches
13. Player completes Situation 4 → Scene cleanup executes: removes room_key, permanently locks Upper Room, grants "LodgingExperienceAcquired" tag to player
14. Scene marks complete, Upper Room remains on hex grid but inaccessible forever
15. Future scenes requiring "LodgingExperienceAcquired" tag now eligible to spawn

Situations 2→3 cascade seamlessly (shared created Upper Room context). Situations 1→2 and 3→4 require navigation (context changes). Scene persists across navigation via CurrentSituationId. Created resources exist throughout scene lifecycle, cleaned up at completion. Tag granted enables spawning of dependent scenes without direct scene-ID references.

**Tutorial Reproducibility:** Every playthrough generates identical scene because: same entities → same properties → same generation output. Elena always Friendly (easier challenge), Upper Floor always moderate cost, service always rest benefit.

**Post-Tutorial Variation:** Same archetype at Marcus's bathhouse generates: Professional demeanor (moderate difficulty), Bathing service (cleanliness benefit instead of stamina), different location context. Negotiation offers: (1) Convince Marcus challenge (grants BathhouseRapport Regional tag), (2) Pay standard fee (grants BathhouseRapport Regional tag), (3) Show Bathhouse Guild Membership (requires MerchantsGuildMember Universal tag, locked). At Thalia's temple: (1) Convince Thalia challenge (grants TempleStanding Regional tag), (2) Donate to temple (grants TempleStanding Regional tag), (3) Show Healer's Credential (requires MedicalKnowledge Universal tag, locked). Different entities, contextually appropriate variation, identical generation logic. Regional tags (BathhouseRapport, TempleStanding, InnkeeperRapport) enable advanced scenes in that region. Universal tags (TravelersGuildMember, MerchantsGuildMember, MedicalKnowledge) unlock bypasses everywhere. Strategic depth: build regional relationships (free, localized) versus acquire universal credentials (expensive, portable). Tags enable spawning without scene-ID dependencies - complete dependency inversion.

### Scene Archetype Catalog Minimal

Situation archetype catalog exists (generates individual situation choice structures) but no scene archetype catalog exists. No code generates complete multi-situation arcs from archetypes. Each scene currently hand-authored defining complete situation sequence.

Need scene archetype catalog that generates: situation sequences, spawn rules, item grant/consume/remove patterns, resource cost/benefit formulas, state modification logic, cleanup sequences. This is more complex than situation archetype generation (single choice structure versus complete arc orchestration).

### Spawn Rule Execution Exists

Scene domain entity has SpawnRules property and situations have completion tracking, but Scene lacks methods to execute rules. SituationCompletionHandler checks rules and spawns next situations, but this is service responsibility not domain entity behavior.

Architecture expects Scene.AdvanceToNextSituation() method that queries own SpawnRules, finds transition for completed situation, activates next situation(s), updates CurrentSituationId, marks scene complete if no more situations. Domain entity should own state machine, not have lifecycle scattered across services.

### Placement Filter Supports Spots

PlacementFilter evaluates NPC and Location entities, but scenes also need LocationSpot references (for lockable areas). Filter can specify location properties (Lodging, Bathing, Healing) but doesn't specify spot requirements (needs lockable interior room, needs private chamber, needs secluded area).

Scene instantiation currently hardcodes spot selection (first matching spot at location). Need spot filtering (accessibility, privacy level, capacity) and selection strategies (prefer higher privacy if affordable, prefer closer to entrance if rushed).

### Item System Exists But Scene Integration Unclear

Player.Inventory exists, items can be granted/removed. Spot.RequiredItems exists for locking mechanism. But scene-managed temporary items unclear. Does room_key persist in inventory forever, or removed by scene? Does scene track "I granted this key, I must clean it up"?

Need scene-level item tracking: items granted by this scene instance, items consumed during progression, items to remove at completion. Scene owns lifecycle for items it introduces.

### Time System Incomplete

World has CurrentDay and CurrentTimeBlock, situations can advance time, but integration unclear. Does rest situation automatically advance to morning, or does player control when to wake? Does time advancement trigger morning-specific narrative in departure situation?

Need robust time advancement integration: situations declare time cost (rest costs night, short activities cost segments, instant activities cost zero), situation completion applies time cost, world state updates, subsequent situations in arc see updated time context (morning versus evening affects narrative).

---

## Design Approach & Rationale

### Scene Archetypes as Multi-Situation Patterns

Scene archetypes define complete narrative arc patterns with multiple situations in sequence:

**Service with Location Access:** Four situations (negotiate, access, service, depart). Negotiation offers three paths: challenge (tests player skill/resources), payment (direct transaction), conditional bypass (requires specific credential/membership). All paths visible simultaneously with requirements and consequences displayed. Sir Brante transparency: locked options show what player lacks, teach strategic value of earlier decisions. Used for: lodging (rest), bathing (cleanliness), storage (item management), training (skill increase), healing (health restoration). Cost varies by service type, benefit varies by tier, but structure identical. Conditional bypass teaches long-term strategic value (guild membership grants free lodging, physician credential grants free healing, noble seal grants privileged access).

**Transaction Sequence:** Three situations (browse inventory, negotiate price, complete transaction). Used for: shopping (buy items), selling (convert items to coins), trading (item exchanges). Complexity varies by merchant type and item rarity.

**Investigation Arc:** Variable situations (gather evidence, analyze findings, confront suspect, resolve mystery). Hub-and-spoke pattern (gather from multiple sources) converging to linear conclusion. Used for: quest lines, mystery solving, knowledge acquisition.

**Gatekeeper Sequence:** Two to four situations (initial confrontation, prove worthiness or bribe, pass checkpoint, optional consequences). Linear or branching based on player approach. Used for: guard posts, restricted areas, authority challenges.

**Crisis Response:** Three to five situations (crisis discovered, assess options, execute solution, handle aftermath, consequences). Time-sensitive, failure spawns escalation. Used for: emergencies, disasters, urgent requests.

Each archetype specifies: situation count and types, spawn rules governing flow, item lifecycle patterns, resource economics (costs versus benefits), state modification logic (what world changes occur), cleanup requirements (what must be restored).

Archetypes are mechanical arc patterns devoid of narrative. Same archetype generates lodging at inn (rest), bathing at bathhouse (cleanliness), healing at temple (health) with identical situation flow but different service benefits and narrative context.

### Why Multi-Situation Arcs Work

Players learn scene patterns through repetition with variation:

**Pattern Recognition:** Encounter "service with access" at inn (lodging), recognize same structure at bathhouse (bathing), apply knowledge to new contexts. Structural familiarity speeds comprehension, narrative variety prevents repetition feeling stale.

**Strategic Planning:** Knowing lodging scene requires negotiation → access → rest → departure helps player plan resource expenditure. "I need 15 coins for negotiation and 6 hours for rest, do I have both?" Perfect information at arc level, not just situation level. Understanding context changes helps plan: "Negotiation at common room, then must travel upstairs, complete service there, return to finish."

**Locked Choices as Teaching:** Sir Brante pattern: show unavailable options with visible requirements and consequences. Guild token choice locked at Elena displays: "Requires: Traveler's Guild Token" and "Would grant: Free lodging, no cost." Player learns long-term trade-offs without punishment. Future playthroughs informed by seeing complete option space. No hidden choices - transparency enables strategic thinking.

**Expectation Management:** Scene archetypes establish expectations. Service scenes restore resources (positive outcome). Investigation scenes grant knowledge (informational outcome). Crisis scenes create cascading consequences (potentially negative outcome). Genre conventions maintained by consistent archetype usage.

**Natural World Integration:** Context-aware progression maintains fiction coherence. Player negotiates lodging, physically travels to room, experiences service, physically returns to conclude. Scene arcs flow naturally through world geography rather than creating isolated narrative bubbles. Scenes persist across time and navigation.

**Mechanical Consistency:** All service scenes balanced similarly. Lodging costs X and restores Y, bathing costs X and restores different Y, healing costs X and restores third Y. Ratios consistent, benefits different, balance maintained. Archetypes enforce economic equilibrium across procedurally generated scenes.

**Sir Brante Choice Design Philosophy:** Wayfarer adopts The Life and Suffering of Sir Brante's transparent choice design. Display all choices including locked options with visible requirements. Show exact consequences before selection (stat changes, relationship impacts, resource costs). Multiple valid approaches to same goal representing different character expressions. Locked high-requirement choices teach strategic planning: "I need Determination 4 to unlock this option - next time I'll prioritize that stat." Perfect information enables strategic planning rather than trial-and-error guessing.

### Formula-Driven Arc Economics

Complete scene economics must balance across entire arc, not just per-situation:

**Total Cost Calculation:** Sum all costs across all situations. Negotiation costs 15 coins OR 20 Focus, access costs time segment, rest costs 6 time segments. Total: 15 coins + 7 time OR 20 Focus + 7 time.

**Total Benefit Calculation:** Sum all benefits across all situations. Rest restores 40 stamina, removes sleep deprivation penalty, grants "well rested" temporary buff. Benefit must justify total cost.

**Tier Scaling:** Early game (tier 0-1) has cheaper services (15 coins, 40 stamina restored). Late game (tier 3-4) has expensive services (60 coins, 120 stamina restored). Both maintain ~2.5x stamina-per-coin ratio. Formulas scale costs and benefits proportionally.

**Opportunity Cost Consideration:** 7 time segments spent resting means 7 segments not spent traveling, investigating, or doing other activities. Must be worth the opportunity cost. Rest benefit must exceed "I could have earned coins elsewhere with this time" threshold.

Scene archetype generation calculates economics holistically: receives player state (maximum resources, earning rates, resource depletion rates), determines appropriate cost (challenging but achievable), determines equivalent benefit (justifies cost), distributes costs/benefits across situations appropriately (front-load costs, back-load benefits).

### Categorical Placement with Spot Filtering

Scenes need THREE entity contexts, not just two:

**NPC Context:** Service provider with appropriate personality. Innkeeper for lodging, attendant for bathing, priest for healing, guard for checkpoints. Personality affects narrative tone and negotiation difficulty.

**Location Context:** Appropriate location type with required properties. Inn for lodging (Lodging property), bathhouse for bathing (Bathing property), temple for healing (Healing property). Location atmosphere affects narrative setting.

**Spot Context:** Specific lockable area player accesses. Upper floor room at inn, private bath chamber at bathhouse, healing sanctuary at temple. Spot privacy level affects service quality (private rooms better than shared).

Placement filter evaluates all three: "any Innkeeper NPC at any Urban location with Lodging property at any location with lockable Interior spot with privacy level Medium+". Resolution finds concrete matches across three entity types simultaneously.

Spot filtering needs properties: accessibility (public/private/restricted), privacy (shared/semi-private/private), capacity (number of users), functionality (rest/storage/service). These enable selection strategies (prefer private if affordable, accept shared if cheap) and ensure mechanical appropriateness (can't rest in public square, can't store items in busy corridor).

### AI Generation Per Situation Within Arc

AI generates narrative SEPARATELY for each situation in scene, not for entire scene at once:

**Situation 1 Generation:** AI receives: NPC context (Elena innkeeper, friendly personality, bond 0, new relationship), location context (tavern common room, evening, busy), situation position (opening of service arc), service type (lodging), player state (tired, low coins). Generates: situation description (Elena greets you), choice action texts (convince her vs pay upfront), success narratives (she hands you key), transition narrative (head upstairs).

**Situation 2 Generation:** AI receives: Same NPC context (but now bond +1 if player chose challenge), location context (upper hallway, still evening), situation position (intermediate, after negotiation), item context (holding room_key), player state (same). Generates: situation description (hallway is quiet), choice action text (enter your room), transition narrative (you unlock door).

**Situation 3 Generation:** AI receives: Same contexts, situation position (intermediate, service phase), location context (NOW MORNING because time advanced), player state (refreshed). Generates: situation description (morning light through window), choice action text (get out of bed), transition narrative (you gather belongings).

**Situation 4 Generation:** AI receives: Same contexts, situation position (conclusion), location context (morning, common room). Generates: situation description (you return to common room), choice action text (leave room), conclusion narrative (Elena nods as you depart).

Each situation generation independent, context-aware of position in arc, receives updated world state (time, location changes), produces narrative appropriate to that beat. Full scene narrative emerges from sequenced per-situation generations.

### Template Cleanup Patterns

Scene templates define cleanup requirements ensuring world state coherence after completion:

**Item Cleanup:** List items granted by this scene that should be removed. Room keys removed (can't keep keys after checkout), permits removed (expired after use), tickets consumed (single use). Permanent items (rewards) not removed.

**State Restoration:** List world states to restore. Upper floor spot re-locked, NPC availability reset to normal, environmental states reverted. Ensures next player finds consistent world state.

**Conditional Cleanup:** Some cleanup conditional on scene outcome. Successful negotiation increases NPC bond permanently (not cleaned up). Failed negotiation might add temporary negative state that expires. Success/failure determines what persists.

**Time Validation:** Ensure time advanced sensibly. Can't complete rest scene without advancing time. Can't complete instant transaction while claiming hours passed. Validation catches inconsistent time handling.

Scene archetype generation includes cleanup logic appropriate to archetype type. Service scenes clean up access items and re-lock locations. Investigation scenes don't clean up knowledge gained. Transaction scenes clean up negotiation state but not purchased items.

### Tag-Based Dependency Inversion

Scenes grant abstract player tags on completion instead of creating direct scene-ID dependencies. Sir Brante pattern: "CONDITIONS MET" displays which tags unlocked scene.

**Problem with Scene-ID Dependencies:** Hardcoded spawn condition "completedScenes: ['tutorial_lodging']" creates brittle coupling. Scene won't spawn if tutorial skipped, alternate path taken, or scene ID renamed. Multiple providers (tutorial lodging OR procedural lodging) require duplicate spawn templates.

**Solution: Abstract State Tags:** Scenes grant tags representing player state changes. Tutorial lodging grants "LodgingExperienceAcquired" tag. Procedural lodging grants same tag. Advanced lodging scene requires tag, not specific scene ID. Any scene granting tag enables spawn. Complete dependency inversion.

**Tag Examples:**
- LodgingExperienceAcquired (any lodging completion)
- TravelersGuildMember (guild enrollment)
- PatronEstablished (patron relationship formed)
- HealersCredential (healer training completed)
- CombatVeteran (survived first combat)

**Multiple Providers:** Tutorial and procedural paths grant identical tags. Player completing tutorial lodging OR finding procedural inn both gain "LodgingExperienceAcquired." Future scenes requiring lodging experience spawn from either source. No duplicate templates needed.

**Player Tags List:** Player entity maintains List<string> Tags. Scene.ExecuteCleanup adds granted tags to player's list. Spawn evaluation checks RequiredTags against player's current tags. Sir Brante transparency: locked scenes display missing tags showing player what state changes would unlock content.

**Benefits:** No brittle scene-ID references, complete flexibility in content ordering, tutorial and procedural equivalence, clear state-based progression teaching, Sir Brante-style transparent requirements display.

---

## Implementation Strategy

### Phase 1: Scene Archetype Catalog Creation

Build catalog generating complete multi-situation scene structures from archetype IDs with entity properties driving contextual generation.

**Generation Method Signature Pattern:** Each archetype generation method receives: archetype ID, tier, strongly-typed entity objects (NPC for service provider, base Location for service context), player state benchmarks. NOT spot entity - archetype creates dependent location.

**Property Query Pattern:** Generation logic queries entity properties to determine contextual mechanical values. Challenge difficulty queries npc.Demeanor (Friendly=0.8x multiplier, Hostile=1.4x). Payment cost queries location.CostModifier. Service benefit queries location.Services (Lodging=stamina, Bathing=cleanliness). Narrative hints query npc.Personality and location.Atmosphere.

**Dependent Resource Specifications:** Archetype produces specifications for resources scene will create at spawn. Service archetype generates: dependent location specification (relative placement adjacent to base, pattern name incorporating NPC name, property requirements IsLocked + Services matching service type, lifecycle strategy PermanentLock), dependent item specification (pattern name "Room Key" or context-appropriate, lifecycle SceneScoped for removal at completion).

**Situation Generation:** Produces four SituationTemplates with Sir Brante transparency. Negotiate situation has ChoiceTemplates: (1) challenge with visible cost (Focus amount) and visible consequences (+bond, grants key), (2) payment with visible cost (coins amount) and visible consequences (grants key, no bond), (3) conditional bypass with visible requirement (specific item) and visible consequences (free key). All three visible simultaneously. Locked choice shows exact requirement text and preview of benefits. Access situation references created location and requires created item. Service situation references created location with property-determined benefit and time cost. Departure situation has cleanup specifying created item removal, created location lock restoration, and player tag granting (LodgingExperienceAcquired, ServiceCompleted, or context-appropriate tag).

**SpawnRules Generation:** Linear flow (negotiate→access→service→depart) with success conditions. Each transition references situation template IDs from generated situations. Situations specify required contexts (base location for negotiate, created location for access/service, base location for depart).

**Item Lifecycle:** Grant created item in negotiate rewards, require created item in access situation, remove created item in depart cleanup. Item lifecycle completely managed within scene boundaries.

**Tutorial and Procedural Use Identical Logic:** Tutorial parser resolves concrete entity IDs to objects (Elena, Tavern Common Room), passes to generation. Procedural placement evaluator selects entities matching filters (any Innkeeper, any Lodging location), passes to generation. Generation receives entity objects either way, queries properties identically, produces contextually appropriate output including dependent resource specifications.

**Test Pattern:** Create test entities with extreme property values. Friendly (0.8x) vs Hostile (1.4x) NPCs should generate 1.75x difficulty difference. Urban (1.0x) vs Remote (1.5x) locations should show cost scaling. Verify formulas balance across property ranges. Test Sir Brante transparency: all choices visible including locked ones, requirements shown clearly, consequences displayed before selection. Test conditional bypass choices: verify locked choice shows "Requires: X" text and consequence preview. Verify dependent resource specifications produced correctly (location placement, item naming, lifecycle strategies). Test tag-based spawning: complete scene granting "LodgingExperienceAcquired" tag, verify future scene with RequiredTags=["LodgingExperienceAcquired"] becomes eligible to spawn. Verify multiple scenes can grant same tag (tutorial lodging OR procedural lodging both enable advanced scenes). Verify Sir Brante "CONDITIONS MET" display shows which tags unlocked scene.

### Phase 2: Scene State Machine and Dynamic Location Creation Pipeline

Implement complete scene spawning pipeline orchestrated by GameFacade with multi-system coordination ensuring facade isolation.

**GameFacade Orchestration (Multi-Step Pipeline):** Receives SceneTemplate from SceneGenerationFacade after validation. Extracts LocationCreationSpec list from template. **Step 1 - JSON Generation:** For each location spec, calls ContentGenerationFacade.CreateDynamicLocationFile(spec). ContentGenerationFacade transforms strongly-typed spec into JSON matching foundation.json structure, writes to /dynamic-content/locations/scene_{sceneId}_location_{index}.json, returns DynamicFileManifest (strongly-typed: filepath, timestamp, checksum). **Step 2 - Package Loading:** For each manifest, calls PackageLoaderFacade.LoadDynamicContent(filepath). PackageLoader parses JSON identically to static content parsing, creates Location entity in GameWorld.Locations, integrates with hex grid and spatial systems, returns LocationCreationResult (strongly-typed: success boolean, locationId string, errorReason enum). **Step 3 - Scene Instantiation:** Collects all created location IDs from successful results, calls SceneInstanceFacade.InstantiateScene(template, locationIds). Scene entity created with CreatedLocationIds tracking list populated.

**Atomic Creation Guarantee (LET IT CRASH):** All locations for scene created successfully before scene instantiation proceeds. If ANY location creation fails - hex coordinate occupied, JSON write error, duplicate location ID, invalid placement rule - entire scene spawn aborted with exception. Already-created locations rolled back: removed from GameWorld, JSON files deleted using manifests, hex grid updated. Player sees error message with full diagnostic information, world state unchanged. **No partial scene spawning ever.** No "created 2 of 3 locations, continuing anyway." No fallbacks. No graceful degradation. Either complete success or clean failure with crash. Developer investigates stack trace, fixes root cause (hex grid algorithm, placement logic, file permissions, whatever), reloads, tries again. System forces correctness rather than tolerating partial states.

**Facade Isolation Enforcement (Critical):** SceneGenerationFacade never imports ContentGenerationFacade, PackageLoaderFacade, GameWorld, or Unity. ContentGenerationFacade never imports SceneGenerationFacade, PackageLoaderFacade, or GameWorld. PackageLoaderFacade never imports SceneGenerationFacade or ContentGenerationFacade. SceneInstanceFacade never imports ContentGenerationFacade or PackageLoaderFacade. **Only GameFacade calls multiple facades.** Facades never call each other. Facades only know domain entities and their immediate responsibilities. Clean dependency graph prevents coupling: Facades → Domain, GameFacade → Facades, no facade-to-facade arrows.

**Scene.AdvanceToNextSituation(completedSituationId):** Queries SpawnRules, finds transitions from completed situation, gets destination situation template IDs, instantiates destination situations as runtime entities (add to GameWorld.Situations with scene ID reference), transitions those situations from nonexistent to Dormant state. **Critical context check:** Compares next situation's required location/NPC context against current context. Location comparison includes both base location and dynamically-created locations by their final GameWorld IDs. If match (same location/NPC), immediately activates next situation and returns flag indicating "continue scene flow" (lock player in seamless cascade). If mismatch (different location/NPC), updates CurrentSituationId but leaves situation dormant, returns flag indicating "exit to world" (player must navigate). Marks scene complete if no valid transitions exist. Situations reference dynamically-created locations by concrete GameWorld location IDs - no temporary references, no specs, only real entities.

**Scene.ShouldActivateAtContext(locationId, npcId):** Queries CurrentSituationId, gets current situation template, compares situation's required context (location/NPC) against provided context. Location comparison checks both base location and created locations. Returns true if match, enabling situation activation when player reaches correct location. Supports scene persistence across navigation.

**Scene.ExecuteCleanup():** Queries template cleanup specifications and scene's created resource tracking lists. Removes temporary items from player inventory (including scene-created items). Applies IntegrationStrategy to created locations: **PermanentLock** sets Location.IsLocked=true permanently (location remains on grid but inaccessible), **TemporaryRemove** returns location IDs to caller (GameFacade then calls ContentGenerationFacade.RemoveDynamicLocation(locationId, manifest) which removes from GameWorld and deletes JSON file), **Reusable** restores location to initial state for future scene spawns. Updates DynamicContentManifest tracking. Grants player tags specified in template (adds to Player.Tags list enabling future scene spawns based on RequiredTags). Reverts NPC availability. Validates time advanced appropriately. Marks scene as fully resolved. No orphaned resources after cleanup. Tag granting implements dependency inversion - future scenes spawn based on player state, not hardcoded scene IDs. Cleanup returns strongly-typed CleanupResult indicating locations requiring removal for GameFacade coordination.

**Scene.IsComplete():** Checks if CurrentSituationId points to conclusion situation and that situation is Completed, or if no valid spawn transitions remain. Boolean indicating arc finished.

These methods encapsulate scene lifecycle while GameFacade orchestrates multi-system operations. GameFacade orchestrates ContentGenerationFacade (JSON creation), PackageLoaderFacade (entity instantiation), and SceneInstanceFacade (scene spawning) during spawn pipeline. SituationCompletionHandler calls Scene.AdvanceToNextSituation() after situation resolves, receives context-match flag determining whether to lock player in scene or return to world. LocationViewBuilder calls Scene.ShouldActivateAtContext() when displaying location to determine which dormant situations should activate. GameFacade calls Scene.ExecuteCleanup() when scene marked complete, receives CleanupResult, then calls ContentGenerationFacade.RemoveDynamicLocation() for any locations requiring removal. Domain entity owns state machine and resource lifecycle tracking. GameFacade orchestrates multi-facade operations. Facades remain isolated from each other. Services execute individual facade calls.

### Phase 3: Dependent Resource Management System

Implement resource creation pipeline for scenes to create locations and items at spawn time:

**Resource Specifications in Templates:** Scene templates include dependent resource specifications. Location specifications define: relative placement rules (adjacent to base, specific distance, directional preference), pattern-based naming incorporating placement context variables, required properties matching service type, lifecycle strategy determining post-completion behavior. Item specifications define: pattern-based naming incorporating context, uniqueness requirements per scene instance, lifecycle strategy determining removal timing.

**Hex Grid Placement:** When scene creates dependent location, queries hex grid for valid placement following specification rules. Adjacent placement finds first available hex neighboring base location. Directional placement searches specific direction from base. Distance placement finds hex at specified range. Created location added to hex grid, inherits world context, automatically integrates with existing intra-venue travel route generation.

**Pattern Name Resolution:** Resource specifications use patterns incorporating context variables. Upper room location pattern: "[NPC.Name]'s Private Room" resolves to "Elena's Private Room" when Elena is placement NPC. Item pattern: "Room Key" or "[Service] Access Token" resolves contextually. Patterns enable generic specifications producing contextual concrete names.

**Resource Tracking:** Scene maintains lists of created resource IDs. Created locations tracked separately from base location reference. Created items tracked separately from player inventory. Tracking enables cleanup to target only scene-created resources, leaving pre-existing world resources unmodified.

**Reference Resolution:** Situations reference created resources through template indices or pattern identifiers, not brittle ID strings. Access situation requires "created_item[0]" or pattern reference "access_token". Service situation requires "created_location[0]" or pattern reference "service_location". Runtime resolution uses scene's tracking lists to find concrete resource IDs.

This system eliminates pre-existing resource dependencies. Scenes create what they need, use it, clean it up. No broken references when world entities missing or modified.

### Phase 4: Scene-Created Item Lifecycle Integration

Implement complete lifecycle management for items created and managed by scenes:

**Item Creation at Spawn:** Scene reads dependent item specifications from template, creates unique item instances incorporating scene ID in identifier, adds to GameWorld.Items catalog, adds item IDs to scene's CreatedItemIds tracking list. Items guaranteed unique per scene instance.

**Item Granting Through Situations:** Early situations grant scene-created items through choice rewards. Negotiate situation rewards include reference to created access item. Runtime reward application resolves item reference through scene tracking, adds concrete item to player inventory. Player receives contextually-named item visible in inventory display.

**Item Requirements in Progression:** Middle situations require scene-created items to proceed through HasItem requirements. Access situation specifies requirement for created access item. Runtime requirement evaluation resolves item reference through scene tracking, checks player inventory for concrete item. Situation remains locked until requirement met.

**Permanent Rewards Distinction:** Some scene-created items persist as permanent rewards. Investigation scenes create knowledge_fragment items with PlayerOwned lifecycle.

**Item Naming and Context:** Created items use pattern-based naming incorporating placement context. "Room Key" at Elena's inn, "Bath Token" at Marcus's bathhouse, "Healing Permit" at Thalia's temple. Same specification, different concrete names based on NPC and service type. Pattern resolution at creation time produces contextual names.

This eliminates item dependency fragility. Scenes create items they need, grant through rewards, require in progression, remove at completion. No references to pre-existing item catalog entities that might not exist.

### Phase 5: Time Integration and Advancement

Robustly integrate time system with situation completion:

**Situation Time Costs:** Expand SituationTemplate with TimeCost property (segments, hours, days). Rest situation costs 6-8 hours (advances to morning), travel situation costs 2-4 segments (advances time block), instant situation costs 0 time (same moment).

**Automatic Time Advancement:** When situation completes, apply time cost automatically. GameWorld.CurrentDay and CurrentTimeBlock updated. Subsequent situations in arc see updated time (morning instead of evening, next day instead of same day).

**Time-Dependent Narrative:** AI generation receives CurrentTimeBlock as part of location context. Morning generates "morning light" narrative, evening generates "dim tavern" narrative. Same location different atmosphere based on time.

**Time Validation:** Scene completion validates time advanced logically. Rest scene must advance time. Transaction scene should not advance much time. Catches situations incorrectly claiming/not claiming time passage.

Time becomes integral to scene flow, not external system. Scenes orchestrate time passage as part of arc progression.

### Phase 6: AI Integration Per Situation

Build generation system calling AI separately for each situation in active scene:

**Context Bundling Per Situation:** When situation activates (transitions Dormant → Active), collect: NPC entity object (all properties), Location entity object (all properties including CURRENT time), Spot entity object (properties), situation position indicator (opening/intermediate/conclusion), service type or scene theme, player state (current resources, prior choices in this scene), items held relevant to this situation. Package as typed context object.

**Situation-Specific Generation Call:** Invoke AI with context bundle and situation template narrative hints. AI returns: situation description text, choice action texts (one per ChoiceTemplate in this situation), success/failure narrative variants, transition narrative to next situation.

**Direct Storage:** Store generated texts directly on Situation entity properties (Description, per-choice ActionText, outcome narratives). No separate storage, no placeholder replacement (no placeholders exist). Situation now has complete narrative for this beat.

**Sequential Generation:** Each situation generates when activated, not all at scene start. Situation 2 generates only after situation 1 completes (context includes situation 1 outcome). Situation 3 generates after situation 2 (context includes morning time because rest advanced time). Emergent coherent narrative from sequential context-aware generation.

**Fallback Handling:** If AI generation fails for a situation, situation activates with template-default generic text or previous working generation. Scene continues playable, just less narrative richness. AI enhancement, not dependency.

This generates contextually rich narrative for each beat while maintaining mechanical coherence across entire arc.

### Phase 7: Multi-Scene UI Display

Build UI layer displaying all active scenes at each NPC as separate interaction options:

**NPC View Model Transformation:** Change from single-scene pattern (FirstOrDefault + single InteractionLabel) to multi-scene pattern (Where + List<SceneViewModel>). Query GameWorld.Scenes for ALL scenes where PlacedNpc matches NPC and scene has active/selectable situations. Each scene produces one item in list.

**Scene Label Derivation Hierarchy:** For each active scene, derive button label using fallback chain: (1) Scene.DisplayName if explicitly authored, (2) Current Situation.Name if available, (3) Placeholder "Talk to [NPC Name]" as functional default. Never hide functional scene for lacking pretty label - playability trumps aesthetics.

**Navigation Routing Update:** Button clicks must pass (npcId, sceneId) tuple instead of just npcId. Navigation handler uses sceneId for direct Scene lookup, eliminating ambiguity when multiple scenes active at same NPC.

**Physical Presence Always Visible:** NPC cards always display when NPC present at location, representing physical fiction. Interaction buttons conditionally render only when active scenes exist. Zero scenes = NPC visible but no buttons. Two scenes = NPC visible with two buttons.

**Scene Independence Rendering:** Each scene button operates independently. Player can engage "Secure Lodging", complete situations, return to find "Inn Trouble Brewing" still available. Completing one scene never affects visibility of others at same NPC.

This enables rich narrative branching where NPCs serve as hubs for multiple concurrent story threads without hiding available content.

### Phase 8: Tag-Based Spawn System

Implement dependency inversion replacing scene-ID dependencies with abstract player state tags:

**Player Tag Tracking:** Add Tags property to Player entity as List<string>. Tags represent abstract state achievements (LodgingExperienceAcquired, TravelersGuildMember, PatronEstablished). Persists in save data. Queried during spawn evaluation.

**Scene Tag Granting:** Templates specify GrantedTags list (tags to add to player on completion). Scene.ExecuteCleanup iterates GrantedTags, adds each to Player.Tags list. Multiple scenes can grant same tag (tutorial lodging OR procedural lodging both grant "LodgingExperienceAcquired").

**Spawn Condition Evaluation:** Replace CompletedSceneIds checks with RequiredTags checks. Scene becomes eligible when player has all required tags. SpawnEvaluator queries Player.Tags, compares against template RequiredTags, spawns scene when match. No hardcoded scene-ID dependencies.

**Sir Brante Transparency:** Locked scenes display missing tags in "CONDITIONS NOT MET" section. Shows "Requires: LodgingExperienceAcquired" when player lacks tag. Teaches player which state changes would unlock content. "CONDITIONS MET" display shows satisfied tags explaining why scene spawned.

**Tag Naming Conventions:** Tags describe abstract player states, not specific scenes. Good: "CombatVeteran" (any combat). Bad: "CompletedTutorialCombat" (specific scene). Good: "PatronEstablished" (any patron). Bad: "MetOctavia" (specific NPC). Abstract tags enable multiple providers.

**Migration Strategy:** Convert existing CompletedSceneIds spawn conditions to equivalent RequiredTags. Tutorial scenes grant tags matching their teaching purpose. Procedural scenes grant same tags. No content changes required, only spawn condition rewiring.

This eliminates brittle scene-ID coupling while maintaining clear progression dependencies and Sir Brante-style transparent requirement display.

---

## System Robustness Architecture

Procedural content generation is mission-critical infrastructure. If scene generation, instantiation, or progression fails, the entire game fails. This architecture ensures rock-solid reliability through isolation, validation, contracts, and comprehensive testing.

### LET IT CRASH Philosophy

System follows Erlang/Elixir philosophy: succeed completely or fail loudly with full diagnostic information. No graceful degradation, no partial states, no fallbacks hiding problems.

**Crash on Invalid Input:** Parse-time validation encounters malformed template? Throw ValidationException with detailed report. Game won't start until templates fixed. No "skip invalid template and continue" option. Forces content authors to fix issues immediately.

**Crash on Contract Violation:** Scene spawn finds no matching NPC? Throw MissingDependencyException with filter and available entities. Location creation fails? Throw and rollback entire spawn. Situation references non-existent resource? Throw with full context. Stack traces show exactly where and why failure occurred.

**No Partial States:** Atomic operations only. Creating 3 locations? Either all 3 succeed or LET IT CRASH. Spawning scene with 4 situations? Either complete scene with all situations or LET IT CRASH.

**No Fallbacks:** Don't catch exceptions to return default values. Don't substitute placeholder content when generation fails. Don't skip validation steps because "it usually works." Every fallback masks root cause. System crashes force developers to fix actual problems, not paper over symptoms.

**Why This Works:** Mission-critical infrastructure demands correctness. Better to crash during development (with stack traces enabling rapid diagnosis) than ship buggy content that breaks player experience. Crashes during testing reveal problems immediately. No fallbacks means no hidden issues accumulating. Forces systematic resolution of root causes.

### Pure Generation Core (Isolated Subsystem)

Scene generation operates as completely isolated subsystem with zero game world dependencies.

**Inputs:** Entity property objects (NPC with Personality/Demeanor/Authority, Location with Services/Atmosphere/CostModifier), tier integer, player state benchmarks. Entity objects passed as parameters, never queried from game world.

**Process:** Generation methods are pure functions with no side effects. Query entity properties, apply formulas, produce template structures. No world state modification, no service calls, no external dependencies. Deterministic: same inputs always produce identical outputs.

**Outputs:** Complete immutable SceneTemplate structures containing situation sequences, spawn rules, dependent resource specifications, choice structures, cost/benefit formulas, cleanup specifications.

**Zero Coupling:** Generation layer never imports GameWorld, GameFacade, service classes, Unity libraries. Only domain entity definitions for property access. Can compile and test generation independently of entire game.

**Testing Advantage:** Create mock entities with controlled property values. Generate scenes. Assert structure correctness. No game world initialization required. Run 10,000 generation tests in seconds.

### Validation Pipeline (Fail-Fast at Parse)

After archetype generation, before template storage, validation pipeline checks structural coherence. Invalid templates crash at parse, never reach runtime. **LET IT CRASH:** No graceful degradation, no warnings-as-errors toggle, no "skip invalid and continue." Validation failure throws exception immediately with detailed report. Game won't start until all templates valid.

**Structure Validation:** All SituationTemplateIds in SpawnRules exist in situation list. All ChoiceTemplates have valid requirement types. All ChoiceRewards have valid reward types. All resource references resolvable (items granted before required, locations created before accessed). Violation = immediate crash with specific rule, situation, and expected/actual values.

**Dependency Validation:** Dependent location specs have valid placement rules (Adjacent/Distance/Direction). Dependent item specs have valid lifecycle strategies (SceneScoped/PlayerOwned). Item lifecycle chains coherent (can't require item never granted). Location unlock/lock chains coherent (can't unlock location never created). Violation = immediate crash with dependency chain trace.

**Economic Validation:** Total costs don't exceed tier budget. Total benefits justify total costs (benefit/cost ratio within range). Time costs reasonable (rest must advance time, transaction can't claim hours). Resource grants balance costs (restoring 40 stamina must cost resources justifying benefit). Violation = immediate crash with economic breakdown showing imbalance.

**Parse-Time Crashes (Deliberate):** Validation failure throws exception immediately. Invalid template never stored. Developer sees detailed report: which rule violated, which situation/choice involved, expected versus actual values, full stack trace for debugging. Fix template definition, reload, validation passes. No running game with invalid templates lurking. No runtime surprises from malformed content.

**Runtime Safety:** Runtime never encounters invalid templates. All templates passed validation at load. Reduces defensive checks, simplifies logic. Runtime can assume structure validity - no "is this situation ID valid?" checks scattered everywhere.

### Scene Package Contract (Self-Describing Templates)

Every SceneTemplate explicitly declares complete dependency and capability contract.

**External Dependencies:** RequiredNpcPersonalities, RequiredLocationServices, RequiredLocationProperties. PlacementFilter evaluates these during spawn resolution.

**Internal Creations:** DependentLocationSpecs declaring locations scene creates (count, placement, properties, lifecycle). DependentItemSpecs declaring items scene creates (count, naming, lifecycle). Resources guaranteed at spawn.

**Progression Contract:** SituationCount, FlowType, SituationContextRequirements (which situation needs which location/NPC), AutoAdvancePoints, ExpectedCompletionTime.

**Cleanup Contract:** ItemsToRemove, LocationLifecycleStrategies, ExpectedFinalPlayerState.

**Contract Enforcement (LET IT CRASH):** Spawn evaluation checks external dependencies before instantiation. Can't spawn if required NPC personality not found? Throw MissingDependencyException with filter details and available entities. Required location service not available? Throw immediately with location list showing what services exist. Instantiation creates all declared resources or fails atomically - hex coordinate occupied? Throw HexOccupiedE exception and rollback entire spawn. JSON file write fails? Throw FileSystemException and clean up partial files. No partial scene spawning. Progression tracking verifies contract - situation references location not in created list? Throw InvalidLocationReferenceException with situation ID and location ID. Cleanup validation confirms contract fulfilled - items should be removed but still in inventory? Throw CleanupContractViolationException with item list. No warnings, no skipping, no "best effort." Crash with full diagnostic information. Developer fixes root cause.

### Deterministic Generation Testing

Generation must be perfectly reproducible. Same inputs always produce identical outputs.

**Property-Driven Reproducibility:** Generate scene with specific entity properties 1000 times. Compare all outputs byte-for-byte. Must be identical. Any variance = bug.

**Test Fixture Library:** 20 test entities covering property combinations. Elena (Friendly Innkeeper, Low Authority). Thorne (Hostile Guard, High Authority). Marcus (Professional Attendant, Medium Authority). Precise known values.

**Exhaustive Test Matrix:** All archetypes (6) × all fixtures (20) = 120 variants. Assert expected patterns for each. Catches property-specific generation bugs.

**Golden Template Storage:** Store reference templates for each archetype+fixture. On every build, regenerate and compare. Output difference = review required. Prevents accidental formula changes.

**Regression Detection:** Any generation code change triggers full matrix regeneration. Diff shows exactly what changed. Verifies changes match intent.

### Debug Visualization Tools

Developer tools for understanding and diagnosing generation and spawning.

**Scene Template Inspector:** Visual representation showing: situation graph with spawn rule arrows, resource flow diagram (item creation/granting/removal), context requirement map, economic balance breakdown.

**Spawn Diagnosis:** Query "why didn't scene X spawn?" Reports: PlacementFilter evaluation (NPCs matched, locations matched, selection result), spawn condition evaluation (requirements checked, pass/fail), resource availability (hex grid placement valid). Specific failure reason.

**Situation Activation Trace:** Player at location, situation didn't activate. Shows: scenes with situations here, situation states (dormant/active/locked), context matching evaluation (location match, NPC match), requirement evaluation (has items, has tags). Explains exactly why no activation.

**Economic Balance Analyzer:** Calculate per-situation costs/benefits, total costs/benefits, benefit/cost ratio, tier appropriateness. Flags imbalanced scenes.

### State Machine Formalism

Scenes and situations operate as explicit state machines with defined states, transitions, guards, audit trails.

**Scene States:** NotSpawned → Spawned → Active → Completed → CleanedUp. Each state has entry/exit conditions.

**Scene Transitions:** NotSpawned → Spawned requires PlacementFilter match and successful resource creation. Spawned → Active requires finalization. Active → Completed requires final situation finished. Completed → CleanedUp requires cleanup contract fulfilled.

**Transition Guards:** Can't transition without preconditions. Attempting invalid transition = assertion failure with detailed error.

**State Transition Logging:** Every transition logged: timestamp, scene ID, states, trigger reason, player location, player resources. Full audit trail.

**Situation States:** Nonexistent → Dormant → Active/Locked → InProgress → Completed/Failed. State determines visibility and interaction.

**Situation Transitions:** Logged with same detail. Provides situation-level audit trail.

### Resource Lifecycle Audit

Scenes create resources. Audit system tracks creation, usage, cleanup ensuring no leaks or ghosts.

**Creation Tracking:** Scene.CreateDependentResources() logs each resource: location created at hex coordinate with pattern name, item created with unique ID. Adds to Scene.CreatedLocationIds and CreatedItemIds.

**Usage Tracking:** Item granted to player logged. Item required logged (passed/failed). Location unlocked logged. All usage tracked relative to created resources.

**Cleanup Tracking:** Scene.ExecuteCleanup() iterates granted items, removes from inventory, logs removal. Iterates created locations, applies lifecycle strategies, logs application. Validates all created resources accounted for.

**Audit Report:** On completion, reports: resources created, granted to player, consumed during scene, cleaned up. Flags orphans (created but not cleaned) and ghosts (cleanup referenced non-existent).

**Developer Review:** Orphans indicate missing cleanup spec. Ghosts indicate incorrect ID reference. Enables systematic leak detection.

### Context Matching Clarity

Situation activation depends on context matching. System provides explicit rules and transparent evaluation.

**Context Specification:** Each SituationTemplate declares RequiredContext with optional LocationId and NpcId. Null means "any".

**Matching Algorithm:** Player at location with NPC. For each dormant situation: check location match (required vs current), check NPC match (required vs current). Overall match requires both.

**Query API:** GetActivatableSituationsAtContext(locationId, npcId) returns matching situation IDs. CanSituationActivateAt(situationId, locationId, npcId) returns boolean plus detailed reason. GetSituationContextRequirements(situationId) returns requirements without evaluation.

**Evaluation Transparency:** Query API explains exactly why situation didn't activate. Shows location match result, NPC match result, requirement check result, overall verdict.

**Auto-Advance Detection:** When situation completes, Scene.AdvanceToNextSituation() compares next situation context to current. Match = immediate activation, lock player. Mismatch = leave dormant, player must navigate.

### Integration Interface

Game interacts with content generation through clean facades. Internal complexity hidden behind stable API.

**SceneGenerationFacade (Parse-Time):** GenerateSceneFromTemplate(archetypeId, npc, location) invokes generation, validates, returns template or throws. ValidateTemplate(template) runs validation pipeline. GetAvailableArchetypes() returns registered archetypes.

**ContentGenerationFacade (Runtime):** CreateDynamicLocationFile(spec) transforms LocationCreationSpec into JSON matching foundation.json structure, writes to dynamic-content directory, returns DynamicFileManifest or throws FileSystemException. RemoveDynamicLocation(locationId, manifest) removes location from GameWorld and deletes JSON file, returns RemovalResult. GetDynamicContentManifest() returns current manifest of all dynamic files.

**PackageLoaderFacade (Runtime):** LoadDynamicContent(filepath) parses JSON, creates Location entity in GameWorld, integrates with hex grid, returns LocationCreationResult. LoadStaticContent(filepath) existing method using identical parsing logic. No distinction between static and dynamic after entity creation.

**SceneInstanceFacade (Runtime):** SpawnScene(template, npc, location, locationIds) instantiates scene with pre-created location IDs from pipeline, returns scene ID or failure. ActivateScene(sceneId) transitions to Active. GetSituationsAtContext(locationId, npcId) queries matching situations. AdvanceScene(sceneId, situationId) executes progression.

**GameFacade (Orchestrator):** Coordinates multi-facade spawn pipeline: extracts LocationCreationSpec from template → calls ContentGenerationFacade for each → calls PackageLoaderFacade for each → calls SceneInstanceFacade with location IDs. Cleanup pipeline: calls Scene.ExecuteCleanup → receives CleanupResult → calls ContentGenerationFacade.RemoveDynamicLocation for locations requiring removal. Only GameFacade orchestrates. Facades never call each other.

**Clean Boundaries:** Game code never accesses template internals, spawn rules, or situation templates directly. All access through facades. Facades never reference each other, only GameFacade coordinates.

**Interface Stability:** Facade signatures stable across versions. Internal implementation can change without breaking game code.

**Dependency Direction:** Game depends on facades. Facades depend on generation and domain entities. Generation depends only on domain entities. GameFacade depends on facades. No circular dependencies. No facade-to-facade dependencies.

### Strongly-Typed Data Structures (No Dictionaries, No var, No object)

All data passing between systems uses explicitly-typed value objects. Never Dictionary&lt;string, object&gt;, never HashSet&lt;dynamic&gt;, never var hiding types, never untyped objects. Compiler verifies all structure transformations.

**LocationCreationSpec (value object):** NamePattern string with context variable placeholders. PlacementRule enum (Adjacent/Distance/Direction with concrete values). HexOffsetFromBase typed as HexCoordinate struct (X integer, Y integer). Properties typed as LocationProperties object (IsLocked boolean, Atmosphere enum, Services List&lt;ServiceType enum&gt;). InitialLockState boolean. IntegrationStrategy enum (Permanent/Temporary/Reusable). Every field strongly typed, no generic collections of objects.

**DynamicFileManifest (value object):** FilePath string. CreatedTimestamp DateTime struct. ContentChecksum string for integrity verification. ManifestType enum identifying content category. Scene owns list of manifests for all files it created.

**LocationCreationResult (value object):** Success boolean. LocationId string when success true. ErrorReason enum (HexOccupied/DuplicateId/InvalidPlacement/FileSystemError) when success false. ErrorDetails string with human-readable explanation for debugging.

**CleanupResult (value object):** LocationIdsToRemove List&lt;string&gt;. ItemIdsRemoved List&lt;string&gt;. TagsGranted List&lt;string&gt;. ManifestsToDelete List&lt;DynamicFileManifest&gt;. All strongly typed lists, never object arrays.

Pipeline transformation: SceneTemplate (with LocationCreationSpec list) → DynamicFileManifest list → LocationCreationResult list → Scene entity (with location ID strings). Each step transforms strongly-typed input to strongly-typed output. Type safety verified at compile time.

### Test Strategy

Comprehensive testing at every layer. Automated suite catches regressions.

**Unit Tests (Generation - 100+ tests):** Each archetype with mock entities. Assert situation count, choice count, cost formulas, benefit formulas, spawn rules, resource specs, cleanup specs. Test LocationCreationSpec production (correct placement rules, properties, naming patterns). Run in milliseconds, no game world.

**Integration Tests (Instantiation - 10+ tests):** Minimal test world with known entities. Test ContentGenerationFacade: receive spec, produce JSON, validate structure matches foundation.json format. Test PackageLoaderFacade: parse JSON, create Location entity, assert properties correct. Test pipeline: GameFacade orchestrates spec → JSON → location → scene, assert all steps succeed, assert atomic failure behavior (one failure LETS IT CRASH). Generate templates, spawn scenes through full pipeline. Assert resources created correctly, situations instantiated, placement strategies work. Run in seconds with lightweight world.

**End-to-End Tests (Progression - 2+ tests):** Simulate complete scene lifecycles with dynamic location creation and cleanup. Create player, spawn scene through GameFacade pipeline (assert JSON files created, locations added to grid, scene references correct IDs). Simulate situation completions, player navigation to created locations, context matching triggering activation. Assert advancement, context handling, item lifecycle. Test cleanup: scene completes, assert cleanup result correct, GameFacade removes locations with TemporaryRemove strategy (assert GameWorld updated, JSON files deleted), assert PermanentLock strategy leaves location on grid but locked. Run in seconds covering representative arc patterns including dynamic resource lifecycle.

---

## Critical Constraints

### Scene Archetype Complexity

Scene archetypes are substantially more complex than situation archetypes. Situation archetype generates 2-4 ChoiceTemplates (single decision point). Scene archetype generates: 3-7 SituationTemplates each with 2-4 ChoiceTemplates, SpawnRules governing flow, dependent location specifications with placement rules, dependent item specifications with lifecycle strategies, complete item lifecycle patterns, and cleanup orchestration (complete multi-situation arc).

Expect scene archetype generation methods to be 100-300 lines implementing complete pattern logic. This is significant engineering effort. Start with simplest archetypes (linear service scenes), validate thoroughly, then tackle complex patterns (branching investigations, recursive crises).

Dependent resource specifications add complexity. Archetype must determine: how many locations to create, where to place them relative to base, what properties they need, what lifecycle strategy to use, how many items to create, what naming patterns to use, when to grant/require/remove items. These specifications must integrate coherently with situation structure and progression flow.

Scene archetypes are reusable mechanical art. Once "service with location access" archetype exists with complete dependent resource specifications, applies to lodging/bathing/healing/storage/training contextually. Effort justified by infinite reuse with zero pre-authored world dependencies.

### HIGHLANDER for Scene Arcs and Tag Dependencies

All situations within scene exist in GameWorld.Situations flat list, not nested in Scene property. Scene stores situation ID references. Situation stores Scene reference for parent queries. No duplication.

SpawnRules reference situation TEMPLATE IDs (from template), not situation instance IDs (runtime). Runtime spawn logic looks up template by ID, instantiates NEW situation from template, adds to GameWorld.Situations. Template IDs stable, instance IDs unique per spawn.

Player tags exist once in Player.Tags list. Tags stored with tagId, scope (Universal/Regional), regionId (if Regional). Spawn evaluation queries tags with scope matching: Universal tags always match, Regional tags match only when regionId equals current region. No duplicating tag data across scenes.

Scene dependencies expressed through tags, not scene IDs. Scene A grants InnkeeperRapport tag (Regional). Scene B requires InnkeeperRapport OR TravelersGuildMember. System matches capabilities to requirements. Multiple scenes can grant same tag - dependency inversion eliminates brittleness.

### Template Immutability

Scene templates never modify after parsing. SpawnRules list, SituationTemplates list, CleanupPatterns all immutable. Scene instances mutable (progression state, completion tracking) but never modify templates.

This enables safe reuse. One "Secure Lodging" scene template generates hundreds of lodging scene instances at different inns with different NPCs. All reference same template structure, each has unique runtime state.

### AI Boundary - Entity Context Not Mechanics

AI receives: NPC entity object (personality, background, bond, history), Location entity object (atmosphere, time, properties), Spot entity object (privacy, accessibility), situation position in arc (opening/middle/conclusion), service type or theme. Full entity contexts with rich properties.

AI does NOT receive: cost formulas, requirement thresholds, spawn rules, mechanical balance ratios, resource restoration amounts. These are game logic concerns. AI generates narrative describing WHAT happens, not determining COSTS.

AI returns: pure narrative strings (situation descriptions, choice texts, outcome narratives). No mechanical decisions. Generated text stored directly on Situation properties, no transformation needed.

### Entity References Are Strongly Typed

Scene stores NPC entity object reference (for AI context), Location entity object reference (for AI context), LocationSpot entity object reference (for access control), PLUS corresponding IDs for each (for serialization).

Object references are primary runtime mechanism. AI generation, requirement evaluation, spawn logic all use typed objects with full property access. IDs only for save/load.

This dual storage (ID + object) acceptable because: same concept, different purposes, both populated once at instantiation, no ongoing synchronization risk.

---

## Key Files & Their Roles

### SceneArchetypeCatalog (NEW)

Generates complete multi-situation scene structures from archetype IDs with entity properties driving contextual generation. Called exclusively during parse phase.

**Method Signature Pattern:** Each archetype generation method receives: archetype ID, tier, strongly-typed entity objects (NPC, Location, LocationSpot), player state benchmarks. NOT just entity IDs - actual entity objects with queryable properties.

**Property Query Logic:** Generation queries entity properties to determine mechanical values. npc.Demeanor scales challenge difficulty and payment costs. location.Services determines benefit type (Lodging=stamina, Bathing=cleanliness). spot.Privacy multiplies costs. Formulas reference properties, not hardcoded values.

**Output Structure:** Returns complete SceneTemplate containing: embedded List<SituationTemplate> (4 situations for service archetype), each SituationTemplate with List<ChoiceTemplate> including tag requirements and tag grants, SpawnRules defining situation flow, item grant/consume/remove patterns, state modification logic, cleanup requirements, tag granting specifications in choice rewards (InnkeeperRapport Regional tag, BathhouseRapport Regional tag, or service-appropriate regional standing enabling future interactions). Complete arc structure from property queries with dependency inversion through tags.

**Tutorial Usage:** Parser resolves concrete entity IDs (elena_innkeeper, tavern_common_room, tavern_upper_floor) to entity objects. Passes objects to generation method. Receives complete scene structure. No situations or choices in JSON, all generated from properties.

**Procedural Usage:** Placement evaluator filters entities by categorical properties (Personality=Innkeeper, Services=Lodging, Privacy=Medium). Selects matching entities. Passes entity objects to generation method (SAME METHOD). Receives contextually appropriate scene structure. Identical generation path.

**Property-Driven Variation:** Elena (Friendly) generates easy challenge (12 Focus). Thorne (Hostile) generates hard challenge (25 Focus). Same formula (tierBase × demeanorMultiplier), different property values, contextually appropriate difficulty. Economic balance maintained via multiplicative scaling.

**This Catalog is Complex:** Each archetype generation is 100-300 lines implementing complete pattern with property queries. Service archetype must: query NPC demeanor for difficulty, query location services for benefit type, query spot privacy for cost premium, generate 4 situations with choices, define spawn rules, manage item lifecycle, ensure cleanup. Significant engineering but unlimited reuse.

### SceneTemplate

Immutable scene structure defining complete self-contained package. Two authoring patterns:

**Tutorial (Concrete Entities):**
```json
{
  "id": "tutorial_lodging",
  "archetypeId": "service_with_location_access",
  "tier": 0,
  "isStarter": true,
  "concreteNpcId": "elena_innkeeper",
  "concreteLocationId": "tavern_common_room"
}
```

Only external dependencies specified. Parser resolves entity IDs to objects, passes to archetype generation. Generation produces complete package including dependent resource specifications.

**Procedural (Categorical Filters):**
```json
{
  "id": "lodging_service",
  "archetypeId": "service_with_location_access", 
  "tier": 1,
  "placementFilter": {
    "npcPersonalities": ["Innkeeper"],
    "locationServices": ["Lodging"]
  },
  "spawnConditions": {
    "minCoins": 20,
    "requiredTags": [
      { "tagId": "InnkeeperRapport", "scope": "Regional", "matchCurrentRegion": true },
      { "tagId": "TravelersGuildMember", "scope": "Universal" }
    ],
    "tagMatchMode": "Any"
  }
}
```

Categorical requirements only. Tag-based spawn conditions check player state: InnkeeperRapport (Regional, current region) OR TravelersGuildMember (Universal). Dependency inversion - scenes require capabilities, not specific prior scenes. Placement evaluator finds matching entities, passes objects to archetype generation. Generation produces identical package structure.

**Template Package Contents:**

Templates contain embedded SituationTemplates with ChoiceTemplates, SpawnRules defining situation flow, dependent location specifications (relative placement, pattern naming, property requirements, lifecycle strategies), dependent item specifications (pattern naming, uniqueness rules, lifecycle strategies), cleanup specifications referencing created resources, player tag granting specifications (tags to grant on completion enabling future spawns), narrative hints for AI generation.

**Both Paths Identical After Entity Resolution:** Tutorial parser and procedural evaluator both produce: NPC object, base Location object. Both invoke identical archetype generation method. Both receive identical SceneTemplate output structure including complete self-contained package specifications.

**Templates Contain Zero Narrative Text:** Pure structure, mechanics, and resource specifications. AI generates all narrative from entity properties at situation activation. Template specifies archetype pattern, entity requirements, and complete dependency graph. Generation produces mechanical structure and resource specifications. AI produces contextual narrative.

### Scene Entity

Runtime scene instance tracking: CurrentSituationId (progression through arc), List<string> SituationIds (references to GameWorld.Situations), CreatedLocationIds (locations created via GameFacade pipeline at spawn), CreatedItemIds (items scene created at spawn), GrantedTags (player tags this scene will grant on completion), PlacedNPC/PlacedLocation (strongly-typed entity references AND IDs for external dependencies), State (Provisional/Active/Completed), Template (reference to SceneTemplate), DisplayName (optional authored label for UI).

Owns state machine methods: **AdvanceToNextSituation** (progression control with context-aware auto-advance - returns flag indicating whether next situation shares context for seamless cascade or requires world navigation, compares against both base and created locations), **ShouldActivateAtContext** (checks if current situation should activate at given location/NPC context including created locations, supporting scene persistence across navigation), **ExecuteCleanup** (returns CleanupResult specifying which created locations need removal - GameFacade then orchestrates ContentGenerationFacade calls, removes created items from player inventory, grants player tags enabling future scene spawns, restoration logic), **IsComplete** (completion detection). Domain entity owns lifecycle state machine and resource tracking. GameFacade orchestrates multi-system operations for resource creation and removal. Scene provides tracking and coordination, not direct creation.

Context tracking: Scene knows which location/NPC each situation requires, comparing both external base location and scene-created locations. Compares consecutive situations to determine auto-advance behavior. Same context = immediate transition, player locked in flow. Different context = exit to world, scene persists with updated CurrentSituationId, reactivates when player reaches matching context.

Resource ownership: Scene maintains authoritative lists of created resources and tags to grant. Situations reference created resources through tracking lists, not brittle ID strings. Cleanup targets only scene-created resources, leaving pre-existing world entities unmodified. Tags granted enable dependency inversion - future scenes require tags, not scene IDs. Complete lifecycle control from creation through cleanup through consequence propagation.

### Situation Entity

Runtime situation instance within scene: Template reference (for lazy action instantiation), ParentScene reference (for context queries), InstantiationState (Deferred/Instantiated), LifecycleStatus (Locked/Selectable/InProgress/Completed/Failed), GeneratedDescription (AI-produced narrative text stored directly), per-choice ActionTexts (AI-generated strings for each choice).

Situations know position in arc via template properties. Opening situations introduce, conclusion situations wrap up. Position affects narrative generation and mechanical behavior.

### SituationTemplate

Embedded in SceneTemplate, not separate top-level entity. Defines single situation: List<ChoiceTemplate> (player options at this beat, each with optional Requirements determining availability and Rewards specifying outcomes including tag granting), TimeCost (how much time this beat consumes), ItemGrants/ItemRequirements (lifecycle participation), NarrativeHints (for AI generation specific to this beat), PositionIndicator (opening/intermediate/conclusion).

ChoiceTemplates can specify requirements (HasItem, MinCoins, MinStat, RequiredTags with scope and region matching) that lock choices until met. RequiredTags check Player.Tags list: Universal tags (TravelersGuildMember, MerchantsGuildMember, MedicalKnowledge) always match, Regional tags (InnkeeperRapport, BathhouseRapport, TempleStanding) match only when stored regionId equals current region. Locked choices remain visible with grayed presentation showing exact requirement and consequence preview, teaching players about paths not taken and strategic value of credentials. ChoiceRewards grant player tags enabling future scene spawns through dependency inversion - scenes require capabilities, not specific prior scenes.

Contains NO narrative text. AI generates all narrative from entity contexts and hints.

### SpawnRules

List of situation transitions defining scene flow. Each transition specifies: SourceSituationTemplateId (which template), DestinationSituationTemplateId (which template spawns), Condition (success/failure/always), RequirementOffsets (if applicable). Scene instance executes these rules to advance progression.

Linear scenes have simple rules (A→B→C→D). Branching scenes have conditional rules (A→B if success, A→C if failure). Hub-and-spoke scenes have parallel rules (A→[B,C,D], [B,C,D]→E when all complete).

### PlacementFilterEvaluator

Resolves two-entity placement for scenes: NPC + base Location.

Receives PlacementFilter with NPC requirements (personality types, demeanor ranges, authority levels), Location requirements (service types, atmosphere types, cost modifiers). Queries GameWorld for matching NPCs, filters by locations where NPC present with required properties. Returns (NPC, Location) pair or null if no match.

Applies selection strategies when multiple matches exist: prefer highest bond NPC for relationship continuity, prefer least recently visited location for variety, prefer closest location for travel efficiency, weighted random for organic distribution. Strategy determined by template preference or gameplay context.

Scene receives minimal external bindings from evaluator. Everything else created by scene from dependent resource specifications. Two-entity resolution sufficient because scene creates additional locations and items as needed.

### SceneInstantiator

Factory creating scene instances from templates. Does NOT create dependent resources - that's GameFacade's orchestration responsibility.

Resolves placement through categorical filter evaluation: queries GameWorld for NPC matching personality filter, queries locations where NPC present matching service filter, selects concrete (NPC, Location) pair using selection strategy (highest bond, least recent, weighted random). External dependencies resolved.

Instantiates scene from template: creates Scene entity with reference to template, binds NPC and Location entity references, marks as Provisional state. **Does not create locations or items** - receives pre-created location IDs from GameFacade after pipeline completes. Populates Scene.CreatedLocationIds with provided IDs. Populates Scene.CreatedItemIds from item creation (items created directly, locations created through multi-system pipeline).

Instantiates all situations in scene simultaneously as Dormant entities in GameWorld.Situations flat list. Each Situation gets reference to SituationTemplate, reference to parent Scene, placement context from template. Situations reference dynamically-created locations by their final GameWorld location IDs. Scene.SituationIds list populated with created situation IDs. Marks first situation as current via CurrentSituationId. No actions instantiated yet, just situation structure in dormant state.

At finalization when scene transitions Provisional to Active, situations remain dormant until player reaches matching context. Scene references all resources by final GameWorld IDs. Resources guaranteed to exist because pipeline completed successfully before instantiation.

Separation of concerns: SceneInstantiator creates scene domain entities. GameFacade orchestrates resource creation pipeline. ContentGenerationFacade creates JSON files. PackageLoaderFacade creates Location entities. Clean boundaries.

### ContentGenerationFacade

Transforms strongly-typed resource specifications into JSON files matching static content structure.

**CreateDynamicLocationFile(spec):** Receives LocationCreationSpec value object. Transforms spec fields into JSON structure matching foundation.json format: id from pattern, name from pattern with context resolution, hexCoordinate from spec, properties object with typed fields. Writes JSON to /dynamic-content/locations/ directory with generated filename scene_{sceneId}_location_{index}.json. Returns DynamicFileManifest on success (filepath, timestamp, checksum) or throws FileSystemException on failure. Never imports GameWorld, SceneGenerationFacade, or PackageLoaderFacade. Only knows JSON structure and file operations.

**RemoveDynamicLocation(locationId, manifest):** Receives location ID and its creation manifest. Validates location is dynamic (in manifest). Removes from GameWorld.Locations collection. Deletes JSON file at manifest filepath. Updates DynamicContentManifest removing entry. Returns RemovalResult indicating success. Handles cleanup phase of resource lifecycle.

**GetDynamicContentManifest():** Returns current manifest listing all dynamically-created content files with metadata. Used for world reset and debugging.

Facade responsibility: JSON generation and file management only. Does not parse, does not create GameWorld entities, does not understand scenes or situations. Pure data transformation and file I/O.

### PackageLoaderFacade

Parses JSON content files and creates GameWorld entities. Treats dynamic content identically to static content.

**LoadDynamicContent(filepath):** Receives filepath to JSON file. Parses JSON using identical parser logic as static foundation.json loading. Extracts location data, creates Location entity with all properties. Adds to GameWorld.Locations collection. Integrates with hex grid at specified coordinates. Spatial systems automatically generate intra-venue travel routes. Returns LocationCreationResult on success (locationId) or failure (errorReason enum: HexOccupied, DuplicateId, InvalidPlacement, ParseError). Never imports SceneGenerationFacade or ContentGenerationFacade. Only knows parsing and entity creation.

**LoadStaticContent(filepath):** Existing method for static content. Uses same parsing logic as LoadDynamicContent. No special dynamic handling - both paths identical after JSON parse.

Facade responsibility: JSON parsing and entity instantiation only. Does not generate JSON, does not create files, does not understand scenes. Pure parsing and GameWorld integration.

Dynamic and static content converge at this boundary: both become Location entities in GameWorld.Locations, indistinguishable after creation.

### AI Generation Service (FUTURE)

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

**Dynamic location creation enables true scene self-containment.** Scenes don't depend on pre-authored locations beyond base. GameFacade orchestrates multi-system pipeline: ContentGenerationFacade produces JSON files, PackageLoaderFacade creates Location entities, SceneInstanceFacade references final IDs. Facades isolated from each other - only GameFacade coordinates. Strongly-typed value objects (LocationCreationSpec, DynamicFileManifest, LocationCreationResult, CleanupResult) ensure compile-time verification. Static and dynamic locations indistinguishable after creation. Complete lifecycle from specification to JSON to entity to cleanup.

**Reliability is paramount.** Pure generation core with zero dependencies enables isolated testing. Parse-time validation prevents structural errors from reaching runtime. Explicit contracts and state machines provide complete traceability. Comprehensive automated test suite (1000+ unit, 100+ integration, 20+ end-to-end tests) catches regressions immediately. Debug visualization tools enable rapid diagnosis. Atomic pipeline guarantees complete success or clean failure. This system architecture ensures rock-solid procedural content generation where failure is not an option.