# Wayfarer: Three Core Game Loops - Interaction Layers

## System Integration Philosophy

The three core game loops answer fundamental design questions while maintaining strict mechanical separation. Each loop creates problems that only the other loops can solve, forcing engagement with all systems.

## Core Loop 1: Card-Based Conversations

### Design Questions Answered
- **What provides challenge?** Managing focus and rapport to reach request cards
- **Why grow stronger?** More tokens improve starting rapport linearly  
- **Why engage with NPCs?** Request cards provide income, access, and world progression

### Mechanical Framework

**The Conversation Puzzle**:
1. Connection States determine focus capacity (3-6) and card draws (3-4)
2. Focus persists across SPEAK actions, refreshes on LISTEN
3. Flow (0-24 internal, displays -3 to +3) tracks success/failure, transitions at thresholds
4. Rapport (-50 to +50) modifies all success rates linearly (+2% per point)
5. Atmosphere persists until changed or failure occurs
6. One card per SPEAK action creates authentic dialogue rhythm

**Connection States**:
- **Disconnected**: Flow 0-4, 3 focus capacity, 3 card draws
- **Guarded**: Flow 5-9, 4 focus capacity, 3 card draws
- **Neutral**: Flow 10-14, 5 focus capacity, 3 card draws
- **Receptive**: Flow 15-19, 5 focus capacity, 4 card draws
- **Trusting**: Flow 20-24, 6 focus capacity, 4 card draws

Flow below 0: Conversation ends immediately.

**NPC Persistent Decks**:
Each NPC maintains five persistent decks:
1. **Conversation Deck**: Standard 20 cards for dialogue
2. **Request Deck**: Goal cards (letters, promises) enabling special conversations
3. **Observation Deck**: Cards from location discoveries relevant to this NPC
4. **Burden Deck**: Cards from failed obligations
5. **Exchange Deck**: Commerce cards (mercantile NPCs only)

These decks determine available conversation types. During conversation, relevant cards form the draw pile.

**Three-Pile Conversation Flow**:
1. **Draw Pile**: Created at start from relevant NPC decks
2. **Active Pile**: Cards currently in hand
3. **Exhaust Pile**: Played and exhausted cards

When draw pile empties, shuffle exhaust pile to create new draw pile. This creates natural deck cycling.

**Request Card Mechanics**:
1. Player chooses conversation type
2. Request card from NPC's request deck added to draw pile
3. Request starts unplayable
4. Becomes playable when LISTEN at sufficient focus capacity
5. Upon becoming playable, gains both Impulse and Opening properties
6. Must play immediately or conversation fails
7. Success: Accept obligation with fixed terms
8. Failure: Add burden card to relationship

**Focus Management**:
- Capacity determined by connection state (3-6)
- Each SPEAK spends focus from pool
- Pool persists until depleted or refreshed
- LISTEN refreshes pool to current maximum
- Can SPEAK multiple turns with remaining focus
- Prepared atmosphere adds +1 to capacity

**Atmosphere Layers**:
- Persists across all actions until changed
- Standard atmospheres (~30% of normal cards)
- Observation-only atmospheres (unique effects)
- Failure clears to Neutral atmosphere
- Shapes entire conversation flow

**Observation Cards in NPC Decks**:
- Location observations create cards for specific NPCs' observation decks
- Mixed into draw pile at conversation start
- Playing them costs one SPEAK action but 0 focus
- Consumed after use
- Can set specific flow values or unlock exchanges

**Conversation Persistence**:
- Flow persists between conversations with same NPC
- Connection state persists based on flow value
- Patience with player persists until day transition
- Rapport resets to token value each conversation

Starting rapport (equal to connection tokens) determines initial success chance, making established relationships easier to navigate.

This creates multi-conversation arcs where relationships built through successful deliveries improve future starting conditions.

### Conversation Outputs
- **Promises**: Create obligations in queue (letters, meetings, escorts, etc.)
- **Tokens**: Gained only through successful letter delivery (+1 to +3)
- **Observations**: Cards added to specific NPCs' observation decks
- **Deck Evolution**: Successful completions modify NPC decks
- **Permits**: Special promises that enable routes
- **Burden Cards**: Failed requests damage relationships

