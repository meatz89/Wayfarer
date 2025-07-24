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
# Build first
dotnet build RunE2ETest.csproj

# Run the executable directly (recommended - faster, no timeout)
./bin/Debug/net8.0/RunE2ETest

# Alternative: Run with dotnet (may timeout due to server startup)
dotnet run --project RunE2ETest.csproj
```

### Example Output - Catching Errors

```
=== WAYFARER E2E TEST ===
TEST 1: GameWorld Creation
ERROR: Failed to parse location_spots.json: Content validation failed with 6 critical errors
ERROR: Failed to parse npcs.json: Content validation failed with 3 critical errors
ERROR: Failed to parse routes.json: Content validation failed with 68 critical errors
ERROR: Failed to parse letter_templates.json: Content validation failed with 80 critical errors
ERROR: Failed to parse standing_obligations.json: Content validation failed with 42 critical errors
✗ FAIL: CRITICAL: Player location initialization failed.
```

This shows the EXACT validation errors that prevent the game from starting, including counts of errors in each JSON file.

### When to Run

- **Before ANY commit** - If test passes, game will start
- **After refactoring** - Catches breaking changes immediately
- **Before deployment** - Final safety check

### Test Maintenance

The test is designed to be maintenance-free. It tests the actual startup path, so if the test passes, the game WILL start successfully.