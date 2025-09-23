/**
 * Balance Validation Tests
 * Evaluates game balance, power levels, and comparative improvements vs old system
 */

const { test, expect } = require('@playwright/test');
const { GameHelpers } = require('./utils/game-helpers');

test.describe('Balance Validation', () => {
  let gameHelpers;

  test.beforeEach(async ({ page }) => {
    gameHelpers = new GameHelpers(page);
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();
  });

  test('No Dominant Strategies: Multiple approaches should be viable', async ({ page }) => {
    const strategies = {
      aggressive: { attempts: 0, successes: 0, avgMomentum: 0, avgTurns: 0 },
      defensive: { attempts: 0, successes: 0, avgMomentum: 0, avgTurns: 0 },
      efficient: { attempts: 0, successes: 0, avgMomentum: 0, avgTurns: 0 },
      scaling: { attempts: 0, successes: 0, avgMomentum: 0, avgTurns: 0 }
    };

    // Test each strategy multiple times
    for (const [strategyName, strategy] of Object.entries(strategies)) {
      console.log(`Testing ${strategyName} strategy...`);

      for (let attempt = 0; attempt < 3; attempt++) {
        // Reset for each attempt
        await page.goto('/');
        await gameHelpers.waitForGameReady();
        await gameHelpers.startNewGame();
        await gameHelpers.startConversation('Elena');

        strategy.attempts++;
        let turnCount = 0;
        let finalMomentum = 0;
        let wasSuccessful = false;

        for (let turn = 0; turn < 10; turn++) {
          const gameState = await gameHelpers.getGameState();
          turnCount++;

          await gameHelpers.executeListen();
          await gameHelpers.waitForNarrativeGeneration();

          const cards = await gameHelpers.getHandCards();
          if (cards.length === 0) break;

          const playableCards = cards.filter(card =>
            card.isSelectable && card.focus <= gameState.conversationState.focus.available
          );

          if (playableCards.length === 0) break;

          // Apply strategy-specific card selection
          let chosenCard = null;

          switch (strategyName) {
            case 'aggressive':
              // Prioritize high-momentum, high-focus cards
              const aggressiveCards = playableCards.filter(card =>
                card.effect?.includes('Strike') || card.focus >= 3
              );
              chosenCard = aggressiveCards.length > 0 ?
                aggressiveCards.reduce((best, current) => current.focus > best.focus ? current : best) :
                playableCards.reduce((best, current) => current.focus > best.focus ? current : best);
              break;

            case 'defensive':
              // Prioritize doubt management and safety
              const defensiveCards = playableCards.filter(card =>
                card.effect?.includes('Soothe') || card.focus <= 2
              );
              chosenCard = defensiveCards.length > 0 ?
                defensiveCards[0] :
                playableCards.reduce((best, current) => current.focus < best.focus ? current : best);
              break;

            case 'efficient':
              // Prioritize setup and resource efficiency
              const efficientCards = playableCards.filter(card =>
                card.effect?.includes('Focusing') ||
                card.name?.includes('mental_reset') ||
                card.name?.includes('careful_words')
              );
              chosenCard = efficientCards.length > 0 ? efficientCards[0] : playableCards[0];
              break;

            case 'scaling':
              // Prioritize cards with scaling effects
              const scalingCards = playableCards.filter(card =>
                card.hasScaling ||
                card.effect?.includes('spend') ||
                card.effect?.includes('hand') ||
                card.effect?.includes('doubt')
              );
              chosenCard = scalingCards.length > 0 ? scalingCards[0] : playableCards[0];
              break;
          }

          if (chosenCard) {
            await gameHelpers.selectCard(chosenCard.name);
            await gameHelpers.executeSpeak();
            await gameHelpers.waitForNarrativeGeneration();

            const newState = await gameHelpers.getGameState();
            finalMomentum = newState.conversationState.momentum;

            // Check for success conditions (high momentum, low doubt)
            if (finalMomentum >= 8 && newState.conversationState.doubt <= 5) {
              wasSuccessful = true;
            }
          }

          if (await gameHelpers.isConversationExhausted()) break;
        }

        if (wasSuccessful) {
          strategy.successes++;
        }

        strategy.avgMomentum += finalMomentum;
        strategy.avgTurns += turnCount;

        await gameHelpers.takeScreenshot(`balance_${strategyName}_attempt_${attempt + 1}_final`);
      }

      // Calculate averages
      strategy.avgMomentum = strategy.avgMomentum / strategy.attempts;
      strategy.avgTurns = strategy.avgTurns / strategy.attempts;
    }

    // Analyze balance
    const successRates = Object.entries(strategies).map(([name, strategy]) => ({
      name,
      successRate: (strategy.successes / strategy.attempts) * 100,
      avgMomentum: strategy.avgMomentum,
      avgTurns: strategy.avgTurns
    }));

    const maxSuccessRate = Math.max(...successRates.map(s => s.successRate));
    const minSuccessRate = Math.min(...successRates.map(s => s.successRate));
    const successRateVariation = maxSuccessRate - minSuccessRate;

    const avgMomentumValues = successRates.map(s => s.avgMomentum);
    const momentumVariation = Math.max(...avgMomentumValues) - Math.min(...avgMomentumValues);

    console.log(`Strategy Balance Analysis:
      - Success rates: ${successRates.map(s => `${s.name}: ${s.successRate.toFixed(1)}%`).join(', ')}
      - Average momentum: ${successRates.map(s => `${s.name}: ${s.avgMomentum.toFixed(1)}`).join(', ')}
      - Average turns: ${successRates.map(s => `${s.name}: ${s.avgTurns.toFixed(1)}`).join(', ')}
      - Success rate variation: ${successRateVariation.toFixed(1)}%
      - Momentum variation: ${momentumVariation.toFixed(1)}
      - Viable strategies: ${successRates.filter(s => s.successRate >= 30).length}/4`);

    // Balance assertions
    expect(successRateVariation).toBeLessThan(40); // No strategy should dominate by >40%
    expect(successRates.filter(s => s.successRate >= 30).length).toBeGreaterThan(2); // At least 3 viable strategies
    expect(momentumVariation).toBeLessThan(6); // Momentum outcomes should be reasonably balanced
  });

  test('Power Level Consistency: Focus cost should correlate with card power', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const powerAnalysis = {
      cardsByFocus: {},
      powerCorrelation: [],
      outliers: []
    };

    for (let turn = 0; turn < 6; turn++) {
      const beforeState = await gameHelpers.getGameState();
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();
      if (cards.length === 0) break;

      // Analyze each card's power relative to focus cost
      for (const card of cards) {
        const focus = card.focus;

        if (!powerAnalysis.cardsByFocus[focus]) {
          powerAnalysis.cardsByFocus[focus] = [];
        }

        // Calculate theoretical power based on description
        let theoreticalPower = 0;

        // Base power from difficulty (implied by description)
        if (card.effect?.includes('Strike')) theoreticalPower += 2;
        if (card.effect?.includes('Soothe')) theoreticalPower += 2;
        if (card.effect?.includes('Advancing')) theoreticalPower += 3;
        if (card.effect?.includes('Focusing')) theoreticalPower += 1;

        // Scaling effects add power
        if (card.hasScaling) theoreticalPower += 1;
        if (card.effect?.includes('spend')) theoreticalPower += 1;

        // Category-based power
        if (card.category === 'Realization') theoreticalPower += 1;
        if (card.isImpulse) theoreticalPower += 0.5; // Risk/reward

        powerAnalysis.cardsByFocus[focus].push({
          name: card.name,
          focus: focus,
          effect: card.effect,
          theoreticalPower: theoreticalPower,
          category: card.category,
          hasScaling: card.hasScaling
        });

        // Test actual power if playable
        if (card.isSelectable && focus <= beforeState.conversationState.focus.available) {
          await gameHelpers.selectCard(card.name);
          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();

          const afterState = await gameHelpers.getGameState();

          const actualMomentumGain = afterState.conversationState.momentum - beforeState.conversationState.momentum;
          const actualDoubtReduction = beforeState.conversationState.doubt - afterState.conversationState.doubt;

          // Calculate actual power
          const actualPower = Math.max(actualMomentumGain, 0) + Math.max(actualDoubtReduction, 0);

          const powerEfficiency = actualPower / Math.max(focus, 1);
          const expectedEfficiency = theoreticalPower / Math.max(focus, 1);

          powerAnalysis.powerCorrelation.push({
            cardName: card.name,
            focus: focus,
            theoreticalPower: theoreticalPower,
            actualPower: actualPower,
            powerEfficiency: powerEfficiency,
            expectedEfficiency: expectedEfficiency,
            momentumGain: actualMomentumGain,
            doubtReduction: actualDoubtReduction
          });

          // Identify outliers (cards with power significantly different from focus cost)
          if (Math.abs(powerEfficiency - 1.0) > 0.5) {
            powerAnalysis.outliers.push({
              cardName: card.name,
              focus: focus,
              actualPower: actualPower,
              powerEfficiency: powerEfficiency,
              type: powerEfficiency > 1.5 ? 'overpowered' : 'underpowered'
            });
          }

          break; // Only test one card per turn
        }
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    // Analyze power correlation
    const focusLevels = Object.keys(powerAnalysis.cardsByFocus).map(k => parseInt(k)).sort((a, b) => a - b);
    let correlationScore = 0;
    let correlationTests = 0;

    for (let i = 0; i < focusLevels.length - 1; i++) {
      const lowerFocus = focusLevels[i];
      const higherFocus = focusLevels[i + 1];

      const lowerCards = powerAnalysis.cardsByFocus[lowerFocus];
      const higherCards = powerAnalysis.cardsByFocus[higherFocus];

      if (lowerCards.length > 0 && higherCards.length > 0) {
        const avgLowerPower = lowerCards.reduce((sum, c) => sum + c.theoreticalPower, 0) / lowerCards.length;
        const avgHigherPower = higherCards.reduce((sum, c) => sum + c.theoreticalPower, 0) / higherCards.length;

        if (avgHigherPower >= avgLowerPower) {
          correlationScore++;
        }
        correlationTests++;
      }
    }

    const correlationPercentage = correlationTests > 0 ? (correlationScore / correlationTests) * 100 : 0;

    const avgPowerEfficiency = powerAnalysis.powerCorrelation.length > 0 ?
      powerAnalysis.powerCorrelation.reduce((sum, c) => sum + c.powerEfficiency, 0) / powerAnalysis.powerCorrelation.length : 0;

    console.log(`Power Level Analysis:
      - Focus levels tested: ${focusLevels.join(', ')}
      - Power correlation: ${correlationScore}/${correlationTests} (${correlationPercentage.toFixed(1)}%)
      - Average power efficiency: ${avgPowerEfficiency.toFixed(2)} (target: ~1.0)
      - Outliers found: ${powerAnalysis.outliers.length}
      - Power correlation data: ${JSON.stringify(powerAnalysis.powerCorrelation, null, 2)}
      - Outliers: ${JSON.stringify(powerAnalysis.outliers, null, 2)}`);

    // Power consistency assertions
    expect(correlationPercentage).toBeGreaterThan(60); // 60%+ proper correlation
    expect(avgPowerEfficiency).toBeGreaterThan(0.7); // Reasonable efficiency floor
    expect(avgPowerEfficiency).toBeLessThan(1.5); // Not overpowered
    expect(powerAnalysis.outliers.filter(o => o.type === 'overpowered').length).toBeLessThan(2); // Minimal overpowered cards
  });

  test('Resource Economy: Generation and spending should feel balanced', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const economyAnalysis = {
      focusGeneration: [],
      focusSpending: [],
      momentumGeneration: [],
      doubtAccumulation: [],
      resourceBalance: [],
      economicPressure: []
    };

    for (let turn = 0; turn < 8; turn++) {
      const beforeState = await gameHelpers.getGameState();

      // Record resource state before turn
      economyAnalysis.focusGeneration.push(beforeState.conversationState.focus.available);
      economyAnalysis.momentumGeneration.push(beforeState.conversationState.momentum);
      economyAnalysis.doubtAccumulation.push(beforeState.conversationState.doubt);

      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();
      if (cards.length === 0) break;

      // Analyze economic choices
      const affordableCards = cards.filter(card => card.focus <= beforeState.conversationState.focus.available);
      const expensiveCards = cards.filter(card => card.focus > beforeState.conversationState.focus.available);

      const economicPressure = {
        turn: turn + 1,
        focusConstraint: expensiveCards.length / Math.max(cards.length, 1),
        doubtPressure: beforeState.conversationState.doubt / 10,
        momentumOpportunity: beforeState.conversationState.momentum >= 5 ? 1 : 0,
        totalPressure: 0
      };

      economicPressure.totalPressure = economicPressure.focusConstraint + economicPressure.doubtPressure + economicPressure.momentumOpportunity;
      economyAnalysis.economicPressure.push(economicPressure);

      // Play a card and analyze economic impact
      if (affordableCards.length > 0) {
        const chosenCard = affordableCards[0];
        await gameHelpers.selectCard(chosenCard.name);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();

        const afterState = await gameHelpers.getGameState();

        // Record resource changes
        const focusSpent = beforeState.conversationState.focus.available - afterState.conversationState.focus.available;
        const momentumGain = afterState.conversationState.momentum - beforeState.conversationState.momentum;
        const doubtChange = afterState.conversationState.doubt - beforeState.conversationState.doubt;

        economyAnalysis.focusSpending.push(focusSpent);

        const resourceBalance = {
          turn: turn + 1,
          focusSpent: focusSpent,
          momentumGain: momentumGain,
          doubtChange: doubtChange,
          efficiency: momentumGain / Math.max(focusSpent, 1),
          cardName: chosenCard.name
        };

        economyAnalysis.resourceBalance.push(resourceBalance);

        await gameHelpers.takeScreenshot(`economy_turn_${turn + 1}_focus_${focusSpent}_momentum_${momentumGain}`);
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    // Analyze economic balance
    const avgFocusAvailable = economyAnalysis.focusGeneration.reduce((a, b) => a + b, 0) / economyAnalysis.focusGeneration.length;
    const avgFocusSpent = economyAnalysis.focusSpending.reduce((a, b) => a + b, 0) / economyAnalysis.focusSpending.length;
    const avgMomentumGain = economyAnalysis.resourceBalance.reduce((sum, b) => sum + b.momentumGain, 0) / economyAnalysis.resourceBalance.length;
    const avgEfficiency = economyAnalysis.resourceBalance.reduce((sum, b) => sum + b.efficiency, 0) / economyAnalysis.resourceBalance.length;

    const focusUtilization = avgFocusSpent / avgFocusAvailable;
    const avgEconomicPressure = economyAnalysis.economicPressure.reduce((sum, p) => sum + p.totalPressure, 0) / economyAnalysis.economicPressure.length;

    const highPressureTurns = economyAnalysis.economicPressure.filter(p => p.totalPressure >= 2).length;
    const lowPressureTurns = economyAnalysis.economicPressure.filter(p => p.totalPressure <= 0.5).length;

    console.log(`Resource Economy Analysis:
      - Average focus available: ${avgFocusAvailable.toFixed(1)}
      - Average focus spent: ${avgFocusSpent.toFixed(1)}
      - Focus utilization: ${(focusUtilization * 100).toFixed(1)}%
      - Average momentum gain: ${avgMomentumGain.toFixed(1)}
      - Average efficiency: ${avgEfficiency.toFixed(2)}
      - Average economic pressure: ${avgEconomicPressure.toFixed(2)}
      - High pressure turns: ${highPressureTurns}/${economyAnalysis.economicPressure.length}
      - Low pressure turns: ${lowPressureTurns}/${economyAnalysis.economicPressure.length}
      - Resource balance data: ${JSON.stringify(economyAnalysis.resourceBalance, null, 2)}`);

    // Economic balance assertions
    expect(focusUtilization).toBeGreaterThan(0.6); // 60%+ focus utilization
    expect(focusUtilization).toBeLessThan(0.95); // Not too constrained
    expect(avgEfficiency).toBeGreaterThan(0.8); // Reasonable efficiency
    expect(avgEconomicPressure).toBeGreaterThan(1.0); // Meaningful economic decisions
    expect(avgEconomicPressure).toBeLessThan(2.5); // Not overwhelming
    expect(highPressureTurns / economyAnalysis.economicPressure.length).toBeGreaterThan(0.2); // 20%+ high pressure
  });

  test('Game Length: Conversations should feel complete but not dragged out', async ({ page }) => {
    const lengthAnalysis = {
      conversationAttempts: [],
      averageTurns: 0,
      averageDuration: 0,
      completionReasons: {}
    };

    // Test multiple conversation lengths
    for (let attempt = 0; attempt < 3; attempt++) {
      await page.goto('/');
      await gameHelpers.waitForGameReady();
      await gameHelpers.startNewGame();
      await gameHelpers.startConversation('Elena');

      const startTime = Date.now();
      let turnCount = 0;
      let momentumProgression = [];
      let doubtProgression = [];

      for (let turn = 0; turn < 15; turn++) { // Cap at 15 turns
        const gameState = await gameHelpers.getGameState();
        turnCount++;

        momentumProgression.push(gameState.conversationState.momentum);
        doubtProgression.push(gameState.conversationState.doubt);

        await gameHelpers.executeListen();
        await gameHelpers.waitForNarrativeGeneration();

        const cards = await gameHelpers.getHandCards();
        if (cards.length === 0) break;

        // Play cards strategically to test natural conversation flow
        const playableCards = cards.filter(card =>
          card.isSelectable && card.focus <= gameState.conversationState.focus.available
        );

        if (playableCards.length > 0) {
          // Choose based on game state
          let chosenCard = playableCards[0];

          if (gameState.conversationState.doubt > 6) {
            // Prioritize doubt management
            const sootheCards = playableCards.filter(card => card.effect?.includes('Soothe'));
            if (sootheCards.length > 0) chosenCard = sootheCards[0];
          } else if (gameState.conversationState.momentum < 5) {
            // Build momentum
            const strikeCards = playableCards.filter(card => card.effect?.includes('Strike'));
            if (strikeCards.length > 0) chosenCard = strikeCards[0];
          }

          await gameHelpers.selectCard(chosenCard.name);
          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();
        }

        if (await gameHelpers.isConversationExhausted()) {
          const exhaustionReason = await gameHelpers.getExhaustionReason();
          const reasonKey = exhaustionReason ? 'success' : 'failure';

          if (!lengthAnalysis.completionReasons[reasonKey]) {
            lengthAnalysis.completionReasons[reasonKey] = 0;
          }
          lengthAnalysis.completionReasons[reasonKey]++;
          break;
        }
      }

      const endTime = Date.now();
      const duration = endTime - startTime;

      lengthAnalysis.conversationAttempts.push({
        attempt: attempt + 1,
        turns: turnCount,
        duration: duration,
        finalMomentum: momentumProgression[momentumProgression.length - 1] || 0,
        finalDoubt: doubtProgression[doubtProgression.length - 1] || 0,
        momentumProgression: momentumProgression,
        doubtProgression: doubtProgression
      });

      await gameHelpers.takeScreenshot(`conversation_length_attempt_${attempt + 1}_turns_${turnCount}`);
    }

    // Analyze conversation length quality
    lengthAnalysis.averageTurns = lengthAnalysis.conversationAttempts.reduce((sum, c) => sum + c.turns, 0) / lengthAnalysis.conversationAttempts.length;
    lengthAnalysis.averageDuration = lengthAnalysis.conversationAttempts.reduce((sum, c) => sum + c.duration, 0) / lengthAnalysis.conversationAttempts.length;

    const turnRange = {
      min: Math.min(...lengthAnalysis.conversationAttempts.map(c => c.turns)),
      max: Math.max(...lengthAnalysis.conversationAttempts.map(c => c.turns))
    };

    // Analyze progression quality
    let progressionQuality = 0;
    for (const attempt of lengthAnalysis.conversationAttempts) {
      const momentumIncrease = attempt.finalMomentum > attempt.momentumProgression[0];
      const reasonableLength = attempt.turns >= 5 && attempt.turns <= 12;
      const progressionVisible = attempt.momentumProgression.some((m, i) => i > 0 && m > attempt.momentumProgression[i - 1]);

      if (momentumIncrease && reasonableLength && progressionVisible) {
        progressionQuality++;
      }
    }

    const progressionPercentage = (progressionQuality / lengthAnalysis.conversationAttempts.length) * 100;

    console.log(`Game Length Analysis:
      - Average turns: ${lengthAnalysis.averageTurns.toFixed(1)}
      - Turn range: ${turnRange.min} to ${turnRange.max}
      - Average duration: ${(lengthAnalysis.averageDuration / 1000 / 60).toFixed(1)} minutes
      - Progression quality: ${progressionQuality}/${lengthAnalysis.conversationAttempts.length} (${progressionPercentage.toFixed(1)}%)
      - Completion reasons: ${JSON.stringify(lengthAnalysis.completionReasons)}
      - Conversation details: ${JSON.stringify(lengthAnalysis.conversationAttempts, null, 2)}`);

    // Game length assertions
    expect(lengthAnalysis.averageTurns).toBeGreaterThan(5); // Minimum meaningful length
    expect(lengthAnalysis.averageTurns).toBeLessThan(12); // Not too long
    expect(turnRange.max - turnRange.min).toBeLessThan(8); // Consistent length
    expect(progressionPercentage).toBeGreaterThan(60); // 60%+ should feel complete
    expect(lengthAnalysis.averageDuration).toBeLessThan(10 * 60 * 1000); // <10 minutes
  });
});