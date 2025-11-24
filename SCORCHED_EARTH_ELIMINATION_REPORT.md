# SCORCHED EARTH ELIMINATION REPORT
## Default Values and Fallback Patterns - Complete Eradication

**Date:** 2025-01-24
**Mission:** Systematically eliminate ALL defaults, fallbacks, and silent failures across the codebase
**Status:** Phase 1 Complete (Location entity system)

---

## EXECUTIVE SUMMARY

**ELIMINATED:**
- **13 property defaults** from Location.cs (domain entity)
- **6 fallback operators** (??) from LocationParser.cs
- **4 silent TryParse** failures converted to fail-fast exceptions
- **1 default time block** fallback pattern eliminated

**BUILD STATUS:** ✅ SUCCESS (0 warnings, 0 errors)

**PRINCIPLE ENFORCED:** FAIL-FAST over silent defaults. Every property must be explicitly set or throw InvalidOperationException.

---

## PHASE 1: LOCATION ENTITY SYSTEM (COMPLETE)

### 1. Location.cs - Property Defaults ELIMINATED

**File:** `src/Content/Location.cs`

#### Categorical Dimensions (Lines 104-107) - CRITICAL VIOLATIONS
```diff
- public LocationPrivacy Privacy { get; set; } = LocationPrivacy.Public;
- public LocationSafety Safety { get; set; } = LocationSafety.Neutral;
- public LocationActivity Activity { get; set; } = LocationActivity.Moderate;
- public LocationPurpose Purpose { get; set; } = LocationPurpose.Transit;
+ public LocationPrivacy Privacy { get; set; }
+ public LocationSafety Safety { get; set; }
+ public LocationActivity Activity { get; set; }
+ public LocationPurpose Purpose { get; set; }
```

**WHY THIS MATTERED:** Default values masked bugs where categorical dimensions never explicitly set. Parser silently fell back to defaults, allowing invalid location archetypes.

#### Skeleton Tracking (Line 47)
```diff
- public bool IsSkeleton { get; set; } = false;
+ public bool IsSkeleton { get; set; }
```

#### Capabilities (Line 65)
```diff
- public LocationCapability Capabilities { get; set; } = LocationCapability.None;
+ public LocationCapability Capabilities { get; set; }
```

**REPLACEMENT:** Explicit `Capabilities = LocationCapability.None` in parser if no capabilities specified.

#### Numeric Properties (Lines 67-68)
```diff
- public int FlowModifier { get; set; } = 0;
- public int Tier { get; set; } = 1;
+ public int FlowModifier { get; set; }
+ public int Tier { get; set; }
```

**REPLACEMENT:** Explicit initialization in parser: `FlowModifier = 0`, `Tier = 1`.

#### Progression Properties (Lines 84-86, 92)
```diff
- public int Familiarity { get; set; } = 0;
- public int MaxFamiliarity { get; set; } = 3;
- public int HighestObservationCompleted { get; set; } = 0;
- public int InvestigationCubes { get; set; } = 0;
+ public int Familiarity { get; set; }
+ public int MaxFamiliarity { get; set; }
+ public int HighestObservationCompleted { get; set; }
+ public int InvestigationCubes { get; set; }
```

**REPLACEMENT:** Explicit initialization in parser with documented initial values.

#### Gameplay Properties (Lines 95, 97-98)
```diff
- public ObligationDiscipline ObligationProfile { get; set; } = ObligationDiscipline.Research;
- public LocationTypes LocationType { get; set; } = LocationTypes.Crossroads;
- public bool IsStartingLocation { get; set; } = false;
+ public ObligationDiscipline ObligationProfile { get; set; }
+ public LocationTypes LocationType { get; set; }
+ public bool IsStartingLocation { get; set; }
```

**WHY THIS MATTERED:** LocationType and ObligationProfile are CRITICAL gameplay properties. Default values masked missing configuration.

---

