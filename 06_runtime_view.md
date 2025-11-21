# Arc42 Section 6: Runtime View

## 6.1 Overview

This section describes the dynamic behavior of the Wayfarer system through key runtime scenarios. Each scenario shows how building blocks interact during execution to realize the architecture's design goals.

---

## 6.2 Procedural Scene Generation Flow (Four-Phase Lifecycle)

This scenario demonstrates how minimal JSON authoring transforms into fully-realized playable scenes through parse-time and runtime processing.

### Scenario Description

Content authors create minimal JSON specifying WHAT content should exist (secure lodging, negotiate with friendly innkeeper), not HOW it works mechanically. The system progressively enriches this minimal definition through four distinct phases spanning three timing tiers.

### Phase 1: Minimal JSON Authoring

**When**: Content creation time
**Who**: Content authors or AI generators
**What**: Authors write tiny SceneTemplate JSON

Content authors define scenes with minimal specification. Each scene template includes a unique template identifier, a reference to an archetype that defines mechanical patterns, a tier level for scaling, and categorical placement filters. These filters describe desired NPC characteristics (such as demeanor), location types, and quality levels. No individual situations, choices, costs, or rewards are specified at authoring time.

**Why Minimal Authoring**:
- No individual situations, choices, costs, or rewards specified
- Content volume stays manageable (10 scenes instead of 10 × 4 situations × 3 choices = 120 entries)
- Bug fixes propagate (fix negotiation pattern once, affects all instances)
- Authors specify WHAT (secure lodging) not HOW (4-choice negotiation mechanics)
- Catalogues encode HOW once, authors instantiate infinite variations

### Phase 2: Parse-Time Catalogue Expansion

**When**: Game startup (one-time cost during load screen)
**Who**: Static parsers + catalogues
**What**: Transform minimal JSON into complete SceneTemplate

The scene template parser receives minimal data transfer objects and initiates catalogue-based generation. The scene archetype catalogue processes the requested archetype type with placement filters, delegating to the situation archetype catalogue for mechanical pattern generation. The situation catalogue returns a complete situation template containing four choice templates: a stat-gated instant success path scaled by NPC demeanor, a money-gated instant success path scaled by quality level, a challenge path that initiates tactical sessions, and a guaranteed fallback option always available to players. The completed scene template is then stored in the game world's scene template collection for runtime instantiation.

**Why Parse-Time Not Runtime**:
- Catalogue generation expensive (context building, formula evaluation, reward routing)
- Per-spawn generation would cause gameplay lag
- One-time cost during load screen, zero cost during gameplay
- Reusable templates spawn instantly at runtime

**Context-Aware Scaling Example**:

Base archetype defines threshold and cost values. Applied context includes NPC demeanor (friendly reduces difficulty by forty percent), quality level (standard maintains baseline), and power dynamic (equal maintains baseline). Scaling calculations apply contextual multipliers to base values, resulting in easier negotiations with friendly NPCs and baseline costs for standard quality services.

### Phase 3: Spawn-Time Template Instantiation

**When**: Scene spawns (during gameplay via triggers or procedural generation)
**Who**: SceneInstantiator service
**What**: Convert immutable template into mutable Scene instance

The scene instantiator transforms immutable reusable templates into mutable scene instances through several transformation steps. First, logical markers for generated content are resolved to concrete globally unique identifiers, ensuring each spawn creates independent resources. Second, if AI narrative generation is enabled, contextual information including NPC names, personalities, and location names combine with template hints to generate specific narrative text. Third, entity placeholders in text are replaced with resolved entity names. The resulting scene instance begins in a deferred instantiation state where actions are not yet created, optimizing performance by deferring work until display time. The scene instance is stored in the game world's active scene collection.

**Why Deferred Action Creation**:
- Creating actions during spawn wastes work if scene never displayed
- Spawn happens during route travel (scene stored for later)
- Display happens when scene becomes active at location
- Actions only needed at display time
- Deferral: Spawn lightweight, display heavier but only when required

