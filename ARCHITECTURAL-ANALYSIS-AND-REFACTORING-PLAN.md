# Wayfarer Codebase Architectural Analysis & Refactoring Plan

## Executive Summary

This document presents a comprehensive architectural analysis of the Wayfarer codebase from 10 different software architecture perspectives. Each analysis identifies specific pain points and provides concrete, actionable refactoring suggestions focused on improving game mechanics implementation, content creation efficiency, and long-term maintainability.

**Key Findings:**
- Game logic is tightly coupled with UI components
- Content pipeline lacks hot-reload and validation
- State management is distributed and inconsistent
- Event flow has unclear causality chains
- Repository pattern is incomplete and inconsistent
- Time-based systems have synchronization issues
- Player feedback mechanisms need enhancement

---

## 1. Game Systems Architect Analysis

### Current Pain Points
1. **Game Logic Mixed with UI Code** - Game rules scattered between managers, UI components, and data models
2. **Hard-coded Rules Throughout** - Game constants and rules hard-coded inline, making balance changes require code modifications
3. **Unclear Ownership of Game Mechanics** - Core mechanics split across multiple managers with circular dependencies
4. **Missing Abstractions for Complex Mechanics** - Leverage, obligations, and narrative requirements implemented ad-hoc
5. **Tight Coupling Between Systems** - Systems directly reference each other, making testing and modification difficult

### Immediate Action Items

#### 1.1 Extract Game Rules Configuration
```csharp
// Create GameConfiguration.cs
public class GameConfiguration
{
    public LetterQueueConfig LetterQueue { get; set; }
    public TokenEconomyConfig TokenEconomy { get; set; }
    public WorkRewardConfig WorkRewards { get; set; }
    public TimeConfig Time { get; set; }
}

// Load from game-config.json
{
    "letterQueue": {
        "basePositions": {
            "Noble": 3,
            "Trade": 5,
            "Shadow": 5,
            "Common": 7
        },
        "leverageMultiplier": 1,
        "skipCostPerPosition": 1
    },
    "tokenEconomy": {
        "baseTokenChance": 0.25,
        "tokenChancePerExisting": 0.05
    },
    "workRewards": {
        "default": { "coins": 4, "stamina": 1 },
        "byProfession": {
            "Merchant": { "coins": 5, "stamina": 1 },
            "TavernKeeper": { "coins": 3, "stamina": 1, "bonus": "meal" }
        }
    }
}
```

#### 1.2 Create Central Rule Engine
```csharp
public interface IGameRuleEngine
{
    bool CanPerformAction(Player player, ActionOption action);
    ActionResult CalculateActionOutcome(Player player, ActionOption action);
    int CalculateLeverage(string npcId, ConnectionType tokenType);
    QueuePosition CalculateLetterPosition(Letter letter);
}

public class GameRuleEngine : IGameRuleEngine
{
    private readonly GameConfiguration _config;
    private readonly ITokenManager _tokens;
    
    public bool CanPerformAction(Player player, ActionOption action)
    {
        var requirements = GetActionRequirements(action);
        return requirements.All(r => r.IsSatisfied(player));
    }
}
```

---

## 2. Content Pipeline Specialist Analysis

### Current Pain Points
1. **Manual Enum Parsing Everywhere** - 100+ instances of manual enum parsing with inconsistent error handling
2. **Complex Multi-Phase Loading** - 5-phase loading system that's hard to understand and debug
3. **Content Duplication** - Multiple copies of JSON files in different directories

### Immediate Action Items

#### 2.1 Implement Content Validation Pipeline
```csharp
public class ContentValidationPipeline
{
    private readonly List<IContentValidator> _validators = new()
    {
        new SchemaValidator(),
        new ReferenceValidator(),
        new BusinessRuleValidator()
    };
    
    public async Task<ValidationReport> ValidateAllContent()
    {
        var report = new ValidationReport();
        
        await Parallel.ForEachAsync(_validators, async (validator, ct) =>
        {
            var results = await validator.ValidateAsync();
            report.MergeResults(results);
        });
        
        return report;
    }
}

// Add to build process
<Target Name="ValidateContent" BeforeTargets="Build">
  <ContentValidator 
    ContentPath="$(ProjectDir)Content"
    FailOnError="true" />
</Target>
```

