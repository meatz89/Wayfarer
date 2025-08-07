#!/bin/bash

echo "=== STARTING WAYFARER TEST SERVER ==="

# Kill any existing processes
pkill -f "dotnet.*Wayfarer" 2>/dev/null || true
sleep 1

# Build
echo "Building..."
dotnet build -o /tmp/wayfarer-run > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi
echo "✅ Build succeeded"

# Start server
echo "Starting server on http://localhost:5089..."
export ASPNETCORE_URLS="http://localhost:5089"
dotnet /tmp/wayfarer-run/Wayfarer.dll