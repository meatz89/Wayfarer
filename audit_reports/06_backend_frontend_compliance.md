# Backend/Frontend Separation Compliance Audit

**Date:** 2025-11-29
**Auditor:** Claude Code Agent
**Scope:** Backend/Frontend separation per arc42 §8.6 and CLAUDE.md

## Audit Status
✅ **COMPLETE** - Systematic examination finished

---

## Summary

**Overall Compliance:** PARTIAL - Several violations found requiring correction

The codebase demonstrates understanding of Backend/Frontend separation principles in most areas, but contains **3 critical violations** in ViewModels and **1 major violation** in UI components.

**Critical Issues:**
- Backend ViewModels returning CSS class names (FocusClass property)
- Backend ViewModels returning icon names (WeatherIcon, Icon properties)
- UI component (TravelContent.razor.cs) containing game logic calculations

**Strengths:**
- Most UI components properly delegate to facades
- No presentation logic found in core services
- Good separation in LocationContent component (explicitly marked as "DUMB DISPLAY")

---

## Backend Presentation Violations

### CRITICAL: CssClass Properties in ViewModels

**Location:** `/home/user/Wayfarer/src/ViewModels/GameFacadeViewModels.cs`

**Violation 1 - Line 27:**
```csharp
public class TravelContextViewModel
{
    public string FocusClass { get; set; } // CSS class: "", "warning", "danger"
}
```

**Impact:** Backend is returning CSS class names ("warning", "danger") instead of domain semantics. Frontend should determine presentation based on domain state.

**Correct Pattern:**
```csharp
// Backend provides domain state
public int FocusPenalty { get; set; } // 0, 1, or 2

// Frontend converts to CSS
string GetFocusClass(int penalty) => penalty switch {
    0 => "",
    1 => "warning",
    2 => "danger",
    _ => ""
};
```

---

### CRITICAL: IconName Properties in ViewModels

**Violation 2 - Line 43 (GameFacadeViewModels.cs):**
```csharp
public class TravelContextViewModel
{
    public string WeatherIcon { get; set; }
}
```

**Violation 3 - Line 81 (GameFacadeViewModels.cs):**
```csharp
public class RouteTokenRequirementViewModel
{
    public string Icon { get; set; }
}
```

**Impact:** Backend is deciding which icons to display instead of providing domain data for frontend to interpret.

**Correct Pattern:**
```csharp
// Backend provides domain enum
public WeatherCondition CurrentWeather { get; set; }
public ConnectionType TokenType { get; set; }

// Frontend maps to icons in Blazor component
string GetWeatherIcon(WeatherCondition weather) => weather switch {
    WeatherCondition.Clear => "sun",
    WeatherCondition.Rain => "raindrop",
    _ => "cloud"
};
```

---

### Color Definitions in Backend

**BORDERLINE - Acceptable Pattern Found:**

**Location:** `/home/user/Wayfarer/src/Services/ObligationIntroResult.cs` (Line 12)
```csharp
public string ColorCode { get; set; } // e.g., "#7a8b5a"
```

**Analysis:** Backend provides raw color data (hex code), frontend transforms it for presentation in `ObligationIntroModal.razor.cs` (GetHeaderColor, GetHeaderColorLight, GetBorderColor methods). This is **ACCEPTABLE** - backend provides data, frontend handles presentation transformation.

**SpawnGraphLinkModel (GameState/SpawnGraph/SpawnGraphLinkModel.cs):**
- Contains CssClass and Color properties
- Only used in UI visualization components (SpawnGraph.razor)
- Not core game state - dev/debug tool only
- **VERDICT:** Acceptable as UI-specific model

---

## Service Presentation Logic

### Display String Generation

**Status:** NO VIOLATIONS FOUND

Searched for DisplayName, DisplayText, GetDisplayString, FormatDisplay patterns in `/home/user/Wayfarer/src/Services/`.

Only finding was a commented-out line in GameFacade.cs:
```csharp
// _messageSystem.AddSystemMessage($"Opportunity expired: {scene.DisplayName}", SystemMessageTypes.Info);
```

Services are properly avoiding presentation string generation.

---

## UI Game Logic Violations

### MAJOR: Game Rules Calculated in UI Component

**Location:** `/home/user/Wayfarer/src/Pages/Components/TravelContent.razor.cs`

