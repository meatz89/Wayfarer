# SCENE/CARD DATA FLOW ARCHITECTURE ANALYSIS

**Document Purpose**: Complete architectural analysis of the transformation from Goal/Obstacle filtering to Scene/Card spawning paradigm

**Date**: 2025-10-23
**Status**: Architectural Analysis (No Code)

---

## EXECUTIVE SUMMARY

**CURRENT PARADIGM**: Static library of goals → Filter at runtime by availability checks
**NEW PARADIGM**: Dynamic scene spawning → Actions spawn scenes → No filtering required

**CRITICAL INSIGHT**: Current system confuses **template** and **instance**. Goals are both definition AND runtime state. Scene/Card architecture separates these completely.

---

## PART 1: CURRENT DATA FLOW (JSON → Parser → Domain → UI)

### 1.1 COMPLETE FLOW MAPPING

```
01_foundation.json (Venues, Locations, Items)
  ↓ Read by
PackageLoader.LoadStaticPackages() [Line 55-89]
  ↓ Deserializes to
VenueDTO, LocationDTO, ItemDTO
  ↓ Converts via
VenueParser, LocationParser, ItemParser
  ↓ Creates
Venue, Location, Item entities
  ↓ Stored in
GameWorld.Venues, GameWorld.Locations, GameWorld.Items (Lists)
  ↓ Referenced at runtime (no filtering)

---

05_goals.json (Goals - strategic layer)
  ↓ Read by
PackageLoader.LoadGoals() [Line 555-589]
  ↓ Deserializes to
GoalDTO [src/Content/DTOs/GoalDTO.cs]
  ↓ Converts via
GoalParser.ConvertDTOToGoal() [src/Content/GoalParser.cs:14-82]
  ↓ Creates
Goal entity [domain model]
  ↓ Stored in TWO places (CRITICAL DUPLICATION)
GameWorld.Goals (centralized list) [Line 564]
NPC.ActiveGoalIds OR Location.ActiveGoalIds (placement references) [Lines 566-587]
  ↓ Queried by
ObstacleGoalFilter.GetVisibleNPCGoals() / GetVisibleLocationGoals()
  ↓ Filtered by
IsAvailable && !IsCompleted && other availability checks
  ↓ Built into
LocationFacade.GetLocationContentViewModel() [Line 489-519]
  ↓ Creates
LocationContentViewModel with GoalCardViewModel list
  ↓ Displayed by
LocationContent.razor.cs [OnInitializedAsync → RefreshLocationData]
```

### 1.2 PARSER → JSON → ENTITY TRIANGLE ANALYSIS

#### Goal Triangle

**JSON Field** (05_goals.json) | **DTO Property** (GoalDTO.cs) | **Domain Property** (Goal.cs)
---|---|---
`id` | `Id` | `Id`
`name` | `Name` | `Name`
`description` | `Description` | `Description`
`systemType` | `SystemType` (string) | `SystemType` (TacticalSystemType enum)
`placementNpcId` | `PlacementNpcId` | `PlacementNpcId`
`placementLocationId` | `PlacementLocationId` | `PlacementLocationId`
`deckId` | `DeckId` | `DeckId`
`costs` | `Costs` (GoalCostsDTO) | `Costs` (GoalCosts)
`difficultyModifiers` | `DifficultyModifiers` | `DifficultyModifiers`
`consequenceType` | `ConsequenceType` (string) | `ConsequenceType` (enum)
`setsResolutionMethod` | `ResolutionMethod` (string) | `SetsResolutionMethod` (enum)
`isAvailable` | `IsAvailable` | `IsAvailable` ← **RUNTIME STATE IN JSON!**
`isCompleted` | `IsCompleted` | `IsCompleted` ← **RUNTIME STATE IN JSON!**
`deleteOnSuccess` | `DeleteOnSuccess` | `DeleteOnSuccess`
`goalCards` | `GoalCards` (List<GoalCardDTO>) | `GoalCards` (List<GoalCard>)

**MISMATCHES**: ✅ No JsonPropertyName workarounds (all names match exactly)

**ARCHITECTURAL VIOLATION**: `isAvailable` and `isCompleted` are RUNTIME STATE fields stored in JSON. JSON should contain only TEMPLATE data. Runtime state should live in GameWorld only.

#### Obstacle Triangle

**JSON Field** (embedded in Obligations) | **DTO Property** (ObstacleDTO.cs) | **Domain Property** (Obstacle.cs)
---|---|---
`id` | `Id` | `Id`
`name` | `Name` | `Name`
`description` | `Description` | `Description`
`intensity` | `Intensity` | `Intensity`
`contexts` | `Contexts` (List<string>) | `Contexts` (List<ObstacleContext> enum)
`isPermanent` | `IsPermanent` | `IsPermanent`
`goals` | `Goals` (List<GoalDTO>) | `GoalIds` (List<string>) ← **FLATTENED AT PARSE**

