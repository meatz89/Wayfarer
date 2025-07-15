# Wayfarer Action Card System Design

## Core Design Philosophy

**Game Abstraction Principle**: Abstract real-world concepts into **game mechanics** that create optimization puzzles through categorical relationships rather than narrative simulation.

**Indirect Effects Rule**: No system directly affects another system. All interactions occur through **resource modification** or **interaction space changes**.

## Action Card System Overview

### Core Abstraction
Replace stamina/energy points with **Action Cards** representing the character's physical, social, and mental capabilities.

**Real-World Concept**: Human energy and focus capacity  
**Game Abstraction**: Collectible cards with categorical requirements and power levels  
**Strategic Layer**: Hand management, card allocation, and refresh timing decisions

### Card Categories & Power Levels
- **Physical Cards**: Strength, Endurance, Agility, Precision
- **Social Cards**: Charm, Persuasion, Deception, Leadership  
- **Mental Cards**: Analysis, Observation, Planning, Knowledge

**Power Levels**: 1-3 (determines threshold requirements)
- Power 1: Basic actions
- Power 2: Skilled actions  
- Power 3: Master-level actions

### Hand Management Mechanics
- **Hand Size**: 6 cards maximum
- **Card Consumption**: Actions consume specific card types based on requirements
- **Discard Pile**: Used cards go to discard pile
- **Refresh System**: Rest action moves cards from discard pile back to hand
- **Decision Layer**: Which cards to use vs which cards to save for future actions

## System Interactions Through Card Requirements

### Travel System Integration
**Route Requirements**:
- **Mountain Routes**: Physical card (Power 2+)
- **Social Routes**: Social card (Power 1+) 
- **Navigation Routes**: Mental card (Power 2+)
- **Complex Routes**: Multiple card types

**Equipment Effects**:
- **Climbing Gear**: Reduces Physical requirement from Power 2 to Power 1
- **Navigation Tools**: Reduces Mental requirement from Power 2 to Power 1
- **Fine Clothes**: Enables Social routes that require appearance

### Work System Integration
**Contract Requirements**:
- **Heavy Labor**: Physical card (Power 2+)
- **Negotiation Work**: Social card (Power 1+)
- **Planning Work**: Mental card (Power 2+)
- **Complex Contracts**: Multiple card types + specific power levels

**Payment Scaling**: Higher power requirements = higher payment

### Market System Integration
**Trading Actions**:
- **Price Negotiation**: Social card (Power 1+)
- **Market Research**: Mental card (Power 1+)
- **Bulk Trading**: Physical card (Power 1+) for handling large items

### Location System Integration
**Access Requirements**:
- **Workshop Entry**: Physical or Mental card (Power 1+)
- **Tavern Negotiations**: Social card (Power 1+)
- **Dangerous Locations**: Physical card (Power 2+)

## Health Integration (Indirect Effects)

### Health → Card Refresh Rate
**The Only Direct Health Effect**:
- Health 10: Refresh all discarded cards during rest
- Health 7: Refresh 5 cards during rest
- Health 4: Refresh 3 cards during rest  
- Health 1: Refresh 1 card during rest

### Cascade Effects
**Low Health → Fewer Cards Available → Fewer Actions Possible → Reduced Capability**

**No Direct Action Blocking**: Health never prevents actions directly, only affects resource availability through card refresh limitations.

### Health Loss Mechanics
**Categorical Health Risks**:
- **Dangerous Routes**: Risk 1 health loss without proper equipment
- **Heavy Labor**: Risk 1 health loss when attempted at low health
- **Protection Contracts**: Accept health risk for higher payment

**Health Recovery**:
- **Rest Recovery**: +1 health per night at private lodging
- **Healer Services**: +3 health for 5 coins + 1 time period
- **Medicine Items**: Herbs (+1 health), Bandages (+2 health)

## Equipment Integration (Card Bonuses)

