# Wayfarer POC - Elena's Letter Scenario

## Scenario Overview

Elena needs her urgent letter delivered to Lord Blackwood before he leaves the city at sunset (5 PM). She faces a forced marriage to him and this letter is her refusal. The challenge: Elena isn't available until Afternoon (2 PM), Lord Blackwood is behind the Noble District checkpoint, and you start with limited resources.

## Starting Conditions

### Player State
- **Coins**: 10 (exactly enough for checkpoint bribe)
- **Health**: 75/100
- **Hunger**: 60/100 
- **Attention**: 6/10 
- **Satchel**: Empty (5 slots max)

### Starting Tokens
- **Trust**: 1 with Elena
- **Commerce**: 2 with Marcus
- **Status**: 0 with all
- **Shadow**: 1 with Guard Captain

### Time & Location
- **Time**: Tuesday 9:00 AM (Morning period)
- **Location**: Market Square - Fountain
- **Critical Timeline**:
  - Elena available: 2 PM (5 hours away)
  - Lord Blackwood leaves: 5 PM (8 hours away)

## World Map

### Market Square
**Spots**:
- Fountain (Crossroads, Public -1 patience)
- Merchant Row (Commercial)
- Guard Post (Authority)
- North Alcove (Discrete)

**Available Actions**:
- Travel from Fountain (Crossroads)
- Work at Merchant Row (2 attention → 8 coins, advance 4 hours)
- Observe (1 attention → observation card)

### Copper Kettle Tavern
**Spots**:
- Common Room (Crossroads, Public -1 patience)
- Corner Table (Private +1 patience)
- Bar (Commercial, Hospitality)

**Available Actions**:
- Travel from Common Room
- Work at Bar (2 attention → 8 coins, advance 4 hours)
- Rest via Bertram's exchange deck

### Noble District Gate
**Spots**:
- Checkpoint (Crossroads, Authority, Tense)
- Side Path (Discrete)

**Access Options**:
- Bribe guards: 10 coins (always available)
- Checkpoint Pass: Letter from Guard Captain
- Alternative route: Via Warehouse District (discoverable)

### Travel Routes
From Market Square Fountain:
- → Copper Kettle: 15 minutes, free
- → Noble District Gate: 25 minutes, free
- → Warehouse District: 30 minutes, free

From Noble District Gate:
- → Blackwood Manor: 10 minutes, requires checkpoint pass

## NPC Definitions

### Elena (Devoted, Corner Table)

**Base Stats**:
- Patience: 12 (Devoted) +1 (Private spot) = 13
- Starting State: Desperate (forced marriage situation)
- Available: Afternoon and Evening only (2 PM - 10 PM)

**Why Desperate**: Elena faces forced marriage regardless of timeline. This creates her desperate emotional state, not the deadline.

**Conversation Deck** (20 cards total):

*Comfort Cards (12 cards)*:
- "Gentle Nod" - Comfort/Trust, D0, W1, Success: +2 comfort
- "Listen Quietly" - Comfort/Trust, D2, W1, Success: +3 comfort  
- "Sympathetic Look" - Comfort/Trust, D4, W1, Success: +4 comfort
- "Understanding Words" - Comfort/Trust, D6, W2, Success: +5 comfort
- "Supportive Statement" - Comfort/Trust, D8, W2, Success: +6 comfort
- "Deep Empathy" - Comfort/Trust, D10, W2, Success: +7 comfort
- "Heartfelt Support" - Comfort/Trust, D12, W3, Success: +8 comfort
- "Complete Understanding" - Comfort/Trust, D14, W3, Success: +9 comfort
- "Perfect Resonance" - Comfort/Trust, D16, W3, Success: +10 comfort
- "Soul Connection" - Comfort/Trust, D18, W3, Success: +12 comfort
- (2 duplicates at various depths)

*Token Cards (3 cards)*:
- "Share Vulnerability" - Token/Trust, D5, W2, Success: +1 Trust token
- "Promise Protection" - Token/Trust, D10, W2, Success: +1 Trust token
- "Vow of Support" - Token/Trust, D15, W3, Success: +1 Trust token

*State Cards (3 cards)*:
- "Calm Reassurance" - State/Trust, D4, W1, Success: Desperate→Tense
- "Find Peace" - State/Trust, D8, W2, Success: Tense→Neutral
- "Open Heart" - State/Trust, D12, W2, Success: Any→Open

*Burden Cards (2 cards, start in deck from past failure)*:
- "Broken Promise" - Burden/Trust, D1, W2, Success: Remove burden
- "Lost Faith" - Burden/Trust, D3, W2, Success: Remove burden

**Letter Deck** (3 letters):

- "Desperate Refusal" - Letter/Trust, D5, W2
  - Available states: Desperate, Tense
  - Success: 4h deadline, position 2, 10 coins payment
  - Failure: 1h deadline, position 1, 15 coins payment

- "Formal Refusal" - Letter/Trust, D10, W3
  - Available states: Neutral, Open
  - Success: 6h deadline, position 3, 12 coins payment
  - Failure: 2h deadline, position 2, 12 coins payment

- "Personal Letter" - Letter/Trust, D15, W3
  - Available states: Open, Connected
  - Success: 8h deadline, flexible position, 20 coins payment
  - Failure: 4h deadline, position 2, 20 coins payment

**Crisis Deck** (1 card, injected when Desperate):
- "Breaking Point" - Crisis/Trust, D0, W5 (counts as W0 in Desperate)
  - Success (30% + tokens): Remove crisis, Desperate→Tense
  - Failure (70%): Add 2 burden cards, conversation ends

### Marcus (Mercantile, Merchant Row)

