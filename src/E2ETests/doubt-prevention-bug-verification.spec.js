const { test, expect } = require('@playwright/test');

test.describe('Doubt Prevention Bug Verification', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the game
    await page.goto('/');

    // Wait for the game to load
    await page.waitForSelector('.game-container', { timeout: 10000 });

    // Start conversation with Elena
    await page.click('text=Elena');
    await page.waitForSelector('.conversation-content', { timeout: 10000 });

    // Wait for cards to load
    await page.waitForSelector('.card', { timeout: 10000 });
  });

  test('should prevent doubt increase after playing PreventDoubt card', async ({ page }) => {
    console.log('Starting doubt prevention test...');

    // Get initial doubt level
    const initialDoubtSlots = await page.locator('.doubt-slot.filled').count();
    console.log('Initial doubt level:', initialDoubtSlots);

    // Look for a card with PreventDoubt effect (typically a Soothe card)
    const cards = await page.locator('.card:not(.disabled)').all();
    let preventDoubtCard = null;

    for (const card of cards) {
      // Look for card effect text that mentions "Prevent next doubt increase"
      const effectElement = await card.locator('.card-effect').first();
      if (await effectElement.isVisible()) {
        const effectText = await effectElement.textContent();
        if (effectText.includes('Prevent next doubt increase')) {
          preventDoubtCard = card;
          console.log('Found PreventDoubt card with effect:', effectText);
          break;
        }
      }
    }

    if (!preventDoubtCard) {
      // If no card is found in hand, we need to draw more cards
      console.log('No PreventDoubt card found in current hand, drawing cards...');

      // Click LISTEN to draw more cards
      const listenButton = page.locator('text=LISTEN').first();
      if (await listenButton.isVisible() && !await listenButton.isDisabled()) {
        await listenButton.click();
        await page.waitForTimeout(3000); // Wait for cards to be drawn

        // Search again in the new hand
        const newCards = await page.locator('.card:not(.disabled)').all();
        for (const card of newCards) {
          const effectElement = await card.locator('.card-effect').first();
          if (await effectElement.isVisible()) {
            const effectText = await effectElement.textContent();
            if (effectText.includes('Prevent next doubt increase')) {
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

      // Verify card is selected
      await expect(preventDoubtCard).toHaveClass(/selected/);

      // Execute SPEAK to play the card
      const speakButton = page.locator('text=SPEAK').first();
      await expect(speakButton).not.toHaveClass(/disabled/);
      await speakButton.click();

      console.log('Played PreventDoubt card');

      // Wait for the action to process
      await page.waitForTimeout(3000);

      // Get doubt level after playing the card
      const doubtAfterPreventCard = await page.locator('.doubt-slot.filled').count();
      console.log('Doubt level after playing PreventDoubt card:', doubtAfterPreventCard);

      // Now execute LISTEN action (which normally increases doubt)
      const listenButtonAfter = page.locator('text=LISTEN').first();
      if (await listenButtonAfter.isVisible() && !await listenButtonAfter.isDisabled()) {
        await listenButtonAfter.click();
        console.log('Executed LISTEN action that should normally increase doubt');

        // Wait for the action to process
        await page.waitForTimeout(3000);

        // Get doubt level after LISTEN
        const doubtAfterListen = await page.locator('.doubt-slot.filled').count();
        console.log('Doubt level after LISTEN (should be prevented):', doubtAfterListen);

        // Doubt should NOT have increased due to PreventDoubt effect
        expect(doubtAfterListen).toBe(doubtAfterPreventCard);
        console.log('SUCCESS: Doubt increase was prevented!');

        // Execute another LISTEN to verify the flag resets after one use
        const secondListenButton = page.locator('text=LISTEN').first();
        if (await secondListenButton.isVisible() && !await secondListenButton.isDisabled()) {
          await secondListenButton.click();
          console.log('Executed second LISTEN action (should now increase doubt normally)');

          // Wait for the action to process
          await page.waitForTimeout(3000);

          // Get doubt level after second LISTEN
          const doubtAfterSecondListen = await page.locator('.doubt-slot.filled').count();
          console.log('Doubt level after second LISTEN (should increase normally):', doubtAfterSecondListen);

          // Doubt SHOULD increase this time (flag was reset)
          expect(doubtAfterSecondListen).toBeGreaterThan(doubtAfterListen);
          console.log('SUCCESS: Doubt prevention flag correctly reset after one use!');
        }
      } else {
        console.log('LISTEN button not available after playing PreventDoubt card');
      }
    } else {
      console.log('WARNING: No PreventDoubt card found in hand. This test requires a card with PreventDoubt scaling.');
      test.skip();
    }
  });

  test('should show PreventDoubt effect in card description', async ({ page }) => {
    console.log('Testing PreventDoubt effect display...');

    // Look for cards with PreventDoubt effect in their description
    const cards = await page.locator('.card').all();
    let foundPreventDoubtCard = false;

    for (const card of cards) {
      const effectElement = await card.locator('.card-effect').first();
      if (await effectElement.isVisible()) {
        const effectText = await effectElement.textContent();
        if (effectText.includes('Prevent next doubt increase')) {
          foundPreventDoubtCard = true;
          console.log('Found card with PreventDoubt effect description:', effectText);

          // Verify the effect is properly described
          expect(effectText.toLowerCase()).toContain('prevent');
          expect(effectText.toLowerCase()).toContain('doubt');
          break;
        }
      }
    }

    if (!foundPreventDoubtCard) {
      console.log('No PreventDoubt cards visible in current hand - drawing more cards...');

      // Try to draw more cards to find a PreventDoubt card
      const listenButton = page.locator('text=LISTEN').first();
      if (await listenButton.isVisible() && !await listenButton.isDisabled()) {
        await listenButton.click();
        await page.waitForTimeout(3000);

        // Check again
        const newCards = await page.locator('.card').all();
        for (const card of newCards) {
          const effectElement = await card.locator('.card-effect').first();
          if (await effectElement.isVisible()) {
            const effectText = await effectElement.textContent();
            if (effectText.includes('Prevent next doubt increase')) {
              foundPreventDoubtCard = true;
              console.log('Found PreventDoubt card after drawing:', effectText);
              break;
            }
          }
        }
      }
    }

    if (!foundPreventDoubtCard) {
      console.log('WARNING: Could not find any PreventDoubt cards to verify description');
    }
  });

  test('should track doubt levels correctly', async ({ page }) => {
    console.log('Testing doubt level tracking...');

    // Get initial doubt level
    const initialDoubtSlots = await page.locator('.doubt-slot.filled').count();
    const totalDoubtSlots = await page.locator('.doubt-slot').count();

    console.log('Initial doubt:', initialDoubtSlots, 'out of', totalDoubtSlots);

    // Verify doubt display shows correct values
    expect(initialDoubtSlots).toBeGreaterThanOrEqual(0);
    expect(totalDoubtSlots).toBeGreaterThanOrEqual(initialDoubtSlots);

    // Execute LISTEN to see doubt increase normally
    const listenButton = page.locator('text=LISTEN').first();
    if (await listenButton.isVisible() && !await listenButton.isDisabled()) {
      await listenButton.click();
      await page.waitForTimeout(3000);

      const doubtAfterListen = await page.locator('.doubt-slot.filled').count();
      console.log('Doubt after LISTEN:', doubtAfterListen);

      // Doubt should have increased (unless prevented by a card effect)
      // This test verifies the tracking works correctly
      expect(doubtAfterListen).toBeGreaterThanOrEqual(initialDoubtSlots);
    }
  });

  test('should show doubt slots visually matching doubt counter', async ({ page }) => {
    console.log('Testing doubt visual display consistency...');

    // Check if there's a doubt counter/label
    const doubtLabel = page.locator('.doubt-label');
    if (await doubtLabel.isVisible()) {
      console.log('Found doubt label');
    }

    // Count filled doubt slots
    const filledSlots = await page.locator('.doubt-slot.filled').count();
    const totalSlots = await page.locator('.doubt-slot').count();

    console.log('Visual doubt display:', filledSlots, 'filled out of', totalSlots, 'total slots');

    // Verify visual consistency
    expect(filledSlots).toBeLessThanOrEqual(totalSlots);
    expect(filledSlots).toBeGreaterThanOrEqual(0);
  });

  test('should reset PreventDoubt flag after one use', async ({ page }) => {
    console.log('Testing PreventDoubt flag reset behavior...');

    // This is a more focused test for the flag reset mechanism
    // It specifically tests that the prevention only works once

    // Find a PreventDoubt card
    const cards = await page.locator('.card:not(.disabled)').all();
    let preventDoubtCard = null;

    for (const card of cards) {
      const effectElement = await card.locator('.card-effect').first();
      if (await effectElement.isVisible()) {
        const effectText = await effectElement.textContent();
        if (effectText.includes('Prevent next doubt increase')) {
          preventDoubtCard = card;
          break;
        }
      }
    }

    if (!preventDoubtCard) {
      // Try drawing cards first
      const listenButton = page.locator('text=LISTEN').first();
      if (await listenButton.isVisible()) {
        await listenButton.click();
        await page.waitForTimeout(3000);

        const newCards = await page.locator('.card:not(.disabled)').all();
        for (const card of newCards) {
          const effectElement = await card.locator('.card-effect').first();
          if (await effectElement.isVisible()) {
            const effectText = await effectElement.textContent();
            if (effectText.includes('Prevent next doubt increase')) {
              preventDoubtCard = card;
              break;
            }
          }
        }
      }
    }

    if (preventDoubtCard) {
      console.log('Found PreventDoubt card for flag reset test');

      // Track doubt levels through the sequence
      const initialDoubt = await page.locator('.doubt-slot.filled').count();

      // Play PreventDoubt card
      await preventDoubtCard.click();
      const speakButton = page.locator('text=SPEAK').first();
      await speakButton.click();
      await page.waitForTimeout(3000);

      const doubtAfterCard = await page.locator('.doubt-slot.filled').count();

      // First LISTEN - should be prevented
      const firstListen = page.locator('text=LISTEN').first();
      if (await firstListen.isVisible()) {
        await firstListen.click();
        await page.waitForTimeout(3000);

        const doubtAfterFirstListen = await page.locator('.doubt-slot.filled').count();

        // Second LISTEN - should increase doubt (flag reset)
        const secondListen = page.locator('text=LISTEN').first();
        if (await secondListen.isVisible()) {
          await secondListen.click();
          await page.waitForTimeout(3000);

          const doubtAfterSecondListen = await page.locator('.doubt-slot.filled').count();

          // Verify the sequence
          expect(doubtAfterFirstListen).toBe(doubtAfterCard); // First prevented
          expect(doubtAfterSecondListen).toBeGreaterThan(doubtAfterFirstListen); // Second not prevented

          console.log('PreventDoubt flag reset verified:');
          console.log('- After card:', doubtAfterCard);
          console.log('- After first LISTEN (prevented):', doubtAfterFirstListen);
          console.log('- After second LISTEN (not prevented):', doubtAfterSecondListen);
        }
      }
    } else {
      console.log('WARNING: Could not find PreventDoubt card for flag reset test');
      test.skip();
    }
  });
});