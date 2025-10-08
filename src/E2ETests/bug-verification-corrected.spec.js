const { test, expect } = require('@playwright/test');

test.describe('Bug Verification - Focus Constraints and Doubt Prevention', () => {
  test.beforeEach(async ({ page }) => {
    console.log('Setting up test...');

    // Navigate to the game
    await page.goto('/');

    // Wait for the game to load
    await page.waitForSelector('.game-container', { timeout: 10000 });

    // Click on the obligations panel to access the letter queue where Elena's letter should be
    const obligationsPanel = page.locator('.obligations-panel');
    if (await obligationsPanel.isVisible()) {
      console.log('Found obligations panel, clicking to access letter queue...');
      await obligationsPanel.click();
      await page.waitForTimeout(2000);

      // Look for Elena's letter in the queue
      const elenaElements = await page.locator('text=Elena').count();
      if (elenaElements > 0) {
        console.log('Found Elena in letter queue!');
        // Click on Elena's letter to start conversation
        await page.click('text=Elena');
        await page.waitForSelector('.conversation-content', { timeout: 10000 });
        console.log('Successfully started conversation with Elena');
      } else {
        console.log('Elena not found in letter queue, looking for alternative access...');

        // If Elena's not in the queue, try looking for any conversation-starting elements
        const conversationElements = await page.locator('button, .btn, .card').all();
        for (const element of conversationElements) {
          const text = await element.textContent();
          if (text && text.toLowerCase().includes('elena')) {
            console.log('Found Elena reference, clicking...');
            await element.click();
            await page.waitForTimeout(2000);
            break;
          }
        }
      }
    } else {
      console.log('Obligations panel not found, trying alternative navigation...');

      // Alternative: look for any mention of Elena directly on the page
      const directElenaElements = await page.locator('text=Elena').count();
      if (directElenaElements > 0) {
        await page.click('text=Elena');
        await page.waitForTimeout(2000);
      }
    }

    // Wait for cards to load if we're in conversation
    await page.waitForSelector('.card, .conversation-content', { timeout: 5000 });
  });

  test('should prevent focus constraint bug - cards disabled when focus insufficient', async ({ page }) => {
    console.log('Testing focus constraint bug fix...');

    // Check if we're in a conversation
    const conversationVisible = await page.locator('.conversation-content').isVisible();
    if (!conversationVisible) {
      console.log('Not in conversation mode, skipping focus constraint test');
      test.skip();
    }

    // Get initial focus display
    const focusDisplay = await page.locator('.focus-value, .focus-display').first();
    if (await focusDisplay.isVisible()) {
      const focusText = await focusDisplay.textContent();
      console.log('Focus display:', focusText);

      // Extract available focus
      const focusMatch = focusText.match(/(\d+)(?:\/\d+)?/);
      const availableFocus = focusMatch ? parseInt(focusMatch[1]) : 3;
      console.log('Available focus:', availableFocus);

      // Find cards and their focus costs
      const cards = await page.locator('.card:not(.disabled)').all();
      console.log('Found', cards.length, 'available cards');

      let totalSelectedFocus = 0;
      let cardsSelected = 0;

      // Try to select cards one by one
      for (const card of cards) {
        const focusCostElement = await card.locator('.card-focus').first();
        if (await focusCostElement.isVisible()) {
          const focusCost = parseInt(await focusCostElement.textContent());
          console.log('Card focus cost:', focusCost);

          if (totalSelectedFocus + focusCost <= availableFocus) {
            // Should be able to select this card
            await card.click();
            await page.waitForTimeout(500);

            // Verify card is selected
            const isSelected = await card.evaluate(el => el.classList.contains('selected'));
            if (isSelected) {
              totalSelectedFocus += focusCost;
              cardsSelected++;
              console.log('Successfully selected card. Total focus used:', totalSelectedFocus);
            }
          } else {
            // This card should be unselectable due to focus constraint
            console.log('Attempting to select card that exceeds focus limit...');
            await card.click();
            await page.waitForTimeout(500);

            // Card should NOT be selected
            const isSelected = await card.evaluate(el => el.classList.contains('selected'));
            expect(isSelected).toBe(false);
            console.log('✅ PASS: Focus constraint properly prevented card selection');
            break;
          }
        }
      }

      // Test SPEAK button behavior
      if (cardsSelected > 0) {
        const speakButton = page.locator('text=SPEAK').first();
        if (await speakButton.isVisible()) {
          const isDisabled = await speakButton.evaluate(el =>
            el.disabled || el.classList.contains('disabled')
          );

          if (totalSelectedFocus <= availableFocus) {
            expect(isDisabled).toBe(false);
            console.log('✅ PASS: SPEAK button enabled with valid focus usage');
          } else {
            expect(isDisabled).toBe(true);
            console.log('✅ PASS: SPEAK button disabled with excessive focus usage');
          }
        }
      }
    } else {
      console.log('Focus display not found, may not be in conversation');
    }
  });

  test('should verify doubt prevention bug fix - PreventDoubt effect works', async ({ page }) => {
    console.log('Testing doubt prevention bug fix...');

    // Check if we're in a conversation
    const conversationVisible = await page.locator('.conversation-content').isVisible();
    if (!conversationVisible) {
      console.log('Not in conversation mode, skipping doubt prevention test');
      test.skip();
    }

    // Get initial doubt level
    const initialDoubtSlots = await page.locator('.doubt-slot.filled').count();
    console.log('Initial doubt level:', initialDoubtSlots);

    // Look for a card with PreventDoubt effect
    const cards = await page.locator('.card:not(.disabled)').all();
    let preventDoubtCard;

    for (const card of cards) {
      const effectElement = await card.locator('.card-effect').first();
      if (await effectElement.isVisible()) {
        const effectText = await effectElement.textContent();
        if (effectText.toLowerCase().includes('prevent') && effectText.toLowerCase().includes('doubt')) {
          preventDoubtCard = card;
          console.log('Found PreventDoubt card with effect:', effectText);
          break;
        }
      }
    }

    if (!preventDoubtCard) {
      // Try drawing more cards to find a PreventDoubt card
      console.log('No PreventDoubt card found, trying to draw more cards...');
      const listenButton = page.locator('text=LISTEN').first();
      if (await listenButton.isVisible() && !await listenButton.evaluate(el => el.disabled || el.classList.contains('disabled'))) {
        await listenButton.click();
        await page.waitForTimeout(3000);

        // Search again
        const newCards = await page.locator('.card:not(.disabled)').all();
        for (const card of newCards) {
          const effectElement = await card.locator('.card-effect').first();
          if (await effectElement.isVisible()) {
            const effectText = await effectElement.textContent();
            if (effectText.toLowerCase().includes('prevent') && effectText.toLowerCase().includes('doubt')) {
              preventDoubtCard = card;
              console.log('Found PreventDoubt card after drawing:', effectText);
              break;
            }
          }
        }
      }
    }

    if (preventDoubtCard) {
      // Play the PreventDoubt card
      await preventDoubtCard.click();
      console.log('Selected PreventDoubt card');

      const speakButton = page.locator('text=SPEAK').first();
      if (await speakButton.isVisible() && !await speakButton.evaluate(el => el.disabled || el.classList.contains('disabled'))) {
        await speakButton.click();
        console.log('Played PreventDoubt card');
        await page.waitForTimeout(3000);

        const doubtAfterCard = await page.locator('.doubt-slot.filled').count();
        console.log('Doubt after playing PreventDoubt card:', doubtAfterCard);

        // Now test LISTEN action (which normally increases doubt)
        const listenButtonAfter = page.locator('text=LISTEN').first();
        if (await listenButtonAfter.isVisible() && !await listenButtonAfter.evaluate(el => el.disabled || el.classList.contains('disabled'))) {
          await listenButtonAfter.click();
          console.log('Executed LISTEN (should be prevented)');
          await page.waitForTimeout(3000);

          const doubtAfterListen = await page.locator('.doubt-slot.filled').count();
          console.log('Doubt after LISTEN (should be same):', doubtAfterListen);

          // Doubt should NOT have increased
          expect(doubtAfterListen).toBe(doubtAfterCard);
          console.log('✅ PASS: Doubt increase was prevented!');

          // Test that prevention flag resets after one use
          const secondListenButton = page.locator('text=LISTEN').first();
          if (await secondListenButton.isVisible() && !await secondListenButton.evaluate(el => el.disabled || el.classList.contains('disabled'))) {
            await secondListenButton.click();
            console.log('Executed second LISTEN (should now increase doubt)');
            await page.waitForTimeout(3000);

            const doubtAfterSecondListen = await page.locator('.doubt-slot.filled').count();
            console.log('Doubt after second LISTEN:', doubtAfterSecondListen);

            // Doubt SHOULD increase this time
            expect(doubtAfterSecondListen).toBeGreaterThan(doubtAfterListen);
            console.log('✅ PASS: Doubt prevention flag correctly reset after one use!');
          }
        }
      }
    } else {
      console.log('❌ WARNING: Could not find PreventDoubt card to test');
    }
  });

  test('should display card effects and costs correctly', async ({ page }) => {
    console.log('Testing card effect display...');

    // Check if we're in a conversation
    const conversationVisible = await page.locator('.conversation-content').isVisible();
    if (!conversationVisible) {
      console.log('Not in conversation mode, skipping card display test');
      test.skip();
    }

    const cards = await page.locator('.card').all();
    console.log('Found', cards.length, 'cards to verify');

    let cardsWithFocus = 0;
    let cardsWithEffects = 0;

    for (const card of cards) {
      // Check focus cost display
      const focusCostElement = await card.locator('.card-focus').first();
      if (await focusCostElement.isVisible()) {
        const focusCost = await focusCostElement.textContent();
        expect(focusCost).toMatch(/^\d+$/);
        cardsWithFocus++;
      }

      // Check effect description
      const effectElement = await card.locator('.card-effect').first();
      if (await effectElement.isVisible()) {
        const effectText = await effectElement.textContent();
        expect(effectText.trim()).not.toBe('');
        cardsWithEffects++;

        // Log specific effects found
        if (effectText.toLowerCase().includes('prevent') && effectText.toLowerCase().includes('doubt')) {
          console.log('✅ Found PreventDoubt effect display:', effectText.substring(0, 80));
        }
      }
    }

    expect(cardsWithFocus).toBeGreaterThan(0);
    expect(cardsWithEffects).toBeGreaterThan(0);
    console.log('✅ PASS: Card effects and costs displayed correctly');
    console.log(`- Cards with focus costs: ${cardsWithFocus}`);
    console.log(`- Cards with effect descriptions: ${cardsWithEffects}`);
  });
});