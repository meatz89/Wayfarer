# WAYFARER USER STORIES

### **Games vs Apps: Fundamental Difference**

**Games create interactive optimization puzzles for the player to solve, not automated systems that solve everything for them.**

✅ **GAME DESIGN PRINCIPLES**
- **GAMEPLAY IS IN THE PLAYER'S HEAD** - Fun comes from systems interacting in clever ways that create optimization challenges
- **DISCOVERY IS GAMEPLAY** - Players must explore, experiment, and learn to find profitable trades, efficient routes, optimal strategies
- **EMERGENT COMPLEXITY** - Simple systems (pricing, time blocks, stamina) that interact to create deep strategic decisions
- **MEANINGFUL CHOICES** - Every decision should involve sacrificing something valuable (time vs money vs stamina)
- **PLAYER AGENCY** - Players discover patterns, build mental models, develop personal strategies through exploration

❌ **APP DESIGN ANTI-PATTERNS (NEVER IMPLEMENT)**
- **NO AUTOMATED CONVENIENCES** - Don't create `GetProfitableItems()` or `GetBestRoute()` methods that solve the puzzle for the player
- **NO GAMEPLAY SHORTCUTS** - No "Trading Opportunities" UI panels that tell players exactly what to do
- **NO OPTIMIZATION AUTOMATION** - Never show "Buy herbs at town_square (4 coins) → Sell at dusty_flagon (5 coins) = 1 profit"

## PLAYER EXPERIENCE TARGET

**Instead of**: "I can't use this route because my climbing skill isn't high enough"
**Target**: "I can't use this route because it goes through [Mountain] terrain and I don't have [Climbing] equipment"

**Instead of**: "I can't carry this item because it's too heavy"
**Target**: "I can carry this [Heavy] item, but it blocks [Cart] transport and takes 2 inventory slots from my 5-slot limit"

---

## EQUIPMENT AND ROUTE ACCESS

### **Logical Equipment Requirements**

**As a player, I want route access determined by equipment categories and terrain categories, so that I can plan equipment loadouts based on logical real-world requirements.**

**Implementation Requirements:**
- [Mountain] routes require [Climbing] equipment category
- [River] routes require [Navigation] equipment category  
- [Urban] routes require [Social_Signal] equipment matching district category
- Equipment categories visible in route interface
- Clear blocking messages when requirements not met

**Player Thought Process:**
"I want to reach the mountain village. The route shows [Mountain] terrain requiring [Climbing] equipment. I have [Navigation] and [Social_Merchant] equipment but no [Climbing]. I need to visit the [Workshop] to commission [Climbing] gear, but that requires 3 days crafting time and I have a contract due in 2 days. Do I take the longer [Road] route that's cart-compatible, or delay the contract to get proper equipment?"

**Success Criteria:**
- Players understand equipment value through logical necessity
- Route planning involves equipment strategy decisions
- All terrain-equipment relationships discoverable through logical inspection

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete route-equipment integration system with logical terrain requirements and strategic equipment planning.
> 
> **Implemented:**
> - ✅ Equipment categories: Climbing_Equipment, Water_Transport, Navigation_Tools, etc. (`src/Game/MainSystem/Item.cs`)
> - ✅ Route terrain categories and blocking logic (`src/Game/MainSystem/RouteOption.cs` - CheckRouteAccess)
> - ✅ Equipment requirement validation framework (`src/Game/ActionSystem/CategoricalRequirements.cs`)
> - ✅ Route-equipment requirement validation in travel system (`src/GameState/TravelManager.cs` - GetRouteAccessInfo)
> - ✅ Equipment category display in route interface (`src/Pages/TravelSelection.razor`)
> - ✅ Hard blocking vs warning system for different equipment types
> - ✅ Weather-terrain-equipment interaction validation
> - ✅ Strategic equipment items available in game content (`src/Content/Templates/items.json`)
> - ✅ Comprehensive test coverage (`Wayfarer.Tests/LogicalEquipmentRequirementsTests.cs`)
> 
> **Evidence:** The system creates exactly the strategic equipment planning described in the user story, with logical terrain-equipment requirements driving route access and equipment acquisition decisions.

---

### **Transport Compatibility Logic**

**As a player, I want transport methods limited by logical physical constraints, so that I understand why certain combinations don't work.**

