# Design Section 11: Design Decision Records (DDRs)

## 11.1 Overview

This section documents major game design decisions (DDRs) that shape Wayfarer's gameplay experience. Each decision record follows the standard format: Context, Decision, Alternatives Considered, Rationale, and Consequences. These records complement the technical Architecture Decision Records (ADRs) in arc42 section 09, focusing specifically on player experience and game design choices.

**Last Updated:** 2025-11 (Post-scene architecture refactoring)

---

## DDR-001: Infinite Procedural A-Story vs Authored Linear Story

### Status
**Active** - Core design pillar

### Context

Traditional RPGs face a fundamental problem: how to end a story satisfyingly while supporting post-ending gameplay. This creates multiple design challenges:

- Arbitrary ending points requiring narrative justification ("why does saving the world end here?")
- Post-ending gameplay awkwardness ("you saved the world, now go collect flowers")
- Limited replayability (same content, same ending)
- Narrative closure pressure forcing rushed conclusions
- Natural player stopping point after ending (players leave when story ends)
- Live game evolution difficulty (endings create departure points)

Additionally, authoring multiple satisfying endings is expensive, yet most players only see one ending per playthrough, leaving 80%+ of ending content unplayed.

### Decision

**The game never ends.** The main storyline (A-story) is an **infinite procedurally-generated spine** that provides structure and progression without resolution.

**Two-Phase Design:**

**Phase 1: Authored Tutorial (A1-A10+)**
- Hand-crafted scenes teaching mechanics (30-60 minutes current, expandable to 2-4 hours)
- Fixed sequence establishing pursuit framework
- Gradual mechanical introduction
- Triggers procedural transition when complete

**Phase 2: Procedural Continuation (A11+ → ∞)**
- Scene archetype selection from catalog (20-30 archetypes)
- Entity resolution via categorical filters (no hardcoded IDs)
- AI narrative generation connecting to player history
- Escalating scope/tier over time (local → regional → continental → cosmic)
- Never ends, never resolves, always deepens

**Narrative Pattern:**
> You travel. You arrive places. You meet people. Each place leads to another. Each person you meet suggests somewhere else worth visiting. The journey itself IS the point, not reaching anywhere specific.

### Alternatives Considered

**Option A: Infinite Procedural (Chosen)**
- **Pros:**
  - Eliminates hardest problem (satisfying ending)
  - Infinite replayability
  - Perfect for live evolution
  - No post-ending awkwardness
  - Player chooses WHEN to pursue A-story
  - Matches "eternal traveler" fantasy
- **Cons:**
  - No narrative closure (some players want endings)
  - Procedural quality must maintain standards
  - Complex validation requirements
  - Balancing without ceiling
- **Why Chosen:** Eliminates ending pressure, enables player agency, supports infinite content, matches core fantasy

**Option B: Traditional Ending + Post-Game (Rejected)**
- **Pros:**
  - Familiar pattern
  - Narrative closure satisfaction
  - Clear progression arc
- **Cons:**
  - Creates "saved the world" disconnect
  - Players leave after ending (natural stopping point)
  - Post-game content feels hollow
  - Replayability limited
- **Why Rejected:** Post-ending gameplay fundamentally awkward, creates player departure point

**Option C: Multiple Endings with Branching (Rejected)**
- **Pros:**
  - Replayability through variation
  - Player choice matters
  - Satisfying conclusions
- **Cons:**
  - Fixed content eventually exhausted
  - High authoring cost for limited play (most endings unseen)
  - Still finite
  - Still creates departure points
- **Why Rejected:** Expensive to create, low engagement per dollar, still finite

**Option D: Repeatable Endgame Loop (New Game+) (Rejected)**
- **Pros:**
  - Replayability
  - Familiar roguelike pattern
  - Progress carries over
- **Cons:**
  - Resets progress (feels artificial)
  - Invalidates earlier content
  - "You've done this before" feeling
  - Not true infinite content
- **Why Rejected:** Artificial reset breaks immersion, not truly infinite

### Rationale

**Design Principle Alignment:**
- Supports infinite player agency (TIER 2: Core Experience)
- Guaranteed progression requirement applies to ALL A-story scenes (TIER 1: No Soft-Locks)
- Matches "eternal traveler" verisimilitude perfectly (TIER 3: Verisimilitude)

**Player Experience Benefits:**
- Player controls pacing (pursue immediately or explore side content first)
- A-story waits patiently (no time pressure, no failure state)
- Journey IS the point (no destination anxiety)
- Never "finished" (always more to discover)

**Development Benefits:**
- Live content addition natural (new archetypes/entities slot in)
- No ending validation complexity
- Procedural system enables infinite variation
- AI generation scales content

### Consequences

**Positive:**
- **No Arbitrary Ending**: Eliminates hardest narrative design problem
- **Infinite Replayability**: Never same twice, always new content
- **Player Agency**: Player chooses WHEN to pursue A-story, not IF
- **Live Evolution**: Perfect for ongoing content additions
- **Narrative Coherence**: Travel as eternal state matches wanderer fantasy
- **No Post-Ending Awkwardness**: Game doesn't outlive its story

**Negative:**
- **No Closure**: Some players want definitive endings (trade-off accepted)
- **Generation Quality**: Procedural content must maintain standards (requires validation)
- **Validation Complexity**: Must guarantee forward progress infinitely (architectural requirement)
- **Balancing**: Must scale difficulty/rewards without ceiling (catalogues handle this)

**Trade-Offs:**
- Sacrifices narrative closure for infinite content (design choice favoring long-term engagement)
- Requires robust procedural generation system (architectural investment required)
- Demands strict structural guarantees (no soft-locks ever - TIER 1 principle)

### Related Decisions
- DDR-007: Four-Choice Pattern (guarantees forward progress)
- DDR-006: Categorical Property Scaling (enables balanced procedural content)
- DDR-002: Tag-Based Scene Dependencies (**SUPERSEDED** - replaced by resource-based SpawnConditions)

