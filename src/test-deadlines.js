const { chromium } = require('playwright');

(async () => {
  console.log('Testing Letter Deadline Decrement...');
  
  const browser = await chromium.launch({ headless: false });
  const page = await browser.newPage();
  
  try {
    // Navigate to the game
    console.log('1. Navigating to game...');
    await page.goto('http://localhost:5099');
    await page.waitForTimeout(2000);
    
    // Start new game
    console.log('2. Starting new game...');
    const newGameButton = page.locator('button:has-text("New Game")');
    if (await newGameButton.isVisible()) {
      await newGameButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Check initial letter queue
    console.log('3. Checking initial letter queue...');
    const letterSlots = await page.locator('.letter-slot').all();
    console.log(`   Found ${letterSlots.length} letter slots`);
    
    // Get initial deadline from first letter
    const firstLetter = letterSlots[0];
    if (firstLetter) {
      const deadlineText = await firstLetter.locator('.deadline').textContent();
      console.log(`   Initial deadline: ${deadlineText}`);
    }
    
    // Click wait button to advance time
    console.log('4. Clicking Wait to advance time...');
    const waitButton = page.locator('button:has-text("Wait")');
    if (await waitButton.isVisible()) {
      await waitButton.click();
      await page.waitForTimeout(2000);
      
      // Check deadline after waiting
      console.log('5. Checking deadline after wait...');
      const letterSlotsAfter = await page.locator('.letter-slot').all();
      const firstLetterAfter = letterSlotsAfter[0];
      if (firstLetterAfter) {
        const deadlineTextAfter = await firstLetterAfter.locator('.deadline').textContent();
        console.log(`   Deadline after wait: ${deadlineTextAfter}`);
      }
    } else {
      console.log('   Wait button not found - trying action menu...');
      
      // Try to find wait in action menu
      const actionButton = page.locator('button:has-text("Actions")');
      if (await actionButton.isVisible()) {
        await actionButton.click();
        await page.waitForTimeout(500);
        
        const waitAction = page.locator('text=/Wait/i');
        if (await waitAction.isVisible()) {
          await waitAction.click();
          await page.waitForTimeout(2000);
          
          // Check deadline after waiting
          console.log('5. Checking deadline after wait...');
          const letterSlotsAfter = await page.locator('.letter-slot').all();
          const firstLetterAfter = letterSlotsAfter[0];
          if (firstLetterAfter) {
            const deadlineTextAfter = await firstLetterAfter.locator('.deadline').textContent();
            console.log(`   Deadline after wait: ${deadlineTextAfter}`);
          }
        }
      }
    }
    
    console.log('✅ Test complete! Check if deadlines decreased.');
    
  } catch (error) {
    console.error('❌ Test failed:', error);
  } finally {
    await page.waitForTimeout(5000); // Keep browser open to see results
    await browser.close();
  }
})();