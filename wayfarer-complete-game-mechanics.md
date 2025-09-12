# Wayfarer: Complete Game Mechanics

## Table of Contents
1. [Core Design Philosophy](#core-design-philosophy)
2. [Three Core Game Loops](#three-core-game-loops)
3. [Resource Economy](#resource-economy)
4. [Conversation System](#conversation-system)
5. [Starter Deck Design Principles](#starter-deck-design-principles)
6. [Queue Management System](#queue-management-system)
7. [Location and Travel System](#location-and-travel-system)
8. [Time System](#time-system)
9. [Exchange System](#exchange-system)
10. [Work System](#work-system)
11. [Strategic Resource Management](#strategic-resource-management)
12. [Content Loading System](#content-loading-system)
13. [Design Verification](#design-verification)

## Core Design Philosophy

### Fundamental Principles

The game must create strategic depth through impossible choices, not mechanical complexity. Like Slay the Spire's first fight with just Strikes and Defends, every turn must force players to choose between multiple suboptimal paths.

- **Elegance Over Complexity**: Every mechanic serves exactly one purpose
- **Verisimilitude Throughout**: All mechanics make narrative sense  
- **Perfect Information**: All calculations visible to players
- **No Soft-Lock Architecture**: Always a path forward, even if suboptimal
- **Deterministic Systems**: No hidden randomness beyond stated percentages
- **No Threshold Design**: Every resource scales linearly (except flow ±3)

### Intentional Mechanic Design

Examples of clean separation:

**BAD**: "Routes require access permit OR 10 coins"
**GOOD**: Routes require access permit. Guards can be bribed for permits. Merchants sell permits.

**BAD**: "High tokens unlock better cards AND improve success"  
**GOOD**: Tokens provide starting rapport. Tokens gate specific exchanges. Two separate mechanics.

**BAD**: "Hunger reduces patience AND attention AND work output"
**GOOD**: Hunger reduces attention (morning calculation). Hunger reduces work output (separate formula). Patience is per-NPC, unaffected.

**BAD**: "Atmosphere affects focus AND success"
**GOOD**: Prepared atmosphere affects focus capacity. Focused atmosphere affects success. Each atmosphere has ONE effect.

**BAD**: "Investigation gives familiarity AND cards"
**GOOD**: Investigation gives familiarity. Observation gives cards (requires familiarity). Two separate actions.

## Three Core Game Loops

### System Integration Philosophy

The three core game loops answer fundamental design questions while maintaining strict mechanical separation. Each loop creates problems that only the other loops can solve, forcing engagement with all systems.

### Core Loop 1: Card-Based Conversations

#### Design Questions Answered
- **What provides challenge?** Managing focus and rapport to reach request cards
- **Why grow stronger?** More tokens improve starting rapport linearly  
- **Why engage with NPCs?** Request cards provide income, access, and world progression

#### The Conversation Puzzle

1. Connection States determine focus capacity (3-6) and card draws (1-3)
2. Focus persists across SPEAK actions, refreshes on LISTEN
3. Flow (-3 to +3) tracks success/failure, triggers state transitions at extremes
4. Rapport (-50 to +50) modifies all success rates linearly (+2% per point)
5. Atmosphere persists until changed or failure occurs
6. One card per SPEAK action creates authentic dialogue rhythm

#### Connection States

- **Disconnected**: 3 focus capacity, 1 card draw
- **Guarded**: 4 focus capacity, 2 card draws
- **Neutral**: 5 focus capacity, 2 card draws
- **Receptive**: 5 focus capacity, 3 card draws
- **Trusting**: 6 focus capacity, 3 card draws

At -3 flow in Disconnected: Conversation ends immediately.

#### NPC Five Persistent Decks

Each NPC maintains five persistent decks:

1. **Conversation Deck**: Standard 20 cards for dialogue
2. **Request Deck**: Goal cards (letters, promises) enabling special conversations
3. **Observation Deck**: Cards from location discoveries relevant to this NPC
4. **Burden Deck**: Cards from failed obligations (TBD mechanics)
5. **Exchange Deck**: Commerce options (mercantile NPCs only)

These decks determine available conversation types. Request cards enable specific conversation options - if NPC has a letter request card, "Letter Request" conversation appears.

#### Conversation Outputs
- **Promises**: Create obligations in queue (letters, meetings, escorts, etc.)
- **Tokens**: Gained only through successful letter delivery (+1 to +3)
- **Observations**: Cards added to specific NPCs' observation decks
- **Deck Evolution**: Successful completions modify NPC decks
- **Permits**: Special promises that enable routes
- **Burden Cards**: Failed requests damage relationships

### Core Loop 2: Obligation Queue Management

#### Design Questions Answered
- **Why travel between locations?** Obligations scattered across the city
- **Why revisit locations?** Building relationships for better starting rapport
- **Why manage time?** Deadlines create pressure and force prioritization

#### Queue Mechanics

**Strict Sequential Execution**:
- Position 1 MUST complete first
- No exceptions to this rule
- Maximum 10 obligations

**Queue Displacement Cost**:
To deliver out of order, burn tokens with EACH displaced NPC:
- Position 3 to 2: Burn 1 token with position 2 NPC
- Position 3 to 1: Burn 2 tokens with position 1 NPC AND 1 token with position 2 NPC
- Each burn adds 1 burden card to that NPC's relationship record

Token type burned matches NPC personality:
- Devoted: Trust tokens
- Mercantile: Commerce tokens
- Proud: Status tokens
- Cunning: Shadow tokens

#### Request Card Terms (Fixed)

When playing a request card:
- **Success**: Accept obligation with predetermined terms
- **Failure**: No obligation, add burden card to relationship

Request cards no longer involve negotiation - terms are fixed based on the request type:
- Letter requests: Specific deadline, position, and payment
- Meeting requests: Fixed time and location
- Resolution requests: Clear existing burden cards

Personality influences which requests are available:
- Proud NPCs offer urgent, high-position requests
- Disconnected connection state only has crisis requests
- Mercantile NPCs focus on profitable exchanges

#### Strategic Queue Patterns

**Obligation Chaining**: Accept multiple obligations in same location, complete efficiently

**Token Preservation**: Accept fixed queue positions to avoid burning relationships

**Emergency Displacement**: Burn tokens only for critical deadlines

**Queue Blocking**: Full queue (10 obligations) prevents new letter acquisition

### Core Loop 3: Location and Travel System

#### Design Questions Answered
- **How does progression manifest?** Access to new routes and locations
- **How does world grow?** Location familiarity and discoveries unlock content
- **What creates exploration?** Information has mechanical value through unique effects

#### Location Familiarity System

**Familiarity Mechanics**:
- Each location tracks Familiarity (0-3)
- Represents player's understanding of location patterns and secrets
- Only increased by Investigation action (not NPC interactions)
- Never decreases
- Determines observation rewards available

**Investigation Action**:
- Costs 1 attention
- Takes 10 minutes game time
- Familiarity gain scales with current spot properties:
  - Quiet spots: +2 familiarity
  - Busy spots: +1 familiarity
  - Other spots: +1 familiarity
- Can investigate same location multiple times
- Creates Istanbul-style timing decisions

#### Travel Mechanics

**Route Requirements**:
- Every route requires an access permit
- No alternatives or "OR" conditions
- Multiple NPCs can provide same permit through different means

**Access Permit Sources**:
- Request cards with fixed terms
- Exchange cards from merchants (coin cost)
- Observation rewards from NPCs
- Location discoveries

**Permits as Physical Items**:
- Take satchel space (max 5 letters/permits)
- Do not expire (they're physical documents)
- No associated obligation
- Enable specific routes while held

#### Observation System

**Building Discoveries**:
- Observations require minimum familiarity levels
- Each observation requires all prior observations at that location
- Cost 0 attention (just noticing what you understand)
- Create cards that go into specific NPCs' observation decks
- Different observations available at different familiarity levels:
  - First observation: Requires familiarity 1+
  - Second observation: Requires familiarity 2+ AND first observation done
  - Third observation: Requires familiarity 3+ AND second observation done

**Observation Effects**:
- Cards created go to predetermined NPCs' observation decks
- Represent location knowledge meaningful to specific NPCs
- Mixed into draw pile when conversing with relevant NPC
- Can unlock exchanges, change connection states, or provide unique effects

#### Travel Encounters

Use conversation mechanics with special decks:
- **Bandits**: Violence deck, combat resolution (TBD)
- **Guards**: Inspection deck, authority check (TBD)
- **Merchants**: Road trade deck, exchange opening (TBD)

Success allows passage, failure costs resources.

## Resource Flow Between Loops

### Attention Economy Connections

**Daily Allocation**: 10 - (Hunger ÷ 25), minimum 2

Attention enables:
- **Conversations** (2): Access to letters and tokens
- **Investigations** (1): Build location familiarity
- **Observations** (0): Discover cards for NPC observation decks
- **Work** (2): Coins but time cost, scaled by hunger
- **Quick Exchange** (1): Simple commerce without full conversation

Work output scales with hunger:
- Formula: coins = 5 - floor(hunger / 25)
- Hungry workers are less productive
- Creates meaningful choice about when to eat

This forces prioritization between relationship building, location investment, and resource generation.

### Token Economy Integration

Tokens serve multiple purposes through different mechanics:
- **Starting Rapport**: Each token provides 1 starting rapport in conversations
- **Queue Displacement**: Burn for queue flexibility (permanent cost)
- **Scaling Effects**: Some cards scale rapport gain with token count
- **Exchange Gates**: Minimum tokens required for special exchanges

Tokens only gained through successful letter delivery:
- Standard delivery: +1 token with recipient (type based on letter)
- Excellent delivery: +2-3 tokens with recipient (type based on letter)
- Failed delivery: -2 tokens with sender

Each use is a different mechanic with one purpose. Higher tokens mean easier conversation starts through rapport.

### Time Pressure Cascades

Time advances through:
- **Travel**: Route-specific time costs
- **Investigation**: 10 minutes per action
- **Work**: 4-hour period advance
- **Rest**: Variable time skip
- **Natural progression**: During lengthy activities

Deadlines create cascading decisions:
- Tight deadline → Need displacement → Burn tokens → Lower future starting rapport
- Or: Rush to complete → Skip relationship building → Miss better letters

### How Loops Create Problems for Each Other

**Conversations create Queue pressure**:
- Every letter accepted adds obligation with fixed terms
- Multiple letters compete for position 1
- Focus management affects ability to reach request cards
- Low rapport makes request success uncertain

**Queue creates Travel pressure**:
- Obligations scattered across city
- Deadlines force inefficient routing
- Displacement damages relationships at distance
- Time-fixed meetings cannot be displaced

**Travel creates Conversation pressure**:
- Access permits require successful request card plays
- Travel time reduces deadline margins
- Encounters can damage resources
- Building familiarity costs attention that could fund conversations

### How Loops Solve Each Other's Problems

**Conversations solve Travel problems**:
- Request cards provide access permits
- Successful deliveries reward observation cards
- Built relationships (more tokens) make future permits easier
- Atmosphere effects can overcome obstacles

**Queue management solves Conversation problems**:
- Completing letters builds tokens for starting rapport
- Meeting deadlines maintains sender relationships
- Efficient routing preserves resources for conversations

**Travel solves Queue problems**:
- Familiarity reveals efficient routes
- Observations unlock better exchanges
- Permits enable shortcuts
- Investigation timing affects resource efficiency

## Resource Economy

### Persistent Resources

#### Coins
- **Range**: 0-999
- **Generation**: 
  - Work actions (5 coins base, scaled by hunger)
  - Letter deliveries (5-15 coins typical)
  - Exchanges and trades
- **Uses**: 
  - Food purchases (2-3 coins typically)
  - Rest options (5-10 coins)
  - Exchanges (varies)
  - Caravan transport (10 coins)
  - Permits (15-20 coins)
- **No decay or automatic loss**
- **Visibility**: Always shown in UI

#### Health
- **Range**: 0-100
- **Current**: Variable based on damage
- **Maximum**: 100 (can be modified by items/conditions)
- **Effects**: 
  - Below 50: -1 focus capacity in conversations
  - At 0: Death/game over (TBD)
- **Loss**: 
  - Starvation (5 per time period at 100 hunger)
  - Combat encounters (TBD)
  - Failed events (TBD)
- **Restoration**: 
  - Rest actions (TBD)
  - Medical exchanges (TBD)
  - Food with healing properties (TBD)

#### Hunger
- **Range**: 0-100
- **Effects**:
  - **Attention Calculation**: Reduces morning attention by (Hunger ÷ 25)
    - At 0 hunger: 10 attention
    - At 25 hunger: 9 attention
    - At 50 hunger: 8 attention
    - At 75 hunger: 7 attention
    - At 100 hunger: 6 attention (minimum 2 enforced)
  - **Work Productivity**: Reduces work output by floor(Hunger ÷ 25) coins
    - At 0 hunger: 5 coins
    - At 25 hunger: 4 coins
    - At 50 hunger: 3 coins
    - At 75 hunger: 2 coins
    - At 100 hunger: 1 coin
  - **Starvation Trigger**: At 100 → lose 5 health per time period
- **Automatic Increase**: +20 per time period
- **Restoration**:
  - Food exchanges (coins → hunger relief)
  - Meals (reset to 0 or reduce by amount)
  - Tavern rest options
  - Consumable items
- **Strategic Role**: Constant pressure that erodes other resources. Forces regular maintenance without hard blocking.

#### Attention
- **Daily Allocation**: 10 - (Hunger ÷ 25), minimum 2
- **Costs**:
  - Standard conversation: 2
  - Quick exchange: 1
  - Investigation: 1
  - Work: 2
  - Observation: 0 (free when available)
- **Cannot be saved between days**
- **Strategic Role**: Core action economy forcing prioritization

#### Location Familiarity
- **Range**: 0-3 per location
- **Generation**: Investigation action only
  - Quiet spots: 1 attention → +2 familiarity
  - Busy spots: 1 attention → +1 familiarity
  - Other spots: 1 attention → +1 familiarity
- **Never decreases**
- **Location-specific** (not global)
- **Independent of NPC relationships**
- **Enables observations at threshold levels**
- **Strategic Role**: Represents location knowledge. Creates Istanbul-style gameplay where timing of investigation matters for efficiency.

#### Connection Tokens

Four types, each with distinct identity:
- **Trust**: Personal bonds (Devoted NPCs prefer)
- **Commerce**: Professional dealings (Mercantile NPCs prefer)
- **Status**: Social standing (Proud NPCs prefer)
- **Shadow**: Shared secrets (Cunning NPCs prefer)

**Single Mechanical Effect**: Provide starting rapport in conversations (1 token = 1 rapport)

**Multiple Uses Through Different Mechanics**:
1. **Starting Rapport**: Each token provides 1 starting rapport in conversations
2. **Displacement Cost**: Burn tokens to jump queue positions
3. **Scaling Effects**: Some rapport cards scale with specific token counts
4. **Exchange Gating**: Minimum tokens required for special exchanges

**Generation**:
- Standard delivery: +1 token with recipient
- Excellent delivery: +2-3 tokens with recipient
- Failed delivery: -2 tokens with sender
- Special events and quests

**Token Investment Return**:
- 3 tokens = 3 starting rapport = +6% all cards
- 6 tokens = 6 starting rapport = +12% all cards
- 10 tokens = 10 starting rapport = +20% all cards
- 25 tokens = 25 starting rapport = +50% all cards (halfway to guarantee)
- Burning 10 tokens severely damages multiple relationships

### Per-Conversation Resources

#### Focus
- **Capacity by Connection State**:
  - Disconnected: 3
  - Guarded: 4
  - Neutral: 5
  - Receptive: 5
  - Trusting: 6
- **Mechanics**:
  - Pool persists across SPEAK actions
  - Refreshes to maximum on LISTEN
  - Each card costs its focus value
  - Prepared atmosphere adds +1 to current capacity
  - Can exceed maximum temporarily with Prepared
  - Health below 50 reduces capacity by 1
- **Strategic Role**: Core resource management within conversations. Enables multi-turn planning with impulse cards that require more focus than currently available.

#### Rapport
- **Range**: -50 to +50
- **Starting Value**: Equal to connection tokens with NPC
- **Effect**: +2% success rate per point on all cards
  - At -50: Guaranteed failure
  - At 0: Base card percentages
  - At +25: +50% to all cards
  - At +50: Guaranteed success
- **Changes**: Through card effects only
- **Resets**: After conversation ends
- **Strategic Role**: Momentum system rewarding successful plays

#### Flow
- **Range**: -3 to +3
- **Always starts at 0**
- **Changes**: 
  - Success on SPEAK: +1 flow
  - Failure on SPEAK: -1 flow
- **Effects**:
  - At +3: State shifts right, flow resets to 0
  - At -3: State shifts left, flow resets to 0
  - Excess flow lost (no banking)
- **State progression**: [Ends] ← Disconnected ← Guarded ← Neutral → Receptive → Trusting
- **Strategic Role**: Progress tracker forcing consistent success

#### Atmosphere
**Standard Atmospheres** (~30% of cards):
- **Neutral**: No effect (default, set after any failure)
- **Prepared**: +1 focus capacity
- **Receptive**: +1 card on LISTEN
- **Focused**: +20% success all cards
- **Patient**: Actions cost 0 patience
- **Volatile**: All rapport changes ±1
- **Final**: Any failure ends conversation

**Observation-Only Atmospheres**:
- **Informed**: Next card cannot fail
- **Exposed**: Double all rapport changes
- **Synchronized**: Next card effect happens twice
- **Pressured**: -1 card on LISTEN

**Mechanics**:
- Persistence: Remains until changed by another card or cleared by failure
- Changes take effect immediately
- Only ~30% of cards change atmosphere
- Failure always clears to Neutral

**Strategic Role**: Environmental modifier that shapes entire conversations. Setup cards create favorable conditions for critical plays.

#### Patience
- **Base Values by Personality**:
  - Devoted: 15
  - Steadfast: 13  
  - Mercantile: 12
  - Cunning: 12
  - Proud: 10
- **Modifiers**:
  - Private spot: +1
  - Public spot: -1
  - Patient atmosphere: Actions cost 0
- **Effect**: Maximum turns in conversation (LISTEN costs 1 turn)
- **Each patience spent = 10 minutes game time**
- **Strategic Role**: Time limit for each conversation. Forces efficient play and tough decisions about when to push vs accept available options.

### Time Resources

#### Time Structure
- **Days** → **Time Blocks** → **Time Segments**
- **Time Blocks** (6 per day, 4 hours each):
  - Dawn (2-6 AM)
  - Morning (6-10 AM)
  - Afternoon (10 AM - 2 PM)
  - Evening (2-6 PM)
  - Night (6-10 PM)
  - Late Night (10 PM - 2 AM)
- **Time Segments**: 4 per time block
- **Time Costs**:
  - Some actions cost 1-2 segments
  - Travel cards may consume segments
  - Extended conversations increase segment cost
  - When segments exceed block, advance to next block

#### Time Advancement Mechanics
- **Travel**: Route-specific time cost (15-60 minutes typical)
- **Work**: Always advances one full time block (4 hours)
- **Investigation**: 10 minutes per action
- **Conversation**: Base 30 minutes + patience spent
- **Rest**: Variable based on rest type
- **Wait**: Strategic time advancement (player choice)
- **Natural**: During lengthy activities

#### Effects of Time
- **No NPC availability windows** (NPCs always present when at location)
- **Spot property changes**:
  - Morning: Often Quiet
  - Afternoon: Often Busy
  - Evening: Often Closing
- **Shop operating hours** (some exchanges time-limited)
- **Deadline pressure** (obligations expire)
- **Investigation efficiency** (Quiet vs Busy spots)

#### Deadlines
- **Range**: 1-24 hours typically
- **Set by**: Request card fixed terms
- **Effect of Missing**: 
  - -2 tokens with sender
  - +2 burden cards to sender's relationship record
  - No payment received
  - Permanent relationship damage
- **Cannot be extended or renegotiated**
- **Strategic Role**: Creates cascading time pressure. Forces queue management decisions and route optimization.

### Information Resources

#### Observation Cards
- **Not from player decks** - gained from world exploration
- **Acquisition Mechanics**:
  - **Location Observation**: 0 attention at spots with sufficient familiarity
  - **NPC Rewards**: Completing promises
  - **Travel Discoveries**: Finding new routes
- **Properties**:
  - Go directly to specific NPC's observation deck
  - Mixed into draw pile at conversation start with that NPC
  - Focus 0 (costs SPEAK action but no focus)
  - Always persistent
  - Consumed when played
  - Can have state-changing effects (advance connection state, unlock exchanges)
- **Gating**:
  - First observation: Requires familiarity 1+
  - Second observation: Requires familiarity 2+ AND first observation done
  - Third observation: Requires familiarity 3+ AND second observation done
- **Strategic Role**: Bridge exploration and NPC relationships. Reward investigation with powerful conversation tools. Create essential preparation for difficult conversations.

#### Access Permits
- **Type**: Special items, not obligations
- **Properties**:
  - Occupy satchel space (max 5 items total with letters)
  - Enable specific routes
  - Never expire (physical documents)
  - Cannot be dropped without consequence
- **Acquisition**:
  - Request cards (fixed terms, no negotiation)
  - Exchange cards (15-20 coins typically)
  - Observation rewards
  - Quest rewards
- **Strategic Role**: Gate exploration and enable efficient routing. Compete for limited satchel space.

#### Burden Cards
- **Not in conversation decks** - tracked per NPC relationship
- **Mechanics**: TBD in detail
- **Acquisition**:
  - Failed request cards: +1 burden card
  - Queue displacement: +1 per token burned
  - Broken promises: +1-2 burden cards
  - Dropped letters: +2 burden cards
- **Effects**: TBD
  - Block relationship progress
  - Enable "Make Amends" conversation type
  - Visual indicator of damaged relationships
- **Resolution**: TBD
  - "Clear the Air" request card removes burdens
  - Very Hard difficulty (40% base + rapport modifier)
- **Strategic Role**: Permanent consequences that must be actively resolved. Create repair arcs for damaged relationships.

## Conversation System

### Core Design Principle

SPEAK Costs No Patience - this is the critical change that transforms the system:
- **LISTEN**: Costs 1 patience, draws cards, refreshes focus to maximum
- **SPEAK**: FREE (only spends focus from pool)

This transforms patience from action currency into focus cycle currency. Each patience point represents potential card plays across the entire conversation, making efficiency paramount.

### NPC Five-Deck System

Each NPC maintains five persistent decks that determine conversation availability:

1. **Conversation Deck**: Standard 20 cards for dialogue
   - Always available for standard conversations
   - Cards have focus costs, difficulty tiers, and persistence types
   
2. **Request Deck**: Goal cards (letters, promises, meetings)
   - Each card type enables specific conversation options
   - Letter cards enable "Letter Request" conversations
   - Promise cards enable "Promise" conversations
   - Fixed terms, no negotiation
   
3. **Observation Deck**: Cards from location discoveries
   - Receives cards from location observations
   - Cards automatically mixed into draw pile at conversation start
   - Provide unique advantages (state changes, exchange unlocks)
   - Consumed when played
   
4. **Burden Deck**: Cards from failed obligations
   - Contains burden cards from relationship damage
   - Enables "Make Amends" conversation type
   - Each burden card makes resolution harder
   - Mechanics TBD
   
5. **Exchange Deck**: Commerce options (mercantile NPCs only)
   - Not conversation cards - simple trades
   - Accessed through Quick Exchange (1 attention)
   - Examples: 2 coins → food, 10 coins → transport

### Three-Pile System

#### Draw Pile
- Created at conversation start from relevant NPC decks
- Contains all cards for this conversation type
- Shuffled once at start
- When empty, shuffle exhaust pile to reform

#### Active Pile (Hand)
- Cards currently available to play
- No maximum hand size
- Observation cards don't count against normal draws

#### Exhaust Pile
- Played cards go here
- Cards removed by Impulse/Opening go here
- Shuffled to create new draw pile when needed

### Starting a Conversation

1. **Pay attention cost** (2 for standard, 1 for quick exchange)
2. **Choose conversation type** (based on available NPC decks)
3. **Build draw pile** from relevant cards:
   - All conversation deck cards (20)
   - All observation deck cards (if any)
   - Relevant request card (if request conversation)
4. **Shuffle draw pile**
5. **Starting rapport** = connection tokens with NPC
6. **Draw initial hand** = cards equal to connection state
   - Disconnected: 1 card
   - Guarded: 2 cards
   - Neutral: 2 cards
   - Receptive: 3 cards
   - Trusting: 3 cards
7. **Set focus** to connection state maximum
8. **Request cards start unplayable** (if present)

### LISTEN Action

Complete sequence:
1. **Check patience cost**
   - Normal: Costs 1 patience
   - Patient atmosphere: Costs 0 patience
2. **Draw cards** equal to connection state
   - If draw pile has fewer: Draw what's available
   - If draw pile empty: Shuffle exhaust → new draw pile → continue
3. **Refresh focus** to connection state maximum
4. **Apply atmosphere modifiers**:
   - Receptive: Draw 1 additional card
   - Pressured: Draw 1 fewer card
   - Prepared: Add +1 to current focus
5. **Remove Opening cards** if unplayed (to exhaust)
6. **Check request card activation**:
   - If at required focus capacity: Becomes playable
   - Gains both Impulse AND Opening properties
   - Must play immediately or lose forever

### SPEAK Action

Complete sequence:
1. **Check focus available** (must have enough for card cost)
2. **Choose one card** from hand
3. **Spend focus** equal to card cost from pool
4. **Calculate success chance**:
   - Base difficulty % + (2 × current rapport)
   - Apply atmosphere modifiers if any
5. **Resolve success/failure**:
   - Success: +1 flow, apply card effects
   - Failure: -1 flow, apply failure effects (if any)
6. **Card goes to exhaust pile**
7. **Apply card effects**:
   - Rapport changes
   - Atmosphere changes
   - Draw/focus effects
8. **Remove Impulse cards** if unplayed (to exhaust)
9. **Check flow transitions**:
   - At ±3: State change, flow resets to 0
10. **Can SPEAK again** if focus remains

### Conversation End

Triggers:
- Patience exhausted
- Flow at -3 in Disconnected state
- Request card accepted/failed
- Player chooses to leave
- Final atmosphere failure

Cleanup:
- All piles cleared
- NPC persistent decks unchanged (except consumed observations)
- Rapport resets to 0
- Atmosphere clears to Neutral
- Connection state persists

### Card Persistence Types

#### Persistent (60% of deck)
- Remain in hand until played
- Most common type
- Strategic planning enabled
- Examples: Basic rapport cards, setup cards

#### Impulse (25% of deck)
- Removed after ANY SPEAK action if unplayed
- High-risk, high-reward
- Often 4+ focus cost
- Examples: Dramatic rapport cards, crisis plays

#### Opening (15% of deck)
- Removed after LISTEN if unplayed
- Timing-critical
- Often utility effects
- Examples: Draw cards, focus-add cards

### On Exhaust Effects

Some Impulse and Opening cards have effects when exhausted unplayed:
- "Missed Opportunity": Draw 1 card when exhausted
- "Hasty Words": -1 rapport when exhausted
- "Lost Focus": +1 focus when exhausted

### Card Difficulty Tiers

All modified by: +2% per rapport point

- **Very Easy** (85% base): Observation cards exclusively (TBD final %)
- **Easy** (70% base): Basic cards, safe plays (TBD final %)
- **Medium** (60% base): Standard cards, balanced risk (TBD final %)
- **Hard** (50% base): Powerful effects, scaled rapport (TBD final %)
- **Very Hard** (40% base): Request cards, dramatic effects (TBD final %)

### Normal Card Generation Rules

#### Focus-Effect Correlation

**0 Focus**: Setup cards
- No effect + atmosphere change (Persistent)
- +1 rapport (Easy, Persistent)

**1 Focus**: Basic cards  
- ±1 rapport (Easy, Persistent)
- Draw 1 card (Medium, Opening with on exhaust: Draw 1)

**2 Focus**: Standard cards
- ±2 rapport (Medium, mix of Persistent and Opening)
- Scaled rapport with low ceiling (Hard, Persistent)
- Add 1 focus (Medium, Opening with on exhaust: +1 focus)

**3 Focus**: Powerful cards
- ±3 rapport (Medium, mix of Persistent and Impulse)
- Scaled rapport with medium ceiling (Hard, Persistent)
- Draw 2 cards (Medium, Opening)

**4+ Focus**: Dramatic cards
- ±4 or ±5 rapport (Hard-Very Hard, Impulse)
- Scaled rapport with high ceiling (Hard, Impulse with on exhaust)
- Add 2 focus (Medium, Impulse)

#### Effect Pools

**Fixed Rapport** (Easy-Medium difficulty):
- +1, +2, +3 rapport
- -1, -2 rapport

**High Fixed Rapport** (Hard-Very Hard difficulty):
- +4, +5 rapport
- -3 rapport

**Scaled Rapport** (Hard difficulty):
- +X where X = Trust tokens (max 5)
- +X where X = Commerce tokens (max 5)
- +X where X = Status tokens (max 5)
- +X where X = Shadow tokens (max 5)
- +X where X = (20 - current rapport) ÷ 5
- +X where X = patience ÷ 3
- +X where X = focus remaining

**Utility Effects** (Medium difficulty):
- Draw 1 card
- Draw 2 cards
- Add 1 focus to pool
- Add 2 focus to pool

#### Atmosphere Assignment

Only ~30% of cards change atmosphere:
- 0-focus setup cards usually have atmosphere change
- 4+ focus cards often set "Final" atmosphere
- Token-associated cards might set "Focused"
- Defensive cards might set "Volatile"

### Deck Cycling Example

**Turn 1**: Draw pile has 23 cards (20 conversation + 2 observation + 1 request), active pile has 2 cards
- SPEAK a card → goes to exhaust pile

**Turn 2**: Draw pile has 21 cards, active has 1, exhaust has 1
- LISTEN → draw 2 more cards from draw pile

**Turn 15**: Draw pile empty, active has 3 cards, exhaust has 20 cards
- LISTEN → need to draw 2 cards
- Shuffle exhaust pile (20 cards) → becomes new draw pile
- Draw 2 cards from new draw pile → active pile

This creates natural deck cycling where all cards remain available throughout the conversation.

## Starter Deck Design Principles

### Core Philosophy

The starter deck creates strategic depth through impossible choices, not mechanical complexity. Every turn forces players to choose between multiple suboptimal paths.

### The Focus Paradox

Limited focus capacity creates tension:
- In Disconnected (3 focus): Can play one 3-focus OR three 1-focus cards
- In Guarded (4 focus): Can play one 4-focus OR two 2-focus cards
- In Neutral (5 focus): Can reach request cards requiring 5 focus

This ensures players cannot play everything optimally.

### The Strategic Triangle

Every card serves one of three purposes:

1. **Progress** (Rapport Building)
   - Build rapport to improve success rates
   - Uses focus that could be spent on setup
   - Risk of failure reduces flow

2. **Setup** (Atmosphere/Economy)
   - Create future advantage through effects
   - Delays immediate rapport building
   - May not have turns to capitalize

3. **Information** (Card Draw)
   - Expand options and find key cards
   - No immediate rapport or setup
   - Risk of drawing unplayable cards

### Deck Composition Guidelines

**Total Size**: 12 cards
- Small enough for consistency
- Large enough for variance

**Persistence Distribution**:
- Persistent: 100% initially (no timing pressure early)
- Later decks add Impulse/Opening for complexity

**Focus Distribution**:
- 1-focus: ~40% (always playable)
- 2-focus: ~35% (standard plays)
- 3-focus: ~15% (full capacity plays)
- 4-focus: ~10% (requires setup)

**Effect Categories**:
- Safe progress: ~25% (reliable rapport, low risk)
- Atmosphere setup: ~15% (future advantage)
- Risk/reward: ~20% (efficient but risky)
- Information: ~15% (card draw)
- Powerful: ~25% (high impact)

### Request Card Mechanics

Request cards define conversation goals:
- Focus: 4-6 (based on urgency and state)
- Difficulty: Hard to Very Hard (40-50% base)
- Start unplayable in draw pile
- Become playable at sufficient focus during LISTEN
- Gain Impulse AND Opening when activated
- Must play immediately or lose opportunity

Fixed terms by personality:
- Devoted: Low payment, flexible position
- Mercantile: Good payment, negotiable deadline
- Proud: Urgent deadline, high position
- Cunning: Complex terms, multiple steps

### Mathematical Balance Targets

**Success Rates**: 
- Optimal play: 60-70% success
- Average play: 45-55% success
- Poor play: 30-40% success

**Turn Economy**:
- Average turns to request: 6-8
- Patience usage: 40-50% of available
- Focus efficiency: 75-85% utilized

**Failure Recovery**:
- Negative flow spiral: ~30% of games
- Patience exhaustion: ~10% of games
- Missed request window: ~5% of games

### Design Achievements

- **No Soft Locks**: Multiple low-focus cards ensure playability
- **Meaningful Choices**: Every turn requires trade-offs
- **Clear Progression**: State advancement enables requests
- **Recovery Arcs**: Failures create challenges, not dead ends
- **Perfect Information**: All mechanics transparent

## Queue Management System

### Core Rules
- **Strict Sequential**: Position 1 MUST complete first
- **Maximum Size**: 10 obligations
- **No Reordering**: Except through displacement
- **No Exceptions**: Time-fixed meetings still follow queue rules

### Queue Displacement Mechanics

To deliver out of order, burn tokens with EACH displaced NPC:

**Example 1**: Moving position 3 to position 1:
- Burn 2 tokens with position 1 NPC
- Burn 1 token with position 2 NPC
- Each burn adds 1 burden card to that relationship

**Example 2**: Moving position 5 to position 2:
- Burn 3 tokens with position 2 NPC
- Burn 2 tokens with position 3 NPC
- Burn 1 token with position 4 NPC
- Total: 6 tokens burned, 3 burden cards added

Token type burned matches NPC personality preference:
- Devoted NPCs: Trust tokens
- Mercantile NPCs: Commerce tokens
- Proud NPCs: Status tokens
- Cunning NPCs: Shadow tokens

### Fixed Request Terms

Request cards have predetermined, non-negotiable terms:

**Letter Requests**:
- Deadline: 1-24 hours (based on urgency)
- Position: 1-10 (based on NPC pride)
- Payment: 0-20 coins (based on value)

**Meeting Requests**:
- Time: Specific time block
- Location: Specific spot
- Duration: 1-2 time segments

**Resolution Requests**:
- Target: Clear burden cards
- Difficulty: Scales with burden count
- Reward: Relationship reset

Personality influences available requests:
- Proud NPCs: Urgent, high-position requests
- Devoted NPCs: Personal, low-payment requests
- Mercantile NPCs: Profitable, flexible requests
- Cunning NPCs: Complex, multi-step requests

## Strategic Resource Management

### Resource Pressure Points

#### Morning Attention Calculation
- Base 10 attention
- Hunger reduces it (every 25 hunger = -1 attention)
- Minimum 2 (never completely blocked)
- Creates pressure to manage hunger without hard lock

Example progression:
- Wake at hunger 50: 8 attention available
- Skip breakfast: Hunger 70 by afternoon
- Afternoon: Only 7 attention
- Evening: Hunger 90, only 6 attention

#### Work Output Calculation
- Base 5 coins (varies by work type)
- Hunger reduces it: 5 - floor(hunger/25)
- At hunger 50: only 3 coins
- Creates pressure to eat before working

Optimal sequence:
1. Buy food (2-3 coins)
2. Eat (reset hunger to 0)
3. Work (gain full 5 coins)
4. Net profit: 2-3 coins

#### Investigation Efficiency
- Quiet spots: 1 attention → +2 familiarity
- Busy spots: 1 attention → +1 familiarity
- Creates pressure to investigate at optimal times

Time management:
- Morning investigation at Quiet spot: Efficient
- Afternoon at same spot (now Busy): Half efficiency
- Must balance with other morning priorities

#### Focus Depletion Cascade
- Need high-focus request card → Must reach better state
- Better state needs positive flow → Requires successful cards
- Success needs rapport → Must build early
- Focus depletes → Must LISTEN to refresh
- LISTEN costs patience → Limited turns available

#### Queue Displacement Cascade
- Need urgent delivery → Must displace
- Displacement burns tokens → Future conversations start with less rapport
- Lower starting rapport → Harder conversations
- Harder conversations → More likely to fail requests
- Failed requests → Burden cards accumulate

#### Satchel Limitations
- 5 slots for letters AND permits
- Full satchel blocks new letters
- Dropping letters damages relationships
- Permits compete with profitable letters

### Resource Conservation Strategies

#### Token Preservation
- Accept fixed queue positions to avoid burning
- Focus deliveries on specific NPCs for concentrated tokens
- Use observations to improve success without token cost
- Build tokens with easy deliveries before hard ones

#### Focus Efficiency
- Chain low-focus cards before refreshing
- Use atmosphere to expand capacity
- Time impulse cards before SPEAK removes them
- Plan multi-turn sequences

#### Attention Efficiency
- Investigate during quiet periods for better returns
- Chain obligations in same location
- Use quick exchanges when full conversation not needed
- Prioritize high-value attention uses

#### Time Optimization
- Plan routes to minimize travel
- Accept letters with compatible deadlines
- Use wait actions strategically
- Investigate early for cascading benefits

## Economic Balance Points

### Daily Attention Budget

10 attention (well-fed) allows:
- 5 quick exchanges OR
- 2 investigations + 2 conversations + 1 work OR
- 1 conversation + 8 investigations OR
- 5 conversations (impossible to sustain) OR
- Maximum flexibility with free observations

At hunger 50 (8 attention):
- Lost 20% of action economy
- Must choose between relationship building OR exploration
- Cannot do both effectively

### Work Profitability Analysis

- **Hunger 0**: 5 coins for 2 attention = 2.5 coins/attention
- **Hunger 25**: 4 coins for 2 attention = 2 coins/attention
- **Hunger 50**: 3 coins for 2 attention = 1.5 coins/attention
- **Hunger 75**: 2 coins for 2 attention = 1 coin/attention
- **Hunger 100**: 1 coin for 2 attention = 0.5 coins/attention

Food cost: 2-3 coins typically
Break-even: Must work at hunger <50

### Letter Profitability

Request cards have fixed terms (no negotiation):
- **Typical letter**: 5-15 coins, 2-6 hour deadline
- **Urgent letter**: 3-8 coins, 1-2 hour deadline
- **Valuable letter**: 10-20 coins, 12-24 hour deadline

Success builds tokens for future rapport:
- First delivery: +1 token = easier next conversation
- Chain effect: More tokens → higher success → more tokens

Failure costs:
- -2 tokens with sender
- +2 burden cards
- Relationship damage compounds

### Token Investment Mathematics

Starting rapport from tokens:
- **3 tokens**: 3 rapport = +6% all cards
- **6 tokens**: 6 rapport = +12% all cards
- **10 tokens**: 10 rapport = +20% all cards
- **15 tokens**: 15 rapport = +30% all cards
- **25 tokens**: 25 rapport = +50% all cards (halfway to guarantee)
- **50 tokens**: 50 rapport = +100% all cards (guaranteed success)

Burning tokens for displacement:
- Displacing 1 position: 1 token
- Displacing 3 positions: 6 tokens total
- Displacing 5 positions: 15 tokens total
- Burning 10 tokens severely damages multiple relationships

### Investigation Return on Investment

Morning investigation (Quiet spot):
- Cost: 1 attention + 10 minutes
- Gain: +2 familiarity
- Efficiency: 2 familiarity per attention

Afternoon investigation (Busy spot):
- Cost: 1 attention + 10 minutes  
- Gain: +1 familiarity
- Efficiency: 1 familiarity per attention

Reaching familiarity 3:
- Optimal: 2 morning investigations (2 attention)
- Suboptimal: 3 afternoon investigations (3 attention)
- Difference: 50% more attention cost

Each familiarity level unlocks one observation:
- Familiarity 1: First observation
- Familiarity 2: Second observation (requires first)
- Familiarity 3: Third observation (requires second)

### Focus Management Economics

**Disconnected** (3 capacity):
- Can play: Three 1-focus OR one 3-focus
- Efficiency: 3 cards vs 1 card
- Trade-off: Many small effects vs one big effect

**Neutral** (5 capacity):
- Can play: Five 1-focus OR one 5-focus
- Can reach request cards
- Sweet spot for most conversations

**Trusting** (6 capacity):
- Maximum flexibility
- Can play any combination
- Rarely achieved without token investment

Prepared atmosphere value:
- Adds +1 focus to current capacity
- In Disconnected: Enables 4-focus card (impossible otherwise)
- In Guarded: Enables 5-focus request cards
- Worth 1 turn setup in long conversations

## Resource Conversion Chains

### Time → Money → Progress
```
Investigation (1 attention + 10 min) → Familiarity
Familiarity → Observation access → NPC advantages
Work Action (2 attention + 4 hours) → Coins (scaled by hunger)
Coins → Food (reset hunger) → Better work output next time
Better output → More coins → Critical purchases
```

### Tokens → Rapport → Success → More Tokens
```
Successful deliveries → +1-3 tokens
Higher tokens → Better starting rapport
Better rapport → Higher success rates
More successes → Positive flow → State advancement
State advancement → Request availability
Request success → More deliveries → More tokens
```

### Familiarity → Knowledge → Access → Efficiency
```
Investigation (1 attention) → Location familiarity
Familiarity → Observation availability
Observation → Card to NPC observation deck
NPC observation card → Conversation advantages or unlocks
Unlocked content → New routes or exchanges
New routes → More opportunities
```

### Focus → Cards → Rapport → Flow
```
Higher states → More focus capacity
More capacity → Access to higher focus cards
Higher focus cards → Bigger rapport changes
More rapport → Better success rates
Better success → Positive flow
Flow ±3 → State transition
Better states → Access to requests
```

## Work System

### Work Action Mechanics

**Standard Work**:
- Base output: 5 coins (varies by type)
- Time cost: 4 hours (one time block)
- Attention cost: 2
- Hunger scaling: Output = Base - floor(hunger/25)

**Enhanced Work** (requires access):
- Base output: 6-7 coins
- Time cost: 4 hours
- Attention cost: 2
- May require permits or relationships

**Service Work**:
- Base output: 3-4 coins + benefits
- Time cost: 4 hours
- Attention cost: 2
- Benefits may include meals or resources

### Work Efficiency Optimization

Optimal work sequence:
1. Morning: Eat breakfast (hunger → 0)
2. Morning: Work first job (5 coins)
3. Afternoon: Hunger at 20, still efficient
4. Afternoon: Work second job (5 coins)
5. Evening: Buy food with profits
6. Net gain: 7-8 coins after food costs

Suboptimal sequence:
1. Skip breakfast (save 2 coins)
2. Work at hunger 50 (only 3 coins)
3. Work at hunger 70 (only 2 coins)
4. Must buy food anyway
5. Net gain: 2-3 coins

## Exchange System

### Quick Exchange Mechanics

- **Cost**: 1 attention (vs 2 for full conversation)
- **No card play**: Direct resource trade
- **No rapport building**: Pure transaction
- **Time cost**: Minimal (5-10 minutes)
- **Always available**: If NPC has exchange deck

### Common Exchanges

**Food Exchanges**:
- Simple meal: 2 coins → -30 hunger
- Full meal: 3 coins → -50 hunger
- Feast: 5 coins → -100 hunger, +10 health

**Permit Exchanges**:
- District permit: 15-20 coins
- Special access: 25-30 coins
- Temporary pass: 5-10 coins

**Information Exchanges**:
- Rumors: 3 coins → location hint
- Maps: 10 coins → reveal routes
- Schedules: 5 coins → NPC timings

**Service Exchanges**:
- Healing: 10 coins → +20 health
- Rest: 5 coins → restore stamina
- Storage: 2 coins → bank items

### Token-Gated Exchanges

Some exchanges require minimum tokens:
- **Caravan transport**: 2+ Commerce tokens
- **Secret information**: 3+ Shadow tokens  
- **Noble introduction**: 5+ Status tokens
- **Temple blessing**: 4+ Trust tokens

Combined requirements example:
- Special transport: 2+ appropriate tokens AND relevant observation played

## No Soft-Lock Architecture

Every system has escape valves preventing unwinnable states:

### Conversation Deadlocks - Never Stuck

**Problem**: All cards too expensive for current focus
**Solution**: Five 1-focus cards guarantee something playable

**Problem**: Request card requires 5 focus, stuck in Disconnected
**Solution**: Can leave and return with better preparation

**Problem**: No rapport, everything failing
**Solution**: 70% base success on easy cards still likely

**Problem**: Patient atmosphere needed but can't set it
**Solution**: Can leave conversation and try different approach

**Problem**: Observation cards provide critical advantage
**Solution**: Can investigate locations to gain them

### Queue Deadlocks - Always Options

**Problem**: Queue full, can't accept new letters
**Solution**: Can drop letters (with relationship cost)

**Problem**: Can't reach position 1 in time
**Solution**: Can displace with tokens (if available)

**Problem**: No tokens to displace
**Solution**: Can let deadline pass, accept consequences

**Problem**: Critical letter blocked by trivial ones
**Solution**: Can complete trivial ones quickly

### Travel Deadlocks - Multiple Paths

**Problem**: Need permit but can't afford
**Solution**: Multiple NPCs provide same permits

**Problem**: Route blocked by encounter
**Solution**: Can build resources and return

**Problem**: No money for permits
**Solution**: Can work for coins

**Problem**: Location inaccessible
**Solution**: Observations can unlock alternate routes

### Resource Deadlocks - Recovery Possible

**Problem**: No attention (hunger at 100)
**Solution**: Minimum 2 attention always available

**Problem**: No money for food
**Solution**: Can work even when hungry (reduced output)

**Problem**: No tokens for rapport
**Solution**: Base success rates still functional

**Problem**: Health critically low
**Solution**: Rest options available (TBD)

## Content Scalability

### Adding NPCs - Simple Framework

New NPCs simply need:
- **Personality type**: Determines patience and token preference
- **Five persistent decks**:
  - Conversation deck (20 cards following template)
  - Request deck (available conversation types)
  - Observation deck (receives location discoveries)
  - Burden deck (damaged relationship tracking)
  - Exchange deck (if mercantile)
- **Token rewards**: For successful deliveries
- **Starting state**: Connection state with player
- **Location**: Where they can be found

### Adding Locations - Modular Design

New locations need:
- **Spot definitions**: With time-based properties
  - Morning: Quiet/Normal/Busy
  - Afternoon: Quiet/Normal/Busy
  - Evening: Closing/Open/Busy
- **Investigation rewards**: At each familiarity level
- **Observation destinations**: Which NPC gets each observation
- **NPCs present**: Who is where when
- **Routes**: Connections and permit requirements

### Adding Cards - Clear Rules

New cards must follow:
- **Single effect**: One clear purpose (not multiple)
- **Focus range**: 0-6 focus cost
- **Difficulty tier**: Very Easy to Very Hard
- **Persistence type**: Persistent (60%), Impulse (25%), Opening (15%)
- **Atmosphere**: Only ~30% change atmosphere
- **Scaling**: Either fixed OR scaled, never both

### Adding Observation Rewards

New observations need:
- **Source location**: Where discovered
- **Familiarity requirement**: 1, 2, or 3
- **Prior observation**: Prerequisites if any
- **Target NPC**: Who receives the card
- **Effect type**: State change, unlock, or advantage
- **Consumption**: Always consumed when played

## The Holistic Experience

### Daily Routine Example

**Morning (6-10 AM)**:
- Check queue, see deadlines
- Attention calculation based on hunger
- Investigate quiet locations for maximum familiarity
- Accept morning letters at good positions
- Work if coins needed

**Afternoon (10 AM - 2 PM)**:
- Locations shift to busy
- Investigation less efficient
- Focus on conversations and deliveries
- Complete position 1 obligations
- Use observations gained morning

**Evening (2-6 PM)**:
- Locations begin closing
- Rush to complete deadlines
- Make difficult displacement decisions
- Burn tokens if necessary
- See results of day's choices

**Night (6-10 PM)**:
- Limited location access
- Focus on available NPCs
- Rest and recovery options
- Plan next day's route
- Manage hunger before sleep

### Emergent Narratives

Stories emerge from mechanical interaction, not scripting:

**Desperate NPC Scenario**:
- Disconnected state (failed relationship)
- Only 3 focus capacity (can't reach request)
- Needs observation card to advance state
- Player must investigate relevant location
- Discovers observation that helps this NPC
- Returns with new conversation option
- Plays observation → advances connection state
- Can now reach higher focus request cards
- Accepts obligation with character-appropriate terms

**Trust Building Scenario**:
- Starts at Neutral (professional relationship)
- Player delivers letters efficiently
- Gains appropriate token type (+2 per delivery)
- Tokens provide starting rapport
- Easier conversations each time
- Eventually unlocks special exchanges
- May need observation cards to activate
- Creates investigation goals

**Queue Management Crisis**:
- Position 1: Urgent letter, 2 hours left
- Position 2: Valuable package, good payment
- Position 3: Personal request, no payment
- Can't complete in order in time
- Must burn tokens to displace
- Each displacement damages a relationship
- Strategic choice about which relationships to sacrifice

### Strategic Mastery Progression

**Beginner**: 
- Takes any available letter
- Investigates randomly
- Works when broke
- Ignores token economy

**Intermediate**:
- Plans queue order
- Investigates at optimal times
- Manages hunger efficiently
- Builds specific token types

**Advanced**:
- Chains obligations efficiently
- Pre-builds observation advantages
- Optimizes attention allocation
- Maintains token portfolios

**Expert**:
- Predicts conversation needs
- Routes perfectly
- Never wastes resources
- Achieves seemingly impossible deliveries

## Content Loading System

### Package Architecture

Content organized in self-contained JSON packages that can:
- Load in any order
- Reference non-existent content
- Be generated by AI
- Replace skeleton content

### Package Structure

```json
{
  "packageId": "unique_package_id",
  "metadata": {
    "name": "Package Name",
    "timestamp": "2025-01-01T00:00:00Z",
    "description": "Package description",
    "author": "Author name",
    "version": "1.0.0"
  },
  "startingConditions": {
    "coins": 10,
    "health": { "current": 100, "max": 100 },
    "hunger": { "current": 50, "max": 100 },
    "attention": 10
  },
  "content": {
    "cards": [...],
    "npcs": [...],
    "locations": [...],
    "spots": [...],
    "routes": [...],
    "observations": [...]
  }
}
```

### Skeleton System

When content references missing entities:

1. **Detection**: PackageLoader detects missing reference
2. **Generation**: SkeletonGenerator creates placeholder
   - Mechanically complete (all required stats)
   - Narratively generic (procedural names)
   - Deterministic (same ID → same skeleton)
3. **Registration**: Tracked in SkeletonRegistry
4. **Resolution**: Real content replaces skeleton
5. **State Preservation**: Accumulated state transfers

Example skeleton NPC:
```
Name: "Unnamed Merchant #47"
Personality: Mercantile (from hash)
Patience: 12 (from personality)
State: Neutral (default)
All 5 decks: Present but empty/minimal
IsSkeleton: true
```

### Load Order Independence

Packages can load in any sequence:
- Package A references Location X (doesn't exist)
- Skeleton created for Location X
- Package B defines Location X
- Skeleton replaced with real Location X
- Game remains playable throughout

### Content Directories

- `Content/Core/`: Essential game content
- `Content/Expansions/`: Additional content packs
- `Content/Generated/`: AI-generated packages
- `Content/TestPackages/`: Testing content

## Core Innovation Summary

The three loops create a complete game where:

### 1. Conversations
Provide puzzle challenge through:
- Focus management across multiple turns
- Rapport building for success rates
- Flow navigation for state changes
- Atmosphere manipulation for advantages
- Observation cards from exploration

### 2. Queue Management
Provides time pressure through:
- Forced sequential completion (position 1 first)
- Token-burning displacement costs
- Fixed deadline pressure
- Relationship damage from failures
- Strategic obligation chaining

### 3. Travel and Exploration
Provides discovery through:
- Location familiarity building
- Time-efficient investigation
- Observation card rewards
- Permit requirements
- Route optimization

### The Intersection IS the Game

Each loop uses different mechanics operating on shared resources:
- **Tokens**: Bridge conversations and queue through starting rapport and displacement
- **Familiarity**: Bridges exploration and conversations through observations
- **Time**: Affects all three but manifests differently
- **Attention**: Enables all three but must be allocated strategically
- **Rapport**: Creates success momentum unique to conversations

The elegance: No mechanic serves two purposes, yet resources flow through multiple systems creating strategic depth from simple rules.

## Design Verification Checklist

### Clean Mechanical Separation ✓
- Each mechanic has exactly ONE purpose
- No "OR" conditions in requirements
- Effects don't overlap or combine
- Clear cause and effect chains

### Perfect Information ✓
- All calculations visible to player
- Success rates shown before playing cards
- Focus costs displayed on all cards
- Displacement costs shown in queue
- Time costs shown on routes
- Resource formulas transparent

### Linear Scaling ✓
- Each rapport point: exactly +2% success
- Each token: exactly 1 starting rapport
- Each 25 hunger: exactly -1 attention
- Each patience: exactly 1 turn
- Each focus point: exactly that much from pool
- Only exception: Flow ±3 triggers state change

### No Soft-Locks ✓
- Always have playable cards (1-focus options)
- Always have minimum 2 attention
- Always can displace (if accept token cost)
- Always can work (even if inefficient)
- Always can leave conversations
- Multiple sources for permits

### Resource Flow ✓
- Tokens flow from deliveries to rapport to success
- Familiarity flows from investigation to observations
- Coins flow from work to food to efficiency
- Time flows through all systems creating pressure
- Attention allocates between all activities

### Emergent Complexity ✓
- Simple rules create complex decisions
- Resource interactions create cascades
- Multiple viable strategies exist
- Failure creates recovery arcs
- Mastery comes from understanding interactions

## Critical Formulas Reference

**Success Rate**: Base% + (2 × Current Rapport)

**Starting Rapport**: Connection Tokens with NPC

**Morning Attention**: 10 - (Hunger ÷ 25), minimum 2

**Work Output**: 5 - floor(Hunger/25) coins

**Investigation Gain**: 
- Quiet spots: +2 familiarity
- Busy spots: +1 familiarity
- Other: +1 familiarity

**Displacement Cost**: 
- To position N from position M: 
- Burn (M-N) tokens with position N
- Burn (M-N-1) tokens with position N+1
- ... continue for each displaced position

**Time per Patience**: 10 minutes game time

**Hunger Increase**: +20 per time period

**Starvation**: At 100 hunger, lose 5 health per period

**Flow State Change**: At ±3, shift state and reset to 0

**Focus Capacity**:
- Disconnected: 3
- Guarded: 4
- Neutral: 5
- Receptive: 5
- Trusting: 6

**Card Draws**:
- Disconnected: 1
- Guarded: 2
- Neutral: 2
- Receptive: 3
- Trusting: 3

**Patience Base**:
- Devoted: 15
- Steadfast: 13
- Mercantile: 12
- Cunning: 12
- Proud: 10

## Implementation Priority

### Phase 1: Core Conversation System ✓
- Three-pile card system
- Focus/rapport/flow mechanics
- Connection states
- Atmosphere effects
- Basic starter deck

### Phase 2: Queue Management ✓
- Sequential execution rules
- Displacement with token burning
- Fixed request terms
- Deadline pressure
- Obligation types

### Phase 3: Location System ✓
- Familiarity building
- Investigation scaling
- Observation rewards
- Spot properties by time
- NPC observation decks

### Phase 4: Resource Economy ✓
- Token system
- Hunger/attention interaction
- Work scaling
- Exchange system
- Time segments and blocks

### Phase 5: Content Loading ✓
- Package structure
- Skeleton generation
- Load order independence
- State preservation
- AI-ready format

## Conclusion

Wayfarer achieves its design goals through:

1. **Mechanical Elegance**: Every system serves one clear purpose
2. **Meaningful Choices**: No optimal path, only trade-offs
3. **Emergent Narrative**: Stories arise from mechanics, not scripting
4. **Strategic Depth**: Simple rules create complex interactions
5. **Perfect Information**: All calculations transparent
6. **No Soft-Locks**: Always a path forward
7. **Scalable Content**: Easy to extend without breaking

The game succeeds when players realize that mastery comes not from optimizing individual systems, but from understanding how resources flow between them, creating cascading consequences from every decision.

## Resource Economy

### Persistent Resources

#### Coins
- **Range**: 0-999
- **Generation**: Work actions, letter deliveries, exchanges
- **Uses**: Food, rest, exchanges, caravan transport
- **No decay**

#### Health
- **Range**: 0-100
- **Effects**: TBD
- **Loss**: TBD
- **Restoration**: TBD

#### Hunger
- **Range**: 0-100
- **Effects**:
  - Reduces morning attention: 10 - (Hunger ÷ 25), minimum 2
  - Reduces work output: 5 - floor(Hunger ÷ 25) coins
  - At 100: Lose 5 health per time period
- **Increase**: +20 per time period automatically
- **Restoration**: Food purchases, meals

#### Attention
- **Daily Allocation**: 10 - (Hunger ÷ 25), minimum 2
- **Costs**:
  - Standard conversation: 2
  - Quick exchange: 1
  - Investigation: 1
  - Work: 2
  - Observation: 0
- **Cannot be saved between days**

#### Location Familiarity
- **Range**: 0-3 per location
- **Generation**: Investigation action only
  - Quiet spots: +2 familiarity per investigation
  - Busy spots: +1 familiarity per investigation
  - Other spots: +1 familiarity per investigation
- **Never decreases**
- **Enables observations at threshold levels**

#### Connection Tokens
Four types: **Trust**, **Commerce**, **Status**, **Shadow**

- **Effect**: Each token = 1 starting rapport in conversations
- **Generation**: Successful letter delivery (+1 to +3 based on quality)
- **Additional Uses**:
  - Queue displacement (burn tokens permanently)
  - Some cards scale with token count
  - Gate certain exchanges (minimum required)
- **Token burning for displacement**:
  - Burns with displaced NPC's preferred type
  - Devoted NPCs: Trust tokens
  - Mercantile NPCs: Commerce tokens
  - Proud NPCs: Status tokens
  - Cunning NPCs: Shadow tokens

### Per-Conversation Resources

#### Focus
- **Capacity by Connection State**:
  - Disconnected: 3
  - Guarded: 4
  - Neutral: 5
  - Receptive: 5
  - Trusting: 6
- **Mechanics**:
  - Pool persists across SPEAK actions
  - Refreshes to maximum on LISTEN
  - Prepared atmosphere adds +1 to current capacity
  - Can exceed maximum temporarily

#### Rapport
- **Range**: -50 to +50
- **Starting Value**: Equal to connection tokens with NPC
- **Effect**: +2% success rate per point on all cards
- **Changes**: Through card effects only
- **Resets**: After conversation ends

#### Flow
- **Range**: -3 to +3
- **Changes**: +1 on success, -1 on failure
- **Effect**: At ±3 triggers connection state change
- **Resets**: To 0 when state changes

#### Atmosphere
**Standard Atmospheres** (~30% of cards):
- **Neutral**: No effect (default after failure)
- **Prepared**: +1 focus capacity
- **Receptive**: +1 card on LISTEN
- **Focused**: +20% success all cards
- **Patient**: Actions cost 0 patience
- **Volatile**: All rapport changes ±1
- **Final**: Any failure ends conversation

**Observation-Only Atmospheres**:
- **Informed**: Next card cannot fail
- **Exposed**: Double all rapport changes
- **Synchronized**: Next card effect happens twice
- **Pressured**: -1 card on LISTEN

**Persistence**: Remains until changed or cleared by failure

#### Patience
- **Base Values by Personality**:
  - Devoted: 15
  - Steadfast: 13
  - Mercantile: 12
  - Cunning: 12
  - Proud: 10
- **Modifiers**:
  - Private spot: +1
  - Public spot: -1
  - Patient atmosphere: Actions cost 0
- **Effect**: Maximum turns in conversation (LISTEN costs 1 turn)

### Time Resources

#### Time Structure
- **Days** → **Time Blocks** → **Time Segments**
- **Time Blocks** (6 per day):
  - Dawn (2-6 AM)
  - Morning (6-10 AM)
  - Afternoon (10 AM - 2 PM)
  - Evening (2-6 PM)
  - Night (6-10 PM)
  - Late Night (10 PM - 2 AM)
- **Time Segments**: 4 per time block
- **Time Costs**:
  - Some actions cost 1-2 segments
  - Travel cards may consume segments
  - Extended conversations increase segment cost
  - When segments exceed block, advance to next block

#### Deadlines
- **Range**: Typically 1-24 hours
- **Effect of Missing**: 
  - -2 tokens with sender
  - +2 burden cards to sender's relationship
  - No payment received
  - Permanent relationship damage

### Information Resources

#### Observation Cards
- **Source**: Location observations at familiarity thresholds
- **Destination**: Specific NPC's observation deck
- **Properties**:
  - Focus 0 (costs SPEAK action but no focus)
  - Always persistent
  - Consumed when played
  - Can advance connection states
  - Can unlock exchanges
- **Requirements**:
  - First observation: Familiarity 1+
  - Second: Familiarity 2+ AND first observation done
  - Third: Familiarity 3+ AND second observation done

#### Access Permits
- **Type**: Special items, not obligations
- **Properties**:
  - Occupy satchel space (max 5 items total)
  - Enable specific routes
  - Never expire
  - Physical documents
- **Acquisition**:
  - Request cards (fixed terms)
  - Exchanges (15-20 coins typically)

#### Burden Cards
- **Mechanics**: TBD
- **Acquisition**: Failed requests, queue displacement, broken promises
- **Effects**: TBD
- **Resolution**: TBD

## Conversation System

### NPC Five-Deck System
Each NPC maintains five persistent decks:

1. **Conversation Deck**: 20 standard cards for dialogue
2. **Request Deck**: Goal cards enabling special conversations
3. **Observation Deck**: Cards from location discoveries
4. **Burden Deck**: Cards from failed obligations
5. **Exchange Deck**: Commerce options (mercantile NPCs only)

Available conversation types depend on deck contents.

### Connection States
Determine focus capacity and card draws:

| State | Focus Capacity | Cards Drawn |
|-------|---------------|-------------|
| Disconnected | 3 | 1 |
| Guarded | 4 | 2 |
| Neutral | 5 | 2 |
| Receptive | 5 | 3 |
| Trusting | 6 | 3 |

At -3 flow in Disconnected: Conversation ends immediately.

### Three-Pile System

#### Starting a Conversation
1. Choose conversation type (based on available NPC decks)
2. Build draw pile from relevant cards:
   - All conversation deck cards
   - All observation deck cards (if any)
   - Relevant request card (if applicable)
3. Shuffle draw pile
4. Draw cards equal to connection state
5. Set focus to connection state maximum

#### LISTEN Action
1. Costs 1 patience (unless Patient atmosphere)
2. Draw cards equal to connection state
3. If draw pile empty: Shuffle exhaust → new draw pile
4. Refresh focus to maximum
5. Remove Opening cards if unplayed
6. Check if request cards become playable

#### SPEAK Action
1. Choose one card from hand
2. Spend focus equal to card cost
3. Resolve success/failure
4. Card goes to exhaust pile
5. Apply effects
6. Remove Impulse cards if unplayed

#### Conversation End
- All piles cleared
- NPC decks unchanged (except consumed observations)
- Rapport resets
- Atmosphere clears

### Card Types

#### Persistence Types
- **Persistent** (60%): Remain in hand until played
- **Impulse** (25%): Removed after any SPEAK action if unplayed
- **Opening** (15%): Removed after LISTEN if unplayed

#### Difficulty Tiers
- **Very Easy**: TBD base %
- **Easy**: TBD base %
- **Medium**: TBD base %
- **Hard**: TBD base %
- **Very Hard**: TBD base %

All modified by: +2% per rapport point

### Request Card Mechanics
1. Player chooses request conversation type
2. Request card added to draw pile (starts unplayable)
3. Becomes playable when reaching required focus capacity during LISTEN
4. When playable: Gains both Impulse AND Opening properties
5. Must play immediately or fails
6. Success: Accept obligation with fixed terms
7. Failure: Add burden card to relationship

## Queue Management System

### Core Rules
- **Strict Sequential**: Position 1 MUST complete first
- **Maximum Size**: 10 obligations
- **No Reordering**: Except through displacement

### Queue Displacement
To deliver out of order, burn tokens with EACH displaced NPC:

**Example**: Moving position 3 to position 1:
- Burn 2 tokens with position 1 NPC
- Burn 1 token with position 2 NPC
- Each burn adds 1 burden card to that relationship

Token type burned matches NPC personality preference.

### Fixed Request Terms
Request cards have predetermined, non-negotiable terms:
- **Letters**: Specific deadline, position, payment
- **Meetings**: Fixed time and location
- **Resolutions**: Clear existing burden cards

No negotiation mechanics - terms are set by request type and NPC personality.

## Location and Travel System

### Familiarity System
- **Range**: 0-3 per location
- **Building**: Investigation action only
- **Efficiency**: Varies by spot property
  - Quiet: 1 attention → +2 familiarity
  - Busy: 1 attention → +1 familiarity
  - Other: 1 attention → +1 familiarity

### Observation System
- **Cost**: 0 attention (just noticing)
- **Requirements**: 
  - Minimum familiarity level
  - All prior observations at location
- **Effect**: Adds card to specific NPC's observation deck

### Spot Properties
Change by time block:
- **Morning**: Often Quiet
- **Afternoon**: Often Busy
- **Evening**: Often Closing
- Properties affect investigation efficiency

### Routes and Permits
- Every route requires specific permit
- No alternatives (no "OR" conditions)
- Multiple NPCs may provide same permit
- Permits are physical items taking satchel space

### Travel Encounters
**Mechanics**: TBD
- Bandits, guards, merchants mentioned
- Use conversation mechanics with special decks

## Exchange System

### Quick Exchanges
- **Cost**: 1 attention (vs 2 for conversation)
- **Mechanics**: Simple resource trade, no card play
- **Examples**:
  - 2 coins → Reset hunger to 0
  - 10 coins → Caravan transport
  - 20 coins → Access permit
- **Requirements**: May need minimum tokens or observation cards

### Exchange Availability
- Determined by NPC's exchange deck contents
- Mercantile NPCs have more exchanges
- Some unlocked by observation cards
- Some gated by token minimums

## Work System

### Work Actions
- **Cost**: 2 attention
- **Time**: Advances one full time block (4 hours)
- **Base Output**: 5 coins (varies by work type)
- **Hunger Scaling**: Output = 5 - floor(hunger/25)
  - At hunger 0: 5 coins
  - At hunger 50: 3 coins
  - At hunger 100: 1 coin

## Rest System
**Mechanics**: TBD
- Various rest types mentioned
- Variable time costs
- Resource restoration effects

## Health System
**Mechanics**: TBD
- Range 0-100
- Loss conditions undefined
- Recovery methods undefined
- Effects at low health undefined

## Content Loading System

### Package Architecture
- Content organized in JSON packages
- Each package self-contained
- Can reference non-existent content
- Load order independent

### Skeleton System
When content references missing entities:
1. System creates "skeleton" placeholder
2. Skeleton is mechanically complete but narratively generic
3. When real content loads, skeleton replaced
4. Preserves any accumulated state (like observation cards)

### Package Format
```json
{
  "packageId": "unique_id",
  "metadata": {...},
  "content": {
    "cards": [...],
    "npcs": [...],
    "locations": [...],
    "observations": [...]
  }
}
```

## No Soft-Lock Guarantees

### Conversation Escapes
- Focus 1 cards always playable at minimum capacity
- Can LISTEN to refresh focus
- Can leave conversation
- Patient atmosphere removes patience cost

### Queue Escapes
- Can always displace (at token cost)
- Can drop letters (at relationship cost)
- Can wait for deadlines to pass

### Travel Escapes
- Multiple NPCs provide same permits
- Can earn coins through work
- Observations provide alternate solutions

## Strategic Resource Cascades

### Token → Rapport → Success → Tokens
1. Successful deliveries → +1-3 tokens
2. Higher tokens → Better starting rapport
3. Better rapport → Higher success rates
4. More successes → More deliveries

### Investigation → Familiarity → Observations → Advantages
1. Investigation (attention + time) → Location familiarity
2. Familiarity thresholds → Observation access
3. Observations → NPC-specific advantages
4. Advantages → Easier conversations/unlocks

### Focus → Cards → Flow → States
1. Higher states → More focus capacity
2. More capacity → Access to powerful cards
3. Success with cards → Positive flow
4. Flow ±3 → State advancement

## Key Formulas

- **Success Rate**: Base % + (2 × Rapport)
- **Starting Rapport**: Connection tokens with NPC
- **Morning Attention**: 10 - (Hunger ÷ 25), minimum 2
- **Work Output**: 5 - floor(Hunger/25)
- **Investigation Gain**: Quiet=+2, Busy=+1, Other=+1
- **Displacement Cost**: Tokens = positions jumped with each NPC

## Design Verification Checklist

✓ Each mechanic has exactly ONE purpose
✓ No "OR" conditions in requirements
✓ All effects visible to player
✓ Linear scaling (no thresholds except flow ±3)
✓ Always an escape path
✓ Resources flow through multiple systems
✓ Perfect information for decisions