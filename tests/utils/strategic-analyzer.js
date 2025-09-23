/**
 * Strategic Analysis Utilities for Card System Evaluation
 * Analyzes gameplay patterns for strategic depth and balance
 */

class StrategicAnalyzer {
  constructor() {
    this.gameHistory = [];
    this.decisionPoints = [];
    this.strategicMetrics = {
      viableStrategiesPerTurn: [],
      resourceTension: [],
      planningDepthRequired: [],
      riskRewardBalance: []
    };
  }

  /**
   * Record a game state for analysis
   */
  recordGameState(gameState, availableActions, playerChoices) {
    const snapshot = {
      timestamp: Date.now(),
      gameState: JSON.parse(JSON.stringify(gameState)),
      availableActions: JSON.parse(JSON.stringify(availableActions)),
      playerChoices: JSON.parse(JSON.stringify(playerChoices))
    };

    this.gameHistory.push(snapshot);
    this.analyzeDecisionPoint(snapshot);
  }

  /**
   * Analyze a single decision point for strategic depth
   */
  analyzeDecisionPoint(snapshot) {
    const analysis = {
      timestamp: snapshot.timestamp,
      viableStrategies: this.countViableStrategies(snapshot),
      resourceTension: this.calculateResourceTension(snapshot.gameState),
      planningRequired: this.assessPlanningDepth(snapshot),
      riskLevel: this.assessRiskLevel(snapshot)
    };

    this.decisionPoints.push(analysis);

    // Update metrics
    this.strategicMetrics.viableStrategiesPerTurn.push(analysis.viableStrategies);
    this.strategicMetrics.resourceTension.push(analysis.resourceTension);
    this.strategicMetrics.planningDepthRequired.push(analysis.planningRequired);
    this.strategicMetrics.riskRewardBalance.push(analysis.riskLevel);
  }

  /**
   * Count viable strategies available at this decision point
   * Strategic Depth Criteria: Each turn should offer 2-3 viable strategies
   */
  countViableStrategies(snapshot) {
    const { gameState, availableActions } = snapshot;
    let viableStrategies = 0;

    // Strategy 1: Conservative play (low-focus cards, doubt management)
    const lowFocusCards = availableActions.cards?.filter(card => card.focus <= 2) || [];
    const hasDoubtManagement = availableActions.cards?.some(card =>
      card.effect?.includes('Soothe') || card.effect?.includes('reduce doubt')
    ) || false;

    if (lowFocusCards.length > 0 && gameState.conversationState?.doubt > 5) {
      viableStrategies++;
    }

    // Strategy 2: Aggressive momentum building
    const highMomentumCards = availableActions.cards?.filter(card =>
      card.effect?.includes('Strike') || card.effect?.includes('momentum')
    ) || [];

    if (highMomentumCards.length > 0 && gameState.conversationState?.focus.available >= 3) {
      viableStrategies++;
    }

    // Strategy 3: Setup/Planning for later turns
    const setupCards = availableActions.cards?.filter(card =>
      card.effect?.includes('Focusing') || card.name?.includes('mental_reset') || card.name?.includes('careful_words')
    ) || [];

    if (setupCards.length > 0) {
      viableStrategies++;
    }

    // Strategy 4: Resource conversion/efficiency
    const conversionCards = availableActions.cards?.filter(card =>
      card.effect?.includes('spend') || card.hasScaling
    ) || [];

    if (conversionCards.length > 0 && gameState.conversationState?.momentum >= 3) {
      viableStrategies++;
    }

    return Math.min(viableStrategies, 4); // Cap at 4 for analysis
  }

  /**
   * Calculate resource tension (competing demands)
   * Tension Criteria: Multiple competing demands (momentum vs doubt vs flow vs cards)
   */
  calculateResourceTension(gameState) {
    const conversationState = gameState.conversationState;
    if (!conversationState) return 0;

    let tensionPoints = 0;

    // Focus pressure
    const focusRatio = conversationState.focus.available / conversationState.focus.capacity;
    if (focusRatio < 0.5) tensionPoints += 2; // High focus pressure
    else if (focusRatio < 0.8) tensionPoints += 1; // Moderate focus pressure

    // Doubt pressure
    const doubtRatio = conversationState.doubt / 10; // Assuming max doubt of 10
    if (doubtRatio > 0.7) tensionPoints += 2; // High doubt pressure
    else if (doubtRatio > 0.4) tensionPoints += 1; // Moderate doubt pressure

    // Momentum opportunity cost
    if (conversationState.momentum >= 5 && conversationState.doubt > 3) {
      tensionPoints += 1; // Choice between spending momentum or saving for goals
    }

    // Hand size pressure
    if (conversationState.handSize <= 2) {
      tensionPoints += 1; // Card scarcity creates tension
    }

    return Math.min(tensionPoints, 5); // Normalize to 0-5 scale
  }