---

## 3. State Management Expert Analysis

### Current Pain Points
1. **Distributed Mutable State** - GameWorld acts as "god object" with 100+ properties
2. **State Synchronization Issues** - Multiple systems track overlapping state without synchronization
3. **Complex Letter Queue State** - 200+ lines of complex queue manipulation without validation
4. **UI State Mixed with Game State** - MainGameplayView mixes UI concerns with game state

### Immediate Action Items

#### 3.1 Implement State Containers
```csharp
public sealed class TimeState
{
    private int _currentHours;
    private int _currentDay;
    
    public int CurrentHours 
    { 
        get => _currentHours;
        private set 
        {
            if (value < 0 || value >= 24)
                throw new InvalidOperationException($"Invalid hour: {value}");
            _currentHours = value;
        }
    }
    
    public TimeAdvancementResult AdvanceTime(int hours)
    {
        // Atomic time advancement with validation
    }
}
```


#### 3.2 Implement Command Pattern
```csharp
public interface IGameCommand
{
    bool CanExecute(GameState state);
    CommandResult Execute(GameState state);
    void Undo(GameState state);
}

public class SpendCoinsCommand : IGameCommand
{
    private readonly int _amount;
    private int _previousCoins;
    
    public bool CanExecute(GameState state)
    {
        return state.Player.Coins >= _amount;
    }
    
    public CommandResult Execute(GameState state)
    {
        _previousCoins = state.Player.Coins;
        state.Player.Coins -= _amount;
        return CommandResult.Success();
    }
}
```

---

## 4. Player Action Architect Analysis

### Current Pain Points
1. **Unclear Action Prerequisites** - Prerequisites not communicated until execution fails
2. **Complex Action-to-Conversation Flow** - Indirect execution path through multiple managers
3. **Delayed Validation** - Choices validated after selection, not before
4. **Lost Action Context** - Original action intent lost by completion time
5. **Hidden Action Conditions** - Players don't know what unlocks new actions

### Immediate Action Items

#### 4.1 Create Unified Prerequisite System 
```csharp
public class ActionPrerequisites
{
    public List<IActionRequirement> Requirements { get; set; } = new();
    
    public ValidationResult Validate(Player player, GameWorld world)
    {
        var result = new ValidationResult();
        foreach (var req in Requirements)
        {
            if (!req.IsSatisfied(player, world))
            {
                result.AddFailure(req.GetFailureReason(), req.CanBeRemedied);
            }
        }
        return result;
    }
}

// Show prerequisites in UI
<div class="action-prerequisites">
    @foreach (var req in action.Prerequisites.Requirements)
    {
        <PrerequisiteDisplay Requirement="@req" IsMet="@req.IsSatisfied()" />
    }
</div>
```

#### 4.2 Simplify Action Execution Pipeline
```csharp
public interface IActionExecutor
{
    Task<ActionResult> Execute(ActionOption action, Player player);
}

public class UnifiedActionExecutor : IActionExecutor
{
    public async Task<ActionResult> Execute(ActionOption action, Player player)
    {
        // Validate prerequisites
        var validation = action.Prerequisites.Validate(player, _world);
        if (!validation.IsValid)
            return ActionResult.Invalid(validation);
            
        // Execute based on action type
        var executor = _executorFactory.GetExecutor(action.Type);
        return await executor.Execute(action, player);
    }
}
```

#### 4.3 Add Action Discovery System 
```csharp
public class ActionDiscovery
{
    public List<LockedAction> GetLockedActions(LocationSpot spot, Player player)
    {
        var locked = new List<LockedAction>();
        
        foreach (var potential in GetPotentialActions(spot))
        {
            if (!potential.IsAvailable(player))
            {
                locked.Add(new LockedAction
                {
                    Action = potential,
                    UnlockHint = potential.GetUnlockHint(player),
                    Progress = potential.GetUnlockProgress(player)
                });
            }
        }
        return locked;
    }
}
```

---

## 5. Narrative Systems Designer Analysis

