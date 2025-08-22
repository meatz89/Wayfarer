# Wayfarer Mechanical Content Contracts

## Core Design Philosophy

This system generates all narrative through mechanical card exchanges and resource management. NPCs are deck containers whose cards determine all possible interactions. Observations provide cards as conversation ammunition. Every mechanic has immediate, visible effects with no hidden state tracking.

## Player Resources

The player manages these trackable resources:

**Core Resources:**
- `coins`: Integer (0-999) - Currency for exchanges and travel
- `health`: Integer (0-100) - Physical condition, 0 = death
- `hunger`: Integer (0-100) - Increases by 20 per time period, 100 = starving
- `attention`: Integer (0-10) - Daily action points, refreshes each morning
- `currentLocation`: Location ID - Where player currently is
- `currentSpot`: Spot ID - Specific spot within location
- `currentPeriod`: Enum (Morning/Midday/Afternoon/Evening/Night)

**Token Resources:**
- `trustTokens`: Integer - Affects patience with Trust-focused NPCs
- `commerceTokens`: Integer - Improves set bonuses
- `statusTokens`: Integer - Improves success rates
- `shadowTokens`: Integer - Provides conversation advantages

## Entity Definitions

### NPC (Deck Container)

**Properties:**
- `id`: Unique identifier
- `personalityType`: Enum (Devoted/Mercantile/Proud/Cunning/Steadfast)
- `locationId`: String (where they permanently reside)
- `spotId`: String (specific spot at location)
- `exchangeDeck`: List of exchange cards (5-10 cards)
- `conversationDeck`: List of conversation cards (20-25 cards)
- `crisisDeck`: List of crisis cards (starts empty)
- `basePatienceRange`: [min, max] integers (8-15 typically)
- `primaryTokenType`: TokenTypes enum
- `relationshipLevel`: Integer (0-10)
- `narrativeTags`: List of strings for AI generation

**Conversation Options:**
Each NPC offers different conversation types based on state and decks:
- Quick Exchange: If exchange deck has cards (0 attention)
- Crisis Resolution: If crisis deck has cards (1 attention, 3 patience)
- Standard Conversation: Always available (2 attention, 8 patience)
- Deep Conversation: If relationship ≥ 3 (3 attention, 12 patience)

**Implementation Notes:**
NPCs don't move between locations. Their decks determine what interactions are possible. Crisis cards force crisis conversations until resolved. The same NPC becomes different puzzles based on deck state.

### Card Types

#### Exchange Card (Quick Trades)
**Properties:**
- `id`: Unique identifier
- `cardType`: "Exchange"
- `tokenType`: Enum (Trust/Commerce/Status/Shadow)
- `cost`: Resource requirement
- `reward`: Resource provided
- `narrativeTags`: List of strings

**Cost/Reward Structure:**
- `resourceType`: Enum (Coins/Health/Hunger/Attention/Time)
- `amount`: Integer or special value

**Example Exchange Cards:**
```
Baker's Commerce: Cost: 2 coins → Reward: Hunger = 0
Doctor's Commerce: Cost: 5 coins → Reward: Health +30
Laborer's Trust: Cost: 3 attention → Reward: 8 coins
Innkeeper's Commerce: Cost: 3 coins → Reward: Skip to Morning, Attention = 10
```

#### Conversation Card (Standard System)
**Properties:**
- `id`: Unique identifier
- `cardType`: Enum (Comfort/State/Crisis)
- `weight`: Integer (0-3 normal, 5+ crisis)
- `tokenType`: Enum (Trust/Commerce/Status/Shadow)
- `persistenceType`: Enum (Persistent/Opportunity/OneShot/Burden/Crisis)
- `successEffect`: Comfort value or state transition
- `failureEffect`: Reduced comfort or no change
- `combineRestriction`: Enum (Combinable/SoloOnly)

#### Observation Card (Player Ammunition)
**Properties:**
- `id`: Unique identifier
- `cardType`: "Observation"
- `weight`: Integer (typically 1-2)
- `tokenType`: Enum (matches observation context)
- `persistenceType`: Always "OneShot"
- `conversationEffect`: Mechanical effect when played
- `validTargets`: List of NPC types or specific NPCs
- `narrativeTags`: List of strings

