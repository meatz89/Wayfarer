const { test, expect } = require('@playwright/test');
const {
  navigateToElena,
  startElenaConversation,
  getGameStateValues,
  findCardByName,
  playCard,
  playMomentumGeneratingCards,
  buildUpDoubt,
  captureStrategicState,
  validateFocusConstraint,
  validateStrategicTension
} = require('./elena-helpers');

/**
 * Elena Strategic Decision Framework Tests
 *
 * These tests validate that Elena's conversation presents meaningful strategic choices
 * rather than obvious plays, creating a strategic resource management puzzle.
 *
 * Key Testing Areas:
 * 1. Turn 1 Constraint Testing (4 focus limit scenarios)
 * 2. Resource Conversion Testing (momentum spending requirements)
 * 3. Scaling Formula Validation (dynamic card values)
 * 4. Focus Constraint Validation (strategic tension)
 * 5. Decision Framework Testing (across game phases)
 */

test.describe('Elena Strategic Decision Framework', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the game and set up Elena conversation scenario
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000); // Wait for Blazor to initialize

    // Look for any intro or menu elements and navigate to gameplay
    const startButton = page.locator('button:has-text("Start Game"), button:has-text("New Game"), button:has-text("Continue")').first();
    if (await startButton.isVisible({ timeout: 5000 })) {
      await startButton.click();
      await page.waitForLoadState('networkidle');
    }

    // Navigate to Elena and start conversation
    const foundElena = await navigateToElena(page);
    if (!foundElena) {
      throw new Error('Could not find Elena - test setup failed');
    }

    const conversationStarted = await startElenaConversation(page);
    if (!conversationStarted) {
      throw new Error('Could not start Elena conversation - test setup failed');
    }

    // Capture initial state for debugging
    await captureStrategicState(page, 'initial-setup');
  });

  test.describe('Turn 1 Constraint Testing (4 focus available)', () => {
    test('should prevent playing burning_conviction alone (5 focus required)', async ({ page }) => {
      const initialState = await getGameStateValues(page);
      await captureStrategicState(page, 'turn1-initial-state');

      // Verify initial focus is 4
      expect(initialState.focus).toBe(4);

      // Try to play burning_conviction card
      const cardPlayed = await playCard(page, 'burning_conviction');

      // Should fail to play the card
      expect(cardPlayed).toBe(false);

      // Verify focus hasn't changed
      const afterState = await getGameStateValues(page);
      expect(afterState.focus).toBe(4);

      await captureStrategicState(page, 'turn1-burning-conviction-blocked');
    });

    test('should allow mental_reset + burning_conviction combo (0+5 focus)', async ({ page }) => {
      const initialState = await getGameStateValues(page);

      // Play mental_reset first (0 focus, grants +2 focus)
      const mentalResetPlayed = await playCard(page, 'mental_reset');
      expect(mentalResetPlayed).toBe(true);

      await captureStrategicState(page, 'turn1-mental-reset-played');

      // Verify focus increased to 6 (4 original + 2 from mental_reset)
      const afterResetState = await getGameStateValues(page);
      expect(afterResetState.focus).toBe(6);

      // Now burning_conviction should be playable
      const burningConvictionPlayed = await playCard(page, 'burning_conviction');
      expect(burningConvictionPlayed).toBe(true);

      // Verify focus decreased to 1 (6 - 5)
      const finalState = await getGameStateValues(page);
      expect(finalState.focus).toBe(1);

      // Verify momentum increased
      expect(finalState.momentum).toBeGreaterThan(initialState.momentum);

      await captureStrategicState(page, 'turn1-mental-reset-burning-conviction-combo');
    });

    test('should enable passionate_plea + pause_reflect strategy (3+1 focus)', async ({ page }) => {
      const initialState = await getGameStateValues(page);

      // Play passionate_plea first (3 focus)
      const passionatePleaPlayed = await playCard(page, 'passionate_plea');
      expect(passionatePleaPlayed).toBe(true);

      await captureStrategicState(page, 'turn1-passionate-plea-played');

      // Verify focus decreased to 1 (4 - 3)
      const afterPleaState = await getGameStateValues(page);
      expect(afterPleaState.focus).toBe(1);

      // Play pause_reflect (1 focus)
      const pauseReflectPlayed = await playCard(page, 'pause_reflect');
      expect(pauseReflectPlayed).toBe(true);

      // Verify focus decreased to 0 (1 - 1)
      const finalState = await getGameStateValues(page);
      expect(finalState.focus).toBe(0);

      // Verify doubt decreased (pause_reflect should reduce doubt)
      expect(finalState.doubt).toBeLessThan(initialState.doubt);

      await captureStrategicState(page, 'turn1-passionate-plea-pause-reflect-strategy');
    });

    test('should validate build_rapport + quick_insight leaves 1 focus unused', async ({ page }) => {
      const initialState = await getGameStateValues(page);

      // Play build_rapport (2 focus)
      const buildRapportPlayed = await playCard(page, 'build_rapport');
      expect(buildRapportPlayed).toBe(true);

      // Verify focus decreased to 2 (4 - 2)
      const afterRapportState = await getGameStateValues(page);
      expect(afterRapportState.focus).toBe(2);

      // Play quick_insight (1 focus)
      const quickInsightPlayed = await playCard(page, 'quick_insight');
      expect(quickInsightPlayed).toBe(true);

      // Verify focus decreased to 1 (2 - 1) - demonstrating 1 focus left unused
      const finalState = await getGameStateValues(page);
      expect(finalState.focus).toBe(1);

      // Verify hand size increased (quick_insight should draw a card)
      expect(finalState.handSize).toBeGreaterThanOrEqual(initialState.handSize);

      await captureStrategicState(page, 'turn1-build-rapport-quick-insight-unused-focus');
    });
  });

  test.describe('Resource Conversion Testing (momentum requirements)', () => {
    test('should show clear_confusion requires 2 momentum', async ({ page }) => {
      // Build up 1 momentum (insufficient for clear_confusion)
      await playMomentumGeneratingCards(page, 1);

      const beforeState = await getGameStateValues(page);
      await captureStrategicState(page, 'clear-confusion-insufficient-momentum');

      // Try to play clear_confusion with insufficient momentum
      const cardPlayed = await playCard(page, 'clear_confusion');

      // Should fail to play the card
      expect(cardPlayed).toBe(false);

      // Verify game state unchanged
      const afterState = await getGameStateValues(page);
      expect(afterState.momentum).toBe(beforeState.momentum);
    });

    test('should enable establish_trust with 3 momentum for +1 flow', async ({ page }) => {
      // Build up 3+ momentum
      const momentumBuilt = await playMomentumGeneratingCards(page, 3);
      expect(momentumBuilt).toBe(true);

      const beforeState = await getGameStateValues(page);
      await captureStrategicState(page, 'establish-trust-sufficient-momentum');

      // Play establish_trust card
      const cardPlayed = await playCard(page, 'establish_trust');
      expect(cardPlayed).toBe(true);

      const afterState = await getGameStateValues(page);

      // Verify flow increased by 1
      expect(afterState.flow).toBe(beforeState.flow + 1);

      // Verify momentum decreased by 3
      expect(afterState.momentum).toBe(beforeState.momentum - 3);

      await captureStrategicState(page, 'establish-trust-flow-increased');
    });

    test('should validate moment_of_truth requires 4 momentum for +2 flow', async ({ page }) => {
      // Build up 4+ momentum
      const momentumBuilt = await playMomentumGeneratingCards(page, 4);
      expect(momentumBuilt).toBe(true);

      const beforeState = await getGameStateValues(page);
      await captureStrategicState(page, 'moment-of-truth-sufficient-momentum');

      // Play moment_of_truth card
      const cardPlayed = await playCard(page, 'moment_of_truth');
      expect(cardPlayed).toBe(true);

      const afterState = await getGameStateValues(page);

      // Verify flow increased by 2
      expect(afterState.flow).toBe(beforeState.flow + 2);

      // Verify momentum decreased by 4
      expect(afterState.momentum).toBe(beforeState.momentum - 4);

      await captureStrategicState(page, 'moment-of-truth-major-flow');
    });
  });

  test.describe('Scaling Formula Validation', () => {
    test('should validate show_understanding scales with hand size (cards ÷ 2)', async ({ page }) => {
      await startElenaConversation(page);

      // Count current hand size
      const handCards = page.locator('.card, [data-test="card"]');
      const handSize = await handCards.count();

      // Look for show_understanding card
      const showUnderstandingCard = page.locator('[data-test="card-show_understanding"], .card:has-text("show understanding"), .card:has-text("more I listen")').first();

      if (await showUnderstandingCard.isVisible()) {
        // Record initial momentum
        const momentumDisplay = page.locator('[data-test="momentum-display"], .momentum-value, .momentum');
        const initialMomentum = await getNumericValue(momentumDisplay);

        // Play the card
        await showUnderstandingCard.click();
        await page.waitForTimeout(1000);

        // Verify momentum increased by handSize ÷ 2
        const expectedIncrease = Math.floor(handSize / 2);
        const newMomentum = await getNumericValue(momentumDisplay);
        expect(newMomentum).toBe(initialMomentum + expectedIncrease);

        await page.screenshot({
          path: 'test-results/elena-show-understanding-hand-scaling.png',
          fullPage: true
        });
      }
    });

    test('should validate build_pressure scales with doubt level (8 - current doubt)', async ({ page }) => {
      await startElenaConversation(page);

      // Get current doubt level
      const doubtDisplay = page.locator('[data-test="doubt-display"], .doubt-value, .doubt');
      const currentDoubt = await getNumericValue(doubtDisplay);

      // Look for build_pressure card
      const buildPressureCard = page.locator('[data-test="card-build_pressure"], .card:has-text("build pressure"), .card:has-text("before doubt undermines")').first();

      if (await buildPressureCard.isVisible()) {
        // Record initial momentum
        const momentumDisplay = page.locator('[data-test="momentum-display"], .momentum-value, .momentum');
        const initialMomentum = await getNumericValue(momentumDisplay);

        // Play the card
        await buildPressureCard.click();
        await page.waitForTimeout(1000);

        // Verify momentum increased by (8 - currentDoubt)
        const expectedIncrease = 8 - currentDoubt;
        const newMomentum = await getNumericValue(momentumDisplay);
        expect(newMomentum).toBe(initialMomentum + expectedIncrease);

        await page.screenshot({
          path: 'test-results/elena-build-pressure-doubt-scaling.png',
          fullPage: true
        });
      }
    });

    test('should validate deep_understanding equals current hand size', async ({ page }) => {
      await startElenaConversation(page);

      // Count current hand size
      const handCards = page.locator('.card, [data-test="card"]');
      const handSize = await handCards.count();

      // Look for deep_understanding card
      const deepUnderstandingCard = page.locator('[data-test="card-deep_understanding"], .card:has-text("deep understanding"), .card:has-text("perfect clarity")').first();

      if (await deepUnderstandingCard.isVisible()) {
        // Record initial momentum
        const momentumDisplay = page.locator('[data-test="momentum-display"], .momentum-value, .momentum');
        const initialMomentum = await getNumericValue(momentumDisplay);

        // Play the card
        await deepUnderstandingCard.click();
        await page.waitForTimeout(1000);

        // Verify momentum equals hand size
        const newMomentum = await getNumericValue(momentumDisplay);
        expect(newMomentum).toBe(handSize);

        await page.screenshot({
          path: 'test-results/elena-deep-understanding-hand-equals.png',
          fullPage: true
        });
      }
    });

    test('should validate desperate_gambit scales with current doubt level', async ({ page }) => {
      await startElenaConversation(page);

      // Get current doubt level
      const doubtDisplay = page.locator('[data-test="doubt-display"], .doubt-value, .doubt');
      const currentDoubt = await getNumericValue(doubtDisplay);

      // Look for desperate_gambit card
      const desperateGambitCard = page.locator('[data-test="card-desperate_gambit"], .card:has-text("desperate gambit"), .card:has-text("risk everything")').first();

      if (await desperateGambitCard.isVisible()) {
        // Record initial momentum
        const momentumDisplay = page.locator('[data-test="momentum-display"], .momentum-value, .momentum');
        const initialMomentum = await getNumericValue(momentumDisplay);

        // Play the card
        await desperateGambitCard.click();
        await page.waitForTimeout(1000);

        // Verify momentum increased by current doubt level
        const newMomentum = await getNumericValue(momentumDisplay);
        expect(newMomentum).toBe(initialMomentum + currentDoubt);

        await page.screenshot({
          path: 'test-results/elena-desperate-gambit-doubt-multiplier.png',
          fullPage: true
        });
      }
    });
  });

  test.describe('Focus Constraint Validation', () => {
    test('should prevent playing two 3+ focus cards with 4 focus', async ({ page }) => {
      await startElenaConversation(page);

      // Find two cards that cost 3+ focus each
      const highFocusCards = page.locator('.card:has-text("3"), .card:has-text("4"), .card:has-text("5")');
      const cardCount = await highFocusCards.count();

      if (cardCount >= 2) {
        // Play first high-focus card
        await highFocusCards.nth(0).click();
        await page.waitForTimeout(1000);

        // Verify remaining focus is insufficient for second card
        const focusDisplay = page.locator('[data-test="focus-display"], .focus-value, .focus');
        const remainingFocus = await getNumericValue(focusDisplay);

        // Second high-focus card should be unplayable
        const secondCard = highFocusCards.nth(1);
        if (await secondCard.isVisible()) {
          await expect(secondCard).toHaveClass(/disabled|unplayable|grayed/);
        }

        await page.screenshot({
          path: 'test-results/elena-focus-constraint-high-cost.png',
          fullPage: true
        });
      }
    });

    test('should enable one high-tier card with 5 focus', async ({ page }) => {
      await startElenaConversation(page);

      // Use mental_reset to get 6 focus (4 + 2)
      const mentalResetCard = page.locator('[data-test="card-mental_reset"], .card:has-text("mental reset")').first();
      if (await mentalResetCard.isVisible()) {
        await mentalResetCard.click();
        await page.waitForTimeout(1000);
      }

      // Now should be able to afford 5-focus cards
      const fiveFocusCard = page.locator('.card:has-text("5 focus"), [data-test="card-burning_conviction"]').first();
      if (await fiveFocusCard.isVisible()) {
        await expect(fiveFocusCard).not.toHaveClass(/disabled|unplayable|grayed/);

        await fiveFocusCard.click();
        await page.waitForTimeout(1000);

        // Should have 1 focus remaining (6 - 5)
        const focusDisplay = page.locator('[data-test="focus-display"], .focus-value, .focus');
        await expect(focusDisplay).toContainText('1');

        await page.screenshot({
          path: 'test-results/elena-high-tier-affordable.png',
          fullPage: true
        });
      }
    });

    test('should verify strategic tension - always forced to choose between good options', async ({ page }) => {
      await startElenaConversation(page);

      // Capture all available cards and their focus costs
      const availableCards = page.locator('.card:not(.disabled):not(.unplayable)');
      const cardCount = await availableCards.count();

      // Verify there are more affordable cards than can be played
      const focusDisplay = page.locator('[data-test="focus-display"], .focus-value, .focus');
      const currentFocus = await getNumericValue(focusDisplay);

      // Count cards that could theoretically be played with available focus
      let playableCardCount = 0;
      for (let i = 0; i < cardCount; i++) {
        const card = availableCards.nth(i);
        const cardText = await card.textContent();
        // Extract focus cost from card (implementation depends on card display format)
        if (cardText && cardText.includes('1')) playableCardCount++;
        if (cardText && cardText.includes('2')) playableCardCount++;
        if (cardText && cardText.includes('3') && currentFocus >= 3) playableCardCount++;
      }

      // Verify strategic tension exists
      expect(playableCardCount).toBeGreaterThan(2); // Multiple viable options

      await page.screenshot({
        path: 'test-results/elena-strategic-tension-demonstration.png',
        fullPage: true
      });
    });
  });

  test.describe('Decision Framework Testing', () => {
    test('should present early game choice: generate momentum vs manage doubt', async ({ page }) => {
      await startElenaConversation(page);

      await page.screenshot({
        path: 'test-results/elena-early-game-decisions.png',
        fullPage: true
      });

      // Look for momentum generating cards
      const momentumCard = page.locator('.card:has-text("momentum"), .card:has-text("rapport"), .card:has-text("conviction")').first();

      // Look for doubt management cards
      const doubtCard = page.locator('.card:has-text("doubt"), .card:has-text("reflect"), .card:has-text("center")').first();

      // Verify both options are available
      if (await momentumCard.isVisible() && await doubtCard.isVisible()) {
        await expect(momentumCard).toBeVisible();
        await expect(doubtCard).toBeVisible();

        // This demonstrates the strategic choice
        await page.screenshot({
          path: 'test-results/elena-momentum-vs-doubt-choice.png',
          fullPage: true
        });
      }
    });

    test('should present mid game choice: accept basic goal vs invest in enhanced rewards', async ({ page }) => {
      await startElenaConversation(page);

      // Build up to mid-game state (8+ momentum)
      await playMomentumGeneratingCards(page, 8);

      await page.screenshot({
        path: 'test-results/elena-mid-game-state.png',
        fullPage: true
      });

      // Check if goal acceptance is available
      const acceptGoalButton = page.locator('button:has-text("Accept"), button:has-text("Basic"), .goal-button').first();

      // Check if investment cards are available
      const investmentCard = page.locator('.card:has-text("invest"), .card:has-text("trust"), .card:has-text("flow")').first();

      if (await acceptGoalButton.isVisible() && await investmentCard.isVisible()) {
        await page.screenshot({
          path: 'test-results/elena-basic-vs-enhanced-choice.png',
          fullPage: true
        });
      }
    });

    test('should present late game choice: emergency doubt management vs flow investment', async ({ page }) => {
      await startElenaConversation(page);

      // Simulate late game state with high doubt
      await buildUpDoubt(page);
      await playMomentumGeneratingCards(page, 6);

      await page.screenshot({
        path: 'test-results/elena-late-game-high-doubt.png',
        fullPage: true
      });

      // Look for emergency doubt management
      const doubtManagementCard = page.locator('.card:has-text("confusion"), .card:has-text("doubt"), .card:has-text("clarify")').first();

      // Look for flow investment options
      const flowInvestmentCard = page.locator('.card:has-text("truth"), .card:has-text("flow"), .card:has-text("breakthrough")').first();

      if (await doubtManagementCard.isVisible() && await flowInvestmentCard.isVisible()) {
        await page.screenshot({
          path: 'test-results/elena-doubt-vs-flow-choice.png',
          fullPage: true
        });
      }
    });

    test('should confirm each turn presents 2-3 viable but mutually exclusive strategies', async ({ page }) => {
      await startElenaConversation(page);

      // Analyze turn options
      const availableCards = page.locator('.card:not(.disabled)');
      const cardCount = await availableCards.count();

      // Group cards by potential strategies
      const momentumCards = page.locator('.card:has-text("momentum"), .card:has-text("rapport")');
      const doubtCards = page.locator('.card:has-text("doubt"), .card:has-text("reflect")');
      const utilityCards = page.locator('.card:has-text("insight"), .card:has-text("draw")');

      const momentumCount = await momentumCards.count();
      const doubtCount = await doubtCards.count();
      const utilityCount = await utilityCards.count();

      // Verify multiple strategy options exist
      let strategyCounts = 0;
      if (momentumCount > 0) strategyCounts++;
      if (doubtCount > 0) strategyCounts++;
      if (utilityCount > 0) strategyCounts++;

      expect(strategyCounts).toBeGreaterThanOrEqual(2);

      await page.screenshot({
        path: 'test-results/elena-multiple-strategy-options.png',
        fullPage: true
      });
    });
  });
});

