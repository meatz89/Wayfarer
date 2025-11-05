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

## Implementation Status

The codebase implements the core property-driven generation philosophy while making pragmatic improvements to the original design. Implementation choices often prove superior to initial architectural proposals.

### Working Scene Archetypes (6 Total)

**SceneArchetypeCatalog.cs** generates complete multi-situation structures at parse time using property queries. Archetypes: `service_with_location_access` (lodging/bathing/healing), `transaction_sequence` (shopping), `gatekeeper_sequence` (checkpoints), `consequence_reflection` (aftermath), `single_situation` (quick interactions), `inn_crisis_escalation` (branching emergencies). All property-driven (query NPC.Demeanor, Location.Services, etc).

### Multi-Scene NPC Display

UI layer supports multiple concurrent scenes at single NPC. Query pattern changed from FirstOrDefault (single scene) to Where (all scenes). Each active scene renders as separate interaction button. Navigation routing includes (npcId, sceneId) tuple for direct scene lookup. Physical NPC presence always visible, interaction buttons conditional on scene availability.

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

**Hex Grid Integration:** Dependent locations specify relative placement (adjacent to base location, specific distance, directional preference). Scene instantiation queries hex grid for valid placement, creates location at that position. Existing intra-venue travel logic automatically generates movement routes between adjacent locations. No special routing logic needed.

**Resource Naming and Identity:** Created resources use pattern-based naming incorporating context. Upper room becomes "Elena's Private Room" when spawned at Elena's inn, "Marcus's Bath Chamber" at Marcus's bathhouse. Item becomes "room_key_scene_{sceneId}" ensuring uniqueness per instance. Names derived from placement context, not pre-authored.

**Lifecycle Ownership:** Scene tracks all resources it created. Scene entity maintains lists: created location IDs, created item IDs. Scene owns complete lifecycle: creation at spawn, modification during progression, cleanup at completion. No orphaned resources when scene completes.

**Cleanup Strategies:** Template defines cleanup behavior per dependent resource. Permanent lock (location exists but forever inaccessible after scene), temporary removal (location deleted from world after scene), reusable (location remains accessible for future scene spawns). Items typically scene-scoped (removed at completion) or permanent rewards (persist in player inventory).

**Self-Containment Benefits:** Scene never references world entities that might not exist. Upper room location guaranteed to exist because scene creates it. Room key guaranteed to exist because scene creates it. Situations reference created resources through template indices (dependentLocations[0], dependentItems[0]) not brittle ID strings. Reproducibility guaranteed: same external context always creates identical dependent structure.

**Tutorial and Procedural Identical:** Tutorial scenes and procedural scenes use identical resource creation pipeline. Tutorial "Secure Lodging" spawns at Elena + Tavern Common Room (concrete), creates upper room adjacent, generates room key. Procedural "Lodging Service" spawns at any Innkeeper + Lodging location (categorical), creates upper room adjacent, generates room key. Same specifications, same creation logic, different placement context.

**Package Completeness:** Scene template is complete specification of everything scene needs. External dependencies declared through filters. Internal dependencies declared through specifications. Situations reference created resources. Archetype generation produces complete package from minimal external bindings. Scene can spawn anywhere matching filters without world modification.

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

**Spawn Independence:** Tutorial scenes spawn at parse-time (concrete bindings), obligation scenes spawn at runtime (categorical filters), multiple obligations can spawn scenes at same NPC simultaneously. Each operates independently.

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

Items can also be permanent rewards. Completing investigation grants knowledge_fragment that persists. Choice rewards distinguish: temporary access tokens (removed by departure), permanent rewards (never removed), consumable items (used once then depleted).

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
- **Join Traveler's Guild** (10 coins, grants guild_token item)
- **Decline** (keep coins)

This choice creates branching for Secure Lodging scene. Players who joined have free access option. Players who declined see locked option teaching consequence: "I saved 10 coins but now need 15 coins or 15 Focus for lodging."

