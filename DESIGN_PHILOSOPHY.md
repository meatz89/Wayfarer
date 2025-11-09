# WAYFARER DESIGN PHILOSOPHY

## Core Design Philosophy

Wayfarer creates strategic depth through impossible choices, not mechanical complexity. Every decision forces the player to choose between multiple suboptimal paths, revealing character through constraint.

### Fundamental Principles

- **Elegance Over Complexity**: Every mechanic serves exactly one purpose
- **Verisimilitude Throughout**: All mechanics make narrative sense in a grounded historical-fantasy world
- **Perfect Information**: All calculations visible to the player
- **No Soft-Lock Architecture**: Always a path forward, even if suboptimal
- **Atmosphere Through Mechanics**: Danger, preparation, and discovery create immersion
- **⚠️ PLAYABILITY OVER IMPLEMENTATION**: A game that compiles but is unplayable is WORSE than a game that crashes

---

## PRIME DIRECTIVE: PLAYABILITY FIRST

Before ANY content is marked complete, answer with 9/10 certainty:

### 1. Can the player REACH this content from game start?
- Trace EXACT player action path from spawn to content
- Verify ALL links (routes, scenes, NPCs, locations)
- No missing connections, broken references, or inaccessible islands

### 2. Can the player SEE this content in UI?
- Actions appear as clickable buttons/cards
- Scene cards render in location view
- Routes visible in travel screen
- NPCs show interaction options

### 3. Can the player EXECUTE this content?
- Actions execute without errors
- Costs deducted correctly
- Rewards applied as expected
- Navigation works (challenge → location → challenge)

### 4. Does player have FORWARD PROGRESS from every state?
- No soft-locks (trapped states)
- No dead-ends (locations with no exits)
- No orphaned content (systems with no entry points)

**The Playability Test:**
Can you trace a COMPLETE PATH of player actions from game start to your content? If ANY link is broken, missing, or inaccessible, the content is NOT PLAYABLE.

- Path EXISTS (all links present in JSON/code)
- Path is FUNCTIONAL (all links execute correctly)
- Path is DISCOVERABLE (player can find through normal play)
- No BROKEN LINKS (no null routes, missing scenes, inaccessible NPCs)

The number of actions is IRRELEVANT. What matters: Path exists, path works, player can find it.

---

## Ten Core Design Principles

### Principle 1: Single Source of Truth with Explicit Ownership

Every entity type has exactly one owner. GameWorld owns all runtime entities via flat lists. Parent entities reference children by ID, never inline at runtime. Child entities reference parents by ID for lifecycle queries.

**Test:** Can you name the owner in one word?

---

### Principle 2: Strong Typing as Design Enforcement

If you can't express relationships with strongly typed objects and lists, the design is wrong. Strong typing forces explicit relationships.

Collections of specific entity types, not generic containers.

**Test:** Can you draw the object graph with clear arrows?

---

### Principle 3: Ownership vs Placement vs Reference

Three distinct relationship types. Don't conflate them.

- **OWNERSHIP** (lifecycle control): Parent creates child, parent destroys child
- **PLACEMENT** (presentation context): Entity appears at a location for UI/narrative purposes, lifecycle independent
- **REFERENCE** (lookup relationship): Entity A needs to find Entity B, stores B's ID, no lifecycle dependency

**Test:** If Entity A is destroyed, should Entity B be destroyed? Yes = Ownership, No = Placement or Reference

---

### Principle 4: Inter-Systemic Rules Over Boolean Gates

Strategic depth emerges from shared resource competition, never through linear unlocks.

**FORBIDDEN:** "If completed A, unlock B" (no resource cost, no opportunity cost)

**REQUIRED:** Shared resource competition, opportunity costs, trade-offs

**Test:** Does the player make a strategic trade-off (accepting one cost to avoid another)? If no, it's a boolean gate.

---

### Principle 5: Typed Rewards as System Boundaries