---

## DDR-002: Tag-Based Scene Dependencies vs Hardcoded Scene Chains

### Status
**SUPERSEDED** - Replaced by resource-based SpawnConditions system
**Superseded Date:** 2025-11 (Commit 3e3b239)
**Superseding Rationale:** Tag system redundant with existing spawn condition capabilities (MinStats, NPCBond, RequiredItems). All documented progression patterns achievable through resource thresholds and reward-driven spawning without abstract tag layer. Removal aligns with Requirement Inversion Principle (resource arithmetic over boolean gates).

**Current Implementation:** See actual progression architecture in code:
- A-Story: Reward-driven sequential spawning (ScenesToSpawn) with AlwaysEligible conditions
- B/C Stories: Resource threshold gating (SpawnConditions with MinStats/NPCBond/RequiredItems)
- Perfect information: Numeric thresholds visible to player ("Need Morality 5, you have 3")

---

### **ORIGINAL DESIGN (NOT IMPLEMENTED - Historical Context Below)**

### Context

Need progression structure supporting both authored tutorial and procedural content. Traditional approaches use hardcoded chains (A1 → A2 → A3) which work for fixed content but break for procedural generation. Player actions should unlock new content naturally based on accumulated capabilities and knowledge, not arbitrary flag checks.

### Decision

**ORIGINAL PROPOSAL (SUPERSEDED):** Scenes spawn based on player state tags (RequiresTags/GrantsTags system) instead of hardcoded predecessor/successor relationships.

**Tag System Architecture:**
- Each Scene defines RequiresTags (what player needs to spawn this scene)
- Each Scene defines GrantsTags (what player gains upon completion)
- Scene spawning evaluates RequiresTags against player's accumulated tags
- No hardcoded scene chains - flexible graph structure

**Example Flow:**
```
A1 completes → Grants ["tutorial_complete", "knows_innkeeper"]
A2 requires ["tutorial_complete"]
A3 requires ["tutorial_complete", "met_merchant"]
B-Story-1 requires ["knows_innkeeper", "has_5_coins"]
```

Player can pursue A2, A3, or B-Story-1 in any order once requirements met.

### Alternatives Considered

**Option A: Tag-Based Dependencies (Chosen)**
- **Pros:**
  - Flexible branching (multiple paths available)
  - Supports player agency (choose priority)
  - Procedural-friendly (AI can generate scenes with categorical tags)
  - Enables cross-storyline dependencies
  - No rigid railroad
- **Cons:**
  - More complex validation (must prevent soft-locks via tag analysis)
  - Requires careful tag design
  - Can create "tag soup" if poorly managed
- **Why Chosen:** Enables branching, supports procedural content, maintains player agency

**Option B: Hardcoded Scene Chains (Rejected)**
- **Pros:**
  - Simple to validate (linear chain)
  - Easy to author (just sequence)
  - Clear progression path
- **Cons:**
  - Railroad (one path only)
  - No player agency
  - Procedural content can't insert naturally
  - Inflexible (changes require surgery)
- **Why Rejected:** Too rigid, kills player agency, incompatible with procedural generation

**Option C: Boolean Flag Dependencies (Rejected)**
- **Pros:**
  - Flexible (many flags possible)
  - Simple to check
- **Cons:**
  - Destroys strategic depth (flag checks = boolean gates)
  - No resource competition
  - Violates Requirement Inversion Principle
  - Creates Cookie Clicker pattern
- **Why Rejected:** Violates core design principle (resource arithmetic over boolean gates)

**Option D: Level Gating (Player Level Required) (Rejected)**
- **Pros:**
  - Simple to balance
  - Clear progression curve
- **Cons:**
  - Not verisimilitude (why does innkeeper care about player level?)
  - Arbitrary gates
  - Doesn't reflect narrative logic
- **Why Rejected:** Breaks fiction, arbitrary gating

### Rationale

**Design Principle Alignment:**
- Supports player agency through branching (TIER 2: Core Experience)
- Enables resource-based progression (tags as accumulated knowledge/capabilities)
- Maintains verisimilitude (gates based on narrative logic, not arbitrary level)

**Enables Procedural Content:**
- AI can generate scenes with categorical tag requirements ("requires_met_merchant" not hardcoded ID)
- New scenes slot into existing tag graph naturally
- Cross-references emergent rather than manually authored

**Supports Multiple Storylines:**
- A-story, B-stories, C-stories all use same tag system
- Dependencies across storylines possible (B-story unlocks A-story path)
- No artificial separation

### Consequences

**Positive:**
- **Flexible Branching**: Multiple valid progression paths
- **Player Agency**: Choose which scenes to pursue when requirements met
- **Procedural Integration**: AI-generated scenes use same dependency system
- **Cross-Storyline Synergy**: Knowledge from one story enables options in another
- **No Railroad**: Player controls pacing and priority

**Negative:**
- **Complex Validation**: Must analyze tag graph to prevent soft-locks
- **Tag Design Required**: Poor tag choices create confusion
- **Debugging Harder**: Non-linear progression harder to trace
- **Can Create Orphans**: Scenes with impossible requirements if tags poorly designed

**Trade-Offs:**
- Sacrifices linear simplicity for branching flexibility
- Requires validation tooling (tag graph analyzer)
- More complex content authoring (must think about tags)

### Related Decisions
- DDR-001: Infinite A-Story (procedural content needs flexible dependencies)
- DDR-007: Four-Choice Pattern (always includes zero-requirement fallback preventing soft-locks)
- DDR-006: Categorical Property Scaling (tags use categorical properties)

---

## DDR-003: Unified 5-Stat System vs Separate Stats Per Challenge Type

### Status
**Active** - Core progression system

### Context

Three parallel challenge systems (Mental, Physical, Social) need stat foundation. Options range from single unified stat (no variety) to separate stat pools per challenge type (too many stats, no synergy) to unified stats manifesting differently per type.

