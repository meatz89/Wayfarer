# Current State and Target Architecture

## Current State (2025-08-03)

### Completed Migrations
1. **Intent-Based Architecture** ✅
   - Removed CommandDiscoveryService, GameWorldManager, NarrativeManager
   - All player actions now use pure intent objects
   - GameFacade handles intent execution with pattern matching
   - GameWorld is the single source of truth

2. **Implemented Intent Handlers** ✅
   - Movement: ExecuteMove, ExecuteTravel
   - Social: ExecuteTalk, ExecuteDiscoverRoute
   - Rest: ExecuteRest, ExecuteObserve
   - Letters: ExecuteDeliverLetter, ExecuteCollectLetter, ExecuteAcceptOffer
   - Exploration: ExecuteExplore
   - Patron: ExecutePatronFunds
   - Guild: ExecuteConvertEndorsements

3. **Game Systems Implemented** ✅
   - Special Letter Generation (Phase 1)
   - Information Letter Satchel (Phase 2)
   - Multi-Context Token Display (Phase 5)
   - Endorsement-to-Seal System (partial)
   - Route Discovery System (partial)

### Current Architecture
```
UI Components → Intent Objects → GameFacade → GameWorld
                                      ↓
                                  Services/Managers
```

## Target Architecture

### Core Principles
1. **No Silent Backend Actions** - Every state change must be player-initiated
2. **GameWorld as Single Source of Truth** - No duplicate state tracking
3. **No Special Rules** - Use categorical mechanics instead of exceptions
4. **Intent-Based Actions** - Commands are pure data, execution in services
5. **Everything Has UI** - If it exists in backend, player must see and interact

### Missing Systems (From IMPLEMENTATION-PLAN-COMPLETE-SYSTEMS.md)

#### Phase 4: Route Discovery System (Partially Complete)
- ✅ Explore action implemented
- ✅ Route discovery from NPCs implemented
- ❌ Integration with Information letters
- ❌ Access requirement UI enhancements

#### Phase 6: Time Cost System (Needs Verification)
- ❌ Time cost preview component
- ❌ Deadline impact warnings
- ❌ Visual time indicators on all actions

#### Phase 7: Leverage Visibility (Not Started)
- ❌ Queue position indicators showing WHY letters move
- ❌ Skip cost breakdown with modifiers
- ❌ Debt warning system with visual indicators

#### Phase 8: Standing Obligations Integration (Not Started)
- ❌ Active obligations display panel
- ❌ Threshold warnings before triggering obligations
- ❌ Obligation effects UI in relevant contexts

### Architectural Gaps

1. **UI Components Missing**
   - TimePreviewComponent
   - LeverageCalculationDisplay
   - ObligationEffectsPanel
   - DeadlineWarningDialog

2. **Service Integration Gaps**
   - StandingObligationManager not fully integrated with UI
   - LeverageCalculations not visible in LetterQueueDisplay
   - Time costs not shown consistently across actions

3. **Information Flow Issues**
   - Route discoveries from Information letters not connected
   - Obligation effects not propagated to skip costs
   - Leverage calculations hidden from player

### Target State
- All game mechanics visible and interactive
- No hidden calculations or effects
- Complete player agency over all actions
- Clear cause-and-effect for all systems