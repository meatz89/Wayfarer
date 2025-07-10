# LOGICAL SYSTEM INTERACTIONS - Design Guidelines for Wayfarer

**Core Principle**: Replace arbitrary mathematical modifiers with logical system interactions where constraints emerge naturally from distinct systems interacting with each other.

## THE FUNDAMENTAL PROBLEM WITH ARBITRARY MATH

**❌ WRONG APPROACH: Sliding Scale Modifiers**
```csharp
// This is still arbitrary math!
efficiency *= playerEquipment.Contains(EquipmentCategory.Navigation_Tools) ? 0.7f : 1.3f;
efficiency *= (1.0f + weatherModifier / 10.0f);
staminaCost = (int)(baseCost * efficiency);
```

**✅ CORRECT APPROACH: Logical Blocking/Enabling**
```csharp
// Equipment either enables access or blocks it entirely
if (terrain == TerrainCategory.Requires_Climbing && !hasClimbingGear)
    return RouteAccessResult.Blocked("No climbing equipment");

// Weather + terrain interactions have logical consequences
if (weather == WeatherCondition.Rain && terrain == TerrainCategory.Mountain_Pass && !hasWeatherGear)
    return RouteAccessResult.Blocked("Mountain passes unsafe in rain without weather protection");
```

## SYSTEM INTERACTION PRINCIPLES

### 1. EQUIPMENT ENABLES/DISABLES, NEVER MODIFIES

**Equipment provides capabilities, not stat bonuses.**

**❌ Wrong**: "Climbing gear gives +2 mountain efficiency"
**✅ Right**: "Climbing gear enables access to mountain routes"

**❌ Wrong**: "Navigation tools reduce stamina cost by 30%"
**✅ Right**: "Navigation tools prevent getting lost in wilderness routes"

### 2. TERRAIN HAS LOGICAL REQUIREMENTS

**Routes have inherent challenges that require specific equipment.**

```csharp
public enum TerrainCategory
{
    // Hard Requirements (blocks access without equipment)
    Requires_Climbing,      // Steep terrain needs climbing gear
    Requires_Water_Transport, // River crossing needs boat/raft
    Requires_Permission,    // Official routes need papers/passes
    
    // Conditional Requirements (weather/time dependent)
    Exposed_Weather,        // Dangerous in storms without protection
    Wilderness_Terrain,     // Easy to get lost without navigation
    Dark_Passage,          // Dangerous at night without light
}
```

### 3. WEATHER AFFECTS TERRAIN LOGICALLY

**Weather doesn't apply universal penalties - it interacts with specific terrain types.**

```csharp
// Weather + Terrain Matrix
Rain + Mountain_Pass = Requires weather protection OR blocked
Rain + Forest_Path = Requires navigation tools OR higher stamina cost
Snow + Any_Route = Time block cost +1 (slower travel)
Storm + Exposed_Weather = Completely blocked regardless of equipment
```

### 4. TRANSPORT HAS PHYSICAL LIMITATIONS

**Transport types have realistic capabilities, not arbitrary efficiency differences.**

```csharp
public enum TransportCompatibility
{
    Walking:   // All terrain types, but slower and limited cargo
    Horseback: // Roads + gentle terrain only, moderate cargo
    Carriage:  // Roads only, high cargo capacity, scheduled departures
}

// Physical logic, not mathematical modifiers
if (transport == TransportType.Carriage && terrain.Contains(TerrainCategory.Wilderness))
    return "Carriages cannot access wilderness routes";
```

### 5. NPC SCHEDULES CREATE TIME CONFLICTS

**NPCs have realistic availability that creates strategic planning challenges.**

```csharp
public class NPCSchedule
{
    public Dictionary<TimeBlocks, string> Locations; // Where they are when
    public List<DayOfWeek> AvailableDays;           // Market days, etc.
    public Dictionary<string, int> RelationshipRequirements; // Who you need to know
}

// Examples:
// Magnus the mountaineer: Evenings at tavern, sells climbing gear
// Lady Meredith: Court hours only, requires formal attire to access
// River Captain: Dawn departures only, weather permitting
```

### 6. INFORMATION FLOWS THROUGH SOCIAL NETWORKS

**Knowledge is a resource controlled by NPCs, not automatically revealed.**

```csharp
public class InformationNetwork
{
    public Dictionary<string, NPCKnowledge> NPCInformation;
    public Dictionary<string, List<string>> RelationshipWebs;
    
    // Information spreads over time through relationships
    // Players must build networks to access knowledge
    // No automated "discovery" systems
}
```

