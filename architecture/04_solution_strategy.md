# Arc42 Section 4: Solution Strategy

## 4.1 Two-Layer Architecture Decision

### Strategic Decision

**Problem:** Players need to make informed strategic decisions (WHAT to attempt) before experiencing tactical complexity (HOW to execute). Mixed concerns create confusion - players can't calculate risk before committing.

**Solution:** Strict architectural separation into TWO DISTINCT LAYERS with explicit bridge pattern:

**STRATEGIC LAYER (Perfect Information):**
- **Flow:** Obligation → Scene → Situation → Choice
- **Purpose:** Narrative progression and player decision-making with complete transparency
- **Characteristics:** Perfect information (all costs/rewards/requirements visible), state machine progression (no victory thresholds), persistent entities (scenes exist until completed/expired)
- **Entities:** Scene, Situation, ChoiceTemplate

**TACTICAL LAYER (Hidden Complexity):**
- **Flow:** Challenge Session → Card Play → Resource Accumulation → Threshold Victory
- **Purpose:** Card-based gameplay execution with emergent tactical depth
- **Characteristics:** Hidden complexity (card draw order unknown), victory thresholds (resource accumulation required), temporary sessions (created and destroyed per engagement)
- **Entities:** Challenge Session, SituationCard, Tactical Cards (Social/Mental/Physical)

**THE BRIDGE (ChoiceTemplate.ActionType):**
- **Instant:** Stay in strategic layer (apply rewards immediately)
- **Navigate:** Stay in strategic layer (move to new context)
- **StartChallenge:** Cross to tactical layer (spawn challenge session)

### Consequences

**Positive:**
- Clear separation enables perfect information at strategic layer while preserving tactical surprise
- Bridge pattern (ActionType property) provides explicit, testable routing
- One-way flow (strategic spawns tactical, tactical returns outcome) prevents circular dependencies
- Three parallel tactical systems (Social/Mental/Physical) all follow same pattern
- Layer purity enforced: Situations are strategic, challenges are tactical (never conflate)

**Negative:**
- Cannot mix strategic and tactical within single moment
- Must design clear entry/exit points for every challenge
- Two distinct entity models to maintain

**Rationale:**
Strategic layer answers "Can I afford this?" Tactical layer answers "Can I execute this?" Mixing them forces player into tactical complexity before knowing if attempt is worthwhile. Separation enables informed strategic planning.

---

## 4.2 Parse-Time Translation Strategy (Catalogue Pattern)

### Strategic Decision

**Problem:** AI content generation requires balance without knowing global game state. Hand-authoring every scene requires specifying exact numeric values (stat thresholds, coin costs) for every NPC/location combination - content explosion and maintenance nightmare.

**Solution:** THREE-PHASE CONTENT PIPELINE with parse-time translation:

**Phase 1: JSON Authoring (Categorical Properties)**
- Authors/AI write descriptive properties: "Friendly" NPC, "Premium" quality service, "Dominant" power dynamic
- NO numeric values in JSON (no "StatThreshold: 8")
- Categorical enums: NPCDemeanor, Quality, PowerDynamic, EnvironmentQuality

**Phase 2: Parsing (Parse-Time Translation - ONE TIME ONLY)**
- Catalogues translate categorical → concrete using universal formulas
- `ScaledStatThreshold = BaseThreshold × NPCDemeanor.Multiplier × PowerDynamic.Multiplier`
- Example: Base 5 × Friendly 0.6 × Equal 1.0 = 3 (easy negotiation)
- Example: Base 5 × Hostile 1.4 × Submissive 1.4 = 10 (hard negotiation)
- Happens ONCE at game load, stored in templates

**Phase 3: Runtime (Use Concrete Values ONLY)**
- NO catalogue calls at runtime
- NO string matching on property names
- NO dictionary lookups
- Direct property access: `if (player.Stat >= choice.RequiredStat)`

### Formula Example

```
Base archetype: StatThreshold = 5, CoinCost = 8

Context: Friendly NPC (0.6x), Premium Quality (1.6x), Equal Power (1.0x)

Scaled values:
- StatThreshold: 5 × 0.6 × 1.0 = 3 (friendly = easier)
- CoinCost: 8 × 1.6 = 13 (premium = more expensive)

Same archetype, contextually appropriate difficulty.
```

### Consequences

