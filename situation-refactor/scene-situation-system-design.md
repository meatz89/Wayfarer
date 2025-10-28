# Scene-Situation System Design

## Core Philosophy

**Scenes orchestrate dynamic narrative emergence through spatial situation spawning.**

The Scene-Situation system generates story beats by spawning narrative contexts (Situations) at locations, NPCs, and routes. Unlike obstacles (persistent challenges), Scenes are ephemeral orchestrators that spawn multiple Situations in various configurations (sequential, parallel, branching), then discard themselves.

**Key Principles:**

1. **Scene as Spawning Orchestrator**: Scene templates define patterns for spawning multiple Situations across the game world
2. **Situation as Narrative Context**: Each Situation is a narrative moment containing 2-4 player action choices
3. **Actions as Player Choices**: Existing entities (LocationAction, ConversationOption, TravelCard) are the actual choices
4. **Spatial Emergence**: Player navigates to locations/NPCs and discovers Situations that have spawned there
5. **Ephemeral Orchestration**: Scenes spawn Situations and discard themselves; Situations persist until completed

**NOT Goals/Obstacles:**
- Obstacles = Persistent barriers requiring multiple Goal completions
- Goals = Tactical challenges with victory conditions (SituationCards)
- Scenes/Situations = Strategic narrative contexts offering player choices

---

## Strategic Layer Hierarchy

**THREE LEVELS OF ABSTRACTION:**

### Level 1: Scene (Ephemeral Orchestrator)
- Spawns from SceneTemplate
- Creates multiple Situations at various placements
- Defines configuration (sequential, parallel, branching, conditional)
- Discards itself after spawning Situations
- NOT stored in GameWorld (ephemeral)

### Level 2: Situation (Persistent Narrative Context)
- Spawns from SituationTemplate within a Scene
- Contains 2-4 action references (by ID)
- Appears at one Location/NPC/Route
- Persists until player completes one of its actions
- Stored in GameWorld.Situations

### Level 3: Actions (Player Choices)
- Existing entities: LocationAction, ConversationOption, TravelCard
- NOT created by Scenes (already exist in GameWorld)
- Situations reference them by ID
- Player selects ONE action from Situation's 2-4 options

---

## Core Entities

### Scene (Ephemeral)

**Purpose**: Orchestrate spawning of multiple related Situations across the game world

**Properties:**
- `TemplateId` (string): References SceneTemplate that spawned this Scene
- `SpawnedSituationIds` (List&lt;string&gt;): Track which Situations this Scene created
- `CompletionCondition` (enum): What triggers Scene conclusion (AllComplete, AnyComplete, TimeExpired, PlayerChoice)
- `State` (enum): Active, Waiting, Completed

**Lifecycle:**
1. GameWorld spawns Scene from SceneTemplate (based on spawn rules)
2. Scene immediately spawns Situations at various placements
3. Scene tracks Situation completion
4. Scene triggers follow-up spawns (new Scenes/Situations) based on outcomes
5. Scene discards itself when completion condition met

**NOT Stored**: Scenes are transient orchestrators, not persisted entities

---

### Situation (Persistent)

**Purpose**: Narrative context at a location/NPC/route offering 2-4 player action choices

**Properties:**
- `Id` (string): Unique identifier
- `TemplateId` (string): References SituationTemplate
- `SceneId` (string): References parent Scene that spawned this
- `NarrativeText` (string): Contextual description player sees
- `PlacementType` (enum): Location, NPC, Route
- `PlacementId` (string): ID of Location/NPC/Route where this appears
- `ActionIds` (List&lt;string&gt;): 2-4 references to LocationAction/ConversationOption/TravelCard
- `RequirementFormula` (string): Categorical formula for spawn requirements (parsed at instantiation)
- `TimeLimit` (int?): Optional segments before Situation expires
- `State` (enum): Available, InProgress, Completed, Expired, Failed
- `Priority` (int): Display ordering when multiple Situations at same placement

