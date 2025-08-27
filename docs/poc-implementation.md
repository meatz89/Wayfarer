# Wayfarer POC Implementation - Complete Refined Mechanics

## Core Mechanical Framework

### Design Principles
- **Singular Purpose**: Each mechanic modifies exactly one thing
- **Linear Progression**: No thresholds, every increment matters
- **Perfect Information**: All effects visible upfront
- **Emergent Complexity**: Simple rules create deep puzzles

## Starting Conditions

### Player State
- **Coins**: 10
- **Health**: 75/100 (below 50 reduces max weight by 1)
- **Hunger**: 60/100 (reduces morning attention by 3)
- **Attention**: 5/10 (after hunger modifier)
- **Satchel**: Empty (5 slots maximum)

### Starting Tokens
- **Trust**: 1 with Elena (+5% to Trust cards)
- **Commerce**: 2 with Marcus (+10% to Commerce cards)
- **Status**: 0 with all
- **Shadow**: 1 with Guard Captain (+5% to Shadow cards)

### Time & Location
- **Time**: Tuesday 9:00 AM (Morning period)
- **Location**: Market Square - Fountain
- **Critical Timeline**:
  - Elena available: 2 PM (5 hours)
  - Lord Blackwood leaves: 5 PM (8 hours)

## Resource Interconnections

### Morning Attention Refresh (6 AM daily)
```
Base attention = 10
Minus (Hunger ÷ 20) rounded down
Minus ((100 - Health) ÷ 25) rounded down
Minimum = 2 attention
```
Example: 60 hunger, 75 health = 10 - 3 - 1 = 6 attention

### Resource Effects
- **Health < 50**: Maximum weight capacity -1
- **Hunger 100**: Starvation, -5 health per period
- **Attention 0**: Cannot take actions requiring attention

## Conversation System

### Core Mechanics
- **Starting Comfort**: Always 5 (access to depth 0-5 cards)
- **Patience Cost**: 1 per turn
- **Card Play**: Exactly ONE card per SPEAK action
- **Weight Limit**: Determined by emotional state

### Emotional States as Tactical Puzzles

**Weight Limits Define Strategy**:
- **Desperate/Tense** (limit 1): Precision puzzle - chain gentle touches
- **Neutral/Open/Eager** (limit 3): Risk management puzzle
- **Connected** (limit 4): Optimization puzzle
- **Overwhelmed** (limit 1): Recovery puzzle

**State-Specific Rules**:

**DESPERATE** (Crisis Mode)
- Listen: Draw 2 + 1 crisis card
- Speak: Weight limit 1 (crisis cards ignore limit)
- Letters available: Crisis/urgent Trust letters
- Strategy: Survive crisis, escape carefully

**TENSE** (Stressed)
- Listen: Draw 1, then state→Guarded
- Speak: Weight limit 1
- Letters available: Shadow letters
- Strategy: Build trust with safe plays

**NEUTRAL** (Baseline)
- Listen: Draw 2 cards
- Speak: Weight limit 3
- Letters available: Standard Commerce letters
- Strategy: Flexible pivot point

**OPEN** (Receptive)
- Listen: Draw 3 cards
- Speak: Weight limit 3
- Letters available: Trust letters
- Strategy: Push for deep connections

**EAGER** (Excited)
- Listen: Draw 3 cards
- Speak: Weight limit 3
- Letters available: Commerce letters with bonus
- Strategy: Capitalize quickly

**CONNECTED** (Deep Bond)
- Listen: Draw 3 cards
- Speak: Weight limit 4
- Letters available: All letter types
- Strategy: Maximize opportunity

## Card System - Strict Effect Separation

### Card Types and Single Effects

**Comfort Cards**
- Effect: Modify comfort value ONLY
- Success: +comfort equal to depth (depth 5 = +5 comfort)
- Failure: Always -1 comfort
- No other effects

**Token Cards**
- Effect: Add tokens ONLY
- Success: +1 token of specific type
- Failure: No change
- No comfort or state changes

**State Cards**
- Effect: Change emotional state ONLY
- Success: Specific state transition
- Failure: State unchanged
- No comfort or token changes

**Letter Cards**
- Effect: Negotiate obligation terms ONLY
- Success: Favorable terms (long deadline, good pay, flexible position)
- Failure: Poor terms (short deadline, low pay, position 1)
- Always receive letter regardless

**Knowledge Cards**
- Effect: Create observation card ONLY
- Success: Add specific observation to hand
- Failure: No observation created
- Observations expire after set hours

**Burden Cards**
- Effect: Block hand slot ONLY
- Weight 2, must resolve when possible
- Success: Remove from deck
- Failure: Remains in deck

**Crisis Cards**
- Effect: Force resolution ONLY
- Must play next turn regardless of weight limit
- Success: Remove crisis, improve state
- Failure: Add burdens, often ends conversation

### Success Rate Calculation
```
Base Rate by Weight:
- Weight 1: 60%
- Weight 2: 50%
- Weight 3: 40%
- Weight 5: 30%

Token Modifier:
+5% per matching token (linear, no cap)

Final = Base + (Tokens × 5%), clamped 5%-95%
```

## Letter Acquisition Flow

### Requirements
Letters require TWO conditions:
1. NPC in eligible emotional state
2. Comfort ≥ letter depth

### Process
1. Build comfort through successful plays
2. Navigate to appropriate emotional state
3. During LISTEN, eligible letters appear as options
4. Play letter card during SPEAK to negotiate
5. Success/failure determines terms, not acceptance

### Letter Examples

**Elena's Letter Deck**:
- "Desperate Refusal" - Depth 5, States: Desperate/Tense
  - Success: 4hr deadline, position 2, 10 coins
  - Failure: 1hr deadline, position 1, 15 coins
  