### Decision

**Single 5-stat system (Insight/Rapport/Authority/Diplomacy/Cunning) manifesting differently per challenge type.**

**Stat Manifestations:**
- **Mental Challenges**: Insight (pattern recognition), Cunning (subtle investigation)
- **Physical Challenges**: Authority (decisive action), Diplomacy (measured technique)
- **Social Challenges**: Rapport (emotional connection), Authority (commanding conversation), Diplomacy (balanced approach)

**Card Binding:**
- Every card in every system bound to one of five stats
- Player's stat level determines card depth access
- Playing any card grants XP to its bound stat
- Single unified progression across all systems

### Alternatives Considered

**Option A: Unified 5-Stat System (Chosen)**
- **Pros:**
  - Cross-challenge progression (playing any type improves capabilities)
  - Thematic coherence (stats = character traits manifesting contextually)
  - Build variety (specialize or generalize)
  - Manageable complexity (5 stats trackable)
  - No wasted effort (every card contributes to unified progression)
- **Cons:**
  - Stats must be carefully designed for multi-purpose use
  - Less direct control per challenge type
  - Balancing requires universal formulas
- **Why Chosen:** Enables cross-challenge synergy, supports specialization, maintains manageable complexity

**Option B: Separate Stats Per Challenge Type (Rejected)**
- **Pros:**
  - Direct control per type
  - Clear specialization per system
  - Simple to balance per type
