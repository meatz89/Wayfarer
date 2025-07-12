# GAME ARCHITECTURE FINDINGS

This document captures critical architectural discoveries and patterns that must be maintained for system stability and design consistency.

## CORE ARCHITECTURAL PATTERNS

### **NO FUNC/ACTION/PREDICATE DELEGATES - CONCRETE TYPES ONLY**

**FUNDAMENTAL PRINCIPLE**: Main application code must never use `Func<>`, `Action<>`, `Predicate<>` or similar delegate types. Use concrete interfaces and classes for maintainability, testability, and clarity.

**Architectural Rationale**:
- **Maintainability**: Named interfaces are self-documenting and searchable
- **Testability**: Concrete interfaces can be easily mocked and stubbed
- **Clarity**: Intention is explicit rather than hidden in lambda expressions
- **Refactoring**: IDE tools work better with concrete types than delegates
- **Debugging**: Stack traces show concrete type names instead of generated delegate code

**Implementation Pattern**:
```csharp
// ❌ FORBIDDEN: Using delegates in main code
public List<RouteOption> FilterRoutes(Func<RouteOption, bool> predicate)
{
    return routes.Where(predicate).ToList();
}

// ✅ CORRECT: Use concrete interface
public interface IRouteValidator
{
    bool IsValid(RouteOption route);
}

public List<RouteOption> FilterRoutes(IRouteValidator validator)
{
    return routes.Where(route => validator.IsValid(route)).ToList();
}
```

**Allowed Exceptions**:
- **Test Files**: Builder patterns and test setup may use delegates for convenience
- **LINQ Methods**: Built-in LINQ operations like `.Where()`, `.Select()` are acceptable
- **Event Handlers**: UI event handlers may use delegates when required by framework

**Enforcement**:
- Code reviews must catch delegate usage in main application code
- Refactor existing delegate usage to concrete interfaces during maintenance
- Create specific, named interfaces for each functional requirement

### **SYNCHRONOUS EXECUTION MODEL - NO CONCURRENCY**

**FUNDAMENTAL ARCHITECTURE**: The game uses a purely synchronous execution model with no background operations, timers, or event-driven patterns.

**Critical Discovery**: Tests were failing due to assumptions about timing and concurrency that don't exist in our architecture. The game executes linearly - when a method is called, it completes fully before returning.

**Architectural Principles**:
1. **No Background Tasks** - Everything executes in the calling thread
2. **No Timers or Scheduling** - Game time advances only through explicit method calls
3. **No Event Bus** - Direct method invocation only, no decoupled messaging
4. **No Concurrent State Access** - Single-threaded model eliminates race conditions
5. **Async/Await for I/O Only** - And always immediately awaited

**Testing Implications**:
```csharp
// ✅ CORRECT: Tests can assume immediate, complete execution
var result = manager.ExecuteAction();
Assert.Equal(expected, result); // No timing issues possible

// ❌ WRONG: Never needed in our architecture
await Task.Delay(100); // NO - nothing runs in background
await WaitForEventCompletion(); // NO - no events to wait for
```

**Debugging Benefits**:
- **Linear execution flow** - Stack traces show complete call chain
- **Predictable state changes** - No concurrent modifications
- **Reproducible behavior** - Same inputs always produce same outputs
- **Simple test setup** - No need for synchronization or mocking of time

**Exceptions to Synchronous Model**:
1. **Blazor UI Polling**:
   - Blazor Server components poll GameWorld state because Blazor doesn't support push notifications
   - This is purely a UI rendering concern - domain logic remains synchronous
   
2. **AI Service Integration** (Future):
   - Long-running AI prompts will be awaited asynchronously to avoid blocking the UI
   - This is an infrastructure boundary concern, not core game logic
   - Game mechanics continue to execute synchronously before and after AI calls
   - **Current Status**: Not implemented - focusing on non-AI game mechanics first

### **TIME SYSTEM ARCHITECTURE - SINGLE TIME SOURCE**

**FUNDAMENTAL DESIGN**: The game uses a single, linear time progression system where all time tracking derives from one authoritative time value.

**Critical Architecture Principles**:

1. **Single Time Authority**: `TimeManager.CurrentTimeHours` is the ONLY authoritative time value
   - All other time representations derive from this value
   - `TimeBlocks` enum (Morning/Afternoon/Evening/Night) calculated from hours, not stored separately
   - No separate tracking of "time blocks consumed" - this is calculated from time progression

2. **Time Blocks Are Internal Mechanics Only**:
   - "Time blocks" represent action point consumption, not UI display concepts  
   - Players see actual time progression: "Morning 6:00" → "Afternoon 14:00"
   - UI should NEVER show "time blocks remaining (2/5)" - this violates player mental model

3. **Time Progression Pattern**:
   ```csharp
   // ✅ CORRECT: Actions advance actual time
   timeManager.ConsumeTimeBlock(1); // Advances CurrentTimeHours by calculated amount
   // Result: "Morning 6:00" becomes "Morning 9:00" or "Afternoon 12:00"
   
   // ❌ WRONG: Separate time block tracking
   usedTimeBlocks++; // This disconnects from actual time progression
   ```

4. **Five Time Blocks System**:
   The game divides each day into exactly 5 time blocks that correspond to natural time periods:
   
   ```csharp
   // ✅ CORRECT: 5 Time Blocks mapped to 24-hour day
   public TimeBlocks GetCurrentTimeBlock() {
       return CurrentTimeHours switch {
           >= 6 and < 9 => TimeBlocks.Dawn,      // 6:00-8:59 (3 hours)
           >= 9 and < 12 => TimeBlocks.Morning,   // 9:00-11:59 (3 hours) 
           >= 12 and < 16 => TimeBlocks.Afternoon, // 12:00-15:59 (4 hours)
           >= 16 and < 20 => TimeBlocks.Evening,   // 16:00-19:59 (4 hours)
           >= 20 or < 6 => TimeBlocks.Night,      // 20:00-5:59 (10 hours)
           _ => TimeBlocks.Night
       };
   }
   ```
   
   **Critical Design Notes**:
   - Exactly 5 time blocks per day (MaxDailyTimeBlocks = 5)
   - Each action typically consumes 1 time block = ~3.6 hours of game time
   - Night is longest period (10 hours) for rest and recovery
   - Dawn/Morning are shorter active periods (3 hours each)
   - Afternoon/Evening are medium active periods (4 hours each)

**Time System Violations to Prevent**:
- ❌ Displaying "time blocks remaining" in UI
- ❌ Separate tracking of time blocks vs actual time
- ❌ TimeBlocks enum stored as separate state
- ❌ Actions that consume time blocks without advancing clock

### **TRAVEL SYSTEM ARCHITECTURE - ROUTES ARE TRANSPORT METHODS**

**FUNDAMENTAL DESIGN**: Each route defines exactly one transport method. Routes ARE the transport selection, not a separate layer.

**Critical Architecture Principles**:

1. **Routes Define Transport Methods**:
   - "Walking Path" = walking transport method
   - "Standard Cart" = cart transport method  
   - "Express Coach" = premium carriage transport method
   - Each route has exactly one `method` field in routes.json

2. **No Separate Transport Selection**:
   - Player chooses route: "Walking Path" or "Standard Cart" or "Express Coach"
   - This IS the transport selection - no additional layer needed
   - UI shows route names with their inherent transport characteristics

3. **Route Selection Pattern**:
   ```csharp
   // ✅ CORRECT: Routes contain all transport information
   var routes = GetAvailableRoutes(fromLocation, toLocation);
   // Routes: [{"name": "Walking Path", "method": "Walking"}, {"name": "Standard Cart", "method": "Carriage"}]
   
   // ❌ WRONG: Separate transport selection on top of routes
   var transports = GetAvailableTransportOptions(route); // This is redundant
   ```

4. **Travel UI Pattern**:
   ```csharp
   // ✅ CORRECT: Direct route selection
   "Choose route to Town Square:"
   - "Walking Path (0 coins, 2 stamina)" 
   - "Standard Cart (4 coins, 1 stamina)"
   - "Express Coach (8 coins, 0 stamina)"
   
   // ❌ WRONG: Double selection
   "Choose route: Walking Path" → "Choose transport: Walking/Horseback/Cart"
   ```

