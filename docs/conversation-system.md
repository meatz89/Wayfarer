# Wayfarer: Complete Game Design Document

## Core Concept

You are a letter carrier navigating the emotional and physical landscapes of a medieval city. Every conversation is a card-based puzzle where emotional states define the rules. Every delivery permanently reshapes relationships. The world exists as mechanical configurations that AI translates into contextual narrative.

## Design Pillars

**Mechanical Causality**: Every story element emerges from mechanical relationships. NPCs are deck containers, observations are cards, letters are deck modifications.

**Elegant Complexity**: Simple rules create deep gameplay through interaction. One card system, configured differently for each conversation type.

**Perfect Information**: All costs and effects are visible upfront. No hidden mechanics, no percentage modifiers, no abstract resources.

**Living World**: The city breathes through systematic time periods, NPC availability, and observation refresh - not through scripted events.

## Core Game Loop

1. **Explore** locations and observe to gain conversation cards
2. **Converse** with NPCs using emotional state navigation  
3. **Generate** letters by reaching comfort thresholds
4. **Deliver** letters against time pressure
5. **Transform** relationships through successful delivery

Each loop iteration permanently evolves NPC decks and conversation possibilities.

## Player Resources

### Primary Resources
- **Coins** (0-999): Currency for exchanges and travel
- **Health** (0-100): Physical condition, 0 = death  
- **Hunger** (0-100): Increases by 20 per period, affects attention
- **Attention** (0-10): Daily action points, refreshes each morning

### Token Resources
- **Trust**: +1 patience per 2 tokens with Trust-primary NPCs
- **Commerce**: Set bonuses improved by 1
- **Status**: +3% success rate per token
- **Shadow**: Preview next card draw before choosing Listen/Speak

### Resource Management
- Hunger increases automatically each time period
- Health only decreases from injuries/failures
- Attention refreshes to 10 each morning (modified by hunger/health)
- Coins earned through work actions and letter delivery

## Attention Economy

Daily allocation of 10 attention points:
- **Quick Exchange**: 0 attention (instant trade)
- **Observation**: 1 attention (gain conversation card)
- **Crisis Conversation**: 1 attention (forced resolution)
- **Standard Conversation**: 2 attention (full system)
- **Work Action**: 2 attention (convert to coins)

Travel costs TIME (affecting deadlines), not attention. This creates natural tension between staying to maximize local interactions versus traveling to meet obligations.

## Conversation System

### The Core Dichotomy

Each conversation turn, choose ONE:

**LISTEN**: 
- ALL Opportunity cards in hand vanish immediately
- Draw X cards based on emotional state (1-3)
- Emotional state may transition
- Persistent cards remain

**SPEAK**: 
- Play cards up to weight limit (determined by state)
- Cards succeed/fail individually
- Comfort accumulates from successes
- State may change if playing state cards

### Conversation Types

All use the same system with different configurations:

**Quick Exchange** (0 attention, instant)
- Draw 1 card from NPC's exchange deck
- Shows exact cost/reward trade
- Accept or refuse
- No emotional states, no patience

**Crisis Resolution** (1 attention, 3 patience)
- Available only when crisis deck has cards
- Uses crisis deck exclusively
- All other conversations locked until resolved
- Successfully playing crisis cards removes them

**Standard Conversation** (2 attention, 8 patience)
- Full emotional state system
- Complete Listen/Speak mechanics
- Can generate letters at comfort thresholds
- Uses conversation deck

### Emotional States

Nine distinct states that modify conversation rules:

**NEUTRAL** (Default)
- Listen: Draw 2 cards, state unchanged
- Speak: Weight limit 3
- Special: None

**GUARDED**
- Listen: Draw 1 card, state→Neutral
- Speak: Weight limit 2
- Special: None

**OPEN**
- Listen: Draw 3 cards, state unchanged
- Speak: Weight limit 3
- Special: Depth can advance

