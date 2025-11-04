# Handoff: Context-Aware Procedural Scene Generation

## Problem & Motivation

### The Scaling Crisis

Content authoring for multi-situation narrative arcs creates exponential burden. A single tutorial scene demonstrating lodging at an inn requires 576 lines of JSON defining four situations, each with 2-4 explicit choices, complete with hand-crafted requirements, costs, rewards, narrative text, spawn rules, and transitions. Every service location (inn, bathhouse, healer's temple, storage facility, training ground) would need similarly explicit hand-authoring.

This approach doesn't scale. Each merchant needs a custom transaction scene. Every checkpoint needs a custom gatekeeper scene. Every service requires bespoke multi-situation orchestration. The authoring burden grows linearly with content while maintaining mechanical consistency becomes nearly impossible.

The business need is **entity-agnostic scene templates**. Define a scene archetype once (service with location access), bind it to specific entities (Elena the innkeeper at Brass Bell Inn), and have the game procedurally generate appropriate situations and choices by reading categorical properties from those entities. The same 12-line template should produce contextually appropriate content whether bound to a DEVOTED innkeeper in a bustling tavern or a MERCANTILE attendant in a private bathhouse.

### The True Challenge

The naive solution generates generic patterns that ignore context. A service transaction with any NPC feels identical - same costs, same choices, same narrative tone. The real challenge is making procedural generation **context-aware**: reading NPC personality types, Location properties, Player state, and generating situations that feel hand-crafted for that specific combination while remaining mechanically balanced and narratively coherent.

The tutorial demonstrates this need perfectly. Elena (DEVOTED personality, Innkeeper profession) at Brass Bell Inn (Commercial, Busy, Restful properties) with a player who has exactly 10 coins should generate fundamentally different choices than a MERCANTILE bathhouse attendant in a Private, Quiet location with a wealthy player. Both use the service_with_location_access archetype, but entity properties drive divergent generation.

## Architectural Discovery

### Three-Tier Timing Model

The codebase enforces strict temporal separation governing when content decisions occur:

**Parse Time** happens at game initialization when JSON loads. Immutable templates are created, catalogues translate categorical IDs into concrete structures, and no runtime state exists yet. Catalogues are imported ONLY in parser classes, never in gameplay facades or managers. This phase populates GameWorld collections with pre-generated archetypes.

**Instantiation Time** occurs when spawn conditions trigger scene creation. Templates become runtime instances placed at concrete entities. The provisional pattern creates shallow scenes first (metadata only, no situations), then finalizes them when player commits (instantiates all situations simultaneously as dormant entities). SpawnConditions evaluate PlayerState, WorldState, and EntityState eligibility.

**Query Time** happens when player enters context. Dormant situations activate and instantiate their actions. Choices become visible. Player interaction advances state machine. This is the only moment when UI reads game state - all prior phases prepared data structures, but nothing renders until query time.

This timing model prevents premature activation, ensures fresh evaluation at each beat, and maintains efficient memory usage. Catalogues called at runtime violate architectural boundaries. Actions created before player sees them waste memory and prevent dynamic evaluation.

### Parser-JSON-Entity Triangle

The codebase follows a strict translation pattern across five layers that must stay synchronized:

JSON source data contains categorical properties (personality types, location properties, archetype IDs). DTO classes deserialize JSON with simple string/int/list properties. Parser code translates categorical strings to domain types, calls catalogues with categorical IDs, performs validation, and creates domain entities. Domain entities use strongly-typed enums, concrete objects, and encapsulate behavior. Runtime code (services, facades, UI) queries domain entities directly, never touches DTOs or catalogues, and relies on parse-time translation being complete.

When any property changes, all five layers update holistically. Adding a new choice type requires JSON field, DTO property, parser translation logic, domain enum value, and facade handling. This prevents desynchronization and enforces fail-fast behavior - invalid data crashes at parse time, not during gameplay.

### Catalogue Constraint Pattern

Catalogues are parse-time ONLY translation tools, never runtime services. The SituationArchetypeCatalog demonstrates this: it contains a GetArchetype method returning mechanical definitions (stats required, coin costs, challenge types) that parser uses to generate ChoiceTemplates. Once parsing completes, the catalogue is never touched again - all data lives in GameWorld collections.

The constraint has philosophical weight: catalogues encode design knowledge (what makes confrontation different from negotiation), not gameplay state (which confrontation is active now). Runtime logic queries pre-populated templates, not regenerates from catalogues. This separation keeps design-time translation distinct from gameplay-time execution.

Violation symptoms appear as catalogue imports in facade or manager classes, GetArchetype calls during gameplay, or runtime translation of categorical strings. Correct architecture has catalogues imported ONLY in parser namespace, called ONLY during JSON deserialization, returning structures that parser embeds in domain entities.

### HIGHLANDER Principle (One Concept, One Representation)

Every domain concept exists in exactly one authoritative location. Situations within scenes live in GameWorld.Situations flat list, not nested in Scene.Situations property. Scene stores List<string> SituationIds as references. Querying situations filters flat list by IDs. This prevents duplication where situations exist in multiple locations simultaneously.

The principle has three implementation patterns. Persistence IDs with runtime navigation uses BOTH ID and object when property comes from JSON and needs frequent access - ID is immutable after parsing, object resolved once by parser, runtime uses ONLY object reference. Runtime-only navigation uses object ONLY with NO ID when property represents runtime state that changes during gameplay - no ID property exists, avoiding desynchronization risk. Lookup on demand uses ID ONLY with NO object when property comes from JSON but has infrequent access - saves memory by not caching object.

Violation symptoms include redundant storage (both object and ID for runtime state), inconsistent access (some files use object while others perform ID lookup), or derived properties (ID computed from object). The decision tree asks: From JSON? Frequent access? If both yes, use Pattern A (both ID and object). If from JSON but rare access, use Pattern C (ID only). If runtime state only, use Pattern B (object ONLY).

### Entity Initialization (Let It Crash)

Collections initialize inline on entity declarations, not in constructors or parsers. Required strings initialize to empty string. Nullable properties explicitly marked with question mark exist only for validation patterns. Parser assigns directly to properties, never uses null-coalescing operators. Game logic trusts entity initialization, never checks null on collections or required strings. ArgumentNullException appears ONLY for null constructor parameters, not property access.

This philosophy forces fixing root causes. Missing JSON data crashes with descriptive errors at parse time. Defensive null checks hide problems and delay discovery. Single source of truth lives in entity initializers - parsers don't duplicate this knowledge. Less code results from eliminating redundant safety checks throughout codebase.

### State Machine Orthogonality

Scenes track multiple independent state dimensions that must not conflate. Scene lifecycle (Provisional, Active, Completed, Expired) governs whether scene appears in world. Situation activation (Dormant, Active) governs whether situation instantiates actions. Situation lifecycle (Locked, Selectable, InProgress, Completed, Failed) governs specific situation status. Progression tracking maintains which situation is current, which completed, what spawn rules triggered, what items granted or consumed, and what world state modifications applied.

These dimensions are genuinely independent. A scene can be Active with current situation Completed while waiting for spawn rule to trigger next situation which remains Dormant. Or situation can be Active (actions instantiated) but Locked (requirements not met yet) because player needs item from prior situation first. Conflating these creates bugs where activation state confused with lifecycle state.

Clear naming prevents collision. Original codebase had problems with "Active" meaning both lifecycle state AND activation state. Separate vocabularies maintain clarity: scenes have lifecycle, situations have activation, both have independent progression tracking.

## Domain Model Understanding

### Scene as Multi-Situation Arc

Scenes are self-contained narrative arcs with beginning, middle, end. Not single moments, but complete story beats spanning multiple player decisions and world state changes. The tutorial demonstrates this: four situations flow sequentially (negotiation, payment, rest, departure), each building on prior choices, culminating in complete lodging experience.

Beginning situations establish premise - innkeeper conversation, guard confrontation, merchant pitch. Player learns what scene offers and what it costs. Choice determines arc progression - challenge versus payment versus refusal paths. Middle situations deliver on promise - access locked area, perform service, achieve goal. May involve navigation (move to location), time passage (wait for result), or resource application (consume items or energy). Mechanical benefits apply here. End situations wrap up arc - return to normal, cleanup temporary state, receive final consequences. Departure from room, leaving checkpoint, completing transaction. World restored to coherent state for next visit.

Situations know position via template properties. IsOpening marks first situation, IsIntermediate marks middle beats, IsConclusion marks final situation. This affects narrative generation (openings introduce, conclusions wrap up) and mechanical behavior (openings may have requirements, conclusions may have cleanup).

### Situation Sequencing via Spawn Rules

Situations don't activate arbitrarily. SituationSpawnRules on SceneTemplate govern flow through declarative data defining transitions between situation IDs. Linear progression has situation A completing spawning situation B, then B completing spawning C - strict sequence, no branching, used for service arcs and tutorials. Conditional branching has situation A completion spawning different situations based on outcome - success spawns B, failure spawns C, both valid continuations with different narrative tones.

Hub-and-spoke has initial situation spawning multiple parallel situations where player chooses order, all must complete before finale. Used for investigation scenes gathering evidence at multiple locations, preparation sequences acquiring items from different sources, or relationship building through multiple conversation topics. Recursive spawning allows situation completion to dynamically spawn additional situations within same scene - investigation gathering evidence might spawn follow-up investigation based on discovery, crisis situation might spawn escalation if player choices worsen things.

Scene domain entity owns state machine via AdvanceToNextSituation method. Not scattered across services as orchestration logic. Domain entity queries own SpawnRules, finds transitions for completed situation, activates next situations, updates CurrentSituationId, marks scene complete if no transitions remain.

### Item Lifecycle Within Scene Scope

Scenes frequently grant temporary items enabling progression. Grant phase has early situation rewarding player with item - room key from innkeeper negotiation, permit from guard approval, token from merchant purchase. Item added to player inventory. Consumption phase has middle situation requiring item to proceed - accessing locked location consumes room key, presenting permit allows checkpoint passage, showing token enables service. Item checked against requirements, consumed if appropriate. Removal phase has final situation cleaning up temporary item if still held - departure removes room key even if player didn't use it yet, expired permits removed, tokens reclaimed. Ensures items don't persist beyond scene scope inappropriately.

Item lifecycle is scene-managed. Reward template grants item, requirement template checks for item, cleanup logic removes item. Same item potentially granted by different scenes (multiple inns grant room_key), but each scene instance manages its granted items independently. Scene tracks items granted by this instance, not all items player possesses.

Items can also be permanent rewards. Completing investigation grants knowledge_fragment persisting forever. Scene distinguishes temporary items (scene-scoped access tokens), permanent items (persistent rewards), and consumable items (used once then depleted).

### Categorical Properties as Generation Inputs

The codebase provides rich categorical properties across entity types enabling context-aware generation without hardcoding specific entity IDs.

NPCs have PersonalityType enum (DEVOTED, MERCANTILE, PROUD, CUNNING, STEADFAST) encoding fundamental approach to interactions. DEVOTED personalities offer emotional connection paths where helping builds bonds. MERCANTILE personalities focus on transactional exchanges without emotional investment. PROUD personalities gate content behind respect and hierarchy. CUNNING personalities trade in secrets and hidden knowledge. STEADFAST personalities value reliability and moral consistency.

NPCs have Profession enum (26 values: Innkeeper, Merchant, Guard, Priest, Laborer, etc.) determining expertise and service offerings. Innkeepers provide lodging services. Merchants enable trade. Guards control access to restricted areas. Priests offer healing. Professions combine with personality to create diverse characterizations - DEVOTED innkeeper (Elena) helps struggling travelers, MERCANTILE innkeeper demands payment upfront.

Locations have LocationPropertyType enum (98 categorical values) describing atmosphere, privacy, social class, authority presence, and functionality. Privacy properties (Private, Discrete, Public, Exposed) affect conversation flow and available choices. Atmosphere properties (Quiet, Loud, Warm, Shaded) provide contextual modifiers. Functional properties (Commercial, SleepingSpace, Rest, Transit) enable specific action types. Personality synergy properties (NobleFavored, CommonerHaunt, MerchantHub, SacredGround) create modifiers when combined with matching NPC personalities.

Player state includes Stats (Insight, Rapport, Authority, Diplomacy, Cunning at levels 1-8), Resources (Coins, Health, Hunger, Stamina, Focus, Resolve), Scales (six moral/behavioral axes from -10 to +10: Morality, Lawfulness, Method, Caution, Transparency, Fame), and History (CompletedSceneIds, ChoiceHistory, BondStrength per NPC, LocationVisits, NPCInteractions). These properties drive requirement formulas, unlock advanced content, and trigger contextually appropriate situations.

Synergies emerge from combinations. DEVOTED NPC at SacredGround location creates flow bonus. High Morality player at altruistic situations unlocks moral choice paths. Low Coins triggers poverty fallbacks. High BondStrength with NPC unlocks relationship-gated advanced options. Locations with Commercial property enable work-for-service exchanges.

### SceneArchetype as Reusable Mechanical Pattern

Scene archetypes define complete multi-situation arc structures reusable across different entity combinations. Service with location access has four situations (negotiate, access, service, depart) usable for lodging, bathing, healing, storage, and training - same structure, different service benefits and narrative context. Transaction sequence has three situations (browse, negotiate, complete) usable for shopping, selling, and trading - mechanical pattern consistent, inventory and pricing vary. Gatekeeper sequence has two situations (confront, pass) usable for checkpoints, restricted areas, and authority challenges - confrontation mechanics identical, narrative theming differs.

Archetypes are mechanical arc patterns devoid of specific narrative. Same archetype generates lodging at inn (rest benefit), bathing at bathhouse (cleanliness benefit), healing at temple (health benefit) with identical situation flow but different service mechanics and contextual narrative. Entity properties provide the variation - DEVOTED innkeeper creates emotional negotiation paths, MERCANTILE bathhouse attendant creates transactional exchanges, PROUD temple priest gates service behind respect requirements.

Players learn pattern recognition through repetition with variation. Encountering service with access at inn, recognizing same structure at bathhouse, applying knowledge to new contexts. Structural familiarity speeds comprehension while narrative variety prevents repetition staleness. Strategic planning becomes possible - knowing lodging scene requires negotiation then access then rest then departure, player can assess resource expenditure before committing. Perfect information at arc level, not just situation level.

Archetypes enforce mechanical consistency. All service scenes balanced similarly - lodging costs X and restores Y stamina, bathing costs X and restores Y cleanliness, healing costs X and restores Y health. Ratios consistent, benefits different, balance maintained. Archetypes prevent unbalanced procedural generation by encoding design-time balance formulas.

## Current State Analysis

### Existing Situation-Level Procedural Generation

The SituationArchetypeCatalog demonstrates working parse-time archetype expansion. When parser encounters SituationTemplate with archetypeId field, it calls GetArchetype with categorical ID string (confrontation, negotiation, investigation, social_maneuvering, crisis). Catalogue returns SituationArchetype struct containing mechanical definitions: which stats tested, coin costs, challenge types, fallback penalties. Parser's GenerateChoiceTemplatesFromArchetype method uses this struct to build four ChoiceTemplate domain entities: stat-gated (best outcome, free if requirement met), money (guaranteed success, expensive), challenge (variable outcome, risky), fallback (poor outcome, always available).

This pattern works. Fifteen situation archetypes exist (confrontation, negotiation, investigation, social_maneuvering, crisis, plus ten expanded patterns). Twenty procedural scene templates in 22_procedural_scenes.json use single-situation generation. Each template has ONE SituationTemplate with archetypeId, parser expands to four choices, game plays correctly.

The limitation is scope - archetypes generate choice structures within single situations, not multi-situation arcs. Tutorial scene has FOUR situations that must coordinate: negotiation grants room key, access consumes key and unlocks location, service restores resources and advances time, departure cleans up key and re-locks location. This coordination logic hand-authored across 576 lines of JSON. No catalogue generates it.

### Multi-Situation Infrastructure Exists

The Scene entity has complete state machine implementation. AdvanceToNextSituation method queries SpawnRules, finds transition for completed situation, gets destination situation IDs, updates CurrentSituationId, marks scene complete if no transitions. IsComplete method returns true when CurrentSituationId is null or State equals Completed. Scene owns progression through situations via domain entity behavior.

SituationSpawnRules entity defines Pattern enum (Linear, HubAndSpoke, Branching, Converging, etc.), InitialSituationId marking where player starts, Transitions list defining source to destination mappings with conditions, CompletionCondition for scene-level completion tracking. SituationTransition entity specifies SourceSituationId, DestinationSituationId, Condition enum (Always, OnChoice, OnSuccess, OnFailure), SpecificChoiceId for choice-triggered transitions.

Tutorial JSON demonstrates complete usage. Four SituationTemplates defined inline with spawn rules establishing linear progression. Scene state machine executes rules during gameplay. Multi-situation flow works end-to-end. The problem isn't missing functionality - it's that tutorial hand-authors what should be procedurally generated from archetype plus entity context.

### PlacementFilter Supports Entity Resolution

PlacementFilter entity supports categorical selection across entity types. PlacementType enum distinguishes NPC, Location, Route targets. NPC filters include PersonalityTypes list (categorical personality matching), MinBond and MaxBond for relationship requirements, NpcTags for additional categorization. Location filters include LocationProperties list (98-value enum for categorical matching), LocationTags, DistrictId, RegionId for spatial filtering. Route filters include TerrainTypes, RouteTier, DangerRating for path selection.

Selection strategies determine tiebreaking. WeightedRandom picks randomly among matches (default). Closest uses hex distance calculation. HighestBond selects NPC with strongest relationship (NPC-only). LeastRecent prefers entities player hasn't interacted with recently (uses interaction history timestamps).

Tutorial's placementFilter demonstrates concrete binding: placementType NPC with npcId elena explicitly binds to specific entity. This override mechanism exists for tutorial, but categorical filtering (personalityTypes DEVOTED, locationProperties Commercial) enables procedural selection across matching entities. Same filter applied to different game states yields different entity selections while maintaining archetype structure.

### Parser Already Handles Optional Fields

SceneTemplateParser shows defensive parsing for optional fields. DisplayNameTemplate defaults to null when missing - game can generate placeholder-free display or use scene ID. IntroNarrativeTemplate defaults to null - no intro text shows, or AI generates from hints. PlacementFilter properties check for null or empty lists before processing - empty PersonalityTypes list skips personality filtering, null LocationProperties array means no property requirements. PresentationMode and ProgressionMode parse with explicit defaults (Atmospheric and Breathe) when strings empty or missing.

This pattern extends throughout parser. DTO properties marked with nullable types or default values. Parser checks nullability before processing. Domain entities initialize collections inline. Missing optional data doesn't crash - it results in sensible defaults or empty collections that game logic handles gracefully.

The implication is tutorial JSON can omit cosmetic fields. DisplayNameTemplate not strictly required. IntroNarrativeTemplate optional. Empty arrays in placementFilter safely omitted. Minimal JSON survives parsing and generates valid domain entities.

## Design Approach & Rationale

### Context-Aware Generation, Not Generic Patterns

The breakthrough insight came from examining tutorial choices. Situation one offers helping Elena (Social challenge building bond), working for Thomas elsewhere (Physical challenge with navigation), or sleeping rough (desperate fallback spawning consequence scene). These aren't generic archetype patterns - they're contextually generated from entity properties.

Elena's DEVOTED personality enables emotional connection path (work-for-bond choice). Her Innkeeper profession at Commercial location enables work-for-service exchanges. Low player bond triggers prove-yourself paths. Nearby Thomas (Laborer profession) enables work-elsewhere navigation option. Player coins below cost threshold triggers poverty fallback with consequence scene spawn. Multiple categorical properties combine to generate three contextually appropriate choices.

The design shifts SceneArchetypeCatalog from returning generic skeletons to reading entity objects and generating situation-specific content. Same archetype ID with different entities produces different situations and choices. Service with location access at DEVOTED innkeeper generates help-for-bond paths. Service with location access at MERCANTILE bathhouse attendant generates pure transactional exchanges. Context drives generation without hardcoding specific entity IDs.

### Why Not Generic Archetypes?

Alternative considered: keep archetypes generic (stat/money/challenge/fallback pattern), accept that all service scenes feel identical, rely on AI narrative generation to provide variety. Rejected because mechanical sameness creates repetitive gameplay even with narrative variation. Players optimize for mechanics, not narrative flavor. If every service negotiation has identical costs and choices, gameplay becomes rote regardless of whether NPC is friendly innkeeper or stern bathhouse attendant.

Context-aware generation solves mechanical variety while maintaining archetype structure. DEVOTED personalities create fundamentally different choice structures (emotional paths, bond rewards) than MERCANTILE personalities (transactional, no emotional investment). Player state drives requirement formulas (poverty triggers fallbacks, high bond unlocks advanced options). Location properties enable contextual actions (Commercial enables work exchanges, Private unlocks intimate conversations). Mechanical variety emerges from categorical property combinations while archetype maintains balance formulas.

### Why Pass Entity Objects to Catalogue?

Alternative considered: pass only categorical property values (personality enum, location property list, player stats). Rejected because it creates fragile coupling and incomplete context. Catalogue needs to check multiple property combinations simultaneously - DEVOTED personality AND Commercial location AND low player coins AND nearby Laborer NPC creates specific choice. Extracting all relevant properties and passing individually becomes unwieldy parameter list. Missing property prevents contextual generation.

Passing complete entity objects gives catalogue access to full categorical property set. Read NPC.PersonalityType, check NPC.Profession, query NPC.ProvidedServices, evaluate Location.LocationProperties, access Player.Coins, examine Player.BondStrength, trace Player.ChoiceHistory. Generate situations by reading properties directly rather than receiving pre-filtered subset. Flexible context access without parameter explosion.

The coupling is acceptable because catalogues read ONLY categorical properties, never entity IDs or runtime-mutable state. Reading PersonalityType enum doesn't couple to specific entity instance. Checking LocationProperties doesn't depend on which location object was passed. Categorical properties are design-time stable characteristics, not gameplay-time mutable state. Catalogues remain parse-time translation tools, not runtime services.

### Why Not Hardcode Tutorial Archetype?

Alternative considered: create tutorial_service_with_access archetype encoding Elena-specific logic (offer help choice, navigate to Thomas, sleep rough fallback). Rejected because it defeats reusability. Tutorial archetype only works with Elena. Other innkeepers would use different archetype, losing consistency. Can't bind tutorial archetype to bathhouse - generates Elena-specific choices at wrong location.

Context-aware generic archetype serves both needs. Tutorial binds service_with_location_access to Elena, generates Elena-appropriate choices from her categorical properties. Other scenes bind same archetype to different NPCs, generate different choices from their properties. Archetype remains reusable across entity types while producing contextually appropriate content. Single archetype definition generates infinite contextual variations through entity property combinations.

### Why Minimize Tutorial JSON?

Tutorial currently demonstrates multi-situation arcs through 576 lines of explicit hand-authoring. Every situation defined, every choice specified, all costs and rewards and requirements manually set. This serves as reference implementation showing what procedural generation should produce. But it also creates maintenance burden - updating tutorial requires changing hundreds of lines, mechanical consistency with other scenes requires manual synchronization.

Minimizing tutorial to twelve lines using archetype achieves multiple goals. Proves archetype generates tutorial-equivalent content through entity context alone. Reduces maintenance burden to single-line archetype ID and entity bindings. Establishes pattern for all future content - bind archetype to entities, let generation handle details. Makes tutorial identical in structure to other scenes, not special-cased hand-authored content requiring separate maintenance.

The constraint that generated content must match legacy tutorial exactly ensures generation quality. Not accepting inferior procedural content justified by convenience. Generated situations and choices must achieve same gameplay experience as hand-crafted tutorial. This forces catalogue to correctly read entity properties and generate contextually appropriate structures rather than generic fallbacks.

## Implementation Strategy

### Phase One: Update Catalogue Signature

SceneArchetypeCatalog.GetSceneArchetype currently receives only archetype ID, service type, and tier. These are insufficient for context-aware generation. Extend signature to accept NPC contextNPC, Location contextLocation, Player contextPlayer parameters. Update all three archetype generation methods (GenerateServiceWithLocationAccess, GenerateTransactionSequence, GenerateGatekeeperSequence) to receive entity parameters.

This change doesn't affect generation logic yet - just makes entity objects available to generation methods. Subsequent refactoring reads properties from these objects. Compilation will break because parser doesn't pass new parameters - that's next phase.

### Phase Two: Refactor Service With Location Access Generation

GenerateServiceWithLocationAccess currently creates four situations with hardcoded archetypeIds and generic rewards. Refactor to read entity properties and generate contextually.

Situation one (negotiate) generation examines multiple properties. Check contextNPC.PersonalityType - if DEVOTED, generate emotional connection choice (Social challenge, bond reward). If MERCANTILE, generate transactional choice only (no emotional path). Check contextLocation.LocationProperties - if contains Commercial, enable work-for-service exchanges. Query contextPlayer.Coins - if below service cost, generate poverty fallback spawning consequence scene. Query nearby NPCs via contextLocation - if Laborer profession present, generate work-elsewhere navigation choice.

Situation two (payment) generation creates requirement formula dynamically. Build CompoundRequirement with OrPaths containing two alternatives: player has sufficient coins for service cost, OR player possesses access token (room_key) granted by situation one choices. This allows multiple paths to access - direct payment or work-earned token. Payment choice unlocks location using LocationsToUnlock reward property.

Situation three (service) generation scales rewards by tier using formulas. Service type lodging gets TimeSegmentCost six, AdvanceToBlock Morning, StaminaChange calculated as thirty plus tier times ten. Service type bathing gets TimeSegmentCost two, no time advancement, cleanliness restoration. Service type healing gets moderate time cost, HealthChange scaled by tier. Formulas ensure mechanical balance across tiers while service type determines which resources restore.

Situation four (depart) generation reads player history. If ChoiceHistory contains social work choice, narrative hints reference bond building. If contains physical work choice, narrative hints reference warehouse opportunities. DEVOTED personality adds caring guidance context. MERCANTILE personality adds transactional dismissal context. Cleanup logic removes temporary access token, re-locks location, spawns follow-up scenes appropriate to player path.

Each situation receives entity-appropriate archetypeId. DEVOTED personality negotiation uses service_transaction archetype. MERCANTILE personality uses negotiation archetype. Access situation uses access_control archetype. Service and depart situations may have null archetypeId (simple narratives, no choices needed) or specific archetypes depending on complexity.

### Phase Three: Update Parser to Resolve and Pass Entities

SceneTemplateParser currently calls GetSceneArchetype with only archetype ID, service type, and tier. This breaks after phase one changes. Update to resolve entity objects before calling catalogue.

Entity resolution examines placementFilter. If contains npcId (tutorial binding), resolve that specific NPC via GameWorld.GetNPC. If contains personalityTypes without npcId (categorical binding), query GameWorld for NPCs matching personality types at specified location - selection strategy determines which among matches. Store resolved NPC as contextNPC variable.

Location resolution queries contextNPC.Location property - NPC's current location becomes contextLocation. This ensures NPC and Location are spatially consistent (can't generate scene for NPC at one location while referencing properties of distant location).

