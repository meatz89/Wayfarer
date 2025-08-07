#!/bin/bash

echo "=== E2E Test: Conversation Flow ==="

# Kill any existing server
pkill -f "dotnet.*Wayfarer" 2>/dev/null || true
sleep 1

# Build the app
echo "Building application..."
dotnet build > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

# Start server in background
echo "Starting server on http://localhost:5089..."
export ASPNETCORE_URLS="http://localhost:5089"
dotnet run &
SERVER_PID=$!

# Wait for server to be ready
echo "Waiting for server to start..."
for i in {1..30}; do
    curl -s http://localhost:5089 > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "✅ Server is ready"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ Server failed to start"
        kill $SERVER_PID 2>/dev/null
        exit 1
    fi
    sleep 1
done

# Run the browser test
echo "Running browser test..."
node test-conversation-browser.js

TEST_RESULT=$?

# Cleanup
echo "Cleaning up..."
kill $SERVER_PID 2>/dev/null || true

if [ $TEST_RESULT -eq 0 ]; then
    echo "✅ E2E Test PASSED"
else
    echo "❌ E2E Test FAILED"
fi

exit $TEST_RESULT