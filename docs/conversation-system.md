# Wayfarer: Complete Integrated Game Design

## Core Concept

You are a letter carrier navigating the social and physical landscapes of a medieval city. Every conversation is a card-based puzzle where emotional states define the rules. Every delivery permanently reshapes relationships through deck modifications. The world exists as mechanical configurations that AI translates into contextual narrative.

## Design Pillars

**Mechanical Causality**: Every story element emerges from mechanical relationships. NPCs are deck containers, observations are cards, letters are deck modifications.

**Perfect Information**: All costs and effects are visible upfront. No hidden mechanics, no percentage modifiers behind the scenes, no abstract resources.

**Singular Purpose**: Every mechanic does exactly one thing. Resources have multiple uses but each mechanic that modifies them has one intentional effect.

**Emergent Narrative**: No scripted events. Stories emerge from mechanical interactions between cards, tokens, and obligations.

## Three Core Game Loops

### 1. Card-Based Conversations
NPCs are containers for four decks (conversation, exchange, letter, crisis). Through emotional state navigation and card play, players build token relationships and generate obligations.

### 2. Obligation Queue Management  
A forced sequential queue where position 1 must be completed first. Players can burn tokens to displace obligations, damaging relationships for flexibility.

### 3. Location and Travel
Routes between locations require time and sometimes access permits. Travel encounters use the conversation system. Observations at locations provide temporary conversation ammunition.

## Player Resources

### Primary Resources
- **Coins** (0-999): Currency for exchanges and travel
- **Health** (0-100): Physical condition, death at 0
- **Hunger** (0-100): Increases by 20 per period
- **Attention** (0-10): Daily action points, refreshes each morning

### Resource Interconnections
- Hunger affects patience: Every 20 hunger reduces base patience by 1
- Health affects weight limit: Below 50 health reduces weight limit by 1  
- Low coins limit route access: Can't use paid transport without fare
- Morning attention = 10 - (hunger/20) - ((100-health)/25)

### Connection Tokens
Permanent relationship capital with each NPC:
- **Trust**: Personal relationship strength
- **Commerce**: Professional relationship strength
- **Status**: Social standing with them
- **Shadow**: Shared secrets and illicit trust

Tokens only decrease through explicit betrayal:
- Failed delivery: -2 tokens of letter type with sender
- Queue displacement: -1 token per position jumped with displaced NPC
- Broken promises: -3 tokens of promise type
- Crisis mishandling: -1 all token types

## Attention Economy

Daily allocation of 10 attention (modified by hunger/health):
- **Observation**: 1 attention - gain temporary conversation card
- **Standard Conversation**: 2 attention - full emotional system
- **Quick Exchange**: 0 attention - instant resource trade (merchants only)
- **Work Action**: 2 attention - earn 8 coins, advances one time period
- **Travel**: 0 attention - costs time based on route
- **Wait**: 0 attention - pass time strategically

Morning attention refresh = 10 - (hunger/20) - ((100-health)/25)

## Conversation System

### The Core Choice

Each conversation turn costs 1 patience. Choose:

**LISTEN**: 
- ALL Opportunity cards in hand vanish immediately
- Draw X cards based on emotional state
- Check letter deck eligibility based on state and tokens
- State may transition per rules

**SPEAK**:
- Play cards up to weight limit
- Each card resolves individually
- Effects apply based on success/failure
- State may change from State cards

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

### Emotional States

Nine states that modify Listen/Speak mechanics:

**NEUTRAL**
- Listen: Draw 2 from conversation deck
- Speak: Weight limit 3

**GUARDED**  
- Listen: Draw 1 from conversation deck, state→Neutral
- Speak: Weight limit 2

**OPEN**
- Listen: Draw 3 from conversation deck, check letter deck for trust letters
- Speak: Weight limit 3

**TENSE**
- Listen: Draw 1 from conversation deck, state→Guarded
- Speak: Weight limit 1

**EAGER**
- Listen: Draw 3 from conversation deck
- Speak: Weight limit 3

**OVERWHELMED**
- Listen: Draw 1 from conversation deck, state→Neutral
- Speak: Maximum 1 card regardless of weight

**CONNECTED**
- Listen: Draw 3 from conversation deck, check letter deck for any letters
- Speak: Weight limit 4

**DESPERATE**
- Listen: Draw 2 from conversation deck + inject 1 from crisis deck, state→Hostile
- Speak: Weight limit 3, crisis cards cost 0 weight

**HOSTILE**
- Listen: Draw 1 from conversation deck + inject 2 from crisis deck, conversation ends
- Speak: Only crisis cards playable

Letter deck "checking" means eligible letters are offered as additional draws if token and state requirements are met.

### Patience Calculation

Base patience determined by NPC personality:
- Devoted: 12 patience base
- Mercantile: 10 patience base
- Proud: 8 patience base
- Cunning: 10 patience base
- Steadfast: 11 patience base

