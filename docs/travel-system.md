# Travel System Design - Path Discovery Mechanics

## Core Concept

Travel uses persistent path cards that start face-down and flip permanently once discovered. Unlike conversations (success/failure probability) or exchanges (resource trades), travel is about **discovery through exploration**. Each route segment presents 2-3 path choices showing only stamina cost until revealed.

## Path Card Mechanics

### Card States

Each path card exists in one of two permanent states:
- **Face-down**: Shows only name and stamina cost
- **Face-up**: Shows full requirements and effects

Once a card flips face-up (through play or revelation), it remains face-up forever for all future travels.

### Card Properties

```
Path Card Structure:
- Name: Descriptive identifier
- Stamina Cost: 0-4 stamina to play
- Requirements: Additional costs (coins, permits, tokens)
- Effect: What happens when played (time, resources, discoveries)
- Hidden: Boolean - cannot be revealed except by playing
- One-Time: Boolean - special rewards only on first play
```

### Playing Path Cards

1. Each segment presents all path cards for that segment
2. Player sees face-down cards (name + stamina) and face-up cards (everything)
3. Player chooses one card they can afford (stamina + requirements)
4. Card flips face-up permanently if face-down
5. Apply card effects
6. Continue to next segment or complete travel

### Cannot Proceed

If player cannot meet requirements for ANY path in a segment:
- Must turn back immediately
- Time already spent is lost
- Stamina already spent is lost
- Return to starting location

## Knowledge Discovery Methods

### 1. Direct Exploration
- Play the path card spending stamina
- Card flips face-up permanently
- Most expensive but always available

### 2. Investigation
Location investigation can reveal specific path cards:
```
Market Square Investigation:
- Familiarity 1+: Reveals "Merchant Avenue" (toll booth visible)
- Familiarity 2+: Reveals "Main Gate" (guard post observable)
```

### 3. NPC Conversations
Conversation cards can reveal paths NPCs know:
```
"Trade Route Advice" (Marcus's deck)
- Effect: +1 rapport, reveals all non-hidden Warehouse route paths
```

### 4. Exchanges
Direct purchase of route knowledge:
```
"Buy Map" Exchange:
- Cost: 5 coins
- Effect: Reveal all non-hidden paths on one route
```

### 5. Observation Cards
Instead of going to NPC decks, some observations reveal paths:
```
"Complete Route Survey" (Market Square Familiarity 3)
- Effect: Reveals all paths on Market Square ↔ Warehouse route
```

## POC Routes Implementation

### Market Square ↔ Copper Kettle Tavern
**Properties**: 15 minutes base, 0 coins, 0 hunger
**Segments**: 1
**Starting Stamina**: 3

#### Segment 1
```
"Common Room" 
- Stamina: 0
- Requirements: None
- Face-up at start: Yes
- Effect: 15 minutes travel time
- Notes: Safe teaching path

"Back Alley"
- Stamina: 1  
- Requirements: None
- Hidden: No
- Effect: 10 minutes
- Reveals through: Elena conversation "Local Knowledge"
```

#### City Events Deck (3 cards)
- "Clear Path" - No effect
- "Beggar" - Pay 1 coin or add 5 minutes  
- "Street Vendor" - Can buy food for 2 coins

### Market Square ↔ Warehouse District
**Properties**: 20 minutes base, 0 coins, +5 hunger
**Segments**: 2
**Starting Stamina**: 3

#### Segment 1
```
"Dock Workers Path"
- Stamina: 1
- Requirements: None
- Hidden: No
- Effect: 5 minutes, +10 hunger from smell
- Reveals through: Marcus conversation "Trade Route Advice"

"Merchant Avenue"
- Stamina: 2
- Requirements: 2 coins (toll)
- Hidden: No
- Effect: 10 minutes, find "Shipping Manifest" for Marcus (once only)
- Reveals through: Market Square investigation (Familiarity 1+)

"Loading Docks"
- Stamina: 1
- Requirements: None
- Hidden: No  
- Effect: 10 minutes
- Reveals through: Marcus conversation "Trade Route Advice"
```

