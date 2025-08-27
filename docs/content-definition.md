# Wayfarer POC - Elena's Letter Scenario

## Scenario Overview

Elena needs her urgent letter delivered to Lord Blackwood before he leaves the city at sunset (5 PM). She faces a forced marriage to him and this letter is her refusal. The challenge: Lord Blackwood is behind the Noble District checkpoint, you start with existing obligations, and resources are limited.

## Core Mechanical Principles

### Strict Effect Separation
- Each card type has ONE effect pool
- No cards do multiple things
- No thresholds - linear progression only
- Perfect information - all effects visible

### Strategic Layers
- **Emotional State Navigation**: States filter drawable cards and modify token effectiveness
- **Comfort Building**: Depth access requires careful accumulation
- **Token Investment**: Linear +5% per token, also displacement currency
- **Queue Management**: Position 1 must complete first, multiple obligations compete

## Starting Configuration

### Player Resources
- **Coins**: 10 (exactly enough for checkpoint bribe)
- **Health**: 75/100 (no weight penalty yet)
- **Hunger**: 60/100 (reducing attention by 2)
- **Attention**: 8/10 (after morning calculation)
- **Satchel**: Empty (5 slots max)

### Starting Queue (Position 1 MUST complete first!)
1. Marcus Package - 5hr deadline - 8 coins payment
2. Guard Report - 8hr deadline - 5 coins payment
3. [Elena's letter will compete for position]

### Starting Tokens (Linear Bonuses)
- **Trust**: 1 with Elena (+5%, becomes +10% in Desperate state)
- **Commerce**: 2 with Marcus (+10%)
- **Status**: 0 with all (+0%)
- **Shadow**: 1 with Guard Captain (+5%)

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
- Day shift: Tense state (until 6 PM)
- Night shift: Neutral state (after 6 PM)

**North Alcove** (Discrete):
- Hidden from authority
- Special encounters at night

### Copper Kettle Tavern
**Common Room** (Crossroads, Public, Hospitality):
- Travel hub for tavern district
- Public: -1 patience
- Can access rest exchanges

**Corner Table** (Private):
- Elena always available
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

**Comfort Cards** (8 total):
- 4 cards at W1/various depths: +2 comfort, 65% base
- 3 cards at W2/various depths: +4 comfort, 55% base
- 1 card at W3/depth 14: +6 comfort, 45% base

**Token Cards** (4 total):
- "Build Trust" (D5/W1): +1 Trust token, 65% base
- "Prove Reliability" (D8/W2): +1 Trust token, 55% base
- "Deep Connection" (D10/W2): +1 Trust token, 55% base
- "Sacred Promise" (D15/W3): +1 Trust token, 45% base

**State Cards** (4 total, all W1 for accessibility):
- "Calm Reassurance" (D3): Desperate→Tense, 65% base
- "Find Balance" (D7): Tense→Neutral, 65% base
- "Open Hearts" (D11): Neutral→Open, 65% base
- "Soul Bond" (D16): Open→Connected, 65% base

**Knowledge Cards** (2 total):
- "Share Route" (D6): Creates "Noble Routes" observation
- "Guard Info" (D9): Creates "Guard Timing" observation

**Burden Cards** (2 starting - past failure):
- "Broken Promise" (D1/W2): Remove on success, 55% base
- "Lost Faith" (D3/W2): Remove on success, 55% base

**Letter Deck** (State + Depth Requirements):

- **"Crisis Refusal"** (D3/Trust)
  - States: Desperate, Tense
  - Success: 4hr deadline, position 3, 10 coins
  - Failure: 1hr deadline, position 1, 5 coins

- **"Formal Refusal"** (D7/Trust)
  - States: Neutral, Open
  - Success: 6hr deadline, position 4, 15 coins
  - Failure: 3hr deadline, position 2, 10 coins

- **"Personal Letter"** (D10/Trust)
  - States: Open, Connected
  - Success: 8hr deadline, flexible position, 20 coins
  - Failure: 4hr deadline, position 3, 15 coins

**Crisis Deck**:
- "Everything Falls Apart" (D0/W5→0 in Desperate)
  - Injected during Desperate LISTEN
  - Success (40% + tokens): Crisis removed, +1 Trust
  - Failure (60%): +1 burden, conversation continues

### Marcus - The Merchant

**Mechanical Identity**:
- Type: Mercantile (12 base patience)
- Location: Merchant Row
- Available: Morning-Evening (6 AM - 10 PM, shop hours)
- Starting State: Neutral (Morning-Afternoon), Eager (Evening)

**Conversation Deck** (15 cards):
- 10 Commerce comfort cards (various depths/weights)
- 2 Token cards: Commerce (D6), Shadow (D10)
- 2 State cards: Neutral→Eager (D4), Any→Neutral (D8)
- 1 Knowledge card: Creates "Hidden Path" observation (D8)

**Letter Deck**:
- "Delivery Contract" (D6/Commerce)
  - States: Eager, Neutral
  - Success: 6hr deadline, position 3, 8 coins
  - Failure: 3hr deadline, position 2, 8 coins

- "Noble Permit Sale" (D4/Commerce)
  - States: Eager only
  - Success: Pay 12 coins, get permit
  - Failure: Pay 15 coins, get permit

**Exchange Deck** (Quick Trade Options):
- "Buy Provisions": 3 coins → Hunger = 0
- "Purchase Medicine": 5 coins → Health +20
- "Buy Access Permit": 15 coins → Noble District Permit
- "Accept Quick Job": Accept → New obligation (8 coins, 3hr)
- "Trade Information": 1 Shadow token → Alternative route knowledge

### Guard Captain - The Gatekeeper

**Mechanical Identity**:
- Type: Steadfast (13 base patience)
- Location: Guard Post
- Available: Always
- State: Tense (day shift until 6 PM) / Neutral (night shift after 6 PM)

**Conversation Deck** (12 cards):
- 6 Status comfort cards
- 3 Shadow comfort cards
- 1 Token card: Shadow (D8)
- 1 State card: Tense→Neutral (D4)
- 1 Knowledge card: Creates "Patrol Schedule" observation

**Letter Deck**:
- "Checkpoint Pass" (D8/Shadow)
  - States: Neutral only
  - Success: 24hr access permit, no cost
  - Failure: 2hr access permit, 5 coin fee

### Bertram - The Innkeeper

**Mechanical Identity**:
- Type: Mercantile (no conversation deck)
- Location: The Bar
- Available: Always (lives upstairs)
- Pure exchange NPC

**Exchange Deck** (Rest & Recovery):
- "Quick Meal": 2 coins → Hunger = 0
- "Short Rest": 2 coins → +3 attention, advance 2 hours
- "Full Night": 5 coins → Morning refresh (loses entire day!)
- "Noble Gossip": 3 coins → "Noble Schedule" observation
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

## Observation System

### Dynamic Knowledge System
Observations are state change cards with expiration:
- Cost: 1 attention at specific locations
- Effect: State change card added to hand
- Weight: 1, Success: 70%
- Expiration: 24-48 hours (no decay)

### Location-Based Observations

**Market Square - Morning**:
- "Guard Routes" → Any state to Tense (expires 24hr)

**Market Square - Afternoon**:
- "Market Gossip" → Tense to Eager (expires 24hr)

**Market Square - Evening**:
- "Night Paths" → Any to Shadow-compatible state (expires 12hr)

**Copper Kettle - When Elena Present**:
- "Shared Hardship" → Desperate to Open (expires 48hr, powerful!)

**Guard Post - Night**:
- "Bribery Option" → Hostile to Neutral (expires 6hr)

### Conversation-Generated Observations
Knowledge cards in NPC decks can create:
- "Noble Routes" → Neutral to Open (travel context)
- "Guard Timing" → Tense to Neutral (authority context)
- "Hidden Path" → Any to Eager (commerce context)

## Emotional State Effects

### State Filtering and Modifications

**DESPERATE** (Elena's starting state):
- Weight limit: 1
- Draws: Trust/Crisis + 1 guaranteed state card
- Tokens: ×2 effectiveness (Trust gives +10% each)
- Letters: Trust/Crisis types available
- Crisis cards inject on LISTEN

**TENSE** (Cautious):
- Weight limit: 2
- Draws: Shadow cards + 1 guaranteed state card
- Special: Observation cards cost 0 weight
- Letters: Shadow types available

**NEUTRAL** (Balanced):
- Weight limit: 3
- Draws: All types equally + 1 guaranteed state card
- No modifications
- Letters: Commerce types available

**OPEN** (Receptive):
- Weight limit: 3
- Draws: Trust/Token cards + 1 guaranteed state card
- Comfort: All successes gain +1 bonus
- Letters: Trust types available

**EAGER** (Excited):
- Weight limit: 3
- Draws: Commerce/Token cards + 1 guaranteed state card
- Tokens: Commerce ×3 effectiveness
- Letters: Commerce with +2 coin bonus

**CONNECTED** (Deep Bond):
- Weight limit: 4
- Draws: 60% Token, 40% any + 1 guaranteed state card
- Tokens: All types ×2 effectiveness
- Letters: All types available

## Strategic Decision Framework

### Emotional State Navigation Paths

**Elena's Optimal Journey**:
1. **Desperate** → Use observation for direct jump to Open
2. **Desperate** → Tense → Neutral → Open (traditional path)
3. **Desperate** → Stay for token multiplication

Each path has trade-offs:
- Direct jump costs observation but saves turns
- Traditional path reliable but slow
- Staying exploits token bonus but faces crisis cards

### Comfort Building Mathematics

**Turn Economy with 16 Patience**:
- Comfort starts at 5
- Need comfort 3+ for Crisis Letter (already met)
- Need comfort 7+ for Formal Letter (need +2)
- Need comfort 10+ for Personal Letter (need +5)

**Build Rates**:
- W1 cards: +2 comfort at 65% + tokens
- W2 cards: +4 comfort at 55% + tokens
- W3 cards: +6 comfort at 45% + tokens

Perfect play: 2-3 turns to reach any letter depth

### Token Economics

**Linear Benefits**:
- 0 tokens: Base rates (45-65%)
- 2 tokens: +10% success (55-75%)
- 4 tokens: +20% success (65-85%)
- 6 tokens: +30% success (75-95%)

**State Multiplication**:
- 1 Trust in Desperate: +10% (doubled)
- 2 Commerce in Eager: +30% (tripled)
- 3 tokens in Connected: +30% (doubled)

**Displacement Costs**:
- Each token burned adds burden card
- Permanent relationship damage
- Future conversations harder

## Queue Management Strategies

### Position Negotiation Outcomes

**Elena's Letter Negotiation**:
- Crisis Letter: Attempts position 1 (Desperate personality)
- Formal Letter: Attempts lowest available
- Personal Letter: Flexible positioning

Success gives your terms, failure gives NPC terms.

### Displacement Calculations

Starting queue:
1. Marcus Package (5hr, 8 coins)
2. Guard Report (8hr, 5 coins)
3. Elena's Letter (negotiated position)

To deliver Elena immediately from position 3:
- Displace Marcus: -2 Commerce tokens, +2 burden cards
- Displace Guard: -1 Shadow token, +1 burden card
- Total cost: 3 tokens, 3 burden cards added

### Strategic Queue Timing
- Complete other obligations first if time allows
- Accept poor position to preserve tokens
- Displace only for critical deadlines

## Multiple Solution Paths

### Path A: Morning Efficiency
**Morning** (8 attention, 10 coins, 60 hunger):
1. Work at Merchant Row (-2 att, +8 coins, →Midday)
2. Exchange: Buy food (-3 coins, hunger→0)
3. Observe "Guard Routes" (-1 att, gain state card)

**Midday** (5 attention, 15 coins, 0 hunger):
4. Complete Marcus delivery (-2 Commerce for displacement)
5. Wait to Afternoon (preserve attention)

**Afternoon** (5 attention, 15 coins):
6. Observe "Shared Hardship" at Copper Kettle (-1 att)
7. Converse with Elena (-2 att, 16 patience)
8. Use observation: Desperate→Open directly
9. Build comfort with +1 bonus to 7+
10. Access "Formal Refusal"

**Results**: Good terms, queue cleared, profitable

### Path B: Token Investment
**Morning**:
1. Exchange: Buy food immediately (-3 coins)
2. Wait to Afternoon (preserve 8 attention)

**Afternoon** (8 attention, 7 coins):
3. Full Elena conversation (-2 att)
4. Stay Desperate for ×2 token bonus
5. Focus on token cards at 50% success
6. Build tokens to 3-4 Trust
7. Accept Crisis Letter quickly

**Results**: Poor letter terms but maximum tokens for future

### Path C: Crisis Management
**Morning**:
1. Skip all preparation
2. Rush to complete existing obligations

**Afternoon** (limited resources):
3. Elena conversation with whatever remains
4. Accept any available letter
5. Heavy displacement if needed

**Evening**:
6. Use all coins for checkpoint
7. Deliver with minimal margin

**Results**: Mission complete but relationships damaged

### Path D: Guard Captain Route
**Morning**:
1. Build Shadow tokens with Guard Captain
2. Wait for night shift (6 PM)

**Evening**:
3. Guard in Neutral state
4. Build comfort to D8
5. Get Checkpoint Pass letter
6. Free access to Noble District

**Results**: No bribe needed but very tight timing

## Work and Rest Options

### Work Actions
**Market Square - Merchant Row**:
- Available: Morning-Evening
- Cost: 2 attention
- Gain: 8 coins
- Time: +4 hours

**Copper Kettle - Bar**:
- Available: All day
- Cost: 2 attention
- Gain: 8 coins
- Time: +4 hours

### Rest Exchanges (Bertram)
- "Quick Meal": 2 coins → Hunger = 0
- "Short Rest": 2 coins → +3 attention, +2 hours
- "Full Night": 5 coins → Skip to morning (fails Elena!)

### Wait Actions
- Wait 30 minutes: Free, no cost
- Wait to next period: Free, strategic positioning
- Wait for NPC availability: Essential for Elena

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
W1 Trust card, 1 Trust token in Desperate:
65% + (1 × 2 × 5%) = 75%

W2 Commerce card, 2 Commerce in Eager:
55% + (2 × 3 × 5%) = 85%

W3 card, 0 tokens:
45% + 0 = 45%
```

## Failure Cascades

### Hard Failure
- 5 PM passes without delivery
- Elena→Hostile permanently
- -3 Trust tokens with Elena
- 3 burden cards added to her deck
- Cannot retry for 24 hours
- Lord Blackwood gone forever

### Soft Failures
- Poor negotiation: 1hr deadline creates panic
- Forced position 1: Must displace everything
- Token burning: Permanent relationship damage
- Resource depletion: Cannot afford checkpoint

### Recovery Options
- Work for emergency coins (loses 4 hours)
- Rest for attention (costs coins)
- Displace obligations (burns tokens)
- Accept any terms (poor rewards)

## Success Metrics

### Perfect Run (Master Strategist)
- "Personal Letter" obtained (D10)
- 8-hour deadline negotiated
- Position 5+ (no displacement)
- Complete by 3 PM
- +2 Trust tokens gained
- 20+ coins earned
- No tokens burned

### Good Run (Competent Courier)
- "Formal Refusal" obtained (D7)
- 4-6 hour deadline
- Reasonable position
- Complete by 4 PM
- Tokens maintained
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
- +30% success (15% base + 15% from ×2 tokens)
- Crisis manageable but still risky
- Can attempt aggressive comfort building

**Scenario 2**: Elena Open, You have -2 Trust tokens
- Weight 3 cards available
- -10% success penalty
- Must rely on high-weight cards

**Scenario 3**: Elena Neutral, 4 burden cards in deck
- Standard rules but diluted draws
- Must persist through bad hands
- Patience becomes critical resource

**Scenario 4**: Queue full (8 obligations already)
- Elena's letter position 9
- Massive displacement required
- Token economy destroyed

Each combination creates unique tactical challenge.

## Long-term Consequences

### Deck Evolution
Successful delivery adds cards to Lord Blackwood's deck:
- Trust comfort cards at low depths
- Makes future Trust conversations easier
- Permanent world change

Failed delivery adds burdens to Elena's deck:
- 3 burden cards clog her draws
- Future conversations much harder
- Relationship permanently scarred

### Token Economy
Burned tokens create cascading damage:
- -1 token = -5% success forever
- +1 burden = harder conversations
- Negative tokens = relationship debt

Twenty deliveries create twenty permanent changes.

## Core Innovation Summary

The scenario demonstrates elegant complexity through simple rules:
- **State Filtering**: Different moods enable different cards
- **Token Multiplication**: States modify resource effectiveness
- **One Card Per Turn**: Authentic conversation rhythm
- **Linear Progression**: Every token matters equally
- **Queue Displacement**: Permanent sacrifice for flexibility
- **Fleeting Cards**: Tactical hand management

No thresholds, no hidden mechanics, no soft locks. Every mechanic serves one purpose while resources flow through multiple systems. The puzzle emerges from interaction, not complication.