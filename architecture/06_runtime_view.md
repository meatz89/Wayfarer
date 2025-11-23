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

Content authors write minimal scene template definitions containing only essential categorical properties. A typical template specifies an identifier, an archetype reference that determines behavioral patterns, a difficulty tier, and a placement filter describing contextual requirements. The placement filter uses categorical properties like character demeanor, location type, and quality level to describe where and how the scene should manifest, without naming specific entities or locations.

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

```
SceneTemplateParser receives minimal DTO
    ↓ calls
SceneArchetypeCatalogue.Generate("service_negotiation", placementFilter)
    ↓ calls
SituationArchetypeCatalogue.Generate("negotiation", contextProperties)
    ↓ returns
Complete SituationTemplate with 4 ChoiceTemplates:
  - Stat-gated instant success (scaled by NPCDemeanor)
  - Money-gated instant success (scaled by Quality)
  - Challenge path (starts tactical session)
  - Guaranteed fallback (always available)
    ↓
SceneTemplate stored in GameWorld.SceneTemplates
```

**Why Parse-Time Not Runtime**:
- Catalogue generation expensive (context building, formula evaluation, reward routing)
- Per-spawn generation would cause gameplay lag
- One-time cost during load screen, zero cost during gameplay
- Reusable templates spawn instantly at runtime

**Context-Aware Scaling Example**:

Catalogues apply contextual multipliers to base archetype values during generation. A base archetype might define a stat threshold of five and a coin cost of eight. When applied to a friendly character context, the system reduces difficulty by applying a sixty percent multiplier, lowering the stat threshold to three. Quality levels affect cost multipliers, with standard quality maintaining baseline pricing at one-to-one. Power dynamics between characters also influence these multipliers. The result is mechanically identical situations that feel appropriately scaled to their narrative context—negotiating with a friendly character is measurably easier than with a hostile one.

### Package-Round Entity Tracking Pattern

**Core Principle**: Initialize ONLY entities from THIS package load round, never re-process existing entities.

**Implementation Pattern**:

Package loading returns explicit tracking structure containing entities added during this specific load operation. Each load round creates a PackageLoadResult object tracking venues, locations, characters, items, routes, and scenes added during that package's processing. Spatial initialization methods receive explicit entity lists as parameters instead of querying GameWorld collections. This ensures each entity initializes exactly once during its originating package round.

```
LoadPackageContent(package)
    ↓
Create PackageLoadResult
    ↓
Parse entities from JSON:
  - Location parsed → add to GameWorld.Locations
  - ALSO add to result.LocationsAdded (track THIS round)
    ↓
Return PackageLoadResult
    ↓
Spatial initialization:
  PlaceLocations(result.LocationsAdded)
  - Receives ONLY new locations
  - NO GameWorld queries
  - NO entity state checks
```

**Two Loading Scenarios**:

**Static Loading (Game Startup)**:
- Load multiple packages (00_base.json, 01_locations.json, etc.)
- Accumulate PackageLoadResult from each package
- Aggregate all results: `allLocations = results.SelectMany(r => r.LocationsAdded)`
- Initialize spatial systems ONCE with aggregated lists
- Performance: O(n) where n = total entities across all packages

**Dynamic Loading (Runtime Scene Activation)**:
- Load single package (deferred scene resources)
- Get PackageLoadResult for this package only
- Initialize spatial systems IMMEDIATELY with result lists
- Performance: O(m) where m = new entities from this package only

**Why Package-Round Tracking Matters**:

Without package-round tracking, spatial initialization methods would iterate entire GameWorld collections, re-scanning all existing entities every time ANY package loads. With package-round tracking, methods receive only new entities from the current load operation. This provides constant-time package loading (independent of GameWorld size) and prevents duplicate processing through architectural enforcement rather than entity state checks.

**Architectural Enforcement**:

The pattern makes violations impossible by construction. Methods cannot re-process existing entities because they never receive them as parameters. No entity state checks needed (`HexPosition == null`) because only uninitialized entities flow through the pipeline. Package isolation guaranteed because each PackageLoadResult contains only its own entities. HIGHLANDER principle enforced: one entity initialized exactly once during its originating package round.

