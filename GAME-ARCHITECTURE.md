# GAME ARCHITECTURE FINDINGS

This document captures critical architectural discoveries and patterns found during development that must be maintained for system stability and design consistency.

## CRITICAL SYSTEM DEPENDENCIES

### **Time Window System Architecture**

**CRITICAL FINDING**: Location spot availability depends on proper time window initialization.

**Root Cause**: `WorldState.CurrentTimeWindow` defaults to `TimeBlocks.Dawn` (enum value 0), but many location spots don't include "Dawn" in their time windows, causing them to be marked as closed by `LocationPropertyManager.SetClosed()`.

**Solution**: Always initialize `CurrentTimeWindow = TimeBlocks.Morning` in WorldState.

```csharp
// CORRECT: WorldState.cs
public TimeBlocks CurrentTimeWindow { get; set; } = TimeBlocks.Morning;

// WRONG: Allowing default enum value (Dawn)
public TimeBlocks CurrentTimeWindow { get; set; }
```

**Impact**: Without this fix, `gameWorldManager.CanMoveToSpot()` returns false for all spots, breaking UI navigation and player movement.

---

### **Repository Pattern Single Source of Truth**

**CRITICAL ARCHITECTURAL PRINCIPLE**: All game state access MUST go through entity repositories, never through direct GameWorld property access.

**Root Cause**: Direct access to `gameWorld.Contracts`, `gameWorld.Items`, etc. breaks encapsulation and makes testing/mocking impossible.

**MANDATORY PATTERN**: Repository-Mediated Access Only

```csharp
// ✅ CORRECT: Repository Pattern - ALL access through repositories
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

// ✅ CORRECT: Repository implementation - STATELESS, GameWorld delegation only
public class ContractRepository 
{
    private readonly GameWorld _gameWorld; // ONLY dependency - NO state storage
    
    public List<Contract> GetAllContracts() 
    {
        return _gameWorld.WorldState.Contracts ?? new List<Contract>(); // Direct GameWorld delegation
    }
    
    // CRITICAL: Repository has NO private fields for data storage
    // ALL data comes from GameWorld on every method call
}

// ❌ WRONG: Direct GameWorld property access
public class ContractSystem 
{
    private readonly GameWorld _gameWorld;
    
    public List<Contract> GetAvailableContracts() 
    {
        return _gameWorld.Contracts.Where(c => c.IsAvailable()).ToList(); // VIOLATES ARCHITECTURE
    }
}

// ❌ WRONG: Test accessing GameWorld properties directly  
Assert.NotEmpty(gameWorld.Contracts); // VIOLATES ARCHITECTURE

// ✅ CORRECT: Test using repository
Assert.NotEmpty(contractRepository.GetAllContracts());
```

**ARCHITECTURAL ENFORCEMENT RULES**:

1. **ONLY repositories may access GameWorld.WorldState properties**
2. **Business logic MUST use repositories, never GameWorld properties**  
3. **Tests MUST use repositories, never GameWorld properties**
4. **UI components MUST use repositories, never GameWorld properties**
5. **GameWorld properties exist ONLY for repository implementation**

**Impact**: This pattern ensures proper separation of concerns, enables testing with mocks, and prevents tight coupling between business logic and data storage.

---

### **Repository Statelessness Requirement**

**CRITICAL PRINCIPLE**: Repositories MUST be completely stateless and only delegate to GameWorld - NO data storage or caching allowed.

**MANDATORY REPOSITORY PATTERN**:

```csharp
// ✅ CORRECT: Stateless repository - only GameWorld dependency
public class ItemRepository 
{
    private readonly GameWorld _gameWorld; // ONLY allowed private field
    
    public ItemRepository(GameWorld gameWorld) 
    {
        _gameWorld = gameWorld;
    }
    
    public List<Item> GetAllItems() 
    {
        return _gameWorld.WorldState.Items ?? new List<Item>(); // Direct delegation every call
    }
    
    public Item GetItemById(string id) 
    {
        return GetAllItems().FirstOrDefault(i => i.Id == id); // Uses delegation method
    }
}

// ❌ WRONG: Repository with state storage or caching
public class ItemRepository 
{
    private readonly GameWorld _gameWorld;
    private List<Item> _cachedItems; // VIOLATES STATELESS PRINCIPLE
    private Dictionary<string, Item> _itemCache; // VIOLATES STATELESS PRINCIPLE
    
    public List<Item> GetAllItems() 
    {
        if (_cachedItems == null) // WRONG - no caching allowed
            _cachedItems = _gameWorld.WorldState.Items;
        return _cachedItems;
    }
}
```

**ENFORCEMENT RULES**:
1. **Repository classes may ONLY have `private readonly GameWorld _gameWorld` field**
2. **NO caching, NO state storage, NO private collections**
3. **Every method call must access GameWorld.WorldState directly**
4. **Repositories are pure delegation layers to GameWorld**
5. **If caching is needed, implement it in GameWorld, not repositories**

**Rationale**: Stateless repositories ensure that all game state changes are immediately visible to all components, prevent stale data issues, and maintain GameWorld as the true single source of truth.

---

### **JSON Content Parsing Validation**

**CRITICAL FINDING**: Enum parsing in JSON deserializers silently fails when enum values don't match, resulting in empty collections.

**Root Cause**: `RouteOptionParser.ParseRouteOption()` uses `Enum.TryParse<TerrainCategory>()` which returns false for invalid enum names, skipping the terrain category addition.

**Solution**: Ensure JSON content uses exact enum value names.

```csharp
// CORRECT: TerrainCategory enum values
Requires_Climbing, Wilderness_Terrain, Exposed_Weather, Dark_Passage

// CORRECT: routes.json
"terrainCategories": ["Exposed_Weather", "Wilderness_Terrain"]

// WRONG: Invalid enum names
"terrainCategories": ["Urban_Terrain", "Mountain_Path"]
```

**Impact**: Invalid enum names result in routes with empty terrain categories, breaking logical blocking system tests and UI terrain requirement displays.

---

## ARCHITECTURAL PATTERNS

### **UI → GameWorldManager Gateway Pattern**
All UI components must route actions through GameWorldManager instead of injecting managers directly.
- ✅ Correct: UI → GameWorldManager → Specific Manager
- ❌ Wrong: UI → Direct Manager Injection

### **Stateless Repositories** 
Repositories must be stateless and access GameWorld.WorldState dynamically.
- ✅ Correct: `private readonly GameWorld _gameWorld` + `_gameWorld.WorldState`
- ❌ Wrong: `private WorldState` caching

### **GameWorld Single Source of Truth**
GameWorld.WorldState is the authoritative source for all game state.
- All game state changes must go through WorldState
- GameWorld contains no business logic, only state management

---

## TEST ARCHITECTURE REQUIREMENTS

### **Repository Pattern in Tests**

**CRITICAL FINDING**: Tests must follow the same architectural patterns as production code.

```csharp
// CORRECT: Test using repository pattern
Assert.NotEmpty(gameWorld.WorldState.Contracts);
var contract = gameWorld.WorldState.Contracts.FirstOrDefault();

// WRONG: Test using legacy static properties
Assert.NotEmpty(GameWorld.AllContracts);
var contract = GameWorld.AllContracts.FirstOrDefault();
```

**Impact**: Tests that violate architectural patterns will fail when the production code is properly refactored to follow the patterns.

---

## CONTENT LOADING PATTERNS

### **GameWorldInitializer Responsibilities**

**CRITICAL FINDING**: GameWorldInitializer must properly populate WorldState collections, not static properties.

```csharp
// CORRECT: Loading contracts
foreach (Contract contract in contracts)
{
    gameWorld.WorldState.Contracts.Add(contract);
}

// WRONG: Legacy static assignment
GameWorld.AllContracts = contracts;
```