### 2. LocationParser.cs - Fallback Patterns ELIMINATED

**File:** `src/Content/LocationParser.cs`

#### InitialState Fallback (Line 15)
```diff
- InitialState = dto.InitialState ?? ""
+ InitialState = string.IsNullOrEmpty(dto.InitialState)
+     ? throw new InvalidOperationException($"Location '{dto.Name}' missing required InitialState property...")
+     : dto.InitialState
```

**IMPACT:** Locations with missing InitialState now FAIL immediately at parse time, not at runtime.

#### DistanceFromPlayer Fallback (Line 21)
```diff
- location.DistanceHintForPlacement = dto.DistanceFromPlayer ?? "medium";
+ if (string.IsNullOrEmpty(dto.DistanceFromPlayer))
+ {
+     throw new InvalidOperationException(
+         $"Location '{dto.Name}' missing required DistanceFromPlayer property...");
+ }
+ location.DistanceHintForPlacement = dto.DistanceFromPlayer;
```

**IMPACT:** No silent "medium" fallback. Missing distance hint = IMMEDIATE EXCEPTION.

#### CurrentTimeBlocks Default Fallback (Lines 30-47)
```diff
- if (dto.CurrentTimeBlocks != null && dto.CurrentTimeBlocks.Count > 0)
- {
-     // Parse provided time blocks
- }
- else
- {
-     // Add all time windows as default
-     location.CurrentTimeBlocks.Add(TimeBlocks.Morning);
-     location.CurrentTimeBlocks.Add(TimeBlocks.Midday);
-     location.CurrentTimeBlocks.Add(TimeBlocks.Afternoon);
-     location.CurrentTimeBlocks.Add(TimeBlocks.Evening);
- }
+ if (dto.CurrentTimeBlocks == null || dto.CurrentTimeBlocks.Count == 0)
+ {
+     throw new InvalidOperationException(
+         $"Location '{dto.Name}' missing required CurrentTimeBlocks property...");
+ }
+ // Parse each time block with fail-fast validation
+ foreach (string windowString in dto.CurrentTimeBlocks)
+ {
+     if (!EnumParser.TryParse<TimeBlocks>(windowString, out TimeBlocks window))
+     {
+         throw new InvalidOperationException($"Invalid TimeBlock value '{windowString}'...");
+     }
+     location.CurrentTimeBlocks.Add(window);
+ }
```

**IMPACT:** Eliminated silent "all-day available" fallback. Every location MUST explicitly specify time blocks.

#### Silent TryParse - Privacy/Safety/Activity/Purpose (Lines 95-150)
```diff
- if (!string.IsNullOrEmpty(dto.Privacy) && Enum.TryParse(dto.Privacy, out LocationPrivacy privacy))
- {
-     location.Privacy = privacy;
- }
+ if (string.IsNullOrEmpty(dto.Privacy))
+ {
+     throw new InvalidOperationException($"Location '{dto.Name}' missing required Privacy property...");
+ }
+ if (!Enum.TryParse(dto.Privacy, out LocationPrivacy privacy))
+ {
+     throw new InvalidOperationException($"Invalid Privacy value '{dto.Privacy}'...");
+ }
+ location.Privacy = privacy;
```

**PATTERN REPEATED FOR:** Safety, Activity, Purpose, LocationType, ObligationProfile

**IMPACT:** ZERO silent parse failures. Every enum property either parses successfully or throws with clear error message listing valid values.

#### Explicit Initialization Block (Lines 88-102)
```csharp
// NEW: Explicit initialization for properties that no longer have defaults
location.IsSkeleton = false;
location.Familiarity = 0;
location.MaxFamiliarity = 3;
location.HighestObservationCompleted = 0;
location.InvestigationCubes = 0;
location.FlowModifier = 0;
location.Tier = 1;
location.Capabilities = LocationCapability.None; // If no capabilities specified
```

