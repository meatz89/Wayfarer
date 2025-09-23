/**
 * Comprehensive Quality Assessment Report
 * Aggregates all test results into a final evaluation of the redesigned card system
 */

const { test, expect } = require('@playwright/test');
const { GameHelpers } = require('./utils/game-helpers');
const { StrategicAnalyzer } = require('./utils/strategic-analyzer');

test.describe('Comprehensive Quality Assessment', () => {
  let gameHelpers;
  let strategicAnalyzer;

  test.beforeEach(async ({ page }) => {
    gameHelpers = new GameHelpers(page);
    strategicAnalyzer = new StrategicAnalyzer();
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();
  });

  test('Complete Player Experience Evaluation: Transform "shit" gameplay into strategic excellence', async ({ page }) => {
    const qualityReport = {
      playerExperienceMetrics: {
        strategicDepth: { score: 0, maxScore: 100, findings: [] },
        learningCurve: { score: 0, maxScore: 100, findings: [] },
        engagement: { score: 0, maxScore: 100, findings: [] },
        uiUxQuality: { score: 0, maxScore: 100, findings: [] },
        balance: { score: 0, maxScore: 100, findings: [] }
      },
      overallAssessment: {
        totalScore: 0,
        grade: '',
        systemQuality: '',
        recommendations: []
      },
      comparativeAnalysis: {
        oldSystemProblems: [
          'Duplicate-heavy deck with redundant choices',
          'Limited strategic depth per turn',
          'Predictable autopilot gameplay',
          'Poor resource tension management',
          'Unclear card effects and scaling'
        ],
        newSystemImprovements: [],
        remainingIssues: []
      },
      detailedFindings: {}
    };

    console.log('🎯 Starting Comprehensive Player Experience Evaluation...');

    // 1. STRATEGIC DEPTH ASSESSMENT
    console.log('📊 Evaluating Strategic Depth...');
    await gameHelpers.startConversation('Elena');

    const strategicDepthResults = await evaluateStrategicDepth(page, gameHelpers, strategicAnalyzer);
    qualityReport.playerExperienceMetrics.strategicDepth = strategicDepthResults;
    qualityReport.detailedFindings.strategicDepth = strategicDepthResults.details;

    // Reset for next evaluation
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();

    // 2. LEARNING CURVE VALIDATION
    console.log('📚 Evaluating Learning Curve...');
    await gameHelpers.startConversation('Elena');

    const learningCurveResults = await evaluateLearningCurve(page, gameHelpers);
    qualityReport.playerExperienceMetrics.learningCurve = learningCurveResults;
    qualityReport.detailedFindings.learningCurve = learningCurveResults.details;

    // Reset for next evaluation
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();

    // 3. ENGAGEMENT TESTING
    console.log('🎮 Evaluating Player Engagement...');
    await gameHelpers.startConversation('Elena');

    const engagementResults = await evaluateEngagement(page, gameHelpers);
    qualityReport.playerExperienceMetrics.engagement = engagementResults;
    qualityReport.detailedFindings.engagement = engagementResults.details;

    // Reset for next evaluation
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();

    // 4. UI/UX QUALITY ASSESSMENT
    console.log('🎨 Evaluating UI/UX Quality...');
    await gameHelpers.startConversation('Elena');

    const uiUxResults = await evaluateUIUX(page, gameHelpers);
    qualityReport.playerExperienceMetrics.uiUxQuality = uiUxResults;
    qualityReport.detailedFindings.uiUxQuality = uiUxResults.details;

    // Reset for next evaluation
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();

    // 5. BALANCE VALIDATION
    console.log('⚖️ Evaluating Game Balance...');
    await gameHelpers.startConversation('Elena');

    const balanceResults = await evaluateBalance(page, gameHelpers);
    qualityReport.playerExperienceMetrics.balance = balanceResults;
    qualityReport.detailedFindings.balance = balanceResults.details;

    // 6. COMPILE OVERALL ASSESSMENT
    console.log('📝 Compiling Overall Assessment...');

    const totalScore = Object.values(qualityReport.playerExperienceMetrics)
      .reduce((sum, metric) => sum + metric.score, 0) / 5;

    qualityReport.overallAssessment.totalScore = totalScore;
    qualityReport.overallAssessment.grade = getQualityGrade(totalScore);
    qualityReport.overallAssessment.systemQuality = getSystemQuality(totalScore);

    // Identify improvements and remaining issues
    qualityReport.comparativeAnalysis.newSystemImprovements = identifyImprovements(qualityReport);
    qualityReport.comparativeAnalysis.remainingIssues = identifyRemainingIssues(qualityReport);
    qualityReport.overallAssessment.recommendations = generateRecommendations(qualityReport);

    // Take final comprehensive screenshot
    await gameHelpers.takeScreenshot('comprehensive_quality_assessment_final');

    // 7. GENERATE FINAL REPORT
    console.log('\n' + '='.repeat(80));
    console.log('🏆 WAYFARER CARD SYSTEM QUALITY ASSESSMENT REPORT');
    console.log('='.repeat(80));

    console.log('\n📊 OVERALL SCORE: ' + totalScore.toFixed(1) + '/100');
    console.log('🎖️  SYSTEM GRADE: ' + qualityReport.overallAssessment.grade);
    console.log('⭐ QUALITY LEVEL: ' + qualityReport.overallAssessment.systemQuality);

    console.log('\n📈 DETAILED METRICS:');
    for (const [category, metric] of Object.entries(qualityReport.playerExperienceMetrics)) {
      console.log(`   ${category.toUpperCase()}: ${metric.score.toFixed(1)}/100`);
      console.log(`   └─ Key Findings: ${metric.findings.slice(0, 2).join('; ')}`);
    }

    console.log('\n✅ NEW SYSTEM IMPROVEMENTS:');
    qualityReport.comparativeAnalysis.newSystemImprovements.forEach((improvement, i) => {
      console.log(`   ${i + 1}. ${improvement}`);
    });

    console.log('\n⚠️  REMAINING ISSUES:');
    qualityReport.comparativeAnalysis.remainingIssues.forEach((issue, i) => {
      console.log(`   ${i + 1}. ${issue}`);
    });

    console.log('\n🎯 RECOMMENDATIONS:');
    qualityReport.overallAssessment.recommendations.forEach((rec, i) => {
      console.log(`   ${i + 1}. ${rec}`);
    });

    console.log('\n🔍 COMPARATIVE ANALYSIS:');
    console.log('   OLD SYSTEM PROBLEMS ADDRESSED:');
    qualityReport.comparativeAnalysis.oldSystemProblems.forEach((problem, i) => {
      const isAddressed = isOldProblemAddressed(problem, qualityReport);
      console.log(`   ${isAddressed ? '✅' : '❌'} ${problem}`);
    });

    console.log('\n📋 DETAILED TEST RESULTS:');
    console.log(JSON.stringify(qualityReport.detailedFindings, null, 2));

    console.log('\n' + '='.repeat(80));
    console.log('🎮 FINAL VERDICT: ' + getFinalVerdict(qualityReport));
    console.log('='.repeat(80));

    // Quality assertions for the overall system
    expect(totalScore).toBeGreaterThan(70); // System should achieve "Good" quality minimum
    expect(qualityReport.comparativeAnalysis.newSystemImprovements.length).toBeGreaterThan(3); // Should show clear improvements
    expect(qualityReport.comparativeAnalysis.remainingIssues.length).toBeLessThan(5); // Limited remaining issues

    // Specific excellence criteria
    expect(qualityReport.playerExperienceMetrics.strategicDepth.score).toBeGreaterThan(65); // Strategic depth is critical
    expect(qualityReport.playerExperienceMetrics.engagement.score).toBeGreaterThan(65); // Engagement is critical
  });
});

