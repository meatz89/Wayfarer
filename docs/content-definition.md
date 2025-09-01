# Wayfarer: Elena's Letter - Demo Scenario

## Scenario Overview

Elena needs her urgent letter delivered to Lord Blackwood before he leaves the city at sunset (5 PM). She faces a forced marriage to him and this letter is her refusal. The challenge: Lord Blackwood is behind the Noble District checkpoint requiring an access permit, you have existing obligations in queue, and resources are limited.

This scenario demonstrates all three core game loops working together to create emergent tactical puzzles.

## Core Mechanical Principles

### Strict Effect Separation
- Each card has ONE effect (fixed or scaling)
- No cards do multiple things
- Perfect information - all effects visible

### Strategic Layers
- **Weight Pool Management**: Capacity 3-6, persists until refreshed
- **Comfort Building**: Battery system (-3 to +3) triggers state transitions
- **Token Investment**: Linear +5% per token, only from deliveries
- **Queue Management**: Position 1 must complete first, multiple obligations compete
- **Atmosphere Control**: Persistent effects shape entire conversations

## Starting Configuration

### Player Resources
- **Coins**: 10 (exactly enough for checkpoint bribe if no permit)
- **Health**: 75/100 (no weight penalty yet)
- **Hunger**: 60/100 (reducing attention by 2)
- **Attention**: 8/10 (after morning calculation: 10 - 2)
- **Satchel**: Empty (5 slots max)

