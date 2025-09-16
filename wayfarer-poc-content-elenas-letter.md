# Wayfarer POC: Elena's Letter - Updated for Stat System

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
This POC demonstrates how conversations are the primary gameplay loop. The player uses their starting conversation deck against different NPC personality rules while developing their problem-solving methodologies (stats) through play. Success requires understanding how each personality transforms the basic conversation puzzle and how your developing stats open new approaches to problems.

### The Discovery
Every seemingly inefficient action has purpose. Investigation unlocks critical observation cards. Building Commerce tokens with Marcus enables signature cards. Optional stranger encounters provide resources and stat development. The optimal time path emerges through system mastery, but different stat builds offer alternative approaches.

### Success Criteria
- Build Market Square familiarity through investigation
- Gain Commerce tokens to unlock Marcus's signature cards
- Use observation cards to advance Elena's connection state
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
- **Commerce**: Level 1 (0/10 XP) - Mercantile thinking
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

20 cards representing basic social repertoire, distributed evenly across stats (4 cards each):

### Insight Cards (Analytical)
1. **"Let me analyze"** - Focus 1, Easy (75%), Thought/Rapport/None/None
2. **"Notice the pattern"** - Focus 2, Medium (60%), Thought/Threading/None/None
3. **"Consider the evidence"** - Focus 3, Medium (60%), Thought/Rapport/None/None
4. **"Critical observation"** - Focus 4, Hard (50%), Impulse/Threading/Backfire/Threading

### Rapport Cards (Empathetic)
5. **"I understand"** - Focus 1, Easy (75%), Thought/Rapport/None/None
6. **"Let me help"** - Focus 2, Medium (60%), Thought/Rapport/None/None
7. **"Share your burden"** - Focus 3, Medium (60%), Thought/Rapport/Backfire/None
8. **"Deep connection"** - Focus 4, Hard (50%), Thought/Rapport/None/None

### Authority Cards (Commanding)
9. **"Listen carefully"** - Focus 1, Easy (75%), Thought/Rapport/None/None
10. **"Take charge"** - Focus 2, Medium (60%), Opening/Atmospheric-Focused/None/Focusing
11. **"Direct order"** - Focus 3, Hard (50%), Impulse/Rapport/Backfire/Regret
12. **"Final word"** - Focus 5, Very Hard (40%), Impulse/Rapport/Disrupting/Regret

### Commerce Cards (Transactional)
13. **"Fair exchange"** - Focus 1, Easy (75%), Thought/Rapport/None/None
14. **"Find the angle"** - Focus 2, Medium (60%), Thought/Threading/None/None
15. **"Mutual benefit"** - Focus 3, Medium (60%), Thought/Rapport/None/None
16. **"Close the deal"** - Focus 4, Hard (50%), Thought/Promising/Backfire/None

### Cunning Cards (Indirect)
17. **"Subtle hint"** - Focus 1, Easy (75%), Thought/Rapport/None/None
18. **"Misdirection"** - Focus 2, Medium (60%), Opening/Atmospheric-Patient/None/Threading
19. **"Hidden meaning"** - Focus 3, Medium (60%), Thought/Rapport/None/None
20. **"Perfect lie"** - Focus 4, Hard (50%), Impulse/Rapport/Disrupting/Regret

**Note**: As stats develop, cards gain bonuses:
- Level 2: +10% success rate
- Level 3: All cards gain Persistent keyword
- Level 4: +20% success rate
- Level 5: Never force LISTEN on failure

## NPCs

### Elena - The Desperate Scribe (Level 3 Conversation)

**Basic Properties**:
- Name: Elena
- Location: Copper Kettle Tavern, Corner Table
- Personality: DEVOTED
- Rule: "When rapport decreases, decrease it twice"
- Starting State: DISCONNECTED (3 focus, draws 3)
- Token Type: Trust
- **Conversation Level**: 3 (3 XP per card played)

**Signature Cards** (Require Trust tokens):
- 1 token: "Elena's Faith" (Rapport card, Thought/Rapport/None/None)
- 3 tokens: "Shared Understanding" (Rapport card, Thought/Threading/None/None)
- 6 tokens: "Elena's Trust" (Authority card, Impulse/Rapport/None/Regret)
- 10 tokens: "Emotional Bond" (Rapport card, Thought/Atmospheric-Receptive/None/None)

**Request**: Elena's Letter
- Available at 5 focus (requires NEUTRAL state)
- Basic (5 rapport): Letter weight 1, deadline 5 PM
- Enhanced (10 rapport): Priority letter weight 2, +1 Trust token
- Premium (15 rapport): Legal documents weight 3, +2 Trust tokens

### Marcus - The Merchant (Level 2 Conversation)

