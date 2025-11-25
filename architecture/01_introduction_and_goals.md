# Arc42 Section 1: Introduction and Goals

> **Note**: For comprehensive game design philosophy, player experience goals, and design rationale, see [design/01_design_vision.md](../design/01_design_vision.md). This document focuses on technical requirements and system quality goals.

## 1.1 Requirements Overview

### System Purpose

**Wayfarer** is a low-fantasy tactical RPG with narrative-driven strategic layer and three parallel tactical challenge systems (Social, Mental, Physical). It combines visual novel-style narrative choices with tactical card-based challenges in a single-player experience.

### Core Game Concept

Players navigate a low-fantasy world as a traveler, engaging with NPCs, pursuing multi-phase investigations (Obligations), and resolving conflicts through strategic decision-making and tactical gameplay. The game creates **strategic depth through impossible choices, not mechanical complexity**. Every decision forces the player to choose between multiple suboptimal paths, revealing character through constraint.

### Central Game Loop

The core gameplay follows a resource-constrained progression model where players **accept delivery jobs → navigate route segments with choice-driven encounters → earn coins → spend on survival → repeat**. Resource pressure creates impossible choices. Optimization skill determines success.

The game is structured around **nested loops** creating strategic tension at three timescales: immediate encounters (10-30s), delivery cycles (5-15m), and long-term progression (hours). These loops force resource trade-offs at different granularities, creating layered strategic depth.

**For detailed gameplay loop documentation**, see [design/02_core_gameplay_loops.md](../design/02_core_gameplay_loops.md).

### Infinite Journey Design

The main storyline (A-story) is **infinite and procedurally-generated**, providing structure and progression without resolution. The journey itself is the point, not reaching any specific destination. Player chooses WHEN to pursue A-story content, not IF.

**For narrative design philosophy and infinite A-story rationale**, see [design/06_narrative_design.md](../design/06_narrative_design.md).

### Perfect Information Principle

Before making any meaningful choice, players see exactly what it will cost and what they'll gain. No hidden consequences. This lets players make strategic decisions about how to spend limited resources (time, energy, health, social capital).

**Resource Pressure Philosophy:**
- Delivery earnings barely cover survival costs
- Small profit margin requires consistent successful deliveries
- Every choice matters mechanically
- Optimization skill provides measurable advantage
- Resource management is core gameplay challenge

---

## 1.2 Quality Goals

The following quality goals are prioritized in three tiers, defining the architectural foundation of Wayfarer:

### TIER 1: Non-Negotiable (Never Compromise)

**1. Playability: No Soft-Locks**
- Always forward progress from every game state
- Can the player REACH this content from game start?
- Can the player SEE this content in UI?
- Can the player EXECUTE this content without errors?
- Does player have FORWARD PROGRESS from every state?

**Rationale:** A game that compiles but is unplayable is WORSE than a game that crashes. In an infinite game, a single soft-lock is catastrophic. Player cannot "restart" or "load earlier save" when 50+ hours deep.

**2. Architectural Integrity: Single Source of Truth**
- One owner per entity type (HIGHLANDER principle)
- GameWorld owns all entities via flat lists
- No redundant storage that creates desync bugs
- Explicit ownership eliminates orphaned entities

**Rationale:** Clear ownership prevents temporal coupling, eliminates ordering constraints, enables atomic operations. If A is destroyed, should B be destroyed? If yes, A owns B.

### TIER 2: Core Experience (Compromise Only with Clear Justification)

**3. Strategic Depth: Impossible Choices Through Resource Scarcity**
- All systems compete for same scarce resources (Time, Focus, Stamina, Health)
- No boolean gates for progression (resource arithmetic only)
- Player must accept one cost to avoid another
- Specialization creates identity AND vulnerability

**Rationale:** Strategic depth emerges from shared resource competition, never through linear unlocks. "I can afford to do A OR B, but not both. Both paths are valid. Both have genuine costs. Which cost will I accept?"

**See also:** [design/05_resource_economy.md](../design/05_resource_economy.md) for resource economy philosophy, [design/08_balance_philosophy.md](../design/08_balance_philosophy.md) for balance principles.

**4. Perfect Information (Strategic Layer)**
- All costs, requirements, rewards visible before selection
- Strategic layer: What/where decisions with complete transparency
- Tactical layer: How execution with hidden complexity until engaged
- Player can calculate exact outcomes before commitment

