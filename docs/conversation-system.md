# Wayfarer Conversation System Design

## Core Concept
Each NPC owns a conversation deck representing their personality, experiences, and relationship with the player. Players navigate these decks through strategic listening and speaking, building comfort to unlock letters and deepen relationships. The system uses a Listen/Speak dichotomy creating clear decision points each turn, with emotional states as the dynamic element affecting all interactions.

## The Core Loop

### Each Turn (Costs 1 Patience)
Players make ONE choice:

1. **LISTEN**: Effects vary by emotional state
   - ALL Opportunity cards currently in hand vanish (moments pass)
   - Draw X cards based on state (1-3)
   - State may transition (usually toward Neutral)
   - Persistent cards remain in hand

2. **SPEAK**: Play cards from hand with state-based restrictions
   - **Weight Limit**: Varies by state (1-4)
   - **Coherent Expression**: Unlimited cards of SAME type (get set bonus)
   - **Scattered Thoughts**: Up to 2 cards of MIXED types (no bonus)
   - **Special Rules**: Crisis cards must be played alone
   - Each card rolls for success individually

### Why This Works
- Binary choice creates clear decisions
- States modify both actions differently
- Listen manages emotional space, not just card draw
- Weight limit of 3 (normal) forces selective expression
- Opportunity cards create meaningful trade-offs

## Emotional State System

### The Central Dynamic
NPC emotional state is THE core system element. Each state defines:
1. **LISTEN effect** - How many cards drawn (1-3)
2. **SPEAK constraint** - Weight limit for that turn (1-4)
3. **LISTEN transition** - How state changes when listening

### 9 Core Emotional States

**NEUTRAL** (Default)
- LISTEN: Draw 2 cards
- SPEAK: Weight limit 3
- LISTEN→ Stays Neutral

**GUARDED** 
- LISTEN: Draw 1 card
- SPEAK: Weight limit 2
- LISTEN→ Neutral

**OPEN**
- LISTEN: Draw 3 cards
- SPEAK: Weight limit 3
- LISTEN→ Stays Open

**TENSE**
- LISTEN: Draw 1 card
- SPEAK: Weight limit 1
- LISTEN→ Guarded

**EAGER**
- LISTEN: Draw 3 cards
- SPEAK: Playing 2+ same-type cards gets +3 comfort bonus
- LISTEN→ Stays Eager

**OVERWHELMED**
- LISTEN: Draw 1 card
- SPEAK: Maximum 1 card only
- LISTEN→ Neutral

**CONNECTED** (Pinnacle state)
- LISTEN: Draw 3 cards
- SPEAK: Weight limit 4
- LISTEN→ Stays Connected
- Special: Depth advances automatically at turn end

**DESPERATE** (Crisis state)
- LISTEN: Draw 2 cards + inject 1 crisis card
- SPEAK: Crisis cards cost 0 weight
- LISTEN→ Hostile (escalates!)

**HOSTILE** (Escalated crisis)
- LISTEN: Draw 1 card + inject 2 crisis cards
- SPEAK: Only crisis cards playable
- LISTEN→ Conversation ends (breakdown)

### State Paths
States form emotional journeys:
- **Negative Journey**: Tense → Guarded → Neutral
- **Positive Journey**: Neutral → Open → Connected  
- **Crisis Journey**: Desperate → Hostile
- **Recovery Journey**: Overwhelmed → Neutral

### Why States Matter
- No percentages or modifiers - only clear binary effects
- States tell emotional story of conversation
- Managing states often more important than comfort
- Depth can only advance in: Neutral, Open, Connected

## Starting a Conversation

### Standard Start
1. **Cost**: 1 attention point to initiate
2. **Starting Patience**: Base (8-15) + Trust bonus - Emotional penalties = Turns available
3. **Starting Hand**: Draw 3 cards from NPC deck + Any relevant observation cards
4. **Starting State**: Based on NPC emotional condition
5. **Starting Depth**: 0 (Surface level)

### Starting Hand with Observations
- Base: 3 cards drawn from NPC deck
- Plus: Any observation cards relevant to this NPC
- If total starting hand > 7: Hand overflow occurs
- Each observation is ONE-USE - consumed when converted to card

### Hand Overflow Rules
When starting hand exceeds 7 cards:
- **Forced SPEAK**: Must SPEAK on turn 1 (cannot LISTEN)
- **Narrative**: Represents urgency to share information
- **Strategic**: Creates unique opening when heavily prepared

## Observation System Integration

### How Observations Become Cards

1. **Location Phase**: 
   - Spend 1 attention to observe something specific
   - Creates "observation token" - knowledge possessed
   - Each observation shows which NPCs would care

