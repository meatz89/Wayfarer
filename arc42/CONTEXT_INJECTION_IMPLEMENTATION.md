# Context Injection Implementation

## What We're Implementing

**arc42 §8.28 Context Injection Pattern** - A fundamental architectural refactoring that establishes clear separation between scene selection and choice scaling.

---

## The Problem

The codebase had three intertwined concepts for content difficulty:

1. **Tier** (int 0-4) - Legacy property on SceneTemplate, NPC, Location, Region, Situation
2. **LocationSafety/LocationPurpose** - Categorical enums influencing selection
3. **RhythmPattern** - Sir Brante narrative rhythm (Building/Crisis/Mixed)

These were conflated in scene selection logic, causing:
- Unpredictable scene generation
- Tight coupling between unrelated concerns
- Inability to author tutorial content with guaranteed behavior

---

## The Solution: Scene = Arc Model

### Core Principle

Each A-Story **scene IS an arc**. The arc structure is defined by the SceneTemplate:

| Position | Structure | Purpose |
|----------|-----------|---------|
| **Situations 1 to N-1** | Building | Stat growth, investment, narrative setup |
| **Situation N (final)** | Crisis | Stat-gated choices test accumulated investment |

### Two Independent Systems

| System | Purpose | Input | Applies To |
|--------|---------|-------|------------|
| **Template Selection** | WHICH SceneTemplate to use | Intensity history + anti-repetition | Template choice |
| **Choice Scaling** | HOW difficult the choices are | Location.Difficulty | Stat thresholds, resource costs |

These systems are **ORTHOGONAL** - they never interact. The same template can produce different difficulty experiences based on location.

### Context Injection Principle

**Context must be SET on SceneSpawnReward at CREATION time, never derived at consumption time.**

| Content Type | When Context Set | By Whom |
|--------------|------------------|---------|
| **Authored (A1-A10)** | JSON authoring | Content author |
| **Procedural (A11+)** | Spawn reward activation | RewardApplicationService |

The same generation code handles both. No `if (isAuthored)` branching.

---

## What's Being Removed

### Tier Property (Legacy)

Tier was the old representation of difficulty. It existed on:
- SceneTemplate
- Location (now replaced by Location.Difficulty)
- NPC
- Region
- Situation
- Various DTOs

**Replaced by:** Location.Difficulty (hex distance from world center / 5)

### HasAuthoredContext Pattern

Code like `if (spawnReward.HasAuthoredContext)` violated the HIGHLANDER principle by branching on content origin.

**Replaced by:** EnsureContextSet() - computes and SETS context on spawn reward if not already authored

### LocationSafety/LocationPurpose in Selection

These categorical properties influenced scene selection, conflating location properties with narrative structure.

**Replaced by:** Pure intensity-history-based template selection. Location properties used only for choice scaling.

---

## Net Challenge Formula

The relationship between player capability and world position:

```
NetChallenge = LocationDifficulty - (PlayerStrength / 5)
```

Where:
- **LocationDifficulty** = hex distance from (0,0) / 5
- **PlayerStrength** = sum of all five stats

Clamped to [-3, +3] and applied to ALL stat requirements via RuntimeScalingContext.

---

## Building → Crisis Structure (Sir Brante Pattern)

The narrative rhythm borrowed from The Life and Suffering of Sir Brante operates at the **situation level** within each scene:

| Position | Structure | Choice Generation |
|----------|-----------|-------------------|
| **1 to N-1** | Building | Choices GRANT stats (investment phase) |
| **N (final)** | Crisis | Stat-gated choices TEST investment |

### Rhythm Through Situation Structure

Rhythm is embodied in the SituationTemplate's ChoiceTemplates:

| Input | Effect |
|-------|--------|
| **Position in scene** | Determines choice structure at generation time |
| **ArchetypeIntensity** | Recovery/Standard/Demanding (difficulty scaling) |

Archetypes generate ChoiceTemplates appropriate to position:
- Earlier situations → Building choices (stat grants)
- Final situation → Crisis choices (stat tests)

The rhythm is baked into the choice definitions—no RhythmPattern property needed.

**Implementation note:** Migration tracked in `gdd/IMPLEMENTATION_PLAN_STORY_SYSTEM.md` Phase 3.

---

## Implementation Order

1. **Scene = Arc structure** - Each scene contains Building situations + final Crisis
2. **Remove HasAuthoredContext** - Forbidden by arc42 §8.28
3. **Remove Tier from entities** - Replace with Location.Difficulty
4. **Update derivation methods** - Use Location.Difficulty for Quality/Environment
5. **Update DTOs and parsers** - Remove Tier parsing
6. **Verify Location.Difficulty usage** - Ensure consistent choice scaling

---

## Why This Matters

### For Content Authors
- Tutorial scenes (A1-A10) behave exactly as authored
- Procedural scenes (A11+) follow predictable arc structure
- SceneTemplate defines arc length and structure

### For Code Maintainers
- HIGHLANDER: One code path for all content
- Clear separation of concerns (story category → structure, location → scaling)
- Fail-fast on missing context

### For Players
- Consistent Building → Crisis narrative pacing within each scene
- Difficulty scales with exploration distance
- Same arc structure feels appropriately tight at all locations

---

## See Also

- `arc42/08_crosscutting_concepts.md` §8.25 (Context Injection Pattern)
- `gdd/08_glossary.md` (A-Story Building → Crisis Rhythm)
- `gdd/06_balance.md` §6.4 (Scene = Arc Structure)