**CRITICAL TRANSFORMATION**: Parser flattens nested `goals` array into `GoalIds` references. Goals added to `GameWorld.Goals`, obstacle stores IDs only.

**LOCATION**: ObstacleParser.ConvertDTOToObstacle() [Lines 57-70]

#### Obligation Triangle

**JSON Field** (13_obligations.json) | **DTO Property** (ObligationDTO.cs) | **Domain Property** (Obligation.cs)
---|---|---
`id` | `Id` | `Id`
`name` | `Name` | `Name`
`description` | `Description` | `Description`
`intro` | `Intro` (ObligationIntroActionDTO) | `IntroAction` (ObligationIntroAction)
`phases` | `Phases` (List<ObligationPhaseDTO>) | `PhaseDefinitions` (List<ObligationPhaseDefinition>)
`obligationType` | `ObligationType` (string) | `ObligationType` (enum)
`colorCode` | `ColorCode` | `ColorCode`

**MISMATCHES**: ✅ No JsonPropertyName workarounds

---

## PART 2: STARTUP VS RUNTIME ENTITIES

### 2.1 CURRENT SYSTEM (CONFUSED TEMPLATE/INSTANCE)

**LOADED AT STARTUP** (Static Templates):
- `GameWorld.Venues` → Never change
- `GameWorld.Locations` → Never change (unless skeleton completion)
- `GameWorld.NPCs` → Change only via relationship/state updates
- `GameWorld.Items` → Static definitions
- `GameWorld.SocialCards` → Static templates
- `GameWorld.MentalCards` → Static templates
- `GameWorld.PhysicalCards` → Static templates
- `GameWorld.Goals` ← **BOTH template AND instance** (VIOLATION)
- `GameWorld.Obstacles` ← **BOTH template AND instance** (VIOLATION)
- `GameWorld.Obligations` ← **BOTH template AND instance** (VIOLATION)

**RUNTIME STATE TRACKED**:
- `Goal.IsAvailable` ← **Boolean gate tracking**
- `Goal.IsCompleted` ← **Boolean gate tracking**
- `NPC.ActiveGoalIds` ← **Placement tracking**
- `Location.ActiveGoalIds` ← **Placement tracking**
- `Player.ActiveObligationIds` ← **Activation tracking**

**THE CONFUSION**:
- **Goal entity** represents BOTH "the definition of 'Help Elena'" AND "the active instance of 'Help Elena' available right now"
- **Obstacle entity** represents BOTH "the obstacle template" AND "the active barrier in the world"
- **Filtering** tries to distinguish template from instance by checking `IsAvailable`

### 2.2 NEW SYSTEM (CLEAN TEMPLATE/INSTANCE SEPARATION)

**LOADED AT STARTUP** (Templates Only):
- `GameWorld.SceneTemplates` → Definitions of ALL possible scenes
- `GameWorld.ActionDefinitions` → Definitions of ALL possible actions
- `GameWorld.SocialCards` → Unchanged (already templates)
- `GameWorld.MentalCards` → Unchanged (already templates)
- `GameWorld.PhysicalCards` → Unchanged (already templates)

**CREATED AT RUNTIME** (Instances Only):
- `Location.ActiveSceneIds` → IDs of currently active scenes at this location
- `NPC.ActiveSceneIds` → IDs of currently active scenes with this NPC
- `GameWorld.ActiveScenes` → ALL active scene instances (single source of truth)
- `GameWorld.SceneSpawnHistory` → Track which action spawned which scene

**THE CLARITY**:
- **SceneTemplate** = Pure definition ("When Elena wants help")
- **Scene** = Runtime instance ("Elena needs help RIGHT NOW at Common Room")
- **No filtering needed** → If scene exists in `Location.ActiveSceneIds`, it's available

---

## PART 3: FILTERING VS SPAWNING TRANSFORMATION

### 3.1 CURRENT APPROACH: FILTERING

**Flow**:
```
ALL goals loaded at startup
  → Stored in GameWorld.Goals
  → ALL goals assigned to NPC/Location placement
  → At runtime: GetAvailableGoals() filters
    - Check IsAvailable flag
    - Check IsCompleted flag
    - Check obligation prerequisites
    - Check difficulty modifiers
    - Check time/resource requirements
  → Display filtered subset
```

**Code Location**: `ObstacleGoalFilter.GetVisibleNPCGoals()`

**Problems**:
1. **Wasted memory**: ALL goals loaded even if never used
2. **Complex filtering**: Availability logic scattered across multiple systems
3. **Boolean gates**: Prerequisites checked continuously
4. **No spawn causality**: Can't track "which action created this goal?"
5. **Template/instance confusion**: Same entity represents both

### 3.2 NEW APPROACH: SPAWNING

