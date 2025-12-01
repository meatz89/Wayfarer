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

## The Solution: Orthogonal Systems

### Two Independent Dimensions

| System | Purpose | Input | Applies To |
|--------|---------|-------|------------|
| **Scene Selection** | WHICH scene template to use | RhythmPattern ONLY | Template selection, archetype choice |
| **Choice Scaling** | HOW difficult the choices are | Location.Difficulty | Stat thresholds, resource costs |

These systems are **ORTHOGONAL** - they never interact. A scene can be:
- Building rhythm + High difficulty (gentle narrative, hard stats)
- Crisis rhythm + Low difficulty (intense narrative, easy stats)

### Context Injection Principle

**RhythmPattern must be SET on SceneSpawnReward at CREATION time, never derived at consumption time.**

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

**Replaced by:** EnsureRhythmPatternSet() - computes and SETS on spawn reward if not already authored

### LocationSafety/LocationPurpose in Selection

These categorical properties influenced scene selection, conflating location properties with narrative rhythm.

**Replaced by:** Pure RhythmPattern selection. Location properties used only for choice scaling.

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

## Sir Brante Rhythm Pattern

The narrative rhythm cycle borrowed from The Life and Suffering of Sir Brante:

| Pattern | Player Experience | Choice Structure |
|---------|-------------------|------------------|
| **Building** | Accumulation, growth | All positive outcomes |
| **Crisis** | Test, high stakes | Damage mitigation, fallback has penalty |
| **Mixed** | Recovery, trade-offs | Standard choices |

This drives WHICH archetypes are selected, not how hard they are.

---

## Implementation Order

1. **EnsureRhythmPatternSet()** - SET context at creation, never derive at consumption
2. **Remove HasAuthoredContext** - Forbidden by arc42 §8.28
3. **Remove Tier from entities** - Replace with Location.Difficulty
4. **Update derivation methods** - Use Location.Difficulty for Quality/Environment
5. **Update DTOs and parsers** - Remove Tier parsing
6. **Verify Location.Difficulty usage** - Ensure consistent choice scaling

---

## Why This Matters

### For Content Authors
- Tutorial scenes (A1-A10) behave exactly as authored
- Procedural scenes (A11+) follow predictable rhythm
- No hidden state derivation affecting behavior

### For Code Maintainers
- HIGHLANDER: One code path for all content
- Clear separation of concerns
- Fail-fast on missing context

### For Players
- Consistent narrative pacing
- Difficulty scales with exploration distance
- Same rhythm feels equally tight at all locations

---

## See Also

- `arc42/08_crosscutting_concepts.md` §8.28 (Context Injection)
- `arc42/08_crosscutting_concepts.md` §8.26 (Sir Brante Rhythm Pattern)
- `gdd/06_balance.md` §6.4 (Scene Selection via RhythmPattern)
- `gdd/06_balance.md` §6.5 (Net Challenge Formula)
