# Wayfarer: Intended Architecture - Complete Conceptual Model

**⚠️ ARCHIVED DOCUMENT - 2025-01 ⚠️**

**Reason for Archiving:** This document had 70% content overlap with ARCHITECTURE.md, violating HIGHLANDER principle (one source of truth per topic). Archived to prevent documentation duplication and maintenance burden.

**Current Documentation:** See ARCHITECTURE.md for authoritative system architecture.

**Historical Context:** This was a conceptual design document from October 2025. Most content has been implemented and integrated into ARCHITECTURE.md. Kept for historical reference.

---

**Original Header:**

**Version:** 1.0 - Intended State
**Date:** 2025-10-28
**Purpose:** High-level architectural specification for complete implementation

---

## Executive Summary

Wayfarer is a narrative strategy game where players navigate a medieval city through interconnected story scenes. The architecture is built on template-driven dynamic content generation, perfect information strategic decision-making, and a unified execution model that maintains clean separation between strategic planning and tactical gameplay.

This document describes the INTENDED architecture - the complete, coherent vision for how all systems integrate. It provides the conceptual foundation needed to implement the full system without referencing implementation details.

---

## Part 1: Core Architectural Principles

### Principle 1: HIGHLANDER - Single Orchestrator

**Concept:** There can be only one execution orchestrator - GameFacade.

**Motivation:** In complex systems, multiple orchestrators create circular dependencies, unclear ownership, and difficult-to-trace state changes. Every action in the game must flow through a single point that coordinates all other systems.

**How It Works:** GameFacade is the conductor. When a player action occurs, the UI calls GameFacade with intent. GameFacade coordinates validation through executors, applies costs through resource facades, applies rewards through consequence facades, and manages time advancement side effects. No other system calls other systems directly.

**Why This Matters:** Clear ownership prevents bugs. When something goes wrong, there's one place to look. When adding features, there's one place to integrate. Testing becomes straightforward because all state changes flow through one coordinator.

---

### Principle 2: Template-Driven Content

**Concept:** All dynamic content originates from reusable templates that define patterns, not specific instances.

**Motivation:** Hand-authoring every narrative beat doesn't scale. Templates separate content structure (reusable) from content instances (throwaway). A single template can generate thousands of unique story moments by varying parameters.

**How It Works:** SceneTemplates define abstract patterns - "a social scene with 2-4 conversation choices". When instantiated, the template selects a concrete NPC from the game world based on filters, generates situations with that NPC's name and personality, and creates unique narrative moments. The template remains unchanged and reusable.

**Why This Matters:** Enables AI-generated content. An AI can write templates in the same JSON format as human designers. The game processes both identically. This allows infinite narrative expansion without code changes.

---

### Principle 3: Perfect Information Through Provisional State

**Concept:** Players see complete consequences of strategic decisions before committing.

**Motivation:** Strategic depth requires informed choice. If players can't see what spawns from their decisions, choices become random gambling rather than strategic planning. Perfect information is necessary for meaningful strategic gameplay.

**How It Works:** When a choice would spawn a new scene, the game immediately creates a provisional version of that scene - a "mechanical skeleton" showing WHERE it spawns, WHAT type of scene it is, and WHICH entity it involves. The player sees "This choice spawns a Social Scene at the Inn with the Innkeeper" before selecting. If selected, the provisional scene becomes active and fully instantiated. If not selected, the provisional scene is deleted.

**Why This Matters:** Strategic decisions have weight. Players can plan paths through the narrative. They accept opportunity costs knowingly. Failed plans are learning experiences, not feel-bad moments caused by hidden information.

---

### Principle 4: Composition Over Duplication

**Concept:** Entities reference shared data sources rather than copying data.

**Motivation:** Duplication creates inconsistency. If ten instances copy data from a template, changing the template doesn't update the instances. References maintain single source of truth while allowing variation in instance-specific runtime state.

**How It Works:** A Scene instance holds a reference to its SceneTemplate. When the game needs archetype information, it reads through the reference. The Scene only stores instance-specific data like which situation is current, what state it's in, and which concrete entity it's placed on.

**Why This Matters:** Templates can be updated without breaking active scenes. Save files remain small because instances don't duplicate template data. Memory usage stays reasonable even with hundreds of active scenes.

---

### Principle 5: Dynamic World With Persistent Scaffolding

**Concept:** The world structure is permanent, but story opportunities are ephemeral.

**Motivation:** Not every location should have content at all times. A living world has quiet moments. Forcing content everywhere creates narrative bloat and choice paralysis.

**How It Works:** Locations, NPCs, and Routes exist permanently as world scaffolding. They have atmospheric properties - names, descriptions, visual elements. Scenes spawn dynamically onto this scaffolding based on player actions. When no scene is active at a location, the location still exists and displays its atmospheric properties, but provides no story actions.

**Why This Matters:** Creates pacing. Players experience downtime between story beats. The world feels persistent and real rather than a content delivery mechanism. Discovery feels meaningful because not every location is immediately interactive.

---

### Principle 6: Separation of Strategic and Tactical Layers

