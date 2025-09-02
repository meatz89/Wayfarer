# Wayfarer: Complete Game Design Document

## Core Concept

You are a letter carrier navigating the social and physical landscape of a medieval city. Every conversation is a card-based puzzle where emotional states determine weight capacity and draw amounts. Every delivery permanently reshapes relationships through deck modifications. The world exists as mechanical configurations that emerge into narrative through play.

## Design Pillars

**Mechanical Causality**: Every narrative element emerges from mechanical state. NPCs are deck containers. Observations are cards. Letters are obligations. No scripted events, only systematic interactions.

**Perfect Information**: All costs and effects visible upfront. No hidden mechanics, no percentage modifiers behind scenes, no abstract resources. Every number shown to player.

**Singular Purpose**: Every mechanic does exactly one thing. Resources have multiple uses but each mechanic that modifies them has one intentional effect. No double duty, no OR statements in rules.

**Emergent Narrative**: Stories emerge from mechanical interactions between cards, tokens, and obligations. AI translates mechanical state into contextual narrative, never inventing mechanics.

## Three Core Game Loops

### 1. Card-Based Conversations
NPCs contain three decks (conversation, goal, exchange). Emotional states determine weight capacity and card draws during LISTEN. Through comfort management and single card plays, players build permanent token relationships and accept obligations. One statement per turn creates authentic dialogue rhythm.

### 2. Obligation Queue Management  
Forced sequential queue where position 1 MUST complete first. Players burn tokens to displace obligations, permanently damaging relationships for temporal flexibility. Queue position negotiated through promise card success/failure. Each obligation adds pressure through deadlines and queue positioning.

### 3. Location and Travel System
Routes between locations require access permits (no alternatives in route rules). Travel encounters use conversation mechanics. Observations at locations provide unique effect cards. Information gained through exploration and dialogue. Time advances through travel and actions.

## Player Resources

### Primary Resources
- **Coins** (0-999): Currency for exchanges and bribes
- **Health** (0-100): Physical condition, death at 0
- **Hunger** (0-100): Increases 20 per period automatically
- **Attention** (0-10): Daily action points

### Resource Interconnections
- Morning attention: 10 - (HungerÃ·25), minimum 2
- Health below 50: Maximum weight capacity -1
- At 100 hunger: Starvation begins, -5 health per period
- Hunger affects player capability, not NPC patience

### Connection Tokens
Permanent relationship capital per NPC:
- **Trust**: Personal bonds (+5% to all cards per token)
- **Commerce**: Professional dealings (+5% to all cards per token)  
- **Status**: Social standing (+5% to all cards per token)
- **Shadow**: Shared secrets (+5% to all cards per token)

Linear progression. No thresholds. Can go negative (relationship debt).

Token changes only through:
- Successful letter delivery: +1 to +3 tokens with recipient
- Failed deliveries: -2 tokens with sender
- Queue displacement: -1 token per position jumped per displaced NPC
- Certain crisis resolutions: +1 token on success

## Attention Economy

Daily allocation of 10 attention (modified by hunger):
- **Observation**: 1 attention - gain observation card for player observation deck
- **Conversation**: 2 attention - full emotional system
- **Quick Exchange**: 0 attention - instant resource trade (merchants only)
- **Work Action**: 2 attention - earn 8 coins, advances one time period
- **Travel**: 0 attention - costs time based on route
- **Wait**: 0 attention - pass time strategically

## Conversation System

After extensive deliberation, here's the complete deck architecture that maintains elegant mechanical separation while enabling rich conversation options:

### NPC Deck Structure

Each NPC has THREE decks:

1. Conversation Deck (20 cards)
**Always Contains**:
- 6 Fixed comfort cards (various weights)
- 4 Scaled comfort cards (matching NPC personality) 
- 2 Draw cards (1 weight each)
- 2 Weight-add cards (2 weight each)
- 3 Setup cards (0 weight with atmosphere)
- 2 High-weight dramatic cards (fleeting)
- 1 Flex slot

