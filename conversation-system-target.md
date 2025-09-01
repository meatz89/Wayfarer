# Wayfarer: Conversation System - Target Design

## Overview
This document describes the complete target state for the Wayfarer conversation system after implementing all refinements. This is the authoritative reference for how the system should work when fully implemented.

## Core Mechanics

### Fundamental Rules
1. **One card per SPEAK action** - Players select and play exactly ONE card when they SPEAK
2. **Each card has ONE effect** - Either fixed or scaling, never both
3. **Weight persists until LISTEN** - Weight pool spent on SPEAK persists across turns, allowing multiple SPEAKs
4. **Atmosphere persists until failure** - Atmospheric effects continue until a card fails
5. **Comfort ±3 triggers state transition** - Comfort battery ranges -3 to +3, transitions occur at boundaries
6. **Tokens add 5% success linearly** - Each token adds exactly 5% to success chance
7. **Fleeting cards removed on SPEAK** - All fleeting cards discarded after SPEAK action
8. **No card type filtering** - Emotional states only affect weight capacity and draw count

## Card Structure

### Card Properties
Every conversation card contains:
- **Id**: Unique identifier
- **Name**: Display name
- **Weight**: 0-6, cost from weight pool
- **Difficulty**: Determines base success rate
  - Easy: 70% base success
  - Medium: 60% base success
  - Hard: 50% base success
  - Very Hard: 40% base success
- **Persistence**: 
  - Fleeting (25% of deck): Removed after SPEAK
  - Persistent (75% of deck): Stays in deck
- **Effect Type**: ONE primary effect
  - FixedComfort: +X or -X comfort
  - ScaledComfort: Comfort based on formula
  - DrawCards: Draw X additional cards
  - AddWeight: Add X to weight pool
  - SetAtmosphere: Change current atmosphere
  - ResetComfort: Set comfort to 0 (observation only)
  - MaxWeight: Set weight to capacity (observation only)
  - FreeAction: Next action costs 0 (observation only)
- **Effect Value/Formula**: Parameter for the effect
- **Atmosphere Change** (Optional): ~30% of cards change atmosphere
- **Has Final Word** (Goal cards only): If not played when fleeting, conversation fails
- **Dialogue Text**: What the player says when playing this card

## Emotional States

### Five Core States
States form a linear progression with specific mechanical effects:

1. **DESPERATE**
   - Weight capacity: 3
   - Cards drawn: 1
   - Special: At -3 comfort, conversation ends immediately

2. **TENSE**
   - Weight capacity: 4
   - Cards drawn: 2

3. **NEUTRAL**
   - Weight capacity: 5
   - Cards drawn: 2
   - Starting state for all conversations

4. **OPEN**
   - Weight capacity: 5
   - Cards drawn: 3

5. **CONNECTED**
   - Weight capacity: 6
   - Cards drawn: 3
   - Maximum positive state

### State Transitions
- **Comfort +3**: State shifts right (toward Connected), comfort resets to 0
- **Comfort -3**: State shifts left (toward Desperate), comfort resets to 0
- **Linear progression**: [Ends] ← Desperate ← Tense ← Neutral → Open → Connected
- **No banking**: Excess comfort beyond ±3 is lost

## Weight System

### Weight Pool Mechanics
- **Capacity determined by state**:
  - Desperate: 3
  - Tense: 4
  - Neutral: 5
  - Open: 5
  - Connected: 6
- **Persistence**: Weight spent persists across turns
- **Refresh on LISTEN**: Pool refills to current capacity
- **Multiple SPEAKs**: Can SPEAK multiple times until weight depleted
- **Prepared atmosphere**: Adds +1 to capacity

### Resource Management
The weight system creates strategic depth:
- High-weight cards require better emotional states
- Multiple low-weight cards vs one high-weight card
- Fleeting cards may require more weight than available
- Weight-add cards enable combo plays

## Atmosphere System

### Standard Atmospheres (30% of normal cards)
- **Neutral**: No effect (default, set after any failure)
- **Prepared**: +1 weight capacity on all SPEAK actions
- **Receptive**: +1 card on all LISTEN actions
- **Focused**: +20% success on all cards
- **Patient**: All actions cost 0 patience
- **Volatile**: All comfort changes ±1
- **Final**: Any failure ends conversation immediately

### Observation-Only Atmospheres
These powerful effects only come from observation cards:
- **Informed**: Next card cannot fail (automatic success)
- **Exposed**: Double all comfort changes
- **Synchronized**: Next card effect happens twice
- **Pressured**: -1 card on all LISTEN actions

### Atmosphere Rules
- Persists until changed by another card or cleared by failure
- LISTEN does NOT reset atmosphere
- Failure always resets to Neutral
- Effects apply to all relevant actions while active

## Comfort Battery