### Equipment Grants Bonus Cards
**While Equipped**:
- **Climbing Gear**: +1 Physical card
- **Fine Clothes**: +1 Social card
- **Scholar's Tools**: +1 Mental card
- **Master's Tools**: +1 Multi-type card (Physical+Mental)

### Strategic Equipment Choices
**Equipment vs Cargo Trade-off**:
- Equipment occupies inventory slots but provides card advantages
- More cards = more action options = strategic flexibility
- **Decision**: Immediate trading profit vs long-term capability enhancement

## Progression System (Card Collection)

### Card Acquisition
**Earn Cards Through Success**:
- Complete physical contracts → Gain Physical cards
- Complete social contracts → Gain Social cards  
- Complete mental contracts → Gain Mental cards
- Discover rare equipment → Gain unique cards
- Build relationships with NPCs → Gain specialized cards

### Card Quality Progression
**Starting State**: All Power 1 cards
**Progression Path**:
- **Skilled Cards**: Power 2 (access to skilled actions)
- **Master Cards**: Power 3 (unlock advanced actions)
- **Combo Cards**: Multi-type (Physical+Social, Mental+Physical)
- **Unique Cards**: Special abilities (route-specific, NPC-specific)

### Collection Strategy
**Card Diversity vs Specialization**:
- Generalist: Balanced collection across all types
- Specialist: Focus on one type for deep capability
- **Trade-off**: Broad capability vs specialized power

## Strategic Decision Examples

### The Card Allocation Puzzle
**Current Hand**: 2 Physical (Power 1), 2 Social (Power 2), 2 Mental (Power 1)
**Available Actions**:
- Mountain route (needs Physical Power 2) - **Impossible**
- Negotiation work (needs Social Power 1) - **Possible**
- Market research (needs Mental Power 1) - **Possible**
- **Equipment Available**: Climbing gear (+1 Physical card)

**Strategic Decision**: Use Social card for negotiation work to earn money for climbing gear, or save Social card for better opportunity tomorrow?

### The Rest Timing Dilemma
**Current State**: Health 6 (can refresh 4 cards), 3 cards in hand, 6 cards in discard
**Tomorrow's Contract**: Needs Physical + Social cards
**Discard Pile**: 3 Physical, 2 Social, 1 Mental

**Strategic Decision**: Rest now to refresh needed cards, or push forward with current hand and risk having wrong cards tomorrow?

### The Equipment vs Cargo Trade-off
**Inventory**: 4 slots available
**Options**: Climbing gear (+1 Physical card) OR 2 trade goods (4 coin profit)
**Context**: Going to mountain region with multiple route options

**Strategic Decision**: Card advantage for route flexibility vs immediate profit potential?

## Elegant Scaling Properties

### Natural Scaling Through Card Economy
- **More Cards**: More action options per day
- **Better Cards**: Access to advanced actions and higher payments
- **Equipment**: Card bonuses create strategic inventory choices
- **Health**: Card availability creates capability management

### No Artificial Modifiers
- **No Percentage Bonuses**: No +10% or -5% modifiers
- **No Sliding Scales**: No gradual difficulty changes
- **Pure Categorical**: Either have required cards or don't
- **Clear Thresholds**: Power levels create definitive capability gates

### Compound Strategic Decisions
**Every Resource Affects Card Economy**:
- **Health Management**: Affects card refresh rate
- **Equipment Strategy**: Affects card bonus optimization
- **Time Management**: Affects card usage optimization
- **Inventory Management**: Equipment vs cargo vs medicine trade-offs

## Implementation Guidelines

### Card Requirements Definition
**Action Template Structure**:
```
Action: "Mountain Route Travel"
Requirements: Physical_Card(Power: 2) OR Physical_Card(Power: 1) + Climbing_Equipment
Time_Cost: 1 period
Health_Risk: 1 (without Weather_Protection)
```

### Equipment Card Bonuses
**Equipment Template Structure**:
```
Equipment: "Climbing Gear"
Inventory_Slots: 1
Card_Bonus: +1 Physical_Card (while equipped)
Route_Modifier: Mountain_Routes(Power: 2→1)
```

