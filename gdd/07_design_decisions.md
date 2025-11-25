# 7. Design Decisions

## Why This Document Exists

This document records major design decisions (DDRs) that shape Wayfarer's player experience. Each decision explains the PROBLEM, the CHOICE made, and WHY alternatives were rejected. For exhaustive DDR documentation, see the reference materials.

---

## DDR-001: Infinite Procedural A-Story

### Problem
Traditional RPGs face ending awkwardness: "You saved the world, now go collect flowers." Post-ending gameplay feels hollow. Most players leave when the story ends.

### Decision
**The game never ends.** The A-story is an infinite procedurally-generated spine.

### Why This Over Alternatives

| Option | Rejected Because |
|--------|------------------|
| Traditional ending + post-game | Post-ending content feels disconnected |
| Multiple authored endings | Most players see one ending; 80% content wasted |
| No main story | Players need structure and direction |

### Consequences
- Eliminates ending pressure
- Enables player-paced engagement
- Requires robust procedural generation
- Matches "eternal traveler" fantasy

---

## DDR-002: Four-Choice Archetype (Guaranteed Progression)

### Problem
How to ensure players can ALWAYS progress (no soft-locks) while maintaining meaningful choice?

### Decision
Every A-story situation presents exactly four path types with orthogonal costs. At least one path is ALWAYS available.

### The Pattern
1. **Stat-Gated:** Free for specialists
2. **Resource:** Available to anyone with coins/items
3. **Challenge:** Risk/reward for skilled players
4. **Fallback:** Always available, higher cost

### Why This Over Alternatives

| Option | Rejected Because |
|--------|------------------|
| Binary pass/fail checks | Creates hard gates, punishes builds |
| Unlimited choices | Decision paralysis, balance impossible |
| Three choices | Insufficient path diversity |

### Consequences
- No soft-locks ever
- All builds viable
- Orthogonal costs prevent single "best" path
- Predictable content structure enables procedural generation

---

## DDR-003: Unified Five-Stat System

### Problem
How to create meaningful character differentiation without overwhelming complexity?

### Decision
Five stats (Insight, Cunning, Rapport, Diplomacy, Authority) govern all capability. Limited total points force specialization.

### Why Five Stats

| Count | Problem |
|-------|---------|
| 3 stats | Insufficient differentiation |
| 10+ stats | Overwhelming, micro-optimization |
| 5 stats | Sweet spot for meaningful builds |

### Consequences
- Clear build identities emerge
- Every stat matters for some content
- Specialization creates both capability and vulnerability
- Stats map cleanly to three challenge systems

---

## DDR-004: Strategic-Tactical Layer Separation

### Problem
How to balance perfect information (strategy) with execution depth (tactics)?

### Decision
Separate into two explicit layers with clear bridge.

**Strategic Layer:** All information visible. Player decides WHETHER to attempt.
**Tactical Layer:** Hidden complexity. Player demonstrates HOW to execute.

### Why Separation

Without separation:
- Hidden information frustrates strategic planning
- Visible information eliminates tactical surprise
- Hybrid systems satisfy neither goal

### Consequences
- Strategic choices are calculable
- Tactical challenges test skill
- Clear mental model for players
- Distinct design spaces for each layer

---

## DDR-005: Earned Scarcity (Tight Economic Margins)

### Problem
How to maintain strategic depth throughout progression without artificial difficulty spikes?

### Decision
Economic margins are deliberately tight at ALL progression stages. Delivery profits barely cover costs. Efficiency IS the reward for mastery.

### Why This Over Alternatives

| Option | Rejected Because |
|--------|------------------|
| Generous economy | Removes resource decisions, trivializes choices |
| Scaling difficulty spikes | Feels artificial, frustrating |
| Unlock-based gates | Contradicts player agency |

### Consequences
- Optimization always matters
- Resources never become trivial
- Expert players complete content more efficiently, not with more power
- Matches "struggling traveler" fantasy

---

## DDR-006: Atmospheric Action Layer

### Problem
How to prevent dead ends where players have "nothing to do"?

### Decision
Core actions (Travel, Work, Rest, Move) are ALWAYS available, independent of scene state. Scene-based narrative actions layer ON TOP, never replacing atmospheric baseline.

### Why This Matters

Without atmospheric layer:
- Scene completion could leave player stranded
- Bugs in scene logic = soft-lock
- No guaranteed forward progress

### Consequences
- Player always has options
- Scene failures don't break game
- Clear separation of persistent vs temporary content
- Simplifies QA (atmospheric layer is safety net)

---

## Cross-References

- **Complete DDR Documentation**: See [design/11_design_decisions.md](../design/11_design_decisions.md) for all DDRs
- **Related ADRs**: See [arc42/09_architecture_decisions.md](../arc42/09_architecture_decisions.md) for technical decisions
