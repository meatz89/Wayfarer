#!/bin/bash

echo "=== Testing Attention System with Unified Choices ==="

# Build the application
echo "Building application..."
cd /mnt/c/git/wayfarer/src
dotnet build > /dev/null 2>&1

# Start the server
echo "Starting server on http://localhost:5100..."
export ASPNETCORE_URLS="http://localhost:5100"
timeout 15 dotnet run &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 5

# Test the conversation endpoint
echo "Testing conversation with Elena..."
curl -s http://localhost:5100/api/conversation/elena | jq '.'

echo ""
echo "Checking for observation choices in response..."
curl -s http://localhost:5100/api/conversation/elena | grep -o '\[Observe:.*\]' || echo "No observation choices found"

# Kill the server
kill $SERVER_PID 2>/dev/null

echo "Test complete."