### Battery Mechanics
- **Range**: -3 to +3
- **Starting value**: Always 0
- **Transitions**: At ±3, state changes and battery resets to 0
- **No accumulation**: Cannot exceed ±3
- **Visual representation**: 7-point scale displayed to player

### Comfort Modifications
- Base comfort change from card effect
- Volatile atmosphere: ±1 to all changes
- Exposed atmosphere: Double all changes
- Effects stack multiplicatively

## Success System

### Success Calculation
```
Base Success = Difficulty percentage (70/60/50/40)
Token Bonus = Total tokens × 5%
Atmosphere Bonus = Focused ? 20% : 0
Final Success = Clamp(Base + Token + Atmosphere, 5%, 95%)
```

### Dice Roll
- Roll 1-100
- Success if roll ≤ Final Success
- Always show percentage to player before playing card
- Informed atmosphere bypasses roll (automatic success)

## Card Types and Generation

### Normal Card Composition (20 cards per NPC)
- 6 Fixed comfort cards (various weights)
- 4 Scaled comfort cards (based on NPC personality)
- 2 Draw cards (weight 1 each)
- 2 Weight-add cards (weight 2 each)
- 3 Setup cards (weight 0 with atmosphere)
- 2 High-weight dramatic cards (fleeting)
- 1 Flex slot

### Weight-Effect Correlations

**Weight 0: Setup cards**
- No effect + atmosphere change
- +1 comfort (Easy)

**Weight 1: Basic cards**
- ±1 comfort (Easy)
- Draw 1 card (Medium)

**Weight 2: Standard cards**
- ±2 comfort (Medium)
- Scaled comfort with low ceiling (Hard)
- Add 1 weight (Medium)

**Weight 3: Powerful cards**
- ±3 comfort (Medium)
- Scaled comfort with medium ceiling (Hard)
- Draw 2 cards (Medium)

**Weight 4+: Dramatic cards**
- ±4 or ±5 comfort (Hard-Very Hard)
- Scaled comfort with high ceiling (Hard)
- Add 2 weight (Medium)
- Usually fleeting

### Scaled Comfort Formulas
- `trust_tokens`: Comfort = Trust token count
- `commerce_tokens`: Comfort = Commerce token count
- `status_tokens`: Comfort = Status token count
- `shadow_tokens`: Comfort = Shadow token count
- `inverse_comfort`: Comfort = 4 - |current comfort|
- `patience_third`: Comfort = Patience ÷ 3
- `weight_remaining`: Comfort = Current weight pool

## Observation Cards

### Universal Properties
- Weight: Always 1
- Difficulty: Always Very Easy (85% success)
- Persistence: Always Persistent
- Never have standard comfort effects

### Unique Effects
Observation cards provide effects unavailable on normal cards:

**Atmosphere Setters**
- Set Informed atmosphere (next auto-succeeds)
- Set Exposed atmosphere (double comfort)
- Set Synchronized atmosphere (next effect x2)
- Set Pressured atmosphere (-1 card on LISTEN)

**Cost Bypasses**
- Next action costs 0 patience
- Next SPEAK costs 0 weight

**Unique Manipulations**
- Comfort = 0 (reset battery)
- Weight = maximum (instant refresh)

## Goal Cards

### Properties
- **Weight**: 5-6 (requires maximum states or Prepared)
- **Difficulty**: Very Hard (30-40% base success)
- **Persistence**: Fleeting with "Final Word" property
- **State Requirements**: NONE (only weight matters)
- **Effect**: Ends conversation, success determines outcome

### Final Word Mechanic
When a fleeting goal card would be discarded without being played:
1. Conversation immediately ends
2. Marked as failure
3. No obligation created
4. Potential relationship damage

This creates natural urgency without special timer rules.

### Goal Types by Conversation
- **Letter Goals**: Create delivery obligations
- **Meeting Goals**: Create time-based obligations  
- **Resolution Goals**: Remove burden cards from deck
- **Commerce Goals**: Enable special trades

## Action Flow

### LISTEN Action
1. **Refresh weight pool** to current capacity
2. **Calculate draw count** based on emotional state
3. **Apply atmosphere modifiers**:
   - Receptive: +1 card
   - Pressured: -1 card
4. **Draw cards** (no type filtering)
5. **Preserve fleeting cards** in hand
6. **Cost**: 1 patience (unless Patient atmosphere)

### SPEAK Action
1. **Select ONE card** from hand
2. **Check weight** availability
3. **Calculate success** percentage
4. **Show percentage** to player
5. **Roll dice** for success
6. **Apply effects** if successful
7. **Clear atmosphere** if failed
8. **Spend weight** from pool
9. **Remove ALL fleeting cards** from hand
10. **Check Final Word** for unplayed goals
11. **Cost**: 1 patience (unless Patient atmosphere)

## Token System

