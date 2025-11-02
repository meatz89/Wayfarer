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

---

## 2. Complete Content Specification

### 2.1 Tutorial NPCs JSON

**File**: `src/Content/Tutorial/02_tutorial_npcs.json`

```json
{
  "packageId": "tutorial_npcs",
  "npcs": [
    {
      "id": "npc_elena_tavern_keeper",
      "name": "Elena Crossroads",
      "role": "TAVERN_KEEPER",
      "gender": "Female",
      "age": "Middle-aged",
      "personalityTypes": ["MERCANTILE", "SYMPATHETIC"],
      "description": "Pragmatic tavern keeper who runs Crossroads Tavern with firm but fair rules. Notices when people genuinely try versus exploit.",
      "initialBondStrength": 0,
      "physicalDescription": "Sturdy woman with graying brown hair pulled back, weathered hands, practical clothing. Assessing eyes that miss nothing.",
      "baseLocation": "location_crossroads_tavern_common_room",
      "voicePattern": "Direct, economical speech. No-nonsense but not unkind.",
      "goals": ["Keep tavern profitable", "Help those who help themselves", "Maintain order"],
      "fears": ["Unpaid debts", "Troublemakers", "Empty rooms"]
    },
    {
      "id": "npc_thomas_warehouse_foreman",
      "name": "Thomas Redhand",
      "role": "WAREHOUSE_FOREMAN",
      "gender": "Male",
      "age": "Early 40s",
      "personalityTypes": ["PRAGMATIC", "DIRECT"],
      "description": "Warehouse foreman who values work ethic over words. Straightforward and respects those who can do physical labor.",
      "initialBondStrength": 0,
      "physicalDescription": "Broad-shouldered man with calloused hands, simple working clothes, slight limp from old injury. Face shows years of outdoor work.",
      "baseLocation": "location_riverside_warehouse",
      "voicePattern": "Few words, matter-of-fact. Says what he means.",
      "goals": ["Get work done on time", "Find reliable workers", "Keep warehouse running"],
      "fears": ["Lazy workers", "Missed deadlines", "Warehouse damage"]
    }
  ]
}
```

### 2.2 Tutorial Locations JSON

**File**: `src/Content/Tutorial/03_tutorial_locations.json`

```json
{
  "packageId": "tutorial_locations",
  "locations": [
    {
      "id": "location_crossroads_tavern",
      "name": "The Crossroads Tavern",
      "role": "TAVERN",
      "locationType": "BUILDING",
      "tags": ["hub", "safe", "social", "indoor"],
      "description": "Two-story timber building at town's edge where trade roads meet. Warm light spills from windows. Sign shows painted crossroads.",
      "atmosphereDescription": "Low conversation hum, fire crackle, food and ale smells. Travelers mix with locals. Elena moves efficiently between tables.",
      "hexCoordinates": { "q": 0, "r": 0 },
      "discoveredByDefault": true,
      "childLocationIds": [
        "location_crossroads_tavern_common_room",
        "location_crossroads_tavern_bedroom"
      ]
    },
    {
      "id": "location_crossroads_tavern_common_room",
      "name": "Common Room",
      "role": "TAVERN_COMMON_ROOM",
      "locationType": "ROOM",
      "tags": ["indoor", "safe", "social"],
      "description": "Main tavern floor. Long bar, scattered tables, fireplace. Job board near door. Stairs to upper rooms.",
      "atmosphereDescription": "Center of tavern life. Elena behind bar, travelers at tables, locals sharing news. Fire provides warmth.",
      "parentLocationId": "location_crossroads_tavern",
      "discoveredByDefault": true
    },
    {
      "id": "location_crossroads_tavern_bedroom",
      "name": "Upstairs Bedroom",
      "role": "TAVERN_BEDROOM",
      "locationType": "ROOM",
      "tags": ["indoor", "private", "safe", "rest"],
      "description": "Simple room. Clean bed, washbasin, small window. Functional, not luxurious.",
      "atmosphereDescription": "Quiet after common room noise. Bed looks inviting. Window shows darkening sky.",
      "parentLocationId": "location_crossroads_tavern",
      "discoveredByDefault": false
    },
    {
      "id": "location_town_square",
      "name": "Town Square",
      "role": "TOWN_SQUARE",
      "locationType": "AREA",
      "tags": ["outdoor", "public", "unsafe_night"],
      "description": "Open plaza at town center. Fountain, benches, market stalls (closed at night). Exposed to weather.",
      "atmosphereDescription": "Empty at evening. Wind picks up. No shelter from elements. Unsafe to sleep rough.",
      "hexCoordinates": { "q": 1, "r": 0 },
      "discoveredByDefault": true
    },
    {
      "id": "location_riverside_warehouse",
      "name": "Riverside Warehouse",
      "role": "WAREHOUSE",
      "locationType": "BUILDING",
      "tags": ["outdoor", "commercial", "physical_work"],
      "description": "Large storage building by river. Stacked crates, loading dock. Thomas oversees workers.",
      "atmosphereDescription": "Activity winding down for evening. Workers finishing last tasks. Heavy lifting, organization needed.",
      "hexCoordinates": { "q": -1, "r": 1 },
      "discoveredByDefault": true
    }
  ]
}
```