### Health-Card Refresh Matrix
**Rest Action Logic**:
```
Card_Refresh_Count = min(Current_Health, Discard_Pile_Size)
Available_Cards = Hand_Cards + Refreshed_Cards
Max_Hand_Size = 6
```

This system transforms Wayfarer into a **tactical resource management game** where every decision involves optimizing card allocation across competing demands, creating deep strategic gameplay through simple categorical interactions.


# Wayfarer System Changes: Game Abstraction Implementation

## Core Changes Overview

Transform simulation systems into game mechanics that create optimization puzzles through collectible resources and management decisions.

## **1. REMOVE: Reputation System**

### What to Remove
- All reputation tracking (numerical values, faction standing)
- Reputation-based contract access
- Reputation-based price modifiers
- Reputation-based NPC behavior changes

### What Replaces It
**Pure Transactional NPCs**: All NPC interactions based solely on:
- Required action cards (Physical/Social/Mental + Power levels)
- Required equipment categories
- Required money payment
- Required knowledge cards (see below)

### Implementation Changes
- Remove `ReputationManager` class
- Remove reputation fields from `NPC` objects
- Remove reputation checks from contract availability
- Convert all reputation gates to card/equipment requirements

**Example Transformation**:
- **Before**: "Need 50+ Merchant reputation for bulk contracts"
- **After**: "Need Social Power 2 card + Trade_Tools equipment for bulk contracts"

## **2. REMOVE: Location Access System**

### What to Remove
- Location access restrictions
- Social expectation requirements for entry
- Locked/unlocked location states
- Access-based progression gating

### What Replaces It
**Open World Access**: All locations accessible to all players, with natural barriers through:
- Action requirements (need specific cards for location actions)
- Equipment requirements (need tools for workshop actions)
- Economic barriers (services cost money)
- Time constraints (NPCs have schedules)

### Implementation Changes
- Remove `LocationAccess` validation
- Remove access-level checking from location entry
- Remove social standing requirements from location spots
- All locations become immediately accessible

**Example Transformation**:
- **Before**: "Need Noble reputation to enter Manor"
- **After**: "Manor accessible to all, but Noble contracts require Social Power 3 cards"

## **3. ADD: Knowledge Card System**

### Core Mechanics
**Knowledge as Collectible Cards**: Transform abstract information into concrete game pieces

**Card Categories**:
- **Route Cards**: Enable specific route optimizations
- **Market Cards**: Show current prices for specific goods
- **Weather Cards**: Predict conditions affecting routes
- **Technique Cards**: Improve specific action types

### Card Properties
- **Usage Limits**: Single-use, daily-use, or permanent
- **Expiration**: Market cards expire after 1-2 days
- **Collection Limits**: Maximum hand size for knowledge cards
- **Acquisition Methods**: Earned through specific actions with NPCs

### Implementation Structure
```
KnowledgeCard {
  Type: Route/Market/Weather/Technique
  Usage: SingleUse/DailyUse/Permanent
  Expiration: Days remaining
  Effect: Specific mechanical benefit
  Acquisition: Source action/NPC
}
```

### Strategic Examples
**Route Weather Card (Single-Use)**:
- **Acquisition**: Spend Social Power 1 + 2 coins with Traveler NPC
- **Effect**: Reveals next day's weather for specific route
- **Decision**: Use now for current planning vs save for future critical journey

**Market Price Card (Expires in 2 days)**:
- **Acquisition**: Spend Mental Power 1 + 1 time period at Market
- **Effect**: Shows exact buy/sell prices for all goods at specific location
- **Decision**: Travel to exploit prices vs hold card for future opportunities

**Climbing Technique Card (Permanent)**:
- **Acquisition**: Complete 3 mountain route contracts
- **Effect**: Mountain routes require Physical Power 1 instead of Power 2
- **Collection Strategy**: Long-term investment in route specialization

