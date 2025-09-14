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
- **Time**: 9:00 AM Tuesday (Morning block, segment 1)
- **Segments Available**: 24 (full day)
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
- Deadline: 12:00 PM (3 hours = 3 time blocks)
- Payment: 7 coins
- Weight: 3
- Recipient: Marcus at Market Square

### Available Time
- Start: 9:00 AM (Morning block)
- Lord Blackwood leaves: 5:00 PM (Evening block ends)
- Total: 8 hours (20 segments across 5 blocks)
- Optimal path uses: 18 segments
- Buffer: 2 segments

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
- Enhanced goal: 10 rapport (delivers priority letter weight 2, 1 Trust token)
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
- Buy Bread: 3 coins → bread item (weight 1, consume later for -30 hunger)
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
- Noble District Permit: 20 coins (weight 1) - impossible to afford, deliberate dead end

## Locations and Spots

Location mechanics remain unchanged from the original POC. The key interactions:

### Market Square
- **Fountain**: Investigation point 
  - Morning: Quiet (+2 familiarity per segment)
  - Afternoon: Busy (+1 familiarity per segment)
- **Merchant Row**: Marcus location, work available
- **Guard Post**: Guard Captain, permit exchange (dead end)

### Copper Kettle Tavern
- **Corner Table**: Elena location (Private spot, +1 patience during conversations)

### Noble Quarter
- **Blackwood Manor**: Lord Blackwood location (until 5 PM)

### Warehouse District
- **Warehouse Entrance**: Delivery point for Marcus's letter

## Observation Cards

Observation cards mix into the player's conversation deck when conversing with the relevant NPC.

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
- **Premium** (15 rapport): Accept legal documents (weight 3), 2 Trust tokens

**The Challenge**: 
Starting in DISCONNECTED with Elena's Devoted personality rule (failures hurt twice), reaching even the basic 5 rapport threshold is difficult. The player needs the Safe Passage Knowledge observation card to advance to NEUTRAL state, giving them 5 focus capacity and 4-card draws on LISTEN.

**Weight Implications**:
The premium goal creates a heavier package (legal documents weighing 3) that limits what else you can carry to the Noble Quarter. This creates natural trade-offs without complex attachment mechanics.

### Marcus's Trade Letter

**Goal Thresholds**:
- **Basic** (5 rapport): Deliver to Warehouse (weight 1), 5 coins, 1 Commerce token
- **Enhanced** (10 rapport): Priority delivery (weight 1), 8 coins, 2 Commerce tokens
- **Premium** (15 rapport): Rush delivery (weight 1), 12 coins, 3 Commerce tokens

**Strategic Note**: 
With Marcus's Mercantile rule, high-focus cards gain +30% success. This makes his enhanced goal achievable with good card play. Gaining 2 Commerce tokens unlocks his first two signature cards for future conversations.

## Exchange Cards

### Marcus's Exchange Deck

**Buy Simple Food**:
- Cost: 2 coins
- Effect: -50 hunger (immediate consumption)
- No weight impact (consumed immediately)

**Buy Bread**:
- Cost: 3 coins
- Effect: Gain bread item (weight 1)
- Bread can be consumed later for -30 hunger
- Strategic value: Carry for optimal consumption timing

**Join Merchant Caravan**:
- Cost: 10 coins
- Effect: Transport to Noble Quarter
- Requirement: 2+ Commerce tokens
- Time: 2 segments
- Weight restrictions apply during transport

## Routes and Travel

### Market Square to Warehouse District

**Walking Route** (2 segments total):
- Segment 1 paths:
  - "Main Road": 1 stamina, 1 segment
  - "Back Alley": 2 stamina, 0 segments (requires under 5 weight)
  - "Struggle Path": 0 stamina, 2 segments
- Segment 2 paths:
  - "Warehouse Gates": 1 stamina, 1 segment
  - "Loading Dock": 2 stamina, 0 segments
  - "Worker's Path": 0 stamina, 2 segments