Modified by:
- Player hunger: -1 patience per 20 hunger
- Location spot traits: +1 for Private spots, -1 for Public spots
- Burden cards in NPC deck: -1 per burden card

Each conversation turn costs 1 patience. At 0 patience, conversation ends.

## Card System

### Card Anatomy

Every card has:
- **Type**: Comfort/State/Crisis/Burden/Letter/Exchange
- **Depth**: 0-20 (determines comfort requirement to access)
- **Weight**: 0-5 (cognitive load)
- **Token Type**: Trust/Commerce/Status/Shadow
- **Persistence**: Persistent/Opportunity/One-shot/Burden
- **Success Effect**: What happens on success
- **Failure Effect**: What happens on failure

### Card Types

Each card type has strictly separated mechanical effects:

**Comfort Cards**
- Build comfort value on success
- May generate tokens on success
- Cannot change states or create obligations
- Most common in conversation decks

**State Cards**  
- Change emotional state on success
- Must be played alone (cannot combine)
- Never generate comfort or tokens
- Rare in conversation decks

**Letter Cards** (includes Promise cards)
- Create obligations when played
- Success: Favorable terms (deadline, queue position, payment)
- Failure: Unfavorable terms (still get letter/obligation)
- Found in letter deck, checked during Listen based on state

**Crisis Cards**
- Appear through state injection or events
- Often end conversation if failed
- Free weight in Desperate/Hostile states
- Must be resolved to clear crisis deck

**Burden Cards**
- Added through failed obligations
- Must be played when weight permits
- Block hand slot until resolved
- Negative effects when played (lose comfort/tokens)

**Exchange Cards** (Mercantile NPCs only)
- Simple resource trades
- Success: Complete trade as written
- Failure: No trade occurs
- Found only in exchange decks

No card type can have effects from another type's pool. A comfort card NEVER changes states. A state card NEVER gives comfort.

### Persistence Types

- **Persistent**: Remains in hand if not played
- **Opportunity**: ALL vanish if Listen chosen
- **One-shot**: Removed from deck after playing
- **Burden**: Cannot vanish, must be resolved

### Weight System

Weight represents cognitive complexity:
- Weight 0: Simple acknowledgments
- Weight 1: Basic statements
- Weight 2: Complex arguments  
- Weight 3: Elaborate plans
- Weight 5: Crisis thoughts (overwhelming)

Emotional states define weight limits for Speak action. Crisis cards cost 0 weight in Desperate state despite being weight 5, representing adrenaline overriding normal limits.

### Success Calculation

Base success by weight:
- Weight 0: 80%
- Weight 1: 60%
- Weight 2: 50%
- Weight 3: 40%
- Weight 5: 30%

Token modifier:
- Each token adds +5% to cards of that type
- Example: 3 Trust tokens = +15% to all Trust cards
- Linear progression, no thresholds

Clean, visible math. No hidden modifiers.

### Comfort System

Comfort acts as a depth gate for card access:
- Every card has a depth value from 0-20
- When drawing cards, only cards with depth ≤ current comfort are available
- Start every conversation at comfort 5
- Build comfort through playing comfort cards to access deeper, better cards

Depth ranges represent card quality:
- Depth 0-5: Basic cards, minimal effects
- Depth 6-10: Decent cards, token generation
- Depth 11-15: Good cards, multiple tokens
- Depth 16-20: Excellent cards, powerful effects

Comfort is continuous - every point matters as it unlocks the next depth level. No thresholds, just a simple gate mechanism.

## NPC System

### Four Deck Architecture

Each NPC maintains distinct decks:

**Conversation Deck** (20-25 cards)
- Contains: Comfort cards, State cards, Burden cards (when added)
- Cards have depth values 0-20
- Modified by letter deliveries
- Evolves through relationships

**Exchange Deck** (Mercantile NPCs only, 5-10 cards)
- Contains: Exchange cards only
- Simple resource trades
- Static offerings
- Not present for non-merchant NPCs

**Letter Deck** (Variable size)
- Contains: Letter cards (create obligations)
- Checked during Listen based on state and tokens
- Populated through gameplay triggers
- Player negotiates terms when accepting

**Crisis Deck** (Usually empty)
- Contains: Crisis cards only
- When non-empty, forces crisis conversation
- Injected in Desperate/Hostile states
- Must be resolved to clear

### Letter Generation

Letters enter letter deck through:
- Specific conversation cards adding letters
- Delivered letters triggering replies
- Time passage adding routine letters
- Crisis resolution generating urgent letters

Letter eligibility checked when:
- Token threshold met (varies by letter importance)
- Correct emotional state for letter type
- Listen action chosen

Letter card negotiation:
- Player plays letter card like any other card
- Success: Favorable terms (longer deadline, flexible queue position, standard pay)
- Failure: Unfavorable terms (tight deadline, forced position 1, higher pay)
- Either result: Player receives letter and obligation

