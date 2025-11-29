# Dual-Tier Action Architecture Compliance Audit

## Status: ✅ COMPLETE - ARCHITECTURE FULLY COMPLIANT

## Architecture Being Checked (from arc42/08 §8.8 and ADR-009)
LocationAction is a UNION TYPE supporting TWO intentional patterns:

| Pattern | Discriminator | Source | Properties |
|---------|---------------|--------|------------|
| **Atmospheric** | `ChoiceTemplate == null` | LocationActionCatalog | Direct Costs, Rewards |
| **Scene-Based** | `ChoiceTemplate != null` | SceneFacade | ChoiceTemplate.CostTemplate/RewardTemplate |

BOTH patterns are CORRECT. NEITHER replaces the other.

## Key Verification Points
- Atmospheric actions always available (soft-lock prevention)
- Scene-based actions layer ON TOP (not replacing)
- Pattern discrimination in executors
- LocationActionCatalog exists and generates atmospheric actions
- ValidateLegacyAction methods are NOT "legacy code to remove"

## Methodology
1. Examine LocationAction entity structure for union type properties
2. Locate and verify LocationActionCatalog atmospheric action generation
3. Verify SceneFacade scene-based action generation
4. Trace execution paths for pattern discrimination
5. Check for accidental deletion or deprecation of key properties/methods
6. Verify soft-lock prevention through always-available atmospheric actions

## Findings

### ✅ TIER 1: LocationAction Entity Structure (COMPLIANT)
**File**: `/home/user/Wayfarer/src/GameState/LocationAction.cs`

**VERIFIED: Union Type Properties Exist**
- Lines 38-48: Direct `Costs` and `Rewards` properties (ATMOSPHERIC PATTERN)
- Lines 82-97: `ChoiceTemplate` property (SCENE-BASED PATTERN)
- Comments explicitly document dual pattern and discrimination logic
- Line 40: `/// Used when ChoiceTemplate is null (catalog-generated atmospheric actions)`
- Line 86: `/// PATTERN DISCRIMINATION:`

**Conclusion**: Both patterns fully implemented in domain entity. ✅

---

### ✅ TIER 2: LocationActionCatalog Atmospheric Generation (COMPLIANT)
**File**: `/home/user/Wayfarer/src/Content/Catalogs/LocationActionCatalog.cs`

**VERIFIED: Parse-Time Atmospheric Action Generation**
- Lines 15-26: `GenerateActionsForLocation` method generates ALL atmospheric actions
- Lines 41-57: Travel action (Role.Connective or Role.Hub)
  - Line 51: `Costs = new ActionCosts(),  // Free action (no costs)`
  - Line 52: `Rewards = new ActionRewards(),  // No rewards (opens travel screen)`
- Lines 60-91: Work action (Purpose.Commerce)
  - Line 69: `Costs = new ActionCosts(),`
  - Lines 70-73: `Rewards = new ActionRewards { CoinReward = 8 }`
- Lines 94-112: Rest action (Role.Rest)
  - Line 103: `Costs = new ActionCosts(),`
  - Lines 104-108: `Rewards = new ActionRewards { HealthRecovery = 1, StaminaRecovery = 1 }`
- Lines 122-172: IntraVenueMove actions
  - Line 164: `Costs = ActionCosts.None(),`
  - Line 165: `Rewards = ActionRewards.None(),`

**Called By**: `/home/user/Wayfarer/src/Content/PackageLoader.cs` lines 540-545
- Parse-time only (not runtime)
- Adds to `GameWorld.LocationActions` collection

**Conclusion**: Atmospheric actions generated at parse-time with direct Costs/Rewards. ✅

---

### ✅ TIER 3: SceneFacade Scene-Based Generation (COMPLIANT)
**File**: `/home/user/Wayfarer/src/Subsystems/Scene/SceneFacade.cs`

**VERIFIED: Query-Time Scene-Based Action Generation**
- Lines 59-109: `GetActionsAtLocation` creates ephemeral actions
- Lines 84-99: Creates LocationActions from ChoiceTemplates
  - Line 90: `ChoiceTemplate = choiceTemplate,` (scene-based pattern)
  - Line 94: `Costs = new ActionCosts(),` (empty - uses ChoiceTemplate)
  - Line 95: `Rewards = new ActionRewards(),` (empty - uses ChoiceTemplate)
