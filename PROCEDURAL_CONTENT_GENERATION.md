# Procedural Content Generation Architecture

## Problem & Motivation

### The Scaling Crisis

Wayfarer's narrative RPG model creates a content volume problem. A scene like "Secure Lodging" encompasses: the innkeeper conversation, payment or persuasion, receiving a room key, navigating to the locked upper floor, resting overnight with resource restoration, time passing to morning, waking up, leaving the room, and the room locking again. This entire multi-situation arc with location state changes, item lifecycle, resource restoration, and time advancement is ONE scene template.

If every merchant needs a hand-authored "purchase services" scene, every guard needs a custom "checkpoint passage" scene, and every location needs unique "rest and recovery" scenes, the authoring burden becomes exponential. Each scene requires: multiple situation flows, choice structures at each beat, mechanical consequences, world state modifications, narrative coherence across the arc, and cleanup logic. Creating one complete scene might take days of design and balancing work.

The traditional solution - procedural generation - fails because these complex multi-situation arcs require careful mechanical orchestration. Generated scenes produce broken progression (situations spawn out of order), unbalanced economics (rest costs too little or too much), narrative incoherence (morning departure happens before sleeping), and broken state management (keys granted but never removed).

### The AI Limitation

Large language models can generate compelling narrative for individual moments but cannot orchestrate complex multi-situation arcs with mechanical coherence. AI can write engaging dialogue for innkeeper negotiation but has no concept of "this grants an item that unlocks a location which enables a rest situation which restores resources using formulas and advances time which triggers morning situation which removes the key and re-locks the location". 

Scene structure requires understanding: situation sequencing (negotiation BEFORE access BEFORE rest BEFORE departure), resource flow (key granted then consumed, energy depleted then restored, coins spent never returned), world state causality (time advances causing morning, rest causes restoration, departure causes lock), and economic balance (rest benefit must justify cost across progression curve).

### The Core Insight

Separate scene archetype patterns from narrative manifestation and entity context. Game logic defines multi-situation arc structure: "service with location access" contains negotiation situation (challenge or payment grants access item), access situation (consume item to unlock location), service situation (consume time/resources for benefit), and departure situation (remove item, re-lock location). This pattern works for: lodging (sleep restores resources), bathing (time for cleanliness stat), storage (coins for inventory space), training (time for skill gain), healing (coins for health restoration).

AI applies narrative texture: the lodging scene happens at an inn with Elena the innkeeper and mentions beds, while bathing happens at bathhouse with attendant Marcus and mentions hot water, while healing happens at temple with priest Thalia and mentions prayers. Same mechanical structure, infinite narrative variations based on service type, NPC personality, location atmosphere, cultural context.

The architecture must support complete scene arc generation from archetypes while allowing AI to provide contextual narrative for each situation within the arc.

### Why This Matters

Without procedural scene generation, every service location needs hand-authored multi-situation arcs. With naive procedural generation, scenes break mechanically (wrong situation ordering, unbalanced costs, broken state). With AI-only generation, scenes lack mechanical coherence (made-up costs, arbitrary progression, broken economy). The solution must generate mechanically sound multi-situation arcs while allowing AI to provide rich narrative at each beat.

---

## Architectural Discovery

### Scene Scope Understanding

Scenes are NOT single conversations with 2-4 choices. Scenes are complete multi-situation narrative arcs with:

**Sequential Situation Progression:** Multiple situations flowing in defined order. Negotiation situation completes, spawns access situation. Access situation completes, spawns service situation. Service situation completes, spawns departure situation. Scene progression rules define this flow (linear, branching, hub-and-spoke, conditional).

**Location State Modification:** Scenes change world state. Location lock states toggle (locked becomes unlocked becomes locked again). NPC availability changes (merchant closes shop, guard changes post). Environmental properties shift (time of day advances, weather changes, danger levels update).

**Item Lifecycle Management:** Scenes grant items (room key, ticket, permit) that exist temporarily within scene scope. Item granted in situation 1, consumed in situation 2, removed in situation 4. Items don't persist beyond scene - they're arc-specific temporary tokens enabling progression.

**Resource Flow Orchestration:** Scenes consume resources (coins, time, energy) and produce benefits (restored stamina, gained knowledge, increased stats). Economic balance maintained across entire arc. Early situations cost resources, later situations provide benefits, net outcome justified by total cost.

**World State Causality:** Actions in one situation cause consequences in later situations. Paying coins in negotiation situation affects what happens in service situation (paid upfront versus pay later). Time advancement in rest situation triggers morning-specific narrative in departure situation. Prior choices ripple forward through arc.

