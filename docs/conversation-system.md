# Wayfarer: Complete Game Design Document

## Core Concept

You are a letter carrier navigating the social and physical landscape of a medieval city. Every conversation is a card-based puzzle where emotional states determine what can be heard. Every delivery permanently reshapes relationships through deck modifications. The world exists as mechanical configurations that emerge into narrative through play.

## Design Pillars

**Mechanical Causality**: Every narrative element emerges from mechanical state. NPCs are deck containers. Observations are cards. Letters are obligations. No scripted events, only systematic interactions.

**Perfect Information**: All costs and effects visible upfront. No hidden mechanics, no percentage modifiers behind scenes, no abstract resources. Every number shown to player.

**Singular Purpose**: Every mechanic does exactly one thing. Resources have multiple uses but each mechanic that modifies them has one intentional effect. No double duty, no OR statements in rules.

**Emergent Narrative**: Stories emerge from mechanical interactions between cards, tokens, and obligations. AI translates mechanical state into contextual narrative, never inventing mechanics.

## Three Core Game Loops

### 1. Card-Based Conversations
NPCs contain four decks (conversation, letter, crisis, exchange). Through emotional state navigation and single card plays, players build permanent token relationships and accept obligations. One statement per turn creates authentic dialogue rhythm.

### 2. Obligation Queue Management
Forced sequential queue where position 1 must complete first. Players burn tokens to displace obligations, permanently damaging relationships for temporal flexibility. Queue position negotiated through card play success/failure.

### 3. Location and Travel System
Routes between locations require access permits (no alternatives in route rules). Travel encounters use conversation mechanics. Observations at locations provide temporary conversation cards. Knowledge gained through both exploration and dialogue.

## Player Resources

### Primary Resources
- **Coins** (0-999): Currency for exchanges and bribes
- **Health** (0-100): Physical condition, death at 0
- **Hunger** (0-100): Increases 20 per period automatically
- **Attention** (0-10): Daily action points

### Resource Interconnections
- Hunger reduces patience: -1 patience per 20 hunger
- Health affects weight capacity: Below 50 health, maximum weight -1
- Low health reduces morning attention: -1 per 25 health missing
- Morning refresh: 10 attention - (hunger÷20) - ((100-health)÷25)

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
- Certain crisis resolutions: variable token loss

## Attention Economy

Daily allocation of 10 attention (modified by hunger/health):
- **Observation**: 1 attention - gain temporary conversation card
- **Conversation**: 2 attention - full emotional system
- **Quick Exchange**: 0 attention - instant resource trade (merchants only)
- **Work Action**: 2 attention - earn 8 coins, advances one time period
- **Travel**: 0 attention - costs time based on route
- **Wait**: 0 attention - pass time strategically

## Conversation System

### Conversation Types

**Standard Conversation** (2 attention, base 8 patience)
- Full emotional state system
- Access to conversation and letter decks
- Can generate letters through letter deck
- Can accept obligations through letter cards

**Quick Exchange** (0 attention, instant)
- Mercantile NPCs only
- Draw 1 card from exchange deck
- Shows exact cost→reward trade
- Accept or refuse
- No emotional states or patience


### The Core Choice

Each turn costs 1 patience. Choose:

**LISTEN**: 
- Draw X cards based on emotional state
- State transitions automatically (usually worsens)
- Check letter deck for eligible letters (matching current state)
- Observation cards remain in hand

**SPEAK**:
- Play exactly ONE card
- Card must be ≤ maximum weight for current emotional state
- Card resolves based on success/failure
- Crisis cards override normal rules when present

### Emotional States and Rules

States modify conversation mechanics only:

**NEUTRAL** (Baseline)
- Listen: Draw 2 cards
- Speak: Maximum weight 3
- Letters offered: Commerce, routine correspondence

**GUARDED** (Defensive)
- Listen: Draw 1 card, then state→Neutral
- Speak: Maximum weight 2
- Letters offered: None (too suspicious)

**OPEN** (Receptive)
- Listen: Draw 3 cards
- Speak: Maximum weight 3
- Letters offered: Trust, Commerce, Status (feeling generous)

**TENSE** (Stressed)
- Listen: Draw 1 card, then state→Guarded
- Speak: Maximum weight 1
- Letters offered: Shadow, urgent matters only

**EAGER** (Excited)
- Listen: Draw 3 cards
- Speak: Maximum weight 3
- Letters offered: Commerce with bonus terms

**OVERWHELMED** (Overloaded)
- Listen: Draw 1 card, then state→Neutral
- Speak: Maximum weight 1
- Letters offered: None (cannot focus)

**CONNECTED** (Deep Bond)
- Listen: Draw 3 cards
- Speak: Maximum weight 4
- Letters offered: All types available

**DESPERATE** (Crisis Mode)
- Listen: Draw 2 + 1 crisis card
- Speak: Maximum weight 1 (crisis cards ignore limit)
- Letters offered: Trust, crisis correspondence

**HOSTILE** (Aggressive)
- Listen: Draw 1 + 2 crisis cards, conversation ends after turn
- Speak: Crisis cards only
- Letters offered: None

