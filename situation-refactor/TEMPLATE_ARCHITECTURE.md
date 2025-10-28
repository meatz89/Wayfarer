# Template Architecture Principle: Three-Level Design

## CRITICAL ARCHITECTURAL FOUNDATION

This document defines the **THREE-LEVEL ARCHITECTURE** for Scene and Situation Spawn Templates.

**LAYER SEPARATION:**
- **STRATEGIC LAYER**: Scene → Situation → Actions (LocationAction/ConversationOption/TravelCard)
- **TACTICAL LAYER**: SituationCard (victory conditions INSIDE challenges - separate system)

**KEY ENTITIES:**
- **Scene** = Ephemeral spawning orchestrator containing multiple Situations in configurations (sequential/parallel/branching)
- **Situation** = Persistent narrative context at location/NPC/route containing 2-4 action references
- **Actions** = Existing entities (LocationAction, ConversationOption, TravelCard) that are response options
- **SituationCard** = TACTICAL LAYER ONLY (victory conditions inside challenges, NOT part of Scene/Situation architecture)

---

## The Three Levels (Never Mix These)

### LEVEL 1: PATTERN (Documentation Layer)

**What It Is:**
- Pure conceptual structures described in markdown documentation
- Authoring guidance for content creators
- NO JSON, NO CODE - just human-readable patterns

**Examples:**
- "Linear Progression" - Scene spawns Situation 1 → Situation 1 completion spawns Situation 2
- "Hub-and-Spoke" - Scene spawns central Situation with multiple parallel follow-up Situations
- "Branching Consequences" - Situation success/failure spawn different follow-up Situations
- "Discovery Chain" - Completing Situations reveals new Situations at new locations

**Purpose:**
- Provide reusable conceptual frameworks
- Document narrative structures that work well
- Guide decision-making when creating content
- Lives ONLY in `situation-spawn-patterns.md`

**Correct Example (conceptual only):**
```markdown
### Pattern: The Plea and the Investigation

**Structure:**
- Scene spawns "plea for help" Situation at NPC
- Situation contains 4 ConversationOptions:
  - "Agree to help" → Launches Social challenge
  - "Offer payment" → Instant (costs coins, spawns investigation)
  - "Investigate alone" → Launches Mental challenge
  - "Decline" → Instant (bond loss)
- Success spawns investigation Situation at related location

**Use Cases:**
- Mystery quests, rescue missions, information gathering
```

---

### LEVEL 2: TEMPLATE (JSON Archetype Layer)

**What It Is:**
- Immutable archetype definitions with formulas and categorical filters
- Shared by ALL instances spawned from it
- **TWO TEMPLATE TYPES:**
  - **SceneTemplate**: Orchestrates spawning of multiple related Situations
  - **SituationTemplate**: Defines individual Situation archetypes with 2-4 action references

#### SceneTemplate (Spawning Orchestrator)

**Lives in:** `GameWorld.SceneTemplates` (List, NOT Dictionary)

**Purpose:** Defines a spawn event that creates multiple Situations and places them at locations/NPCs/routes

**What SceneTemplate JSON MUST Contain:**
- ✅ List of SituationTemplate references with spawn configuration
- ✅ Spawn sequencing (sequential, parallel, branching)
- ✅ Placement strategies per Situation
- ✅ Categorical entity filters (`"npcArchetype": "Innocent"`)
- ✅ Scene lifecycle rules (when to discard)

**What SceneTemplate JSON MUST NOT Contain:**
- ❌ Specific entity IDs (`"npcId": "elena"` - instance data)
- ❌ Narrative content (Situations contain narrative, not Scenes)
- ❌ Action definitions (Situations reference actions, not Scenes)

**Correct SceneTemplate JSON Structure:**
```json
{
  "id": "rescue_quest_scene_template",
  "archetype": "RescueQuest",
  "tier": 1,
  "situationSpawns": [
    {
      "situationTemplateId": "rescue_plea_template",
      "placementStrategy": "SelectedNpc",
      "npcFilters": {
        "archetype": "Innocent",
        "locationProximity": "CurrentLocation"
      },
      "spawnTiming": "Immediate"
    },
    {
      "situationTemplateId": "investigation_followup_template",
      "placementStrategy": "RelatedLocation",
      "spawnTiming": "OnPreviousSituationSuccess",
      "locationFilters": {
        "locationType": "Urban",
        "distanceFromNpc": "Adjacent"
      }
    }
  ],
  "discardCondition": "AllSituationsSpawned"
}
```

#### SituationTemplate (Narrative Context Archetype)

**Lives in:** `GameWorld.SituationTemplates` (List, NOT Dictionary)

**Purpose:** Defines a single persistent narrative context archetype with 2-4 action references

