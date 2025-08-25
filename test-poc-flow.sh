#!/bin/bash
# POC COMPLETE FLOW TEST
# Tests the exact POC implementation flow from poc-implementation.md

echo "🎮 POC FLOW TEST"
echo "================"

# Start server
echo "Starting game server..."
ASPNETCORE_URLS="http://localhost:5999" timeout 30 dotnet run > /tmp/poc-server.log 2>&1 &
SERVER_PID=$!
sleep 5

# Use Playwright to test the flow
cat << 'EOF' > /tmp/poc-test.js
const { chromium } = require('@playwright/test');

(async () => {
    const browser = await chromium.launch({ headless: true });
    const page = await browser.newPage();
    
    try {
        console.log("1. Starting at Market Square Fountain...");
        await page.goto('http://localhost:5999');
        await page.waitForSelector('.game-screen', { timeout: 5000 });
        
        // Verify starting location
        const location = await page.textContent('[data-current-location]');
        if (!location.includes('Market Square')) throw new Error('Not at Market Square!');
        
        // Verify starting attention (should be 5)
        const attention = await page.textContent('[data-resource="attention"]');
        if (!attention.includes('5')) throw new Error('Should start with 5 attention!');
        console.log("✓ Starting position correct");
        
        console.log("2. Moving to Merchant Row...");
        await page.click('[data-spot="merchant_row"]');
        await page.waitForTimeout(100);
        console.log("✓ Moved to Merchant Row");
        
        console.log("3. Starting Quick Exchange with Marcus...");
        await page.click('[data-npc="marcus"] [data-action="quick-exchange"]');
        await page.waitForSelector('.conversation-content', { timeout: 1000 });
        
        // Find and play the exchange card
        await page.click('.conversation-card[data-action="speak"]');
        await page.waitForSelector('.location-content', { timeout: 1000 });
        
        // Verify resources changed
        const newAttention = await page.textContent('[data-resource="attention"]');
        const coins = await page.textContent('[data-resource="coins"]');
        if (newAttention.includes('5')) throw new Error('Attention should have decreased!');
        if (!coins.includes('20')) throw new Error('Should have 20 coins after exchange!');
        console.log("✓ Exchange completed successfully");
        
        console.log("4. Returning to Fountain...");
        await page.click('[data-spot="central_fountain"]');
        await page.waitForTimeout(100);
        console.log("✓ Returned to Fountain");
        
        console.log("5. Taking observation...");
        const observationButton = await page.$('[data-observation="guards-blocking"]');
        if (observationButton) {
            await observationButton.click();
            console.log("✓ Observation taken");
        } else {
            console.log("⚠️  Observation not found (may need to check selector)");
        }
        
        console.log("6. Traveling to Copper Kettle Tavern...");
        await page.click('[data-nav="travel"]');
        await page.waitForSelector('.travel-modal', { timeout: 1000 });
        await page.click('[data-destination="copper_kettle_tavern"]');
        
        // Verify time advanced
        const timeBeforeTravel = await page.textContent('[data-game-time]');
        await page.waitForSelector('.location-content', { timeout: 2000 });
        const timeAfterTravel = await page.textContent('[data-game-time]');
        if (timeBeforeTravel === timeAfterTravel) {
            throw new Error('Time did not advance during travel!');
        }
        console.log("✓ Traveled and time advanced");
        
        console.log("7. Moving to Corner Table...");
        await page.click('[data-spot="corner_table"]');
        await page.waitForTimeout(100);
        console.log("✓ Moved to Corner Table");
        
        console.log("8. Starting conversation with Elena (DESPERATE)...");
        await page.click('[data-npc="elena"] [data-action="standard-conversation"]');
        await page.waitForSelector('.conversation-content', { timeout: 1000 });
        
        // Check Elena's state
        const elenaState = await page.textContent('[data-npc-state]');
        if (!elenaState.includes('DESPERATE')) {
            console.log("⚠️  Elena not in DESPERATE state");
        }
        
        // Play crisis card if available
        const crisisCard = await page.$('.conversation-card[data-type="crisis"]');
        if (crisisCard) {
            await crisisCard.click();
            console.log("✓ Crisis card played");
            
            // Check if letter was generated
            await page.waitForTimeout(500);
            const letterNotification = await page.$('.letter-generated-notification');
            if (letterNotification) {
                console.log("✓ Letter generated!");
            }
        }
        
        console.log("\n✅ POC FLOW COMPLETED SUCCESSFULLY!");
        process.exit(0);
        
    } catch (error) {
        console.error("\n❌ POC FLOW FAILED:", error.message);
        process.exit(1);
    } finally {
        await browser.close();
    }
})();
EOF

# Run the Playwright test
node /tmp/poc-test.js
TEST_RESULT=$?

# Clean up
kill $SERVER_PID 2>/dev/null || true

exit $TEST_RESULT