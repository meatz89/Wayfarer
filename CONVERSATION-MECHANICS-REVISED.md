# PURE MECHANICAL CONVERSATION SYSTEM

## Core Design: State-Driven Generation
No cards, no templates - pure mechanical generation from current game state.

## THE CONVERSATION ENGINE

### 1. CONVERSATION AS PRESSURE VALVE

Think of conversation as managing pressure between two vessels:

```
PLAYER PRESSURE          NPC PRESSURE
    [####...]     ←→     [...#####]
       4/10               6/10
```

**Pressure Sources:**
- Player: Unpaid debts, undelivered letters, low resources
- NPC: Their needs, their leverage, their knowledge of your situation

**Mechanical Rule:** Whoever has higher pressure controls conversation flow

### 2. THE ATTENTION ECONOMY

**3 Attention Points** distribute across three channels:

```
OBSERVE [0-3]: See what NPC reveals
GUARD   [0-3]: Protect what you reveal  
PRESS   [0-3]: Push your agenda
```

**The Mechanical Tension:** You can't do all three. Examples:
- 3 OBSERVE, 0 GUARD, 0 PRESS = Learn everything, reveal everything, achieve nothing
- 0 OBSERVE, 3 GUARD, 0 PRESS = Learn nothing, reveal nothing, achieve nothing
- 1 OBSERVE, 1 GUARD, 1 PRESS = Balanced but mediocre at all

### 3. TOPIC GENERATION FROM STATE

Topics aren't cards but emerge from mechanical intersection:

```python
def generate_available_topics():
    topics = []
    
    # From player state
    if player.has_overdue_letters:
        topics.append("Delivery delays")
    if player.coin < 5:
        topics.append("Need for work")
    if player.stamina < 30%:
        topics.append("Exhaustion")
    
    # From NPC state  
    if npc.has_unfulfilled_request:
        topics.append("Their pending request")
    if npc.knows_player_secret:
        topics.append("What they know")
    
    # From location context
    if location.recent_event:
        topics.append(location.recent_event)
    
    # From relationship dynamics
    if relationship.trust < 3:
        topics.append("Small talk")  # Safe
    else:
        topics.append("Personal matters")  # Risky
    
    return topics
```

Topics are ALWAYS generated from current state, never pre-defined.

### 4. THE REVELATION MECHANIC

Information isn't traded but EXTRACTED through mechanical advantage:

**Revelation Threshold = NPC Guard - Player Press**

```
If Player Press > NPC Guard:
    Reveal information tier = difference
    Tier 1: Surface info (shop hours, common knowledge)
    Tier 2: Useful info (shortcuts, NPC schedules)  
    Tier 3: Secret info (hidden routes, NPC weaknesses)
```

**Critical:** NPCs also extract info from you using same mechanic!

### 5. NPC BEHAVIORAL PATTERNS (Not Personality)

NPCs have mechanical behaviors based on their current state:

```python
def calculate_npc_allocation(npc_state):
    observe = 0
    guard = 0  
    press = 0
    
    # Desperate NPCs press hard
    if npc_state.desperate:
        press = 3
        
    # Suspicious NPCs observe carefully
    elif npc_state.suspicious:
        observe = 2
        guard = 1
        
    # Content NPCs are balanced
    elif npc_state.content:
        observe = 1
        guard = 1
        press = 1
        
    return observe, guard, press
```

NPC behavior changes based on their mechanical state, not fixed personality.

### 6. THE CONVERSATION PHYSICS

**Momentum System:**
Each exchange has weight and direction:

```
MOMENTUM = (Pressure Differential) × (Topic Weight)

High pressure + Heavy topic = Massive momentum (hard to change direction)
Low pressure + Light topic = Low momentum (easy to redirect)
```

**Topic Weights** (generated from state):
- Overdue letter from noble: HEAVY (weight 3)
- Weather chat: LIGHT (weight 1)
- Request for work: MEDIUM (weight 2)

### 7. THE ESCALATION LADDER

Conversation naturally escalates through mechanical pressure:

```
Round 1: Opening Moves (low stakes)
         ↓ (pressure builds)
Round 2: Positioning (medium stakes)
         ↓ (pressure builds)
Round 3: Core Exchange (high stakes)
         ↓ (pressure builds)
Round 4: Resolution or Breakdown
```

Each round, unresolved pressure carries forward and compounds.

### 8. BODY LANGUAGE AS PRESSURE READOUT

Generated purely from mechanical state:

```python
def generate_body_language(npc_pressure, allocation):
    if npc_pressure > 7:
        base = "white knuckles"
    elif npc_pressure > 4:
        base = "tense shoulders"
    else:
        base = "relaxed posture"
    
    if allocation.observe > allocation.press:
        focus = "watching carefully"
    elif allocation.press > allocation.observe:
        focus = "leaning forward"
    else:
        focus = "neutral stance"
    
    return f"{base}, {focus}"
```

### 9. THE BREAKDOWN MECHANIC

When total pressure exceeds 15 (both vessels combined):
- Conversation BREAKS
- Someone storms off
- Relationship damage
- Information gates close

This creates natural time pressure - resolve before breakdown!

## EXAMPLE MECHANICAL PLAY

**Game State:**
- Player has 3 overdue letters (pressure: 6)
- NPC is merchant needing delivery (pressure: 4)
- Total pressure: 10/15 (getting dangerous)

**Round 1:**
- Topics generated: "Overdue deliveries", "Need work", "Market conditions"
- Player allocates: OBSERVE-2, GUARD-1, PRESS-0
- NPC allocates: OBSERVE-1, GUARD-0, PRESS-2
- Result: Player learns NPC is desperate, NPC learns about overdue letters

**Round 2:**
- Pressure increases: 12/15 (critical!)
- New topics: "Urgent delivery job", "Payment terms"
- Player must decide: Take risky job or let conversation break?

**UI Generation:**
- Body: "Merchant grips counter edge" (from pressure 6)
- Internal: "They know about my delays" (from OBSERVE result)
- Peripheral: "Other customers waiting" (from location state)
- Tones: ["Desperate", "Aggressive"] (from high pressure)

## WHY THIS IS BETTER

1. **No Content Creation**: Everything emerges from game state
2. **Natural Tension**: The 3-point allocation creates impossible choices
3. **Meaningful State**: Every aspect of game state affects conversations
4. **Predictable Chaos**: Players can read the pressure but can't control it fully
5. **No Special Rules**: Same mechanics for all NPCs, different states create variety

## THE KEY INSIGHT

Conversations aren't scripted events but PRESSURE RELEASE VALVES. The game builds pressure through:
- Overdue letters
- Depleting resources
- Unfulfilled obligations
- Accumulated secrets

Conversations are where pressure either releases productively (get help, find solutions) or destructively (breakdown, damaged relationships).

The UI isn't showing pre-written content but visualizing mechanical pressure states through generated descriptions. Every conversation is unique because the mechanical state is never identical.