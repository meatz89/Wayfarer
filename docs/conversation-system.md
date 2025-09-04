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
9. **No card type filtering** - Emotional states only affect focus and draws

## Card Anatomy

Every card has:
- **Primary Effect**: ONE effect (either fixed or scaling) on success
- **Focus**: 0-6, cost from focus
- **Difficulty**: Easy (70%), Medium (60%), Hard (50%), Very Hard (40%) base success
- **Persistence**: Persistent (60%), Impulse (25%), or Opening (15%)
- **Atmosphere Change** (Optional): ~30% of cards change atmosphere on success
- **Failure Effect** (Optional): Only if different from standard "no effect"
- **On Exhaust** (Optional): Effect when card vanishes unplayed (~20% of non-persistent cards)

## Emotional States

States determine focus capacity and cards drawn. No filtering of card types.

- **Desperate**: 3 focus capacity, draws 1 card
- **Tense**: 4 focus capacity, draws 2 cards
- **Neutral**: 5 focus capacity, draws 2 cards
- **Open**: 5 focus capacity, draws 3 cards
- **Connected**: 6 focus capacity, draws 3 cards

Desperate at -3 flow ends conversation immediately.

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

- Focus determined by emotional state (3-6)
- Each SPEAK spends card focus from pool
- Pool persists across turns
- LISTEN refreshes pool to current maximum
- Can SPEAK multiple turns until depleted
- "Prepared" atmosphere adds +1 to capacity

Focus creates complex decisions with persistence types:
- Impulse high-focus cards need immediate play
- Opening cards must be used before LISTEN refreshes focus
- Managing both creates tight tactical windows

## Card Persistence System

Three types creating different tactical pressures:

### Persistent (60% of deck)
- Remain in hand until played or conversation ends
- Standard cards for reliable plays

### Impulse (25% of deck)
- Removed after SPEAK action if unplayed
- Forces "play now or lose" decisions
- Often on high-focus dramatic cards

### Opening (15% of deck)
- Removed after LISTEN action if unplayed
- Must play before refreshing focus
- Often on utility cards with timing sensitivity

### On Exhaust Effects
~20% of non-persistent cards trigger effects when vanishing unplayed:
- Draw 1-2 cards (compensation for lost opening)
- +1 rapport (minor consolation)
- Add 1 focus (resource compensation)
- Set negative atmosphere (Volatile or Final - consequence for missing the moment)

This creates pressure: Play the risky card or suffer the atmospheric consequence when it exhausts.

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

State progression: [Ends] ← Desperate ← Tense ← Neutral → Open → Connected

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
- Often with on exhaust: Draw 1-2 cards

**Opening** (15% of deck):
- Utility cards (draw, focus-add)
- Timing-sensitive plays
- Often with on exhaust: Same effect but weaker

### On Exhaust Distribution
- ~50% of Impulse cards have on exhaust
- ~30% of Opening cards have on exhaust
- Common effects: Draw 1 card, +1 rapport, Add 1 focus
- Negative effects: Set Volatile atmosphere (missed opening creates tension)
- Severe effects: Set Final atmosphere (critical moment passed, high stakes now)

### Atmosphere Changes

Only ~30% of cards change atmosphere:
- 0-focus setup cards usually have atmosphere change
- 4+ focus cards often set "Final" atmosphere
- Token-associated cards might set "Focused"
- Defensive cards might set "Volatile"

### NPC Deck Composition (20 cards)

- 6 Fixed rapport cards (various focuses, mostly persistent)
- 4 Scaled rapport cards (matching NPC personality, persistent)
- 2 Draw cards (1 focus each, 1 persistent, 1 opening)
- 2 Focus-add cards (2 focus each, opening with on exhaust)
- 3 Setup cards (0 focus with atmosphere, persistent)
- 2 High-focus dramatic cards (impulse with on exhaust)
- 1 Flex slot (varies by NPC type)

## Observation Cards

Always: Focus 1, Persistent, 85% base success (Very Easy)

### Unique Effects (Not Available on Normal Cards)