**Concept:** Strategic decision-making and tactical execution are distinct gameplay modes with different information models.

**Motivation:** Mixing strategy and tactics creates cognitive overload. Players can't plan strategically if they must simultaneously execute tactically. The genres require different mental models.

**How It Works:** Strategic layer shows perfect information about available choices, their costs, their rewards, and their consequences. Players decide WHETHER to attempt challenges. Tactical layer hides specific card draws, exact challenge flow, and execution details. Players demonstrate skill to overcome challenges they've strategically chosen to face.

**Why This Matters:** Each layer can be deep without overwhelming players. Strategic planning feels meaningful. Tactical execution feels skillful. Both coexist without interference.

---

## Part 2: Entity Conceptual Model

### The Template Hierarchy

Templates are immutable archetypes that define reusable patterns. They exist independently of any specific game instance.

**SceneTemplate** is the top-level pattern definition. It describes:
- What type of story content this represents (social encounter, investigation, combat scenario)
- What kind of entity it needs to spawn on (location with certain tags, NPC with certain personality)
- Whether it should spawn at game initialization as starter content
- What situations compose this scene

**SituationTemplate** defines a narrative context within a scene. Each situation represents one player-facing moment with choices. It describes:
- Narrative structure and emotional tone
- How situations cascade (linear progression, branching paths, hub-and-spoke options)
- What choices are available within this narrative moment

**ChoiceTemplate** defines one player-facing decision within a situation. Following the Sir Brante pattern, each situation offers 2-4 choices. Each choice describes:
- What requirements must be met for this choice to be available versus locked
- What strategic costs the choice demands (Resolve, Coins, Time)
- What rewards the choice grants (resources, state changes, new scene spawns)
- What type of action this represents (instant effect, start challenge, navigation)

This hierarchy remains stable. Templates never change during gameplay. They are the design language authors use to define content patterns.

### Scene Archetypes vs Situation Archetypes

The template generation system operates at two levels: **SceneArchetypeCatalog** and **SituationArchetypeCatalog**. Understanding their relationship is critical for content authoring.

**SituationArchetypeCatalog** generates single-situation mechanical patterns:
- Defines number of choices (typically 4)
- Defines path types (stat-gated/money-gated/challenge/fallback)
- Defines base cost formulas (scaled by entity properties)
- Defines base reward formulas (scaled by environmental properties)
- Returns List<ChoiceTemplate> ready for embedding in SituationTemplate

**SceneArchetypeCatalog** composes multiple situations into complete narrative arcs:
- Calls SituationArchetypeCatalog to generate choices for each situation
- **Enriches** choices with scene-specific rewards (location unlocks, item grants, cleanup)
- Defines SituationSpawnRules (how situations transition)
- Declares DependentResources (locations/items scene will create)
- Returns complete SceneArchetypeDefinition with multiple situations

**The Enrichment Layer:**

Situation archetypes define BASE mechanical structure. Scene archetypes ADD context-specific rewards.

Example:
```
SituationArchetypeCatalog.Generate("service_negotiation") returns:
  Choice 1: Stat-gated path (base structure, no specific rewards)
  Choice 2: Money-gated path (base structure, no specific rewards)
  Choice 3: Challenge path (base structure, no specific rewards)
  Choice 4: Fallback path (base structure, no specific rewards)

SceneArchetypeCatalog.Generate("inn_lodging") receives these choices and enriches:
  Choice 1: + Unlock private_room, Grant room_key
  Choice 2: + Unlock private_room, Grant room_key
  Choice 3: + OnSuccess: Unlock private_room, Grant room_key
  Choice 4: (no enrichment - fallback remains poor outcome)
```

**Why Two Levels:**

1. **Reusability:** Same situation archetype used by multiple scene archetypes
   - `service_negotiation` used by: inn_lodging, bathhouse_service, healer_treatment, ferry_passage
   - Each scene enriches with different resources (room_key vs bathhouse_token vs treatment_pass)

2. **Separation of Concerns:** Situations define HOW (mechanical paths), Scenes define WHAT (specific resources)

3. **Modularity:** Change negotiation mechanics once (situation archetype), affects all services

**Composition Flow:**
1. JSON references: `"sceneArchetypeId": "inn_lodging"`
2. Parser calls: `SceneArchetypeCatalog.Generate("inn_lodging", tier, context)`
3. Scene archetype calls: `SituationArchetypeCatalog.Generate("service_negotiation", ...)`
4. Situation archetype returns: List<ChoiceTemplate> with base structure
5. Scene archetype enriches: Adds LocationsToUnlock, ItemIds, etc.
6. Scene archetype repeats for all situations in scene (negotiate, execute, depart)
7. Returns complete SceneArchetypeDefinition to parser

### Implementing New Situation Archetype

Situation archetypes generate base choice structures reusable across multiple scene types.

**Method Signature:**
```csharp
private static List<ChoiceTemplate> GenerateMyArchetypeChoices(
    SituationArchetype archetype,
    string situationTemplateId,
    GenerationContext context)
```

