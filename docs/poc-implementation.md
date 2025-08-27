# Wayfarer POC Implementation - Refined Mechanics

## ⚠️ IMPLEMENTATION STATUS - 2025-01-27

### HONEST ASSESSMENT: ~40-50% COMPLETE

**WORKING:**
- Basic conversations with emotional states
- Crisis card mechanics
- Exchange system for resource trading
- Attention persistence within time blocks
- Work button UI exists (needs backend verification)

**NOT WORKING:**
- Token progression (tokens calculated but never awarded - missing TokenManager.AddTokensToNPC call)
- Letter generation (should check Letter Deck for state-matching cards during LISTEN)
- Observation cards (created but never added to persistent hand)
- Queue displacement (logic exists but needs UI feedback)
- SPEAK should play ONE card (may allow multiple currently)

## POC Scenario Flow

Player must deliver Elena's urgent letter to Lord Blackwood before he leaves the city. The challenge: balancing resource management, time pressure, and relationship building with meaningful recovery options through work and rest.

**Starting Conditions**:
- Time: Tuesday 9:00 AM (Morning period)
- Location: Market Square Fountain
- Resources: 10 coins, 75 health, 80 hunger, 5 attention
- Elena's deadline: Available at tavern starting Afternoon (6 hours)
- Lord Blackwood: Leaving at sunset (5:00 PM, 8 hours)

## World Construction Rules

### Location Creation

**Market Square** contains spots:
- Fountain (traits: Crossroads, Public)
- Merchant Row (traits: Commercial, Loud)
- North Alcove (traits: Discrete)
- Guard Post (traits: Authority, Tense)

**Copper Kettle Tavern** contains spots:
- Common Room Entrance (traits: Crossroads, Public, Hospitality)
- Corner Table (traits: Private, +1 patience)
- The Bar (traits: Commercial, Social, Hospitality)
- Hearthside (traits: Warm, +2 patience)

Rule: Each location must have exactly one spot with Crossroads trait for travel.

## Work Action Mechanics

Available at Commercial spots only:
- **Market Square - Merchant Row**: Haul goods for merchants
- **Copper Kettle - The Bar**: Serve drinks to patrons

Mechanics:
- Cost: 2 attention
- Reward: 8 coins
- Time: Advances one full period (4 hours)
- Availability: Morning through Evening only

Strategic considerations:
- Each work action consumes significant time
- Morning work → Midday (Elena still unavailable)
- Midday work → Afternoon (Elena now available)
- Afternoon work → Evening (limited time remains)

With Lord Blackwood leaving at 5 PM, maximum 2 work actions possible while still completing delivery.# Wayfarer POC Implementation - Refined Mechanics

### Work and Rest Locations

**Work Actions** available at Commercial spots:
- Market Square: Merchant Row (haul goods)
- Copper Kettle: The Bar (serve drinks)
- Cost: 2 attention → 8 coins, advance one period

**Rest Exchanges** available at Hospitality spots:
- Copper Kettle: Common Room or Bar
- "Stay the Night": 5 coins → Full rest until morning
- "Quick Nap": 2 coins → +3 attention, advance 2 hours

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

### NPC Placement and Availability

**Marcus** permanently at:
- Location: Market Square
- Spot: Merchant Row
- **Available**: Morning through Evening only (shop closed at Night)
- Type: Mercantile (has exchange deck)

**Elena** permanently at:
- Location: Copper Kettle Tavern
- Spot: Corner Table
- **Available**: Afternoon through Evening only
- Type: Devoted (no exchange deck)
- **Special**: Deadline creates Desperate state when under 2 hours

**Town Crier** permanently at:
- Location: Market Square
- Spot: Fountain
- **Available**: Midday and Evening only (announces news twice daily)
- Type: Steadfast (no exchange deck)

