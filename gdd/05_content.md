# 5. Content Structure

## Why This Document Exists

This document explains HOW content is organized and WHY the archetype-based system enables infinite balanced content. The separation of mechanical pattern from narrative flavor is the key insight.

---

## 5.1 Narrative Hierarchy: A/B/C Stories

### A-Story: The Infinite Main Thread

The primary narrative spine that never ends:

**Phase 1: Tutorial Instantiation (A1-A10)**
- Uses the SAME selection logic as procedural content
- Tutorial scenes emerge from authored RhythmPattern (not overrides)
- SceneSpawnReward specifies: RhythmPattern only
- Selection logic processes this identically to procedural
- 30-60 minutes of guided introduction

**Phase 2: Procedural Continuation (A11+)**
- Uses the SAME selection logic as tutorial
- RhythmPattern computed from intensity history
- Anti-repetition filters recent categories/archetypes
- Never resolves, always deepens

**Critical Principle (HIGHLANDER):** Tutorial and procedural content flow through IDENTICAL selection logic. The ONLY difference is RhythmPattern source:

| Content | RhythmPattern Source | Selection Logic |
|---------|---------------------|-----------------|
| Tutorial | Authored in SceneSpawnReward | Same |
| Procedural | Computed from intensity history | Same |

Tutorial A1 produces a Social scene NOT because of a "TargetCategory=Social" override, but because its authored RhythmPattern (Building) naturally flows through selection logic to produce Social. The logic doesn't know it's tutorial—it just processes RhythmPattern.

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

### Archetype Reusability Principle

**Every archetype must work in ANY context through categorical scaling.**

| Context | Same Archetype Produces |
|---------|------------------------|
| Tutorial (Tier 0, Friendly, Basic) | Easy requirements, low costs, modest rewards |
| Mid-game (Tier 2, Neutral, Standard) | Medium requirements, balanced costs/rewards |
| Late-game (Tier 3, Hostile, Premium) | Hard requirements, high costs, high rewards |

**Forbidden:**
- Checking story sequence (`AStorySequence == N`) in archetypes
- Different code paths for tutorial vs non-tutorial
- Hardcoded values that don't scale with context

**Required:**
- All difficulty scaling via categorical properties (NPCDemeanor, Quality, PowerDynamic)
- Same four-choice structure regardless of context
- Context-agnostic archetype code

---

## 5.4 Categorical Property Scaling

### How Context Affects Difficulty

Properties on entities modify base archetype values:

**NPCDemeanor:** Friendly (reduced costs) → Neutral (base costs) → Suspicious (increased costs) → Hostile (heavily increased costs)

**Quality:** Poor (reduced costs) → Standard (base costs) → Fine (increased costs) → Exceptional (heavily increased costs)

**PowerDynamic:** Subordinate (reduced costs) → Equal (base costs) → Superior (increased costs) → Authority (heavily increased costs)

### Why This Enables AI Content

AI authors can write compelling narrative without knowing game state:
1. AI writes descriptive properties ("suspicious innkeeper", "fine quality inn")
2. Catalog translates properties to absolute adjustments
3. Archetype applies adjustments to base costs
4. Result: Balanced content without global knowledge

This approach implements DDR-007 (Intentional Numeric Design): properties affect costs through absolute additions and subtractions, never through multipliers or percentages. This ensures players can mentally calculate final costs and maintain transparency in game math.

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

## 5.6 Story Category Rules

### A-Story Guarantees

A-Story scenes have strict requirements to prevent soft-locks:

| Guarantee | Enforcement |
|-----------|-------------|
| **Fallback exists** | Every situation has at least one choice with NO requirements |
| **Progression assured** | ALL final situation choices spawn next A-scene |
| **World expansion** | Scenes create new venues/districts/regions/routes/NPCs |

These guarantees are enforced at parse-time via **Centralized Invariant Enforcement**. The parser—not individual archetypes—applies category-wide rules. This ensures no archetype can accidentally omit an invariant.

See [arc42/08_crosscutting_concepts.md §8.18](../arc42/08_crosscutting_concepts.md) and [ADR-016](../arc42/09_architecture_decisions.md) for technical details.

**Why A-Story is special:** The Frieren principle—infinite, never-ending. Primary purpose is world expansion, creating new places to explore, new people to meet. Player must ALWAYS be able to progress.

### Scene Instance Creation (Context Injection)

**Two concepts:**

| Concept | What It Is | Example |
|---------|------------|---------|
| **SceneTemplate** | Pure archetype from catalog | InnLodging, SeekAudience, DeliveryContract |
| **Scene** | Runtime instance (Situations from SituationTemplates) | Playable game state |

**SceneDTO** serves both creation and runtime. Both authored and procedural content use the same DTO:
- `sceneArchetype`: Reference to SceneTemplate (InnLodging, etc.)
- `tier`, `rhythmPattern`, `locationSafety`, `locationPurpose`: Complete non-nullable context
- `mainStorySequence`, `isStarter`: A-story progression
- `state`, `currentSituationId`: Runtime state (populated during play)

| Path | DTO Source | Context Source |
|------|------------|----------------|
| **Authored (A1-A10)** | JSON → SceneDTO | Pre-defined in JSON |
| **Procedural (A11+)** | Code → SceneDTO | Derived from GameWorld |

