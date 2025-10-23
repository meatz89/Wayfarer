# Scene/Card Architecture Analysis: From Goal/Obstacle/Obligation to Dynamic Content Creation

## Executive Summary

This document analyzes the current Goal/Obstacle/Obligation entity architecture and designs the complete Scene/Card replacement architecture based on DYNAMIC_CONTENT_PRINCIPLES.md. The transformation shifts from **content unlocking** (check conditions → filter → present) to **content creation** (actions spawn scenes → resources gate entry → scenes consumed/expired).

**Core Insight**: The current architecture treats content as pre-existing libraries awaiting unlock conditions. The new architecture treats content as dynamically spawned consequences of player actions.

---

## Part 1: Current Architecture Deep Analysis

### 1.1 Goal Entity - Complete Property Inventory

**Purpose**: Tactical challenge approach to overcome obstacles

**All Properties (19 total)**:

```
IDENTITY:
- string Id
- string Name
- string Description

TACTICAL SYSTEM CONFIGURATION:
- TacticalSystemType SystemType (Social/Mental/Physical)
- string DeckId (which challenge deck to use)
- string Category (for Social system matching)
- ConnectionType ConnectionType (token type for Social system)

PLACEMENT CONTEXT (NOT OWNERSHIP):
- string PlacementLocationId (where button appears)
- string PlacementNpcId (where button appears)
- string PlacementRouteId (where button appears)
- Location PlacementLocation (runtime reference)
- NPC PlacementNpc (runtime reference)

OWNERSHIP HIERARCHY:
- string ObligationId (parent obligation for UI grouping)
- Obligation Obligation (runtime reference to parent)
- Obstacle ParentObstacle (runtime reference to container)

STATE TRACKING:
- bool IsIntroAction (special obligation intro flag)
- GoalStatus Status (Available/Completed)
- bool IsAvailable (visibility flag)
- bool IsCompleted (completion flag)
- bool DeleteOnSuccess (remove on completion vs persist)

RESOURCE COSTS:
- GoalCosts Costs (Time, Focus, Stamina, Coins)

DIFFICULTY MODIFIERS:
- List<DifficultyModifier> DifficultyModifiers (graduated difficulty reduction)

VICTORY CONDITIONS:
- List<GoalCard> GoalCards (inline tactical victory conditions)

CONSEQUENCE SYSTEM:
- ConsequenceType ConsequenceType (Resolution/Bypass/Transform/Modify/Grant)
- ResolutionMethod SetsResolutionMethod (AI narrative context)
- RelationshipOutcome SetsRelationshipOutcome (affects future interactions)
- string TransformDescription (replacement text for obstacle)
- ObstaclePropertyReduction PropertyReduction (reduce obstacle intensity)
```

**Key Architectural Patterns**:
- **Placement vs Ownership**: PlacementLocationId is WHERE it appears, not WHO owns it
- **Inline Victory Conditions**: GoalCards are NOT separate entities, defined inline
- **Consequence-Based Resolution**: ConsequenceType determines what happens to parent Obstacle
- **Graduated Modifiers**: DifficultyModifiers create strategic resource investment choices

### 1.2 Obstacle Entity - Complete Property Inventory

**Purpose**: Strategic barrier with property-based gating

**All Properties (10 total)**:

```
IDENTITY:
- string Id
- string Name
- string Description

CONTEXT SYSTEM:
- List<ObstacleContext> Contexts (Darkness, Climbing, Water - for equipment matching)

DIFFICULTY PROPERTIES:
- int Intensity (1-3 scale, reduces to 0 when cleared)

STATE TRACKING:
- ObstacleState State (Active/Cleared/Transformed)
- ResolutionMethod ResolutionMethod (how it was overcome - AI context)
- RelationshipOutcome RelationshipOutcome (social impact of resolution)
- string TransformedDescription (replacement text after Transform)

PERSISTENCE:
- bool IsPermanent (removed when cleared vs persists at zero)

GOAL CONTAINMENT:
- List<string> GoalIds (references to GameWorld.Goals)
```

**Key Architectural Patterns**:
- **Property-Based Gating**: Intensity property gates goal visibility (NOT boolean flags)
- **Context-Driven Equipment**: ObstacleContext enums match equipment categories
- **Resolution Metadata**: Tracks HOW obstacle was overcome for narrative consequences
- **Lifecycle Control**: IsPermanent determines whether obstacle removed or persists

### 1.3 Obligation Entity - Complete Property Inventory

**Purpose**: Multi-phase mystery/quest structure template

**All Properties (14 total)**:

```
IDENTITY:
- string Id
- string Name
- string Description
- string CompletionNarrative (shown when completed)

INTRO SYSTEM:
- ObligationIntroAction IntroAction (discovery trigger and activation)

UI CONFIGURATION:
- string ColorCode (visual grouping)

PHASE DEFINITIONS:
- List<ObligationPhaseDefinition> PhaseDefinitions (spawns obstacles as phases complete)

OBLIGATION TYPE:
- ObligationObligationType ObligationType (NPCCommissioned vs SelfDiscovered)
- string PatronNpcId (who commissioned it)
- NPC PatronNpc (runtime reference)
- int? DeadlineSegment (absolute segment for NPCCommissioned)

COMPLETION REWARDS:
- int CompletionRewardCoins
- List<string> CompletionRewardItems (equipment IDs)
- List<StatXPReward> CompletionRewardXP
- List<string> SpawnedObligationIds (chain to next obligations)

FAILURE TRACKING:
- bool IsFailed (missed deadline)
```

**Key Architectural Patterns**:
- **Phase-Based Spawning**: PhaseDefinitions spawn obstacles when prerequisites met
- **Commissioned vs Self-Discovered**: Two distinct patterns (pressure vs freedom)
- **Chaining**: SpawnedObligationIds create investigation chains
- **Deadline Pressure**: NPCCommissioned obligations have time pressure

**ObligationPhaseDefinition Structure**:
```
- string Id
- string Name
- string Description
- string CompletionNarrative
- string OutcomeNarrative
- PhaseCompletionReward CompletionReward
  - int UnderstandingReward (Mental expertise)
  - List<ObstacleSpawnInfo> ObstaclesSpawned
    - ObstacleSpawnTargetType TargetType (Location/Route/NPC)
    - string TargetEntityId (where to spawn)
    - Obstacle Obstacle (the obstacle content)
```

**ObligationIntroAction Structure**:
```
- DiscoveryTriggerType TriggerType
- ObligationPrerequisites TriggerPrerequisites
  - string LocationId (where discoverable)
- string ActionText (button text)
- string LocationId (where button appears)
- string IntroNarrative (quest acceptance modal)
- PhaseCompletionReward CompletionReward (spawns Phase 1)
```

### 1.4 MemoryFlag Entity - Complete Property Inventory

**Purpose**: Tracks player knowledge/history with expiration

**All Properties (5 total)**:

```
IDENTITY:
- string Key (unique identifier)
- string Description (what player knows)

TEMPORAL:
- object CreationDay (when acquired)
- object ExpirationDay (when forgotten, -1 = permanent)

IMPORTANCE:
- int Importance (semantic weight)

METHODS:
- bool IsActive(int currentDay) (currently returns true always - broken)
```

**Key Architectural Patterns**:
- **Expiration System**: Knowledge can be forgotten over time
- **String Key Matching**: Uses key strings to check memory existence
- **Boolean Gates**: Player.HasMemory(key) creates if/then unlock patterns

### 1.5 Supporting Entities - Complete Property Inventory

**GoalCard** (Victory Condition):
```
- string Id
- string Name
- string Description
- int threshold (Momentum/Progress/Breakthrough required)
- GoalCardRewards Rewards
  - int? Coins
  - int? Progress/Breakthrough
  - string ObligationId
  - string Item
  - int? InvestigationCubes/StoryCubes/ExplorationCubes
  - string EquipmentId
  - CreateObligationReward CreateObligationData
  - RouteSegmentUnlock RouteSegmentUnlock
  - ObstaclePropertyReduction ObstacleReduction
- bool IsAchieved
```

**GoalCosts** (Entry Cost):
```
- int Time (segments consumed)
- int Focus (Mental resource)
- int Stamina (Physical resource)
- int Coins (rare economic cost)
```

**DifficultyModifier** (Graduated Reduction):
```
- ModifierType Type (Understanding/Mastery/Familiarity/ConnectionTokens/ObstacleProperty/HasItemCategory)
- string Context (for Mastery/ObstacleProperty/HasItemCategory)
- int Threshold (minimum resource needed)
- int Effect (difficulty change when threshold met)
```

**ObstaclePropertyReduction** (Obstacle Weakening):
```
- int ReduceIntensity (amount to reduce)
```

### 1.6 Entity Relationship Architecture

**Current Hierarchical Structure**:

```
GameWorld (Single Source of Truth)
│
├─ Obligations (List<Obligation>)
│  │
│  └─ PhaseDefinitions (inline List<ObligationPhaseDefinition>)
│     │
│     └─ CompletionReward.ObstaclesSpawned (inline List<ObstacleSpawnInfo>)
│        └─ Obstacle (inline at authoring, referenced by ID at runtime)
│
├─ Obstacles (List<Obstacle>)
│  │
│  └─ GoalIds (List<string>) → references GameWorld.Goals
│
├─ Goals (List<Goal>)
│  │
│  ├─ PlacementLocationId → references Location (placement context)
│  ├─ PlacementNpcId → references NPC (placement context)
│  ├─ ObligationId → references Obligation (UI grouping)
│  ├─ ParentObstacle → references Obstacle (lifecycle owner)
│  │
│  └─ GoalCards (inline List<GoalCard>)
│     └─ Rewards (inline GoalCardRewards)
│        ├─ ObstacleReduction (inline ObstaclePropertyReduction)
│        └─ CreateObligationData (inline CreateObligationReward)
│
├─ Locations (List<Location>)
│  │
│  ├─ ActiveGoalIds (List<string>) → references GameWorld.Goals
│  └─ ObstacleIds (List<string>) → references GameWorld.Obstacles
│
└─ NPCs (List<NPC>)
   │
   └─ (presumed) ObstacleIds (List<string>) → references GameWorld.Obstacles

Player
│
├─ ActiveObligationIds (List<string>) → references GameWorld.Obligations
└─ Memories (List<MemoryFlag>) → inline storage
```