Player resolution calls GameWorld.GetPlayer() - returns current player state including coins, stats, bonds, history. No filtering needed, just retrieve player entity.

Pass resolved entities to GetSceneArchetype call. Catalogue receives concrete entity objects to read properties from. Generated SceneArchetypeDefinition contains situations with context-aware choices. Parser continues existing enrichment flow - bare SituationTemplates from catalogue expanded via EnrichSituationTemplateFromArchetype, archetypeIds generate ChoiceTemplates, complete SceneTemplate created with enriched situations.

### Phase Four: Minimize Tutorial JSON

Tutorial currently defines archetype, displayNameTemplate, introNarrativeTemplate, spawnRules, and four explicit situationTemplates with all choices. Strip to essential fields only.

Required fields: id (scene identifier), archetype (Linear pattern for backward compatibility with existing code), sceneArchetypeId (triggers catalogue generation), serviceType (lodging contextualizes generation), tier (difficulty scaling), isStarter (game initialization flag), placementFilter with npcId binding Elena specifically.

Optional fields removed: displayNameTemplate (null is acceptable, game can use scene ID or generate), introNarrativeTemplate (null is acceptable, no intro or AI generation), spawnRules (generated by catalogue), situationTemplates (generated by catalogue). PlacementFilter properties emptied except npcId - no explicit personalityTypes or locationProperties needed because NPC binding is concrete.

