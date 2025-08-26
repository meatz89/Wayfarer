#!/bin/bash

echo "Testing Exchange Availability (Only MERCANTILE NPCs should have Quick Exchange)"
echo "================================================================"

# Start server
echo "Starting server on port 5099..."
ASPNETCORE_URLS="http://localhost:5099" timeout 15 dotnet run --no-build > /tmp/server.log 2>&1 &
SERVER_PID=$!

# Wait for server
sleep 5

# Run Playwright test
node -e "
const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ headless: true });
    const page = await browser.newPage();
    
    try {
        console.log('\\n1. Navigating to game...');
        await page.goto('http://localhost:5099');
        await page.waitForTimeout(2000);
        
        // Start game
        console.log('2. Starting game...');
        await page.click('text=\"Start Journey\"');
        await page.waitForTimeout(1000);
        
        // Check Elena (DEVOTED) - should NOT have Quick Exchange
        console.log('\\n3. Checking Elena (DEVOTED personality)...');
        const elenaCard = await page.locator('.npc-card:has-text(\"Elena\")').first();
        if (await elenaCard.isVisible()) {
            console.log('   ✓ Found Elena NPC card');
            
            // Click Elena to see options
            await elenaCard.click();
            await page.waitForTimeout(500);
            
            // Check for conversation options
            const quickExchangeButton = page.locator('button:has-text(\"Quick Exchange\")');
            if (await quickExchangeButton.isVisible({ timeout: 1000 }).catch(() => false)) {
                console.log('   ✗ ERROR: Elena has Quick Exchange option (should NOT have it!)');
            } else {
                console.log('   ✓ Elena does NOT have Quick Exchange (correct!)');
            }
            
            // Check what options Elena does have
            const standardButton = page.locator('button:has-text(\"Standard Conversation\")');
            if (await standardButton.isVisible({ timeout: 1000 }).catch(() => false)) {
                console.log('   ✓ Elena has Standard Conversation option');
            }
            
            // Close dialog
            const closeButton = page.locator('button:has-text(\"Cancel\")');
            if (await closeButton.isVisible()) {
                await closeButton.click();
            }
        } else {
            console.log('   ✗ Could not find Elena');
        }
        
        // Check Marcus (MERCANTILE) - SHOULD have Quick Exchange
        console.log('\\n4. Checking Marcus (MERCANTILE personality)...');
        const marcusCard = await page.locator('.npc-card:has-text(\"Marcus\")').first();
        if (await marcusCard.isVisible()) {
            console.log('   ✓ Found Marcus NPC card');
            
            // Click Marcus to see options
            await marcusCard.click();
            await page.waitForTimeout(500);
            
            // Check for Quick Exchange option
            const quickExchangeButton = page.locator('button:has-text(\"Quick Exchange\")');
            if (await quickExchangeButton.isVisible({ timeout: 1000 }).catch(() => false)) {
                console.log('   ✓ Marcus HAS Quick Exchange option (correct!)');
                
                // Try to click it
                await quickExchangeButton.click();
                await page.waitForTimeout(1000);
                
                // Check if exchange cards are shown
                const exchangeCard = page.locator('.exchange-card').first();
                if (await exchangeCard.isVisible({ timeout: 2000 }).catch(() => false)) {
                    console.log('   ✓ Exchange cards displayed for Marcus');
                } else {
                    console.log('   ℹ Exchange interface not fully visible');
                }
            } else {
                console.log('   ✗ ERROR: Marcus does NOT have Quick Exchange (should have it!)');
            }
        } else {
            console.log('   ✗ Could not find Marcus');
        }
        
        console.log('\\n5. Test Summary:');
        console.log('   Only MERCANTILE NPCs (like Marcus) should have Quick Exchange');
        console.log('   Other personalities (like Elena with DEVOTED) should NOT have it');
        
    } catch (error) {
        console.error('Test error:', error);
    } finally {
        await browser.close();
    }
})();
"

# Kill server
kill $SERVER_PID 2>/dev/null

echo ""
echo "Test complete!"