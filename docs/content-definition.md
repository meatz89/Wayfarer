# Wayfarer POC - Complete Content Definition

## Strategic Challenge Overview

Elena needs her urgent letter delivered to Lord Blackwood before he leaves at sunset (5 PM). The challenge: Elena isn't available until Afternoon, Lord Blackwood is behind the Noble District checkpoint, and you start with limited resources. Strategic use of work, rest, and wait actions creates multiple viable paths.

## Starting Conditions

**Player Resources**:
- Coins: 10 (barely enough for checkpoint bribe)
- Health: 75 
- Hunger: 80 (-4 patience penalty)
- Attention: 5 (limited initial actions)
- Satchel: Empty (5 slots)
- Tokens: 1 Trust with Elena, 2 Commerce with Marcus, 1 Shadow with Guard Captain

**Time**: Tuesday 9:00 AM (Morning period)
**Starting Location**: Market Square - Fountain
**Critical Timeline**:
- Elena available: 2 PM (Afternoon, 5 hours)
- Lord Blackwood leaves: 5 PM (Sunset, 8 hours)
- Actual letter deadline: ~3 hours from when received

## World Map

### Locations & Routes

**Market Square**
- Fountain (Crossroads, Public -1 patience)
- Merchant Row (Commercial)
- Guard Post (Authority)
- North Alcove (Discrete)

**Copper Kettle Tavern** 
- Common Room (Crossroads, Public -1 patience)
- Corner Table (Private +1 patience)
- Bar (Commercial)

**Noble District Gate**
- Checkpoint (Crossroads, Authority, Tense)
- Side Entrance (Discrete, Locked)

**Blackwood Manor**
- Main Gate (Crossroads, Status Required)
- Servant Entrance (Discrete)

### Travel Routes

From Market Square Fountain:
- → Copper Kettle: 15 min walk, free
- → Noble District Gate: 25 min walk, free
- → Warehouse District: 30 min walk, 2 coin toll

From Noble District Gate:
- → Blackwood Manor: 10 min walk, requires Access Permit OR checkpoint pass

Alternative Route (discoverable):
- Warehouse District → Blackwood Manor Servant Entrance: 20 min, requires Shadow token with Marcus

## NPCs & Their Decks

### Elena (Devoted, Corner Table)

**Stats**: 12 base patience, Desperate state when deadline <2 hours
**Availability**: Afternoon and Evening only (2 PM - 10 PM)
**Not Available Morning**: Player must wait or do other activities

**Conversation Deck** (20 cards):

*Depth 0-5 (Starting Access)*:
- "Gentle Nod" - Comfort/Trust, D0, W0, Success: +1 comfort
- "Simple Agreement" - Comfort/Trust, D2, W1, Success: +2 comfort
- "I Understand" - Comfort/Trust, D3, W1, Success: +3 comfort
- "Active Listening" - Comfort/Trust, D5, W1, Success: +4 comfort, +1 Trust
- "Suggest Patience" - State/Trust, D4, W1, Success: Desperate→Tense
- "Past Failure Haunts" - Burden/Trust, D1, W2, Success: Remove, -2 comfort

*Depth 6-10*:
- "Promise Support" - Comfort/Trust, D7, W2, Success: +5 comfort, +1 Trust
- "Share Sympathy" - Comfort/Trust, D8, W2, Success: +6 comfort
- "Offer Comfort" - Comfort/Trust, D9, W1, Success: +4 comfort
- "Calm Her Nerves" - State/Trust, D10, W2, Success: Desperate→Neutral

*Depth 11-15*:
- "Deep Connection" - Comfort/Trust, D12, W3, Success: +8 comfort, +2 Trust
- "Heartfelt Promise" - Comfort/Trust, D14, W3, Success: +10 comfort, +2 Trust
- "Find Peace Together" - State/Trust, D13, W2, Success: Any→Open

*Depth 16-20*:
- "Perfect Trust" - Comfort/Trust, D18, W3, Success: +12 comfort, +3 Trust
- "Soul Bond" - State/Trust, D19, W3, Success: Any→Connected

