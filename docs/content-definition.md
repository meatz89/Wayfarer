# Wayfarer POC - Elena's Letter Scenario

## Scenario Overview

Elena faces forced marriage to Lord Blackwood. Her refusal letter must reach him before he leaves at sunset (5 PM). The tactical puzzle: navigate emotional states, build comfort to access letters at different depths, and manage limited resources without thresholds or hidden mechanics.

## Core Mechanical Principles

### Strict Effect Separation
- Each card type has ONE effect pool
- No cards do multiple things
- No thresholds - linear progression only
- Perfect information - all effects visible

### Strategic Layers
- **Emotional State Navigation**: Different states enable different letter types
- **Comfort Building**: Depth access requires careful accumulation
- **Token Investment**: Linear +5% per token, also displacement currency
- **Queue Management**: Position 1 must complete first

## Starting Configuration

### Player Resources
- **Coins**: 10 (checkpoint bribe cost)
- **Health**: 75/100 (no weight penalty yet)
- **Hunger**: 60/100 (reducing attention by 3)
- **Attention**: 6/10 (after morning calculation)
- **Satchel**: 0/5 letters

### Connection Tokens (Linear Bonuses)
- **Trust**: 1 with Elena (+5% Trust cards)
- **Commerce**: 2 with Marcus (+10% Commerce cards)
- **Status**: 0 with all (+0%)
- **Shadow**: 1 with Guard Captain (+5% Shadow cards)

### Time Management
- **Current**: Tuesday 9:00 AM (Morning)
- **Elena Available**: 2:00 PM (Afternoon)
- **Lord Blackwood Departs**: 5:00 PM (Evening)
- **Maximum Actions**: ~15 with perfect efficiency

## Location Architecture

### Market Square
**Fountain** (Crossroads, Public):
- Travel hub to all districts
- Public: -1 patience in conversations
- Observations available per time period

**Merchant Row** (Commercial):
- Marcus available (Morning-Evening)
- Work action: 2 attention → 8 coins + 4 hours
- Quick exchanges available

**Guard Post** (Authority, Tense):
- Guard Captain always present
- Day shift: Tense state
- Night shift: Neutral state

### Copper Kettle Tavern
**Common Room** (Crossroads, Public):
- Travel hub for tavern district
- Public: -1 patience

**Corner Table** (Private):
- Elena available (Afternoon-Evening only)
- Private: +1 patience
- Quiet atmosphere for deep conversations

**The Bar** (Commercial, Hospitality):
- Bertram always available
- Work action available
- Rest exchanges via Bertram's deck

### Noble District Gate
**Checkpoint** (Crossroads, Authority):
- Requires: 10 coin bribe OR access permit
- Guards inspect all travelers
- Direct route to Lord Blackwood

## NPC Configurations

### Elena - The Letter Sender

**Mechanical Identity**:
- Type: Devoted (12 base patience)
- Location: Corner Table (+1 patience = 13 total)
- Available: Afternoon-Evening only
- Starting State: Desperate (narrative situation)

**Conversation Deck Structure**:

**Comfort Cards** (12 total - primary deck component):
- Depths 0-5: 6 cards (always accessible)
  - "Gentle Understanding" (D2/W1): +2 comfort
  - "Active Listening" (D3/W1): +3 comfort
  - "Sympathetic Response" (D4/W1): +4 comfort
  - "Shared Experience" (D5/W2): +5 comfort
- Depths 6-10: 3 cards (requires building)
  - "Deep Connection" (D7/W2): +7 comfort
  - "Emotional Support" (D9/W2): +9 comfort
- Depths 11-20: 3 cards (requires excellence)
  - "Perfect Understanding" (D15/W3): +15 comfort

**Token Cards** (3 total - relationship builders):
- "Build Trust" (D5/W2): +1 Trust token
- "Prove Reliability" (D10/W2): +1 Trust token
- "Sacred Promise" (D15/W3): +1 Trust token

**State Cards** (3 total - emotional navigation):
- "Calm Reassurance" (D4/W1): Desperate→Tense
- "Find Balance" (D8/W2): Tense→Neutral
- "Open Hearts" (D12/W2): Neutral→Open

**Burden Cards** (2 starting - past failure):
- "Broken Promise" (D1/W2): Remove on success
- "Lost Faith" (D3/W2): Remove on success

**Letter Deck** (State + Depth Requirements):

