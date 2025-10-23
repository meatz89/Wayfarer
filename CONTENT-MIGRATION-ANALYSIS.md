# Wayfarer Content Migration Analysis
## From Goal/Obstacle/Obligation → To Scene/Card/Action Architecture

**Analysis Date:** 2025-10-23
**Status:** Tutorial-only content (minimal complexity, ideal for migration testing)

---

## 1. CONTENT FILE INVENTORY

### Current Organization (14 JSON files in `src/Content/Core/`)

| File | Contains | Count | Purpose |
|------|----------|-------|---------|
| `01_foundation.json` | Venues, Locations, LocationActions, PlayerActions, Obstacles | 2 venues, 2 locations, 3 location actions, 3 player actions, 0 obstacles | World structure |
| `03_npcs.json` | NPCs | 3 NPCs | Character data |
| `04_connections.json` | Routes, PathCards | 1 route | Travel system |
| `05_goals.json` | Goals | 2 goals | **PRIMARY MIGRATION TARGET** |
| `06_gameplay.json` | Stats, Progression, DialogueTemplates | 5 stats | Core systems config |
| `07_equipment.json` | Equipment/Items | ~20 items | Item catalog |
| `08_social_cards.json` | SocialCards | 60 cards | Social challenge cards |
| `09_mental_cards.json` | MentalCards | 5 cards | Mental challenge cards |
| `10_physical_cards.json` | PhysicalCards | 5 cards | Physical challenge cards |
| `11_exchange_cards.json` | ExchangeCards | ? | Trade/economy cards |
| `12_challenge_decks.json` | Deck compositions | 3 decks | Challenge system setup |
| `13_obligations.json` | Obligations | 1 obligation | **SECONDARY MIGRATION TARGET** |
| `14_npc_decks.json` | NPC-specific decks | ? | NPC challenge setups |
| `Narratives/conversation_narratives.json` | Conversation text | ? | Narrative content |

**TOTAL ENTITIES TO MIGRATE:**
- **2 Goals** (both tutorial work goals)
- **1 Obligation** (tutorial investigation)
- **0 Obstacles** (none in current tutorial)
- **3 LocationActions** (Rest, SecureRoom, Work)
- **3 PlayerActions** (CheckBelongings, Wait, SleepOutside)

**Migration Complexity Assessment:** **VERY LOW** - This is tutorial-only content with minimal complexity, perfect for testing the new architecture.

---

## 2. GOAL JSON STRUCTURE ANALYSIS

### Example 1: "Help with Evening Service" (Social Work Goal)

```json
{
  "id": "elena_evening_service",
  "name": "Help with Evening Service",
  "description": "Assist Elena managing the evening crowd. One hour of work serving drinks and managing tables. Social engagement, uses Focus.",
  "systemType": "Social",
  "placementNpcId": "elena",
  "deckId": "friendly_chat",
  "costs": {
    "time": 1,
    "focus": 1,
    "stamina": 0,
    "coins": 0
  },
  "difficultyModifiers": [],
  "consequenceType": "Resolution",
  "setsResolutionMethod": "Cooperation",
  "setsRelationshipOutcome": "Positive",
  "isAvailable": true,
  "deleteOnSuccess": false,
  "propertyRequirements": {},
  "goalCards": [
    {
      "id": "evening_service_complete",
      "name": "Competent Service",
      "description": "Move through the common room with practiced ease. Drinks delivered, conversations managed, chaos ordered. Elena notices your skill.",
      "threshold": 6,
      "rewards": {
        "coins": 5,
        "storyCubes": 1,
        "equipmentId": "rations"
      }
    }
  ]
}
```

**Field Analysis:**