**Implementation Structure:**
```csharp
1. Scale base values by context properties
   scaledStatThreshold = context.NpcDemeanor switch { ... }
   scaledCoinCost = context.Quality switch { ... }

2. Build requirement formulas
   CompoundRequirement rapportReq = new CompoundRequirement {
       OrPaths = [new OrPath { NumericRequirements = [...] }]
   };

3. Create 4 choices with PathType assignments:

   Choice 1: Stat-Gated (PathType.InstantSuccess)
   - RequirementFormula = rapportReq (scaled)
   - CostTemplate = empty (free)
   - RewardTemplate = EMPTY (scene enriches)
   - ActionType = Instant

   Choice 2: Money-Gated (PathType.InstantSuccess)
   - RequirementFormula = empty (always available if has coins)
   - CostTemplate = scaledCoinCost
   - RewardTemplate = EMPTY
   - ActionType = Instant

   Choice 3: Challenge (PathType.Challenge)
   - RequirementFormula = empty (always available)
   - CostTemplate = Resolve cost
   - RewardTemplate = EMPTY
   - ActionType = StartChallenge
   - ChallengeType = Social/Mental/Physical
   - ChallengeId = deck reference

   Choice 4: Fallback (PathType.Fallback)
   - RequirementFormula = empty
   - CostTemplate = empty
   - RewardTemplate = EMPTY
   - ActionType = Instant
   - ActionTextTemplate = "Decline/Leave/Abandon"

4. Return List<ChoiceTemplate>
```

**Key Patterns:**
- **RewardTemplate ALWAYS EMPTY:** Scene archetype enriches based on PathType
- **Scale by context:** Use universal scaling properties (NPCDemeanor, Quality, PowerDynamic)
- **Consistent PathType assignment:** 2 InstantSuccess, 1 Challenge, 1 Fallback
- **ActionTextTemplate uses placeholders:** `{npcName}`, `{locationName}`, `{serviceType}`

### Implementing New Scene Archetype

Scene archetypes compose multiple situations with enriched rewards.

**Method Signature:**
```csharp
private static SceneArchetypeDefinition GenerateMyScene(
    int tier,
    GenerationContext context)
```

**Implementation Structure:**
```csharp
1. Define situation IDs:
   string sceneId = "my_scene";
   string sit1Id = $"{sceneId}_situation1";
   string sit2Id = $"{sceneId}_situation2";

2. FOR EACH SITUATION:
   a. Get archetype:
      SituationArchetype arch = SituationArchetypeCatalog.GetArchetype("archetype_id");

   b. Generate base choices:
      List<ChoiceTemplate> baseChoices =
          SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(arch, sitId, context);

   c. Enrich choices by PathType:
      List<ChoiceTemplate> enriched = new List<ChoiceTemplate>();
      foreach (ChoiceTemplate choice in baseChoices) {
          switch (choice.PathType) {
              case InstantSuccess:
                  // Add immediate rewards
                  enriched.Add(new ChoiceTemplate {
                      ...choice properties...,
                      RewardTemplate = new ChoiceReward {
                          LocationsToUnlock = [...],
                          ItemIds = [...]
                      }
                  });
              case Challenge:
                  // Add OnSuccessReward
                  enriched.Add(new ChoiceTemplate {
                      ...choice properties...,
                      OnSuccessReward = new ChoiceReward { ... }
                  });
              case Fallback:
                  // Pass through unchanged
                  enriched.Add(choice);
          }
      }

   d. Create SituationTemplate:
      SituationTemplate sit = new SituationTemplate {
          Id = sitId,
          Name = "Situation Name",
          Type = SituationType.Normal,
          NarrativeTemplate = null,  // AI generates
          ChoiceTemplates = enriched,
          Priority = 100,
          NarrativeHints = new NarrativeHints { ... },
          RequiredLocationId = context.LocationId or "generated:resource_id",
          RequiredNpcId = context.NpcId or null
      };

3. Define spawn rules:
   SituationSpawnRules rules = new SituationSpawnRules {
       Pattern = SpawnPattern.Linear,
       InitialSituationId = sit1Id,
       Transitions = new List<SituationTransition> {
           new SituationTransition {
               SourceSituationId = sit1Id,
               DestinationSituationId = sit2Id,
               Condition = TransitionCondition.Always
           }
       }
   };

4. Declare dependent resources (if needed):
   DependentResourceCatalog.DependentResources resources =
       DependentResourceCatalog.GenerateForActivity(ServiceActivityType.MyActivity);

5. Return SceneArchetypeDefinition:
   return new SceneArchetypeDefinition {
       SituationTemplates = [sit1, sit2, ...],
       SpawnRules = rules,
       DependentLocations = [resources.LocationSpec],
       DependentItems = [resources.ItemSpec]
   };
```

**Key Patterns:**
- **Enrichment by PathType:** Switch on PathType to determine reward placement
- **InstantSuccess:** Add RewardTemplate (immediate rewards)
- **Challenge:** Add OnSuccessReward (conditional rewards)
- **Fallback:** Pass through unchanged (no rewards)
- **Dependent resources:** Use "generated:id" markers, resolve at spawn time
- **Transitions:** Define Pattern + explicit Transition list

