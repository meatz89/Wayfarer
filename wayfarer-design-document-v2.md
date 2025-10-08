# Wayfarer: Game Design Document

## Core Design Philosophy

### Fundamental Principles

Wayfarer creates strategic depth through impossible choices, not mechanical complexity. Every decision forces players to choose between multiple suboptimal paths, revealing character through constraint.

- **Elegance Over Complexity**: Every mechanic serves exactly one purpose
- **Verisimilitude Throughout**: All mechanics make narrative sense in a grounded historical-fantasy world
- **Perfect Information**: All calculations visible to players
- **No Soft-Lock Architecture**: Always a path forward, even if suboptimal
- **Atmosphere Through Mechanics**: Danger, preparation, and discovery create immersion

### Genre Definition

Low-fantasy historical CRPG with visual novel dialogue and resource management. Text-based with simple decisions. Atmosphere like Frieren or Roadwarden - player versus nature, relationships matter, small personal scale.

**Not included:** Modern concepts, checkpoints/guards, mountain passes, epic scope. **Focus:** Small timeframes (days), short distances (walking), actions spanning seconds/minutes like D&D sessions.

## Unified Stat System Architecture

### Five Universal Stats

Wayfarer uses a **single unified progression system** across all three challenge types (Mental, Physical, Social). Five stats govern all tactical gameplay:

- **Insight**: Pattern recognition, analysis, understanding
- **Rapport**: Empathy, connection, emotional intelligence
- **Authority**: Command, decisiveness, power
- **Diplomacy**: Balance, patience, measured approach
- **Cunning**: Subtlety, strategy, risk management

### Card Binding and Depth Access

**Every card in every system** is bound to one of these five stats. This binding determines:

1. **Depth Access**: Player's stat level determines which card depths they can play
   - Level 1: Access depths 1-2 (Foundation cards only)
   - Level 2: Access depths 1-3
   - Level 3: Access depths 1-4 (Standard cards)
   - Level 5: Access depths 1-6 (Advanced cards)
   - Level 7: Access depths 1-8 (Powerful cards)
   - Level 9+: Access depths 1-10 (Master cards)

2. **XP Gain**: Playing any card grants XP to its bound stat
   - Mental "Insight-bound Observation" → grants Insight XP
   - Physical "Authority-bound Power Move" → grants Authority XP
   - Social "Rapport-bound Friendly Approach" → grants Rapport XP
   - **Single unified progression**: All three challenge types level the same five stats

### Stat Manifestation Across Challenge Types

The same stat manifests differently depending on challenge type, creating thematic coherence while maintaining mechanical consistency:

| Stat | Mental Manifestation | Physical Manifestation | Social Manifestation |
|------|---------------------|----------------------|---------------------|
| **Insight** | Pattern recognition in evidence, analyzing crime scenes, deductive reasoning | Structural analysis, reading terrain features, finding weaknesses | Understanding NPC motivations, reading between lines, seeing hidden agendas |
| **Rapport** | Empathetic observation, understanding human element, sensing emotions | Flow state, body awareness, natural movement, athletic grace | Building emotional connection, creating trust, resonating with feelings |
| **Authority** | Commanding the scene, decisive analysis, authoritative conclusions | Decisive action, power moves, commanding environment | Asserting position, directing conversation, establishing dominance |
| **Diplomacy** | Balanced investigation, patient observation, measured approach | Measured technique, controlled force, pacing endurance | Finding middle ground, compromise, balanced conversation |
| **Cunning** | Subtle investigation, covering tracks, tactical information gathering | Risk management, tactical precision, adaptive technique | Strategic conversation, subtle manipulation, reading and responding |

### Card Examples Across Systems

**Insight-Bound Cards**:
- **Mental**: "Detailed Examination" (depth 4) - Systematic observation of evidence (+3 Progress, +1 Understanding)
- **Physical**: "Structural Analysis" (depth 4) - Identify load-bearing points (+2 Breakthrough, reveal optimal path)
- **Social**: "Read Motivation" (depth 4) - Understand NPC's true desires (+2 Momentum, reduce Doubt)

**Rapport-Bound Cards**:
- **Mental**: "Empathetic Reading" (depth 4) - Sense emotional context (+2 Progress, +1 to next Social at location)
- **Physical**: "Flow State" (depth 6) - Natural fluid movement (+3 Breakthrough, costs 1 less Exertion)
- **Social**: "Emotional Resonance" (depth 5) - Connect on feeling level (+3 Momentum, +1 Rapport token)

**Authority-Bound Cards**:
- **Mental**: "Command Scene" (depth 5) - Take control of investigation space (+3 Progress, +1 Exposure from obviousness)
- **Physical**: "Power Move" (depth 6) - Decisive forceful action (+4 Breakthrough, +1 Danger)
- **Social**: "Assert Position" (depth 5) - Direct conversation firmly (+3 Momentum, +1 Cadence)

**Diplomacy-Bound Cards**:
- **Mental**: "Patient Observation" (depth 4) - Methodical investigation (+2 Progress, -1 Exposure from care)
- **Physical**: "Measured Technique" (depth 4) - Controlled application of force (+2 Breakthrough, efficient technique)
- **Social**: "Find Common Ground" (depth 4) - Seek compromise (+2 Momentum, -1 Cadence)

**Cunning-Bound Cards**:
- **Mental**: "Cover Tracks" (depth 4) - Investigate without leaving traces (+2 Progress, -1 Exposure)
- **Physical**: "Calculated Risk" (depth 5) - Tactical risk assessment (+3 Breakthrough, -1 Danger)
- **Social**: "Strategic Deflection" (depth 5) - Subtle redirection (+2 Momentum, manipulate Cadence)

### Why Unified Stats Matter

**Single Progression Path**: Playing any challenge type improves your capabilities across all systems. Mental investigations improve your Social Insight cards. Physical challenges improve your Mental Authority cards. Social conversations improve your Physical Cunning cards.

**Thematic Coherence**: Stats represent fundamental character traits that manifest differently in different contexts. A highly Insightful character excels at observation (Mental), structural analysis (Physical), and reading people (Social).

**Build Variety**: Players can specialize (focus on 2-3 stats) or generalize (develop all stats evenly), creating distinct playstyles that affect all three challenge types simultaneously.

**No Wasted Effort**: Every card played in every system contributes to unified character progression.

## Strategic-Tactical Architecture

Wayfarer operates on two distinct layers that separate decision-making from execution:

### Strategic Layer
The strategic layer handles **what** and **where** decisions:
- **Location Selection**: Choose which location to visit from travel map
- **Goal Selection**: Choose which investigation phase or NPC request to pursue
- **Resource Planning**: Evaluate equipment, knowledge, and stat requirements
- **Risk Assessment**: Consider danger levels, time limits, exposure thresholds

Strategic decisions happen in safe spaces (taverns, towns, travel map) with perfect information. Players can see all requirements, evaluate all options, and make informed choices about which tactical challenges to attempt.

### Tactical Layer
The tactical layer handles **how** execution through three parallel challenge systems:

**Mental Challenges** - Location-based investigations using observation and deduction
**Physical Challenges** - Location-based obstacles using strength and precision
**Social Challenges** - NPC-based conversations using rapport and persuasion

Each system is a distinct tactical game with unique resources and action pairs, but all follow a common structural pattern:
- **Builder Resource**: Primary progress toward victory (Progress, Breakthrough, Momentum)
- **Threshold Resource**: Accumulating danger toward failure (Exposure, Danger, Doubt)
- **Session Resource**: Tactical capacity for playing cards (Attention from Focus, Exertion from Stamina, Initiative from Foundation cards)
- **Balance Tracker**: Bipolar scale affecting gameplay (ObserveActBalance, Commitment, Cadence)
- **Action Pair**: Two actions creating tactical rhythm (OBSERVE/ACT, ASSESS/EXECUTE, SPEAK/LISTEN)
- **Understanding**: Shared tier-unlocking resource across all three systems

The systems are parallel implementations of the same architectural pattern, not identical mechanics with different theming.

### Bridge: LocationGoals
LocationGoals bridge the strategic and tactical layers. When a player selects an investigation phase or NPC request (strategic decision), the system spawns a LocationGoal at the specified location. Visiting that location reveals the goal as a card option, which when selected launches the appropriate tactical challenge (Mental, Physical, or Social).

This architecture ensures:
- **Clear Separation**: Strategic planning never mixes with tactical execution
- **Perfect Information**: All requirements visible before committing to tactical challenge
- **Architectural Consistency**: Three systems follow same structural pattern
- **Mechanical Diversity**: Each system offers distinct tactical gameplay

## Three Core Loops

### Conversations (Primary Gameplay)

Card-based dialogue system using builder/spender dynamics. Five resources (Initiative, Momentum, Doubt, Cadence, Statements) create tactical decisions. Personality rules fundamentally alter approach.

