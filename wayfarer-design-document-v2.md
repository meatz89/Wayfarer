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
- **Mental**: "Detailed Examination" (depth 4, ACT) - Systematic observation (+3 Progress, +2 Leads, +1 Understanding, costs 3 Attention)
- **Physical**: "Structural Analysis" (depth 4, EXECUTE) - Identify load-bearing points (+2 Breakthrough, if first in combo -2 Danger, costs 2 Exertion)
- **Social**: "Read Motivation" (depth 4, SPEAK) - Understand NPC's true desires (+2 Momentum, reduce Doubt, costs 3 Initiative)

**Rapport-Bound Cards**:
- **Mental**: "Empathetic Reading" (depth 4, ACT) - Sense emotional context (+2 Progress, +2 Leads, +1 to next Social, costs 3 Attention)
- **Physical**: "Flow State" (depth 6, EXECUTE) - Natural fluid movement (+3 Breakthrough, costs 1 less Exertion if 2+ in combo, costs 3 Exertion base)
- **Social**: "Emotional Resonance" (depth 5, SPEAK) - Connect on feeling level (+3 Momentum, +1 Rapport token, costs 4 Initiative)

**Authority-Bound Cards**:
- **Mental**: "Command Scene" (depth 5, ACT) - Take investigative control (+3 Progress, +3 Leads, +1 Exposure, costs 3 Attention)
- **Physical**: "Power Move" (depth 6, EXECUTE) - Decisive forceful action (+4 Breakthrough, +1 Danger, costs 4 Exertion)
- **Social**: "Assert Position" (depth 5, SPEAK) - Direct conversation firmly (+3 Momentum, +1 Cadence, costs 4 Initiative)

**Diplomacy-Bound Cards**:
- **Mental**: "Patient Observation" (depth 4, ACT) - Methodical investigation (+2 Progress, +2 Leads, -1 Exposure, costs 2 Attention)
- **Physical**: "Measured Technique" (depth 4, EXECUTE) - Controlled application of force (+2 Breakthrough, costs 2 Exertion)
- **Social**: "Find Common Ground" (depth 4, SPEAK) - Seek compromise (+2 Momentum, -1 Cadence, costs 3 Initiative)

**Cunning-Bound Cards**:
- **Mental**: "Cover Tracks" (depth 4, ACT) - Investigate without traces (+2 Progress, +2 Leads, -1 Exposure, costs 3 Attention)
- **Physical**: "Calculated Risk" (depth 5, EXECUTE) - Tactical risk assessment (+3 Breakthrough, -1 Danger per card in combo, costs 3 Exertion)
- **Social**: "Strategic Deflection" (depth 5, SPEAK) - Subtle redirection (+2 Momentum, manipulate Cadence, costs 4 Initiative)

### Why Unified Stats Matter

**Single Progression Path**: Playing any challenge type improves your capabilities across all systems. Mental investigations improve your Social Insight cards. Physical challenges improve your Mental Authority cards. Social conversations improve your Physical Cunning cards.

**Thematic Coherence**: Stats represent fundamental character traits that manifest differently in different contexts. A highly Insightful character excels at observation (Mental), structural analysis (Physical), and reading people (Social).

**Build Variety**: Players can specialize (focus on 2-3 stats) or generalize (develop all stats evenly), creating distinct playstyles that affect all three challenge types simultaneously.

**No Wasted Effort**: Every card played in every system contributes to unified character progression.

## Strategic-Tactical Architecture

Wayfarer operates on two distinct layers that separate decision-making from execution:

### Strategic Layer
The strategic layer handles **what** and **where** decisions:
- **Venue Selection**: Choose which venue to visit from travel map
- **Goal Selection**: Choose which investigation phase or NPC request to pursue
- **Resource Planning**: Evaluate equipment, knowledge, and stat requirements
- **Risk Assessment**: Consider danger levels, time limits, exposure thresholds

Strategic decisions happen in safe spaces (taverns, towns, travel map) with perfect information. Players can see all requirements, evaluate all options, and make informed choices about which tactical challenges to attempt.

### Tactical Layer
The tactical layer handles **how** execution through three parallel challenge systems:

**Mental Challenges** - Location-based investigations using observation and deduction
**Physical Challenges** - Location-based obstacles using strength and precision
**Social Challenges** - NPC-based conversations using rapport and persuasion

Each system is a distinct tactical game with unique resources and action pairs, following a common structural pattern while respecting what you're actually doing:
- **Builder Resource**: Primary progress toward victory (Progress, Breakthrough, Momentum)
- **Threshold Resource**: Accumulating danger toward failure (Exposure, Danger, Doubt)
- **Session Resource**: Tactical capacity for playing cards (Attention from Focus, Exertion from Stamina, Initiative from Foundation cards)
- **Flow Mechanic**: System-specific card flow reflecting verisimilitude through distinct pile naming:
  - **Mental**: Details (input pile) → Methods (hand) → Applied (discard)
  - **Physical**: Situation (input pile) → Options (hand) → exhausts back to Situation
  - **Social**: Topics (input pile) → Mind (hand) → Spoken (discard)
- **Action Pair**: Two actions creating tactical rhythm (OBSERVE/ACT, ASSESS/EXECUTE, LISTEN/SPEAK)
- **Understanding**: Shared tier-unlocking resource across all three systems

The systems differentiate through distinct card mechanics that respect verisimilitude. Mental challenges OBSERVE Details to gather investigative threads, ACT on Methods to apply understanding, moving completed methods to Applied. Physical challenges ASSESS the Situation to evaluate context, EXECUTE Options from prepared choices, with all Options exhausting back to Situation for fresh assessment. Social challenges LISTEN to Topics from NPC words, SPEAK thoughts from Mind, moving Statement thoughts to Spoken while Echo thoughts return to Topics.

### Why Card Flow Matters

