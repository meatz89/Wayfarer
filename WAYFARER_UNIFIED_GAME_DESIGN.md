# Wayfarer: Unified Game Design Document

**Last Updated:** 2025-11  
**Purpose:** Comprehensive game design reference consolidating vision, systems, mechanics, patterns, and philosophy

---

## 1. CORE VISION & PHILOSOPHY

### 1.1 Experience Statement

**Wayfarer creates strategic depth through impossible choices, not mechanical complexity.**

Every decision forces the player to choose between multiple suboptimal paths, revealing character through constraint. The game is not about winning - it's about deciding what you're willing to sacrifice to achieve what matters most.

### 1.2 Player Fantasy

You are a low-fantasy traveler managing scarce resources across an unforgiving landscape. You accept delivery jobs to earn coins, navigate dangerous routes where every encounter demands difficult choices, and return barely able to afford food and shelter. The core feeling: **"I can afford A OR B, but not both. Both are valid. Which cost will I accept?"**

### 1.3 The Frieren Principle: Infinite Journey

**The game never ends.** The main storyline (A-story) is infinite procedurally-generated narrative providing structure and progression without resolution. You travel, arrive places, meet people. Each place leads to another. The journey itself IS the point, not reaching anywhere specific.

**Why Infinite:**
- Eliminates ending pressure and post-game awkwardness
- Player chooses WHEN to pursue A-story, not IF
- Perfect for live evolution and infinite replayability
- Matches "eternal traveler" fantasy coherently

### 1.4 Design Principle Hierarchy

**TIER 1: Non-Negotiable (Never Compromise)**
- No soft-locks ever (always forward progress)
- Single source of truth (one owner per entity)
- Playability over compilation (broken links worse than crashes)
- Perfect information at strategic layer (all costs visible)

**TIER 2: Core Experience (Compromise Only with Justification)**
- Resource scarcity creates choices
- Impossibility through shared resource competition
- Specialization creates identity
- Perfect information enables calculation

**TIER 3: Architectural Quality (Prefer but Negotiable)**
- Elegance through minimal interconnection
- Verisimilitude in entity relationships
- One purpose per entity

### 1.5 What Wayfarer Is Not

- **Not completionist-friendly** - Cannot do everything, scarcity forces choices
- **Not power fantasy** - Mastery through optimization, not domination
- **Not boolean-gated** - Content spawns through rewards, not unlock flags
- **Not mechanically complex** - Depth from resource competition, not rule count

---

## 2. CORE SYSTEMS ARCHITECTURE

### 2.1 Three-Tier Loop Structure

**SHORT LOOP (10-30 seconds): Individual Choices**
- Atomic decision units (choose path, execute action)
- Perfect information displayed (costs, rewards, requirements)
- Four-choice pattern (stat/money/challenge/fallback)
- Guaranteed forward progress architecturally

**MEDIUM LOOP (5-15 minutes): Delivery Cycles**
- Accept job → Navigate route → Earn coins → Spend on survival → Repeat
- Tight economic margins (earnings barely cover costs)
- Route travel with encounter management
- Economic pressure creates impossible choices

**LONG LOOP (Hours): Progression & Exploration**
- A-story progression (infinite procedural spine)
- B/C-story engagement (side content for resources)
- Stat advancement through specialization
- NPC relationship building
- Geographic exploration and region unlocking

### 2.2 Two-Layer Architecture

**STRATEGIC LAYER (Perfect Information)**
- Scene → Situation → Choice flow
- All costs, rewards, requirements visible
- Resource arithmetic over boolean gates
- Player calculates before committing

**TACTICAL LAYER (Hidden Complexity)**
- Mental/Physical/Social challenge systems
- Card-based gameplay with hidden draw order
- Builder/Session/Threshold resources
- Skill expression through efficiency

**Bridge:** StartChallenge action type transitions strategic → tactical with perfect information at entry (costs, difficulty category, success/failure rewards visible)

---

## 3. RESOURCE ECONOMY & SCARCITY

### 3.1 Resource Taxonomy