**Related**: See ADR-015 for complete architectural decision rationale and alternatives considered.

### Phase 3a: Deferred Scene Creation (Two-Phase Spawning)

**When**: Scene spawns (game startup for IsStarter scenes, runtime for reward scenes)
**Who**: SceneInstantiator.CreateDeferredScene()
**What**: Create Scene + Situations WITHOUT dependent resources

```
Template (immutable, reusable)
    ↓ SceneInstantiator.CreateDeferredScene()
    ↓
Generate SceneDTO:
  - State = "Deferred"
  - Situations embedded (with null Location/NPC references)
  - NO dependent locations or items yet
    ↓
Write package JSON to disk (Dynamic folder)
    ↓
PackageLoader loads package:
  - Creates Scene entity with State=Deferred
  - Creates embedded Situation entities
  - NO location placement yet
  - NO item creation yet
    ↓
Scene stored in GameWorld.Scenes (State=Deferred)
```

**Why Two-Phase Spawning**:
- Scene creation separate from resource spawning (lazy loading)
- Startup scenes created as Deferred (lightweight initialization)
- Dependent resources only spawn when player enters location
- Prevents duplicate location placement (HIGHLANDER principle)
- Clear separation: Domain entities early, content entities late

**Phase 3a Result**:
- Scene entity exists in GameWorld.Scenes
- Situations exist as embedded entities
- State = Deferred
- NO dependent locations placed yet
- NO dependent items created yet
- Awaiting activation trigger

### Phase 3b: Scene Activation (Resource Spawning)

**When**: Player enters location containing deferred scene
**Who**: LocationFacade.CheckAndActivateDeferredScenes()
**What**: Spawn dependent resources and transition State to Active

```
Player navigates to Location
    ↓
LocationFacade.MoveToSpot() succeeds
    ↓
Check for deferred scenes at location:
  Query GameWorld.Scenes where State=Deferred
  Filter by CurrentSituation.Location == targetLocation
    ↓ Found deferred scene(s)
SceneInstantiator.ActivateScene(scene, context)
    ↓
Generate dependent resource DTOs:
  - DependentLocationSpec → LocationDTO
  - DependentItemSpec → ItemDTO
    ↓
Marker Resolution:
  "generated:private_room" → "location_guid_12345"
  "generated:room_key" → "item_guid_67890"
    ↓
AI Narrative Generation (if enabled):
  Context: {npcName: "Elena", npcPersonality: "warm", locationName: "The Silver Hart Inn"}
  Template hints: "transactional negotiation, friendly tone"
  Generated: "Elena smiles warmly as you approach the counter..."
    ↓
Entity Placeholder Replacement:
  {npcName} → "Elena"
  {locationName} → "The Silver Hart Inn"
    ↓
Write resource package JSON to disk
    ↓
PackageLoader loads resources:
  - Creates Location entities
  - Places locations on hex grid
  - Creates Item entities
    ↓
Scene.State = Active (State transition: Deferred → Active)
```

**Why Activation on Location Entry**:
- HIGHLANDER: Single entry point for scene activation
- Player "enters" location → scenes activate (narrative trigger)
- Dependent resources spawn exactly once (no idempotence checks needed)
- Clear lifecycle: Deferred → Active → Completed/Expired

**Marker Resolution Purpose**:
- Templates can't hardcode resource IDs (reusable, need unique resources per spawn)
- Logical markers ("generated:private_room") resolved to concrete GUIDs at activation
- First spawn: marker → GUID-1, second spawn: marker → GUID-2
- True instance isolation, no resource sharing between scene instances

### Phase 4: Query-Time Action Creation

**When**: UI queries for active scene content (player enters location, views NPC)
**Who**: SceneFacade + UI components
**What**: Lazy instantiation creates context-appropriate actions