**Key Ownership Principles**:
- **Obligation SPAWNS Obstacles**: PhaseDefinitions contain ObstacleSpawnInfo
- **Obstacle CONTAINS Goals**: GoalIds list references goals in GameWorld.Goals
- **Goals APPEAR AT Locations/NPCs**: PlacementLocationId/PlacementNpcId (placement, NOT ownership)
- **Location DISPLAYS Goals**: ActiveGoalIds (filtered view, NOT ownership)
- **Player TRACKS Obligations**: ActiveObligationIds (active pursuit tracking)
- **Player STORES Memories**: Inline list (player owns memory state)

---

## Part 2: Semantic Gaps Analysis

### 2.1 What Goal Represents vs What Scene Doesn't Capture

**Goal Semantics**:
- **Tactical approach** to overcome strategic barrier (parent Obstacle)
- **One-time action** that advances investigation/quest (DeleteOnSuccess)
- **Hierarchical child** of Obstacle (ParentObstacle reference)
- **Multiple approaches** to same barrier (Obstacle has many Goals)
- **Consequence-based resolution** (ConsequenceType: Resolution/Bypass/Transform/Modify/Grant)
- **Graduated difficulty reduction** (DifficultyModifiers based on player resources)

**Scene Semantics (from DYNAMIC_CONTENT_PRINCIPLES.md)**:
- **Ephemeral content** created by action completion (spawned, not pre-existing)
- **Location-placed opportunity** (appears at location until consumed/expired)
- **Entry-cost gated** (resources required to enter)
- **Expiration lifecycle** (time-based or interaction-based removal)
- **Narrative moment** that advances story
- **No hierarchical parent** (independent content, not child of barrier)

**SEMANTIC GAP**:
1. **Parent Obstacle Concept**: Goal is "approach to overcome barrier". Scene is "opportunity at location". Goal has PARENT, Scene does NOT.
2. **Multiple Approaches Pattern**: Goal represents ONE of MANY ways to overcome Obstacle. Scene represents A SINGLE OPPORTUNITY.
3. **Consequence System**: Goal.ConsequenceType determines what happens to parent Obstacle (Resolution/Bypass/Transform/Modify). Scene has NO "parent to modify" concept.
4. **Graduated Difficulty**: Goal.DifficultyModifiers create strategic resource investment (Understanding 2 reduces difficulty by 3, Understanding 5 reduces by 6). Scene has FIXED entry cost, no graduated difficulty.

**CRITICAL REALIZATION**:
Goal is not equivalent to Scene. Goal is a **tactical approach within a strategic challenge structure**. Scene is a **standalone narrative opportunity**. The Obstacle → Goal hierarchy creates the "multiple approaches to one barrier" pattern. Scene has NO hierarchical parent.

### 2.2 What Obstacle Represents vs What Scene Doesn't Capture

**Obstacle Semantics**:
- **Strategic barrier** with multiple tactical approaches (contains Goals)
- **Property-based gating** (Intensity 1-3 gates which Goals visible)
- **Graduated weakening** (Goals reduce Intensity, unlock other Goals)
- **Lifecycle control** over child Goals (Obstacle cleared → Goals removed)
- **Equipment context matching** (ObstacleContext enums match equipment categories)
- **Resolution metadata tracking** (ResolutionMethod, RelationshipOutcome for AI)

