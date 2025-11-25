# 9. Architecture Decisions

This section documents significant architectural decisions using the ADR (Architecture Decision Record) format.

---

## ADR-001: Two-Layer Architecture (Strategic/Tactical)

**Status:** Accepted

**Context:**
Players need to make informed resource allocation decisions before committing. Traditional RPG architectures mix planning with execution, forcing players into tactical complexity before they can assess costs. This violates the "impossible choices" design goal—players cannot strategize about resource trade-offs when outcomes are hidden.

**Decision:**
Separate gameplay into two distinct layers with explicit bridge:
- **Strategic layer:** Perfect information, visible costs/rewards, state machine progression
- **Tactical layer:** Hidden complexity, card-based execution, resource accumulation

**Consequences:**
- Players calculate exact costs before committing (supports impossible choices goal)
- Tactical surprise preserved (card draw order unknown)
- Requires explicit bridge mechanism (ActionType enum)
- Cannot mix strategic planning with tactical execution in single moment

---

## ADR-002: Parse-Time Translation via Catalogues

**Status:** Accepted

**Context:**
AI-generated content cannot specify exact numeric values because AI lacks knowledge of global game balance. Hand-authoring every scene with specific stat thresholds and coin costs creates maintenance nightmare—changing balance requires updating hundreds of files.

**Decision:**
Authors write categorical properties (Friendly, Premium, Hostile). Catalogues translate categories to concrete values at parse-time using universal formulas. Translation happens once at startup; runtime uses only concrete values.

**Consequences:**
- AI generates balanced content by describing categories
- Single formula change rebalances all affected content
- Zero runtime translation overhead
- Cannot hand-tune specific instances (all scaling formulaic)

**Alternatives Rejected:**
- Runtime translation: Performance overhead, complexity in hot paths
- Hardcoded values: Breaks AI generation, maintenance nightmare

---

## ADR-003: No Entity Instance IDs

**Status:** Accepted

**Context:**
Procedural content generation requires finding entities by characteristics ("a Friendly innkeeper at a Premium inn"), not by specific IDs ("NPC elena_001"). ID-based references create brittleness—content only works with exact entities that may not exist in procedurally generated worlds.

**Decision:**
Domain entities have no instance IDs. Use direct object references for relationships. Use categorical properties for queries. Template IDs allowed (immutable archetypes, not game state).

**Consequences:**
- Procedural generation works with categorical filters
- No ID parsing logic or composite ID construction
- Cannot reference specific entity instances from content
- Requires EntityResolver pattern for find-or-create queries

**Alternatives Rejected:**
- Optional IDs: Creates two patterns, inconsistent codebase
- ID + Object reference: Redundancy violates HIGHLANDER, desync risk

---

## ADR-004: GameWorld as Single Source of Truth

**Status:** Accepted

**Context:**
Multiple services need access to game state. Distributed state across services creates synchronization bugs—when two services disagree about player location, which is correct? Observer patterns and event systems add complexity and temporal coupling.

**Decision:**
GameWorld is the sole state container with zero external dependencies. All services read/write GameWorld directly. Services are stateless—they contain logic, not state.

**Consequences:**
- No synchronization bugs (single canonical location for each state)
- GameWorld testable in isolation
- Services interchangeable without state migration
- All queries go through GameWorld (no shortcuts)

---

## ADR-005: List Over Dictionary for Domain Collections

**Status:** Accepted

**Context:**
Game operates at minimal scale (20 NPCs, 30 Locations). Dictionary optimizes O(1) lookup vs O(n) scan. At n=20, this saves ~0.0009ms—unmeasurable when browser render takes 16ms and human reaction takes 200ms.

**Decision:**
Use `List<T>` for all domain entity collections. Query with LINQ. Never use Dictionary or HashSet for domain entities.

**Consequences:**
- Uniform query patterns (all LINQ)
- Better debugger visibility (no KeyValuePair expansion)
- Fail-fast on missing entities (FirstOrDefault returns null, immediate NullReferenceException at access)
- No performance benefit at current scale (premature optimization avoided)

**Alternatives Rejected:**
- Dictionary: Adds complexity for zero measurable benefit
- Hybrid: Inconsistent patterns, harder to maintain

---

## ADR-006: Quality Goal Priority Hierarchy

**Status:** Accepted

**Context:**
Quality goals sometimes conflict. "No soft-locks" might conflict with "resource scarcity." "Perfect information" might conflict with "tactical surprise." Without clear priority, decisions become arbitrary.

**Decision:**
Three-tier priority hierarchy:
- **TIER 1 (Non-negotiable):** No soft-locks, Single source of truth
- **TIER 2 (Core experience):** Impossible choices, Perfect information, Elegance
- **TIER 3 (Architectural):** Verisimilitude, Maintainability, Clarity

Higher tier always wins in conflicts.

**Consequences:**
- Predictable conflict resolution
- TIER 1 violations are bugs (never acceptable)
- TIER 3 can be compromised for TIER 1/2 requirements
- Clear framework for design discussions

---

## ADR-007: Blazor Server with ServerPrerendered

**Status:** Accepted

**Context:**
Single-player game needs rich UI without client-side state complexity. Options: Blazor Server (server state, WebSocket), Blazor WebAssembly (client state, no server), or traditional SPA (JavaScript, API calls).

**Decision:**
Blazor Server with ServerPrerendered mode. Server maintains all state. UI updates via SignalR WebSocket.

**Consequences:**
- No client state synchronization
- Fast initial render (ServerPrerendered)
- Requires persistent WebSocket connection
- Double-render lifecycle requires idempotent initialization
- No offline play capability

**Alternatives Rejected:**
- Blazor WebAssembly: Client state adds complexity for single-player
- Traditional SPA: Requires JavaScript, loses C# end-to-end benefit

---

## ADR-008: Three-Tier Timing Model

**Status:** Accepted

**Context:**
Scenes contain many situations with multiple choices. Instantiating all actions at startup wastes memory—players access one location at a time. Full instantiation creates thousands of inaccessible entities.

**Decision:**
Three-tier lazy instantiation:
- Templates (parse-time): Immutable, permanent
- Instances (spawn-time): Mutable, persist until completed
- Actions (query-time): Ephemeral, deleted after execution

**Consequences:**
- Memory contains only accessible content
- Templates reusable across spawns
- Clear entity lifecycles
- Requires InstantiationState tracking on scenes/situations

---

## Related Documentation

- [04_solution_strategy.md](04_solution_strategy.md) — High-level strategy these decisions implement
- [08_crosscutting_concepts.md](08_crosscutting_concepts.md) — Patterns arising from these decisions
- [01_introduction_and_goals.md](01_introduction_and_goals.md) — Quality goals driving decisions