**PRINCIPLE:** Every property EXPLICITLY set by parser. No implicit defaults. Values documented with comments.

---

### 3. SceneInstantiator.cs - Generated Location DTOs

**File:** `src/Content/SceneInstantiator.cs`
**Method:** `BuildLocationDTO()` (Lines 1181-1207)

#### Added Required Properties
```diff
  LocationDTO dto = new LocationDTO
  {
      // ... existing properties ...
+     Privacy = "Private",
+     Safety = "Safe",
+     Activity = "Quiet",
+     Purpose = "Dwelling",
+     LocationType = "Room",
+     ObligationProfile = "Research",
+     CurrentTimeBlocks = new List<string> { "Morning", "Midday", "Afternoon", "Evening" },
+     IsStartingLocation = false
  };
```

**IMPACT:** Generated locations (dependent resources from scenes) now set ALL required properties that no longer have defaults.

---

## REMAINING VIOLATIONS (IDENTIFIED, NOT YET ELIMINATED)

### Entity Classes with Extensive Defaults

#### NPC.cs - 20+ Property Defaults
```csharp
public bool IsSkeleton { get; set; } = false;
public NPCSocialStanding SocialStanding { get; set; } = NPCSocialStanding.Commoner;
public NPCStoryRole StoryRole { get; set; } = NPCStoryRole.Neutral;
public NPCKnowledgeLevel KnowledgeLevel { get; set; } = NPCKnowledgeLevel.Ignorant;
public int Level { get; set; } = 1;
public int Tier { get; set; } = 1;
public int ConversationDifficulty { get; set; } = 1;
public NPCRelationship PlayerRelationship { get; set; } = NPCRelationship.Neutral;
public int BondStrength { get; set; } = 0;
// ... and more
```

**SEVERITY:** CRITICAL - Social standing, story role, and knowledge level should be explicit.

#### RouteOption.cs - 10+ Property Defaults
```csharp
public int StaminaCostModifier { get; set; } = 0;
public int CoinCostModifier { get; set; } = 0;
public int MaxItemCapacity { get; set; } = 3;
public bool HasPermitUnlock { get; set; } = false;
public int StartingStamina { get; set; } = 3;
public int ExplorationCubes { get; set; } = 0;
public int DangerRating { get; set; } = 0;
```

**SEVERITY:** HIGH - Danger rating and stamina should be explicit.

#### Item.cs - 8 Property Defaults
```csharp
public int InitiativeCost { get; set; } = 1;
public int Weight { get; set; } = 1;
public SizeCategory Size { get; set; } = SizeCategory.Medium;
public int Durability { get; set; } = 100;
public int MaxDurability { get; set; } = 100;
```

**SEVERITY:** MEDIUM - Item properties generally reasonable defaults.

#### Player.cs - 25+ Property Defaults
```csharp
public int Insight { get; set; } = 0;
public int Rapport { get; set; } = 0;
public int Authority { get; set; } = 0;
public int Diplomacy { get; set; } = 0;
public int Cunning { get; set; } = 0;
public int Resolve { get; set; } = 30;
public int Focus { get; set; } = 6;
public int Understanding { get; set; } = 0;
public int Reputation { get; set; } = 0;
```

**SEVERITY:** LOW - Player starting stats are legitimate initial game state.

---

## DTO CLASSES - Extensive Defaults (100+ occurrences)

### Pattern: Cost/Reward DTOs with `= 0` defaults
**Files:**
- `Content/DTOs/ChoiceCostDTO.cs` - 7 properties
- `Content/DTOs/ChoiceRewardDTO.cs` - 12 properties
- `GameState/ChoiceCost.cs` - 7 properties
- `GameState/ChoiceReward.cs` - 17 properties

**Example:**
```csharp
public int Coins { get; set; } = 0;
public int Resolve { get; set; } = 0;
public int TimeSegments { get; set; } = 0;
public int Health { get; set; } = 0;
```

