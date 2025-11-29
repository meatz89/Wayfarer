# Code Style & Constraints Compliance Audit

## Status: COMPLETE

**Audit Date:** 2025-11-29
**Auditor:** Claude Code Compliance System
**Scope:** All .cs files under /home/user/Wayfarer/src/

## Constraints Being Checked (from arc42/02_constraints.md)
- NO `var` keyword - explicit types only
- List<T> for collections - NO Dictionary or HashSet for domain entities
- Named methods - NO anonymous delegates/lambdas in backend (LINQ allowed)
- int for numbers - NO float/double
- JSON field names = C# property names (no JsonPropertyName attributes)

## Methodology
1. Comprehensive grep searches across all .cs files under src/
2. Manual review of findings to distinguish domain code from framework/test code
3. Classification of violations by severity (domain vs test code)
4. File:line references for all violations
5. Analysis of float/double usage to separate domain violations from acceptable framework uses

## Executive Summary

| Constraint | Status | Violations |
|------------|--------|------------|
| No `var` keyword | PASS | 0 (only 1 commented-out) |
| No Dictionary/HashSet in domain | FAIL | 2 |
| No lambdas in backend | PASS | 0 |
| No float/double in domain | FAIL | ~100+ |
| No JsonPropertyName | PARTIAL | 3 domain, multiple external API |
| No extension methods | PASS | 0 |
| No Helper/Utility classes | FAIL | 3 directories + 1 class |
| No TODO comments | FAIL | 1 |

**SEVERITY ASSESSMENT:**
- **CRITICAL**: float/double usage pervasive in domain code (market, tokens, exertion)
- **HIGH**: Helper/Utility naming violations (directories and classes)
- **MEDIUM**: Dictionary usage in StandingObligation domain entity
- **LOW**: JsonPropertyName in LocationDTO, TODO comment

## Detailed Findings

### 1. var Usage - COMPLIANT ✓
**Status:** Only 1 commented-out instance found
- `/home/user/Wayfarer/src/Services/LocationActionExecutor.cs:78` - commented out

### 2. Dictionary/HashSet in Domain Code - VIOLATIONS FOUND ✗

**Domain Entity Violations:**
- `/home/user/Wayfarer/src/GameState/StandingObligation.cs` - `public Dictionary<int, float> SteppedThresholds`
  - Domain entity using Dictionary
  - Also uses float (double violation)

**DTO Violations:**
- `/home/user/Wayfarer/src/Content/DTOs/StandingObligationDTO.cs:28` - `public Dictionary<int, float> SteppedThresholds`
  - DTO matching JSON structure, but JSON should be refactored to use List

**Note:** No other Dictionary/HashSet usage found - rest of codebase is compliant!

### 3. Anonymous Delegates/Lambdas in Backend - COMPLIANT ✓
**Status:** No Action<> or Func<> found in backend services
- All LINQ lambdas (allowed)
- No DI registration lambdas
- No backend event handler lambdas

### 4. float/double in Domain Code - EXTENSIVE VIOLATIONS ✗

#### DOMAIN CODE VIOLATIONS (Must use int):

**Market Subsystem (PriceManager.cs):**
- Line 13: `private const float MIN_PRICE_MULTIPLIER = 0.5f;`
- Line 14: `private const float MAX_PRICE_MULTIPLIER = 2.5f;`
- Line 15: `private const float BUY_SELL_SPREAD = 0.15f;`
- Lines 39-42: `float SupplyModifier/DemandModifier/LocationModifier/FinalModifier`
- Line 159: `private float CalculateSupplyModifier(...)`
- Line 161: `float supplyLevel`
- Line 179: `private float CalculateDemandModifier(...)`
- Line 181: `float demandLevel`
- Line 199: `private float CalculateLocationModifier(...)`
- Line 201: `float modifier = 1.0f;`
- Line 361: `float trendModifier = 1.0f;`
- Line 384: `public float CalculatePriceVolatility(...)`
- Lines 389-391: `float avgPrice/variance/stdDev`
- Line 415: `float currentDemand`
- Line 441: `float discount = 1.0f;`

**Market Subsystem (MarketStateTracker.cs):**
- Line 35: `public float SupplyLevel { get; set; } = 1.0f;`
- Line 36: `public float DemandLevel { get; set; } = 1.0f;`

**Market Subsystem (MarketSubsystemManager.cs):**
- Line 65: `public float Confidence { get; set; }`
- Line 154: `public float SupplyLevel { get; set; }`

