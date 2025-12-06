# 8. Essential Glossary

## Why This Document Exists

This glossary defines essential game design terms for quick reference.

---

## Core Concepts

### A-Story
The infinite main narrative spine. Phase 1 (A1-A10) is authored tutorial. Phase 2 (A11+) is procedurally generated continuation that never ends.

### Archetype
Reusable mechanical pattern defining situation structure (choice count, path types, cost formulas) independent of narrative content. Same archetype + different entity properties = infinite variations.

### Atmospheric Action Layer
Always-available core actions (Travel, Work, Rest, Move) forming persistent gameplay baseline. Scene-based actions layer on top, never replacing atmospheric scaffolding.

### Build
Player's character specialization emerging from stat allocation choices. Not a pre-selected class—identity emerges from resource investment decisions.

### Four-Choice Archetype
The pattern guaranteeing every A-story situation offers four path types: stat-gated (free for specialists), resource (costs coins/items), challenge (skill test), and fallback (always available).

---

## Gameplay Terms

### Impossible Choice
Core design principle. Every decision forces trade-offs between multiple valid alternatives. No optimal path exists—only the path you choose.

### Mental Math Design
Design principle (DDR-007). All numeric values small enough for players to calculate in their heads without external tools. Values like +2 or -5, not +147 or -2,340.

### Deterministic Arithmetic
Design principle (DDR-007). Strategic outcomes are predictable from inputs with no randomness. "Insight 5 required" means exactly that—not "70% chance with Insight 5."

### Absolute Modifier
Numeric adjustment that stacks additively. Two +3 bonuses always equal +6. Contrast with multipliers which compound unexpectedly. Part of Intentional Numeric Design (DDR-007).

### Perfect Information
Design principle. All costs, requirements, and rewards visible before selection. Strategic layer has no hidden gotchas. Extended by DDR-007: information must be not just visible but mentally calculable.

### Requirement Inversion
Foundational principle: stat requirements affect COST, not ACCESS. Unlike traditional RPGs with boolean gates ("Level 5 required"), Wayfarer uses resource arithmetic—everyone can progress, but high-stat players pay less. This ensures no soft-locks while rewarding specialization.

### Scene-Situation-Choice Flow
Narrative structure. Scene (container) → Situation (decision point) → Choice (four paths). All content follows this hierarchy.

### Strategic Layer
The WHAT and WHERE layer. Player sees all information. Decides WHETHER to attempt. Entities are persistent.

### Tactical Layer
The HOW layer. Card-based challenges. Hidden complexity (draw order). Tests execution skill. Sessions are temporary.

---

## Resources

### Universal Resources
Resources competing across all systems: Time (blocks per day), Coins (currency), Focus (Mental pool), Stamina (Physical pool), Resolve (Willpower gate—see below), Health (survival threshold).

### Resolve (Sir Brante Willpower Pattern)
The willpower resource governing meaningful story choices. Unlike other resources:
- **Starts at 0** (must earn before spending)
- **Can go negative** (minimum -10, locks out costly choices)
- **Large swings** (+5/+10 gains, -5/-10 costs)
- **Dual nature** every costly choice requires Resolve >= 0 AND costs Resolve

This creates the "willpower gate"—players cannot make difficult choices until they've built resolve through earlier positive choices. See [04_systems.md §4.1](04_systems.md#resolve-the-willpower-gate-sir-brante-pattern) for full design rationale.

### Tactical Resources
Resources existing only within challenge sessions: Builder resources (Momentum/Progress/Breakthrough), Session resources (Initiative/Attention/Exertion), Threshold resources (Doubt/Exposure/Danger).

---

## The Five Stats

| Stat | Domain | Governs |
|------|--------|---------|
| **Insight** | Mental | Investigation, observation, puzzle-solving |
| **Cunning** | Mental | Deception, misdirection, reading situations |
| **Rapport** | Social | Building trust, friendly persuasion |
| **Diplomacy** | Social | Formal negotiation, authority appeal |
| **Authority** | Physical | Intimidation, command presence |

---

## Challenge Systems

### Mental Challenge
Card-based investigation. Build Progress through Leads and Details. Session resource: Attention. Threshold: Exposure.

### Physical Challenge
Card-based obstacle resolution. Overcome through Exertion. Session resource: Exertion. Threshold: Danger.

