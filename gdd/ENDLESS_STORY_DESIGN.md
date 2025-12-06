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
| Player agency | A and C are **mandatory**; only B is **opt-in** |
| Economic loop | A=sink, B=source, C=texture, Atmospheric Work=fallback |
| B-Story structure | One scene with 3-8 situations (not multiple linked scenes) |
| C-Story spawn | Probabilistic based on location, player state, world state |
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
| Player Agency | Mandatory | Opt-in | Mandatory |
| Spawn Trigger | Previous A-scene | Player accepts | Probabilistic |

### Implementation Notes

| Component | Enum Values / Purpose |
|-----------|----------------------|
| StoryCategory | MainStory, SideStory, Encounter |
| Route generation | Filters C-story templates by category |
| Scene visualization | Uses category for node styling |

---

## The Economic Loop

```
A-Story N completes
    ↓
A-Story N+1 spawns at distance N+1
    ↓
Travel costs exceed resources
    ↓
Player seeks B-Stories (quests)
    ↓
Resources accumulate
    ↓
Afford travel to A-Story N+1
    ↓
C-story encounters during travel
    ↓
Repeat forever
```

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

- B-story opportunity generation (what creates job board listings?)
- Terrain-aware C-story archetype selection in code
- Multiple route alternatives (A* currently finds single path)
