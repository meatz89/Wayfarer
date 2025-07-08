#!/bin/bash
# Start Ollama in the background
ollama serve &
SERVER_PID=$!

# Wait for Ollama to start
echo "Waiting for Ollama server to start..."
sleep 10

# Attempt to pull the model
echo "Pulling gemma3:12b-it-qat model..."
ollama pull gemma3:12b-it-qat

# Stop the Ollama server
echo "Stopping Ollama server..."
kill $SERVER_PID
wait $SERVER_PID

echo "Model preparation complete."