**Travel System Violations to Prevent**:
- ❌ `TravelMethods` enum separate from route definitions
- ❌ Transport selection UI on top of route selection
- ❌ Multiple transport options per route
- ❌ "Transport compatibility" logic separate from route access logic

### **Repository Pattern Single Source of Truth**

**CRITICAL PRINCIPLE**: All game state access MUST go through entity repositories, never through direct GameWorld property access.

```csharp
// ✅ CORRECT: Repository Pattern
public class ContractSystem 
{
    private readonly ContractRepository _contractRepository;
    
    public List<Contract> GetAvailableContracts() 
    {
        return _contractRepository.GetAllContracts()
            .Where(c => c.IsAvailable())
            .ToList();
    }
}

// ✅ CORRECT: Stateless repository - ONLY GameWorld dependency
public class ContractRepository 
{
    private readonly GameWorld _gameWorld; // ONLY allowed private field
    
    public List<Contract> GetAllContracts() 
    {
        return _gameWorld.WorldState.Contracts ?? new List<Contract>();
    }
}

// ❌ WRONG: Direct GameWorld property access
return _gameWorld.Contracts.Where(c => c.IsAvailable()).ToList();
```

**ENFORCEMENT RULES**:
1. **ONLY repositories may access GameWorld.WorldState properties**
2. **Business logic MUST use repositories, never GameWorld properties**  
3. **Tests MUST use repositories, never GameWorld properties**
4. **Repositories are completely stateless - NO caching or state storage**

### **UI → GameWorldManager Gateway Pattern**
All UI components must route actions through GameWorldManager instead of injecting managers directly.
- ✅ Correct: UI → GameWorldManager → Specific Manager
- ❌ Wrong: UI → Direct Manager Injection

### **GameWorld Single Source of Truth**
GameWorld.WorldState is the authoritative source for all game state.
- All game state changes must go through WorldState
- GameWorld contains no business logic, only state management

## DUAL INITIALIZER PATTERN FOR TESTING

**CRITICAL FINDING**: Tests require different initialization strategy than production to eliminate async complexity.

**Architecture Pattern**:
```
Production: JSON Files → GameWorldSerializer → GameWorldInitializer (async) → GameWorld → Repositories
Testing:   TestScenarioBuilder → TestGameWorldInitializer (sync) → GameWorld → Repositories
```

**Key Implementation**:
- **TestGameWorldInitializer**: Synchronous, direct object creation for deterministic test state
- **TestScenarioBuilder**: Declarative fluent API for readable test scenario definition
- **Same GameWorld Type**: Both patterns produce identical GameWorld objects for consistent behavior

**Benefits**:
- Zero async complexity in tests
- Deterministic state (same input = same output)
- Production-identical game flow execution
- No mocks required through real object initialization

### **Separate JSON Files for Tests**

**CRITICAL ARCHITECTURAL DECISION**: Tests need isolated JSON data files, not shared production files.

```
Production: src/Content/Templates/*.json
Testing:    Wayfarer.Tests/Content/Templates/*.json (copied/customized)
```

**MANDATORY PROCESS**: When changing JSON data structures in production content, test JSON files MUST be updated to match.

**Files That Must Stay in Sync**:
- `contracts.json` - Contract structure, properties, requirements
- `items.json` - Item categories, properties, pricing
- `locations.json` - Location structure and properties  
- `routes.json` - Route definitions and terrain categories
- `actions.json` - Action definitions and requirements

## CRITICAL SYSTEM DEPENDENCIES

### **Time Window System Architecture**

**CRITICAL FINDING**: Location spot availability depends on proper time window initialization.

**Root Cause**: `WorldState.CurrentTimeWindow` defaults to `TimeBlocks.Dawn` (enum value 0), but many location spots don't include "Dawn" in their time windows.

**Solution**: Always initialize `CurrentTimeWindow = TimeBlocks.Morning` in WorldState.

```csharp
// CORRECT: WorldState.cs
public TimeBlocks CurrentTimeWindow { get; set; } = TimeBlocks.Morning;

// WRONG: Allowing default enum value (Dawn)
public TimeBlocks CurrentTimeWindow { get; set; }
```

**Impact**: Without this fix, `gameWorldManager.CanMoveToSpot()` returns false for all spots, breaking UI navigation and player movement.

