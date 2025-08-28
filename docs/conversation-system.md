# Wayfarer: Complete Game Design Document

## Core Concept

You are a letter carrier navigating the social and physical landscape of a medieval city. Every conversation is a card-based puzzle where emotional states filter drawable cards. Every delivery permanently reshapes relationships through deck modifications. The world exists as mechanical configurations that emerge into narrative through play.

## Design Pillars

**Mechanical Causality**: Every narrative element emerges from mechanical state. NPCs are deck containers. Observations are cards. Letters are obligations. No scripted events, only systematic interactions.

**Perfect Information**: All costs and effects visible upfront. No hidden mechanics, no percentage modifiers behind scenes, no abstract resources. Every number shown to player.

**Singular Purpose**: Every mechanic does exactly one thing. Resources have multiple uses but each mechanic that modifies them has one intentional effect. No double duty, no OR statements in rules.

**Emergent Narrative**: Stories emerge from mechanical interactions between cards, tokens, and obligations. AI translates mechanical state into contextual narrative, never inventing mechanics.

## Three Core Game Loops

### 1. Card-Based Conversations
NPCs contain four decks (conversation, letter, crisis, exchange). Emotional states filter drawable cards during LISTEN. Through state navigation and single card plays, players build permanent token relationships and accept obligations. One statement per turn creates authentic dialogue rhythm.

### 2. Obligation Queue Management  
Forced sequential queue where position 1 MUST complete first. Players burn tokens to displace obligations, permanently damaging relationships for temporal flexibility. Queue position negotiated through letter card success/failure. Each obligation adds pressure through deadlines and queue positioning.

### 3. Location and Travel System
Routes between locations require access permits (no alternatives in route rules). Travel encounters use conversation mechanics. Observations at locations provide state change cards. Knowledge gained through both exploration and dialogue. Time advances through travel and actions.

## Player Resources

### Primary Resources
- **Coins** (0-999): Currency for exchanges and bribes
- **Health** (0-100): Physical condition, death at 0
- **Hunger** (0-100): Increases 20 per period automatically
- **Attention** (0-10): Daily action points

### Resource Interconnections
- Morning attention: 10 - (Hunger÷25), minimum 2
- Health below 50: Maximum conversation weight -1
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
- Token cards played successfully: +1 token
- Failed deliveries: -2 tokens with sender
- Queue displacement: -1 token per position jumped per displaced NPC
- Certain crisis resolutions: +1 token on success

## Attention Economy

Daily allocation of 10 attention (modified by hunger):
- **Observation**: 1 attention - gain state change card
- **Conversation**: 2 attention - full emotional system
- **Quick Exchange**: 0 attention - instant resource trade (merchants only)
- **Work Action**: 2 attention - earn 8 coins, advances one time period
- **Travel**: 0 attention - costs time based on route
- **Wait**: 0 attention - pass time strategically

## Conversation System

### Conversation Resources

**Comfort** (Temporary)
- Range: 0-20 within single conversation
- Starting Value: Always 5
- Effect: Determines maximum card depth accessible
- Modification: Only through comfort cards

**Momentum** (Temporary)
- Range: -3 to +3 within single conversation
- Starting Value: Always 0
- Effect: Varies by emotional state (see state rules)
- Modification: +1 on any successful card play, -1 on any failure

**Patience** (Per-NPC)
- Base Values: Vary by personality type
- Effect: Determines conversation length (1 patience per turn)
- Modification: Spot traits and momentum effects

### Conversation Types

**Standard Conversation** (2 attention, base patience varies by NPC)
- Full emotional state system with web transitions
- Goal card shuffled into conversation deck
- Can generate obligations through promise cards (goal cards)
- Momentum affects state stability

**Quick Exchange** (0 attention, instant)
- Mercantile NPCs only
- Draw 1 card from exchange deck
- Shows exact cost→reward trade
- Accept or refuse
- No emotional states, patience, or goals

### Conversation Goals

Each conversation type has a specific goal card shuffled into the deck:

**Letter Conversation**: Goal card creates delivery obligation
**Promise Conversation**: Goal card creates meeting/escort/investigation obligation  
**Resolution Conversation**: Goal card removes burden cards
**Commerce Conversation**: Goal card enables special trades
**Relationship Conversation**: No goal card - pure token building
**Crisis Conversation**: Goal card resolves emergency situation

Goal determines when conversation ends and what rewards are possible.