**Flow**:
```
SceneTemplates loaded at startup (definitions only)
  → Stored in GameWorld.SceneTemplates
  → NO placement, NO availability flags
  → Player executes Action
    → Action.OnComplete lists SceneSpawns
    → SpawnScene(templateId, location, context)
      → Creates Scene instance from template
      → Scene.Id = unique runtime ID
      → Scene.TemplateId = reference to template
      → Scene.SpawnedBy = action that created it
      → Scene.SpawnContext = runtime data
      → Adds to Location.ActiveSceneIds
      → Adds to GameWorld.ActiveScenes
  → UI calls GetActiveScenes(locationId)
    → Returns ALL scenes in Location.ActiveSceneIds
    → NO filtering (presence = availability)
  → Display all active scenes
```

**Key Difference**:
- **OLD**: "Is this goal available?" (boolean check)
- **NEW**: "Does this scene exist?" (presence check)

---

## PART 4: NEW DATA FLOW DESIGN

### 4.1 TEMPLATE LOADING (STARTUP)

```
01_templates.json (SceneTemplates + ActionDefinitions)
  ↓ Read by
PackageLoader.LoadTemplates()
  ↓ Deserializes to
SceneTemplateDTO, ActionDefinitionDTO
  ↓ Converts via
SceneTemplateParser, ActionDefinitionParser
  ↓ Creates
SceneTemplate, ActionDefinition entities (PURE DEFINITIONS)
  ↓ Stored in
GameWorld.SceneTemplates (List<SceneTemplate>)
GameWorld.ActionDefinitions (List<ActionDefinition>)
  ↓ Never modified after load
```

**SceneTemplate Properties**:
```csharp
public class SceneTemplate
{
    public string Id { get; set; } // "elena_help_service"
    public string Name { get; set; } // "Help with Evening Service"
    public string Description { get; set; }
    public TacticalSystemType SystemType { get; set; } // Social/Mental/Physical
    public string DeckId { get; set; } // Which challenge deck
    public GoalCosts Costs { get; set; } // Resource costs
    public List<Card> Cards { get; set; } // Victory conditions
    public ConsequenceType ConsequenceType { get; set; }
    // NO IsAvailable, NO IsCompleted, NO PlacementId
}
```

**ActionDefinition Properties**:
```csharp
public class ActionDefinition
{
    public string Id { get; set; } // "investigate_common_room"
    public string Name { get; set; } // "Survey the common room"
    public string Description { get; set; }
    public List<SceneSpawnInfo> SpawnsScenes { get; set; } // What scenes this creates
    public ActionCosts Costs { get; set; }
    public string Narrative { get; set; } // Completion narrative
}

public class SceneSpawnInfo
{
    public string SceneTemplateId { get; set; }
    public SpawnTargetType TargetType { get; set; } // Location, NPC, Route
    public string TargetEntityId { get; set; }
    public Dictionary<string, object> SpawnContext { get; set; } // Runtime data
}
```

### 4.2 RUNTIME SPAWNING (DYNAMIC)

```
Player clicks "Survey the common room" action
  ↓
GameFacade.ProcessIntent(new ExecuteActionIntent("investigate_common_room"))
  ↓
ActionExecutor.Execute(actionId)
  ↓ Finds action definition
ActionDefinition action = GameWorld.ActionDefinitions.Find(a => a.Id == actionId)
  ↓ Reads spawn info
foreach (SceneSpawnInfo spawnInfo in action.SpawnsScenes)
  ↓
SceneSpawner.SpawnScene(spawnInfo)
  ↓ Creates runtime instance
Scene scene = new Scene
{
    Id = Guid.NewGuid().ToString(), // Unique runtime ID
    TemplateId = spawnInfo.SceneTemplateId,
    SpawnedBy = actionId,
    SpawnedAt = DateTime.Now,
    TargetEntityId = spawnInfo.TargetEntityId,
    IsActive = true,
    IsCompleted = false
}
  ↓ Adds to GameWorld
GameWorld.ActiveScenes.Add(scene)
  ↓ Adds to placement entity
if (spawnInfo.TargetType == SpawnTargetType.Location)
    Location.ActiveSceneIds.Add(scene.Id)
else if (spawnInfo.TargetType == SpawnTargetType.NPC)
    NPC.ActiveSceneIds.Add(scene.Id)
```

### 4.3 UI DISPLAY (NO FILTERING)

```
LocationContent.OnInitializedAsync()
  ↓
LocationFacade.GetLocationContentViewModel()
  ↓
GetActiveScenes(currentLocationId)
  ↓ Simple lookup (NO filtering)
Location location = GameWorld.GetLocation(locationId)
List<string> sceneIds = location.ActiveSceneIds
List<Scene> activeScenes = sceneIds
    .Select(id => GameWorld.ActiveScenes.Find(s => s.Id == id))
    .ToList()
  ↓ Build view models
foreach (Scene scene in activeScenes)
{
    SceneTemplate template = GameWorld.SceneTemplates.Find(t => t.Id == scene.TemplateId)
    // Build SceneCardViewModel from template + instance data
}
  ↓ Return to UI
LocationContentViewModel with List<SceneCardViewModel>
  ↓ Display (no availability checks)
```

