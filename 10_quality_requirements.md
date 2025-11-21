# Arc42 Section 10: Quality Requirements

## 10.1 Quality Tree

The Wayfarer quality model follows a three-tier hierarchy prioritizing requirements critical to player experience and system integrity.

```
Wayfarer Quality Goals
│
├─ TIER 1: Non-Negotiable (System Integrity)
│  ├─ No Soft-Locks Ever
│  ├─ Single Source of Truth
│  └─ Playability Over Compilation
│
├─ TIER 2: Core Experience (Player Value)
│  ├─ Strategic Depth Through Impossible Choices
│  ├─ Perfect Information at Strategic Layer
│  └─ Elegance Over Complexity
│
└─ TIER 3: Architectural Quality (Long-Term Maintainability)
   ├─ Verisimilitude in All Systems
   ├─ Maintainability Over Performance
   └─ Clarity Over Cleverness
```

---

## 10.1.1 Non-Functional Quality Requirements

### Game Context (Why Performance Doesn't Matter)

Wayfarer is a **synchronous, browser-based, single-player, turn-based narrative game** with fundamentally different quality priorities than real-time, multiplayer, or high-scale systems.

**This game is:**
- Synchronous (single-threaded execution, no concurrency)
- Browser-based (browser render time dominates, 16ms+ per frame)
- Single-player (one human making decisions at human speed: 200ms+ reaction time)
- Minimal scale (20 NPCs, 30 Locations, 50 Items, 10 active Scenes maximum)
- Turn-based narrative (seconds between player actions while reading text)

**Performance Reality:**
- Typical collection sizes: 20 NPCs, 30 Locations, 50 Items, 10 Scenes
- Linear scan of 100 items: ~0.001 milliseconds (one microsecond)
- Browser render frame: 16+ milliseconds (16,000× slower than scan)
- Human reaction time: 200+ milliseconds (200,000× slower than scan)
- Network latency: 50-200 milliseconds (even localhost)

**Conclusion:** Performance optimization of data structures provides unmeasurable benefit in a system where human cognition (200ms+) and browser rendering (16ms+) dominate timing.

### Priority Hierarchy

**TIER 1 (Critical):**
1. **Maintainability** - Code must be readable, debuggable, and modifiable by future developers
2. **Correctness** - System behavior must match domain semantics exactly
3. **Testability** - All business logic must be verifiable through automated tests

**TIER 2 (Important):**
4. **Clarity** - Code should read like prose, expressing intent directly
5. **Debuggability** - Errors should fail-fast with clear stack traces
6. **Domain Alignment** - Technical implementation should mirror game design concepts

**TIER 3 (Nice to Have):**
7. **Conciseness** - Minimize ceremony while preserving clarity

**NOT REQUIRED (Explicitly Deprioritized):**
- ❌ **Performance** - Optimization provides no measurable benefit at current scale
- ❌ **Scalability** - Game design constrains scale to 10-100 entities per collection
- ❌ **Concurrency** - Single-player, single-threaded execution model
- ❌ **Cleverness** - Sophisticated algorithms add complexity without benefit

### Architectural Principles

**Principle 1: Domain-Driven Collections**
- Use `List<T>` for all entity collections (NPCs, Locations, Scenes, Items)
- Never use `Dictionary<TKey, TValue>` or `HashSet<T>` for domain entities
- Rationale: Dictionary optimizes for O(1) lookup, which is irrelevant for n=20 entities

**Principle 2: LINQ for Queries**
- Use declarative LINQ for all collection queries (`.Where()`, `.FirstOrDefault()`, `.Select()`)
- Never use imperative loops with early returns
- Rationale: LINQ reads like English, composes cleanly, and is easier to test

**Principle 3: Fail-Fast Error Handling**
- Let null-reference exceptions surface immediately at call site
- Never use `TryGetValue()` patterns that defer errors
- Rationale: Early failures produce clear stack traces pointing to root cause

**Principle 4: Explicit Domain Types**
- Use strongly-typed entities with explicit properties
- Never use `Dictionary<string, object>` or type erasure patterns
- Rationale: Compiler catches type errors at compile time, not runtime

**Principle 5: Semantic Honesty**
- Collections should represent domain concepts (entities), not technical structures (indexes)
- Method names must match actual behavior exactly
- Rationale: Code should be self-documenting through honest naming

---

## 10.2 Quality Scenarios

Each quality goal translates into concrete, testable scenarios. Scenarios follow the format: Context → Stimulus → Response.

