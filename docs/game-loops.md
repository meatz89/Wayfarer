# Wayfarer: Three Core Game Loops - Interaction Layers

## System Integration Philosophy

The three core game loops answer fundamental design questions while maintaining strict mechanical separation. Each loop creates problems that only the other loops can solve, forcing engagement with all systems.

## Core Loop 1: Card-Based Conversations

### Design Questions Answered
- **What provides challenge?** Managing weight pools and comfort battery to reach goal cards
- **Why grow stronger?** More tokens improve all success rates linearly  
- **Why engage with NPCs?** Goal cards provide income, access, and world progression

### Mechanical Framework

**The Conversation Puzzle**:
1. Emotional states determine weight capacity (3-6) and card draws (1-3)
2. Weight pool persists across SPEAK actions, refreshes on LISTEN
3. Comfort (-3 to +3) triggers state transitions at extremes
4. Tokens (permanent) modify all success rates linearly (+5% per token)
5. Atmosphere persists until changed or failure occurs
6. One card per SPEAK action creates authentic dialogue rhythm

**Goal Card Selection**:
1. Player chooses conversation type
2. Appropriate goal from goal deck shuffled into conversation deck copy
3. Goal card requires 5-6 weight to play (needs Open/Connected or Prepared atmosphere)
4. Goal cards have "Final Word" - if fleeting goal discarded, conversation fails

**Weight Pool Management**:
- Capacity determined by emotional state (3-6)
- Each SPEAK spends weight from pool
- Pool persists until depleted or refreshed
- LISTEN refreshes pool to current maximum
- Can SPEAK multiple turns with remaining weight
- Prepared atmosphere adds +1 to capacity

**Atmosphere Layers**:
- Persists across all actions until changed
- Standard atmospheres (~30% of normal cards)
- Observation-only atmospheres (unique effects)
- Failure clears to Neutral atmosphere
- Shapes entire conversation flow

Tokens improve negotiation success (+5% per matching token type), determining the quality of terms (deadline, payment, queue position). Letters are never gated by tokens - only negotiation quality improves.

This creates multi-conversation arcs where relationships built through successful deliveries improve future negotiations.

### Conversation Outputs
- **Promises**: Create obligations in queue (letters, meetings, escorts, etc.)
- **Tokens**: Gained only through successful letter delivery (+1 to +3)
- **Observations**: Cards for player's observation deck with unique effects
- **Deck Evolution**: Successful completions modify NPC decks
- **Permits**: Special promises that enable routes

## Core Loop 2: Obligation Queue Management

### Design Questions Answered
- **Why travel between locations?** Obligations scattered across the city
- **Why revisit locations?** Building relationships for better letters
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

### Queue Position Negotiation

When playing a goal card (promise):
- **Success**: Your terms (usually lowest available position, better deadline/payment)
- **Failure**: NPC's terms (forced higher position, tighter deadline)

Goal card difficulty affects negotiation:
- Letter goals: Very Hard (40% base + tokens)
- Meeting goals: Hard (50% base + tokens)
- Resolution goals: Hard (50% base + tokens)
- Crisis goals: Very Hard (40% base + tokens)

Personality modifiers:
- Proud NPCs always attempt position 1
- Desperate emotional state forces position 1 attempt
- Mercantile NPCs negotiate hardest on payment

### Strategic Queue Patterns

**Obligation Chaining**: Accept multiple obligations in same location, complete efficiently

**Token Preservation**: Accept poor queue positions to avoid burning relationships

**Emergency Displacement**: Burn tokens only for critical deadlines

**Queue Blocking**: Full queue (10 obligations) prevents new letter acquisition

## Core Loop 3: Location and Travel System

### Design Questions Answered
- **How does progression manifest?** Access to new routes and locations
- **How does world grow?** Player observation deck and discoveries unlock content
- **What creates exploration?** Information has mechanical value through unique effects

### Travel Mechanics

