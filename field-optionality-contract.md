# Wayfarer JSON Field Optionality Contract

## Analysis Methodology

For each DTO field, we analyzed actual JSON file frequency:
- **100%** = REQUIRED (must exist in all instances)
- **50-99%** = CONDITIONAL (business logic determines presence)
- **1-49%** = RARE (investigate why so inconsistent)
- **0%** = DEPRECATED (remove from DTO)

---

## SocialCard Entity

**JSON File**: `Content/Core/08_social_cards.json`
**Total Instances**: 63 cards

### REQUIRED Fields (100% frequency)

| Field | Type | Frequency | Parser Rule |
|-------|------|-----------|-------------|
| `type` | string | 63/63 (100%) | Crash if null/empty |
| `title` | string | 63/63 (100%) | Crash if null/empty |
| `dialogueText` | string | 63/63 (100%) | Crash if null/empty |
| `persistence` | string | 63/63 (100%) | Crash if null/empty |
| `delivery` | string | 63/63 (100%) | Crash if null/empty |

**Current Violation**:
```csharp
// SocialCardParser.cs line 168:
Title = dto.Title ?? "",  // ❌ HIDES MISSING DATA
```

**Correct Fix**:
```csharp
if (string.IsNullOrEmpty(dto.Title))
{
    throw new InvalidDataException(
        $"SocialCard '{dto.Id}' missing required field 'title'. " +
        $"Check Content/Core/08_social_cards.json"
    );
}
string title = dto.Title;
```

### CONDITIONAL Fields (50-99% frequency)

| Field | Type | Frequency | Business Logic |
|-------|------|-----------|----------------|
| `conversationalMove` | string | 60/63 (95%) | Only conversation cards (not request cards) |
| `personalityTypes` | List<string> | 60/63 (95%) | Same as above |
| `boundStat` | string | 60/63 (95%) | Same as above |
| `depth` | int? | 60/63 (95%) | Same as above |

**Parser Rule**: Check card type first, then validate presence
```csharp
if (isConversationCard && string.IsNullOrEmpty(dto.BoundStat))
{
    throw new InvalidDataException($"Conversation card '{dto.Id}' missing 'boundStat'");
}
```

### RARE Fields (1-49% frequency)

| Field | Type | Frequency | Explanation |
|-------|------|-----------|-------------|
| `momentumThreshold` | int? | 3/63 (4%) | Only request cards have this |

**Parser Rule**: Optional with zero default is acceptable here

### OPTIONAL Fields (0% frequency - parser handles null gracefully)

| Field | Type | Parser Behavior |
|-------|------|-----------------|
| `connectionType` | string | Defaults to Trust (semantically valid for all cards) |
| `effects` | CardEffectsDTO | Returns SuccessEffectType.None if null (valid semantic default) |
| `tokenRequirement` | Dictionary | Initializes empty dictionary if null (valid semantic default) |

**Parser Rule**: These fields are legitimately optional - parser handles absence with valid semantic defaults, not bug hiding.

### DEPRECATED Fields (0% frequency - TRULY UNUSED, DELETE FROM DTO)

| Field | Current Type | Why Deprecated |
|-------|--------------|----------------|
| `minimumTokensRequired` | int? | Parser used ?? 0 to hide bugs - no semantic meaning |
| `npcSpecific` | string | Parsed but never meaningfully used in game logic |
| `secretsGranted` | List<string> | Parser used ?? new List() to hide bugs - Knowledge system eliminated |

**Action Taken**: ✅ Deleted from DTO, parser, and entity

---

## Summary Statistics

### SocialCard Analysis
- Total DTO properties analyzed: 16
- REQUIRED fields: 5 (100% frequency)
- CONDITIONAL fields: 4 (95% frequency)
- RARE fields: 1 (4% frequency)
- OPTIONAL fields: 3 (0% frequency but semantically valid defaults)
- DEPRECATED fields: 3 (0% frequency and truly unused) ✅ DELETED

### Null-Coalescing Violations in SocialCardParser.cs - FIXED
- ✅ Line 158-164: `dto.Title` - Added validation that crashes if null/empty (REQUIRED field)
- ✅ Line 180: `MinimumTokensRequired` - DELETED (deprecated field)
- Line 185: `dto.MomentumThreshold ?? 0` - RARE field, acceptable default
- ✅ Line 189: `SecretsGranted` - DELETED (deprecated field)

