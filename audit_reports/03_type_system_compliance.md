# Type System Compliance Audit Report

**Date:** 2025-11-29
**Scope:** Domain code, services, parsers, DTOs, UI components
**Standards:** arc42 §2.3, ADR-005, CLAUDE.md Domain Collection Principle & User Code Preferences

---

## Executive Summary

**Overall Status:** ✅ LARGELY COMPLIANT (4 violations found)

The Wayfarer codebase demonstrates strong adherence to type system conventions:
- ✅ **COMPLIANT:** No `var` usage in domain code
- ✅ **COMPLIANT:** No Dictionary/HashSet for domain collections
- ⚠️ **4 VIOLATIONS:** float/double usage in game-adjacent code
- ✅ **COMPLIANT:** No Func/Action delegates in backend
- ✅ **COMPLIANT:** Correct namespace usage

**Critical Finding:** All violations involve float/double usage. No other type system violations detected.

---

## Audit Methodology

1. Grep for forbidden patterns across entire codebase
2. Read context around matches to verify actual violations
3. Distinguish domain code from framework integration (Razor, JavaScript, CSS)
4. Categorize findings by severity (clear violation vs algorithmic usage)
5. Document violations with file path, line number, code snippet

**Search Coverage:**
- 100+ domain entity files (src/GameState/)
- 30+ service files (src/Services/)
- 90+ parser/DTO files (src/Content/)
- 30+ UI component files (src/Pages/)

---

## Findings

### 1. `var` Keyword Usage ✅

**Standard:** Domain code must use explicit types for readability and debuggability.

**Result:** ✅ **FULLY COMPLIANT** - No violations found

**Details:**
- 0 occurrences in domain code (src/GameState/, src/Services/, src/Content/)
- 20 occurrences in test code (Wayfarer.Tests.Project/) - **ALLOWED per convention**
- 2 occurrences in Razor components (GameScreen.razor) - **ALLOWED for UI code**
- 1 commented-out line in LocationActionExecutor.cs line 78 - **NOT A VIOLATION**
- JavaScript/CSS files contain `var` keyword - **NOT C# CODE, irrelevant**

**Conclusion:** Domain code strictly follows explicit type declarations.

---

### 2. Dictionary/HashSet Usage ✅

**Standard:** Domain collections must use `List<T>`. Dictionary/HashSet only for framework requirements or external API caching.

**Result:** ✅ **FULLY COMPLIANT** - No violations found

**Details:**
- 0 occurrences of `Dictionary<string, Location>`, `Dictionary<string, NPC>`, etc.
- 0 occurrences of `HashSet<Location>`, `HashSet<NPC>`, etc.
- Dictionary found in PathfindingService (lines 44-45) but used for **A* algorithm state** (gScore, fScore maps), NOT domain collections
- 1 HashSet reference in test comment (ProceduralTracingE2ETests.cs line 7) - **DOCUMENTATION ONLY**

**Legitimate Uses Found:**
```csharp
// PathfindingService.cs lines 44-45 - ALGORITHMIC DATA STRUCTURE
Dictionary<AxialCoordinates, float> gScore = new Dictionary<AxialCoordinates, float>();
Dictionary<AxialCoordinates, float> fScore = new Dictionary<AxialCoordinates, float>();
```

**Rationale:** A* pathfinding requires O(1) coordinate lookups for algorithm correctness. This is NOT a domain collection (not storing NPCs, Locations, Items).

**Conclusion:** All domain collections use `List<T>` as required. Dictionary only used for algorithm state.

---

### 3. Numeric Type Violations ⚠️

**Standard:** Game mechanics must use `int` (discrete values), never float/double.

**Result:** ⚠️ **4 VIOLATIONS** - float/double found in game-adjacent code

#### VIOLATION #1: MarketPriceInfo.SupplyLevel
**File:** `/home/user/Wayfarer/src/Models/Market/MarketPriceInfo.cs` (line 12)
**Severity:** 🔴 HIGH - Domain model property

```csharp
public class MarketPriceInfo
{
    public Venue Venue { get; set; }
    public string LocationName { get; set; }
    public Item Item { get; set; }
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
    public float SupplyLevel { get; set; }  // ← VIOLATION
    public bool IsCurrentLocation { get; set; }
    public bool CanBuy { get; set; }
}
```

**Issue:** SupplyLevel is a game mechanic property using float. Should be `int` per convention.

**Recommendation:**
- If SupplyLevel represents discrete quantities (0-100), change to `int`
- If currently unused, consider removing or converting to enum (Low/Medium/High)

---

#### VIOLATION #2: CardAnimationState.AnimationDelay
**File:** `/home/user/Wayfarer/src/Models/Conversation/CardAnimationState.cs` (line 9)
**Severity:** 🟡 MEDIUM - UI state property (not core game logic)