### Archetype Reuse Decision Criteria

**Create New Situation Archetype When:**
- New mechanical pattern (different number/types of choices)
- New scaling requirements (different properties affect difficulty)
- New cost/reward structure (different resource types)

**Reuse Existing Situation Archetype When:**
- Same mechanical pattern, different narrative context
- Same 4-choice structure (stat/money/challenge/fallback)
- Same scaling properties apply

**Create New Scene Archetype When:**
- New multi-situation structure (different situation count/transitions)
- New resource dependency pattern (different locations/items generated)
- New narrative arc structure

**Reuse Existing Scene Archetype When:**
- Same situation count and transition pattern
- Same dependent resource types
- Different fictional context (same structure, different domain)

---

### The Runtime Entity Hierarchy

Runtime entities are living instances created from templates during gameplay. They have state, lifecycle, and mutability.

**Scene** is a persistent instance of a SceneTemplate. When created, it:
- References its template for all archetypal properties
- Selects a concrete placement entity from the game world based on template's filters
- Records where it exists (Location X, NPC Y, or Route Z)
- Embeds instantiated situations as children
- Tracks progression through those situations
- Maintains lifecycle state (Provisional, Active, Completed)

**Situation** is an embedded child within a Scene. It represents the current narrative moment. When instantiated:
- Narrative text is generated from template with placeholders replaced
- References back to its parent Scene for context
- Embeds instantiated choices as children
- Tracks which choices are available versus locked based on player state

**Choice** is an embedded child within a Situation. It represents one player-facing option. When instantiated:
- References its ChoiceTemplate for requirements, costs, and rewards
- May trigger creation of provisional scenes if rewards include scene spawns
- Stores runtime state (available, locked, selected)
- Records reference to any provisional scene it would spawn

This hierarchy is compositional. Scenes own Situations. Situations own Choices. Destruction cascades downward. The parent's lifecycle controls children's lifecycle.

---

### The Action Entity Layer

Action entities are execution interfaces that bridge the conceptual choice model to the UI's display contexts.

When a Situation is displayed to the player, its Choices must appear in context-appropriate UI. A choice at a location appears as a LocationAction. A choice with an NPC appears as an NPCAction. A choice during travel appears as a PathCard.

These are NOT separate content types. They are view projections of the same underlying Choice entity. When instantiating:
- Examine the Situation's placement context (Location, NPC, or Route)
- Create the appropriate action entity for that context
- Give it a reference to the Choice's ChoiceTemplate for requirements/costs/rewards
- Store it in a flat collection for execution lookup

Action entities are ephemeral. They exist while Situations are displayed and are cleaned up when situations advance. They do not persist in save files.

---

## Part 3: Provisional State and Perfect Information

### The Problem

Strategic decision-making requires knowing consequences. In many games, players choose blindly:
- "Talk to the merchant" - but what happens?
- "Investigate the warehouse" - but what spawns?
- "Help the guard" - but where does it lead?

Without visibility into consequences, choices aren't strategic decisions. They're random exploration.

### The Solution

Provisional state provides perfect information without spoiling tactical execution.

When a player views a Situation, they see all available Choices. For any Choice that would spawn a new Scene, the game immediately creates a provisional version of that Scene. This provisional Scene is a mechanical skeleton:
- Shows the Scene's archetype (Social, Investigation, Combat)
- Shows WHERE it would spawn (at which Location, with which NPC, on which Route)
- Shows WHEN it would appear (immediately, after delay, after prerequisite)

The provisional Scene does NOT show:
- Specific narrative text
- Exact choices within that future Scene
- Tactical challenge details

The player sees strategic information: "This choice spawns a Social Scene at the Inn with the Innkeeper". They can evaluate: "Do I want a social scene there? Do I have time? Do I have resources? What opportunity am I giving up?"

### Lifecycle

**Creation:** When Situation instantiates Choices, any Choice with scene spawn rewards triggers provisional scene creation. The SceneInstantiator creates the Scene with state set to Provisional. It's stored in a separate collection from active Scenes.

**Display:** UI queries provisional scenes by ID and displays preview information in the Choice card. Player sees strategic context before committing.

**Finalization:** When player executes the Choice, GameFacade finalizes the provisional Scene. Placeholders are replaced with concrete entity names. Narrative intro is generated. State changes to Active. Scene moves from provisional collection to active scenes collection.

**Deletion:** When player executes ANY Choice in the Situation, all OTHER Choices' provisional Scenes are deleted. Those futures didn't happen in this timeline. Memory is freed.

### Benefits

Players make informed strategic decisions. They accept opportunity costs knowingly. They plan multi-step paths through the narrative. They understand what they're committing to before acting.

Game designers can create branching content without hand-holding. Players discover naturally through exploration backed by perfect strategic information.

---

## Part 4: Execution Architecture

### The Pure Validator Pattern

Executors are stateless validators. They:
- Receive an action entity and player state
- Evaluate requirements using player's current resources and achievements
- Check if strategic costs can be paid
- Extract cost and reward data from the action's ChoiceTemplate
- Return a structured plan describing validation result and extracted data