### 2.3 Tutorial Scenes JSON

**File**: `src/Content/Tutorial/01_tutorial_scenes.json`

```json
{
  "packageId": "tutorial_scenes",
  "sceneTemplates": [
    {
      "id": "scene_evening_arrival",
      "archetype": "Linear",
      "tier": 1,
      "isStarter": true,
      "displayNameTemplate": "Arrival at the Crossroads",
      "placementFilter": {
        "placementType": "NPC",
        "npcRole": "TAVERN_KEEPER",
        "minBondStrength": 0,
        "maxBondStrength": 0
      },
      "situationTemplates": [
        {
          "id": "situation_shelter_shortage",
          "narrativeTemplate": "{NPCName} wipes down tables while watching travelers settle in for the evening. She glances at you as you enter.\n\n'Need a room?' she asks. 'Ten coins. Standard rate.'\n\nYou have {CoinsCurrent} coins. Night is approaching—you can see the sky darkening through the windows. Other travelers are already claiming tables and rooms.",
          "choiceTemplates": [
            {
              "id": "choice_elena_evening_service",
              "actionTextTemplate": "Offer to Help Elena with Evening Service",
              "narrativeHints": ["Social challenge", "Earn coins through conversation and service"],
              "actionType": "StartChallenge",
              "challengeType": "Social",
              "requirementFormula": null,
              "costTemplate": {
                "coins": 0,
                "resolve": 0,
                "timeSegments": 1,
                "focus": 1,
                "stamina": 0,
                "health": 0,
                "hunger": 0
              },
              "rewardTemplate": {
                "coins": 5,
                "resolve": 0,
                "timeSegments": 0,
                "focus": 0,
                "stamina": 0,
                "health": 2,
                "hunger": -10,
                "bondChanges": [
                  {
                    "npcRole": "TAVERN_KEEPER",
                    "bondDelta": 5,
                    "relationshipMilestone": "Friendly"
                  }
                ],
                "scaleShifts": [],
                "stateApplications": [],
                "achievementIds": [],
                "itemIds": [],
                "scenesToSpawn": [
                  {
                    "sceneTemplateId": "scene_secure_room",
                    "placementRelation": "SameNPC",
                    "spawnTiming": "Immediate"
                  }
                ]
              }
            },
            {
              "id": "choice_thomas_warehouse_work",
              "actionTextTemplate": "Go to Thomas at the Warehouse for Work",
              "narrativeHints": ["Physical challenge", "Earn coins through manual labor"],
              "actionType": "StartChallenge",
              "challengeType": "Physical",
              "requirementFormula": null,
              "costTemplate": {
                "coins": 0,
                "resolve": 0,
                "timeSegments": 1,
                "focus": 0,
                "stamina": 1,
                "health": 0,
                "hunger": 0
              },
              "rewardTemplate": {
                "coins": 5,
                "resolve": 0,
                "timeSegments": 0,
                "focus": 0,
                "stamina": 0,
                "health": 0,
                "hunger": 5,
                "bondChanges": [
                  {
                    "npcRole": "WAREHOUSE_FOREMAN",
                    "bondDelta": 5,
                    "relationshipMilestone": "Friendly"
                  }
                ],
                "scaleShifts": [],
                "stateApplications": [],
                "achievementIds": [],
                "itemIds": [],
                "scenesToSpawn": [
                  {
                    "sceneTemplateId": "scene_secure_room",
                    "placementRelation": "SpecificNPC",
                    "specificPlacementId": "npc_elena_tavern_keeper",
                    "spawnTiming": "Immediate"
                  }
                ]
              }
            },
            {
              "id": "choice_sleep_rough",
              "actionTextTemplate": "Sleep Rough in Town Square",
              "narrativeHints": ["Save coins", "Risk health damage from exposure"],
              "actionType": "Instant",
              "requirementFormula": null,
              "costTemplate": {
                "coins": 0,
                "resolve": 0,
                "timeSegments": 6,
                "focus": 0,
                "stamina": 0,
                "health": 0,
                "hunger": 0
              },
              "rewardTemplate": {
                "coins": 0,
                "resolve": 0,
                "timeSegments": 0,
                "focus": 0,
                "stamina": 0,
                "health": -2,
                "hunger": 0,
                "bondChanges": [
                  {
                    "npcRole": "TAVERN_KEEPER",
                    "bondDelta": -3,
                    "relationshipMilestone": "Disappointed"
                  }
                ],
                "scaleShifts": [],
                "stateApplications": [],
                "achievementIds": [],
                "itemIds": [],
                "scenesToSpawn": [
                  {
                    "sceneTemplateId": "scene_rough_sleep_recovery",
                    "placementRelation": "SpecificLocation",
                    "specificPlacementId": "location_town_square",
                    "spawnTiming": "Immediate"
                  }
                ]
              }
            }
          ]
        }
      ]
    },
    {
      "id": "scene_secure_room",
      "archetype": "Linear",
      "tier": 1,
      "isStarter": false,
      "displayNameTemplate": "Secure Lodging",
      "placementFilter": {
        "placementType": "NPC",
        "npcRole": "TAVERN_KEEPER"
      },
      "situationTemplates": [
        {
          "id": "situation_pay_for_room",
          "narrativeTemplate": "{NPCName} counts your coins on the bar.\n\n'Ten coins,' she says. 'Upstairs, second door on the left. I'll have someone bring up water for washing.'",
          "choiceTemplates": [
            {
              "id": "choice_pay_room_cost",
              "actionTextTemplate": "Pay for the Room (10 Coins)",
              "actionType": "Instant",
              "requirementFormula": {
                "orPaths": [
                  {
                    "label": "Have enough coins",
                    "requirements": [
                      {
                        "requirementType": "Coins",
                        "thresholdValue": 10,
                        "comparisonOperator": "GreaterThanOrEqual"
                      }
                    ]
                  }
                ]
              },
              "costTemplate": {
                "coins": 10,
                "resolve": 0,
                "timeSegments": 0,
                "focus": 0,
                "stamina": 0,
                "health": 0,
                "hunger": 0
              },
              "rewardTemplate": {
                "coins": 0,
                "resolve": 0,
                "timeSegments": 0,
                "focus": 0,
                "stamina": 0,
                "health": 0,
                "hunger": 0,
                "bondChanges": [],
                "scaleShifts": [],
                "stateApplications": [],
                "achievementIds": [],
                "itemIds": [],
                "scenesToSpawn": [
                  {
                    "sceneTemplateId": "scene_night_rest",
                    "placementRelation": "SpecificLocation",
                    "specificPlacementId": "location_crossroads_tavern_bedroom",
                    "spawnTiming": "Immediate"
                  }
                ]
              }
            }
          ]
        }
      ]
    },
    {
      "id": "scene_night_rest",
      "archetype": "AutoAdvance",
      "tier": 1,
      "isStarter": false,
      "displayNameTemplate": "Night's Rest",
      "placementFilter": {
        "placementType": "Location",
        "locationRole": "TAVERN_BEDROOM"
      },
      "situationTemplates": [
        {
          "id": "situation_sleep_and_recover",
          "narrativeTemplate": "The bed is simple but clean. You wash off the road dust and collapse into the mattress.\n\nNight passes. You sleep deeply—the first real bed in days. No nightmares, no disturbances.\n\nMorning light filters through the window. You hear the town waking up below: cart wheels, vendor calls, the day beginning.\n\nYou feel restored. Ready.",
          "choiceTemplates": [],
          "autoProgressRewards": {
            "coins": 0,
            "resolve": 0,
            "timeSegments": 12,
            "focus": 6,
            "stamina": 6,
            "health": 6,
            "hunger": 0,
            "advanceToDay": "NextDay",
            "advanceToBlock": "Morning",
            "bondChanges": [],
            "scaleShifts": [],
            "stateApplications": [],
            "achievementIds": [],
            "itemIds": [],
            "scenesToSpawn": [
              {
                "sceneTemplateId": "scene_day2_morning",
                "placementRelation": "SpecificLocation",
                "specificPlacementId": "location_crossroads_tavern_common_room",
                "spawnTiming": "Immediate"
              }
            ]
          }
        }
      ]
    },
    {
      "id": "scene_day2_morning",
      "archetype": "WorldStateReveal",
      "tier": 1,
      "isStarter": false,
      "displayNameTemplate": "A New Day",
      "placementFilter": {
        "placementType": "Location",
        "locationRole": "TAVERN_COMMON_ROOM"
      },
      "situationTemplates": [
        {
          "id": "situation_world_opens",
          "narrativeTemplate": "You descend to the common room. {NPCName} is already working, preparing breakfast for the morning crowd.\n\n'Sleep well?' she asks without looking up. 'Good. If you're looking for work, Thomas mentioned he might have steady jobs at the warehouse—if you proved reliable yesterday.'\n\nShe nods toward the job board near the door. 'There's also talk of trouble at the old mill. Something about the waterwheel. Could be worth investigating if you're the curious type.'\n\nThe day stretches ahead. The town is open. Choices to make.",
          "choiceTemplates": []
        }
      ]
    },
    {
      "id": "scene_rough_sleep_recovery",
      "archetype": "Linear",
      "tier": 1,
      "isStarter": false,
      "displayNameTemplate": "Cold Night",
      "placementFilter": {
        "placementType": "Location",
        "locationRole": "TOWN_SQUARE"
      },
      "situationTemplates": [
        {
          "id": "situation_morning_after_rough_sleep",
          "narrativeTemplate": "Morning comes slowly. You're cold, stiff, and your body aches from sleeping on stone.\n\nThe fountain provided water, but no shelter from the wind. You feel the damage—not crippling, but real.\n\nThe tavern is across the square. Elena is visible through the windows, serving breakfast. She doesn't look your way.\n\nYou saved your coins. But at what cost?",
          "choiceTemplates": [
            {
              "id": "choice_continue_after_rough_sleep",
              "actionTextTemplate": "Face the Day",
              "actionType": "Instant",
              "requirementFormula": null,
              "costTemplate": {
                "coins": 0,
                "resolve": 0,
                "timeSegments": 0,
                "focus": 0,
                "stamina": 0,
                "health": 0,
                "hunger": 0
              },
              "rewardTemplate": {
                "coins": 0,
                "resolve": 0,
                "timeSegments": 12,
                "focus": 4,
                "stamina": 4,
                "health": 0,
                "hunger": 0,
                "advanceToDay": "NextDay",
                "advanceToBlock": "Morning",
                "bondChanges": [],
                "scaleShifts": [],
                "stateApplications": [],
                "achievementIds": [],
                "itemIds": [],
                "scenesToSpawn": []
              }
            }
          ]
        }
      ]
    }
  ]
}
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

### 5.4 Challenge Definition Requirements

**New Entities Needed**:

**ChallengeDefinition**:
```csharp
public class ChallengeDefinition
{
    public string Id { get; set; }
    public ChallengeType Type { get; set; } // Social/Physical/Mental
    public string NarrativeIntro { get; set; }
    public string NarrativeSuccess { get; set; }
    public string NarrativeFailure { get; set; }

