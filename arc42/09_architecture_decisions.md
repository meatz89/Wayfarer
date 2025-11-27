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

## ADR-009: Dual-Tier Action Architecture

**Status:** Accepted

**Context:**
Players need guaranteed forward progress (no soft-locks) regardless of narrative state. Scene-based actions are ephemeral—they exist only when a scene is active. If all actions were scene-based, players could reach locations with no active scenes and have nothing to do.

Additionally, atmospheric actions (Work, Rest, Travel) are simple constants that don't benefit from the ChoiceTemplate system's complexity. Using ChoiceTemplate for "Work costs 1 time segment, gives 8 coins" adds indirection without value.

**Decision:**
LocationAction is a union type with two patterns discriminated by `ChoiceTemplate == null`:
- **Atmospheric (Tier 1):** Permanent scaffolding from LocationActionCatalog; uses direct `Costs`/`Rewards` properties
- **Scene-Based (Tier 2):** Ephemeral narrative from SceneFacade; uses `ChoiceTemplate` reference

**Consequences:**
- Atmospheric actions always available (soft-lock prevention)
- Scene-based actions layer on top (narrative depth)
- Pattern discrimination required in executors
- Cannot accidentally delete "legacy" properties—both patterns intentional
- LocationActionCatalog is critical infrastructure

**Alternatives Rejected:**
- ChoiceTemplate for everything: Overkill for constants, adds complexity
- Direct properties for everything: Cannot express dynamic formulas and OR requirements
- Separate entity types: Would require parallel collections and duplicate validation logic

**Historical Note:** This ADR documents architecture after a near-miss where Costs/Rewards properties were almost deleted as "legacy code." Both patterns are intentional and critical.

---

## ADR-010: Entity Resolution via Categorical Filters

**Status:** Accepted

**Context:**
Procedural content generation requires finding or creating entities matching characteristics ("a Friendly innkeeper at a Premium inn"). ID-based references would require hardcoding specific entities that may not exist in generated worlds.

**Decision:**
EntityResolver pattern with PlacementFilter for categorical queries:
- Filters specify categorical requirements (Purpose, Safety, Demeanor)
- Resolver searches existing entities matching filter
- If found, returns existing entity (find)
- If not found, creates new entity from filter (create)

**Consequences:**
- Content templates work with any procedurally generated world
- No hardcoded entity references in templates
- Requires well-defined categorical properties on all entities
- Find-or-create semantics may create unexpected entities if filter too loose

**Alternatives Rejected:**
- ID-based references: Breaks procedural generation
- Always-create: Would create duplicate entities unnecessarily
- Manual resolution: Error-prone, doesn't scale

---

## ADR-011: Package-Round Entity Tracking

**Status:** Accepted

**Context:**
Previous spatial initialization implementation violated the package-round principle by processing ALL entities in GameWorld whenever ANY package loaded. Methods like `PlaceAllLocations()` scanned entire collections and used entity state checks (`HexPosition == null`) to filter already-processed entities. This caused O(n×p) performance (where p = package count) and masked architectural violations.

**Decision:**
Implement explicit package-round entity tracking via `PackageLoadResult` structure. Spatial initialization methods accept explicit entity lists instead of querying GameWorld collections.

- As entities are parsed, track them in `PackageLoadResult.LocationsAdded`, etc.
- Spatial methods receive `List<Location>` parameters, not queries
- No GameWorld collection scanning during initialization
- No entity state checks for deduplication

**Consequences:**
- O(n) total processing instead of O(n×p)
- Explicit data flow (entities flow through parameters, not queries)
- Package-round principle enforced by architecture (impossible to violate)
- Spatial methods are intentionally non-idempotent (fail-fast on double-call)
- Slightly more verbose (explicit parameters vs implicit queries)

**Alternatives Rejected:**
- Entity state checks: Hides architectural problem with tactical fix
- PackageId on entities: Pollutes domain with loading metadata
- Separate "new entity" collection: Violates Single Source of Truth

---

## ADR-012: Dual-Model Location Accessibility

**Status:** Accepted

**Context:**
The game has two types of locations with fundamentally different accessibility requirements:
- **Authored locations:** Defined in base game JSON, exist from game start (inns, taverns, checkpoints)
- **Scene-created locations:** Created dynamically by scenes during gameplay (private rooms, meeting chambers)

A naive implementation blocked ALL locations unless an active scene granted access. This violated TIER 1 (No Soft-Locks) because authored locations became inaccessible when no scene was active at them.

A proposed `GrantsLocationAccess` property on SituationTemplate was dead code—if a situation exists at a scene-created location, the player MUST access it to engage. Setting `GrantsLocationAccess = false` would guarantee a soft-lock.

