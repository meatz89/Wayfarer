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
9. [Player Stats System](#player-stats-system)
10. [Conversation Card Distribution](#conversation-card-distribution)
11. [Queue Management System](#queue-management-system)
12. [Strategic Resource Management](#strategic-resource-management)
13. [Economic Balance Points](#economic-balance-points)
14. [Resource Conversion Chains](#resource-conversion-chains)
15. [Work System](#work-system)
16. [Exchange System](#exchange-system)
17. [No Soft-Lock Architecture](#no-soft-lock-architecture)
18. [Content Scalability](#content-scalability)
19. [The Holistic Experience](#the-holistic-experience)
20. [Content Loading System](#content-loading-system)
21. [Core Innovation Summary](#core-innovation-summary)
22. [Design Verification](#design-verification-checklist)

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

**BAD**: "Hunger reduces doubt AND work output AND travel speed"
**GOOD**: Hunger reduces work output (coins = 5 - floor(hunger/25)). Hunger increases travel segments (+1 at 75+). Two separate formulas.

**BAD**: "Focus affects multiple unrelated things"
**GOOD**: Focus has one clear purpose - determining how many cards can be played per SPEAK cycle.

**BAD**: "Investigation gives familiarity AND cards"
**GOOD**: Investigation gives familiarity. Observation gives cards (requires familiarity). Two separate actions.

## Conversation Type System

### Core Concept

Instead of players owning a personal conversation deck that levels up, each conversation TYPE has its own predefined deck. This ensures cards are always contextually appropriate - you won't have authority cards when comforting desperate Elena or momentum cards when confronting bandits. Cards feel like natural conversation options, not collectible battle cards.

### Strategic Card Architecture

Each conversation deck is built around a 4-tier strategic framework:

**Generators** (6 cards): Build momentum at different efficiency/power levels
**Converters** (6 cards): Trade momentum for other resources (doubt reduction, flow, cards)
**Investments** (4 cards): Scaling effects that reward different game states
**Utility** (4 cards): Focus manipulation and support effects

### Power Tier Structure

**TIER 1 (1-2 Focus) - Efficient but Weak:**
- High efficiency (1 momentum per focus)
- Always affordable foundation cards
- Enable consistent progress every turn

**TIER 2 (3-4 Focus) - Standard Power:**
- Balanced efficiency (0.75-1 momentum per focus)
- Core strategic options requiring planning
- Most conversation decisions happen here

**TIER 3 (5-6 Focus) - High Impact:**
- Lower efficiency but high absolute power
- Require significant planning or focus manipulation
- Game-changing potential with proper setup

### Standard Conversation Types

**Friendly Chat**: Casual, balanced mix of all stats
- Used for building relationships without pressure
- Contains cards from all four strategic categories
- Lower difficulty cards predominate

**Desperate Request**: Heavy Rapport/Insight, zero Authority
- For NPCs in crisis needing help
- Empathetic generators and doubt-reducing converters
- Scaling cards that reward emotional investment

**Trade Negotiation**: Heavy Commerce/Insight, zero Rapport
- Business discussions and deals
- Momentum-efficient generators and flow converters
- Utility cards for focus optimization

**Authority Challenge**: Heavy Authority/Cunning, zero Rapport
- Confrontations and power struggles
- High-impact generators and flow investments
- Risk-reward converters for power plays

**Information Gathering**: Heavy Insight/Cunning
- Extracting knowledge and secrets
- Card-draw converters and scaling investments
- Focus manipulation for sustained questioning

**Intimate Confession**: Emotional depth, all stats present
- Deep personal conversations
- Full strategic spectrum with scaling rewards
- Requires established relationships for effectiveness

### Contextual Deck Usage

The same NPC uses different conversation decks based on context:
- Marcus at his shop: Trade Negotiation deck (momentum-efficient, flow-focused)
- Marcus at the tavern: Friendly Chat deck (balanced strategic options)
- Marcus when desperate: Desperate Request deck (doubt-reduction focused)

Elena's progression:
- First meeting: Desperate Request (doubt management critical)
- After trust built: Intimate Confession (scaling investments available)
- At market: Friendly Chat (casual momentum building)

### Verisimilitude Through Context

This system prioritizes verisimilitude over mechanical flexibility. Conversations feel like real interactions where available responses match the context and strategic situation. You literally cannot access high-authority generators when someone needs emotional support because those cards aren't in that conversation type's deck.

Player expression comes through:
- Understanding the strategic framework of each conversation type
- Managing focus resources across the power tier structure
- Choosing between momentum generation and resource conversion
- Leveraging scaling cards based on current game state
- Building relationships that unlock different strategic contexts

## Three Core Game Loops

### System Integration Philosophy

The three core game loops answer fundamental design questions while maintaining strict mechanical separation. Each loop creates problems that only the other loops can solve, forcing engagement with all systems.

### Core Loop 1: Card-Based Conversations

#### Design Questions Answered
- **What provides challenge?** Using conversation type cards effectively against NPC personality rules
- **Why grow stronger?** Leveling cards and gaining new ones improves success across all conversations
- **Why engage with NPCs?** Request cards provide income, access, world progression, and stat XP

#### The Conversation as Core Activity

Conversations are the primary gameplay loop - your "combat encounters" expressed through social dynamics. Each conversation type provides a specific deck of cards appropriate to that social context. Your stats determine how effectively you can use these cards, representing your growing competence with different conversational approaches.

The mechanical depth comes from playing contextually appropriate cards against each NPC's personality rules, creating different puzzles based on the conversation type. Success in conversations leads to request completion and stat XP gain, making you permanently more effective with cards bound to those stats.

#### Connection States

- **Disconnected**: 3 focus capacity, 3 cards drawn on LISTEN (0 tokens)
- **Guarded**: 4 focus capacity, 3 cards drawn on LISTEN (1-2 tokens)
- **Neutral**: 5 focus capacity, 4 cards drawn on LISTEN (3-5 tokens)
- **Receptive**: 5 focus capacity, 4 cards drawn on LISTEN (6-9 tokens)
- **Trusting**: 6 focus capacity, 5 cards drawn on LISTEN (10+ tokens)

Tokens with an NPC determine your starting connection state for conversations. This represents the accumulated relationship capital affecting how open they are from the start. Better connection states provide more focus capacity and cards drawn on LISTEN.

At -3 flow in Disconnected: Conversation ends immediately.
At -3 flow in other states: State shifts left, flow resets to 0.

Connection states also affect doubt accumulation rates, with better relationships providing some protection against doubt increases during failed interactions.

#### NPC Deck Systems

Each NPC maintains four persistent decks and a list of requests:

**Four Persistent Decks**:
1. **Signature Deck**: Unique cards that enhance conversations based on token count
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

Each NPC has 5 unique signature cards that are mixed into the conversation based on your token count:
- **1-2 tokens**: 1 signature card added
- **3-5 tokens**: 2 signature cards added
- **6-9 tokens**: 3 signature cards added
- **10-14 tokens**: 4 signature cards added
- **15+ tokens**: All 5 signature cards added

These cards are specific to that NPC, not generic token type cards. Marcus doesn't give "Commerce cards," he gives "Marcus's Bargain," "Silk Road Knowledge," and "Marcus's Favor." Elena gives "Elena's Trust," "Shared Burden," and "Elena's Hope." These cards mechanically represent the nature of your specific relationship and enhance the base conversation type deck.

#### Personality Rules

Each personality type applies one rule that fundamentally changes how conversations work:

- **Proud**: Cards must be played in ascending focus order each turn (resets when you LISTEN)
- **Devoted**: When doubt increases, increase by 1 additional point (catastrophizes failure)
- **Mercantile**: Your highest focus card each turn gains +30% success
- **Cunning**: Playing same focus as previous card costs -2 momentum
- **Steadfast**: All momentum changes are capped at ±2 per card

These rules represent how different personalities respond to conversation. A Proud person needs escalating respect. A Devoted person catastrophizes failure by accumulating extra doubt. A Merchant rewards getting to the point with success bonuses.

#### Conversation Outputs
- **Stat XP**: Each card play grants XP to its bound stat (1 XP base, multiplied by conversation difficulty)
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
- **Why revisit locations?** Building relationships for better starting connection states
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
Permanent equipment providing persistent benefits at weight cost. A crowbar weighs 2, enables "Forced Entry" path cards. A merchant ledger weighs 1, provides +1 starting Commerce momentum. A rope weighs 2, enables "Cliff Descent" paths. Tools never leave your satchel unless dropped, creating long-term capacity decisions.

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

### Momentum Economy Integration

**Momentum Generation Rebalancing**:
- **Previous**: 2-4 momentum per card average created resource inflation
- **New**: 1-3 momentum per card average requires strategic planning
- **Goal Thresholds**: Basic 8, Enhanced 12, Premium 16 (reduced from 10/15/20)
- **Impact**: Matches reduced generation while maintaining 5-8 turn conversations

**Resource Conversion Introduction**:
- Cards that spend momentum for doubt reduction, flow advancement, card draw
- Creates spend vs save decisions every turn
- Prevents pure momentum hoarding strategies
- Enables multiple paths to goal achievement

**Focus Constraint Management**:
- Tier 1 cards (1-2 focus): Always affordable foundation
- Tier 2 cards (3-4 focus): Require turn planning
- Tier 3 cards (5-6 focus): Demand setup or focus manipulation
- Creates meaningful resource allocation decisions

### Time Segment Economy Connections

**Daily Allocation**: 24 segments (6 blocks × 4 segments each)

Time segments enable:
- **Conversations** (1 segment + doubt depth): Access to letters and tokens
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
- **Conversation**: 1 segment base + doubt depth
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
- Card effects can overcome obstacles

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

**Primary Effect**: Determine starting connection state for conversations
- 0 tokens: Disconnected
- 1-2 tokens: Guarded
- 3-5 tokens: Neutral
- 6-9 tokens: Receptive
- 10+ tokens: Trusting

**Additional Uses Through Different Mechanics**:
1. **Starting State**: Higher tokens mean better initial connection
2. **Displacement Cost**: Burn tokens to jump queue positions
3. **Exchange Gating**: Minimum tokens required for special exchanges

**Generation**:
- Standard delivery: +1 token with recipient
- Failed delivery: -2 tokens with sender
- Special events and quests

**Token Investment Benefits**:
- Better starting connection states
- More signature cards mixed into conversation
- Access to token-gated exchanges
- Flexibility for queue displacement when needed

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
  - Certain cards can temporarily increase focus capacity
  - Can exceed maximum temporarily with Prepared
  - Health below 50 reduces capacity by 1
- **Strategic Role**: Core resource management within conversations. Enables multi-turn planning knowing failure typically forces LISTEN.

#### Momentum
- **Range**: 0 to 20+
- **Starting Value**: 0 (built through conversation)
- **Goal Thresholds**: Basic (8), Enhanced (12), Premium (16)
- **Effect**: Used to unlock request goals and can be consumed for techniques
- **Reduction**: Each LISTEN action, doubt reduces momentum by 1 point per doubt
- **Consumption**: Cards can spend momentum for doubt reduction, flow advancement, card draw
- **Resets**: After conversation ends
- **Strategic Role**: Core resource representing conversation progress toward goals

#### Flow
- **Range**: -3 to +3
- **Always starts at 0**
- **Changes**: 
  - Success on SPEAK: +1 flow
  - Failure on SPEAK: -1 flow
- **Effects**:
  - At +3: State shifts right, flow resets to 0
  - At -3 in Disconnected: Conversation ENDS
  - At -3 in other states: State shifts left, flow resets to 0
  - Excess flow lost (no banking)
- **State progression**: [Ends] ← Disconnected ← Guarded ← Neutral → Receptive → Trusting
- **Strategic Role**: Progress tracker forcing consistent success


#### Doubt
- **Range**: 0 to 10+ (inverted player health)
- **Starting Value**: 0 (increases through conversation)
- **Increase**: +1 doubt when card plays fail
- **Effect**: High doubt causes conversation failure/loss
- **Momentum Reduction**: Each LISTEN action, doubt reduces momentum by 1 point per doubt
- **Consumption**: Can be reduced through momentum-consuming cards
- **Strategic Role**: Rising doubt creates urgency and makes momentum-consuming cards "essentially free" since doubt would reduce momentum anyway

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
- Standard conversations (base cost + doubt depth)
- Investigation at any spot
- Observation when available
- Each segment of travel based on path cards

**Full Block Actions**:
- Work consumes all 4 segments, advancing to next block

#### Block Advancement

When segments in a block are exhausted, time advances to the next block. Starting an action without sufficient segments forces advancement. Work always jumps to the next block regardless of current segment position.

### Conversation Time Integration

Conversations cost 1 base segment plus doubt becomes time depth. Each point of doubt adds complexity to conversation resolution. This creates tension as doubt accumulates through failures.

**Connection State Doubt Modifiers**:
- Disconnected: Stressful conversation, doubt accumulates faster
- Guarded: Normal doubt accumulation
- Neutral: Normal doubt accumulation
- Receptive: Comfortable flow, -20% doubt from failures
- Trusting: Natural rhythm, doubt accumulates slower

This creates mechanical differentiation where better relationships enable longer conversations for the same time investment.

### Investigation Mechanics

Investigation has multiple approaches unlocked by player stats:

#### Investigation Approaches

**Standard Investigation** (Always available):
- Cost: 1 segment
- Effect: Normal familiarity gain based on spot properties

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

The conversation system represents the primary gameplay loop using a depth-based card progression inspired by Steamworld Quest. Players progress through **stat-gated card depths** while managing five distinct resources. Each conversation type provides a fixed deck appropriate to the context, but player stats determine which depths of cards are accessible, creating meaningful long-term progression without breaking verisimilitude.

### The Five Core Resources

Each resource has exactly ONE mechanical identity with no overlap:

**Initiative** - Conversational Action Economy:
- Determines how many cards can be played before the NPC responds
- Starts at Connection State value (3-6 based on relationship)
- Does NOT fully refresh - must be built through cards
- Some cards generate Initiative, others spend it
- Creates builder/spender dynamic inspired by Steamworld Quest

**Momentum** - Progress Toward Goals:
- Victory track toward conversation objectives
- Goal thresholds: Basic (8), Enhanced (12), Premium (16)
- Can be consumed by cards for powerful effects
- Subject to "doubt tax" - each point of Doubt reduces momentum gains by 20%
- Represents how close you are to achieving conversation objectives

**Doubt** - Rising NPC Skepticism:
- Timer that ends conversation at 10 points
- Increases by +1 when card plays fail
- Creates momentum tax (-20% gains per Doubt point)
- Can be reduced through cards or momentum spending
- Represents NPC's growing distrust of the conversation

**Cadence** - Conversation Balance (-5 to +10):
- Tracks who dominates the conversation
- +1 per card played (you talking), -3 per LISTEN (giving them space)
- High Cadence (6+): +1 Doubt per point above 5 on LISTEN
- Negative Cadence (-3+): +1 card draw on LISTEN
- Creates natural conversation rhythm and rewards strategic listening

**Connection State** - Relationship Baseline (Static):
- Determined by token count before conversation starts
- Sets starting Initiative (3-6) and base card draw (3-5)
- Does not change during conversation
- Represents existing comfort level between characters

### Stat-Gated Depth System

**Card Depth Architecture**:
Each conversation type deck contains cards organized by depth (1-10):
- **Depth 1-3**: Foundation cards (simple, always accessible to all players)
- **Depth 4-6**: Sophisticated cards (require stat progression)
- **Depth 7-10**: Master cards (complex, powerful, endgame content)

**Depth Access Mechanics**:
Your stat level determines maximum accessible depth for that stat's cards:
- **Rapport Level 3**: Can access Rapport cards up to Depth 3
- **Commerce Level 5**: Can access Commerce cards up to Depth 5
- **Authority Level 2**: Can access Authority cards up to Depth 2

**Progressive Deck Composition**:
- Novice players (stats 1-2): Small decks with simple cards only
- Experienced players (stats 5-7): Large decks mixing builders and powerful spenders
- Master players (stats 8+): Full access to conversation-defining depth 10 cards

**Conversation Structure**:
- NPC request chosen BEFORE conversation determines conversation type
- Each conversation type has exactly ONE unique card deck organized by depth
- Your stats filter which depths are included in your personal deck for this conversation
- Creates meaningful progression while maintaining contextual appropriateness

**Card Pile System**:
- **Deck** → **Mind** (hand) → **Spoken** (conversation memory)
- Most cards stay in Spoken pile permanently (you can't unsay things)
- No automatic reshuffling maintains conversation verisimilitude

### Builder/Spender Card Architecture

Cards create a Steamworld Quest-inspired dynamic where powerful cards require setup:

#### The Three Strategic Tiers

**Foundation Cards (Depth 1-3)** - Builders:
- Cost: 0-2 Initiative
- Effect: Often generate +1 to +2 Initiative
- Purpose: Enable powerful plays while making progress
- Example: "Active Listening" (Rapport) - 0 Initiative, +2 Initiative, +1 Cadence

**Standard Cards (Depth 4-6)** - Balanced:
- Cost: 3-5 Initiative
- Effect: Solid resource effects without Initiative generation
- Purpose: Efficient tactical options requiring some setup
- Example: "Thoughtful Response" (Insight) - 4 Initiative, +3 Momentum, +1 Cadence

**Decisive Cards (Depth 7-10)** - Spenders:
- Cost: 6-12 Initiative (often exceeding base capacity)
- Effect: Powerful conversation-defining effects
- Purpose: Win conditions requiring significant setup
- Example: "Perfect Understanding" (Rapport) - 8 Initiative, Double current Momentum

#### The Five Stats as Card Identities

**Insight** - Information and Analysis:
- Effects: Draw cards, reveal information, pattern recognition
- Scaling: Based on cards played or conversation length
- 0-Initiative: "Quick Question" (0 cost, Draw 1, +1 Doubt, +1 Cadence)

**Rapport** - Support and Trust Building:
- Effects: Generate Initiative, reduce Doubt, build connection
- Scaling: Based on current Momentum or positive game states
- 0-Initiative: "Active Listening" (0 cost, +2 Initiative, +1 Cadence)

**Commerce** - Resource Exchange:
- Effects: Convert between resources at favorable rates
- Scaling: Efficient ratios and multi-resource effects
- 0-Initiative: "Quick Trade" (0 cost, Convert 2 Momentum → 2 Initiative, +1 Cadence)

**Authority** - Power and Impact:
- Effects: High Initiative costs for powerful single effects
- Scaling: Based on Flow or conversation intensity
- 0-Initiative: "Direct Statement" (0 cost, +2 Momentum, +2 Doubt, +1 Cadence)

**Cunning** - Timing and Conditionals:
- Effects: Exploit specific game states, cost reductions
- Scaling: Based on Doubt, Cadence, or negative conditions
- 0-Initiative: "Deflection" (0 cost, -1 Doubt, -1 Momentum, +1 Cadence)

#### Scaling Using Visible State Only

All card effects scale with visible game state:
- **Cadence Scaling**: "Passionate Plea" (+X Momentum where X = current Cadence)
- **Spoken Cards**: "Building Argument" (+1 Momentum per 5 cards in Spoken)
- **Doubt Scaling**: "Desperate Gambit" (+X Momentum where X = current Doubt)
- **Hand Size**: "Overwhelming Options" (+X Initiative where X = cards in hand ÷ 2)
- **Momentum Scaling**: "Press Advantage" (costs -1 Initiative per 5 Momentum)

No hidden state tracking - all conditions visible to players.

#### Card Persistence Types

**Standard**: Goes to Spoken pile when played (most cards)
**Echo**: Returns to hand after playing (represents recurring themes)
- Example: "Persistent Argument" - Returns if conversation momentum increased
**Persistent**: Stays in hand until forcibly discarded
- Example: "Core Belief" - Remains available until directly challenged
**Banish**: Removes itself from conversation entirely
- Example: "Final Ultimatum" - One-use only, removed after playing

#### Alternative Cost Mechanics

Some powerful cards offer multiple payment options:
- **Momentum Alternative**: "8 Initiative OR 4 Initiative + spend 3 Momentum"
- **Cadence Discount**: "7 Initiative OR 4 Initiative if Cadence ≥ 5"
- **Doubt Emergency**: "6 Initiative OR 3 Initiative if Doubt ≥ 7"
- **State-Based**: "10 Initiative OR 6 Initiative + 2 cards from Spoken"

These create multiple strategic paths to enable powerful cards.

#### Stat Binding and Progression

Every card bound to exactly one stat (Insight, Rapport, Authority, Commerce, Cunning):
- Determines which stat gains XP when played
- Your stat level determines maximum accessible depth for those cards
- Higher stats make all bound cards more reliable through success bonuses
- Creates natural build specialization through depth access

#### Power Scaling by Depth

**Depth 1-2**: Simple, reliable effects
- "Agreement" - 1 Initiative, +1 Momentum
- "Quick Nod" - 0 Initiative, +1 Initiative

**Depth 4-5**: Multi-resource effects
- "Thoughtful Negotiation" - 4 Initiative, +2 Momentum, +1 Initiative
- "Strategic Pause" - 3 Initiative, +2 Momentum if Cadence ≤ 2

**Depth 7-8**: Complex, powerful effects
- "Emotional Breakthrough" - 6 Initiative (or 3 + spend 2 Flow), +4 Momentum, -3 Doubt
- "Master's Insight" - 8 Initiative, +X Momentum where X = Spoken cards ÷ 3

**Depth 9-10**: Conversation-defining effects
- "Perfect Understanding" - 10 Initiative, Double current Momentum, remove all Doubt
- "Ultimate Authority" - 12 Initiative OR 6 Initiative + 2 Cadence, Set Momentum to 20

### Player Stats System

Stats determine your conversational competencies and unlock deeper card options through the depth access system:

#### The Five Stats and Their Card Depth Access

**Insight** - Analytical thinking and observation
- **Card Identity**: Information gathering, pattern recognition, drawing cards
- **Depth Access**: Your Insight level = maximum depth for Insight cards you can access
- **Example Progression**: Level 1 (basic questions) → Level 5 (complex analysis) → Level 8 (master deduction)
- **World Effects**: Unlocks systematic investigation approaches, gates scholarly travel paths

**Rapport** - Empathetic connection and supportive communication
- **Card Identity**: Initiative generation, doubt reduction, trust building
- **Depth Access**: Your Rapport level = maximum depth for Rapport cards you can access
- **Example Progression**: Level 1 (simple agreement) → Level 5 (emotional breakthrough) → Level 8 (perfect understanding)
- **World Effects**: Unlocks social investigation through locals, gates community-based travel paths

**Authority** - Leadership and commanding presence
- **Card Identity**: High-cost powerful effects, direct impact, flow manipulation
- **Depth Access**: Your Authority level = maximum depth for Authority cards you can access
- **Example Progression**: Level 1 (mild assertion) → Level 5 (commanding presence) → Level 8 (ultimate authority)
- **World Effects**: Unlocks demanding access to restricted areas, gates noble and official travel paths

**Commerce** - Negotiation and trade thinking
- **Card Identity**: Resource conversion, efficient exchanges, multi-resource effects
- **Depth Access**: Your Commerce level = maximum depth for Commerce cards you can access
- **Example Progression**: Level 1 (simple trade) → Level 5 (complex negotiation) → Level 8 (master dealmaker)
- **World Effects**: Unlocks purchasing information directly, gates merchant caravan paths

**Cunning** - Subtlety and indirect approach
- **Card Identity**: State-based effects, cost reductions, exploiting conditions
- **Depth Access**: Your Cunning level = maximum depth for Cunning cards you can access
- **Example Progression**: Level 1 (simple deflection) → Level 5 (perfect timing) → Level 8 (master manipulation)
- **World Effects**: Unlocks covert investigation without alerting NPCs, gates shadow and secret paths

#### Depth-Based Progression System

**Mechanical Progression**:
- **Stats 1-2**: Access depths 1-2 (foundation cards only)
- **Stats 3-4**: Access depths 1-4 (foundation + developing cards)
- **Stats 5-6**: Access depths 1-6 (foundation + sophisticated cards)
- **Stats 7-8**: Access depths 1-8 (foundation + sophisticated + powerful cards)
- **Stats 9-10**: Access depths 1-10 (complete mastery, all conversation options)

**XP Requirements**:
- Level 1→2: 10 XP
- Level 2→3: 25 XP
- Level 3→4: 50 XP
- Level 4→5: 100 XP
- Level 5→6: 175 XP
- Level 6→7: 275 XP
- Level 7→8: 400 XP

**Stat Level Effects**:
- **All Levels**: Bound cards get +10% success rate per stat level
- **Level 3+**: Unlock advanced investigation approaches and travel paths
- **Level 5+**: Bound cards gain immunity to ForceListen failure effect
- **Level 7+**: Access to conversation-defining master cards

**XP Sources**:
- Playing any card: 1 XP to bound stat × conversation difficulty (1x/2x/3x)
- Success or failure both grant XP (practice makes perfect)
- Master conversations (difficulty 3) provide optimal XP for challenging progression

#### Build Specialization Through Depth Access

**Natural Specialization**:
Players develop distinct conversational styles based on their highest stats:
- **Insight Focus**: Masters of information gathering and analysis
- **Rapport Master**: Excels at trust building and emotional connection
- **Authority Build**: Dominates through direct power and commanding presence
- **Commerce Specialist**: Expert at resource management and efficient exchanges
- **Cunning Expert**: Exploits timing and conditions for maximum advantage

**Cross-Stat Synergies**:
Advanced players develop multiple stats for versatile approaches:
- **Insight + Commerce**: Analytical negotiator
- **Rapport + Authority**: Inspiring leader
- **Cunning + Insight**: Master manipulator with perfect information
- **Authority + Commerce**: Corporate power broker

**Progression Strategy**:
- Early game: Focus one stat to access deeper cards quickly
- Mid game: Develop 2-3 stats for tactical flexibility
- Late game: Master specialization while maintaining broad competency

### NPC Architecture

Each NPC maintains four persistent decks plus a request system:

**Persistent Decks**:
1. **Signature Deck**: Unique cards mixed based on token count
   - Not drawn at start - shuffled into conversation deck
   - Represents the specific relationship with this NPC
   - 5 cards total, more added as tokens increase
   
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
- Created at conversation start from conversation type deck + NPC signature cards + observation cards
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
- Must be unlocked by reaching the card's momentum threshold
- All unlockable cards get moved to Active Pile on next Listen
- Request cards in Active Pile are always playable with 100% success chance, ending the conversation when played

### LISTEN Action

Complete sequence:
1. **Apply Cadence Effects**
   - If Cadence ≥ 6: +1 Doubt per point above 5 (NPC frustrated by domination)
   - If Cadence ≤ -3: Mark for +1 card draw (NPC appreciates being heard)
   - Cadence decreases by 3 (giving them conversational space)

2. **Apply Doubt Tax on Momentum**
   - Reduce momentum by 20% per current Doubt point
   - Example: At 3 Doubt, momentum gains reduced by 60%
   - Makes momentum-consuming cards strategically valuable at high Doubt

3. **Handle Card Persistence**
   - **Standard cards**: Remain in Spoken pile (conversation memory)
   - **Echo cards**: Return to hand if conditions met
   - **Persistent cards**: Stay in hand until forcibly removed
   - **Banished cards**: Removed from conversation entirely

4. **Calculate Card Draw**
   - Base: 3 cards
   - Connection State modifier: +0 to +2 (Disconnected to Trusting)
   - Cadence bonus: +1 if marked from negative Cadence
   - If Deck empty: No reshuffling (conversation history preserved)

5. **Refresh Initiative**
   - Initiative refreshes to Connection State base (3-6)
   - Does NOT accumulate - starts fresh each LISTEN

6. **Check Goal Card Activation**
   - Check momentum thresholds (Basic 8, Enhanced 12, Premium 16)
   - Activated goals become playable with 100% success
   - Goal selection immediately ends conversation

7. **Reset Turn-Based Effects**
   - Personality restrictions reset (like Proud's ascending order)
   - Temporary effects expire

### SPEAK Action

Complete sequence:
1. **Check Initiative Available**
   - Must have enough Initiative for card's Initiative cost
   - Alternative costs may apply (momentum spending, state conditions)

2. **Choose One Card** from hand
   - Consider Initiative cost and alternative payment options
   - Check if card is playable under current conditions

3. **Check Personality Restrictions**
   - **Proud**: Cards must be played in ascending Initiative order within turn
   - **Mercantile**: Highest Initiative card each turn gets +30% success
   - **Cunning**: Playing same Initiative as previous card costs -2 momentum
   - **Devoted**: Doubt increases by +1 additional on failure
   - **Steadfast**: All momentum changes capped at ±2

4. **Pay Card Cost**
   - Spend Initiative equal to card cost
   - Or pay alternative cost (momentum, state conditions)
   - Some cards generate Initiative as part of their effect

5. **Increase Cadence**
   - +1 Cadence (you're speaking)
   - Tracks conversation balance for later LISTEN effects

6. **Calculate Success Chance**
   - Base card difficulty % + (2% × current momentum)
   - Apply personality modifiers
   - Apply stat level bonus (+10% per stat level)
   - Apply doubt tax (reduced momentum affects success)

7. **Resolve Success/Failure**
   - **Success**: Apply card effects, may continue speaking
   - **Failure**: +1 Doubt, apply failure effects (typically ForceListen)

8. **Card to Appropriate Pile**
   - **Standard**: Goes to Spoken pile permanently
   - **Echo**: Returns to hand if conditions met
   - **Persistent**: Stays in hand
   - **Banish**: Removed entirely

9. **Apply Card Effects**
   - Momentum changes (reduced by doubt tax)
   - Initiative generation/spending
   - Resource conversions
   - Scaling effects based on visible state

10. **Check Conversation End Conditions**
    - Doubt ≥ 10: Conversation failure
    - Goal card played: Conversation success
    - Special end conditions triggered

11. **Continue or Force LISTEN**
    - If Initiative remains: Can play another card
    - If ForceListen effect: Must LISTEN next
    - If no Initiative: Can choose to LISTEN or wait

### Promise Cards - Queue Manipulation

Promise cards are special conversation cards bundled with requests that manipulate the obligation queue mid-conversation:

**Mechanics**:
- Play like any other conversation card (costs Initiative, has difficulty)
- **Success Effect**: Target obligation immediately moves to position 1
- **Queue Impact**: All displaced obligations shift down
- **Token Burning**: Automatically burn tokens with ALL displaced NPCs
- **Momentum Bonus**: Gain +5 to +10 momentum immediately
- **Narrative**: "I'll prioritize your request" - literally and mechanically
- **High Cost**: Usually cost 5-8 Initiative (require significant setup)

**Strategic Use**:
- Enables reaching higher goal thresholds otherwise impossible
- Trades future relationships for immediate success
- Creates visible sacrifice that justifies higher rewards
- Transforms conversations from probability to economics

### Multi-Threshold Goal System

Each Request bundles multiple goal cards with different momentum requirements:

#### Goal Ladder Example
- **Basic Goal** (8 momentum): Standard delivery, 1 token reward
- **Enhanced Goal** (12 momentum): Priority delivery, 2 tokens + observation
- **Premium Goal** (16 momentum): Immediate action, 3 tokens + permit

#### Strategic Decision Tree
1. **Safe Exit**: Accept basic goal once achievable
2. **Natural Building**: Risk doubt accumulation for higher thresholds
3. **Promise Sacrifice**: Burn relationships to guarantee premium

#### Cascading Consequences
- Accept basic = preserve queue relationships, avoid doubt accumulation
- Build naturally = risk doubt accumulation that reduces momentum on LISTEN
- Use promise = damage multiple relationships for one success, but guarantee goal achievement

The tension: You're not asking "can I succeed?" but "how much doubt can I accumulate before momentum gets consumed by LISTEN actions?"

### Conversation End

Triggers:
- Doubt too high (conversation failure)
- Flow at -3 in Disconnected state
- Request card accepted/failed
- Player chooses to leave

Cleanup:
- All piles cleared
- NPC persistent decks unchanged (except consumed observations)
- Momentum resets to 0
- Connection state persists
- Time cost: Base segment + (doubt accumulated × complexity factor)

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

All success rates modified by: +2% per momentum point

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

**Level 1**: +10% success rate, 1 XP per card, typical: street vendors
**Level 2**: Normal success rates, 2 XP per card, typical: named NPCs
**Level 3**: -10% success rate, 3 XP per card, typical: nobles, scholars

### Stranger Encounter System

Strangers are unnamed NPCs that provide resource generation and stat grinding opportunities without building relationships.

#### Stranger Properties
- **No Token Generation** - Only named NPCs build relationships
- **No Signature Cards** - Use only conversation type deck
- **No Observation Deck** - Cannot receive observations
- **No Burden Accumulation** - One-time interactions
- **Standard Personality Rules** - Apply normally
- **Single Availability** - Can converse once per time block

#### Encounter Levels

**Level 1 Strangers**:
- Examples: Street vendors, pilgrims, workers
- +10% success rate on all cards
- 1 XP per card played
- Lower momentum thresholds

**Level 2 Strangers**:
- Examples: Traveling merchants, guards, clerks
- Normal success rates
- 2 XP per card played
- Standard momentum thresholds

**Level 3 Strangers**:
- Examples: Visiting nobles, foreign scholars
- -10% success rate on all cards
- 3 XP per card played
- Higher momentum thresholds

#### Conversation Rewards

**Friendly Chat** (8/12/16 momentum):
- Basic: 2 coins
- Enhanced: 4 coins + bread (weight 1)
- Premium: 6 coins + medicine (weight 1)

**Trade Negotiation** (8/12/16 momentum):
- Basic: Trade goods (weight 2, value 5 coins)
- Enhanced: Trade goods (weight 2, value 8 coins)
- Premium: Trade goods (weight 3, value 12 coins)

**Information Gathering** (8/12/16 momentum):
- Basic: Reveals one face-down path
- Enhanced: Reveals two paths + marks observation
- Premium: Reveals all paths on entire route

**Seek Favor** (8/12/16 momentum):
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

## Conversation Card Distribution

### Strategic Card Examples by Tier

Each conversation type contains cards distributed across strategic categories:

**TIER 1 - Efficient Foundation (1-2 Focus):**
- "Gentle Agreement" - Focus 1, Strike, +1 momentum (Generator)
- "Quick Insight" - Focus 1, Threading, Draw 1 card (Utility)
- "Pause Reflect" - Focus 1, Soothe, -1 doubt (Converter)
- "Build Momentum" - Focus 2, Strike, +2 momentum (Generator)

**TIER 2 - Strategic Core (3-4 Focus):**
- "Passionate Plea" - Focus 3, Strike, +3 momentum (Generator)
- "Clear Confusion" - Focus 3, Soothe, Spend 2 momentum → -3 doubt (Converter)
- "Establish Trust" - Focus 3, Advancing, Spend 3 momentum → +1 flow (Converter)
- "Racing Thoughts" - Focus 3, Threading, Draw 2 cards (Utility)

**TIER 3 - High Impact (5-6 Focus):**
- "Burning Conviction" - Focus 5, Strike, +5 momentum (Generator)
- "Moment of Truth" - Focus 5, Advancing, Spend 4 momentum → +2 flow (Converter)
- "Deep Understanding" - Focus 6, Strike, Momentum = cards in hand (Investment)

**SCALING/INVESTMENT CARDS:**
- "Show Understanding" - Focus 3, Strike, Momentum based on cards in hand (Investment)
- "Build Pressure" - Focus 4, Strike, Momentum based on current doubt (Investment)
- "Mental Reset" - Focus 0, Focusing, +2 focus this turn only (Utility)
- "Desperate Gambit" - Focus 2, Strike, Momentum scales with doubt level (Investment)

### Resource Conversion Examples

**Direct Converters** - Transform momentum immediately:
- Clear Confusion: 2 momentum → 3 doubt reduction
- Establish Trust: 3 momentum → 1 flow advancement
- Moment of Truth: 4 momentum → 2 flow advancement

**Investment Scaling** - Power varies by game state:
- Show Understanding: More cards in hand = more momentum
- Build Pressure: Higher doubt = more momentum generated
- Desperate Gambit: Doubt becomes strength through desperation

**Focus Manipulation** - Enable tier 3 plays:
- Mental Reset: Temporary focus boost for big plays
- Efficient cards preserve focus for powerful effects
- Planning required for focus-intensive combinations

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

### Stat Development Through Play

**From Named NPCs**: Conversations grant XP to stats based on cards played
**From Strangers**: Higher XP rates (×2 or ×3) for stat grinding
**From Observations**: Can unlock new conversation types or contexts
**Stat Properties**: Every stat affects all cards bound to it uniformly

### Stat Specialization

Players naturally specialize through play patterns:
- Frequently used stats gain more XP
- Higher stat levels make those approaches more reliable
- Creates character differentiation through methodology preference
- All stats remain viable but specialization provides advantages

### NPC Signature Cards

Each NPC has 5 unique cards that mix into the conversation based on tokens:

#### Example: Marcus's Commerce Cards
- **1 token**: "Marcus's Momentum" - Focus 2, Hard, Thought/Momentum/None/None
- **3 tokens**: "Trade Knowledge" - Focus 3, Easy, Thought/Threading/None/None
- **6 tokens**: "Commercial Bond" - Focus 1, Very Hard, Thought/Momentum/Backfire/None
- **10 tokens**: "Marcus's Favor" - Focus 4, Hard, Thought/Momentum/None/None (special: None failure effect without ForceListen fallback)
- **15 tokens**: "Master Trader" - Focus 5, Medium, Thought/Atmospheric-Focused/None/None

These cards represent the mechanical expression of each relationship, making every NPC conversation unique even with the same conversation type.

### Strategic Stat Development

**Personality Optimization**: Develop stats for specific NPC types
- **Proud Focus**: Authority and Commerce work well with ascending requirements
- **Devoted Safety**: Rapport minimizes negative effects from additional doubt losses
- **Mercantile Power**: Commerce naturally has high-focus cards for +30% bonus
- **Cunning Variety**: Insight and Cunning provide diverse approaches
- **Steadfast Consistency**: Rapport cards provide steady momentum gains within ±2 cap

**Token Strategy**: Which relationships to prioritize
- Commerce tokens for Marcus improve starting state for trade conversations
- Trust tokens for emotional NPCs start intimate conversations in better states
- Status tokens for proud NPCs begin authority challenges more favorably
- Shadow tokens for cunning NPCs enable information gathering from connected state

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
- Benefit: +5-10 momentum, reach premium goals
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

### Strategic Cards → Success → Better Conversations
```
Successful card plays → XP for connected Stat
Stat levels up → All bound cards gain benefits (success rate, persistence, etc.)
Request completion → Stat XP and progression
Higher stats → Better performance with card categories
Stronger strategic play → Handle complex personality rules
Harder NPCs → Better rewards → Access to more powerful strategic contexts
```

### Momentum Management → Goal Achievement
```
Generate momentum → Build toward goal thresholds
Convert momentum → Reduce doubt, advance flow, draw cards
Manage focus → Enable tier 3 high-impact plays
Strategic decisions → Generator vs converter vs investment cards
Resource optimization → Multiple paths to success
Completed goals → Token rewards and relationship advancement
```

### Tokens → Signature Cards → Easier Conversations
```
Successful deliveries → +1-3 tokens
Higher tokens → Unlock signature cards
Signature cards → Mixed into conversation
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

## Economic Balance Points

### Momentum Economy Rebalancing

**Previous System Issues**:
- 2-4 momentum per card average created resource inflation
- Goals at 10/15/20 became trivial with high generation
- No resource tension or strategic decisions
- Focus generosity allowed playing entire hand most turns

**New Strategic Framework**:
- **Momentum Generation**: 1-3 momentum per card average
- **Goal Thresholds**: Basic 8, Enhanced 12, Premium 16
- **Resource Conversion**: Momentum spending for doubt, flow, cards
- **Focus Constraints**: Tier system creates meaningful limitations

### Strategic Decision Creation

**Every Turn Forces Choices**:
- Generator vs Converter vs Investment cards
- Tier 1 efficiency vs Tier 3 power
- Momentum banking vs immediate conversion
- Focus allocation across power tiers

**Multiple Victory Paths**:
- Pure momentum accumulation (generator focus)
- Doubt management (converter focus)
- Flow advancement (investment focus)
- Card advantage (utility focus)

**Economic Pressure Points**:
- Focus constraints prevent "play everything" strategies
- Momentum conversion prevents pure hoarding
- Goal thresholds require planning across multiple turns
- Power tiers force resource allocation decisions

### Focus → Strategic Choices → Progress
```
Higher states → More focus capacity
Focus allocation → Choose between tier 1/2/3 strategic options
Successful plays → Momentum generation and goal progression
Resource conversion → Momentum to doubt reduction, flow, cards
ForceListen effect → Topic change, strategic reset
Better connection states → More focus capacity and card draws
Strategic mastery → Optimal focus usage patterns
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
- No focus, momentum, or conversation mechanics involved

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
- **No conversation mechanics** (no focus, momentum, flow, doubt)
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
- These permanently add new exchange options to the NPC's exchange deck

### Exchange vs Conversation

**Exchanges ARE**:
- Separate card system with own UI
- Simple resource trades
- Quick (0 segments)
- No social mechanics
- Deterministic outcomes

**Exchanges ARE NOT**:
- Part of conversations
- Subject to focus/momentum/flow
- Affected by connection states
- Using doubt mechanics

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
- **Personality type**: Determines doubt tolerance, token preference, and conversation rule
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

The core gameplay loop is **conversations as strategic resource management**:

### Stats as Character Development

Your stats represent your conversational competencies. Every successful approach reinforces that methodology. Higher stat levels make all cards of that type more reliable. This isn't abstract character growth - it's developing actual conversational skills through practice.

### Conversations as Strategic Puzzles

The four-tier strategic framework (Generators, Converters, Investments, Utility) means cards represent strategic approaches rather than simple effects. Each conversation becomes a resource management puzzle where you balance momentum generation, resource conversion, and focus allocation.

Each conversation is a tactical puzzle where you play strategic cards against NPC personality rules:
- **Proud NPCs** force ascending focus order - rewards tier planning
- **Devoted NPCs** double momentum losses - punishes failed conversions
- **Mercantile NPCs** reward big plays - synergizes with tier 3 power
- **Cunning NPCs** punish patterns - requires strategic variety
- **Steadfast NPCs** cap momentum changes - favors consistent generators

### Strategic Resource Framework

The power tier structure creates meaningful constraints:
- **Tier 1 (1-2 focus)**: Efficient foundation, always affordable
- **Tier 2 (3-4 focus)**: Strategic core, requires planning
- **Tier 3 (5-6 focus)**: High impact, demands setup

Resource conversion adds strategic depth:
- **Momentum → Doubt reduction**: Spend momentum to reduce doubt accumulation
- **Momentum → Flow advancement**: Convert momentum to progress connection state
- **Momentum → Card draw**: Trade momentum for additional options
- **Doubt mechanics**: High doubt makes momentum-consuming cards "essentially free" since LISTEN would consume momentum anyway
- Creates constant decisions between accumulation, conversion, and managing doubt levels

### Strategic Planning Creates Engagement

The power tier system requires forward thinking. Tier 3 cards demand focus manipulation or multi-turn setup. Resource conversion creates immediate vs long-term tension. Focus constraints prevent autopilot play. Every turn requires evaluating multiple strategic paths based on current resources and game state.

### Failure Creates Strategic Resets

When you fail a card play, the failure effect applies (typically ForceListen), forcing you to LISTEN and drawing new cards. This isn't punishment - it's strategic reset. Success maintains your current strategic momentum. Failure effects force strategic adaptation. The mechanical flow mirrors both dialogue rhythm and tactical decision-making.

### Weight Creates Physical Reality

The 10-weight capacity isn't arbitrary limitation - it's physical reality. Heavy packages prevent accepting other obligations. Tools enable paths but consume capacity permanently. Food takes space until consumed. Every choice about what to carry affects what paths you can take, what obligations you can accept, and what opportunities you can pursue.

### Time Segments Create Natural Rhythm

The 24-segment day organized into 6 blocks creates natural activity patterns. Morning investigations are twice as efficient. Work consumes entire blocks. Travel costs segments that could enable conversations. Every action has opportunity cost measured in concrete time.

### Relationships as Equipment

NPC signature cards unlocked by tokens are like equipment in other RPGs. Marcus's commerce cards are your "merchant armor." Elena's trust cards are your "emotional weapons." Each relationship provides unique tools that fundamentally change how conversations play.

### Clear Strategic Progression Path

The loop is crystal clear:
1. **Have conversations** using strategic card framework
2. **Master resource management** across generators, converters, investments, utility
3. **Complete requests** to gain XP and progress stats
4. **Level up stats** to improve all bound cards uniformly
5. **Unlock signature cards** through token relationships
6. **Handle complex personalities** with advanced strategic thinking
7. **Face tougher strategic challenges** with multi-tier planning

This is "Fight enemies → Gain XP → Level up → Fight stronger enemies" expressed through social dynamics and stat progression rather than combat statistics.

The strategic card framework transforms character progression from simple power increases to mastering resource management methodologies. Players literally become better strategic thinkers by practicing different approaches - momentum generation, resource conversion, strategic investment, and focus optimization. These developed competencies then unlock investigation methods and travel paths, making strategic conversation mastery the engine for all progression. Stranger encounters provide the "grinding" layer expected from RPGs while maintaining focus on meaningful strategic relationships with named NPCs.

## Critical Formulas Reference

### New Conversation System Formulas

**Initiative Capacity by Connection State**:
- Disconnected: 3 Initiative
- Guarded: 4 Initiative
- Neutral: 5 Initiative
- Receptive: 5 Initiative
- Trusting: 6 Initiative

**Card Success Rate**: Base% + (2% × Current Momentum) + (10% × Bound Stat Level)

**Doubt Tax on Momentum**: Momentum gains reduced by 20% per Doubt point
- Example: At 3 Doubt, momentum gains reduced by 60%

**Cadence System (-5 to +10)**:
- +1 per card played (you talking)
- -3 per LISTEN action (giving them space)
- Cadence ≥ 6: +1 Doubt per point above 5 on LISTEN
- Cadence ≤ -3: +1 card draw on LISTEN

**Cards Drawn on LISTEN**:
- Base: 3 cards
- Connection State: +0 to +2 (Disconnected to Trusting)
- Cadence bonus: +1 if negative Cadence achieved previous turn

**Stat-Gated Depth Access**:
- Your stat level = maximum depth accessible for that stat's cards
- Example: Commerce Level 4 = Commerce cards depth 1-4 accessible

**Extended Stat Level Thresholds**:
- Level 1→2: 10 XP
- Level 2→3: 25 XP
- Level 3→4: 50 XP
- Level 4→5: 100 XP
- Level 5→6: 175 XP
- Level 6→7: 275 XP
- Level 7→8: 400 XP

**Stat Success Bonuses**:
- +10% success rate per stat level (cumulative)
- Level 3+: Unlock advanced world approaches
- Level 5+: Immune to ForceListen failure effect
- Level 7+: Access to master conversation cards (depth 7-8)

**Card Initiative Costs by Depth**:
- Depth 1-3: 0-2 Initiative (Foundation/Builder cards)
- Depth 4-6: 3-5 Initiative (Standard/Balanced cards)
- Depth 7-10: 6-12 Initiative (Decisive/Spender cards)

**Momentum Goal Thresholds**:
- Basic Goal: 8 momentum
- Enhanced Goal: 12 momentum
- Premium Goal: 16 momentum

**Conversation Difficulty Effects**:
- Level 1: +10% success rate, 1× XP multiplier
- Level 2: Normal success rates, 2× XP multiplier
- Level 3: -10% success rate, 3× XP multiplier

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

**Conversation Time**: 1 segment + (doubt accumulated × complexity factor)

**Hunger Increase**: +20 per time block

**Starvation**: At 100 hunger, lose 5 health per block

**Personality Rule Effects**:
- **Proud**: Cards must be played in ascending Initiative order each turn
- **Devoted**: Doubt increases by +1 additional on failure (double doubt)
- **Mercantile**: Highest Initiative card each turn gets +30% success bonus
- **Cunning**: Playing same Initiative as previous card costs -2 momentum
- **Steadfast**: All momentum changes capped at ±2 per card

**Alternative Card Costs**:
Many high-depth cards offer multiple payment options:
- "8 Initiative OR 4 Initiative + spend 3 Momentum"
- "7 Initiative OR 4 Initiative if Cadence ≥ 5"
- "6 Initiative OR 3 Initiative if Doubt ≥ 7"

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

The genius is that this uses contextual conversation decks to ensure verisimilitude, while NPC personality rules and signature cards ensure every conversation feels unique. The ForceListen failure effect creates natural conversation rhythm where success builds momentum and failure forces adaptation.

Players always know what to do: have conversations to gain XP, level up their stats, build relationships for better starting states, and tackle increasingly complex NPC personalities. The strategic depth emerges from how conversation types, stat levels, signature cards, personality rules, weight limits, and time constraints interact to create unique puzzles.

The system succeeds because mastery comes from understanding these interactions and building a deck that reflects your personal approach to social challenges while managing the physical realities of weight and time. Every conversation is practice. Every card gained is permanent progression. Every relationship built provides new tools. Every item carried has opportunity cost. This is character growth made tangible, strategic, and elegantly integrated with the narrative of becoming a better conversationalist and courier in a living world.