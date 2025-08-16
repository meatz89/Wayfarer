# Wayfarer: Complete Game Design Document - Dual-Layer System

## Design Principles

### Verisimilitude Above All
- Cards represent shared memories with specific NPCs
- Relationships build through actions, not transactions
- Desperate people naturally have less patience
- Debt creates leverage even among friends
- Time only advances through your actions

### Two Complete Games
- **Strategic Layer**: Deck-building relationship management
- **Tactical Layer**: Route and queue optimization
- Both are complete games that require each other
- Neither is subordinate or optional

### Mechanical Elegance
- Each resource has ONE clear purpose
- Each relationship type has UNIQUE effects
- Simple rules create complex decisions
- Everything interconnects without redundancy

### Emergent Narrative
- Mechanical states tell complete stories
- AI interprets states into literary prose
- Same mechanics create different narratives
- Player choices shape unique storylines

## The Two-Layer System Architecture

### Layer 1: Conversation Deck-Building
Each NPC is represented by a deck of conversation cards. Through conversations, players navigate these decks using patience (NPC resource) to achieve comfort thresholds. Success unlocks letters. Letter delivery permanently improves decks.

### Layer 2: Letter Delivery Management
Letters are time-pressured obligations that must be routed efficiently through a constrained queue system. Completing letters builds specific relationship types and adds powerful cards to NPC decks, enabling deeper conversations.

### The Interconnection
- Conversations test your current deck → Success generates letters
- Letters create routing/time puzzles → Delivery improves decks
- Improved decks enable harder conversations → Unlock better letters
- Better letters provide bigger improvements → Access elite storylines

## The Conversation System - Strategic Relationship Building

### Core Mechanics