---

## LOGICAL SYSTEM INTERACTIONS

### **Equipment-Terrain-Weather Dependencies**

**CRITICAL FINDING**: Location spot closure system depends on time window compatibility, creating complex system interdependencies.

**Pattern**: `LocationPropertyManager.SetClosed()` sets `spot.IsClosed = !spot.TimeWindows.Contains(timeWindow)`

**Dependencies**:
1. WorldState.CurrentTimeWindow must be properly initialized
2. Location spots must have appropriate TimeWindows arrays
3. GameWorldManager.CanMoveToSpot() depends on IsClosed state

**Impact**: This creates a chain dependency where improper time initialization breaks the entire location navigation system.

---

## VALIDATION CHECKLIST

Before implementing any system changes:

1. ✅ **Time Window Compatibility**: Ensure CurrentTimeWindow is initialized to a value that exists in location spot time windows
2. ✅ **Repository Pattern Compliance**: All state access goes through GameWorld.WorldState
3. ✅ **Enum Value Validation**: JSON content uses exact enum value names from C# enums
4. ✅ **Single Source of Truth**: No static property usage for game state
5. ✅ **Test Pattern Compliance**: Tests follow the same architectural patterns as production code

---

## FAILURE PATTERNS TO AVOID

1. **❌ Time Window Defaults**: Never rely on enum default values for time-sensitive systems
2. **❌ Static State Management**: Never use static properties for game state that should be instance-based
3. **❌ Silent Enum Failures**: Always validate that JSON enum values match C# enum definitions
4. **❌ Dual State Systems**: Never maintain the same data in both static and instance properties
5. **❌ Test Architecture Violations**: Never allow tests to use different patterns than production code

These patterns ensure system stability and prevent the cascade failures discovered during this debugging session.

---

## ADDITIONAL FINDINGS - SESSION 2025-07-10

### **Test Suite Repository Pattern Compliance**

**CRITICAL FINDING**: All test suites in the codebase were using legacy static properties instead of proper Repository pattern access.

**Root Cause**: Tests in `EconomicGameInitializationTests` and `GameInitializationFlowTests` were checking `GameWorld.AllContracts` instead of `gameWorld.WorldState.Contracts`.

**Files Fixed**:
- `/mnt/c/git/wayfarer/Wayfarer.Tests/EconomicGameInitializationTests.cs` - 4 instances
- `/mnt/c/git/wayfarer/Wayfarer.Tests/GameInitializationFlowTests.cs` - 3 instances  
- `/mnt/c/git/wayfarer/Wayfarer.Tests/ComprehensiveGameInitializationTests.cs` - 2 instances

**Pattern**:
```csharp
// WRONG: Test violating architectural patterns
Assert.NotEmpty(GameWorld.AllContracts);
foreach (Contract contract in GameWorld.AllContracts)

// CORRECT: Test following Repository pattern
Assert.NotEmpty(gameWorld.WorldState.Contracts);
foreach (Contract contract in gameWorld.WorldState.Contracts)
```

**Impact**: When production code properly follows Repository patterns but tests don't, tests fail even when the production code is correct. This creates a false negative test scenario that wastes debugging time.

### **Systematic Pattern Violation Detection**

**LESSON LEARNED**: When fixing architectural violations, check ALL test files that might be using the same anti-pattern.

**Validation Process**:
1. Fix production code to follow architectural patterns
2. Run tests to identify which test files are using legacy patterns
3. Systematically update all test files to use the same patterns as production code
4. Verify 0 test failures after all fixes

**Commands Used**:
```bash
# Find all static property usage in tests
grep -r "GameWorld.AllContracts" /mnt/c/git/wayfarer/Wayfarer.Tests/ --include="*.cs"

# Replace with proper Repository pattern
gameWorld.WorldState.Contracts
```

This systematic approach reduced total failing tests from 20+ to 0 across all test suites.