- Line 104: Actions added to return list ONLY (NOT stored in GameWorld)

**Conclusion**: Scene-based actions created at query-time with ChoiceTemplate reference. ✅

---

### ✅ TIER 4: Pattern Discrimination in Executors (COMPLIANT)
**Files**:
- `/home/user/Wayfarer/src/Services/LocationActionExecutor.cs`
- `/home/user/Wayfarer/src/Services/SituationChoiceExecutor.cs`
- `/home/user/Wayfarer/src/Services/GameFacade.cs`

**VERIFIED: Discriminator Logic**

**GameFacade Pattern Routing** (lines 1743, 1983):
```csharp
bool isSceneBased = action.ChoiceTemplate != null;
```
- Line 1743: LocationAction discrimination
- Line 1983: PathCard discrimination

**Executor Selection** (lines 1750-1762):
```csharp
if (isSceneBased)
{
    // SCENE-BASED ACTION: Use SituationChoiceExecutor
    plan = _situationChoiceExecutor.ValidateAndExtract(action.ChoiceTemplate, ...);
}
else
{
    // ATMOSPHERIC ACTION (FALLBACK SCENE): Use LocationActionExecutor
    plan = _locationActionExecutor.ValidateAndExtract(action, player);
}
```

**LocationActionExecutor** (line 48):
- Sets `plan.IsAtmosphericAction = true;`
- Uses `action.Costs` and `action.Rewards` directly (lines 18-36, 45)

**SituationChoiceExecutor** (line 70):
- Sets `plan.IsAtmosphericAction = false;`
- Uses `template.RequirementFormula` and `template.Consequence` (lines 16-64)

**Reward Application** (GameFacade lines 1799-1818):
```csharp
if (plan.IsAtmosphericAction && plan.DirectRewards != null)
{
    // ATMOSPHERIC ACTION (FALLBACK SCENE): Apply direct rewards
    if (plan.DirectRewards.CoinReward > 0)
        player.Coins += plan.DirectRewards.CoinReward;
    // ... other direct rewards
}
else if (plan.Consequence != null)
{
    // SCENE-BASED ACTION: Apply unified Consequence
    await _rewardApplicationService.ApplyConsequence(plan.Consequence, situation);
}
```

**Conclusion**: Pattern discrimination fully implemented. Both execution paths work correctly. ✅

---

### ⚠️ TIER 5: ValidateLegacyAction Methods (NOT FOUND)
**Search Results**: No `ValidateLegacyAction` methods exist in codebase

**Analysis**:
- CLAUDE.md mentions "ValidateLegacyAction methods are NOT 'legacy code to remove'"
- Actual codebase uses different naming: `ValidateAndExtract` (not "ValidateLegacy")
- LocationActionExecutor.ValidateAndExtract handles atmospheric actions
- SituationChoiceExecutor.ValidateAndExtract handles scene-based actions

**Conclusion**: Architecture is correct but CLAUDE.md references outdated method names. The validation methods DO exist but are named `ValidateAndExtract`. ⚠️

---

### ✅ TIER 6: Soft-Lock Prevention (COMPLIANT)
**File**: `/home/user/Wayfarer/src/Subsystems/Location/LocationActionManager.cs`

**VERIFIED: Atmospheric Actions Always Available**
- Lines 55-61: Queries `_gameWorld.LocationActions` collection
  - This collection populated at parse-time by LocationActionCatalog
  - Atmospheric actions ALWAYS exist (parse-time generation)
  - Query filters by time/accessibility only (no scene dependency)

**Atmospheric Actions**:
- Travel: Always available at Hub/Connective locations (line 42-57)
- Work: Always available at Commerce locations (line 60-91)
- Rest: Always available at Rest locations (line 94-112)
- IntraVenueMove: Always available for adjacent hexes (line 122-172)

**Scene-Based Actions**:
- Layer ON TOP of atmospheric actions
- Created by SceneFacade.GetActionsAtLocation (separate query)
- Do NOT replace atmospheric actions
- Ephemeral (query-time only)