**Purpose:**
- Information gathering for challenges
- Relationship building for practical support
- Stat progression through experience
- Primary expression of player methodology

**Integration:** NPCs provide route knowledge, equipment sources, investigation clues, danger warnings. Conversations serve adventure goals.

### Obligation Journal (Adventure Hooks)

Simple quest log tracking active requests from NPCs. No complex management - obligations are reasons to explore dangerous places, not logistics puzzles.

**Structure:**
- Accept delivery/task from NPC
- Destination becomes adventure location
- Weather/time creates natural urgency
- Completion provides payment, relationships, access

**Purpose:** Drive player toward interesting content (dangerous routes, mysterious locations, challenging investigations).

### Investigation & Travel (Three Challenge Systems)

Investigations are multi-phase mysteries resolved through Mental, Physical, and Social tactical challenges. Each phase spawns a LocationGoal at a specific location, bridging strategic planning to tactical execution.

**Three Challenge Types:**
- **Mental Challenges**: Observation-based investigation at location spots (examine scenes, deduce patterns, search thoroughly)
  - Resources: Progress (builder), Attention (session budget from Focus), Exposure (threshold), ObserveActBalance (balance)
  - Actions: OBSERVE (cautious builder) / ACT (aggressive spender)

- **Physical Challenges**: Strength-based obstacles at location spots (climb carefully, leverage tools, precise movement)
  - Resources: Breakthrough (builder), Exertion (session budget from Stamina), Danger (threshold), Commitment (balance)
  - Actions: ASSESS (cautious builder) / EXECUTE (aggressive spender)

- **Social Challenges**: Rapport-based conversations with NPCs (build trust, gather information, persuade cooperation)
  - Resources: Momentum (builder), Initiative (session builder), Doubt (threshold), Cadence (balance), Statements (history)
  - Actions: SPEAK (advance conversation) / LISTEN (reset and draw)

Each system is a distinct tactical game following the same architectural pattern: builder resource toward victory, threshold resource toward failure, session-scoped spender, balance tracker affecting gameplay, and binary action choice creating tactical rhythm.

**Investigation Lifecycle:**
1. **Discovery**: Triggered by observation, conversation, item discovery, or obligation
2. **Phase Spawning**: Each phase creates LocationGoal at specified location with requirements
3. **Strategic Planning**: Evaluate requirements (knowledge, equipment, stats, completed phases)
4. **Tactical Execution**: Visit location, select goal card, complete Mental/Physical/Social challenge
5. **Progression**: Victory grants discoveries, unlocks subsequent phases, builds toward completion

**AI-Generated Content**: Investigation templates define structure (phases, requirements, rewards). AI generates specific content (locations, NPCs, narratives, card text) from templates, creating unique investigations that follow proven mechanical patterns.

**Purpose:** Bridge strategic decision-making to tactical challenge execution. Investigations provide context and progression, challenges provide gameplay.

## NPC System

### Five Persistent Decks

Each NPC maintains:
1. **Conversation Deck**: Base cards for their conversation types
2. **Request Deck**: Available tasks and deliveries
3. **Observation Deck**: Cards gained from your discoveries
4. **Burden Deck**: Consequences of failures and conflicts
5. **Exchange Deck**: Trade opportunities and special purchases

### Relationship Progression

Relationships deepen gradually through:
- Completed deliveries (earn trust)
- Information shared (prove resourcefulness)
- Challenges overcome together (demonstrate competence)
- Time spent in conversation (build familiarity)

**Mechanical Benefits:**
- Signature cards appear in conversations
- Access to specialized equipment/items
- Route knowledge and safer paths revealed
- Investigation clues and context provided
- Support during dangerous challenges

### Personality Rules

Each NPC has conversational personality affecting tactical approach:
- **Proud**: Must play cards in ascending Initiative order
- **Devoted**: Doubt accumulates faster (+2 instead of +1)
- **Mercantile**: Highest Initiative card gets bonus effect
- **Cunning**: Repeated Initiative costs penalty
- **Steadfast**: All effects capped at ±2

These transform the conversation puzzle while maintaining core mechanics.

## Investigation System

### Architecture Overview

Investigations are multi-phase mysteries that bridge strategic planning to tactical execution through three parallel challenge systems (Mental, Physical, Social). Each investigation consists of discrete phases resolved through card-based tactical challenges, with AI-generated content built from authored templates.

### Investigation Lifecycle

**1. Potential State**
Investigation exists as template but not yet discovered by player. Waiting for trigger condition.

**2. Discovery Triggers (Five Types)**