**UNIVERSAL RESOURCES (Compete Across All Systems)**
- **Time** - Time blocks per day (8-10), most constrained, cannot buy more
- **Focus** - Mental energy (0-10), consumed by Mental challenges
- **Stamina** - Physical energy (0-10), consumed by Physical challenges  
- **Health** - Life force (0-10), injury from failed challenges
- **Coins** - Universal currency, earned through deliveries
- **Resolve** - Emotional resilience (0-10), consumed by Social challenges

**TACTICAL RESOURCES (Challenge-Specific)**
- Mental: Progress/Attention/Exposure/Leads
- Physical: Breakthrough/Exertion/Danger/Aggression
- Social: Momentum/Initiative/Doubt/Cadence
- Created at session start, destroyed at session end

**PERSISTENT PROGRESSION RESOURCES**
- 5 Stats: Insight/Rapport/Authority/Diplomacy/Cunning
- Understanding - Cross-system card tier unlock currency
- NPC Bonds - Relationship levels enabling shortcuts

### 3.2 Tight Economic Margins Philosophy

Delivery earnings barely cover survival costs:
- Delivery payment: 15-25 coins
- Survival costs (food + lodging): 12-18 coins per day
- **Net profit: 3-7 coins per day**
- Equipment upgrades: 50-200 coins (weeks of savings)

This creates strategic pressure where every coin matters, optimization is rewarded, and impossible choices emerge naturally.

### 3.3 Impossible Choice Pattern

**Four-Choice Archetype (Orthogonal Resource Costs):**

1. **Stat-Gated Path** - Requires threshold (e.g., Authority ≥ 5), free if met, best outcome
2. **Money-Gated Path** - Pay coins (e.g., 15 coins), reliable, always available if affordable
3. **Challenge Path** - Time + skill + entry resources, variable outcome, both success/failure advance
4. **Fallback Path** - Zero requirements, zero immediate cost, poor outcome, guarantees forward progress

Different resource types ensure no dominant strategy. Player chooses based on current resources and strategic priorities.

---

## 4. PROGRESSION SYSTEMS

### 4.1 Unified Five-Stat System

**Single progression path across all challenge types:**
- **Insight** - Pattern recognition, analysis, understanding
- **Rapport** - Empathy, connection, emotional intelligence
- **Authority** - Command, decisiveness, power
- **Diplomacy** - Balance, patience, measured approach
- **Cunning** - Subtlety, strategy, risk management

**Cross-System Manifestation:**
- Same stat manifests differently per challenge type (Insight analyzes evidence in Mental, reads terrain in Physical, sees hidden agendas in Social)
- Every card bound to one stat (playing any card grants XP to that stat)
- Stat levels gate card depth access (Insight 10 unlocks Tier 3 Insight cards)
- Build diversity through specialization vs generalization

### 4.2 Understanding (Cross-System Mastery)

Global resource unlocking higher card tiers in ALL challenge types:
- Victory in any challenge grants Understanding
- Represents growing tactical mastery generally
- Tier 1 (0-5): Basic cards only
- Tier 2 (6-10): Intermediate tactics
- Tier 3 (11-15): Advanced tactics
- Tier 4 (16+): Mastery level

### 4.3 NPC Bonds & StoryCubes

Relationship tracking through StoryCubes (conversational threads):
- Cold (0-2): Wary, minimal help
- Neutral (3-4): Polite, standard interactions
- Friendly (5-6): Helpful, occasional favors
- Close (7-8): Trusted, reliable support
- Deep (9-10): Intimate, extraordinary help

Benefits scale with relationship level: discounts, shortcuts, exclusive information, mechanical advantages

### 4.4 Content Unlocking Through Rewards

No boolean gates. Content spawns through typed rewards:
- LocationUnlock reward → Location becomes accessible
- TagApply reward → New content becomes eligible (tag-based spawning)
- SceneSpawn reward → New scene added to GameWorld
- Regions unlock progressively through A-story completion

---

## 5. TACTICAL LAYER: THREE CHALLENGE SYSTEMS