**Rationale:** Player makes informed strategic decisions. Hidden complexity belongs in tactical layer only. Can player decide WHETHER to attempt before entering?

**5. Elegance: Minimal Interconnection**
- Systems connect at explicit boundaries via typed rewards
- One purpose per entity, one arrow per connection
- No tangled web of dependencies
- Clean separation of concerns

**Rationale:** Good design feels elegant. Bad design requires workarounds. Strong typing and explicit relationships aren't constraints - they're filters that catch bad design before it propagates.

### TIER 3: Architectural Quality (Prefer but Negotiable)

**6. Verisimilitude Throughout**
- All mechanics make narrative sense in grounded historical-fantasy world
- Entity relationships match conceptual model
- Real human dynamics justify mechanical guarantees
- Fiction supports mechanics

**Rationale:** Relationships should feel natural, not backwards. Does explanation feel forced? Scenes spawn from Obligations (correct), not Locations own Scenes (backwards).

**7. Maintainability Over Performance**
- Code optimizes for long-term readability and debuggability
- Use `List<T>` for domain entity collections (NOT Dictionary/HashSet)
- LINQ queries over imperative loops (declarative over procedural)
- Performance optimization explicitly deprioritized at current scale

**Rationale:** This is a synchronous, browser-based, single-player, turn-based narrative game with minimal scale (20 NPCs, 30 Locations max). Performance optimization provides **literally zero measurable benefit** (List scan: 0.001ms, browser render: 16ms, human reaction: 200ms+) while imposing **significant maintainability cost**. Dictionary for 20 entities is premature optimization. Code should be CLEAR, not CLEVER.

**See also:** [10_quality_requirements.md Section 10.1.1](10_quality_requirements.md) for game context analysis, [CLAUDE.md Dictionary/HashSet Antipattern](../CLAUDE.md) for detailed enforcement.

**8. Clarity Over Cleverness**
- Code reads like prose expressing domain intent directly
- Semantic honesty (method names match actual behavior exactly)
- Domain-driven collections (entities, not indexes)
- Fail-fast error handling (immediate null-reference at call site)

**Rationale:** Code is read 10× more than written. Future maintainers (including AI assistants) must understand intent without explanation. Clever algorithms that save microseconds while obscuring meaning are rejected. Explicit domain types beat generic abstractions.

---

## 1.3 Stakeholders

### Players (Primary Stakeholders)

**Interest:** Strategic decision-making experience with meaningful choices
**Expectations:**
- Transparent information display enabling calculated decisions
- Resource management challenges with visible trade-offs
- No soft-locks or unwinnable states
- Infinite content with coherent narrative progression
- Tactical gameplay depth (card-based challenges)

**Concerns:**
- Is content accessible from game start?
- Are consequences clear before commitment?
- Can I optimize strategy through skill improvement?
- Will I encounter dead ends or broken progression?

### Developers (Implementation Stakeholders)

**Interest:** Maintainable, extensible architecture enabling rapid content creation
**Expectations:**
- Clear architectural patterns (HIGHLANDER, Catalogue, Three-Tier Timing)
- Strong typing enforcing design quality
- Single source of truth eliminating desync bugs
- Explicit ownership preventing orphaned entities
- Parse-time validation catching errors early
- Code optimized for readability over performance (List<T> + LINQ, NOT Dictionary/HashSet)
- Declarative LINQ queries over imperative loops
- Domain-driven collections (entities, not indexes)

**Concerns:**
- Can I add new content without breaking existing systems?
- Are dependencies explicit and manageable?
- Does type system catch design errors at compile time?
- Can I test features in isolation?
- Is code readable and debuggable six months later?
- Are performance optimizations necessary or premature?

**Quality Priorities:**
1. **Maintainability** (CRITICAL) - Code must be readable, debuggable, modifiable
2. **Correctness** (CRITICAL) - Behavior matches domain semantics exactly
3. **Testability** (CRITICAL) - Business logic verifiable via automated tests
4. **Clarity** (Important) - Code reads like prose, expresses intent directly
5. **Performance** (NOT REQUIRED) - Explicitly deprioritized at n=20 entity scale

**See also:** [10_quality_requirements.md Section 10.1.1](10_quality_requirements.md) for non-functional quality requirements and game context, [CLAUDE.md](../CLAUDE.md) for coding standards and enforcement.