Parser receives SceneDTO and produces Scene. Parser has no knowledge of source.

See [arc42/08_crosscutting_concepts.md §8.28](../arc42/08_crosscutting_concepts.md) for implementation pattern.

### B/C Story Flexibility

B and C stories have relaxed rules:

- Can require stats, resources, or completed prerequisites
- Can include challenge paths without fallbacks
- Can fail (scene marked Failed, not Completed)
- Focus on narrative depth, not world expansion

| | A-Story | B/C Stories |
|-|---------|-------------|
| Purpose | World expansion | Narrative depth |
| Typical scope | New venues, districts, regions | Existing venues |
| Can fail? | Never | Yes |
| Fallback required? | Yes | No |
| Requirements allowed? | Limited (fallback must exist) | Unlimited |

---

## 5.7 Scene Progression

Situations are ordered within scenes. Player experiences them sequentially.

### Flow

1. Scene activates → Situation Instances created from templates
2. Scene starts at first Situation Instance
3. Player completes situation (selects choice)
4. Scene index advances to next Situation Instance
5. Player returns to location view
6. Player navigates to next situation's location
7. Scene resumes when player enters matching location
8. Repeat until final situation complete
9. Scene marked Completed, spawn rewards applied

**Multi-location scenes:** Player must travel between situation locations. Scene waits for player to arrive at correct location before resuming.

---

## 5.8 Situation Presentation Patterns

How situations appear depends on which entities they reference. Three distinct patterns exist.

### Pattern 1: Location-Only (Modal)

When situation has Location but NO NPC:

1. Player enters location matching situation's Location
2. **IMMEDIATE:** Modal appears with situation choices
3. Player MUST select a choice (no escape)
4. After selection: costs/rewards applied, return to location view

**Feel:** Sir Brante style. Enter a place, confronted with decision. Cannot leave until resolved.

### Pattern 2: Location + NPC (Conversation Entry)

When situation has BOTH Location AND NPC:

1. Player enters location matching situation's Location
2. Player uses "Look Around" action
3. NPCs at location displayed
4. For each NPC with active situation: conversation option appears
5. Player clicks conversation option (voluntary entry)
6. Modal appears with situation choices (mandatory decision)
7. After selection: costs/rewards applied, return to location view

**Feel:** You spot someone. You can approach them or not. Once you engage, you must see it through.

### Pattern 3: Route Segments (Travel)

When situation has Route:

1. Player initiates travel on route
2. Route divided into segments (each segment = one situation)
3. At each segment: modal appears with situation choices
4. Player MUST select a choice (no turning back mid-journey)
5. After selection: costs/rewards applied, continue to next segment
6. After final segment: arrive at destination

**Feel:** Journey encounters. The road presents challenges. You handle each as it comes.

### Filter Determines Presentation

| LocationFilter | NpcFilter | RouteFilter | Presentation |
|----------------|-----------|-------------|--------------|
| Set | null | null | Location modal (immediate) |
| Set | Set | null | NPC conversation (Look Around → click → modal) |
| Set | null | Set | Route segment (during travel) |

LocationFilter is always required. NpcFilter and RouteFilter determine presentation mode.

### Mandatory Decision Principle

Once a situation is entered (by any path), player MUST make a choice. No backing out. This creates weight—approaching an NPC or entering a location has consequences.

The four-choice archetype guarantees at least one choice is always available (fallback), so player is never stuck.

---

## 5.9 Choice Patterns by Category

### A-Story: Fallback Required

Every A-Story situation MUST have at least one choice with NO requirements:

| Choice Type | Requirement | Cost | Outcome |
|-------------|-------------|------|---------|
| Stat-gated | PrimaryStat ≥ threshold | None | Best |
| Money-gated | None | Coins | Good |
| Challenge | None | Resolve | Variable |
| **Fallback** | **None** | Time | Worst |

The fallback guarantees forward progress. A-Story validation rejects situations without one.

### B/C Stories: Flexible

B and C stories can have any choice structure:

- All choices can have requirements
- Challenge-only situations allowed
- Failure states permitted
- No mandatory fallback

This enables narrative tension, gating, and consequences that A-Story cannot have.

---

## 5.10 Text Generation Rules

### Two-Phase Creation Model

Entity creation happens in two phases. Names and descriptions generated in Phase 2 persist for the remainder of the game.

| Phase | What Happens | Output |
|-------|--------------|--------|
| **1. Mechanical** | Structure, references, categorical properties | Generic identifiers |
| **2. Narrative** | AI examines complete context | PERSISTENT names stored on entities |

### NO PLACEHOLDERS

Placeholder syntax like `{NPCName}`, `{LocationName}` is FORBIDDEN in templates.

| Approach | When to Use |
|----------|-------------|
| **Generic text** | Works standalone without substitution |
| **Null** | AI generates contextually appropriate text in Phase 2 |

**Rationale:** Phase 2 (narrative finalization) generates all contextual text after mechanical creation completes. Templates contain generic text or null—never placeholders. Generated names persist and display consistently throughout the game.

**See also:** arc42 §8.27 for technical details on two-phase creation.

---

## Cross-References

- **Technical Architecture**: See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) for Dual-Tier Action system, Catalogue Pattern, and Template vs Instance Lifecycle
- **Core Loop**: See [03_core_loop.md](03_core_loop.md) for how situations fit into the SHORT loop
