# Wayfarer Starter Deck Design Philosophy

## Core Design Principle

The starter deck must create strategic depth through impossible choices, not mechanical complexity. Like Slay the Spire's first fight with just Strikes and Defends, every turn must force players to choose between multiple suboptimal paths.

## Fundamental Resource Economy

### The Critical Change: SPEAK Costs No Patience
- **LISTEN**: Costs 1 patience, draws cards, refreshes focus to maximum
- **SPEAK**: FREE (only spends focus from pool)

This transforms patience from action currency into focus cycle currency. Each patience point represents potential card plays across the entire conversation, making efficiency paramount.

### Time as Persistent Resource
- Each patience spent = 10 minutes game time
- Starting conversations have 15 patience = 150 minutes maximum
- Time pressure creates urgency without artificial deadlines
- Inefficient conversations literally waste the player's day

### The Focus Paradox
In GUARDED state (4 focus capacity):
- Can play one 4-focus card OR
- Can play two 2-focus cards OR
- Can play 1-focus + 3-focus cards OR
- Can play single card and waste focus

This mirrors Slay the Spire's 3 energy with 5 cards - you cannot play everything optimally.

## The Strategic Triangle

Every card serves one of three strategic purposes:

### 1. Progress (Rapport Building)
**Purpose**: Build rapport to improve success rates and enable request success
**Trade-off**: Uses focus that could be spent on setup or information
**Risk**: Failure reduces flow and might reduce rapport

### 2. Setup (Atmosphere/Economy)
**Purpose**: Create future advantage through atmospheric effects
**Trade-off**: Delays immediate rapport building
**Risk**: Might not have turns to capitalize on setup

### 3. Information (Card Draw)
**Purpose**: Expand options and find key cards
**Trade-off**: No immediate rapport or setup
**Risk**: Drawing cards you can't afford to play

# Wayfarer Universal Starter Deck

## Deck Composition (12 cards)
- **12 cards total**: Small enough to create consistency, large enough for variance
- **No impulse/opening**: Early game focuses on fundamentals, not timing pressure
- **All persistent**: Allows multi-turn planning and hand building
- **Simple effects**: One clear purpose per card, no multi-layered complexity

### Safe Progress Cards (3 cards)
**"I hear you"** (3 copies)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: +1 rapport
- Persistence: Persistent
- Purpose: Reliable rapport building, focus flexibility, combo filler

### Atmosphere Setup Cards (2 cards)
**"Let me think"** (1 copy)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: No rapport, sets Patient atmosphere
- Persistence: Persistent
- Purpose: Saves patience for longer conversations

**"Let me prepare"** (1 copy)
- Focus: 1
- Difficulty: Easy (70% base success)
- Effect: No rapport, sets Prepared atmosphere
- Persistence: Persistent
- Purpose: Enables higher focus plays next turn

### Risk/Reward Cards (2 cards)
**"How can I assist?"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base success)
- Effect: +2 rapport on success, -1 rapport on failure
- Persistence: Persistent
- Purpose: Efficient rapport with genuine risk

### Information Cards (2 cards)
**"Tell me more"** (2 copies)
- Focus: 2
- Difficulty: Medium (60% base success)
- Effect: Draw 2 cards
- Persistence: Persistent
- Purpose: Expand options, find key cards

### Powerful Cards (2 cards)
**"I'm here for you"** (1 copy)
- Focus: 3
- Difficulty: Easy (70% base success)
- Effect: +3 rapport
- Persistence: Persistent
- Purpose: Efficient safe play at full focus

**"We'll figure this out"** (1 copy)
- Focus: 3
- Difficulty: Hard (50% base success)
- Effect: +X rapport where X = patience ÷ 3
- Persistence: Persistent
- Purpose: Scaling reward for early play

### Dramatic Card (1 card)
**"Everything will be alright"** (1 copy)
- Focus: 4
- Difficulty: Hard (50% base success)
- Effect: +4 rapport
- Persistence: Persistent
- Purpose: Prepared atmosphere payoff in Guarded state

