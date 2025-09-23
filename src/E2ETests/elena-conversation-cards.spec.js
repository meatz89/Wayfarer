// @ts-check
const { test, expect } = require('@playwright/test');

/**
 * Expected cards from the 19-card deck (deck_desperate_empathy)
 * Based on cards.json data
 */
const EXPECTED_CARDS = [
  // Tier 1 (1-2 focus): 7 cards
  { id: 'gentle_agreement', name: 'gentle agreement', focus: 1, tier: 1 },
  { id: 'quick_insight', name: 'quick insight', focus: 1, tier: 1 },
  { id: 'pause_reflect', name: 'pause reflect', focus: 1, tier: 1 },
  { id: 'build_rapport', name: 'build rapport', focus: 2, tier: 1 },
  { id: 'desperate_gambit', name: 'desperate gambit', focus: 2, tier: 1 },
  { id: 'emotional_appeal', name: 'emotional appeal', focus: 2, tier: 1 },
  { id: 'careful_words', name: 'careful words', focus: 2, tier: 1 },

  // Tier 2 (3-4 focus): 8 cards
  { id: 'passionate_plea', name: 'passionate plea', focus: 3, tier: 2 },
  { id: 'clear_confusion', name: 'clear confusion', focus: 3, tier: 2 },
  { id: 'establish_trust', name: 'establish trust', focus: 3, tier: 2 },
  { id: 'racing_thoughts', name: 'racing thoughts', focus: 3, tier: 2 },
  { id: 'show_understanding', name: 'show understanding', focus: 3, tier: 2 },
  { id: 'build_pressure', name: 'build pressure', focus: 4, tier: 2 },
  { id: 'authoritative_demand', name: 'authoritative demand', focus: 4, tier: 2 },
  { id: 'strategic_pause', name: 'strategic pause', focus: 4, tier: 2 },

  // Tier 3 (5-6 focus): 3 cards
  { id: 'burning_conviction', name: 'burning conviction', focus: 5, tier: 3 },
  { id: 'moment_of_truth', name: 'moment of truth', focus: 5, tier: 3 },
  { id: 'deep_understanding', name: 'deep understanding', focus: 6, tier: 3 },

  // Special case: mental_reset has 0 focus
  { id: 'mental_reset', name: 'mental reset', focus: 0, tier: 0 }
];

/**
 * Cards with special momentum scaling effects
 */
const MOMENTUM_SCALING_CARDS = [
  { id: 'clear_confusion', expectedEffect: 'Spend 2 momentum → -3 doubt' },
  { id: 'establish_trust', expectedEffect: 'Spend 3 momentum → +1 flow' },
  { id: 'moment_of_truth', expectedEffect: 'Spend 4 momentum → +2 flow' },
  { id: 'deep_understanding', expectedEffect: 'Cards in hand' },
  { id: 'show_understanding', expectedEffect: 'Cards in hand divided' },
  { id: 'build_pressure', expectedEffect: 'Doubt reduction' },
  { id: 'desperate_gambit', expectedEffect: 'Doubt multiplier' },
  { id: 'careful_words', expectedEffect: 'Card discard' },
  { id: 'strategic_pause', expectedEffect: 'Prevent doubt' }
];

