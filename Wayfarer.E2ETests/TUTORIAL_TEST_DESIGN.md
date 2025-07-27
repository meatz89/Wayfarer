# Tutorial E2E Test Design

## Overview

This comprehensive test suite validates the entire tutorial system integration, ensuring all 57 tutorial steps work correctly and the tutorial provides a smooth onboarding experience.

## Test Categories

### 1. Core System Tests

**Test_01_TutorialAutoStart**
- Verifies tutorial starts automatically on new game
- Checks initial flags are set correctly
- Validates player state is forced to tutorial starting conditions

**Test_02_NarrativeOverlayState**
- Ensures overlay has all required data (name, description, guidance)
- Validates progress tracking (step X of Y)
- Checks overlay visibility state

**Test_03_CommandFiltering**
- Verifies only allowed actions are available
- Tests command discovery filtering
- Validates action restrictions per step

**Test_04_ActionRestrictions**
- Tests that non-allowed commands fail with appropriate messages
- Verifies allowed commands succeed
- Checks error messages indicate tutorial restrictions

**Test_05_DialogueOverrides**
- Validates NPC dialogue is overridden during specific steps
- Tests override priority over default dialogue
- Ensures overrides are step-specific

### 2. Tutorial Step Tests (Key Steps)

**Day 1 Tests**
- Test_06_Day1_WakeUp: Initial step, location restrictions
- Test_07_Day1_MeetMartha: NPC visibility, dialogue
- Test_08_Day1_FirstWork: Work action availability
- Test_09_Day1_LetterBoard: Letter board override mechanics
- Test_10_Day1_FirstDelivery: Delivery mechanics
- Test_11_Day1_PersonalLetter: Trust building
- Test_12_Day1_Rest: Rest mechanics

**Day 2 Tests**
- Test_13_Day2_UrgentDelivery: Time-sensitive deliveries
- Test_14_Day2_TrustTokens: Trust token gain/spend
- Test_15_Day2_Obligations: Obligation acceptance
- Test_16_Day2_RiskyDelivery: Special delivery types

**Day 3 Tests**
- Test_17_Day3_PatronIntroduction: Patron system intro
- Test_18_Day3_PatronageAcceptance: Employment mechanics
- Test_19_Day3_QueuePriority: Patron letter priority
- Test_20_TutorialCompletion: Completion and unlock

### 3. Integration Tests

**Test_21_SaveLoadDuringTutorial**
- Save/load preserves tutorial progress
- Tutorial resumes at correct step
- All restrictions maintained after load

**Test_22_UIElementVisibility**
- UI elements show/hide based on tutorial progress
- Letter queue, trust tokens, obligations visibility
- UI state persists through save/load

**Test_23_NPCVisibilityControl**
- NPCs appear only when specified by step
- Visibility transitions smoothly between steps
- All NPCs visible after tutorial completion

**Test_24_LocationRestrictions**
- Travel limited to allowed locations
- Visible locations match step definition
- All locations accessible after tutorial

**Test_25_SpecialItemHandling**
- Tutorial items (satchel) given at right time
- Item effects apply correctly (queue size increase)
- Narrative items distributed properly

**Test_26_ProgressTracking**
- Step index increments correctly
- Total step count accurate
- Progress UI updates properly

**Test_27_ErrorRecovery**
- Handles invalid step IDs gracefully
- Recovers from corrupted state
- Missing data doesn't crash tutorial

### 4. Edge Case Tests

**TutorialOverlayRendering**
- Overlay updates on step changes
- Hides after tutorial completion
- All required fields populated

**CommandFilteringEdgeCases**
- Empty allowed actions list
- Invalid actions in allowed list
- Case sensitivity handling
- Partial action name matches

**StepTransitionRaceConditions**
- Concurrent step progressions
- Save during transitions
- Multiple progression checks

**MultipleNarrativeConflicts**
- Can't start other narratives during tutorial
- Tutorial takes priority
- Clean transition after completion

**DialogueOverridePriority**
- Overrides take precedence
- Fallback to default when no override
- Per-step override isolation

**LetterBoardOverrideMechanics**
- Shows only tutorial letters
- Acceptance restrictions work
- Board returns to normal after tutorial

**PatronQueuePriorityEdgeCases**
- Patron letters force into full queue
- Multiple patron letters stack correctly
- Regular letters pushed down

**TrustTokenEdgeCases**
- Can't spend more than available
- Trust never goes negative
- Zero trust handled gracefully

**TimeSensitiveStepHandling**
- Time limits enforced
- Consequences for delays
- Rewards/penalties apply correctly

**CompletionEffectOrdering**
- Multiple effects apply in order
- All effect types work
- Tutorial completion effects comprehensive

## Key Assertions

Each test validates specific behaviors:

1. **State Consistency**: Tutorial state remains valid through all operations
2. **Progression Logic**: Steps advance only when requirements met
3. **Restriction Enforcement**: Action/visibility limits properly applied
4. **Data Integrity**: All required data present and valid
5. **Error Handling**: Graceful failure and recovery
6. **Save/Load Safety**: Tutorial state preserved correctly
7. **UI Synchronization**: UI reflects tutorial state accurately
8. **Completion Effects**: Full game properly unlocked

## Test Execution

Run all tests with:
```bash
./RunComprehensiveTutorialTests.sh
```

This executes:
1. Main comprehensive test suite (27 core tests)
2. Edge case test suite (15 specialized tests)
3. Provides detailed results and summary

## Expected Outcomes

When all tests pass:
- Tutorial auto-starts for new players
- All 57 steps can be completed in sequence
- Action restrictions enforce learning path
- UI guides player appropriately
- Save/load works at any point
- Tutorial completion unlocks full game
- Edge cases handled gracefully
- No crashes or invalid states

## Common Issues Caught

These tests will catch:
- Tutorial not starting automatically
- Steps not progressing when requirements met
- Command filtering not working
- Dialogue overrides not applying
- UI elements showing at wrong times
- NPC visibility issues
- Save/load breaking tutorial state
- Patron letter priority problems
- Trust token calculation errors
- Tutorial completion not unlocking content