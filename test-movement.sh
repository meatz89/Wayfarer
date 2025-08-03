#!/bin/bash

# Test the movement system with the new intent-based architecture

echo "Testing movement system..."

# Start the server in background
cd /mnt/c/git/wayfarer/src
dotnet run &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 5

# Test if server is running
curl -s http://localhost:5200 > /dev/null
if [ $? -eq 0 ]; then
    echo "✓ Server started successfully"
else
    echo "✗ Server failed to start"
    kill $SERVER_PID 2>/dev/null
    exit 1
fi

# Clean up
kill $SERVER_PID
echo "Test completed"