- **"Crisis Refusal"** (D5/Trust)
  - States: Desperate, Tense
  - Success: 4hr deadline, position 2, 10 coins
  - Failure: 1hr deadline, position 1, 15 coins

- **"Formal Refusal"** (D10/Trust)
  - States: Neutral, Open
  - Success: 6hr deadline, position 3, 12 coins
  - Failure: 2hr deadline, position 2, 12 coins

- **"Heartfelt Letter"** (D15/Trust)
  - States: Open, Connected
  - Success: 8hr deadline, flexible, 20 coins
  - Failure: 4hr deadline, position 2, 20 coins

**Crisis Deck**:
- "Everything Falls Apart" (D0/W5→0 in Desperate)
  - Injected during Desperate LISTEN
  - Success (30% + tokens): Crisis removed, →Tense
  - Failure (70%): +2 burdens, conversation ends

### Marcus - The Merchant

**Mechanical Identity**:
- Type: Mercantile (10 base patience)
- Location: Merchant Row
- Available: Morning-Evening (shop hours)
- Starting State: Neutral

**Conversation Deck** (15 cards):
- 10 Commerce comfort cards (various depths)
- 2 Token cards (Commerce, Shadow)
- 2 State cards (→Eager for business)
- 1 Knowledge card (creates route observation)

**Letter Deck**:
- "Delivery Contract" (D6/Commerce)
  - States: Eager, Neutral
  - Standard merchant correspondence

**Exchange Deck** (Quick Trade Options):
- "Buy Provisions": 3 coins → Hunger = 0
- "Purchase Medicine": 5 coins → Health +20
- "Buy Access Permit": 15 coins → Noble permit
- "Accept Quick Job": → New obligation
- "Trade Information": Shadow token → Route knowledge

### Guard Captain - The Gatekeeper

**Mechanical Identity**:
- Type: Steadfast (11 base patience)
- Location: Guard Post
- Available: Always
- State: Tense (day) / Neutral (night)

**Conversation Deck** (12 cards):
- Mix of Status and Shadow cards
- State manipulation important for access

**Letter Deck**:
- "Checkpoint Pass" (D8/Shadow)
  - States: Neutral only
  - Creates 24hr access permit

### Bertram - The Innkeeper

**Mechanical Identity**:
- Type: Mercantile (no conversations)
- Location: The Bar
- Available: Always
- Pure exchange NPC

**Exchange Deck** (Rest & Recovery):
- "Quick Meal": 2 coins → Hunger = 0
- "Short Rest": 2 coins → +3 attention, +2 hours
- "Full Night": 5 coins → Morning refresh (loses day)
- "Noble Gossip": 3 coins → Status observation
- "Packed Lunch": 4 coins → Hunger reset item

## Observation Opportunities

### Dynamic Knowledge System
Observations create comfort cards with expiration:
- Cost: 1 attention at specific locations
- Effect: Comfort card added to hand
- Expiration: Fixed deadline (6-24 hours)

### Location-Based Observations

**Market Square** (Morning):
- "Early Trade Routes" → Commerce card (D2, expires 6hr)

**Market Square** (Afternoon):  
- "Guard Shift Patterns" → Shadow card (D0, expires 12hr)

**Copper Kettle** (When Elena present):
- "Family Seal Details" → Status card (D3, expires 24hr)

### Conversation-Generated Observations
Knowledge cards in NPC decks can create:
- "Ask About Routes" → Travel observation
- "Inquire About Work" → Commerce observation
- "Share Secrets" → Shadow observation

## Strategic Decision Framework

### Emotional State Navigation

**Elena's State Journey**:
1. **Desperate** (Starting): Crisis cards, weight limit 1
2. **Tense** (Escape): Shadow letters available
3. **Neutral** (Stabilize): Commerce letters, weight 3
4. **Open** (Goal): Trust letters, best terms

Each transition requires specific state cards and successful plays.

### Comfort Building Strategy

**Turn Economy**:
- Start: Comfort 5 (depth 0-5 accessible)
- Target for Crisis Letter: Comfort 5+ (already met)
- Target for Formal Letter: Comfort 10+ (need +5)
- Target for Best Letter: Comfort 15+ (need +10)

**Build Rate Examples**:
- Safe plays (W1): +2-4 comfort per success
- Moderate (W2): +5-7 comfort per success
- Risky (W3): +8-12 comfort per success

