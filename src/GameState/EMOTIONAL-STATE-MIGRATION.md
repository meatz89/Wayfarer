# Emotional State Calculator V2 Migration Guide

## Overview
The new `EmotionalStateCalculatorV2` replaces the simplistic deadline-only calculation with a comprehensive multi-factor analysis system.

## Key Improvements

### Old System (NPCEmotionalStateCalculator)
```csharp
// Simple decision tree based only on letters
if (deadline <= 0) return HOSTILE;
if (deadline < 2 || stakes == SAFETY) return DESPERATE;
if (hasLetters) return CALCULATING;
return WITHDRAWN;
```

### New System (EmotionalStateCalculatorV2)
```csharp
// Multi-factor weighted calculation
Factors:
- Letter Pressure (35%): Urgency, stakes, queue position
- Token Balance (25%): Debt vs surplus with NPC
- History (15%): Recent delivery failures
- Time Constraints (10%): Availability window
- Obligations (10%): Standing obligation requirements
- Social Factors (5%): Patron leverage, rivalries
```

## Migration Steps

### 1. Update Service Registration
```csharp
// In ServiceConfiguration.cs
services.AddScoped<EmotionalStateCalculatorV2>();
// Keep old one temporarily for compatibility
services.AddScoped<NPCEmotionalStateCalculator>();
```

### 2. Update GameFacade
```csharp
public class GameFacade
{
    private readonly EmotionalStateCalculatorV2 _emotionalCalculatorV2;
    
    // Add to constructor
    public GameFacade(..., EmotionalStateCalculatorV2 emotionalCalculatorV2)
    {
        _emotionalCalculatorV2 = emotionalCalculatorV2;
    }
    
    // Update method to use new calculator
    public NPCEmotionalState GetNPCEmotionalState(string npcId)
    {
        var result = _emotionalCalculatorV2.CalculateState(npcId);
        return result.State;
    }
    
    // New method to get detailed result
    public EmotionalStateResult GetNPCEmotionalStateDetailed(string npcId)
    {
        return _emotionalCalculatorV2.CalculateState(npcId);
    }
}
```

### 3. Update UI Components
```csharp
// In ConversationScreen.razor.cs
private void UpdateNPCState()
{
    var result = _gameFacade.GetNPCEmotionalStateDetailed(npcId);
    
    // Use intensity for visual effects
    var intensityClass = result.Intensity > 0.75 ? "high-intensity" : 
                         result.Intensity > 0.5 ? "medium-intensity" : "low-intensity";
    
    // Show dominant factor to player
    var factorHint = GetFactorHint(result.DominantFactor);
}
```

### 4. Cache Invalidation Points
Add cache invalidation at key state change points:

```csharp
// In LetterQueueManager
public void AddLetter(Letter letter)
{
    // ... existing code ...
    _emotionalCalculatorV2.InvalidateCache(letter.SenderId);
}

// In ConnectionTokenManager
public void AddTokensToNPC(ConnectionType type, int count, string npcId)
{
    // ... existing code ...
    _emotionalCalculatorV2.InvalidateCache(npcId);
}

// In TimeManager
public void AdvanceTime()
{
    // ... existing code ...
    _emotionalCalculatorV2.ClearCache(); // Time changes affect all NPCs
}
```

## Testing Strategy

### Unit Tests
- Test each factor calculation in isolation
- Verify threshold boundaries
- Test cache behavior
- Edge case handling (null NPCs, empty data)

### Integration Tests
1. **Overdue Letter Scenario**: Verify HOSTILE state with high intensity
2. **Safety Stakes Scenario**: Verify DESPERATE state
3. **Token Debt Scenario**: Verify increased pressure from negative tokens
4. **History Failure Scenario**: Verify impact of recent failures
5. **Combined Factors**: Test realistic multi-factor scenarios

### Performance Tests
- Cache hit rate should be > 80% during normal gameplay
- Calculation time < 10ms per NPC
- Memory usage stable with 20+ NPCs

## Rollback Plan
If issues arise, the old calculator can be restored:
1. Keep both calculators registered
2. Use feature flag to switch between them
3. Log differences for analysis

```csharp
public NPCEmotionalState GetNPCEmotionalState(string npcId)
{
    if (_featureFlags.UseNewEmotionalSystem)
    {
        return _emotionalCalculatorV2.CalculateState(npcId).State;
    }
    return _emotionalCalculator.CalculateState(_npcRepository.GetById(npcId));
}
```

## Monitoring
Log the following metrics:
- State distribution (% in each state)
- Dominant factor distribution
- Average intensity levels
- Cache hit/miss ratio
- Calculation time percentiles

## Benefits
1. **More Nuanced NPCs**: States reflect multiple pressures, not just letters
2. **Player Strategy**: Can manipulate multiple factors to influence NPCs
3. **Emergent Gameplay**: Token debt + urgent letter = interesting decisions
4. **Performance**: Caching reduces repeated calculations
5. **Debuggability**: Dominant factor helps understand NPC behavior

## Configuration Tuning
Weights can be adjusted in `EmotionalStateCalculatorV2`:
```csharp
// Adjust these based on playtesting
private const float LETTER_PRESSURE_WEIGHT = 0.35f;  // Increase for more letter-focused gameplay
private const float TOKEN_BALANCE_WEIGHT = 0.25f;    // Increase for relationship focus
private const float HISTORY_WEIGHT = 0.15f;          // Increase for consequence persistence
```