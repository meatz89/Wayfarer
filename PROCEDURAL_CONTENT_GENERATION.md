# Procedural Content Generation Architecture

## Problem & Motivation

### The Scaling Crisis

Wayfarer's narrative RPG model creates a content volume problem. A scene like "Secure Lodging" encompasses: the innkeeper conversation, payment or persuasion, receiving a room key, navigating to the locked upper floor, resting overnight with resource restoration, time passing to morning, waking up, leaving the room, and the room locking again. This entire multi-situation arc with location state changes, item lifecycle, resource restoration, and time advancement is ONE scene template.

The naive solution - hand-author every scene in JSON - doesn't scale. Writing explicit situations and choices for every service location, every checkpoint, every transaction creates exponential authoring burden. Tutorial lodging at Elena's tavern needs JSON defining: negotiate situation with 3 choices, access situation, rest situation, depart situation. Then bathhouse with Marcus needs separate JSON. Then healing temple with Thalia needs more JSON. Each mechanically similar but requiring separate authoring.

The insight: **entities have categorical properties that drive procedural generation**. Elena has Personality=Innkeeper and Demeanor=Friendly. Marcus has Personality=Attendant and Demeanor=Professional. Thalia has Personality=Priest and Demeanor=Compassionate. These properties determine how archetype generates situations and choices. Tutorial JSON needs only: archetype ID + concrete entity references. Generation logic queries entity properties and produces contextually appropriate complete arc.

The traditional solution - procedural generation - fails because these complex multi-situation arcs require careful mechanical orchestration. Generated scenes produce broken progression (situations spawn out of order), unbalanced economics (rest costs too little or too much), narrative incoherence (morning departure happens before sleeping), and broken state management (keys granted but never removed).

### The AI Limitation

Large language models can generate compelling narrative for individual moments but cannot orchestrate complex multi-situation arcs with mechanical coherence. AI can write engaging dialogue for innkeeper negotiation but has no concept of "this grants an item that unlocks a location spot which enables a rest situation which restores resources using formulas and advances time which triggers morning situation which removes the key and re-locks the spot". 

Scene structure requires understanding: situation sequencing (negotiation BEFORE access BEFORE rest BEFORE departure), resource flow (key granted then consumed, energy depleted then restored, coins spent never returned), world state causality (time advances causing morning, rest causes restoration, departure causes lock), and economic balance (rest benefit must justify cost across progression curve).

### The Core Insight

Separate scene archetype patterns from entity context, with entity categorical properties driving generation. Game logic defines multi-situation arc structure: "service with location access" contains negotiation situation (challenge or payment grants access item), access situation (consume item to unlock spot), service situation (consume time/resources for benefit), and departure situation (remove item, re-lock spot).

**Entity properties control generation specifics:** NPC Personality=Innkeeper + Demeanor=Friendly generates easier challenge difficulty and conversational tone. Location Services=Lodging determines service provides rest benefit. Spot Privacy=Medium sets moderate cost premium. Same archetype, different entities, contextually appropriate mechanical variation.

**Tutorial and procedural use identical generation path:** Tutorial JSON specifies archetype + concrete entity IDs (Elena, Tavern, Upper Floor). Procedural templates specify archetype + categorical filters (any Innkeeper at any Urban location with Lodging service). Both paths invoke same generation logic receiving entity objects. Generation queries properties and produces complete arc.

AI applies narrative texture using entity properties: Elena generates "the innkeeper greets you warmly" because Demeanor=Friendly. Marcus generates "the attendant regards you professionally" because Demeanor=Professional. Thalia generates "the priestess offers a compassionate smile" because Demeanor=Compassionate. Same situation position, different entity properties, contextually appropriate narrative.

The architecture enables: minimal JSON authoring (just archetype + entity references), tutorial reproducibility (same entities always generate same scene), infinite procedural variation (different entities generate appropriate variations), mechanical consistency (same formulas balance all instances), narrative appropriateness (AI receives property-derived hints).

### Why This Matters

