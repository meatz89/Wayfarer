const { test, expect } = require('@playwright/test');

test.describe('Focus Constraint Bug Verification', () => {
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

  test('should prevent selecting cards that exceed available focus', async ({ page }) => {
    console.log('Starting focus constraint test...');

    // Get initial focus display
    const focusDisplay = await page.locator('.focus-value').textContent();
    console.log('Initial focus display:', focusDisplay);

    // Extract available focus from display (format: "3/3")
    const availableFocus = parseInt(focusDisplay.split('/')[0]);
    console.log('Available focus:', availableFocus);

    // Find all cards and their focus costs
    const cards = await page.locator('.card:not(.disabled)').all();
    console.log('Found', cards.length, 'available cards');

    let totalSelectedFocus = 0;
    let selectedCards = [];

    // Try to select cards one by one until we exceed focus limit
    for (const card of cards) {
      // Get card focus cost
      const focusCostElement = await card.locator('.card-focus').first();
      if (!await focusCostElement.isVisible()) continue;

      const focusCostText = await focusCostElement.textContent();
      const focusCost = parseInt(focusCostText);

      console.log('Card focus cost:', focusCost);

      if (totalSelectedFocus + focusCost <= availableFocus) {
        // Should be able to select this card
        await card.click();
        console.log('Selected card with focus cost:', focusCost);
        totalSelectedFocus += focusCost;
        selectedCards.push({ card, focusCost });

        // Verify card appears selected
        await expect(card).toHaveClass(/selected/);
      } else {
        // This card should exceed our focus limit
        console.log('Attempting to select card that would exceed focus limit...');

        // Try to click the card
        await card.click();

        // Card should NOT become selected
        await expect(card).not.toHaveClass(/selected/);
        console.log('Correctly prevented card selection that would exceed focus');
        break;
      }
    }

    // Verify SPEAK button behavior with exact focus usage
    if (totalSelectedFocus === availableFocus) {
      // If we used all available focus, SPEAK should be enabled
      const speakButton = page.locator('text=SPEAK').first();
      await expect(speakButton).not.toHaveClass(/disabled/);
      console.log('SPEAK button correctly enabled when using all available focus');
    } else if (totalSelectedFocus > 0) {
      // If we have some cards selected but haven't maxed out, SPEAK should be enabled
      const speakButton = page.locator('text=SPEAK').first();
      await expect(speakButton).not.toHaveClass(/disabled/);
      console.log('SPEAK button correctly enabled with valid focus usage');
    }
  });

  test('should disable SPEAK button when no cards are selected', async ({ page }) => {
    console.log('Testing SPEAK button without card selection...');

    // Initially, no cards should be selected and SPEAK should be disabled
    const speakButton = page.locator('text=SPEAK').first();

    // Check if SPEAK button is disabled when no cards are selected
    await expect(speakButton).toHaveClass(/disabled/);
    console.log('SPEAK button correctly disabled with no card selection');
  });

  test('should enforce focus constraint in backend', async ({ page }) => {
    console.log('Testing backend focus constraint enforcement...');

    // Get available focus
    const focusDisplay = await page.locator('.focus-value').textContent();
    const availableFocus = parseInt(focusDisplay.split('/')[0]);

    // Select a card that uses some focus
    const cards = await page.locator('.card:not(.disabled)').all();
    let foundValidCard = false;

    for (const card of cards) {
      const focusCostElement = await card.locator('.card-focus').first();
      if (!await focusCostElement.isVisible()) continue;

      const focusCost = parseInt(await focusCostElement.textContent());

      if (focusCost > 0 && focusCost <= availableFocus) {
        await card.click();
        foundValidCard = true;
        console.log('Selected card with focus cost:', focusCost);
        break;
      }
    }

    if (foundValidCard) {
      // Execute SPEAK action
      const speakButton = page.locator('text=SPEAK').first();
      await speakButton.click();

      // Wait for action to process
      await page.waitForTimeout(2000);

      // Verify focus was actually consumed
      const newFocusDisplay = await page.locator('.focus-value').textContent();
      const newAvailableFocus = parseInt(newFocusDisplay.split('/')[0]);

      expect(newAvailableFocus).toBeLessThan(availableFocus);
      console.log('Focus correctly consumed by backend:', availableFocus, '->', newAvailableFocus);
    }
  });

  test('should show correct focus costs on cards', async ({ page }) => {
    console.log('Verifying card focus cost display...');

    // Check that all cards display their focus costs correctly
    const cards = await page.locator('.card').all();

    for (const card of cards) {
      const focusCostElement = await card.locator('.card-focus').first();
      if (await focusCostElement.isVisible()) {
        const focusCost = await focusCostElement.textContent();

        // Focus cost should be a number >= 0
        expect(parseInt(focusCost)).toBeGreaterThanOrEqual(0);

        // Verify focus cost is displayed as a number
        expect(focusCost).toMatch(/^\d+$/);
        console.log('Card shows focus cost:', focusCost);
      }
    }
  });

  test('should update focus display correctly', async ({ page }) => {
    console.log('Testing focus display updates...');

    // Check initial focus display format
    const focusDisplay = await page.locator('.focus-value').textContent();
    expect(focusDisplay).toMatch(/^\d+\/\d+$/);
    console.log('Focus display format correct:', focusDisplay);

    // Check focus dots match the numeric display
    const availableFocus = parseInt(focusDisplay.split('/')[0]);
    const maxFocus = parseInt(focusDisplay.split('/')[1]);

    const availableDots = await page.locator('.focus-dot.available').count();
    const totalDots = await page.locator('.focus-dot').count();

    expect(availableDots).toBe(availableFocus);
    expect(totalDots).toBe(maxFocus);
    console.log('Focus dots match numeric display:', availableDots, 'available out of', totalDots);
  });
});