#### Segment 2
```
"Direct Route"
- Stamina: 1
- Requirements: None
- Hidden: No
- Effect: 10 minutes
- Reveals through: Warehouse investigation (Familiarity 1+)

"Warehouse Maze"  
- Stamina: 0
- Requirements: None
- Hidden: Yes (cannot be revealed except by playing)
- Effect: 15 minutes, find 3 coins (once only, then "Already looted")
- Reveals through: Only by playing

"Security Checkpoint"
- Stamina: 2
- Requirements: None
- Hidden: No
- Effect: 5 minutes if carrying merchant letter, 10 minutes otherwise
- Reveals through: Warehouse investigation (Familiarity 2+)
```

#### Dock Events Deck (4 cards)
- "Busy Period" - Add 5 minutes
- "Worker Strike" - Pay 2 coins or add 10 minutes
- "Loading Accident" - Must return and take different Segment 1 path
- "Shift Change" - Save 5 minutes

### Market Square → Noble Quarter (Checkpoint)
**Properties**: 25 minutes base, 0 coins, 0 hunger
**Segments**: 2
**Starting Stamina**: 3

#### Segment 1
```
"Main Gate"
- Stamina: 1
- Requirements: Noble Permit
- Hidden: No
- Effect: 5 minutes to Segment 2
- Reveals through: Market Square investigation (Familiarity 2+)

"Bribe Guard"
- Stamina: 2
- Requirements: 20 coins
- Hidden: No
- Effect: 10 minutes to Segment 2
- Reveals through: Guard exchange "Route Intelligence"

"Argue Entry"
- Stamina: 1
- Requirements: None
- Hidden: No
- Effect: Waste 60 minutes arguing, return to Market Square
- Reveals through: Guard exchange "Route Intelligence"
```

#### Segment 2
```
"Noble Promenade"
- Stamina: 1
- Requirements: None
- Hidden: No
- Effect: 20 minutes
- Reveals through: Lord Blackwood conversation "Estate Grounds"

"Garden Path"
- Stamina: 2
- Requirements: None
- Hidden: Yes
- Effect: 15 minutes, find "Garden Layout" for Lord Blackwood (once only)
- Reveals through: Only by playing
```

### Market Square → Noble Quarter (Merchant Caravan)
**Properties**: 20 minutes base, 10 coins paid upfront, 0 hunger
**Segments**: 1
**Starting Stamina**: 4 (comfortable travel)
**Access Requirements**: 2+ Commerce tokens with Marcus AND played "Merchant Caravan Route" observation

#### Segment 1
```
"Standard Caravan"
- Stamina: 1
- Requirements: None
- Face-up at start: Yes (Marcus explains)
- Effect: 20 minutes

"Express Wagon"
- Stamina: 3
- Requirements: None
- Face-up at start: Yes (Marcus explains)
- Effect: 10 minutes

"Rest in Caravan"
- Stamina: 0
- Requirements: None
- Face-up at start: Yes (Marcus explains)
- Effect: 20 minutes, restore 1 attention
```

## Travel State System

Similar to Connection States in conversations:

### States and Capacity
- **Fresh**: 3 stamina capacity, can play most paths
- **Steady**: 4 stamina capacity, optimal state
- **Tired**: 2 stamina capacity, limited options
- **Weary**: 1 stamina capacity, only cheap paths
- **Exhausted**: 0 stamina capacity, must REST

### State Transitions
- Start each journey in Fresh state
- Each segment may affect state (future content)
- REST action: Skip segment, add 30 minutes, refresh stamina to capacity

## Integration with Other Systems

### Investigation Rewards
```
Location: Market Square
- Familiarity 1: Reveals "Merchant Avenue" toll on Warehouse route
- Familiarity 2: Reveals "Main Gate" permit requirement on Checkpoint
- Familiarity 3: Reveals all remaining paths on one chosen route
```

### Conversation Cards
```
NPC: Marcus
"Trade Route Advice" (2 focus, Easy)
- Effect: +1 rapport, reveals Warehouse route Segment 1 paths

NPC: Elena  
"Local Knowledge" (1 focus, Easy)
- Effect: +1 rapport, reveals "Back Alley" on Tavern route

NPC: Guard Captain
"Checkpoint Briefing" (3 focus, Medium)
- Effect: +2 rapport, reveals all Checkpoint route paths
```