// Helper functions for evaluation

async function evaluateStrategicDepth(page, gameHelpers, strategicAnalyzer) {
  const result = { score: 0, maxScore: 100, findings: [], details: {} };

  let viableStrategiesScores = [];
  let resourceTensionScores = [];
  let planningDepthScores = [];

  for (let turn = 0; turn < 6; turn++) {
    const gameState = await gameHelpers.getGameState();
    await gameHelpers.executeListen();
    await gameHelpers.waitForNarrativeGeneration();

    const cards = await gameHelpers.getHandCards();
    if (cards.length === 0) break;

    const availableActions = { cards };
    strategicAnalyzer.recordGameState(gameState, availableActions, {});

    // Evaluate decision complexity
    const viableStrategies = strategicAnalyzer.countViableStrategies({ gameState, availableActions });
    const resourceTension = strategicAnalyzer.calculateResourceTension(gameState);
    const planningDepth = strategicAnalyzer.assessPlanningDepth({ gameState, availableActions });

    viableStrategiesScores.push(viableStrategies);
    resourceTensionScores.push(resourceTension);
    planningDepthScores.push(planningDepth);

    // Play a card to continue
    if (cards.length > 0) {
      await gameHelpers.selectCard(0);
      await gameHelpers.executeSpeak();
      await gameHelpers.waitForNarrativeGeneration();
    }

    if (await gameHelpers.isConversationExhausted()) break;
  }

  // Calculate scores
  const avgViableStrategies = viableStrategiesScores.reduce((a, b) => a + b, 0) / viableStrategiesScores.length;
  const avgResourceTension = resourceTensionScores.reduce((a, b) => a + b, 0) / resourceTensionScores.length;
  const avgPlanningDepth = planningDepthScores.reduce((a, b) => a + b, 0) / planningDepthScores.length;

  // Score components (0-100 each, then averaged)
  const strategiesScore = Math.min(100, (avgViableStrategies / 3) * 100); // Target: 3 strategies
  const tensionScore = Math.min(100, (avgResourceTension / 3) * 100); // Target: 3 tension
  const planningScore = Math.min(100, (avgPlanningDepth / 2) * 100); // Target: 2 planning depth

  result.score = (strategiesScore + tensionScore + planningScore) / 3;

  result.findings.push(`Average viable strategies: ${avgViableStrategies.toFixed(1)}/3 (${strategiesScore.toFixed(1)}%)`);
  result.findings.push(`Average resource tension: ${avgResourceTension.toFixed(1)}/3 (${tensionScore.toFixed(1)}%)`);
  result.findings.push(`Average planning depth: ${avgPlanningDepth.toFixed(1)}/2 (${planningScore.toFixed(1)}%)`);

  result.details = {
    avgViableStrategies,
    avgResourceTension,
    avgPlanningDepth,
    strategiesScore,
    tensionScore,
    planningScore,
    viableStrategiesScores,
    resourceTensionScores,
    planningDepthScores
  };

  return result;
}