### Social Challenge
Card-based persuasion. Build Momentum through dialogue. Session resource: Initiative. Threshold: Doubt.

---

## Story Categories

A-Story, B-Story, and C-Story are distinguished by a **combination of properties**, not a single axis. All three use identical Scene-Situation-Choice structure; category determines rules and validation.

### Story Category Property Matrix

| Property | A-Story | B-Story | C-Story |
|----------|---------|---------|---------|
| **Scene Count** | Infinite chain | Multi-scene arc (3-8) | Single scene |
| **Repeatability** | One-time (sequential) | One-time (completable arc) | Repeatable |
| **Fallback Required** | Yes (every situation) | No | No |
| **Can Fail** | Never | Yes | Yes |
| **Resource Flow** | Sink (travel costs to reach) | Source (significant rewards) | Source (incremental rewards) |
| **Typical Scope** | World expansion | Venue depth | Location/route |
| **Player Initiation** | Automatic (previous scene spawns next) | Voluntary (player accepts) | Organic (encountered during play) |

### A-Story
The infinite main narrative spine. Scenes chain sequentially (A1 → A2 → A3...). Every situation requires a fallback choice guaranteeing forward progress. Primary purpose is world expansion—creating new venues, districts, regions, routes, and NPCs. Can never fail. Phase 1 (A1-A10) is authored tutorial; Phase 2 (A11+) is procedurally generated.

### B-Story
Multi-scene narrative arcs with substantial stakes. Player voluntarily initiates by accepting an obligation or engaging with discovered content. Spans 3-8 connected scenes forming a complete arc. Can include requirements on all choices (no mandatory fallback). Can fail with consequences. Typically works within a single venue, adding narrative depth. Provides significant resource rewards that fund A-story progression.

### C-Story
Single-scene encounters providing world texture. Encountered organically during play—route encounters during travel, service interactions at locations, opportunistic moments. Repeatable (same encounter type can occur multiple times). Can fail without major consequences. Provides incremental resource rewards. Examples: route hazards, inn services, merchant interactions, work opportunities.

### Why Three Categories?

The categories create the core gameplay loop:
1. **A-Story** creates distant goals requiring travel resources
2. **B-Story** provides substantial resource injections through meaningful engagement
3. **C-Story** provides steady resource drip through routine activities

This prevents mindless clicking through A-story (resource gate) while ensuring the player is never stuck (C-story always available).

---

## Content Terms

### Categorical Property
Strongly-typed entity attribute with intentional domain meaning. Two types exist:
- **Identity Dimensions** (what entity IS): Privacy, Safety, Activity, Purpose for locations; Profession, Personality for NPCs
- **Capabilities** (what entity CAN DO): Crossroads enables Travel; Commercial enables Work; SleepingSpace enables Rest

All categorical properties are enums with specific game effects—never generic strings.

### Explicit Property Principle
Architecture pattern requiring explicit strongly-typed properties instead of string-based generic routing. `InsightRequired` (explicit int property) catches typos at compile-time; `Type="Insight"` (string routing) fails silently at runtime. Applies to requirements, state modifications, and entity relationships. See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) §8.19 for technical details.

### Frieren Principle
Design philosophy: The game never ends. The journey is the point, not arrival. Success measured by journey quality, not reaching destination.

---

## Location Properties

Locations have two types of categorical properties that determine scene activation and gameplay.

### Identity Dimensions (What Location IS)

| Dimension | Values | Game Effect |
|-----------|--------|-------------|
| **Privacy** | Public, SemiPublic, Private | Witness presence, social stakes |
| **Safety** | Dangerous, Neutral, Safe | Threat level, guard presence |
| **Activity** | Quiet, Moderate, Busy | Population density, NPC availability |
| **Purpose** | Transit, Dwelling, Commerce, Civic, etc. | Primary functional role |

### Capabilities (What Location CAN DO)

| Capability | Enables |
|------------|---------|
| **Crossroads** | Travel action (route selection) |
| **Commercial** | Work action (earn coins) |
| **SleepingSpace** | Rest action (restore health/stamina) |
| **Restful** | Enhanced restoration quality |
| **Market** | Trading with pricing modifiers |

Scene activation uses these properties for categorical matching—scenes activate at locations matching their filter criteria.

---

## Cross-References

- **Technical Terms**: See [arc42/12_glossary.md](../arc42/12_glossary.md) for implementation terminology
