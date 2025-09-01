# Wayfarer: Conversation System Architecture

## Core Rules

1. **One card per SPEAK action** - Maintains conversation rhythm
2. **Each card has ONE effect** - Either fixed or scaling, never both
3. **Weight persists until LISTEN** - Creates multi-turn resource management
4. **Atmosphere persists until failure** - Not reset by LISTEN
5. **Comfort ±3 triggers state transition** - Battery resets to 0
6. **Tokens add 5% success linearly** - Only gained through letter delivery
7. **Fleeting cards removed on SPEAK** - LISTEN preserves them
8. **No card type filtering** - Emotional states only affect weight and draws

## Card Anatomy

Every card has:
- **Primary Effect**: ONE effect, either fixed or scaling
- **Weight**: 0-6, cost from weight pool
- **Difficulty**: Easy (70%), Medium (60%), Hard (50%), Very Hard (40%) base success
- **Persistence**: Fleeting (25% of deck) or Persistent (75% of deck)
- **Atmosphere Change** (Optional): ~30% of cards change atmosphere

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
- **Final**: Any failure ends conversation

### Observation-Only Atmospheres
- **Informed**: Next card cannot fail (automatic success)
- **Exposed**: Double all comfort changes
- **Synchronized**: Next card effect happens twice
- **Pressured**: -1 card on all LISTEN actions

## Weight System

- Weight pool determined by emotional state (3-6)
- Each SPEAK spends card weight from pool
- Pool persists across turns
- LISTEN refreshes pool to current maximum
- Can SPEAK multiple turns until depleted
- "Prepared" atmosphere adds +1 to capacity

Weight creates the core resource management puzzle, especially with fleeting cards requiring more weight than currently available.

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
- No effect + atmosphere change
- +1 comfort (Easy)

**1 Weight**: Basic cards  
- ±1 comfort (Easy)
- Draw 1 card (Medium)

**2 Weight**: Standard cards
- ±2 comfort (Medium)
- Scaled comfort with low ceiling (Hard)
- Add 1 weight (Medium)

**3 Weight**: Powerful cards
- ±3 comfort (Medium)
- Scaled comfort with medium ceiling (Hard)
- Draw 2 cards (Medium)

**4+ Weight**: Dramatic cards
- ±4 or ±5 comfort (Hard-Very Hard)
- Scaled comfort with high ceiling (Hard)
- Add 2 weight (Medium)
- Usually fleeting

### Atmosphere Changes

Only ~30% of cards change atmosphere:
- 0-weight setup cards usually have atmosphere change
- 4+ weight cards often set "Final" atmosphere
- Token-associated cards might set "Focused"
- Defensive cards might set "Volatile"

### NPC Deck Composition (20 cards)

- 6 Fixed comfort cards (various weights)
- 4 Scaled comfort cards (matching NPC personality)
- 2 Draw cards (1 weight each)
- 2 Weight-add cards (2 weight each)
- 3 Setup cards (0 weight with atmosphere)
- 2 High-weight dramatic cards (fleeting)
- 1 Flex slot

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
- **Persistence**: Fleeting with "Final Word" property
- **State Requirements**: NONE (playable in any state with weight)
- **Effect**: Ends conversation, success determines obligation terms

### "Final Word" Property
When a fleeting goal card would be discarded (not played during SPEAK), conversation immediately ends in failure. This creates natural pressure without special rules.

### Goal Challenge Layers
1. Need state with 5+ weight capacity (or Prepared atmosphere)
2. Must draw the goal card
3. Need tokens for reasonable success chance (30% + tokens × 5%)
4. Must play before next SPEAK or lose conversation

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
- **"Desperate Plea"** (3 weight, Hard, Fleeting): +X comfort where X = 4 - current comfort
- **"Final Statement"** (5 weight, Very Hard, Fleeting): +5 comfort, Atmosphere: Final
- **"Quick Thought"** (1 weight, Medium, Persistent): Draw 1 card
- **"Gather Strength"** (2 weight, Medium, Persistent): Add 1 weight

### Observation Cards
- **"Leverage Knowledge"**: Set Informed atmosphere
- **"Reveal Contradiction"**: Set Exposed atmosphere  
- **"Free Action"**: Next action costs 0 patience
- **"Emergency Reset"**: Comfort = 0
- **"Full Recovery"**: Weight pool = maximum

### Goal Cards
- **"Urgent Letter"** (5 weight, Very Hard, Fleeting + Final Word): End conversation, create delivery
- **"Arrange Meeting"** (6 weight, Very Hard, Fleeting + Final Word): End conversation, schedule meeting
- **"Make Amends"** (5 weight, Very Hard, Fleeting + Final Word): End conversation, clear burdens

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