**Base Stats**:
- Patience: 10 (Mercantile)
- Starting State: Neutral
- Available: Morning through Evening (6 AM - 10 PM)

**Conversation Deck** (15 cards):

*Comfort Cards (10 cards)*:
- Various Commerce comfort cards, depths 0-15
- Success: +2 to +9 comfort based on depth

*Token Cards (2 cards)*:
- "Profitable Partnership" - Token/Commerce, D6, W2, Success: +1 Commerce
- "Trade Secret" - Token/Shadow, D10, W2, Success: +1 Shadow

*State Cards (2 cards)*:
- "Business Opportunity" - State/Commerce, D4, W1, Success: Neutral→Eager
- "Calm Markets" - State/Commerce, D8, W2, Success: Any→Neutral

*Observation Creation Card (1 card)*:
- "Share Routes" - Observation/Shadow, D8, W2, Success: Create "Hidden Path" observation (2h expiration)

**Letter Deck** (2 letters):

- "Delivery Contract" - Letter/Commerce, D6, W2
  - Available states: Eager, Neutral
  - Success: 6h deadline, position 3, 8 coins
  - Failure: 3h deadline, position 2, 8 coins

- "Noble Permit Sale" - Letter/Commerce, D4, W2
  - Available states: Eager
  - Success: Pay 12 coins, get permit
  - Failure: Pay 15 coins, get permit

**Exchange Deck** (5 cards, accessed via Quick Exchange):
- "Buy Food" - 3 coins → Hunger = 0
- "Purchase Permit" - 15 coins → Noble District Permit
- "Quick Job" - Accept → New obligation (8 coins, 3h)
- "Buy Medicine" - 5 coins → Health +20
- "Trade Information" - 1 Shadow token → Alternative route knowledge

### Guard Captain (Steadfast, Guard Post)

**Base Stats**:
- Patience: 11 (Steadfast)
- State: Tense (day shift) or Neutral (night shift after 6 PM)
- Available: Always

**Conversation Deck** (12 cards):
- Comfort cards (Status/Shadow types)
- State card: "Official Business" - State/Status, D4, W2, Success: Tense→Neutral
- Token card: "Mutual Understanding" - Token/Shadow, D8, W2, Success: +1 Shadow

**Letter Deck** (1 letter):
- "Checkpoint Pass" - Letter/Shadow, D8, W2
  - Available states: Neutral
  - Success: 24h access permit
  - Failure: 2h access permit

### Bertram (Mercantile, Copper Kettle Bar)

**Base Stats**:
- No conversation deck - pure exchange NPC
- Available: Always

**Exchange Deck** (5 cards):
- "Quick Meal" - 2 coins → Hunger = 0
- "Short Rest" - 2 coins → +3 attention, advance 2 hours
- "Room for Night" - 5 coins → Full rest (skip to morning, too late!)
- "Gossip" - 3 coins → Create "Noble Schedule" observation
- "Packed Lunch" - 4 coins → Item that sets Hunger = 0 when used

## Observation Opportunities

### Market Square - Fountain

**Morning Period**:
- "Early Merchants" - 1 attention → Commerce observation card (D2, W1, expires 2h)
  - Provides +3 comfort with merchant NPCs

**Afternoon Period**:
- "Guard Shift Change" - 1 attention → Shadow observation card (D0, W1, expires 2h)
  - Provides +4 comfort, reveals night shift more lenient

### Copper Kettle - Corner Table

**When Elena Present**:
- "Family Seal" - 1 attention → Status observation card (D3, W1, expires 2h)
  - Provides +5 comfort with nobles, shows letter importance

## Strategic Paths Analysis

### Path A: Morning Efficiency
1. Work at Merchant Row (2 att, gain 8 coins, now Midday 1 PM)
2. Quick Exchange with Marcus: Buy food (3 coins, Hunger→0)
3. Observe guard shift (1 att, gain Shadow card)
4. Wait 1 hour to Afternoon
5. Talk to Elena with 13 patience, 3 attention remaining
6. Build comfort to 5+ to access "Desperate Refusal"
7. Use Shadow observation card for comfort boost
8. Play letter card (65% success with +1 Trust token)

**Risks**: Less time for delivery, but more resources

### Path B: Relationship Investment
1. Don't work morning (preserve time)
2. Quick Exchange: Buy food immediately (3 coins)
3. Wait to Afternoon (still Morning, wait 5 hours)
4. Full Elena conversation with 6 attention
5. Focus on token cards at depths 5, 10, 15
6. Build to "Formal Refusal" at depth 10
7. Better success rate and terms

**Risks**: Fewer coins, but better letter terms

### Path C: Speed Run
1. Skip food (accept reduced patience from hunger)
2. Wait to Afternoon
3. Rush Elena conversation
4. Accept first available letter at depth 5
5. Take poor terms if needed
6. Use all 10 coins for checkpoint bribe
7. Deliver immediately

**Risks**: Poor terms, damaged relationships, but guaranteed completion

## Success Metrics

**Perfect Run**:
- Obtain "Formal Refusal" or better
- Negotiate 6+ hour deadline
- Complete with 2+ hours to spare
- Gain Trust tokens
- 15+ coins profit

**Good Run**:
- Obtain any letter
- Complete before 5 PM
- Maintain positive tokens
- Break even on coins

**Bare Success**:
- Deliver letter by any means
- Even with poor terms
- Even burning tokens

## Failure States

**Total Failure**:
- 5 PM passes without delivery
- Elena→Hostile permanently
- -3 Trust tokens with Elena
- No payment

**Partial Failures**:
- Accept 1-hour deadline (extreme pressure)
- Forced position 1 (disrupts other obligations)
- Burn tokens for queue management
- Health drops below 25 (weight capacity reduced)