**CRITICAL**: No `IsAvailable` checks, no prerequisite evaluation, no filtering. If scene exists in `ActiveSceneIds`, it's displayed.

---

## PART 5: WHAT CHANGES AT EACH LAYER

### 5.1 JSON LAYER CHANGES

**DELETE**:
- `05_goals.json` (replaced by scene templates)
- `isAvailable` fields (runtime state)
- `isCompleted` fields (runtime state)
- `placementNpcId` fields (spawning determines placement)
- `placementLocationId` fields (spawning determines placement)

**CREATE**:
- `01_scene_templates.json` → Pure template definitions
- `02_action_definitions.json` → Action spawn definitions

**TRANSFORM**:
```json
// OLD: Goal with placement
{
  "id": "elena_evening_service",
  "name": "Help with Evening Service",
  "placementNpcId": "elena",  ← DELETE
  "isAvailable": true,  ← DELETE (runtime state)
  "systemType": "Social",
  "deckId": "friendly_chat",
  "costs": { "time": 1, "focus": 1 }
}

// NEW: SceneTemplate (no placement)
{
  "id": "elena_help_service_template",
  "name": "Help with Evening Service",
  "systemType": "Social",
  "deckId": "friendly_chat",
  "costs": { "time": 1, "focus": 1 }
  // NO placementNpcId, NO isAvailable
}

// NEW: ActionDefinition (defines spawning)
{
  "id": "investigate_common_room",
  "name": "Survey the common room",
  "description": "Look around for work opportunities",
  "costs": { "time": 0, "focus": 0 },
  "spawnsScenes": [
    {
      "sceneTemplateId": "elena_help_service_template",
      "targetType": "NPC",
      "targetEntityId": "elena"
    },
    {
      "sceneTemplateId": "thomas_warehouse_template",
      "targetType": "NPC",
      "targetEntityId": "thomas"
    }
  ],
  "narrative": "You observe the room carefully..."
}
```

### 5.2 DTO LAYER CHANGES

**DELETE**:
- `GoalDTO.cs` (replaced by SceneTemplateDTO)
- `GoalDTO.PlacementNpcId`
- `GoalDTO.PlacementLocationId`
- `GoalDTO.IsAvailable`
- `GoalDTO.IsCompleted`

**CREATE**:
- `SceneTemplateDTO.cs`
- `ActionDefinitionDTO.cs`
- `SceneSpawnInfoDTO.cs`

**KEEP UNCHANGED**:
- `GoalCostsDTO.cs` → `ActionCostsDTO.cs` (rename only)
- `GoalCardDTO.cs` → `CardDTO.cs` (rename only)
- `ObstacleDTO.cs` → **DELETE** (obstacles become scene context, not separate entities)

### 5.3 PARSER LAYER CHANGES

**DELETE**:
- `GoalParser.cs` (replaced by SceneTemplateParser)
- `ObstacleParser.cs` (obstacles become spawn context)
- Flattening logic (ObstacleParser lines 57-70)

**CREATE**:
- `SceneTemplateParser.cs`
- `ActionDefinitionParser.cs`
- `SceneSpawner.cs` (runtime spawning logic)

**TRANSFORM**:
```csharp
// OLD: GoalParser flattens nested goals
public static Obstacle ConvertDTOToObstacle(ObstacleDTO dto, ...)
{
    foreach (GoalDTO goalDto in dto.Goals)  ← Flattening
    {
        Goal goal = GoalParser.ConvertDTOToGoal(goalDto, gameWorld);
        gameWorld.Goals.Add(goal);  ← Adds to global list
        obstacle.GoalIds.Add(goal.Id);  ← Stores reference
    }
}

// NEW: ActionDefinitionParser preserves spawn structure
public static ActionDefinition Parse(ActionDefinitionDTO dto)
{
    return new ActionDefinition
    {
        Id = dto.Id,
        Name = dto.Name,
        SpawnsScenes = dto.SpawnsScenes.Select(s => new SceneSpawnInfo
        {
            SceneTemplateId = s.SceneTemplateId,
            TargetType = ParseTargetType(s.TargetType),
            TargetEntityId = s.TargetEntityId
        }).ToList()
        // NO flattening, NO adding to GameWorld yet
    };
}
```

### 5.4 DOMAIN LAYER CHANGES

**DELETE**:
- `Goal.cs` (replaced by Scene + SceneTemplate)
- `Obstacle.cs` (becomes scene spawn context)
- `Goal.IsAvailable` property
- `Goal.IsCompleted` property
- `Goal.PlacementNpcId` property
- `NPC.ActiveGoalIds` property
- `Location.ActiveGoalIds` property

**CREATE**:
- `SceneTemplate.cs` (pure definition)
- `Scene.cs` (runtime instance)
- `ActionDefinition.cs` (action with spawns)
- `SceneSpawnInfo.cs` (spawn metadata)
- `NPC.ActiveSceneIds` property
- `Location.ActiveSceneIds` property