**Example Observation Cards:**
```
"Merchant's Ledger": Weight 2, Commerce, +5 comfort with merchants
"Guard Schedule": Weight 1, Shadow, Creates Open state with guards
"Noble's Secret": Weight 3, Status, +8 comfort but adds Burden
```

### Observation (Knowledge Opportunity)

**Properties:**
- `id`: Unique identifier
- `locationId`: String (where observation available)
- `spotId`: String or null (specific spot requirement)
- `timePeriods`: List of periods when available
- `attentionCost`: Integer (usually 1)
- `observationCard`: Card player receives
- `narrativeScene`: Description template for AI

**Implementation Notes:**
Observations give players specific cards they can use in conversations. Each observation is available only during certain time periods at specific spots. Once taken, the observation is consumed for that day. The card received is added to the player's hand for use in any conversation.

### Letter (Obligation Package)

**Properties:**
- `id`: Unique identifier
- `senderNpcId`: String
- `recipientNpcId`: String
- `urgencyLevel`: Enum (Routine/Important/Urgent/Critical)
- `deadlineHours`: Integer (based on urgency: Critical=2, Urgent=6, Important=12, Routine=24)
- `coinReward`: Integer (higher for tighter deadlines)
- `successEffect`: Deck modifications for sender/recipient
- `failureEffect`: Relationship damage
- `letterCategory`: Enum (Love/Business/Plea/Warning/Contract/News)

**Deck Modification Structure:**
- `targetNpcId`: String
- `cardsToAdd`: List of cards to add to conversation deck
- `cardsToRemove`: List of card IDs to remove
- `crisisCardsToAdd`: List of crisis cards if failure

**Implementation Notes:**
Letters are generated through successful conversations reaching comfort thresholds. Delivery modifies NPC decks permanently, changing future conversations. Failed deliveries add burden cards to sender's deck.

### Location (Spatial Container)

**Properties:**
- `id`: Unique identifier
- `parentDistrictId`: String
- `locationType`: Enum (Market/Tavern/Temple/Palace/Street/Square/Shop/Home)
- `locationSpots`: List of spot IDs
- `narrativeTags`: List of strings for AI description

**Time-Based Availability:**
- `periodSchedules`: List of period configurations

**Period Configuration:**
- `timePeriod`: Enum
- `availableNpcs`: List of NPC IDs present
- `availableObservations`: List of observation IDs
- `crowdLevel`: Enum (Empty/Sparse/Moderate/Busy/Packed)

### Location Spot (Interaction Point)

**Properties:**
- `id`: Unique identifier
- `parentLocationId`: String
- `spotType`: Enum (Table/Corner/Bar/Stage/Altar/Counter/Booth/Alcove/Entrance)
- `spotTraits`: List of traits
- `capacity`: Integer (how many can occupy)
- `comfortModifier`: Integer (-2 to +2, affects patience in conversations)

**Spot Traits:**
- `Crossroads`: Enables travel action
- `Private`: +1 comfort, better for intimate conversations
- `Public`: -1 comfort, exposed
- `Commercial`: Enables work action

### Route (Travel Path)

**Properties:**
- `id`: Unique identifier
- `originLocationId`: String
- `destinationLocationId`: String
- `timeCost`: Integer (periods consumed)
- `coinCost`: Integer (0 for walking)
- `requirements`: List of conditions
- `dangerLevel`: Integer (0-5, affects encounters)

**Route Requirements:**
- `minStatusTokens`: Integer or null
- `timePeriods`: List of when available
- `discovered`: Boolean (hidden routes need discovery)

## System Rules

### Daily Attention Economy

**Attention Allocation:**
- Start each day with 10 attention (modified by health/hunger)
- Quick Exchange: 0 attention
- Observation: 1 attention
- Crisis Conversation: 1 attention
- Standard Conversation: 2 attention
- Deep Conversation: 3 attention
- Work Action: 2 attention

**Resource Depletion:**
- Hunger increases by 20 each time period
- Health only decreases from events/injuries
- Attention refreshes to 10 each morning (if rested)