**What SituationTemplate JSON MUST Contain:**
- ✅ Archetype enums (`"archetype": "Rescue"`)
- ✅ Narrative hints for AI generation
- ✅ **2-4 Action references** (LocationAction IDs, ConversationOption IDs, or TravelCard IDs)
- ✅ Placement type (Location, NPC, or Route)
- ✅ Requirement formulas
- ✅ Success/failure spawn rules

**What SituationTemplate JSON MUST NOT Contain:**
- ❌ Specific entity IDs for placement (`"npcId": "elena"` - instance data)
- ❌ Fixed requirement thresholds (`"threshold": 5` - should be formula)
- ❌ Runtime state (`"spawnedDay"`, `"completedDay"`)

**Correct SituationTemplate JSON Structure:**
```json
{
  "id": "rescue_plea_template",
  "archetype": "Rescue",
  "tier": 1,
  "placementType": "NPC",
  "narrativeHints": {
    "tone": "Urgent, desperate",
    "theme": "Plea for help",
    "setup": "NPC requests rescue of captured relative"
  },
  "actionReferences": [
    {
      "actionId": "agree_help_conversation",
      "actionType": "ConversationOption",
      "requirementFormula": {
        "orPaths": [
          {
            "numericRequirements": [
              {
                "type": "BondStrength",
                "baseValue": "CurrentPlayerBond",
                "offset": 3,
                "label": "Need trust to accept dangerous mission"
              }
            ]
          }
        ]
      },
      "onSuccess": {
        "spawns": [
          {
            "childTemplateId": "investigation_followup_template",
            "placementStrategy": "SameNpc"
          }
        ]
      }
    },
    {
      "actionId": "offer_payment_conversation",
      "actionType": "ConversationOption",
      "costs": {
        "coins": 50
      },
      "effects": {
        "spawns": [
          {
            "childTemplateId": "investigation_followup_template",
            "placementStrategy": "RelatedLocation"
          }
        ]
      }
    },
    {
      "actionId": "investigate_alone_conversation",
      "actionType": "ConversationOption",
      "requirementFormula": {
        "orPaths": [
          {
            "numericRequirements": [
              {
                "type": "StatThreshold",
                "stat": "Cunning",
                "threshold": 12
              }
            ]
          }
        ]
      }
    },
    {
      "actionId": "decline_conversation",
      "actionType": "ConversationOption",
      "effects": {
        "bondChanges": [
          {
            "npcId": "ContextNpc",
            "bondDelta": -5
          }
        ]
      }
    }
  ]
}
```

**CRITICAL: Situation References Existing Action Entities (Sir Brante Pattern)**

Each Situation is a narrative context containing:
- Narrative setup (generated from hints)
- 2-4 action references (LocationAction, ConversationOption, or TravelCard)
- Each action may:
  - Trigger Social/Mental/Physical challenge
  - Execute instantly with costs/rewards (Sir Brante)
  - Navigate to new location
  - Spawn new Situations/Scenes

---

### LEVEL 3: INSTANCE (Runtime Entity Layer)

**What It Is:**
- Concrete runtime entities created from templates
- **TWO INSTANCE TYPES:**
  - **Scene**: Ephemeral spawning container (discarded after spawning Situations)
  - **Situation**: Persistent narrative context (lives in `GameWorld.Situations`)

#### Scene Instance (Ephemeral Spawner)

**Lives in:** NOT stored in GameWorld (ephemeral - created and discarded)

**Lifecycle:**
1. Instantiated from SceneTemplate
2. Selects entities using categorical filters
3. Spawns Situations from referenced SituationTemplates
4. Places Situations at selected locations/NPCs/routes
5. Discarded when spawn complete

**Composition Architecture:**
```csharp
public class Scene
{
    // Composition: Reference shared immutable template
    public SceneTemplate Template { get; set; }

    // Ephemeral runtime state ONLY
    public string Id { get; set; }
    public List<NPC> SelectedNpcs { get; set; }  // Concrete entities selected by filters
    public List<Location> SelectedLocations { get; set; }
    public List<Situation> SpawnedSituations { get; set; }  // Created and placed
    public SceneStatus Status { get; set; }  // Active, Spawning, Completed
}
```

#### Situation Instance (Persistent Narrative Context)

**Lives in:** `GameWorld.Situations` (List, NOT Dictionary)

**Lifecycle:**
1. Created by Scene from SituationTemplate
2. Placed at specific Location/NPC/Route
3. Persists in GameWorld after Scene discarded
4. Player sees 2-4 action options
5. Player selects action (triggers challenge OR executes instantly)

