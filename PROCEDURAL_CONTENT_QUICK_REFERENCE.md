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

## The 21 Situation Archetypes (SituationArchetypeCatalog)

**Architecture:** 3 tiers of increasing specificity
- **5 Core Archetypes:** Fundamental interaction types
- **10 Expanded Archetypes:** Domain-specific variations
- **6 Specialized Service Archetypes:** Multi-phase service flows

**Universal Scaling:** ALL archetypes benefit from categorical property scaling (NPCDemeanor, Quality, PowerDynamic, EnvironmentQuality). Same archetype + different properties = contextually appropriate difficulty.

### TIER 1: Core Archetypes (5)

#### 1. CONFRONTATION (Authority/Dominance)
- **When:** Gatekeepers, obstacles, authority challenges
- **Primary Stat:** Authority | **Coin Cost:** 15 | **Challenge Type:** Physical
- **Pattern:** Authority stat → Pay off → Physical challenge → Submit

#### 2. NEGOTIATION (Diplomacy/Trade)
- **When:** Merchants, transactional exchanges, deals
- **Primary Stat:** Diplomacy/Rapport | **Coin Cost:** 15 | **Challenge Type:** Mental
- **Pattern:** Persuade → Pay premium → Debate → Accept terms

#### 3. INVESTIGATION (Insight/Discovery)
- **When:** Information gathering, puzzle solving, deduction
- **Primary Stat:** Insight/Cunning | **Coin Cost:** 10 | **Challenge Type:** Mental
- **Pattern:** Deduce → Hire expert → Work puzzle → Give up

#### 4. SOCIAL MANEUVERING (Rapport/Manipulation)
- **When:** Social circles, subtle influence, persuasion
- **Primary Stat:** Rapport/Cunning | **Coin Cost:** 10 | **Challenge Type:** Social
- **Pattern:** Read people → Offer gift → Bold gambit → Alienate

#### 5. CRISIS (Emergency Response)
- **When:** Urgent situations, decisive action, time pressure
- **Primary Stat:** Authority/Insight | **Coin Cost:** 25 | **Challenge Type:** Physical
- **Pattern:** Expert action → Expensive solution → Personal risk → Flee

---

### TIER 2: Expanded Archetypes (10)

#### 6. SERVICE TRANSACTION
- **When:** Paying for services (lodging, food, passage)
- **Primary Stat:** None | **Coin Cost:** 5 | **Challenge Type:** Mental

#### 7. ACCESS CONTROL
- **When:** Gatekeepers, locked doors, restricted areas
- **Primary Stat:** Authority/Cunning | **Coin Cost:** 15 | **Challenge Type:** Physical

#### 8. INFORMATION GATHERING
- **When:** Rumors, gossip, local knowledge
- **Primary Stat:** Rapport/Insight | **Coin Cost:** 8 | **Challenge Type:** Social

#### 9. SKILL DEMONSTRATION
- **When:** Proving competence, showing credentials
- **Primary Stat:** Diplomacy/Insight | **Coin Cost:** 12 | **Challenge Type:** Mental

#### 10. REPUTATION CHALLENGE
- **When:** Defending honor, responding to accusations
- **Primary Stat:** Authority/Diplomacy | **Coin Cost:** 10 | **Challenge Type:** Social

#### 11. EMERGENCY AID
- **When:** Medical crisis, rescue situations
- **Primary Stat:** Insight/Authority | **Coin Cost:** 20 | **Challenge Type:** Physical

#### 12. ADMINISTRATIVE PROCEDURE
- **When:** Bureaucracy, permits, official processes
- **Primary Stat:** Diplomacy/Insight | **Coin Cost:** 12 | **Challenge Type:** Mental

#### 13. TRADE DISPUTE
- **When:** Disagreements over goods, quality, terms
- **Primary Stat:** Insight/Diplomacy | **Coin Cost:** 15 | **Challenge Type:** Mental

#### 14. CULTURAL FAUX PAS
- **When:** Social blunders, tradition violations
- **Primary Stat:** Rapport/Insight | **Coin Cost:** 10 | **Challenge Type:** Social

#### 15. RECRUITMENT
- **When:** Join requests, commitment decisions
- **Primary Stat:** Cunning/Diplomacy | **Coin Cost:** 8 | **Challenge Type:** Social

---

### TIER 3: Specialized Service Archetypes (6)

These compose into complete multi-situation service flows (inn_lodging, bathhouse_service, healer_treatment):

#### 16. SERVICE_NEGOTIATION
- **Pattern:** 4 choices (stat/money/challenge/fallback) → Secures service access → Grants key/token item

#### 17. SERVICE_EXECUTION_REST
- **Pattern:** 4 rest variants (balanced/physical/mental/special) → Restores resources → Advances to next day Morning

#### 18. SERVICE_DEPARTURE
- **Pattern:** 2 choices (immediate/careful) → Cleanup and exit → Optional preparation buff

#### 19. REST_PREPARATION
- **Pattern:** Preparing to rest → Optimize recovery → Comfort items → Force relaxation → Collapse

#### 20. ENTERING_PRIVATE_SPACE
- **Pattern:** First entry into private room → Inspect and optimize → Request amenities → Push through → Collapse

#### 21. DEPARTING_PRIVATE_SPACE
- **Pattern:** Leaving private space → Check carefully → Leave gratuity → Rush out

---

**Archetype Reusability:** Each archetype is a mechanical pattern (typically 4 choices, path types, cost/requirement formulas, rewards). AI generates narrative from entity context at finalization. Use categorical placement filters, not concrete NPC IDs. Same archetypes reused across entire game via property combinations.

**Implementation Evidence:** SituationArchetypeCatalog.cs lines 19-720

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