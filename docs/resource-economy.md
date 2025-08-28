# Wayfarer: Resource Economy & Intentional Design

## Core Economic Philosophy

Every resource flows through multiple systems via different mechanics. Each mechanic has exactly ONE intentional effect. No thresholds, no secondary effects, no "OR" statements. Resources create strategic depth through their multiple uses, not through complex mechanics.

## Primary Resources

### Attention
**Daily Allocation**: 10 - (Hunger ÷ 25), minimum 2

**Uses** (each via different mechanic):
- **Conversation Mechanic**: Spend 2 → Full emotional state conversation
- **Observation Mechanic**: Spend 1 → Gain state change card
- **Work Mechanic**: Spend 2 → Gain 8 coins + advance 4 hours

**Regeneration**:
- Morning refresh (6 AM daily)
- Rest exchanges (coins → attention)

**Strategic Role**: The daily action budget. Forces prioritization between relationship building, information gathering, and resource generation.

### Coins
**Range**: 0-999

**Uses**:
- **Exchange Mechanic**: Trade for resources/items
- **Rest Mechanic**: Buy attention/health/hunger relief
- **Bribe Mechanic**: Gain access permits from guards
- **Payment Receipt**: Completing letter deliveries

**Generation**:
- Work actions (8 coins per action)
- Letter delivery payments (5-20 coins based on negotiation)
- Exchange trades

**Strategic Role**: Flexible resource converter. Enables recovery from bad situations but competes with progression needs.

### Health
**Range**: 0-100 (death at 0)

**Effects**:
- **Weight Capacity**: Below 50 health → maximum weight -1 in conversations
- **Starvation Damage**: -5 health per period at 100 hunger

**Restoration**:
- Rest exchanges
- Medical items from exchanges
- Time (slow natural recovery)

**Strategic Role**: Long-term sustainability. Affects conversation capability without being a hard gate.

### Hunger
**Range**: 0-100

**Effects**:
- **Attention Calculation**: Reduces morning attention by (Hunger ÷ 25)
- **Starvation Trigger**: At 100 → lose 5 health per period
- **Automatic Increase**: +20 per time period

**Restoration**:
- Food exchanges (coins → hunger relief)
- Tavern rest options
- Consumable items

**Strategic Role**: Constant pressure that erodes other resources. Forces regular maintenance without hard blocking.

## Relationship Resources

### Connection Tokens

Four types, each with distinct identity:
- **Trust**: Personal bonds
- **Commerce**: Professional dealings
- **Status**: Social standing  
- **Shadow**: Shared secrets

**Single Mechanical Effect**: +5% success rate per token (linear, no threshold)

**Multiple Uses Through Different Mechanics**:
1. **Success Modifier**: Every token adds 5% to all card success rates
2. **Promise Negotiation**: Better terms with matching token types  
3. **Displacement Cost**: Burn tokens to jump queue positions
4. **Special Effects**: Some cards may reference token counts
5. **Crisis Mitigation**: +10% per token on crisis cards (double value)

Each use is a separate mechanic. Tokens never gate access to content.

**Token Acquisition**:
- Token cards played successfully (+1 token)
- Crisis resolution success (+1 token)
- Some letter delivery bonuses (+1 token)

**Token Loss**:
- Failed deliveries (-2 tokens with sender)
- Queue displacement (-1 per position jumped per displaced NPC)
- Can go negative (relationship debt)

## Conversation Resources

### Comfort (Temporary)
**Range**: 0-20 within single conversation
**Starting Value**: Always 5
**Effect**: Determines maximum card depth accessible

**Modification**:
- Comfort cards ONLY modify comfort
- W1 cards: +2 (success) / -1 (failure)
- W2 cards: +4 (success) / -1 (failure)  
- W3 cards: +6 (success) / -2 (failure)

**Strategic Role**: Tactical resource for reaching valuable cards within current conversation. Resets between conversations.

### Momentum (Temporary)
**Range**: -3 to +3 within single conversation
**Starting Value**: Always 0
**Effect**: Varies by emotional state (see emotional state rules)

**Modification**:
- Any successful card play: +1
- Any failed card play: -1
- Capped at -3 and +3

**State Degradation at -3**:
- Connected → Tense (trust broken)
- Open → Guarded (walls go up)  
- Eager → Neutral (enthusiasm dies)
- Neutral → Tense (patience wears)
- Desperate → Hostile (crisis explodes)
- Others → Tense (default degradation)

**Strategic Role**: Rewards successful chains within conversations. Each emotional state uses momentum differently, creating distinct conversation textures. At -3, automatic state degradation creates downward spirals that must be avoided.

### Patience (Per-NPC Resource)
**Base Values by Personality**:
- Devoted: 15
- Steadfast: 13
- Mercantile: 12
- Cunning: 12
- Proud: 10

**Modifiers**:
- Private spot: +1
- Public spot: -1

**Effect**: Determines conversation length (1 patience per turn)

**Strategic Role**: Time limit for each conversation. Forces efficient play and tough decisions about when to push vs accept available options.

## Time Resources

### Time Periods
Six daily periods, each 4 hours:
- Morning (6-10 AM)
- Midday (10 AM - 2 PM)
- Afternoon (2-6 PM)
- Evening (6-10 PM)
- Night (10 PM - 2 AM)
- Deep Night (2-6 AM)

**Time Advancement Mechanics**:
- **Travel**: Route-specific time cost
- **Work**: Always advances one full period (4 hours)
- **Rest**: Variable based on rest type
- **Wait**: Strategic time advancement
- **Natural**: During lengthy activities

**Effects of Time**:
- NPC availability windows
- Observation availability
- Shop operating hours
- Deadline pressure

