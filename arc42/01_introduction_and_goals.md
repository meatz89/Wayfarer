# 1. Introduction and Goals

## 1.1 Requirements Overview

**Wayfarer** is a single-player tactical RPG combining visual novel narrative choices with card-based tactical challenges. Players navigate a low-fantasy world as a traveling courier, engaging NPCs, pursuing investigations, and resolving conflicts through strategic resource management.

### The Core Problem This Architecture Solves

Players need to make **informed strategic decisions** before committing resources. Traditional RPG architectures mix strategic planning with tactical execution, forcing players into complexity before they can assess whether an attempt is worthwhile.

**Wayfarer separates these concerns:**
- **Strategic Layer**: Perfect information. Players see all costs, requirements, and rewards before committing. Decisions are about WHAT to attempt.
- **Tactical Layer**: Hidden complexity. Card draw order, exact outcomes emerge during play. Execution is about HOW to succeed.

This separation enables the game's core design principle: **impossible choices through resource scarcity**. When players can calculate exact costs, they experience genuine strategic tension choosing between competing options rather than gambling on unknown outcomes.

### Central Gameplay Loop

Players accept delivery jobs, navigate routes encountering choice-driven scenes, earn coins, spend on survival, and repeat. Resource pressure creates impossible choices where optimization skill determines success.

The game operates on **nested time scales**: immediate encounters (seconds), delivery cycles (minutes), and long-term progression (hours). Each scale forces different resource trade-offs, creating layered strategic depth.

### Infinite Journey Design

The main storyline is procedurally generated and infinite. There is no "ending" to reach—the journey itself is the experience. This architectural choice eliminates soft-lock risks from finite content while requiring robust procedural generation systems.

---

## 1.2 Quality Goals

Quality goals are prioritized into three tiers. When goals conflict, higher tiers take precedence.

### TIER 1: Non-Negotiable

| Goal | Rationale |
|------|-----------|
| **No Soft-Locks** | An infinite game with one soft-lock is catastrophic. Players 50+ hours deep cannot "restart from earlier save." Every game state must have forward progress. |
| **Single Source of Truth** | Redundant state creates desync bugs. If player location is stored in two places and they disagree, which is correct? One canonical location eliminates ambiguity. |

### TIER 2: Core Experience

| Goal | Rationale |
|------|-----------|
| **Impossible Choices** | Strategic depth emerges from resource scarcity, not mechanical complexity. Players should ask "Can I afford A or B?" not "What does this button do?" |
| **Perfect Information** | Strategic layer requires complete transparency. Hidden information belongs only in tactical layer after player commits. |
| **Elegance** | Systems connect at explicit boundaries. One purpose per entity, minimal coupling. Complexity signals design problems. |

### TIER 3: Architectural Quality

| Goal | Rationale |
|------|-----------|
| **Verisimilitude** | Entity relationships should match player mental models. If explanations feel backwards ("Locations own Scenes" vs "Scenes appear at Locations"), the model is wrong. |
| **Maintainability** | Code optimized for reading and debugging, not execution speed. In a turn-based browser game where humans react in 200ms, microsecond optimizations are meaningless. |
| **Clarity** | Code should read as domain prose. Clever algorithms that obscure intent are rejected regardless of performance benefits. |

### Conflict Resolution

When quality goals conflict, resolution follows tier priority:

- **No Soft-Locks vs Resource Scarcity**: Add zero-cost fallback choices. TIER 1 wins; scarcity preserved via poor fallback rewards.
- **Perfect Information vs Tactical Surprise**: Layer separation. Strategic layer shows costs before entry; tactical layer hides card draw.
- **Maintainability vs Performance**: Always choose maintainability. Performance optimization provides no measurable benefit at this game's scale.

---

## 1.3 Stakeholders

| Stakeholder | Concern | Architecture Response |
|-------------|---------|----------------------|
| **Players** | Can I make informed decisions? Will I get stuck? | Perfect information at strategic layer. Guaranteed fallback choices prevent soft-locks. |
| **Developers** | Can I add content without breaking systems? Is the code debuggable? | Single source of truth eliminates desync. Explicit entity relationships catch errors at compile time. |
| **Content Authors** | Can I create balanced content without knowing game internals? | Categorical properties (Friendly, Premium, Hostile) translate to balanced numbers automatically. Authors describe WHAT, system determines HOW. |
| **Future Maintainers** | Why was this decision made? What alternatives existed? | Architecture Decision Records document rationale. Quality goal tiers resolve conflicts predictably. |

---

## Related Documentation

- [02_constraints.md](02_constraints.md) — Technical and organizational boundaries
- [04_solution_strategy.md](04_solution_strategy.md) — Fundamental decisions realizing these goals
- [10_quality_requirements.md](10_quality_requirements.md) — Testable quality scenarios
- [design/01_design_vision.md](../design/01_design_vision.md) — Game design philosophy (player experience, not technical)