**Marker Resolution Purpose**:
- Templates can't hardcode resource IDs (reusable, need unique resources per spawn)
- Logical markers ("generated:private_room") resolved to concrete GUIDs at spawn
- First spawn: marker → GUID-1, second spawn: marker → GUID-2
- True instance isolation, no resource sharing between scene instances

### Phase 4: Query-Time Action Creation

**When**: UI queries for active scene content (player enters location, views NPC)
**Who**: SceneFacade + UI components
**What**: Lazy instantiation creates context-appropriate actions

When the player enters a location containing an active scene, the UI component requests action refresh. The scene facade retrieves actions available at the location and checks the current situation's instantiation state. If deferred, the system iterates through choice templates for the current situation, creating actions from each template. Each action receives a direct object reference to its parent situation and is added to the appropriate context-specific action collection. The instantiation state transitions to instantiated, and the resulting actions are returned to the UI for rendering as choice buttons displaying costs and rewards.

**Why Context-Appropriate Actions**:
- Same ChoiceTemplate spawns different action types based on placement
- Template at Location → LocationAction
- Same template at NPC → NPCAction
- Placement-agnostic design enables template reuse without special-casing

**Memory Efficiency**:
- Scene with 5 situations × 3 choices = 15 potential actions
- Only 3 actions exist at any time (current situation only)
- Thousands of template choices across all scenes
- Only instantiate subset player can currently access

---

## 6.3 Strategic Layer Execution (No Challenge)

This scenario shows pure strategic layer flow with instant choices that never cross to tactical layer.

### Scenario: Simple Purchase

**Flow**:

1. Player positioned at market location using object reference
2. Active scene at location has current situation (food purchase) in deferred instantiation state
3. UI queries for actions by passing location object reference, which creates actions from choice templates and transitions instantiation state to instantiated
4. UI displays choices with perfect information showing costs and paths: instant purchase, challenge negotiation, and fallback withdrawal
5. Player selects instant choice, which identifies as instant action type with instant success path
6. System evaluates requirements by checking resource availability, confirming player has sufficient coins
7. Costs applied immediately by deducting coins from player resources, with game world persisting state change
8. Rewards applied immediately by adding item to inventory and restoring hunger, with game world persisting state change
9. Situation marked complete
10. Scene evaluates transitions, checking situation spawn rules, determining no more situations exist, returning scene complete routing decision
11. Actions cleaned up by removing from location action collection and removing scene from game world
12. UI refreshes displaying updated resource values, choice no longer displayed

**Key Characteristics**:
- **Perfect Information**: All costs/rewards visible before commitment
- **Immediate Application**: Resources change instantly, no deferred resolution
- **Atomic Transaction**: Costs and rewards applied together
- **State Machine**: Scene advances deterministically
- **Clean Lifecycle**: Actions created → displayed → executed → deleted

---

## 6.4 Strategic-Tactical Bridge (Challenge Flow)

This scenario demonstrates crossing from strategic layer (perfect information) to tactical layer (hidden complexity) and returning with outcomes.

### Scenario: Negotiate with Challenge

**STRATEGIC LAYER - CHOICE SELECTION**

1. Player positioned at inn location with active lodging negotiation scene and active service negotiation situation
2. UI displays four choices: instant payment path, stat-gated persuasion path requiring minimum rapport, challenge negotiation path, and fallback common room path
3. Player selects challenge choice which defines properties including challenge initiation action type, challenge path type, social challenge category, specific deck identifier, success rewards unlocking private room, and failure penalty charging extra coins
4. System evaluates requirements, finding none (challenge paths always accessible)
5. Entry costs applied by deducting stamina from player resources

**BRIDGE CROSSING**

6. Pending challenge context stored containing object references to parent scene, parent situation, success reward template, and failure reward template
7. Tactical victory conditions extracted from current situation's threshold cards, defining momentum threshold of eight points with coin and understanding rewards
8. Challenge session created by social facade, loading specified negotiation deck and initializing session resources (starting initiative, zero momentum building toward threshold, zero opposition doubt, measured pacing cadence)
9. Navigation to tactical UI occurs, displaying NPC portrait, available cards, session resources, and binary actions

**TACTICAL LAYER - CHALLENGE SESSION**

