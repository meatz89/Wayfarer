#!/bin/bash

# Start the server and run Playwright test for dynamic conversation choices

echo "🚀 Starting Wayfarer server..."
cd /mnt/c/git/wayfarer/src
dotnet run --project Wayfarer.csproj &
SERVER_PID=$!

# Wait for server to be ready
echo "⏳ Waiting for server to start..."
sleep 10

# Check if server is running
if ! ps -p $SERVER_PID > /dev/null; then
    echo "❌ Server failed to start"
    exit 1
fi

echo "✅ Server started with PID: $SERVER_PID"

# Run Playwright test
echo "🎭 Running Playwright browser test..."
npx playwright test test-conversation-flow.js --headed

# Kill the server
echo "🛑 Stopping server..."
kill $SERVER_PID

echo "✅ Test complete!"