Executors do NOT:
- Modify any game state
- Call facades or other services
- Apply costs or rewards
- Make decisions about execution flow

There is one executor per action context:
- LocationActionExecutor validates LocationAction entities
- NPCActionExecutor validates NPCAction entities  
- PathCardExecutor validates PathCard entities

All three follow identical patterns. They're type-safe wrappers around the same validation logic, specialized for their context.

### The Orchestration Pattern

GameFacade orchestrates all execution. When the UI signals player intent:

**Step 1: Retrieval**  
GameFacade fetches the relevant action entity from GameWorld's flat collection.

**Step 2: Validation**  
GameFacade calls the appropriate executor, passing the action entity, player, and game world. Executor returns an ActionExecutionPlan - a pure data structure containing validation result and cost/reward data.

**Step 3: Early Exit**  
If validation failed, GameFacade immediately returns failure result to UI. No state changes occur.

**Step 4: Cost Application**  
GameFacade applies strategic costs using domain facades. It consumes Resolve through ResourceFacade. Consumes Coins through ResourceFacade. Advances Time through TimeFacade. Each facade handles its domain concern.

**Step 5: Action Routing**
Based on the Choice's ActionType:
- If Instant: Apply rewards immediately and complete
- If StartChallenge: Route player to appropriate tactical system (Social, Mental, Physical) - see Bridge to Tactical Layer below
- If Navigate: Apply navigation payload to move player

**Step 6: Scene Lifecycle**  
If the executed Choice had a provisional Scene:
- Finalize that Scene (replace placeholders, generate narrative, change state to Active)
- Move it from provisional collection to active scenes collection

For all OTHER Choices in the same Situation:
- Delete their provisional Scenes from the provisional collection

**Step 7: Reward Application**  
Apply the Choice's rewards through ConsequenceFacade. This may grant resources, change player state, modify NPC bonds, or trigger achievement unlocks.

**Step 8: Time Side Effects**  
Process time advancement side effects. This is the HIGHLANDER principle in action - only GameFacade triggers time-dependent systems. It evaluates:
- NPC schedule changes if time block advanced
- Obligation deadline checks
- Scene expiration conditions
- World state updates

**Step 9: Result Return**  
Return structured result to UI indicating success or failure, whether refresh needed, any navigation changes.

### Why This Pattern

The orchestrator pattern provides:
- Single point of coordination (HIGHLANDER)
- Clear separation of validation and application
- Easy testing (executors are pure functions)
- Extensibility (add new action types by adding executors)
- Maintainability (all execution flows follow same pattern)

---

## Part 5: Bridge to Tactical Layer

### Strategic vs Tactical Layers

Wayfarer has two distinct gameplay layers that must never be confused:

**Strategic Layer** (Scene → Situation → Choice):
- Perfect information: All costs, requirements, rewards visible before selection
- Player decides WHAT to attempt
- Persistent entities in GameWorld
- State machine progression via Scene.AdvanceToNextSituation()

**Tactical Layer** (Mental/Physical/Social Challenges):
- Hidden complexity: Card draws, exact flow not visible before entry
- Player demonstrates skill in HOW to execute
- Temporary sessions created/destroyed per engagement
- Victory thresholds via SituationCards

### The Bridge Mechanism

ChoiceTemplate.ActionType is the ONLY connection between layers:

```csharp
public enum ChoiceActionType
{
    Instant,         // Stay in strategic layer
    Navigate,        // Stay in strategic layer
    StartChallenge   // Cross to tactical layer
}
```

When ActionType = StartChallenge:
- `ChallengeType` (TacticalSystemType): Social/Mental/Physical
- `ChallengeId` (string): Which deck to load for tactical cards
- `OnSuccessReward` / `OnFailureReward`: Applied after challenge outcome

### Challenge Spawning Flow

**Step 1: Bridge Detection**
GameFacade checks `plan.ActionType == ChoiceActionType.StartChallenge`

**Step 2: Context Storage**
GameFacade stores reward in pending context for later application:
```csharp
_gameWorld.PendingSocialContext = new SocialChallengeContext
{
    CompletionReward = plan.OnSuccessReward
};
```

**Step 3: Navigation**
Returns `IntentResult.NavigateScreen(ScreenMode.SocialChallenge)` to UI

**Step 4: Session Creation**
Appropriate facade (SocialFacade/MentalFacade/PhysicalFacade) creates temporary session

**Step 5: SituationCard Extraction**
Challenge extracts `Situation.SituationCards` for victory conditions:
- SituationCards stored in strategic layer (Situation.SituationCards list)
- Challenges READ them when spawned (tactical layer usage)
- Define victory thresholds: "Reach 8 Momentum = basic reward, 15 Momentum = optimal reward"

**Step 6: Tactical Gameplay**
Player plays tactical cards (SocialCard/MentalCard/PhysicalCard) to build resources

**Step 7: Victory/Failure**
When threshold reached or failure condition met, challenge ends

**Step 8: Return to Strategic**
GameFacade applies `OnSuccessReward` or `OnFailureReward` based on outcome, then Scene.AdvanceToNextSituation()