### Market Square to Noble Quarter

**Direct Route** (blocked):
- "Guard Checkpoint": Requires Noble District Permit (weight 1)
- Without permit: Cannot proceed, must turn back

**Merchant Caravan** (available after unlocking):
- Cost: 10 coins
- Time: 2 segments
- Comfortable travel (no stamina cost)
- Still subject to event cards during transport

### Market Square to Copper Kettle Tavern

**Walking Route** (1 segment):
- "Market Streets": 1 stamina, 1 segment
- "Crowded Path": 0 stamina, 2 segments
- "Quick Cut": 2 stamina, 0 segments (requires under 3 weight)

## The Only Successful Path

The path remains mechanically identical but now uses time segments and weight management.

### Complete Timeline

#### Morning Block (9:00 AM - 10:00 AM, 4 segments)

**9:00 AM - Investigate Market Square** (Segment 1)
- Spot: Fountain (QUIET in morning)
- Cost: 1 segment
- Result: +2 familiarity (0→2)
- Satchel: Viktor's package (3/10 weight)

**First Observation** (Segment 2)
- Cost: 1 segment
- Gain: "Safe Passage Knowledge" observation card
- Will be added to deck when conversing with Elena

**Converse with Marcus** (Segment 3)
- Cost: 1 segment base + patience depth
- Personality Rule: Highest focus card gains +30% success
- Starting deck: 20 player cards (no signatures yet - 0 Commerce tokens)
- Connection state: NEUTRAL (5 focus, draws 4)
- Deliver Viktor's package (frees 3 weight)
- Accept Marcus's letter (weight 1, now 1/10)
- Try for enhanced goal for 2 Commerce tokens
- Conversation depth: ~15 minutes (3 patience spent)

**Travel to Warehouse** (Segment 4)
- Take "Main Road" path (1 stamina, 1 segment)
- Carrying: Marcus's letter (1/10 weight)

#### Afternoon Block (10:00 AM - 2:00 PM, 4 segments consumed by work)

**10:05 AM - Deliver and Return**
- Deliver Marcus's letter at Warehouse
- Gain: 8 coins (enhanced completion), 2 Commerce tokens
- Marcus now has two signature cards unlocked
- Return to Market Square via "Back Alley" (2 stamina, 0 segments - possible at 0 weight)

**10:10 AM - Buy Food**
- Quick exchange with Marcus: 2 coins for -50 hunger
- Hunger: 50→0
- Coins: 6 remaining

**10:10 AM - Work**
- Cost: Entire afternoon block (4 segments)
- At hunger 0: Full 5 coins output
- Time advances to 2:00 PM (Evening block begins)
- Total coins: 11

#### Evening Block (2:00 PM - 5:00 PM deadline)

**2:00 PM - Second Investigation** (Segment 1)
- Fountain now BUSY: Only +1 familiarity
- Familiarity: 2→3

**Second Observation** (Segment 2)
- Gain: "Merchant Caravan Route" observation card

**Travel to Copper Kettle** (Segment 3)
- Path: "Market Streets" (1 stamina, 1 segment)

**Critical Conversation with Elena** (Segment 4 + conversation depth)
- Cost: 1 segment base + patience spent
- Personality Rule: Failures decrease rapport twice
- Starting State: DISCONNECTED (3 focus, draws 3)
- Deck: 20 player cards + Safe Passage Knowledge
- Turn 1: Play Safe Passage Knowledge (0 focus, 85% success)
- Effect: Advance to NEUTRAL state (5 focus, draws 4)
- Build to 5 rapport carefully (failures hurt double)
- Accept basic goal: Elena's letter (weight 1)
- Conversation uses into next block's segments

**3:30 PM - Purchase Caravan**
- Return to Market Square
- Quick conversation with Marcus to unlock caravan
- Exchange: 10 coins for caravan transport

