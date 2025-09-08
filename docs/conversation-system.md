# Wayfarer: Conversation System Architecture

## Core Rules

1. **One card per SPEAK action** - Maintains conversation rhythm
2. **Each card has ONE effect** - Either fixed or scaling, never both
3. **Focus persists until LISTEN** - Creates multi-turn resource management
4. **Atmosphere persists until failure** - Not reset by LISTEN
5. **Flow ±3 triggers state transition** - Battery resets to 0
6. **Success adds +1 flow, failure -1 flow** - Deterministic flow progression
7. **Rapport modifies success linearly** - +2% per point, -50 to +50 range
8. **Card persistence varies** - Persistent stays, Impulse removed on SPEAK, Opening removed on LISTEN
9. **No card type filtering** - Connection States only affect focus and draws
10. **Draw pile cycles through exhaust** - When draw pile empty, shuffle exhaust pile to create new draw pile

## NPC Persistent Decks

Each NPC maintains multiple persistent decks that determine available conversation types:

### Conversation Card Deck
- Contains NPC's standard 20 conversation cards
- Always present for all NPCs
- Enables "Standard Conversation" option

### Goal Card Deck
- Contains letter requests, promise requests, meeting requests
- If NPC has goals available: Enables specific conversation types
- Example: Elena with marriage refusal letter enables "Crisis Letter" conversation

### Observation Card Deck
- Contains cards from player's location observations
- Cards added when player observes locations (specific cards go to specific NPCs)
- Mixed into draw pile for any conversation type with this NPC
- Provides conversation advantages

### Burden Card Deck
- Contains burden cards from failed requests or broken promises
- If contains cards: Enables "Make Amends" conversation
- Each burden card makes resolution harder

### Exchange Deck (Mercantile NPCs only)
- Contains quick exchange options
- Enables "Quick Exchange" conversation (1 attention)
- No card play, just resource trades

## Conversation Types

Available types determined by NPC's deck contents and player state:

### Standard Conversation
- Available: Always (if NPC has conversation deck)
- Cards used: Conversation deck + observation deck
- Purpose: Build rapport, gain tokens through promises

### Letter Request
- Available: If NPC has letter in goal deck
- Cards used: Conversation deck + observation deck + specific request card
- Purpose: Accept letter delivery obligation

### Letter Delivery
- Available: If player has letter for this NPC in satchel
- Cards used: Minimal deck or quick conversation
- Purpose: Complete delivery, gain tokens

### Make Amends
- Available: If NPC has burden cards
- Cards used: Conversation deck + observation deck + resolution request
- Purpose: Clear burden cards from relationship

### Quick Exchange
- Available: If NPC has exchange deck
- Cards used: None (simple transaction)
- Purpose: Trade resources efficiently

## Three-Pile System

During any conversation, cards exist in three piles:

### Draw Pile
- Created at conversation start from relevant NPC decks
- Cards drawn from here to active pile
- When empty and need to draw: Shuffle exhaust pile to create new draw pile

### Active Pile (Hand)
- Cards currently available to play
- Maximum size determined by connection state draws
- Can exceed maximum through special effects

### Exhaust Pile
- Played cards go here after use
- Exhausted cards (Impulse/Opening) go here when removed
- Shuffled to become draw pile when draw pile empty

## Conversation Flow

### Conversation Start
1. Player chooses conversation type (based on available options)
2. Relevant cards from NPC decks added to draw pile:
   - All cards from conversation deck
   - All cards from observation deck (if any)
   - Relevant request card (if applicable)
3. Draw pile shuffled
4. Initial draw: Draw cards equal to connection state to active pile
5. Focus set to connection state maximum

### LISTEN Action
1. Costs 1 patience (unless Patient atmosphere)
2. Draw cards from draw pile to active pile (amount by connection state)
3. If draw pile has fewer cards than needed: Draw what's available
4. If draw pile empty and need more: Shuffle exhaust pile → draw pile → continue drawing
5. Refresh focus to connection state maximum
6. Remove Opening cards if unplayed (to exhaust pile)
7. Check if request cards become playable

### SPEAK Action
1. Choose one card from active pile
2. Spend focus equal to card cost
3. Resolve success/failure
4. Card goes to exhaust pile
5. Apply card effects
6. Remove Impulse cards if unplayed (to exhaust pile)

### Conversation End
1. All piles cleared
2. NPC persistent decks unchanged (except consumed observation cards)
3. Rapport resets
4. Atmosphere clears

## Connection States

States determine focus capacity and cards drawn. No filtering of card types.

- **Disconnected**: 3 focus capacity, draws 1 card
- **Guarded**: 4 focus capacity, draws 2 cards
- **Neutral**: 5 focus capacity, draws 2 cards
- **Receptive**: 5 focus capacity, draws 3 cards
- **Trusting**: 6 focus capacity, draws 3 cards

Desperate at -3 flow ends conversation immediately.

## Connection State Advancement

Certain cards can directly change connection state:
- Card effect specifies target state (e.g., "Advances to Neutral")
- Overrides current state immediately
- Flow resets to 0 when state changes this way
- Typically found on observation cards