**Patience** (NPC's conversation energy):
- Base from personality (3-10)
- Modified by Trust level and emotional state
- Depletes as cards are played
- Determines conversation depth

**Success Probability**:
```
Success Chance = (Patience - Difficulty + 5) × 12%
```
Creates meaningful risk/reward decisions for every card played.

**Comfort** (Conversation progress):
- Always starts at 0
- Build through successful card plays
- Thresholds unlock rewards:
  - Comfort ≥ Patience/2: Maintain relationship
  - Comfort ≥ Patience: Letter becomes available
  - Comfort ≥ Patience × 1.5: Perfect conversation bonus

### The Deck-Building System

**Each NPC Has One Unique Deck**:
- Represents your shared history with that specific person
- 10-20 cards maximum
- Cards can only be added through meaningful actions
- Each card represents a type of interaction you've had

**Universal Starting Cards**:
1. "Small Talk" - Difficulty 2, Cost 1, +2 comfort
2. "Listen" - Difficulty 3, Cost 1, +3 comfort
3. "How Are Things?" - Difficulty 2, Cost 1, reveals if letter available
4. "Offer Help" - Difficulty 4, Cost 2, +4 comfort
5. "Quick Exit" - Difficulty 0, Cost 0, ends conversation
6-7. Two negative cards (Awkward Silence, Bad Memory)
8-10. Three personality-specific cards

**Card Evolution** (Only through actions):
- **Letter Delivery**: Add powerful cards matching letter type
- **Perfect Conversations**: Neutralize negatives to harmless versions
- **Failed Delivery**: Forced to add negative cards
- **Broken Promises**: Negative cards replace positives

### How Relationships Work

**Four Relationship Types** (Each mechanically distinct):
- **Trust**: Adds to starting patience (deeper conversations)
- **Commerce**: Reduces card patience costs by 1 (efficiency)
- **Status**: Reduces card difficulties by 1 (higher success)
- **Shadow 3+**: Play cards without requirements (bypass gates)

**Building Relationships**:
- Small gains through conversation successes
- Major gains through letter delivery
- Each type unlocks different letter types
- Negative relationships create mechanical disadvantages

### Conversation Flow

1. **Start**: Pay 1 attention (fixed cost)
2. **Calculate Patience**: Base + Trust - emotional penalties
3. **Draw Phase**: Draw 5 cards from NPC deck
4. **Play Phase**: Play cards spending patience
5. **Resolution**: Roll for success, apply effects
6. **Letter Generation**: If threshold met, letter cards become available
7. **End**: When patience depleted or player exits

### NPC Personalities and Emotional States

**Personality Types** (Determine base patience and letter preferences):
- **DEVOTED** (Family, Clergy): High patience (8-10), Trust letters
- **MERCANTILE** (Merchants): Medium patience (5-7), Commerce letters
- **PROUD** (Nobles): Low patience (3-5), Status letters
- **CUNNING** (Spies): Variable patience (4-6), Shadow letters
- **STEADFAST** (Workers): Balanced patience (6-8), Mixed letters

**Emotional States** (From letter deadlines):
- **DESPERATE** (<6 hours): -3 patience, crisis cards available
- **ANXIOUS** (6-12 hours): -1 patience
- **NEUTRAL** (no active letter): Normal patience
- **HOSTILE** (letter expired): Cannot converse until resolved

### Letter Generation Through Conversations

Letters aren't random - they emerge from conversation success:

1. **Reach Comfort Threshold**: Achieve Comfort ≥ Patience
2. **Letter Cards Unlock**: Added to hand if available
3. **Play Letter Card**: Must meet requirements
4. **Generate Letter**: Enters queue based on relationships
5. **Queue Position**: Determined by relationship values

This makes letter generation a strategic choice, not automatic.

### Core Mechanics

**Patience**: NPC's conversational energy (3-10 points)
- Determined by relationship depth + personality base
- Each card costs patience to play
- Depletes throughout conversation
- Determines success probability for all cards

**Success Probability Formula**:
```
Success Chance = (Patience - Difficulty + 5) × 12%
```
- Equal values (Patience 5, Difficulty 5) = 60% success
- Each point of patience advantage = +12% success
- Minimum 0%, Maximum 100%

**Outcome Thresholds**:
- Roll 66%+: Success outcome
- Roll 33-65%: Neutral outcome
- Roll 0-32%: Failure outcome

### Card Structure

Each conversation card contains:
```
Card Name: "Share Personal Story"
Requirements: Trust 2+ OR Previous "Small Talk" success
Difficulty: 5
Patience Cost: 2
Success: Adds "Deep Bond" to deck, +3 comfort
Neutral: +1 comfort
Failure: Adds "Awkward Memory" to deck
```

### Conversation Flow

1. **Conversation Start**: 
   - Calculate starting patience (base + relationship bonus - negative card penalties)
   - Negative cards in deck reduce starting patience by 0.5 each
   - Shuffle entire NPC deck

2. **Each Round**:
   - Draw exactly 5 cards from deck
   - Check requirements on all 5
   - Display cards that meet requirements + "Exit" option
   - Player selects one card to play

3. **Resolution**:
   - Pay patience cost
   - Roll for success based on current patience vs difficulty
   - Apply outcome effects
   - Reduce NPC patience

4. **Conversation End**:
   - When patience reaches 0
   - When player chooses "Exit"
   - When no playable cards remain

### Comfort System

**Comfort Scale**: 0-15 (tracked per conversation)
- Starts at 5
- Modified by card outcomes
- Natural decay of -1 per round if no comfort gained

**Comfort Thresholds** (Relative to Starting Patience):
- **Maintain Relationship**: Comfort ≥ Patience/2
- **Unlock Letter**: Comfort ≥ Patience
- **Perfect Conversation**: Comfort ≥ Patience × 1.5

Example: Elena with 8 patience needs comfort 4 to maintain, 8 for letter, 12 for perfect.

This scales naturally with NPC state - desperate NPCs with low patience are easier to satisfy but offer less.

### NPC Deck Composition

**Universal Starting Deck** (10 cards):
1. "Small Talk" - Difficulty 2, Cost 1 patience, +2 comfort
2. "Listen" - Difficulty 3, Cost 1 patience, +3 comfort  
3. "How Are Things?" - Difficulty 2, Cost 1 patience, reveals need
4. "Offer Help" - Difficulty 4, Cost 2 patience, +1 comfort, +1 token if comfort ≥5
5. "Quick Exit" - Difficulty 0, Cost 0, ends conversation
6. "Awkward Silence" - Difficulty 8, Cost 0, -2 comfort (negative)
7. "Bad Memory" - Difficulty 6, Cost 1, -1 comfort (negative)
8-10. Three personality-specific cards based on NPC type

This provides basic tools with clear room for growth through letter delivery.

**Card Types**:

**Basic Cards** (no requirements):
- "Small Talk": Difficulty 2, Cost 1, +2 comfort
- "Listen": Difficulty 3, Cost 1, +3 comfort
- "How Are Things": Difficulty 2, Cost 1, reveals letter need

**Relationship Cards** (require specific token levels):
- "Share Trust": Requires Trust 3+, Difficulty 5, Cost 2
- "Business Deal": Requires Commerce 2+, Difficulty 4, Cost 2
- "Formal Request": Requires Status 3+, Difficulty 6, Cost 3
- "Share Secret": Requires Shadow 2+, Difficulty 7, Cost 3

**Letter Unlock Cards** (require comfort threshold):
- "Ask for Trust Letter": Requires Comfort 8+, Difficulty 4
- "Request Commerce Letter": Requires Comfort 7+, Difficulty 3

**Negative Cards** (deck pollution):
- "Awkward Memory": Difficulty 2, Cost 2
  - Success: Remove from deck, +1 comfort
  - Failure: -3 comfort, adds another negative
- "Broken Promise": Difficulty 4, Cost 3
  - Success: Remove from deck
  - Failure: -5 comfort, damages relationship

**Advanced Cards** (added through letter delivery):
- "Deep Bond": Difficulty 3, Cost 2, +5 comfort
- "Perfect Trust": Difficulty 6, Cost 3, +8 comfort
- "Inside Joke": Difficulty 1, Cost 1, +4 comfort if comfort already 8+

### Deck Evolution Through Letter Delivery

### Letter Delivery Rewards by Type

When delivering a letter, choose ONE reward based on letter type:

**Trust Letter Rewards**:
- Add comfort-building card (high comfort gain)
- Upgrade "Small Talk" → "Warm Greeting" (lower difficulty)
- Neutralize emotional negative ("Awkward Memory" → "Resolved")

**Commerce Letter Rewards**:
- Add efficiency card (low patience cost)
- Upgrade all cards to cost -1 patience
- Remove resource negative ("Debt Burden")

**Status Letter Rewards**:
- Add reliability card (low difficulty)
- Upgrade all cards to -1 difficulty
- Remove social negative ("Social Blunder")

**Shadow Letter Rewards**:
- Add manipulation card (exhaust negatives)
- Upgrade exhaust abilities (free exhausts)
- Remove secret negative ("Dangerous Knowledge")

This creates clear progression paths through specialization.

**Delivery Outcomes**:
- On-time delivery: Standard reward
- Early delivery: Bonus card quality
- Late delivery: Reward + add "Disappointed" negative
- Failed delivery: Add "Broken Trust" negative, no reward

### NPC Personalities and Patience

**DEVOTED** (Family, Clergy):
- Base patience: 8-10
- Trust-focused cards more common
- Generate personal crisis letters

**MERCANTILE** (Merchants, Traders):
- Base patience: 5-7
- Commerce cards more common
- Generate trade and debt letters

**PROUD** (Nobles, Officials):
- Base patience: 3-5
- Status cards more common
- Generate reputation letters

**CUNNING** (Spies, Criminals):
- Base patience: 4-6
- Shadow cards more common
- Generate secret letters

**STEADFAST** (Workers, Guards):
- Base patience: 6-8
- Balanced card distribution
- Generate mixed letter types

## The Letter Delivery System - Complete Tactical Game

### Core Mechanics

**The Queue** (Your tactical battlefield):
- 8 ordered positions (1-8)
- 12 total weight capacity
- Only position 1 can be delivered
- Letters must progress through positions to reach delivery

**The Displacement Mechanic** (Core tactical decision):
Moving a letter up X positions requires burning X tokens with the NPC whose letter gets displaced. The token type matches the displaced letter's type.

Example: Elena's Trust letter in position 4, Lord's Status letter in position 1. Moving Elena to position 1 burns 3 Status tokens with Lord Blackwood.

This creates cascading consequences - every queue decision affects relationships.

### Physical Travel System

**Locations and Districts**:
The city contains multiple districts, each with specific locations:
- **Common Quarter**: Tavern, Market, Elena's Shop
- **Merchant Quarter**: Guild Hall, Warehouses, Marcus's Office
- **Noble District**: Lord's Manor, Gardens, Court
- **Castle Grounds**: Throne Room, Barracks, Archives
- **Shadow Alleys**: Hidden throughout, accessible only at night

**Routes Connect Locations**:
Each route between locations has properties:
- **Transport Type**: Walking (always available), Cart (faster), Carriage (premium)
- **Travel Time**: 10-60 minutes depending on distance and transport
- **Requirements**: Some routes require permits or relationships
- **Cost**: Premium transports cost coins

**Route Examples**:
- Tavern → Market: Walking (30 min, free), Cart (15 min, 2 coins)
- Market → Noble District: Walking (45 min, free), Carriage (15 min, 5 coins, requires Status 3+)
- Noble District → Castle: Restricted (requires Castle Permit letter)

### Letter Types and Properties

**Standard Delivery Letters**:
- **Sender**: The NPC who needs it delivered
- **Recipient**: The destination NPC
- **Type**: Trust/Commerce/Status/Shadow
- **Weight**: 1-3 (affects queue capacity)
- **Deadline**: Hours until expiration
- **Reward**: Coins and relationship tokens

**Special Letters** (System progression):
- **Access Permits**: Unlock restricted routes permanently
  - "Castle Permit": Enables Noble District → Castle route
  - "Merchant Pass": Reduces cost of cart travel
- **Introduction Letters**: Unlock new NPCs
  - "Letter to the Duchess": Adds high-tier NPC to Noble District
  - "Underground Contact": Reveals Shadow network NPC
- **Information Letters**: Reveal hidden mechanics or opportunities
- **Payment Letters**: Pure coin value, no relationship component

Special letters compete for queue space but provide permanent unlocks.

### Accepting and Delivering Letters

**Acceptance Process**:
1. Successfully achieve comfort threshold in conversation
2. Play letter card from NPC's deck
3. Letter enters queue at calculated position
4. Must manage weight capacity and position conflicts

**Delivery Process**:
1. Letter must be in position 1
2. Travel to recipient's location (costs time)
3. Start conversation with recipient (costs attention)
4. Delivery happens automatically if letter present
5. Choose reward type based on letter

**Failed Deliveries**:
- Expired letters damage relationship with sender
- Cannot be recovered once expired
- Sender becomes HOSTILE until resolved

### Standing Obligations (Mechanical Promises)

Obligations are binding promises that override normal queue rules:

**Types of Obligations**:
- **Trust Obligation**: "I'll handle this personally"
  - That NPC's letters always enter position 1
  - Cannot be displaced without breaking promise
- **Commerce Obligation**: "Your business comes first"
  - Letters enter position 2, displace anything except position 1
- **Status Obligation**: "I stake my reputation"
  - Must deliver on time or lose Status globally
- **Shadow Obligation**: "This stays between us"
  - Cannot refuse or reveal letter contents

**Creating Obligations**:
- Only during DESPERATE states through crisis cards
- Provides immediate +3 tokens
- Remains active until fulfilled or broken
- Each NPC can have ONE active obligation

**Breaking Obligations**:
- Costs -5 tokens with that NPC
- They become HOSTILE
- Adds "Betrayal" card to their deck
- Future letters enter at worst positions

### Time Management in Delivery

**How Time Advances**:
- Starting conversations: 15 minutes
- Traveling between adjacent locations: 10-30 minutes
- Traveling across districts: 30-60 minutes
- Delivering letters: 15 minutes
- All deadlines count down when time advances

**No Passive Time**: Time only advances through your actions

**Managing Deadlines**:
- Multiple letters create routing puzzles
- Must optimize travel paths
- Balance urgent vs efficient delivery
- Some letters chain (deliver A to unlock B)

### Strategic Depth of Letter Delivery

**Queue Tetris**:
- Managing 8 positions with different letters
- Weight capacity forces hard choices
- Displacement costs create relationship trade-offs
- Obligations override normal positioning

**Route Optimization**:
- Batch deliveries to same district
- Unlock shortcuts through special letters
- Balance cost vs speed for transport
- Plan around NPC availability windows

**Deadline Juggling**:
- Urgent letters force suboptimal routing
- Late delivery damages relationships
- Failed delivery cascades into future problems
- Must sometimes sacrifice letters strategically

### How Letter Delivery Feeds Back

**Into Conversations**:
- Successful delivery adds cards to NPC decks
- Failed delivery adds negative cards
- On-time delivery improves starting patience
- Late delivery reduces patience

**Into Future Letters**:
- Good reputation means better queue positions
- Bad reputation means letters enter at position 2
- Special letters unlock new NPCs and routes
- Chains create narrative progressions

This tactical layer provides moment-to-moment gameplay challenges while the strategic conversation layer builds long-term relationships.

### The Simplified Attention Economy

**What Attention Represents**:
Attention is your mental bandwidth for taking actions, not for making them easier. It's a daily budget that limits how much you can do, not how efficiently you do it.

**Daily Attention Budget**:
- Morning (6 AM - 12 PM): 5 attention
- Afternoon (12 PM - 6 PM): 5 attention  
- Evening (6 PM - 10 PM): 3 attention
- Night (10 PM - 12 AM): 2 attention
- Total: 15 attention per day

**Fixed Attention Costs**:
- Start any conversation: 1 attention
- Observe location: 1 attention
- Special actions in conversation: 2-3 attention
- Travel: No attention cost (costs time instead)

**No Relationship Discounts**:
Unlike our original Splendor-inspired design, relationships do NOT reduce attention costs. Instead, relationships affect:
- Starting patience in conversations (more patience with trusted NPCs)
- Available cards in conversation deck
- Letter tier and rewards
- Queue entry position

**Attention Refresh**:
- Quick drink: 1 coin = +1 attention  
- Full meal: 3 coins = +2 attention
- Maximum once per time block
- Can exceed normal maximum (e.g., 7/5 attention possible)

### Letter Properties

Each letter is a complete obligation containing:

**Core Properties**:
- **Sender**: The NPC offering the letter (determines emotional state)
- **Recipient**: Destination NPC (determines route)
- **Type**: Trust/Commerce/Status/Shadow (determines which relationship builds)
- **Deadline**: Hours/days until expiration (creates time pressure)
- **Weight**: 1-3 slots (affects queue capacity)
- **Stakes**: What the letter represents narratively

**Letter Complexity by Weight**:
- Weight 1: Simple messages, easy to manage
- Weight 2: Standard deliveries, moderate importance
- Weight 3: Critical packages, major storylines

**Deadline Pressure**:
- Personal Safety letters: 3-6 hours (urgent)
- Reputation letters: 8-12 hours (moderate)
- Commerce letters: 12-24 hours (flexible)
- Shadow letters: Variable (hidden until accepted)

### Letter Generation: Achievement Unlocks

**How Letters Become Available**:
Letters aren't random daily spawns but achievement unlocks. Each NPC has potential letters that become available when you reach comfort thresholds in conversation:

- **First threshold** (Comfort ≥ Patience/2): Maintain relationship
- **Second threshold** (Comfort ≥ Patience): Letter becomes available
- **Third threshold** (Comfort ≥ Patience × 1.5): Perfect conversation bonus

**Letter Cards in Deck**:
When you achieve the letter threshold, letter cards are added to the NPC's deck:
- "Ask for Trust Letter" (if Trust highest relationship)
- "Request Commerce Letter" (if Commerce highest)
- "Request Status Letter" (if Status highest)
- "Share Secret Letter" (if Shadow highest)

These remain in deck until successfully played or crisis resolved.

**Playing Letter Cards**:
- Must meet requirements (relationship level + current comfort)
- Success generates letter in queue
- Failure means try again next conversation
- Letter type matches card type, tier matches relationship depth

**Letter Properties Determined by Context**:
- Type: Matches the card played
- Tier: Based on relationship depth (0-2: Tier 1, 3-5: Tier 2, 6+: Tier 3)
- Deadline: Based on NPC personality and current state
- Weight: Based on tier (Tier 1: weight 1, Tier 2: weight 2, Tier 3: weight 3)
- Reward: Coins based on tier, tokens based on type

### NPC Emotional States and Patience Integration

**How Letter Deadlines Affect Conversations**:

NPCs with active letters in queue have modified starting patience:
- **DESPERATE** (letter <6 hours remaining): Starting patience -3
- **ANXIOUS** (letter 6-12 hours remaining): Starting patience -1  
- **CALCULATING** (letter >12 hours remaining): Normal patience
- **HOSTILE** (letter expired): Cannot converse until resolved
- **NEUTRAL** (no active letter): Normal patience

Example: Elena normally has patience 8. With a 4-hour deadline letter, she's DESPERATE, starting with only 5 patience. This makes conversations harder when NPCs are in crisis.

**Special Resolution**:
- Perfect conversation (comfort 15) can reset HOSTILE state
- Delivering expired letter late still repairs relationship partially
- Obligations bypass HOSTILE state (they must talk to you)

### Accepting and Refusing Letters

**The Integrated Card-Based Process**:

**Step 1: Discovery**
Play "How Are Things?" card (Difficulty 2, Cost 1 patience)
- Success: Reveals NPC has letter need, adds letter cards to hand
- Failure: No revelation this conversation

**Step 2: Request**
Play letter request card (e.g., "Ask for Trust Letter")
- Requirements: Appropriate relationship level + comfort threshold
- Success: Letter generated and enters queue
- Failure: No letter, may damage relationship

**Queue Entry Position**:
Letters enter queue based on relationships, with negative relationships taking precedence:

- **Any relationship -3 or worse**: Position 2 (they have leverage)
- **Active obligation**: Position 1 (absolute priority)
- **Otherwise, based on highest positive**:
  - Relationship 5+: Position 6 (respect your time)
  - Relationship 3-4: Position 5 (some deference)
  - Relationship 0-2: Position 4 (neutral)

Example: Trust 5 but Commerce -3 with Elena = Position 2 (debt creates leverage despite friendship)

**Refusing After Letter Generated**:
If you can't accept the letter (queue full, bad position):
- Immediate relationship damage based on their state
- DESPERATE: -3 tokens
- ANXIOUS: -1 token
- CALCULATING/NEUTRAL: No penalty
- Add "Broken Promise" card to their deck

### Queue Management - The Central Puzzle

**Queue Structure**:
- 8 slots (positions 1-8)
- 12 total weight capacity
- Only position 1 can be delivered
- Letters must be delivered in order

**Queue Entry Position** (Where letters start):
Base position: 6

Modified by relationships:
- Commerce -3 to -4: Position 2 (they have leverage)
- Commerce -5: Position 1 (maximum leverage)
- Status +3 to +4: Position 7 (they respect your time)
- Status +5: Position 8 (maximum deference)
- Active obligation: Specified position (usually 1-2)
- DESPERATE + Trust ≥3: Can request position 3

**Moving Letters** (The Token Burn):
- Moving a letter up X positions costs X tokens
- Token type matches the displaced letter's type
- Can't afford the tokens? Can't move the letter
- Creates cascading relationship consequences

Example: Moving Elena's Trust letter from position 4 to 1, displacing Lord's Status letter = burn 3 Status tokens with Lord

**Queue Overflow Crisis**:
When accepting would exceed 12 weight:

Option 1: "I'll make room"
- Choose a letter to abandon
- Full refusal penalties apply
- Letter is lost forever

Option 2: "I can't carry more"
- Refuse the new letter
- Standard refusal penalties

### Travel and Routing

**Travel Methods**:
- **Walking**: Always available, 30 minutes, FREE
- **Cart**: 2 coins, 15 minutes
- **Carriage**: 5 coins, 10 minutes
- **Special Routes**: Unlocked through letter delivery

**Time Cost**:
- Adjacent districts: Base travel time
- Across city: Double travel time
- Time advancement progresses ALL deadlines
- Creates routing optimization puzzle

### NPC Availability (No Arbitrary Gates)

**Conversation Requirements**:
NPCs will converse based on simple rules:
- **Common NPCs**: Always willing (no requirements)
- **Merchant NPCs**: Require any Commerce 1+ (with anyone)
- **Noble NPCs**: Require any Status 3+ (total across all NPCs)
- **Shadow NPCs**: Only appear at night or after specific events

**Location Access**: 
All districts physically accessible, but finding specific NPCs may require:
- **Observation** to reveal who's present
- **Time of day** (merchants morning, nobles afternoon, shadows night)
- **Personality schedules** (DEVOTED: morning/evening, MERCANTILE: all day except night)

This removes arbitrary gates while maintaining progression.

### Delivery Process and Rewards

**Delivery Steps**:
1. Travel to recipient location (costs time)
2. Find recipient NPC (may require observation)
3. Start conversation (1 attention base)
4. Deliver letter option appears automatically
5. Complete delivery (1 attention)

**Delivery Rewards**:
- **Immediate**: +1-3 tokens of letter type
- **Coins**: 2-20 based on tier and Commerce multiplier
- **Deck Improvement**: Choose card to add/upgrade/remove
- **Special**: Unlock new NPCs, routes, or storylines

**Tier System**:
- Tier 1 (Relationship 0-2): Simple delivery, 2-5 coins, +1 token
- Tier 2 (Relationship 3-5): Complex delivery, 5-10 coins, +2 tokens
- Tier 3 (Relationship 6-10): Storyline delivery, 10-20 coins, +3 tokens

### How Relationships Transform Gameplay

**Trust → TIME**:
- Each Trust point adds patience in conversations
- Trust 5: Letters get +6 hours to deadline
- Trust 3: Can request one deadline extension per letter
- Trust -3: All actions cost +1 attention
- Trust -5: NPC won't interact until repaired

**Commerce → COINS**:
- Multiplies ALL coin rewards from ALL deliveries
- Commerce +5: 200% coin rewards
- Commerce +3: 150% coin rewards
- Commerce -3: 50% coin rewards
- Commerce -5: No coin rewards
- Commerce debt affects queue entry position

**Status → ACCESS**:
- Total Status across all NPCs determines district access
- Individual Status affects letter quality
- Status +3: Access to privileged information
- Status +5: Letters enter at favorable positions
- Status -5: Only receive Tier 1 letters

**Shadow → LEVERAGE**:
- Shadow 3+: Can refuse letters without penalty
- Shadow 3+: Can displace letters without burning tokens
- Shadow 5+: Can force deadline modifications
- Shadow -3: Must accept "quiet" letters
- Shadow -5: Adds +1 weight to all your letters

### Strategic Depth of Letter Game

**Route Optimization**:
- Batch deliveries to same district
- Balance deadline urgency vs efficiency
- Premium transport for critical deadlines
- Walking for flexible timelines

**Queue Tetris**:
- Managing weight capacity
- Prioritizing by deadline
- Considering displacement costs
- Planning for obligation letters

**Relationship Investment**:
- Which NPCs to develop for letter sources
- Balancing token types for different benefits
- Managing negative relationships
- Building toward specific endings

**Time Management**:
- Morning: Fresh attention, new letters offered
- Afternoon: Continued activity, mounting pressure
- Evening: Fatigue, personal focus
- Night: Limited options, shadow dealings

### Letter-Driven Progression

**Early Game Letters**:
- Simple point-to-point deliveries
- Long deadlines
- Low weight
- Teaching basic mechanics

**Mid Game Letters**:
- Multi-step deliveries
- Tighter deadlines
- Relationship conflicts emerge
- Route optimization critical

**Late Game Letters**:
- Complex storyline chains
- Very tight deadlines
- Heavy weight packages
- Multiple simultaneous crises

### Crisis Management

**Cascading Failures**:
- Expired letter → NPC becomes HOSTILE
- HOSTILE NPC → Won't offer new letters
- No letters → Can't build relationship
- Low relationship → High attention costs
- High costs → Can't manage other letters
- Spiral requires intervention

**Recovery Options**:
- Perfect conversation to reset hostility
- Obligation to guarantee priority
- Coins to extend deadlines
- Shadow leverage to bypass penalties

### Obligations - Promises in Crisis

**Crisis Cards** (appear only when NPC is DESPERATE):
These represent making serious promises during someone's moment of need:

- "Promise Personal Help": Difficulty 3, Cost 3 patience
  - Success: +3 Trust, creates Trust obligation
  - Failure: -2 Trust, add "Broken Trust" to deck
  
- "Promise Business Priority": Difficulty 4, Cost 2 patience
  - Success: +3 Commerce, creates Commerce obligation
  - Failure: -2 Commerce, add "Failed Deal" to deck

**What Obligations Mean**:
- **Trust Obligation**: You've promised to help personally - their letters enter position 1
- **Commerce Obligation**: You've promised business priority - their letters enter position 2
- **Status Obligation**: You've staked your reputation - must deliver on time
- **Shadow Obligation**: You've agreed to keep secrets - cannot refuse their letters

**Breaking Promises**:
- Costs -5 tokens of the obligation type
- Replaces a positive card with "Betrayal" in their deck
- They become HOSTILE until resolved
- Represents real relationship damage from broken trust

Each NPC can have ONE active obligation - you can't make multiple conflicting promises.

### Coins and Tactical Flexibility

**The Role of Coins**:
Unlike tokens (permanent relationships), coins provide immediate tactical solutions.

**Earning Coins**:
- Base letter delivery: 2-20 coins by tier
- Commerce multiplier applies to ALL deliveries:
  - Average Commerce across all NPCs determines multiplier
  - +5 average: 200% coins
  - +3 average: 150% coins
  - 0 average: 100% coins
  - -3 average: 50% coins
  - -5 average: 0% coins

**Spending for Efficiency**:
- **Attention Refresh**: 
  - Quick drink: 1 coin = +1 attention
  - Full meal: 3 coins = +2 attention
  - Once per time block maximum
- **Premium Routes**:
  - Cart: 2 coins for 15 min travel
  - Carriage: 5 coins for 10 min travel
- **NPC Schedule Extension**:
  - 5 coins = 1 hour extension
  - Only with Commerce ≥3 or Mercantile NPCs
- **Queue Services** (requires Commerce ≥3):
  - Priority handling: 10 coins = jump to position 3
  - Express service: 15 coins = +6 hours to deadline

### Complete Attention Economy

**Daily Attention Budget**:
- Morning (6 AM - 12 PM): 5 attention
- Afternoon (12 PM - 6 PM): 5 attention
- Evening (6 PM - 10 PM): 3 attention
- Night (10 PM - 12 AM): 2 attention
- Total: 15 attention per day

**Base Attention Costs**:
- Start conversation: 1-10 attention (based on letter value)
- Accept letter: Cost reduced by relationship (minimum 1)
- Deliver letter: 1 attention base
- Observation: 1 attention
- Create obligation: 2-3 attention
- Travel: No attention cost (costs time instead)

**How Relationships Reduce Costs**:
- Base cost - Relationship level = Actual cost
- Example: 7 attention letter with Trust 5 = 2 attention cost
- Minimum cost always 1 (can't be free)
- This creates the Splendor-like engine where early investments compound

### Observation Actions

**What Observations Do**:
- Reveal which NPCs are present but not immediately visible
- Show temporary environmental states
- Discover route shortcuts
- Find coins or items

**Location-Specific Observations**:
- **Tavern**: Who's drinking in corners, mood of room
- **Market**: Which merchants are leaving soon, price fluctuations
- **Temple**: Who's seeking guidance, crisis states
- **Castle**: Political movements, noble availability
- **Alleys**: Shadow network activity, hidden passages

**Mechanical Effects**:
- Costs 1 attention
- Reveals 1-2 hidden opportunities
- Some NPCs only appear after observation
- Critical for finding time-sensitive opportunities

## Initial Game State and Tutorial Flow

## Opening Tutorial - Both Systems in Action

### Day 1, Morning, 6 AM

**Your Starting Position**:
- 5 attention available
- 10 coins
- One letter already in queue

**The Tactical Challenge** (Letter System):
- Position 1: Lord Blackwood's Status letter (6 hours deadline, weight 2)
- Must deliver or relationship suffers
- Located in Noble District (30 min travel)
- His ANXIOUS state means low patience for conversation

**The Strategic Opportunity** (Conversation System):
- **Elena** (Tavern): Trust 3, Commerce -1
  - High patience (11 total) enables deep conversation
  - Has urgent need (marriage letter)
  - Commerce debt means her letters get queue priority
- **Marcus** (Market): Commerce 1
  - Neutral patience (6)
  - Routine commerce opportunity
  - Safe relationship building

**The Integrated Dilemma**:
1. **Tactical Priority**: Deliver Lord's letter before deadline
2. **Strategic Opportunity**: Elena's high patience enables letter unlock
3. **Queue Conflict**: Accepting Elena's letter creates displacement problem
4. **Resource Management**: Limited attention for all actions

**What This Teaches**:
- Displacement burns tokens (moving Elena's letter up)
- Emotional states affect conversations (Lord's anxiety)
- Debt creates leverage (Elena's Commerce -1)
- Routes cost time (travel to Noble District)
- Every decision cascades through both systems

The opening forces players to immediately engage with both the tactical letter puzzle and strategic relationship building.

### How Players Build From Zero

**First Conversation** (No relationship):
- All options cost full attention
- Basic cards only in NPC deck
- Low patience (3-5)
- Goal: Reach comfort 10 to unlock first letter

**First Letter Delivery**:
- Provides first relationship token
- Unlocks first relationship card in deck
- Increases base patience by 1
- Opens pathway to Tier 2 letters

**Building Momentum**:
- Each delivery adds cards to NPC decks
- Improved decks enable harder conversations
- Harder conversations unlock better letters
- Better letters provide bigger improvements

## The Complete Integrated Game Loop

### Morning Routine (6 AM)

1. **State Check Phase**:
   - Each NPC's letter deadline updates
   - Emotional states shift (DESPERATE/ANXIOUS/etc.)
   - Letter cards added to decks based on needs
   - Negative cards from yesterday's failures activate

2. **Planning Phase**:
   - Review queue deadlines and positions
   - Check NPC emotional states (affects patience)
   - Identify urgent conversations (low patience NPCs)
   - Plan route for delivery efficiency

### Observation Actions - Strategic Reconnaissance

**What Observations Reveal** (1 attention per location):
- Which NPCs are currently present
- Their emotional states (DESPERATE/ANXIOUS/NEUTRAL/HOSTILE)
- Their current patience levels
- Whether they have letter needs available
- Any temporary conditions affecting the location

**Observation During Travel**:
While traveling, you can observe for opportunities:
- Shortcuts that reduce travel time
- NPCs in transit who might have urgent needs
- Environmental events affecting deadlines

**Observation During Conversation** (Peripheral Awareness):
While conversing, you remain aware of surroundings:
- Other NPCs arriving/leaving
- Changes in location atmosphere
- Time-sensitive opportunities
- This costs attention to investigate, creating tension

**Why Observation Matters**:
- Avoid wasting attention on hostile NPCs
- Identify crisis situations needing immediate help
- Plan efficient routes based on NPC availability
- Discover hidden opportunities
- Make informed decisions before committing resources

Observation transforms partial information into strategic advantage.

### Integrated Conversation Flow

1. **Start Conversation**: 1 attention cost (fixed, not discounted)
2. **Calculate Starting Patience**:
   - Base from personality (3-10)
   - + Relationship bonus (Trust adds to patience)
   - - Emotional state penalty (DESPERATE -3, ANXIOUS -1)
   - - Negative cards in deck (-0.5 each)
3. **Draw 5 Cards**: Check requirements, show available
4. **Play Cards**:
   - Pay patience cost
   - Roll success based on (Patience - Difficulty + 5) × 12%
   - Build comfort toward thresholds
5. **Letter Interaction** (if letter cards drawn):
   - "How Are Things?" reveals need
   - Letter request card generates letter if successful
6. **Rewards**:
   - Comfort 10+: Letter becomes available
   - Comfort 15: Remove negative card
   - Various cards may grant relationship tokens

### Queue Management with Integrated Systems

**When Letter Generated Through Cards**:
- Check weight capacity (12 total)
- Calculate entry position based on relationships
- If queue full, must abandon existing letter or refuse
- Refusal penalties based on emotional state

**Token Burning Still Matters**:
- Moving letters up costs tokens with displaced NPC
- Token type matches displaced letter type
- Can't afford tokens = can't move letter
- Creates relationship consequences beyond conversations

### Delivery Process

1. **Travel**: Time advances, all deadlines progress
2. **Find Recipient**: May need observation (1 attention)
3. **Start Conversation**: 1 attention (their patience affected by incoming letter)
4. **Delivery Automatic**: Letter in position 1 creates delivery option
5. **Choose Deck Reward**:
   - Add card: New conversation option
   - Upgrade: Reduce card difficulties
   - Remove: Clean negative cards
6. **Gain Tokens**: Build specific relationship type

### How Systems Interconnect

**Conversation → Letters**:
- Build comfort through smart card play
- Reach threshold to unlock letter cards
- Successfully play letter card to generate
- Letter enters queue based on relationships

**Letters → Conversations**:
- Delivery improves NPC deck permanently
- Better deck = easier future conversations
- Failed delivery adds negative cards
- Deadline pressure reduces patience

**Emotional States Bridge Both**:
- Letter deadlines create emotional states
- Emotional states reduce conversation patience
- Low patience makes conversations harder
- Failed conversations prevent letter generation
- Vicious or virtuous cycles emerge

### Daily Resource Management

**Attention Allocation** (15 total):
- Each conversation: 1 attention
- Each observation: 1 attention
- No discounts from relationships
- Coins can refresh for flexibility

**Patience per Conversation** (varies):
- High with trusted NPCs
- Low with desperate NPCs
- Depletes as cards played
- Determines success probabilities

**Time Through Travel**:
- Walking: Free but 30 minutes
- Cart: 2 coins, 15 minutes
- Carriage: 5 coins, 10 minutes
- All deadlines advance with time

## Why The Integrated System Works

### Clear Separation of Mechanics

**Attention**: Limited daily actions (breadth)
**Patience**: Conversation depth per NPC (depth)
**Comfort**: Conversation success metric
**Cards**: Conversation options and progression
**Letters**: Time-pressured obligations
**Tokens**: Permanent relationship markers

Each resource has ONE clear purpose without overlap.

### Natural Difficulty Progression

**Early Game**:
- Low relationships = low starting patience
- Few cards in decks = limited options
- Must play safe, build slowly
- Focus on clearing negatives and building base

**Mid Game**:
- Some relationships established
- Decks have mix of cards
- Can attempt riskier plays
- Letter chains become possible

**Late Game**:
- High relationships = high patience
- Rich decks with powerful cards
- Complex letter storylines
- Time pressure from 30-day limit

### Emergent Complexity from Simple Rules

The same conversation (Elena) plays differently based on:
- Her emotional state (letter deadlines)
- Your relationship (starting patience)
- Deck composition (available cards)
- Current patience (success probabilities)
- Queue state (can you accept letters?)
- Time remaining (urgency)

All emerging from simple, consistent rules.

## How The Two Layers Interconnect

### Conversations Generate Letters
- Achieve comfort thresholds → Letter cards unlock
- Play letter cards successfully → Letters enter queue
- Letter type matches highest relationship
- Queue position based on relationship dynamics

### Letters Improve Conversations
- Successful delivery → Add powerful cards to deck
- Failed delivery → Add negative cards
- On-time delivery → Improve future patience
- Special letters → Unlock new NPCs

### Emotional States Bridge Both Systems
- Letter deadlines create emotional states
- Emotional states reduce conversation patience
- Creates natural integration without complex rules
- Desperate NPCs are harder to converse with

### Relationships Affect Both Layers

**In Conversations**:
- Trust: More starting patience
- Commerce: Lower patience costs
- Status: Easier success rolls
- Shadow: Bypass requirements

**In Letters**:
- Trust: Affects deadline flexibility
- Commerce: Multiplies coin rewards
- Status: Determines available NPCs
- Shadow: Enables queue manipulation

### Queue Position Dynamics
When letters enter queue, relationships determine position:
- Any negative relationship: Position 2 (leverage)
- Obligation active: Position 1 (promise)
- High positive (5+): Position 6 (deference)
- Medium positive (3-4): Position 5
- Low/neutral (0-2): Position 4

### Displacement Creates Consequences
Moving letters burns tokens with displaced NPCs:
- Creates relationship damage
- Affects future queue positions
- Changes emotional states
- Cascades through both systems

### Standing Obligations Override Rules
Promises made during crises:
- Bypass normal queue positions
- Cannot be displaced without breaking
- Create lasting commitments
- Bridge tactical and strategic decisions

## System Integration: Resolving the Two-Layer Design

### How We Unified Two Different Economies

**Original Conflict**: 
We developed letters using Splendor's discount mechanic (relationships reduce attention costs), then created conversations using Slay the Spire's deck-building (patience determines success). These were incompatible.

**Resolution**:
- Attention: Fixed costs for actions (no discounts)
- Patience: NPC's conversation energy (modified by relationships)
- Relationships: Affect patience, not attention
- Cards: Generate letters through gameplay, not direct offers

### Key Integration Points

**1. Emotional States Bridge Both Systems**:
- Letter deadlines determine emotional state
- Emotional state modifies starting patience
- This naturally makes desperate NPCs harder to converse with
- Mechanically elegant, narratively authentic

**2. Letter Generation Through Cards**:
- Letters aren't "offered" but generated by successful card plays
- Requires building comfort first (conversation success)
- Then playing letter card (relationship gate)
- Integrates perfectly with deck mechanics

**3. Relationships Have Distinct Roles**:
- In Conversations: Determine starting patience and available cards
- In Letters: Determine queue position and coin multipliers
- No overlap or confusion about what affects what

**4. Progression Requires Both Systems**:
- Can't get letters without successful conversations
- Can't improve conversations without letter deliveries
- Neither system alone is sufficient
- Creates necessary engagement with both layers

### Why This Integration Works

**Clear Resource Purposes**:
- Attention: How many actions per day (breadth)
- Patience: How deep each conversation goes (depth)
- Comfort: Success metric within conversations
- Tokens: Permanent relationship state
- Cards: Conversation possibilities
- Letters: Time-pressured objectives

**No Redundancy**: Each resource does ONE thing

**Natural Feedback Loops**:
- Good conversations → Letters unlocked
- Letters delivered → Decks improved
- Better decks → Easier conversations
- Easier conversations → Better letters

**Emergent Difficulty**:
- Not artificial scaling but natural consequences
- Desperate NPCs are harder (less patience)
- Polluted decks create challenges
- Time pressure from deadlines
- All emerging from simple rules

### Specialization Strategies

**Trust Path** (The Intimate):
- Focus 3-4 NPCs deeply
- High patience, easy conversations
- Personal storylines dominate
- Ending: Deep personal connections

**Commerce Path** (The Merchant):
- Broad shallow relationships
- Maximize coin generation
- Economic storylines
- Ending: Wealthy trader

**Status Path** (The Noble):
- Build total Status score
- Unlock all districts
- Political storylines
- Ending: Social elite

**Shadow Path** (The Spider):
- Leverage and secrets
- Rule-breaking abilities
- Hidden storylines
- Ending: Information broker

**Balanced Path** (The Diplomat):
- Even distribution
- All storylines accessible
- Most challenging
- Ending: Community leader

### Why 30 Days Matter

The time limit creates crucial tensions:
- Can't maximize all relationships
- Must choose which storylines to pursue
- Failed deliveries have lasting impact
- No perfect optimization possible
- Different paths each playthrough

### Cascading Consequences

Every action ripples through both systems:

**Example Chain**:
1. Accept Elena's urgent letter → Queue position 3
2. Must displace Marcus's letter → Burn 2 Commerce tokens
3. Marcus relationship drops → His letters enter higher
4. Commerce multiplier drops → Less coins from all deliveries
5. Can't afford premium transport → More travel time
6. Other deadlines at risk → Cascade continues

This creates emergent narrative from mechanical state.

## Game Progression Arc

### Early Game (Days 1-10)
- **Conversations**: Basic decks, low patience, simple cards
- **Letters**: Tier 1 only, simple deliveries
- **Challenge**: Learning NPC personalities, establishing routes
- **Goal**: Build foundation relationships, clear initial negatives

### Mid Game (Days 11-20)
- **Conversations**: Some negatives cleared, relationship cards unlocked
- **Letters**: Tier 2 available, multi-step deliveries
- **Challenge**: Managing complex routes, multiple storylines
- **Goal**: Specialize in certain relationships, access better districts

### Late Game (Days 21-30)
- **Conversations**: Refined decks, high patience with developed NPCs
- **Letters**: Tier 3 storylines, interconnected narratives
- **Challenge**: Time pressure, competing storylines
- **Goal**: Complete chosen storylines, achieve desired ending

### Endings
### The 30-Day Story Arc

**Why 30 Days**:
The game represents one month in your character's life - a complete narrative arc. After 30 days:
- The game generates an ending based on your relationship patterns
- You see the consequences of your choices
- NPCs' lives conclude their current storylines
- Your reputation in the city is established

**Can Continue Playing**:
After the ending, you can continue in "endless mode" but:
- The scored story has concluded
- No new major storylines begin
- Relationships continue but without narrative weight
- Like playing Civilization after victory - possible but anticlimactic

**Different Endings**:
Your relationship configuration determines which of multiple endings you receive, encouraging replay with different strategies.

## Strategic Depth

### Conversation Strategy
- **Patience Management**: Use high patience early for difficult cards
- **Negative Clearing**: Address deck pollution while patience sufficient
- **Comfort Building**: Balance immediate comfort vs deck improvement
- **Risk Assessment**: High difficulty cards offer better rewards but risk failure

### Letter Strategy
- **Route Optimization**: Batch deliveries to same district
- **Deadline Management**: Priority vs efficiency trade-offs
- **Queue Tetris**: Weight and position management
- **Relationship Focus**: Which types to develop for which NPCs

### Deck Building Strategy
- **Card Selection**: Add power vs remove negatives
- **Deck Size**: Larger deck = more options but less consistency
- **Upgrade Path**: Reduce difficulty vs add new capabilities
- **Negative Management**: Clear through conversation vs letter delivery

### Resource Strategy
- **Attention Economy**: Conversations vs observations vs travel
- **Coin Usage**: Attention refresh vs transport vs emergency options
- **Time Management**: Efficient routing vs relationship maintenance
- **Token Building**: Specialization vs diversification

## Implementation Rules Summary

### Conversation Rules
1. Starting Patience = Base + Relationship Bonus - (Negatives × 0.5)
2. Success Chance = (Patience - Difficulty + 5) × 12%
3. Draw exactly 5 cards per round
4. Requirements must be met to show card
5. Comfort thresholds: 5/10/15
6. Max deck size: 20 cards

### Letter Rules
1. Queue: 8 slots, 12 weight capacity
2. Only position 1 can be delivered
3. Moving costs 1 token per position
4. Deadlines advance with time
5. Tier based on relationship depth
6. Type matches relationship being built

### Progression Rules
1. First letter delivery unlocks card rewards
2. Every 3 letters increases base patience by 1
3. Relationship 0-2: Tier 1, 3-5: Tier 2, 6-10: Tier 3
4. Game ends after 30 days
5. Cannot maximize all relationships
6. Ending determined by final configuration

## Critical Design Insights

### Why It Works
- **Two Games, One Loop**: Conversations and letters are complete games that enhance each other
- **Permanent Consequences**: No save-scumming, every choice shapes the future
- **Transparent Information**: Players see all probabilities and requirements
- **Natural Progression**: Difficulty emerges from deck state, not artificial scaling
- **Meaningful Failure**: Failed conversations create future challenges, not game over
- **Strategic Variety**: Multiple viable paths through different relationship configurations

### What to Avoid
- **Temporary Modifiers**: All changes should be permanent for clarity
- **Hidden Information**: Everything except die rolls should be visible
- **Forced Actions**: Players should always have agency
- **Abstract Resources**: Everything should map to intuitive concepts
- **Complexity Creep**: Resist adding edge cases or special rules

### Critical Design Insights for Integration

**What We Learned from Integration**:

1. **Different Resources for Different Purposes**: Attention and patience seem similar but serve completely different roles. Attention gates breadth (how many conversations), patience gates depth (how far each conversation goes).

2. **Emotional States as Bridge**: Having letter deadlines affect conversation patience creates natural integration without complex rules. Desperate people naturally have less patience.

3. **Cards Generate, Not Offer**: Letters emerging from successful card plays feels more organic than NPCs "offering" them directly. You earn the right to help through good conversation.

4. **Keep Discounts and Probabilities Separate**: We removed attention discounts (Splendor mechanic) because patience-based probabilities (Slay the Spire mechanic) already create progression. Don't double-dip on the same concept.

5. **Queue Position Still Matters**: Even though relationships primarily affect conversations, having Commerce debt affect queue position creates meaningful secondary effects without complexity.

**What to Avoid in Future Development**:

1. **Don't Mix Economies**: Attention discounts and patience probabilities were incompatible. Pick one progression system per resource.

2. **Don't Double-Stack States**: NPCs can't be both DESPERATE and have high patience. States must align across systems.

3. **Don't Hide Integration Points**: Make it clear how systems connect (letter cards in conversation deck) rather than mysterious background calculations.

4. **Don't Abandon Good Ideas**: Queue position based on Commerce debt was good even after removing attention discounts. Test if mechanics can survive system changes.

5. **Don't Fear Simplification**: Removing attention discounts made the game cleaner, not worse. Sometimes integration means subtraction.