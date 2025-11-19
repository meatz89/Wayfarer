# Arc42 Section 9: Architecture Decisions

## 9.1 Overview

This section documents major architectural decisions (ADRs) that shape the Wayfarer system. Each decision record follows the standard format: Context, Decision, Consequences, and Alternatives Considered.

---

## ADR-001: Infinite A-Story Without Resolution (Frieren Principle)

> **For game design rationale and player experience philosophy**, see [design/11_design_decisions.md](design/11_design_decisions.md) DDR-001 (Infinite A-Story).

### Status
**Accepted** - Core architectural pattern

### Context

Traditional narrative games with fixed endings create technical challenges:
- Content exhaustion (finite authored content eventually completed)
- Post-ending system states (game continues but narrative complete)
- Limited replayability (same content pipeline on replay)
- Natural stopping points (players leave after completion)

The system requires indefinite content generation supporting ongoing play without arbitrary endpoints.

### Decision

**Implement infinite procedurally-generated A-story** using two-phase content pipeline:

**Phase 1: Authored Foundation (A1-A10)**
- Hand-crafted scenes loaded from JSON
- Fixed sequence establishing system patterns
- Triggers procedural generation when complete

**Phase 2: Procedural Generation (A11+ → ∞)**
- Scene generation via archetype catalog selection
- Entity resolution using categorical filters (no hardcoded IDs)
- AI narrative generation via Ollama integration
- Escalating difficulty scaling over progression
- Content validation ensures structural guarantees (no soft-locks)

### Consequences

**Technical Requirements:**
- **Procedural Generation Pipeline**: Archetype catalog, entity resolution, validation framework
- **Content Validation**: 100% of generated scenes must pass structural validation (zero-requirement fallback, forward progression)
- **Scaling System**: Difficulty/rewards scale with player progression without ceiling
- **AI Integration**: Ollama API for narrative text generation (optional enhancement)
- **State Management**: Track progression tier, escalation level, generation history

**Architecture Implications:**
- SceneGenerator service for procedural content creation
- ContentValidator for structural guarantees
- ArchetypeCatalog for reusable patterns
- Generation seeded by player history (locations visited, NPCs met)
- All generated content validated before spawn (fail-fast if violations)

### Alternatives Considered

**Option 1: Traditional Ending + Post-Game**
- Rejected: Content exhaustion, system state complexity after ending

**Option 2: Multiple Endings with Branching**
- Rejected: Fixed content pool, high authoring cost, eventual exhaustion

**Option 3: Repeatable Endgame Loop ("New Game+")**
- Rejected: State reset complexity, progress invalidation issues

### Principle Alignment

- **No Soft-Locks (TIER 1)**: All generated scenes validated for guaranteed forward progression
- **Playability Over Compilation (TIER 1)**: Content validation ensures reachability
- **Single Source of Truth (TIER 1)**: Generation history tracked in GameWorld

---

## ADR-002: Resource Arithmetic Over Boolean Gates (Requirement Inversion)

> **For complete resource economy design and impossible choices philosophy**, see [design/05_resource_economy.md](design/05_resource_economy.md) and [design/11_design_decisions.md](design/11_design_decisions.md) DDR-004 (Tight Economy).

### Status
**Accepted** - Fundamental data model pattern

### Context

Progression systems require visibility model for content availability. Boolean flag patterns (`if player.HasRope`) hide upcoming content and prevent strategic planning.

### Decision

**All requirements stored as numeric thresholds enabling arithmetic comparison.** Resources modeled as integer properties on Player entity. Choices specify requirements as numeric values compared at runtime.

**Data Model:**
```csharp
public class ChoiceTemplate {
    public int StatThreshold { get; set; }      // Numeric requirement
    public int CoinCost { get; set; }           // Numeric cost
    public int StaminaCost { get; set; }        // Numeric cost
}

public class Player {
    public int Rapport { get; set; }            // Numeric capability
    public int Coins { get; set; }              // Numeric resource
    public int Stamina { get; set; }            // Numeric resource
}

// Evaluation uses arithmetic
bool isAffordable = player.Rapport >= choice.StatThreshold
                 && player.Coins >= choice.CoinCost
                 && player.Stamina >= choice.StaminaCost;
```

