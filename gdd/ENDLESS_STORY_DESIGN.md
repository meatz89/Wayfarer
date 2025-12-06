# Endless Story Design - Master Reference

Quick-start document for the infinite A-Story system. Points to all relevant documentation created in this design session.

---

## The Core Problem

A-Story must never soft-lock (fallback always exists), yet players must not mindlessly click through without engagement. Solution: **Travel Cost Gate**.

---

## Key Design Decisions

| Decision | Outcome |
|----------|---------|
| A/B/C categories | Distinguished by **combination of 8 properties**, not single axis |
| A-Story rhythm | **Building → Checking** phases (Sir Brante pattern) |
| B-Story spawn | **Reward for A-story success** at hard stat checks |
| B-Story narrative | Continues A-story thread with **same characters/locations** |
| C-Story emergence | **Natural consequence** of journey (travel, locations, NPCs) |
| Story causality | A creates B (success) and C (journey); not independent systems |
| Terrain | Affects both travel cost AND encounter themes |

---

## Documentation Map

### Primary References

| Topic | Location | Section |
|-------|----------|---------|
| **Story Category Property Matrix** | [gdd/08_glossary.md](08_glossary.md) | §Story Categories |
| **Travel Cost Gate Design** | [gdd/05_content.md](05_content.md) | §5.7 |
| **Core Loop Integration** | [gdd/03_core_loop.md](03_core_loop.md) | §3.5 |
| **Technical Glossary** | [arc42/12_glossary.md](../arc42/12_glossary.md) | §Domain Terms |
| **Content Architecture** | [CONTENT_ARCHITECTURE.md](../CONTENT_ARCHITECTURE.md) | §2 |

### Property Matrix (Canonical Definition)

| Property | A-Story | B-Story | C-Story |
|----------|---------|---------|---------|
| Structure | Infinite scene chain | One scene, 3-8 situations | One scene, 1-2 situations |
| Repeatability | One-time (sequential) | One-time (completable) | System-repeatable |
| Fallback Required | Yes | No | No |
| Can Fail | Never | Yes | Yes |
| Resource Flow | Sink | Source (significant) | Texture (minor) |
| Scope | World expansion | Venue depth | Location flavor |
| Player Agency | Mandatory | Earned, then opt-in | Mandatory |
| Spawn Trigger | Previous A-scene | A-story stat check success | Natural journey emergence |

### Implementation Notes

| Component | Enum Values / Purpose |
|-----------|----------------------|
| StoryCategory | MainStory, SideStory, Encounter |
| Route generation | Filters C-story templates by category |
| Scene visualization | Uses category for node styling |

---

## Story Causality

```
A-Story (Building → Checking rhythm)
    │
    ├── SUCCEED at hard stat check
    │       ↓
    │   B-Story spawns (reward thread)
    │       └── Continues narrative with same characters/locations
    │       └── Great rewards fund travel
    │
    └── JOURNEY to A-Story location
            ↓
        C-Stories emerge naturally
            └── Route encounters (terrain-themed)
            └── Location flavor
            └── NPC interactions
```

**The economic consequence:** Mastery → B-stories → Rewards → Easier travel. Fallback path works but slower.

---

## Terrain System

**Cost:** Road (0) < Plains/Water (1) < Forest (2) < Mountains/Swamp (3)

**Encounter Themes:** Each terrain spawns thematically appropriate C-stories (forest=ambush, road=trade, etc.)

**Route Choice = Impossible Choice:** Cannot optimize time, stamina, coins, and encounters simultaneously.

See [gdd/05_content.md §5.7](05_content.md) "Terrain Shapes Route Cost and Encounters" for full tables.

---

## Commits in This Design Session

1. `55739a1` - Travel Cost Gate design
2. `333139f` - A/B/C property matrix
3. `4154f3c` - Player agency clarification
4. `688ba9f` - Remove concrete numbers (doc purity)
5. `3284229` - B/C mechanics, rename Service→Encounter
6. `65e8e96` - Terrain-based route design

---

## Open Questions (For Future Sessions)

- B-story spawn mechanics (how does A-story success technically trigger B-story creation?)
- B-story narrative continuity (how to ensure same characters/locations carry forward?)
- Terrain-aware C-story archetype selection in code
- Multiple route alternatives (A* currently finds single path)