### Conversation Configurations

All use the same emotional state system, different patience allocations:

**Quick Exchange** (0 attention, instant):
- Draw 1 card from NPC's exchange deck
- Show cost and reward
- Player accepts or refuses
- No emotional states, no patience

**Crisis Resolution** (1 attention, 3 patience):
- Available only when crisis deck has cards
- Uses only crisis deck
- All other conversations locked until resolved
- Successfully playing crisis cards removes them

**Standard Conversation** (2 attention, 8 patience):
- Full emotional state system
- Normal Listen/Speak mechanics
- Can generate letters at comfort thresholds
- Uses conversation deck

**Deep Conversation** (3 attention, 12 patience):
- Extended emotional navigation
- Higher comfort thresholds
- Better letter rewards
- Requires relationship level 3+

### Emotional States (For Conversations Only)

The 9 states apply only to Standard and Deep conversations:

**NEUTRAL**: Draw 2, weight limit 3
**GUARDED**: Draw 1, weight limit 2, Listen→Neutral
**OPEN**: Draw 3, weight limit 3
**TENSE**: Draw 1, weight limit 1, Listen→Guarded
**EAGER**: Draw 3, weight limit 3, +3 bonus for 2+ same type
**OVERWHELMED**: Draw 1, max 1 card, Listen→Neutral
**CONNECTED**: Draw 3, weight limit 4, depth advances
**DESPERATE**: Draw 2 + crisis, crisis free, Listen→Hostile
**HOSTILE**: Draw 1 + 2 crisis, only crisis playable

### Observation System

**How Observations Work:**
1. Spend 1 attention at specific spot
2. Receive observation card to hand
3. Card is one-shot, removed after playing
4. Different observations available each time period
5. Can use observation cards in any conversation

**Observation Refresh:**
- New observations each time period
- Location type determines observation types
- Cannot repeat same observation in one day

### Letter Generation

**Through Conversations:**
- 5-9 comfort: Simple Letter (24h deadline, 5 coins)
- 10-14 comfort: Important Letter (12h deadline, 10 coins)
- 15-19 comfort: Urgent Letter (6h deadline, 15 coins)
- 20+ comfort: Critical Letter (2h deadline, 20 coins)

**Letter Effects:**
- Delivery modifies recipient's conversation deck
- Failed delivery adds burden cards to sender
- Some letters trigger reply letter generation

## Content Generation Pipeline

### Exchange Generation
1. NPC type determines exchange deck composition
2. Each exchange card has mechanical cost/reward
3. AI translates based on NPC context:
   - Baker + "2 coins → Hunger = 0" = "Fresh bread for sale"
   - Guard + "5 coins → Pass" = "Checkpoint fee"
   - Doctor + "5 coins → Health +30" = "Medical treatment"

### Observation Generation
1. Location type determines observation types available
2. Time period affects which observations appear
3. Each observation provides specific card as reward
4. AI generates scene describing how knowledge was gained

### Conversation Generation
1. Check which conversation types available (based on decks/state)
2. Display options with patience and attention costs
3. Player chooses depth of engagement
4. Use appropriate deck and rules for chosen type

## Implementation Priority

1. **Core resource system** (coins, health, hunger, attention)
2. **Exchange cards** and quick exchange mechanics
3. **Observation cards** as player ammunition
4. **Multiple conversation depths** with clear costs
5. **Crisis deck** forcing urgent conversations
6. **Letter deck modifications** from delivery
7. **Time period** observation refresh

## Design Principles

**No Hidden State**: Every effect is immediate and visible. No "tomorrow you get X" or "influences future Y."

**Everything Is Cards**: NPCs are deck containers. Observations give cards. Letters modify decks. Relationships are deck evolution.

**Clear Trades**: Every exchange shows exact cost and reward. Every conversation shows patience and attention cost upfront.

**One Purpose Per Element**: Observations give cards. Exchanges trade resources. Conversations generate letters. No element does multiple things.

**Mechanical Clarity**: The same card creates different narrative through AI translation, but the mechanical effect never changes.