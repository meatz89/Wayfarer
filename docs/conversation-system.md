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
NPCs contain three decks (conversation, goal exchange). Emotional states filter drawable cards during LISTEN. Through state navigation and single card plays, players build permanent token relationships and accept obligations. One statement per turn creates authentic dialogue rhythm.

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
- Morning attention: 10 - (HungerÃ·25), minimum 2
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

After extensive deliberation, here's the complete deck architecture that maintains elegant mechanical separation while enabling rich conversation options:

### NPC Deck Structure

Each NPC has THREE decks:

1. Conversation Deck (20-30 cards)
**Always Contains**:
- 8-10 Comfort cards (various weights/depths)
- 3-5 Token cards (NPC's preferred type)
- 3-5 State cards (all weight 1, various depths)
- 2-3 Knowledge cards
- 0-X Burden cards (added through failures)

**Draw Rules**: Standard emotional state filtering during LISTEN

2. Goal Deck (Variable, 2-8 cards)
**Can Contain**:
- Letter goals (Trust/Commerce/Status/Shadow)
- Promise goals (Meeting/Escort/Investigation)
- Resolution goal (Remove Burdens)
- Commerce goals (Special trades)
- Crisis goals (Emergency situations)

**Draw Rules**: NEVER drawn directly. ONE goal selected based on conversation type and shuffled into conversation deck copy.

3. Exchange Deck (Mercantile NPCs only, 5-10 cards)
**Contains**: Simple instant trades
**Draw Rules**: Draw 1, accept or decline, conversation ends

### Conversation Types & Requirements

The **presence of specific cards determines available conversation options** on location screen:

### Standard Conversation (Always Available)
- **UI Label**: "Chat with [NPC]"
- **Cost**: 2 attention
- **Requirement**: None
- **Goal**: None - pure relationship building
- **Duration**: Full patience
- **Purpose**: Token farming, state navigation practice

### Letter Conversation
- **UI Label**: "Discuss Letter"
- **Cost**: 2 attention  
- **Requirement**: Letter goal in NPC's goal deck
- **Goal**: Appropriate letter goal shuffled in
- **Duration**: Until goal played or patience depletes
- **Purpose**: Accept delivery obligations

### Resolution Conversation
- **UI Label**: "Make Amends"
- **Cost**: 2 attention
- **Requirement**: 2+ burden cards in conversation deck
- **Goal**: "Clear the Air" resolution goal shuffled in
- **Duration**: Until goal played or patience depletes
- **Purpose**: Remove burden cards, repair relationship

### Promise Conversation
- **UI Label**: Context-specific ("Arrange Meeting", "Discuss Escort")
- **Cost**: 2 attention
- **Requirement**: Promise goal in goal deck
- **Goal**: Appropriate promise goal shuffled in
- **Duration**: Until goal played or patience depletes
- **Purpose**: Accept time/location obligations

### Crisis Conversation  
- **UI Label**: "Address Crisis"
- **Cost**: 2 attention
- **Requirement**: Crisis goal in goal deck + crisis trigger met
- **Goal**: Crisis goal shuffled in
- **Duration**: Until goal played (urgent!)
- **Purpose**: Resolve emergency, prevent relationship damage

### Commerce Conversation
- **UI Label**: "Negotiate Deal"
- **Cost**: 2 attention
- **Requirement**: Commerce goal in goal deck
- **Goal**: Commerce goal shuffled in
- **Duration**: Until goal played or patience depletes
- **Purpose**: Special trades with negotiated terms

### Quick Exchange
- **UI Label**: "Quick Trade"
- **Cost**: 0 attention
- **Requirement**: Exchange deck exists
- **Goal**: None (instant resolution)
- **Duration**: 1 card draw, accept/decline
- **Purpose**: Simple resource trades

### Goal Card Selection Rules

When starting a conversation with a goal:

1. **Identify conversation type** from player's choice
2. **Select appropriate goal** from goal deck:
   - Letter: Highest priority letter matching current state
   - Promise: Most urgent promise
   - Resolution: "Clear the Air" card
   - Crisis: Active crisis goal
   - Commerce: Best available commerce goal
3. **Create conversation instance**: Copy conversation deck + selected goal
4. **Shuffle goal into deck** at appropriate depth
5. **Begin conversation** with standard rules

### Example: Elena's Deck Configuration

### Conversation Deck
- 5 Trust comfort cards (W1-W3, D2-D14)
- 3 Trust token cards (D5, D8, D12)
- 4 State cards (D2, D5, D8, D14)
- 2 Burden cards (from your past failure)
- 2 Observation cards IN PLAYER DECK (D6, D9)

### Goal Deck
- "Marriage Refusal Letter" (Trust, D8, [Desperate/Tense])
- "Personal Letter" (Trust, D10, [Open/Connected])
- "Meet Tonight" (Promise, D5, [Any state])
- "Clear the Air" (Resolution, always available when burdens exist)

### Available Conversations at Location
- **Make Amends** (burden cards present)
- **Discuss Letter** (letter goals available)
- **Chat** (always available)

If player chooses "Discuss Letter" and Elena is Desperate, only "Marriage Refusal Letter" qualifies (state match). This goal shuffles into the conversation deck at depth 8.

### The Goal Urgency Rule

Once drawn, goal cards get the "Urgent" tag, creating authentic pressure:
- **Turn 1 after draw**: Goal permanent in hand, - play or conversation fails

This prevents ignoring important matters once revealed.

## Deck Evolution Through Play

**Successful Letter Delivery**: 
- Adds 2 comfort cards to recipient's conversation deck
- May add new goals to recipient's goal deck

**Failed Delivery**:
- Adds 2 burden cards to sender's conversation deck
- Enables "Make Amends" conversation option

**Promise Completion**:
- Adds token cards to NPC's deck
- May unlock new goal cards

**Burden Resolution Success**:
- Removes burden cards permanently
- May add trust token cards

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
- Shows exact costâ†’reward trade
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
- Success rate = Base + (Tokens Ã— 5%), clamped 5%-95%
- Playing goal card ends conversation immediately
- Fleeting cards return to deck if unplayed

**The Urgency Rule**: Once a goal card is drawn, you have 3 turns to play it or the conversation fails. This creates authentic pressure - once important matters surface, they can't be ignored.

### Emotional States - Web Structure and Effects

States form a web, not a linear progression. Any state can transition to any other if the appropriate state card exists in the deck.

**State Definitions and Momentum Effects**:

**DESPERATE** (Crisis State)
- Listen: Draws Trust cards + 1 guaranteed state card
- Speak: Maximum Light (W1)
- Momentum Effect: Each point reduces patience cost by 1 (minimum 0 patience per turn)
- Goal cards: Delivery with urgent deadlines

**HOSTILE** (Aggressive State)
- Listen: Draw burden cards only, conversation ends after turn
- Speak: Light only (W1)
- Momentum Effect: None (conversation ending)
- Goal cards: None

**OVERWHELMED** (Overloaded State)
- Listen: Draw 1 card only (no guaranteed state)
- Speak: Maximum Light (W1)
- Momentum Effect: Positive momentum allows drawing 1 additional card
- Goal cards: None (cannot focus)

**TENSE** (Stressed State)  
- Listen: Draws Shadow cards + 1 guaranteed state card
- Speak: Maximum Medium (W2)
- Momentum Effect: Positive momentum makes observation cards weight 0
- Goal cards: Shadow promises, burden resolution

**GUARDED** (Defensive State)
- Listen: Draws state cards only
- Speak: Maximum Medium (W2)
- Momentum Effect: Negative momentum increases card weight by |momentum|
- Goal cards: None typically (too suspicious)

**NEUTRAL** (Baseline State)
- Listen: Draws all types equally + 1 guaranteed state card
- Speak: Maximum Heavy (W3)
- Momentum Effect: None (balanced state)
- Goal cards: Commerce, routine promises

**OPEN** (Receptive State)
- Listen: Draws Trust and Token cards + 1 guaranteed state card
- Speak: Maximum Heavy (W3)
- Momentum Effect: Positive momentum adds +1 comfort to successful comfort cards
- Goal cards: Trust promises, personal requests

**EAGER** (Excited State)
- Listen: Draws Commerce and Token cards + 1 guaranteed state card
- Speak: Maximum Heavy (W3)
- Momentum Effect: Each point of momentum adds +5% to token card success
- Goal cards: Commerce promises with bonus potential

**CONNECTED** (Deep Bond State)
- Listen: Draws 60% Token, 40% any + 1 guaranteed state card
- Speak: Maximum Heavy (W3)
- Momentum Effect: Momentum increases maximum weight capacity (can play W3 even in crisis states at +3 momentum)
- Goal cards: All types available

### Momentum Degradation

At -3 momentum, automatic state degradation occurs:
- Connected â†’ Tense (trust broken)
- Open â†’ Guarded (walls go up)
- Eager â†’ Neutral (enthusiasm dies)
- Neutral â†’ Tense (patience wears)
- Desperate â†’ Hostile (crisis boils over)
- Overwhelmed â†’ Neutral (mental reset)
- Guarded â†’ Hostile (paranoia escalates)
- Others â†’ Tense (default degradation)

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

Weight represents emotional intensity:
- **Light (W1)**: Gentle, simple statements
- **Medium (W2)**: Normal conversational depth
- **Heavy (W3)**: Complex or emotionally charged statements

States limit maximum processable weight:
- Crisis states: Maximum Light (W1)
- Cautious states: Maximum Medium (W2)
- Open states: Maximum Heavy (W3)

Cannot overwhelm someone already overwhelmed.

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
- **Weight**: Light/Medium/Heavy (W1/W2/W3)
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
- Light (W1): +2 comfort (success) / -1 comfort (failure) at 70% base
- Medium (W2): +4 comfort (success) / -1 comfort (failure) at 60% base
- Heavy (W3): +8 comfort (success) / -2 comfort (failure) at 45% base
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
- All Light (W1) for accessibility
- Exist at all depth levels (0-20)
- Emergency transitions available at depths 0-3 from every state
- Can transition from ANY state to ANY other
- Success: Specific state transition
- Failure: State unchanged
- Persistence: Fleeting

**Goal Cards** (One per conversation, shuffled into deck)
- Effect: Define conversation objective and create obligations
- Types: Delivery (letters/items), Meeting (time-fixed), Resolution (burden removal), Connection (relationship milestones)
- Requirements: Specific emotional states + depth
- Success: Favorable terms (deadline, payment, queue position)
- Failure: Unfavorable terms or partial resolution
- Token bonus: +10% per matching token type for negotiation
- Playing ends conversation immediately
- Persistence: Permanent once drawn
- **Urgency Rule**: Must play within 3 turns of drawing or conversation fails

**Exchange Cards**
- Effect: Simple resource trades ONLY
- Mercantile NPCs only
- Instant resolution
- Success: Trade completes
- Failure: No trade

**Burden Cards**
- Effect: Block hand slots ONLY
- Added through failures and displacements
- Medium (W2) weight
- Success: Remove from deck
- Failure: Remains in deck
- Persistence: Permanent until resolved

**Observation Cards**
- Effect: State change ONLY
- Gained from location observations (1 attention) or appear in NPC decks
- Light (W1) weight typically
- Success rate: 85%
- When drawn from deck: Gained permanently instead of played
- When from location: Added to hand
- Expire after 24-48 hours
- Persistence: Permanent until expired

### Success Rate Calculation
Base success by weight:
- Weight 1: 65%
- Weight 2: 55%
- Weight 3: 45%
- Weight 4: 35%
- Weight 5: 25%

Token modification:
- Each matching token: +5% success
- Applied to ALL card types equally
- Can go negative (damaged relationships)

Final rate = Base + (Tokens Ã— 5%), clamped 5%-95%

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
2. Current comfort â‰¥ goal's depth

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
- Creates: Remove burden cards from NPC deck
- Requires: Successful resolution through card play
- Payment: Relationship repair, special rewards
- Rebuilds damaged relationships

### Promise Negotiation

When playing a promise card (goal card):
- Success: Favorable terms (longer deadline, better payment, flexible queue position)
- Failure: Unfavorable terms (tight deadline, poor payment, forced position)
- Token bonus: +10% per matching token type for negotiation

Specific base rates by promise type:
- Urgent promises: 50% base (crisis situations)
- Commerce promises: 45% base (transactional)
- Trust promises: 40% base (personal risk)
- Shadow promises: 35% base (dangerous)

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
- "Ask About Routes" â†’ Travel state change
- "Request Information" â†’ Contextual state change
- "Ask About Work" â†’ Creates work opportunity at location

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
Region â†’ District â†’ Location â†’ Spot

Example:
Lower Wards â†’ Market District â†’ Central Square â†’ Fountain

### Spot Properties

- **Crossroads**: Enables travel to other locations
- **Commercial**: Enables work actions and quick exchanges
- **Private**: +2 patience modifier in conversations

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
- "Quick Nap": 2 coins â†’ +3 attention, advance 2 hours
- "Full Rest": 5 coins â†’ Full refresh, advance to morning
- "Hot Meal": 2 coins â†’ Hunger = 0
- "Luxury": 10 coins â†’ Full refresh + health restoration

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
- Time â†’ Money (work actions)
- Money â†’ Attention (rest exchanges)
- Money â†’ Hunger relief (food exchanges)
- Attention â†’ Progress (conversations/observations)
- Progress â†’ Money (complete obligations)
- Tokens â†’ Success rates (linear improvement)
- Tokens â†’ Queue flexibility (displacement cost)

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
- Same card â†’ Different narrative per NPC
- Emotional states â†’ Appropriate dialogue
- Observation cards â†’ Discovery scenes
- Letter delivery â†’ Relationship evolution

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