### Content Authors (Game Designers)

**Interest:** Authoring engaging narrative content without programming
**Expectations:**
- JSON-based content format (human-readable, version-controllable)
- Archetype system enabling mechanical reuse across fictional contexts
- Categorical properties scaling difficulty automatically
- AI narrative generation augmenting hand-crafted content
- Package cohesion rules preventing broken references

**Concerns:**
- Can I create balanced content without knowing global game state?
- Will my content integrate seamlessly with procedural generation?
- Are mechanical patterns reusable across different narratives?
- Will validation catch broken references at load time?

**See also:** [design/07_content_generation.md](../design/07_content_generation.md) for archetype system, [design/09_design_patterns.md](../design/09_design_patterns.md) for reusable patterns, [design/10_tutorial_design.md](../design/10_tutorial_design.md) for tutorial content design.

### AI Assistants (Development Support Stakeholders)

**Interest:** Clear constraints and patterns enabling reliable code generation
**Expectations:**
- Explicit coding standards (type restrictions, lambda rules, formatting)
- Documented architectural patterns with enforcement rules
- Investigation protocols before making changes (read docs FIRST, achieve 100% certainty)
- Semantic honesty (names match reality, no backwards explanations)
- Gordon Ramsay enforcement philosophy (partner, not sycophant)
- Maintainability over performance (List<T> + LINQ, NOT Dictionary/HashSet)
- Game context understanding (synchronous, browser-based, n=20 entities = performance irrelevant)

**Concerns:**
- Are constraints explicit and machine-verifiable?
- Can I distinguish between principles (mandatory) and preferences (guidance)?
- Will the codebase reject invalid changes automatically?
- Are there clear tests for architectural correctness?
- Why is Dictionary forbidden? (Game context: 0.001ms vs 16ms vs 200ms = unmeasurable benefit)
- When is performance optimization premature? (Always, at this game's scale)

**Critical Understanding:**
This game optimizes for **MAINTAINABILITY, not PERFORMANCE**. Performance optimization provides zero measurable benefit (browser render 16ms, human reaction 200ms+ dominate timing). Dictionary/HashSet are **premature optimization** for n=20 entities. Use List<T> with LINQ queries. Code should be **CLEAR, not CLEVER**.

**See also:** [CLAUDE.md](../CLAUDE.md) for mandatory documentation protocol and Dictionary/HashSet antipattern, [10_quality_requirements.md](10_quality_requirements.md) for quality scenarios.

### Future Maintainers (Long-Term Stakeholders)

**Interest:** Understanding system design decisions and rationale
**Expectations:**
- Architecture decision records (ADRs) documenting WHY choices were made
- Principle priority hierarchy for conflict resolution (TIER 1 > TIER 2 > TIER 3)
- Clear glossary of canonical term definitions
- Implementation status matrix (what's real vs. planned)
- Game context documentation explaining architectural choices (why no Dictionary)
- Non-functional quality requirements (maintainability over performance)

**Concerns:**
- Why was this design chosen over alternatives?
- Which principles take priority when they conflict?
- What terminology is current vs. deprecated?
- What features exist vs. are designed but not implemented?
- Why is performance optimization deprioritized? (Game context: synchronous, browser-based, n=20 entities)
- Why use List<T> instead of Dictionary? (Maintainability benefit >> unmeasurable performance cost)

**Key Design Decisions:**
- **Maintainability over Performance:** Code optimized for readability, not speed (List<T> + LINQ patterns)
- **Game Context Drives Architecture:** Browser render (16ms) and human cognition (200ms+) dominate timing, data structure choice (0.001ms) irrelevant
- **Premature Optimization Forbidden:** Dictionary/HashSet provide zero measurable benefit at n=20 entity scale
- **Clarity over Cleverness:** Explicit domain types, declarative LINQ, fail-fast error handling

**See also:** [09_architecture_decisions.md](09_architecture_decisions.md) for ADR-006 (principle priority hierarchy) and conflict resolution examples, [10_quality_requirements.md](10_quality_requirements.md) for detailed quality scenarios.

---

## Related Documentation

- **02_constraints.md** - Technical, organizational, and convention constraints
- **04_solution_strategy.md** - Fundamental solution decisions and core architectural approach
- **10_quality_requirements.md** - Detailed quality scenarios and acceptance criteria
- **12_glossary.md** - Canonical term definitions (60+ terms)