### The Core Choice

Comfort starts at 5 every conversation.
Momentum starts at 0 every conversation (range -3 to +3).

Each turn costs 1 patience. Choose:

**LISTEN**: 
- Draw cards filtered by emotional state
- Guaranteed 1 state card if available in depth range
- Goal cards become permanent when drawn (don't return to deck)
- Fleeting cards return to deck if unplayed

**SPEAK**:
- Play cards up to maximum weight for current emotional state (modified by momentum based on state rules)
- Card resolves based on success/failure
- Success: Momentum +1 (capped at +3)
- Failure: Momentum -1 (minimum -3)
- Success rate = Base + (Tokens × 5%), clamped 5%-95%
- Playing goal card ends conversation immediately
- Fleeting cards return to deck if unplayed

**The Urgency Rule**: Once a goal card is drawn, you have 3 turns to play it or the conversation fails. This creates authentic pressure - once important matters surface, they can't be ignored.

### Emotional States - Web Structure and Effects

States form a web, not a linear progression. Any state can transition to any other if the appropriate state card exists in the deck.

**State Definitions and Momentum Effects**:

**DESPERATE** (Crisis Mode)
- Listen: Draws Trust and Crisis cards + 1 guaranteed state card
- Speak: Maximum Light weight only (crisis cards weight becomes 0)
- Momentum Effect: Each point reduces patience cost by 1 (minimum 0 patience per turn)
- Goal cards: Crisis and urgent promises

**TENSE** (Stressed)  
- Listen: Draws Shadow cards + 1 guaranteed state card
- Speak: Maximum Medium weight
- Momentum Effect: Positive momentum makes observation cards weight 0
- Goal cards: Shadow promises, burden resolution

**NEUTRAL** (Baseline)
- Listen: Draws all types equally + 1 guaranteed state card
- Speak: Maximum Heavy weight
- Momentum Effect: None (balanced state)
- Goal cards: Commerce, routine promises

**GUARDED** (Defensive)
- Listen: Draws state cards only
- Speak: Maximum Medium weight
- Momentum Effect: Negative momentum increases card weight by 1 category per point
- Goal cards: None typically (too suspicious)

**OPEN** (Receptive)
- Listen: Draws Trust and Token cards + 1 guaranteed state card
- Speak: Maximum Heavy weight
- Momentum Effect: Positive momentum adds +1 comfort to successful comfort cards
- Goal cards: Trust promises, personal requests

**EAGER** (Excited)
- Listen: Draws Commerce and Token cards + 1 guaranteed state card
- Speak: Maximum Heavy weight
- Momentum Effect: Each point of momentum adds +5% to token card success
- Goal cards: Commerce promises with bonus potential

**OVERWHELMED** (Overloaded)
- Listen: Draw 1 card only (no guaranteed state)
- Speak: Maximum Light weight only
- Momentum Effect: Positive momentum allows drawing 1 additional card
- Goal cards: None (cannot focus)

**CONNECTED** (Deep Bond)
- Listen: Draws 60% Token, 40% any + 1 guaranteed state card
- Speak: Maximum Heavy weight (plus momentum allows heavier plays)
- Momentum Effect: Positive momentum allows playing one weight category higher
- Goal cards: All types available

**HOSTILE** (Aggressive)
- Listen: Draw crisis cards only, conversation ends after turn
- Speak: Crisis cards only
- Momentum Effect: None (conversation ending)
- Goal cards: None

### Momentum Degradation

At -3 momentum, automatic state degradation occurs:
- Positive states (Open/Eager/Connected) → Neutral
- Neutral → Guarded  
- Negative states (Guarded/Tense) → Hostile
- Desperate → Hostile

This creates predictable downward spirals that must be avoided through successful play.

### Card Persistence Rules

**Permanent Cards** (stay in hand):
- Observations (expire after 24-48 hours)
- Burden cards (must resolve)
- Crisis cards (must address)
- Letters (standing offers)

**Fleeting Cards** (return to deck if unplayed):
- Comfort cards
- Token cards
- State cards

Fleeting cards don't vanish - they shuffle back into the NPC's deck and may be drawn again in future turns with contextually appropriate narrative.

### Weight as Emotional Intensity

Weight represents emotional intensity, not cognitive load:
- Light: Gentle, simple statements
- Medium: Normal conversational depth
- Heavy: Complex or deeply charged statements

States limit maximum processable weight. Cannot overwhelm someone already Overwhelmed.

### Patience Calculation

Base patience by personality type:
- Devoted: 15 (patient, caring)
- Mercantile: 12 (time is money)
- Proud: 10 (easily offended)
- Cunning: 12 (calculating)
- Steadfast: 13 (reliable)

Modifiers (NPC-specific only):
- +1 for Private spot trait
- -1 for Public spot trait
- -1 per burden card in NPC deck

## Card System

### Card Anatomy
Every card contains:
- **Type**: Determines effect pool
- **Depth**: 0-20, determines comfort requirement
- **Weight**: 1-5, emotional intensity
- **Token Type**: Trust/Commerce/Status/Shadow (if applicable)
- **Success Rate**: Base percentage before tokens
- **Success Effect**: What happens on success
- **Failure Effect**: What happens on failure

### Card Types - Strict Effect Separation

**Comfort Cards**
- Effect: Modify comfort value ONLY
- Weight 1: +2 comfort (success) / -1 comfort (failure) at 65% base
- Weight 2: +4 comfort (success) / -1 comfort (failure) at 55% base
- Weight 3: +6 comfort (success) / -2 comfort (failure) at 45% base
- Higher depths may have better ratios
- Persistence: Fleeting

**Token Cards**
- Effect: Add 1 token of specific type ONLY
- Success: +1 token
- Failure: No change
- Higher depth = more reliable success rate
- Persistence: Fleeting

### Card Types - Strict Effect Separation

**Comfort Cards**
- Effect: Modify comfort value ONLY
- Weight 1: +2 comfort (success) / -1 comfort (failure) at 65% base
- Weight 2: +4 comfort (success) / -1 comfort (failure) at 55% base
- Weight 3: +8 comfort (success) / -2 comfort (failure) at 45% base
- Higher depths may have better ratios
- Persistence: Fleeting

**Token Cards**
- Effect: Add 1 token of specific type ONLY
- Success: +1 token
- Failure: No change
- Higher depth = more reliable success rate
- Persistence: Fleeting

**State Cards**
- Effect: Change emotional state ONLY
- All Weight 1 for accessibility
- Exist at all depth levels (0-20)
- Can transition from ANY state to ANY other
- Success: Specific state transition
- Failure: State unchanged
- Persistence: Fleeting

**Goal Cards** (One per conversation, shuffled into deck)
- Effect: Define conversation objective and create obligations
- Types: Promise (letters, meetings, escorts), Resolution (burden removal), Commerce (special trades), Crisis
- Requirements: Specific emotional states + depth
- Success: Favorable terms (deadline, payment, queue position)
- Failure: Unfavorable terms or partial resolution
- Playing ends conversation immediately
- Persistence: Permanent once drawn
- **Urgency Rule**: Must play within 3 turns of drawing or conversation fails

**Knowledge Cards**
- Effect: Create observation card in hand ONLY
- Success: Add specific observation
- Failure: No observation created
- Found in conversation decks

**Exchange Cards**
- Effect: Simple resource trades ONLY
- Mercantile NPCs only
- Instant resolution
- Success: Trade completes
- Failure: No trade

**Burden Cards**
- Effect: Block hand slots ONLY
- Added through failures and displacements
- Weight 2, negative effects
- Success: Remove from deck
- Failure: Remains in deck
- Persistence: Permanent until resolved

**Crisis Cards**
- Effect: Force state changes or add burdens
- Weight 5 (becomes 0 in crisis states)
- Success (40% base, +10% per matching token): Crisis resolved
- Failure (60%): +2 burden cards
- Persistence: Permanent until resolved

**Observation Cards**
- Effect: State change ONLY
- Gained from location observations (1 attention cost) or knowledge cards
- Weight 0-2 (flexible based on observation type)
- Success rate: 85%
- Expire after 24-48 hours
- Persistence: Permanent until expired

### Success Rate Calculation
Base success by weight:
- Light (W1): 60%
- Medium (W2): 50%
- Heavy (W3): 40%

Token modification:
- Regular cards: +5% per matching token
- Goal/Crisis cards: +10% per matching token
- Applied to ALL card types equally
- Can go negative (damaged relationships)

Final rate = Base + (Token Bonus), clamped 5%-95%

## NPC System

### Two Deck Architecture

**Conversation Deck** (20-25 cards typical)
- 8 Comfort cards (various weights/depths)
- 4 Token cards (various depths)
- 4 State cards (all weight 1, various depths)
- 2 Knowledge cards
- 2 Special/context cards
- 1 Goal card (shuffled in at conversation start)
- Burden cards added through failures
- Modified by deliveries and outcomes

**Exchange Deck** (Mercantile NPCs only)
- Simple resource trades
- 5-10 options
- Quick exchange costs 0 attention
- No goal cards (instant resolution)

### Goal Card Requirements

Goal cards require TWO aligned conditions:
1. Current emotional state matches goal's valid states
2. Current comfort ≥ goal's depth

During LISTEN, goal cards drawn become permanent in hand and trigger the urgency rule (3 turns to play).

Tokens affect success chance when playing the goal card (+5% per matching token), determining negotiation outcomes (deadline, payment, queue position). No token thresholds exist - goals are accessible at any token level, but more tokens mean better terms.

### Personality Types

**Devoted** (Elena, Priests, Family)
- 15 patience, Trust-focused deck
- Burns Trust tokens when displaced
- Letters often personal, emotional

**Mercantile** (Marcus, Innkeepers, Traders)
- 12 patience, Commerce-focused deck
- Burns Commerce tokens when displaced
- Has exchange deck for instant trades

**Proud** (Lord Blackwood, Nobles)
- 10 patience, Status-focused deck
- Burns Status tokens when displaced
- Letters attempt queue position 1

**Cunning** (Spies, Informants)
- 12 patience, Shadow-focused deck
- Burns Shadow tokens when displaced
- Letters often time-sensitive

**Steadfast** (Guards, Workers)
- 13 patience, Balanced deck
- Burns varied tokens based on context
- Reliable but rigid

## Promise System (Obligations)

### Promise Types

Promise cards (goal cards) create various obligations when played:

**Letter Promise**: Deliver correspondence
- Type: Trust/Commerce/Status/Shadow
- Creates: Delivery obligation to specific NPC
- Payment: 5-20 coins based on negotiation
- Deck Effect: Cards added to recipient upon delivery

**Meeting Promise**: Attend scheduled meeting
- Creates: Be at specific location at specific time
- Cannot be displaced (time-fixed)
- Payment: Information or tokens typically

**Escort Promise**: Guide NPC between locations  
- Creates: Pick up and deliver NPC
- Requires return journey
- Payment: High coin rewards

**Investigation Promise**: Gather specific information
- Creates: Obtain specific observation card
- Requires exploration
- Payment: Knowledge cards or permits

**Retrieval Promise**: Find and return item
- Creates: Locate item at specific location
- May require multiple conversations
- Payment: Items or resources

**Introduction Promise**: Connect two NPCs
- Creates: Bring two NPCs together
- Complex routing puzzle
- Payment: Network effects

### Promise Negotiation

When playing a promise card (goal card):
- Success: Favorable terms (longer deadline, better payment, flexible queue position)
- Failure: Unfavorable terms (tight deadline, poor payment, forced position)
- Token bonus: +10% per matching token type for negotiation

Base rates by promise difficulty:
- Easy promises: 50% base (routine deliveries, simple meetings)
- Standard promises: 45% base (most letters, investigations)
- Hard promises: 40% base (crisis resolutions, dangerous tasks)

### Access Permits

Special promise type that:
- Occupy satchel space
- Have no delivery obligation
- Enable specific routes/locations
- Do not expire (physical documents)
- Generated through promise cards or exchanges

### Satchel Management

- Capacity: 5 items maximum (letters, permits, retrieved items)
- Cannot accept new items when full
- Dropping letters: -2 tokens with sender, add 2 burden cards
- Access permits count against limit
- Retrieved items for promise completion count against limit

### Promise Effects

Successful completion:
- Payment received (coins, items, knowledge)
- May trigger new goal cards in recipient's deck
- Permanent relationship improvement
- Deck evolution based on promise type

Failed completion (deadline passed):
- Adds 2 burden cards to promiser's deck
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
Each burned token adds 1 burden card to their deck.

## Observation System

### Creating Observations
Cost 1 attention at specific locations/times.
Creates observation card in hand:
- Type: State change cards
- Depth 0-5 (accessible early in conversations)
- Weight 1 (always playable in Tense)
- 70% success rate

### Observation Sources
**Location Observations**: Spend attention to observe environment
- Different observations available at different times
- Refresh each time period

**Conversation Cards**: Knowledge cards create observations
- "Ask About Routes" → Travel state change
- "Request Information" → Contextual state change
- "Ask About Work" → Creates work opportunity at location

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
- **Evening** (18-24): Shops closing, taverns peak, social time
- **Night** (24-6): Dangerous, most closed, special encounters

Time advances through:
- Travel: Each route costs specific time
- Work actions: Advance one full period
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
- **Commercial**: Enables work actions
- **Private**: +1 patience modifier in conversations
- **Public**: -1 patience modifier in conversations
- **Discrete**: Hidden from authority
- **Hospitality**: Offers rest exchanges
- **Authority**: Guards present
- **Tense**: Adds pressure to conversations

Movement between spots within a location is instant and free.

## Work and Rest

### Work Action
Available at Commercial spots or through knowledge cards:
- Cost: 2 attention + advance one period
- Reward: 8 coins exactly
- No other effects (intentional design)
- Can be unlocked by "Ask About Work" knowledge cards

### Rest Exchanges
Available at Hospitality spots:
- "Quick Nap": 2 coins → +3 attention, advance 2 hours
- "Full Rest": 5 coins → Full refresh, advance to morning
- "Hot Meal": 2 coins → Hunger = 0
- "Luxury": 10 coins → Full refresh + health restoration

## Morning Refresh
At 6 AM daily:
1. Base attention = 10
2. Subtract (Hunger ÷ 25)
3. Minimum = 2 attention

Example: 60 hunger = 10 - 2 = 8 attention

## Deck Evolution

### Successful Delivery
Adds cards to recipient's deck based on:
- Quality determined by relationship with sender
- Type based on letter type
- Permanent addition creating lasting change

Example: Delivering Elena's Trust letter to Lord Blackwood adds Trust comfort cards to his deck, making future Trust conversations easier.

### Failed Delivery
Adds burden cards to sender's deck:
- 2 burden cards per failure
- Weight 2, blocks hand slot
- Must be resolved through play

### Long-term Consequence
NPC decks become archaeological records of relationship history. Twenty interactions create twenty permanent modifications. A deck full of burden cards represents a damaged relationship requiring repair.

## Strategic Depth

### Context-Dependent Card Value

The same card has different strategic value based on emotional state:
- State cards become critical in Desperate (escape crisis)
- Token cards become optimal in Open (accessible draws)
- Comfort cards become essential when approaching letter depth

### Multi-Conversation Arcs

Letters require aligned conditions across multiple meetings:
1. First meeting: Build tokens to meet threshold
2. Second meeting: Practice state navigation
3. Third meeting: Build comfort to reach letter

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
- Observe for state control but spend attention
- Displace queue but burn tokens permanently

### No Soft-Locks
Always have options:
- Can work if broke
- Can rest if exhausted
- Can wait if early
- Can displace if desperate
- State cards guaranteed in draws
- Crisis cards weight 0 in crisis states

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
- Observations: State change + availability
- Letters: Token requirement + state tag + depth
- Routes: Time cost + permit requirements

### AI Translation

AI translates mechanical state contextually:
- Same card → Different narrative per NPC
- Emotional states → Appropriate dialogue
- Observation cards → Discovery scenes
- Letter delivery → Relationship evolution

**Card Text**: What player says based on type and effect
- Comfort: "I understand your situation"
- Token: "Let me prove my reliability"
- State: "Perhaps we should calm down"
- Letter: "I'll deliver this by sunset for 10 coins"

**NPC Responses**: Generated from emotional state + personality + context

**Atmospheric Description**: Location + time + weather + city mood

No pre-authored content. Mechanical state generates narrative through AI translation.

## Core Innovations

**One Card Per Turn**: Revolutionary simplification that increases depth. Real conversations happen one statement at a time.

**State Filtering**: Emotional states determine available cards, creating contextual conversations.

**Token Linearity**: Every token matters equally. No thresholds, pure linear progression.

**Queue Displacement**: Permanent token sacrifice for temporal flexibility.

**Fleeting Cards**: Tactical hand management without deck building complexity.

**Strict Effect Separation**: Each mechanic does one thing. Each resource has multiple uses through different mechanics.

This is Wayfarer: Where every conversation is a puzzle, every obligation a commitment, and every delivery permanently changes the world. Emotional states filter possibilities, tokens accumulate linearly, and every letter negotiation shapes your journey through a world of mechanical poetry.