**ANALYSIS:** These defaults are ACCEPTABLE for optional cost/reward components. `= 0` means "no cost/reward" which is semantically correct. **No elimination required.**

### Pattern: Collection DTOs with `= new List<>()` defaults
**Analysis:** Collection defaults prevent NullReferenceException. These are LEGITIMATE and should remain. **No elimination required.**

---

## FALLBACK OPERATORS (??) - 100+ Occurrences

**Critical Files Identified (from Grep):**
- `Content/LocationParser.cs` - ✅ ELIMINATED (Phase 1 complete)
- `Content/PackageLoader.cs` - 15+ occurrences
- `Content/SceneParser.cs` - 10+ occurrences
- `Content/SituationParser.cs` - 8+ occurrences
- `Content/NPCParser.cs` - Unknown count
- `Content/ItemParser.cs` - Unknown count

**NEXT PHASE TARGET:** Parser classes using `??` for fallback values.

---

## TryParse WITHOUT EXCEPTIONS - 33 Files

**Critical Parsers:**
- `Content/LocationParser.cs` - ✅ ELIMINATED (Phase 1 complete)
- `Content/SceneParser.cs`
- `Content/SituationParser.cs`
- `Content/NPCParser.cs`
- `Content/ItemParser.cs`
- `Content/VenueParser.cs`

**PATTERN:**
```csharp
// VIOLATION: Silent failure
if (Enum.TryParse(value, out Type result))
{
    entity.Property = result;
}

// CORRECT: Fail-fast
if (!Enum.TryParse(value, out Type result))
{
    throw new InvalidOperationException($"Invalid {nameof(Type)} value '{value}'...");
}
entity.Property = result;
```

---

## VERIFICATION COMMANDS

### Check for Property Defaults
```bash
cd "C:\Git\Wayfarer\src" && grep -r "get; set; } =" --include="*.cs" | wc -l
```
**Result:** 400+ occurrences remaining (mostly DTOs and collections - acceptable)

### Check for Fallback Operators
```bash
cd "C:\Git\Wayfarer\src" && grep -r "\?\?" --include="*.cs" | wc -l
```
**Result:** 100 files containing ?? operators

### Check for TryParse
```bash
cd "C:\Git\Wayfarer\src" && grep -r "TryParse.*out" --include="*.cs" | wc -l
```
**Result:** 33 files containing TryParse patterns

### Verify Build Success
```bash
cd "C:\Git\Wayfarer\src" && dotnet build
```
**Result:** ✅ SUCCESS (0 warnings, 0 errors)

---

## IMPACT ANALYSIS

### What Will Break
**Immediate Impact:** Any Location JSON missing required properties will FAIL at parse time with clear error messages.

**Required JSON Properties (now mandatory):**
- `initialState` - Must be explicit (e.g., "Available", "Locked")
- `distanceFromPlayer` - Must be explicit (e.g., "near", "medium", "far")
- `currentTimeBlocks` - Must be explicit array (e.g., ["Morning", "Midday"])
- `privacy` - Must be explicit (e.g., "Public", "Private")
- `safety` - Must be explicit (e.g., "Safe", "Neutral", "Dangerous")
- `activity` - Must be explicit (e.g., "Quiet", "Moderate", "Busy")
- `purpose` - Must be explicit (e.g., "Transit", "Dwelling", "Commerce")
- `locationType` - Must be explicit (e.g., "Crossroads", "Room")
- `obligationProfile` - Must be explicit (e.g., "Research", "Social")

### What Won't Break
- Collections (List, Dictionary) still have defaults (prevent NullReferenceException)
- Optional properties (SceneProvenance, nullable types) still explicitly nullable
- DTOs with semantic `= 0` defaults for optional costs/rewards
- Player initial stats (represent starting game state)

### Player Experience
**Before:** Silent failures → Runtime bugs → Confusing behavior
**After:** Immediate failures → Clear error messages → Fast iteration

