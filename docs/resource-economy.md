# Wayfarer: Resource Economy & Intentional Design

## Core Economic Philosophy

Every resource flows through multiple systems via different mechanics. Each mechanic has exactly ONE intentional effect. No thresholds (except comfort Â±3 for state transitions), no secondary effects, no "OR" statements. Resources create strategic depth through their multiple uses, not through complex mechanics.

## Primary Resources

### Attention
**Daily Allocation**: 10 - (Hunger Ã· 25), minimum 2

**Uses** (each via different mechanic):
- **Conversation Mechanic**: Spend 2 → Full emotional state conversation
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
- Letter delivery payments (5-20 coins based on negotiation)
- Exchange trades

**Strategic Role**: Flexible resource converter. Enables recovery from bad situations but competes with progression needs.

### Health
**Range**: 0-100 (death at 0)

**Effects**:
- **Weight Capacity**: Below 50 health → maximum weight capacity -1 in conversations
- **Starvation Damage**: -5 health per period at 100 hunger

**Restoration**:
- Rest exchanges
- Medical items from exchanges
- Time (slow natural recovery)

**Strategic Role**: Long-term sustainability. Affects conversation capability without being a hard gate.

### Hunger
**Range**: 0-100

**Effects**:
- **Attention Calculation**: Reduces morning attention by (Hunger Ã· 25)
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

**Single Mechanical Effect**: +5% success rate per token on MATCHING card types only

**Token-Type Matching**:
- Trust tokens ONLY boost Trust-type cards
- Commerce tokens ONLY boost Commerce-type cards
- Status tokens ONLY boost Status-type cards
- Shadow tokens ONLY boost Shadow-type cards

**Multiple Uses Through Different Mechanics**:
1. **Success Modifier**: Every token adds 5% to matching card types only
2. **Goal Negotiation**: Better terms with matching token types  
3. **Displacement Cost**: Burn tokens to jump queue positions
4. **Scaling Effects**: Some comfort cards scale with specific token types

Each use is a separate mechanic. Tokens never gate access to content.

**Token Acquisition** (ONLY through letter delivery):
- Standard successful delivery: +1 token with recipient (type matches letter)
- Excellent delivery (high negotiation success): +2-3 tokens with recipient
- Trust letters build Trust tokens
- Commerce letters build Commerce tokens
- Status letters build Status tokens
- Shadow letters build Shadow tokens

**Token Loss**:
- Failed deliveries: -2 tokens with sender (matching type)
- Queue displacement: -1 per position jumped per displaced NPC
- Can go negative (relationship debt)

## Conversation Resources

### Comfort (Temporary)
**Range**: -3 to +3 within single conversation
**Starting Value**: Always 0
**Effect**: At Â±3, triggers emotional state transition

**Modification**:
- Fixed comfort cards: +1 to +5 or -1 to -3 based on card
- Scaled comfort cards: +X where X varies by scaling type
- Atmosphere effects can modify comfort changes

**State Transitions at Â±3**:
- DESPERATE: +3→Tense, -3→Conversation ends
- TENSE: +3→Neutral, -3→Desperate
- NEUTRAL: +3→Open, -3→Tense
- OPEN: +3→Connected, -3→Neutral
- CONNECTED: +3→Stays Connected, -3→Open

**Strategic Role**: Tactical battery for triggering state transitions. Positive comfort leads to better states, negative to worse. Resets to 0 after each transition.

### Weight Pool (Per-Conversation)
**Capacity by Emotional State**:
- Desperate: 3
- Tense: 4
- Neutral: 5
- Open: 5
- Connected: 6

**Mechanics**:
- Refreshes to maximum on LISTEN
- Persists across SPEAK actions
- Each card costs its weight value
- Prepared atmosphere adds +1 capacity
- Health below 50 reduces capacity by 1

**Strategic Role**: Core resource management within conversations. Enables multi-turn planning with fleeting cards that require more weight than currently available.

### Atmosphere (Per-Conversation)
**Standard Atmospheres** (~30% of normal cards):
- **Neutral**: No effect (default, set after any failure)
- **Prepared**: +1 weight capacity
- **Receptive**: +1 card on LISTEN
- **Focused**: +20% success all cards
- **Patient**: Actions cost 0 patience
- **Volatile**: All comfort changes Â±1
- **Final**: Any failure ends conversation