**Mental (Investigation)**: You cannot observe what you haven't investigated. ACT on Methods generates Leads (investigative threads) based on card depth (depth 1-2 = +1 Lead, depth 3-4 = +2 Leads, depth 5-6 = +3 Leads). **ACT does NOT draw cards** - it only generates Leads. OBSERVE Details draws exactly Leads count - zero Leads means zero draw. **OBSERVE is the ONLY action that draws cards.** Methods in hand persist because investigation knowledge doesn't vanish. Leads persist between visits because uncovered threads remain open. Completed methods move to Applied pile, representing applied investigative understanding.

**Physical (Challenges)**: You prepare actions then execute them. EXECUTE locks Options as preparation while increasing Aggression (+1 base + card approach modifier) - setting up your stance, building momentum. **EXECUTE uses projection to determine affordability** (Exertion cost includes modifiers). ASSESS evaluates the Situation, triggers all locked Options as a combo while decreasing Aggression (-2 base + approach modifiers), **applying full projection per card** (Breakthrough, Danger, Aggression), then exhausts all Options back to Situation and draws fresh Options for the changed context. **Locked cards displayed in UI above hand with "LOCKED" badge.** You want to stay balanced - both overcautious and reckless approaches create penalties.

**Social (Conversations)**: Thoughts persist because that's how thinking works. SPEAK moves Statement thoughts from Mind to Spoken pile (said aloud) while Echo thoughts return to Topics (fleeting thoughts that recirculate). LISTEN to Topics draws new cards while thoughts in Mind persist - your mind doesn't empty when you pause to listen, it accumulates understanding.

### Bridge: Goals and GoalCards

**Two-Layer Goal System** bridges strategic planning to tactical execution:

**Goals (Strategic Layer - Defines UI Actions)**:
- First-class entities in JSON (separate from NPCs/locations)
- Creates ONE action button in venue UI per goal
- Has: `id`, `name`, `description`, `systemType` (Social/Mental/Physical), `deckId` (direct deck reference), assignment via `npcId` OR `locationId`
- Contains: Inline array of GoalCards (`goalCards` property) defining victory conditions
- Assignment: Goals reference NPCs/locations (not inline within them)
- Storage: `NPC.ActiveGoals` and `Location.ActiveGoals` lists
- Purpose: Defines WHAT tactical engagement is available WHERE
- **Direct Deck Reference**: Goals reference ChallengeDeck directly via `deckId` (no intermediary ChallengeType entity)

**GoalCards (Tactical Layer - Victory Conditions)**:
- Defined inline within goal JSON (not separate entities)
- Has: `id`, `name`, `description`, `momentumThreshold`, `rewards`
- Universal: Same structure across all three challenge types (Social/Mental/Physical)
- Purpose: Tiered victory conditions - when momentum threshold reached, goal card becomes playable
- Playing goal card: Completes challenge with specified rewards
- Contextual: Cannot be reused across goals - bound to parent goal
- **Self-Contained**: GoalCards are complete cards with no external references - created directly as `CardInstance(goalCard)`

**Architectural Flow**:
```
JSON → GoalDTO → GoalParser → Goal (with inline GoalCards) → NPC.ActiveGoals / Location.ActiveGoals
```

**Key Distinctions**:
- Goals define WHERE actions appear (strategic placement)
- GoalCards define WHEN victory occurs (tactical thresholds)
- Each goal creates ONE button in UI (not multiple buttons per goal card)
- Goals assigned to entities via reference (not inline definitions)
- `GameWorld.Goals` dictionary stores all goals for investigation spawning

This architecture ensures:
- **Clear Separation**: Strategic planning never mixes with tactical execution
- **Perfect Information**: All requirements visible before committing to tactical challenge
- **Architectural Consistency**: Three systems follow same structural pattern
- **Mechanical Diversity**: Each system offers distinct tactical gameplay
- **Dynamic Assignment**: Investigations can spawn goals at NPCs/locations when phases activate

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
- Destination becomes adventure venue
- Weather/time creates natural urgency
- Completion provides payment, relationships, access

**Purpose:** Drive player toward interesting content (dangerous routes, mysterious venues, challenging investigations).

### Investigation & Travel (Three Challenge Systems)

Investigations are multi-phase mysteries resolved through Mental, Physical, and Social tactical challenges. Each phase spawns a LocationGoal at a specific location, bridging strategic planning to tactical execution.

**Three Challenge Types:**
- **Mental Challenges**: Observation-based investigation at locations (examine scenes, deduce patterns, search thoroughly)
  - Resources: Progress (builder), Attention (session budget from Focus), Exposure (threshold), Leads (observation flow)
  - Actions: ACT (generate Leads through action) / OBSERVE (follow Leads through observation)
  - Card Flow: ACT generates Leads by depth (1-2 = +1, 3-4 = +2, 5-6 = +3), OBSERVE draws exactly Leads count

- **Physical Challenges**: Strength-based obstacles at locations (climb carefully, leverage tools, precise movement)
  - Resources: Breakthrough (builder), Exertion (session budget from Stamina), Danger (threshold), Aggression (balance)
  - Actions: EXECUTE (lock cards, increase Aggression) / ASSESS (trigger combo, decrease Aggression, exhaust hand, draw fresh)
  - Card Flow: EXECUTE locks cards as prepared sequence while building aggressive momentum, ASSESS executes combo and returns to balanced state

- **Social Challenges**: Rapport-based conversations with NPCs (build trust, gather information, persuade cooperation)
  - Resources: Momentum (builder), Initiative (session builder), Doubt (threshold), Cadence (balance), Statements (history)
  - Actions: SPEAK (advance conversation) / LISTEN (reset and draw)
  - Card Flow: SPEAK moves Statements to Spoken pile, Echoes reshuffle, LISTEN draws while hand persists

Each system is a distinct tactical game following the same architectural pattern while respecting what you're actually doing through system-specific card flow mechanics.

**Investigation Lifecycle:**
1. **Discovery**: Triggered by observation, conversation, item discovery, or obligation
2. **Phase Spawning**: Each phase creates LocationGoal at specified location with requirements
3. **Strategic Planning**: Evaluate requirements (knowledge, equipment, stats, completed phases)
4. **Tactical Execution**: Visit location, select goal card, complete Mental/Physical/Social challenge
5. **Progression**: Victory grants discoveries, unlocks subsequent phases, builds toward completion