Result is twelve-line JSON containing only archetype binding plus entity reference. All structural content (situations, choices, spawn rules, costs, rewards) comes from catalogue generation reading entity properties.

### Phase Five: Build and Validate

Compilation after these changes may reveal missing dependencies or type mismatches. Catalogue needs entity type imports. Parser needs to handle null cases (what if placementFilter npcId invalid - fail fast with descriptive error). Generated situations need proper initialization following Let It Crash pattern.

Runtime validation tests catalogue generation. Console logs should show entity property reading: Elena DEVOTED personality detected, Commercial location property found, low player coins triggering fallback, three choices generated. Launch game, tutorial scene should appear with four situations progressing identically to legacy hand-authored version. Choice texts may differ slightly (generic archetype text versus hand-crafted narrative), but mechanical structure (costs, requirements, rewards) must match exactly.

Create test scene binding same archetype to different NPC. Mercantile merchant at marketplace using service_with_location_access should generate different situation structure than DEVOTED innkeeper. Validate contextual variety - same archetype produces meaningfully different gameplay when bound to entities with different categorical properties.

## Critical Constraints

### Catalogue Pattern Compliance

Catalogues called ONLY at parse time, NEVER at runtime. SceneArchetypeCatalog imported ONLY in SceneTemplateParser namespace. GetSceneArchetype called ONLY during JSON deserialization when sceneArchetypeId field detected. Once parsing completes, catalogue never touched again - all data lives in GameWorld collections as pre-generated SceneTemplate entities.

