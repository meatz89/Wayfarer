// Test script for letter generation thresholds and deadlines
// This test verifies that letters are generated with correct thresholds and deadlines

console.log("Starting Letter Generation Test...");

// First, run the server in the background
console.log("Starting the game server...");

// Wait for server to start
setTimeout(() => {
    console.log("Server should be running on http://localhost:5000");
    console.log("Please use Playwright tools to:");
    console.log("1. Navigate to http://localhost:5000");
    console.log("2. Start a conversation with an NPC");
    console.log("3. Build comfort to various levels (5, 10, 15, 20)");
    console.log("4. Check if letters are generated with correct deadlines:");
    console.log("   - 5-9 comfort: 24h deadline, 5 coins");
    console.log("   - 10-14 comfort: 12h deadline, 10 coins");
    console.log("   - 15-19 comfort: 6h deadline, 15 coins");
    console.log("   - 20+ comfort: 2h deadline, 20 coins");
    console.log("5. Verify letters appear in the obligation queue");
}, 3000);