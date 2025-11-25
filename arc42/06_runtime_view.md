# 6. Runtime View

This section shows key runtime scenarios illustrating how building blocks interact.

---

## 6.1 Strategic Choice Execution

The most common runtime flow: player selects a choice, system applies costs/rewards, scene advances.

```mermaid
sequenceDiagram
    participant Player
    participant UI as GameScreen
    participant GF as GameFacade
    participant SF as SceneFacade
    participant GW as GameWorld

    Player->>UI: Select choice
    UI->>GF: ExecuteChoice(choice)
    GF->>SF: ProcessChoice(choice)
    SF->>GW: Apply costs
    SF->>GW: Apply rewards
    SF->>GW: Advance situation
    SF-->>GF: Result
    GF-->>UI: Updated state
    UI-->>Player: Render new state
```

---

## 6.2 Strategic-Tactical Bridge

When a choice crosses from strategic to tactical layer via StartChallenge action type.

```mermaid
sequenceDiagram
    participant Player
    participant UI as GameScreen
    participant GF as GameFacade
    participant SF as SceneFacade
    participant TF as TacticalFacade
    participant GW as GameWorld

    Player->>UI: Select challenge choice
    UI->>GF: ExecuteChoice(choice)
    GF->>SF: ProcessChoice(choice)
    SF->>GW: Apply entry costs
    SF->>TF: CreateSession(situationCards)
    TF->>GW: Store session state
    TF-->>UI: Navigate to tactical screen

    Note over Player,GW: Tactical play occurs...

    Player->>UI: Complete challenge
    TF->>GW: Apply outcome rewards
    TF->>SF: ReturnToStrategic(outcome)
    SF->>GW: Advance situation
    SF-->>UI: Navigate to strategic screen
```

---

## 6.3 Content Loading (Startup)

One-time flow at application startup populating GameWorld.

```mermaid
sequenceDiagram
    participant App as Application
    participant PL as PackageLoader
    participant P as Parsers
    participant C as Catalogues
    participant GW as GameWorld

    App->>PL: LoadContent()
    loop Each JSON package
        PL->>P: Parse(jsonFile)
        P->>C: Translate(categoricalProps)
        C-->>P: Concrete values
        P-->>PL: Domain entities
        PL->>GW: Add to collections
    end
    PL-->>App: GameWorld populated
```

---

## Related Documentation

- [05_building_block_view.md](05_building_block_view.md) — Static structure of these blocks
- [04_solution_strategy.md](04_solution_strategy.md) — Two-layer architecture driving these flows