Violation symptoms: catalogue import in facade or manager, GetArchetype calls during gameplay, runtime translation of categorical properties. Enforcement: scan imports - catalogues appear only in parser files. Trace GetArchetype calls - only from ParseSceneTemplate method. Verify no archetype ID strings in runtime code.

### Three-Tier Timing Enforcement

Parse time generates SceneTemplate with embedded SituationTemplates. Instantiation time creates Scene with dormant Situations from templates. Query time activates specific Situation and instantiates its actions. Never mix tiers - don't generate templates at instantiation, don't instantiate situations at query time, don't activate actions during parsing.

Catalogue operates at parse time tier exclusively. Receives entity objects but reads only immutable categorical properties (PersonalityType enum, LocationProperties list, Profession enum). Never reads mutable runtime state (current health, active situations, bonds accumulated since parsing). Entity state at parse time differs from runtime state, but categorical properties remain stable.

### HIGHLANDER for Entity References

Scene stores entity references ONE way consistently. NPC reference stored as contextNPC object (frequent navigation during generation). Location reference stored as contextLocation object (frequent property queries). Both resolved once during parsing, cached on SceneTemplate, never looked up again. No redundant ID storage alongside object for these parse-time entities.

However, Scene instances DO store IDs for serialization purposes. SceneTemplate has entity object references used during generation. Scene (runtime instance) has entity ID strings for save/load. This dual storage acceptable because different purposes (design-time generation versus persistence), populated once, no synchronization risk.