**Cleanup and Reset Logic:** Scenes restore world to sensible state after completion. Temporary items removed. Locked locations re-locked. NPC availability reset. Time advanced appropriately. Next player visit finds world in coherent state, not broken by prior scene completion.

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

Location lock states stored once on LocationLocation entity. Access situation modifies location.IsLocked property, departure situation restores it, but lock state isn't cached in scene. Query location entity for current state, don't maintain duplicate tracking.

### Catalogue Constraint for Scene Archetypes

Scene archetype catalogues generate complete multi-situation structures at parse time exclusively. A scene archetype like "service_with_location_access" receives context (service type, tier, expected player resources, location properties) and outputs: complete sequence of SituationTemplates, ChoiceTemplates for each situation, cost/benefit formulas, item grant/consume patterns, SpawnRules governing flow, and cleanup logic.

The catalogue performs complex generation: calculates balanced economics (total cost versus total benefit), determines appropriate item types (temporary access tokens), defines sensible progression (negotiation before access before service), sets tier-appropriate costs (scales with player progression), and embeds default narrative hints (service type influences tone).

This generation happens ONCE per template load. Runtime never calls archetype catalogues. After parsing, Scene entities have concrete SituationTemplate lists with embedded ChoiceTemplates. The archetype ID becomes metadata for content tools. Runtime uses strongly-typed template structures, never regenerates from archetype.

This ensures fail-fast behavior (broken archetypes crash at load) and architectural boundaries (parsers import catalogues, game logic never does).

### Entity Type Requirements for Scene Context

Scene templates specify required entity contexts without narrative text. "Service with location access" template declares: requires NPC entity (service provider), requires Location entity (where service happens), requires LocationLocation entity (locked area player accesses), may require items as prerequisites.

These are categorical requirements. Template says "needs Merchant or Innkeeper personality NPC" and "needs Urban location with Lodging property" and "needs lockable interior location", not concrete entity IDs (except tutorial).

At instantiation, placement filter evaluates categorical requirements against game world entities, selects concrete matches (Elena NPC, Tavern location, Upper Floor location), binds strongly-typed entity references onto Scene instance. Scene stores NPC object, Location object, LocationLocation object as properties.

At finalization for each situation, AI generation receives entity contexts: NPC object with all properties (personality, background, bond, relationship history), Location object with all properties (atmosphere, current time, weather, properties), service type (lodging/bathing/healing), situation position in arc (first/middle/last), prior situation outcomes.

AI generates complete narrative for that situation: situation description, all choice action texts, success/failure narrative, transition narrative to next situation. Each situation gets contextually appropriate narrative based on where it sits in arc and what entities it involves.

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

### Scene as Multi-Situation Arc

Scenes are self-contained narrative arcs with beginning, middle, end. Not single moments, but complete story beats spanning multiple player decisions and world state changes.

**Beginning:** Setup situation establishes premise. Innkeeper conversation, guard confrontation, merchant pitch. Player learns what scene offers and what it costs. Choice determines how arc progresses (challenge versus payment versus refusal).

**Middle:** Execution situations deliver on promise. Access locked area, perform service, achieve goal. May involve navigation (move to location), time passage (wait for result), resource application (consume items/energy). Mechanical benefit applied here.

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

**Consumption Phase:** Middle situation requires item to proceed. Accessing locked location consumes room key, presenting permit allows checkpoint passage, showing token enables service. Item checked against requirements, consumed if appropriate.

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

**Temporary State Changes:** Location locks toggle for scene duration. Upper floor unlocked when player has key, re-locked when scene completes. NPC availability shifts (shopkeeper busy during your transaction). These revert after scene.

**Permanent State Changes:** Location properties modified permanently. Completing investigation at location adds "Investigated" property preventing repeat. Causing scene failure might add "Hostile" property changing future interactions. These persist beyond scene.

**Time Advancement:** Service situations advance world clock. Resting advances to morning. Long travels advance multiple time blocks. Training advances by days. Time affects all world state (NPC schedules, shop availability, random events, spawn eligibility for time-gated scenes).

**State Restoration:** Cleanup situations ensure coherent world state. Can't leave location permanently unlocked (breaks future lodging scenes). Can't leave NPC in "transaction active" state. Can't leave time frozen. Scene completion restores or advances appropriately.

Scenes don't modify structural entities (won't delete locations or NPCs), only modify state properties on those entities temporarily.

---

