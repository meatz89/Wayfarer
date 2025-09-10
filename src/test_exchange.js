const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ headless: false });
    const page = await browser.newPage();
    
    try {
        console.log('Navigating to game...');
        await page.goto('http://localhost:5999');
        await page.waitForTimeout(2000);
        
        // Find Marcus NPC
        console.log('Looking for Marcus NPC...');
        const marcusCard = await page.locator('.npc-card').filter({ hasText: 'Marcus' }).first();
        if (await marcusCard.count() === 0) {
            throw new Error('Marcus NPC not found');
        }
        
        // Click on Marcus to see options
        console.log('Clicking on Marcus...');
        await marcusCard.click();
        await page.waitForTimeout(1000);
        
        // Look for Exchange button in conversation options
        console.log('Looking for Exchange option...');
        const exchangeButton = await page.locator('.conversation-options button, .conversation-card').filter({ hasText: /Exchange|Trade|Purchase/i }).first();
        
        if (await exchangeButton.count() > 0) {
            console.log('Found Exchange option! Clicking it...');
            await exchangeButton.click();
            await page.waitForTimeout(2000);
            
            // Check if exchange UI is showing
            const exchangeContent = await page.locator('.exchange-content, .exchange-container').first();
            if (await exchangeContent.count() > 0) {
                console.log('✅ Exchange UI is displayed!');
                
                // Look for exchange cards
                const exchangeCards = await page.locator('.exchange-card, .card').filter({ hasText: /food|provisions|bread/i });
                const cardCount = await exchangeCards.count();
                console.log(`Found ${cardCount} exchange cards`);
                
                if (cardCount > 0) {
                    console.log('✅ Exchange cards are showing! The fix worked!');
                    
                    // Get the text of the first exchange card
                    const firstCardText = await exchangeCards.first().textContent();
                    console.log('First exchange card content:', firstCardText);
                } else {
                    console.log('⚠️ No exchange cards found - checking for empty state...');
                    const pageContent = await page.content();
                    if (pageContent.includes('No exchanges available') || pageContent.includes('empty')) {
                        console.log('❌ Exchange list is empty - ExchangeFacade may not be returning data');
                    }
                }
            } else {
                console.log('❌ Exchange UI not found after clicking Exchange');
            }
        } else {
            console.log('❌ No Exchange option found in conversation options');
            
            // Try to check what options are available
            const allOptions = await page.locator('.conversation-options button, .conversation-card').allTextContents();
            console.log('Available options:', allOptions);
        }
        
        // Take a screenshot for debugging
        await page.screenshot({ path: 'exchange_test.png' });
        console.log('Screenshot saved as exchange_test.png');
        
    } catch (error) {
        console.error('Error during test:', error);
        await page.screenshot({ path: 'exchange_error.png' });
    } finally {
        await browser.close();
    }
})();