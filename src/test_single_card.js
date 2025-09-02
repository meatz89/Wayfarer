const { chromium } = require('playwright');
const { spawn } = require('child_process');

async function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

async function testSingleCardSelection() {
    let server = null;
    let browser = null;
    
    try {
        // Start the server
        console.log('Starting server...');
        server = spawn('dotnet', ['run', '--no-build'], {
            cwd: '/mnt/c/git/wayfarer/src',
            env: { ...process.env, ASPNETCORE_URLS: 'http://localhost:5555' }
        });
        
        // Wait for server to start
        await sleep(5000);
        
        // Launch browser
        console.log('Launching browser...');
        browser = await chromium.launch({ headless: false });
        const context = await browser.newContext();
        const page = await context.newPage();
        
        // Navigate to the app
        console.log('Navigating to application...');
        await page.goto('http://localhost:5555');
        await page.waitForLoadState('networkidle');
        
        // Take initial screenshot
        await page.screenshot({ path: '/tmp/wayfarer_initial.png' });
        console.log('Initial screenshot saved to /tmp/wayfarer_initial.png');
        
        // Start a new game
        const startButton = await page.locator('button:has-text("Start New Game")').first();
        if (await startButton.isVisible()) {
            console.log('Starting new game...');
            await startButton.click();
            await sleep(2000);
        }
        
        // Find an NPC to talk to
        const npcCards = await page.locator('.npc-card').all();
        if (npcCards.length > 0) {
            console.log(`Found ${npcCards.length} NPCs`);
            
            // Click on first NPC
            await npcCards[0].click();
            await sleep(1000);
            
            // Look for conversation option
            const chatOption = await page.locator('button:has-text("Chat"), button:has-text("Discuss Letter")').first();
            if (await chatOption.isVisible()) {
                console.log('Starting conversation...');
                await chatOption.click();
                await sleep(2000);
                
                // Take screenshot of conversation screen
                await page.screenshot({ path: '/tmp/wayfarer_conversation.png' });
                console.log('Conversation screenshot saved to /tmp/wayfarer_conversation.png');
                
                // Look for conversation cards
                const cards = await page.locator('.conversation-card, .card, .dialog-card').all();
                console.log(`Found ${cards.length} conversation cards`);
                
                if (cards.length > 0) {
                    // Try to select first card
                    console.log('Selecting first card...');
                    await cards[0].click();
                    await sleep(500);
                    
                    // Check if card is selected
                    const selectedCards = await page.locator('.conversation-card.selected, .card.selected, .dialog-card.selected').all();
                    console.log(`${selectedCards.length} card(s) selected`);
                    
                    // Try to select second card (should replace first)
                    if (cards.length > 1) {
                        console.log('Selecting second card (should replace first)...');
                        await cards[1].click();
                        await sleep(500);
                        
                        const newSelectedCards = await page.locator('.conversation-card.selected, .card.selected, .dialog-card.selected').all();
                        console.log(`After selecting second card: ${newSelectedCards.length} card(s) selected`);
                        
                        if (newSelectedCards.length === 1) {
                            console.log('✅ SUCCESS: Single-card selection is working!');
                        } else {
                            console.log('❌ FAILED: Multiple cards selected');
                        }
                    }
                    
                    // Check SPEAK button text
                    const speakButton = await page.locator('button:has-text("SPEAK"), .action-button:has-text("SPEAK")').first();
                    if (await speakButton.isVisible()) {
                        const buttonText = await speakButton.textContent();
                        console.log(`SPEAK button text: ${buttonText}`);
                        
                        // Take final screenshot
                        await page.screenshot({ path: '/tmp/wayfarer_single_card_selected.png' });
                        console.log('Final screenshot saved to /tmp/wayfarer_single_card_selected.png');
                    }
                }
            }
        }
        
        console.log('Test complete!');
        
    } catch (error) {
        console.error('Test failed:', error);
    } finally {
        // Cleanup
        if (browser) await browser.close();
        if (server) {
            server.kill();
            await sleep(1000);
        }
    }
}

// Run the test
testSingleCardSelection().catch(console.error);