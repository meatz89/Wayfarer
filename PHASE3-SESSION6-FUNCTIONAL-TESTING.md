# Phase 3 Session 6: Functional System Integration Testing

## Overview

Test that all POC systems work together functionally and create the intended strategic gameplay experience through manual testing and gameplay validation.

## Testing Strategy

**NOT testing mathematical constraints** (these are design guidelines for content creation)
**Testing functional integration** - do the systems actually work as intended?
**Testing strategic experience** - does gameplay feel strategic and meaningful?

## Functional System Testing Areas

### 1. Route Blocking Functionality ⏳
**Goal**: Verify equipment requirements actually block/enable routes

**Test Cases**:
- Start game at millbrook with only Trade Tools
- Try to take Mountain Pass route → Should be blocked (requires Climbing Equipment)
- Buy Climbing Gear from workshop → Should now allow Mountain Pass
- Try Forest Trail without Navigation Tools → Should show warnings
- Test cart transport → Should block equipment-gated routes but add +3 slots

**Expected Results**:
- Route blocking messages appear when missing equipment
- Equipment purchase unlocks previously blocked routes
- Cart transport trade-off works (capacity vs access)

### 2. Contract Generation System ⏳
**Goal**: Test NPCs generate contracts from their categories

**Test Cases**:
- Start new day → Trigger contract refresh
- Check Workshop Master → Should offer Craft contracts
- Check Market Trader → Should offer Standard/Rush contracts
- Check Trade Captain → Should offer Rush/Exploration contracts
- Accept and complete contract → Should work with existing completion system

**Expected Results**:
- NPCs generate 1-2 contracts per day matching their categories
- Contract requirements are satisfiable with available items/routes
- Contract acceptance and completion flow works end-to-end

### 3. Inventory and Cart System ⏳
**Goal**: Test cart provides +3 slots bonus and inventory constraints work

**Test Cases**:
- Start with 4 base inventory slots → Try to carry 5+ items (should fail)
- Acquire cart transport → Should show 7 total slots available
- Test large items (Lumber, Pottery) → Should occupy 2 slots each
- Try to carry full equipment loadout + optimal cargo → Should exceed capacity

**Expected Results**:
- Base inventory: 4 slots enforced
- Cart bonus: +3 slots (7 total) when using cart transport
- Large items: 2 slots each, creates strategic choices
- Cannot carry optimal equipment AND optimal cargo (strategic pressure)

### 4. Equipment Commissioning Workflow ⏳
**Goal**: Test workshop system functional for equipment creation

**Test Cases**:
- Visit workshop with Trade Tools → Should be able to commission equipment
- Commission Climbing Gear (costs 5 coins) → Should take time blocks
- Commission Navigation Tools → Should work same way
- Try commissioning without Trade Tools → Should be blocked

**Expected Results**:
- Workshop requires Trade Tools for access
- Equipment commissioning costs 5 coins and time blocks
- Created equipment unlocks route access as expected

### 5. Time and Stamina Systems ⏳
**Goal**: Test travel/work consume resources correctly

**Test Cases**:
- Travel between locations → Should consume 1 stamina per time block
- Work on contracts → Should consume 2 stamina per time block
- Commission equipment → Should consume stamina and time
- Try to exceed 10 stamina → Should be blocked or require rest

**Expected Results**:
- Travel: 1 stamina per time block consumed
- Work: 2 stamina per time block consumed
- Daily stamina limit creates scheduling pressure
- Rest restores stamina for next day

## Strategic Experience Validation

### 6. Equipment vs Trading Choices ⏳
**Goal**: Validate meaningful strategic decisions emerge

**Test Scenarios**:
- **Scenario A**: Buy equipment immediately → Can access all routes but have less trading capital
- **Scenario B**: Trade first to build capital → Limited to cart routes initially
- **Scenario C**: Mixed approach → Some equipment, some trading

**Expected Results**:
- Each approach feels viable but different
- Equipment investment vs immediate profit creates ongoing tension
- No single approach is obviously superior

### 7. POC Gameplay Flow ⏳
**Goal**: Test player discovery and learning curve

**Test Flow**:
- **Day 1**: Simple delivery contract using cart routes
- **Discovery**: Try blocked route → Learn equipment requirements
- **Investment**: Save coins to buy equipment vs immediate trading
- **Mastery**: Develop personal strategy based on preferences

**Expected Results**:
- New players can understand and progress
- Equipment discovery happens naturally through route blocking
- Strategic learning curve feels engaging, not overwhelming

## Testing Methodology

### Manual Testing Approach
1. **Start fresh game**: Begin with POC starting conditions
2. **Play through scenarios**: Test each system through actual gameplay
3. **Document behavior**: Record what works, what doesn't, what feels strategic
4. **Fix functional issues**: Address any systems that don't work as intended
5. **Validate strategic feel**: Ensure optimization pressure feels engaging

### Test Execution Plan
1. **System functionality testing**: Verify all systems work technically
2. **Integration testing**: Test systems work together properly
3. **Strategic experience testing**: Validate gameplay feels meaningful
4. **Edge case testing**: Test boundary conditions and error cases
5. **Flow testing**: Test complete player journey from Day 1 onwards

## Success Criteria

### Functional Validation
- ✅ Route blocking works based on equipment categories
- ✅ Contract generation creates renewable strategic content
- ✅ Inventory system enforces capacity constraints meaningfully
- ✅ Equipment commissioning provides viable progression path
- ✅ Time/stamina systems create scheduling pressure

### Strategic Validation
- ✅ Equipment investment vs trading creates ongoing optimization tension
- ✅ Multiple viable strategies exist with distinct trade-offs
- ✅ Strategic decisions feel meaningful and impactful
- ✅ Gameplay complexity emerges from simple system interactions

### Experience Validation
- ✅ POC provides engaging strategic gameplay without overwhelming complexity
- ✅ Players can discover optimization opportunities through natural play
- ✅ Systems teach themselves through logical consequences
- ✅ Strategic depth emerges organically from content design principles

## Expected Issues and Solutions

### Potential Functional Issues
- **Route blocking not working**: Fix equipment requirement checking logic
- **Contract generation failing**: Debug NPC category parsing and template matching
- **Inventory constraints bypassed**: Enforce capacity limits properly
- **Time/stamina systems ignored**: Ensure consumption tracking works

### Potential Strategic Issues
- **Choices feel arbitrary**: Ensure decisions have logical consequences
- **One strategy dominates**: Rebalance to ensure multiple viable approaches
- **Optimization unclear**: Improve feedback to help players understand choices
- **Complexity overwhelming**: Simplify systems that don't add strategic value

## Documentation Deliverables

1. **Functional Test Results**: What works, what needs fixing
2. **Strategic Experience Report**: Does gameplay feel engaging and meaningful?
3. **Issue Resolution Log**: Problems found and solutions implemented
4. **POC Validation Status**: Is the POC ready for player experience testing?

This testing validates that the POC systems work together to create the intended strategic optimization gameplay experience.