**Parse-Time Generation:** SceneArchetypeCatalog.GenerateServiceWithLocationAccess receives: archetype ID, tier 0, Elena entity object (Personality=Innkeeper, Demeanor=Friendly, Authority=Low), Tavern Common Room location object (Services=Lodging), player state benchmarks.

Generation queries properties and produces complete self-contained package:

**Dependent Resources Created at Spawn:**
- Upper Room location (adjacent hex to Tavern Common Room, pattern name "Elena's Private Room", IsLocked=true, Services=Lodging, lifecycle=PermanentLock)
- Room Key item (pattern name "Room Key", unique per scene instance, lifecycle=SceneScoped)

**Situation Structure (references created resources):**

**Situation 1 - Negotiation (Common Room + Elena):** Three choices demonstrating multiple solution paths:

1. **Convince Elena** - Social challenge (15 Focus, Friendly demeanor = easier difficulty, grants +1 bond)
2. **Pay Elena** - Direct transaction (15 coins, tier 0 base cost)
3. **Show Traveler's Guild Token** - Free access (requires HasItem: guild_token, **LOCKED** if player declined guild membership earlier)

The locked third option teaches perfect information and consequence. Earlier tutorial scene offers Guild Recruiter: join for 10 coins (grants guild_token) or decline. Players who declined see locked option at Elena, understand: "I could have free lodging if I'd paid 10 coins for guild membership. Now I need 15 coins or 15 Focus." Players who joined use token, get free room, learn guild membership has ongoing value. All three ChoiceTemplates generated from property queries. Successful choices grant scene-created room_key item and unlock scene-created Upper Room location.

**Situation 2 - Access (Upper Room):** Navigate to created Upper Room location (now unlocked). Single choice (enter room) with HasItem requirement (scene-created room_key). Choice cost includes TimeSegments (time to walk upstairs). Location and item both created by scene at spawn, guaranteed to exist.

**Situation 3 - Rest (Upper Room):** Time cost 6 hours via ChoiceCost.TimeSegments (advances to morning). Benefit: restore 40 stamina (Tier 0 formula). Choice reward advances time to morning block.

**Situation 4 - Departure (Common Room + Elena):** Cleanup situation. ChoiceReward.ItemsToRemove=[scene-created room_key], LocationsToLock=[scene-created Upper Room]. Explicit author-visible cleanup. Single automatic choice (leave room). Upper Room permanently locked after scene completion (lifecycle=PermanentLock). Room key removed from inventory.

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
13. Player completes Situation 4 → Scene cleanup executes: removes room_key, permanently locks Upper Room
14. Scene marks complete, Upper Room remains on hex grid but inaccessible forever

Situations 2→3 cascade seamlessly (shared created Upper Room context). Situations 1→2 and 3→4 require navigation (context changes). Scene persists across navigation via CurrentSituationId. Created resources exist throughout scene lifecycle, cleaned up at completion.

**Tutorial Reproducibility:** Every playthrough generates identical scene because: same entities → same properties → same generation output. Elena always Friendly (easier challenge), Upper Floor always moderate cost, service always rest benefit.

**Post-Tutorial Variation:** Same archetype at Marcus's bathhouse generates: Professional demeanor (moderate difficulty), Bathing service (cleanliness benefit instead of stamina), different location context. Negotiation offers: (1) Convince Marcus challenge, (2) Pay standard fee, (3) Show Bathhouse Guild Membership (locked unless player joined). At Thalia's temple: (1) Convince Thalia challenge, (2) Donate to temple, (3) Show Healer's Credential (locked unless player earned). Different entities, contextually appropriate variation, identical generation logic. Conditional bypass teaches: memberships and credentials have ongoing strategic value across multiple services.

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

**Service with Location Access:** Four situations (negotiate, access, service, depart). Negotiation offers three paths: challenge (tests player skill/resources), payment (direct transaction), conditional bypass (requires specific credential/membership). Used for: lodging (rest), bathing (cleanliness), storage (item management), training (skill increase), healing (health restoration). Cost varies by service type, benefit varies by tier, but structure identical. Conditional bypass teaches long-term strategic value (guild membership grants free lodging, physician credential grants free healing, noble seal grants privileged access).

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