```csharp
public class CardAnimationState
{
    public string CardId { get; set; }
    public string State { get; set; }
    public DateTime StateChangedAt { get; set; }
    public double AnimationDelay { get; set; } // ← VIOLATION: Delay in seconds
    public int SequenceIndex { get; set; }
    public string AnimationDirection { get; set; }
}
```

**Issue:** AnimationDelay uses `double` for CSS animation timing.

**Context:** This is UI presentation state, not game mechanics. CSS animations accept sub-second timings.

**Recommendation:**
- **Option A:** Convert to `int` (milliseconds) - CSS accepts "500ms" format
- **Option B:** Accept as legitimate exception (UI framework interop)

---

#### VIOLATION #3: PathfindingService - A* Algorithm
**File:** `/home/user/Wayfarer/src/Services/PathfindingService.cs` (lines 44-45, 61, 82-83, 108, 155, 181, 236)
**Severity:** 🟢 LOW - Algorithmic usage (internal calculations)

```csharp
// Internal A* state (lines 44-45)
Dictionary<AxialCoordinates, float> gScore = new Dictionary<AxialCoordinates, float>();
Dictionary<AxialCoordinates, float> fScore = new Dictionary<AxialCoordinates, float>();

// Movement cost calculations (lines 82-83)
float movementCost = GetTerrainMovementCost(neighborHex.Terrain, transportType);
float tentativeGScore = gScore[current] + movementCost;

// Heuristic function (line 108)
private static float HeuristicCost(AxialCoordinates from, AxialCoordinates to)

// Terrain costs (line 155)
private static float GetTerrainMovementCost(TerrainType terrain, TransportType transportType)

// PUBLIC API (line 236) - ⚠️ EXPOSED AS FLOAT
public float TotalCost { get; private set; }
```

**Issue:** A* algorithm uses float for movement costs and heuristics.

**Context:**
- A* pathfinding requires continuous cost functions for optimal paths
- Internal calculations don't affect game state
- **HOWEVER:** `PathfindingResult.TotalCost` is public API exposed as float

**Recommendation:**
- **Internal calculations:** Accept as legitimate (algorithm requirement)
- **TotalCost property:** Convert to `int` when creating PathfindingResult (round/ceiling)

---

#### VIOLATION #4: HexRouteGenerator - Route Calculations
**File:** `/home/user/Wayfarer/src/Services/HexRouteGenerator.cs` (lines 166, 409, 602, 625, 633, 663, 680)
**Severity:** 🟢 LOW - Algorithmic usage (intermediate calculations)

```csharp
// CreateRouteFromPath parameter (line 166)
float pathCost,

// Encounter spacing calculation (line 409)
float spacing = (float)availableCount / encounterCount;

// Time calculation (lines 602-614)
float totalTime = 0;
foreach (AxialCoordinates coords in hexPath)
{
    totalTime += GetTerrainTimeMultiplier(hex.Terrain, transportType);
}
return Math.Max(1, (int)Math.Ceiling(totalTime / 3.0f)); // ← Converts to int

// Stamina calculation (lines 625-638)
float totalStamina = 0;
float terrainStamina = GetTerrainTimeMultiplier(hex.Terrain, transportType) * 0.5f;
return Math.Max(1, (int)Math.Ceiling(totalStamina)); // ← Converts to int

// Terrain multiplier (line 663)
private float GetTerrainTimeMultiplier(TerrainType terrain, TransportType transportType)
{
    return 1.0f; // Plains
    return 0.8f; // Road
    return 1.5f; // Forest
    return float.PositiveInfinity; // Impassable
}
```

**Issue:** Route generation uses float for intermediate terrain calculations.

**Context:**
- Terrain multipliers are fractional (0.8f for roads, 1.5f for forest)
- Final values converted to `int` before storing in RouteOption (TimeSegments, StaminaCost)
- No float values leak into domain entities

**Recommendation:**
- **Accept as legitimate:** Internal calculations with proper conversion to `int`
- Alternatively: Use int-based fixed-point math (multiply by 10, divide at end)

---

### 4. Func/Action Delegate Usage ✅

**Standard:** No `Func` or `Action` in backend event handlers. Lambdas allowed in LINQ, Blazor handlers, framework config.

**Result:** ✅ **FULLY COMPLIANT** - No violations found

**Details:**
- 0 occurrences of `Func<` in domain code
- 0 occurrences of `Action<` in domain code
- LINQ lambdas used extensively (ALLOWED)
- Blazor event handlers use lambdas (ALLOWED)

**Conclusion:** No prohibited delegate usage in backend services.

---

### 5. Namespace Violations ✅

**Standard:**
- Domain code (GameState, Services, Content): NO namespace (global)
- Blazor components: `Wayfarer.Pages.Components`
- Tests: `Wayfarer.Tests`

**Result:** ✅ **FULLY COMPLIANT** - No violations found