**Observation-Only Atmospheres**:
- **Informed**: Next card cannot fail
- **Exposed**: Double all comfort changes
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
- Weight 1 (minimal pool requirement)
- Always persistent
- 85% success rate (Very Easy difficulty)
- Unique effects not available on normal cards
- Expire after 24-48 hours

**Unique Effects**:
- Set special atmospheres (Informed, Exposed, Synchronized, Pressured)
- Bypass costs (next action free, next SPEAK costs 0 weight)
- Unique manipulations (reset comfort to 0, refresh weight pool)

**Strategic Role**: Bypass normal conversation limitations. Reward exploration and information gathering. Create powerful one-time effects.

### Access Permits
**Special letter type** - no delivery obligation

**Properties**:
- Occupy satchel space (max 5 with letters)
- Enable specific routes
- Do not expire (physical documents)
- Cannot be dropped without consequence

**Acquisition**:
- Goal cards (requires successful negotiation)
- Exchange cards (15-20 coins)

**Strategic Role**: Gate exploration and enable efficient routing. Compete for limited satchel space.

### Burden Cards (Relationship Records)
**Not in conversation decks** - tracked per NPC relationship

**Acquisition**:
- Failed deliveries: +2 burden cards
- Queue displacement: +1 per token burned
- Broken promises: +1-2 burden cards

**Effects**:
- Block relationship progress
- Enable "Make Amends" conversation type
- Visual indicator of damaged relationships

**Resolution**:
- "Clear the Air" goal card removes burdens
- Very Hard difficulty (40% base + tokens)

**Strategic Role**: Permanent consequences that must be actively resolved. Create repair arcs for damaged relationships.

## Card Difficulty Resources

### Success Rate Tiers
Instead of flat 60%, cards use difficulty tiers:

**Very Easy** (85% base):
- Observation cards exclusively

**Easy** (70% base):
- Basic fixed comfort (+1, -1)
- Setup cards (0 weight atmosphere setters)

**Medium** (60% base):
- Standard fixed comfort (+2, -2)
- Utility cards (draw, weight-add)

**Hard** (50% base):
- Scaled comfort cards
- High fixed comfort (+3)
- Resolution goals

**Very Hard** (40% base):
- Dramatic fixed comfort (+4, +5, -3)
- Goal cards (letters, meetings)
- Crisis resolutions

**Token Modification**: +5% per token, applied equally to all difficulties

**Strategic Role**: Creates risk/reward calculations. Higher impact requires accepting lower base success.

## Resource Conversion Chains

### Time → Money → Progress
```
Work Action (2 attention + 4 hours) → 8 coins
8 coins → Food (reset hunger) → Better morning attention  
Better attention → More conversations → Goal card access
Goal cards → Letter obligations → Delivery success
Delivery success → Payment + Tokens
```

### Tokens → Success → Better Terms → More Tokens
```
Successful deliveries → +1-3 tokens
Higher tokens → Better card success rates
Better goal negotiation → Favorable queue position
Efficient completion → No displacement needed
Preserved tokens → Stronger future relationships
```

### Knowledge → Access → Efficiency
```
Observation (1 attention) → Unique effect card
Special atmosphere → Bypass normal limitations
Better success rates → Reach goal cards
Goal cards → Letters or permits
Permits → New routes → More opportunities
```

### Weight → Comfort → States → Capacity
```
Higher states → More weight capacity
More capacity → Access to higher weight cards
Higher weight cards → Bigger comfort changes
Comfort Â±3 → State transitions
Better states → More cards drawn on LISTEN
```

## Strategic Resource Management

### Resource Pressure Points

**Morning Attention Calculation**:
- Base 10 attention
- Hunger reduces it (every 25 hunger = -1 attention)
- Minimum 2 (never completely blocked)
- Creates pressure to manage hunger without hard lock

**Weight Pool Depletion**:
- Need high-weight goal card → Must reach better state
- Better state needs comfort building → Requires weight for comfort cards
- Weight pool depletes → Must LISTEN to refresh
- LISTEN costs patience → Limited turns available

**Queue Displacement Cascade**:
- Need urgent delivery → Must displace
- Displacement burns tokens → Relationships damaged
- Damaged relationships → Lower success rates
- Lower success → Worse negotiation terms
- Worse terms → More displacement needed

**Satchel Limitations**:
- 5 slots for letters AND permits
- Full satchel blocks new letters
- Dropping letters damages relationships
- Permits compete with profitable letters

