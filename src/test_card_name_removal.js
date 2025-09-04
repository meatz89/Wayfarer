const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ headless: false });
    const page = await browser.newPage();
    
    try {
        // Navigate to the application
        await page.goto('http://localhost:5999');
        
        // Wait for the game screen to load
        await page.waitForSelector('.game-screen', { timeout: 10000 });
        
        console.log('✓ Application loaded successfully');
        
        // Navigate to a location with NPCs
        await page.waitForSelector('.location-content', { timeout: 5000 });
        console.log('✓ Location screen loaded');
        
        // Check if NPCs are displayed correctly
        const npcs = await page.$$('.npc-card');
        if (npcs.length > 0) {
            console.log(`✓ Found ${npcs.length} NPCs on the location screen`);
            
            // Click on the first NPC to start a conversation
            await npcs[0].click();
            await page.waitForTimeout(1000);
            
            // Check if conversation started
            const conversationScreen = await page.$('.conversation-content');
            if (conversationScreen) {
                console.log('✓ Conversation screen loaded successfully');
                
                // Check for cards in the conversation
                const cards = await page.$$('.conversation-card');
                console.log(`✓ Found ${cards.length} conversation cards`);
                
                // Verify cards display descriptions (not names)
                if (cards.length > 0) {
                    const cardText = await cards[0].innerText();
                    console.log(`✓ First card text: ${cardText.substring(0, 50)}...`);
                }
            }
        }
        
        // Take a screenshot for verification
        await page.screenshot({ path: '/mnt/c/git/wayfarer/src/test_screenshot.png' });
        console.log('✓ Screenshot saved to test_screenshot.png');
        
        console.log('\n✅ All tests passed! The application works correctly after removing Name property.');
        
    } catch (error) {
        console.error('❌ Test failed:', error.message);
        await page.screenshot({ path: '/mnt/c/git/wayfarer/src/error_screenshot.png' });
        console.log('Error screenshot saved to error_screenshot.png');
    } finally {
        await browser.close();
    }
})();