**AI-Generated Content**: Investigation templates define structure (phases, requirements, rewards). AI generates specific content (venues, NPCs, narratives, card text) from templates, creating unique investigations that follow proven mechanical patterns.

**Purpose:** Bridge strategic decision-making to tactical challenge execution. Investigations provide context and progression, challenges provide gameplay.

## Goal and GoalCard System Architecture

### Two-Layer Design Philosophy

Wayfarer separates **strategic placement** (Goals) from **tactical victory conditions** (GoalCards) to enable dynamic content generation while maintaining clear mechanical boundaries.

### Goals (Strategic Layer)

**Definition**: First-class entities that create action opportunities at specific locations or NPCs.

**Key Properties**:
- **System Type**: Which tactical system (Social/Mental/Physical)
- **Deck ID**: Which challenge deck to use (direct reference, no intermediary)
- **Assignment**: `npcId` OR `locationId` (mutually exclusive)
- **Goal Cards**: Inline array of victory condition cards (self-contained, no external references)
- **Requirements**: Knowledge, equipment, stats, completed phases (optional)

**Assignment Models**:
- **NPC Goals**: Social challenges assigned via `npcId`, stored in `NPC.ActiveGoals`
- **Location Goals**: Mental/Physical challenges assigned via `locationId`, stored in `Location.ActiveGoals`
- **Reference Pattern**: Goals reference entities, NOT inline definitions within entities

**Storage and Access**:
- `GameWorld.Goals` dictionary: All goals indexed by ID for investigation spawning
- `NPC.ActiveGoals` list: Social goals currently available at this NPC
- `Location.ActiveGoals` list: Mental/Physical goals currently available at this location

**UI Presentation**:
- Each goal creates ONE action button in venue UI
- Button displays goal name and description
- Clicking button launches appropriate tactical challenge (Social/Mental/Physical)
- Goal cards NOT directly visible until inside challenge

### GoalCards (Tactical Layer)

**Definition**: Tiered victory conditions defined inline within parent goal.

**Properties**:
- **Momentum Threshold**: When this card becomes playable (8, 12, 16 typical)
- **Rewards**: What player receives on completion (coins, knowledge, obligations, items)
- **Contextual**: Bound to parent goal, cannot be reused across goals
- **Universal Structure**: Same structure across all three challenge types

**Tactical Flow**:
1. Player enters challenge (Mental/Physical/Social)
2. Player builds builder resource through card play:
   - **Social**: Momentum vs GoalCard.momentumThreshold
   - **Mental**: Progress vs GoalCard.progressThreshold
   - **Physical**: Breakthrough vs GoalCard.breakthroughThreshold
3. When builder resource reaches threshold, goal card becomes playable
4. **Player choice**: Player CAN play goal card or continue building to unlock better goal cards
   - No forced completion - player chooses when to end challenge
   - Higher thresholds unlock better rewards
   - Strategic decision: Accept current reward or risk continuing
5. Playing chosen goal card completes challenge with its specified rewards
6. No partial success - must reach threshold to make card playable
7. No weighted selection - player explicitly chooses which goal card to play

**Why Inline Definition**:
- Goal cards are contextual to their parent goal (not reusable entities)
- Tiered rewards specific to this exact challenge
- Simplifies content authoring (rewards defined where goal defined)
- Prevents card reuse across incompatible contexts

### Three Parallel Systems Integration

**Social Goals** (NPC-based):
```json
{
  "id": "elena_delivery",
  "systemType": "Social",
  "npcId": "elena",
  "deckId": "desperate_request",
  "goalCards": [...]
}
```
- Assigned to NPCs via `npcId`
- Stored in `NPC.ActiveGoals`
- Launches Social challenge (conversation) using specified deck
- Goal cards unlock at momentum thresholds during conversation

**Mental Goals** (Location-based investigation):
```json
{
  "id": "notice_waterwheel",
  "systemType": "Mental",
  "locationId": "courtyard",
  "deckId": "mental_challenge",
  "goalCards": [...]
}
```
- Assigned to locations via `locationId`
- Stored in `Location.ActiveGoals`
- Launches Mental challenge (investigation) using specified deck
- Goal cards unlock at progress thresholds during investigation

**Physical Goals** (Location-based obstacle):
```json
{
  "id": "climb_waterwheel",
  "systemType": "Physical",
  "locationId": "courtyard",
  "deckId": "physical_challenge",
  "goalCards": [...]
}
```
- Assigned to locations via `locationId`
- Stored in `Location.ActiveGoals`
- Launches Physical challenge (obstacle) using specified deck
- Goal cards unlock at breakthrough thresholds during challenge

### Investigation System Integration

**Dynamic Goal Spawning**:
Investigations spawn goals dynamically when phases activate:

```
Investigation Phase Requirements Met
    ↓
Investigation System Adds Goal to Entity
    ↓
Goal Appears in NPC.ActiveGoals OR Location.ActiveGoals
    ↓
UI Displays Goal as Action Button
    ↓
Player Clicks Button → Tactical Challenge Launches
    ↓
Player Completes Challenge via Goal Card
    ↓
Investigation Phase Marked Complete → Next Phase Unlocks
```

**Multi-Phase Example**:
```
Waterwheel Mystery Investigation
├─ Phase 1: Mental goal added to Location.ActiveGoals at "courtyard"
├─ Phase 2: Social goal added to NPC.ActiveGoals for "mill_owner"
├─ Phase 3: Physical goal added to Location.ActiveGoals at "mill_interior"
└─ Phase 4: Social goal added to NPC.ActiveGoals for "suspect_npc"
```

Each phase completion spawns next phase's goals dynamically across the game world.

### Content Authoring Pattern

