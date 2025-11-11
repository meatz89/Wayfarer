# Arc42 Section 1: Introduction and Goals

> **Note**: For comprehensive game design philosophy, player experience goals, and design rationale, see [design/01_design_vision.md](design/01_design_vision.md). This document focuses on technical requirements and system quality goals.

## 1.1 Requirements Overview

### System Purpose

**Wayfarer** is a low-fantasy tactical RPG with narrative-driven strategic layer and three parallel tactical challenge systems (Social, Mental, Physical). It combines visual novel-style narrative choices with tactical card-based challenges in a single-player experience.

### Core Game Concept

Players navigate a low-fantasy world as a traveler, engaging with NPCs, pursuing multi-phase investigations (Obligations), and resolving conflicts through strategic decision-making and tactical gameplay. The game creates **strategic depth through impossible choices, not mechanical complexity**. Every decision forces the player to choose between multiple suboptimal paths, revealing character through constraint.

### Central Game Loop

The core gameplay follows a resource-constrained progression model where players **accept delivery jobs → navigate route segments with choice-driven encounters → earn coins → spend on survival → repeat**. Resource pressure creates impossible choices. Optimization skill determines success.

The game is structured around **nested loops** creating strategic tension at three timescales: immediate encounters (10-30s), delivery cycles (5-15m), and long-term progression (hours). These loops force resource trade-offs at different granularities, creating layered strategic depth.

**For detailed gameplay loop documentation**, see [design/02_core_gameplay_loops.md](design/02_core_gameplay_loops.md).

### Infinite Journey Design

The main storyline (A-story) is **infinite and procedurally-generated**, providing structure and progression without resolution. The journey itself is the point, not reaching any specific destination. Player chooses WHEN to pursue A-story content, not IF.

**For narrative design philosophy and infinite A-story rationale**, see [design/06_narrative_design.md](design/06_narrative_design.md).

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

**See also:** [design/05_resource_economy.md](design/05_resource_economy.md) for resource economy philosophy, [design/08_balance_philosophy.md](design/08_balance_philosophy.md) for balance principles.

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

**Concerns:**
- Can I add new content without breaking existing systems?
- Are dependencies explicit and manageable?
- Does type system catch design errors at compile time?
- Can I test features in isolation?

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

**See also:** [design/07_content_generation.md](design/07_content_generation.md) for archetype system, [design/09_design_patterns.md](design/09_design_patterns.md) for reusable patterns, [design/10_tutorial_design.md](design/10_tutorial_design.md) for tutorial content design.

### AI Assistants (Development Support Stakeholders)

**Interest:** Clear constraints and patterns enabling reliable code generation
**Expectations:**
- Explicit coding standards (type restrictions, lambda rules, formatting)
- Documented architectural patterns with enforcement rules
- Investigation protocols before making changes
- Semantic honesty (names match reality)
- Gordon Ramsay enforcement philosophy (partner, not sycophant)

**Concerns:**
- Are constraints explicit and machine-verifiable?
- Can I distinguish between principles and preferences?
- Will the codebase reject invalid changes automatically?
- Are there clear tests for architectural correctness?

### Future Maintainers (Long-Term Stakeholders)

**Interest:** Understanding system design decisions and rationale
**Expectations:**
- Architecture decision records (ADRs) documenting WHY choices were made
- Principle priority hierarchy for conflict resolution
- Clear glossary of canonical term definitions
- Implementation status matrix (what's real vs. planned)

**Concerns:**
- Why was this design chosen over alternatives?
- Which principles take priority when they conflict?
- What terminology is current vs. deprecated?
- What features exist vs. are designed but not implemented?

---

## Related Documentation

- **02_constraints.md** - Technical, organizational, and convention constraints
- **04_solution_strategy.md** - Fundamental solution decisions and core architectural approach
- **10_quality_requirements.md** - Detailed quality scenarios and acceptance criteria
- **12_glossary.md** - Canonical term definitions (60+ terms)