Systems connect through explicitly typed rewards applied at completion, not through continuous evaluation or state queries.

System A completes action → Applies typed reward → Reward has specific effect on System B → Effect applied immediately

**Test:** Is the connection a one-time application of a typed effect, or a continuous check of boolean state? First is correct, second is wrong.

---

### Principle 6: Resource Scarcity Creates Impossible Choices

For strategic depth to exist, all systems must compete for the same scarce resources.

**Shared Resources (Universal Costs):**
- Time (segments)
- Focus
- Stamina
- Health

**System-Specific Resources (Tactical Only):**
- Momentum/Progress/Breakthrough
- Initiative/Cadence/Aggression

**The Impossible Choice:** "I can afford to do A OR B, but not both. Both paths are valid. Both have genuine costs. Which cost will I accept?"

**Test:** Can the player pursue all interesting options without trade-offs? If yes, no strategic depth exists.

---

### Principle 7: One Purpose Per Entity

Every entity type serves exactly one clear purpose.

**Test:** Can you describe the entity's purpose in one sentence without "and" or "or"? If not, split it.

---

### Principle 8: Verisimilitude in Entity Relationships

Entity relationships should match the conceptual model, not implementation convenience.

**Correct (matches concept):**
- Scenes spawn from Obligations
- Situations contain Choices
- Scenes appear at Locations

**Wrong (backwards):**
- Locations own Scenes
- NPCs own Routes

**Test:** Does the explanation feel backwards or forced?

---

### Principle 9: Elegance Through Minimal Interconnection

Systems should connect at explicit boundaries, not pervasively.

**Correct:** System A produces typed output, System B consumes typed input, ONE connection point, clean boundary

**Wrong:** System A sets flags in SharedData, System B queries multiple flags, tangled web

**Test:** Can you draw the system interconnections with one arrow per connection? If you need a web of arrows, too many dependencies.

---

### Principle 10: Perfect Information with Hidden Complexity

All strategic information visible to player. All tactical complexity hidden in execution.

**Strategic Layer (Always Visible):**
- What scenes/situations are available
- Costs
- Rewards
- Requirements
- Current world state

**Tactical Layer (Hidden Until Engaged):**
- Specific cards
- Exact card draw order
- Precise challenge flow

**Test:** Can the player make an informed decision about WHETHER to attempt a scene before entering the challenge? If not, strategic layer is leaking tactical complexity.

---

## Meta-Principle: Design Constraint as Quality Filter

When you find yourself reaching for:
- Dictionaries of generic types
- Boolean flags checked continuously
- Multi-purpose entities
- Ambiguous ownership
- Hidden costs

**STOP. The design is wrong.**

These aren't implementation problems to solve with clever code. They're signals that the game design itself is flawed. Strong typing and explicit relationships aren't constraints that limit you - they're filters that catch bad design before it propagates.

**Good design feels elegant. Bad design requires workarounds.**

---

## Unified Stat System Architecture

Wayfarer uses a **single unified progression system** across all three challenge types (Mental, Physical, Social). Five stats govern all tactical gameplay:

- **Insight**: Pattern recognition, analysis, understanding
- **Rapport**: Empathy, connection, emotional intelligence
- **Authority**: Command, decisiveness, power
- **Diplomacy**: Balance, patience, measured approach
- **Cunning**: Subtlety, strategy, risk management

### Card Binding and Depth Access

Every card in every system is bound to one of these five stats. This binding determines:

1. **Depth Access**: Player's stat level determines which card depths they can access
2. **XP Gain**: Playing any card grants XP to its bound stat - Single unified progression across all three challenge types

### Stat Manifestation Across Challenge Types

The same stat manifests differently depending on challenge type:

| Stat | Mental Manifestation | Physical Manifestation | Social Manifestation |
|------|---------------------|----------------------|---------------------|
| **Insight** | Pattern recognition in evidence, analyzing crime scenes, deductive reasoning | Structural analysis, reading terrain features, finding weaknesses | Understanding NPC motivations, reading between lines, seeing hidden agendas |
| **Rapport** | Empathetic observation, understanding human element, sensing emotions | Flow state, body awareness, natural movement, athletic grace | Building emotional connection, creating trust, resonating with feelings |
| **Authority** | Commanding the scene, decisive analysis, authoritative conclusions | Decisive action, power moves, commanding environment | Asserting position, directing conversation, establishing dominance |
| **Diplomacy** | Balanced investigation, patient observation, measured approach | Measured technique, controlled force, pacing endurance | Finding middle ground, compromise, balanced conversation |
| **Cunning** | Subtle investigation, covering tracks, tactical information gathering | Risk management, tactical precision, adaptive technique | Strategic conversation, subtle manipulation, reading and responding |

### Why Unified Stats Matter

- **Single Progression Path**: Playing any challenge type improves capabilities across all systems
- **Thematic Coherence**: Stats represent fundamental character traits that manifest differently
- **Build Variety**: Specialize (few stats) or generalize (all stats), distinct playstyles
- **No Wasted Effort**: Every card played contributes to unified character progression

---

## Strategic-Tactical Architecture

Wayfarer operates on two distinct layers that separate decision-making from execution:

### Strategic Layer

The strategic layer handles **what** and **where** decisions:
- **Venue Selection**: Choose which venue to visit from travel map
- **Scene Selection**: Choose which investigation or request to pursue
- **Resource Planning**: Evaluate equipment, requirements, and stat thresholds
- **Risk Assessment**: Consider danger levels, time limits, exposure thresholds

Strategic decisions happen in safe spaces with perfect information. Player can see all requirements, evaluate all options, make informed choices about which tactical challenges to attempt.

### Tactical Layer

The tactical layer handles **how** execution through three parallel challenge systems:

**Mental Challenges** - Location-based investigations using observation and deduction
**Physical Challenges** - Location-based obstacles using strength and precision
**Social Challenges** - NPC-based conversations using rapport and persuasion

Each system follows same structural pattern:
- **Builder Resource**: Primary progress toward victory (Progress, Breakthrough, Momentum)
- **Threshold Resource**: Accumulating danger toward failure (Exposure, Danger, Doubt)
- **Session Resource**: Tactical capacity for playing cards (Attention from Focus, Exertion from Stamina, Initiative from Foundation cards)
- **Flow Mechanic**: System-specific card flow
- **Action Pair**: Two actions creating tactical rhythm
- **Understanding**: Shared tier-unlocking resource across all systems

### Bridge: Strategic to Tactical

**Strategic Layer (Scene → Situation → Choice):**
- **Scene**: Persistent narrative container in GameWorld, contains situations, tracks progression
- **Situation**: Narrative moment with 2-4 choices, embedded in scene, presents options to player
- **Choice**: Player option with visible costs/requirements/rewards, determines execution path

**The Bridge (ChoiceTemplate.ActionType):**
- **Instant**: Stay in strategic layer (apply costs/rewards immediately)
- **Navigate**: Stay in strategic layer (move to location/NPC)
- **StartChallenge**: Cross to tactical layer (spawn Mental/Physical/Social challenge)

**Tactical Layer (Mental/Physical/Social Challenges):**
- **Challenge Session**: Temporary card-based gameplay (MentalSession, PhysicalSession, SocialSession)
- **SituationCards**: Victory conditions with thresholds, extracted from parent Situation
- **Tactical Cards**: Playable cards (MentalCard, PhysicalCard, SocialCard) from challenge deck

This architecture ensures:
- **Clear Separation**: Strategic (WHAT to attempt) vs Tactical (HOW to execute)
- **Perfect Information**: Strategic layer visible, tactical layer emergent
- **Explicit Bridge**: ChoiceTemplate.ActionType routes between layers
- **One-Way Flow**: Strategic spawns tactical, tactical returns outcome
- **Layer Purity**: Situations are strategic, challenges are tactical (never conflate)

