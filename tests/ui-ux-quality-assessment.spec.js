/**
 * UI/UX Quality Assessment Tests
 * Evaluates information clarity, visual feedback, and user experience
 */

const { test, expect } = require('@playwright/test');
const { GameHelpers } = require('./utils/game-helpers');

test.describe('UI/UX Quality Assessment', () => {
  let gameHelpers;

  test.beforeEach(async ({ page }) => {
    gameHelpers = new GameHelpers(page);
    await page.goto('/');
    await gameHelpers.waitForGameReady();
    await gameHelpers.startNewGame();
  });

  test('Information Clarity: All relevant game state visible and understandable', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const clarityAnalysis = {
      requiredElements: {
        momentum: false,
        doubt: false,
        focus: false,
        flowState: false,
        npcName: false,
        cardHand: false,
        actionButtons: false
      },
      accessibilityScore: 0,
      readabilityIssues: [],
      missingInformation: []
    };

    // Check presence of essential UI elements
    const momentumElement = page.locator('.momentum-counter, .momentum-display');
    clarityAnalysis.requiredElements.momentum = await momentumElement.count() > 0;

    const doubtElements = page.locator('.doubt-slot, .doubt-display');
    clarityAnalysis.requiredElements.doubt = await doubtElements.count() > 0;

    const focusElements = page.locator('.focus-dot, .focus-display');
    clarityAnalysis.requiredElements.focus = await focusElements.count() > 0;

    const flowElements = page.locator('.flow-segment, .current-state');
    clarityAnalysis.requiredElements.flowState = await flowElements.count() > 0;

    const npcNameElement = page.locator('.npc-name, .npc-info');
    clarityAnalysis.requiredElements.npcName = await npcNameElement.count() > 0;

    const cardElements = page.locator('.card, .card-grid');
    clarityAnalysis.requiredElements.cardHand = await cardElements.count() > 0;

    const actionElements = page.locator('.action-button, .action-choices');
    clarityAnalysis.requiredElements.actionButtons = await actionElements.count() > 0;

    // Take screenshot for visual inspection
    await gameHelpers.takeScreenshot('ui_information_clarity_overview');

    // Test information accessibility and readability
    for (const [element, present] of Object.entries(clarityAnalysis.requiredElements)) {
      if (present) {
        clarityAnalysis.accessibilityScore++;
      } else {
        clarityAnalysis.missingInformation.push(element);
      }
    }

    // Check text readability
    const textElements = await page.locator('.card-text, .npc-dialogue, .narrative-text, .card-effect').all();
    for (let i = 0; i < Math.min(textElements.length, 10); i++) {
      const element = textElements[i];
      const text = await element.textContent();
      const styles = await element.evaluate(el => {
        const style = window.getComputedStyle(el);
        return {
          fontSize: style.fontSize,
          lineHeight: style.lineHeight,
          color: style.color,
          backgroundColor: style.backgroundColor,
          fontWeight: style.fontWeight
        };
      });

      // Check for readability issues
      const fontSize = parseFloat(styles.fontSize);
      if (fontSize < 12) {
        clarityAnalysis.readabilityIssues.push(`Text too small: ${fontSize}px`);
      }

      if (text && text.length > 100 && !styles.lineHeight.includes('px') && styles.lineHeight === 'normal') {
        clarityAnalysis.readabilityIssues.push('Long text without explicit line-height');
      }
    }

    // Test information updates in real-time
    const beforeState = await gameHelpers.getGameState();
    await gameHelpers.executeListen();
    await gameHelpers.waitForNarrativeGeneration();

    const cards = await gameHelpers.getHandCards();
    if (cards.length > 0) {
      await gameHelpers.selectCard(0);
      await gameHelpers.executeSpeak();
      await gameHelpers.waitForNarrativeGeneration();
    }

    const afterState = await gameHelpers.getGameState();

    // Verify UI updates match state changes
    const stateChanges = {
      momentumChanged: afterState.conversationState.momentum !== beforeState.conversationState.momentum,
      doubtChanged: afterState.conversationState.doubt !== beforeState.conversationState.doubt,
      focusChanged: afterState.conversationState.focus.available !== beforeState.conversationState.focus.available,
      handChanged: afterState.conversationState.handSize !== beforeState.conversationState.handSize
    };

    await gameHelpers.takeScreenshot('ui_after_state_change');

    const accessibilityPercentage = (clarityAnalysis.accessibilityScore / Object.keys(clarityAnalysis.requiredElements).length) * 100;

    console.log(`Information Clarity Analysis:
      - Required elements present: ${clarityAnalysis.accessibilityScore}/${Object.keys(clarityAnalysis.requiredElements).length} (${accessibilityPercentage.toFixed(1)}%)
      - Missing information: ${clarityAnalysis.missingInformation.join(', ') || 'None'}
      - Readability issues: ${clarityAnalysis.readabilityIssues.length}
      - State changes detected: ${Object.entries(stateChanges).filter(([key, changed]) => changed).map(([key]) => key).join(', ')}
      - Readability issues: ${JSON.stringify(clarityAnalysis.readabilityIssues, null, 2)}`);

    // Information clarity assertions
    expect(accessibilityPercentage).toBeGreaterThan(85); // 85%+ required elements present
    expect(clarityAnalysis.readabilityIssues.length).toBeLessThan(3); // Minimal readability issues
    expect(Object.values(stateChanges).some(changed => changed)).toBeTruthy(); // UI should update with state
  });

  test('Effect Previews: Players can predict card outcomes before playing', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const previewAnalysis = {
      cardsWithPreviews: 0,
      cardsWithoutPreviews: 0,
      accuratePreviews: 0,
      inaccuratePreviews: 0,
      previewDetails: []
    };

    for (let turn = 0; turn < 5; turn++) {
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();
      if (cards.length === 0) break;

      // Analyze preview information for each card
      for (let cardIndex = 0; cardIndex < Math.min(cards.length, 3); cardIndex++) {
        const card = cards[cardIndex];
        const beforeState = await gameHelpers.getGameState();

        // Check if card has preview information
        const cardElement = page.locator('.card').nth(cardIndex);
        const hasEffectText = card.effect && card.effect.length > 0;
        const hasFocusCost = card.focus !== undefined;
        const hasDialoguePreview = card.dialogueFragment && card.dialogueFragment.length > 0;

        let previewQuality = 0;
        if (hasEffectText) previewQuality++;
        if (hasFocusCost) previewQuality++;
        if (hasDialoguePreview) previewQuality++;

        if (previewQuality >= 2) {
          previewAnalysis.cardsWithPreviews++;
        } else {
          previewAnalysis.cardsWithoutPreviews++;
        }

        // Test preview accuracy by playing the card
        if (card.isSelectable && card.focus <= beforeState.conversationState.focus.available) {
          await gameHelpers.selectCard(card.name);

          // Take screenshot before playing for preview comparison
          await gameHelpers.takeScreenshot(`card_preview_${card.name}_turn_${turn + 1}`);

          await gameHelpers.executeSpeak();
          await gameHelpers.waitForNarrativeGeneration();

          const afterState = await gameHelpers.getGameState();

          // Analyze prediction accuracy
          const actualMomentumChange = afterState.conversationState.momentum - beforeState.conversationState.momentum;
          const actualDoubtChange = afterState.conversationState.doubt - beforeState.conversationState.doubt;
          const actualFocusChange = afterState.conversationState.focus.available - beforeState.conversationState.focus.available;

          // Predict effects based on card description
          const predictedPositiveMomentum = card.effect?.includes('Strike') || card.effect?.includes('momentum');
          const predictedDoubtReduction = card.effect?.includes('Soothe') || card.effect?.includes('reduce doubt');
          const expectedFocusChange = -card.focus;

          let predictionAccuracy = 0;
          let totalPredictions = 0;

          // Check momentum prediction
          if (predictedPositiveMomentum) {
            totalPredictions++;
            if (actualMomentumChange > 0) predictionAccuracy++;
          }

          // Check doubt prediction
          if (predictedDoubtReduction) {
            totalPredictions++;
            if (actualDoubtChange < 0) predictionAccuracy++;
          }

          // Check focus prediction (should always be accurate)
          totalPredictions++;
          if (Math.abs(actualFocusChange - expectedFocusChange) <= 1) predictionAccuracy++;

          if (totalPredictions > 0 && predictionAccuracy / totalPredictions >= 0.8) {
            previewAnalysis.accuratePreviews++;
          } else {
            previewAnalysis.inaccuratePreviews++;
          }

          previewAnalysis.previewDetails.push({
            cardName: card.name,
            effect: card.effect,
            focus: card.focus,
            predicted: {
              positiveMomentum: predictedPositiveMomentum,
              doubtReduction: predictedDoubtReduction,
              focusChange: expectedFocusChange
            },
            actual: {
              momentumChange: actualMomentumChange,
              doubtChange: actualDoubtChange,
              focusChange: actualFocusChange
            },
            accuracy: totalPredictions > 0 ? (predictionAccuracy / totalPredictions) * 100 : 0
          });

          break; // Only test one card per turn
        }
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    const previewPercentage = previewAnalysis.cardsWithPreviews + previewAnalysis.cardsWithoutPreviews > 0 ?
      (previewAnalysis.cardsWithPreviews / (previewAnalysis.cardsWithPreviews + previewAnalysis.cardsWithoutPreviews)) * 100 : 0;

    const accuracyPercentage = previewAnalysis.accuratePreviews + previewAnalysis.inaccuratePreviews > 0 ?
      (previewAnalysis.accuratePreviews / (previewAnalysis.accuratePreviews + previewAnalysis.inaccuratePreviews)) * 100 : 0;

    console.log(`Effect Preview Analysis:
      - Cards with good previews: ${previewAnalysis.cardsWithPreviews} (${previewPercentage.toFixed(1)}%)
      - Cards with poor previews: ${previewAnalysis.cardsWithoutPreviews}
      - Accurate predictions: ${previewAnalysis.accuratePreviews} (${accuracyPercentage.toFixed(1)}%)
      - Inaccurate predictions: ${previewAnalysis.inaccuratePreviews}
      - Preview details: ${JSON.stringify(previewAnalysis.previewDetails, null, 2)}`);

    // Preview quality assertions
    expect(previewPercentage).toBeGreaterThan(80); // 80%+ cards should have good previews
    if (previewAnalysis.accuratePreviews + previewAnalysis.inaccuratePreviews > 0) {
      expect(accuracyPercentage).toBeGreaterThan(70); // 70%+ prediction accuracy
    }
  });

  test('Visual Feedback: Clear indication of successful/failed actions', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const feedbackAnalysis = {
      actionsWithFeedback: 0,
      actionsWithoutFeedback: 0,
      visualFeedbackTypes: new Set(),
      feedbackTimings: [],
      feedbackExamples: []
    };

    for (let turn = 0; turn < 6; turn++) {
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();

      const cards = await gameHelpers.getHandCards();
      if (cards.length === 0) break;

      // Monitor for visual feedback during action
      const beforeState = await gameHelpers.getGameState();

      if (cards[0] && cards[0].isSelectable) {
        // Select a card and monitor feedback
        await gameHelpers.selectCard(cards[0].name);

        // Check for selection feedback
        const selectedCard = page.locator('.card.selected, .card[data-selected="true"]');
        const hasSelectionFeedback = await selectedCard.count() > 0;

        if (hasSelectionFeedback) {
          feedbackAnalysis.visualFeedbackTypes.add('card_selection');
        }

        // Take screenshot of selection state
        await gameHelpers.takeScreenshot(`visual_feedback_selection_turn_${turn + 1}`);

        const speakStartTime = Date.now();
        await gameHelpers.executeSpeak();

        // Monitor for processing feedback
        const processingIndicator = page.locator(':text("Processing"), .processing, .loading');
        const hasProcessingFeedback = await processingIndicator.count() > 0;

        if (hasProcessingFeedback) {
          feedbackAnalysis.visualFeedbackTypes.add('processing_indicator');
        }

        await gameHelpers.waitForNarrativeGeneration();
        const actionCompleteTime = Date.now();

        const actionDuration = actionCompleteTime - speakStartTime;
        feedbackAnalysis.feedbackTimings.push(actionDuration);

        const afterState = await gameHelpers.getGameState();

        // Check for result feedback
        const momentumChange = afterState.conversationState.momentum - beforeState.conversationState.momentum;
        const doubtChange = afterState.conversationState.doubt - beforeState.conversationState.doubt;

        // Look for visual indicators of change
        const momentumDisplay = page.locator('.momentum-counter, .momentum-display');
        const doubtDisplay = page.locator('.doubt-slot, .doubt-display');

        const momentumVisible = await momentumDisplay.count() > 0;
        const doubtVisible = await doubtDisplay.count() > 0;

        let feedbackPresent = false;

        if (momentumChange !== 0 && momentumVisible) {
          feedbackAnalysis.visualFeedbackTypes.add('momentum_change');
          feedbackPresent = true;
        }

        if (doubtChange !== 0 && doubtVisible) {
          feedbackAnalysis.visualFeedbackTypes.add('doubt_change');
          feedbackPresent = true;
        }

        // Check for narrative feedback
        const narrativeElement = page.locator('.narrative-text, .npc-dialogue');
        const hasNarrativeFeedback = await narrativeElement.count() > 0;

        if (hasNarrativeFeedback) {
          feedbackAnalysis.visualFeedbackTypes.add('narrative_response');
          feedbackPresent = true;
        }

        // Check for animation or transition effects
        const animatedElements = page.locator('[class*="animate"], [class*="transition"], .card.animating');
        const hasAnimations = await animatedElements.count() > 0;

        if (hasAnimations) {
          feedbackAnalysis.visualFeedbackTypes.add('animations');
          feedbackPresent = true;
        }

        if (feedbackPresent) {
          feedbackAnalysis.actionsWithFeedback++;
        } else {
          feedbackAnalysis.actionsWithoutFeedback++;
        }

        feedbackAnalysis.feedbackExamples.push({
          turn: turn + 1,
          cardPlayed: cards[0].name,
          momentumChange,
          doubtChange,
          actionDuration,
          feedbackTypes: Array.from(feedbackAnalysis.visualFeedbackTypes),
          hasFeedback: feedbackPresent
        });

        // Take screenshot after action for comparison
        await gameHelpers.takeScreenshot(`visual_feedback_result_turn_${turn + 1}`);
      }

      if (await gameHelpers.isConversationExhausted()) break;
    }

    const feedbackPercentage = feedbackAnalysis.actionsWithFeedback + feedbackAnalysis.actionsWithoutFeedback > 0 ?
      (feedbackAnalysis.actionsWithFeedback / (feedbackAnalysis.actionsWithFeedback + feedbackAnalysis.actionsWithoutFeedback)) * 100 : 0;

    const avgActionDuration = feedbackAnalysis.feedbackTimings.length > 0 ?
      feedbackAnalysis.feedbackTimings.reduce((a, b) => a + b, 0) / feedbackAnalysis.feedbackTimings.length : 0;

    console.log(`Visual Feedback Analysis:
      - Actions with feedback: ${feedbackAnalysis.actionsWithFeedback} (${feedbackPercentage.toFixed(1)}%)
      - Actions without feedback: ${feedbackAnalysis.actionsWithoutFeedback}
      - Feedback types found: ${Array.from(feedbackAnalysis.visualFeedbackTypes).join(', ')}
      - Average action duration: ${avgActionDuration.toFixed(0)}ms
      - Feedback examples: ${JSON.stringify(feedbackAnalysis.feedbackExamples, null, 2)}`);

    // Visual feedback assertions
    expect(feedbackPercentage).toBeGreaterThan(90); // 90%+ actions should have feedback
    expect(feedbackAnalysis.visualFeedbackTypes.size).toBeGreaterThan(2); // Multiple feedback types
    expect(avgActionDuration).toBeLessThan(5000); // Actions should complete in reasonable time
  });

  test('Performance: Smooth gameplay without lag or visual glitches', async ({ page }) => {
    await gameHelpers.startConversation('Elena');

    const performanceAnalysis = {
      actionTimings: [],
      renderingIssues: [],
      memorySnapshots: [],
      smoothnessScore: 0,
      totalActions: 0
    };

    // Monitor performance metrics
    const startMemory = await page.evaluate(() => performance.memory?.usedJSHeapSize || 0);
    performanceAnalysis.memorySnapshots.push({ turn: 0, memory: startMemory });

    for (let turn = 0; turn < 8; turn++) {
      const turnStartTime = Date.now();

      // Test LISTEN performance
      const listenStartTime = performance.now();
      await gameHelpers.executeListen();
      await gameHelpers.waitForNarrativeGeneration();
      const listenEndTime = performance.now();
      const listenDuration = listenEndTime - listenStartTime;

      performanceAnalysis.actionTimings.push({
        action: 'listen',
        turn: turn + 1,
        duration: listenDuration
      });

      const cards = await gameHelpers.getHandCards();
      if (cards.length === 0) break;

      // Test card interaction performance
      const cardInteractionStart = performance.now();
      await gameHelpers.selectCard(0);

      // Check for visual lag during selection
      const selectionDelay = 100; // Expected max selection time
      await page.waitForTimeout(selectionDelay);

      const selectedElements = page.locator('.card.selected, .card[data-selected="true"]');
      const selectionWorks = await selectedElements.count() > 0;

      if (!selectionWorks) {
        performanceAnalysis.renderingIssues.push({
          turn: turn + 1,
          issue: 'Card selection not reflected in UI',
          timestamp: Date.now()
        });
      }

      // Test SPEAK performance
      const speakStartTime = performance.now();
      await gameHelpers.executeSpeak();
      await gameHelpers.waitForNarrativeGeneration();
      const speakEndTime = performance.now();
      const speakDuration = speakEndTime - speakStartTime;

      performanceAnalysis.actionTimings.push({
        action: 'speak',
        turn: turn + 1,
        duration: speakDuration
      });

      const cardInteractionEnd = performance.now();
      const totalInteractionTime = cardInteractionEnd - cardInteractionStart;

      performanceAnalysis.actionTimings.push({
        action: 'full_interaction',
        turn: turn + 1,
        duration: totalInteractionTime
      });

      // Monitor memory usage
      const currentMemory = await page.evaluate(() => performance.memory?.usedJSHeapSize || 0);
      performanceAnalysis.memorySnapshots.push({ turn: turn + 1, memory: currentMemory });

      // Check for layout shifts or visual glitches
      const elementCount = await page.locator('.card, .action-button, .momentum-display, .doubt-display').count();
      if (elementCount < 5) { // Expect at least 5 UI elements
        performanceAnalysis.renderingIssues.push({
          turn: turn + 1,
          issue: `Unexpected element count: ${elementCount}`,
          timestamp: Date.now()
        });
      }

      // Rate smoothness based on timing
      if (listenDuration < 2000 && speakDuration < 3000 && totalInteractionTime < 4000) {
        performanceAnalysis.smoothnessScore++;
      }

      performanceAnalysis.totalActions++;

      // Take performance screenshot
      await gameHelpers.takeScreenshot(`performance_turn_${turn + 1}`);

      if (await gameHelpers.isConversationExhausted()) break;
    }

    // Analyze performance metrics
    const avgListenTime = performanceAnalysis.actionTimings
      .filter(t => t.action === 'listen')
      .reduce((sum, t) => sum + t.duration, 0) / performanceAnalysis.actionTimings.filter(t => t.action === 'listen').length;

    const avgSpeakTime = performanceAnalysis.actionTimings
      .filter(t => t.action === 'speak')
      .reduce((sum, t) => sum + t.duration, 0) / performanceAnalysis.actionTimings.filter(t => t.action === 'speak').length;

    const avgInteractionTime = performanceAnalysis.actionTimings
      .filter(t => t.action === 'full_interaction')
      .reduce((sum, t) => sum + t.duration, 0) / performanceAnalysis.actionTimings.filter(t => t.action === 'full_interaction').length;

    const memoryGrowth = performanceAnalysis.memorySnapshots.length > 1 ?
      performanceAnalysis.memorySnapshots[performanceAnalysis.memorySnapshots.length - 1].memory - performanceAnalysis.memorySnapshots[0].memory : 0;

    const smoothnessPercentage = performanceAnalysis.totalActions > 0 ?
      (performanceAnalysis.smoothnessScore / performanceAnalysis.totalActions) * 100 : 0;

    console.log(`Performance Analysis:
      - Average LISTEN time: ${avgListenTime.toFixed(0)}ms
      - Average SPEAK time: ${avgSpeakTime.toFixed(0)}ms
      - Average full interaction: ${avgInteractionTime.toFixed(0)}ms
      - Smoothness score: ${performanceAnalysis.smoothnessScore}/${performanceAnalysis.totalActions} (${smoothnessPercentage.toFixed(1)}%)
      - Memory growth: ${(memoryGrowth / 1024 / 1024).toFixed(2)}MB
      - Rendering issues: ${performanceAnalysis.renderingIssues.length}
      - Action timings: ${JSON.stringify(performanceAnalysis.actionTimings, null, 2)}
      - Rendering issues: ${JSON.stringify(performanceAnalysis.renderingIssues, null, 2)}`);

    // Performance assertions
    expect(avgListenTime).toBeLessThan(3000); // LISTEN should complete in <3s
    expect(avgSpeakTime).toBeLessThan(4000); // SPEAK should complete in <4s
    expect(avgInteractionTime).toBeLessThan(5000); // Full interaction in <5s
    expect(smoothnessPercentage).toBeGreaterThan(70); // 70%+ smooth interactions
    expect(performanceAnalysis.renderingIssues.length).toBeLessThan(2); // Minimal rendering issues
    expect(memoryGrowth).toBeLessThan(50 * 1024 * 1024); // <50MB memory growth
  });
});