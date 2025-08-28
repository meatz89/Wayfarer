# Wayfarer: Elena's Letter - Demo Scenario

## Scenario Overview

Elena needs her urgent letter delivered to Lord Blackwood before he leaves the city at sunset (5 PM). She faces a forced marriage to him and this letter is her refusal. The challenge: Lord Blackwood is behind the Noble District checkpoint requiring an access permit, you have existing obligations in queue, and resources are limited.

This scenario demonstrates all three core game loops working together to create emergent tactical puzzles.

## Core Mechanical Principles

### Strict Effect Separation
- Each card type has ONE effect pool
- No cards do multiple things
- No thresholds - linear progression only
- Perfect information - all effects visible

### Strategic Layers
- **Emotional State Navigation**: States filter drawable cards only
- **Comfort Building**: Depth access requires careful accumulation within conversation
- **Token Investment**: Linear +5% per token, also displacement currency
- **Queue Management**: Position 1 must complete first, multiple obligations compete

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

### Starting Tokens (Linear Bonuses)
- **Elena**: 1 Trust (+5% success with her)
- **Marcus**: 2 Commerce (+10% success)
- **Guard Captain**: 1 Shadow (+5% success)
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

**Comfort Cards** (8 total):
- 4 cards at W1/various depths: +2 comfort, 65% base
  - "I understand" (D2)
  - "Let me help" (D3) 
  - "You're not alone" (D5)
  - "We'll solve this" (D7)
- 3 cards at W2/various depths: +4 comfort, 55% base
  - "Trust in me" (D4)
  - "I promise to help" (D6)
  - "Together we're strong" (D9)
- 1 card at W3/depth 14: +6 comfort, 45% base
  - "Soul connection" (D14)

**Token Cards** (4 total):
- "Build Trust" (D5/W1): +1 Trust token, 65% base
- "Prove Reliability" (D8/W2): +1 Trust token, 55% base
- "Deep Connection" (D10/W2): +1 Trust token, 55% base
- "Sacred Promise" (D15/W3): +1 Trust token, 45% base

**State Cards** (4 total, all W1 for accessibility, various depths):
- "Calm Reassurance" (D2): Desperate→Tense, 65% base
- "Find Balance" (D5): Tense→Neutral, 65% base
- "Open Hearts" (D8): Neutral→Open, 65% base
- "Soul Bond" (D14): Open→Connected, 65% base

**Knowledge Cards** (2 total):
- "Share Route" (D6): Creates "Noble Routes" observation
- "Guard Info" (D9): Creates "Guard Timing" observation

**Burden Cards** (2 starting - past failure):
- "Broken Promise" (D1/W2): Remove on success, 55% base
- "Lost Faith" (D3/W2): Remove on success, 55% base

**Letter Deck** (State + Token + Depth Requirements):

- **"Crisis Refusal"** (Trust Letter)
  - Required: 1+ Trust tokens ✓ (you have 1)
  - State Tag: [Desperate/Tense]
  - Depth: 3 (need comfort 3+ to draw)
  - Negotiation Success: 4hr deadline, position 3, 10 coins
  - Negotiation Failure: 1hr deadline, position 1, 5 coins

- **"Formal Refusal"** (Trust Letter)
  - Required: 2+ Trust tokens ✗ (need 1 more)
  - State Tag: [Neutral/Open]
  - Depth: 7 (need comfort 7+ to draw)
  - Negotiation Success: 6hr deadline, lowest available, 15 coins
  - Negotiation Failure: 3hr deadline, position 2, 10 coins

- **"Personal Letter"** (Trust Letter)
  - Required: 3+ Trust tokens ✗ (need 2 more)
  - State Tag: [Open/Connected]
  - Depth: 10 (need comfort 10+ to draw)
  - Negotiation Success: 8hr deadline, flexible position, 20 coins
  - Negotiation Failure: 4hr deadline, position 3, 15 coins

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

**Conversation Deck** (16 cards total):
- 10 Commerce comfort cards (various depths/weights)
  - 5 at W1: +2 comfort, 65% base
  - 4 at W2: +4 comfort, 55% base
  - 1 at W3: +8 comfort, 45% base
- 2 Token cards: Commerce (D6), Shadow (D10)
- 2 State cards: Neutral→Eager (D4), Any→Neutral (D8)
- 1 Knowledge card: Creates "Hidden Path" observation (D8)
- 1 Goal card (shuffled in based on conversation type)

**Possible Goal Cards** (one chosen per conversation):
- "Package Delivery" (Commerce Promise)
  - States: Eager, Neutral
  - Depth: 6
  - Base Success: 45% (+5% per Commerce token)
  - Success: 6hr deadline, position 3, 8 coins
  - Failure: 3hr deadline, position 2, 8 coins

- "Noble Permit Sale" (Commerce Promise)
  - States: Eager only
  - Depth: 4
  - Base Success: 50% (+5% per Commerce token)
  - Success: Pay 12 coins, get permit
  - Failure: Pay 15 coins, get permit

