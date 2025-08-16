# Wayfarer: Complete Game Design - Strategic and Tactical Layers

## Core Architecture: Two-Layer System

Like Lords of Waterdeep or XCOM, Wayfarer operates on two interconnected layers:
- **Strategic Layer**: Relationship building through conversation deck-building
- **Tactical Layer**: Letter delivery through queue management and route navigation

Neither layer functions independently - conversations unlock letters, letters improve conversations, creating an endless progression loop.

## The Tactical Layer: Letter Delivery System

### Core Loop
The letter delivery system is a complete tactical puzzle with its own mechanics:
1. **Accept** letters through conversations (adds to queue)
2. **Manage** queue through displacement and prioritization
3. **Navigate** routes between locations using transport
4. **Deliver** letters through recipient conversations
5. **Receive** rewards that feed back into strategic layer

### Queue Management

**Queue Structure**:
- 8 slots numbered 1-8
- 12 total weight capacity
- Only position 1 can be delivered
- Letters have weight 1-3 based on complexity

**Displacement Mechanics**:
To deliver a letter not in position 1, you must displace intervening letters:
- Moving letter from position 4 to position 1 requires 3 displacements
- Each displacement burns 1 token with the displaced letter's sender
- Token type matches the displaced letter type
- Cannot displace if you lack required tokens

Example: Elena's Trust letter in position 3, Lord's Status letter in position 1
- To deliver Elena's letter, must displace Lord's letter
- Burns 2 Status tokens with Lord Blackwood
- If you only have 1 Status token, cannot displace

**Special Cases**:
- Shadow 5+: Can displace without burning tokens
- Obligations: Override normal position rules
- Commerce debt: Forces letters to enter at position 2

### Travel and Route System

**Location Structure**:
```
District (e.g., Noble Quarter)
  └── Spots (e.g., Court, Garden, Gate)
      └── NPCs present at specific times
```

**Route Network**:
Routes connect specific spots across districts:
- Tavern → Market Square: Walking route, 30 minutes
- Market → Noble Gate: Cart route, 15 minutes, 2 coins
- Noble Gate → Castle: Restricted route, requires permit

**Transport Types**:
- **Walking**: Always available, 30 minutes base time
- **Cart**: Common routes, 15 minutes, 2 coins
- **Carriage**: Premium routes, 10 minutes, 5 coins
- **Horse**: Unlockable, 5 minutes, 10 coins
- **Boat**: Specific routes only (Docks → Castle)

**Route Unlocking**:
- Start with basic walking routes
- Cart routes unlock with Commerce relationships
- Carriage routes require Status levels
- Secret routes discovered through Shadow relationships
- Permit letters permanently unlock restricted routes

### Letter Types and Properties

**Standard Delivery Letters**:
- **Trust Letters**: Personal matters, tight deadlines (3-6 hours)
- **Commerce Letters**: Business deals, flexible deadlines (12-24 hours)
- **Status Letters**: Social obligations, specific time windows
- **Shadow Letters**: Secret deliveries, hidden properties

**Special Letters** (compete for queue space):
- **Access Permits**: Sacrifice to permanently unlock routes
- **Introduction Letters**: Unlock new NPCs when delivered
- **Information Letters**: Reveal game state when read
- **Payment Letters**: Pure coin value, no relationship reward

**Letter Properties**:
```
Sender: NPC who gave you the letter
Recipient: NPC who receives it
Type: Determines reward type and displacement cost
Tier: 1-3, complexity and reward scale
Weight: 1-3, queue capacity consumption
Deadline: Hours until expiration
Stakes: What the letter represents narratively
```

### Standing Obligations

**What Are Obligations**:
Mechanically enforced promises that modify queue behavior permanently until fulfilled.

**Types of Obligations**:
- **Priority Promise** (Trust): Their letters always enter position 1
- **Business Agreement** (Commerce): Their letters enter position 2, payment required
- **Reputation Stake** (Status): Must deliver on time or lose 5 Status
- **Secret Pact** (Shadow): Cannot refuse their letters

**Creating Obligations**:
Through crisis conversation cards when NPC is desperate:
- Costs significant patience and comfort
- Provides immediate +3 tokens
- Creates lasting mechanical change
- Each NPC can have ONE active obligation

**Breaking Obligations**:
- Costs -5 tokens of obligation type
- NPC becomes hostile
- Adds "Betrayal" card to their deck
- Future letters enter at worst positions

### Delivery Process

**Steps to Deliver**:
1. **Travel** to recipient location (costs time)
2. **Find** recipient at specific spot (may need observation)
3. **Converse** with recipient (costs 1 attention)
4. **Deliver** option appears automatically for position 1 letter
5. **Choose** reward type based on letter

**Delivery Rewards**:
- Coins: 2-20 based on tier and Commerce modifiers
- Tokens: +1-3 of letter type
- Deck improvement: Add/upgrade/remove card
- Special: Unlock routes, NPCs, or information

**Timing Matters**:
- Early delivery: Bonus rewards
- On-time delivery: Standard rewards
- Late delivery: Reduced rewards, negative cards added
- Failed delivery: Major relationship damage, hostile NPC

## The Strategic Layer: Relationship Building

### Conversation Deck-Building

**Core Mechanics**:
Each NPC has a deck of conversation cards representing your shared history:
- Start with 10 universal cards
- Add cards through letter delivery and story events
- Maximum 20 cards per NPC
- Cards cannot be traded or purchased