## Current State Analysis

### Tutorial Scene Structure

Tutorial "Secure Lodging" scene is hand-authored demonstrating complete arc pattern:

**Situation 1 - Negotiation (Opening):** Talk to Elena, choose challenge (costs Focus, grants bond) or payment (costs coins, no bond). Completion grants room_key item, activates situation 2.

**Situation 2 - Access (Intermediate):** Navigate to upper floor location (now unlocked due to room_key). Completion consumes room_key (or stores for later), activates situation 3.

**Situation 3 - Rest (Intermediate):** Sleep for night. Costs time (advances to morning). Restores stamina/energy. Completion activates situation 4.

**Situation 4 - Departure (Conclusion):** Leave room. Removes room_key if still held. Re-locks upper floor location. Scene complete.

This establishes pattern vocabulary: multi-situation flow, item lifecycle, resource restoration, state cleanup. Players learn "scenes are arcs not moments" through handcrafted example. Procedural scenes must maintain this structural pattern.

### Scene Archetype Catalog Minimal

Situation archetype catalog exists (generates individual situation choice structures) but no scene archetype catalog exists. No code generates complete multi-situation arcs from archetypes. Each scene currently hand-authored defining complete situation sequence.

Need scene archetype catalog that generates: situation sequences, spawn rules, item grant/consume/remove patterns, resource cost/benefit formulas, state modification logic, cleanup sequences. This is more complex than situation archetype generation (single choice structure versus complete arc orchestration).

### Spawn Rule Execution Exists

Scene domain entity has SpawnRules property and situations have completion tracking, but Scene lacks methods to execute rules. SituationCompletionHandler checks rules and spawns next situations, but this is service responsibility not domain entity behavior.

Architecture expects Scene.AdvanceToNextSituation() method that queries own SpawnRules, finds transition for completed situation, activates next situation(s), updates CurrentSituationId, marks scene complete if no more situations. Domain entity should own state machine, not have lifecycle scattered across services.

### Placement Filter Supports Locations

PlacementFilter evaluates NPC and Location entities, but scenes also need LocationLocation references (for lockable areas). Filter can specify location properties (Lodging, Bathing, Healing) but doesn't specify location requirements (needs lockable interior room, needs private chamber, needs secluded area).

Scene instantiation currently hardcodes location selection (first matching location at location). Need location filtering (accessibility, privacy level, capacity) and selection strategies (prefer higher privacy if affordable, prefer closer to entrance if rushed).

### Item System Exists But Scene Integration Unclear

Player.Inventory exists, items can be granted/removed. Location.RequiredItems exists for locking mechanism. But scene-managed temporary items unclear. Does room_key persist in inventory forever, or removed by scene? Does scene track "I granted this key, I must clean it up"?

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

### Categorical Placement with Location Filtering

Scenes need THREE entity contexts, not just two:

**NPC Context:** Service provider with appropriate personality. Innkeeper for lodging, attendant for bathing, priest for healing, guard for checkpoints. Personality affects narrative tone and negotiation difficulty.

**Location Context:** Appropriate location type with required properties. Inn for lodging (Lodging property), bathhouse for bathing (Bathing property), temple for healing (Healing property). Location atmosphere affects narrative setting.

**Location Context:** Specific lockable area player accesses. Upper floor room at inn, private bath chamber at bathhouse, healing sanctuary at temple. Location privacy level affects service quality (private rooms better than shared).

Placement filter evaluates all three: "any Innkeeper NPC at any Urban location with Lodging property at any location with lockable Interior location with privacy level Medium+". Resolution finds concrete matches across three entity types simultaneously.

Location filtering needs properties: accessibility (public/private/restricted), privacy (shared/semi-private/private), capacity (number of users), functionality (rest/storage/service). These enable selection strategies (prefer private if affordable, accept shared if cheap) and ensure mechanical appropriateness (can't rest in public square, can't store items in busy corridor).

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

**State Restoration:** List world states to restore. Upper floor location re-locked, NPC availability reset to normal, environmental states reverted. Ensures next player finds consistent world state.

**Conditional Cleanup:** Some cleanup conditional on scene outcome. Successful negotiation increases NPC bond permanently (not cleaned up). Failed negotiation might add temporary negative state that expires. Success/failure determines what persists.

**Time Validation:** Ensure time advanced sensibly. Can't complete rest scene without advancing time. Can't complete instant transaction while claiming hours passed. Validation catches inconsistent time handling.

