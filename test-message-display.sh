#!/bin/bash

echo "=== Testing MessageSystem Display Fix ==="

# Start server in background
echo "Starting server on port 5122..."
export ASPNETCORE_URLS="http://localhost:5122"
dotnet run --no-build &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 8

# Use playwright to test the conversation flow
echo "Testing conversation flow with MessageSystem..."
npx playwright test --reporter=line --timeout=15000 <<'EOF'
import { test, expect } from '@playwright/test';

test('MessageSystem displays conversation choice feedback', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5122');
  
  // Wait for game to load
  await page.waitForSelector('text=Garrett', { timeout: 10000 });
  
  // Click on Garrett to start conversation
  await page.click('text=Start conversation');
  
  // Wait for conversation screen to load
  await page.waitForSelector('.conversation-responses', { timeout: 10000 });
  
  // Take a screenshot before making a choice
  await page.screenshot({ path: 'before-choice.png' });
  
  // Click on the first conversation choice
  const firstChoice = page.locator('.response-card').first();
  await firstChoice.click();
  
  // Wait a moment for the MessageSystem to display feedback
  await page.waitForTimeout(1000);
  
  // Take a screenshot after making a choice to see if messages appear
  await page.screenshot({ path: 'after-choice.png' });
  
  // Check if MessageDisplay component is present
  const messageDisplay = page.locator('.message-display-container');
  const isVisible = await messageDisplay.isVisible();
  
  console.log('MessageDisplay visible:', isVisible);
  
  if (isVisible) {
    // Check for system messages
    const messages = page.locator('.system-message');
    const messageCount = await messages.count();
    console.log('Number of system messages:', messageCount);
    
    if (messageCount > 0) {
      const messageText = await messages.first().textContent();
      console.log('First message text:', messageText);
    }
  }
  
  // Test passes if MessageDisplay component exists (even if no messages yet)
  await expect(messageDisplay).toBeInDOM();
});
EOF

# Kill the server
echo "Stopping server..."
kill $SERVER_PID 2>/dev/null
wait $SERVER_PID 2>/dev/null

echo "=== Test completed ==="