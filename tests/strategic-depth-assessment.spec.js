/**
 * Strategic Depth Assessment - Core Player Experience Quality Tests
 * Evaluates if the redesigned card system achieves strategic excellence
 */

const { test, expect } = require('@playwright/test');
const { GameHelpers } = require('./utils/game-helpers');
const { StrategicAnalyzer } = require('./utils/strategic-analyzer');

test.describe('Strategic Depth Assessment', () => {
  let gameHelpers;
  let strategicAnalyzer;

  test.beforeEach(async ({ page }) => {
    gameHelpers = new GameHelpers(page);
    strategicAnalyzer = new StrategicAnalyzer();

    // Start fresh game session
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();
  });

  test('Decision Complexity: Each turn offers 2-3 viable strategies with clear tradeoffs', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    let turnCount = 0;
    const maxTurns = 10;
    const decisionQualityScores = [];

    while (turnCount < maxTurns && !await gameHelpers.isConversationExhausted()) {
      turnCount++;

      // Get current game state
      const gameState = await gameHelpers.getGameState();
      const availableCards = await gameHelpers.getHandCards();

      // Analyze available strategies
      const availableActions = { cards: availableCards };
      strategicAnalyzer.recordGameState(gameState, availableActions, {});

      // Evaluate decision complexity
      const viableStrategies = strategicAnalyzer.countViableStrategies({
        gameState,
        availableActions
      });

      decisionQualityScores.push(viableStrategies);

      // Take screenshot of decision point
      await gameHelpers.takeScreenshot(`strategic_decision_turn_${turnCount}`);

      // Execute a turn
      if (await gameHelpers.executeListen()) {
        await gameHelpers.waitForNarrativeGeneration();

        // Make strategic choice
        const cards = await gameHelpers.getHandCards();
        if (cards.length > 0) {
          // Select first available card for testing
          await gameHelpers.selectCard(0);
          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();
        }
      }
    }

    // Analyze results
    const averageViableStrategies = decisionQualityScores.reduce((a, b) => a + b, 0) / decisionQualityScores.length;
    const excellentDecisions = decisionQualityScores.filter(score => score >= 3).length;
    const poorDecisions = decisionQualityScores.filter(score => score <= 1).length;

    console.log(`Strategic Depth Analysis:
      - Average viable strategies per turn: ${averageViableStrategies.toFixed(2)}
      - Excellent decisions (3+ strategies): ${excellentDecisions}/${turnCount}
      - Poor decisions (≤1 strategy): ${poorDecisions}/${turnCount}
      - Decision quality scores: ${decisionQualityScores.join(', ')}`);

    // Quality assertions
    expect(averageViableStrategies).toBeGreaterThan(2.0);
    expect(excellentDecisions / turnCount).toBeGreaterThan(0.4); // 40%+ excellent decisions
    expect(poorDecisions / turnCount).toBeLessThan(0.2); // <20% poor decisions
  });

  test('Long-term Planning: Setup turns enable bigger plays', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    let setupCardsUsed = 0;
    let planningDepthScores = [];
    let turnCount = 0;
    const maxTurns = 8;

    while (turnCount < maxTurns && !await gameHelpers.isConversationExhausted()) {
      turnCount++;

      const gameState = await gameHelpers.getGameState();
      const cards = await gameHelpers.getHandCards();

      // Look for setup cards (mental_reset, careful_words)
      const setupCards = cards.filter(card =>
        card.name?.includes('mental_reset') ||
        card.name?.includes('careful_words') ||
        card.effect?.includes('Focusing')
      );

      // Assess planning depth required
      const planningDepth = strategicAnalyzer.assessPlanningDepth({
        gameState,
        availableActions: { cards }
      });
      planningDepthScores.push(planningDepth);

      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      // Use setup cards when available to test planning mechanics
      if (setupCards.length > 0) {
        await gameHelpers.selectCard(setupCards[0].name);
        setupCardsUsed++;
        await gameHelpers.takeScreenshot(`setup_card_used_turn_${turnCount}`);
      } else if (cards.length > 0) {
        await gameHelpers.selectCard(0);
      }

      await gameHelpers.executeSpeak();
      await gameHelpers.waitForNarrativeGeneration();
    }

    const averagePlanningDepth = planningDepthScores.reduce((a, b) => a + b, 0) / planningDepthScores.length;

    console.log(`Planning Depth Analysis:
      - Setup cards available and used: ${setupCardsUsed}
      - Average planning depth required: ${averagePlanningDepth.toFixed(2)}
      - Planning depth scores: ${planningDepthScores.join(', ')}`);

    // Planning quality assertions
    expect(setupCardsUsed).toBeGreaterThan(0); // Setup cards should be available
    expect(averagePlanningDepth).toBeGreaterThan(1.0); // Require meaningful planning
  });

  test('Risk/Reward Balance: High-focus cards require planning but provide proportional benefits', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const riskRewardAnalysis = {
      highFocusCards: [],
      lowFocusCards: [],
      riskLevels: [],
      turnOutcomes: []
    };

    let turnCount = 0;
    const maxTurns = 8;

    while (turnCount < maxTurns && !await gameHelpers.isConversationExhausted()) {
      turnCount++;

      const beforeState = await gameHelpers.getGameState();
      const cards = await gameHelpers.getHandCards();

      // Categorize cards by focus cost
      const highFocusCards = cards.filter(card => card.focus >= 4);
      const lowFocusCards = cards.filter(card => card.focus <= 2);

      riskRewardAnalysis.highFocusCards.push(highFocusCards.length);
      riskRewardAnalysis.lowFocusCards.push(lowFocusCards.length);

      // Assess risk level
      const riskLevel = strategicAnalyzer.assessRiskLevel({
        gameState: beforeState,
        availableActions: { cards }
      });
      riskRewardAnalysis.riskLevels.push(riskLevel);

      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      // Try to use high-focus cards when possible to test risk/reward
      let cardUsed = null;
      if (highFocusCards.length > 0 && beforeState.conversationState.focus.available >= highFocusCards[0].focus) {
        cardUsed = highFocusCards[0];
        await gameHelpers.selectCard(cardUsed.name);
        await gameHelpers.takeScreenshot(`high_risk_card_turn_${turnCount}`);
      } else if (lowFocusCards.length > 0) {
        cardUsed = lowFocusCards[0];
        await gameHelpers.selectCard(cardUsed.name);
      }

      if (cardUsed) {
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();

        const afterState = await gameHelpers.getGameState();

        // Analyze outcome
        const momentumGain = afterState.conversationState.momentum - beforeState.conversationState.momentum;
        riskRewardAnalysis.turnOutcomes.push({
          cardFocus: cardUsed.focus,
          momentumGain,
          doubtChange: afterState.conversationState.doubt - beforeState.conversationState.doubt
        });
      }
    }

    // Analyze risk/reward balance
    const avgRiskLevel = riskRewardAnalysis.riskLevels.reduce((a, b) => a + b, 0) / riskRewardAnalysis.riskLevels.length;
    const totalHighFocusCards = riskRewardAnalysis.highFocusCards.reduce((a, b) => a + b, 0);
    const highFocusOutcomes = riskRewardAnalysis.turnOutcomes.filter(o => o.cardFocus >= 4);
    const lowFocusOutcomes = riskRewardAnalysis.turnOutcomes.filter(o => o.cardFocus <= 2);

    const avgHighFocusReward = highFocusOutcomes.length > 0 ?
      highFocusOutcomes.reduce((sum, o) => sum + o.momentumGain, 0) / highFocusOutcomes.length : 0;
    const avgLowFocusReward = lowFocusOutcomes.length > 0 ?
      lowFocusOutcomes.reduce((sum, o) => sum + o.momentumGain, 0) / lowFocusOutcomes.length : 0;

    console.log(`Risk/Reward Analysis:
      - Average risk level: ${avgRiskLevel.toFixed(2)}
      - High-focus cards encountered: ${totalHighFocusCards}
      - High-focus average reward: ${avgHighFocusReward.toFixed(2)}
      - Low-focus average reward: ${avgLowFocusReward.toFixed(2)}
      - Risk/reward ratio: ${(avgHighFocusReward / Math.max(avgLowFocusReward, 1)).toFixed(2)}`);

    // Risk/reward assertions
    expect(totalHighFocusCards).toBeGreaterThan(0); // High-focus cards should exist
    expect(avgRiskLevel).toBeGreaterThan(1.0); // Some risk should be present
    if (highFocusOutcomes.length > 0 && lowFocusOutcomes.length > 0) {
      expect(avgHighFocusReward).toBeGreaterThan(avgLowFocusReward); // Higher risk should mean higher reward
    }
  });

  test('Resource Tension: Multiple competing demands create engaging decisions', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const tensionAnalysis = {
      tensionScores: [],
      resourceStates: [],
      competingDemands: []
    };

    let turnCount = 0;
    const maxTurns = 10;

    while (turnCount < maxTurns && !await gameHelpers.isConversationExhausted()) {
      turnCount++;

      const gameState = await gameHelpers.getGameState();
      const cards = await gameHelpers.getHandCards();

      // Calculate resource tension
      const tensionScore = strategicAnalyzer.calculateResourceTension(gameState);
      tensionAnalysis.tensionScores.push(tensionScore);

      // Record resource state
      const resourceState = {
        focus: gameState.conversationState.focus,
        momentum: gameState.conversationState.momentum,
        doubt: gameState.conversationState.doubt,
        handSize: gameState.conversationState.handSize
      };
      tensionAnalysis.resourceStates.push(resourceState);

      // Identify competing demands
      const demands = [];
      if (resourceState.doubt > 5) demands.push('doubt_management');
      if (resourceState.focus.available <= 2) demands.push('focus_conservation');
      if (resourceState.momentum >= 5) demands.push('momentum_spending_opportunity');
      if (resourceState.handSize <= 3) demands.push('card_economy');

      tensionAnalysis.competingDemands.push(demands);

      // Take screenshot of high-tension moments
      if (tensionScore >= 3) {
        await gameHelpers.takeScreenshot(`high_tension_turn_${turnCount}_score_${tensionScore}`);
      }

      // Execute turn
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      if (cards.length > 0) {
        await gameHelpers.selectCard(0);
        await gameHelpers.executeSpeak();
        await gameHelpers.waitForNarrativeGeneration();
      }
    }

    // Analyze tension quality
    const avgTension = tensionAnalysis.tensionScores.reduce((a, b) => a + b, 0) / tensionAnalysis.tensionScores.length;
    const highTensionTurns = tensionAnalysis.tensionScores.filter(score => score >= 3).length;
    const lowTensionTurns = tensionAnalysis.tensionScores.filter(score <= 1).length;
    const avgCompetingDemands = tensionAnalysis.competingDemands.reduce((sum, demands) => sum + demands.length, 0) / tensionAnalysis.competingDemands.length;

    console.log(`Resource Tension Analysis:
      - Average tension score: ${avgTension.toFixed(2)}
      - High tension turns (≥3): ${highTensionTurns}/${turnCount}
      - Low tension turns (≤1): ${lowTensionTurns}/${turnCount}
      - Average competing demands per turn: ${avgCompetingDemands.toFixed(2)}
      - Tension scores: ${tensionAnalysis.tensionScores.join(', ')}`);

    // Tension quality assertions
    expect(avgTension).toBeGreaterThan(2.0); // Meaningful tension
    expect(avgTension).toBeLessThan(4.5); // Not overwhelming
    expect(highTensionTurns / turnCount).toBeGreaterThan(0.3); // 30%+ high tension turns
    expect(avgCompetingDemands).toBeGreaterThan(1.5); // Multiple competing demands
  });

  test('Strategic Variety: Multiple viable approaches to same goal', async ({ page }) => {
    // Test different strategic approaches by running multiple conversations
    const strategicApproaches = [];

    for (let approach = 1; approach <= 3; approach++) {
      // Reset game state
      await page.goto('/');
      await gameHelpers.waitForGameReady();
      await gameHelpers.startNewGame();
      await gameHelpers.startConversation('Elena');

      const approachAnalysis = {
        approach: approach,
        cardCategoriesUsed: new Set(),
        momentumBuilding: [],
        strategicChoices: []
      };

      let turnCount = 0;
      const maxTurns = 6;

      while (turnCount < maxTurns && !await gameHelpers.isConversationExhausted()) {
        turnCount++;

        const gameState = await gameHelpers.getGameState();
        const cards = await gameHelpers.getHandCards();

        await gameHelpers.executeListen();
        await gameHelpers.waitForNarrativeGeneration();

        // Use different strategic approaches
        let selectedCard = null;
        if (approach === 1 && cards.length > 0) {
          // Approach 1: Aggressive momentum building
          const highMomentumCards = cards.filter(card => card.effect?.includes('Strike'));
          selectedCard = highMomentumCards.length > 0 ? highMomentumCards[0] : cards[0];

        } else if (approach === 2 && cards.length > 0) {
          // Approach 2: Conservative doubt management
          const sootheCards = cards.filter(card => card.effect?.includes('Soothe'));
          selectedCard = sootheCards.length > 0 ? sootheCards[0] : cards.find(card => card.focus <= 2) || cards[0];

        } else if (approach === 3 && cards.length > 0) {
          // Approach 3: Setup and efficiency
          const setupCards = cards.filter(card => card.effect?.includes('Focusing') || card.name?.includes('mental_reset'));
          selectedCard = setupCards.length > 0 ? setupCards[0] : cards[0];
        }

        if (selectedCard) {
          approachAnalysis.cardCategoriesUsed.add(selectedCard.category);
          approachAnalysis.strategicChoices.push(selectedCard.category);

          await gameHelpers.selectCard(selectedCard.name);
          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();

          const afterState = await gameHelpers.getGameState();
          approachAnalysis.momentumBuilding.push(afterState.conversationState.momentum);
        }
      }

      strategicApproaches.push(approachAnalysis);
      await gameHelpers.takeScreenshot(`strategic_approach_${approach}_final`);
    }

    // Analyze strategic variety
    const uniqueCategories = new Set();
    strategicApproaches.forEach(approach => {
      approach.cardCategoriesUsed.forEach(category => uniqueCategories.add(category));
    });

    const finalMomentumScores = strategicApproaches.map(approach =>
      approach.momentumBuilding.length > 0 ? approach.momentumBuilding[approach.momentumBuilding.length - 1] : 0
    );

    const momentumVariation = Math.max(...finalMomentumScores) - Math.min(...finalMomentumScores);

    console.log(`Strategic Variety Analysis:
      - Unique card categories used: ${Array.from(uniqueCategories).join(', ')}
      - Final momentum scores: ${finalMomentumScores.join(', ')}
      - Momentum variation between approaches: ${momentumVariation}
      - Strategic approaches analyzed: ${strategicApproaches.length}`);

    // Strategic variety assertions
    expect(uniqueCategories.size).toBeGreaterThan(2); // Multiple categories should be viable
    expect(momentumVariation).toBeLessThan(8); // Approaches should be roughly balanced
    expect(Math.min(...finalMomentumScores)).toBeGreaterThan(0); // All approaches should be viable
  });

  test.afterEach(async ({ page }) => {
    // Generate per-test analysis
    const report = strategicAnalyzer.generateStrategicDepthReport();
    console.log('\n=== STRATEGIC DEPTH REPORT ===');
    console.log(JSON.stringify(report, null, 2));
  });
});