**Lifecycle:**
1. Scene spawns Situation from SituationTemplate
2. Situation appears at specified placement (location/NPC/route)
3. Player navigates to placement and sees Situation narrative
4. Player selects ONE of 2-4 actions
5. Action executes (may start challenge, apply cost/reward, navigate)
6. Situation marks itself Completed
7. Situation may trigger follow-up spawns (defined by parent Scene)

**Stored**: `GameWorld.Situations` (persistent until completed)

---

### Actions (Existing Entities - NOT Created by Scenes)

**Three Action Types:**

#### LocationAction
- Appears at Locations
- Can be instant (cost/reward) or start challenge (Social/Mental/Physical)
- Examples: "Search the warehouse", "Rest at inn", "Investigate rumor"

#### ConversationOption
- Appears at NPCs (within conversations)
- Social challenge entry points
- Examples: "Persuade the guard", "Intimidate the merchant", "Console the widow"

#### TravelCard
- Appears at Routes
- Navigation between locations
- Examples: "Travel to Capital", "Take shortcut through woods", "Hire carriage"

**Situations Reference Actions by ID:**
- Situation does NOT create actions
- Situation contains `ActionIds` pointing to existing actions
- Actions may be shared across multiple Situations
- Actions lifecycle independent of Situations

---

## Template System

### SceneTemplate

**Purpose**: Define pattern for spawning multiple Situations in configuration

**Structure:**
```
SceneTemplate:
  - id: Unique template identifier
  - archetype: Categorical type (Investigation, Rescue, Discovery, Social Web, etc.)
  - tier: Difficulty/progression tier (0-3)
  - spawnRules: Requirements for Scene to spawn
  - situationSpawns: List of Situations to create with placement logic
  - configuration: Sequential, Parallel, Branching, Conditional
  - completionCondition: What triggers Scene conclusion
  - followUpSpawns: What spawns when Scene completes (success/failure paths)
```

**Example Archetypes:**
- **Linear Progression**: Spawn A → After A completes, spawn B → After B completes, spawn C
- **Hub and Spoke**: Spawn central Situation + 3 parallel child Situations
- **Branching Consequences**: Success path spawns different Situations than failure path
- **Discovery Chain**: Complete Situation A reveals new location, spawns Situation B there
- **Timed Cascade**: Spawn Situation with deadline; completion before/after spawns different follow-ups

---

### SituationTemplate

**Purpose**: Define narrative context with action references and placement logic

**Structure:**
```
SituationTemplate:
  - id: Unique template identifier
  - archetype: Categorical type (Encounter, Discovery, Decision, Obstacle, etc.)
  - narrativeHints: Guidance for narrative text generation
  - placementType: Location, NPC, Route
  - placementFilters: Categorical requirements (e.g., "Urban location with Bazaar tag")
  - actionCount: How many actions (2-4)
  - actionFilters: Requirements for which actions to reference
  - requirementFormula: Categorical spawn requirements
  - timeLimit: Optional expiration (segments)
  - priority: Display ordering
```

**Key Concept: Templates Define Patterns, Runtime Instantiates with Concrete Entities**

Templates use **categorical filters** to describe WHAT KIND of entities to use:
- "Urban location with high traffic" → Parser selects actual Location from GameWorld
- "NPC with Cunning personality" → Parser selects actual NPC from GameWorld
- "Challenge-starting action costing Resolve" → Parser selects actual LocationAction from GameWorld

This enables:
- **AI Content Generation**: AI describes patterns categorically, doesn't need to know exact entity IDs
- **Dynamic Scaling**: Requirements scale based on player level, game state
- **Reusability**: One template instantiates differently based on what entities currently exist

---

## Storage Architecture

### GameWorld.Situations (Persistent)
```csharp
public List<Situation> Situations { get; set; } = new List<Situation>();
```

**Contains**: All active Situations spawned by Scenes
**Lifecycle**: Added when Scene spawns, removed when completed/expired
**Access Pattern**:
- By placement: `Situations.Where(s => s.PlacementType == PlacementType.Location && s.PlacementId == locationId)`
- By state: `Situations.Where(s => s.State == SituationState.Available)`
- By priority: `Situations.OrderByDescending(s => s.Priority)`