**Composition Architecture:**
```csharp
public class Situation
{
    // Composition: Reference shared immutable template (NOT cloned)
    public SituationTemplate Template { get; set; }

    // Runtime instance properties ONLY
    public string Id { get; set; }
    public NPC PlacementNpc { get; set; }  // Concrete entity (Scene selects)
    public Location PlacementLocation { get; set; }  // Concrete entity (Scene selects)
    public Route PlacementRoute { get; set; }  // Concrete entity (Scene selects)

    // Action references (2-4 actions from template)
    // These reference EXISTING action entities in GameWorld
    public List<string> LocationActionIds { get; set; }  // If at location
    public List<string> ConversationOptionIds { get; set; }  // If with NPC
    public List<string> TravelCardIds { get; set; }  // If on route

    public SituationStatus Status { get; set; }
    public int SpawnedDay { get; set; }
    public string GeneratedNarrative { get; set; }  // AI-generated or JSON fallback
    public Scene ParentScene { get; set; }  // Scene that spawned this
}
```

**Access actions at runtime:**
```csharp
// ✅ CORRECT - Situation references existing action entities
Situation situation = gameWorld.Situations
    .FirstOrDefault(s => s.PlacementLocation == playerLocation);

// Get action entities from GameWorld
List<LocationAction> actions = situation.LocationActionIds
    .Select(id => gameWorld.LocationActions.FirstOrDefault(a => a.Id == id))
    .ToList();

// Player selects an action
LocationAction selectedAction = actions[0];

// Execute action (may trigger challenge or be instant)
if (selectedAction.ActionType == ActionType.Challenge)
{
    StartChallenge(selectedAction.ChallengeType);
}
else
{
    ExecuteInstantAction(selectedAction);
}
```

---

## The Complete Flow (Spawn to Play)

### 1. Scene Instantiation (Ephemeral Spawner)
```csharp
// Select SceneTemplate
SceneTemplate sceneTemplate = gameWorld.SceneTemplates
    .FirstOrDefault(t => t.Id == "rescue_quest_scene_template");

// Instantiate Scene
Scene scene = new Scene
{
    Template = sceneTemplate,
    Id = GenerateUniqueId(),
    Status = SceneStatus.Active
};
```

### 2. Scene Spawns Situations (Persistent Contexts)
```csharp
foreach (var spawn in sceneTemplate.SituationSpawns)
{
    // Select SituationTemplate
    SituationTemplate sitTemplate = gameWorld.SituationTemplates
        .FirstOrDefault(t => t.Id == spawn.SituationTemplateId);

    // Select entities using categorical filters
    NPC targetNpc = gameWorld.NPCs
        .Where(npc => npc.Archetype == spawn.NpcFilters.Archetype)
        .Where(npc => npc.Location == currentLocation)
        .FirstOrDefault();

    // Instantiate Situation with action references
    Situation situation = new Situation
    {
        Template = sitTemplate,
        Id = GenerateUniqueId(),
        PlacementNpc = targetNpc,
        PlacementLocation = targetNpc.Location,
        ConversationOptionIds = sitTemplate.ActionReferences
            .Where(a => a.ActionType == "ConversationOption")
            .Select(a => a.ActionId)
            .ToList(),
        Status = SituationStatus.Available,
        SpawnedDay = currentDay,
        ParentScene = scene
    };

    // Add to GameWorld (persists after Scene discarded)
    gameWorld.Situations.Add(situation);
    scene.SpawnedSituations.Add(situation);
}
```

### 3. Scene Discarded, Situations Persist
```csharp
// Scene completes spawning
scene.Status = SceneStatus.Completed;

// Scene discarded (not stored anywhere)
// Situations remain in gameWorld.Situations

// Player can now interact with spawned Situations
```

### 4. Player Interacts with Situation Actions
```csharp
// Player sees Situation with 2-4 actions
Situation activeSituation = gameWorld.Situations
    .FirstOrDefault(s => s.PlacementLocation == playerLocation
                      && s.Status == SituationStatus.Available);

// Get action entities
List<ConversationOption> options = activeSituation.ConversationOptionIds
    .Select(id => gameWorld.ConversationOptions.FirstOrDefault(o => o.Id == id))
    .ToList();

// Player selects an action
ConversationOption selectedOption = options[0];

if (selectedOption.TriggersChallenge)
{
    // Launch Social/Mental/Physical challenge
    StartChallenge(selectedOption.ChallengeType);
}
else
{
    // Execute instant action with costs/rewards (Sir Brante pattern)
    ApplyCosts(selectedOption.Costs);
    ApplyRewards(selectedOption.Rewards);
}
```

---

## Strong Typing Enforcement (NO DICTIONARIES, NO HASHSETS)

### FORBIDDEN:
```csharp
// ❌ WRONG - Dictionary lookup pattern
public Dictionary<string, SceneTemplate> SceneTemplates { get; set; }
public Dictionary<string, SituationTemplate> SituationTemplates { get; set; }

// ❌ WRONG - HashSet
public HashSet<Situation> Situations { get; set; }
```