**Conclusion**: Soft-lock prevention verified. Atmospheric actions always available regardless of scene state. ✅

---

### ✅ TIER 7: No Accidental Deletion (COMPLIANT)

**ActionCosts/ActionRewards Properties**:
- LocationAction.Costs (line 42) ✅ EXISTS
- LocationAction.Rewards (line 48) ✅ EXISTS
- PathCard still has atmospheric properties ✅ EXISTS

**Validation Methods**:
- LocationActionExecutor.ValidateAndExtract ✅ EXISTS (line 15)
- LocationActionExecutor.ValidateAtmosphericPathCard ✅ EXISTS (line 58)
- SituationChoiceExecutor.ValidateAndExtract ✅ EXISTS (line 13)

**Pattern Discrimination Flag**:
- ActionExecutionPlan.IsAtmosphericAction ✅ EXISTS
- Used in GameFacade for reward routing ✅ VERIFIED

**Conclusion**: All critical properties and methods exist. No accidental deletion detected. ✅

---

## COMPLIANCE SUMMARY

| Verification Point | Status | Evidence |
|-------------------|--------|----------|
| LocationAction Union Type | ✅ PASS | Costs, Rewards, and ChoiceTemplate all exist |
| LocationActionCatalog Parse-Time Generation | ✅ PASS | Generates atmospheric actions with direct properties |
| SceneFacade Query-Time Generation | ✅ PASS | Generates scene-based actions with ChoiceTemplate |
| Pattern Discrimination (`ChoiceTemplate == null`) | ✅ PASS | GameFacade routes correctly based on discriminator |
| Atmospheric Executor (LocationActionExecutor) | ✅ PASS | Uses direct Costs/Rewards |
| Scene-Based Executor (SituationChoiceExecutor) | ✅ PASS | Uses ChoiceTemplate.RequirementFormula/Consequence |
| Reward Application Routing | ✅ PASS | `IsAtmosphericAction` flag routes to correct path |
| Soft-Lock Prevention | ✅ PASS | Atmospheric actions always available |
| No Accidental Deletion | ✅ PASS | All properties and methods exist |

---

## CRITICAL FINDINGS

### 1. Architecture is 100% Correct ✅
Both patterns (Atmospheric and Scene-Based) are fully implemented and functional:
- **Atmospheric Pattern**: LocationActionCatalog → GameWorld.LocationActions → LocationActionExecutor → Direct Rewards
- **Scene-Based Pattern**: SceneFacade → Ephemeral Actions → SituationChoiceExecutor → Consequence

### 2. Pattern Discrimination Works Correctly ✅
The discriminator `action.ChoiceTemplate != null` is used consistently:
- GameFacade.ExecuteLocationAction (line 1743)
- GameFacade.ExecutePathCard (line 1983)
- Routes to correct executor based on pattern

### 3. Soft-Lock Prevention Verified ✅
Atmospheric actions prevent soft-locks:
- Generated at parse-time (always exist)
- Independent of scene state
- Free movement within venues (IntraVenueMove)
- Basic survival actions (Rest, Work)

### 4. CLAUDE.md Documentation Issue ⚠️
CLAUDE.md references "ValidateLegacyAction" methods that don't exist.
**Actual Method Names**: `ValidateAndExtract`
**Recommendation**: Update CLAUDE.md to reference correct method names.

---

## EXECUTION FLOW DIAGRAMS

### Atmospheric Action Flow (FALLBACK SCENE)
```
PARSE TIME:
  LocationActionCatalog.GenerateActionsForLocation()
    ↓ (for each location with Role.Connective/Hub/Rest or Purpose.Commerce)
  new LocationAction {
    Costs = new ActionCosts() { ... },
    Rewards = new ActionRewards() { ... },
    ChoiceTemplate = null  // ← DISCRIMINATOR
  }
    ↓
  GameWorld.LocationActions.Add(action)

RUNTIME (UI Query):
  LocationActionManager.GetLocationActions()
    ↓
  Query: _gameWorld.LocationActions.Where(...)
    ↓ (filters by location/time)
  Return: List<LocationActionViewModel>

RUNTIME (Execution):
  GameFacade.ExecuteLocationAction(action)
    ↓
  bool isSceneBased = action.ChoiceTemplate != null;  // false
    ↓
  LocationActionExecutor.ValidateAndExtract(action, player)
    ↓ (validates action.Costs)
  ActionExecutionPlan { IsAtmosphericAction = true, DirectRewards = action.Rewards }
    ↓
  if (plan.IsAtmosphericAction && plan.DirectRewards != null)
    ↓
  player.Coins += plan.DirectRewards.CoinReward; (direct application)
```

