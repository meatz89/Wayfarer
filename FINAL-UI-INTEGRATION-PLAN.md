# Final UI Integration Plan

## Overview
This document consolidates all remaining work to:
1. Remove wrongly implemented systems that violate game design principles
2. Ensure ALL game mechanics are accessible through the UI
3. Achieve 100% GameFacade architectural compliance

## Part 1: Remove Wrong Systems

### Phase 1.1: Mechanical Social Commands (IMMEDIATE)

#### 1.1.1 Remove SocializeCommand
**Rationale**: Creates artificial "token farming" instead of natural conversation
- [ ] Delete `/src/GameState/Commands/SocializeCommand.cs`
- [ ] Remove from CommandTypes.cs
- [ ] Remove from CommandDiscoveryService
- [ ] Update any UI references

#### 1.1.2 Remove PersonalErrandCommand  
**Rationale**: Requires "medicine" for all errands (special rule violation)
- [ ] Delete `/src/GameState/Commands/PersonalErrandCommand.cs`
- [ ] Delete `/src/Pages/PersonalErrandScreen.razor`
- [ ] Remove PersonalErrandScreen from CurrentViews enum
- [ ] Remove from NavigationBar and MainGameplayView routing
- [ ] Remove from CommandDiscoveryService

#### 1.1.3 Remove WorkCommand
**Rationale**: Generic "work for coins" bypasses letter economy
- [ ] Delete `/src/GameState/Commands/WorkCommand.cs`
- [ ] Remove from CommandTypes.cs
- [ ] Remove from CommandDiscoveryService
- [ ] Update NPCActionsView to remove work options

### Phase 1.2: Special Rules Commands

#### 1.2.1 Evaluate PatronFundsCommand
**Rationale**: 7-day cooldown is arbitrary special rule
- [ ] Review if patron funding should go through letter/conversation system
- [ ] If removing, delete `/src/GameState/Commands/PatronFundsCommand.cs`
- [ ] Update patron interaction to use natural systems

#### 1.2.2 Review Endorsement System
**Rationale**: May be redundant with token system
- [ ] Analyze if ConvertEndorsementsCommand creates special mechanics
- [ ] Determine if seals/endorsements are needed or redundant
- [ ] Remove if redundant with token progression

## Part 2: Add Missing UI (Critical)

### Phase 2.1: Letter Queue Management UI ✅ COMPLETED
- Morning swap, priority move, extend deadline, purge actions
- All queue management commands now have UI access

### Phase 2.2: Special Item Reading
**Current**: Information letters go to inventory but can't be read
**Implementation**:
```csharp
// Already exists in IGameFacade:
Task<bool> UseItemAsync(string itemId);

// Update inventory display:
- Add "Read" button for readable items (letters, books, notes)
- Show reading interface with content
- Update player knowledge/memories after reading
- Show discovered information effects
```

### Phase 2.3: Information Discovery UI
**Current**: Two-phase discovery system completely hidden
**Implementation**:
```csharp
// Add to IGameFacade:
ExploreOptionsViewModel GetExploreOptions();
Task<bool> ExploreLocationAsync(string targetId);

// Add exploration UI:
- Show "Explore" action at appropriate locations
- Display discovery progress (phase 1: rumors, phase 2: actual discovery)
- Show locked content with requirements
- Integrate with carried information letters
```

## Part 3: Architecture Compliance

### Phase 3.1: Fix StandingObligationsScreen
**Current**: Injects 6 managers directly, violating GameFacade pattern
**Implementation**:
- Remove all @inject directives except IGameFacade
- Add missing methods to IGameFacade for obligation data
- Update component to use only GameFacade

### Phase 3.2: Consolidate UI Services
**Current**: Multiple UI services alongside GameFacade
**Implementation Strategy**:
1. **LocationActionsUIService** → GameFacade
   - Already partially integrated via GetLocationActions()
   - Move command discovery and availability logic
   - Consolidate action filtering and tutorial restrictions
   
2. **RestUIService** → GameFacade
   - Already integrated via GetRestOptions()
   - Consider if any additional logic needs moving
   
3. **MarketUIService** → GameFacade
   - Already integrated via GetMarket()
   - Verify all market functionality accessible
   
4. **LetterQueueUIService** → GameFacade
   - Already integrated for queue operations
   - Check for any missing queue management functions
   
5. **TravelUIService** → GameFacade
   - Already integrated via travel methods
   - Ensure route discovery and requirements visible

**Key Considerations**:
- Don't delete services if GameFacade still depends on them
- Move logic gradually to avoid breaking existing functionality
- Update all UI components that reference these services directly

## Part 4: Mechanical Transparency

### Phase 4.1: Show Leverage Effects
**Current**: Negative tokens create leverage but effects hidden
**Implementation**:
- Show queue position modifications in real-time
- Display leverage calculations on letter cards
- Add tooltips explaining token → position effects
- Highlight when letters jump queue due to leverage