    // Goal thresholds
    public int GoalThreshold { get; set; } // Understanding/Breakthrough target
    public int DangerThreshold { get; set; } // Doubt/Danger limit

    // Starting deck
    public List<string> StartingCardIds { get; set; }
    public int InitialHandSize { get; set; }
    public int InitialInitiative { get; set; } // Social/Mental
    public int InitialExertion { get; set; } // Physical
}
```

**JSON Example**:
```json
{
  "id": "challenge_elena_evening_service",
  "type": "Social",
  "narrativeIntro": "Elena hands you a tray. 'Start with table three. Listen more than you talk. Learn what people need.'",
  "narrativeSuccess": "Elena nods as you finish. 'You listen. That's rare. Here—' She counts five coins onto the bar. 'And eat something before you go upstairs. You've earned it.'",
  "narrativeFailure": "Elena frowns. 'That's enough. You're more trouble than help tonight.'",
  "goalThreshold": 15,
  "dangerThreshold": 10,
  "startingCardIds": [
    "social_card_simple_question",
    "social_card_friendly_remark",
    "social_card_observation"
  ],
  "initialHandSize": 3,
  "initialInitiative": 3
}
```

**ChoiceTemplate Extension**:
```csharp
public class ChoiceTemplate
{
    // ... existing properties
    public string? ChallengeDefinitionId { get; set; } // NEW
}
```

---

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

### 6.3 SituationTemplate.AutoProgressRewards

**New Property**:
```csharp
public class SituationTemplate
{
    // ... existing properties

