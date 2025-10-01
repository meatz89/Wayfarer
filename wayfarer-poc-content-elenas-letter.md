# Wayfarer POC: Elena's Letter - Refined System

## Table of Contents
1. [Scenario Overview](#scenario-overview)
2. [Starting Conditions](#starting-conditions)
3. [Player Starting Deck](#player-starting-deck)
4. [NPCs](#npcs)
5. [Stranger Encounters](#stranger-encounters)
6. [Locations and Investigation](#locations-and-investigation)
7. [Observation Cards](#observation-cards)
8. [Request Cards](#request-cards)
9. [Routes and Travel](#routes-and-travel)
10. [The Optimal Path](#the-optimal-path)
11. [Alternative Approaches by Build](#alternative-approaches-by-build)
12. [Failure Analysis](#failure-analysis)
13. [System Reference](#system-reference)

## Scenario Overview

### The Story
Elena, a young scribe at the Copper Kettle Tavern, desperately needs to refuse an arranged marriage. Lord Blackwood, who could intervene, leaves the city at 5:00 PM. The player must navigate complex systems to help Elena deliver her letter before the deadline.

### The Core Challenge
This POC demonstrates how conversations are the primary gameplay loop. The player uses conversation type decks filtered by their stat depths against different NPC personality rules while developing their problem-solving methodologies (stats) through play. Success requires understanding how each personality transforms the basic conversation puzzle and how your developing stats open new card depths to problems.

### The Discovery
Every seemingly inefficient action has purpose. Investigation unlocks critical observation cards. Building Diplomacy tokens with Marcus enables signature cards. Optional stranger encounters provide resources and stat development. The optimal time path emerges through system mastery, but different stat builds offer alternative approaches.

### Success Criteria
- Build Market Square familiarity through investigation
- Gain Diplomacy tokens to unlock Marcus's signature cards
- Use observation cards to advance Elena's conversation
- Optional: Develop stats through stranger encounters
- Manage resources with precision
- Complete Elena's letter delivery before 5:00 PM

## Starting Conditions

### Player State
- **Time**: 9:00 AM Tuesday (Morning block, segment 4)
- **Segments Remaining**: 1 in current block, 13 in day total
- **Coins**: 0
- **Hunger**: 50
- **Health**: 100
- **Satchel Weight**: 3/10 (Viktor's package weighs 3)

### Player Stats (All Level 1)
- **Insight**: Level 1 (0/10 XP) - Analytical thinking
- **Rapport**: Level 1 (0/10 XP) - Emotional connection  
- **Authority**: Level 1 (0/10 XP) - Force of personality
- **Diplomacy**: Level 1 (0/10 XP) - Mercantile thinking
- **Cunning**: Level 1 (0/10 XP) - Indirect approaches

### All NPCs
- **Tokens**: 0 with all NPCs
- **Connection States**: All at default
- **Familiarity**: 0 at all locations

### Initial Obligation
**Viktor's Package to Marcus**
- Position: 1 (must complete first)
- Deadline: 12:00 PM (Midday block)
- Payment: 7 coins
- Weight: 3
- Recipient: Marcus at Market Square

### Available Time
- Start: 9:00 AM (Morning block, segment 4)
- Lord Blackwood leaves: 5:00 PM (Afternoon block, segment 4)
- Total segments: 13 available
- Optimal path: 9-12 segments depending on choices

## Player Starting Deck

The conversation system uses type-specific decks filtered by stat depth access. With all stats at Level 1, players can only access depth 1-2 cards from any conversation type.

### Depth Access by Stat Level
- **Level 1**: Access depths 1-2 (Foundation cards only)
- **Level 3**: Access depths 1-4 (Foundation + Standard)
- **Level 5**: Access depths 1-6 (Foundation + Standard + Advanced)
- **Level 7**: Access depths 1-8 (Foundation through Powerful)
- **Level 9**: Access depths 1-10 (Complete mastery)

### Card Structure
Every card has:
- **Initiative Cost**: 0 for depth 1-2, scales with depth
- **Either** Requirement OR Cost (never both):
  - Requirement: Condition to play (e.g., "5+ Statements in Spoken")
  - Cost: Consume resource (e.g., "Consume 3 Momentum")
- **One Effect**: Deterministic result
  - Flat increase ("+3 Momentum")
  - Scaled increase ("+1 Momentum per 2 Statements")
  - Flat decrease ("-2 Doubt")

### Foundation Cards Available at Level 1
Since all stats start at Level 1, players access only depth 1-2 cards:

**Depth 1 (0 Initiative Cost) - Builders**:
- Generate Initiative to enable higher-cost cards
- Provide small momentum gains
- Create conversation foundation

**Depth 2 (0-2 Initiative Cost) - Early Options**:
- Small effects requiring minimal setup
- Begin building toward requirements

## NPCs

### Elena - The Desperate Scribe (Level 3 Conversation)

**Basic Properties**:
- Name: Elena
- Location: Copper Kettle Tavern, Corner Table
- Personality: DEVOTED
- Rule: "Doubt increases by +2 instead of +1"
- Starting Initiative: 0
- Token Type: Trust
- **Conversation Level**: 3 (3 XP per card played)

**Signature Cards** (Mixed based on Trust tokens):
- 1 token: "Elena's Faith" (Rapport card, depth 3)
- 3 tokens: "Shared Understanding" (Rapport card, depth 4)
- 6 tokens: "Elena's Trust" (Authority card, depth 5)
- 10 tokens: "Emotional Bond" (Rapport card, depth 6)

**Request**: Elena's Letter
- Available at 8 momentum
- Basic (8 momentum): Letter weight 1, deadline 5 PM
- Enhanced (12 momentum): Priority letter weight 2, +1 Trust token
- Premium (16 momentum): Legal documents weight 3, +2 Trust tokens

**Conversation Mechanics with Elena**:
- Devoted personality means card effects that increase Doubt add +2 instead of +1
- Level 3 conversation grants 3 XP per card played to bound stat
- Time cost: 1 segment + Statement cards in Spoken pile

### Marcus - The Merchant (Level 2 Conversation)

**Basic Properties**:
- Name: Marcus  
- Location: Market Square, Merchant Row
- Personality: MERCANTILE
- Rule: "Highest Initiative cost card each turn gains double effect"
- Starting Initiative: 0
- Token Type: Diplomacy
- **Conversation Level**: 2 (2 XP per card played)

**Signature Cards**:
- 1 token: "Marcus's Bargain" (Diplomacy card, depth 3)
- 3 tokens: "Trade Knowledge" (Diplomacy card, depth 4)
- 6 tokens: "Commercial Trust" (Diplomacy card, depth 5)
- 10 tokens: "Marcus's Favor" (Authority card, depth 6)

**Request**: Trade Letter to Warehouse
- Basic (8 momentum): 5 coins, +1 Diplomacy token
- Enhanced (12 momentum): 8 coins, +2 Diplomacy tokens
- Premium (16 momentum): 12 coins, +3 Diplomacy tokens

**Exchange Options**:
- Buy Food: 2 coins → -50 hunger
- Buy Bread: 3 coins → bread item (weight 1)
- Join Caravan: 10 coins → Noble Quarter transport (requires 2+ Diplomacy tokens)

### Lord Blackwood - The Noble

**Basic Properties**:
- Name: Lord Blackwood
- Location: Noble Quarter, Blackwood Manor
- Personality: PROUD
- Rule: "Cards must be played in ascending Initiative order"
- Leaves: 5:00 PM sharp
- **Note**: Letter has noble seal, no conversation required

## Stranger Encounters

Optional conversations for resources and XP. Each stranger available once per time block.

### Morning - Market Square

**Tea Vendor** (Level 1)
- Location: Market Square Fountain
- Personality: STEADFAST (all effects capped at ±2)
- Conversation Rewards:
  - Basic (8 momentum): 2 coins
  - Enhanced (12 momentum): 4 coins
  - Premium (16 momentum): Tea item (weight 1, +2 Initiative in conversations)
- **XP Benefit**: 1 XP per card played

**Pilgrim** (Level 1)  
- Location: Market Square entrance
- Personality: DEVOTED (doubt increases +2)
- Conversation Rewards:
  - Basic (8 momentum): Blessing (+1 starting Initiative today)
  - Enhanced (12 momentum): Reveals one observation
  - Premium (16 momentum): Holy symbol (weight 1)
- **XP Benefit**: 1 XP per card played

### Midday - Copper Kettle Tavern

**Traveling Scholar** (Level 2)
- Location: Tavern common room
- Personality: STEADFAST
- Conversation Rewards:
  - Basic (8 momentum): Information about routes
  - Enhanced (12 momentum): 5 coins
  - Premium (16 momentum): Insight training (next 10 Insight cards grant +1 XP)
- **XP Benefit**: 2 XP per card played

### Afternoon - Various Locations

**Foreign Merchant** (Level 3)
- Location: Market Square
- Personality: MERCANTILE (highest Initiative card double effect)  
- Conversation Rewards:
  - Basic (8 momentum): Trade goods (weight 2, value 8 coins)
  - Enhanced (12 momentum): Exotic permit (weight 1)
  - Premium (16 momentum): Partnership (-1 coin all exchanges today)
- **XP Benefit**: 3 XP per card played

## Locations and Investigation

### Market Square

**Standard Investigation**: 1 segment for base familiarity
- Morning (Quiet): +2 familiarity
- Other times (Busy): +1 familiarity

**Stat-Gated Approaches** (Require Level 2+):
- **Systematic Observation** (Insight 2+): +1 additional familiarity
- **Local Inquiry** (Rapport 2+): Learn which NPCs want observations
- **Purchase Information** (Diplomacy 2+): 2 coins per familiarity level
- **Note**: Authority and Cunning approaches not useful here

**Observation Rewards**:
- Familiarity 1: "Safe Passage Knowledge" (Insight card for Elena)
- Familiarity 2: "Merchant Caravan Route" (Diplomacy card for Marcus)

### Copper Kettle Tavern

**Standard Investigation**: 1 segment for +1 familiarity

**Stat-Gated Approaches**:
- **Local Inquiry** (Rapport 2+): Learn Elena needs comfort
- **Covert Search** (Cunning 2+): Find hidden letter draft

### Noble Quarter

**Standard Investigation**: Requires permit or Authority 2+
- **Demand Access** (Authority 2+): Investigate without permit

## Observation Cards

### Safe Passage Knowledge
- **Stat**: Insight
- **Depth**: 3
- **Initiative Cost**: 2
- **Effect**: +4 Momentum
- **Critical**: Provides momentum boost for Elena conversation

### Merchant Caravan Route  
- **Stat**: Diplomacy
- **Depth**: 3
- **Initiative Cost**: 3
- **Effect**: Unlocks Marcus's caravan exchange option
- **Value**: Alternative to expensive permit

## Routes and Travel

### Standard Paths

**Market Square → Copper Kettle**: 1 segment
**Market Square → Warehouse**: 1 segment  
**Copper Kettle → Noble Quarter**: 2 segments
**Market Square → Noble Quarter**: 2 segments

### Stat-Gated Paths

**Scholar's Shortcut** (Insight 2+)
- Market Square → Noble Quarter: 1 segment instead of 2

**Local's Path** (Rapport 2+)
- Copper Kettle → Noble Quarter: 1 segment instead of 2

**Shadow Alley** (Cunning 2+)
- Warehouse → Noble Quarter: 1 segment (new connection)

**Merchant Road** (Diplomacy 2+)
- Any route: -10 minutes if carrying trade goods

**Noble's Gate** (Authority 2+ OR Noble Permit)
- Direct access to Noble Quarter from anywhere: 1 segment

## The Optimal Path

### Morning Block (9:00 AM - 10:00 AM)

**9:00 AM - Investigate Market Square** (Segment 5)
- Standard investigation in Quiet morning: +2 familiarity
- Unlock "Safe Passage Knowledge" observation
- **Morning block ends**

### Midday Block (12:00 PM - 2:00 PM)

**12:00 PM - Investigate Market Square Again** (Segment 1)
- Now Busy: +1 familiarity (total: 3)
- Unlock "Merchant Caravan Route" observation

**12:20 PM - Conversation with Marcus** (Segment 2-3)
- Level 2 conversation (2 XP per card)
- Deliver Viktor's package first
- Use Foundation cards to build Initiative
- Play highest Initiative cards for Mercantile bonus (double effect)
- Use both observation cards when drawn
- Build to 12 momentum for Enhanced goal
- Receive: 8 coins, 2 Diplomacy tokens
- Accept trade letter (weight 1)
- XP gained: ~8-12 to various stats
- Time: 1 segment + ~2 Statements played

**12:40 PM - Quick Travel** (Segment 4)
- To Warehouse District: 1 segment
- Deliver Marcus's letter: +7 coins (total: 15)

**1:00 PM - Return to Market Square** (End of block)
- 1 segment travel

### Afternoon Block (2:00 PM - 6:00 PM)

**2:00 PM - Final Marcus Preparation** (Segment 1)
- Buy food if needed (-2 coins, -50 hunger)
- Unlock caravan with 2 Diplomacy tokens
- Buy caravan passage (-10 coins, have 5 remaining)

**2:20 PM - Travel to Copper Kettle** (Segment 2)

**2:40 PM - Critical Elena Conversation** (Segment 3-5)
- Level 3 conversation (3 XP per card played)
- Starting: 0 Initiative, 0 Cadence
- Devoted personality: Doubt increases +2
- Use Foundation cards to build Initiative
- Carefully manage Cadence (avoid high positive)
- Build to 8+ momentum for Basic letter
- Accept basic letter (weight 1)
- XP gained: ~15-24 to various stats
- Time: 1 segment + ~3-4 Statements

**3:20 PM - Caravan to Noble Quarter** (Segment 6)
- Use pre-purchased caravan passage
- Instant travel via caravan
- Deliver letter to Lord Blackwood
- **SUCCESS** with ~40 minutes buffer

## Alternative Approaches by Build

### Insight Build (If Insight reaches Level 2)
- Morning: Use Systematic Observation for +3 familiarity total
- Unlock both observations in one investigation
- Save 1 segment for stranger encounter
- Access depth 3-4 Insight cards in conversations
- Take Scholar's Shortcut to Noble Quarter (1 segment saved)

### Rapport Build (If Rapport reaches Level 2)
- Access depth 3-4 Rapport cards
- Better momentum generation with Elena (Devoted personality)
- Take Local's Path from Tavern to Noble Quarter
- Could attempt Elena's Enhanced goal more safely

### Authority Build (If Authority reaches Level 2)
- Skip permit/caravan entirely
- Use Noble's Gate for direct access (1 segment)
- More coins available for resources
- Authority cards risky with Elena's Devoted rule

### Diplomacy Build (Natural from Marcus conversation)
- Marcus conversation likely pushes Diplomacy toward Level 2
- Access depth 3-4 Diplomacy cards
- Better Marcus conversation outcomes with Mercantile bonus
- Merchant Road speeds up goods delivery

### Cunning Build (If developed through strangers)
- Shadow Alley creates new route options
- Covert investigations don't alert NPCs
- High risk with straightforward NPCs like Marcus

## Failure Analysis

### Common Failure Paths

#### Work Instead of Investigate
- Work consumes entire 4-segment block
- Cannot complete necessary steps before deadline
- Maximum 11 segments left after work

#### Skip Morning Investigation
- Miss quiet bonus (+2 vs +1 familiarity)
- May need extra segment later
- Delays observation card acquisition

#### Ignore Stranger Opportunities
- Miss XP that could push stat to Level 2
- Miss resources (coins, items)
- Miss alternate solutions

#### Rush Elena Without Preparation
- 0 Initiative start requires Foundation cards
- Cannot reach momentum thresholds without building
- Safe Passage Knowledge provides critical momentum

#### Poor Resource Management in Conversations
- High positive Cadence causes excessive Doubt on LISTEN
- Not building Initiative before attempting powerful cards
- Consuming momentum too early, unable to reach goals

#### Accept Heavy Obligations
- Marcus offers 5-weight silk delivery
- Would max out carrying capacity
- Cannot accept Elena's letter

### Why The Path Works

1. **Investigation timing**: Morning quiet bonus maximized
2. **Conversation management**: Build Initiative, manage Cadence, reach goals
3. **Stat development**: Natural Diplomacy growth from Marcus
4. **Resource management**: Coins for caravan, food if needed
5. **Weight management**: Never exceed capacity
6. **Time efficiency**: 9 segments used, 4 buffer remaining

## System Reference

### Conversation Resources

**Initiative** - Conversational Action Economy:
- Starting Value: 0
- Persists between LISTEN actions
- Cards cost their Initiative value when played
- Foundation cards (depth 1-2) generate Initiative

**Momentum** - Progress Toward Goals:
- Range: 0 to 20+
- Goal thresholds: Basic (8), Enhanced (12), Premium (16)
- Can be consumed by cards for effects
- Primary victory condition

**Doubt** - Conversation Timer:
- Range: 0 to 10
- Conversation ends at 10 Doubt
- Increases through Cadence or card effects
- Reduced by cards that consume momentum

**Cadence** - Conversation Balance:
- Range: -5 to +5
- Always starts at 0
- SPEAK action: +1 Cadence
- LISTEN action: -2 Cadence
- Positive Cadence: +1 Doubt per point on LISTEN
- Negative Cadence: +1 card draw per point on LISTEN

**Statements in Spoken** - Conversation History:
- Count of Statement cards played (not Echo)
- Scales card effects
- Enables requirements
- Determines conversation time cost

### Card Depth Architecture

- **Depth 1-2**: Foundation (0 Initiative, generate Initiative)
- **Depth 3-4**: Standard (0-4 Initiative cost)
- **Depth 5-6**: Advanced (2-6 Initiative cost)
- **Depth 7-8**: Powerful (4-8 Initiative cost)
- **Depth 9-10**: Master (7-12 Initiative cost)

### Stat Progression
- **Level 1**: Access depths 1-2
- **Level 2**: Access depths 1-3, unlock investigation approaches
- **Level 3**: Access depths 1-4
- **Level 4**: Access depths 1-5
- **Level 5**: Access depths 1-6
- **Level 6**: Access depths 1-7
- **Level 7**: Access depths 1-8
- **Level 8**: Access depths 1-9
- **Level 9+**: Access all depths (1-10)

### XP Requirements
- Level 1→2: 10 XP
- Level 2→3: 25 XP
- Level 3→4: 50 XP
- Level 4→5: 100 XP

### XP Accumulation
- Every card played grants XP to its bound stat
- Multiplied by conversation difficulty:
  - Level 1: 1 XP per card
  - Level 2: 2 XP per card
  - Level 3: 3 XP per card

### Conversation Time Cost
- Base: 1 segment
- Additional: +1 segment per Statement card in Spoken pile
- Echo cards don't add time (they return to deck)

### Investigation Approaches by Stat
- **Insight**: Systematic patterns and connections
- **Rapport**: Social information networks
- **Authority**: Forced access to restricted areas
- **Diplomacy**: Purchase information directly
- **Cunning**: Covert investigation without detection

### Personality Rules
- **Devoted**: Doubt increases +2 instead of +1
- **Mercantile**: Highest Initiative card each turn gets double effect
- **Proud**: Cards must be played in ascending Initiative order
- **Cunning**: Playing same Initiative as previous costs 2 Momentum
- **Steadfast**: All effects capped at ±2

## Conclusion

This POC demonstrates the complete Wayfarer experience:

1. **Conversations drive everything**: Initiative management and Cadence balance create tactical depth
2. **Stats are methodologies**: Unlock deeper card access as you develop
3. **Strangers provide options**: Optional grinding for resources and XP
4. **Stats gate content**: Higher stats unlock new solutions
5. **Multiple builds work**: Time-optimal path exists but alternatives viable
6. **Every choice matters**: Resource management shapes outcomes
7. **Personality rules transform gameplay**: Same cards, different puzzle

The scenario takes 30-45 minutes and teaches all core mechanics. Players finish understanding that success requires both tactical resource management (Initiative building, Cadence balance, momentum goals) and strategic character development through stat progression and depth access.