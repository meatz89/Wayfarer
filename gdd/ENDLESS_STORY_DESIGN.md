# Endless Story Design - Quick Reference

**Purpose:** One-page overview for session continuity. Details live in referenced documents.

---

## High-Concept Flow Diagrams

### Story Causality

```mermaid
flowchart TD
    A[A-Story<br/>Building → Checking] -->|SUCCESS at check| B[B-Story Spawns]
    A -->|JOURNEY to location| C[C-Story Emerges]

    B -->|continues narrative| B2[Same Characters<br/>Same Locations]
    B2 -->|great rewards| R[Resources for Travel]

    C -->|natural texture| C2[Route Encounters<br/>Location Flavor<br/>NPC Moments]

    R -->|funds| A2[Travel to Next A-Story]
    A2 -->|distance N+1| A
```

### The A-Story Rhythm

```mermaid
flowchart LR
    subgraph Building Phase
        B1[Situation] -->|stat growth choice| B2[Stats Accumulate]
        B2 --> B3[Situation]
        B3 -->|stat growth choice| B4[Stats Accumulate]
    end

    subgraph Checking Phase
        C1[Hard Stat Check]
        C1 -->|SUCCESS| R[B-Story Reward]
        C1 -->|FALLBACK| F[Progress, No Reward]
    end

    B4 --> C1
    R --> N[Next Building Phase]
    F --> N
```

### Economic Loop

```mermaid
flowchart TD
    subgraph "Resource Sink"
        A1[A-Story at Distance N]
        T[Travel Cost]
    end

    subgraph "Resource Source"
        B[B-Story Rewards]
        W[Atmospheric Work<br/>Fallback]
    end

    subgraph "Texture Layer"
        C[C-Stories<br/>Minor Resources]
    end

    A1 -->|requires| T
    B -->|funds| T
    W -->|funds slowly| T
    T -->|journey creates| C

    A1 -->|success spawns| B
    T -->|arrive| A2[A-Story at Distance N+1]
    A2 -->|repeat| A1
```

### Route Choice (Impossible Choice)

```
                    ┌─────────────────────────────────────────┐
                    │           DESTINATION (A-Story)          │
                    └─────────────────────────────────────────┘
                                        ▲
            ┌───────────────────────────┼───────────────────────────┐
            │                           │                           │
    ┌───────┴───────┐           ┌───────┴───────┐           ┌───────┴───────┐
    │  ROAD (Safe)  │           │ FOREST (Fast) │           │MOUNTAIN(Direct)│
    │───────────────│           │───────────────│           │───────────────│
    │ Time: Long    │           │ Time: Medium  │           │ Time: Short   │
    │ Stamina: Low  │           │ Stamina: High │           │ Stamina: V.High│
    │ Coins: Tolls  │           │ Coins: Free   │           │ Coins: Free   │
    │ C-Story: Law  │           │ C-Story: Ambush│          │ C-Story: Harsh│
    └───────────────┘           └───────────────┘           └───────────────┘
                                        ▲
                                        │
                            ┌───────────┴───────────┐
                            │    ORIGIN (Player)    │
                            └───────────────────────┘

    Cannot optimize all dimensions. Pick your trade-off.
```

---

## The Core Insight

A-Story creates B-Stories (success rewards) and C-Stories (journey texture). They are causally linked, not independent systems.

---

## Detailed Documentation

| Topic | Document | Section |
|-------|----------|---------|
| **Story Category Definitions** | [08_glossary.md](08_glossary.md) | §Story Categories |
| **A/B/C Property Matrix** | [08_glossary.md](08_glossary.md) | Story Category Property Matrix |
| **Travel Cost Gate** | [05_content.md](05_content.md) | §5.7 |
| **Terrain System** | [05_content.md](05_content.md) | §5.7 Terrain Shapes Route Cost |
| **Core Loop Integration** | [03_core_loop.md](03_core_loop.md) | §3.5 |

---

## Key Principles (Summary)

| Principle | Description |
|-----------|-------------|
| **Building → Checking** | A-stories alternate stat growth and stat tests (Sir Brante rhythm) |
| **B = Earned Reward** | B-stories spawn when player succeeds at hard A-story checks |
| **C = Natural Texture** | C-stories emerge from journey—not spawned, experienced |
| **Narrative Continuity** | B-stories continue A-story threads with same characters/locations |
| **Travel Cost Gate** | Distance creates resource demand; B-story rewards fund travel |
| **Terrain Variety** | Route choice = impossible choice (time vs stamina vs coins vs encounters) |

---

## Code References

| Component | Values |
|-----------|--------|
| StoryCategory | MainStory, SideStory, Encounter |

---

## Open Questions

- B-story spawn mechanics (A-story success → B-story creation technically)
- Narrative continuity (same characters/locations carrying forward)
- Terrain-aware C-story archetype selection
- Multiple route alternatives
