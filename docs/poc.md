# Wayfarer POC: 10-Minute Vertical Slice

## POC Objective
Prove both game loops function and interconnect within 10 minutes of play. Player must navigate an immediate crisis that requires both conversation (strategic) and letter delivery (tactical) to resolve.

## Success Condition
Deliver Elena's urgent letter to Lord Blackwood AND maintain positive relationship with Marcus (Commerce ≥0).

## Failure Conditions
- Elena's letter expires (8 minutes)
- Marcus becomes hostile (Commerce -3)
- Player runs out of attention before completing objective

## World Setup

### Locations (3 Total)

**1. Market Square** (Starting Location)
- Connected to: Tavern (walking 1 min), Noble District (walking 2 min, cart 1 min)
- NPCs: Marcus (merchant)
- Observable: "Cart service available for 2 coins"

**2. Tavern**
- Connected to: Market Square (walking 1 min), Noble District (locked - requires Noble Pass)
- NPCs: Elena (seamstress)
- Observable: "Notice board shows Noble District cart running hourly"

**3. Noble District**
- Connected to: Market Square (walking 2 min, cart 1 min)
- NPCs: Lord Blackwood (noble)
- Observable: "Lord preparing to leave in 5 minutes"

### Routes

**Walking Routes** (Always Available):
- Market ↔ Tavern: 1 minute
- Market ↔ Noble District: 2 minutes

**Cart Route** (Costs 2 coins):
- Market → Noble District: 1 minute (one way only)

**Locked Route**:
- Tavern → Noble District: Requires Noble Pass (not available in POC)

## NPCs and Starting State

### Elena (Tavern)
**Personality**: DEVOTED (base patience 8)
**Starting State**: DESPERATE (letter deadline in 8 minutes)
**Relationships**: Trust 3, Commerce 0, Status 0, Shadow 0
**Starting Patience**: 8 + 3 (Trust) - 3 (desperate) = 8
**Starting Deck**:
1. Small Talk (Diff 2, Cost 1p, +2 comfort)
2. Listen (Diff 3, Cost 1p, +3 comfort)
3. How Are Things? (Diff 2, Cost 1p, reveals need)
4. Share Trust (Diff 4, Cost 2p, +4 comfort) - from Trust 3
5. Quick Exit (Diff 0, Cost 0, ends conversation)
6. Awkward Silence (Diff 8, Cost 0, -2 comfort)
7. Ask for Trust Letter (Diff 3, Cost 2p, requires comfort 6)

**Active Need**: Personal letter to Lord Blackwood (marriage refusal)

### Marcus (Market Square)
**Personality**: MERCANTILE (base patience 6)
**Starting State**: CALCULATING (has commerce letter opportunity)
**Relationships**: Trust 0, Commerce 1, Status 0, Shadow 0
**Starting Patience**: 6
**Starting Deck**:
1. Small Talk (Diff 2, Cost 1p, +2 comfort)
2. Listen (Diff 3, Cost 1p, +3 comfort)
3. How Are Things? (Diff 2, Cost 1p, reveals need)
4. Trade Talk (Diff 3, Cost 1p, +2 comfort) - from Commerce 1
5. Quick Exit (Diff 0, Cost 0, ends conversation)
6. Bad Memory (Diff 6, Cost 1p, -1 comfort)
7. Request Commerce Letter (Diff 4, Cost 2p, requires comfort 5)

**Active Need**: Delivery to Noble District (trade documents)

### Lord Blackwood (Noble District)
**Personality**: PROUD (base patience 4)
**Starting State**: NEUTRAL (waiting, will leave at 5 minutes)
**Relationships**: Trust 0, Commerce 0, Status 1, Shadow 0
**Starting Patience**: 4
**Not conversable beyond delivery** (Status requirement not met for deep conversation)

## Starting Resources

**Player Resources**:
- Attention: 10 (special POC allocation)
- Coins: 3
- Time: 8 minutes until Elena's letter expires

**Starting Queue**:
- Position 1: Marcus's Commerce letter (weight 2, deadline 15 min, reward: 3 coins)
- Positions 2-8: Empty
- Weight: 2/12

## The Opening Dilemma

Elena desperately needs her refusal letter delivered to Lord Blackwood in 8 minutes. But Marcus's letter already occupies position 1 in your queue. 

**Key Decisions**:
1. Accept Elena's letter (enters position 4 due to Trust 3)?
2. Displace Marcus's letter (costs 3 Commerce tokens, drops you to Commerce -2)?
3. Deliver Marcus's letter first (uses precious time)?
4. Use coins for cart to save time?

## Optimal Path (Proof of Concept)