10. Player executes tactical turns: first turn plays friendly remark card increasing momentum and opposition doubt, second turn performs speak action building understanding and advancing conversation, third turn plays sympathy appeal increasing momentum and reducing doubt, fourth turn plays diplomatic offer exceeding momentum threshold
11. Threshold achievement recognized, situation card marked achieved, rewards applied including coins and understanding increases
12. Challenge concludes with success outcome, temporary session destroyed

**BRIDGE RETURN**

13. System returns to strategic layer, retrieving pending challenge context
14. Conditional rewards applied based on success outcome: private room unlocked by removing locked state, success message generated
15. Scene advances to next situation requiring private room location, context comparison determines different location required, routing decision directs exit to world for player navigation

**STRATEGIC LAYER - CONTINUATION**

16. Player navigates to unlocked private room location
17. Next situation auto-activates when scene facade detects context match, instantiation state transitions from deferred to instantiated, actions created from choice templates
18. UI displays new choices: extended rest option, short nap option, immediate departure option

**Key Bridge Mechanics**:
- **One-Way Flow**: Strategic spawns tactical, tactical returns outcome
- **Pending Context**: OnSuccess/OnFailure rewards stored, applied after challenge
- **Victory Conditions**: SituationCards define thresholds, extracted when challenge spawns
- **Temporary Sessions**: Challenge session created and destroyed, not persisted
- **Conditional Rewards**: Different outcomes based on challenge success/failure
- **Hidden Complexity**: Strategic layer shows costs before entry, tactical layer hides card draw order

---

## 6.5 Context Activation and Auto-Progression

This scenario demonstrates seamless multi-situation progression without artificial navigation friction.

### Scenario: Service Flow with Auto-Activation

**INITIAL STATE**

Scene contains four situations in sequence: service negotiation in common room, private room entry, rest in room, and room departure. Player begins in common room with first situation active.

**SITUATION 1: NEGOTIATION**

1. Context matching occurs when scene facade checks activation, comparing situation's required location against player's current location, finding match, activating situation
2. Actions instantiated and displayed to player
3. Player completes negotiation via instant success path paying coins, rewards applied unlocking private room location by removing locked state
4. Scene advances to next situation requiring private room, context comparison determines current and required locations differ, routing decision directs exit to world for manual navigation
5. UI displays guidance directing player to navigate to private room

**NAVIGATION TRANSITION**

6. Player executes navigation action to private room, player's current location updated, game world persists state change
7. Location change automatically triggers activation check, evaluating all active scenes against new player context

**SITUATION 2: AUTO-ACTIVATION**

8. Context matching finds situation's required location matches player's current location, situation auto-activates without player action
9. Actions instantiated immediately by scene facade creating actions from choice templates, instantiation state transitions from deferred to instantiated
10. UI updates automatically displaying three choices: extended rest, short nap, and room examination

**SITUATION 3: SEAMLESS CASCADE**

11. Player selects room examination instant action with no costs, rewards provide descriptive message, scene advances to next situation
12. Context comparison for next situation finds required and current location match (same room), routing decision returns continue in scene
13. Immediate auto-activation occurs seamlessly with no navigation required, no continuation prompt, next situation's actions appearing immediately, UI transitioning without interruption
14. UI displays updated rest choices, previous examination choice removed, new rest choices appearing instantly

**SITUATION 4: CLEANUP**

15. Player rests until morning, time advances eight hours, resources restored, scene advances to next situation at same location, continue in scene decision triggers auto-activation
16. Departure choices appear offering careful departure and rushed departure options with different cleanup consequences
17. Player departs, key removed from inventory, room locked preventing re-entry, scene advances finding no more situations, scene marked complete, scene removed from game world

**Auto-Activation Mechanics**:
- **Context Matching**: PlacementFilter (categorical properties) matched against player context (NO hardcoded entity IDs)
- **Automatic Trigger**: No explicit player action needed when context matches
- **Seamless Flow**: ContinueInScene enables instant progression within same context
- **Manual Navigation**: ExitToWorld requires player to navigate when context changes
- **Progressive Disclosure**: Choices appear/disappear as situations activate/complete

