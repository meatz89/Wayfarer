# Conversation System Refinement - Fix Plan

**Created**: 2025-10-01
**Status**: Ready for Implementation
**Validation Report Completeness**: 74% (50/68 criteria met)

## Executive Summary

Based on comprehensive validation of the conversation system implementation against `conversation-system-refinement.md`, we have identified **5 critical issues** that require immediate fixes:

1. **Diplomacy → Diplomacy Rename** (332 occurrences, 86 files)
2. **JSON Cards Missing "delivery" Property** (CRITICAL - blocks system)
3. **Authority Effect Formula Errors** (Understanding values too high + wrong Doubt system)
4. **Rapport Effect Formula Errors** (Missing Momentum secondaries)
5. **Diplomacy Foundation Initiative Cost** (0 instead of 1)

---

## Issue 1: Diplomacy → Diplomacy Rename

### Problem
The codebase uses "Diplomacy" throughout, but the specification uses "Diplomacy". This creates:
- Confusion when reading spec vs code
- Incorrect domain language (Diplomacy implies trade, Diplomacy implies negotiation)
- Inconsistency in documentation

### Scope
- **Files Affected**: 86 files
- **Total Occurrences**: 332
- **Categories**:
  - Enum values: `PlayerStatType.Diplomacy`
  - JSON properties: `"boundStat": "Diplomacy"`
  - Comments and documentation
  - UI display text
  - Method names and variables

### Fix Steps

#### 1.1 Rename Enum Value
**File**: `C:\Git\Wayfarer\src\GameState\Enums\PlayerStatType.cs`

```csharp
// BEFORE:
public enum PlayerStatType
{
    Insight,
    Rapport,
    Authority,
    Diplomacy,  // <- RENAME
    Cunning
}

// AFTER:
public enum PlayerStatType
{
    Insight,
    Rapport,
    Authority,
    Diplomacy,  // <- RENAMED
    Cunning
}
```

#### 1.2 Update All Code References
Use global find/replace with **case-sensitive** matching:

**Pattern 1**: `PlayerStatType.Diplomacy` → `PlayerStatType.Diplomacy`
- Files: All `.cs` files
- Estimated: ~150 occurrences

**Pattern 2**: `stat == PlayerStatType.Diplomacy` → `stat == PlayerStatType.Diplomacy`
- Files: CardEffectCatalog.cs, ConversationFacade.cs, etc.

**Pattern 3**: Variable names: `diplomacy` → `diplomacy` (case by case review)
- Example: `diplomacyLevel` → `diplomacyLevel`

#### 1.3 Update JSON Content Files
**Files**:
- `C:\Git\Wayfarer\src\Content\Core\02_cards.json` (15 occurrences)
- `C:\Git\Wayfarer\src\Content\Core\03_npcs.json` (6 occurrences)
- `C:\Git\Wayfarer\src\Content\Core\05_gameplay.json` (5 occurrences)

**Find/Replace**:
```json
// BEFORE:
"boundStat": "Diplomacy"

// AFTER:
"boundStat": "Diplomacy"
```

#### 1.4 Update UI Display Text
**Files**:
- `C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor` (line 208)
- `C:\Git\Wayfarer\src\Pages\Components\TokenDisplay.razor` (6 occurrences)
- `C:\Git\Wayfarer\src\Pages\Components\ExchangeContent.razor` (4 occurrences)

**Pattern**:
```razor
<!-- BEFORE -->
<span class="stat-icon diplomacy">💰</span>

<!-- AFTER -->
<span class="stat-icon diplomacy">💰</span>
```

#### 1.5 Update CSS Classes
**Files**:
- `C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor.css`
- `C:\Git\Wayfarer\src\wwwroot\css\exchange-screen.css`

**Pattern**:
```css
/* BEFORE */
.stat-badge.diplomacy { }
.card.diplomacy { }

/* AFTER */
.stat-badge.diplomacy { }
.card.diplomacy { }
```

#### 1.6 Update Documentation
**Files**: All `.md` files in root directory
- IMPLEMENTATION-GUIDE.md (5 occurrences)
- CONVERSATION-CARD-ARCHITECTURE.md (10 occurrences)
- COMPLETE-CARD-LIBRARY-SPECIFICATION.md (25 occurrences)
- And 6 others

**Method**: Manual review and update for context accuracy

