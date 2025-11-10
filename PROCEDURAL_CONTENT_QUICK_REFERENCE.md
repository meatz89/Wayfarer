# Procedural Content Generation - Quick Reference

## Architecture Overview

```
JSON Package
    ↓
SceneTemplateParser
    ├─→ Parse SceneTemplate
    ├─→ Parse SituationTemplates
    │   ├─→ If ArchetypeId specified:
    │   │   └─→ SituationArchetypeCatalog.GetArchetype()
    │   │       └─→ Generates 4 ChoiceTemplates (PARSE-TIME)
    │   └─→ If ArchetypeId null:
    │       └─→ Parse hand-authored ChoiceTemplates
    ├─→ Parse PlacementFilter
    ├─→ Parse SpawnConditions (3 dimensions)
    └─→ Store in GameWorld.SceneTemplates
           (embedded SituationTemplates stored inline)

        ↓↓↓ RUNTIME ↓↓↓

Game Advancement Event (Time, Location, NPC)
    ↓
SpawnFacade.CheckAndSpawnEligibleScenes()
    ├─→ Query procedural SceneTemplates (isStarter=false)
    ├─→ For each template:
    │   └─→ SpawnConditionsEvaluator.EvaluateAll()
    │       ├─→ PlayerState conditions
    │       ├─→ WorldState conditions (time, weather, day range)
    │       └─→ EntityState conditions (bonds, reputation, travels)
    │
    └─→ If eligible:
        └─→ SceneInstantiator.CreateProvisionalScene()
            ├─→ Check spawn conditions
            ├─→ Resolve placement via PlacementFilter
            │   ├─→ FindMatchingNPC/Location/Route()
            │   ├─→ ApplySelectionStrategy()
            │   │   ├─→ Closest (hex distance)
            │   │   ├─→ HighestBond (NPC only)
            │   │   ├─→ LeastRecent (interaction history)
            │   │   └─→ WeightedRandom (default)
            │   └─→ Return selected entity ID
            ├─→ Store scene in GameWorld.Scenes with State=Provisional
            └─→ SituationIds = empty (deferred instantiation)

Player selects Choice
    ↓
SceneInstantiator.FinalizeScene()
    ├─→ Instantiate Situations from SituationTemplates
    ├─→ Add Situations to GameWorld.Situations
    ├─→ Generate AI narrative (if template lacks narrative)
    ├─→ Replace placeholders ({NPCName}, {LocationName}, etc.)
    ├─→ Set CurrentSituationId
    ├─→ Change State from Provisional to Active
    └─→ Return finalized Scene

Player engages Situation
    ↓
Choice selection triggers:
    ├─→ SceneSpawnReward (spawn new scene)
    │   └─→ Cascading Provisional → Finalized scenes
    ├─→ Consequence (bonds, scales, states)
    └─→ Scene advancement (SpawnRules)
        └─→ Scene.AdvanceToNextSituation()

Scene completion
    ↓
SpawnFacade.ExecuteSpawnRules()
    ├─→ Clone child Situations from templates
    ├─→ Apply requirement offsets
    ├─→ Add to GameWorld.Situations
    └─→ Trigger cascade pattern
```

---

## Core Entities Quick Reference

### SceneTemplate
- **Where:** GameWorld.SceneTemplates
- **Immutable:** Yes (archetype)
- **Contains:** SituationTemplates (embedded), PlacementFilter, SpawnConditions
- **Key Properties:**
  - Id, Archetype (SpawnPattern enum)
  - PlacementFilter (categorical: PersonalityTypes, LocationProperties, TerrainTypes)
  - SpawnConditions (PlayerState, WorldState, EntityState)
  - IsStarter (false = procedural)
  - Tier (0-4 difficulty), ExpirationDays
  - PresentationMode (Atmospheric/Modal), ProgressionMode (Breathe/Cascade)