### Exchange Options
```
Guard Post Exchange:
"Buy Route Map" - 5 coins
- Choose one route, reveal all non-hidden paths

Marcus Exchange:
"Caravan Routes" - Free with 2+ Commerce tokens
- Reveals all Merchant Caravan paths
```

### Observation Discoveries
```
"Shipping Manifest" (found on Merchant Avenue)
- Goes to Marcus's observation deck
- When played: Unlocks special commerce exchange

"Garden Layout" (found on Garden Path)
- Goes to Lord Blackwood's observation deck
- When played: Advances connection state to Receptive
```

## One-Time Discoveries

Certain paths have permanent world effects on first play:

```
"Warehouse Maze" - First play: Find 3 coins
"Merchant Avenue" - First play: Find observation card
"Garden Path" - First play: Find observation card
"Hidden Tunnel" - First play: Unlock new route (future content)
```

After first play, these show modified effects:
- "Warehouse Maze" → "15 minutes (already looted)"
- "Merchant Avenue" → "10 minutes (manifest already taken)"

## Strategic Considerations

### Knowledge Investment
Players choose between:
- Exploration (costs stamina, risks dead ends)
- Investigation (costs attention, reveals infrastructure)
- Conversation (costs focus, reveals NPC knowledge)
- Exchange (costs coins, reveals complete routes)

### Route Mastery
- First journey: Expensive exploration, suboptimal paths
- Second journey: Some knowledge, better choices
- Third journey: Optimal path selection based on current needs

### Resource Routes
After exploration, players know:
- Warehouse Maze gives 3 coins (once)
- Back Alley might have street vendor for food
- Rest in Caravan restores attention

### Dead End Avoidance
Revealing requirements prevents wasted journeys:
- Know checkpoint needs permit before attempting
- Know toll costs before traveling
- Know which paths have eventss

## Implementation Notes

### Path Card Storage
Each route maintains array of segments, each segment contains array of path cards with persistent face-up/face-down state.

### events Deck Management
Each route has optional events deck. When ⚠ triggered, draw one card, apply effect, discard. Reshuffle when empty.

### Knowledge Persistence
Path card states persist across entire game session. Consider persisting across save/load for continued exploration.

### UI Requirements
- Show face-down cards as name + stamina cost only
- Show face-up cards with full details
- Highlight playable paths based on available resources
- Show events deck contents (but not order)

## Future Expansions

### Weather Effects
Storm weather could:
- Add stamina cost to all paths
- Add "Storm Shelter" cards to segments
- Trigger weather-specific eventss

### Time-of-Day Variations
Night travel could:
- Add "Darkness" eventss
- Increase stamina costs
- Hide certain paths until discovered

### Route Unlocking
Successfully playing certain Very Hard paths could unlock entirely new routes between locations, permanently expanding the travel network.


## Core Mechanic: Card Selection

Every travel segment presents 2-3 cards. The player picks ONE card, pays its cost, and applies its effect. This core mechanic remains identical across all transport modes. What changes is whether the cards are fixed (walking) or drawn randomly (caravan).

## Walking Routes: Fixed Path Cards

When walking, each segment has permanently assigned path cards representing physical routes. These cards start face-down and flip permanently face-up when first played.

### Example: Market Square → Warehouse District

**Segment 1 (always these exact 3 cards):**
```
"Dock Workers Path" (1 stamina) 
- Face-down initially
- When played reveals: "5 minutes, +10 hunger from smell"
- Stays face-up forever

"Merchant Avenue" (2 stamina, requires 2 coins)
- Face-down initially  
- When played reveals: "10 minutes, find 'Shipping Manifest' for Marcus (once only)"
- Stays face-up forever

"Loading Docks" (1 stamina)
- Face-down initially
- When played reveals: "10 minutes, draw from Dock events deck"
- Stays face-up forever
```

**Segment 2 (always these exact 3 cards):**
```
"Direct Route" (1 stamina)
- When played reveals: "10 minutes"

"Warehouse Maze" (0 stamina)
- When played reveals: "15 minutes, find 3 coins (once only, then shows 'already looted')"

"Security Checkpoint" (2 stamina)
- When played reveals: "5 minutes with merchant letter, 10 minutes without"
```

