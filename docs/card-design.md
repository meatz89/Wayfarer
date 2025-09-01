# Wayfarer: Card System & Strategic Design

## Core Card Philosophy

Cards are the atomic unit of interaction in Wayfarer. Each card has exactly ONE effect - either fixed or scaling, never both. No card does multiple things. The strategy emerges from weight management, atmosphere control, and context-dependent value, not mechanical complexity.

## Strategic Framework

### Four Advancement Tracks

1. **Comfort**: Emotional battery for state transitions within conversation
2. **Tokens**: Permanent relationship capital from deliveries (+5% success per token)
3. **Weight Management**: Resource pool that enables card plays
4. **Observations**: Player's deck of unique effects for tactical advantages

Each track serves different strategic goals. The puzzle is recognizing which track matters most in the current context.

### Context-Dependent Value

The same card has different strategic importance based on:
- **Current weight pool** (what's playable this turn?)
- **Active atmosphere** (how are rules modified?)
- **Token relationship** (how reliable are my plays?)
- **Comfort level** (approaching state transition at ±3?)
- **Deadline pressure** (can I afford multiple meetings?)
- **Queue position** (do I need this letter now?)
- **Card persistence** (fleeting cards removed on SPEAK)

## Card Anatomy

Every card has:
- **Token Type**: Trust, Commerce, Status, or Shadow
- **Weight**: 0-6, cost from weight pool
- **Difficulty**: Very Easy (85%), Easy (70%), Medium (60%), Hard (50%), Very Hard (40%)
- **Persistence**: Fleeting (25% of deck) or Persistent (75% of deck)
- **Primary Effect**: ONE effect, either fixed or scaling
- **Atmosphere Change** (Optional): ~30% of cards change atmosphere

## Card Generation System

### Effect Pools for Normal Cards

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
- Token-associated scaling cards might set "Focused"
- Defensive cards might set "Volatile"

**Standard Atmospheres** (on normal cards):
- **Neutral**: No effect (default after failure)
- **Prepared**: +1 weight capacity all SPEAK actions
- **Receptive**: +1 card all LISTEN actions
- **Focused**: +20% success all cards
- **Patient**: All actions cost 0 patience
- **Volatile**: All comfort changes ±1
- **Final**: Any failure ends conversation

## Observation Card Design

Observation cards represent external knowledge affecting conversations in ways normal discourse cannot.

**Fixed Properties**:
- Weight: 1 (minimal requirement)
- Persistence: Always persistent
- Difficulty: Very Easy (85% base success)
- Expiration: 24-48 hours

**Unique Effects** (Not Available on Normal Cards):

**Atmosphere Setters**
- Set "Informed" atmosphere (next card cannot fail)
- Set "Exposed" atmosphere (double all comfort changes)
- Set "Synchronized" atmosphere (next effect happens twice)
- Set "Pressured" atmosphere (-1 card on LISTEN)

**Cost Bypasses**
- Next action costs 0 patience
- Next SPEAK costs 0 weight (plays for free)

**Unique Manipulations**
- Comfort = 0 (force reset)
- Weight pool = maximum (instant refresh)

## Goal Card Design

Goals are the win condition for conversations. They appear based on conversation type chosen.

### Goal Properties
- **Weight**: 5-6 (requires Open/Connected state or Prepared atmosphere)
- **Difficulty**: Very Hard (30-40% base success) or Hard (50% base)
- **Persistence**: Fleeting with "Final Word" property
- **Effect**: Ends conversation, success determines obligation terms

### "Final Word" Property
When a fleeting goal card would be discarded (not played during SPEAK), conversation immediately ends in failure. This creates natural pressure without special tracking rules.

### Goal Challenge Layers
1. Need state with 5+ weight capacity (or Prepared atmosphere)
2. Must draw the goal card from shuffled deck
3. Need tokens for reasonable success chance (base + tokens × 5%)
4. Must play before next SPEAK or lose conversation

### Goal Types by Conversation
- **Letter Goals**: Create delivery obligations (Very Hard 40%)
- **Meeting Goals**: Create time-based obligations (Hard 50%)
- **Resolution Goals**: Remove burden cards from record (Hard 50%)
- **Commerce Goals**: Special trades or exchanges (Very Hard 40%)

## Standard NPC Deck Composition (20 cards)

Every NPC deck follows this template with personality-based token types:

- **6 Fixed comfort cards** (various weights)
  - 4-5 cards of NPC's primary token type
  - 1-2 cards of secondary types
  - Mix of Easy (70%) and Medium (60%) difficulty
  - Range from W1 (+1 comfort) to W5 (+5 comfort)
  
- **4 Scaled comfort cards** (matching NPC personality)
  - ALL same token type as NPC's primary
  - All Hard difficulty (50%)
  - Devoted NPCs: Trust-type, scale with Trust tokens
  - Mercantile NPCs: Commerce-type, scale with Commerce tokens
  - Proud NPCs: Status-type, scale with Status tokens
  - Cunning NPCs: Shadow-type, scale with Shadow tokens
  
- **2 Draw cards** (W1 each, Medium 60%)
  - Same token type as NPC's primary
  - Simple card advantage
  
- **2 Weight-add cards** (W2 each, Medium 60%)
  - Same token type as NPC's primary
  - Pool expansion for multi-turn plays
  
- **3 Setup cards** (W0, Easy 70%)
  - Mixed token types for flexibility
  - Each sets different atmosphere
  - No other effect
  
- **2 High-weight dramatic cards** (W4-6, fleeting)
  - Primary token type for maximum synergy
  - Hard or Very Hard difficulty
  - Major comfort swings
  - Often set Final atmosphere
  
- **1 Flex slot**
  - Primary token type
  - Personality-specific effect
  - May be negative comfort for emotional NPCs

## Emotional State Effects on Cards

States determine weight capacity and cards drawn. No filtering of card types.

### Weight Capacity by State
- **Desperate**: 3 (can only play W0-3 cards)
- **Tense**: 4 (can play W0-4 cards)
- **Neutral**: 5 (can play W0-5 cards)
- **Open**: 5 (can play W0-5 cards)
- **Connected**: 6 (can play all cards)

### Cards Drawn on LISTEN
- **Desperate**: 1 card
- **Tense**: 2 cards
- **Neutral**: 2 cards
- **Open**: 3 cards
- **Connected**: 3 cards

### Strategic State Navigation

States create natural progression gates:
- Goal cards need W5-6 (requires Open/Connected)
- Dramatic cards need W4+ (requires Tense or better)
- Multiple small plays need capacity to chain

Prepared atmosphere (+1 capacity) enables goal cards in Neutral state.

## Multi-Turn Weight Management

### The Weight Pool Puzzle

Managing weight as persistent resource:
- Start with capacity based on state
- Each SPEAK spends from pool
- Pool persists until LISTEN refreshes
- Can SPEAK multiple times per turn

### Fleeting Card Pressure

Fleeting cards create timing decisions:
- Removed after SPEAK (whether played or not)
- High-impact cards often fleeting
- Must be played immediately or lost
- Goal cards have "Final Word" - discard fails conversation

### Example Weight Sequences

**Desperate State (3 capacity)**:
- Turn 1: Play W2 card, 1 weight remains
- Turn 2: Play W1 card, pool empty
- Turn 3: Must LISTEN to refresh

**Connected State (6 capacity)**:
- Turn 1: Play W5 goal card, 1 remains
- Or: Play three W2 cards across turns
- Or: Play W6 dramatic card immediately

**With Prepared Atmosphere**:
- All capacities +1
- Desperate becomes 4 (can play W4 cards)
- Neutral becomes 6 (can play goal cards)

## Token Investment Strategies

Tokens only gained through successful deliveries:
- Trust letter delivery: +1-3 Trust tokens with recipient
- Commerce letter delivery: +1-3 Commerce tokens with recipient
- Status letter delivery: +1-3 Status tokens with recipient
- Shadow letter delivery: +1-3 Shadow tokens with recipient
- Failed delivery: -2 tokens (matching type) with sender

**Token-Type Matching Effects**:
- +5% success ONLY on cards of matching type
- Trust tokens boost Trust-type cards only
- Commerce tokens boost Commerce-type cards only
- Status tokens boost Status-type cards only
- Shadow tokens boost Shadow-type cards only

**Building Specialized Economies**:
- Focus deliveries based on district needs
  - Noble District: Build Status tokens for Proud NPCs
  - Market District: Build Commerce tokens for Merchants
  - Temple District: Build Trust tokens for Devoted NPCs
  - Shadow District: Build Shadow tokens for Cunning NPCs
- Wrong token types provide zero benefit
- 5 Trust tokens useless when talking to Commerce-focused merchant
- Must match tokens to NPC personality for effectiveness

## Atmosphere Management

### Strategic Atmosphere Use

**Setup Phase**: W0 cards establish atmosphere
- Prepared: Enable higher weight plays
- Receptive: Draw more cards
- Focused: Improve success rates

**Escalation Phase**: Dramatic cards change atmosphere
- Final: High risk/reward
- Volatile: Unstable comfort swings

**Observation Phase**: Unique atmospheres from player deck
- Informed: Guarantee next play
- Synchronized: Double effects
- Exposed: Amplify comfort changes

### Atmosphere Persistence

Atmosphere remains until:
- Another card changes it
- Any failure occurs (resets to Neutral)
- Conversation ends

This creates setup strategies where early turns establish favorable conditions.

## Risk Management

### Success Rate Calculations

Base success by difficulty:
```
Very Easy: 85% (Observations only)
Easy: 70% (Basic comfort, setup)
Medium: 60% (Standard comfort, utility)
Hard: 50% (Scaled comfort, dramatic)
Very Hard: 40% (Goals, major effects)

Token Bonus:
Each MATCHING token: +5%
Trust tokens only boost Trust-type cards
Commerce tokens only boost Commerce-type cards
Status tokens only boost Status-type cards
Shadow tokens only boost Shadow-type cards

Final Rate:
Base + (Matching Tokens × 5%), clamped 5%-95%

Example:
Trust-type card with 3 Trust tokens: +15% bonus
Trust-type card with 3 Commerce tokens: +0% bonus
Commerce-type card with 3 Commerce tokens: +15% bonus
```

### Failure Mitigation

Every card type has different failure cost:
- Setup cards: Atmosphere clears (minor setback)
- Comfort cards: No effect (wasted weight)
- Utility cards: No effect (wasted weight)
- Goal cards: Conversation might end (Final Word)
- Observations: No effect (wasted card)

Weight failure costs against potential gains.

## No Soft-Lock Design

Always have options:
- Weight 1 cards playable in all states (3+ capacity)
- Can LISTEN to refresh weight pool
- Can leave and return later
- Observations provide emergency advantages
- Setup cards cost 0 weight

## Advanced Techniques

### Weight Efficiency Patterns

Optimal weight usage sequences:
- W0 → W0 → W3: Setup atmosphere then use capacity
- W2 → W1: Use most weight before refresh
- Multiple W1: Chain small effects

### Comfort Sprint Strategies

Racing to ±3 for state transition:
- High-weight dramatic cards for single-turn transition
- Token scaling when tokens high
- Atmosphere amplification (Volatile, Exposed)

### Observation Timing

Observation cards as tactical tools:
- Spend attention when entering location
- Hold unique effects for critical moments
- Informed atmosphere for guaranteed goal play
- Cost bypasses for emergency plays

## Content Extensibility

### NPC Variety Through Deck Composition

Same mechanics, different token-type puzzles:

**Devoted NPC** (Elena):
- ~75% Trust-type cards (15+ cards)
- Trust-scaling comfort cards
- Trust tokens essential for success
- Patient atmosphere setup
- High patience (15 base)
- Worthless Commerce/Status/Shadow tokens

**Mercantile NPC** (Marcus):
- ~75% Commerce-type cards (15+ cards)
- Commerce-scaling comfort
- Commerce tokens essential for success
- Focused atmosphere for business
- Moderate patience (12 base)
- Exchange deck available
- Trust tokens provide no benefit

**Proud NPC** (Lord Blackwood):
- ~75% Status-type cards (15+ cards)
- Status-scaling comfort
- Status tokens essential for success
- Final atmosphere common
- Low patience (10 base)
- Letters demand position 1
- Shadow tokens useless here

**Cunning NPC** (Spy):
- ~75% Shadow-type cards (15+ cards)
- Shadow-scaling comfort
- Shadow tokens essential for success
- Pressured atmosphere tactics
- Calculating patience (12)
- Trust tokens provide no advantage

**Steadfast NPC** (Guards):
- ~25% each token type (balanced)
- Any tokens provide some benefit
- Mixed scaling cards
- Reliable patience (13)
- No specialization needed

### Infinite Puzzle Generation

Variables that create unique puzzles:
- Starting emotional state (determines initial capacity)
- Deck composition (which effects at which weights)
- Token relationships (modifies all success rates)
- Active atmosphere (changes all rules)
- Deadline pressure (forces suboptimal plays)
- Queue position competition
- Burden accumulation in records

Same framework, endless variations.

## Strategic Card Evaluation

### Evaluating Card Value

A card's true value depends on:
1. **Token-type match**: Do you have matching tokens for bonus?
2. **Weight efficiency**: Effect per weight spent
3. **Success probability**: Base + matching token modifier
4. **Timing**: Persistent vs fleeting
5. **Context**: Current state, atmosphere, comfort
6. **Opportunity cost**: What else could you play?

### Example: Token-Type Importance

**"Trust Bond"** (Trust-type, W2, Medium 60%, +2 comfort)
With 3 Trust tokens: 75% chance (60% + 15%)
With 3 Commerce tokens: 60% chance (no bonus!)

**"Good Business"** (Commerce-type, W2, Hard 50%, +X where X = Commerce tokens)
With 3 Commerce tokens: 65% chance of +3 comfort
With 3 Trust tokens: 50% chance of +0 comfort (no tokens match!)

The same tokens have completely different value depending on card type. Building the RIGHT tokens for each NPC is crucial.

## The Elegance

Every mechanic maintains its single purpose while creating cascading strategic implications:

- Weight capacity ONLY limits playability → but capacity changes everything
- Tokens ONLY modify success rates of matching types → creating specialization not generic power
- Token types ONLY match specific cards → forcing strategic relationship focus
- Comfort ONLY triggers state transitions → but states determine capacity
- Atmosphere ONLY modifies one rule → but persists across all actions
- Observations ONLY provide unique effects → but effects unavailable elsewhere

The strategy isn't in card complexity - it's in recognizing which simple effect is most valuable in the current weight, atmospheric, emotional, temporal, and token-type context. You can't be good at everything - you must specialize your token investments based on which NPCs matter most. The puzzle is specialization, not generalization. The challenge is matching, not accumulation.

This creates tactical depth through token-type alignment rather than generic power growth.