# Wayfarer: Conversation System Architecture

## Core Rules

1. **One card per SPEAK action** - Maintains conversation rhythm
2. **Each card has ONE effect** - Either fixed or scaling, never both
3. **Weight persists until LISTEN** - Creates multi-turn resource management
4. **Atmosphere persists until failure** - Not reset by LISTEN
5. **Comfort ±3 triggers state transition** - Battery resets to 0
6. **Tokens add 5% success linearly** - Only gained through letter delivery
7. **Card persistence varies** - Persistent stays, Fleeting removed on SPEAK, Opportunity removed on LISTEN
8. **No card type filtering** - Emotional states only affect weight and draws

## Card Anatomy

Every card has:
- **Primary Effect**: ONE effect (either fixed or scaling) on success
- **Weight**: 0-6, cost from weight pool
- **Difficulty**: Easy (70%), Medium (60%), Hard (50%), Very Hard (40%) base success
- **Persistence**: Persistent (60%), Fleeting (25%), or Opportunity (15%)
- **Atmosphere Change** (Optional): ~30% of cards change atmosphere on success
- **Failure Effect** (Optional): Only if different from standard "no effect"
- **On Exhaust** (Optional): Effect when card vanishes unplayed (~20% of non-persistent cards)

## Emotional States

States determine weight capacity and cards drawn. No filtering of card types.

- **Desperate**: 3 weight capacity, draws 1 card
- **Tense**: 4 weight capacity, draws 2 cards
- **Neutral**: 5 weight capacity, draws 2 cards
- **Open**: 5 weight capacity, draws 3 cards
- **Connected**: 6 weight capacity, draws 3 cards

Desperate at -3 comfort ends conversation immediately.

## Atmosphere System

Atmosphere affects all actions until changed by a card or cleared by failure. LISTEN does not reset atmosphere.

### Standard Atmospheres (from normal cards)
- **Neutral**: No effect (default after failure)
- **Prepared**: +1 weight capacity all SPEAK actions
- **Receptive**: +1 card all LISTEN actions
- **Focused**: +20% success all cards
- **Patient**: All actions cost 0 patience
- **Volatile**: All comfort changes ±1
- **Pressured**: -1 weight capacity all SPEAK actions
- **Final**: Any failure ends conversation

### Observation-Only Atmospheres
- **Informed**: Next card cannot fail (automatic success)
- **Exposed**: Double all comfort changes
- **Synchronized**: Next card effect happens twice

## Weight System

- Weight pool determined by emotional state (3-6)
- Each SPEAK spends card weight from pool
- Pool persists across turns
- LISTEN refreshes pool to current maximum
- Can SPEAK multiple turns until depleted
- "Prepared" atmosphere adds +1 to capacity

Weight creates complex decisions with persistence types:
- Fleeting high-weight cards need immediate play
- Opportunity cards must be used before LISTEN refreshes weight
- Managing both creates tight tactical windows

## Card Persistence System

Three types creating different tactical pressures:

### Persistent (60% of deck)
- Remain in hand until played or conversation ends
- Standard cards for reliable plays

### Fleeting (25% of deck)
- Removed after SPEAK action if unplayed
- Forces "play now or lose" decisions
- Often on high-weight dramatic cards

### Opportunity (15% of deck)
- Removed after LISTEN action if unplayed
- Must play before refreshing weight
- Often on utility cards with timing sensitivity

### On Exhaust Effects
~20% of non-persistent cards trigger effects when vanishing unplayed:
- Draw 1-2 cards (compensation for lost opportunity)
- +1 comfort (minor consolation)
- Add 1 weight (resource compensation)
- Set negative atmosphere (Volatile or Final - consequence for missing the moment)

This creates pressure: Play the risky card or suffer the atmospheric consequence when it exhausts.

## Comfort Battery

- Range: -3 to +3
- Always starts at 0
- At +3: State shifts right, comfort resets to 0
- At -3: State shifts left, comfort resets to 0
- Excess comfort lost (no banking)

State progression: [Ends] ← Desperate ← Tense ← Neutral → Open → Connected

## Normal Card Generation