- **Cons:**
  - Too many stats (15 stats if 5 per type)
  - No cross-challenge synergy
  - Wasted effort (playing Mental doesn't help Physical)
  - Cognitive overload
- **Why Rejected:** Too complex, no synergy between systems

**Option C: Single Unified Stat (Rejected)**
- **Pros:**
  - Simplest possible
  - All progress unified
  - No stat choices
- **Cons:**
  - No build variety
  - No specialization
  - Boring (one number goes up)
  - No strategic depth
- **Why Rejected:** No interesting choices, no identity

**Option D: Two-Stat System (Physical/Mental) (Rejected)**
- **Pros:**
  - Simple
  - Some specialization
  - Easy to understand
- **Cons:**
  - Binary (two builds only)
  - Social challenges don't fit cleanly
  - Limited variety
- **Why Rejected:** Too limiting, binary choice

### Rationale

**Design Principle Alignment:**
- Supports build specialization (TIER 2: Resource Scarcity Creates Choices)
- Enables strategic depth through stat allocation
- Maintains single source of truth (one stat system, not three)

**Thematic Coherence:**
- Insight manifests as deductive reasoning (Mental), structural analysis (Physical), reading motivations (Social)
- Authority manifests as decisive conclusions (Mental), power moves (Physical), commanding conversation (Social)
- Same trait, different contexts

**Build Variety:**
- "Investigator" (Insight + Cunning): Dominates Mental, weak Social
- "Diplomat" (Rapport + Diplomacy): Dominates Social, weak Mental
- "Leader" (Authority + Diplomacy): Balanced Social, weak Insight/Cunning
- "Generalist" (balanced): Handles all moderately, dominates none

### Consequences

**Positive:**
- **Single Progression Path**: Playing any challenge type improves capabilities across all systems
- **Thematic Coherence**: Stats represent fundamental character traits manifesting differently
- **Build Variety**: Specialize (few stats) or generalize (all stats) = distinct playstyles
- **No Wasted Effort**: Every card played contributes to unified character progression
- **Manageable Complexity**: 5 stats trackable without cognitive overload

**Negative:**
- **Less Granular Control**: Can't optimize per challenge type independently
- **Universal Balance Required**: Stats must work across all three systems
- **Indirect Relationships**: Stat → card depth access less obvious than dedicated stats

**Trade-Offs:**
- Sacrifices per-type specialization for cross-type synergy
- Universal balancing more complex but enables coherent progression
- Less direct control but more meaningful stat allocation

### Related Decisions
- DDR-009: Three Challenge Types (unified stats support three systems)
- DDR-004: Tight Economic Margins (limited XP forces stat specialization)

---

## DDR-004: Tight Economic Margins vs Generous Resource Availability

### Status
**Active** - Core economic pressure

### Context

Need strategic depth and impossible choices. Resource availability determines whether player faces meaningful trade-offs. Generous economy ("can afford everything eventually") removes tension. Harsh poverty ("never enough") creates frustration. Sweet spot is tight margins where optimization matters.

### Decision

**Delivery earnings barely cover survival costs (tight margins).** Player must optimize to accumulate small profits. Every coin spent is a coin not available elsewhere.

**Economic Structure:**
- Delivery jobs earn 15-25 coins
- Survival costs (food + lodging) consume 12-18 coins per day
- Net profit: 3-7 coins per day (tight margin)
- Equipment/upgrades cost 50-200 coins (weeks of savings)
- Emergency expenses can wipe out savings

**Consequence:**
- Player cannot afford everything
- Must prioritize (upgrade OR emergency fund OR relationship investment)
- Optimization skill determines prosperity
- Strategic decisions matter

### Alternatives Considered

**Option A: Tight Margins (Chosen)**
- **Pros:**
  - Creates impossible choices
  - Rewards optimization
  - Maintains engagement
  - Every coin matters
  - Strategic depth
- **Cons:**
  - Requires careful balance tuning
  - Can feel grindy if too tight
  - Player frustration if unbalanced
- **Why Chosen:** Creates strategic depth, rewards skill, maintains tension

**Option B: Generous Economy (Rejected)**
- **Pros:**
  - Low frustration
  - Player can experiment freely
  - No resource stress
- **Cons:**
  - Removes tension
  - No trade-offs (buy everything)
  - Strategic depth collapses
  - Becomes Cookie Clicker (just accumulate)
- **Why Rejected:** Destroys strategic depth, no interesting choices

**Option C: Resource Abundance (Rejected)**
- **Pros:**
  - Very player-friendly
  - No stress
  - Exploration encouraged
- **Cons:**
  - No resource scarcity
  - No opportunity cost
  - Choices meaningless (always say yes)
  - Violates core design principle
- **Why Rejected:** Violates Requirement Inversion Principle (resource scarcity required)

**Option D: Harsh Poverty (Rejected)**
- **Pros:**
  - High tension
  - Survival challenge
  - Strong atmosphere
- **Cons:**
  - Frustrating (never enough)
  - Blocks content (can't afford to try things)
  - Failure spiral (one mistake = bankruptcy)
  - Not fun
- **Why Rejected:** Too frustrating, blocks engagement with content

### Rationale

**Design Principle Alignment:**
- Resource scarcity creates impossible choices (TIER 2: Core principle)
- Strategic decisions matter (optimization rewarded)
- No soft-locks (tight but not impossible)

**Psychological Effect:**
- "Can I afford this?" = engagement
- Saving for upgrade = accomplishment
- Choosing priorities = identity
- Optimizing routes = skill expression

**Balancing Act:**
- Tight enough to matter
- Loose enough to allow experimentation
- Forgiving enough to recover from mistakes
- Rewarding enough to feel progression

### Consequences

**Positive:**
- **Impossible Choices**: Cannot afford everything, must prioritize
- **Optimization Rewarded**: Skill determines prosperity
- **Engagement Maintained**: Every decision matters
- **Strategic Depth**: Resource competition creates depth
- **Accomplishment**: Savings feel earned

**Negative:**
- **Balance Required**: Too tight = frustration, too loose = meaningless
- **Tuning Complexity**: Must calibrate earnings vs costs carefully
- **Potential Grind**: If imbalanced, can feel grindy
- **Learning Curve**: New players may struggle initially

**Trade-Offs:**
- Sacrifices player comfort for strategic depth
- Requires careful balancing but creates engagement
- Tension over relaxation

### Related Decisions
- DDR-008: Delivery Courier Core Loop (tight margins drive loop engagement)
- DDR-003: Unified Stat System (limited XP = another scarce resource)

---

## DDR-005: Strategic-Tactical Layer Separation vs Unified Gameplay Layer

### Status
**Active** - Core architectural pattern

### Context

Need informed decisions (WHAT to attempt) AND tactical depth (HOW to execute). If strategic and tactical concerns mix, player can't calculate risk before committing. Perfect information impossible if tactical complexity bleeds into strategic layer. Must separate or unify.

### Decision

**Strict separation into two distinct layers: Strategic (perfect information) and Tactical (hidden complexity).**

**STRATEGIC LAYER (Scene → Situation → Choice):**
- Perfect information (all costs/rewards/requirements visible)
- State machine progression (no victory thresholds)
- Persistent entities (scenes exist until completed/expired)
- WHAT decisions (which challenge to attempt, when)

**TACTICAL LAYER (Mental/Physical/Social Challenges):**
- Hidden complexity (card draw order unknown)
- Victory thresholds (resource accumulation required)
- Temporary sessions (created and destroyed per engagement)
- HOW execution (card play, resource management)

**THE BRIDGE (ChoiceTemplate.ActionType):**
- **Instant**: Stay strategic (apply rewards immediately)
- **Navigate**: Stay strategic (move to new context)
- **StartChallenge**: Cross to tactical (spawn challenge session)

### Alternatives Considered

**Option A: Strict Separation (Chosen)**
- **Pros:**
  - Perfect information at strategic layer
  - Tactical surprise preserved
  - Clear player expectations
  - Explicit bridge mechanism
  - One-way flow (strategic spawns tactical, returns outcome)
- **Cons:**
  - Cannot mix layers (single moment can't be both)
  - Two entity models to maintain
  - Explicit bridge design required
- **Why Chosen:** Enables informed risk-taking, preserves tactical surprise, clear boundaries

**Option B: Unified Layer (No Separation) (Rejected)**
- **Pros:**
  - Simpler architecture
  - One entity model
  - No bridge needed
- **Cons:**
  - Can't provide perfect information with card complexity
  - Strategic calculations impossible with hidden tactical variables
  - Player can't assess risk before commitment
- **Why Rejected:** Violates perfect information principle, can't calculate risk

**Option C: Three Layers (Strategic, Tactical, Operational) (Rejected)**
- **Pros:**
  - Very granular separation
  - Maximum clarity
- **Cons:**
  - Over-engineering
  - Two layers sufficient
  - Additional complexity with no benefit
- **Why Rejected:** Unnecessary complexity, two layers handle requirements

**Option D: Soft Boundary (Guidelines Not Enforcement) (Rejected)**
- **Pros:**
  - Flexibility
  - Can bend rules when needed
- **Cons:**
  - Slippery slope to layer violations
  - Unclear when to cross
  - No enforcement mechanism
- **Why Rejected:** Guidelines without enforcement lead to violations, need hard boundaries

### Rationale

**Design Principle Alignment:**
- Perfect Information (TIER 2): Strategic layer shows all costs/rewards before commitment
- Single Responsibility: Each layer has one purpose
- Elegance (TIER 3): Clear separation reduces coupling

**Player Experience:**
- Strategic: "Should I attempt this challenge?" (informed decision)
- Tactical: "How do I win this challenge?" (execution skill)
- Bridge: Clear transition, no confusion

**Architectural Clarity:**
- Situations are strategic entities (persistent, state machine)
- Challenges are tactical sessions (temporary, threshold-based)
- Never conflate

### Consequences

**Positive:**
- **Clear Separation**: Perfect information at strategic layer, surprise at tactical layer
- **Bridge Pattern**: Explicit routing via ActionType property
- **One-Way Flow**: Strategic spawns tactical, tactical returns outcome
- **Three Parallel Systems**: Social/Mental/Physical all follow same pattern
- **Layer Purity**: Situations strategic, challenges tactical (never conflate)

**Negative:**
- **Cannot Mix**: Single moment can't be both strategic and tactical simultaneously
- **Clear Entry/Exit**: Must design explicit bridge points for every challenge
- **Two Entity Models**: Distinct models to maintain (strategic vs tactical)

**Trade-Offs:**
- Sacrifices architectural simplicity for layer clarity
- Two models but clear boundaries
- Explicit bridges but no confusion

### Related Decisions
- DDR-009: Three Challenge Types (tactical layer systems)
- DDR-001: Infinite A-Story (strategic layer structure)

---

## DDR-006: Categorical Property Scaling vs Hand-Tuned Content Instances

### Status
**Active** - Content generation architecture

### Context

Need infinite procedural content with consistent balance. Hand-tuning every instance doesn't scale (100 NPCs × 10 services = 1000 manual entries). AI-generated absolute numbers break balance (AI doesn't know player progression). Need approach enabling AI generation without breaking balance.

### Decision

**Categorical properties (Friendly/Premium/Hostile) translated to balanced numbers via universal formulas.**

**Three-Phase Pipeline:**
1. **JSON Authoring**: Categorical properties ("Friendly", "Standard", "Premium")
2. **Parse-Time Translation**: Catalogues convert categorical → concrete (StatThreshold: 5 × 0.6 = 3)
3. **Runtime**: Use concrete properties (no catalogue calls, no string matching)

**Scaling Formulas:**
- NPCDemeanor: Friendly (0.6×), Neutral (1.0×), Hostile (1.4×) → scales stat thresholds
- Quality: Basic (0.6×), Standard (1.0×), Premium (1.6×), Luxury (2.4×) → scales coin costs
- PowerDynamic: Dominant (0.6×), Equal (1.0×), Submissive (1.4×) → scales authority checks

**Example:**
```
Base: StatThreshold 5, CoinCost 8
Context: Friendly NPC, Premium Quality, Equal Power
Scaled: StatThreshold 3 (5 × 0.6), CoinCost 13 (8 × 1.6)
```

### Alternatives Considered

**Option A: Categorical Scaling (Chosen)**
- **Pros:**
  - Enables AI content generation
  - Maintains relative balance
  - Scales infinitely
  - One formula affects all instances
  - Dynamic difficulty adjustment
- **Cons:**
  - Less granular control per instance
  - Universal formulas must work everywhere
  - Cannot hand-tune specific instances
- **Why Chosen:** Enables infinite balanced content, scales perfectly, AI-friendly

**Option B: Hand-Tuned Absolute Values (Rejected)**
- **Pros:**
  - Precise control per instance
  - Can optimize each encounter
  - No formula complexity
- **Cons:**
  - Doesn't scale (1000+ manual entries)
  - Breaks AI generation (AI doesn't know balance)
  - Maintenance nightmare (change formula = fix 1000 files)
  - Not infinite
- **Why Rejected:** Doesn't scale, breaks AI generation, unmaintainable

**Option C: AI-Generated Absolute Numbers (Rejected)**
- **Pros:**
  - AI can generate directly
  - No catalogue needed
  - Simple pipeline
- **Cons:**
  - AI doesn't know game balance
  - AI doesn't know player progression
  - Will create imbalanced content
  - No consistency guarantee
- **Why Rejected:** Breaks balance, AI lacks context

**Option D: Random Values Within Ranges (Rejected)**
- **Pros:**
  - Variation
  - No authoring required
  - Simple generation
- **Cons:**
  - No consistent difficulty
  - No narrative coherence (friendly NPC as hard as hostile)
  - Breaks verisimilitude
- **Why Rejected:** Inconsistent, breaks fiction

### Rationale

**Design Principle Alignment:**
- Single Source of Truth (TIER 1): Catalogues are canonical translation
- Elegance (TIER 3): One formula affects all instances uniformly
- Type Safety: Enums over strings, properties over dictionaries

**AI Generation Enablement:**
- AI describes categorically ("friendly", "premium")
- System translates to balanced numbers
- AI doesn't need global game state knowledge
- Infinite content without breaking balance

**Mathematical Variety:**
- 21 archetypes × 3 NPCDemeanor × 4 Quality × 3 PowerDynamic × 3 EnvironmentQuality
- = 2,268 mechanical variations from property combinations
- Add narrative variety = effectively infinite content

### Consequences

**Positive:**
- **AI Generation**: AI generates balanced content without global state knowledge
- **Minimal Authoring**: Specify entity type, not 50 numeric values
- **Universal Formulas**: One negotiation archetype scales to all contexts
- **Dynamic Scaling**: Change multiplier, all scenes rebalance automatically
- **Zero Runtime Overhead**: Translation happens once at parse-time

**Negative:**
- **No Hand-Tuning**: Cannot adjust specific instances (all scaling formulaic)
- **Universal Scaling**: Must design formulas that work across ALL contexts
- **Parse-Time Cost**: One-time cost during load screen (acceptable)

**Trade-Offs:**
- Sacrifices granular control for infinite scaling
- Universal formulas more complex but enable infinite content
- Parse-time cost but zero runtime cost

### Related Decisions
- DDR-001: Infinite A-Story (procedural content needs scaling)
- DDR-002: Tag-Based Dependencies (**SUPERSEDED** - resource thresholds use categorical properties)

---

## DDR-007: Four-Choice Pattern vs Variable Choice Count

### Status
**Active** - Guaranteed progression pattern

### Context

Need guaranteed forward progress to prevent soft-locks in infinite game. Player can't "restart" 50 hours deep. Every A-story situation must have path forward. Options range from variable choice count (inconsistent) to fixed pattern (predictable).

### Decision

**Four-choice archetype pattern (stat/money/challenge/fallback) for A-story situations.**

**Four Choices:**
1. **Stat-Gated Path** (PathType.InstantSuccess)
   - Requires stat threshold (Rapport ≥ 3, Authority ≥ 4)
   - Free if qualified
   - Best rewards
   - Rewards character building

2. **Money-Gated Path** (PathType.InstantSuccess)
   - Requires coins (scaled by Quality)
   - Guaranteed success
   - Good rewards
   - Always available if player has coins

3. **Challenge Path** (PathType.StartChallenge)
   - Costs Resolve/Stamina/Focus
   - Routes to tactical challenge
   - Variable rewards (success/failure)
   - Demonstrates tactical skill

4. **Fallback Path** (PathType.Fallback)
   - Always available (no requirements)
   - Minimal rewards
   - Poor outcome
   - Guarantees forward progress

**All four spawn next A-scene** (different entry states, same progression).

### Alternatives Considered

**Option A: Four-Choice Pattern (Chosen)**
- **Pros:**
  - Multiple approaches (stat OR money OR skill OR patience)
  - Guarantees forward progress (fallback always available)
  - Orthogonal costs (different resource types)
  - Consistent structure (player learns pattern)
- **Cons:**
  - Content authoring more structured
  - Less flexibility per situation
  - Four always feels formulaic
- **Why Chosen:** Provides multiple approaches, guarantees no soft-locks, orthogonal costs

**Option B: Variable Choice Count (Rejected)**
- **Pros:**
  - Flexibility per situation
  - Natural variation
  - Less predictable
- **Cons:**
  - Inconsistent experience
  - Hard to guarantee no soft-locks (what if all 2 choices require resources player lacks?)
  - Balance harder (2-choice vs 5-choice situations)
- **Why Rejected:** Cannot guarantee forward progress, inconsistent

**Option C: Three-Choice Pattern (Rejected)**
- **Pros:**
  - Simpler than four
  - Still multiple approaches
- **Cons:**
  - No challenge path (removes tactical integration)
  - OR no fallback (soft-lock risk)
  - Less variety than four
- **Why Rejected:** Missing either challenge path or fallback (both needed)

**Option D: Two-Choice Binary (Rejected)**
- **Pros:**
  - Simple
  - Clear decision
- **Cons:**
  - Binary (no variety)
  - Hard to prevent soft-locks (both might require resources)
  - No nuance
- **Why Rejected:** Too limiting, soft-lock risk

### Rationale

**Design Principle Alignment:**
- No Soft-Locks (TIER 1): Fallback always available
- Resource Scarcity (TIER 2): Stat/money/challenge/fallback use different resources
- Balanced Choice Design (Principle 11): Orthogonal resource costs

**Guaranteed Progression:**
- Player ALWAYS has at least one choice available (fallback)
- Fallback costs nothing (no requirements)
- Fallback advances progression (spawns next scene)
- Might be slow/poor outcome but NEVER stuck

**Resource Orthogonality:**
- Stat: Permanent build (invested XP)
- Money: Consumable currency (depleted)
- Challenge: Time + Resolve/Stamina/Focus (opportunity cost)
- Fallback: Patience (poor outcome but moves forward)

Each costs DIFFERENT resource type = no dominance.

### Consequences

**Positive:**
- **Multiple Approaches**: Stat OR money OR skill OR patience
- **Guaranteed Forward Progress**: Fallback prevents soft-locks
- **Resource Orthogonality**: Different choices cost different resources
- **Consistent Structure**: Player learns pattern, applies everywhere
- **Build Flexibility**: All builds can progress (some easier, some harder)

**Negative:**
- **Formulaic**: Four-choice pattern becomes predictable
- **Less Flexibility**: Content authors have less freedom
- **Structural Constraint**: Must fit situations into pattern

**Trade-Offs:**
- Sacrifices flexibility for consistency
- Predictability accepted for guarantee of no soft-locks
- Structure over freedom

### Related Decisions
- DDR-001: Infinite A-Story (no soft-locks critical for infinite game)
- DDR-004: Tight Economic Margins (money path consumes scarce resource)

---

## DDR-008: Delivery Courier Core Loop vs Alternative Professions

### Status
**Active** - Core gameplay loop

### Context

Need core gameplay loop supporting all systems (travel, resource management, challenge types, NPC relationships). Options include merchant (trade-focused), adventurer (combat-focused), scholar (investigation-focused), or courier (travel-focused).

### Decision

**Delivery courier (package delivery between locations) as core loop.**

**Loop Structure:**
1. Wake at location
2. Accept delivery job (view rewards, distance, destination)
3. Travel route segments (encounter-choice cycles)
4. Reach destination, earn coins
5. Spend on survival (food, lodging)
6. Sleep advances day

**Why Courier:**
- Supports travel (routes core mechanic)
- Integrates resource management (tight margins)
- All challenge types fit (obstacles on route, NPCs at destinations, investigations at locations)
- Neutral flavor (not combat-focused, not trade-focused)
- Scalable (new routes = new content)

### Alternatives Considered

**Option A: Delivery Courier (Chosen)**
- **Pros:**
  - Supports travel naturally
  - Tight economic pressure (margins)
  - All challenge types fit
  - Neutral flavor
  - Route learning mechanic
  - Scalable content (new routes)
- **Cons:**
  - Less glamorous than adventurer
  - Might feel repetitive
  - Limited narrative drama
- **Why Chosen:** Supports all systems, natural travel integration, tight economic pressure

**Option B: Merchant (Trade-Focused) (Rejected)**
- **Pros:**
  - Economic loop natural
  - Buy low, sell high
  - Clear profit motive
- **Cons:**
  - Too economic-focused (other systems feel tacked on)
  - Combat challenges don't fit
  - Investigations tangential
  - Margins might be too loose (trade profits)
- **Why Rejected:** Doesn't support all challenge types equally

**Option C: Adventurer (Combat-Focused) (Rejected)**
- **Pros:**
  - Exciting narrative
  - Combat natural
  - Hero fantasy
- **Cons:**
  - Too combat-focused
  - Social challenges feel forced ("negotiate during dungeon")
  - Travel motivated by quests not natural movement
  - Economic loop unclear (loot? bounties?)
- **Why Rejected:** Doesn't support all systems equally, combat-centric

**Option D: Scholar (Investigation-Focused) (Rejected)**
- **Pros:**
  - Investigation natural
  - Research travel makes sense
  - Intellectual theme
- **Cons:**
  - Physical challenges don't fit (why is scholar climbing?)
  - Economic loop unclear (research grants?)
  - Travel less natural (visiting archives vs open world)
- **Why Rejected:** Doesn't support physical challenges naturally

### Rationale

**Design Principle Alignment:**
- Supports all three challenge types equally
- Economic pressure (tight margins)
- Travel as core mechanic
- Verisimilitude (couriers travel, deliver, encounter problems)

**System Integration:**
- Routes: Natural (delivery requires travel)
- Resource Management: Natural (delivery earnings barely cover costs)
- Mental Challenges: Natural (route investigations, locations to explore)
- Physical Challenges: Natural (obstacles on routes)
- Social Challenges: Natural (NPCs at destinations, relationships matter)

**Scalability:**
- New routes = new content
- Route mastery (learning segments)
- Repeat customers (NPC relationships)
- Geographic expansion natural

### Consequences

**Positive:**
- **Travel Natural**: Delivery requires moving between locations
- **Economic Pressure**: Tight margins create strategic decisions
- **All Challenge Types**: Mental/Physical/Social all fit naturally
- **Scalable**: New routes easy to add
- **Route Mastery**: Learning fixed segments rewarding

**Negative:**
- **Less Glamorous**: Courier less exciting than adventurer
- **Repetitive Risk**: Delivery loop might feel same-y
- **Narrative Limits**: Harder to create epic narrative (you're just courier)

**Trade-Offs:**
- Sacrifices narrative glamour for mechanical support
- Courier over adventurer to support all systems equally
- Scalability over drama

### Related Decisions
- DDR-004: Tight Economic Margins (delivery earnings barely cover costs)
- DDR-009: Three Challenge Types (courier encounters all three naturally)

---

## DDR-009: Three Challenge Types (Mental/Physical/Social) vs More/Fewer

### Status
**Active** - Tactical layer systems

### Context

Need tactical variety without overwhelming players. Options range from one type (monotonous), two types (binary), three types (balanced), to four+ types (overwhelming).

### Decision

**Three parallel tactical systems with equivalent depth: Mental, Physical, Social.**

**Three Systems:**
- **Mental**: Investigation challenges (location-based, pauseable, Progress/Attention/Exposure)
- **Physical**: Obstacle challenges (location-based, one-shot, Breakthrough/Exertion/Danger)
- **Social**: Conversation challenges (NPC-based, session-bounded, Momentum/Initiative/Doubt)

**Equivalent Depth:**
- Each has Builder/Threshold/Session resources
- Each has unique flow mechanic
- Each has action pair
- Each binds to unified stat system

### Alternatives Considered

**Option A: Three Challenge Types (Chosen)**
- **Pros:**
  - Covers major interaction types
  - Manageable complexity
  - Supports specialization
  - Variety without overload
- **Cons:**
  - Must maintain equivalent depth across three systems
  - Balancing more complex than one/two
  - Three parallel systems to develop
- **Why Chosen:** Balanced variety, covers major interactions, manageable

**Option B: One Challenge Type (Rejected)**
- **Pros:**
  - Simple
  - Deep (all effort into one system)
  - Easy to balance
- **Cons:**
  - Monotonous
  - No build variety
  - All problems solved same way
  - Boring long-term
- **Why Rejected:** No variety, monotonous

**Option C: Two Challenge Types (Rejected)**
- **Pros:**
  - Simple
  - Some variety
  - Easy to balance
- **Cons:**
  - Binary (Physical/Mental or Physical/Social)
  - Social OR Mental suffers (can't have both)
  - Limited build variety (2 archetypes only)
- **Why Rejected:** Too binary, misses key interaction type

**Option D: Four+ Challenge Types (Rejected)**
- **Pros:**
  - Maximum variety
  - Very specialized builds
  - Covers niche interactions
- **Cons:**
  - Overwhelming (too many systems)
  - Hard to maintain equivalent depth
  - Cognitive overload
  - Development burden (4+ parallel systems)
- **Why Rejected:** Too complex, overwhelming players

### Rationale

**Design Principle Alignment:**
- One Purpose Per Entity (Principle 7): Each challenge type one purpose
- Unified Stat System (DDR-003): Three types supported by 5 stats
- Specialization Creates Identity (Principle): Three types enable 3+ builds

**Interaction Coverage:**
- Mental: Investigations, puzzles, deduction (Insight/Cunning)
- Physical: Obstacles, athletic challenges, confrontations (Authority/Diplomacy)
- Social: Conversations, negotiations, relationships (Rapport/Authority/Diplomacy)

**Build Variety:**
- "Investigator": High Insight/Cunning → dominates Mental
- "Diplomat": High Rapport/Diplomacy → dominates Social
- "Leader": High Authority/Diplomacy → balanced Social/Physical
- "Generalist": Balanced stats → handles all moderately

### Consequences

**Positive:**
- **Variety**: Three distinct tactical experiences
- **Build Specialization**: Different builds excel at different types
- **Manageable**: Three systems trackable without cognitive overload
- **Coverage**: Major interaction types represented
- **Depth**: Can maintain equivalent depth across three

**Negative:**
- **Balancing Complex**: Must maintain equivalent depth across three
- **Development Burden**: Three parallel systems to develop/maintain
- **Learning Curve**: Players must learn three systems

**Trade-Offs:**
- Sacrifices simplicity for variety
- Three systems but manageable
- Development burden but worthwhile variety

### Related Decisions
- DDR-003: Unified Stat System (5 stats support 3 challenge types)
- DDR-008: Delivery Courier (courier encounters all 3 naturally)
- DDR-010: Pauseable Mental Challenges (differentiates Mental from Physical)

---

## DDR-010: Pauseable Mental Challenges vs One-Shot Sessions

### Status
**Active** - Session model differentiation

### Context

Investigation challenges feel different from obstacles in fiction. Real investigations take time across multiple visits. Physical obstacles are immediate tests. Need session model matching fiction (verisimilitude).

### Decision

**Mental challenges pauseable (progress persists), Physical challenges one-shot (complete or fail in single attempt).**

**Mental Session Model (Pauseable):**
- Can pause anytime (leave location, state persists)
- Progress accumulates across visits
- Exposure persists (investigative footprint)
- Attention resets per visit
- No forced ending
- Incremental victory

**Physical Session Model (One-Shot):**
- Single attempt only
- Must complete or fail in one session
- Personal state carries (Health/Stamina persist)
- Danger threshold = immediate failure
- Cannot pause and resume

**Social Session Model (Session-Bounded):**
- Must complete in single conversation
- MaxDoubt ends conversation (NPC frustration)
- Can leave early voluntarily
- Relationship persists between conversations

### Alternatives Considered

**Option A: Differentiated Models (Chosen)**
- **Pros:**
  - Verisimilitude (investigations take time, obstacles immediate)
  - Different pacing per type
  - Fiction supports mechanics
  - Strategic variety
- **Cons:**
  - More complex session management
  - Three models to maintain
  - Player must learn different patterns
- **Why Chosen:** Verisimilitude, fiction fit, strategic variety

**Option B: All Pauseable (Rejected)**
- **Pros:**
  - Simple (one model)
  - Player-friendly (always can pause)
  - Less pressure
- **Cons:**
  - Physical challenges don't fit fiction (pause mid-climb?)
  - No urgency for Physical
  - Social challenges break (pause mid-conversation?)
- **Why Rejected:** Breaks verisimilitude for Physical/Social

**Option C: All One-Shot (Rejected)**
- **Pros:**
  - Simple (one model)
  - High pressure
  - Clear stakes
- **Cons:**
  - Investigations don't fit fiction (must solve crime in one visit?)
  - No incremental progress
  - Too harsh for Mental
- **Why Rejected:** Investigations take time in fiction, one-shot breaks verisimilitude

**Option D: All Session-Bounded (Rejected)**
- **Pros:**
  - Middle ground
  - Can leave voluntarily
  - Some flexibility
- **Cons:**
  - Still doesn't fit Mental (investigations need multiple visits)
  - Physical forced ending arbitrary
  - Not optimal for any type
- **Why Rejected:** Compromises all types, fits none perfectly

### Rationale

**Design Principle Alignment:**
- Verisimilitude (TIER 3): Mechanics match fiction
- One Purpose Per Entity (Principle 7): Each challenge type serves distinct purpose
- Different systems for different contexts

**Fiction Support:**
- Mental: "I'll investigate this location over several visits" (pauseable)
- Physical: "I attempt to climb this cliff now" (one-shot)
- Social: "I have this conversation with this person now" (session-bounded)

**Strategic Implications:**
- Mental: Can leave and return (safer, incremental)
- Physical: All-or-nothing commitment (riskier, immediate)
- Social: Must finish or lose opportunity (conversation ends)

### Consequences

**Positive:**
- **Verisimilitude**: Session models match fiction
- **Pacing Variety**: Different pressures per type
- **Fiction Support**: Mechanics express narrative reality
- **Strategic Depth**: Different risk profiles

**Negative:**
- **Complex Session Management**: Three models to implement/maintain
- **Learning Curve**: Players must understand three patterns
- **Consistency Trade-off**: Not uniform across types

**Trade-Offs:**
- Sacrifices uniformity for verisimilitude
- Three models but fiction fit
- Complexity but strategic variety

### Related Decisions
- DDR-009: Three Challenge Types (different types benefit from different models)
- DDR-005: Strategic-Tactical Separation (all three are tactical layer)

---

## 11.2 Summary

These ten DDRs represent the foundational game design decisions shaping Wayfarer's player experience:

1. **Infinite A-Story**: Never-ending journey without resolution (eliminates ending pressure)
2. **Tag-Based Dependencies**: Flexible progression graph vs rigid chains (supports branching)
3. **Unified 5-Stat System**: Cross-challenge progression (single advancement path)
4. **Tight Economic Margins**: Delivery earnings barely cover costs (creates pressure)
5. **Strategic-Tactical Separation**: Perfect information vs hidden complexity (informed decisions)
6. **Categorical Scaling**: Relative properties enable infinite content (AI generation)
7. **Four-Choice Pattern**: Guaranteed forward progress (no soft-locks)
8. **Delivery Courier Loop**: Travel-based profession (supports all systems)
9. **Three Challenge Types**: Mental/Physical/Social (variety without overload)
10. **Pauseable Mental Challenges**: Session models match fiction (verisimilitude)

**Common Themes:**
- Player agency through informed decisions (perfect information at strategic layer)
- Resource scarcity creating impossible choices (tight margins, limited stats)
- No soft-locks ever (TIER 1 non-negotiable, fallback paths guaranteed)
- Verisimilitude through fiction-matching mechanics (session models, categorical properties)
- Infinite scalable content (procedural generation, categorical scaling)

**Trade-Off Patterns:**
- Sacrifices narrative closure for infinite content (DDR-001)
- Sacrifices simplicity for resource transparency (DDR-002 **SUPERSEDED** - resource thresholds over tags)
- Sacrifices player comfort for strategic depth (DDR-004)
- Sacrifices uniformity for verisimilitude (DDR-010)

---

## Related Documentation

**Game Design Documentation:**
- **01_game_vision.md** - Vision and goals driving these decisions
- **02_core_principles.md** - Principles applied in decisions
- **12_design_glossary.md** - Canonical term definitions

**Technical Documentation (arc42):**
- **09_architecture_decisions.md** - Technical ADRs implementing these design decisions
- **01_introduction_and_goals.md** - Quality goals validating decisions
- **10_quality_requirements.md** - Quality scenarios testing decisions