    /// <summary>
    /// Rewards applied automatically when Situation activates (no player input)
    /// Only used when SceneTemplate.Archetype = AutoAdvance
    /// </summary>
    public ChoiceReward? AutoProgressRewards { get; set; }
}
```

**JSON Example** (Scene 3: Night Rest):
```json
{
  "id": "situation_sleep_and_recover",
  "narrativeTemplate": "Night passes. You sleep deeply...",
  "choiceTemplates": [],
  "autoProgressRewards": {
    "coins": 0,
    "resolve": 0,
    "timeSegments": 12,
    "focus": 6,
    "stamina": 6,
    "health": 6,
    "hunger": 0,
    "advanceToDay": "NextDay",
    "advanceToBlock": "Morning",
    "scenesToSpawn": [...]
  }
}
```

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

### 6.5 Time Advancement in AutoProgressRewards

**New ChoiceReward Properties**:
```csharp
public class ChoiceReward
{
    // ... existing properties

    /// <summary>
    /// Advance to specific time block (overrides TimeSegments)
    /// </summary>
    public TimeBlock? AdvanceToBlock { get; set; }

    /// <summary>
    /// Advance to next day (or stay on current day)
    /// </summary>
    public DayAdvancement? AdvanceToDay { get; set; }
}

public enum DayAdvancement
{
    CurrentDay,
    NextDay
}
```

**TimeService Integration**:
```csharp
TimeService.ApplyRewardTimeAdvancement(reward, gameWorld):
  if (reward.AdvanceToDay == NextDay):
    gameWorld.CurrentDay++

  if (reward.AdvanceToBlock.HasValue):
    gameWorld.CurrentTimeBlock = reward.AdvanceToBlock.Value
    gameWorld.CurrentTimeSegment = 1 // Reset to segment 1 of new block

  else if (reward.TimeSegments > 0):
    // Normal segment advancement
    AdvanceBySegments(reward.TimeSegments)
