# Wayfarer Testing Strategy

## Single E2E Test Approach

We use ONE comprehensive E2E test (`E2E.Test.cs`) that catches ALL startup and runtime issues.

### Why One Test?

1. **Simplicity** - No complex test infrastructure needed
2. **Coverage** - Tests the exact same code path as starting the game
3. **Fast Feedback** - Run one command to know if the game works

### What the E2E Test Validates

1. **GameWorld Creation** - Catches initialization errors like missing player location
2. **Web Server Startup** - Verifies the application can serve HTTP requests  
3. **Critical Services** - Ensures all key services can be created without circular dependencies

### Running the Test

```bash
dotnet run --project RunE2ETest.csproj
```

### Example Output - Catching Errors

```
TEST 1: GameWorld Creation
✗ FAIL: CRITICAL: Player location initialization failed. CurrentLocation and CurrentLocationSpot must never be null.
```

This is the EXACT error that would appear when starting the game in browser.

### When to Run

- **Before ANY commit** - If test passes, game will start
- **After refactoring** - Catches breaking changes immediately
- **Before deployment** - Final safety check

### Test Maintenance

The test is designed to be maintenance-free. It tests the actual startup path, so if the test passes, the game WILL start successfully.