### Phase 4.2: Token Threshold Visibility
**Current**: 5+ tokens unlock special letters but not shown
**Implementation**:
- Show progress bars to token thresholds
- Display "Next unlock at X tokens" indicators
- Preview available special letter types
- Show relationship milestone effects

### Phase 4.3: Obligation Transparency
**Current**: Standing obligations modify queue but effects unclear
**Implementation**:
- Show active obligation effects on queue display
- Highlight which letters are affected by obligations
- Display obligation triggers and conditions
- Show ways to gain/remove obligations

## Part 5: Service & Schedule Clarity

### Phase 5.1: Complete NPC Service Display
**Implementation**:
- Show all services each NPC provides
- Display service availability by time
- Show requirements for each service
- Indicate when services are locked/unlocked

### Phase 5.2: Market Hours Display
**Implementation**:
- Show market open/close times clearly
- Display time until next market opening
- Show which NPCs are traders
- Preview available goods before traveling

## Implementation Order

### Week 1: Remove Wrong Systems
1. Delete mechanical social commands (2 days)
2. Remove PersonalErrandScreen and routing (1 day)
3. Evaluate patron/endorsement systems (1 day)
4. Test and fix any broken references (1 day)

### Week 2: Critical Missing UI
1. Special Item Reading interface (2 days)
2. Information Discovery UI (2 days)
3. Testing & Polish (1 day)

### Week 3: Architecture & Transparency
1. Fix StandingObligationsScreen injection (1 day)
2. Consolidate UI services (2 days)
3. Implement mechanical transparency (2 days)

### Week 4: Final Polish
1. Service & schedule clarity (2 days)
2. Cross-reference testing (1 day)
3. New player experience validation (2 days)

## Success Criteria

1. **No Mechanical Commands**: All token earning through natural interaction
2. **No Special Rules**: Every mechanic emerges from core systems
3. **100% UI Coverage**: Every backend action visible and accessible
4. **Clean Architecture**: All UI uses GameFacade exclusively
5. **Transparent Mechanics**: Players understand all cause-effect relationships
6. **Natural Discovery**: New players find mechanics through play, not tutorials

## Testing Checklist

- [ ] Verify no mechanical token farming exists
- [ ] Confirm all commands accessible through UI
- [ ] Test that removing items doesn't break core loop
- [ ] Validate GameFacade is sole UI-backend interface
- [ ] Ensure new players can discover all mechanics
- [ ] Check that efficiency emerges from natural overlap

## Notes

- Prioritize removing wrong systems before adding new UI
- Test thoroughly after each removal
- Document any edge cases discovered
- Keep market/trade systems as they support natural efficiency

## New Learnings from Implementation

### CSS Organization
- **NEVER write styles in .razor files** - All styles must be in dedicated CSS files
- Reuse existing CSS classes before creating new ones
- ui-components.css already contains extensive styles for common patterns
- Check for existing warning, threshold, debt styles before creating duplicates

### Architecture Patterns
- MainGameplayViewBase provides GameFacade access - don't re-inject it
- ViewModels should be comprehensive to avoid multiple backend calls
- Null checks are essential when managers might not be initialized
- Keep view models flat and simple for easier binding

### Refactoring Process
1. Read ALL related files before making changes
2. Check for existing implementations before creating new ones
3. Use comprehensive ViewModels to replace multiple service calls
4. Remove ALL direct service/repository injections
5. Ensure GameFacade methods handle null dependencies gracefully

## Implementation Progress

### Phase 1: Remove Wrong Systems ✅
- [x] Phase 1.1.1: Remove SocializeCommand - COMPLETE
- [x] Phase 1.1.2: Remove PersonalErrandCommand - COMPLETE
- [x] Phase 1.1.3: Remove WorkCommand - COMPLETE
- [x] Phase 1.2.1: Evaluate PatronFundsCommand - KEPT (uses token system correctly)
- [x] Phase 1.2.2: Review Endorsement System - KEPT (natural progression system)

### Phase 2: Critical Missing UI ✅
- [x] Phase 2.1: Letter Queue Management UI - COMPLETE
- [x] Phase 2.2: Special Item Reading - ALREADY IMPLEMENTED
- [x] Phase 2.3: Information Discovery UI - COMPLETE

### Phase 3: Architecture Compliance 🔄
- [x] Phase 3.1: Fix StandingObligationsScreen to use GameFacade - COMPLETE
- [ ] Phase 3.2: Consolidate UI Services

### Phase 4: Mechanical Transparency
- [x] Phase 4.1: Show Leverage Effects - COMPLETE
- [x] Phase 4.2: Token Threshold Visibility - COMPLETE
- [x] Phase 4.3: Obligation Transparency - COMPLETE
- Ensure conversation system can handle removed command functionality