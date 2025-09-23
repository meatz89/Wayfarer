/**
 * Elena Conversation Test Helper Functions
 *
 * These utilities provide reusable functions for testing Elena's strategic
 * conversation framework, focusing on validating the decision-making system.
 */

// Game Navigation Helpers
async function navigateToElena(page) {
  console.log('🎯 Navigating to Elena...');

  // Strategy 1: Direct navigation if Elena is visible immediately
  const directElenaButton = page.locator('button:has-text("Elena"), .npc[data-npc="elena"], .character:has-text("Elena")').first();
  if (await directElenaButton.isVisible({ timeout: 2000 })) {
    await directElenaButton.click();
    await page.waitForLoadState('networkidle');
    return true;
  }

  // Strategy 2: Navigate to Copper Kettle Tavern first
  const locationButtons = [
    'button:has-text("Tavern")',
    'button:has-text("Copper Kettle")',
    '.location:has-text("tavern")',
    '.location-button[data-location="copper_kettle_tavern"]',
    'button:has-text("Copper Kettle Tavern")'
  ];

  for (const selector of locationButtons) {
    const locationButton = page.locator(selector).first();
    if (await locationButton.isVisible({ timeout: 1000 })) {
      console.log(`📍 Found location button: ${selector}`);
      await locationButton.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);
      break;
    }
  }

  // Strategy 3: Look for Elena in current location
  const elenaSelectors = [
    'button:has-text("Elena")',
    '.npc:has-text("Elena")',
    '.character:has-text("scribe")',
    'button:has-text("scribe")',
    '.npc[data-npc="elena"]',
    'button[data-character="elena"]'
  ];

  for (const selector of elenaSelectors) {
    const elenaButton = page.locator(selector).first();
    if (await elenaButton.isVisible({ timeout: 2000 })) {
      console.log(`👤 Found Elena: ${selector}`);
      await elenaButton.click();
      await page.waitForLoadState('networkidle');
      return true;
    }
  }

  console.log('❌ Could not find Elena');
  return false;
}

async function startElenaConversation(page) {
  console.log('💬 Starting Elena conversation...');

  // Look for conversation initiation buttons
  const conversationSelectors = [
    'button:has-text("Talk")',
    'button:has-text("Speak")',
    'button:has-text("Conversation")',
    'button:has-text("Approach")',
    'button:has-text("Start Conversation")',
    '.conversation-button',
    '.action-talk',
    'button[data-action="conversation"]'
  ];

  for (const selector of conversationSelectors) {
    const startButton = page.locator(selector).first();
    if (await startButton.isVisible({ timeout: 2000 })) {
      console.log(`🎮 Found conversation button: ${selector}`);
      await startButton.click();
      await page.waitForTimeout(2000); // Wait for conversation UI to load
      await page.waitForLoadState('networkidle');

      // Verify conversation started
      const isInConversation = await verifyConversationState(page);
      if (isInConversation) {
        return true;
      }
    }
  }

  console.log('❌ Could not start conversation');
  return false;
}

async function verifyConversationState(page) {
  // Check for conversation-specific UI elements
  const conversationIndicators = [
    '.card, [data-test*="card"]',
    '.focus, [data-test="focus-display"]',
    '.momentum, [data-test="momentum-display"]',
    '.conversation-ui',
    '.hand, .card-hand'
  ];

  for (const selector of conversationIndicators) {
    if (await page.locator(selector).isVisible({ timeout: 1000 })) {
      console.log(`✅ Conversation active - found: ${selector}`);
      return true;
    }
  }

  return false;
}