**Draw Rules**: Weight capacity and number of cards determined by emotional state

2. Goal Deck (Variable, 2-8 cards)
**Can Contain**:
- Letter goals (Trust/Commerce/Status/Shadow)
- Promise goals (Meeting/Escort/Investigation)
- Resolution goals (Remove Burdens)
- Commerce goals (Special trades)

**Draw Rules**: NEVER drawn directly. ONE goal selected based on conversation type and shuffled into conversation deck copy.

3. Exchange Deck (Mercantile NPCs only, 5-10 cards)
**Contains**: Simple instant trades
**Draw Rules**: Draw 1, accept or decline, conversation ends

### Player Observation Deck

The player builds and maintains their own observation deck through:
- Location observations (1 attention at specific spots/times)
- Conversation rewards (NPCs share observations)
- Travel discoveries (discovering routes)

**Observation Properties**:
- All weight 1
- Always persistent
- 85% success rate (Very Easy difficulty)
- Unique effects not available on normal cards
- Expire after 24-48 hours

### Conversation Flow

**Starting a Conversation**: Attention cost depends on conversation type chosen (see below)

**Initial Draw**: When a conversation starts, an automatic LISTEN action occurs with no patience cost, drawing cards based on the NPC's emotional state. This gives the player initial options without spending resources.

**Actions** (both cost 1 patience, NO attention cost):
- **LISTEN**: Draw cards based on emotional state, refresh weight pool to maximum
- **SPEAK**: Play cards up to available weight from pool, fleeting cards discarded after resolution

### Conversation Types & Requirements

The **player chooses conversation type** on location screen, determining which goal gets shuffled in:

### Standard Conversation (Always Available)
- **UI Label**: "Chat with [NPC]"
- **Cost**: 2 attention
- **Requirement**: None
- **Goal**: None - pure relationship building
- **Duration**: Full patience
- **Purpose**: Token farming, state navigation practice

### Letter Offer (Promise Type)
- **UI Label**: "Discuss Letter"
- **Cost**: 1 attention
- **Requirement**: Letter goal in NPC's goal deck
- **Goal**: Letter promise card shuffled in
- **Duration**: Until goal played or patience depletes
- **Purpose**: Accept delivery obligations (adds letter to satchel + obligation to queue)

### Letter Delivery
- **UI Label**: "Deliver Letter"
- **Cost**: 0 attention (free)
- **Requirement**: Have letter for this NPC in satchel
- **Goal**: None - automatic delivery card in hand
- **Duration**: Play delivery card to complete
- **Purpose**: Complete delivery obligations

### Resolution Conversation
- **UI Label**: "Make Amends"
- **Cost**: 2 attention
- **Requirement**: 2+ burden cards in NPC's relationship record
- **Goal**: "Clear the Air" resolution goal shuffled in
- **Duration**: Until goal played or patience depletes
- **Purpose**: Remove burden cards, repair relationship

### Meeting Arrangement (Promise Type)
- **UI Label**: Context-specific ("Arrange Meeting", "Discuss Escort")
- **Cost**: 2 attention
- **Requirement**: Meeting promise goal in goal deck
- **Goal**: Meeting promise card shuffled in
- **Duration**: Until goal played or patience depletes
- **Purpose**: Accept meeting obligations (time/location requirements)

### Quick Exchange
- **UI Label**: "Quick Trade"
- **Cost**: 0 attention (free)
- **Requirement**: NPC has exchange deck (merchants, location-specific)
- **Goal**: None - immediate accept/decline cards
- **Duration**: Single turn exchange
- **Purpose**: Simple resource swaps

### Goal Card Selection Rules

When starting a conversation with a goal:

1. **Identify conversation type** from player's choice
2. **Select appropriate goal** from goal deck based on GoalType
3. **Create conversation instance**: Copy conversation deck + selected goal
4. **Shuffle goal into deck**
5. **Begin conversation** with standard rules

### Example: Elena's Deck Configuration

