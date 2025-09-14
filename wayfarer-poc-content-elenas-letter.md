# Wayfarer POC: Elena's Letter - Complete Content

## Table of Contents
1. [Scenario Overview](#scenario-overview)
2. [Starting Conditions](#starting-conditions)
3. [Player Starting Deck](#player-starting-deck)
4. [NPCs](#npcs)
5. [Locations and Spots](#locations-and-spots)
6. [Observation Cards](#observation-cards)
7. [Request Cards](#request-cards)
8. [Exchange Cards](#exchange-cards)
9. [Routes and Travel](#routes-and-travel)
10. [The Only Successful Path](#the-only-successful-path)
11. [Failure Analysis](#failure-analysis)
12. [JSON Implementation](#json-implementation)

## Scenario Overview

### The Story
Elena, a young scribe at the Copper Kettle Tavern, desperately needs to refuse an arranged marriage. Lord Blackwood, who could intervene, leaves the city at 5:00 PM. The player must navigate complex systems to help Elena deliver her letter before the deadline.

### The Core Challenge
This POC demonstrates how conversations are the primary gameplay loop. The player uses their starting conversation deck against different NPC personality rules. Success requires understanding how each personality transforms the basic conversation puzzle and building resources to unlock signature cards that make harder conversations possible.

### The Discovery
Every seemingly inefficient action (investigating twice, buying food before working, building infrastructure before the main quest) is actually essential. The optimal path emerges through understanding system interactions, not obvious choices. The player literally cannot reach Elena's request card without proper preparation.

### Success Criteria
- Build Market Square familiarity through investigation
- Gain Commerce tokens to unlock Marcus's signature cards  
- Use observation cards to advance Elena's connection state
- Manage resources with perfect precision
- Complete Elena's letter delivery before 5:00 PM

## Starting Conditions

### Player State
- **Time**: 9:00 AM Tuesday (Morning block, segment 4)
- **Segments Remaining**: 1 in current block, 13 in day total
- **Coins**: 0
- **Hunger**: 50
- **Health**: 100
- **Satchel Weight**: 3/10 (Viktor's package weighs 3)
- **All Tokens**: 0 with all NPCs
- **All Familiarity**: 0 at all locations
- **Connection States**: All NPCs at default
- **Card XP**: All starting cards at 0 XP (level 1)

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
- Total segments available: 13 (1 morning + 4 midday + 4 afternoon + 4 evening partial)
- Optimal path uses: 12 segments
- Buffer: 1 segment

## Player Starting Deck

The player begins with a 20-card starter deck. This deck represents their basic social repertoire and will be used in ALL conversations, modified by NPC personality rules and any unlocked signature cards.

### Complete Starting Deck (20 Cards)

#### Safe Building Cards (8 cards)
**"I hear you"** (3 copies)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: +1 rapport on success
- Failure: No effect
- Persistence: No
- Purpose: Reliable rapport building, flexible focus use

**"Let me think"** (2 copies)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: No rapport, sets Patient atmosphere
- Failure: Forces LISTEN
- Persistence: Yes (survives LISTEN)
- Purpose: Atmosphere setup for longer conversations

**"I understand"** (3 copies)
- Focus: 2
- Difficulty: Easy (70% base success)
- Effect: +2 rapport on success
- Failure: Forces LISTEN
- Persistence: No
- Purpose: Efficient rapport with manageable risk

#### Risk/Reward Cards (6 cards)
**"How can I assist?"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base success)
- Effect: +2 rapport on success
- Failure: -1 rapport, forces LISTEN
- Persistence: No
- Purpose: Standard conversation advancement

**"Trust me on this"** (2 copies)
- Focus: 3
- Difficulty: Medium (60% base success)
- Effect: +3 rapport on success
- Failure: -1 rapport, forces LISTEN
- Persistence: No
- Purpose: Bigger plays for momentum

**"Everything will work out"** (2 copies)
- Focus: 4
- Difficulty: Hard (50% base success)
- Effect: +4 rapport on success
- Failure: -2 rapport, forces LISTEN
- Persistence: No
- Purpose: High-risk finishing moves

#### Utility Cards (6 cards)
**"Tell me more"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base success)
- Effect: Draw 2 cards
- Failure: Forces LISTEN
- Persistence: No
- Purpose: Find key cards

**"Let me prepare"** (2 copies)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: +1 focus to current pool
- Failure: Forces LISTEN
- Persistence: Yes
- Purpose: Enable bigger plays

**"We can figure this out"** (2 copies)
- Focus: 3
- Difficulty: Hard (50% base success)
- Effect: +X rapport where X = patience ÷ 3
- Failure: Forces LISTEN
- Persistence: No
- Purpose: Scaling reward for early play

### Starting Deck Strategy

With 20 cards and most being non-persistent, the rhythm becomes:
1. Draw initial hand (3-5 cards based on connection state)
2. Play cards while building rapport
3. Success maintains hand, failure forces LISTEN and new cards
4. Find the right combination to reach goal thresholds

The lack of persistence means players must seize opportunities when they appear. Drawing "Everything will work out" early with full patience might be the only chance to play it before it's swept away by a forced LISTEN.

## NPCs

### Elena - The Desperate Scribe

**Basic Properties**:
- ID: `elena`
- Name: Elena
- Profession: Scribe
- Location: Copper Kettle Tavern, Corner Table spot
- Personality: DEVOTED
- Personality Rule: "When rapport would decrease, decrease it twice"
- Starting State: DISCONNECTED (3 focus, draws 3)
- Token Type: Trust

**Story Context**:
Young woman facing forced marriage to a merchant she despises. Works at her uncle's tavern while pursuing her writing. Lord Blackwood could intervene due to an old debt to her late father.

**Signature Cards** (Not available in POC - player has 0 Trust tokens):
- 1 token: "Elena's Faith"
- 3 tokens: "Shared Understanding"
- 6 tokens: "Elena's Trust"
- 10 tokens: "Emotional Bond"
- 15 tokens: "Elena's Devotion"

**Request Details**:
- Request available at 5 focus capacity (requires NEUTRAL state)
- Basic goal: 5 rapport (delivers letter weight 1, no payment)
- Enhanced goal: 10 rapport (delivers letter weight 2, 1 Trust token)
- Premium goal: 15 rapport (delivers legal documents weight 3, 2 Trust tokens)

**Conversation Challenge**:
The Devoted rule makes Elena's conversation particularly tense. Every failure hurts twice as much, and forcing LISTEN after failure means you lose your hand while in a worse position. Without Trust tokens to unlock her signature cards, the player must rely on their base deck and the observation card advantage.

### Marcus - The Merchant

**Basic Properties**:
- ID: `marcus`
- Name: Marcus
- Profession: Merchant
- Location: Market Square, Merchant Row spot
- Personality: MERCANTILE
- Personality Rule: "Your highest focus card each turn gains +30% success"
- Starting State: NEUTRAL (5 focus, draws 4)
- Token Type: Commerce

**Story Context**:
Established trader who runs regular caravans. Cannot leave his stall (merchandise would be stolen). Values reliable business partners.

**Signature Cards**:
- 1 token: "Marcus's Bargain" (3 focus, +3 rapport, efficient)
- 3 tokens: "Trade Knowledge" (1 focus, draw 2 cards)
- 6 tokens: "Commercial Trust" (4 focus, +5 rapport)
- 10 tokens: "Marcus's Favor" (5 focus, doesn't force LISTEN on failure)
- 15 tokens: "Master Trader's Secret" (0 focus, set Mercantile atmosphere)

**Request Details**:
- Letter to Warehouse District (weight 1)
- Basic goal: 5 rapport (5 coins, 1 Commerce token)
- Enhanced goal: 10 rapport (8 coins, 2 Commerce tokens)
- Premium goal: 15 rapport (12 coins, 3 Commerce tokens)

**Exchange Deck**:
- Buy Simple Food: 2 coins → -50 hunger (immediate consumption)
- Buy Bread: 3 coins → bread item (weight 1, -30 hunger when consumed)
- Join Merchant Caravan: 10 coins → Transport to Noble Quarter (requires 2+ Commerce tokens)

**Conversation Challenge**:
The Mercantile rule rewards high-focus plays. "Everything will work out" (4 focus) becomes 80% success instead of 50%. This encourages gambling on big cards rather than safe small plays.

### Lord Blackwood - The Noble

**Basic Properties**:
- ID: `lord_blackwood`
- Name: Lord Blackwood
- Profession: Noble
- Location: Noble Quarter, Blackwood Manor spot
- Personality: PROUD
- Personality Rule: "Cards must be played in ascending focus order each turn"
- Starting State: NEUTRAL
- Token Type: Status

**Story Context**:
Influential noble who owes a debt to Elena's late father. Leaving for summer estate at 5:00 PM sharp. Will accept Elena's letter immediately due to noble seal.

**Conversation Challenge**:
Not relevant for POC - Elena's letter has a noble seal allowing quick delivery without conversation.

### Warehouse Recipient

**Basic Properties**:
- ID: `warehouse_clerk`
- Name: Warehouse Clerk
- Profession: Clerk
- Location: Warehouse District, Warehouse Entrance
- Personality: STEADFAST
- Personality Rule: "Rapport changes are capped at ±2 per card"
- Starting State: NEUTRAL
- Token Type: Commerce

Accepts deliveries without conversation needed.

### Guard Captain (Dead End)

**Basic Properties**:
- ID: `guard_captain`
- Name: Guard Captain
- Profession: Guard
- Location: Market Square, Guard Post
- Personality: PROUD
- Personality Rule: "Cards must be played in ascending focus order each turn"
- Starting State: GUARDED
- Token Type: Shadow

**Exchange Deck**:
- Noble District Permit: 20 coins (impossible to afford - deliberate dead end)

## Locations and Spots

Location mechanics remain unchanged from the original POC. The key interactions:

### Market Square
- **Fountain**: Investigation point (Quiet in morning for +2 familiarity, Busy in midday/afternoon for +1)
- **Merchant Row**: Marcus location, work available
- **Guard Post**: Guard Captain, permit exchange (dead end)

### Copper Kettle Tavern
- **Corner Table**: Elena location (Private spot, +1 patience during conversations)

### Noble Quarter
- **Blackwood Manor**: Lord Blackwood location (until 5 PM)

### Warehouse District
- **Warehouse Entrance**: Delivery point for Marcus's letter

## Observation Cards

Observation cards now mix into the player's conversation deck when conversing with the relevant NPC.

### Safe Passage Knowledge

**Properties**:
- ID: `safe_passage_knowledge`
- Name: "Safe Passage Knowledge"
- Source: Market Square first observation (familiarity 1+)
- Destination: Added to deck when conversing with Elena
- Focus: 0
- Difficulty: Very Easy (85% base)
- Effect: Advance Elena's connection state from DISCONNECTED to NEUTRAL
- Persistence: Yes (survives LISTEN)
- Consumed: When played successfully

**Critical Importance**: Without this card, Elena remains in DISCONNECTED state (3 focus capacity), making her 5-focus request card impossible to reach. This observation card is mandatory for success.

### Merchant Caravan Route

**Properties**:
- ID: `merchant_caravan_route`
- Name: "Merchant Caravan Route"
- Source: Market Square second observation (familiarity 2+, requires first observation)
- Destination: Added to deck when conversing with Marcus
- Focus: 0
- Difficulty: Very Easy (85% base)
- Effect: Unlock Marcus's caravan exchange option
- Persistence: Yes
- Consumed: When played successfully

**Strategic Value**: Enables caravan transport to Noble Quarter for 10 coins (cheaper than 20-coin permit).

## Request Cards

Request cards represent conversation goals. Multiple thresholds exist, but the player must declare which they're attempting when it becomes playable.

### Elena's Urgent Letter

**Conversation Requirements**:
- Connection state: NEUTRAL or better (5 focus capacity)
- Must build rapport to threshold through card play

**Goal Thresholds**:
- **Basic** (5 rapport): Accept letter (weight 1), no payment, standard urgency
- **Enhanced** (10 rapport): Accept priority letter (weight 2), 1 Trust token reward
- **Premium** (15 rapport): Accept legal documents (weight 3), 2 Trust tokens, guaranteed success

**The Challenge**: 
Starting in DISCONNECTED with Elena's Devoted personality rule (failures hurt twice), reaching even the basic 5 rapport threshold is difficult. The player needs the Safe Passage Knowledge observation card to advance to NEUTRAL state, giving them 5 focus capacity and 4-card draws on LISTEN.

**Weight Implications**:
The premium tier creates a heavier package (3 weight) representing the additional legal documents Elena provides. This weight affects travel options - the "Narrow Alley" shortcut becomes impassable, forcing use of main roads or expensive transport.

### Marcus's Trade Letter

**Goal Thresholds**:
- **Basic** (5 rapport): Deliver to Warehouse (weight 1), 5 coins, 1 Commerce token
- **Enhanced** (10 rapport): Priority delivery (weight 1), 8 coins, 2 Commerce tokens
- **Premium** (15 rapport): Rush delivery (weight 2), 12 coins, 3 Commerce tokens

**Strategic Note**: 
With Marcus's Mercantile rule, high-focus cards gain +30% success. This makes his enhanced goal achievable with good card play. Gaining 2 Commerce tokens unlocks his first two signature cards for future conversations.

## Exchange Cards

### Marcus's Exchange Deck

**Buy Simple Food**
- Cost: 2 coins
- Effect: -50 hunger (immediate consumption)
- Weight: N/A (consumed immediately)
- Availability: Always

**Buy Bread**
- Cost: 3 coins  
- Effect: Bread item (weight 1)
- When consumed: -30 hunger
- Availability: Always

**Join Merchant Caravan**
- Cost: 10 coins
- Effect: Transport to Noble Quarter
- Requirements: 2+ Commerce tokens
- Time: 2 segments
- Availability: After playing Merchant Caravan Route observation

### Guard Captain's Exchange Deck

**Noble District Permit**
- Cost: 20 coins
- Effect: Permit item (weight 1)
- Availability: Always (but impossible to afford in POC)

## Routes and Travel

### Market Square to Warehouse District

**Segment 1 Path Cards**:
- **Main Road**: 1 segment, 1 stamina, no weight restrictions
- **Back Alley**: 0 segments, 2 stamina, blocked over 7 weight
- **Merchant Cart**: 1 segment, 2 coins OR free with trade goods

**Segment 2 Path Cards**:
- **Warehouse Direct**: 1 segment, 1 stamina
- **Loading Docks**: 0 segments, 2 stamina, blocked over 8 weight
- **Struggle Path**: 2 segments, 0 stamina, always available

### Market Square to Noble Quarter

**Direct Route (Blocked)**:
- **Guard Checkpoint**: Requires Noble District Permit
- Cannot proceed without permit

**Caravan Route (Requires 2+ Commerce tokens)**:
- **Merchant Caravan**: 2 segments total, 10 coins
- No weight restrictions on caravan
- Comfortable travel

### Market Square to Copper Kettle Tavern

**Single Segment Route**:
- **Direct Path**: 1 segment, 1 stamina
- **Market Route**: 1 segment, 2 stamina, allows one quick exchange
- **Busy Street**: 1 segment, 0 stamina, +5 hunger

## The Only Successful Path

The path uses the correct time blocks and segment constraints.

### Complete Timeline

#### Morning Block (6:00 AM - 10:00 AM)
**Starting at 9:00 AM = segment 4 of Morning block**
**1 segment remaining in block**

**9:00 AM - Investigate Market Square** (Segment 4)
- Spot: Fountain (QUIET in morning)
- Cost: 1 segment
- Result: +2 familiarity (0→2)
- Weight carried: 3 (Viktor's package)
- **Morning block ends, advances to Midday**

#### Midday Block (10:00 AM - 2:00 PM)
**4 segments available**

**10:00 AM - First Observation** (Segment 1)
- Cost: 1 segment
- Gain: "Safe Passage Knowledge" observation card
- Will be added to deck when conversing with Elena

**10:10 AM - Converse with Marcus** (Segment 2)
- Cost: 1 segment base
- Personality Rule: Highest focus card gains +30% success
- Starting deck: 20 player cards (no signatures yet - 0 Commerce tokens)
- Connection state: NEUTRAL (5 focus, draws 4)
- Deliver Viktor's package (frees 3 weight)
- Accept Marcus's letter (weight 1)
- Play pattern: Use high-focus cards for Mercantile bonus
- Try for enhanced goal for 2 Commerce tokens

**10:30 AM - Travel to Warehouse** (Segment 3)
- Weight carried: 1 (Marcus's letter)
- Path choices benefit from light load
- Use "Back Alley" (0 segments) if discovered
- Otherwise "Main Road" (1 segment)

**10:40 AM - Deliver and Return** (Segment 4)
- Deliver Marcus's letter
- Gain: 8 coins (enhanced completion), 2 Commerce tokens
- Return to Market Square
- **Midday block ends, advances to Afternoon**

#### Afternoon Block (2:00 PM - 6:00 PM)
**4 segments available**
**Critical: Lord Blackwood leaves at 5:00 PM (segment 4 of this block)**

**2:00 PM - Buy Food and Work Decision**
- Quick exchange with Marcus: 2 coins for immediate meal
- Hunger: 50→0
- **Cannot work** - would consume entire block, missing deadline
- Must proceed with 6 coins remaining

**2:10 PM - Second Investigation** (Segment 1)
- Fountain now BUSY: Only +1 familiarity  
- Familiarity: 2→3
- Cost: 1 segment

**2:20 PM - Second Observation** (Segment 1 continued)
- Gain: "Merchant Caravan Route" observation card
- No additional segment cost

**2:30 PM - Quick Conversation with Marcus** (Segment 2)
- Cost: 1 segment
- Deck: 20 player cards + 2 Marcus signature cards (from 2 Commerce tokens)
- Marcus's Bargain and Trade Knowledge now available
- Play Merchant Caravan Route observation card
- Unlocks caravan exchange option
- **Cannot afford caravan** (only 6 coins, need 10)

**2:40 PM - Travel to Copper Kettle** (Segment 3)
- Weight: 0 (no items carried)
- Direct path: 1 segment

**3:00 PM - Critical Conversation with Elena** (Segment 4)
- Cost: 1 segment base
- Personality Rule: Failures decrease rapport twice
- Starting State: DISCONNECTED (3 focus, draws 3)
- Deck: 20 player cards + Safe Passage Knowledge
- Turn 1: Play Safe Passage Knowledge (0 focus, 85% success)
- Effect: Advance to NEUTRAL state (5 focus, draws 4)
- Now can attempt 5-focus request
- Build to 5 rapport carefully (failures hurt double)
- Accept basic goal: Elena's letter (weight 1)
- **Afternoon block ends, advances to Evening**

#### Evening Block (6:00 PM - 10:00 PM)
**Cannot use - Lord Blackwood left at 5:00 PM**

### Alternative Successful Path (With Work)

#### Morning Block (Starting 9:00 AM)
**9:00 AM - Investigate** (Segment 4)
- +2 familiarity at quiet fountain
- **Block advances to Midday**

#### Midday Block (10:00 AM - 2:00 PM)
**10:00 AM - Work Action**
- Consumes entire block (4 segments)
- At hunger 70: Output = 5 - floor(70/25) = 5 - 2 = 3 coins
- **Block advances to Afternoon**

#### Afternoon Block (2:00 PM - 6:00 PM)
**2:00 PM - Rush Sequence** (Segment 1)
- Buy food: 2 coins (1 coin remaining)
- Second investigation: +1 familiarity (now at 3)
- Both observations available

**2:10 PM - Marcus Conversation** (Segment 2)
- Deliver Viktor's package
- Enhanced goal: 8 coins, 2 Commerce tokens
- Total: 9 coins

**2:30 PM - Travel to Warehouse** (Segment 3)
- Deliver Marcus's letter
- Return to Market Square

**3:00 PM - Elena Conversation** (Segment 4)
- Use Safe Passage Knowledge
- Reach basic goal
- Accept letter

**3:30 PM - Direct Route Attempt**
- Only 9 coins, cannot afford 20-coin permit
- **FAILURE** - Cannot reach Noble Quarter

### Why Only One Path Works

The constraint is the 5:00 PM deadline falling in segment 4 of the Afternoon block. Working consumes an entire block, making it impossible to complete all necessary steps and still deliver before the deadline unless perfectly sequenced. The lack of 10 coins for caravan transport forces reliance on the impossible guard checkpoint route.

### Resource Accounting

**Segment Usage** (13 total available from 9 AM):
- Morning Block: 1 used (investigation)
- Midday Block: 4 used (observation, conversation, travel, delivery)
- Afternoon Block: 4 used (investigation, conversation, travel, conversation)
- **Total**: 9 segments used, 4 buffer remaining
- **Critical**: Must complete Elena conversation by segment 4 of Afternoon

**Weight Management**:
- Start: 3/10 (Viktor's package)
- After Marcus delivery: 1/10 (Marcus's letter)
- After Warehouse: 0/10 (empty)
- After Elena: 1/10 (Elena's letter)
- Never exceeded capacity, all paths accessible

**Card Experience Gained**:
Each successful card play during conversations grants 1 XP to that specific card. Over the course of the POC, players will likely gain 10-15 XP across various cards, beginning their progression journey.

## Failure Analysis

### Common Failure Paths

#### Work at Wrong Time
- Working in Morning wastes quiet investigation bonus
- Working in Midday makes Afternoon delivery impossible
- Working in Afternoon guarantees missing deadline
- The 4-segment block consumption is absolute

#### Rush to Elena First
- Elena in DISCONNECTED (3 focus, draws 3)
- Without Safe Passage Knowledge, stuck at 3 focus
- Cannot reach 5-focus request
- Even with perfect play, cannot accept letter

#### Skip Investigation
- No Safe Passage Knowledge observation
- Elena remains DISCONNECTED
- Mathematically impossible to succeed

#### Ignore Marcus's Enhanced Goal
- Only get 1 Commerce token
- Only unlocks Marcus's first signature card
- Cannot afford caravan even if unlocked
- Less coins for crucial purchases

#### Try Guard Checkpoint
- Permit costs 20 coins
- Maximum achievable: 11 coins (3 from work + 8 from Marcus)
- Designed as impossible dead end

#### Poor Conversation Play with Elena
- Devoted rule: Failures hurt twice
- One failed 3-focus card: -2 rapport AND forces LISTEN
- Lose assembled hand, must rebuild from worse position
- May run out of patience before reaching even basic goal

#### Accept Heavy Obligations Early
- Marcus offers "Silk Bundle Delivery" (weight 5, pays 15 coins)
- Accepting limits travel options severely
- "Narrow Alley" shortcuts become impassable
- Forces expensive main roads
- May prevent accepting Elena's letter

### Why This Path is Unique

The new conversation system and resource management make the strategic requirements even clearer:

1. **Must investigate** for observation card to unlock Elena's capacity
2. **Must build Commerce tokens** to unlock Marcus's signature cards
3. **Cannot work** due to 4-segment block consumption
4. **Must use Marcus's enhanced delivery** for coins
5. **Must manage weight** to keep travel options open
6. **Must complete within Afternoon block** before 5 PM deadline
7. **Must play Elena conversation perfectly** due to Devoted penalty

Each element directly supports the core conversation gameplay loop while the time pressure makes every segment precious.

## JSON Implementation

### Package Structure

```json
{
  "packageId": "poc_elenas_letter_v3",
  "metadata": {
    "name": "Elena's Letter POC v3",
    "version": "3.0.0",
    "description": "POC with weight system and time segments",
    "author": "Wayfarer Team",
    "timestamp": "2025-01-01T00:00:00Z"
  },
  "startingConditions": {
    "time": {
      "hour": 9,
      "minute": 0,
      "dayOfWeek": "Tuesday",
      "timeBlock": "Morning",
      "blockSegment": 4,
      "totalSegmentsRemaining": 13
    },
    "resources": {
      "coins": 0,
      "health": 100,
      "hunger": 50
    },
    "satchel": {
      "maxWeight": 10,
      "currentWeight": 3,
      "items": [
        {
          "id": "viktor_package",
          "weight": 3,
          "type": "obligation"
        }
      ]
    },
    "playerDeck": ["starter_deck_cards"],
    "obligationQueue": [
      {
        "id": "viktor_package",
        "type": "delivery",
        "position": 1,
        "deadline": "12:00",
        "payment": 7,
        "weight": 3,
        "recipient": "marcus"
      }
    ]
  },
  "content": {
    "playerCards": [...],
    "npcs": [...],
    "locations": [...],
    "observations": [...],
    "routes": [...],
    "items": [...]
  }
}
```

### Time Block Definition

```json
{
  "timeBlocks": [
    {
      "id": "dawn",
      "name": "Dawn",
      "startHour": 2,
      "endHour": 6,
      "segments": 4,
      "spotProperties": {
        "market_fountain": "quiet",
        "merchant_row": "closed"
      }
    },
    {
      "id": "morning",
      "name": "Morning",
      "startHour": 6,
      "endHour": 10,
      "segments": 4,
      "spotProperties": {
        "market_fountain": "quiet",
        "merchant_row": "normal"
      }
    },
    {
      "id": "midday",
      "name": "Midday",
      "startHour": 10,
      "endHour": 14,
      "segments": 4,
      "spotProperties": {
        "market_fountain": "busy",
        "merchant_row": "busy"
      }
    },
    {
      "id": "afternoon",
      "name": "Afternoon",
      "startHour": 14,
      "endHour": 18,
      "segments": 4,
      "spotProperties": {
        "market_fountain": "busy",
        "merchant_row": "normal"
      },
      "events": [
        {
          "time": "17:00",
          "segment": 4,
          "event": "lord_blackwood_departs"
        }
      ]
    },
    {
      "id": "evening",
      "name": "Evening",
      "startHour": 18,
      "endHour": 22,
      "segments": 4,
      "spotProperties": {
        "market_fountain": "quiet",
        "merchant_row": "closing"
      }
    },
    {
      "id": "night",
      "name": "Night",
      "startHour": 22,
      "endHour": 2,
      "segments": 4,
      "spotProperties": {
        "market_fountain": "quiet",
        "merchant_row": "closed"
      }
    }
  ]
}
```

## Testing Checklist

### Core Conversation Mechanics
- [ ] Player deck used in all conversations
- [ ] NPC personality rules apply correctly
- [ ] Failure forces LISTEN action
- [ ] Non-persistent cards discarded on LISTEN
- [ ] Focus refreshes on LISTEN
- [ ] Connection state determines card draw count

### Deck Building System
- [ ] Successful plays grant 1 XP to specific card
- [ ] Cards level up at correct thresholds (3, 7, 15, 30)
- [ ] Level benefits apply correctly
- [ ] Signature cards unlock at token thresholds
- [ ] Signature cards shuffle into conversation deck

### Elena Conversation
- [ ] Devoted rule doubles negative rapport
- [ ] Starts in DISCONNECTED state
- [ ] Safe Passage Knowledge advances state
- [ ] Request becomes available at NEUTRAL
- [ ] Basic goal achievable at 5 rapport
- [ ] Premium goal creates weight 3 obligation

### Marcus Conversation  
- [ ] Mercantile rule boosts highest focus card
- [ ] Starts in NEUTRAL state
- [ ] Enhanced goal gives 2 Commerce tokens
- [ ] Commerce tokens unlock signature cards
- [ ] Caravan route observation unlocks exchange

### Resource Management
- [ ] Morning investigation gives +2 familiarity
- [ ] Midday/Afternoon investigation gives +1 familiarity
- [ ] Work at hunger 50 gives 3 coins
- [ ] Work consumes entire time block (4 segments)
- [ ] Lord Blackwood disappears at 5:00 PM (Afternoon segment 4)

### Weight System
- [ ] Satchel capacity is 10 weight maximum
- [ ] Viktor's package weighs 3
- [ ] Elena's letters have different weights (1/2/3)
- [ ] Path cards check weight restrictions
- [ ] Items can be dropped with consequences
- [ ] Consumables can be carried or used immediately

### Time Segment System
- [ ] Each block has 4 segments
- [ ] 6 blocks per day (Dawn/Morning/Midday/Afternoon/Evening/Night)
- [ ] Actions cost appropriate segments
- [ ] Work consumes entire block
- [ ] Block advancement happens correctly
- [ ] Deadline enforcement at Afternoon segment 4

## Conclusion

This POC demonstrates the new conversation system where:

1. **The player's deck is their character** - It grows through XP and new cards
2. **NPCs provide unique puzzles** - Personality rules transform how cards work
3. **Relationships unlock tools** - Signature cards reward token investment
4. **Failure creates rhythm** - Forced LISTEN makes conversations feel authentic
5. **Every conversation matters** - Card XP accumulates across all interactions
6. **Weight creates constant pressure** - Every item carried affects all other choices
7. **Time segments create urgency** - Real time constraints force prioritization within rigid blocks

The scenario is completable in approximately 30 minutes while teaching all core mechanics. Players finish with a clear understanding that conversations are the primary gameplay loop, their deck represents their character's growth over time, and every resource decision cascades through all systems. The time block structure creates natural activity rhythms where working consumes entire blocks, making the deadline genuinely challenging to meet.