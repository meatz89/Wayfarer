#!/bin/bash

echo "=== Testing Verb System Implementation ==="
echo "Starting server on port 5099..."

# Kill any existing process on port 5099
pkill -f "dotnet.*5099" 2>/dev/null
sleep 2

# Start server in background
cd /mnt/c/git/wayfarer/src
ASPNETCORE_URLS="http://localhost:5099" timeout 30 dotnet run &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 10

# Check if server is running
if ! curl -s http://localhost:5099 > /dev/null; then
    echo "ERROR: Server failed to start"
    exit 1
fi

echo "Server started successfully"

# Test conversation with Elena to see verb choices
echo ""
echo "=== Testing Conversation Choices with Elena ==="
curl -s -X POST http://localhost:5099/api/test/start-conversation \
  -H "Content-Type: application/json" \
  -d '{"npcId": "elena_scribe"}' | jq '.choices[] | {verb: .baseVerb, text: .narrativeText, attention: .attentionCost, description: .mechanicalDescription}'

echo ""
echo "=== Checking if verbs have distinct identity ==="
echo "Expected:"
echo "- HELP choices (Attention: 1) - Trust building"
echo "- NEGOTIATE choices (Attention: 1-2) - Queue management"  
echo "- INVESTIGATE choices (Attention: 1-3) - Information discovery"
echo "- EXIT choice (Attention: 0)"

# Kill server
kill $SERVER_PID 2>/dev/null
pkill -f "dotnet.*5099" 2>/dev/null

echo ""
echo "Test complete"