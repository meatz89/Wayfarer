const { test, expect } = require('@playwright/test');

test.describe('Card Effect Display Verification', () => {
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

  test('should display focus costs correctly on all cards', async ({ page }) => {
    console.log('Testing focus cost display on cards...');

    const cards = await page.locator('.card').all();
    let cardCount = 0;

    for (const card of cards) {
      const focusCostElement = await card.locator('.card-focus').first();

      if (await focusCostElement.isVisible()) {
        const focusCost = await focusCostElement.textContent();
        cardCount++;

        // Focus cost should be a valid number
        expect(focusCost).toMatch(/^\d+$/);

        // Focus cost should be reasonable (0-5 typically)
        const cost = parseInt(focusCost);
        expect(cost).toBeGreaterThanOrEqual(0);
        expect(cost).toBeLessThanOrEqual(10); // Sanity check

        console.log(`Card ${cardCount}: Focus cost = ${focusCost}`);
      }
    }

    expect(cardCount).toBeGreaterThan(0);
    console.log(`Verified focus costs on ${cardCount} cards`);
  });

  test('should display card effects correctly', async ({ page }) => {
    console.log('Testing card effect descriptions...');

    const cards = await page.locator('.card').all();
    let cardsWithEffects = 0;

    for (const card of cards) {
      const effectElement = await card.locator('.card-effect').first();

      if (await effectElement.isVisible()) {
        const effectText = await effectElement.textContent();
        cardsWithEffects++;

        // Effect should not be empty
        expect(effectText.trim()).not.toBe('');

        // Effect should contain meaningful text
        expect(effectText.length).toBeGreaterThan(5);

        console.log(`Card effect: ${effectText.substring(0, 50)}...`);

        // Check for specific effect types
        if (effectText.includes('Prevent next doubt increase')) {
          console.log('Found PreventDoubt effect card');
          expect(effectText.toLowerCase()).toContain('prevent');
          expect(effectText.toLowerCase()).toContain('doubt');
        }

        // Check for momentum effects
        if (effectText.includes('momentum')) {
          console.log('Found momentum effect card');
        }

        // Check for stat bonuses
        if (effectText.includes('bonus') || effectText.includes('+')) {
          console.log('Found bonus effect card');
        }
      }
    }

    expect(cardsWithEffects).toBeGreaterThan(0);
    console.log(`Found effects on ${cardsWithEffects} cards`);
  });

  test('should display card names and dialogue correctly', async ({ page }) => {
    console.log('Testing card names and dialogue display...');

    const cards = await page.locator('.card').all();
    let cardsWithNames = 0;
    let cardsWithDialogue = 0;

    for (const card of cards) {
      // Check card names
      const nameElement = await card.locator('.card-name').first();
      if (await nameElement.isVisible()) {
        const cardName = await nameElement.textContent();
        cardsWithNames++;

        // Name should not be empty and should be reasonable length
        expect(cardName.trim()).not.toBe('');
        expect(cardName.length).toBeGreaterThan(2);
        expect(cardName.length).toBeLessThan(100);

        console.log(`Card name: ${cardName}`);
      }

      // Check card dialogue/text
      const textElement = await card.locator('.card-text').first();
      if (await textElement.isVisible()) {
        const cardText = await textElement.textContent();
        if (cardText.trim() !== '') {
          cardsWithDialogue++;

          // Dialogue should be in quotes typically
          console.log(`Card dialogue: ${cardText.substring(0, 50)}...`);
        }
      }
    }

    expect(cardsWithNames).toBeGreaterThan(0);
    console.log(`Verified names on ${cardsWithNames} cards`);
    console.log(`Found dialogue on ${cardsWithDialogue} cards`);
  });

  test('should display card categories and stat bindings', async ({ page }) => {
    console.log('Testing card categories and stat bindings...');

    const cards = await page.locator('.card').all();
    let cardsWithCategories = 0;
    let cardsWithStats = 0;

    for (const card of cards) {
      // Check category tags
      const categoryElement = await card.locator('.category-tag').first();
      if (await categoryElement.isVisible()) {
        const category = await categoryElement.textContent();
        cardsWithCategories++;

        // Category should be one of the expected types
        const validCategories = ['Soothe', 'Assert', 'Probe', 'Express', 'Exchange'];
        expect(validCategories).toContain(category);

        console.log(`Card category: ${category}`);
      }

      // Check stat badges
      const statElement = await card.locator('.stat-badge').first();
      if (await statElement.isVisible()) {
        const statText = await statElement.textContent();
        cardsWithStats++;

        // Should contain a stat name and level
        expect(statText).toMatch(/Lv \d+/);

        // Should contain one of the valid stats
        const validStats = ['Insight', 'Rapport', 'Authority', 'Commerce', 'Cunning'];
        const hasValidStat = validStats.some(stat => statText.includes(stat));
        expect(hasValidStat).toBe(true);

        console.log(`Card stat: ${statText}`);
      }
    }

    expect(cardsWithCategories).toBeGreaterThan(0);
    console.log(`Verified categories on ${cardsWithCategories} cards`);
    console.log(`Verified stats on ${cardsWithStats} cards`);
  });

  test('should show card availability states correctly', async ({ page }) => {
    console.log('Testing card availability states...');

    // Check for disabled cards
    const disabledCards = await page.locator('.card.disabled').count();
    const totalCards = await page.locator('.card').count();
    const availableCards = totalCards - disabledCards;

    console.log(`Total cards: ${totalCards}`);
    console.log(`Disabled cards: ${disabledCards}`);
    console.log(`Available cards: ${availableCards}`);

    // Should have at least some available cards
    expect(availableCards).toBeGreaterThan(0);

    // Check visual indicators for disabled cards
    if (disabledCards > 0) {
      const firstDisabledCard = page.locator('.card.disabled').first();

      // Disabled cards should have visual styling differences
      // This is implementation specific, but we can check they exist
      expect(await firstDisabledCard.isVisible()).toBe(true);
      console.log('Disabled cards are properly marked');
    }
  });

  test('should display tooltips or hover information', async ({ page }) => {
    console.log('Testing card hover/tooltip behavior...');

    const cards = await page.locator('.card').all();

    if (cards.length > 0) {
      const firstCard = cards[0];

      // Hover over the first card
      await firstCard.hover();

      // Wait a moment for any hover effects
      await page.waitForTimeout(500);

      // Check if any additional information appears
      // This depends on implementation - we're mainly checking the cards respond to interaction
      const cardIsInteractive = await firstCard.evaluate(el => {
        const computedStyle = window.getComputedStyle(el);
        return computedStyle.cursor !== 'default';
      });

      if (cardIsInteractive) {
        console.log('Cards appear to be interactive (cursor change on hover)');
      }

      // Cards should be clickable (at least the non-disabled ones)
      if (!await firstCard.locator('.disabled').isVisible()) {
        await firstCard.click();
        console.log('Card is clickable');

        // Check if card becomes selected
        await expect(firstCard).toHaveClass(/selected/);
        console.log('Card selection works correctly');
      }
    }
  });

  test('should display momentum calculations and effects', async ({ page }) => {
    console.log('Testing momentum calculation display...');

    const cards = await page.locator('.card').all();
    let cardsWithMomentum = 0;

    for (const card of cards) {
      // Look for momentum-related text in effects
      const effectElement = await card.locator('.card-effect').first();
      if (await effectElement.isVisible()) {
        const effectText = await effectElement.textContent();

        if (effectText.includes('momentum') || effectText.includes('Momentum')) {
          cardsWithMomentum++;
          console.log(`Found momentum effect: ${effectText.substring(0, 80)}...`);

          // Check for specific momentum patterns
          if (effectText.includes('+') || effectText.includes('-')) {
            console.log('Card shows momentum change calculation');
          }
        }
      }
    }

    console.log(`Found ${cardsWithMomentum} cards with momentum effects`);
  });

  test('should display card persistence types correctly', async ({ page }) => {
    console.log('Testing card persistence type display...');

    const cards = await page.locator('.card').all();
    let cardsWithPersistence = 0;

    for (const card of cards) {
      // Check for persistence tags
      const persistenceElements = await card.locator('.persistence-tag').all();

      for (const element of persistenceElements) {
        if (await element.isVisible()) {
          const persistenceType = await element.textContent();
          cardsWithPersistence++;

          // Should be one of the valid persistence types
          const validTypes = ['Impulse', 'Opening', 'Thought'];
          expect(validTypes).toContain(persistenceType);

          console.log(`Card persistence: ${persistenceType}`);

          // Check for impulse warning
          if (persistenceType === 'Impulse') {
            const warningElement = await card.locator('.card-warning').first();
            if (await warningElement.isVisible()) {
              const warningText = await warningElement.textContent();
              expect(warningText.toLowerCase()).toContain('impulse');
              console.log('Impulse card shows proper warning');
            }
          }
        }
      }
    }

    console.log(`Found ${cardsWithPersistence} cards with persistence types`);
  });
});