### Token Investment

**Linear Benefits**:
- 0 tokens: Base success rates (40-60%)
- 2 tokens: +10% success (+50-70%)
- 4 tokens: +20% success (60-80%)
- 6 tokens: +30% success (70-90%)

**Token as Currency**:
- Building tokens improves all future plays
- Burning tokens for queue jumps permanent damage
- Each burned token adds burden to NPC deck

## Multiple Solution Paths

### Path A: Morning Efficiency
**Morning** (6 attention, 10 coins, 60 hunger):
1. Work at Merchant Row (-2 att, +8 coins, →Midday)
2. Exchange: Buy food (-3 coins, hunger→0)
3. Observe guard patterns (-1 att, gain Shadow card)

**Afternoon** (3 attention, 15 coins, 0 hunger):
4. Converse with Elena (-2 att, 13 patience available)
5. Navigate Desperate→Tense→Neutral
6. Build comfort to 10+
7. Access "Formal Refusal" in Open state

**Results**: Good letter terms, tokens gained, profitable

### Path B: Token Investment
**Morning**:
1. Exchange: Buy food immediately (-3 coins)
2. Wait to Afternoon (preserve 6 attention)

**Afternoon** (6 attention, 7 coins):
3. Full Elena conversation (can retry if needed)
4. Focus on token cards at depths 5, 10, 15
5. Build to maximum depth for best letter

**Results**: Best letter terms, maximum tokens, low coins

### Path C: Crisis Management
**Morning**:
1. Skip all preparation
2. Wait directly to Afternoon

**Afternoon** (6 attention, 10 coins, hungry):
3. Elena conversation with reduced patience (9 turns)
4. Accept crisis letter at depth 5 immediately
5. Poor negotiation outcome accepted

**Evening**:
6. Use all coins for checkpoint bribe
7. Deliver with minimal time remaining

**Results**: Bare success, poor terms, damaged relationships

## Queue Management Mechanics

### Position Negotiation
When accepting Elena's letter:
- Base: Attempts lowest available position
- Desperate Elena: Attempts position 1
- Success: Your terms (position 2-3)
- Failure: Her terms (position 1)

### Displacement Costs
If other obligations exist:
- Jump 1 spot: -1 Trust with Elena
- Jump 2 spots: -2 Trust with EACH NPC
- Each burned token → burden card in deck

### Strategic Queue Timing
- Accept letter with good position early
- Complete other obligations first
- Only displace in emergencies

## Failure Conditions

### Hard Failure
- 5 PM passes without delivery
- Elena permanently Hostile
- -3 Trust tokens
- 3 burden cards added
- Scenario restart required

### Soft Failures
- Poor negotiation (1hr deadline)
- Forced position 1 (queue chaos)
- Token burning (relationship damage)
- Resource depletion (no recovery)

## Success Metrics

### Perfect Run
- "Heartfelt Letter" obtained (D15)
- 8-hour deadline negotiated
- Complete by 3 PM
- +2 Trust tokens gained
- 20+ coins earned
- No tokens burned

### Good Run
- "Formal Refusal" obtained (D10)
- 4-6 hour deadline
- Complete by 4 PM
- Tokens maintained
- Break even on coins

### Acceptable Run
- Any letter delivered
- Before 5 PM deadline
- Regardless of terms
- Even with displacement

## Emergent Puzzle Variations

### Context Changes Everything

**Same Elena, Different Puzzles**:

**Scenario 1**: Elena Desperate, You have 3 Trust tokens
- +15% success but crisis cards inject
- Must balance aggression with crisis management

**Scenario 2**: Elena Open, You have -2 Trust tokens
- Weight 3 cards available but -10% success
- Must rebuild trust or accept poor odds

**Scenario 3**: Elena Neutral, 4 burden cards in deck
- Draw quality diluted
- Must persist through bad draws

**Scenario 4**: Multiple obligations already queued
- Must negotiate position carefully
- Displacement costs compound

Each combination creates unique tactical challenge with multiple valid approaches.

## Core Innovation Summary

The scenario demonstrates how simple rules create complex decisions:
- No thresholds means every token matters
- Emotional states create distinct puzzle modes
- Comfort building requires risk management
- Queue displacement forces permanent sacrifice
- Linear progression maintains perfect information

Every mechanical element serves exactly one purpose, but resources flow through multiple systems, creating strategic depth through emergence rather than complexity.