#!/bin/bash

echo "=== TESTING CONVERSATION NAVIGATION ==="

# Build to temp directory to avoid locked files
echo "Building project..."
dotnet build -o /tmp/wayfarer-test > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi
echo "✅ Build succeeded"

# Start server
echo "Starting server..."
export ASPNETCORE_URLS="http://localhost:5089"
dotnet /tmp/wayfarer-test/Wayfarer.dll > /tmp/test-server.log 2>&1 &
SERVER_PID=$!

# Wait for server to start
sleep 5

# Test server is running
curl -s http://localhost:5089/api/test/startup > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "✅ Server started"
else
    echo "❌ Server failed to start"
    kill $SERVER_PID 2>/dev/null
    exit 1
fi

echo ""
echo "Server is running on http://localhost:5089"
echo "You can now test clicking on Elena to start a conversation"
echo "Check the console logs at: tail -f /tmp/test-server.log"
echo ""
echo "Press Ctrl+C to stop the server"

# Keep server running
wait $SERVER_PID