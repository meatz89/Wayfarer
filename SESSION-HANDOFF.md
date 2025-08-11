# Session Handoff - Complete Wayfarer Implementation Fix

## Session Summary (Date: 2025-08-11)

### Previous Context
User implemented unified action system but discovered fundamental misalignments with the vision document. The current implementation has complex calculations when it should have simple, interconnected mechanics.

### Critical Issues Identified

#### 1. Emotional State Calculation WRONG
- **Current**: Complex weighted math (30% letter, 25% tokens, 20% history)
- **Should Be**: Simple formula: Stakes + Time = State
  - Personal Safety + <6h = DESPERATE
  - Reputation + <12h = ANXIOUS
  - Wealth = CALCULATING
  - No letter = NEUTRAL

#### 2. Token Types Have No Distinct Mechanics
- **Current**: All tokens work identically (just numbers)
- **Should Be**:
  - **Trust**: Affects deadlines (+2h per token)
  - **Commerce**: Affects queue entry position
  - **Status**: Affects letter tier access
  - **Shadow**: Reveals hidden information

#### 3. Queue Manipulation Wrong
- **Current**: Generic token burning
- **Should Be**: Position-based costs (move 3 positions = 3 tokens)

#### 4. No Systemic Interconnection
- **Current**: Systems work in isolation
- **Should Be**: Letter properties → States → Choices → Tokens → Queue → Letters

### Agent Consensus on Fix

#### Implementation Approach (80 hours total)
1. **Phase 1**: Simplify emotional states (8h)
2. **Phase 2**: Token mechanics with debt system (15h)
3. **Phase 2.5**: Consequence preview UI (10h)
4. **Phase 3**: Queue manipulation (10h)
5. **Phase 4**: Letter properties integration (15h)
6. **Phase 5**: Literary presentation layer (15h)
7. **Phase 6**: Content and testing (7h)

#### Critical Requirements
- **Vertical Slice First**: One complete path (1 NPC, 1 location, 3 verbs)
- **Token Debt Must Be Systemic**: Debt affects ALL interactions globally
- **Preview System Mandatory**: Players must see consequences before acting
- **Delete First, Build Second**: Remove complex systems entirely

### Implementation Plan

#### Files to DELETE
- NPCEmotionalStateCalculator.cs (270 lines of complexity)
- LeverageCalculator.cs (entire leverage system)
- All weighted calculation code

#### Files to CREATE
- SimplifiedEmotionalStateCalculator.cs (~50 lines)
- TokenEffectRegistry.cs (~100 lines)
- LetterPropertyHub.cs (~150 lines)
- ConsequencePreviewSystem.cs (~200 lines)

#### Files to REFACTOR
- LetterQueueManager.cs (position-based costs)
- ConnectionTokenManager.cs (unique token mechanics)
- ConversationChoiceGenerator.cs (state-driven choices)

### Current Implementation Status

#### Completed
✅ Unified action system architecture
✅ Tier system (T1-T3) implementation
✅ Binary availability checker
✅ Shared attention pools
✅ Route and letter tier systems

#### In Progress
🔄 Vertical slice implementation (Elena, Copper Kettle, 3 verbs)
🔄 Simplifying emotional state calculator

#### Pending
⏳ Token mechanics with debt system
⏳ Consequence preview UI
⏳ Position-based queue costs
⏳ Literary presentation layer
⏳ Full system interconnection

### Key Design Principles (From Vision)

1. **Emotional States = Stakes + Time** (not calculations)
2. **Every System Interconnects** (no isolation)
3. **Players Never See Machinery** (literary presentation)
4. **Mechanics Generate Narrative** (not skin over mechanics)
5. **Simple Rules Create Depth** (through interconnection)

### Production Notes

- **Vertical Slice by Hour 20**: Critical milestone
- **Save System Will Break**: Needs complete rewrite (+5h)
- **Content Creation**: 45 conversation combinations needed
- **Testing Buffer**: Add 50% to all estimates

### Agent Insights

- **Chen**: "Token debt must hurt EVERYWHERE or tension collapses"
- **Jordan**: "Mechanics are story - don't hide them, celebrate them"
- **Alex**: "Vertical slice or we're in trouble"
- **Priya**: "Preview everything - players must feel clever, not lucky"
- **Kai**: "Delete first, build second - legacy code poisons implementation"

### Next Immediate Steps

1. Create SimplifiedEmotionalStateCalculator
2. Implement Elena vertical slice
3. Add token debt system
4. Build consequence preview
5. Test with Playwright

---
*Session continues with full implementation...*