## Atmosphere System

Atmosphere affects all actions until changed by a card or cleared by failure. LISTEN does not reset atmosphere.

### Standard Atmospheres (from normal cards)
- **Neutral**: No effect (default after failure)
- **Prepared**: +1 focus capacity all SPEAK actions
- **Receptive**: +1 card all LISTEN actions
- **Focused**: +20% success all cards
- **Patient**: All actions cost 0 patience
- **Volatile**: All rapport changes ±1
- **Pressured**: -1 focus capacity all SPEAK actions
- **Final**: Any failure ends conversation

### Observation-Only Atmospheres
- **Informed**: Next card cannot fail (automatic success)
- **Exposed**: Double all rapport changes
- **Synchronized**: Next card effect happens twice

## Focus System

- Focus determined by connection state (3-6)
- Each SPEAK spends card focus from pool
- Pool persists across turns
- LISTEN refreshes pool to current maximum
- Can SPEAK multiple turns until depleted
- "Prepared" atmosphere adds +1 to capacity

## Card Persistence System

Three types creating different tactical pressures:

### Persistent (60% of deck)
- Remain in active pile until played or conversation ends
- Standard cards for reliable plays

### Impulse (25% of deck)
- Removed after any SPEAK action if unplayed (to exhaust pile)
- Forces "play now or lose" decisions
- Often on high-focus dramatic cards

### Opening (15% of deck)
- Removed after LISTEN action if unplayed (to exhaust pile)
- Must play before refreshing focus
- Often on utility cards with timing sensitivity

### On Exhaust Effects
~20% of non-persistent cards trigger effects when removed unplayed:
- Draw 1-2 cards (from draw pile)
- +1 rapport (minor consolation)
- Add 1 focus (resource compensation)
- Set negative atmosphere (consequence for missing moment)

## Rapport System

- Range: -50 to +50
- Starts at value equal to connection tokens with NPC
- Modified by card effects during conversation
- Each point provides +2% success to ALL cards
- Resets when conversation ends
- Can go negative (creating downward spiral risk)

Starting with 5 tokens = 5 rapport = +10% success on all cards from start.

## Flow Battery

- Range: -3 to +3
- Always starts at 0
- Success on SPEAK: +1 flow
- Failure on SPEAK: -1 flow
- At +3: State shifts right, flow resets to 0
- At -3: State shifts left, flow resets to 0
- Excess flow lost (no banking)

State progression: [Ends] ← Disconnected ← Guarded ← Neutral → Receptive → Trusting

## Normal Card Generation

### Effect Pools

**Fixed Rapport** (Easy-Medium difficulty)
- +1, +2, +3 rapport
- -1, -2 rapport

**High Fixed Rapport** (Hard-Very Hard difficulty)
- +4, +5 rapport
- -3 rapport

**Scaled Rapport** (Hard difficulty)
- +X where X = Trust tokens (max 5)
- +X where X = Commerce tokens (max 5)
- +X where X = Status tokens (max 5)
- +X where X = Shadow tokens (max 5)
- +X where X = (20 - current rapport) ÷ 5
- +X where X = patience ÷ 3
- +X where X = focus remaining

**Utility Effects** (Medium difficulty)
- Draw 1 card
- Draw 2 cards
- Add 1 focus to pool
- Add 2 focus to pool

### Focus-Effect Correlation

**0 Focus**: Setup cards
- No effect + atmosphere change (Persistent)
- +1 rapport (Easy, Persistent)

**1 Focus**: Basic cards  
- ±1 rapport (Easy, Persistent)
- Draw 1 card (Medium, Opening with on exhaust: Draw 1)

**2 Focus**: Standard cards
- ±2 rapport (Medium, mix of Persistent and Opening)
- Scaled rapport with low ceiling (Hard, Persistent)
- Add 1 focus (Medium, Opening with on exhaust: +1 focus)

**3 Focus**: Powerful cards
- ±3 rapport (Medium, mix of Persistent and Impulse)
- Scaled rapport with medium ceiling (Hard, Persistent)
- Draw 2 cards (Medium, Opening)

**4+ Focus**: Dramatic cards
- ±4 or ±5 rapport (Hard-Very Hard, Impulse)
- Scaled rapport with high ceiling (Hard, Impulse with on exhaust)
- Add 2 focus (Medium, Impulse)

### Persistence Assignment Rules

**Persistent** (60% of deck):
- Basic rapport cards (±1, ±2)
- Setup cards with atmosphere
- Core strategic options

**Impulse** (25% of deck):
- High focus cards (4+)
- Powerful effects
- Crisis plays
- Often with on exhaust effects

**Opening** (15% of deck):
- Utility cards (draw, focus-add)
- Timing-sensitive plays
- Often with on exhaust effects

### Atmosphere Changes

Only ~30% of cards change atmosphere:
- 0-focus setup cards usually have atmosphere change
- 4+ focus cards often set "Final" atmosphere
- Token-associated cards might set "Focused"
- Defensive cards might set "Volatile"