### Current Pain Points
1. **Hard-Coded Narrative Content** - 519 lines of C# code for tutorial narrative
2. **Tight Coupling** - NarrativeManager has 17 constructor dependencies
3. **Missing State Tracking** - No persistent narrative history or journal
4. **Duplicate Conversation Patterns** - 1000+ lines of switch statements
5. **Inflexible Story Structures** - Only linear step-by-step progression

### Immediate Action Items

#### 5.1 Move Narratives to JSON
```json
// narratives/tutorial.json
{
  "id": "wayfarer_tutorial",
  "name": "From Destitute to Patronage",
  "steps": [{
    "id": "meet_tam",
    "name": "Meet Tam",
    "description": "Talk to Tam the Beggar",
    "requiredAction": "Converse",
    "requiredNPC": "tam_beggar",
    "conversationIntro": "New to the gutters? Word of advice...",
    "completionFlag": "tutorial_tam_met"
  }]
}
```

#### 5.2 Create Narrative Effect System
```csharp
public interface INarrativeEffect
{
    Task Apply(GameWorld world, Dictionary<string, object> parameters);
}

public class EffectRegistry
{
    private readonly Dictionary<string, Type> _effects = new()
    {
        ["CreateObligation"] = typeof(CreateObligationEffect),
        ["GrantItem"] = typeof(GrantItemEffect),
        ["SetFlag"] = typeof(SetFlagEffect)
    };
    
    public INarrativeEffect CreateEffect(string type, JObject parameters)
    {
        var effectType = _effects[type];
        var effect = Activator.CreateInstance(effectType) as INarrativeEffect;
        // Configure with parameters
        return effect;
    }
}
```

#### 5.3 Add Narrative Journal
```csharp
public class NarrativeJournal
{
    public List<NarrativeEvent> History { get; set; } = new();
    public Dictionary<string, int> ChoiceCounters { get; set; } = new();
    
    public void RecordChoice(string narrativeId, string stepId, string choiceId)
    {
        History.Add(new NarrativeEvent
        {
            Timestamp = DateTime.Now,
            NarrativeId = narrativeId,
            StepId = stepId,
            ChoiceId = choiceId
        });
        
        var key = $"{narrativeId}.{choiceId}";
        ChoiceCounters[key] = ChoiceCounters.GetValueOrDefault(key) + 1;
    }
    
    public bool HasMadeChoice(string narrativeId, string choiceId)
    {
        return ChoiceCounters.ContainsKey($"{narrativeId}.{choiceId}");
    }
}
```

---

## 6. UI-Game Logic Separation Specialist Analysis

### Current Pain Points
1. **Game Logic in Razor Components** - Weight calculations, time planning, NPC availability in UI
2. **Direct State Manipulation** - UI components directly modify player state
3. **Business Rules in UI** - Validation logic embedded in components
4. **Missing Presentation Models** - Domain entities used directly in UI
5. **UI Polling Game State** - Constant polling for state changes

### Immediate Action Items

#### 6.1 Create View Models
```csharp
public class LetterQueueViewModel
{
    public List<LetterViewModel> Letters { get; set; }
    public QueueStatistics Statistics { get; set; }
    public List<QueueAction> AvailableActions { get; set; }
}

public class LetterViewModel
{
    public string Id { get; set; }
    public string SenderDisplay { get; set; }
    public DeadlineStatus DeadlineStatus { get; set; }
    public string DeadlineIcon { get; set; }
    public bool CanDeliver { get; set; }
    public int SkipCost { get; set; }
}

// Service to create view models
public class ViewModelService
{
    public LetterQueueViewModel GetLetterQueueViewModel()
    {
        var letters = _letterQueueManager.GetAllLetters();
        return new LetterQueueViewModel
        {
            Letters = letters.Select(MapToViewModel).ToList(),
            Statistics = CalculateStatistics(letters),
            AvailableActions = DetermineAvailableActions()
        };
    }
}
```