**3:45 PM - Final Sprint**
- Take caravan to Noble Quarter (2 segments)
- Deliver to Lord Blackwood (quick delivery)
- **SUCCESS**: Completed before 5:00 PM deadline

### Resource Accounting

**Segment Usage** (20 available before deadline):
- Morning Block: 4 segments used (investigate, observe, Marcus conversation, travel)
- Afternoon Block: 4 segments used (work consumes entire block)
- Evening Block: 7 segments used (investigate, observe, travel, Elena conversation, caravan)
- **Total**: 15 segments used, 5 buffer remaining

**Weight Management**:
- Start: Viktor's package (3/10)
- After Viktor delivery: Marcus's letter (1/10)
- After Marcus delivery: Empty (0/10)
- After Elena: Elena's letter (1/10)
- Plenty of capacity throughout

**Hunger Management**:
- Start: 50 hunger
- After morning block: 70 hunger (+20)
- Buy food before work: 20 hunger (-50)
- After work: 40 hunger (+20)
- Manageable throughout without travel penalties

**Card Experience Gained**:
Each successful card play during conversations grants 1 XP to that specific card. Over the course of the POC, players will likely gain 10-15 XP across various cards, beginning their progression journey.

## Failure Analysis

### Common Failure Paths

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
- Makes future conversations harder
- Less coins for crucial purchases

#### Try Guard Checkpoint
- Permit costs 20 coins (weight 1)
- Maximum achievable: 17 coins
- Designed as impossible dead end

#### Poor Conversation Play with Elena
- Devoted rule: Failures hurt twice
- One failed 3-focus card: -2 rapport AND forces LISTEN
- Lose assembled hand, must rebuild from worse position
- May run out of patience before reaching even basic goal

#### Work at Wrong Time
- Working consumes entire time block (4 segments)
- Working too early wastes morning investigation efficiency
- Working too late leaves insufficient time for Elena
- Must time work to maximize both coins and available segments

#### Weight Mismanagement
- Accepting heavy obligations limits travel options
- "Narrow Alley" blocked over 8 weight
- "Quick Cut" blocked over 3 weight
- Forced to take longer, more expensive paths
- Could miss deadline due to extra segments

### Why This Path is Unique

The new systems make the strategic requirements even clearer:

1. **Must investigate morning** for +2 familiarity efficiency
2. **Must build Commerce tokens** to unlock Marcus's signature cards
3. **Must manage hunger** to maximize work output
4. **Must time work block** to preserve critical segments
5. **Must use Marcus's enhanced delivery** for sufficient tokens
6. **Must manage weight** to access efficient paths
7. **Must play Elena conversation perfectly** due to Devoted penalty

Each element directly supports the core conversation gameplay loop while maintaining physical verisimilitude.

## JSON Implementation

### Package Structure

