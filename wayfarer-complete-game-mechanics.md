# Wayfarer: Complete Game Mechanics (Refined)

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
10. [Conversation Card Architecture](#conversation-card-architecture)
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

### Intentional Mechanic Design

Examples of clean separation:

**BAD**: "Routes require access permit OR 10 coins"
**GOOD**: Routes require access permit. Guards can be bribed for permits. Merchants sell permits.

**BAD**: "High tokens unlock better cards AND improve success"  
**GOOD**: Tokens unlock NPC signature cards. Card levels improve success. Two separate mechanics.

**BAD**: "Hunger reduces doubt AND work output AND travel speed"
**GOOD**: Hunger reduces work output (coins = 5 - floor(hunger/25)). Hunger increases travel segments (+1 at 75+). Two separate formulas.

**BAD**: "Initiative affects multiple unrelated things"
**GOOD**: Initiative has one clear purpose - determining how many cards can be played in sequence.

**BAD**: "Investigation gives familiarity AND cards"
**GOOD**: Investigation gives familiarity. Observation gives cards (requires familiarity). Two separate actions.

## Conversation Type System

### Core Concept

Instead of players owning a personal conversation deck that levels up, each conversation TYPE has its own predefined deck. This ensures cards are always contextually appropriate - you won't have authority cards when comforting desperate Elena or momentum cards when confronting bandits. Cards feel like natural conversation options, not collectible battle cards.

### Standard Conversation Types

**Friendly Chat**: Casual, balanced mix of all stats
- Used for building relationships without pressure
- Contains cards from all five stats
- Lower depth cards predominate

**Desperate Request**: Heavy Rapport/Insight, zero Authority
- For NPCs in crisis needing help
- Empathetic builders and doubt-reducing cards
- Scaling cards that reward emotional investment

**Trade Negotiation**: Heavy Diplomacy/Insight, zero Rapport
- Business discussions and deals
- Momentum-efficient builders
- Cards for resource optimization

**Authority Challenge**: Heavy Authority/Cunning, zero Rapport
- Confrontations and power struggles
- High-initiative cost cards with powerful effects
- Risk-reward cards for power plays

**Information Gathering**: Heavy Insight/Cunning
- Extracting knowledge and secrets
- Card-draw effects and scaling investments
- Initiative manipulation for sustained questioning

**Intimate Confession**: Emotional depth, all stats present
- Deep personal conversations
- Full strategic spectrum with scaling rewards
- Requires established relationships for effectiveness

### Contextual Deck Usage

The same NPC uses different conversation decks based on context:
- Marcus at his shop: Trade Negotiation deck
- Marcus at the tavern: Friendly Chat deck
- Marcus when desperate: Desperate Request deck

Elena's progression:
- First meeting: Desperate Request (doubt management critical)
- After trust built: Intimate Confession (scaling investments available)
- At market: Friendly Chat (casual momentum building)

### Verisimilitude Through Context

This system prioritizes verisimilitude over mechanical flexibility. Conversations feel like real interactions where available responses match the context and strategic situation. You literally cannot access high-authority cards when someone needs emotional support because those cards aren't in that conversation type's deck.

Player expression comes through:
- Understanding each conversation type's card distribution
- Managing Initiative resources through builder/spender dynamics
- Building Statements in Spoken to enable powerful cards
- Choosing when to consume momentum for effects
- Developing stats that unlock deeper card depths

## Three Core Game Loops

### System Integration Philosophy

The three core game loops answer fundamental design questions while maintaining strict mechanical separation. Each loop creates problems that only the other loops can solve, forcing engagement with all systems.

### Core Loop 1: Card-Based Conversations

#### Design Questions Answered
- **What provides challenge?** Using conversation type cards effectively against NPC personality rules
- **Why grow stronger?** Higher stats unlock deeper card depths improving your options
- **Why engage with NPCs?** Request cards provide income, access, world progression, and stat XP

#### The Conversation as Core Activity

Conversations are the primary gameplay loop - your "combat encounters" expressed through social dynamics. Each conversation type provides a specific deck of cards appropriate to that social context. Your stats determine which depths of cards you can access, representing your growing competence with different conversational approaches.

The mechanical depth comes from the builder/spender dynamic where Foundation cards (0 Initiative) generate Initiative to enable Standard (3-5 Initiative) and Decisive cards (6-12 Initiative). Managing Initiative, building Statements in Spoken, and balancing Cadence creates tactical decisions every turn.

#### NPC Deck Systems

Each NPC maintains three persistent decks and a list of requests:

**Three Persistent Decks**:
1. **Signature Deck**: Unique cards that enhance conversations based on token count
2. **Observation Deck**: Cards from location discoveries relevant to this NPC
3. **Burden Deck**: Cards from failed obligations and damaged relationships

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

These cards are specific to that NPC, not generic token type cards. Marcus doesn't give "Diplomacy cards," he gives "Marcus's Bargain," "Silk Road Knowledge," and "Marcus's Favor." Elena gives "Elena's Trust," "Shared Burden," and "Elena's Hope." These cards mechanically represent the nature of your specific relationship and enhance the base conversation type deck.

#### Personality Rules

Each personality type applies one rule that fundamentally changes how conversations work:

- **Proud**: Cards must be played in ascending Initiative order each turn (resets when you LISTEN)
- **Devoted**: When card effects add Doubt, add +1 additional Doubt
- **Mercantile**: Your highest Initiative cost card each turn gains +3 Momentum bonus
- **Cunning**: Playing same Initiative cost as previous card costs -2 Momentum
- **Steadfast**: All Momentum changes are capped at ±2 per card

These rules represent how different personalities respond to conversation. A Proud person needs escalating respect. A Devoted person catastrophizes when things go wrong. A Merchant rewards getting to the point with bigger plays.

#### Conversation Outputs
- **Stat XP**: Each card play grants XP to its bound stat (1 XP base, multiplied by conversation difficulty)
- **Tokens**: Gained through successful letter delivery (+1 to +3)
- **Observations**: Cards added to specific NPCs' observation decks
- **Permits**: Special promises that enable routes
- **Burden Cards**: Failed requests damage relationships

### Core Loop 2: Obligation Queue Management

#### Design Questions Answered
- **Why travel between locations?** Obligations scattered across the city
- **Why revisit locations?** Building relationships for better signature cards
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
- Mercantile: Diplomacy tokens
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
- Can unlock exchanges or provide unique effects
- Some observations yield physical items instead of cards

#### Travel Encounters

Use conversation mechanics with special decks:
- **Bandits**: Violence deck, combat resolution (TBD)
- **Guards**: Inspection deck, authority check (TBD)
- **Merchants**: Road trade deck, exchange opening (TBD)

Success allows passage, failure costs resources.

## Travel System

### Core Concept

Travel uses persistent path cards that start face-down and flip permanently once discovered. Unlike conversations (deterministic resource management) or exchanges (resource trades), travel is about **discovery through exploration**. Each route segment presents 2-3 path choices showing only stamina cost until revealed.

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

**Diplomacy Paths**:
- "Merchant Caravan" - Requires Diplomacy 2+ OR 10 coins
- "Trade Route" - Requires Diplomacy 3+
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

Similar to Initiative base in conversations:

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
Permanent equipment providing persistent benefits at weight cost. A crowbar weighs 2, enables "Forced Entry" path cards. A merchant ledger weighs 1, provides +1 starting Diplomacy momentum. A rope weighs 2, enables "Cliff Descent" paths. Tools never leave your satchel unless dropped, creating long-term capacity decisions.

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

### Initiative Economy Integration

**Initiative Generation Through Builder/Spender**:
- **Foundation Cards** (Depth 1-2, 0 Initiative cost): Generate 1-3 Initiative
- **Standard Cards** (Depth 3-6, 3-5 Initiative cost): Core effects, no generation
- **Decisive Cards** (Depth 7-10, 6-12 Initiative cost): Powerful effects requiring setup
- **Base Initiative**: Starts at 3-6 based on stat levels, refreshes to base on LISTEN

**Resource Identity Separation**:
- **Initiative**: Action economy for playing cards
- **Momentum**: Progress toward conversation goals (8/12/16 thresholds)
- **Doubt**: Timer (ends at 10) and conversation pressure
- **Cadence**: Conversation balance affecting LISTEN (-5 to +10)
- **Statements**: Cards in Spoken pile that scale effects

### Time Segment Economy Connections

**Daily Allocation**: 24 segments (6 blocks × 4 segments each)

Time segments enable:
- **Conversations** (1 segment + doubt depth): Access to letters and tokens
- **Investigations** (1 segment): Build location familiarity
- **Observations** (1 segment): Discover cards or items for NPCs
- **Work** (4 segments/full block): Coins but time cost, scaled by hunger
- **Quick Exchange** (0 segments): Simple diplomacy without conversation
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
- Initiative management affects ability to reach request goals
- Low stat levels limit card depth access
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
- **Diplomacy**: Professional dealings (Mercantile NPCs prefer)
- **Status**: Social standing (Proud NPCs prefer)
- **Shadow**: Shared secrets (Cunning NPCs prefer)

**Primary Effect**: More signature cards mixed into conversation
- 0 tokens: No signature cards
- 1-2 tokens: 1 signature card
- 3-5 tokens: 2 signature cards
- 6-9 tokens: 3 signature cards
- 10-14 tokens: 4 signature cards
- 15+ tokens: 5 signature cards

**Additional Uses Through Different Mechanics**:
1. **Signature Cards**: Higher tokens mean more unique NPC cards
2. **Displacement Cost**: Burn tokens to jump queue positions
3. **Exchange Gating**: Minimum tokens required for special exchanges

**Generation**:
- Standard delivery: +1 token with recipient
- Failed delivery: -2 tokens with sender
- Special events and quests

**Token Investment Benefits**:
- More signature cards mixed into conversation
- Access to token-gated exchanges
- Flexibility for queue displacement when needed

### Per-Conversation Resources

#### Initiative
- **Starting Value**: 0 (must be generated from nothing)
- **Mechanics**:
  - Accumulates and persists between LISTEN actions
  - Each card costs its Initiative value on SPEAK
  - Foundation cards generate Initiative
  - Can bank Initiative for future turns
  - No maximum cap - can accumulate indefinitely
- **Strategic Role**: Core action economy. Must generate through Foundation cards before any other cards become playable.

#### Momentum
- **Range**: 0 to 20+
- **Starting Value**: 0 (built through conversation)
- **Goal Thresholds**: Basic (8), Enhanced (12), Premium (16)
- **Effect**: Progress toward request goals
- **Consumption**: Can be consumed by card costs for powerful effects
- **Resets**: After conversation ends
- **Strategic Role**: Victory condition and consumable resource

#### Cadence
- **Range**: -5 to +5
- **Always starts at 0**
- **Changes**: 
  - Each SPEAK action: +1 Cadence
  - Each LISTEN action: -2 Cadence
- **Effects**:
  - Positive Cadence: +1 Doubt per point on LISTEN
  - Negative Cadence: +1 card draw per point on LISTEN
  - At ±5 cannot go further in that direction
- **Strategic Role**: Conversation balance forcing strategic listening. Dominating creates Doubt, listening grants cards.

#### Doubt
- **Range**: 0 to 10
- **Starting Value**: 0
- **Increase**: Through Cadence effects on LISTEN or card effects
- **Effect**: Conversation ends at 10 Doubt
- **Reduction**: Through card effects that consume momentum
- **Strategic Role**: Timer creating urgency and punishing conversation dominance

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

Conversations cost 1 base segment plus Statements in Spoken as time depth. Each Statement card played adds to the conversation's substance and duration. Echo cards don't increase time since they return to the Deck. This creates tension between thorough conversations and time efficiency.

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

**Purchase Information** (Diplomacy 2+):
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

The conversation system represents the primary gameplay loop using Initiative-based builder/spender dynamics inspired by Steamworld Quest. Players must generate Initiative through Foundation cards to enable Standard and Decisive cards while managing five distinct resources. Each conversation type provides a fixed deck appropriate to the context, with player stats determining which depths are accessible.

### The Five Core Resources

**IMPORTANT REFINEMENT**: Resources use a **Specialist with Universal Access** model, NOT hard exclusivity. Each stat SPECIALIZES in one resource (2-3x efficiency) but can ACCESS other universal resources at weaker rates.

#### Universal Resources (All Stats Can Generate)

**Initiative** - Conversational Action Economy:
- Determines how many cards can be played in sequence
- Starts at 0 (must be built from nothing)
- Accumulates and persists between LISTEN actions
- **Cunning specializes** (2-3x generation), but ALL stats provide some Initiative
- Creates builder/spender dynamic where you must generate before spending

**Momentum** - Progress Toward Goals:
- Victory track toward conversation objectives
- Goal thresholds: Basic (8), Enhanced (12), Premium (16)
- Can be consumed by card costs for powerful effects
- **Authority specializes** (2-3x generation), but ALL stats provide some Momentum
- CRITICAL: Every conversation deck MUST generate Momentum to reach goals
- Verisimilitude: In real conversations, all approaches advance progress, not just commands

#### Specialist Resources (Only Specialists Generate Efficiently)

**Cards** (Insight Specialty):
- Card draw to maintain options
- Insight generates +2-6 cards efficiently
- Others might get +1 card occasionally as secondary effect

**Cadence** (Rapport Specialty):
- Conversation Balance (-5 to +5)
- +1 per SPEAK action, -2 per LISTEN action
- Positive Cadence: +1 Doubt per point on LISTEN
- Negative Cadence: +1 card draw per point on LISTEN
- Rapport specializes in Cadence manipulation
- Others rarely touch Cadence directly

**Doubt** (Diplomacy Specialty):
- Timer that ends conversation at 10 points
- Increases through Cadence effects or card effects
- Diplomacy specializes in Doubt reduction
- Others rarely reduce Doubt (except by avoiding Cadence buildup)

**Statements in Spoken** - Conversation History:
- Count of Statement cards played (not Echo cards)
- Used to scale card effects ("per Statement")
- Enables requirements for powerful cards
- Determines conversation time cost (1 segment + Statements)
- Represents accumulated conversation foundation

### Stat-Gated Depth System

**Card Depth Architecture**:
Each conversation type deck contains cards organized by depth (1-10):
- **Depth 1-2**: Foundation cards (0 Initiative cost, generate Initiative)
- **Depth 3-4**: Standard cards (0-4 Initiative cost)
- **Depth 5-6**: Advanced cards (2-6 Initiative cost)
- **Depth 7-8**: Powerful cards (4-8 Initiative cost)
- **Depth 9-10**: Master cards (7-12 Initiative cost)

**Depth Access Mechanics**:
Your stat level determines maximum accessible depth for that stat's cards:
- **Rapport Level 3**: Can access Rapport cards up to Depth 3
- **Diplomacy Level 5**: Can access Diplomacy cards up to Depth 5
- **Authority Level 2**: Can access Authority cards up to Depth 2

**Progressive Deck Composition**:
- Novice players (stats 1-2): Small decks with Foundation cards only
- Experienced players (stats 3-5): Medium decks with Standard options
- Advanced players (stats 6-8): Large decks with Advanced and Powerful cards
- Master players (stats 9-10): Full access including Master cards

### Card Architecture

Every card has exactly:
1. **Initiative Cost** (0 for depth 1-2, higher for deeper cards)
2. **Either** a Requirement **OR** a Cost (never both):
   - **Requirement**: Condition to play (e.g., "Requires 3+ Statements in Spoken")
   - **Cost**: Consume resource (e.g., "Consume 3 Momentum")
3. **One Deterministic Effect**:
   - Flat increase ("+3 Momentum")
   - Scaled increase ("+1 Momentum per 2 Statements in Spoken")
   - Flat decrease ("-2 Doubt")

No branches, no choices, no "or" conditions. Complete determinism and perfect information.

#### Example Card Structures

**Depth 1 Foundation**
- Initiative: 0
- Requirement: None
- Effect: +2 Initiative

**Depth 4 Standard**
- Initiative: 3
- Requirement: 5+ Statements in Spoken
- Effect: +6 Momentum

**Depth 7 Decisive**
- Initiative: 8
- Cost: Consume 4 Momentum
- Effect: -5 Doubt

#### The Five Stats as Specialists with Universal Access

**Specialist Framework Pattern:**
- Primary resource at 2-3x specialist efficiency
- Universal resources (Momentum/Initiative) at 1x universal rate
- Secondary resources occasionally as compound effects

**Authority** (Momentum Specialist):
- **Specialist**: +2-12 Momentum (scales 2-3x faster than others)
- **Universal**: +1-2 Initiative (same rate as others)
- **Trade-off**: Generates Doubt (+1-4) for higher momentum
- Foundation Example: "+2 Momentum, +1 Doubt" (0 Initiative)
- Standard Example: "+5 Momentum, +2 Doubt, +1 Initiative" (2 Initiative cost)

**Insight** (Cards Specialist):
- **Specialist**: Draw 2-6 cards (efficient card generation)
- **Universal**: +1-3 Momentum (slower than Authority)
- **Secondary**: +1-2 Initiative occasionally
- Foundation Example: "Draw 2 cards, +1 Momentum" (0 Initiative)
- Standard Example: "Draw 3 cards, +2 Momentum, +1 Initiative" (3 Initiative cost)

**Rapport** (Cadence Specialist):
- **Specialist**: Reduce Cadence 1-3 (prevents Doubt accumulation)
- **Universal**: +1-3 Momentum, +1-3 Initiative
- **Trade-off**: Lower momentum generation but sustainable
- Foundation Example: "-1 Cadence, +1 Momentum, +1 Initiative" (0 Initiative)
- Standard Example: "-2 Cadence, +2 Momentum, +2 Initiative" (2 Initiative cost)

**Diplomacy** (Doubt Specialist):
- **Specialist**: -1 to -6 Doubt (crisis management)
- **Universal**: +1-3 Momentum
- **Trade-off**: Often consumes Momentum to reduce Doubt
- Foundation Example: "-1 Doubt, +1 Momentum" (0 Initiative)
- Standard Example: "-2 Doubt, +2 Momentum, Consume 2 Momentum" (2 Initiative cost)

**Cunning** (Initiative Specialist):
- **Specialist**: +2-6 Initiative (enables longer action chains)
- **Universal**: +1-3 Momentum (same rate as Rapport/Diplomacy)
- **Secondary**: Draw 1-2 cards occasionally
- Foundation Example: "+2 Initiative, +1 Momentum" (0 Initiative)
- Standard Example: "+4 Initiative, +2 Momentum, Draw 1 card" (2 Initiative cost)

**KEY INSIGHT**: Every stat generates SOME Momentum because all conversations must progress toward goals. Authority just does it 2-3x more efficiently (and with Doubt costs).

#### Card Persistence Types

**Statement**: Standard behavior - played once, goes to Spoken pile
**Echo**: Returns to Deck after playing (recurring themes that resurface)

Most cards are Statements that become permanent conversation history. Echo cards represent ideas you can return to throughout the conversation.

### Three-Pile System

**Revolutionary Pile Mechanics**:
1. **Deck**: Undrawn cards (filtered by stat depth access)
2. **Mind**: Current hand (your current thoughts)
3. **Spoken**: Used Statement cards (conversation history)

**No Automatic Reshuffling**: Once spoken, Statement cards stay in Spoken pile. Echo cards return to Deck. This represents that you can't unsay things or repeat the same arguments endlessly.

### Player Stats System

Stats determine your conversational competencies and unlock deeper card options:

#### The Five Stats and Their Depth Access

**Insight** - Analytical thinking and observation
- **Card Identity**: Information gathering, pattern recognition, drawing cards
- **Depth Access**: Your Insight level = maximum depth for Insight cards
- **World Effects**: Unlocks systematic investigation approaches

**Rapport** - Empathetic connection and support
- **Card Identity**: Initiative generation, doubt reduction, trust building
- **Depth Access**: Your Rapport level = maximum depth for Rapport cards
- **World Effects**: Unlocks social investigation through locals

**Authority** - Leadership and commanding presence
- **Card Identity**: High-cost powerful effects, direct impact
- **Depth Access**: Your Authority level = maximum depth for Authority cards
- **World Effects**: Unlocks demanding access to restricted areas

**Diplomacy** - Negotiation and trade thinking
- **Card Identity**: Resource conversion, efficient exchanges
- **Depth Access**: Your Diplomacy level = maximum depth for Diplomacy cards
- **World Effects**: Unlocks purchasing information directly

**Cunning** - Subtlety and indirect approach
- **Card Identity**: State-based effects, exploiting conditions
- **Depth Access**: Your Cunning level = maximum depth for Cunning cards
- **World Effects**: Unlocks covert investigation without alerting NPCs

#### Stat Progression

**XP Requirements**:
- Level 1→2: 10 XP
- Level 2→3: 25 XP
- Level 3→4: 50 XP
- Level 4→5: 100 XP
- Level 5→6: 175 XP
- Level 6→7: 275 XP
- Level 7→8: 400 XP
- Level 8→9: 550 XP
- Level 9→10: 750 XP

**Stat Level Effects**:
- **All Levels**: Unlock deeper card depths
- **Level 3+**: Unlock advanced investigation approaches
- **Level 5+**: Higher Initiative base (4 instead of 3)
- **Level 7+**: Access to Master cards (depth 7-8)

**XP Sources**:
- Playing any card: 1 XP to bound stat × conversation difficulty
- Conversation difficulty levels:
  - Level 1: 1 XP per card
  - Level 2: 2 XP per card  
  - Level 3: 3 XP per card

### NPC Architecture

Each NPC maintains three persistent decks plus a request system:

**Persistent Decks**:
1. **Signature Deck**: Unique cards mixed based on token count
   - Not drawn at start - shuffled into conversation deck
   - Represents the specific relationship with this NPC
   - 5 cards total, more added as tokens increase
   
2. **Observation Deck**: Cards from location discoveries
   - Receives cards from location observations
   - Cards automatically mixed into draw pile at conversation start
   - Provide unique advantages
   - Consumed when played
   
3. **Burden Deck**: Cards from failed obligations
   - Contains burden cards from relationship damage
   - Enables "Make Amends" conversation type
   - Each burden card makes resolution harder

**Request System**:
NPCs have a list of Requests (not a deck). Each Request bundles:
- **Request Cards**: Different goal thresholds (basic, enhanced, premium)
- **Promise Cards**: Can manipulate queue position
- **Status Tracking**: Available, completed, or failed

### LISTEN Action

Complete sequence:
1. **Apply Cadence Effects**
   - If Cadence > 0: +1 Doubt per positive point
   - If Cadence < 0: +1 card draw per negative point
   - Cadence decreases by 2 (giving them space)

2. **Calculate Card Draw**
   - Base: 3 cards
   - Plus bonus cards from negative Cadence
   - Draw from Deck

3. **Initiative Persists**
   - Initiative does NOT reset
   - Accumulated Initiative carries forward

4. **Check Goal Activation**
   - If Momentum ≥ goal threshold, can play request card next turn

### SPEAK Action

Complete sequence:
1. **Increase Cadence by +1** (you're speaking)

2. **Choose One Card** from Mind

3. **Check Initiative Cost**
   - Must have enough Initiative

4. **Check Personality Restrictions**
   - **Proud**: Cards must be in ascending Initiative order
   - **Mercantile**: Highest Initiative card gets +3 Momentum
   - **Cunning**: Same Initiative as previous costs -2 Momentum
   - **Devoted**: Card effects that add Doubt add +1 additional Doubt
   - **Steadfast**: Momentum changes capped at ±2

5. **Pay Initiative Cost**

6. **Check Requirement or Pay Cost**
   - If Requirement not met: Card cannot be played
   - If Cost: Consume the resource

7. **Apply Deterministic Effect**

8. **Card to Appropriate Pile**
   - **Statement**: Goes to Spoken pile
   - **Echo**: Returns to Deck

9. **Continue or Must LISTEN**
   - Can SPEAK again if Initiative remains
   - Or choose to LISTEN

### Promise Cards - Queue Manipulation

Promise cards manipulate the obligation queue mid-conversation:

**Mechanics**:
- Play like any conversation card (costs Initiative)
- Target obligation immediately moves to position 1
- All displaced obligations shift down
- Automatically burn tokens with displaced NPCs
- Gain +5 to +10 Momentum immediately

### Multi-Threshold Goal System

Each Request bundles multiple goal cards:

#### Goal Ladder Example
- **Basic Goal** (8 momentum): Standard delivery, 1 token
- **Enhanced Goal** (12 momentum): Priority delivery, 2 tokens
- **Premium Goal** (16 momentum): Immediate action, 3 tokens

### Conversation End

Triggers:
- Doubt reaches 10
- Request card played (goal achieved)
- Player chooses to leave

Cleanup:
- All piles cleared
- NPC persistent decks unchanged
- Time cost: 1 segment + total Statements in Spoken pile

### Conversation Difficulty Levels

**Level 1**: Street vendors, simple NPCs
- +1 XP per card played

**Level 2**: Named NPCs, standard difficulty  
- +2 XP per card played

**Level 3**: Nobles, scholars, complex NPCs
- +3 XP per card played

### Stranger Encounter System

Strangers are unnamed NPCs providing resources without building relationships:

#### Stranger Properties
- **No Token Generation**
- **No Signature Cards**
- **No Observation Deck**
- **No Burden Accumulation**
- **Standard Personality Rules Apply**
- **Single Availability Per Time Block**

#### Conversation Rewards

Based on momentum thresholds achieved:
- Basic (8): Small resource gain
- Enhanced (12): Moderate resources
- Premium (16): Significant resources or items

## Conversation Card Architecture

### Builder/Spender Dynamic

The core tactical loop requires using Foundation cards to generate Initiative from zero, enabling all other cards:

#### Foundation Cards (Depth 1-2)
- **Cost**: 0 Initiative
- **Effect**: Generate 1-3 Initiative
- **Purpose**: Essential starting point - nothing else playable without Initiative
- **Example**: "Active Listening" - 0 Initiative, +2 Initiative

#### Standard Cards (Depth 3-6)
- **Cost**: 0-6 Initiative (overlapping ranges)
- **Effect**: Core momentum and resource effects
- **Purpose**: Primary conversation tools with varying costs
- **Example**: "Thoughtful Response" - 4 Initiative, +5 Momentum

#### Decisive Cards (Depth 7-10)
- **Cost**: 4-12 Initiative (requires significant buildup)
- **Effect**: Powerful conversation-defining effects
- **Purpose**: Win conditions requiring multi-turn setup
- **Example**: "Perfect Understanding" - 10 Initiative, +12 Momentum

Note that depths overlap in Initiative costs - a depth 8 card might cost only 4 Initiative if it's a simple effect, while a depth 4 card might cost 4 Initiative if it's complex. Depth gates variety and options, not raw power.

### Scaling Through Visible State

All scaling uses visible game state only:
- **Statement Scaling**: "+1 Momentum per 2 Statements in Spoken"
- **Cadence Scaling**: "+X Momentum where X = current Cadence"
- **Doubt Scaling**: "+X Momentum where X = current Doubt"

No hidden tracking - all conditions visible to players.

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
- Mercantile NPCs: Diplomacy tokens
- Proud NPCs: Status tokens
- Cunning NPCs: Shadow tokens

### Promise Cards - Mid-Conversation Queue Manipulation

Promise cards create unique interaction between conversation and queue systems:

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

### Initiative Management → Goal Achievement
```
Foundation cards → Generate Initiative
Initiative enables → Standard and Decisive cards
Card plays → Build Momentum and Statements
Statements enable → Requirements for powerful cards
Momentum reaches → Goal thresholds
Complete goals → Token rewards
```

### Tokens → Signature Cards → Easier Conversations
```
Successful deliveries → +1-3 tokens
Higher tokens → More signature cards mixed in
Signature cards → Better conversation options
Better options → Higher momentum generation
More momentum → Reach higher goals
Higher goals → More tokens
```

### Familiarity → Knowledge → Access → Efficiency
```
Investigation (1 segment) → Location familiarity
Familiarity → Observation availability
Observation → Card to NPC deck OR physical item
NPC observation card → Conversation advantages
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

### Initiative Economy

**Base Values**:
- Starting Initiative: 3-6 based on highest stat
- Foundation Generation: 1-3 Initiative per card
- Standard Cost: 3-5 Initiative
- Decisive Cost: 6-12 Initiative

**Economic Pressure**:
- Can't play Decisive cards without setup
- Must balance builders vs spenders
- Initiative doesn't accumulate between SPEAK

### Momentum Thresholds

**Goal Requirements**:
- Basic: 8 Momentum
- Enhanced: 12 Momentum  
- Premium: 16 Momentum

**Generation Rate**:
- Foundation cards: 0-2 Momentum
- Standard cards: 3-6 Momentum
- Decisive cards: 7-12 Momentum
- Scaling cards: Variable based on state

### Strategic Decision Points

**Every Turn**:
- Generate Initiative vs Build Momentum
- Play safe Foundation vs risk Standard
- Consume Momentum for effect vs save for goals
- SPEAK to progress vs LISTEN to refresh

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
- No Initiative, Momentum, or conversation mechanics involved

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
- **No conversation mechanics**
- **Pure resource trade**

### Common Exchange Types

**Food Exchanges**:
- Simple meal: 2 coins → -30 hunger
- Buy bread: 3 coins → bread item (weight 1)
- Full meal: 5 coins → -50 hunger
- Feast: 8 coins → -100 hunger, +10 health

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
- Storage: 2 coins → bank items

**Item Exchanges**:
- Buy tool: X coins → tool item (permanent weight)
- Sell trade goods: trade good item → X coins
- Buy consumable: X coins → consumable item (weight 1)

### Token-Gated Exchanges

Some exchange cards require minimum tokens to appear:
- **Transport**: 2+ Diplomacy tokens → caravan available
- **Secret information**: 3+ Shadow tokens → special intel
- **Noble introduction**: 5+ Status tokens → social access
- **Temple blessing**: 4+ Trust tokens → spiritual services

The tokens don't get spent - they just gate access.

### Observation-Unlocked Exchanges

Some exchanges only appear after playing observation cards:
- Play "Trade Route Knowledge" → Unlocks transport exchange
- Play "Black Market Contact" → Unlocks illegal goods
- Permanently add new options to NPC's exchange deck

### Exchange vs Conversation

**Exchanges ARE**:
- Separate card system with own UI
- Simple resource trades
- Quick (0 segments)
- No social mechanics
- Deterministic outcomes

**Exchanges ARE NOT**:
- Part of conversations
- Subject to Initiative/Momentum/Cadence
- Using Doubt mechanics

### Strategic Role

Exchanges provide:
- Resource conversion without social challenge
- Predictable outcomes
- Quick transactions when time matters
- Token-gated progression rewards
- Observation-unlocked special options
- Weight management through consumables

## No Soft-Lock Architecture

Every system has escape valves preventing unwinnable states:

### Conversation Soft-Lock Prevention
- Foundation cards always playable (0 Initiative)
- Can always LISTEN to refresh Initiative
- Can always leave conversation
- No minimum stats required for basic cards

### Queue Soft-Lock Prevention
- Can always burn tokens to displace
- Can always drop obligations (with penalties)
- Basic work always available for coins

### Travel Soft-Lock Prevention
- Every location has "Struggle Path" (high cost but always available)
- Can always turn back
- REST action always available

### Weight Soft-Lock Prevention
- Can always drop items
- Basic letters only 1 weight
- Food consumable frees weight when eaten

### Resource Soft-Lock Prevention
- Work always generates some coins
- Basic exchanges always available
- Can survive without food (health loss)

## Content Scalability

### Adding NPCs - Simple Framework

New NPCs simply need:
- **Personality type**: Determines conversation rule
- **Three persistent decks**:
  - Signature deck (5 unique cards at token thresholds)
  - Observation deck (receives location discoveries)
  - Burden deck (damaged relationship tracking)
- **Request list**: Bundles of request and promise cards
- **Token rewards**: For successful deliveries
- **Location**: Where they can be found
- **Conversation Level**: 1-3 difficulty rating

### Adding Locations - Modular Design

New locations need:
- **Spot definitions**: With time-based properties
  - Morning: Quiet/Normal/Busy
  - Afternoon: Quiet/Normal/Busy
  - Evening: Closing/Open/Busy
- **Investigation rewards**: At each familiarity level
- **Observation destinations**: Which NPC gets each observation
- **NPCs present**: Who is where when
- **Routes**: Connections with path cards

### Adding Cards - Clear Rules

New cards must follow:
- **Initiative cost**: 0 for Foundation, 3-5 for Standard, 6-12 for Decisive
- **Single requirement OR cost**: Never both
- **One deterministic effect**: No branches
- **Persistence type**: Statement or Echo
- **Stat binding**: Which stat gains XP
- **Depth level**: 1-10 determining access

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
- Every choice has permanent consequences

**Building Power Through One NPC**:
- Focus all deliveries to Marcus
- Gain 10 Diplomacy tokens
- Unlock 4 signature cards
- Conversations become easier with more options
- Can reliably reach Premium goals
- But neglected other relationships

**Initiative Mastery**:
- Master Foundation card timing
- Build 8+ Initiative reserves
- Play multiple Standard cards in sequence
- Enable Decisive cards others can't afford
- Conversation style becomes aggressive and dominant

**The Cascading Failure**:
- Failed conversation requirements (+Doubt)
- Forced to LISTEN repeatedly
- Can't build momentum for goals
- Accept basic delivery for survival
- Low payment means can't afford food
- Poor work output due to hunger
- Miss deadline due to segments
- Lose tokens with recipient
- Future conversations even harder

## Core Innovation Summary

The refined conversation system creates strategic depth through resource management:

### Initiative as Action Economy

The builder/spender dynamic means you can't simply play your best cards. Foundation cards generate the Initiative needed for powerful effects, creating multi-turn planning and setup/payoff moments.

### Stats as Depth Gates

Your stats represent conversational competencies that unlock deeper card options. Every conversation uses contextually appropriate decks, but your stats determine which depths you can access, creating natural progression without deck building.

### Deterministic Strategy

No success/failure rolls means perfect planning. If you have the Initiative and meet requirements, the card WILL work. This transforms conversations from probability management to resource optimization.

### Visible State Scaling

Statements in Spoken create cumulative power. Early conversation builds the foundation for powerful late-game effects. The conversation has memory that mechanically matters.

### Cadence Creates Rhythm

The balance between speaking and listening isn't just thematic - it mechanically affects resource generation. Dominating conversations creates Doubt, while strategic listening grants card advantage.

### Clear Resource Separation

Five resources, five purposes, no overlap:
- Initiative: Action economy
- Momentum: Progress track
- Doubt: Timer
- Cadence: Balance
- Statements: Scaling

This is character progression through conversational mastery, not statistical advancement.

## Critical Formulas Reference

### Conversation System Formulas

**Initiative Base by Highest Stat**:
- Stats 1-4: 3 Initiative base
- Stats 5-6: 4 Initiative base
- Stats 7-8: 5 Initiative base
- Stats 9-10: 6 Initiative base

**Card Initiative Costs**:
- Depth 1-2: 0 Initiative (Foundation cards)
- Depth 3-4: 3-4 Initiative
- Depth 5-6: 5-6 Initiative
- Depth 7-8: 7-8 Initiative
- Depth 9-10: 9-12 Initiative

**Cadence System**:
- Range: -5 to +10
- SPEAK action: +1 Cadence
- LISTEN action: -3 Cadence
- Cadence ≥ 6: +1 Doubt per point above 5 on LISTEN
- Cadence ≤ -3: +1 card draw on LISTEN

**Cards Drawn on LISTEN**:
- Base: 3 cards
- +1 if Cadence ≤ -3

**Stat-Gated Depth Access**:
- Your stat level = maximum depth accessible for that stat's cards

**Stat XP Requirements**:
- Level 1→2: 10 XP
- Level 2→3: 25 XP
- Level 3→4: 50 XP
- Level 4→5: 100 XP
- Level 5→6: 175 XP
- Level 6→7: 275 XP
- Level 7→8: 400 XP
- Level 8→9: 550 XP
- Level 9→10: 750 XP

**Momentum Goal Thresholds**:
- Basic Goal: 8 momentum
- Enhanced Goal: 12 momentum
- Premium Goal: 16 momentum

**Conversation Difficulty XP**:
- Level 1: 1× XP multiplier
- Level 2: 2× XP multiplier
- Level 3: 3× XP multiplier

**Personality Rule Effects**:
- **Proud**: Cards must be played in ascending Initiative order
- **Devoted**: +2 Doubt instead of +1 on failed requirements
- **Mercantile**: Highest Initiative card gets +3 Momentum bonus
- **Cunning**: Same Initiative as previous costs -2 Momentum
- **Steadfast**: All Momentum changes capped at ±2

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
- Continue for each displaced position

**Weight Capacity**: Maximum 10, no exceptions

**Time per Day**: 24 segments (6 blocks × 4 segments)

**Conversation Time**: 1 segment + accumulated doubt

**Hunger Increase**: +20 per time block

**Starvation**: At 100 hunger, lose 5 health per block

**Item Weights**:
- Letters: 1
- Simple packages: 1-2
- Trade goods: 3-4
- Heavy deliveries: 5-6
- Tools: 1-3
- Consumables: 1
- Permits: 1

## Conclusion

The refined Wayfarer system achieves its design goals through the builder/spender conversation dynamic that maintains perfect verisimilitude. Initiative doesn't magically refresh - you must generate it through Foundation cards to enable more powerful effects, creating natural conversation flow where you build up to important points.

The stat-gated depth system means progression comes from accessing deeper conversation options rather than abstract power increases. A player with Rapport 5 literally has more empathetic responses available than someone with Rapport 2. This is character growth made tangible through expanded conversational repertoire.

The genius is deterministic card effects with visible state scaling. No hidden probabilities or success rates - if you have the Initiative and meet the requirements, the card works. Period. This transforms conversations from gambling to strategic resource management where every decision is informed.

The five resources each serve exactly one purpose with no overlap. Initiative is your action economy, built and spent. Momentum tracks progress. Doubt creates urgency. Cadence rewards listening. Statements scale power. Clean, elegant, purposeful.

Players always know what to do: have conversations to gain XP, level up stats to access deeper cards, complete requests to build tokens, gain signature cards for easier conversations. The loop is transparent and compelling.

The system succeeds because it uses familiar game mechanics (builder/spender from Steamworld, resource management from Eurogames) to create verisimilitude in conversation. You literally can't dominate a conversation endlessly because Initiative runs out. You can't repeat the same arguments because Statements stay in Spoken. You must listen periodically because Cadence creates mechanical pressure.

Every conversation becomes a puzzle of managing five resources toward a clear goal while navigating personality rules that fundamentally change the puzzle. This is the combat system of Wayfarer, expressed through social dynamics rather than violence, creating depth through elegant mechanics rather than complex rules.