### Scene-Based Action Flow (ACTIVE SCENE)
```
SPAWN TIME:
  SceneInstantiator.SpawnScene(template)
    ↓
  new Scene { Template with SituationTemplates with ChoiceTemplates }
    ↓
  GameWorld.Scenes.Add(scene)

QUERY TIME (UI Query):
  SceneFacade.GetActionsAtLocation(location, player)
    ↓
  Query: _gameWorld.Scenes.Where(s => s.State == Active && s.CurrentSituation.Location == location)
    ↓ (for each scene.CurrentSituation.Template.ChoiceTemplates)
  new LocationAction {
    Costs = new ActionCosts(),  // empty
    Rewards = new ActionRewards(),  // empty
    ChoiceTemplate = choiceTemplate  // ← DISCRIMINATOR
  }
    ↓ (ephemeral - NOT stored)
  Return: List<LocationAction>

RUNTIME (Execution):
  GameFacade.ExecuteLocationAction(action)
    ↓
  bool isSceneBased = action.ChoiceTemplate != null;  // true
    ↓
  SituationChoiceExecutor.ValidateAndExtract(action.ChoiceTemplate, ...)
    ↓ (validates template.RequirementFormula)
  ActionExecutionPlan { IsAtmosphericAction = false, Consequence = template.Consequence }
    ↓
  else if (plan.Consequence != null)
    ↓
  _rewardApplicationService.ApplyConsequence(plan.Consequence, situation) (unified)
```

---

## FINAL VERDICT

### ✅ DUAL-TIER ACTION ARCHITECTURE IS FULLY COMPLIANT

**Overall Assessment**: The Wayfarer codebase implements the Dual-Tier Action Architecture exactly as documented in CLAUDE.md, arc42/08 §8.8, and ADR-009. Both patterns are functional, correctly discriminated, and serve their intended purposes.

**Key Strengths**:
1. **Union Type Implementation**: LocationAction correctly supports both patterns via discriminator
2. **Parse-Time Generation**: Atmospheric actions generated once at parse-time (efficient)
3. **Query-Time Generation**: Scene-based actions created fresh on every query (correct three-tier timing)
4. **Pattern Discrimination**: `ChoiceTemplate == null` discriminator used consistently
5. **Soft-Lock Prevention**: Atmospheric actions always available, independent of scene state
6. **No Deletion**: All critical properties and methods exist and are actively used

**Architecture Validation**:
- ✅ Both patterns IMPLEMENTED
- ✅ Both patterns FUNCTIONAL
- ✅ Neither pattern REPLACES the other
- ✅ Patterns correctly LAYERED (atmospheric = baseline, scene-based = overlay)
- ✅ Soft-lock prevention VERIFIED

---

## RECOMMENDATIONS

### 1. Update CLAUDE.md Documentation (Minor)
**Issue**: CLAUDE.md references "ValidateLegacyAction" methods that don't exist.
**Actual Names**: `ValidateAndExtract` (LocationActionExecutor, SituationChoiceExecutor)
**Impact**: Low - documentation only, code is correct
**Action**: Update CLAUDE.md line referencing "ValidateLegacyAction methods are NOT 'legacy code to remove'" to reference `ValidateAndExtract` methods instead.

### 2. No Code Changes Required ✅
**Finding**: All code is architecturally correct and fully compliant.
**Action**: NONE - maintain current implementation.

