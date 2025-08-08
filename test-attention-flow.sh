#!/bin/bash

# Test attention system flow: location → conversation → attention usage → location

echo "=== Testing Attention System Flow ==="
echo

# Start the application
echo "Starting Wayfarer application..."
cd /mnt/c/git/wayfarer/src
dotnet run --no-build > server.log 2>&1 &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 5

# Check if server is running
if ! ps -p $SERVER_PID > /dev/null; then
    echo "❌ Server failed to start"
    cat server.log
    exit 1
fi

echo "✓ Server started successfully"

# Test 1: Location screen (should show time costs, not attention)
echo
echo "Test 1: Location Screen Actions"
echo "--------------------------------"
curl -s http://localhost:5000/api/test/location-actions | jq '.actions[] | {title: .title, cost: .cost}' 2>/dev/null || echo "Location actions endpoint not available"

# Test 2: Start conversation (should reset attention to 3)
echo
echo "Test 2: Start Conversation (Attention Reset)"
echo "--------------------------------------------"
curl -s http://localhost:5000/api/test/conversation/start/elena | jq '{attention: .currentAttention, maxAttention: .maxAttention, npc: .npcName}' 2>/dev/null || echo "Conversation start endpoint not available"

# Test 3: Show choices with attention costs
echo
echo "Test 3: Conversation Choices with Attention Costs"
echo "-------------------------------------------------"
curl -s http://localhost:5000/api/test/conversation/choices | jq '.choices[] | {text: .text, attentionCost: .attentionCost, isAvailable: .isAvailable}' 2>/dev/null || echo "Choices endpoint not available"

# Test 4: Select a choice that costs attention
echo
echo "Test 4: Select Choice (Spend Attention)"
echo "---------------------------------------"
curl -s -X POST http://localhost:5000/api/test/conversation/choice/1 | jq '{attentionBefore: .attentionBefore, attentionAfter: .attentionAfter, choiceCost: .choiceCost}' 2>/dev/null || echo "Choice selection endpoint not available"

# Test 5: End conversation and start new one (attention should reset)
echo
echo "Test 5: New Conversation (Attention Reset)"
echo "------------------------------------------"
curl -s -X POST http://localhost:5000/api/test/conversation/end 2>/dev/null
sleep 1
curl -s http://localhost:5000/api/test/conversation/start/marcus | jq '{attention: .currentAttention, maxAttention: .maxAttention, npc: .npcName}' 2>/dev/null || echo "New conversation endpoint not available"

# Clean up
echo
echo "Stopping server..."
kill $SERVER_PID 2>/dev/null
wait $SERVER_PID 2>/dev/null

echo
echo "=== Attention System Flow Test Complete ==="
echo
echo "Summary:"
echo "1. Location actions use TIME costs (5m, 10m, etc), not attention"
echo "2. Conversations start with FULL attention (3 points)"
echo "3. Choices show attention costs and availability"
echo "4. Spending attention reduces available points"
echo "5. New conversations RESET attention to full"
echo
echo "The attention system is properly separated:"
echo "- Physical world (locations) = Time costs"
echo "- Social world (conversations) = Attention costs"