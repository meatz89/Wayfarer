# Outstanding Test Fixes - Updated After Architectural Learnings

## Major Progress This Session ✅

### COMPLETED - Architecture-Driven Improvements:
1. **Test Isolation Principle** - Tests now use test-specific JSON data 
2. **Proper Access Patterns** - Tests follow same patterns as business logic (no test-only APIs)
3. **TimeManager Architecture** - Fixed ArchitecturalComplianceTests and TimeManagerArchitectureTests
4. **Contract Pipeline Tests** - All 4 tests now use test-specific contracts

### COMPLETED FIXES:
- ✅ TimeSystemComplianceTests (time boundaries)
- ✅ ContractCompletionTests (8 tests - contract IDs and completion logic)  
- ✅ NPCParserTests (NPC schedule enum expansion)
- ✅ ContractPipelineIntegrationTests (4 tests - test isolation applied)
- ✅ ArchitecturalComplianceTests (proper access patterns)
- ✅ TimeManagerArchitectureTests (StartNewDay expectations fixed)

## Remaining Outstanding Test Failures

Based on the last test run, these are likely still failing:

### 1. RouteConditionVariationsTests.Route_Should_Respect_TimeOfDay_Restrictions
**Issue**: Route not found in available routes collection
**Root Cause**: Test likely depends on production routes.json
**Solution Strategy**:
- Apply Test Isolation Principle: Create test-specific routes.json with time-restricted routes
- Ensure test uses TestGameWorldInitializer with test routes
- Verify route has `DepartureTime` property for time restrictions

### 2. PlayerLocationInitializationTests.PlayerLocation_ShouldSupportSystemOperations  
**Issue**: Location initialization failing
**Root Cause**: Test likely depends on production location/locationSpots JSON
**Solution Strategy**:
- Apply Test Isolation Principle: Create minimal test-specific location data
- Ensure CurrentLocation and CurrentLocationSpot are properly set in test data
- Use TestGameWorldInitializer with test location data

### 3. ContractPipelineIntegrationTests (some may still fail)
**Issue**: Missing items in test JSON (herbs, rare_materials, etc.)
**Root Cause**: Tests converted to test contracts but may need test items
**Solution Strategy**:
- Check test contracts require items that exist in test items.json
- Add missing items to Wayfarer.Tests/Content/Templates/items.json

### 4. Any remaining NPC/Market integration issues
**Issue**: NPCs may not be loaded properly in test environment
**Solution Strategy**:
- Apply Test Isolation: Create test-specific npcs.json if needed
- Verify NPCs are connected to LocationSpots correctly in test data

## Implementation Strategy

### Step 1: Run tests to identify current failures
```bash
dotnet test --no-restore --verbosity normal
```

### Step 2: Apply Test Isolation to remaining failures
For each failing test:
1. Identify what production JSON data it depends on
2. Create minimal test-specific data in `Wayfarer.Tests/Content/Templates/`
3. Ensure test uses TestGameWorldInitializer
4. Verify test validates system behavior, not data integrity

### Step 3: Follow Business Logic Patterns
- Tests should access properties the same way as GameWorldManager
- No test-specific APIs or abstractions
- Direct property access: `gameWorld.TimeManager.UsedTimeBlocks`

## Testing Pattern (Corrected)

**CORRECT Pattern for All Tests**:
```csharp
// 1. Use test-specific data
GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

// 2. Follow business logic access patterns  
int usedBlocks = gameWorld.TimeManager.UsedTimeBlocks;
bool canPerform = gameWorld.TimeManager.CanPerformTimeBlockAction;

// 3. Validate system behavior
Assert.Equal(2, usedBlocks);
Assert.True(canPerform);
```

**WRONG Pattern (Avoid)**:
```csharp
// Don't create test-only APIs
GameTimeSnapshot snapshot = gameWorld.TimeManager.GetTimeSnapshot();

// Don't use production JSON content
Contract contract = contracts.GetContract("village_herb_delivery"); // production data
```

## Key Files for Test Data Isolation

### Already Created:
- `Wayfarer.Tests/Content/Templates/contracts.json` ✅
- `Wayfarer.Tests/Content/Templates/items.json` ✅

### May Need Creation:
- `Wayfarer.Tests/Content/Templates/routes.json` (for RouteConditionVariationsTests)
- `Wayfarer.Tests/Content/Templates/locations.json` (for PlayerLocationInitializationTests)  
- `Wayfarer.Tests/Content/Templates/location_spots.json` (for PlayerLocationInitializationTests)
- `Wayfarer.Tests/Content/Templates/npcs.json` (if NPC tests fail)

## Success Criteria

All tests should:
1. ✅ Use test-specific data (no production JSON dependencies)
2. ✅ Follow business logic access patterns (no test-only APIs)
3. ✅ Validate system behavior (not data integrity)
4. ✅ Pass consistently regardless of production content changes

This approach applies the architectural learnings to systematically fix remaining test failures.