### Consequences

**Architecture Implications:**
- All resources stored as `int` properties (never bool, never Dictionary<string, int>)
- UI can display exact requirements and current values
- Validation logic uses arithmetic comparison throughout codebase
- No boolean "completed" flags for progression gates

**Related Patterns:**
- Perfect Information principle (Section 8.3.2 Principle 10)
- Guaranteed Progression Pattern (zero-requirement fallbacks)
- Catalogue Pattern (translate categorical properties to numeric thresholds)

---

## ADR-003: Two-Layer Architecture (Strategic vs Tactical)

> **For player experience rationale and design philosophy**, see [design/11_design_decisions.md](design/11_design_decisions.md) DDR-005 (Two-Layer Separation).

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

> **For archetype system design and content generation patterns**, see [design/07_content_generation.md](design/07_content_generation.md).

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
- No Soft-Locks vs Resource Scarcity
- Maintainability vs Conciseness

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
- Example: Template reference in Situation enables lazy instantiation (playability pattern)
- Solution: Store authoritative ID + template reference for deferred materialization
- Note: NOT for performance optimization, for playability (deferred instantiation pattern)

**Conflict: Perfect Information vs Tactical Surprise**
- Both TIER 2 → **Creative solution satisfying both**
- Solution: Layer separation (strategic = perfect information, tactical = hidden complexity)

**Conflict: No Soft-Locks vs Resource Scarcity**
- Both high priority → **Creative solution satisfying both**
- Solution: Add zero-cost fallback choices (scarcity creates choices, fallback prevents locks)

**Conflict: Single Source of Truth vs Query Performance**
- **NOT A REAL CONFLICT** in this game's context (false dichotomy)
- This game: synchronous, browser-based, single-player, n=20 entities
- List scan of 20 items: 0.001ms, Dictionary lookup: 0.0001ms, difference: 0.0009ms (unmeasurable)
- Browser render frame: 16ms (16,000× slower), human reaction: 200ms+ (200,000× slower)
- **Resolution**: Always choose Single Source of Truth, ignore performance
- No caching, no indexing, no Dictionary - use simple List<T> with LINQ queries
- See: 10_quality_requirements.md Section 10.1.1 (Non-Functional Quality Requirements)

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

## ADR-007: Hex-Based Spatial Architecture (ID Elimination)

> **For complete spatial hierarchy and location relationships**, see [03_context_and_scope.md](03_context_and_scope.md) and [05_building_block_view.md](05_building_block_view.md).

### Status
**Accepted** - Fundamental architectural pattern (currently violated, requires refactoring)

### Context

**Current Implementation (WRONG):**
- Domain entities store synthetic ID strings (`NPC.ID`, `RouteOption.OriginLocationId`)
- JSON files contain ID cross-references (`"locationId": "common_room"`, `"venueId": "brass_bell_inn"`)
- Parsers perform ID lookups to resolve references (`gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId)`)
- Redundant storage of both IDs and object references (RouteOption comment: "HIGHLANDER Pattern A (BOTH ID + Object)")
- Routes manually defined in JSON with origin/destination ID references

**Problems with ID-Based Architecture:**
1. **Serialization Leakage**: IDs are JSON parsing artifacts polluting domain entities
2. **Redundancy**: Entities store both ID strings AND object references
3. **Violations Enabled**: Composite IDs, ID parsing, GetHashCode misuse
4. **Manual Route Definition**: Routes hardcoded in JSON instead of generated procedurally

**Architectural Intent (from code comments):**
- Location.cs line 15: *"HIGHLANDER: Location.HexPosition is source of truth, Hex.LocationId is derived lookup"*
- NPCParser.cs line 107: *"HIGHLANDER: ID is parsing artifact"*