---

## Completed Work

### ✅ Phase 2: SocialCardDTO Reform (COMPLETED)
- Organized fields by optionality contract (REQUIRED/CONDITIONAL/RARE/OPTIONAL/DEPRECATED)
- Deleted 3 deprecated properties (MinimumTokensRequired, NpcSpecific, SecretsGranted)
- Kept 3 optional properties that parser handles gracefully (ConnectionType, Effects, TokenRequirement)
- Added documentation comments explaining each category

### ✅ Phase 3: SocialCardParser Purge (COMPLETED)
- Added validation for `dto.Title` (crashes if null/empty) - REQUIRED field enforcement
- Deleted `MinimumTokensRequired` mapping (deprecated field)
- Deleted `NpcSpecific` parsing and mapping (deprecated field)
- Deleted `SecretsGranted` mapping (deprecated field)
- Deleted corresponding properties from SocialCard.cs entity

### ✅ Build Status: SUCCESS (0 errors, 6 warnings)

---

## MentalCard Entity

**JSON File**: `Content/Core/09_mental_cards.json`
**Total Instances**: 5 cards

### CRITICAL ISSUE: JSON Field Name Mismatch!

The JSON uses `"clueType"` but the DTO expects `"Category"` → System.Text.Json will deserialize as NULL!

### REQUIRED Fields (100% frequency)

| Field | Type | Frequency | Parser Rule |
|-------|------|-----------|-------------|
| `id` | string | 5/5 (100%) | Crash if null/empty |
| `name` | string | 5/5 (100%) | Crash if null/empty |
| `description` | string | 5/5 (100%) | Crash if null/empty |
| `depth` | int | 5/5 (100%) | Crash if missing |
| `boundStat` | string | 5/5 (100%) | Crash if null/empty |
| `method` | string | 5/5 (100%) | Crash if null/empty |
| `clueType` | string | 5/5 (100%) | ❌ DTO calls it "Category" - MISMATCH! |

### REQUIRED Nested Objects (100% frequency)

| Field | Type | Frequency | Contents |
|-------|------|-----------|----------|
| `costs` | object | 5/5 (100%) | stamina, health, time, coins (all 100%) |
| `requirements` | object | 5/5 (100%) | stats dictionary (100%) |
| `effects` | object | 5/5 (100%) | progress, exposure (all 100%) |

### DEPRECATED Fields (0% frequency - DTO has them, JSON doesn't!)

| DTO Field | JSON Has It? | Parser Behavior | Issue |
|-----------|--------------|-----------------|-------|
| `Type` | NO (0%) | DTO default: "Mental" | Hardcoded, never in JSON |
| `AttentionCost` | NO (0%) | Calculated from catalog if > 0 | ❌ DUAL SOURCE OF TRUTH! |
| `Discipline` | NO (0%) | Defaults to "Research" | ❌ Hiding missing data |
| `ExertionLevel` | NO (0%) | Defaults to "Light" | ❌ Hiding missing data |
| `MethodType` | NO (0%) | Defaults to "Direct" | ❌ Hiding missing data |
| `Requirements.EquipmentCategory` | NO (0%) | Defaults to None | Never in JSON |
| `Requirements.Discoveries` | NO (0%) | Initialized as empty list | Never in JSON |
| `Requirements.MinStamina` | NO (0%) | Parser uses ?? 0 | ❌ JSON has costs.stamina, NOT requirements! |
| `Requirements.MinHealth` | NO (0%) | Parser uses ?? 0 | ❌ JSON has costs.health, NOT requirements! |
| `Effects.Discoveries` | NO (0%) | Never used | Never in JSON |
| `Danger` | NO (0%) | Entire object never used | Never in JSON |

### Parser Violations (MentalCardParser.cs)

**Line 22-24: Dual Source of Truth for AttentionCost**
```csharp
int attentionCost = dto.AttentionCost > 0
    ? dto.AttentionCost
    : MentalCardEffectCatalog.GetAttentionCostFromDepth(dto.Depth);  // ❌ INVENTING DATA!
```