**Immediate Visibility** - Investigation visible upon entering location
- Obvious environmental features (collapsed bridge, abandoned waterwheel)
- Public knowledge mysteries (town's water problem, missing merchant)
- Location-inherent investigation opportunities

**Environmental Observation** - Triggered by examining location features
- Player examines specific location spot, reveals hidden investigation
- "You notice something unusual about the mill mechanism..."
- Requires player attention, rewards thorough exploration

**Conversational Discovery** - NPC dialogue reveals investigation existence
- NPC mentions mystery during conversation
- Grants observation card mentioning the investigation
- Playing that card in subsequent conversation spawns investigation

**Item Discovery** - Finding specific item triggers related investigation
- Discover torn letter → spawns investigation about its sender
- Find broken mechanism part → spawns investigation about sabotage
- Physical evidence creates investigation context

**Obligation-Triggered** - Accepting NPC request spawns investigation
- Merchant asks you to investigate waterwheel
- Investigation spawns when obligation accepted
- Direct causal link between social commitment and investigation

**3. Active State**
Investigation discovered, phases available based on requirements. Player can see phase structure, requirements, current progress. Investigation persists across sessions - partial progress retained, can retreat and return prepared.

**4. Completion**
All required phases completed. Investigation marked complete, rewards granted, knowledge added to player state. May unlock subsequent investigations or alter world state.

### Phase Structure

Each investigation consists of 3-7 phases resolved sequentially or in parallel (based on requirements):

**Phase Definition:**
- **System Type**: Mental, Physical, or Social challenge
- **Challenge Type**: Specific challenge configuration (victory threshold, danger threshold, deck)
- **Requirements**: Knowledge, equipment, stats, completed phases
- **Location Assignment**: Where to find this phase (location + spot for Mental/Physical, NPC for Social)
- **Progress Threshold**: Victory points needed to complete phase
- **Completion Reward**: Discoveries granted, phases unlocked, narrative reveals

**Phase Requirements:**
- **Completed Phases**: Must finish phases 1-3 before phase 4 unlocks
- **Knowledge**: Must have discovered specific information ("mill_mechanism_broken")
- **Equipment**: Must possess specific items ("rope", "crowbar")
- **Stats**: Minimum Observation, Strength, Rapport thresholds

**Phase Spawning:**
When requirements met, phase spawns LocationGoal at specified location. Mental/Physical goals appear at location spots as card options. Social goals target specific NPCs, creating conversation opportunities with investigation context.

### Three Challenge Systems Detailed

All three systems provide equivalent tactical depth through parallel architecture. Each interacts with different elements of the game world (locations, obstacles, NPCs) and respects verisimilitude through appropriate session models and resource costs.

---

## Mental Challenges - Investigation at Locations

**Interaction Model**: Player investigates static locations (crime scenes, mysterious sites, evidence locations)

### Session Model: Pauseable Static Puzzle

Mental investigations can be paused and resumed, respecting the reality that investigations take time:

- **Can pause anytime**: Leave location, state persists exactly where you left off
- **Progress persists**: Accumulates at location across multiple visits
- **Exposure persists**: Your investigative "footprint" at location increases difficulty
- **Attention resets**: Return with fresh mental energy after rest
- **No forced ending**: High Exposure makes investigation harder but doesn't force failure
- **Incremental victory**: Reach Progress threshold across multiple visits (typically 10-20 total)
- **Verisimilitude**: Real investigations take days/weeks with breaks between sessions

### Core Session Resources

- **Progress** (builder, persists): Accumulates toward completion (10-20 total typical)
- **Attention** (session budget, resets): Mental capacity for ACT cards, derived from permanent Focus at challenge start (max determined by current Focus level). **Cannot replenish during investigation** - must rest outside challenge to restore.
- **Exposure** (persistent penalty): Investigative footprint at location (no max, higher = harder future visits)
- **ObserveActBalance** (balance): Cautious (-10) vs reckless (+10) investigation style
- **Understanding** (global, persistent): Tier unlocking across all three systems

### Action Pair

- **OBSERVE**: Cautious investigation, build Progress slowly, shift toward overcautious, lower Exposure risk, **does not restore Attention**
- **ACT**: Aggressive investigation, spend Attention, build Progress faster, higher Exposure risk

### Stat Binding Examples

Mental cards bind to stats based on investigative approach:

**Insight-Bound** (pattern recognition, deduction):
- "Detailed Examination" (depth 4): Systematic evidence observation (+3 Progress, +1 Understanding)
- "Pattern Analysis" (depth 6): Connect disparate clues (+4 Progress, reveal hidden connections)
- "Scientific Method" (depth 8): Rigorous hypothesis testing (+5 Progress, +2 Understanding)

**Cunning-Bound** (subtle investigation, covering tracks):
- "Cover Tracks" (depth 4): Investigate without leaving traces (+2 Progress, -1 Exposure)
- "Subtle Investigation" (depth 6): Gather evidence discreetly (+3 Progress, -2 Exposure)
- "Misdirection" (depth 8): Redirect attention while investigating (+4 Progress, manipulate Exposure)

**Authority-Bound** (commanding scene, decisive analysis):
- "Command Scene" (depth 5): Take investigative control (+3 Progress, +1 Exposure from obviousness)
- "Authoritative Analysis" (depth 7): Make definitive conclusions (+5 Progress, +2 Exposure)
- "Investigative Authority" (depth 9): Assert investigative dominance (+6 Progress, force breakthroughs)

**Diplomacy-Bound** (balanced approach, patience):
- "Patient Observation" (depth 4): Methodical investigation (+2 Progress, -1 Exposure)
- "Balanced Approach" (depth 6): Steady measured investigation (+3 Progress, maintain Exposure)
- "Thorough Method" (depth 8): Complete systematic coverage (+4 Progress, +1 Understanding)

**Rapport-Bound** (empathetic observation, human element):
- "Empathetic Reading" (depth 4): Sense emotional context (+2 Progress, +1 to next Social at location)
- "Human Element Focus" (depth 6): Understand people involved (+3 Progress, unlock NPC leads)
- "Emotional Reconstruction" (depth 8): Recreate emotional state (+5 Progress, deep NPC insights)

### Permanent INPUT Resources (Costs to Attempt)

**Focus** (max 100, Mental-specific):
- Cost: 5-20 Focus per investigation session (depending on complexity)
- Depletion effect: <30 Focus → Exposure accumulates faster (+1 per action)
- Recovery: Rest blocks (+30 per block), light activity, food
- Verisimilitude: Mental work depletes concentration

### Permanent OUTPUT Resources (Rewards from Success)

1. **Knowledge Discoveries**: Unlock investigation phases, conversation options, world state changes
2. **Familiarity Tokens** (per location): +1 per successful investigation, reduce Exposure baseline (-1 per token, max -3 at location)
3. **Investigation Depth Level** (per location): Cumulative Progress unlocks expertise (Surface 0-20 → Detailed 20-50 → Deep 50-100 → Expert 100+)
4. **Understanding**: Tier unlocking across all systems
5. **Stat XP**: Level unified stats via bound cards
6. **Coins**: Investigation completion rewards (5-20 coins typical)
7. **Equipment**: Find items during investigations

### Investigation Profiles (5 Tactical Modifiers)

Each location has an investigation profile that fundamentally alters tactics:

1. **Delicate** (fragile evidence, high Exposure risk): Exposure +2 per action, requires Cunning-focused approach
2. **Obscured** (degraded/hidden evidence): Progress -1 per action, requires Insight breakthroughs
3. **Layered** (complex mystery): Goal cards require 2+ stat types used, requires diverse approach
4. **Time-Sensitive** (degrading evidence): +1 Exposure per time segment spent, requires efficient Authority
5. **Resistant** (subtle patterns): Progress capped at ±2, requires patient Diplomacy grinding

### Exhaustion Mechanics

**Evidence Cards**: One-time clues that exhaust after ACT (represents using up physical evidence)
**Insight Cards**: Fleeting observations that exhaust after OBSERVE (represents momentary clarity)

### Victory Condition

Accumulate Progress threshold (10-20 typical) across one or more visits to location. High Exposure doesn't end investigation but makes future visits harder.

### Example Phase

"Examine the waterwheel mechanism" - Mental challenge, Delicate profile, 15 Progress threshold, at Mill Waterwheel spot, requires 10 Focus, costs 1 time segment per session

---

## Physical Challenges - Obstacles at Locations

**Interaction Model**: Player attempts immediate physical tests (climbing, combat, athletics, finesse, endurance)

### Session Model: One-Shot Test

Physical challenges are immediate tests of current capability:

- **Single attempt**: Must complete or fail in one session
- **No challenge persistence**: Each attempt at challenge starts fresh
- **Personal state carries**: Your Health/Stamina persist between challenges
- **Danger threshold = consequences**: Reaching max Danger causes injury/failure immediately
- **Must complete now**: Cannot pause halfway and return later
- **Verisimilitude**: Can't pause halfway up a cliff or mid-combat

### Core Session Resources

- **Breakthrough** (builder): Toward victory in single session (10-20 typical)
- **Exertion** (session budget): Physical capacity for EXECUTE cards, derived from permanent Stamina at challenge start (max determined by current Stamina level). **Cannot replenish during challenge except through specific Foundation cards** like "Deep Breath" which restore small amounts.
- **Danger** (threshold): Accumulates from risky actions, max typically 10-15 before injury
- **Commitment** (balance): Cautious (-10) vs decisive (+10) physical approach
- **Understanding** (global, persistent): Tier unlocking across all three systems

### Action Pair

- **ASSESS**: Cautious analysis, build Breakthrough slowly, shift toward overcautious, lower Danger, **does not restore Exertion**
- **EXECUTE**: Decisive action, spend Exertion, build Breakthrough faster, higher Danger risk

### Stat Binding Examples

Physical cards bind to stats based on physical approach:

**Authority-Bound** (power, decisive action, command):
- "Power Move" (depth 6): Decisive forceful action (+4 Breakthrough, +1 Danger)
- "Commanding Presence" (depth 4): Assert control over environment (+3 Breakthrough, +1 Commitment)
- "Dominant Force" (depth 8): Overwhelming physical assertion (+6 Breakthrough, +2 Danger)

**Cunning-Bound** (risk management, tactical precision):
- "Calculated Risk" (depth 5): Tactical risk assessment (+3 Breakthrough, -1 Danger)
- "Tactical Precision" (depth 7): Precise calculated movement (+4 Breakthrough, restore 1 Exertion)
- "Adaptive Technique" (depth 9): Respond to changing conditions (+5 Breakthrough, manipulate Danger)

**Insight-Bound** (structural analysis, finding weaknesses):
- "Structural Analysis" (depth 4): Identify load-bearing points (+2 Breakthrough, reveal optimal path)
- "Find Weakness" (depth 6): Spot structural vulnerabilities (+3 Breakthrough, reduce Danger)
- "Engineering Assessment" (depth 8): Complete structural understanding (+4 Breakthrough, reduce Exertion cost of next card)

**Rapport-Bound** (flow state, body awareness, grace):
- "Flow State" (depth 6): Natural fluid movement (+3 Breakthrough, costs 1 less Exertion)
- "Body Awareness" (depth 4): Kinesthetic understanding (+2 Breakthrough, maintain balance)
- "Athletic Grace" (depth 8): Perfect physical harmony (+4 Breakthrough, reduce Danger)

**Diplomacy-Bound** (measured technique, controlled force):
- "Measured Technique" (depth 4): Controlled application of force (+2 Breakthrough, efficient technique)
- "Controlled Force" (depth 6): Balanced power application (+3 Breakthrough, maintain Danger)
- "Paced Endurance" (depth 8): Sustainable effort (+4 Breakthrough, reduce exhaustion)

### Permanent INPUT Resources (Costs to Attempt)

**Health** (max 100, Physical-specific):
- Risk: Danger threshold consequences damage Health (5-15 damage typical)
- Depletion effect: <30 Health → Danger accumulates faster (+1 per action)
- Recovery: Rest blocks, medical treatment, restorative food
- Verisimilitude: Physical challenges risk injury

**Stamina** (max 100, Physical-specific):
- Cost: 10-30 Stamina per challenge attempt (depending on difficulty)
- Depletion effect: <30 Stamina → Max Exertion reduced (start challenges with lower Exertion capacity)
- Recovery: Rest blocks, food, reduced activity
- Verisimilitude: Physical exertion drains energy

### Permanent OUTPUT Resources (Rewards from Success)

1. **Mastery Tokens** (per challenge type): +1 per success at Challenge Type (Combat/Athletics/Finesse/Endurance/Strength), reduce Danger baseline (-1 per token, max -3 per type)
2. **Challenge Proficiency Level** (per challenge type): Cumulative Breakthrough unlocks expertise (Novice 0-20 → Competent 20-50 → Skilled 50-100 → Master 100+)
3. **Equipment Discoveries**: Find items during physical challenges
4. **Reputation**: Affect Social interactions (physical feats build reputation)
5. **Understanding**: Tier unlocking across all systems
6. **Stat XP**: Level unified stats via bound cards
7. **Coins**: Challenge completion rewards (5-15 coins typical)

### Challenge Type Profiles (5 Types of Physical Engagement)

Physical challenges are categorized by engagement type, not terrain:

1. **Combat** (tactical fighting): Authority/Cunning/Insight-focused, high Danger from aggression, Exertion = tactical advantage
2. **Athletics** (climbing/running/jumping): Insight/Rapport/Cunning-focused, Danger from falls, Exertion = stable footing
3. **Finesse** (lockpicking/delicate work): Cunning/Insight/Diplomacy-focused, Danger from mistakes, Exertion = control/steadiness
4. **Endurance** (long marches/holding out): Rapport/Diplomacy/Authority-focused, Danger from exhaustion, Exertion = sustainable pace
5. **Strength** (lifting/breaking/forcing): Authority/Insight/Diplomacy-focused, Danger from strain, Exertion = mechanical advantage

### Exhaustion Mechanics

**Desperation Cards**: All-in risky moves that exhaust after EXECUTE (represents committing fully)
**Setup Cards**: One-time advantages that exhaust after ASSESS (represents preparing position)

### Victory Condition

Reach Breakthrough threshold (10-20 typical) in single attempt. Reaching Danger maximum causes injury and failure.

### Example Phase

"Climb the damaged mill wheel" - Physical challenge, Athletics type, 15 Breakthrough threshold, 12 Danger maximum, at Mill exterior, costs 20 Stamina, risks 10 Health on failure

---

## Social Challenges - Conversations with NPCs

**Interaction Model**: Player converses with dynamic entities (NPCs with personalities, agency, memory)

### Session Model: Session-Bounded Dynamic Interaction

Conversations are real-time interactions with entities that have agency:

- **Session-bounded**: Must complete in single interaction (conversation happens in real-time)
- **Doubt=10 ends**: NPC frustration forces conversation end (dynamic entity response)
- **No pause/resume**: Cannot pause mid-conversation and return (unrealistic with dynamic entity)
- **Can Leave early**: Voluntarily end conversation (consequences to relationship)
- **Session clears**: Resources reset on conversation end
- **Relationship persists**: Connection tokens/level remember you between conversations
- **Verisimilitude**: Conversations happen continuously with entities who have patience limits

### Core Session Resources

- **Momentum** (builder): Progress toward goal (8-16 typical in single session)
- **Initiative** (session): Action economy currency, accumulated via Foundation cards, persists through LISTEN (max 10)
- **Doubt** (threshold): NPC skepticism/frustration, maximum 10 ends conversation
- **Cadence** (balance): Dominating (+10) vs deferential (-10) conversation style
- **Statements** (history): Count of Statement cards played, determines time cost (1 segment + Statements)
- **Understanding** (global, persistent): Tier unlocking across all three systems

### Action Pair

- **SPEAK**: Play card, advance conversation, increment Cadence (+1)
- **LISTEN**: Reset action, draw cards (base 3 + bonuses from negative Cadence), decrement Cadence (-2), apply Doubt penalty from positive Cadence

### Stat Binding Examples

Social cards bind to stats based on conversational approach (existing system):

**Insight**: Read motivations, understand subtext, see hidden agendas
**Rapport**: Emotional connection, empathy, resonating with feelings
**Authority**: Assert position, direct conversation, establish dominance
**Diplomacy**: Find middle ground, compromise, balanced conversation
**Cunning**: Strategic conversation, subtle manipulation, reading and responding

### Permanent INPUT Resources (Costs to Attempt)

**None**: Conversation is "free" in terms of permanent resources, but costs time (1 segment + Statements count)
- Verisimilitude: Talking doesn't deplete resources (but takes time)

### Permanent OUTPUT Resources (Rewards from Success)

1. **Connection Tokens** (per NPC): Rapport, Trust, Commitment (affect mechanics with that specific NPC)
2. **Connection Level** (per NPC): Stranger → Acquaintance → Friend → Close Friend (unlocks options, reduces Doubt accumulation)
3. **NPC Observation Cards**: Knowledge discoveries add cards to THAT NPC's conversation deck
4. **Understanding**: Tier unlocking across all systems
5. **Stat XP**: Level unified stats via bound cards
6. **Coins**: From completing NPC requests/rewards (10-50 coins typical)
7. **Equipment**: From NPC gifts/trades
8. **Knowledge**: From NPC information sharing
9. **Route Knowledge**: NPCs reveal hidden paths

### Personality Rules (5 Tactical Modifiers)

Each NPC has personality that fundamentally alters conversation tactics:

1. **Proud**: Must play cards in ascending Initiative order (rewards planning)
2. **Devoted**: Doubt accumulates +2 per action (requires efficiency)
3. **Mercantile**: Highest Initiative card gets bonus effect (rewards hoarding Initiative)
4. **Cunning**: Repeated Initiative costs penalty (requires variety)
5. **Steadfast**: All effects capped at ±2 (requires patient grinding)

### Social-Specific Mechanics (No Mental/Physical Equivalent)

**Request Cards**: NPCs offer obligations (locations/challenges don't - they lack agency)
**Promise Cards**: Commitments to NPCs (social contracts)
**Burden Cards**: Relationship damage consequences

These exist ONLY in Social because NPCs have agency to offer tasks, make demands, and damage relationships. Locations and challenges lack agency - they don't "offer" anything or "remember" slights.

### Exhaustion Mechanics

**Opening Cards**: Exhaust on LISTEN (relationship building moments)
**Impulse Cards**: Exhaust on SPEAK (emotional outbursts)

### Victory Condition

Reach Momentum threshold (8-16 typical) before Doubt reaches 10 (which ends conversation).

### Example Phase

"Question the mill owner about sabotage" - Social challenge, Proud personality, 12 Momentum threshold, targets Mill_Owner NPC at Mill location, costs 0 resources, takes 1+ time segments

---

## Common Architectural Pattern

All three systems follow the same core structure (creating equivalent tactical depth):

**Universal Elements** (identical across all three):
- **Unified 5-stat system**: All cards bind to Insight/Rapport/Authority/Diplomacy/Cunning
- **Builder Resource** → Victory (Progress, Breakthrough, Momentum)
- **Threshold Resource** → Failure consequence (Exposure, Danger, Doubt)
- **Session Resource** → Tactical spending (Attention, Exertion, Initiative)
- **Balance Tracker** → Playstyle modifier (ObserveActBalance, Commitment, Cadence)
- **Binary Action Choice** → Tactical rhythm (OBSERVE/ACT, ASSESS/EXECUTE, SPEAK/LISTEN)
- **Understanding** → Persistent tier unlocking (shared globally)
- **5 Tactical Modifiers** → Fundamental gameplay variety (Profiles, Challenge Types, Personalities)
- **Expertise Tokens** → Mechanical benefits from repeated success (Familiarity, Mastery, Connection)
- **Progression Levels** → Cumulative expertise tracking (Depth, Proficiency, Connection)
- **Exhaustion Mechanics** → One-time use cards for tactical timing
- **Goal Cards** → Victory condition cards unlock at thresholds

**Intentional Differences** (justified by verisimilitude):
- **Session Models**: Mental (pauseable), Physical (one-shot), Social (session-bounded)
- **Resource Persistence**: Mental resources persist at location, Physical/Social clear on end
- **Permanent Costs**: Mental (Focus), Physical (Health+Stamina), Social (none, but time)
- **Session Resource Mechanics**:
  - Mental Attention: Derived from Focus at start, finite budget, **no replenishment during challenge**
  - Physical Exertion: Derived from Stamina at start, finite budget, **minimal replenishment via specific Foundation cards only**
  - Social Initiative: Starts at 0, **actively builds during session via Foundation cards**, persists through LISTEN
- **Special Systems**: Social (Request/Promise/Burden from NPC agency), Mental/Physical (none - locations/challenges lack agency)

**Result**: Three systems with equivalent tactical depth (all ~1,000-1,100 lines of implementation) achieved through parallel architecture that respects the different natures of what you interact with (entities vs places vs challenges).

## Expertise and Progression Systems

All three challenge types feature parallel systems for tracking and rewarding repeated engagement:

### Token Systems (Immediate Mechanical Benefits)

Tokens provide direct mechanical advantages from repeated success:

**Mental: Familiarity Tokens** (per location)
- **Earned**: +1 token per successful investigation at specific location
- **Effect**: Reduce Exposure baseline at that location (-1 Exposure per token, maximum -3)
- **Stacks**: Accumulate up to 3 tokens per location
- **Verisimilitude**: You learn how to investigate this place safely and discreetly
- **Example**: 3 Familiarity tokens at Mill → all future investigations at Mill start with -3 Exposure baseline

**Physical: Mastery Tokens** (per challenge type)
- **Earned**: +1 token per successful challenge of specific type (Combat, Athletics, Finesse, Endurance, Strength)
- **Effect**: Reduce Danger baseline for that challenge type (-1 Danger per token, maximum -3)
- **Stacks**: Accumulate up to 3 tokens per challenge type (15 tokens total across 5 types)
- **Verisimilitude**: Experience with challenge type reduces inherent risk
- **Example**: 3 Mastery tokens in Athletics → all future Athletics challenges start with -3 Danger baseline

**Social: Connection Tokens** (per NPC)
- **Earned**: Through successful conversations and specific card effects with that NPC
- **Types**: Rapport (emotional bond), Trust (reliability), Commitment (mutual obligation)
- **Effect**: Affect conversation mechanics with that specific NPC (reduce Doubt accumulation, enable special cards, affect thresholds)
- **Stacks**: Multiple tokens of each type per NPC
- **Verisimilitude**: NPCs remember your relationship history and respond accordingly
- **Example**: 2 Rapport tokens with Mill Owner → reduced Doubt accumulation in conversations

### Progression Level Systems (Long-Term Expertise)

Progression levels track cumulative mastery and unlock escalating benefits:

**Mental: Investigation Depth** (per location)

Cumulative Progress at location determines expertise level:

- **Surface** (0-20 cumulative Progress): Basic observations, standard card access
- **Detailed** (20-50 cumulative): Patterns emerge, unlock depth-specific observation cards
- **Deep** (50-100 cumulative): Hidden connections visible, advanced cards available
- **Expert** (100+ cumulative): Complete reconstruction capability, master cards unlocked

**Benefits by Level**:
- Higher levels unlock better Mental cards specific to that location type
- Expert-level investigators see connections novices miss
- Represents accumulated investigative experience at this specific place

**Verisimilitude**: Real investigators develop deep expertise with specific locations through repeated visits

**Physical: Challenge Proficiency** (per challenge type)

Cumulative Breakthrough per challenge type determines mastery:

- **Novice** (0-20 cumulative Breakthrough): Basic techniques, standard card access
- **Competent** (20-50 cumulative): Efficient movement, unlock advanced techniques
- **Skilled** (50-100 cumulative): Advanced techniques available, tactical variety
- **Master** (100+ cumulative): Risk mitigation expertise, master techniques unlocked

**Benefits by Level**:
- Higher levels unlock better Physical cards for that challenge type
- Masters execute techniques that overwhelm novices
- Represents accumulated physical expertise with this engagement type

**Verisimilitude**: Athletes and fighters develop specialized mastery through repeated practice

**Social: Connection Levels** (per NPC)

Relationship depth through accumulated interactions:

- **Stranger** (0 interactions): Base conversation difficulty, limited options
- **Acquaintance** (1-3 successful conversations): Reduced Doubt accumulation, basic trust
- **Friend** (4-7 successful conversations): Significant Doubt reduction, personal conversations unlocked
- **Close Friend** (8+ successful conversations): Minimal Doubt, deep personal topics available

**Benefits by Level**:
- Higher levels reduce Doubt accumulation in conversations
- Deeper relationships unlock personal conversation branches
- Close friends forgive mistakes more readily (Doubt penalty reduction)

**Verisimilitude**: Real relationships deepen through accumulated positive interactions over time

### Why Parallel Progression Matters

**Cross-System Growth**: All three systems contribute to unified stat progression (Insight/Rapport/Authority/Diplomacy/Cunning), while also building system-specific expertise (Familiarity/Mastery/Connection and Depth/Proficiency/Levels).

**Specialization vs Generalization**: Players can:
- Specialize in one challenge type (master Mental investigations)
- Generalize across all three (competent at everything)
- Mix strategies (expert Mental + competent Physical/Social)

**No Wasted Effort**: Every challenge attempted builds toward:
- Immediate token benefits (easier future attempts)
- Long-term progression (unlock better cards)
- Unified stat growth (benefit all systems)

**Verisimilitude**: Real people develop both general capabilities (stats) and specialized expertise (tokens/levels) through practice.

### Knowledge System (Connective Tissue)

Knowledge entries are structured discoveries that connect investigations, unlock phases, enhance conversations, and alter world state.

**Knowledge Structure:**
- **ID**: Unique identifier ("mill_sabotage_discovered")
- **Display Name**: Player-visible label ("Sabotage Evidence")
- **Description**: What player learned
- **Investigation Context**: Which investigation granted this
- **Unlock Effects**: Which phases/investigations this enables

**Knowledge Functions:**

**Phase Unlocking** - Knowledge gates investigation progression
- Phase 3 requires knowledge from Phase 1 completion
- Creates meaningful progression, prevents skipping ahead
- Example: Can't question suspect until you've examined crime scene

**Investigation Discovery** - Knowledge triggers new investigations
- Discovering "broken_cog" knowledge spawns "Sabotage Mystery" investigation
- Creates investigation chains and narrative continuity
- Knowledge from one investigation opens doors to related mysteries

**Conversation Enhancement** - Knowledge adds observation cards to NPC decks
- Discover "mill_sabotage" → gain observation card about sabotage
- Play that card in conversation with mill owner → special dialogue branch
- Knowledge creates conversation opportunities and advantages

**World State** - Knowledge alters NPC behavior and available options
- NPCs react differently when you possess certain knowledge
- Locations reveal additional options based on what you know
- Knowledge creates emergent narrative consequences

### AI-Generated Investigation Content

Investigations use **template-driven generation** where designers author mechanical structure, AI generates specific content:

**Authored Template (Designer):**
```
Investigation: Waterwheel Mystery
- Phase 1: Mental challenge, 10 Progress, at [LOCATION], requires [EQUIPMENT]
- Phase 2: Social challenge, 10 Progress, targets [NPC], requires Phase 1 complete
- Phase 3: Physical challenge, 12 Progress, at [LOCATION], requires knowledge from Phase 2
- Completion grants [KNOWLEDGE], unlocks [NEXT_INVESTIGATION]
```

**AI-Generated Content:**
- **Specific locations**: Mill, Waterwheel Spot, Mill Owner's House
- **Specific NPCs**: Mill Owner (Proud personality), Miller's Apprentice (Devoted)
- **Narrative text**: Phase descriptions, completion narratives, discovery text
- **Card text**: Challenge-specific card descriptions matching investigation theme
- **Knowledge entries**: "mill_mechanism_damaged", "sabotage_evidence", "suspect_identified"

**Generation Constraints:**
- Must respect mechanical template structure (phase count, challenge types, thresholds)
- Must create valid location/NPC references that exist in world
- Must generate knowledge IDs used by subsequent phases
- Must maintain narrative coherence across phases
- Must balance difficulty progression (early phases easier than late phases)

**Content Validation:**
- Parser validates all references exist in GameWorld (locations, NPCs, challenge types)
- [Oracle] principle: Fail at load time, not runtime
- Clear error messages identify specific validation failures

### Investigation Progression Examples

**Example 1: Linear Investigation**
```
Waterwheel Mystery (5 phases, linear progression)
├─ Phase 1: Observe waterwheel (Mental) → grants "mechanism_damaged"
├─ Phase 2: Question mill owner (Social, requires Phase 1) → grants "owner_evasive"
├─ Phase 3: Examine interior (Physical, requires "mechanism_damaged") → grants "sabotage_evidence"
├─ Phase 4: Confront suspect (Social, requires Phase 3) → grants "confession"
└─ Phase 5: Report findings (Social, requires Phase 4) → completes investigation
```

**Example 2: Branching Investigation**
```
Missing Merchant Mystery (6 phases, parallel paths)
├─ Phase 1: Question townspeople (Social) → grants "last_seen_location"
├─ Phase 2a: Search forest path (Mental, requires Phase 1)
├─ Phase 2b: Search riverside (Mental, requires Phase 1)
├─ Phase 3: Examine discovered evidence (Mental, requires 2a OR 2b) → grants "attack_evidence"
├─ Phase 4: Track attackers (Physical, requires Phase 3) → grants "bandit_camp_location"
└─ Phase 5: Confront bandits (Social, requires Phase 4) → completes investigation
```

**Example 3: Equipment-Gated Investigation**
```
Ancient Ruin Investigation (4 phases, equipment requirements)
├─ Phase 1: Examine exterior (Mental) → grants "sealed_entrance"
├─ Phase 2: Force entry (Physical, requires "crowbar" equipment) → grants "dark_interior"
├─ Phase 3: Explore interior (Mental, requires "lantern" equipment + Phase 2) → grants "ancient_text"
└─ Phase 4: Decipher inscriptions (Mental, requires "scholar_knowledge" + Phase 3) → completes
```

### Investigation UI Flow

**Discovery Notification**
Player discovers investigation → Modal appears with investigation name, description, initial phase visibility
- "New Investigation: The Waterwheel Mystery"
- "The waterwheel has stopped turning. The mill owner seems worried."
- Shows Phase 1 requirements and location

**Journal Integration**
Investigation added to Journal's Investigations tab:
- Shows all phases (locked/unlocked status)
- Displays requirements for locked phases
- Shows progress toward completion (2/5 phases complete)
- Tracks discovered knowledge related to this investigation

**Phase Completion**
Complete tactical challenge → Return to location screen → Modal appears:
- "Phase Complete: Examine Waterwheel"
- Narrative text describing what you learned
- Knowledge gained: "Mechanism Damaged"
- New phases unlocked (if any)
- Investigation progress updated in journal

**Investigation Complete**
Final phase complete → Modal appears:
- "Investigation Complete: The Waterwheel Mystery"
- Summary of discoveries
- Rewards granted (knowledge, items, access)
- Related investigations unlocked (if any)

## Travel System

### Core Concept

Travel presents real obstacles requiring preparation, equipment, knowledge, and careful choices. Not just stamina/time costs.

### Route Structure

**Visible Paths:**
When approaching travel between locations, visible options show:
- Basic description of path type
- General difficulty indication
- Obvious requirements (if any)

**Hidden Paths:**
Additional routes revealed through:
- NPC conversations providing knowledge
- Investigation discoveries finding hidden ways
- Relationship trust sharing local secrets
- Previous travel experience noting alternatives

### Travel Obstacles

**Terrain Challenges:**
Natural barriers requiring preparation:
- Dense forest (navigation difficulty, stamina cost)
- Water crossings (risk of exposure, equipment loss)
- Steep inclines (high stamina requirement, injury risk)
- Poor visibility (getting lost, missing safer paths)
- Distance (extended exposure to elements)

**Environmental Hazards:**
Weather and conditions affecting travel:
- Rain making paths slippery or impassable
- Cold requiring warm clothing
- Darkness limiting visibility and safety
- Time of day affecting safety and witnesses

**Equipment Requirements:**
Some paths require specific gear:
- Rope for difficult climbs
- Proper footwear for rough terrain
- Weather protection for exposure
- Light sources for darkness
- Tools for clearing obstacles

**Knowledge Gates:**
Routes fully navigable only with information:
- Hidden shortcuts requiring directions
- Dangerous sections avoidable with warnings
- Optimal timing known from local knowledge
- Safe camping spots for long journeys
- Alternate routes when primary blocked

### Travel Choices

Each obstacle presents options with different trade-offs:

**Force Through:**
- High stamina/health cost
- Risk of injury or getting lost
- Faster if successful
- Learn what preparation needed for next time

**Careful Approach:**
- Lower risk but more time
- Better chance of success
- Might discover information for future trips
- Safer with limited resources

**Alternate Route:**
- Avoid obstacle entirely
- Usually longer distance
- Might have own challenges
- Sometimes requires different preparation

**Turn Back:**
- Admit current preparation insufficient
- Preserve resources for better attempt
- Learn what's needed for success
- Can return when properly equipped

### Travel Preparation

**Before attempting challenging routes:**

Gather information:
- Talk to NPCs who know the area
- Learn about hazards and requirements
- Discover hidden paths or shortcuts
- Understand weather factors

Acquire equipment:
- Purchase or craft needed tools
- Ensure adequate supplies
- Prepare for specific challenges
- Consider backup equipment

Build capability:
- Ensure sufficient health/stamina
- Rest if depleted from previous activities
- Time departure for optimal conditions
- Plan for recovery after arrival

## Time & Resource Economy

### Time Structure

**Blocks and Segments:**
- Day contains 6 blocks (Morning, Midday, Afternoon, Evening, Night, Late Night)
- Each block contains 4 segments
- Total: 24 segments per day

**Block Properties:**
- Morning: Fresh start, NPCs in regular locations, quiet investigations
- Midday: Full activity, busy locations, normal operations
- Afternoon: Winding down, some locations closing, changing availability
- Evening: Social time, taverns busy, offices closed, different NPC locations
- Night: Most locations closed, investigation opportunities, travel risks
- Late Night: Sleep recommended, exhaustion penalties, emergency only

**Activity Costs:**
- Conversations: 1 segment + accumulated doubt
- Work: 4 segments (entire block)
- Investigation: 1-4 segments depending on depth
- Travel: Variable by route and obstacles
- Rest: Typically full block for recovery

### Resource Management

Wayfarer features permanent resources that persist across all gameplay. Three challenge-specific resources (Focus, Health, Stamina) create different strategic pressures:

**Focus** (Mental-specific permanent resource):
- Maximum: 100
- **Cost**: Mental investigations cost 5-20 Focus to initiate (depending on complexity)
- **Lost to**: Mental work, investigation sessions, intense concentration
- **Depletion effect**: Below 30 Focus → Exposure accumulates faster (+1 per action in Mental challenges)
- **Cannot attempt**: Mental investigations when Focus insufficient for session cost
- **Recovered through**: Rest blocks (+30 Focus per block), light activity, food, avoiding mental strain
- **Integration**: Must balance Mental investigations against other activities requiring mental clarity
- **Verisimilitude**: Concentration depletes with mental work, recovers with rest

**Health** (Physical-specific permanent resource):
- Maximum: 100
- **Risk**: Physical challenges risk Health (Danger threshold consequences deal 5-15 damage typical)
- **Lost to**: Physical hazards, environmental exposure, injuries from challenge failures, combat damage
- **Depletion effect**: Below 30 Health → Danger accumulates faster (+1 per action in Physical challenges)
- **Cannot attempt**: Dangerous Physical challenges when Health too low (minimum thresholds vary)
- **Recovered through**: Rest blocks (slow natural healing), medical treatment (faster recovery), food with restorative properties
- **Critical threshold**: Below 30 Health increases risk of further injury
- **Integration**: Must balance Physical challenge attempts against injury risk
- **Verisimilitude**: Physical challenges risk bodily harm, injuries require recovery time

**Stamina** (Physical-specific permanent resource):
- Maximum: 100
- **Cost**: Physical challenges cost 10-30 Stamina to attempt (depending on difficulty)
- **Lost to**: Physical exertion (challenges, travel, labor)
- **Depletion effect**: Below 30 Stamina → Max Exertion reduced (start challenges with lower Exertion capacity)
- **Cannot attempt**: Stamina-requiring activities when insufficient
- **Recovered through**: Rest blocks (full recovery), food (moderate recovery), reduced activity
- **Integration**: Must balance Physical challenges, travel, and work against stamina depletion
- **Verisimilitude**: Physical exertion drains energy, rest and food restore it

**Why Different Challenge-Specific Costs**:
- **Mental** costs Focus (concentration) - respects that mental work is exhausting
- **Physical** costs Health (injury risk) + Stamina (exertion) - respects that physical challenges are dangerous AND tiring
- **Social** costs nothing permanent (but takes time) - respects that conversation is "free" but time-consuming
- **Verisimilitude**: Different activities have different costs in reality

**Hunger** (Universal pressure):
- Increases: 20 per time block (affects all activities equally)
- Maximum: 100
- **Effects**: At 75+ hunger → movement slowed, work efficiency reduced, Stamina recovery impaired
- **Management**: Food costs coins, has weight, must be carried or purchased at locations
- **Integration**: Creates pressure to work for income vs. pursue investigations/relationships
- **Verisimilitude**: Everyone needs to eat, regardless of activity type

**Coins** (Universal currency):
- **Earned through**: Completed deliveries/requests, work, trade, Mental/Physical/Social challenge rewards
- **Spent on**: Food, equipment, route fees (bribes, transport), supplies, information
- **Balance**: Must work for income vs. spend time on investigations/relationships
- **Integration**: All three challenge types can reward coins, all gameplay requires coins eventually
- **Verisimilitude**: Money enables preparation and survival

**Understanding** (Universal progression):
- **Earned**: Through Mental/Physical/Social challenge cards (specific cards grant Understanding)
- **Effect**: Unlocks higher card tiers across ALL three systems
- **Integration**: Any challenge type can build Understanding, Understanding benefits all systems
- **Verisimilitude**: Experience in any domain broadens overall capability

## Verisimilitude: Why Different Is Right

The three challenge systems achieve equivalent tactical depth (~1,000-1,100 lines of implementation each) while respecting fundamentally different interaction models. Asymmetries are features, not bugs.

### Entity vs Place vs Challenge Interaction Models

**Social Interacts with ENTITIES (NPCs)**:
- NPCs have **agency**: They offer tasks, make demands, judge your actions
- NPCs have **personalities**: Proud, Devoted, Mercantile, Cunning, Steadfast fundamentally alter tactics
- NPCs have **memory**: Connection tokens, levels, observation cards remember relationship history
- Conversations are **real-time**: Dynamic entities have patience limits (Doubt=10 ends interaction)
- **Session-bounded model**: Must complete conversation in one sitting (can't pause mid-conversation)
- **Special mechanics justified**: Request/Promise/Burden exist because NPCs can offer obligations, make commitments, and inflict relational damage

**Verisimilitude Requirement**: Conversations happen continuously with entities who respond dynamically. You can't pause mid-conversation and return hours later - the NPC would leave.

---

**Mental Interacts with PLACES (Locations)**:
- Locations are **static**: No agency, no personality, no dynamic responses
- Locations have **investigative properties**: Delicate, Obscured, Layered, Time-Sensitive, Resistant profiles
- Locations **accumulate state**: Progress and Exposure persist at location between visits
- Investigations are **pauseable puzzles**: You can leave and return with state preserved
- **Pauseable model**: Leave anytime, state persists exactly where you left off
- **No obligation mechanics**: Places can't offer tasks (they lack agency), can't demand things, can't damage relationships

**Verisimilitude Requirement**: Real investigations take days/weeks with breaks between sessions. Evidence doesn't disappear when you leave. You return with fresh mental energy but the scene is as you left it.

---

**Physical Interacts with CHALLENGES (Obstacles/Terrain)**:
- Challenges are **immediate tests**: No agency, no personality, no memory
- Challenges have **types**: Combat, Athletics, Finesse, Endurance, Strength determine tactical approach
- Challenge state **doesn't persist**: Each attempt starts fresh (but YOUR Health/Stamina carry forward)
- Challenges are **one-shot attempts**: Must complete or fail in single session
- **One-shot model**: Can't pause halfway and return later
- **No obligation mechanics**: Obstacles can't offer tasks, don't make demands, don't remember attempts

**Verisimilitude Requirement**: You can't pause halfway up a cliff face and return tomorrow to continue. Physical challenges are immediate tests requiring completion or failure in the moment.

### Why Session Models Differ

**Mental Pauseable** (respects investigation reality):
- Investigations take multiple sessions with breaks
- Evidence doesn't vanish when investigator leaves
- Returning with fresh perspective is normal investigative practice
- Exposure accumulates (scene gets more disturbed with each visit)

**Physical One-Shot** (respects physical reality):
- Can't pause mid-climb and return tomorrow
- Can't pause mid-combat for a rest break
- Physical challenges test current capability in the moment
- Your body state (Health/Stamina) persists, but challenge state doesn't

**Social Session-Bounded** (respects conversation reality):
- Conversations happen in real-time with dynamic entities
- NPCs have patience limits (Doubt accumulates, 10 ends conversation)
- Can't pause mid-conversation and return hours later
- Relationship state persists between conversations, not mid-conversation

### Why Permanent Costs Differ

**Mental Costs Focus** (concentration depletes):
- Mental work is genuinely exhausting
- Concentration is finite and must recover
- Respects that thinking hard drains mental energy

**Physical Costs Health + Stamina** (injury risk + exertion):
- Physical challenges genuinely risk bodily harm
- Physical activity genuinely drains energy
- Respects that action is both dangerous and tiring

**Social Costs Nothing Permanent** (but takes time):
- Talking doesn't deplete resources
- Conversation is "free" but time-consuming
- Respects that social interaction is low-cost but requires time investment

### Why Special Mechanics Differ

**Social Has Request/Promise/Burden** (NPC agency):
- NPCs can offer tasks (Request cards)
- NPCs can make commitments (Promise cards)
- NPCs can damage relationships (Burden cards)
- **Justified**: Entities with agency can do these things

**Mental/Physical Have No Equivalent** (no agency):
- Locations can't offer tasks (places lack agency)
- Obstacles can't make promises (challenges lack social contract capability)
- Terrain can't damage relationships (non-entities can't have relationships)
- **Justified**: Static elements don't have agency to offer obligations or inflict social consequences

### What IS Parallel (Creating Equivalent Depth)

Despite intentional differences, all three systems share:

1. **Unified 5-stat system**: All cards bind to Insight/Rapport/Authority/Diplomacy/Cunning
2. **Builder → Victory**: Progress/Breakthrough/Momentum resources
3. **Threshold → Failure**: Exposure/Danger/Doubt resources
4. **Session → Spending**: Attention/Exertion/Initiative resources
5. **Balance → Playstyle**: ObserveActBalance/Commitment/Cadence trackers
6. **Binary Actions**: OBSERVE-ACT / ASSESS-EXECUTE / SPEAK-LISTEN pairs
7. **5 Tactical Modifiers**: Profiles/Challenge Types/Personalities fundamentally alter tactics
8. **Expertise Tokens**: Familiarity/Mastery/Connection provide mechanical benefits
9. **Progression Levels**: Depth/Proficiency/Connection track cumulative mastery
10. **Exhaustion Mechanics**: One-time use cards for tactical timing
11. **Goal Cards**: Victory condition cards unlock at thresholds
12. **Understanding**: Shared tier-unlocking resource

**Result**: Three systems with equivalent tactical depth (~1,000-1,100 lines each) achieved through parallel architecture that respects verisimilitude. Parity is in depth and complexity, not mechanical sameness.

### Weight Capacity

**Satchel Maximum: 10**

Must balance:
- Obligations accepted (letters 1, packages 1-3, goods 3-6)
- Equipment carried (tools 1-3, supplies 1-2 each)
- Food reserves (1 per meal)
- Discovered items (variable 1-4)

Trade-offs:
- Accept profitable heavy obligation, can't carry much equipment
- Carry full investigation gear, limited obligation capacity
- Stock food for travel, less room for discoveries
- Travel light for exploration, vulnerable without supplies

## Conversation System

### Five Core Resources

**Initiative** (Action Economy):
- Starts at 0 each conversation
- Must be generated through Foundation cards
- Spent to play higher-cost cards
- Persists between LISTEN actions
- Creates builder/spender dynamic

**Momentum** (Progress Track):
- Starts at 0
- Built toward goal thresholds (8, 12, 16)
- Can be consumed by card effects
- Primary victory condition
- Represents conversation progress

**Doubt** (Timer):
- Starts at 0
- Increases through effects and Cadence
- Conversation ends at 10
- Creates urgency
- Forces efficiency and listening

**Cadence** (Balance):
- Starts at 0, range -5 to +5
- SPEAK action: +1
- LISTEN action: -2
- High Cadence: +1 Doubt per point on LISTEN
- Low Cadence: +1 card draw per point on LISTEN
- Rewards strategic listening

**Statements in Spoken** (History):
- Count of Statement cards played
- Scales card effects
- Enables requirements
- Determines time cost (1 segment + statements)
- Conversation has memory

### Card Structure

Every card has:
- **Initiative Cost**: 0 for Foundation (depth 1-2), scales with depth
- **Either** requirement OR cost, never both
- **One deterministic effect**: No branching or randomness
- **Stat binding**: Which stat gains XP when played
- **Persistence**: Statement (stays in Spoken) or Echo (returns to deck)

### Stat-Gated Depth Access

Stats determine card depth access:
- Stat Level 1: Access depths 1-2 (Foundation only)
- Stat Level 2: Access depths 1-3
- Stat Level 3: Access depths 1-4 (Standard cards)
- Stat Level 5: Access depths 1-6 (Advanced)
- Stat Level 7: Access depths 1-8 (Powerful)
- Stat Level 9+: Access depths 1-10 (Master)

Progression represents growing conversational competence and expanded repertoire.

### Social Challenge Types (Conversations)

Conversations are **Social Challenges** - the third and most complex parallel tactical system alongside Mental and Physical challenges. Social follows the same architectural pattern but adds significant mechanical depth.

**Social Challenge Resources (Most Complex):**
- **Momentum** (builder): Progress toward conversation goal (typically 8-16 threshold)
- **Initiative** (session resource): Action economy currency, accumulated through Foundation cards, persists through LISTEN
- **Doubt** (threshold): Failure condition at maximum (10), tracks NPC skepticism
- **Cadence** (balance): Dominating vs deferential style (-10 to +10), creates Doubt penalties or card draw bonuses
- **Statements** (history): Count of Statement cards played, determines time cost (1 segment + statements)
- **Understanding** (tier unlock): Persistent connection depth (shared with Mental/Physical)

**Additional Social Complexity:**
- **Personality Rules**: Each NPC has unique modifier (Proud, Devoted, Mercantile, Cunning, Steadfast) that fundamentally alters card play rules
- **Token Mechanics**: Relationship tokens (Rapport, Trust, Commitment) unlock special conversation branches and affect momentum thresholds
- **Connection Progression**: Conversations deepen NPC relationships through multi-stage connection system (Stranger → Acquaintance → Friend → Close Friend)
- **Observation Cards**: Knowledge from investigations injects special cards into conversation decks
- **Request Cards**: NPC-specific cards that drive conversation toward specific outcomes

**SocialChallengeType Configuration:**
Each SocialChallengeType defines:
- **Deck ID**: Which card deck to use (investigation conversations, casual chats, negotiations, etc.)
- **Victory Threshold**: Momentum needed to complete conversation successfully (8-16 typical)
- **Danger Threshold**: Doubt limit before conversation failure (10 standard)
- **Initial Hand Size**: Starting cards (typically 5)
- **Max Hand Size**: Maximum hand capacity (typically 7)

**Context-Specific Social Challenges:**
- **Investigation Conversations**: Social phases of investigations (question witnesses, confront suspects)
- **NPC Requests**: Conversations triggered by accepting obligations (deliveries, tasks)
- **Relationship Building**: Casual conversations deepening NPC connections
- **Information Gathering**: Conversations focused on discovering knowledge
- **Negotiation**: Trade and deal-making conversations

All of the three tactical systems should privde the SAME depth. All three follow the same architectural pattern (builder/threshold/session/balance resources + binary actions).

## Three-Layer Content Architecture

### Mechanical Layer (Authoritative)

All deterministic systems:
- Card costs, effects, requirements
- Resource calculations and formulas
- Investigation obstacle requirements
- Travel danger thresholds
- Equipment functionality
- Time segment costs
- Weight constraints

**Authority:** Mechanics determine what's possible. AI cannot override.

### Authored Layer (Structural, Sparse)

Memorable moments identical for all players:
- NPC introduction scenes (first meeting)
- Location first-visit vignettes (atmospheric establishment)
- Relationship milestone scenes (at relationship thresholds)
- High-difficulty investigation discoveries (lore payoffs)

**Purpose:** Create identity, establish tone, provide structural peaks in slice-of-life flow.

**Constraint:** No branching. Same content for everyone. Happens based on triggers (first visit, relationship level, calendar date).

### AI Flavor Layer (Contextual, Pervasive)

Dynamic narration adapting to exact game state:
- All dialogue reflecting relationship history
- Card text showing conversation context
- Investigation descriptions building on knowledge
- Travel narration noting preparation and conditions
- Work scenes showing world continuity
- NPC reactions to player reputation and past actions
- Location descriptions deepening with familiarity

**Constraint:** AI flavors mechanics, never invents consequences. Provides context and atmosphere for deterministic outcomes.

## Integration Principles

### Mechanics Express Narrative

System states have narrative meaning:
- High Cadence = dominating conversation
- Burden cards = damaged relationship
- Investigation progress = location expertise
- Equipment owned = preparedness
- NPC knowledge = trusted advisor

Players read story through mechanical state.

### Systems Create Impossible Choices

Every decision has meaningful trade-offs:
- Accept lucrative heavy obligation OR maintain equipment capacity
- Investigate thoroughly OR meet deadline
- Spend coins on equipment OR save for emergency
- Help desperate NPC OR preserve other relationships
- Optimize efficiency OR explore thoroughly
- Work for income OR develop relationships/investigations

No obvious correct choice. Character revealed through priorities.

### Resources Serve Challenges

Not abstract optimization:
- Coins buy equipment for specific investigation
- Stamina enables difficult travel route
- Knowledge reveals safer path
- Relationships provide critical information
- Equipment allows accessing investigation layers
- Time permits thorough preparation

Every resource has concrete purpose toward adventure goals.

### Preparation Matters

Success comes from planning:
- Gather information before attempting challenge
- Acquire appropriate equipment
- Build sufficient capability (health/stamina)
- Choose optimal timing
- Understand risks and backup plans

Grinding serves preparation, not arbitrary gates.

### Failure Teaches

Unsuccessful attempts provide learning:
- Discover what equipment needed
- Learn hazard locations and types
- Understand timing requirements
- Identify knowledge gaps
- Reveal hidden paths or approaches

State persistence means failed attempts aren't wasted - you return better prepared.

## Slice-of-Life Structure

### Frieren Model

**Core Elements:**
- No endgame or climax
- Relationships deepen through accumulated moments
- World exists independent of player
- Discovery is intrinsic reward
- Small personal scale
- Time creates meaning through persistence

**Mechanical Expression:**
- Open-ended progression (no level cap or story end)
- Gradual relationship building (not binary quest completion)
- NPCs have ongoing lives (requests reflect their activities)
- Investigations reward curiosity (not just mechanical benefit)
- Daily routine creates comfort (familiar rhythms)
- Authored milestones mark passage (but not conclusion)

### Daily Rhythm

**Morning:**
- Fresh start, full resources
- Quiet investigations (better conditions)
- Regular NPC locations
- Planning for day ahead

**Midday/Afternoon:**
- Active challenges (travel, difficult investigations)
- Business and trade
- Deliveries and obligations
- Resource expenditure

**Evening:**
- Social time (different conversation opportunities)
- Rest and recovery
- Reflection on progress
- Preparation for tomorrow

**Night:**
- Special investigations (when locations empty)
- Emergency travel if needed
- Sleep and full recovery
- Day boundary

This creates natural pacing without artificial urgency.

### Relationship Depth

Bonds form through time spent together:
- Multiple conversations over days/weeks
- Helping with varied requests
- Sharing discovered knowledge
- Overcoming challenges cooperatively
- Witnessing NPC ongoing lives

Authored milestones mark progress but bulk of relationship is emergent interaction.

### World Independence

NPCs have lives beyond player:
- Requests reflect their actual needs
- Conversations reference their activities
- World events proceed regardless
- Player is participant, not center

This maintains immersion and verisimilitude.

## Content Scalability

### Package-Based Loading

Content structured as modular packages:
- Core game foundation
- Expansion locations and NPCs
- AI-generated contextual content

### Skeleton System

Missing references create functional placeholders:
- Mechanically complete
- Narratively generic
- Replaced when real content loads
- Enables AI procedural generation

### AI Content Generation

AI creates packages filling gaps:
- New NPCs with proper deck structure
- Additional locations with investigations
- Route connections and obstacles
- Observation rewards and discoveries
- Always follows mechanical templates

## Critical Design Constraints

### Verisimilitude Requirements

**World Building:**
- Low fantasy, historical setting
- Small personal scale (village/town area)
- Walking distances (hours not days)
- Actions span seconds/minutes
- No modern concepts
- Grounded dangers (nature, exposure, injury, social)

**Not Included:**
- Epic scope or destiny
- Modern equipment (crowbars, etc.)
- Large-scale travel (mountain passes)
- Abstract checkpoints/guards
- Gamey mechanics without world logic

### Mechanical Clarity

**Always Visible:**
- All calculations shown to player
- Resource costs before commitment
- Success/failure conditions clear
- No hidden randomness
- Perfect information gameplay

**Always Deterministic:**
- Card effects work exactly as stated
- Requirements either met or not
- No probability beyond stated percentages
- Player skill determines outcomes

### No Soft-Lock

**Always Possible:**
- Earn coins through work
- Progress relationships with any NPC
- Access locations through alternatives
- Recover from resource depletion
- Continue after failures

**Consequences Not Blocks:**
- Mistakes create friction not walls
- Failures add difficulty not impossibility
- Bad choices have costs not game-overs
- Learning enables better future attempts

## Verification Questions

For any new mechanic, ask:

1. **Does it serve exactly one purpose?** (No multi-function systems)
2. **Does it make narrative sense?** (Verisimilitude check)
3. **Is it fully visible to player?** (Perfect information)
4. **Is it deterministic?** (No hidden randomness)
5. **Does it create impossible choices?** (Meaningful trade-offs)
6. **Can it soft-lock the player?** (If yes, redesign)
7. **Does it pull toward adventure or away?** (Integration check)
8. **Is it grounded in world logic?** (No gamey abstractions)
9. **Does AI need mechanical authority?** (If yes, redesign)
10. **Would it work in a D&D session?** (Scale and pacing check)

## Conclusion

Wayfarer achieves integration of visual novel and simulation through:

**Mechanical Foundation:** Elegant systems creating strategic depth through impossible choices and resource constraints.

**Authored Structure:** Sparse memorable moments establishing identity and marking progress without branching complexity.

**AI Contextualization:** Pervasive responsive narration making mechanical choices feel personal and meaningful.

The result: Slice-of-life adventure where character emerges through decisions under constraint, relationships deepen through time and shared challenges, and world reveals itself to curious preparation and persistent effort.

Player versus nature, small personal scale, grounded in verisimilitude, expressed through text and choice.