**Goals Defined Separately** (04_goals.json):
```json
{
  "goals": [
    { "id": "goal1", "npcId": "elena", ... },
    { "id": "goal2", "locationId": "courtyard", ... }
  ]
}
```

**NPCs/Locations Reference Goals** (via assignment):
- Parser reads goal definition
- Parser checks `npcId` or `locationId`
- Parser adds goal to appropriate `ActiveGoals` list
- Entity now has available action

**NOT This Pattern** (inline goals in entities):
```json
{
  "npcs": [
    {
      "id": "elena",
      "goals": [ ... ]  // ❌ WRONG - goals not inline
    }
  ]
}
```

### Architectural Benefits

**Clear Separation**:
- Strategic layer (Goals) handles WHERE actions appear
- Tactical layer (GoalCards) handles WHEN victory occurs
- No confusion between placement and victory conditions

**Dynamic Assignment**:
- Investigations can spawn goals anywhere in world
- Goals move between entities as investigations progress
- Single goal can be reassigned (though rare in practice)

**Universal Victory Conditions**:
- GoalCards work identically across all three systems
- Same momentum threshold concept (Progress/Breakthrough/Momentum)
- Same reward structure regardless of system type

**Content Scalability**:
- AI can generate goals following mechanical templates
- Goals reference existing NPCs/locations via IDs
- GoalCards inline definition keeps context localized

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

**1. Unknown State**
Investigation exists as template but not yet discovered by player. Not visible in UI. Waiting for discovery trigger.

**2. Discovery Triggers (Five Types)**

