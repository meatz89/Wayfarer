# WAYFARER SYSTEM ARCHITECTURE

**CRITICAL: This document MUST be read and understood before making ANY changes to the Wayfarer codebase.**

## Related Documentation

- **[DESIGN_PHILOSOPHY.md](DESIGN_PHILOSOPHY.md)** - Design principles and conflict resolution
- **[ARCHITECTURAL_PATTERNS.md](ARCHITECTURAL_PATTERNS.md)** - Reusable architectural patterns (HIGHLANDER, Catalogue, Three-Tier Timing)
- **[CODING_STANDARDS.md](CODING_STANDARDS.md)** - Coding conventions and standards
- **[GLOSSARY.md](GLOSSARY.md)** - Canonical term definitions
- **[IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)** - Feature implementation status

**Historical Note:** INTENDED_ARCHITECTURE.md was archived to HISTORICAL/ (2025-01) due to 70% content overlap with this document (HIGHLANDER violation). This document is the authoritative source for system architecture.

## TABLE OF CONTENTS

1. [System Overview](#system-overview)
2. [Content Pipeline Architecture](#content-pipeline-architecture)
3. [Action Execution Pipeline](#action-execution-pipeline)
4. [GameWorld State Management](#gameworld-state-management)
5. [Service & Subsystem Layer](#service--subsystem-layer)
6. [UI Component Architecture](#ui-component-architecture)
7. [Data Flow Patterns](#data-flow-patterns)
8. [Critical Architectural Principles](#critical-architectural-principles)
9. [Component Dependencies](#component-dependencies)

---

## FUNDAMENTAL GAME SYSTEM ARCHITECTURE

**⚠️ READ THIS FIRST - THIS IS THE MOST IMPORTANT SECTION ⚠️**

## TWO-LAYER ARCHITECTURE: STRATEGIC vs TACTICAL

Wayfarer has TWO DISTINCT LAYERS that must NEVER be confused:

1. **STRATEGIC LAYER** - Scene → Situation → Choice (narrative, perfect information, WHAT to attempt)
2. **TACTICAL LAYER** - Mental/Physical/Social Challenges (card gameplay, hidden complexity, HOW to execute)

These layers are connected by the **BRIDGE** (ChoiceTemplate.ActionType) but are fundamentally different systems.

### LAYER 1: STRATEGIC (Perfect Information)

**Purpose**: Narrative progression and player decision-making with complete transparency.

**Progression Flow:**
Obligations spawn Scenes which contain Situations. Each Situation presents 2-4 player Choices with visible costs, rewards, and requirements. Player selects one Choice which has three possible execution outcomes: instant effects applied immediately, navigation to new context, or bridging to tactical layer via StartChallenge action type.

**Key Strategic Layer Entities:**
- **Scene**: Persistent container in GameWorld.Scenes, owns embedded Situations collection, tracks current active Situation
- **Situation**: Narrative moment embedded in Scene, contains ChoiceTemplates defining player options
- **ChoiceTemplate**: Player option with ActionType determining execution path, visible costs, requirements, and rewards
- **NO victory thresholds** - strategic layer uses state machine progression, not resource accumulation

### LAYER 2: TACTICAL (Hidden Complexity)

**Purpose**: Card-based gameplay execution with emergent tactical depth.

**Progression Flow:**
Choices with StartChallenge action type spawn temporary Challenge Sessions. Three parallel challenge types exist: Mental challenges use Progress/Attention/Exposure resources, Physical challenges use Breakthrough/Exertion/Danger resources, Social challenges use Momentum/Initiative/Doubt resources. Challenge sessions extract SituationCards from parent Situation defining victory thresholds. Player builds resources through tactical card play until reaching threshold, triggering SituationCard rewards, then returning to strategic layer.

**Key Tactical Layer Entities:**
- **Challenge Session**: Temporary gameplay instances specific to each challenge type
- **SituationCard**: Victory condition with threshold plus rewards, stored in parent Situation
- **Tactical Cards**: Playable cards specific to each challenge type drawn from decks
- **YES victory thresholds** - tactical layer requires resource accumulation to win

### THE BRIDGE: ChoiceTemplate.ActionType

**How Layers Connect:**

ChoiceTemplate sits at the boundary between layers. Its ActionType property determines execution path with three possible values: Instant applies rewards immediately within strategic layer, Navigate moves player to new context within strategic layer, StartChallenge crosses bridge to spawn tactical challenge session.

**If ActionType = StartChallenge**, additional properties specify the challenge:
- ChallengeType determines which tactical system to use (Social/Mental/Physical)
- ChallengeId specifies which deck to use for tactical cards
- OnSuccessReward and OnFailureReward define conditional outcomes applied after challenge completion

**Bridge Flow:**
Player selects Choice with StartChallenge action type. GameFacade reads challenge specification and delegates to appropriate tactical facade. Facade creates temporary session and extracts SituationCards from parent Situation defining victory conditions. Player plays tactical cards building resources until threshold reached or failure occurs. On completion, apply conditional rewards and return to strategic layer with outcome.

### CRITICAL: SituationCards Are Tactical, Not Strategic

**WRONG THINKING:**
> "Situation contains SituationCards, so SituationCards are strategic choices."

**CORRECT THINKING:**
> "Situation.SituationCards stores tactical victory conditions. Challenges READ these when spawned. They are NOT strategic choices - they're tactical win conditions."

**Storage vs Usage:**
- **Storage**: SituationCards stored in `Situation.SituationCards` (List<SituationCard>)
- **Usage**: When challenge spawns, it extracts these cards and uses them to determine victory
- **Purpose**: Define "reach 8 Momentum = basic reward" vs "reach 15 Momentum = optimal reward"

**ChoiceTemplates vs SituationCards:**
- **ChoiceTemplate**: Strategic choice presented to player ("Persuade", "Intimidate", "Bribe")
- **SituationCard**: Tactical victory tier ("8 Momentum", "12 Momentum", "15 Momentum")
- Player selects ChoiceTemplate before entering tactical layer
- Player achieves SituationCard thresholds during tactical layer

### ENTITY OWNERSHIP HIERARCHY

**GameWorld** is the single source of truth containing all game entities. It directly owns Scenes collection. Each Scene owns its embedded Situations collection via direct object containment, NOT ID references. Scenes track their CurrentSituation via direct object reference and manage SpawnRules defining situation flow. Scenes reference their placement context via PlacementType and PlacementId properties.

**Situations** are EMBEDDED IN SCENES, not a separate GameWorld collection. Each Situation is owned by its parent Scene. Situations reference their Template containing ChoiceTemplates. Situations have SystemType property (Social/Mental/Physical) for bridge routing. Situations store SituationCards list defining tactical victory conditions.

**SituationCards** are EMBEDDED IN SITUATIONS as tactical victory conditions. They are stored in Situation's SituationCards list property. Challenges extract and READ these cards when spawned. SituationCards define threshold values (momentum/progress/breakthrough) and grant rewards on achievement.

**Locations and NPCs** exist in GameWorld's collections as PLACEMENT CONTEXT ONLY. Scenes appear at Locations and NPCs but are NOT owned by them. This is placement not ownership - Location lifecycle is independent from Scene lifecycle.

### LAYER SEPARATION EXAMPLES

**Strategic Layer Flow (No Challenge):**
Player at location sees active Situation presenting Choice with Instant action type. Player selects Choice. System applies costs (deducts resources), applies rewards (grants items/access), advances Scene to next Situation. Flow remains entirely within strategic layer.

**Bridge to Tactical Layer:**
Player at location sees Situation presenting Choice with StartChallenge action type. Player selects Choice. System spawns challenge session of specified type (Social/Mental/Physical). Challenge session extracts SituationCards from parent Situation defining victory thresholds. Player enters tactical card-based gameplay.

**Tactical Layer Flow:**
Challenge session active with initial resource state (Initiative, Momentum, Doubt for Social challenges). Player plays tactical cards that modify resources through card effects. Resources accumulate until reaching SituationCard threshold. Threshold reached triggers SituationCard rewards application. Challenge ends, return to strategic layer. Scene advances to next Situation.

### DATA FLOW EXAMPLE

**Complete Flow: Securing Lodging**

SceneInstantiator spawns Scene containing multiple Situations. Scene placed at Location via PlacementFilter resolution. First Situation becomes active. Player sees multiple ChoiceTemplates with different action types (Instant payment, Social negotiation challenge, Physical theft challenge). Player selects negotiation choice with StartChallenge action type. SocialFacade creates challenge session extracting SituationCards for victory conditions. Player plays tactical Social cards building Momentum resource. Momentum reaches threshold defined in SituationCard. System applies SituationCard rewards. Challenge ends returning to strategic layer. Scene progresses to next Situation. Player navigates to new location. Next Situation activates at new context.

### FORBIDDEN PATTERNS

**Layer Confusion:** Showing SituationCards in strategic progression flow (Obligation → Scene → Situation → SituationCard). SituationCards are tactical victory conditions, not strategic progression elements.

**Wrong Collection Assignment:** Treating SituationCards as strategic choices by assigning them to Situation.ChoiceTemplates. These are completely different concepts belonging to different layers.

**Wrong Ownership:** GameWorld owning separate Situations collection. Situations are owned by their parent Scene through embedded collection, not stored separately in GameWorld.

**Layer Misclassification:** Describing Situations as part of tactical layer. Situations are strategic layer entities. Tactical layer consists of Challenge sessions only.

**Wrong Entity Type:** Treating SituationCard as separate reusable Card entity inheriting from base Card class. SituationCards are inline victory condition definitions, not playable cards.

**Legacy Entities:** Referencing Obstacle, Goal, or GoalCard entities. These were deleted from codebase and replaced by Scene/Situation architecture.

### CORRECT PATTERNS

**Scene Ownership:** Scene owns Situations directly through embedded collection property. Scene tracks CurrentSituation via direct object reference. Scene manages SpawnRules defining situation flow. Scene references placement context via PlacementType and PlacementId properties.

**Situation Structure:** Situation stores SystemType property for bridge routing metadata. Situation references Template containing ChoiceTemplates. Situation stores SituationCards list defining tactical victory conditions.

**ChoiceTemplate Bridge:** ChoiceTemplate bridges layers via ActionType property. ChallengeType property specifies which tactical system if StartChallenge. ChallengeId specifies which deck to use. OnSuccessReward and OnFailureReward define conditional outcomes applied after challenge completion.

**SituationCard Purpose:** SituationCard defines tactical victory condition with threshold property universal across all challenge types. Rewards property defines what player receives on achievement. IsAchieved property tracks runtime completion state.

**GameWorld Collections:** GameWorld owns Scenes collection only for strategic layer. NO separate Situations collection exists - Scenes own them. NO Challenges collection exists - challenges are temporary sessions created and destroyed per engagement.

### KEY ARCHITECTURAL RULES

**Strategic Layer Rules:**
1. Scene → Situation → Choice is the COMPLETE strategic flow
2. Situations do NOT progress to SituationCards (different layer)
3. Perfect information: All costs, requirements, rewards visible before selection
4. State machine: Scene.AdvanceToNextSituation() manages progression
5. Persistent: Scenes exist until completed/expired

**Tactical Layer Rules:**
1. Challenge sessions are TEMPORARY (created/destroyed per engagement)
2. SituationCards define victory conditions with thresholds
3. Three parallel systems: Social/Mental/Physical with equivalent depth
4. Hidden complexity: Card draw, exact challenge flow not visible before entry
5. Return to strategic layer on completion with success/failure outcome

**Bridge Rules:**
1. ChoiceTemplate.ActionType determines if bridge crosses
2. Only StartChallenge crosses to tactical layer
3. Instant and Navigate stay in strategic layer
4. One-way: Strategic spawns tactical, tactical returns outcome to strategic
5. SituationCards extracted when challenge spawns, NOT before

---

## PROCEDURAL SCENE GENERATION

### Four-Phase Scene Lifecycle

**Phase 1: Minimal JSON Authoring**

Authors write tiny SceneTemplate JSON: identifier, archetype reference, tier, placement filter. Crucially, authors do NOT specify individual situations, choices, costs, or rewards. This defers complexity to catalogues.

**Why Minimal Authoring:**
If authors specified full scene structure (every situation, every choice, every cost formula), content volume explodes. Ten service scenes would require ten copies of negotiation mechanics. Bug in negotiation pattern requires fixing ten files. With minimal authoring, authors specify WHAT (secure lodging at friendly inn) not HOW (4-choice negotiation with stat/money/challenge/fallback). Catalogues encode HOW once, authors instantiate infinite variations.

**Phase 2: Parse-Time Catalogue Expansion**

Parser triggers catalogue generation which transforms minimal JSON into complete SceneTemplate with full SituationTemplates. Happens ONCE at game load, not per spawn. Templates stored in GameWorld.SceneTemplates as reusable blueprints.

**Why Parse-Time Not Runtime:**
Catalogue generation expensive (context building, formula evaluation, reward routing). Doing this per scene spawn would cause lag. Parse-time generation means one-time cost during load screen, zero cost during gameplay. Reusable templates spawned instantly.

**Phase 3: Spawn-Time Template Instantiation**

Spawning converts immutable template into mutable Scene instance. Resolves markers (logical resource references become concrete IDs), generates AI narrative if needed, replaces entity placeholders. Actions NOT created yet.

**Why Deferred Action Creation:**
Creating actions during spawn wastes work if scene never displayed. Spawn happens during route travel (scene stored for later), display happens when scene becomes active. Actions only needed at display time. Deferral means spawn lightweight, display heavier but only when required.

**Phase 4: Query-Time Action Creation**

When UI queries active scene, lazy instantiation creates context-appropriate actions from ChoiceTemplates. LocationActions for location-based scenes, NPCActions for conversation scenes, PathCards for challenge scenes. Adds actions to appropriate GameWorld collections for UI rendering.

**Why Context-Appropriate Actions:**
Same ChoiceTemplate spawns different action types based on placement. Template at Location becomes LocationAction, same template at NPC becomes NPCAction. This placement-agnostic design enables template reuse across contexts without special-casing action generation.

### Two-Tier Archetype Composition System

**The Pattern: Mechanics Separate From Rewards**

Tier 1 (SituationArchetypeCatalog) generates mechanical choice structure without scene-specific rewards. Tier 2 (SceneArchetypeCatalog) composes multiple Tier 1 situations and enriches with context-specific rewards.

**Why This Separation:**

Negotiation mechanics identical across all service contexts: 4 choices (stat-gated instant, money-gated instant, challenge path, guaranteed fallback), same cost formulas, same PathType routing. Only REWARDS differ - lodging unlocks private room, bathing unlocks bathhouse access, healing unlocks clinic visit.

If each scene archetype implemented its own negotiation mechanics, bugs would appear inconsistently. Balancing negotiation difficulty requires updating 20+ scene files. Player confusion when negotiation pattern varies by context.

With two-tier separation, situation catalogue encodes negotiation mechanics ONCE. Scene catalogues inherit mechanics, customize only rewards via PathType routing. Bug fix propagates automatically. Balance adjustment affects all contexts uniformly. Player learns pattern once, applies everywhere.

**The Enrichment Flow:**

Tier 1 returns ChoiceTemplates with PathType properties but empty RewardTemplate. Tier 2 routes on PathType: InstantSuccess path gets immediate rewards (LocationsToUnlock, ItemIds), Challenge path gets conditional rewards (OnSuccessReward), Fallback path gets minimal/zero rewards. Same base choices, different reward assignments per scene context.

**The Trade-Off:**

Two-tier pattern adds indirection - must understand both catalogues to grasp full scene generation. Simpler to inline everything in single catalogue. But indirection buys massive reusability - one negotiation archetype supports infinite service types. Architectural complexity chosen over content duplication.

### Context-Aware Scaling Mechanism

**The Principle: Categorical Properties Drive Dynamic Difficulty**

Archetypes define baseline numeric values (StatThreshold: 5, CoinCost: 5). Catalogues scale these values using multipliers derived from entity categorical properties (NPCDemeanor, Quality, PowerDynamic). Same archetype + different entity context = automatically balanced difficulty.

**Why Categorical Scaling:**

Without scaling, every scene requires manual numeric tuning. "Friendly innkeeper negotiation needs threshold 3, hostile innkeeper needs threshold 8" - multiply across 50 NPCs and hundreds of scenes. Balancing nightmare, maintenance disaster.

With categorical properties, authors/AI specify entity nature descriptively (Friendly, Premium quality). Catalogues translate categories to multipliers (Friendly = 0.6x difficulty, Premium = 1.6x cost). Archetype baseline scales automatically. Add new friendly NPC - scenes auto-balance easier. Mark location luxury - scenes auto-scale more expensive. Zero manual tuning.

**The Formula Pattern:**

Base archetype: StatThreshold 5. NPCDemeanor multipliers: Friendly 0.6x, Neutral 1.0x, Hostile 1.4x. Final threshold: 5 × multiplier. Friendly innkeeper: 5 × 0.6 = 3. Hostile guard: 5 × 1.4 = 7. Same negotiation archetype, contextually appropriate difficulty.

**Why This Enables AI Generation:**

AI can't know global game balance (player level, progression state, economic calibration). But AI CAN describe entities categorically (this innkeeper feels friendly, this inn seems premium quality). System translates categories to balanced numbers using formula context. AI generates infinite content without breaking game balance.

### Marker Resolution for Self-Contained Scenes

**The Problem: Templates Can't Reference What Doesn't Exist**

Lodging scene template needs to reference private room location. But room doesn't exist until scene spawns. Can't hardcode room ID (template is reusable, each spawn needs unique room). Can't use null (situations require location references for context).

**The Solution: Logical Markers Resolved At Spawn**

Templates reference logical markers ("generated:private_room"). Parser creates DependentLocationSpec declaring marker. At spawn time, system generates actual Location with real GUID, builds MarkerResolutionMap (marker → GUID), resolves all marker references throughout scene's situations and choices. Runtime uses only concrete GUIDs, no marker syntax remains.

**Why This Enables Reusability:**

Single template spawns infinite times, each spawn generates independent resource set. First spawn creates room A with GUID-1, second spawn creates room B with GUID-2. No resource sharing between instances, no collision, no cross-contamination.

Without markers, would need either: (1) Pre-generate all possible rooms and hardcode IDs (doesn't scale), or (2) Use global shared room (first scene locks room, second scene can't spawn). Markers enable true instance isolation.

### AI Narrative Generation Integration

**The Principle: Mechanical Structure + Narrative Hints = AI-Generated Content**

Archetypes define mechanics (costs, choices, rewards) and narrative constraints (tone, theme, context). AI generates actual narrative text at spawn time using entity properties and hint constraints. Mechanical structure reusable, narrative unique per instance.

**Why Narrative Hints Not Full Text:**

If archetypes contained full narrative text, reusability breaks. "You negotiate with the innkeeper for lodging" doesn't work when NPC is bathhouse attendant or healer. Would need separate text for every entity combination. Content explosion.

With narrative hints, archetype specifies WHAT KIND of narrative (transactional negotiation, urgent tone) not exact text. AI generates text fitting entity context - innkeeper negotiation reads differently than guard negotiation, but both use same mechanical archetype. One archetype, infinite narrative variations.

**Why AI Generation At Spawn Not Parse:**

Parse-time doesn't know which specific entity instance will spawn. Template knows "spawn at Friendly NPC" but doesn't know if that's Elena or Marcus until spawn evaluates game state. Can't generate narrative for unknown entity.

Spawn-time has concrete entity with full properties (personality, relationship history, location atmosphere). AI generates narrative incorporating these details naturally. Marcus negotiation references his gruff demeanor, Elena negotiation references established rapport.

---

## RUNTIME EXECUTION FLOW

### Context Activation & Auto-Activation

Situations activate when player location/NPC context matches situation requirements.

**Activation Check Process:**
Player enters location or interacts with NPC triggering SceneFacade query. For each active Scene SceneFacade checks if Scene should activate at current context. Scene compares CurrentSituation's required context (RequiredLocationId, RequiredNpcId) against player's current context. If match found, situation activates.

**Activation Requirements:**
**Location + NPC:** Both properties must match for activation. Used for service negotiation at specific NPC in specific location, NPC conversations requiring privacy.
**Location Only:** Location must match, NPC null or ignored. Used for private room rest (solo), location-based investigations without NPC involvement.
**NPC Only:** NPC must match, location optional. Used for traveling merchants appearing at multiple locations, roaming characters.

**Auto-Activation Flow:**
Scene completes Situation causing AdvanceToNextSituation call returning ContinueInScene decision. Scene updates CurrentSituation pointer to next situation. Player enters next situation's required location. SceneFacade detects context match automatically. Next situation auto-activates without explicit player action. Choices appear immediately in UI.

**Purpose:** Seamless multi-situation progression without artificial navigation friction. Player completes negotiation acquiring room key, enters unlocked room, next situation's choices immediately available. No "click here to continue" artificial steps.

### Action Instantiation (Query-Time Creation)

Actions created on-demand when situation activates reducing memory footprint.

**InstantiationState Transition:**
Initial state at situation creation: Situation.InstantiationState equals Deferred. NO actions exist in GameWorld collections yet. Query-time instantiation occurs when SceneFacade queries actions for location. If InstantiationState equals Deferred, facade iterates situation's ChoiceTemplates creating action instances, each action references its ChoiceTemplate plus parent SituationId, adds actions to appropriate GameWorld collection, sets InstantiationState to Instantiated.

**Why Query-Time:**
**Memory Efficiency:** Actions only exist when contextually relevant to player's current state. Scene with 5 situations times 3 choices equals 15 potential actions but only 3 exist at any time.
**Lazy Evaluation:** Game may contain thousands of template choices across all scenes. Only instantiate subset player can currently access avoiding massive memory bloat.
**Clean Lifecycle:** Actions deleted when situation completes. If situation re-activates later, actions recreated fresh from template. No stale or orphaned action references.

**Action Properties:**
ChoiceTemplate reference storing requirements, costs, rewards from template. SituationId for parent lookup enabling cleanup. Ephemeral lifecycle - exists only while situation active, deleted on completion.

### Choice Execution Routing

ChoiceTemplate.PathType determines execution flow when player selects choice.

**Routing Logic:**
**InstantSuccess Path:** Evaluate requirements immediately. Apply costs immediately. Apply RewardTemplate immediately. Mark situation complete. Advance to next situation.

**Challenge Path:** Evaluate requirements immediately. Apply costs immediately. Store OnSuccessReward and OnFailureReward in PendingContext for later application. Navigate to tactical screen (Social/Mental/Physical system). Wait for challenge completion. Apply OnSuccessReward if victory OR OnFailureReward if defeat. Advance to next situation.

**Fallback Path:** NO requirements (always available as safety valve). NO costs (free forward progress). Minimal or zero rewards (poor outcome). Mark situation complete. Advance to next situation (may exit scene).

**ActionType Bridge Integration:**
**Navigate Action Type:** Apply rewards first modifying game state. Then move player to destination location/NPC. Scene may continue if next situation exists at new location enabling cascading progression.

**StartChallenge Action Type:** Cross strategic-tactical bridge. Store CompletionReward in PendingContext. Navigate to appropriate challenge screen (Social/Mental/Physical). Challenge system takes over until completion or abandonment.

**Instant Action Type:** Apply rewards modifying game state. Player remains in current scene context. Situation advances based on transition rules.

**Execution Order:**
Check requirements, lock choice if failed OR continue if passed. Apply costs deducting resources from player state. Route by PathType and ActionType determining next step. Apply rewards immediately for InstantSuccess OR conditionally for Challenge. Update situation state marking complete. Evaluate transitions determining next situation or scene exit.

### Scene Progression & Transition Evaluation

Scene.AdvanceToNextSituation() evaluates transition rules to determine next situation.

**Transition Evaluation:**
Get completed situation from Scene state. Lookup transition rules from SituationSpawnRules.Transitions collection. Evaluate Transition.Condition types: Always means unconditional progression, OnChoice checks if LastChoiceId matches SpecificChoiceId, OnSuccess checks LastChallengeSucceeded equals true, OnFailure checks LastChallengeSucceeded equals false. If matching transition found, set Scene.CurrentSituation to destination situation, return SceneRoutingDecision (either ContinueInScene or ExitToWorld). If no matching transition found, scene complete with no more situations, return SceneRoutingDecision.SceneComplete.

**SceneRoutingDecision Values:**
**ContinueInScene:** Next situation shares same location/NPC context enabling auto-activation immediately without player navigation.
**ExitToWorld:** Next situation requires different context forcing player to manually navigate to new location or find different NPC.
**SceneComplete:** No more situations exist in scene, scene ends releasing resources.

**Context Comparison:**
Scene.CompareContexts determines routing decision. Compare current context (CurrentLocationId, CurrentNpcId) against next context (Situation.RequiredLocationId, Situation.RequiredNpcId). If contexts match return ContinueInScene enabling seamless cascade. If contexts differ return ExitToWorld requiring player navigation.

**Progression State Machine:**
Situation active, player selects choice, choice executes, AdvanceToNextSituation called, system evaluates transitions, updates CurrentSituation pointer, compares contexts. If ContinueInScene next situation auto-activates. If ExitToWorld system awaits player navigation. If SceneComplete scene ends.

### Dependent Resource Lifecycle

Complete flow from declaration to removal for scene-generated resources spanning six phases across three timing tiers.

**Phase 1: Declaration (Parse Time)**
SceneArchetypeCatalog declares dependent resources via DependentLocationSpec and DependentItemSpec. Specifications contain: logical identifier (example: "private_room"), name template with placeholders (example: "{npcName}'s Private Room"), properties defining resource characteristics (example: Safe, Restful location properties), item type classification (example: Key type).

**Phase 2: Creation (Parse Time)**
SceneTemplateParser creates actual entities from specifications. For each DependentLocationSpec parser generates Location entity with real GUID, name containing unresolved placeholders, IsLocked starting true, properties copied from spec. Adds location to GameWorld.Locations collection. Stores mapping from logical identifier to actual GUID for later marker resolution.

**Phase 3: Marker Resolution (Spawn Time)**
Scene spawns building MarkerResolutionMap mapping logical identifiers to actual GUIDs (example: "generated:private_room" maps to "location_guid_12345"). Situations resolve markers via map lookup. SituationTemplate.RequiredLocationId containing "generated:private_room" becomes Situation.ResolvedRequiredLocationId containing actual GUID. ChoiceReward.LocationsToUnlock containing marker gets resolved at choice execution to actual location GUID.

**Phase 4: Grant (Runtime - Choice Execution)**
Choice rewards grant access to dependent resources. RewardTemplate.LocationsToUnlock sets Location.IsLocked to false enabling player entry. RewardTemplate.ItemIds adds items to Player.Inventory enabling possession and usage.

**Phase 5: Usage (Runtime - Situation Activation)**
Situation requires access checking Situation.RequiredLocationId against player's accessible locations. Player can only activate situation if Location.IsLocked equals false indicating access granted. Situation auto-activates when player enters accessible required location.

**Phase 6: Removal (Runtime - Cleanup)**
Departure choice cleans up dependent resources. RewardTemplate.ItemsToRemove removes items from Player.Inventory. RewardTemplate.LocationsToLock sets Location.IsLocked back to true preventing re-entry.

**Lifecycle Summary:**
Declare resources at parse time, create entities at parse time, resolve markers at spawn time, grant access via choice execution, use resources during situation activation, remove access via cleanup choice. Resources exist throughout but access controlled. Player cannot re-enter locked room after departure. Key removed from inventory. Location persists for potential future scenes but becomes inaccessible.

---

## THREE-TIER TIMING MODEL

### Why Three Tiers Exist

The three-tier timing model enables lazy instantiation drastically reducing memory usage and preventing GameWorld from bloating with thousands of inaccessible actions.

### Tier 1: Templates (Parse Time)

**When:** Game startup during JSON parsing.
**What:** Immutable archetypes defining reusable patterns.

**Structure:** SceneTemplate contains embedded List of SituationTemplates, each SituationTemplate contains embedded List of ChoiceTemplates.

**Properties:**
Created once from JSON at parse time. Stored in GameWorld.SceneTemplates collection. Never modified during gameplay. Serves as design language for content authors enabling reusable pattern definitions.

### Tier 2: Scenes/Situations (Spawn Time)

**When:** Scene spawns from Obligation or SceneSpawnReward trigger.
**What:** Runtime instances with lifecycle and mutable state.

**Structure:** Scene spawned from SceneTemplate reference. Scene contains embedded List of Situation instances created during spawn. Each Situation stores Template reference pointing back to immutable SituationTemplate. ChoiceTemplates NOT instantiated as actions yet.

**Properties:**
Scene created with embedded Situations collection. Situation.Template reference stored enabling access to ChoiceTemplates. Situation.InstantiationState set to Deferred. NO actions created in GameWorld collections yet avoiding premature memory allocation.

**Why Deferred:**
Situations may require specific context (Location X plus NPC Y) player hasn't reached yet. Creating actions prematurely bloats GameWorld with thousands of UI elements player cannot see or access. Example: Scene with 5 Situations each having 3 ChoiceTemplates equals 15 potential actions. If all instantiated immediately GameWorld contains 15 action buttons though only 3 contextually accessible. Deferred instantiation creates zero actions at spawn deferring until player enters matching context.

### Tier 3: Actions (Query Time)

**When:** Player enters matching context (Location plus optional NPC).
**What:** Ephemeral UI projections of ChoiceTemplates.

**Process:** Player at specific location with specific NPC present. SceneFacade queries which Situations match current context. Finds matching Situation with InstantiationState equals Deferred. Instantiates action instances from Situation.Template.ChoiceTemplates. Adds actions to appropriate GameWorld collection (NPCActions, LocationActions, PathCards). UI displays action cards/buttons to player.

**Ephemeral Lifecycle:**
**Creation:** Actions instantiated when player enters matching context. **Display:** Actions rendered in UI as clickable cards or buttons. **Execution:** Player selects action, GameFacade executes choice logic. **Deletion:** When Situation completes actions deleted from GameWorld collections. **Regeneration:** If player returns to same context later, actions recreated fresh from Template.

**Why Ephemeral:**
Template serves as single source of truth (HIGHLANDER principle). Actions are view projections generated on demand not persistent entities. Prevents duplicate actions accumulating in GameWorld. Eliminates orphaned actions from completed situations. Memory efficient - only contextually relevant actions exist.

### InstantiationState Tracking

**InstantiationState Enum:** Deferred value means Situation exists but NO actions in GameWorld. Instantiated value means Player entered context and actions materialized.

**State Transitions:**
Situation spawned with InstantiationState set to Deferred. Player enters matching context triggering SceneFacade to instantiate actions and set InstantiationState to Instantiated. Situation completes causing actions deletion and Situation marked complete or deleted.

### Complete Example Flow

**Parse Time (Tier 1):**
JSON defines SceneTemplate for securing lodging tutorial. Contains three SituationTemplates: obtain_key situation with 3 ChoiceTemplates (pay coins, negotiate, steal), access_room situation with 2 ChoiceTemplates (unlock door, break window), claim_space situation with 2 ChoiceTemplates (rest immediately, inspect first). Complete template stored in GameWorld.SceneTemplates collection.

**Spawn Time (Tier 2):**
Obligation spawns Scene from template creating Scene.Situations collection with 3 embedded Situation instances. Situation 1 requires inn_common_room location with InstantiationState Deferred. Situation 2 requires inn_upper_floor location with InstantiationState Deferred. Situation 3 requires generated private_room marker with InstantiationState Deferred. Scene.CurrentSituation set to Situation 1. GameWorld.Scenes incremented by 1 scene. GameWorld.LocationActions remains at 0 actions - nothing instantiated yet.

**Query Time (Tier 3):**
Player navigates to inn_common_room location. UI calls SceneFacade.GetActionsForLocation querying. SceneFacade finds Situation 1 matching location with InstantiationState Deferred. SceneFacade instantiates 3 LocationActions from Situation.Template.ChoiceTemplates: pay coins action, negotiate action with StartChallenge, steal action with StartChallenge. Situation 1.InstantiationState set to Instantiated. GameWorld.LocationActions incremented by 3 actions. UI renders 3 action cards. Player selects negotiate action. GameFacade executes spawning Social challenge. Challenge succeeds completing Situation 1. System deletes 3 LocationActions from GameWorld. Scene.AdvanceToNextSituation updates CurrentSituation to Situation 2. GameWorld.LocationActions decremented by 3 actions. Player navigates to inn_upper_floor. SceneFacade finds Situation 2 matching location with Deferred state. Instantiates 2 LocationActions from Situation 2 templates. GameWorld.LocationActions incremented by 2 actions. UI renders 2 action cards.

### Benefits

**Memory Efficiency:** Only actions for current context exist in GameWorld. Scene with 5 Situations times 3 Choices equals 15 potential actions but only 3 exist at any time.

**Performance:** UI queries GameWorld.LocationActions collection (3 items) instead of scanning all Situations across all Scenes (potentially hundreds).

**Single Source of Truth:** ChoiceTemplate defined once in SituationTemplate. Runtime actions reference Template never duplicating data.

**Clean Lifecycle:** Actions created when needed, deleted when done. No orphaned references. No stale data accumulation.

**Easy Debugging:** Check InstantiationState to determine if actions should exist. Deferred state means player hasn't entered required context yet.

### Forbidden Patterns

**Creating Actions at Spawn Time:** Immediately instantiating ALL actions for ALL Situations when Scene spawns causes GameWorld bloat with inaccessible actions resulting in memory waste and performance penalty.

**Storing Actions Permanently:** Adding Actions collection property to Situation entity violates lazy instantiation principle creating duplicate action collections.

**Instantiating Without Context Check:** Iterating all Situations instantiating actions without checking context match creates actions player cannot access.

### Correct Patterns

**Query Time Instantiation with Context Check:** SceneFacade.GetActionsForLocation iterates GameWorld.Scenes, for each Scene iterates Situations, checks if Situation.RequiredLocationId matches queried location, checks if InstantiationState equals Deferred, instantiates actions from Template.ChoiceTemplates, sets InstantiationState to Instantiated, returns actions.

**Cleanup After Execution:** GameFacade.ExecuteLocationAction executes action logic, checks if Situation completes, deletes all actions for that Situation from GameWorld collections, calls Scene.AdvanceToNextSituation updating state machine.

---

## SYSTEM OVERVIEW

Wayfarer is a **low-fantasy tactical RPG** with **three parallel challenge systems** (Social, Mental, Physical) built with **C# ASP.NET Core** and **Blazor Server**. The architecture follows **clean architecture principles** with strict **dependency inversion** and **single responsibility** patterns.

### Core Design Philosophy
- **GameWorld as Single Source of Truth**: All game state flows through GameWorld with zero external dependencies
- **Three Parallel Tactical Systems**: Social (conversations), Mental (investigations), Physical (obstacles) with equivalent depth
- **Strategic-Tactical Bridge**: Scenes spawn Situations with Choices, ChoiceActionType.StartChallenge routes to tactical systems
- **Static Content Loading**: JSON content parsed once at startup without DI dependencies
- **Facade Pattern**: Business logic coordinated through specialized facades
- **Authoritative UI Pattern**: GameScreen owns all UI state, children communicate upward
- **No Shared Mutable State**: Services provide operations, not state storage

---

## CONTENT PIPELINE ARCHITECTURE

### 1. JSON Content Structure

### 2. Static Parser Layer

**Location**: `src/Content/*Parser.cs`

**Parser Responsibilities:**
Each parser converts DTO to domain entity: SocialCardParser converts SocialCardDTO to SocialCard, MentalCardParser converts MentalCardDTO to MentalCard, PhysicalCardParser converts PhysicalCardDTO to PhysicalCard, NPCParser converts NPCDTO to NPC, VenueParser converts VenueDTO to Venue, LocationParser converts LocationDTO to Location, SceneParser converts SceneDTO to Scene, SituationParser converts SituationDTO to Situation.

**CRITICAL PARSER PRINCIPLES:**
**PARSE AT THE BOUNDARY:** JSON artifacts NEVER pollute domain layer. All conversion happens at parser boundary.
**NO JsonElement PASSTHROUGH:** Parsers MUST convert to strongly-typed objects. No passing through raw JSON elements.
**NO Dictionary Properties:** Use proper typed properties on domain models. No generic string-keyed dictionaries.
**JSON FIELD NAMES MUST MATCH C# PROPERTIES:** Direct mapping without JsonPropertyName attributes hiding mismatches.
**STATELESS:** Parsers are static classes with no side effects or state storage.
**SINGLE PASS:** Each parser converts DTO to domain entity in one operation without multiple passes.
**CATEGORICAL TO MECHANICAL TRANSLATION:** Parsers translate categorical JSON properties to absolute mechanical values through catalogues.

### 3. Categorical Properties → Dynamic Scaling Pattern (AI Content Generation)

**CRITICAL ARCHITECTURE DECISION: Why JSON uses categorical properties instead of absolute values**

**The Problem: AI-Generated Runtime Content**

AI-generated content (procedural generation, LLM-created entities, user-generated content) CANNOT specify absolute mechanical values because AI doesn't know current player progression level (Level 1 versus Level 10), existing game balance (what items/cards/challenges already exist), global difficulty curve (early game versus late game tuning), or economy state (coin inflation, resource scarcity).

**The Solution: Relative Categorical Properties Plus Dynamic Scaling Catalogues**

AI generates JSON with categorical properties providing relative descriptions. Parser reads current game state (player level, difficulty mode). Catalogue translates categorical properties to absolute values applying scaling based on game state. Domain entity receives scaled mechanical values appropriate to current progression.

**Example: Equipment Durability**

JSON authored by AI or humans specifies categorical durability property (example: "Fragile") which is relative category not absolute value. Parser translates using catalogue plus game state parsing enum value, reading player level and difficulty mode. Catalogue returns scaled values: Level 1 Fragile equals 2 uses and 10 coins, Level 5 Fragile equals 4 uses and 25 coins (scaled up for progression), Level 10 Fragile equals 6 uses and 40 coins (continues scaling). Critical principle: Fragile ALWAYS weaker than Sturdy maintaining relative consistency regardless of scaling factors.

**Example: Card Effects**

JSON specifies categorical conversational move type (example: "Remark"), bound stat type (example: "Rapport"), depth value. Parser translates with scaling calling catalogue method passing categorical properties plus player level as scaling factor. Early game (Level 1) Remark with Rapport at Depth 2 produces +4 Understanding. Late game (Level 5) same categorical properties produce +6 Understanding demonstrating automatic scaling.

**Why This Architecture Exists:**

**AI Content Generation:** AI describes entities relatively (Fragile rope, Cunning NPC) without needing absolute game values knowledge.
**Dynamic Difficulty Scaling:** Same content scales automatically as player progresses through game.
**Consistent Relative Balance:** Fragile ALWAYS weaker than Sturdy regardless of current scaling factors applied.
**Future-Proof:** Supports procedural generation, LLM content generation, user mods, runtime content creation.
**Centralized Balance:** Change ONE catalogue formula automatically affects ALL entities of that category consistently.

**Catalogue Implementation Requirements:**

Catalogues located in src/Content/Catalogues folder as static classes. Context-aware scaling functions take categorical enum value plus scaling context (player level, difficulty mode). Calculate base values for each category via switch expression. Apply dynamic scaling multipliers based on game state (example: 20 percent per player level, difficulty-based multipliers). Return scaled values as tuple or structured result. Throw exceptions for unknown categorical values ensuring fail-fast behavior.

**Existing Catalogues:**
SocialCardEffectCatalog, MentalCardEffectCatalog, PhysicalCardEffectCatalog, EquipmentDurabilityCatalog located in src/Content/Catalogs folder.

**When to Use Categorical Properties:**

Ask these questions for ANY numeric property in DTO: Could AI generate this entity at runtime without knowing global game state? Should this value scale with player progression or difficulty? Is this RELATIVE (compared to similar entities) rather than ABSOLUTE? If YES to any question: Create categorical enum plus scaling catalogue. If NO: Consider if truly design-time constant (rare - most values should scale).

**Anti-Pattern: Hardcoded Absolute Values in JSON**

WRONG: JSON with absolute numeric values (exhaustAfterUses 2, repairCost 10, understanding 4, momentum 2) breaks AI generation and prevents scaling.
CORRECT: JSON with categorical properties (durability "Fragile", conversationalMove "Remark", depth 2) enables AI generation and automatic scaling.

### 4. Content Loading Orchestration

**Location**: `src/Content/PackageLoader.cs` & `GameWorldInitializer.cs`

**Loading Sequence**:
```
Startup → GameWorldInitializer.CreateGameWorld()
       → PackageLoader.LoadContent()
       → Static Parsers (for each content type)
       → Domain Objects → GameWorld Population
```

**Initialization Architecture**:
- `GameWorldInitializer` is **STATIC** - no DI dependencies
- Creates GameWorld **BEFORE** service registration completes
- Prevents circular dependencies during ServerPrerendered mode
- Content loaded once at startup, never reloaded

---

## ACTION EXECUTION PIPELINE

**Location**: `src/Content/Parsers/*ActionParser.cs`, `src/GameState/*Action.cs`, `src/Services/GameFacade.cs`

### 1. Action System Overview

**⚠️ CRITICAL ARCHITECTURE CHANGE: Actions are NO LONGER defined in JSON**

**Actions are PROCEDURALLY GENERATED from categorical location properties at parse time.**

**Two Action Types**:
- **PlayerActions**: Universal actions available everywhere (e.g., "Check Belongings", "Wait", "Sleep Outside")
- **LocationActions**: Property-driven actions generated from LocationPropertyType enums

**Architecture Goals**:
1. **Catalogue Pattern**: Actions generated from categorical properties (parse-time ONLY)
2. **No JSON Bloat**: Locations define properties, catalogues generate complete actions
3. **Strong Typing**: All action types, costs, rewards strongly-typed
4. **No Dictionary Disease**: Zero string-based matching or dictionary lookups

### 2. Complete System Documentation

**⚠️ FULL DOCUMENTATION**: See [LOCATION_PROPERTY_ACTION_SYSTEM.md](./LOCATION_PROPERTY_ACTION_SYSTEM.md) for comprehensive documentation of:
- Location Property → Action Generation pattern
- Catalogue implementation details
- Parse-time integration
- Runtime querying and filtering
- Property → Action mapping table
- Adding new action types
- Testing and debugging

### 3. Quick Reference

**Location Property → Action Mapping**:

| Property | Generated Action | Costs | Rewards |
|----------|-----------------|-------|---------|
| Crossroads | Travel | None | None |
| Commercial | Work | Time + Stamina | 8 Coins |
| Restful | Rest | Time | +1 Health, +1 Stamina |
| Lodging | Secure Room | 10 Coins | Full Recovery |

**Universal Player Actions**:
- Check Belongings
- Wait
- Sleep Outside

### 5. GameFacade Orchestration (Single Dispatch Point)

**Location**: `src/Services/GameFacade.cs`

**GameFacade Methods** - Single point of entry for action execution:
```csharp
// Execute PlayerAction by enum type
public async Task ExecutePlayerAction(PlayerActionType actionType)
{
    switch (actionType)
    {
        case PlayerActionType.CheckBelongings:
            // Navigate to equipment screen
            break;

        case PlayerActionType.Wait:
            // Delegate to ResourceFacade for time/hunger progression
            await _resourceFacade.ExecuteWait();
            break;

        default:
            throw new InvalidOperationException($"Unknown PlayerActionType: {actionType}");
    }
}

// Execute LocationAction by enum type
public async Task ExecuteLocationAction(LocationActionType actionType, string locationId)
{
    switch (actionType)
    {
        case LocationActionType.Travel:
            // Navigate to travel screen
            break;

        case LocationActionType.Rest:
            // Delegate to ResourceFacade for recovery
            await _resourceFacade.ExecuteRest();
            break;

        case LocationActionType.Work:
            // Delegate to ResourceFacade for work rewards
            await _resourceFacade.PerformWork();
            break;

        case LocationActionType.Investigate:
            // Delegate to LocationFacade for familiarity gain
            await _locationFacade.InvestigateLocation(locationId);
            break;

        default:
            throw new InvalidOperationException($"Unknown LocationActionType: {actionType}");
    }
}
```

**Why GameFacade**: Follows existing patterns like `ExecuteListen()`, `PerformWork()`, `TravelToDestinationAsync()`. GameFacade orchestrates specialized facades while keeping them decoupled.

### 7. Specialized Facade Implementation

**Location**: `src/Subsystems/Resource/ResourceFacade.cs`

**ResourceFacade.ExecuteRest()** - Example action handler:
```csharp
public async Task ExecuteRest()
{
    Player player = _gameWorld.GetPlayer();

    // Advance 1 time segment
    await _timeFacade.AdvanceSegments(1);

    // Hunger increases by +5 per segment (automatic via time progression)
    // No need to manually modify hunger here

    // Resource recovery
    player.Health = Math.Min(player.Health + 1, player.MaxHealth);      // +1 health (16.7% of 6-point max)
    player.Stamina = Math.Min(player.Stamina + 1, player.MaxStamina);   // +1 stamina (16.7% of 6-point max)
}
```

**ResourceFacade.ExecuteWait()** - Example action handler:
```csharp
public async Task ExecuteWait()
{
    // Advance 1 time segment
    await _timeFacade.AdvanceSegments(1);

    // Hunger increases by +5 per segment (automatic via time progression)
    // No resource recovery - just passing time
}
```

### 8. UI Layer Integration

**Location**: `src/Pages/Components/LocationContent.razor.cs`

**BEFORE** (String Matching - Anti-Pattern):
```csharp
// WRONG - scattered string matching
if (action.ActionType == "travel")
{
    // Handle travel inline
}
else if (action.ActionType == "rest")
{
    // Handle rest inline
}
```

**AFTER** (Enum Dispatch through GameFacade):
```csharp
// CORRECT - enum-based dispatch through GameFacade
private async Task HandleLocationAction(LocationAction action)
{
    await GameFacade.ExecuteLocationAction(action.ActionType, currentLocationId);
    await RefreshUI();
}

private async Task HandlePlayerAction(PlayerAction action)
{
    await GameFacade.ExecutePlayerAction(action.ActionType);
    await RefreshUI();
}
```

**Why This Works**: UI layer is dumb display that calls GameFacade with strongly-typed enums. GameFacade handles all dispatch logic. No string matching in UI layer.

### 9. Complete Vertical Slice Example: "Rest" Action

**Complete Flow from JSON to Execution**:

```
1. JSON DEFINITION (01_foundation.json)
   {
     "id": "rest",
     "name": "Rest",
     "actionType": "Rest",  // String in JSON
     "requiredProperties": ["rest", "restful"]
   }

2. PARSER VALIDATION (LocationActionParser.cs)
   - Reads JSON via PackageLoader
   - Validates "Rest" against LocationActionType enum
   - Throws InvalidDataException if "Rest" not in enum
   - Converts string "Rest" to LocationActionType.Rest enum
   - Converts property strings to LocationPropertyType enums

3. GAMEWORLD STORAGE
   - Parsed LocationAction stored in GameWorld.LocationActions
   - ActionType is LocationActionType.Rest (strongly typed)

4. UI DISPLAY (LocationContent.razor.cs)
   - Fetches available LocationActions for current location
   - Filters by property matching (location has "rest" or "restful")
   - Displays "Rest" card in UI

5. USER INTERACTION
   - Player clicks "Rest" card
   - UI calls: await HandleLocationAction(action)

6. GAMEFACADE DISPATCH (GameFacade.cs)
   - UI calls: await GameFacade.ExecuteLocationAction(LocationActionType.Rest, locationId)
   - GameFacade switches on action.ActionType enum
   - Case LocationActionType.Rest: delegates to ResourceFacade.ExecuteRest()

7. SPECIALIZED FACADE EXECUTION (ResourceFacade.cs)
   - ResourceFacade.ExecuteRest() executes game logic:
     - Advances 1 time segment via TimeFacade
     - Recovers +1 health
     - Recovers +1 stamina
     - Hunger increases +5 (automatic via time progression)

8. STATE UPDATE
   - Player resources updated in GameWorld
   - UI refreshes with new state
```

### 10. Critical Principles

**1. Enum Catalogues Prevent Runtime Errors**
- All valid action types defined in enums
- Parsers validate JSON against enums at startup
- Unknown action types crash with descriptive errors BEFORE game starts
- NO runtime string matching errors

**2. Single Dispatch Point in GameFacade**
- ALL action execution goes through GameFacade methods
- NO scattered string matching across multiple files
- GameFacade orchestrates specialized facades
- Follows existing patterns (ExecuteListen, PerformWork, etc.)

**3. Strong Typing Throughout Pipeline**
- JSON strings converted to enums by parsers
- Domain entities use enum types
- GameFacade switches on enums
- UI passes enums to GameFacade
- NO string comparisons at runtime

**4. Property-Based Matching for LocationActions**
- LocationActions use RequiredProperties/OptionalProperties/ExcludedProperties
- Locations tagged with LocationPropertyType enums
- Actions automatically filtered by property matching
- NO manual ID lists or hardcoded availability

**5. Specialized Facades Handle Domain Logic**
- GameFacade delegates to ResourceFacade, TimeFacade, LocationFacade, etc.
- Each facade encapsulates ONE business domain
- Facades remain decoupled (never call each other directly)
- GameFacade orchestrates cross-facade operations

**6. No Dictionary Disease**
- NO string-based actionType matching
- NO Dictionary<string, object> for actions
- NO runtime type checking or casting
- Strong typing enforced from JSON → Parser → Domain → UI

---

## GAMEWORLD STATE MANAGEMENT

**Location**: `src/GameState/GameWorld.cs`

### GameWorld Responsibilities

**State Collections**:
```csharp
// Core Entities
public List<NPC> NPCs { get; set; }
public List<Venue> Venues { get; set; }
public List<LocationEntry> Locations { get; set; }
private Player Player { get; set; }

// Three Parallel Tactical Systems - Card Templates
public List<SocialCard> SocialCards { get; set; }
public List<MentalCard> MentalCards { get; set; }
public List<PhysicalCard> PhysicalCards { get; set; }

// Three Parallel Tactical Systems - Challenge Decks
public List<SocialChallengeDeck> SocialChallengeDecks { get; }
public List<MentalChallengeDeck> MentalChallengeDecks { get; }
public List<PhysicalChallengeDeck> PhysicalChallengeDecks { get; }

// Strategic Layer - Scene System
public List<SceneTemplate> SceneTemplates { get; set; }
public List<Scene> Scenes { get; set; }

// Player Stats System
public List<PlayerStatDefinition> PlayerStatDefinitions { get; set; }
public StatProgression StatProgression { get; set; }
```

**State Operations**:
```csharp
GetPlayer() → Single player instance
GetPlayerResourceState() → Current player resources
GetLocation(string locationId) → Location by ID
GetAvailableStrangers() → NPCs available at venue/time
RefreshStrangersForTimeBlock() → Time-based NPC availability
ApplyInitialPlayerConfiguration() → Apply starting conditions from JSON
```

### CRITICAL GAMEWORLD PRINCIPLES

**1. Zero External Dependencies**
- GameWorld NEVER depends on services, managers, or external components
- All dependencies flow **INWARD** toward GameWorld, never outward
- GameWorld does **NOT** create any managers or services

**2. Single Source of Truth**
- ALL game state lives in GameWorld - no parallel state in services
- Services read/write to GameWorld but don't maintain their own copies
- NO SharedData dictionaries or TempData storage

**3. No Business Logic**
- GameWorld contains **STATE**, not **BEHAVIOR**
- Business logic belongs in services/facades, not GameWorld
- GameWorld provides data access, not game rules

---

## SERVICE & SUBSYSTEM LAYER

**Location**: `src/Services/GameFacade.cs` & `src/Subsystems/*/`

### Service Architecture Hierarchy

```
GameFacade (Pure Orchestrator)
├── THREE PARALLEL TACTICAL SYSTEMS
│   ├── SocialFacade (Social challenges - conversations with NPCs)
│   ├── MentalFacade (Mental challenges - investigations at locations)
│   └── PhysicalFacade (Physical challenges - obstacles at locations)
├── SUPPORTING SYSTEMS
│   ├── LocationFacade (Movement and spot management)
│   ├── ResourceFacade (Health, hunger, coins, stamina)
│   ├── TimeFacade (Time progression and segments)
│   ├── TravelFacade (Route management and travel)
│   ├── TokenFacade (Relationship tokens)
│   ├── NarrativeFacade (Messages and observations)
│   └── ExchangeFacade (NPC trading system)
└── SCENES & CONTENT
    ├── SceneInstantiator (Scene spawning from templates)
    ├── SpawnFacade (Scene spawn condition evaluation)
    ├── ContentGenerationFacade (Dynamic package creation)
    └── RewardApplicationService (Reward application after choices)
```

### Facade Responsibilities

**GameFacade** - Pure orchestrator for UI-Backend communication
- Delegates ALL business logic to specialized facades
- Coordinates cross-facade operations (e.g., completing goals triggers investigations)
- Handles UI-specific orchestration
- NO business logic - only coordination

**Three Parallel Tactical System Facades** - Equivalent depth challenge systems
- `SocialFacade`: Initiative/Momentum/Doubt/Cadence conversation mechanics with NPCs
- `MentalFacade`: Progress/Attention/Exposure/Leads investigation mechanics at locations
- `PhysicalFacade`: Breakthrough/Exertion/Danger/Aggression obstacle mechanics at locations
- Each follows same architectural pattern: Builder/Threshold/Session resources + Binary actions
- All three systems use unified 5-stat progression (Insight/Rapport/Authority/Diplomacy/Cunning)

**Supporting Facades** - Game systems that integrate with tactical challenges
- `ExchangeFacade`: Separate NPC trading system (instant exchanges, not conversations)
- `LocationFacade`: Movement validation, location properties, spot management
- `ResourceFacade`: Permanent resources (Health, Stamina, Hunger, Focus, Coins)
- Each facade encapsulates ONE business domain

### Subsystem Organization

**Location**: `src/Subsystems/[Domain]/`

```
THREE PARALLEL TACTICAL SYSTEMS (Equivalent Depth)
Social/         → Social challenges: Card mechanics, momentum, conversation sessions
Mental/         → Mental challenges: Investigation mechanics, leads, observation system
Physical/       → Physical challenges: Obstacle mechanics, aggression, combo execution

SUPPORTING SYSTEMS
Exchange/       → NPC trading, inventory validation
Location/       → Movement, spot properties, location actions
Resource/       → Health, hunger, coins, stamina, focus management
Time/           → Segment progression, time block transitions
Token/          → Relationship tokens, connection tracking
Travel/         → Route discovery, travel validation
Narrative/      → Message system, observation rewards

SCENE & SPAWN SYSTEMS
Spawn/          → Scene spawn condition evaluation, template instantiation
ProceduralContent/ → Procedural scene generation, AI content integration
Catalogues/     → Parse-time categorical property translation
```

---

## UI COMPONENT ARCHITECTURE

**Location**: `src/Pages/*.razor` & `src/Pages/Components/*.razor`

### Authoritative Page Pattern

**GameScreen.razor** - Single authoritative parent
- Owns ALL screen state and manages child components directly
- Provides outer structure (resources bar, headers, time display)
- Child components rendered INSIDE GameScreen's container
- Children call parent methods directly via CascadingValue

**Child Components** - Screen-specific content only
```
THREE PARALLEL TACTICAL SYSTEMS
ConversationContent.razor  → Social challenges (conversations with NPCs)
MentalContent.razor        → Mental challenges (investigations at locations)
PhysicalContent.razor      → Physical challenges (obstacles at locations)

SUPPORTING SCREENS
LocationContent.razor      → Location exploration UI
ExchangeContent.razor      → NPC trading interface
TravelContent.razor        → Route selection and travel

INVESTIGATION MODALS
InvestigationDiscoveryModal.razor  → Investigation discovery notifications
InvestigationActivationModal.razor → Investigation intro completion
InvestigationProgressModal.razor   → Phase completion notifications
InvestigationCompleteModal.razor   → Investigation completion rewards
```

### Component Communication Pattern

**Direct Parent-Child Communication**:
```csharp
// Child receives parent reference
<CascadingValue Value="@this">
  <LocationContent OnActionExecuted="RefreshUI" />
</CascadingValue>

// Child calls parent methods directly
GameScreen.StartConversation(npcId, requestId)
GameScreen.NavigateToQueue()
GameScreen.HandleTravelRoute(routeId)
```

**Context Objects for Complex State**:
```csharp
THREE PARALLEL TACTICAL SYSTEMS
SocialChallengeContext   → Complete conversation state (Social challenge)
MentalSession            → Investigation state (Mental challenge)
PhysicalSession          → Obstacle state (Physical challenge)

SUPPORTING CONTEXTS
ExchangeContext          → NPC trading session state
TravelDestinationViewModel → Route and destination display state
LocationScreenViewModel  → Location exploration state
```

### CRITICAL UI PRINCIPLES

**1. UI is Dumb Display Only**
- NO game logic in Razor components
- NO attention costs or availability logic in UI
- Backend determines ALL game mechanics through facades

**2. No Shared Mutable State**
- Services provide operations, NOT state storage
- NavigationCoordinator handles navigation ONLY, not data passing
- State lives in components, not services

**3. Screen Component Constraints**
- Screen components NEVER define their own game-container or headers
- GameScreen provides outer structure, children provide content only
- Screen components wrapped in semantic classes like 'conversation-content'

---

## DATA FLOW PATTERNS

### Complete Pipeline Flow

```
JSON Files (Content Definition)
    ↓ [Static Parsers]
Domain Models (Strongly Typed Objects)
    ↓ [GameWorldInitializer]
GameWorld (Single Source of Truth)
    ↓ [Service Facades]
Business Logic Operations
    ↓ [Context Objects]
UI Components (User Interface Display)
    ↓ [User Actions]
Service Facades (State Updates)
    ↓ [GameWorld Updates]
State Persistence
```

### Request/Response Flow

**User Action → UI Response**:
```
1. User clicks UI element
2. Component calls GameScreen method
3. GameScreen calls GameFacade method
4. GameFacade orchestrates subsystem facades
5. Facades execute business logic
6. Facades update GameWorld state
7. GameScreen refreshes UI with new state
```

### Context Creation Pattern

**Complex Operations Use Dedicated Contexts**:
```csharp
// Context created atomically BEFORE navigation
ConversationContext context = await GameFacade.CreateConversationContext(npcId, requestId);

// Context contains ALL data needed for operation
context.NpcInfo = npc data
context.LocationInfo = current location
context.PlayerResources = resource state
context.Session = conversation session

// Context passed as single parameter to child component
<ConversationContent Context="@context" />
```

---

## CRITICAL ARCHITECTURAL PRINCIPLES

### 1. Dependency Inversion
- **All dependencies flow INWARD toward GameWorld**
- GameWorld has zero external dependencies
- Services depend on GameWorld, not vice versa
- UI depends on services, services never depend on UI

### 2. Single Responsibility
- Each facade handles exactly ONE business domain
- GameWorld provides state access, not business logic
- UI components render state, don't contain game rules
- Parsers convert data formats, don't store state

### 3. Immutable Content Pipeline
- JSON content loaded once at startup
- Static parsers create immutable domain objects
- Content never reloaded or modified at runtime
- Domain models are data containers, not active objects

### 4. Clean Architecture Boundaries
```
UI Layer          → GameScreen, Components (Blazor)
Application Layer → GameFacade, Specialized Facades
Domain Layer      → GameWorld, Domain Models
Infrastructure    → Parsers, JSON Files
```

### 5. No Abstraction Over-Engineering
- **NO interfaces unless absolutely necessary**
- **NO inheritance hierarchies** - use composition
- **NO abstract base classes** - keep code direct
- Concrete classes only, straightforward implementations

### 6. State Isolation
- **NO parallel state tracking** across multiple objects
- When duplicate state found, identify single source of truth
- Other objects delegate to canonical source
- **NO caching layers** that can become stale

### 7. Catalogue Pattern: No String Matching, No Dictionaries

**THE FUNDAMENTAL PRINCIPLE:**

**JSON describes entities categorically. Parsers translate categorical descriptions to concrete values via Catalogues. Runtime code uses only concrete, strongly-typed properties. No strings, no dictionaries, no lookups.**

#### Three Phases of Data Flow

**PHASE 1: AUTHORING (JSON - Categorical/Descriptive)**
- Content creators describe entities in human-readable categorical terms
- Properties are descriptive ("Full", "Partial", "Fragile", "Simple")
- Some properties are absolute concrete values (coinCost: 10, time: 1)
- NO runtime semantics embedded in JSON - JSON describes WHAT, not HOW

**PHASE 2: PARSING (Translation - One Time Only)**
- Parser reads JSON via DTO
- Parser calls Catalogue to translate categorical → concrete
- Catalogue returns strongly-typed concrete values (int, bool, object)
- Parser stores concrete values directly on entity properties
- Entity persisted to GameWorld
- Translation happens ONCE at game initialization, NEVER during gameplay

**PHASE 3: RUNTIME (Concrete Values Only)**
- GameFacade/Facades fetch entities from GameWorld
- Use concrete properties directly (action.HealthRecovery, action.CoinCost)
- NO catalogue lookups - values already calculated
- NO string matching - no "if (id == 'something')" ever
- NO dictionary lookups - no Cost["coins"] ever
- PURE strongly-typed property access - compiler-enforced correctness

#### What This Eliminates

**FORBIDDEN FOREVER:**
1. String matching: `if (action.Id == "secure_room")`
2. Dictionary lookups: `Cost["coins"]`, `Cost.ContainsKey("coins")`
3. Dictionary properties: `Dictionary<string, int> Cost`
4. Enum routing at runtime: `switch (recoveryType)` in GameFacade
5. Catalogue calls at runtime: Catalogues live in Parsers folder, never imported by Facades
6. ID-based behavior branching: Entity IDs are for reference only, never control logic

**Why these are forbidden:**
- String matching = runtime typo bugs, no IntelliSense, couples code to JSON IDs
- Dictionaries = no type safety, hidden properties, runtime string keys
- Runtime catalogues = wasted CPU, violates parse-time translation principle
- ID-based logic = magic behavior tied to JSON string values, brittle

#### Benefits of Parse-Time Translation

1. **Fail Fast**: Bad JSON crashes at game init with clear error, not during gameplay
2. **Zero Runtime Overhead**: All calculations done once, not on every action execution
3. **Type Safety**: Compiler catches property access errors, no runtime string bugs
4. **IntelliSense Works**: Developers see real properties (action.HealthRecovery: int)
5. **AI Content Generation**: AI describes effects categorically, parser scales to current game state
6. **No Magic Strings**: Eliminates entire class of typo bugs ("coins" vs "coin")
7. **Testable**: Can unit test catalogues independently from runtime logic
8. **Maintainable**: Change catalogue = affects all entities, change entity property = compiler finds usages

#### Existing Catalogue Examples

- **EquipmentDurabilityCatalog**: JSON "Fragile" → (uses: 2, repairCost: 10) stored on entity
- **SocialCardEffectCatalog**: JSON (stat, depth) → CardEffectFormula with concrete values
- **PhysicalCardEffectCatalog, MentalCardEffectCatalog**: Same pattern

#### Code Review Enforcement

**Parser Code:**
- All categorical JSON properties translated via catalogue?
- All concrete values stored on entity properties (int, bool, object)?
- NO Dictionary<string, X> on entities?
- Catalogue throws on unknown categorical values?

**Runtime Code (GameFacade, Facades, Services):**
- NO catalogue imports?
- NO string comparisons (`action.Id == "something"`)?
- NO dictionary lookups?
- ONLY concrete property access?

**Entity Classes:**
- Properties are concrete types (int, bool, strongly-typed classes)?
- NO Dictionary<string, X> properties?
- Property names describe mechanics, not categories?

**If you see Dictionary or string matching in runtime code, STOP. The architecture is violated.**

#### The Complete Principle

**JSON is descriptive. Parsers translate descriptions to mechanics. Runtime executes mechanics.**

Content creators describe WHAT entities do in human terms.
Parsers translate WHAT to HOW using catalogues.
Runtime executes HOW using strongly-typed properties.

No strings in runtime. No dictionaries in entities. No catalogue lookups after initialization.
**Parse once. Execute forever with pure data access.**

---

## COMPONENT DEPENDENCIES

### Core Dependencies Graph

```
GameScreen
├── Requires: GameFacade
├── Manages: All screen navigation
└── Children: LocationContent, ConversationContent, etc.

GameFacade
├── Requires: GameWorld + All Specialized Facades
├── Provides: Orchestration layer
└── Dependencies: ConversationFacade, LocationFacade, etc.

Specialized Facades
├── Require: GameWorld + Domain-specific managers
├── Provide: Business logic for specific domains
└── Update: GameWorld state through operations

GameWorld
├── Requires: Nothing (zero external dependencies)
├── Provides: Single source of truth for all state
└── Contains: All domain model collections
```

### Service Registration Pattern

```csharp
// GameWorld created BEFORE service registration
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();

// Services registered with GameWorld dependency
services.AddSingleton(gameWorld);
services.AddScoped<ConversationFacade>();
services.AddScoped<GameFacade>();
```

### Critical Integration Points

**1. Three Parallel Tactical Systems Integration**
- **SocialFacade**: Social challenges (conversations) at NPCs
  - Initiative/Momentum/Doubt/Cadence resource system
  - Integrates with TokenFacade for relationship tokens
  - Integrates with KnowledgeService for knowledge/secrets
  - Personality rules (Proud, Devoted, Mercantile, Cunning, Steadfast)

- **MentalFacade**: Mental challenges (investigations) at Locations
  - Progress/Attention/Exposure/Leads resource system
  - Pauseable session model (can leave and return)
  - Location properties (Delicate, Obscured, Layered, Time-Sensitive, Resistant)

- **PhysicalFacade**: Physical challenges (obstacles) at Locations
  - Breakthrough/Exertion/Danger/Aggression resource system
  - One-shot session model (must complete in single attempt)
  - Challenge types (Combat, Athletics, Finesse, Endurance, Strength)

**2. Scene System Integration**
- Scenes spawn from SceneTemplates based on SpawnConditions
- Situations contain ChoiceTemplates that route to tactical systems
- ChoiceActionType.StartChallenge bridges strategic to tactical layer
- Scene.PlacementType determines where scene appears (NPC/Location/Route)

**3. Unified 5-Stat Progression**
- All cards across all three systems bind to: Insight/Rapport/Authority/Diplomacy/Cunning
- Stat levels determine card depth access (Level 1: depths 1-2, Level 3: depths 1-4, etc.)
- Playing cards grants XP to bound stat
- Stats manifest differently per system (Insight = pattern recognition in Mental, structural analysis in Physical, reading people in Social)

**4. Location System Integration**
- LocationFacade coordinates NPC placement at locations
- Integrates with TravelFacade for route validation
- Coordinates with TimeFacade for time-based availability
- Scenes can appear at Locations based on PlacementFilter

**5. Resource System Integration**
- ResourceFacade manages permanent resources (Health, Stamina, Focus, Hunger, Coins)
- **Mental challenges cost Focus** (concentration depletes)
- **Physical challenges cost Health + Stamina** (injury risk + exertion)
- **Social challenges cost nothing permanent** (but take time)
- Integrates with TimeFacade for time-based resource changes

---

## DEVELOPMENT GUIDELINES

### Before Making Any Changes

1. **Read this entire architecture document**
2. **Identify which layer you're modifying** (Content/Domain/Service/UI)
3. **Trace dependencies** using search tools to find all references
4. **Understand the impact radius** of your changes
5. **Verify architectural principles** are maintained

### Adding New Features

1. **Determine the business domain** - which facade should own it?
2. **Check if GameWorld state** needs new properties
3. **Design the service interface** following existing patterns
4. **Create context objects** for complex UI operations
5. **Follow the UI component hierarchy** - GameScreen → Child Components

### Modifying Existing Systems

1. **Never break the dependency flow** - dependencies always flow inward
2. **Never add shared mutable state** - use GameWorld as single source
3. **Never put business logic in UI** - keep facades responsible for rules
4. **Never bypass the facade layer** - UI must go through GameFacade

---

**This architecture ensures clean separation of concerns, predictable data flow, and maintainable code structure while supporting the complex conversation-based RPG mechanics of Wayfarer.**