**Atmosphere Setters**
- Set "Informed" atmosphere
- Set "Exposed" atmosphere
- Set "Synchronized" atmosphere
- Set "Pressured" atmosphere

**Cost Bypasses**
- Next action costs 0 patience
- Next SPEAK costs 0 focus (plays for free)

**Unique Manipulations**
- Rapport = 15 (set to specific value)
- Focus = maximum (instant refresh)

Observation cards NEVER have rapport scaling or standard effects. They represent external knowledge affecting the conversation in ways normal discourse cannot.

## Request Cards

Requests are the win condition for conversations. They are added to hand at conversation start based on chosen conversation type.

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

This creates maximum pressure without special rules - the NPC "asks the question" and you must respond immediately.

### Request Types by Conversation
- **Letter Requests**: Create delivery obligations (fixed terms)
- **Meeting Requests**: Create time-based obligations (fixed terms)
- **Resolution Requests**: Remove burden cards from deck
- **Commerce Requests**: Special trades or exchanges

## Example Generated Cards

### Normal Cards
- **"Setup"** (0 focus, Easy, Persistent): No effect, Atmosphere: Prepared
- **"Simple Rapport"** (1 focus, Easy, Persistent): +1 rapport
- **"Trust Building"** (3 focus, Hard, Persistent): +X rapport where X = Trust tokens
- **"Desperate Plea"** (3 focus, Hard, Impulse): +X rapport where X = (20 - current rapport) ÷ 5, On Exhaust: Draw 1 card
- **"Final Statement"** (5 focus, Very Hard, Impulse): +5 rapport, Atmosphere: Final, On Exhaust: Atmosphere: Final
- **"Quick Opening"** (2 focus, Medium, Opening): +2 rapport, On Exhaust: Atmosphere: Volatile
- **"Interrupt"** (1 focus, Hard, Opening): Atmosphere: Receptive, Failure: -2 rapport, On Exhaust: Atmosphere: Pressured
- **"Timely Insight"** (1 focus, Medium, Opening): Draw 2 cards, On Exhaust: Draw 1 card
- **"Gather Strength"** (2 focus, Medium, Persistent): Add 1 focus

### Tactical Example: Desperate State Trap
In Desperate state (1 card draw, 3 focus capacity), drawing "Interrupt" creates a dilemma:
- **Risk the play**: 50% success for Receptive atmosphere (more cards), but failure risks -2 rapport (making future cards harder)
- **LISTEN instead**: Refreshes focus but exhausts card, setting Pressured atmosphere (-1 focus capacity)
- **The trap**: You need more cards but both options have serious downsides

This emergent situation represents the NPC rambling desperately, giving you a narrow window to redirect the conversation or suffer consequences.

### Observation Cards
- **"Leverage Knowledge"**: Set Informed atmosphere
- **"Reveal Contradiction"**: Set Exposed atmosphere  
- **"Free Action"**: Next action costs 0 patience
- **"Emergency Boost"**: Rapport = 15
- **"Full Recovery"**: Focus = maximum

### Request Cards
- **"Urgent Letter"** (5 focus, Very Hard, Unplayable→Impulse+Opening): Success: Create delivery obligation (fixed terms), Failure: Add burden card
- **"Arrange Meeting"** (6 focus, Very Hard, Unplayable→Impulse+Opening): Success: Schedule meeting (fixed terms), Failure: Add burden card
- **"Make Amends"** (5 focus, Very Hard, Unplayable→Impulse+Opening): Success: Clear burden cards, Failure: Add burden card

## Connection Tokens

Four types: Trust, Commerce, Status, Shadow

- Determine starting rapport (1 token = 1 starting rapport)
- Only gained through successful letter delivery
- Can be burned for queue displacement
- Linear benefit through rapport conversion
- Do not affect draw rules or card availability

Token association in NPCs affects deck composition:
- **Devoted NPCs**: More cards scaling with Trust tokens
- **Mercantile NPCs**: More cards scaling with Commerce tokens
- **Proud NPCs**: More cards scaling with Status tokens
- **Cunning NPCs**: More cards scaling with Shadow tokens