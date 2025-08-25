# Wayfarer - Refined Game Design Document

## Core Concept
A letter carrier navigates medieval city relationships through card-based conversations where emotional states define rules. Every delivery permanently reshapes NPC decks. The innovation: conversation cards generate contextual dialogue each turn while maintaining mechanical consistency.

## Design Pillars (Refined)

**1. Mechanical Clarity First**
Every mechanic is immediately visible. Narrative enhances but never obscures function.

**2. Dynamic Restraint**  
Only conversation cards generate fresh narrative. Everything else uses minimal static text.

**3. Progressive Disclosure**
Information reveals through player action, not passive display.

**4. Elegant Simplicity**
One card system configured differently creates depth without complexity.

---

## CORE MECHANICS

### Resource System

**Primary Resources** (Always Visible)
- **Coins** (0-999): Currency for exchanges
- **Health** (0-100): Physical condition, 0 = death
- **Hunger** (0-100): +20 per period, affects attention
- **Attention** (0-10): Daily action points

**Token Resources** (Affect Conversations)
- **Trust**: +1 patience per 2 tokens
- **Commerce**: Improves set bonuses
- **Status**: +3% success per token
- **Shadow**: Preview next card draw

**Design Principle**: Resources display as numbers only. No narrative description.

### Time System

**Six Daily Periods**
- Morning (6-10)
- Midday (10-14)
- Afternoon (14-18)
- Evening (18-22)
- Night (22-2)
- Deep Night (2-6)

**Deadline Pressure Tiers**
- **>12 hours**: Normal display
- **6-12 hours**: Yellow warning
- **2-6 hours**: Orange warning
- **<2 hours**: Red pulsing warning → NPC Desperate state
- **<30 min**: Critical animations

**Design Principle**: Deadline pressure shows visually, not through narrative flooding.

---

## CONVERSATION SYSTEM

### The Core Innovation

**Dynamic Card Narrative**: Each conversation card displays contextually generated dialogue based on:
- Current turn number
- Conversation history
- Emotional state
- Deadline pressure
- Previous cards played
- Relationship level

**Mechanical Constants**: While narrative changes, these never change:
- Card name (label only)
- Weight value
- Success percentage  
- Comfort/effect values
- Card type

### Conversation Types

**1. Quick Exchange** (0 attention, instant)
- No emotional states
- Single card draw from exchange deck
- Shows cost → reward
- Accept or decline

**2. Crisis Resolution** (1 attention, 3 patience)
- Only when crisis deck has cards
- Forces immediate resolution
- Crisis cards cost 0 weight
- Other conversations locked

**3. Standard Conversation** (2 attention, 8 patience base)
- Full emotional state system
- Listen/Speak each turn
- Weight limits apply
- Generates letters at thresholds

**4. Deep Conversation** (3 attention, 12 patience base)
- Requires relationship level 3+
- Higher comfort thresholds
- Better rewards
- More complex states

### Emotional State Rules

| State | Draw | Weight Limit | Special Rules |
|-------|------|-------------|---------------|
| **Neutral** | 2 | 3 | Default state |
| **Guarded** | 1 | 2 | Listen→Neutral |
| **Open** | 3 | 3 | Can advance depth |
| **Tense** | 1 | 1 | Listen→Guarded |
| **Eager** | 3 | 3 | +3 comfort for 2+ same type |
| **Overwhelmed** | 1 | 1 max cards | Listen→Neutral |
| **Connected** | 3 | 4 | Auto-advance depth |
| **Desperate** | 2+crisis | 3 | Crisis cards free |
| **Hostile** | 1+2 crisis | Crisis only | Ends next turn |

### The Listen/Speak Dichotomy

**LISTEN Action**
- All Opportunity cards vanish immediately
- Draw X cards based on state
- May trigger state transition
- Costs 1 patience

**SPEAK Action**  
- Play cards up to weight limit
- Each card resolves individually
- Accumulate comfort from successes
- Costs 1 patience

### Card Types

**Comfort Cards** (Most common)
- Build toward letter generation
- Can combine with others
- Success rate: 70% - (Weight × 10%)

**State Cards** (Rare)
- Change emotional state
- Must play alone
- Success changes state

**Crisis Cards** (Emergency)
- Appear in crisis states
- Free in Desperate/Hostile
- Often end conversation

**Burden Cards** (Negative)
- Block hand slots
- Must resolve to remove
- Created by failures

### Card Persistence Types