### Conversation Deck
- 6 Fixed comfort cards (W0-W4, various difficulties)
  - 5 Trust-type cards (match her Devoted personality)
  - 1 Status-type card (secondary option)
- 4 Scaled comfort cards (Trust-based scaling)
  - All Trust-type cards
  - Scale with Trust tokens specifically
- 2 Draw cards (W1 each)
  - Both Trust-type cards
- 2 Weight-add cards (W2 each)
  - Both Trust-type cards
- 3 Setup cards (W0 with atmosphere changes)
  - Mixed types (1 Trust, 1 Commerce, 1 Status)
- 2 High-weight dramatic cards (W5, fleeting)
  - Both Trust-type for maximum synergy
- 1 Flex card
  - Trust-type emotional overflow card

### Goal Deck
- "Marriage Refusal Letter" (Trust-type, Weight 5)
- "Personal Letter" (Trust-type, Weight 6)
- "Meet Tonight" (Trust-type, Weight 5)
- "Clear the Air" (Trust-type, Weight 5)

### Available Conversations at Location
- **Make Amends** (burden cards in relationship record)
- **Discuss Letter** (letter goals available)
- **Chat** (always available)

If player has 3 Trust tokens with Elena, they get +15% success on her Trust-type cards (most of her deck) but +0% on her few non-Trust cards.

### The Goal Urgency Rule

Goal cards have the "Final Word" property - if a fleeting goal card would be discarded (not played during SPEAK), the conversation immediately ends in failure. This creates authentic pressure without special tracking rules.

## Deck Evolution Through Play

**Successful Letter Delivery**: 
- Adds 2 comfort cards to recipient's conversation deck
- May add new goals to recipient's goal deck
- Adds 1-3 tokens to relationship

**Failed Delivery**:
- Adds 2 burden cards to sender's relationship record
- Enables "Make Amends" conversation option
- -2 tokens with sender

**Promise Completion**:
- May unlock new goal cards
- Improves relationship standing

**Burden Resolution Success**:
- Removes burden cards from relationship record
- May add trust token

### Conversation Resources

**Comfort** (Temporary)
- Range: -3 to +3 within single conversation
- Starting Value: Always 0
- Effect: At Â±3, triggers emotional state transition, resets to 0 after state transition
- Modification: Only through comfort cards (fixed or scaled effects)
- State progression: [Ends] â† Desperate â† Tense â† Neutral → Open → Connected

**Patience** (Per-NPC)
- Base Values: Vary by personality type
- Effect: Determines conversation length (1 patience per turn)
- Modification: Spot traits and patience atmosphere

**Weight Pool** (Per-Conversation)
- Capacity: 3-6 based on emotional state
- Refreshes to maximum on LISTEN
- Persists across SPEAK actions until depleted
- Modified by Prepared atmosphere (+1 capacity)

**Atmosphere** (Per-Conversation)
- Persists until changed by card or failure
- Affects all actions while active
- Not reset by LISTEN
- Failure clears to Neutral atmosphere

### Conversation Types

**Standard Conversation** (2 attention, base patience varies by NPC)
- Full emotional state system with transitions
- Goal card shuffled into conversation deck based on conversation type chosen
- Can generate obligations through promise cards (goal cards)
- Comfort battery affects state stability

**Quick Exchange** (0 attention, instant)
- Mercantile NPCs only
- Draw 1 card from exchange deck
- Shows exact cost→reward trade
- Accept or refuse
- No emotional states, patience, or goals

### Conversation Goals

Each conversation type determines which goal card from the goal deck gets shuffled in:

**Letter Conversation**: Letter goal card creates delivery obligation
**Promise Conversation**: Promise goal card creates meeting/escort/investigation obligation  
**Resolution Conversation**: Resolution goal card removes burden cards
**Commerce Conversation**: Commerce goal card enables special trades
**Standard Conversation**: No goal card - pure token building

Goal determines when conversation ends and what rewards are possible.

### The Core Choice

Comfort starts at 0 every conversation.