The player picks ONE card per segment. Once revealed, that card's effect is known for all future travels.

## Caravan Routes: Event Collections

When riding a caravan, each segment draws ONE event from the route's event pool. Each event contains 2-3 thematically related cards. The player picks ONE card from the drawn event.

### Example: Marcus's Merchant Caravan Event Pool

**Event: "Merchant Gossip"**
```
Contains 3 cards:
- "Listen Carefully" (1 attention) → "Gain 1 Commerce token, reveal one path"
- "Join Discussion" (2 attention + 2 coins) → "Gain 2 Commerce tokens"  
- "Rest Instead" (0 cost) → "Restore 2 stamina"
```

**Event: "Guard Checkpoint"**
```
Contains 3 cards:
- "Marcus Handles It" (0 cost) → "Add 10 minutes"
- "Expedite with Coins" (3 coins) → "No delay"
- "Show Your Letters" (requires letters) → "Save 5 minutes"
```

**Event: "Broken Wheel"**
```
Contains 3 cards:
- "Help Fix" (2 stamina) → "Save 10 minutes, gain 1 Commerce token"
- "Wait Patiently" (0 cost) → "Add 15 minutes"
- "Explore Surroundings" (1 attention) → "Find 3 coins"
```

**Event: "Fellow Passenger - Noble"**
```
Contains 3 cards:
- "Reassure Them" (1 attention) → "Gain 1 Status token"
- "Ignore Them" (0 cost) → "No effect"
- "Mock Their Fear" (0 cost) → "Lose 1 Status token if any, gain 2 coins"
```

### How Caravan Travel Works

1. **Segment 1**: Draw one event randomly (e.g., "Merchant Gossip")
2. Player sees all 3 cards in that event
3. Player picks ONE card (e.g., "Listen Carefully")
4. Apply effect, continue to next segment

5. **Segment 2**: Draw another event randomly (e.g., "Broken Wheel")
6. Player sees all 3 cards in that new event
7. Player picks ONE card (e.g., "Help Fix")
8. Apply effect, continue to next segment

Each journey draws different events, creating unique experiences.

## The Same Core Mechanic

**Walking Segment**: Here are 3 fixed paths → pick one → pay cost → apply effect
**Caravan Segment**: Here are 3 cards from random event → pick one → pay cost → apply effect

The decision structure is identical. The only difference is card source:
- Walking = fixed cards per segment (exploration that leads to mastery)
- Caravan = random event per segment (adaptation to circumstances)

## Complete POC Implementation

### Market Square ↔ Copper Kettle Tavern (Walking)
**1 Segment, Fixed Cards**

**Segment 1:**
- "Common Room" (0 stamina) - Face-up from start: "15 minutes"
- "Back Alley" (1 stamina) - Reveals: "10 minutes, draw City events"

### Market Square ↔ Warehouse District (Walking)
**2 Segments, Fixed Cards**

**Segment 1:**
- "Dock Workers Path" (1 stamina) - Reveals: "5 minutes, +10 hunger"
- "Merchant Avenue" (2 stamina + 2 coins) - Reveals: "10 minutes, find observation"
- "Loading Docks" (1 stamina) - Reveals: "10 minutes, draw Dock events"

**Segment 2:**
- "Direct Route" (1 stamina) - Reveals: "10 minutes"
- "Warehouse Maze" (0 stamina) - Reveals: "15 minutes, 3 coins (once)"
- "Security Checkpoint" (2 stamina) - Reveals: "Varies by letters carried"

### Market Square → Noble Quarter (Checkpoint) (Walking)
**2 Segments, Fixed Cards**

**Segment 1:**
- "Main Gate" (1 stamina + Noble Permit) - Reveals: "5 minutes to Segment 2"
- "Bribe Guard" (2 stamina + 20 coins) - Reveals: "10 minutes to Segment 2"  
- "Argue Entry" (1 stamina) - Reveals: "Waste 60 minutes, return to Market"

