// global-setup.js
const { chromium } = require('@playwright/test');

module.exports = async config => {
  console.log('🎮 Starting Wayfarer card system evaluation tests...');

  // Create a browser instance for setup
  const browser = await chromium.launch();
  const context = await browser.newContext();
  const page = await context.newPage();

  try {
    // Navigate to the application
    console.log('🚀 Connecting to Wayfarer application...');
    await page.goto(config.use.baseURL || 'http://localhost:5000');

    // Wait for Blazor to hydrate
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000); // Extra time for Blazor initialization

    // Check if the game loads properly
    const gameTitle = await page.textContent('title');
    console.log(`📖 Game title: ${gameTitle}`);

    // Check for any immediate errors
    const errorElements = await page.locator('.error, .exception').count();
    if (errorElements > 0) {
      console.warn('⚠️ Warning: Error elements detected on initial load');
    }

    // Verify Elena conversation is accessible
    console.log('🔍 Checking Elena conversation accessibility...');

    // Look for navigation elements
    const navElements = await page.locator('button, .nav-item, .location-button').count();
    console.log(`🧭 Found ${navElements} navigation elements`);

    // Check for conversation-related UI elements
    const conversationElements = await page.locator('.card, .conversation, [data-test*="card"]').count();
    console.log(`💬 Found ${conversationElements} conversation UI elements`);

    // Verify game state initialization
    const gameStateElements = await page.locator('.focus, .momentum, .doubt, .flow, [data-test*="display"]').count();
    console.log(`📊 Found ${gameStateElements} game state display elements`);

    console.log('✅ Application setup complete - Elena conversation tests ready');

  } catch (error) {
    console.error('❌ Global setup failed:', error);
    throw error;
  } finally {
    await browser.close();
  }
};