| Field | Current Purpose | Migration Strategy |
|-------|----------------|-------------------|
| `id` | Unique identifier | **Becomes SceneTemplate.Id** |
| `name` | Player-facing title | **Becomes SceneTemplate.Name** |
| `description` | What player sees before engaging | **Becomes SceneTemplate.Description** |
| `systemType` | Which challenge system (Social/Mental/Physical) | **Becomes SceneTemplate.ChallengeType** |
| `placementNpcId` | Where goal appears (NPC context) | **REMOVED** - Placement determined by ActionDefinition spawn location |
| `deckId` | Which challenge deck to use | **Becomes SceneTemplate.DeckId** |
| `costs` | Entry requirements (time/focus/stamina/coins) | **Becomes SceneTemplate.EntryCosts** |
| `difficultyModifiers` | Adjustments to challenge difficulty | **Becomes SceneTemplate.DifficultyModifiers** |
| `consequenceType` | What happens on failure? | **Becomes SceneTemplate.ConsequenceType** |
| `setsResolutionMethod` | Narrative framing | **Becomes SceneTemplate.ResolutionMethod** |
| `setsRelationshipOutcome` | Relationship impact | **Becomes SceneTemplate.RelationshipOutcome** |
| `isAvailable` | Static availability flag | **DELETED** - Availability controlled by ActionDefinition spawn conditions |
| `deleteOnSuccess` | Should goal disappear after completion? | **DELETED** - Lifecycle controlled by SceneOutcome.DespawnActions |
| `propertyRequirements` | Location properties required | **DELETED** - Not relevant (goals don't check location properties) |
| `goalCards` | Victory conditions within challenge | **Becomes SceneTemplate.VictoryConditions (List<CardTemplate>)** |

**Key Insight:** Goals are VERY CLOSE to Scenes already! Main changes:
1. Remove placement/availability logic (handled by Actions)
2. Rename `goalCards` → `victoryConditions`
3. Move entry costs to top-level `entryCosts`

### Example 2: "Warehouse Loading" (Physical Work Goal)

```json
{
  "id": "thomas_warehouse_loading",
  "name": "Warehouse Loading",
  "description": "Heavy crates need moving before the morning shipment. Physical labor, but pays well. Uses Stamina.",
  "systemType": "Physical",
  "placementNpcId": "thomas",
  "deckId": "physical_challenge",
  "costs": {
    "time": 1,
    "focus": 0,
    "stamina": 1,
    "coins": 0
  },
  "difficultyModifiers": [],
  "consequenceType": "Resolution",
  "setsResolutionMethod": "Strength",
  "setsRelationshipOutcome": "Neutral",
  "isAvailable": true,
  "deleteOnSuccess": false,
  "propertyRequirements": {},
  "goalCards": [
    {
      "id": "warehouse_loading_complete",
      "name": "Loading Complete",
      "description": "Crates moved, shipment ready. Thomas nods approval and hands over payment. Hard work, fair pay.",
      "threshold": 6,
      "rewards": {
        "coins": 5,
        "storyCubes": 1
      }
    }
  ]
}
```

**Same pattern as Example 1** - Physical goal with identical structure to Social goal. Migration strategy is identical.

---

## 3. OBSTACLE JSON STRUCTURE ANALYSIS

**CRITICAL FINDING:** Current tutorial content has **ZERO obstacles** defined!

The `01_foundation.json` contains `"obstacles": []` (empty array).

**What This Means:**
- Current tutorial doesn't use the Obligation → Obstacle → Goal hierarchy
- Goals exist independently as work opportunities
- No complex availability conditions
- No progressive unlocking

**Migration Impact:** **SIMPLIFIES MIGRATION** - We can focus on Scene/Action patterns without needing to handle Obstacle complexity.

---

## 4. OBLIGATION JSON STRUCTURE ANALYSIS

### Example: "Survey the Common Room" (Tutorial Investigation)

```json
{
  "id": "investigate_inn_opportunities",
  "name": "Survey the Common Room",
  "description": "Look around the inn's common room to understand who might offer work or information",
  "obligationType": "SelfDiscovered",
  "deadlineSegment": 0,
  "colorCode": "#7a9eb8",
  "intro": {
    "triggerType": "Manual",
    "actionText": "Survey the common room",
    "locationId": "common_room",
    "introNarrative": "The common room hums with evening activity. A few patrons sit at tables, some nursing drinks, others in quiet conversation. Elena moves efficiently between tables. A warehouse foreman sits in the corner, looking like he needs capable hands.",
    "completionReward": {
      "obstaclesSpawned": []
    }
  },
  "personality": "SequentialSite",
  "exposureThreshold": 0,
  "timeLimit": 0,
  "phases": [
    {
      "id": "observe_common_room",
      "name": "Observe the common room",
      "description": "Take in the scene and identify potential opportunities",
      "outcomeNarrative": "You observe the room carefully. Elena seems approachable - she manages the evening crowd with practiced ease. The warehouse foreman looks like he values hard work over conversation. Both might have work to offer.",
      "completionReward": {}
    }
  ],
  "completionNarrative": "You've surveyed the room and identified two potential work opportunities: Elena's evening service (social work) and Thomas's warehouse loading (physical labor).",
  "completionRewards": {
    "coins": 0,
    "experience": 5
  },
  "observationCardRewards": [],
  "requiredGoals": []
}
```

**Field Analysis:**

| Field | Current Purpose | Migration Strategy |
|-------|----------------|-------------------|
| `intro.triggerType` | How obligation starts | **Becomes ActionDefinition.TriggerType** |
| `intro.actionText` | What action player takes | **Becomes ActionDefinition.Name** |
| `intro.locationId` | Where action appears | **Becomes ActionDefinition.SpawnLocation** |
| `intro.introNarrative` | Story shown when starting | **Becomes SceneTemplate.IntroNarrative** |
| `intro.completionReward.obstaclesSpawned` | What spawns on completion | **Becomes SceneOutcome.SpawnActions** |
| `phases` | Multi-step progression | **Becomes multiple SceneTemplates in sequence** OR **single Scene with multiple CardTemplates** |
| `completionNarrative` | End-of-obligation story | **Becomes SceneOutcome.OutroNarrative** |
| `completionRewards` | Rewards for finishing | **Becomes SceneOutcome.Rewards** |

**Key Insight:** Obligations are actually **ACTION-TRIGGERED SCENES**!
- The `intro` section defines the **ActionDefinition** that spawns the Scene
- The obligation itself is a **SceneTemplate** (investigation type)
- The phases could be multiple scenes OR progressive victory conditions

---

## 5. MEMORY FLAG PATTERNS

**CRITICAL FINDING:** Current tutorial content has **ZERO memory flags** or availability conditions!

**Search Results:**
- No `memoryFlags` references in JSON
- No `availabilityConditions` logic
- All goals have `isAvailable: true` (static, always available)
- All goals have `propertyRequirements: {}` (empty)

**What This Means:**
- Tutorial doesn't test conditional content
- No progressive unlocking based on player actions
- No state tracking via memory flags
- **EXTREMELY SIMPLE** migration path

**Migration Impact:** We can implement Scene/Action patterns without needing to handle complex spawn conditions initially.

---

## 6. CONTENT MIGRATION COMPLEXITY

### Complexity Categorization

| Complexity Level | Definition | Count in Tutorial |
|-----------------|------------|-------------------|
| **Simple** (No conditions, always available) | No availability logic, no memory flags, no property checks | **5 entities** (2 goals + 3 location actions) |
| **Conditional** (Simple conditions) | Single memory flag check OR single property requirement | **0 entities** |
| **Complex** (Boolean logic) | AND/OR conditions, multiple memory flags, complex requirements | **0 entities** |
| **Dynamic** (Runtime evaluation) | Conditions based on NPC state, time of day, player stats | **0 entities** |

**WORST CASE COMPLEXITY:** None! Tutorial content is entirely simple.

### Migration Complexity Estimate

**PHASE 1 (Tutorial Migration): TRIVIAL**
- 2 goals → 2 SceneTemplates (minimal field changes)
- 1 obligation → 1 SceneTemplate + 1 ActionDefinition
- 3 location actions → 3 SceneTemplates (already action-like)
- 3 player actions → 3 ActionDefinitions (already defined)
- **Estimated effort:** 2-4 hours to manually convert JSON
- **Risk level:** Very low (no complex patterns to handle)

**PHASE 2 (Full Content Migration): UNKNOWN**
- Would need to analyze actual game content (not just tutorial)
- Likely contains complex availability conditions
- Likely contains memory flag chains
- **Estimated effort:** Depends on content volume and complexity

---

## 7. SCENE TEMPLATE DESIGN (Based on Goal Patterns)

```json
{
  "id": "scene_elena_evening_service",
  "name": "Help with Evening Service",
  "description": "Assist Elena managing the evening crowd. One hour of work serving drinks and managing tables. Social engagement, uses Focus.",
  "sceneType": "Challenge",
  "challengeType": "Social",
  "deckId": "friendly_chat",
  "entryCosts": {
    "time": 1,
    "focus": 1,
    "stamina": 0,
    "coins": 0,
    "health": 0
  },
  "difficultyModifiers": [],
  "consequenceType": "Resolution",
  "resolutionMethod": "Cooperation",
  "relationshipOutcome": "Positive",
  "introNarrative": "Elena wipes down a table and glances at you. 'Evening crowd's starting. Care to help? I'll pay fair.'",
  "victoryConditions": [
    {
      "id": "evening_service_complete",
      "name": "Competent Service",
      "description": "Move through the common room with practiced ease. Drinks delivered, conversations managed, chaos ordered. Elena notices your skill.",
      "threshold": 6,
      "outcomeId": "elena_service_success"
    }
  ],
  "outcomes": [
    {
      "id": "elena_service_success",
      "outroNarrative": "Elena nods approvingly. 'Good work. You know your way around a common room.'",
      "rewards": {
        "coins": 5,
        "storyCubes": 1,
        "equipmentId": "rations"
      },
      "spawnActions": [],
      "despawnActions": []
    }
  ]
}
```

**Key Design Decisions:**

1. **SceneType enum:** `Challenge`, `Conversation`, `Investigation`, `Event`
2. **ChallengeType enum:** `Social`, `Mental`, `Physical` (only relevant if sceneType = Challenge)
3. **EntryCosts:** Unified cost structure (time/focus/stamina/coins/health)
4. **VictoryConditions:** Renamed from `goalCards`, now `List<CardTemplate>`
5. **Outcomes:** NEW - multiple possible outcomes based on which victory condition achieved
6. **SpawnActions/DespawnActions:** NEW - controls what actions appear/disappear after scene

**Differences from Goal:**
- ✅ Removed `placementNpcId` (handled by ActionDefinition)
- ✅ Removed `isAvailable` (handled by ActionDefinition spawn conditions)
- ✅ Removed `deleteOnSuccess` (handled by outcome.despawnActions)
- ✅ Removed `propertyRequirements` (not used in current content)
- ✅ Added `outcomes` (more flexible than fixed rewards)
- ✅ Added `introNarrative`/`outroNarrative` (narrative framing)

---

## 8. CARD TEMPLATE DESIGN (Based on GoalCard Patterns)

```json
{
  "id": "evening_service_complete",
  "name": "Competent Service",
  "description": "Move through the common room with practiced ease. Drinks delivered, conversations managed, chaos ordered. Elena notices your skill.",
  "cardType": "VictoryCondition",
  "threshold": 6,
  "outcomeId": "elena_service_success"
}
```

**Key Design Decisions:**

1. **CardType enum:** `VictoryCondition`, `Challenge`, `Event`, `Equipment`
2. **Threshold:** Progress required to achieve this card
3. **OutcomeId:** References which SceneOutcome to trigger

**Differences from GoalCard:**
- ✅ Removed `rewards` (moved to SceneOutcome)
- ✅ Added `outcomeId` (links to outcome)
- ✅ Added `cardType` (distinguishes from other card types)

---

## 9. ACTION DEFINITION DESIGN (Based on Obligation Intro + LocationAction Patterns)

### Pattern 1: NPC-Contextual Action (spawns Scene at NPC)

```json
{
  "id": "action_talk_to_elena",
  "name": "Talk to Elena",
  "description": "Speak with the innkeeper about work opportunities",
  "actionType": "SpawnScene",
  "priority": 100,
  "spawnSceneId": "scene_elena_evening_service",
  "spawnLocation": {
    "locationType": "NPC",
    "npcId": "elena"
  },
  "spawnConditions": {
    "requireMemoryFlags": [],
    "requirePlayerStats": {},
    "requireTimeBlocks": ["Evening"],
    "requireLocationProperties": []
  },
  "despawnConditions": {
    "onSceneOutcome": ["elena_service_success"]
  },
  "iconHint": "conversation"
}
```

### Pattern 2: Location-Contextual Action (always available at location)

```json
{
  "id": "action_rest",
  "name": "Rest",
  "description": "Take time to rest and recover. Advances 1 time segment. Restores +1 Health and +1 Stamina. Hunger increases by +5 automatically.",
  "actionType": "SpawnScene",
  "priority": 50,
  "spawnSceneId": "scene_rest",
  "spawnLocation": {
    "locationType": "LocationProperty",
    "requiredProperties": ["restful"]
  },
  "spawnConditions": {
    "requireMemoryFlags": [],
    "requirePlayerStats": {},
    "requireTimeBlocks": ["Morning", "Midday", "Afternoon", "Evening"],
    "requireLocationProperties": []
  },
  "despawnConditions": {},
  "iconHint": "rest"
}
```

### Pattern 3: Player Action (available everywhere, no scene)

```json
{
  "id": "action_check_belongings",
  "name": "Check Belongings",
  "description": "Review your current equipment and inventory",
  "actionType": "SystemCommand",
  "priority": 100,
  "systemCommand": "OpenInventory",
  "spawnLocation": {
    "locationType": "Global"
  },
  "spawnConditions": {},
  "despawnConditions": {},
  "iconHint": "inventory"
}
```

**Key Design Decisions:**

1. **ActionType enum:** `SpawnScene`, `SystemCommand`, `Transition`
2. **SpawnLocation struct:**
   - `locationType`: `NPC`, `LocationProperty`, `SpecificLocation`, `Global`
   - Context-specific fields (`npcId`, `locationId`, `requiredProperties`)
3. **SpawnConditions:** When action becomes available
4. **DespawnConditions:** When action disappears
5. **Priority:** Display order (higher = shown first)

**Differences from Current Actions:**
- ✅ Unified LocationAction + PlayerAction into single ActionDefinition
- ✅ Added `spawnLocation` (determines where action appears)
- ✅ Added `spawnConditions` (replaces `availability` array + property checks)
- ✅ Added `despawnConditions` (replaces `deleteOnSuccess`)
- ✅ Removed `cost`/`reward`/`timeRequired` (moved to Scene)

---

## 10. MIGRATION PATTERNS THAT SIMPLIFY

### Pattern 1: Always-Available Work Goals

**OLD (Goal):**
```json
{
  "id": "elena_evening_service",
  "name": "Help with Evening Service",
  "placementNpcId": "elena",
  "isAvailable": true,
  "deleteOnSuccess": false,
  "propertyRequirements": {}
}
```

**NEW (Action → Scene):**
```json
// ActionDefinition
{
  "id": "action_work_for_elena",
  "spawnSceneId": "scene_elena_evening_service",
  "spawnLocation": { "locationType": "NPC", "npcId": "elena" },
  "spawnConditions": {},  // Always available
  "despawnConditions": {}  // Never disappears
}

// SceneTemplate
{
  "id": "scene_elena_evening_service",
  "name": "Help with Evening Service",
  // ... rest of scene definition
}
```

**Simplification:** Separation of concerns - Action handles WHERE/WHEN, Scene handles WHAT/HOW.

### Pattern 2: Location Actions

**OLD (LocationAction):**
```json
{
  "id": "rest",
  "actionType": "Rest",
  "cost": {},
  "reward": { "health": 1, "stamina": 1 },
  "timeRequired": 1,
  "availability": ["Morning", "Midday", "Afternoon", "Evening"],
  "requiredProperties": ["restful"]
}
```

**NEW (Action → Scene):**
```json
// ActionDefinition
{
  "id": "action_rest",
  "actionType": "SpawnScene",
  "spawnSceneId": "scene_rest",
  "spawnLocation": {
    "locationType": "LocationProperty",
    "requiredProperties": ["restful"]
  },
  "spawnConditions": {
    "requireTimeBlocks": ["Morning", "Midday", "Afternoon", "Evening"]
  }
}

// SceneTemplate
{
  "id": "scene_rest",
  "sceneType": "Event",
  "entryCosts": { "time": 1 },
  "outcomes": [
    {
      "id": "rest_outcome",
      "rewards": { "health": 1, "stamina": 1 }
    }
  ]
}
```

**Simplification:** Location property checks moved to spawn location. Time availability moved to spawn conditions. Costs/rewards moved to Scene.

### Pattern 3: Obligation Intro → Action

**OLD (Obligation intro):**
```json
{
  "intro": {
    "triggerType": "Manual",
    "actionText": "Survey the common room",
    "locationId": "common_room",
    "introNarrative": "The common room hums with evening activity..."
  }
}
```

**NEW (Action → Scene):**
```json
// ActionDefinition
{
  "id": "action_survey_common_room",
  "name": "Survey the common room",
  "spawnSceneId": "scene_investigate_inn",
  "spawnLocation": {
    "locationType": "SpecificLocation",
    "locationId": "common_room"
  }
}

// SceneTemplate
{
  "id": "scene_investigate_inn",
  "sceneType": "Investigation",
  "introNarrative": "The common room hums with evening activity..."
}
```

**Simplification:** Action text becomes Action name. Location becomes spawn location. Narrative moves to Scene.

---

## 11. EDGE CASES THAT COMPLICATE

### Edge Case 1: Goals with Multiple Victory Conditions

**CURRENT:** Goals have `goalCards` array, but tutorial only has 1 card per goal.

**MIGRATION QUESTION:** How do multiple victory conditions work?
- Do they represent ALTERNATIVE win conditions (player chooses one)?
- Or SEQUENTIAL requirements (must achieve all)?

**SOLUTION NEEDED:** Examine non-tutorial content to see actual multi-card patterns.

### Edge Case 2: Obligations with Multiple Phases

**CURRENT:** Obligations have `phases` array, tutorial has 1 phase.

**MIGRATION QUESTION:** Are phases:
- Multiple scenes in sequence?
- Multiple cards within one scene?
- Progressive unlock pattern?

**SOLUTION:** Each phase becomes a Scene, linked via outcome spawn chains.

### Edge Case 3: Obstacles that Gate Goals

**CURRENT:** Tutorial has no obstacles.

**MIGRATION QUESTION:** If obstacles exist in real content:
- Do they gate which goals are available?
- How do they interact with availability conditions?
- Do they have their own spawn/despawn logic?

**SOLUTION NEEDED:** Analyze real content (not tutorial) to understand obstacle patterns.

### Edge Case 4: Dynamic Availability Based on NPC State

**CURRENT:** Goals have static `isAvailable: true`.

**HYPOTHETICAL:** What if a goal requires NPC relationship level?

**MIGRATION STRATEGY:**
```json
{
  "id": "action_deep_conversation",
  "spawnConditions": {
    "requireNPCRelationship": {
      "npcId": "elena",
      "minLevel": "Receptive"
    }
  }
}
```

**COMPLEXITY:** Moderate - requires SpawnConditions to support NPC state queries.

### Edge Case 5: Memory Flag Chains

**CURRENT:** No memory flags in tutorial.

**HYPOTHETICAL:** What if action requires completing previous scene?

**MIGRATION STRATEGY:**
```json
{
  "id": "action_follow_up_conversation",
  "spawnConditions": {
    "requireMemoryFlags": ["completed_first_conversation"],
    "requireMemoryFlagsAbsent": ["failed_second_challenge"]
  }
}
```

**COMPLEXITY:** Low - straightforward condition check.

---

## 12. MIGRATION COMPLEXITY ESTIMATE

### Phase 1: Tutorial Content (THIS ANALYSIS)

| Entity Type | Current Count | Migration Method | Estimated Time |
|------------|---------------|------------------|----------------|
| Goals | 2 | Manual JSON conversion | 30 minutes |
| Obligations | 1 | Manual JSON conversion | 30 minutes |
| LocationActions | 3 | Manual JSON conversion | 30 minutes |
| PlayerActions | 3 | Manual JSON conversion | 15 minutes |
| GoalCards (VictoryConditions) | 2 | Manual JSON conversion | 15 minutes |
| ActionDefinitions | 9 (new) | Manual JSON creation | 1 hour |
| **TOTAL** | **11 → 20 entities** | **Manual** | **3 hours** |

**Risk Level:** **Very Low**
- No complex conditions
- No memory flags
- No obstacle hierarchy
- Clear 1:1 mapping from old to new

**Validation Strategy:**
1. Manually convert tutorial JSON
2. Update parsers to read new format
3. Test tutorial flow end-to-end
4. Verify all actions appear in correct locations
5. Verify scenes spawn correctly
6. Verify outcomes apply correctly

### Phase 2: Full Content Migration (FUTURE)

**UNKNOWN COMPLEXITY** - Would require:
1. Inventory of full game content
2. Analysis of complex availability patterns
3. Identification of obstacle chains
4. Memory flag dependency mapping
5. NPC state interaction patterns

**Estimated Approach:**
1. Automated JSON transformation script (80% conversion)
2. Manual review of complex patterns (20% edge cases)
3. Comprehensive testing of progression chains

---

## 13. RECOMMENDED MIGRATION STRATEGY

### Step 1: Define New JSON Schemas (DONE IN THIS ANALYSIS)

✅ **SceneTemplate** schema designed
✅ **CardTemplate** schema designed
✅ **ActionDefinition** schema designed
✅ **SceneOutcome** schema designed

### Step 2: Create Tutorial Content in New Format (NEXT)

1. Manually convert `05_goals.json` → `scenes_work_goals.json`
2. Manually convert `13_obligations.json` → `scenes_investigations.json`
3. Manually convert `01_foundation.json` location/player actions → `actions_tutorial.json`
4. Create new `action_definitions.json` for spawning scenes

**Deliverable:** 4 new JSON files in new format

### Step 3: Update DTOs and Parsers (CODE CHANGES)

1. Create `SceneTemplateDTO` + `SceneParser`
2. Create `CardTemplateDTO` + `CardParser`
3. Create `ActionDefinitionDTO` + `ActionParser`
4. Create `SceneOutcomeDTO` + `OutcomeParser`

**Deliverable:** New parser classes

### Step 4: Update Domain Entities (CODE CHANGES)

1. Create `Scene` domain entity (replaces Goal)
2. Create `Card` domain entity (replaces GoalCard)
3. Create `Action` domain entity (new)
4. Update `GameWorld` to track Scenes/Cards/Actions

**Deliverable:** New domain entities

### Step 5: Update Game Logic (CODE CHANGES)

1. Update `GameFacade` to handle Action execution (spawn scenes)
2. Update challenge systems to use Scene templates
3. Update location rendering to display Actions
4. Update NPC rendering to display Actions

**Deliverable:** Refactored game logic

### Step 6: Test Tutorial Flow (VALIDATION)

1. Start game at tutorial
2. Verify actions appear at correct locations
3. Engage scenes (work goals, investigation)
4. Verify outcomes apply correctly
5. Verify progression works

**Deliverable:** Working tutorial in new architecture

### Step 7: Migrate Full Content (FUTURE)

1. Analyze full content patterns
2. Write automated conversion script
3. Run conversion
4. Manual review of edge cases
5. Comprehensive testing

**Deliverable:** Full game content in new format

---

## 14. JSON SCHEMA DEFINITIONS (For Content Creators)

### SceneTemplate Schema

```typescript
interface SceneTemplate {
  id: string;
  name: string;
  description: string;
  sceneType: "Challenge" | "Conversation" | "Investigation" | "Event";
  challengeType?: "Social" | "Mental" | "Physical";  // Only if sceneType = Challenge
  deckId?: string;  // Only if sceneType = Challenge
  entryCosts: {
    time: number;
    focus: number;
    stamina: number;
    coins: number;
    health: number;
  };
  difficultyModifiers: string[];  // e.g., ["NightTime", "Hostile"]
  consequenceType?: string;  // e.g., "Resolution", "Damage"
  resolutionMethod?: string;  // e.g., "Cooperation", "Strength"
  relationshipOutcome?: string;  // e.g., "Positive", "Neutral", "Negative"
  introNarrative: string;
  victoryConditions: CardTemplate[];
  outcomes: SceneOutcome[];
}
```

### CardTemplate Schema

```typescript
interface CardTemplate {
  id: string;
  name: string;
  description: string;
  cardType: "VictoryCondition" | "Challenge" | "Event" | "Equipment";
  threshold?: number;  // Progress required (VictoryCondition only)
  outcomeId: string;  // Which SceneOutcome to trigger
}
```

### ActionDefinition Schema

```typescript
interface ActionDefinition {
  id: string;
  name: string;
  description: string;
  actionType: "SpawnScene" | "SystemCommand" | "Transition";
  priority: number;  // Display order (higher = first)
  spawnSceneId?: string;  // Only if actionType = SpawnScene
  systemCommand?: string;  // Only if actionType = SystemCommand (e.g., "OpenInventory")
  spawnLocation: {
    locationType: "NPC" | "LocationProperty" | "SpecificLocation" | "Global";
    npcId?: string;  // Only if locationType = NPC
    locationId?: string;  // Only if locationType = SpecificLocation
    requiredProperties?: string[];  // Only if locationType = LocationProperty
  };
  spawnConditions: {
    requireMemoryFlags?: string[];
    requireMemoryFlagsAbsent?: string[];
    requirePlayerStats?: { [stat: string]: number };
    requireTimeBlocks?: string[];
    requireLocationProperties?: string[];
    requireNPCRelationship?: {
      npcId: string;
      minLevel: string;
    };
  };
  despawnConditions: {
    onSceneOutcome?: string[];  // Action disappears when these outcomes occur
    onMemoryFlag?: string[];  // Action disappears when these flags set
  };
  iconHint?: string;  // UI hint for icon selection
}
```

### SceneOutcome Schema

```typescript
interface SceneOutcome {
  id: string;
  outroNarrative: string;
  rewards: {
    coins?: number;
    experience?: number;
    storyCubes?: number;
    health?: number;
    stamina?: number;
    focus?: number;
    equipmentId?: string;
  };
  memoryFlagsGranted?: string[];
  spawnActions?: string[];  // ActionDefinition IDs to spawn
  despawnActions?: string[];  // ActionDefinition IDs to remove
}
```

---

## 15. SUMMARY AND RECOMMENDATIONS

### Current State (Tutorial Content)

✅ **Very simple structure** - No complex conditions, no memory flags, no obstacles
✅ **Clear patterns** - Goals map cleanly to Scenes, LocationActions map to Actions
✅ **Low risk migration** - Tutorial is perfect test case for new architecture
✅ **Small scope** - Only 11 entities to migrate

### New Architecture Benefits

✅ **Separation of concerns** - Actions handle WHERE/WHEN, Scenes handle WHAT/HOW
✅ **Flexible outcomes** - Multiple outcomes per scene, dynamic action spawning
✅ **Better narrative framing** - Intro/outro narratives built into scenes
✅ **Cleaner availability logic** - Spawn conditions explicit, not buried in entity state
✅ **More composable** - Actions can spawn any scene, scenes can spawn any actions

### Migration Path

1. ✅ **Tutorial first** (this analysis) - Test architecture with simple content
2. ⏸️ **Validate patterns** - Ensure new architecture works in practice
3. ⏸️ **Full content analysis** - Inventory non-tutorial content complexity
4. ⏸️ **Automated migration** - Script conversion for bulk content
5. ⏸️ **Manual edge cases** - Handle complex patterns by hand
6. ⏸️ **Testing** - Comprehensive validation of all progression chains

### Immediate Next Steps

1. **Create new JSON files** - Convert tutorial content manually
2. **Update parsers** - Support new DTO structures
3. **Update domain entities** - Scene/Card/Action classes
4. **Refactor game logic** - Handle action execution → scene spawning
5. **Test tutorial** - Validate end-to-end

**Estimated Time to Working Tutorial in New Architecture:** 8-12 hours of focused work

---

## APPENDIX: Example Full Migration

### BEFORE: Goal JSON

```json
{
  "id": "elena_evening_service",
  "name": "Help with Evening Service",
  "description": "Assist Elena managing the evening crowd. One hour of work serving drinks and managing tables. Social engagement, uses Focus.",
  "systemType": "Social",
  "placementNpcId": "elena",
  "deckId": "friendly_chat",
  "costs": { "time": 1, "focus": 1, "stamina": 0, "coins": 0 },
  "difficultyModifiers": [],
  "consequenceType": "Resolution",
  "setsResolutionMethod": "Cooperation",
  "setsRelationshipOutcome": "Positive",
  "isAvailable": true,
  "deleteOnSuccess": false,
  "propertyRequirements": {},
  "goalCards": [
    {
      "id": "evening_service_complete",
      "name": "Competent Service",
      "description": "Move through the common room with practiced ease. Drinks delivered, conversations managed, chaos ordered. Elena notices your skill.",
      "threshold": 6,
      "rewards": { "coins": 5, "storyCubes": 1, "equipmentId": "rations" }
    }
  ]
}
```

### AFTER: Scene JSON + Action JSON

**File: `scenes_work_goals.json`**
```json
{
  "packageId": "scenes_work_goals",
  "metadata": {
    "name": "Tutorial Work Scenes",
    "description": "Social and Physical work scenes for tutorial"
  },
  "content": {
    "sceneTemplates": [
      {
        "id": "scene_elena_evening_service",
        "name": "Help with Evening Service",
        "description": "Assist Elena managing the evening crowd. One hour of work serving drinks and managing tables. Social engagement, uses Focus.",
        "sceneType": "Challenge",
        "challengeType": "Social",
        "deckId": "friendly_chat",
        "entryCosts": { "time": 1, "focus": 1, "stamina": 0, "coins": 0, "health": 0 },
        "difficultyModifiers": [],
        "consequenceType": "Resolution",
        "resolutionMethod": "Cooperation",
        "relationshipOutcome": "Positive",
        "introNarrative": "Elena wipes down a table and glances at you. 'Evening crowd's starting. Care to help? I'll pay fair.'",
        "victoryConditions": [
          {
            "id": "evening_service_complete",
            "name": "Competent Service",
            "description": "Move through the common room with practiced ease. Drinks delivered, conversations managed, chaos ordered. Elena notices your skill.",
            "cardType": "VictoryCondition",
            "threshold": 6,
            "outcomeId": "elena_service_success"
          }
        ],
        "outcomes": [
          {
            "id": "elena_service_success",
            "outroNarrative": "Elena nods approvingly. 'Good work. You know your way around a common room.'",
            "rewards": { "coins": 5, "storyCubes": 1, "equipmentId": "rations" },
            "memoryFlagsGranted": [],
            "spawnActions": [],
            "despawnActions": []
          }
        ]
      }
    ]
  }
}
```

**File: `actions_tutorial.json`**
```json
{
  "packageId": "actions_tutorial",
  "metadata": {
    "name": "Tutorial Action Definitions",
    "description": "Actions that spawn tutorial scenes"
  },
  "content": {
    "actionDefinitions": [
      {
        "id": "action_work_for_elena",
        "name": "Work for Elena",
        "description": "Help with evening service (Social challenge, 1 Focus)",
        "actionType": "SpawnScene",
        "priority": 100,
        "spawnSceneId": "scene_elena_evening_service",
        "spawnLocation": {
          "locationType": "NPC",
          "npcId": "elena"
        },
        "spawnConditions": {
          "requireTimeBlocks": ["Evening"]
        },
        "despawnConditions": {},
        "iconHint": "conversation"
      }
    ]
  }
}
```

---

**END OF ANALYSIS**