### Let It Crash Initialization

SituationTemplates generated by catalogue initialize all collections inline. ChoiceTemplates property initializes to empty list, not null. NarrativeHints initializes if provided, remains null if absent. AutoProgressRewards initializes if service provides rewards, remains null otherwise. Parser trusts initialization, never adds null-coalescing operators. Runtime queries properties directly, crashes descriptively if data missing rather than hiding problems with defensive checks.

This extends to entity property access in catalogue. Reading contextNPC.PersonalityType throws if NPC null rather than silently defaulting. Querying contextLocation.LocationProperties crashes if Location null rather than assuming empty list. Fail fast with clear errors identifying parse-time data problems rather than propagating bad data into gameplay.

### No ID-Based Logic

Entity IDs serve only reference and debugging purposes, never control behavior. Catalogue generation never checks if contextNPC.ID equals specific string. Never examines Location.Id to determine generation path. Never matches Player.Name or entity IDs in conditional logic. All branching decisions use categorical properties (enums, typed flags, numerical thresholds), not string matching on identifiers.

ActionType enum routes choice handling, not choice ID strings. PersonalityType enum determines generation paths, not checking if NPC.Name contains keywords. LocationPropertyType list gates functionality, not parsing Location.Id for location type indicators. Strongly typed properties throughout, zero string-based routing.

