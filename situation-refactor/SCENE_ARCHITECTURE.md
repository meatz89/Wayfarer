# Wayfarer: Intended Architecture - Complete Conceptual Model

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
- What choices are available within this narrative moment (2-4 ChoiceTemplates, Sir Brante pattern)

**ChoiceTemplate** defines one player-facing decision within a situation. Each choice template describes:
- What requirements must be met for this choice to be available versus locked
- What strategic costs the choice demands (Resolve, Coins, Time)
- What rewards the choice grants (resources, state changes, new scene spawns)
- What type of action this represents (instant effect, start challenge, navigation)

This hierarchy remains stable. Templates never change during gameplay. They are the design language authors use to define content patterns.

**Important:** ChoiceTemplate is NOT instantiated into a generic "Choice" entity. Instead, when the Situation activates (player enters location/conversation/route), ChoiceTemplates become LocationActions, NPCActions, or PathCards depending on the Situation's placement context.

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
- Contains references to ChoiceTemplates from its SituationTemplate (NOT runtime choice entities)
- When Situation activates (player enters context), these ChoiceTemplates are instantiated into action entities

**There is NO runtime "Choice" entity.** Instead, ChoiceTemplates are instantiated into context-specific action entities based on where the Situation exists.

This hierarchy is compositional. Scenes own Situations. Situations reference their templates' ChoiceTemplates. The parent Scene's lifecycle controls Situation lifecycle.

---

### The Action Entity Layer

When a Situation switches from dormant to active (which happens when player enters the location/conversation/route), its ChoiceTemplates are instantiated into concrete action entities. These action entities ARE the runtime choices - there is no separate "Choice" entity.

**LocationAction, NPCAction, and PathCard are the three types of instantiated choices.** Which type gets created depends on the Situation's placement context:

**LocationAction** is created when:
- Situation exists at a Location (Scene.PlacementType = Location)
- Player enters that location
- Situation switches from dormant to active
- Each ChoiceTemplate in the Situation becomes a LocationAction
- Stored in GameWorld.LocationActions (flat collection)

**NPCAction** is created when:
- Situation exists with an NPC (Scene.PlacementType = NPC)
- Player opens conversation with that NPC
- Situation switches from dormant to active
- Each ChoiceTemplate in the Situation becomes an NPCAction
- Stored in GameWorld.NPCActions (flat collection)

**PathCard** is created when:
- Situation exists on a Route (Scene.PlacementType = Route)
- Player begins traveling that route
- Situation switches from dormant to active
- Each ChoiceTemplate in the Situation becomes a PathCard
- Stored in GameWorld.PathCards (flat collection)

**These are NOT wrappers around choices. They ARE the choices.** They:
- Reference their ChoiceTemplate (composition) for requirements, costs, and rewards
- Reference their parent Situation for context
- Store runtime state (has provisional scene ID if spawns new scene)
- Are the entities that executors validate and GameFacade executes

**Lifecycle:** Action entities are ephemeral. Created when Situation activates (player enters context). Cleaned up when Situation advances or Scene completes. They do not persist in save files - they're regenerated from Situations when needed.

---

### World Scaffolding

The game world contains permanent entities that exist independently of scenes:

**Location** represents a physical place with:
- Venue type (inn, market square, residential district)
- Atmospheric description that sets tone
- Tags that make it eligible for certain scene placements
- List of NPCs currently present
- List of routes departing to other locations

**NPC** represents a person with:
- Portrait and visual identity
- Personality type that affects scene spawns
- Bond level with player
- Current location
- Conversational history

**Route** represents a path between locations with:
- Terrain type affecting travel difficulty
- Distance determining time cost
- Environmental hazards
- Currently active events

These entities always exist. Scenes spawn onto them dynamically but never modify their fundamental existence.

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

When a player views a Situation, its ChoiceTemplates are instantiated into action entities (LocationActions, NPCActions, or PathCards depending on placement context). For any ChoiceTemplate that includes scene spawn rewards, the game immediately creates a provisional version of that Scene. This provisional Scene is a mechanical skeleton:
- Shows the Scene's archetype (Social, Investigation, Combat)
- Shows WHERE it would spawn (at which Location, with which NPC, on which Route)
- Shows WHEN it would appear (immediately, after delay, after prerequisite)

The provisional Scene does NOT show:
- Specific narrative text
- Exact choices within that future Scene
- Tactical challenge details

The player sees strategic information: "This choice spawns a Social Scene at the Inn with the Innkeeper". They can evaluate: "Do I want a social scene there? Do I have time? Do I have resources? What opportunity am I giving up?"

### Lifecycle