## Request Cards (Not Part of Base 12)

Request cards are placed directly in draw pile at conversation start based on conversation type chosen. They start unplayable and become playable when reaching appropriate focus capacity.

### Elena's Letter Request
**"Accept Elena's Letter"**
- Focus: 5
- Difficulty: Very Hard (40% base success)
- Starting State: Unplayable
- Becomes Playable: When LISTEN at 5+ focus capacity
- When Playable: Gains Impulse AND Opening properties
- Effect: Creates delivery obligation (fixed terms)
- Success Terms: No payment, next available position
- Failure: Add 1 burden card to Elena's relationship
- On Exhaust: Conversation ends in failure
- Purpose: Win condition requiring state progression

## Codified Conversation Rules

### Starting a Conversation
1. Pay attention cost (2 for standard conversation)
2. Starting rapport = connection tokens with NPC
3. Build draw pile from relevant NPC decks
4. Shuffle draw pile
5. Draw cards equal to connection state (Disconnected=1, Guarded=2, Neutral=2, Receptive=3, Trusting=3)
6. Request card included in draw pile but starts unplayable
7. Focus set to connection state maximum

### Connection States
- **Disconnected**: 3 focus capacity, 1 card draw
- **Guarded**: 4 focus capacity, 2 card draws
- **Neutral**: 5 focus capacity, 2 card draws
- **Receptive**: 5 focus capacity, 3 card draws
- **Trusting**: 6 focus capacity, 3 card draws

### Focus Mechanics
- Base capacity determined by connection state
- Pool persists across multiple SPEAK actions
- LISTEN refreshes focus to connection state maximum
- Prepared atmosphere adds +1 to current focus
- Can exceed maximum with Prepared (e.g., 5/4 in Guarded)
- If atmosphere clears, bonus vanishes immediately

### SPEAK Action Sequence
1. Check if focus available
2. Play ONE card (spending its focus from pool)
3. Resolve success/failure
4. Success: +1 flow, apply card effects
5. Failure: -1 flow, apply failure effects (if any)
6. Remove ALL impulse cards from hand (played or not)
7. Check remaining focus
8. Can SPEAK again if focus remains

### LISTEN Action
1. Costs 1 patience (unless Patient atmosphere active)
2. Draw cards equal to connection state
3. Refresh focus to connection state maximum
4. If Prepared active, gain +1 to current focus
5. Opening cards removed if unplayed
6. Check if request card becomes playable (sufficient focus reached)
7. If request becomes playable: Gains Impulse AND Opening properties

### Atmosphere Mechanics
- Changes when card succeeds
- Takes effect immediately
- Failure clears atmosphere to Neutral immediately
- Prepared adds +1 to current focus when active (not maximum)
- Persists until changed or cleared

### Flow Transitions
- Range: -3 to +3
- Every successful SPEAK: +1 flow
- Every failed SPEAK: -1 flow
- At +3: State shifts right, flow resets to 0
- At -3: State shifts left, flow resets to 0
- Disconnected at -3: Conversation ends immediately

### Rapport System
- Range: -50 to +50
- Starts at value equal to connection tokens
- Modified by card effects
- Each point: +2% success chance on ALL cards
- At 50 rapport: Guaranteed success
- At -50 rapport: Guaranteed failure

### Deck Cycling
- Draw pile created from NPC decks at start
- When draw pile exhausted, shuffle exhaust pile to create new draw pile
- All cards remain available throughout conversation
- No deck thinning or permanent removal

## Strategic Analysis

### Focus Distribution
- 1-focus: 5 cards (42%) - Always playable
- 2-focus: 4 cards (33%) - Standard plays
- 3-focus: 2 cards (17%) - Full capacity plays
- 4-focus: 1 card (8%) - Requires Prepared in Guarded

### Opening Hand Probabilities (Disconnected)
Drawing 1 card from 12:
- 42% chance of 1-focus card (can play)
- 33% chance of 2-focus card (can play partially)
- 17% chance of 3-focus card (can play fully)
- 8% chance of 4-focus card (cannot play)