**TRANSFORM**:
```csharp
// OLD: Goal entity (template + instance)
public class Goal
{
    public string Id { get; set; }  // "elena_evening_service"
    public string Name { get; set; }
    public string PlacementNpcId { get; set; }  ← DELETE
    public bool IsAvailable { get; set; }  ← DELETE
    public bool IsCompleted { get; set; }  ← DELETE
    public TacticalSystemType SystemType { get; set; }
    public GoalCosts Costs { get; set; }
}

// NEW: SceneTemplate (pure template)
public class SceneTemplate
{
    public string Id { get; set; }  // "elena_help_service_template"
    public string Name { get; set; }
    public TacticalSystemType SystemType { get; set; }
    public SceneCosts Costs { get; set; }
    public List<Card> Cards { get; set; }
    // NO placement, NO state
}

// NEW: Scene (runtime instance)
public class Scene
{
    public string Id { get; set; }  // Runtime GUID
    public string TemplateId { get; set; }  // Reference to template
    public string SpawnedBy { get; set; }  // Action that created this
    public DateTime SpawnedAt { get; set; }
    public string TargetEntityId { get; set; }  // Where it's placed
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    // Instance state only
}
```

### 5.5 GAMEWORLD LAYER CHANGES

**DELETE**:
- `GameWorld.Goals` list
- `GameWorld.Obstacles` list

**CREATE**:
- `GameWorld.SceneTemplates` list (loaded at startup)
- `GameWorld.ActionDefinitions` list (loaded at startup)
- `GameWorld.ActiveScenes` list (runtime instances)
- `GameWorld.SceneSpawnHistory` list (causality tracking)

**UPDATE**:
- `NPC.ActiveGoalIds` → `NPC.ActiveSceneIds`
- `Location.ActiveGoalIds` → `Location.ActiveSceneIds`

### 5.6 UI LAYER CHANGES

**DELETE**:
- `GetAvailableGoals()` filtering logic
- `ObstacleGoalFilter.GetVisibleNPCGoals()`
- `ObstacleGoalFilter.GetVisibleLocationGoals()`
- Availability checking logic

**UPDATE**:
```csharp
// OLD: LocationFacade.GetLocationContentViewModel()
private List<NpcWithGoalsViewModel> BuildNPCsWithGoals(Location spot, TimeBlocks currentTime)
{
    foreach (NPC npc in npcsAtSpot)
    {
        // Get ALL goals for THIS NPC
        List<Goal> allNpcGoals = _obstacleGoalFilter.GetVisibleNPCGoals(npc, _gameWorld);  ← DELETE

        // Filter to available
        List<Goal> npcSocialGoals = allNpcGoals
            .Where(g => g.IsAvailable && !g.IsCompleted)  ← DELETE filtering
            .ToList();
    }
}

// NEW: LocationFacade.GetLocationContentViewModel()
private List<NpcWithScenesViewModel> BuildNPCsWithScenes(Location spot, TimeBlocks currentTime)
{
    foreach (NPC npc in npcsAtSpot)
    {
        // Get ACTIVE scenes for this NPC (NO filtering)
        List<Scene> activeScenes = npc.ActiveSceneIds
            .Select(id => _gameWorld.ActiveScenes.Find(s => s.Id == id))
            .ToList();

        // NO availability checks - presence = availability
    }
}
```

**CREATE**:
- `GetActiveScenes(locationId)` → Simple lookup
- `SceneCardViewModel` to replace `GoalCardViewModel`

---

## PART 6: SAVE/LOAD IMPACT

### 6.1 CURRENT SAVE STATE

**What's Saved**:
```json
{
  "player": {
    "activeObligationIds": ["investigate_inn_opportunities"],
    "coins": 10,
    "stamina": 5
  },
  "goals": [  ← ALL goals saved (template + state mixed)
    {
      "id": "elena_evening_service",
      "isAvailable": true,
      "isCompleted": false
    }
  ],
  "npcs": [
    {
      "id": "elena",
      "activeGoalIds": ["elena_evening_service"]
    }
  ]
}
```

**Problem**: Saves template data (goal definitions) mixed with runtime state

### 6.2 NEW SAVE STATE

**What's Saved**:
```json
{
  "player": {
    "activeObligationIds": ["investigate_inn_opportunities"],
    "coins": 10,
    "stamina": 5
  },
  "activeScenes": [  ← ONLY runtime instances saved
    {
      "id": "scene_guid_123",
      "templateId": "elena_help_service_template",
      "spawnedBy": "investigate_common_room",
      "spawnedAt": "2025-10-23T10:00:00Z",
      "targetEntityId": "elena",
      "isCompleted": false
    }
  ],
  "sceneSpawnHistory": [  ← Track causality
    {
      "actionId": "investigate_common_room",
      "sceneIds": ["scene_guid_123", "scene_guid_456"],
      "timestamp": "2025-10-23T10:00:00Z"
    }
  ],
  "npcs": [
    {
      "id": "elena",
      "activeSceneIds": ["scene_guid_123"]  ← Runtime IDs only
    }
  ]
}
```