**Creation:** When Situation activates (player enters location/conversation/route) and instantiates ChoiceTemplates into action entities, any ChoiceTemplate with scene spawn rewards triggers provisional scene creation. The SceneInstantiator creates the Scene with state set to Provisional. The action entity receives the provisional Scene's ID. The provisional Scene is stored in a separate collection from active Scenes.

**Display:** UI queries provisional scenes by ID from the action entity and displays preview information in the action button/card. Player sees strategic context before committing.

**Finalization:** When player executes the action entity (LocationAction/NPCAction/PathCard), GameFacade finalizes the provisional Scene. Placeholders are replaced with concrete entity names. Narrative intro is generated. State changes to Active. Scene moves from provisional collection to active scenes collection.

**Deletion:** When player executes ANY action entity from the Situation, all OTHER action entities' provisional Scenes are deleted. Those futures didn't happen in this timeline. Memory is freed.

### Benefits

Players make informed strategic decisions. They accept opportunity costs knowingly. They plan multi-step paths through the narrative. They understand what they're committing to before acting.

Game designers can create branching content without hand-holding. Players discover naturally through exploration backed by perfect strategic information.

Action entities (LocationActions, NPCActions, PathCards) serve as the bridge between ChoiceTemplates and execution, holding references to provisional Scenes and enabling the perfect information display.

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
Based on the action entity's ChoiceTemplate ActionType:
- If Instant: Apply rewards immediately and complete
- If StartChallenge: Route player to appropriate tactical system (Social, Mental, Physical)
- If Navigate: Apply navigation payload to move player

**Step 6: Scene Lifecycle**  
If the executed action entity (LocationAction/NPCAction/PathCard) had a provisional Scene:
- Finalize that Scene (replace placeholders, generate narrative, change state to Active)
- Move it from provisional collection to active scenes collection

For all OTHER action entities from the same Situation:
- Delete their provisional Scenes from the provisional collection

**Step 7: Reward Application**  
Apply the action entity's ChoiceTemplate rewards through ConsequenceFacade. This may grant resources, change player state, modify NPC bonds, or trigger achievement unlocks.

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

## Part 5: Content Pipeline and Dynamic Generation

### The Unified Parser Approach

All content enters the game through the same pipeline regardless of source:

**Input:** JSON files containing templates (SceneTemplates, SituationTemplates, ChoiceTemplates)

**Parsing:** PackageLoader reads JSON, invokes specialized parsers for each template type, validates structure, stores in GameWorld's template collections

**Instantiation:** SceneInstantiator creates runtime entities from templates when spawn conditions trigger

This pipeline is source-agnostic. Whether content was hand-authored by designers or generated by AI makes no difference. Both use identical JSON structure. Both flow through identical parsers. Both instantiate identically.

### Starter Content

Certain SceneTemplates are marked as "starter" content. During game initialization, GameWorldInitializer:
- Queries all SceneTemplates with starter flag set
- Calls SceneInstantiator to create Active scenes from each
- Places these scenes according to their PlacementFilters
- Populates the initial world with playable content

Starter content bootstraps the narrative. It gives players initial actions at key locations. It establishes the world's baseline state.

### Dynamic Spawning

After initialization, new Scenes spawn from player actions. When an action entity (LocationAction/NPCAction/PathCard) executes with scene spawn rewards in its ChoiceTemplate:
- GameFacade receives the reward specification (which template, what placement relation, any delay)
- Creates provisional Scene showing player where it would appear
- On commitment, finalizes Scene and makes it active
- New content seamlessly integrates into existing world

This creates cascading narratives. One action spawns a scene. Completing that scene spawns more scenes. The story grows organically from player decisions.

### AI Integration Point

AI-generated content enters through the same parser pipeline:

**Process:**
- AI generates JSON matching exact template structure
- Game loads JSON through existing PackageLoader
- Parsers validate and store new templates
- Templates become available for spawning
- No code changes needed

**Quality Control:**
- AI follows schema constraints enforced by parsers
- PlacementFilters ensure spawns make narrative sense
- Template structure prevents AI from creating broken content
- Validation catches malformed generation

**Scope:**
- AI generates narrative text, choice options, requirement formulas
- AI does NOT generate code or game mechanics
- AI works within design-defined systems and patterns
- Human designers define the pattern language, AI fills patterns with content

This separation means designers can iterate on game systems without retraining AI. AI can generate infinite content variations without breaking game systems.

---

## Part 6: Scene Lifecycle and State Management

### Scene States

Scenes progress through defined lifecycle states:

**Provisional State** exists when a Scene is created as consequence preview. The Scene has:
- Structural skeleton (template reference, placement, embedded situations)
- Placeholder text not yet finalized
- Not yet playable
- Visible to player in UI previews
- Stored separately from active scenes

**Active State** exists when a Scene becomes playable. The Scene has:
- Finalized narrative text with placeholders replaced
- Fully instantiated Situations and Choices
- Current situation tracking
- Available for player interaction
- Stored in main scene collection

