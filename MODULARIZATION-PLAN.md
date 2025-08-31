# Wayfarer Codebase Modularization Plan

## Executive Summary

This document outlines the complete refactoring strategy to transform Wayfarer's monolithic codebase into a modular, maintainable architecture. The refactoring will be done incrementally without breaking existing functionality.

**Key Problems:**
- GameFacade.cs: 3,448 lines with 30+ dependencies
- ObligationQueueManager: 2,819 lines mixing multiple concerns
- Large UI components with 1,700+ lines
- Tight coupling between unrelated systems

**Solution:** Subsystem-based architecture with focused facades, maintaining GameWorld as single source of truth.

## Core Principles

1. **NO INTERFACES/ABSTRACTS** - Use concrete classes only
2. **GameWorld Immutable** - Remains single source of truth, never modified
3. **NO FALLBACKS** - Target architecture immediately, no compatibility layers
4. **NO TODOS** - Complete implementation only, no placeholders
5. **NO INTERMEDIARY STEPS** - Direct migration to final architecture
6. **Single Responsibility** - Each subsystem has ONE clear purpose
7. **No Shared State** - Subsystems communicate through GameWorld only
8. **COMPLETE MIGRATION** - When refactoring, move ALL related code at once

## Architecture Overview

```
src/
├── Subsystems/
│   ├── Location/
│   │   ├── LocationFacade.cs          [Public API for location operations]
│   │   ├── LocationManager.cs         [Core location logic]
│   │   ├── LocationSpotManager.cs     [Spot-specific operations]
│   │   ├── MovementValidator.cs       [Movement rules and validation]
│   │   └── NPCLocationTracker.cs      [NPC position tracking]
│   │
│   ├── Conversation/
│   │   ├── ConversationFacade.cs      [Public API for conversations]
│   │   ├── ConversationOrchestrator.cs [Conversation flow control]
│   │   ├── CardDeckManager.cs         [Deck operations]
│   │   ├── EmotionalStateManager.cs   [State transitions]
│   │   ├── ComfortManager.cs          [Comfort battery system]
│   │   └── DialogueGenerator.cs       [Narrative generation]
│   │
│   ├── Obligation/
│   │   ├── ObligationFacade.cs        [Public API for obligations]
│   │   ├── DeliveryManager.cs         [Letter deliveries only]
│   │   ├── MeetingManager.cs          [Meeting obligations only]
│   │   ├── QueueManipulator.cs        [Queue operations]
│   │   ├── DeadlineTracker.cs         [Deadline management]
│   │   └── DisplacementCalculator.cs  [Token burn calculations]
│   │
│   ├── Travel/
│   │   ├── TravelFacade.cs            [Public API for travel]
│   │   ├── RouteManager.cs            [Route operations]
│   │   ├── RouteDiscoveryManager.cs   [Discovery mechanics]
│   │   ├── PermitValidator.cs         [Access permit checking]
│   │   └── TravelTimeCalculator.cs    [Time cost calculations]
│   │
│   ├── Resource/
│   │   ├── ResourceFacade.cs          [Public API for resources]
│   │   ├── CoinManager.cs             [Coin transactions]
│   │   ├── HealthManager.cs           [Health operations]
│   │   ├── HungerManager.cs           [Hunger mechanics]
│   │   ├── AttentionManager.cs        [Attention allocation]
│   │   └── ResourceCalculator.cs      [Resource formulas]
│   │
│   ├── Time/
│   │   ├── TimeFacade.cs              [Public API for time]
│   │   ├── TimeManager.cs             [Core time operations]
│   │   ├── TimeBlockManager.cs        [Period management]
│   │   ├── WeatherManager.cs          [Weather system]
│   │   └── TimeEventScheduler.cs      [Time-based events]
│   │
│   ├── Market/
│   │   ├── MarketFacade.cs            [Public API for markets]
│   │   ├── MarketManager.cs           [Core market logic]
│   │   ├── ArbitrageCalculator.cs     [Arbitrage opportunities]
│   │   ├── PriceManager.cs            [Price calculations]
│   │   └── MarketStateTracker.cs      [Market conditions]
│   │
│   ├── Token/
│   │   ├── TokenFacade.cs             [Public API for tokens]
│   │   ├── ConnectionTokenManager.cs   [Token operations]
│   │   ├── TokenEffectProcessor.cs    [Token effect calculations]
│   │   ├── TokenUnlockManager.cs      [Unlock mechanics]
│   │   └── RelationshipTracker.cs     [Relationship state]
│   │
│   └── Narrative/
│       ├── NarrativeFacade.cs         [Public API for narrative]
│       ├── ObservationManager.cs      [Observation system]
│       ├── MessageSystem.cs           [Message handling]
│       ├── NarrativeRenderer.cs       [Story generation]
│       └── EventNarrator.cs           [Event descriptions]
```

