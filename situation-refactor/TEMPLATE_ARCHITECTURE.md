# Template Architecture Principle: Three-Level Design

## CRITICAL ARCHITECTURAL FOUNDATION

This document defines the **THREE-LEVEL ARCHITECTURE** for Scene and Situation Spawn Templates.

**LAYER SEPARATION:**
- **STRATEGIC LAYER**: Scene → Situation → Actions (LocationAction/ConversationOption/TravelCard)
- **TACTICAL LAYER**: SituationCard (victory conditions INSIDE challenges - separate system)

**KEY ENTITIES:**
- **Scene** = Provisional → Active lifecycle container with multiple embedded Situations (perfect information display, then playable content)
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
  - **Scene**: Provisional → Active lifecycle (perfect information display, then playable content)
  - **Situation**: Persistent narrative context (lives in `GameWorld.Situations`)

#### Scene Instance (Provisional → Active Lifecycle)

**Lives in:**
- **Provisional Phase:** `GameWorld.ProvisionalScenes` (Dictionary<string, Scene>)
- **Active Phase:** `GameWorld.Scenes` (List<Scene>)

**Purpose:** Scene instances provide perfect information BEFORE selection, then become playable content AFTER selection.

**Lifecycle:**

**1. PROVISIONAL PHASE (Created at Situation instantiation):**
- Created when Situation instantiated (for each Choice with SceneSpawnReward)
- `SceneInstantiator.CreateProvisionalScene(templateId, placementRelation, context)`
- Selects concrete placement via PlacementFilter (e.g., "Adjacent NPC")
- Creates embedded Situations from SituationTemplates
- Placeholders NOT replaced (still `"{NPCName}"`, `"{LocationName}"`)
- State = `SceneState.Provisional`
- Stored in `GameWorld.ProvisionalScenes[Scene.Id]`
- Choice.ProvisionalSceneId references this Scene

**2. DISPLAY PHASE (Player sees WHERE Scene spawns):**
- UI reads provisional Scene from GameWorld.ProvisionalScenes
- Player sees: "Creates investigation at The Mill with Martha"
- Perfect information: knows placement, distance, NPC relationships BEFORE selecting Choice
- Choice card displays Scene archetype, location name, distance calculation

**3. FINALIZATION OR DELETION:**
- **If Choice selected:** `SceneFinalizer.FinalizeScene(sceneId)`
  - Replace placeholders: `"{NPCName}"` → `"Martha"`
  - Generate intro narrative: `"As you approach The Mill, Martha steps forward with worry in her eyes..."`
  - Move from ProvisionalScenes to Scenes
  - State = `SceneState.Active`
  - Situations become playable
- **If different Choice selected:** Delete from ProvisionalScenes (never finalized)

**Composition Architecture (Provisional Phase):**
```csharp
public class Scene
{
    // Template reference
    public SceneTemplate Template { get; set; }

    // Concrete placement (selected at creation)
    public PlacementType PlacementType { get; set; }  // Location, NPC, Route
    public string PlacementId { get; set; }  // Concrete entity ID

    // Embedded content (placeholders NOT replaced yet)
    public List<Situation> Situations { get; set; }  // Mechanical skeleton only

    // Provisional state
    public SceneState State { get; set; }  // = SceneState.Provisional

    // NO narrative yet (placeholder only)
    public string IntroNarrative { get; set; }  // = "{NPCName} approaches you..."
}
```

**Composition Architecture (Active Phase):**
```csharp
public class Scene
{
    // Same structure, but:

    // Narrative finalized
    public string IntroNarrative { get; set; }  // = "Martha approaches you with urgency..."

    // Situations have placeholders replaced
    public List<Situation> Situations { get; set; }  // "{NPCName}" → "Martha"

    // Active state
    public SceneState State { get; set; }  // = SceneState.Active

    // Activation metadata
    public int SpawnedDay { get; set; }
}
```

**Why Provisional Scenes Exist (Perfect Information Principle):**

**1. Strategic Decision-Making:**
- Player sees WHERE Scene spawns before selecting Choice
- Can evaluate distance, placement viability, NPC relationships
- Makes informed strategic decision (not blind gamble)

**2. UI Richness:**
- Choice cards show Scene archetype, location name, distance
- "Creates investigation at The Mill (2 hours away)"
- "Spawns rescue plea with Martha at Current Location"

