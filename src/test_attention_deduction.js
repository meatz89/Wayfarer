// Test script to verify attention deduction when starting conversations
// This script uses Playwright to test the actual UI flow

const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ headless: false });
    const page = await browser.newPage();
    
    try {
        console.log('Navigating to game...');
        await page.goto('http://localhost:5099');
        
        // Wait for game to load
        await page.waitForSelector('.game-content', { timeout: 10000 });
        console.log('Game loaded successfully');
        
        // Take initial screenshot
        await page.screenshot({ path: 'before_conversation.png', fullPage: true });
        console.log('Initial screenshot saved as before_conversation.png');
        
        // Check initial attention state in location screen
        const initialAttention = await page.locator('.attention-display').first().textContent();
        console.log(`Initial attention state: ${initialAttention}`);
        
        // Look for NPCs with conversation options
        const npcCards = await page.locator('.npc-card').count();
        console.log(`Found ${npcCards} NPCs in location`);
        
        if (npcCards > 0) {
            // Find a conversation option that costs 2 attention
            const conversationOption = page.locator('.interaction').filter({ hasText: '2 attention' }).first();
            const hasConversation = await conversationOption.count() > 0;
            
            if (hasConversation) {
                console.log('Found conversation option requiring 2 attention');
                
                // Click to start conversation
                await conversationOption.click();
                console.log('Clicked conversation option');
                
                // Wait for navigation to conversation screen
                await page.waitForTimeout(1000);
                
                // Check if we're in conversation screen
                const inConversation = await page.locator('.conversation-screen').count() > 0;
                
                if (inConversation) {
                    console.log('Successfully entered conversation screen');
                    
                    // Take screenshot of conversation
                    await page.screenshot({ path: 'in_conversation.png', fullPage: true });
                    console.log('Conversation screenshot saved as in_conversation.png');
                    
                    // Exit conversation to check attention in location screen
                    const exitButton = page.locator('button').filter({ hasText: /exit|leave|return/i }).first();
                    if (await exitButton.count() > 0) {
                        await exitButton.click();
                        await page.waitForTimeout(1000);
                    }
                } else {
                    console.log('Did not enter conversation - checking for error message');
                    const errorMessage = await page.locator('.system-message').last().textContent();
                    console.log(`System message: ${errorMessage}`);
                }
                
                // Take final screenshot
                await page.screenshot({ path: 'after_conversation.png', fullPage: true });
                console.log('Final screenshot saved as after_conversation.png');
                
                // Check final attention state
                const finalAttention = await page.locator('.attention-display').first().textContent();
                console.log(`Final attention state: ${finalAttention}`);
                
                // Verify attention was deducted
                console.log('\n=== TEST RESULTS ===');
                console.log(`Initial attention: ${initialAttention}`);
                console.log(`Final attention: ${finalAttention}`);
                
                if (initialAttention !== finalAttention) {
                    console.log('✅ SUCCESS: Attention was deducted!');
                } else {
                    console.log('❌ FAILURE: Attention was NOT deducted');
                }
                
            } else {
                console.log('No conversation options with 2 attention cost found');
                console.log('Looking for any interaction options...');
                const anyInteraction = await page.locator('.interaction').first();
                if (await anyInteraction.count() > 0) {
                    const interactionText = await anyInteraction.textContent();
                    console.log(`Found interaction: ${interactionText}`);
                }
            }
        } else {
            console.log('No NPCs found in current location');
        }
        
    } catch (error) {
        console.error('Test failed:', error);
        await page.screenshot({ path: 'error_screenshot.png', fullPage: true });
        console.log('Error screenshot saved as error_screenshot.png');
    } finally {
        await browser.close();
        console.log('\nTest completed');
    }
})();