#### 1.7 Verification Steps
1. ✅ Build compiles successfully
2. ✅ All tests pass
3. ✅ Search codebase for remaining "Diplomacy" - should only appear in:
   - Historical commit messages
   - Possibly exchange/trade-related code (different context)
4. ✅ Visual inspection: UI shows "Diplomacy" not "Diplomacy"

---

## Issue 2: JSON Cards Missing "delivery" Property

### Problem
**CRITICAL**: No cards in `02_cards.json` have the `delivery` field. This means:
- Entire Delivery system is non-functional
- Authority cards won't use Commanding delivery (+2 Cadence)
- Rapport can't use Measured/Yielding to manage tension
- Diplomacy can't use Measured for controlled conversations
- All cards default to Standard (+1 Cadence)

### Impact
**Severity**: CRITICAL - Core mechanic completely disabled

### Fix Steps

#### 2.1 Add "delivery" Property to Card Schema
**File**: `C:\Git\Wayfarer\src\Content\Core\02_cards.json`

All cards need `"delivery"` field added with one of four values:
- `"Standard"` - +1 Cadence on SPEAK
- `"Commanding"` - +2 Cadence on SPEAK (ALL Authority cards)
- `"Measured"` - +0 Cadence on SPEAK
- `"Yielding"` - -1 Cadence on SPEAK

#### 2.2 Delivery Assignment Guidelines by Stat

**Authority** (15 cards):
```json
{
  "id": "authority_*",
  "boundStat": "Authority",
  "delivery": "Commanding",  // ALL Authority = Commanding
  ...
}
```
- **Rule**: 100% Commanding
- **Reason**: Authority creates tension through forceful delivery

**Rapport** (15 cards):
```json
// Distribution:
// - 60% Measured (9 cards) - Calm, measured approach
// - 20% Yielding (3 cards) - Gentle, yielding approach
// - 20% Standard (3 cards) - Balanced approach
```
- **Rule**: Mostly Measured (60%), some Yielding/Standard
- **Reason**: Rapport manages cadence carefully to avoid tension

**Diplomacy** (15 cards):
```json
// Distribution:
// - 80% Measured (12 cards) - Controlled negotiation
// - 20% Standard (3 cards) - Direct when needed
```
- **Rule**: Primarily Measured (80%)
- **Reason**: Diplomacy focuses on control and de-escalation

**Cunning** (15 cards):
```json
// Distribution:
// - 80% Standard (12 cards) - Adaptable delivery
// - 20% Measured (3 cards) - When precision matters
```
- **Rule**: Mostly Standard (80%)
- **Reason**: Cunning is flexible, adapts delivery to situation

**Insight** (15 cards):
```json
// Distribution:
// - 60% Standard (9 cards) - Direct observation
// - 40% Measured (6 cards) - Thoughtful analysis
```
- **Rule**: Mix of Standard (60%) and Measured (40%)
- **Reason**: Insight balances directness with careful thought

#### 2.3 Example Card Updates

**Before** (missing delivery):
```json
{
  "id": "authority_commanding_presence",
  "title": "Assert Authority",
  "boundStat": "Authority",
  "depth": 1,
  "persistence": "Statement",
  "initiativeCost": 2,
  "effectVariant": "Primary",
  "dialogueText": "Listen carefully. This is how it's going to be."
}
```

**After** (with delivery):
```json
{
  "id": "authority_commanding_presence",
  "title": "Assert Authority",
  "boundStat": "Authority",
  "depth": 1,
  "persistence": "Statement",
  "initiativeCost": 2,
  "effectVariant": "Primary",
  "delivery": "Commanding",  // <- ADDED
  "dialogueText": "Listen carefully. This is how it's going to be."
}
```

#### 2.4 Automated Addition Script
Create a script to add delivery properties based on stat:

```bash
# Pseudo-script for adding delivery
for each card in 02_cards.json:
    if card.boundStat == "Authority":
        card.delivery = "Commanding"
    elif card.boundStat == "Rapport":
        card.delivery = random_weighted(["Measured": 0.6, "Yielding": 0.2, "Standard": 0.2])
    elif card.boundStat == "Diplomacy":
        card.delivery = random_weighted(["Measured": 0.8, "Standard": 0.2])
    elif card.boundStat == "Cunning":
        card.delivery = random_weighted(["Standard": 0.8, "Measured": 0.2])
    elif card.boundStat == "Insight":
        card.delivery = random_weighted(["Standard": 0.6, "Measured": 0.4])
```