### Token Types
- **Trust**: Personal bonds
- **Commerce**: Professional dealings
- **Status**: Social standing
- **Shadow**: Shared secrets

### Token Effects
- Universal: +5% success per token on ALL cards
- Linear scaling with no thresholds
- Can go negative (relationship debt)
- Maximum practical benefit: +25% (5 tokens)

### Token Sources
- Successful letter delivery: +1 token
- Failed delivery: -2 tokens with sender
- Queue displacement: -X tokens based on positions
- Special goal cards: Variable tokens

## NPC Personality Integration

### Deck Composition by Type
- **Devoted NPCs**: More Trust-scaling cards
- **Mercantile NPCs**: More Commerce-scaling cards
- **Proud NPCs**: More Status-scaling cards
- **Cunning NPCs**: More Shadow-scaling cards

### Personality Affects
- Which scaled comfort cards appear
- Token preference for rewards
- Goal card types available
- Exchange deck composition (merchants only)

## UI Requirements

### Essential Visual Elements

**Weight Pool Display**
- Show current/maximum as dots or bar
- Highlight spent vs available
- Indicate Prepared atmosphere bonus

**Comfort Battery**
- 7-point scale (-3 to +3)
- Current position clearly marked
- Threshold indicators at ±3

**Atmosphere Indicator**
- Current atmosphere name
- Effect description
- Persistent indicator (not clearing each turn)

**Card Display**
- Weight cost prominently shown
- Success percentage before playing
- Difficulty indicator
- Persistence type (fleeting marker)
- Effect description

**State Display**
- Current emotional state
- Weight capacity for state
- Draw count for state

## Implementation Validation Checklist

### Core Mechanics
- [ ] SPEAK plays exactly ONE card
- [ ] Weight pool persists between turns
- [ ] LISTEN refreshes weight to capacity
- [ ] Can SPEAK multiple times if weight available

### Atmosphere
- [ ] Persists until failure or card change
- [ ] LISTEN does not reset atmosphere
- [ ] Effects apply correctly
- [ ] Observation atmospheres work

### Comfort Battery
- [ ] Range limited to -3 to +3
- [ ] Transitions trigger at exactly ±3
- [ ] Battery resets to 0 after transition
- [ ] Desperate at -3 ends conversation

### Cards
- [ ] Each card has ONE effect only
- [ ] No type filtering by emotional state
- [ ] Fleeting cards removed after SPEAK
- [ ] Final Word ends conversation if not played

### Success System
- [ ] Base rates: 70/60/50/40%
- [ ] Tokens add exactly 5% each
- [ ] Percentages shown before playing
- [ ] Dice roll 1-100 implemented

### States
- [ ] Five states only (Desperate/Tense/Neutral/Open/Connected)
- [ ] Weight capacity: 3/4/5/5/6
- [ ] Draw count: 1/2/2/3/3
- [ ] Linear progression only

## Design Rationale

### Why These Changes?

**One Card Per SPEAK**: Creates meaningful decision points and authentic conversation rhythm. Each statement matters.

**Weight Persistence**: Enables strategic multi-turn planning. Do you speak twice with light cards or save for one heavy card?

**Atmosphere System**: Adds persistent tactical layer without complexity. Sets up combos and creates momentum.

**Comfort Battery**: Clear visual feedback and predictable state changes. No hidden math or accumulation confusion.

**No Type Filtering**: Simplifies deck building and makes all cards potentially available. State affects capability, not options.

**Final Word**: Natural urgency for goals without artificial timers. The important topic demands attention.

**Linear Token Scaling**: Transparent progression with no threshold hunting. Every token equally valuable.

## Migration Notes

### From Old System
- Remove all state modifiers and weight modifiers
- Convert comfort accumulation to battery system
- Remove card type filtering code
- Update multi-card selection to single card
- Remove deprecated emotional states
- Transform existing cards to single-effect model

### Data Migration
- Analyze existing cards for primary effect
- Assign appropriate difficulty levels
- Set ~25% of cards as fleeting
- Add atmosphere changes to ~30% of cards
- Create observation card set
- Define goal cards with Final Word

## Future Considerations

### Potential Extensions
- More atmosphere types (with unique effects)
- Special combo effects for specific card sequences
- Temporary weight bonuses from items
- Narrative atmospheres that affect AI generation
- Burden cards that add negative atmospheres

### Balance Tuning
- Exact percentages for difficulty levels
- Token scaling rate (currently 5%)
- Weight capacities per state
- Atmosphere effect magnitudes
- Fleeting/persistent ratios

## Conclusion

This design creates a conversation system where:
- Every decision is meaningful
- All information is transparent
- Mechanics drive narrative
- Complexity emerges from simple rules
- Player skill matters through weight management and timing

The system rewards both tactical play (managing weight and atmosphere) and strategic thinking (building token relationships and managing goals).