2. **Conversation Start**:
   - Relevant observations convert to Opportunity cards
   - Added to starting hand automatically
   - Marked as "From [Location] observation"
   - Always Opportunity type (will vanish if not used)

3. **Usage**:
   - Play like any other card (subject to weight limit)
   - Some observations create states when played
   - Once played or vanished, observation is consumed
   - Cannot be used in future conversations

### Example Flow
```
Market Square: Observe "Guards blocking north road" (-1 attention)
→ Start conversation with Elena
→ Starting hand: 3 drawn cards + 1 observation card = 4 total
→ Observation card is Opportunity (fleeting)
→ Playing it creates Tense state (urgent information)
→ If you LISTEN instead, observation vanishes forever
```


## Card Types - Separation of Purpose

### Three Distinct Card Types

**1. Comfort Cards**
- Primary purpose: Build comfort
- CAN combine with other comfort cards
- Get set bonuses for same type
- NEVER change state
- Success/Failure only affects comfort amount

**2. State Cards**
- Primary purpose: Change emotional state
- MUST be played alone
- NO comfort gain (or minimal)
- Success/Failure determines state change
- Clearly marked as "STATE CARD"

**3. Crisis Cards**
- Emergency actions
- MUST be played alone
- Ignore weight limits
- Free to play in Desperate/Hostile states
- Often end conversation

### Why This Separation Works
- No conflicts possible - state cards can't combine
- Clear purpose for each card type
- Strategic choice: comfort within state OR change state
- Matches real conversation dynamics

## Card Anatomy

### Comfort Cards (Most common)
```
Card Name
Type: Trust/Commerce/Status/Shadow
Persistence: Persistent/Opportunity/One-shot/Burden
Weight: 0-3

Effects:
- Success: +X comfort
- Failure: +Y comfort (less)
```

### State Cards (Rare, powerful)
```
Card Name [STATE CARD]
Type: Trust/Commerce/Status/Shadow  
Persistence: Usually Persistent
Weight: 1-2 (must fit in constrained states)

Effects:
- Success: [State A] → [State B]
- Failure: No state change
```

### Crisis Cards (Emergency)
```
Card Name [CRISIS]
Type: Usually Trust
Persistence: Crisis
Weight: 5+ (but free in crisis states)

Effects:
- Success: Major effect (letter, obligation, etc.)
- Failure: Negative consequence + state change
```

## Card Persistence Types

### Persistent
- Remains in hand if not played when Listening
- Basic conversation options (Small Talk, Listen, Nod)
- Always available fallbacks

### Opportunity
- **Vanishes if you LISTEN** (ALL Opportunities in hand disappear)
- Time-sensitive topics, emotional openings, observations
- Creates tension between drawing and playing
- Observation cards are ALWAYS this type

### One-shot
- Stays in hand if not played (too important to vanish)
- Permanently removed from deck after playing
- Major confessions, life-changing promises

### Burden
- Cannot vanish, must be addressed
- Negative cards from failures or broken promises
- Clogs hand until played and resolved

## Weight System

### Weight Limits by State
Weight represents emotional bandwidth. Total weight per SPEAK action cannot exceed state limit:
- **Tense**: Weight limit 1
- **Guarded**: Weight limit 2
- **Neutral/Open**: Weight limit 3
- **Connected**: Weight limit 4 (exceptional)
- **Overwhelmed**: Maximum 1 card regardless of weight
- **Desperate/Hostile**: Crisis cards cost 0 weight

### Weight Values
- **Weight 0**: Trivial (nod, small talk)
- **Weight 1**: Light (casual stories)
- **Weight 2**: Moderate (personal shares)
- **Weight 3**: Heavy (deep feelings)

### Why Weight 3 Maximum (Normal)
- Forces genuine choice between options
- One heavy statement OR two light ones OR three trivial
- Represents realistic emotional bandwidth
- Prevents "dump hand" strategies

## Success Calculation

```
Base Success Rate = 70%
- (Weight × 10%)
+ (Status tokens × 3%)

Minimum: 10%, Maximum: 95%
```

Simple, clear, no hidden modifiers. States don't affect success rates - they affect what's possible.

## Depth System (Simplified)

### Depth Levels
- **Depth 0**: Surface (small talk, pleasantries)
- **Depth 1**: Personal (sharing experiences)
- **Depth 2**: Intimate (deep connection)
- **Depth 3**: Soul-deep (profound moments)

### Depth Progression
Depth can ONLY advance in positive states:
- **Neutral**: Can advance with breakthrough (10+ comfort single turn)
- **Open**: Can advance normally
- **Connected**: Advances automatically each turn