**Violation - Lines 143-157:**
```csharp
private int CalculateHungerCost(RouteOption route)
{
    // Base hunger cost from route data
    int hungerCost = route.BaseStaminaCost;

    // Add load penalties
    Player player = GameFacade.GetPlayer();
    int itemCount = player.Inventory.GetAllItems().Count;
    if (itemCount > 3) // Light load threshold
    {
        hungerCost += (itemCount - 3);
    }

    return hungerCost;
}
```

**Violation - Lines 159-172:**
```csharp
private RouteType DetermineRouteType(RouteOption route)
{
    foreach (TerrainCategory terrain in route.TerrainCategories)
    {
        if (terrain == TerrainCategory.Requires_Permission)
            return RouteType.Guarded;
        if (terrain == TerrainCategory.Dark_Passage ||
            terrain == TerrainCategory.Wilderness_Terrain)
            return RouteType.Dangerous;
        // ... more game logic
    }
    return RouteType.Common;
}
```

**Violation - Lines 174-196:**
```csharp
private List<string> ExtractRouteTags(RouteOption route)
{
    List<string> tags = new List<string>();
    foreach (TerrainCategory terrain in route.TerrainCategories)
    {
        switch (terrain)
        {
            case TerrainCategory.Exposed_Weather:
                tags.Add("EXPOSED");
                break;
            // ... more game logic
        }
    }
    return tags;
}
```

**Impact:** UI component is making game logic decisions (calculating costs, determining route types). This logic should be in TravelFacade, with pre-calculated values in ViewModels.

**Correct Pattern:** TravelFacade should return RouteViewModel with:
```csharp
public class RouteViewModel
{
    public int TotalStaminaCost { get; set; } // Pre-calculated by backend
    public RouteType Type { get; set; } // Determined by backend
    public List<string> Tags { get; set; } // Extracted by backend
}
```

---

## ViewModel Analysis

### All ViewModels Examined

**File:** `/home/user/Wayfarer/src/Pages/Components/ExchangeOptionViewModel.cs`
- **Status:** ✅ CLEAN - Domain properties only (ConversationType, Label, IsAvailable)

**File:** `/home/user/Wayfarer/src/ViewModels/MarketViewModel.cs`
- **Status:** ✅ CLEAN - Domain properties (prices, counts, availability)

**File:** `/home/user/Wayfarer/src/ViewModels/TravelViewModel.cs`
- **Status:** ✅ CLEAN - Domain properties, object references (HIGHLANDER compliant)
- Contains FocusClass in nested TravelStatusViewModel (see violations above)

**File:** `/home/user/Wayfarer/src/ViewModels/NarrativeOverlayViewModel.cs`
- **Status:** ✅ CLEAN - Simple domain properties (title, step info, guidance)

**File:** `/home/user/Wayfarer/src/ViewModels/GameFacadeViewModels.cs`
- **Status:** ❌ VIOLATIONS - Contains FocusClass, WeatherIcon, Icon properties
- Otherwise well-structured with object references (HIGHLANDER compliant)

**File:** `/home/user/Wayfarer/src/ViewModels/GameViewModels.cs`
- **Status:** ✅ CLEAN - Domain properties, object references

---

## Correct Patterns Found

### Excellent Example: LocationContent.razor.cs

**Philosophy stated explicitly (Lines 3-7):**
```csharp
/// DUMB DISPLAY COMPONENT - NO BUSINESS LOGIC
/// All filtering/querying/view model building happens in LocationFacade
/// This component ONLY displays pre-built view models and delegates actions to backend
```

**Implementation:**
- Single facade call: `GameFacade.GetLocationFacade().GetLocationContentViewModel()`
- Intent-based execution: Creates intent objects, backend determines effects
- No game logic calculations
- Proper separation of concerns

### Good Pattern: Landing.razor.cs

**Presentation mapping in frontend (Lines 22-31):**
```csharp
protected string GetActionClass(LocationActionViewModel action)
{
    return action.ActionType switch
    {
        "rest" => "rest",
        "secureroom" => "lodging",
        "work" => "work",
        _ => ""
    };
}
```

**Analysis:** Frontend converts domain ActionType to CSS class. This is CORRECT.

### Acceptable Pattern: ObligationIntroModal.razor.cs

**Backend provides data, frontend transforms (Lines 21-56):**
```csharp
protected string GetHeaderColor()
{
    if (string.IsNullOrEmpty(Data.ColorCode))
        return "#7a8b5a";
    return Data.ColorCode;
}

protected string GetHeaderColorLight()
{
    // Lightens color by adding 32 to RGB components
    string hex = Data.ColorCode.TrimStart('#');
    // ... color transformation logic
}
```