### SceneTemplates (Content)
```csharp
// Loaded from JSON packages at initialization
public Dictionary<string, SceneTemplate> SceneTemplates { get; set; }
```

**Source**: JSON files in content packages (e.g., `18_scene_templates.json`)
**Loaded By**: `SceneTemplateParser` during `PackageLoader.LoadPackage()`
**Immutable**: Templates never change at runtime

### SituationTemplates (Content)
```csharp
// Loaded from JSON packages at initialization
public Dictionary<string, SituationTemplate> SituationTemplates { get; set; }
```

**Source**: JSON files in content packages (e.g., `19_situation_templates.json`)
**Loaded By**: `SituationTemplateParser` during `PackageLoader.LoadPackage()`
**Immutable**: Templates never change at runtime

---

## Spawn Orchestration Patterns

### Pattern Categories

**1. Linear Progression**
- Scene spawns Situation A
- After A completes, Scene spawns Situation B
- After B completes, Scene spawns Situation C
- Scene concludes when chain reaches end

**2. Hub and Spoke**
- Scene spawns central Situation + 3 parallel Situations
- All available simultaneously
- Scene concludes when all completed (or central completed, depending on rules)

**3. Branching Consequences**
- Scene spawns initial Situation
- Success spawns Situations D, E, F
- Failure spawns Situations G, H, I
- Different narrative paths based on outcome

**4. Discovery Chain**
- Scene spawns Situation at known Location A
- Completing reveals Location B (previously hidden)
- Scene spawns follow-up Situation at Location B
- Progression through spatial discovery

**5. Conditional Multi-Spawn**
- Scene evaluates player state (stats, items, relationships)
- Spawns different Situation combinations based on conditions
- Creates tailored narrative experiences

**6. Timed Cascade**
- Scene spawns Situation with time limit
- Completing before deadline: spawns one set of Situations
- Completing after deadline: spawns degraded set
- Missing entirely: spawns consequence Situations

**7. Converging Paths**
- Scene spawns multiple independent Situations
- When ALL completed, Scene spawns finale Situation
- Requires player to pursue all threads

**8. Mutually Exclusive Paths**
- Scene spawns two Situations simultaneously
- Completing one removes/blocks the other
- Forces permanent choice

**See**: `situation-spawn-patterns.md` for detailed pattern documentation

---

## Runtime Flow

### 1. Scene Spawning

**Trigger Points:**
- Game initialization (starter Scenes)
- Time progression (scheduled spawns)
- Situation completion (follow-up spawns)
- Player state changes (threshold spawns)
- Location discovery (exploration spawns)

**Process:**
```
1. GameWorld evaluates spawn rules for all SceneTemplates
2. Eligible templates identified (requirements met)
3. GameWorld selects template(s) based on priority/randomization
4. SceneFacade.SpawnScene(templateId):
   - Instantiate Scene from template
   - Evaluate situation spawn configuration
   - For each Situation in configuration:
     - Select concrete placement (Location/NPC/Route) matching filters
     - Select concrete actions (2-4) matching filters
     - Instantiate Situation with placement + action references
     - Add Situation to GameWorld.Situations
   - Track spawned Situation IDs in Scene
   - Register Scene completion monitor (evaluates condition)
```

---

### 2. Situation Appearance

**Player Experience:**
```
Player navigates to Location
→ UI queries GameWorld.Situations for this Location
→ Finds Situation(s) with PlacementType=Location, PlacementId=currentLocationId
→ Displays Situation narrative text
→ Shows 2-4 actions as player choices (cards)
```

**Important**: Actions themselves are NOT Situation-specific. They're existing entities that Situation references. Same action might appear in multiple Situations.

---

### 3. Player Action Selection

**When player selects action from Situation:**

**Instant Action (cost/reward only):**
```
1. GameFacade.ExecuteLocationAction(actionId)
2. Validate requirements (stats, items, etc.)
3. Apply costs (resources, time)
4. Apply rewards (stats, items, discoveries)
5. Mark Situation as Completed
6. Remove Situation from GameWorld.Situations
7. Trigger parent Scene completion check
```