```

---

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

### 7.2 Required Executor Changes

**NPCActionExecutor.Execute()**:
```csharp
public async Task<ExecutionResult> Execute(NPCAction action, GameWorld gameWorld)
{
    ChoiceTemplate choiceTemplate = action.ChoiceTemplate;
    Player player = gameWorld.Player;

    // 1. Validate requirements (ALREADY EXISTS)
    if (!RequirementChecker.CheckRequirements(choiceTemplate.RequirementFormula, player, gameWorld))
    {
        return ExecutionResult.Failure("Requirements not met");
    }

    // 2. Apply costs (EXTEND)
    ApplyCosts(choiceTemplate.CostTemplate, player, gameWorld);

    // 3. Execute action based on type (EXTEND)
    if (choiceTemplate.ActionType == ActionType.StartChallenge)
    {
        // Store reward for challenge completion
        gameWorld.PendingChallengeReward = choiceTemplate.RewardTemplate;

        // Start challenge
        await ChallengeFacade.StartChallenge(
            choiceTemplate.ChallengeType,
            choiceTemplate.ChallengeDefinitionId,
            action.NpcId
        );

        // Don't apply rewards yet (applied after challenge)
        return ExecutionResult.Success();
    }
    else if (choiceTemplate.ActionType == ActionType.Instant)
    {
        // Apply rewards immediately (NEW)
        ApplyRewards(choiceTemplate.RewardTemplate, player, gameWorld);

        // Create provisional scenes (NEW)
        CreateProvisionalScenes(choiceTemplate.RewardTemplate, gameWorld);

        // Mark situation completed
        MarkSituationCompleted(action.SituationId, gameWorld);

        return ExecutionResult.Success();
    }

    return ExecutionResult.Failure("Unknown action type");
}

private void ApplyCosts(ChoiceCost cost, Player player, GameWorld gameWorld)
{
    player.Coins -= cost.Coins;
    player.Health -= cost.Health;
    player.Focus -= cost.Focus;
    player.Stamina -= cost.Stamina;
    player.Hunger += cost.Hunger; // Positive increases hunger

    if (cost.TimeSegments > 0)
    {
        TimeService.AdvanceBySegments(cost.TimeSegments, gameWorld);
    }
}

private void ApplyRewards(ChoiceReward reward, Player player, GameWorld gameWorld)
{
    // Resources
    player.Coins += reward.Coins;
    player.Health = Math.Clamp(player.Health + reward.Health, 0, player.MaxHealth);
    player.Focus = Math.Clamp(player.Focus + reward.Focus, 0, player.MaxFocus);
    player.Stamina = Math.Clamp(player.Stamina + reward.Stamina, 0, player.MaxStamina);
    player.Hunger = Math.Clamp(player.Hunger + reward.Hunger, 0, player.MaxHunger);

    // Bond changes
    foreach (var bondChange in reward.BondChanges)
    {
        NPC npc = FindNPCByRole(bondChange.NpcRole, gameWorld);
        npc.BondStrength += bondChange.BondDelta;
        npc.RelationshipMilestone = bondChange.RelationshipMilestone;
    }

    // Scale shifts
    foreach (var scaleShift in reward.ScaleShifts)
    {
        player.Scales.ApplyShift(scaleShift);
    }

    // States
    foreach (var stateApp in reward.StateApplications)
    {
        player.ApplyState(stateApp);
    }

    // Achievements
    foreach (var achievementId in reward.AchievementIds)
    {
        player.GrantAchievement(achievementId);
    }

    // Items
    foreach (var itemId in reward.ItemIds)
    {
        player.Inventory.AddItem(itemId);
    }

    // Time advancement
    TimeService.ApplyRewardTimeAdvancement(reward, gameWorld);
}