### Weight as Emotional Intensity

Weight represents emotional intensity, not cognitive load:
- Weight 1: Gentle, simple statements
- Weight 2: Normal conversational depth
- Weight 3: Complex or charged statements
- Weight 5: Crisis-level intensity

States limit maximum processable weight. Cannot overwhelm someone already Overwhelmed.

### Patience Calculation

Base patience by personality type:
- Devoted: 12 (patient, caring)
- Mercantile: 10 (time is money)
- Proud: 8 (easily offended)
- Cunning: 10 (calculating)
- Steadfast: 11 (reliable)

Example Modifiers:
- +1 for Private spot trait
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

**Comfort Cards**: Add comfort value
- Depth 0-5: +2-3 comfort
- Depth 6-10: +4-5 comfort
- Depth 11-15: +6-7 comfort
- Depth 16-20: +8-10 comfort
- Failure: Always +1 comfort

**Token Cards**: Add 1 token of specific type
- Represent meaningful relationship moments
- Rarer than comfort cards
- Higher depth = more reliable success rate

**State Cards**: Change emotional state
- Specific state transitions only
- Cannot skip states
- Failure: State unchanged

**Letter Cards**: Accept letter with negotiated terms
- Success: Favorable (longer deadline, better pay, flexible queue position)
- Failure: Unfavorable (tight deadline, poor pay, forced position 1)
- Either result: Receive letter and obligation

**Observation Cards**: Create knowledge
- Gained from location observations or conversation cards
- Provide comfort when contextually relevant
- Decay over time (Fresh→Stale→Expired)

**Exchange Cards**: Simple resource trades
- Mercantile NPCs only
- Instant resolution
- Success: Trade completes
- Failure: No trade

**Burden Cards**: Block hand slots
- Added through failures
- Must be resolved when possible
- Weight 2, negative effects

**Crisis Cards**: Force immediate resolution
- Must be played next SPEAK regardless of weight limits
- Often end conversation on failure
- Represent urgent situations that dominate discussion

### Success Rate Calculation
Base success by weight:
- Weight 1: 60%
- Weight 2: 50%
- Weight 3: 40%
- Weight 5: 30%

Token modification:
- Each matching token: +5% success
- Linear, no caps or thresholds
- Can go negative (damaged relationships)

Final rate = Base + (Tokens × 5%), clamped 5%-95%

## NPC System

### Four Deck Architecture

**Conversation Deck** (20-25 cards)
- Comfort, Token, State cards
- Depths 0-20
- Modified by letter deliveries
- Burden cards added through failures

**Letter Deck** (Variable)
- Letters based on NPC situation
- Each tagged with eligible emotional states
- Offered during LISTEN when state matches
- No token thresholds for access

**Crisis Deck** (Usually empty)
- Populated by failures and betrayals
- Injected during Desperate/Hostile states
- Must be resolved to clear

**Exchange Deck** (Mercantile NPCs only)
- Simple resource trades
- 5-10 options
- Quick exchange costs 0 attention

### Letter Eligibility

Letters exist based on narrative context:
- Elena has "Marriage Refusal" because she faces forced marriage
- Marcus has "Trade Contracts" because he's a merchant
- Guard Captain has "Access Permits" because he controls checkpoints

Each letter tagged with emotional states when naturally offered:
- "Marriage Refusal": Desperate, Tense (when urgent)
- "Trade Contract": Eager, Neutral (when business-focused)
- "Access Permit": Neutral, Open (when cooperative)

No token requirements. Tokens only affect success chance.

### Personality Types

**Devoted** (Elena, Priest, Family)
- 12 patience, Trust-focused deck
- Burns Trust tokens when displaced
- Letters often personal, emotional

**Mercantile** (Marcus, Innkeepers, Traders)
- 10 patience, Commerce-focused deck
- Burns Commerce tokens when displaced
- Has exchange deck for instant trades

**Proud** (Lord Blackwood, Nobles)
- 8 patience, Status-focused deck
- Burns Status tokens when displaced
- Letters attempt queue position 1

**Cunning** (Spies, Informants)
- 10 patience, Shadow-focused deck
- Burns Shadow tokens when displaced
- Letters often time-sensitive

**Steadfast** (Guards, Workers)
- 11 patience, Balanced deck
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
- Generated like letters through letter deck

### Satchel Management

- Capacity: 5 letters maximum
- Cannot accept new letters when full
- Dropping letters destroys sender relationship
- Access permits count against limit

### Letter Effects

Successful delivery:
- Adds cards to recipient's conversation deck
- Generates payment
- May trigger reply letters
- Increases reputation

Failed delivery:
- Adds burden cards to sender's deck
- Reduces tokens with sender
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
- Jump 3 positions: -3 tokens with each displaced NPC

Token type burned matches NPC personality.
Each negative token with NPC adds burden card to their deck.

## Observation System

### Creating Observations
Cost 1 attention at specific locations/times.
Creates observation card in hand:
- Type matches context (Market→Commerce, Night→Shadow)
- Depth 0-5 (accessible early in conversations)
- Weight 1 (gentle information sharing)
- Provides comfort when contextually relevant