## IMPLEMENTATION GUIDELINES

### REPLACE EFFICIENCY SYSTEMS

**Current Problem**: RouteOption.CalculateEfficiency() applies arbitrary multipliers

**Solution**: Replace with logical access checks:
```csharp
public RouteAccessResult CanAccess(Player player, WeatherCondition weather)
{
    // Check hard requirements first
    foreach (var requirement in TerrainCategories)
    {
        if (!player.HasEquipmentFor(requirement))
            return RouteAccessResult.Blocked($"Requires {requirement.GetEquipmentName()}");
    }
    
    // Check weather interactions
    var weatherBlocking = CheckWeatherTerrainInteraction(weather);
    if (weatherBlocking.IsBlocked && !player.HasWeatherProtection())
        return weatherBlocking;
    
    return RouteAccessResult.Allowed();
}
```

### ELIMINATE ARBITRARY MATH

**Remove these patterns entirely:**
- Efficiency multipliers (0.7x, 1.3x, etc.)
- Percentage bonuses/penalties
- Sliding scale modifiers
- Mathematical "difficulty" calculations

**Replace with logical states:**
- Enabled/Disabled
- Available/Unavailable  
- Accessible/Blocked
- Known/Unknown

### CREATE MEANINGFUL CHOICES THROUGH SCARCITY

**Instead of making things expensive, create conflicting opportunities:**

```csharp
// ❌ Wrong: "You can't afford this"
if (player.Coins < cost) return false;

// ✅ Right: "You could afford this, but..."
var opportunity1 = "Buy climbing gear from Magnus (only available evenings)";
var opportunity2 = "Attend court gathering (evening only) to meet Lady Meredith";
var opportunity3 = "Take evening carriage (last departure of day)";
// Player must choose one, creating strategic conflict
```

## INTERACTION EXAMPLES

### MOUNTAIN DELIVERY SCENARIO

**Player Goal**: Deliver package to mountain village by tomorrow

**System Interactions**:
1. **Terrain System**: Mountain route requires climbing gear
2. **NPC System**: Magnus sells gear, available evenings only  
3. **Time System**: Currently afternoon, Magnus unavailable until evening
4. **Weather System**: Snow forecast tomorrow may require weather protection too
5. **Transport System**: Carriage can't access mountain terrain
6. **Social System**: Alternative info from fellow travelers costs reputation

**Player Experience**: "I need climbing gear from Magnus tonight, but if it snows tomorrow I'll need weather protection too. Maybe I should ask other travelers about alternative routes, but that would use up my reputation with them..."

### INFORMATION GATHERING SCENARIO

**Player Goal**: Learn profitable trade route

**System Interactions**:
1. **Social System**: Only veteran trader Magnus knows the secret route
2. **Time System**: Magnus only at tavern every third evening
3. **Relationship System**: Magnus owes player favor from previous help
4. **Information System**: Route requires special equipment details
5. **Seasonal System**: Route only viable during dry season (time pressure)

**Player Experience**: "Magnus owes me a favor, but he's only at the tavern every few days. Even if he tells me about the route, I need to understand what equipment it requires, and the dry season won't last much longer..."

## DESIGN VALIDATION CHECKLIST

Before implementing any system, ask these questions:

### ✅ LOGICAL INTERACTION CHECK
- Does this emerge from realistic system relationships?
- Would this make sense in the real world?
- Are we simulating logical cause and effect?

### ❌ ARBITRARY MATH CHECK  
- Are we applying percentage modifiers?
- Are we using "difficulty scaling"?
- Does this involve mathematical convenience rather than logical simulation?

### 🎯 PLAYER AGENCY CHECK
- Can players still attempt "bad" choices?
- Do consequences emerge naturally rather than being enforced artificially?
- Are we creating strategic conflicts rather than optimization puzzles?

### 🔗 SYSTEM INTERACTION CHECK
- Does this involve at least 2 different systems interacting?
- Do the constraints emerge from system relationships?
- Would removing any one system break the constraint naturally?

## MANDATORY IMPLEMENTATION RULES

1. **NEVER** use mathematical multipliers for game balance
2. **ALWAYS** implement enable/disable rather than modify/penalty
3. **REQUIRE** logical justification for all constraints
4. **CREATE** time conflicts through NPC scheduling
5. **IMPLEMENT** information scarcity through social networks
6. **ENSURE** all systems have internal logical consistency

---

**Remember**: If you're tempted to write `someValue *= modifier`, you're probably implementing arbitrary math. Step back and think about what logical systems should be interacting instead.