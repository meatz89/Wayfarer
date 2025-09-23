/**
 * Learning Curve Validation Tests
 * Evaluates if the card system provides clear learning progression
 */

const { test, expect } = require('@playwright/test');
const { GameHelpers } = require('./utils/game-helpers');

test.describe('Learning Curve Validation', () => {
  let gameHelpers;

  test.beforeEach(async ({ page }) => {
    gameHelpers = new GameHelpers(page);
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();
  });

  test('Card Effects Clarity: All cards have clear, understandable effects', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const cardClarityAnalysis = {
      totalCards: 0,
      cardsWithClearEffects: 0,
      cardsWithComplexEffects: 0,
      effectTypes: new Set(),
      clarityIssues: []
    };

    // Draw cards and analyze their clarity
    for (let turn = 0; turn < 5; turn++) {
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();

      for (const card of cards) {
        cardClarityAnalysis.totalCards++;

        // Check effect clarity
        const effect = card.effect || '';
        const hasEffect = effect.length > 0;
        const hasJargon = /advanced|complex|specialized/.test(effect.toLowerCase());
        const hasNumbers = /\d+/.test(effect);
        const hasActionVerb = /strike|soothe|advance|focus|thread/i.test(effect);

        if (hasEffect && hasActionVerb) {
          cardClarityAnalysis.cardsWithClearEffects++;
        }

        if (hasJargon || (!hasNumbers && card.focus > 3)) {
          cardClarityAnalysis.cardsWithComplexEffects++;
          cardClarityAnalysis.clarityIssues.push({
            cardName: card.name,
            issue: hasJargon ? 'Contains jargon' : 'High focus without numeric clarity'
          });
        }

        // Extract effect type
        if (effect.includes('Strike')) cardClarityAnalysis.effectTypes.add('Strike');
        if (effect.includes('Soothe')) cardClarityAnalysis.effectTypes.add('Soothe');
        if (effect.includes('Advancing')) cardClarityAnalysis.effectTypes.add('Advancing');
        if (effect.includes('Focusing')) cardClarityAnalysis.effectTypes.add('Focusing');
        if (effect.includes('Threading')) cardClarityAnalysis.effectTypes.add('Threading');
      }

      // Take screenshot of hand for analysis
      await gameHelpers.takeScreenshot(`card_clarity_turn_${turn + 1}`);

      // Play a card to continue
      if (cards.length > 0) {
        await gameHelpers.selectCard(0);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    const clarityPercentage = (cardClarityAnalysis.cardsWithClearEffects / cardClarityAnalysis.totalCards) * 100;

    console.log(`Card Clarity Analysis:
      - Total cards analyzed: ${cardClarityAnalysis.totalCards}
      - Cards with clear effects: ${cardClarityAnalysis.cardsWithClearEffects} (${clarityPercentage.toFixed(1)}%)
      - Complex/unclear cards: ${cardClarityAnalysis.cardsWithComplexEffects}
      - Effect types encountered: ${Array.from(cardClarityAnalysis.effectTypes).join(', ')}
      - Clarity issues: ${JSON.stringify(cardClarityAnalysis.clarityIssues, null, 2)}`);

    // Clarity assertions
    expect(clarityPercentage).toBeGreaterThan(80); // 80%+ cards should have clear effects
    expect(cardClarityAnalysis.effectTypes.size).toBeGreaterThan(3); // Multiple effect types for learning
    expect(cardClarityAnalysis.clarityIssues.length).toBeLessThan(cardClarityAnalysis.totalCards * 0.2); // <20% problematic
  });

  test('Scaling Formula Transparency: Players should understand why effects vary', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const scalingAnalysis = {
      cardsWithScaling: [],
      scalingTypes: new Set(),
      transparencyScore: 0,
      predictabilityTests: []
    };

    for (let turn = 0; turn < 6; turn++) {
      const beforeState = await gameHelpers.getGameState();
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();

      // Look for cards with scaling effects
      const scalingCards = cards.filter(card => card.hasScaling ||
        card.effect?.includes('spend') ||
        card.effect?.includes('hand') ||
        card.effect?.includes('doubt') ||
        card.effect?.includes('cards'));

      for (const card of scalingCards) {
        scalingAnalysis.cardsWithScaling.push({
          name: card.name,
          effect: card.effect,
          gameState: {
            handSize: beforeState.conversationState.handSize,
            momentum: beforeState.conversationState.momentum,
            doubt: beforeState.conversationState.doubt,
            focus: beforeState.conversationState.focus.available
          }
        });

        // Identify scaling type
        if (card.effect.includes('hand')) scalingAnalysis.scalingTypes.add('HandSize');
        if (card.effect.includes('doubt')) scalingAnalysis.scalingTypes.add('Doubt');
        if (card.effect.includes('spend')) scalingAnalysis.scalingTypes.add('Resource');
        if (card.effect.includes('discard')) scalingAnalysis.scalingTypes.add('CardManagement');
      }

      // Test predictability by using a scaling card if available
      if (scalingCards.length > 0) {
        const testCard = scalingCards[0];
        await gameHelpers.selectCard(testCard.name);
        await gameHelpers.takeScreenshot(`scaling_test_${testCard.name}_turn_${turn + 1}`);

        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();

        const afterState = await gameHelpers.getGameState();

        // Record predictability test
        scalingAnalysis.predictabilityTests.push({
          cardName: testCard.name,
          beforeMomentum: beforeState.conversationState.momentum,
          afterMomentum: afterState.conversationState.momentum,
          momentumChange: afterState.conversationState.momentum - beforeState.conversationState.momentum,
          beforeDoubt: beforeState.conversationState.doubt,
          afterDoubt: afterState.conversationState.doubt,
          doubtChange: afterState.conversationState.doubt - beforeState.conversationState.doubt
        });

        // Score transparency based on whether change makes sense
        const expectedPositive = testCard.effect.includes('Strike') || testCard.effect.includes('momentum');
        const actualPositive = afterState.conversationState.momentum > beforeState.conversationState.momentum;
        if (expectedPositive === actualPositive) {
          scalingAnalysis.transparencyScore++;
        }
      } else if (cards.length > 0) {
        await gameHelpers.selectCard(0);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    const transparencyPercentage = scalingAnalysis.predictabilityTests.length > 0 ?
      (scalingAnalysis.transparencyScore / scalingAnalysis.predictabilityTests.length) * 100 : 0;

    console.log(`Scaling Transparency Analysis:
      - Cards with scaling effects: ${scalingAnalysis.cardsWithScaling.length}
      - Scaling types found: ${Array.from(scalingAnalysis.scalingTypes).join(', ')}
      - Predictability tests: ${scalingAnalysis.predictabilityTests.length}
      - Transparency score: ${scalingAnalysis.transparencyScore}/${scalingAnalysis.predictabilityTests.length} (${transparencyPercentage.toFixed(1)}%)
      - Test results: ${JSON.stringify(scalingAnalysis.predictabilityTests, null, 2)}`);

    // Transparency assertions
    expect(scalingAnalysis.cardsWithScaling.length).toBeGreaterThan(0); // Scaling cards should exist
    expect(scalingAnalysis.scalingTypes.size).toBeGreaterThan(1); // Multiple scaling types
    if (scalingAnalysis.predictabilityTests.length > 0) {
      expect(transparencyPercentage).toBeGreaterThan(70); // 70%+ predictable outcomes
    }
  });

  test('Strategic Feedback: Results teach players about better strategies', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const feedbackAnalysis = {
      strategicLessons: [],
      outcomePatterns: [],
      learningOpportunities: 0
    };

    let previousMomentum = 0;
    let previousDoubt = 0;

    for (let turn = 0; turn < 8; turn++) {
      const beforeState = await gameHelpers.getGameState();
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();

      // Categorize available strategic choices
      const strategicChoices = {
        aggressive: cards.filter(card => card.focus >= 4 || card.effect?.includes('Strike')),
        defensive: cards.filter(card => card.effect?.includes('Soothe') || card.focus <= 1),
        efficiency: cards.filter(card => card.effect?.includes('Focusing') || card.name?.includes('mental_reset')),
        scaling: cards.filter(card => card.hasScaling || card.effect?.includes('spend'))
      };

      // Choose strategy based on game state
      let chosenStrategy = 'none';
      let chosenCard = null;

      if (beforeState.conversationState.doubt > 6 && strategicChoices.defensive.length > 0) {
        chosenStrategy = 'defensive';
        chosenCard = strategicChoices.defensive[0];
      } else if (beforeState.conversationState.focus.available >= 4 && strategicChoices.aggressive.length > 0) {
        chosenStrategy = 'aggressive';
        chosenCard = strategicChoices.aggressive[0];
      } else if (strategicChoices.efficiency.length > 0) {
        chosenStrategy = 'efficiency';
        chosenCard = strategicChoices.efficiency[0];
      } else if (cards.length > 0) {
        chosenStrategy = 'fallback';
        chosenCard = cards[0];
      }

      if (chosenCard) {
        await gameHelpers.selectCard(chosenCard.name);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();

        const afterState = await gameHelpers.getGameState();

        // Analyze strategic outcome
        const outcome = {
          strategy: chosenStrategy,
          cardName: chosenCard.name,
          cardFocus: chosenCard.focus,
          momentumChange: afterState.conversationState.momentum - beforeState.conversationState.momentum,
          doubtChange: afterState.conversationState.doubt - beforeState.conversationState.doubt,
          focusEfficiency: (afterState.conversationState.momentum - beforeState.conversationState.momentum) / Math.max(chosenCard.focus, 1)
        };

        feedbackAnalysis.outcomePatterns.push(outcome);

        // Identify learning opportunities
        if (chosenStrategy === 'defensive' && outcome.doubtChange < 0) {
          feedbackAnalysis.strategicLessons.push('Defensive cards effectively reduce doubt');
          feedbackAnalysis.learningOpportunities++;
        }

        if (chosenStrategy === 'aggressive' && outcome.momentumChange > outcome.cardFocus) {
          feedbackAnalysis.strategicLessons.push('High-focus aggressive cards can provide outsized momentum gains');
          feedbackAnalysis.learningOpportunities++;
        }

        if (outcome.focusEfficiency > 1.5) {
          feedbackAnalysis.strategicLessons.push('Some cards provide excellent focus efficiency');
          feedbackAnalysis.learningOpportunities++;
        }

        // Take screenshot for significant strategic moments
        if (Math.abs(outcome.momentumChange) >= 3 || Math.abs(outcome.doubtChange) >= 2) {
          await gameHelpers.takeScreenshot(`strategic_feedback_${chosenStrategy}_turn_${turn + 1}`);
        }

        previousMomentum = afterState.conversationState.momentum;
        previousDoubt = afterState.conversationState.doubt;
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    // Analyze feedback quality
    const strategicVariety = new Set(feedbackAnalysis.outcomePatterns.map(o => o.strategy)).size;
    const positiveOutcomes = feedbackAnalysis.outcomePatterns.filter(o => o.momentumChange > 0).length;
    const negativeOutcomes = feedbackAnalysis.outcomePatterns.filter(o => o.doubtChange > 0).length;

    console.log(`Strategic Feedback Analysis:
      - Total strategic choices: ${feedbackAnalysis.outcomePatterns.length}
      - Strategic variety: ${strategicVariety} different strategies used
      - Learning opportunities identified: ${feedbackAnalysis.learningOpportunities}
      - Positive momentum outcomes: ${positiveOutcomes}
      - Doubt-inducing outcomes: ${negativeOutcomes}
      - Strategic lessons: ${feedbackAnalysis.strategicLessons.join('; ')}
      - Outcome patterns: ${JSON.stringify(feedbackAnalysis.outcomePatterns, null, 2)}`);

    // Feedback quality assertions
    expect(strategicVariety).toBeGreaterThan(2); // Multiple strategies should be viable
    expect(feedbackAnalysis.learningOpportunities).toBeGreaterThan(2); // Clear learning moments
    expect(positiveOutcomes / feedbackAnalysis.outcomePatterns.length).toBeGreaterThan(0.5); // Mostly positive reinforcement
  });

  test('Progressive Complexity: Lower focus cards simple, higher focus cards more complex', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const complexityAnalysis = {
      cardsByFocus: {},
      complexityMetrics: [],
      progressionQuality: 0
    };

    for (let turn = 0; turn < 6; turn++) {
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();

      for (const card of cards) {
        const focus = card.focus;

        if (!complexityAnalysis.cardsByFocus[focus]) {
          complexityAnalysis.cardsByFocus[focus] = [];
        }

        // Measure complexity factors
        const complexity = {
          focusCost: focus,
          effectLength: (card.effect || '').length,
          hasScaling: card.hasScaling || false,
          hasConditions: card.effect?.includes('if') || card.effect?.includes('spend') || false,
          categoryComplexity: card.category === 'Realization' ? 2 : card.category === 'Regulation' ? 1 : 0,
          persistenceComplexity: card.isImpulse ? 1 : 0
        };

        complexity.totalComplexity = complexity.effectLength / 20 +
          (complexity.hasScaling ? 2 : 0) +
          (complexity.hasConditions ? 2 : 0) +
          complexity.categoryComplexity +
          complexity.persistenceComplexity;

        complexityAnalysis.cardsByFocus[focus].push(complexity);
        complexityAnalysis.complexityMetrics.push(complexity);
      }

      // Take screenshot showing complexity progression
      await gameHelpers.takeScreenshot(`complexity_progression_turn_${turn + 1}`);

      // Play a card to continue
      if (cards.length > 0) {
        await gameHelpers.selectCard(0);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    // Analyze complexity progression
    const focusLevels = Object.keys(complexityAnalysis.cardsByFocus).map(k => parseInt(k)).sort((a, b) => a - b);
    let progressionScore = 0;
    let comparisons = 0;

    for (let i = 0; i < focusLevels.length - 1; i++) {
      const lowerFocus = focusLevels[i];
      const higherFocus = focusLevels[i + 1];

      const lowerComplexity = complexityAnalysis.cardsByFocus[lowerFocus];
      const higherComplexity = complexityAnalysis.cardsByFocus[higherFocus];

      if (lowerComplexity.length > 0 && higherComplexity.length > 0) {
        const avgLowerComplexity = lowerComplexity.reduce((sum, c) => sum + c.totalComplexity, 0) / lowerComplexity.length;
        const avgHigherComplexity = higherComplexity.reduce((sum, c) => sum + c.totalComplexity, 0) / higherComplexity.length;

        if (avgHigherComplexity > avgLowerComplexity) {
          progressionScore++;
        }
        comparisons++;
      }
    }

    const progressionQuality = comparisons > 0 ? (progressionScore / comparisons) * 100 : 0;

    console.log(`Progressive Complexity Analysis:
      - Focus levels encountered: ${focusLevels.join(', ')}
      - Progression comparisons: ${progressionScore}/${comparisons}
      - Progression quality: ${progressionQuality.toFixed(1)}%
      - Cards by focus level: ${JSON.stringify(Object.fromEntries(
        Object.entries(complexityAnalysis.cardsByFocus).map(([focus, cards]) => [
          focus,
          {
            count: cards.length,
            avgComplexity: cards.reduce((sum, c) => sum + c.totalComplexity, 0) / cards.length
          }
        ])
      ), null, 2)}`);

    // Progressive complexity assertions
    expect(focusLevels.length).toBeGreaterThan(2); // Multiple focus levels should exist
    expect(progressionQuality).toBeGreaterThan(60); // 60%+ proper progression
    expect(focusLevels[0]).toBeLessThan(focusLevels[focusLevels.length - 1]); // Range of complexity
  });
});