#### 6.2 Extract UI Commands
```csharp
public class DeliverLetterCommand : IUICommand
{
    public string LetterId { get; set; }
    
    public async Task<UICommandResult> Execute(IGameContext context)
    {
        var result = await context.GameCommands.Execute(
            new GameDeliverLetterCommand { LetterId = LetterId }
        );
        
        return new UICommandResult
        {
            Success = result.Success,
            Message = FormatUserMessage(result),
            RefreshRequired = true
        };
    }
}

// In Razor component
@inject IUICommandDispatcher CommandDispatcher

private async Task OnDeliverClick(string letterId)
{
    var result = await CommandDispatcher.Execute(
        new DeliverLetterCommand { LetterId = letterId }
    );
    
    if (result.RefreshRequired)
        await RefreshViewModel();
}
```

#### 6.3 Implement Event-Driven Updates
```csharp
public interface IGameStateEvents
{
    event EventHandler<LetterQueueChangedEventArgs> LetterQueueChanged;
    event EventHandler<LocationChangedEventArgs> LocationChanged;
    event EventHandler<TimeBlockChangedEventArgs> TimeBlockChanged;
}

// In component
protected override void OnInitialized()
{
    _gameStateEvents.LetterQueueChanged += OnLetterQueueChanged;
}

private void OnLetterQueueChanged(object sender, LetterQueueChangedEventArgs e)
{
    InvokeAsync(async () =>
    {
        _viewModel = await _viewModelService.GetLetterQueueViewModel();
        StateHasChanged();
    });
}
```

---

## 7. Event Flow Analyst Analysis

### Current Pain Points
1. **Complex Action Chains** - Action → Conversation → Effect flow split across managers
2. **No Cascade Notification** - Systems must poll for time changes
3. **Queue Displacement Complexity** - Adding letters can trigger multiple cascading effects
4. **Order Sensitivity** - Morning activities depend on specific execution order

### Immediate Action Items

#### 7.1 Add Transaction Support
```csharp
public class GameTransaction
{
    private readonly List<IGameOperation> _operations = new();
    private readonly List<IGameOperation> _completed = new();
    
    public GameTransaction AddOperation(IGameOperation operation)
    {
        _operations.Add(operation);
        return this;
    }
    
    public TransactionResult Execute()
    {
        try
        {
            foreach (var operation in _operations)
            {
                operation.Execute();
                _completed.Add(operation);
            }
            return TransactionResult.Success();
        }
        catch (Exception ex)
        {
            // Rollback completed operations
            foreach (var operation in _completed.Reverse())
            {
                operation.Rollback();
            }
            return TransactionResult.Failure(ex);
        }
    }
}
```

#### 7.2 Create Queue Displacement Preview
```csharp
public class QueueDisplacementPlanner
{
    public DisplacementPlan PlanLetterAddition(Letter newLetter, int targetPosition)
    {
        var plan = new DisplacementPlan { NewLetter = newLetter };
        
        // Calculate all movements
        var currentQueue = _queueManager.GetCurrentQueue();
        for (int i = targetPosition; i < currentQueue.Length; i++)
        {
            if (currentQueue[i] != null)
            {
                plan.Movements.Add(new LetterMovement
                {
                    Letter = currentQueue[i],
                    FromPosition = i,
                    ToPosition = i + 1
                });
            }
        }
        
        // Identify evictions
        if (currentQueue[7] != null)
        {
            plan.Evictions.Add(currentQueue[7]);
        }
        
        return plan;
    }
}
```

---

## 8. Repository Pattern Optimizer Analysis

### Current Pain Points
1. **No Repository Interfaces** - All repositories are concrete classes
2. **Inconsistent APIs** - Different naming conventions across repositories
3. **Business Logic Leakage** - Repositories contain business rules
4. **Direct GameWorld Dependency** - Tight coupling to game state
5. **No Base Repository** - Repeated patterns across repositories

### Immediate Action Items

#### 8.1 Create Repository Interfaces
```csharp
public interface IRepository<T> where T : class
{
    T GetById(string id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    bool Remove(string id);
}

public interface IItemRepository : IRepository<Item>
{
    IEnumerable<Item> GetItemsForLocation(string locationId, string spotId = null);
    Item GetByName(string name);
}

public interface INPCRepository : IRepository<NPC>
{
    IEnumerable<NPC> GetNPCsForLocation(string locationId);
    IEnumerable<NPC> GetAvailableNPCs(TimeBlocks currentTime);
}
```