### Minimum Paths to Victory

**Prepared Rush** (5-6 turns minimum):
1. Turn 1: Play "Let me prepare" (1 focus)
2. Turn 2: Prepared active, play rapport cards with bonus focus
3. Turns 3-4: Build to +3 flow (reach Guarded)
4. Turn 5: Continue to +6 flow (reach Neutral)
5. LISTEN at Neutral: Request becomes playable at 5 focus
6. Play request immediately (40% + rapport bonus)

**Safe Progression** (7-8 turns typical):
1. Turns 1-3: Build rapport steadily with safe cards
2. Reach +3 net successes for Guarded
3. Continue to +6 net successes for Neutral
4. Build rapport to 10+ for better request success
5. LISTEN at Neutral: Request becomes playable
6. Play request with accumulated rapport bonus (60%+ success)

### Critical Decision Points

**Turn 1 with Prepared in Hand**: Playing it immediately enables future flexibility but delays rapport building.

**Risk Management at Low Rapport**: "How can I assist?" can reduce rapport on failure. Avoid when negative.

**Focus Efficiency Choices**: With 4/4 focus, playing two 2-focus cards gives more actions than one 3-focus card.

**Request Card Timing**: Once playable, must play immediately or lose forever (gains Impulse AND Opening).

### Mathematical Validation

**Success Rate with Optimal Play**: 60-70% (perfect balance of skill and variance)

**Average Turns to Request**: 6-8 turns (uses 6-8 patience of 15 available)

**Failure Modes**:
- 30% negative flow spiral (too many failures)
- 10% patience exhaustion (inefficient LISTEN usage)
- 5% request card missed (didn't play when available)

### Key Design Achievements

**No Soft Locks**: Five 1-focus cards ensure something is always playable.

**Meaningful Choices**: Every turn forces compromise between rapport, setup, and information.

**Clear Prepared Payoff**: The 4-focus card creates specific unlock in Guarded state.

**Authentic Risk**: Failed gambles create recovery arcs, not instant loss.

**Perfect Tension**: Focus arithmetic prevents optimal play without waste.

## Token Interaction

Starting rapport equals connection tokens with NPC:
- Elena with 0 tokens: Start at 0 rapport (base rates only)
- Elena with 1 Trust token (after preparation): Start at 1 rapport (+2% all cards)
- Marcus with 2 Commerce tokens: Start at 2 rapport (+4% all cards)
- Lord Blackwood with 0 tokens: Start at 0 rapport (base rates only)

Building to 10 rapport during conversation:
- Easy cards: 90% (70% + 20%)
- Medium cards: 80% (60% + 20%)
- Hard cards: 70% (50% + 20%)
- Very Hard request: 60% (40% + 20%)

## Observation Card Integration

When NPC has observation cards in their deck:
- Mixed into draw pile at conversation start
- 0 focus cost but uses SPEAK action
- Provide unique effects (state advancement, exchange unlocks)
- Create dramatic conversation shifts

Example: "Safe Passage Knowledge" in Elena's observation deck immediately advances her from Disconnected to Neutral, transforming the conversation's difficulty.

## Strategic Depth from Simple Rules

The 12-card starter deck creates emergent complexity:

**Resource Management**: Focus persists, creating multi-turn planning.

**Risk Assessment**: When to play risky cards versus safe options.

**Timing Decisions**: Setup early or build rapport immediately.

**Recovery Arcs**: Failed plays don't end conversations, they create challenges.

**Discovery Moments**: Learning that Prepared enables the 4-focus card in Guarded state.

The deck teaches all core mechanics while remaining approachable. Players learn through play that patience is precious (LISTEN costs), focus management matters (SPEAK is free but limited), and rapport accumulation creates momentum.

This foundation scales naturally as players acquire specialized cards, build NPC-specific strategies, and discover observation advantages, while the core 12 cards remain useful throughout the game as reliable, flexible options.