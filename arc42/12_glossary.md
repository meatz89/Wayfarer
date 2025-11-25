# 12. Glossary

Domain and technical terms used throughout this documentation.

---

## Domain Terms

| Term | Definition |
|------|------------|
| **A-Story** | The infinite, procedurally-generated main storyline providing structure without resolution |
| **Atmospheric Action** | Persistent gameplay scaffolding (Travel, Work, Rest) that prevents soft-locks; always available |
| **Bridge** | The ActionType mechanism crossing from strategic to tactical layer via StartChallenge |
| **Categorical Property** | Descriptive enum (Friendly, Premium, Hostile) translated to concrete values at parse-time |
| **Challenge** | Tactical card-based gameplay session (Social, Mental, or Physical) |
| **Choice** | Player-facing option within a Situation; has costs, requirements, and rewards |
| **Connection State** | NPC relationship level (Disconnected → Guarded → Neutral → Receptive → Trusting) |
| **Fallback Choice** | Zero-requirement option ensuring forward progress; prevents soft-locks |
| **Four-Choice Pattern** | Standard situation structure: stat-gated, money-gated, challenge, fallback |
| **Impossible Choice** | Design goal: player must choose between multiple suboptimal paths, revealing character through constraint |
| **Obligation** | Quest definition triggering scene spawning; drives narrative progression |
| **Perfect Information** | Strategic layer principle: all costs/rewards visible before commitment |
| **Scene** | Persistent narrative container holding embedded Situations; spawns from template |
| **Situation** | Single decision point within a Scene; presents Choices to player |
| **SituationCard** | Tactical victory condition defining threshold and rewards |
| **Soft-Lock** | Game state with no forward progress; TIER 1 violation, never acceptable |
| **Venue** | Top-level location cluster containing multiple Locations |

---

## Technical Terms

| Term | Definition |
|------|------------|
| **Catalogue** | Static class translating categorical properties to concrete values at parse-time |
| **DTO** | Data Transfer Object; intermediate structure between JSON and domain entity |
| **EntityResolver** | Pattern for find-or-create queries using categorical filters |
| **Facade** | Stateless service encapsulating business logic for one domain area |
| **GameWorld** | Single source of truth; state container with zero external dependencies |
| **HIGHLANDER** | "There can be only one" — single canonical storage for each piece of state |
| **Hex Position** | AxialCoordinates (Q, R) defining spatial location on hex grid |
| **InstantiationState** | Scene/Situation lifecycle: Deferred → Instantiated |
| **Object Reference** | Direct entity relationship (not ID string); e.g., `NPC.Location` |
| **Parse-Time Translation** | One-time categorical → concrete conversion at startup |
| **PlacementFilter** | Categorical criteria for entity resolution (Purpose, Safety, Demeanor) |
| **Query-Time** | Third timing tier; actions created when player enters context |
| **Spawn-Time** | Second timing tier; scenes/situations created when triggered |
| **Template** | Immutable archetype (SceneTemplate, ChoiceTemplate); first timing tier |
| **Template ID** | Identifier for immutable archetypes; allowed (unlike entity instance IDs) |

---

## Architectural Terms

| Term | Definition |
|------|------------|
| **Clean Architecture** | Dependencies flow inward: UI → Facades → GameWorld |
| **Fail-Fast** | Errors surface immediately at point of failure; no defensive hiding |
| **Idempotent** | Safe to execute multiple times; required for Blazor double-render |
| **Layer Separation** | Strategic (perfect info) vs Tactical (hidden complexity) |
| **Single Source of Truth** | Each state has exactly one canonical storage location |
| **Stateless Service** | Facade containing logic but no state; operates on GameWorld |
| **Three-Tier Timing** | Templates (parse) → Instances (spawn) → Actions (query) |
| **Whitebox** | Internal decomposition of a higher-level blackbox |

---

## Challenge System Terms

| Term | Definition |
|------|------------|
| **Social Challenge** | Conversation with NPC; Momentum/Initiative/Doubt resources |
| **Mental Challenge** | Investigation/puzzle; Progress/Attention/Exposure resources |
| **Physical Challenge** | Obstacle/combat; Breakthrough/Exertion/Danger resources |
| **Momentum** | Builder resource in Social challenges; accumulates toward threshold |
| **Progress** | Builder resource in Mental challenges |
| **Breakthrough** | Builder resource in Physical challenges |
| **Threshold** | Victory condition value; reaching it completes SituationCard |

---

## Abbreviations

| Abbreviation | Expansion |
|--------------|-----------|
| **ADR** | Architecture Decision Record |
| **DI** | Dependency Injection |
| **DTO** | Data Transfer Object |
| **NPC** | Non-Player Character |
| **UI** | User Interface |