### Minute 0-2: Observe and Plan
1. **Observe Market** (1 attention): See Marcus is CALCULATING, has letter
2. **Converse with Marcus** (1 attention):
   - Draw: Small Talk, Listen, Bad Memory, Trade Talk, Quick Exit
   - Play Listen (73% success) → +3 comfort
   - Play Trade Talk (73% success) → +2 comfort
   - Comfort 5, patience 4 remaining
   - Exit (don't accept new letter)

### Minute 2-3: Travel to Tavern
3. **Walk to Tavern** (1 minute, 0 attention)
4. **Observe Tavern** (1 attention): See Elena DESPERATE, Lord leaving soon

### Minute 3-5: Critical Conversation with Elena
5. **Converse with Elena** (1 attention):
   - Patience: 8 (good for desperate NPC)
   - Draw: Small Talk, Share Trust, How Are Things, Ask Letter, Listen
   - Play How Are Things (85% success) → reveals need
   - Play Listen (73% success) → +3 comfort
   - Play Share Trust (60% success) → +4 comfort
   - Comfort 7, unlocks letter card
   - Play Ask for Trust Letter (73% success) → Letter generated!
   - Elena's letter enters at position 4 (Trust 3)

### Minute 5-6: Queue Management Crisis
6. **Critical Decision**: Elena's letter needs delivery NOW
   - Option A: Deliver Marcus first (not enough time)
   - Option B: Displace Marcus's letter
   - **Choose B**: Move Elena's letter to position 1
   - **Cost**: 3 Commerce tokens with Marcus (drops to Commerce -2)

### Minute 6-7: Rush Delivery
7. **Walk to Noble District** (2 minutes, 0 attention)
   - Could take cart (1 minute for 2 coins) but walking still makes deadline

### Minute 7-8: Deliver to Lord Blackwood
8. **Deliver Letter** (1 attention):
   - Lord Blackwood receives Elena's letter
   - Success! +3 Trust with Elena
   - Reward: Choose to add "Deep Trust" card to Elena's deck

### Minute 8-9: Repair with Marcus
9. **Return to Market** (1 attention for quick travel)
10. **Converse with Marcus** (1 attention):
    - He's now ANXIOUS (Commerce -2)
    - Use remaining attention to rebuild
    - Deliver his letter if possible, or promise obligation

## Test Acceptance Criteria

### Core Loop Tests

**Letter System (Tactical)**:
- [ ] Queue displays current letters with position, weight, deadline
- [ ] Accepting letter adds to correct position based on relationships
- [ ] Displacement burns tokens equal to positions moved
- [ ] Deadlines count down with time progression
- [ ] Delivery only possible from position 1
- [ ] Special letters (permits/introductions) have unique effects

**Conversation System (Strategic)**:
- [ ] Draw 5 cards from NPC deck
- [ ] Patience decreases as cards are played
- [ ] Success probability = (Patience - Difficulty + 5) × 12%
- [ ] Comfort accumulates based on card outcomes
- [ ] Letter cards appear when comfort threshold reached
- [ ] Relationships modify gameplay (Trust adds patience, etc.)

**Integration Points**:
- [ ] Emotional states from letter deadlines affect starting patience
- [ ] Letter delivery rewards improve conversation decks
- [ ] Queue position determined by relationships
- [ ] Obligations modify queue rules when created

### Time and Resource Management
- [ ] Actions consume appropriate attention
- [ ] Time advances through travel and conversations
- [ ] Coins enable tactical flexibility (cart, refreshes)
- [ ] Observation reveals strategic information

### Success Metrics
- [ ] Playable in 10 minutes or less
- [ ] Both success and failure paths possible
- [ ] Meaningful decisions with clear trade-offs
- [ ] Systems interconnect as designed
- [ ] Narrative emerges from mechanical state

## Failure States to Test

### Route 1: Ignore Elena
- Don't accept Elena's letter
- Elena becomes HOSTILE at 8 minutes
- Lose Trust relationship
- Test: Can player recover from hostile NPC?

### Route 2: Ignore Marcus  
- Immediately displace Marcus's letter
- Drop to Commerce -3 (hostile)
- Marcus won't converse
- Test: Do negative relationships properly gate interactions?

### Route 3: Try to Do Both
- Accept both letters
- Attempt both deliveries
- Run out of time/attention
- Test: Does resource scarcity create proper tension?

## Variations for Extended Testing

### Variation A: Add Noble Pass
- Marcus's letter rewards include Noble Pass
- Opens Tavern → Noble District route
- Tests special letter functionality

### Variation B: Add Introduction Letter
- Lord Blackwood has introduction to Guard Captain
- Opens fourth NPC and location
- Tests expansion mechanics

### Variation C: Add Standing Obligation
- Elena offers Trust obligation in desperation
- Her future letters enter position 1
- Tests promise system modification of queue rules

## Technical Requirements

### UI Elements Needed
- Queue visualization (8 slots, weight indicator)
- Patience/comfort meters during conversation
- Card hand display with probability percentages
- Relationship token displays (4 types per NPC)
- Timer/deadline countdown
- Route map showing connections and travel times

### State Tracking Required
- NPC emotional states
- Letter deadlines
- Queue positions and weights
- Conversation deck contents
- Relationship values
- Obligations active
- Time remaining
- Attention available

### Narrative Generation Triggers
- Conversation starts (describe NPC state)
- Card played (describe action)
- Letter accepted (describe urgency)
- Displacement occurs (describe social tension)
- Delivery complete (describe resolution)
- Relationship changes (describe impact)

## POC Success Criteria

The POC succeeds if:
1. **Both loops demonstrated**: Player must engage with conversations AND letter delivery
2. **Systems interconnect**: Conversation outcomes affect letters, deliveries affect conversations
3. **Meaningful decisions**: Multiple viable paths with different trade-offs
4. **Clear feedback**: Player understands why they succeeded or failed
5. **10-minute completion**: Full playthrough achievable in time limit
6. **Replayable**: Different strategies lead to different outcomes

## Core Insights to Validate

1. **Queue displacement creates tension**: Burning tokens for queue management is meaningful
2. **Route optimization matters**: Travel time forces strategic planning  
3. **Patience limits conversations**: Can't infinitely farm comfort
4. **Relationships have distinct effects**: Each type provides unique advantages
5. **Time pressure drives urgency**: Deadlines force difficult decisions
6. **Both layers necessary**: Can't succeed with just conversations or just deliveries

This POC proves Wayfarer's core thesis: managing social obligations through letter delivery while building relationships through conversations creates emergent narrative from mechanical state.