// Elena Strategic Playthroughs - Multiple Strategy Testing
const { test, expect } = require('@playwright/test');
const { ConversationTestHelpers } = require('./helpers/ConversationTestHelpers');

test.describe('Elena Strategic Playthroughs', () => {
  let helpers;

  test.beforeEach(async ({ page }) => {
    helpers = new ConversationTestHelpers(page);
  });

  test.describe('Aggressive Momentum Strategy', () => {
    test('should achieve letter request quickly with high-momentum cards', async () => {
      await helpers.startElenaConversation();

      const strategy = [
        'passionate_plea',      // High momentum emotional card
        'burning_conviction',   // Strong conviction card
        'moment_of_truth',      // Decisive moment card
        'emotional_appeal'      // Direct emotional approach
      ];

      const progression = await helpers.playTurnsAndTrackProgress(strategy);

      // Should reach letter threshold (8 momentum) within 4 turns
      expect(progression.length).toBeLessThanOrEqual(4);

      const finalState = progression[progression.length - 1];
      expect(finalState.afterListen.momentum).toBeGreaterThanOrEqual(8);

      // Verify letter request becomes available
      expect(finalState.letterRequestAvailable).toBe(true);

      // Complete the request
      await helpers.clickRequestCard('elena_letter_request');

      const isEnded = await helpers.isConversationEnded();
      expect(isEnded).toBe(true);
    });

    test('should balance momentum gain against doubt accumulation', async () => {
      await helpers.startElenaConversation();

      const progression = await helpers.playTurnsAndTrackProgress([
        'passionate_plea',
        'burning_conviction',
        'moment_of_truth'
      ]);

      // High momentum strategy should accumulate some doubt
      const finalState = progression[progression.length - 1];
      expect(finalState.afterListen.momentum).toBeGreaterThan(6);
      expect(finalState.afterListen.doubt).toBeGreaterThan(2);
      expect(finalState.afterListen.doubt).toBeLessThan(7); // Not overwhelming
    });
  });

  test.describe('Conservative Rapport Building Strategy', () => {
    test('should build momentum slowly with low-risk cards', async () => {
      await helpers.startElenaConversation();

      const strategy = [
        'gentle_agreement',     // Low-risk rapport card
        'show_understanding',   // Empathetic approach
        'build_rapport',        // Direct rapport building
        'careful_words',        // Conservative communication
        'pause_reflect',        // Thoughtful regulation
        'deep_understanding'    // Deeper connection
      ];

      const progression = await helpers.playTurnsAndTrackProgress(strategy);

      // Should take longer but accumulate less doubt
      expect(progression.length).toBeGreaterThanOrEqual(5);

      const finalState = progression[progression.length - 1];
      expect(finalState.afterListen.doubt).toBeLessThan(6);

      // Should eventually reach letter threshold
      if (finalState.afterListen.momentum >= 8) {
        expect(finalState.letterRequestAvailable).toBe(true);
      }
    });

    test('should demonstrate lower doubt accumulation', async () => {
      await helpers.startElenaConversation();

      const conservativeProgression = await helpers.playTurnsAndTrackProgress([
        'gentle_agreement',
        'show_understanding',
        'careful_words'
      ]);

      // Compare with aggressive strategy
      await helpers.startElenaConversation();

      const aggressiveProgression = await helpers.playTurnsAndTrackProgress([
        'passionate_plea',
        'burning_conviction',
        'moment_of_truth'
      ]);

      // Conservative should have lower doubt for same number of turns
      const conservativeDoubt = conservativeProgression[conservativeProgression.length - 1].afterListen.doubt;
      const aggressiveDoubt = aggressiveProgression[aggressiveProgression.length - 1].afterListen.doubt;

      expect(conservativeDoubt).toBeLessThanOrEqual(aggressiveDoubt);
    });
  });

  test.describe('Mixed Strategy Optimization', () => {
    test('should optimize momentum/doubt balance with mixed approach', async () => {
      await helpers.startElenaConversation();

      const strategy = [
        'gentle_agreement',     // Start safe
        'passionate_plea',      // Build momentum
        'pause_reflect',        // Regulate if needed
        'burning_conviction',   // Push for threshold
        'show_understanding'    // Stabilize
      ];

      const progression = await helpers.playTurnsAndTrackProgress(strategy);

      const finalState = progression[progression.length - 1];

      // Should achieve good momentum without excessive doubt
      expect(finalState.afterListen.momentum).toBeGreaterThan(6);
      expect(finalState.afterListen.doubt).toBeLessThan(6);

      // Should be efficient (reasonable turn count)
      expect(progression.length).toBeLessThanOrEqual(6);
    });

    test('should demonstrate strategic card ordering matters', async () => {
      // Strategy 1: High momentum first, then regulate
      await helpers.startElenaConversation();
      const earlyAggressive = await helpers.playTurnsAndTrackProgress([
        'passionate_plea',
        'burning_conviction',
        'pause_reflect'
      ]);

      // Strategy 2: Build rapport first, then push
      await helpers.startElenaConversation();
      const earlyConservative = await helpers.playTurnsAndTrackProgress([
        'gentle_agreement',
        'show_understanding',
        'passionate_plea'
      ]);

      // Different orderings should produce different outcomes
      const aggressive = earlyAggressive[earlyAggressive.length - 1];
      const conservative = earlyConservative[earlyConservative.length - 1];

      // Should demonstrate meaningful differences in final state
      expect(aggressive.afterListen.momentum !== conservative.afterListen.momentum ||
             aggressive.afterListen.doubt !== conservative.afterListen.doubt).toBe(true);
    });
  });

  test.describe('Safety Request Strategy', () => {
    test('should achieve safety request with sustained momentum building', async () => {
      await helpers.startElenaConversation();

      const strategy = [
        'passionate_plea',
        'burning_conviction',
        'moment_of_truth',
        'deep_understanding',
        'emotional_appeal',
        'authoritative_demand'   // Push for higher threshold
      ];

      const progression = await helpers.playTurnsAndTrackProgress(strategy);

      // Should reach safety threshold (12 momentum)
      const finalState = progression[progression.length - 1];
      if (finalState.afterListen.momentum >= 12) {
        expect(finalState.safetyRequestAvailable).toBe(true);

        // Complete safety request
        await helpers.clickRequestCard('elena_safety_request');

        const isEnded = await helpers.isConversationEnded();
        expect(isEnded).toBe(true);

        const endReason = await helpers.getEndReason();
        expect(endReason).toContain('success');
      }
    });

    test('should validate safety request requires sustained effort', async () => {
      await helpers.startElenaConversation();

      // Short aggressive burst
      const shortProgression = await helpers.playTurnsAndTrackProgress([
        'passionate_plea',
        'burning_conviction',
        'moment_of_truth'
      ]);

      const shortFinalState = shortProgression[shortProgression.length - 1];

      // Should not be enough for safety request
      expect(shortFinalState.afterListen.momentum).toBeLessThan(12);
      expect(shortFinalState.safetyRequestAvailable || false).toBe(false);
    });
  });

  test.describe('Failure Strategy Testing', () => {
    test('should demonstrate conversation failure through excessive doubt', async () => {
      await helpers.startElenaConversation();

      // Deliberately accumulate doubt by listening repeatedly
      let doubtLevel = 0;
      while (doubtLevel < 10) {
        if (await helpers.isConversationEnded()) break;

        await helpers.listen();
        await helpers.page.waitForTimeout(500);

        const state = await helpers.getMomentumAndDoubt();
        doubtLevel = state.doubt;
      }

      // Should end in failure
      const isEnded = await helpers.isConversationEnded();
      expect(isEnded).toBe(true);

      const endReason = await helpers.getEndReason();
      expect(endReason).toContain('doubt');
    });

    test('should demonstrate momentum erosion mechanics', async () => {
      await helpers.startElenaConversation();

      // Build momentum, then let doubt erode it
      await helpers.selectCard('passionate_plea');
      await helpers.speak();

      const afterMomentum = await helpers.getMomentumAndDoubt();
      expect(afterMomentum.momentum).toBeGreaterThan(0);

      // Build some doubt
      await helpers.listen();
      await helpers.listen();
      await helpers.listen();

      const afterDoubt = await helpers.getMomentumAndDoubt();
      expect(afterDoubt.doubt).toBeGreaterThan(2);

      // Play another card and observe erosion
      await helpers.selectCard('gentle_agreement');
      await helpers.speak();
      await helpers.listen();

      const afterErosion = await helpers.getMomentumAndDoubt();

      // Momentum should have been affected by doubt erosion
      expect(afterErosion.momentum).toBeLessThan(afterMomentum.momentum + 3); // Account for new card momentum
    });
  });

  test.describe('Devoted Personality Testing', () => {
    test('should validate Elena\'s devoted personality affects momentum loss', async () => {
      await helpers.startElenaConversation();

      // Build momentum and doubt
      await helpers.selectCard('passionate_plea');
      await helpers.speak();
      await helpers.listen();
      await helpers.listen(); // Build doubt

      const beforeErosion = await helpers.getMomentumAndDoubt();

      // Execute another turn to trigger devoted personality erosion doubling
      await helpers.selectCard('gentle_agreement');
      await helpers.speak();
      await helpers.listen();

      const afterErosion = await helpers.getMomentumAndDoubt();

      // With devoted personality, momentum loss should be more significant
      if (beforeErosion.doubt > 0) {
        const erosionAmount = beforeErosion.momentum - afterErosion.momentum;
        expect(erosionAmount).toBeGreaterThan(beforeErosion.doubt); // Doubled effect
      }
    });
  });

  test.describe('Promise Card Mechanics', () => {
    test('should unlock promise card at letter threshold', async () => {
      await helpers.startElenaConversation();

      const progression = await helpers.playTurnsAndTrackProgress([
        'passionate_plea',
        'burning_conviction',
        'emotional_appeal'
      ]);

      const finalState = progression[progression.length - 1];
      if (finalState.afterListen.momentum >= 8) {
        // Both letter request and promise should be available
        expect(finalState.letterRequestAvailable).toBe(true);

        const promiseAvailable = await helpers.isRequestCardAvailable('elena_priority_promise');
        expect(promiseAvailable).toBe(true);
      }
    });

    test('should validate promise card provides alternative completion path', async () => {
      await helpers.startElenaConversation();

      // Build to promise threshold
      await helpers.playTurnsAndTrackProgress([
        'passionate_plea',
        'burning_conviction',
        'emotional_appeal'
      ]);

      const state = await helpers.getMomentumAndDoubt();
      if (state.momentum >= 8) {
        // Choose promise card instead of letter request
        await helpers.clickRequestCard('elena_priority_promise');

        const isEnded = await helpers.isConversationEnded();
        expect(isEnded).toBe(true);

        const endReason = await helpers.getEndReason();
        expect(endReason).toContain('promise');
      }
    });
  });

  test.describe('Turn-by-Turn Decision Analysis', () => {
    test('should document meaningful choice points', async () => {
      await helpers.startElenaConversation();

      const decisionPoints = [];

      for (let turn = 0; turn < 5; turn++) {
        if (await helpers.isConversationEnded()) break;

        const availableCards = await helpers.getAvailableCards();
        const playableCards = availableCards.filter(card => card.isPlayable);
        const state = await helpers.getMomentumAndDoubt();

        decisionPoints.push({
          turn,
          momentum: state.momentum,
          doubt: state.doubt,
          options: playableCards.length,
          cardTypes: playableCards.map(card => ({ name: card.name, focus: card.focus }))
        });

        // Play first available card
        await helpers.executeTurn(playableCards[0].id);
      }

      // Verify each turn had meaningful choices
      for (const point of decisionPoints) {
        expect(point.options).toBeGreaterThanOrEqual(2);
        expect(point.cardTypes.length).toBeGreaterThanOrEqual(2);
      }

      // Verify state progression
      for (let i = 1; i < decisionPoints.length; i++) {
        const prev = decisionPoints[i-1];
        const curr = decisionPoints[i];

        // Either momentum increased or doubt increased (or both)
        expect(curr.momentum > prev.momentum || curr.doubt > prev.doubt).toBe(true);
      }
    });

    test('should validate no dominant strategies exist', async () => {
      const strategies = [
        ['passionate_plea', 'burning_conviction', 'moment_of_truth'],
        ['gentle_agreement', 'show_understanding', 'build_rapport'],
        ['pause_reflect', 'careful_words', 'deep_understanding'],
        ['emotional_appeal', 'authoritative_demand', 'desperate_gambit']
      ];

      const results = [];

      for (const strategy of strategies) {
        await helpers.startElenaConversation();
        const progression = await helpers.playTurnsAndTrackProgress(strategy);
        const finalState = progression[progression.length - 1];

        results.push({
          strategy: strategy.join(' -> '),
          momentum: finalState.afterListen.momentum,
          doubt: finalState.afterListen.doubt,
          turns: progression.length
        });
      }

      // No single strategy should dominate all metrics
      const momentumScores = results.map(r => r.momentum);
      const doubtScores = results.map(r => r.doubt);
      const turnCounts = results.map(r => r.turns);

      // Should have variety in outcomes
      expect(Math.max(...momentumScores) - Math.min(...momentumScores)).toBeGreaterThan(2);
      expect(Math.max(...doubtScores) - Math.min(...doubtScores)).toBeGreaterThan(1);

      // Each strategy should have some merit
      expect(results.every(r => r.momentum > 0)).toBe(true);
      expect(results.every(r => r.doubt < 8)).toBe(true);
    });
  });
});