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
NPCs contain three decks (conversation, goal, exchange). Emotional states filter drawable cards during LISTEN. Through state navigation and single card plays, players build permanent token relationships and accept obligations. One statement per turn creates authentic dialogue rhythm.

### 2. Obligation Queue Management  
Forced sequential queue where position 1 MUST complete first. Players burn tokens to displace obligations, permanently damaging relationships for temporal flexibility. Queue position negotiated through promise card success/failure. Each obligation adds pressure through deadlines and queue positioning.

### 3. Location and Travel System
Routes between locations require access permits (no alternatives in route rules). Travel encounters use conversation mechanics. Observations at locations provide state change cards. Information gained through exploration and dialogue. Time advances through travel and actions.

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
- **Observation**: 1 attention - gain state change card for player observation deck
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
- 8-10 Comfort cards (various weights)
- 3-5 Token cards (NPC's preferred type)
- 3-5 State cards (all weight 1)
- 0-2 Patience cards (extend conversation)
- 0-X Burden cards (added through failures)

**Draw Rules**: Standard emotional state filtering during LISTEN

2. Goal Deck (Variable, 2-8 cards)
**Can Contain**:
- Letter goals (Trust/Commerce/Status/Shadow)
- Promise goals (Meeting/Escort/Investigation)
- Resolution goal (Remove Burdens)
- Commerce goals (Special trades)

**Draw Rules**: NEVER drawn directly. ONE goal selected based on conversation type and shuffled into conversation deck copy.

3. Exchange Deck (Mercantile NPCs only, 5-10 cards)
**Contains**: Simple instant trades
**Draw Rules**: Draw 1, accept or decline, conversation ends

### Player Observation Deck

The player builds and maintains their own observation deck through:
- Location observations (1 attention at specific spots/times)
- Conversation rewards (NPCs share observations)
- Travel encounters (discovering routes)

**Observation Properties**:
- All weight 1 (always playable except in Hostile)
- 85% success rate
- State change effects only
- Expire after 24-48 hours

### Conversation Types & Requirements

The **player chooses conversation type** on location screen, determining which goal gets shuffled in:

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
2. **Select appropriate goal** from goal deck based on GoalType
3. **Create conversation instance**: Copy conversation deck + selected goal
4. **Shuffle goal into deck**
5. **Begin conversation** with standard rules

### Example: Elena's Deck Configuration

### Conversation Deck
- 5 Trust comfort cards (W1-W3, Drawable: [Desperate, Open, Connected])
- 3 Trust token cards (Drawable: [Open, Connected])
- 4 State cards (All W1, Drawable: [All non-Hostile])
- 1 Patience card (W1, Drawable: [All non-Hostile])
- 2 Burden cards (from your past failure)

### Goal Deck
- "Marriage Refusal Letter" (Trust, Valid States: [Desperate/Tense])
- "Personal Letter" (Trust, Valid States: [Open/Connected])
- "Meet Tonight" (Promise, Valid States: [Any state])
- "Clear the Air" (Resolution, always available when burdens exist)

### Available Conversations at Location
- **Make Amends** (burden cards present)
- **Discuss Letter** (letter goals available)
- **Chat** (always available)

If player chooses "Discuss Letter" and Elena is Desperate, only "Marriage Refusal Letter" qualifies (state match). This goal shuffles into the conversation deck.

### The Goal Urgency Rule

Once drawn, goal cards get the "Goal" persistence type, creating authentic pressure:
- Must be played within 3 turns or conversation fails
- Playing goal card ends conversation immediately

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
- Range: -3 to +3 within single conversation
- Starting Value: Always 0
- Effect: At ±3, triggers emotional state transition, resets to 0 after state transition
- Modification: Only through comfort cards (weight determines change amount)

**Patience** (Per-NPC)
- Base Values: Vary by personality type
- Effect: Determines conversation length (1 patience per turn)
- Modification: Spot traits and patience cards

### Conversation Types

**Standard Conversation** (2 attention, base patience varies by NPC)
- Full emotional state system with web transitions
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
- Draw cards filtered by emotional state (only cards listing current state as drawable)
- Goal cards get Goal persistence when drawn
- Fleeting cards return to deck if unplayed

**SPEAK**:
- Play cards up to maximum weight for current emotional state
- Card resolves based on success/failure
- Success: Comfort increases by card weight
- Failure: Comfort decreases by card weight
- Success rate = Base 60% + (Tokens × 5%), clamped 5%-95%
- Playing goal card ends conversation immediately
- Fleeting cards return to deck if unplayed

**The Urgency Rule**: Once a goal card is drawn, you have 3 turns to play it or the conversation fails. This creates authentic pressure - once important matters surface, they can't be ignored.

### Emotional States - Web Structure and Effects

States form a web, not a linear progression. Any state can transition to any other if the appropriate state card exists in the deck.

**State Definitions and Comfort Transitions**:

**DESPERATE** (Crisis State)
- Listen: Draws cards listing Desperate as drawable
- Speak: Maximum Light (W1)
- Comfort: +3→Tense, -3→Hostile
- Goal cards: Delivery with urgent deadlines

**HOSTILE** (Aggressive State)
- Listen: Draw burden cards only, conversation ends after turn
- Speak: Weight 0 (cannot play cards)
- Comfort: +3→Tense, -3→Conversation ends
- Goal cards: None

**TENSE** (Stressed State)  
- Listen: Draws cards listing Tense as drawable
- Speak: Maximum Medium (W2)
- Comfort: +3→Neutral, -3→Hostile
- Goal cards: Shadow promises, burden resolution

**GUARDED** (Defensive State)
- Listen: Draws cards listing Guarded as drawable
- Speak: Maximum Light (W1)
- Comfort: +3→Neutral, -3→Hostile
- Goal cards: None typically (too suspicious)

**NEUTRAL** (Baseline State)
- Listen: Draws cards listing Neutral as drawable
- Speak: Maximum Heavy (W3)
- Comfort: +3→Open, -3→Tense
- Goal cards: Commerce, routine promises

**OPEN** (Receptive State)
- Listen: Draws cards listing Open as drawable
- Speak: Maximum Heavy (W3)
- Comfort: +3→Connected, -3→Guarded
- Goal cards: Trust promises, personal requests

**EAGER** (Excited State)
- Listen: Draws cards listing Eager as drawable
- Speak: Maximum Heavy (W3)
- Comfort: +3→Connected, -3→Neutral
- Goal cards: Commerce promises with bonus potential

**CONNECTED** (Deep Bond State)
- Listen: Draws cards listing Connected as drawable
- Speak: Maximum Very Heavy (W4)
- Comfort: +3→Stays Connected, -3→Tense
- Goal cards: All types available

### Card Persistence Rules

**Persistent Cards** (stay in hand):
- Observations (expire after 24-48 hours)
- Burden cards (must resolve)
- Letters (standing offers)

**Fleeting Cards** (return to deck if unplayed):
- Comfort cards
- Token cards
- State cards
- Patience cards

**Goal Persistence** (special rule):
- When drawn, goal cards gain Goal persistence type
- Must be played within 3 turns or conversation fails
- Playing ends conversation immediately

**Opportunity Cards** (one chance):
- Removed from deck forever if discarded
- Represent unique moments

Fleeting cards don't vanish - they shuffle back into the NPC's deck and may be drawn again in future turns with contextually appropriate narrative.

### Weight as Emotional Intensity

Weight represents emotional intensity:
- **Light (W1)**: Gentle, simple statements
- **Medium (W2)**: Normal conversational depth
- **Heavy (W3)**: Complex or emotionally charged statements
- **Very Heavy (W4)**: Deep emotional commitment (only in Connected)

States limit maximum processable weight:
- Crisis/Defensive states: Maximum Light (W1)
- Cautious states: Maximum Medium (W2)
- Open states: Maximum Heavy (W3)
- Connected state: Maximum Very Heavy (W4)

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
- **Drawable States**: List of emotional states where card can be drawn
- **Weight**: Light/Medium/Heavy/Very Heavy (W1/W2/W3/W4)
- **Success Rate**: Base percentage before tokens
- **Success Effect**: What happens on success
- **Failure Effect**: What happens on failure

### Card Types - Strict Effect Separation

**Comfort Cards**
- Effect: Modify comfort value ONLY
- Light (W1): +1 comfort (success) / -1 comfort (failure) at 60% base
- Medium (W2): +2 comfort (success) / -2 comfort (failure) at 60% base
- Heavy (W3): +3 comfort (success) / -3 comfort (failure) at 60% base
- Persistence: Fleeting

**Token Cards**
- Effect: Add 1 token of specific type ONLY
- Success: +1 token
- Failure: No change
- All weights have same 60% base rate
- Persistence: Fleeting

**State Cards**
- Effect: Change emotional state ONLY
- All Light (W1) for accessibility
- Can transition from ANY state to ANY other
- Success: Specific state transition
- Failure: State unchanged
- 60% base rate
- Persistence: Fleeting

**Patience Cards**
- Effect: Extend conversation ONLY
- Success: +X patience (varies by card)
- Failure: No change
- Light (W1) for accessibility
- 60% base rate
- Persistence: Fleeting

**Goal Cards** (One per conversation type, shuffled into deck)
- Effect: Define conversation objective and create obligations
- Types: Delivery (letters/items), Meeting (time-fixed), Resolution (burden removal), Connection (relationship milestones)
- Requirements: Specific emotional states
- Success: Favorable terms (deadline, payment, queue position)
- Failure: Unfavorable terms or partial resolution
- Token bonus: +5% per matching token type for negotiation
- Playing ends conversation immediately
- Persistence: Goal (must play within 3 turns once drawn)

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
- Persistence: Persistent until resolved

**Observation Cards** (Player's Deck)
- Effect: State change ONLY
- Gained from location observations (1 attention) or NPC rewards
- Light (W1) weight typically
- Success rate: 85%
- Added to player's observation deck
- Expire after 24-48 hours
- Persistence: Persistent until expired

### Success Rate Calculation
Base success for all cards:
- All cards: 60% base (weight does NOT affect)

Token modification:
- Each matching token: +5% success
- Applied to ALL card types equally
- Can go negative (damaged relationships)

Final rate = Base 60% + (Tokens × 5%), clamped 5%-95%

## NPC System

### Three Deck Architecture

**Conversation Deck** (20-25 cards typical)
- 8 Comfort cards (various weights, with drawable state lists)
- 4 Token cards (with drawable state lists)
- 4 State cards (all weight 1, drawable in all non-Hostile)
- 1-2 Patience cards (weight 1, drawable in all non-Hostile)
- 2 Special/context cards
- Burden cards added through failures
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
2. Current emotional state compatibility

During LISTEN, drawn goal cards gain Goal persistence and trigger the urgency rule (3 turns to play).

Tokens affect success chance when playing the goal card (+5% per matching token), determining negotiation outcomes (deadline, payment, queue position). Letters are never gated by tokens - only negotiation quality improves.

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
- Base success: 50% standard, 40% urgent
- Token bonus: +5% per matching token type
- Success: Favorable terms (longer deadline, better payment, flexible queue position)
- Failure: Unfavorable terms (tight deadline, poor payment, forced position)

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
- Payment received (coins, items, observation cards for player deck)
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

### Player Observation Deck
The player builds their own observation deck through:
- Location observations: Spend 1 attention to observe environment
- Conversation rewards: NPCs share observations
- Travel discoveries: Finding new routes and information

### Creating Observations
Cost 1 attention at specific locations/times.
Adds observation card to player's deck:
- Type: State change cards
- Weight 1 (always playable except in Hostile)
- 85% success rate

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
- State cards become critical in negative comfort situations
- Token cards become optimal in Open/Connected (accessible draws)
- Comfort cards become essential when approaching ±3 transitions

### Multi-Conversation Arcs

Letters require aligned conditions across multiple meetings:
1. First meeting: Build tokens for better negotiation
2. Second meeting: Practice state navigation
3. Third meeting: Reach optimal state for letter

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
- State cards drawable in most states

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
- Letters: Emotional state compatibility
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