**Basic Properties**:
- Name: Marcus  
- Location: Market Square, Merchant Row
- Personality: MERCANTILE
- Rule: "Highest focus card each turn gains +30% success"
- Starting State: NEUTRAL (5 focus, draws 4)
- Token Type: Commerce
- **Conversation Level**: 2 (2 XP per card played)

**Signature Cards**:
- 1 token: "Marcus's Bargain" (Commerce card, Thought/Rapport/None/None)
- 3 tokens: "Trade Knowledge" (Commerce card, Thought/Threading/None/None)
- 6 tokens: "Commercial Trust" (Commerce card, Thought/Rapport/Backfire/None)
- 10 tokens: "Marcus's Favor" (Authority card, Thought/Rapport/None/None)

**Request**: Trade Letter to Warehouse
- Basic (5 rapport): 5 coins, +1 Commerce token
- Enhanced (10 rapport): 8 coins, +2 Commerce tokens
- Premium (15 rapport): 12 coins, +3 Commerce tokens

**Exchange Options**:
- Buy Food: 2 coins → -50 hunger
- Buy Bread: 3 coins → bread item (weight 1)
- Join Caravan: 10 coins → Noble Quarter transport (requires 2+ Commerce tokens)

### Lord Blackwood - The Noble

**Basic Properties**:
- Name: Lord Blackwood
- Location: Noble Quarter, Blackwood Manor
- Personality: PROUD
- Rule: "Cards must be played in ascending focus order"
- Leaves: 5:00 PM sharp
- **Note**: Letter has noble seal, no conversation required

## Stranger Encounters

Optional conversations for resources and XP. Each stranger available once per time block.

### Morning - Market Square

**Tea Vendor** (Level 1)
- Location: Market Square Fountain
- Personality: STEADFAST (rapport changes capped at ±2)
- Conversation Types:
  - Friendly Chat: 2 coins (5 rapport)
  - Buy Special Tea: Tea item for 4 coins (10 rapport, weight 1, +20 focus in conversations)
- **XP Benefit**: 1 XP per card played

**Pilgrim** (Level 1)  
- Location: Market Square entrance
- Personality: DEVOTED (failures hurt twice)
- Conversation Types:
  - Friendly Chat: Blessing (+1 starting rapport with Devoted NPCs today)
  - Share Wisdom: Reveals one observation at current location
- **XP Benefit**: 1 XP per card played

### Midday - Copper Kettle Tavern

**Traveling Scholar** (Level 2)
- Location: Tavern common room
- Personality: STEADFAST
- Conversation Types:
  - Information Trade: Learn about stat-gated paths (5 rapport)
  - Academic Debate: 5 coins + temporary Insight +1 for day (10 rapport)
- **XP Benefit**: 2 XP per card played

### Afternoon - Various Locations

**Foreign Merchant** (Level 3)
- Location: Market Square
- Personality: MERCANTILE (high focus cards +30% success)  
- Conversation Types:
  - Trade Negotiation: Exotic goods worth 8 coins (10 rapport)
  - Partnership: Reduces all exchange costs by 1 coin today (15 rapport)
- **XP Benefit**: 3 XP per card played
- **Risk**: -10% success on all cards

## Locations and Investigation

### Market Square

**Standard Investigation**: 1 segment for base familiarity
- Morning (Quiet): +2 familiarity
- Other times (Busy): +1 familiarity

**Stat-Gated Approaches** (Require Level 2+):
- **Systematic Observation** (Insight 2+): +1 additional familiarity
- **Local Inquiry** (Rapport 2+): Learn which NPCs want observations
- **Purchase Information** (Commerce 2+): 2 coins per familiarity level
- **Note**: Authority and Cunning approaches not useful here

**Observation Rewards**:
- Familiarity 1: "Safe Passage Knowledge" (Insight card for Elena)
- Familiarity 2: "Merchant Caravan Route" (Commerce card for Marcus)

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
- **Focus**: 0
- **Difficulty**: Very Easy (85%)
- **Effect**: Advances Elena from DISCONNECTED to NEUTRAL
- **Critical**: Required for Elena's request to be reachable

### Merchant Caravan Route  
- **Stat**: Commerce
- **Focus**: 0
- **Difficulty**: Very Easy (85%)
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

**Merchant Road** (Commerce 2+)
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

**12:20 PM - Conversation with Marcus** (Segment 2)
- Level 2 conversation (2 XP per card)
- Deliver Viktor's package first
- Play high-focus cards for Mercantile bonus
- Use both observation cards
- Achieve Enhanced goal (10 rapport)
- Receive: 8 coins, 2 Commerce tokens
- Accept trade letter (weight 1)
- XP gained: ~10-14 to various stats

**12:40 PM - Quick Travel** (Segment 3)
- To Warehouse District: 1 segment
- Deliver Marcus's letter: +7 coins (total: 15)

**Optional: 1:00 PM - Traveling Scholar** (Segment 4)
- Level 2 stranger at Copper Kettle
- Could gain coins or Insight boost
- Cost: 1 segment (risky with time)

