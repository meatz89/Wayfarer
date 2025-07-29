#!/bin/bash

# Wayfarer E2E Test Runner Script
# This script runs all E2E tests with various options

echo "=== WAYFARER E2E TEST RUNNER ==="
echo ""

# Default to running the main test runner
COMMAND="dotnet run --project /mnt/c/git/wayfarer/Wayfarer.E2ETests/Wayfarer.E2ETests.csproj -- Program=RunAllE2ETests"

# Check for specific test requests
if [ "$1" == "quick" ]; then
    echo "Running quick validation test only..."
    COMMAND="$COMMAND --quick"
elif [ "$1" == "fast" ]; then
    echo "Running fast in-process tests only..."
    COMMAND="$COMMAND --fast"
elif [ "$1" == "http" ]; then
    echo "Running HTTP endpoint tests only..."
    COMMAND="$COMMAND --http"
elif [ "$1" == "tutorial" ]; then
    echo "Running tutorial integration tests only..."
    COMMAND="$COMMAND --tutorial"
elif [ "$1" == "comprehensive" ]; then
    echo "Running ALL comprehensive tests..."
    COMMAND="$COMMAND --comprehensive --http"
elif [ "$1" == "help" ] || [ "$1" == "--help" ]; then
    echo "Usage: ./run-all-tests.sh [option]"
    echo ""
    echo "Options:"
    echo "  quick         - Run quick validation only (fastest)"
    echo "  fast          - Run fast in-process tests"
    echo "  http          - Run HTTP endpoint tests"
    echo "  tutorial      - Run tutorial integration tests"
    echo "  comprehensive - Run all tests including HTTP"
    echo "  help          - Show this help message"
    echo ""
    echo "Default (no args): Runs fast + tutorial tests"
    exit 0
else
    echo "Running default test set (fast + tutorial)..."
    # Default arguments are handled by the C# program
fi

# Create a log directory if it doesn't exist
mkdir -p /mnt/c/git/wayfarer/Wayfarer.E2ETests/logs

# Run the tests and capture output
LOG_FILE="/mnt/c/git/wayfarer/Wayfarer.E2ETests/logs/test-run-$(date +%Y%m%d-%H%M%S).log"
echo "Logging to: $LOG_FILE"
echo ""

# Execute the tests
$COMMAND 2>&1 | tee "$LOG_FILE"

# Capture exit code
EXIT_CODE=${PIPESTATUS[0]}

# Print summary
echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "✓ ALL TESTS PASSED"
else
    echo "✗ TESTS FAILED (see $LOG_FILE for details)"
fi

exit $EXIT_CODE