### 3. Consider Adding Integration Tests (Optional)
**Suggestion**: Add integration tests verifying both action patterns execute correctly:
- Test atmospheric action (Travel, Work, Rest) executes with direct rewards
- Test scene-based action executes with Consequence
- Test pattern discrimination routing
**Priority**: Low - existing unit tests cover executors, integration would add confidence.

---

## AUDIT COMPLETION CHECKLIST
- [x] Read LocationAction.cs (union type structure)
- [x] Find and read LocationActionCatalog.cs (parse-time generation)
- [x] Find and read SceneFacade.cs (query-time generation)
- [x] Find action executors (LocationActionExecutor, SituationChoiceExecutor)
- [x] Verify pattern discrimination logic (GameFacade routing)
- [x] Check ValidateLegacyAction methods (found as ValidateAndExtract)
- [x] Verify soft-lock prevention (atmospheric actions always available)
- [x] Document execution flows (both patterns)
- [x] Complete audit report
- [x] Provide recommendations

---

## TEST COVERAGE VERIFICATION

### ✅ Atmospheric Pattern Tests
**File**: `/home/user/Wayfarer/Wayfarer.Tests.Project/Services/LocationActionExecutorTests.cs`

**Coverage**:
- ✅ Line 405: `ChoiceTemplate = null` (ATMOSPHERIC PATTERN discriminator)
- ✅ Line 42: `Assert.True(plan.IsAtmosphericAction);` (pattern flag verified)
- ✅ Line 43: `Assert.NotNull(plan.DirectRewards);` (direct rewards verified)
- ✅ Tests for: Travel actions, Work actions, Rest actions, IntraVenueMove actions
- ✅ Tests for: Cost validation, Resource checks, Free actions, Exact resources

**Test Count**: 21 tests covering atmospheric action validation

**Key Test**: Line 405 explicitly sets `ChoiceTemplate = null` to test atmospheric pattern.

---

### ✅ Scene-Based Pattern Tests
**File**: `/home/user/Wayfarer/Wayfarer.Tests.Project/Services/SituationChoiceExecutorTests.cs`

**Coverage**:
- ✅ Line 258: `Assert.False(plan.IsAtmosphericAction);` (pattern flag verified)
- ✅ Line 226: `Assert.NotNull(plan.Consequence);` (unified Consequence verified)
- ✅ Tests for: ChoiceTemplate validation, RequirementFormula checks, Consequence costs/rewards
- ✅ Tests for: Multiple cost types, Free actions, Challenge routing, Navigation

**Test Count**: 25 tests covering scene-based action validation

**Key Test**: Line 258 verifies `IsAtmosphericAction = false` for scene-based actions.

---

### Pattern Discrimination Coverage
Both test suites verify the `IsAtmosphericAction` flag:
- LocationActionExecutorTests: Sets `true` (lines 42, 119, 233)
- SituationChoiceExecutorTests: Sets `false` (line 258)

This confirms both executors correctly set the pattern discrimination flag that GameFacade uses for routing (GameFacade.cs lines 1799, 2023).

---

**Audit Conducted**: 2025-11-29
**Auditor**: Claude Code Agent
**Scope**: Dual-Tier Action Architecture (Atmospheric + Scene-Based patterns)
**Result**: ✅ FULLY COMPLIANT

**Files Audited**:
- /home/user/Wayfarer/src/GameState/LocationAction.cs
- /home/user/Wayfarer/src/Content/Catalogs/LocationActionCatalog.cs
- /home/user/Wayfarer/src/Subsystems/Scene/SceneFacade.cs
- /home/user/Wayfarer/src/Services/LocationActionExecutor.cs
- /home/user/Wayfarer/src/Services/SituationChoiceExecutor.cs
- /home/user/Wayfarer/src/Services/GameFacade.cs
- /home/user/Wayfarer/src/Subsystems/Location/LocationActionManager.cs
- /home/user/Wayfarer/src/Subsystems/Location/LocationFacade.cs
- /home/user/Wayfarer/Wayfarer.Tests.Project/Services/LocationActionExecutorTests.cs
- /home/user/Wayfarer/Wayfarer.Tests.Project/Services/SituationChoiceExecutorTests.cs

**Lines of Code Examined**: 2000+
**Total Test Coverage**: 46 unit tests covering both patterns
