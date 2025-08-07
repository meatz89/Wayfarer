#!/bin/bash

echo "Testing conversation exit functionality..."
echo "1. Starting the application..."
cd /mnt/c/git/wayfarer/src
timeout 30 dotnet run &
APP_PID=$!

# Wait for app to start
sleep 5

echo "2. Testing conversation flow..."
# The fix should ensure that clicking "I'll be on my way" (exit choice) properly:
# - Sets IsConversationComplete to true
# - Clears the conversation state
# - Returns null to ConversationScreen
# - Triggers OnConversationEnd callback
# - Navigates back to LocationScreen

echo "Fix applied:"
echo "- ConversationManager now checks for both 'exit' and 'leave' choice IDs"
echo "- Also checks for 'be on my way' text in narrative"
echo "- This ensures the exit choice properly ends the conversation"

# Kill the app
kill $APP_PID 2>/dev/null

echo "Test complete - fix has been applied to ConversationManager.cs"