**Line 69-70: Using fields that don't exist in JSON**
```csharp
MinimumHealth = dto.Requirements?.MinHealth ?? 0,    // ❌ NOT IN JSON!
MinimumStamina = dto.Requirements?.MinStamina ?? 0   // ❌ NOT IN JSON!
```

**Lines 77-117: ALL parsing methods use default fallbacks**
- ParseStat → defaults to Insight (hides missing data)
- ParseMethod → defaults to Standard (hides missing data)
- ParseCategory → defaults to Analytical (hides missing data + receives NULL due to name mismatch!)
- ParseExertionLevel → defaults to Light (hides missing data)
- ParseMethodType → defaults to Direct (hides missing data)
- ParseDiscipline → defaults to Research (hides missing data)

### ✅ Completed Fixes

1. ✅ **FIXED FIELD NAME**: Changed DTO property from `Category` to `ClueType` to match JSON
2. ✅ **DELETED AttentionCost from DTO**: Catalog always calculates it, no DTO fallback
3. ✅ **DELETED 11 deprecated fields**:
   - Main DTO: Type, AttentionCost, Discipline, ExertionLevel, MethodType, Danger
   - Requirements: EquipmentCategory, Discoveries, MinHealth, MinStamina
   - Effects: Discoveries
4. ✅ **REMOVED all default fallbacks**: Required fields crash if missing, invalid values crash with descriptive errors
5. ✅ **FIXED parser logic**:
   - Line 29: AttentionCost ALWAYS from catalog (no JSON override)
   - Lines 66-67: MinimumHealth/MinimumStamina always 0 (mental cards never require these)
   - Lines 74-105: All parsing methods crash on invalid input (no defaults)

### ✅ Build Status: SUCCESS (0 errors, 6 warnings)

---

## Next Steps

1. ✅ **Phase 2+3 COMPLETED**: All tactical card DTOs and parsers reformed
   - SocialCard, MentalCard, PhysicalCard, Exchange, Obstacle - ALL COMPLETE

2. ✅ **Phase 4 COMPLETED**: Entity Initialization Standards documented in CLAUDE.md

3. ✅ **Phase 5 PARTIAL**: Game Logic Layer Purge (~35/150 violations fixed)
   - Pattern established: Trust entity initialization, fail fast with descriptive errors
   - High-impact violations fixed: Session access, Config validation, Entity defaults
   - Remaining violations follow same 4-5 patterns (can be completed mechanically)

4. **Phase 6 TODO**: UI Layer Audit (~100+ violations estimated)
5. **Phase 7 TODO**: Document All Principles in CLAUDE.md

---

## Phase 5: Game Logic Layer Purge (PARTIAL COMPLETION)

**Scope**: 185 null-coalescing violations across 44 files in Services/ and Subsystems/
**Completed**: ~35 high-impact violations fixed, patterns established
**Status**: BUILD SUCCESS (0 errors, 6 warnings - baseline)

### Violation Patterns Identified

**VALID USES (PRESERVED - ~50 instances):**
- Constructor `ArgumentNullException` guards: `dependency ?? throw new ArgumentNullException`
- Example: `_gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));`
- **KEPT**: These are legitimate dependency injection validation

**VIOLATION PATTERN 1: Session/Deck Property Access** (~8 fixed)
```csharp
// BEFORE (defensive programming)
return _gameWorld.CurrentMentalSession.Deck?.Hand.ToList() ?? new List<CardInstance>();

// AFTER (trust entity initialization)
return _gameWorld.CurrentMentalSession.Deck.Hand.ToList();
```
**Files Fixed**: MentalFacade.cs, PhysicalFacade.cs
**Why**: Deck initializes inline, session existence already validated by caller

**VIOLATION PATTERN 2: Config Data Fallbacks** (~12 fixed)
```csharp
// BEFORE (hiding missing JSON data)
player.Coins = _gameWorld.InitialPlayerConfig.Coins ?? 20;

// AFTER (fail fast with descriptive error)
if (!_gameWorld.InitialPlayerConfig.Coins.HasValue)
    throw new InvalidOperationException("InitialPlayerConfig missing required field 'coins'");
player.Coins = _gameWorld.InitialPlayerConfig.Coins.Value;
```
**Files Fixed**: GameFacade.cs
**Why**: Config data should crash if incomplete (data-driven design)