async function evaluateLearningCurve(page, gameHelpers) {
  const result = { score: 0, maxScore: 100, findings: [], details: {} };

  let totalCards = 0;
  let cardsWithClearEffects = 0;
  let complexityProgression = [];

  for (let turn = 0; turn < 5; turn++) {
    await gameHelpers.executeListen();
    await gameHelpers.waitForNarrativeGeneration();

    const cards = await gameHelpers.getHandCards();
    if (cards.length === 0) break;

    for (const card of cards) {
      totalCards++;

      // Check effect clarity
      if (card.effect && card.effect.length > 0 && (card.effect.includes('Strike') || card.effect.includes('Soothe') || card.effect.includes('Focusing'))) {
        cardsWithClearEffects++;
      }

      // Measure complexity
      const complexity = card.focus + (card.hasScaling ? 2 : 0) + (card.effect?.includes('spend') ? 1 : 0);
      complexityProgression.push({ focus: card.focus, complexity });
    }

    if (cards.length > 0) {
      await gameHelpers.selectCard(0);
      await gameHelpers.executeSpeak();
      await gameHelpers.waitForNarrativeGeneration();
    }

    if (await gameHelpers.isConversationExhausted()) break;
  }

  // Calculate scores
  const clarityScore = totalCards > 0 ? (cardsWithClearEffects / totalCards) * 100 : 0;

  // Check complexity progression
  const focusLevels = [...new Set(complexityProgression.map(c => c.focus))].sort((a, b) => a - b);
  let progressionScore = 0;
  if (focusLevels.length > 1) {
    progressionScore = 80; // Base score for having progression
  }

  result.score = (clarityScore + progressionScore) / 2;

  result.findings.push(`Card effect clarity: ${cardsWithClearEffects}/${totalCards} (${clarityScore.toFixed(1)}%)`);
  result.findings.push(`Complexity progression: ${focusLevels.length} focus levels found`);

  result.details = {
    totalCards,
    cardsWithClearEffects,
    clarityScore,
    progressionScore,
    focusLevels,
    complexityProgression
  };

  return result;
}