private void CreateProvisionalScenes(ChoiceReward reward, GameWorld gameWorld)
{
    foreach (var sceneSpawnReward in reward.ScenesToSpawn)
    {
        SceneTemplate template = GetSceneTemplate(sceneSpawnReward.SceneTemplateId);
        string placementId = ResolvePlacementId(sceneSpawnReward, currentContext);

        Scene provisionalScene = SceneInstantiator.CreateProvisionalScene(
            template,
            placementId
        );

        gameWorld.ProvisionalScenes.Add(provisionalScene);

        // If spawnTiming = Immediate, finalize now
        if (sceneSpawnReward.SpawnTiming == SpawnTiming.Immediate)
        {
            SceneInstantiator.FinalizeScene(provisionalScene.Id, gameWorld);
        }
    }
}
```

### 7.3 ChallengeFacade Reward Application

**ChallengeFacade.CompleteChallenge()**:
```csharp
public void CompleteChallenge(ChallengeType challengeType, GameWorld gameWorld)
{
    // Retrieve pending reward
    ChoiceReward reward = gameWorld.PendingChallengeReward;
    if (reward == null) return;

    // Apply rewards (same logic as NPCActionExecutor)
    ApplyRewards(reward, gameWorld.Player, gameWorld);

    // Create provisional scenes
    CreateProvisionalScenes(reward, gameWorld);

    // Clear pending reward
    gameWorld.PendingChallengeReward = null;

    // Mark situation completed
    MarkSituationCompleted(gameWorld.CurrentSituationId, gameWorld);
}
```

---

## 8. Time System Integration

### 8.1 Time Structure

**Units**:
- **Day**: 24 segments total
- **Block**: 4 segments per block, 6 blocks per day
- **Segment**: Smallest unit (1 segment = ~1 hour of in-game time)

**Blocks**:
1. Morning (segments 1-4)
2. Midday (segments 5-8)
3. Afternoon (segments 9-12)
4. Evening (segments 13-16)
5. Night (segments 17-20)
6. LateNight (segments 21-24)

**Tutorial Timeline**:
- Start: Day 1, Evening, Segment 1 (13th of 24)
- Scene 1 Choice Costs: 1 segment → Evening Segment 2
- Scene 2 Instant: 0 segments → stays Evening Segment 2
- Scene 3 AutoAdvance: 12 segments → Day 2, Morning, Segment 1

### 8.2 TimeService Methods

**TimeService.AdvanceBySegments(int segments, GameWorld gameWorld)**:
```csharp
public void AdvanceBySegments(int segments, GameWorld gameWorld)
{
    gameWorld.CurrentTimeSegment += segments;

    while (gameWorld.CurrentTimeSegment > 4)
    {
        gameWorld.CurrentTimeSegment -= 4;
        AdvanceBlock(gameWorld);
    }
}

