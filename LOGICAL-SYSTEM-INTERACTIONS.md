# LOGICAL SYSTEM INTERACTIONS

**CRITICAL DESIGN GUIDELINES - MANDATORY FOR ALL IMPLEMENTATIONS**

This document defines the core principle that replaces arbitrary mathematical modifiers with logical system interactions in Wayfarer. All game mechanics must emerge from entity category relationships, not hardcoded bonuses or penalties.

## FUNDAMENTAL DESIGN PRINCIPLE

### **Entity Category-Based System Interactions**

**CORE RULE: Always prefer system interdependencies over single mechanic modifiers**

✅ **CORRECT: Logical Category Interactions**
```csharp
// Weather + Terrain + Equipment interactions create emergent constraints
if (weather == WeatherCondition.Rain && 
    terrain == TerrainCategory.Exposed_Weather && 
    !playerEquipment.Contains(EquipmentCategory.Weather_Protection))
    return RouteAccessResult.Blocked("Rain makes exposed terrain unsafe without protection");
```

❌ **WRONG: Arbitrary Mathematical Modifiers**
```csharp
// Arbitrary math that doesn't involve system relationships
efficiency *= weather == WeatherCondition.Rain ? 0.8f : 1.0f;
staminaCost = (int)(baseCost * efficiency);
```

## IMPLEMENTED CATEGORICAL SYSTEMS ✅

### **Transport Compatibility System - COMPLETE ✅**

**IMPLEMENTED**: Categorical transport restrictions based on logical physical constraints.

**Transport Restriction Rules**:
- **Cart Transport**: Blocked on mountain/wilderness terrain (TerrainCategory.Requires_Climbing, Wilderness_Terrain)
- **Boat Transport**: Only works on water routes (TerrainCategory.Requires_Water_Transport)
- **Heavy Equipment**: Large/Massive items block boat and horseback transport
- **Water Routes**: All non-boat transport blocked on water terrain

**Player Strategic Decisions Created**:
- Equipment loadout vs transport efficiency trade-offs
- Route planning based on transport compatibility
- Inventory management affecting transport options

### **Categorical Inventory Constraints System - COMPLETE ✅**

**IMPLEMENTED**: Size-based inventory system with transport bonuses creating strategic loadout decisions.

**Inventory Size Categories**:
- **Tiny/Small/Medium**: 1 slot each (standard items)
- **Large**: 2 slots (bulky equipment, blocks some transport)
- **Massive**: 3 slots (major cargo, requires transport planning)

**Transport Inventory Bonuses**:
- **Base Inventory**: 5 slots (walking capacity)
- **Cart Transport**: +2 slots but blocks mountain/wilderness routes
- **Carriage Transport**: +1 slot with route flexibility
- **Walking/Horseback/Boat**: Use base capacity

**Player Strategic Decisions Created**:
- Equipment vs carrying capacity trade-offs
- Transport choice affecting available inventory space
- Size-aware item acquisition planning
- Multi-item strategic loadout decisions

### **Period-Based Activity Planning System - COMPLETE ✅**

**IMPLEMENTED**: Time-based categorical scheduling that creates strategic activity pressure and choice conflicts.

**NPC Scheduling Categories**:
- **Schedule.Morning**: Available Morning timeblock only
- **Schedule.Afternoon**: Available Afternoon timeblock only  
- **Schedule.Evening**: Available Evening timeblock only
- **Schedule.Market_Days**: Available Morning/Afternoon (most traders)
- **Schedule.Always**: Available all timeblocks (innkeepers, guards)

**Transport Departure Scheduling**:
- **RouteOption.DepartureTime**: Specific timeblock departure restrictions
- **Express services**: Morning departures for time-efficient travel
- **Regular services**: Multiple departure times throughout day
- **Seasonal/weather-dependent**: Conditional scheduling based on conditions

**Time Block Strategic Pressure**:
- **Daily Limit**: 5 time blocks per day maximum
- **Activity Consumption**: Every action (travel, trading, contracts) consumes time
- **Scheduling Conflicts**: NPC availability vs transport schedules vs contract deadlines
- **Resource vs Time Trade-offs**: Fast transport costs more but saves time

**Player Strategic Decisions Created**:
- **Morning Planning**: Which NPCs to visit during their availability windows
- **Transport Timing**: Coordinating departure schedules with destination NPC availability
- **Contract Deadlines**: Managing time pressure vs thorough preparation
- **Activity Prioritization**: Limited daily actions force strategic choices

**Implementation Details**:
```csharp
// NPC availability checking enforces scheduling
bool isMarketOpen = _npcRepository.GetNPCsForLocationAndTime(locationId, currentTime)
    .Where(npc => npc.CanProvideService(ServiceTypes.Trade))
    .Any();

// Transport schedules restrict route availability  
List<RouteOption> availableRoutes = routes
    .Where(r => !r.DepartureTime.HasValue || r.DepartureTime.Value == currentTime)
    .ToList();

// Time blocks create daily activity pressure
if (timeManager.RemainingTimeBlocks == 0)
    throw new InvalidOperationException("No time blocks remaining for today");
```