async function evaluateEngagement(page, gameHelpers) {
  const result = { score: 0, maxScore: 100, findings: [], details: {} };

  let totalDecisions = 0;
  let meaningfulChoices = 0;
  let tensionScores = [];

  for (let turn = 0; turn < 6; turn++) {
    const gameState = await gameHelpers.getGameState();
    await gameHelpers.executeListen();
    await gameHelpers.waitForNarrativeGeneration();

    const cards = await gameHelpers.getHandCards();
    if (cards.length === 0) break;

    totalDecisions++;

    // Analyze choice meaningfulness
    const playableCards = cards.filter(card => card.isSelectable && card.focus <= gameState.conversationState.focus.available);
    const effectTypes = new Set(playableCards.map(card => {
      if (card.effect?.includes('Strike')) return 'momentum';
      if (card.effect?.includes('Soothe')) return 'doubt';
      return 'other';
    }));

    if (playableCards.length > 1 && effectTypes.size > 1) {
      meaningfulChoices++;
    }

    // Calculate tension
    let tension = 0;
    if (gameState.conversationState.doubt > 5) tension += 2;
    if (gameState.conversationState.focus.available <= 2) tension += 1;
    tensionScores.push(tension);

    if (cards.length > 0) {
      await gameHelpers.selectCard(0);
      await gameHelpers.executeSpeak();
      await gameHelpers.waitForNarrativeGeneration();
    }

    if (await gameHelpers.isConversationExhausted()) break;
  }

  const choiceScore = totalDecisions > 0 ? (meaningfulChoices / totalDecisions) * 100 : 0;
  const avgTension = tensionScores.length > 0 ? tensionScores.reduce((a, b) => a + b, 0) / tensionScores.length : 0;
  const tensionScore = Math.min(100, (avgTension / 2) * 100);

  result.score = (choiceScore + tensionScore) / 2;

  result.findings.push(`Meaningful choices: ${meaningfulChoices}/${totalDecisions} (${choiceScore.toFixed(1)}%)`);
  result.findings.push(`Average tension: ${avgTension.toFixed(1)}/2 (${tensionScore.toFixed(1)}%)`);

  result.details = {
    totalDecisions,
    meaningfulChoices,
    choiceScore,
    avgTension,
    tensionScore,
    tensionScores
  };

  return result;
}

