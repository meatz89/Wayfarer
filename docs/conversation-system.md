# Wayfarer Conversation System Design

## Core Concept
Each NPC owns a conversation deck representing their personality, experiences, and relationship with the player. Players navigate these decks through strategic listening and speaking, building comfort to unlock letters and deepen relationships. The system uses a Listen/Speak dichotomy inspired by Jaipur, creating clear decision points each turn.

## The Core Loop

### Each Turn (Costs 1 Patience)
Players make ONE choice:

1. **LISTEN**: Draw 2 cards from NPC deck into hand
   - Opportunity cards in hand vanish (moments pass)
   - Persistent cards remain
   - Gain information but lose fleeting moments

2. **SPEAK**: Play cards from hand with restrictions
   - **Coherent Expression**: Unlimited cards of SAME type (get set bonus)
   - **Scattered Thoughts**: Up to 2 cards of MIXED types (no bonus)
   - **Weight Limit**: Total emotional weight ≤ 7
   - **Special Rules**: Crisis cards must be played alone
   - Each card rolls for success individually

### Why This Works
- Binary choice creates clear decisions
- Listen vs Speak models conversation rhythm
- Hand limit (7) represents mental capacity
- Patience as turns creates time pressure
- Opportunity cards create meaningful trade-offs

## Starting a Conversation

1. **Cost**: 1 attention point to initiate
2. **Starting Patience**: Base (8-15) + Trust bonus - Emotional penalties = Turns available
3. **Starting Hand**: Draw 3 cards
4. **Starting State**: Usually Neutral (may vary based on NPC emotion)
5. **Starting Depth**: 0 (Surface level)

## Card Anatomy

```
Card Name [Rarity: Common/Uncommon/Rare/Legendary]
Type: Trust/Commerce/Status/Shadow/Neutral
Persistence: Persistent/Opportunity/One-shot/Burden/Crisis
Emotional Weight: 0-5 (affects success chance, not playability)
Optimal Depth: 0-3 (best effectiveness level)

Effect:
- Comfort: +X (base value)
- Special: [Create state/Generate letter/Obligation/etc.]

Success Rate = 70% - (Weight × 10%) - (20% if wrong depth) + State bonus + Status bonus
```

## Card Persistence Types

### Persistent
- Remains in hand if not played when Listening
- Basic conversation options (Small Talk, Listen, Nod)
- Always available fallbacks

### Opportunity
- **Vanishes if you Listen** (moment passes)
- Time-sensitive topics, emotional openings
- Creates tension between drawing and playing

### One-shot
- Stays in hand if not played (too important to vanish)
- Permanently removed from deck after playing
- Major confessions, life-changing promises

### Burden
- Cannot vanish, must be addressed
- Negative cards from failures or broken promises
- Clogs hand until played and resolved

### Crisis
- Only appears when NPC is desperate
- Often costs extreme amounts (all remaining patience)
- Can break normal rules

## Emotional Weight System

Weight represents how emotionally difficult something is to express:
- **Weight 0**: Trivial (nod, small talk) - No penalty
- **Weight 1**: Light (casual stories) - -10% success
- **Weight 2**: Moderate (personal shares) - -20% success
- **Weight 3**: Heavy (deep feelings) - -30% success
- **Weight 4**: Intense (confessions) - -40% success
- **Weight 5**: Overwhelming (desperate pleas) - -50% success

Weight is NOT a cost - all cards are playable regardless of weight.

## Depth System

### Depth Levels
- **Depth 0**: Surface (small talk, pleasantries)
- **Depth 1**: Personal (sharing experiences)
- **Depth 2**: Intimate (deep connection)
- **Depth 3**: Soul-deep (profound moments)

### Optimal Depth vs Current Depth
- **At optimal depth**: Full comfort reward
- **Below optimal**: -50% comfort (too intimate too soon)
- **Above optimal**: -25% comfort (too shallow for connection)

Cards are NEVER locked by depth - you can always attempt them.

### Depth Progression
Depth advances when:
- Playing resonant cards in positive states
- Achieving breakthrough moments (comfort 10+ in single turn)
- Special card effects

Depth decreases when:
- Playing severely mismatched cards
- Major failures on high-weight cards
- Negative state transitions

## Set Bonuses and Expression Types

