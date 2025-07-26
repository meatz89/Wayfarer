# Dynamic Obligation Scaling - Implementation Progress

## Current State Analysis (Completed)

### ✅ Examined Core Systems
1. **StandingObligation.cs**
   - Currently uses static effects (PatronLettersPosition1, PatronLettersPosition3)
   - Threshold-based activation/deactivation
   - Effects are boolean - either on or off
   - No dynamic scaling based on token levels

2. **LetterQueueManager.cs**
   - `CalculateLeveragePosition()` already has some scaling:
     - Debt creates leverage (each negative token moves position up)
     - Extreme debt (-10 tokens) pushes to position 1
   - This is good! We can build on this pattern

3. **StandingObligationManager.cs**
   - `ApplyLeverageModifiers()` applies static modifiers
   - Already handles some dynamic cases (DebtSpiral, MerchantRespect)
   - Has framework for checking token balances

### 💡 Key Findings
- The leverage system already exists in `LetterQueueManager`!
- We have mathematical scaling for debt → position
- The obligation system just needs to enhance this base calculation
- No need to completely rewrite - extend what's there

## Implementation Tasks

### 🔄 Phase 1: Extend Existing Scaling (In Progress)
- [ ] Create `DynamicObligationEffect` enum values
- [ ] Add scaling calculation methods to `StandingObligation`
- [ ] Update obligation JSON to use dynamic thresholds
- [ ] Modify `ApplyLeverageModifiers` to return scaling factors

### ⏳ Phase 2: Refactor Static Effects (Not Started)
- [ ] Replace PatronLettersPosition1/3 with DynamicPatronLeverage
- [ ] Convert NoblesPriority to scale with token balance
- [ ] Update obligation validation to handle dynamic effects
- [ ] Migrate existing game saves

### ⏳ Phase 3: Enhanced Feedback (Not Started)
- [ ] Show scaling strength in obligation UI
- [ ] Add "leverage meter" to letter queue
- [ ] Display threshold warnings ("One more debt = cannot refuse!")
- [ ] Narrative messages for crossing thresholds

### ⏳ Phase 4: Testing & Balance (Not Started)
- [ ] Unit tests for scaling calculations
- [ ] E2E tests for obligation → position flow
- [ ] Balance testing for difficulty curve
- [ ] Player feedback on clarity

## Code Snippets Identified

### Existing Leverage Calculation (Good Foundation!)
```csharp
// From LetterQueueManager.CalculateLeveragePosition()
if (tokenBalance < 0)
{
    // Debt creates leverage - each negative token moves position up
    leveragePosition += tokenBalance; // Subtracts since negative
    
    // Extreme debt (patron-level) can push to position 1
    if (tokenBalance <= -10)
    {
        leveragePosition = Math.Max(1, leveragePosition);
    }
}
```

### Areas Needing Enhancement
1. **Static Position Setting**:
   ```csharp
   // Current - static positions
   if (HasEffect(ObligationEffect.PatronLettersPosition1))
   {
       return 1; // Force to position 1
   }
   ```
   
2. **Binary Constraints**:
   ```csharp
   // Current - on/off refusal
   if (HasEffect(ObligationEffect.CannotRefuseLetters))
   {
       return true; // Cannot refuse any letters
   }
   ```

## Next Steps

### Immediate Actions (Today)
1. Design the new obligation effect structure
2. Create scaling helper methods
3. Update patron obligations in JSON

### Tomorrow
1. Implement dynamic calculation in obligation manager
2. Test with patron debt scaling
3. Update UI to show current leverage

### This Week
1. Complete all static → dynamic conversions
2. Add comprehensive tests
3. Update documentation
4. Player testing session

## Design Decisions Made

### ✅ Build on Existing System
Rather than replacing the leverage system, we'll enhance it:
- Keep base leverage calculation in LetterQueueManager
- Obligations modify the calculation with multipliers/offsets
- Preserves existing game balance while adding depth

### ✅ Mathematical Scaling Pattern
```
Position = BasePosition + (TokenBalance * ScalingFactor) + ObligationModifiers
```
Where:
- BasePosition = social class position (Noble=3, Trade=5, etc.)
- TokenBalance = negative for debt, positive for favor
- ScalingFactor = how much each token affects position
- ObligationModifiers = additional effects from active obligations

### ✅ Threshold Messaging
Show players the edges:
- "-2 tokens: Letters enter at position 3"
- "-3 tokens: WARNING - One more debt = cannot refuse!"
- "-4 tokens: CANNOT REFUSE patron letters"

## Risks & Mitigations

### Risk: Breaking Existing Saves
**Mitigation**: Migration code to convert static obligations to dynamic

### Risk: Unclear to Players
**Mitigation**: Clear UI showing current scaling and next threshold

### Risk: Unbalanced Difficulty
**Mitigation**: Extensive playtesting with different debt levels

## Questions to Resolve

1. Should positive tokens also scale benefits dynamically?
   - **Decision**: Yes, but more gradually than debt
   
2. How to handle obligations without specific NPCs?
   - **Decision**: Use total tokens of type instead of NPC-specific
   
3. Should scaling be linear or curved?
   - **Decision**: Start linear, adjust based on playtesting