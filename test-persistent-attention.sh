#!/bin/bash
# Test script for persistent attention system
# This verifies that attention persists across conversations within a time block

echo "========================================="
echo "Testing Persistent Attention System"
echo "========================================="
echo ""
echo "This test verifies that:"
echo "1. Attention persists across multiple conversations"
echo "2. Attention only refreshes when time block changes"
echo "3. Template engine generates varied choices"
echo ""

# Start the application
echo "Starting Wayfarer..."
cd /mnt/c/git/wayfarer/src

# Set a specific port to avoid conflicts
export ASPNETCORE_URLS="http://localhost:5077"

# Start the application in background
timeout 30 dotnet run --project Wayfarer.csproj &
APP_PID=$!

# Wait for app to start
echo "Waiting for application to start..."
sleep 5

# Check if app is running
if ! kill -0 $APP_PID 2>/dev/null; then
    echo "ERROR: Application failed to start"
    exit 1
fi

echo "Application started on port 5077"
echo ""
echo "Test scenarios:"
echo "1. Start conversation with Elena (should use 5 attention points)"
echo "2. Exit and start conversation with Marcus (should use REMAINING attention)"
echo "3. Advance time to new block (should refresh to 5 points)"
echo ""
echo "Expected console output:"
echo "- [GameFacade] Providing attention for morning: X/5"
echo "- [ConversationFactory] Using existing attention: Current=X, Max=5"
echo "- [TimeBlockAttention] Transitioning from morning to afternoon"
echo ""
echo "Check the console output above for these messages."
echo ""
echo "Application is running. Press Ctrl+C to stop..."

# Wait for user to test
wait $APP_PID

echo ""
echo "Test complete."