**Scene Semantics**:
- **Standalone opportunity** (no child approaches)
- **Entry cost** (not property-based gating)
- **No graduated progression** (consumed in one interaction)
- **No lifecycle control** over children (no children)
- **No context matching** (equipment reduces costs, doesn't match contexts)

**SEMANTIC GAP**:
1. **Container of Approaches**: Obstacle CONTAINS multiple Goals (approaches). Scene is SINGLE opportunity, not container.
2. **Property-Based Progression**: Obstacle.Intensity gates Goal visibility. Scene has NO "gate other content" mechanism.
3. **Weakening Pattern**: Goals reduce Obstacle properties, making OTHER Goals easier. Scene is one-shot, no "make other scenes easier" pattern.
4. **Context System**: Obstacle.Contexts match equipment categories to reduce properties. Scene has entry cost modification, but no typed context matching.

**CRITICAL REALIZATION**:
Obstacle is not equivalent to Scene. Obstacle is a **strategic challenge container** that holds multiple tactical approaches and tracks graduated progression. Scene is a **single atomic opportunity**. The "multiple approaches to one barrier" pattern requires Obstacle as container.

### 2.3 What Obligation Represents vs What Needs Different Entity

**Obligation Semantics**:
- **Multi-phase mystery structure** (PhaseDefinitions spawn obstacles)
- **Phase-based spawning** (completing phase spawns next phase obstacles)
- **Commissioned vs Self-Discovered** pressure pattern
- **Deadline tracking** (NPCCommissioned type has absolute segment deadline)
- **Chaining obligations** (SpawnedObligationIds create quest chains)
- **Intro action pattern** (discovery trigger → modal → acceptance → Phase 1 spawn)
- **Completion rewards** (coins, items, XP, spawned obligations)

**Scene Semantics**:
- **Single narrative moment** (not multi-phase)
- **No spawning structure** (Scene completes, outputs spawn other scenes, but no "phase" concept)
- **No deadline tracking** (expiration, but not commissioned pressure)
- **No chaining structure** (outputs spawn scenes, but no "next phase" sequence)
- **No intro pattern** (Scene just appears, no discovery trigger)

**SEMANTIC GAP**:
1. **Multi-Phase Structure**: Obligation has PhaseDefinitions (ordered sequence). Scene is atomic, no "phase sequence" concept.
2. **Commissioned Pressure**: Obligation.ObligationType (NPCCommissioned vs SelfDiscovered) creates deadline pressure. Scene has expiration, but not commissioned relationship pressure.
3. **Discovery Pattern**: ObligationIntroAction creates "discoverable quest acceptance" pattern. Scene just spawns, no discovery/acceptance.
4. **Chaining Structure**: Obligation.SpawnedObligationIds create multi-obligation arcs. Scene outputs spawn scenes, but no "arc" structure.

**CRITICAL REALIZATION**:
Obligation represents a **multi-phase investigation arc** with discovery, phases, and chaining. Scene is a **single narrative beat**. If we remove Obligation, we lose:
- Multi-phase sequential structure
- Commissioned relationship pressure (deadline + patron)
- Discovery/acceptance pattern
- Investigation arc tracking

**QUESTION**: Does Scene-based architecture still need an "Investigation Arc" entity to represent multi-scene sequences?

### 2.4 What MemoryFlag Represents vs What Card Doesn't Capture

**MemoryFlag Semantics**:
- **Player knowledge/history** (boolean: has memory or not)
- **String key matching** (Player.HasMemory("talked_to_elena"))
- **Expiration system** (CreationDay, ExpirationDay)
- **Importance weighting** (semantic significance)
- **Boolean gate pattern** (if HasMemory then unlock content)

**Card Semantics (from DYNAMIC_CONTENT_PRINCIPLES.md)**:
- **Player-owned resource** (appears in hand during challenges)
- **Playable effect** (spend card for mechanical benefit)
- **Collection storage** (player owns cards, not boolean flags)
- **Permanent or consumable** (card consumed after use or persists)
- **NO string key matching** (cards are typed objects, not string flags)

**SEMANTIC GAP**:
1. **Boolean vs Resource**: MemoryFlag is boolean check (has or not). Card is playable resource with effect.
2. **String Keys vs Typed Objects**: MemoryFlag uses string matching. Card is strongly-typed domain object.
3. **Passive vs Active**: MemoryFlag passively gates content. Card actively provides benefit when played.
4. **Expiration Pattern**: MemoryFlag has ExpirationDay. Card is permanent or consumed, not time-expired.

**CRITICAL REALIZATION**:
MemoryFlag is a **boolean gate mechanism** (if has memory, then unlock). Card is a **playable tactical resource** (spend for effect). These are NOT equivalent. Card eliminates boolean gates by making knowledge a PLAYABLE RESOURCE instead of a CHECK CONDITION.

**TRANSFORMATION**:
- OLD: Player.HasMemory("elena_fire") → if true, unlock "Remind Elena" conversation option
- NEW: Player owns card "Elena Remembers Fire" → appears in hand during Elena conversation → play card for +4 Momentum effect

The semantic is fundamentally different: from CHECKING CONDITIONS to PLAYING RESOURCES.

---

## Part 3: Scene/Card Architecture Design

### 3.1 Core Entity Design - Scene

**Purpose**: Ephemeral narrative opportunity created by action completion, consumed or expired

**Properties Needed**:

```
IDENTITY:
- string Id (unique identifier)
- string Title (display name)
- string Description (narrative text)

PLACEMENT:
- string LocationId (where scene appears)
- Location LocationRef (runtime reference)

ENTRY ECONOMICS:
- SceneCosts EntryCost (resources to enter)
  - int Time (segments)
  - int Focus (Mental resource)
  - int Stamina (Physical resource)
  - int Coins (economic cost)
  - string RequiredCardId (must own card to enter at reduced/zero cost)

CONTENT TYPE:
- SceneContentType ContentType (Narrative/SocialChallenge/MentalChallenge/PhysicalChallenge/Exchange)

CHALLENGE CONFIGURATION (if ContentType is challenge):
- TacticalSystemType SystemType (Social/Mental/Physical)
- string DeckId (which challenge deck)
- string Category (for Social matching)
- ConnectionType ConnectionType (for Social token type)

NARRATIVE CONFIGURATION (if ContentType is Narrative):
- string NarrativeText (story content)
- List<SceneChoice> Choices (player decisions)
  - string ChoiceText (button text)
  - SceneOutcomes Outcomes (what happens when chosen)

EXCHANGE CONFIGURATION (if ContentType is Exchange):
- List<ExchangeOption> ExchangeOptions
  - string OfferText
  - ExchangeCosts Costs
  - ExchangeRewards Rewards

LIFECYCLE:
- SceneExpirationType ExpirationType (OneTime/TimeExpired/EventExpired/Persistent)
- int? ExpirationSegment (absolute segment when expires, null for non-time)
- string ExpirationEventId (event that makes scene obsolete, null for non-event)
- bool IsConsumed (has been entered and completed)

OUTPUTS (what this scene creates when completed):
- List<SceneSpawnInfo> SpawnedScenes (scenes created at other locations)
  - string SceneTemplateId (which scene to create)
  - string TargetLocationId (where to spawn)
- List<CardGrantInfo> GrantedCards (cards added to player collection)
  - string CardId
  - bool IsPermanent (vs consumable)
- ResourceRewards ResourceRewards (immediate resource grants)
  - int Coins
  - int Health
  - int Stamina
  - int Focus
  - List<string> EquipmentIds
  - int InvestigationCubes (to LocationId)
  - int StoryCubes (to NpcId if social scene)
  - int ExplorationCubes (to RouteId if travel scene)

STATE:
- SceneState State (Available/Expired/Consumed)
```

**Key Design Decisions**:
1. **No Parent Hierarchy**: Scene is standalone, not child of Obstacle/Obligation
2. **Typed Content**: SceneContentType enum determines what scene CONTAINS
3. **Entry Cost Modification**: RequiredCardId allows "own card → reduced cost" pattern
4. **Output-Based Chaining**: SpawnedScenes create follow-up content without parent/child hierarchy
5. **Lifecycle Variants**: ExpirationType enum handles one-time, time-based, event-based, persistent patterns
6. **Challenge Integration**: If ContentType = Challenge, delegates to existing Social/Mental/Physical systems
7. **No Graduated Difficulty**: Entry cost is FIXED, not modified by Understanding/Mastery (equipment can reduce)

**What This LOSES from Goal**:
- Graduated DifficultyModifiers (Understanding 2 vs 5 changes difficulty)
- Parent Obstacle relationship (no "approach to barrier" concept)
- ConsequenceType system (Resolution/Bypass/Transform/Modify)
- Property-based gating (Intensity threshold gates visibility)

**What This GAINS**:
- Dynamic spawning (scenes created by actions, not pre-existing)
- Expiration lifecycle (content can be missed)
- Entry cost transparency (perfect information before committing)
- Output chaining (scenes spawn scenes spawn scenes)

### 3.2 Core Entity Design - Card

**Purpose**: Player-owned resource that appears in hand during challenges, playable for mechanical effect

**Properties Needed**:

```
IDENTITY:
- string Id (unique identifier)
- string Title (display name)
- string Description (effect description)

CONTEXT MATCHING:
- List<CardContext> Contexts (when this card appears in hand)
  - CardContextType Type (SocialChallenge/MentalChallenge/PhysicalChallenge/AnyChallenge/NarrativeChoice)
  - string ContextFilter (optional: specific NpcId, LocationId, DeckId, SceneId)

EFFECT:
- CardEffect Effect (what happens when played)
  - int MomentumBonus (Social system builder)
  - int ProgressBonus (Mental system builder)
  - int BreakthroughBonus (Physical system builder)
  - int DoubtReduction (Social system penalty reduction)
  - int ExposureReduction (Mental system penalty reduction)
  - int DangerReduction (Physical system penalty reduction)
  - string TriggeredSceneId (creates scene when played)
  - ResourceRewards ResourceGrant (immediate resources)

LIFECYCLE:
- CardPersistenceType Persistence (Permanent/Consumable)
- bool IsConsumed (has been played and consumed)

ACQUISITION:
- string SourceDescription (narrative: how player got this card)
- int AcquisitionDay (when acquired)

STATE:
- bool IsInPlayerCollection (player owns this card)
```

**Key Design Decisions**:
1. **Context-Based Appearance**: Card appears in hand when context matches (Elena conversation → "Elena Remembers Fire" appears)
2. **No String Key Matching**: Strongly-typed CardContext objects, not string flags
3. **Playable Effect**: Card has mechanical effect when played (not passive boolean gate)
4. **Scene Triggering**: Card can spawn scene when played ("Remind Elena" → creates "Elena's Gratitude" scene)
5. **Permanent vs Consumable**: Some cards permanent (evidence), some one-use (favors)
6. **Collection Storage**: Player owns cards in typed collection, not inline List<MemoryFlag>

**What This REPLACES**:
- MemoryFlag boolean gates (HasMemory checks)
- String key matching (Player.HasMemory("key"))
- Passive unlock conditions (if memory then show option)

**What This ENABLES**:
- Tactical card play (spend knowledge for momentum bonus)
- Evidence-based challenges (play "Evidence of Sabotage" for +4 Progress)
- Relationship cards (play "Elena's Favor" for bonus in any social situation)
- Scene triggering ("Play this card to unlock special scene")

### 3.3 Supporting Entity Design - SceneTemplate

**Purpose**: Reusable scene definition for dynamic content generation (AI or parameterized spawning)

**Properties Needed**:

```
IDENTITY:
- string Id (template identifier)
- string TemplateType (Investigation/Relationship/Event/Challenge)

PARAMETERS:
- List<SceneParameter> Parameters
  - string ParameterName
  - SceneParameterType Type (NpcId/LocationId/ObstacleContext/Number/Text)
  - object DefaultValue (optional)

CONTENT STRUCTURE:
- string TitleTemplate (with {npcName}, {locationName} placeholders)
- string DescriptionTemplate (with parameter placeholders)
- SceneContentType ContentType
- (all other Scene properties, using parameters)

GENERATION RULES:
- List<ParameterConstraint> Constraints
  - string ParameterName
  - ConstraintType Type (MustMatchContext/MustBeInList/MustBeNumericRange)
  - object ConstraintValue
```

**Key Design Decisions**:
1. **Parameterized Content**: Templates have placeholders filled at spawn time
2. **AI Generation Fit**: AI receives template + parameters → generates specific scene
3. **Reusability**: Same template creates multiple concrete scenes with different parameters
4. **Type Safety**: SceneParameterType enum ensures correct parameter types

**Example**:
```
SceneTemplate: "npc_gratitude"
Parameters:
  - npcId (NpcId type)
  - actionDescription (Text type)
TitleTemplate: "{npcName} Thanks You"
DescriptionTemplate: "{npcName} expresses gratitude for {actionDescription}"

Spawn Call:
CreateSceneFromTemplate(
  templateId: "npc_gratitude",
  parameters: { npcId: "elena", actionDescription: "helping during fire" },
  targetLocationId: "brass_bell_inn"
)

Result:
Scene Id: "elena_gratitude_12345"
Title: "Elena Thanks You"
Description: "Elena expresses gratitude for helping during fire"
LocationId: "brass_bell_inn"
```

### 3.4 Supporting Entity Design - ActionDefinition

**Purpose**: Defines persistent location features (always-available actions) and their outputs

**Properties Needed**:

```
IDENTITY:
- string Id
- string Name
- string Description

PLACEMENT:
- string LocationId (where action available)
- ActionAvailabilityType Availability (AlwaysAvailable/TimeSpecific/ResourceGated)
- List<TimeBlocks> AvailableTimes (if TimeSpecific)
- ActionCosts GatingCosts (if ResourceGated)

COSTS:
- ActionCosts Costs
  - int Time
  - int Focus
  - int Stamina
  - int Coins

OUTPUTS:
- List<SceneSpawnInfo> SpawnedScenes (scenes created)
- List<CardGrantInfo> GrantedCards (cards granted)
- ResourceRewards ResourceRewards (immediate resources)

TYPE:
- ActionDefinitionType Type (PersistentFeature/OneTimeEvent/RepeatableGoal)
```

**Key Design Decisions**:
1. **Persistent Features**: Represents "Buy room (10 coins → rest)" type actions
2. **Output-Based**: Completing action spawns scenes/cards/resources
3. **Availability Patterns**: Always available vs time-specific vs resource-gated
4. **Replaces**: Current LocationAction system with output-based pattern

**Example**:
```
ActionDefinition:
Id: "deliver_package_to_thomas"
Name: "Deliver Package to Thomas"
LocationId: "mill"
Type: PersistentFeature
Costs: { Time: 1 }
Outputs:
  SpawnedScenes:
    - SceneTemplateId: "thomas_mentions_problem"
      TargetLocationId: "mill"
  ResourceRewards:
    Coins: 15
    StoryCubes: 2 (to Thomas)
```

### 3.5 Entity Inventory Summary

**New Architecture Entities**:

1. **Scene** - Ephemeral narrative opportunity (21 properties)
2. **Card** - Player-owned playable resource (9 properties)
3. **SceneTemplate** - Reusable scene definition for dynamic generation (6 properties)
4. **ActionDefinition** - Persistent location feature with outputs (9 properties)

**Supporting Structures**:
- SceneCosts (4 properties)
- SceneChoice (2 properties)
- SceneOutcomes (3 properties: SpawnedScenes, GrantedCards, ResourceRewards)
- ExchangeOption (3 properties)
- CardContext (2 properties)
- CardEffect (8 properties)
- SceneParameter (3 properties)
- ActionCosts (4 properties)

**Enums**:
- SceneContentType (Narrative/SocialChallenge/MentalChallenge/PhysicalChallenge/Exchange)
- SceneExpirationType (OneTime/TimeExpired/EventExpired/Persistent)
- SceneState (Available/Expired/Consumed)
- CardContextType (SocialChallenge/MentalChallenge/PhysicalChallenge/AnyChallenge/NarrativeChoice)
- CardPersistenceType (Permanent/Consumable)
- SceneParameterType (NpcId/LocationId/ObstacleContext/Number/Text)
- ActionAvailabilityType (AlwaysAvailable/TimeSpecific/ResourceGated)
- ActionDefinitionType (PersistentFeature/OneTimeEvent/RepeatableGoal)

---

## Part 4: Clean Replacement Strategy

### 4.1 Can Scene COMPLETELY Replace Goal?

**NO. Scene cannot completely replace Goal because:**

1. **Goal is "approach to strategic barrier"**: Goal.ParentObstacle creates "multiple approaches to one barrier" pattern. Scene is standalone opportunity with NO parent concept.

2. **Goal has graduated difficulty**: Goal.DifficultyModifiers create "Understanding 2 reduces difficulty by 3, Understanding 5 reduces difficulty by 6" pattern. Scene has FIXED entry cost (equipment can reduce, but not graduated player expertise).

3. **Goal modifies parent**: Goal.ConsequenceType (Modify) + Goal.PropertyReduction weakens parent Obstacle, making OTHER Goals easier/visible. Scene completes independently, no "modify shared barrier" concept.

4. **Goal is hierarchical child**: Goal lives INSIDE Obstacle lifecycle. When Obstacle cleared, all child Goals removed. Scene is independent, no parent lifecycle.

**What Scene DOES Capture from Goal**:
- Entry cost transparency (GoalCosts → SceneCosts)
- Challenge delegation (Goal.SystemType + DeckId → Scene.SystemType + DeckId)
- Victory conditions (Goal.GoalCards → embedded in challenge configuration)
- Resource rewards (GoalCard.Rewards → Scene.ResourceRewards)
- Content spawning (GoalCard spawns obligations → Scene.SpawnedScenes)

**What Scene CANNOT Capture from Goal**:
- Parent Obstacle relationship (approach to barrier)
- Graduated difficulty modifiers (expertise-based reduction)
- Obstacle property modification (weaken shared barrier)
- Hierarchical lifecycle (parent cleared → children removed)
- Multiple approaches pattern (Goal 1, Goal 2, Goal 3 all target same Obstacle)

**CRITICAL DECISION**:
If we want "multiple approaches to strategic barrier" pattern, we CANNOT use Scene alone. We need:
- **Option A**: Keep Obstacle + Goal hierarchy, add Scene as separate content type
- **Option B**: Eliminate "multiple approaches to barrier" pattern entirely, use only Scenes
- **Option C**: Create NEW entity "Challenge" that replaces Obstacle but keeps hierarchical pattern

### 4.2 What Happens to Obstacle Concept?

**Analysis**: Obstacle represents "strategic barrier with multiple tactical approaches". Dynamic Content Principles doc doesn't directly address this pattern.

**From DYNAMIC_CONTENT_PRINCIPLES.md Section "Obstacles at Locations"**:
```
Obstacle "Locked Gate" at Town Border

Creates three persistent features at location:
1. "Force Gate" (Physical challenge, 3 Stamina)
2. "Pick Lock" (requires Lockpicks, 1 Focus)
3. "Bribe Guard" (10 coins)

All three always available. Player chooses which to afford.

Completing any approach:
→ Creates scene "Beyond Gate" at destination
→ Removes gate features from Town Border
→ Travel now possible
```

**Interpretation**: Obstacle becomes **three ActionDefinitions** (persistent features). Completing any action removes all three actions. This is NOT the same as Obstacle → Goals hierarchy, because:
- ActionDefinitions are PEERS (not children of Obstacle)
- No shared Intensity property (each has independent cost)
- No graduated weakening (completing one doesn't make others easier)
- Removal is ALL-OR-NOTHING (complete any → remove all)

**TRANSFORMATION OPTIONS**:

**Option A - Obstacle → SceneGroup**:
```csharp
public class SceneGroup
{
  string Id
  string Name (e.g., "Locked Gate")
  List<string> SceneIds (e.g., "force_gate", "pick_lock", "bribe_guard")
  string LocationId (where group appears)
  SceneGroupCompletionType CompletionType (CompleteAny/CompleteAll)
  // When completion type met, remove all scenes in group
}
```
This preserves "multiple approaches" pattern but loses graduated weakening.

**Option B - Obstacle → Multiple Persistent ActionDefinitions**:
```csharp
ActionDefinition "force_gate":
  Costs: { Stamina: 3 }
  Outputs:
    SpawnedScenes: ["beyond_gate"]
    RemovedActions: ["force_gate", "pick_lock", "bribe_guard"]

ActionDefinition "pick_lock":
  Costs: { Focus: 1, RequiredEquipment: "lockpicks" }
  Outputs:
    SpawnedScenes: ["beyond_gate"]
    RemovedActions: ["force_gate", "pick_lock", "bribe_guard"]
```
This flattens hierarchy entirely. Each action independently spawns same outcome and removes peers.

**Option C - Keep Obstacle, Reframe as "Barrier State"**:
```csharp
public class Barrier
{
  string Id
  string Name
  string LocationId
  BarrierState State (Active/Cleared)
  List<string> ApproachActionIds (ActionDefinitions that target this barrier)
  // When any approach completes, State = Cleared, remove all ApproachActions
}
```
This keeps Obstacle concept but eliminates Intensity/graduated weakening. Approaches are ActionDefinitions, not Goals.

**RECOMMENDATION**:
**Option B (Flatten to ActionDefinitions)** aligns best with Dynamic Content Principles:
- No boolean gates (just resource costs)
- No property-based gating (just entry costs)
- Actions create content (spawn scenes)
- Removal handled via action outputs (RemovedActions list)

**What This LOSES**:
- Graduated weakening pattern (Goal A reduces Intensity, making Goal B easier)
- Shared strategic state (Obstacle.Intensity)
- Hierarchical child lifecycle (Obstacle cleared → children removed)

**What This GAINS**:
- Flat action model (all peer actions)
- Output-based consequences (actions spawn scenes and remove other actions)
- No special container entity (simpler architecture)

### 4.3 What Happens to Obligation Concept?

**Analysis**: Obligation represents "multi-phase investigation arc with discovery, commissioned pressure, and chaining". Dynamic Content Principles addresses this:

**From DYNAMIC_CONTENT_PRINCIPLES.md Section "Investigations Are Scene Chains, Not Phase Gates"**:
```
Accept investigation "Silent Mill"
→ Creates scene "Examine Waterwheel" at Mill
→ Cost: 1 Focus

Complete "Examine Waterwheel"
→ Grants card "Evidence of Sabotage"
→ Creates scene "Question Mill Owner" at Mill Owner's House
→ Cost: 2 Initiative

Complete "Question Mill Owner"
→ Creates TWO scenes (player chooses which):
  • "Search Shed" at Mill Storage (Physical)
  • "Ask Neighbors" at Market Square (Social)
→ Cost: 1 Stamina or 1 Focus respectively

Complete either
→ Creates scene "Confront Culprit" (final)
→ Investigation completes

Each scene creates next scene. No phase checking.
```

**Interpretation**: Obligation.PhaseDefinitions BECOMES **Scene.SpawnedScenes chaining**. Each scene spawns next scene(s). No "phase" entity needed.

**CRITICAL LOSS**: What about **commissioned pressure** (deadline + patron + failure consequences)?

**From DYNAMIC_CONTENT_PRINCIPLES.md**: No explicit commissioned pressure pattern. Document focuses on resource costs and expiration, not NPC relationship deadlines.

**TRANSFORMATION OPTIONS**:

**Option A - Eliminate Commissioned Pressure**:
- Remove ObligationType distinction
- Remove DeadlineSegment
- Remove PatronNpcId
- Remove failure consequences
- All investigations become exploration-based (self-discovered pattern)
- Pressure comes only from scene expiration

**Option B - Commissioned Pressure via Scene Chain Metadata**:
```csharp
public class InvestigationArc
{
  string Id
  string Name
  string PatronNpcId (if commissioned)
  int? DeadlineSegment (if commissioned)
  string CurrentSceneId (tracks progress)
  List<string> CompletedSceneIds (history)
  bool IsFailed (missed deadline)
}

// Scenes reference arc:
Scene
{
  string InvestigationArcId (optional: which arc this scene belongs to)
  // When scene completes, update arc.CurrentSceneId
  // When deadline passes, arc.IsFailed = true, all arc scenes expire
}
```
This preserves commissioned pressure without "phase" concept. Arc tracks chain, scenes spawn scenes.

**Option C - Commissioned Pressure via NPC Obligation Tracking**:
```csharp
NPC
{
  List<NPCObligation> ActiveObligations
  {
    string FirstSceneId (chain entry point)
    int DeadlineSegment
    int CoinReward
    bool IsFailed
  }
}

// When deadline passes, NPC.RelationshipFlow decreases, FirstSceneId scene expires
```
This embeds commissioned tracking in NPC relationship, not separate Obligation entity.

**RECOMMENDATION**:
**Option B (InvestigationArc entity)** preserves commissioned pressure while adopting scene chaining:
- Commissioned vs Self-Discovered distinction maintained
- Deadline pressure preserved
- Scene chaining replaces phase system
- Arc tracks progress without "phase" concept

**What This LOSES**:
- PhaseDefinitions structure (replaced by scene chaining)
- ObligationIntroAction formal discovery pattern (replaced by initial scene spawn)
- Inline ObstacleSpawnInfo (replaced by Scene.SpawnedScenes)

**What This PRESERVES**:
- Commissioned pressure (deadline + patron + failure)
- Investigation chain tracking
- Completion rewards
- Chaining to next investigations

**What This GAINS**:
- Scene-based chaining (each scene spawns next)
- Branching paths (scene spawns multiple scenes, player chooses)
- Failure creates different content (scene spawns "failure path" scenes)

### 4.4 What Happens to MemoryFlag?

**Analysis**: MemoryFlag is boolean gate mechanism. Card is playable resource. Transformation is semantic shift.

**COMPLETE REPLACEMENT**:

```
OLD PATTERN:
// Action completion
Player.AddMemory("elena_fire", "Helped Elena during fire", currentDay, importance: 5)

// Later, checking
if (Player.HasMemory("elena_fire")) {
  // Unlock "Remind Elena" conversation option
}

NEW PATTERN:
// Action completion grants card
Card elenaFireCard = new Card {
  Id = "elena_remembers_fire",
  Title = "Elena Remembers",
  Description = "You helped Elena during the fire. She won't forget this.",
  Contexts = [
    new CardContext { Type = CardContextType.SocialChallenge, ContextFilter = "elena" }
  ],
  Effect = new CardEffect {
    MomentumBonus = 4,
    TriggeredSceneId = "elena_deep_gratitude"
  },
  Persistence = CardPersistenceType.Permanent
}
Player.Cards.Add(elenaFireCard)

// Later, during Elena conversation (Social challenge)
// Card automatically appears in hand (context matches: SocialChallenge with NPC=elena)
// Player can PLAY card for +4 Momentum
// OR if card has TriggeredSceneId, playing creates scene "Elena's Deep Gratitude"
```

**Transformation Complete**: MemoryFlag → Card replacement is TOTAL. No semantic loss, only semantic GAIN (from boolean gate to playable resource).

**What This ELIMINATES**:
- String key matching (Player.HasMemory("key"))
- Boolean gate pattern (if HasMemory then unlock)
- Passive unlock conditions
- ExpirationDay temporal forgetting (cards don't expire, they're permanent or consumed)

**What This ENABLES**:
- Tactical resource play (evidence cards for bonus)
- Context-based appearance (card appears when relevant)
- Scene triggering (play card → spawn scene)
- Permanent vs consumable patterns (favors consumed, evidence permanent)

**MIGRATION STRATEGY**:
1. Search all Player.HasMemory() calls
2. For each memory key:
   - Create corresponding Card definition
   - Set CardContext to match where memory was checked
   - Set CardEffect to provide benefit (momentum/progress bonus)
   - If memory unlocked content, CardEffect.TriggeredSceneId spawns that content
3. Replace AddMemory() with GrantCard()
4. Delete MemoryFlag class and Player.Memories list

### 4.5 Is Anything LOST in Transformation?

**LOST COMPLETELY**:
1. **Graduated Difficulty Modifiers**: Goal.DifficultyModifiers (Understanding 2 reduces by 3, Understanding 5 reduces by 6) has NO equivalent in Scene architecture. Scene has fixed entry cost.

2. **Property-Based Gating**: Obstacle.Intensity gates which Goals visible. Scene architecture has NO "property threshold gates content visibility" pattern.

3. **Obstacle Weakening Chain**: Goal A reduces Obstacle.Intensity → Goal B becomes easier → Goal C becomes visible. Scene architecture has NO "completing content makes other content easier" pattern.

4. **Hierarchical Lifecycle**: Obstacle cleared → all child Goals removed. SceneGroup cleared → scenes removed, but no PARENT entity with properties.

5. **Discovery Trigger Formalism**: ObligationIntroAction with TriggerPrerequisites creates formal "when condition met, spawn discoverable quest acceptance button" pattern. Scene architecture has scenes spawned by actions, no formal discovery trigger.

6. **Temporal Forgetting**: MemoryFlag.ExpirationDay creates "knowledge forgotten over time" pattern. Card.Persistence is Permanent or Consumable, no time-based expiration.

**TRANSFORMED (Semantic Change)**:
1. **Multi-Phase Structure**: Obligation.PhaseDefinitions → Scene chaining (each scene spawns next). Phases become scene chains, not formal phase objects.

2. **Commissioned Pressure**: Obligation with PatronNpcId + DeadlineSegment → InvestigationArc entity tracking deadline. Pressure preserved but structure changed.

3. **Multiple Approaches**: Obstacle → Goals (multiple approaches to one barrier) → Multiple ActionDefinitions targeting same barrier (peer actions, not hierarchical).

4. **Boolean Gates**: MemoryFlag checks → Card ownership and play. From passive unlock to active resource.

**PRESERVED**:
1. **Entry Cost Transparency**: GoalCosts → SceneCosts (perfect information)
2. **Challenge Delegation**: Goal.SystemType → Scene.SystemType (existing tactical systems)
3. **Resource Rewards**: GoalCard.Rewards → Scene.ResourceRewards
4. **Content Chaining**: Obligation chains → Scene spawning chains
5. **Expiration**: Goal.DeleteOnSuccess → Scene.ExpirationType
6. **Equipment Cost Reduction**: DifficultyModifier.HasItemCategory → Scene.EntryCost modification by equipment

**NEW CAPABILITIES**:
1. **Dynamic Spawning**: Scenes created by actions, not pre-existing libraries
2. **Expiration Lifecycle**: Scenes can be missed (time/event expiration)
3. **Branching Chains**: Scene spawns multiple scenes, player chooses path
4. **Failure Paths**: Scene failure spawns different content
5. **Playable Knowledge**: Cards played for tactical effect
6. **Scene-Triggered Cards**: Playing card spawns scene

---

## Part 5: Entity Relationship Redesign

### 5.1 Current Relationship Structure

```
Obligation → Spawns → Obstacle → Contains → Goal → Appears At → Location
                                                  → Appears At → NPC

Player.ActiveObligationIds → Obligation
Location.ActiveGoalIds → Goal (filtered view)
Location.ObstacleIds → Obstacle
Goal.ParentObstacle → Obstacle (lifecycle owner)
Goal.PlacementLocationId → Location (placement context)
Player.Memories → MemoryFlag (inline ownership)
```

**Key Patterns**:
- **Spawning**: Obligation spawns Obstacles (parent creates children)
- **Containment**: Obstacle contains Goals (parent owns children)
- **Placement**: Goal appears at Location/NPC (placement ≠ ownership)
- **Filtered View**: Location.ActiveGoalIds shows relevant goals (NOT ownership)
- **Inline Ownership**: Player owns Memories directly

### 5.2 New Relationship Structure

```
GameWorld (Single Source of Truth)
│
├─ SceneTemplates (List<SceneTemplate>)
│  └─ (reusable definitions for dynamic generation)
│
├─ Scenes (List<Scene>)
│  └─ LocationId → references Location (placement)
│
├─ ActionDefinitions (List<ActionDefinition>)
│  └─ LocationId → references Location (where available)
│
├─ Cards (List<Card>)
│  └─ (global card definitions)
│
├─ InvestigationArcs (List<InvestigationArc>)
│  ├─ PatronNpcId → references NPC (if commissioned)
│  ├─ CurrentSceneId → references Scene (progress tracking)
│  └─ CompletedSceneIds → references Scenes (history)
│
├─ Locations (List<Location>)
│  ├─ ActiveSceneIds (List<string>) → references GameWorld.Scenes
│  └─ PersistentActionIds (List<string>) → references GameWorld.ActionDefinitions
│
└─ NPCs (List<NPC>)
   └─ ActiveSceneIds (List<string>) → references GameWorld.Scenes (social scenes)

Player
│
├─ Cards (List<string>) → references GameWorld.Cards (owned cards)
└─ ActiveInvestigationIds (List<string>) → references GameWorld.InvestigationArcs
```

**Key Patterns**:
- **Flat Lists**: All Scenes in GameWorld.Scenes (no hierarchical parent)
- **Placement References**: Scene.LocationId determines where scene appears
- **Action Outputs**: ActionDefinition.SpawnedScenes creates new scenes
- **Scene Chaining**: Scene.SpawnedScenes creates follow-up scenes
- **Arc Tracking**: InvestigationArc tracks scene chain progress
- **Card Ownership**: Player.Cards references global Card definitions
- **Filtered Views**: Location.ActiveSceneIds shows scenes at this location

### 5.3 Spawning and Chaining Mechanics

**ActionDefinition Spawns Scene**:
```
Player completes ActionDefinition "deliver_package_to_thomas"
→ ActionDefinition.Outputs.SpawnedScenes: ["thomas_mentions_problem"]
→ Create Scene from SceneTemplate "thomas_mentions_problem"
→ Scene.LocationId = "mill"
→ Add Scene.Id to Location.ActiveSceneIds["mill"]
→ Scene now appears at Mill location
```

**Scene Spawns Scene**:
```
Player enters Scene "examine_waterwheel" (Mental challenge)
→ Completes challenge successfully
→ Scene.Outputs.SpawnedScenes: ["question_mill_owner"]
→ Scene.Outputs.GrantedCards: ["evidence_of_sabotage"]
→ Create Scene "question_mill_owner" at "mill_owner_house"
→ Grant Card "evidence_of_sabotage" to Player.Cards
→ Scene "examine_waterwheel" State = Consumed
```

**Scene Spawns Multiple Scenes (Branching)**:
```
Player completes Scene "question_mill_owner"
→ Scene.Outputs.SpawnedScenes: ["search_shed", "ask_neighbors"]
→ Create Scene "search_shed" at "mill_storage" (Physical)
→ Create Scene "ask_neighbors" at "market_square" (Social)
→ Player sees TWO new scenes, chooses which to pursue
```

**InvestigationArc Tracking**:
```
InvestigationArc "silent_mill"
  PatronNpcId: "mill_owner"
  DeadlineSegment: 150
  CurrentSceneId: "question_mill_owner"
  CompletedSceneIds: ["accept_investigation", "examine_waterwheel"]

When Scene "question_mill_owner" completes:
→ Arc.CompletedSceneIds.Add("question_mill_owner")
→ Arc.CurrentSceneId = "search_shed" OR "ask_neighbors" (player choice)

When CurrentSegment >= DeadlineSegment:
→ Arc.IsFailed = true
→ All scenes with InvestigationArcId = "silent_mill" State = Expired
→ NPC "mill_owner" RelationshipFlow -= 3
```

**Card Context Matching**:
```
Player enters Scene "elena_conversation" (Social challenge with Elena)
→ Scene.ContentType = SocialChallenge
→ Scene.SystemType = Social
→ Scene starts Social tactical session

During session, build player hand:
→ Query Player.Cards where Card.Contexts contains:
   - CardContextType.SocialChallenge
   - CardContextType.AnyChallenge
   - ContextFilter = "elena" OR ContextFilter = null (generic social)
→ Found: Card "elena_remembers_fire" (context: SocialChallenge, filter: elena)
→ Add to player hand
→ Player can play card for +4 Momentum
→ If card.Effect.TriggeredSceneId set, playing spawns that scene
```

### 5.4 Storage and Lifecycle

**Where Scenes Live**:
- **Authoring**: SceneTemplates in JSON (reusable definitions)
- **Runtime**: Scenes in GameWorld.Scenes (all active/consumed/expired scenes)
- **Filtered View**: Location.ActiveSceneIds (scenes at this location, State = Available)
- **Cleanup**: Expired/Consumed scenes remain in GameWorld.Scenes for history, filtered out of active views

**Where Cards Live**:
- **Authoring**: Card definitions in JSON (card library)
- **Runtime**: Cards in GameWorld.Cards (all possible cards)
- **Player Ownership**: Player.Cards = List<string> CardIds (cards player owns)
- **Context Matching**: When challenge starts, query Player.Cards for matching contexts

**Where ActionDefinitions Live**:
- **Authoring**: ActionDefinitions in JSON (persistent features)
- **Runtime**: ActionDefinitions in GameWorld.ActionDefinitions (all actions)
- **Location View**: Location.PersistentActionIds (actions available at location)
- **Removal**: When action.Outputs.RemovedActions triggers, remove from Location.PersistentActionIds

**Where InvestigationArcs Live**:
- **Authoring**: InvestigationArc definitions in JSON (commissioned investigations)
- **Runtime**: InvestigationArcs in GameWorld.InvestigationArcs (all arcs)
- **Player Tracking**: Player.ActiveInvestigationIds (investigations player pursuing)
- **Cleanup**: Failed/Completed arcs remain in GameWorld for history

---

## Part 6: Data Migration Requirements

### 6.1 Goal → SceneTemplate Mapping

**Mapping Analysis**: Goal is NOT 1:1 with SceneTemplate because Goal is hierarchical child of Obstacle. Multiple Goals target same Obstacle.

**Pattern 1: Obstacle with Multiple Goals → Multiple ActionDefinitions + Shared Outcome Scene**

```
OLD (Goal/Obstacle):
Obstacle "Locked Gate" (Intensity 3)
  Goal "Force Gate" (Physical, cost 3 Stamina, ConsequenceType: Resolution)
  Goal "Pick Lock" (Mental, cost 1 Focus, requires Lockpicks, ConsequenceType: Resolution)
  Goal "Bribe Guard" (cost 10 Coins, ConsequenceType: Bypass)

NEW (ActionDefinition + Scene):
ActionDefinition "force_gate"
  LocationId: "town_border"
  Costs: { Stamina: 3 }
  Outputs:
    SpawnedScenes: ["beyond_gate"]
    RemovedActions: ["force_gate", "pick_lock", "bribe_guard"]

ActionDefinition "pick_lock"
  LocationId: "town_border"
  Costs: { Focus: 1 }
  RequiredEquipment: ["lockpicks"]
  Outputs:
    SpawnedScenes: ["beyond_gate"]
    RemovedActions: ["force_gate", "pick_lock", "bribe_guard"]

ActionDefinition "bribe_guard"
  LocationId: "town_border"
  Costs: { Coins: 10 }
  Outputs:
    SpawnedScenes: ["beyond_gate"]
    RemovedActions: ["force_gate", "pick_lock", "bribe_guard"]

SceneTemplate "beyond_gate"
  Title: "Beyond the Gate"
  Description: "You've passed the locked gate and can now access the restricted area."
  ContentType: Narrative
  ExpirationType: Persistent
```

**Mapping**: 1 Obstacle with 3 Goals → 3 ActionDefinitions + 1 shared SceneTemplate

**Pattern 2: Goal with GoalCards → Scene with Challenge + GoalCard Rewards**

```
OLD (Goal with GoalCards):
Goal "Persuade Guard"
  SystemType: Social
  DeckId: "guard_conversation"
  GoalCards:
    - Name: "Basic Success" (threshold: 10 Momentum, rewards: +2 NPC cubes)
    - Name: "Exceptional Success" (threshold: 18 Momentum, rewards: +5 NPC cubes, Equipment: "guard_token")

NEW (Scene with Challenge):
SceneTemplate "persuade_guard"
  Title: "Persuade the Guard"
  ContentType: SocialChallenge
  SystemType: Social
  DeckId: "guard_conversation"
  VictoryConditions:
    - Threshold: 10, Rewards: { StoryCubes: 2 }
    - Threshold: 18, Rewards: { StoryCubes: 5, EquipmentId: "guard_token" }
  ExpirationType: OneTime
```

**Mapping**: 1 Goal with GoalCards → 1 SceneTemplate with embedded VictoryConditions

**Pattern 3: Obligation Phase → Scene Chain**

```
OLD (Obligation with Phases):
Obligation "Silent Mill Investigation"
  PhaseDefinitions:
    - Phase 1: "Examine Waterwheel" (spawns Obstacle "Suspicious Waterwheel")
      Obstacle contains Goal "Inspect Mechanism" (Mental challenge)
      Completion: UnderstandingReward +2, spawns Phase 2
    - Phase 2: "Question Mill Owner" (spawns Obstacle "Evasive Owner")
      Obstacle contains Goal "Press for Truth" (Social challenge)
      Completion: spawns Phase 3 obstacles

NEW (Scene Chain):
SceneTemplate "examine_waterwheel"
  ContentType: MentalChallenge
  SystemType: Mental
  DeckId: "investigation_basic"
  Outputs:
    GrantedCards: ["evidence_of_sabotage"]
    SpawnedScenes: ["question_mill_owner"]
    ResourceRewards: { Understanding: 2 }

SceneTemplate "question_mill_owner"
  ContentType: SocialChallenge
  SystemType: Social
  DeckId: "mill_owner_evasive"
  Outputs:
    SpawnedScenes: ["search_shed", "ask_neighbors"] // Branching!

SceneTemplate "search_shed"
  ContentType: PhysicalChallenge
  SystemType: Physical
  DeckId: "search_physical"
  Outputs:
    SpawnedScenes: ["confront_culprit"]

SceneTemplate "ask_neighbors"
  ContentType: SocialChallenge
  SystemType: Social
  DeckId: "gather_gossip"
  Outputs:
    SpawnedScenes: ["confront_culprit"]

InvestigationArc "silent_mill"
  PatronNpcId: "mill_owner"
  DeadlineSegment: 150
  InitialSceneId: "examine_waterwheel"
  CompletionSceneId: "confront_culprit"
```

**Mapping**: 1 Obligation with N PhaseDefinitions → N SceneTemplates chained via SpawnedScenes + 1 InvestigationArc

### 6.2 Obstacle → ??? Mapping

**Mapping Strategy**: Obstacle DELETED. Replaced by pattern from 6.1 Pattern 1:

1. **Identify Obstacle Goals**: For each Obstacle, list all Goals referencing it
2. **Create ActionDefinitions**: Each Goal becomes ActionDefinition
3. **Shared Outcome Scene**: Obstacle resolution becomes shared SceneTemplate
4. **Mutual Removal**: Each ActionDefinition.Outputs.RemovedActions includes all peer actions
5. **Graduated Weakening LOST**: If Obstacle has Goals with ConsequenceType.Modify (reduce Intensity), this pattern is LOST. Must decide:
   - **Option A**: Eliminate graduated weakening (all approaches peer actions)
   - **Option B**: Keep Obstacle as "Barrier" entity with State (Active/Cleared), ActionDefinitions reference Barrier.Id, any action clears barrier

**Recommendation**: **Option A (Eliminate)** aligns with Dynamic Content Principles (no property-based gating, just resource costs). Graduated weakening is complexity that doesn't fit Scene model.

### 6.3 Obligation → InvestigationArc + Scene Chain Mapping

**Mapping Strategy**:

1. **Obligation.IntroAction → Initial SceneTemplate**:
   - IntroAction.ActionText → ActionDefinition.Name (discovery button)
   - IntroAction.IntroNarrative → SceneTemplate.Description (quest acceptance scene)
   - IntroAction.CompletionReward.ObstaclesSpawned → SceneTemplate.SpawnedScenes (Phase 1)

2. **Obligation.PhaseDefinitions → SceneTemplates Chain**:
   - Each PhaseDefinition → SceneTemplate
   - PhaseDefinition.CompletionReward.ObstaclesSpawned → SceneTemplate.SpawnedScenes (next phase)
   - ObstacleSpawnInfo.Obstacle.Goals → SceneTemplate.ContentType configuration

3. **Obligation Metadata → InvestigationArc**:
   - Obligation.Id → InvestigationArc.Id
   - Obligation.PatronNpcId → InvestigationArc.PatronNpcId
   - Obligation.DeadlineSegment → InvestigationArc.DeadlineSegment
   - Obligation.CompletionRewardCoins → InvestigationArc.CompletionRewards.Coins
   - Obligation.SpawnedObligationIds → InvestigationArc.ChainedArcIds

**Example Migration**:

```
OLD:
Obligation "silent_mill"
  IntroAction:
    ActionText: "Accept Investigation"
    IntroNarrative: "The mill owner asks you to investigate the waterwheel damage."
    CompletionReward:
      ObstaclesSpawned: [Obstacle "Suspicious Waterwheel" at Mill]
  PhaseDefinitions:
    - Phase "Examine Waterwheel"
      CompletionReward:
        UnderstandingReward: 2
        ObstaclesSpawned: [Obstacle "Evasive Owner" at Mill Owner's House]
  PatronNpcId: "mill_owner"
  DeadlineSegment: 50 (duration, converted to absolute at activation)
  CompletionRewardCoins: 25

NEW:
ActionDefinition "accept_silent_mill_investigation"
  Name: "Accept Investigation"
  LocationId: "mill"
  Type: OneTimeEvent
  Costs: { Time: 0 }
  Outputs:
    SpawnedScenes: ["accept_investigation_scene"]

SceneTemplate "accept_investigation_scene"
  Title: "The Mill Owner's Request"
  Description: "The mill owner asks you to investigate the waterwheel damage."
  ContentType: Narrative
  Choices:
    - ChoiceText: "I'll look into it"
      Outcomes:
        SpawnedScenes: ["examine_waterwheel"]
        ResourceRewards: { InvestigationArcActivation: "silent_mill" }
    - ChoiceText: "Not right now"
      Outcomes: {} (scene ends, no arc activation)

SceneTemplate "examine_waterwheel"
  Title: "Examine the Waterwheel"
  ContentType: MentalChallenge
  SystemType: Mental
  DeckId: "investigation_basic"
  InvestigationArcId: "silent_mill"
  Outputs:
    GrantedCards: ["evidence_of_sabotage"]
    SpawnedScenes: ["question_mill_owner"]
    ResourceRewards: { Understanding: 2 }

SceneTemplate "question_mill_owner"
  Title: "Question the Mill Owner"
  ContentType: SocialChallenge
  SystemType: Social
  DeckId: "mill_owner_evasive"
  InvestigationArcId: "silent_mill"
  Outputs:
    SpawnedScenes: ["confront_culprit"]

InvestigationArc "silent_mill"
  Name: "Silent Mill Investigation"
  PatronNpcId: "mill_owner"
  DeadlineDuration: 50 (converted to absolute segment on activation)
  CompletionRewards:
    Coins: 25
  CompletionSceneId: "confront_culprit"
```

**Mapping Summary**: 1 Obligation → 1 ActionDefinition (intro) + N SceneTemplates (phases) + 1 InvestigationArc (metadata)

### 6.4 MemoryFlag → Card Mapping

**Mapping Strategy**: 1:1 replacement with semantic transformation

1. **MemoryFlag.Key → Card.Id**: Unique identifier becomes card ID
2. **MemoryFlag.Description → Card.Description**: Narrative description becomes card description
3. **Memory Check Context → Card.Contexts**: Where HasMemory() was called determines CardContext
4. **Unlocked Content → Card.Effect.TriggeredSceneId**: Content unlocked by memory becomes scene spawned by card

**Example Migration**:

```
OLD:
// Action grants memory
Player.AddMemory("elena_fire", "Helped Elena during fire", currentDay, importance: 5)

// Conversation checks memory
if (Player.HasMemory("elena_fire")) {
  // Show "Remind Elena" conversation option
  Goal "Remind Elena of Fire"
    SystemType: Social
    ConsequenceType: Grant
    Rewards: +4 Elena cubes
}

NEW:
// Action grants card
Card "elena_remembers_fire"
  Title: "Elena Remembers the Fire"
  Description: "You helped Elena during the fire. She won't forget this."
  Contexts:
    - CardContextType: SocialChallenge
      ContextFilter: "elena" (appears only during Elena social challenges)
  Effect:
    MomentumBonus: 4 (direct tactical benefit)
    TriggeredSceneId: "elena_deep_gratitude" (optional: spawn special scene)
  Persistence: Permanent

// Action completion grants card
ActionDefinition.Outputs.GrantedCards: ["elena_remembers_fire"]

// During Elena conversation (Social challenge):
// - Card automatically appears in hand (context matches)
// - Player can play for +4 Momentum
// - If TriggeredSceneId set, playing also spawns "Elena's Deep Gratitude" scene
```

**Mapping Summary**: 1 MemoryFlag → 1 Card (1:1, semantic transformation)

### 6.5 Migration Complexity Assessment

**Straightforward Migrations**:
- MemoryFlag → Card (1:1, semantic clear)
- Goal with GoalCards → SceneTemplate (1:1, structure similar)

**Complex Migrations**:
- Obstacle with Goals → ActionDefinitions (1:many, hierarchical → flat)
- Obligation with Phases → Scene Chain + Arc (1:many+1, phase structure → chaining)

**Lossy Migrations**:
- Goal.DifficultyModifiers → ??? (graduated difficulty LOST)
- Obstacle.Intensity gating → ??? (property-based gating LOST)
- Obstacle Modify consequence → ??? (graduated weakening LOST)

**Decision Points**:
1. **Accept graduated difficulty loss?** (Scene has fixed entry cost)
2. **Accept property-based gating loss?** (Scene has resource cost, not property threshold)
3. **Accept weakening chain loss?** (ActionDefinitions are peers, not hierarchical with shared state)

---

## Part 7: Architectural Recommendations

### 7.1 Hybrid Architecture Proposal

**CRITICAL OBSERVATION**: Complete replacement loses valuable patterns. Recommend **HYBRID ARCHITECTURE** that preserves strengths of both systems.

**Hybrid Model**:

1. **Keep Obstacle + Goal for Strategic Challenges**:
   - Obstacle represents "strategic barrier with multiple approaches and graduated weakening"
   - Goal represents "tactical approach with difficulty modifiers and consequence types"
   - Preserve Intensity-based gating and property reduction
   - Use for structured challenge content (tutorial, designed obstacles)

2. **Add Scene + Card for Dynamic Content**:
   - Scene represents "ephemeral narrative opportunity spawned by actions"
   - Card represents "playable knowledge resource"
   - Use for investigation chains, relationship moments, random events
   - Use for AI-generated content and procedural quests

3. **Integration Points**:
   - **Goal can spawn Scene**: GoalCard.Rewards.SpawnedSceneId creates scene when goal completed
   - **Scene can spawn Goal**: Scene.Outputs.SpawnedGoals creates goal at location (rare, for tutorial)
   - **Scene can grant Card**: Scene.Outputs.GrantedCards adds cards to player collection
   - **Card can reduce Goal difficulty**: Card in collection modifies Goal.DifficultyModifiers (expertise cards)
   - **Obstacle cleared can spawn Scene**: Obstacle completion creates celebration/consequence scene

**Why Hybrid?**:
- **Preserves**: Graduated difficulty, property-based gating, weakening chains, multiple approaches
- **Adds**: Dynamic spawning, expiration lifecycle, playable knowledge, scene chaining
- **Flexibility**: Use Goal/Obstacle for designed challenges, Scene for dynamic content
- **Migration**: No lossy transformation required, additive architecture

### 7.2 Pure Scene Architecture (If No Hybrid)

**If commitment to PURE Scene architecture** (eliminating Goal/Obstacle entirely):

**Required New Entities**:

1. **Challenge (Replaces Obstacle)**:
```csharp
public class Challenge
{
  string Id
  string Name
  string Description
  string LocationId
  List<string> ApproachActionIds (ActionDefinitions that resolve this challenge)
  ChallengeState State (Active/Resolved)
  // When any approach completes, State = Resolved, remove all ApproachActions
}
```

2. **ApproachAction (Replaces Goal)**:
```csharp
public class ApproachAction : ActionDefinition
{
  string ChallengeId (which challenge this resolves)
  // Inherits all ActionDefinition properties
  // Outputs.RemovedActions includes all peer approaches
  // Outputs.ChallengeStateChange marks Challenge as Resolved
}
```

3. **ExpertiseModifier (Replaces DifficultyModifier)**:
```csharp
public class ExpertiseModifier
{
  ExpertiseType Type (Understanding/Mastery/Familiarity)
  int Threshold
  int CostReduction (reduces Scene.EntryCost when threshold met)
  // Applied when displaying Scene costs
  // Understanding 2 → reduce Focus cost by 3
  // Understanding 5 → reduce Focus cost by 6
  // Graduated via multiple modifiers
}
```

**Graduated Difficulty Pattern**:
```
Scene "Complex Investigation"
  EntryCost: { Focus: 20 }
  ExpertiseModifiers:
    - Type: Understanding, Threshold: 2, CostReduction: 3
    - Type: Understanding, Threshold: 5, CostReduction: 6
    - Type: Familiarity, Threshold: 2, CostReduction: 2

Display Logic:
  baseCost = 20 Focus
  if Player.Understanding >= 5: baseCost -= 6 (cumulative best modifier)
  elif Player.Understanding >= 2: baseCost -= 3
  if Player.GetLocationFamiliarity(sceneLocationId) >= 2: baseCost -= 2
  displayedCost = baseCost
```

**Property-Based Gating Pattern**:
```
Challenge "Locked Gate"
  State: Active
  ApproachActionIds: ["force_gate", "pick_lock", "bribe_guard"]

ApproachAction "force_gate"
  ChallengeId: "locked_gate"
  DisplayCondition: Challenge.State == Active
  Outputs:
    ChallengeStateChange: { ChallengeId: "locked_gate", State: Resolved }
    RemovedActions: ["force_gate", "pick_lock", "bribe_guard"]
    SpawnedScenes: ["beyond_gate"]
```

**Graduated Weakening Pattern** (COMPLEX, may not be worth it):
```
Challenge "Fortified Gate"
  State: Active
  Intensity: 3
  ApproachActionIds: ["weaken_gate_1", "weaken_gate_2", "force_weakened_gate"]

ApproachAction "weaken_gate_1"
  ChallengeId: "fortified_gate"
  DisplayCondition: Challenge.Intensity > 0
  Outputs:
    ChallengePropertyChange: { ChallengeId: "fortified_gate", ReduceIntensity: 1 }
    // After this completes, Intensity = 2

ApproachAction "weaken_gate_2"
  ChallengeId: "fortified_gate"
  DisplayCondition: Challenge.Intensity >= 2
  Outputs:
    ChallengePropertyChange: { ChallengeId: "fortified_gate", ReduceIntensity: 1 }
    // After this completes, Intensity = 1

ApproachAction "force_weakened_gate"
  ChallengeId: "fortified_gate"
  DisplayCondition: Challenge.Intensity <= 1
  Costs: { Stamina: 5 }
  Outputs:
    ChallengeStateChange: { ChallengeId: "fortified_gate", State: Resolved }
    RemovedActions: ["weaken_gate_1", "weaken_gate_2", "force_weakened_gate"]
    SpawnedScenes: ["beyond_gate"]
```

**This is COMPLEX**. Graduated weakening requires Challenge entity with Intensity property + DisplayCondition checks + PropertyChange outputs. Essentially recreating Obstacle under different name.

### 7.3 Final Recommendation

**RECOMMENDATION: Hybrid Architecture**

**Reasoning**:
1. **Goal/Obstacle patterns have value**: Graduated difficulty, property-based gating, weakening chains create strategic depth
2. **Scene/Card patterns have value**: Dynamic spawning, expiration, playable knowledge eliminate boolean gates
3. **Pure Scene architecture loses too much**: Recreating Obstacle as Challenge + ApproachAction is complex and gains nothing
4. **Hybrid is additive**: No lossy migration, both systems coexist, use each where appropriate

**Implementation Strategy**:
1. **Phase 1**: Implement Scene + Card + InvestigationArc entities alongside existing Goal/Obstacle
2. **Phase 2**: Migrate MemoryFlag → Card (complete replacement, no loss)
3. **Phase 3**: Add Scene spawning to existing Goal outputs (GoalCard.Rewards.SpawnedSceneId)
4. **Phase 4**: Create new investigation content using Scene chains (prove scene chaining pattern)
5. **Phase 5**: Evaluate if Goal/Obstacle still needed, or Scene pattern sufficient for new content

**Long-Term Vision**:
- **Designed Challenges**: Use Goal/Obstacle (tutorial, boss battles, climactic moments)
- **Dynamic Content**: Use Scene (investigation chains, relationship moments, procedural quests)
- **Knowledge System**: Use Card exclusively (eliminates all MemoryFlag boolean gates)
- **Both systems reference shared resources**: Cards can appear in Goals OR Scenes, equipment reduces costs in Goals OR Scenes

---

## Part 8: Summary Tables

### 8.1 Entity Property Count Comparison

| Entity Type | Old Architecture | New Architecture |
|-------------|-----------------|------------------|
| **Challenge Container** | Obstacle (10 properties) | Challenge (5 properties) OR eliminate |
| **Tactical Approach** | Goal (19 properties + 4 inline) | Scene (21 properties + 3 inline) |
| **Multi-Phase Arc** | Obligation (14 properties + nested) | InvestigationArc (8 properties) |
| **Player Knowledge** | MemoryFlag (5 properties) | Card (9 properties) |
| **Supporting** | GoalCard, GoalCosts, DifficultyModifier, ObstaclePropertyReduction (13 total) | SceneTemplate, ActionDefinition, CardContext, CardEffect (19 total) |

### 8.2 Mapping Summary

| Source | Destination | Ratio | Notes |
|--------|-------------|-------|-------|
| Goal → | SceneTemplate | 1:1 | If standalone goal |
| Obstacle + Goals → | ActionDefinitions + Scene | 1:N+1 | Multiple approaches to barrier |
| Obligation → | InvestigationArc + Scenes | 1:1+N | Arc tracks chain |
| MemoryFlag → | Card | 1:1 | Semantic transformation |

### 8.3 Feature Preservation Matrix

| Feature | Current System | Pure Scene | Hybrid | Notes |
|---------|---------------|------------|--------|-------|
| **Multiple Approaches** | Obstacle + Goals | Challenge + Actions | Goal + Obstacle | Preserved all |
| **Graduated Difficulty** | DifficultyModifier | ExpertiseModifier | Both | Scene uses ExpertiseModifier |
| **Property-Based Gating** | Obstacle.Intensity | Challenge.Intensity OR none | Obstacle.Intensity | Pure Scene loses or recreates |
| **Weakening Chain** | Goal reduces Intensity | ApproachAction reduces Intensity OR none | Goal reduces Intensity | Complex in Pure Scene |
| **Dynamic Spawning** | None | Scene.SpawnedScenes | Scene.SpawnedScenes | New capability |
| **Expiration Lifecycle** | DeleteOnSuccess | Scene.ExpirationType | Both | Scene more sophisticated |
| **Playable Knowledge** | None | Card.Effect | Card.Effect | New capability |
| **Commissioned Pressure** | Obligation | InvestigationArc | Both | Preserved |
| **Scene Chaining** | Obligation phases | Scene.SpawnedScenes | Scene.SpawnedScenes | Better in Scene |
| **Boolean Gates** | HasMemory | Eliminated (Card) | Eliminated (Card) | Good riddance |

---

## Conclusion

**The transformation from Goal/Obstacle/Obligation to Scene/Card is NOT a simple 1:1 replacement.** It represents a fundamental architectural paradigm shift from **hierarchical challenge structures with property-based gating** to **flat content spawning with resource-based economics**.

**Key Semantic Gaps**:
1. Goal is "approach to barrier", Scene is "standalone opportunity"
2. Obstacle is "strategic container with graduated weakening", no Scene equivalent
3. Obligation is "multi-phase arc with commissioned pressure", InvestigationArc preserves core but loses phase formalism
4. MemoryFlag is "boolean gate", Card is "playable resource" (complete semantic transformation)

**What Pure Scene Architecture Loses**:
- Graduated difficulty modifiers (expertise-based cost reduction requires new ExpertiseModifier)
- Property-based gating (Intensity threshold gates visibility)
- Obstacle weakening chain (completing Goal A makes Goal B easier)
- Hierarchical lifecycle (parent cleared → children removed)
- Multiple approaches to shared barrier (requires Challenge + ApproachAction recreation)

**What Scene Architecture Gains**:
- Dynamic content spawning (actions create content)
- Expiration lifecycle (content can be missed)
- Scene chaining (scenes spawn scenes, branching paths)
- Playable knowledge (cards for tactical effect)
- Eliminates boolean gates (no more if/then unlocks)

**Architectural Recommendation**: **HYBRID** architecture that uses Goal/Obstacle for structured challenges and Scene/Card for dynamic content. This preserves valuable existing patterns while adding dynamic content capabilities, avoiding lossy migration and architectural complexity of recreating Obstacle as Challenge.

**Clean Migration Path** (if pure Scene architecture required):
1. MemoryFlag → Card (complete replacement, semantic transformation)
2. Obligation → InvestigationArc + Scene chain (phase structure → chaining)
3. Obstacle + Goals → Challenge + ApproachActions (recreate container pattern)
4. Accept loss of graduated weakening OR implement complex Challenge.Intensity system

**End of Analysis**

---

## Appendix: Absolute File Paths

All entity files analyzed:
- `C:\Git\Wayfarer\DYNAMIC_CONTENT_PRINCIPLES.md`
- `C:\Git\Wayfarer\src\GameState\Goal.cs`
- `C:\Git\Wayfarer\src\GameState\Obstacle.cs`
- `C:\Git\Wayfarer\src\GameState\Obligation.cs`
- `C:\Git\Wayfarer\src\GameState\Player.cs`
- `C:\Git\Wayfarer\src\Content\Location.cs`
- `C:\Git\Wayfarer\src\GameState\GameWorld.cs`
- `C:\Git\Wayfarer\src\GameState\MemoryFlag.cs`
- `C:\Git\Wayfarer\src\GameState\Cards\GoalCard.cs`
- `C:\Git\Wayfarer\src\GameState\DifficultyModifier.cs`
- `C:\Git\Wayfarer\src\GameState\GoalCosts.cs`
- `C:\Git\Wayfarer\src\GameState\ObstaclePropertyReduction.cs`