- "Formal Refusal" - Depth 10, States: Neutral/Open
  - Success: 6hr deadline, position 3, 12 coins
  - Failure: 2hr deadline, position 2, 12 coins
  
- "Personal Letter" - Depth 15, States: Open/Connected
  - Success: 8hr deadline, flexible, 20 coins
  - Failure: 4hr deadline, position 2, 20 coins

## Obligation Queue System

### Core Rules
- Position 1 MUST complete first
- Maximum 10 obligations
- New obligations enter at lowest available position

### Queue Displacement
To jump queue positions, burn tokens with displaced NPCs:
- Jump 1 position: -1 token with displaced NPC
- Jump 2 positions: -2 tokens with EACH displaced NPC
- Jump 3+ positions: -3 tokens with EACH displaced NPC

Each burned token adds 1 burden card to that NPC's deck.

### Position Negotiation
Letter cards negotiate position through success/failure:
- Standard letters: Attempt lowest available
- Crisis letters: Attempt position 1
- Proud personality: Always attempts position 1

## NPC Deck Architecture

### Elena (Devoted Type)
**Base Patience**: 12 (+1 at Private spot = 13)
**Starting State**: Desperate (forced marriage situation)

**Conversation Deck** (20 cards):
- 12 Comfort cards (depths 0-18)
- 3 Token cards (depths 5, 10, 15)
- 3 State cards (depths 4, 8, 12)
- 2 Burden cards (from past failure)

**Letter Deck**: 3 letters at different depths
**Crisis Deck**: 1 card (injected when Desperate)
**Exchange Deck**: None (not Mercantile)

### Marcus (Mercantile Type)
**Base Patience**: 10
**Starting State**: Neutral

**Conversation Deck** (15 cards):
- 10 Comfort cards
- 2 Token cards
- 2 State cards
- 1 Knowledge creation card

**Exchange Deck** (5 cards):
- "Buy Food" - 3 coins → Hunger = 0
- "Buy Medicine" - 5 coins → Health +20
- "Purchase Permit" - 15 coins → Access permit
- "Quick Job" - Accept → New obligation
- "Trade Info" - Shadow token → Route knowledge

## Observation System

### Creation Methods
**Location Observations**: 1 attention at specific spots/times
**Conversation Cards**: Knowledge cards create observations

### Properties
- Depth 0-5 (always accessible)
- Weight 1 (gentle sharing)
- Provide comfort based on context
- Expire after set hours (no decay states)

### Example Observations
- "Guard Routes" - Expires 6 hours, +4 comfort with authority
- "Market Gossip" - Expires 12 hours, +3 comfort with merchants
- "Noble Schedule" - Expires 24 hours, +5 comfort with aristocrats

## Work and Rest Actions

### Work Action
- **Availability**: Commercial spots only
- **Cost**: 2 attention
- **Effect**: Gain 8 coins, advance one period (4 hours)
- **Locations**: Market Square (Merchant Row), Copper Kettle (Bar)

### Rest Exchanges
Via Bertram's exchange deck at Hospitality spots:
- "Quick Nap" - 2 coins → +3 attention, advance 2 hours
- "Full Rest" - 5 coins → Full refresh, skip to morning
- "Hot Meal" - 2 coins → Hunger = 0

## Strategic Decision Trees

### Turn-by-Turn Example

**Turn 1** (Elena Desperate, Comfort 5):
- Weight limit 1 only
- Can access depth 0-5 cards
- Crisis card will inject on LISTEN
- Options: Build comfort safely or attempt state change

**Turn 2** (If escaped to Tense, Comfort 7):
- Still weight limit 1
- Can access depth 0-7 cards
- Shadow letters now available
- Decision: Continue building or navigate to Neutral?

**Turn 3** (If reached Neutral, Comfort 9):
- Weight limit 3 now available
- Can access depth 0-9 cards
- Commerce letters accessible
- Choice: Which state enables your goal?

### Resource Management Paths

**Path A - Morning Work**:
1. Work (+8 coins, -2 attention, advance to Midday)
2. Buy food (-3 coins, hunger→0)
3. Wait to Afternoon
4. Talk to Elena with full patience

**Path B - Direct Approach**:
1. Buy food immediately (-3 coins)
2. Wait to Afternoon (preserve attention)
3. Full conversation with Elena
4. Focus on token generation

**Path C - Speed Run**:
1. Skip food (accept reduced conversation length)
2. Rush to Elena
3. Accept first available letter
4. Use coins for checkpoint

## Success Conditions

### Perfect Run
- Obtain "Formal Refusal" or better (depth 10+)
- Negotiate 6+ hour deadline
- Complete with 2+ hours spare
- Gain Trust tokens
- 15+ coins profit

### Good Run
- Obtain any letter
- Complete before 5 PM
- Maintain positive tokens
- Break even on coins

### Failure States
- 5 PM passes without delivery
- Elena→Hostile permanently
- -3 Trust tokens with Elena
- 3 burden cards added to her deck

## Key Mechanical Insights

### No Soft Locks
- Depth 0-5 cards always accessible
- Every state has exits
- Resources renewable through work/rest
- Crisis cards ignore weight limits

### Strategic Layers
**Micro**: Which card this turn given weight limit?
**Tactical**: Which state enables my goal?
**Strategic**: How to spend limited attention today?
**Meta**: Which relationships to invest in long-term?

### Emergent Puzzles
Same NPC, different contexts create unique challenges:
- Elena Desperate with 3 tokens: Crisis but +15% success
- Elena Open with -2 tokens: Weight 3 available but -10% success
- Elena Neutral with 4 burdens: Diluted draws require patience

Every combination of state, tokens, deck composition, and resources creates a unique solvable puzzle.