**What's NOT Saved**:
- `SceneTemplates` (loaded from JSON at startup)
- `ActionDefinitions` (loaded from JSON at startup)
- Template data (always comes from packages)

**Migration Strategy**:
```
OLD SAVE FILE:
  ↓
Migration: Convert Goal → Scene
  - Create Scene.Id = new GUID
  - Scene.TemplateId = Goal.Id
  - Scene.IsCompleted = Goal.IsCompleted
  - Scene.SpawnedBy = "legacy_migration"
  ↓
NEW SAVE FILE FORMAT
```

---

## PART 7: CRITICAL TRANSFORMATION POINTS

### 7.1 WHERE THE PARADIGM SHIFT HAPPENS

**POINT 1: Loading (Startup)**

**OLD**:
```
PackageLoader loads 05_goals.json
  → Creates Goal entities (template + instance mixed)
  → Adds to GameWorld.Goals (ALL goals)
  → Assigns to NPC.ActiveGoalIds (placement)
```

**NEW**:
```
PackageLoader loads 01_scene_templates.json
  → Creates SceneTemplate entities (PURE definitions)
  → Adds to GameWorld.SceneTemplates (templates only)
  → NO placement, NO instances created
```

**LOCATION**: `PackageLoader.LoadGoals()` → `PackageLoader.LoadSceneTemplates()`

---

**POINT 2: Action Execution (Runtime)**

**OLD**:
```
Obligation phase completes
  → Sets Goal.IsAvailable = true (boolean gate)
  → UI filtering detects IsAvailable
  → Displays goal
```

**NEW**:
```
Action executes ("Survey the common room")
  → ActionExecutor reads ActionDefinition.SpawnsScenes
  → SceneSpawner.SpawnScene(templateId, targetEntity)
    → Creates Scene instance
    → Adds to GameWorld.ActiveScenes
    → Adds to NPC.ActiveSceneIds or Location.ActiveSceneIds
  → UI queries ActiveSceneIds (no filtering)
  → Displays scene
```

**LOCATION**: NEW `ActionExecutor.cs` + `SceneSpawner.cs`

---

**POINT 3: UI Query (Display)**

**OLD**:
```
LocationFacade.GetLocationContentViewModel()
  → ObstacleGoalFilter.GetVisibleNPCGoals(npc)
    → Filters ALL goals in GameWorld.Goals
    → Checks PlacementNpcId == npc.Id
    → Checks IsAvailable && !IsCompleted
    → Returns filtered subset
```

**NEW**:
```
LocationFacade.GetLocationContentViewModel()
  → GetActiveScenes(npc.ActiveSceneIds)
    → Simple lookup in GameWorld.ActiveScenes
    → NO filtering (presence = availability)
    → Returns ALL active scenes
```

**LOCATION**: `LocationFacade.cs:654-697` → Simplified lookup

---

### 7.2 DEPENDENCY CHAIN BREAKS

**BROKEN DEPENDENCY 1**: Goals depend on Obstacles

**OLD**: `Obstacle.GoalIds` references `GameWorld.Goals`
**NEW**: Obstacles become spawn context, not separate entities

**BROKEN DEPENDENCY 2**: Obligations spawn Goals

**OLD**: `ObligationPhase.CompletionReward.ObstaclesSpawned` creates obstacles with nested goals
**NEW**: `ActionDefinition.SpawnsScenes` creates scenes directly

**BROKEN DEPENDENCY 3**: UI filters Goals

**OLD**: `ObstacleGoalFilter` queries `GameWorld.Goals` and filters
**NEW**: UI queries `GameWorld.ActiveScenes` directly (no filter service)

---

## PART 8: WHAT STAYS THE SAME

### 8.1 UNCHANGED SYSTEMS

✅ **Challenge Subsystems** (Social/Mental/Physical)
- `SocialChallengeDeck`, `MentalChallengeDeck`, `PhysicalChallengeDeck`
- `SocialSession`, `MentalSession`, `PhysicalSession`
- Card mechanics, difficulty calculation, outcome resolution
- **WHY**: These are tactical gameplay, not strategic spawning

✅ **Card Definitions**
- `GameWorld.SocialCards`
- `GameWorld.MentalCards`
- `GameWorld.PhysicalCards`
- **WHY**: Cards are already pure templates

✅ **Entity Storage**
- `GameWorld.Venues`, `GameWorld.Locations`, `GameWorld.NPCs`
- `GameWorld.Items`, `GameWorld.Routes`
- **WHY**: These are world entities, not spawnable content

✅ **UI Components** (Mostly)
- `LocationContent.razor` structure
- `GameScreen.razor` navigation
- Challenge screens (Social/Mental/Physical)
- **WHY**: Display logic stays same, data source changes

✅ **Resource Management**
- `Player.Coins`, `Player.Stamina`, `Player.Focus`
- Cost deduction, resource tracking
- **WHY**: Costs still apply, just to scenes instead of goals

