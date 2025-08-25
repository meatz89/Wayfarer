# Wayfarer POC Implementation Rules

## POC Scenario Flow

Player completes this sequence:
1. Starts at Market Square Fountain spot
2. Optional: Observe to gain conversation cards (1 attention each)
3. Optional: Exchange with Marcus (free, instant)
4. Travel to Copper Kettle Tavern (15 minutes time cost)
5. Move to Corner Table spot (instant within location)
6. Initiate Standard Conversation with Elena (2 attention)
7. Navigate emotional states to build 10 comfort
8. Generate letter through comfort or crisis path
9. Deliver to Lord Blackwood before deadline

---

## CORE RESOURCE SYSTEM

### Primary Resources

**Health (0-100)** - Affects tomorrow's attention
- 76-100: No penalty
- 51-75: -1 attention tomorrow
- 26-50: -2 attention tomorrow  
- 0-25: -3 attention tomorrow

**Hunger (0-100)** - Affects tomorrow's attention
- 0-25: No penalty
- 26-50: -1 attention tomorrow
- 51-75: -2 attention tomorrow
- 76-100: -3 attention tomorrow

**Attention Refresh Formula**:
```
Tomorrow's Attention = 10 - Health Penalty - Hunger Penalty
```

**Coins (0-999)** - Currency for exchanges and bribes

### Token System (Consumable Resources)

**Generation**: Successfully playing a card of that type grants +1 token

**Token Uses in Conversations** (ONE effect each):
- **Trust**: +1 card from Comfort deck when Listening
- **Commerce**: +1 weight limit when Speaking
- **Status**: +10% success rate (stackable)
- **Shadow**: See top card before drawing

**Token Uses in Exchanges**: 5 tokens of any type unlock special trades

---

## CONVERSATION SYSTEM

### Core Principle

**NPCs have thoughts (cards in decks). Players have observations (cards from world). Conversations emerge from managing emotional states to access the right thoughts.**

### NPC Deck Structure

Every NPC has THREE decks:

**1. Comfort Deck (15-20 cards)**
- Normal thoughts, pleasant interactions
- Mostly Comfort cards, few State cards
- Exhaustible during conversation (no reshuffle)

**2. Crisis Deck (5-10 cards)**
- Fears, problems, urgent needs
- Crisis and Burden cards
- Successfully resolved cards are REMOVED

**3. State Deck (5 cards)**
- Emotional regulation capacity
- ONLY State change cards
- Cycles - played cards return to bottom

### Player Cards

**Players have ONLY**:
- Observation cards gathered from world (0-5 typically)
- One-shot Opportunity type
- Vanish if Listen chosen
- No personal conversation deck

### Conversation Flow

1. **Initiation** (2 attention cost)
   - Player enters with observation cards (if any)
   - NPC in emotional state based on deadlines/history
   
2. **First Turn - Mandatory Listen**
   - No choice - must Listen first
   - Draw cards based on NPC's starting state
   - Establishes opening emotional context

3. **Subsequent Turns** (each costs 1 patience)
   - Choose: Listen or Speak
   - Listen: Draw based on state, lose Opportunity cards
   - Speak: Play cards up to weight limit

4. **End Conditions**
   - Patience exhausted
   - Letter generated (comfort threshold reached)
   - State makes continuation impossible
   - Natural Conclusion (decks exhausted)

---

## EMOTIONAL STATE RULES

Each state modifies BOTH Listen draws AND Speak rules:

| State | LISTEN: Draw From | SPEAK: Special Rules | Weight Limit |
|-------|------------------|---------------------|--------------|
| **NEUTRAL** | 2 Comfort + 1 State | Normal play | 3 |
| **GUARDED** | 1 Comfort + 1 State | Persistent cards stay if not played | 2 |
| **OPEN** | 3 Comfort + 1 State | Can play combinations | 3 |
| **TENSE** | 1 Comfort + 1 Crisis + 1 State | One-shots vanish if not played | 1 |
| **EAGER** | 2 Comfort + 2 State | Same-type cards get +1 comfort | 3 |
| **OVERWHELMED** | 1 Crisis + 1 State | Can only play 1 card total | 1 |
| **CONNECTED** | 2 Comfort + 1 Crisis + 1 State | All cards persistent this turn | 4 |
| **DESPERATE** | 1 Comfort + 2 Crisis + 1 State | Crisis cards cost 0 weight | 3 |
| **HOSTILE** | 3 Crisis + 1 State | Can ONLY play Crisis or State cards | 2 |

**Key**: Every state draws at least 1 State card (prevents deadlock)

---

## EMPTY DECK RESOLUTION

### State Deck - NEVER EMPTIES
- After playing, card returns to BOTTOM of State deck
- Always cycles, always available
- Represents persistent emotional flexibility