**Completed State** exists when a Scene finishes. The Scene has:
- All relevant situations finished
- No longer displayed in location/NPC/route UI
- Archived for save file persistence
- Still referenced for history and conditionals

### State Transitions

**Provisional → Active** occurs when:
- Player selects the action entity (LocationAction/NPCAction/PathCard) that created this provisional Scene
- GameFacade calls SceneInstantiator to finalize
- Placeholders replaced, narrative generated, state changed
- Scene moved from provisional to active collection

**Active → Completed** occurs when:
- Final situation in scene finishes
- OR scene expiration condition met (time limit, world state change)
- OR scene explicitly terminated by special choice

**Provisional → Deleted** occurs when:
- Player selects ANY other action entity from same Situation
- GameFacade calls SceneInstantiator to delete
- Memory freed, no trace remains

### Situation Progression

Within an Active Scene, Situations advance according to spawn rules defined in template:

**Linear Progression:** Complete Situation A, Situation B spawns at same placement. Complete B, C spawns. Creates guided narrative arc.

**Hub-and-Spoke:** Complete initial Situation, multiple follow-up Situations spawn simultaneously. Player chooses order. All must complete to reach finale.

**Branching:** Success and failure paths lead to different follow-up Situations. Both valid, both continue story, different tones.

**Cascade Patterns:** Templates define complex flows combining these primitives.

GameFacade coordinates situation transitions. When a Situation completes, GameFacade:
- Consults Scene's spawn rules
- Determines which Situation(s) spawn next
- Updates Scene's current situation tracking
- Instantiates new Situation if appropriate
- Marks Scene as complete if no more situations

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

---

## Part 8: The World Model

### Permanent Scaffolding

The game world exists independently of narrative content. Locations, NPCs, and Routes form permanent structure. They:
- Define the world's geography
- Provide personality and flavor
- Establish relationships and proximity
- Create navigation possibilities

This scaffolding persists across save/load. It's the stage on which scenes perform.

### Ephemeral Opportunities

Scenes are temporary. They spawn, play out, complete, despawn. At any moment:
- Some locations have active scenes
- Some locations have no scenes (atmospheric only)
- Some NPCs have conversations available
- Some NPCs are uninteresting currently

This creates pacing and discovery. The world feels alive because not everything is always available. Finding new content feels meaningful because it wasn't guaranteed.

### Scene-World Binding

When a Scene spawns, the SceneInstantiator:
- Evaluates PlacementFilter against current world state
- Finds eligible entities (Locations matching tags, NPCs matching personality)
- Selects best fit based on:
  - Current player location (prefer nearby)
  - Entity availability (not already hosting conflicting scene)
  - Narrative coherence (personality match, relationship history)
  - Design priority hints in filter
- Binds Scene to selected entity with PlacementType + PlacementId

Once bound, Scene exists at that placement until completed. If player visits, they encounter the Scene. If player doesn't visit, Scene may expire based on template rules.

### No Scene State

When no Scene is active at a placement:
- Location displays atmospheric properties only
- UI shows "There's nothing pressing here right now"
- Player can still navigate away, rest, or perform universal actions
- World continues existing

This is intentional design, not missing content. Quiet moments are valid. Not every location needs story beats simultaneously.

---

## Part 9: Integration Patterns

### UI to Strategic Layer

When player enters a location/opens conversation/begins route travel:
- LocationFacade returns persistent wrapper (WHERE you are)
- SceneFacade queries GameWorld.Scenes for active Scenes at this placement
- If Situations found, they switch from dormant to active
- SceneFacade instantiates ChoiceTemplates into action entities (LocationActions/NPCActions/PathCards)
- For action entities with SceneSpawnRewards, provisional Scenes created immediately
- SceneFacade evaluates requirements, categorizes as available/locked
- Returns query result to UI

UI then:
- Combines LocationFacade wrapper with SceneFacade content
- Displays unified location/conversation/route view
- Presents action entities as buttons/cards
- Captures player clicks as intent
- Calls GameFacade execution methods with action entity IDs

UI does NOT:
- Directly modify game state
- Apply costs or rewards
- Manage scene lifecycle
- Create action entities (SceneFacade does this)

UI is presentation and intent capture only. Action entity instantiation happens in SceneFacade before UI rendering.

### Strategic to Tactical Layer

When an action entity's ChoiceTemplate has ActionType of StartChallenge, GameFacade routes player to tactical systems:
- Social challenges for conversation/persuasion
- Mental challenges for investigation/deduction
- Physical challenges for combat/athletics

Routing includes challenge configuration:
- Which deck to use
- Win/loss conditions
- Resource pools (Focus, Stamina)
- Success/failure consequence divergence