**Challenge Action (starts Social/Mental/Physical):**
```
1. GameFacade.StartChallenge(actionId, challengeType)
2. Load challenge configuration (deck, goal cards, etc.)
3. Enter challenge subsystem (tactical layer)
4. Player plays challenge
5. On challenge completion:
   - Apply challenge rewards
   - Mark Situation as Completed
   - Remove Situation from GameWorld.Situations
   - Trigger parent Scene completion check
```

**Navigation Action (TravelCard):**
```
1. GameFacade.ExecuteTravelCard(cardId)
2. Validate route exists
3. Apply travel costs (time, resources)
4. Move player to destination Location
5. Mark Situation as Completed (if travel was the Situation action)
6. Trigger parent Scene completion check
```

---

### 4. Scene Completion

**Completion Conditions:**
- **AllComplete**: All spawned Situations completed
- **AnyComplete**: Any one Situation completed
- **TimeExpired**: Segment/day limit reached
- **PlayerChoice**: Specific Situation marked as "Scene conclusion"

**On Scene Completion:**
```
1. SceneFacade evaluates completion condition
2. Determines outcome (success/failure/neutral based on which Situations completed)
3. Evaluates followUpSpawns rules:
   - Success path → Spawn new Scene(s) / Situation(s)
   - Failure path → Spawn different Scene(s) / Situation(s)
4. Spawn follow-ups (recursive Scene spawning)
5. Discard Scene (ephemeral, not stored)
```

---

### 5. Situation Expiration

**Time Limit:**
- Situation has optional `TimeLimit` (segments)
- TimeFacade tracks active Situations with limits
- On limit reached: Mark Situation as Expired
- May trigger consequence spawns (Scene defines expired outcome)

**Blocking:**
- Situation A may block Situation B (mutually exclusive)
- Completing A removes B from GameWorld.Situations
- Player sees B disappear from UI

**Prerequisite Failure:**
- Situation requirements may become impossible (NPC dies, location destroyed)
- Mark Situation as Failed
- Remove from GameWorld.Situations
- May trigger alternative spawns

---

## Sir Brante Pattern in Wayfarer Context

**Sir Brante Structure:**
- Player sees Situation narrative
- 2-4 choices presented
- Each choice has:
  - Requirements (visible)
  - Costs (visible)
  - Outcomes (hidden until selected)
- Some choices instant, some start challenges
- Choice locks in outcome, progresses story

**Wayfarer Implementation:**
- **Situation** = Sir Brante narrative moment
- **Actions** = Sir Brante choices (2-4 options)
- Player navigates to Location/NPC/Route
- Sees Situation narrative
- Selects one of 2-4 Actions
- Action executes (instant or challenge)
- Situation completes, story progresses

**Key Difference:**
- Sir Brante: Linear progression through scripted situations
- Wayfarer: Spatial navigation, dynamic situation spawning, emergent narrative

---

## Relationship to Other Systems

### Obstacles/Goals
- **Completely Separate**: Obstacle system tracks persistent barriers with tactical challenges
- **No Overlap**: Situations do NOT create Obstacles
- **Potential Connection**: Action reward could spawn Obstacle (but Situation itself is not an Obstacle)

### Conversations
- **ConversationOption Actions**: Situations can reference conversation entry points
- **NPC Placement**: Situations at NPCs offer conversation-starting actions
- **Social Challenges**: ConversationOption actions start Social challenge subsystem

### Locations/Routes
- **Placement Context**: Locations/Routes host Situations (not owners)
- **Discovery**: Situation rewards can reveal new Locations/Routes
- **Navigation**: TravelCard actions enable movement

### Time
- **Segment Costs**: Actions cost time (advance clock)
- **Time Limits**: Situations expire after N segments
- **Scheduled Spawns**: Scenes spawn at specific times

### Resources
- **Action Costs**: Actions require/consume resources (coins, health, focus, stamina, resolve)
- **Action Rewards**: Actions grant resources
- **Gating**: Situation requirements include resource thresholds