private void AdvanceBlock(GameWorld gameWorld)
{
    gameWorld.CurrentTimeBlock = GetNextBlock(gameWorld.CurrentTimeBlock);

    if (gameWorld.CurrentTimeBlock == TimeBlock.Morning)
    {
        gameWorld.CurrentDay++;
    }
}
```

**TimeService.ApplyRewardTimeAdvancement(ChoiceReward reward, GameWorld gameWorld)**:
```csharp
public void ApplyRewardTimeAdvancement(ChoiceReward reward, GameWorld gameWorld)
{
    // Priority: AdvanceToBlock overrides TimeSegments
    if (reward.AdvanceToBlock.HasValue)
    {
        gameWorld.CurrentTimeBlock = reward.AdvanceToBlock.Value;
        gameWorld.CurrentTimeSegment = 1;

        // Check if advancing to next day
        if (reward.AdvanceToDay == DayAdvancement.NextDay)
        {
            gameWorld.CurrentDay++;
        }
    }
    else if (reward.TimeSegments > 0)
    {
        AdvanceBySegments(reward.TimeSegments, gameWorld);
    }
}
```

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

## 11. Implementation Sequence

### Phase 1: Parser and Data Extensions (30 minutes)

**Tasks**:
1. ✅ Extend ChoiceCost entity (Health, Hunger, Stamina, Focus)
2. ✅ Extend ChoiceCostDTO (same fields)
3. ✅ Extend ChoiceReward entity (same fields)
4. ✅ Extend ChoiceRewardDTO (same fields)
5. ✅ Update SceneTemplateParser mappings
6. Add AdvanceToBlock, AdvanceToDay properties to ChoiceReward
7. Add AutoProgressRewards property to SituationTemplate
8. Build and verify compilation

### Phase 2: Content Authoring (2 hours)

**Tasks**:
1. Create 02_tutorial_npcs.json (Elena, Thomas)
2. Create 03_tutorial_locations.json (Tavern, Bedroom, Town Square, Warehouse)
3. Create 01_tutorial_scenes.json (6 scene templates)
4. Create 04_tutorial_player_config.json (initial state)
5. Create package_manifest.json
6. Verify JSON syntax with linter

### Phase 3: Challenge System Integration (2-3 hours)

**Tasks**:
1. Analyze existing ChallengeFacade code
2. Analyze existing SocialChallenge/PhysicalChallenge code
3. Determine if ActionType.StartChallenge exists (or create)
4. Add ChallengeType property to ChoiceTemplate
5. Add ChallengeDefinitionId property to ChoiceTemplate
6. Create ChallengeDefinition entity (if needed)
7. Create challenge_elena_evening_service.json (challenge definition)
8. Create challenge_thomas_warehouse_work.json (challenge definition)
9. Test challenge start from Choice
10. Test challenge completion reward application

### Phase 4: AutoAdvance Pattern (1-2 hours)

**Tasks**:
1. Add Archetype enum value: AutoAdvance
2. Add AutoProgressRewards property to SituationTemplate
3. Update SceneTemplateParser to parse AutoProgressRewards
4. Update SceneFacade.GetActionsAtLocation to detect AutoAdvance
5. Implement immediate reward application in SceneFacade
6. Test Scene 3 (Night Rest) auto-progression

### Phase 5: Executor Integration (2 hours)

**Tasks**:
1. Update NPCActionExecutor.Execute():
   - Add ApplyCosts method (Health, Hunger, Stamina, Focus)
   - Add ApplyRewards method (all reward types)
   - Add CreateProvisionalScenes method
   - Add StartChallenge detection
2. Update LocationActionExecutor.Execute() (same changes)
3. Update ChallengeFacade.CompleteChallenge():
   - Add reward application
   - Add provisional scene creation
4. Test cost application
5. Test reward application
6. Test scene spawning

### Phase 6: Time System Integration (1 hour)

**Tasks**:
1. Update TimeService.ApplyRewardTimeAdvancement():
   - Add AdvanceToBlock handling
   - Add AdvanceToDay handling
2. Test time advancement in Scene 3
3. Test Day 1 → Day 2 transition

### Phase 7: Testing and Validation (2 hours)

**Tasks**:
1. Run unit tests (parsers, placement filter, instantiator)
2. Run integration tests (scene spawning, executors)
3. Run end-to-end test (Elena path)
4. Run end-to-end test (Thomas path)
5. Run end-to-end test (Sleep Rough path)
6. Fix bugs discovered
7. Verify all tutorial learning objectives met

---

## 12. Success Criteria (Final Checklist)

### Content Loaded
- [ ] Tutorial package loads without errors
- [ ] Elena NPC exists with TAVERN_KEEPER role
- [ ] Thomas NPC exists with WAREHOUSE_FOREMAN role
- [ ] Tavern locations exist (Common Room, Bedroom)
- [ ] All 6 scene templates parse correctly

### Scene Spawning
- [ ] Scene 1 spawns at Elena on game start
- [ ] PlacementFilter correctly finds Elena by role
- [ ] Placeholder replacement works ({NPCName}, {CoinsCurrent})
- [ ] Situation transitions Dormant → Active when player enters

### Elena Path (Social Challenge)
- [ ] Choice 1 costs 1 Focus and 1 Time Segment
- [ ] Social Challenge starts correctly
- [ ] Challenge completion grants rewards (5 coins, +2 health, -10 hunger, +5 bond)
- [ ] Scene 2 spawns at Elena
- [ ] Paying for room costs 10 coins
- [ ] Scene 3 spawns at Bedroom

### Thomas Path (Physical Challenge)
- [ ] Choice 2 costs 1 Stamina and 1 Time Segment
- [ ] Physical Challenge starts correctly
- [ ] Challenge completion grants rewards (5 coins, +5 hunger, +5 bond with Thomas)
- [ ] Scene 2 spawns at Elena (SpecificNPC)
- [ ] Rest of flow same as Elena path

### Sleep Rough Path
- [ ] Choice 3 costs 6 Time Segments
- [ ] Instant action applies rewards (-2 health, -3 bond with Elena)
- [ ] Alternate Scene 2B spawns at Town Square
- [ ] Day 2 starts with damaged health (4/6)

### AutoAdvance (Night Rest)
- [ ] Scene 3 has no player choices
- [ ] AutoProgressRewards apply immediately
- [ ] Resources restore to 6 (Health, Focus, Stamina)
- [ ] Time advances to Day 2, Morning Block
- [ ] Scene 4 spawns at Common Room

### Day 2 Morning
- [ ] Scene 4 displays correctly
- [ ] WorldStateReveal shows tutorial complete message
- [ ] No choices presented (narrative only)
- [ ] Player can transition to full game

### Resource Management
- [ ] All resource costs apply correctly
- [ ] All resource rewards apply correctly
- [ ] 6-point pools clamped to 0-6 range
- [ ] Hunger clamped to 0-100 range
- [ ] Coins can go negative (debt possible)

### UI Display
- [ ] Resource bar always visible at top
- [ ] Resource changes animate
- [ ] Choice cards show costs and rewards
- [ ] Challenge type icons display (💭 Social, 💪 Physical)
- [ ] NPC relationship status displays ([Friendly])
- [ ] New scene notifications appear

### Learning Objectives
- [ ] Player understands resource scarcity (8 < 10 coins)
- [ ] Player understands work safety net (challenges earn 5 coins)
- [ ] Player understands resource trade-offs (Focus vs Stamina)
- [ ] Player understands rest mechanics (lodging restores all)
- [ ] Player understands consequences (Elena relationship changes)

---

## 13. Known Risks and Mitigation

### Risk: Challenge System Incompatibility
**Description**: Existing challenge system may not support starting from Choice
**Mitigation**: Phase 3 analysis will determine if refactoring needed
**Fallback**: Create simple tutorial-specific challenge flow if full integration too complex

### Risk: AutoAdvance Pattern Breaks Existing Scenes
**Description**: Adding AutoAdvance may affect existing scene archetypes
**Mitigation**: Only apply AutoAdvance to scenes explicitly marked as such
**Testing**: Verify existing Linear/Branching scenes unaffected

### Risk: Time Advancement Edge Cases
**Description**: Advancing to next day may have unexpected side effects
**Mitigation**: TimeService already handles day advancement, tutorial just uses it
**Testing**: Verify Day 1 → Day 2 transition doesn't break persistent state

### Risk: PlacementFilter Not Finding NPCs
**Description**: NPC role matching may fail if roles not unique
**Mitigation**: Tutorial NPCs have unique roles (TAVERN_KEEPER, WAREHOUSE_FOREMAN)
**Testing**: Unit test PlacementFilter evaluation with tutorial data

### Risk: Content Package Load Order
**Description**: Tutorial content may load after Core content, causing reference errors
**Mitigation**: Package manifest declares load order dependency
**Testing**: Verify tutorial package loads successfully on clean game start

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

---

## Appendix A: File Checklist

### Files to Create
- [ ] `src/Content/Tutorial/01_tutorial_scenes.json`
- [ ] `src/Content/Tutorial/02_tutorial_npcs.json`
- [ ] `src/Content/Tutorial/03_tutorial_locations.json`
- [ ] `src/Content/Tutorial/04_tutorial_player_config.json`
- [ ] `src/Content/Tutorial/package_manifest.json`
- [ ] `src/Content/Tutorial/Challenges/challenge_elena_evening_service.json`
- [ ] `src/Content/Tutorial/Challenges/challenge_thomas_warehouse_work.json`

### Files to Modify
- [ ] `src/GameState/ChoiceCost.cs` ✅
- [ ] `src/Content/DTOs/ChoiceCostDTO.cs` ✅
- [ ] `src/GameState/ChoiceReward.cs` ✅
- [ ] `src/Content/DTOs/ChoiceRewardDTO.cs` ✅
- [ ] `src/Content/Parsers/SceneTemplateParser.cs` ✅
- [ ] `src/GameState/ChoiceTemplate.cs` (add ChallengeType, ChallengeDefinitionId)
- [ ] `src/GameState/SituationTemplate.cs` (add AutoProgressRewards)
- [ ] `src/GameState/SceneTemplate.cs` (add AutoAdvance archetype)
- [ ] `src/Services/Executors/NPCActionExecutor.cs`
- [ ] `src/Services/Executors/LocationActionExecutor.cs`
- [ ] `src/Facades/ChallengeFacade.cs`
- [ ] `src/Facades/SceneFacade.cs`
- [ ] `src/Services/TimeService.cs`

### Files to Analyze (Read-Only)
- [ ] `src/GameState/Player.cs`
- [ ] `src/GameState/Scene.cs`
- [ ] `src/GameState/Situation.cs`
- [ ] `src/Services/SceneInstantiator.cs`
- [ ] `src/Facades/GameFacade.cs`

---

## Appendix B: Glossary

**Archetype**: SceneTemplate pattern (Linear, Branching, AutoAdvance)
**AutoAdvance**: Scene type that progresses without player input
**Bond**: NPC relationship strength (0-100)
**Challenge**: Tactical gameplay (Social/Physical/Mental)
**ChoiceTemplate**: Immutable action definition in JSON
**Focus**: 6-point mental resource pool
**Placement**: Where a Scene spawns (NPC/Location/Route)
**PlacementFilter**: Categorical criteria for entity matching
**Provisional Scene**: Uncommitted scene created before finalization
**Scene**: Narrative container with Situations
**Situation**: Narrative moment with Choices
**Stamina**: 6-point physical resource pool
**StartChallenge**: ActionType that initiates tactical challenge
**WorldStateReveal**: Scene type showing game systems (no choices)