### 5.1 Common Challenge Structure

**Five Core Resources:**
- **Builder Resource** - Primary progress (Progress/Breakthrough/Momentum)
- **Session Resource** - Tactical capacity (Attention/Exertion/Initiative)
- **Threshold Resource** - Danger toward failure (Exposure/Danger/Doubt)
- **Flow Mechanic** - System-specific pacing (Leads/Aggression/Cadence)
- **Understanding** - Global persistent tier-unlocker

**Action Pair Pattern:**
- Each system has two primary actions creating tactical rhythm
- Primary action: Spend session resource, build progress
- Secondary action: Reset/trigger accumulated effects

### 5.2 Mental Challenges (Pauseable Investigations)

**Session Model:** Pauseable static puzzle
- Progress persists across multiple visits
- Exposure persists (investigative footprint)
- Attention resets per session
- Can leave mid-investigation and return

**Core Resources:**
- **Progress** (Builder) - Accumulates toward victory threshold, persists
- **Attention** (Session) - Derived from Focus stat, resets per visit
- **Exposure** (Threshold) - Investigative footprint, increases costs but doesn't force failure
- **Leads** (Flow) - Generated by ACT, determines OBSERVE card draw count

**Action Pair:** ACT (investigate, generate Leads) / OBSERVE (draw Details equal to Leads)

**Why Pauseable:** Investigations take time in fiction, multiple visits narratively coherent

### 5.3 Physical Challenges (One-Shot Obstacles)

**Session Model:** One-shot commitment
- Breakthrough resets each attempt (no persistence)
- Must complete or fail in single session
- Cannot pause and return
- High-stakes immediate test

**Core Resources:**
- **Breakthrough** (Builder) - Progress toward victory, resets per attempt
- **Exertion** (Session) - Derived from Stamina stat, limited per attempt
- **Danger** (Threshold) - Reaching MaxDanger causes immediate injury/failure
- **Aggression** (Flow) - Overcautious to Reckless spectrum affecting costs/effects

**Action Pair:** EXECUTE (lock Option as preparation) / ASSESS (trigger all locked Options as combo)

**Why One-Shot:** Physical obstacles are immediate tests in fiction, can't pause mid-climb

### 5.4 Social Challenges (Session-Bounded Conversations)

**Session Model:** Session-bounded by NPC agency
- Momentum resets per conversation
- Must complete or NPC ends conversation (MaxDoubt reached)
- Can voluntarily leave before completion
- NPC patience limited

**Core Resources:**
- **Momentum** (Builder) - Progress toward convincing NPC, resets per conversation
- **Initiative** (Session) - Accumulated via Foundation cards, required for Statements
- **Doubt** (Threshold) - NPC frustration, reaching MaxDoubt ends conversation
- **Cadence** (Flow) - Conversation rhythm affecting Initiative generation and Doubt

**Action Pair:** SPEAK (advance via Statements building Momentum) / LISTEN (reset hand, manage flow)

**Why Session-Bounded:** Conversations have natural endpoints determined by NPC agency

---

## 6. INFINITE A-STORY ARCHITECTURE

### 6.1 Two-Phase Design

**Phase 1: Authored Tutorial (A1-A10)**
- Hand-crafted scenes teaching mechanics (30 minutes current, expandable to 2-4 hours)
- Fixed sequence establishing pursuit framework
- Gradual mechanical introduction
- Specific entity references (tutorial NPCs, starting locations)
- Triggers procedural transition at completion

**Phase 2: Procedural Continuation (A11+ → ∞)**
- Scene archetype selection from catalog (20-30 archetypes)
- Entity resolution via categorical filters (no hardcoded IDs)
- AI narrative generation from player history
- Escalating scope over time (local → regional → continental → cosmic)
- Never ends, never resolves, always deepens

### 6.2 Guaranteed Progression Requirements

**Every A-story situation MUST have:**
1. **Zero-requirement path** - Always visible and selectable
2. **Cannot-fail mechanism** - Instant success OR both victory/defeat advance
3. **Forward progress** - Spawns next scene or transitions to next situation