  /**
   * Assess planning depth required for optimal play
   * Planning Criteria: Setup turns enable bigger plays, requiring forethought
   */
  assessPlanningDepth(snapshot) {
    const { gameState } = snapshot;
    const conversationState = gameState.conversationState;
    if (!conversationState) return 0;

    let planningDepth = 0;

    // Check for setup cards that require future payoff
    const setupCards = snapshot.availableActions.cards?.filter(card =>
      card.name?.includes('mental_reset') ||
      card.name?.includes('careful_words') ||
      card.effect?.includes('Focusing')
    ) || [];

    if (setupCards.length > 0) {
      planningDepth += 2; // Setup cards require 2+ turn planning
    }

    // Check for scaling cards that benefit from timing
    const scalingCards = snapshot.availableActions.cards?.filter(card => card.hasScaling) || [];
    if (scalingCards.length > 0) {
      planningDepth += 1; // Scaling cards require situational awareness
    }

    // Check for high-focus cards requiring resource management
    const expensiveCards = snapshot.availableActions.cards?.filter(card => card.focus >= 4) || [];
    if (expensiveCards.length > 0 && conversationState.focus.available < 4) {
      planningDepth += 2; // Need to plan focus management
    }

    return Math.min(planningDepth, 4); // Normalize to 0-4 scale
  }

  /**
   * Assess risk/reward balance of available options
   */
  assessRiskLevel(snapshot) {
    const { gameState } = snapshot;
    const conversationState = gameState.conversationState;
    if (!conversationState) return 0;

    let riskLevel = 0;

    // High doubt creates risk
    if (conversationState.doubt > 7) {
      riskLevel += 2; // High-risk situation
    }

    // Low focus creates risk
    if (conversationState.focus.available <= 1) {
      riskLevel += 1; // Resource constraint risk
    }

    // High-focus cards in hand create opportunity
    const highFocusCards = snapshot.availableActions.cards?.filter(card => card.focus >= 4) || [];
    if (highFocusCards.length > 0) {
      riskLevel += 1; // Risk/reward from expensive cards
    }

    return Math.min(riskLevel, 4); // Normalize to 0-4 scale
  }

  /**
   * Generate strategic depth assessment report
   */
  generateStrategicDepthReport() {
    const metrics = this.strategicMetrics;

    const report = {
      averageViableStrategies: this.calculateAverage(metrics.viableStrategiesPerTurn),
      averageResourceTension: this.calculateAverage(metrics.resourceTension),
      averagePlanningDepth: this.calculateAverage(metrics.planningDepthRequired),
      averageRiskLevel: this.calculateAverage(metrics.riskRewardBalance),

      // Quality assessments
      strategicQuality: this.assessStrategicQuality(),
      tensionQuality: this.assessTensionQuality(),
      planningQuality: this.assessPlanningQuality(),

      // Decision point analysis
      totalDecisionPoints: this.decisionPoints.length,
      excellentDecisions: this.decisionPoints.filter(d => d.viableStrategies >= 3).length,
      poorDecisions: this.decisionPoints.filter(d => d.viableStrategies <= 1).length,

      recommendations: this.generateRecommendations()
    };

    return report;
  }

  calculateAverage(array) {
    if (array.length === 0) return 0;
    return array.reduce((sum, val) => sum + val, 0) / array.length;
  }

  assessStrategicQuality() {
    const avgStrategies = this.calculateAverage(this.strategicMetrics.viableStrategiesPerTurn);

    if (avgStrategies >= 2.5) return 'EXCELLENT';
    if (avgStrategies >= 2.0) return 'GOOD';
    if (avgStrategies >= 1.5) return 'ACCEPTABLE';
    return 'POOR';
  }

  assessTensionQuality() {
    const avgTension = this.calculateAverage(this.strategicMetrics.resourceTension);

    if (avgTension >= 2.5 && avgTension <= 4.0) return 'EXCELLENT';
    if (avgTension >= 2.0 && avgTension <= 4.5) return 'GOOD';
    if (avgTension >= 1.5) return 'ACCEPTABLE';
    return 'POOR';
  }

  assessPlanningQuality() {
    const avgPlanning = this.calculateAverage(this.strategicMetrics.planningDepthRequired);

    if (avgPlanning >= 2.0) return 'EXCELLENT';
    if (avgPlanning >= 1.5) return 'GOOD';
    if (avgPlanning >= 1.0) return 'ACCEPTABLE';
    return 'POOR';
  }

  generateRecommendations() {
    const recommendations = [];

    const avgStrategies = this.calculateAverage(this.strategicMetrics.viableStrategiesPerTurn);
    const avgTension = this.calculateAverage(this.strategicMetrics.resourceTension);
    const avgPlanning = this.calculateAverage(this.strategicMetrics.planningDepthRequired);

    if (avgStrategies < 2.0) {
      recommendations.push('Increase viable strategic options per turn - consider adding more card variety or effect types');
    }

    if (avgTension < 2.0) {
      recommendations.push('Increase resource tension - competing demands create engaging decisions');
    }

    if (avgPlanning < 1.5) {
      recommendations.push('Add more setup/payoff mechanics to encourage multi-turn planning');
    }

    if (this.decisionPoints.filter(d => d.viableStrategies <= 1).length > this.decisionPoints.length * 0.3) {
      recommendations.push('Too many turns with limited options - review card availability and costs');
    }

    return recommendations;
  }
}

module.exports = { StrategicAnalyzer };