### Observation Sources
**Location Observations**: Spend attention to observe environment
**Conversation Cards**: Some cards create observations
- "Ask About Work" → "Work Opportunity" observation
- "Request Information" → Contextual knowledge card

### Decay Timeline
- 0-5 hours: full effect
- 6+ hours: discard

Represents information becoming outdated.
Permanent knowledge (maps, blackmail) might not decay.

## Travel System

### Routes and Access
Routes connect locations through Crossroads spots.
Every route requires access permit (no alternatives in route rules).

Getting permits:
- Guard Captain's "Issue Pass" letter card
- Merchant's "Forge Documents" exchange
- Noble's "Grant Seal" token card

Multiple paths to same requirement maintains elegant design.

### Travel Encounters
Use conversation mechanics with special decks:
- Bandits: Combat conversation (combat deck)
- Guards: Authority inspection (Inspection deck)
- Merchants: Trade opportunity (Trade deck)

Success passes, failure costs resources.

### Time Periods
Six daily periods advancing naturally:
- Morning (6-10): Attention refresh (modified by lodging)
- Midday (10-14): Peak activity
- Afternoon (14-18): Social time
- Evening (18-22): Taverns active
- Night (22-2): Dangerous, shops closed
- Deep Night (2-6): City sleeps

Time advances through:
- Travel: Each route costs specific time
- Work actions: Advance one full period (4 hours)
- Rest actions: Skip to next morning
- Wait actions: Skip to next time block
- Natural progression: Time passes during lengthy activities

## Location Structure

### Hierarchy

Region → District → Location → Spot

### Spot Properties

- **Crossroads**: Enables travel to other locations
- **Commercial**: Enables work actions
- **Private**: +1 patience modifier in conversations
- **Public**: -1 patience modifier in conversations  
- **Discrete**: Hidden from authority
- **Hospitality**: Offers rest exchanges

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
- "Luxury": 10 coins → Full refresh + health restoration

## Morning Refresh
At 6 AM daily:
1. Base attention = 10
2. Subtract (Hunger ÷ 20)
3. Subtract ((100 - Health) ÷ 25)
4. Minimum = 2 attention

Example: 60 hunger, 50 health = 10 - 3 - 2 = 5 attention

## Deck Evolution

### Successful Delivery
Adds cards to recipient's deck:
- Quality based on relationship with sender
- Type based on letter type
- Permanent addition

### Failed Delivery
Adds burden cards to sender's deck:
- 1 burden cards per failure
- Weight 2, blocks hand slot
- Must be resolved through play

### Long-term Consequence
NPC decks become archaeological records of relationship history. Twenty interactions create twenty permanent modifications.

## Strategic Depth

### Resource Conversion Loops
- Time → Money (work actions)
- Money → Attention (rest exchanges)
- Money → Hunger relief (food exchanges)
- Attention → Progress (conversations/observations)
- Progress → Money (complete obligations)

### Scheduling Puzzle
- NPC availability windows
- Shop operating hours
- Deadline management
- Travel time calculation

### Meaningful Decisions
Every choice has opportunity cost:
- Work for coins but lose time
- Rest for attention but spend coins
- Observe for cards but spend attention
- Displace queue but burn tokens

### No Soft-Locks
Always have options:
- Can work if broke
- Can rest if exhausted
- Can wait if early
- Can displace if desperate

But each option costs something valuable.

## Victory Conditions

### Short Demo
Successfully navigate Elena's crisis and deliver her letter before deadline.

### Full Game
Build a network of relationships through successful deliveries while managing competing obligations and resource constraints.

## Content Generation

### Mechanical Contracts

Every element defined mechanically:
- NPCs: Deck compositions + personality
- Observations: Card reward + availability
- Letters: Deadline + payment + deck effect
- Routes: Time cost + requirements

### AI Translation

AI translates mechanical state contextually:
- Same card → Different narrative per NPC
- Emotional states → Appropriate dialogue
- Observation cards → Discovery scenes
- Letter delivery → Relationship evolution

No pre-authored content. Every story emerges from mechanical causality.

All text emerges from mechanical properties:

**Card Text**: What player says based on type and effect
- Comfort: "I understand your situation"
- Token: "Let me prove my reliability"
- State: "Perhaps we should calm down"
- Letter: "I'll deliver this by sunset for 10 coins"

**NPC Responses**: Generated from emotional state + personality + context

**Atmospheric Description**: Location + time + weather + city mood

No pre-authored content. Mechanical state generates narrative through AI translation.

## Core Innovation

**One Card Per Turn**: Revolutionary simplification that increases depth. Real conversations happen one statement at a time.

**Weight as Emotional Intensity**: States limit what emotional weight someone can process, creating authentic interaction.

**No Thresholds**: Every token matters equally. Linear progression without breakpoints.

**Strict Mechanical Separation**: Each mechanic does one thing. Each resource has multiple uses through different mechanics.

This is Wayfarer: Where every conversation is a puzzle, every obligation a commitment. Comfort unlocks deeper conversations, trust is earned token by token, and every letter negotiation shapes your journey through a world of mechanical poetry.