Without procedural scene generation, every service location needs hand-authored multi-situation arcs. With naive procedural generation, scenes break mechanically (wrong situation ordering, unbalanced costs, broken state). With AI-only generation, scenes lack mechanical coherence (made-up costs, arbitrary progression, broken economy). The solution must generate mechanically sound multi-situation arcs while allowing AI to provide rich narrative at each beat.

---

## Architectural Discovery

### Scene Scope Understanding

Scenes are NOT single conversations with 2-4 choices. Scenes are complete multi-situation narrative arcs with:

**Sequential Situation Progression:** Multiple situations flowing in defined order. Negotiation situation completes, spawns access situation. Access situation completes, spawns service situation. Service situation completes, spawns departure situation. Scene progression rules define this flow (linear, branching, hub-and-spoke, conditional).

**Location State Modification:** Scenes change world state. Spot lock states toggle (locked becomes unlocked becomes locked again). NPC availability changes (merchant closes shop, guard changes post). Environmental properties shift (time of day advances, weather changes, danger levels update).

**Item Lifecycle Management:** Scenes grant items (room key, ticket, permit) that exist temporarily within scene scope. Item granted in situation 1, consumed in situation 2, removed in situation 4. Items don't persist beyond scene - they're arc-specific temporary tokens enabling progression.

**Resource Flow Orchestration:** Scenes consume resources (coins, time, energy) and produce benefits (restored stamina, gained knowledge, increased stats). Economic balance maintained across entire arc. Early situations cost resources, later situations provide benefits, net outcome justified by total cost.

**World State Causality:** Actions in one situation cause consequences in later situations. Paying coins in negotiation situation affects what happens in service situation (paid upfront versus pay later). Time advancement in rest situation triggers morning-specific narrative in departure situation. Prior choices ripple forward through arc.

**Cleanup and Reset Logic:** Scenes restore world to sensible state after completion. Temporary items removed. Locked spots re-locked. NPC availability reset. Time advanced appropriately. Next player visit finds world in coherent state, not broken by prior scene completion.

### The Three-Tier Timing Model

The codebase enforces strict separation between when content is defined, when it's instantiated, and when it becomes visible.

**Parse Time** occurs at game initialization. JSON files define immutable scene templates containing: complete situation sequences, spawn rules governing flow between situations, mechanical formulas for costs/benefits, item grant/consume/remove patterns, location state modification logic, and narrative hints for AI generation. Catalogues translate scene archetype IDs into these complete multi-situation structures during parsing exclusively.

**Instantiation Time** occurs when spawn conditions trigger. A scene template becomes runtime scene instance placed at concrete location/NPC. All situations within scene created as dormant entities simultaneously - they exist but none are active yet. First situation in sequence marked as current, others wait for spawn rules to activate them. No actions instantiated yet, just situation structure in dormant state.

**Query Time** occurs when player enters context where scene lives. First situation transitions from dormant to active, instantiates its choices into action entities. Player selects action, situation completes, spawn rules determine next situation, that situation activates and instantiates its actions. Progression continues until scene completes.