## Phase 1: Foundation (Week 1)

### Work Packet 1.1: Complete LocationSubsystem Implementation
**Owner:** Systems Architect Agent
**Dependencies:** None
**Deliverables:**
- COMPLETE LocationSubsystem with ALL components
- ALL location code migrated from GameFacade
- NO placeholders, NO TODOs
- Fully functional from day one

**Detailed Steps:**
1. Create `/src/Subsystems/Location/` directory
2. Create COMPLETE LocationFacade.cs with ALL methods
3. Create COMPLETE LocationManager.cs with ALL logic
4. Create COMPLETE LocationSpotManager.cs with ALL spot operations
5. Create COMPLETE MovementValidator.cs with ALL validation rules
6. Create COMPLETE NPCLocationTracker.cs with ALL tracking logic
7. Migrate ALL location code from GameFacade IMMEDIATELY
8. Update GameFacade to use LocationFacade COMPLETELY
9. NO partial implementations allowed

**Validation:**
- ALL location operations work through LocationFacade
- GameFacade COMPLETELY delegates to LocationFacade
- UI components function perfectly
- NO legacy code remains

### Work Packet 1.2: Complete ConversationSubsystem Implementation
**Owner:** Systems Architect Agent
**Dependencies:** None (parallel execution)
**Deliverables:**
- COMPLETE ConversationSubsystem with ALL components
- ALL conversation code migrated from GameFacade and ConversationManager
- NO partial implementations
- Fully functional conversation system

**Detailed Steps:**
1. Create `/src/Subsystems/Conversation/` directory
2. Create COMPLETE ConversationFacade.cs with ALL conversation methods
3. Create COMPLETE ConversationOrchestrator.cs migrating ALL logic from ConversationManager
4. Create COMPLETE CardDeckManager.cs with ALL deck operations
5. Create COMPLETE EmotionalStateManager.cs with ALL state logic
6. Create COMPLETE ComfortManager.cs with ALL comfort calculations
7. Create COMPLETE DialogueGenerator.cs with ALL narrative generation
8. Migrate ALL 833 lines from ConversationManager.cs
9. Migrate ALL 1,348 lines from ConversationSession.cs
10. DELETE old ConversationManager after migration
11. Update GameFacade to use ConversationFacade COMPLETELY

**Validation:**
- ALL conversation mechanics work perfectly
- NO old conversation code remains
- Card play mechanics fully functional
- State transitions correct
- UI components work without modification

## Phase 2: Core Subsystems (Parallel Execution)

### Work Packet 2.1: Complete ObligationSubsystem Implementation
**Owner:** Systems Architect Agent  
**Dependencies:** None (parallel execution)
**Deliverables:**
- COMPLETE ObligationSubsystem with ALL components
- ALL 2,819 lines from ObligationQueueManager migrated and DELETED
- Five new managers fully implemented
- GameFacade updated to use ObligationFacade

**Detailed Steps:**
1. Create `/src/Subsystems/Obligation/` directory
2. Create COMPLETE ObligationFacade.cs with ALL obligation methods
3. Create COMPLETE DeliveryManager.cs (migrate ~500 lines for letter delivery)
4. Create COMPLETE MeetingManager.cs (migrate ~400 lines for meetings)
5. Create COMPLETE QueueManipulator.cs (migrate ~600 lines for queue ops)
6. Create COMPLETE DisplacementCalculator.cs (migrate ~500 lines for displacement)
7. Create COMPLETE DeadlineTracker.cs (migrate ~400 lines for deadlines)
8. Migrate ALL statistics and utility methods (~400 lines)
9. DELETE ObligationQueueManager.cs completely
10. Update ALL references in GameFacade to use ObligationFacade
11. Update ALL UI components to work with new structure
12. NO old code remains, NO compatibility layers

**Complete Method Migration Map:**
```
ObligationQueueManager → ObligationFacade delegation:
- AcceptLetter() → DeliveryManager.AcceptLetter()
- DeliverLetter() → DeliveryManager.DeliverLetter()
- ScheduleMeeting() → MeetingManager.ScheduleMeeting()
- MoveToPosition() → QueueManipulator.MoveToPosition()
- CalculateDisplacement() → DisplacementCalculator.Calculate()
- CheckDeadlines() → DeadlineTracker.CheckDeadlines()
[... ALL 2,819 lines accounted for ...]
```