This architectural guarantee prevents soft-locks in infinite game (player cannot restart at 50+ hours)

### 6.3 Procedural Generation Process

**Seven-Step Pipeline:**

1. **Build AI Context** - Player history, recent choices, entities involved, progression tier
2. **Select Archetype** - Choose from catalog avoiding repetition, match progression tier
3. **Resolve Entities** - Categorical filters find appropriate NPCs/Locations/Routes
4. **Generate Mechanical Structure** - Apply archetype, scale via entity properties
5. **AI Enriches Narrative** - Generate scene text, situation narratives, choice actions
6. **Validate Structure** - Verify guaranteed success path, forward progress from all states
7. **Spawn via HIGHLANDER** - Add to GameWorld.Scenes, mark Active, apply SpawnConditions

**Anti-Repetition Mechanisms:**
- Track last 5 archetypes used, filter out recent
- Track last 3 regions visited, prefer different regions
- Rotate entities (avoid same NPC in consecutive scenes)
- Tier escalation (A1-A20 local, A21-A40 regional, A41+ continental)

---

## 7. CONTENT GENERATION & ARCHETYPE SYSTEMS

### 7.1 Archetype Philosophy

**Archetypes define mechanical structure independent of narrative:**
- Choice count (typically 4)
- Path types per choice (InstantSuccess/Challenge/Fallback distribution)
- Base cost formulas (stat threshold, coin cost, challenge difficulty)
- Base reward formulas (resource restoration, state changes)

**Archetypes do NOT specify:**
- Narrative text (AI-generated from entity context)
- Specific entity IDs (uses placement/filters)
- Absolute numeric values (uses base × multipliers)

### 7.2 Categorical Property Scaling

**Entity properties drive contextual difficulty automatically:**

**NPCDemeanor:**
- Friendly: 0.6× stat threshold (easier)
- Neutral: 1.0× baseline
- Hostile: 1.4× stat threshold (harder)

**Quality:**
- Basic: 0.6× coin cost (cheap)
- Standard: 1.0× baseline
- Premium: 1.6× coin cost (expensive)
- Luxury: 2.4× coin cost (very expensive)

**PowerDynamic:**
- Dominant: 0.6× authority check (player has power)
- Equal: 1.0× balanced
- Submissive: 1.4× authority check (NPC has power)

**EnvironmentQuality:**
- Basic: 1.0× restoration (minimal)
- Standard: 2.0× restoration (good)
- Premium: 3.0× restoration (exceptional)

**Formula:** FinalValue = BaseValue × PropertyMultiplier₁ × PropertyMultiplier₂ × ProgressionMultiplier

**Result:** Same archetype + different properties = contextually appropriate difficulty

### 7.3 AI Generation Enablement

**AI writes categorical descriptions:**
```json
{
  "npcId": "elena_innkeeper",
  "demeanor": "Friendly",
  "quality": "Standard"
}
```

**Catalogues translate to balanced numbers (parse-time only):**
```
service_negotiation archetype:
  BaseStatThreshold = 5
  BaseCoinCost = 8

Context scaling:
  NPCDemeanor.Friendly = 0.6×
  Quality.Standard = 1.0×

Result:
  StatThreshold = 5 × 0.6 = 3 (easy)
  CoinCost = 8 × 1.0 = 8 (standard)
```

AI describes fiction ("friendly innkeeper"), system derives mechanics (easier negotiation). No AI balance knowledge required.

### 7.4 Three-Tier Archetype Hierarchy

**TIER 1: Core Archetypes (5 fundamental patterns)**
- Confrontation (Authority/dominance)
- Negotiation (Diplomacy/trade)
- Investigation (Insight/discovery)
- Request Fulfillment (fetch/deliver)
- Service (recovery/preparation)

**TIER 2: Expanded Archetypes (10 specialized variants)**
- Checkpoint, Transaction, Information Gathering, Evidence Analysis, Multi-Phase Fetch, Temporal Service, etc.