**Analysis:** Backend provides ColorCode (data), frontend performs presentation transformations (lightening, darkening). Correct separation.

---

## Recommendations

### Immediate Actions Required

1. **Fix FocusClass in TravelContextViewModel**
   - Remove `FocusClass` property
   - Add domain property: `public int FocusPenalty { get; set; }` (0, 1, or 2)
   - Frontend converts penalty to CSS class

2. **Fix WeatherIcon in TravelContextViewModel**
   - Remove `WeatherIcon` property (already has `CurrentWeather` enum)
   - Frontend maps `WeatherCondition` enum to icon names

3. **Fix Icon in RouteTokenRequirementViewModel**
   - Remove `Icon` property
   - Add domain property: `public ConnectionType TokenType { get; set; }`
   - Frontend maps `ConnectionType` to icon names

4. **Refactor TravelContent.razor.cs**
   - Move CalculateHungerCost to TravelFacade
   - Move DetermineRouteType to TravelFacade
   - Move ExtractRouteTags to TravelFacade
   - TravelFacade returns RouteViewModel with pre-calculated values
   - UI component only maps domain data to presentation

### Architectural Guidance

**Backend Responsibilities (WHAT):**
- Calculate costs based on player state
- Determine route types from terrain
- Validate affordability
- Return domain enums and values

**Frontend Responsibilities (HOW):**
- Map domain enums to CSS classes
- Map domain enums to icon names
- Map domain enums to display text
- Transform colors for gradients/borders

### Testing Strategy

After fixes, verify:
- Changing button styles never touches ViewModels
- Changing cost calculations never touches UI components
- Icon changes only require frontend updates
- CSS class changes only require frontend updates

---

## Audit Log

### Files Examined

**ViewModels:**
- ✅ `/home/user/Wayfarer/src/Pages/Components/ExchangeOptionViewModel.cs`
- ✅ `/home/user/Wayfarer/src/ViewModels/MarketViewModel.cs`
- ✅ `/home/user/Wayfarer/src/ViewModels/TravelViewModel.cs`
- ✅ `/home/user/Wayfarer/src/ViewModels/NarrativeOverlayViewModel.cs`
- ❌ `/home/user/Wayfarer/src/ViewModels/GameFacadeViewModels.cs` (violations found)
- ✅ `/home/user/Wayfarer/src/ViewModels/GameViewModels.cs`

**UI Components:**
- ✅ `/home/user/Wayfarer/src/Pages/Components/Landing.razor.cs` (correct pattern)
- ✅ `/home/user/Wayfarer/src/Pages/Components/LocationContent.razor.cs` (exemplary)
- ❌ `/home/user/Wayfarer/src/Pages/Components/TravelContent.razor.cs` (violations found)
- ✅ `/home/user/Wayfarer/src/Pages/Components/Modals/ObligationIntroModal.razor.cs` (correct pattern)
- ✅ `/home/user/Wayfarer/src/Pages/Components/SpawnGraph/ChoiceNodeWidget.razor` (UI mapping only)
- ✅ `/home/user/Wayfarer/src/Pages/Components/SpawnGraph/EntityNodeWidget.razor` (UI mapping only)

**Backend Services:**
- ✅ `/home/user/Wayfarer/src/Services/GameFacade.cs` (no presentation logic)
- ✅ `/home/user/Wayfarer/src/Services/` (searched for DisplayName patterns - clean)

**Backend Models:**
- ⚠️ `/home/user/Wayfarer/src/GameState/SpawnGraph/SpawnGraphLinkModel.cs` (acceptable - UI-only tool)
- ✅ `/home/user/Wayfarer/src/Services/ObligationIntroResult.cs` (correct pattern)

### Search Patterns Used
- `CssClass` across entire codebase
- `IconName` across entire codebase
- Color hex patterns `#[0-9a-fA-F]{3,6}` in .cs files
- `DisplayName|DisplayText|FormatDisplay` in services
- All *ViewModel.cs files
- UI component code-behind files

### Verdict
**3 Critical Violations + 1 Major Violation = CORRECTIVE ACTION REQUIRED**

The architectural understanding is present, but implementation has drifted from principles in a few key areas. Violations are concentrated and fixable.
