#!/bin/bash
# WAYFARER SMOKE TEST SUITE
# Run time: < 30 seconds
# Purpose: Catch catastrophic failures before they waste time

echo "🔥 WAYFARER SMOKE TESTS STARTING..."
echo "=================================="

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counter
TESTS_RUN=0
TESTS_PASSED=0
TESTS_FAILED=0

# Function to run a test
run_test() {
    local test_name=$1
    local test_command=$2
    
    echo -n "Testing: $test_name... "
    TESTS_RUN=$((TESTS_RUN + 1))
    
    if eval $test_command; then
        echo -e "${GREEN}✓ PASSED${NC}"
        TESTS_PASSED=$((TESTS_PASSED + 1))
    else
        echo -e "${RED}✗ FAILED${NC}"
        TESTS_FAILED=$((TESTS_FAILED + 1))
    fi
}

# Clean build first
echo "🏗️  Building project..."
cd /mnt/c/git/wayfarer/src
if dotnet build --no-incremental > /tmp/build.log 2>&1; then
    echo -e "${GREEN}Build successful${NC}"
else
    echo -e "${RED}BUILD FAILED! Check /tmp/build.log${NC}"
    exit 1
fi
echo ""

# Start server in background
echo "🚀 Starting server..."
ASPNETCORE_URLS="http://localhost:5999" timeout 10 dotnet run > /tmp/smoke-server.log 2>&1 &
SERVER_PID=$!
sleep 5  # Give server time to start

# Test 1: Server starts without errors
run_test "Server starts without infinite loop" \
    "! grep -q 'StackOverflowException' /tmp/smoke-server.log 2>/dev/null"

# Test 2: API health check
run_test "API responds to health check" \
    "curl -s http://localhost:5999/api/test/ping > /dev/null 2>&1"

# Test 3: Game initialization
run_test "Game initializes without errors" \
    "! grep -q 'INITIALIZATION FAILED' /tmp/smoke-server.log 2>/dev/null"

# Test 4: Check that server is actually running
run_test "Server process is running" \
    "ps -p $SERVER_PID > /dev/null 2>&1"

# Kill server
kill $SERVER_PID 2>/dev/null || true

echo ""
echo "=================================="
echo "SMOKE TEST RESULTS:"
echo "Tests Run: $TESTS_RUN"
echo -e "Tests Passed: ${GREEN}$TESTS_PASSED${NC}"
if [ $TESTS_FAILED -gt 0 ]; then
    echo -e "Tests Failed: ${RED}$TESTS_FAILED${NC}"
    echo ""
    echo -e "${RED}❌ SMOKE TESTS FAILED!${NC}"
    echo "DO NOT COMMIT! Fix failures first."
    exit 1
else
    echo -e "${GREEN}✅ ALL SMOKE TESTS PASSED!${NC}"
    echo "Safe to proceed with commit."
    exit 0
fi