# Tutorial Refactoring Progress - Session 1

## Date: 2025-10-30

## Context

Major refactoring to implement tutorial system based on revised specification:
- **Tutorial Design**: Single evening arrival scene with 3 challenge-based paths
- **Duration**: Evening Segment 1 → Night Rest → Day 2 Morning
- **Core Mechanic**: Player arrives with 8 coins, needs 10 for room, must work (Social OR Physical challenge) or sleep rough
- **Resources**: 6-point pools (Health, Focus, Stamina), Hunger, Coins
- **Time System**: 24 segments per day (6 blocks × 4 segments)

## Work Completed

### 1. Extended Cost/Reward System ✅ COMPLETE

**Files Modified:**
- `src/GameState/ChoiceCost.cs` ✅
- `src/Content/DTOs/ChoiceCostDTO.cs` ✅
- `src/GameState/ChoiceReward.cs` ✅
- `src/Content/DTOs/ChoiceRewardDTO.cs` ✅
- `src/Content/Parsers/SceneTemplateParser.cs` ✅

**Changes:**
- Added Health property (physical damage/healing)
- Added Hunger property (food/exertion system)
- Added Stamina property (6-point pool for Physical challenges)
- Added Focus property (6-point pool for Social/Mental challenges)
- Parser fully updated with mappings for all new fields

**Rationale:**
- Tutorial requires granular resource management across multiple pools
- Original ChoiceCost only had Coins/Resolve/TimeSegments
- New tutorial needs Health/Stamina/Focus costs for challenge-based gameplay

**Status:** ✅ COMPLETE

### 2. Time Advancement System ✅ COMPLETE

**Files Modified:**
- `src/GameState/TimeBlocks.cs` ✅
- `src/GameState/ChoiceReward.cs` ✅
- `src/Content/DTOs/ChoiceRewardDTO.cs` ✅
- `src/Content/Parsers/SceneTemplateParser.cs` ✅

**Changes:**
- Added DayAdvancement enum (CurrentDay, NextDay)
- Added AdvanceToBlock property to ChoiceReward (nullable TimeBlocks)
- Added AdvanceToDay property to ChoiceReward (nullable DayAdvancement)
- Added ParseTimeBlock() helper method to parser
- Added ParseDayAdvancement() helper method to parser
- Parser fully maps time advancement properties

**Rationale:**
- AutoAdvance scenes need to jump to specific time blocks (e.g., sleep until morning)
- Normal TimeSegments insufficient for major time jumps
- Tutorial Night Rest scene needs to advance from Evening → Morning + NextDay

**Status:** ✅ COMPLETE

**CRITICAL DISCOVERY:** Existing time system has 16 segments per day (4 blocks × 4 segments), not 24 as tutorial spec said. Working with actual codebase: Morning, Midday, Afternoon, Evening.

### 3. AutoProgressRewards System ✅ COMPLETE

**Files Modified:**
- `src/GameState/SituationTemplate.cs` ✅
- `src/Content/DTOs/SituationTemplateDTO.cs` ✅
- `src/Content/Parsers/SceneTemplateParser.cs` ✅

**Changes:**
- Added AutoProgressRewards property to SituationTemplate (ChoiceReward type)
- Added AutoProgressRewards property to SituationTemplateDTO (ChoiceRewardDTO type)
- Parser maps AutoProgressRewards using existing ParseChoiceReward() method

**Rationale:**
- AutoAdvance scenes have no player choices
- Rewards must apply automatically when scene activates
- Night Rest scene uses this to restore all resources and advance time

**Status:** ✅ COMPLETE

### 2. Parser Integration (IN PROGRESS)

**File:** `src/Content/Parsers/SceneTemplateParser.cs`

**Completed:**
- ParseChoiceCost() method: Added mapping for Health, Hunger, Stamina, Focus fields (lines 313-316)

**Remaining:**
- ParseChoiceReward() method: Need to add same field mappings

## Work Remaining

### Phase 1: Complete Parser Integration (30 min)
- Finish ParseChoiceReward() mapping
- Build and verify no compilation errors

### Phase 2: Challenge System Integration (2-3 hours)
**Critical Gap:** Tutorial requires ActionType = StartChallenge pattern