---

### QS-001: No Soft-Locks Ever (TIER 1)

**Quality Goal**: Player must ALWAYS have at least one viable path forward, regardless of previous choices or resource state.

#### Scenario 1.1: Zero-Resource A-Story Progression

**Context:**
- Player at A-story scene final situation
- Player resources: Coins 0, Stamina 0, Focus 0, Resolve 0
- Player stats: All below any threshold requirements
- No items in inventory

**Stimulus:**
- Player needs to advance A-story to next scene

**Response:**
- System presents at least one choice with:
  - Zero resource requirements
  - Zero stat requirements
  - Guaranteed success (Instant action OR Challenge with assured victory)
  - Advances to next A-scene
- **Metric**: 100% of A-story situations have ≥1 zero-requirement choice
- **Validation**: Automated test suite verifies all A-story JSON

#### Scenario 1.2: Challenge Failure Still Progresses

**Context:**
- Player in A-story situation with challenge-path choice
- Player selects challenge despite low stats
- Tactical challenge fails (didn't reach threshold)

**Stimulus:**
- Challenge ends with failure outcome

**Response:**
- OnFailureReward applied (may have costs, e.g., "pay extra 5 coins")
- Scene advances to next situation OR spawns next A-scene
- Player continues forward (different entry state, but progresses)
- **Metric**: 100% of A-story challenge paths have OnFailureReward that advances
- **Validation**: Parser validation rejects challenge without OnFailureReward

#### Scenario 1.3: Infinite Generation Never Soft-Locks

**Context:**
- Player 50+ hours into game
- Procedural A-story generation active
- Generated scene with multiple situations

**Stimulus:**
- System generates next A-scene via catalogues

**Response:**
- Generated scene passes structural validation:
  - Every situation has ≥1 zero-requirement choice
  - Final situation's all paths spawn next A-scene
  - No circular dependencies (Scene A requires Scene B, Scene B requires Scene A)
- **Metric**: 100% of generated scenes pass validation before spawn
- **Validation**: ContentValidator.ValidateSceneStructure() must pass

---

### QS-002: Single Source of Truth (TIER 1)

**Quality Goal**: Every piece of game state has exactly ONE canonical storage location. No redundant or parallel state tracking.

#### Scenario 2.1: Player Location Consistency

**Context:**
- Player navigates to new location
- Multiple systems need to know player location (UI, SceneFacade, SpawnFacade)

**Stimulus:**
- GameFacade.NavigateToLocation(newLocation)  // Pass object, NOT ID

**Response:**
- GameWorld.Player.CurrentLocation updated (ONLY place storing location - object reference)
- All systems query GameWorld for location, never cache independently
- **Metric**: Zero properties named "CurrentLocationId" exist (use CurrentLocation object reference)
- **Validation**: Code search confirms Player.CurrentLocation is object reference, NOT ID string
- **NO ENTITY INSTANCE IDs**: Player.CurrentLocation is Location object, never string ID

#### Scenario 2.2: Scene State Synchronization

**Context:**
- Scene has multiple situations
- Scene.CurrentSituation points to active situation
- Situation state changes (completes, advances)

**Stimulus:**
- Player completes choice, situation advances

**Response:**
- Scene.CurrentSituation updated via Scene.AdvanceToNextSituation() (single method)
- No parallel "activeSituationId" stored elsewhere
- UI queries Scene.CurrentSituation for display
- **Metric**: Zero duplicate situation tracking outside Scene entity
- **Validation**: No properties named "ActiveSituation" or "CurrentSituation" in services/facades

#### Scenario 2.3: Resource State Authority

**Context:**
- Player resources (Coins, Health, Stamina) modified during gameplay
- Multiple systems need current values (UI, ResourceFacade, choice validation)

**Stimulus:**
- Choice execution applies resource costs

**Response:**
- GameWorld.Player.Resources updated (ONLY authoritative source)
- ResourceFacade operates on GameWorld.Player (no internal state)
- UI queries GameWorld.Player for display (no cached copies)
- **Metric**: Zero resource properties outside GameWorld.Player
- **Validation**: ResourceFacade class has zero fields (stateless service)

---

### QS-003: Playability Over Compilation (TIER 1)

**Quality Goal**: Code that compiles but cannot be played/tested is unacceptable. Every feature must be reachable and testable.

#### Scenario 3.1: New Scene Reachability

**Context:**
- Developer adds new scene JSON to content package
- Scene has correct spawn conditions and situations
- Game compiles successfully

**Stimulus:**
- QA tester attempts to reach scene from game start

**Response:**
- Tester can trace exact path: Start → Actions → Scene activation
- All intermediate scenes/situations reachable without external tools
- **Metric**: 100% of authored scenes reachable via gameplay path
- **Validation**: Manual QA checklist + automated graph traversal test

#### Scenario 3.2: Challenge Deck Completability

**Context:**
- Developer creates new challenge deck for tactical session
- Deck has victory condition (SituationCard with threshold)
- Code compiles successfully

**Stimulus:**
- QA tester starts challenge session with deck

**Response:**
- Deck contains cards that can reach threshold:
  - Momentum-building cards for Social challenges
  - Progress-building cards for Mental challenges
  - Breakthrough-building cards for Physical challenges
- **Metric**: All challenge decks mathematically solvable
- **Validation**: DeckValidator checks card effects sum to threshold minimum

#### Scenario 3.3: Missing Dependency Detection

**Context:**
- JSON references entity by ID (locationId, npcId, cardId)
- Referenced entity doesn't exist in loaded content

**Stimulus:**
- Game starts, content loading begins

**Response:**
- Parser throws PackageLoadException with clear message:
  - "Scene 'inn_lodging_001' references NPC 'elena' which doesn't exist"
- Game doesn't load with broken references (fail-fast)
- **Metric**: Zero broken references reach runtime
- **Validation**: All ID references validated at parse-time

---

### QS-004: Strategic Depth Through Impossible Choices (TIER 2)

> **For game design philosophy of impossible choices and resource economy**, see [design/05_resource_economy.md](design/05_resource_economy.md).

**Quality Goal**: System enforces resource scarcity requiring strategic prioritization. Player cannot pursue all available options simultaneously.

#### Scenario 4.1: Resource Competition Validation

**Context:**
- Player has finite resources (time blocks, coins, stamina)
- Multiple actions available requiring those resources
- System tracks action availability and completion

**Stimulus:**
- Player allocates resources across competing actions

**Response:**
- System prevents pursuing all options (insufficient resources)
- UI displays exact costs and current resource levels
- Validation logic enforces resource constraints
- **Metric**: Average player completes 40-60% of available daily actions
- **Validation**: Telemetry tracks action completion rates, resource utilization patterns

#### Scenario 4.2: Stat Distribution Analysis

**Context:**
- Player progresses through game, advancing stats
- System tracks stat advancement across all players
- Multiple stat-gated choices exist throughout game

**Stimulus:**
- System collects player stat distributions at various progression points

**Response:**
- Stat advancement system enforces specialization (cannot max all stats)
- Telemetry shows variance in stat distributions across player population
- **Metric**: End-game stat distributions show specialization patterns (high variance, not uniform)
- **Validation**: Statistical analysis of player stat profiles

#### Scenario 4.3: Economy Pressure Monitoring

**Context:**
- Player earns and spends coins throughout gameplay
- System tracks coin reserves, income, expenses
- Multiple competing coin sinks exist

**Stimulus:**
- System monitors player economy state over time

**Response:**
- Economy maintains pressure (player rarely has surplus for all options)
- Resource management remains meaningful throughout gameplay
- **Metric**: Player coin reserves remain under 2× typical expense threshold
- **Validation**: Telemetry tracks coin balances, spending patterns, reserve ratios

---

### QS-005: Perfect Information at Strategic Layer (TIER 2)

**Quality Goal**: Player can see exact costs, requirements, and rewards before committing to strategic choices. No hidden gotchas.

#### Scenario 5.1: Choice Cost Visibility

**Context:**
- Player at location viewing available choices
- Multiple choices with different resource costs

**Stimulus:**
- UI displays choices to player

**Response:**
- All costs shown BEFORE selection:
  - "Pay 15 coins" (exact amount)
  - "Stamina -3" (exact deduction)
  - "Requires Rapport 6" (exact threshold, current value shown)
- Current resources displayed: "You have: 12 coins, Stamina 5/10"
- Gap visible: "Requires Rapport 6, you have 4" (need 2 more)
- **Metric**: 100% of strategic choices show exact costs/requirements
- **Validation**: UI review checklist

#### Scenario 5.2: Reward Transparency

**Context:**
- Player selecting choice with visible rewards
- Rewards include resource gains, unlocks, narrative outcomes

**Stimulus:**
- UI displays choice with rewards

**Response:**
- All rewards visible BEFORE execution:
  - "Gain: Coins +10, Understanding +1"
  - "Unlocks: Private Room access"
  - "OnSuccess: Room unlocked / OnFailure: Pay extra 5 coins"
- No "mystery boxes" or "???" rewards
- **Metric**: 100% of rewards shown explicitly (no hidden outcomes)
- **Validation**: UI review checklist

#### Scenario 5.3: Challenge Entry Transparency

**Context:**
- Player considering challenge-path choice
- Choice crosses to tactical layer

**Stimulus:**
- UI displays challenge choice

**Response:**
- Strategic costs shown: "Entry cost: Stamina -2"
- Conditional rewards shown:
  - "OnSuccess: Unlock private room, gain Coins +10"
  - "OnFailure: Pay extra 5 coins"
- Victory condition shown: "Reach Momentum 8 to succeed"
- Player can calculate: "Worth -2 Stamina for chance at +10 coins?"
- **Metric**: 100% of challenge choices show entry costs AND both outcomes
- **Validation**: UI review checklist

---

### QS-006: Elegance Over Complexity (TIER 2)

**Quality Goal**: Systems achieve goals with minimal interconnection. Clear boundaries, minimal coupling.

#### Scenario 6.1: Adding New Tactical System

**Context:**
- Developer wants to add fourth tactical system (e.g., "Spiritual" challenges)
- Existing: Social, Mental, Physical systems

**Stimulus:**
- Implement new tactical system

**Response:**
- Changes required:
  - Create new facade (SpiritualFacade) following pattern
  - Add enum value (ChallengeType.Spiritual)
  - Create cards following existing card structure
  - NO changes to Scene/Situation layer (bridge already generic)
  - NO changes to other tactical systems (parallel, not interdependent)
- **Metric**: <100 lines of integration code (system self-contained)
- **Validation**: Code review verifies no cross-system dependencies

#### Scenario 6.2: Refactoring Resource System

**Context:**
- Developer wants to change how Stamina works
- Current: Simple numeric depletion
- New: Tiered thresholds with different recovery rates

**Stimulus:**
- Refactor ResourceFacade

**Response:**
- Changes contained to:
  - ResourceFacade implementation
  - GameWorld.Player.Resources property structure
  - UI display components (reading new structure)
- NO changes to:
  - Scene/Situation definitions (use costs abstractly)
  - Challenge systems (read resources, don't define behavior)
  - Navigation systems (independent of resources)
- **Metric**: <5% of codebase modified for resource system change
- **Validation**: Git diff analysis

#### Scenario 6.3: Content Pipeline Independence

**Context:**
- Parser loads JSON, creates entities, populates GameWorld
- Developer wants to change JSON format (add new field)

**Stimulus:**
- Add categorical property to JSON schema

**Response:**
- Changes required:
  - Update DTO class with new property
  - Update Parser to read new property
  - Update Catalogue to translate new property
  - Update Entity class with concrete property
- NO changes to:
  - GameWorld collections
  - Service facades (use entity properties)
  - UI components (render entity properties)
- **Metric**: Changes contained to Parsers/ folder + single entity class
- **Validation**: Dependency analysis shows no ripple effects

---

### QS-007: Verisimilitude in All Systems (TIER 3)

**Quality Goal**: Entity relationships and system behavior match player's conceptual model. No backwards explanations.

#### Scenario 7.1: Spatial Hierarchy Comprehension

**Context:**
- New player explores game world
- World structure: Venue → Location → (within-venue instant navigation)

**Stimulus:**
- Player navigates between locations

**Response:**
- Mental model matches implementation:
  - "Inn" venue contains "Common Room", "Private Room", "Stables"
  - Moving within inn is instant (conceptually same building)
  - Traveling to different inn requires route (conceptually different place)
- Explanation feels natural: "Locations within venue are sub-spaces"
- **Metric**: <10% of playtesters confused by spatial navigation
- **Validation**: Playtester feedback survey

#### Scenario 7.2: Scene Ownership Intuition

**Context:**
- Player engages with scene at location
- Scene has multiple situations sequenced across locations

**Stimulus:**
- Player completes situation, scene advances to new location

**Response:**
- Mental model: "This scene continues at new location"
- NOT confused by: "Why is scene at location?" (placement, not ownership)
- Explanation matches code: Scene placed at Locations, not owned by them
- **Metric**: <5% of playtesters ask "Why is this scene appearing here?"
- **Validation**: Playtester comprehension survey

#### Scenario 7.3: Challenge Ephemeral Nature

**Context:**
- Player engages in tactical challenge (conversation, investigation, obstacle)
- Challenge session active with temporary resources

**Stimulus:**
- Challenge completes (success or failure)

**Response:**
- Mental model: "Challenge was a moment in time, now it's done"
- Understand: Session resources (Momentum, Progress) don't persist
- Persistent resources (Understanding, NPC bonds) carry forward
- Explanation natural: "Conversations happen and end, relationships persist"
- **Metric**: <5% of playtesters expect Momentum to persist between conversations
- **Validation**: Playtester mental model survey

---

### QS-008: Maintainability Over Performance (TIER 3)

**Quality Goal**: Code optimizes for long-term maintainability. Performance optimizations are rejected if they harm readability or debuggability.

#### Scenario 8.1: Entity Collection Storage

**Context:**
- GameWorld stores domain entities (NPCs, Locations, Scenes, Items)
- Developer chooses data structure for entity storage
- Collections contain 10-100 entities maximum

**Stimulus:**
- Code review evaluates data structure choice

**Response:**
- Uses `List<T>` for all entity collections (NOT Dictionary/HashSet)
- Queries use LINQ: `.Where()`, `.FirstOrDefault()`, `.Select()`
- Justification: Maintainability (readable queries) outweighs performance (0.001ms difference)
- **Metric**: Zero Dictionary/HashSet usage for domain entity storage
- **Validation**: Code search for `Dictionary<string, NPC>` returns zero results

**Anti-Pattern Example (REJECTED):**
```csharp
// WRONG - Premature optimization
private Dictionary<string, NPC> _npcs;

public NPC GetNPCById(string id)
{
    return _npcs[id]; // KeyNotFoundException if not found
}

public List<NPC> GetFriendlyNPCs()
{
    return _npcs.Values.Where(n => n.Demeanor == Demeanor.Friendly).ToList();
    // Using Dictionary as List with extra steps
}
```

**Correct Pattern (APPROVED):**
```csharp
// CORRECT - Domain-driven collection
private List<NPC> _npcs;

public NPC GetNPCById(string id)
{
    return _npcs.FirstOrDefault(n => n.Id == id);
    // Null if not found, fail-fast at call site
}

public List<NPC> GetFriendlyNPCs()
{
    return _npcs.Where(n => n.Demeanor == Demeanor.Friendly).ToList();
    // Uniform LINQ query pattern
}
```

**Performance Analysis:**
- Dictionary lookup: ~0.0001ms (O(1) hash calculation)
- List scan of 20 items: ~0.001ms (O(n) equality checks)
- Difference: **0.0009 milliseconds** (completely unmeasurable)
- Browser render frame: **16 milliseconds** (16,000× slower)
- Conclusion: Performance benefit is **ZERO** in practice

**Maintainability Analysis:**
- Dictionary: Requires `TryGetValue()` defensive code, confusing debugger view, ID duplication in tests
- List: Declarative LINQ queries, clear debugger view, simple test setup
- Conclusion: Maintainability benefit is **SIGNIFICANT**

#### Scenario 8.2: Code Review for Clarity

**Context:**
- Developer implements new service method
- Method queries GameWorld entities

**Stimulus:**
- Code review evaluates readability

**Response:**
- LINQ queries read like English: "Get NPCs where location matches"
- No clever optimizations or sophisticated algorithms
- Domain concepts map directly to code structure
- **Metric**: Code reviewers understand intent without explanation
- **Validation**: Peer review requires zero clarification questions

**Example (APPROVED):**
```csharp
public List<NPC> GetNPCsAtLocation(string locationId)
{
    return _npcs
        .Where(n => n.CurrentLocation == locationId)
        .OrderBy(n => n.Name)
        .ToList();
    // Reads: "NPCs where current location matches, ordered by name"
}
```

#### Scenario 8.3: Debugging Session Efficiency

**Context:**
- Bug reported: "NPC not appearing at expected location"
- Developer debugs using Visual Studio

**Stimulus:**
- Set breakpoint, inspect GameWorld state

**Response:**
- List debugger view shows immediate entity state:
  ```
  _npcs: List<NPC> (Count = 5)
    [0]: {NPC: Elena, Location: common_room, Demeanor: Friendly}
    [1]: {NPC: Marcus, Location: market_square, Demeanor: Neutral}
  ```
- Developer sees problem IMMEDIATELY (location mismatch visible)
- No need to expand KeyValuePair structures
- **Metric**: Average debug session time reduced by 30% vs Dictionary
- **Validation**: Developer productivity tracking

**Anti-Pattern (Dictionary):**
```
_npcs: Dictionary<string, NPC> (Count = 5)
  [0]: {["npc_001", Wayfarer.GameState.NPC]}  // Must expand to see properties
  [1]: {["npc_002", Wayfarer.GameState.NPC]}  // Extra clicks required
```

---

## 10.3 Quality Metrics Summary

### Automated Validation (Continuous Integration)

| Quality Goal | Metric | Threshold | Enforcement |
|---|---|---|---|
| No Soft-Locks | A-story scenes with zero-req choice | 100% | Parser validation |
| Single Source of Truth | Duplicate state properties | 0 | Code review checklist |
| Playability | Scenes reachable from start | 100% | Graph traversal test |
| Strategic Depth | Player action completion rate | 40-60% | Telemetry analysis |
| Perfect Information | Choices showing costs/rewards | 100% | UI review checklist |
| Elegance | Cross-system dependencies | <5% | Dependency analysis |
| Verisimilitude | Playtester confusion rate | <10% | Survey feedback |
| Maintainability | Dictionary/HashSet for entities | 0 | Code search + review |
| Clarity | LINQ usage for queries | 100% | Code review checklist |

### Manual Validation (Release Checklist)

- **QA Playthrough**: Can tester reach all authored content from game start?
- **Challenge Solvability**: Are all challenge decks mathematically completable?
- **Resource Pressure**: Does economy maintain strategic tension (not too easy/hard)?
- **Mental Model Alignment**: Do playtesters' descriptions match architecture?

### Telemetry Monitoring (Post-Release)

- **Soft-Lock Detection**: Any players stuck >30 minutes without forward progress?
- **Choice Distribution**: Are stat-gated choices accessed by ≥30% of players?
- **Resource Utilization**: Are players maintaining strategic reserves or always broke?
- **Session Length**: Average 30-120 minute sessions (not too short/long)?

---

## 10.4 Quality Goal Trade-Offs

When quality goals conflict, resolve via principle priority (see ADR-006):

| Conflict | Resolution | Rationale |
|---|---|---|
| No Soft-Locks vs Resource Scarcity | Add zero-cost fallback choices | TIER 1 wins, scarcity preserved via poor fallback rewards |
| Perfect Information vs Tactical Surprise | Layer separation | Strategic = perfect info, Tactical = hidden complexity |
| Elegance vs Playability | Accept complexity for critical features | TIER 2 > TIER 3 |
| Maintainability vs Performance | Always choose maintainability | Performance optimization provides zero measurable benefit (n=20, not n=20000) |
| Clarity vs Conciseness | Choose clarity (explicit over implicit) | TIER 2 wins, verbose clear code beats clever terse code |
| Verisimilitude vs Implementation Cost | Hierarchical spatial model | TIER 3 quality worth implementation complexity |

### Non-Conflicts (Not Actually Trade-Offs)

Some apparent conflicts are **false dichotomies** in this game's context:

| Apparent Conflict | Why It's Not Actually a Trade-Off | Resolution |
|---|---|---|
| Single Source of Truth vs Performance | Performance optimization (caching, indexing) provides unmeasurable benefit at game's scale | Use simple List<T>, no caching needed |
| Correctness vs Performance | O(n) vs O(1) lookup saves 0.0009ms in system where human reaction is 200ms | Always choose correctness, ignore performance |
| Debuggability vs Performance | Dictionary makes debugging harder (KeyValuePair expansion) for zero performance gain | Always choose debuggability |

**Key Insight:** In a synchronous, browser-based, single-player, turn-based narrative game with n=20 entities, performance optimization is **premature optimization**. The browser render frame (16ms) and human cognition (200ms+) dominate all timing. Data structure choice (List vs Dictionary) provides **literally zero measurable benefit** while imposing **significant maintainability cost**.

---

## Related Documentation

- **01_introduction_and_goals.md** - Quality goals and stakeholder concerns
- **09_architecture_decisions.md** - ADRs implementing quality requirements
- **02_constraints.md** - Constraints affecting quality achievement
- **08_crosscutting_concepts.md** - Patterns ensuring quality across system
- **CLAUDE.md** - Detailed enforcement of maintainability principles (Dictionary/HashSet antipattern, coding standards)