Each turn costs 1 patience. Choose:

**LISTEN**: 
- Draw cards based on emotional state
- Refresh weight pool to maximum
- Preserve fleeting cards in hand

**SPEAK**:
- Play cards up to available weight from pool
- Card resolves based on difficulty and success chance
- Success: Primary effect occurs
- Failure: No effect (atmosphere clears to Neutral)
- Fleeting cards played are discarded after resolution
- Playing goal card ends conversation immediately

### Emotional States - Capacity and Draw Rules

States determine weight capacity and cards drawn. No filtering of card types.

**State Definitions**:

**DESPERATE** (Crisis State)
- Listen: Draws 1 card
- Weight Capacity: 3
- Comfort: +3→Tense, -3→Conversation ends immediately

**TENSE** (Stressed State)  
- Listen: Draws 2 cards
- Weight Capacity: 4
- Comfort: +3→Neutral, -3→Desperate

**NEUTRAL** (Baseline State)
- Listen: Draws 2 cards
- Weight Capacity: 5
- Comfort: +3→Open, -3→Tense

**OPEN** (Receptive State)
- Listen: Draws 3 cards
- Weight Capacity: 5
- Comfort: +3→Connected, -3→Neutral

**CONNECTED** (Deep Bond State)
- Listen: Draws 3 cards
- Weight Capacity: 6
- Comfort: +3→Stays Connected, -3→Open

### Card Persistence Rules

**Persistent Cards** (75% of deck):
- Remain in hand after LISTEN
- Stay until played or conversation ends
- Observations always persistent
- Most utility and comfort cards

**Fleeting Cards** (25% of deck):
- Removed from hand after SPEAK resolution (whether played or not)
- High-impact dramatic cards
- Goal cards with "Final Word" property
- Create urgency and timing decisions

### Card Difficulty and Success Rates

Base success rates by difficulty:
- **Very Easy**: 85% (Observation cards only)
- **Easy**: 70% (Basic comfort, setup cards)
- **Medium**: 60% (Standard comfort, utility)
- **Hard**: 50% (Scaled comfort, dramatic effects)
- **Very Hard**: 40% (Goal cards, major effects)

Token modification:
- Each matching token: +5% success
- Applied to ALL card types equally
- Can go negative (damaged relationships)

Final rate = Base + (Tokens Ã— 5%), clamped 5%-95%

### Atmosphere System

Atmosphere affects all actions until changed or cleared. Standard atmospheres available on ~30% of normal cards:

**Standard Atmospheres**:
- **Neutral**: No effect (default after failure)
- **Prepared**: +1 weight capacity all SPEAK actions
- **Receptive**: +1 card all LISTEN actions
- **Focused**: +20% success all cards
- **Patient**: All actions cost 0 patience
- **Volatile**: All comfort changes Â±1
- **Final**: Any failure ends conversation

**Observation-Only Atmospheres**:
- **Informed**: Next card cannot fail
- **Exposed**: Double all comfort changes
- **Synchronized**: Next card effect happens twice
- **Pressured**: -1 card on all LISTEN actions

## NPC System

### Three Deck Architecture

**Conversation Deck** (20 cards standard)
- 6 Fixed comfort cards (various weights and difficulties)
  - Primary token type matches NPC personality
  - 4-5 cards of primary type, 1-2 of secondary types
- 4 Scaled comfort cards (personality-matched)
  - All same token type as NPC personality
  - Scale with specific token type (Trust/Commerce/Status/Shadow)
- 2 Draw cards (weight 1 each)
  - Token type matches NPC personality
- 2 Weight-add cards (weight 2 each)
  - Token type matches NPC personality
- 3 Setup cards (weight 0 with atmosphere)
  - Mixed token types for flexibility
- 2 Dramatic cards (high weight, fleeting)
  - Primary token type for best synergy
- 1 Flex slot
  - Personality-specific token type
- Modified by deliveries and outcomes