**Investigation Needed:**
- How do Social/Physical challenges currently start?
- Where is challenge completion rewarded?
- How do Focus/Stamina consumption mechanics work?

**Implementation Needed:**
- Add ChallengeCost properties to ChoiceCost OR create separate ChallengeCost class
  - ChallengeType (Social/Physical/Mental)
  - FocusConsumption (for Mental/Social)
  - StaminaConsumption (for Physical)
  - HealthRisk (Low/Medium/High for Physical)
- Connect Choice execution → Challenge initiation
- Ensure challenge completion applies ChoiceReward

### Phase 3: Tutorial Content Authoring (2 hours)

**Create 4 Tutorial Scenes:**

**Scene 1: Evening Arrival**
- PlacementFilter: NPCRole = TAVERN_KEEPER
- 3 Choices:
  1. Elena's Evening Service (ActionType: StartChallenge Social, Cost: 1 Focus, Reward: 5 coins + Friendly + spawn Scene 2)
  2. Thomas's Warehouse Work (ActionType: StartChallenge Physical, Cost: 1 Stamina, Reward: 5 coins + Friendly + spawn Scene 2)
  3. Sleep Rough (ActionType: Instant, Cost: none, Reward: -2 Health + Disappointed + spawn alt scene)

**Scene 2: Secure Room**
- PlacementFilter: NPCRole = TAVERN_KEEPER (spawned by Scene 1)
- 1 Choice:
  - Pay Room Cost (Requirement: 10 coins, Cost: 10 coins, Reward: spawn Scene 3)

**Scene 3: Night Rest**
- PlacementFilter: LocationRole = TAVERN_BEDROOM (spawned by Scene 2)
- ActionType: AutoAdvance (NEW PATTERN - no choices, auto-executes)
- AutoProgressRewards:
  - Health → 6 (max)
  - Focus → 6 (max)
  - Stamina → 6 (max)
  - Time → Day 2, Morning Block
  - Spawn Scene 4

**Scene 4: Day 2 Morning**
- PlacementFilter: LocationRole = TAVERN_COMMON_ROOM (spawned by Scene 3)
- ActionType: WorldStateReveal (NEW PATTERN - exposition, no choices)
- Purpose: Tutorial complete, full game begins

**Files to Create:**
- `src/Content/Tutorial/01_tutorial_scenes.json`
- `src/Content/Tutorial/02_tutorial_npcs.json` (Elena, Thomas)
- `src/Content/Tutorial/03_tutorial_locations.json` (Tavern, Common Room, Bedroom, Town Square)

### Phase 4: AutoAdvance Scene Support (1-2 hours)

**New Pattern Required:** Some scenes progress without player input

**Implementation:**
- Add ActionType.AutoAdvance enum value
- SceneFacade detects AutoAdvance situations
- Automatically applies AutoProgressRewards
- Advances to next scene without player interaction

**Used By:** Scene 3 (Night Rest) - restores all resources and advances to Day 2

### Phase 5: SceneSpawn Integration (1 hour)

**Connect Executors:**
- LocationActionExecutor.Execute() → SceneInstantiator.CreateProvisionalScene()
- NPCActionExecutor.Execute() → SceneInstantiator.CreateProvisionalScene()
- On commit → SceneInstantiator.FinalizeScene()
- On abandon → SceneInstantiator.DeleteProvisionalScene()

**Pattern:** Perfect information - player sees WHERE scene spawns BEFORE committing to choice

### Phase 6: Testing (1 hour)

**End-to-End Flow:**
1. Start game → Evening Segment 1, player has 8 coins
2. Scene 1 appears (Elena/Thomas/Sleep choices)
3. Select Elena → Social Challenge starts
4. Complete challenge → gain 5 coins, Elena becomes Friendly
5. Scene 2 spawns (Secure Room)
6. Pay 10 coins → Scene 3 spawns (Night Rest)
7. Scene 3 auto-advances → resources restore to 6, time advances to Day 2 Morning
8. Scene 4 spawns (World Opens) → tutorial complete

## Architectural Decisions

### Challenge-Based Actions (NEW)
- Choices can start challenges, not just apply instant rewards
- Challenge completion applies the ChoiceReward
- This is a MAJOR pattern addition to the action system

