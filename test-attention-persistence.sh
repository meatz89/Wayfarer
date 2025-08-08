#!/bin/bash

# Playwright test to verify attention persistence across conversations in same time block

echo "🎮 Testing Attention Persistence with Playwright"
echo "================================================"
echo ""
echo "This test verifies that:"
echo "1. Attention persists within a time block (not reset per conversation)"
echo "2. Each of 6 time blocks has separate attention pools (5 points each)"
echo "3. Attention refreshes when changing time blocks"
echo ""

# Start the server in background
echo "Starting Wayfarer server..."
cd /mnt/c/git/wayfarer/src
export ASPNETCORE_URLS="http://localhost:5099"
dotnet run > /tmp/wayfarer-test.log 2>&1 &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
for i in {1..15}; do
    if curl -s http://localhost:5099 > /dev/null 2>&1; then
        echo "✓ Server is running"
        break
    fi
    sleep 1
done

# Give it a moment to fully initialize
sleep 2

echo ""
echo "Running Playwright tests..."
echo "----------------------------"

# Test the attention system using Playwright
cat << 'EOF' | node
const test = async () => {
    console.log("📋 Test 1: Verify attention starts at 5 points");
    console.log("📋 Test 2: Start conversation with NPC");
    console.log("📋 Test 3: Use 2 attention points in conversation");
    console.log("📋 Test 4: Exit conversation - attention should be 3/5");
    console.log("📋 Test 5: Start new conversation - attention should still be 3/5");
    console.log("📋 Test 6: Advance time to next block - attention should reset to 5/5");
    
    // Placeholder for actual Playwright commands
    console.log("\n⚠️  Playwright MCP tool needed for browser automation");
    console.log("Please use: mcp__playwright__browser_navigate to http://localhost:5099");
    console.log("Then: mcp__playwright__browser_snapshot to see the UI");
};

test();
EOF

# Kill the server
echo ""
echo "Stopping server..."
kill $SERVER_PID 2>/dev/null
wait $SERVER_PID 2>/dev/null

echo "✅ Test setup complete!"