**TIER 3: Scene Archetypes (12 multi-situation structures)**
- Service with Location Access, Investigation Hub-and-Spoke, Arrival Sequence, etc.

Mathematical variety: 21 situation archetypes × 3 demeanors × 4 qualities × 3 power dynamics × 3 environment qualities = **2,268 base mechanical variations**

Add narrative variety from AI = effectively infinite content

---

## 8. BALANCE PHILOSOPHY & DIFFICULTY SCALING

### 8.1 Core Balance Principles

**Perfect Information Enables Optimization**
- Players see exact costs, rewards, requirements before commitment
- Challenge emerges from resource allocation strategy
- Not from hidden information or surprise penalties

**Resource Scarcity Creates Challenge**
- Difficulty from managing finite resources across competing demands
- Not from arbitrary thresholds that can't be met
- Shared resources force impossible choices

**Specialization Creates Identity**
- Multiple viable builds (specialist vs generalist vs hybrid)
- Trade-offs exist for all approaches
- No universally "correct" build

**Guaranteed Progression via Four-Choice Pattern**
- Orthogonal resource costs (stat/money/skill/time)
- Player chooses HOW to progress, never IF
- Different builds experience different costs

### 8.2 Progression Curve

**Early Game (A1-A20):**
- Tight margins (5-10 coins profit)
- Limited options (low stats)
- Tutorial safety nets (generous rewards)
- Teaching phase

**Mid Game (A21-A40):**
- Economic buffer possible (15-25 coins)
- Specialization emerges (4-6 in focused stats)
- Tactical depth accessible
- Build identity forms

**Late Game (A41+):**
- Higher scale (30-50 coins) but margins stay proportionally tight
- Mastery through optimization (6-8 in specialized stats)
- Scope escalation (local → regional → continental)
- Never trivial through power creep

**Progression via scope, not power:** Geographic reach and narrative complexity increase, not statistical dominance

### 8.3 Difficulty Scaling Mechanisms

**Categorical Property Scaling** - Entity properties drive appropriate difficulty
**Route Opacity** - Known routes show exact info, unknown routes require estimation
**Economic Pressure** - Tight margins throughout, never abundant
**Build Vulnerabilities** - Specialization creates strengths AND weaknesses
**Sweet Spots Matter** - Balance points exist, extremes have costs

---

## 9. DESIGN PATTERNS & ANTI-PATTERNS

### 9.1 Essential Patterns

**Four-Choice Pattern**
- Stat-gated optimal / Money-gated reliable / Challenge risky / Fallback guaranteed
- Orthogonal resource costs prevent dominant strategy
- Architectural guarantee of forward progress

**Tag-Based Dependencies**
- Content spawns when player has required tags
- State accumulated through rewards creates organic flow
- No hardcoded linear chains, flexible graph structure

**Service Flow Pattern**
- Negotiation → Execution → Departure
- Reusable 3-phase linear arc
- Self-contained service context

**Archetype Composition**
- Scene archetypes compose Situation archetypes
- Two-tier generation with reward enrichment
- Mechanical reuse with contextual variety

**Infinite Obligation Pattern**
- Persistent mystery container spawns Scenes sequentially
- CurrentPhase increments indefinitely
- Authored Phase 1-3, Procedural Phase 4+

**Arrival Pattern**
- Arrival → Exploration → Engagement → Departure
- Repeating rhythm creates travel consistency
- Variety within structure

### 9.2 Anti-Patterns (Forbidden)