#### 8.2 Implement Base Repository
```csharp
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly IWorldStateAccessor _worldState;
    protected readonly ILogger _logger;
    
    protected BaseRepository(IWorldStateAccessor worldState, ILogger logger)
    {
        _worldState = worldState;
        _logger = logger;
    }
    
    public virtual void Add(T entity)
    {
        var collection = GetCollection();
        var entityId = GetEntityId(entity);
        
        if (collection.Any(e => GetEntityId(e) == entityId))
        {
            throw new InvalidOperationException($"Entity '{entityId}' already exists");
        }
        
        collection.Add(entity);
        _logger.LogDebug($"Added {typeof(T).Name} with ID '{entityId}'");
    }
    
    protected abstract List<T> GetCollection();
    protected abstract string GetEntityId(T entity);
}
```

#### 8.3 Extract Business Logic to Services
```csharp
// Move from NPCRepository to NPCService
public class NPCService
{
    private readonly INPCRepository _repository;
    private readonly ITimeManager _timeManager;
    
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId)
    {
        var npcs = _repository.GetNPCsForLocation(locationId);
        var plan = new List<TimeBlockServiceInfo>();
        
        foreach (var timeBlock in Enum.GetValues<TimeBlocks>())
        {
            var availableNpcs = npcs.Where(n => IsAvailable(n, timeBlock));
            plan.Add(new TimeBlockServiceInfo
            {
                TimeBlock = timeBlock,
                AvailableServices = GetServices(availableNpcs)
            });
        }
        
        return plan;
    }
}
```

---

## 9. Temporal Systems Expert Analysis

### Current Pain Points
1. **Inconsistent Time Representations** - Hours and time blocks tracked separately
2. **Missing Time Validation** - No centralized validation for action time costs
3. **Race Conditions** - Morning activities aren't atomic
4. **Unclear Dependencies** - Time blocks used inconsistently
5. **Hardcoded Constants** - Magic numbers throughout

### Immediate Action Items

#### 9.1 Create Unified Time Model 
```csharp
public class TimeModel
{
    public const int HOURS_PER_DAY = 24;
    public const int ACTIVE_DAY_START = 6;
    public const int ACTIVE_DAY_END = 22;
    
    private int _currentHour;
    private int _currentDay;
    
    public int CurrentHour => _currentHour;
    public int CurrentDay => _currentDay;
    public TimeBlock CurrentBlock => CalculateTimeBlock(_currentHour);
    public int ActiveHoursRemaining => Math.Max(0, ACTIVE_DAY_END - _currentHour);
    
    public TimeAdvancementResult AdvanceTime(int hours)
    {
        var oldHour = _currentHour;
        var oldDay = _currentDay;
        
        _currentHour += hours;
        while (_currentHour >= HOURS_PER_DAY)
        {
            _currentHour -= HOURS_PER_DAY;
            _currentDay++;
        }
        
        return new TimeAdvancementResult
        {
            HoursAdvanced = hours,
            NewDay = _currentDay > oldDay,
            NewTimeBlock = CurrentBlock
        };
    }
}
```

#### 9.2 Implement Time Transaction System
```csharp
public class TimeTransaction
{
    private readonly TimeModel _time;
    private readonly List<ITimeBasedEffect> _effects = new();
    private int _hoursCost;
    
    public TimeTransaction WithHours(int hours)
    {
        _hoursCost += hours;
        return this;
    }
    
    public TimeTransaction WithEffect(ITimeBasedEffect effect)
    {
        _effects.Add(effect);
        return this;
    }
    
    public bool CanExecute()
    {
        return _time.ActiveHoursRemaining >= _hoursCost;
    }
    
    public TimeTransactionResult Execute()
    {
        if (!CanExecute())
            return TimeTransactionResult.InsufficientTime();
            
        var advancement = _time.AdvanceTime(_hoursCost);
        var effectResults = _effects.Select(e => e.Apply(advancement)).ToList();
        
        return TimeTransactionResult.Success(advancement, effectResults);
    }
}
```