**Exchange Deck** (Quick Trade Options, 0 attention):
- "Buy Provisions": 3 coins → Hunger = 0
- "Purchase Medicine": 5 coins → Health +20
- "Buy Access Permit": 15 coins → Noble District Permit (you can't afford!)
- "Accept Quick Job": Accept → New obligation (8 coins, 3hr deadline)
- "Trade Information": 1 Shadow token → Alternative route knowledge

### Guard Captain - The Gatekeeper

**Mechanical Identity**:
- Type: Steadfast (13 base patience)
- Location: Guard Post
- Available: Always
- State: Tense (day shift until 6 PM) / Neutral (night shift after 6 PM)

**Conversation Deck** (13 cards total):
- 6 Status comfort cards
- 3 Shadow comfort cards  
- 1 Token card: Shadow (D8)
- 1 State card: Tense→Neutral (D4), Guarded→Open (D7)
- 1 Knowledge card: Creates "Patrol Schedule" observation
- 1 Goal card (conversation dependent)

**Goal Card**:
- "Checkpoint Pass" (Shadow Promise/Permit)
  - States: Neutral only
  - Depth: 8
  - Base Success: 35% (+5% per Shadow token)
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
- Weight: 0-2 (varies by observation)
- Success rate: 85%
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

## Emotional State Effects

### State Web and Momentum Effects

**DESPERATE** (Elena's starting state):
- Weight limit: 1
- Draws: Trust/Crisis + 1 guaranteed state card
- Crisis cards weight becomes 0
- Momentum: Each point reduces patience cost by 1 (min 0)
- Goals Available: Crisis promises, urgent letters
- Degradation: → Hostile at -3 momentum

**TENSE** (Cautious):
- Weight limit: 2
- Draws: Shadow cards + 1 guaranteed state card
- Momentum: Positive momentum makes observations weight 0
- Goals Available: Shadow promises, burden resolution
- Degradation: Default target for most states

**NEUTRAL** (Balanced):
- Weight limit: 3
- Draws: All types equally + 1 guaranteed state card
- Momentum: No effect (balanced state)
- Goals Available: Commerce promises, routine letters
- Degradation: → Tense at -3 momentum

**OPEN** (Receptive):
- Weight limit: 3
- Draws: Trust/Token cards + 1 guaranteed state card
- Momentum: Positive momentum adds +1 comfort to successes
- Goals Available: Trust promises, personal requests
- Degradation: → Guarded at -3 momentum

**EAGER** (Excited):
- Weight limit: 3
- Draws: Commerce/Token cards + 1 guaranteed state card
- Momentum: Each point adds +5% to token card success
- Goals Available: Commerce promises with bonus potential
- Degradation: → Neutral at -3 momentum

**CONNECTED** (Deep Bond):
- Weight limit: 4
- Draws: 60% Token, 40% any + 1 guaranteed state card
- Momentum: Increases maximum weight by momentum value (up to 7!)
- Goals Available: All promise types
- Degradation: → Tense at -3 momentum (devastating)

## Strategic Decision Framework

### Emotional State Navigation Paths

**Elena's Optimal Journey**:
1. **Desperate** → Use observation for direct jump to Open
2. **Desperate** → Tense → Neutral → Open (traditional path)
3. **Desperate** → Stay for crisis card resolution

Each path has trade-offs:
- Direct jump costs observation but saves turns
- Traditional path reliable but slow
- Staying faces crisis cards but weight 0 advantage

### Comfort Building Mathematics

**Turn Economy with 16 Patience**:
- Comfort starts at 5
- Need comfort 3+ for Crisis Letter (already met!)
- Need comfort 7+ for Formal Letter (need +2)
- Need comfort 10+ for Personal Letter (need +5)

**Build Rates**:
- W1 cards: +2 comfort at 65% + tokens (70% with 1 Trust)
- W2 cards: +4 comfort at 55% + tokens (60% with 1 Trust)
- W3 cards: +6 comfort at 45% + tokens (50% with 1 Trust)

Perfect play: 2-3 turns to reach any letter depth

### Token Economics

**Linear Benefits (ALL cards including letters)**:
- 0 tokens: Base rates (35-65% depending on card)
- 2 tokens: +10% success (45-75%)
- 4 tokens: +20% success (55-85%)
- 6 tokens: +30% success (65-95%, capped at 95%)

**Letter Negotiation Advantage**:
- Trust tokens help Trust letter negotiations
- Commerce tokens help Commerce letter negotiations
- Each matching token type adds 5% to negotiation success
- Better success = better deadlines, payment, queue position
- No gates - letters accessible at 0 tokens, just harder negotiation

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
4. Stay Desperate for crisis resolution
5. Focus on token cards at 70% success
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
1. Build Shadow tokens with Guard Captain (need 4 more!)
2. Wait for night shift (6 PM)

**Evening**:
3. Guard in Neutral state
4. Build comfort to D8
5. Get Checkpoint Pass letter (if had 5 Shadow tokens)
6. Free access to Noble District

**Results**: No bribe needed but requires heavy token investment

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
W1 Trust card, 1 Trust token with Elena:
65% + (1 × 5%) = 70%

W2 Commerce card, 2 Commerce with Marcus:
55% + (2 × 5%) = 65%

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
- +15% success on all cards (base + 15% from 3 tokens)
- Letter negotiations much more favorable
- Crisis cards still risky but manageable
- Can build momentum to reduce patience cost

**Scenario 2**: Elena Open, You have -2 Trust tokens (from past failures)
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
- **Token Linearity**: Every token adds exactly 5% success
- **One Card Per Turn**: Authentic conversation rhythm
- **Linear Progression**: Every token matters equally
- **Queue Displacement**: Permanent sacrifice for flexibility
- **Fleeting Cards**: Tactical hand management

No thresholds, no hidden mechanics, no soft locks. Every mechanic serves one purpose while resources flow through multiple systems. The puzzle emerges from interaction, not complication.