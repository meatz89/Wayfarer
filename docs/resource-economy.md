# Wayfarer: Resource Economy & Intentional Design

## Core Economic Philosophy

Every resource flows through multiple systems via different mechanics. Each mechanic has exactly ONE intentional effect. No thresholds (except flow ±3 for state transitions), no secondary effects, no "OR" statements. Resources create strategic depth through their multiple uses, not through complex mechanics.

## Primary Resources

### Attention
**Daily Allocation**: 10 - (Hunger ÷ 25), minimum 2

**Uses** (each via different mechanic):
- **Conversation Mechanic**: Spend 2 → Full connection state conversation
- **Observation Mechanic**: Spend 1 → Gain observation card with unique effects
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
- Letter delivery payments (5-20 coins based on fixed terms)
- Exchange trades

**Strategic Role**: Flexible resource converter. Enables recovery from bad situations but competes with progression needs.

### Health
**Range**: 0-100 (death at 0)

**Effects**:
- **Focus Capacity**: Below 50 health → maximum focus capacity -1 in conversations
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

**Single Mechanical Effect**: Provide starting rapport in conversations (1 token = 1 rapport)

**Multiple Uses Through Different Mechanics**:
1. **Starting Rapport**: Each token provides 1 starting rapport in conversations
2. **Displacement Cost**: Burn tokens to jump queue positions
3. **Scaling Effects**: Some rapport cards scale with specific token counts

Each use is a separate mechanic. Tokens never gate access to content.

**Token Acquisition** (ONLY through letter delivery):
- Standard successful delivery: +1 token with recipient (type matches letter)
- Excellent delivery: +2-3 tokens with recipient
- Trust letters build Trust tokens
- Commerce letters build Commerce tokens
- Status letters build Status tokens
- Shadow letters build Shadow tokens

**Token Loss**:
- Failed deliveries: -2 tokens with sender (matching type)
- Queue displacement: -1 per position jumped per displaced NPC
- Can go negative (relationship debt)

## Conversation Resources

### Rapport (Temporary)
**Range**: -50 to +50 within single conversation
**Starting Value**: Equal to connection tokens with NPC
**Effect**: +2% success chance per point on ALL cards

**Modification**:
- Fixed rapport cards: +1 to +5 or -1 to -3 based on card
- Scaled rapport cards: +X where X varies by scaling type
- Atmosphere effects can modify rapport changes

**Strategic Properties**:
- At -50: All cards have -100% (guaranteed failure)
- At 0: Base success rates apply
- At +50: All cards have +100% (guaranteed success)
- Resets when conversation ends
- Can create positive or negative spirals

**Strategic Role**: Primary success modifier within conversations. Early rapport building compounds success probability, creating momentum. Starting with tokens provides initial advantage.

### Flow (Per-Conversation)
**Range**: -3 to +3 within single conversation
**Starting Value**: Always 0
**Modification**: +1 on successful SPEAK, -1 on failed SPEAK

**State Transitions at ±3**:
- DISCONNECTED: +3→Guarded, -3→Conversation ends
- GUARDED: +3→Neutral, -3→Disconnected
- NEUTRAL: +3→Open, -3→Guarded
- OPEN: +3→Connected, -3→Neutral
- CONNECTED: +3→Stays Connected, -3→Open

**Strategic Role**: Pure success/failure counter that triggers connection state transitions. Creates predictable progression based on net successes, with randomness coming from success rates modified by rapport.

### Focus (Per-Conversation)
**Capacity by Connection State**:
- Disconnected: 3
- Guarded: 4
- Neutral: 5
- Open: 5
- Connected: 6

**Mechanics**:
- Refreshes to maximum on LISTEN
- Persists across SPEAK actions
- Each card costs its focus value
- Prepared atmosphere adds +1 capacity
- Health below 50 reduces capacity by 1

**Strategic Role**: Core resource management within conversations. Enables multi-turn planning with impulse cards that require more focus than currently available.

### Atmosphere (Per-Conversation)
**Standard Atmospheres** (~30% of normal cards):
- **Neutral**: No effect (default, set after any failure)
- **Prepared**: +1 focus capacity
- **Receptive**: +1 card on LISTEN
- **Focused**: +20% success all cards
- **Patient**: Actions cost 0 patience
- **Volatile**: All rapport changes ±1
- **Final**: Any failure ends conversation

**Observation-Only Atmospheres**:
- **Informed**: Next card cannot fail
- **Exposed**: Double all rapport changes
- **Synchronized**: Next card effect happens twice
- **Pressured**: -1 card on LISTEN

**Persistence**: Remains until changed by another card or cleared by failure