**Goal Deck** (3-8 cards typical)
- Letter goals (Trust/Commerce/Status/Shadow)
- Promise goals (Meeting/Escort/Investigation)
- Resolution goals (Remove burdens)
- Commerce goals (Special trades)
- Crisis goals (Emergency situations)

**Exchange Deck** (Mercantile NPCs only)
- Simple resource trades
- 5-10 options
- Quick exchange costs 0 attention
- No goal cards (instant resolution)

### Goal Card Requirements

Goal cards are selected from goal deck based on:
1. Player's chosen conversation type
2. Current context and urgency

Goal cards require sufficient weight capacity to play (5-6 weight typically), creating the need to reach Open or Connected states (or use Prepared atmosphere).

### Personality Types

**Devoted** (Elena, Priests, Family)
- 15 patience, Trust-type cards dominate deck
- Trust-scaling comfort cards
- Burns Trust tokens when displaced
- Letters often personal, emotional
- ~15 Trust-type cards, ~5 mixed other types

**Mercantile** (Marcus, Innkeepers, Traders)
- 12 patience, Commerce-type cards dominate deck
- Commerce-scaling comfort cards
- Burns Commerce tokens when displaced
- Has exchange deck for instant trades
- ~15 Commerce-type cards, ~5 mixed other types

**Proud** (Lord Blackwood, Nobles)
- 10 patience, Status-type cards dominate deck
- Status-scaling comfort cards
- Burns Status tokens when displaced
- Letters attempt queue position 1
- ~15 Status-type cards, ~5 mixed other types

**Cunning** (Spies, Informants)
- 12 patience, Shadow-type cards dominate deck
- Shadow-scaling comfort cards
- Burns Shadow tokens when displaced
- Letters often time-sensitive
- ~15 Shadow-type cards, ~5 mixed other types

**Steadfast** (Guards, Workers)
- 13 patience, Balanced deck across all types
- Mixed scaling comfort cards
- Burns varied tokens based on context
- Reliable but rigid
- ~5 cards of each token type

## Promise System (Obligations)

### Promise Types

Promise cards (goal cards) create three types of obligations:

**Delivery Promise**: Transport items or letters
- Type: Trust/Commerce/Status/Shadow
- Creates: Delivery obligation to specific NPC
- Payment: 5-20 coins based on negotiation
- Deck Effect: Cards added to recipient upon delivery
- Can be displaced in queue

**Meeting Promise**: Attend scheduled appointment
- Creates: Be at specific location at specific time
- Cannot be displaced (time-fixed)
- Payment: Information or tokens typically
- Failure has severe consequences

**Resolution Promise**: Fix problems
- Creates: Remove burden cards from relationship record
- Requires: Successful resolution through card play
- Payment: Relationship repair, special rewards
- Rebuilds damaged relationships

### Promise Negotiation

When playing a promise card (goal card):
- Base success: 30-50% (Very Hard to Hard difficulty)
- Token bonus: +5% per matching token type
- Success: Favorable terms (longer deadline, better payment, flexible queue position)
- Failure: Unfavorable terms (tight deadline, poor payment, forced position)

### Access Permits

Special delivery type that:
- Occupy satchel space
- Have no delivery obligation
- Enable specific routes/locations
- Do not expire (physical documents)
- Generated through promise cards or exchanges

### Satchel Management

- Capacity: 5 items maximum (letters, permits, retrieved items)
- Cannot accept new items when full
- Dropping letters: -2 tokens with sender, add 2 burden cards to relationship
- Access permits count against limit
- Retrieved items for promise completion count against limit

### Promise Effects

Successful completion:
- Payment received (coins, items, observation cards for player deck)
- May trigger new goal cards in recipient's deck
- Permanent relationship improvement
- Deck evolution based on promise type

Failed completion (deadline passed):
- Adds 2 burden cards to promiser's relationship record
- -2 tokens with promiser
- No payment
- Damages relationship permanently
- May lock out future promises

## Obligation Queue

### Queue Rules
- Position 1 MUST complete first (no exceptions)
- New obligations enter at lowest available position
- Maximum 10 obligations
- Cannot accept new obligations when full