```
Player enters location containing active Scene
    ↓
UI: LocationContent.razor calls RefreshActions()
    ↓
SceneFacade.GetActionsAtLocation(locationId)
    ↓
Check scene.CurrentSituation.InstantiationState
    ↓ if Deferred
Iterate CurrentSituation.ChoiceTemplates:
  - Create Action from ChoiceTemplate
  - Set Action.SituationId = CurrentSituation.Id
  - Add to GameWorld.LocationActions (for location context)
    ↓
Set InstantiationState = Instantiated
    ↓
Return actions to UI for rendering
    ↓
UI: Displays choice buttons with costs/rewards visible
```

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

The player begins at a market location with the game world tracking their current position. An active scene exists at this location containing a current situation in deferred instantiation state. When the UI queries for available actions at the market, the scene facade creates action objects from choice templates and transitions the situation to instantiated state. The UI displays three choices with perfect information: purchasing bread for five coins as an instant success path, haggling for a discount as a challenge path, and walking away as a fallback option.

When the player selects the instant purchase choice, the game facade executes the choice by first identifying it as an instant action type with instant success path designation. The system evaluates requirements by checking whether the player possesses at least five coins, confirming eligibility and proceeding with execution. Costs apply immediately by deducting five coins from the player's resources, with the game world persisting this state change. Rewards follow immediately by adding bread to the player's inventory and increasing hunger satisfaction by three points, again persisting to the game world.

The system marks the situation as completed and evaluates scene transitions by advancing to the next situation. The scene checks situation spawn rules and transition conditions, determining that no further situations exist and returning a scene complete routing decision. Cleanup occurs by deleting the action objects from the location's action collection and removing the completed scene from the game world's scene collection. Finally, the UI refreshes to display updated resource values, with the completed choice no longer appearing as an option.

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

**Strategic Layer - Choice Selection**

The player arrives at an inn location with an active lodging scene containing a current negotiate service situation. The UI displays four choice options: paying fifteen coins as an instant success path, persuading the innkeeper using rapport stat as a gated instant success path requiring minimum stat level, negotiating diplomatically as a challenge path, and sleeping in the common room as a fallback path. The player selects the diplomatic negotiation challenge option.

The selected choice template contains properties indicating it starts a challenge action of social type, references a specific negotiation card deck, defines success rewards that unlock a private room location, and specifies failure consequences including coin loss and a message about paying extra. The system evaluates entry requirements, finding none since challenge paths remain always accessible. Entry costs apply immediately by deducting two stamina points from the player's resources.

**Bridge Crossing**

The system stores pending challenge context containing references to the parent scene and situation, along with both success and failure reward templates for later application. It extracts tactical victory conditions from the current situation's card collection, finding a threshold of eight momentum points that when achieved grants coin and understanding rewards. The social facade creates a challenge session by loading the appropriate card deck and initializing session resources: three initiative points, zero momentum building toward the threshold, zero doubt as opposition resource, and measured cadence pacing. The game screen navigates to tactical UI displaying the character portrait, available cards, resource meters, and binary action options.

**Tactical Layer - Challenge Session**

The player engages in turn-based tactical play over multiple turns. First turn involves playing a friendly remark card that increases momentum by two points and raises opposition doubt by one. Second turn uses the speak action to build understanding and advance conversation state. Third turn plays an appeal to sympathy card adding three momentum points and reducing doubt by one. Fourth turn plays a diplomatic offer card adding four momentum points, exceeding the eight-point threshold.

Threshold achievement triggers situation card completion, applying rewards of ten coins and two understanding points. The challenge ends with success outcome and the temporary session entity is destroyed.

**Bridge Return**

Control returns to strategic layer as the game screen switches back to location content view, retrieving the stored pending challenge context. The system applies conditional rewards based on success outcome by unlocking the private room location, setting its locked property to false, and displaying a message about receiving the room key. Scene advancement proceeds to the next enter private room situation, comparing context requirements and determining that a different location is required, returning an exit to world routing decision instructing the player to navigate.

**Strategic Layer - Continuation**

