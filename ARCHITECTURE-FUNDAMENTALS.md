# Wayfarer Architecture Fundamentals

**CRITICAL: Read this before working on any Wayfarer code**

---

## Backend vs Frontend Entity Distinction

### The Problem This Solves

Games need both:
- **Data containers** for game mechanics (backend)
- **Player-facing UI elements** (frontend)

Conflating these creates architectural chaos. Wayfarer maintains strict separation.

---

## Obstacle vs Goal (The Core Pattern)

### Obstacle (Backend Entity)

**Purpose**: Container for challenge metadata that spawns multiple resolution approaches

**Contents**:
```csharp
public class Obstacle
{
    public string Id { get; set; }
    public string Name { get; set; }              // Backend identifier
    public string Description { get; set; }       // Backend documentation
    public int Intensity { get; set; }            // 1-3 scale (Core Loop)
    public List<ObstacleContext> Contexts { get; set; }  // Physical/Mental/Social tags
    public bool IsPermanent { get; set; }         // Cleared or persists
    public List<string> GoalIds { get; set; }     // Multiple resolution approaches
}
```

**Spawned By**:
- Investigation phases (PhaseCompletionReward.ObstaclesSpawned)
- Route segments (PathCard.ObstacleId)
- Initial location setup (Location.ObstacleIds)

**Referenced By**:
- Locations: `Location.ObstacleIds` (where obstacle is active)
- NPCs: `NPC.ObstacleIds` (social barriers)
- Routes: `PathCard.ObstacleId` (path challenge)
- Goals: `Goal.ParentObstacleId` (metadata source)

**NEVER**:
- Displayed as separate card to player
- Shown in UI outside of Goal context
- Has its own component for rendering

---

### Goal (Frontend Entity)

**Purpose**: Player-facing action button that starts a tactical challenge

**Contents**:
```csharp
public class Goal
{
    public string Id { get; set; }
    public string Name { get; set; }              // Player-facing action text
    public string Description { get; set; }       // Player-facing explanation
    public string ParentObstacleId { get; set; }  // References parent metadata
    public ChallengeSystemType SystemType { get; set; }  // Physical/Mental/Social
    public string DeckId { get; set; }            // Which challenge deck to use
    public string PlacementLocationId { get; set; }  // Where goal appears
    public string PlacementNpcId { get; set; }    // Or which NPC offers it
    public List<GoalCard> GoalCards { get; set; }  // Victory conditions
}
```

**Spawned By**:
- Obstacles (one obstacle spawns multiple goals for different approaches)
- Investigations (quest progression)
- NPCs (social requests)

**Displayed As**:
- Action button at location
- Action button in NPC interaction
- Path card option during travel

**Shows Parent Obstacle Data**:
```razor
<!-- Goal Action Button -->
<div class="goal-action-btn" @onclick="StartGoal(goal)">
    <div class="goal-name">@goal.Name</div>
    <div class="goal-description">@goal.Description</div>

    <!-- Parent Obstacle Metadata -->
    <div class="goal-intensity">Intensity: @GetParentObstacle(goal).Intensity / 3</div>
    <div class="goal-contexts">
        @foreach (var context in GetParentObstacle(goal).Contexts)
        {
            <ContextTag ContextName="@context.ToString()" />
        }
    </div>

    <!-- Equipment Matching (via parent Obstacle contexts) -->
    @if (HasMatchingEquipment(goal))
    {
        <div class="equipment-help">
            Equipment can help: @GetMatchingEquipmentNames(goal)
        </div>
    }
</div>
```

---

## Example Flow: Mill Investigation

### 1. Investigation Spawns Obstacle

Investigation phase completion spawns:
```json
{
  "obstaclesSpawned": [
    {
      "targetType": "Location",
      "targetEntityId": "mill_entrance",
      "obstacle": {
        "id": "mill_entrance_blocked",
        "name": "Boarded Door",
        "description": "Heavy boards cover the main entrance",
        "intensity": 2,
        "contexts": ["Strength", "Precision"],
        "isPermanent": false,
        "goals": [
          {
            "id": "force_door",
            "name": "Force the door open",
            "description": "Break through the boards with brute strength",
            "systemType": "Physical",
            "deckId": "physical_challenge",
            ...
          },
          {
            "id": "find_alternate_entry",
            "name": "Find another way in",
            "description": "Search for an unboarded window",
            "systemType": "Mental",
            "deckId": "mental_challenge",
            ...
          }
        ]
      }
    }
  ]
}
```

### 2. Backend Processing

