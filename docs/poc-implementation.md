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
10. Must reach 10 comfort OR play crisis card to generate letter

## World Construction Rules

### Location Creation

**Market Square** contains spots:
- Fountain (traits: Crossroads, Public)
- Merchant Row (traits: Commercial, Loud)
- North Alcove (traits: Discrete)
- Guard Post (traits: Authority, Tense)

**Copper Kettle Tavern** contains spots:
- Common Room Entrance (traits: Crossroads, Public)
- Corner Table (traits: Private, +1 comfort → +1 patience)
- The Bar (traits: Commercial, Social)
- Hearthside (traits: Warm, +2 comfort → +2 patience)

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
- Requires: Pass checkpoint

Rule: Routes only exist between spots with Crossroads trait.

## NPC Placement Rules

### NPC to Spot Assignment

**Marcus** permanently at:
- Location: Market Square
- Spot: Merchant Row
- Present Morning through Evening

**Elena** permanently at:
- Location: Copper Kettle Tavern
- Spot: Corner Table
- Always present during Afternoon period

**Town Crier** permanently at:
- Location: Market Square
- Spot: Fountain (sometimes North Alcove)
- Present Midday and Evening for proclamations

Rule: NPCs never move between spots or locations.

### NPC State Determination

Elena's state determined by obligation timer:
- 1h 13m remaining = under 2 hours = **Desperate** state

Marcus's state:
- No active obligations = **Neutral** state always
- Mercantile types rarely show strong emotions

Desperate state rules:
- Listen: Draw 2 cards + inject 1 crisis card
- Speak: Weight limit 3, crisis cards cost 0 weight
- Listen transitions to Hostile

Neutral state rules:
- Listen: Draw 2 cards, state unchanged
- Speak: Weight limit 3
- Standard baseline state

## Deck Construction Rules

### Three Deck System

Each NPC maintains:
1. **Conversation Deck** - Normal cards for standard conversations
2. **Exchange Deck** - Trade cards for quick exchanges
3. **Burden Deck** - Negative cards from failed obligations
4. **Crisis Deck** - Emergency cards that inject in crisis states

### Marcus's Exchange Deck (Mercantile Type)

**Exchange cards (4 drawn per exchange)**:

Commerce trades:
- Buy Travel Provisions: 3 coins → Hunger = 0 (Repeatable)
- Purchase Fine Silk: 15 coins → Fine Silk item (One-time)
- Buy Medicine: 5 coins → Health +20 (Repeatable)

Service trades:
- Help Inventory: 3 attention → 8 coins (advances time period)
- Quick Delivery: Accept package → Creates delivery obligation

Special trades:
- Information Trade: Guard observation → Hidden route unlock

### Elena's Base Conversation Deck (Devoted Type)

**Starting 20 cards**:

Trust cards (8):
- Small Talk (Weight 1, 70% success, +1 comfort)
- Listen Actively (Weight 1, 60% success, +2 comfort) 
- Share Memory (Weight 2, 50% success, +3 comfort)
- Promise to Help (Weight 2, 50% success, +4 comfort, fail: +1) ×2
- Deep Empathy (Weight 3, 40% success, +5 comfort)
- Solemn Vow (Weight 3, 30% success, +8 comfort)
- Calm Reassurance [STATE] (Weight 1, 60% success, Desperate→Tense)

Commerce cards (4):
- Mention Trade (Weight 1, 60% success, +1 comfort)
- Discuss Prices (Weight 2, 50% success, +2 comfort) ×2
- Negotiate Terms (Weight 2, 40% success, +3 comfort)

Status cards (3):
- Formal Greeting (Weight 1, 70% success, +1 comfort)
- Show Respect (Weight 2, 50% success, +2 comfort)
- Acknowledge Position (Weight 2, 50% success, +3 comfort)

Shadow cards (2):
- Hint at Secrets (Weight 2, 40% success, +3 comfort)
- Share Rumor (Weight 1, 50% success, +2 comfort)

Universal cards (3):
- Nod (Weight 0, 90% success, +0 comfort)
- Smile (Weight 0, 80% success, +1 comfort)
- Wait Patiently (Weight 1, 70% success, +1 comfort)

### Elena's Burden Deck

Starts empty. Burden cards added when:
- Player fails to meet Elena on time
- Player fails to deliver her letter on time
- Player makes promises they don't keep

Burden cards:
- "Broken Trust" (Weight 2, Burden type, -2 comfort when played to resolve)
- "Bitter Memory" (Weight 3, Burden type, blocks hand slot until resolved)
- "Lost Faith" (Weight 2, Burden type, all cards -10% success while in hand)

Burden cards:
- Persist in hand until played
- Playing them "resolves" them (removes from deck)
- Resolution has negative effects but clears the burden

### Elena's Crisis Deck

Contains one card when Desperate:
- "Desperate Promise" [CRISIS]
  - Weight: 5 (but 0 in Desperate state)
  - Success (40%): Generate letter immediately, Add 3 burden cards to Elena
  - Failure (60%): No letter, Add 1 burden card, Elena→Overwhelmed

## Observation System Rules

### Observation Creation at Spots

**Market Square Fountain** observations (Afternoon):
- "Guards blocking north road" 
  - Cost: 1 attention
  - Creates: "Guard Checkpoint Warning" card
  - Effect: Weight 1, Shadow type, 60% success, +3 comfort
  - One-shot, Opportunity type