The player manually navigates to the unlocked room, updating their current location reference. The next situation auto-activates as the scene facade detects matching context between required location and player location, transitioning instantiation state from deferred to instantiated and creating action objects from choice templates. The UI displays new choices for resting until morning over eight hours, taking a short two-hour nap, or leaving immediately.

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

**Initial State**

A lodging negotiation scene contains four sequential situations: negotiating service at the inn common room, entering the private room after unlocking, resting in the private room, and departing the private room. The player's current location references the inn common room and the scene's current situation is set to negotiate service.

**Situation 1: Negotiation**

The scene facade checks activation and finds matching context between the player's current location at the common room and the situation's required location, causing the situation to activate. Actions instantiate and display for player interaction. The player completes negotiation through instant success by paying coins, which applies rewards that unlock the private room location by setting its locked property to false. Scene advancement proceeds to the next enter private room situation, comparing contexts and finding the current common room differs from the required private room location, returning an exit to world routing decision. The player must manually navigate as the UI displays a message about receiving the room key and instructions to go to the private room.

**Navigation Transition**

The player clicks a navigation action that calls the game facade to navigate to the resolved room identifier, updating the player's current location and persisting state to the game world. Location change automatically triggers an activation check that evaluates all active scenes against the new context.

**Situation 2: Auto-Activation**

Context now matches as the situation's required location equals the player's current location at the private room, causing the situation to auto-activate without requiring explicit player action. The scene facade immediately creates action objects from choice templates, transitioning instantiation state from deferred to instantiated. The UI updates automatically to display three choices: resting until morning for eight hours, taking a short two-hour nap, or examining the room first.

**Situation 3: Seamless Cascade**

The player selects the examine room option, an instant action with no costs that provides a message reward describing the room and advances the scene to the next situation. Context comparison for the next rest in room situation finds the required location matches the current location at the same private room, returning a continue in scene routing decision. Immediate auto-activation occurs seamlessly without navigation requirements or continuation prompts—the next situation's actions appear immediately as the UI transitions to new choices. The previous examination choice disappears while rest choices for morning or short nap appear instantly in its place.

**Situation 4: Cleanup**

The player rests until morning, advancing time by eight hours and restoring resources, then the scene advances to the depart room situation at the same location which auto-activates via continue in scene routing. Departure choices appear offering careful departure that removes the key item and locks the room location, or rushing out with the same cleanup but additional stamina cost. The player departs, triggering key removal from inventory and setting the room's locked property to true preventing re-entry. Scene advancement finds no more situations, returning scene complete status and removing the scene from the game world's collection.

**Auto-Activation Mechanics**:
- **Context Matching**: RequiredLocationId/RequiredNpcId compared to player context
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

**Phase 1: Scene Selection (System 1 - Decision Logic)**

The player executes a choice containing a scene spawn reward that references a secure lodging template without placement filter overrides, defaulting to the template's own filter. The scene facade checks eligibility by evaluating spawn conditions on the template, verifying the player possesses required tags like being in town and meets minimum day requirements, confirming eligibility and proceeding to the next system.

**Phase 2: Scene Specification (System 2 - Data Structure)**

The scene spawn reward structure contains only the template identifier with categorical properties, explicitly avoiding concrete entity identifiers, placement relation enumerations, or context binding specifications. The template itself defines all placement requirements through its placement filter specification.

**Phase 3: Package Generation (System 3 - SceneInstantiator)**

The scene instantiator writes categorical filters to a data transfer object structure that includes a unique scene identifier and template reference. Location filtering specifies categorical properties like indoor, private, and safe along with tags for lodging and security, using a closest selection strategy. Character filtering defines personality types such as innkeeper or merchant with neutral demeanor, employing a least recently used selection strategy. Embedded situations contain identifiers with null location and character references awaiting resolution. No entity resolution occurs yet—the package contains only categorical filters ready for the resolver system.

**Phase 4: Entity Resolution (System 4 - EntityResolver)**