**Token Subsystem (TokenEffectProcessor.cs):**
- Line 33: `float totalModifier`
- Line 154: `float decayRate`
- Line 158: `float decayMultiplier`
- Line 169: `private float GetEquipmentTokenModifier(...)`
- Line 172: `float totalModifier = 1.0f;`
- Line 179: `item.TokenGenerationModifiers.TryGetValue(tokenType, out float modifier)`
- Line 242: `private float GetDecayRate(...)`

**Token Subsystem (RelationshipTracker.cs):**
- Line 279: `float decayRate`
- Line 290: `float decayMultiplier`

**Token Subsystem (TokenMechanicsManager.cs):**
- Line 72: `float modifier`
- Line 333: `private float GetEquipmentTokenModifier(...)`
- Line 338: `float totalModifier = 1.0f;`
- Line 345: `item.TokenGenerationModifiers.TryGetValue(tokenType, out float modifier)`

**Player Exertion (PlayerExertionCalculator.cs):**
- Line 28: `float staminaPercent = (float)player.Stamina / maxStamina;`
- Line 49: `float healthPercent = (float)player.Health / maxHealth;`
- Line 73: `float avgPercent`

**Travel Subsystem (HexRouteGenerator.cs):**
- Line 166: `float pathCost` parameter
- Line 409: `float spacing`
- Line 602: `float totalTime = 0;`
- Line 625: `float totalStamina = 0;`
- Line 633: `float terrainStamina`
- Line 663: `private float GetTerrainTimeMultiplier(...)`

**Travel Subsystem (TravelTimeCalculator.cs):**
- Line 63: `double modifier`
- Line 89: `private double GetTransportModifier(...)`

**Emergency System (EmergencyCatalog.cs):**
- Line 43: `public static double GetCostScalingFactor(...)`
- Line 59: `public static double GetRewardScalingFactor(...)`

**StandingObligation (DTOs and Domain):**
- `/home/user/Wayfarer/src/Content/DTOs/StandingObligationDTO.cs`:
  - Line 24: `public float ScalingFactor { get; set; } = 1.0f;`
  - Line 25: `public float BaseValue { get; set; } = 0f;`
  - Line 26: `public float MinValue { get; set; } = 0f;`
  - Line 27: `public float MaxValue { get; set; } = 100f;`

**Game Constants:**
- `/home/user/Wayfarer/src/GameState/Constants/GameConstants.cs:65`
  - `public const float DEFAULT_ENCOUNTER_CHANCE = 0.3f;`

#### ACCEPTABLE USES (Framework/External API):

**A* Algorithm (PathfindingService.cs):**
- Lines 44-45, 61, 82-83, 108, 155, 231, 250-251, 271, 273, 284: float for A* precision
- **JUSTIFIED**: Line 231 has explicit comment "TYPE SYSTEM: Internal A* uses float for precision, public API converts to int"

**External Layout API (SpawnGraphBuilder.cs):**
- Lines 415, 426-429, 621-622, 670, 673, 682, 685, 688, 691, 709, 712: double for dagre layout
- **JUSTIFIED**: External JavaScript interop for graph layout

**UI Components (Blazor):**
- `StreamingContentState.cs:7` - float StreamProgress (UI progress bar)
- `ResourceBar.razor.cs:16` - double PercentageWidth (CSS)
- `PhysicalContent.razor.cs:787` - double percentage (UI)
- `InventoryContent.razor.cs:43` - double GetWeightPercent (UI)
- `ObligationProgressModal.razor.cs:15` - double GetProgressPercent (UI)
- `HexMapContent.razor.cs:25,79-90` - double for CSS pixel positioning
- `DiscoveryJournal.razor.cs:49,136` - double for UI percentages
- `MentalContent.razor.cs:705` - double percentage (UI)
- **JUSTIFIED**: Blazor framework and CSS require double for percentages/positioning

**Validation (ItemValidator.cs):**
- Line 111: "double" is a string literal in error message, not a type usage

### 5. JsonPropertyName Attributes - VIOLATIONS FOUND ✗

**External API (ACCEPTABLE):**
- `/home/user/Wayfarer/src/Services/SpawnGraph/SpawnGraphBuilder.cs:627-711`
  - Multiple JsonPropertyName attributes for dagre layout DTOs
  - **JUSTIFIED**: External JavaScript API with camelCase conventions