**Bertram (Innkeeper)** permanently at:
- Location: Copper Kettle Tavern
- Spot: The Bar
- **Available**: Always (lives upstairs)
- Type: Mercantile (has exchange deck for rest options)

Rule: NPCs never move between spots or locations. Unavailable NPCs cannot be interacted with - requires waiting or returning later.

### NPC State Determination

Elena's state:
- **Always Desperate** when facing forced marriage (narrative situation)
- Deadline approaching intensifies desperation but doesn't create it
- She's desperate at 2 PM when you meet her, not just at <2 hours

Marcus's state:
- **Eager** during business hours (Morning-Afternoon)
- **Neutral** during slow hours (Evening)
- No active crisis = emotionally stable

Guard Captain's state:
- **Tense** during day shift (strict duty hours)
- **Neutral** during night shift (relaxed patrol)

Emotional states reflect narrative reality, not arbitrary timers.

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

### Bertram's Exchange Deck (Innkeeper)

Rest and hospitality services:

- "Stay the Night" - 5 coins → Rest until morning (Attention = 10, Hunger = 20, Health +20)
- "Quick Nap" - 2 coins → +3 attention, advance 2 hours
- "Hot Meal" - 2 coins → Hunger = 0
- "Rent Private Room" - 8 coins → Rest with full health restoration
- "Information Trade" - 3 coins → Learn about noble schedules

### Elena's Conversation Deck (20 cards)

**Depth 0-5 cards** (accessible at start):
- "I understand" - Comfort/Trust, Depth 3, Weight 1, Success: +3 comfort, +1 Trust token
- "Gentle nod" - Comfort/Trust, Depth 2, Weight 0, Success: +2 comfort
- "Share sympathy" - Comfort/Trust, Depth 4, Weight 1, Success: +3 comfort, +1 Trust token
- "Listen actively" - Comfort/Trust, Depth 5, Weight 1, Success: +4 comfort, +1 Trust token
- "Suggest we wait" - State/Trust, Depth 4, Weight 1, Success: Desperate→Tense
- "Address past failure" - Burden/Trust, Depth 1, Weight 2, Success: Remove burden, -2 comfort

**Depth 6-10 cards**:
- "Promise to help" - Comfort/Trust, Depth 7, Weight 2, Success: +5 comfort, +2 Trust tokens
- "Share experience" - Comfort/Trust, Depth 8, Weight 2, Success: +6 comfort, +1 Trust token
- "Calm reassurance" - State/Trust, Depth 9, Weight 2, Success: Desperate→Neutral
- "Offer solution" - Comfort/Trust, Depth 10, Weight 2, Success: +7 comfort, +2 Trust tokens

**Depth 11-15 cards**:
- "Deep empathy" - Comfort/Trust, Depth 12, Weight 3, Success: +8 comfort, +3 Trust tokens
- "Solemn vow" - Comfort/Trust, Depth 14, Weight 3, Success: +10 comfort, +3 Trust tokens
- "Open connection" - State/Trust, Depth 13, Weight 2, Success: Any state→Open

**Depth 16-20 cards**:
- "Perfect understanding" - Comfort/Trust, Depth 17, Weight 3, Success: +12 comfort, +4 Trust tokens
- "Soul connection" - State/Trust, Depth 18, Weight 3, Success: Any state→Connected

Note: Token generation available at all depth levels, increasing with depth.

### Elena's Letter Deck

- "Desperate Plea to Lord Blackwood" - Letter/Trust, Depth 5
  - Eligible: Trust tokens ≥ 1, states: Desperate, Tense, or Neutral
  - Success: 4-hour deadline, queue position 2, 10 coins payment
  - Failure: 1-hour deadline, forces queue position 1, 15 coins payment

- "Formal Refusal to Lord Blackwood" - Letter/Trust, Depth 10
  - Eligible: Trust tokens ≥ 2, states: Open, Connected, or Neutral
  - Success: 6-hour deadline, queue position 3, 12 coins payment
  - Failure: 2-hour deadline, queue position 2, 12 coins payment