- **Persistent**: Stays if not played
- **Opportunity**: Vanishes if Listen chosen
- **One-shot**: Removed after playing
- **Burden**: Cannot vanish until resolved
- **Crisis**: Only in crisis states

### Success Calculation

```
Base Rate = 70%
- (Weight × 10%)
+ (Status tokens × 3%)
Range: 10% to 95%
```

### Set Bonuses (Same Token Type)
- 1 card: Base comfort
- 2 cards: +2 comfort
- 3 cards: +5 comfort
- 4+ cards: +8 comfort

### Letter Generation Thresholds
- 5-9 comfort: Simple (24h deadline, 5 coins)
- 10-14 comfort: Important (12h, 10 coins)
- 15-19 comfort: Urgent (6h, 15 coins)
- 20+ comfort: Critical (2h, 20 coins)

---

## UI DISPLAY PRINCIPLES

### Three-Tier Information Hierarchy

**Tier 1: Always Visible** (Static)
- Resource numbers
- Progress bars
- State indicators
- Card mechanics
- Location names

**Tier 2: Discoverable** (Costs Action)
- Observation details (pay attention first)
- NPC conversations (initiate first)
- Exchange specifics (engage first)
- Travel routes (select first)

**Tier 3: Contextually Generated** (Dynamic)
- Conversation card dialogue
- NPC responses
- Critical narrative moments
- Letter content

### Progressive Disclosure Pattern

**Observations Example**:
```
Before: "Guard Checkpoint Activity" [1 attention] [Shadow +3]
After paying: Full narrative about checkpoint delays and alternate routes
```

**Design Principle**: Generate narrative only when player commits resources.

### Visual Communication Priority

Instead of narrative descriptions, use:
- Color coding for urgency (green→yellow→red)
- Icons for resource types
- Progress bars for goals
- Badges for urgent elements
- Animations for critical states

---

## NPC SYSTEM

### NPCs as Deck Containers

Each NPC maintains:
- **Exchange Deck** (5-10 cards): Simple trades
- **Conversation Deck** (20-25 cards): Standard cards
- **Crisis Deck** (0+): Emergency cards
- **Burden Deck** (0+): Failed obligations

### Personality Archetypes

| Type | Patience | Focus | Never Says |
|------|----------|-------|------------|
| **Devoted** | 12-15 | Trust, family, duty | Cynical calculations |
| **Mercantile** | 10-12 | Profit, trade | Emotional over practical |
| **Proud** | 8-10 | Status, dignity | Begging, weakness |
| **Cunning** | 10-12 | Secrets, leverage | Full transparency |
| **Steadfast** | 11-13 | Reliability | Deception |

### State Determination by Deadline
- No obligation: Neutral
- 12+ hours: Neutral
- 6-12 hours: Guarded
- 2-6 hours: Tense
- <2 hours: Desperate
- Expired: Locked (cannot converse)

---

## OBSERVATION SYSTEM

### Observation Mechanics

**Cost**: 1 attention
**Reward**: One-shot conversation card
**Availability**: Refreshes each time period
**Display**: Title only until purchased

### Observation Types by Location

| Location | Observation Types | Card Rewards |
|----------|------------------|--------------|
| Markets | Commerce, shortage info | Commerce cards +3-5 |
| Taverns | Personal secrets, gossip | Trust cards +2-4 |
| Temples | Spiritual insights | Trust cards +3-5 |
| Streets | Authority movements | Shadow cards +2-4 |
| Noble Areas | Status information | Status cards +4-6 |

**Design Principle**: Observations are ammunition for conversations, not story beats.

---

## EXCHANGE SYSTEM

### Exchange Display

**Minimal Format**:
```
[Exchange Name]
Cost → Reward
```

**Examples**:
- "Buy Food": 3 coins → Hunger = 0
- "Quick Work": 30 minutes → 8 coins
- "Trade Info": Guard intel → Hidden route

**Design Principle**: Same mechanical exchange generates different NPC dialogue based on context, but card shows only mechanics.

---

## LOCATION SYSTEM

### Spatial Hierarchy
Region → District → Location → Spot

### Location Display

**Always Visible**:
- Location name
- Current spot
- Available NPCs (names only)
- Basic actions

**On Interaction**:
- Spot descriptions
- NPC emotional states
- Available observations

### Spot Properties
- **Crossroads**: Enables travel
- **Commercial**: Enables work
- **Private**: +1 comfort modifier
- **Public**: -1 comfort modifier

**Design Principle**: Spots display as simple lists until player engages.

---

## LETTER SYSTEM