**Implementation Requirements:**
- [Cart] transport blocks [Mountain] and [Forest] terrain categories
- [Boat] transport only works on [River] routes
- [Heavy] equipment blocks [Boat] and [Caravan] transport
- Transport options show compatibility before selection
- Clear explanations for blocked combinations

**Player Thought Process:**
"I have [Heavy] [Trade_Tools] and want to take the fast [Caravan] to [Crossbridge]. The interface shows [Caravan] transport is blocked by [Heavy] equipment. I can either drop the [Trade_Tools] and lose crafting opportunities, or use [Walking] transport which costs 2 stamina and takes 2 time periods instead of 1. The [Trade_Tools] might be needed for a [Craftsman] contract worth 12 coins. Is the time savings worth losing that opportunity?"

**Success Criteria:**
- Transport limitations feel logical and understandable
- Players make strategic decisions about equipment vs transport efficiency
- No arbitrary "you can't do this" without clear logical reason

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete transport compatibility system with logical constraints and clear explanations.
> 
> **Implemented:**
> - ✅ Transport-terrain restrictions: Cart blocked by Mountain/Forest terrain (`src/Game/MainSystem/TransportCompatibilityValidator.cs`)
> - ✅ Equipment size/weight affecting transport compatibility (`src/Game/MainSystem/TransportCompatibilityValidator.cs`)
> - ✅ Boat transport limited to River routes with logical validation
> - ✅ Transport compatibility checking before route selection (`TransportOption.IsCompatible`)
> - ✅ Clear blocking reasons and warnings (`TransportOption.BlockingReason`, `TransportOption.Warnings`)
> - ✅ Comprehensive test coverage (`Wayfarer.Tests/TransportCompatibilityTests.cs`)
> 
> **Evidence:** The system provides strategic choices between equipment loadouts and transport efficiency, exactly as specified in the user story.

---

## CONTRACT AND DEADLINE SYSTEMS

### **Deadline Pressure Without Arbitrary Penalties**

**As a player, I want contract deadlines to create strategic pressure through logical consequences, not arbitrary mathematical penalties.**

**Implementation Requirements:**
- Failed contracts damage relationship with issuing [NPC_Category]
- [Hostile] relationships block future contract access from that category
- No percentage-based payment reductions
- Deadline pressure comes from opportunity cost and relationship consequences
- Contract requirements clearly state time and equipment needs

**Player Thought Process:**
"I have a [Rush] contract due tomorrow requiring [Mountain] route access. I don't have [Climbing] equipment and the [Craftsman] needs 2 days to make it. Alternative: take the [Road] route (3 time periods vs 1), but that means working tonight to earn travel money and arriving exhausted. Do I sacrifice future opportunities or present efficiency?"

**Success Criteria:**
- Deadline pressure creates strategic decisions about prioritization
- Consequences affect future opportunities, not arbitrary punishment
- Players understand relationship stakes clearly

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete deadline system with reputation consequences instead of arbitrary penalties.
> 
> **Implemented:**
> - ✅ Time-bounded contracts with `StartDay` and `DueDay` (`src/Game/MainSystem/Contract.cs`)
> - ✅ Automatic contract failure detection via `CheckForFailedContracts()` (`src/GameState/GameWorldManager.cs`)
> - ✅ Reputation system with relationship consequences (`src/Game/MainSystem/NPC.cs` - NPCRelationship)
> - ✅ No percentage-based payment reductions - full payment or failure only
> - ✅ Equipment and time requirements clearly specified in contract structure
> - ✅ Contract validation prevents impossible deadline scenarios (`src/Game/MainSystem/ContractValidationService.cs`)
> 
> **Evidence:** Failed contracts result in relationship damage that affects future opportunities, creating strategic pressure without arbitrary mathematical penalties.

---

### **Contract Complexity Through Equipment Requirements**

**As a player, I want contracts to require specific equipment categories, so that contract selection becomes part of equipment strategy.**

**Implementation Requirements:**
- [Merchant] contracts require [Trade_Tools] for optimal completion
- [Craftsman] contracts require [Workshop] access and [Trade_Tools]
- [Exploration] contracts require [Navigation] and terrain-specific equipment
- Contract requirements visible before acceptance