#### 2.5 Manual Review Required
After automated addition:
1. Review each Authority card - confirm ALL are Commanding
2. Review high-depth cards (7-8) - might need specific delivery for balance
3. Review card dialogue text - delivery should match tone
   - Example: "I demand..." → Commanding ✓
   - Example: "Perhaps we could..." → Measured or Yielding ✓

#### 2.6 Verification Steps
1. ✅ All 75 cards (15 per stat × 5 stats) have `"delivery"` field
2. ✅ All Authority cards use `"Commanding"`
3. ✅ Parser logs show delivery values being read (not defaulting)
4. ✅ In-game test: Authority card shows "+2 Cadence" effect
5. ✅ In-game test: Rapport Measured card shows "+0 Cadence" effect

---

## Issue 3: Authority Effect Formula Errors

### Problem 1: Understanding Values Too High
**File**: `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs`

**Current** (Lines 401-431):
- Advanced (D5-6): +3 Understanding ❌
- Master (D7-8): +4 Understanding ❌

**Specification** (conversation-system-refinement.md):
- Advanced (D5-6): +1 Understanding ✓
- Master (D7-8): +2 Understanding ✓

**Impact**: Authority generates nearly as much Understanding as Rapport, breaking stat specialization.

### Problem 2: Direct Doubt Effects (Wrong System)
**Current** (Lines 357-366, 389-397, 412, 428):
- Foundation variants: +1 Doubt
- Advanced: +3 Doubt
- Master: +4 Doubt

**Specification** (Indirect Doubt System):
- Doubt should ONLY come from Cadence during LISTEN
- Authority creates tension via Commanding delivery (+2 Cadence)
- Cadence → Doubt conversion happens during LISTEN, not on card play

**Impact**: Violates core design principle of "Indirect Doubt System"

### Fix Steps

#### 3.1 Fix Understanding Values
**File**: `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs`

**Line 401-415** (Advanced Authority):
```csharp
// BEFORE (WRONG):
5 or 6 => new List<CardEffectFormula>
{
    // +8 Momentum, +2 Initiative, +3 Understanding (WRONG!)
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 3 },  // <- WRONG
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 3 }  // <- REMOVE
        }
    }
},

// AFTER (CORRECT):
5 or 6 => new List<CardEffectFormula>
{
    // +8 Momentum, +2 Initiative, +1 Understanding (CORRECT)
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 1 }  // <- FIXED
        }
    }
},
```

**Line 417-431** (Master Authority):
```csharp
// BEFORE (WRONG):
new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },  // <- WRONG
new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 4 }  // <- REMOVE

// AFTER (CORRECT):
new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 2 }  // <- FIXED
```

#### 3.2 Remove All Direct Doubt Effects
**Lines to modify**: 357-366 (Foundation variants), 389-397 (Standard variant), 412 (Advanced), 428 (Master)

**Remove all lines containing**:
```csharp
new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = X }
```

**Reasoning**:
- Doubt comes ONLY from Cadence during LISTEN (Indirect Doubt System)
- Authority creates tension via Commanding delivery → +2 Cadence → Doubt accumulates when player uses LISTEN
- This teaches players about delayed consequences, not immediate punishment

#### 3.3 Verification Steps
1. ✅ Authority Advanced Understanding: 1 (not 3)
2. ✅ Authority Master Understanding: 2 (not 4)
3. ✅ No Authority cards have direct Doubt effects
4. ✅ Search codebase for `ConversationResourceType.Doubt` in CardEffectCatalog - should ONLY appear in Diplomacy effects (negative values)
5. ✅ In-game test: Authority card at D5-6 gives exactly +1 Understanding

---

## Issue 4: Rapport Effect Formula Errors

### Problem: Missing Momentum Secondaries
**File**: `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs`

**Specification** (conversation-system-refinement.md):
- Standard (D3-4): +4 Understanding, +1 Initiative, +1 Momentum
- Advanced (D5-6): +6 Understanding, +2 Initiative, +2 Momentum
- Master (D7-8): +10 Understanding, +3 Initiative, +3 Momentum

