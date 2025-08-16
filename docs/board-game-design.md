# Wayfarer: A Board Game Designer's Analysis

## Core Concept
Wayfarer is a two-layer game combining strategic relationship building with tactical letter delivery, similar to how Lords of Waterdeep combines worker placement with quest completion. Each layer is a complete game that feeds into the other.

## The Two Game Layers

### Strategic Layer: Conversation Deck-Building
Think of this as Dominion meets Slay the Spire. Each NPC has their own deck of conversation cards representing your relationship history. Through conversations, you:
- Draw 5 cards from their deck
- Spend their patience (energy) to play cards
- Roll dice modified by card difficulty vs current patience
- Build comfort to reach thresholds
- Unlock letter opportunities at comfort thresholds

**Key Mechanics**:
- **Patience**: NPC's conversation energy (3-10 base)
- **Comfort**: Success points per conversation (0-15 scale)
- **Cards**: Each has difficulty, patience cost, and effect
- **Success Formula**: (Patience - Difficulty + 5) × 12% = success chance

### Tactical Layer: Letter Delivery Optimization
This is the physical game - like Ticket to Ride meets pandemic routing. You manage:

**The Queue** (Core Puzzle):
- 8 slots, only position 1 can be delivered
- 12 weight capacity total
- Moving letters up costs tokens with displaced NPC
- Example: Move Elena's letter from slot 4 to 1, displacing Marcus = burn 3 Commerce tokens with Marcus

**The Route Network**:
- Locations connected by routes (like train routes in Ticket to Ride)
- Each route has transport type: Walking (free, slow) or Cart (costs coins, faster)
- Some routes locked until unlocked via special letters or relationships
- Travel costs time, all deadlines tick down

**Special Letters** (Power-ups):
- Access Permits: Unlock new routes permanently
- Introduction Letters: Unlock new NPCs
- Payment Letters: Pure coin value
- These compete for queue space with regular deliveries

## Resources and Their Roles

**Daily Resources**:
- **Attention**: 15 points per day (5/5/3/2 by time block) - your action points
- **Time**: Continuous clock, advances through travel and actions
- **Patience**: Per-conversation energy, unique to each NPC

**Permanent Resources**:
- **Coins**: Buy services (transport, food, deadline extensions)
- **Relationship Tokens**: Four types per NPC (Trust/Commerce/Status/Shadow)
- **Cards**: Permanent additions to NPC decks through letter delivery

## Core Game Loops

### Morning Planning Phase
1. Check letter deadlines in queue
2. Observe locations for NPC availability (costs attention)
3. Plan optimal route through city
4. Allocate attention budget

### Conversation Loop
1. Spend 1 attention to start
2. Draw 5 cards, check requirements
3. Play cards using patience
4. Roll for success/failure
5. Build comfort toward thresholds
6. At threshold: Letter cards appear
7. Play letter card to generate letter

### Queue Management Loop
1. Letter enters at position based on relationships
2. Negative relationships = higher priority (leverage)
3. To deliver earlier: burn tokens to displace
4. Weight management (12 max)
5. Crisis: Overflow forces abandonment

### Travel and Delivery Loop
1. Plot route through connected locations
2. Choose transport (walk free, cart costs coins)
3. Time advances, deadlines countdown
4. Arrive at location, find recipient
5. Converse to deliver (automatic option)
6. Choose reward: add card, upgrade card, or remove negative

## Strategic Decisions

**Queue Tetris**: Which letters to accept given weight/position constraints?

**Token Burning**: Is moving this letter up worth damaging that relationship?

**Route Optimization**: Batch deliveries vs urgent individual runs?

**Transport Economics**: Walk and save coins, or pay for speed?

**Relationship Investment**: Which NPCs to develop for better cards/letters?

**Deck Curation**: Add powerful cards or remove negatives?

## Victory Conditions and Progression

**30-Day Campaign**: Complete story arc with multiple endings based on final relationships

**Scoring Vectors**:
- Trust Path: Deep personal connections (few NPCs, high tokens)
- Commerce Path: Economic empire (coin maximization)
- Status Path: Social climbing (unlock all districts)
- Shadow Path: Information broker (leverage and secrets)

**Progression Mechanics**:
- Early: Simple letters, basic cards, learn systems
- Mid: Complex letters, improved decks, route unlocking
- Late: Storyline chains, powerful cards, time pressure

## Why It Works

**Clear Decision Points**: Every choice has transparent trade-offs

**Multiple Solutions**: Same problem solvable through different resources

**Escalating Complexity**: Simple rules create complex situations

**Perfect Information**: All costs and probabilities visible

**Meaningful Failure**: Mistakes create recovery puzzles, not game over

**Emergent Narrative**: Mechanical states generate unique stories

This is board game design at its finest - two interlocking games where success in one enables progress in the other, creating endless replayability through systematic variation rather than randomness.