**Activation Requirement Patterns**:
- **Location + NPC**: Both must match (service negotiation at specific NPC in specific location)
- **Location Only**: NPC null or ignored (private room rest, solo activities)
- **NPC Only**: Location optional (traveling merchants, roaming characters)

---

## 6.6 Entity Resolution and Scene Spawning Lifecycle

This scenario shows complete lifecycle of scene spawning with entity resolution using the 5-system architecture.

### Scenario: Lodging Scene with Private Room

**PHASE 1: SCENE SELECTION (System 1 - Decision Logic)**

Player executes choice containing scene spawn reward identifying secure lodging template with no placement filter override. Scene facade evaluates eligibility by checking spawn conditions on template against player state, verifying required tags present and minimum day threshold met, determining scene eligible for spawning.

**PHASE 2: SCENE SPECIFICATION (System 2 - Data Structure)**

Scene spawn reward structure contains only categorical properties: template identifier with no concrete entity identifiers, no placement relation enumeration, no context binding. Template defines placement requirements via categorical placement filters.

**PHASE 3: PACKAGE GENERATION (System 3 - SceneInstantiator)**

Scene instantiator writes categorical filters describing desired entities without resolving to concrete instances. Location filter specifies indoor private safe properties with lodging tags and closest selection strategy. NPC filter specifies innkeeper or merchant personality types with neutral demeanor and least recently used selection strategy. Situation definitions reference locations and NPCs through null placeholders awaiting resolution. No entity resolution occurs yet, no concrete identifiers written, package prepared for entity resolution phase.

**PHASE 4: ENTITY RESOLUTION (System 4 - EntityResolver)**

Package loader invokes entity resolver with categorical filters. Location resolver queries existing locations matching all required properties and tags, returning existing location if found, otherwise generating new location with specified properties and adding to game world, using eager creation strategy. NPC resolver follows same pattern querying by personality type and demeanor, returning existing NPC or generating new one. Private room resolver queries for sublocation matching parent location with private property and guest room tag, creating new guest room if none found. Resolution produces pre-resolved entity objects (main location, innkeeper NPC, private room location) ready for scene instantiation.

**PHASE 5: SCENE INSTANTIATION (System 5 - SceneParser)**

Scene parser receives pre-resolved entity objects and constructs scene instance with direct object references. Scene contains location object property and NPC object property, no identifier strings or enumerations. Situations contain required location and required NPC as direct object references. Scene added to game world. AI narrative generation occurs after resolution using concrete entity names in context, generating complete narrative text with no placeholders or markers.

**PHASE 6: GAMEPLAY (Runtime - Situation Activation)**

Player navigates to main location using object reference. Scene facade checks activation comparing situation's required location against player's current location via object equality, finding match, auto-activating negotiation situation. Player views negotiation with NPC showing four choice paths. Player executes negotiation choice, rewards unlock private room by removing locked state. Player navigates to guest room, activation check finds context match via object equality, rest situation auto-activates displaying rest choices.

**PHASE 7: PERSISTENCE (Entity Lifecycle)**

All resolved entities persist in game world permanently. Main location, innkeeper NPC, and private room location persist beyond scene completion. Scene removed when completed while entities remain. Future scene spawns reuse existing entities through categorical matching, allowing entity accumulation over time with no cleanup system.

**Lifecycle Summary**:

Parse phase declares content and creates entities with globally unique identifiers. Spawn phase resolves logical markers and replaces placeholders with concrete references. Runtime phase grants access through unlock operations, allows resource usage, and removes access through lock operations.

**Key Principles**:
- **Resources exist throughout**: Location persists in GameWorld
- **Access controlled**: IsLocked property gates usage
- **Scene-scoped**: Each scene spawn generates independent resources
- **No re-entry**: Locked locations inaccessible without new unlock
- **Clean separation**: Parse-time creation, runtime access control

---

## 6.7 Data Flow Patterns

### Complete Pipeline Flow

