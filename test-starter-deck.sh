#!/bin/bash

# Test script to verify starter deck implementation
echo "Testing Starter Deck Implementation..."
echo "======================================"

# Start the game server
cd /mnt/c/git/wayfarer/src
ASPNETCORE_URLS="http://localhost:5999" timeout 5 dotnet run --no-build 2>&1 | grep -E "(PackageLoader|CardDeckManager|Creating card|Generated card|starter deck|universal|Elena)" &

# Wait for server to start
sleep 3

# Make a request to trigger conversation with Elena
curl -s -X POST http://localhost:5999/api/test/start-conversation \
  -H "Content-Type: application/json" \
  -d '{"npcId": "elena"}' 2>/dev/null || true

# Try to navigate to game
curl -s http://localhost:5999 > /dev/null

# Wait for logs
sleep 2

# Kill the server
pkill -f "dotnet.*5999" 2>/dev/null || true

echo ""
echo "Test complete. Check logs above for card generation."