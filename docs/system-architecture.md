# Wayfarer: A Systems Architecture Analysis

## System Overview
Wayfarer is a dual-layer interconnected system where two complete games feed into each other through multiple touchpoints. Like XCOM's strategy/tactical layers, each system is self-contained but generates inputs for the other.

## Primary Systems Architecture

### System 1: Relationship State Machine
Each NPC maintains a state composed of:
- **4 Token Values**: Trust, Commerce, Status, Shadow (range -5 to +10)
- **Conversation Deck**: 10-20 cards representing interaction history
- **Emotional State**: Calculated from letter deadlines (DESPERATE/ANXIOUS/NEUTRAL/HOSTILE)
- **Availability Schedule**: Time and location presence

**State Transitions**:
- Letter deadline < 6 hours → DESPERATE
- Letter deadline 6-12 hours → ANXIOUS
- Letter expired → HOSTILE
- No active letter → NEUTRAL

### System 2: Queue Management System
A constrained priority queue with:
- **8 Position Slots**: First-in-first-out delivery
- **12 Weight Units**: Total capacity constraint
- **Displacement Cost**: Moving up X positions costs X tokens
- **Entry Algorithm**: Position = f(relationships, obligations)

**Queue Entry Position Logic**:
```
if (obligation exists) position = 1
else if (any relationship < -3) position = 2  
else if (highest relationship >= 5) position = 6
else if (highest relationship >= 3) position = 5
else position = 4
```

### System 3: Geographic Network
A graph structure where:
- **Nodes**: Locations with internal spots
- **Edges**: Routes with transport properties
- **Weights**: Time cost per route
- **Gates**: Relationship requirements for route access

**Route Properties**:
- Transport Type (Walking/Cart/Carriage)
- Time Cost (10-60 minutes)
- Coin Cost (0-5)
- Unlock Requirements (permits, relationships)

## Interconnection Points

### Conversation → Letter Pipeline
1. Comfort threshold reached in conversation
2. Letter card added to active hand
3. Card played successfully
4. Letter object created with properties:
   - Type (matches dominant relationship)
   - Deadline (based on NPC state + type)
   - Weight (based on tier)
   - Reward (coins + tokens + cards)
5. Letter enters queue at calculated position

### Letter → Deck Modification Pipeline
1. Letter delivered successfully
2. Reward choice triggered:
   - Add card to sender's deck
   - Upgrade existing cards
   - Remove negative cards
3. Deck composition changes
4. Future conversation possibilities modified

### Emotional State Cascade
1. Letter deadline approaches
2. NPC emotional state shifts
3. Starting patience modified (-3 for desperate)
4. Conversation difficulty increases
5. Harder to generate new letters
6. Negative spiral potential

### Token Economy Feedback Loops

**Trust Loop**:
- Trust tokens → Increased patience
- More patience → Longer conversations
- Longer conversations → More comfort building
- More comfort → Letter unlocking
- Letter delivery → More Trust tokens

**Commerce Loop**:
- Commerce tokens → Reduced patience costs
- Lower costs → More card plays
- More plays → Higher success rates
- Success → Commerce letters
- Delivery → Coins + Commerce tokens
- Coins → Service purchases → Efficiency

**Status Loop**:
- Status tokens → Reduced difficulties
- Easier success → Comfort building
- Comfort → Status letters
- Delivery → More Status
- Total Status → District access
- New districts → New NPCs → New opportunities

**Shadow Loop**:
- Shadow tokens → Bypass requirements
- Bypassing → Access to powerful cards
- Powerful cards → Better outcomes
- Success → Shadow letters
- Delivery → Leverage abilities
- Leverage → Rule breaking → Unique paths

## System Constraints and Pressure Points

### Hard Constraints
- 15 attention per day (5/5/3/2 by time block)
- 8 queue slots maximum
- 12 weight capacity
- 30-day game length
- 1 obligation per NPC maximum

### Soft Constraints
- Patience per conversation (3-10 typical)
- Comfort needed for success (threshold based)
- Travel time between locations
- NPC availability windows
- Deck size limits (20 cards max)

### Pressure Generators
- Deadline countdown (creates urgency)
- Queue overflow (forces abandonment)
- Emotional state degradation (reduces patience)
- Relationship debt (affects queue position)
- Token burning for displacement (permanent loss)

## Feedback Loop Analysis

### Positive Feedback (Snowballing)
- Successful deliveries → Better cards → Easier conversations → More deliveries
- High relationships → Better positions → Easier routing → More time for relationships

### Negative Feedback (Stabilizing)
- More cards → Harder to draw specific cards
- High traffic routes → More time cost → Less efficiency
- Many relationships → Spread thin → Harder to maintain all

### Death Spirals (Failure Cascades)
- Failed delivery → Negative card added → Harder conversations → Fewer letters → Can't improve deck → Spiral continues

### Recovery Mechanisms
- Perfect conversations neutralize negatives
- Obligations guarantee priority
- Coins provide emergency options
- Shadow tokens enable rule-breaking

## Information Flow

### Hidden Information
- NPC deck contents (only revealed when drawn)
- Letter deadlines (until accepted)
- NPC locations (until observed)
- Available needs (until conversation)

### Perfect Information
- All costs and requirements
- Success probabilities
- Current relationships
- Queue state
- Time remaining

### Information Revelation
- Observation: 1 attention → Reveal NPC states
- Conversation start → See patience level
- Card draw → See available options
- Letter acceptance → See all properties

## System Optimization Patterns

### Batching Pattern
- Group deliveries to same district
- Combine observations with travel
- Stack relationship building in one area

### Pipeline Pattern
- Build comfort → Generate letter → Queue management → Delivery → Deck improvement → Return to start

### Leverage Pattern
- Use Shadow to bypass requirements
- Convert coins to time via transport
- Burn tokens for queue priority

### Investment Pattern
- Early attention to observation
- Build specific relationships deeply
- Curate decks through selective addition

## Emergence Properties

From these simple systems emerge:
- Unique relationship webs each playthrough
- Different optimal strategies based on starting conditions
- Narrative arcs from mechanical states
- Social dynamics from resource management
- Personality from deck composition
- Crisis from deadline pressure

The brilliance is that no single system dominates - success requires mastery of both layers and understanding of their interconnections. The conversation system provides strategic depth while the letter system provides tactical challenges, creating a complete game experience where every decision ripples through multiple systems.