Depth decreases when:
- Conversation ends below 5 comfort
- Major failures on heavy cards
- Certain state transitions

### No Depth Requirements
Cards don't have depth requirements or penalties. Depth affects:
- Which cards appear in draws
- Comfort thresholds for letters
- Narrative framing

## Set Bonuses and Expression Types

### Coherent Expression (Same Type)
Playing multiple cards of the SAME type in one SPEAK action:
- **1 card**: Base comfort only
- **2 same type**: +2 comfort bonus
- **3 same type**: +5 comfort bonus
- **4+ same type**: +8 comfort bonus (rare with weight limits)

### Scattered Expression (Mixed Types)
- Maximum 2 cards when mixing types
- No set bonus
- Still subject to weight limit

### Special Bonuses
- **Eager state**: +3 bonus for playing 2+ same type
- **Connected state**: All comfort gains +2

## Comfort System

### Building Comfort
- Accumulates through successful card plays
- Set bonuses multiply effectiveness
- Some states modify comfort (Connected: +2 all gains)
- Cards show base comfort value only

### Comfort Thresholds
- **5 comfort**: Relationship maintained (+1 token)
- **10 comfort**: Progress achieved (letter cards activate)
- **15 comfort**: Strong connection (+2 tokens)
- **20 comfort**: Perfect conversation (special rewards)
- **Below 5**: Relationship strains (-1 token)

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
4. Physical letter enters inventory (satchel)
5. New delivery obligation created in queue

