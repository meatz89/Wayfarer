#!/bin/bash

echo "=== TESTING LETTER DELIVERY FIX ==="
echo "Testing that letters in position 1 can be delivered through conversation"
echo ""

# Start the server
cd /mnt/c/git/wayfarer/src
dotnet run &
SERVER_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 10

# Test sequence using curl
BASE_URL="http://localhost:5099"

echo "1. Starting game..."
curl -s "$BASE_URL/api/tutorial/start" > /dev/null

echo "2. Checking initial queue..."
QUEUE=$(curl -s "$BASE_URL/api/tutorial/queue")
echo "Initial queue: $QUEUE"

echo "3. Getting letter in position 1..."
POSITION1=$(echo "$QUEUE" | grep -o '"position":1[^}]*' | head -1)
echo "Position 1 letter: $POSITION1"

# Extract recipient ID from position 1 letter if it exists
if [[ ! -z "$POSITION1" ]]; then
    RECIPIENT=$(echo "$POSITION1" | grep -o '"recipientId":"[^"]*' | cut -d'"' -f4)
    echo "Recipient ID: $RECIPIENT"
    
    if [[ "$RECIPIENT" == "lord_aldwin" ]]; then
        echo "4. Traveling to Noble District (Lord Aldwin's location)..."
        curl -s -X POST "$BASE_URL/api/tutorial/travel/noble_district/route_market_to_noble" > /dev/null
        sleep 2
        
        echo "5. Starting conversation with Lord Aldwin..."
        CONV=$(curl -s "$BASE_URL/api/tutorial/conversation/lord_aldwin")
        
        echo "6. Checking for delivery choice..."
        if echo "$CONV" | grep -q "deliver_letter"; then
            echo "✅ SUCCESS: Delivery choice found in conversation!"
            echo "Conversation choices:"
            echo "$CONV" | grep -o '"choiceId":"[^"]*"' | head -5
        else
            echo "❌ FAILURE: No delivery choice found"
            echo "Available choices:"
            echo "$CONV" | grep -o '"choiceId":"[^"]*"' | head -5
        fi
        
        echo "7. Attempting to select delivery choice..."
        RESULT=$(curl -s -X POST "$BASE_URL/api/tutorial/conversation/choice/deliver_letter")
        
        echo "8. Checking queue after delivery..."
        NEW_QUEUE=$(curl -s "$BASE_URL/api/tutorial/queue")
        echo "Queue after delivery attempt: $NEW_QUEUE"
    else
        echo "Position 1 letter is not for Lord Aldwin (recipient: $RECIPIENT)"
    fi
else
    echo "No letter in position 1"
fi

# Kill the server
kill $SERVER_PID 2>/dev/null

echo ""
echo "=== TEST COMPLETE ==="