### **JSON Content Parsing Validation**

**CRITICAL FINDING**: Enum parsing in JSON deserializers silently fails when enum values don't match, resulting in empty collections.

**Solution**: Ensure JSON content uses exact enum value names.

```csharp
// CORRECT: TerrainCategory enum values
Requires_Climbing, Wilderness_Terrain, Exposed_Weather, Dark_Passage

// CORRECT: routes.json
"terrainCategories": ["Exposed_Weather", "Wilderness_Terrain"]

// WRONG: Invalid enum names
"terrainCategories": ["Urban_Terrain", "Mountain_Path"]
```

## ACTION SYSTEM ARCHITECTURE

### **Action Creation and Execution Flow**

The action system uses a multi-stage transformation pipeline that converts static JSON definitions into executable runtime actions when players visit location spots.

**Key Transformation**: `ActionDefinition` → `LocationAction`

**Flow**:
```
1. Player visits location spot via MoveToLocationSpot()
2. actionRepository.GetActionsForSpot() loads templates
3. ActionFactory.CreateActionFromTemplate() creates runtime actions
4. ActionProcessor.CanExecute() validates requirements
5. ActionStateTracker.SetLocationSpotActions() makes available to UI
```

### **IRequirement/IMechanicalEffect Extension Pattern**

**CRITICAL ARCHITECTURAL DISCOVERY**: The game already has perfect extensible interfaces for categorical systems.

```csharp
public interface IRequirement
{
    bool IsMet(GameWorld gameWorld);
    string GetDescription();
}

public interface IMechanicalEffect  
{
    void Apply(EncounterState state);
    string GetDescriptionForPlayer();
}
```

**CORRECT APPROACH**: Implement categorical logic as `IRequirement` and `IMechanicalEffect` implementations, not parallel systems.

**Integration Points**:
- `ActionProcessor.CanExecute()` validates all `IRequirement` implementations
- `MessageSystem.AddOutcome()` processes `IMechanicalEffect` implementations  
- `ActionPreview.razor` displays requirement descriptions to players
- `ActionFactory.CreateRequirements()` builds requirement lists from templates

## CONTRACT-ACTION INTEGRATION

### **CRITICAL DESIGN PHILOSOPHY: Basic Actions Complete Contracts**

**FUNDAMENTAL PRINCIPLE**: Contracts are completed through the same basic actions players use for normal gameplay, NOT through special contract-specific actions.

**CORE DESIGN RULE**: Contracts create **context and objectives** for basic actions, they do NOT create new action types.

### **Contracts Only Check Completion Actions, Not Process**

**FUNDAMENTAL RULE**: Contracts should ONLY check for the specific action that completes them, NOT how the player got to that point.

**Example: "Deliver Trade Goods to Millbrook" Contract**
- ✅ **ONLY CHECKS**: Sell/deliver [Silk Bolts] at [Millbrook]
- ❌ **DOES NOT CHECK**: How player acquired Silk Bolts (buy, find, craft, steal)
- ❌ **DOES NOT CHECK**: How player reached Millbrook (travel, already there)

**Why This Matters**:
1. **Player Agency**: Players can complete contracts using ANY strategy they devise
2. **Emergent Gameplay**: Creative solutions are rewarded, not blocked
3. **No Railroad**: Players aren't forced into specific sequences
4. **True Sandbox**: Every contract has multiple valid completion paths

## CATEGORICAL SYSTEMS ARCHITECTURE

### **Equipment Categories Enhancement**

**Item Categorical Dimensions**:
- `EquipmentCategory`: Climbing_Equipment, Navigation_Tools, Weather_Protection, Social_Signaling, etc.
- `Size`: Tiny → Small → Medium → Large → Massive (affects transport and inventory)
- `Fragility`: Sturdy → Standard → Delicate → Fragile (affects travel risk)
- `SocialSignaling`: Enhances/blocks social interactions based on context

### **Stamina Categorical System**

**PhysicalDemand Integration**: Hard categorical gates instead of sliding scale penalties.

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

### **Transport Compatibility System - COMPLETE ✅**

**IMPLEMENTED**: Categorical transport restrictions based on logical physical constraints.