The package loader invokes the entity resolver with categorical filters to find or create required entities. Location resolution first queries existing entities in the game world's location collection, filtering by properties like indoor, private, and safe along with lodging tags. Finding an existing location returns that object for reuse. When no match exists, the system generates a new location with unique identifier and name, adds it to the game world, and returns the new object through eager creation.

Character resolution follows the same pattern, querying existing characters by personality type and demeanor, returning existing objects when found or generating new ones when needed. Private room resolution queries for sublocations with the main location as parent, containing private properties and guest room tags, generating new sublocation entities when necessary.

The resolver returns pre-resolved entity objects—the main location object, the character object, and the private room object—all ready for direct use by the scene parser.

**Phase 5: Scene Instantiation (System 5 - SceneParser)**

The scene parser receives pre-resolved objects and constructs a scene entity with direct object references rather than identifiers or enumerations. The scene contains location and character properties referencing actual objects. Embedded situations reference required locations and characters through direct object properties, establishing activation context without string lookups. The complete scene adds to the game world's collection.

AI narrative generation occurs after resolution with entities already determined, receiving entity context containing concrete names. The AI generates complete narrative text like negotiating lodging with the specific character at the specific location, avoiding placeholders and markers entirely by using actual resolved entity names.

**Phase 6: Gameplay (Runtime - Situation Activation)**

The player navigates to the main location, updating their current location object reference. The scene facade checks activation by comparing the situation's required location object against the player's current location object, finding equality and auto-activating the negotiation situation. The player sees choices for negotiating lodging with the character, including stat, money, challenge, and fallback options.

Executing the negotiation choice applies rewards that unlock the private room by setting its locked property to false. The player navigates to the private room, updating their current location reference. Activation checking finds the new situation's required location matches the player's current location, auto-activating the rest situation. The player sees rest and sleep choices for the private room.

**Phase 7: Persistence (Entity Lifecycle)**

All resolved entities persist indefinitely in the game world collection. The main location, character, and private room remain available permanently while the scene itself is removed upon completion. Future scene spawns that require similar entities trigger the find-or-create pattern, reusing existing entities like the character or location when filters match. Entities accumulate in the game world over time with no cleanup system removing them.

**Lifecycle Summary**:

The entity lifecycle progresses through three stages. Parse-time declares entities and creates them with unique identifiers. Spawn-time resolves categorical markers and replaces placeholders with concrete objects. Runtime grants access through unlocking, permits resource usage, and revokes access through locking.

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
```
1. User clicks "Play Card" button
   ↓
2. ConversationContent.razor handles @onclick
   ↓
3. Component calls GameScreen.HandleCardPlay(cardId)
   ↓
4. GameScreen calls GameFacade.PlayConversationCard(cardId)
   ↓
5. GameFacade delegates to SocialFacade.PlayCard(cardId)
   ↓
6. SocialFacade executes business logic:
   - Validate card play requirements
   - Apply card effects (Momentum +2, Doubt +1)
   - Update session state
   - Check threshold achievement
   ↓
7. SocialFacade updates GameWorld:
   - GameWorld.GetPlayer().CurrentSession.Momentum += 2
   - GameWorld.GetPlayer().CurrentSession.Doubt += 1
   ↓
8. SocialFacade returns result to GameFacade
   ↓
9. GameFacade returns result to GameScreen
   ↓
10. GameScreen calls StateHasChanged()
    ↓
11. Blazor re-renders component tree
    ↓
12. ConversationContent displays updated resources:
    - Momentum: 5 → 7
    - Doubt: 2 → 3
    - Card removed from hand
```

### Context Creation Pattern

**Complex Operations Use Dedicated Contexts**:

Complex operations avoid passing multiple individual parameters by instead creating dedicated context objects that encapsulate all required data. Rather than methods accepting separate parameters for character identifier, request identifier, player object, location object, and available card collection, the system creates a single context object through a facade method. Context classes contain strongly-typed properties for all operation requirements: character information, location information, player resources, tactical session state, available card view models, and victory condition thresholds.

The facade creates contexts atomically before navigation occurs, gathering all necessary data in one operation. Child components receive the complete context as a single parameter rather than multiple individual values.

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