### SituationTemplate
- **Where:** Embedded in SceneTemplate.SituationTemplates
- **Immutable:** Yes (archetype)
- **Key Properties:**
  - Id, Type (Normal/Crisis)
  - **ArchetypeId** ("confrontation", "negotiation", "investigation", "social_maneuvering", "crisis")
    - If specified → SituationArchetypeCatalog generates 4 ChoiceTemplates
    - If null → Parse hand-authored ChoiceTemplates
  - NarrativeTemplate (with {Placeholders})
  - ChoiceTemplates (2-4 per Sir Brante pattern)

### Scene (Instance)
- **Where:** GameWorld.Scenes (unified collection)
- **State:** Provisional → Active → Completed/Expired
- **Placement:** PlacementType (Location/NPC/Route) + PlacementId
- **Content:** SituationIds (empty if Provisional, populated if Active)
- **Progress:** CurrentSituationId (tracks player position)

### Situation (Instance)
- **Where:** Scene.Situations (embedded, owned by Scene)
- **Deferred:** Yes (actions created at query time, not instantiation time)
- **Placement:** Inherited from parent Scene via GetPlacementId()
- **Narrative:** From template or AI-generated

---

## Spawn Conditions - Three Dimensions

### PlayerStateConditions
- CompletedScenes (List) - "Only after intro completed"
- ChoiceHistory (List) - "Only if player chose to help Marcus"
- MinStats (Dict<ScaleType, int>) - "Morality +5+"
- RequiredItems (List) - "Must have investigation notes"
- LocationVisits (Dict<string, int>) - "Visited tavern 3+ times"

### WorldStateConditions
- Weather (nullable enum) - "Only in rain"
- TimeBlock (nullable enum) - "Only at night"
- MinDay, MaxDay (nullable int) - "Days 3-10 only"
- LocationStates (List) - "Only at flooded locations"

### EntityStateConditions
- NPCBond (Dict<string, int>) - "Elena bond 10+"
- LocationReputation (Dict<string, int>) - "Market reputation 5+"
- RouteTravelCount (Dict<string, int>) - "Mountain pass 2+ traversals"
- Properties (List) - "Only dangerous, remote entities"

**Combination:** AND (all must pass) OR (any passes)

---

## Placement Filter - Categorical Selection

### Structure
```
PlacementFilter
├─ PlacementType: Location | NPC | Route
├─ SelectionStrategy: WeightedRandom | Closest | HighestBond | LeastRecent
│
├─ [NPC Filters]
│  ├─ PersonalityTypes: [Mercantile, Commanding, etc.]
│  ├─ MinBond, MaxBond: 0-20 scale
│  └─ NpcTags: ["Wealthy", "UrbanResident"] [TODO]
│
├─ [Location Filters]
│  ├─ LocationProperties: [Commercial, Secluded, Indoor, etc.]
│  ├─ LocationTags: ["Marketplace", "Noble"] [TODO]
│  ├─ DistrictId, RegionId [TODO]
│
├─ [Route Filters]
│  ├─ TerrainTypes: ["Urban", "Forest", "Mountain"]
│  ├─ RouteTier: 0-4
│  └─ DangerRating: 0-100
│
└─ [Player State Filters - All Types]
   ├─ RequiredStates: [Injured, Exhausted, Wanted]
   ├─ ForbiddenStates: [Rested, Healthy]
   ├─ RequiredAchievements: ["FirstCombat", etc.]
   └─ ScaleRequirements: [{ ScaleType: Morality, MinValue: 5 }]
```

### Selection Strategy Behavior

| Strategy | NPC | Location | Route | Tie-breaker |
|----------|-----|----------|-------|-------------|
| **Closest** | Hex distance | Hex distance | N/A | Random |
| **HighestBond** | Highest bond | N/A | N/A | Random |
| **LeastRecent** | Oldest interaction | Fewest visits | Oldest traversal | Never interacted |
| **WeightedRandom** | Random | Random | Random | (default) |

---

## The Five Archetypes (SituationArchetypeCatalog)

### 1. CONFRONTATION (Authority)
- **When:** Authority challenges, intimidation, physical barriers
- **Primary Stat:** Authority
- **Coin Cost:** 15
- **Challenge Type:** Physical
- **4 Choices:**
  1. Authority 3+ (free, best)
  2. 15 coins (expensive)
  3. Physical challenge (risky)
  4. Fallback/Submit (poor, always available)

