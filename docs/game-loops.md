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
1. Connection States determine focus capacity (3-6) and card draws (1-3)
2. Focus persists across SPEAK actions, refreshes on LISTEN
3. Flow (-3 to +3) tracks success/failure, triggers state transitions at extremes
4. Rapport (-50 to +50) modifies all success rates linearly (+2% per point)
5. Atmosphere persists until changed or failure occurs
6. One card per SPEAK action creates authentic dialogue rhythm

**Request Card Mechanics**:
1. Player chooses conversation type
2. Appropriate request from request deck added to hand at start (unplayable)
3. Request becomes playable when LISTEN at sufficient focus capacity
4. Upon becoming playable, gains both Impulse and Opening properties
5. Must play immediately or conversation fails (exhaust effect)
6. Success: Accept obligation with fixed terms
7. Failure: Add burden card to relationship

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

**NPC Observation Decks** (NEW):
- Each NPC has an observation deck alongside conversation and request decks
- Location observations create cards that go into specific NPCs' observation decks
- At conversation start, all observation deck cards automatically drawn as persistent
- These cards don't count against hand size or normal draw limits
- Playing them costs one SPEAK action but 0 focus
- Cards consumed after use

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
- Desperate connection state only has crisis requests
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

### Location Familiarity System (NEW)

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

### Observation System (REFINED)

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
- Automatically available when conversing with relevant NPC
- Can unlock exchanges, change connection states, or provide unique effects

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

Work output scales with hunger:
- Formula: coins = base_amount - floor(hunger / 25)
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
- Managing flow to avoid unwanted transitions
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

## No Soft-Lock Architecture

Each loop provides escape valves:

**Conversation deadlocks**:
- Focus 1 cards always playable with minimum capacity (3)
- Can LISTEN to refresh focus
- Can leave and return later
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
- Deck composition (20 cards following template)
- Request deck (available conversation types with fixed terms)
- Observation deck (receives cards from location observations)
- Exchange deck (if mercantile)
- Token rewards for successful deliveries

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
- Follow persistence rules (75% persistent, 25% impulse)

## The Holistic Experience

The player experiences:

**Morning**: Check queue, investigate locations while quiet, plan route efficiently

**Travel**: Navigate using permits, build familiarity through investigation

**Conversations**: Use observation cards from NPC decks, build rapport, navigate flow

**Afternoon**: Investigate busy locations (less efficient), work for resources

**Evening**: Complete deliveries, gain tokens, see deck evolution results

Each session creates unique stories through mechanical interaction:
- Elena desperate about forced marriage (needs observation card to calm)
- Marcus calculating profit margins (Commerce tokens unlock caravan)  
- Guard Captain suspicious of bribes (Shadow tokens help initial trust)

These emerge from mechanical state, not scripted events.

## Core Innovation Summary

The three loops create a complete game where:

1. **Conversations** provide puzzle challenge through focus management, rapport building, and flow navigation, enhanced by observation cards in NPC decks
2. **Queue** provides time pressure through forced sequential completion and token-burning displacement
3. **Travel** provides exploration through location familiarity, investigation timing, and observation discoveries

Each loop uses different mechanics that operate on shared resources:
- Tokens provide starting rapport, creating easier conversations with investment
- Familiarity provides observations, creating NPC advantages through exploration
- Only gained through delivery success, forcing engagement with all systems
- Time pressure affects all three but manifests differently
- Attention enables all three but must be allocated strategically
- Rapport creates success momentum unique to conversations

The elegance is that no mechanic serves two purposes, yet resources flow through multiple systems creating strategic depth from simple rules. Starting rapport from tokens creates natural relationship progression. Location familiarity from investigation creates exploration progression. The game is the intersection, not the individual loops.