**System Context:**
- Blazor Server in-process execution (no serialization between frontend/backend)
- Hex-based world map for spatial scaffolding
- Procedural route generation via A* pathfinding
- Natural keys available (NPC.Name, Location.Name are unique)

### Decision

**Eliminate synthetic IDs from domain entities. Use hex-based spatial positioning and object references for all relationships.**

**Exception:** Template IDs are acceptable (SceneTemplate.Id, SituationTemplate.Id) because templates are immutable archetypes, not mutable entity instances. Templates are content definitions, not game state.

**JSON Layer:**
- Locations defined with hex coordinates (Q, R) indicating spatial position
- NPCs defined with hex coordinates (where they spawn), NOT locationId cross-reference
- Routes NOT defined in JSON - generated procedurally from hex grid
- Hex grid defines terrain, danger levels, traversability at each coordinate

**Parser Layer:**
- Resolves spatial relationships via hex coordinate matching
- Builds object graph using categorical properties (EntityResolver.FindOrCreate)
- Creates object references based on categorical matching
- Parser queries existing entities by categorical properties, creates new if no match

**Domain Layer:**
- Entities have object references, NOT ID strings
- NPC has `Location` object reference (no `ID`, `LocationId` properties)
- RouteOption has `OriginLocation`, `DestinationLocation` objects (no ID strings)
- Location.HexPosition (AxialCoordinates) is spatial source of truth
- Player has `List<Obligation>` (not `List<string> ObligationIds`)

**Runtime:**
- Routes generated procedurally via pathfinding: `Origin.HexPosition → Destination.HexPosition`
- Travel system navigates hex grid terrain and danger levels
- No hardcoded route definitions - all routes emerge from spatial data
- Entity queries use categorical properties (profession, personality, location, tier)

### Consequences

**Positive:**
- **Eliminates Redundancy**: No duplicate storage (ID string + object reference)
- **Prevents Violations**: No composite IDs, ID parsing, or GetHashCode misuse possible
- **Procedural Generation**: Routes emerge from spatial data, not manual authoring
- **Clear Domain Model**: Entities model game concepts, not database schemas
- **Compile-Time Safety**: Object references catch errors at compile time vs runtime ID lookups

**Negative:**
- **Requires Refactoring**: Massive codebase change across JSON, DTOs, parsers, domain, services
- **Migration Complexity**: Must update all 5 layers simultaneously (JSON → DTO → Parser → Entity → Services)
- **Breaking Change**: Existing JSON files need restructuring (add Q,R coordinates, remove ID cross-references)

**Technical Requirements:**
- Add Q,R properties to LocationDTO, NPCDTO
- Remove ID properties from all domain entities
- Refactor parsers to use spatial resolution instead of ID lookups
- Remove 04_connections.json (routes generated procedurally)
- Update services to use object references instead of ID strings
- Add unit tests for spatial resolution logic

**Migration Strategy:**
1. **Phase 1: Fix Immediate Violations** (Quick wins)
   - NPC.cs line 148: Replace `ID.GetHashCode()` with dedicated `DeterministicSeed` property
   - HexRouteGenerator.cs: Replace composite IDs with opaque GUIDs, add origin/destination object properties
2. **Phase 2: JSON Restructuring** (Data layer)
   - Add Q,R coordinates to all location JSON (source of truth for spatial position)
   - Add Q,R coordinates to all NPC JSON (where they spawn)
   - Remove locationId, venueId cross-references
   - Delete 04_connections.json (routes generated procedurally)
3. **Phase 3: Domain Entity Cleanup** (Model layer)
   - Remove ID properties from NPC, RouteOption, Location
   - Remove LocationId, OriginLocationId, DestinationLocationId redundant properties
4. **Phase 4: Parser Refactoring** (Resolution layer)
   - Spatial resolution: Match entities by hex coordinates
   - Build object graph during parsing
   - Eliminate ID lookup patterns
5. **Phase 5: Service Layer Updates** (Application layer)
   - Replace ID parameters with object parameters
   - Remove `GetById` methods, use object references
   - Update frontend to pass objects instead of ID strings