### Effect Pools

**Fixed Comfort** (Easy-Medium difficulty)
- +1, +2, +3 comfort
- -1, -2 comfort

**High Fixed Comfort** (Hard-Very Hard difficulty)
- +4, +5 comfort
- -3 comfort

**Scaled Comfort** (Hard difficulty)
- +X where X = Trust tokens (max 5)
- +X where X = Commerce tokens (max 5)
- +X where X = Status tokens (max 5)
- +X where X = Shadow tokens (max 5)
- +X where X = (4 - current comfort)
- +X where X = patience ÷ 3
- +X where X = weight remaining

**Utility Effects** (Medium difficulty)
- Draw 1 card
- Draw 2 cards
- Add 1 weight to pool
- Add 2 weight to pool

### Weight-Effect Correlation

**0 Weight**: Setup cards
- No effect + atmosphere change (Persistent)
- +1 comfort (Easy, Persistent)

**1 Weight**: Basic cards  
- ±1 comfort (Easy, Persistent)
- Draw 1 card (Medium, Opportunity with on exhaust: Draw 1)

**2 Weight**: Standard cards
- ±2 comfort (Medium, mix of Persistent and Opportunity)
- Scaled comfort with low ceiling (Hard, Persistent)
- Add 1 weight (Medium, Opportunity with on exhaust: +1 weight)

**3 Weight**: Powerful cards
- ±3 comfort (Medium, mix of Persistent and Fleeting)
- Scaled comfort with medium ceiling (Hard, Persistent)
- Draw 2 cards (Medium, Opportunity)

**4+ Weight**: Dramatic cards
- ±4 or ±5 comfort (Hard-Very Hard, Fleeting)
- Scaled comfort with high ceiling (Hard, Fleeting with on exhaust)
- Add 2 weight (Medium, Fleeting)

### Persistence Assignment Rules

**Persistent** (60% of deck):
- Basic comfort cards (±1, ±2)
- Setup cards with atmosphere
- Core strategic options

**Fleeting** (25% of deck):
- High weight cards (4+)
- Powerful effects
- Crisis plays
- Often with on exhaust: Draw 1-2 cards

**Opportunity** (15% of deck):
- Utility cards (draw, weight-add)
- Timing-sensitive plays
- Often with on exhaust: Same effect but weaker

### On Exhaust Distribution
- ~50% of Fleeting cards have on exhaust
- ~30% of Opportunity cards have on exhaust
- Common effects: Draw 1 card, +1 comfort, Add 1 weight
- Negative effects: Set Volatile atmosphere (missed opportunity creates tension)
- Severe effects: Set Final atmosphere (critical moment passed, high stakes now)

### Atmosphere Changes

Only ~30% of cards change atmosphere:
- 0-weight setup cards usually have atmosphere change
- 4+ weight cards often set "Final" atmosphere
- Token-associated cards might set "Focused"
- Defensive cards might set "Volatile"

### NPC Deck Composition (20 cards)

- 6 Fixed comfort cards (various weights, mostly persistent)
- 4 Scaled comfort cards (matching NPC personality, persistent)
- 2 Draw cards (1 weight each, 1 persistent, 1 opportunity)
- 2 Weight-add cards (2 weight each, opportunity with on exhaust)
- 3 Setup cards (0 weight with atmosphere, persistent)
- 2 High-weight dramatic cards (fleeting with on exhaust)
- 1 Flex slot (varies by NPC type)

## Observation Cards

Always: Weight 1, Persistent, 85% base success (Very Easy)

### Unique Effects (Not Available on Normal Cards)

**Atmosphere Setters**
- Set "Informed" atmosphere
- Set "Exposed" atmosphere
- Set "Synchronized" atmosphere
- Set "Pressured" atmosphere

**Cost Bypasses**
- Next action costs 0 patience
- Next SPEAK costs 0 weight (plays for free)

**Unique Manipulations**
- Comfort = 0 (force reset)
- Weight pool = maximum (instant refresh)

Observation cards NEVER have comfort scaling or standard effects. They represent external knowledge affecting the conversation in ways normal discourse cannot.