## **4. ADD: Equipment Condition Token System**

### Core Mechanics
**Discrete Condition States**: Replace durability with token-based condition tracking

**Condition Token Types**:
- **Fresh Token**: Equipment works at full effectiveness
- **Worn Token**: Equipment works but provides reduced benefits
- **Damaged Token**: Equipment requires repair before use

### Condition Change Triggers
**Specific Actions Consume Condition**:
- **Dangerous Routes**: Mountain/River routes consume 1 condition level
- **Heavy Work**: Physical Power 3 actions consume 1 condition level
- **Combat/Protection**: Guard contracts consume 1 condition level
- **No Time Decay**: Equipment only degrades through usage

### Repair System
**Restoration Actions**:
- **Workshop Repair**: Spend 2 coins + Physical Power 1 → restore 1 condition level
- **Field Repair**: Spend repair kit item → restore 1 condition level
- **Master Repair**: Spend 5 coins → restore to Fresh condition

### Implementation Structure
```
EquipmentCondition {
  Fresh: Full benefits
  Worn: -1 Power level effectiveness
  Damaged: Cannot use until repaired
}

ConditionTriggers {
  DangerousRoute: -1 condition
  HeavyWork: -1 condition  
  CombatAction: -1 condition
}
```

### Strategic Examples
**Climbing Gear Condition Management**:
- **Fresh Climbing Gear**: Enables Mountain Power 2 routes
- **Worn Climbing Gear**: Requires Physical Power 3 for same routes
- **Damaged Climbing Gear**: Cannot access mountain routes until repaired
- **Decision**: Risk further damage for immediate route access vs repair first

## **5. KEEP: Simple Money System**

### Why Keep Money Unchanged
- **Universal Understanding**: Players immediately understand coin economy
- **Clear Purpose**: Buy equipment, pay for services, hire NPCs
- **Authentic Abstraction**: Historically accurate for medieval setting
- **Strategic Clarity**: Single currency prevents confusing token portfolio management

### Money Integration with New Systems
**Knowledge Card Economy**:
- Buy information from NPCs using coins
- No complex token exchanges needed

**Equipment Repair Economy**:
- Repair services cost coins
- Repair items purchasable with coins

**Action Card Economy**:
- Equipment that grants bonus cards costs coins
- No artificial token conversion needed

## **Implementation Priority**

### Phase 1: Remove Systems
1. **Remove Reputation System**
   - Delete reputation tracking code
   - Convert reputation gates to card requirements
   - Update NPC interaction logic

2. **Remove Location Access**
   - Delete access validation code
   - Make all locations immediately accessible
   - Update UI to remove access indicators

### Phase 2: Add Game Systems
3. **Implement Knowledge Cards**
   - Create KnowledgeCard data structure
   - Add knowledge hand management
   - Create knowledge acquisition actions
   - Implement card usage mechanics

4. **Implement Condition Tokens**
   - Replace equipment durability with condition states
   - Add condition change triggers
   - Implement repair actions
   - Update equipment effectiveness based on condition

## **Strategic Transformation Examples**

### Before (Simulation Systems)
**Scenario**: Player wants access to premium contracts
- **Process**: Build reputation through grinding relationship activities
- **Barriers**: Arbitrary numerical thresholds, unclear progression
- **Decisions**: "Do I have enough reputation points?"

### After (Game Systems)
**Scenario**: Player wants access to premium contracts
- **Process**: Collect required action cards + equipment + knowledge cards
- **Barriers**: Concrete resource requirements, clear collection goals
- **Decisions**: "Do I have Social Power 3 card + Noble Technique card + Market Intelligence card?"

### Resource Management Transformation
**Before**: Track reputation numbers, location access, durability percentages
**After**: Manage action card hand, knowledge card collection, equipment condition tokens

This creates **tactical resource allocation** across concrete game pieces rather than abstract simulation tracking, perfectly implementing the core design principle of game abstraction through indirect resource effects.