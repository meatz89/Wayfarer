#!/bin/bash

echo "==============================================="
echo "WAYFARER TUTORIAL COMPREHENSIVE TEST SUITE"
echo "==============================================="
echo ""
echo "This test suite validates:"
echo "- All 57 tutorial steps"
echo "- Action restrictions at each step"
echo "- UI element visibility controls"
echo "- Dialogue override system"
echo "- Tutorial progression and save/load"
echo "- Tutorial completion and content unlock"
echo "- Edge cases and error recovery"
echo ""
echo "Starting tests..."
echo ""

# Build the E2E test project
echo "Building E2E test project..."
cd /mnt/c/git/wayfarer/Wayfarer.E2ETests
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "❌ Build failed!"
    exit 1
fi

echo ""
echo "Running comprehensive tutorial tests..."
echo ""

# Run the main comprehensive test
dotnet run --project /mnt/c/git/wayfarer/Wayfarer.E2ETests/Wayfarer.E2ETests.csproj -- comprehensive-tutorial

MAIN_RESULT=$?

echo ""
echo "Running edge case tests..."
echo ""

# Run edge case tests
dotnet run --project /mnt/c/git/wayfarer/Wayfarer.E2ETests/Wayfarer.E2ETests.csproj -- tutorial-edge-cases

EDGE_RESULT=$?

echo ""
echo "==============================================="
echo "TEST SUMMARY"
echo "==============================================="

if [ $MAIN_RESULT -eq 0 ] && [ $EDGE_RESULT -eq 0 ]; then
    echo "✅ ALL TESTS PASSED!"
    echo ""
    echo "The tutorial system is working correctly:"
    echo "- Auto-start on new game ✓"
    echo "- Narrative overlay displays properly ✓"
    echo "- Command filtering enforced ✓"
    echo "- All 57 steps can be completed ✓"
    echo "- Save/load preserves tutorial state ✓"
    echo "- Tutorial completion unlocks full game ✓"
    echo "- Edge cases handled gracefully ✓"
    exit 0
else
    echo "❌ SOME TESTS FAILED!"
    echo ""
    if [ $MAIN_RESULT -ne 0 ]; then
        echo "- Main tutorial tests: FAILED"
    else
        echo "- Main tutorial tests: PASSED"
    fi
    
    if [ $EDGE_RESULT -ne 0 ]; then
        echo "- Edge case tests: FAILED"
    else
        echo "- Edge case tests: PASSED"
    fi
    
    echo ""
    echo "Please review the test output above for details."
    exit 1
fi