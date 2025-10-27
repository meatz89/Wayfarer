# State Clearing Integration for Challenge Facades

## Overview
Challenge facades (Social, Mental, Physical) need to integrate state clearing on success/failure.

## Integration Pattern

### Location
- **SocialFacade**: `src/Subsystems/Social/SocialFacade.cs`
- **MentalFacade**: `src/Subsystems/Mental/MentalFacade.cs`
- **PhysicalFacade**: `src/Subsystems/Physical/PhysicalFacade.cs`

### Dependencies to Add
```csharp
private readonly StateClearingResolver _stateClearingResolver;

// Add to constructor parameter list
StateClearingResolver stateClearingResolver
```

### Integration Points

#### On Challenge Success
After determining challenge was successful, before returning outcome:

```csharp
// STATE CLEARING: Get projection of states to clear on challenge success
List<StateType> statesToClear = _stateClearingResolver.GetStatesToClearOnChallengeSuccess();

// Apply state clearing
foreach (StateType stateType in statesToClear)
{
    _gameWorld.ClearState(stateType);
}

// TODO Phase 6: Trigger cascade after clearing states
// if (statesToClear.Any())
// {
//     await _spawnFacade.EvaluateDormantSituations();
// }
```

#### On Challenge Failure
After determining challenge failed, before returning outcome:

```csharp
// STATE CLEARING: Get projection of states to clear on challenge failure
List<StateType> statesToClear = _stateClearingResolver.GetStatesToClearOnChallengeFailure();

// Apply state clearing
foreach (StateType stateType in statesToClear)
{
    _gameWorld.ClearState(stateType);
}

// TODO Phase 6: Trigger cascade
// if (statesToClear.Any())
// {
//     await _spawnFacade.EvaluateDormantSituations();
// }
```

## Finding Success/Failure Points

### SocialFacade
- Method: `EndConversation()` or `FinalizeConversation()`
- Success determined by: Connection state, Understanding threshold, or other criteria
- Check `SocialChallengeOutcome` for success/failure indication

### MentalFacade
- Method: Look for completion/finalize methods
- Success determined by: Clue discovery, investigation completion
- Check outcome object for success/failure

### PhysicalFacade
- Method: Look for completion/finalize methods
- Success determined by: Obstacle overcome, progress threshold met
- Check outcome object for success/failure

## Using Statements Needed
```csharp
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;
```

## Notes
- This integration requires identifying where each challenge type determines success/failure
- Current implementation uses projection pattern (resolver returns what SHOULD clear, facade applies)
- State clearing should happen AFTER challenge outcome is determined but BEFORE returning to caller
- Phase 6 will add SpawnFacade.EvaluateDormantSituations cascade triggering
