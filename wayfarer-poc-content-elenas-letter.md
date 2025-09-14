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
12. [Card System Reference](#card-system-reference)

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

The player begins with a 20-card starter deck representing their basic social repertoire. Cards are defined by categorical properties that determine their effects based on context.

### Card Property System

Each card has four categorical properties:
1. **Persistence**: When the card can be played (Thought/Impulse/Opening)
2. **Success**: Effect type on success (Rapport/Continuing/Atmospheric-[Type]/Focusing/None)
3. **Failure**: Effect type on failure (Disrupting/Backfire/None)
4. **Exhaust**: Effect when discarded unplayed (Continuing/Focusing/Regret/None)

Effect magnitudes are determined by difficulty:
- Very Easy: Magnitude 1
- Easy: Magnitude 2
- Medium: Magnitude 2
- Hard: Magnitude 3
- Very Hard: Magnitude 4

### Complete Starting Deck (20 Cards)

#### Safe Building Cards (8 cards)

**"I hear you"** (3 copies)
- Focus: 1
- Difficulty: Easy (75% base)
- Properties: Thought, Rapport, None, None
- Effect: +2 rapport on success (Easy = magnitude 2)
- On failure: Forces LISTEN

**"Let me think"** (2 copies)
- Focus: 1
- Difficulty: Easy (75% base)
- Properties: Thought, Atmospheric-Patient, None, None
- Effect: Sets Patient atmosphere on success
- On failure: Forces LISTEN

**"I understand"** (3 copies)
- Focus: 2
- Difficulty: Easy (75% base)
- Properties: Thought, Rapport, None, None
- Effect: +2 rapport on success
- On failure: Forces LISTEN

#### Risk/Reward Cards (6 cards)

**"How can I assist?"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base)
- Properties: Thought, Rapport, Backfire, None
- Effect: +2 rapport on success (Medium = magnitude 2)
- On failure: -2 rapport and forces LISTEN

**"Trust me on this"** (2 copies)
- Focus: 3
- Difficulty: Medium (60% base)
- Properties: Thought, Rapport, Backfire, None
- Effect: +2 rapport on success
- On failure: -2 rapport and forces LISTEN

**"Everything will work out"** (2 copies)
- Focus: 4
- Difficulty: Hard (50% base)
- Properties: Impulse, Rapport, Disrupting, Regret
- Effect: +3 rapport on success (Hard = magnitude 3)
- On failure: Discards all cards with Focus 3+ and forces LISTEN
- If exhausted: -3 rapport (Hard = magnitude 3)

#### Utility Cards (6 cards)

**"Tell me more"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base)
- Properties: Thought, Continuing, None, None
- Effect: Draw 2 cards on success (Medium = magnitude 2)
- On failure: Forces LISTEN

**"Let me prepare"** (2 copies)
- Focus: 1
- Difficulty: Easy (75% base)
- Properties: Thought, Focusing, None, None
- Effect: +2 focus to current pool on success (Easy = magnitude 2)
- On failure: Forces LISTEN

**"We can figure this out"** (2 copies)
- Focus: 3
- Difficulty: Hard (50% base)
- Properties: Opening, Rapport, None, Continuing
- Effect: +3 rapport on success (Hard = magnitude 3)
- On failure: Forces LISTEN
- If exhausted: Draw 3 cards (Hard = magnitude 3)

### Conversation Flow with New Rules

When a card play fails:
1. Apply any failure effects (Backfire reduces rapport, Disrupting clears high-focus cards)
2. Apply -1 flow
3. Player MUST take LISTEN action next turn
4. Cannot play any more cards this SPEAK cycle

This creates natural conversation rhythm where success maintains momentum and failure forces reconsideration.

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
- 1 token: "Elena's Faith" (Thought, Rapport, None, None)
- 3 tokens: "Shared Understanding" (Thought, Continuing, None, None)
- 6 tokens: "Elena's Trust" (Impulse, Rapport, None, Regret)
- 10 tokens: "Emotional Bond" (Thought, Atmospheric-Receptive, None, None)
- 15 tokens: "Elena's Devotion" (Thought, Promising, None, None)

