// Playwright test to verify systemic conversation mechanics
// Tests that Elena's DESPERATE state emerges from game state
// and that queue displacement costs are paid by the DISPLACED party

const { chromium } = require('playwright');

async function testSystemicConversation() {
    const browser = await chromium.launch({ 
        headless: false,
        slowMo: 500 // Slow down for visibility
    });
    
    const context = await browser.newContext({
        viewport: { width: 1280, height: 720 }
    });
    
    const page = await context.newPage();
    
    try {
        console.log('🎮 Starting Wayfarer systemic conversation test...');
        
        // Navigate to game
        await page.goto('http://localhost:5099');
        await page.waitForTimeout(3000);
        
        // Check initial queue state
        console.log('📋 Checking initial queue state...');
        const queueSlots = await page.locator('.queue-slot').all();
        console.log(`Found ${queueSlots.length} queue slots`);
        
        // Check if Elena's letter is in the queue but not position 1
        const elenaLetterPosition = await page.evaluate(() => {
            const slots = document.querySelectorAll('.queue-slot');
            for (let i = 0; i < slots.length; i++) {
                const letterInfo = slots[i].textContent || '';
                if (letterInfo.includes('Elena')) {
                    return i + 1; // 1-indexed position
                }
            }
            return -1;
        });
        
        console.log(`Elena's letter is at position: ${elenaLetterPosition}`);
        
        // Check who's in position 1 (will be displaced)
        const position1Letter = await page.evaluate(() => {
            const slot1 = document.querySelector('.queue-slot:nth-child(1)');
            if (!slot1) return null;
            const senderSpan = slot1.querySelector('.letter-sender');
            return senderSpan ? senderSpan.textContent : 'Unknown';
        });
        
        console.log(`Letter in position 1 is from: ${position1Letter}`);
        
        // Find Elena at the Copper Kettle
        console.log('🔍 Looking for Elena at Copper Kettle...');
        const elenaChoice = await page.locator('.npc-interaction-choice:has-text("Elena")').first();
        
        if (await elenaChoice.isVisible()) {
            console.log('✅ Found Elena, starting conversation...');
            await elenaChoice.click();
            await page.waitForTimeout(2000);
            
            // Check Elena's emotional state
            const emotionalState = await page.evaluate(() => {
                // Look for desperate indicators in body language or dialogue
                const bodyLanguage = document.querySelector('.npc-body-language');
                const dialogue = document.querySelector('.npc-dialogue');
                const text = (bodyLanguage?.textContent || '') + (dialogue?.textContent || '');
                
                if (text.includes('trembling') || text.includes('desperate') || 
                    text.includes('urgent') || text.includes('marriage proposal')) {
                    return 'DESPERATE';
                }
                if (text.includes('cold') || text.includes('angry')) {
                    return 'HOSTILE';
                }
                if (text.includes('calculating') || text.includes('thoughtful')) {
                    return 'CALCULATING';
                }
                return 'UNKNOWN';
            });
            
            console.log(`Elena's emotional state: ${emotionalState}`);
            
            // Check if state is DESPERATE (should be due to SAFETY stakes + urgent deadline)
            if (emotionalState === 'DESPERATE') {
                console.log('✅ Elena is correctly DESPERATE from systemic factors');
            } else {
                console.log('⚠️ Elena state not DESPERATE - checking factors...');
            }
            
            // Look for NEGOTIATE choices
            console.log('🔄 Looking for NEGOTIATE choices...');
            const negotiateChoices = await page.locator('.conversation-choice').all();
            
            for (const choice of negotiateChoices) {
                const choiceText = await choice.textContent();
                const mechanicalDesc = await choice.locator('.mechanical-description').textContent().catch(() => '');
                
                // Check if this is the queue reordering choice
                if (mechanicalDesc.includes('Move to position 1')) {
                    console.log('📌 Found queue reordering choice:');
                    console.log(`   Narrative: ${choiceText}`);
                    console.log(`   Mechanical: ${mechanicalDesc}`);
                    
                    // CRITICAL: Check WHO pays the cost
                    if (mechanicalDesc.includes(`with ${position1Letter}`)) {
                        console.log(`✅ CORRECT: Displaced party (${position1Letter}) pays the cost!`);
                    } else if (mechanicalDesc.includes('with Elena')) {
                        console.log('❌ WRONG: Elena pays the cost (should be displaced party)');
                    } else {
                        console.log('⚠️ Unclear who pays the cost');
                    }
                    
                    // Check attention cost
                    const attentionBadge = await choice.locator('.attention-badge').textContent().catch(() => '');
                    if (attentionBadge.includes('◆◆')) {
                        console.log('✅ Correct attention cost: 2 points');
                    }
                    
                    // Check affordability
                    const isAffordable = await choice.evaluate(el => !el.classList.contains('disabled'));
                    console.log(`   Affordable: ${isAffordable}`);
                }
            }
            
            // Look for HELP choices
            const helpChoices = await page.locator('.conversation-choice:has-text("help")').all();
            console.log(`\n📦 Found ${helpChoices.length} HELP choices`);
            
            // Look for INVESTIGATE choices
            const investigateChoices = await page.locator('.conversation-choice:has-text("investigate")').all();
            console.log(`🔍 Found ${investigateChoices.length} INVESTIGATE choices`);
            
            // Check if choices emerge from state (not hardcoded)
            console.log('\n🎯 Testing systemic emergence...');
            
            // The presence of certain choices should depend on:
            // 1. Elena's emotional state (DESPERATE from multiple factors)
            // 2. Queue state (her letter not in position 1)
            // 3. Token relationships (trust levels)
            // 4. Available obligations
            
            const systemicFactors = {
                hasUrgentLetter: elenaLetterPosition > 0,
                notInPosition1: elenaLetterPosition > 1,
                emotionalState: emotionalState,
                displacedPartyIdentified: mechanicalDesc.includes(`with ${position1Letter}`)
            };
            
            console.log('Systemic factors:', systemicFactors);
            
            if (systemicFactors.hasUrgentLetter && 
                systemicFactors.notInPosition1 && 
                systemicFactors.emotionalState === 'DESPERATE' &&
                systemicFactors.displacedPartyIdentified) {
                console.log('\n✅ SUCCESS: Elena conversation emerges systemically!');
                console.log('- Emotional state from letter urgency + stakes');
                console.log('- Queue manipulation available because letter not in position 1');
                console.log('- Cost correctly attributed to displaced party');
            } else {
                console.log('\n⚠️ Some systemic factors missing');
            }
            
        } else {
            console.log('❌ Elena not found at current location');
        }
        
    } catch (error) {
        console.error('❌ Test failed:', error);
    } finally {
        console.log('\n📊 Test complete. Closing browser in 5 seconds...');
        await page.waitForTimeout(5000);
        await browser.close();
    }
}

// Run the test
testSystemicConversation().catch(console.error);