- "Personal Letter to Lord Blackwood" - Letter/Trust, Depth 15
  - Eligible: Trust tokens ≥ 4, states: Connected or Open
  - Success: 8-hour deadline, flexible position, 20 coins payment
  - Failure: 4-hour deadline, queue position 2, 20 coins payment

Note: Letters available in multiple emotional states for flexibility.

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

Observation cards have Opportunity persistence:
- Fresh (0-2 hours): Full effect as written
- Stale (2-6 hours): Half comfort value, tokens still work
- Expired (6+ hours): Must discard, unplayable

**Critical Change**: Observation cards do NOT vanish when choosing Listen. They remain in hand and decay naturally over time. This makes observations worth their 1 attention cost as they provide lasting value across multiple conversation turns.

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

Elena's patience (when encountered):
- Base (Devoted type): 12
- Player hunger 80: -4 (one per 20 hunger) - unless resolved
- Corner Table (Private): +1
- **Total**: 9 patience if hungry, 13 if fed

This makes the hunger resolution decision critical - spending 3 coins on food gains 4 patience turns.

### Resource Decision Tree

**Morning Start (5 attention, 10 coins, 80 hunger)**:

Option 1 - Immediate food:
- Exchange with Marcus (3 coins → hunger=0)
- Now have 7 coins, full patience for conversations
- Risk: May not afford checkpoint bribe

Option 2 - Work first:
- Work at Merchant Row (2 attention → 8 coins)
- Now have 18 coins but only 3 attention
- Time advances to Midday

Option 3 - Endure hunger:
- Keep all resources
- Accept -4 patience penalty
- More coins and attention for other uses

Each choice cascades through the entire POC experience.

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

**Success**: Deliver Elena's letter to Lord Blackwood before 5 PM
- Perfect: Negotiated 4+ hour deadline, maintained relationships, profit earned
- Good: Delivered with decent terms, resources remaining
- Acceptable: Delivered by any means, even with poor terms

**Failure**: Lord Blackwood leaves without receiving letter
- Elena permanently hostile
- Lose all Trust tokens
- Add 3 burden cards to Elena's deck
- Locks out Elena content for 24 hours

## Strategic Depth

The POC demonstrates how simple mechanics create complex decisions:

**Resource Conversion Loop**:
- Time → Money (work actions)
- Money → Attention (rest at inn)
- Attention → Progress (conversations)
- Progress → Rewards (complete obligations)

**Scheduling Puzzle**:
- Elena unavailable mornings
- Marcus closed nights
- Guard shifts affect difficulty

**Deadline Pressure**:
- Can't infinitely grind due to 5 PM hard deadline
- Each work action costs 4 hours
- Rest to morning would miss deadline entirely

The player has agency to recover from mistakes but at meaningful cost in time, creating engaging resource management rather than punishing optimization.

## Exchange Conversation Flow

With Marcus (Mercantile NPC):
1. Player initiates Quick Exchange (0 attention)
2. Draw 1 card from Marcus's exchange deck
3. Display cost→reward clearly
4. Player accepts or declines
5. If accepted, immediate resource exchange
6. Conversation ends

No emotional states, no patience, no turns. Pure mechanical trade.

With Bertram (Innkeeper):
1. Same Quick Exchange mechanics
2. Rest options provide attention recovery
3. "Stay the Night" advances time to next morning
4. "Quick Nap" advances 2 hours but gives immediate attention

## Wait Action Mechanics

Available anywhere, costs 0 attention:
- Wait 30 minutes
- Wait 1 hour
- Wait 2 hours  
- Wait until next time period

Strategic uses:
- Elena unavailable until Afternoon? Wait 5 hours
- Guards change shift at 4 PM? Wait for night shift
- Need specific NPC? Wait for their availability window

Time advancement without resource cost, but opportunity cost of not using that time productively.

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