**Validation:**
- ALL queue operations work perfectly
- NO ObligationQueueManager code remains
- ALL UI components function correctly
- Zero regression in functionality

**Detailed Steps:**
1. Analyze ObligationQueueManager methods:
   - Letter delivery methods (lines 1-500)
   - Meeting management (lines 501-1000)
   - Queue operations (lines 1001-1500)
   - Displacement logic (lines 1501-2000)
   - Statistics/reporting (lines 2001-2500)
   - Utility methods (lines 2501-2819)

2. Create responsibility map:
```
DeliveryManager:
- AcceptLetter()
- DeliverLetter()
- ProcessDeliveryEffects()
- GetActiveDeliveries()

MeetingManager:
- ScheduleMeeting()
- CheckMeetingTime()
- CompleteMeeting()
- GetUpcomingMeetings()

QueueManipulator:
- AddToQueue()
- RemoveFromQueue()
- MoveToPosition()
- GetQueueState()

DisplacementCalculator:
- CalculateDisplacementCost()
- PreviewDisplacement()
- ExecuteDisplacement()
- BurnTokensForDisplacement()

DeadlineTracker:
- CheckDeadlines()
- ProcessExpiredObligations()
- GetTimeRemaining()
- SendDeadlineWarnings()
```

### Work Packet 2.3: ObligationSubsystem - Implementation
**Owner:** Systems Architect Agent
**Dependencies:** WP 2.2
**Deliverables:**
- Create ObligationFacade
- Implement all manager classes
- Migrate code from ObligationQueueManager
- Maintain queue integrity

**Detailed Steps:**
1. Create ObligationFacade.cs
2. Create DeliveryManager.cs (extract ~500 lines)
3. Create MeetingManager.cs (extract ~400 lines)
4. Create QueueManipulator.cs (extract ~600 lines)
5. Create DisplacementCalculator.cs (extract ~500 lines)
6. Create DeadlineTracker.cs (extract ~400 lines)
7. Update GameFacade to use ObligationFacade

**Validation:**
- Queue operations work correctly
- Displacement calculations accurate
- Deadlines tracked properly
- Letter delivery functional

## Phase 3: Supporting Subsystems (Week 3)

### Work Packet 3.1: ResourceSubsystem
**Owner:** Game Mechanics Designer Agent
**Dependencies:** WP 2.3
**Deliverables:**
- ResourceFacade implementation
- Individual resource managers
- Resource calculation formulas
- Morning refresh logic

**Detailed Steps:**
1. Create ResourceFacade.cs
2. Extract from Player.cs and GameFacade:
   - Coin operations → CoinManager
   - Health mechanics → HealthManager
   - Hunger system → HungerManager
   - Attention allocation → AttentionManager

3. Implement ResourceCalculator:
```csharp
public class ResourceCalculator
{
    public int CalculateMorningAttention(int hunger)
    {
        return Math.Max(2, 10 - (hunger / 25));
    }
    
    public int CalculateWeightLimit(int health)
    {
        return health < 50 ? -1 : 0;
    }
}
```

### Work Packet 3.2: TimeSubsystem
**Owner:** Systems Architect Agent
**Dependencies:** WP 3.1
**Deliverables:**
- TimeFacade implementation
- Time period management
- Weather system integration
- Event scheduling

**Detailed Steps:**
1. Create TimeFacade.cs
2. Move from TimeManager:
   - Period transitions → TimeBlockManager
   - Weather updates → WeatherManager
   - Scheduled events → TimeEventScheduler

### Work Packet 3.3: TravelSubsystem
**Owner:** Systems Architect Agent
**Dependencies:** WP 3.2
**Deliverables:**
- TravelFacade implementation
- Route management
- Permit validation
- Discovery mechanics

**Detailed Steps:**
1. Create TravelFacade.cs
2. Extract from TravelManager and GameFacade:
   - Route operations → RouteManager
   - Discovery system → RouteDiscoveryManager
   - Permit checks → PermitValidator
   - Time calculations → TravelTimeCalculator

## Phase 4: Complex Subsystems (Week 4)

### Work Packet 4.1: MarketSubsystem
**Owner:** Content Integrator Agent
**Dependencies:** WP 3.3
**Deliverables:**
- MarketFacade implementation
- Arbitrage system
- Price calculations
- Market state tracking