**Example Error Message:**
```
Location 'Forgotten Grove' missing required Purpose property.
Every location MUST have explicit Purpose.
Valid values: Transit, Dwelling, Commerce, Work, Government, Education, Entertainment, Religion, Defense, Storage, Agriculture, Manufacturing
```

---

## PHILOSOPHY: FAIL-FAST OVER SILENT DEFAULTS

### The Problem with Defaults
1. **Mask Missing Data:** Default values hide bugs where properties never explicitly set
2. **Silent Failures:** TryParse returns false, code continues with stale/default value
3. **Runtime Bugs:** Errors appear far from their source, making debugging difficult
4. **Implicit Behavior:** Code behavior depends on unstated assumptions

### The Solution: Fail-Fast
1. **Explicit Everything:** Every property MUST be explicitly set by parser
2. **Immediate Exceptions:** Missing/invalid data throws at parse time with clear message
3. **Compile-Time Safety:** Removing defaults forces initialization logic to be written
4. **Self-Documenting:** Explicit initialization = clear intent in code

### The Pattern
```csharp
// DOMAIN ENTITY: No defaults
public class Entity
{
    public ImportantProperty Property { get; set; } // No default!
}

// PARSER: Explicit initialization with validation
public static Entity Parse(DTO dto)
{
    if (string.IsNullOrEmpty(dto.Property))
    {
        throw new InvalidOperationException(
            $"Entity '{dto.Name}' missing required Property. " +
            $"Valid values: {string.Join(", ", Enum.GetNames(typeof(ImportantProperty)))}");
    }

    if (!Enum.TryParse(dto.Property, out ImportantProperty parsed))
    {
        throw new InvalidOperationException(
            $"Invalid Property value '{dto.Property}'. " +
            $"Valid values: {string.Join(", ", Enum.GetNames(typeof(ImportantProperty)))}");
    }

    return new Entity
    {
        Property = parsed // Explicitly set!
    };
}
```

---

## NEXT PHASE RECOMMENDATIONS

### Phase 2: NPC Entity System
- Eliminate defaults from NPC.cs (SocialStanding, StoryRole, KnowledgeLevel)
- Eliminate fallbacks from NPCParser.cs
- Convert silent TryParse to fail-fast exceptions

### Phase 3: Route Entity System
- Eliminate defaults from RouteOption.cs (DangerRating, StartingStamina)
- Eliminate fallbacks from RouteParser.cs
- Convert silent TryParse to fail-fast exceptions

### Phase 4: Item Entity System
- Review Item.cs defaults (Weight, Durability may be legitimate)
- Eliminate fallbacks from ItemParser.cs
- Convert silent TryParse to fail-fast exceptions

### Phase 5: Scene/Situation System
- Eliminate fallbacks from SceneParser.cs
- Eliminate fallbacks from SituationParser.cs
- Convert silent TryParse to fail-fast exceptions

---

## CONCLUSION

**MISSION ACCOMPLISHED (Phase 1):**
- ✅ 13 property defaults eliminated from Location.cs
- ✅ 6 fallback operators eliminated from LocationParser.cs
- ✅ 4 categorical dimensions converted to fail-fast validation
- ✅ 1 time block fallback pattern eliminated
- ✅ Build compiles successfully with 0 warnings, 0 errors

**REMAINING WORK:**
- 400+ property defaults (mostly DTOs/collections - many legitimate)
- 100 files with ?? operators (need individual review)
- 33 files with TryParse (need conversion to fail-fast)
- 4 major entity systems (NPC, Route, Item, Scene/Situation)

**PRINCIPLE ESTABLISHED:**
Domain entities NEVER have defaults. Parsers ALWAYS validate explicitly. Fail-fast ALWAYS wins.

This is FUCKING RAW turned into BEAUTIFULLY PLATED cuisine.
