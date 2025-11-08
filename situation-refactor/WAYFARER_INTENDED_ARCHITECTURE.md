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
- What choices are available within this narrative moment

**ChoiceTemplate** defines one player-facing decision within a situation. Following the Sir Brante pattern, each situation offers 2-4 choices. Each choice describes:
- What requirements must be met for this choice to be available versus locked
- What strategic costs the choice demands (Resolve, Coins, Time)
- What rewards the choice grants (resources, state changes, new scene spawns)
- What type of action this represents (instant effect, start challenge, navigation)

This hierarchy remains stable. Templates never change during gameplay. They are the design language authors use to define content patterns.

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
- If StartChallenge: Route player to appropriate tactical system (Social, Mental, Physical)
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