**Request Details**:
- Request available at 5 focus capacity (requires NEUTRAL state)
- Basic goal: 5 rapport (delivers letter weight 1, no payment)
- Enhanced goal: 10 rapport (delivers letter weight 2, 1 Trust token)
- Premium goal: 15 rapport (delivers legal documents weight 3, 2 Trust tokens)

**Conversation Challenge**:
The Devoted rule makes Elena's conversation particularly tense. Every failure hurts twice as much due to her personality (Backfire effects doubled), and failure forces LISTEN, losing your assembled hand. Without Trust tokens for signature cards, success requires the observation card to advance her state.

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
- 1 token: "Marcus's Bargain" (Thought, Rapport, None, None, Hard difficulty)
- 3 tokens: "Trade Knowledge" (Thought, Continuing, None, None, Easy difficulty)
- 6 tokens: "Commercial Trust" (Thought, Rapport, Backfire, None, Very Hard difficulty)
- 10 tokens: "Marcus's Favor" (Thought, Rapport, None, None, Hard difficulty - special: doesn't force LISTEN on failure)
- 15 tokens: "Master Trader's Secret" (Thought, Atmospheric-Focused, None, None, Medium difficulty)

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

Observation cards mix into the player's conversation deck when conversing with the relevant NPC. They use the categorical property system.

### Safe Passage Knowledge

**Properties**:
- ID: `safe_passage_knowledge`
- Name: "Safe Passage Knowledge"
- Source: Market Square first observation (familiarity 1+)
- Destination: Added to deck when conversing with Elena
- Focus: 0
- Difficulty: Very Easy (85% base)
- Categorical: Thought, None, None, None
- Special Effect: Advances Elena's connection state from DISCONNECTED to NEUTRAL (unique mechanic)
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
- Categorical: Thought, None, None, None
- Special Effect: Unlocks Marcus's caravan exchange option (unique mechanic)
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
Starting in DISCONNECTED with Elena's Devoted personality rule (failures hurt twice), reaching even the basic 5 rapport threshold is difficult. The forced LISTEN on failure means each failed attempt costs patience and resets your hand. The player needs the Safe Passage Knowledge observation card to advance to NEUTRAL state, giving them 5 focus capacity and 4-card draws on LISTEN.

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

The path uses the correct time blocks and segment constraints. Success requires precise execution with the new conversation mechanics.

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
- Strategy: Use high-focus cards for Mercantile bonus
- "Everything will work out" becomes 80% success with +30% bonus
- Build to 10 rapport for enhanced goal
- Critical: Each failure forces LISTEN, costing patience
- Deliver Viktor's package (frees 3 weight)
- Accept Marcus's letter (weight 1)

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
- Build to 5 rapport carefully
- Critical: With forced LISTEN on failure, each failed play costs patience
- "Everything will work out" with Disrupting property is extreme risk
- Accept basic goal: Elena's letter (weight 1)
- **Afternoon block ends, advances to Evening**

#### Evening Block (6:00 PM - 10:00 PM)
**Cannot use - Lord Blackwood left at 5:00 PM**

### Alternative Successful Path (With Work)

The work path fails because work consumes an entire 4-segment block, making it impossible to complete all necessary steps before the 5 PM deadline.

### Why Only One Path Works

The constraint is the 5:00 PM deadline falling in segment 4 of the Afternoon block. The forced LISTEN on failure rule makes conversations harder but more strategic - players must choose reliable cards over high-impact risky ones when patience is limited.

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
- Devoted rule: Failures decrease rapport twice
- Failure forces LISTEN, losing assembled hand
- Cards with Disrupting property clear high-focus cards on failure
- May run out of patience before reaching even basic goal

#### Accept Heavy Obligations Early
- Marcus offers "Silk Bundle Delivery" (weight 5, pays 15 coins)
- Accepting limits travel options severely
- "Narrow Alley" shortcuts become impassable
- Forces expensive main roads
- May prevent accepting Elena's letter

### Why This Path is Unique

The new conversation system makes the strategic requirements even clearer:

1. **Must investigate** for observation card to unlock Elena's capacity
2. **Must build Commerce tokens** to unlock Marcus's signature cards
3. **Cannot work** due to 4-segment block consumption
4. **Must use Marcus's enhanced delivery** for coins
5. **Must manage weight** to keep travel options open
6. **Must complete within Afternoon block** before 5 PM deadline
7. **Must play Elena conversation strategically** with forced LISTEN on failure

Each element directly supports the core conversation gameplay loop while the time pressure makes every segment precious.

## Card System Reference

### Categorical Properties

**Persistence Types**:
- **Thought**: Remains in hand until played (survives LISTEN)
- **Impulse**: Removed after any SPEAK if unplayed
- **Opening**: Removed after LISTEN if unplayed

**Success Types**:
- **Rapport**: Changes connection strength (magnitude based on difficulty)
- **Continuing**: Draw cards (magnitude based on difficulty)
- **Atmospheric-[Type]**: Sets specific atmosphere (Patient/Focused/Volatile/etc)
- **Focusing**: Restore focus to pool (magnitude based on difficulty)
- **Promising**: Move obligation to position 1 and gain rapport
- **None**: No effect on success

**Failure Types**:
- **Disrupting**: Discard all cards with Focus 3+ from hand
- **Backfire**: Negative rapport (magnitude based on difficulty)
- **None**: No effect beyond forced LISTEN

**Exhaust Types**:
- **Continuing**: Draw cards when discarded unplayed (magnitude based on difficulty)
- **Focusing**: Restore focus when discarded unplayed (magnitude based on difficulty)
- **Regret**: Negative rapport when discarded unplayed (magnitude based on difficulty)
- **None**: No effect when discarded

### Difficulty and Magnitude

Base success rates (modified by +2% per rapport point):
- **Very Easy**: 85% success, magnitude 1
- **Easy**: 75% success, magnitude 2
- **Medium**: 60% success, magnitude 2
- **Hard**: 50% success, magnitude 3
- **Very Hard**: 40% success, magnitude 4

### Conversation Flow

1. **SPEAK Action**: Choose and play one card from hand
2. **Success**: Apply success effect, continue playing if focus remains
3. **Failure**: Apply failure effect, apply -1 flow, MUST LISTEN next
4. **LISTEN Action**: Discard non-Thought cards, draw new cards, refresh focus
5. **Flow Transitions**: At ±3 flow, connection state changes

### Personality Rules

- **Devoted**: When rapport decreases, decrease it twice
- **Mercantile**: Highest focus card each turn gains +30% success
- **Proud**: Cards must be played in ascending focus order each turn
- **Cunning**: Playing same focus as previous card costs -2 rapport
- **Steadfast**: All rapport changes capped at ±2 per card


## Conclusion

This POC demonstrates the refined conversation system where:

1. **Cards use categorical properties** - Effects emerge from context rather than being hardcoded
2. **Failure forces LISTEN** - Creates natural conversation rhythm and strategic weight
3. **Persistence types matter** - Thoughts persist, Impulses demand action, Openings expire
4. **Magnitude scales with difficulty** - Same categorical effect has different power levels
5. **NPCs provide unique puzzles** - Personality rules transform how cards work
6. **Every choice has weight** - Failed plays cost patience and momentum
7. **Conversation feels authentic** - Mechanical flow matches narrative expectations

The scenario is completable in approximately 30 minutes while teaching all core mechanics. Players finish understanding that conversation success requires both strategic deck play and careful risk management, with the forced LISTEN on failure creating genuine conversational flow rather than mechanical card burning.