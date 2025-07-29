# Wayfarer E2E Test Suite

This directory contains comprehensive end-to-end tests for the Wayfarer game, testing all major workflows through the GameFacade interface.

## Test Structure

### Core Tests

1. **E2E.Test.cs** - Quick validation test that catches startup/initialization issues
2. **FastE2ETestSuite.cs** - Fast in-process tests using GameFacade directly (no HTTP)
3. **ComprehensiveE2ETestSuite.cs** - Full HTTP endpoint tests using TutorialTestController
4. **ComprehensiveTutorialTest.cs** - Tutorial system integration tests
5. **FocusedWorkflowTests.cs** - Specific workflow tests for debugging

### Test Coverage

The test suite covers:
- Game initialization and startup
- Tutorial auto-start and progression
- Letter queue operations (accept, deliver, skip, purge)
- Travel between locations with different routes
- NPC interactions and token earning
- Market operations (buy/sell items)
- Rest and stamina management
- Stamina collapse scenarios
- Day advancement and morning activities
- Save/load game state mechanisms
- Narrative progression
- Error recovery and invalid operations

## Running Tests

### Quick Test Run
```bash
# Run the original quick validation test
dotnet run

# Or use the shell script
./run-tutorial-test.sh
```

### Full Test Suite
```bash
# Run all default tests (fast + tutorial)
./run-all-tests.sh

# Run specific test sets
./run-all-tests.sh quick         # Quick validation only
./run-all-tests.sh fast          # Fast in-process tests
./run-all-tests.sh http          # HTTP endpoint tests
./run-all-tests.sh tutorial      # Tutorial integration
./run-all-tests.sh comprehensive # All tests including HTTP
```

### Individual Test Suites
```bash
# Run specific test suite directly
dotnet run -- Program=FastE2ETestSuite
dotnet run -- Program=ComprehensiveE2ETestSuite
dotnet run -- Program=ComprehensiveTutorialTest
```

### Focused Workflow Tests
```bash
# Test specific workflows
dotnet run -- Program=FocusedWorkflowTests letter-delivery
dotnet run -- Program=FocusedWorkflowTests stamina-collapse
dotnet run -- Program=FocusedWorkflowTests token-purge
```

## Test Design Principles

1. **Use GameFacade Interface** - All tests go through the IGameFacade interface, which is the same interface used by the UI
2. **Fast Execution** - Default tests run in-process for speed
3. **Comprehensive Coverage** - Tests cover happy paths, edge cases, and error scenarios
4. **Clear Output** - Tests provide detailed progress and error information
5. **Isolated Tests** - Each test suite can run independently

## Test Types

### Fast Tests (Default)
- Run in-process using GameFacade directly
- Use in-memory storage for speed
- Complete in seconds
- Good for rapid development feedback

### HTTP Tests (Optional)
- Start real web server on port 5013
- Test actual HTTP endpoints
- Slower but more realistic
- Good for integration testing

### Workflow Tests
- Focus on specific user journeys
- Useful for debugging specific features
- Can be run individually

## Adding New Tests

To add new tests:

1. Add test methods to existing suites for related functionality
2. Create new test classes for major new features
3. Update Program.cs to route to new test classes
4. Follow the pattern of using IGameFacade for all operations

## Troubleshooting

### Port Already in Use
If you get "port 5013 already in use" errors:
```bash
# Find and kill the process using port 5013
lsof -i :5013
kill -9 [PID]
```

### Test Failures
- Check logs in `/logs` directory
- Run focused workflow tests to debug specific features
- Use quick validation test to check basic setup

### Content Validation Errors
If you see content validation errors:
- Check the JSON files in Content/Templates
- Look for missing references or typos
- The E2E test will report specific validation issues