### Resource Conservation Strategies

**Token Preservation**:
- Accept poor queue positions to avoid burning
- Focus deliveries on specific NPCs for concentrated tokens
- Use observations to improve success without token cost

**Weight Efficiency**:
- Chain low-weight cards before refreshing
- Use atmosphere to expand capacity
- Time fleeting cards before SPEAK removes them

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
- Each token: exactly +5% success
- Each 25 hunger: exactly -1 attention
- Each patience: exactly 1 turn
- Each weight point: exactly that much from pool
- Each difficulty tier: specific base percentage

Only exception: Comfort Â±3 triggers state transitions (necessary mechanical breakpoint).

## Intentional Mechanic Design

Examples of clean separation:

**BAD**: "Routes require access permit OR 10 coins"
**GOOD**: Routes require access permit. Guards can be bribed for permits. Merchants sell permits.

**BAD**: "High tokens unlock better cards AND improve success"  
**GOOD**: Tokens improve success rates. Weight capacity determines playable cards.

**BAD**: "Hunger reduces patience AND attention"
**GOOD**: Hunger reduces attention only. Patience is per-NPC, not affected by player resources.

**BAD**: "Atmosphere affects weight AND success"
**GOOD**: Prepared atmosphere affects weight capacity. Focused atmosphere affects success. Each atmosphere has ONE effect.

## Resource Flow Visibility

All resource effects visible to player:
- Success rates shown before playing cards (base + tokens)
- Weight costs displayed on all cards
- Current weight pool always visible
- Displacement costs shown when viewing queue
- Time costs shown on routes
- Attention formula transparent
- Comfort position always visible (-3 to +3)
- Active atmosphere displayed prominently

No hidden calculations. Perfect information for strategic decisions.

## Economic Balance Points

### Daily Attention Budget
- 10 attention (well-fed) allows:
  - 5 observations OR
  - 2 conversations + 1 work OR
  - 1 conversation + 4 observations

### Letter Profitability
- Poor negotiation (40% success): 5 coins, 2-hour deadline, position 1
- Average negotiation (60% success): 10 coins, 4-hour deadline, position 3
- Good negotiation (80% success): 15-20 coins, 6+ hour deadline, flexible position

### Token Investment Return
- 3 matching tokens: +15% success on same-type cards
- 6 matching tokens: +30% success on same-type cards
- 10 matching tokens: +50% success on same-type cards
- But wrong token type: +0% (Trust tokens useless on Commerce cards)
- Burning 10 tokens destroys multiple relationships

### Strategic Token Specialization
- **Devoted NPCs** (75% Trust cards): Build Trust tokens
- **Mercantile NPCs** (75% Commerce cards): Build Commerce tokens
- **Proud NPCs** (75% Status cards): Build Status tokens
- **Cunning NPCs** (75% Shadow cards): Build Shadow tokens
- **Steadfast NPCs** (balanced): Any tokens help equally

### Weight Pool Management
- Desperate (3 capacity): Can play three 1-weight or one 3-weight card
- Neutral (5 capacity): Can play goal cards if exactly 5 weight
- Connected (6 capacity): Full flexibility for any combination
- Prepared atmosphere: +1 enables goal cards in Open state

## Extensibility Through Resource Configuration

New content uses same resources differently:

**Desperate Scenario**: Low weight capacity creates severe limitations
**Political Scenario**: Status tokens critical for Status-type cards in Noble District
**Merchant Campaign**: Commerce tokens essential for Commerce-type cards in Markets
**Shadow Path**: Shadow tokens required for Shadow-type cards with spies
**Temple Route**: Trust tokens needed for Trust-type cards with clergy

Same resources, same mechanics, but token-type matching creates distinct strategic paths requiring specialized relationships.

## Core Innovation

Resources flow through multiple mechanics without any mechanic doing multiple things:

- Tokens affect success (one mechanic) BUT only on matching card types
- Different token types create specialization rather than generic power
- Weight pools limit card plays (one mechanic) while atmosphere modifies capacity (different mechanic)
- Comfort triggers transitions (one mechanic) while atmosphere can modify comfort changes (different mechanic)
- Attention enables conversations (one mechanic) AND observations (different mechanic) AND work (third mechanic)

This creates strategic depth from simple, intentional rules. Every resource matters in multiple ways, but token-type matching forces players to specialize their approach. The economy is the intersection of these mechanics, not their individual complexity.