**Letter Deck** (3 letters):

- "Desperate Refusal" - Letter/Trust, D8
  - Requires: 1+ Trust, Desperate state
  - Success: 4h deadline, position 2, 10 coins
  - Failure: 1h deadline, position 1, 15 coins

- "Formal Refusal" - Letter/Trust, D12
  - Requires: 3+ Trust, Open state  
  - Success: 6h deadline, position 3, 8 coins
  - Failure: 2h deadline, position 2, 12 coins

- "Personal Letter" - Letter/Trust, D16
  - Requires: 5+ Trust, Connected state
  - Success: 8h deadline, flexible position, 20 coins
  - Failure: 4h deadline, position 2, 20 coins

**Crisis Deck** (1 card, injected in Desperate):
- "Breaking Point" - Crisis/Trust, D0, W5 (0 in Desperate)
  - Success (30%): Remove crisis, Desperate→Tense
  - Failure (70%): Add 2 burden cards, conversation ends

### Marcus (Mercantile, Merchant Row)

**Stats**: 10 base patience, Neutral state
**Availability**: Morning through Evening (6 AM - 10 PM)
**Shop Closed at Night**: Cannot trade or converse during Night/Deep Night

**Conversation Deck** (20 cards):

*Depth 0-5*:
- "Fair Trade Talk" - Comfort/Commerce, D2, W1, Success: +2 comfort
- "Market Wisdom" - Comfort/Commerce, D4, W1, Success: +3 comfort, +1 Commerce
- "Profitable Ideas" - Comfort/Commerce, D5, W2, Success: +5 comfort

*Depth 6-10*:
- "Business Partnership" - Comfort/Commerce, D8, W2, Success: +6 comfort, +2 Commerce
- "Share Routes" - Comfort/Shadow, D9, W2, Success: +5 comfort, +1 Shadow
- "Trust in Trade" - State/Commerce, D7, W1, Success: Any→Eager

*Depth 11-15*:
- "Exclusive Deal" - Comfort/Commerce, D13, W3, Success: +9 comfort, +3 Commerce
- "Secret Knowledge" - Comfort/Shadow, D14, W3, Success: +8 comfort, +2 Shadow

**Letter Deck** (2 letters):

- "Delivery Contract" - Letter/Commerce, D6
  - Requires: 2+ Commerce, Eager state
  - Success: 6h deadline, position 3, 12 coins
  - Failure: 3h deadline, position 2, 12 coins

- "Access Permit Request" - Letter/Shadow, D10
  - Requires: 3+ Shadow, any state
  - Success: Gain Noble District Access Permit
  - Failure: Must pay 10 coins for permit

**Exchange Deck** (5 cards):

- "Buy Provisions" - 3 coins → Hunger = 0
- "Purchase Access" - 15 coins → Noble District Permit  
- "Trade Information" - Guard Intel → Alternative Route Knowledge
- "Quick Delivery" - Accept → New obligation (8 coins, 3h deadline)
- "Buy Medicine" - 5 coins → Health +20

### Guard Captain (Steadfast, Guard Post)

**Stats**: 11 base patience, Tense state (checkpoint duty)
**Availability**: Always (different shifts affect state)
- Day shift (6 AM - 6 PM): Tense state, strict
- Night shift (6 PM - 6 AM): Neutral state, more lenient

**Conversation Deck** (15 cards):

*Depth 0-5*:
- "Show Papers" - Comfort/Status, D1, W1, Success: +2 comfort
- "Explain Purpose" - Comfort/Trust, D3, W1, Success: +3 comfort
- "Official Business" - State/Status, D4, W2, Success: Tense→Neutral

*Depth 6-10*:
- "Bribe Subtly" - Comfort/Shadow, D7, W2, Success: +5 comfort, +2 Shadow
- "Name Drop" - Comfort/Status, D9, W2, Success: +6 comfort, +1 Status

**Letter Deck** (1 letter):

- "Checkpoint Pass" - Letter/Shadow, D8
  - Requires: 3+ Shadow, Neutral state
  - Success: 24h Noble District access
  - Failure: 2h access only