### Perfect Information Preservation

Generated choices maintain perfect information principle - player can calculate outcomes before attempting. Choice texts describe what happens. Requirements show exact thresholds. Costs display resource consumption. Rewards preview benefits. No hidden mechanics, no surprise consequences, no unclear prerequisites.

Catalogue generation preserves this by creating complete ChoiceTemplates with full mechanical specifications before player sees situation. RequirementFormula contains explicit stat thresholds and item requirements. CostTemplate lists exact coin/resource costs. RewardTemplate specifies all gains. ActionType indicates whether instant resolution or challenge engagement. Player has all information needed for informed decision.

Contextual generation doesn't obscure mechanics through dynamic complexity. DEVOTED innkeeper's help-for-bond choice shows explicit cost (Focus, time segments) and reward (bond increase, room key). Poverty fallback shows consequence scene spawn explicitly. Navigation to Thomas shows destination and challenge type. Generated content more transparent than hand-authored equivalent because archetype formulas encode player-visible rules.

## Key Files & Their Roles

### SceneArchetypeCatalog.cs

Location: src/Content/Catalogs/

Purpose: Parse-time catalogue translating scene archetype IDs plus entity context into complete multi-situation structures. Contains GetSceneArchetype method as public entry point receiving archetype ID string, service type string, tier integer, and entity objects (NPC, Location, Player). Returns SceneArchetypeDefinition containing bare SituationTemplates with archetypeIds and SpawnRules with transitions.