Rule: Observations refresh each time period. Once taken, unavailable until next period.

### Observation Card Properties

Observation cards:
- Enter player's hand as ammunition
- Are Opportunity type (vanish if Listen chosen)
- One-shot (removed after playing)
- Generally beneficial (+2 to +5 comfort)
- Success rate follows standard calculation

## Conversation Flow Rules

### Starting Hand

**Turn 1 Hand** for Elena conversation:
1. "Guard Checkpoint Warning" (observation, unplayed)
2. "Promise to Help" (drawn from deck)
3. "Calm Reassurance" (drawn from deck)
4. "Small Talk" (drawn from deck)

Total weight available: 1 + 2 + 1 + 1 = 5 (limit 3 in Desperate)

### Patience Calculation

Elena's base patience (Devoted type): 12
Current state (Desperate): -4 patience
Spot modifier (Corner Table, Private): +1 patience
**Total: 9 patience for conversation**

Each turn costs 1 patience.

### Success Rate Calculation

Base success = 70%
- (Weight × 10%)
+ (Status tokens × 3%) [Player has 0]
= Final %

Examples:
- Weight 0: 70% success
- Weight 1: 60% success
- Weight 2: 50% success
- Weight 3: 40% success

### Multiple Paths to Goal

**Standard Path**: Reach 10+ comfort
- Turn 1: Play Observation (W1) + Promise (W2) = potential +7 comfort
- Turn 2: Play Calm Reassurance to change state
- Turn 3-5: Continue building comfort in Tense state

**Crisis Path**: Play crisis card
- Immediate letter generation if successful (40%)
- No comfort requirement
- Severe consequence: 3 burden cards added

**Strategic Considerations**:
- Observation card beneficial but not essential
- State change reduces crisis risk but limits options
- Weight management crucial for combinations
- Average 2+ comfort per turn needed for standard success

### Card Effect Types

Cards can have various mechanical effects:
- **Add Comfort**: Primary goal-reaching mechanism
- **Change State**: Alters conversation rules
- **Generate Letter**: Creates deliverable + obligation
- **Add Burden**: Makes future conversations harder
- **Create Obligation**: Adds deadline to queue
- **End Conversation**: Immediate termination
- **Resource Change**: Modify coins/health/hunger
- **Unlock Content**: Open routes/NPCs/locations

### Turn Structure

**Each turn costs 1 patience**

Player chooses:
- **Listen**: 
  - All Opportunities vanish (including observation)
  - Draw 2 cards + 1 crisis (Desperate state)
  - State transitions to Hostile (conversation ends next turn)
  
- **Speak**: 
  - Play cards up to weight limit (3 in Desperate)
  - Crisis cards cost 0 weight in Desperate
  - Each card resolves individually
  - Accumulate comfort from successes

### Crisis Decision Point

The crisis card presents a devil's bargain:
- **Success (40%)**: Instant letter, but 3 burden cards
- **Failure (60%)**: No letter, 1 burden card, state worsens

Players should only choose this when:
- Patience running low (1-2 turns left)
- Comfort far from goal (<5 accumulated)
- Deadline pressure extreme
- Willing to sacrifice future relationship

### Win Conditions

**Standard Victory**: Reach 10+ comfort
- Generates Important Letter (12h deadline, 10 coins)
- Creates delivery obligation
- No burden cards added

**Crisis Victory**: Successfully play Desperate Promise
- Generates Urgent Letter (2h deadline, 15 coins)
- Creates urgent delivery obligation
- Adds 3 burden cards to Elena's deck

**Failure**: Run out of patience without 10 comfort
- No letter generated
- Add 1 burden card to Elena
- Must return later to try again

## Card Upgrade System

Cards upgrade through successful use:
- Play successfully 3 times → Tier 2
- Play successfully 6 times → Tier 3

**Upgrade Options** (player chooses):
- Path A: Reduce difficulty (+20% success rate)
- Path B: Increase reward (+2 comfort)

Example:
- "Promise to Help" Tier 1: Weight 2, 50% success, +4 comfort
- "Promise to Help" Tier 2A: Weight 2, 70% success, +4 comfort
- "Promise to Help" Tier 2B: Weight 2, 50% success, +6 comfort

Weight NEVER changes through upgrades. Letter delivery adds NEW cards to decks.

## Exchange Conversation Flow

1. Player initiates Quick Exchange (0 attention)
2. Draw 4 cards from Marcus's exchange deck
3. Display all cards with cost→reward clearly shown
4. Player selects one card or declines
5. If accepted, immediate resource exchange
6. Conversation ends

No emotional states, no patience, no turns. Pure mechanical trade.

## Content Generation

All text is template-generated from mechanical properties:

**Location atmosphere**: [Time period] + [Location type] + [Crowd level]
"The afternoon tavern is nearly empty."

**NPC description**: [Emotional state] + [Personality type] + [Current obligation]
"Elena clutches letter, white knuckles, desperate."

**Card dialogue**: [Card type] + [Weight] + [NPC relationship]
Weight 2 Trust card = "I promise I'll help you, Elena."

**Exchange offers**: [Cost type] + [Reward type] + [NPC personality]
"3 coins → Hunger = 0" = "Buy fresh bread" (baker) or "Purchase provisions" (merchant)

**Burden resolution**: [Burden type] + [NPC history]
"Broken Trust" = "I need to apologize for failing you before."

No pre-written content. Every element generates from mechanical state.