---

## Template-to-Instance Flow

**Three-Stage Process:**

### Stage 1: Content Authoring (JSON)
```json
{
  "sceneTemplates": [
    {
      "id": "investigation_warehouse",
      "archetype": "Discovery Chain",
      "tier": 1,
      "spawnRules": {
        "playerInLocation": "merchants_district",
        "hasState": "heard_rumor_of_smuggling"
      },
      "situationSpawns": [
        {
          "templateId": "warehouse_exterior_situation",
          "placementType": "Location",
          "placementFilter": { "locationTag": "Warehouse" }
        }
      ]
    }
  ]
}
```

**Categorical Properties:**
- `archetype`: "Discovery Chain" (not hardcoded logic)
- `placementFilter`: Describes WHAT KIND of location (not specific ID)
- `spawnRules`: Categorical requirements (not concrete values)

---

### Stage 2: Parsing (Translation)

**SceneTemplateParser:**
```csharp
public SceneTemplate ParseSceneTemplate(SceneTemplateDTO dto, GameWorld gameWorld)
{
    return new SceneTemplate
    {
        Id = dto.Id,
        Archetype = ParseEnum<SceneArchetype>(dto.Archetype),
        Tier = dto.Tier,
        SpawnRules = SpawnRuleCatalog.TranslateRules(dto.SpawnRules, gameWorld),
        SituationSpawns = dto.SituationSpawns.Select(s => ParseSituationSpawn(s)).ToList(),
        Configuration = ParseEnum<SpawnConfiguration>(dto.Configuration),
        CompletionCondition = ParseEnum<CompletionCondition>(dto.CompletionCondition),
        FollowUpSpawns = ParseFollowUpSpawns(dto.FollowUpSpawns)
    };
}
```

**Key Translation:**
- Categorical properties → Enums
- Spawn rules → Strongly-typed requirement objects
- Filters → LINQ query predicates (evaluated at instantiation)

---

### Stage 3: Runtime Instantiation

**SceneFacade.SpawnScene:**
```csharp
public void SpawnScene(string templateId)
{
    var template = _gameWorld.SceneTemplates[templateId];

    // Evaluate spawn rules (already translated at parse time)
    if (!EvaluateSpawnRules(template.SpawnRules)) return;

    // Create ephemeral Scene
    var scene = new Scene
    {
        TemplateId = templateId,
        SpawnedSituationIds = new List<string>()
    };

    // Spawn each Situation in configuration
    foreach (var situationSpawn in template.SituationSpawns)
    {
        var situationTemplate = _gameWorld.SituationTemplates[situationSpawn.TemplateId];

        // Select concrete placement using filters (LINQ query on GameWorld)
        var placement = SelectPlacement(situationSpawn.PlacementType, situationSpawn.PlacementFilter);
        if (placement == null) continue; // No valid placement

        // Select 2-4 concrete actions using filters (LINQ query on GameWorld)
        var actions = SelectActions(situationTemplate.ActionFilters, situationTemplate.ActionCount);
        if (actions.Count < 2) continue; // Not enough valid actions

        // Instantiate Situation
        var situation = new Situation
        {
            Id = GenerateId(),
            TemplateId = situationSpawn.TemplateId,
            SceneId = scene.TemplateId, // Track parent
            NarrativeText = GenerateNarrative(situationTemplate.NarrativeHints, placement, actions),
            PlacementType = situationSpawn.PlacementType,
            PlacementId = placement.Id,
            ActionIds = actions.Select(a => a.Id).ToList(),
            State = SituationState.Available
        };

        // Add to GameWorld
        _gameWorld.Situations.Add(situation);
        scene.SpawnedSituationIds.Add(situation.Id);
    }

    // Register Scene completion monitor (ephemeral, evaluates condition)
    RegisterSceneCompletionMonitor(scene, template);
}
```

**Key Points:**
- Template filters → LINQ queries selecting actual entities from GameWorld
- Categorical requirements → Already translated to strong types at parse time
- Scene ephemeral (not stored), Situations persistent (stored in GameWorld.Situations)