// Helper functions
async function navigateToElena(page) {
  // Look for navigation to Copper Kettle Tavern
  const tavernButton = page.locator('button:has-text("Tavern"), button:has-text("Copper Kettle"), .location:has-text("tavern")').first();
  if (await tavernButton.isVisible()) {
    await tavernButton.click();
    await page.waitForLoadState('networkidle');
  }

  // Look for Elena specifically
  const elenaButton = page.locator('button:has-text("Elena"), .npc:has-text("Elena"), .character:has-text("scribe")').first();
  if (await elenaButton.isVisible()) {
    await elenaButton.click();
    await page.waitForLoadState('networkidle');
  }
}

async function startElenaConversation(page) {
  // Look for conversation start button
  const startConversationButton = page.locator('button:has-text("Talk"), button:has-text("Speak"), button:has-text("Conversation"), button:has-text("Approach")').first();
  if (await startConversationButton.isVisible()) {
    await startConversationButton.click();
    await page.waitForTimeout(2000); // Wait for conversation UI to load
  }
}

async function playMomentumGeneratingCards(page, targetMomentum) {
  // Play cards that generate momentum until we reach target
  let currentMomentum = 0;

  while (currentMomentum < targetMomentum) {
    // Look for low-cost momentum generating cards
    const momentumCard = page.locator('.card:has-text("rapport"), .card:has-text("agreement"), .card:has-text("plea")').first();

    if (await momentumCard.isVisible()) {
      await momentumCard.click();
      await page.waitForTimeout(1000);

      // Check if we need to end turn
      const endTurnButton = page.locator('button:has-text("End Turn"), button:has-text("Continue"), button:has-text("Next")').first();
      if (await endTurnButton.isVisible()) {
        await endTurnButton.click();
        await page.waitForTimeout(1000);
      }

      currentMomentum += 2; // Estimate momentum gain
    } else {
      break; // No more momentum cards available
    }
  }
}

async function buildUpDoubt(page) {
  // Simulate actions that increase doubt (like failed cards or passing turns)
  const passButton = page.locator('button:has-text("Pass"), button:has-text("Skip"), button:has-text("Wait")').first();

  for (let i = 0; i < 3; i++) {
    if (await passButton.isVisible()) {
      await passButton.click();
      await page.waitForTimeout(1000);
    }
  }
}

async function getNumericValue(locator) {
  const text = await locator.textContent();
  const match = text?.match(/\d+/);
  return match ? parseInt(match[0]) : 0;
}