✅ **Time Management**
- `TimeManager`, `TimeBlocks`, attention system
- **WHY**: Time still advances, scenes may spawn based on time

### 8.2 MODIFIED SYSTEMS (NOT REPLACED)

🔄 **PackageLoader**
- Still loads JSON packages
- Still converts DTOs → Domain
- Still validates and flattens
- **CHANGE**: Loads templates, not instances

🔄 **LocationFacade**
- Still builds view models
- Still queries game state
- Still returns display data
- **CHANGE**: Queries ActiveScenes, not filtering Goals

🔄 **GameWorld**
- Still single source of truth
- Still flat lists for entities
- Still zero dependencies
- **CHANGE**: Stores templates + instances separately

---

## PART 9: VERIFICATION STEPS

### 9.1 HOW TO CONFIRM TRANSFORMATION COMPLETE

**VERIFICATION 1: JSON Structure**
```bash
# OLD files should not exist
[ ! -f "05_goals.json" ] || echo "ERROR: Goals file still exists"

# NEW files should exist
[ -f "01_scene_templates.json" ] && echo "PASS: Templates loaded"
[ -f "02_action_definitions.json" ] && echo "PASS: Actions loaded"

# Check for runtime state in JSON (should be NONE)
grep -r "isAvailable\|isCompleted" Content/**/*.json && echo "ERROR: Runtime state in JSON"
```

**VERIFICATION 2: Domain Entities**
```csharp
// Goal.cs should NOT exist
// SceneTemplate.cs SHOULD exist
// Scene.cs SHOULD exist

// GameWorld should have:
if (gameWorld.Goals != null) throw new Exception("Goals list still exists");
if (gameWorld.SceneTemplates == null) throw new Exception("SceneTemplates not loaded");
if (gameWorld.ActiveScenes == null) throw new Exception("ActiveScenes not initialized");
```

**VERIFICATION 3: UI Filtering**
```csharp
// ObstacleGoalFilter should NOT exist
// No calls to GetVisibleNPCGoals / GetVisibleLocationGoals

// Search codebase:
grep -r "GetVisibleNPCGoals\|GetVisibleLocationGoals" src/ && echo "ERROR: Filtering still exists"
grep -r "IsAvailable.*IsCompleted" src/ && echo "ERROR: Boolean gate checks still exist"
```

**VERIFICATION 4: Runtime Spawning**
```csharp
// When action executes, scenes should spawn
ActionExecutor.Execute("investigate_common_room");

// Verify scenes created
var activeScenes = gameWorld.ActiveScenes.Where(s => s.SpawnedBy == "investigate_common_room");
if (activeScenes.Count() == 0) throw new Exception("No scenes spawned");

// Verify placement
var elena = gameWorld.NPCs.Find(n => n.ID == "elena");
if (!elena.ActiveSceneIds.Any()) throw new Exception("Scene not placed on NPC");
```

**VERIFICATION 5: Save/Load**
```csharp
// Save game
var saveData = SaveGame();

// Should NOT contain SceneTemplates (those come from packages)
if (saveData.Contains("SceneTemplates")) throw new Exception("Templates in save file");

// SHOULD contain ActiveScenes (runtime instances)
if (!saveData.Contains("ActiveScenes")) throw new Exception("No active scenes in save");

// Load game
LoadGame(saveData);

// Verify scenes restored
if (gameWorld.ActiveScenes.Count == 0) throw new Exception("Scenes not restored");
```

---

## PART 10: MIGRATION COMPLEXITY ASSESSMENT

### 10.1 RISK AREAS

**HIGH RISK**:
1. **Save file compatibility** → Old saves have Goals, new system expects Scenes
2. **Obligation phase progression** → Currently spawns obstacles with goals, needs action-based spawning
3. **Tutorial system** → Heavily relies on goal availability gates
4. **UI goal detail screen** → Displays Goal properties, needs Scene properties

**MEDIUM RISK**:
1. **PackageLoader sequencing** → Templates must load before actions
2. **GameWorld initialization** → New lists need initialization
3. **NPC/Location placement** → ActiveGoalIds → ActiveSceneIds references
4. **Filtering service deletion** → ObstacleGoalFilter used throughout

**LOW RISK**:
1. **DTO definitions** → Simple rename and restructure
2. **Parser logic** → Replace GoalParser with SceneTemplateParser
3. **JSON content** → Rewrite goals as templates
4. **View models** → GoalCardViewModel → SceneCardViewModel

### 10.2 ESTIMATED IMPACT RADIUS

**FILES TO DELETE** (~15 files):
- `Goal.cs`, `GoalParser.cs`, `GoalDTO.cs`
- `Obstacle.cs`, `ObstacleParser.cs`, `ObstacleDTO.cs`
- `ObstacleGoalFilter.cs`
- `05_goals.json` (rewrite as templates)

