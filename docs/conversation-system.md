# Wayfarer: Complete Game Design Document

## Core Concept

You are a letter carrier navigating the social and physical landscape of a medieval city. Every conversation is a card-based puzzle where emotional states filter what can be heard and modify resource effectiveness. Every delivery permanently reshapes relationships through deck modifications. The world exists as mechanical configurations that emerge into narrative through play.

## Design Pillars

**Mechanical Causality**: Every narrative element emerges from mechanical state. NPCs are deck containers. Observations are cards. Letters are obligations. No scripted events, only systematic interactions.

**Perfect Information**: All costs and effects visible upfront. No hidden mechanics, no percentage modifiers behind scenes, no abstract resources. Every number shown to player.

**Singular Purpose**: Every mechanic does exactly one thing. Resources have multiple uses but each mechanic that modifies them has one intentional effect. No double duty, no OR statements in rules.

**Emergent Narrative**: Stories emerge from mechanical interactions between cards, tokens, and obligations. AI translates mechanical state into contextual narrative, never inventing mechanics.

## Three Core Game Loops

### 1. Card-Based Conversations
NPCs contain four decks (conversation, letter, crisis, exchange). Emotional states filter drawable cards and modify resource effectiveness. Through state navigation and single card plays, players build permanent token relationships and accept obligations. One statement per turn creates authentic dialogue rhythm.

### 2. Obligation Queue Management
Forced sequential queue where position 1 must complete first. Players burn tokens to displace obligations, permanently damaging relationships for temporal flexibility. Queue position negotiated through card play success/failure. Each obligation adds pressure through deadlines and queue positioning.

### 3. Location and Travel System
Routes between locations require access permits (no alternatives in route rules). Travel encounters use conversation mechanics. Observations at locations provide state change cards. Knowledge gained through both exploration and dialogue. Time advances through travel and actions.

## Player Resources

### Primary Resources
- **Coins** (0-999): Currency for exchanges and bribes
- **Health** (0-100): Physical condition, death at 0
- **Hunger** (0-100): Increases 20 per period automatically
- **Attention** (0-10): Daily action points

### Resource Interconnections
- Hunger affects morning attention: -1 per 25 hunger
- Health affects weight capacity: Below 50 health, maximum weight -1
- At 100 hunger: Starvation begins, -5 health per period
- Morning refresh: 10 attention - (Hunger÷25), minimum 2

### Connection Tokens
Permanent relationship capital per NPC:
- **Trust**: Personal bonds (+5% to Trust cards per token)
- **Commerce**: Professional dealings (+5% to Commerce cards per token)
- **Status**: Social standing (+5% to Status cards per token)
- **Shadow**: Shared secrets (+5% to Shadow cards per token)

Linear progression. No thresholds. Can go negative (relationship debt).

Token changes only through:
- Token cards played successfully: +1 token
- Failed deliveries: -2 tokens with sender
- Queue displacement: -1 token per position jumped
- Certain crisis resolutions: +1 token on success

## Attention Economy

Daily allocation of 10 attention (modified by hunger/health):
- **Observation**: 1 attention - gain state change card
- **Conversation**: 2 attention - full emotional system
- **Quick Exchange**: 0 attention - instant resource trade (merchants only)
- **Work Action**: 2 attention - earn 8 coins, advances one time period
- **Travel**: 0 attention - costs time based on route
- **Wait**: 0 attention - pass time strategically

## Conversation System

### Conversation Types

**Standard Conversation** (2 attention, base 15 patience)
- Full emotional state system
- State filtering of drawable cards
- Can generate letters through letter deck
- Can accept obligations through letter cards

**Quick Exchange** (0 attention, instant)
- Mercantile NPCs only
- Draw 1 card from exchange deck
- Shows exact cost→reward trade
- Accept or refuse
- No emotional states or patience

### The Core Choice

Comfort starts at 5 every conversation.

Each turn costs 1 patience. Choose:

**LISTEN**: 
- Draw cards filtered by emotional state
- Guaranteed 1 state card if available in depth range
- Check letter deck for eligible letters (matching current state AND depth ≤ comfort)
- Fleeting cards return to deck if unplayed

**SPEAK**:
- Play exactly ONE card
- Card must be ≤ maximum weight for current emotional state
- Card resolves based on success/failure
- Fleeting cards return to deck if unplayed

### Emotional States and Rules

States filter draw pools and modify resource effectiveness:

**DESPERATE** (Crisis Mode)
- Listen: Draws Trust/Crisis cards + 1 guaranteed state card
- Speak: Maximum weight 1 (crisis cards weight 0)
- Token bonus: ×2 effectiveness (each token +10%)
- Letters offered: Trust, crisis correspondence

**TENSE** (Stressed)
- Listen: Draws Shadow cards + 1 guaranteed state card
- Speak: Maximum weight 2
- Special: Observation cards cost 0 weight
- Letters offered: Shadow, urgent matters only

**NEUTRAL** (Baseline)
- Listen: Draws all types equally + 1 guaranteed state card
- Speak: Maximum weight 3
- No modifications
- Letters offered: Commerce, routine correspondence

**GUARDED** (Defensive)
- Listen: Draws state cards only
- Speak: Maximum weight 2
- Letters offered: None (too suspicious)

**OPEN** (Receptive)
- Listen: Draws Trust/Token cards + 1 guaranteed state card
- Speak: Maximum weight 3
- Comfort bonus: All successes gain +1 comfort
- Letters offered: Trust, Commerce, Status (feeling generous)

**EAGER** (Excited)
- Listen: Draws Commerce/Token cards + 1 guaranteed state card
- Speak: Maximum weight 3
- Token bonus: Commerce tokens ×3 effectiveness
- Letters offered: Commerce with +2 coin bonus terms

**OVERWHELMED** (Overloaded)
- Listen: Draw 1 card only (no guaranteed state)
- Speak: Maximum weight 1
- Letters offered: None (cannot focus)

**CONNECTED** (Deep Bond)
- Listen: Draws 60% Token, 40% any + 1 guaranteed state card
- Speak: Maximum weight 4
- Token bonus: All tokens ×2 effectiveness
- Letters offered: All types available

**HOSTILE** (Aggressive)
- Listen: Draw crisis cards only, conversation ends after turn
- Speak: Crisis cards only
- Letters offered: None

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

### Weight as Emotional Intensity

Weight represents emotional intensity, not cognitive load:
- Weight 1: Gentle, simple statements
- Weight 2: Normal conversational depth
- Weight 3: Complex or charged statements
- Weight 5: Crisis-level intensity (becomes 0 in crisis states)

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
- **Token Type**: Trust/Commerce/Status/Shadow
- **Success Rate**: Base percentage before tokens
- **Success Effect**: What happens on success
- **Failure Effect**: What happens on failure

### Card Types (Strictly Separated Effects)

**Comfort Cards**
- Effect: Modify comfort value ONLY
- Success by weight: W1 +2, W2 +4, W3 +6 comfort
- Failure: Always -1 comfort
- Persistence: Fleeting

**Token Cards**
- Effect: Add 1 token of specific type
- Success: +1 token
- Failure: No change
- Higher depth = more reliable success rate
- Persistence: Fleeting

**State Cards**
- Effect: Change emotional state ONLY
- All Weight 1 for accessibility
- Success: Specific state transition
- Failure: State unchanged
- Persistence: Fleeting

**Letter Cards**
- Effect: Accept letter with negotiated terms
- Success: Favorable (longer deadline, better pay, flexible queue position)
- Failure: Unfavorable (tight deadline, poor pay, forced position 1)
- Either result: Receive letter and obligation
- Persistence: Permanent offer

**Observation Cards**
- Effect: State change ONLY
- Gained from location observations (1 attention cost)
- Weight 1, 70% success rate
- Expire after 24-48 hours
- Persistence: Permanent until expired

**Knowledge Cards**
- Effect: Create observation card in hand
- Success: Add specific observation
- Failure: No observation created
- Found in conversation decks

**Exchange Cards**
- Effect: Simple resource trades
- Mercantile NPCs only
- Instant resolution
- Success: Trade completes
- Failure: No trade

**Burden Cards**
- Effect: Block hand slots
- Added through failures and displacements
- Weight 2, negative effects
- Success: Remove from deck
- Failure: Remains in deck
- Persistence: Permanent until resolved

**Crisis Cards**
- Effect: Crisis resolution
- Weight 0 in crisis states
- Success (40%): Remove crisis, +1 token
- Failure (60%): +1 burden, conversation continues
- Persistence: Permanent until resolved

### Success Rate Calculation
Base success by weight:
- Weight 1: 65%
- Weight 2: 55%
- Weight 3: 45%
- Weight 5: 35%

Token modification:
- Each matching token: +5% success
- Modified by state multipliers (×2 or ×3)
- Can go negative (damaged relationships)

