# SYSTEM FIX IMPLEMENTATION PLAN
## Aligning Wayfarer with the Vision Document

### EXECUTIVE SUMMARY

**Current State**: Systems are overcomplicated and disconnected from the vision
**Goal**: Simplify all systems to match the elegant design in IMPLEMENTATION-PLAN.md
**Timeline**: 5 phases, ~40 hours total
**Approach**: Systematic refactoring, not rewriting from scratch

---

## PHASE 1: EMOTIONAL STATE SIMPLIFICATION (8 hours)

### Problem
Current emotional state uses complex weighted calculations with 5+ factors instead of simple "Stakes + Time" lookup.

### Solution
Replace NPCEmotionalStateCalculator with simple lookup table:

```csharp
EmotionalState = (Stakes, TimeRemaining) switch {
    (URGENT, < 2h) => DESPERATE,
    (VALUABLE, < 2h) => DESPERATE,
    (DANGEROUS, < 4h) => DESPERATE,
    (URGENT, < 6h) => ANXIOUS,
    (VALUABLE, < 6h) => ANXIOUS,
    (DANGEROUS, < 8h) => ANXIOUS,
    (_, _) when hasLetter => CALCULATING,
    _ => NEUTRAL
}
```

### Implementation Steps
1. **Create SimplifiedEmotionalStateCalculator.cs** (2h)
   - Pure function: (Letter, TimeRemaining) → EmotionalState
   - No dependencies on tokens, obligations, history
   - Clear lookup table matching vision

2. **Replace all usages** (3h)
   - Update ConversationChoiceGenerator
   - Update UI components
   - Update narrative generation

3. **Delete old system** (1h)
   - Remove NPCEmotionalStateCalculator
   - Remove all weighted calculation code
   - Clean up dependencies

4. **Test with Playwright** (2h)
   - Verify emotional states change correctly
   - Confirm UI updates properly
   - Test all 4 states with different stakes

### Success Criteria
- Emotional state = simple function of Stakes + Time
- No complex calculations or weights
- All tests pass

---

## PHASE 2: TOKEN MECHANICS DIFFERENTIATION (10 hours)

### Problem
All token types work identically. They should have distinct mechanics per the vision.

### Solution
Implement unique mechanics for each token type:

```csharp
// Trust: Affects deadlines
letter.DeadlineInHours += trustTokens * 2;

// Commerce: Affects queue entry position  
int entryPosition = Math.Max(1, basePosition - commerceTokens);

// Status: Gates letter tiers
bool canAccessTier3 = statusTokens >= 5;

// Shadow: Reveals information
if (shadowTokens >= requiredShadow) RevealInfo();
```

### Implementation Steps

1. **Trust Token Mechanics** (2h)
   - Modify LetterQueueManager.AddLetter()
   - Add deadline extension: +2h per Trust token
   - Show in UI: "Elena's trust grants +4h"

2. **Commerce Token Mechanics** (2h)
   - Modify queue entry position calculation
   - Each Commerce token = -1 position (closer to front)
   - Cap at position 1

3. **Status Token Mechanics** (3h)
   - Add tier checking to letter generation
   - Gate high-tier letters behind Status requirements
   - T1: 0 Status, T2: 3 Status, T3: 5 Status

4. **Shadow Token Mechanics** (2h)
   - Add information gating to InvestigateEffects
   - Each secret requires Shadow threshold
   - Show locked info: "Requires 3 Shadow tokens"

5. **Test all token types** (1h)
   - Verify each has unique behavior
   - Confirm no shared mechanics
   - Test edge cases

### Success Criteria
- Each token type has distinct, visible mechanics
- Players can see different effects immediately
- Token choices become strategic

---

## PHASE 3: QUEUE MANIPULATION SIMPLIFICATION (8 hours)

### Problem
Queue manipulation uses complex leverage calculations instead of simple "Cost = Positions Moved"

### Solution
Implement position-based costs:

```csharp
// Moving letter from position 5 to position 2
int cost = 5 - 2; // 3 tokens
tokenManager.SpendTokens(letterTokenType, cost);
queueManager.MoveToPosition(letterId, 2);
```

### Implementation Steps

1. **Simplify LetterReorderEffect** (2h)
   - Remove leverage calculations
   - Cost = Math.Abs(currentPos - targetPos)
   - Use letter's token type for payment

2. **Update ConversationChoiceGenerator** (2h)
   - Generate reorder options for positions 1-3
   - Show clear costs: "Move to #1 (3 Commerce)"
   - Only show if player has enough tokens

3. **Remove LeverageCalculator** (2h)
   - Delete entire leverage system
   - Remove all references
   - Simplify queue entry to use Commerce tokens only

4. **Update UI to show costs** (1h)
   - Display position-based costs clearly
   - Show token requirements
   - Preview final position

5. **Test queue mechanics** (1h)
   - Verify costs are correct
   - Test multiple movements
   - Confirm token spending

### Success Criteria
- Queue manipulation cost = positions moved
- No complex leverage calculations
- Clear, predictable costs

---

## PHASE 4: SYSTEM INTERCONNECTION (8 hours)

### Problem
Systems operate in isolation. Everything should flow through letter properties.