**Positive:**
- AI can generate balanced content (describe categorically, system translates to balanced numbers)
- Minimal authoring (just specify entity type, not 50 numeric values)
- Universal formulas (one negotiation archetype scales to all contexts)
- Dynamic scaling (change multiplier, all scenes rebalance)
- Zero runtime overhead (translation happens once at parse-time)

**Negative:**
- Cannot hand-tune specific instances (all scaling formulaic)
- Must design universal scaling that works across all contexts
- Parse-time cost (one-time, acceptable during load screen)

**FORBIDDEN Forever:**
- Runtime catalogue calls (parse-time ONLY)
- String matching on IDs or property names
- Dictionary<string, int> for costs/requirements
- ID-based logic for routing

**Rationale:**
AI doesn't know "player is level 15, economy is 1000 coins/day, stat progression is 1-20 range." But AI CAN describe "this innkeeper seems friendly, this inn is premium quality." System translates categories to balanced numbers. Enables infinite AI-generated content without balance knowledge.

---

## 4.3 Lazy Instantiation Strategy (Three-Tier Timing Model)

### Strategic Decision

**Problem:** If all scenes instantiate all actions at spawn, GameWorld bloats with thousands of inaccessible actions. Memory waste, query performance degradation, UI rendering unnecessary entities.

**Solution:** THREE-TIER TIMING MODEL with lazy instantiation:

**TIER 1: Templates (Parse Time - Immutable Archetypes)**
- SceneTemplate with embedded SituationTemplates and ChoiceTemplates
- Created ONCE from JSON during package loading
- Stored in GameWorld.SceneTemplates (reusable blueprints)
- Never mutated at runtime

**TIER 2: Scenes/Situations (Spawn Time - Mutable Instances)**
- Scene created from template when Obligation triggers or procedural system generates
- Contains embedded Situations (NOT separate collection)
- InstantiationState = Deferred (actions NOT created yet)
- Stored in GameWorld.Scenes

**TIER 3: Actions (Query Time - Ephemeral UI Projections)**
- Actions created ONLY when player enters matching context
- SceneFacade checks: "Is player at Location X and Scene has active Situation there?"
- If yes: Instantiate actions from ChoiceTemplates
- Add to GameWorld.LocationActions or GameWorld.NPCActions
- InstantiationState = Instantiated
- Actions deleted after execution (ephemeral lifecycle)

### Complete Flow Example

```
PARSE TIME:
JSON → SceneTemplate "lodge_001"
     → Contains SituationTemplate "negotiate"
     → Contains ChoiceTemplate "persuade"
     → Stored in GameWorld.SceneTemplates

SPAWN TIME (Day 5):
Obligation triggers → Creates Scene instance from template
                   → Scene.Situations populated
                   → Scene.InstantiationState = Deferred
                   → NO actions created yet
                   → Stored in GameWorld.Scenes

QUERY TIME (Player enters common room):
Player at Location → SceneFacade checks active scenes
                  → Scene.CurrentSituation context matches
                  → Instantiate Action from ChoiceTemplate
                  → Add to GameWorld.LocationActions
                  → Scene.InstantiationState = Instantiated
                  → UI renders action button

EXECUTION:
Player clicks action → GameFacade processes
                    → Apply costs/rewards
                    → Delete action (ephemeral)
                    → Scene advances to next Situation
```

### Consequences

**Positive:**
- Memory efficiency (only active context actions exist)
- Performance (no querying thousands of inactive actions)
- Clean lifecycle (actions created, displayed, executed, deleted)
- Single source of truth (templates immutable, instances mutable, actions ephemeral)

**Negative:**
- Three-tier complexity (must understand timing model)
- Cannot pre-compute all actions (lazy = query-time cost)
- InstantiationState tracking required

**Rationale:**
Player can only interact with current context. Creating actions for 100 locations when player at one location wastes 99% of work. Lazy instantiation pays cost only when needed. Three tiers separate immutable patterns (templates) from mutable state (scenes) from ephemeral UI (actions).

---

## 4.4 Clean Architecture Approach

### Dependency Inversion

**ALL DEPENDENCIES FLOW INWARD TOWARD GAMEWORLD:**

```
UI Layer (Blazor Components)
    ↓ depends on
Facade Layer (Orchestration Services)
    ↓ depends on
GameWorld (Zero Dependencies)
```

