# Complete UI Integration Plan

## Overview
This document outlines the plan to remove wrongly implemented systems and ensure ALL game mechanics are accessible through the UI, fulfilling the core principle: "the player must be able to access and see all mechanics and all effects of all systems through the UI."

## Current Status (35% of mechanics hidden from players)
- Core gameplay accessible but strategic mechanics hidden
- Critical queue management commands have no UI
- Debt repayment system disconnected
- Information discovery completely invisible
- Many UI components bypass GameFacade architecture

## Phase 1: Remove Wrong/Redundant Systems

### 1.1 Remove Resource Gathering System
**Rationale**: Contradicts letter carrier premise, may already be removed
- [ ] Verify if `GatherResourcesCommand.cs` still exists
- [ ] Remove gathering logic from `CommandDiscoveryService`
- [ ] Remove any UI references to resource gathering
- [ ] Clean up location FEATURE spots

### 1.2 Remove Browse Command
**Rationale**: Information display doesn't need a command
- [ ] Delete `BrowseCommand.cs` if exists
- [ ] Remove from `CommandDiscoveryService`
- [ ] Ensure market prices display directly

### 1.3 Remove Mechanical Social Commands
**Rationale**: Social interactions should be meaningful choices
- [ ] Remove `ShareLunchCommand.cs` (mechanical token farming)
- [ ] Remove `EquipmentSocializeCommand.cs` (equipment shouldn't affect relationships)
- [ ] Keep only meaningful social interactions

## Phase 2: Critical Missing UI (HIGH PRIORITY)

### 2.1 Letter Queue Management UI
**Current**: Commands exist but no UI access
**Implementation**:
```csharp
// Add to IGameFacade:
Task<bool> SwapLettersAsync(int position1, int position2);
Task<bool> PriorityMoveAsync(int fromPosition);
Task<bool> ExtendDeadlineAsync(int position);
Task<bool> PurgeLetterAsync(int position, Dictionary<string, int> tokens);

// Add to LetterQueueScreen.razor:
- Swap button (Dawn only, free action)
- Priority move button (costs Trust tokens)
- Extend deadline button (costs Commerce tokens)
- Purge button for position 8 (costs tokens)
```

### 2.2 Debt Repayment System
**Current**: Can view debts but cannot repay
**Implementation**:
```csharp
// Add to IGameFacade:
Task<bool> RepayDebtAsync(string npcId, int amount);
List<DebtRepaymentOption> GetRepaymentOptions(string npcId);

// Update DebtManagementScreen.razor:
- Add "Repay" button for each debt
- Show repayment amount options
- Display immediate effects (leverage reduction)
```

### 2.3 Special Item Reading
**Current**: Information letters go to inventory but can't be read
**Implementation**:
```csharp
// Already exists in IGameFacade:
Task<bool> UseItemAsync(string itemId);

// Update inventory display:
- Add "Read" button for readable items
- Show item effects when read
- Update player knowledge/memories
```

## Phase 3: Important Missing Systems

### 3.1 Information Discovery UI
**Current**: Two-phase discovery system completely hidden
**Implementation**:
```csharp
// Add to IGameFacade:
ExploreOptionsViewModel GetExploreOptions();
Task<bool> ExploreLocationAsync(string targetId);

// Add exploration UI:
- Show "Explore" action at appropriate locations
- Display discovery progress
- Show locked content with requirements
```

### 3.2 Mechanical Transparency
**Current**: Hidden effects on game systems
**Implementation**:
- Show leverage effects on letter queue positions
- Display token thresholds for special letters (5+)
- Indicate standing obligation modifications
- Add tooltips explaining mechanical connections

### 3.3 Service & Schedule Clarity
**Current**: Partial implementation, needs completion
**Implementation**:
- Complete NPC service display at each location
- Show market hours and availability
- Display time-gated content clearly
- Add service type icons consistently

## Phase 4: Architecture Compliance

### 4.1 Fix UI Service Injection
**Current**: 20+ components inject services directly
**Implementation**:
- Refactor all UI components to use only IGameFacade
- Remove direct service injection
- Add missing GameFacade methods as needed

### 4.2 Complete Command Registration
**Current**: Some commands not discoverable
**Implementation**:
- Ensure all commands registered in CommandDiscoveryService
- Verify all commands accessible through UI
- Add missing command UI connections

## Phase 5: UI Usability Improvements

### 5.1 Implement ui-usability-plan.md Phase 1
- Market schedule display
- Service type clarity
- Action availability indicators
- Queue pressure in navigation

### 5.2 Information Architecture
- Move data to conceptually appropriate screens
- Ensure "two-click rule" compliance
- Add cross-references where needed

## Implementation Order

### Week 1: Critical Systems
1. Letter Queue Management UI (2 days)
2. Debt Repayment Connection (1 day)
3. Special Item Reading (1 day)
4. Testing & Bug Fixes (1 day)

### Week 2: Discovery & Transparency
1. Information Discovery UI (2 days)
2. Mechanical Transparency (2 days)
3. Service Display Completion (1 day)

### Week 3: Architecture & Polish
1. UI Service Injection Fixes (3 days)
2. Command Registration Audit (1 day)
3. Usability Improvements (1 day)

## Success Criteria

After implementation:
1. **100% of game mechanics accessible through UI**
2. No hidden backend actions affecting gameplay
3. All effects clearly visible to players
4. Complete GameFacade architecture compliance
5. Players can make fully informed strategic decisions
6. New players can discover all mechanics naturally

## Testing Plan

1. **Mechanical Coverage Test**: Verify every command has UI access
2. **Information Visibility Test**: Ensure all effects are displayed
3. **Architecture Test**: Verify no direct service injection
4. **New Player Test**: Can new players discover all mechanics?
5. **Strategic Depth Test**: Can players access all strategic options?

## Notes

- Prioritize player agency and mechanical transparency
- Follow existing UI patterns and CSS conventions
- Maintain clean architecture principles
- Test thoroughly after each phase
- Document any new patterns or conventions