#!/bin/bash
# Fast E2E Test Runner - Runs the FastE2ETestSuite with clear output

echo "======================================"
echo "Running Fast E2E Test Suite"
echo "======================================"
echo ""

# Run the fast tests
dotnet run -- Program=FastE2ETestSuite

# Check exit code
if [ $? -eq 0 ]; then
    echo ""
    echo "======================================"
    echo "✅ ALL TESTS PASSED"
    echo "======================================"
else
    echo ""
    echo "======================================"
    echo "❌ TESTS FAILED"
    echo "======================================"
    echo ""
    echo "Note: If tests failed due to tutorial restrictions,"
    echo "this is expected behavior. The tests will pass"
    echo "normally after tutorial completion."
fi