### Three Parallel Challenge Systems

**Social Challenges** (NPC conversations):
- Resources: Initiative, Momentum (builder), Doubt (threshold), Cadence (balance)
- Victory: Reach Momentum threshold from SituationCards
- Action Pair: SPEAK / LISTEN

**Mental Challenges** (location investigations):
- Resources: Attention (session), Progress (builder), Exposure (threshold), Leads (flow)
- Victory: Reach Progress threshold from SituationCards
- Action Pair: ACT / OBSERVE

**Physical Challenges** (location obstacles):
- Resources: Exertion (session), Breakthrough (builder), Danger (threshold), Aggression (balance)
- Victory: Reach Breakthrough threshold from SituationCards
- Action Pair: EXECUTE / ASSESS

All three systems have equivalent depth and follow parallel architecture.

### Critical Distinctions

**ChoiceTemplate vs SituationCard:**
- ChoiceTemplate: Strategic choice presented to player ("Persuade", "Intimidate", "Bribe")
- SituationCard: Tactical victory tier ("8 Momentum", "12 Momentum", "15 Momentum")
- Player selects ChoiceTemplate BEFORE entering tactical layer
- Player achieves SituationCard thresholds DURING tactical layer

**Storage vs Usage:**
- Storage: SituationCards stored in `Situation.SituationCards`
- Usage: Challenges extract and use them at spawn time
- Purpose: Define tactical victory conditions, NOT strategic choices

---

## Part 6: Three-Tier Timing Model

### Why Three Tiers

The three-tier timing model enables lazy instantiation, reducing memory and preventing action bloat in GameWorld.

### Tier 1: Templates (Parse Time)

Created once at game startup from JSON:
- SceneTemplate → SituationTemplate → ChoiceTemplate hierarchy
- Immutable archetypes stored in GameWorld.SceneTemplates
- Never modified during gameplay
- Design language for content authors

### Tier 2: Scenes/Situations (Spawn Time)

Created when Scene spawns from Obligation or SceneSpawnReward:
- Scene instance created with embedded Situations
- Situation.Template reference stored (ChoiceTemplates NOT instantiated yet)
- InstantiationState = Deferred
- NO actions created in GameWorld.LocationActions/NPCActions yet

**Why Deferred:**
Situations may require specific context (location + NPC combo) that player hasn't reached yet. Creating actions prematurely bloats GameWorld with thousands of inaccessible actions.

### Tier 3: Actions (Query Time)

Created ONLY when player enters matching context:

**Context Check:**
```csharp
// Player at Location X
SceneFacade queries: Which Situations require Location X?
For each matching Situation:
    If InstantiationState == Deferred:
        Instantiate actions from Template.ChoiceTemplates
        Set InstantiationState = Instantiated
        Add to GameWorld.LocationActions/NPCActions
```

**Ephemeral Lifecycle:**
- Actions created when context matches
- Actions displayed in UI
- Actions executed by GameFacade
- Actions deleted when Situation completes
- If player returns to same context later, actions recreated from Template

**Why Ephemeral:**
Prevents duplicate actions accumulating in GameWorld. Template is single source of truth. Actions are view projections generated on demand.

### Example Flow

**Spawn Time (Tier 2):**
```
Obligation spawns Scene "Secure Lodging"
  └─ Scene contains 3 Situations:
      ├─ Situation 1 (requires Location: Inn Common Room)
      │   └─ Template has 3 ChoiceTemplates (NOT instantiated)
      ├─ Situation 2 (requires Location: Inn Upper Floor)
      │   └─ Template has 2 ChoiceTemplates (NOT instantiated)
      └─ Situation 3 (requires Location: Private Room)
          └─ Template has 2 ChoiceTemplates (NOT instantiated)
```

**Query Time (Tier 3):**
```
Player enters Inn Common Room
  → SceneFacade queries: Situations at this location?
  → Finds Situation 1 (InstantiationState = Deferred)
  → Instantiates 3 LocationActions from Template.ChoiceTemplates
  → Adds to GameWorld.LocationActions
  → UI displays 3 action buttons

Player selects action, Situation 1 completes
  → Delete 3 LocationActions from GameWorld.LocationActions
  → Scene.AdvanceToNextSituation() → CurrentSituation = Situation 2

Player navigates to Inn Upper Floor
  → SceneFacade queries: Situations at this location?
  → Finds Situation 2 (InstantiationState = Deferred)
  → Instantiates 2 LocationActions from Template.ChoiceTemplates
  → Adds to GameWorld.LocationActions
  → UI displays 2 action buttons
```

### Benefits

**Memory Efficiency:**
Only actions at current context exist in GameWorld. No bloat from future/past situations.

**Single Source of Truth:**
Template defines ChoiceTemplates once. Runtime actions reference Template, never duplicate.

**Clean Lifecycle:**
Actions created when needed, deleted when done. No orphaned actions from completed situations.

**Easy Debugging:**
Query SceneFacade to see which Situations should appear at location. If InstantiationState = Deferred, actions haven't materialized yet.

---

## Part 7: Facade Responsibilities

### SceneFacade - Query Layer