```json
{
  "packageId": "poc_elenas_letter_v3",
  "metadata": {
    "name": "Elena's Letter POC v3",
    "version": "3.0.0",
    "description": "POC with weight and time segments",
    "author": "Wayfarer Team",
    "timestamp": "2025-01-01T00:00:00Z"
  },
  "startingConditions": {
    "time": {
      "block": "Morning",
      "segment": 1,
      "hour": "09:00",
      "day": "Tuesday"
    },
    "resources": {
      "coins": 0,
      "health": 100,
      "hunger": 50
    },
    "satchel": {
      "capacity": 10,
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

### NPC Definition with Weight

```json
{
  "id": "elena",
  "name": "Elena",
  "profession": "Scribe",
  "personalityType": "DEVOTED",
  "personalityRule": {
    "id": "devoted_rule",
    "description": "When rapport would decrease, decrease it twice",
    "effectType": "MultiplyNegativeRapport",
    "effectValue": 2
  },
  "locationId": "copper_kettle_tavern",
  "spotId": "corner_table",
  "currentState": "DISCONNECTED",
  "tokenType": "Trust",
  "signatureCards": [...],
  "requestGoals": [
    {
      "id": "elena_letter_basic",
      "rapportRequired": 5,
      "obligationWeight": 1,
      "reward": "letter_delivery"
    },
    {
      "id": "elena_letter_enhanced",
      "rapportRequired": 10,
      "obligationWeight": 2,
      "reward": "priority_delivery",
      "tokenReward": 1
    },
    {
      "id": "elena_letter_premium",
      "rapportRequired": 15,
      "obligationWeight": 3,
      "reward": "legal_documents",
      "tokenReward": 2
    }
  ]
}
```

### Path Card with Weight Restrictions

```json
{
  "id": "narrow_alley",
  "name": "Narrow Alley",
  "segmentNumber": 1,
  "staminaCost": 2,
  "segmentCost": 0,
  "faceUp": false,
  "weightRestriction": {
    "maximum": 8,
    "effect": "impassable"
  },
  "description": "A tight squeeze between buildings"
}
```

### Investigation Yielding Items

```json
{
  "id": "warehouse_investigation_3",
  "sourceLocation": "warehouse_district",
  "familiarityRequired": 3,
  "segmentCost": 1,
  "result": {
    "type": "item",
    "item": {
      "id": "shipping_manifests",
      "name": "Shipping Manifests",
      "weight": 1,
      "sellValue": 5,
      "effect": "Unlocks special merchant exchanges"
    }
  }
}
```

### Time Block Definition

```json
{
  "timeBlocks": [
    {
      "name": "Dawn",
      "startHour": "02:00",
      "endHour": "06:00",
      "segments": 4
    },
    {
      "name": "Morning",
      "startHour": "06:00",
      "endHour": "10:00",
      "segments": 4,
      "spotProperties": {
        "fountain": "quiet"
      }
    },
    {
      "name": "Afternoon",
      "startHour": "10:00",
      "endHour": "14:00",
      "segments": 4,
      "spotProperties": {
        "fountain": "busy"
      }
    },
    {
      "name": "Evening",
      "startHour": "14:00",
      "endHour": "18:00",
      "segments": 4
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

### Weight System
- [ ] Satchel capacity maximum 10
- [ ] Letters weigh 1 each
- [ ] Packages have variable weight (1-6)
- [ ] Tools persist at constant weight
- [ ] Consumables can be carried and consumed
- [ ] Path cards check weight restrictions
- [ ] Dropping items has permanent consequences

### Time Segment System
- [ ] 24 segments per day (6 blocks × 4 segments)
- [ ] Conversations cost 1 base segment + patience depth
- [ ] Work consumes entire block (4 segments)
- [ ] Investigation costs 1 segment
- [ ] Travel segments based on path cards
- [ ] Quick exchanges cost 0 segments

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
- [ ] Premium goal creates weight 3 package

### Marcus Conversation  
- [ ] Mercantile rule boosts highest focus card
- [ ] Starts in NEUTRAL state
- [ ] Enhanced goal gives 2 Commerce tokens
- [ ] Commerce tokens unlock signature cards
- [ ] Caravan route observation unlocks exchange

### Resource Management
- [ ] Morning investigation gives +2 familiarity
- [ ] Afternoon investigation gives +1 familiarity
- [ ] Work at hunger 0 gives 5 coins
- [ ] Hunger increases by 20 per block
- [ ] Travel slowed at 75+ hunger
- [ ] Lord Blackwood disappears at 5:00 PM

## Conclusion

This POC demonstrates the integrated systems where conversations drive character progression through deck building, weight creates constant physical trade-offs, and time segments force meaningful activity prioritization. 

The scenario is completable in approximately 30 minutes while teaching all core mechanics. Players finish understanding that their deck represents their character's social skills, weight limitations create cascading decisions, and time management determines success or failure. Every conversation matters because cards gain XP. Every item carried has opportunity cost. Every segment spent shapes what's possible. This creates emergent gameplay where the same scenario plays differently based on player choices about what to carry, when to act, and how to build their deck over time.