test.describe('Elena Conversation - Card Display and Effects', () => {

  test.beforeEach(async ({ page }) => {
    // Navigate to the game
    await page.goto('/');

    // Wait for the game to load
    await page.waitForSelector('.game-container', { timeout: 15000 });

    // Take a screenshot to see what's available
    await page.screenshot({ path: 'initial-load.png', fullPage: true });

    // Look for Elena in any form - might be in different locations
    const possibleElenaSelectors = [
      'text=Elena',
      '[data-npc="elena"]',
      '.npc-card:has-text("Elena")',
      '.location-npc:has-text("Elena")',
      'button:has-text("Elena")',
      '.npc:has-text("Elena")'
    ];

    let elenaFound = false;

    for (const selector of possibleElenaSelectors) {
      const elenaElement = page.locator(selector);
      if (await elenaElement.count() > 0) {
        console.log(`Found Elena with selector: ${selector}`);
        await elenaElement.first().click();
        elenaFound = true;
        break;
      }
    }

    if (!elenaFound) {
      // Try navigating to different areas
      const navigationOptions = [
        'text=Tavern',
        'text=Town Square',
        'text=Market',
        '.location-button',
        '.nav-button'
      ];

      for (const navOption of navigationOptions) {
        const navElement = page.locator(navOption);
        if (await navElement.count() > 0) {
          console.log(`Trying navigation: ${navOption}`);
          await navElement.first().click();
          await page.waitForTimeout(2000);

          // Check for Elena again
          for (const selector of possibleElenaSelectors) {
            const elenaElement = page.locator(selector);
            if (await elenaElement.count() > 0) {
              console.log(`Found Elena after navigation with selector: ${selector}`);
              await elenaElement.first().click();
              elenaFound = true;
              break;
            }
          }

          if (elenaFound) break;
        }
      }
    }

    if (!elenaFound) {
      await page.screenshot({ path: 'elena-not-found.png', fullPage: true });
      throw new Error('Could not find Elena NPC');
    }

    // Look for conversation start options
    const conversationStarters = [
      'text=Start Conversation',
      'text=Talk',
      'text=Speak',
      'text=Desperate Request',
      '.conversation-option',
      '.conversation-type',
      'button:has-text("conversation")'
    ];

    let conversationStarted = false;

    for (const starter of conversationStarters) {
      const startElement = page.locator(starter);
      if (await startElement.count() > 0) {
        console.log(`Starting conversation with: ${starter}`);
        await startElement.first().click();
        conversationStarted = true;
        break;
      }
    }

    if (!conversationStarted) {
      await page.screenshot({ path: 'conversation-not-started.png', fullPage: true });
      throw new Error('Could not start conversation with Elena');
    }

    // Wait for conversation interface to load
    await page.waitForSelector('.conversation-content', { timeout: 15000 });

    // Look for LISTEN button and initial card draw
    const listenButton = page.locator('text=LISTEN').or(page.locator('.action-button:has-text("LISTEN")'));
    if (await listenButton.count() > 0) {
      await listenButton.click();
      await page.waitForSelector('.card-grid .card', { timeout: 15000 });
    } else {
      // Cards might already be visible
      await page.waitForSelector('.card-grid .card', { timeout: 10000 });
    }
  });

  test('should display all 19 unique cards from deck_desperate_empathy', async ({ page }) => {
    // Track all cards seen across multiple draws
    const cardsSeenIds = new Set();
    const cardsSeenNames = new Set();
    let drawAttempts = 0;
    const maxDraws = 10; // Prevent infinite loops

    while (cardsSeenIds.size < EXPECTED_CARDS.length && drawAttempts < maxDraws) {
      // Get currently visible cards
      const cardElements = await page.locator('.card-grid .card').all();

      for (const cardElement of cardElements) {
        // Extract card name from the card-name element
        const cardNameElement = cardElement.locator('.card-name');
        if (await cardNameElement.count() > 0) {
          const cardName = await cardNameElement.textContent();
          if (cardName) {
            const cleanName = cardName.replace(/\+\d+ XP$/, '').trim().toLowerCase();
            cardsSeenNames.add(cleanName);

            // Find corresponding ID
            const expectedCard = EXPECTED_CARDS.find(c => c.name === cleanName);
            if (expectedCard) {
              cardsSeenIds.add(expectedCard.id);
            }
          }
        }
      }

      // If we haven't seen all cards, try to draw more
      if (cardsSeenIds.size < EXPECTED_CARDS.length) {
        // Try to play a low-cost card to continue the conversation
        const lowCostCard = page.locator('.card').filter({ hasText: '1' }).first();
        if (await lowCostCard.count() > 0) {
          await lowCostCard.click();
          await page.click('text=SPEAK');
          await page.waitForTimeout(1000); // Wait for action to process

          // Listen again to draw new cards
          const listenButton = page.locator('text=LISTEN');
          if (await listenButton.count() > 0) {
            await listenButton.click();
            await page.waitForTimeout(1000);
          }
        }
      }

      drawAttempts++;
    }

    // Take a screenshot for debugging
    await page.screenshot({ path: 'elena-cards-full-test.png', fullPage: true });

    // Verify we found all expected cards
    console.log(`Found ${cardsSeenIds.size} unique cards out of ${EXPECTED_CARDS.length} expected`);
    console.log('Cards seen:', Array.from(cardsSeenNames).sort());
    console.log('Expected cards:', EXPECTED_CARDS.map(c => c.name).sort());

    // Check for each expected card
    for (const expectedCard of EXPECTED_CARDS) {
      expect(cardsSeenIds.has(expectedCard.id),
        `Missing card: ${expectedCard.name} (${expectedCard.id})`).toBeTruthy();
    }

    // Verify no duplicates in names (each card should appear only once)
    expect(cardsSeenNames.size).toBe(EXPECTED_CARDS.length);
  });

  test('should display correct focus costs for all power tiers', async ({ page }) => {
    const focusCostsSeen = new Map(); // card name -> focus cost
    let drawAttempts = 0;
    const maxDraws = 8;

    while (focusCostsSeen.size < EXPECTED_CARDS.length && drawAttempts < maxDraws) {
      const cardElements = await page.locator('.card-grid .card').all();

      for (const cardElement of cardElements) {
        const cardNameElement = cardElement.locator('.card-name');
        const focusElement = cardElement.locator('.card-focus');

        if (await cardNameElement.count() > 0 && await focusElement.count() > 0) {
          const cardName = await cardNameElement.textContent();
          const focusText = await focusElement.textContent();

          if (cardName && focusText) {
            const cleanName = cardName.replace(/\+\d+ XP$/, '').trim().toLowerCase();
            const focusCost = parseInt(focusText);
            focusCostsSeen.set(cleanName, focusCost);
          }
        }
      }

      // Continue conversation to see more cards if needed
      if (focusCostsSeen.size < EXPECTED_CARDS.length) {
        const lowCostCard = page.locator('.card').filter({ hasText: '1' }).first();
        if (await lowCostCard.count() > 0) {
          await lowCostCard.click();
          await page.click('text=SPEAK');
          await page.waitForTimeout(1000);

          const listenButton = page.locator('text=LISTEN');
          if (await listenButton.count() > 0) {
            await listenButton.click();
            await page.waitForTimeout(1000);
          }
        }
      }

      drawAttempts++;
    }

    // Verify focus costs for each card
    for (const expectedCard of EXPECTED_CARDS) {
      const actualFocus = focusCostsSeen.get(expectedCard.name);
      expect(actualFocus,
        `Focus cost mismatch for ${expectedCard.name}: expected ${expectedCard.focus}, got ${actualFocus}`)
        .toBe(expectedCard.focus);
    }

    // Verify power tier distribution
    const tier1Count = Array.from(focusCostsSeen.values()).filter(cost => cost >= 1 && cost <= 2).length;
    const tier2Count = Array.from(focusCostsSeen.values()).filter(cost => cost >= 3 && cost <= 4).length;
    const tier3Count = Array.from(focusCostsSeen.values()).filter(cost => cost >= 5 && cost <= 6).length;
    const specialCount = Array.from(focusCostsSeen.values()).filter(cost => cost === 0).length;

    console.log(`Tier distribution - Tier 1 (1-2): ${tier1Count}, Tier 2 (3-4): ${tier2Count}, Tier 3 (5-6): ${tier3Count}, Special (0): ${specialCount}`);

    expect(tier1Count).toBe(7); // 7 cards in tier 1
    expect(tier2Count).toBe(8); // 8 cards in tier 2
    expect(tier3Count).toBe(3); // 3 cards in tier 3
    expect(specialCount).toBe(1); // 1 special card (mental_reset)
  });

  test('should display effect descriptions correctly', async ({ page }) => {
    const effectsSeen = new Map(); // card name -> effect description
    let drawAttempts = 0;
    const maxDraws = 8;

    while (effectsSeen.size < EXPECTED_CARDS.length && drawAttempts < maxDraws) {
      const cardElements = await page.locator('.card-grid .card').all();

      for (const cardElement of cardElements) {
        const cardNameElement = cardElement.locator('.card-name');
        const effectElement = cardElement.locator('.card-effect');

        if (await cardNameElement.count() > 0 && await effectElement.count() > 0) {
          const cardName = await cardNameElement.textContent();
          const effectText = await effectElement.textContent();

          if (cardName && effectText) {
            const cleanName = cardName.replace(/\+\d+ XP$/, '').trim().toLowerCase();
            effectsSeen.set(cleanName, effectText);
          }
        }
      }

      // Continue conversation to see more cards if needed
      if (effectsSeen.size < EXPECTED_CARDS.length) {
        const lowCostCard = page.locator('.card').filter({ hasText: '1' }).first();
        if (await lowCostCard.count() > 0) {
          await lowCostCard.click();
          await page.click('text=SPEAK');
          await page.waitForTimeout(1000);

          const listenButton = page.locator('text=LISTEN');
          if (await listenButton.count() > 0) {
            await listenButton.click();
            await page.waitForTimeout(1000);
          }
        }
      }

      drawAttempts++;
    }

    // Verify each card has an effect description
    for (const expectedCard of EXPECTED_CARDS) {
      const actualEffect = effectsSeen.get(expectedCard.name);
      expect(actualEffect,
        `Missing effect description for ${expectedCard.name}`).toBeTruthy();

      // Verify effect contains expected category information
      expect(actualEffect,
        `Effect should contain category info for ${expectedCard.name}`).toMatch(/^(Expression|Regulation|Realization):/);
    }
  });

  test('should display momentum conversion effects correctly', async ({ page }) => {
    const momentumEffectsSeen = new Map(); // card name -> momentum effect
    let drawAttempts = 0;
    const maxDraws = 8;

    while (momentumEffectsSeen.size < MOMENTUM_SCALING_CARDS.length && drawAttempts < maxDraws) {
      const cardElements = await page.locator('.card-grid .card').all();

      for (const cardElement of cardElements) {
        const cardNameElement = cardElement.locator('.card-name');

        if (await cardNameElement.count() > 0) {
          const cardName = await cardNameElement.textContent();

          if (cardName) {
            const cleanName = cardName.replace(/\+\d+ XP$/, '').trim().toLowerCase();

            // Check if this is a momentum scaling card
            const momentumCard = MOMENTUM_SCALING_CARDS.find(c => c.id === cleanName.replace(/\s+/g, '_'));
            if (momentumCard) {
              // Look for momentum calculation text
              const momentumElement = cardElement.locator('text*="→"').or(
                cardElement.locator('text*="momentum"')).or(
                cardElement.locator('text*="Spend"')).or(
                cardElement.locator('.card-effect'));

              if (await momentumElement.count() > 0) {
                const momentumText = await momentumElement.textContent();
                if (momentumText) {
                  momentumEffectsSeen.set(cleanName, momentumText);
                }
              }
            }
          }
        }
      }

      // Continue conversation to see more cards if needed
      if (momentumEffectsSeen.size < MOMENTUM_SCALING_CARDS.length) {
        const lowCostCard = page.locator('.card').filter({ hasText: '1' }).first();
        if (await lowCostCard.count() > 0) {
          await lowCostCard.click();
          await page.click('text=SPEAK');
          await page.waitForTimeout(1000);

          const listenButton = page.locator('text=LISTEN');
          if (await listenButton.count() > 0) {
            await listenButton.click();
            await page.waitForTimeout(1000);
          }
        }
      }

      drawAttempts++;
    }

    // Take a screenshot for debugging momentum effects
    await page.screenshot({ path: 'elena-momentum-effects.png', fullPage: true });

    // Log what we found for debugging
    console.log('Momentum effects found:');
    for (const [cardName, effect] of momentumEffectsSeen.entries()) {
      console.log(`${cardName}: ${effect}`);
    }

    // Verify key momentum conversion cards show the correct effects
    const clearConfusion = momentumEffectsSeen.get('clear confusion');
    if (clearConfusion) {
      expect(clearConfusion).toMatch(/spend.*momentum.*doubt/i);
    }

    const establishTrust = momentumEffectsSeen.get('establish trust');
    if (establishTrust) {
      expect(establishTrust).toMatch(/spend.*momentum.*flow/i);
    }

    const momentOfTruth = momentumEffectsSeen.get('moment of truth');
    if (momentOfTruth) {
      expect(momentOfTruth).toMatch(/spend.*momentum.*flow/i);
    }
  });

  test('should not display any "card not found" errors', async ({ page }) => {
    // Check browser console for errors
    const consoleErrors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    // Draw several hands to test various cards
    for (let i = 0; i < 5; i++) {
      await page.click('text=LISTEN');
      await page.waitForTimeout(1000);

      // Play a card if possible
      const playableCard = page.locator('.card').filter({ hasNotText: 'disabled' }).first();
      if (await playableCard.count() > 0) {
        await playableCard.click();
        await page.click('text=SPEAK');
        await page.waitForTimeout(1000);
      }
    }

    // Check for card-related errors
    const cardErrors = consoleErrors.filter(error =>
      error.toLowerCase().includes('card not found') ||
      error.toLowerCase().includes('card template') ||
      error.toLowerCase().includes('undefined card')
    );

    expect(cardErrors.length, `Found card-related errors: ${cardErrors.join(', ')}`).toBe(0);
  });

  test('should display dialogue fragments correctly', async ({ page }) => {
    const dialoguesSeen = new Set();
    let drawAttempts = 0;
    const maxDraws = 6;

    while (dialoguesSeen.size < 15 && drawAttempts < maxDraws) { // Aim to see most card dialogues
      const cardElements = await page.locator('.card-grid .card').all();

      for (const cardElement of cardElements) {
        const dialogueElement = cardElement.locator('.card-text');

        if (await dialogueElement.count() > 0) {
          const dialogueText = await dialogueElement.textContent();
          if (dialogueText && dialogueText.trim()) {
            const cleanDialogue = dialogueText.replace(/^"|"$/g, '').trim();
            dialoguesSeen.add(cleanDialogue);
          }
        }
      }

      // Continue conversation to see more cards
      if (dialoguesSeen.size < 15) {
        const playableCard = page.locator('.card').filter({ hasNotText: 'disabled' }).first();
        if (await playableCard.count() > 0) {
          await playableCard.click();
          await page.click('text=SPEAK');
          await page.waitForTimeout(1000);

          const listenButton = page.locator('text=LISTEN');
          if (await listenButton.count() > 0) {
            await listenButton.click();
            await page.waitForTimeout(1000);
          }
        }
      }

      drawAttempts++;
    }

    console.log(`Found ${dialoguesSeen.size} unique dialogue fragments`);
    console.log('Sample dialogues:', Array.from(dialoguesSeen).slice(0, 5));

    // Verify we have meaningful dialogue content
    expect(dialoguesSeen.size).toBeGreaterThan(10);

    // Verify dialogues are not empty or placeholder text
    for (const dialogue of dialoguesSeen) {
      expect(dialogue).not.toBe('');
      expect(dialogue).not.toMatch(/placeholder|todo|test/i);
      expect(dialogue.length).toBeGreaterThan(3);
    }
  });
});