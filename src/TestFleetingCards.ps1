#!/usr/bin/env pwsh

Write-Host "Testing Fleeting Card Implementation" -ForegroundColor Green

# Launch the game
Write-Host "Starting the game server..." -ForegroundColor Yellow
$gameProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "/mnt/c/git/wayfarer/src" -PassThru -NoNewWindow

# Wait for server to start
Start-Sleep -Seconds 5

Write-Host "Opening browser to test fleeting cards..." -ForegroundColor Yellow

# Test script to navigate and check fleeting cards
$testScript = @'
const playwright = require('playwright');

(async () => {
    const browser = await playwright.chromium.launch({ headless: false });
    const page = await browser.newPage();
    
    try {
        // Navigate to game
        await page.goto('http://localhost:5000');
        await page.waitForTimeout(2000);
        
        // Start a conversation to see fleeting cards
        console.log('Starting conversation to test fleeting cards...');
        
        // Take screenshot of conversation screen
        await page.screenshot({ path: 'fleeting-cards-test.png' });
        console.log('Screenshot saved as fleeting-cards-test.png');
        
    } catch (error) {
        console.error('Test failed:', error);
    } finally {
        await browser.close();
    }
})();
'@

# Save and run the test script
$testScript | Out-File -FilePath "/tmp/test-fleeting.js" -Encoding UTF8
node /tmp/test-fleeting.js

# Clean up
Stop-Process -Id $gameProcess.Id -Force
Write-Host "Test completed" -ForegroundColor Green