**Locked Choices as Teaching:** Seeing unavailable options teaches consequence and strategic value. Guild token choice locked at Elena shows: "I could have free lodging if I'd joined guild for 10 coins earlier. Now I need more resources." Player learns to evaluate long-term trade-offs and opportunity costs. Future playthroughs informed by seeing paths not taken.

**Expectation Management:** Scene archetypes establish expectations. Service scenes restore resources (positive outcome). Investigation scenes grant knowledge (informational outcome). Crisis scenes create cascading consequences (potentially negative outcome). Genre conventions maintained by consistent archetype usage.

**Natural World Integration:** Context-aware progression maintains fiction coherence. Player negotiates lodging, physically travels to room, experiences service, physically returns to conclude. Scene arcs flow naturally through world geography rather than creating isolated narrative bubbles. Scenes persist across time and navigation.

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

**Generation Method Signature Pattern:** Each archetype generation method receives: archetype ID, tier, strongly-typed entity objects (NPC for service provider, base Location for service context), player state benchmarks. NOT spot entity - archetype creates dependent location.

**Property Query Pattern:** Generation logic queries entity properties to determine contextual mechanical values. Challenge difficulty queries npc.Demeanor (Friendly=0.8x multiplier, Hostile=1.4x). Payment cost queries location.CostModifier. Service benefit queries location.Services (Lodging=stamina, Bathing=cleanliness). Narrative hints query npc.Personality and location.Atmosphere.

**Dependent Resource Specifications:** Archetype produces specifications for resources scene will create at spawn. Service archetype generates: dependent location specification (relative placement adjacent to base, pattern name incorporating NPC name, property requirements IsLocked + Services matching service type, lifecycle strategy PermanentLock), dependent item specification (pattern name "Room Key" or context-appropriate, lifecycle SceneScoped for removal at completion).

**Situation Generation:** Produces four SituationTemplates. Negotiate situation has ChoiceTemplates: (1) challenge with property-scaled difficulty, (2) payment with property-scaled cost, (3) conditional bypass option requiring specific item (guild_token, professional_credential, noble_seal based on service context). Access situation references created location and requires created item. Service situation references created location with property-determined benefit and time cost. Departure situation has cleanup specifying created item removal and created location lock restoration.

**SpawnRules Generation:** Linear flow (negotiate→access→service→depart) with success conditions. Each transition references situation template IDs from generated situations. Situations specify required contexts (base location for negotiate, created location for access/service, base location for depart).

**Item Lifecycle:** Grant created item in negotiate rewards, require created item in access situation, remove created item in depart cleanup. Item lifecycle completely managed within scene boundaries.

**Tutorial and Procedural Use Identical Logic:** Tutorial parser resolves concrete entity IDs to objects (Elena, Tavern Common Room), passes to generation. Procedural placement evaluator selects entities matching filters (any Innkeeper, any Lodging location), passes to generation. Generation receives entity objects either way, queries properties identically, produces contextually appropriate output including dependent resource specifications.

**Test Pattern:** Create test entities with extreme property values. Friendly (0.8x) vs Hostile (1.4x) NPCs should generate 1.75x difficulty difference. Urban (1.0x) vs Remote (1.5x) locations should show cost scaling. Verify formulas balance across property ranges. Test conditional bypass choices: verify requirements properly lock/unlock options based on player state (has guild_token vs lacks guild_token should show unlocked vs locked third choice). Verify dependent resource specifications produced correctly (location placement, item naming, lifecycle strategies).

### Phase 2: Scene State Machine and Resource Creation

Add domain methods to Scene entity for arc progression control with context-aware advancement and dependent resource lifecycle management:

**Scene.CreateDependentResources(gameWorld, hexGrid):** At scene spawn, reads template dependent resource specifications. Creates locations on hex grid adjacent to base location using pattern names incorporating placement context. Creates items with unique identifiers incorporating scene ID. Adds created location IDs and item IDs to scene tracking lists. Returns success indicating all resources created successfully. Resources guaranteed to exist because scene creates them.

