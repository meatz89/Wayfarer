#!/bin/bash

echo "Starting server on port 5099..."
ASPNETCORE_URLS="http://localhost:5099" dotnet run > server.log 2>&1 &
SERVER_PID=$!

echo "Waiting for server to start..."
sleep 8

echo "Server running with PID: $SERVER_PID"
echo "Test the conversation system manually at http://localhost:5099"
echo "Look for Elena and start a conversation to see the choices"
echo ""
echo "Server logs:"
tail -f server.log | grep -E "\[NPCEmotionalStateCalculator\]|\[ConversationChoiceGenerator\]|\[Conversation\]"