**Current Implementation**:
- Standard (D3-4): +4 Understanding, +2 Initiative ❌ (missing Momentum, wrong Initiative)
- Advanced (D5-6): +6 Understanding, +3 Initiative, +2 Momentum ❌ (wrong Initiative)
- Master (D7-8): +10 Understanding, +4 Initiative, +3 Momentum ❌ (wrong Initiative)

**Impact**: Rapport cards don't provide balanced secondary resources, weakening their "universal secondary" role.

### Fix Steps

#### 4.1 Fix Standard Rapport (D3-4)
**File**: `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs` (Lines 262-291)

```csharp
// BEFORE (Lines 262-291) - Two variants, both WRONG:
3 or 4 => new List<CardEffectFormula>
{
    // Pure Understanding
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Fixed,
        TargetResource = ConversationResourceType.Understanding,
        BaseValue = 4
    },
    // Understanding + Initiative (MISSING MOMENTUM!)
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }  // WRONG: Should be 1
        }
    },
    // Understanding + Momentum (WRONG VARIANT - Should have BOTH Initiative and Momentum together)
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }  // WRONG: Should be 1
        }
    }
},

// AFTER (CORRECT):
3 or 4 => new List<CardEffectFormula>
{
    // Pure Understanding specialist
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Fixed,
        TargetResource = ConversationResourceType.Understanding,
        BaseValue = 4
    },
    // Understanding + BOTH secondaries
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 4 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },  // FIXED
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }   // ADDED
        }
    }
},
```

#### 4.2 Fix Advanced Rapport (D5-6)
**Lines 294-307**:

```csharp
// BEFORE (WRONG):
5 or 6 => new List<CardEffectFormula>
{
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 6 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 },  // WRONG: Should be 2
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
        }
    }
},

// AFTER (CORRECT):
5 or 6 => new List<CardEffectFormula>
{
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 6 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },  // FIXED
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
        }
    }
},
```

#### 4.3 Fix Master Rapport (D7-8)
**Lines 310-323**:

```csharp
// BEFORE (WRONG):
7 or 8 => new List<CardEffectFormula>
{
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 10 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 4 },  // WRONG: Should be 3
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 }
        }
    }
},

// AFTER (CORRECT):
7 or 8 => new List<CardEffectFormula>
{
    new CardEffectFormula
    {
        FormulaType = EffectFormulaType.Compound,
        CompoundEffects = new List<CardEffectFormula>
        {
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Understanding, BaseValue = 10 },
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 },  // FIXED
            new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 }
        }
    }
},
```

#### 4.4 Verification Steps
1. ✅ Rapport Standard (D3-4): Has variant with +4 Understanding, +1 Initiative, +1 Momentum
2. ✅ Rapport Advanced (D5-6): +6 Understanding, +2 Initiative, +2 Momentum
3. ✅ Rapport Master (D7-8): +10 Understanding, +3 Initiative, +3 Momentum
4. ✅ In-game test: Rapport D3 card shows both Initiative AND Momentum in effect description
5. ✅ Effect description reads: "+4 Understanding +1 Initiative +1 Momentum"

---

## Issue 5: Diplomacy Foundation Initiative Cost

### Problem
**File**: `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs` (Line 676)

**Current**:
```csharp
if (stat == PlayerStatType.Diplomacy)  // Note: Will be Diplomacy after rename
{
    return depth switch
    {
        1 or 2 => 0,  // WRONG: Should be 1
        3 or 4 => 4,
        5 or 6 => 6,
        7 or 8 => 8,
        _ => 0
    };
}
```

**Specification**:
- Foundation (D1-2): 1 Initiative
- Standard (D3-4): 4 Initiative
- Advanced (D5-6): 6 Initiative
- Master (D7-8): 8 Initiative

**Impact**: Minor - Diplomacy Foundation cards are free instead of costing 1 Initiative. This is a small balance issue.

### Fix Steps

#### 5.1 Update Initiative Cost
**File**: `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs` (Line 676)

```csharp
// BEFORE:
if (stat == PlayerStatType.Diplomacy)  // After Diplomacy rename
{
    return depth switch
    {
        1 or 2 => 0,  // WRONG
        3 or 4 => 4,
        5 or 6 => 6,
        7 or 8 => 8,
        _ => 0
    };
}

// AFTER:
if (stat == PlayerStatType.Diplomacy)
{
    return depth switch
    {
        1 or 2 => 1,  // FIXED
        3 or 4 => 4,
        5 or 6 => 6,
        7 or 8 => 8,
        _ => 0
    };
}
```