**TENSE**
- Listen: Draw 1 card, state→Guarded
- Speak: Weight limit 1
- Special: None

**EAGER**
- Listen: Draw 3 cards, state unchanged
- Speak: Weight limit 3
- Special: Playing 2+ same-type cards grants +3 comfort

**OVERWHELMED**
- Listen: Draw 1 card, state→Neutral
- Speak: Maximum 1 card regardless of weight
- Special: None

**CONNECTED** (Pinnacle)
- Listen: Draw 3 cards, state unchanged
- Speak: Weight limit 4
- Special: Depth advances automatically, all comfort +2

**DESPERATE** (Crisis)
- Listen: Draw 2 + inject 1 crisis, state→Hostile
- Speak: Weight limit 3, crisis cards cost 0
- Special: Crisis cards free to play

**HOSTILE** (Breakdown)
- Listen: Draw 1 + inject 2 crisis, conversation ends
- Speak: Only crisis cards playable
- Special: Normal cards cannot be played

### Card System

#### Card Type Separation

**Comfort Cards** (Most common)
- Build comfort through successful play
- CAN combine with other comfort cards
- Get set bonuses for same token type
- Weight + comfort value + type

**State Cards** (Rare)
- Change emotional state when played
- MUST be played alone
- No comfort gain (or minimal)
- Success changes state, failure maintains

**Crisis Cards** (Emergency)
- Appear in crisis states
- Ignore weight limits in Desperate/Hostile
- Must be played alone
- Often end conversation

#### Persistence Types

**Persistent**: Remains in hand if not played when listening
**Opportunity**: Vanishes if you listen (ALL opportunities vanish together)
**One-shot**: Stays in hand but permanently removed from deck when played
**Burden**: Cannot vanish, must be addressed
**Crisis**: Only in crisis states, free to play

#### Success Calculation

```
Base Rate = 70%
- (Weight × 10%)
+ (Status tokens × 3%)
Minimum: 10%, Maximum: 95%
```

### Set Bonuses

Playing multiple cards of same token type:
- 1 card: Base comfort only
- 2 same type: +2 comfort bonus
- 3 same type: +5 comfort bonus
- 4+ same type: +8 comfort bonus (rare)

### Depth System

Depth levels affect card availability and comfort requirements:
- **Depth 0**: Surface (small talk)
- **Depth 1**: Personal (experiences)
- **Depth 2**: Intimate (connection)

Depth advances only in Neutral/Open/Connected states when reaching comfort thresholds.

## NPC System

### NPCs as Deck Containers

Each NPC maintains three separate decks:

**Exchange Deck** (5-10 cards)
- Simple resource trades
- Each card has cost/reward pair
- Context determines narrative framing

**Conversation Deck** (20-25 cards)
- Full emotional conversation cards
- Comfort, State, and Burden cards
- Modified by letter delivery

**Crisis Deck** (Starts empty)
- Added through observations or events
- Forces crisis conversations
- Must be resolved before normal conversations

### Personality Archetypes

**Devoted** (Family/Clergy)
- 12-15 base patience
- Trust-focused deck
- Higher Trust card percentage

**Mercantile** (Traders)
- 10-12 base patience
- Commerce-focused deck
- Trade and negotiation cards

**Proud** (Nobles)
- 8-10 base patience
- Status-focused deck
- Formal interaction cards

**Cunning** (Spies)
- 10-12 base patience
- Shadow-focused deck
- Information and secret cards

**Steadfast** (Workers)
- 11-13 base patience
- Balanced deck composition
- Reliable, persistent cards

### NPC State Determination

Emotional state determined by meeting obligations:
- Expired obligation: Cannot converse (locked)
- <2 hours remaining: Desperate state
- 2-6 hours: Tense state  
- 6-12 hours: Guarded state
- 12+ hours or no obligation: Neutral state

## Exchange System

### Exchange Cards

Direct resource trades with immediate effects:

**Commerce Examples**:
- Baker: "2 coins → Hunger = 0"
- Doctor: "5 coins → Health +30"
- Innkeeper: "3 coins → Skip to Morning, Attention = 10"

**Trust Examples**:
- Laborer: "3 attention → 8 coins"
- Priest: "1 Trust token → Remove 1 Burden"

**Shadow Examples**:
- Smuggler: "5 coins → Access restricted route"
- Informant: "2 coins → Observation location"

The same mechanical card creates different narrative based on NPC context. A "2 coins → Hunger = 0" card becomes "Buy bread" from baker, "Buy soup" from innkeeper, "Buy rations" from merchant.

## Observation System

### How Observations Work

1. Spend 1 attention at specific location spot
2. Choose from available observations (refreshes each period)
3. Receive specific conversation card to hand
4. Card is one-shot, removed after playing
5. Can use in any conversation

### Observation Cards

Observations provide ammunition for conversations:

**Authority Observations** → Shadow cards
- "Guard Schedule": Weight 1, +3 comfort with guards
- "Checkpoint Weakness": Weight 2, creates Open state

**Commerce Observations** → Commerce cards
- "Merchant's Ledger": Weight 2, +5 comfort with traders
- "Supply Shortage": Weight 1, +3 comfort with affected NPCs

**Social Observations** → Trust cards
- "Family Secret": Weight 3, +8 comfort but adds Burden
- "Kind Gesture": Weight 1, +2 comfort universally

**Secret Observations** → Powerful mixed cards
- "Noble's Weakness": Weight 3, creates Connected state
- "Hidden Route": Weight 0, enables special travel

Each location type generates appropriate observations. Markets create commerce observations, temples create trust observations, docks create shadow observations.

## Letter System

### Letter Generation

Through successful conversations:
- 5-9 comfort: Simple Letter (24h deadline, 5 coins)
- 10-14 comfort: Important Letter (12h deadline, 10 coins)
- 15-19 comfort: Urgent Letter (6h deadline, 15 coins)
- 20+ comfort: Critical Letter (2h deadline, 20 coins)

Higher comfort creates tighter deadlines but better rewards - natural risk/reward.

### Letter Effects

**Successful Delivery**:
- Modifies recipient's conversation deck
- Adds relationship cards between sender/recipient
- Generates payment
- May trigger reply letters

**Failed Delivery**:
- Adds burden cards to sender's deck
- Damages relationship permanently
- May add crisis cards
- Reputation consequences

### Letter Types

Categories determine urgency and stakes:
- **Love**: High emotional stakes, trust rewards
- **Business**: Commerce tokens, trade opportunities
- **Plea**: Crisis resolution, urgent deadlines
- **Warning**: Adds crisis cards if delayed
- **Contract**: Legal obligations, status rewards
- **News**: Information spread, multiple recipients

## World Structure

### Spatial Hierarchy

**Region** → **District** → **Location** → **Spot**

Each level provides context for AI narrative generation:
- Regions set tone (prosperity, authority type)
- Districts provide wealth/danger context  
- Locations are actual playable spaces
- Spots are specific interaction points

### Time System

**Six Time Periods Daily**:
- Morning (6-10): Commerce active, shops open
- Midday (10-14): Peak activity, crowds
- Afternoon (14-18): Social time, moderate activity
- Evening (18-22): Taverns busy, shops closing
- Night (22-2): Dangerous, illicit activity
- Deep Night (2-6): Empty, very dangerous

Each period refreshes observations and changes available NPCs/actions.

### Location Spots

Spots within locations have specific properties:
- **Crossroads**: Enables travel action
- **Commercial**: Enables work action
- **Private**: +1 comfort modifier for conversations
- **Public**: -1 comfort modifier
- **Discrete**: Hidden from authority

Movement between spots is instant and free within a location.

### Travel System

**Routes** connect specific locations:
- Single transport type per route (walk/cart/horse/boat)
- Time cost in periods (not attention)
- May have requirements (Status tokens, time of day)
- Encounters based on familiarity level

