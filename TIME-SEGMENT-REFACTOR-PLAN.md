# Time System Refactoring: Minutes → Segments

## Overview
Complete refactoring of Wayfarer's time system from minute/hour-based tracking to a segment-based system that creates meaningful tactical decisions with clear opportunity costs.

## Design Specifications

### Time Structure
- **16 playable segments per day** (6 AM - 10 PM)
- **5 time periods**:
  - Dawn (6-10 AM): 3 segments
  - Midday (10 AM-2 PM): 4 segments  
  - Afternoon (2-6 PM): 4 segments
  - Evening (6-10 PM): 4 segments
  - Night (10 PM-12 AM): 1 segment
- **Deep Night (12 AM-6 AM)**: Sleep only, jumps to Dawn

### Action Costs
- **FREE (0 segments)**: Observations, quick exchanges (<1 attention), same-location deliveries
- **MINOR (1 segment)**: Standard conversations, investigations, adjacent travel
- **MAJOR (2 segments)**: Crisis conversations, distant travel, extended investigations  
- **PERIOD (4 segments)**: Work, full rest (jumps to next period)

### Key Mechanics
- **Patience exhaustion** in conversations costs +1 segment to continue
- **Waiting** costs 1 patience per segment
- **Markets close** at Evening segment 4/4
- **Deadlines** shown as "X segments remain" or "Due by [Period]"

## Implementation Phases

### Phase 1: Core Time System Refactoring

#### 1.1 TimeState (`/src/GameState/StateContainers/TimeState.cs`)
- Remove: `CurrentHour`, `CurrentMinute` 
- Add: `CurrentSegment`, `SegmentsInCurrentPeriod`
- Keep: `CurrentDay`, `CurrentTimeBlock`
- New methods: `AdvanceSegments()`, `GetSegmentDisplay()`, `CanAdvanceSegments()`

#### 1.2 TimeModel (`/src/GameState/TimeModel.cs`)
- Remove: All minute/hour methods
- Add: Segment validation and advancement
- Update: Day rollover logic for segments

#### 1.3 TimeManager (`/src/GameState/TimeManager.cs`)
- Complete refactor to segment-based operations
- Remove: `AdvanceTimeMinutes()`, `AdvanceTime(hours)`
- Add: `AdvanceSegments(int segments)`, `JumpToNextPeriod()`
- Update: All time transaction logic

#### 1.4 TimeBlocks (`/src/GameState/TimeBlocks.cs`)
```csharp
public enum TimeBlocks
{
    Dawn,     // 3 segments
    Midday,   // 4 segments  
    Afternoon,// 4 segments
    Evening,  // 4 segments
    Night,    // 1 segment
    DeepNight // Sleep only
}

public static Dictionary<TimeBlocks, int> SegmentsPerBlock = new()
{
    { TimeBlocks.Dawn, 3 },
    { TimeBlocks.Midday, 4 },
    { TimeBlocks.Afternoon, 4 },
    { TimeBlocks.Evening, 4 },
    { TimeBlocks.Night, 1 }
};
```

### Phase 2: Deadline System Conversion

#### 2.1 DeadlineTracker (`/src/Subsystems/Obligation/DeadlineTracker.cs`)
- Change ALL `DeadlineInMinutes` to `DeadlineInSegments`
- Update urgency levels: Critical (≤2), Urgent (2-4), Normal (>4)
- Refactor: `ProcessHourlyDeadlines()` → `ProcessSegmentDeadlines()`

#### 2.2 Obligation Classes
- `DeliveryObligation.cs`: Change deadline property to segments
- `MeetingObligation.cs`: Update meeting time to segments
- Update all display methods to show segments

### Phase 3: Travel System Refactoring

#### 3.1 TravelTimeCalculator (`/src/Subsystems/Travel/TravelTimeCalculator.cs`)
- Remove ALL minute calculations
- Base travel costs: Adjacent (1 segment), Near (2 segments), Far (3 segments)
- Weather modifiers: Rain (+1 segment), Storm (+2 segments)

#### 3.2 Travel Content
- Update all route definitions with segment costs
- Remove transport method time modifiers (complexity reduction)

### Phase 4: Action Commands

#### 4.1 Update All Time-Consuming Commands
- `AdvanceTimeCommand.cs`: Use segments
- `StartConversationCommand.cs`: 1 segment base
- `InvestigateCommand.cs`: 1 segment
- `WorkCommand.cs`: 4 segments (period jump)
- `RestCommand.cs`: 4 segments (period jump)
- `SleepCommand.cs`: Jump to Dawn

### Phase 5: UI System Updates

#### 5.1 Time Display Components
- `TimeDisplayHelper.cs`: Show "AFTERNOON ●●○○ [2/4]"
- `TimeDisplayFormatter.cs`: Format segments, not clock time
- `GameScreen.razor`: Update resource bar time display

#### 5.2 Deadline Display
- `ObligationQueueContent.razor`: Show segments remaining
- Update all deadline formatting throughout UI

### Phase 6: JSON Content Migration

#### 6.1 Letter Deadlines (`core_game_package.json`)
- Convert all `deadlineInMinutes` to `deadlineInSegments`
- Example: 1440 minutes (1 day) → 16 segments

#### 6.2 Travel Times (`travel_package.json`)
- Convert all path card `travelTimeMinutes` to `segmentCost`
- Example: 30 minutes → 1 segment, 90 minutes → 2 segments

### Phase 7: Save System Migration

#### 7.1 SaveGame Structure
- Add migration logic to convert old minute-based saves
- Store segment data in new format
- Maintain backwards compatibility for one version

## File Change Summary

### Core Files (Must Change)
1. TimeState.cs - Complete refactor
2. TimeModel.cs - Complete refactor  
3. TimeManager.cs - Complete refactor
4. TimeBlocks.cs - Add segment definitions
5. DeadlineTracker.cs - Convert to segments
6. TravelTimeCalculator.cs - Convert to segments
7. All obligation classes - Update deadline properties
8. All time display components - Show segments
9. All JSON content - Update time values

### Secondary Files (Update References)
- GameFacade.cs - Update all time advancement calls
- All command classes - Use segment costs
- All test files - Update to segment-based testing

## Testing Strategy

1. **Unit Tests**: Verify segment arithmetic and period transitions
2. **Integration Tests**: Test full action sequences with segment costs
3. **UI Tests**: Verify segment display and deadline visualization
4. **Save Migration**: Test converting old saves to new format

## Success Criteria

- [ ] No references to minutes/hours remain in codebase
- [ ] All actions have clear segment costs
- [ ] UI displays segments clearly (●●○○ format)
- [ ] Deadlines show as segments or period ends
- [ ] Travel costs are in segments
- [ ] Conversations consume segments on patience exhaustion
- [ ] Save/load works with new system
- [ ] All tests pass

## Risk Mitigation

- Create parallel segment system first before removing minutes
- Test each phase independently
- Maintain save compatibility
- Clear UI indicators for segment costs

## Estimated Impact

This refactoring will touch approximately 50+ files directly and affect 200+ files indirectly through reference updates. The change is fundamental but will dramatically improve gameplay clarity and tactical decision-making.