Scene archetype generation includes cleanup logic appropriate to archetype type. Service scenes clean up access items and re-lock locations. Investigation scenes don't clean up knowledge gained. Transaction scenes clean up negotiation state but not purchased items.

---

## Implementation Strategy

### Phase 1: Scene Archetype Catalog Creation

Build new catalog generating complete multi-situation scene structures from archetype IDs:

**Service with Location Access Archetype:** Generates four SituationTemplates (negotiate with NPC, access locked location, perform service, depart and cleanup), each SituationTemplate has ChoiceTemplates appropriate to situation type (negotiate has challenge/payment/refuse, access has enter, service has use/leave-early, depart has automatic cleanup). Generates SpawnRules (negotiate-complete → access, access-complete → service, service-complete → depart). Generates item lifecycle (grant room_key, require room_key, remove room_key). Generates resource economics (cost formulas in negotiate, benefit formulas in service). Generates state modifications (unlock location, advance time, re-lock location).

**Transaction Sequence Archetype:** Generates three SituationTemplates (browse inventory, negotiate price, complete transaction). More complex than service (inventory browsing has many items, negotiation has haggling mechanics, transaction has multiple payment options). Generates conditional branching (browse-select-item → negotiate, negotiate-accept → transact, negotiate-refuse → browse-again).

**Investigation Arc Archetype:** Generates variable situation count based on complexity parameter (3-7 situations). Hub-and-spoke pattern (initial situation spawns multiple investigation situations, all must complete before conclusion situation). Evidence gathering mechanics (each investigation grants evidence item, conclusion requires evidence threshold). More complex spawn rules (parallel situations with convergence).

Document archetype capabilities: what situations generated, what spawn patterns used, what economics applied, what states modified, what items managed. Each archetype is substantial implementation effort (50-200 lines of generation logic), not simple templates.

Test archetypes in isolation with hand-authored test parameters. Verify generated situations make mechanical sense, spawn rules create logical flow, economics balance across tier ranges, state modifications don't break world consistency.

### Phase 2: Scene State Machine Implementation

Add domain methods to Scene entity for arc progression control:

**Scene.AdvanceToNextSituation(completedSituationId):** Queries SpawnRules, finds transitions from completed situation, gets destination situation template IDs, instantiates destination situations as runtime entities (add to GameWorld.Situations with scene ID reference), transitions those situations from nonexistent to Dormant state, updates CurrentSituationId to point to newly spawned situation(s), marks scene complete if no valid transitions exist.

**Scene.ExecuteCleanup():** Queries template CleanupPatterns, removes temporary items from player inventory, restores location lock states, reverts NPC availability, validates time advanced appropriately, marks scene as fully resolved.

**Scene.IsComplete():** Checks if CurrentSituationId points to conclusion situation and that situation is Completed, or if no valid spawn transitions remain. Boolean indicating arc finished.

These methods encapsulate scene lifecycle. SituationCompletionHandler calls Scene.AdvanceToNextSituation() after situation resolves. GameFacade calls Scene.ExecuteCleanup() when scene marked complete. Domain entity owns state machine, services orchestrate calls.

### Phase 3: Location Filtering and Integration

Extend placement filtering to handle LocationLocation entities:

**Location Properties:** Add properties to LocationLocation entity (Accessibility enum, Privacy enum, Capacity int, Functionality enum). These enable filtering (need private lockable interior location for lodging, need public accessible exterior location for market stall).

**Placement Filter Location Requirements:** Add LocationRequirements to PlacementFilter (functionality required, minimum privacy, accessibility constraints). Scene templates declare location needs alongside NPC and location requirements.

**Three-Entity Resolution:** PlacementFilterEvaluator resolves NPC + Location + Location simultaneously. Find NPCs matching personality, filter locations where NPC present with required properties, filter locations at those locations matching location requirements. Return triple (NPC, Location, Location) or null if no match.

**Location Selection Strategies:** When multiple locations match, apply template preference (prefer highest privacy if service scene, prefer closest to entrance if time-sensitive, random if no preference). Store selected location reference on Scene instance.