**FILES TO CREATE** (~10 files):
- `SceneTemplate.cs`, `SceneTemplateParser.cs`, `SceneTemplateDTO.cs`
- `Scene.cs`
- `ActionDefinition.cs`, `ActionDefinitionParser.cs`, `ActionDefinitionDTO.cs`
- `SceneSpawner.cs`, `ActionExecutor.cs`
- `01_scene_templates.json`, `02_action_definitions.json`

**FILES TO MODIFY** (~25 files):
- `GameWorld.cs` (add SceneTemplates/ActiveScenes lists)
- `PackageLoader.cs` (load templates instead of goals)
- `LocationFacade.cs` (query ActiveScenes instead of filtering)
- `LocationContent.razor.cs` (display scenes)
- `NPC.cs`, `Location.cs` (ActiveGoalIds → ActiveSceneIds)
- `SaveGameService.cs` (save/load ActiveScenes)
- All UI components displaying goals
- Tutorial system files

**TOTAL ESTIMATED**: ~50 files touched, ~5,000 lines changed

---

## PART 11: ARCHITECTURAL PRINCIPLES VALIDATION

### 11.1 DOES THIS UPHOLD ARCHITECTURE?

**✅ PRINCIPLE 1: Single Source of Truth**
- **OLD**: Goals in `GameWorld.Goals` AND `NPC.ActiveGoalIds` (duplication)
- **NEW**: Scenes in `GameWorld.ActiveScenes` ONLY, references by ID

**✅ PRINCIPLE 2: Strong Typing**
- **OLD**: `Goal` mixes template and instance (weak separation)
- **NEW**: `SceneTemplate` vs `Scene` (strong separation)

**✅ PRINCIPLE 3: NO Boolean Gates**
- **OLD**: `IsAvailable` checks everywhere (boolean gate)
- **NEW**: Spawning creates presence (no gates)

**✅ PRINCIPLE 4: Verisimilitude**
- **OLD**: "Goal becomes available magically"
- **NEW**: "Action causes scene to appear" (causal)

**✅ PRINCIPLE 5: GameWorld Zero Dependencies**
- **OLD**: GameWorld stores goals (no dependency issues)
- **NEW**: GameWorld stores templates + instances (no dependency issues)

**✅ PRINCIPLE 6: Parser-JSON-Entity Triangle**
- **OLD**: ✅ Already aligned
- **NEW**: ✅ Remains aligned (SceneTemplateDTO → SceneTemplate)

### 11.2 ARCHITECTURAL DEBT ELIMINATED

**DEBT 1: Template/Instance Confusion**
- **OLD**: `Goal` entity is BOTH template ("Help Elena") AND instance ("Help Elena right now")
- **NEW**: `SceneTemplate` (pure def) vs `Scene` (runtime instance)

**DEBT 2: Runtime State in JSON**
- **OLD**: `isAvailable: true` in 05_goals.json (runtime state in content file)
- **NEW**: Templates in JSON have NO state, state lives in `Scene` instances only

**DEBT 3: Filtering Complexity**
- **OLD**: `ObstacleGoalFilter` with complex availability checks
- **NEW**: Simple presence check (if in ActiveSceneIds, it exists)

**DEBT 4: Placement Duplication**
- **OLD**: `Goal.PlacementNpcId` AND `NPC.ActiveGoalIds` (two sources of truth)
- **NEW**: `NPC.ActiveSceneIds` ONLY (single source)

---

## CONCLUSION

### TRANSFORMATION SUMMARY

**FROM**:
- Static library of goals loaded at startup
- Filtered at runtime by availability checks
- Boolean gates (`IsAvailable`, `IsCompleted`)
- Template/instance confusion
- Runtime state in JSON

**TO**:
- Pure templates loaded at startup
- Dynamic scenes spawned by actions
- Presence-based availability (no filtering)
- Clean template/instance separation
- JSON contains only definitions

### CRITICAL SUCCESS FACTORS

1. ✅ **Templates separated from instances** (SceneTemplate vs Scene)
2. ✅ **Spawning causality tracked** (Scene.SpawnedBy records action)
3. ✅ **No filtering needed** (ActiveSceneIds presence = availability)
4. ✅ **Save state simplified** (save instances, load templates from packages)
5. ✅ **Architecture principles upheld** (no boolean gates, strong typing, single source)

### NEXT STEPS (NOT IMPLEMENTED YET)

1. Create `SceneTemplate.cs` and `Scene.cs` entities
2. Create `ActionDefinition.cs` entity
3. Create `SceneTemplateParser.cs` and `ActionDefinitionParser.cs`
4. Create `SceneSpawner.cs` runtime spawning service
5. Modify `PackageLoader.cs` to load templates
6. Modify `GameWorld.cs` to store templates + active scenes
7. Rewrite `LocationFacade.cs` to query ActiveScenes (delete filtering)
8. Update UI components to display SceneCardViewModel
9. Rewrite JSON content (goals → templates, create action definitions)
10. Update save/load system
11. Migrate tutorial system from goal gates to action spawning

---

**END OF ARCHITECTURAL ANALYSIS**