**Crisis Deck** (if caught lying):
- "Arrest Threat" - Crisis/Status, D0, W5
  - Success (40%): Talk your way out
  - Failure (60%): -10 coins bribe or conversation ends

### Lord Blackwood (Proud, Blackwood Manor)

**Stats**: 8 base patience, Dismissive state (not expecting courier)
**Availability**: Until 5 PM (leaves at sunset)

**Not fully implemented for POC - just receives letter**

### Bertram (Mercantile, Copper Kettle Bar)

**Stats**: 10 base patience, Neutral state
**Availability**: Always (lives above tavern)

**Exchange Deck** (5 cards):
- "Stay the Night" - 5 coins → Full rest (Attention=10, Hunger=20, Health+20, skip to morning)
- "Quick Nap" - 2 coins → +3 attention, advance 2 hours  
- "Hot Meal" - 2 coins → Hunger = 0
- "Rent Private Room" - 8 coins → Full rest with complete health restoration
- "Noble Gossip" - 3 coins → Learn Lord Blackwood's exact departure time

**No conversation deck for POC** - purely exchange-based NPC

## Observations Available

### Market Square - Fountain

**Morning**:
- "Early Bird Merchants" (1 attention)
  - Creates: Commerce card, D0, W1, Success: +3 comfort
  - Info: Best deals available at dawn

**Afternoon**:
- "Guard Shift Schedule" (1 attention)
  - Creates: Shadow card, D0, W1, Success: +4 comfort
  - Info: Guards change at 4pm, less strict night shift

**Evening**:
- "Noble Carriage Preparing" (1 attention)
  - Creates: Status card, D2, W1, Success: +3 comfort, +1 Status
  - Info: Lord Blackwood leaving at sunset (5pm)

### Market Square - Merchant Row

**Any Time Period**:
- "Hidden Route Gossip" (1 attention)
  - Creates: Commerce card, D3, W1, Success: +4 comfort
  - Info: Warehouse workers know servant passages

### Copper Kettle - Corner Table

**Afternoon/Evening** (when Elena present):
- "Elena's Family Seal" (1 attention)  
  - Creates: Status card, D4, W2, Success: +6 comfort
  - Info: High noble family, adds weight to letter

### Noble District Gate - Checkpoint

**Day Shift**:
- "Strict Inspection" (1 attention)
  - Creates: Shadow card, D2, W1, Success: +2 comfort
  - Info: Day guards check everything thoroughly

**Night Shift**:
- "Bribery in Action" (1 attention)
  - Creates: Shadow card, D1, W1, Success: +3 comfort, +1 Shadow
  - Info: 10 coins standard bribe for passage

Note: Observations refresh each time period. Once taken, unavailable until next period. Cards decay over time (Fresh 0-2h, Stale 2-6h, Expired 6+h).

## Strategic Paths

### Path A: Morning Optimization (Balanced Resource Management)
1. Work at Merchant Row (2 attention → 8 coins, advances to Midday)
2. Exchange with Marcus: Buy provisions (3 coins, hunger→0, removes -4 patience)
3. Wait until Afternoon (0 attention, time passes)
4. Converse with Elena (2 attention, now 13 patience without hunger penalty)
5. Build comfort to negotiate good letter terms
6. Travel to checkpoint (25 min)
7. Bribe guards (10 coins) or converse for pass
8. Deliver to Blackwood (10 min)
Total: 5 attention, 5 coins spent, arrive ~4 PM

