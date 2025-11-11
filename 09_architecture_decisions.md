# Arc42 Section 9: Architecture Decisions

## 9.1 Overview

This section documents major architectural decisions (ADRs) that shape the Wayfarer system. Each decision record follows the standard format: Context, Decision, Consequences, and Alternatives Considered.

---

## ADR-001: Infinite A-Story Without Resolution (Frieren Principle)

### Status
**Accepted** - Core design principle

### Context

Traditional narrative games face a fundamental problem: How to end a story in a satisfying way while supporting post-ending gameplay. This creates:
- Arbitrary ending points requiring narrative justification
- Post-ending gameplay awkwardness ("you saved the world, now go collect flowers")
- No replayability (same content, same ending)
- Narrative closure pressure forcing rushed conclusions

Additionally, live game evolution requires ongoing content, but traditional endings create natural stopping points where players leave.

### Decision

**The game never ends.** The main storyline (A-story) is an **infinite procedurally-generated spine** that provides structure and progression without resolution.

**Two-Phase Design:**

**Phase 1: Authored Tutorial (A1-A3, expandable to A10+)**
- Hand-crafted scenes teaching mechanics
- Fixed sequence establishing pursuit framework
- 30-60 minutes (current) to 2-4 hours (expanded)
- Triggers procedural transition when complete

**Phase 2: Procedural Continuation (A4+ → ∞)**
- Scene archetype selection from catalog (20-30 archetypes)
- Entity resolution via categorical filters (no hardcoded IDs)
- AI narrative generation connecting to player history
- Escalating scope/tier over time (local → regional → continental → cosmic)
- Never ends, never resolves, always deepens

**Narrative Pattern:**
> You travel. You arrive places. You meet people. Each place leads to another. Each person you meet suggests somewhere else worth visiting. The journey itself IS the point, not reaching anywhere specific.

### Consequences

**Positive:**
- **No Arbitrary Ending**: Eliminates hardest problem of narrative design
- **Infinite Replayability**: Never same twice, always new content
- **Player Agency**: Player chooses WHEN to pursue A-story, not IF
- **Live Evolution**: Perfect for ongoing content additions
- **Narrative Coherence**: Travel as eternal state matches wanderer fantasy
- **No Post-Ending Awkwardness**: Game doesn't outlive its story

**Negative:**
- **No Closure**: Some players want definitive endings
- **Generation Quality**: Procedural content must maintain standards
- **Validation Complexity**: Must guarantee forward progress infinitely
- **Balancing**: Must scale difficulty/rewards without ceiling

**Trade-Offs:**
- Sacrifices narrative closure for infinite content
- Requires robust procedural generation system
- Demands strict structural guarantees (no soft-locks ever)

### Alternatives Considered

**Option 1: Traditional Ending + Post-Game**
- Rejected: Creates awkward "you saved the world" disconnect
- Players leave after ending (natural stopping point)

**Option 2: Multiple Endings with Branching**
- Rejected: Fixed content eventually exhausted
- High authoring cost for limited replayability

**Option 3: Repeatable Endgame Loop**
- Rejected: "New Game+" feels artificial
- Resets progress or renders earlier content obsolete

### Principle Alignment

- **Core Experience (TIER 2)**: Supports infinite player agency
- **No Soft-Locks (TIER 1)**: Guaranteed progression requirement applies to ALL A-story scenes
- **Verisimilitude (TIER 3)**: Matches "eternal traveler" fantasy perfectly

---

## ADR-002: Resource Arithmetic Over Boolean Gates (Requirement Inversion)

### Status
**Accepted** - Fundamental architecture pattern

### Context

Most progression systems use boolean gate patterns:
```csharp
if (player.HasRope) { EnableClimbingAction(); }
if (player.CompletedQuest("phase_1")) { UnlockQuest("phase_2"); }
```