**Detailed Steps:**
1. Analyze MarketManager.cs (825 lines)
2. Create MarketFacade.cs
3. Extract:
   - Core logic → MarketManager (simplified)
   - Arbitrage → ArbitrageCalculator
   - Prices → PriceManager
   - State → MarketStateTracker

### Work Packet 4.2: TokenSubsystem
**Owner:** Game Mechanics Designer Agent
**Dependencies:** WP 4.1
**Deliverables:**
- TokenFacade implementation
- Token effect processing
- Relationship tracking
- Unlock mechanics

**Detailed Steps:**
1. Create TokenFacade.cs
2. Extract from TokenMechanicsManager:
   - Core operations → ConnectionTokenManager
   - Effects → TokenEffectProcessor
   - Unlocks → TokenUnlockManager
   - Relationships → RelationshipTracker

### Work Packet 4.3: NarrativeSubsystem
**Owner:** Narrative Designer Agent
**Dependencies:** WP 4.2
**Deliverables:**
- NarrativeFacade implementation
- Observation system
- Message handling
- Event narration

**Detailed Steps:**
1. Create NarrativeFacade.cs
2. Consolidate:
   - ObservationManager
   - MessageSystem
   - NarrativeRenderer
   - EventNarrator

## Phase 5: GameFacade Refactoring (Week 5)

### Work Packet 5.1: GameFacade Slimming
**Owner:** Systems Architect Agent
**Dependencies:** All previous WPs
**Deliverables:**
- Refactored GameFacade (target: <500 lines)
- All logic delegated to subsystems
- Clean dependency injection
- Backward compatibility maintained

**Detailed Steps:**
1. Review current GameFacade (3,448 lines)
2. Ensure all methods delegate to subsystems
3. Remove all business logic
4. Keep only coordination logic
5. Update constructor to inject all facades

**Target Structure:**
```csharp
public class GameFacade
{
    // Subsystem facades
    private readonly LocationFacade _location;
    private readonly ConversationFacade _conversation;
    private readonly ObligationFacade _obligation;
    private readonly TravelFacade _travel;
    private readonly ResourceFacade _resource;
    private readonly TimeFacade _time;
    private readonly MarketFacade _market;
    private readonly TokenFacade _token;
    private readonly NarrativeFacade _narrative;
    
    // All methods just delegate
    public Location GetCurrentLocation() => _location.GetCurrentLocation();
    public ConversationContext StartConversation(string npcId, ConversationType type) 
        => _conversation.StartConversation(npcId, type);
    // etc...
}
```

### Work Packet 5.2: Dependency Injection Update
**Owner:** Systems Architect Agent
**Dependencies:** WP 5.1
**Deliverables:**
- Updated ServiceConfiguration.cs
- All subsystems registered
- Proper dependency chains
- No circular dependencies

## Phase 6: UI Optimization (Week 6)

### Work Packet 6.1: UI Component Analysis
**Owner:** Content Integrator Agent
**Dependencies:** WP 5.2
**Deliverables:**
- Analysis of large UI components
- Identify subsystem dependencies
- Plan for direct facade injection

**Components to Analyze:**
- ConversationContent.razor.cs (1,784 lines)
- LocationContent.razor.cs (656 lines)
- MainGameplayView.razor.cs (582 lines)
- GameScreen.razor.cs (443 lines)

### Work Packet 6.2: UI Refactoring
**Owner:** Systems Architect Agent
**Dependencies:** WP 6.1
**Deliverables:**
- Refactored UI components
- Direct subsystem facade injection
- Reduced component size
- Improved performance

**Example Refactoring:**
```csharp
// Before: Inject GameFacade
@inject GameFacade GameFacade

// After: Inject only needed subsystems
@inject ConversationFacade Conversation
@inject ResourceFacade Resources
```

## Phase 7: Testing & Validation (Week 7)

### Work Packet 7.1: Unit Test Creation
**Owner:** Change Validator Agent
**Dependencies:** WP 6.2
**Deliverables:**
- Unit tests for each subsystem
- Integration tests for facade interactions
- Performance benchmarks
- Test coverage report

### Work Packet 7.2: End-to-End Testing
**Owner:** Change Validator Agent
**Dependencies:** WP 7.1
**Deliverables:**
- Full game flow testing
- UI interaction testing
- Save/load testing
- Performance validation

## CRITICAL EXECUTION RULES

### NO FALLBACKS
- When implementing a subsystem, implement it COMPLETELY
- NO placeholder methods
- NO "will implement later" comments
- NO partial functionality
- If a method exists in old code, it MUST exist in new code IMMEDIATELY

