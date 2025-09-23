# Elena Conversation Dynamics E2E Tests

This test suite provides comprehensive Playwright-based end-to-end testing for Elena's desperate request conversation, focusing on momentum/doubt dynamics and strategic gameplay validation.

## Test Structure

### 1. Core System Tests (`elena-conversation-dynamics.spec.js`)

**Momentum/Doubt Pressure System:**
- Validates momentum starts at 0 and doubt starts at 0
- Tests doubt increases by 1 after each LISTEN action (doubtPerListen setting)
- Verifies momentum erosion mechanics when doubt > 0
- Confirms doubt caps at 10 and causes conversation failure
- Validates pressure through doubt accumulation creates urgency

**Turn Structure Validation:**
- Tests focus refresh during SPEAK phase
- Validates focus limits prevent "play everything" exploit
- Confirms hand persistence between turns (cards don't auto-discard)
- Verifies turn number incrementation
- Enforces proper SPEAK → LISTEN sequence

**Goal Achievement Testing:**
- Tests Elena letter request unlock at 8 momentum threshold
- Validates Elena safety request unlock at 12 momentum threshold
- Confirms "Make Elena my priority" promise card availability at 8 momentum
- Verifies conversation ends appropriately when thresholds reached

**Balance and Pacing Validation:**
- Ensures conversations complete in 5-8 turns with efficient play
- Validates meaningful pressure without being punishing
- Prevents exploitation through focus constraints
- Confirms multiple viable strategic choices each turn

**Player Experience Testing:**
- Validates meaningful choices presented each turn
- Tests strategic planning rewards over random play
- Creates tension through resource (focus) management
- Ensures no obvious "correct" plays exist

### 2. Strategic Playthroughs (`elena-strategic-playthroughs.spec.js`)

**Aggressive Momentum Strategy:**
- Tests high-momentum cards (passionate_plea, burning_conviction, moment_of_truth)
- Validates quick letter request achievement (≤4 turns)
- Balances momentum gain against doubt accumulation

**Conservative Rapport Building:**
- Tests low-risk cards (gentle_agreement, show_understanding, careful_words)
- Validates slower but sustainable progress (5-6 turns)
- Demonstrates lower doubt accumulation compared to aggressive approach

**Mixed Strategy Optimization:**
- Tests balanced approach combining momentum and regulation
- Optimizes momentum/doubt balance for efficiency
- Demonstrates strategic card ordering importance

**Safety Request Strategy:**
- Tests sustained momentum building for 12 threshold
- Validates safety request requires more effort than letter request
- Confirms higher-stakes gameplay for secondary objectives

**Failure Strategy Testing:**
- Demonstrates conversation failure through excessive doubt
- Tests momentum erosion mechanics under pressure
- Validates Devoted personality affects momentum loss (doubling effect)

**Promise Card Mechanics:**
- Tests promise card unlock at letter threshold
- Validates alternative completion path through promises
- Confirms player choice between different request types

### 3. Edge Cases and Performance (`elena-edge-cases-performance.spec.js`)

**Edge Case Handling:**
- Maximum hand size overflow protection
- Zero momentum gameplay scenarios
- Precise doubt threshold (exactly 10) behavior
- Focus constraint edge cases
- Rapid UI interaction handling
- Conversation restart state reset

**Performance Validation:**
- UI responsiveness during long conversations (average <3s per turn)
- Multiple session performance consistency
- Memory usage monitoring during extended play
- Network interruption simulation

**State Consistency:**
- Complex interaction pattern state validation
- Deterministic behavior verification
- Concurrent UI update handling
- Numerical bounds validation (momentum 0-100, doubt 0-10)

**Browser Compatibility:**
- Page reload recovery
- Network delay tolerance
- Accessibility feature validation
- Keyboard navigation support

## Required UI Test Attributes

For these tests to work, the Wayfarer UI must include the following `data-testid` attributes:

```html
<!-- Game Structure -->
<div data-testid="game-screen">
<div data-testid="conversation-content">
<div data-testid="location-content">

<!-- Navigation -->
<button data-testid="travel-button">
<div data-testid="location-copper_kettle_tavern">
<div data-testid="npc-elena">
<button data-testid="start-conversation">

<!-- Conversation State -->
<span data-testid="momentum-value">
<span data-testid="doubt-value">
<span data-testid="current-focus">
<span data-testid="max-focus">
<span data-testid="turn-number">
<span data-testid="connection-state">
<span data-testid="atmosphere">

<!-- Cards and Actions -->
<div data-testid="hand-card" data-card-id="card_id" data-focus="2" data-playable="true">
<div data-testid="selected-card">
<button data-testid="speak-button">
<button data-testid="listen-button">
<div data-testid="request-card" data-request-id="elena_letter_request">

<!-- Conversation End -->
<div data-testid="conversation-ended">
<div data-testid="end-reason">
```

## Running the Tests

### Prerequisites

1. Ensure the Wayfarer application is configured to run on `https://localhost:7298`
2. Install dependencies: `npm install`
3. Start the application separately or use the configured webServer in playwright.config.js

### Execution Commands

```bash
# Run all Elena conversation tests
npx playwright test E2ETests/

# Run specific test suite
npx playwright test E2ETests/elena-conversation-dynamics.spec.js
npx playwright test E2ETests/elena-strategic-playthroughs.spec.js
npx playwright test E2ETests/elena-edge-cases-performance.spec.js

# Run with UI mode for debugging
npx playwright test --ui

# Run with headed browser for observation
npx playwright test --headed

# Generate test report
npx playwright show-report
```

### Debug Mode

For debugging failed tests:

```bash
npx playwright test --debug
npx playwright test --headed --slowMo=1000
```

## Test Configuration

The tests are configured for:
- **Sequential execution** (fullyParallel: false) to prevent state conflicts
- **Single browser worker** to maintain game state consistency
- **60-second timeout** per test with 10-second expect timeout
- **HTTPS localhost:7298** as base URL with certificate error ignoring
- **Screenshot and video** capture on failure
- **Trace collection** on retry for debugging

## Expected Outcomes

### Successful Test Run Indicators:

1. **Momentum/Doubt System**: All pressure mechanics work as designed
2. **Turn Structure**: SPEAK/LISTEN phases enforce proper game flow
3. **Goal Achievement**: Request thresholds (8/12 momentum) trigger correctly
4. **Strategic Depth**: Multiple viable approaches produce different outcomes
5. **Balance Validation**: 5-8 turn conversations with meaningful tension
6. **Performance**: <3s average response time, consistent across sessions
7. **Edge Cases**: Graceful handling of boundary conditions

### Test Data Analysis:

The tests collect progression data showing:
- Turn-by-turn momentum/doubt changes
- Strategic choice outcomes
- Performance metrics
- State transition validation
- Balance verification

This data validates that Elena's conversation provides engaging, strategic gameplay with proper tension through the momentum/doubt pressure system.

## Integration with CI/CD

These tests can be integrated into automated pipelines to validate:
- Conversation system changes don't break core mechanics
- Balance adjustments maintain intended gameplay experience
- UI changes preserve test automation compatibility
- Performance regressions are caught early

The deterministic nature of the conversation system ensures tests provide reliable validation of game mechanics and player experience quality.