### Starting Queue (Position 1 MUST complete first!)
1. Marcus Package - 5hr deadline - 8 coins payment
2. Guard Report - 8hr deadline - 5 coins payment  
3. [Elena's letter will compete for position]

### Starting Tokens (Matching Type Bonuses Only)
- **Elena**: 1 Trust (+5% on her Trust-type cards only)
- **Marcus**: 2 Commerce (+10% on his Commerce-type cards only)
- **Guard Captain**: 1 Shadow (+5% on his Shadow-type cards only)
- **Lord Blackwood**: 0 all types

### Time Management
- **Current**: Tuesday 9:00 AM (Morning period)
- **Lord Blackwood Departs**: 5:00 PM (Evening period)
- **Maximum Actions**: ~15 with perfect efficiency

## World Map

### Location Hierarchy
Region → District → Location → Spot

Lower Wards → Market District → Central Square → Fountain

### Market Square
**Fountain** (Crossroads, Public):
- Travel hub to all districts
- Public: -1 patience in conversations
- Observations available per time period
- Town Crier present Midday/Evening

**Merchant Row** (Commercial):
- Marcus available (Morning-Evening)
- Work action: 2 attention → 8 coins + 4 hours
- Quick exchanges available

**Guard Post** (Authority, Tense):
- Guard Captain always present
- Day shift: Tense atmosphere default
- Night shift: Neutral atmosphere default

**North Alcove** (Discrete):
- Hidden from authority
- Special encounters at night

### Copper Kettle Tavern
**Common Room** (Crossroads, Public, Hospitality):
- Travel hub for tavern district
- Public: -1 patience
- Can access rest exchanges

**Corner Table** (Private):
- Elena always available (Afternoon-Evening only)
- Private: +1 patience
- Quiet atmosphere for deep conversations

**The Bar** (Commercial, Hospitality):
- Bertram always available
- Work action available
- Rest exchanges via Bertram's deck

**Hearthside** (Warm):
- +2 patience in conversations
- Special encounters evening

### Noble District Gate
**Checkpoint** (Crossroads, Authority):
- Requires: 10 coin bribe OR Access Permit
- Guards inspect all travelers
- Direct route to Lord Blackwood
- Tense atmosphere

**Side Path** (Discrete):
- Alternative entry (requires Shadow knowledge)
- Avoids checkpoint inspection

### Lord Blackwood's Manor
**Study**:
- Lord Blackwood present until 5 PM
- Deliver letter here
- Proud personality type

### Travel Routes
From Market Square Fountain:
- → Copper Kettle: 15 minutes, free
- → Noble District Gate: 25 minutes, free
- → Warehouse District: 30 minutes, free
- → Temple District: 20 minutes, free

From Noble District Gate:
- → Blackwood Manor: 10 minutes, requires permit

From Copper Kettle Common Room:
- → Market Square: 15 minutes, free
- → Temple District: 20 minutes, free

## NPC Complete Configurations

### Elena - The Letter Sender

**Mechanical Identity**:
- Type: Devoted (15 base patience)
- Location: Corner Table (+1 patience = 16 total)
- Available: Afternoon-Evening only (2 PM - 10 PM)
- Starting State: Desperate (forced marriage situation)

**Conversation Deck** (20 cards):

**Fixed Comfort Cards** (6 total):
- "I understand" (Trust-type, W1, Easy 70%): +1 comfort
- "Let me help" (Trust-type, W1, Easy 70%): +1 comfort  
- "You're safe with me" (Trust-type, W2, Medium 60%): +2 comfort
- "Trust in our bond" (Trust-type, W2, Medium 60%): +2 comfort
- "Together we're strong" (Trust-type, W3, Medium 60%): +3 comfort
- "Soul connection" (Trust-type, W5, Very Hard 40%, Fleeting): +5 comfort

**Scaled Comfort Cards** (4 total, all Trust-type):
- "Our trust runs deep" (Trust-type, W2, Hard 50%): +X comfort where X = Trust tokens
- "Remember our history" (Trust-type, W3, Hard 50%): +X comfort where X = Trust tokens
- "Lean on me" (Trust-type, W3, Hard 50%): +X comfort where X = 4 - current comfort
- "Crisis shared" (Trust-type, W4, Hard 50%, Fleeting): +X comfort where X = patience ÷ 3

**Utility Cards** (4 total, all Trust-type):
- "Let me think" (Trust-type, W1, Medium 60%): Draw 1 card
- "Consider options" (Trust-type, W1, Medium 60%): Draw 1 card
- "Gather strength" (Trust-type, W2, Medium 60%): Add 1 weight to pool
- "Deep breath" (Trust-type, W2, Medium 60%): Add 1 weight to pool

**Setup Cards** (3 total, 0 weight, mixed types):
- "Careful approach" (Trust-type, W0, Easy 70%): No effect, Atmosphere: Prepared
- "Open my heart" (Commerce-type, W0, Easy 70%): No effect, Atmosphere: Receptive
- "This is critical" (Status-type, W0, Easy 70%): No effect, Atmosphere: Final

**Dramatic Cards** (2 total, fleeting, Trust-type):
- "Desperate plea" (Trust-type, W4, Hard 50%, Fleeting): +4 comfort, Atmosphere: Volatile
- "All or nothing" (Trust-type, W6, Very Hard 40%, Fleeting): +5 comfort, Atmosphere: Final

**Flex Slot** (1, Trust-type):
- "Shared pain" (Trust-type, W2, Medium 60%): -2 comfort (represents emotional overflow)

**Goal Deck** (Separate from conversation deck):

- **"Crisis Refusal"** (Trust Letter)
  - Weight: 5
  - Difficulty: Very Hard (40% base + Trust tokens × 5%)
  - Success Terms: 4hr deadline, position 3, 10 coins
  - Failure Terms: 1hr deadline, position 1, 5 coins
  - Has "Final Word" property

- **"Formal Refusal"** (Trust Letter)
  - Weight: 6
  - Difficulty: Very Hard (40% base + Trust tokens × 5%)
  - Success Terms: 6hr deadline, lowest available, 15 coins
  - Failure Terms: 3hr deadline, position 2, 10 coins
  - Has "Final Word" property

- **"Personal Letter"** (Trust Letter)
  - Weight: 5
  - Difficulty: Very Hard (40% base + Trust tokens × 5%)
  - Success Terms: 8hr deadline, flexible position, 20 coins
  - Failure Terms: 4hr deadline, position 3, 15 coins
  - Has "Final Word" property

- **"Clear the Air"** (Resolution Goal)
  - Weight: 5
  - Difficulty: Hard (50% base + Trust tokens × 5%)
  - Effect: Remove burden cards from relationship record
  - Has "Final Word" property

**Relationship Record**:
- 2 burden cards from past failure (visible marker of damaged trust)

### Marcus - The Merchant

**Mechanical Identity**:
- Type: Mercantile (12 base patience)
- Location: Merchant Row
- Available: Morning-Evening (6 AM - 10 PM, shop hours)
- Starting State: Neutral (commerce-minded)

**Conversation Deck** (20 cards):

**Fixed Comfort Cards** (6, mostly Commerce-type):
- 3 at W1 (Commerce-type, Easy 70%): +1 comfort each
- 2 at W2 (Commerce-type, Medium 60%): +2 comfort each
- 1 at W3 (Status-type, Medium 60%): +3 comfort

**Scaled Comfort Cards** (4, all Commerce-type):
- 2 "Good business" (Commerce-type, W2, Hard 50%): +X where X = Commerce tokens
- 1 "Profitable relationship" (Commerce-type, W3, Hard 50%): +X where X = Commerce tokens
- 1 "Time is money" (Commerce-type, W3, Hard 50%): +X where X = weight remaining

**Utility Cards** (4, Commerce-type):
- 2 Draw cards (Commerce-type, W1, Medium 60%)
- 2 Weight-add cards (Commerce-type, W2, Medium 60%)

**Setup Cards** (3, W0, mixed types):
- "Let's negotiate" (Commerce-type, Easy 70%): Atmosphere: Focused
- "Time for business" (Trust-type, Easy 70%): Atmosphere: Patient
- "High stakes" (Shadow-type, Easy 70%): Atmosphere: Final

**Dramatic Cards** (2, fleeting, Commerce-type):
- "Deal of lifetime" (Commerce-type, W5, Very Hard 40%): +5 comfort
- "All in" (Commerce-type, W4, Hard 50%): +4 comfort, Atmosphere: Volatile

**Flex**: 1 negative comfort card (Commerce-type)

**Goal Deck**:
- "Package Delivery" (Commerce Promise, W5, Very Hard 40%)
- "Noble Permit Sale" (Commerce Promise, W6, Very Hard 40%)

**Exchange Deck** (Quick Trade Options, 0 attention):
- "Buy Provisions": 3 coins → Hunger = 0
- "Purchase Medicine": 5 coins → Health +20
- "Buy Access Permit": 15 coins → Noble District Permit
- "Accept Quick Job": Accept → New obligation (8 coins, 3hr deadline)
- "Trade Information": Give observation card → Alternative route knowledge

### Guard Captain - The Gatekeeper

**Mechanical Identity**:
- Type: Steadfast (13 base patience)
- Location: Guard Post
- Available: Always
- Default Atmosphere: Tense (day) / Neutral (night)

**Conversation Deck** (20 cards):

**Fixed Comfort Cards** (6, balanced types):
- 2 at W1 (Status-type, Easy 70%): +1 comfort each
- 2 at W1 (Shadow-type, Easy 70%): +1 comfort each
- 1 at W2 (Status-type, Medium 60%): +2 comfort
- 1 at W3 (Shadow-type, Medium 60%): +3 comfort

**Scaled Comfort Cards** (4, mixed):
- 2 scaling with Status tokens (Status-type, W2, Hard 50%)
- 2 scaling with Shadow tokens (Shadow-type, W3, Hard 50%)

**Utility Cards** (4, mixed types): 
- 2 Draw cards (1 Status-type, 1 Shadow-type, W1, Medium 60%)
- 2 Weight-add cards (1 Status-type, 1 Shadow-type, W2, Medium 60%)

**Setup Cards** (3, W0, mixed types):
- "Official business" (Status-type, Easy 70%): Atmosphere: Volatile
- "By the book" (Trust-type, Easy 70%): Atmosphere: Prepared
- "Under scrutiny" (Shadow-type, Easy 70%): Atmosphere: Pressured

**Dramatic Cards** (2, mixed types): 
- "Authority demonstrated" (Status-type, W4, Hard 50%, Fleeting): +4 comfort
- "Secrets revealed" (Shadow-type, W5, Very Hard 40%, Fleeting): +5 comfort

**Flex**: 1 Authority-themed card (Status-type)

**Goal Deck**:
- "Checkpoint Pass" (Shadow Promise, W5, Hard 50%)
  - Success: 24hr access permit, no cost
  - Failure: 2hr access permit, 5 coin fee

### Bertram - The Innkeeper

**Mechanical Identity**:
- Type: Mercantile (no conversation deck needed)
- Location: The Bar
- Available: Always (lives upstairs)
- Pure exchange NPC

**Exchange Deck** (Rest & Recovery):
- "Quick Meal": 2 coins → Hunger = 0
- "Short Rest": 2 coins → +3 attention, advance 2 hours
- "Full Night": 5 coins → Morning refresh (loses entire day!)
- "Noble Gossip": 3 coins → Observation card for player deck
- "Packed Lunch": 4 coins → Item: Reset hunger when used

### Lord Blackwood - The Recipient

**Mechanical Identity**:
- Type: Proud (10 base patience)
- Location: Manor Study
- Available: Until 5 PM (then leaves city)
- State: Neutral

**Special Rules**:
- Receives Elena's letter
- Cannot converse (just delivery)
- Proud personality affects queue negotiations

## Player Observation Deck System

### Building Your Deck
The player maintains their own observation deck (max 20 cards):
- Cost: 1 attention at specific locations
- Weight: 1 (minimal requirement)
- Success rate: 85% (Very Easy)
- Always persistent
- Expiration: 24-48 hours

### Location-Based Observations

**Market Square - Morning**:
- "Guard Routes": Set Pressured atmosphere (expires 24hr)

**Market Square - Afternoon**:
- "Market Gossip": Set Receptive atmosphere (expires 24hr)

**Market Square - Evening**:
- "Night Paths": Next SPEAK costs 0 weight (expires 12hr)

**Copper Kettle - When Elena Present**:
- "Shared Hardship": Set Informed atmosphere (expires 48hr, powerful!)

**Guard Post - Night**:
- "Bribery Option": Comfort = 0 (expires 6hr, emergency reset)

### Conversation-Generated Observations
NPCs can reward observation cards:
- "Noble Routes": Next action costs 0 patience
- "Guard Timing": Set Synchronized atmosphere
- "Hidden Path": Weight pool = maximum

## Emotional State Effects

### State Transitions and Weight Capacity

**DESPERATE** (Elena's starting state):
- Weight capacity: 3
- Cards drawn: 1
- Comfort: +3→Tense (escape!), -3→Conversation ends
- Goals Available: Crisis promises, urgent letters

**TENSE** (Cautious):
- Weight capacity: 4
- Cards drawn: 2
- Comfort: +3→Neutral, -3→Desperate
- Goals Available: Shadow promises, burden resolution

**NEUTRAL** (Balanced):
- Weight capacity: 5
- Cards drawn: 2
- Comfort: +3→Open, -3→Tense
- Goals Available: Commerce promises, routine letters

**OPEN** (Receptive):
- Weight capacity: 5
- Cards drawn: 3
- Comfort: +3→Connected, -3→Neutral
- Goals Available: Trust promises, personal requests

**CONNECTED** (Deep Bond):
- Weight capacity: 6
- Cards drawn: 3
- Comfort: +3→Stays Connected (maxed), -3→Open
- Goals Available: All promise types

## Strategic Decision Framework

### Weight Pool Navigation

**Elena's Challenge in Desperate (3 capacity)**:
- Can play three W1 cards before refresh
- Can play one W3 card then need LISTEN
- Cannot play W4+ cards without Prepared atmosphere
- Cannot play W5 goal cards without state change

**Reaching Goal Cards**:
- Need 5+ weight capacity (Open/Connected states)
- OR use Prepared atmosphere (+1 capacity)
- OR use observation to set Informed (auto-success)

### Comfort Building Mathematics

**Turn Economy with 16 Patience**:
- Comfort starts at 0
- Need +3 to transition states
- Desperate → Tense → Neutral → Open (9 comfort total)

**Build Rates with 1 Trust Token (45-75% success)**:
- W1 cards: +1 comfort at 75% (Easy + token)
- W2 cards: +2 comfort at 65% (Medium + token)
- W3 cards: +3 comfort at 65% (Medium + token)
- W5 cards: +5 comfort at 45% (Very Hard + token)

**Scaled Options**:
- Trust scaling: +1 comfort (only 1 token)
- Comfort scaling: +4 when at 0 comfort
- Patience scaling: +5 comfort (16 patience ÷ 3)

### Token Economics

**Token-Type Matching (cards only boosted by matching tokens)**:
- Trust tokens: +5% only on Trust-type cards
- Commerce tokens: +5% only on Commerce-type cards
- Status tokens: +5% only on Status-type cards
- Shadow tokens: +5% only on Shadow-type cards

**Elena's Deck with 1 Trust Token**:
- ~17 Trust-type cards: Get +5% bonus (45-75% success)
- ~3 non-Trust cards: Get +0% bonus (40-70% success)

**Letter Negotiation with Elena's Trust Goal**:
- Crisis Letter (Trust-type): 45% success (40% + 1 Trust × 5%)
- If you had Commerce tokens instead: Still 40% (no bonus!)

**Building the Right Tokens**:
- Delivering Trust letters → Trust tokens with recipient
- Delivering Commerce letters → Commerce tokens with recipient
- Must match NPC personality for effectiveness

## Queue Management Strategies

### Position Negotiation Outcomes

**Elena's Letter Negotiation**:
- Crisis Letter: 45% chance of 4hr/position 3 vs 1hr/position 1
- Formal Letter: 45% chance of 6hr/flexible vs 3hr/position 2
- Personal Letter: 45% chance of 8hr/flexible vs 4hr/position 3

### Displacement Calculations

Starting queue:
1. Marcus Package (5hr, 8 coins)
2. Guard Report (8hr, 5 coins)
3. Elena's Letter (negotiated position)

To deliver Elena immediately from position 3:
- Displace Marcus: -2 Commerce tokens, +2 burden cards
- Displace Guard: -1 Shadow token, +1 burden card
- Total cost: 3 tokens burned, 3 burden cards added

### Strategic Queue Timing
- Complete other obligations first if time allows
- Accept poor position to preserve tokens
- Displace only for critical deadlines

## Multiple Solution Paths

### Path A: Morning Efficiency
**Morning** (8 attention, 10 coins, 60 hunger):
1. Work at Merchant Row (-2 att, +8 coins, →Midday)
2. Exchange: Buy food (-3 coins, hunger→0)
3. Observe "Guard Routes" (-1 att, gain Pressured atmosphere card)

**Midday** (5 attention, 15 coins, 0 hunger):
4. Complete Marcus delivery (-2 Commerce for displacement)
5. Wait to Afternoon (preserve attention)

**Afternoon** (5 attention, 15 coins):
6. Observe "Shared Hardship" at Copper Kettle (-1 att, Informed atmosphere)
7. Converse with Elena (-2 att, 16 patience)
8. Use observation: Set Informed atmosphere (next card auto-succeeds)
9. Play high comfort card with guaranteed success
10. Build to Open state, access goal cards

**Results**: Good terms, queue cleared, profitable

### Path B: Weight Management
**Morning**:
1. Exchange: Buy food immediately (-3 coins)
2. Wait to Afternoon (preserve 8 attention)

**Afternoon** (8 attention, 7 coins):
3. Full Elena conversation (-2 att)
4. Carefully manage 3 weight capacity in Desperate
5. Use setup cards (W0) for Prepared atmosphere
6. Now 4 capacity - still need state change
7. Focus on scaled comfort for efficiency

**Results**: Challenging but possible with good atmosphere use

### Path C: Crisis Management
**Morning**:
1. Skip all preparation
2. Rush to complete existing obligations

**Afternoon** (limited resources):
3. Elena conversation with whatever remains
4. Accept any available letter at poor odds
5. Heavy displacement if needed

**Evening**:
6. Use all coins for checkpoint
7. Deliver with minimal margin

**Results**: Mission complete but relationships damaged

### Path D: Guard Captain Route
**Morning**:
1. Cannot build Shadow tokens (no token cards exist)
2. Must rely on existing 1 Shadow token
3. Conversation for permit goal card

**Evening**:
4. Guard in Neutral atmosphere (better than Tense)
5. 45% chance for free permit (Hard + 1 Shadow)
6. Success: Free access to Noble District

**Results**: Risky but preserves resources

## Atmosphere Management

### Strategic Atmosphere Chains

**Setup for Success**:
1. Play "Careful approach" (W0) → Prepared atmosphere
2. Now have 4 weight capacity in Desperate (3+1)
3. Play multiple cards before refresh needed

**High Risk/Reward**:
1. Play "This is critical" (W0) → Final atmosphere
2. Any failure ends conversation
3. But can attempt dramatic plays

**Information Advantage**:
1. Use "Shared Hardship" observation → Informed atmosphere
2. Next card cannot fail
3. Guarantee critical comfort gain or goal play

## Resource Calculations

### Morning Refresh Formula
```
Base = 10 attention
Subtract (Hunger ÷ 25)
Minimum = 2
```
At 60 hunger: 10 - 2 = 8 attention

### Success Rate Examples
```
Trust-type W1 Easy card, 1 Trust token with Elena:
70% + (1 × 5%) = 75%

Commerce-type W1 Easy card, 1 Trust token with Elena:
70% + 0 = 70% (wrong token type!)

Trust-type W2 Hard scaled card, 1 Trust token:
50% + (1 × 5%) = 55%

Trust-type W5 Very Hard goal, 1 Trust token:
40% + (1 × 5%) = 45%

Commerce-type card with Marcus, 2 Commerce tokens:
Base rate + (2 × 5%) = +10% bonus

Shadow-type card with Guard, 1 Shadow token:
Base rate + (1 × 5%) = +5% bonus
```

## Failure Cascades

### Hard Failure
- 5 PM passes without delivery
- Elena permanently hostile
- -3 Trust tokens with Elena
- 3 burden cards added to relationship
- Cannot retry for 24 hours
- Lord Blackwood gone forever

### Soft Failures
- Poor negotiation: 1hr deadline creates panic
- Forced position 1: Must displace everything
- Token burning: Permanent relationship damage
- Resource depletion: Cannot afford checkpoint
- Fleeting goal discarded: Conversation fails

### Recovery Options
- Work for emergency coins (loses 4 hours)
- Rest for attention (costs coins)
- Displace obligations (burns tokens)
- Accept any terms (poor rewards)
- Use observations for emergency advantages

## Success Metrics

### Perfect Run (Master Strategist)
- "Personal Letter" obtained (Connected state)
- 8-hour deadline negotiated
- Position 5+ (no displacement)
- Complete by 3 PM
- Gain +3 Trust tokens from delivery
- 20+ coins earned
- No tokens burned

### Good Run (Competent Courier)
- "Formal Refusal" obtained (Open state)
- 4-6 hour deadline
- Reasonable position
- Complete by 4 PM
- Gain +2 Trust tokens
- Break even on coins

### Acceptable Run (Desperate Success)
- Any letter delivered
- Poor terms accepted
- Heavy displacement
- Before 5 PM deadline
- Mission complete

## Emergent Puzzle Variations

### Context Changes Everything

**Scenario 1**: Elena Desperate, You have 3 Trust tokens
- +15% success on all cards
- Trust scaling gives +3 comfort
- Can manage weight pool carefully

**Scenario 2**: Elena Open, You have 0 Trust tokens
- 5 weight capacity available
- Base success rates only
- Can play goal cards immediately

**Scenario 3**: Elena Neutral, 4 burden cards in record
- Standard capacity but damaged relationship
- Must persist through mistrust
- Patience becomes critical resource

**Scenario 4**: Queue full (8 obligations already)
- Elena's letter position 9
- Massive displacement required
- Token economy destroyed

Each combination creates unique tactical challenge.

## Long-term Consequences

### Deck Evolution
Successful delivery adds cards to Lord Blackwood's deck:
- Trust-scaling comfort cards
- Makes future Trust conversations easier
- Permanent world change

Failed delivery adds burdens to Elena's relationship:
- 3 burden cards in record
- Future conversations require resolution
- Relationship permanently scarred

### Token Economy
Successful delivery creates cascading benefits:
- +1-3 Trust tokens with Lord Blackwood
- Better future negotiations
- Easier Trust-scaling cards

Burned tokens create cascading damage:
- -1 token = -5% success forever
- +1 burden = damaged relationship
- Negative tokens = relationship debt

Twenty deliveries create twenty permanent changes.

## Core Innovation Summary

The scenario demonstrates elegant complexity through simple rules:
- **Weight Pools**: Persistent capacity creates multi-turn planning
- **Token Linearity**: Every token adds exactly 5% success
- **Atmosphere Persistence**: Environmental effects shape conversations
- **Comfort Battery**: ±3 triggers state transitions
- **Queue Displacement**: Permanent sacrifice for flexibility
- **Observation Effects**: Unique advantages from exploration

No thresholds (except comfort ±3), no hidden mechanics, no soft locks. Every mechanic serves one purpose while resources flow through multiple systems. The puzzle emerges from interaction, not complication.