### Deadlines
**Range**: 1-24 hours typically

**Effect**: Failed delivery at deadline expiration:
- -2 tokens with sender
- +2 burden cards to sender's deck
- No payment received
- Permanent relationship damage

**Strategic Role**: Creates cascading time pressure. Forces queue management decisions and route optimization.

## Information Resources

### Observation Cards
**Not from decks** - gained from world

**Acquisition Mechanics**:
- **Location Observation**: 1 attention at specific spots/times
- **Knowledge Cards**: Create observations during conversations

**Properties**:
- Weight 1 (always playable)
- 70% success rate
- State change effects only
- Expire after 24-48 hours

**Strategic Role**: Bypass normal state navigation in conversations. Reward exploration and information gathering.

### Access Permits
**Special letter type** - no delivery obligation

**Properties**:
- Occupy satchel space (max 5 with letters)
- Enable specific routes
- Do not expire (physical documents)
- Cannot be dropped without consequence

**Acquisition**:
- Letter cards (requires 5+ tokens typically)
- Exchange cards (15-20 coins)
- Knowledge cards creating opportunities

**Strategic Role**: Gate exploration and enable efficient routing. Compete for limited satchel space.

## Resource Conversion Chains

### Time → Money → Progress
```
Work Action (2 attention + 4 hours) → 8 coins
8 coins → Food (reset hunger) → Better morning attention  
Better attention → More conversations → More letters
More letters → More payment → More coins
```

### Tokens → Success → Letters → Tokens
```
Token cards → Build specific relationship type
Higher tokens → Better letter card success rates
Better negotiation → Favorable queue position
Efficient completion → Preserve tokens
Preserved tokens → Stronger future relationships
```

### Knowledge → Access → Efficiency
```
Observation (1 attention) → State change card
State change → Reach better emotional state
Better state → Access to desired cards
Desired cards → Letters or tokens
Letters → Payment and reputation
```

## Strategic Resource Management

### Resource Pressure Points

**Morning Attention Calculation**:
- Base 10 attention
- Hunger reduces it (every 25 hunger = -1 attention)
- Minimum 2 (never completely blocked)
- Creates pressure to manage hunger without hard lock

**Queue Displacement Cascade**:
- Need urgent delivery → Must displace
- Displacement burns tokens → Relationships damaged
- Damaged relationships → Harder conversations
- Harder conversations → Fewer letters
- Fewer letters → Less income

**Satchel Limitations**:
- 5 slots for letters AND permits
- Full satchel blocks new letters
- Dropping letters damages relationships
- Permits compete with profitable letters

### Resource Conservation Strategies

**Token Preservation**:
- Accept poor queue positions to avoid burning
- Build tokens in specific NPCs for permit access
- Use observations to improve success without token cost

**Attention Efficiency**:
- Chain obligations in same location
- Observe before conversing for state advantages
- Work only when coins critically needed

**Time Optimization**:
- Plan routes to minimize travel
- Accept letters with compatible deadlines
- Use wait actions strategically

## No Threshold Design

Every resource scales linearly:
- Each token: exactly +5% success
- Each 25 hunger: exactly -1 attention
- Each patience: exactly 1 turn
- Each comfort: access to that exact depth

No breakpoints. No "magic numbers." Every point matters equally.

## Intentional Mechanic Design

Examples of clean separation:

**BAD**: "Routes require access permit OR 10 coins"
**GOOD**: Routes require access permit. Guards can be bribed for permits. Merchants sell permits.

**BAD**: "High tokens unlock letters AND improve success"  
**GOOD**: Tokens improve success rates. Letter cards check token threshold separately.

**BAD**: "Hunger reduces patience AND attention"
**GOOD**: Hunger reduces attention only. Patience is per-NPC, not affected by player resources.

**BAD**: "Emotional states filter draws AND modify weights"
**GOOD**: Emotional states filter draws. Weight limits are based on state, but that's a separate rule.

## Resource Flow Visibility

All resource effects visible to player:
- Success rates shown before playing cards
- Token requirements shown on letter cards
- Displacement costs shown when viewing queue
- Time costs shown on routes
- Attention formula transparent

No hidden calculations. Perfect information for strategic decisions.

## Economic Balance Points

### Daily Attention Budget
- 10 attention (well-fed) allows:
  - 5 observations OR
  - 2 conversations + 1 work OR
  - 1 conversation + 4 observations

### Letter Profitability
- Poor negotiation: 5 coins, 2-hour deadline
- Average negotiation: 10 coins, 4-hour deadline
- Good negotiation: 15-20 coins, 6+ hour deadline

### Token Investment Return
- 3 tokens: +15% success (noticeable)
- 6 tokens: +30% success (reliable)
- 10 tokens: +50% success (dominant)
- But burning 10 tokens destroys relationships

## Extensibility Through Resource Configuration

New content uses same resources differently:

**Desperate Scenario**: High hunger, low coins, tight deadlines
**Political Scenario**: Status tokens critical, Trust secondary
**Merchant Campaign**: Commerce focus, coin optimization
**Shadow Path**: Shadow tokens unlock unique routes

Same resources, same mechanics, different strategic emphasis.

## Core Innovation

Resources flow through multiple mechanics without any mechanic doing multiple things:

- Tokens affect success (one mechanic) AND gate letters (different mechanic) AND enable displacement (third mechanic)
- Attention enables conversations (one mechanic) AND observations (different mechanic) AND work (third mechanic)
- Comfort gates depth (one mechanic) while tokens affect success (different mechanic) even though both impact letter access

This creates strategic depth from simple, intentional rules. Every resource matters in multiple ways, but each way is a distinct, single-purpose mechanic. The economy is the intersection of these mechanics, not their individual complexity.