### NO COMPATIBILITY LAYERS
- Delete old code as soon as new code is ready
- NO keeping both versions
- NO switch statements choosing implementations
- NO gradual migration
- Cut over COMPLETELY or not at all

### NO TODOS
- Every work packet produces COMPLETE, WORKING code
- NO TODO comments in code
- NO stub implementations
- NO "temporary" solutions
- Production-ready code from day one

### COMPLETE MIGRATION ONLY
- When moving code, move ALL related code
- Delete source immediately after migration
- Update ALL references at once
- NO partial migrations
- NO leaving code in both places

## Migration Checklist

### Per Subsystem (MUST BE 100% BEFORE PROCEEDING):
- [ ] Facade COMPLETELY implemented
- [ ] ALL internal managers created and functional
- [ ] ALL code migrated from monoliths
- [ ] OLD code DELETED
- [ ] GameFacade FULLY updated
- [ ] ALL UI components working
- [ ] ZERO old code remains
- [ ] ZERO compatibility layers

### Global:
- [ ] ALL subsystems COMPLETELY implemented
- [ ] GameFacade FULLY refactored (<500 lines)
- [ ] ALL monolithic classes DELETED
- [ ] NO legacy code anywhere
- [ ] Full test suite passing
- [ ] Performance improved
- [ ] NO TODOS in codebase

## Success Metrics

1. **Code Size Reduction:**
   - GameFacade: 3,448 → <500 lines
   - ObligationQueueManager: 2,819 → 0 (split into 5 managers)
   - Average class size: <500 lines

2. **Complexity Reduction:**
   - No class with >10 dependencies
   - Clear single responsibility
   - No circular dependencies

3. **Performance:**
   - Faster startup time
   - Reduced memory usage
   - Better test execution time

4. **Maintainability:**
   - New features added to appropriate subsystem
   - Changes isolated to subsystem
   - Tests run in isolation

## Risk Mitigation

1. **Breaking Changes:**
   - Maintain GameFacade API
   - Incremental migration
   - Comprehensive testing

2. **Performance Regression:**
   - Benchmark before/after
   - Profile critical paths
   - Optimize hot spots

3. **Lost Functionality:**
   - Complete method inventory
   - Trace all code paths
   - Extensive testing

## EXECUTION ORDER (NO DEVIATIONS)

### Immediate Parallel Execution (ALL AT ONCE):
1. **LocationSubsystem** - COMPLETE implementation
2. **ConversationSubsystem** - COMPLETE implementation  
3. **ObligationSubsystem** - COMPLETE implementation

Each subsystem MUST be:
- 100% complete before any code is committed
- Fully tested and working
- Old code completely deleted
- NO intermediate states

### Sequential Execution (AFTER first three):
4. **ResourceSubsystem** - COMPLETE implementation
5. **TimeSubsystem** - COMPLETE implementation
6. **TravelSubsystem** - COMPLETE implementation
7. **MarketSubsystem** - COMPLETE implementation
8. **TokenSubsystem** - COMPLETE implementation
9. **NarrativeSubsystem** - COMPLETE implementation
10. **GameFacade Refactoring** - Reduce to <500 lines

## Agent Delegation Strategy

### Work Packet Assignments:

**Systems Architect Agent**:
- Work Packet 1.1: LocationSubsystem (COMPLETE)
- Work Packet 1.2: ConversationSubsystem (COMPLETE)
- Work Packet 2.1: ObligationSubsystem (COMPLETE)
- NO PARTIAL IMPLEMENTATIONS ALLOWED

**Content Integrator Agent**:
- Analyze and map ALL dependencies BEFORE implementation
- Ensure COMPLETE migration paths
- Verify NO code left behind

**Change Validator Agent**:
- Validate COMPLETE functionality after EACH subsystem
- Ensure ZERO regression
- Confirm OLD code DELETED

## IMMEDIATE NEXT STEPS

1. Delegate Work Packet 1.1 (LocationSubsystem) to Systems Architect
2. Delegate Work Packet 1.2 (ConversationSubsystem) to Systems Architect
3. Delegate Work Packet 2.1 (ObligationSubsystem) to Systems Architect
4. ALL THREE MUST BE COMPLETE BEFORE ANY COMMITS
5. NO INTERMEDIATE STATES ALLOWED

This modularization will transform Wayfarer from a monolithic codebase into a clean, maintainable, extensible architecture while preserving all existing functionality.