**Domain DTOs (VIOLATIONS):**
- `/home/user/Wayfarer/src/Content/DTOs/LocationDTO.cs:61` - `[JsonPropertyName("proximityConstraint")]`
- `/home/user/Wayfarer/src/Content/DTOs/LocationDTO.cs:74` - `[JsonPropertyName("proximity")]` (ProximityConstraintDTO)
- `/home/user/Wayfarer/src/Content/DTOs/LocationDTO.cs:80` - `[JsonPropertyName("referenceLocation")]` (ProximityConstraintDTO)

**VIOLATION**: Domain DTOs should have property names that match JSON exactly without attributes. Either:
- Rename C# properties to match JSON (ProximityConstraint → proximityConstraint), OR
- Update JSON to match C# property names (proximityConstraint → ProximityConstraint)

### 6. Extension Methods - COMPLIANT ✓
**Status:** No extension methods found
- Note: `/home/user/Wayfarer/src/GameState/Helpers/ListBasedHelpers.cs:150` has comment "ALL EXTENSION METHODS DELETED"

### 7. Helper/Utility Classes - VIOLATIONS FOUND ✗

**Directory Name Violations:**
- `/home/user/Wayfarer/src/UIHelpers/` - Directory should be renamed (e.g., UI, UIState, UIServices)
- `/home/user/Wayfarer/src/GameState/Helpers/` - Directory should be renamed
- `/home/user/Wayfarer/src/Content/Utilities/` - Directory should be renamed (e.g., Content/Parsing)

**File Name Violations:**
- `/home/user/Wayfarer/src/GameState/Helpers/ListBasedHelpers.cs` - File contains legitimate domain entity classes (ResourceEntry, NPCTokenEntry, etc.) but has "Helper" in name
  - **ACTION REQUIRED**: Rename file to match its purpose (e.g., DomainEntries.cs, CollectionEntries.cs)

**Actual Utility Class Violations:**
- `/home/user/Wayfarer/src/Content/Utilities/EnumParser.cs`
  - **VIOLATION**: Static utility class with generic parsing methods
  - **ACTION REQUIRED**: Move enum parsing to domain services or eliminate generic approach

**Files in UIHelpers directory (analysis):**
- `CurrentViews.cs` - Enum (not a helper, legitimate)
- `StreamingContentState.cs` - UI state class (legitimate, but directory name violates)
- `MusicService.cs` - Service class (legitimate, but directory name violates)
- `Track.cs` - Domain class (legitimate, but directory name violates)

### 8. TODO Comments - VIOLATION FOUND ✗
- `/home/user/Wayfarer/src/Subsystems/Consequence/RewardApplicationService.cs:292`
  - `// TODO: Refactor all callers to use Consequence directly (HIGHLANDER)`
  - **VIOLATION**: Per CLAUDE.md "RULE #0: NO HALF MEASURES" - no TODO comments allowed

## Recommendations

### Priority 1: CRITICAL - float/double in Domain Code

**Impact:** 100+ violations across market, token, and travel subsystems

**Recommended Approach:**
1. **Market System**: Convert all price modifiers from float to int using basis points
   - Example: `0.5f` multiplier → `5000` basis points (where 10000 = 1.0x)
   - Example: `1.15f` multiplier → `11500` basis points
   - Benefits: Eliminates floating point, maintains precision, common financial practice

2. **Token System**: Convert decay rates and modifiers to int percentages
   - Example: `1.0f` modifier → `100` (percentage)
   - Example: `0.1f` decay → `10` (10% decay per week)

3. **Travel System**: Convert time/stamina to int (already using int in many places)
   - Example: `float totalTime` → `int totalTimeMinutes`
   - Example: `float spacing` → `int spacingUnits`

4. **Standing Obligations**: Change scaling from float to int percentages
   - `ScalingFactor: 1.0f` → `ScalingPercentage: 100`
   - Update JSON files to match

5. **Game Constants**: Convert encounter chance to int percentage
   - `DEFAULT_ENCOUNTER_CHANCE = 0.3f` → `DEFAULT_ENCOUNTER_CHANCE_PERCENT = 30`

