const { test, expect } = require('@playwright/test');

test.describe('Find Elena Location', () => {
  test('should navigate through locations to find Elena', async ({ page }) => {
    console.log('Starting search for Elena...');

    // Navigate to the game
    await page.goto('/');

    // Wait for the game to load
    await page.waitForSelector('.game-container', { timeout: 10000 });

    console.log('Game loaded. Current location: Market Square');

    // Try clicking on Crossroads
    const crossroadsButton = page.locator('text=Crossroads');
    if (await crossroadsButton.isVisible()) {
      console.log('Found Crossroads button, clicking...');
      await crossroadsButton.click();
      await page.waitForTimeout(2000);

      // Check if Elena is here
      const elenaElements = await page.locator('text=Elena').count();
      if (elenaElements > 0) {
        console.log('Found Elena at Crossroads!');
        await page.screenshot({ path: 'elena-found-crossroads.png' });
        return;
      } else {
        console.log('Elena not at Crossroads, continuing search...');
        await page.screenshot({ path: 'crossroads-location.png' });
      }
    }

    // Look for other navigation options
    const navigationLinks = await page.locator('a, button').all();
    console.log('Found', navigationLinks.length, 'potential navigation elements');

    for (let i = 0; i < Math.min(navigationLinks.length, 20); i++) {
      const text = await navigationLinks[i].textContent();
      if (text && text.trim() && !text.includes('×') && !text.includes('Debug')) {
        console.log(`Navigation option ${i + 1}: "${text}"`);

        // Try clicking on promising navigation options
        if (text.toLowerCase().includes('inn') ||
            text.toLowerCase().includes('tavern') ||
            text.toLowerCase().includes('upper') ||
            text.toLowerCase().includes('residential') ||
            text.toLowerCase().includes('district')) {
          console.log(`Trying to navigate to: ${text}`);
          try {
            await navigationLinks[i].click();
            await page.waitForTimeout(2000);

            // Check for Elena
            const elenaCount = await page.locator('text=Elena').count();
            if (elenaCount > 0) {
              console.log(`Found Elena at: ${text}!`);
              await page.screenshot({ path: `elena-found-${text.replace(/[^a-zA-Z]/g, '')}.png` });
              return;
            } else {
              console.log(`Elena not found at: ${text}`);
            }
          } catch (error) {
            console.log(`Failed to navigate to: ${text}`, error.message);
          }
        }
      }
    }

    // Check if there are any investigation or action options that might lead to Elena
    const investigationButton = page.locator('text=Standard Investigation');
    if (await investigationButton.isVisible()) {
      console.log('Trying Standard Investigation...');
      await investigationButton.click();
      await page.waitForTimeout(2000);

      const elenaCount = await page.locator('text=Elena').count();
      if (elenaCount > 0) {
        console.log('Found Elena through Standard Investigation!');
        await page.screenshot({ path: 'elena-found-investigation.png' });
        return;
      }
    }

    // Look for any letter queue or conversation-related elements
    const letterElements = await page.locator('text=/letter|queue|conversation|talk/i').all();
    console.log('Found', letterElements.length, 'letter/conversation elements');

    for (let i = 0; i < letterElements.length; i++) {
      const text = await letterElements[i].textContent();
      console.log(`Letter/conversation element: "${text}"`);
    }

    console.log('Search complete. Elena location not immediately found.');
    await page.screenshot({ path: 'search-complete.png' });
  });

  test('should check letter queue for Elena', async ({ page }) => {
    console.log('Checking letter queue for Elena...');

    // Navigate to the game
    await page.goto('/');
    await page.waitForSelector('.game-container', { timeout: 10000 });

    // Look for footer navigation that might include letter queue
    const footer = page.locator('footer, .footer, .nav-footer, .bottom-nav, .game-nav');
    if (await footer.isVisible()) {
      console.log('Found footer navigation');

      // Look for queue, letters, conversations, etc.
      const queueButton = page.locator('text=/queue|letter|conversation|mail/i');
      if (await queueButton.isVisible()) {
        console.log('Found queue/letter button, clicking...');
        await queueButton.click();
        await page.waitForTimeout(2000);

        // Check for Elena
        const elenaCount = await page.locator('text=Elena').count();
        if (elenaCount > 0) {
          console.log('Found Elena in letter queue!');
          await page.screenshot({ path: 'elena-found-queue.png' });
        }
      }
    }

    // Take final screenshot
    await page.screenshot({ path: 'queue-search-complete.png' });
  });
});