### Queue Position Negotiation
When playing letter card:
- Success: Your terms (usually lowest available position)
- Failure: NPC's terms (often position 1)

Personality modifiers:
- Proud: Always attempts position 1
- Desperate state: Attempts position 1
- Crisis letters: Force position 1

### Queue Displacement
To deliver out of order, burn tokens with EACH displaced NPC:
- Jump 1 position: -1 token with that NPC
- Jump 2 positions: -2 tokens with that NPC
- Jump 3+ positions: -3 tokens with that NPC

Token type burned matches NPC personality.
Each burned token adds 1 burden card to their relationship record.

## Observation System

### Player Observation Deck
The player builds their own observation deck through:
- Location observations: Spend 1 attention to observe environment
- Conversation rewards: NPCs share observations
- Travel discoveries: Finding new routes and information

### Creating Observations
Cost 1 attention at specific locations/times.
Adds observation card to player's deck:
- Weight: 1 (always playable with minimal capacity)
- Persistence: Always persistent
- 85% success rate (Very Easy difficulty)
- Unique effects not available on normal cards

### Observation Sources
**Location Observations**: Spend attention to observe environment
- Different observations available at different times
- Refresh each time period

**Conversation Rewards**: NPCs share observations
- Completing promises may reward observation cards
- Building relationships unlocks shared knowledge

### Expiration
- 24-48 hours depending on type
- No decay states, simple deadline
- Permanent knowledge (maps) might not expire

## Travel System

### Routes and Access
Routes connect locations through Crossroads spots.
Every route requires access permit (no alternatives).

Multiple paths to same permit maintains elegant design:
- Different NPCs provide same permits
- Different methods (letter vs exchange)
- Different costs (coins vs tokens)

### Travel Encounters
Use conversation mechanics with special decks:
- Bandits: Combat conversation (violence deck)
- Guards: Authority inspection (inspection deck)
- Merchants: Trade opportunity (road trade deck)

Success passes, failure costs resources.

### Time Periods
Four daily periods advancing naturally:
- **Morning** (6-12): Attention refresh, shops opening, NPCs fresh
- **Afternoon** (12-18): Peak activity, all NPCs available
- **Evening** (18-24): Social time, taverns busy, shops closing
- **Night** (24-6): Dangerous, most closed, special encounters

Time advances through:
- Travel: Each route costs specific time
- Work actions: Advance one full period (6 hours)
- Rest actions: Skip to next morning
- Wait actions: Skip to next time block
- Natural progression: Time passes during lengthy activities

## Location Structure

### Hierarchy
Region → District → Location → Spot

Example:
Lower Wards → Market District → Central Square → Fountain

### Spot Properties

- **Crossroads**: Enables travel to other locations
- **Commercial**: Enables work actions and quick exchanges
- **Private**: +2 patience modifier in conversations

Movement between spots within a location is instant and free.

## Work and Rest

### Work Action
Available at Commercial spots:
- Cost: 2 attention + advance one period
- Reward: 8 coins exactly
- No other effects (intentional design)

### Rest Exchanges
Available at Hospitality spots:
- "Quick Nap": 2 coins → +3 attention, advance 2 hours
- "Full Rest": 5 coins → Full refresh, advance to morning
- "Hot Meal": 2 coins → Hunger = 0
- "Luxury": 10 coins → Full refresh + health restoration

## Morning Refresh
At 6 AM daily:
1. Base attention = 10
2. Subtract (Hunger Ã· 25)
3. Minimum = 2 attention

Example: 60 hunger = 10 - 2 = 8 attention

## Deck Evolution

### Successful Delivery
Adds cards to recipient's deck based on:
- Quality determined by relationship with sender
- Type based on letter type
- Permanent addition creating lasting change

Example: Delivering Elena's Trust letter to Lord Blackwood adds Trust-scaling comfort cards to his deck, making future Trust conversations easier.