**Immediate Visibility** - Investigation visible upon entering venue
- Obvious environmental features (collapsed bridge, abandoned waterwheel)
- Public knowledge mysteries (town's water problem, missing merchant)
- Location-inherent investigation opportunities

**Environmental Observation** - Triggered by examining location features
- Player examines specific location, reveals hidden investigation
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

**3. Pending State**
Investigation discovered, intro goal spawned at appropriate location/NPC. Player sees investigation in journal but must complete intro goal to activate full investigation. Intro goal provides context and unlocks subsequent phases.

**4. Active State**
Intro goal complete, investigation progressing. All goals tracked individually with their states. Player can see phase structure, requirements, and completion status for each goal. Investigation persists across sessions - partial progress retained, can retreat and return prepared. Active investigation displays list of all goals with their individual states in journal.

**5. Completed State**
All required goals completed. Investigation marked complete, rewards granted, knowledge added to player state. May unlock subsequent investigations or alter world state.

### Goal Lifecycle

**All goals defined in JSON:**
- Parser creates all goals at game initialization from JSON
- Goals stored in GameWorld.Goals dictionary
- Initial assignment: Parser adds goals to NPC.ActiveGoals or Location.ActiveGoals based on npcId/locationId

**Investigation phase assignment:**
- Investigation phase definition references goal ID
- When phase requirements met, investigation system looks up goal in GameWorld.Goals
- Investigation system adds goal to appropriate NPC.ActiveGoals or Location.ActiveGoals
- Goal becomes available as action button in venue UI

**Goal completion behavior:**
- Some goals: DELETED from entity's ActiveGoals on successful completion (investigation progression)
- Some goals: NEVER deleted, remain in ActiveGoals (persistent/repeatable content)
- On failure: Goal always REMAINS in ActiveGoals, player can retry
- Delete-on-success property determines lifecycle behavior

**Goal Tracking:**
All goals tracked individually for investigation journal UI:
- Active investigations display all goals with their states (requirements met, completed, blocked)
- Player sees which goals completed, which remain, what requirements needed
- Individual tracking enables meaningful progress display

### Phase Structure

Each investigation consists of 3-7 phases resolved sequentially or in parallel (based on requirements):

**Phase Definition:**
- **System Type**: Mental, Physical, or Social challenge
- **Challenge Type**: Specific challenge configuration (victory threshold, danger threshold, deck)
- **Requirements**: Knowledge, equipment, stats, completed phases
- **Location Assignment**: Where to find this phase (venue + location for Mental/Physical, NPC for Social)
- **Progress Threshold**: Victory points needed to complete phase
- **Completion Reward**: Discoveries granted, phases unlocked, narrative reveals

**Phase Requirements:**
- **Completed Phases**: Must finish phases 1-3 before phase 4 unlocks
- **Knowledge**: Must have discovered specific information ("mill_mechanism_broken")
- **Equipment**: Must possess specific items ("rope", "crowbar")
- **Stats**: Minimum Observation, Strength, Rapport thresholds

**Phase Spawning (Dynamic Goal Assignment):**
When phase requirements are met, the investigation system dynamically assigns goals to NPCs or locations:
- **Mental/Physical Phases**: Goal added to `Location.ActiveGoals` list at specified location
- **Social Phases**: Goal added to `NPC.ActiveGoals` list for specified NPC
- Goals become available in venue UI as action buttons (one button per goal)
- Completing goal advances investigation to next phase
- Investigations can spawn multiple goals simultaneously across different locations/NPCs

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
- **Leads** (observation flow, persists): Investigative threads generated by ACT cards, determines OBSERVE draw count. Persists when leaving, resets only on investigation completion.
- **Understanding** (global, persistent): Tier unlocking across all three systems

### Action Pair

- **ACT**: Take investigative action, spend Attention, generate Leads based on card depth (depth 1-2 = +1 Lead, depth 3-4 = +2 Leads, depth 5-6 = +3 Leads), build Progress, increase Exposure risk, move completed methods to Applied pile. **Does NOT draw cards** - only generates Leads for future OBSERVE actions.
- **OBSERVE**: Follow investigative threads, draw Details equal to Leads count (zero Leads = zero draw), methods in Methods pile persist, does not spend Attention. **This is the ONLY action that draws cards in Mental challenges**.

### Card Flow Mechanics

**You cannot observe what you haven't investigated.** ACT on methods in Methods generates investigative Leads - threads to follow, evidence to examine, patterns to explore. Each ACT creates Leads based on its depth (depth 1-2 = +1 Lead, depth 3-4 = +2 Leads, depth 5-6 = +3 Leads), representing how much investigative material that action uncovers. **ACT does NOT draw cards immediately** - it only generates Leads as investigative threads.

OBSERVE then follows those Leads by drawing Details equal to your total Leads count. Without Leads, you have no Details to observe. **OBSERVE is the ONLY way to draw cards in Mental challenges** - you accumulate Leads through ACT, then follow them through OBSERVE.

Methods already in Methods persist when you OBSERVE because investigation knowledge doesn't vanish. Leads persist when you leave the location because uncovered threads remain available to pursue when you return. Only completing the investigation resets Leads to zero. Successfully applied methods move to the Applied pile, representing investigative understanding that has been put into practice.

### Stat Binding Examples

Mental cards bind to stats based on investigative approach. ACT cards generate Leads based on depth when played:

**Insight-Bound** (pattern recognition, deduction):
- "Detailed Examination" (depth 4, ACT): Systematic evidence observation (+3 Progress, +2 Leads, +1 Understanding, costs 3 Attention)
- "Pattern Analysis" (depth 6, ACT): Connect disparate clues (+4 Progress, +3 Leads, reveal hidden connections, costs 4 Attention)
- "Scientific Method" (depth 8, ACT): Rigorous hypothesis testing (+5 Progress, +4 Leads, +2 Understanding, costs 5 Attention)

**Cunning-Bound** (subtle investigation, covering tracks):
- "Cover Tracks" (depth 4, ACT): Investigate without leaving traces (+2 Progress, +2 Leads, -1 Exposure, costs 3 Attention)
- "Subtle Investigation" (depth 6, ACT): Gather evidence discreetly (+3 Progress, +3 Leads, -2 Exposure, costs 4 Attention)
- "Misdirection" (depth 8, ACT): Redirect attention while investigating (+4 Progress, +4 Leads, manipulate Exposure, costs 5 Attention)

**Authority-Bound** (commanding scene, decisive analysis):
- "Command Scene" (depth 5, ACT): Take investigative control (+3 Progress, +3 Leads, +1 Exposure from obviousness, costs 3 Attention)
- "Authoritative Analysis" (depth 7, ACT): Make definitive conclusions (+5 Progress, +4 Leads, +2 Exposure, costs 4 Attention)
- "Investigative Authority" (depth 9, ACT): Assert investigative dominance (+6 Progress, +5 Leads, force breakthroughs, costs 6 Attention)

**Diplomacy-Bound** (balanced approach, patience):
- "Patient Observation" (depth 4, ACT): Methodical investigation (+2 Progress, +2 Leads, -1 Exposure, costs 2 Attention)
- "Balanced Approach" (depth 6, ACT): Steady measured investigation (+3 Progress, +3 Leads, maintain Exposure, costs 3 Attention)
- "Thorough Method" (depth 8, ACT): Complete systematic coverage (+4 Progress, +4 Leads, +1 Understanding, costs 5 Attention)

**Rapport-Bound** (empathetic observation, human element):
- "Empathetic Reading" (depth 4, ACT): Sense emotional context (+2 Progress, +2 Leads, +1 to next Social at location, costs 3 Attention)
- "Human Element Focus" (depth 6, ACT): Understand people involved (+3 Progress, +3 Leads, unlock NPC leads, costs 4 Attention)
- "Emotional Reconstruction" (depth 8, ACT): Recreate emotional state (+5 Progress, +4 Leads, deep NPC insights, costs 5 Attention)

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

### Location Properties (5 Tactical Modifiers)

Each location has properties that fundamentally alter investigation tactics:

1. **Delicate** (fragile evidence, high Exposure risk): Exposure +2 per ACT, requires Cunning-focused approach to minimize footprint
2. **Obscured** (degraded/hidden evidence): Progress -1 per ACT, requires high-depth Insight cards generating more Leads
3. **Layered** (complex mystery): Requires diverse stat approach, each ACT generates -1 Lead if using same stat consecutively
4. **Time-Sensitive** (degrading evidence): Leads decay -1 per time segment spent, requires efficient investigation
5. **Resistant** (subtle patterns): Progress capped at ±2 per ACT, but Leads generation normal, requires patient grinding

### Victory Condition

Accumulate Progress threshold (10-20 typical) across one or more visits to location. High Exposure doesn't end investigation but makes future visits harder.

### Example Phase

"Examine the waterwheel mechanism" - Mental challenge, Delicate profile, 15 Progress threshold, at Mill Waterwheel location, requires 10 Focus, costs 1 time segment per session

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
- **Aggression** (balance): Overcautious (-10) to Reckless (+10) spectrum. **Both extremes are bad** - you want to stay near 0 (balanced, controlled).
  - High Aggression (reckless): +1 Danger per action at +3 or higher, injury risk increases
  - Low Aggression (overcautious): -1 Breakthrough per action at -3 or lower, wasting opportunities through hesitation
  - Sweet spot: -2 to +2 range (balanced approach)
- **Understanding** (global, persistent): Tier unlocking across all three systems

### Action Pair

- **EXECUTE**: Lock Option as preparation, spend Exertion (from projection including modifiers like fatigue penalties and Foundation generation), increase Aggression (+1 base + card approach modifier from projection). Multiple EXECUTE actions build prepared sequence and aggressive momentum. **Uses projection to determine affordability** - Exertion cost includes all modifiers.
- **ASSESS**: Evaluate Situation, trigger all locked Options as combo (effects resolve together using full projection per card including Breakthrough, Danger, and Aggression modifiers), decrease Aggression (-2 base + card approach modifier per card from projection), then exhaust all Options back to Situation and draw fresh Options. Careful assessment brings you back toward balanced state. **Applies full projection per locked card** including approach modifiers.

### Card Flow Mechanics

**You prepare actions then execute them.** EXECUTE doesn't immediately perform the action - it locks the Option into position, preparing that move. You can EXECUTE multiple Options, building up a sequence of prepared actions. When you ASSESS the Situation, all locked Options trigger together as a combo, their effects resolving simultaneously. After the combo executes, you've fundamentally changed the physical context - all Options exhaust back to Situation because you've altered what's possible. You draw fresh Options to assess the new Situation. Unplayed Options return to Situation because they were considerations you didn't act on in that moment.

### Stat Binding Examples

Physical cards bind to stats based on physical approach. EXECUTE locks cards for combo execution:

**Authority-Bound** (power, decisive action, command):
- "Power Move" (depth 6, EXECUTE): Decisive forceful action (+4 Breakthrough when combo triggers, +1 Danger, costs 4 Exertion)
- "Commanding Presence" (depth 4, EXECUTE): Assert control over environment (+3 Breakthrough, +1 Danger if only locked card, costs 3 Exertion)
- "Dominant Force" (depth 8, EXECUTE): Overwhelming physical assertion (+6 Breakthrough, +2 Danger, costs 5 Exertion)

**Cunning-Bound** (risk management, tactical precision):
- "Calculated Risk" (depth 5, EXECUTE): Tactical risk assessment (+3 Breakthrough, -1 Danger per locked card in combo, costs 3 Exertion)
- "Tactical Precision" (depth 7, EXECUTE): Precise calculated movement (+4 Breakthrough, if 3+ cards in combo restore 1 Exertion, costs 4 Exertion)
- "Adaptive Technique" (depth 9, EXECUTE): Respond to changing conditions (+5 Breakthrough, combo bonus: +1 per other locked card, costs 5 Exertion)

**Insight-Bound** (structural analysis, finding weaknesses):
- "Structural Analysis" (depth 4, EXECUTE): Identify load-bearing points (+2 Breakthrough, reveal optimal path, reduces Danger of next card in combo, costs 2 Exertion)
- "Find Weakness" (depth 6, EXECUTE): Spot structural vulnerabilities (+3 Breakthrough, if first in combo sequence -2 Danger, costs 3 Exertion)
- "Engineering Assessment" (depth 8, EXECUTE): Complete structural understanding (+4 Breakthrough, combo multiplier: +1 per locked card, costs 4 Exertion)

**Rapport-Bound** (flow state, body awareness, grace):
- "Flow State" (depth 6, EXECUTE): Natural fluid movement (+3 Breakthrough, costs 1 less Exertion if 2+ cards in combo, costs 3 Exertion base)
- "Body Awareness" (depth 4, EXECUTE): Kinesthetic understanding (+2 Breakthrough, maintain balance, costs 2 Exertion)
- "Athletic Grace" (depth 8, EXECUTE): Perfect physical harmony (+4 Breakthrough, if last in combo sequence -1 Danger, costs 4 Exertion)

**Diplomacy-Bound** (measured technique, controlled force):
- "Measured Technique" (depth 4, EXECUTE): Controlled application of force (+2 Breakthrough, efficient technique, costs 2 Exertion)
- "Controlled Force" (depth 6, EXECUTE): Balanced power application (+3 Breakthrough, if only card in combo -1 Danger, costs 3 Exertion)
- "Paced Endurance" (depth 8, EXECUTE): Sustainable effort (+4 Breakthrough, distribute Danger reduction across combo, costs 4 Exertion)

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

### Challenge Types (5 Types of Physical Engagement)

Physical challenges are categorized by type, affecting combo dynamics:

1. **Combat** (tactical fighting): Authority/Cunning/Insight-focused, high Danger from aggression, combo bonuses scale with sequence length
2. **Athletics** (climbing/running/jumping): Insight/Rapport/Cunning-focused, Danger from falls, first card in combo reduces risk for subsequent cards
3. **Finesse** (lockpicking/delicate work): Cunning/Insight/Diplomacy-focused, Danger from mistakes, single-card combos preferred (precision over power)
4. **Endurance** (long marches/holding out): Rapport/Diplomacy/Authority-focused, Danger from exhaustion, longer combos increase Danger but maximize Breakthrough
5. **Strength** (lifting/breaking/forcing): Authority/Insight/Diplomacy-focused, Danger from strain, combo order matters (setup then execution)

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

- **SPEAK**: Play thought from Mind, advance conversation, increment Cadence (+1)
- **LISTEN**: Hear Topics, draw cards to Mind (base 3 + bonuses from negative Cadence), decrement Cadence (-2), apply Doubt penalty from positive Cadence

### Card Flow Mechanics

**Thoughts persist in your mind because that's how thinking works.** When you SPEAK, Statement thoughts move from Mind to Spoken pile (what you've said aloud) while Echo thoughts return to Topics (fleeting thoughts that return to consideration). When you LISTEN to Topics, all thoughts already in Mind remain - your mind doesn't empty when you pause to listen. You draw additional Topics into Mind (base 3 + Cadence bonuses) representing new thoughts and considerations from what you hear. Your Mind accumulates considerations, building up your understanding of the conversation as it progresses.

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