### AutoAdvance Scenes (NEW)
- Some scenes have no player choices
- Scene automatically applies rewards and spawns next scene
- Used for narrative transitions (sleep, travel, forced progression)

### 6-Point Resource Pools
- Health, Focus, Stamina use 0-6 scale
- Not arbitrary integers - represents physical/mental capacity
- Rest restores to maximum (6)
- May need resource cap validation

### Time Segment Mechanics
- 24 segments per day (4 per block, 6 blocks)
- Actions cost 1-4 segments
- Blocks: Morning, Midday, Afternoon, Evening, Night, LateNight
- TimeService integration required for segment advancement

## Key Files to Understand Before Next Session

**Challenge System:**
- `src/Facades/ChallengeFacade.cs` - Challenge initiation/execution
- `src/GameState/SocialChallenge.cs` - Social challenge state
- `src/GameState/PhysicalChallenge.cs` - Physical challenge state
- `src/Services/Executors/*ActionExecutor.cs` - Action execution patterns

**Scene System:**
- `src/Facades/SceneFacade.cs` - Scene query and situation activation
- `src/Services/SceneInstantiator.cs` - Provisional/finalized scene lifecycle
- `src/GameState/SceneTemplate.cs` - Template domain entity
- `src/GameState/Scene.cs` - Instance domain entity

**Time System:**
- `src/Services/TimeService.cs` - Time advancement logic
- `src/GameState/TimeBlock.cs` - Time block enum
- `src/GameState/Player.cs` - Current time state

## Technical Debt Created

**None yet** - All changes follow existing patterns:
- ChoiceCost/ChoiceReward extended with new properties (YAGNI-compliant)
- DTOs extended in parallel (parser-JSON-entity triangle maintained)
- Parser mappings added (no logic changes)

## Risks Identified

**Medium Risk: Challenge Integration Complexity**
- Challenge system may have complex state management
- Uncertain how Focus/Stamina consumption currently works
- May need significant refactoring to connect Choice → Challenge

**Low Risk: AutoAdvance Pattern**
- Conceptually simple (just skip player input)
- Implementation straightforward (check ActionType, auto-execute)

**Low Risk: Content Authoring**
- Follows existing JSON patterns
- PlacementFilter, ChoiceTemplate, SceneSpawn all exist

## Questions for Next Session

1. **Challenge Initiation:** How does ActionType.StartChallenge currently work? Does it exist?
2. **Resource Consumption:** Where is Focus/Stamina consumption logic? ChallengeFacade?
3. **Challenge Completion:** Where are challenge rewards applied? ChallengeFacade.CompleteChallenge()?
4. **AutoAdvance:** Does ActionType.AutoAdvance exist? Or create new?
5. **Time Advancement:** How does ChoiceCost.TimeSegments integrate with TimeService?

## Estimated Time to Complete

**Total Remaining: 7-9 hours**
- Phase 1 (Parser): 30 min
- Phase 2 (Challenge Integration): 2-3 hours ⚠️ HIGH COMPLEXITY
- Phase 3 (Content Authoring): 2 hours
- Phase 4 (AutoAdvance): 1-2 hours
- Phase 5 (SceneSpawn): 1 hour
- Phase 6 (Testing): 1 hour

## Success Criteria

Tutorial is complete when:
- ✅ Player starts Evening with 8 coins (2 short of room cost)
- ✅ Three choices available (Elena Social, Thomas Physical, Sleep Rough)
- ✅ Elena choice → Social Challenge → 5 coins + Friendly status
- ✅ Thomas choice → Physical Challenge → 5 coins + Friendly status
- ✅ Sleep Rough → -2 Health + Disappointed status
- ✅ Secure Room scene spawns after work completed
- ✅ Pay 10 coins → Night Rest scene spawns
- ✅ Night Rest auto-advances → all resources restore to 6
- ✅ Time advances to Day 2 Morning
- ✅ Day 2 world opens (full game begins)

## Notes

- Tutorial design is MUCH simpler than original 5-scene spec
- Focus on teaching resource scarcity and work safety net
- Day 2 reveals full game systems (deliveries, investigations, equipment)
- Tutorial constrains to ONE decision: How to earn 2 coins for shelter?
