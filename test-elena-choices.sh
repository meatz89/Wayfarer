#!/bin/bash

# Test Elena's conversation choices with the new additive system
echo "Testing Elena's conversation choices..."

# Start the server
echo "Starting server on port 5099..."
ASPNETCORE_URLS="http://localhost:5099" timeout 30 dotnet run &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 10

# Run Playwright test
echo "Running Playwright test..."
cat << 'EOF' | node
const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  try {
    console.log('Navigating to game...');
    await page.goto('http://localhost:5099');
    await page.waitForTimeout(2000);
    
    // Click through morning activities
    console.log('Skipping morning activities...');
    const skipButton = await page.$('text=Skip Morning Activities');
    if (skipButton) {
      await skipButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Navigate to Elena
    console.log('Navigating to Elena at Market Square...');
    
    // Find and click Elena's card
    const elenaCard = await page.$('text=Elena');
    if (elenaCard) {
      await elenaCard.click();
      await page.waitForTimeout(1000);
      
      // Click Start Conversation
      const startButton = await page.$('text=Start Conversation');
      if (startButton) {
        await startButton.click();
        await page.waitForTimeout(2000);
        
        // Count conversation choices
        console.log('Analyzing conversation choices...');
        const choices = await page.$$('.choice-option, .choice-button, .unified-choice');
        console.log(`Found ${choices.length} conversation choices`);
        
        // Get text of each choice
        for (let i = 0; i < choices.length; i++) {
          const choiceText = await choices[i].textContent();
          const mechanicalText = await choices[i].$eval('.choice-mechanics, .mechanic-item, .choice-effects', el => el?.textContent || '') 
            .catch(() => '');
          console.log(`Choice ${i + 1}: ${choiceText.trim()}`);
          if (mechanicalText) {
            console.log(`  Mechanics: ${mechanicalText.trim()}`);
          }
        }
        
        // Check if we have exactly 5 choices
        if (choices.length === 5) {
          console.log('✓ SUCCESS: Elena shows exactly 5 conversation choices!');
          
          // Check for variety of effect types
          const pageContent = await page.content();
          const hasExitChoice = pageContent.includes('Leave conversation') || pageContent.includes('should go');
          const hasTokenChoice = pageContent.includes('Trust token') || pageContent.includes('♥');
          const hasObligationChoice = pageContent.includes('binding obligation') || pageContent.includes('⛓');
          const hasRouteChoice = pageContent.includes('route') || pageContent.includes('🗺');
          const hasInvestigateChoice = pageContent.includes('investigation') || pageContent.includes('🔍');
          
          console.log('\nChoice variety check:');
          console.log(`  Exit choice: ${hasExitChoice ? '✓' : '✗'}`);
          console.log(`  Token choice: ${hasTokenChoice ? '✓' : '✗'}`);
          console.log(`  Obligation choice: ${hasObligationChoice ? '✓' : '✗'}`);
          console.log(`  Route/Location choice: ${hasRouteChoice ? '✓' : '✗'}`);
          console.log(`  Investigation choice: ${hasInvestigateChoice ? '✓' : '✗'}`);
          
        } else {
          console.log(`✗ FAIL: Expected 5 choices, but found ${choices.length}`);
        }
        
      } else {
        console.log('ERROR: Could not find Start Conversation button');
      }
    } else {
      console.log('ERROR: Could not find Elena card');
    }
    
  } catch (error) {
    console.error('Test failed:', error);
  } finally {
    await browser.close();
  }
})();
EOF

# Kill the server
echo "Stopping server..."
kill $SERVER_PID 2>/dev/null

echo "Test complete!"