**Segment 2:**
- "Noble Promenade" (1 stamina) - Reveals: "20 minutes"
- "Garden Path" (2 stamina) - Reveals: "15 minutes, find observation (once)"

### Market Square → Noble Quarter (Merchant Caravan)
**2 Segments, Event Cards**
**Entry**: 10 coins + 2 Commerce tokens + route observation card

**Event Pool (7 events total, draw 1 per segment):**

**Event: "Smooth Travel"**
- "Sleep" (0 cost) → "Restore 3 stamina"
- "Study Landscape" (1 attention) → "Reveal 2 walking paths"
- "Practice Skills" (1 attention) → "+5 starting rapport next conversation"

**Event: "Merchant Networking"**
- "Listen" (1 attention) → "Gain 1 Commerce token"
- "Participate" (2 attention + 2 coins) → "Gain 2 Commerce tokens"
- "Rest" (0 cost) → "Restore 2 stamina"

**Event: "Guard Stop"**
- "Let Marcus Handle" (0 cost) → "Add 10 minutes"
- "Bribe Guards" (3 coins) → "No delay"
- "Show Papers" (requires letters) → "Save 5 minutes"

**Event: "Mechanical Problem"**
- "Help Repair" (2 stamina) → "Save 10 minutes, +1 Commerce"
- "Wait" (0 cost) → "Add 15 minutes"
- "Scout Ahead" (1 attention) → "Find 3 coins"

**Event: "Noble Passenger"**
- "Converse" (1 attention) → "Gain 1 Status token"
- "Avoid" (0 cost) → "No effect"
- "Insult" (0 cost) → "Gain 2 coins, lose 1 Status if any"

**Event: "Bandit Warning"**
- "Hide" (0 cost) → "Continue normally"
- "Help Guards" (2 stamina) → "Gain 1 Shadow token"
- "Negotiate" (3 coins) → "Learn hidden route"

**Event: "Arrival Options"**
- "Standard" (0 cost) → "Complete journey"
- "Tip Guards" (2 coins) → "Reveal Noble Quarter paths"
- "Rush" (1 stamina) → "Save 5 minutes"

**Journey Example:**
- Segment 1: Draw "Merchant Networking" → Pick "Listen" → Gain Commerce token
- Segment 2: Draw "Mechanical Problem" → Pick "Wait" → Add 15 minutes
Result: Arrived with +1 Commerce token, journey took 35 minutes total

## events Decks (Walking Routes Only)

Some walking path cards have "draw events" as part of their effect. This adds minor randomness to fixed routes.

**City eventss (3 cards):**
- "Clear" - No effect
- "Beggar" - Pay 1 coin or +5 minutes
- "Vendor" - Can buy food for 2 coins

**Dock eventss (4 cards):**
- "Busy" - Add 5 minutes
- "Strike" - Pay 2 coins or +10 minutes  
- "Accident" - Must take different path
- "Shift Change" - Save 5 minutes

When a path card says "draw events," draw one card from that deck and apply its effect after the path's base effect.

## Knowledge Systems

### Walking Routes
- Path cards flip permanently when played
- Can be revealed through investigation, conversation, or exchanges
- Once known, perfect information for planning

### Caravan Routes
- Events remain random each journey
- Can learn general event types through experience
- Cannot predict specific segment draws

## Resource Economics

### Walking Resources
- **Stamina**: Physical exertion to traverse paths
- **Coins**: Tolls and bribes
- **Permits**: Gate access
- **Time**: Accept longer paths to avoid costs

### Caravan Resources  
- **Attention**: Engage with opportunities (NOT stamina - you're riding)
- **Coins**: Purchase advantages
- **Time**: Accept delays for free options
- **Items**: Letters, permits affect specific events

## Why This Design Works

1. **Single Mechanic**: Pick one card from 2-3 options every segment
2. **Transport Variety**: Fixed vs random creates different experiences
3. **Versimilitude**: Walking = choosing paths, Caravan = responding to events
4. **Mastery vs Adaptation**: Walking rewards memory, Caravan rewards flexibility
5. **Resource Logic**: Stamina for walking, attention for riding

Both use identical pick-one-from-available mechanics. The only difference is the source of available cards: fixed (walking) or random draw (caravan).