# Session Handoff - Unified Action System Implementation & Design Analysis

## Session Date: 2025-08-11

## What Was Requested
User requested implementation of a unified action system where location and conversation actions share the same attention resource pool, with tier systems for progression and binary availability (actions either available or not, no variable costs).

## What Was Implemented

### ✅ Successfully Completed:

1. **Refactored LocationTags.cs**
   - Removed all HashSet usage (user requirement: no HashSet/Dictionary)
   - Replaced with List<string> and strongly-typed structures
   - Added TierLevel enum and TierRequirement class

2. **Linked Attention Pools**
   - ActionGenerator and ConversationChoiceGenerator now share TimeBlockAttentionManager
   - Attention persists within time blocks (Dawn, Morning, Afternoon, Evening, Night, LateNight)
   - Prevents "infinite conversation exploit" where players could reset attention

3. **Added Tier System (T1-T3)**
   - **ActionGenerator**: Location actions check player tier with lock reasons
   - **ConversationChoiceGenerator**: Conversation choices respect tier requirements
   - Tier names: T1 (Stranger), T2 (Associate), T3 (Confidant)
   - Binary availability: actions either available or locked

4. **Created Binary Availability Checker**
   - Centralized BinaryAvailabilityChecker class
   - Ensures all actions have binary availability (no variable costs)
   - Returns simple IsAvailable bool with LockReason string

5. **Implemented Letter Tier System**
   - Letters have T1-T3 tiers determining complexity/reward
   - Special letters (permits, introductions) are T3 and don't queue
   - Player tier determines what letters they can accept

6. **Implemented Route Tier System**
   - Routes have T1-T3 tier requirements
   - Transport permits (special letters) unlock higher tier routes
   - Binary access: route available or locked

7. **Created AI Prompt Templates**
   - AIPromptTemplates class for narrative generation
   - Production-focused approach (39 hours vs 126 hours)
   - Templates for conversations, reactions, locations, letters

8. **Cleaned Up Legacy Code**
   - Removed test controllers
   - Removed temporary documentation files
   - Fixed compilation errors
   - Build succeeds with 0 errors

## What's Wrong (Design Analysis)

### 🔴 Critical Misalignments with Vision Document:

1. **Emotional State Calculation COMPLETELY WRONG**
   - **Current**: Complex weighted math (30% letter, 25% tokens, 20% history)
   - **Should Be**: Simple formula: Stakes + Time = State
   - **Example**: Personal Safety + <6h = DESPERATE (not calculated)

2. **Token Types Have No Distinct Mechanics**
   - **Current**: All tokens work identically (just numbers)
   - **Should Be**: 
     - Trust: Affects deadlines (patience/faith)
     - Commerce: Affects queue position (priority)  
     - Status: Affects letter quality/access
     - Shadow: Affects information visibility

3. **Queue Manipulation Wrong**
   - **Current**: Generic token burning
   - **Should Be**: Position-based costs (move 3 positions = burn 3 tokens)

4. **No Systemic Interconnection**
   - **Current**: Systems work in isolation
   - **Should Be**: Letter properties → States → Choices → Tokens → Queue → Letters
   - Every system should feed every other system

5. **Choices Don't Emerge from State**
   - **Current**: Hardcoded choices with complex checks
   - **Should Be**: DESPERATE state enables desperate choices, HOSTILE blocks trust

6. **Literary Presentation Missing**
   - **Current**: Shows mechanics ("burn 2 Commerce tokens")
   - **Should Be**: Shows moments ("take her trembling hand")
   - Players see spreadsheet, not medieval life

## Production Scope to Fix

### Phase 1: Simplify Core (16 hours)
- Replace NPCEmotionalStateCalculator with Stakes + Time formula
- Delete all weighted calculations
- Keep existing content temporarily

### Phase 2: Fix Token Mechanics (16 hours)
- Give each token type distinct mechanical effects
- Update 30+ call sites
- Test each token type separately

### Phase 3: Content Reduction (12 hours)
- Reduce to 5 NPCs (currently 7)
- Replace hardcoded JSON with AI templates
- Create ~1000 words seed content

### Phase 4: Polish (10 hours)
- Fix UI to hide mechanics
- Update tests
- Performance optimization

**Total: 54 hours** (1-2 weeks for small team)

## Key Files Modified

### Implementation Files:
- `/mnt/c/git/wayfarer/src/GameState/LocationTags.cs` - Removed HashSet
- `/mnt/c/git/wayfarer/src/GameState/TimeBlockAttentionManager.cs` - Shared attention
- `/mnt/c/git/wayfarer/src/Services/ActionGenerator.cs` - Tier checking
- `/mnt/c/git/wayfarer/src/Game/ConversationSystem/VerbOrganizedChoiceGenerator.cs` - Tier checking
- `/mnt/c/git/wayfarer/src/Services/BinaryAvailabilityChecker.cs` - Binary availability
- `/mnt/c/git/wayfarer/src/GameState/Letter.cs` - Letter tiers
- `/mnt/c/git/wayfarer/src/Game/MainSystem/RouteOption.cs` - Route tiers

### Files That Need Fixing:
- `/mnt/c/git/wayfarer/src/Game/ConversationSystem/NPCEmotionalStateCalculator.cs` - Needs complete rewrite
- `/mnt/c/git/wayfarer/src/GameState/ConnectionTokenManager.cs` - Needs distinct mechanics
- `/mnt/c/git/wayfarer/src/GameState/LetterQueue.cs` - Needs position-based costs

## Critical Design Insights from Vision

1. **Emotional States = Stakes + Time** (not complex calculations)
   - Personal Safety + <6h = DESPERATE
   - Reputation + <12h = ANXIOUS
   - Wealth = CALCULATING
   - No letter = NEUTRAL

2. **Every System Interconnects**
   - Letter properties determine NPC states
   - States enable specific conversations
   - Conversations create obligations
   - Obligations modify queue rules
   - Queue affects all future interactions

3. **Players Never See the Machinery**
   - Show "trembling hands" not "DESPERATE state"
   - Show "worn purse" not "-3 Commerce tokens"
   - Literary presentation over gamey mechanics

## Agent Consensus

- **Chen (Game Design)**: Current implementation has mechanics but misses tension. Needs simple rules that create depth through interconnection.
- **Kai (Systems)**: Too much float arithmetic. Need integer lookups and deterministic rules.
- **Jordan (Narrative)**: Shows spreadsheet not story. Mechanics should generate narrative, not display numbers.
- **Alex (Production)**: 54 hours to fix. Simplification is easier than current complexity.
- **Priya (UI/UX)**: Would focus on hiding mechanics behind literary presentation.

## Next Steps

1. **Immediate**: Simplify NPCEmotionalStateCalculator to Stakes + Time
2. **Priority**: Give each token type distinct mechanics
3. **Important**: Make queue manipulation position-based
4. **Polish**: Hide all mechanics behind literary presentation

## Session Notes

- User emphasized "use your agents for everything"
- No HashSet, Dictionary, untyped structures, functions, or delegates allowed
- Must refactor existing code, not create new
- Tier systems are REQUIRED, not optional
- Binary availability only (no variable costs)
- Test with Playwright
- Adhere to CLAUDE.md and game-architecture.md

---
*End of Session Handoff*