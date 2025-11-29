# 4. Solution Strategy

This section describes the fundamental decisions shaping Wayfarer's architecture.

---

## 4.1 Two-Layer Architecture

The central architectural decision separates gameplay into two distinct layers: strategic and tactical.

Players need to assess whether an attempt is worthwhile before experiencing execution complexity. Traditional RPG architectures intermingle planning with execution, forcing players into tactical details before they can evaluate costs. Wayfarer solves this by maintaining strict layer separation.

The **strategic layer** provides perfect information. Players see all costs, requirements, and rewards before committing to any choice. Scenes contain situations, situations present choices, and each choice displays exactly what it costs and what it grants. This layer operates as a state machine—situations advance deterministically based on player decisions.

The **tactical layer** contains hidden complexity. When a player enters a challenge (conversation, investigation, or obstacle), they engage with card-based mechanics where draw order is unknown and outcomes emerge through play. Resources accumulate toward victory thresholds defined by situation cards.

The bridge between layers is explicit. Each choice template carries an action type: instant choices apply rewards immediately and remain in the strategic layer; navigate choices move the player to new contexts; challenge choices cross into the tactical layer by spawning a session. This one-way flow—strategic spawns tactical, tactical returns outcome—prevents circular dependencies and maintains layer purity.

---

## 4.2 Parse-Time Translation

Content authors and AI generators cannot specify exact numeric values because they lack knowledge of global game balance. An author describing a "friendly innkeeper at a premium inn" doesn't know whether the stat threshold should be 3 or 8, or whether the coin cost should be 5 or 50.

Wayfarer solves this through categorical properties and parse-time translation. Authors write descriptive categories: demeanor (Friendly, Neutral, Hostile), quality (Budget, Standard, Premium), power dynamic (Submissive, Equal, Dominant). At startup, catalogues translate these categories into concrete mechanical values using universal formulas.

A friendly NPC reduces difficulty thresholds. Premium quality increases costs. Categories translate to absolute adjustments that stack predictably—no compounding multipliers that obscure final values. See GDD DDR-007 (Intentional Numeric Design) for the design rationale. The same negotiation archetype produces an easy interaction at a friendly budget inn and a challenging one at a hostile premium establishment. Catalogues encode balance formulas once; authors instantiate infinite variations through categorical description.

This translation happens exactly once during content loading. At runtime, domain entities contain only concrete values—no catalogue lookups, no string matching, no runtime translation overhead.

---

## 4.3 Three-Tier Timing

Scenes can contain dozens of situations, each with multiple choices. Instantiating all possible actions at startup would bloat memory with thousands of inaccessible entities. Players interact with one location at a time; most content sits dormant.

Wayfarer employs lazy instantiation across three timing tiers. Templates exist permanently from parse-time—scene templates, situation templates, choice templates. These immutable archetypes define patterns without consuming significant memory.

Instances spawn when triggered. A scene spawns from its template when an obligation activates or procedural generation fires. The scene contains embedded situations, but these exist as lightweight structures referencing their templates.

Actions materialize only at query-time. When a player enters a location, the scene facade checks for active situations at that context and instantiates actions from choice templates. These actions are ephemeral—created for display, executed by the player, then deleted. The next location generates fresh actions from fresh template queries.

This three-tier model means memory contains only what players can currently access. Templates provide reusability, instances track progression state, actions serve immediate interaction.

---

## 4.4 Clean Architecture

All dependencies flow inward toward GameWorld. This single state container has zero external dependencies—it knows nothing about facades, UI components, or content pipelines.

Facades form the business logic layer. Each facade handles one domain area: social challenges, mental challenges, locations, resources, time, travel. Facades read and write GameWorld state but maintain no state themselves. They are stateless orchestrators of game rules.

UI components sit at the outer edge. Blazor razor components call facades, receive state, and render displays. They contain no game logic—the backend determines what's available, what's valid, what costs apply. The UI simply presents choices and captures input.

The content pipeline populates GameWorld at startup. JSON files flow through parsers into domain entities, which populate GameWorld collections. After initialization, the pipeline is dormant.

This inward dependency flow makes GameWorld testable in isolation, facades replaceable without UI changes, and the entire system comprehensible through clear responsibility boundaries.

---

## 4.5 Technology Selection

Blazor Server fits the single-player model naturally. Server-side state eliminates client synchronization complexity. The persistent WebSocket connection carries UI updates efficiently. ServerPrerendered mode provides fast initial render at the cost of double-execution during initialization—an acceptable trade-off managed through idempotent initialization patterns.

The domain-driven approach avoids abstraction layers. There is no IRepository interface, no generic CRUD operations, no abstraction for abstraction's sake. The game domain is concrete and specific. Scene, Situation, Location, NPC—these concepts map directly to code. Indirection without purpose obscures rather than clarifies.

Static initialization creates GameWorld before dependency injection completes. This prevents circular dependencies during the Blazor lifecycle and ensures content loading happens exactly once at startup.

---

## Related Documentation

- [05_building_block_view.md](05_building_block_view.md) — Static structure implementing this strategy
- [06_runtime_view.md](06_runtime_view.md) — Dynamic behavior realizing this strategy
- [08_crosscutting_concepts.md](08_crosscutting_concepts.md) — Patterns underlying these decisions
