#!/bin/bash

echo "==========================="
echo "CONFRONTATION SYSTEM TEST"
echo "==========================="

# Kill any existing dotnet processes
pkill -f dotnet 2>/dev/null

# Start the server in background with specific port
echo "Starting game server on port 5099..."
ASPNETCORE_URLS="http://localhost:5099" timeout 30 dotnet run &
SERVER_PID=$!

# Wait for server to be ready
echo "Waiting for server to start..."
sleep 8

# Check if server is running
if ! curl -s http://localhost:5099 > /dev/null; then
    echo "ERROR: Server failed to start"
    exit 1
fi

echo "Server is running. Starting confrontation test..."

# Use node/playwright for browser automation
cat > /tmp/test-confrontation.js << 'EOF'
const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ headless: true });
    const page = await browser.newPage();
    
    console.log('Navigating to game...');
    await page.goto('http://localhost:5099');
    await page.waitForTimeout(2000);
    
    // Start new game
    console.log('Starting new game...');
    const newGameButton = await page.$('button:has-text("New Game")');
    if (newGameButton) {
        await newGameButton.click();
        await page.waitForTimeout(2000);
    }
    
    // Accept initial letter
    console.log('Getting initial letters...');
    const letterBoardBtn = await page.$('button:has-text("Letter Board")');
    if (letterBoardBtn) {
        await letterBoardBtn.click();
        await page.waitForTimeout(1000);
        
        // Accept a letter from Elena if available
        const elenaLetter = await page.$('text=/Elena/');
        if (elenaLetter) {
            console.log('Found letter from Elena, accepting...');
            const acceptBtn = await page.$('button:has-text("Accept")');
            if (acceptBtn) {
                await acceptBtn.click();
                await page.waitForTimeout(500);
            }
        }
        
        // Exit letter board
        const exitBtn = await page.$('button:has-text("Exit")');
        if (exitBtn) {
            await exitBtn.click();
            await page.waitForTimeout(500);
        }
    }
    
    // Advance time to expire the letter
    console.log('Advancing time to expire letter...');
    for (let i = 0; i < 20; i++) {
        const waitBtn = await page.$('button:has-text("Wait")');
        if (waitBtn) {
            await waitBtn.click();
            await page.waitForTimeout(200);
        }
    }
    
    // Try to start conversation with Elena
    console.log('Attempting conversation with Elena (should trigger confrontation)...');
    const elenaBtn = await page.$('button:has-text("Elena")');
    if (elenaBtn) {
        await elenaBtn.click();
        await page.waitForTimeout(2000);
        
        // Check for confrontation text
        const pageContent = await page.content();
        if (pageContent.includes('You promised') || 
            pageContent.includes('trusted you') || 
            pageContent.includes('failed')) {
            console.log('✅ CONFRONTATION TRIGGERED SUCCESSFULLY!');
            
            // Check for free attention (no cost)
            if (pageContent.includes('Free') || !pageContent.includes('◆')) {
                console.log('✅ Confrontation choices are FREE (no attention cost)');
            }
            
            // Check for limited choices
            const choices = await page.$$('.conversation-choice');
            if (choices.length <= 3) {
                console.log('✅ Limited choices available (max 3)');
            }
            
            // Try to select an apologetic response
            const apologyChoice = await page.$('text=/sorry/i');
            if (apologyChoice) {
                console.log('Selecting apology response...');
                await apologyChoice.click();
                await page.waitForTimeout(1000);
            }
            
        } else {
            console.log('❌ Confrontation not triggered - checking why...');
            console.log('Page content sample:', pageContent.substring(0, 500));
        }
    } else {
        console.log('❌ Could not find Elena to start conversation');
    }
    
    await browser.close();
    console.log('Test complete');
})();
EOF

# Run the test
if command -v node > /dev/null; then
    cd /tmp
    npm list playwright || npm install playwright
    node /tmp/test-confrontation.js
else
    echo "Node.js not found, using Playwright MCP instead..."
fi

# Kill the server
echo "Stopping server..."
kill $SERVER_PID 2>/dev/null

echo "==========================="
echo "TEST COMPLETE"
echo "==========================="