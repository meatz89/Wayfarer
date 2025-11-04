# Wayfarer Tutorial: Complete Implementation Plan

## Table of Contents
1. [Tutorial Design Overview](#tutorial-design-overview)
2. [Complete Content Specification](#complete-content-specification)
3. [UI Flow and Player Experience](#ui-flow-and-player-experience)
4. [Scene Spawning Architecture](#scene-spawning-architecture)
5. [Challenge System Integration](#challenge-system-integration)
6. [AutoAdvance Pattern Implementation](#autoadvance-pattern-implementation)
7. [Executor Integration](#executor-integration)
8. [Time System Integration](#time-system-integration)
9. [Resource Management](#resource-management)
10. [Testing Strategy](#testing-strategy)
11. [Implementation Sequence](#implementation-sequence)

---

## 1. Tutorial Design Overview

### Purpose
Teach resource scarcity and work safety net through ONE critical decision: How to earn 2 coins for shelter?

### Duration
- **Start**: Day 1, Evening Block, Segment 1 (13th of 24 segments)
- **End**: Day 2, Morning Block, Segment 1 (1st of 24 segments)
- **Total Time**: ~8-10 segments of gameplay

### Core Loop Demonstrated
Hub (Tavern) → Decision Point (Work vs Risk) → Challenge/Consequence → Recovery → Day 2 World Opens

### Learning Objectives
1. **Resource Scarcity**: Starting 8 coins < 10 needed for room = forced decision
2. **Work Safety Net**: Social OR Physical challenge earns 5 coins = safety
3. **Risk vs Reward**: Sleep rough saves coins, costs health
4. **Resource Pools**: Health/Focus/Stamina are 6-point pools, not arbitrary numbers
5. **Challenge Types**: Social costs Focus, Physical costs Stamina, both have distinct gameplay
6. **Rest Mechanics**: Paying for lodging restores ALL resources to maximum
7. **Cascading Consequences**: Early choices affect Day 2 starting state

### Tutorial Scenes Flow
```
Scene 1: Evening Arrival
    ├─ Choice A: Elena's Evening Service (Social Challenge)
    │   └─> Scene 2: Secure Room
    ├─ Choice B: Thomas's Warehouse Work (Physical Challenge)
    │   └─> Scene 2: Secure Room
    └─ Choice C: Sleep Rough
        └─> Scene 2B: Rough Sleep Recovery (alternate path)

Scene 2: Secure Room
    └─ Choice: Pay 10 Coins for Room
        └─> Scene 3: Night Rest

Scene 3: Night Rest (AutoAdvance)
    └─> Scene 4: Day 2 Morning

Scene 4: Day 2 Morning (WorldStateReveal)
    └─> Tutorial Complete, Full Game Begins
```


### 2.4 Content Package Structure

```
src/Content/Tutorial/
├── 01_tutorial_scenes.json      (6 scene templates)
├── 02_tutorial_npcs.json         (2 NPCs: Elena, Thomas)
├── 03_tutorial_locations.json    (5 locations: Tavern + children, Town Square, Warehouse)
└── package_manifest.json         (declares tutorial package, load order)
```

### 2.5 Initial Player State Configuration

**File**: `src/Content/Tutorial/04_tutorial_player_config.json`

```json
{
  "playerInitialState": {
    "coins": 8,
    "health": 6,
    "maxHealth": 6,
    "focus": 6,
    "maxFocus": 6,
    "stamina": 6,
    "maxStamina": 6,
    "hunger": 40,
    "maxHunger": 100,
    "currentPosition": { "q": 0, "r": 0 },
    "currentTimeBlock": "Evening",
    "currentTimeSegment": 1,
    "currentDay": 1
  }
}
```

---

## 3. UI Flow and Player Experience

### 3.1 Game Start Screen (Existing)

**What Player Sees:**
- Game title screen
- "New Game" button
- "Continue" button (disabled if no save)
- "Settings" button

**Player Action**: Clicks "New Game"

**System Response:**
- GameWorldInitializer.InitializeNewGame()
- Loads Tutorial package
- Sets player initial state (8 coins, 6/6/6 resources, Evening Segment 1)
- Spawns starter scenes (Scene 1: Evening Arrival)
- Transitions to GameScreen

### 3.2 Tutorial Scene 1: Evening Arrival

#### Screen Layout

```
╔═══════════════════════════════════════════════════════════╗
║  CROSSROADS TAVERN - COMMON ROOM                          ║
║  Evening, Segment 1                                        ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  [RESOURCES BAR]                                          ║
║  ⚕️ Health: ●●●●●● (6/6)                                   ║
║  🧠 Focus: ●●●●●● (6/6)                                    ║
║  💪 Stamina: ●●●●●● (6/6)                                  ║
║  🍞 Hunger: 40/100                                         ║
║  💰 Coins: 8                                               ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  Elena Crossroads (TAVERN KEEPER)                         ║
║                                                            ║
║  Elena wipes down tables while watching travelers          ║
║  settle in for the evening. She glances at you as you      ║
║  enter.                                                    ║
║                                                            ║
║  'Need a room?' she asks. 'Ten coins. Standard rate.'     ║
║                                                            ║
║  You have 8 coins. Night is approaching—you can see        ║
║  the sky darkening through the windows. Other travelers    ║
║  are already claiming tables and rooms.                    ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║  CHOICES:                                                  ║
║                                                            ║
║  [1] Offer to Help Elena with Evening Service             ║
║      💭 Social challenge                                   ║
║      Cost: 1 Focus, 1 Time Segment                        ║
║      Reward: 5 Coins, +2 Health, -10 Hunger, Elena        ║
║              becomes Friendly                              ║
║                                                            ║
║  [2] Go to Thomas at the Warehouse for Work               ║
║      💪 Physical challenge                                 ║
║      Cost: 1 Stamina, 1 Time Segment                      ║
║      Reward: 5 Coins, +5 Hunger, Thomas becomes Friendly  ║
║                                                            ║
║  [3] Sleep Rough in Town Square                           ║
║      ⚠️ Risk health damage                                 ║
║      Cost: 6 Time Segments (night passes)                 ║
║      Reward: -2 Health, Elena disappointed                ║
║                                                            ║
╚═══════════════════════════════════════════════════════════╝
```

**UI Elements:**
- Resource bar at top (ALWAYS VISIBLE - critical for tutorial)
- NPC portrait (Elena)
- Narrative text (placeholders replaced: {NPCName} → Elena Crossroads, {CoinsCurrent} → 8)
- Choice cards (Sir Brante pattern: costs VISIBLE, rewards VISIBLE for tutorial transparency)
- Choice cards show icons for challenge type (💭 Social, 💪 Physical)

**Player Experience:**
- Reads narrative
- Sees they have 8 coins but need 10
- Sees two work options (both give 5 coins) with different resource costs
- Sees risk option (sleep rough) that saves coins but costs health
- Must choose: Spend Focus OR Stamina to earn coins, OR risk health to save coins

### 3.3 Tutorial Scene 1: Elena Choice Selected

**Player Action**: Clicks choice [1] "Offer to Help Elena with Evening Service"

**System Response:**
1. **Cost Application** (IMMEDIATE):
   - Focus: 6 → 5 (1 consumed)
   - Time: Evening Segment 1 → Segment 2
   - UI updates resource bar

2. **Challenge Initiation**:
   - System checks ActionType = StartChallenge
   - ChallengeType = Social
   - ChallengeFacade.StartSocialChallenge(challengeDefinitionId)
   - Screen transitions to SocialChallenge screen

### 3.4 Social Challenge Screen (Elena's Evening Service)

#### Screen Layout

```
╔═══════════════════════════════════════════════════════════╗
║  SOCIAL CHALLENGE: Evening Service                        ║
║  Elena Crossroads (Bond: 0 → 5 Friendly)                  ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  [CHALLENGE RESOURCES]                                    ║
║  🎯 Initiative: 3                                          ║
║  ⚡ Momentum: 0                                            ║
║  💭 Understanding: 0/15 (Goal)                             ║
║  ⚠️ Doubt: 0/10 (Threshold)                               ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  Elena hands you a tray. 'Start with table three.         ║
║  Listen more than you talk. Learn what people need.'      ║
║                                                            ║
║  You move through the common room, taking orders,          ║
║  delivering meals, reading the room...                     ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║  YOUR HAND: (3 cards)                                      ║
║                                                            ║
║  [Simple Question] [Friendly Remark] [Observation]        ║
║    Initiative: 1      Initiative: 2      Initiative: 0    ║
║    +2 Understanding   +2 Momentum       +1 Understanding  ║
║                                                            ║
╚═══════════════════════════════════════════════════════════╝
```

**Challenge Flow:**
1. Draw initial hand (3 cards)
2. Play cards to build Understanding (goal: 15)
3. Manage Initiative (spending resource)
4. Manage Momentum (tactical advantage)
5. Avoid Doubt buildup (threshold: 10)
6. Reach 15 Understanding → Challenge complete

**Completion:**
- Narrative: "Elena nods as you finish. 'You listen. That's rare. Here—' She counts five coins onto the bar. 'And eat something before you go upstairs. You've earned it.'"
- Rewards Applied:
  - Coins: 8 → 13 (+5)
  - Health: 6 → 6 (already max, but +2 shown)
  - Hunger: 40 → 30 (-10)
  - Elena Bond: 0 → 5 (Neutral → Friendly)
- Scene 2 Spawns: "Secure Room" at Elena (TAVERN_KEEPER NPC)
- Screen returns to Location screen (Common Room)

### 3.5 Tutorial Scene 2: Secure Room

#### Screen Layout

```
╔═══════════════════════════════════════════════════════════╗
║  CROSSROADS TAVERN - COMMON ROOM                          ║
║  Evening, Segment 2                                        ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  [RESOURCES BAR]                                          ║
║  ⚕️ Health: ●●●●●● (6/6)                                   ║
║  🧠 Focus: ●●●●● (5/6)      ⬅️ DEPLETED                    ║
║  💪 Stamina: ●●●●●● (6/6)                                  ║
║  🍞 Hunger: 30/100          ⬅️ IMPROVED                    ║
║  💰 Coins: 13               ⬅️ EARNED                      ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  Elena Crossroads (TAVERN KEEPER) [Friendly]             ║
║                                                            ║
║  Elena counts your coins on the bar.                       ║
║                                                            ║
║  'Ten coins,' she says. 'Upstairs, second door on the      ║
║  left. I'll have someone bring up water for washing.'     ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║  CHOICES:                                                  ║
║                                                            ║
║  [1] Pay for the Room (10 Coins)                          ║
║      Cost: 10 Coins                                        ║
║      Requirement: ✅ Have 10+ coins (you have 13)          ║
║                                                            ║
╚═══════════════════════════════════════════════════════════╝
```

**Player Experience:**
- Sees updated resources (Focus depleted, Coins earned, Hunger improved)
- Elena now shows [Friendly] status
- One choice available (requirement met)
- Clear cause-effect: Worked → Earned coins → Can afford room

**Player Action**: Clicks [1] "Pay for the Room"

**System Response:**
1. Cost Applied:
   - Coins: 13 → 3 (-10)
2. Scene 3 Spawns: "Night Rest" at Upstairs Bedroom location
3. Screen transitions to Bedroom

### 3.6 Tutorial Scene 3: Night Rest (AutoAdvance)

#### Screen Layout

```
╔═══════════════════════════════════════════════════════════╗
║  CROSSROADS TAVERN - UPSTAIRS BEDROOM                     ║
║  Evening → Night → Morning (TIME ADVANCING...)             ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  [RESOURCES BAR - ANIMATING RESTORATION]                  ║
║  ⚕️ Health: ●●●●●● (6/6) ✨                                ║
║  🧠 Focus: ●●●●●● (5/6 → 6/6) ✨                           ║
║  💪 Stamina: ●●●●●● (6/6) ✨                               ║
║  🍞 Hunger: 30/100                                         ║
║  💰 Coins: 3                                               ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  The bed is simple but clean. You wash off the road        ║
║  dust and collapse into the mattress.                      ║
║                                                            ║
║  Night passes. You sleep deeply—the first real bed in      ║
║  days. No nightmares, no disturbances.                     ║
║                                                            ║
║  Morning light filters through the window. You hear the    ║
║  town waking up below: cart wheels, vendor calls, the      ║
║  day beginning.                                            ║
║                                                            ║
║  You feel restored. Ready.                                 ║
║                                                            ║
║  [AUTO-ADVANCING TO MORNING...]                            ║
║                                                            ║
╚═══════════════════════════════════════════════════════════╝
```

**AutoAdvance Behavior:**
- NO player input required
- Narrative displays for 3-5 seconds (readable time)
- Resources animate to maximum:
  - Focus: 5 → 6 (restored)
  - Stamina: 6 → 6 (already max)
  - Health: 6 → 6 (already max)
- Time advances:
  - Evening Segment 2 → Night Block → Morning Block Segment 1
  - Day 1 → Day 2
- Scene 4 spawns: "Day 2 Morning" at Common Room
- Screen transitions automatically

### 3.7 Tutorial Scene 4: Day 2 Morning (WorldStateReveal)

#### Screen Layout

```
╔═══════════════════════════════════════════════════════════╗
║  CROSSROADS TAVERN - COMMON ROOM                          ║
║  Day 2, Morning, Segment 1                                 ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  [RESOURCES BAR]                                          ║
║  ⚕️ Health: ●●●●●● (6/6)       ✅ FULL                     ║
║  🧠 Focus: ●●●●●● (6/6)        ✅ RESTORED                 ║
║  💪 Stamina: ●●●●●● (6/6)      ✅ FULL                     ║
║  🍞 Hunger: 30/100                                         ║
║  💰 Coins: 3                   ⚠️ LOW                      ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║                                                            ║
║  Elena Crossroads (TAVERN KEEPER) [Friendly]             ║
║                                                            ║
║  You descend to the common room. Elena is already          ║
║  working, preparing breakfast for the morning crowd.       ║
║                                                            ║
║  'Sleep well?' she asks without looking up. 'Good. If      ║
║  you're looking for work, Thomas mentioned he might have   ║
║  steady jobs at the warehouse—if you proved reliable       ║
║  yesterday.'                                               ║
║                                                            ║
║  She nods toward the job board near the door. 'There's     ║
║  also talk of trouble at the old mill. Something about     ║
║  the waterwheel. Could be worth investigating if you're    ║
║  the curious type.'                                        ║
║                                                            ║
║  The day stretches ahead. The town is open. Choices to     ║
║  make.                                                     ║
║                                                            ║
╠═══════════════════════════════════════════════════════════╣
║  [TUTORIAL COMPLETE]                                       ║
║                                                            ║
║  You've learned:                                           ║
║  ✅ Work safety net (Social/Physical challenges earn      ║
║     coins)                                                 ║
║  ✅ Resource management (Focus/Stamina are limited)       ║
║  ✅ Rest mechanics (lodging restores all resources)       ║
║  ✅ Consequence system (choices affect NPC relationships) ║
║                                                            ║
║  [CONTINUE] ───────────────────────────────────────────>  ║
║                                                            ║
╚═══════════════════════════════════════════════════════════╝
```

**WorldStateReveal Behavior:**
- NO choices presented (narrative only)
- Exposition reveals Day 2 systems:
  - Delivery obligations (job board)
  - Investigations (mill mystery)
  - Repeatable work (warehouse)
- Tutorial summary displayed
- [CONTINUE] button → transitions to full game (location actions appear)

**Player Experience:**
- Sees resources fully restored (teaches rest value)
- Has 3 coins left (teaches ongoing resource pressure)
- Elena mentions Thomas (teaches consequences persist)
- Job board and mill teased (teaches world systems exist)
- Tutorial complete message → confidence to proceed

---

## 4. Scene Spawning Architecture

### 4.1 Starter Scene Initialization

**When**: GameWorldInitializer.InitializeNewGame() is called

**Process**:
1. Load all SceneTemplates from JSON
2. Filter for `isStarter: true` templates
3. For each starter template:
   - Evaluate PlacementFilter
   - Query GameWorld for matching entities
   - Instantiate Scene at first match
   - Add to GameWorld.Scenes (active immediately)

**Code Flow** (conceptual):
```
GameWorldInitializer.InitializeNewGame()
  └─> ContentLoader.LoadSceneTemplates()
  └─> foreach (sceneTemplate where isStarter == true)
        └─> PlacementFilterEvaluator.FindMatchingEntity(sceneTemplate.PlacementFilter, gameWorld)
            └─> Returns: Elena (NPC with role TAVERN_KEEPER)
        └─> SceneInstantiator.InstantiateScene(sceneTemplate, placementEntity)
            └─> Creates Scene with placementNpcId = "npc_elena_tavern_keeper"
            └─> Instantiates SituationTemplates as Situations (state = Dormant)
        └─> gameWorld.Scenes.Add(scene)
```

**Result**: Scene 1 (Evening Arrival) exists in GameWorld.Scenes, placed at Elena

### 4.2 PlacementFilter Evaluation Logic

**Purpose**: Find concrete entity matching categorical filter

**PlacementFilter Fields**:
- `placementType`: NPC | Location | Route
- **NPC filters**:
  - `npcRole`: Enum value (TAVERN_KEEPER, WAREHOUSE_FOREMAN, etc.)
  - `personalityTypes`: List<string> (ANY match)
  - `minBondStrength`: int (player bond must be ≥)
  - `maxBondStrength`: int? (player bond must be ≤, null = no limit)
  - `npcTags`: List<string> (ALL must match)
  - `requiredStates`: List<string> (NPC must have all)
  - `forbiddenStates`: List<string> (NPC must NOT have any)
- **Location filters**:
  - `locationRole`: Enum value (TAVERN_COMMON_ROOM, etc.)
  - `locationProperties`: List<string> (ALL must match)
  - `locationTags`: List<string> (ANY match)
  - `districtId`: string? (must be in district)
  - `regionId`: string? (must be in region)
- **Route filters**:
  - `routeRole`: Enum value (TAVERN_TO_MARKETPLACE, etc.)
  - `segmentPosition`: Start | Middle | End | null
  - `terrainTypes`: List<TerrainType> (ANY match)
  - `routeTier`: int? (exact match)
  - `minDangerRating`: int (route danger ≥)
  - `maxDangerRating`: int (route danger ≤)

**Evaluation Algorithm**:
```
PlacementFilterEvaluator.FindMatchingEntity(filter, gameWorld):
  if (filter.placementType == NPC):
    candidates = gameWorld.NPCs

    if (filter.npcRole != null):
      candidates = candidates.Where(npc => npc.Role == filter.npcRole)

    if (filter.personalityTypes.Any()):
      candidates = candidates.Where(npc =>
        npc.PersonalityTypes.Any(p => filter.personalityTypes.Contains(p)))

    if (filter.minBondStrength.HasValue):
      candidates = candidates.Where(npc =>
        gameWorld.Player.GetBondStrength(npc.Id) >= filter.minBondStrength)

    if (filter.maxBondStrength.HasValue):
      candidates = candidates.Where(npc =>
        gameWorld.Player.GetBondStrength(npc.Id) <= filter.maxBondStrength)

    return candidates.FirstOrDefault()

  else if (filter.placementType == Location):
    // Similar logic for Location filters

  else if (filter.placementType == Route):
    // Similar logic for Route filters
```

**Example - Scene 1 Placement**:
```
PlacementFilter: { placementType: NPC, npcRole: TAVERN_KEEPER, minBondStrength: 0, maxBondStrength: 0 }

Evaluation:
  gameWorld.NPCs.Where(npc => npc.Role == "TAVERN_KEEPER")
    → [Elena] (only NPC with this role)

  Elena.BondStrength = 0 (neutral, first meeting)

  Filter requires minBondStrength: 0, maxBondStrength: 0
    → Elena matches (bond exactly 0)

  Return: Elena
```

### 4.3 Scene Instantiation Process

**Process**: SceneTemplate → Scene (instance)

**Steps**:
1. **Create Scene entity**:
   - SceneId: Generated GUID
   - TemplateId: Reference to SceneTemplate
   - PlacementType: From filter (NPC/Location/Route)
   - PlacementId: Concrete entity ID (e.g., "npc_elena_tavern_keeper")
   - State: Provisional OR Active (depending on spawn type)
   - DisplayName: PlaceholderReplacer.Replace(template.DisplayNameTemplate, context)

2. **Instantiate Situations**:
   - For each SituationTemplate in SceneTemplate:
     - Create Situation entity
     - SituationId: Generated GUID
     - TemplateId: Reference to SituationTemplate
     - SceneId: Parent scene ID
     - State: Dormant (not active until player enters context)
     - Narrative: PlaceholderReplacer.Replace(template.NarrativeTemplate, context)
     - ChoiceTemplates: Store reference (not instantiated yet)

3. **Add to GameWorld**:
   - If spawn type = Starter or Immediate: gameWorld.Scenes.Add(scene)
   - If spawn type = Provisional: gameWorld.ProvisionalScenes.Add(scene)

**PlaceholderReplacer Context**:
- `{NPCName}`: gameWorld.NPCs[scene.PlacementId].Name
- `{LocationName}`: gameWorld.Locations[scene.PlacementId].Name
- `{PlayerName}`: gameWorld.Player.Name
- `{CoinsCurrent}`: gameWorld.Player.Coins.ToString()
- `{HealthCurrent}`: gameWorld.Player.Health.ToString()
- `{FocusCurrent}`: gameWorld.Player.Focus.ToString()
- `{StaminaCurrent}`: gameWorld.Player.Stamina.ToString()

### 4.4 Reward-Based Scene Spawning

**Trigger**: Player selects Choice with SceneSpawnReward in ChoiceReward

**Process**:
1. **Choice Execution**:
   - Executor applies costs (coins, resources, time)
   - Executor applies immediate rewards (coins, resources)
   - Executor detects ScenesToSpawn in ChoiceReward

2. **Provisional Scene Creation** (Perfect Information):
   - For each SceneSpawnReward:
     - Load SceneTemplate by sceneTemplateId
     - Determine placement:
       - SameNPC: Use current NPC
       - SameLocation: Use current Location
       - SpecificNPC: Use specificPlacementId
       - SpecificLocation: Use specificPlacementId
     - SceneInstantiator.CreateProvisionalScene(template, placementId)
     - Add to gameWorld.ProvisionalScenes
     - UI shows WHERE scene will spawn

3. **Player Commit/Abandon**:
   - **Commit**: SceneInstantiator.FinalizeScene(provisionalSceneId)
     - Moves Scene from ProvisionalScenes to Scenes
     - Scene becomes active
   - **Abandon**: SceneInstantiator.DeleteProvisionalScene(provisionalSceneId)
     - Removes Scene from ProvisionalScenes
     - No trace left

**Example - Scene 1 Choice 1**:
```
Player selects: "Offer to Help Elena with Evening Service"

ChoiceReward contains:
  scenesToSpawn: [{
    sceneTemplateId: "scene_secure_room",
    placementRelation: "SameNPC",
    spawnTiming: "Immediate"
  }]

Executor:
  1. Applies costs (Focus: 6 → 5, Time: +1 segment)
  2. Starts Social Challenge
  3. [Challenge completes]
  4. Applies rewards (Coins: +5, Health: +2, Hunger: -10, Bond: +5)
  5. Creates provisional Scene:
       - Template: scene_secure_room
       - Placement: NPC (Elena, same as current)
       - State: Provisional
  6. UI shows: "New scene available: Secure Room (Elena)"
  7. Player commits (Challenge completed successfully)
  8. Finalize Scene:
       - Move to gameWorld.Scenes
       - Activate situations
```

### 4.5 Situation Activation (Query-Time Pattern)

**Trigger**: Player enters location/NPC context where Scene exists

**Process**:
1. **SceneFacade.GetActionsAtLocation(locationId)**:
   - Query: gameWorld.Scenes.Where(s => s.PlacementType == Location && s.PlacementId == locationId)
   - For each Scene:
     - Get situations where State == Dormant
     - Transition State: Dormant → Active
     - For each ChoiceTemplate in Situation:
       - Create ephemeral LocationAction entity
       - ActionId: Generated GUID
       - ChoiceTemplateId: Reference to ChoiceTemplate
       - SituationId: Parent situation ID
       - Evaluate requirements (CompoundRequirement.IsAnySatisfied)
       - Set IsAvailable flag
     - Return List<LocationAction>

2. **SceneFacade.GetActionsForNPC(npcId)**:
   - Same logic but filter by NPC placement
   - Create NPCAction entities

3. **UI Display**:
   - Location screen shows LocationActions as choice cards
   - NPC dialog shows NPCActions as conversation options

**Example - Scene 2 Activation**:
```
Player paid for room, Scene 2 spawned at Elena

SceneFacade.GetActionsForNPC("npc_elena_tavern_keeper"):
  1. Find Scene 2 (placementType: NPC, placementId: Elena)
  2. Get Situation "situation_pay_for_room" (state: Dormant)
  3. Transition state: Dormant → Active
  4. For ChoiceTemplate "choice_pay_room_cost":
       - Create NPCAction
       - Evaluate requirement: Coins >= 10
       - Player has 13 coins → requirement met
       - IsAvailable = true
  5. Return [NPCAction]

UI:
  Elena dialog shows:
    [1] Pay for the Room (10 Coins) ✅ Available
```

---

## 5. Challenge System Integration

### 5.1 Challenge Types and Resource Consumption

**Challenge Types**:
- **Social**: Costs Initiative (tactical), consumes Focus (strategic)
- **Physical**: Costs Exertion/Stamina (strategic), risks Health
- **Mental**: Costs Attention (tactical), consumes Focus (strategic)

**Tutorial Uses**:
- Scene 1, Choice 1: Social Challenge (costs 1 Focus)
- Scene 1, Choice 2: Physical Challenge (costs 1 Stamina)

### 5.2 ActionType.StartChallenge Pattern

**Current State** (to verify):
- ActionType enum likely has: Instant, Navigate, StartSocialChallenge?, StartPhysicalChallenge?
- OR: Single StartChallenge value with challengeType parameter?

**Required Implementation**:
- ChoiceTemplate needs `challengeType` property (Social/Physical/Mental)
- Executor detects ActionType.StartChallenge
- Executor calls ChallengeFacade.StartChallenge(challengeType, challengeDefinitionId)
- Challenge screen loads
- Challenge completion applies ChoiceReward

**Code Flow** (conceptual):
```
NPCActionExecutor.Execute(npcAction, player, gameWorld):
  choiceTemplate = npcAction.ChoiceTemplate

  // Apply costs
  player.Coins -= choiceTemplate.CostTemplate.Coins
  player.Focus -= choiceTemplate.CostTemplate.Focus
  player.Stamina -= choiceTemplate.CostTemplate.Stamina
  TimeService.AdvanceTime(choiceTemplate.CostTemplate.TimeSegments)

  // Execute action
  if (choiceTemplate.ActionType == StartChallenge):
    // Store ChoiceReward for challenge completion
    gameWorld.PendingChallengeReward = choiceTemplate.RewardTemplate

    // Start challenge
    ChallengeFacade.StartChallenge(
      challengeType: choiceTemplate.ChallengeType,
      challengeDefinitionId: choiceTemplate.ChallengeDefinitionId,
      contextNpcId: npcAction.NpcId
    )

    // Transition to challenge screen
    GameScreen.NavigateToChallenge(challengeType)

    return // Rewards applied AFTER challenge completes

  else if (choiceTemplate.ActionType == Instant):
    // Apply rewards immediately
    ApplyRewards(choiceTemplate.RewardTemplate, player, gameWorld)

    // Create provisional scenes
    CreateProvisionalScenes(choiceTemplate.RewardTemplate.ScenesToSpawn)
```

### 5.3 Challenge Completion Flow

**Trigger**: Player completes Social/Physical challenge (reaches goal)

**Process**:
1. **ChallengeFacade.CompleteChallenge()**:
   - Retrieve PendingChallengeReward from GameWorld
   - Apply rewards (coins, resources, bonds, etc.)
   - Create provisional scenes from ScenesToSpawn
   - Clear PendingChallengeReward
   - Transition back to Location/NPC screen

2. **UI Update**:
   - Resources update (coins, hunger, bond)
   - New scene notification appears
   - Choice card for new scene becomes available

**Example - Elena Challenge Completion**:
```
Player completes Social Challenge (Understanding reaches 15)

ChallengeFacade.CompleteChallenge():
  reward = gameWorld.PendingChallengeReward

  // Apply resource rewards
  player.Coins += reward.Coins (8 → 13)
  player.Health += reward.Health (6 → 6, already max)
  player.Hunger -= 10 (40 → 30)

  // Apply bond changes
  elena = gameWorld.NPCs["npc_elena_tavern_keeper"]
  elena.BondStrength += 5 (0 → 5)
  elena.RelationshipMilestone = "Friendly"

  // Create provisional scenes
  foreach (sceneSpawnReward in reward.ScenesToSpawn):
    SceneInstantiator.CreateProvisionalScene(
      templateId: "scene_secure_room",
      placementRelation: SameNPC,
      placementId: "npc_elena_tavern_keeper"
    )

  // Finalize (immediate spawn)
  SceneInstantiator.FinalizeScene(provisionalSceneId)

  // Transition
  GameScreen.NavigateToLocation(player.CurrentLocationId)
```

## 6. AutoAdvance Pattern Implementation

### 6.1 Purpose

Allow scenes to progress automatically without player input, used for:
- Narrative transitions (night passes)
- Forced progression (time advances)
- Resource restoration (sleep recovers all)

### 6.2 SceneTemplate.Archetype Values

**Current** (assumed):
- Linear: Player progresses through choices sequentially
- Branching: Player choices lead to different paths
- **NEW: AutoAdvance**: No choices, auto-executes rewards

**Required Changes**:
- Add `Archetype` enum value: AutoAdvance
- SceneTemplate validation: If archetype = AutoAdvance, SituationTemplates[0].ChoiceTemplates must be empty

### 6.4 SceneFacade AutoAdvance Detection

**Process**:
1. **SceneFacade.GetActionsAtLocation(locationId)**:
   - Query scenes at location
   - For each Scene:
     - Check SceneTemplate.Archetype
     - If Archetype == AutoAdvance:
       - Do NOT create actions
       - Instead, immediately execute AutoProgressRewards
       - Transition state: Dormant → Active → Completed
       - Return empty action list (scene handled)

2. **AutoProgressRewards Execution**:
   - Apply all rewards (coins, resources, time advancement)
   - Create provisional scenes
   - Finalize immediately (no commit needed)
   - Mark Situation as Completed
   - Mark Scene as Completed

3. **UI Behavior**:
   - Display narrative for 3-5 seconds (readable)
   - Show resource restoration animation
   - Auto-transition to next scene's location

**Code Flow** (conceptual):
```
SceneFacade.GetActionsAtLocation(locationId):
  scenes = gameWorld.Scenes.Where(s =>
    s.PlacementType == Location &&
    s.PlacementId == locationId &&
    s.State == Active)

  actions = new List<LocationAction>()

  foreach (scene in scenes):
    situation = scene.Situations.First(s => s.State == Dormant)
    sceneTemplate = GetTemplate(scene.TemplateId)

    if (sceneTemplate.Archetype == AutoAdvance):
      // Execute immediately, no actions created
      situationTemplate = GetTemplate(situation.TemplateId)

      ApplyRewards(situationTemplate.AutoProgressRewards, player, gameWorld)
      CreateProvisionalScenes(situationTemplate.AutoProgressRewards.ScenesToSpawn)
      FinalizeProvisionalScenes() // Immediate

      situation.State = Completed
      scene.State = Completed

      // Continue to next scene (will activate in subsequent query)
      continue

    else:
      // Normal flow: create actions
      situation.State = Active
      actions.AddRange(CreateActions(situation))

  return actions
```

## 7. Executor Integration

### 7.1 Current Executor Pattern

**Executors**:
- LocationActionExecutor
- NPCActionExecutor
- PathCardExecutor

**Current Flow** (assumed):
1. Player selects action (LocationAction/NPCAction)
2. Executor validates requirements
3. Executor applies costs
4. Executor executes action logic
5. Executor applies rewards (MISSING for ChoiceReward)

**Gap**: ChoiceReward not fully integrated

---

## 9. Resource Management

### 9.1 Resource Pools (6-Point Scale)

**Health, Focus, Stamina**:
- Scale: 0-6
- 0 = Depleted (critical state)
- 6 = Maximum (full capacity)
- Tutorial teaches: Preserving pools is strategic choice

**Tutorial Demonstration**:
- Elena path: Costs 1 Focus (6 → 5), preserves Stamina
- Thomas path: Costs 1 Stamina (6 → 5), preserves Focus
- Sleep rough: Costs 2 Health (6 → 4), preserves both
- Night rest: Restores all to 6 (full recovery)

### 9.2 Hunger System

**Scale**: 0-100
- 0 = Sated (full)
- 100 = Starving (critical)
- Increases per time block naturally
- Eating food reduces hunger

**Tutorial Demonstration**:
- Start: 40 hunger (moderate)
- Elena path: -10 hunger (meal included, 40 → 30)
- Thomas path: +5 hunger (physical work, 40 → 45)
- No explicit food purchase taught (Day 2 content)

### 9.3 Coins (Currency)

**Tutorial Demonstration**:
- Start: 8 coins (insufficient for room)
- Work: +5 coins (8 → 13, sufficient)
- Room: -10 coins (13 → 3, scarce again)
- Day 2 starts with 3 coins (pressure continues)

**Lesson**: Coins are constantly scarce, work is necessary

---

## 10. Testing Strategy

### 10.1 Unit Tests

**Parser Tests**:
- Test ChoiceCost mapping (Health, Hunger, Stamina, Focus)
- Test ChoiceReward mapping (same fields)
- Test AutoProgressRewards parsing

**PlacementFilter Tests**:
- Test NPC role matching
- Test bond strength filtering
- Test Location role matching

**SceneInstantiator Tests**:
- Test starter scene spawning
- Test provisional scene creation
- Test scene finalization
- Test placeholder replacement

### 10.2 Integration Tests

**Scene Spawning**:
- Load tutorial package
- Verify Scene 1 spawns at Elena
- Verify PlacementFilter evaluation works
- Verify Situation instantiation

**Executor Integration**:
- Execute Choice 1 (Elena)
- Verify costs applied (Focus, Time)
- Verify challenge starts
- Verify Scene 2 spawns after challenge

**AutoAdvance**:
- Execute Scene 3 (Night Rest)
- Verify auto-progression
- Verify time advancement
- Verify resource restoration

### 10.3 End-to-End Test (Critical Path)

**Test Scenario: Elena Path**
1. Start new game
2. Verify initial state:
   - Coins: 8
   - Health/Focus/Stamina: 6
   - Hunger: 40
   - Time: Day 1, Evening, Segment 1
3. Verify Scene 1 active at Elena
4. Select Choice 1 (Elena's Evening Service)
5. Verify costs applied:
   - Focus: 6 → 5
   - Time: Evening Segment 1 → Segment 2
6. Complete Social Challenge
7. Verify rewards applied:
   - Coins: 8 → 13
   - Health: 6 → 6 (already max)
   - Hunger: 40 → 30
   - Elena bond: 0 → 5 (Friendly)
8. Verify Scene 2 spawned at Elena
9. Select Choice (Pay for Room)
10. Verify costs applied:
    - Coins: 13 → 3
11. Verify Scene 3 spawned at Bedroom
12. Verify auto-progression:
    - Resources: Focus 5 → 6, others already 6
    - Time: Day 1, Evening → Day 2, Morning
13. Verify Scene 4 spawned at Common Room
14. Verify tutorial complete message
15. Verify Day 2 world state ready

**Test Scenario: Thomas Path**
(Same flow but Physical Challenge, different resource costs)

**Test Scenario: Sleep Rough Path**
1-3. Same as Elena
4. Select Choice 3 (Sleep Rough)
5. Verify costs applied:
   - Time: Evening Segment 1 → Night Block (6 segments)
6. Verify rewards applied:
   - Health: 6 → 4 (-2)
   - Elena bond: 0 → -3 (Disappointed)
7. Verify alternate Scene 2B spawned at Town Square
8. Verify Day 2 starts with damaged health

---

## 14. Post-Tutorial Next Steps

### Day 2 Content (Not Part of Tutorial)
- Delivery obligation system
- Job board with multiple contracts
- Investigation system (mill mystery)
- Equipment shop
- Route travel with challenges
- NPC conversations beyond tutorial

### Tutorial Metrics to Track
- % players completing tutorial
- Which path chosen (Elena/Thomas/Sleep Rough)
- Resource state at Day 2 start
- Time to complete tutorial
- Drop-off points (where players quit)

### Tutorial Iteration Based on Metrics
- If most players sleep rough → tutorial too punishing, adjust costs
- If tutorial takes > 15 minutes → too long, condense scenes
- If players confused about challenges → add more hints
- If players don't understand resource pools → add visual feedback
