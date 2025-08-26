# Wayfarer POC Implementation Rules

## POC Scenario Flow

Player completes this exact sequence:
1. Starts at Market Square Fountain spot
2. Moves to Merchant Row spot (instant)
3. Quick Exchange with Marcus (buy provisions: 3 coins → Hunger = 0)
4. Returns to Fountain spot
5. Observes "Guards blocking north road" 
6. Receives observation card "Guard Checkpoint Warning"
7. Travels to Copper Kettle Tavern (15 minutes)
8. Moves to Corner Table spot (instant)
9. Initiates Standard Conversation with Elena
10. Must negotiate letter terms through card play

## World Construction Rules

### Location Creation

**Market Square** contains spots:
- Fountain (traits: Crossroads, Public)
- Merchant Row (traits: Commercial, Loud)
- North Alcove (traits: Discrete)
- Guard Post (traits: Authority, Tense)

**Copper Kettle Tavern** contains spots:
- Common Room Entrance (traits: Crossroads, Public)
- Corner Table (traits: Private, +1 patience)
- The Bar (traits: Commercial, Social)
- Hearthside (traits: Warm, +2 patience)

Rule: Each location must have exactly one spot with Crossroads trait for travel.

### Spot Movement

Within a location, movement between any spots is:
- Instant (no time cost)
- Free (no attention cost)
- Always available

Rule: Player can only interact with NPCs at their current spot.

### Route Creation

Routes connect Crossroads spots between locations:

**Market Square Fountain → Copper Kettle Entrance**
- Transport: Walk
- Time: 15 minutes
- Cost: 0 coins
- Always available

**Market Square Fountain → Noble District Gate**
- Transport: Walk
- Time: 25 minutes
- Cost: 0 coins
- Requires: Pass checkpoint or Access Permit

Rule: Routes only exist between spots with Crossroads trait.

## NPC Placement Rules

### NPC to Spot Assignment

**Marcus** permanently at:
- Location: Market Square
- Spot: Merchant Row
- Present: Morning through Evening
- Type: Mercantile (has exchange deck)

**Elena** permanently at:
- Location: Copper Kettle Tavern
- Spot: Corner Table
- Present: Always during Afternoon period
- Type: Devoted (no exchange deck)

**Town Crier** permanently at:
- Location: Market Square
- Spot: Fountain
- Present: Midday and Evening
- Type: Steadfast (no exchange deck)

Rule: NPCs never move between spots or locations.

### NPC State Determination

Elena's state determined by deadline:
- 1h 13m remaining = under 2 hours = **Desperate** state

Marcus's state:
- No active obligations = **Neutral** state always

Desperate state rules:
- Listen: Draw 2 from conversation deck, inject 1 from crisis deck
- Speak: Weight limit 3, crisis cards cost 0 weight
- Listen transitions to Hostile (ends conversation next turn)

Neutral state rules:
- Listen: Draw 2 from conversation deck
- Speak: Weight limit 3
- Standard baseline state

## Deck Construction Rules

### Four Deck System

Each NPC maintains appropriate decks based on type:

**Mercantile NPCs (Marcus)**: Conversation, Letter, Crisis, Exchange decks
**All Other NPCs**: Conversation, Letter, Crisis decks only

### Marcus's Exchange Deck

Exchange cards (draw 1 per exchange):

- "Buy Provisions" - 3 coins → Hunger = 0
- "Purchase Fine Cloth" - 15 coins → Fine Cloth item
- "Buy Medicine" - 5 coins → Health +20
- "Quick Delivery Job" - Accept → Delivery obligation (5 coins reward)

### Elena's Conversation Deck (20 cards)

**Depth 0-5 cards** (accessible at start):
- "I understand" - Comfort/Trust, Depth 3, Weight 1, Success: +3 comfort
- "Gentle nod" - Comfort/Trust, Depth 2, Weight 0, Success: +2 comfort
- "Listen actively" - Comfort/Trust, Depth 5, Weight 1, Success: +4 comfort, +1 Trust token
- "Suggest we wait" - State/Trust, Depth 4, Weight 1, Success: Desperate→Tense
- "Address past failure" - Burden/Trust, Depth 1, Weight 2, Success: Remove burden, -2 comfort

**Depth 6-10 cards**:
- "Promise to help" - Comfort/Trust, Depth 7, Weight 2, Success: +5 comfort, +1 Trust token
- "Share experience" - Comfort/Trust, Depth 8, Weight 2, Success: +6 comfort
- "Calm reassurance" - State/Trust, Depth 9, Weight 2, Success: Desperate→Neutral
- "Offer solution" - Comfort/Trust, Depth 10, Weight 2, Success: +7 comfort, +1 Trust token

**Depth 11-15 cards**:
- "Deep empathy" - Comfort/Trust, Depth 12, Weight 3, Success: +8 comfort, +2 Trust tokens
- "Solemn vow" - Comfort/Trust, Depth 14, Weight 3, Success: +10 comfort, +2 Trust tokens
- "Open connection" - State/Trust, Depth 13, Weight 2, Success: Any state→Open

**Depth 16-20 cards**:
- "Perfect understanding" - Comfort/Trust, Depth 17, Weight 3, Success: +12 comfort, +3 Trust tokens
- "Soul connection" - State/Trust, Depth 18, Weight 3, Success: Any state→Connected

### Elena's Letter Deck

- "Urgent Refusal to Lord Blackwood" - Letter/Trust, Depth 8
  - Eligible: Trust tokens ≥ 1, Desperate state
  - Success: 4-hour deadline, queue position 2, 10 coins payment
  - Failure: 1-hour deadline, forces queue position 1, 15 coins payment

