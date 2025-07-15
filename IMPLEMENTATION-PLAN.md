# Wayfarer Unified Implementation Plan: Complete Game Abstraction System

## Core Design Philosophy

Transform all simulation systems into **collectible game pieces** with **management rules** that create optimization puzzles through **indirect resource effects**.

## **Complete System Architecture**

### **1. Action Card System (Core Player Capability)**

**Purpose**: Replace stamina/energy with tactical hand management

**Card Categories**:
- **Physical Cards**: Mountain Trained, Cargo Experienced, Craft Skilled, Road Hardened (Power 1-3)
- **Social Cards**: Trade Savvy, Authority Trained, Street Smart, Well Connected (Power 1-3)
- **Mental Cards**: Route Wise, Market Sharp, Detail Focused, Big Picture (Power 1-3)

**Universal Applicability**: All cards of same type can be used for basic actions of that type
**Categorical Bonuses**: Each card provides specific advantages for related activities

**Physical Card Specializations**:
- **Mountain Trained**: Basic physical actions + mountain route categorical bonus
- **Cargo Experienced**: Basic physical actions + heavy cargo handling categorical bonus
- **Craft Skilled**: Basic physical actions + precision workshop categorical bonus
- **Road Hardened**: Basic physical actions + travel endurance categorical bonus

**Social Card Specializations**:
- **Trade Savvy**: Basic social actions + merchant negotiation categorical bonus
- **Authority Trained**: Basic social actions + leadership/command categorical bonus
- **Street Smart**: Basic social actions + deception/subtlety categorical bonus
- **Well Connected**: Basic social actions + relationship/network categorical bonus

**Mental Card Specializations**:
- **Route Wise**: Basic mental actions + navigation/planning categorical bonus
- **Market Sharp**: Basic mental actions + trade analysis categorical bonus
- **Detail Focused**: Basic mental actions + observation/investigation categorical bonus
- **Big Picture**: Basic mental actions + strategic/complex planning categorical bonus

**Categorical Bonus Examples**:
- **Mountain Route**: Requires Physical Power 2 OR Mountain Trained Power 1
- **Heavy Cargo Contract**: Requires Physical Power 2 OR Cargo Experienced Power 1
- **Precision Workshop**: Requires Physical Power 2 OR Craft Skilled Power 1
- **Merchant Negotiation**: Requires Social Power 2 OR Trade Savvy Power 1
- **Route Planning**: Requires Mental Power 2 OR Route Wise Power 1
- **Market Analysis**: Requires Mental Power 2 OR Market Sharp Power 1

**Universal Usage**: All Physical cards work for basic physical actions (travel, basic work), all Social cards work for basic social actions (conversation, simple negotiation), all Mental cards work for basic mental actions (observation, planning).

**Specialization Advantage**: Cards with matching categorical bonus can substitute higher power requirements with lower power + specialization, creating strategic collection incentives.

**Hand Management Mechanics**:
- **Hand Size**: 6 cards maximum
- **Card Consumption**: Actions require specific card types + power levels
- **Discard Pile**: Used cards go to discard pile
- **Refresh System**: Rest action moves cards from discard to hand

**Equipment Integration**: Equipment grants bonus cards while equipped
- Climbing Gear: +1 Mountain Trained card
- Fine Clothes: +1 Well Connected card  
- Scholar's Tools: +1 Detail Focused card
- Trade Tools: +1 Craft Skilled card

**Progression Through Acquisition**:
- **Mountain Trained**: Earned through completing dangerous mountain contracts
- **Cargo Experienced**: Earned through completing heavy cargo transport contracts
- **Craft Skilled**: Earned through completing precision workshop contracts
- **Road Hardened**: Earned through completing long-distance travel contracts
- **Trade Savvy**: Earned through completing profitable merchant negotiations
- **Authority Trained**: Earned through completing leadership/command contracts
- **Street Smart**: Earned through completing deception/infiltration contracts
- **Well Connected**: Earned through completing relationship-building contracts
- **Route Wise**: Earned through discovering new routes or travel optimizations
- **Market Sharp**: Earned through completing market analysis or trade intelligence
- **Detail Focused**: Earned through completing investigation or observation contracts
- **Big Picture**: Earned through completing complex multi-stage contracts

### **2. Health System (Indirect Card Economy Control)**

**Purpose**: Affect action card refresh rate without directly blocking actions

**Health Scale**: 1-10 points

**Core Interaction**: **Health = Card Refresh Rate**
- Health 10: Refresh all discarded cards during rest
- Health 7: Refresh 5 cards during rest
- Health 4: Refresh 3 cards during rest
- Health 1: Refresh 1 card during rest

**Health Loss Triggers**:
- Dangerous routes without proper equipment: -1 health
- Heavy work while injured: -1 health
- Protection contracts: Accept health risk for higher payment

**Health Recovery**:
- Rest at private lodging: +1 health per night
- Healer services: +3 health for 5 coins + 1 time period
- Medicine items: Herbs (+1), Bandages (+2)

### **3. Equipment Condition System (Usage-Based Degradation)**

**Purpose**: Replace durability with discrete condition states

**Condition States**:
- **Fresh**: Full effectiveness, equipment works as designed
- **Worn**: Reduced effectiveness (-1 power level requirement)
- **Damaged**: Cannot use until repaired

