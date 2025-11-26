# 5. Content Structure

## Why This Document Exists

This document explains HOW content is organized and WHY the archetype-based system enables infinite balanced content. The separation of mechanical pattern from narrative flavor is the key insight.

---

## 5.1 Narrative Hierarchy: A/B/C Stories

### A-Story: The Infinite Main Thread

The primary narrative spine that never ends:

**Phase 1: Authored Tutorial (A1-A10)**
- Hand-crafted scenes teaching mechanics
- Fixed sequence establishing gameplay patterns
- 30-60 minutes of guided introduction

**Phase 2: Procedural Continuation (A11+)**
- Scene archetypes selected from catalog
- AI narrative generation connecting to player history
- Escalating scope over time (local → regional → continental)
- Never resolves, always deepens

**Why infinite:** Eliminates ending pressure. No post-game awkwardness. Player chooses when to engage. The journey IS the destination.

### B-Stories: Major Side Content

Substantial optional narrative threads:
- 3-8 scenes per B-story
- Character arcs, faction storylines, thematic exploration
- Run parallel to A-story
- Player-initiated engagement

### C-Stories: Minor Side Content

Small narrative moments:
- 1-2 scenes typically
- World flavor, quick opportunities
- Procedurally generated more easily
- Organic encounters during travel

---

## 5.2 Scene-Situation-Choice Flow

All narrative content follows the same structure:

```
Scene (Container)
├── Situation 1
│   ├── Choice A (4 paths)
│   ├── Choice B (4 paths)
│   └── ...
├── Situation 2
│   └── ...
└── Situation N (Resolution)
```

**Scene:** Complete narrative arc with beginning, development, resolution
**Situation:** Decision point within scene presenting choices
**Choice:** The four-choice archetype (stat-gated, resource, challenge, fallback)

---

## 5.3 The Archetype System

### The Core Insight

Separate reusable mechanical patterns from narrative content. Same archetype + different entity properties = infinite contextually-appropriate variations.

### What an Archetype Defines

**Structure:**
- Number of choices (typically 4)
- Path types per choice
- Base cost formulas
- Base reward formulas

**What an Archetype Does NOT Define:**
- Narrative text (generated from context)
- Specific entity IDs (uses categorical filters)
- Absolute numeric values (scaled by properties)

### Example: Negotiation Archetype

The same "negotiation" archetype works for:
- Securing lodging from innkeeper
- Gaining passage through checkpoint
- Acquiring information from scholar
- Purchasing goods from merchant
- Requesting healing from priest

Entity properties (NPC demeanor, location quality, power dynamic) scale difficulty. Same mechanical structure, contextually appropriate experience.

### Mathematical Variety

21 situation archetypes × 4 NPC demeanors × 4 quality tiers × 3 power dynamics = **1,008 mechanically distinct variations** from finite patterns.

---

## 5.4 Categorical Property Scaling

### How Context Affects Difficulty

Properties on entities modify base archetype values:

**NPCDemeanor:** Friendly (0.8x costs) → Neutral (1.0x) → Suspicious (1.2x) → Hostile (1.5x)

**Quality:** Poor (0.7x) → Standard (1.0x) → Fine (1.3x) → Exceptional (1.6x)

**PowerDynamic:** Subordinate (0.8x) → Equal (1.0x) → Superior (1.2x) → Authority (1.5x)

### Why This Enables AI Content

AI authors can write compelling narrative without knowing game state:
1. AI writes descriptive properties ("suspicious innkeeper", "fine quality inn")
2. Catalog translates properties to multipliers
3. Archetype applies multipliers to base costs
4. Result: Balanced content without global knowledge

---

## 5.5 The Atmospheric Action Layer

### Always-Available Actions

Regardless of scene state, players can always:
- **Travel** — Initiate route to another venue, earn delivery coins
- **Work** — Perform odd jobs for immediate income
- **Rest** — Consume resources to restore pools
- **Move** — Navigate between adjacent locations

**Why this matters:** Scene-based content layers ON TOP of atmospheric actions. When scenes complete, atmospheric actions remain. Player is NEVER trapped with "nothing to do."

### Scene-Based Actions

Temporary actions available only while scenes are active:
- Appear when scenes spawn
- Present narrative choices
- Disappear when scenes complete
- Layer alongside atmospheric actions, not replacing them

---

## Cross-References

- **Technical Architecture**: See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) for Dual-Tier Action system and Catalogue Pattern