**GameWorld:**
- Single source of truth
- Zero external dependencies
- Contains STATE, not BEHAVIOR
- Collections: Scenes, Locations, NPCs, Routes, Player, Cards, Templates

**Facades:**
- Orchestration layer (GameFacade coordinates all subsystems)
- Business logic (SocialFacade, MentalFacade, PhysicalFacade, LocationFacade, etc.)
- ALL depend on GameWorld (read and write state)
- Stateless (operate on GameWorld, hold no state themselves)

**UI Components:**
- Presentation layer only (GameScreen.razor, LocationContent.razor, etc.)
- Depend on GameFacade (never access GameWorld directly)
- Render GameWorld state via context objects
- Send user actions to GameFacade

### Single Responsibility

**GameFacade:**
- Pure orchestrator
- Routes requests to specialized facades
- NO business logic (delegates everything)

**Specialized Facades:**
- ONE domain area each (Social challenges, Mental challenges, Locations, Resources, etc.)
- Business logic encapsulated
- Update GameWorld state directly

**No Shared Mutable State:**
- NO SharedData dictionaries
- NO Context objects with setState methods
- NO Observer patterns with event subscriptions
- State lives in GameWorld, modified by facades, rendered by UI

---

## 4.5 Technology Stack Decisions

### C# ASP.NET Core + Blazor Server

**Chosen:**
- .NET 8 / ASP.NET Core (mature, performant runtime)
- Blazor Server with ServerPrerendered mode (rich UI, server-side state)
- C# 12 (strong typing, LINQ, async/await)

**Rationale:**
- Single-player game benefits from server-side state (no client-side state synchronization)
- Strong typing enforces architectural constraints at compile time
- Blazor Server eliminates JavaScript requirement (C# end-to-end)
- ServerPrerendered mode provides fast initial render (better UX)

**Trade-Offs:**
- Persistent WebSocket required (limits hosting options vs static sites)
- Idempotence required (double-rendering lifecycle)
- Single-player only (no distributed state)

### Static Content Loading (No Dependency Injection for GameWorld)

**Chosen:**
- GameWorld initialized explicitly: `GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();`
- Registered as singleton: `builder.Services.AddSingleton(gameWorld);`
- Content loaded at startup from JSON packages

**Rationale:**
- Game state initialization NOT a service lifetime concern (happens once at startup)
- Explicit initialization more debuggable than DI lambda factory
- Static content loading fits single-player model (no dynamic content sources)

**Rejected Alternative:**
- FORBIDDEN: Lambda factory functions in DI registration
- CORRECT: Explicit instance creation followed by singleton registration

### Domain-Driven Design (No Abstraction Over-Engineering)

**Chosen:**
- Direct domain concepts (Scene, Situation, Location, NPC, Route)
- Domain services (Facades) encapsulate business logic
- No generic layers (no IRepository, no generic CRUD)
- No abstraction for abstraction's sake

**Rationale:**
- Game domain is concrete and specific (not a generic problem space)
- Over-abstraction creates indirection without benefit
- Direct domain modeling matches conceptual model (verisimilitude)
- Easier to understand ("Scene contains Situations" vs "IContainer<ISceneElement>")

**Principle:**
> "Strong typing and explicit relationships aren't constraints that limit you - they're filters that catch bad design before it propagates."

---

## Solution Strategy Summary

The Wayfarer architecture strategy prioritizes:

1. **Layer Separation:** Strategic (perfect information) vs Tactical (hidden complexity) with explicit bridge
2. **Parse-Time Translation:** Categorical properties enable AI generation without balance knowledge
3. **Lazy Instantiation:** Three-tier timing (Templates → Scenes → Actions) for memory efficiency
4. **Dependency Inversion:** All dependencies flow inward toward zero-dependency GameWorld
5. **Technology Fit:** C# Blazor Server chosen for single-player server-side state model

**Guiding Philosophy:**
Elegance over complexity. One purpose per entity. Strong typing as design enforcement. Perfect information at strategic layer. No soft-locks ever.

---

## Related Documentation

- **01_introduction_and_goals.md** - Quality goals driving these decisions
- **02_constraints.md** - Technical constraints affecting solution choices
- **05_building_block_view.md** - Static structure implementing this strategy
- **06_runtime_view.md** - Dynamic behavior realizing this strategy
- **08_crosscutting_concepts.md** - Patterns and principles underlying all decisions
- **09_architecture_decisions.md** - Detailed ADRs with full context and consequences
