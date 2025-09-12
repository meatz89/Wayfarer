# Wayfarer POC: Elena's Letter - Complete Content

## Table of Contents
1. [Scenario Overview](#scenario-overview)
2. [Starting Conditions](#starting-conditions)
3. [Universal Starter Deck](#universal-starter-deck)
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

### The Challenge
Players naturally try to help Elena immediately or earn coins for the Noble Quarter checkpoint. Both approaches fail. Success requires building Market Square familiarity through investigation, discovering knowledge that helps specific NPCs, and managing resources with perfect precision.

### The Discovery
Every seemingly inefficient action (investigating twice, buying food before working, building infrastructure before the main quest) is actually essential. The optimal path emerges through understanding system interactions, not obvious choices.

### Success Criteria
- Accept Elena's urgent letter
- Deliver to Lord Blackwood before 5:00 PM
- Use exactly 10 attention and 10 coins
- Build familiarity to unlock observations
- Gain Commerce tokens to enable transport

## Starting Conditions

### Player State
- **Time**: 9:00 AM Tuesday
- **Attention**: 10 (no reduction since hunger is 50)
- **Coins**: 0
- **Hunger**: 50
- **Health**: 100
- **Satchel**: Viktor's package (position 1 obligation)
- **All Tokens**: 0 with all NPCs
- **All Familiarity**: 0 at all locations
- **Connection States**: All NPCs at default

### Initial Obligation
**Viktor's Package to Marcus**
- Position: 1 (must complete first)
- Deadline: 12:00 PM (3 hours)
- Payment: 7 coins
- Recipient: Marcus at Market Square

### Available Time
- Start: 9:00 AM
- Lord Blackwood leaves: 5:00 PM
- Total: 8 hours
- Optimal path uses: 7 hours
- Buffer: 1 hour

## Universal Starter Deck

### Complete 12-Card Set

#### Card 1-3: Safe Progress
**"I hear you"** (3 copies)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: +1 rapport on success
- Failure: No effect
- Persistence: Persistent
- Atmosphere: None
- Purpose: Reliable rapport building, focus flexibility

#### Card 4: Atmosphere Setup
**"Let me think"** (1 copy)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: No rapport, sets Patient atmosphere
- Failure: No effect
- Persistence: Persistent
- Purpose: Saves patience for longer conversations

#### Card 5: Atmosphere Setup
**"Let me prepare"** (1 copy)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: No rapport, sets Prepared atmosphere
- Failure: No effect
- Persistence: Persistent
- Purpose: Enables higher focus plays next turn

#### Card 6-7: Risk/Reward
**"How can I assist?"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base success)
- Effect: +2 rapport on success, -1 rapport on failure
- Persistence: Persistent
- Atmosphere: None
- Purpose: Efficient rapport with genuine risk

#### Card 8-9: Information
**"Tell me more"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base success)
- Effect: Draw 2 cards
- Failure: No effect
- Persistence: Persistent
- Atmosphere: None
- Purpose: Expand options, find key cards

#### Card 10: Powerful Safe
**"I'm here for you"** (1 copy)
- Focus: 3
- Difficulty: Easy (70% base success)
- Effect: +3 rapport
- Failure: No effect
- Persistence: Persistent
- Atmosphere: None
- Purpose: Efficient safe play at full focus

#### Card 11: Powerful Scaling
**"We'll figure this out"** (1 copy)
- Focus: 3
- Difficulty: Hard (50% base success)
- Effect: +X rapport where X = patience ÷ 3
- Failure: No effect
- Persistence: Persistent
- Atmosphere: None
- Purpose: Scaling reward for early play

#### Card 12: Dramatic
**"Everything will be alright"** (1 copy)
- Focus: 4
- Difficulty: Hard (50% base success)
- Effect: +4 rapport
- Failure: -1 rapport
- Persistence: Persistent
- Atmosphere: None
- Purpose: Prepared atmosphere payoff

## NPCs

### Elena - The Desperate Scribe

**Basic Properties**:
- ID: `elena`
- Name: Elena
- Profession: Scribe
- Location: Copper Kettle Tavern, Corner Table spot
- Personality: DEVOTED (15 base patience)
- Starting State: DISCONNECTED (3 focus, 1 card draw)
- Token Type: Trust

**Story Context**:
Young woman facing forced marriage to a merchant she despises. Works at her uncle's tavern while pursuing her writing. Lord Blackwood could intervene due to an old debt to her late father.

**Persistent Decks**:
1. **Conversation Deck**: Standard 12 cards (starter deck)
2. **Request Deck**: Contains "Elena's Urgent Letter"
3. **Observation Deck**: Empty (receives "Safe Passage Knowledge")
4. **Burden Deck**: Empty
5. **Exchange Deck**: None (not mercantile)

**Special Mechanics**:
- Playing "Safe Passage Knowledge" immediately advances to NEUTRAL
- Cannot reach 5-focus request without state advancement
- No payment for letter (she has nothing)

### Marcus - The Merchant

**Basic Properties**:
- ID: `marcus`
- Name: Marcus
- Profession: Merchant
- Location: Market Square, Merchant Row spot
- Personality: MERCANTILE (12 base patience)
- Starting State: NEUTRAL (5 focus, 2 card draws)
- Token Type: Commerce

**Story Context**:
Established trader who runs regular caravans. Cannot leave his stall (merchandise would be stolen). Values reliable business partners.

**Persistent Decks**:
1. **Conversation Deck**: Standard 12 cards
2. **Request Deck**: "Marcus's Trade Letter"
3. **Observation Deck**: Empty (receives "Merchant Caravan Route")
4. **Burden Deck**: Empty
5. **Exchange Deck**: Food, Caravan transport

**Special Mechanics**:
- Caravan exchange requires 2+ Commerce tokens AND route card played
- Provides Commerce tokens for successful deliveries
- Always available during market hours

### Lord Blackwood - The Noble

**Basic Properties**:
- ID: `lord_blackwood`
- Name: Lord Blackwood
- Profession: Noble
- Location: Noble Quarter, Blackwood Manor spot
- Personality: PROUD (10 base patience)
- Starting State: NEUTRAL
- Token Type: Status

**Story Context**:
Influential noble who owes a debt to Elena's late father. Leaving for summer estate at 5:00 PM sharp. Will accept Elena's letter immediately due to noble seal.

**Persistent Decks**:
1. **Conversation Deck**: Not needed (quick delivery only)
2. **Request Deck**: Empty
3. **Observation Deck**: Empty
4. **Burden Deck**: Empty
5. **Exchange Deck**: None

**Special Mechanics**:
- Quick delivery (1 attention) due to Elena's seal
- Disappears at 5:00 PM exactly
- Success ends scenario

### Warehouse Recipient

**Basic Properties**:
- ID: `warehouse_clerk`
- Name: Warehouse Clerk
- Profession: Clerk
- Location: Warehouse District, Warehouse Entrance
- Personality: STEADFAST (13 base patience)
- Starting State: NEUTRAL
- Token Type: Commerce

**Story Context**:
Receives deliveries for warehouse. Simple, efficient, no conversation needed.

**Persistent Decks**: All minimal/empty (delivery notification only)

**Special Mechanics**:
- Quick delivery (0 attention cost)
- Provides token reward immediately

### Guard Captain (Dead End)

**Basic Properties**:
- ID: `guard_captain`
- Name: Guard Captain
- Profession: Guard
- Location: Market Square, Guard Post
- Personality: PROUD (10 base patience)
- Starting State: GUARDED
- Token Type: Shadow

**Story Context**:
Controls checkpoint to Noble Quarter. Permit costs 20 coins (impossible to afford).

**Exchange Deck**:
- Noble District Permit: 20 coins (deliberate dead end)

## Locations and Spots

### Market Square

**Location Properties**:
- ID: `market_square`
- Name: Central Market Square
- Tier: 1
- Type: Hub
- Max Familiarity: 3
- Travel Hub: Fountain spot

**Spots**:

#### Fountain (Investigation Point)
- ID: `fountain`
- Properties by time:
  - Morning (6-10 AM): QUIET
  - Afternoon (10 AM-2 PM): BUSY
  - Evening (2-6 PM): CLOSING
- Can Investigate: Yes
- Investigation scaling:
  - QUIET: +2 familiarity per investigation
  - BUSY: +1 familiarity per investigation

#### Merchant Row
- ID: `merchant_row`
- Properties: COMMERCIAL (all times)
- NPCs: Marcus
- Actions: Work (Haul Goods)

#### Guard Post
- ID: `guard_post`
- Properties: AUTHORITY (all times)
- NPCs: Guard Captain
- Actions: Exchange permits

### Copper Kettle Tavern

**Location Properties**:
- ID: `copper_kettle_tavern`
- Name: Copper Kettle Tavern
- Tier: 1
- Type: Social
- Max Familiarity: 1

**Spots**:

#### Common Room
- ID: `common_room`
- Properties: PUBLIC
- Travel hub for tavern

#### Corner Table
- ID: `corner_table`
- Properties: PRIVATE (+1 patience)
- NPCs: Elena (always present)

#### Bar Counter
- ID: `bar_counter`
- Properties: SERVICE
- NPCs: Bertram (not used in POC)

### Noble Quarter

**Location Properties**:
- ID: `noble_quarter`
- Name: Noble Quarter
- Tier: 2
- Type: Restricted
- Max Familiarity: 0

**Spots**:

#### Gate Entrance
- ID: `gate_entrance`
- Properties: GUARDED
- Checkpoint location

#### Blackwood Manor
- ID: `blackwood_manor`
- Properties: NOBLE
- NPCs: Lord Blackwood (until 5 PM)

### Warehouse District

**Location Properties**:
- ID: `warehouse_district`
- Name: Warehouse District
- Tier: 1
- Type: Commercial
- Max Familiarity: 0

**Spots**:

#### Warehouse Entrance
- ID: `warehouse_entrance`
- Properties: COMMERCIAL
- NPCs: Warehouse Clerk
- Delivery point for Marcus's letter

## Observation Cards

### Safe Passage Knowledge

**Properties**:
- ID: `safe_passage_knowledge`
- Name: "Safe Passage Knowledge"
- Source: Market Square first observation
- Destination: Elena's observation deck
- Focus: 0
- Persistence: Persistent
- Difficulty: Very Easy (85% base)

**Requirements**:
- Location: Market Square
- Familiarity: 1+
- Prior observations: None

**Effect**:
- Type: Advance Connection State
- Target State: NEUTRAL
- Immediate effect when played

**Narrative**: Knowledge of merchant caravan escape routes calms Elena's panic about her situation.

### Merchant Caravan Route

**Properties**:
- ID: `merchant_caravan_route`
- Name: "Merchant Caravan Route"
- Source: Market Square second observation
- Destination: Marcus's observation deck
- Focus: 0
- Persistence: Persistent
- Difficulty: Very Easy (85% base)

**Requirements**:
- Location: Market Square
- Familiarity: 2+
- Prior observations: Safe Passage Knowledge completed

**Effect**:
- Type: Unlock Exchange
- Exchange ID: `marcus_caravan_transport`
- Must be played to activate exchange

**Narrative**: Detailed knowledge of Marcus's private caravan schedule and routes.

## Request Cards

### Elena's Urgent Letter

**Properties**:
- ID: `elena_urgent_letter`
- Name: "Please deliver my refusal!"
- Type: Letter Request
- Focus: 5
- Difficulty: Very Hard (40% base success)
- Persistence: Special (see below)

**Mechanics**:
- Starting State: Unplayable (in draw pile)
- Becomes Playable: When LISTEN at 5+ focus capacity
- When Playable: Gains IMPULSE and OPENING
- Must play immediately or conversation fails

**Success Terms** (Fixed):
- Recipient: Lord Blackwood
- Position: Next available
- Deadline: Before 5:00 PM (external)
- Payment: None

**Failure Effects**:
- Add 1 burden card to Elena
- Conversation ends
- Can retry with burden penalty

### Marcus's Trade Letter

**Properties**:
- ID: `marcus_trade_letter`
- Name: "Warehouse delivery needed"
- Type: Letter Request
- Focus: 4
- Difficulty: Hard (50% base success)
- Persistence: Special

**Success Terms** (Fixed):
- Recipient: Warehouse District
- Position: Next available
- Deadline: 2 hours
- Payment: 5 coins
- Token Reward: 2 Commerce tokens

## Exchange Cards

### Marcus's Exchanges

#### Buy Simple Food
- ID: `marcus_food_simple`
- Cost: 2 coins
- Effect: -50 hunger
- Requirements: None
- Type: Quick exchange (1 attention)

#### Join Merchant Caravan
- ID: `marcus_caravan_transport`
- Cost: 10 coins
- Effect: Transport to Noble Quarter
- Requirements: 
  - 2+ Commerce tokens with Marcus
  - "Merchant Caravan Route" observation played
- Type: Quick exchange (1 attention)
- One-time use

### Guard Captain's Exchange

#### Noble District Permit
- ID: `guard_permit_noble`
- Cost: 20 coins
- Effect: Permanent checkpoint access
- Requirements: None
- Type: Quick exchange (1 attention)
- Purpose: Deliberate dead end (impossible to afford)

## Routes and Travel

### Available Routes

#### Market Square ↔ Copper Kettle Tavern
- Distance: Short
- Time: 15 minutes
- Permits Required: None
- Always available

#### Market Square ↔ Warehouse District
- Distance: Medium
- Time: 20 minutes
- Permits Required: None
- Always available

#### Market Square → Noble Quarter (Checkpoint)
- Distance: Medium
- Time: 25 minutes
- Permits Required: Noble District Permit
- Guard checkpoint (20 coin permit - impossible)

#### Market Square → Noble Quarter (Caravan)
- Distance: Medium
- Time: 20 minutes
- Cost: 10 coins (paid to Marcus)
- Requirements: Marcus caravan exchange unlocked
- One-time transport

#### Copper Kettle ↔ Noble Quarter
- Not available (must go through Market Square)

## The Only Successful Path

### Complete Timeline

#### Morning Block (9:00 AM - 10:10 AM)

**9:00 AM - Start**
- Location: Market Square
- Resources: 10 attention, 0 coins, hunger 50
- Queue: Viktor's package (position 1)

**9:00 AM - Investigate Market Square**
- Spot: Fountain (QUIET in morning)
- Cost: 1 attention
- Result: +2 familiarity (0→2)
- Time: 9:10 AM

**9:10 AM - First Observation**
- Familiarity 1+ unlocked
- Cost: 0 attention
- Gain: "Safe Passage Knowledge" to Elena's deck
- Time: 9:10 AM

**9:10 AM - Converse with Marcus**
- Cost: 2 attention
- Deliver Viktor's package (position 1)
- Gain: 7 coins
- Accept: Marcus's letter to Warehouse
- Time: 9:30 AM

**9:30 AM - Travel to Warehouse**
- Route: Market Square → Warehouse District
- Time cost: 20 minutes
- Arrival: 9:50 AM

**9:50 AM - Deliver Marcus's Letter**
- Cost: 0 attention
- Gain: 5 coins (total: 12)
- Gain: 2 Commerce tokens with Marcus
- Time: 9:50 AM

**9:50 AM - Return to Market Square**
- Time cost: 20 minutes
- Arrival: 10:10 AM

**10:10 AM - Buy Food from Marcus**
- Cost: 2 coins (10 remaining)
- Effect: Hunger 50→0
- Time: 10:10 AM

#### Afternoon Block (10:10 AM - 2:30 PM)

**10:10 AM - Work at Market Square**
- Action: Haul Goods
- Cost: 2 attention
- At hunger 0: +5 coins (total: 15)
- Time advances to: 2:10 PM

**2:20 PM - Second Investigation**
- Spot: Fountain (BUSY in afternoon)
- Cost: 1 attention
- Result: +1 familiarity (2→3)
- Time: 2:20 PM

**2:20 PM - Second Observation**
- Familiarity 2+ AND first observation done
- Cost: 0 attention
- Gain: "Merchant Caravan Route" to Marcus's deck
- Time: 2:20 PM

**2:20 PM - Quick Exchange with Marcus**
- Cost: 1 attention
- Play "Merchant Caravan Route" (0 focus)
- Unlocks caravan (have 2 Commerce tokens)
- Pay 10 coins for transport (5 remaining)
- Time: 2:30 PM

#### Evening Block (2:30 PM - 4:00 PM)

**2:30 PM - Travel to Copper Kettle**
- Time cost: 15 minutes
- Arrival: 2:45 PM

**2:45 PM - Converse with Elena**
- Cost: 2 attention
- Starting state: DISCONNECTED
- Play "Safe Passage Knowledge" → advances to NEUTRAL
- Now 5 focus capacity
- Accept letter at 5 focus
- Time: 3:10 PM

**3:10 PM - Return to Market Square**
- Time cost: 15 minutes
- Arrival: 3:25 PM

**3:25 PM - Take Caravan to Noble Quarter**
- Uses purchased transport
- Time cost: 20 minutes
- Arrival: 3:45 PM

**3:45 PM - Deliver to Lord Blackwood**
- Cost: 1 attention (quick delivery with seal)
- Time: 4:00 PM
- **SUCCESS**: 1 hour before deadline

### Resource Accounting

**Attention Usage**:
- Investigate (morning): 1
- Marcus conversation: 2
- Work: 2
- Investigate (afternoon): 1
- Marcus exchange: 1
- Elena conversation: 2
- Lord Blackwood: 1
- **Total**: 10 (perfect)

**Coin Flow**:
- Start: 0
- Viktor delivery: +7
- Marcus letter: +5
- Food purchase: -2
- Work: +5
- Caravan: -10
- **Final**: 5 (unused)

**Token Progression**:
- Start: 0 Commerce tokens
- Marcus delivery: +2 Commerce tokens
- Enables caravan when combined with route

**Familiarity Build**:
- Morning investigation: 0→2
- Afternoon investigation: 2→3
- Unlocks both observations

## Failure Analysis

### Common Failure Paths

#### Rush to Elena First
- Elena in DISCONNECTED (3 focus capacity)
- Cannot reach 5-focus request card
- Even with perfect draws, mathematically improbable
- Waste attention on failed attempt

#### Investigate Only Once
- First investigation: +2 familiarity
- Allows first observation only
- Second observation requires familiarity 2+
- Cannot unlock caravan without second observation

#### Skip Food Purchase
- Work at hunger 50: Only 3 coins output
- Total coins: 7 (Viktor) + 5 (Marcus) + 3 (work) = 15
- After food eventually: 13 coins
- Still short for caravan + other expenses

#### Skip Marcus's Letter
- No Commerce tokens gained
- Caravan locked even with route knowledge
- Cannot reach Noble Quarter

#### Try Checkpoint Route
- Permit costs 20 coins
- Maximum possible: 7 + 5 + 5 = 17 coins
- Designed as impossible dead end

#### Wrong Investigation Timing
- Afternoon investigation: Only +1 familiarity
- Would need 3 investigations to reach level 3
- Not enough attention available

### Why This Path is Unique

Every action has a specific purpose:
1. **Morning investigation**: Efficient familiarity gain
2. **Viktor's delivery**: Enables coin economy
3. **Marcus's letter**: Provides Commerce tokens
4. **Food purchase**: Maximizes work output
5. **Work timing**: After eating for full output
6. **Afternoon investigation**: Reaches threshold
7. **Observation order**: First helps Elena, second helps Marcus
8. **Exchange timing**: After gaining tokens
9. **Elena timing**: After gaining observation
10. **Transport method**: Only viable route

Remove any step and the path fails.

## JSON Implementation

### Package Structure

```json
{
  "packageId": "poc_elenas_letter",
  "metadata": {
    "name": "Elena's Letter POC",
    "version": "1.0.0",
    "description": "Complete POC scenario demonstrating all three game loops",
    "author": "Wayfarer Team",
    "timestamp": "2025-01-01T00:00:00Z"
  },
  "startingConditions": {
    "time": "09:00",
    "day": "Tuesday",
    "coins": 0,
    "health": 100,
    "hunger": 50,
    "attention": 10,
    "obligationQueue": [
      {
        "id": "viktor_package",
        "type": "delivery",
        "position": 1,
        "deadline": "12:00",
        "payment": 7,
        "recipient": "marcus"
      }
    ]
  },
  "content": {
    "cards": [...],
    "npcs": [...],
    "locations": [...],
    "observations": [...],
    "routes": [...]
  }
}
```

### NPC Definition Example

```json
{
  "id": "elena",
  "name": "Elena",
  "profession": "Scribe",
  "personalityType": "DEVOTED",
  "locationId": "copper_kettle_tavern",
  "spotId": "corner_table",
  "currentState": "DISCONNECTED",
  "tokenType": "Trust",
  "persistentDecks": {
    "conversationDeck": [
      "hear_you_1", "hear_you_2", "hear_you_3",
      "let_me_think", "let_me_prepare",
      "how_assist_1", "how_assist_2",
      "tell_more_1", "tell_more_2",
      "here_for_you", "figure_it_out",
      "everything_alright"
    ],
    "requestDeck": ["elena_urgent_letter"],
    "observationDeck": [],
    "burdenDeck": [],
    "exchangeDeck": []
  }
}
```

### Card Definition Example

```json
{
  "id": "hear_you_1",
  "name": "I hear you",
  "type": "Normal",
  "focus": 1,
  "difficulty": "Easy",
  "baseSuccessRate": 70,
  "persistence": "Persistent",
  "effect": {
    "type": "FixedRapport",
    "value": 1
  },
  "failureEffect": null,
  "atmosphere": null,
  "description": "A simple acknowledgment that builds trust"
}
```

### Observation Definition Example

```json
{
  "id": "safe_passage_knowledge",
  "name": "Safe Passage Knowledge",
  "sourceLocation": "market_square",
  "sourceSpot": "fountain",
  "familiarityRequired": 1,
  "priorObservationRequired": null,
  "targetNpcId": "elena",
  "targetDeck": "observation",
  "effect": {
    "type": "AdvanceConnectionState",
    "targetState": "NEUTRAL"
  },
  "consumed": true,
  "description": "Knowledge of merchant escape routes"
}
```

### Route Definition Example

```json
{
  "id": "market_to_noble_caravan",
  "name": "Merchant Caravan to Noble Quarter",
  "from": "market_square",
  "to": "noble_quarter",
  "timeMinutes": 20,
  "requirements": {
    "type": "Exchange",
    "exchangeId": "marcus_caravan_transport"
  },
  "oneTime": true
}
```

## Testing Checklist

### Core Mechanics
- [ ] Elena starts in DISCONNECTED state
- [ ] Cannot reach 5-focus request without observation
- [ ] Safe Passage Knowledge advances to NEUTRAL
- [ ] Request card gains Impulse+Opening when playable

### Resource Management
- [ ] Morning investigation gives +2 familiarity
- [ ] Afternoon investigation gives +1 familiarity
- [ ] Work at hunger 0 gives 5 coins
- [ ] Work at hunger 50 gives 3 coins
- [ ] Food costs 2 coins, removes 50 hunger

### Token System
- [ ] Marcus letter gives 2 Commerce tokens
- [ ] Caravan requires 2+ Commerce tokens
- [ ] Token check happens before exchange

### Observation System
- [ ] First observation at familiarity 1+
- [ ] Second observation at familiarity 2+ with first done
- [ ] Observations go to correct NPC decks
- [ ] Playing observation costs 0 focus

### Time Management
- [ ] Work advances 4 hours
- [ ] Investigation takes 10 minutes
- [ ] Travel times accurate
- [ ] Lord Blackwood disappears at 5:00 PM

### Failure States
- [ ] Checkpoint actually costs 20 coins
- [ ] Maximum coins achievable is 17
- [ ] Missing Commerce tokens blocks caravan
- [ ] Wrong investigation timing breaks path

## Conclusion

This POC demonstrates all three core Wayfarer loops:
1. **Conversations**: Managing focus to reach request cards
2. **Queue**: Forced sequential completion of obligations
3. **Travel/Investigation**: Building familiarity for observations

The tight resource constraints (10 attention, 10 coins used perfectly) and precise action ordering showcase the strategic depth emerging from simple mechanics. Players discover the solution through understanding system interactions, not following obvious paths.

The scenario is completable in approximately 30 minutes of play, providing a satisfying introduction to Wayfarer's mechanical depth while maintaining narrative coherence throughout.