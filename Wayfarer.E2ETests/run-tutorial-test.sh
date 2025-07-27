#!/bin/bash
# Run the tutorial test by compiling and executing it directly

echo "=== RUNNING TUTORIAL INTEGRATION TEST ==="

# Compile the test
dotnet build

# Run the tutorial test class directly
dotnet exec bin/Debug/net8.0/Wayfarer.E2ETests.dll TutorialTest