**Scene.AdvanceToNextSituation(completedSituationId):** Queries SpawnRules, finds transitions from completed situation, gets destination situation template IDs, instantiates destination situations as runtime entities (add to GameWorld.Situations with scene ID reference), transitions those situations from nonexistent to Dormant state. **Critical context check:** Compares next situation's required location/NPC context against current context. If match (same location/NPC), immediately activates next situation and returns flag indicating "continue scene flow" (lock player in seamless cascade). If mismatch (different location/NPC), updates CurrentSituationId but leaves situation dormant, returns flag indicating "exit to world" (player must navigate). Marks scene complete if no valid transitions exist.

**Scene.ShouldActivateAtContext(locationId, npcId):** Queries CurrentSituationId, gets current situation template, compares situation's required context (location/NPC) against provided context. Location comparison checks both base location and created locations. Returns true if match, enabling situation activation when player reaches correct location. Supports scene persistence across navigation.

**Scene.ExecuteCleanup():** Queries template cleanup specifications and scene's created resource tracking lists. Removes temporary items from player inventory (including scene-created items). Applies lifecycle strategies to created locations (permanent lock, temporary removal, reusable state). Reverts NPC availability. Validates time advanced appropriately. Marks scene as fully resolved. No orphaned resources after cleanup.

**Scene.IsComplete():** Checks if CurrentSituationId points to conclusion situation and that situation is Completed, or if no valid spawn transitions remain. Boolean indicating arc finished.

These methods encapsulate scene lifecycle including dependent resource management. SceneInstantiator calls Scene.CreateDependentResources() after scene instantiation. SituationCompletionHandler calls Scene.AdvanceToNextSituation() after situation resolves, receives context-match flag determining whether to lock player in scene or return to world. LocationViewBuilder calls Scene.ShouldActivateAtContext() when displaying location to determine which dormant situations should activate. GameFacade calls Scene.ExecuteCleanup() when scene marked complete. Domain entity owns state machine and resource lifecycle, services orchestrate calls.

### Phase 3: Dependent Resource Management System

Implement resource creation pipeline for scenes to create locations and items at spawn time:

**Resource Specifications in Templates:** Scene templates include dependent resource specifications. Location specifications define: relative placement rules (adjacent to base, specific distance, directional preference), pattern-based naming incorporating placement context variables, required properties matching service type, lifecycle strategy determining post-completion behavior. Item specifications define: pattern-based naming incorporating context, uniqueness requirements per scene instance, lifecycle strategy determining removal timing.

**Hex Grid Placement:** When scene creates dependent location, queries hex grid for valid placement following specification rules. Adjacent placement finds first available hex neighboring base location. Directional placement searches specific direction from base. Distance placement finds hex at specified range. Created location added to hex grid, inherits world context, automatically integrates with existing intra-venue travel route generation.

**Pattern Name Resolution:** Resource specifications use patterns incorporating context variables. Upper room location pattern: "[NPC.Name]'s Private Room" resolves to "Elena's Private Room" when Elena is placement NPC. Item pattern: "Room Key" or "[Service] Access Token" resolves contextually. Patterns enable generic specifications producing contextual concrete names.

**Resource Tracking:** Scene maintains lists of created resource IDs. Created locations tracked separately from base location reference. Created items tracked separately from player inventory. Tracking enables cleanup to target only scene-created resources, leaving pre-existing world resources unmodified.

**Lifecycle Strategies:** Specifications declare cleanup behavior. Location lifecycle options: PermanentLock (location remains on grid but forever inaccessible), TemporaryRemove (location deleted from grid), Reusable (location remains accessible for future scenes). Item lifecycle options: SceneScoped (removed at completion), PlayerOwned (persists as permanent reward). Strategy determines ExecuteCleanup behavior.

**Reference Resolution:** Situations reference created resources through template indices or pattern identifiers, not brittle ID strings. Access situation requires "created_item[0]" or pattern reference "access_token". Service situation requires "created_location[0]" or pattern reference "service_location". Runtime resolution uses scene's tracking lists to find concrete resource IDs.