## Core Loop 2: Obligation Queue Management

### Design Questions Answered
- **Why travel between locations?** Obligations scattered across the city
- **Why revisit locations?** Building relationships for better starting rapport
- **Why manage time?** Deadlines create pressure and force prioritization

### Queue Mechanics

**Strict Sequential Execution**:
- Position 1 MUST complete first
- No exceptions to this rule
- Maximum 10 obligations

**Queue Displacement Cost**:
To deliver out of order, burn tokens with EACH displaced NPC:
- Position 3 to 2: Burn 1 token with position 2 NPC
- Position 3 to 1: Burn 2 tokens with position 1 NPC AND 1 token with position 2 NPC
- Each burn adds 1 burden card to that NPC's relationship record

Token type burned matches NPC personality:
- Devoted: Trust tokens
- Mercantile: Commerce tokens
- Proud: Status tokens
- Cunning: Shadow tokens

### Request Card Terms (Fixed)

When playing a request card:
- **Success**: Accept obligation with predetermined terms
- **Failure**: No obligation, add burden card to relationship

Request cards no longer involve negotiation - terms are fixed based on the request type:
- Letter requests: Specific deadline, position, and payment
- Meeting requests: Fixed time and location
- Resolution requests: Clear existing burden cards

Personality influences which requests are available:
- Proud NPCs offer urgent, high-position requests
- Disconnected connection state only has crisis requests
- Mercantile NPCs focus on profitable exchanges

### Strategic Queue Patterns

**Obligation Chaining**: Accept multiple obligations in same location, complete efficiently

**Token Preservation**: Accept fixed queue positions to avoid burning relationships

**Emergency Displacement**: Burn tokens only for critical deadlines

**Queue Blocking**: Full queue (10 obligations) prevents new letter acquisition

## Core Loop 3: Location and Travel System

### Design Questions Answered
- **How does progression manifest?** Access to new routes and locations
- **How does world grow?** Location familiarity and discoveries unlock content
- **What creates exploration?** Information has mechanical value through unique effects

### Location Familiarity System

**Familiarity Mechanics**:
- Each location tracks Familiarity (0-3)
- Represents player's understanding of location patterns and secrets
- Only increased by Investigation action (not NPC interactions)
- Never decreases
- Determines observation rewards available

**Investigation Action**:
- Costs 1 attention
- Takes 10 minutes game time
- Familiarity gain scales with current spot properties:
  - Quiet spots: +2 familiarity
  - Busy spots: +1 familiarity
  - Other spots: +1 familiarity
- Can investigate same location multiple times
- Creates Istanbul-style timing decisions

### Travel Mechanics

**Route Requirements**:
- Every route requires an access permit
- No alternatives or "OR" conditions
- Multiple NPCs can provide same permit through different means

**Access Permit Sources**:
- Request cards with fixed terms
- Exchange cards from merchants (coin cost)
- Observation rewards from NPCs
- Location discoveries

