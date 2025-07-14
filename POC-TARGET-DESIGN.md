# Wayfarer Minimal POC Target Design

**⚠️ IMPORTANT: This is the TARGET DESIGN specification, not current implementation status.**

## CORE DESIGN TARGET

Create **impossible scheduling conflicts** and **equipment trade-offs** that force strategic prioritization even with minimal content. Players should face genuine optimization puzzles where perfect solutions don't exist.

## ENTITY LIMITS (5-8 Each)

### LOCATIONS (3)
1. **Millbrook** (Starting Hub) - [Market], [Tavern], [Workshop]
2. **Thornwood** (Resource Hub) - [Logging_Camp], [Tavern]
3. **Crossbridge** (Trade Hub) - [Market], [Workshop], [Dock]

### ROUTES (8)
1. **Main Road** (Millbrook ↔ Crossbridge) - [Cart] compatible, 2 periods
2. **Mountain Pass** (Millbrook ↔ Crossbridge) - Requires [Climbing], 1 period
3. **Forest Trail** (Millbrook ↔ Thornwood) - Requires [Navigation], 1 period
4. **River Route** (Thornwood ↔ Crossbridge) - Requires [Navigation], 1 period
5. **Logging Road** (Millbrook ↔ Thornwood) - [Cart] compatible, 2 periods
6. **Trade Highway** (Crossbridge ↔ Thornwood) - [Cart] compatible, 2 periods
7. **Rapids Route** (Thornwood ↔ Crossbridge) - Requires [Climbing] + [Navigation], 1 period
8. **Direct Path** (Thornwood ↔ Millbrook) - Requires [Climbing], 1 period

### EQUIPMENT CATEGORIES (3)
1. **[Climbing]** - Enables mountain routes, takes 1 slot, costs 5 coins + 1 period at workshop
2. **[Navigation]** - Prevents getting lost in forest/river routes, takes 1 slot, costs 5 coins + 1 period at workshop
3. **[Trade_Tools]** - Enables workshop access and craft contracts, takes 1 slot, costs 5 coins + 1 period at workshop

### ITEM SIZE SYSTEM (Separate from Equipment)
- **Standard Items**: 1 slot each (herbs, grain, basic goods)
- **Large Items**: 2 slots each (tools, crafted goods, bulk materials)
- **Base Inventory**: 4 slots
- **Cart Transport**: +3 slots (7 total) but blocks mountain/forest routes

### STAMINA SYSTEM
- **Base Stamina**: 10 points
- **Travel Cost**: 1 stamina per period traveled
- **Work Cost**: 2 stamina per period worked (contracts, equipment commissioning)
- **Recovery**: 3 stamina per rest period, 6 stamina per night's sleep

### ROUTE DISCOVERY SYSTEM
- **All routes visible**: Players can see all 8 routes from the start
- **Equipment requirements shown**: Route interface displays required equipment categories
- **Blocked routes**: Attempting routes without required equipment shows "Cannot travel - requires [Climbing]"
- **Discovery through failure**: Players learn equipment needs by attempting blocked routes

### CONTRACT SYSTEM
**Each NPC offers renewable contracts:**
- **Delivery Contracts**: Transport goods between locations (3-5 coins, 1-2 day deadlines)
- **Labor Contracts**: Work for NPCs at their location (4-6 coins, 1 day completion)
- **Transport Contracts**: Help move goods using specific routes (6-8 coins, equipment requirements)

### NPCS (Location-Specific, 9 total)
**Millbrook**:
- **[Workshop_Master]** - Commissions equipment, offers workshop contracts
- **[Market_Trader]** - Buys/sells trade goods, offers delivery contracts
- **[Tavern_Keeper]** - Offers room rentals, simple labor contracts

**Thornwood**:
- **[Logger]** - Offers logging contracts, sells lumber/grain
- **[Herb_Gatherer]** - Sells herbs, offers gathering contracts
- **[Camp_Boss]** - Offers heavy labor contracts, equipment repair

**Crossbridge**:
- **[Dock_Master]** - Sells fish, offers dock work contracts
- **[Trade_Captain]** - Offers transport contracts, bulk trade deals
- **[River_Worker]** - Simple labor contracts, boat maintenance work

