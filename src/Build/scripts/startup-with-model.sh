#!/bin/bash
# Check if model already exists
MODEL_DIR="/root/.ollama/models"
MODEL_NAME="gemma3:12b-it-qat"

if [ ! -d "$MODEL_DIR/$MODEL_NAME" ]; then
  echo "Model not found, starting Ollama and pulling model..."
  # Start Ollama in the background
  ollama serve &
  SERVER_PID=$!
  
  # Wait for Ollama to start
  echo "Waiting for Ollama server to start..."
  sleep 10
  
  # Pull the model
  echo "Pulling $MODEL_NAME model..."
  ollama pull $MODEL_NAME
  
  # Stop the Ollama server
  echo "Model downloaded, restarting Ollama..."
  kill $SERVER_PID
  wait $SERVER_PID
fi

# Start Ollama in the foreground
echo "Starting Ollama server with pre-pulled model..."
exec ollama serve