### Coherent Expression (Same Type)
Playing multiple cards of the SAME type in one SPEAK action:
- **1 card**: Base comfort only
- **2 same type**: +2 comfort bonus
- **3 same type**: +5 comfort bonus
- **4+ same type**: +8 comfort bonus
- No limit on number if all same type (weight permitting)

### Scattered Expression (Mixed Types)
Playing cards of DIFFERENT types:
- Maximum 2 cards when mixing types
- No set bonus
- Represents unfocused thoughts
- Still subject to weight limit

### Weight Limit
Total emotional weight per SPEAK action cannot exceed 7:
- Prevents emotional overload
- Crisis cards ignore this limit
- Represents conversational capacity

## Conversation States

One state active at a time, affecting the conversation's emotional climate.

### Positive States
- **Open**: Trust cards resonate, can advance depth
- **Warm**: All weights -1 (easier expression)
- **Vulnerable**: Double comfort or nothing (high risk/reward)
- **Professional**: Commerce/Status cards resonate

### Negative States
- **Guarded**: All comfort -2, cannot advance depth
- **Tense**: All weights +1, cannot advance depth

### Special State Interactions
- **Desperate NPCs**: More cards become Opportunities
- **Vulnerable state**: Most cards become fleeting
- States can modify card persistence

## Success Calculation

```
Base Success Rate = 70%
- (Weight × 10%)
- 20% if not at optimal depth
+ 10% if card resonates with state
- 10% if card conflicts with state
+ (Status tokens × 3%)
+ Special modifiers

Minimum: 10%, Maximum: 95%
```

Players see exact percentages before committing.

## Comfort System

### Building Comfort
- Accumulates through successful card plays
- Set bonuses multiply effectiveness
- State modifiers apply after base calculation
- Some cards give comfort even on failure

### Comfort Thresholds
- **5 comfort**: Relationship maintained (+1 token)
- **10 comfort**: Progress achieved (letter cards activate)
- **15 comfort**: Strong connection (+2 tokens)
- **20 comfort**: Perfect conversation (special rewards)
- **Below 5**: Relationship strains (-1 token)

## Hand Management

### Hand Limit: 7 Cards
- Represents conversational working memory
- Cannot draw beyond 7
- Must play or Listen choices become forced
- Burden cards count against limit

### Drawing Strategy
- Listen gives 2 cards but Opportunities vanish
- More cards enable better sets
- Risk of losing crucial moments
- Must balance information vs action

## Token System

### Token Types and Effects
- **Trust**: +1 patience per 2 tokens (more conversation time)
- **Commerce**: Set bonuses improved by 1
- **Status**: +3% success rate per token
- **Shadow**: Once per conversation, prevent 1 Opportunity from vanishing

### Earning Tokens
- Reaching comfort thresholds
- Delivering letters successfully
- Special card effects
- Perfect conversations (15+ comfort)

## Letter Generation

### How Letters Emerge
1. Build comfort to threshold (usually 10+)
2. Letter cards appear in draws at relationship milestones
3. Successfully play letter card when drawn
4. Letter enters queue system

### Letter Card Requirements
- Added to deck at Trust/Commerce/Status 3, 5, 7
- Must achieve comfort threshold in conversation
- One letter maximum per conversation
- Type matches dominant relationship

## NPC Personalities

### Personality Types
- **Devoted** (family, clergy): 12-15 base patience, Trust-focused
- **Mercantile** (traders): 10-12 patience, Commerce-focused
- **Proud** (nobles): 8-10 patience, Status-focused
- **Cunning** (spies): 10-12 patience, Shadow-focused
- **Steadfast** (workers): 11-13 patience, balanced

### Emotional States
- **Desperate** (<6 hours on letter): -3 patience, crisis cards appear, more Opportunities
- **Anxious** (6-12 hours): -1 patience, starts Tense
- **Hostile** (failed letter): Cannot converse
- **Neutral**: Normal patience and cards

## Deck Evolution

### Starting Deck (15 cards)
- 5 universal basics (always Persistent)
- 6 personality-specific cards
- 3 contextual cards (mix of types)
- 1 mild burden

### Deck Growth (max 25 cards)
- Letter delivery adds powerful cards
- Observations add Opportunity cards
- Failures add Burden cards
- Perfect conversations transform negatives

### Deck Curation
When at maximum, must replace cards to add new ones, forcing relationship shaping.