**Boolean Gate** - "Complete A to unlock B" (use resource costs instead)
**Dictionary-Based Type System** - Generic storage hiding relationships (use strong typing)
**Multi-Purpose Entities** - One entity serving multiple roles (one purpose per entity)
**Hidden Requirements** - Mystery gates (use perfect information)
**Backwards Relationships** - Location owns Scenes (Location doesn't create Scenes)
**Circular Dependencies** - Systems calling each other (explicit one-way connections only)

---

## 10. TUTORIAL PHILOSOPHY

### 10.1 Teaching Principles

**Teach Through Natural Play** - No disconnected exercises, mechanics emerge from fiction
**Gradual Complexity** - One concept at a time, layer as understanding demonstrated
**Safety Nets Early** - Generous margins and forgiving difficulty initially, gradually remove
**Perfect Information Display** - All costs visible, requirements transparent, gaps shown
**Implicit Guidance** - Route learning, economic feedback, no quest markers
**Discovery Rewards** - Optimization, shortcuts, synergies found through play

### 10.2 Tutorial Sequence

**First 30 Minutes (A1):**
- Resource display (Health, Stamina, coins visible)
- Four-choice pattern introduction
- Guaranteed progression demonstration
- Basic navigation

**Next 30 Minutes (A2):**
- Stat gating (see locked paths, understand gaps)
- First Mental challenge (easy, forgiving)
- Challenge basics (Action/Foundation/Discard)

**Hours 2-3 (A3):**
- NPC bonds and StoryCubes
- Multi-situation scenes
- Social challenges
- Time block introduction

**Hours 4-6 (Expandable A4-A10):**
- All three challenge types
- Complex economic trade-offs
- Build specialization choices
- Full system integration

**Hour 6+ (A11+):**
- Procedural continuation triggered
- Full complexity accessible
- Player-driven priorities

---

## 11. MAJOR DESIGN DECISIONS (DDRs)

### DDR-001: Infinite Procedural A-Story
**Decision:** Never-ending journey without resolution
**Rationale:** Eliminates ending pressure, enables player agency, supports infinite content, matches core fantasy
**Trade-Off:** Sacrifices narrative closure for infinite replayability

### DDR-002: Tag-Based Dependencies
**Decision:** State-based spawning via RequiresTags/GrantsTags
**Rationale:** Flexible progression graph, no hardcoded chains, supports branching
**Trade-Off:** More complex validation vs rigid linear simplicity

### DDR-003: Unified 5-Stat System
**Decision:** Single stat progression across all challenge types
**Rationale:** Cross-challenge advancement, no wasted effort, thematic coherence
**Trade-Off:** Less system-specific flavor vs consistent progression

### DDR-004: Tight Economic Margins
**Decision:** Delivery earnings barely cover survival costs
**Rationale:** Creates impossible choices, rewards optimization, maintains tension
**Trade-Off:** Requires careful balance tuning vs player-friendly abundance

### DDR-005: Strategic-Tactical Separation
**Decision:** Strict two-layer architecture with perfect info at strategic, hidden complexity at tactical
**Rationale:** Enables informed decisions, maintains tactical engagement
**Trade-Off:** Implementation complexity vs unified gameplay layer

### DDR-006: Categorical Scaling
**Decision:** Relative properties translated to concrete values via catalogues
**Rationale:** Enables AI generation without balance knowledge, infinite scaling
**Trade-Off:** No hand-tuning vs granular per-instance control

### DDR-007: Four-Choice Pattern
**Decision:** All A-story situations have stat/money/challenge/fallback paths
**Rationale:** Guarantees forward progress, no soft-locks architecturally
**Trade-Off:** Predictable structure vs variable choice counts

### DDR-008: Delivery Courier Core Loop
**Decision:** Travel-based profession as economic foundation
**Rationale:** Coherent with travel fantasy, justifies route exploration
**Trade-Off:** Profession-specific context vs flexible backstory

### DDR-009: Three Challenge Types
**Decision:** Mental/Physical/Social parallel systems
**Rationale:** Thematic coverage, playstyle variety, unified progression
**Trade-Off:** Three systems to maintain vs single challenge type

### DDR-010: Pauseable Mental Challenges
**Decision:** Different session models per challenge type
**Rationale:** Verisimilitude (investigations take time, obstacles are immediate, conversations bounded)
**Trade-Off:** Complex session management vs uniform model

---

## 12. KEY TERMINOLOGY

### Core Gameplay Terms

**Scene** - Multi-situation narrative container (3-4 situations forming coherent arc)
**Situation** - Single decision point presenting 2-4 choices
**Choice** - Player-selectable option with visible cost/reward
**Action** - Executable game instruction (route to tactical or apply effects)
**Route** - Path connecting two locations via hex-based travel with segment encounters

**A-Story** - Infinite procedural main storyline, never-ending journey
**B-Story** - Authored side content for resource accumulation
**C-Story** - Small encounters and services

**Obligation** - Persistent mystery container spawning Scenes sequentially

### Challenge Terms

**Strategic Layer** - Scene/Situation/Choice flow with perfect information
**Tactical Layer** - Card-based challenge gameplay with hidden complexity

**Builder Resource** - Primary progress toward victory (Progress/Breakthrough/Momentum)
**Session Resource** - Tactical capacity per session (Attention/Exertion/Initiative)
**Threshold Resource** - Danger toward failure (Exposure/Danger/Doubt)
**Flow Mechanic** - System-specific pacing (Leads/Aggression/Cadence)

**Victory Threshold** - Builder resource value required for challenge success
**Session Model** - Challenge continuation rules (pauseable/one-shot/session-bounded)

### Progression Terms

**StoryCube** - Conversational thread tracking NPC relationship
**Bond Level** - Relationship strength with NPC (0-10 scale)
**Understanding** - Cross-system card tier unlock currency
**Card Depth** - Power tier (Tier 1-4) gated by stat levels

### Economy Terms

**Economic Pressure** - Resource scarcity driving strategic decisions
**Impossible Choice** - Multiple valid options, insufficient resources for all
**Tight Margins** - Delivery earnings barely covering survival costs (3-7 coins net)
**Resource Arithmetic** - Numeric comparisons (≥ thresholds) vs boolean gates
**Orthogonal Costs** - Different resource types preventing dominant strategy

### Content Generation Terms

**Archetype** - Reusable mechanical pattern independent of narrative
**Categorical Property** - Descriptive entity attribute enabling dynamic scaling (Friendly/Hostile, Basic/Luxury)
**Entity-Driven Generation** - Difficulty emerging from entity properties vs hardcoded values
**Archetype Composition** - Scene archetypes composing Situation archetypes
**Marker Resolution** - Template references resolved at spawn-time (generated:private_room → actual GUID)

### Architecture Terms

**HIGHLANDER Principle** - "There can be only one" instantiation path per entity
**Catalogue Pattern** - Parse-time translation from categorical to concrete values
**Perfect Information** - All strategic costs/rewards visible before commitment
**Guaranteed Progression** - Zero-requirement fallback path architecturally mandated
**Tag-Based Spawning** - Content eligibility via RequiresTags/GrantsTags system

---

## 13. DESIGN PHILOSOPHY SUMMARY

**Strategic Depth Through Constraint**
- Impossible choices from shared resource scarcity
- Specialization creates identity through vulnerability
- Mastery via optimization, not power accumulation

**Fiction Serves Mechanics**
- Narrative coherent justification for mechanical requirements
- Verisimilitude where possible, mechanics when necessary
- Story subordinate to player agency

**No Soft-Locks Ever**
- Architectural guarantee in infinite game
- Four-choice pattern with zero-requirement fallback
- Forward progress from every state

**Perfect Information at Strategic Layer**
- All costs, requirements, rewards visible before commitment
- Resource arithmetic over boolean gates
- Players calculate, game doesn't trick

**Procedural Infinity with Quality Guarantees**
- AI generates entities categorically (no balance knowledge needed)
- System translates to balanced mechanics via catalogues
- Structural validation ensures no soft-locks
- Effectively infinite content from finite archetypes

**Tight Margins Throughout**
- Scale increases (10 coins early, 80 coins late)
- Proportional pressure maintained
- Optimization always rewarded
- Never trivial through abundance

**Build Diversity Through Trade-Offs**
- Multiple viable specializations
- No universally correct build
- All approaches have strengths and vulnerabilities
- Player values expressed through resource prioritization

---

**End of Unified Design Document**

This document consolidates all 12 game design files into a single comprehensive reference. For technical implementation details, see arc42 architectural documentation. For specific deep-dives, refer to original design documents.