This three-tier separation prevents premature activation of later situations (rest situation doesn't activate before negotiation), ensures fresh evaluation at each beat (departure situation evaluates morning-specific conditions), and maintains efficient memory usage (actions only exist when player can see them).

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

### Situation Sequencing via Spawn Rules

Situations don't activate arbitrarily. SpawnRules on Scene template govern flow:

**Linear Progression:** Situation A completes, spawn rule activates situation B. Situation B completes, spawn rule activates situation C. Strict sequence, no branching. Used for: service arcs (negotiate → access → use → depart), tutorial flows, time-bound sequences.

**Conditional Branching:** Situation A completion spawns different situations based on outcome. Success spawns situation B, failure spawns situation C. Both valid continuations, different narrative tones. Used for: negotiation failures (success grants access, failure demands alternative), challenge outcomes (victory rewards, defeat consequences).

**Hub-and-Spoke:** Initial situation spawns multiple parallel situations, player chooses order, all must complete before finale. Used for: investigation scenes (gather clues at multiple locations), preparation sequences (acquire items from different sources), relationship building (multiple conversation topics).

**Recursive Spawning:** Situation completion can spawn additional situations within same scene dynamically. Investigation gathering evidence might spawn follow-up investigation based on what was found. Crisis situation might spawn escalation situations if player choices make things worse.

Spawn rules are DECLARATIVE DATA defining flow, not imperative code. Scene template lists transitions (source situation → destination situation, with conditions). Scene instance executes rules by checking completion, evaluating conditions, activating next situations. Domain entity owns state machine.

### Item Lifecycle Within Scene Scope

Scenes frequently grant temporary items enabling progression:

**Grant Phase:** Early situation rewards player with item. Room key from innkeeper negotiation, permit from guard approval, token from merchant purchase. Item added to player inventory.

**Consumption Phase:** Middle situation requires item to proceed. Accessing locked spot consumes room key, presenting permit allows checkpoint passage, showing token enables service. Item checked against requirements, consumed if appropriate.

**Removal Phase:** Final situation cleans up temporary item if still held. Departure removes room key even if player didn't use it yet (checkout process), expired permits removed, tokens reclaimed. Ensures items don't persist beyond scene scope inappropriately.

Item lifecycle is scene-managed. Reward template grants item, requirement template checks for item, cleanup logic removes item. Same item potentially granted by different scenes (multiple inns grant room_key), but each scene instance manages its granted items independently.

Items can also be permanent rewards. Completing investigation grants knowledge_fragment that persists forever. Scene distinguishes: temporary items (scene-scoped access tokens), permanent items (persistent rewards), consumable items (used once then depleted).

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
  "concreteLocationId": "tavern_common_room",
  "concreteSpotId": "tavern_upper_floor"
}
```

No situations defined. No choices defined. Just archetype reference + concrete entity bindings.

**Parse-Time Generation:** SceneArchetypeCatalog.GenerateServiceWithLocationAccess receives: archetype ID, tier 0, Elena entity object (Personality=Innkeeper, Demeanor=Friendly, Authority=Low), Tavern entity object (Services=Lodging, CostModifier=1.0x, Atmosphere=Casual), Upper Floor entity object (Privacy=Medium, Functionality=Rest, Lockable=true), player state benchmarks.

Generation queries properties and produces:

**Situation 1 - Negotiation:** Challenge option (15 Focus because Friendly demeanor, Social threshold 1 because Tier 0 + Low authority), grants +1 bond. Payment option (15 coins because Urban + Medium privacy). Refuse option (always available). All three ChoiceTemplates generated from property queries.

**Situation 2 - Access:** Navigate to Upper Floor spot (now unlocked due to room_key granted by situation 1). Single choice (enter room) consuming key.

**Situation 3 - Rest:** Time cost 6 hours (advances to morning). Benefit: restore 40 stamina (Tier 0 formula × Rest functionality). Single choice (sleep).

**Situation 4 - Departure:** Cleanup situation. Remove room_key from inventory, re-lock Upper Floor spot. Single automatic choice (leave room).

**Complete 4-situation arc generated from properties + archetype pattern.** Situation flow (SpawnRules), choice structures, costs, benefits, item lifecycle all procedurally determined by querying Elena/Tavern/UpperFloor properties and applying archetype formulas.

**Tutorial Reproducibility:** Every playthrough generates identical scene because: same entities → same properties → same generation output. Elena always Friendly (easier challenge), Tavern always moderate cost, Upper Floor always medium privacy rest benefit.

**Post-Tutorial Variation:** Same archetype at Marcus's bathhouse generates: Professional demeanor (moderate difficulty), Bathing service (cleanliness benefit instead of stamina), Private chamber (2x cost premium). Different entities, contextually appropriate variation, identical generation logic.

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

**Service with Location Access:** Four situations (negotiate, access, service, depart). Used for: lodging (rest), bathing (cleanliness), storage (item management), training (skill increase), healing (health restoration). Cost varies by service type, benefit varies by tier, but structure identical.

**Transaction Sequence:** Three situations (browse inventory, negotiate price, complete transaction). Used for: shopping (buy items), selling (convert items to coins), trading (item exchanges). Complexity varies by merchant type and item rarity.

**Investigation Arc:** Variable situations (gather evidence, analyze findings, confront suspect, resolve mystery). Hub-and-spoke pattern (gather from multiple sources) converging to linear conclusion. Used for: quest lines, mystery solving, knowledge acquisition.

**Gatekeeper Sequence:** Two to four situations (initial confrontation, prove worthiness or bribe, pass checkpoint, optional consequences). Linear or branching based on player approach. Used for: guard posts, restricted areas, authority challenges.

**Crisis Response:** Three to five situations (crisis discovered, assess options, execute solution, handle aftermath, consequences). Time-sensitive, failure spawns escalation. Used for: emergencies, disasters, urgent requests.

Each archetype specifies: situation count and types, spawn rules governing flow, item lifecycle patterns, resource economics (costs versus benefits), state modification logic (what world changes occur), cleanup requirements (what must be restored).

Archetypes are mechanical arc patterns devoid of narrative. Same archetype generates lodging at inn (rest), bathing at bathhouse (cleanliness), healing at temple (health) with identical situation flow but different service benefits and narrative context.

### Why Multi-Situation Arcs Work

Players learn scene patterns through repetition with variation:

**Pattern Recognition:** Encounter "service with access" at inn (lodging), recognize same structure at bathhouse (bathing), apply knowledge to new contexts. Structural familiarity speeds comprehension, narrative variety prevents repetition feeling stale.

**Strategic Planning:** Knowing lodging scene requires negotiation → access → rest → departure helps player plan resource expenditure. "I need 15 coins for negotiation and 6 hours for rest, do I have both?" Perfect information at arc level, not just situation level.

**Expectation Management:** Scene archetypes establish expectations. Service scenes restore resources (positive outcome). Investigation scenes grant knowledge (informational outcome). Crisis scenes create cascading consequences (potentially negative outcome). Genre conventions maintained by consistent archetype usage.

**Mechanical Consistency:** All service scenes balanced similarly. Lodging costs X and restores Y, bathing costs X and restores different Y, healing costs X and restores third Y. Ratios consistent, benefits different, balance maintained. Archetypes enforce economic equilibrium across procedurally generated scenes.

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

Scene archetype generation includes cleanup logic appropriate to archetype type. Service scenes clean up access items and re-lock spots. Investigation scenes don't clean up knowledge gained. Transaction scenes clean up negotiation state but not purchased items.

---

## Implementation Strategy

### Phase 1: Scene Archetype Catalog Creation

Build catalog generating complete multi-situation scene structures from archetype IDs with entity properties driving contextual generation.

**Generation Method Signature:**
```
GenerateServiceWithLocationAccess(
    archetypeId: string,
    tier: int,
    npc: NPC,              // Entity object with properties
    location: Location,    // Entity object with properties  
    spot: LocationSpot,    // Entity object with properties
    playerState: PlayerState
) -> SceneTemplate
```

**Property Query Pattern:** Generation logic queries entity properties to determine contextual mechanical values. Challenge difficulty queries npc.Demeanor (Friendly=0.8x multiplier, Hostile=1.4x). Payment cost queries location.CostModifier and spot.Privacy. Service benefit queries spot.Functionality (Rest=stamina, Bathing=cleanliness). Narrative hints query npc.Personality and location.Atmosphere.

**Situation Generation:** Produces four SituationTemplates. Negotiate situation has ChoiceTemplates (challenge with property-scaled difficulty, payment with property-scaled cost, refuse option). Access situation has single choice consuming granted key. Service situation has single choice with property-determined benefit and time cost. Departure situation has cleanup choice removing temporary items and restoring spot lock state.

**SpawnRules Generation:** Linear flow (negotiate→access→service→depart) with success conditions. Each transition references situation template IDs from generated situations.

**Item Lifecycle:** Grant room_key in negotiate rewards, require room_key in access situation, remove room_key in depart cleanup. Item type matches service context (key for lodging, token for passage, permit for restricted area).

**Tutorial and Procedural Use Identical Logic:** Tutorial parser resolves concrete entity IDs to objects, passes to generation. Procedural placement evaluator selects entities matching filters, passes to generation. Generation receives entity objects either way, queries properties identically, produces contextually appropriate output.

**Test Pattern:** Create test entities with extreme property values. Friendly (0.8x) vs Hostile (1.4x) NPCs should generate 1.75x difficulty difference. Shared (0.5x) vs Private (2.0x) spots should generate 4x cost difference. Verify formulas balance across property ranges.

### Phase 2: Scene State Machine Implementation

Add domain methods to Scene entity for arc progression control:

**Scene.AdvanceToNextSituation(completedSituationId):** Queries SpawnRules, finds transitions from completed situation, gets destination situation template IDs, instantiates destination situations as runtime entities (add to GameWorld.Situations with scene ID reference), transitions those situations from nonexistent to Dormant state, updates CurrentSituationId to point to newly spawned situation(s), marks scene complete if no valid transitions exist.

**Scene.ExecuteCleanup():** Queries template CleanupPatterns, removes temporary items from player inventory, restores spot lock states, reverts NPC availability, validates time advanced appropriately, marks scene as fully resolved.

**Scene.IsComplete():** Checks if CurrentSituationId points to conclusion situation and that situation is Completed, or if no valid spawn transitions remain. Boolean indicating arc finished.

These methods encapsulate scene lifecycle. SituationCompletionHandler calls Scene.AdvanceToNextSituation() after situation resolves. GameFacade calls Scene.ExecuteCleanup() when scene marked complete. Domain entity owns state machine, services orchestrate calls.

### Phase 3: Spot Filtering and Integration

Extend placement filtering to handle LocationSpot entities:

**Spot Properties:** Add properties to LocationSpot entity (Accessibility enum, Privacy enum, Capacity int, Functionality enum). These enable filtering (need private lockable interior spot for lodging, need public accessible exterior spot for market stall).

**Placement Filter Spot Requirements:** Add SpotRequirements to PlacementFilter (functionality required, minimum privacy, accessibility constraints). Scene templates declare spot needs alongside NPC and location requirements.

**Three-Entity Resolution:** PlacementFilterEvaluator resolves NPC + Location + Spot simultaneously. Find NPCs matching personality, filter locations where NPC present with required properties, filter spots at those locations matching spot requirements. Return triple (NPC, Location, Spot) or null if no match.

**Spot Selection Strategies:** When multiple spots match, apply template preference (prefer highest privacy if service scene, prefer closest to entrance if time-sensitive, random if no preference). Store selected spot reference on Scene instance.

Spot integration enables location-based service scenes (can't have lodging without lockable room, can't have storage without secure vault, can't have private training without isolated space).

### Phase 4: Item Lifecycle Management

Implement scene-scoped item tracking:

**Scene Item Tracking:** Add GrantedItemIds list to Scene instance tracking which items this scene gave player. When situation grants item (via reward), also add to scene's tracking list. When scene completes, cleanup logic iterates tracking list and removes any still in player inventory.

**Situation Item Requirements:** Expand requirement system to check player inventory for specific items. Access situation requires room_key (granted by prior negotiation situation). If player lacks key, situation locked even if previous situation completed (player discarded key somehow).

**Conditional Item Removal:** Some items removed only under conditions. If player uses key to access room, consumed immediately. If player never accessed room (departed without using service), key removed at cleanup. Flexible item lifecycle patterns.

**Permanent vs Temporary Items:** Item definitions specify whether scene-managed (temporary access token) or permanent (reward). Scene cleanup only removes scene-managed items, never touches permanent rewards.

This prevents item pollution (temporary tokens persisting inappropriately) and enables complex item-gated progressions within scenes.

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

---

## Critical Constraints

### Scene Archetype Complexity

Scene archetypes are substantially more complex than situation archetypes. Situation archetype generates 2-4 ChoiceTemplates (single decision point). Scene archetype generates 3-7 SituationTemplates each with 2-4 ChoiceTemplates plus SpawnRules plus item lifecycle plus economics (complete multi-situation arc).

Expect scene archetype generation methods to be 100-300 lines implementing complete pattern logic. This is significant engineering effort. Start with simplest archetypes (linear service scenes), validate thoroughly, then tackle complex patterns (branching investigations, recursive crises).

Scene archetypes are reusable mechanical art. Once "service with location access" archetype exists, applies to lodging/bathing/healing/storage/training contextually. Effort justified by infinite reuse.

### HIGHLANDER for Scene Arcs

All situations within scene exist in GameWorld.Situations flat list, not nested in Scene property. Scene stores situation ID references. Situation stores Scene reference for parent queries. No duplication.

SpawnRules reference situation TEMPLATE IDs (from template), not situation instance IDs (runtime). Runtime spawn logic looks up template by ID, instantiates NEW situation from template, adds to GameWorld.Situations. Template IDs stable, instance IDs unique per spawn.

Item tracking lives on Scene (this scene granted these items), not duplicated on Player (player has items, doesn't know source). Scene cleanup queries own tracking list, removes those items from player inventory. Scene owns lifecycle for items it introduced.

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

**Output Structure:** Returns complete SceneTemplate containing: embedded List<SituationTemplate> (4 situations for service archetype), each SituationTemplate with List<ChoiceTemplate>, SpawnRules defining situation flow, item grant/consume/remove patterns, state modification logic, cleanup requirements. Complete arc structure from property queries.

**Tutorial Usage:** Parser resolves concrete entity IDs (elena_innkeeper, tavern_common_room, tavern_upper_floor) to entity objects. Passes objects to generation method. Receives complete scene structure. No situations or choices in JSON, all generated from properties.

**Procedural Usage:** Placement evaluator filters entities by categorical properties (Personality=Innkeeper, Services=Lodging, Privacy=Medium). Selects matching entities. Passes entity objects to generation method (SAME METHOD). Receives contextually appropriate scene structure. Identical generation path.

**Property-Driven Variation:** Elena (Friendly) generates easy challenge (12 Focus). Thorne (Hostile) generates hard challenge (25 Focus). Same formula (tierBase × demeanorMultiplier), different property values, contextually appropriate difficulty. Economic balance maintained via multiplicative scaling.

**This Catalog is Complex:** Each archetype generation is 100-300 lines implementing complete pattern with property queries. Service archetype must: query NPC demeanor for difficulty, query location services for benefit type, query spot privacy for cost premium, generate 4 situations with choices, define spawn rules, manage item lifecycle, ensure cleanup. Significant engineering but unlimited reuse.

### SceneTemplate

Immutable scene structure. Two authoring patterns:

**Tutorial (Concrete Entities):**
```json
{
  "id": "tutorial_lodging",
  "archetypeId": "service_with_location_access",
  "tier": 0,
  "isStarter": true,
  "concreteNpcId": "elena_innkeeper",
  "concreteLocationId": "tavern_common_room",
  "concreteSpotId": "tavern_upper_floor"
}
```

No situations array. No choices array. Parser resolves entity IDs to objects, passes to archetype generation, receives complete scene structure.

**Procedural (Categorical Filters):**
```json
{
  "id": "lodging_service",
  "archetypeId": "service_with_location_access", 
  "tier": 1,
  "placementFilter": {
    "npcPersonalities": ["Innkeeper"],
    "locationServices": ["Lodging"],
    "spotPrivacy": ["Medium", "Private"],
    "spotFunctionality": ["Rest"]
  },
  "spawnConditions": {
    "minCoins": 20,
    "completedScenes": ["tutorial_lodging"]
  }
}
```

No situations array. No choices array. Placement evaluator finds matching entities, passes objects to archetype generation, receives complete scene structure.

**Both Paths Identical After Entity Resolution:** Tutorial parser and procedural evaluator both produce: NPC object, Location object, LocationSpot object. Both invoke identical archetype generation method. Both receive identical SceneTemplate output structure (embedded SituationTemplates with ChoiceTemplates, SpawnRules, item lifecycle, cleanup patterns).

**Templates Contain Zero Narrative Text:** Pure structure and mechanics. AI generates all narrative from entity properties at situation activation. Template specifies archetype pattern and entity requirements, generation produces mechanical structure, AI produces contextual narrative.

### Scene Entity

Runtime scene instance tracking: CurrentSituationId (progression through arc), List<string> SituationIds (references to GameWorld.Situations), GrantedItemIds (item lifecycle tracking), PlacedNPC/PlacedLocation/PlacedSpot (strongly-typed entity references AND IDs), State (Provisional/Active/Completed), Template (reference to SceneTemplate).

Owns state machine methods: AdvanceToNextSituation (progression control), ExecuteCleanup (restoration logic), IsComplete (completion detection). Domain entity owns lifecycle, not scattered across services.

### Situation Entity

Runtime situation instance within scene: Template reference (for lazy action instantiation), ParentScene reference (for context queries), InstantiationState (Deferred/Instantiated), LifecycleStatus (Locked/Selectable/InProgress/Completed/Failed), GeneratedDescription (AI-produced narrative text stored directly), per-choice ActionTexts (AI-generated strings for each choice).

Situations know position in arc via template properties. Opening situations introduce, conclusion situations wrap up. Position affects narrative generation and mechanical behavior.

### SituationTemplate

Embedded in SceneTemplate, not separate top-level entity. Defines single situation: List<ChoiceTemplate> (player options at this beat), TimeCost (how much time this beat consumes), ItemGrants/ItemRequirements (lifecycle participation), NarrativeHints (for AI generation specific to this beat), PositionIndicator (opening/intermediate/conclusion).

Contains NO narrative text. AI generates all narrative from entity contexts and hints.

### SpawnRules

List of situation transitions defining scene flow. Each transition specifies: SourceSituationTemplateId (which template), DestinationSituationTemplateId (which template spawns), Condition (success/failure/always), RequirementOffsets (if applicable). Scene instance executes these rules to advance progression.

Linear scenes have simple rules (A→B→C→D). Branching scenes have conditional rules (A→B if success, A→C if failure). Hub-and-spoke scenes have parallel rules (A→[B,C,D], [B,C,D]→E when all complete).

### PlacementFilterEvaluator

Expanded to handle three-entity resolution: NPC + Location + Spot.

Receives PlacementFilter with NPC requirements (personality types), Location requirements (properties), Spot requirements (functionality, privacy, accessibility). Queries GameWorld for matching NPCs, filters by locations where NPC present with properties, filters by spots at those locations with requirements.

Returns triple (NPC, Location, Spot) or null. Applies selection strategies if multiple matches (prefer highest bond NPC, prefer highest privacy spot, prefer closest location).

### SceneInstantiator

Factory creating scene instances from templates. Resolves placement (categorical filter → concrete NPC/Location/Spot triple). Instantiates all situations in scene simultaneously as Dormant entities in GameWorld.Situations. Binds entity references (NPC object, Location object, Spot object) onto Scene. Marks first situation as current.

At finalization (if scene provisional), doesn't instantiate situations yet (already done). Just transitions scene Provisional→Active. Situations activate individually as player progresses, not all at finalization.

### AI Generation Service (FUTURE)

Context bundling per situation: collect NPC/Location/Spot entity objects, situation position indicator, service type, player state, items held. Package as typed context.

Generation call per situation activation: AI receives context and narrative hints, returns situation description + all choice action texts + outcome narratives. Store directly on Situation properties.

Sequential generation: Situation 2 generates with updated context (situation 1 outcome, possibly advanced time). Situation 3 generates with further updates. Emergent coherent narrative from context-aware sequential generation.

---

This architecture supports procedural generation of complete multi-situation narrative arcs while maintaining mechanical coherence and economic balance. Scene archetypes define reusable arc patterns. AI provides contextual narrative for each beat. Game logic orchestrates progression through situations with state modification, item lifecycle, and time advancement. Templates contain pure structure, instances track runtime state, entities own their lifecycle behavior.