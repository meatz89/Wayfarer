# Wayfarer: Three Core Game Loops - Interaction Layers

## System Integration Philosophy

The three core game loops answer fundamental design questions while maintaining strict mechanical separation. Each loop creates problems that only the other loops can solve, forcing engagement with all systems.

## Core Loop 1: Card-Based Conversations

### Design Questions Answered
- **What provides challenge?** Navigating emotional state web to access needed cards
- **Why grow stronger?** Better cards become drawable in better states  
- **Why engage with NPCs?** Goal cards provide income, access, and world progression

### Mechanical Framework

**The Conversation Puzzle**:
1. Emotional states filter what cards can be drawn (web structure, not linear)
2. Comfort (-3 to +3) triggers state transitions at extremes
3. Tokens (permanent) modify all success rates linearly (+5% per token)
4. Weight limits based on emotional state constrain plays
5. One card per turn creates authentic dialogue rhythm

**Goal Card Selection**:
1. Player chooses conversation type
2. Appropriate goal from goal deck shuffled into conversation deck copy
3. Goal card drawable based on current state compatibility
4. Once drawn, Goal persistence activates (3 turns to play)

**The Urgency Rule**: Once goal drawn, must play within 3 turns or conversation fails.

Tokens improve negotiation success (+5% per matching token type), determining the quality of terms (deadline, payment, queue position) but never gate access to goals.

This creates multi-conversation arcs where relationships improve negotiation outcomes over time.

### Conversation Outputs
- **Promises**: Create obligations in queue (letters, meetings, escorts, etc.)
- **Tokens**: Permanent relationship modifiers
- **Observations**: Cards for player's observation deck
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
- Each burn adds 1 burden card to that NPC's deck

Token type burned matches NPC personality:
- Devoted: Trust tokens
- Mercantile: Commerce tokens
- Proud: Status tokens
- Cunning: Shadow tokens

### Queue Position Negotiation

When playing a goal card (promise):
- **Success**: Your terms (usually lowest available position, better deadline/payment)
- **Failure**: NPC's terms (forced higher position, tighter deadline)

Promise type affects negotiation:
- Letter promises: Standard negotiation
- Meeting promises: Often force specific time (less flexible)
- Escort promises: Negotiate pickup time
- Crisis promises: Always attempt position 1

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
- **What creates exploration?** Information has mechanical value

### Travel Mechanics

**Route Requirements**:
- Every route requires an access permit
- No alternatives or "OR" conditions
- Multiple NPCs can provide same permit through different means

**Access Permit Sources**:
- Letter cards from high-token relationships
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
- Creates state change cards (weight 1, 85% success)
- Expire after 24-48 hours
- Maximum 20 cards in deck
- Represent temporal knowledge

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

**Daily Allocation**: 10 - (Hunger ÷ 25), minimum 2

Attention enables:
- **Conversations** (2): Access to letters and tokens
- **Observations** (1): Build player deck for state changes
- **Work** (2): Coins but time cost

This forces prioritization between relationship building, information gathering, and resource generation.

### Token Economy Integration

Tokens serve multiple purposes through different mechanics:
- **In Conversations**: +5% success rate per token (linear, all cards)
- **For Negotiations**: Better terms (no gates, just better outcomes)
- **For Displacement**: Burn for queue flexibility (permanent cost)
- **For Special Cards**: Some cards may check tokens for unique effects

Each use is a different mechanic with one purpose, but tokens flow through all.

### Time Pressure Cascades

Time advances through:
- **Travel**: Route-specific time costs
- **Work**: 4-hour period advance
- **Rest**: Variable time skip
- **Natural progression**: During lengthy activities

Deadlines create cascading decisions:
- Tight deadline → Need displacement → Burn tokens → Harder future conversations
- Or: Rush to complete → Skip relationship building → Miss better letters

## Mechanical Interconnections

### How Loops Create Problems for Each Other

**Conversations create Queue pressure**:
- Every letter accepted adds obligation
- Poor negotiation forces bad queue positions
- Multiple letters compete for position 1

**Queue creates Travel pressure**:
- Obligations scattered across city
- Deadlines force inefficient routing
- Displacement damages relationships at distance

**Travel creates Conversation pressure**:
- Access permits require successful negotiations
- Travel time reduces deadline margins
- Encounters can damage resources

### How Loops Solve Each Other's Problems

**Conversations solve Travel problems**:
- Goal cards provide access permits
- NPC rewards add observation cards to player deck
- Tokens unlock permit opportunities

**Queue management solves Conversation problems**:
- Completing deliveries improves NPC decks
- Payment provides resources for exchanges
- Successful delivery builds reputation

**Travel solves Queue problems**:
- Efficient routing chains obligations
- Exploration reveals shortcuts
- Observations provide state advantages

## Strategic Layer Emergence

### Short-term Tactics
- Which card to play this turn
- Whether to accept current letter terms
- Which observation to make now
- Whether to displace queue position
- Managing comfort to avoid unwanted transitions

### Medium-term Planning
- Building toward specific state transitions
- Managing queue to chain obligations
- Accumulating permits for route access
- Timing observations for conversation advantages
- Navigating state web to access needed cards

### Long-term Strategy
- Which NPCs to build relationships with
- Which districts to gain access to
- Which token types to prioritize
- How to shape NPC decks through deliveries
- Which states to master navigation toward

## No Soft-Lock Architecture

Each loop provides escape valves:

**Conversation deadlocks**:
- State cards at W1 ensure navigation
- Weight 1 cards always playable in most states
- Can leave and return later
- Patience cards can extend conversations

**Queue deadlocks**:
- Can always displace (at token cost)
- Can drop letters (at relationship cost)
- Can wait for deadlines to pass (accepting failure)

**Travel deadlocks**:
- Multiple NPCs provide same permits
- Can earn coins through work for exchanges
- Observations provide alternate solutions

## Content Scalability

### Adding NPCs
New NPCs simply need:
- Personality type (determines patience and token burning)
- Deck composition (which cards drawable in which states)
- Goal deck (available conversation types)
- Exchange deck (if mercantile)

### Adding Locations
New locations simply need:
- Spot properties (Crossroads, Commercial, etc.)
- Available observations per time period
- NPCs present at which times
- Routes and permit requirements

### Adding Cards
New cards must:
- Have exactly ONE effect from their type's pool
- List drawable emotional states clearly
- Fit within existing weight/success frameworks
- Not create secondary effects or thresholds

## The Holistic Experience

The player experiences:

**Morning**: Check queue, plan route to chain obligations efficiently

**Travel**: Navigate using permits, add observations to player deck

**Conversations**: Build relationships, manage comfort battery, navigate states

**Afternoon**: Work for resources or rush to meet deadlines

**Evening**: Complete deliveries, see deck evolution results

Each session creates unique stories through mechanical interaction:
- Elena desperate about forced marriage (Desperate state, Trust letter)
- Marcus calculating profit margins (Eager state, Commerce letter)  
- Guard Captain suspicious of bribes (Guarded state, high Shadow requirement)

These emerge from mechanical state, not scripted events.

## Core Innovation Summary

The three loops create a complete game where:

1. **Conversations** provide the puzzle challenge through emotional state navigation and comfort management
2. **Queue** provides the time pressure through forced sequential completion
3. **Travel** provides the exploration through observation-based route unlocking

Each loop uses different mechanics that operate on shared resources:
- Tokens flow through all three but serve different purposes in each
- Time pressure affects all three but manifests differently
- Attention enables all three but must be allocated strategically

The elegance is that no mechanic serves two purposes, yet resources flow through multiple systems creating strategic depth from simple rules. The game is the intersection, not the individual loops.