### Card Persistence Mechanics

**Statement Cards**: Move from Mind to Spoken pile when played (what you've said aloud stays said), count toward Statements total
**Echo Cards**: Return from Mind to Topics when played (fleeting thoughts that recirculate), can be played multiple times in conversation

### Victory Condition

Reach Momentum threshold (8-16 typical) before Doubt reaches 10 (which ends conversation).

### Example Phase

"Question the mill owner about sabotage" - Social challenge, Proud personality, 12 Momentum threshold, targets Mill_Owner NPC at Mill venue, costs 0 resources, takes 1+ time segments

---

## Common Architectural Pattern

All three systems follow the same core structure (creating equivalent tactical depth):

**Universal Elements** (identical across all three):
- **Unified 5-stat system**: All cards bind to Insight/Rapport/Authority/Diplomacy/Cunning
- **Builder Resource** → Victory (Progress, Breakthrough, Momentum)
- **Threshold Resource** → Failure consequence (Exposure, Danger, Doubt)
- **Session Resource** → Tactical spending (Attention, Exertion, Initiative)
- **Binary Action Choice** → Tactical rhythm (OBSERVE/ACT, ASSESS/EXECUTE, SPEAK/LISTEN)
- **Understanding** → Persistent tier unlocking (shared globally)
- **5 Tactical Modifiers** → Fundamental gameplay variety (Profiles, Challenge Types, Personalities)
- **Expertise Tokens** → Mechanical benefits from repeated success (Familiarity, Mastery, Connection)
- **Progression Levels** → Cumulative expertise tracking (Depth, Proficiency, Connection)
- **Goal Cards** → Universal victory condition cards unlock at momentum thresholds (same structure across all three systems)

**Intentional Differences** (justified by verisimilitude):
- **Session Models**: Mental (pauseable), Physical (one-shot), Social (session-bounded)
- **Resource Persistence**: Mental resources persist at location, Physical/Social clear on end
- **Permanent Costs**: Mental (Focus), Physical (Health+Stamina), Social (none, but time)
- **Session Resource Mechanics**:
  - Mental Attention: Derived from Focus at start, finite budget, **no replenishment during challenge**
  - Physical Exertion: Derived from Stamina at start, finite budget, **minimal replenishment via specific Foundation cards only**
  - Social Initiative: Starts at 0, **actively builds during session via Foundation cards**, persists through LISTEN
- **Card Flow Mechanics** (distinct per system, respecting verisimilitude):
  - Mental: ACT on Methods generates Leads (depth-based), OBSERVE draws Details equal to Leads count, Methods persist in hand, Leads persist until completion, completed methods move to Applied pile
  - Physical: EXECUTE locks Options as preparation (+Aggression), ASSESS Situation triggers combo (-Aggression) then exhausts all Options back to Situation and draws fresh Options
  - Social: SPEAK moves Statement thoughts from Mind to Spoken (Echo thoughts return to Topics), LISTEN to Topics draws while Mind persists, thoughts accumulate in Mind
- **Pile Naming** (reflects what system does):
  - Mental: Details (scene evidence) → Methods (investigative approaches) → Applied (completed understanding)
  - Physical: Situation (challenge evolution) → Options (available actions) → exhausts back to Situation (no discard - context resets)
  - Social: Topics (NPC's words) → Mind (your thoughts) → Spoken (what you've said)
- **Balance Trackers** (different penalty models):
  - Mental: No balance tracker (uses Leads flow instead)
  - Physical: Aggression (both extremes penalized - overcautious reduces Breakthrough, reckless increases Danger)
  - Social: Cadence (asymmetric - negative gives bonus card draw, positive gives Doubt penalty)
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
- Venues reveal additional options based on what you know
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
- **Specific venues**: Mill, Mill Owner's House
- **Specific locations**: Waterwheel
- **Specific NPCs**: Mill Owner (Proud personality), Miller's Apprentice (Devoted)
- **Narrative text**: Phase descriptions, completion narratives, discovery text
- **Card text**: Challenge-specific card descriptions matching investigation theme
- **Knowledge entries**: "mill_mechanism_damaged", "sabotage_evidence", "suspect_identified"

**Generation Constraints:**
- Must respect mechanical template structure (phase count, challenge types, thresholds)
- Must create valid venue/location/NPC references that exist in world
- Must generate knowledge IDs used by subsequent phases
- Must maintain narrative coherence across phases
- Must balance difficulty progression (early phases easier than late phases)

**Content Validation:**
- Parser validates all references exist in GameWorld (venues, locations, NPCs, challenge types)
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
- Shows Phase 1 requirements and venue

**Journal Integration**
Investigation added to Journal's Investigations tab:
- Shows all phases (locked/unlocked status)
- Displays requirements for locked phases
- Shows progress toward completion (2/5 phases complete)
- Tracks discovered knowledge related to this investigation

**Phase Completion**
Complete tactical challenge → Return to venue screen → Modal appears:
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
When approaching travel between venues, visible options show:
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
- Morning: Fresh start, NPCs in regular venues, quiet investigations
- Midday: Full activity, busy venues, normal operations
- Afternoon: Winding down, some venues closing, changing availability
- Evening: Social time, taverns busy, offices closed, different NPC venues
- Night: Most venues closed, investigation opportunities, travel risks
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
- **Management**: Food costs coins, has weight, must be carried or purchased at venues
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

### Card Flow Reflects Reality

**Mental: You Cannot Observe What You Haven't Investigated**

ACT on Methods generates Leads - investigative threads, evidence to examine, patterns to explore. The depth of your action determines how much material you uncover (depth 1-2 = +1 Lead, depth 3-4 = +2 Leads, depth 5-6 = +3 Leads). OBSERVE then follows those Leads by drawing Details equal to your total Leads. Zero Leads means zero draw because you have no investigative Details to observe.

**Pile Verisimilitude - Mental**:
- **Details** (input pile): Scene evidence, physical clues, observable facts waiting to be examined
- **Methods** (hand): Investigative approaches held in mind, ready to apply to the scene
- **Applied** (discard): Methods you've already used, investigative understanding put into practice

Methods in Methods persist when you OBSERVE because investigation knowledge doesn't vanish when you pause to examine Details. Leads persist when you leave the location because uncovered threads remain available when you return. Only completing the investigation resets Leads - you've resolved the mystery, threads are no longer open. Successfully applied methods move to Applied pile, representing investigative techniques you've already employed.

**Physical: You Prepare Actions Then Execute Them**

EXECUTE doesn't immediately perform the action - it locks the Option into position, preparing that move. Real physical action requires setup. You can EXECUTE multiple Options, building up a prepared sequence - ready your stance, set your grip, position your weight. Each EXECUTE increases Aggression (+1 base, plus card approach modifier from projection) as you commit to action. **EXECUTE uses projection to determine affordability** - the Exertion cost includes modifiers like fatigue penalties and any Foundation generation effects.

When you ASSESS the Situation, all locked Options trigger together as a combo. Their effects resolve simultaneously because you execute the prepared sequence, with **full projection applied per card** (Breakthrough, Danger, Aggression including approach modifiers). ASSESS decreases Aggression (-2 base, plus each card's approach modifier from projection) as careful evaluation brings you back toward balance. After the combo, you've fundamentally changed the physical context - all Options exhaust back to Situation because the challenge has evolved. You draw fresh Options to assess the new Situation. Unplayed Options return to Situation because they were considerations for a context that no longer exists.

**Pile Verisimilitude - Physical**:
- **Situation** (input pile): The evolving physical challenge, what's currently possible
- **Options** (hand): Available actions you're considering in this moment
- **Locked Cards** (exhaust pile): Prepared sequence of actions ready to execute as combo, **displayed in UI above hand with special "LOCKED" styling**
- **No traditional discard pile**: All Options exhaust back to Situation after combo execution - the challenge resets with new possibilities

**Projection Principle**: EXECUTE and ASSESS use PhysicalEffectResolver projections as single source of truth for all effects. Projections include: Exertion changes (with modifiers like fatigue penalties or Foundation generation), Aggression changes (action balance + card approach modifier), Breakthrough/Danger changes, and all strategic resource costs. The facade applies projections to session state - never hardcodes values or reads directly from card templates.

Aggression tracks your physical approach: Both extremes are dangerous. Too overcautious (-3 or below) means hesitation reduces Breakthrough - you're wasting opportunities. Too reckless (+3 or above) means carelessness increases Danger - you're risking injury. You want to stay balanced (-2 to +2), executing with controlled commitment then assessing to recalibrate.

**Social: Thoughts Persist Because That's How Thinking Works**

When you SPEAK, Statement thoughts move from Mind to Spoken pile (what you've said aloud stays said) while Echo thoughts return to Topics (fleeting thoughts that return to consideration). When you LISTEN to Topics, all thoughts in Mind remain - your mind doesn't empty when you pause to listen. You draw Topics into Mind representing new thoughts and considerations from what you hear. Your Mind accumulates understanding as the conversation progresses.

**Pile Verisimilitude - Social**:
- **Topics** (input pile): The NPC's words, what they're saying that you can hear and consider
- **Mind** (hand): Your thoughts, considerations, things you're thinking about saying
- **Spoken** (discard): What you've said aloud, statements that cannot be unsaid

Cadence tracks conversation dominance: Unlike Physical's symmetric penalties, Social's Cadence is asymmetric by design. Negative Cadence (deferential, giving space) REWARDS you with bonus card draw when you LISTEN. Positive Cadence (dominating) PENALIZES you with Doubt accumulation. Sometimes you intentionally go negative to gain card advantage.

### Balance Tracker Philosophy

**Physical Aggression** (symmetric penalties): Both extremes are bad. You want to stay balanced. Going too far in either direction creates problems.

**Social Cadence** (asymmetric rewards/penalties): One direction is strategically valuable (negative = card draw bonus), the other is dangerous (positive = Doubt penalty). You intentionally manage which side you're on.

**Mental** (no balance tracker): Uses Leads flow instead - you generate investigative threads then follow them. No spectrum to manage.

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
5. **Binary Actions**: OBSERVE-ACT / ASSESS-EXECUTE / SPEAK-LISTEN pairs (but distinct mechanics per system)
6. **5 Tactical Modifiers**: Profiles/Challenge Types/Personalities fundamentally alter tactics
7. **Expertise Tokens**: Familiarity/Mastery/Connection provide mechanical benefits
8. **Progression Levels**: Depth/Proficiency/Connection track cumulative mastery
9. **Goal Cards**: Victory condition cards unlock at thresholds
10. **Understanding**: Shared tier-unlocking resource

**What Differentiates (verisimilitude-justified card flow):**
- **Mental**: Leads generation (ACT on Methods by depth) → Leads-based draw (OBSERVE Details), Methods persist in hand, Leads persist until completion, Applied pile for completed methods, no balance tracker
- **Physical**: Preparation locking (EXECUTE Options +Aggression) → Combo execution (ASSESS Situation -Aggression, exhausts all Options back to Situation), Aggression penalizes both extremes, no discard pile
- **Social**: Statement/Echo persistence (SPEAK moves Statements from Mind to Spoken, Echoes to Topics) → Accumulating thoughts (LISTEN to Topics draws while Mind persists), Cadence asymmetrically rewards negative, three distinct piles (Topics/Mind/Spoken)

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

**ChallengeDeck Configuration (Direct Reference):**
Goals reference ChallengeDeck entities directly via `deckId`. Each ChallengeDeck defines:
- **Deck ID**: Unique identifier (e.g., "desperate_request", "mental_challenge", "athletics_challenge")
- **Card IDs**: Which cards are in this deck (conversation cards, investigation cards, physical action cards)
- **Description**: Narrative context for this challenge type
- **Initial Hand Size**: Starting cards (typically 5)
- **Max Hand Size**: Maximum hand capacity (typically 7)

**Architectural Simplification:**
- **Before**: Goal → ChallengeType (intermediary) → ChallengeDeck (two-step lookup)
- **After**: Goal → ChallengeDeck (direct reference, one-step lookup)
- **Result**: ChallengeType entities eliminated - unnecessary intermediary providing no unique value
- ChallengeDeck is ONLY card container (CardIds list)
- GoalCard is single source of truth for victory conditions (thresholds, rewards)

**Context-Specific Challenge Decks:**
- **Social Decks**: Investigation conversations, NPC requests, relationship building, information gathering, negotiation
- **Mental Decks**: Location investigations, evidence examination, pattern analysis, deduction challenges
- **Physical Decks**: Combat, Athletics, Finesse, Endurance, Strength challenges

All three tactical systems provide the SAME depth through parallel architecture (builder/threshold/session resources + binary actions + system-specific card flow mechanics).

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
- Venue first-visit vignettes (atmospheric establishment)
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
- Venue descriptions deepening with familiarity

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
- Regular NPC venues
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
- Special investigations (when venues empty)
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
- Expansion venues and NPCs
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
- Additional venues with investigations
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
- Access venues through alternatives
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