This creates "Cookie Clicker" pattern:
- No strategic depth (unlocks are "free", no resource cost)
- No opportunity cost (actions don't compete for shared resources)
- Linear progression (A → B → C railroad)
- No specialization (eventually max everything)
- Checklist completion instead of strategic choices

Inspired by **The Life and Suffering of Sir Brante**, which creates impossible choices through resource scarcity and perfect information.

### Decision

**Replace boolean gates with resource arithmetic.** Systems evaluate numeric resource comparisons, not binary flags.

**Resource-Based Visibility:**
```csharp
// NOT: if (player.hasHighValor) { show option; }
// YES: Valor 12 vs requirement 14 → disable but show with exact gap

Choice {
  RequiredStat: "Valor",
  StatThreshold: 14,
  IsVisible: true,          // ALWAYS visible
  IsSelectable: player.Valor >= 14  // Arithmetic comparison
}
```

**Perfect Information Display:**
```
"Confront the magistrate with evidence"
Requires: Valor 14, you have 12
[DISABLED - Need 2 more Valor]
```

**Five Resource Layers:**
1. **Personal Stats**: Capability thresholds (Insight, Rapport, Authority, etc.)
   - Range: 0-20, sweet spots exist (14 comfortable, 16 excessive)
   - Arithmetic comparison: `player.Stat >= threshold`

2. **Per-Person Relationships**: Individual capital with each NPC
   - Separate numeric score per character
   - Builds/depletes through interaction

3. **Permanent Resources**: Health, Stamina, Focus, Resolve, Coins
   - Depleting resources forcing rationing decisions
   - Restoration costs time/money

4. **Time as Competition**: Calendar days, time blocks per day
   - Finite per-day time forces prioritization
   - Cannot pursue all opportunities

5. **Ephemeral Context**: Current location, active scenes, NPC availability
   - State-based rather than permanent
   - Changes naturally through gameplay

### Consequences

**Positive:**
- **Strategic Depth**: Resources cost accumulation effort, force trade-offs
- **Opportunity Cost**: Shared resources create competition between actions
- **Specialization**: Can't max everything, builds/identity emerge
- **Perfect Information**: Player calculates EXACTLY what they can afford
- **Impossible Choices**: All options valid, insufficient resources force accepting one cost to avoid another

**Negative:**
- **Complexity**: More moving parts than simple boolean flags
- **Balancing**: Resource costs/scaling require careful tuning
- **Arithmetic Everywhere**: Every system needs numeric resource model

**Implementation Requirements:**
- All choices show exact numeric requirements
- All resources show current values and capacity
- UI displays gaps ("need 2 more Valor")
- No hidden gates (everything visible with requirements)

### Alternatives Considered

**Option 1: Keep Boolean Gates**
- Rejected: Creates shallow Cookie Clicker gameplay
- No strategic decisions, just checklist completion

**Option 2: Hybrid (Some Boolean, Some Arithmetic)**
- Rejected: Confusing mix of visibility models
- Players can't predict which system applies

**Option 3: Hidden Requirements**
- Rejected: Violates Perfect Information principle
- Trial-and-error instead of planning

### Principle Alignment

- **Resource Scarcity Creates Choices (TIER 2)**: Core mechanism
- **Perfect Information (TIER 2)**: Arithmetic enables calculation
- **No Soft-Locks (TIER 1)**: Every situation has zero-requirement fallback

### Related Patterns

- **Guaranteed Progression Pattern**: Every A-story situation has at least one choice with zero requirements
- **Four-Choice Archetype**: Stat-gated (optimal), Money-gated (reliable), Challenge (risky), Guaranteed (patient)

---

## ADR-003: Two-Layer Architecture (Strategic vs Tactical)

### Status
**Accepted** - Core architectural separation

### Context

Players need to make informed strategic decisions (WHAT to attempt) before experiencing tactical complexity (HOW to execute). If strategic and tactical concerns mix:
- Player can't calculate risk before committing
- Perfect information impossible (tactical complexity bleeds into strategic layer)
- Design complexity from intertwined systems

The challenge: Provide rich tactical gameplay without hiding strategic costs.

### Decision

**Strict architectural separation into TWO DISTINCT LAYERS** with explicit bridge pattern.

**STRATEGIC LAYER (Perfect Information):**
- Flow: Obligation → Scene → Situation → Choice
- Purpose: Narrative progression and player decision-making with complete transparency
- Characteristics:
  - Perfect information (all costs/rewards/requirements visible)
  - State machine progression (no victory thresholds)
  - Persistent entities (scenes exist until completed/expired)
- Entities: Scene, Situation, ChoiceTemplate

**TACTICAL LAYER (Hidden Complexity):**
- Flow: Challenge Session → Card Play → Resource Accumulation → Threshold Victory
- Purpose: Card-based gameplay execution with emergent tactical depth
- Characteristics:
  - Hidden complexity (card draw order unknown)
  - Victory thresholds (resource accumulation required)
  - Temporary sessions (created and destroyed per engagement)
- Entities: Challenge Session, SituationCard, Tactical Cards (Social/Mental/Physical)

**THE BRIDGE (ChoiceTemplate.ActionType):**
- **Instant**: Stay in strategic layer (apply rewards immediately)
- **Navigate**: Stay in strategic layer (move to new context)
- **StartChallenge**: Cross to tactical layer (spawn challenge session)

### Consequences

**Positive:**
- **Clear Separation**: Perfect information at strategic layer while preserving tactical surprise
- **Bridge Pattern**: Explicit, testable routing via ActionType property
- **One-Way Flow**: Strategic spawns tactical, tactical returns outcome (no circular dependencies)
- **Three Parallel Systems**: Social/Mental/Physical all follow same pattern
- **Layer Purity**: Situations are strategic, challenges are tactical (never conflate)

**Negative:**
- **Cannot Mix**: Single moment can't be both strategic and tactical simultaneously
- **Clear Entry/Exit**: Must design explicit bridge points for every challenge
- **Two Entity Models**: Distinct models to maintain (strategic vs tactical)

**Architecture Flow:**
```
STRATEGIC LAYER
  Player sees Choice: "Negotiate diplomatically"
  Costs visible: Stamina -2
  Rewards visible: OnSuccess (room unlocked), OnFailure (pay extra 5 coins)
  Player commits with full knowledge
  ↓ [BRIDGE: ActionType.StartChallenge]
TACTICAL LAYER
  Challenge session created
  SituationCards extracted (victory condition: Momentum ≥ 8)
  Card-based gameplay (draw order hidden)
  Player plays cards, builds Momentum
  Threshold reached: Victory
  Session destroyed (temporary)
  ↓ [BRIDGE: Return outcome]
STRATEGIC LAYER
  OnSuccessReward applied (room unlocked)
  Scene advances to next Situation
```

### Alternatives Considered

**Option 1: Unified Layer (No Separation)**
- Rejected: Can't provide perfect information with card-based complexity
- Strategic calculations impossible with hidden tactical variables

**Option 2: Three Layers (Strategic, Tactical, Operational)**
- Rejected: Over-engineering, two layers sufficient
- Additional layer adds no design value

**Option 3: Soft Boundary (Guidelines not Enforcement)**
- Rejected: Slippery slope leads to layer violations
- Enforcement via property routing (ActionType) provides clarity

### Principle Alignment

- **Perfect Information (TIER 2)**: Strategic layer shows all costs/rewards before commitment
- **Single Responsibility (Implicit)**: Each layer has one purpose
- **Elegance (TIER 3)**: Clear separation reduces coupling

### Implementation Details

**Bridge Enforcement:**
```csharp
// ChoiceTemplate routes execution
switch (choice.ActionType) {
  case Instant:
    // Stay strategic: Apply rewards, advance situation
    break;
  case Navigate:
    // Stay strategic: Move player, may advance situation
    break;
  case StartChallenge:
    // Cross bridge: Store pending context, spawn tactical session
    break;
}
```

**Pending Context Pattern:**
```csharp
// Bridge crossing stores strategic context
PendingChallengeContext {
  ParentSceneId,
  ParentSituationId,
  OnSuccessReward,  // Applied if tactical victory
  OnFailureReward   // Applied if tactical defeat
}
// Both outcomes advance progression (no soft-locks)
```

---

## ADR-004: Parse-Time Translation via Catalogues (No Runtime String Matching)

### Status
**Accepted** - Core content pipeline architecture

### Context

**The AI Generation Problem:**
AI-generated content (procedural generation, LLM-created entities) CANNOT specify absolute mechanical values because AI doesn't know:
- Current player progression level (Level 1 vs Level 10)
- Existing game balance (what items/cards already exist)
- Global difficulty curve (early game vs late game tuning)
- Economy state (coin inflation, resource scarcity)

**The Authoring Problem:**
Hand-authoring every scene with exact numeric values (stat thresholds, coin costs) creates:
- Content explosion (10 services × 50 NPCs = 500 manual tuning entries)
- Maintenance nightmare (bug in formula requires fixing 500 files)
- Balancing hell (change progression curve, rebalance all content)

### Decision

**THREE-PHASE CONTENT PIPELINE** with parse-time translation:

**Phase 1: JSON Authoring (Categorical Properties)**
- Authors/AI write descriptive properties: "Friendly" NPC, "Premium" quality, "Equal" power dynamic
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

### Consequences

**Positive:**
- **AI Generation**: AI can generate balanced content (describe categorically, system translates to balanced numbers)
- **Minimal Authoring**: Just specify entity type, not 50 numeric values
- **Universal Formulas**: One negotiation archetype scales to all contexts
- **Dynamic Scaling**: Change multiplier, all scenes rebalance automatically
- **Zero Runtime Overhead**: Translation happens once at parse-time

**Negative:**
- **No Hand-Tuning**: Cannot adjust specific instances (all scaling formulaic)
- **Universal Scaling**: Must design formulas that work across ALL contexts
- **Parse-Time Cost**: One-time cost during load screen (acceptable)

**FORBIDDEN Forever:**
- ❌ Runtime catalogue calls (parse-time ONLY)
- ❌ String matching on IDs or property names
- ❌ Dictionary<string, int> for costs/requirements
- ❌ ID-based logic for routing

### Formula Example

```
Base archetype: StatThreshold = 5, CoinCost = 8

Context: Friendly NPC (0.6×), Premium Quality (1.6×), Equal Power (1.0×)

Scaled values:
- StatThreshold: 5 × 0.6 × 1.0 = 3 (friendly = easier)
- CoinCost: 8 × 1.6 = 13 (premium = more expensive)

Same archetype, contextually appropriate difficulty.
```

### Alternatives Considered

**Option 1: Hardcoded Absolute Values in JSON**
- Rejected: Breaks AI generation, prevents scaling, requires manual tuning for every instance

**Option 2: Runtime Catalogue Lookups**
- Rejected: Performance cost every query, enables string-matching antipatterns
- Parse-time translation eliminates runtime overhead

**Option 3: String-Keyed Cost Dictionaries**
- Rejected: `Cost["coins"]` enables magic strings, runtime errors, no compile-time safety
- Strongly-typed properties: `CoinCost` property instead

**Option 4: AI Generates Exact Numeric Values**
- Rejected: AI doesn't know global game state, will create imbalanced content
- Categorical properties let AI describe RELATIVELY without breaking balance

### Principle Alignment

- **Single Source of Truth (TIER 1)**: Catalogues are canonical translation mechanism
- **Elegance (TIER 3)**: One formula affects all instances uniformly
- **Type Safety (Implicit)**: Enums over strings, properties over dictionaries

### Related Patterns

- **Catalogue Pattern**: Static classes, context-aware, deterministic, relative preservation
- **Archetype Composition**: Situation archetypes use baseline values, catalogues scale by context
- **Marker Resolution**: Related but distinct (runtime GUID resolution for self-contained resources)

---

## ADR-005: Blazor ServerPrerendered Mode (Double-Rendering Lifecycle)

### Status
**Accepted** - Required for performance UX

### Context

Blazor Server offers two rendering modes:
- **Server**: Single render after SignalR connection established (slower initial load)
- **ServerPrerendered**: Prerender static HTML, then establish SignalR (faster perceived load)

ServerPrerendered comes with complexity: ALL component lifecycle methods run TWICE:
1. First render: Server-side prerendering (generates static HTML)
2. Second render: After establishing interactive SignalR connection

This creates risks:
- Duplicate state initialization (coins doubled, messages shown twice)
- Double event subscriptions (memory leaks)
- Non-idempotent operations break

### Decision

**Use Blazor ServerPrerendered mode** despite double-rendering complexity.

**Idempotence Requirements:**
All initialization code MUST be idempotent:
- Check flags before mutating state (`IsGameStarted`)
- Never add duplicate messages (use flags)
- Protect resource initialization (prevent doubling coins/health)
- Manage event subscriptions carefully (avoid double subscription)

**Implementation Pattern:**
```csharp
public async Task StartGameAsync()
{
    if (_gameWorld.IsGameStarted)
    {
        return; // Already initialized, skip
    }
    // ... initialization code
    _gameWorld.IsGameStarted = true;
}
```

**Safe Patterns:**
- All services as Singletons (persist across renders)
- GameWorld maintains state across both render phases
- Read-only operations safe to run multiple times
- User actions only happen after interactive phase

### Consequences

**Positive:**
- **Faster Initial Load**: User sees content immediately (better UX)
- **Reduced Perceived Latency**: Static HTML renders before SignalR handshake
- **Better SEO**: If ever needed (static HTML visible to crawlers)
- **Professional Polish**: No "blank screen then content" flash

**Negative:**
- **Double Execution**: All lifecycle methods run twice (debugging confusion)
- **Idempotence Required**: Must design all initialization to be repeatable safely
- **Complexity**: Developers must understand double-rendering lifecycle
- **Debugging**: Breakpoints hit twice, console logs appear twice

**Architecture Requirements:**
- GameWorld singleton persists across render phases
- Initialization guarded by flags (`IsGameStarted`)
- No state mutations without checks
- Services stateless (operate on GameWorld, hold no state)

### Alternatives Considered

**Option 1: Server Mode (Single Render)**
- Rejected: Slower initial load, worse UX
- User sees blank screen during SignalR connection

**Option 2: Blazor WebAssembly**
- Rejected: Larger download size, client-side limitations
- Server-side state model better for single-player game

**Option 3: Static Site Generation**
- Rejected: No persistent WebSocket, no real-time state updates
- Game requires continuous server-side state

### Principle Alignment

- **Playability (TIER 2)**: Fast initial load improves player experience
- **Single Source of Truth (TIER 1)**: GameWorld singleton maintains consistency across renders
- **Elegance (TIER 3)**: Complexity justified by UX improvement

### Testing Considerations

**Expected Behavior:**
```
[GameFacade.StartGameAsync] Player initialized at Market Square   // First render
[GameFacade.StartGameAsync] Game already started, skipping        // Second render
```

This is EXPECTED and shows idempotence protection working correctly.

---

## ADR-006: Principle Priority Hierarchy (Conflict Resolution Framework)

### Status
**Accepted** - Meta-decision guiding all other ADRs

### Context

Design principles inevitably conflict. Examples:
- HIGHLANDER (fail-fast) vs Playability (graceful degradation)
- Perfect Information vs Tactical Surprise
- Single Source of Truth vs Query Performance
- No Soft-Locks vs Resource Scarcity

Without clear priority, teams make inconsistent decisions or endless debates.

### Decision

**Establish three-tier priority hierarchy** for conflict resolution:

### TIER 1: Non-Negotiable (Never Compromise)

1. **No Soft-Locks**: Always forward progress. If design creates possibility of unwinnable state, redesign completely.
2. **Single Source of Truth**: One owner per entity type. Redundant storage creates desync bugs.

**Rule**: If violating TIER 1, **STOP** - redesign completely.

### TIER 2: Core Experience (Compromise Only with Clear Justification)

3. **Playability Over Compilation**: Game must be testable and playable. Unplayable code is worthless even if architecturally pure.
4. **Perfect Information at Strategic Layer**: Player can calculate strategic decisions. Hidden complexity belongs in tactical layer only.
5. **Resource Scarcity Creates Choices**: Shared resources force trade-offs. Boolean gates insufficient.

**Rule**: Higher tier wins. TIER 2 beats TIER 3.

### TIER 3: Architectural Quality (Prefer but Negotiable)

6. **HIGHLANDER - One Path**: One instantiation path per entity type. Break only when playability demands it.
7. **Elegance Through Minimal Interconnection**: Systems connect at explicit boundaries. Acceptable complexity for critical features.
8. **Verisimilitude**: Relationships match conceptual model. Can bend for gameplay necessity.

**Rule**: Within same tier, find creative solutions satisfying both.

### Consequences

**Positive:**
- **Clear Decisions**: Priority immediately resolves most conflicts
- **Consistency**: Same conflicts resolved same way across codebase
- **Fast Resolution**: No endless debates, hierarchy provides answer
- **Document Rationale**: Tier reference documents WHY decision made

**Negative:**
- **Learning Curve**: Team must internalize hierarchy
- **Occasional Dissatisfaction**: Lower-tier principles sometimes sacrificed
- **Requires Judgment**: Same-tier conflicts need creative solutions

### Conflict Resolution Examples

**Conflict: HIGHLANDER vs Playability**
- TIER 3 vs TIER 2 → **Playability wins**
- Example: Cached Template reference violates HIGHLANDER but acceptable for performance
- Solution: Store authoritative ID + ephemeral cache (not redundant if one is cache)

**Conflict: Perfect Information vs Tactical Surprise**
- Both TIER 2 → **Creative solution satisfying both**
- Solution: Layer separation (strategic = perfect information, tactical = hidden complexity)

**Conflict: No Soft-Locks vs Resource Scarcity**
- Both high priority → **Creative solution satisfying both**
- Solution: Add zero-cost fallback choices (scarcity creates choices, fallback prevents locks)

**Conflict: Single Source of Truth vs Query Performance**
- TIER 1 vs Performance → **TIER 1 wins with pattern**
- Solution: Store BOTH (ID authoritative, object cached/restored on load)

### Decision Framework

When principles conflict:
1. Identify which tier each principle belongs to
2. Higher tier wins
3. Within same tier, find creative solution satisfying both
4. Document decision and reasoning
5. If violating TIER 1, **STOP** - redesign completely

### Anti-Patterns

❌ **WRONG: "It compiles but is unplayable"**
- Compilation necessary but insufficient
- TIER 2 Playability means: Can player actually play? Can we test it?

❌ **WRONG: "Let's null-coalesce everywhere for safety"**
- Hides bugs, violates TIER 1 Single Source of Truth
- Safety comes from crashing obviously, not silently continuing

❌ **WRONG: "Player can soft-lock but it's rare"**
- TIER 1 principle, even 0.1% soft-lock rate unacceptable
- Add safety nets, ensure forward progress always possible

✅ **CORRECT: "This violates elegance but prevents soft-locks"**
- TIER 1 beats TIER 3, accept complexity if ensures forward progress

✅ **CORRECT: "Hierarchical data adds complexity but matches player mental model"**
- Both TIER 3, use judgment (if significantly improves comprehension, accept complexity)

### Alternatives Considered

**Option 1: No Priority (Case-by-Case Decisions)**
- Rejected: Leads to inconsistency, endless debates, no clear framework

**Option 2: Flat Priority (All Principles Equal)**
- Rejected: Doesn't resolve conflicts, forces compromise on critical principles

**Option 3: More Granular Tiers (5-6 tiers)**
- Rejected: Over-complexity, 3 tiers sufficient for decision-making

### Principle Alignment

This meta-decision IS the alignment framework for all other principles.

---

## 9.2 Summary

These six ADRs represent the foundational architectural decisions shaping Wayfarer:

1. **Infinite A-Story**: Never-ending journey without resolution
2. **Resource Arithmetic**: Perfect information through numeric comparisons
3. **Two-Layer Architecture**: Strategic (perfect info) vs Tactical (hidden complexity)
4. **Parse-Time Translation**: Catalogues enable AI generation and dynamic scaling
5. **ServerPrerendered Mode**: Fast initial load despite double-rendering complexity
6. **Principle Priority**: Three-tier hierarchy resolves design conflicts

**Common Themes:**
- Player agency through perfect information
- Resource scarcity creating impossible choices
- No soft-locks ever (TIER 1 non-negotiable)
- Elegance through clear separation of concerns
- AI content generation support via categorical properties

---

## Related Documentation

- **01_introduction_and_goals.md** - Quality goals driving these decisions
- **04_solution_strategy.md** - Strategic approaches implementing these decisions
- **02_constraints.md** - Technical constraints affecting decisions
- **10_quality_requirements.md** - Quality scenarios validating decisions
- **08_crosscutting_concepts.md** - Patterns and principles underlying decisions
