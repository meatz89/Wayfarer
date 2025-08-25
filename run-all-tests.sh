#!/bin/bash
# WAYFARER COMPLETE TEST SUITE
# Runs ALL tests before allowing any release

set -e

echo "🧪 WAYFARER COMPLETE TEST SUITE"
echo "================================"
echo ""

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Results tracking
TOTAL_TESTS=0
TOTAL_PASSED=0
TOTAL_FAILED=0
FAILED_SUITES=""

run_suite() {
    local suite_name=$1
    local suite_command=$2
    
    echo -e "${BLUE}Running: $suite_name${NC}"
    echo "----------------------------"
    
    if eval $suite_command; then
        echo -e "${GREEN}✅ $suite_name PASSED${NC}\n"
        TOTAL_PASSED=$((TOTAL_PASSED + 1))
    else
        echo -e "${RED}❌ $suite_name FAILED${NC}\n"
        TOTAL_FAILED=$((TOTAL_FAILED + 1))
        FAILED_SUITES="$FAILED_SUITES\n  - $suite_name"
    fi
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
}

# Clean build first
echo "🏗️  Clean building project..."
cd /mnt/c/git/wayfarer/src
dotnet clean > /dev/null 2>&1
dotnet build --no-incremental || {
    echo -e "${RED}BUILD FAILED! Cannot run tests.${NC}"
    exit 1
}
echo -e "${GREEN}Build successful${NC}\n"

# 1. SMOKE TESTS
run_suite "Smoke Tests" "./run-smoke-tests.sh"

# 2. UNIT TESTS
run_suite "Unit Tests" "dotnet test --filter Category=Unit --no-build"

# 3. E2E CONVERSATION TESTS
echo "🚀 Starting test server for E2E tests..."
ASPNETCORE_URLS="http://localhost:5999" dotnet run > /tmp/e2e-server.log 2>&1 &
SERVER_PID=$!
sleep 5  # Give server time to fully start

run_suite "Conversation E2E Tests" "dotnet test --filter FullyQualifiedName~ConversationTests --no-build"

run_suite "Obligation Queue E2E Tests" "dotnet test --filter FullyQualifiedName~ObligationQueueTests --no-build"

run_suite "Resource System E2E Tests" "dotnet test --filter FullyQualifiedName~ResourceTests --no-build"

run_suite "Travel System E2E Tests" "dotnet test --filter FullyQualifiedName~TravelTests --no-build"

# Kill test server
kill $SERVER_PID 2>/dev/null || true

# 4. POC FLOW TEST
run_suite "POC Complete Flow" "./test-poc-flow.sh"

# 5. VISUAL REGRESSION TESTS (if baselines exist)
if [ -d "./visual-baselines" ]; then
    run_suite "Visual Regression Tests" "./run-visual-tests.sh"
else
    echo -e "${YELLOW}⚠️  Visual baselines not found, skipping visual tests${NC}\n"
fi

# FINAL RESULTS
echo ""
echo "================================"
echo -e "${BLUE}FINAL TEST RESULTS${NC}"
echo "================================"
echo "Total Test Suites: $TOTAL_TESTS"
echo -e "Passed: ${GREEN}$TOTAL_PASSED${NC}"
echo -e "Failed: ${RED}$TOTAL_FAILED${NC}"

if [ $TOTAL_FAILED -gt 0 ]; then
    echo ""
    echo -e "${RED}❌ TEST SUITE FAILED!${NC}"
    echo -e "Failed suites:$FAILED_SUITES"
    echo ""
    echo "DO NOT RELEASE! Fix all failures first."
    exit 1
else
    echo ""
    echo -e "${GREEN}✅ ALL TESTS PASSED!${NC}"
    echo "Safe to release."
    exit 0
fi