**Player Thought Process:**
"Three available contracts: [Merchant] contract (12 coins, requires [Trade_Tools]), [Exploration] contract (8 coins + bonuses, requires [Navigation] + [Climbing] equipment). I have [Trade_Tools] and [Navigation] but no [Climbing]. The [Merchant] contract is immediately doable. The [Exploration] contract needs 1 equipment piece. Which investment path creates the best long-term opportunities?"

**Success Criteria:**
- Equipment decisions integrate with contract strategy
- Players evaluate equipment ROI across multiple contracts
- Contract requirements drive equipment acquisition planning

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete contract-equipment integration system with strategic equipment planning.
> 
> **Implemented:**
> - ✅ Equipment category requirements for contracts (`src/Game/MainSystem/Contract.cs` - RequiredEquipmentCategories)
> - ✅ Tool category requirements (`src/Game/MainSystem/Contract.cs` - RequiredToolCategories)
> - ✅ Contract validation against player equipment (`src/Game/MainSystem/ContractValidationService.cs`)
> - ✅ Social requirement system for contract access (`src/Game/MainSystem/Contract.cs` - SocialRequirement)
> - ✅ Contract categories (Merchant, Craftsman, Exploration, etc.) with appropriate requirements
> - ✅ Equipment-contract integration in progression system (`src/Game/MainSystem/ContractProgressionService.cs`)
> 
> **Evidence:** Contract system requires specific equipment categories and validates player equipment before acceptance, driving strategic equipment acquisition decisions.

---

### **Location-Based Discovery**

**As a player, I want to discover opportunities through logical location exploration, not automated quest systems.**

**Implementation Requirements:**
- [Tavern] locations contain [Traveler] NPCs with [Route_Information]
- [Workshop] locations enable [Equipment_Commissioning] and [Craft_Opportunities]
- [Market] locations provide [Trade_Opportunities] and [Price_Information]
- Discovery requires appropriate equipment and timing

**Player Thought Process:**
"I'm in a new town and need to discover opportunities. [Tavern] visit could reveal routes but costs 2 coins and 1 time period. [Workshop] visit might have equipment or contracts but needs [Trade_Tools] for full access. [Market] visit provides trade information but only during [Morning] and [Afternoon]. I have 3 time periods and [Social_Merchant] equipment. Which locations give the best information ROI for my current situation?"

**Success Criteria:**
- Location exploration drives opportunity discovery
- Players develop location visit strategies based on equipment and goals
- No automated opportunity detection

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete location-based discovery system with equipment-gated information gathering and specialized NPCs.
> 
> **Implemented:**
> - ✅ Location categories and access control (`src/Game/MainSystem/Location.cs` - Social_Expectation, Access_Level)
> - ✅ NPC placement and professional schedules (`src/Game/MainSystem/NPC.cs` - Profession, Schedule)
> - ✅ Time-based NPC availability (`IsAvailable(TimeBlocks currentTime)`)
> - ✅ Location spot system for different areas within locations (`src/Content/LocationSpot.cs`)
> - ✅ Information repository system (`src/Content/InformationRepository.cs`)
> - ✅ Tavern route information discovery through Traveler NPCs (`src/Content/Templates/npcs.json` - traveler_magnus)
> - ✅ Workshop equipment commissioning and craft opportunities (`src/Content/Templates/actions.json` - commission_equipment, explore_craft_opportunities)
> - ✅ Market trade opportunity investigation with equipment requirements (`src/Content/Templates/actions.json` - investigate_market_opportunities)
> - ✅ Equipment-gated discovery actions requiring Trade_Samples, Documentation, and Basic_Tools
> - ✅ Information creation system with Route_Conditions and Market_Intelligence information objects
> - ✅ Workshop location spots and Master Craftsman NPC (`src/Content/Templates/location_spots.json`)
> 
> **Evidence:** The system creates exactly the strategic location exploration described, with Tavern→Route discovery, Workshop→Equipment commissioning, and Market→Trade opportunities, all requiring appropriate equipment and timing.

---

## INVENTORY AND RESOURCE MANAGEMENT

### **Categorical Inventory Constraints**

**As a player, I want inventory limitations based on logical item categories, not arbitrary carrying capacity numbers.**

**Implementation Requirements:**
- Items have size categories: [Portable] (1 slot), [Heavy] (2 slots), [Bulk] (3 slots)
- Base inventory: 5 slots
- [Cart] transport adds 2 slots but blocks [Mountain] routes
- [Heavy] items block certain transport methods
- Inventory decisions affect transport and route strategy

