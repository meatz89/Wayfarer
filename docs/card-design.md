# Wayfarer: Card System & Strategic Design

## Core Card Philosophy

Cards are the atomic unit of interaction in Wayfarer. Each card has exactly ONE effect from its type's pool. No card does multiple things. The strategy emerges from context-dependent value, not mechanical complexity.

## Strategic Framework

### Four Advancement Tracks

1. **Comfort**: Emotional battery for state transitions within conversation
2. **Tokens**: Permanent relationship capital (+5% success per token)
3. **State Changes**: Reshaping the conversation landscape
4. **Observations**: Player's deck of world knowledge for tactical advantages

Each track serves different strategic goals. The puzzle is recognizing which track matters most in the current context.

### Context-Dependent Value

The same card has different strategic importance based on:
- **Current emotional state** (what's drawable next turn?)
- **Token relationship** (how reliable are my plays?)
- **Comfort level** (approaching state transition at ±3?)
- **Deadline pressure** (can I afford multiple meetings?)
- **Queue position** (do I need this letter now?)

## Card Types and Effect Pools

### Comfort Cards
**Single Effect**: Modify comfort value

**Weight-Based Effects**:
- **Light (W1)** - Reliable building:
  - "Gentle Agreement": +1 comfort on success, -1 on failure
  - "Thoughtful Response": +1 comfort on success, -1 on failure
- **Medium (W2)** - Balanced approach:
  - "Bold Statement": +2 comfort on success, -2 on failure
  - "Deep Understanding": +2 comfort on success, -2 on failure
- **Heavy (W3)** - High impact:
  - "Perfect Response": +3 comfort on success, -3 on failure
  - "Soul Connection": +3 comfort on success, -3 on failure

**Drawable States**: Each comfort card lists which emotional states allow drawing it
- Trust comfort cards: [Open, Connected, Desperate]
- Commerce comfort cards: [Eager, Neutral]
- Shadow comfort cards: [Tense, Guarded]

**Strategic Role**: Build toward +3 for positive state transition or manage away from -3 to avoid negative transition. Choice between reliable small gains vs risky large swings depends on patience remaining and current comfort.

### Token Cards
**Single Effect**: Add 1 token of specific type

**Success/Failure**:
- Success: +1 token of specified type
- Failure: No change

**Drawable States**: Based on token type
- Trust tokens: [Open, Connected]
- Commerce tokens: [Eager, Neutral]
- Status tokens: [Neutral, Open]
- Shadow tokens: [Tense, Guarded]

**Strategic Role**: Build specific relationship types for better negotiation outcomes and future success rates. Investment in long-term relationship vs immediate needs.

### State Cards
**Single Effect**: Change emotional state

**Web Structure**: States form a web, not linear progression. Cards can transition from ANY state to ANY other if that card exists in deck.

**Drawable States**: All state cards drawable in all non-Hostile states (Weight 1 ensures playability in most states)

All Weight 1 for accessibility regardless of emotional state limits.

**Comfort-Triggered Transitions**: At ±3 comfort, automatic state transitions occur:
- DESPERATE: +3→Tense, -3→Hostile
- HOSTILE: +3→Tense, -3→Conversation ends
- TENSE: +3→Neutral, -3→Hostile
- GUARDED: +3→Neutral, -3→Hostile
- NEUTRAL: +3→Open, -3→Tense
- OPEN: +3→Connected, -3→Guarded
- EAGER: +3→Connected, -3→Neutral
- CONNECTED: +3→Stays Connected, -3→Tense

**Strategic Role**: Navigate emotional landscape to access needed cards and goals. States determine drawable card pools. Comfort management prevents unwanted transitions.

### Patience Cards
**Single Effect**: Extend conversation time

**Success/Failure**:
- Success: +X patience (varies by card)
- Failure: No change

**Drawable States**: [All except Hostile]

**Strategic Role**: Buy more turns when approaching valuable cards or goals. Critical when patience runs low but goal card appears.

### Goal Cards (The Conversation Heart)
**Single Effect**: Define conversation purpose and create obligations

**Goal Selection**: Based on conversation type player chooses:
- **Letter Conversation**: Letter goal shuffled in
- **Promise Conversation**: Meeting/Escort/Investigation goal
- **Resolution Conversation**: Burden removal goal
- **Commerce Conversation**: Special trade goal

**The Urgency Rule**: Once drawn, gains Goal persistence - must be played within 3 turns or conversation fails. This creates authentic pressure - important matters can't be ignored.

**Negotiation Mechanics**:
- Base success varies by goal type (35-50%)
- +5% per matching token type
- Success: Favorable terms (deadline, payment, position)
- Failure: Poor terms but still complete goal
- Playing ends conversation immediately

**Strategic Role**: The culmination of conversation investment. Everything builds toward accessing and successfully negotiating the goal.

### Observation Cards (Player's Deck)
**Single Effect**: State change cards

**Acquisition Types**:
- Location observations (1 attention at spots)
- Conversation rewards (NPCs share knowledge)
- Travel discoveries (route information)

**Properties**:
- Weight 1 (always playable except in Hostile)
- 85% success rate
- Expire after 24-48 hours
- Maximum 20 cards in player deck

**Strategic Role**: Build tactical advantages for future conversations. Information as currency. Bypass normal state navigation with high-reliability state changes.

### Burden Cards
**Single Effect**: Block hand slot until resolved

**Resolution Challenge**:
- Weight 2, must spend turn to attempt removal
- 55% base success rate
- Failure keeps burden in deck
- Success removes permanently

**Strategic Role**: Permanent consequence of failed relationships. Forces suboptimal turns to clear, reducing conversation efficiency.

## Emotional State Strategy

### State Web Navigation

States connect as a web, not a track. Strategic considerations for each:

**DESPERATE** (Crisis State)
- Draws: Cards listing Desperate as drawable state
- Weight limit: 1 (crisis prevents complex thought)
- Comfort: +3→Tense, -3→Hostile
- Goals Available: Crisis, urgent letters
- Strategy: Manage comfort carefully to avoid Hostile

**TENSE** (Cautious State)
- Draws: Cards listing Tense as drawable state
- Weight limit: 2
- Comfort: +3→Neutral, -3→Hostile
- Goals Available: Shadow promises, burden resolution
- Strategy: Use for shadow relationships and careful navigation

**NEUTRAL** (Balanced State)
- Draws: Cards listing Neutral as drawable state
- Weight limit: 3
- Comfort: +3→Open, -3→Tense
- Goals Available: Commerce, routine promises
- Strategy: Exploration state for unknown NPCs

**GUARDED** (Defensive State)
- Draws: Cards listing Guarded as drawable state
- Weight limit: 1
- Comfort: +3→Neutral, -3→Hostile
- Goals Available: Usually none (too suspicious)
- Strategy: Pure repositioning, avoid when goal needed

**OPEN** (Trust State)
- Draws: Cards listing Open as drawable state
- Weight limit: 3
- Comfort: +3→Connected, -3→Guarded
- Goals Available: Trust promises, personal requests
- Strategy: Optimal for relationship building

**EAGER** (Commerce State)
- Draws: Cards listing Eager as drawable state
- Weight limit: 3
- Comfort: +3→Connected, -3→Neutral
- Goals Available: Commerce with bonus potential
- Strategy: Build positive comfort before token plays

**CONNECTED** (Deep Bond State)
- Draws: Cards listing Connected as drawable state
- Weight limit: 4
- Comfort: +3→Stays Connected, -3→Tense
- Goals Available: All types
- Strategy: Maintain positive comfort to stay in best state

**HOSTILE** (End State)
- Draws: NO cards
- Weight limit: 0
- Comfort: +3→Tense, -3→Conversation ends
- Goals Available: None
- Cannot play cards, only LISTEN to try for +3 comfort

## Multi-Turn Tactics

### The Comfort Battery

Managing comfort as emotional energy:
- Start at comfort 0
- Each success adds comfort equal to card weight
- Each failure subtracts comfort equal to card weight
- At +3: Positive state transition, reset to 0
- At -3: Negative state transition, reset to 0

### Goal Urgency Management

Once goal card drawn, the 3-turn timer creates intense tactical decisions:

**Turn 1 after draw**: Assess situation
- Can you play it now? (check weight, state requirements)
- Need state change first?

**Turn 2**: Often forced attempt
- State navigation becomes risky
- May need to accept current odds

**Turn 3**: Last chance
- Must play regardless of odds
- Failure better than timeout
- Desperation creates drama

### State Web Navigation

Common strategic paths:

**Crisis Path** (Elena desperate):
- Stay Desperate carefully managing comfort
- Or build to +3 for escape to Tense
- Avoid -3 which triggers Hostile

**Trust Building** (Relationship focus):
- Navigate to Open (Trust draws)
- Build comfort toward +3 for Connected
- Draw goal in Connected (best state)
- Negotiate with accumulated tokens

**Commerce Optimization** (Merchant dealings):
- Reach Eager state
- Build tokens before goal appears
- Maximum negotiation success

### Token Investment Strategies

**Early Meeting**: Focus on token building
- Low weight requirements in starting states
- Build specific relationship type
- Sets up future conversations

**Middle Meeting**: Balance tokens and comfort
- Some tokens for reliability
- Manage comfort for state control
- Explore state changes

**Late Meeting**: Sprint to goals
- Tokens already established
- Focus on reaching goal-favorable states
- Accept available goals

## Deck Evolution Strategy

### Reading NPC Decks

Deck composition reveals strategy:
- Many Trust cards = Open/Connected states optimal
- Many Commerce cards = Eager state optimal
- Many state cards = Flexible navigation
- Many burden cards = Damaged relationship

### Long-term Deck Shaping

Each delivery modifies recipient's deck:
- Trust letters → Trust comfort cards added
- Commerce letters → Commerce comfort cards added
- Failed deliveries → Burden cards added

Twenty deliveries create twenty permanent changes. Build favorable decks in frequently visited NPCs.

## Risk Management

### Success Rate Calculations

Base success rate uniform across weights:
```
Base Rate: 60% for all cards (weight doesn't affect)

Token Bonus:
Each token: +5% (linear)

Final Rate:
Base + (Tokens × 5%), clamped 5%-95%
```

### Failure Mitigation

Every card type has different failure cost:
- Comfort: Negative comfort equal to weight
- Token: No change (wasted turn)
- State: No change (wasted turn)
- Patience: No change (wasted turn)
- Letter: Poor terms (still get letter)
- Observation: No effect (wasted turn)
- Burden: Remains in deck (permanent problem)

Weight failure costs against potential gains.

## No Soft-Lock Design

Always have options:
- State cards at Weight 1 (playable in most states)
- Observations provide backup state changes
- Can leave and return later
- Can work/wait to reset

## Advanced Techniques

### Comfort Efficiency Patterns

Optimal comfort building sequences:
- W1 → W1 → W1: Safe path to +3 transition
- W3: Risky single-turn transition attempt
- W2 → W1: Balanced approach to +3

### Queue Position Gaming

Letter card negotiation tactics:
- High tokens = better negotiation success
- Proud NPCs always attempt position 1
- Crisis states force position 1
- Accept poor position to preserve tokens
- Displace only for critical deadlines

### Observation Timing

Observation cards as tactical tools:
- Spend attention when entering location
- Hold state changes for critical moments
- Observations expire - use before deadline
- Build player deck for future flexibility

## Content Extensibility

### NPC Variety Through Deck Composition

Same mechanics, different puzzles:

**Devoted NPC** (Elena):
- Cards drawable in: [Desperate, Open, Connected]
- Trust tokens improve negotiations
- Patient (15 patience base)
- Personal letter types

**Mercantile NPC** (Marcus):
- Cards drawable in: [Eager, Neutral]
- Eager state oriented
- Efficient (12 patience base)
- Business letter types

**Proud NPC** (Lord Blackwood):
- Cards drawable in: [Neutral, Open]
- Comfort cards at higher weights
- Impatient (10 patience base)
- Letters demand position 1

**Cunning NPC** (Spy):
- Cards drawable in: [Tense, Guarded]
- State cards for misdirection
- Calculating (12 patience)
- Time-sensitive letters

### Infinite Puzzle Generation

Variables that create unique puzzles:
- Starting emotional state
- Deck composition (which cards drawable in which states)
- Token effects on negotiation success
- Deadline pressure
- Queue position competition
- Burden cards from past failures

Same framework, endless variations.

## The Elegance

Every mechanic maintains its single purpose while creating cascading strategic implications:

- Emotional states ONLY filter draws → but filtering changes everything
- Tokens ONLY modify success rates → but linearly, so every token matters
- Comfort ONLY triggers state transitions → but states gate different cards
- Weight ONLY limits playability → but emotional states set the limit
- Letters ONLY create obligations → but negotiation determines viability

The strategy isn't in card complexity - it's in recognizing which simple effect is most valuable in the current emotional, relational, and temporal context. The puzzle is navigation, not calculation. The challenge is recognition, not memorization.

This creates Slay the Spire's tactical depth through context rather than card text complexity.