### **Contract Categorical System - COMPLETE**

**IMPLEMENTED**: Contracts with comprehensive categorical requirements for strategic planning.

#### **Contract Requirement Categories ✅**
- **Equipment Categories**: Climbing_Equipment, Navigation_Tools, Weather_Protection, Social_Signaling
- **Tool Categories**: Specialized_Equipment, Quality_Materials, Trade_Samples, Documentation
- **Social Requirements**: Commoner, Merchant_Class, Professional, Minor_Noble, Major_Noble
- **Physical Demands**: None, Light, Moderate, Heavy, Extreme (with stamina thresholds)
- **Information Requirements**: Categorical type, quality, and freshness prerequisites
- **Knowledge Requirements**: Basic, Professional, Advanced, Expert, Master levels

#### **Contract Category Interactions ✅**
```csharp
// CORRECT: Multiple categorical prerequisites creating strategic complexity
var explorationContract = new Contract {
    RequiredEquipmentCategories = { EquipmentCategory.Navigation_Tools, EquipmentCategory.Weather_Protection },
    RequiredSocialStanding = SocialRequirement.Professional,
    PhysicalRequirement = PhysicalDemand.Heavy,
    RequiredInformation = { new InformationRequirementData(InformationType.Route_Conditions, InformationQuality.Verified) },
    Category = ContractCategory.Exploration,
    Priority = ContractPriority.High,
    RiskLevel = ContractRisk.Moderate
};

// Contract validation provides detailed strategic information
ContractAccessResult result = contract.GetAccessResult(player, currentLocation);
// Returns: CanAccept, CanComplete, AcceptanceBlockers, CompletionBlockers, MissingRequirements
```

### **Physical Demand Categorical System - COMPLETE**

**IMPLEMENTED**: Hard categorical gates replacing arbitrary mathematical penalties.

#### **Stamina Categorical Gates ✅**
```csharp
// CORRECT: Hard categorical thresholds
public bool CanPerformStaminaAction(PhysicalDemand demand) =>
    demand switch {
        PhysicalDemand.None => true,
        PhysicalDemand.Light => Stamina >= 2,
        PhysicalDemand.Moderate => Stamina >= 4,
        PhysicalDemand.Heavy => Stamina >= 6,
        PhysicalDemand.Extreme => Stamina >= 8,
        _ => false
    };

// WRONG: Sliding scale penalties
// efficiency = Stamina / MaxStamina; // FORBIDDEN PATTERN
```

#### **Recovery Based on Demand Categories ✅**
```csharp
// CORRECT: Recovery based on activity category
private int GetPhysicalRecoveryAmount(PhysicalDemand demand) =>
    demand switch {
        PhysicalDemand.None => 2,      // Rest activities provide recovery
        PhysicalDemand.Light => 1,     // Light activity with some recovery
        PhysicalDemand.Moderate => 0,  // No recovery during moderate work
        PhysicalDemand.Heavy => 0,     // No recovery during heavy work
        PhysicalDemand.Extreme => 0,   // No recovery during extreme exertion
        _ => 0
    };
```

## ENTITY CATEGORIZATION SYSTEM

### **Items (Multiple Categories Per Item) - ENHANCED ✅**
- **EquipmentCategory**: [Climbing_Equipment, Weather_Protection, Navigation_Tools, Social_Signaling, Permission_Documents]
- **ToolCategory**: [Specialized_Equipment, Quality_Materials, Trade_Samples, Documentation, Measurement_Tools]
- **Size**: [Tiny, Small, Medium, Large, Massive] (affects transport and inventory slots)

### **Routes (Multiple Categories Per Route)**
- **TerrainCategory**: [Requires_Climbing, Wilderness_Terrain, Exposed_Weather, Dark_Passage, Requires_Water_Transport, Requires_Permission]
- **Difficulty**: [Easy, Moderate, Challenging, Extreme]
- **Access_Level**: [Public, Private, Guild_Only, Noble_Only]
- **Traffic**: [Busy, Moderate, Quiet, Abandoned]

### **Locations (Multiple Categories Per Location)**
- **Function**: [Commerce, Crafting, Social, Official, Residential]
- **Access_Level**: [Public, Semi_Private, Private, Restricted]
- **Social_Expectation**: [Any, Merchant_Class, Noble_Class, Professional]
- **Service_Type**: [Trade, Repair, Information, Lodging, Authority]

### **NPCs (Multiple Categories Per NPC)**
- **Social_Class**: [Commoner, Merchant, Craftsman, Minor_Noble, Major_Noble]
- **Profession**: [Trader, Smith, Guard, Official, Farmer, Guide]
- **Schedule**: [Morning, Afternoon, Evening, Always, Market_Days]
- **Relationship**: [Helpful, Neutral, Wary, Hostile]

## LOGICAL INTERACTION RULES