## Goal Cards

Goals are the win condition for conversations. They appear based on conversation type chosen.

### Goal Properties
- **Weight**: 5-6 (requires maximum state or Prepared atmosphere)
- **Difficulty**: Very Hard (30-40% base success)
- **Persistence**: BOTH Fleeting AND Opportunity (exhausts on any action if unplayed)
- **On Exhaust**: End conversation in failure
- **Success Effect**: End conversation with favorable terms
- **Failure Effect**: End conversation with unfavorable terms

### Goal Pressure
Having both Fleeting and Opportunity means:
- Cannot SPEAK something else (Fleeting would trigger)
- Cannot LISTEN for more weight (Opportunity would trigger)
- Must play immediately or conversation fails

This creates maximum pressure without special rules - the goal demands immediate action using existing persistence mechanics.

### Goal Types by Conversation
- **Letter Goals**: Create delivery obligations
- **Meeting Goals**: Create time-based obligations
- **Resolution Goals**: Remove burden cards from deck
- **Commerce Goals**: Special trades or exchanges

## Example Generated Cards

### Normal Cards
- **"Setup"** (0 weight, Easy, Persistent): No effect, Atmosphere: Prepared
- **"Simple Comfort"** (1 weight, Easy, Persistent): +1 comfort
- **"Trust Building"** (3 weight, Hard, Persistent): +X comfort where X = Trust tokens
- **"Desperate Plea"** (3 weight, Hard, Fleeting): +X comfort where X = 4 - current comfort, On Exhaust: Draw 1 card
- **"Final Statement"** (5 weight, Very Hard, Fleeting): +5 comfort, Atmosphere: Final, On Exhaust: Atmosphere: Final
- **"Quick Opening"** (2 weight, Medium, Opportunity): +2 comfort, On Exhaust: Atmosphere: Volatile
- **"Interrupt"** (1 weight, Hard, Opportunity): Atmosphere: Receptive, Failure: -2 comfort, On Exhaust: Atmosphere: Pressured
- **"Timely Insight"** (1 weight, Medium, Opportunity): Draw 2 cards, On Exhaust: Draw 1 card
- **"Gather Strength"** (2 weight, Medium, Persistent): Add 1 weight

### Tactical Example: Desperate State Trap
In Desperate state (1 card draw, 3 weight capacity), drawing "Interrupt" creates a dilemma:
- **Risk the play**: 50% success for Receptive atmosphere (more cards), but failure risks -2 comfort (potentially ending conversation at -3)
- **LISTEN instead**: Refreshes weight but exhausts card, setting Pressured atmosphere (-1 weight capacity)
- **The trap**: You need more cards but both options have serious downsides

This emergent situation represents the NPC rambling desperately, giving you a narrow window to redirect the conversation or suffer consequences.

### Observation Cards
- **"Leverage Knowledge"**: Set Informed atmosphere
- **"Reveal Contradiction"**: Set Exposed atmosphere  
- **"Free Action"**: Next action costs 0 patience
- **"Emergency Reset"**: Comfort = 0
- **"Full Recovery"**: Weight pool = maximum

### Goal Cards
- **"Urgent Letter"** (5 weight, Very Hard, Fleeting + Opportunity): Success: Create delivery obligation, On Exhaust: End conversation in failure
- **"Arrange Meeting"** (6 weight, Very Hard, Fleeting + Opportunity): Success: Schedule meeting, On Exhaust: End conversation in failure
- **"Make Amends"** (5 weight, Very Hard, Fleeting + Opportunity): Success: Clear burden cards, On Exhaust: End conversation in failure

## Connection Tokens

Four types: Trust, Commerce, Status, Shadow

- Universal effect: +5% success per token on ALL cards
- Only gained through successful letter delivery
- Can go negative from failures
- Linear scaling, no thresholds
- Do not affect draw rules or card availability

Token association in NPCs affects deck composition:
- **Devoted NPCs**: More cards scaling with Trust
- **Mercantile NPCs**: More cards scaling with Commerce
- **Proud NPCs**: More cards scaling with Status
- **Cunning NPCs**: More cards scaling with Shadow