#!/bin/bash

echo "=== WAYFARER AUTOMATED STARTUP TEST ==="
echo "Building and testing game initialization..."

cd /mnt/c/git/wayfarer/src

# Build first
echo "Building project..."
dotnet build --no-restore > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi
echo "✅ Build succeeded"

# Start the server in background
echo "Starting server..."
ASPNETCORE_URLS="http://localhost:5089" dotnet run --no-build > startup.log 2>&1 &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server initialization..."
for i in {1..10}; do
    if grep -q "Now listening on" startup.log 2>/dev/null; then
        echo "✅ Server started"
        break
    fi
    sleep 1
done

# Give it a moment more for full initialization
sleep 2

# Check if there were phase errors in startup
if grep -q "Phase .* threw exception" startup.log; then
    echo "⚠️ Phase errors detected during startup:"
    grep -A 3 "threw exception" startup.log
fi

if grep -q "Content validation failed" startup.log; then
    echo "⚠️ Content validation errors:"
    grep -A 5 "validation failed" startup.log
fi

# Call the test endpoint
echo "Testing game initialization..."
RESPONSE=$(curl -s http://localhost:5089/api/test/startup)

if [ -z "$RESPONSE" ]; then
    echo "❌ No response from test endpoint"
    echo "Server output:"
    tail -20 startup.log
    kill $SERVER_PID 2>/dev/null
    exit 1
fi

# Parse the response (basic parsing without jq)
if echo "$RESPONSE" | grep -q '"success":true'; then
    echo "✅ GAME INITIALIZATION SUCCESSFUL!"
    echo "Stats:"
    echo "$RESPONSE" | grep -o '"locations":[0-9]*' | cut -d: -f2 | xargs echo "  - Locations:"
    echo "$RESPONSE" | grep -o '"spots":[0-9]*' | cut -d: -f2 | xargs echo "  - Location Spots:"
    echo "$RESPONSE" | grep -o '"npcs":[0-9]*' | cut -d: -f2 | xargs echo "  - NPCs:"
    echo "$RESPONSE" | grep -o '"lettersInQueue":[0-9]*' | cut -d: -f2 | xargs echo "  - Letters in Queue:"
else
    echo "❌ GAME INITIALIZATION FAILED!"
    echo "Response:"
    echo "$RESPONSE"
    echo ""
    echo "Server startup log:"
    grep -E "ERROR|CRITICAL|threw exception|validation failed" startup.log
fi

# Clean up
kill $SERVER_PID 2>/dev/null
rm -f startup.log

echo ""
echo "Test complete."