### NPC Deck Composition (20 cards)

Standard conversation deck contains:
- 6 Fixed rapport cards (various focuses, mostly persistent)
- 4 Scaled rapport cards (matching NPC personality, persistent)
- 2 Draw cards (1 focus each, 1 persistent, 1 opening)
- 2 Focus-add cards (2 focus each, opening with on exhaust)
- 3 Setup cards (0 focus with atmosphere, persistent)
- 2 High-focus dramatic cards (impulse with on exhaust)
- 1 Flex slot (varies by NPC type)

## Observation Cards

Generated by location observations, placed in NPC observation decks:

### Properties
- Focus 0 (special SPEAK action)
- Always Persistent
- Mixed into draw pile at conversation start
- Consumed when played (removed from NPC's observation deck)

### Unique Effects (Not Available on Normal Cards)

**Connection State Changes**
- Advance to specific state (e.g., "Advances Elena to Neutral")
- Reset flow to 0

**Exchange Unlocks**
- Enable special NPC exchanges
- Make hidden options visible

**Atmosphere Setters**
- Set special atmospheres (Informed, Exposed, Synchronized)

**Cost Bypasses**
- Next action costs 0 patience
- Next SPEAK costs 0 focus

Observation cards represent external knowledge affecting the conversation in ways normal discourse cannot.

## Request Cards

Requests are the win condition for conversations. Added to draw pile based on chosen conversation type.

### Request Properties
- **Focus**: 5-6 (requires maximum state or Prepared atmosphere)
- **Difficulty**: Very Hard (30-40% base success)
- **Starting State**: Unplayable (cannot be played regardless of focus)
- **Becomes Playable**: When LISTEN at sufficient focus capacity
- **When Playable**: Gains BOTH Impulse AND Opening
- **On Exhaust**: Conversation ends in failure
- **Success Effect**: Accept obligation with fixed terms
- **Failure Effect**: Add burden card to relationship

### Request Pressure
When the request becomes playable (after LISTEN at correct focus):
- Gains Impulse: Will be discarded if you SPEAK something else
- Gains Opening: Will be discarded if you LISTEN again
- Must play immediately or conversation fails

### Request Types
- **Letter Requests**: Create delivery obligations (fixed terms)
- **Meeting Requests**: Create time-based obligations (fixed terms)
- **Resolution Requests**: Remove burden cards from deck
- **Commerce Requests**: Special trades or exchanges

## Example Cards

### Normal Conversation Cards
- **"Simple Rapport"** (1 focus, Easy, Persistent): +1 rapport
- **"Trust Building"** (3 focus, Hard, Persistent): +X rapport where X = Trust tokens
- **"Desperate Plea"** (3 focus, Hard, Impulse): +X rapport where X = (20 - current rapport) ÷ 5
- **"Setup Atmosphere"** (0 focus, Easy, Persistent): No effect, Atmosphere: Prepared

### Observation Cards (in NPC observation decks)
- **"Safe Passage Knowledge"** (0 focus, Persistent): Advances Elena to Neutral state
- **"Merchant Caravan Route"** (0 focus, Persistent): Unlocks Marcus's caravan exchange
- **"Noble Introduction"** (0 focus, Persistent): Sets rapport to 15

### Request Cards (in goal decks)
- **"Elena's Letter"** (5 focus, Very Hard): Accept urgent delivery (1hr deadline, position 1)
- **"Marcus's Trade"** (6 focus, Hard): Accept commerce obligation

## Connection Tokens

Four types: Trust, Commerce, Status, Shadow

- Determine starting rapport (1 token = 1 starting rapport)
- Only gained through successful letter delivery
- Can be burned for queue displacement
- Can gate special exchanges (minimum token requirements)
- Do not affect draw rules or card availability

Token association in NPCs affects deck composition:
- **Devoted NPCs**: More Trust-scaling cards
- **Mercantile NPCs**: More Commerce-scaling cards
- **Proud NPCs**: More Status-scaling cards
- **Cunning NPCs**: More Shadow-scaling cards

## Quick Exchange Conversations

Simplified mechanics for basic transactions:
- Cost 1 attention instead of 2
- No card play (no draw/active/exhaust piles)
- Simple resource trades from exchange deck
- Still advance game time
- Used when full conversation unnecessary

Examples:
- Simple purchases from merchants
- Quick deliveries to busy NPCs
- Information trades
- Permit purchases

## Deck Cycling Example

Turn 1: Draw pile has 23 cards (20 conversation + 2 observation + 1 request), active pile has 2 cards
- SPEAK a card → goes to exhaust pile

Turn 2: Draw pile has 21 cards, active has 1, exhaust has 1
- LISTEN → draw 2 more cards from draw pile

Turn 15: Draw pile empty, active has 3 cards, exhaust has 20 cards
- LISTEN → need to draw 2 cards
- Shuffle exhaust pile (20 cards) → becomes new draw pile
- Draw 2 cards from new draw pile → active pile

This creates natural deck cycling where all cards remain available throughout the conversation.