### Comfort Deck - CAN EMPTY
- When empty: Required Comfort draws = 0 cards
- Still counts toward total cards drawn
- Represents exhausting pleasant topics

### Crisis Deck - CAN EMPTY
- When empty: Required Crisis draws = 0 cards
- Successfully resolved Crisis cards are REMOVED
- Represents problems being solved

### Edge Case Prevention
- If Listen would draw 0 total cards: Draw 1 extra State card
- If only State cards cycling with ≤2 patience: Can end gracefully
- Natural conversation conclusion when topics exhausted

---

## CARD MECHANICS

### Card Types

**Comfort Cards**
- Build comfort value toward letter generation
- Can combine with other Comfort cards
- Success rate: 70% - (Weight × 10%) + (Status tokens × 3%)

**State Cards**
- Change emotional state on success
- Must be played alone (cannot combine)
- Low weight (0-1), moderate success (50-70%)
- No comfort gain

**Crisis Cards**
- High weight normally (3-5)
- Free (0 weight) in Desperate state
- Often end conversation or create obligations

**Burden Cards**
- Occupy hand slots
- Cannot be discarded except by playing
- Cost comfort to clear (usually -2)
- ONE PURPOSE: Take up mental space

### Persistence Types

- **Persistent**: Stays in hand if not played
- **Opportunity**: ALL vanish if Listen chosen
- **One-shot**: Removed after playing (all observation cards)
- **Burden**: Never vanishes except by resolution

### Success Calculation
```
Base Rate = 70%
- (Weight × 10%)
+ (Status tokens × 3%)
Range: 10% to 95%
```

### Combination Rules
- Only Comfort cards can combine
- State cards must be played alone
- Same token type: +2 comfort (2 cards), +5 (3 cards)

---

## LETTER GENERATION

### Comfort Thresholds
- 5-9 comfort: Simple Letter (24h deadline, 5 coins)
- 10-14 comfort: Standard Letter (12h deadline, 10 coins)
- 15-19 comfort: Important Letter (6h deadline, 15 coins)
- 20+ comfort: Urgent Letter (2h deadline, 20 coins)

### Delivery Effects

**Successful Delivery** transforms recipient's decks:
- REPLACE 3 Comfort cards with 3 relationship-specific cards
- REMOVE 1 Crisis card (crisis resolved)
- State deck unchanged

**Failed Delivery** damages sender's decks:
- ADD 2 Burden cards to Comfort deck
- MOVE 1 Comfort card to Crisis deck
- State deck unchanged

---

## STATE CONSEQUENCES

How you leave an NPC affects next conversation:

- End in **Desperate** → Next starts **Guarded**
- End in **Hostile** → Next starts **Hostile**
- End in **Connected** → Next starts **Open**
- End in **Overwhelmed** → Next starts **Tense**
- End in **Neutral/Open** → Next starts **Neutral**

ONE RULE: Ending state determines starting state.

---

## ELENA SCENARIO SPECIFICS

### Starting Conditions

