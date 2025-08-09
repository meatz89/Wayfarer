#!/bin/bash
# Test delivery system through conversations

echo "=== TESTING LETTER DELIVERY SYSTEM ==="
echo ""
echo "Test objective: Verify letters can be delivered through NPC conversations"
echo "Expected: Position 1 letter (Elena's marriage refusal to Lord Aldwin) should be deliverable"
echo ""

# Use Playwright to test the delivery flow
cat << 'EOF' > /tmp/test-delivery.js
const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  try {
    console.log("1. Loading game...");
    await page.goto('http://localhost:5099');
    await page.waitForTimeout(2000);
    
    // Take initial screenshot
    const snapshot1 = await page.locator('body').textContent();
    console.log("\n2. Initial location: Market Square");
    console.log("   Queue status: " + (snapshot1.match(/Queue: \d+\/\d+/) || ["Not found"])[0]);
    
    // Travel to Noble District
    console.log("\n3. Traveling to Noble District...");
    
    // Click on Noble District route
    const nobleRoute = page.locator('text=/Noble District|noble_district/i').first();
    if (await nobleRoute.isVisible()) {
      await nobleRoute.click();
      await page.waitForTimeout(2000);
      console.log("   ✓ Arrived at Noble District");
    } else {
      console.log("   ✗ Could not find route to Noble District");
    }
    
    // Check for Lord Aldwin
    const snapshot2 = await page.locator('body').textContent();
    if (snapshot2.includes('Lord Aldwin')) {
      console.log("\n4. Lord Aldwin is present");
      
      // Start conversation
      console.log("5. Starting conversation with Lord Aldwin...");
      await page.locator('text=Start conversation').first().click();
      await page.waitForTimeout(1000);
      
      // Check for delivery option
      const conversationContent = await page.locator('body').textContent();
      console.log("\n6. Checking conversation choices:");
      
      // Look for delivery-related text
      if (conversationContent.includes('deliver') || 
          conversationContent.includes('letter') ||
          conversationContent.includes('Elena')) {
        console.log("   ✓ DELIVERY OPTION FOUND!");
        
        // Try to click delivery option
        const deliveryOption = page.locator('text=/deliver|letter from/i').first();
        if (await deliveryOption.isVisible()) {
          console.log("   Attempting delivery...");
          await deliveryOption.click();
          await page.waitForTimeout(1000);
          
          // Check if delivery succeeded
          const afterDelivery = await page.locator('body').textContent();
          const newQueue = (afterDelivery.match(/Queue: \d+\/\d+/) || [""])[0];
          console.log("   Queue after delivery: " + newQueue);
        }
      } else {
        console.log("   ✗ NO DELIVERY OPTION VISIBLE");
        console.log("   Available choices found:");
        
        // List visible conversation options
        const choices = await page.locator('.conversation-choice, [class*="choice"]').allTextContents();
        choices.forEach((choice, i) => {
          if (choice.trim()) console.log(`     ${i+1}. "${choice.trim().substring(0, 50)}..."`);
        });
      }
    } else {
      console.log("   ✗ Lord Aldwin not found at Noble District");
    }
    
  } catch (error) {
    console.error("Test failed:", error.message);
  } finally {
    await browser.close();
  }
})();
EOF

# Install playwright if not already installed
if ! command -v npx &> /dev/null; then
  echo "Installing npx..."
  npm install -g npx
fi

if ! npx playwright --version &> /dev/null; then
  echo "Installing playwright..."
  npm init -y > /dev/null 2>&1
  npm install playwright > /dev/null 2>&1
fi

# Run the test
echo "Running delivery test..."
echo "========================"
node /tmp/test-delivery.js

echo ""
echo "=== TEST COMPLETE ==="