**1:00 PM - Return to Market Square** (Segment 4)
- 1 segment travel

### Afternoon Block (2:00 PM - 6:00 PM)

**2:00 PM - Final Marcus Preparation** (Segment 1)
- Buy food if needed (-2 coins, -50 hunger)
- Unlock caravan with 2 Commerce tokens
- Buy caravan passage (-10 coins, have 5 remaining)

**2:20 PM - Travel to Copper Kettle** (Segment 2)

**2:40 PM - Critical Elena Conversation** (Segment 3)
- Level 3 conversation (3 XP per card)
- Starting: DISCONNECTED (3 focus)
- Play Safe Passage Knowledge → NEUTRAL (5 focus)
- Devoted personality: failures hurt double
- Carefully build to 5 rapport
- Accept basic letter (weight 1)
- XP gained: ~15-20 to various stats

**3:00 PM - Caravan to Noble Quarter** (Segment 4)
- Use pre-purchased caravan passage
- Instant travel via caravan
- Deliver letter to Lord Blackwood
- **SUCCESS** with 1 hour buffer

## Alternative Approaches by Build

### Insight Build (If Insight reaches Level 2)
- Morning: Use Systematic Observation for +3 familiarity total
- Unlock both observations in one investigation
- Save 1 segment for stranger encounter
- Take Scholar's Shortcut to Noble Quarter (1 segment saved)

### Rapport Build (If Rapport reaches Level 2)
- Use Local Inquiry to learn NPC needs directly
- Better success rates with Elena (Devoted personality)
- Take Local's Path from Tavern to Noble Quarter
- Could attempt Elena's Enhanced goal more safely

### Authority Build (If Authority reaches Level 2)
- Skip permit/caravan entirely
- Use Noble's Gate for direct access (1 segment)
- More coins available for resources
- Authority cards risky with Elena's Devoted rule

### Commerce Build (Natural from Marcus conversation)
- Marcus conversation likely pushes Commerce toward Level 2
- All Commerce cards gain +10% success
- Better Marcus conversation outcomes
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
- DISCONNECTED state: only 3 focus capacity
- Cannot reach 5-focus request card
- Safe Passage Knowledge essential

#### Poor Stat Development
- Playing random cards instead of focusing a stat
- Missing Level 2 threshold benefits
- Not utilizing personality bonuses

#### Accept Heavy Obligations
- Marcus offers 5-weight silk delivery
- Would max out carrying capacity
- Cannot accept Elena's letter

### Why The Path Works

1. **Investigation timing**: Morning quiet bonus maximized
2. **Conversation order**: Marcus before Elena for resources
3. **Stat development**: Natural Commerce growth from Marcus
4. **Resource management**: Coins for caravan, food if needed
5. **Weight management**: Never exceed capacity
6. **Time efficiency**: 9 segments used, 4 buffer remaining

## System Reference

### Stat Progression
- **Level 1**: Base abilities
- **Level 2**: +10% success, unlock investigation approaches and special paths
- **Level 3**: Cards gain Persistent, more approaches unlock
- **Level 4**: +20% success, powerful investigation abilities
- **Level 5**: Never force LISTEN on failure

### XP Accumulation
- Every card played grants XP to its bound stat
- Base: 1 XP per card (Level 1 conversation)
- Scaled: 2 XP (Level 2), 3 XP (Level 3)
- Success and failure both grant XP

### Conversation Difficulty
- **Level 1**: +10% success all cards, 1 XP per card
- **Level 2**: Normal success rates, 2 XP per card
- **Level 3**: -10% success all cards, 3 XP per card

### Investigation Approaches by Stat
- **Insight**: Systematic patterns and connections
- **Rapport**: Social information networks
- **Authority**: Forced access to restricted areas
- **Commerce**: Purchase information directly
- **Cunning**: Covert investigation without detection

### Personality Rules
- **Devoted**: Rapport losses doubled
- **Mercantile**: Highest focus +30% success
- **Proud**: Ascending focus order required
- **Cunning**: Same focus as previous -2 rapport
- **Steadfast**: Rapport changes capped at ±2

## Conclusion

This POC demonstrates the complete Wayfarer experience:

1. **Conversations drive everything**: Every card played develops your character
2. **Stats are methodologies**: Not numbers but problem-solving approaches
3. **Strangers provide options**: Optional grinding for resources and XP
4. **Stats gate content**: Higher stats unlock new solutions
5. **Multiple builds work**: Time-optimal path exists but alternatives viable
6. **Every choice matters**: Card selection shapes your character's growth
7. **Personality rules transform gameplay**: Same deck, different puzzle

The scenario takes 30-45 minutes and teaches all core mechanics. Players finish understanding that success requires both tactical card play and strategic character development, with the stat system providing meaningful progression from the very first conversation.