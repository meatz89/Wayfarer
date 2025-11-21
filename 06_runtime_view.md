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

```json
{
  "id": "inn_lodging_001",
  "archetypeId": "service_negotiation",
  "tier": "basic",
  "placementFilter": {
    "npcDemeanor": "Friendly",
    "locationType": "Inn",
    "quality": "Standard"
  }
}
```

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
```
Base archetype: StatThreshold = 5, CoinCost = 8

Applied context:
- NPCDemeanor: Friendly (0.6× difficulty multiplier)
- Quality: Standard (1.0× cost multiplier)
- PowerDynamic: Equal (1.0× multiplier)

Scaled results:
- StatThreshold: 5 × 0.6 = 3 (friendly = easier negotiation)
- CoinCost: 8 × 1.0 = 8 (standard = baseline cost)
```

### Phase 3: Spawn-Time Template Instantiation

**When**: Scene spawns (during gameplay via triggers or procedural generation)
**Who**: SceneInstantiator service
**What**: Convert immutable template into mutable Scene instance

```
Template (immutable, reusable)
    ↓ SceneInstantiator.SpawnFromTemplate()
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
Scene instance (mutable, specific)
  - InstantiationState = Deferred
  - Actions NOT created yet
  - Stored in GameWorld.Scenes
```

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
  - Set Action.ParentSituation = CurrentSituation  // Object reference, NOT ID
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

```
1. Player at market location
   GameWorld.Player.CurrentLocation = marketLocation  // Object reference, NOT ID string

2. Active Scene at location with current situation
   Scene.CurrentSituation = Buy Food Situation
   Situation.InstantiationState = Deferred

3. UI queries for actions
   SceneFacade.GetActionsAtLocation(marketLocation)  // Pass object, NOT ID string
   → Creates actions from ChoiceTemplates
   → Situation.InstantiationState = Instantiated

4. UI displays choices with perfect information
   Choice 1: "Buy bread (5 coins)" - Instant path
   Choice 2: "Haggle for discount" - Challenge path
   Choice 3: "Walk away" - Fallback path

5. Player selects instant choice
   GameFacade.ExecuteChoice(choiceId)
   → ChoiceTemplate.ActionType = Instant
   → ChoiceTemplate.PathType = InstantSuccess

6. System evaluates requirements
   Check: Player.Coins >= 5 ? YES
   → Proceed with execution

7. Apply costs immediately
   Player.Coins -= 5
   GameWorld persists state change

8. Apply rewards immediately
   Player.Inventory.Add("bread")
   Player.Hunger += 3
   GameWorld persists state change

9. Mark situation complete
   Situation.IsCompleted = true

10. Evaluate scene transitions
    Scene.AdvanceToNextSituation()
    → Check SituationSpawnRules.Transitions
    → No more situations: Return SceneRoutingDecision.SceneComplete

11. Clean up actions
    Delete actions from GameWorld.LocationActions
    Scene removed from GameWorld.Scenes

12. UI refreshes
    LocationContent shows updated resources
    Choice no longer displayed
```

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

