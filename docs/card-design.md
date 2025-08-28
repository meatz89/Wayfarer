# Wayfarer: Card System & Strategic Design

## Core Card Philosophy

Cards are the atomic unit of interaction in Wayfarer. Each card has exactly ONE effect from its type's pool. No card does multiple things. The strategy emerges from context-dependent value, not mechanical complexity.

## Strategic Framework

### Four Advancement Tracks

1. **Comfort**: Temporary depth access within current conversation
2. **Tokens**: Permanent relationship capital (+5% success per token)
3. **State Changes**: Reshaping the conversation landscape
4. **Knowledge**: Gaining observations for world navigation

Each track serves different strategic goals. The puzzle is recognizing which track matters most in the current context.

### Context-Dependent Value

The same card has different strategic importance based on:
- **Current emotional state** (what's drawable next turn?)
- **Token relationship** (how reliable are my plays?)
- **Comfort level** (what depth am I approaching?)
- **Deadline pressure** (can I afford multiple meetings?)
- **Queue position** (do I need this letter now?)

## Card Types and Effect Pools

### Comfort Cards
**Single Effect**: Modify comfort value

**Weight-Based Risk/Reward**:
- **Light (W1)** - Reliable building:
  - "Gentle Agreement" (D2): +2 comfort at 70% base
  - "Thoughtful Response" (D5): +2 comfort at 70% base
- **Medium (W2)** - Balanced approach:
  - "Bold Statement" (D4): +4 comfort at 60% base
  - "Deep Understanding" (D8): +4 comfort at 60% base
- **Heavy (W3)** - High risk/reward:
  - "Perfect Response" (D10): +8 comfort at 45% base
  - "Soul Connection" (D15): +8 comfort at 45% base

**Strategic Role**: Build toward goal depth thresholds. Choice between reliable small gains vs risky large jumps depends on patience remaining and current comfort.

### Token Cards
**Single Effect**: Add 1 token of specific type

**Depth-Based Reliability**:
- "Initial Trust" (Light/D3): +1 Trust at 70%
- "Prove Reliability" (Medium/D6): +1 Trust at 60%
- "Deep Bond" (Medium/D10): +1 Trust at 60%
- "Sacred Promise" (Heavy/D14): +1 Trust at 45%

Same pattern for Commerce, Status, Shadow tokens.

**Strategic Role**: Build specific relationship types for promise access and future success rates. Investment in long-term relationship vs immediate needs.

### State Cards
**Single Effect**: Change emotional state

**Web Structure**: States form a web, not linear progression. Cards can transition from ANY state to ANY other if that card exists in deck.

**Depth Distribution** (exist at ALL depths):
- Low depth (0-3): Common transitions (Desperate→Tense, Neutral→Open)
- Mid depth (4-8): Complex transitions (Tense→Eager, Hostile→Neutral)  
- High depth (9-15): Powerful transitions (Any→Connected, Desperate→Open)
- Very high depth (16-20): Rare/unique transitions

All Weight 1 for accessibility regardless of depth.

**Momentum Degradation**: At -3 momentum, automatic negative transitions occur:
- Connected → Tense (trust broken)
- Open → Guarded (walls up)
- Eager → Neutral (enthusiasm dies)
- Neutral → Tense (patience wears)
- Desperate → Hostile (crisis explodes)

**Strategic Role**: Navigate emotional landscape to access needed cards and goals. Higher depth state cards offer better transitions but require comfort investment. Momentum management prevents degradation.

## Card Types and Effect Pools

### Goal Cards (The Conversation Heart)
**Single Effect**: Define conversation purpose and create obligations

**Goal Types**:
- **Letter Promise**: Accept delivery obligation (Trust/Commerce/Status/Shadow)
- **Meeting Promise**: Accept time-specific appointment
- **Escort Promise**: Accept NPC transport obligation
- **Investigation Promise**: Accept information gathering task
- **Resolution Goal**: Remove burden cards from deck
- **Commerce Goal**: Complete special exchange
- **Crisis Goal**: Resolve emergency situation

**The Urgency Rule**: Once drawn, must be played within 3 turns or conversation fails. This creates authentic pressure - important matters can't be ignored.

**Negotiation Mechanics**:
- Base success varies by goal type (35-50%)
- +5% per matching token type
- Success: Favorable terms (deadline, payment, position)
- Failure: Poor terms but still complete goal
- Playing ends conversation immediately

**Strategic Role**: The culmination of conversation investment. Everything builds toward accessing and successfully negotiating the goal.

### Knowledge Cards
**Single Effect**: Create observation card

**Information Types**:
- "Ask About Routes": Creates travel state change card
- "Request Work": Creates work opportunity at location
- "Probe Secrets": Creates shadow state change
- "Learn Schedule": Creates NPC availability knowledge
- "Gather Intel": Creates authority avoidance card

**Strategic Role**: Build options for future conversations and world navigation. Information as currency.

### Burden Cards
**Single Effect**: Block hand slot until resolved

**Resolution Challenge**:
- Weight 2, must spend turn to attempt removal
- 55% base success rate
- Failure keeps burden in deck
- Success removes permanently

**Strategic Role**: Permanent consequence of failed relationships. Forces suboptimal turns to clear, reducing conversation efficiency.

### Crisis Cards
**Single Effect**: Emergency resolution

**High Stakes**:
- Weight 5 (becomes 0 in Desperate state)
- Success (40%): Crisis resolved, +1 token
- Failure (60%): +2 burden cards

**Strategic Role**: Forced plays in crisis states. Low success rate creates tension - do you have tokens to improve odds?

### Observation Cards (Not from deck)
**Single Effect**: State change

**World Knowledge**:
- Gained from location observations (1 attention)
- Or from knowledge cards
- Weight 0-2 (varies by observation type)
- 85% success rate
- Expires after deadline (24-48 hours)

**Strategic Role**: Bypass normal state navigation. Tactical advantage from world exploration. More reliable than deck state cards due to high success rate.

## Emotional State Strategy with Momentum

### State Web Navigation

States connect as a web, not a track. Strategic considerations for each:

**DESPERATE** (Crisis State)
- Draws: Trust and Crisis cards
- Weight limit: 1 (crisis weight 0)
- Momentum: Reduces patience cost (-1 per point, min 0)
- Goals Available: Crisis, urgent letters
- Degradation Risk: → Hostile at -3 momentum
- Strategy: Build momentum to extend conversation despite crisis

**TENSE** (Cautious State)
- Draws: Shadow cards
- Weight limit: 2
- Momentum: Positive makes observations weight 0
- Goals Available: Shadow promises, burden resolution
- Degradation Risk: Default degradation target
- Strategy: Use for shadow relationships and observation plays

**NEUTRAL** (Balanced State)
- Draws: All types equally
- Weight limit: 3
- Momentum: No effect
- Goals Available: Commerce, routine promises
- Degradation Risk: → Tense at -3 momentum
- Strategy: Exploration state for unknown NPCs

**GUARDED** (Defensive State)
- Draws: State cards only
- Weight limit: 2
- Momentum: Negative increases card weights
- Goals Available: Usually none (too suspicious)
- Degradation Risk: Stable at -3 (already defensive)
- Strategy: Pure repositioning, avoid when goal needed

**OPEN** (Trust State)
- Draws: Trust and Token cards
- Weight limit: 3
- Momentum: Positive adds comfort bonus
- Goals Available: Trust promises, personal requests
- Degradation Risk: → Guarded at -3 momentum
- Strategy: Optimal for relationship building

**EAGER** (Commerce State)
- Draws: Commerce and Token cards
- Weight limit: 3
- Momentum: Each point adds +5% to token success
- Goals Available: Commerce with bonus potential
- Degradation Risk: → Neutral at -3 momentum
- Strategy: Build momentum before token plays

**OVERWHELMED** (Overloaded State)
- Draws: 1 card only
- Weight limit: 1
- Momentum: Positive allows +1 card draw
- Goals Available: None (cannot focus)
- Degradation Risk: Stable (already overwhelmed)
- Strategy: Escape quickly or conversation stalls

**CONNECTED** (Deep Bond State)
- Draws: 60% Token, 40% any
- Weight limit: 4 + momentum
- Momentum: Increases maximum weight
- Goals Available: All types
- Degradation Risk: → Tense at -3 (devastating)
- Strategy: Maintain positive momentum for heavy plays

**HOSTILE** (End State)
- Draws: Crisis cards only
- Weight limit: 1
- Momentum: No effect
- Goals Available: None
- Conversation ends after next turn
- Strategy: Last chance crisis resolution only

## Multi-Turn Tactics

### The Comfort Ladder

Building to goal depths requires planning:
- Start at comfort 5
- Goal at depth 12 requires +7 comfort
- Can reach in 2 turns with perfect W3 plays
- Or 3-4 turns with reliable W1 plays
- But patience is limited (10-15 turns)

### Goal Urgency Management

Once goal card drawn, the 3-turn timer creates intense tactical decisions:

**Turn 1 after draw**: Assess situation
- Can you play it now? (check weight, state requirements)
- Build momentum for better success?
- Need state change first?

**Turn 2**: Often forced attempt
- Momentum building becomes risky
- May need to accept current odds
- State degradation threat looms

**Turn 3**: Last chance
- Must play regardless of odds
- Failure better than timeout
- Desperation creates drama

### Momentum Chains

Building momentum creates cascading advantages:
- First success: Momentum +1 (state benefit activates)
- Second success: Momentum +2 (benefit strengthens)
- Third success: Momentum +3 (maximum benefit)
- Single failure: Momentum drops, benefit weakens
- Chain failures: Risk state degradation at -3

**Example in Open State**:
- Turn 1 success: +1 momentum, next comfort gets +1 bonus
- Turn 2 success: +2 momentum, next comfort gets +2 bonus
- Turn 3 with W2 comfort: Base +4 becomes +6 with momentum!
- Goal drawn on turn 4: Have momentum for negotiation

### State Web Navigation

Common strategic paths:

**Crisis Path** (Elena desperate):
- Stay Desperate for crisis goals (weight 0)
- Or escape via state cards before goal appears
- Momentum critical (reduces patience drain)

**Trust Building** (Relationship focus):
- Navigate to Open (Trust draws)
- Build momentum for comfort bonuses
- Draw goal at high comfort
- Negotiate with accumulated tokens

**Commerce Optimization** (Merchant dealings):
- Reach Eager state
- Build momentum (+5% per point to token cards)
- Farm tokens before goal appears
- Maximum negotiation success

Common paths based on goals:

**Trust Letter Path**:
1. Navigate to Open (Trust draws)
2. Build Trust tokens
3. Accumulate comfort to letter depth
4. Play letter card when available

**Crisis Management Path**:
1. Stay in Desperate (crisis cards weight 0)
2. Play crisis cards until resolved
3. Use state card to escape
4. Rebuild from stable state

**Commerce Optimization Path**:
1. Navigate to Eager
2. Farm Commerce tokens
3. Build comfort efficiently
4. Access commerce letters

### Token Investment Strategies

**Early Meeting**: Focus on token building
- Low comfort requirements
- Build specific relationship type
- Sets up future conversations

**Middle Meeting**: Balance tokens and comfort
- Some tokens for reliability
- Push comfort for better cards
- Explore state changes

**Late Meeting**: Comfort sprint to letters
- Tokens already established
- Maximum comfort building
- Accept available letters

## Deck Evolution Strategy

### Reading NPC Decks

Deck composition reveals strategy:
- Many Trust cards = Open state optimal
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

Always visible before playing:
```
Base Rate (by weight):
W1: 65%, W2: 55%, W3: 45%, W4: 35%, W5: 25%

Token Bonus:
Each token: +5% (linear)

Final Rate:
Base + (Tokens × 5%), clamped 5%-95%
```

### Failure Mitigation

Every card type has different failure cost:
- Comfort: -1 comfort (minor setback)
- Token: No change (wasted turn)
- State: No change (wasted turn)
- Letter: Poor terms (still get letter)
- Knowledge: No observation (wasted turn)
- Burden: Remains in deck (permanent problem)
- Crisis: +2 burdens (major setback)

Weight failure costs against potential gains.

## No Soft-Lock Design

Always have options:
- State cards at depth 1-3 (always accessible)
- Weight 1 cards always playable (minimum capacity)
- Crisis cards become weight 0 in Desperate
- Observations provide backup state changes
- Can leave and return later
- Can work/wait to reset

## Advanced Techniques

### State Multiplication Effects

Certain states modify token effectiveness:
- Desperate: Trust tokens ×2 value
- Eager: Commerce tokens ×2 value
- Connected: All tokens ×1.5 value

Stay in multiplier states when token-rich.

### Comfort Efficiency Patterns

Optimal comfort building sequences:
- W1 → W1 → W2: Reliable to depth 8-10
- W2 → W3: Aggressive push to depth 10-12
- W1 → State → W1: Navigation between comfort builds

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
- Knowledge cards create observations mid-conversation

## Content Extensibility

### NPC Variety Through Deck Composition

Same mechanics, different puzzles:

**Devoted NPC** (Elena):
- 80% Trust cards
- Deep Trust tokens required
- Patient (15 patience base)
- Personal letter types

**Mercantile NPC** (Marcus):
- 60% Commerce cards
- Eager state oriented
- Efficient (12 patience base)
- Business letter types

**Proud NPC** (Lord Blackwood):
- 60% Status cards
- Comfort cards at high depths
- Impatient (10 patience base)
- Letters demand position 1

**Cunning NPC** (Spy):
- 60% Shadow cards
- State cards for misdirection
- Calculating (12 patience)
- Time-sensitive letters

### Infinite Puzzle Generation

Variables that create unique puzzles:
- Starting emotional state
- Deck composition (which cards at which depths)
- Token requirements for letters
- Deadline pressure
- Queue position competition
- Burden cards from past failures

Same framework, endless variations.

## The Elegance

Every mechanic maintains its single purpose while creating cascading strategic implications:

- Emotional states ONLY filter draws → but filtering changes everything
- Tokens ONLY modify success rates → but linearly, so every token matters
- Comfort ONLY grants depth access → but depth gates the best cards
- Weight ONLY limits playability → but emotional states set the limit
- Letters ONLY create obligations → but negotiation determines viability

The strategy isn't in card complexity - it's in recognizing which simple effect is most valuable in the current emotional, relational, and temporal context. The puzzle is navigation, not calculation. The challenge is recognition, not memorization.

This creates Slay the Spire's tactical depth through context rather than card text complexity.