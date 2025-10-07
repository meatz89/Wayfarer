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
- **Session Resource**: Limited spendable resource (Attention, Position, Initiative)
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
  - Resources: Progress (builder), Attention (session), Exposure (threshold), ObserveActBalance (balance)
  - Actions: OBSERVE (cautious builder) / ACT (aggressive spender)

- **Physical Challenges**: Strength-based obstacles at location spots (climb carefully, leverage tools, precise movement)
  - Resources: Breakthrough (builder), Position (session), Danger (threshold), Commitment (balance)
  - Actions: ASSESS (cautious builder) / EXECUTE (aggressive spender)

- **Social Challenges**: Rapport-based conversations with NPCs (build trust, gather information, persuade cooperation)
  - Resources: Momentum (builder), Initiative (session), Doubt (threshold), Cadence (balance), Statements (history)
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

**Mental Challenges** - Observation-based investigation at location spots

*Core Resources:*
- **Progress** (builder): Built toward victory threshold (typically 10-20)
- **Attention** (session resource): Spent on Act cards, limited pool (max 10), represents investigative focus
- **Exposure** (threshold): Accumulates from reckless investigation, persists at location, failure at maximum
- **ObserveActBalance** (balance): Tracks cautious (-10) vs reckless (+10) approach, affects penalties
- **Understanding** (tier unlock): Persistent sophistication, unlocks higher card tiers

*Action Pair:*
- **OBSERVE**: Play card cautiously, build Progress, shift balance toward overcautious
- **ACT**: Play card aggressively, spend Attention, shift balance toward reckless, build Progress faster with more Exposure risk

*Victory Condition:* Reach Progress threshold before Exposure exceeds maximum
*Example Phase:* "Examine the waterwheel mechanism" (Mental challenge, 15 Progress threshold, at Mill spot)

---

**Physical Challenges** - Strength-based obstacles at location spots

*Core Resources:*
- **Breakthrough** (builder): Built toward victory threshold (typically 10-20)
- **Position** (session resource): Represents advantageous positioning, spent on Execute cards (max 10)
- **Danger** (threshold): Accumulates from risky actions, failure at maximum
- **Commitment** (balance): Tracks cautious (-10) vs decisive (+10) approach, affects card effectiveness
- **Understanding** (tier unlock): Persistent capability, unlocks higher card tiers

*Action Pair:*
- **ASSESS**: Play card cautiously, build Breakthrough slowly, shift balance toward overcautious
- **EXECUTE**: Play card decisively, spend Position, shift balance toward decisive, build Breakthrough faster with more Danger

*Victory Condition:* Reach Breakthrough threshold before Danger exceeds maximum
*Example Phase:* "Climb the damaged mill wheel" (Physical challenge, 15 Breakthrough threshold, at Mill spot)

---

**Social Challenges** - Rapport-based conversations with NPCs

*Core Resources:*
- **Momentum** (builder): Built toward victory threshold (typically 8-16), tracks conversation progress
- **Initiative** (session resource): Conversation action economy, accumulated through Foundation cards, spent on higher-cost cards, persists through LISTEN
- **Doubt** (threshold): NPC skepticism, accumulates through failed approaches and Cadence penalties, failure at maximum (10)
- **Cadence** (balance): Tracks dominating (+10) vs deferential (-10) conversation style, creates Doubt penalties or card draw bonuses during LISTEN
- **Statements** (history): Count of Statement cards played, creates conversation memory, determines time cost
- **Understanding** (tier unlock): Persistent connection depth, unlocks higher card tiers

*Action Pair:*
- **SPEAK**: Play card from hand, advance conversation state, increment Cadence (+1)
- **LISTEN**: Reset action, draw cards (base 3 + bonuses from negative Cadence), decrement Cadence (-2), apply Doubt penalty from positive Cadence

*Victory Condition:* Reach Momentum threshold before Doubt exceeds maximum
*Additional Complexity:* Personality Rules (Proud, Devoted, Mercantile, Cunning, Steadfast) modify base mechanics per NPC
*Example Phase:* "Question the mill owner about sabotage" (Social challenge, 12 Momentum threshold, targets Mill_Owner NPC)

---

**Common Architectural Pattern:**

All three systems follow the same structure:
- **Builder Resource** → Victory (Progress, Breakthrough, Momentum)
- **Threshold Resource** → Failure (Exposure, Danger, Doubt)
- **Session Resource** → Tactical spending (Attention, Position, Initiative)
- **Balance Tracker** → Playstyle modifier (ObserveActBalance, Commitment, Cadence)
- **Binary Action Choice** → Tactical rhythm (OBSERVE/ACT, ASSESS/EXECUTE, SPEAK/LISTEN)
- **Understanding** → Persistent tier unlocking (shared across all three)

The systems are **parallel implementations** with distinct resource names, thresholds, and gameplay rhythms. Social has the most complexity (5 resources + Personality Rules), Physical and Mental are more streamlined (4 resources each).

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

**Health:**
- Maximum 100
- Lost to: Physical hazards, environmental exposure, injuries from failures
- Recovered through: Rest (blocks spent sleeping/recuperating), medical treatment, food with restorative properties
- Critical threshold: Below 30 increases danger of further harm

**Stamina:**
- Maximum 100
- Lost to: Physical exertion (travel, investigation, labor)
- Recovered through: Rest blocks, food, reduced activity
- Depletion effects: Cannot attempt stamina-requiring activities when insufficient

**Hunger:**
- Increases 20 per block
- Maximum 100
- Effects: At 75+ movement slowed, work efficiency reduced, stamina recovery impaired
- Management: Food costs coins, weighs capacity, must be carried or purchased at locations

**Coins:**
- Earned through: Completed deliveries, work, trade, discoveries
- Spent on: Food, equipment, route fees (bribes, transport), supplies, information
- Balance: Must work for income vs. spend time on investigation/relationships

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