```
STRATEGIC LAYER - CHOICE SELECTION
──────────────────────────────────

1. Player at inn location
   Scene: "inn_lodging_friendly"
   CurrentSituation: "negotiate_service" (active)

2. UI displays choices
   Choice 1: "Pay 15 coins" - InstantSuccess path
   Choice 2: "Persuade innkeeper (Rapport 5+)" - InstantSuccess path (stat-gated)
   Choice 3: "Negotiate diplomatically" - Challenge path ← PLAYER SELECTS
   Choice 4: "Sleep in common room" - Fallback path

3. Player selects Challenge choice
   ChoiceTemplate properties:
   - ActionType: StartChallenge
   - PathType: Challenge
   - ChallengeType: Social
   - ChallengeId: "inn_negotiation_deck"
   - OnSuccessReward: {LocationsToUnlock: ["generated:private_room"]}
   - OnFailureReward: {Coins: -5, Message: "You pay extra for subpar room"}

4. System evaluates requirements
   Requirements: None (challenge path always accessible)

5. Apply entry costs
   Choice.ResourceCosts: {Stamina: -2}
   Player.Stamina -= 2

BRIDGE CROSSING
───────────────

6. Store pending context
   PendingChallengeContext:
   - ParentSceneId: "inn_lodging_friendly"
   - ParentSituationId: "negotiate_service"
   - OnSuccessReward: template reference
   - OnFailureReward: template reference

7. Extract tactical victory conditions
   CurrentSituation.SituationCards:
   [
     {
       threshold: 8,
       resourceType: "Momentum",
       rewards: {Coins: +10, Understanding: +2}
     }
   ]

8. Create challenge session
   SocialFacade.StartConversation(npcId, challengeId)
   → Load challenge deck: "inn_negotiation_deck"
   → Initialize session resources:
      - Initiative: 3
      - Momentum: 0 (builds toward threshold)
      - Doubt: 0 (opposition resource)
      - Cadence: "Measured"

9. Navigate to tactical UI
   GameScreen.CurrentScreen = ConversationContent
   UI displays: NPC portrait, cards, resources, binary actions

TACTICAL LAYER - CHALLENGE SESSION
──────────────────────────────────

10. Player plays cards and performs actions
    Turn 1: Play "Friendly Remark" card
            → Momentum +2 (now 2/8)
            → Doubt +1 (opposition rises)

    Turn 2: Perform SPEAK action
            → Build Understanding (+1)
            → Advance conversation

    Turn 3: Play "Appeal to Sympathy" card
            → Momentum +3 (now 5/8)
            → Doubt -1 (opposition reduces)

    Turn 4: Play "Diplomatic Offer" card
            → Momentum +4 (now 9/8) ← THRESHOLD EXCEEDED

11. Threshold reached
    SituationCard.IsAchieved = true
    Apply SituationCard.Rewards:
    - Coins +10
    - Understanding +2

12. Challenge ends with success
    ChallengeOutcome: Success
    Session destroyed (temporary entity)

BRIDGE RETURN
─────────────

13. Return to strategic layer
    GameScreen.CurrentScreen = LocationContent
    Retrieve PendingChallengeContext

14. Apply conditional rewards
    ChallengeOutcome = Success
    → Apply OnSuccessReward:
       - LocationsToUnlock: ["generated:private_room"]
       - Location.IsLocked = false
       - Message: "Elena hands you the room key with a smile"

15. Advance scene
    Scene.AdvanceToNextSituation()
    → Next situation: "enter_private_room"
    → Context: RequiredLocationId = resolved room GUID
    → SceneRoutingDecision: ExitToWorld (different location required)

STRATEGIC LAYER - CONTINUATION
──────────────────────────────

16. Player navigates to unlocked room
    Player.CurrentLocationId = resolved room GUID

17. Next situation auto-activates
    SceneFacade detects context match
    → CurrentSituation: "enter_private_room"
    → InstantiationState: Deferred → Instantiated
    → Create actions from ChoiceTemplates

18. UI displays new choices
    Choice 1: "Rest until morning (8 hours)"
    Choice 2: "Take short nap (2 hours)"
    Choice 3: "Leave immediately"
```

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

