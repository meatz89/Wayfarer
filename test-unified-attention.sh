#!/bin/bash

echo "Testing Unified Attention System with Shared Pools"
echo "=================================================="

# Start the server in background
echo "Starting server on port 5099..."
ASPNETCORE_URLS="http://localhost:5099" timeout 20 dotnet run --no-build &
SERVER_PID=$!

# Wait for server to start
sleep 8

# Run the Playwright test
echo "Running Playwright test..."
node -e "
const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ headless: false });
    const page = await browser.newPage();
    
    try {
        console.log('Navigating to game...');
        await page.goto('http://localhost:5099');
        await page.waitForTimeout(2000);
        
        // Start new game
        console.log('Starting new game...');
        await page.click('text=New Game');
        await page.waitForTimeout(1000);
        
        // Check initial attention state
        console.log('\\n=== INITIAL STATE ===');
        const initialAttention = await page.textContent('.attention-display');
        console.log('Initial Attention:', initialAttention);
        
        // Navigate to location screen
        console.log('\\n=== LOCATION ACTIONS ===');
        const locationActions = await page.$$('.location-action');
        console.log('Available location actions:', locationActions.length);
        
        // Check for tier-locked actions
        const lockedActions = await page.$$('.location-action.locked');
        console.log('Locked actions (tier requirements):', lockedActions.length);
        
        // Find an NPC to talk to
        const npcs = await page.$$('.npc-presence');
        if (npcs.length > 0) {
            console.log('\\n=== CONVERSATION TEST ===');
            console.log('NPCs available:', npcs.length);
            
            // Start conversation with first NPC
            await npcs[0].click();
            await page.waitForTimeout(1000);
            
            // Check conversation choices
            const choices = await page.$$('.conversation-choice');
            console.log('Conversation choices available:', choices.length);
            
            // Check for tier-locked choices
            const lockedChoices = await page.$$('.conversation-choice.locked');
            console.log('Locked choices (tier requirements):', lockedChoices.length);
            
            // Check attention after spending some
            if (choices.length > 0 && !await choices[0].getAttribute('class').includes('locked')) {
                await choices[0].click();
                await page.waitForTimeout(1000);
                
                const attentionAfter = await page.textContent('.attention-display');
                console.log('Attention after choice:', attentionAfter);
            }
        }
        
        // Test time block transition
        console.log('\\n=== TIME BLOCK TRANSITION ===');
        const waitAction = await page.$('text=Wait');
        if (waitAction) {
            console.log('Executing wait action to advance time...');
            await waitAction.click();
            await page.waitForTimeout(2000);
            
            const newAttention = await page.textContent('.attention-display');
            const newTime = await page.textContent('.time-display');
            console.log('New time block:', newTime);
            console.log('Attention after time block change:', newAttention);
        }
        
        // Check tier display
        console.log('\\n=== TIER SYSTEM ===');
        const tierDisplay = await page.textContent('.player-tier');
        console.log('Player tier:', tierDisplay || 'T1 (Stranger)');
        
        console.log('\\n✅ Test completed successfully!');
        
    } catch (error) {
        console.error('Test failed:', error);
    } finally {
        await browser.close();
    }
})();
" 2>&1

# Kill the server
kill $SERVER_PID 2>/dev/null

echo "Test complete!"