// Resource Reading Helpers
async function getGameStateValues(page) {
  const state = {
    focus: 0,
    momentum: 0,
    doubt: 0,
    flow: 0,
    handSize: 0
  };

  try {
    // Focus
    const focusSelectors = ['[data-test="focus-display"]', '.focus-value', '.focus', '.resource-focus'];
    state.focus = await getNumericValueFromSelectors(page, focusSelectors);

    // Momentum
    const momentumSelectors = ['[data-test="momentum-display"]', '.momentum-value', '.momentum', '.resource-momentum'];
    state.momentum = await getNumericValueFromSelectors(page, momentumSelectors);

    // Doubt
    const doubtSelectors = ['[data-test="doubt-display"]', '.doubt-value', '.doubt', '.resource-doubt'];
    state.doubt = await getNumericValueFromSelectors(page, doubtSelectors);

    // Flow
    const flowSelectors = ['[data-test="flow-display"]', '.flow-value', '.flow', '.resource-flow'];
    state.flow = await getNumericValueFromSelectors(page, flowSelectors);

    // Hand size
    const handCards = page.locator('.card:not(.disabled), [data-test*="card"]:not(.disabled)');
    state.handSize = await handCards.count();

  } catch (error) {
    console.warn('⚠️ Could not read all game state values:', error);
  }

  return state;
}

async function getNumericValueFromSelectors(page, selectors) {
  for (const selector of selectors) {
    try {
      const element = page.locator(selector).first();
      if (await element.isVisible({ timeout: 500 })) {
        const text = await element.textContent();
        const match = text?.match(/\d+/);
        if (match) {
          return parseInt(match[0]);
        }
      }
    } catch (error) {
      // Continue to next selector
    }
  }
  return 0;
}

// Card Action Helpers
async function findCardByName(page, cardName) {
  const cardSelectors = [
    `[data-test="card-${cardName}"]`,
    `.card:has-text("${cardName}")`,
    `.card[data-card="${cardName}"]`,
    `.card-${cardName}`
  ];

  for (const selector of cardSelectors) {
    const card = page.locator(selector).first();
    if (await card.isVisible({ timeout: 1000 })) {
      return card;
    }
  }

  // Fallback: search by partial text content
  const cards = page.locator('.card');
  const cardCount = await cards.count();

  for (let i = 0; i < cardCount; i++) {
    const card = cards.nth(i);
    const text = await card.textContent();
    if (text && text.toLowerCase().includes(cardName.toLowerCase().replace('_', ' '))) {
      return card;
    }
  }

  return null;
}

async function playCard(page, cardName) {
  console.log(`🃏 Attempting to play card: ${cardName}`);

  const card = await findCardByName(page, cardName);
  if (!card) {
    console.log(`❌ Card not found: ${cardName}`);
    return false;
  }

  // Check if card is playable
  const isDisabled = await card.evaluate(el => el.classList.contains('disabled') || el.classList.contains('unplayable'));
  if (isDisabled) {
    console.log(`❌ Card is disabled: ${cardName}`);
    return false;
  }

  // Play the card
  await card.click();
  await page.waitForTimeout(1000);
  console.log(`✅ Played card: ${cardName}`);
  return true;
}

async function playMomentumGeneratingCards(page, targetMomentum) {
  console.log(`🎯 Building momentum to ${targetMomentum}...`);

  const initialState = await getGameStateValues(page);
  let currentMomentum = initialState.momentum;
  let attempts = 0;
  const maxAttempts = 10;

  const momentumCards = [
    'gentle_agreement',
    'build_rapport',
    'passionate_plea',
    'burning_conviction',
    'emotional_appeal'
  ];

  while (currentMomentum < targetMomentum && attempts < maxAttempts) {
    let cardPlayed = false;

    for (const cardName of momentumCards) {
      if (await playCard(page, cardName)) {
        cardPlayed = true;
        break;
      }
    }

    if (!cardPlayed) {
      // Try to end turn and continue
      const endTurnButton = page.locator('button:has-text("End Turn"), button:has-text("Continue"), button:has-text("Next Turn")').first();
      if (await endTurnButton.isVisible({ timeout: 1000 })) {
        await endTurnButton.click();
        await page.waitForTimeout(1000);
      } else {
        break; // Can't progress further
      }
    }

    // Update momentum reading
    const newState = await getGameStateValues(page);
    currentMomentum = newState.momentum;
    attempts++;
  }

  console.log(`📊 Final momentum: ${currentMomentum} (target: ${targetMomentum})`);
  return currentMomentum >= targetMomentum;
}

