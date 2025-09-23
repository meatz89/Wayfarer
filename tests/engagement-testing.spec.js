/**
 * Engagement Testing - Meaningful Choices and Player Investment
 * Evaluates if the card system creates engaging, non-autopilot gameplay
 */

const { test, expect } = require('@playwright/test');
const { GameHelpers } = require('./utils/game-helpers');

test.describe('Engagement Testing', () => {
  let gameHelpers;

  test.beforeEach(async ({ page }) => {
    gameHelpers = new GameHelpers(page);
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();
  });

  test('Meaningful Choices: No autopilot turns with obvious plays', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const choiceAnalysis = {
      totalDecisionPoints: 0,
      trivialChoices: 0,
      meaningfulChoices: 0,
      complexChoices: 0,
      decisionDetails: []
    };

    for (let turn = 0; turn < 8; turn++) {
      const gameState = await gameHelpers.getGameState();
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();
      if (cards.length === 0) break;

      choiceAnalysis.totalDecisionPoints++;

      // Analyze choice complexity
      const playableCards = cards.filter(card => card.isSelectable && card.focus <= gameState.conversationState.focus.available);

      if (playableCards.length <= 1) {
        // Trivial choice - only one option
        choiceAnalysis.trivialChoices++;
        choiceAnalysis.decisionDetails.push({
          turn: turn + 1,
          type: 'trivial',
          availableCards: playableCards.length,
          reason: 'Only one playable card'
        });
      } else {
        // Analyze if choices are meaningfully different
        const focusRanges = new Set(playableCards.map(card => Math.floor(card.focus / 2))); // Group by focus tier
        const effectTypes = new Set(playableCards.map(card => {
          if (card.effect?.includes('Strike')) return 'momentum';
          if (card.effect?.includes('Soothe')) return 'doubt_management';
          if (card.effect?.includes('Focusing')) return 'setup';
          if (card.effect?.includes('Advancing')) return 'progression';
          return 'other';
        }));

        const categories = new Set(playableCards.map(card => card.category));

        if (focusRanges.size >= 2 && effectTypes.size >= 2) {
          choiceAnalysis.complexChoices++;
          choiceAnalysis.decisionDetails.push({
            turn: turn + 1,
            type: 'complex',
            availableCards: playableCards.length,
            focusRanges: Array.from(focusRanges),
            effectTypes: Array.from(effectTypes),
            categories: Array.from(categories),
            reason: 'Multiple viable strategies with different focus costs and effects'
          });
        } else {
          choiceAnalysis.meaningfulChoices++;
          choiceAnalysis.decisionDetails.push({
            turn: turn + 1,
            type: 'meaningful',
            availableCards: playableCards.length,
            limitingFactor: focusRanges.size < 2 ? 'similar_focus_costs' : 'similar_effect_types'
          });
        }
      }

      // Take screenshot of meaningful/complex decisions
      if (playableCards.length > 1) {
        await gameHelpers.takeScreenshot(`meaningful_choice_turn_${turn + 1}_options_${playableCards.length}`);
      }

      // Make choice and continue
      if (playableCards.length > 0) {
        // For testing, choose highest focus card when possible to stress-test the system
        const chosenCard = playableCards.reduce((best, current) =>
          current.focus > best.focus ? current : best
        );
        await gameHelpers.selectCard(chosenCard.name);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    const meaningfulPercentage = ((choiceAnalysis.meaningfulChoices + choiceAnalysis.complexChoices) / choiceAnalysis.totalDecisionPoints) * 100;
    const complexPercentage = (choiceAnalysis.complexChoices / choiceAnalysis.totalDecisionPoints) * 100;
    const trivialPercentage = (choiceAnalysis.trivialChoices / choiceAnalysis.totalDecisionPoints) * 100;

    console.log(`Meaningful Choices Analysis:
      - Total decision points: ${choiceAnalysis.totalDecisionPoints}
      - Trivial choices: ${choiceAnalysis.trivialChoices} (${trivialPercentage.toFixed(1)}%)
      - Meaningful choices: ${choiceAnalysis.meaningfulChoices} (${((choiceAnalysis.meaningfulChoices / choiceAnalysis.totalDecisionPoints) * 100).toFixed(1)}%)
      - Complex choices: ${choiceAnalysis.complexChoices} (${complexPercentage.toFixed(1)}%)
      - Overall engagement: ${meaningfulPercentage.toFixed(1)}% non-trivial decisions
      - Decision breakdown: ${JSON.stringify(choiceAnalysis.decisionDetails, null, 2)}`);

    // Engagement assertions
    expect(trivialPercentage).toBeLessThan(30); // <30% trivial choices
    expect(meaningfulPercentage).toBeGreaterThan(70); // >70% meaningful decisions
    expect(complexPercentage).toBeGreaterThan(20); // >20% complex strategic decisions
  });

  test('Strategic Variety: Multiple viable approaches to same goal', async ({ page }) => {
    // Test multiple strategic approaches in separate conversation runs
    const strategicApproaches = {
      aggressive: { wins: 0, momentum: [], doubt: [], turns: 0 },
      defensive: { wins: 0, momentum: [], doubt: [], turns: 0 },
      balanced: { wins: 0, momentum: [], doubt: [], turns: 0 }
    };

    // Test each approach
    for (const [approachName, approach] of Object.entries(strategicApproaches)) {
      // Reset for each approach
      await page.goto('/');
      await gameHelpers.waitForGameReady();
      await gameHelpers.startNewGame();
      await gameHelpers.startConversation('Elena');

      console.log(`Testing ${approachName} approach...`);

      for (let turn = 0; turn < 8; turn++) {
        const gameState = await gameHelpers.getGameState();
        approach.turns++;

        // Record state
        approach.momentum.push(gameState.conversationState.momentum);
        approach.doubt.push(gameState.conversationState.doubt);

        await gameHelpers.executeListen();
        await gameHelpers.waitForNarrativeGeneration();

        const cards = await gameHelpers.getHandCards();
        if (cards.length === 0) break;

        const playableCards = cards.filter(card =>
          card.isSelectable && card.focus <= gameState.conversationState.focus.available
        );

        if (playableCards.length === 0) break;

        // Apply strategic approach
        let chosenCard = null;

        if (approachName === 'aggressive') {
          // Prioritize high momentum, high focus cards
          const aggressiveCards = playableCards.filter(card =>
            card.effect?.includes('Strike') || card.focus >= 3
          );
          chosenCard = aggressiveCards.length > 0 ?
            aggressiveCards.reduce((best, current) => current.focus > best.focus ? current : best) :
            playableCards[0];

        } else if (approachName === 'defensive') {
          // Prioritize doubt management and low-risk plays
          const defensiveCards = playableCards.filter(card =>
            card.effect?.includes('Soothe') || card.focus <= 2
          );
          chosenCard = defensiveCards.length > 0 ?
            defensiveCards.reduce((best, current) => current.focus < best.focus ? current : best) :
            playableCards.reduce((best, current) => current.focus < best.focus ? current : best);

        } else if (approachName === 'balanced') {
          // Balanced approach - moderate focus, consider game state
          if (gameState.conversationState.doubt > 5) {
            // Switch to defensive
            const doubtCards = playableCards.filter(card => card.effect?.includes('Soothe'));
            chosenCard = doubtCards.length > 0 ? doubtCards[0] : playableCards[0];
          } else {
            // Moderate aggression
            const balancedCards = playableCards.filter(card => card.focus >= 2 && card.focus <= 4);
            chosenCard = balancedCards.length > 0 ? balancedCards[0] : playableCards[0];
          }
        }

        if (chosenCard) {
          await gameHelpers.selectCard(chosenCard.name);
          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();
        }

        if (await gameHelpers.isConversationExhausted()) {
          const exhaustionReason = await gameHelpers.getExhaustionReason();
          if (exhaustionReason && !exhaustionReason.includes('failed')) {
            approach.wins++;
          }
          break;
        }
      }

      await gameHelpers.takeScreenshot(`strategic_approach_${approachName}_final`);
    }

    // Analyze strategic variety
    const approaches = Object.entries(strategicApproaches);
    const maxMomentum = Math.max(...approaches.map(([name, data]) => Math.max(...data.momentum)));
    const minMomentum = Math.min(...approaches.map(([name, data]) => Math.min(...data.momentum)));
    const momentumVariation = maxMomentum - minMomentum;

    const avgDoubts = approaches.map(([name, data]) => ({
      name,
      avgDoubt: data.doubt.reduce((a, b) => a + b, 0) / data.doubt.length
    }));

    console.log(`Strategic Variety Analysis:
      - Momentum range across approaches: ${minMomentum} to ${maxMomentum} (variation: ${momentumVariation})
      - Average doubt by approach: ${avgDoubts.map(a => `${a.name}: ${a.avgDoubt.toFixed(1)}`).join(', ')}
      - Wins by approach: ${approaches.map(([name, data]) => `${name}: ${data.wins}`).join(', ')}
      - Turn counts: ${approaches.map(([name, data]) => `${name}: ${data.turns}`).join(', ')}`);

    // Strategic variety assertions
    expect(momentumVariation).toBeGreaterThan(3); // Approaches should produce different outcomes
    expect(approaches.filter(([name, data]) => data.wins > 0).length).toBeGreaterThan(1); // Multiple approaches should be viable
  });

  test('Comeback Mechanics: Losing positions can be recovered', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const comebackAnalysis = {
      highDoubtSituations: [],
      recoveryAttempts: [],
      successfulComebacks: 0,
      totalRecoveryTurns: 0
    };

    let inComebackMode = false;
    let comebackStartDoubt = 0;

    for (let turn = 0; turn < 12; turn++) {
      const gameState = await gameHelpers.getGameState();
      const currentDoubt = gameState.conversationState.doubt;

      // Detect high-doubt situations requiring comeback
      if (currentDoubt >= 7 && !inComebackMode) {
        inComebackMode = true;
        comebackStartDoubt = currentDoubt;
        comebackAnalysis.highDoubtSituations.push({
          turn: turn + 1,
          doubt: currentDoubt,
          momentum: gameState.conversationState.momentum,
          focus: gameState.conversationState.focus.available
        });

        await gameHelpers.takeScreenshot(`comeback_situation_turn_${turn + 1}_doubt_${currentDoubt}`);
      }

      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();
      if (cards.length === 0) break;

      if (inComebackMode) {
        // Prioritize recovery cards
        const recoveryCards = cards.filter(card =>
          card.effect?.includes('Soothe') ||
          card.effect?.includes('reduce doubt') ||
          (card.focus <= 2 && card.effect?.includes('Focusing'))
        );

        let chosenCard = null;
        if (recoveryCards.length > 0) {
          // Choose best recovery card
          chosenCard = recoveryCards.reduce((best, current) => {
            const bestScore = (best.effect?.includes('Soothe') ? 3 : 0) + (best.focus <= 1 ? 2 : 0);
            const currentScore = (current.effect?.includes('Soothe') ? 3 : 0) + (current.focus <= 1 ? 2 : 0);
            return currentScore > bestScore ? current : best;
          });
        } else {
          // Choose safest available card
          const safeCards = cards.filter(card => card.focus <= gameState.conversationState.focus.available);
          chosenCard = safeCards.length > 0 ?
            safeCards.reduce((best, current) => current.focus < best.focus ? current : best) :
            cards[0];
        }

        if (chosenCard) {
          comebackAnalysis.recoveryAttempts.push({
            turn: turn + 1,
            cardName: chosenCard.name,
            cardEffect: chosenCard.effect,
            cardFocus: chosenCard.focus,
            doubtBefore: currentDoubt
          });

          await gameHelpers.selectCard(chosenCard.name);
          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();

          comebackAnalysis.totalRecoveryTurns++;

          // Check if comeback was successful
          const afterState = await gameHelpers.getGameState();
          const newDoubt = afterState.conversationState.doubt;

          if (newDoubt < comebackStartDoubt - 2) {
            comebackAnalysis.successfulComebacks++;
            inComebackMode = false;
            await gameHelpers.takeScreenshot(`successful_comeback_turn_${turn + 1}_doubt_reduced_to_${newDoubt}`);
          }
        }
      } else {
        // Normal play - potentially create difficult situations for testing
        const aggressiveCards = cards.filter(card => card.focus >= 4);
        const chosenCard = aggressiveCards.length > 0 ? aggressiveCards[0] : cards[0];

        if (chosenCard && chosenCard.focus <= gameState.conversationState.focus.available) {
          await gameHelpers.selectCard(chosenCard.name);
          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();
        }
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    const comebackSuccessRate = comebackAnalysis.recoveryAttempts.length > 0 ?
      (comebackAnalysis.successfulComebacks / comebackAnalysis.highDoubtSituations.length) * 100 : 0;

    console.log(`Comeback Mechanics Analysis:
      - High doubt situations encountered: ${comebackAnalysis.highDoubtSituations.length}
      - Recovery attempts made: ${comebackAnalysis.recoveryAttempts.length}
      - Successful comebacks: ${comebackAnalysis.successfulComebacks}
      - Comeback success rate: ${comebackSuccessRate.toFixed(1)}%
      - Total recovery turns: ${comebackAnalysis.totalRecoveryTurns}
      - High doubt situations: ${JSON.stringify(comebackAnalysis.highDoubtSituations, null, 2)}
      - Recovery attempts: ${JSON.stringify(comebackAnalysis.recoveryAttempts, null, 2)}`);

    // Comeback assertions
    expect(comebackAnalysis.highDoubtSituations.length).toBeGreaterThan(0); // Should encounter challenging situations
    if (comebackAnalysis.highDoubtSituations.length > 0) {
      expect(comebackSuccessRate).toBeGreaterThan(30); // 30%+ comeback success rate
    }
    expect(comebackAnalysis.recoveryAttempts.length).toBeGreaterThan(0); // Recovery options should exist
  });

  test('Tension Maintenance: Doubt pressure creates urgency without frustration', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const tensionAnalysis = {
      tensionCurve: [],
      stagnantTurns: 0,
      overwhelmingTurns: 0,
      sweetSpotTurns: 0,
      tensionEvents: []
    };

    let previousDoubt = 0;
    let previousMomentum = 0;

    for (let turn = 0; turn < 10; turn++) {
      const gameState = await gameHelpers.getGameState();
      const currentDoubt = gameState.conversationState.doubt;
      const currentMomentum = gameState.conversationState.momentum;
      const focusAvailable = gameState.conversationState.focus.available;

      // Calculate tension level
      let tensionLevel = 0;

      // Doubt pressure (0-3 points)
      if (currentDoubt >= 8) tensionLevel += 3;
      else if (currentDoubt >= 6) tensionLevel += 2;
      else if (currentDoubt >= 4) tensionLevel += 1;

      // Focus pressure (0-2 points)
      if (focusAvailable <= 1) tensionLevel += 2;
      else if (focusAvailable <= 2) tensionLevel += 1;

      // Momentum opportunity pressure (0-1 point)
      if (currentMomentum >= 6) tensionLevel += 1;

      // Progress pressure (0-1 point)
      const doubtChange = currentDoubt - previousDoubt;
      const momentumChange = currentMomentum - previousMomentum;
      if (doubtChange > 0 && momentumChange <= 0) tensionLevel += 1;

      tensionAnalysis.tensionCurve.push({
        turn: turn + 1,
        doubt: currentDoubt,
        momentum: currentMomentum,
        focus: focusAvailable,
        tensionLevel: tensionLevel
      });

      // Categorize tension quality
      if (tensionLevel <= 1) {
        tensionAnalysis.stagnantTurns++;
      } else if (tensionLevel >= 6) {
        tensionAnalysis.overwhelmingTurns++;
        tensionAnalysis.tensionEvents.push({
          turn: turn + 1,
          type: 'overwhelming',
          tensionLevel,
          factors: { doubt: currentDoubt, focus: focusAvailable }
        });
      } else {
        tensionAnalysis.sweetSpotTurns++;
      }

      // Record significant tension changes
      if (turn > 0) {
        const previousTension = tensionAnalysis.tensionCurve[turn - 1].tensionLevel;
        if (Math.abs(tensionLevel - previousTension) >= 2) {
          tensionAnalysis.tensionEvents.push({
            turn: turn + 1,
            type: tensionLevel > previousTension ? 'spike' : 'relief',
            from: previousTension,
            to: tensionLevel
          });
        }
      }

      // Screenshot high-tension moments
      if (tensionLevel >= 4) {
        await gameHelpers.takeScreenshot(`tension_moment_turn_${turn + 1}_level_${tensionLevel}`);
      }

      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();
      if (cards.length > 0) {
        // Choose card that maintains interesting tension
        let chosenCard = cards[0];

        if (tensionLevel <= 2) {
          // Low tension - increase stakes with higher focus cards
          const riskCards = cards.filter(card => card.focus >= 3);
          if (riskCards.length > 0) chosenCard = riskCards[0];
        } else if (tensionLevel >= 5) {
          // High tension - consider relief options
          const reliefCards = cards.filter(card => card.effect?.includes('Soothe') || card.focus <= 2);
          if (reliefCards.length > 0) chosenCard = reliefCards[0];
        }

        await gameHelpers.selectCard(chosenCard.name);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();
      }

      previousDoubt = currentDoubt;
      previousMomentum = currentMomentum;

      if (await gameHelpers.isConversationExhausted()) break;
    }

    const totalTurns = tensionAnalysis.tensionCurve.length;
    const avgTension = tensionAnalysis.tensionCurve.reduce((sum, t) => sum + t.tensionLevel, 0) / totalTurns;
    const stagnantPercentage = (tensionAnalysis.stagnantTurns / totalTurns) * 100;
    const overwhelmingPercentage = (tensionAnalysis.overwhelmingTurns / totalTurns) * 100;
    const sweetSpotPercentage = (tensionAnalysis.sweetSpotTurns / totalTurns) * 100;

    console.log(`Tension Maintenance Analysis:
      - Average tension level: ${avgTension.toFixed(2)} (target: 2-4)
      - Stagnant turns: ${tensionAnalysis.stagnantTurns} (${stagnantPercentage.toFixed(1)}%)
      - Sweet spot turns: ${tensionAnalysis.sweetSpotTurns} (${sweetSpotPercentage.toFixed(1)}%)
      - Overwhelming turns: ${tensionAnalysis.overwhelmingTurns} (${overwhelmingPercentage.toFixed(1)}%)
      - Tension events: ${tensionAnalysis.tensionEvents.length}
      - Tension curve: ${JSON.stringify(tensionAnalysis.tensionCurve, null, 2)}`);

    // Tension quality assertions
    expect(avgTension).toBeGreaterThan(1.5); // Sufficient tension
    expect(avgTension).toBeLessThan(4.5); // Not overwhelming
    expect(stagnantPercentage).toBeLessThan(40); // <40% low-tension turns
    expect(overwhelmingPercentage).toBeLessThan(20); // <20% overwhelming turns
    expect(sweetSpotPercentage).toBeGreaterThan(40); // >40% good tension turns
  });
});