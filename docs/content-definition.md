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
- **Focus Management**: Capacity 3-6, persists until refreshed
- **Rapport Building**: Temporary success modifier (-50 to +50) within conversation
- **Flow Tracking**: +1 on success, -1 on failure, ±3 triggers state transitions
- **Token Investment**: Starting rapport = connection tokens with NPC
- **Queue Management**: Position 1 must complete first, multiple obligations compete
- **Atmosphere Control**: Persistent effects shape entire conversations

## Starting Configuration

### Player Resources
- **Coins**: 10 (exactly enough for checkpoint bribe if no permit)
- **Health**: 75/100 (no focus penalty yet)
- **Hunger**: 60/100 (reducing attention by 2)
- **Attention**: 8/10 (after morning calculation: 10 - 2)
- **Satchel**: Empty (5 slots max)

### Starting Queue (Position 1 MUST complete first!)
1. Marcus Package - 5hr deadline - 8 coins payment
2. Guard Report - 8hr deadline - 5 coins payment  
3. [Elena's letter will compete for position]

### Starting Tokens (Determine Starting Rapport)
- **Elena**: 1 Trust (starts conversations with 1 rapport = +2% all cards)
- **Marcus**: 2 Commerce (starts conversations with 2 rapport = +4% all cards)
- **Guard Captain**: 1 Shadow (starts conversations with 1 rapport = +2% all cards)
- **Lord Blackwood**: 0 all types (starts conversations with 0 rapport)

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

**Guard Post** (Authority, Guarded):
- Guard Captain always present
- Day shift: Guarded atmosphere default
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
- Guarded atmosphere

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
- Starting State: Disconnected (forced marriage situation)
- Starting Rapport: Equal to your Trust tokens with her

**Conversation Deck** (20 cards):

**Fixed Rapport Cards** (6 total):
- "I understand" (Trust-type, W1, Easy 70%): +1 rapport
- "Let me help" (Trust-type, W1, Easy 70%): +1 rapport  
- "You're safe with me" (Trust-type, W2, Medium 60%): +2 rapport
- "Trust in our bond" (Trust-type, W2, Medium 60%): +2 rapport
- "Together we're strong" (Trust-type, W3, Medium 60%): +3 rapport
- "Soul connection" (Trust-type, W5, Very Hard 40%, Impulse): +5 rapport

**Scaled Rapport Cards** (4 total, all Trust-type):
- "Our trust runs deep" (Trust-type, W2, Hard 50%): +X rapport where X = Trust tokens
- "Remember our history" (Trust-type, W3, Hard 50%): +X rapport where X = Trust tokens
- "Lean on me" (Trust-type, W3, Hard 50%): +X rapport where X = 4 - current rapport/5
- "Crisis shared" (Trust-type, W4, Hard 50%, Impulse): +X rapport where X = patience ÷ 3

**Utility Cards** (4 total, all Trust-type):
- "Let me think" (Trust-type, W1, Medium 60%): Draw 1 card
- "Consider options" (Trust-type, W1, Medium 60%): Draw 1 card
- "Gather strength" (Trust-type, W2, Medium 60%): Add 1 focus to pool
- "Deep breath" (Trust-type, W2, Medium 60%): Add 1 focus to pool

**Setup Cards** (3 total, 0 focus, mixed types):
- "Careful approach" (Trust-type, W0, Easy 70%): No effect, Atmosphere: Prepared
- "Open my heart" (Commerce-type, W0, Easy 70%): No effect, Atmosphere: Receptive
- "This is critical" (Status-type, W0, Easy 70%): No effect, Atmosphere: Final

**Dramatic Cards** (2 total, impulse, Trust-type):
- "Disconnected plea" (Trust-type, W4, Hard 50%, Impulse): +4 rapport, Atmosphere: Volatile
- "All or nothing" (Trust-type, W6, Very Hard 40%, Impulse): +5 rapport, Atmosphere: Final

**Flex Slot** (1, Trust-type):
- "Shared pain" (Trust-type, W2, Medium 60%): -2 rapport (represents emotional overflow)

**Request Deck** (Separate from conversation deck):

- **"Crisis Refusal"** (Trust Letter)
  - Focus: 5
  - Difficulty: Very Hard (40% base + rapport × 2%)
  - Success Effect: Accept Letter for 1hr deadline, position 1, 5 coins
  - Failure Effect: Add 1 burden card to Elena's relationship
  - Has "Impulse" and "Opening" property when becomes playable

- **"Clear the Air"** (Resolution Request)
  - Focus: 5
  - Difficulty: Hard (50% base + rapport × 2%)
  - Success Effect: Remove burden cards from relationship record
  - Failure Effect: Add 1 burden card
  - Has "Impulse" and "Opening" property when becomes playable

**Relationship Record**:
- 2 burden cards from past failure (visible marker of damaged trust)

### Marcus - The Merchant

**Mechanical Identity**:
- Type: Mercantile (12 base patience)
- Location: Merchant Row
- Available: Morning-Evening (6 AM - 10 PM, shop hours)
- Starting State: Neutral (commerce-minded)
- Starting Rapport: Equal to your Commerce tokens with him

**Conversation Deck** (20 cards):

**Fixed Rapport Cards** (6, mostly Commerce-type):
- 3 at W1 (Commerce-type, Easy 70%): +1 rapport each
- 2 at W2 (Commerce-type, Medium 60%): +2 rapport each
- 1 at W3 (Status-type, Medium 60%): +3 rapport

**Scaled Rapport Cards** (4, all Commerce-type):
- 2 "Good business" (Commerce-type, W2, Hard 50%): +X where X = Commerce tokens
- 1 "Profitable relationship" (Commerce-type, W3, Hard 50%): +X where X = Commerce tokens
- 1 "Time is money" (Commerce-type, W3, Hard 50%): +X where X = focus remaining

**Utility Cards** (4, Commerce-type):
- 2 Draw cards (Commerce-type, W1, Medium 60%)
- 2 Focus-add cards (Commerce-type, W2, Medium 60%)

**Setup Cards** (3, W0, mixed types):
- "Let's negotiate" (Commerce-type, Easy 70%): Atmosphere: Focused
- "Time for business" (Trust-type, Easy 70%): Atmosphere: Patient
- "High stakes" (Shadow-type, Easy 70%): Atmosphere: Final

**Dramatic Cards** (2, impulse, Commerce-type):
- "Deal of lifetime" (Commerce-type, W5, Very Hard 40%): +5 rapport
- "All in" (Commerce-type, W4, Hard 50%): +4 rapport, Atmosphere: Volatile

**Flex**: 1 negative rapport card (Commerce-type)

**Request Deck**:
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
- Default Atmosphere: Guarded (day) / Neutral (night)
- Starting Rapport: Equal to your Shadow tokens with him

**Conversation Deck** (20 cards):

**Fixed Rapport Cards** (6, balanced types):
- 2 at W1 (Status-type, Easy 70%): +1 rapport each
- 2 at W1 (Shadow-type, Easy 70%): +1 rapport each
- 1 at W2 (Status-type, Medium 60%): +2 rapport
- 1 at W3 (Shadow-type, Medium 60%): +3 rapport

**Scaled Rapport Cards** (4, mixed):
- 2 scaling with Status tokens (Status-type, W2, Hard 50%)
- 2 scaling with Shadow tokens (Shadow-type, W3, Hard 50%)

**Utility Cards** (4, mixed types): 
- 2 Draw cards (1 Status-type, 1 Shadow-type, W1, Medium 60%)
- 2 Focus-add cards (1 Status-type, 1 Shadow-type, W2, Medium 60%)

**Setup Cards** (3, W0, mixed types):
- "Official business" (Status-type, Easy 70%): Atmosphere: Volatile
- "By the book" (Trust-type, Easy 70%): Atmosphere: Prepared
- "Under scrutiny" (Shadow-type, Easy 70%): Atmosphere: Pressured

**Dramatic Cards** (2, mixed types): 
- "Authority demonstrated" (Status-type, W4, Hard 50%, Impulse): +4 rapport
- "Secrets revealed" (Shadow-type, W5, Very Hard 40%, Impulse): +5 rapport

**Flex**: 1 Authority-themed card (Status-type)

**Request Deck**:
- "Checkpoint Pass" (Shadow Promise, W5, Hard 50%)
  - Success: 24hr access permit (fixed terms)
  - Failure: Add 1 burden card

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
- Starting Rapport: 0 (no existing relationship)

**Special Rules**:
- Receives Elena's letter
- Cannot converse (just delivery)
- Proud personality affects queue negotiations

## Player Observation Deck System

### Building Your Deck
The player maintains their own observation deck (max 20 cards):
- Cost: 1 attention at specific locations
- Focus: 1 (minimal requirement)
- Success rate: 85% (Very Easy)
- Always persistent
- Expiration: 24-48 hours

### Location-Based Observations

**Market Square - Morning**:
- "Guard Routes": Set Pressured atmosphere (expires 24hr)

**Market Square - Afternoon**:
- "Market Gossip": Set Receptive atmosphere (expires 24hr)

**Market Square - Evening**:
- "Night Paths": Next SPEAK costs 0 focus (expires 12hr)

**Copper Kettle - When Elena Present**:
- "Shared Hardship": Set Informed atmosphere (expires 48hr, powerful!)

**Guard Post - Night**:
- "Bribery Option": Rapport = 15 (expires 6hr, emergency boost)

### Conversation-Generated Observations
NPCs can reward observation cards:
- "Noble Routes": Next action costs 0 patience
- "Guard Timing": Set Synchronized atmosphere
- "Hidden Path": Focus = maximum

## Connection State Effects

### State Transitions and Focus Capacity

**DISCONNECTED** (Elena's starting state):
- Focus capacity: 3
- Cards drawn: 1
- Flow: +3→Guarded (escape!), -3→Conversation ends
- Requests Available: Crisis promises, urgent letters

**GUARDED** (Cautious):
- Focus capacity: 4
- Cards drawn: 2
- Flow: +3→Neutral, -3→Disconnected
- Requests Available: Shadow promises, burden resolution

**NEUTRAL** (Balanced):
- Focus capacity: 5
- Cards drawn: 2
- Flow: +3→Open, -3→Guarded
- Requests Available: Commerce promises, routine letters

**OPEN** (Receptive):
- Focus capacity: 5
- Cards drawn: 3
- Flow: +3→Connected, -3→Neutral
- Requests Available: Trust promises, personal requests

**CONNECTED** (Deep Bond):
- Focus capacity: 6
- Cards drawn: 3
- Flow: +3→Stays Connected (maxed), -3→Open
- Requests Available: All promise types

## Strategic Decision Framework

### Focus Navigation

**Elena's Challenge in Disconnected (3 capacity)**:
- Can play three W1 cards before refresh
- Can play one W3 card then need LISTEN
- Cannot play W4+ cards without Prepared atmosphere
- Cannot play W5 request cards without state change

**Reaching Request Cards**:
- Need 5+ focus capacity (Open/Connected states)
- OR use Prepared atmosphere (+1 capacity)
- OR use observation to set Informed (auto-success)

### Flow Building Mathematics

**Turn Economy with 16 Patience**:
- Flow starts at 0
- Each net success moves flow +1
- Each net failure moves flow -1
- Need +3 net successes to advance state
- Disconnected → Guarded → Neutral → Open (9 net successes total)

**Success Rates with Starting Rapport**:
- With 1 Trust token (Elena): Start at 1 rapport (+2% to all cards)
- With 2 Commerce tokens (Marcus): Start at 2 rapport (+4% to all cards)
- Build to 25 rapport: +50% success on all cards
- Reach 50 rapport cap: Guaranteed success on everything

**Rapport Building Strategy**:
- Early rapport cards compound success
- High rapport makes flow progression reliable
- Failed cards reduce flow AND potentially rapport
- Momentum is key - early success breeds later success

### Token Economics

**Starting Rapport Benefits**:
- 5 tokens = 5 starting rapport = +10% all cards
- 10 tokens = 10 starting rapport = +20% all cards
- 20 tokens = 20 starting rapport = +40% all cards

**Token Acquisition** (ONLY through letter delivery):
- Standard successful delivery: +1 token with recipient
- Excellent delivery: +2-3 tokens with recipient
- Trust letters build Trust tokens
- Commerce letters build Commerce tokens
- Status letters build Status tokens
- Shadow letters build Shadow tokens

**Building the Right Tokens**:
- Delivering Trust letters → Trust tokens with recipient
- Delivering Commerce letters → Commerce tokens with recipient
- Must match letter type to build desired relationships

## Queue Management Strategies

### Position Negotiation (Now Fixed Terms)

**Elena's Letter Terms**:
- Crisis Letter: Always 1hr deadline, position 1, 5 coins
- No negotiation - accept these terms or fail

### Displacement Calculations

Starting queue:
1. Marcus Package (5hr, 8 coins)
2. Guard Report (8hr, 5 coins)
3. Elena's Letter (if accepted)

To deliver Elena immediately from position 3:
- Displace Marcus: -2 Commerce tokens
- Displace Guard: -1 Shadow token
- Total cost: 3 tokens burned permanently

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
4. Complete Marcus delivery (avoiding displacement)
5. Wait to Afternoon (preserve attention)

**Afternoon** (5 attention, 15 coins):
6. Observe "Shared Hardship" at Copper Kettle (-1 att, Informed atmosphere)
7. Converse with Elena (-2 att, 16 patience)
8. Starting with 1 rapport (+2% to all cards)
9. Build rapport through successful cards
10. Reach Open state through 3 net successes
11. LISTEN at Open state - request becomes playable with Impulse+Opening
12. Play request immediately with boosted success from rapport

**Results**: Fixed terms accepted, queue cleared, relationship maintained

### Path B: Focus Management
**Morning**:
1. Exchange: Buy food immediately (-3 coins)
2. Wait to Afternoon (preserve 8 attention)

**Afternoon** (8 attention, 7 coins):
3. Full Elena conversation (-2 att)
4. Carefully manage 3 focus capacity in Disconnected
5. Use setup cards (W0) for Prepared atmosphere
6. Build rapport early for higher success rates
7. Navigate to Open through careful flow management

**Results**: Challenging but possible with good rapport building

### Path C: Crisis Management
**Morning**:
1. Skip all preparation
2. Rush to complete existing obligations

**Afternoon** (limited resources):
3. Elena conversation with whatever remains
4. Low starting rapport makes success difficult
5. Accept request at any state when possible
6. Heavy displacement if needed

**Evening**:
6. Use all coins for checkpoint
7. Deliver with minimal margin

**Results**: Mission complete but relationships damaged

### Path D: Guard Captain Route
**Morning**:
1. Build Shadow tokens through previous deliveries
2. Start conversation with higher rapport from tokens
3. Navigate to request availability

**Evening**:
4. Guard in Neutral atmosphere (better than Guarded)
5. Use rapport to boost success chance for permit request
6. Success: Free access to Noble District

**Results**: Risky but preserves resources

## Atmosphere Management

### Strategic Atmosphere Chains

**Setup for Success**:
1. Play "Careful approach" (W0) → Prepared atmosphere
2. Now have 4 focus capacity in Disconnected (3+1)
3. Play multiple cards before refresh needed

**High Risk/Reward**:
1. Play "This is critical" (W0) → Final atmosphere
2. Any failure ends conversation
3. But can attempt dramatic plays

**Information Advantage**:
1. Use "Shared Hardship" observation → Informed atmosphere
2. Next card cannot fail
3. Guarantee critical rapport gain or request play

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
Base Easy card (70%) with 5 rapport:
70% + (5 × 2%) = 80%

Base Medium card (60%) with 10 rapport:
60% + (10 × 2%) = 80%

Base Hard card (50%) with 25 rapport:
50% + (25 × 2%) = 100%

Base Very Hard request (40%) with 15 rapport:
40% + (15 × 2%) = 70%

Negative rapport (-10):
Base rate - 20% (can make cards impossible)
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
- Request card failure: +1 burden card
- Poor rapport management: Negative spiral
- Token burning: Permanent relationship damage
- Resource depletion: Cannot afford checkpoint
- Impulse request discarded: Conversation fails

### Recovery Options
- Work for emergency coins (loses 4 hours)
- Rest for attention (costs coins)
- Displace obligations (burns tokens)
- Accept any terms (poor rewards)
- Use observations for emergency advantages

## Success Metrics

### Perfect Run (Master Strategist)
- Build high rapport early
- Reach Open state efficiently
- Complete request with high success chance
- Complete by 3 PM
- Gain +3 Trust tokens from delivery
- 20+ coins earned
- No tokens burned

### Good Run (Competent Courier)
- Moderate rapport building
- Reach request availability
- Complete by 4 PM
- Gain +2 Trust tokens
- Break even on coins

### Acceptable Run (Disconnected Success)
- Any letter delivered
- Fixed terms accepted
- Heavy displacement
- Before 5 PM deadline
- Mission complete

## Emergent Puzzle Variations

### Context Changes Everything

**Scenario 1**: Elena Disconnected, You have 3 Trust tokens
- Start with 3 rapport (+6% all cards)
- Can build momentum quickly
- Flow progression more reliable

**Scenario 2**: Elena Open, You have 0 Trust tokens
- 5 focus capacity available
- Start with 0 rapport (base rates)
- Can play request cards immediately but success uncertain

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
Successful delivery adds cards to NPC decks:
- Rapport-building cards matching relationship
- Makes future conversations easier
- Permanent world change

Failed request adds burden to relationship:
- 1 burden card in record per failure
- Future conversations require resolution
- Relationship permanently scarred

### Token Economy
Successful delivery creates cascading benefits:
- +1-3 tokens with recipient
- Better starting rapport in future
- Easier conversations compound

Burned tokens create cascading damage:
- -1 token = -1 starting rapport forever
- Negative tokens = relationship debt

Twenty deliveries create twenty permanent changes.

## Core Innovation Summary

The scenario demonstrates elegant complexity through simple rules:
- **Focus**: Persistent capacity creates multi-turn planning
- **Rapport**: Linear +2% per point, starts at token value
- **Flow**: Pure success/failure tracker, ±3 triggers transitions
- **Atmosphere Persistence**: Environmental effects shape conversations
- **Queue Displacement**: Permanent token sacrifice for flexibility
- **Observation Effects**: Unique advantages from exploration

No thresholds (except flow ±3), no hidden mechanics, no soft locks. Every mechanic serves one purpose while resources flow through multiple systems. The puzzle emerges from interaction, not complication.