**Player State**:
- Health: 75 (yesterday's state)
- Hunger: 60 (yesterday's state)
- Current Attention: 6 (from yesterday: 10 - 1 - 2 = 7, spent 1 observing)
- Observation Card: "Guard Checkpoint Warning" (Shadow, W1, +3 comfort)

**Elena's State**:
- Emotional State: **Desperate** (deadline 1h 13m, under 2 hours)
- Base Patience: 12 (Devoted personality)
- Spot Modifier: +1 (Private corner table)
- Total Patience: 13
- Comfort Target: 10 for letter

### Elena's Deck Composition

**Comfort Deck (20 cards)**:
- Small Talk (W1, 70%, +1 comfort) ×3
- Listen Actively (W1, 60%, +2 comfort) ×3
- Share Memory (W2, 50%, +3 comfort) ×2
- Promise to Help (W2, 50%, +4 comfort) ×2
- Deep Empathy (W3, 40%, +5 comfort) ×1
- Pleasant Chat (W1, 60%, +1 comfort) ×3
- Offer Support (W2, 50%, +3 comfort) ×2
- Mention Plans (W1, 60%, +1 comfort) ×2
- Show Respect (W2, 50%, +2 comfort) ×2

**Crisis Deck (8 cards)**:
- Help Me Please (W3, 40%, +2 comfort) ×2
- Time Running Out (W4, 30%, creates urgency)
- Family Pressure (W3, 40%, +1 comfort)
- Desperate Promise (W5/0 in Desperate, 40%, instant letter)
- Breaking Point (W4, 30%, ends conversation)
- Lost Hope (W3, 40%, state → Hostile)
- Father's Debt (Burden, W2, -2 comfort to clear)

**State Deck (5 cards, cycling)**:
- Take a Breath (W1, 60%, Desperate → Tense)
- Open Up (W1, 50%, Any → Open)  
- Pull Back (W0, 70%, Any → Guarded)
- Find Balance (W1, 60%, Any → Neutral)
- Make Connection (W2, 40%, Open → Connected)

### Turn-by-Turn Example

**Turn 1 - Mandatory Listen**
- Desperate: Draw 1 Comfort + 2 Crisis + 1 State
- Draws: "Small Talk", "Help Me Please", "Time Running Out", "Take a Breath"
- Observation card remains (didn't choose to Listen)
- 12 patience remaining

**Turn 2 Options**
- SPEAK: Play observation (W1) + state card (W1) = manage crisis
- SPEAK: Play crisis cards (free in Desperate) = embrace urgency
- LISTEN: Draw more but lose observation

**If choosing manage crisis**:
- Play "Guard Checkpoint Warning" + "Take a Breath"
- Success: +3 comfort, Elena → Tense
- New state changes future draws

**Turn 3 in Tense**
- Tense: Draw 1 Comfort + 1 Crisis + 1 State
- One-shots vanish if not played this turn
- Weight limit now 1 (Tense restriction)

### Multiple Paths

**Standard Path**: 
- Manage emotional states
- Build to 10+ comfort steadily
- 4-5 turns typical

**Crisis Embrace**:
- Stay in Desperate
- Play "Desperate Promise" (40% success)
- Instant letter but adds 3 burdens to Elena

**State Navigation**:
- Desperate → Tense → Neutral → Open
- Each state change affects available cards
- Longer but more controlled

---

## OBSERVATION SYSTEM

### Market Square Observations (Afternoon)

**Available Observations**:
1. "Guard Checkpoint Warning" - 1 attention → Shadow card (W1, +3)
2. "Noble's Departure" - 1 attention → Status card (W2, +5)
3. "Merchant Shortage" - 1 attention → Commerce card (W1, +2)

**Observation Rules**:
- Each costs 1 attention
- Grants one-shot Opportunity card
- Card vanishes if Listen chosen
- Refreshes each time period

---

## EXCHANGE SYSTEM

### Marcus's Exchange Options

**Simple Exchanges**:
- "Buy Provisions": 3 coins → Hunger = 0
- "Buy Medicine": 5 coins → Health +20
- "Quick Work": 2 attention → 8 coins

**Token Exchanges** (require 5 tokens):
- "Merchant Route": 5 Shadow tokens → Unlock hidden path
- "Bulk Deal": 5 Commerce tokens → All items half price

---

## PROGRESSION SYSTEM

### Relationship Milestones

**3 Letters Delivered**:
- All Comfort cards gain +1 base comfort
- Replace 3 generic cards with relationship cards
- Base patience -1 (higher expectations)

**5 Letters Delivered**:
- Generic cards evolve to relationship versions
- "Small Talk" → "Our Usual Chat" (same weight, +1 comfort)
- Comfort threshold +2

**10 Letters Delivered**:
- Deck is 75% relationship-specific
- Can start conversations in Open state
- Access to unique letter types

---

## SUCCESS METRICS

### Optimal Elena Path
1. Turn 1: Mandatory Listen (Desperate draw)
2. Turn 2: Play observation + state card (change to Tense)
3. Turn 3-5: Build comfort in manageable state
4. Turn 6: Reach 10 comfort
5. Generate Standard Letter (12h, 10 coins)
6. Deliver with time to spare

### Crisis Path
1. Turn 1: Mandatory Listen
2. Turn 2: Play "Desperate Promise" (40% chance)
3. Success: Instant letter generation
4. Rush delivery (30 minutes)
5. Elena gains 3 burden cards

### Failure State
- Patience exhausted at <10 comfort
- Elena remains Desperate
- Next conversation starts Guarded
- Deadline passes, obligation failed

---

## KEY DESIGN PRINCIPLES

**No Hidden State**: All information visible
**One Purpose Per Mechanic**: Each system does one thing
**Natural Difficulty**: Success creates complexity
**Conversation Flow**: Must Listen before Speaking
**Deck Psychology**: Different states access different mental spaces
**Prevention of Deadlocks**: State cards always available
**Resource Conversion**: Clear chain of inputs/outputs

---

## IMPLEMENTATION PRIORITIES

### Phase 1: Core Conversation Loop
- Three-deck NPC structure
- Emotional state rules
- Listen/Speak dichotomy
- Empty deck handling

### Phase 2: Resource Integration  
- Health/Hunger → Attention
- Token generation and use
- State consequences

### Phase 3: Progression
- Letter generation
- Deck transformation
- Relationship milestones

### Phase 4: Full System
- Multiple NPCs
- Complete locations
- Full obligation system