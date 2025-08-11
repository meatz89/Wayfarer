# Multi-Factor Emotional State System

## Overview
NPC emotional states emerge from multiple compounding factors, not single triggers. This creates authentic human responses where pressure accumulates, trust erodes gradually, and relationships have momentum.

## Emotional States

### DESPERATE
Emerges from cumulative pressure:
- **Immediate crisis**: Letter deadline < 2 hours with high stakes
- **Queue burial**: Their letter at position 6+ with < 12 hour deadline
- **Multiple urgencies**: 3+ letters with < 6 hour deadlines
- **Token debt**: Owing them 5+ tokens creates leverage
- **Previous failures**: Failed their letter in last 24 hours
- **Time pressure**: NPC becoming unavailable for 8+ hours

### HOSTILE  
Emerges from betrayal and broken trust:
- **Overdue letters**: Deadline passed
- **Broken obligations**: Had promise to prioritize, didn't
- **Pattern of neglect**: 2+ of their letters failed in past 3 days
- **Token exploitation**: Took tokens but didn't deliver
- **Queue manipulation**: Moved their letter deeper recently
- **Rival prioritization**: Chose competitor's letter over theirs

### CALCULATING
Emerges from balanced strategic tension:
- **Moderate pressure**: Letter in positions 3-5, deadline 6-12 hours
- **Balanced tokens**: Neither debt nor credit (near zero)
- **Mixed history**: Some successes, some failures
- **Competition**: Multiple NPCs with similar urgency
- **Information asymmetry**: They know something you need
- **Strategic position**: Their help could solve multiple problems

### WITHDRAWN
Emerges from disengagement and abandonment:
- **No investment**: No active letters AND no recent interactions (24+ hours)
- **Consistent deprioritization**: Always pushed to back of queue
- **Severed relationship**: Burned all tokens, no rebuilding
- **Broken trust**: Multiple critical failures
- **Time gaps**: Haven't seen them in 2+ days
- **Social distance**: No mutual connections active

## Implementation

```csharp
public class MultiFactorEmotionalStateCalculator
{
    public EmotionalStateResult CalculateState(NPC npc, GameContext context)
    {
        var factors = new EmotionalFactors();
        
        // Factor 1: Letter Pressure (weight: 0.30)
        factors.LetterPressure = CalculateLetterPressure(npc, context);
        
        // Factor 2: Token Relationships (weight: 0.25)
        factors.TokenBalance = CalculateTokenBalance(npc, context);
        
        // Factor 3: Interaction History (weight: 0.20)
        factors.History = CalculateHistory(npc, context);
        
        // Factor 4: Time Constraints (weight: 0.15)
        factors.TimeConstraint = CalculateTimeConstraint(npc, context);
        
        // Factor 5: Obligations (weight: 0.10)
        factors.Obligations = CalculateObligations(npc, context);
        
        // Weighted calculation
        var totalScore = factors.GetWeightedScore();
        
        // Map score to state
        return MapScoreToState(totalScore, factors);
    }
    
    private float CalculateLetterPressure(NPC npc, GameContext context)
    {
        var pressure = 0f;
        var letters = context.GetNPCLetters(npc);
        
        foreach (var letter in letters)
        {
            // Deadline pressure
            if (letter.DeadlineInHours <= 0) pressure += 1.0f;    // Overdue
            else if (letter.DeadlineInHours < 2) pressure += 0.8f; // Critical
            else if (letter.DeadlineInHours < 6) pressure += 0.5f; // Urgent
            else if (letter.DeadlineInHours < 12) pressure += 0.3f; // Pressing
            
            // Stakes multiplier
            pressure *= letter.Stakes switch
            {
                StakeType.SAFETY => 2.0f,
                StakeType.SECRET => 1.5f,
                StakeType.REPUTATION => 1.3f,
                StakeType.WEALTH => 1.1f,
                _ => 1.0f
            };
            
            // Queue position penalty
            var position = context.GetLetterPosition(letter);
            if (position > 5) pressure += 0.3f;
            else if (position > 3) pressure += 0.1f;
        }
        
        return Math.Min(pressure, 1.0f); // Normalize to 0-1
    }
}
```

## Factor Weights

| Factor | Weight | Rationale |
|--------|--------|-----------|
| Letter Pressure | 30% | Primary driver of urgency |
| Token Balance | 25% | Relationship investment |
| History | 20% | Past actions matter |
| Time Constraints | 15% | Availability pressure |
| Obligations | 10% | Promises and commitments |

## State Thresholds

| Total Score | Emotional State | Intensity |
|------------|-----------------|-----------|
| 0.75 - 1.00 | HOSTILE | High |
| 0.55 - 0.74 | DESPERATE | High |
| 0.35 - 0.54 | CALCULATING | Medium |
| 0.15 - 0.34 | WITHDRAWN | Medium |
| 0.00 - 0.14 | WITHDRAWN | Low |

## Examples

### Elena becomes DESPERATE:
```
Letter Pressure: 0.8 (SAFETY stakes, 1h deadline, position 2)
Token Balance: 0.2 (owes player 2 Trust)
History: 0.5 (1 recent success, 1 recent failure)
Time: 0.7 (becoming unavailable soon)
Obligations: 0.0 (no obligations)

Weighted Score: (0.8*0.3) + (0.2*0.25) + (0.5*0.2) + (0.7*0.15) + (0*0.1) = 0.595
Result: DESPERATE
```

### Marcus becomes HOSTILE:
```
Letter Pressure: 0.6 (overdue Commerce letter)
Token Balance: 0.8 (player owes 8 Commerce tokens)
History: 0.9 (3 recent failures)
Time: 0.3 (available all day)
Obligations: 1.0 (broken trade promise)

Weighted Score: (0.6*0.3) + (0.8*0.25) + (0.9*0.2) + (0.3*0.15) + (1.0*0.1) = 0.705
Result: HOSTILE (trending toward very hostile)
```

## Narrative Emergence

This system creates authentic emotional progressions:
- **Gradual shifts**: NPCs don't flip states instantly
- **Multiple causes**: Same state from different factor combinations
- **Recovery paths**: Clear ways to improve relationships
- **Strategic depth**: Managing multiple factors simultaneously
- **Emergent stories**: "Marcus turned hostile after I prioritized Elena's rival"

## Testing Requirements

1. **Unit tests**: Each factor calculation isolated
2. **Integration tests**: Full state calculation with mocked data
3. **Regression tests**: Known scenarios produce expected states
4. **Performance tests**: < 10ms per calculation
5. **Edge case tests**: Zero data, negative tokens, etc.

## Migration from Simple System

1. Keep old calculator as fallback
2. Run both systems in parallel, log differences
3. Tune weights based on observed patterns
4. Switch to new system when validated
5. Remove old system after stability confirmed