```
INITIAL STATE
─────────────

Scene: "inn_lodging_negotiation"
Situations:
1. "negotiate_service" (RequiredLocationId: "inn_common_room")
2. "enter_private_room" (RequiredLocationId: "generated:private_room")
3. "rest_in_room" (RequiredLocationId: "generated:private_room")
4. "depart_room" (RequiredLocationId: "generated:private_room")

Player.CurrentLocationId = "inn_common_room"
Scene.CurrentSituation = "negotiate_service"

SITUATION 1: NEGOTIATION
────────────────────────

1. Context matches (player at inn_common_room)
   SceneFacade.CheckActivation()
   → Situation.RequiredLocationId = "inn_common_room"
   → Player.CurrentLocationId = "inn_common_room"
   → MATCH: Situation activates

2. Actions instantiated and displayed

3. Player completes negotiation (instant success, pays coins)
   → Rewards applied: LocationsToUnlock: ["generated:private_room"]
   → Location.IsLocked = false

4. Scene advances
   Scene.AdvanceToNextSituation()
   → CurrentSituation = "enter_private_room"
   → CompareContexts:
      - Current: "inn_common_room"
      - Required: "generated:private_room" (resolved GUID)
      - DIFFERENT: Return SceneRoutingDecision.ExitToWorld

5. Player must manually navigate
   UI shows: "You received a room key. Go to your private room."

NAVIGATION TRANSITION
─────────────────────

6. Player clicks navigation action
   GameFacade.NavigateToLocation(resolvedRoomId)
   → Player.CurrentLocationId = resolvedRoomId
   → GameWorld persists state

7. Location change triggers activation check
   SceneFacade.CheckActivation() (called automatically)
   → Active scenes checked against new context

SITUATION 2: AUTO-ACTIVATION
────────────────────────────

8. Context now matches
   → Situation.RequiredLocationId = resolvedRoomId
   → Player.CurrentLocationId = resolvedRoomId
   → MATCH: Situation auto-activates (no player action needed)

9. Actions instantiated immediately
   SceneFacade creates actions from ChoiceTemplates
   → InstantiationState: Deferred → Instantiated

10. UI updates automatically
    LocationContent displays:
    Choice 1: "Rest until morning (8 hours)"
    Choice 2: "Take short nap (2 hours)"
    Choice 3: "Examine room first"

SITUATION 3: SEAMLESS CASCADE
─────────────────────────────

11. Player selects "Examine room first"
    → Instant action, no costs
    → Rewards: Message about room description
    → Scene.AdvanceToNextSituation()

12. Context comparison for next situation
    → Next: "rest_in_room" (RequiredLocationId: same room)
    → Current: same room
    → MATCH: Return SceneRoutingDecision.ContinueInScene

13. Immediate auto-activation (seamless cascade)
    → NO navigation required
    → NO "click to continue" button
    → Next situation's actions appear immediately
    → UI seamlessly transitions to new choices

14. UI now displays rest choices
    Choice 1: "Rest until morning (8 hours)"
    Choice 2: "Take short nap (2 hours)"
    → Previous examination choice disappeared
    → New rest choices appeared instantly

SITUATION 4: CLEANUP
───────────────────

15. Player rests until morning
    → Time advances 8 hours
    → Resources restored
    → Scene.AdvanceToNextSituation()
    → Next: "depart_room" (same location)
    → ContinueInScene: Auto-activates

16. Departure choices appear
    Choice 1: "Leave carefully" (ItemsToRemove: [key], LocationsToLock: [room])
    Choice 2: "Rush out" (same cleanup, Stamina cost)

17. Player departs
    → Key removed from inventory
    → Room.IsLocked = true (no re-entry)
    → Scene.AdvanceToNextSituation()
    → No more situations: SceneComplete
    → Scene removed from GameWorld
```

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

