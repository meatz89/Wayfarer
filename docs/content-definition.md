# Wayfarer POC - Complete Content Definition

## Strategic Challenge Overview

The player must deliver Elena's letter to Lord Blackwood before he leaves the city (1h 13m deadline). The core challenge: Lord Blackwood is in the Noble District, which requires either an Access Permit, checkpoint bribe, or alternative route. With only 7 attention, 23 coins, and 60 hunger, every decision matters.

## Starting Conditions

**Player Resources**:
- Coins: 23
- Health: 75 
- Hunger: 60 (-3 patience penalty)
- Attention: 7/10
- Satchel: Empty (5 slots)
- Tokens: 1 Trust with Elena, 2 Commerce with Marcus, 1 Shadow with Guard Captain

**Time**: Tuesday 3:47 PM (Afternoon period)
**Starting Location**: Market Square - Fountain

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

**Stats**: 12 base patience, Desperate state (deadline <2 hours)

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

**Not fully implemented for POC - just receives letter**

## Observations Available

### Market Square - Fountain (Afternoon)

- "Guard Shift Schedule" (1 attention)
  - Creates: Shadow card, D0, W1, Success: +4 comfort
  - Info: Guards change at 4pm, less strict night shift

- "Noble Carriage Preparing" (1 attention)
  - Creates: Status card, D2, W1, Success: +3 comfort, +1 Status
  - Info: Lord Blackwood leaving at sunset (5pm)

### Market Square - Merchant Row

- "Hidden Route Gossip" (1 attention)
  - Creates: Commerce card, D3, W1, Success: +4 comfort
  - Info: Warehouse workers know servant passages

### Copper Kettle - Corner Table

- "Elena's Family Seal" (1 attention)  
  - Creates: Status card, D4, W2, Success: +6 comfort
  - Info: High noble family, adds weight to letter

### Noble District Gate - Checkpoint

- "Bribery in Action" (1 attention)
  - Creates: Shadow card, D1, W1, Success: +3 comfort, +1 Shadow
  - Info: 10 coins standard bribe for passage

## Strategic Paths

### Path A: Direct Bribery (Fastest, Expensive)
1. Quick Exchange with Marcus: Buy provisions (3 coins, hunger→0)
2. Minimal Elena conversation: Accept Desperate letter (poor terms)
3. Travel to checkpoint (25 min)
4. Bribe guards (10 coins) or use checkpoint pass
5. Reach Blackwood (10 min)
Total: 35 min travel, 13+ coins, 2 attention

### Path B: Optimal Negotiation (Balanced)
1. Observe at Fountain for leverage (1 attention)
2. Exchange with Marcus: Buy provisions (3 coins)
3. Full Elena conversation: Build comfort for better letter terms (2 attention)
4. Build Shadow tokens with Guard Captain (2 attention)
5. Get checkpoint pass through conversation
6. Deliver with good payment terms
Total: 45 min, 3 coins, 5 attention

### Path C: Alternative Route (Token Heavy)
1. Build Commerce/Shadow with Marcus (2 attention)
2. Learn alternative route through conversation
3. Quick Elena conversation (2 attention)
4. Travel through Warehouse District (30 min + 20 min)
5. Enter through servant entrance
Total: 50 min travel, 2 coins toll, 4 attention

## Resource Management Challenges

**Hunger Pressure**: Starting at 60 hunger (-3 patience) makes conversations harder. Must decide whether to spend coins on food.

**Attention Scarcity**: Only 7 attention for:
- Observations (1 each)
- Conversations (2 each)
- Managing multiple obligations

**Time Pressure**: 1h 13m for Elena, but travel alone takes 35-50 min depending on route.

**Token Investment**: Better success rates and letter terms require token building, but costs precious attention.

## Obligation Queue Complications

If player accepts Marcus's "Quick Delivery" obligation:
- Position 1: Marcus's delivery (3h deadline)
- Position 2: Elena's letter (1h 13m)
- Must burn tokens to reorder or complete Marcus first

## Failure Conditions

**Total Failure**: Elena's deadline expires
- Elena becomes Hostile
- Lose all Trust tokens with Elena
- Add 3 burden cards to her deck
- No payment

**Partial Success**: Deliver with poor terms
- Forced position 1 (disrupts other obligations)
- Higher payment but damaged relationships
- 1-hour deadline pressure

## Success Conditions

**Optimal Success**:
- Negotiate good terms with Elena (4+ hour deadline)
- Maintain queue position flexibility
- Preserve tokens with all NPCs
- Complete delivery with time to spare
- Earn 15+ coins profit after expenses

**Acceptable Success**:
- Deliver Elena's letter before deadline
- Any payment received
- Relationships intact for future play

## Content Scaling Notes

This POC uses minimum content to demonstrate:
- Depth gating (comfort 0-20 range)
- Token progression (+5% per token)
- Obligation queue management
- Resource interconnections
- Multiple strategic paths
- Meaningful failure states

Full game would add:
- More NPCs per location
- Deeper conversation decks (30+ cards)
- Complex multi-party obligations
- Seasonal events affecting routes
- Reputation system affecting starting states