### CONTRACTS (4 types)
1. **[Rush]** - 1 day deadline, 15 coins, requires specific equipment
2. **[Standard]** - 3 days deadline, 8 coins, moderate requirements
3. **[Craft]** - 2 days deadline, 12 coins, requires [Trade_Tools] + workshop access
4. **[Exploration]** - 5 days deadline, 6 coins + discovery bonus, requires terrain equipment

### TRADE GOODS (6)
1. **[Grain]** - Standard (1 slot), buy Thornwood (2 coins), sell Crossbridge (4 coins) = 2 profit/slot
2. **[Herbs]** - Standard (1 slot), buy Thornwood (3 coins), sell Millbrook (6 coins) = 3 profit/slot
3. **[Lumber]** - Large (2 slots), buy Thornwood (4 coins), sell Crossbridge (8 coins) = 2 profit/slot
4. **[Fish]** - Standard (1 slot), buy Crossbridge (3 coins), sell Millbrook (6 coins) = 3 profit/slot
5. **[Pottery]** - Large (2 slots), buy Millbrook (5 coins), sell Thornwood (9 coins) = 2 profit/slot
6. **[Cloth]** - Standard (1 slot), buy Millbrook (4 coins), sell Crossbridge (6 coins) = 2 profit/slot

## STRATEGIC TENSION DESIGN

### Core Mathematical Impossibilities
**Equipment Slots (3) vs Optimal Loadout (5+)**
- All 3 equipment types needed for maximum route flexibility = 3 slots
- High-value trade opportunity (2 Large items) = 4 slots  
- **Mathematical Impossibility**: 3 + 4 = 7 slots needed, only 4 available
- Cart adds +3 slots but blocks mountain/forest routes (eliminates 4 of 8 routes)

### Route Network Constraints
**8 Routes but Equipment Dependencies Create Strategic Choices**
- Without [Climbing]: Only 4 of 8 routes available (Main Road, Logging Road, Trade Highway, Forest Trail)
- Without [Navigation]: Only 4 of 8 routes available (Main Road, Logging Road, Trade Highway, Mountain Pass)  
- With Cart: 3 cart-compatible routes but cannot access fast mountain/forest shortcuts
- **Strategic Dilemma**: Equipment specialization vs route flexibility vs cargo capacity

### Stamina Management Pressure
**10 Stamina vs 12+ Daily Demands**
- Travel costs: 1 stamina per period (2-4 stamina for typical routes)
- Work costs: 2 stamina per period (4-6 stamina for daily activities)
- Equipment commissioning: 2 stamina (1 period work)
- Information gathering: 2 stamina (1 period work)
- **Result**: Cannot do everything without stamina management and rest periods

### Information vs Income Tension
- **Tavern visits** provide route/price information but cost time + money
- **Workshop visits** provide crafting opportunities but require [Trade_Tools]
- **Market visits** provide immediate trade profits but consume carrying capacity
- **Cannot optimize everything simultaneously**

## POC CHALLENGE DESIGN

### Day 1 Breadcrumb: Simple Delivery Contract
**Starting Conditions**: 
- Location: Millbrook
- Equipment: [Trade_Tools] only
- Inventory: 4 slots
- Money: 12 coins
- Stamina: 10/10

**Initial Contract Available**: Deliver 1 [Grain] to Crossbridge by end of day (payment: 3 coins)
- Teaches: basic travel, route options, stamina costs
- [Grain] available from Market_Trader for 2 coins
- Route options: Main Road (2 periods, cart-compatible) vs Mountain Pass (1 period, requires [Climbing])

**Natural Discovery Path**: Player realizes Mountain Pass is faster but blocked, leading to workshop discovery and equipment commissioning

### Open Sandbox Challenge: "Make 50 Coins in 14 Days"
**Victory Condition**: Accumulate 50 coins within 14 days

**Strategic Dimensions**:
1. **Route Mastery**: Learn which routes require which equipment, discover time-saving paths
2. **Trade Optimization**: Identify profitable trade circuits, balance cargo size vs profit margins  
3. **Equipment Investment**: Commission gear that unlocks route access vs immediate trade goods
4. **Contract Strategy**: Balance guaranteed contract income vs speculative trading profits
5. **Stamina Management**: Balance work intensity with rest needs and travel demands

**Discovery Through Experimentation**:
- Which routes require which equipment (discovered through route blocking messages)
- Which trade goods are profitable where (learned through market price observation)
- Which equipment enables which strategic options (discovered through route access changes)
- How to balance time between equipment investment vs income generation (learned through resource pressure)

