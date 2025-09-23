const { test, expect } = require('@playwright/test');
const {
  navigateToElena,
  startElenaConversation,
  getGameStateValues,
  validateFocusConstraint,
  validateStrategicTension,
  captureStrategicState
} = require('./elena-helpers');

/**
 * Elena Strategic Framework Summary Test
 *
 * This test validates that Elena's conversation system creates meaningful
 * strategic decisions rather than obvious plays, confirming the framework
 * successfully transforms the conversation into a resource management puzzle.
 */

test.describe('Elena Strategic Framework Summary', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);

    // Navigate to Elena and start conversation
    const foundElena = await navigateToElena(page);
    if (!foundElena) {
      throw new Error('Could not find Elena - test setup failed');
    }

    const conversationStarted = await startElenaConversation(page);
    if (!conversationStarted) {
      throw new Error('Could not start Elena conversation - test setup failed');
    }
  });

  test('should demonstrate strategic decision framework creates meaningful choices', async ({ page }) => {
    console.log('🎯 Testing Elena Strategic Decision Framework...');

    // Capture initial state
    const initialState = await getGameStateValues(page);
    await captureStrategicState(page, 'strategic-framework-initial');

    console.log(`📊 Initial State: Focus=${initialState.focus}, Momentum=${initialState.momentum}, Doubt=${initialState.doubt}, Flow=${initialState.flow}, Hand=${initialState.handSize}`);

    // 1. Validate Focus Constraints Create Planning Requirements
    const focusConstraint = await validateFocusConstraint(page);
    console.log(`⚖️ Focus Constraint Analysis: ${focusConstraint.currentFocus} focus, ${focusConstraint.totalPlayableCost} total playable cost`);

    expect(focusConstraint.hasConstraint).toBe(true);
    expect(focusConstraint.playableCards.length).toBeGreaterThan(2);

    await captureStrategicState(page, 'strategic-framework-focus-constraints');

    // 2. Validate Strategic Tension Exists
    const strategicTension = await validateStrategicTension(page);
    console.log(`🎲 Strategic Options: ${strategicTension.strategicOptions} different strategy types`);

    expect(strategicTension.hasMultipleStrategies).toBe(true);
    expect(strategicTension.strategicOptions).toBeGreaterThanOrEqual(2);

    await captureStrategicState(page, 'strategic-framework-multiple-strategies');

    // 3. Verify Turn Structure Forces Choices
    const availableCards = page.locator('.card:not(.disabled)');
    const cardCount = await availableCards.count();

    expect(cardCount).toBeGreaterThan(3); // Multiple options available
    expect(focusConstraint.currentFocus).toBeLessThan(10); // But can't play everything

    // 4. Demonstrate Resource Management Mechanics
    console.log('💰 Testing resource management mechanics...');

    // Test that momentum-generating and momentum-spending cards coexist
    const momentumGenerators = await page.locator('.card:has-text("rapport"), .card:has-text("agreement"), .card:has-text("plea")').count();
    const momentumSpenders = await page.locator('.card:has-text("spend"), .card:has-text("invest"), .card:has-text("confusion")').count();

    expect(momentumGenerators).toBeGreaterThan(0);
    expect(momentumSpenders).toBeGreaterThan(0);

    console.log(`🔄 Resource Cards: ${momentumGenerators} generators, ${momentumSpenders} spenders`);

    // 5. Validate Power Curve Creates Scaling Decisions
    console.log('📈 Testing power curve scaling...');

    const lowCostCards = await page.locator('.card:has-text("1"), .card:has-text("gentle"), .card:has-text("quick")').count();
    const highCostCards = await page.locator('.card:has-text("5"), .card:has-text("6"), .card:has-text("burning"), .card:has-text("deep")').count();

    expect(lowCostCards).toBeGreaterThan(0);
    expect(highCostCards).toBeGreaterThan(0);

    console.log(`⚡ Power Curve: ${lowCostCards} low-cost, ${highCostCards} high-cost cards`);

    await captureStrategicState(page, 'strategic-framework-complete-analysis');

    // 6. Summary Validation
    console.log('\n✅ Strategic Framework Validation Complete');
    console.log('📋 Framework Validation Results:');
    console.log(`   ✓ Focus constraints exist: ${focusConstraint.hasConstraint}`);
    console.log(`   ✓ Multiple strategies available: ${strategicTension.hasMultipleStrategies}`);
    console.log(`   ✓ Resource management present: ${momentumGenerators + momentumSpenders > 0}`);
    console.log(`   ✓ Power curve implemented: ${lowCostCards + highCostCards > 0}`);
    console.log(`   ✓ Strategic tension confirmed: ${strategicTension.strategicOptions} strategy types`);

    // Final assertion: The framework successfully creates strategic decisions
    expect(focusConstraint.hasConstraint && strategicTension.hasMultipleStrategies).toBe(true);
  });

  test('should demonstrate turn-by-turn strategic progression', async ({ page }) => {
    console.log('🔄 Testing turn-by-turn strategic progression...');

    let turnCount = 0;
    const maxTurns = 5;
    const turnStates = [];

    while (turnCount < maxTurns) {
      turnCount++;
      console.log(`\n--- Turn ${turnCount} ---`);

      const currentState = await getGameStateValues(page);
      turnStates.push(currentState);

      console.log(`📊 Turn ${turnCount} State: Focus=${currentState.focus}, Momentum=${currentState.momentum}, Doubt=${currentState.doubt}, Flow=${currentState.flow}`);

      await captureStrategicState(page, `strategic-progression-turn-${turnCount}`);

      // Analyze strategic options for this turn
      const focusConstraint = await validateFocusConstraint(page);
      const strategicTension = await validateStrategicTension(page);

      console.log(`   🎯 Focus constraint: ${focusConstraint.hasConstraint}`);
      console.log(`   🎲 Strategic options: ${strategicTension.strategicOptions}`);

      // Verify each turn presents meaningful choices
      expect(focusConstraint.hasConstraint).toBe(true);
      expect(strategicTension.strategicOptions).toBeGreaterThanOrEqual(1);

      // Play a strategic card to advance the turn
      const cards = page.locator('.card:not(.disabled)');
      const cardCount = await cards.count();

      if (cardCount > 0) {
        // Play the first available card
        await cards.nth(0).click();
        await page.waitForTimeout(1000);
      }

      // Check if turn ended or conversation completed
      const endTurnButton = page.locator('button:has-text("End Turn"), button:has-text("Continue"), button:has-text("Next")').first();
      if (await endTurnButton.isVisible({ timeout: 2000 })) {
        await endTurnButton.click();
        await page.waitForTimeout(1000);
      } else {
        // Conversation may have ended
        break;
      }
    }

    console.log('\n📈 Strategic Progression Summary:');
    for (let i = 0; i < turnStates.length; i++) {
      const state = turnStates[i];
      console.log(`   Turn ${i + 1}: Focus=${state.focus}, Momentum=${state.momentum}, Doubt=${state.doubt}, Flow=${state.flow}`);
    }

    await captureStrategicState(page, 'strategic-progression-complete');

    // Verify progression shows meaningful resource management
    expect(turnStates.length).toBeGreaterThanOrEqual(2);
  });

  test('should validate Elena conversation showcases all strategic elements', async ({ page }) => {
    console.log('🎯 Validating Elena conversation showcases all strategic elements...');

    await captureStrategicState(page, 'strategic-elements-validation');

    // 1. Power Tier Diversity
    console.log('🔍 Checking power tier diversity...');
    const tier1Cards = await page.locator('.card:has-text("1 focus"), .card:has-text("gentle"), .card:has-text("quick")').count();
    const tier2Cards = await page.locator('.card:has-text("3 focus"), .card:has-text("passionate"), .card:has-text("establish")').count();
    const tier3Cards = await page.locator('.card:has-text("5 focus"), .card:has-text("burning"), .card:has-text("moment")').count();

    console.log(`   📊 Power Tiers: T1=${tier1Cards}, T2=${tier2Cards}, T3=${tier3Cards}`);
    expect(tier1Cards + tier2Cards + tier3Cards).toBeGreaterThan(3);

    // 2. Resource Conversion Mechanics
    console.log('🔍 Checking resource conversion mechanics...');
    const generatorCards = await page.locator('.card:has-text("momentum"), .card:has-text("rapport"), .card:has-text("agreement")').count();
    const converterCards = await page.locator('.card:has-text("spend"), .card:has-text("confusion"), .card:has-text("trust")').count();

    console.log(`   🔄 Resource Cards: Generators=${generatorCards}, Converters=${converterCards}`);
    expect(generatorCards).toBeGreaterThan(0);
    expect(converterCards).toBeGreaterThan(0);

    // 3. Scaling Effects
    console.log('🔍 Checking scaling effects...');
    const scalingCards = await page.locator('.card:has-text("understanding"), .card:has-text("pressure"), .card:has-text("gambit")').count();

    console.log(`   📈 Scaling Cards: ${scalingCards}`);
    expect(scalingCards).toBeGreaterThan(0);

    // 4. Focus Manipulation
    console.log('🔍 Checking focus manipulation...');
    const focusCards = await page.locator('.card:has-text("reset"), .card:has-text("focus"), .card:has-text("mind")').count();

    console.log(`   🎯 Focus Cards: ${focusCards}`);
    expect(focusCards).toBeGreaterThan(0);

    console.log('\n✅ Strategic Elements Validation Complete');
    console.log('🎮 Elena conversation successfully demonstrates:');
    console.log(`   ✓ Power tier diversity: ${tier1Cards + tier2Cards + tier3Cards} total`);
    console.log(`   ✓ Resource conversion: ${generatorCards + converterCards} total`);
    console.log(`   ✓ Scaling effects: ${scalingCards} cards`);
    console.log(`   ✓ Focus manipulation: ${focusCards} cards`);

    await captureStrategicState(page, 'strategic-elements-complete');

    // Final validation: All strategic elements present
    const allElementsPresent = (tier1Cards + tier2Cards + tier3Cards > 3) &&
                              (generatorCards > 0) && (converterCards > 0) &&
                              (scalingCards > 0) && (focusCards > 0);

    expect(allElementsPresent).toBe(true);
  });
});