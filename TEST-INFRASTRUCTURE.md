# WAYFARER TEST INFRASTRUCTURE
**Created**: 2025-01-26
**Purpose**: PREVENT BUGS FROM REACHING PRODUCTION

## 🚨 CRITICAL TESTING PRINCIPLES

### NEVER SHIP BROKEN CODE
1. **Every commit** must pass ALL tests
2. **Every feature** must have E2E tests BEFORE marking complete
3. **Every bug fix** must have a regression test
4. **Every UI change** must have visual validation

### TEST CATEGORIES

## 1. SMOKE TESTS (Run on EVERY build)
**Time**: < 30 seconds
**Purpose**: Catch catastrophic failures instantly

```bash
# Run before ANY commit
./run-smoke-tests.sh
```

### Required Smoke Tests:
- [ ] Server starts without errors
- [ ] API responds to health check
- [ ] Game initializes without infinite loops
- [ ] Basic navigation works (Location → Queue → Travel)
- [ ] Player has starting resources (attention, coins)

## 2. CORE LOOP TESTS (Run after EVERY feature)
**Time**: < 2 minutes
**Purpose**: Verify fundamental gameplay works

### Obligation Queue Tests
```csharp
[Test: ObligationQueueNoTravelButtons]
// NEVER show "Travel to X" in obligation queue
// Only "View Details" button should exist
// This prevents confusion about where actions happen

[Test: DisplaceActionWorks]
// Clicking Displace MUST move letter down queue
// MUST show immediate visual feedback
// MUST update queue positions instantly

[Test: DeadlineTracking]
// Deadlines MUST count down with time
// Expired letters MUST move to expired section
// Critical deadlines MUST show red
```

### Conversation Tests
```csharp
[Test: QuickExchangeInstantCompletion]
// Exchange MUST complete IMMEDIATELY
// Resources MUST update INSTANTLY
// NO async delays or animations
// Player MUST see result immediately

[Test: CardPlayFeedback]
// Every card play MUST show success/failure
// Toast notification MUST appear
// Resources MUST update visibly
// Effect MUST be clear to player

[Test: ConsistentCardStyling]
// ALL cards MUST have weight indicator
// Free cards MUST show "FREE" tag
// Cost cards MUST show weight number
// NO inconsistent styling between cards

[Test: ExitConversationAlwaysAvailable]
// Exit button MUST always be visible
// Exit MUST work at any conversation stage
// Exit MUST preserve game state correctly
```

### Resource System Tests
```csharp
[Test: WorkActionGivesCoins]
// Work action MUST give exact coins
// Work MUST consume attention
// Work MUST advance time
// UI MUST update immediately

[Test: TavernRestRefreshesAttention]
// Rest MUST cost exact coins
// Rest MUST fully refresh attention
// Rest MUST show in UI immediately

[Test: ResourcesAlwaysVisible]
// Resources bar MUST show on ALL screens
// Coins/Health/Hunger/Attention MUST update
// Changes MUST be instantly visible
```

### Travel Tests
```csharp
[Test: TravelAdvancesTime]
// EVERY travel MUST advance game time
// Time MUST match route duration
// Time UI MUST update after travel
// NPCs MUST react to time passage
```

## 3. INTEGRATION TESTS (Run before release)
**Time**: < 5 minutes
**Purpose**: Verify complete flows work

### POC Flow Test
```javascript
// Complete POC walkthrough automatically
async function testPOCFlow() {
    // 1. Start at Market Square Fountain
    await validateLocation("market_square", "central_fountain");
    await validateAttention(5);
    
    // 2. Move to Merchant Row
    await clickSpot("merchant_row");
    await validateInstantMovement();
    
    // 3. Quick Exchange with Marcus
    await startQuickExchange("marcus");
    await validateExchangeCard("Help with Inventory");
    await playExchangeCard();
    await validateResources({ attention: 2, coins: 20 });
    await validateConversationEnded();
    
    // 4. Return to Fountain
    await navigateToLocation("central_fountain");
    
    // 5. Take Observation
    await takeObservation("Guards blocking north road");
    await validateAttention(1);
    await validateCardInHand("Guards blocking north road");
    
    // 6. Travel to Copper Kettle
    await initiateTravel("copper_kettle_tavern");
    await validateTravelTime(15);
    await validateTimeAdvanced(15);
    
    // 7. Move to Corner Table
    await clickSpot("corner_table");
    
    // 8. Conversation with Elena (DESPERATE)
    await startStandardConversation("elena");
    await validateEmotionalState("DESPERATE");
    await playCrisisCard();
    await validateLetterGenerated();
}
```

## 4. REGRESSION TESTS (For EVERY bug fix)

### Bug Archive Tests
```javascript
// Every bug gets a permanent test
[Test: Bug_2025_01_26_ElenaQuickExchangeCrash]
// Elena MUST have quick exchange OR
// UI MUST NOT show unavailable options
// Game MUST NOT crash on missing exchanges

[Test: Bug_2025_01_26_AsyncConversationEnd]
// Conversations MUST end synchronously
// NO delayed state updates
// NO animation delays on game logic
```

## 5. VISUAL VALIDATION TESTS

### Screenshot Comparison Tests
```javascript
async function validateUIConsistency() {
    // Take screenshots of each screen
    await screenshot("location-screen");
    await screenshot("conversation-screen");
    await screenshot("obligation-queue");
    await screenshot("travel-modal");
    
    // Compare with baseline
    await compareWithBaseline();
    
    // Flag any visual regressions
    if (differences > threshold) {
        throw new Error("Visual regression detected!");
    }
}
```

## 🔥 MANDATORY TEST EXECUTION

### Before EVERY Commit:
```bash
# Clean build
dotnet clean && dotnet build --no-incremental

# Run smoke tests
./run-smoke-tests.sh

# Run core loop tests for changed area
./run-core-tests.sh --filter "conversation"
```

### Before Claiming "Complete":
```bash
# Full test suite
./run-all-tests.sh

# Manual POC walkthrough
./test-poc-flow.sh

# Visual validation
./run-visual-tests.sh
```

### On CI/CD Pipeline:
```yaml
on: [push, pull_request]
jobs:
  test:
    steps:
      - run: ./run-smoke-tests.sh
      - run: ./run-core-tests.sh
      - run: ./run-integration-tests.sh
      - run: ./run-visual-tests.sh
```

## 🚨 TEST FAILURE PROTOCOL

1. **STOP ALL WORK** - Don't write new code with failing tests
2. **FIX IMMEDIATELY** - Broken tests = broken game
3. **ADD REGRESSION TEST** - Every bug gets a permanent test
4. **DOCUMENT FAILURE** - Update this file with new test case

## 📊 TEST METRICS TO TRACK

- **Test Coverage**: Minimum 80% for core systems
- **Test Execution Time**: Smoke < 30s, Core < 2min, Full < 5min
- **Failure Rate**: Track which tests fail most often
- **Bug Escape Rate**: How many bugs reach users?

## 🎯 SUCCESS CRITERIA

**ZERO tolerance for:**
- Infinite loops
- Resource update failures  
- Missing UI elements
- Async state corruption
- Inconsistent styling
- Broken core loops

**Every release MUST:**
- Pass 100% of tests
- Complete POC flow
- Match UI mockups
- Update resources instantly
- Provide clear feedback