**3. Cost Calculation:**
- Scene distance can affect Choice cost
- Placement validation (is location locked? is NPC available?)
- Dynamic costs based on provisional Scene properties

**4. Memory Efficiency:**
- Provisional Scenes are cheap: no narrative generation, just mechanical skeleton
- Unselected Choices: provisional Scenes deleted immediately
- Only selected Choice: Scene finalized and persists

**5. Fail-Fast Validation:**
- Placement issues detected at provisional creation (not mid-gameplay)
- Invalid placements throw errors immediately
- Player never sees impossible Choices

**Example Flow:**
```csharp
// PROVISIONAL PHASE (at Situation instantiation)
Scene provisionalScene = SceneInstantiator.CreateProvisionalScene(
    templateId: "rescue_quest_scene_template",
    placementRelation: new PlacementRelation { Type = PlacementType.AdjacentLocation },
    context: currentSituation
);

// Placement selected: The Mill (adjacent to current location)
provisionalScene.PlacementId = "the_mill";

// Situations created with placeholders
provisionalScene.Situations[0].IntroNarrative = "{NPCName} pleads for help...";

// Stored for display
gameWorld.ProvisionalScenes[provisionalScene.Id] = provisionalScene;
choice.ProvisionalSceneId = provisionalScene.Id;

// DISPLAY PHASE (player sees Choice card)
// UI reads: "Creates investigation at The Mill (2 hours away)"

// FINALIZATION PHASE (player selects Choice)
SceneFinalizer.FinalizeScene(provisionalScene.Id);

// Placeholders replaced
provisionalScene.Situations[0].IntroNarrative = "Martha pleads for help...";

// Intro narrative generated
provisionalScene.IntroNarrative = "As you approach The Mill, Martha steps forward...";

// Moved to active Scenes
gameWorld.ProvisionalScenes.Remove(provisionalScene.Id);
gameWorld.Scenes.Add(provisionalScene);
provisionalScene.State = SceneState.Active;
```

#### Situation Instance (Persistent Narrative Context)

**Lives in:** `GameWorld.Situations` (List<Situation>)

**Lifecycle:**
1. Created by Scene from SituationTemplate (during provisional OR active phase)
2. Placed at specific Location/NPC/Route
3. Persists in GameWorld after Scene finalized
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
    public string ParentSceneId { get; set; }  // Scene that spawned this
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

## The Complete Flow (Provisional → Active → Play)

### 1. Provisional Scene Creation (Perfect Information Display)
```csharp
// Triggered when Situation instantiated with Choice containing SceneSpawnReward
Choice choice = situation.Choices[0];
SceneSpawnReward sceneReward = choice.Rewards.OfType<SceneSpawnReward>().FirstOrDefault();

// Select SceneTemplate
SceneTemplate sceneTemplate = gameWorld.SceneTemplates
    .FirstOrDefault(t => t.Id == sceneReward.SceneTemplateId);

// Create provisional Scene
Scene provisionalScene = SceneInstantiator.CreateProvisionalScene(
    sceneTemplate,
    sceneReward.PlacementRelation,
    situation
);

// Provisional Scene state
provisionalScene.State = SceneState.Provisional;
provisionalScene.Id = GenerateUniqueId();

// Select concrete placement using filters
Location targetLocation = PlacementFilter.SelectLocation(
    gameWorld,
    sceneReward.PlacementRelation,
    situation.PlacementLocation
);

provisionalScene.PlacementType = PlacementType.Location;
provisionalScene.PlacementId = targetLocation.Id;

// Create embedded Situations with placeholders
foreach (var spawn in sceneTemplate.SituationSpawns)
{
    SituationTemplate sitTemplate = gameWorld.SituationTemplates
        .FirstOrDefault(t => t.Id == spawn.SituationTemplateId);

    Situation embeddedSituation = new Situation
    {
        Template = sitTemplate,
        Id = GenerateUniqueId(),
        PlacementLocation = targetLocation,
        GeneratedNarrative = "{NPCName} pleads for your help...",  // Placeholder NOT replaced
        Status = SituationStatus.Provisional
    };

    provisionalScene.Situations.Add(embeddedSituation);
}

// Store provisional Scene
gameWorld.ProvisionalScenes[provisionalScene.Id] = provisionalScene;
choice.ProvisionalSceneId = provisionalScene.Id;
```