### Solution
Make all systems read from letter properties:

```csharp
// Everything derives from the letter
var letter = queue.GetLetterAt(1);
var emotionalState = GetState(letter.Stakes, letter.DeadlineInHours);
var choices = GenerateChoices(letter, emotionalState);
var tokenEffects = ApplyTokenMechanics(letter.TokenType);
```

### Implementation Steps

1. **Create LetterPropertySystem** (3h)
   - Central class that reads letter properties
   - Generates all derived state
   - Single source of truth

2. **Update all systems to use letters** (3h)
   - ConversationChoiceGenerator reads from letters
   - EmotionalState derived from letters
   - Token effects based on letter type

3. **Remove isolated calculations** (1h)
   - Delete standalone state managers
   - Remove cached calculations
   - Simplify data flow

4. **Test interconnections** (1h)
   - Verify changing letter affects all systems
   - Test cascade effects
   - Confirm no isolated state

### Success Criteria
- All mechanics derive from letter properties
- Changing a letter updates everything
- No isolated state or calculations

---

## PHASE 5: LITERARY PRESENTATION (6 hours)

### Problem
Mechanics are visible as numbers and calculations instead of narrative.

### Solution
Hide all mechanics behind literary descriptions:

```csharp
// Instead of: "+3 Trust tokens"
"Elena's eyes soften with gratitude"

// Instead of: "Move to position 1 (cost: 3)"
"Call in favors to prioritize this letter"

// Instead of: "Deadline: 2 hours"
"The wax seal burns with urgency"
```

### Implementation Steps

1. **Create NarrativeWrapper** (2h)
   - Converts all mechanical effects to narrative
   - Token gains → relationship descriptions
   - Position changes → favor calling
   - Deadlines → urgency metaphors

2. **Update UI components** (2h)
   - Replace mechanical text with narrative
   - Hide numbers behind descriptions
   - Show consequences as story beats

3. **Polish conversation choices** (1h)
   - Remove mechanical previews
   - Use narrative hints for costs
   - Frame everything as story

4. **Test narrative flow** (1h)
   - Play full conversation
   - Verify no numbers visible
   - Confirm mechanics still work

### Success Criteria
- No visible numbers or calculations
- All mechanics expressed through narrative
- Player understands effects through story

---

## DEPENDENCIES & RISKS

### Dependencies Between Phases
- Phase 2 depends on Phase 1 (emotional states affect token mechanics)
- Phase 3 can run parallel to Phase 2
- Phase 4 requires Phases 1-3 complete
- Phase 5 can begin after Phase 4

### Technical Risks
1. **Breaking existing gameplay**: Mitigate with comprehensive Playwright tests
2. **Save game compatibility**: Accept breaking changes, bump version
3. **Missing edge cases**: Extensive testing after each phase

### Time Risks
- Each phase estimated conservatively
- Buffer time included for testing
- Can pause between phases if needed

---

## DEFINITION OF DONE

### System-Level Success
✅ Emotional states use simple Stakes + Time lookup  
✅ Each token type has unique mechanics  
✅ Queue manipulation costs = positions moved  
✅ All systems interconnect through letters  
✅ Mechanics hidden behind narrative  

### Code-Level Success
✅ Removed all complex calculations  
✅ Deleted leverage system entirely  
✅ No weighted factors or percentages  
✅ Clean, simple, readable code  
✅ Full test coverage  

### Player Experience Success
✅ Mechanics feel intuitive  
✅ Choices emerge from game state  
✅ No mental math required  
✅ Story drives understanding  
✅ Tension comes from time pressure  

---

## PHASE EXECUTION ORDER

### Week 1 (20 hours)
- Monday-Tuesday: Phase 1 - Emotional States (8h)
- Wednesday-Thursday: Phase 2 - Token Mechanics (10h)
- Friday: Testing & Bug Fixes (2h)

### Week 2 (20 hours)
- Monday-Tuesday: Phase 3 - Queue Simplification (8h)
- Wednesday-Thursday: Phase 4 - System Connection (8h)
- Friday: Phase 5 - Literary Presentation (4h)

### Week 3 (Buffer)
- Integration testing
- Polish and bug fixes
- Performance optimization
- Final validation

---

## IMMEDIATE NEXT STEPS

1. **Read all related files** to understand current implementation
2. **Create test scenarios** for current behavior
3. **Begin Phase 1** with SimplifiedEmotionalStateCalculator
4. **Test each change** with Playwright
5. **Commit after each successful phase**

---

## SUCCESS METRICS

### Quantitative
- Lines of code reduced by 50%+
- Complexity score reduced by 70%
- Test coverage at 80%+
- Zero weighted calculations
- All phases complete

### Qualitative
- Code is self-documenting
- New developers understand in <30 min
- Players never see math
- Choices feel natural
- Systems feel connected

---

## FINAL NOTES

This plan transforms Wayfarer from a complex simulation into the elegant game described in the vision. Each phase makes the game simpler, clearer, and more focused on the core tension of "too many letters, not enough time."

The key is discipline: resist adding complexity, delete rather than deprecate, and always return to the vision document when making decisions.

**Remember**: The game succeeds on elegant simplicity, not complex systems.