This system eliminates pre-existing resource dependencies. Scenes create what they need, use it, clean it up. No broken references when world entities missing or modified.

### Phase 4: Scene-Created Item Lifecycle Integration

Implement complete lifecycle management for items created and managed by scenes:

**Item Creation at Spawn:** Scene reads dependent item specifications from template, creates unique item instances incorporating scene ID in identifier, adds to GameWorld.Items catalog, adds item IDs to scene's CreatedItemIds tracking list. Items guaranteed unique per scene instance.

**Item Granting Through Situations:** Early situations grant scene-created items through choice rewards. Negotiate situation rewards include reference to created access item. Runtime reward application resolves item reference through scene tracking, adds concrete item to player inventory. Player receives contextually-named item visible in inventory display.

**Item Requirements in Progression:** Middle situations require scene-created items to proceed through HasItem requirements. Access situation specifies requirement for created access item. Runtime requirement evaluation resolves item reference through scene tracking, checks player inventory for concrete item. Situation remains locked until requirement met.

**Item Removal at Cleanup:** Final situations and cleanup phase remove scene-created items through explicit cleanup specifications. Departure choice rewards include ItemsToRemove referencing created items. Scene.ExecuteCleanup iterates CreatedItemIds tracking list, removes any still present in player inventory. Prevents item pollution from temporary access tokens persisting inappropriately.

**Permanent Rewards Distinction:** Some scene-created items persist as permanent rewards. Investigation scenes create knowledge_fragment items with PlayerOwned lifecycle. Cleanup does not remove these items - they remain in inventory permanently. Lifecycle strategy determines cleanup behavior.

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

---

## Critical Constraints

### Scene Archetype Complexity

Scene archetypes are substantially more complex than situation archetypes. Situation archetype generates 2-4 ChoiceTemplates (single decision point). Scene archetype generates: 3-7 SituationTemplates each with 2-4 ChoiceTemplates, SpawnRules governing flow, dependent location specifications with placement rules, dependent item specifications with lifecycle strategies, complete item lifecycle patterns, and cleanup orchestration (complete multi-situation arc).

Expect scene archetype generation methods to be 100-300 lines implementing complete pattern logic. This is significant engineering effort. Start with simplest archetypes (linear service scenes), validate thoroughly, then tackle complex patterns (branching investigations, recursive crises).

Dependent resource specifications add complexity. Archetype must determine: how many locations to create, where to place them relative to base, what properties they need, what lifecycle strategy to use, how many items to create, what naming patterns to use, when to grant/require/remove items. These specifications must integrate coherently with situation structure and progression flow.