- "Formal Refusal to Lord Blackwood" - Letter/Trust, Depth 12
  - Eligible: Trust tokens ≥ 3, Open/Connected state
  - Success: 8-hour deadline, queue position 3, 8 coins payment
  - Failure: 2-hour deadline, queue position 2, 10 coins payment

### Elena's Crisis Deck

When Desperate, contains:
- "Everything falls apart" - Crisis/Trust, Depth 0, Weight 5 (0 in Desperate)
  - Success (30%): Remove crisis, state→Tense
  - Failure (70%): Add 2 burden cards, conversation ends

## Observation System Rules

### Observation Creation at Spots

**Market Square Fountain** observations (Afternoon):
- "Guards blocking north road" 
  - Cost: 1 attention
  - Creates: "Guard Checkpoint Warning" card
  - Card properties: Comfort/Shadow, Depth 0, Weight 1
  - Success (60%): +4 comfort
  - Failure (40%): +1 comfort
  - Persistence: Opportunity (vanishes if Listen)

Rule: Observations refresh each time period. Once taken, unavailable until next period.

### Observation Decay

Observation cards decay in hand:
- Fresh (0-2 hours): Full effect
- Stale (2-6 hours): Half comfort value
- Expired (6+ hours): Must discard, unplayable

## Conversation Flow Rules

### Starting Values

**Every conversation begins at**:
- Comfort: 5
- This allows access to depth 0-5 cards only

### Token Effects

Each token adds +5% success to cards of that type:
- Elena has 1 Trust token with player
- All Trust cards get +5% success rate
- Linear progression, no thresholds

### Success Rate Calculation

Base success by weight:
- Weight 0: 80%
- Weight 1: 60%
- Weight 2: 50%
- Weight 3: 40%
- Weight 5: 30%

Modified by tokens:
- Weight 1 Trust card with 1 Trust token: 60% + 5% = 65%

### Patience Calculation

Elena's patience (Desperate conversation):
- Base (Devoted type): 12
- Player hunger 60: -3 (one per 20 hunger)
- Corner Table (Private): +1
- **Total: 10 patience/turns**

### Turn Structure

Each turn costs 1 patience.

**Listen Option**:
- Opportunity cards (observation) vanish immediately
- Draw cards based on state
- Check letter deck eligibility
- State may transition

**Speak Option**:
- Play cards up to weight limit
- Each card resolves individually
- Effects apply based on success/failure

### Letter Card Negotiation

When eligible letter appears:
1. Player can play it like any card
2. Success = favorable terms (longer deadline, flexible position)
3. Failure = unfavorable terms (tight deadline, forced position 1)
4. Either way, player gets letter and obligation

Example with "Urgent Refusal":
- Success: 4-hour deadline, position 2, 10 coins
- Failure: 1-hour deadline, forces position 1, 15 coins

### Depth Progression Example

**Turn 1** (Comfort 5):
- Can only play depth 0-5 cards
- Play "Guard Checkpoint" (W1) + "Listen actively" (W1) + "Gentle nod" (W0)
- Best case: +4 +4 +2 = Comfort becomes 15

**Turn 2** (Comfort 15):
- Now can access depth 0-15 cards
- Listen draws from larger card pool
- Might draw "Deep empathy" (depth 12)

**Turn 3+**:
- Continue building comfort to access depth 16+ cards
- Or accept letter at current depth

### Win Conditions

**Success**: Negotiate letter with manageable terms
- Good negotiation: 4+ hour deadline, position 2+
- Acceptable: 2-hour deadline, any position
- Poor but survivable: 1-hour deadline, position 1

**Failure**: Run out of patience without letter
- No letter obtained
- Add 1 burden card to Elena's deck
- Elena unavailable for 4 hours

## Exchange Conversation Flow

With Marcus (Mercantile NPC):
1. Player initiates Quick Exchange (0 attention)
2. Draw 1 card from Marcus's exchange deck
3. Display cost→reward clearly
4. Player accepts or declines
5. If accepted, immediate resource exchange
6. Conversation ends

No emotional states, no patience, no turns. Pure mechanical trade.

## Obligation Queue Rules

### Queue Management

- Position 1 MUST be completed first
- New obligations enter at lowest available position
- Crisis obligations attempt position 1

### Letter Negotiation for Position

When accepting letter:
- Standard letters respect queue, attempt lowest position
- Crisis/Proud personality letters attempt position 1
- Card play determines final position:
  - Success: Your negotiated position
  - Failure: NPC's demanded position

### Queue Displacement

To deliver out of order, burn tokens with displaced NPCs:
- Jump 1 position: -1 token with displaced NPC
- Jump 2 positions: -2 tokens with each displaced NPC
- Jump 3 positions: -3 tokens with each displaced NPC

## Content Generation

All text is template-generated from mechanical properties:

**Location atmosphere**: [Time period] + [Location type] + [Spot traits]
"The afternoon tavern's corner table offers privacy."

**NPC state description**: [Emotional state] + [Personality type] + [Current need]
"Elena, desperate, clutches her letter with trembling hands."

**Card dialogue**: Player speaking TO NPC based on [Card type] + [Effect]
- Comfort card: "I understand how important this is, Elena."
- State card: "Let's take a breath and think this through."
- Letter card: "I'll take your letter. For urgent delivery, I'll need 10 coins."

**Exchange offers**: [Cost] + [Reward] + [NPC type]
"3 coins → Hunger = 0" = "Buy fresh bread" (Marcus the merchant)

No pre-written content. Every element generates from mechanical state.