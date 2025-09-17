# Wayfarer: Complete Game Mechanics

## Table of Contents
1. [Core Design Philosophy](#core-design-philosophy)
2. [Three Core Game Loops](#three-core-game-loops)
3. [Travel System](#travel-system)
4. [Weight System](#weight-system)
5. [Resource Flow Between Loops](#resource-flow-between-loops)
6. [Resource Economy](#resource-economy)
7. [Time Segment System](#time-segment-system)
8. [Conversation System](#conversation-system)
9. [Player Deck Design and Progression](#player-deck-design-and-progression)
10. [Queue Management System](#queue-management-system)
11. [Strategic Resource Management](#strategic-resource-management)
12. [Economic Balance Points](#economic-balance-points)
13. [Resource Conversion Chains](#resource-conversion-chains)
14. [Work System](#work-system)
15. [Exchange System](#exchange-system)
16. [No Soft-Lock Architecture](#no-soft-lock-architecture)
17. [Content Scalability](#content-scalability)
18. [The Holistic Experience](#the-holistic-experience)
19. [Content Loading System](#content-loading-system)
20. [Core Innovation Summary](#core-innovation-summary)
21. [Design Verification](#design-verification-checklist)

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

**BAD**: "Hunger reduces patience AND work output AND travel speed"
**GOOD**: Hunger reduces work output (coins = 5 - floor(hunger/25)). Hunger increases travel segments (+1 at 75+). Two separate formulas.

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

- **Proud**: Cards must be played in ascending focus order each turn (resets when you LISTEN)
- **Devoted**: When rapport decreases, it decreases twice
- **Mercantile**: Your highest focus card each turn gains +30% success
- **Cunning**: Playing same focus as previous card costs -2 rapport
- **Steadfast**: All rapport changes are capped at ±2 per card

These rules represent how different personalities respond to conversation. A Proud person needs escalating respect. A Devoted person catastrophizes failure. A Merchant rewards getting to the point.

#### Conversation Outputs
- **Stat XP**: Each card play grants XP to its bound stat (1 XP base, multiplied by conversation difficulty)
- **New Cards**: Request completion grants new cards to player deck
- **Tokens**: Gained through successful letter delivery (+1 to +3)
- **Observations**: Cards added to specific NPCs' observation decks
- **Permits**: Special promises that enable routes
- **Burden Cards**: Failed requests damage relationships

- **Stranger Encounter System**: Unnamed NPCs at specific locations offering one-time conversations for resources and scaled XP
- **Conversation Difficulty Levels**: Level 1-3 affecting both XP gain and base success rates
- **Stat-Based Card Enhancement**: Cards gain bonuses from their bound stat level rather than individual progression

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
- Letter requests: Specific deadline, position, payment, and weight
- Meeting requests: Fixed time and location
- Resolution requests: Clear existing burden cards

Personality influences which requests are available:
- Proud NPCs offer urgent, high-position requests
- Disconnected connection state only has crisis requests
- Mercantile NPCs focus on profitable exchanges

#### Strategic Queue Patterns

**Obligation Chaining**: Accept multiple obligations in same location, complete efficiently if weight permits

**Token Preservation**: Accept fixed queue positions to avoid burning relationships

**Emergency Displacement**: Burn tokens only for critical deadlines

**Queue Blocking**: Full queue (10 obligations) or weight capacity prevents new letter acquisition

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
- Costs 1 time segment
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
- Take weight in satchel (typically weight 1)
- Do not expire (they're physical documents)
- No associated obligation
- Enable specific routes while held

#### Observation System

**Building Discoveries**:
- Observations require minimum familiarity levels
- Each observation requires all prior observations at that location
- Cost 1 time segment when available
- Create cards that go into specific NPCs' observation decks OR yield physical items
- Different observations available at different familiarity levels:
  - First observation: Requires familiarity 1+
  - Second observation: Requires familiarity 2+ AND first observation done
  - Third observation: Requires familiarity 3+ AND second observation done

**Observation Effects**:
- Cards created go to predetermined NPCs' observation decks
- Represent location knowledge meaningful to specific NPCs
- Mixed into draw pile when conversing with relevant NPC
- Can unlock exchanges, change connection states, or provide unique effects
- Some observations yield physical items instead of cards

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
- **Face-up**: Shows full requirements and effects including weight restrictions

Once a card flips face-up (through play or revelation), it remains face-up forever for all future travels on that route.

Path cards have:
- Weight limits
- Stamina costs  
- Permit requirements
- Stat requirements (minimum levels to use)

#### Stat-Gated Paths

Certain paths require minimum stat levels:

**Insight Paths**:
- "Scholar's Shortcut" - Requires Insight 2+
- "Complex Route" - Requires Insight 3+
- Navigate through understanding patterns

**Rapport Paths**:
- "Local's Favor" - Requires Rapport 2+
- "Safe House Route" - Requires Rapport 4+
- Friends help you pass

**Authority Paths**:
- "Noble's Gate" - Requires Authority 2+ OR Noble Permit
- "Checkpoint Bypass" - Requires Authority 3+
- Walk through on reputation alone

**Commerce Paths**:
- "Merchant Caravan" - Requires Commerce 2+ OR 10 coins
- "Trade Route" - Requires Commerce 3+
- Commercial reputation grants access

**Cunning Paths**:
- "Shadow Path" - Requires Cunning 3+
- "Misdirection Route" - Requires Cunning 2+
- See paths others miss

These create natural character differentiation where different builds access different routes.

#### Playing Path Cards

1. Each segment presents all path cards for that segment
2. Player sees face-down cards (name + stamina) and face-up cards (everything)
3. Player chooses one card they can afford (stamina + requirements + weight allows)
4. Card flips face-up permanently if face-down
5. Apply card effects including segment costs
6. Continue to next segment or complete travel

#### Cannot Proceed

If player cannot meet requirements for ANY path in a segment:
- Must turn back immediately
- Time segments already spent are lost
- Stamina already spent is lost
- Return to starting location

To prevent soft-locks, every location must have at least one "Struggle" path that works at any weight but costs maximum segments. This ensures forward progress while maintaining weight's strategic importance.

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
- REST action: Skip segment, refresh stamina to capacity
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
- Weight restrictions create natural limitations

#### Caravan Routes: Event Collections

When using transport, each segment draws from event pools:
- Draw one event randomly per segment
- Each event contains 2-3 thematically related cards
- Player picks one card from the drawn event
- Different events each journey
- Weight still affects available choices

The decision structure is identical in both modes: pick one card from available options. Only the source differs:
- Walking = fixed cards per segment (exploration leading to mastery)
- Caravan = random events per segment (adaptation to circumstances)

### Weight Integration with Path Cards

Path cards check total satchel weight creating physical constraints:

**Standard Weight Effects**:
- "Steep Hill": 1 stamina normally, 2 stamina if carrying over 7 weight
- "Narrow Alley": Impassable over 8 weight
- "Merchant Cart": 2 coins normally, free if carrying trade goods over 5 weight
- "Guard Checkpoint": Inspects satchel, may confiscate contraband
- "Struggle Path": Always available but costs 3 segments

### Knowledge Discovery Methods

#### 1. Direct Exploration
- Play the path card spending stamina
- Card flips face-up permanently including weight restrictions
- Most expensive but always available
- Only method for hidden paths

#### 2. Investigation
Location investigation can reveal specific path cards:
```
Location Investigation:
- Familiarity 1+: Reveals 1-2 basic paths
- Familiarity 2+: Reveals intermediate paths including weight limits
- Familiarity 3+: Reveals all non-hidden paths on one route
```

#### 3. NPC Conversations
Conversation cards can reveal paths NPCs know:
```
Card Effect Examples:
- "reveals all non-hidden paths on [route]"
- "reveals cheapest path on each segment"
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
- **Time Segments**: Add segments to travel time
- **Resources**: Gain/lose coins, hunger, health
- **Stamina**: Restore or drain stamina
- **Discoveries**: Find items or observation cards
- **Events**: Draw from event deck (walking routes only)

#### One-Time Discoveries

Certain paths have permanent world effects on first play:
- Find coins or items (marked "already looted" after)
- Discover items that add weight to satchel
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
- Resource costs (stamina for physical effort)
- Weight restrictions (cannot choose if over limit)
- Requirements (coins, items, tokens)
- Effects (time segments, resources, discoveries)
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
- **Permits**: Gate access requirements (weight 1 each)
- **Time Segments**: Accept longer paths to avoid costs
- **Weight Capacity**: Limits available paths

#### Transport Resources
- **Stamina**: Still used for physical events during transport
- **Coins**: Purchase advantages or comfort
- **Items**: Letters, permits affect specific events
- **Time Segments**: Accept delays for free options
- **Weight**: Still restricts choices even when riding

### Integration with Other Systems

#### Investigation Integration
Location familiarity levels unlock path revelations:
- Each familiarity level can reveal specific paths
- Higher familiarity reveals weight restrictions
- Familiarity 3 often reveals complete route knowledge

#### Conversation Integration
NPCs can provide route knowledge through:
- Standard conversation cards with reveal effects
- Observation cards that unlock route information
- Exchange options for complete route maps

#### Observation Integration
Travel discoveries can create items or observation cards:
- Found items add weight to satchel
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

## Weight System

### Core Concept

Weight creates constant decision pressure through physical constraints. The satchel has capacity 10, forcing brutal trade-offs between obligations, tools, food, and discovered items. Every choice about what to carry ripples through all other systems.

### Weight Mechanics

**Basic Weight Values**:
- Letters: 1 weight each
- Permits: 1 weight each
- Simple packages: 1-2 weight
- Trade goods: 3-4 weight
- Heavy deliveries: 5-6 weight
- Tools: 1-3 weight based on size
- Consumables: 1 weight each

**Capacity Rules**:
- Absolute maximum: 10 weight
- No temporary expansion
- No exceptions or flexibility
- Exceeding capacity impossible

### Item Categories

#### Obligations
Every accepted obligation adds physical weight. "Deliver Elena's letter" adds 1 weight. "Deliver Marcus's silk samples" adds 3 weight. "Transport warehouse crate" adds 5 weight. Some premium requests create heavier packages with better rewards - Elena's legal documents weigh 3 but pay triple.

#### Consumables
Strategic resources carried until needed. Bread weighs 1, removes 30 hunger when consumed. Medicine weighs 1, restores 20 health when used. Carrying these means accepting fewer obligations but having resources exactly when needed. The timing of consumption becomes critical.

#### Tools
Permanent equipment providing persistent benefits at weight cost. A crowbar weighs 2, enables "Forced Entry" path cards. A merchant ledger weighs 1, provides +1 starting Commerce rapport. A rope weighs 2, enables "Cliff Descent" paths. Tools never leave your satchel unless dropped, creating long-term capacity decisions.

#### Trade Goods
Items with no immediate use but sellable at specific locations. Silk weighs 3, sells for 10 coins at Noble Quarter. Ore weighs 4, sells for 8 coins at Warehouse. Wine weighs 2, sells for 6 coins at Tavern. Finding these creates immediate decisions about profit versus capacity.

#### Discovered Items
Investigations and path discoveries yield physical objects. "Shipping Manifests" weigh 1, unlock special merchant exchanges. "Noble Seal" weighs 1, enables guard checkpoint passage. "Ancient Map" weighs 2, reveals hidden location. These compete with obligations for limited space.

### Weight and Travel Integration

Path cards check total weight creating natural restrictions that make perfect physical sense:

**Movement Restrictions**:
- Light load (0-3 weight): All paths available
- Medium load (4-6 weight): Some paths restricted
- Heavy load (7-9 weight): Many paths blocked or penalized
- Maximum load (10 weight): Only struggle paths available

**Specific Path Examples**:
- "Steep Hill": +1 stamina cost per 3 weight over 6
- "Narrow Alley": Impassable over 8 weight

### Investigation and Weight

Investigations can yield items or observations, never both. This creates strategic choice about which locations to investigate:

**Item-Yielding Investigations**:
- Warehouse: "Shipping Manifests" (weight 1)
- Noble Garden: "Ornamental Dagger" (weight 2, sells for 10 coins)

**Observation-Yielding Investigations**:
- Market Fountain: Knowledge for NPC conversations

The distinction matters because items consume weight while observations enhance conversations without physical burden.

### Strategic Weight Management

#### Obligation Bundling
Multiple deliveries to the same location can complete in one trip if weight permits. Three letters to Noble Quarter (3 weight total) is efficient. But adding Marcus's silk (3 weight) might force dropping one letter.

#### Tool Investment
Carrying a crowbar permanently reduces capacity by 2 but enables shortcuts saving segments. The rope enables cliff paths avoiding checkpoints. Tools provide options at permanent weight cost.

#### Consumable Timing
Carrying food means being prepared for hunger but reduces delivery capacity. Eating immediately frees weight but risks hunger during critical moments. The timing of consumption affects everything.

#### Discovery Decisions
Finding valuable trade goods forces immediate choice. Drop current obligations for profit? Continue with less capacity? Return later risking the item disappears? Every discovery creates tension.

## Resource Flow Between Loops

### Time Segment Economy Connections

**Daily Allocation**: 24 segments (6 blocks × 4 segments each)

Time segments enable:
- **Conversations** (1 segment + patience depth): Access to letters and tokens
- **Investigations** (1 segment): Build location familiarity
- **Observations** (1 segment): Discover cards or items for NPCs
- **Work** (4 segments/full block): Coins but time cost, scaled by hunger
- **Quick Exchange** (0 segments): Simple commerce without conversation
- **Travel** (varies by path): Physical movement between locations

Work output scales with hunger:
- Formula: coins = 5 - floor(hunger / 25)
- Hungry workers are less productive
- Creates meaningful choice about when to eat

This forces prioritization between relationship building, location investment, and resource generation within the rigid structure of time blocks.

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
- **Travel**: Path cards cost segments directly
- **Investigation**: 1 segment per action
- **Work**: Full block advancement (4 segments)
- **Conversation**: 1 segment base + patience depth
- **Observation**: 1 segment when available

Deadlines create cascading decisions:
- Tight deadline → Need displacement → Burn tokens → Lose signature cards in future conversations
- Or: Rush to complete → Skip relationship building → Miss better letters

### How Loops Create Problems for Each Other

**Conversations create Queue pressure**:
- Every letter accepted adds obligation with fixed terms and weight
- Multiple letters compete for position 1
- Focus management affects ability to reach request cards
- Low card levels make request success uncertain
- Heavy packages limit what else you can carry

**Queue creates Travel pressure**:
- Obligations scattered across city
- Deadlines force inefficient routing
- Displacement damages relationships at distance
- Time-fixed meetings cannot be displaced
- Weight limits force multiple trips or dropping items

**Travel creates Conversation pressure**:
- Access permits require successful request card plays
- Travel time reduces deadline margins
- Encounters can damage resources
- Building familiarity costs segments that could fund conversations
- Weight restrictions change available routes

### How Loops Solve Each Other's Problems

**Conversations solve Travel problems**:
- Request cards provide access permits
- Successful deliveries reward observation cards or items
- Built relationships unlock signature cards for easier future conversations
- Atmosphere effects can overcome obstacles

**Queue management solves Conversation problems**:
- Completing letters builds tokens for signature cards
- Meeting deadlines maintains sender relationships
- Efficient routing preserves resources for conversations
- Managing weight enables accepting profitable requests

**Travel solves Queue problems**:
- Familiarity reveals efficient routes and weight limits
- Observations unlock better exchanges or yield valuable items
- Permits enable shortcuts
- Investigation timing affects resource efficiency
- Path mastery enables weight optimization

### Connection Tokens

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
- **Strategic Role**: Core resource management within conversations. Enables multi-turn planning knowing failure typically forces LISTEN.

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
- **Patience as Time Depth**: Each patience spent represents 5 minutes of conversation depth beyond the base segment
- **Strategic Role**: Time limit for each conversation. Forces efficient play since failure typically forces LISTEN which costs patience.

## Time Segment System

### Core Structure

Time replaces abstract attention with concrete segments representing actual duration. Each day contains 24 segments organized into meaningful blocks that create natural activity rhythms and strategic chunking.

#### Time Organization
- **Days** → **Time Blocks** → **Time Segments**
- **Time Blocks** (6 per day, 4 segments each):
  - Dawn (2-6 AM): 4 segments
  - Morning (6-10 AM): 4 segments
  - Midday (10 AM - 2 PM): 4 segments
  - Afternoon (2-6 PM): 4 segments
  - Evening (6-10 PM): 4 segments
  - Night (10 PM - 2 AM): 4 segments
- **Total**: 24 segments per day

#### Segment Costs

**Zero Segment Actions** preserve immediate responses:
- Accepting/rejecting requests during conversations
- Playing cards in conversations
- Dropping items from satchel
- Moving between spots at same location
- Quick exchanges with merchants
- Choosing path cards during travel

**One Segment Actions** represent meaningful time:
- Standard conversations (base cost + patience depth)
- Investigation at any spot
- Observation when available
- Each segment of travel based on path cards

**Full Block Actions**:
- Work consumes all 4 segments, advancing to next block

#### Block Advancement

When segments in a block are exhausted, time advances to the next block. Starting an action without sufficient segments forces advancement. Work always jumps to the next block regardless of current segment position.

### Conversation Time Integration

Conversations cost 1 base segment plus patience becomes time depth. Each patience spent adds 5 minutes of conversation complexity. This maintains patience importance while simplifying tracking.

**Connection State Time Modifiers**:
- Disconnected: Rushed conversation, patience counts double
- Guarded: Normal patience cost
- Neutral: Normal patience cost
- Receptive: Comfortable flow, -20% patience cost
- Trusting: Natural rhythm, patience counts half

This creates mechanical differentiation where better relationships enable longer conversations for the same time investment.

### Investigation Mechanics

Investigation has multiple approaches unlocked by player stats:

#### Investigation Approaches

**Standard Investigation** (Always available):
- Cost: 1 segment
- Effect: Normal familiarity gain
- Morning quiet bonus: +1 familiarity

**Systematic Observation** (Insight 2+):
- Cost: 1 segment
- Effect: +1 additional familiarity
- Reveals hidden connections

**Local Inquiry** (Rapport 2+):
- Cost: 1 segment
- Effect: Normal familiarity + learn which NPCs want observations from this location
- Builds social map of location

**Demand Access** (Authority 2+):
- Cost: 1 segment
- Effect: Can investigate restricted spots without permits
- Forces entry through commanding presence

**Purchase Information** (Commerce 2+):
- Cost: 2 coins per familiarity level
- Effect: Instant familiarity gain without time cost
- Buying local knowledge directly

**Covert Search** (Cunning 2+):
- Cost: 1 segment
- Effect: Investigation doesn't trigger NPC state changes or alerts
- Undetected information gathering

### Travel Segment Integration

Path cards specify segment costs directly:
- "Quick Route": 0 segments but strict weight limits
- "Main Road": 1 segment reliably
- "Scenic Route": 2 segments but restores stamina
- "Struggle Path": 3 segments but always available

Multi-segment journeys may force block advancement. Starting a 2-segment journey with 1 segment remaining advances time to the next block upon completion.

### Work Time Commitment

Work consumes an entire time block (4 segments), representing half-day labor commitments. You cannot work and have conversations in the same morning block. This forces activity chunking matching natural daily rhythms.

**Productivity Timing**:
- Morning work: Often optimal (low hunger after breakfast)
- Afternoon work: Moderate efficiency
- Evening work: Poor efficiency (high hunger accumulation)

### Deadline Pressure

Deadlines specified in blocks and segments create precise time management:
- "Deliver by Evening" means before Evening block ends
- "Urgent: 2 blocks" means 8 segments maximum
- "Before close of business" means before shops close in Evening

Missing deadlines has permanent consequences:
- -2 tokens with sender
- +2 burden cards to relationship
- No payment received
- Obligation removed from queue

## Conversation System

### Core Design Principle

The conversation system represents the primary gameplay loop. Players build and improve their conversation deck over time, representing their growing social repertoire. This deck is used in all conversations but each NPC modifies the experience through their personality rules and unlocked signature cards.

**Failure Effects Apply**: When a SPEAK action fails, the card's failure effect is applied. Cards without specific failure effects automatically apply ForceListen as a fallback, maintaining natural conversation rhythm where failure typically forces topic changes.

### Card Categorical Property System

Each card is defined by exactly five categorical properties that determine its behavior:

#### Persistence (when the card can be played)
- **Thought**: Remains in hand until played (survives LISTEN)
- **Impulse**: Removed after any SPEAK if unplayed
- **Opening**: Removed after LISTEN if unplayed

#### Success (effect type on success)
- **Rapport**: Changes connection strength determined by magnitude
- **Threading**: Draw cards to hand determined by magnitude
- **Atmospheric**: Sets specific atmosphere determined by magnitude
- **Focusing**: Restore focus to current pool determined by magnitude
- **Advancing**: Advance flow battery by x steps determined by magnitude
- **Promising**: Move obligation to position x determined by magnitude and gain rapport
- **None**: No effect on success

#### Failure (effect type on failure)
- **Backfire**: Negative rapport change
- **ForceListen**: Player must LISTEN on next turn after failure
- **None**: No additional effect (automatically applies ForceListen as fallback)

#### Exhaust (effect when discarded unplayed)
- **Threading**: Draw cards when discarded
- **Focusing**: Restore focus when discarded
- **Regret**: Negative rapport when discarded
- **None**: No effect when discarded

#### Stat Binding (which stat gains XP and provides bonuses)
- Every card bound to exactly one stat: Insight, Rapport, Authority, Commerce, or Cunning
- Determines which stat gains XP when played
- Determines which stat level provides card bonuses

#### Magnitude Determination
Effect magnitudes are determined by difficulty level, not hardcoded:
- **Very Easy**: Magnitude 1
- **Easy**: Magnitude 2
- **Medium**: Magnitude 2
- **Hard**: Magnitude 3
- **Very Hard**: Magnitude 4

### Player Stats System

Players have five core stats representing different problem-solving methodologies developed through conversation:

#### The Five Stats

**Insight** - Analytical thinking and observation
- Bound cards focus on analysis, patterns, and understanding
- Unlocks systematic investigation approaches
- Gates scholarly and complex travel paths

**Rapport** - Emotional connection and empathy
- Bound cards focus on understanding, support, and connection
- Unlocks social investigation through locals
- Gates community-based travel paths

**Authority** - Leadership and commanding presence
- Bound cards focus on direction, control, and decisiveness
- Unlocks demanding access to restricted areas
- Gates noble and official travel paths

**Commerce** - Negotiation and trade thinking
- Bound cards focus on deals, exchanges, and mutual benefit
- Unlocks purchasing information directly
- Gates merchant caravan paths

**Cunning** - Subtlety and indirect approach
- Bound cards focus on misdirection, implication, and hidden meanings
- Unlocks covert investigation without alerting NPCs
- Gates shadow and secret paths

#### Progression Mechanics

**Starting Level**: All stats begin at 1
**XP Requirements**:
- Level 1→2: 10 XP
- Level 2→3: 25 XP
- Level 3→4: 50 XP
- Level 4→5: 100 XP

**Stat Level Effects on Bound Cards**:
- Level 1: Base card values
- Level 2: +10% success rate, unlock basic stat approaches
- Level 3: All cards gain Persistent keyword, unlock advanced paths
- Level 4: +20% success rate (total), unlock powerful investigation
- Level 5: Cards ignore ForceListen failure effect (other failure effects still apply), master reputation

**XP Sources**:
- Playing any card: Base XP to bound stat
- Conversation difficulty multiplier (1x/2x/3x)
- Success or failure both grant XP

### Player Deck Architecture

The player owns a single conversation deck used in all conversations:

**Starting Deck**: 20 basic cards representing fundamental social skills
**Deck Growth**: New cards gained through request completion
**Stat Enhancement**: Cards gain bonuses from their bound stat's level, not individual progression
**Deck Refinement**: Certain locations allow deck thinning for a cost

### NPC Architecture

Each NPC maintains four persistent decks plus a request system:

**Persistent Decks**:
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
   - Accessed through Exchange action (0 segments)
   - Has its own UI, not part of conversation flow
   - Cards show trades: resource A → resource B

**Request System**:
  NPCs don't have a "Request Deck" but rather a list of Requests. Each Request is a higher-level bundle containing:
  - **Request Cards**: Multiple cards representing different goal thresholds (basic, enhanced, premium)
  - **Promise Cards**: Associated cards that can force queue position and burn tokens
  - **Status Tracking**: Whether the request is available, completed, or failed
  - **Narrative Context**: The story reason for this request

When a player chooses a request conversation type, ALL cards from that Request bundle (both request cards and promise cards) are added to the conversation. Request cards are added to the request pile and promise cards are shuffled into the draw pile.

### Four-Pile System

#### Draw Pile
- Created at conversation start from player deck + NPC signature cards + observation cards
- Shuffled once at start
- When empty, shuffle exhaust pile to reform

#### Active Pile (Hand)
- Cards currently available to play
- No maximum hand size
- Most cards lost on LISTEN 

#### Exhaust Pile
- Played cards go here
- Cards removed by Impulse/Opening go here
- Shuffled to create new draw pile when needed

#### Request Pile
- Request cards go here
- Must be unlocked by reaching the card's rapport threshold
- All unlockable cards get moved to Active Pile on next Listen
- Request cards in Active Pile are always playable with 100% success chance, ending the conversation when played

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
   - Check ALL goal cards for rapport thresholds
   - Each card that meets its threshold becomes playable
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
   - Failure: -1 flow, apply failure effects (ForceListen applied if effect is None)
7. **Card goes to exhaust pile**
8. **Apply card effects**:
   - Rapport changes
   - Atmosphere changes
   - Draw/focus effects
9. **Apply personality effects** (like Devoted's double rapport loss)
10. **Check flow transitions**:
    - At ±3: State change, flow resets to 0
11. **If Success**: Execute Card Success Effect. Can SPEAK again if focus remains
12. **If Failure**: Execute Card Failure Effect (ForceListen applied automatically if effect is None)

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
- Time cost: Base segment + (patience spent × 5 minutes depth)

#### Card Persistence Types

Cards use the Persistence property to determine when they must be played:

#### Thought
- Remains in hand through LISTEN actions
- Represents considered statements you can hold until the right moment
- Never exhausts automatically
- Valuable anchors for strategy

#### Impulse
- Must be played this SPEAK or removed
- Represents reactive, spontaneous responses
- Removed after any SPEAK action if unplayed
- Creates urgency to act

#### Opening
- Time-sensitive conversational opportunity
- Removed after LISTEN if unplayed
- Represents moments that pass if not seized

### Card Difficulty Tiers

Difficulty determines both base success rate and effect magnitude:

- **Very Easy** (85% base): Magnitude 1 effects
- **Easy** (70% base): Magnitude 2 effects  
- **Medium** (60% base): Magnitude 2 effects
- **Hard** (50% base): Magnitude 3 effects
- **Very Hard** (40% base): Magnitude 4 effects

All success rates modified by: +2% per rapport point

### Deck Cycling Example

**Turn 1**: Draw pile has 28 cards (20 player + 3 signature + 5 observation), hand has 4 cards
- SPEAK a card → success → goes to exhaust pile

**Turn 2**: Draw pile has 24 cards, hand has 3, exhaust has 1
- SPEAK a card → **FAILURE** → failure effect applied (often ForceListen)

**Turn 3**: Must LISTEN (due to ForceListen effect)
- Discard 2 non-persistent cards from hand
- Keep 1 persistent card
- Draw 4 new cards from draw pile
- Focus refreshes to maximum

**Turn 20**: Draw pile empty, hand has 2 persistent cards, exhaust has 26 cards
- LISTEN → need to draw 4 cards
- Shuffle exhaust pile (26 cards) → becomes new draw pile
- Draw 4 cards from new draw pile → hand

This creates natural deck cycling where all cards remain available throughout the conversation, but timing of when cards appear becomes critical.

### Stat-Based Card Enhancement

Cards don't level individually. Instead, they gain power from their bound stat:

**Stat Level Effects on Bound Cards**:
- **Level 1**: Cards function at base values
- **Level 2**: +10% success rate
- **Level 3**: All cards gain Persistent keyword  
- **Level 4**: +20% success rate (total)
- **Level 5**: Immune to ForceListen failure effect

**XP Accumulation**:
- Each card play grants XP to its bound stat
- Base rate: 1 XP per play (success or failure)
- Modified by conversation difficulty:
  - Level 1 conversation: 1 XP per card
  - Level 2 conversation: 2 XP per card
  - Level 3 conversation: 3 XP per card

### Conversation Difficulty Levels

All conversations have a difficulty level affecting both challenge and rewards:

**Level 1**:
- +10% success rate on all cards
- 1 XP per card played to bound stat
- Typical: Street vendors, pilgrims, clerks
- Lower rapport thresholds for goals

**Level 2**:
- Normal success rates
- 2 XP per card played to bound stat  
- Typical: Named NPCs, traveling merchants
- Standard rapport thresholds

**Level 3**:
- -10% success rate on all cards
- 3 XP per card played to bound stat
- Typical: Nobles, scholars, master traders
- Higher rapport thresholds for goals

Difficulty is determined by:
- NPC level (strangers have explicit levels)
- Conversation type chosen (request harder than chat)
- NPC importance (Elena's letter is level 3)

### Stranger Encounter System

Strangers are unnamed NPCs that provide resource generation and stat grinding opportunities without building relationships.

#### Stranger Properties
- **No Token Generation** - Only named NPCs build relationships
- **No Signature Cards** - Use only player deck
- **No Observation Deck** - Cannot receive observations
- **No Burden Accumulation** - One-time interactions
- **Standard Personality Rules** - Apply normally
- **Single Availability** - Can converse once per time block

#### Encounter Levels

**Level 1 Strangers**:
- Examples: Street vendors, pilgrims, workers
- +10% success rate on all cards
- 1 XP per card played
- Lower rapport thresholds

**Level 2 Strangers**:
- Examples: Traveling merchants, guards, clerks
- Normal success rates
- 2 XP per card played
- Standard rapport thresholds

**Level 3 Strangers**:
- Examples: Visiting nobles, foreign scholars
- -10% success rate on all cards
- 3 XP per card played
- Higher rapport thresholds

#### Conversation Rewards

**Friendly Chat** (5/10/15 rapport):
- Basic: 2 coins
- Enhanced: 4 coins + bread (weight 1)
- Premium: 6 coins + medicine (weight 1)

**Trade Negotiation** (5/10/15 rapport):
- Basic: Trade goods (weight 2, value 5 coins)
- Enhanced: Trade goods (weight 2, value 8 coins)
- Premium: Trade goods (weight 3, value 12 coins)

**Information Gathering** (5/10/15 rapport):
- Basic: Reveals one face-down path
- Enhanced: Reveals two paths + marks observation
- Premium: Reveals all paths on entire route

**Seek Favor** (8/12/20 rapport):
- Basic: 5 coins
- Enhanced: Simple permit (weight 1)
- Premium: 10 coins + valuable item

#### Location Scheduling

Strangers appear at specific spots during certain blocks:

**Market Square Morning**:
- Fresh Bread Vendor (L1, Steadfast)
- Spice Merchant (L2, Mercantile)
- Foreign Dignitary (L3, Proud)

**Tavern Evening**:
- Tired Laborer (L1, Devoted)
- Traveling Minstrel (L2, Cunning)
- Merchant Captain (L3, Mercantile)

[Additional locations and schedules as needed]

## Player Deck Design and Progression

### Starting Deck Composition

The player begins with 20 cards distributed across five stats:

**Insight Cards** (4 cards) - Analytical and observational:
- "Let me analyze" - Focus 1, Easy, Thought/Rapport/None/None
- "Notice pattern" - Focus 2, Medium, Thought/Threading/None/None
- "Consider evidence" - Focus 3, Medium, Thought/Rapport/None/None  
- "Deep observation" - Focus 4, Hard, Thought/Focusing/None/None

**Rapport Cards** (4 cards) - Empathetic and supportive:
- "I understand" - Focus 1, Easy, Thought/Rapport/None/None
- "Let me help" - Focus 2, Medium, Thought/Rapport/None/None
- "Share burden" - Focus 3, Medium, Thought/Rapport/Backfire/None
- "Deep connection" - Focus 4, Hard, Thought/Rapport/None/None

**Authority Cards** (4 cards) - Commanding and dominant:
- "Listen carefully" - Focus 1, Easy, Thought/Rapport/None/None
- "Take charge" - Focus 2, Medium, Thought/Atmospheric-Focused/None/None
- "Direct order" - Focus 3, Hard, Impulse/Rapport/Backfire/Regret
- "Final word" - Focus 5, Very Hard, Impulse/Rapport/Disrupting/Regret

**Commerce Cards** (4 cards) - Transactional and optimizing:
- "Fair exchange" - Focus 1, Easy, Thought/Rapport/None/None
- "Find the angle" - Focus 2, Medium, Thought/Threading/None/None
- "Mutual benefit" - Focus 3, Medium, Thought/Rapport/None/None
- "Close the deal" - Focus 4, Hard, Thought/Rapport/Backfire/None

**Cunning Cards** (4 cards) - Indirect and subtle:
- "Subtle hint" - Focus 1, Easy, Thought/Rapport/None/None
- "Misdirection" - Focus 2, Medium, Thought/Atmospheric-Patient/None/None
- "Hidden meaning" - Focus 3, Medium, Thought/Rapport/None/None
- "Perfect lie" - Focus 4, Hard, Impulse/Rapport/Disrupting/None

### Stat Progression System

**XP Accumulation**:
- Every card played grants XP to its bound stat
- Base rate: 1 XP (modified by conversation difficulty)
- Both success and failure grant XP (practice is practice)

**Stat Level Benefits**:
All cards bound to a stat gain uniform benefits:
- Level 2: +10% success rate
- Level 3: Gain Persistent keyword
- Level 4: +20% success rate total
- Level 5: Immune to ForceListen failure effect

**Strategic Development**:
Players naturally develop specialties through play patterns. High Rapport players find empathetic approaches more reliable. High Cunning players excel at indirect communication. Balanced builds remain viable but less specialized.

### Gaining New Cards

**From Named NPCs**: Request completion grants 1-3 cards based on tier
**From Strangers**: Never grant cards (only resources)
**From Observations**: Some grant player cards instead of NPC advantages
**Card Properties**: Every new card must specify its bound stat

### Deck Refinement

Deck thinning services available at specific locations:
- Cost: 10 coins per card removed
- Minimum deck size: 20 cards
- Strategic consideration: Remove cards from underdeveloped stats

### NPC Signature Cards

Each NPC has 5 unique cards that shuffle into the player's deck based on tokens:

#### Example: Marcus's Commerce Cards
- **1 token**: "Marcus's Rapport" - Focus 2, Hard, Thought/Rapport/None/None
- **3 tokens**: "Trade Knowledge" - Focus 3, Easy, Thought/Threading/None/None
- **6 tokens**: "Commercial Bond" - Focus 1, Very Hard, Thought/Rapport/Backfire/None
- **10 tokens**: "Marcus's Favor" - Focus 4, Hard, Thought/Rapport/None/None (special: None failure effect without ForceListen fallback)
- **15 tokens**: "Master Trader" - Focus 5, Medium, Thought/Atmospheric-Focused/None/None

These cards represent the mechanical expression of each relationship, making every NPC conversation unique even with the same player deck.

### Strategic Deck Building

**Personality Optimization**: Build toward specific NPC types
- **Proud Focus**: Many different focus values for ascending order
- **Devoted Safety**: High success rate cards to avoid double penalties
- **Mercantile Power**: High-focus cards to maximize +30% bonus
- **Cunning Variety**: Diverse focus values to avoid repetition penalty
- **Steadfast Consistency**: Many small rapport gains within ±2 cap

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
- **Weight Constraint**: Cannot accept obligations exceeding weight capacity

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
- Weight: 1-6 (based on package size)

**Meeting Requests**:
- Time: Specific time block
- Location: Specific spot
- Duration: 1-2 time segments

**Resolution Requests**:
- Target: Clear burden cards
- Difficulty: Scales with burden count
- Reward: Relationship reset

Personality influences available requests:
- Proud NPCs: Urgent, high-position, heavy requests
- Devoted NPCs: Personal, low-payment, light requests
- Mercantile NPCs: Profitable, flexible, varied weight
- Cunning NPCs: Complex, multi-step requests

### Weight and Queue Integration

The queue gains depth through weight management:
- Multiple light letters (1 weight each) can bundle efficiently
- One heavy package (5 weight) limits other acceptances
- Trade goods compete with obligations for capacity
- Tools reduce available space for deliveries

Strategic patterns emerge:
- **Light Load Strategy**: Many small deliveries per trip
- **Heavy Commitment**: One major obligation for maximum profit
- **Mixed Approach**: Balance obligations with tools and consumables

## Resource Conversion Chains

### Time → Money → Progress
```
Investigation (1 segment) → Familiarity
Familiarity → Observation access → Items or NPC advantages
Work Action (4 segments) → Coins (scaled by hunger)
Coins → Food (reduces hunger) → Better work output next time
Better output → More coins → Critical purchases
```

### Cards → Success → Stronger Deck
```
Successful card plays → XP for connected Stat
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
Investigation (1 segment) → Location familiarity
Familiarity → Observation availability
Observation → Card to NPC deck OR physical item
NPC observation card → Conversation advantages or unlocks
Items → Enable paths or provide resources
New routes → More opportunities
```

### Weight → Choices → Consequences
```
Limited capacity (10) → Must prioritize
Heavy obligations → Fewer other items
Tools → Enable paths but reduce capacity
Consumables → Insurance but less profit
Trade goods → Profit but opportunity cost
Every choice → Different paths available
```

### Focus → Cards → Rapport → Flow
```
Higher states → More focus capacity
More capacity → Can play more cards before LISTEN
Successful plays → Positive rapport momentum
ForceListen effect → Topic change, lose cards
Must rebuild → Draw new hand
Better states → More cards drawn on LISTEN
```

## Work System

### Work Action Mechanics

**Standard Work**:
- Base output: 5 coins (varies by type)
- Time cost: 4 segments (one full block)
- Hunger scaling: Output = Base - floor(hunger/25)
- Advances immediately to next time block

**Enhanced Work** (requires access):
- Base output: 6-7 coins
- Time cost: 4 segments (one full block)
- May require permits or relationships
- Still affected by hunger scaling

**Service Work**:
- Base output: 3-4 coins + benefits
- Time cost: 4 segments (one full block)
- Benefits may include meals or resources
- Reduces hunger as part of payment

### Work Efficiency Optimization

Optimal work sequence:
1. Morning: Eat breakfast (hunger → 0, costs 1 weight temporarily)
2. Morning: Work first job (5 coins, consumes block)
3. Afternoon: Hunger at 20, still efficient
4. Afternoon: Work second job (5 coins, consumes block)
5. Evening: Buy food with profits
6. Net gain: 7-8 coins after food costs

Suboptimal sequence:
1. Skip breakfast (save 2 coins)
2. Work at hunger 50 (only 3 coins)
3. Work at hunger 70 (only 2 coins)
4. Must buy food anyway
5. Net gain: 2-3 coins

The time block commitment makes work a major strategic decision. You sacrifice an entire block that could enable 4 investigations or multiple conversations.

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

- **Cost**: 0 segments (instantaneous trade)
- **Process**:
  1. Access NPC's exchange deck
  2. See all available exchange cards in separate UI
  3. Select one exchange card (if you can afford it)
  4. Pay cost, receive effect
  5. Exchange completes immediately
- **No conversation mechanics** (no focus, rapport, flow, patience)
- **Pure resource trade**

### Common Exchange Types

**Food Exchanges**:
- Simple meal: 2 coins → -30 hunger (immediate)
- Buy bread: 3 coins → bread item (weight 1, consume later for -30 hunger)
- Full meal: 5 coins → -50 hunger (immediate)
- Feast: 8 coins → -100 hunger, +10 health (immediate)

**Permit Exchanges**:
- District permit: 15-20 coins → permit item (weight 1)
- Special access: 25-30 coins → special permit (weight 1)
- Temporary pass: 5-10 coins → limited permit (weight 1)

**Information Exchanges**:
- Rumors: 3 coins → location hint
- Maps: 10 coins → reveal routes
- Schedules: 5 coins → NPC timings

**Service Exchanges**:
- Healing: 10 coins → +20 health
- Rest: 5 coins → restore stamina
- Storage: 2 coins → bank items (future content)
- Deck thinning: 5-10 coins → remove one card

**Item Exchanges**:
- Buy tool: X coins → tool item (permanent weight)
- Sell trade goods: trade good item → X coins
- Buy consumable: X coins → consumable item (weight 1)

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
- Quick (0 segments)
- No social mechanics
- Deterministic outcomes

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
- Weight management through consumable purchases

## No Soft-Lock Architecture

Every system has escape valves preventing unwinnable states.

## Content Scalability

### Adding NPCs - Simple Framework

New NPCs simply need:
- **Personality type**: Determines patience, token preference, and conversation rule
- **Four persistent decks**:
  - Signature deck (5 unique cards at token thresholds)
  - Observation deck (receives location discoveries)
  - Burden deck (damaged relationship tracking)
  - Exchange deck (if mercantile)
- **Request list**: Bundles of request and promise cards with weight specifications
- **Token rewards**: For successful deliveries
- **Starting state**: Connection state with player
- **Location**: Where they can be found
- **Conversation Level**: 1-3 difficulty rating
- **For Strangers**: Location schedule, reward tables, no tokens

### Adding Locations - Modular Design

New locations need:
- **Spot definitions**: With time-based properties
  - Morning: Quiet/Normal/Busy
  - Afternoon: Quiet/Normal/Busy
  - Evening: Closing/Open/Busy
- **Investigation rewards**: At each familiarity level (items or observations)
- **Observation destinations**: Which NPC gets each observation OR what item found
- **NPCs present**: Who is where when
- **Routes**: Connections with weight-aware path cards

### Adding Cards - Clear Rules

New player cards must follow:
- **Single effect**: One clear purpose (not multiple)
- **Focus range**: 0-6 focus cost
- **Difficulty tier**: Very Easy to Very Hard
- **Persistence**: ~20% should be Persistent
- **Rarity**: Common, Uncommon, or Rare
- **Stat Binding**: Must specify which stat (Insight/Rapport/Authority/Commerce/Cunning)

### Adding Items

New items need:
- **Weight**: 1-6 typically
- **Type**: Obligation, Tool, Consumable, Trade Good, or Permit
- **Effect**: Clear mechanical benefit or trade value
- **Source**: Where/how obtained
- **Restrictions**: Any special limitations

### Adding Observation Rewards

New observations need:
- **Source location**: Where discovered
- **Familiarity requirement**: 1, 2, or 3
- **Prior observation**: Prerequisites if any
- **Output Type**: Card to NPC deck OR physical item
- **Target NPC**: Who receives the card (if card type)
- **Item Properties**: Weight and effect (if item type)
- **Consumption**: Always consumed when played (if card)

### Adding Strangers

When creating stranger NPCs:
- **Assign Level**: 1-3 determining difficulty and XP multiplier
- **Set Personality**: Uses standard personality rules
- **Define Availability**: Which locations and time blocks
- **Create Reward Tables**: Based on conversation types offered
- **No Token Generation**: Strangers never give tokens
- **No Signature Cards**: Don't have unique cards
- **Resource Focus**: Primary purpose is resources and XP

### Emergent Narratives

Stories emerge from mechanical interaction, not scripting:

**The Overloaded Courier**:
- Accept Marcus's heavy silk delivery (weight 5) for profit
- Find valuable trade goods (weight 3) during travel
- Elena's urgent letter arrives (weight 1)
- Already at 9/10 capacity
- Must choose: profit, discovery, or relationship
- Drop silk, lose merchant reputation
- Or drop trade goods, lose profit opportunity
- Or refuse Elena, damage friendship
- Every choice has permanent consequences

**Building Power Through One NPC**:
- Focus all deliveries to Marcus
- Gain 10 Commerce tokens
- Unlock 4 signature cards
- Conversations with Marcus become trivial
- His Mercantile rule (+30% to highest focus) synergizes with high-focus cards
- Can reliably reach Premium goals
- But neglected other relationships
- And Marcus's heavy packages limit other opportunities

**Card Mastery Story**:
- "Bold Claim" card used successfully 30 times
- Reaches level 5 - Mastered
- Immune to ForceListen failure effect
- Becomes anchor card for risky strategies
- Enables conversation momentum others can't maintain
- Player known for bold, aggressive conversation style

**The Cascading Failure**:
- Failed conversation with Elena (no signature cards)
- Forced to accept Basic goal for urgent delivery
- Low payment means can't afford food
- Work at high hunger for poor output
- Can't afford permit for efficient route
- Take longer path costing more segments
- Miss deadline due to time shortage
- Lose tokens with recipient
- Future conversations even harder

**Weight Management Master**:
- Always carries crowbar (weight 2) for shortcuts
- Keeps one food (weight 1) for emergencies
- Leaves 7 capacity for obligations
- Can accept most profitable combinations
- Takes paths others cannot due to tool access
- But sacrifices maximum delivery capacity
- Strategic tool investment over pure profit

## Core Innovation Summary

The core gameplay loop is **conversations as character progression**:

### The Player Deck as Character

Your conversation deck IS your character. Every card represents a social skill you've learned. Every level up makes that specific approach more reliable. Every new card gained expands your repertoire. This isn't abstract character growth - it's tangible, visible, and strategic.

### Conversations as Combat

The categorical property system means cards represent conversational approaches rather than specific outcomes. A Thought/Rapport/None/None card represents a calculated statement that could strengthen or destroy the relationship. The same card performs differently under various atmospheres and rapport levels, creating emergent complexity from simple rules.

Each conversation is a tactical puzzle where you play your deck against NPC personality rules:
- **Proud NPCs** force ascending focus order - like enemies that punish repetition
- **Devoted NPCs** double rapport losses - like glass cannon fights
- **Mercantile NPCs** reward big plays - like bosses with weak points
- **Cunning NPCs** punish patterns - like adaptive AI opponents
- **Steadfast NPCs** cap rapport changes - like damage reduction enemies

### Failure Creates Rhythm

When you fail a card play, the failure effect applies (typically ForceListen), forcing you to LISTEN and drawing new cards. This isn't punishment - it's conversation rhythm. Success maintains momentum with your current topics. Failure effects create topic changes. The mechanical flow mirrors actual dialogue.

### Weight Creates Physical Reality

The 10-weight capacity isn't arbitrary limitation - it's physical reality. Heavy packages prevent accepting other obligations. Tools enable paths but consume capacity permanently. Food takes space until consumed. Every choice about what to carry affects what paths you can take, what obligations you can accept, and what opportunities you can pursue.

### Time Segments Create Natural Rhythm

The 24-segment day organized into 6 blocks creates natural activity patterns. Morning investigations are twice as efficient. Work consumes entire blocks. Travel costs segments that could enable conversations. Every action has opportunity cost measured in concrete time.

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

The stat system transforms character progression from card-specific mastery to developing problem-solving methodologies. Players literally become better conversationalists by practicing different approaches - analytical, empathetic, authoritative, mercantile, or cunning. These developed competencies then unlock investigation methods and travel paths, making conversation truly the engine for all progression. Stranger encounters provide the "grinding" layer expected from RPGs while maintaining focus on meaningful relationships with named NPCs.

## Critical Formulas Reference

**Success Rate**: Base% + (2 × Current Rapport)

**Effect Magnitude by Difficulty**:
- Very Easy: 1
- Easy: 2
- Medium: 2
- Hard: 3
- Very Hard: 4

**Atmosphere Magnitude Modifiers**:
- Volatile: Rapport effects ±1
- Focused: All magnitudes +1
- Exposed: All magnitudes ×2
- Synchronized: Effect happens twice

**Stat Level Thresholds**:
- Level 1→2: 10 XP
- Level 2→3: 25 XP
- Level 3→4: 50 XP
- Level 4→5: 100 XP

**Stat Success Modifiers**:
- Stat Level 2: +10% success on all bound cards
- Stat Level 3: All bound cards gain Persistent keyword
- Stat Level 4: +20% success on all bound cards
- Stat Level 5: Bound cards are immune to ForceListen failure effect

**Stat XP Gain Formula**:
Base XP × Conversation Difficulty Multiplier
- Base: 1 XP per card played
- Level 1 conversation: ×1
- Level 2 conversation: ×2
- Level 3 conversation: ×3

**Conversation Difficulty Modifiers**:
- Level 1: +10% success rate all cards
- Level 2: Normal success rates
- Level 3: -10% success rate all cards

**Work Output**: 5 - floor(Hunger/25) coins

**Travel Speed Penalty**: At 75+ hunger, +1 segment to all paths

**Investigation Gain**: 
- Quiet spots: +2 familiarity per segment
- Busy spots: +1 familiarity per segment
- Other: +1 familiarity per segment

**Displacement Cost**: 
- To position N from position M: 
- Burn (M-N) tokens with position N
- Burn (M-N-1) tokens with position N+1
- ... continue for each displaced position

**Weight Capacity**: Maximum 10, no exceptions

**Time per Day**: 24 segments (6 blocks × 4 segments)

**Conversation Time**: 1 segment + (patience spent × 5 minutes depth)

**Hunger Increase**: +20 per time block

**Starvation**: At 100 hunger, lose 5 health per block

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

**Item Weights**:
- Letters: 1
- Simple packages: 1-2
- Trade goods: 3-4
- Heavy deliveries: 5-6
- Tools: 1-3
- Consumables: 1
- Permits: 1

## Conclusion

Wayfarer achieves its design goals through making conversations the core gameplay loop while maintaining perfect verisimilitude through physical constraints. The player's conversation deck represents their character growth in the most literal sense - every card is a social skill they've learned and mastered through practice.

The addition of weight and time segments transforms abstract resources into physical reality. You cannot carry more than 10 weight because that's what a person can physically manage. Travel takes longer when hungry because exhaustion slows movement. Morning investigations yield more because locations are quieter. Everything makes immediate intuitive sense.

The genius is that this uses familiar deck-building mechanics to represent character progression, while NPC personality rules and signature cards ensure every conversation feels unique despite using the same player deck. The ForceListen failure effect creates natural conversation rhythm where success builds momentum and failure forces adaptation.

Players always know what to do: have conversations to gain cards and XP, level up their deck, unlock signature cards through relationships, and tackle increasingly complex NPC personalities. The strategic depth emerges from how player deck composition, card levels, signature cards, personality rules, weight limits, and time constraints interact to create unique puzzles.

The system succeeds because mastery comes from understanding these interactions and building a deck that reflects your personal approach to social challenges while managing the physical realities of weight and time. Every conversation is practice. Every card gained is permanent progression. Every relationship built provides new tools. Every item carried has opportunity cost. This is character growth made tangible, strategic, and elegantly integrated with the narrative of becoming a better conversationalist and courier in a living world.