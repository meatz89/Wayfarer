// Elena Conversation Edge Cases and Performance Validation
const { test, expect } = require('@playwright/test');
const { ConversationTestHelpers } = require('./helpers/ConversationTestHelpers');

test.describe('Elena Conversation Edge Cases and Performance', () => {
  let helpers;

  test.beforeEach(async ({ page }) => {
    helpers = new ConversationTestHelpers(page);
  });

  test.describe('Edge Case Handling', () => {
    test('should handle maximum hand size gracefully', async () => {
      await helpers.startElenaConversation();

      // Keep drawing cards without playing to test hand overflow
      let handSize = 0;
      let previousHandSize = 0;

      for (let i = 0; i < 15; i++) {
        if (await helpers.isConversationEnded()) break;

        previousHandSize = handSize;
        await helpers.listen();
        handSize = await helpers.getHandSize();

        // Verify hand size increases until cap
        if (handSize <= 10) {
          expect(handSize).toBeGreaterThanOrEqual(previousHandSize);
        } else {
          // Should handle overflow gracefully
          expect(handSize).toBeLessThanOrEqual(15); // Reasonable upper bound
        }
      }

      // UI should remain functional even with large hand
      const cards = await helpers.getAvailableCards();
      expect(cards.length).toBeGreaterThan(0);

      // Should be able to play cards normally
      await helpers.selectCard(cards[0].id);
      await helpers.speak();
    });

    test('should handle zero momentum edge case', async () => {
      await helpers.startElenaConversation();

      // Start with zero momentum
      const initialState = await helpers.getMomentumAndDoubt();
      expect(initialState.momentum).toBe(0);

      // Build doubt without momentum
      await helpers.listen();
      await helpers.listen();

      const doubtState = await helpers.getMomentumAndDoubt();
      expect(doubtState.doubt).toBeGreaterThan(0);
      expect(doubtState.momentum).toBe(0);

      // Should still be able to play cards
      const cards = await helpers.getAvailableCards();
      await helpers.selectCard(cards[0].id);
      await helpers.speak();

      // Verify momentum can increase from zero
      const afterSpeak = await helpers.getMomentumAndDoubt();
      expect(afterSpeak.momentum).toBeGreaterThanOrEqual(0);
    });

    test('should handle maximum doubt threshold precisely', async () => {
      await helpers.startElenaConversation();

      // Build to exactly 9 doubt
      for (let i = 0; i < 9; i++) {
        await helpers.listen();
        const state = await helpers.getMomentumAndDoubt();

        if (state.doubt >= 9) break;
      }

      const nearMaxState = await helpers.getMomentumAndDoubt();
      expect(nearMaxState.doubt).toBeLessThan(10);

      // One more listen should trigger end
      await helpers.listen();

      const maxState = await helpers.getMomentumAndDoubt();
      expect(maxState.doubt).toBe(10);

      // Conversation should end
      const isEnded = await helpers.isConversationEnded();
      expect(isEnded).toBe(true);
    });

    test('should handle focus edge cases correctly', async () => {
      await helpers.startElenaConversation();

      const focusInfo = await helpers.getFocusInfo();
      const cards = await helpers.getAvailableCards();

      // Find cards with exact focus limit
      const exactFocusCard = cards.find(card => card.focus === focusInfo.max);
      const overFocusCard = cards.find(card => card.focus > focusInfo.max);

      if (exactFocusCard) {
        // Should be able to play card with exact focus
        await helpers.selectCard(exactFocusCard.id);
        await helpers.speak();
      }

      if (overFocusCard) {
        // Should not be able to play card over focus
        await helpers.selectCard(overFocusCard.id);
        const speakButton = helpers.page.locator('[data-testid="speak-button"]');
        expect(await speakButton.isDisabled()).toBe(true);
      }
    });

    test('should handle rapid UI interactions', async () => {
      await helpers.startElenaConversation();

      const cards = await helpers.getAvailableCards();

      // Rapid card selection changes
      for (let i = 0; i < Math.min(5, cards.length); i++) {
        await helpers.selectCard(cards[i].id);
        await helpers.page.waitForTimeout(100);
      }

      // Should maintain correct final selection
      const finalCard = cards[cards.length - 1];
      await helpers.selectCard(finalCard.id);

      const selectedCard = await helpers.page.locator('[data-testid="selected-card"]').textContent();
      expect(selectedCard).toContain(finalCard.name);

      // Should execute normally
      await helpers.speak();
      await helpers.listen();
    });

    test('should handle conversation restart edge case', async () => {
      await helpers.startElenaConversation();

      // Play a few turns
      await helpers.executeTurn('gentle_agreement');
      await helpers.executeTurn('show_understanding');

      const midState = await helpers.getMomentumAndDoubt();
      expect(midState.momentum).toBeGreaterThan(0);

      // Restart conversation
      await helpers.startElenaConversation();

      // Should reset to initial state
      const restartState = await helpers.getMomentumAndDoubt();
      expect(restartState.momentum).toBe(0);
      expect(restartState.doubt).toBe(0);

      const restartTurn = await helpers.getTurnNumber();
      expect(restartTurn).toBe(0);
    });
  });

  test.describe('Performance Validation', () => {
    test('should maintain responsive UI during long conversations', async () => {
      await helpers.startElenaConversation();

      const responseTimes = [];

      for (let turn = 0; turn < 10; turn++) {
        if (await helpers.isConversationEnded()) break;

        const startTime = Date.now();

        const cards = await helpers.getAvailableCards();
        await helpers.selectCard(cards[0].id);
        await helpers.speak();
        await helpers.listen();

        const endTime = Date.now();
        responseTimes.push(endTime - startTime);
      }

      // Response times should remain reasonable
      const averageTime = responseTimes.reduce((a, b) => a + b, 0) / responseTimes.length;
      expect(averageTime).toBeLessThan(3000); // 3 seconds average

      // No individual turn should take too long
      const maxTime = Math.max(...responseTimes);
      expect(maxTime).toBeLessThan(5000); // 5 seconds max
    });

    test('should handle multiple conversation sessions without degradation', async () => {
      const sessionResults = [];

      for (let session = 0; session < 3; session++) {
        const sessionStart = Date.now();

        await helpers.startElenaConversation();

        // Execute standard conversation
        const progression = await helpers.playTurnsAndTrackProgress([
          'gentle_agreement',
          'passionate_plea',
          'show_understanding'
        ]);

        const sessionEnd = Date.now();
        sessionResults.push({
          duration: sessionEnd - sessionStart,
          turns: progression.length,
          finalMomentum: progression[progression.length - 1].afterListen.momentum
        });
      }

      // Performance should not degrade across sessions
      const durations = sessionResults.map(r => r.duration);
      const lastSession = durations[durations.length - 1];
      const firstSession = durations[0];

      expect(lastSession).toBeLessThan(firstSession * 2); // No major degradation

      // Results should be consistent
      const momentumValues = sessionResults.map(r => r.finalMomentum);
      const momentumVariance = Math.max(...momentumValues) - Math.min(...momentumValues);
      expect(momentumVariance).toBeLessThan(3); // Reasonable consistency
    });

    test('should validate memory usage during extended play', async () => {
      await helpers.startElenaConversation();

      // Check initial memory usage via performance API if available
      const initialMemory = await helpers.page.evaluate(() => {
        return performance.memory ? performance.memory.usedJSHeapSize : null;
      });

      // Execute extended conversation
      let turnCount = 0;
      while (!await helpers.isConversationEnded() && turnCount < 20) {
        const cards = await helpers.getAvailableCards();
        await helpers.executeTurn(cards[0].id);
        turnCount++;
      }

      // Check final memory usage
      const finalMemory = await helpers.page.evaluate(() => {
        return performance.memory ? performance.memory.usedJSHeapSize : null;
      });

      if (initialMemory && finalMemory) {
        const memoryIncrease = finalMemory - initialMemory;
        const memoryIncreaseKB = memoryIncrease / 1024;

        // Memory increase should be reasonable (less than 5MB)
        expect(memoryIncreaseKB).toBeLessThan(5000);
      }
    });
  });

  test.describe('State Consistency Validation', () => {
    test('should maintain consistent state through complex interactions', async () => {
      await helpers.startElenaConversation();

      const stateHistory = [];

      // Execute complex interaction pattern
      const interactions = [
        () => helpers.selectCard('passionate_plea'),
        () => helpers.speak(),
        () => helpers.listen(),
        () => helpers.selectCard('pause_reflect'),
        () => helpers.speak(),
        () => helpers.listen(),
        () => helpers.selectCard('emotional_appeal'),
        () => helpers.speak(),
        () => helpers.listen()
      ];

      for (const interaction of interactions) {
        await interaction();
        const state = await helpers.getMomentumAndDoubt();
        const turn = await helpers.getTurnNumber();
        const focus = await helpers.getFocusInfo();

        stateHistory.push({
          momentum: state.momentum,
          doubt: state.doubt,
          turn,
          currentFocus: focus.current,
          maxFocus: focus.max
        });
      }

      // Validate state transitions are logical
      for (let i = 1; i < stateHistory.length; i++) {
        const prev = stateHistory[i-1];
        const curr = stateHistory[i];

        // Momentum should never go negative
        expect(curr.momentum).toBeGreaterThanOrEqual(0);

        // Doubt should never exceed maximum
        expect(curr.doubt).toBeLessThanOrEqual(10);

        // Turn should only increase or stay same
        expect(curr.turn).toBeGreaterThanOrEqual(prev.turn);

        // Focus should be within reasonable bounds
        expect(curr.currentFocus).toBeGreaterThanOrEqual(0);
        expect(curr.currentFocus).toBeLessThanOrEqual(curr.maxFocus);
      }
    });

    test('should validate deterministic behavior', async () => {
      const runs = [];

      // Execute same sequence multiple times
      for (let run = 0; run < 3; run++) {
        await helpers.startElenaConversation();

        const progression = await helpers.playTurnsAndTrackProgress([
          'gentle_agreement',
          'passionate_plea',
          'show_understanding'
        ]);

        runs.push(progression);
      }

      // Results should be deterministic (same inputs = same outputs)
      for (let turn = 0; turn < runs[0].length; turn++) {
        const momentum0 = runs[0][turn].afterListen.momentum;
        const doubt0 = runs[0][turn].afterListen.doubt;

        for (let run = 1; run < runs.length; run++) {
          if (runs[run][turn]) {
            expect(runs[run][turn].afterListen.momentum).toBe(momentum0);
            expect(runs[run][turn].afterListen.doubt).toBe(doubt0);
          }
        }
      }
    });

    test('should handle concurrent UI updates correctly', async () => {
      await helpers.startElenaConversation();

      // Simulate concurrent operations
      const cards = await helpers.getAvailableCards();

      // Start multiple async operations
      const operations = [
        helpers.selectCard(cards[0].id),
        helpers.getMomentumAndDoubt(),
        helpers.getFocusInfo(),
        helpers.getHandSize()
      ];

      const results = await Promise.all(operations);

      // All operations should complete successfully
      expect(results).toHaveLength(4);
      expect(results[1]).toHaveProperty('momentum');
      expect(results[1]).toHaveProperty('doubt');
      expect(results[2]).toHaveProperty('current');
      expect(results[2]).toHaveProperty('max');
      expect(typeof results[3]).toBe('number');

      // UI should remain in consistent state
      await helpers.speak();
      await helpers.listen();

      const finalState = await helpers.getMomentumAndDoubt();
      expect(finalState.momentum).toBeGreaterThanOrEqual(0);
      expect(finalState.doubt).toBeGreaterThanOrEqual(0);
    });
  });

  test.describe('Browser Compatibility and Reliability', () => {
    test('should handle page reload gracefully', async () => {
      await helpers.startElenaConversation();

      // Play a few turns
      await helpers.executeTurn('gentle_agreement');
      await helpers.executeTurn('passionate_plea');

      // Reload page
      await helpers.page.reload();

      // Should return to main menu or initial state
      await helpers.page.waitForSelector('[data-testid="game-screen"]', { timeout: 10000 });

      // Should be able to start new conversation
      await helpers.startElenaConversation();

      const state = await helpers.getMomentumAndDoubt();
      expect(state.momentum).toBe(0);
      expect(state.doubt).toBe(0);
    });

    test('should handle network interruption simulation', async () => {
      await helpers.startElenaConversation();

      // Simulate slow network
      await helpers.page.route('**/*', route => {
        setTimeout(() => route.continue(), 100);
      });

      // Should still function with network delays
      await helpers.executeTurn('gentle_agreement');

      const state = await helpers.getMomentumAndDoubt();
      expect(state).toBeDefined();
      expect(typeof state.momentum).toBe('number');
      expect(typeof state.doubt).toBe('number');

      // Remove network simulation
      await helpers.page.unroute('**/*');
    });

    test('should validate accessibility features', async () => {
      await helpers.startElenaConversation();

      // Check for proper ARIA labels and roles
      const cards = await helpers.page.locator('[data-testid="hand-card"]').all();
      for (const card of cards.slice(0, 3)) { // Check first 3 cards
        const ariaLabel = await card.getAttribute('aria-label');
        const role = await card.getAttribute('role');

        // Should have proper accessibility attributes
        expect(ariaLabel || role).toBeTruthy();
      }

      // Check keyboard navigation
      await helpers.page.keyboard.press('Tab');
      const focusedElement = await helpers.page.locator(':focus').count();
      expect(focusedElement).toBeGreaterThan(0);
    });
  });

  test.describe('Data Validation and Integrity', () => {
    test('should validate all numerical values remain within bounds', async () => {
      await helpers.startElenaConversation();

      const validationChecks = [];

      // Execute extended conversation with validation
      for (let turn = 0; turn < 8; turn++) {
        if (await helpers.isConversationEnded()) break;

        const cards = await helpers.getAvailableCards();
        await helpers.executeTurn(cards[0].id);

        const state = await helpers.getMomentumAndDoubt();
        const focus = await helpers.getFocusInfo();
        const handSize = await helpers.getHandSize();

        validationChecks.push({
          turn,
          momentum: state.momentum,
          doubt: state.doubt,
          focus: focus.current,
          maxFocus: focus.max,
          handSize
        });

        // Validate bounds
        expect(state.momentum).toBeGreaterThanOrEqual(0);
        expect(state.momentum).toBeLessThan(100); // Reasonable upper bound
        expect(state.doubt).toBeGreaterThanOrEqual(0);
        expect(state.doubt).toBeLessThanOrEqual(10);
        expect(focus.current).toBeGreaterThanOrEqual(0);
        expect(focus.current).toBeLessThanOrEqual(focus.max);
        expect(handSize).toBeGreaterThan(0);
        expect(handSize).toBeLessThan(20); // Reasonable upper bound
      }

      // Export validation data for analysis
      console.log('Validation checks:', JSON.stringify(validationChecks, null, 2));
    });

    test('should ensure card availability matches game rules', async () => {
      await helpers.startElenaConversation();

      for (let turn = 0; turn < 5; turn++) {
        if (await helpers.isConversationEnded()) break;

        const cards = await helpers.getAvailableCards();
        const state = await helpers.getMomentumAndDoubt();

        // Validate request card availability rules
        if (state.momentum >= 8) {
          const letterRequest = await helpers.isRequestCardAvailable('elena_letter_request');
          const promiseCard = await helpers.isRequestCardAvailable('elena_priority_promise');

          // At least one should be available at this threshold
          expect(letterRequest || promiseCard).toBe(true);
        }

        if (state.momentum >= 12) {
          const safetyRequest = await helpers.isRequestCardAvailable('elena_safety_request');
          expect(safetyRequest).toBe(true);
        }

        // All playable cards should respect focus constraints
        const focus = await helpers.getFocusInfo();
        const playableCards = cards.filter(card => card.isPlayable);

        for (const card of playableCards) {
          expect(card.focus).toBeLessThanOrEqual(focus.max);
        }

        await helpers.executeTurn(cards[0].id);
      }
    });
  });
});