SceneFacade is a read-only query service. It:
- Queries GameWorld's scene collections
- Filters for scenes at specific placements
- Evaluates choice requirements against player state
- Extracts display data for UI
- Returns query results as view models

SceneFacade does NOT:
- Execute choices
- Modify scenes
- Apply costs or rewards
- Make state changes

When UI needs to display a location's available actions:
- Calls SceneFacade with location ID and player
- SceneFacade queries active scenes at that location
- Extracts current situations from those scenes
- Evaluates each choice's requirements
- Categorizes choices as available versus locked
- Returns structured display model

UI receives everything needed for display without coupling to execution logic.

### LocationFacade - Wrapper Layer

LocationFacade provides the persistent context for location displays. It:
- Returns location's atmospheric properties
- Lists NPCs currently present
- Lists routes available for departure
- Provides header information (venue name, time)

This is the "where you are" layer. It's independent of scenes. Even with no active scenes, LocationFacade returns valid data showing the location exists and has properties.

UI combines LocationFacade output (persistent wrapper) with SceneFacade output (ephemeral content) to create complete location displays.

### GameFacade - Orchestrator

GameFacade is the HIGHLANDER. All execution flows through it. When player takes action:
- Validates through executor
- Applies costs through resource facades
- Routes to tactical systems if needed
- Applies rewards through consequence facade
- Manages scene lifecycle
- Processes time side effects
- Returns result to UI

GameFacade maintains no state itself. It coordinates stateful facades. It's the conductor, not a musician.

### Domain Facades

ResourceFacade, TimeFacade, ConsequenceFacade, and others manage domain-specific state. They:
- Encapsulate their domain logic
- Provide focused APIs for their concern
- Are invoked by GameFacade only
- Never call each other or GameFacade

This creates clean dependency flow: UI → GameFacade → Domain Facades. No circular dependencies. No hidden coupling.


## Part 10: Spawn Patterns and Cascade Design

Templates define how Situations cascade through spawn patterns. These patterns enable complex narrative flows without hardcoded content.

### Linear Progression

Situation A completes → spawns Situation B at same placement → B completes → spawns C. Creates guided story arc. Used for:
- Investigation chains that reveal information sequentially
- Tutorial sequences with building complexity
- Character introduction arcs

### Hub-and-Spoke

Central Situation completes → spawns multiple Situations simultaneously → all available at once → completing all unlocks convergence. Used for:
- Player agency moments with parallel paths
- Exploration branches
- Multi-threaded investigations

### Branching Consequences

Situation completes via success → spawns success path. Completes via failure → spawns failure path. Both continue story with different tone. Used for:
- High-stakes decisions with permanent impact
- Relationship forks
- Moral dilemmas

### Discovery Chain

Complete Situation at Location A → spawns Situation at newly revealed Location B → rewards thorough exploration. Used for:
- Secret area discovery
- Following clues to new locations
- World expansion

### Escalating Stakes

Complete easy Situation → spawns harder version with better rewards → player opts in to difficulty. Used for:
- Arena progression
- Risk/reward optimization
- Challenge towers

### Timed Cascade

Complete before deadline → spawns ideal path. Miss deadline → spawns degraded path. Both valid. Used for:
- Urgent quests with priority tension
- Time pressure gameplay
- Consequences for delays

### Reputation Threshold

Complete N situations of same category → spawns special situation. Rewards specialization. Used for:
- Faction progression
- Relationship depth
- Mastery recognition

### Converging Paths

Multiple independent situations → all must complete → spawns finale. Used for:
- Gather-the-party quests
- Investigation threading
- Prerequisite orchestration

### Mutually Exclusive Paths

Completing A prevents B from spawning. Permanent choice. Used for:
- Faction exclusivity
- Meaningful decisions with regret
- Replayability creation

### Multi-Situation Scene Composition Patterns

Scene archetypes compose multiple situations to create complete narrative arcs. Common composition patterns:

**Linear Progression Pattern:**
Situations cascade sequentially with forced progression. Scene defines N situations with linear transitions.

Structure:
- Situation 1 completes → Transition(Always) → Situation 2
- Situation 2 completes → Transition(Always) → Situation 3
- Situation 3 completes → Scene ends

Usage: Tutorial sequences, narrative arcs with clear beginning/middle/end, phased interactions requiring completion order.

**Branching Outcome Pattern:**
Situation completion routes to different next situations based on success/failure.

Structure:
- Situation 1 with challenge → OnSuccess → Situation 2A (success path)
- Situation 1 with challenge → OnFailure → Situation 2B (failure path)
- Both paths eventually converge or diverge permanently

Usage: High-stakes decisions, skill-gated progression, consequence management.

**Hub-and-Spoke Pattern:**
Central situation spawns multiple situations simultaneously, player chooses engagement order.

Structure:
- Hub situation completes → Spawns Situations A, B, C simultaneously
- Player pursues in any order
- Completing all three unlocks convergence situation

Usage: Investigation with parallel leads, multi-faceted problems, player agency moments.

**Choice-Conditional Transitions:**
Specific choice selection determines next situation (not just success/failure).