```
PHASE 1: SCENE SELECTION (System 1 - Decision Logic)
─────────────────────────────────────────────────────

Player executes choice with SceneSpawnReward:

SceneSpawnReward {
  SceneTemplateId: "secure_lodging",
  PlacementFilterOverride: null  // Use template's filter
}

SceneFacade.IsSceneEligible():
  - Check SpawnConditions on template
  - RequiredTags: ["in_town"] → Player has tag ✓
  - MinDay: 1 → Player.CurrentDay = 3 ✓
  - Eligible → Proceed to System 2

PHASE 2: SCENE SPECIFICATION (System 2 - Data Structure)
─────────────────────────────────────────────────────────

SceneSpawnReward structure (categorical only):

SceneSpawnReward {
  SceneTemplateId: "secure_lodging",
  // NO concrete entity IDs
  // NO PlacementRelation enum
  // NO ContextBinding
  // Categorical properties ONLY
}

Template defines placement requirements via PlacementFilter.

PHASE 3: PACKAGE GENERATION (System 3 - SceneInstantiator)
───────────────────────────────────────────────────────────

SceneInstantiator writes categorical filters to JSON:

SceneDTO {
  Id: "scene_guid_12345",
  TemplateId: "secure_lodging",

  // Categorical filters (NOT concrete IDs)
  LocationFilter: {
    LocationProperties: ["Indoor", "Private", "Safe"],
    LocationTags: ["lodging", "secure"],
    SelectionStrategy: "Closest"
  },

  NpcFilter: {
    PersonalityTypes: ["Innkeeper", "Merchant"],
    NpcDemeanor: "Neutral",
    SelectionStrategy: "LeastRecentlyUsed"
  },

  Situations: [
    {
      Id: "negotiate_lodging",
      RequiredLocationId: null,  // Will reference resolved location
      RequiredNpcId: null,       // Will reference resolved NPC
      Choices: [...]
    },
    {
      Id: "rest_in_room",
      RequiredLocationId: null,  // Will reference resolved private room
      RequiredNpcId: null,
      Choices: [...]
    }
  ]
}

→ NO entity resolution yet
→ NO concrete IDs written
→ Package ready for System 4

PHASE 4: ENTITY RESOLUTION (System 4 - EntityResolver)
───────────────────────────────────────────────────────

PackageLoader calls EntityResolver with filters:

EntityResolver.FindOrCreateLocation(LocationFilter):
  // STEP 1: Query existing entities
  existing = GameWorld.Locations.Where(loc =>
    loc.Properties.Contains("Indoor") &&
    loc.Properties.Contains("Private") &&
    loc.Properties.Contains("Safe") &&
    loc.Tags.Contains("lodging")
  ).FirstOrDefault();

  if (existing != null)
    return existing;  // Reuse "The Silver Hart Inn"

  // STEP 2: Generate new entity if no match
  generated = GenerateLocation(LocationFilter);
  // Location { Id: "location_guid_456", Name: "The Golden Rest", Properties: [...] }
  GameWorld.AddOrUpdateLocation(generated);
  return generated;  // Eager creation

EntityResolver.FindOrCreateNPC(NpcFilter):
  // Same pattern: query existing, generate if needed
  existing = GameWorld.NPCs.Where(npc =>
    npc.PersonalityType == "Innkeeper" &&
    npc.Demeanor == "Neutral"
  ).FirstOrDefault();

  return existing ?? GenerateNPC(NpcFilter);
  // Returns: Elena (existing NPC object)

EntityResolver.FindOrCreateLocation(PrivateRoomFilter):
  // Scene needs private room sublocation
  privateRoom = GameWorld.Locations.Where(loc =>
    loc.ParentLocationId == mainLocation.Id &&
    loc.Properties.Contains("Private") &&
    loc.Tags.Contains("guest_room")
  ).FirstOrDefault();

  if (privateRoom == null) {
    privateRoom = GeneratePrivateRoom(mainLocation);
    // Location { Id: "location_guid_789", Name: "Guest Room", ParentLocationId: "location_guid_456" }
    GameWorld.AddOrUpdateLocation(privateRoom);
  }

  return privateRoom;  // Object reference

Result: Pre-resolved entity objects ready for System 5
  - mainLocation = Location object (The Silver Hart Inn)
  - innkeeper = NPC object (Elena)
  - privateRoom = Location object (Guest Room)

PHASE 5: SCENE INSTANTIATION (System 5 - SceneParser)
──────────────────────────────────────────────────────

SceneParser receives pre-resolved objects:

Scene scene = new Scene {
  Id: "scene_guid_12345",
  TemplateId: "secure_lodging",

  // Direct object references (NO IDs, NO enums)
  Location: mainLocation,  // Object property
  Npc: innkeeper,          // Object property

  Situations: [
    new Situation {
      Id: "negotiate_lodging",
      RequiredLocation: mainLocation,  // Direct object reference
      RequiredNpc: innkeeper,          // Direct object reference
      Choices: [...]
    },
    new Situation {
      Id: "rest_in_room",
      RequiredLocation: privateRoom,  // Direct object reference
      RequiredNpc: null,
      Choices: [...]
    }
  ]
};

GameWorld.AddScene(scene);

AI NARRATIVE GENERATION (After Resolution):
  - Entities already resolved: Elena (innkeeper), The Silver Hart Inn (location)
  - AI receives entity context: { npc: "Elena", location: "The Silver Hart Inn", room: "Guest Room" }
  - AI generates complete narrative: "Negotiate lodging with Elena at The Silver Hart Inn"
  - NO placeholders, NO markers
  - Complete text generated with concrete entity names

PHASE 6: GAMEPLAY (Runtime - Situation Activation)
───────────────────────────────────────────────────

Player navigates to The Silver Hart Inn:
  Player.CurrentLocation = mainLocation  // Object reference

SceneFacade.CheckActivation():
  Scene.CurrentSituation.RequiredLocation == mainLocation ✓
  Player.CurrentLocation == mainLocation ✓
  → Situation "negotiate_lodging" auto-activates

Player sees:
  - "Negotiate lodging with Elena"
  - Four choices (stat/money/challenge/fallback)

Player executes negotiation choice:
  Choice.Reward.LocationsToUnlock: [privateRoom.Id]
  → privateRoom.IsLocked = false

Player navigates to Guest Room:
  Player.CurrentLocation = privateRoom  // Object reference

SceneFacade.CheckActivation():
  Scene.CurrentSituation.RequiredLocation == privateRoom ✓
  → Situation "rest_in_room" auto-activates

Player sees:
  - "Rest in your private room"
  - Rest/sleep choices

PHASE 7: PERSISTENCE (Entity Lifecycle)
────────────────────────────────────────

All entities persist in GameWorld:
  - mainLocation (The Silver Hart Inn) persists forever
  - innkeeper (Elena) persists forever
  - privateRoom (Guest Room) persists forever
  - Scene removed when completed, entities remain

Future spawns:
  - New scene needs innkeeper → FindOrCreate finds Elena → reuse
  - New scene needs lodging → FindOrCreate finds Silver Hart Inn → reuse
  - Entities accumulate in GameWorld over time
  - No cleanup system (entities never deleted)
```

**Lifecycle Summary**:
```
Parse:   Declare → Create entities with GUIDs
Spawn:   Resolve markers → Replace placeholders
Runtime: Grant access → Use resources → Remove access
```

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

```csharp
// WRONG: Passing multiple parameters
StartConversation(string npcId, string requestId, Player player,
                  Location location, List<SocialCard> availableCards)

// CORRECT: Single context parameter
SocialChallengeContext context = await GameFacade.CreateConversationContext(npcId, requestId);

// Context contains ALL data needed for operation
public class SocialChallengeContext {
    public NPCInfo NpcInfo { get; set; }           // NPC data
    public LocationInfo LocationInfo { get; set; }  // Current location
    public PlayerResources PlayerResources { get; set; } // Resource state
    public ConversationSession Session { get; set; } // Tactical session
    public List<SocialCardViewModel> AvailableCards { get; set; } // Playable cards
    public List<SituationCard> VictoryConditions { get; set; } // Thresholds
}

// Context created atomically BEFORE navigation
ConversationContext context = await GameFacade.CreateConversationContext(npcId, requestId);

// Context passed as single parameter to child component
<ConversationContent Context="@context" />
```

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