Location integration enables location-based service scenes (can't have lodging without lockable room, can't have storage without secure vault, can't have private training without isolated space).

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

**Context Bundling Per Situation:** When situation activates (transitions Dormant → Active), collect: NPC entity object (all properties), Location entity object (all properties including CURRENT time), Location entity object (properties), situation position indicator (opening/intermediate/conclusion), service type or scene theme, player state (current resources, prior choices in this scene), items held relevant to this situation. Package as typed context object.

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

AI receives: NPC entity object (personality, background, bond, history), Location entity object (atmosphere, time, properties), Location entity object (privacy, accessibility), situation position in arc (opening/middle/conclusion), service type or theme. Full entity contexts with rich properties.

AI does NOT receive: cost formulas, requirement thresholds, spawn rules, mechanical balance ratios, resource restoration amounts. These are game logic concerns. AI generates narrative describing WHAT happens, not determining COSTS.

AI returns: pure narrative strings (situation descriptions, choice texts, outcome narratives). No mechanical decisions. Generated text stored directly on Situation properties, no transformation needed.

### Entity References Are Strongly Typed

Scene stores NPC entity object reference (for AI context), Location entity object reference (for AI context), LocationLocation entity object reference (for access control), PLUS corresponding IDs for each (for serialization).

Object references are primary runtime mechanism. AI generation, requirement evaluation, spawn logic all use typed objects with full property access. IDs only for save/load.

This dual storage (ID + object) acceptable because: same concept, different purposes, both populated once at instantiation, no ongoing synchronization risk.

---

## Key Files & Their Roles

### SceneArchetypeCatalog (NEW)

Generates complete multi-situation scene structures from archetype IDs. Called exclusively during parse phase.

Methods include: GenerateServiceWithLocationAccess (produces 4-situation service arc), GenerateTransactionSequence (produces 3-situation shopping arc), GenerateInvestigationArc (produces variable-length investigation flow), each returning complete SceneTemplate with embedded SituationTemplates and SpawnRules.

This catalog is complex. Each archetype generation method orchestrates: situation count/types, choice structures per situation, spawn rule definitions, item grant/consume/remove patterns, cost/benefit formulas across arc, state modification logic, cleanup requirements. Substantial engineering effort per archetype, unlimited reuse after implementation.

### SceneTemplate

Immutable scene structure containing: embedded List<SituationTemplate> (all situations in arc), SpawnRules (transitions between situations), PlacementFilter (what NPC/Location/Location required), SpawnConditions (when eligible), CleanupPatterns (what to restore), economic metadata (cost/benefit tier scaling), narrative hints per situation (for AI generation).

Templates contain NO narrative text. Pure structure and mechanics. AI generates narrative per-situation from entity contexts.

Hand-authored for tutorial (concrete entity IDs, exact costs). Generated procedurally post-tutorial (categorical filters, formula-driven costs). Always immutable after parsing.

### Scene Entity

Runtime scene instance tracking: CurrentSituationId (progression through arc), List<string> SituationIds (references to GameWorld.Situations), GrantedItemIds (item lifecycle tracking), PlacedNPC/PlacedLocation/PlacedLocation (strongly-typed entity references AND IDs), State (Provisional/Active/Completed), Template (reference to SceneTemplate).

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

Expanded to handle three-entity resolution: NPC + Location + Location.

Receives PlacementFilter with NPC requirements (personality types), Location requirements (properties), Location requirements (functionality, privacy, accessibility). Queries GameWorld for matching NPCs, filters by locations where NPC present with properties, filters by locations at those locations with requirements.

Returns triple (NPC, Location, Location) or null. Applies selection strategies if multiple matches (prefer highest bond NPC, prefer highest privacy location, prefer closest location).

### SceneInstantiator

Factory creating scene instances from templates. Resolves placement (categorical filter → concrete NPC/Location/Location triple). Instantiates all situations in scene simultaneously as Dormant entities in GameWorld.Situations. Binds entity references (NPC object, Location object, Location object) onto Scene. Marks first situation as current.

At finalization (if scene provisional), doesn't instantiate situations yet (already done). Just transitions scene Provisional→Active. Situations activate individually as player progresses, not all at finalization.

### AI Generation Service (FUTURE)

Context bundling per situation: collect NPC/Location/Location entity objects, situation position indicator, service type, player state, items held. Package as typed context.

Generation call per situation activation: AI receives context and narrative hints, returns situation description + all choice action texts + outcome narratives. Store directly on Situation properties.

Sequential generation: Situation 2 generates with updated context (situation 1 outcome, possibly advanced time). Situation 3 generates with further updates. Emergent coherent narrative from context-aware sequential generation.

---

This architecture supports procedural generation of complete multi-situation narrative arcs while maintaining mechanical coherence and economic balance. Scene archetypes define reusable arc patterns. AI provides contextual narrative for each beat. Game logic orchestrates progression through situations with state modification, item lifecycle, and time advancement. Templates contain pure structure, instances track runtime state, entities own their lifecycle behavior.