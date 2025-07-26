# Dynamic Obligation Scaling Implementation Plan

## Overview
Implement dynamic obligation scaling based on token levels, where debt obligations scale with negative token levels to create natural difficulty curves through player choice.

## Design Goals
1. **Dynamic Effect Strength**: Effects scale mathematically with token levels
2. **No Special Rules**: Use mathematical scaling, not conditional logic
3. **Clear Cause & Effect**: Players understand how debt depth affects leverage
4. **Emergent Difficulty**: Harder gameplay emerges from player choices

## Target Examples
### Patron Debt Scaling
- At -1 to -2 tokens: "Patron's Expectation" (letters enter at position 3)
- At -3 to -4 tokens: "Patron's Leverage" (letters enter at position 2) 
- At -5+ tokens: "Patron's Heavy Hand" (letters enter at position 1, cannot refuse)

### Merchant Leverage Scaling
- At -1 token: Letters enter at position 5
- At -2 tokens: Letters enter at position 4
- At -3+ tokens: Letters enter at position 3

### Positive Token Scaling
- At 5+ tokens: "Elena's Devotion" (Trust letters get +2 days deadline)
- At 10+ tokens: Enhanced benefits scale with relationship strength

## Implementation Approach

### 1. Refactor Obligation System
**Current State**: 
- Static effects once activated (e.g., PatronLettersPosition3)
- Threshold-based on/off activation

**Target State**:
- Dynamic position calculation based on token level
- Graduated effects that scale smoothly
- Single obligation with multiple strength levels

### 2. Create Scaling Functions
```csharp
public class ObligationScaler
{
    // Calculate leverage position based on debt depth
    public int CalculateLeveragePosition(int basePosition, int tokenBalance)
    {
        if (tokenBalance >= 0) return basePosition;
        
        // Each negative token moves position up by 1
        // But cap at position 1 to avoid going off queue
        return Math.Max(1, basePosition + tokenBalance);
    }
    
    // Calculate constraint strength
    public bool CanRefuseLetter(int tokenBalance, int refusalThreshold = -5)
    {
        // Can refuse until debt gets too deep
        return tokenBalance > refusalThreshold;
    }
}
```

### 3. Replace Static Effects
**Remove**:
- `PatronLettersPosition1`
- `PatronLettersPosition3` 
- `NoblesPriority`
- `CommonFolksPriority`

**Add**:
- `DynamicLeverage` - Scales position with token balance
- `DebtConstraints` - Scales restrictions with debt depth
- `RelationshipBenefits` - Scales benefits with positive tokens

### 4. Update Obligation Processing
Modify `StandingObligationManager.ApplyLeverageModifiers()` to:
1. Get current token balance with relevant NPC
2. Apply mathematical scaling based on balance
3. Return dynamically calculated position/effects

### 5. UI Feedback System
Show players how obligations scale:
- "Patron's Leverage: -3 tokens (Position 2)"
- "As debt deepens, leverage increases"
- Visual indicators when crossing thresholds

## Implementation Steps

### Phase 1: Core Scaling System (2-3 days)
1. Create `ObligationScaler` class with scaling functions
2. Add `DynamicScalingEffect` enum values
3. Update `StandingObligation` to support dynamic calculations
4. Create unit tests for scaling math

### Phase 2: Refactor Existing Obligations (2-3 days)
1. Update obligation JSON files to use dynamic effects
2. Modify `CalculateEntryPosition()` to use scaler
3. Update `IsForbiddenAction()` to check debt thresholds
4. Remove static position effects

### Phase 3: Integration & Testing (2-3 days)
1. Update `LetterQueueManager` leverage calculations
2. Ensure obligation manager properly tracks token changes
3. Test full flow from token change to position update
4. Verify save/load preserves dynamic state

### Phase 4: UI & Feedback (1-2 days)
1. Update obligation UI to show current scaling
2. Add tooltips explaining the scaling system
3. Show leverage changes when tokens change
4. Clear messaging in letter queue

## Technical Considerations

### Breaking Changes
- Existing saves with static obligations need migration
- JSON content files need updating
- Unit tests for static effects need rewriting

### Performance
- Scaling calculations are lightweight (simple math)
- Cache calculations per turn to avoid redundancy
- No complex state tracking needed

### Edge Cases
- Handle obligations with no NPC (general obligations)
- Cap scaling to prevent invalid queue positions
- Smooth transitions when tokens change rapidly

## Success Criteria
1. ✅ Patron letters dynamically scale position based on debt
2. ✅ Constraints (like refusal) activate at debt thresholds
3. ✅ Benefits scale with positive token levels
4. ✅ Players understand the system through play
5. ✅ No special rules - all emergent from math

## Future Extensions
- Compound scaling (debt + relationship history)
- Time-based scaling (obligations get stronger over time)
- Cross-token scaling (shadow debt affects noble letters)
- Obligation "interest" (debt obligations worsen daily)