Tactical systems are independent gameplay modes with their own UI. They resolve to success or failure. Result returns to GameFacade which applies appropriate consequences.

### Tactical to Strategic Layer

When tactical challenge completes:
- Result (success or failure) returns to GameFacade
- GameFacade consults the action entity's ChoiceTemplate reward structure
- Applies success consequences or failure consequences
- Advances Situation or completes Scene as appropriate
- Returns control to strategic navigation UI

The boundary is clean. Tactical systems don't know about Scenes or Situations. They're black-box challenge resolution systems.

---

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

Templates combine these primitives to create rich narrative structures. Content authors think in patterns, not specific instances.

---

## Part 11: Requirements and Availability

### CompoundRequirement Model

ChoiceTemplates have CompoundRequirements that determine availability when instantiated into action entities. These are OR-of-ANDs structures:
- Path 1: Requirement A AND Requirement B
- Path 2: Requirement C AND Requirement D
- Path 3: Requirement E alone

An action entity is available if ANY path fully satisfied. This creates multiple valid approaches.

### Requirement Types

**Resource Requirements:** Player has threshold Coins, Resolve, health, stamina

**Achievement Requirements:** Player has completed specific goals, investigations, milestones

**Relationship Requirements:** Player has bond level threshold with specific NPC

**State Requirements:** World is in specific state (time of day, location discovered, event occurred)

**Skill Requirements:** Player has demonstrated capability (won challenge type X times)

### Locked Action Display

When CompoundRequirement not satisfied, the action entity (LocationAction/NPCAction/PathCard) displays as locked. UI shows:
- Action text grayed out
- Requirement paths displayed
- Current progress toward each path
- "You need X or Y or Z"

This maintains perfect information. Player knows exactly what's needed to unlock. They can plan progression toward unlocks.

### Strategic Costs vs Requirements

Requirements are gates (binary). You have it or you don't. Costs are consumption (graduated). You pay and have less.

Requirements determine availability. Costs determine affordability.

A ChoiceTemplate might define:
- Require bond level 3 with NPC (gate)
- Cost 50 coins + 2 resolve (consumption)

When instantiated into an action entity, player must meet requirement before paying cost. Both are visible before commitment.

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

**Action Entities:** Not saved. Recreated from Situations when they activate (player enters context).

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

## Part 14: Design Philosophy

### Elegance Through Constraint

Strong typing, clear ownership, explicit relationships aren't limitations. They're quality filters. When design requires workarounds, the design is wrong.

Good architecture makes correct paths easy and incorrect paths hard. It guides toward maintainable solutions.

### Systems Over Content

Content is infinite. Systems are precious. Design systems that enable infinite content variation without code changes. Let authors and AI fill systems with content.

### Perfect Information Without Spoilers

Strategic layer shows consequences. Tactical layer hides execution. This split enables informed decision-making while preserving surprise and challenge.

### Failure as Content

Failure states should continue story with different tone, not block progress. Players learn through consequences, not through retries.

### Player Agency Through Scarcity

Agency comes from impossible choices, not from doing everything. Limited resources force prioritization. Perfect information makes prioritization fair.

---

## Part 15: Implementation Priorities

### Phase 1: Foundation
- Create entity models (Scene with embedded Situations)
- Create action entity models (LocationAction, NPCAction, PathCard with ChoiceTemplate composition)
- Create template models (SceneTemplate, SituationTemplate, ChoiceTemplate)
- Implement SceneInstantiator with provisional state support
- Update GameWorld collections

### Phase 2: Execution Pipeline
- Implement three executors as pure validators
- Update GameFacade with unified Execute methods
- Wire action entity instantiation
- Connect UI to GameFacade execution

### Phase 3: Content Pipeline
- Update parsers for template structure
- Implement PlacementFilter evaluation
- Wire starter scene initialization
- Test scene spawning from choices

### Phase 4: Display Integration
- Update SceneFacade with query methods
- Integrate provisional scene display in UI
- Show locked choices with requirement paths
- Display strategic costs clearly

### Phase 5: Polish
- Implement spawn pattern cascade logic
- Add time side effect coordination
- Complete save/load integration
- Test end-to-end flows

---

## Conclusion

This architecture provides foundation for deep strategic gameplay with infinite narrative variation. Templates enable AI content generation. Provisional state enables perfect information. Clean execution flow enables maintainability.

Every principle serves game design goals:
- Strategic depth through informed choice
- Tactical challenge through skill execution
- Infinite content through template variation
- Maintainable codebase through clean architecture

Implementation should follow this conceptual model. When conflicts arise, refer to principles. Architecture should guide toward correct solutions.

The world is persistent scaffolding. Scenes are ephemeral opportunities. Players navigate strategic choices with perfect information. Templates enable infinite variation. GameFacade orchestrates all execution.

This is the intended state. Build toward this vision.