Scene archetypes are reusable mechanical art. Once "service with location access" archetype exists with complete dependent resource specifications, applies to lodging/bathing/healing/storage/training contextually. Effort justified by infinite reuse with zero pre-authored world dependencies.

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
    "completedScenes": ["tutorial_lodging"]
  }
}
```

Categorical requirements only. Placement evaluator finds matching entities, passes objects to archetype generation. Generation produces identical package structure.

**Template Package Contents:**

Templates contain embedded SituationTemplates with ChoiceTemplates, SpawnRules defining situation flow, dependent location specifications (relative placement, pattern naming, property requirements, lifecycle strategies), dependent item specifications (pattern naming, uniqueness rules, lifecycle strategies), cleanup specifications referencing created resources, narrative hints for AI generation.

**Both Paths Identical After Entity Resolution:** Tutorial parser and procedural evaluator both produce: NPC object, base Location object. Both invoke identical archetype generation method. Both receive identical SceneTemplate output structure including complete self-contained package specifications.

**Templates Contain Zero Narrative Text:** Pure structure, mechanics, and resource specifications. AI generates all narrative from entity properties at situation activation. Template specifies archetype pattern, entity requirements, and complete dependency graph. Generation produces mechanical structure and resource specifications. AI produces contextual narrative.

### Scene Entity

Runtime scene instance tracking: CurrentSituationId (progression through arc), List<string> SituationIds (references to GameWorld.Situations), CreatedLocationIds (locations scene created at spawn), CreatedItemIds (items scene created at spawn), PlacedNPC/PlacedLocation (strongly-typed entity references AND IDs for external dependencies), State (Provisional/Active/Completed), Template (reference to SceneTemplate), DisplayName (optional authored label for UI).

Owns state machine methods: **CreateDependentResources** (reads template specifications, creates locations on hex grid adjacent to base, creates items with unique identifiers, populates tracking lists), **AdvanceToNextSituation** (progression control with context-aware auto-advance - returns flag indicating whether next situation shares context for seamless cascade or requires world navigation), **ShouldActivateAtContext** (checks if current situation should activate at given location/NPC context, supporting scene persistence across navigation), **ExecuteCleanup** (applies lifecycle strategies to created locations, removes created items from player inventory, restoration logic), **IsComplete** (completion detection). Domain entity owns lifecycle including dependent resource management, not scattered across services.

Context tracking: Scene knows which location/NPC each situation requires, comparing both external base location and scene-created locations. Compares consecutive situations to determine auto-advance behavior. Same context = immediate transition, player locked in flow. Different context = exit to world, scene persists with updated CurrentSituationId, reactivates when player reaches matching context.

Resource ownership: Scene maintains authoritative lists of created resources. Situations reference created resources through tracking lists, not brittle ID strings. Cleanup targets only scene-created resources, leaving pre-existing world entities unmodified. Complete lifecycle control from creation through cleanup.

### Situation Entity

Runtime situation instance within scene: Template reference (for lazy action instantiation), ParentScene reference (for context queries), InstantiationState (Deferred/Instantiated), LifecycleStatus (Locked/Selectable/InProgress/Completed/Failed), GeneratedDescription (AI-produced narrative text stored directly), per-choice ActionTexts (AI-generated strings for each choice).

Situations know position in arc via template properties. Opening situations introduce, conclusion situations wrap up. Position affects narrative generation and mechanical behavior.

### SituationTemplate

Embedded in SceneTemplate, not separate top-level entity. Defines single situation: List<ChoiceTemplate> (player options at this beat, each with optional Requirements determining availability), TimeCost (how much time this beat consumes), ItemGrants/ItemRequirements (lifecycle participation), NarrativeHints (for AI generation specific to this beat), PositionIndicator (opening/intermediate/conclusion).

ChoiceTemplates can specify requirements (HasItem, MinCoins, MinStat, CompletedScene) that lock choices until met. Locked choices remain visible with grayed presentation, teaching players about paths not taken and strategic value of earlier decisions.

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

Factory creating scene instances from templates with complete dependent resource creation.

Resolves placement through categorical filter evaluation: queries GameWorld for NPC matching personality filter, queries locations where NPC present matching service filter, selects concrete (NPC, Location) pair using selection strategy (highest bond, least recent, weighted random). External dependencies resolved.

Instantiates scene from template: creates Scene entity with reference to template, binds NPC and Location entity references, marks as Provisional state. Calls Scene.CreateDependentResources passing GameWorld and hex grid. Scene reads dependent location specifications, finds valid hex adjacent to base location, creates location with contextual name, adds to hex grid. Scene reads dependent item specifications, creates items with unique identifiers, adds to GameWorld.Items. Scene populates CreatedLocationIds and CreatedItemIds tracking lists.

Instantiates all situations in scene simultaneously as Dormant entities in GameWorld.Situations flat list. Each Situation gets reference to SituationTemplate, reference to parent Scene, placement context from template. Scene.SituationIds list populated with created situation IDs. Marks first situation as current via CurrentSituationId. No actions instantiated yet, just situation structure in dormant state.

At finalization when scene transitions Provisional to Active, situations remain dormant until player reaches matching context. Scene already created dependent resources at instantiation. Resources exist throughout scene lifecycle.

Complete self-contained package: scene receives minimal external bindings (NPC + base location), creates everything else it needs (dependent locations, items, situations), manages complete lifecycle (progression, cleanup, resource removal).

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