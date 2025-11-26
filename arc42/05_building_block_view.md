# 5. Building Block View

This section shows the static decomposition of Wayfarer into building blocks. Level 1 opens the blackbox from the context diagram, revealing the system's internal structure.

---

## 5.1 Level 1: Overall System Whitebox

The context diagram showed Wayfarer as a blackbox receiving player input and content packages. This whitebox reveals the internal decomposition.

```mermaid
flowchart TB
    subgraph External
        Player([Player])
        Content[(Content Packages)]
    end

    subgraph Wayfarer["Wayfarer (Whitebox)"]
        subgraph UI["UI Layer"]
            GameScreen[GameScreen]
            Components[Screen Components]
        end

        subgraph Services["Service Layer"]
            GameFacade[GameFacade]
            TacticalFacades[Tactical Facades]
            SupportFacades[Support Facades]
        end

        subgraph Domain["Domain Layer"]
            GameWorld[(GameWorld)]
        end

        subgraph Pipeline["Content Pipeline"]
            Parsers[Parsers]
            Catalogues[Catalogues]
        end
    end

    Player -->|interactions| GameScreen
    GameScreen --> Components
    Components --> GameFacade
    GameFacade --> TacticalFacades
    GameFacade --> SupportFacades
    TacticalFacades --> GameWorld
    SupportFacades --> GameWorld
    Content -->|JSON| Parsers
    Parsers --> Catalogues
    Catalogues -->|entities| GameWorld
```

| Building Block | Responsibility |
|----------------|----------------|
| **UI Layer** | Blazor components rendering game state; captures player input; contains no game logic |
| **GameFacade** | Orchestrates cross-cutting operations; delegates to specialized facades; single entry point for UI |
| **Tactical Facades** | Business logic for three parallel challenge systems (Social, Mental, Physical) |
| **Support Facades** | Business logic for game systems (Location, Resource, Time, Travel, Token, Narrative) |
| **GameWorld** | Single source of truth; holds all game state; zero external dependencies |
| **Parsers** | Convert JSON to DTOs; invoke catalogues for translation; create domain entities |
| **Catalogues** | Translate categorical properties to concrete values at parse-time |

**Decomposition Rationale:** The layered structure enforces dependency inversion—all arrows point toward GameWorld. The separation between tactical facades (challenge systems) and support facades (game systems) reflects the two-layer architecture from the solution strategy.

---

## 5.2 Level 2: GameWorld Whitebox

GameWorld is the central state container. This whitebox shows its internal collections.

```mermaid
flowchart TB
    subgraph GameWorld["GameWorld (Whitebox)"]
        subgraph Spatial["Spatial Entities"]
            Venues[(Venues)]
            Locations[(Locations)]
            HexMap[(HexMap)]
        end

        subgraph Characters["Characters"]
            Player[(Player)]
            NPCs[(NPCs)]
        end

        subgraph Strategic["Strategic Layer"]
            Scenes[(Scenes)]
            Obligations[(Obligations)]
            Templates[(SceneTemplates)]
        end

        subgraph Tactical["Tactical Layer"]
            SocialCards[(SocialCards)]
            MentalCards[(MentalCards)]
            PhysicalCards[(PhysicalCards)]
            Decks[(Challenge Decks)]
        end

        subgraph Actions["Actions"]
            LocationActions[(LocationActions)]
            PlayerActions[(PlayerActions)]
        end
    end
```

| Building Block | Responsibility |
|----------------|----------------|
| **Venues** | Top-level location clusters; contain multiple locations |
| **Locations** | Specific places; hold hex position as spatial source of truth |
| **HexMap** | Grid-based world representation; enables pathfinding |
| **Player** | Single player instance; resources, stats, position, inventory |
| **NPCs** | Non-player characters; relationships, personalities, locations |
| **Scenes** | Persistent narrative containers; embed situations |
| **Obligations** | Quest definitions; trigger scene spawning |
| **SceneTemplates** | Immutable archetypes; define scene patterns |
| **SocialCards/MentalCards/PhysicalCards** | Tactical cards for three challenge systems |
| **Challenge Decks** | Card collections defining challenge configurations |
| **LocationActions/PlayerActions** | Atmospheric scaffolding; persistent gameplay actions |

**Decomposition Rationale:** Collections group by concern—spatial entities define the world, characters populate it, strategic entities drive narrative, tactical entities enable challenges. This mirrors the two-layer architecture.

---

See [§8.11 Location Accessibility Architecture](08_crosscutting_concepts.md#811-location-accessibility-architecture) and [ADR-012](09_architecture_decisions.md#adr-012-dual-model-location-accessibility).

**Decomposition Rationale:** GameFacade delegates everything—it contains no business logic. Tactical facades handle the three parallel challenge systems with equivalent depth. Support facades handle game systems. This ensures single responsibility and testability.

---

## Related Documentation

- [03_context_and_scope.md](03_context_and_scope.md) — Context diagram showing system as blackbox
- [04_solution_strategy.md](04_solution_strategy.md) — Decisions driving this decomposition
- [06_runtime_view.md](06_runtime_view.md) — Dynamic behavior of these blocks