**Route Requirements**:
- Every route requires an access permit
- No alternatives or "OR" conditions
- Multiple NPCs can provide same permit through different means

**Access Permit Sources**:
- Goal cards from high-token relationships
- Exchange cards from merchants (coin cost)
- Observation rewards from NPCs
- Location discoveries

**Permits as Physical Items**:
- Take satchel space (max 5 letters/permits)
- Do not expire (they're physical documents)
- No associated obligation
- Enable specific routes while held

### Player Observation Deck System

**Building Your Deck** (1 attention at locations):
- Different observations available each time period
- Creates observation cards with unique effects
- Weight 1, persistent, 85% success (Very Easy)
- Expire after 24-48 hours
- Maximum 20 cards in deck
- Represent temporal knowledge

**Unique Observation Effects**:
- **Atmosphere Setters**: Informed, Exposed, Synchronized, Pressured
- **Cost Bypasses**: Next action free, next SPEAK costs 0 weight
- **Unique Manipulations**: Reset comfort to 0, refresh weight pool

**Observation Sources**:
- Location observations (spend attention)
- NPC rewards (completing promises)
- Travel discoveries (finding routes)

### Travel Encounters

Use conversation mechanics with special decks:
- **Bandits**: Violence deck, combat resolution
- **Guards**: Inspection deck, authority check
- **Merchants**: Road trade deck, exchange opportunity

Success allows passage, failure costs resources.

## Resource Flow Between Loops

### Attention Economy Connections

**Daily Allocation**: 10 - (Hunger Ã· 25), minimum 2

Attention enables:
- **Conversations** (2): Access to letters and tokens
- **Observations** (1): Build player deck for unique effects
- **Work** (2): Coins but time cost

This forces prioritization between relationship building, information gathering, and resource generation.

### Token Economy Integration

Tokens serve multiple purposes through different mechanics:
- **In Conversations**: +5% success rate per token ONLY on matching card types
  - Trust tokens only boost Trust-type cards
  - Commerce tokens only boost Commerce-type cards
  - Status tokens only boost Status-type cards
  - Shadow tokens only boost Shadow-type cards
- **For Negotiations**: Better terms when playing goal cards (matching types)
- **For Displacement**: Burn for queue flexibility (permanent cost)

Tokens only gained through successful letter delivery:
- Standard delivery: +1 token with recipient (type based on letter)
- Excellent delivery: +2-3 tokens with recipient (type based on letter)
- Failed delivery: -2 tokens with sender

Each use is a different mechanic with one purpose, but tokens must match card types for conversation bonuses.

### Time Pressure Cascades

Time advances through:
- **Travel**: Route-specific time costs
- **Work**: 4-hour period advance
- **Rest**: Variable time skip
- **Natural progression**: During lengthy activities

Deadlines create cascading decisions:
- Tight deadline â†' Need displacement â†' Burn tokens â†' Harder future conversations
- Or: Rush to complete â†' Skip relationship building â†' Miss better letters

## Mechanical Interconnections

### How Loops Create Problems for Each Other

**Conversations create Queue pressure**:
- Every letter accepted adds obligation
- Poor negotiation (low tokens) forces bad queue positions
- Multiple letters compete for position 1
- Weight management affects ability to reach goal cards

**Queue creates Travel pressure**:
- Obligations scattered across city
- Deadlines force inefficient routing
- Displacement damages relationships at distance
- Time-fixed meetings cannot be displaced

**Travel creates Conversation pressure**:
- Access permits require successful goal card plays
- Travel time reduces deadline margins
- Encounters can damage resources
- Observations cost attention that could fund conversations

### How Loops Solve Each Other's Problems

**Conversations solve Travel problems**:
- Goal cards provide access permits
- Successful deliveries reward observation cards
- Built relationships unlock permit opportunities
- Atmosphere effects can overcome obstacles

**Queue management solves Conversation problems**:
- Completing deliveries adds tokens for better success
- Payment provides resources for exchanges
- Successful delivery improves NPC decks
- Cleared obligations free satchel space

**Travel solves Queue problems**:
- Efficient routing chains obligations
- Exploration reveals shortcuts
- Observations provide conversation advantages through unique effects
- Strategic timing aligns with NPC availability

## Strategic Layer Emergence

### Short-term Tactics
- Managing weight pool across multiple SPEAK actions
- Timing fleeting cards before they're discarded
- Setting beneficial atmosphere before critical plays
- Whether to accept current goal card terms
- Which observation to make now
- Whether to displace queue position
- Managing comfort to avoid unwanted transitions

### Medium-term Planning
- Building toward states with sufficient weight capacity
- Managing queue to chain obligations
- Accumulating permits for route access
- Timing observations for conversation advantages
- Setting up atmosphere chains for maximum effect
- Planning multi-SPEAK sequences with weight pool

### Long-term Strategy
- **Which NPCs to focus deliveries on for specific token types**
  - Devoted NPCs build Trust tokens
  - Mercantile NPCs build Commerce tokens
  - Proud NPCs build Status tokens
  - Cunning NPCs build Shadow tokens
- **Token specialization based on district focus**
  - Noble District needs Status tokens
  - Market District needs Commerce tokens
  - Temple District needs Trust tokens
  - Shadow District needs Shadow tokens
- **How to shape NPC decks through deliveries**
- **Building observation deck for specific challenges**
- **Managing burden accumulation in relationship records**

## No Soft-Lock Architecture

Each loop provides escape valves:

**Conversation deadlocks**:
- Weight 1 cards always playable with minimum capacity (3)
- Can LISTEN to refresh weight pool
- Can leave and return later
- Patient atmosphere removes patience cost
- Observation cards provide unique solutions

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
- Goal deck (available conversation types)
- Exchange deck (if mercantile)
- Token scaling focus for comfort cards

### Adding Locations
New locations simply need:
- Spot properties (Crossroads, Commercial, etc.)
- Available observations per time period
- NPCs present at which times
- Routes and permit requirements

### Adding Cards
New cards must:
- Have exactly ONE effect (fixed or scaling, never both)
- Fit within weight system (0-6)
- Use difficulty tiers (Very Easy to Very Hard)
- Atmosphere changes on ~30% of cards
- Follow persistence rules (75% persistent, 25% fleeting)

## The Holistic Experience

The player experiences:

**Morning**: Check queue, plan route to chain obligations efficiently, refresh weight pools

**Travel**: Navigate using permits, add observations to player deck

**Conversations**: Manage weight pools, build atmosphere chains, navigate comfort battery

**Afternoon**: Work for resources or rush to meet deadlines

**Evening**: Complete deliveries, gain tokens, see deck evolution results

Each session creates unique stories through mechanical interaction:
- Elena desperate about forced marriage (3 weight capacity, scaled comfort with Trust)
- Marcus calculating profit margins (Commerce scaling, exchange opportunities)  
- Guard Captain suspicious of bribes (Shadow requirements, Volatile atmosphere)

These emerge from mechanical state, not scripted events.

## Core Innovation Summary

The three loops create a complete game where:

1. **Conversations** provide puzzle challenge through weight pool management, comfort battery navigation, atmosphere manipulation, and token-type matching (Trust tokens only help Trust cards, etc.)
2. **Queue** provides time pressure through forced sequential completion and token-burning displacement
3. **Travel** provides exploration through observation effects and permit-locked routes

Each loop uses different mechanics that operate on shared resources:
- Tokens flow through all three but must match card types for success bonuses
- Only gained through delivery success, creating specialization arcs
- Time pressure affects all three but manifests differently
- Attention enables all three but must be allocated strategically
- Weight pools create multi-turn tactical planning unique to conversations

The elegance is that no mechanic serves two purposes, yet resources flow through multiple systems creating strategic depth from simple rules. Token-type matching forces players to specialize their relationships rather than building generic power. The game is the intersection, not the individual loops.