**Player Thought Process:**
"I found [Bulk] [Trade_Goods] worth 15 coins but they take 3 slots. I have [Climbing] equipment (1 slot), [Navigation] equipment (1 slot), and [Social_Merchant] equipment (1 slot). That's 6 slots needed but only 5 available. I could drop [Climbing] equipment but that blocks [Mountain] routes. I could use [Cart] transport for +2 slots but that limits route choices and costs 2 coins. Do I sacrifice equipment flexibility, pay transport costs, or miss the trading opportunity?"

**Success Criteria:**
- Inventory decisions create strategic equipment trade-offs
- Slot limitations drive transport and route decisions
- No arbitrary weight/strength calculations

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete categorical inventory system with logical size constraints and transport integration.
> 
> **Implemented:**
> - ✅ Size categories: Tiny, Small, Medium (1 slot), Large (2 slots), Massive (3 slots) (`src/Game/MainSystem/Item.cs`)
> - ✅ Base inventory: 5 slots exactly as specified (`src/Game/MainSystem/Player.cs` - Inventory.MaxSlots)
> - ✅ Transport bonuses: Cart (+2 slots), Carriage (+1 slot) (`src/Game/MainSystem/TransportOption.cs`)
> - ✅ Size-aware inventory checking (`src/Game/MainSystem/PlayerInventory.cs` - CanAddItem, GetRequiredSlots)
> - ✅ Transport method blocking based on item size and equipment (`src/Game/MainSystem/TransportCompatibilityValidator.cs`)
> - ✅ Strategic inventory decisions affecting route and transport choices
> 
> **Evidence:** The system creates exactly the strategic trade-offs described in the user story, with logical size categories driving transport and route decisions.

---

## TIME AND SCHEDULING SYSTEMS

### **Period-Based Activity Planning**

**As a player, I want daily scheduling based on logical time period constraints, not arbitrary action point systems.**

**Implementation Requirements:**
- Day divided into 5 periods: [Dawn], [Morning], [Afternoon], [Evening], [Night]
- Each significant activity consumes 1 period
- NPC availability tied to logical professional schedules
- Transport schedules operate on period system
- No action point regeneration or management mini-games

**Player Thought Process:**
"[Caravan] departs at [Dawn], [Market] operates [Morning] and [Afternoon]. I need to buy [Trade_Goods] at [Market], and catch [Dawn] [Caravan] tomorrow. That requires [Afternoon] market visit + [Evening] manor visit + staying overnight + [Dawn] departure. Can't do everything today - what's the optimal sequence?"

**Success Criteria:**
- Scheduling creates natural strategic pressure
- Time periods feel like logical day segments
- Players develop daily planning strategies

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete period-based time system with 5 time blocks and logical scheduling constraints.
> 
> **Implemented:**
> - ✅ Day divided into exactly 5 periods: Dawn, Morning, Afternoon, Evening, Night (`src/Game/MainSystem/TimeBlocks.cs`)
> - ✅ Time block consumption system: each activity consumes 1 period (`src/GameState/TimeManager.cs` - ConsumeTimeBlock)
> - ✅ NPC professional schedules tied to time periods (`src/Game/MainSystem/NPC.cs` - Schedule enum)
> - ✅ Transport schedules operate on period system (`src/Game/MainSystem/RouteOption.cs` - DepartureTime)
> - ✅ No action point regeneration - clean period-based progression (`src/GameState/TimeManager.cs`)
> - ✅ Time-based availability validation for NPCs and services
> 
> **Evidence:** The system creates exactly the strategic scheduling pressure described, with logical time periods driving daily planning decisions.

---

### **Transport Schedule Integration**

**As a player, I want transport schedules that create logical timing constraints, not arbitrary cooldowns.**

**Implementation Requirements:**
- [Caravan] departs [Dawn] daily from major towns
- [Boat] schedules depend on [River] conditions and [Weather]
- [Cart] available for rental during [Workshop] hours
- Missing scheduled transport means waiting until next availability
- No "speed up" options or arbitrary delay mechanics

**Player Thought Process:**
"I need to reach [Millbrook] for a contract due tomorrow [Evening]. [Caravan] leaves [Dawn] (arrives [Morning], costs 3 coins, 0 stamina). [Walking] takes [Afternoon] + [Evening] (arrives [Night], costs 0 coins, 3 stamina). Missing [Dawn] [Caravan] means [Walking] is only option, but arriving [Night] means no [Evening] work opportunities. Do I spend money for time efficiency, or accept reduced opportunities to save coins?"

