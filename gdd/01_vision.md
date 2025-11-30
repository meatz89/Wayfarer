# 1. Vision and Design Pillars

## Why This Document Exists

This document defines WHAT Wayfarer is and WHY it works. Every design decision flows from these pillars. When evaluating a feature, mechanic, or content piece, ask: "Does this reinforce our pillars, or undermine them?"

---

## 1.1 Core Experience Statement

**Wayfarer creates strategic depth through impossible choices, not mechanical complexity.**

Every decision forces the player to choose between multiple suboptimal paths, revealing character through constraint. The game is not about winning—it's about deciding what you're willing to sacrifice to achieve what matters most.

### The Essential Experience

You are a traveler in a low-fantasy world managing scarce resources across an unforgiving landscape. You accept delivery jobs to earn coins, navigate dangerous routes where every encounter demands difficult choices, and return barely able to afford food and shelter.

The core feeling: **"I can afford A OR B, but not both."**

This is not a puzzle. There is no correct answer. Your character emerges from what you sacrifice along the way.

---

## 1.2 Player Fantasy

### You Are: A Low-Fantasy Traveler

Not a hero. Not chosen. Not destined. You survive through:
- **Optimization skill** — Learning routes, spotting opportunities, managing resources efficiently
- **Social intelligence** — Building relationships that provide advantages
- **Strategic planning** — Deciding what to pursue and what to sacrifice
- **Tactical execution** — Succeeding in card-based challenges when necessary

### The World: Grounded Historical-Fantasy

Low-magic setting where danger comes from terrain, weather, bandits, and human nature. Magic exists but is rare and never your primary tool. All systems mirror real-world dynamics—relationships deplete when used, resources are genuinely scarce, the world doesn't care about your success.

### The Journey: Infinite and Personal

No ending. The road goes on forever. Your story emerges through resource priorities, relationship patterns, build specialization, and moments where you reveal character through constraint.

---

## 1.3 Design Pillars Explained

### Pillar 1: Impossible Choices

**Why:** Strategic depth without mechanical complexity. When resources are scarce and all options have genuine costs, decision-making becomes the game—not memorizing combos or grinding stats.

**How it manifests:**
- Orthogonal resource costs (time vs coins vs energy vs relationships)
- Four-choice archetype ensuring multiple valid approaches
- Tight economic margins forcing continuous prioritization
- Specialization creating capability AND vulnerability

### Pillar 2: Perfect Information

**Why:** Strategy requires predictability. Hidden gotchas create frustration, not depth. When players can calculate costs and plan approaches, skill expression becomes meaningful.

**How it manifests:**
- All costs, requirements, and rewards visible before selection
- Challenge outcomes predictable from player stats and card quality
- No random "you lose" events without player agency
- Strategic layer shows everything; tactical layer tests execution

Extended by DDR-007 (Intentional Numeric Design), which ensures the math itself is transparent and calculable.

### Pillar 3: Infinite Journey (Frieren Principle)

**Why:** Eliminates the hardest design problem—satisfying endings. Removes pressure to rush. Creates permission to engage at your own pace. The A-story waits.

**How it manifests:**
- Main narrative never resolves—you travel, arrive, meet people, move on
- Procedural content generation for infinite A-story continuation
- No "post-game awkwardness" of having saved the world but still collecting herbs
- Player chooses when to stop, not the game

### Pillar 4: Earned Scarcity

**Why:** Tight margins create tension. When delivery earnings barely cover survival, every efficiency gain feels earned. Mastery means optimization, not power accumulation.

**How it manifests:**
- Delivery profits deliberately minimal
- Rest, healing, and services consume significant resources
- Equipment provides incremental advantages, not transformative power
- Late-game challenges remain genuinely challenging

### Pillar 5: Character Through Constraint (Sir Brante Model)

**Why:** Identity emerges from sacrifice, not accumulation. Limited stat points force specialization. Helping one person means disappointing another. Your build reflects your values.

**Reference Model:** "The Life and Suffering of Sir Brante" demonstrates this principle masterfully—every stat point matters immediately, creating a rhythm where building stats in one situation enables gates in the next. See gdd/06_balance.md §6.4 for detailed implementation.

**How it manifests:**
- Cannot maximize all stats—must choose specialization
- Relationship investment creates obligations
- Time spent on side content is time not spent on main story
- Choices close doors permanently (and this is not failure)
- Stat-building choices alternate with stat-gated choices (the rhythm)
- Trade-off consequences on ALL paths (gain X, lose Y)
- OR-type requirements enabling multiple valid approaches

---

## 1.4 Anti-Goals: What Wayfarer Is NOT

### Not a Power Fantasy
No exponential growth. Stats improve slowly. Equipment provides incremental advantages. You never become unstoppable. Resource scarcity persists throughout.

### Not a Hero's Journey
You don't save the world. You don't defeat the dark lord. You travel, work, survive, build relationships, and continue traveling. The journey is the point.

### Not a Puzzle With Correct Answers
Multiple valid paths exist. Choosing one means sacrificing another. Optimization is possible but "optimal" depends on what you value.

### Not Artificial Urgency
No ticking clocks forcing rushed decisions. Time pressure comes from opportunity cost (doing X means not doing Y), never arbitrary deadlines.

### Not Guilt Manipulation
The game never punishes you for not engaging with content. NPCs don't die because you ignored them. The world persists, patient.