### Alternatives Considered

**Option 1: Keep IDs for "Performance"**
- Rejected: O(1) lookup vs O(n) scan irrelevant for collections of size 20-100
- Premature optimization (see DOMAIN COLLECTION PRINCIPLE in CLAUDE.md)
- Complexity cost > performance benefit in this context

**Option 2: Hybrid Approach (IDs in DTOs, Objects in Domain)**
- Partially implemented currently (redundant storage pattern)
- Rejected: Redundancy creates maintenance burden, enables violations
- Better to eliminate IDs completely

**Option 3: Lazy Loading via ID References**
- Rejected: No database in this architecture, entities loaded at startup
- Blazor Server in-process, no serialization boundary requiring lazy loading

**Option 4: Keep ID Cross-References in JSON, Resolve at Parse Time**
- Current implementation (being replaced)
- Rejected: JSON structure should reflect spatial reality (hex coordinates), not parsing convenience

### Principle Alignment

- **Single Source of Truth (TIER 1)**: Location.HexPosition is spatial source of truth, eliminates ID redundancy
- **No Soft-Locks (TIER 1)**: Procedural route generation ensures connectivity between all reachable locations
- **Playability Over Compilation (TIER 2)**: Routes must be actually traversable, not just defined in JSON
- **HIGHLANDER (TIER 3)**: One source of truth for entity positioning (hex coordinates), one relationship type (object references)
- **Verisimilitude (TIER 3)**: Spatial relationships match player mental model (hex grid positions)

### Current Violations

1. **NPC.cs line 148**: `(currentDay * ID.GetHashCode()) % ExchangeDeck.Count` - Misusing ID for randomization
2. **HexRouteGenerator.cs**: `routeId = $"route_{origin.Id}_{destination.Id}"` - Composite ID generation
3. **All JSON files**: `"locationId": "common_room"` - ID cross-references instead of hex coordinates
4. **RouteOption.cs lines 58-63**: Redundant storage of both ID strings and object references
5. **04_connections.json**: Manual route definitions instead of procedural generation

### Implementation Notes

**Entity Pattern Example:**
```csharp
// CORRECT - NO ID, object references only
public class NPC
{
    // NO ID property
    public string Name { get; set; }
    public Location Location { get; set; }  // Object reference
    public Professions Profession { get; set; }  // Categorical property
    public PersonalityType PersonalityType { get; set; }  // Categorical property
}

// Parser uses categorical properties to find/create
Location location = EntityResolver.FindOrCreateLocation(new PlacementFilter
{
    Purpose = dto.Purpose,
    Safety = dto.Safety,
    LocationProperties = dto.Properties
});
npc.Location = location;  // Store object reference
```

**Procedural Route Generation Example:**
```csharp
// CORRECT - Routes generated from hex coordinates
List<AxialCoordinates> hexPath = pathfinder.FindPath(
    origin.HexPosition,
    destination.HexPosition,
    hexMap);

RouteOption route = new RouteOption
{
    OriginLocation = origin,  // Object reference
    DestinationLocation = destination,  // Object reference
    HexPath = hexPath,  // Spatial path
    DangerRating = hexPath.Sum(coord => hexMap.GetHex(coord).DangerLevel)
};
```

---

## 9.2 Summary

These seven ADRs represent the foundational architectural decisions shaping Wayfarer:

1. **Infinite A-Story**: Never-ending journey without resolution
2. **Resource Arithmetic**: Perfect information through numeric comparisons
3. **Two-Layer Architecture**: Strategic (perfect info) vs Tactical (hidden complexity)
4. **Parse-Time Translation**: Catalogues enable AI generation and dynamic scaling
5. **ServerPrerendered Mode**: Fast initial load despite double-rendering complexity
6. **Principle Priority**: Three-tier hierarchy resolves design conflicts
7. **Hex-Based Spatial Architecture**: Object references and spatial positioning, no synthetic IDs

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