Structure:
- Situation 1 with multiple endings → OnChoice(choice_id_1) → Situation 2A
- Same situation → OnChoice(choice_id_2) → Situation 2B
- Different choices route to completely different continuations

Usage: Branching narratives, faction selection, mutually exclusive paths.

**Scene Completeness Criteria:**

Complete scene fulfills conceptual goal through situation sequence:
- Can player initiate the interaction? (Entry situation exists)
- Can player accomplish the goal? (Core situation(s) present)
- Can player exit cleanly? (Conclusion situation handles cleanup)
- Are resources properly managed? (Generated entities granted/removed)
- Does player have forward progress? (Fallback paths prevent soft-locks)

**Test:** Trace player action path from scene start to completion. If any situation leaves player with abandoned state (unreachable locations, undeletable items, perpetual flags), scene incomplete.

**Archetype Flexibility:**

Scene archetypes define STRUCTURE (how many situations, transition pattern) independent of DOMAIN (what fictional context). Same structural pattern applies to multiple domains:

- **Linear 3-phase:** Access negotiation → Primary activity → Cleanup/exit
  - Applies to: Services, resource acquisition, staged interactions
- **Branching 2-phase:** Decisive moment → Success/failure consequences
  - Applies to: Challenges, pivotal decisions, skill demonstrations
- **Hub 4-phase:** Investigation start → 3 parallel leads → Convergence conclusion
  - Applies to: Mysteries, complex problems, thorough examination

Situation archetypes provide MECHANICS (4 choices with PathType), Scene archetypes provide STRUCTURE (how situations connect), Entity properties provide SCALING (difficulty adjustment), AI provides NARRATIVE (contextually appropriate text).

Templates combine these primitives to create rich narrative structures. Content authors think in patterns, not specific instances.

---

## Part 11: Requirements and Availability

### CompoundRequirement Model

Choices have CompoundRequirements that determine availability. These are OR-of-ANDs structures:
- Path 1: Requirement A AND Requirement B
- Path 2: Requirement C AND Requirement D
- Path 3: Requirement E alone

Choice is available if ANY path fully satisfied. This creates multiple valid approaches.

### Requirement Types

**Resource Requirements:** Player has threshold Coins, Resolve, health, stamina

**Achievement Requirements:** Player has completed specific goals, investigations, milestones

**Relationship Requirements:** Player has bond level threshold with specific NPC

**State Requirements:** World is in specific state (time of day, location discovered, event occurred)

**Skill Requirements:** Player has demonstrated capability (won challenge type X times)

### Locked Choice Display

When CompoundRequirement not satisfied, Choice displays as locked. UI shows:
- Choice text grayed out
- Requirement paths displayed
- Current progress toward each path
- "You need X or Y or Z"

This maintains perfect information. Player knows exactly what's needed to unlock. They can plan progression toward unlocks.

### Strategic Costs vs Requirements

Requirements are gates (binary). You have it or you don't. Costs are consumption (graduated). You pay and have less.

Requirements determine availability. Costs determine affordability.

A Choice might:
- Require bond level 3 with NPC (gate)
- Cost 50 coins + 2 resolve (consumption)

Player must meet requirement before paying cost. Both are visible before commitment.

---

## Part 12: Time, Progression, and Pacing

### Time as Universal Cost

Every meaningful action costs time. Time is segmented resource:
- Day divided into blocks (Morning, Afternoon, Evening, Night)
- Blocks divided into segments
- Actions specify segment cost
- Advancing segments may change block

Time creates opportunity cost. Spending segments here means unavailable there. All actions compete for same time budget.

### Time Side Effects

When time advances, world reacts:
- NPC schedules update (NPCs move between locations)
- Obligation deadlines approach
- Scene expiration conditions evaluate
- World state changes (shops close, events begin)

GameFacade coordinates these through ProcessTimeAdvancement. It's the HIGHLANDER moment - only one system triggers time side effects, preventing duplicate or missed effects.

### Pacing Through Scarcity

Limited time forces prioritization. Player cannot do everything. Must choose:
- Pursue main story or side content?
- Optimize resources or explore?
- Rush urgent deadline or invest in preparation?

Impossible choices create strategic depth. Perfect information makes choices fair.

---

## Part 13: Save System Integration

### What Persists

**Templates:** Never saved. Loaded fresh each session from content packages.

**Active Scenes:** Full state including:
- Template reference (ID only)
- Current situation ID
- Placement binding
- Completion progress

**Completed Scenes:** Minimal data:
- Template ID
- Completion timestamp
- Key choices made (for conditionals)

**Provisional Scenes:** Not saved. Recreated when situation loads.

**Action Entities:** Not saved. Recreated from Situations on display.

**World Scaffolding:** Full state including:
- Location properties and discoveries
- NPC bonds and relationship history
- Route availability and knowledge

### Reconstruction

On load:
- Parse templates from content packages
- Restore world scaffolding
- Restore active scenes with template references
- Recreate current situation display
- Regenerate action entities from situations
- Evaluate requirements with current player state

This minimal save approach keeps file sizes small while maintaining full world state.

---