async function evaluateUIUX(page, gameHelpers) {
  const result = { score: 0, maxScore: 100, findings: [], details: {} };

  // Check UI elements presence
  const requiredElements = {
    momentum: await page.locator('.momentum-counter, .momentum-display').count() > 0,
    doubt: await page.locator('.doubt-slot, .doubt-display').count() > 0,
    focus: await page.locator('.focus-dot, .focus-display').count() > 0,
    cards: await page.locator('.card, .card-grid').count() > 0,
    actions: await page.locator('.action-button, .action-choices').count() > 0
  };

  const elementScore = (Object.values(requiredElements).filter(Boolean).length / Object.keys(requiredElements).length) * 100;

  // Test responsiveness
  const beforeState = await gameHelpers.getGameState();
  await gameHelpers.executeListen();
  await gameHelpers.waitForNarrativeGeneration();

  const cards = await gameHelpers.getHandCards();
  let responsivenessScore = 50; // Base score

  if (cards.length > 0) {
    const actionStart = Date.now();
    await gameHelpers.selectCard(0);
    await gameHelpers.executeSpeak();
    await gameHelpers.waitForNarrativeGeneration();
    const actionEnd = Date.now();

    const actionTime = actionEnd - actionStart;
    if (actionTime < 3000) responsivenessScore = 100;
    else if (actionTime < 5000) responsivenessScore = 75;
    else responsivenessScore = 50;
  }

  result.score = (elementScore + responsivenessScore) / 2;

  result.findings.push(`Required UI elements: ${Object.values(requiredElements).filter(Boolean).length}/${Object.keys(requiredElements).length}`);
  result.findings.push(`Responsiveness score: ${responsivenessScore}%`);

  result.details = {
    requiredElements,
    elementScore,
    responsivenessScore
  };

  return result;
}

async function evaluateBalance(page, gameHelpers) {
  const result = { score: 0, maxScore: 100, findings: [], details: {} };

  let focusEfficiencies = [];
  let conversationLength = 0;

  for (let turn = 0; turn < 6; turn++) {
    const beforeState = await gameHelpers.getGameState();
    await gameHelpers.executeListen();
    await gameHelpers.waitForNarrativeGeneration();

    const cards = await gameHelpers.getHandCards();
    if (cards.length === 0) break;

    conversationLength++;

    if (cards.length > 0) {
      const card = cards[0];
      await gameHelpers.selectCard(0);
      await gameHelpers.executeSpeak();
      await gameHelpers.waitForNarrativeGeneration();

      const afterState = await gameHelpers.getGameState();
      const momentumGain = afterState.conversationState.momentum - beforeState.conversationState.momentum;
      const efficiency = momentumGain / Math.max(card.focus, 1);
      focusEfficiencies.push(efficiency);
    }

    if (await gameHelpers.isConversationExhausted()) break;
  }

  // Calculate balance scores
  const avgEfficiency = focusEfficiencies.length > 0 ? focusEfficiencies.reduce((a, b) => a + b, 0) / focusEfficiencies.length : 0;
  const efficiencyScore = Math.min(100, Math.max(0, (1.5 - Math.abs(avgEfficiency - 1.0)) * 100)); // Target efficiency around 1.0

  const lengthScore = conversationLength >= 5 && conversationLength <= 10 ? 100 : Math.max(0, 100 - Math.abs(conversationLength - 7.5) * 10);

  result.score = (efficiencyScore + lengthScore) / 2;

  result.findings.push(`Average focus efficiency: ${avgEfficiency.toFixed(2)} (target: ~1.0)`);
  result.findings.push(`Conversation length: ${conversationLength} turns (target: 5-10)`);

  result.details = {
    avgEfficiency,
    efficiencyScore,
    conversationLength,
    lengthScore,
    focusEfficiencies
  };

  return result;
}

function getQualityGrade(score) {
  if (score >= 90) return 'A+ (Excellent)';
  if (score >= 85) return 'A (Very Good)';
  if (score >= 80) return 'B+ (Good)';
  if (score >= 75) return 'B (Acceptable)';
  if (score >= 70) return 'C+ (Below Average)';
  if (score >= 60) return 'C (Poor)';
  return 'F (Failing)';
}

function getSystemQuality(score) {
  if (score >= 85) return 'Strategic Excellence Achieved';
  if (score >= 75) return 'Solid Strategic Gameplay';
  if (score >= 65) return 'Adequate Improvement';
  if (score >= 50) return 'Some Progress Made';
  return 'Significant Issues Remain';
}