---

## Challenge System Design Patterns

### Mental Challenges - Investigation at Locations

Mental investigations can be paused and resumed, respecting the reality that investigations take time:

**Session Model: Pauseable Static Puzzle**
- Can pause anytime: Leave location, state persists exactly where you left off
- Progress persists: Accumulates at location across multiple visits
- Exposure persists: Investigative "footprint" increases difficulty
- Attention resets: Return with fresh mental energy after rest
- No forced ending: High Exposure makes investigation harder but doesn't force failure
- Incremental victory: Reach Progress threshold across multiple visits

**Core Session Resources**
- **Progress** (builder, persists): Accumulates toward completion across sessions
- **Attention** (session budget, resets): Mental capacity for ACT cards, derived from permanent Focus
- **Exposure** (threshold, persists): Starts at 0, accumulates as investigative footprint
- **Leads** (observation flow, persists): Investigative threads generated by ACT cards
- **Understanding** (global, persistent): Tier unlocking across all systems

**Action Pair: ACT / OBSERVE**
- **ACT**: Take investigative action, spend Attention, generate Leads, build Progress
- **OBSERVE**: Follow investigative threads, draw Details equal to Leads count

**Key Mechanic:** You cannot observe what you haven't investigated. ACT generates Leads. OBSERVE follows Leads.

---

### Physical Challenges - Obstacles at Locations

Physical challenges are immediate tests of current capability:

**Session Model: One-Shot Test**
- Single attempt: Must complete or fail in one session
- No challenge persistence: Each attempt starts fresh
- Personal state carries: Health/Stamina persist between challenges
- Danger threshold = consequences: Reaching max Danger causes injury/failure immediately
- Must complete now: Cannot pause halfway and return later

**Core Session Resources**
- **Breakthrough** (builder): Progress in single session toward victory
- **Exertion** (session budget): Physical capacity for EXECUTE cards, derived from permanent Stamina
- **Danger** (threshold): Starts at 0, accumulates from risky actions
- **Aggression** (balance): Overcautious to Reckless spectrum
- **Understanding** (global, persistent): Tier unlocking across systems

**Action Pair: EXECUTE / ASSESS**
- **EXECUTE**: Lock Option as preparation, spend Exertion, build prepared sequence
- **ASSESS**: Evaluate Situation, trigger all locked Options as combo

---

### Social Challenges - Conversations with NPCs

Conversations are real-time interactions with entities that have agency:

**Session Model: Session-Bounded Dynamic Interaction**
- Session-bounded: Must complete in single interaction
- MaxDoubt ends: NPC frustration forces conversation end when Doubt reaches maximum
- No pause/resume: Cannot pause mid-conversation and return
- Can Leave early: Voluntarily end conversation (consequences to relationship)
- Relationship persists: StoryCubes/relationship state remember between conversations

**Core Session Resources**
- **Momentum** (builder): Progress toward goal in single conversation session
- **Initiative** (session): Action economy currency, accumulated via Foundation cards
- **Doubt** (threshold): NPC frustration toward conversation end
- **Cadence** (balance): Conversation rhythm and pacing
- **Understanding** (global, persistent): Tier unlocking across systems

**Action Pair: SPEAK / LISTEN**
- **SPEAK**: Advance conversation through Statements
- **LISTEN**: Reset and draw, take in new information

---

## Summary

These design principles form the philosophical foundation of Wayfarer's architecture. They prioritize:

1. **Playability** over implementation completeness
2. **Elegance** over mechanical complexity
3. **Strategic depth** through resource competition
4. **Perfect information** at strategic layer
5. **Verisimilitude** in all systems
6. **No soft-locks** in any game state

Every architectural decision should trace back to these principles. When in doubt, return to: **Does this create impossible choices through resource scarcity while maintaining perfect information and forward progress?**

If yes, the design is sound. If no, reconsider.