**Strategic Role**: Environmental modifier that shapes entire conversations. Setup cards create favorable conditions for critical plays.

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
- Patient atmosphere: Actions cost 0

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
- +2 burden cards to sender's relationship record
- No payment received
- Permanent relationship damage

**Strategic Role**: Creates cascading time pressure. Forces queue management decisions and route optimization.

## Information Resources

### Observation Cards
**Not from decks** - gained from world

**Acquisition Mechanics**:
- **Location Observation**: 1 attention at specific spots/times
- **NPC Rewards**: Completing promises
- **Travel Discoveries**: Finding new routes

**Properties**:
- Focus 1 (minimal pool requirement)
- Always persistent
- 85% success rate (Very Easy difficulty)
- Unique effects not available on normal cards
- Expire after 24-48 hours

**Unique Effects**:
- Set special atmospheres (Informed, Exposed, Synchronized, Pressured)
- Bypass costs (next action free, next SPEAK costs 0 focus)
- Unique manipulations (set rapport to 15, refresh focus)

**Strategic Role**: Bypass normal conversation limitations. Reward exploration and information gathering. Create powerful one-time effects.

### Access Permits
**Special letter type** - no delivery obligation

**Properties**:
- Occupy satchel space (max 5 with letters)
- Enable specific routes
- Do not expire (physical documents)
- Cannot be dropped without consequence

**Acquisition**:
- Request cards (fixed terms, no negotiation)
- Exchange cards (15-20 coins)

**Strategic Role**: Gate exploration and enable efficient routing. Compete for limited satchel space.

### Burden Cards (Relationship Records)
**Not in conversation decks** - tracked per NPC relationship

**Acquisition**:
- Failed request cards: +1 burden card
- Queue displacement: +1 per token burned
- Broken promises: +1-2 burden cards

**Effects**:
- Block relationship progress
- Enable "Make Amends" conversation type
- Visual indicator of damaged relationships

**Resolution**:
- "Clear the Air" request card removes burdens
- Very Hard difficulty (40% base + rapport modifier)

**Strategic Role**: Permanent consequences that must be actively resolved. Create repair arcs for damaged relationships.

## Card Difficulty Resources

### Success Rate Tiers
Cards use difficulty tiers modified by rapport:

**Very Easy** (85% base):
- Observation cards exclusively

**Easy** (70% base):
- Basic fixed rapport (+1, -1)
- Setup cards (0 focus atmosphere setters)

**Medium** (60% base):
- Standard fixed rapport (+2, -2)
- Utility cards (draw, focus-add)

**Hard** (50% base):
- Scaled rapport cards
- High fixed rapport (+3)
- Resolution requests

**Very Hard** (40% base):
- Dramatic fixed rapport (+4, +5, -3)
- Request cards (letters, meetings)
- Crisis resolutions

**Rapport Modification**: +2% per point of rapport, applied to all cards equally

**Strategic Role**: Creates risk/reward calculations. Higher impact requires accepting lower base success.

## Resource Conversion Chains

### Time → Money → Progress
```
Work Action (2 attention + 4 hours) → 8 coins
8 coins → Food (reset hunger) → Better morning attention  
Better attention → More conversations → Request card access
Request cards → Letter obligations → Delivery success
Delivery success → Payment + Tokens
```

### Tokens → Rapport → Success → More Tokens
```
Successful deliveries → +1-3 tokens
Higher tokens → Better starting rapport
Better rapport → Higher success rates
More successes → Positive flow → State advancement
State advancement → Request availability
Request success → More deliveries → More tokens
```

### Knowledge → Access → Efficiency
```
Observation (1 attention) → Unique effect card
Special atmosphere → Bypass normal limitations
Better success rates → Reach request cards
Request cards → Letters or permits
Permits → New routes → More opportunities
```

### Focus → Cards → Rapport → Flow
```
Higher states → More focus capacity
More capacity → Access to higher focus cards
Higher focus cards → Bigger rapport changes
More rapport → Better success rates
Better success → Positive flow
Flow ±3 → State transitions
```

## Strategic Resource Management

### Resource Pressure Points

**Morning Attention Calculation**:
- Base 10 attention
- Hunger reduces it (every 25 hunger = -1 attention)
- Minimum 2 (never completely blocked)
- Creates pressure to manage hunger without hard lock

**Focus Depletion**:
- Need high-focus request card → Must reach better state
- Better state needs positive flow → Requires successful cards
- Success needs rapport → Must build early
- Focus depletes → Must LISTEN to refresh
- LISTEN costs patience → Limited turns available

