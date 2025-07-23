# Temporal Systems Migration Guide

## Overview

This guide explains how to migrate from the current distributed time management system to the new unified temporal system.

## Architecture Changes

### Before (Current System)
- Time tracked in multiple places: `TimeManager`, `GameWorld`, `WorldState`
- No atomic time operations
- Day transitions scattered across multiple managers
- Time synchronization issues between systems

### After (Unified System)
- Single source of truth: `TimeModel` with immutable `TimeState`
- Atomic time operations via `TimeTransaction`
- Centralized day transitions via `DayTransitionOrchestrator`
- Event-driven synchronization

## Migration Steps

### Step 1: Add Unified Time System to DI Container

```csharp
// In ServiceConfiguration.cs or Program.cs
services.AddUnifiedTimeSystem();
```

### Step 2: Update Time-Dependent Actions

Replace direct time manipulation:

```csharp
// OLD
if (timeManager.CanPerformAction(2))
{
    timeManager.SpendHours(2);
    player.Stamina -= 1;
    player.CurrentLocation = newLocation;
}

// NEW
var transaction = unifiedTimeService.CreateTransaction()
    .WithHours(2, "Travel")
    .WithEffect(new StaminaCostEffect(player, 1))
    .WithEffect(new LocationChangeEffect(player, newLocation));

var result = transaction.Execute();
if (!result.Success)
{
    // Handle failure
}
```

### Step 3: Update Day Transitions

Replace scattered day transition logic:

```csharp
// OLD (in GameWorldManager)
public async Task StartNewDay()
{
    ProcessDailyLetterQueue();
    morningActivitiesManager.ProcessMorningActivities();
    scenarioManager.CheckScenarioProgress();
}

// NEW
// Day transitions are now handled automatically by DayTransitionOrchestrator
// Just call:
await unifiedTimeService.AdvanceToNextDay();
```

### Step 4: Create Custom Day Transition Handlers

For any system that needs day transition logic:

```csharp
public class MySystemDayHandler : IDayTransitionHandler
{
    public int Priority => 150; // Determines order
    public bool IsCritical => false;
    public string Description => "Process my system's daily tasks";

    public async Task<HandlerResult> ProcessDayTransition(DayTransitionContext context)
    {
        // Your day transition logic here
        return new HandlerResult
        {
            Success = true,
            Message = "Processed successfully"
        };
    }
}

// Register in DI
services.AddTransient<IDayTransitionHandler, MySystemDayHandler>();
```

### Step 5: Update Time Queries

Replace direct time access:

```csharp
// OLD
var currentHour = timeManager.CurrentTimeHours;
var timeBlock = timeManager.GetCurrentTimeBlock();
var canAct = timeManager.CanPerformAction(3);

// NEW
var currentHour = unifiedTimeService.CurrentHour;
var timeBlock = unifiedTimeService.CurrentTimeBlock;
var canAct = unifiedTimeService.CanPerformAction(3);
```

## Common Patterns

### Simple Time Action
```csharp
// Wait or simple time passage
await unifiedTimeService.Wait(2);
await unifiedTimeService.SpendTime(1, "Rest");
```

### Complex Action with Multiple Effects
```csharp
var transaction = unifiedTimeService.CreateTransaction()
    .WithHours(3, "Complex task")
    .WithEffect(new StaminaCostEffect(player, 2))
    .WithEffect(new TokenGainEffect(player, npcId, 1))
    .WithEffect(new MoneyEarnedEffect(player, 5));

if (transaction.CanExecute().IsValid)
{
    var result = transaction.Execute();
    // Handle result
}
```

### Travel Action
```csharp
await unifiedTimeService.Travel(player, destination, travelHours, staminaCost);
```

## Testing

### Unit Testing Time Operations
```csharp
[Test]
public void TimeTransaction_RollsBackOnFailure()
{
    var timeModel = new TimeModel(1, 10);
    var player = new Player { Stamina = 1 };
    
    var transaction = new TimeTransaction(timeModel)
        .WithHours(2)
        .WithEffect(new StaminaCostEffect(player, 5)); // Will fail
    
    var result = transaction.Execute();
    
    Assert.IsFalse(result.Success);
    Assert.AreEqual(10, timeModel.CurrentHour); // Time not advanced
    Assert.AreEqual(1, player.Stamina); // Stamina unchanged
}
```

### Testing Day Transitions
```csharp
[Test]
public async Task DayTransition_ExecutesHandlersInOrder()
{
    var handlers = new[]
    {
        new TestHandler(priority: 100),
        new TestHandler(priority: 50),
        new TestHandler(priority: 200)
    };
    
    var orchestrator = new DayTransitionOrchestrator(handlers, timeModel, logger);
    var result = await orchestrator.ProcessNewDay();
    
    // Verify handlers executed in priority order: 50, 100, 200
}
```

## Backward Compatibility

The `TimeManagerRefactoringAdapter` provides backward compatibility:

```csharp
// Old code continues to work
ITimeManager timeManager = serviceProvider.GetService<ITimeManager>();
timeManager.SpendHours(2); // Delegates to UnifiedTimeService
```

## Benefits

1. **Atomic Operations**: All time-related changes happen together or not at all
2. **Centralized Logic**: Day transitions in one place
3. **Better Testing**: Immutable state and clear transactions
4. **Event-Driven**: Systems react to time changes instead of polling
5. **Extensible**: Easy to add new effects and handlers

## Troubleshooting

### Time Synchronization Issues
If you see time mismatches:
1. Ensure only `UnifiedTimeService` modifies time
2. Check that all systems subscribe to time events
3. Verify `WorldState` sync in the adapter

### Day Transition Failures
If day transitions fail:
1. Check handler priorities for correct order
2. Verify critical handlers aren't failing
3. Review logs for specific handler errors

### Transaction Rollback Issues
If rollbacks aren't working:
1. Ensure effects implement `Rollback` correctly
2. Store original state before modifications
3. Test rollback paths explicitly