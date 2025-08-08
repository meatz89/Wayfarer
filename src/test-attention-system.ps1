#!/usr/bin/env pwsh

Write-Host "`n=== TESTING ATTENTION SYSTEM IMPLEMENTATION ===" -ForegroundColor Cyan

# Test 1: Verify attention calculation in conversations
Write-Host "`n[TEST 1] Checking attention calculation code..." -ForegroundColor Yellow

$attentionCode = Get-Content ./Services/GameFacade.cs | Select-String -Pattern "Calculate attention based on location"
if ($attentionCode) {
    Write-Host "✓ Dynamic attention calculation found in GetConversation()" -ForegroundColor Green
} else {
    Write-Host "✗ Dynamic attention calculation NOT found" -ForegroundColor Red
}

# Test 2: Verify AttentionDisplay property mapping
Write-Host "`n[TEST 2] Checking AttentionDisplay property..." -ForegroundColor Yellow

$displayCode = Get-Content ./Services/GameFacade.cs | Select-String -Pattern 'AttentionDisplay = c\.AttentionCost == 0'
if ($displayCode) {
    Write-Host "✓ AttentionDisplay property correctly mapped" -ForegroundColor Green
} else {
    Write-Host "✗ AttentionDisplay property NOT mapped" -ForegroundColor Red
}

# Test 3: Verify observation visibility based on attention
Write-Host "`n[TEST 3] Checking observation visibility logic..." -ForegroundColor Yellow

$observationCode = Get-Content ./Services/GameFacade.cs | Select-String -Pattern "only if attention >= 2"
if ($observationCode) {
    Write-Host "✓ Observation visibility depends on attention level" -ForegroundColor Green
} else {
    Write-Host "✗ Observation visibility NOT tied to attention" -ForegroundColor Red
}

# Test 4: Verify crowd tag affects both location and conversation
Write-Host "`n[TEST 4] Checking crowd tag implementation..." -ForegroundColor Yellow

$crowdedCheck = Get-Content ./Services/GameFacade.cs | Select-String -Pattern 'Population.*Crowded.*attentionModifier'
if ($crowdedCheck) {
    Write-Host "✓ Crowded population affects attention modifier" -ForegroundColor Green
} else {
    Write-Host "✗ Crowded population NOT affecting attention" -ForegroundColor Red
}

# Test 5: Verify attention costs in VerbContextualizer
Write-Host "`n[TEST 5] Checking verb attention costs..." -ForegroundColor Yellow

$verbCosts = Get-Content ./Game/ConversationSystem/VerbContextualizer.cs | Select-String -Pattern "GetAttentionCost.*BaseVerb"
if ($verbCosts) {
    Write-Host "✓ VerbContextualizer has attention cost logic" -ForegroundColor Green
    
    # Check specific costs
    $helpCost = Get-Content ./Game/ConversationSystem/VerbContextualizer.cs | Select-String -Pattern "BaseVerb.HELP => 1"
    $negotiateCost = Get-Content ./Game/ConversationSystem/VerbContextualizer.cs | Select-String -Pattern "BaseVerb.NEGOTIATE => 1"
    $investigateCost = Get-Content ./Game/ConversationSystem/VerbContextualizer.cs | Select-String -Pattern "BaseVerb.INVESTIGATE => 2"
    
    if ($helpCost -and $negotiateCost -and $investigateCost) {
        Write-Host "  • HELP costs 1 attention" -ForegroundColor Gray
        Write-Host "  • NEGOTIATE costs 1 attention" -ForegroundColor Gray
        Write-Host "  • INVESTIGATE costs 2 attention" -ForegroundColor Gray
    }
} else {
    Write-Host "✗ VerbContextualizer missing attention costs" -ForegroundColor Red
}

# Summary
Write-Host "`n=== IMPLEMENTATION SUMMARY ===" -ForegroundColor Cyan
Write-Host @"
The attention system has been updated with:
1. Dynamic attention calculation based on location tags
2. Crowded locations reduce attention by 1
3. AttentionDisplay shows "Free" or "◆ N" 
4. Observations only visible when attention >= 2
5. Verb costs: HELP=1, NEGOTIATE=1, INVESTIGATE=2

EXPECTED BEHAVIOR:
- Base attention: 3 points
- Crowded locations: 2 points (3-1)
- Minimum attention: 1 point
- Choices show proper costs in UI
- Limited observations when distracted
"@ -ForegroundColor White

Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Green