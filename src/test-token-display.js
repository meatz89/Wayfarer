// Test script for narrative token display
// Run with: node test-token-display.js

const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: false });
  const context = await browser.newContext();
  const page = await context.newPage();

  try {
    console.log('Testing narrative token display...');
    
    // Navigate to the game
    await page.goto('http://localhost:5099/');
    await page.waitForLoadState('networkidle');
    
    // Start a conversation to see token display
    console.log('Starting conversation with Elena...');
    
    // Look for Elena or another NPC to start conversation
    const npcButton = await page.locator('button:has-text("Elena")').first();
    if (await npcButton.isVisible()) {
      await npcButton.click();
      await page.waitForTimeout(2000);
      
      // Check for token display
      const tokenDisplay = await page.locator('.token-display');
      if (await tokenDisplay.isVisible()) {
        console.log('Token display found!');
        
        // Get the relationship insights
        const insights = await page.locator('.relationship-insight').all();
        for (const insight of insights) {
          const text = await insight.textContent();
          console.log(`Relationship insight: ${text}`);
        }
        
        // Check if we're showing narrative descriptions
        const hasNarrativeText = await page.locator('.relationship-insight').first().evaluate(el => {
          const text = el.textContent;
          // Check for narrative language instead of numbers
          return text.includes('trust') || text.includes('business') || 
                 text.includes('reputation') || text.includes('secret');
        });
        
        if (hasNarrativeText) {
          console.log('✓ Narrative descriptions are working!');
        } else {
          console.log('✗ Still showing mechanical display');
        }
        
        // Check if we're filtering tokens contextually
        const tokenCount = await page.locator('.relationship-insight').count();
        console.log(`Showing ${tokenCount} relevant token types (should be 1-2, not all 4)`);
        
        // Take a screenshot
        await page.screenshot({ 
          path: 'token-display-test.png',
          clip: await tokenDisplay.boundingBox()
        });
        console.log('Screenshot saved as token-display-test.png');
      } else {
        console.log('Token display not visible in conversation');
      }
    } else {
      console.log('Could not find NPC to start conversation');
    }
    
  } catch (error) {
    console.error('Test failed:', error);
  } finally {
    await browser.close();
  }
})();