Final rate = Base + (Modified Tokens × 5%), clamped 5%-95%

## NPC System

### Four Deck Architecture

**Conversation Deck** (20 cards)
- 8 Comfort cards (various weights/depths)
- 4 Token cards (various depths)
- 4 State cards (all weight 1)
- 2 Knowledge cards
- 2 Special/context cards
- Modified by letter deliveries
- Burden cards added through failures

**Letter Deck** (Variable)
- Letters based on NPC situation
- Each tagged with eligible emotional states
- Depth determines comfort requirement
- Offered during LISTEN when conditions met

**Crisis Deck** (Usually empty)
- Populated by failures and betrayals
- Injected during Desperate/Hostile states
- Must be resolved to clear

**Exchange Deck** (Mercantile NPCs only)
- Simple resource trades
- 5-10 options
- Quick exchange costs 0 attention

### Letter Eligibility

Letters exist in NPC letter decks based on narrative context:
- Elena has "Marriage Refusal" because she faces forced marriage
- Marcus has "Trade Contract" because he's a merchant
- Guard Captain has "Access Permit" because he controls checkpoints

During LISTEN, letters are eligible if:
1. Current emotional state matches letter's tags
2. Current comfort ≥ letter's depth

Tokens only affect success chance when playing the letter card, never gate access.

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

## Letter System

### Letter Properties

Each letter represents an NPC trusting you with their correspondence:
- **Type**: Trust/Commerce/Status/Shadow/Crisis/Access
- **Sender**: NPC who trusts you with the letter
- **Recipient**: NPC to deliver to
- **Deadline**: Hours until delivery required
- **Payment**: Coins earned on successful delivery
- **Deck Effect**: Cards added to recipient's deck upon delivery

### Access Permits

Special letters that:
- Occupy satchel space
- Have no delivery obligation
- Enable specific routes/locations
- Expire after 24 hours
- Generated through letter cards or exchanges

Getting permits:
- Guard Captain's "Issue Pass" letter card
- Merchant's "Forge Documents" exchange (15 coins)
- Noble's "Grant Seal" state card at Connected
- Thief's observation creating temporary permit

### Satchel Management

- Capacity: 5 letters maximum
- Cannot accept new letters when full
- Dropping letters: -2 tokens with sender, add 2 burden cards
- Access permits count against limit

### Letter Effects

Successful delivery:
- Adds quality cards to recipient's conversation deck
- Generates payment
- May trigger reply letters in recipient's letter deck
- Permanent world change

Failed delivery (deadline passed):
- Adds 2 burden cards to sender's deck
- -2 tokens with sender
- No payment
- Damages relationship permanently

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
To deliver out of order, burn tokens with displaced NPCs:
- Jump 1 position: -1 token with each displaced NPC
- Jump 2 positions: -2 tokens with each displaced NPC
- Jump 3+ positions: -3 tokens with each displaced NPC

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

### Expiration
- 24-48 hours depending on type
- No decay states, binary valid/expired
- Permanent knowledge (maps) might not expire

## Travel System

### Routes and Access
Routes connect locations through Crossroads spots.
Every route requires access permit (no alternatives in route rules).

Multiple paths to same requirement maintains elegant design:
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
Six daily periods advancing naturally:
- **Morning** (6-10): Attention refresh, shops opening
- **Midday** (10-14): Peak activity, all NPCs available
- **Afternoon** (14-18): Social time, taverns busy
- **Evening** (18-22): Shops closing, taverns peak
- **Night** (22-2): Dangerous, most closed
- **Deep Night** (2-6): City sleeps, special encounters

Time advances through:
- Travel: Each route costs specific time
- Work actions: Advance one full period (4 hours)
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
- **Tense**: Adds tension to conversations

Movement between spots within a location is instant and free.

## Work and Rest

### Work Action
Available at Commercial spots only:
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
- Letters: Deadline + payment + deck effect
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

**Weight as Emotional Intensity**: States limit what emotional weight someone can process, creating authentic interaction.

**No Thresholds**: Every token matters equally. Linear progression without breakpoints.

**Fleeting Cards**: Tactical hand management without deck building complexity.

**Strict Mechanical Separation**: Each mechanic does one thing. Each resource has multiple uses through different mechanics.

This is Wayfarer: Where every conversation is a puzzle, every obligation a commitment, and every delivery permanently changes the world. Comfort unlocks deeper conversations, states filter possibilities, tokens are earned linearly, and every letter negotiation shapes your journey through a world of mechanical poetry.