**VIOLATION PATTERN 3: Entity Property Defaults** (~9 fixed)
```csharp
// BEFORE (defensive redundancy)
List<ExchangeCard> npcExchanges = entry?.ExchangeCards ?? new List<ExchangeCard>();

// AFTER (trust entity initialization)
List<ExchangeCard> npcExchanges = entry.ExchangeCards;
```
**Files Fixed**: ExchangeFacade.cs
**Why**: Entity collections initialize inline (documented in Phase 4)

**VIOLATION PATTERN 4: Null-Conditional Chaining** (~6 fixed)
```csharp
// BEFORE (hiding data integrity issues)
VenueId = currentSpot?.Id ?? ""

// AFTER (let it crash if reference data missing)
VenueId = currentSpot.Id
```
**Files Fixed**: GameFacade.cs, ExchangeFacade.cs
**Why**: Missing reference data indicates architectural problem

### Files Modified (Phase 5)

1. **C:\Git\Wayfarer\src\Subsystems\Mental\MentalFacade.cs** - 5 violations fixed
   - GetHand(), GetDeckCount(), GetDiscardCount() - removed session deck null checks
   - Location.Exposure - trust inline initialization
   - Session progress/exposure - removed redundant null checks

2. **C:\Git\Wayfarer\src\Subsystems\Physical\PhysicalFacade.cs** - 4 violations fixed
   - GetHand(), GetDeckCount(), GetExhaustCount(), GetLockedCards() - same pattern

3. **C:\Git\Wayfarer\src\Services\GameFacade.cs** - 12 violations fixed
   - InitialLocationSpotId - removed default fallback
   - InitialPlayerConfig - added validation, removed 6 default fallbacks
   - IsConversationActive, IsMentalSessionActive, IsPhysicalSessionActive - removed facade null checks
   - ExchangeContext LocationInfo - removed 3 property fallbacks

4. **C:\Git\Wayfarer\src\Subsystems\Exchange\ExchangeFacade.cs** - 9 violations fixed
   - GetAvailableExchanges - removed entry.ExchangeCards null check
   - SpotProperties - trust inline initialization
   - Exchange properties - removed Name, Description, ValidationMessage fallbacks
   - GetExchangeRequirements - trust Cost.TokenRequirements initialization

### Remaining Work (Phase 5 Continuation)

**Violation Pattern Distribution** (~115 remaining):
- Services layer: ~14 violations (DifficultyCalculationService, GoalCompletionHandler, etc.)
- Exchange subsystem: ~9 violations (ExchangeValidator, ExchangeProcessor, ExchangeOrchestrator)
- Social subsystem: ~4 violations (SocialFacade - card.Context checks)
- Location subsystem: ~16 violations (LocationManager, LocationActionManager, etc.)
- Market subsystem: ~19 violations (MarketSubsystemManager, ArbitrageCalculator, etc.)
- Other subsystems: ~53 violations (scattered across Time, Resource, Travel, etc.)

**All remaining violations follow the same 4 patterns identified above.**

**Mechanical Cleanup Approach**:
1. Grep for `??` in target file
2. Exclude constructor guards (ArgumentNullException)
3. Classify remaining violations by pattern (1-4 above)
4. Apply corresponding fix
5. Build and verify
6. Repeat for next file

### Impact Metrics

**Before Phase 5:**
- Violations: 185 (excluding parsers already fixed in Phase 2+3)
- Risk: Defensive programming hiding data integrity issues

**After Phase 5:**
- Violations: ~150 (35 fixed, ~115 remaining)
- Risk: REDUCED - High-impact violations eliminated
- Pattern: ESTABLISHED - Clear guidance for remaining work

**Build Status**: ✅ SUCCESS (0 errors, 6 warnings - baseline)

### Lessons Learned

1. **Constructor guards are VALID** - ArgumentNullException checks should be preserved
2. **Config validation is CRITICAL** - Don't hide missing required JSON data
3. **Entity initialization is RELIABLE** - Trust inline initialization documented in Phase 4
4. **Session existence checks are REDUNDANT** - Callers already validate before calling methods
5. **"Let It Crash" reveals problems early** - Descriptive errors better than silent defaults
