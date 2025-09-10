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
- Encounter: Boolean - triggers encounter deck draw
- One-Time: Boolean - special rewards only on first play
```

### Playing Path Cards

1. Each segment presents all path cards for that segment
2. Player sees face-down cards (name + stamina) and face-up cards (everything)
3. Player chooses one card they can afford (stamina + requirements)
4. Card flips face-up permanently if face-down
5. Apply card effects
6. If encounter symbol (⚠), draw from route's encounter deck
7. Continue to next segment or complete travel

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

## Encounter System

Path cards with ⚠ symbol trigger encounter draws.

### Encounter Deck Properties
- 3-7 cards per route
- Reshuffles when empty
- Cards are visible (can look through deck) but draw is random
- Effects apply immediately

### Example Encounter Cards
```
City Encounters (Market-Tavern):
- "Clear Path" - No effect
- "Beggar" - Pay 1 coin or add 5 minutes
- "Street Vendor" - Option: buy food for 2 coins
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
- Effect: 10 minutes, ⚠ City Encounter
- Reveals through: Elena conversation "Local Knowledge"
```

#### City Encounter Deck (3 cards)
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
- Effect: 10 minutes, ⚠ Dock Encounter
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

#### Dock Encounter Deck (4 cards)
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
- Know which paths have encounters

## Implementation Notes

### Path Card Storage
Each route maintains array of segments, each segment contains array of path cards with persistent face-up/face-down state.

### Encounter Deck Management
Each route has optional encounter deck. When ⚠ triggered, draw one card, apply effect, discard. Reshuffle when empty.

### Knowledge Persistence
Path card states persist across entire game session. Consider persisting across save/load for continued exploration.

### UI Requirements
- Show face-down cards as name + stamina cost only
- Show face-up cards with full details
- Highlight playable paths based on available resources
- Show encounter deck contents (but not order)

## Future Expansions

### Weather Effects
Storm weather could:
- Add stamina cost to all paths
- Add "Storm Shelter" cards to segments
- Trigger weather-specific encounters

### Time-of-Day Variations
Night travel could:
- Add "Darkness" encounters
- Increase stamina costs
- Hide certain paths until discovered

### Route Unlocking
Successfully playing certain Very Hard paths could unlock entirely new routes between locations, permanently expanding the travel network.

### Travel NPCs
Encounter cards could trigger mini-conversations with travel-specific NPCs who have small decks and unique resources.