## Drawing and Card Generation

### Standard Listen Action
When choosing LISTEN, draw 2 cards:
- Random from NPC deck
- Filtered by current depth:
  - Depth 0: Draw from depth 0-1 cards
  - Depth 1: Draw from depth 0-2 cards
  - Depth 2+: Draw from any depth
- Opportunity cards in hand vanish first

### Special Card Injection
Certain triggers add cards DIRECTLY to hand (bypassing Listen):
- **NPC becomes desperate**: 1-2 crisis cards inject to hand
- **Player observation**: Relevant card injects to hand
- **Emotional breakthrough**: Special cards may inject
- Can cause hand overflow (>7 cards), forcing immediate Speak

### Deck Management
- **No reshuffling** during conversation
- **Played cards** go to discard pile
- **One-shots** removed permanently when played
- **Opportunities** removed if not played by conversation end
- **Persistent cards** return to deck after conversation
- **Burdens** remain in deck until resolved

### Empty Deck Scenario
If deck runs out:
- Can only play from hand
- Listen action draws nothing
- Conversation naturally concludes soon

## Observation Integration

When players observe events in the world:
- Cards inject DIRECTLY to hand (not deck)
- Usually Opportunity type (time-sensitive)  
- "Mention the guards" only relevant now
- Can cause hand overflow, forcing quick decisions
- Creates immediate conversational pressure

## Special Card Restrictions

### Crisis Cards
- Must be played alone (cannot combine)
- Ignore weight limits
- Often end conversation immediately
- Represent desperate moments

### One-Shot Cards  
- Maximum one per SPEAK action
- Too important to rush
- Cannot combine with crisis cards
- Permanently removed after playing

### Burden Cards
- Can be mixed with regular cards
- Add awkwardness to expression
- Must be resolved eventually
- Count toward weight limit

## Example Turn Flow

**Elena Conversation (Trust 4, Desperate, Depth 1)**

Starting: 5 patience (turns remaining)

**Hand**: 3 Trust Opportunities (weight 2 each), 1 Persistent (weight 0), 1 One-shot (weight 4), 1 Crisis (weight 5)

**Turn 5 Decision**:
- **Listen**: Lose 3 Opportunities, draw 2 new cards
- **Speak**: Play cards within restrictions

**Option A - Coherent Expression**: 
Play 3 Trust cards (same type, total weight 6 ≤ 7):
- Card 1: Weight 2, 50% success → Success! +4 comfort
- Card 2: Weight 2, 50% success → Success! +4 comfort  
- Card 3: Weight 2, 50% success → Fail, +1 comfort
- Set bonus: +5 comfort
- Total: 14 comfort (letter threshold reached!)

**Option B - Crisis**:
Play Crisis card alone (ignores weight limit):
- Weight 5, 45% success → Success! 
- Generates letter immediately
- Conversation ends

**Option C - Mixed Expression**:
Play 1 Trust + 1 Persistent (mixed, max 2 cards):
- No set bonus
- Lower comfort gain
- Preserves other Opportunities

## Design Philosophy

### Mechanical Elegance
- One choice per turn (Listen or Speak)
- All cards always playable
- Weight affects difficulty, not availability
- Depth affects effectiveness, not access
- No complex state tracking

### Verisimilitude
- Can attempt anything anytime (like real conversation)
- Heavier topics harder to land
- Mistimed intimacy backfires
- Some moments are fleeting
- Time pressure from limited turns

### Player Experience
- Clear decisions with visible consequences
- Discovery through deck exploration
- Tension from vanishing Opportunities
- Satisfaction from successful sets
- Recovery possible from failures

## Key Differences from Traditional Systems

### What Makes This Unique
1. **Listen/Speak Dichotomy**: Not playing cards one by one, but choosing to gather or express
2. **Persistence Types**: Cards that vanish create temporal pressure
3. **Weight Not Cost**: Emotional burden affects success, not playability
4. **All Cards Playable**: No dead draws, only suboptimal timing
5. **Turns Not Points**: Patience is time remaining, not spent per card
6. **Extended Expression**: Multiple cards = one coherent statement

### Why It Works
The system models real conversation: sometimes you listen to understand better, sometimes you express yourself fully. Moments pass if not seized. Heavy topics are always attemptable but harder to land. Time is limited. Every choice matters, but nothing is ever impossible.