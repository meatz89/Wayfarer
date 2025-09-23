const { test, expect } = require('@playwright/test');

test.describe('Game Exploration', () => {
  test('should explore the game to find Elena', async ({ page }) => {
    console.log('Starting game exploration...');

    // Navigate to the game
    await page.goto('/');

    // Wait for the game to load
    await page.waitForSelector('.game-container', { timeout: 10000 });

    // Take a screenshot of the initial screen
    await page.screenshot({ path: 'initial-screen.png' });

    console.log('Game loaded, taking inventory of UI elements...');

    // Look for navigation elements
    const navigationElements = await page.locator('[data-testid]').all();
    console.log('Found', navigationElements.length, 'elements with data-testid');

    // Look for buttons and clickable elements
    const buttons = await page.locator('button, .btn, .action-button, .nav-button').all();
    console.log('Found', buttons.length, 'button-like elements');

    for (let i = 0; i < Math.min(buttons.length, 10); i++) {
      const buttonText = await buttons[i].textContent();
      console.log(`Button ${i + 1}: "${buttonText}"`);
    }

    // Look for links
    const links = await page.locator('a').all();
    console.log('Found', links.length, 'link elements');

    for (let i = 0; i < Math.min(links.length, 10); i++) {
      const linkText = await links[i].textContent();
      console.log(`Link ${i + 1}: "${linkText}"`);
    }

    // Look for any text containing "Elena"
    const elenaElements = await page.locator('text=Elena').all();
    console.log('Found', elenaElements.length, 'elements containing "Elena"');

    // Look for NPCs or character names
    const npcElements = await page.locator('.npc, .character, .person').all();
    console.log('Found', npcElements.length, 'NPC-like elements');

    // Look for location/movement options
    const locationElements = await page.locator('.location, .area, .region').all();
    console.log('Found', locationElements.length, 'location-like elements');

    // Check if there are tabs or other sections
    const tabs = await page.locator('.tab, .nav-tab, .section-tab').all();
    console.log('Found', tabs.length, 'tab-like elements');

    // Look for any elements mentioning conversation, talk, or interact
    const conversationElements = await page.locator('text=/talk|speak|conversation|interact/i').all();
    console.log('Found', conversationElements.length, 'conversation-related elements');

    for (let i = 0; i < Math.min(conversationElements.length, 5); i++) {
      const text = await conversationElements[i].textContent();
      console.log(`Conversation element ${i + 1}: "${text}"`);
    }

    // Try to find the footer or navigation menu
    const footer = page.locator('footer, .footer, .nav-footer, .game-nav');
    if (await footer.isVisible()) {
      console.log('Found footer/navigation');
      const footerButtons = await footer.locator('button, a, .nav-item').all();
      for (let i = 0; i < footerButtons.length; i++) {
        const text = await footerButtons[i].textContent();
        console.log(`Footer button ${i + 1}: "${text}"`);
      }
    }

    // Check current page content for clues
    const pageContent = await page.textContent('body');
    if (pageContent.includes('Elena')) {
      console.log('Elena is mentioned in the page content!');
    } else {
      console.log('Elena is not mentioned in the current page content');
    }

    // Try clicking on different areas to navigate
    console.log('Looking for navigation options...');

    // Take a final screenshot
    await page.screenshot({ path: 'exploration-complete.png' });

    console.log('Game exploration complete');
  });
});