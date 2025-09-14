# Wayfarer: Complete Game Mechanics

## Table of Contents
1. [Core Design Philosophy](#core-design-philosophy)
2. [Three Core Game Loops](#three-core-game-loops)
3. [Travel System](#travel-system)
4. [Resource Flow Between Loops](#resource-flow-between-loops)
5. [Resource Economy](#resource-economy)
6. [Conversation System](#conversation-system)
7. [Player Deck Design and Progression](#player-deck-design-and-progression)
8. [Queue Management System](#queue-management-system)
9. [Strategic Resource Management](#strategic-resource-management)
10. [Economic Balance Points](#economic-balance-points)
11. [Resource Conversion Chains](#resource-conversion-chains)
12. [Work System](#work-system)
13. [Exchange System](#exchange-system)
14. [No Soft-Lock Architecture](#no-soft-lock-architecture)
15. [Content Scalability](#content-scalability)
16. [The Holistic Experience](#the-holistic-experience)
17. [Content Loading System](#content-loading-system)
18. [Core Innovation Summary](#core-innovation-summary)
19. [Design Verification](#design-verification-checklist)

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
**GOOD**: Tokens unlock NPC signature cards. Card levels improve success. Two separate mechanics.

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
- **What provides challenge?** Managing your deck against NPC personality rules
- **Why grow stronger?** Leveling cards and gaining new ones improves success across all conversations
- **Why engage with NPCs?** Request cards provide income, access, world progression, and new cards for your deck

#### The Conversation as Core Activity

Conversations are the primary gameplay loop - your "combat encounters" expressed through social dynamics. The player owns a conversation deck representing their growing social repertoire. This deck is used in all conversations but each NPC relationship modifies the available card pool through their unique signature cards.

The mechanical depth comes from playing YOUR cards against each NPC's personality rules, creating different puzzles with the same base deck. Success in conversations leads to request completion, which grants new cards and levels existing ones, making you permanently stronger for all future conversations.

#### Connection States

- **Disconnected**: 3 focus capacity, 3 cards drawn on LISTEN
- **Guarded**: 4 focus capacity, 3 cards drawn on LISTEN
- **Neutral**: 5 focus capacity, 4 cards drawn on LISTEN
- **Receptive**: 5 focus capacity, 4 cards drawn on LISTEN
- **Trusting**: 6 focus capacity, 5 cards drawn on LISTEN

At -3 flow in Disconnected: Conversation ends immediately.

#### NPC Deck Systems

Each NPC maintains four persistent decks and a list of requests:

**Four Persistent Decks**:
1. **Signature Deck**: Unique cards unlocked by token thresholds (replaces conversation deck)
2. **Observation Deck**: Cards from location discoveries relevant to this NPC
3. **Burden Deck**: Cards from failed obligations and damaged relationships
4. **Exchange Deck**: Commerce options (mercantile NPCs only)

**Request System**:
NPCs have a list of Requests, not a deck. Each Request is a bundled package containing:
- Multiple request cards (representing different goal thresholds)
- Associated promise cards (that can manipulate the queue)
- Status tracking (available, completed, failed)

When a player selects a request conversation type, all cards from that Request bundle (both request cards and promise cards) are added to the conversation draw pile.

#### NPC Signature Cards

Each NPC has 5 unique signature cards unlocked at specific token thresholds:
- **1 token**: Basic signature card
- **3 tokens**: Intermediate signature card
- **6 tokens**: Advanced signature card
- **10 tokens**: Elite signature card
- **15 tokens**: Legendary signature card

These cards are specific to that NPC, not generic token type cards. Marcus doesn't give "Commerce cards," he gives "Marcus's Bargain," "Silk Road Knowledge," and "Marcus's Favor." Elena gives "Elena's Trust," "Shared Burden," and "Elena's Hope." These cards mechanically represent the nature of your specific relationship.

#### Personality Rules

Each personality type applies one rule that fundamentally changes how conversations work:

- **Proud**: Cards must be played in ascending focus order each turn (resets on LISTEN)
- **Devoted**: When rapport decreases, it decreases twice
- **Mercantile**: Your highest focus card each turn gains +30% success
- **Cunning**: Playing same focus as previous card costs -2 rapport
- **Steadfast**: All rapport changes are capped at ±2 per card

These rules represent how different personalities respond to conversation. A Proud person needs escalating respect. A Devoted person catastrophizes failure. A Merchant rewards getting to the point.

#### Conversation Outputs
- **Card Progression**: Successful plays grant XP to that specific card
- **New Cards**: Request completion grants new cards to player deck
- **Tokens**: Gained through successful letter delivery (+1 to +3)
- **Observations**: Cards added to specific NPCs' observation decks
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

## Travel System

### Core Concept

Travel uses persistent path cards that start face-down and flip permanently once discovered. Unlike conversations (probability-based) or exchanges (resource trades), travel is about **discovery through exploration**. Each route segment presents 2-3 path choices showing only stamina cost until revealed.

### Path Card Mechanics

#### Card States

Each path card exists in one of two permanent states:
- **Face-down**: Shows only name and stamina cost
- **Face-up**: Shows full requirements and effects

Once a card flips face-up (through play or revelation), it remains face-up forever for all future travels on that route.

#### Card Properties

```
Path Card Structure:
- Name: Descriptive identifier  
- Stamina Cost: 0-4 stamina to play
- Requirements: Additional costs (coins, permits, tokens)
- Effect: What happens when played (time, resources, discoveries)
- Hidden: Boolean - cannot be revealed except by playing
- One-Time: Boolean - special rewards only on first play
```

#### Playing Path Cards

1. Each segment presents all path cards for that segment
2. Player sees face-down cards (name + stamina) and face-up cards (everything)
3. Player chooses one card they can afford (stamina + requirements)
4. Card flips face-up permanently if face-down
5. Apply card effects
6. Continue to next segment or complete travel

#### Cannot Proceed

If player cannot meet requirements for ANY path in a segment:
- Must turn back immediately
- Time already spent is lost
- Stamina already spent is lost
- Return to starting location

### Travel States (Stamina System)

Similar to Connection States in conversations:

#### States and Capacity
- **Fresh**: 3 stamina capacity, can play most paths
- **Steady**: 4 stamina capacity, optimal state  
- **Tired**: 2 stamina capacity, limited options
- **Weary**: 1 stamina capacity, only cheap paths
- **Exhausted**: 0 stamina capacity, must REST

#### State Management
- Start each journey in Fresh state
- Each segment may affect state (future content)
- REST action: Skip segment, add 30 minutes, refresh stamina to capacity
- Some paths restore stamina as part of effect

### Core Mechanic: Card Selection

Every travel segment presents 2-3 cards. The player picks ONE card, pays its cost, and applies its effect. This mechanic remains identical across all transport modes.

### Two Travel Modes

#### Walking Routes: Fixed Path Cards

When walking, each segment has permanently assigned path cards representing physical routes:
- Cards start face-down
- Flip permanently face-up when first played
- Same cards always available at each segment
- Creates mastery through repeated travel

#### Caravan Routes: Event Collections

When using transport, each segment draws from event pools:
- Draw one event randomly per segment
- Each event contains 2-3 thematically related cards
- Player picks one card from the drawn event
- Different events each journey

The decision structure is identical in both modes: pick one card from available options. Only the source differs:
- Walking = fixed cards per segment (exploration leading to mastery)
- Caravan = random events per segment (adaptation to circumstances)

### Knowledge Discovery Methods

#### 1. Direct Exploration
- Play the path card spending stamina
- Card flips face-up permanently
- Most expensive but always available
- Only method for hidden paths

#### 2. Investigation
Location investigation can reveal specific path cards:
```
Location Investigation:
- Familiarity 1+: Reveals 1-2 basic paths
- Familiarity 2+: Reveals intermediate paths
- Familiarity 3+: Reveals all non-hidden paths on one route
```

#### 3. NPC Conversations
Conversation cards can reveal paths NPCs know:
```
Card Effect Examples:
- "+1 rapport, reveals all non-hidden paths on [route]"
- "+2 rapport, reveals cheapest path on each segment"
```

#### 4. Exchanges
Direct purchase of route knowledge:
```
Exchange Examples:
- "Buy Map": 5 coins → Reveal all non-hidden paths on one route
- "Route Intelligence": 10 coins → Reveal all paths on all routes from location
```

#### 5. Observation Cards
Some observations reveal paths instead of going to NPC decks:
```
Observation Effect:
- "Complete Route Survey" → Reveals all paths on specific route
```

### Path Effects

#### Standard Effects
- **Time**: Add minutes to travel time
- **Resources**: Gain/lose coins, hunger, health
- **Stamina**: Restore or drain stamina
- **Discoveries**: Find observation cards or items
- **Events**: Draw from event deck (walking routes only)

#### One-Time Discoveries

Certain paths have permanent world effects on first play:
- Find coins or items (marked "already looted" after)
- Discover observation cards (marked "already taken" after)
- Unlock new routes (permanent world change)
- Gain tokens (one-time relationship bonus)

After first play, these paths show modified effects indicating the discovery was already claimed.

### Event System Architecture

For routes using event collections (transport/caravan):

#### Event Cards
Individual cards with costs and effects:
```
Event Card Properties:
- Name and narrative text
- Resource costs (attention for caravans, not stamina)
- Requirements (coins, items, tokens)
- Effects (time, resources, discoveries)
```

#### Events
Thematic groupings of 2-3 related cards:
```
Event Properties:
- Event name and narrative
- Collection of 2-3 event card references
- Player chooses one card when event occurs
```

#### Event Collections
Pools of events for random selection:
```
Collection Properties:
- Collection name
- List of event references
- One event drawn randomly per segment
```

### Resource Logic

#### Walking Resources
- **Stamina**: Physical exertion to traverse paths
- **Coins**: Tolls, bribes, shortcuts
- **Permits**: Gate access requirements
- **Time**: Accept longer paths to avoid costs

#### Transport Resources
- **Attention**: Engage with opportunities (NOT stamina - you're riding)
- **Coins**: Purchase advantages or comfort
- **Items**: Letters, permits affect specific events
- **Time**: Accept delays for free options

### Strategic Considerations

#### Knowledge Investment

Players choose between:
- **Exploration** (costs stamina, risks dead ends)
- **Investigation** (costs attention, reveals infrastructure)
- **Conversation** (costs focus, reveals NPC knowledge)
- **Exchange** (costs coins, reveals complete routes)

#### Route Mastery Progression

- **First journey**: Expensive exploration, suboptimal paths
- **Second journey**: Some knowledge, better choices
- **Third journey**: Optimal path selection based on current needs
- **Mastered route**: Perfect information for planning

#### Dead End Avoidance

Revealing requirements prevents wasted journeys:
- Know permit requirements before attempting
- Know toll costs before traveling
- Know which paths have valuable discoveries
- Know which paths are traps or dead ends

### Integration with Other Systems

#### Investigation Integration
Location familiarity levels unlock path revelations:
- Each familiarity level can reveal specific paths
- Higher familiarity reveals more valuable information
- Familiarity 3 often reveals complete route knowledge

#### Conversation Integration
NPCs can provide route knowledge through:
- Standard conversation cards with reveal effects
- Observation cards that unlock route information
- Exchange options for complete route maps

#### Observation Integration
Travel discoveries can create observation cards:
- Found items go to specific NPC observation decks
- Create conversation advantages
- May unlock special exchanges or routes

### Event Deck Management

Some walking paths trigger random events:

#### Event Decks
Small decks (3-5 cards) for minor randomness:
- Draw one card when path triggers event
- Apply effect after path's base effect
- Reshuffle when deck empty
- Adds variety to fixed routes

#### Event Types
- **Encounters**: NPCs, beggars, vendors
- **Obstacles**: Crowds, accidents, weather
- **Opportunities**: Shortcuts, discoveries, trades

### Future Expansion Mechanics

#### Weather Effects
Could modify all paths in a segment:
- Add stamina costs
- Change time requirements
- Hide certain paths
- Add weather-specific events

#### Time-of-Day Variations
Could affect path availability:
- Night paths have different costs
- Some paths only available at certain times
- Different event pools by time

#### Route Unlocking
Successfully playing certain very hard paths could:
- Permanently unlock new routes
- Open shortcuts between locations
- Reveal hidden transportation options

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

Tokens serve specific purposes through different mechanics:
- **Signature Card Unlocking**: Each token threshold unlocks unique NPC cards
- **Queue Displacement**: Burn for queue flexibility (permanent cost)
- **Exchange Gates**: Minimum tokens required for special exchanges

Tokens only gained through successful letter delivery:
- Standard delivery: +1 token with recipient (type based on letter)
- Excellent delivery: +2-3 tokens with recipient (type based on letter)
- Failed delivery: -2 tokens with sender

Each use is a different mechanic with one purpose. Higher tokens unlock more signature cards that change the conversation dynamic with that specific NPC.

### Time Pressure Cascades

Time advances through:
- **Travel**: Route-specific time costs
- **Investigation**: 10 minutes per action
- **Work**: 4-hour period advance
- **Rest**: Variable time skip
- **Natural progression**: During lengthy activities

Deadlines create cascading decisions:
- Tight deadline → Need displacement → Burn tokens → Lose signature cards in future conversations
- Or: Rush to complete → Skip relationship building → Miss better letters

### How Loops Create Problems for Each Other

**Conversations create Queue pressure**:
- Every letter accepted adds obligation with fixed terms
- Multiple letters compete for position 1
- Focus management affects ability to reach request cards
- Low card levels make request success uncertain

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
- Built relationships unlock signature cards for easier future conversations
- Atmosphere effects can overcome obstacles

**Queue management solves Conversation problems**:
- Completing letters builds tokens for signature cards
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
  - Deck thinning (cost TBD)
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

**Primary Effect**: Unlock NPC signature cards at thresholds (1, 3, 6, 10, 15 tokens)

**Additional Uses Through Different Mechanics**:
1. **Signature Cards**: Each threshold unlocks unique cards for that NPC
2. **Displacement Cost**: Burn tokens to jump queue positions
3. **Exchange Gating**: Minimum tokens required for special exchanges

**Generation**:
- Standard delivery: +1 token with recipient
- Excellent delivery: +2-3 tokens with recipient
- Failed delivery: -2 tokens with sender
- Special events and quests

**Token Investment Return**:
- 1 token = Basic signature card unlocked
- 3 tokens = Two signature cards available
- 6 tokens = Three signature cards in conversation
- 10 tokens = Four powerful cards
- 15 tokens = All five signature cards active

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
- **Strategic Role**: Core resource management within conversations. Enables multi-turn planning knowing failure forces LISTEN.

#### Rapport
- **Range**: -50 to +50
- **Starting Value**: 0 (no longer modified by tokens)
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
- **Strategic Role**: Time limit for each conversation. Forces efficient play since failure forces LISTEN which costs patience.

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

The conversation system represents the primary gameplay loop. Players build and improve their conversation deck over time, representing their growing social repertoire. This deck is used in all conversations but each NPC modifies the experience through their personality rules and unlocked signature cards.

**Failure Forces LISTEN**: When a SPEAK action fails, the player must LISTEN on their next turn. This creates a natural conversation rhythm where failure forces topic changes, making success streaks feel like finding conversational flow.

### Player Deck Architecture

The player owns a single conversation deck used in all conversations:

**Starting Deck**: 20 basic cards representing fundamental social skills
**Deck Growth**: New cards gained through request completion
**Card Progression**: Each successful play grants XP to that specific card
**Deck Refinement**: Certain locations allow deck thinning for a cost

### NPC Architecture

Each NPC maintains four persistent decks plus a request system:

**Four Persistent Decks**:
1. **Signature Deck**: Unique cards unlocked by token thresholds
   - Not drawn at start - shuffled into player deck
   - Represents the specific relationship with this NPC
   - 5 cards total, unlocked at 1, 3, 6, 10, 15 tokens
   
2. **Observation Deck**: Cards from location discoveries
   - Receives cards from location observations
   - Cards automatically mixed into draw pile at conversation start
   - Provide unique advantages (state changes, exchange unlocks)
   - Consumed when played
   
3. **Burden Deck**: Cards from failed obligations
   - Contains burden cards from relationship damage
   - Enables "Make Amends" conversation type
   - Each burden card makes resolution harder
   - Mechanics TBD
   
4. **Exchange Deck**: Commerce options (mercantile NPCs only)
   - NOT conversation cards - separate card system
   - Accessed through Exchange action (1 attention)
   - Has its own UI, not part of conversation flow
   - Cards show trades: resource A → resource B

**Request System**:
NPCs don't have a "Request Deck" but rather a list of Requests. Each Request is a higher-level bundle containing:
- **Request Cards**: Multiple cards representing different goal thresholds (basic, enhanced, premium)
- **Promise Cards**: Associated cards that can force queue position and burn tokens
- **Status Tracking**: Whether the request is available, completed, or failed
- **Narrative Context**: The story reason for this request

When a player chooses a request conversation type, ALL cards from that Request bundle (both request cards and promise cards) are added to the conversation draw pile.

### Three-Pile System

#### Draw Pile
- Created at conversation start from player deck + NPC signature cards + observation cards
- Shuffled once at start
- When empty, shuffle exhaust pile to reform

#### Active Pile (Hand)
- Cards currently available to play
- No maximum hand size
- Most cards lost on LISTEN (only ~20% are Persistent)

#### Exhaust Pile
- Played cards go here
- Cards removed by Impulse/Opening go here
- Shuffled to create new draw pile when needed

### Starting a Conversation

1. **Pay attention cost** (2 for standard conversation)
2. **Choose conversation type** (based on available NPC decks and requests)
3. **Build draw pile**:
   - All player deck cards (20+)
   - Unlocked NPC signature cards (based on tokens)
   - All observation deck cards (if any)
   - If request conversation: ALL cards from selected Request bundle
4. **Shuffle draw pile**
5. **Starting rapport** = 0 (no token modifier)
6. **Draw initial hand** = cards based on connection state
   - Disconnected: 3 cards
   - Guarded: 3 cards
   - Neutral: 4 cards
   - Receptive: 4 cards
   - Trusting: 5 cards
7. **Set focus** to connection state maximum
8. **Apply personality rule** for this NPC type
9. **Request cards start unplayable** (if present)

### LISTEN Action

Complete sequence:
1. **Check patience cost**
   - Normal: Costs 1 patience
   - Patient atmosphere: Costs 0 patience
2. **Discard non-persistent cards** (~80% of hand lost)
3. **Draw new cards** equal to connection state
   - Disconnected/Guarded: 3 cards
   - Neutral/Receptive: 4 cards
   - Trusting: 5 cards
   - If draw pile has fewer: Draw what's available
   - If draw pile empty: Shuffle exhaust → new draw pile → continue
4. **Refresh focus** to connection state maximum
5. **Apply atmosphere modifiers**:
   - Receptive: Draw 1 additional card
   - Pressured: Draw 1 fewer card
   - Prepared: Add +1 to current focus
6. **Check request card activation**:
   - Check ALL goal cards in hand for rapport thresholds
   - Each card that meets its threshold becomes playable
   - All activated cards gain Impulse AND Opening properties
   - Player must choose ONE activated goal immediately
   - Unchosen goals are lost
7. **Reset turn-based personality effects** (like Proud's ascending focus requirement)

### SPEAK Action

Complete sequence:
1. **Check focus available** (must have enough for card cost)
2. **Choose one card** from hand
3. **Check personality restrictions** (like Proud's ascending order)
4. **Spend focus** equal to card cost from pool
5. **Calculate success chance**:
   - Base difficulty % + (2 × current rapport)
   - Apply atmosphere modifiers if any
   - Apply personality modifiers (like Mercantile's +30% to highest focus)
6. **Resolve success/failure**:
   - Success: +1 flow, apply card effects
   - Failure: -1 flow, apply failure effects (if any), **MUST LISTEN NEXT**
7. **Card goes to exhaust pile**
8. **Apply card effects**:
   - Rapport changes
   - Atmosphere changes
   - Draw/focus effects
   - Card gains 1 XP if successful
9. **Apply personality effects** (like Devoted's double rapport loss)
10. **Check flow transitions**:
    - At ±3: State change, flow resets to 0
11. **If Success**: Can SPEAK again if focus remains
12. **If Failure**: Next action must be LISTEN

### Promise Cards - Queue Manipulation

Promise cards are special conversation cards bundled with requests that manipulate the obligation queue mid-conversation:

**Mechanics**:
- Play like any other conversation card (costs focus, has difficulty)
- **Success Effect**: Target obligation immediately moves to position 1
- **Queue Impact**: All displaced obligations shift down
- **Token Burning**: Automatically burn tokens with ALL displaced NPCs
- **Rapport Bonus**: Gain +5 to +10 rapport immediately
- **Narrative**: "I'll prioritize your request" - literally and mechanically

**Strategic Use**:
- Enables reaching higher goal thresholds otherwise impossible
- Trades future relationships for immediate success
- Creates visible sacrifice that justifies higher rewards
- Transforms conversations from probability to economics

### Multi-Threshold Goal System

Each Request bundles multiple goal cards with different rapport requirements:

#### Goal Ladder Example
- **Basic Goal** (5 rapport): Standard delivery, 1 token reward
- **Enhanced Goal** (10 rapport): Priority delivery, 2 tokens + observation
- **Premium Goal** (15 rapport): Immediate action, 3 tokens + permit

#### Strategic Decision Tree
1. **Safe Exit**: Accept basic goal once achievable
2. **Natural Building**: Risk patience for higher thresholds  
3. **Promise Sacrifice**: Burn relationships to guarantee premium

#### Cascading Consequences
- Accept basic = preserve queue relationships
- Build naturally = risk patience exhaustion
- Use promise = damage multiple relationships for one success

The tension: You're not asking "can I succeed?" but "how much will I sacrifice?"

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

#### Persistent (~20% of deck)
- Remain in hand through LISTEN
- Valuable anchors for strategy
- Often gained as card level-up reward
- Examples: Key rapport builders, setup cards

#### Non-Persistent (~80% of deck)
- Lost when LISTEN occurs
- Creates urgency to play while available
- Most common card type
- Forces adaptation to new hands

### Card Difficulty Tiers

All modified by: +2% per rapport point

- **Very Easy** (85% base): Observation cards exclusively (TBD final %)
- **Easy** (70% base): Basic cards, safe plays (TBD final %)
- **Medium** (60% base): Standard cards, balanced risk (TBD final %)
- **Hard** (50% base): Powerful effects, scaled rapport (TBD final %)
- **Very Hard** (40% base): Request cards, dramatic effects (TBD final %)

### Deck Cycling Example

**Turn 1**: Draw pile has 28 cards (20 player + 3 signature + 5 observation), hand has 4 cards
- SPEAK a card → success → goes to exhaust pile

**Turn 2**: Draw pile has 24 cards, hand has 3, exhaust has 1
- SPEAK a card → **FAILURE** → must LISTEN next

**Turn 3**: Forced LISTEN
- Discard 2 non-persistent cards from hand
- Keep 1 persistent card
- Draw 4 new cards from draw pile
- Focus refreshes to maximum

**Turn 20**: Draw pile empty, hand has 2 persistent cards, exhaust has 26 cards
- LISTEN → need to draw 4 cards
- Shuffle exhaust pile (26 cards) → becomes new draw pile
- Draw 4 cards from new draw pile → hand

This creates natural deck cycling where all cards remain available throughout the conversation, but timing of when cards appear becomes critical.

## Player Deck Design and Progression

### Starting Deck Composition

The player begins with 20 basic cards representing fundamental social skills:

**Basic Rapport Cards** (8 cards):
- "I hear you" (x3) - 1 focus, Easy, +1 rapport
- "Let me help" (x2) - 2 focus, Medium, +2 rapport
- "Trust me" (x2) - 3 focus, Medium, +3 rapport
- "I understand" (x1) - 4 focus, Hard, +4 rapport

**Setup Cards** (4 cards):
- "Let me think" (x1) - 1 focus, Easy, sets Patient atmosphere
- "Let me prepare" (x1) - 1 focus, Easy, sets Prepared atmosphere
- "Focus on this" (x1) - 2 focus, Medium, sets Focused atmosphere
- "Stay calm" (x1) - 2 focus, Medium, sets Receptive atmosphere

**Utility Cards** (4 cards):
- "Tell me more" (x2) - 2 focus, Medium, draw 2 cards
- "Gather thoughts" (x1) - 1 focus, Easy, add 1 focus
- "Deep breath" (x1) - 3 focus, Medium, add 2 focus

**Risk/Reward Cards** (4 cards):
- "Bold claim" (x2) - 3 focus, Hard, +5 rapport / -2 on failure
- "Personal story" (x1) - 4 focus, Hard, +6 rapport / -3 on failure
- "Everything will be alright" (x1) - 5 focus, Very Hard, +8 rapport / -4 on failure

All starting cards are non-persistent except "Let me think" and "I hear you" (first copy).

### Card Leveling System

Each card tracks its own XP gained from successful plays:

**XP Thresholds and Rewards**:
- **Level 1** (0 XP): Base card
- **Level 2** (3 XP): +10% success rate
- **Level 3** (7 XP): Choose upgrade:
  - Gain Persistent keyword
  - Gain "Draw 1" on success
  - Gain +1 rapport (if rapport card)
- **Level 4** (15 XP): +10% success rate
- **Level 5** (30 XP): Becomes "Mastered"
  - Does not force LISTEN on failure
  - Card glows gold in hand

### Gaining New Cards

**Request Completion**: Each successful request grants 1-3 new cards based on goal tier
- Basic goal: 1 common card
- Enhanced goal: 1 uncommon card + 1 common card
- Premium goal: 1 rare card + 1 uncommon + 1 common

**Card Rarity Examples**:
- **Common**: Basic rapport builders, simple utilities
- **Uncommon**: Atmosphere setters, scaled effects
- **Rare**: Powerful uniques, special mechanics

**Location Discoveries**: Some observations grant player cards instead of going to NPC decks

**Special Events**: Story moments may grant unique cards

### Deck Thinning

Certain locations offer deck thinning services:
- **Cost**: 5-10 coins per card removed
- **Requirement**: Minimum 25 cards in deck
- **Limit**: Cannot go below 20 cards
- **Strategy**: Remove low-level commons to increase rare card density

### NPC Signature Cards

Each NPC has 5 unique cards that shuffle into the player's deck based on tokens:

#### Example: Marcus's Commerce Cards
- **1 token**: "Marcus's Rapport" - 2 focus, +2 rapport, Persistent
- **3 tokens**: "Trade Knowledge" - 3 focus, +3 rapport, draw 1
- **6 tokens**: "Commercial Bond" - 1 focus, +X rapport where X = Commerce tokens (max 6)
- **10 tokens**: "Marcus's Favor" - 4 focus, cannot fail if rapport ≥ 10
- **15 tokens**: "Master Trader" - 5 focus, +10 rapport, next card succeeds

#### Example: Elena's Trust Cards
- **1 token**: "Elena's Trust" - 1 focus, +1 rapport, Persistent
- **3 tokens**: "Shared Burden" - 2 focus, double next rapport gain
- **6 tokens**: "Deep Connection" - 3 focus, +5 rapport, ignore failures this turn
- **10 tokens**: "Elena's Hope" - 0 focus, advance connection state
- **15 tokens**: "Unbreakable Bond" - 4 focus, all cards Persistent this conversation

These cards represent the mechanical expression of each relationship, making every NPC conversation unique even with the same player deck.

### Strategic Deck Building

**Personality Optimization**: Build toward specific NPC types
- **Proud Focus**: Many different focus values for ascending order
- **Devoted Safety**: High success rate cards to avoid double penalties
- **Mercantile Power**: High-focus cards to maximize +30% bonus
- **Cunning Variety**: Diverse focus values to avoid repetition penalty
- **Steadfast Consistency**: Many small rapport gains within ±2 cap

**Leveling Priority**: Focus XP on versatile cards
- Cards useful against multiple personalities
- Atmosphere setters for setup
- Card draw for consistency
- Focus-adding cards for flexibility

**Token Strategy**: Which relationships to prioritize
- Commerce tokens for Marcus enable transport
- Trust tokens for emotional NPCs unlock powerful state changes
- Status tokens for proud NPCs provide authority
- Shadow tokens for cunning NPCs reveal secrets

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

Note: When tokens are burned, the corresponding signature cards remain available at their thresholds. You're burning relationship capital, not losing the fundamental connection.

### Promise Cards - Mid-Conversation Queue Manipulation

Promise cards create a unique interaction between conversation and queue systems:

**Immediate Queue Effect**:
- Played during conversation like any other card
- Target obligation immediately moves to position 1
- All displaced obligations shift down
- Tokens burned with displaced NPCs instantly

**Strategic Implications**:
- Enables reaching higher conversation thresholds
- Damages multiple relationships for one benefit
- Can solve queue problems while in conversation
- Creates visible sacrifice (narrative emergence)

**Cost-Benefit Analysis**:
- Benefit: +5-10 rapport, reach premium goals
- Cost: Burn tokens equal to positions displaced
- Risk: Damaged relationships harder to repair
- Opportunity: Higher tier rewards often worth sacrifice

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
- Need high-rapport goal card → Must build significant rapport
- Building rapport needs successful cards → Requires good flow
- Good flow needs consistent success → Must manage risk
- Failure forces LISTEN → Lose most cards, costs patience
- Limited patience → Must be efficient with plays

#### Promise Card Calculus
- Want higher reward tier → Need more rapport than achievable
- Promise card offers +10 rapport → Guarantees threshold
- But moves obligation to position 1 → Displaces current queue
- Burns tokens with displaced NPCs → Reduces future signature cards
- Creates narrative emergence → Sacrifice visible to all

#### Queue Displacement Cascade
- Need urgent delivery → Must displace
- Displacement burns tokens → Lose relationship progress
- Fewer tokens → Fewer signature cards in conversations
- Fewer signature cards → Harder conversations
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
- Chain low-focus cards before failure forces LISTEN
- Use atmosphere to expand capacity
- Recognize when to play safe vs risk failure
- Plan around personality restrictions

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

Success builds tokens for signature cards:
- First delivery: +1 token = 1 signature card available
- Chain effect: More tokens → more signature cards → easier conversations

Failure costs:
- -2 tokens with sender (lose signature cards)
- +2 burden cards
- Relationship damage compounds

### Token Investment Mathematics

Signature cards unlocked by tokens:
- **1 token**: Basic signature card available
- **3 tokens**: 2 signature cards in deck
- **6 tokens**: 3 powerful cards mixed in
- **10 tokens**: 4 signature cards active
- **15 tokens**: All 5 signature cards
- Each signature card fundamentally changes conversation dynamics

Burning tokens for displacement:
- Displacing 1 position: 1 token
- Displacing 3 positions: 6 tokens total
- Displacing 5 positions: 15 tokens total
- Burning tokens doesn't remove unlocked cards but damages relationship

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
- Failure forces LISTEN, lose non-persistent cards
- Must rebuild hand from new draws

**Neutral** (5 capacity):
- Can play: Five 1-focus OR one 5-focus
- Can reach request cards
- Sweet spot for most conversations

**Trusting** (6 capacity):
- Maximum flexibility
- 5 cards drawn on LISTEN
- Best recovery from forced topic changes

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

### Cards → Success → Stronger Deck
```
Successful card plays → XP for that card
Card levels up → Better success rate / gains keywords
Request completion → New cards for deck
Deck refinement → Remove weak cards
Stronger deck → Handle harder NPCs
Harder NPCs → Better rewards → More powerful cards
```

### Tokens → Signature Cards → Easier Conversations
```
Successful deliveries → +1-3 tokens
Higher tokens → Unlock signature cards
Signature cards → Mixed into player deck
More signature cards → Better conversation options
Better options → Higher success rates
More successes → More tokens
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
More capacity → Can play more cards before LISTEN
Successful plays → Positive rapport momentum
Failure forces LISTEN → Topic change, lose cards
Must rebuild → Draw new hand
Better states → More cards drawn on LISTEN
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

### Core Concept

Exchanges are a **separate card system** from conversations. NPCs with exchange decks offer trades through a dedicated exchange UI, not during conversations.

### Exchange Card Mechanics

**Exchange Cards ARE Cards**:
- Each exchange is a card in the NPC's exchange deck
- Cards show simple trades: Input → Output
- Player sees all available exchange cards at once
- Pick one card to execute the trade
- No focus, rapport, or conversation mechanics involved

**Exchange Card Properties**:
```
Exchange Card Structure:
- Name: "Buy Simple Meal"
- Cost: Resources required (coins, items, tokens)
- Effect: What you receive
- Requirements: Minimum tokens or observations needed
- Availability: Time restrictions or one-time limits
```

### Quick Exchange Action

- **Cost**: 1 attention
- **Time**: 5-10 minutes
- **Process**:
  1. Pay 1 attention to access NPC's exchange deck
  2. See all available exchange cards in separate UI
  3. Select one exchange card (if you can afford it)
  4. Pay cost, receive effect
  5. Exchange completes immediately
- **No conversation mechanics** (no focus, rapport, flow, patience)
- **Pure resource trade**

### Common Exchange Types

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
- Deck thinning: 5-10 coins → remove one card

### Token-Gated Exchanges

Some exchange cards require minimum tokens to appear:
- **Transport**: 2+ Commerce tokens → caravan becomes available
- **Secret information**: 3+ Shadow tokens → special intel unlocked
- **Noble introduction**: 5+ Status tokens → social access granted
- **Temple blessing**: 4+ Trust tokens → spiritual services offered

The tokens don't get spent - they just gate access to the exchange card.

### Observation-Unlocked Exchanges

Some exchanges only appear after playing specific observation cards:
- Play "Trade Route Knowledge" → Unlocks special transport exchange
- Play "Black Market Contact" → Unlocks illegal goods exchanges
- These permanently add new cards to the NPC's exchange deck

### Exchange vs Conversation

**Exchanges ARE**:
- Separate card system with own UI
- Simple resource trades
- Quick (1 attention, minimal time)
- No social mechanics

**Exchanges ARE NOT**:
- Part of conversations
- Subject to focus/rapport/flow
- Affected by connection states
- Using patience or atmosphere

### Strategic Role

Exchanges provide:
- Resource conversion without social challenge
- Predictable outcomes (no success/failure)
- Quick transactions when time matters
- Token-gated progression rewards
- Observation-unlocked special options

## No Soft-Lock Architecture

Every system has escape valves preventing unwinnable states:

### Conversation Deadlocks - Never Stuck

**Problem**: All cards too expensive for current focus
**Solution**: Starting deck has multiple 1-focus cards

**Problem**: Request card requires 5 focus, stuck in Disconnected
**Solution**: Can leave and return with better preparation or leveled cards

**Problem**: No rapport, everything failing
**Solution**: 70% base success on easy cards still likely

**Problem**: Personality rule makes cards unplayable
**Solution**: LISTEN changes available cards, resets some restrictions

**Problem**: Failed and must LISTEN, no patience left
**Solution**: Can leave conversation and try different approach

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

**Problem**: No signature cards with NPC
**Solution**: Base deck still functional, can earn tokens

**Problem**: Health critically low
**Solution**: Rest options available (TBD)

**Problem**: Deck too weak for NPC
**Solution**: Can gain cards elsewhere, level existing cards

## Content Scalability

### Adding NPCs - Simple Framework

New NPCs simply need:
- **Personality type**: Determines patience, token preference, and conversation rule
- **Four persistent decks**:
  - Signature deck (5 unique cards at token thresholds)
  - Observation deck (receives location discoveries)
  - Burden deck (damaged relationship tracking)
  - Exchange deck (if mercantile)
- **Request list**: Bundles of request and promise cards
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

New player cards must follow:
- **Single effect**: One clear purpose (not multiple)
- **Focus range**: 0-6 focus cost
- **Difficulty tier**: Very Easy to Very Hard
- **Persistence**: ~20% should be Persistent
- **Level progression**: Define XP thresholds and rewards
- **Rarity**: Common, Uncommon, or Rare

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
- Use observations and signature cards gained

**Evening (2-6 PM)**:
- Locations begin closing
- Rush to complete deadlines
- Make difficult displacement decisions
- Burn tokens if necessary
- See card XP accumulate from successful plays

**Night (6-10 PM)**:
- Limited location access
- Focus on available NPCs
- Rest and recovery options
- Plan next day's route
- Consider deck thinning if coins available

### Emergent Narratives

Stories emerge from mechanical interaction, not scripting:

**Building Power Through One NPC**:
- Focus all deliveries to Marcus
- Gain 10 Commerce tokens
- Unlock 4 signature cards
- Conversations with Marcus become trivial
- His Mercantile rule (+30% to highest focus) synergizes with his high-focus signature cards
- Can reliably reach Premium goals
- But neglected other relationships

**Card Mastery Story**:
- "Bold Claim" card used successfully 30 times
- Reaches level 5 - Mastered
- No longer forces LISTEN on failure
- Becomes anchor card for risky strategies
- Enables conversation momentum others can't maintain
- Player known for bold, aggressive conversation style

**The Cascading Failure**:
- Failed conversation with Elena (no signature cards)
- Forced to accept Basic goal for urgent delivery
- Low payment means can't afford food
- Work at high hunger for poor output
- Can't afford permit for efficient route
- Take longer path, miss deadline
- Lose tokens with recipient
- Future conversations even harder

### Strategic Mastery Progression

**Beginner**: 
- Uses starting deck without plan
- Doesn't understand personality rules
- Ignores investigation timing
- Takes any available letter

**Intermediate**:
- Recognizes which cards work against which personalities
- Plans investigation for quiet periods
- Manages hunger for work efficiency
- Focuses token building on key NPCs

**Advanced**:
- Shapes deck for specific challenges
- Times card leveling for key conversations
- Pre-builds observation advantages
- Maintains token portfolios across multiple NPCs
- Uses promise cards strategically

**Expert**:
- Has mastered cards for momentum control
- Knows exact token thresholds needed
- Routes perfectly for time efficiency
- Manipulates queue for maximum profit
- Deck refined to personal playstyle

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
    "attention": 10,
    "playerDeck": ["hear_you_1", "hear_you_2", ...] 
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
Signature deck: 5 generic cards
Request list: Empty
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

- `Content/Core/`: Essential game content including starting deck
- `Content/Expansions/`: Additional content packs
- `Content/Generated/`: AI-generated packages
- `Content/TestPackages/`: Testing content

## Core Innovation Summary

The core gameplay loop is **conversations as character progression**:

### The Player Deck as Character

Your conversation deck IS your character. Every card represents a social skill you've learned. Every level up makes that specific approach more reliable. Every new card gained expands your repertoire. This isn't abstract character growth - it's tangible, visible, and strategic.

### Conversations as Combat

Each conversation is a tactical puzzle where you play your deck against NPC personality rules:
- **Proud NPCs** force ascending focus order - like enemies that punish repetition
- **Devoted NPCs** double rapport losses - like glass cannon fights
- **Mercantile NPCs** reward big plays - like bosses with weak points
- **Cunning NPCs** punish patterns - like adaptive AI opponents
- **Steadfast NPCs** cap rapport changes - like damage reduction enemies

### Failure Creates Rhythm

When you fail a card play, you must LISTEN, losing most cards and drawing new ones. This isn't punishment - it's conversation rhythm. Success maintains momentum with your current topics. Failure forces topic changes. The mechanical flow mirrors actual dialogue.

### Relationships as Equipment

NPC signature cards unlocked by tokens are like equipment in other RPGs. Marcus's commerce cards are your "merchant armor." Elena's trust cards are your "emotional weapons." Each relationship provides unique tools that fundamentally change how conversations play.

### Clear Progression Path

The loop is crystal clear:
1. **Have conversations** using your deck
2. **Complete requests** to gain new cards and XP
3. **Level up cards** to improve success rates
4. **Unlock signature cards** through token relationships
5. **Build stronger deck** to tackle harder NPCs
6. **Face tougher personalities** with complex rules

This is "Fight enemies → Gain XP → Level up → Fight stronger enemies" expressed through social dynamics and deck building rather than combat statistics.

## Design Verification Checklist

### Clean Mechanical Separation ✓
- Player owns conversation deck (social skills)
- NPCs provide signature cards (relationship expression)
- Card XP tracks individual mastery
- Personality rules modify play space
- Each mechanic has exactly ONE purpose

### Perfect Information ✓
- All calculations visible to player
- Success rates shown before playing cards
- Personality rules displayed at conversation start
- Focus costs displayed on all cards
- Token thresholds for signature cards shown

### Linear Scaling ✓
- Each rapport point: exactly +2% success
- Each token: unlocks cards at fixed thresholds
- Each 25 hunger: exactly -1 attention
- Each patience: exactly 1 turn
- Each card level: specific defined benefit
- Only exception: Flow ±3 triggers state change

### No Soft-Locks ✓
- Starting deck has multiple 1-focus cards
- Always have minimum 2 attention
- Can leave conversations at any time
- Can complete requests for new cards
- Multiple sources for same permits
- Deck thinning available to refine strategy

### Resource Flow ✓
- Cards flow from requests to deck to mastery
- Tokens flow from deliveries to signature cards
- XP flows from successful plays to card levels
- Familiarity flows from investigation to observations
- Time flows through all systems creating pressure

### Emergent Complexity ✓
- Simple personality rules create different puzzles
- Card combinations enable various strategies
- Token relationships shape available tools
- Failure forcing LISTEN creates natural rhythm
- Player deck reflects personal playstyle
- Every conversation is practice toward mastery

## Critical Formulas Reference

**Success Rate**: Base% + (2 × Current Rapport)

**Card Level Thresholds**: 
- Level 2: 3 XP
- Level 3: 7 XP
- Level 4: 15 XP
- Level 5: 30 XP

**Signature Card Thresholds**:
- 1 token: First card
- 3 tokens: Second card
- 6 tokens: Third card
- 10 tokens: Fourth card
- 15 tokens: Fifth card

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

**Card Draws on LISTEN**:
- Disconnected: 3
- Guarded: 3
- Neutral: 4
- Receptive: 4
- Trusting: 5

**Patience Base**:
- Devoted: 15
- Steadfast: 13
- Mercantile: 12
- Cunning: 12
- Proud: 10

**Card Persistence**: ~20% of deck should be Persistent

## Implementation Priority

### Phase 1: Core Conversation System with Player Deck ✓
- Player owns conversation deck
- Card XP and leveling system
- NPC signature cards from tokens
- Personality rules
- Failure forces LISTEN mechanic
- Most cards non-persistent

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
- Token system unlocks signature cards
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

Wayfarer achieves its design goals through making conversations the core gameplay loop. The player's conversation deck represents their character growth in the most literal sense - every card is a social skill they've learned and mastered through practice.

The genius is that this uses familiar deck-building mechanics to represent character progression, while NPC personality rules and signature cards ensure every conversation feels unique despite using the same player deck. The failure-forces-LISTEN mechanic creates natural conversation rhythm where success builds momentum and failure forces adaptation.

Players always know what to do: have conversations to gain cards and XP, level up their deck, unlock signature cards through relationships, and tackle increasingly complex NPC personalities. The strategic depth emerges from how player deck composition, card levels, signature cards, and personality rules interact to create unique puzzles.

The system succeeds because mastery comes from understanding these interactions and building a deck that reflects your personal approach to social challenges. Every conversation is practice. Every card gained is permanent progression. Every relationship built provides new tools. This is your character growth made tangible, strategic, and elegantly integrated with the narrative of becoming a better conversationalist.