### Path B: Direct Rush (Time-Focused)
1. Exchange with Marcus: Buy provisions immediately (3 coins)
2. Wait until Afternoon (advance 5 hours)
3. Rush Elena conversation (2 attention, accept any terms)
4. Observe for leverage card (1 attention)
5. Travel directly to checkpoint
6. Bribe guards (remaining 7 coins won't cover)
7. Must converse with Guard Captain for pass (2 attention)
Total: 5 attention used, risky on coins

### Path C: Rest and Grind (Maximum Optimization)
1. Work at Merchant Row (2 attention → 8 coins, now Midday)
2. Work again at Bar (2 attention → 8 coins, now Afternoon)
3. Exchange with Bertram: Quick Nap (2 coins → +3 attention)
4. Exchange with Marcus: Buy provisions (3 coins)
5. Full Elena conversation with maximum patience (2 attention)
6. Build tokens for future while getting letter
7. Plenty of coins for bribes and flexibility
Total: Uses rest system to extend capability

### Path D: Token Investment (Long-term Building)
1. Morning conversation with Marcus (2 attention, build Commerce tokens)
2. Work at Merchant Row (2 attention → 8 coins)
3. Wait for Elena (0 attention)
4. Use Commerce tokens for better Marcus exchanges later
5. Shadow tokens help with Guard Captain
6. Creates foundation for future obligations
Total: Sacrifices immediate optimization for relationships

## Resource Management Challenges

**The Resource Conversion Loop**:
- Time → Money (Work: 4 hours → 8 coins)
- Money → Attention (Rest: 2 coins → 3 attention)
- Attention → Progress (Conversations, observations)
- Progress → Money (Complete obligations for payment)

**Starting Scarcity**:
- 10 coins: Exactly enough for checkpoint bribe OR food + rest
- 5 attention: Only 2 conversations without rest
- 80 hunger: -4 patience makes conversations brutal
- Morning start: Elena unavailable for 5 hours

**Time Pressure Layers**:
- Lord Blackwood leaves: 8 hours (hard deadline)
- Elena available: 5 hours (soft deadline - waiting required)
- Work advances time: Each work action costs 4 hours
- Rest advances time: Full rest skips to next morning (too late!)

**Strategic Decisions**:
- Work early for resources but lose time?
- Fix hunger immediately or endure penalty?
- Wait for Elena or optimize morning time?
- Rest for more attention or preserve coins?

## Obligation Queue Complications

If player accepts Marcus's "Quick Delivery" obligation:
- Position 1: Marcus's delivery (3h deadline)
- Position 2: Elena's letter (1h 13m)
- Must burn tokens to reorder or complete Marcus first

## Failure Conditions

**Total Failure**: Lord Blackwood leaves before delivery (5 PM deadline)
- Elena becomes Hostile (permanent)
- Lose all Trust tokens with Elena
- Add 3 burden cards to her deck
- No payment
- Cannot interact with Elena for 24 hours

**Partial Failure**: Accept terrible letter terms
- 1-hour deadline from receipt (extreme pressure)
- Forces position 1 in queue (disrupts everything)
- Must pay premium (15+ coins) for urgent service
- Still counts as success if delivered

**Resource Death Spiral**: 
- Run out of attention with no coins to rest
- Run out of coins with urgent obligations pending
- Hunger reaches 100 (collapse, lose half health)

## Success Conditions

**Perfect Success**:
- Get Elena's letter with excellent terms (6+ hour deadline)
- Maintain flexible queue position
- Complete delivery with 1+ hours to spare
- Earn 15+ coins profit after expenses
- Build tokens for future interactions

**Good Success**:
- Negotiate decent terms (3-4 hour deadline)
- Deliver before Lord Blackwood leaves
- Maintain positive coin balance
- Keep all relationships intact

**Acceptable Success**:
- Deliver Elena's letter by any means necessary
- Even with poor terms and damaged relationships
- Proves you can complete the core obligation

## Strategic Depth Demonstration

The POC showcases how simple mechanics create complex decisions:

**Work/Rest/Wait Trinity**:
- Work converts time to money but risks deadlines
- Rest converts money to attention but costs precious coins  
- Wait costs nothing but pure opportunity

**NPC Availability Creates Scheduling Puzzle**:
- Elena unavailable mornings forces time management
- Marcus closed at night limits trading windows
- Guard shifts affect negotiation difficulty

**Resource Conversion Prevents Soft-locks**:
- Always have options (work, rest, wait)
- But each option has meaningful cost
- No single optimal path

This transforms Wayfarer from a tight optimization puzzle into a strategic resource management game where player agency and deadline pressure create emergent challenge.