### 2. Player Sees Provisional Scene (Choice Card Display)
```csharp
// UI reads provisional Scene for display
Choice choice = situation.AvailableChoices[0];
Scene provisionalScene = gameWorld.ProvisionalScenes[choice.ProvisionalSceneId];

// Display on Choice card:
// "Creates investigation at The Mill (2 hours away)"
// "Spawns 3 Situations: Plea, Investigation, Rescue"
string locationName = gameWorld.Locations
    .FirstOrDefault(l => l.Id == provisionalScene.PlacementId).Name;

int distance = CalculateDistance(playerLocation, provisionalScene.PlacementId);

// Player makes informed decision with perfect information
```

### 3. Scene Finalization (Player Selects Choice)
```csharp
// Player selects Choice
Choice selectedChoice = situation.Choices[0];
Scene provisionalScene = gameWorld.ProvisionalScenes[selectedChoice.ProvisionalSceneId];

// Finalize Scene
SceneFinalizer.FinalizeScene(provisionalScene.Id, gameWorld);

// Replace placeholders in Situations
foreach (Situation situation in provisionalScene.Situations)
{
    NPC contextNpc = gameWorld.NPCs
        .FirstOrDefault(n => n.Location == provisionalScene.PlacementId);

    situation.GeneratedNarrative = situation.GeneratedNarrative
        .Replace("{NPCName}", contextNpc.Name);

    situation.PlacementNpc = contextNpc;
    situation.Status = SituationStatus.Available;
    situation.SpawnedDay = gameWorld.CurrentDay;
}

// Generate intro narrative
provisionalScene.IntroNarrative = NarrativeGenerator.GenerateSceneIntro(
    provisionalScene,
    gameWorld
);

// Move from provisional to active
gameWorld.ProvisionalScenes.Remove(provisionalScene.Id);
gameWorld.Scenes.Add(provisionalScene);
provisionalScene.State = SceneState.Active;
provisionalScene.SpawnedDay = gameWorld.CurrentDay;

// Add Situations to GameWorld
foreach (Situation situation in provisionalScene.Situations)
{
    gameWorld.Situations.Add(situation);
}

// Delete unselected provisional Scenes
foreach (Choice otherChoice in situation.Choices.Where(c => c != selectedChoice))
{
    if (otherChoice.ProvisionalSceneId != null)
    {
        gameWorld.ProvisionalScenes.Remove(otherChoice.ProvisionalSceneId);
    }
}
```

### 4. Player Interacts with Active Situation Actions
```csharp
// Player navigates to Scene location
// Active Scene displays intro narrative
Scene activeScene = gameWorld.Scenes
    .FirstOrDefault(s => s.PlacementId == playerLocation.Id
                      && s.State == SceneState.Active);

Console.WriteLine(activeScene.IntroNarrative);
// "As you approach The Mill, Martha steps forward with urgency in her eyes..."

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
| **INSTANCE** | Scene (Provisional) | `Dictionary<string, Scene>` (ProvisionalScenes) | Mechanical skeleton with placeholders | Perfect information display |
| **INSTANCE** | Scene (Active) | `List<Scene>` (Scenes) | Finalized narrative + embedded Situations | Playable content container |
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

**Scene Instance (Provisional → Active Lifecycle):**
- [ ] References SceneTemplate via composition (NOT cloned)
- [ ] Provisional phase: Stored in GameWorld.ProvisionalScenes (Dictionary for O(1) lookup)
- [ ] Selects concrete placement using PlacementFilter
- [ ] Creates embedded Situations with placeholders (NOT replaced yet)
- [ ] State = SceneState.Provisional
- [ ] Choice.ProvisionalSceneId references provisional Scene
- [ ] Active phase: Finalized when Choice selected
- [ ] Placeholders replaced, intro narrative generated
- [ ] Moved from ProvisionalScenes to Scenes (List)
- [ ] State = SceneState.Active
- [ ] Unselected provisional Scenes deleted immediately

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
✅ Provisional → Active Scene lifecycle (perfect information before selection)
✅ Perfect information principle (player sees placement before deciding)
✅ Memory efficiency (provisional Scenes cheap, unselected deleted)
✅ Situations reference existing action entities (LocationAction/ConversationOption/TravelCard)
✅ Sir Brante pattern (Situation = narrative + 2-4 action references)
✅ Strong typing enforcement (Lists for collections, Dictionary only for O(1) provisional lookup)
✅ Composition over cloning (template reference, NOT duplication)
✅ Type safety (compiler-enforced access patterns)
✅ Reusability (templates spawn many instances)