**Queue Displacement Cascade**:
- Need urgent delivery → Must displace
- Displacement burns tokens → Future conversations start with less rapport
- Lower starting rapport → Harder conversations
- Harder conversations → More likely to fail requests
- Failed requests → Burden cards accumulate

**Satchel Limitations**:
- 5 slots for letters AND permits
- Full satchel blocks new letters
- Dropping letters damages relationships
- Permits compete with profitable letters

### Resource Conservation Strategies

**Token Preservation**:
- Accept fixed queue positions to avoid burning
- Focus deliveries on specific NPCs for concentrated tokens
- Use observations to improve success without token cost

**Focus Efficiency**:
- Chain low-focus cards before refreshing
- Use atmosphere to expand capacity
- Time impulse cards before SPEAK removes them

**Attention Efficiency**:
- Chain obligations in same location
- Observe before conversing for unique advantages
- Work only when coins critically needed

**Time Optimization**:
- Plan routes to minimize travel
- Accept letters with compatible deadlines
- Use wait actions strategically

## No Threshold Design

Every resource scales linearly:
- Each rapport point: exactly +2% success
- Each token: exactly 1 starting rapport
- Each 25 hunger: exactly -1 attention
- Each patience: exactly 1 turn
- Each focus point: exactly that much from pool
- Each difficulty tier: specific base percentage

Only exception: Flow ±3 triggers state transitions (necessary mechanical breakpoint).

## Intentional Mechanic Design

Examples of clean separation:

**BAD**: "Routes require access permit OR 10 coins"
**GOOD**: Routes require access permit. Guards can be bribed for permits. Merchants sell permits.

**BAD**: "High tokens unlock better cards AND improve success"  
**GOOD**: Tokens provide starting rapport. Focus capacity determines playable cards.

**BAD**: "Hunger reduces patience AND attention"
**GOOD**: Hunger reduces attention only. Patience is per-NPC, not affected by player resources.

**BAD**: "Atmosphere affects focus AND success"
**GOOD**: Prepared atmosphere affects focus capacity. Focused atmosphere affects success. Each atmosphere has ONE effect.

## Resource Flow Visibility

All resource effects visible to player:
- Success rates shown before playing cards (base + rapport modifier)
- Focus costs displayed on all cards
- Current focus always visible
- Displacement costs shown when viewing queue
- Time costs shown on routes
- Attention formula transparent
- Flow position always visible (-3 to +3)
- Active atmosphere displayed prominently
- Current rapport clearly shown

No hidden calculations. Perfect information for strategic decisions.

## Economic Balance Points

### Daily Attention Budget
- 10 attention (well-fed) allows:
  - 5 observations OR
  - 2 conversations + 1 work OR
  - 1 conversation + 4 observations

### Letter Profitability
- Request cards have fixed terms (no negotiation)
- Typical: 5-15 coins, 2-6 hour deadline, position varies
- Success builds tokens for future rapport
- Failure adds burden cards

### Token Investment Return
- 3 tokens = 3 starting rapport = +6% all cards
- 6 tokens = 6 starting rapport = +12% all cards
- 10 tokens = 10 starting rapport = +20% all cards
- 25 tokens = 25 starting rapport = +50% all cards (halfway to guarantee)
- Burning 10 tokens severely damages multiple relationships

### Focus Management
- Disconnected (3 capacity): Can play three 1-focus or one 3-focus card
- Neutral (5 capacity): Can play request cards if exactly 5 focus
- Connected (6 capacity): Full flexibility for any combination
- Prepared atmosphere: +1 enables request cards in Open state

## Extensibility Through Resource Configuration

New content uses same resources differently:

**Disconnected Scenario**: Low focus capacity creates severe limitations
**Political Scenario**: Build Status tokens for better starting rapport with nobles
**Merchant Campaign**: Commerce tokens essential for trade relationships
**Shadow Path**: Shadow tokens help with spy networks
**Temple Route**: Trust tokens needed for clergy relationships

Same resources, same mechanics, but different token types create distinct strategic paths through starting rapport advantages.

## Core Innovation

Resources flow through multiple mechanics without any mechanic doing multiple things:

- Tokens determine starting rapport (one mechanic) for all conversations equally
- Different token types create relationship specialization
- Focus limits card plays (one mechanic) while atmosphere modifies capacity (different mechanic)
- Flow tracks success/failure (one mechanic) while rapport modifies success chance (different mechanic)
- Attention enables conversations (one mechanic) AND observations (different mechanic) AND work (third mechanic)

This creates strategic depth from simple, intentional rules. Every resource matters in multiple ways through different systems. The economy is the intersection of these mechanics, not their individual complexity.