### 2. NEGOTIATION (Economic)
- **When:** Price disputes, deal-making, compromise
- **Primary Stat:** Diplomacy/Rapport
- **Coin Cost:** 15
- **Challenge Type:** Mental
- **4 Choices:**
  1. Diplomacy/Rapport 3+ (free, best)
  2. 15 coins (expensive)
  3. Mental challenge (risky)
  4. Fallback/unfavorable terms (poor)

### 3. INVESTIGATION (Mental)
- **When:** Mysteries, puzzles, information gathering
- **Primary Stat:** Insight/Cunning
- **Coin Cost:** 10
- **Challenge Type:** Mental
- **4 Choices:**
  1. Insight/Cunning 3+ (free, best)
  2. 10 coins/informant (moderate)
  3. Mental challenge (risky)
  4. Fallback/guess (poor)

### 4. SOCIAL MANEUVERING (Social)
- **When:** Reputation, relationship building, social hierarchy
- **Primary Stat:** Rapport/Cunning
- **Coin Cost:** 10
- **Challenge Type:** Social
- **4 Choices:**
  1. Rapport/Cunning 3+ (free, best)
  2. 10 coins/gift (transactional)
  3. Social challenge (risky)
  4. Fallback/alienate (poor)

### 5. CRISIS (Physical)
- **When:** Emergencies, high-stakes, moral dilemmas
- **Primary Stat:** Authority/Insight
- **Coin Cost:** 25
- **Challenge Type:** Physical
- **4 Choices:**
  1. Authority 4+ (high requirement, best)
  2. 25 coins (very expensive)
  3. Physical challenge (risky)
  4. Fallback/flee (worst)

---

## Key Enums

### SpawnPattern (Scene archetype)
```
Linear, HubAndSpoke, Branching, Converging, Conditional,
Exclusive, Sequential, Recursive, Hybrid, Standalone
```

### SceneState (Lifecycle)
```
Provisional (awaiting Choice)
Active (finalized, playable)
Completed (all situations finished)
Expired (time limit reached)
```

### PlacementType
```
Location, NPC, Route
```

### PlacementSelectionStrategy
```
WeightedRandom (default), Closest, HighestBond, LeastRecent
```

### PresentationMode
```
Atmospheric (menu option), Modal (full screen)
```

### ProgressionMode
```
Breathe (return to menu), Cascade (continuous)
```

---

## Common Patterns

### Pattern 1: Time-Based Scene Spawn
```json
{
  "id": "night_market",
  "spawnConditions": {
    "worldState": {
      "timeBlock": "Evening",
      "minDay": 3
    }
  }
}
```

### Pattern 2: State-Based Scene Spawn
```json
{
  "id": "injured_healer",
  "placementFilter": {
    "placementType": "NPC",
    "personalityTypes": ["Healer"]
  },
  "spawnConditions": {
    "playerState": {
      "minStats": { "Health": 5 }  // Only if injured
    }
  }
}
```

### Pattern 3: Archetype-Driven Choices
```json
{
  "id": "guard_encounter",
  "situationTemplates": [
    {
      "id": "confrontation",
      "archetypeId": "confrontation"  // Parser generates 4 choices
    }
  ]
}
```

### Pattern 4: Perfect Information Preview
```
Scene State: Provisional
├─ DisplayName: "Night Market Encounter"
├─ SituationCount: 2
├─ EstimatedDifficulty: "Standard"
└─ PlacementId: "market_square"
```

---

## Fail-Safe Principles

1. **Sentinel Values:** SpawnConditions.AlwaysEligible (never null)
2. **Fail Fast:** Parser throws on invalid archetype ID
3. **Strong Typing:** PlacementSelectionStrategy is enum (no strings)
4. **Parse-Time Translation:** Catalogue converts categorical → choices (NEVER at runtime)
5. **Provisional Validation:** Check conditions before creating scene
6. **Perfect Information:** Provisional scene shows all metadata before finalization

---