### REQUIRED:
```csharp
// ✅ CORRECT - Lists with LINQ queries
public List<SceneTemplate> SceneTemplates { get; set; } = new();
public List<SituationTemplate> SituationTemplates { get; set; } = new();
public List<Situation> Situations { get; set; } = new();

SceneTemplate template = gameWorld.SceneTemplates
    .FirstOrDefault(t => t.Id == "rescue_quest_scene_template");
```

---

## Composition Over Cloning (NEVER CLONE TEMPLATES)

### WHY COMPOSITION:
1. **Single source of truth** - Template changes propagate to all instances
2. **Memory efficient** - Template data stored once, not duplicated per instance
3. **Type safety** - Access template properties through instance.Template reference
4. **Clear separation** - Runtime state vs design-time archetypes

### CORRECT Pattern:
```csharp
// Scene instance references SceneTemplate
Scene scene = new Scene { Template = sceneTemplate };

// Situation instance references SituationTemplate
Situation situation = new Situation { Template = situationTemplate };

// Actions referenced by ID, NOT embedded
List<string> actionIds = situation.ConversationOptionIds;
```

### WRONG Pattern:
```csharp
// ❌ Cloning template properties into instance
Situation situation = new Situation
{
    Archetype = template.Archetype,  // Duplicated!
    NarrativeHints = template.NarrativeHints  // Duplicated!
};

// ❌ Embedding action entities inline
Situation situation = new Situation
{
    Actions = new List<ConversationOption> { ... }  // WRONG! Reference by ID!
};
```

---

## Summary Table

| Level | Entity Type | Lives In | Contains | Purpose |
|-------|-------------|----------|----------|---------|
| **PATTERN** | Conceptual | Markdown docs | Spawn patterns, use cases | Guide content authoring |
| **TEMPLATE** | SceneTemplate | `List<SceneTemplate>` | Multiple SituationTemplate references + spawn config | Define spawn orchestration |
| **TEMPLATE** | SituationTemplate | `List<SituationTemplate>` | Narrative + 2-4 action references | Define context archetypes |
| **INSTANCE** | Scene | NOT stored (ephemeral) | Spawns Situations, then discarded | Execute spawn event |
| **INSTANCE** | Situation | `List<Situation>` | Narrative + 2-4 action ID references | Persistent playable context |

---

## Validation Checklist

Before any template implementation:

**Pattern (Documentation):**
- [ ] Pattern documentation in markdown (NO JSON, NO entity IDs)
- [ ] Clear description of spawn flow (Scene → Situations → Actions)

**SceneTemplate (Spawning Orchestrator):**
- [ ] References SituationTemplates by ID (NOT inline definitions)
- [ ] Has categorical entity filters (NO specific entity IDs)
- [ ] Defines placement strategies and spawn timing
- [ ] Lives in `List<SceneTemplate>` (NOT Dictionary)

**SituationTemplate (Narrative Context Archetype):**
- [ ] Contains 2-4 action references (LocationAction/ConversationOption/TravelCard IDs)
- [ ] Has narrative hints for AI generation
- [ ] Has formulas with offsets (NO fixed thresholds)
- [ ] Actions reference EXISTING entities by ID
- [ ] Lives in `List<SituationTemplate>` (NOT Dictionary)

**Scene Instance (Ephemeral Spawner):**
- [ ] References SceneTemplate via composition (NOT cloned)
- [ ] Selects entities using LINQ queries (NOT dictionary lookups)
- [ ] Spawns Situations and adds to GameWorld.Situations
- [ ] Discarded after spawning (NOT stored anywhere)

**Situation Instance (Persistent Context):**
- [ ] References SituationTemplate via composition (NOT cloned)
- [ ] Has 2-4 action ID references (LocationActionIds/ConversationOptionIds/TravelCardIds)
- [ ] Lives in `List<Situation>` in GameWorld (persists)
- [ ] References existing action entities by ID (NOT embedded)
- [ ] NO template properties duplicated in instance entity

---

## Reference Implementation

See existing codebase examples:
- **PATTERN documentation:** `obstacle-templates.md` (pure conceptual structures)
- **Strong typing:** `GameWorld.Locations` (List, NOT Dictionary)
- **Entity lookups:** LINQ queries throughout facade layer
- **Composition pattern:** Situation instances reference SituationTemplate

This architecture ensures:
✅ Clear separation of concerns (pattern, template, instance)
✅ Clear separation of layers (STRATEGIC Scene/Situation vs TACTICAL SituationCard)
✅ Ephemeral Scene spawners vs persistent Situation contexts
✅ Situations reference existing action entities (LocationAction/ConversationOption/TravelCard)
✅ Sir Brante pattern (Situation = narrative + 2-4 action references)
✅ Strong typing enforcement (Lists, NO Dictionaries/HashSets)
✅ Memory efficiency (composition, NOT cloning)
✅ Type safety (compiler-enforced access patterns)
✅ Reusability (templates spawn many instances)