**Condition Loss Triggers**:
- Dangerous routes: -1 condition level
- Heavy work (Power 3 actions): -1 condition level
- Protection/combat contracts: -1 condition level
- **No Time Decay**: Only usage triggers condition loss

**Repair System**:
- **Workshop Repair**: 2 coins + Physical Power 1 → restore 1 condition level
- **Repair Kit**: Consumable item → restore 1 condition level
- **Master Repair**: 5 coins → restore to Fresh condition

### **4. Removed Systems**

**Reputation System**: Completely removed
- No relationship tracking
- No reputation-based contract access
- All NPC interactions purely transactional (cards + equipment + money)

**Location Access System**: Completely removed  
- All locations immediately accessible
- Natural barriers through action requirements, not artificial gates

## **System Interconnections**

### **Core Resource Optimization Loop**
**Health → Card Refresh → Action Capability → Economic Efficiency → Health Investment**

### **Equipment Strategy Triangle**
**Equipment Slots ↔ Card Bonuses ↔ Trade Cargo**
- Equipment provides card bonuses but takes inventory space
- More cards = more action options
- Trade cargo provides money for equipment/health

### **Condition Management Pressure**
**Equipment Usage → Condition Degradation → Repair Costs → Economic Pressure**
- Using equipment degrades condition
- Degraded equipment less effective
- Repair competes with other money uses

## **Implementation Plan**

### **Phase 1: Remove Old Systems**
**Priority**: Remove simulation systems first to establish clean foundation

1. **Remove Reputation System**
   - Delete `ReputationManager` class
   - Remove reputation fields from NPC objects
   - Remove reputation-based contract filtering
   - Convert reputation gates to card requirements

2. **Remove Location Access System**
   - Delete location access validation
   - Remove access-level checking from location entry
   - Make all locations immediately accessible
   - Update UI to remove access indicators

### **Phase 2: Implement Core Game Systems**
**Priority**: Action cards first, then supporting systems

3. **Implement Action Card System**
   - Create `ActionCard` data structure with type and power
   - Implement hand management (6 card limit)
   - Create card consumption mechanics for all actions
   - Add rest action for card refresh
   - Integrate equipment card bonuses

4. **Implement Health System**
   - Add health resource (1-10 scale)
   - Implement health → card refresh rate logic
   - Add health loss triggers to dangerous actions
   - Create health recovery mechanics (lodging, healer, medicine)

### **Phase 3: Add Advanced Game Systems**

5. **Implement Equipment Condition System**
   - Replace durability with condition states (Fresh/Worn/Damaged)
   - Add condition loss triggers to specific actions
   - Implement condition effects on equipment effectiveness
   - Create repair mechanics and costs

### **Phase 4: Integration and Balance**
6. **System Integration Testing**
   - Verify all systems interact through indirect effects only
   - Test resource optimization puzzles across all systems
   - Validate strategic depth through playtesting
   - Balance resource costs and benefits

## **Strategic Gameplay Examples**

### **The Exhausted Expert Scenario**
**Situation**: Health 3, can only refresh 3 cards per rest
**Hand**: 2 Basic Physical Power 1, 1 Trade Savvy Power 2
**Challenge**: Mountain contract needs Physical Power 2, but worn climbing gear makes it Power 3
**Options**: 
- Repair climbing gear (2 coins + Physical Power 1) → enables contract with basic cards
- Rest to refresh cards hoping for Mountain Trained Power 1 card (would bypass power requirement)
- Decline contract and focus on health recovery first
**Decision**: Resource allocation between immediate opportunity vs capability building

### **The Specialization Investment Dilemma**
**Situation**: Market day tomorrow, have Trade Savvy Power 1 card
**Options**:
- Use Trade Savvy for immediate merchant negotiation work (4 coins due to specialization bonus)
**Knowledge Value**: Price card might reveal 6+ coin opportunities vs guaranteed 4 coins from specialized work
**Decision**: Immediate specialized income vs information investment with compound returns

### **The Equipment Condition Crisis**
**Situation**: Climbing gear at Damaged condition, mountain route saves 1 time period
**Current Cards**: Mountain Trained Power 1 (would normally make route easy)
**Problem**: Damaged gear blocks categorical bonus, now need Physical Power 2
**Repair Cost**: 2 coins + Physical Power 1 card
**Alternative**: Use slow route (extra 1 time period) but preserve resources
**Decision**: Equipment maintenance vs time efficiency vs resource conservation

## **Success Metrics**

### **Player Experience Goals**
- **Tactical Planning**: Players spend time optimizing card usage and resource allocation
- **Collection Strategy**: Players develop personal approaches to card acquisition  
- **Compound Decisions**: Every choice affects multiple resource types with cascading consequences
- **No Simulation Tracking**: Players manage concrete game pieces, not abstract meters

### **Strategic Depth Indicators**
- Multiple viable playstyles through different card/equipment specializations
- Meaningful trade-offs between immediate benefits and long-term capability building
- Resource scarcity creates genuine optimization challenges
- System mastery improves efficiency without breaking game balance

This unified system transforms Wayfarer into a **tactical resource management game** where all player progression occurs through **collecting and optimizing concrete game pieces** rather than accumulating abstract simulation points, perfectly implementing the core design principle of game abstraction through indirect resource effects.