### Letter Card Requirements
- Added to deck at Trust/Commerce/Status 3, 5, 7
- Must achieve comfort threshold in conversation
- One letter maximum per conversation
- Letter cards are Persistent (don't vanish)

### Physical Letters vs Obligations
- **Physical Letter**: Item in satchel with sender/recipient
- **Delivery Obligation**: Deadline and stakes in queue
- **Separation**: Letter doesn't expire, obligation does

## Obligation System and NPC States

### Two Types of Obligations

**Meeting Obligations**
```
"Meet Elena at Tavern"
Deadline: 2 hours
Stakes: SAFETY (her marriage)
Effect: Determines NPC emotional state
```

**Delivery Obligations**
```
"Deliver Elena's letter to Lord Blackwood"  
Deadline: 6 hours (set when letter received)
Stakes: REPUTATION
Requires: Physical letter in satchel
```

### Physical Letters vs Obligations
- **Physical Letters**: Inventory items in your satchel
- **Obligations**: Time pressure tracked in queue
- **NPCs**: Don't track deadlines, react to YOUR meeting obligations
- **Emotional States**: Derived from how late you are to meet them

### How Meeting Obligations Work

1. **NPC sends urgent word** → Meeting obligation enters queue
2. **Time remaining determines emotional state**:
   - <2 hours: Desperate
   - 2-6 hours: Tense
   - 6+ hours: Neutral
   - Expired: Hostile (cannot converse)
3. **During conversation** → May receive physical letter
4. **New delivery obligation** → Created when letter received

This means Elena's desperation isn't because her letter is expiring - it's because YOU'RE almost too late to meet her!

## NPC Personalities

### Personality Types
- **Devoted** (family, clergy): 12-15 base patience, Trust-focused
- **Mercantile** (traders): 10-12 patience, Commerce-focused
- **Proud** (nobles): 8-10 patience, Status-focused
- **Cunning** (spies): 10-12 patience, Shadow-focused
- **Steadfast** (workers): 11-13 patience, balanced

### Emotional States Based on Meeting Obligations
- **Desperate** (<2 hours on meeting): Starts in Desperate state
- **Tense** (2-6 hours on meeting): Starts in Tense state
- **Hostile** (failed to meet): Cannot converse
- **Neutral** (no urgent meeting): Starts in personality default

## Deck Evolution

### Starting Deck (15 cards)
- 5 universal basics (always Persistent)
- 6 personality-specific cards
- 3 contextual cards (mix of types)
- 1 mild burden

### Deck Growth (max 25 cards)
- Letter delivery adds powerful cards
- Only special "state cards" manipulate emotions (rare)
- Most cards are comfort cards (weight + comfort + type)
- Failures add Burden cards
- Perfect conversations transform negatives

### Card Design Philosophy
- **Most cards are comfort cards**: Weight, comfort, type only
- **State cards are rare**: Clear markers, must be played alone
- **Crisis cards are special**: Ignore weight limits, end conversations
- **No complex requirements**: All cards always playable (weight permitting)
- **Clear separation**: Each card type has distinct purpose

## Drawing and Card Generation

### Standard Listen Action
When choosing LISTEN:
1. ALL Opportunities in hand vanish first
2. Draw X cards based on current state (1-3)
3. State may transition based on state rules
4. Exception: Reflecting state preserves Opportunities

### Special Card Injection
- **Desperate state**: Injects 1 crisis card when listening
- **Hostile state**: Injects 2 crisis cards when listening
- **Observations**: Added to starting hand only

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
- Conversation naturally concludes

## State Manipulation

### How States Change

**Through Listening**:
Each state has defined transition when listening
- Negative states gradually relax toward Neutral
- Positive states often maintain
- Crisis states escalate if ignored (Desperate → Hostile)

**Through State Cards**:
Only STATE CARDS can change states when speaking
- Must be played alone
- Success changes state as specified
- Failure maintains current state
- Clearly marked to avoid confusion

Example State Cards:
```
"Calm Reassurance" [STATE CARD]
Success: Desperate → Tense
Failure: No change

"Break the Ice" [STATE CARD]
Success: Guarded → Neutral
Failure: No change

"Share Vulnerability" [STATE CARD]
Success: → Connected
Failure: → Guarded
```

**State Cards Cannot Combine**:
This prevents conflicts - you either:
- Play comfort cards to build within current state
- Play one state card to shift emotional landscape
- Never both simultaneously

### State Strategy
- Sometimes worth spending turns to clear negative states
- State cards are precious - limited in deck
- Crisis states demand immediate attention
- Connected state is pinnacle but fragile

## Example Turn Flow

**Elena Conversation (Meeting obligation: 2 hours left → Desperate state)**

Starting patience: 5 turns
Starting state: Desperate (draw 2 + crisis, crisis free, listen worsens)

**Hand**: 
- "Promise to Help" - Trust Comfort card (weight 2)
- "Mention Guards" - Shadow Comfort card from observation (weight 1)
- "Calm Reassurance" - Trust State card (weight 1)
- "Desperate Promise" - Trust Crisis card (weight 5, but FREE in Desperate)

**Turn 1 Decision**:
- **Listen**: Lose both comfort cards (Opportunities), draw 2 + crisis card, state → Hostile
- **Speak**: Play within weight limit (crisis free in this state)

**Option A - Crisis Resolution**:
Play "Desperate Promise" alone (free in Desperate state):
- 43% success → Generates letter, ends conversation
- 57% failure → +5 comfort, state → Overwhelmed

**Option B - State Management**:
Play "Calm Reassurance" alone (state card, must be solo):
- 63% success → State → Tense (improved!)
- 37% failure → State stays Desperate

**Option C - Comfort Building**:
Play "Promise" + "Mention Guards" (both comfort cards, weight 3 total):
- No state change possible when combining
- Promise: 53% → +4 comfort on success
- Guards: 63% → +2 comfort on success
- Total if both succeed: 6 comfort

## Design Philosophy

### Mechanical Elegance
- One choice per turn (Listen or Speak)
- States create the dynamic element
- Weight limits by state, not math
- Cards are simple: weight + comfort + type
- No hidden requirements or modifiers

### Verisimilitude
- Emotional states affect everything naturally
- Can attempt anything (weight permitting)
- Some moments are fleeting (Opportunities)
- Limited emotional bandwidth (weight limits)
- Listening gives space, speaking uses it

### Player Experience
- Clear decisions with visible consequences
- State management as strategic layer
- Observation rewards exploration
- Crisis moments demand attention
- Recovery always possible

## Key Innovations

### What Makes This Unique
1. **Card Type Separation**: Comfort cards build rapport, state cards shift emotions, crisis cards handle emergencies - never mixing purposes
2. **Emotional States as Core**: 9 distinct states affect all aspects of conversation
3. **Listen as State Management**: Not just drawing cards but managing emotional space through defined transitions
4. **No State Conflicts**: State cards must be played alone, structurally preventing conflicts
5. **Observation Integration**: World knowledge becomes conversation ammunition
6. **Crisis Escalation**: Ignoring desperate situations makes them worse (Desperate → Hostile)
7. **Meeting Obligations**: NPC emotional states derived from YOUR punctuality, not their own deadlines

### Why It Works
The system models real conversation through mechanical state. You either work within the current emotional context (comfort cards) OR carefully shift the mood (state cards) - never both simultaneously. Emotional states create complete conversational dynamics - someone Guarded shares less (draw 1) and handles less (weight limit 2). Managing emotional space through listening is as strategic as what you say. NPCs react to how late you are to meet them, creating natural urgency. Crisis situations demand singular focus. Every conversation becomes a unique emotional journey with clear, predictable rules.