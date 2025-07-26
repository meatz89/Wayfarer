#!/bin/bash
# Run the E2E test specifically
dotnet build --no-restore -v quiet
echo "Running E2E Test..."
dotnet bin/Debug/net8.0/Wayfarer.E2ETests.dll E2E