#### 9.3 Centralize Day Transition
```csharp
public class DayTransitionOrchestrator
{
    private readonly List<IDayTransitionHandler> _handlers;
    
    public DayTransitionOrchestrator(IEnumerable<IDayTransitionHandler> handlers)
    {
        _handlers = handlers.OrderBy(h => h.Priority).ToList();
    }
    
    public async Task<DayTransitionResult> ProcessNewDay()
    {
        var results = new List<HandlerResult>();
        
        foreach (var handler in _handlers)
        {
            try
            {
                var result = await handler.ProcessDayTransition();
                results.Add(result);
                
                if (result.BlocksSubsequentHandlers)
                    break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Handler {handler.GetType().Name} failed");
                // Continue with other handlers
            }
        }
        
        return new DayTransitionResult(results);
    }
}
```

---

## 10. Game Feel Engineer Analysis

### Current Pain Points
1. **Delayed Feedback** - Token spending has no preview, consequences shown after actions
2. **Weak Cause-Effect** - Players pick choices blindly without consequence previews
3. **Missing Decision Weight** - All messages look the same, major decisions vanish quickly
4. **Poor Progress Indication** - No proactive deadline warnings or pressure visualization

### Immediate Action Items

#### 10.1 Implement Action Preview System
```csharp
public class ActionPreviewService
{
    public ActionPreview GetPreview(ActionOption action, Player player)
    {
        return new ActionPreview
        {
            ActionName = action.Name,
            Costs = CalculateCosts(action, player),
            PotentialOutcomes = GetPotentialOutcomes(action),
            Risks = IdentifyRisks(action, player),
            CanAfford = CanPlayerAfford(action, player)
        };
    }
}

// In UI
<ActionPreviewPanel Action="@currentAction" Preview="@preview">
    <CostDisplay Costs="@preview.Costs" CanAfford="@preview.CanAfford" />
    <OutcomeList Outcomes="@preview.PotentialOutcomes" />
    <RiskWarnings Risks="@preview.Risks" />
</ActionPreviewPanel>
```

#### 10.2 Create Persistent Activity Feed
```csharp
public class ActivityFeed
{
    private readonly Queue<GameActivity> _recentActivities = new();
    private readonly List<GameActivity> _majorEvents = new();
    
    public void RecordActivity(GameActivity activity)
    {
        _recentActivities.Enqueue(activity);
        
        if (activity.IsMajorEvent)
        {
            _majorEvents.Add(activity);
        }
        
        ActivityRecorded?.Invoke(activity);
    }
    
    public IEnumerable<GameActivity> GetRecentActivities(int count)
    {
        return _recentActivities.TakeLast(count);
    }
    
    public IEnumerable<GameActivity> GetMajorEvents()
    {
        return _majorEvents;
    }
}
```

---

## Implementation Roadmap

###  1: Foundation 
1. **Day 1-2**: Extract game configuration and create rule engine
2. **Day 3-4**: Implement repository interfaces and base repository
3. **Day 5**: Add state invariant checking

###  2: Core Systems 
1. **Day 6-7**: Implement event bus and command pattern
2. **Day 8-9**: Create unified time model and transactions
3. **Day 10**: Add action preview system

###  3: Content & UI 
1. **Day 11-12**: Implement content validation and hot-reload
2. **Day 13-14**: Create view models and UI commands
3. **Day 15**: Move narratives to JSON

## Success Metrics

1. **Code Quality**
   - Reduce coupling between systems by 50%
   - Eliminate game logic from UI components
   - Achieve 80% test coverage for game mechanics

2. **Development Speed**
   - Content iteration time reduced from minutes to seconds
   - New feature implementation time reduced by 40%
   - Bug fix time reduced by 50%

3. **Player Experience**
   - All actions have visible prerequisites
   - Consequences previewed before commitment
   - Feedback delivered within 100ms of action

## Conclusion

These refactoring suggestions address the core architectural issues in the Wayfarer codebase while maintaining focus on practical improvements that directly benefit game development. By implementing these changes incrementally, the team can improve code quality without disrupting ongoing development.

The key is to start with the highest-impact, lowest-risk changes (configuration extraction, repository interfaces) before moving to more complex refactorings (event bus, state management). Each improvement builds on the previous ones, creating a more maintainable and extensible codebase.