```
┌─────────────────────────────────────────────┐
│ JSON Files (Content Definition)            │
│ - scenes.json, npcs.json, locations.json   │
└────────────────┬────────────────────────────┘
                 │ Parse at startup
                 ▼
┌─────────────────────────────────────────────┐
│ Static Parsers (Conversion Layer)          │
│ - SceneParser, NPCParser, LocationParser   │
│ - Catalogues translate categorical→concrete│
└────────────────┬────────────────────────────┘
                 │ Create domain entities
                 ▼
┌─────────────────────────────────────────────┐
│ Domain Models (Strongly Typed Objects)     │
│ - Scene, NPC, Location entities            │
│ - Concrete properties (int, bool, etc.)   │
└────────────────┬────────────────────────────┘
                 │ Populate collections
                 ▼
┌─────────────────────────────────────────────┐
│ GameWorld (Single Source of Truth)         │
│ - Scenes, NPCs, Locations collections      │
│ - Player state                             │
└────────────────┬────────────────────────────┘
                 │ Read/Write by services
                 ▼
┌─────────────────────────────────────────────┐
│ Service Facades (Business Logic)           │
│ - GameFacade, SceneFacade, SocialFacade    │
│ - Stateless operations on GameWorld        │
└────────────────┬────────────────────────────┘
                 │ Create context objects
                 ▼
┌─────────────────────────────────────────────┐
│ Context Objects (Operation State)          │
│ - SocialChallengeContext                   │
│ - LocationScreenViewModel                  │
└────────────────┬────────────────────────────┘
                 │ Pass to UI
                 ▼
┌─────────────────────────────────────────────┐
│ UI Components (Display Layer)              │
│ - GameScreen.razor (authoritative parent)  │
│ - Child components (content only)          │
└────────────────┬────────────────────────────┘
                 │ User interaction
                 ▼
┌─────────────────────────────────────────────┐
│ User Actions (Button clicks, navigation)   │
└────────────────┬────────────────────────────┘
                 │ Call facades
                 ▼
┌─────────────────────────────────────────────┐
│ Service Facades (State Updates)            │
│ - Execute business logic                   │
│ - Update GameWorld state                   │
└────────────────┬────────────────────────────┘
                 │ State persisted
                 ▼
┌─────────────────────────────────────────────┐
│ GameWorld (Updated State)                  │
└─────────────────────────────────────────────┘
```

### Request/Response Flow

**User Action → UI Response**:

1. User clicks play card button triggering UI event handler
2. Conversation content component handles click event
3. Component invokes parent game screen's card play handler passing card identifier
4. Game screen delegates to game facade's conversation card method
5. Game facade delegates to social facade's card play method
6. Social facade executes business logic validating card play requirements, applying card effects increasing momentum and doubt, updating session state, checking threshold achievement
7. Social facade updates game world by incrementing current session's momentum and doubt values
8. Social facade returns result to game facade
9. Game facade returns result to game screen
10. Game screen signals state change to Blazor framework
11. Blazor re-renders component tree based on updated state
12. Conversation content displays updated resource values showing momentum and doubt increases with card removed from hand

### Context Creation Pattern

**Complex Operations Use Dedicated Contexts**:

Complex operations avoid passing multiple individual parameters. Instead, dedicated context objects encapsulate all data needed for an operation. Context objects contain NPC information, location details, player resource state, tactical session data, available action options, and victory condition thresholds.

The facade creates the context atomically before any operation begins, gathering all required data in a single consistent snapshot. The context is then passed as a single strongly-typed parameter to components and services. This ensures atomic creation, consistent state snapshots, clean parameter lists, type safety, and improved testability through easy context mocking.

**Why Context Objects**:
- **Atomic Creation**: All data gathered in one operation
- **Consistent State**: Snapshot of game state at creation time
- **Clean Parameters**: Single parameter instead of many
- **Type Safety**: Strongly typed properties
- **Testability**: Mock entire context easily

---

## 6.8 Related Documentation

- **04_solution_strategy.md** - Strategic decisions enabling these runtime patterns
- **05_building_block_view.md** - Static structure of components shown in dynamic behavior
- **08_crosscutting_concepts.md** - Patterns and principles used throughout
- **03_context_and_scope.md** - System boundaries and gameplay loops