Queue position negotiation:
- Most letters respect existing queue, enter at lowest available position
- Crisis/Proud personalities attempt position 1
- Player negotiates through card play success/failure
- Failed negotiation forces position 1, displacing all other obligations

### Personality Types

**Devoted** (Family/Clergy)
- 12 patience base
- Trust-focused conversation deck
- Generates trust letters (personal correspondence)

**Mercantile** (Traders)
- 10 patience base
- Commerce-focused conversation deck
- Generates commerce letters (business deals)
- Has exchange deck for quick trades

**Proud** (Nobles)
- 8 patience base
- Status-focused conversation deck
- Generates status letters (formal correspondence)
- Letters often attempt queue position 1

**Cunning** (Spies)
- 10 patience base
- Shadow-focused conversation deck
- Generates shadow letters (secrets)

**Steadfast** (Workers)
- 11 patience base
- Balanced conversation deck
- Generates routine letters (everyday correspondence)

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

- Position 1 MUST be completed first
- New obligations enter at lowest available position
- Crisis obligations attempt position 1
- Maximum 10 obligations

### Queue Displacement

To complete out of order, burn tokens with displaced NPCs:
- Jump 1 position: -1 token with displaced NPC
- Jump 2 positions: -2 tokens with each displaced NPC
- Jump 3 positions: -3 tokens with each displaced NPC

This permanently damages relationships.

### Obligation Types

**Delivery**: Take letter to recipient
**Meeting**: Arrive at location by deadline
**Promise**: Complete specific action
**Crisis**: Urgent obligation, attempts position 1

## Observation System

### Observation Mechanics

- Cost 1 attention at specific spot
- Receive observation card to hand
- Card has three decay states
- Different observations per time period

### Decay States

**Fresh** (0-2 hours): Full effect as written
**Stale** (2-6 hours): Half comfort value
**Expired** (6+ hours): Must discard, unplayable

Models information becoming outdated.

### Observation Cards

Temporary conversation ammunition:
- One-shot persistence type
- Opportunity cards that vanish on Listen
- Provide comfort or state changes
- Specific to contexts

## Travel System

### Routes

Connect locations with:
- Time cost in periods
- Coin cost for some transport
- Access requirements (permits)
- Encounter probability

### Travel Encounters

Use conversation system:
- Bandits: 5-card combat deck
- Guards: Inspection deck
- Merchants: Trade deck

Success passes, failure costs resources.

### Time Periods

Six daily periods:
- Morning (6-10): Refreshes attention
- Midday (10-14): Peak activity
- Afternoon (14-18): Social time
- Evening (18-22): Taverns active
- Night (22-2): Dangerous, most shops closed
- Deep Night (2-6): Very dangerous, city sleeps

Time advances through:
- Travel: Each route costs specific time
- Work actions: Advance one full period (4 hours)
- Rest actions: Skip to next morning
- Wait actions: Choose duration (30 min to full period)
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

## Work and Rest System

### Work Actions

Available at Commercial spots:
- Cost: 2 attention
- Reward: 8 coins
- Time cost: Advances one full period (4 hours)
- Availability: Morning through Evening only

Creates time→money conversion, allowing recovery from low funds at the cost of deadline pressure.

### Rest Options

**Full Rest** (Hospitality spots):
- "Stay at Inn" exchange: 5 coins → Sleep until morning
- Effects: Attention = 10, Hunger = 20, Health +20
- Time advances to next Morning period

**Quick Rest** (Hospitality spots):
- "Short Nap" exchange: 2 coins → Rest 2 hours
- Effects: +3 attention immediately
- Time advances 2 hours

Creates money→attention conversion, allowing extended play at cost.

### Wait Actions

Strategic time advancement:
- Cost: 0 attention
- Options: 30 min, 1 hour, 2 hours, or until next period
- Use: Waiting for NPC availability or time-specific events

## NPC Availability

NPCs have specific availability windows:
- **Merchants**: Morning through Evening (shops closed at night)
- **Tavern NPCs**: Afternoon through Deep Night
- **Guards**: Always available but different shifts affect state
- **Nobles**: Midday through Evening only

Missing an NPC's window requires waiting or finding alternatives.

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

## Core Innovations

### Depth-Gated Conversations
Comfort acts as a continuous gate for card access. Every card has a depth value, and you can only access cards at or below your current comfort level. Build comfort to literally dig deeper into better cards.

### Linear Token Progression
Every token provides +5% success to its card type with no thresholds or breakpoints. Each token matters equally, creating smooth progression.

### Strict Mechanical Separation
Each card type and mechanic has exactly one purpose. Comfort cards never change states. State cards never give tokens. Perfect mechanical clarity.

### Information Decay
Observations naturally expire through three stages (Fresh/Stale/Expired), modeling how information becomes outdated over time.

This is Wayfarer: Where comfort unlocks deeper conversations, trust is earned token by token, and every letter negotiation shapes your journey through a world of mechanical poetry.