**Success Criteria:**
- Transport timing drives strategic resource allocation
- Schedule conflicts create meaningful transportation decisions
- No arbitrary waiting penalties

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete transport schedule integration with weather-dependent boat schedules and logical timing constraints.
> 
> **Implemented:**
> - ✅ Transport departure times defined (`src/Game/MainSystem/RouteOption.cs` - DepartureTime)
> - ✅ Time-based transport availability checking (`src/GameState/TravelManager.cs` - GetAvailableRoutes)
> - ✅ Weather-dependent boat schedule blocking (`src/GameState/TravelManager.cs` - IsBoatScheduleAvailable)
> - ✅ No arbitrary delay mechanics - clean time-based system
> - ✅ Caravan/boat schedule enforcement with Dawn departures and weather dependency
> - ✅ Workshop hours integration with cart rental (Morning/Afternoon departures)
> - ✅ Daily schedule repetition system through TimeManager day cycling
> - ✅ Missed transport waiting mechanics - players must wait for next scheduled departure
> - ✅ River ferry routes with weather-dependent schedules (`src/Content/Templates/routes.json`)
> - ✅ Cargo barge routes for heavy equipment transport with Dawn departures only
> 
> **Evidence:** The system creates exactly the strategic timing pressure described, with Caravan→Dawn departures, Workshop→Cart rental hours, and Boat→Weather dependency, forcing strategic resource allocation decisions.

---

## LOCATION AND WORLD SYSTEMS

### **Logical Location Access**

**As a player, I want location access based on categorical prerequisites, not arbitrary unlocking systems.**

**Implementation Requirements:**
- [Workshop] areas accessible with [Trade_Tools] or [Craftsman] relationship
- Access requirements visible before attempting entry

**Player Thought Process:**
"I want to access [Guild_Hall] for exclusive [Merchant] contracts. Entry requires [Guild_Token] (costs 20 coins) or [Helpful] relationship with [Guild_Master]. Building [Guild_Master] relationship requires completing [Merchant] contracts, but I need [Guild_Hall] access to get good [Merchant] contracts. Catch-22. Alternative: pay 20 coins for immediate access, but that uses money needed for [Equipment] upgrades. Do I invest in network access or capability expansion?"

**Success Criteria:**
- Location access drives relationship and equipment investment
- Prerequisites create logical progression challenges
- No arbitrary level-gating or hidden requirements

> **✅ FULLY IMPLEMENTED**
> 
> **Current Status:** Complete location access system with categorical prerequisites and social requirements.
> 
> **Implemented:**
> - ✅ Location access levels: Public, Semi_Private, Private, Restricted (`src/Game/MainSystem/Location.cs`)
> - ✅ Social expectation requirements for location entry (`src/Game/MainSystem/Location.cs` - Social_Expectation)
> - ✅ Equipment-based access validation (`src/Game/MainSystem/LocationSystem.cs`)
> - ✅ NPC relationship-based access control (`src/Game/MainSystem/NPC.cs` - NPCRelationship)
> - ✅ Location spot system for different access areas within locations (`src/Content/LocationSpot.cs`)
> - ✅ Time-based location availability (`src/Content/LocationSpot.cs` - CurrentTimeBlocks)
> 
> **Evidence:** Location access is driven by logical categorical prerequisites (equipment, social standing, relationships) without arbitrary level-gating systems.

---

## SUCCESS METRICS AND VALIDATION

### **Strategic Depth Indicators**
- Players discuss equipment loadout strategies
- Multiple viable approaches to achieving goals
- Scheduling and resource allocation become optimization puzzles
- Equipment categories drive strategic decisions

### **Logical Consistency Validation**
- All constraints explainable through real-world logic
- Category interactions follow intuitive patterns
- No hidden mathematical modifiers affecting outcomes
- Equipment and terrain relationships obvious to players

### **Player Agency Confirmation**
- Players discover optimization strategies through experimentation
- Multiple solutions exist for most challenges
- Game systems respond to player planning and preparation
- Success comes from strategic thinking, not automated assistance

The goal is creating a game where strategic depth emerges from logical categorical interactions, making every decision feel like solving an interesting optimization puzzle rather than navigating arbitrary game restrictions.