### Letter Properties
- Sender/Recipient
- Urgency level
- Deadline hours
- Coin reward
- Category (love/business/plea/warning)

### Delivery Effects
**Success**: Modifies recipient's conversation deck
**Failure**: Adds burden cards to sender

**Design Principle**: Letter content generates only when created, not displayed constantly.

---

## CONTENT GENERATION RULES

### What Generates Dynamically

**Always**:
1. Conversation card dialogue (per turn)
2. NPC responses to card plays
3. Letter content when generated

**On Trigger**:
1. Observation narratives (when purchased)
2. Exchange resolutions (when accepted)
3. Major state transitions

### What Remains Static

**Always**:
1. UI labels and buttons
2. Resource displays
3. Location/spot names
4. Progress indicators
5. Basic action descriptions

### Context Object for Generation

Required context for each generation:
```
{
  current_turn: number,
  conversation_history: array,
  emotional_state: enum,
  deadline_remaining: minutes,
  recent_cards_played: array,
  relationship_level: number,
  active_obligations: array,
  environmental_context: {
    time_period: enum,
    weather: string,
    season: string
  }
}
```

---

## NARRATIVE GENERATION PRINCIPLES

### Dynamic Card Example

**"Promise to Help" Evolution**:
- Turn 1: "I know I failed before, but I promise I'll help."
- Turn 3: "The merchant route avoids checkpoints. I promise."
- Turn 5: "We're close. I promise to see this through."
- Turn 7: "One final push. I promise."

### Restraint Guidelines

**Maximum Text Per Element**:
- Card dialogue: 2 sentences
- NPC response: 3 sentences
- Atmosphere: 1 sentence
- Observation: 1 paragraph (after purchase)

**Never Generate**:
- Descriptions of UI elements
- Explanations of mechanics
- Redundant information
- Purple prose

---

## TECHNICAL IMPLEMENTATION

### Generation Triggers

**Per Turn**:
- Conversation cards in hand (batch request)
- NPC response to plays

**Per Action**:
- Observation purchase
- Exchange acceptance
- Letter generation

**Caching Strategy**:
- Cache generated card text for current turn
- Invalidate on state change
- Preserve conversation history

### Performance Targets
- Card text generation: <1 second
- NPC response: <1 second  
- Full hand refresh: <2 seconds

---

## PROGRESSION SYSTEM

### Relationship Evolution
Not a number but deck composition:
- Successful deliveries add better cards
- Failed obligations add burden cards
- Shared experiences create unique cards

### Unlock Path
1. Start: Only Quick Exchanges
2. Level 1: Standard Conversations
3. Level 3: Deep Conversations
4. Level 5: Special interactions

---

## VICTORY CONDITIONS

### 10-Minute Demo
Successfully navigate Elena's desperate conversation:
- Build 10 comfort or play crisis card
- Generate letter
- Deliver before deadline

### Full Game
- Develop relationship network
- Manage competing obligations
- Achieve economic stability
- Unlock all districts

---

## CORE INNOVATIONS SUMMARY

1. **Dynamic Card Narrative**: Same mechanics, different story each turn
2. **Progressive Disclosure**: Information reveals through action
3. **Emotional States as Rules**: States change game rules, not just tone
4. **Deck Evolution**: Relationships are deck composition changes
5. **Visual Over Verbal**: Urgency through color/animation, not text

---

## DESIGN CONSTRAINTS

### Cognitive Load Management
- Maximum 3-5 text blocks visible
- Core action in center screen
- Secondary info in periphery
- Details on demand only

### Narrative Restraint
- One dynamic element per interaction
- Static text for navigation
- Generated text only for emotional beats
- Mechanics always visible

### Player Agency
- All costs visible upfront
- Consequences clear before action
- No hidden information
- Strategic depth through simplicity

---

## WHAT THIS GAME IS NOT

- Not a visual novel with cards
- Not a card battler with story
- Not a dating sim
- Not a text adventure
- Not a deck builder

## WHAT THIS GAME IS

**Wayfarer is a conversation engine where mechanical card play generates contextual narrative. Every conversation is unique because the same cards mean different things at different moments. The city lives through systematic time and deadline pressure. Stories emerge from deck evolution and successful deliveries.**

The core loop is elegant:
1. Observe to gain information (observation cards into deck)
2. Converse using contextual cards (combination of npc deck and player deck)
3. Generate letters through conversation (at certain cards played / game state achieved)
4. Deliver against deadlines (determined by the obligation queue)
5. Permanently reshape relationships (through letter delivery + conversation)

Everything else supports this loop.