### Failure States That Teach
**Equipment Poverty Spiral**: Player focuses only on trading, never invests in route equipment, gets stuck using slow routes, cannot compete with time-sensitive opportunities

**Information Starvation**: Player avoids NPC interactions to save time/money, misses route discoveries and price intelligence, makes suboptimal decisions

**Overspecialization Trap**: Player invests heavily in one equipment type, gets locked out of alternative strategies when conditions change

**Cart Dependency**: Player relies on cart transport for cargo capacity, becomes unable to access profitable mountain/forest routes when cart-compatible routes become congested or expensive

## SUCCESS METRICS FOR POC

### Strategic Depth Indicators
1. **Multiple Valid Strategies**: Players find 3+ different approaches to same challenge
2. **Trade-off Recognition**: Players understand they can't optimize everything
3. **Planning Horizon**: Players consider consequences 2-3 decisions ahead
4. **Failure Recovery**: Poor choices create problems, but recovery strategies exist

### Optimization Puzzle Validation
1. **No Dominant Strategy**: Different equipment loadouts viable for different goals
2. **Scheduling Tension**: Players regularly face impossible time requirements
3. **Resource Allocation**: Inventory decisions affect strategic options
4. **Information Value**: Route/price knowledge creates genuine advantages

### Player Experience Goals
1. **"Clever" Moments**: Players discover equipment combos that solve multiple problems
2. **"Impossible" Choices**: Face decisions where all options have significant costs
3. **Learning Curve**: Understanding system interactions improves performance
4. **Emergent Strategy**: Players develop personal approaches through experimentation

## VALIDATION SCENARIOS

### Test 1: Equipment Specialization
- Can player succeed focusing only on [Climbing] routes?
- Does [Navigation] specialization create viable alternative strategy?
- Are [Trade_Tools] valuable enough to justify inventory space?

### Test 2: Contract Prioritization  
- Do [Rush] contracts create enough pressure to justify risk?
- Can player succeed avoiding time pressure entirely?
- Does contract mixing create optimal income vs risk balance?

### Test 3: Information Investment
- Is tavern information gathering worth the time/money cost?
- Do players develop reliable information sources?
- Does route knowledge compound to create strategic advantages?

### Test 4: Transport Optimization
- Do players find cart vs walking optimal balance points?
- Does scheduled transport create meaningful planning constraints?
- Are dangerous routes worth the equipment investment?

## IMPLEMENTATION REQUIREMENTS

### JSON Content Structure
All entities must be defined in JSON files with proper categorical relationships:
- **locations.json**: 3 locations with defined spots and NPC assignments
- **routes.json**: 8 routes with equipment requirements and terrain categories
- **items.json**: 6 trade goods with size categories and equipment definitions
- **contracts.json**: Contract templates for 4 types with categorical requirements
- **npcs.json**: 9 NPCs with location assignments and contract offerings

### Categorical System Implementation
- **EquipmentCategory**: Climbing, Navigation, Trade_Tools
- **TerrainCategory**: Requires_Climbing, Requires_Navigation, Cart_Compatible
- **ItemSize**: Standard (1 slot), Large (2 slots)
- **ContractType**: Rush, Standard, Craft, Exploration
- **TransportMethod**: Walking, Cart

### Mathematical Constraint Validation
- Inventory system must enforce slot limits and transport bonuses
- Route system must block access based on equipment requirements
- Stamina system must create impossible daily scheduling scenarios
- Contract system must create time pressure that conflicts with equipment investment

## DESIGN VALIDATION CHECKLIST

- [ ] **Mathematical Impossibilities Confirmed**: 7 slots needed vs 4 available creates genuine constraints
- [ ] **Route Network Creates Strategic Choices**: Equipment specialization vs flexibility tradeoffs exist
- [ ] **Stamina Creates Scheduling Pressure**: 10 stamina vs 12+ daily demands forces rest periods
- [ ] **Contract System Creates Time Pressure**: Rush contracts conflict with equipment investment time
- [ ] **Discovery System Works**: Route blocking teaches equipment requirements naturally
- [ ] **Trade Circuits Viable**: Multiple profitable trade routes exist with different risk/reward profiles
- [ ] **No Dominant Strategy**: Different equipment loadouts enable different viable strategies

The POC should demonstrate that even with minimal content, the categorical prerequisite system creates genuine optimization challenges where success requires strategic thinking, planning, and trade-off recognition rather than following predetermined optimal paths.