### **Equipment-Terrain Interactions**
1. **TerrainCategory.Requires_Climbing** + No EquipmentCategory.Climbing_Equipment = Route Blocked
2. **TerrainCategory.Wilderness_Terrain** + No EquipmentCategory.Navigation_Tools = Route Blocked (in fog/snow)
3. **TerrainCategory.Exposed_Weather** + Weather.Rain + No EquipmentCategory.Weather_Protection = Route Blocked
4. **TerrainCategory.Dark_Passage** + No EquipmentCategory.Navigation_Tools = Route Blocked
5. **TerrainCategory.Requires_Water_Transport** + No EquipmentCategory.Water_Transport = Route Blocked
6. **TerrainCategory.Requires_Permission** + No EquipmentCategory.Permission_Documents = Route Blocked

### **Weather-Terrain Interactions**
1. **Weather.Rain** + TerrainCategory.Exposed_Weather = Requires Weather_Protection
2. **Weather.Snow** + TerrainCategory.Wilderness_Terrain = Requires Navigation_Tools
3. **Weather.Fog** + TerrainCategory.Wilderness_Terrain = Requires Navigation_Tools
4. **Weather.Clear** = No additional equipment requirements

### **Social-Access Interactions**
1. **Location.Social_Expectation.Noble_Class** + Player.Social_Signal.Commoner = Entry Denied
2. **NPC.Social_Class.Noble** + Player.Social_Signal.Commoner = Limited Services
3. **Location.Access_Level.Private** + NPC.Relationship.Not_Helpful = Access Refused
4. **NPC.Profession.Trader** + NPC.Relationship.Helpful = Shares Market Information

### **Size-Transport Interactions**
1. **Item.Size.Large** + TravelMethods.Boat/Horseback = Transport Blocked (too bulky)
2. **Item.Size.Massive** + No Transport = Cannot carry (requires transport)
3. **TravelMethods.Cart** = +2 inventory slots but blocks mountain routes
4. **TravelMethods.Carriage** = +1 inventory slot with route flexibility
5. **Base Inventory** = 5 slots, expandable only through transport bonuses

## IMPLEMENTATION REQUIREMENTS

### **1. All Entities Must Have Categories**
Every entity (items, routes, locations, NPCs) must belong to meaningful categories that can interact with other system categories.

### **2. Game Rules Emerge from Category Relationships**
Instead of hardcoded bonuses/penalties, create logical relationships between categories:
- Weather + Terrain → Access requirements
- Equipment + Terrain → Capability enablement  
- NPC Profession + Location Type → Service availability
- Time + NPC Schedule → Social interaction windows

### **3. Constraints Require Multiple Systems**
No single system should create arbitrary restrictions:
- ✅ Good: "Mountain routes need climbing gear, but only accessible in good weather, and guides are only available on market days"
- ❌ Bad: "Mountain routes cost +50% stamina"

### **4. Categories Enable Discovery Gameplay**
Players learn system relationships through experimentation:
- Trying to travel in fog without navigation tools → blocked → learn navigation tools enable fog travel
- Attempting to trade with nobles without proper attire → blocked → learn social categories matter
- Weather changes block previously accessible routes → learn weather-terrain interactions

### **5. All Categories Must Be Visible in UI**
For players to formulate strategies, they must see and understand the categories that influence game rules:
- Items must display their EquipmentCategory and ItemCategory
- Routes must show their TerrainCategory and requirements
- Locations should indicate their access requirements
- NPCs should reveal their profession and social categories
- Weather conditions and terrain effects must be discoverable

## VALIDATION CHECKLIST

Before implementing any game mechanic, verify:

1. ✅ **Logical Justification**: Can you explain the constraint using real-world logic?
2. ✅ **Category-Based**: Does the rule involve interactions between entity categories?
3. ✅ **No Arbitrary Math**: Are you avoiding percentage bonuses or efficiency multipliers?
4. ✅ **Multiple Systems**: Does the constraint involve at least 2 different systems?
5. ✅ **Player Visibility**: Can players see and understand the categories involved?
6. ✅ **Discovery Gameplay**: Will players learn these relationships through experimentation?

## CORE DESIGN RULES

- **NEVER** use arbitrary mathematical modifiers (efficiency multipliers, percentage bonuses, etc.)
- **ALWAYS** implement logical blocking/enabling instead of sliding scale penalties
- **REQUIRE** logical justification for all constraints based on system interactions
- **ENSURE** all entity categories are visible and understandable in the UI
- **VALIDATE** all designs against the logical interaction checklist

## PLAYER EXPERIENCE TARGET

**Instead of**: "I can't use this route because my climbing skill isn't high enough"
**Target**: "I can't use this route because it goes through mountain terrain and I don't have climbing equipment"

**Instead of**: "This NPC won't talk to me because my reputation is too low"  
**Target**: "This noble won't see me because I'm not dressed appropriately for their social class"

**Instead of**: "I can't carry this item because it's too heavy"
**Target**: "I can carry this massive item, but it takes up 2 inventory slots and I'll need a cart if I want to travel efficiently"

Every constraint emerges from **logical categorical connections** that players intuitively understand, creating complex strategic experiences through simple, obvious rules rather than hidden mathematical systems.