Internal generation methods create situation-specific structures by reading entity categorical properties. GenerateServiceWithLocationAccess examines NPC personality type for emotional versus transactional paths, checks location commercial property for work exchanges, evaluates player coins for poverty fallbacks, queries nearby NPCs for alternative work options. Generates four situations (negotiate, access, service, depart) with contextually appropriate archetypeIds and AutoProgressRewards.

Other methods handle different archetype patterns. GenerateTransactionSequence creates browse-negotiate-complete flow for shopping. GenerateGatekeeperSequence creates confront-pass flow for authority challenges. Each reads entity properties to contextualize generic pattern for specific entity combination.

### SceneArchetypeDefinition.cs

Location: src/GameState/

Purpose: Return type for SceneArchetypeCatalog methods containing bare situation structures ready for parser embedding. Simple data transfer object with two properties: SituationTemplates list containing skeleton situations (IDs, types, archetypeIds, narrative hints, auto-progress rewards), and SpawnRules defining transition pattern (linear, hub-spoke, branching) with source-destination mappings.

Entity objects NOT stored here - generation consumes entity properties and produces entity-agnostic structures. Bare SituationTemplates reference archetypeIds that SituationArchetypeCatalog will expand later. This separation keeps scene generation distinct from choice generation - scene catalogue creates situation framework, situation catalogue creates choice details.