function identifyImprovements(qualityReport) {
  const improvements = [];

  if (qualityReport.playerExperienceMetrics.strategicDepth.score > 70) {
    improvements.push('Strategic depth significantly improved with multiple viable approaches per turn');
  }

  if (qualityReport.playerExperienceMetrics.engagement.score > 70) {
    improvements.push('Player engagement enhanced through meaningful choices and tension management');
  }

  if (qualityReport.playerExperienceMetrics.learningCurve.score > 70) {
    improvements.push('Learning curve optimized with clear progression from simple to complex cards');
  }

  if (qualityReport.playerExperienceMetrics.uiUxQuality.score > 70) {
    improvements.push('UI/UX quality provides clear feedback and information display');
  }

  if (qualityReport.playerExperienceMetrics.balance.score > 70) {
    improvements.push('Game balance achieved with proper power-to-cost ratios and conversation pacing');
  }

  return improvements;
}

function identifyRemainingIssues(qualityReport) {
  const issues = [];

  if (qualityReport.playerExperienceMetrics.strategicDepth.score < 70) {
    issues.push('Strategic depth still limited - need more viable strategies per turn');
  }

  if (qualityReport.playerExperienceMetrics.engagement.score < 70) {
    issues.push('Player engagement issues - too many trivial or overwhelming choices');
  }

  if (qualityReport.playerExperienceMetrics.learningCurve.score < 70) {
    issues.push('Learning curve problems - unclear effects or poor complexity progression');
  }

  if (qualityReport.playerExperienceMetrics.uiUxQuality.score < 70) {
    issues.push('UI/UX quality needs improvement - unclear information or poor responsiveness');
  }

  if (qualityReport.playerExperienceMetrics.balance.score < 70) {
    issues.push('Balance issues remain - power levels or conversation length need adjustment');
  }

  return issues;
}

function generateRecommendations(qualityReport) {
  const recommendations = [];

  const avgScore = Object.values(qualityReport.playerExperienceMetrics).reduce((sum, metric) => sum + metric.score, 0) / 5;

  if (avgScore < 85) {
    recommendations.push('Continue iterating on card effects and scaling formulas for better strategic depth');
  }

  if (qualityReport.playerExperienceMetrics.strategicDepth.score < 75) {
    recommendations.push('Add more setup cards and resource conversion mechanics to increase viable strategies');
  }

  if (qualityReport.playerExperienceMetrics.engagement.score < 75) {
    recommendations.push('Review decision points to ensure consistent meaningful choices throughout conversations');
  }

  if (qualityReport.playerExperienceMetrics.balance.score < 75) {
    recommendations.push('Adjust focus costs and momentum rewards to improve power-level consistency');
  }

  if (qualityReport.playerExperienceMetrics.uiUxQuality.score < 75) {
    recommendations.push('Enhance visual feedback for card effects and game state changes');
  }

  return recommendations;
}

function isOldProblemAddressed(problem, qualityReport) {
  if (problem.includes('Duplicate-heavy deck')) {
    return qualityReport.playerExperienceMetrics.strategicDepth.score > 70;
  }
  if (problem.includes('Limited strategic depth')) {
    return qualityReport.playerExperienceMetrics.strategicDepth.score > 75;
  }
  if (problem.includes('autopilot gameplay')) {
    return qualityReport.playerExperienceMetrics.engagement.score > 70;
  }
  if (problem.includes('resource tension')) {
    return qualityReport.playerExperienceMetrics.strategicDepth.score > 65;
  }
  if (problem.includes('Unclear card effects')) {
    return qualityReport.playerExperienceMetrics.learningCurve.score > 70;
  }
  return false;
}

function getFinalVerdict(qualityReport) {
  const totalScore = qualityReport.overallAssessment.totalScore;

  if (totalScore >= 85) {
    return 'TRANSFORMATION SUCCESSFUL: Card system achieves strategic excellence! 🎉';
  } else if (totalScore >= 75) {
    return 'SIGNIFICANT IMPROVEMENT: Card system much better than before, with minor refinements needed. ✅';
  } else if (totalScore >= 65) {
    return 'MODERATE PROGRESS: Card system improved but still needs substantial work. 🔄';
  } else {
    return 'TRANSFORMATION INCOMPLETE: Major issues remain, system not yet ready. ❌';
  }
}