**Files to Modify:**
- PriceManager.cs (13 float occurrences)
- MarketStateTracker.cs (2 float properties)
- MarketSubsystemManager.cs (2 float properties)
- TokenEffectProcessor.cs (8 float occurrences)
- RelationshipTracker.cs (2 float occurrences)
- TokenMechanicsManager.cs (4 float occurrences)
- PlayerExertionCalculator.cs (3 float calculations)
- HexRouteGenerator.cs (6 float occurrences)
- TravelTimeCalculator.cs (2 double occurrences)
- EmergencyCatalog.cs (2 double methods)
- StandingObligationDTO.cs (4 float properties + JSON files)
- GameConstants.cs (1 float constant)

### Priority 2: HIGH - Helper/Utility Naming Violations

**Impact:** 3 directories + 1 file + 1 static utility class

**Recommended Actions:**
1. **Rename directories:**
   - `src/UIHelpers/` → `src/UI/State/` or `src/UI/Services/`
   - `src/GameState/Helpers/` → `src/GameState/Entities/` or `src/GameState/Domain/`
   - `src/Content/Utilities/` → `src/Content/Parsing/`

2. **Rename file:**
   - `ListBasedHelpers.cs` → `CollectionEntries.cs` or `DomainEntries.cs`
   - Update all using statements and references

3. **Refactor EnumParser.cs:**
   - Move to a proper service class OR
   - Eliminate generic approach and use direct Enum.Parse calls OR
   - Convert to instance methods in parser classes that need it

### Priority 3: MEDIUM - Dictionary in StandingObligation

**Impact:** 1 domain entity + 1 DTO

**Recommended Approach:**
1. Create `SteppedThresholdEntry` class:
   ```csharp
   public class SteppedThresholdEntry
   {
       public int Threshold { get; set; }
       public int Value { get; set; }  // Changed from float
   }
   ```

2. Replace `Dictionary<int, float> SteppedThresholds` with `List<SteppedThresholdEntry> SteppedThresholds`

3. Update JSON structure:
   ```json
   "steppedThresholds": [
       {"threshold": 50, "value": 100},
       {"threshold": 100, "value": 150}
   ]
   ```

4. Update all query code to use LINQ on List instead of Dictionary lookups

**Files to Modify:**
- StandingObligation.cs
- StandingObligationDTO.cs
- StandingObligationParser.cs
- standing_obligations.json (if it exists)

### Priority 4: LOW - Miscellaneous Violations

**JsonPropertyName in LocationDTO:**
- Option A: Rename C# properties to camelCase (violates C# conventions)
- Option B: Update JSON to PascalCase (recommended)
  - `proximityConstraint` → `ProximityConstraint`
  - `proximity` → `Proximity`
  - `referenceLocation` → `ReferenceLocation`

**TODO Comment:**
- Remove TODO comment in RewardApplicationService.cs:292
- Either complete the refactoring or delete the comment

## Implementation Strategy

### Phase 1: Quick Wins (Low Risk)
1. Remove TODO comment
2. Rename directories and files
3. Fix JsonPropertyName (update JSON files)

### Phase 2: Structural Changes (Medium Risk)
1. Convert Dictionary to List in StandingObligation
2. Refactor or eliminate EnumParser utility class

### Phase 3: Major Refactoring (High Risk - Requires Testing)
1. Convert all float/double to int in domain code
2. Update all calculations to use basis points/percentages
3. Comprehensive testing of market, token, and travel systems
4. Update any JSON files that store float values

## Testing Requirements

After fixing violations:
- Run full test suite: `cd src && dotnet test`
- Manual testing of:
  - Market price calculations
  - Token generation and decay
  - Travel time/stamina calculations
  - Standing obligation activation/scaling
  - Encounter chance calculations

## Estimated Effort

| Priority | Violations | Estimated Hours | Risk |
|----------|-----------|-----------------|------|
| Priority 1 (float/double) | 100+ | 16-24 hours | High |
| Priority 2 (naming) | 3 dirs + 1 file + 1 class | 2-4 hours | Low |
| Priority 3 (Dictionary) | 2 files | 2-3 hours | Medium |
| Priority 4 (misc) | 4 items | 1-2 hours | Low |
| **TOTAL** | **110+** | **21-33 hours** | **Mixed** |

## Remaining TODOs
- [x] Initialize report
- [x] Check for `var` usage
- [x] Check for Dictionary/HashSet in domain code
- [x] Check for forbidden lambdas in backend
- [x] Check for float/double/decimal
- [x] Check for JsonPropertyName attributes
- [x] Check for extension methods
- [x] Check for Helper/Utility classes
- [x] Check for TODO comments
- [x] Generate summary statistics
- [x] Create recommendations for fixes

## Status: COMPLETE