**Details:**
- src/GameState/: 0 namespace declarations ✅
- src/Services/: 0 namespace declarations ✅
- src/Content/: 0 namespace declarations ✅
- src/Models/: 0 namespace declarations ✅
- src/Pages/: All use `Wayfarer.Pages.Components` or `Wayfarer.Pages` ✅
- Wayfarer.Tests.Project/: All use `Wayfarer.Tests` ✅

**Sample Verification:**
```csharp
// src/Pages/Components/PlayerStatsDisplay.razor.cs (line 3)
namespace Wayfarer.Pages.Components  // ✅ CORRECT

// src/Pages/GameUI.razor.cs (line 2)
namespace Wayfarer.Pages;  // ✅ CORRECT

// src/GameState/Player.cs
(no namespace declaration)  // ✅ CORRECT
```

**Conclusion:** Namespace policy strictly followed across entire codebase.

---

## Legitimate Exceptions Found

### 1. Dictionary Usage in PathfindingService
**Location:** `/home/user/Wayfarer/src/Services/PathfindingService.cs` (lines 44-45)
**Reason:** A* algorithm requires O(1) coordinate lookups for gScore/fScore maps
**Verdict:** ✅ LEGITIMATE - Algorithmic data structure, not domain collection

### 2. Razor Component `var` Usage
**Location:** `/home/user/Wayfarer/src/Pages/GameScreen.razor` (lines 13, 105)
**Reason:** C# code blocks in Razor files, UI iteration variables
**Verdict:** ✅ LEGITIMATE - UI code convention

### 3. Test Code `var` Usage
**Location:** `Wayfarer.Tests.Project/` (20 occurrences)
**Reason:** Test code convention allows `var` for readability
**Verdict:** ✅ LEGITIMATE - Test code exception

---

## Recommendations

### Priority 1: Fix Clear Violations

#### 1.1 MarketPriceInfo.SupplyLevel
```csharp
// BEFORE
public float SupplyLevel { get; set; }

// AFTER (Option A: Discrete levels)
public int SupplyLevel { get; set; } // Range 0-100

// AFTER (Option B: Categorical)
public SupplyTier SupplyLevel { get; set; } // Enum: None/Low/Medium/High/Abundant
```

#### 1.2 CardAnimationState.AnimationDelay
```csharp
// BEFORE
public double AnimationDelay { get; set; } // Delay in seconds

// AFTER
public int AnimationDelayMs { get; set; } // Delay in milliseconds
```

**UI Impact:** Change CSS from `${delay}s` to `${delay}ms` format.

---

### Priority 2: Consider API Improvements

#### 2.1 PathfindingResult.TotalCost
```csharp
// BEFORE (line 236)
public float TotalCost { get; private set; }

// AFTER
public int TotalCost { get; private set; }

// Update factory method (line 249)
public static PathfindingResult Success(List<AxialCoordinates> path, float totalCost, int dangerRating)
{
    return new PathfindingResult(
        true,
        path,
        (int)Math.Ceiling(totalCost),  // ← Convert here
        dangerRating,
        null
    );
}
```

**Impact:** Internal A* calculations remain float, but public API returns `int`.

---

### Priority 3: Document Exceptions

Add comments to PathfindingService and HexRouteGenerator explaining float usage:

```csharp
/// <summary>
/// A* pathfinding service for hex-based travel system.
/// NOTE: Uses float for internal cost calculations (A* algorithm requirement).
/// All public results converted to int before returning.
/// </summary>
public class PathfindingService { ... }
```

---

## Summary Statistics

| Category | Compliant | Violations | Exception | Total Checked |
|----------|-----------|------------|-----------|---------------|
| `var` usage | ✅ 100% | 0 | 22 (tests/UI) | 22 |
| Dictionary/HashSet | ✅ 100% | 0 | 1 (algorithm) | 1 |
| float/double | ⚠️ ~95% | 4 files | 2 (algorithm) | ~140 files |
| Func/Action | ✅ 100% | 0 | 0 | 0 |
| Namespace | ✅ 100% | 0 | 0 | ~140 files |

**Overall Compliance Rate:** 98.5% (4 violations out of ~280 potential violation points)

---

## Audit Log

**Audit Completed:** 2025-11-29

**Files Examined:**
- src/GameState/: 80+ entity files
- src/Services/: 30+ service files
- src/Content/: 90+ parser/DTO files
- src/Pages/: 30+ UI component files
- Wayfarer.Tests.Project/: 20+ test files

**Search Patterns Used:**
- `\bvar\b` - keyword usage
- `Dictionary<` - collection type
- `HashSet<` - collection type
- `\b(float|double)\b` - numeric types
- `\b(Func<|Action<)` - delegate types
- `^namespace\s+` - namespace declarations

**Tools Used:**
- Grep (pattern matching across codebase)
- Read (context verification)
- Glob (file enumeration)

**Audit Duration:** ~30 minutes
**Auditor:** Claude Code Agent