**Transport Restriction Categories**:
- **Cart Transport**: Blocked on TerrainCategory.Requires_Climbing, TerrainCategory.Wilderness_Terrain
- **Boat Transport**: Only works on TerrainCategory.Requires_Water_Transport  
- **Heavy Equipment**: SizeCategory.Large/Massive blocks TravelMethods.Boat/Horseback
- **Water Routes**: All non-boat transport blocked on water terrain

**Architecture Pattern**:
```csharp
// CORRECT: Transport compatibility checking
public TransportCompatibilityResult CheckFullCompatibility(TravelMethods transport, RouteOption route, Player player)
{
    // Check terrain compatibility first
    TransportCompatibilityResult terrainResult = CheckTerrainCompatibility(transport, route.TerrainCategories);
    if (!terrainResult.IsCompatible) return terrainResult;
    
    // Check equipment compatibility  
    return CheckEquipmentCompatibility(transport, player);
}
```

**UI Integration**: TravelSelection.razor shows transport options with compatibility feedback.

### **Categorical Inventory Constraints System - COMPLETE ✅**

**Size-Aware Inventory Architecture**:
```csharp
// Size categories determining slot requirements
public enum SizeCategory { Tiny, Small, Medium, Large, Massive }

// Item slot calculation based on size
public int GetRequiredSlots() => Size switch {
    SizeCategory.Tiny => 1,    SizeCategory.Small => 1,   SizeCategory.Medium => 1,
    SizeCategory.Large => 2,   SizeCategory.Massive => 3, _ => 1
};

// Transport bonuses to inventory capacity
public int GetMaxSlots(TravelMethods? transport) => transport switch {
    TravelMethods.Cart => 7,      // Base 5 + 2 slots
    TravelMethods.Carriage => 6,  // Base 5 + 1 slot
    _ => 5                        // Base capacity
};
```

**Integration Architecture**:
- **TravelManager**: Provides transport-aware inventory status checking
- **MarketManager**: Uses size-aware inventory methods for purchase validation
- **UI Components**: Display slot usage, transport bonuses, and item constraints
- **TransportCompatibilityValidator**: Integrates inventory overflow checking

**Strategic Gameplay Impact**:
- Cart transport adds slots but blocks terrain access
- Large/Massive items require transport planning
- Equipment vs carrying capacity optimization
- Visual feedback for slot usage and transport bonuses

### **Contract Categorical System**

**Contract Enhancement Structure**:
- `RequiredEquipmentCategories`: List<EquipmentCategory>
- `RequiredToolCategories`: List<ToolCategory>
- `RequiredSocialStanding`: SocialRequirement
- `PhysicalRequirement`: PhysicalDemand
- `RequiredInformation`: List<InformationRequirementData>
- `Category`: ContractCategory (affects NPC relationships)
- `Priority`: ContractPriority (affects payment and reputation)
- `RiskLevel`: ContractRisk (affects failure consequences)

## VALIDATION CHECKLIST

Before implementing any system changes:

1. ✅ **Time Window Compatibility**: Ensure CurrentTimeWindow is initialized to a value that exists in location spot time windows
2. ✅ **Repository Pattern Compliance**: All state access goes through GameWorld.WorldState
3. ✅ **Enum Value Validation**: JSON content uses exact enum value names from C# enums
4. ✅ **Single Source of Truth**: No static property usage for game state
5. ✅ **Test Pattern Compliance**: Tests follow the same architectural patterns as production code

## FAILURE PATTERNS TO AVOID

1. **❌ Time Window Defaults**: Never rely on enum default values for time-sensitive systems
2. **❌ Static State Management**: Never use static properties for game state that should be instance-based
3. **❌ Silent Enum Failures**: Always validate that JSON enum values match C# enum definitions
4. **❌ Dual State Systems**: Never maintain the same data in both static and instance properties
5. **❌ Test Architecture Violations**: Never allow tests to use different patterns than production code
6. **❌ Direct GameWorld Access**: Never access GameWorld properties directly - always use repositories
7. **❌ Parallel Validation Systems**: Never build new validation systems when IRequirement/IMechanicalEffect interfaces exist

These patterns ensure system stability and prevent the cascade failures discovered during debugging sessions.