```csharp
// Parser creates Obstacle
Obstacle obstacle = new Obstacle
{
    Id = "mill_entrance_blocked",
    Intensity = 2,
    Contexts = ["Strength", "Precision"],
    GoalIds = ["force_door", "find_alternate_entry"]
};

// Add to GameWorld.Obstacles list
GameWorld.Obstacles.Add(obstacle);

// Add reference to Location
Location.ObstacleIds.Add("mill_entrance_blocked");

// Create Goals from obstacle.Goals JSON
foreach (var goalData in obstacleData.Goals)
{
    Goal goal = new Goal
    {
        Id = goalData.Id,
        Name = goalData.Name,
        ParentObstacleId = "mill_entrance_blocked",  // Reference parent
        PlacementLocationId = "mill_entrance"
    };
    GameWorld.Goals.Add(goal.Id, goal);
}
```

### 3. Frontend Display

```razor
<!-- LocationContent.razor -->

<!-- Actions Available Here -->
@foreach (var goal in AvailablePhysicalGoals)
{
    <div class="goal-action-btn" @onclick="() => StartPhysicalGoal(goal)">
        <div class="goal-system-badge physical">Physical</div>
        <div class="goal-name">Force the door open</div>
        <div class="goal-description">Break through the boards with brute strength</div>

        <!-- Parent Obstacle Metadata -->
        <div class="goal-difficulty">
            Intensity: 2/3
            Contexts: [Strength] [Precision]
        </div>

        <!-- Equipment Matching -->
        <div class="equipment-help">
            ⚙ Crowbar can help (-1 intensity)
        </div>
    </div>
}

<!-- NO SEPARATE OBSTACLE CARD -->
<!-- Obstacle data only accessed through Goal reference -->
```

### 4. Player Interaction

1. Player sees action button: **"Force the door open"**
2. Button shows parent Obstacle metadata: Intensity 2, contexts [Strength] [Precision]
3. Button shows equipment help: "Crowbar can help"
4. Player clicks button → starts Physical challenge
5. Challenge uses parent Obstacle.Intensity and Obstacle.Contexts

---

## Why This Architecture

### Correct Pattern (Current):
```
Obstacle (backend) → spawns multiple Goals (frontend) → Player sees Goals as action buttons
```

**Benefits**:
- One Obstacle can have multiple resolution approaches (force door vs find alternate)
- Equipment matching calculated once (via Obstacle.Contexts)
- Clear separation of data (Obstacle) from presentation (Goal)
- Goals can reference shared parent metadata

### Anti-Pattern (DO NOT DO):
```
Obstacle (frontend card) + Goals (action buttons) → Both shown separately
```

**Problems**:
- Duplicate display: Obstacle card + Goal buttons showing same info
- Confusing UI: "Is the obstacle separate from the goal?"
- Architectural violation: Backend entity leaked to frontend

---

## Other Backend vs Frontend Patterns

### Investigation vs Obligation

**Investigation (Backend)**:
- Template defining phases, requirements, rewards
- Contains phase definitions with obstacles to spawn
- NOT a player-facing card

**Active Obligation (Frontend)**:
- Entry in ObligationList showing investigation name, deadline, patron
- Player sees THIS, not raw Investigation template
- References Investigation for metadata

### Route vs TravelSession

**Route (Backend)**:
- Defines segments, path options, obstacles
- Contains PathCard definitions

**TravelSession (Frontend)**:
- Active journey with current segment, stamina, discovered paths
- Player interacts with THIS during travel
- References Route for segment data

---

## Component Architecture Rules

### CORRECT: Goal Components
- `LocationContent.razor` shows Goal action buttons
- `TravelPathContent.razor` shows Goal preview (via parent Obstacle)
- Goal buttons fetch parent Obstacle data for preview
- Equipment matching displays via parent Obstacle contexts

### INCORRECT: Obstacle Components
- ❌ ObstacleDisplay.razor - violates backend/frontend separation
- ❌ "Obstacles at This Location" section in UI
- ❌ Separate obstacle cards alongside goal buttons

---

## Validation Checklist

When adding new features, ask:

**Is this entity backend or frontend?**
- Backend = Data container, spawns other entities, not shown directly
- Frontend = Player-facing UI, references backend for data

**Does this entity spawn other entities?**
- Yes = Backend (Obstacle spawns Goals)
- No = Probably frontend (Goal doesn't spawn anything)

**Should player click this entity directly?**
- Yes = Frontend (Goal action button)
- No = Backend (Obstacle metadata)

**Does this need its own component?**
- Frontend entity = Yes (Goal action button)
- Backend entity = No (Obstacle accessed via Goal)

---

## References

- **Goal.cs**: Frontend entity with ParentObstacleId reference
- **Obstacle.cs**: Backend entity with GoalIds list
- **TravelPathContent.razor**: Correct pattern - shows Goals with parent Obstacle preview
- **CONTENT-AUTHORING-CHECKLIST.md**: Content examples following this pattern