**Card Play**:
1. Draw 5 cards from NPC deck
2. Check requirements (relationships, comfort, location)
3. Play cards using patience (NPC's conversation energy)
4. Roll for success based on difficulty vs patience
5. Build comfort toward thresholds

**Success Formula**:
```
Success Chance = (Patience - Difficulty + 5) × 12%
```

### Relationship Types and Effects

Each NPC tracks four separate relationship values:

**Trust** (Personal bonds):
- Adds to starting patience in conversations
- Enables deadline extensions
- Unlocks personal letter storylines
- Trust 5+: Their letters get +3 hours grace

**Commerce** (Business ties):
- Reduces card patience costs by 1
- Multiplies coin rewards from their letters
- Creates leverage when negative (queue position)
- Commerce 5+: Their letters pay 200% coins

**Status** (Social standing):
- Reduces all card difficulties by 1
- Affects letter queue entry position
- Gates access to noble NPCs
- Status 5+: Letters enter at favorable positions

**Shadow** (Hidden knowledge):
- Bypasses card requirements at Shadow 3+
- Enables displacement without token burning
- Can refuse letters without penalty
- Shadow 5+: Complete rule-breaking ability

### Letter Generation Through Achievement

Letters aren't random but emerge from conversation success:

1. **Build Comfort**: Play cards to reach comfort thresholds
2. **Unlock Letter Cards**: Achieving thresholds adds letter cards to deck
3. **Play Letter Card**: Must meet requirements and comfort level
4. **Generate Letter**: Success creates letter with properties based on context
5. **Queue Entry**: Position determined by relationships

### NPC Emotional States

Letter deadlines create emotional states that affect conversations:
- **DESPERATE** (<6 hours): Starting patience -3
- **ANXIOUS** (6-12 hours): Starting patience -1
- **CALCULATING** (>12 hours): Normal patience
- **HOSTILE** (expired letter): Cannot converse
- **NEUTRAL** (no letter): Normal patience

## Resource Management

### Attention (Daily Action Budget)
- Morning: 5 points
- Afternoon: 5 points
- Evening: 3 points
- Night: 2 points

Fixed costs (no relationship discounts):
- Start conversation: 1 attention
- Observe location: 1 attention
- Special actions: 2-3 attention

### Time (Deadline Pressure)
Advances through actions:
- Conversation: 15 minutes
- Travel: Varies by route and transport
- Delivery: 15 minutes
- Major events: 30 minutes

All deadlines count down when time advances.

### Patience (NPC Conversation Energy)
Starting patience = Base + Trust - Emotional modifiers

Depletes as cards are played. When it reaches 0, conversation ends.

### Coins (Tactical Flexibility)
**Earning**: Letter delivery with Commerce multipliers
**Spending**: 
- Attention refresh (food/drink)
- Premium transport
- Deadline extensions
- NPC schedule extensions

### Tokens (Permanent Relationships)
Four types per NPC, range -5 to +10:
- Built through conversation and letter delivery
- Cannot be traded or spent (permanent history)
- Affect both strategic and tactical layers
- Negative values create leverage and penalties

## Complete Game Loop Integration

### Daily Cycle
**Morning (6 AM)**:
1. NPCs refresh emotional states based on letters
2. Observe locations to see opportunities
3. Plan route based on deadlines and queue
4. Begin conversations or travel

**Conversation Flow**:
1. Spend attention to start
2. Calculate patience (base + trust - emotional state)
3. Draw and play cards
4. Build comfort toward thresholds
5. Generate letters when successful

**Queue Decision**:
1. Check position based on relationships
2. Evaluate displacement costs
3. Consider deadline pressure
4. Accept or refuse based on strategy

**Route Planning**:
1. Check NPC locations and schedules
2. Calculate travel time vs deadlines
3. Choose transport based on urgency and coins
4. Navigate using unlocked routes

**Delivery Execution**:
1. Letter in position 1 enables delivery
2. Conversation with recipient
3. Choose reward type
4. Gain tokens and coins
5. Deck improvement feeds strategic layer

### Progression Arc

**Early Game (Days 1-10)**:
- Limited routes (mostly walking)
- Basic conversation cards
- Simple letters (Tier 1)
- Learning NPC patterns
- Building foundation relationships

**Mid Game (Days 11-20)**:
- Unlocked cart routes
- Improved decks
- Complex letters (Tier 2)
- Managing obligations
- Route optimization crucial

**Late Game (Days 21-30)**:
- Full route network
- Rich conversation options
- Storyline letters (Tier 3)
- Time pressure intense
- Racing toward ending

### Victory Conditions

After 30 days, the game generates an ending based on:
- Total relationships across all NPCs
- Depth of specific relationships
- Completed storyline chains
- Failed/succeeded obligations
- Final reputation configuration

Multiple endings encourage different strategic approaches.

## Why This Design Works

**Clear Layer Separation**:
- Strategic: Build relationships through conversations
- Tactical: Deliver letters through queue and route management
- Neither works alone, both essential

**Meaningful Choices**:
- Queue: Which letters to prioritize
- Displacement: Whether to burn tokens
- Routes: Time vs cost trade-offs
- Conversations: Risk vs reward on card plays
- Relationships: Which to develop for what benefits

**Natural Progression**:
- No arbitrary gates or thresholds
- Relationships unlock opportunities organically
- Difficulty emerges from complexity, not scaling
- Player skill improves through understanding

**Verisimilitude Throughout**:
- Cards represent actual shared experiences
- Desperate people have less patience
- Debt creates leverage despite friendship
- Travel takes real time with real costs
- Promises have lasting consequences

The result is a game where medieval courier work becomes a tactical puzzle of queue management and route optimization, while relationship building through conversations provides the strategic framework that shapes what letters appear and how difficult they are to deliver, creating endless emergent storytelling through mechanical state.