**Familiarity Levels**:
- Known: No encounters
- Learning: Draw 2 encounters, choose 1
- Unfamiliar: Draw 1 encounter, must face
- Dangerous: Draw 1 encounter with penalties

## Daily Structure

### Action Planning

With 10 attention points daily, typical distribution:
- 2-3 Full conversations (4-6 points)
- 2-3 Observations (2-3 points)
- 1 Work action (2 points)
- Quick exchanges (free, unlimited)

### Time Management

Travel costs time, creating deadline pressure:
- Stay at one location: Maximum conversation opportunities
- Travel between locations: Access different NPCs but lose time
- Meeting obligations: Forces travel at specific times

### Location Day Example

**Morning at Market**:
- Observe merchant argument (1 attention) → Commerce card
- Quick exchange with baker (free) → Breakfast
- Standard conversation with Marcus (2 attention)
- Work hauling goods (2 attention) → 8 coins

**Afternoon Travel**:
- Travel to Tavern (30 minutes, deadline pressure)
- Observe guard gossip (1 attention) → Shadow card
- Crisis conversation with Elena (1 attention)
- Letter generated, new obligation

**Evening at Tavern**:
- conversation with Bertram (3 attention)
- Attention exhausted
- Rest for night

## Progression System

### Relationship Evolution

Relationships aren't numbers but deck composition:
- Delivery success → Better cards added
- Shared experiences → Unique cards
- Failures → Burden cards
- Time → Card degradation

### Unlock Progression

- Early: Only Quick Exchange available
- Level 1: Standard Conversations unlock
- Level 3: conversations unlock
- Level 5: Special conversation types

### Economic Progression

- Letters provide primary income
- Work actions provide steady coins
- Higher relationships → Better letters
- Better letters → Tighter deadlines → Higher pay

## Content Generation

### Mechanical Contracts

Every element defined mechanically:
- NPCs: Deck compositions + personality type
- Observations: Card rewards + location/time availability
- Letters: Deadline/payment/deck modifications
- Routes: Time cost + requirements

### AI Translation

The AI translates mechanical elements contextually:
- Same exchange card → Different narrative per NPC
- Emotional states → Appropriate dialogue tone
- Observation cards → Knowledge discovery scenes
- Letter delivery → Relationship evolution narrative

### No Authored Content

Stories emerge from:
- Player choices in conversations
- Letter delivery success/failure
- Observation timing
- Meeting obligation punctuality
- Deck evolution over time

## Victory Conditions

### 10-Minute Demo
Deliver Elena's letter successfully:
- Navigate her Desperate state
- Generate letter through conversation
- Reach Noble District before deadline
- Complete delivery for payment

### Full Game
Build a network of fulfilled obligations:
- Develop multiple relationships
- Create letter chains
- Manage competing deadlines
- Achieve economic stability
- Unlock hidden locations/routes

## Core Innovations

### Unified Mechanics
- Single card system configured differently
- Emotional states as different game modes
- Patience as depth selector
- Observations as conversation ammunition

### Emergent Narrative
- NPCs defined by decks, not scripts
- Crisis states from deck composition
- Relationships through deck evolution
- Stories from mechanical causality

### Player Agency
- All costs visible upfront
- Choose conversation depth intentionally
- Manage attention economy strategically
- Create narrative through mechanical choices

## Design Principles

**One Purpose Per Element**: Observations give cards. Exchanges trade resources. Conversations generate letters. Nothing does multiple things.

**No Hidden State**: Every effect is immediate and visible. No future benefits, no percentage modifiers behind the scenes.

**Configuration Over Complexity**: Same mechanics, different configurations. Emotional states change rules, not add modifiers.

**Mechanical Versimilitude**: The mechanics model reality. Limited patience for quick talks. Crisis states force urgent resolution. Time pressure from deadlines.

This is Wayfarer: A game where every conversation is a puzzle, every letter changes the world, and every story emerges from mechanical poetry.