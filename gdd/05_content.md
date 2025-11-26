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

## 5.6 Story Category Rules

### A-Story Guarantees

A-Story scenes have strict requirements to prevent soft-locks:

| Guarantee | Enforcement |
|-----------|-------------|
| **Fallback exists** | Every situation has at least one choice with NO requirements |
| **Progression assured** | ALL final situation choices spawn next A-scene |
| **World expansion** | Scenes create new venues/districts/regions/routes/NPCs |

These guarantees are enforced at parse-time. A-Story scenes that violate them fail validation.

**Why A-Story is special:** The Frieren principle—infinite, never-ending. Primary purpose is world expansion, creating new places to explore, new people to meet. Player must ALWAYS be able to progress.

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

### NO PLACEHOLDERS

Placeholder syntax like `{NPCName}`, `{LocationName}` is FORBIDDEN in templates.

| Approach | When to Use |
|----------|-------------|
| **Generic text** | Works standalone without substitution |
| **Null** | AI generates contextually appropriate text |

**Rationale:** AI text generation pass will replace all narrative content. Templates should contain either generic text that works standalone, or null for full AI generation.

---

## Cross-References

- **Technical Architecture**: See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) for Dual-Tier Action system, Catalogue Pattern, and Template vs Instance Lifecycle
- **Core Loop**: See [03_core_loop.md](03_core_loop.md) for how situations fit into the SHORT loop