**Decision:**
Dual-model accessibility via explicit `Location.Origin` enum (not nullable Provenance):
- **Authored locations** (`Origin == LocationOrigin.Authored`): ALWAYS accessible per TIER 1 No Soft-Locks principle
- **Scene-created locations** (`Origin == LocationOrigin.SceneCreated`): Accessible when ANY active scene's current situation is at that location

**Consequences:**
- Authored locations always reachable (no soft-locks in base game)
- Scene-created locations automatically unlock when scene advances to their situation
- No explicit "unlock" properties needed—situation presence implies access
- Explicit `Origin` enum provides type-safe discriminator (not null-as-domain-meaning)
- `Provenance` property is separate forensic metadata (which scene, when) not used for accessibility
- MovementValidator delegates to LocationAccessibilityService for all accessibility checks

**Alternatives Rejected:**
- `GrantsLocationAccess` property: Dead code—can never meaningfully be false without causing soft-lock
- Flag-based locking (`IsLocked`): Requires explicit unlock actions, risks desync between lock state and scene state
- Scene-based access for ALL locations: Blocked authored locations without active scenes
- Null-as-domain-meaning (`Provenance == null`): Bad pattern—null is absence, not a domain concept

**Related Decisions:**
- ADR-006: TIER 1 No Soft-Locks drives authored location always-accessible requirement
- ADR-010: Entity Resolution via Categorical Filters creates scene-created locations
- ADR-011: Provenance tracking provides forensic metadata separate from accessibility model

---

## ADR-013: Provenance Entity Metadata

**Status:** Accepted

**Context:**
Debugging scene-created entities requires knowing which scene created them and when. Without forensic metadata, answering "where did this NPC come from?" requires searching all scene templates. Provenance tracking is pure debugging utility—not used for game logic or accessibility.

**Decision:**
Add `Provenance` property to Location and NPC entities containing SceneTemplate.Id and creation timestamp. This is forensic metadata only, never used for game logic or accessibility decisions.

**Consequences:**
- Debugger shows entity origin immediately
- No runtime logic depends on Provenance (kept separate from functional properties)
- Provenance is nullable—null means authored entity, not scene-created
- Separate from `LocationOrigin` enum which drives accessibility logic

**Alternatives Rejected:**
- Using Provenance for accessibility: Conflates debugging metadata with game logic
- Provenance as discriminator: Null-as-domain-meaning antipattern

---

## ADR-014: Package-Round Principle

**Status:** Accepted

**Context:**
A package is a unit of JSON content defining templates and initial entities. During initialization, parsers must process exactly the entities from the current package being loaded—no more, no less. Methods that process "all entities in GameWorld" violate this principle, causing O(n×p) performance and architectural drift.

**Decision:**
All initialization methods accept explicit entity lists as parameters. No method scans GameWorld collections during initialization. Package-round processing is enforced architecturally via `PackageLoadResult` tracking.

**Consequences:**
- O(n) total initialization instead of O(n×p)
- Explicit data flow (entities flow through parameters)
- Impossible to accidentally process entities from other packages
- Spatial initialization methods are non-idempotent (fail-fast on double-call)
- Slightly more verbose (explicit parameters vs implicit queries)

**Related Decisions:**
- ADR-011: Package-Round Entity Tracking implements this principle for spatial initialization

---

## ADR-015: Consequence ValueObject with Hybrid Responsibility Pattern

**Status:** Accepted

**Context:**
Scene-based choices have complex cost/reward structures with 30+ properties scattered across ChoiceCost and ChoiceReward classes. This led to:
- 15+ field sprawl in UI conditionals
- Duplicate affordability logic across services and UI
- No centralized projection for Perfect Information display

**Decision:**
1. Create unified `Consequence` ValueObject with signed values (negative = cost, positive = reward)
2. Implement Hybrid Responsibility Pattern:
   - Query methods on ValueObject: `HasAnyEffect()`, `IsAffordable()`, `GetProjectedState()`
   - Mutation delegated to `RewardApplicationService`
3. Keep separate `ActionCosts`/`ActionRewards` for atmospheric actions (Dual-Tier Architecture)

**Consequences:**
- (+) Single "no consequences" check: `consequence.HasAnyEffect()`
- (+) Centralized affordability: `consequence.IsAffordable(player)`
- (+) Perfect Information via `GetProjectedState()`
- (+) UI simplified from 15+ field checks to single method call
- (-) Breaking change requiring test updates

---

## Related Documentation

- [04_solution_strategy.md](04_solution_strategy.md) — High-level strategy these decisions implement
- [08_crosscutting_concepts.md](08_crosscutting_concepts.md) — Patterns arising from these decisions
- [01_introduction_and_goals.md](01_introduction_and_goals.md) — Quality goals driving decisions