**Permits as Physical Items**:
- Take satchel space (max 5 letters/permits)
- Do not expire (they're physical documents)
- No associated obligation
- Enable specific routes while held

### Observation System

**Building Discoveries**:
- Observations require minimum familiarity levels
- Each observation requires all prior observations at that location
- Cost 0 attention (just noticing what you understand)
- Create cards that go into specific NPCs' observation decks
- Different observations available at different familiarity levels:
  - First observation: Requires familiarity 1+
  - Second observation: Requires familiarity 2+ AND first observation done
  - Third observation: Requires familiarity 3+ AND second observation done

**Observation Effects**:
- Cards created go to predetermined NPCs' observation decks
- Represent location knowledge meaningful to specific NPCs
- Mixed into draw pile when conversing with relevant NPC
- Can set flow values, change connection states, or provide unique effects

### Travel Encounters

Use conversation mechanics with special decks:
- **Bandits**: Violence deck, combat resolution
- **Guards**: Inspection deck, authority check
- **Merchants**: Road trade deck, exchange opening

Success allows passage, failure costs resources.

## Resource Flow Between Loops

### Attention Economy Connections

**Daily Allocation**: 10 - (Hunger ÷ 25), minimum 2

Attention enables:
- **Conversations** (2): Access to letters and tokens
- **Investigations** (1): Build location familiarity
- **Observations** (0): Discover cards for NPC observation decks
- **Work** (2): Coins but time cost, scaled by hunger
- **Quick Exchange** (1): Simple commerce without full conversation

Work output scales with hunger:
- Formula: coins = 5 - floor(hunger / 25)
- Hungry workers are less productive
- Creates meaningful choice about when to eat

This forces prioritization between relationship building, location investment, and resource generation.

### Token Economy Integration

Tokens serve multiple purposes through different mechanics:
- **Starting Rapport**: Each token provides 1 starting rapport in conversations
- **Queue Displacement**: Burn for queue flexibility (permanent cost)
- **Scaling Effects**: Some cards scale rapport gain with token count
- **Exchange Gates**: Minimum tokens required for special exchanges

Tokens only gained through successful letter delivery:
- Standard delivery: +1 token with recipient (type based on letter)
- Excellent delivery: +2-3 tokens with recipient (type based on letter)
- Failed delivery: -2 tokens with sender

Each use is a different mechanic with one purpose. Higher tokens mean easier conversation starts through rapport.

### Time Pressure Cascades

Time advances through:
- **Travel**: Route-specific time costs
- **Investigation**: 10 minutes per action
- **Work**: 4-hour period advance
- **Rest**: Variable time skip
- **Natural progression**: During lengthy activities

Deadlines create cascading decisions:
- Tight deadline → Need displacement → Burn tokens → Lower future starting rapport
- Or: Rush to complete → Skip relationship building → Miss better letters

### Day Transition Mechanics

At 6 AM each day:
- **Patience Refresh**: All NPCs reset patience with player to base values
- **Flow Smoothing**: Each NPC's flow moves 1 point toward 12 (neutral center)
  - Flow 0-11: +1 flow (moves toward Neutral)
  - Flow 13-24: -1 flow (moves toward Neutral)
  - Flow 12: No change
- **Connection State Update**: Recalculated based on new flow value
- **Attention Refresh**: Reset to 10 - (Hunger ÷ 25)

This creates natural relationship normalization over time without full resets.

## Mechanical Interconnections

### How Loops Create Problems for Each Other

**Conversations create Queue pressure**:
- Every letter accepted adds obligation with fixed terms
- Multiple letters compete for position 1
- Focus management affects ability to reach request cards
- Low rapport makes request success uncertain

**Queue creates Travel pressure**:
- Obligations scattered across city
- Deadlines force inefficient routing
- Displacement damages relationships at distance
- Time-fixed meetings cannot be displaced

**Travel creates Conversation pressure**:
- Access permits require successful request card plays
- Travel time reduces deadline margins
- Encounters can damage resources
- Building familiarity costs attention that could fund conversations

### How Loops Solve Each Other's Problems

**Conversations solve Travel problems**:
- Request cards provide access permits
- Successful deliveries reward observation cards
- Built relationships (more tokens) make future permits easier
- Atmosphere effects can overcome obstacles

**Queue management solves Conversation problems**:
- Completing deliveries adds tokens for better starting rapport
- Payment provides resources for exchanges
- Successful delivery improves NPC decks
- Cleared obligations free satchel space

**Travel solves Queue problems**:
- Efficient routing chains obligations
- Location familiarity unlocks observations for NPC advantages
- Investigations at optimal times maximize efficiency
- Strategic timing aligns with NPC availability

## Strategic Layer Emergence

### Short-term Tactics
- Managing focus across multiple SPEAK actions
- Timing impulse cards before they're discarded
- Setting beneficial atmosphere before critical plays
- Building rapport early for success momentum
- Managing flow to reach state thresholds
- Whether to play request immediately when available
- Which spot to investigate from for best familiarity gain
- Whether to displace queue position

### Medium-term Planning
- Building toward states with sufficient focus capacity
- Managing queue to chain obligations
- Accumulating permits for route access
- Building location familiarity for observations
- Timing investigations for optimal spot properties
- Setting up atmosphere chains for maximum effect
- Planning multi-SPEAK sequences with focus
- Building rapport curves within conversations

### Long-term Strategy
- **Which NPCs to focus deliveries on for token accumulation**
  - Each NPC relationship provides starting rapport
  - Concentrated tokens create conversation advantages
  - Different letter types build different token types
- **Which locations to invest familiarity in**
  - Each location offers different observation rewards
  - Some observations unlock critical NPC options
  - Familiarity investment is permanent
- **Managing burden accumulation across relationships**
- **Building observation advantages for specific NPCs**
- **Developing permit collection for route flexibility**
- **Balancing token preservation vs queue efficiency**
- **Managing flow positions across multiple NPCs**
  - Flow persists between conversations
  - Daily smoothing moves flow toward neutral
  - Strategic timing of difficult conversations

## No Soft-Lock Architecture

Each loop provides escape valves:

**Conversation deadlocks**:
- Focus 1 cards always playable with minimum capacity (3)
- Can LISTEN to refresh focus
- Can leave and return later (flow persists)
- Patient atmosphere removes patience cost
- Observation cards in NPC decks provide unique solutions

**Queue deadlocks**:
- Can always displace (at token cost)
- Can drop letters (at relationship cost)
- Can wait for deadlines to pass (accepting failure)

**Travel deadlocks**:
- Multiple NPCs provide same permits
- Can earn coins through work for exchanges
- Observations provide alternate solutions
- Some routes open at different times

## Content Scalability

### Adding NPCs
New NPCs simply need:
- Personality type (determines patience and token burning)
- Five persistent decks (conversation, request, observation, burden, exchange)
- Token rewards for successful deliveries
- Which observations go to their observation deck
- Starting flow value (0-24)

### Adding Locations
New locations simply need:
- Spot properties (Crossroads, Commercial, Quiet, Busy, etc.)
- Observation rewards at each familiarity level
- Which NPC receives each observation card
- NPCs present at which times
- Routes and permit requirements

### Adding Cards
New cards must:
- Have exactly ONE effect (fixed or scaling, never both)
- Fit within focus system (0-6)
- Use difficulty tiers (Very Easy to Very Hard)
- Atmosphere changes on ~30% of cards
- Follow persistence rules (60% persistent, 25% impulse, 15% opening)

## The Holistic Experience

The player experiences:

**Morning**: Check queue, investigate locations while quiet for maximum familiarity gain, plan route efficiently

**Travel**: Navigate using permits, build familiarity through investigation

**Conversations**: Use observation cards from NPC decks, build rapport, navigate flow

**Afternoon**: Investigate busy locations (less efficient), work for resources

**Evening**: Complete deliveries, gain tokens, see deck evolution results

**Next Day**: Relationships have smoothed toward neutral, patience refreshed, new opportunities

Each session creates unique stories through mechanical interaction:
- Elena at flow 2 (Disconnected) needs observation card to reach flow 10 (Neutral)
- Marcus at flow 12 (Neutral) calculating profit margins (Commerce tokens unlock caravan)  
- Guard Captain at flow 8 (Guarded) suspicious of bribes (Shadow tokens help initial trust)

These emerge from mechanical state, not scripted events.

## Core Innovation Summary

The three loops create a complete game where:

1. **Conversations** provide puzzle challenge through focus management, rapport building, and flow navigation with persistent states
2. **Queue** provides time pressure through forced sequential completion and token-burning displacement
3. **Travel** provides exploration through location familiarity, investigation timing, and observation discoveries

Each loop uses different mechanics that operate on shared resources:
- Tokens provide starting rapport, creating easier conversations with investment
- Familiarity provides observations, creating NPC advantages through exploration
- Flow persists between conversations, creating relationship continuity
- Time pressure affects all three but manifests differently
- Attention enables all three but must be allocated strategically
- Rapport creates success momentum unique to conversations

The elegance is that no mechanic serves two purposes, yet resources flow through multiple systems creating strategic depth from simple rules. Starting rapport from tokens creates natural relationship progression. Location familiarity from investigation creates exploration progression. Persistent flow creates relationship history. The game is the intersection, not the individual loops.