### SceneTemplateParser.cs

Location: src/Content/Parsers/

Purpose: Parses SceneTemplateDTO from JSON into SceneTemplate domain entity. Orchestrates archetype-driven generation when sceneArchetypeId field present in DTO. Main method ParseSceneTemplate detects sceneArchetypeId, resolves entity objects from placementFilter, calls SceneArchetypeCatalog with entities, receives bare situations, enriches situations via existing pipeline.

Entity resolution logic queries GameWorld for NPC matching placementFilter specifications. If npcId present, direct lookup. If only personalityTypes, query and select via strategy. Retrieved NPC provides Location via property reference. Player retrieved via GetPlayer method. Three entity objects passed to catalogue call.

Enrichment flow processes bare SituationTemplates from catalogue. EnrichSituationTemplateFromArchetype method checks each situation's archetypeId - if present, calls GenerateChoiceTemplatesFromArchetype to expand into four choices, if absent, uses empty list (AutoAdvance situation). Creates new SituationTemplate with populated ChoiceTemplates while preserving other properties from bare template.

Handles both archetype-driven and hand-authored content. If sceneArchetypeId present, uses catalogue generation. If absent, parses explicit situationTemplates from DTO (legacy behavior). Backward compatible with existing hand-authored scenes while enabling new procedural approach.

### SceneTemplateDTO.cs

Location: src/Content/DTOs/

Purpose: Deserializes JSON scene definitions. Contains SceneArchetypeId and ServiceType string properties enabling catalogue-driven generation. Parser detects these fields and triggers procedural path instead of parsing explicit situation templates.

Optional fields like DisplayNameTemplate and IntroNarrativeTemplate allow null - parser provides sensible defaults when absent. PlacementFilter property may contain concrete npcId for tutorial binding or categorical personalityTypes for procedural selection. SituationTemplates list remains for backward compatibility with hand-authored content but unnecessary when archetype ID present.

### SituationArchetypeCatalog.cs

Location: src/Content/Catalogues/

Purpose: Existing parse-time catalogue generating choice structures from situation archetype IDs. Contains GetArchetype method returning SituationArchetype struct with mechanical definitions (stats, costs, challenge types). Parser calls this for each SituationTemplate with archetypeId during enrichment phase.

Not modified for scene generation feature but crucial to understanding two-tier catalogue architecture. Scene catalogue generates situation skeletons, situation catalogue generates choice details. Scene archetypes reference situation archetypes via archetypeId field. Composition separates concerns - scene patterns versus choice patterns.

### Scene.cs

Location: src/GameState/

Purpose: Domain entity representing runtime scene instance. Contains AdvanceToNextSituation method executing spawn rules to progress through situations. IsComplete method checks completion state. Stores CurrentSituationId tracking player position in arc, SituationIds list referencing all situations in scene, SpawnRules copied from template defining transition pattern.

State machine implementation demonstrates multi-situation progression working. When situation completes, facade calls AdvanceToNextSituation with completed situation ID. Method queries SpawnRules.Transitions for matching source, retrieves destination situation ID, updates CurrentSituationId, marks scene complete if no transitions remain. Domain behavior, not service orchestration.

### SituationTemplate.cs

Location: src/GameState/

Purpose: Immutable template embedded in SceneTemplate defining single situation. Contains archetypeId field referencing SituationArchetypeCatalog patterns. Empty ChoiceTemplates list when freshly generated - parser enrichment populates via archetype expansion. NarrativeHints property guides AI generation with tone, theme, context. AutoProgressRewards property enables situations that auto-execute without player choices.

Bare templates from SceneArchetypeCatalog have minimal fields populated: ID, Type, ArchetypeId, NarrativeHints, AutoProgressRewards. Parser enrichment creates complete templates adding ChoiceTemplates list generated from ArchetypeId lookup. Final SceneTemplate contains fully realized SituationTemplates with all choices specified despite JSON containing only archetype binding.

### 21_tutorial_scenes.json

Location: src/Content/Core/

Purpose: Tutorial scene demonstrating lodging service arc. Currently 576 lines of explicit hand-authoring defining four situations with all choices. Target is twelve-line archetype-driven definition with only scene ID, archetype pattern, service type, tier, and NPC binding. Generated content must produce gameplay identical to legacy tutorial proving archetype quality.

Serves as test case and reference. If archetype-driven generation matches hand-crafted tutorial quality, approach validated for broader use. If generated content differs mechanically or degrades experience, archetype logic needs refinement. Tutorial fidelity is quality gate for procedural generation acceptance.

