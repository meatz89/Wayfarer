# Wayfarer Game Architecture: Program Flow Design

## Core Architecture Layers

### 1. State Layer
- **GameWorld**: Central state container
  - **Player**: Character skills, stats, cards, progression
  - **WorldState**: Game environment, locations, NPCs, flags
  - **ActionStateTracker**: Current actions, encounters, choices

### 2. Manager Layer
- **GameWorldManager**: Main orchestrator, no direct UI dependencies
- **EncounterSystem**: Encounter generation, progression, conclusions
- **LocationSystem**: Location data, spot management, transitions
- **ActionProcessor**: Action validation, execution, consequences

### 3. AI Integration Layer
- **AIGameMaster**: API for AI-driven encounter generation
- **AIResponseProcessor**: Converts AI responses to game entities
- **ChoiceConverter**: Transforms AI choices to player-facing options

### 4. Presentation Layer
- **GameUI**: Stateless view that renders current game state
- **EncounterViewBase**: Renders encounter state and choices

## Key Data Flows

### Game Initialization Flow
```
Program.Main()
└── ConfigureServices()
    ├── Register all services (repositories, systems, managers)
    └── Initialize GameWorld with empty state
        └── GameWorldManager.StartGame()
            ├── ProcessPlayerArchetype()
            ├── Initialize first location
            ├── Set initial time
            └── UpdateState() → Updates available actions
```

### Action Execution Flow
```
GameUI.HandleActionSelection(UserActionOption)
└── GameWorldManager.ExecuteAction(action)
    ├── Set current action in ActionStateTracker
    ├── Determine execution type (Instant vs Encounter)
    ├── IF Instant:
    │   └── ActionProcessor.ProcessAction() → Apply effects directly
    │
    └── IF Encounter:
        ├── EncounterSystem.GenerateEncounter() → Creates EncounterManager
        ├── AI generates initial beat and choices
        ├── Store encounter in ActionStateTracker
        └── Return to UI showing initial choices
```

### Encounter Flow
```
EncounterViewBase.HandleChoiceSelection(UserEncounterChoiceOption)
└── GameWorldManager.ExecuteEncounterChoice(choice)
    ├── EncounterSystem.ExecuteChoice()
    │   ├── ChoiceResolver.ResolveChoice()
    │   │   ├── Apply focus cost
    │   │   ├── Exhaust skill card if used
    │   │   ├── Perform skill check
    │   │   └── Apply payload (success or failure)
    │   └── Update encounter state
    │
    ├── IF encounter ongoing:
    │   ├── Generate next AI beat 
    │   ├── Create new choice options
    │   └── Update ActionStateTracker with new choices
    │
    └── IF encounter complete:
        ├── Process conclusion 
        ├── Apply persistent changes to world
        ├── CompleteAction in ActionStateTracker
        └── Update game state
```

### Choice Projection System
```
EncounterViewBase.ShowTooltip(UserEncounterChoiceOption)
└── GameWorldManager.GetChoicePreview(choice)
    └── EncounterSystem.GetChoiceProjection(encounter, choice)
        ├── Project focus cost impacts
        ├── Calculate skill check success chances
        ├── Preview payload effects (without applying them)
        └── Return projected outcomes for UI display
```

## Critical State Management Points

### GameWorldManager Methods (Central Coordination)

1. **StartGame()**: Initializes world, player position, and first state update
2. **ExecuteAction()**: Entry point for all player-initiated actions
3. **ExecuteEncounterChoice()**: Processes encounter beat selections
4. **UpdateState()**: Refreshes available actions after state changes
5. **GetChoicePreview()**: Projects choice outcomes without applying them

### EncounterSystem Methods (Encounter Management)

1. **GenerateEncounter()**: Creates encounter context and first beat
2. **ExecuteChoice()**: Processes player choice within encounter
3. **GetChoiceProjection()**: Calculates potential outcomes for UI preview
4. **GetCurrentEncounter()**: Retrieves active encounter from state

### ActionProcessor Methods (Action Resolution)

1. **InitializeEncounter()**: Creates encounter context from action
2. **ProcessAction()**: Resolves immediate effects for instant actions
3. **CanExecute()**: Validates if action requirements are met
4. **UpdateState()**: Refreshes action availability based on game state

## Implementation Guide

### Key State Changes

1. **ActionStateTracker** holds the current state for UI to read:
   - `CurrentUserAction`: Currently executing action
   - `ActiveEncounter`: Current encounter if one is active
   - `UserEncounterChoiceOptions`: Available choices for current encounter beat
   - `LocationSpotActions`: Available actions at current location spot

2. **Each player choice should**:
   - Update appropriate state in GameWorld
   - Return immediately to UI without blocking
   - Prepare next set of options in ActionStateTracker

3. **Choice projection should**:
   - Create temporary clone of relevant state
   - Apply effects to clone
   - Return projected outcome without modifying actual state

4. **Encounter state progression**:
   - Initialize with first AI beat
   - Process each choice independently
   - Generate next beat after each choice
   - Maintain encounter state between choices

### UI Interaction Pattern

```
UI never calls directly into EncounterSystem or lower-level services
↓
All UI requests go through GameWorldManager
↓
GameWorldManager updates game state
↓
UI reads from ActionStateTracker and renders accordingly
```

This architecture maintains clean separation between UI and game logic while supporting the asynchronous, event-driven approach needed for Blazor components.