#### 5.2 Verification Steps
1. ✅ Diplomacy Foundation cards cost 1 Initiative
2. ✅ Search for `=> 0,` in CardEffectCatalog - should only appear in default cases
3. ✅ In-game test: Diplomacy D1-2 card shows "1" in Initiative cost circle

---

## Implementation Order (Prioritized)

### Phase 1: Critical Fixes (Blocks System)
**Must complete before testing**

1. ✅ **Add "delivery" to JSON cards** (Issue #2)
   - Estimated Time: 2 hours (automated script + manual review)
   - Blocker: YES - System non-functional without this
   - Files: `02_cards.json` (75 cards)

### Phase 2: Formula Corrections (Gameplay Balance)
**Must complete for correct mechanics**

2. ✅ **Fix Authority formulas** (Issue #3)
   - Estimated Time: 30 minutes
   - Blocker: NO - But wrong balance
   - Files: `CardEffectCatalog.cs` (4 locations)

3. ✅ **Fix Rapport formulas** (Issue #4)
   - Estimated Time: 30 minutes
   - Blocker: NO - But wrong balance
   - Files: `CardEffectCatalog.cs` (3 locations)

4. ✅ **Fix Diplomacy Initiative cost** (Issue #5)
   - Estimated Time: 5 minutes
   - Blocker: NO - Minor balance issue
   - Files: `CardEffectCatalog.cs` (1 line)

### Phase 3: Terminology Consistency (Quality)
**Important for clarity, not blocking**

5. ✅ **Diplomacy → Diplomacy rename** (Issue #1)
   - Estimated Time: 3-4 hours (automated + manual verification)
   - Blocker: NO - Clarity issue only
   - Files: 86 files, 332 occurrences

**Note**: Diplomacy rename can happen in parallel with other fixes or after, as it doesn't affect functionality.

---

## Testing Strategy

### Unit Tests
**After each fix, run**:
```bash
dotnet test
```

**Expected**: All existing tests pass

### Integration Tests
**Create new tests for**:
1. Delivery property parsing from JSON
2. Authority Understanding values (Advanced: +1, Master: +2)
3. Rapport secondary resources (Initiative + Momentum)
4. Diplomacy Initiative costs (Foundation: 1)
5. No direct Doubt effects in CardEffectCatalog (except Diplomacy negative)

### Manual E2E Testing
**Test Scenario 1: Delivery System**
1. Start new game
2. Enter conversation with NPC
3. Play Authority card (depth 1-2)
4. Verify: Cadence increases by +2 (Commanding delivery)
5. Play Rapport card (depth 1-2)
6. Verify: Cadence increases by 0 or +1 (Measured/Standard delivery)

**Test Scenario 2: Understanding Tier Unlocks**
1. Start conversation with Rapport focus
2. Play multiple Rapport cards
3. Verify: Understanding increases correctly
4. Verify: At 6 Understanding → Tier 2 unlocks (D3-4 available)
5. Verify: At 12 Understanding → Tier 3 unlocks (D5-6 available)
6. Verify: At 18 Understanding → Tier 4 unlocks (D7-8 available)

**Test Scenario 3: Authority Tension System**
1. Play multiple Authority cards (Commanding delivery)
2. Verify: Cadence increases by +2 each time
3. Verify: NO immediate Doubt added
4. Execute LISTEN action
5. Verify: Doubt added based on Cadence level (Indirect Doubt System)
6. Verify: Understanding preserved during LISTEN

**Test Scenario 4: Rapport Balance**
1. Play Rapport Standard card (D3-4)
2. Verify effect shows: "+4 Understanding +1 Initiative +1 Momentum"
3. Verify all three resources increased correctly

---

## Rollback Plan

If issues arise after fixes:

### Git Rollback Points
**Before starting**:
```bash
git branch pre-conversation-fix
git commit -am "Checkpoint before conversation system fixes"
```

**After each phase**:
```bash
git commit -am "Phase 1: Added delivery to JSON cards"
git commit -am "Phase 2: Fixed Authority formulas"
git commit -am "Phase 3: Fixed Rapport formulas"
git commit -am "Phase 4: Fixed Diplomacy cost"
git commit -am "Phase 5: Renamed Diplomacy to Diplomacy"
```

### Rollback Commands
If Phase X fails:
```bash
git reset --hard <commit-before-phase-X>
```

### Safe Rollback Zones
- **After Phase 1**: Can rollback delivery changes, parser handles missing property
- **After Phase 2-4**: Can rollback formula changes independently
- **After Phase 5**: Can rollback rename (most complex, use automated reverse script)

---

## Success Criteria

### Phase 1 Complete When:
- ✅ All 75 cards have `"delivery"` property
- ✅ Parser reads delivery values successfully
- ✅ Authority cards show "+2 Cadence" in effect description
- ✅ Rapport Measured cards show "+0 Cadence"

### Phase 2 Complete When:
- ✅ Authority Advanced gives +1 Understanding (not +3)
- ✅ Authority Master gives +2 Understanding (not +4)
- ✅ No Authority cards add direct Doubt
- ✅ CardEffectCatalog search for `Doubt, BaseValue = [positive number]` returns 0 results in Authority section

### Phase 3 Complete When:
- ✅ Rapport Standard has variant: +4 Understanding +1 Initiative +1 Momentum
- ✅ Rapport Advanced: +6 Understanding +2 Initiative +2 Momentum
- ✅ Rapport Master: +10 Understanding +3 Initiative +3 Momentum

### Phase 4 Complete When:
- ✅ Diplomacy Foundation costs 1 Initiative
- ✅ UI shows "1" in Initiative cost circle for D1-2 Diplomacy cards

### Phase 5 Complete When:
- ✅ Global search for "Diplomacy" returns 0 results in code (except historical commits)
- ✅ PlayerStatType enum shows: Insight, Rapport, Authority, Diplomacy, Cunning
- ✅ UI displays "Diplomacy" in all locations
- ✅ JSON files use `"boundStat": "Diplomacy"`
- ✅ All tests pass
- ✅ Build succeeds with 0 warnings

---

## Files Modified Summary

### JSON Files (3)
- `C:\Git\Wayfarer\src\Content\Core\02_cards.json` - Add delivery to 75 cards, rename Diplomacy → Diplomacy
- `C:\Git\Wayfarer\src\Content\Core\03_npcs.json` - Rename Diplomacy → Diplomacy
- `C:\Git\Wayfarer\src\Content\Core\05_gameplay.json` - Rename Diplomacy → Diplomacy

### Code Files (5 major)
- `C:\Git\Wayfarer\src\GameState\Enums\PlayerStatType.cs` - Rename enum value
- `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs` - Fix 4 formula issues
- All `.cs` files with `PlayerStatType.Diplomacy` references (~50 files)

### UI Files (10+)
- `C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor`
- `C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor.cs`
- `C:\Git\Wayfarer\src\Pages\Components\TokenDisplay.razor`
- `C:\Git\Wayfarer\src\Pages\Components\ExchangeContent.razor`
- And 6 others

### CSS Files (2)
- `C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor.css`
- `C:\Git\Wayfarer\src\wwwroot\css\exchange-screen.css`

### Documentation Files (10+)
- All `.md` files in root with Diplomacy references

**Total Files**: ~86 files

---

## Estimated Timeline

### Phase 1: Add Delivery to JSON
- Script creation: 30 minutes
- Automated addition: 5 minutes
- Manual review: 1.5 hours
- Testing: 30 minutes
- **Total**: 2.5 hours

### Phase 2: Fix Authority Formulas
- Code changes: 15 minutes
- Testing: 15 minutes
- **Total**: 30 minutes

### Phase 3: Fix Rapport Formulas
- Code changes: 20 minutes
- Testing: 10 minutes
- **Total**: 30 minutes

### Phase 4: Fix Diplomacy Cost
- Code change: 2 minutes
- Testing: 3 minutes
- **Total**: 5 minutes

### Phase 5: Diplomacy → Diplomacy Rename
- Automated rename: 30 minutes
- Manual review: 1.5 hours
- Documentation updates: 1 hour
- Testing: 1 hour
- **Total**: 4 hours

### **TOTAL ESTIMATED TIME: 7.5 hours**

---

## Risk Assessment

### High Risk Items
1. **Delivery JSON Addition** - Risk of incorrect stat distribution
   - Mitigation: Automated script with manual review per stat

2. **Diplomacy → Diplomacy Rename** - Risk of missing references
   - Mitigation: Multiple search patterns, automated tools, comprehensive testing

### Medium Risk Items
3. **Formula Changes** - Risk of introducing new imbalances
   - Mitigation: Thorough testing with multiple card combinations

### Low Risk Items
4. **Initiative Cost Fix** - Simple one-line change
   - Mitigation: Direct testing of affected cards

---

## Post-Implementation Validation

### Automated Validation Script
```bash
#!/bin/bash

echo "=== Conversation System Fix Validation ==="

# Check 1: All cards have delivery
echo "Checking delivery property..."
MISSING_DELIVERY=$(jq '[.[] | select(.delivery == null)] | length' src/Content/Core/02_cards.json)
if [ "$MISSING_DELIVERY" -eq 0 ]; then
    echo "✅ All cards have delivery property"
else
    echo "❌ $MISSING_DELIVERY cards missing delivery"
fi

# Check 2: No Diplomacy references
echo "Checking Diplomacy references..."
COMMERCE_COUNT=$(rg -i "diplomacy" --type cs --type json | wc -l)
if [ "$COMMERCE_COUNT" -eq 0 ]; then
    echo "✅ No Diplomacy references found"
else
    echo "⚠️  $COMMERCE_COUNT Diplomacy references remain"
fi

# Check 3: Build succeeds
echo "Running build..."
dotnet build
if [ $? -eq 0 ]; then
    echo "✅ Build successful"
else
    echo "❌ Build failed"
fi

# Check 4: Tests pass
echo "Running tests..."
dotnet test
if [ $? -eq 0 ]; then
    echo "✅ All tests pass"
else
    echo "❌ Tests failed"
fi

echo "=== Validation Complete ==="
```

---

## Appendix A: Exact Formula Reference

### Authority (After Fix)
```
Foundation (D1-2):
- Variant 1: +2 Momentum
- Variant 2: +2 Momentum, +1 Initiative

Standard (D3-4):
- +5 Momentum, +1 Initiative, +1 Understanding

Advanced (D5-6):
- +8 Momentum, +2 Initiative, +1 Understanding (FIXED from +3)

Master (D7-8):
- +12 Momentum, +3 Initiative, +2 Understanding (FIXED from +4)
```

### Rapport (After Fix)
```
Foundation (D1-2):
- +2 Understanding

Standard (D3-4):
- Variant 1: +4 Understanding
- Variant 2: +4 Understanding, +1 Initiative, +1 Momentum (FIXED - added Momentum)

Advanced (D5-6):
- +6 Understanding, +2 Initiative, +2 Momentum (FIXED from +3 Initiative)

Master (D7-8):
- +10 Understanding, +3 Initiative, +3 Momentum (FIXED from +4 Initiative)
```

### Diplomacy (After Fix)
```
Foundation (D1-2):
- Initiative Cost: 1 (FIXED from 0)
- Effect: -1 Doubt, +1 Understanding

Standard (D3-4):
- Initiative Cost: 4
- Effect: -2 Doubt, +2 Momentum (consume 2), +1 Understanding

Advanced (D5-6):
- Initiative Cost: 6
- Effect: -4 Doubt, +3 Momentum (consume 3), +2 Understanding

Master (D7-8):
- Initiative Cost: 8
- Effect: Set Doubt to 0, +4 Momentum (consume 4), +3 Understanding
```

---

## Appendix B: Delivery Distribution Reference

### Final Distribution Target
**Authority** (15 cards):
- Commanding: 15 (100%)

**Rapport** (15 cards):
- Measured: 9 (60%)
- Yielding: 3 (20%)
- Standard: 3 (20%)

**Diplomacy** (15 cards):
- Measured: 12 (80%)
- Standard: 3 (20%)

**Cunning** (15 cards):
- Standard: 12 (80%)
- Measured: 3 (20%)

**Insight** (15 cards):
- Standard: 9 (60%)
- Measured: 6 (40%)

**Total Delivery Breakdown**:
- Commanding: 15 cards (20%)
- Measured: 30 cards (40%)
- Standard: 27 cards (36%)
- Yielding: 3 cards (4%)

---

**END OF FIX PLAN**