async function buildUpDoubt(page, targetDoubt = 5) {
  console.log(`⚠️ Building doubt to ${targetDoubt}...`);

  // Simulate actions that increase doubt
  const passSelectors = [
    'button:has-text("Pass")',
    'button:has-text("Skip")',
    'button:has-text("Wait")',
    'button:has-text("End Turn")',
    '.pass-button'
  ];

  for (let i = 0; i < targetDoubt; i++) {
    let actionTaken = false;

    for (const selector of passSelectors) {
      const button = page.locator(selector).first();
      if (await button.isVisible({ timeout: 1000 })) {
        await button.click();
        await page.waitForTimeout(1000);
        actionTaken = true;
        break;
      }
    }

    if (!actionTaken) {
      // Try playing a card that might fail
      const cards = page.locator('.card');
      const cardCount = await cards.count();
      if (cardCount > 0) {
        await cards.nth(0).click();
        await page.waitForTimeout(1000);
      }
    }
  }

  const finalState = await getGameStateValues(page);
  console.log(`📊 Final doubt: ${finalState.doubt}`);
  return finalState.doubt;
}

// Screenshot Helpers
async function captureStrategicState(page, scenarioName, additionalInfo = '') {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const filename = `test-results/elena-${scenarioName}-${timestamp}.png`;

  const state = await getGameStateValues(page);
  console.log(`📸 Capturing ${scenarioName}: Focus=${state.focus}, Momentum=${state.momentum}, Doubt=${state.doubt}, Flow=${state.flow}, Hand=${state.handSize}`);

  await page.screenshot({
    path: filename,
    fullPage: true
  });

  return filename;
}

// Validation Helpers
async function validateFocusConstraint(page, maxFocus = 4) {
  const state = await getGameStateValues(page);
  const cards = page.locator('.card:not(.disabled)');
  const cardCount = await cards.count();

  let totalPlayableCost = 0;
  let playableCards = [];

  for (let i = 0; i < cardCount; i++) {
    const card = cards.nth(i);
    const text = await card.textContent();

    // Extract focus cost (this logic may need adjustment based on UI)
    let focusCost = 1; // default
    if (text?.includes('2 focus') || text?.includes('focus: 2')) focusCost = 2;
    if (text?.includes('3 focus') || text?.includes('focus: 3')) focusCost = 3;
    if (text?.includes('4 focus') || text?.includes('focus: 4')) focusCost = 4;
    if (text?.includes('5 focus') || text?.includes('focus: 5')) focusCost = 5;

    if (focusCost <= state.focus) {
      playableCards.push({ cost: focusCost, text });
      totalPlayableCost += focusCost;
    }
  }

  const hasConstraint = totalPlayableCost > state.focus;
  console.log(`🎯 Focus constraint validation: Current=${state.focus}, Playable cost=${totalPlayableCost}, Constraint exists=${hasConstraint}`);

  return {
    currentFocus: state.focus,
    totalPlayableCost,
    hasConstraint,
    playableCards
  };
}

async function validateStrategicTension(page) {
  const state = await getGameStateValues(page);

  // Count different types of strategic options
  const momentumCards = await page.locator('.card:has-text("momentum"), .card:has-text("rapport"), .card:has-text("agreement")').count();
  const doubtCards = await page.locator('.card:has-text("doubt"), .card:has-text("reflect"), .card:has-text("confusion")').count();
  const utilityCards = await page.locator('.card:has-text("insight"), .card:has-text("draw"), .card:has-text("focus")').count();
  const conversionCards = await page.locator('.card:has-text("spend"), .card:has-text("invest"), .card:has-text("flow")').count();

  const strategicOptions = [momentumCards, doubtCards, utilityCards, conversionCards].filter(count => count > 0).length;

  console.log(`⚖️ Strategic tension: ${strategicOptions} different strategy types available`);
  console.log(`   Momentum: ${momentumCards}, Doubt: ${doubtCards}, Utility: ${utilityCards}, Conversion: ${conversionCards}`);

  return {
    strategicOptions,
    hasMultipleStrategies: strategicOptions >= 2,
    breakdown: { momentumCards, doubtCards, utilityCards, conversionCards }
  };
}

module.exports = {
  navigateToElena,
  startElenaConversation,
  verifyConversationState,
  getGameStateValues,
  getNumericValueFromSelectors,
  findCardByName,
  playCard,
  playMomentumGeneratingCards,
  buildUpDoubt,
  captureStrategicState,
  validateFocusConstraint,
  validateStrategicTension
};