---

## 1.5 Foundational Principle: Requirement Inversion

**Traditional RPGs:** "You need Level 5 to enter this area" (boolean gate)
**Wayfarer:** "You can enter anytime, but without Insight 4, it costs 20 extra coins" (resource arithmetic)

### Why This Matters

Boolean gates create hard stops. Players hit walls. Content is locked behind arbitrary thresholds. This contradicts our "No Soft-Locks" principle.

Resource arithmetic creates soft pressure. Players can ALWAYS progress—the question is COST. High-stat players pay less. Low-stat players pay more. Everyone moves forward.

### How It Manifests

- **Stat requirements affect COST, not ACCESS**
- **Four-choice archetype ensures fallback path always exists**
- **Challenge difficulty scales rewards AND costs**
- **Content exists from game start; requirements filter optimal paths, not availability**

This is WHY Wayfarer feels different from traditional RPGs. Progression is continuous, not gated.

---

## 1.6 Design Principle Tier Hierarchy

When design principles conflict, resolve by tier priority:

### TIER 1: Non-Negotiable (Never Compromise)
1. **No Soft-Locks** — Player can ALWAYS make forward progress
2. **Single Source of Truth** — One owner per entity type (HIGHLANDER)

### TIER 2: Core Experience (Compromise Only for Tier 1)
3. **Playability Over Compilation** — Inaccessible content is worse than crashes
4. **Perfect Information** — All costs visible before selection
5. **Resource Scarcity Creates Challenge** — Not stat checks

### TIER 3: Architectural Quality (Compromise for Tier 1 or 2)
6. **Elegance** — Simple solutions over complex ones
7. **Verisimilitude** — Systems mirror real-world dynamics

### Conflict Resolution Example

*"Should this choice require Insight 6 to see?"*

- TIER 3 (Verisimilitude): Yes, experts notice things others miss
- TIER 2 (Perfect Information): No, hidden options frustrate planning
- TIER 1 (No Soft-Locks): Choice must exist regardless

**Resolution:** Choice is VISIBLE to everyone. Insight 6 provides FREE path; others pay resource cost. Verisimilitude via cost scaling, not visibility.

---

## 1.7 Emotional Aesthetic Goals

**Primary: Contemplation Through Constraint**
Choices are not timed. You can pause, consider, calculate. Pressure comes from scarcity, not urgency.

**Secondary: Meaningful Consequence**
Every choice has weight. Resources spent are genuinely spent. You live with your decisions.

**Tertiary: Journey Over Destination**
Success is measured by journey quality, not arrival. The game teaches letting go of perfectionism—you cannot have everything.

---

## 1.8 History-Driven Scene Generation

**The game measures PAST, never reacts to PRESENT player state.**

### Core Principle

Scene generation is deterministic based on what HAS HAPPENED, not what the player currently has. Current Resolve, current stats, current resources—none of these influence what scene comes next. The player's current state is their own responsibility.

### Why This Matters

| Anti-Pattern | Why It's Wrong |
|--------------|----------------|
| "Player has low Resolve, give easier scene" | Removes consequences; player learns bad choices are cushioned |
| "Rotate through categories by sequence number" | Arbitrary; ignores actual rhythm of what player experienced |
| "Skip Crisis if player is struggling" | Undermines the tension that makes choices meaningful |

**Correct Pattern:** Measure intensity of past scenes. If player has experienced several demanding situations, the next scene should be recovery. If player has had too much recovery, challenge them. The rhythm emerges from HISTORY, not current state.

### What Drives Scene Selection

| Factor | Examples | How It Works |
|--------|----------|--------------|
| **Intensity History** | Recent demanding/recovery counts, scenes since last crisis | Determines rhythm phase (accumulation, test, recovery) |
| **Location Context** | Safety, purpose, privacy, activity level | Influences appropriate scene categories for the setting |
| **Categorical Properties** | NPC demeanor, quality tier, power dynamic | Combine through deterministic logic to select archetype |

### HIGHLANDER Principle for Generation

Authored tutorial and procedural content use IDENTICAL selection logic:

| Content Type | DTO Source | Selection Logic |
|--------------|------------|-----------------|
| **Authored (Tutorial)** | Hardcoded categorical properties | Same |
| **Procedural** | Derived from GameWorld state | Same |

The tutorial produces specific scenes NOT through overrides or bypasses, but because its hardcoded categorical properties (safe location, friendly NPC, recovery rhythm phase) naturally flow through the selection logic to produce the appropriate scene type.

**Forbidden:**
- TargetCategory override that bypasses selection
- Sequence-based rotation
- Current player state influencing selection
- Different code paths for authored vs procedural

**Required:**
- Same selection logic always
- Categorical properties as sole inputs
- History-based rhythm determination
- Location-appropriate scene matching

---

## Cross-References

- **Core Loops**: See [03_core_loop.md](03_core_loop.md) for how pillars manifest in gameplay
- **Balance**: See [06_balance.md](06_balance.md) for how scarcity creates depth
- **Rhythm System**: See [06_balance.md §6.4](06_balance.md) for Sir Brante rhythm implementation
- **Psychological Laws**: See [design/13_player_experience_emergence_laws.md](../design/13_player_experience_emergence_laws.md) for player psychology
- **Detailed Design**: See [design/01_design_vision.md](../design/01_design_vision.md) for exhaustive treatment