---

## Validation Checklist

**SceneTemplate Validation:**
- ✅ Has at least one SituationSpawn
- ✅ CompletionCondition defined
- ✅ FollowUpSpawns defined (or explicitly none)
- ✅ SpawnRules reference valid player states, locations, NPCs
- ✅ Tier appropriate for archetype

**SituationTemplate Validation:**
- ✅ ActionCount between 2-4
- ✅ PlacementType defined (Location, NPC, or Route)
- ✅ PlacementFilters valid for PlacementType
- ✅ ActionFilters reference valid action properties
- ✅ NarrativeHints provide sufficient guidance
- ✅ RequirementFormula valid categorical properties

**Runtime Situation Validation:**
- ✅ Placement exists in GameWorld
- ✅ 2-4 valid actions selected
- ✅ Actions appropriate for PlacementType (LocationAction for Location, etc.)
- ✅ No duplicate ActionIds in same Situation
- ✅ SceneId references valid parent Scene

---

## Design Constraints

### Sir Brante Pattern Requirements
1. **2-4 Choices**: Every Situation must offer 2-4 actions (no more, no less)
2. **Narrative Context**: Situation provides story/context, actions are responses
3. **Mixed Types**: Actions can be instant (cost/reward) or challenge-starting
4. **Visible Requirements**: All action requirements shown to player before selection
5. **Hidden Outcomes**: Exact rewards/consequences hidden until action selected
6. **Progression**: Selecting action completes Situation, advances story

### Spatial Navigation
1. **Placement Persistence**: Situation persists at location until completed
2. **Player Discovery**: Player navigates to placement to discover Situation
3. **Multiple Situations**: One placement can host multiple Situations simultaneously
4. **Priority Display**: Higher priority Situations shown more prominently

### Strategic Layer Separation
1. **No Tactical Mechanics**: Situations do NOT contain SituationCards (those are challenge victory conditions)
2. **Action References Only**: Situations reference existing actions by ID, don't create them
3. **Scene Orchestration**: Scenes spawn Situations, Situations don't spawn Situations directly
4. **Ephemeral Scenes**: Scenes discard after spawning, not stored in GameWorld

### Categorical Design
1. **Template Filters**: Use categorical properties, not concrete IDs
2. **Parse-Time Translation**: All categorical → concrete at instantiation
3. **Dynamic Scaling**: Requirements scale based on player progression
4. **AI-Friendly**: Templates describable without knowing exact game state

---

## Future Extensions

### Procedural Narrative
- AI generates SituationTemplates at runtime based on player history
- Categorical filters ensure valid entity selection
- Narrative hints guide procedural text generation

### Save/Load
- GameWorld.Situations serialized (active Situations)
- Scene state NOT serialized (ephemeral)
- On load, re-evaluate Scene completion monitors

### Scene Stacks
- Parent Scenes spawn child Scenes
- Child Scene completion can trigger parent Scene progression
- Nested narrative structures (mysteries within mysteries)

### Situation Variants
- Same SituationTemplate instantiates with different action sets based on player state
- Replayability through varied action combinations
- Contextual adaptation (NPC relationship affects available actions)

---

## Summary

**Scenes** are ephemeral orchestrators that spawn **Situations** in various configurations. **Situations** are persistent narrative contexts appearing at locations/NPCs/routes, offering players 2-4 **action** choices. Actions (LocationAction/ConversationOption/TravelCard) are existing entities that Situations reference, NOT inline definitions.

This architecture enables:
- **Emergent Narrative**: Dynamic story beats generated from templates
- **Spatial Discovery**: Player navigates world to find spawned Situations
- **Strategic Choice**: Player selects among visible action options with clear costs/requirements
- **Reusability**: Templates instantiate with different entities, creating variety
- **AI Generation**: Categorical templates enable procedural content creation

The Scene-Situation system is the **STRATEGIC LAYER** of Wayfarer's narrative engine. It sits above the tactical challenge layer (Social/Mental/Physical with SituationCards) and provides the framework for dynamic quest/story generation across the game world.