### Failed Delivery
Adds burden cards to sender's relationship record:
- 2 burden cards per failure
- Blocks future progress
- Must be resolved through "Make Amends" conversation

### Long-term Consequence
NPC decks become archaeological records of relationship history. Twenty interactions create twenty permanent modifications. A relationship record full of burden cards represents damaged trust requiring repair.

## Strategic Depth

### Context-Dependent Card Value

The same card has different strategic value based on:
- Current weight pool remaining
- Emotional state (capacity and draws)
- Active atmosphere effects
- Comfort position relative to Â±3
- Fleeting vs persistent timing

### Multi-Conversation Arcs

Letters require aligned conditions across multiple meetings:
1. First meeting: Build relationship without goals
2. Second meeting: Reach sufficient weight capacity states
3. Third meeting: Draw and successfully play goal

Deadlines force tactical compromises.

### Resource Conversion Loops
- Time → Money (work actions)
- Money → Attention (rest exchanges)
- Money → Hunger relief (food exchanges)
- Attention → Progress (conversations/observations)
- Progress → Money (complete obligations)
- Tokens → Success rates (linear improvement)
- Tokens → Queue flexibility (displacement cost)

### Scheduling Puzzle
- NPC availability windows
- Shop operating hours
- Deadline management
- Travel time calculation
- Observation timing

### Meaningful Decisions
Every choice has opportunity cost:
- Work for coins but lose time
- Rest for attention but spend coins
- Observe for unique effects but spend attention
- Displace queue but burn tokens permanently
- SPEAK multiple times but risk fleeting cards

### No Soft-Locks
Always have options:
- Can work if broke
- Can rest if exhausted
- Can wait if early
- Can displace if desperate
- Weight 1 cards always playable with minimum capacity

But each option costs something valuable.

## Victory Conditions

### Short Demo (Elena's Letter)
Successfully navigate Elena's crisis and deliver her letter before deadline while managing existing obligations.

### Full Game
Build a network of relationships through successful deliveries while managing competing obligations and resource constraints. NPCs remember everything through deck modifications.

## Content Generation

### Mechanical Contracts

Every element defined mechanically:
- NPCs: Deck compositions + personality
- Observations: Unique effects + availability
- Letters: Weight requirements + token bonuses
- Routes: Time cost + permit requirements

### AI Translation

AI translates mechanical state contextually:
- Same card → Different narrative per NPC
- Emotional states → Appropriate dialogue
- Observation effects → Discovery scenes
- Letter delivery → Relationship evolution

**Card Text**: What player says based on type and effect
- Fixed Comfort: "I understand your situation" (+1 comfort)
- Scaled Comfort: "Our trust runs deep" (+X where X = Trust tokens)
- Draw: "Let me think about this" (Draw 1 card)
- Setup: "We should be careful here" (Atmosphere: Volatile)

**NPC Responses**: Generated from emotional state + personality + atmosphere + context

**Atmospheric Description**: Location + time + weather + city mood + active atmosphere

No pre-authored content. Mechanical state generates narrative through AI translation.

## Core Innovations

**Weight Pool System**: Revolutionary resource management where capacity persists across turns, enabling multi-turn planning with fleeting cards.

**Atmosphere Persistence**: Environmental effects that shape entire conversations until changed, creating strategic setup plays.

**Token-Type Matching**: Tokens only boost cards of the same type, creating specialized relationships. Trust tokens only help with Trust cards, Commerce with Commerce cards, etc. You must build the RIGHT tokens for each NPC.

**Queue Displacement**: Permanent token sacrifice for temporal flexibility.

**Fleeting on SPEAK**: Cards removed after SPEAK creates urgency without complex tracking.

**Strict Effect Separation**: Each mechanic does one thing. Each resource has multiple uses through different mechanics.

This is Wayfarer: Where every conversation is a puzzle, every obligation a commitment, and every delivery permanently changes the world. Weight pools create resource management, atmospheres shape tactical landscapes, and token-type matching forces strategic specialization through a world of mechanical poetry.