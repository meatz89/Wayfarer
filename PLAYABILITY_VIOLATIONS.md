# PLAYABILITY VIOLATIONS AUDIT

**Date:** 2025-10-31
**Auditor:** Claude Code
**Scope:** Silent defaults and missing validation that hide unplayable game states

---

## EXECUTIVE SUMMARY

**Critical Finding:** The game compiles and runs but is UNPLAYABLE due to silent failures that hide missing JSON content.

**Root Cause:** Code uses nullable types, ?? operators, and missing validation to hide broken player paths instead of failing fast.

**Impact:** Player cannot reach content, gets trapped at locations, sees empty screens. Technical success masks gameplay failure.

---

## CRITICAL VIOLATIONS (Blocks Player Path)

### 1. Missing Starting Location Validation
**File:** `src/Content/PackageLoader.cs:274-277`

**Code:**
```csharp
if (!string.IsNullOrEmpty(conditions.StartingSpotId))
{
    _gameWorld.InitialLocationSpotId = conditions.StartingSpotId;
}
```

**Impact:**
- If `StartingSpotId` is null/empty in JSON, player has NO spawn point
- Game runs but player cannot start - no location, no actions visible
- No validation that StartingSpotId exists in parsed locations

**Player Impact:** GAME UNPLAYABLE - Cannot start game

**Fix Required:**
```csharp
if (string.IsNullOrEmpty(conditions.StartingSpotId))
    throw new InvalidOperationException("StartingSpotId is required - player has no spawn location!");

Location startingLocation = _gameWorld.Locations.FirstOrDefault(l => l.Id == conditions.StartingSpotId);
if (startingLocation == null)
    throw new InvalidOperationException($"StartingSpotId '{conditions.StartingSpotId}' not found in parsed locations - player cannot spawn!");

_gameWorld.InitialLocationSpotId = conditions.StartingSpotId;
```

---

### 2. Missing Location Validation in Situation Query
**File:** `src/Services/GameFacade.cs:1407-1408`

**Code:**
```csharp
Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
if (location == null) return new List<Situation>();
```

**Impact:**
- If locationId doesn't exist in GameWorld, silently returns empty list
- Player at that location sees NO situations (empty screen)
- Hides broken reference - should fail fast during content parsing

**Player Impact:** Player sees empty location with no interactions

**Fix Required:**
```csharp
Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
if (location == null)
    throw new InvalidOperationException($"Location '{locationId}' not found - cannot query situations!");

return _gameWorld.Situations
    .Where(s => s.PlacementLocation?.Id == locationId)
    .ToList();
```

**Alternative (Parser-Time Validation):**
Better to validate during scene/situation parsing that all referenced locationIds exist in GameWorld.Locations

---

### 3. Scene Current Situation Assignment Without Validation
**File:** `src/Content/SceneInstantiator.cs:69`

**Code:**
```csharp
scene.CurrentSituationId = scene.SituationIds.FirstOrDefault();
```

**Impact:**
- If `SituationIds` is empty, CurrentSituationId becomes null
- Scene exists but has no active situation
- Player can see scene but cannot interact (no choices available)

**Player Impact:** Scene appears in UI but is non-functional (no choices, dead-end)

**Fix Required:**
```csharp
if (!scene.SituationIds.Any())
    throw new InvalidOperationException($"Scene '{scene.Id}' has no SituationIds - player cannot interact!");

scene.CurrentSituationId = scene.SituationIds.First();
```

---

## HIGH PRIORITY VIOLATIONS (Hides Content)

### 4. State Parser Uses Silent Defaults
**File:** `src/Content/StateParser.cs:33-37`

**Code:**
```csharp
BlockedActions = dto.BlockedActions ?? new List<string>(),
EnabledActions = dto.EnabledActions ?? new List<string>(),
ClearingBehavior = StateClearConditionsCatalog.GetClearingBehavior(dto.ClearConditions ?? new List<string>())
```

**Impact:**
- If JSON missing BlockedActions/EnabledActions, defaults to empty list
- State exists but has no mechanical effect
- Content author thinks they defined a state, but it does nothing

**Player Impact:** State system appears broken - states don't block/enable actions as expected

**Assessment:** MEDIUM severity - states are optional modifiers, empty is valid but suspicious

**Recommendation:** Add validation warning (not error) if state has NO blocked actions, NO enabled actions, AND NO clear conditions (completely empty state)

---

### 5. Emergency Parser Uses Silent Defaults
**File:** `src/Content/Parsers/EmergencyParser.cs:47, 143-145`

**Code:**
```csharp
TriggerLocationIds = dto.TriggerLocationIds ?? new List<string>(),
GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
SpawnedSituationIds = dto.SpawnedSituationIds ?? new List<string>(),
GrantedItemIds = dto.GrantedItemIds ?? new List<string>(),
```

**Impact:**
- Emergency with empty TriggerLocationIds never triggers (unreachable content)
- Emergency with no knowledge/situations/items granted is functionally empty
- Content exists in JSON but player never sees it

**Player Impact:** Content author created emergency, but player never encounters it

**Fix Required:**
```csharp
if (dto.TriggerLocationIds == null || !dto.TriggerLocationIds.Any())
    throw new InvalidDataException($"Emergency '{dto.Id}' has no TriggerLocationIds - will never trigger!");

TriggerLocationIds = dto.TriggerLocationIds;
GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(); // Rewards can be empty
// ... etc
```

---

### 6. Conversation Tree Parser Uses Silent Defaults
**File:** `src/Content/Parsers/ConversationTreeParser.cs:63, 175-176`

**Code:**
```csharp
RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
SpawnedSituationIds = dto.SpawnedSituationIds ?? new List<string>(),
```

**Impact:**
- Trees/nodes missing RequiredKnowledge appear when they shouldn't (gate broken)
- Trees/nodes missing rewards have no mechanical effect

**Player Impact:** Conversation system feels hollow - no progression rewards

**Assessment:** MEDIUM severity - empty knowledge lists are valid (not all trees gate/reward)

---

## MEDIUM PRIORITY VIOLATIONS (Degrades Experience)

### 7. Location Parser Silent Default for Domain Tags
**File:** `src/Content/LocationParser.cs:98`

**Code:**
```csharp
location.DomainTags = dto.DomainTags ?? new List<string>(); // Optional - defaults to empty list if missing
```

**Impact:**
- Locations missing DomainTags get empty list
- Reduces location personality/categorization
- Not critical for playability but degrades richness

**Player Impact:** Minimal - locations still function

**Assessment:** LOW severity - DomainTags are optional metadata

---

## PATTERNS REQUIRING INVESTIGATION

### Pattern A: UI Component Default Collections
**Files:** Multiple `.razor.cs` files

**Examples:**
```csharp
// PhysicalContent.razor.cs:713
List<CardInstance> handCards = Hand ?? new List<CardInstance>();

// MentalContent.razor.cs:631
List<CardInstance> handCards = Hand ?? new List<CardInstance>();
```

**Assessment:** REQUIRES CONTEXT - If Hand comes from GameWorld session state, null means no active session (valid). If null means broken data, should throw.

**Action:** Verify Hand property source - if from nullable session context, current code is correct. If from required game state, should validate.

---

### Pattern B: Schema Validator Optional Fields
**File:** `src/Content/Validation/Validators/SchemaValidator.cs:113`

**Code:**
```csharp
List<string> knownFields = schema.RequiredFields.Concat(schema.OptionalFields ?? Array.Empty<string>())
```

**Assessment:** ACCEPTABLE - Schema.OptionalFields is genuinely optional, empty array is correct default

---

## RECOMMENDATIONS

### Immediate Actions (Critical Path)

1. **Add StartingSpotId validation in PackageLoader**
   - Fail fast if missing or invalid
   - Validate location exists in GameWorld

2. **Add Scene.SituationIds validation in SceneInstantiator**
   - Scenes must have at least one situation
   - Fail fast if empty

3. **Add Emergency.TriggerLocationIds validation in EmergencyParser**
   - Emergencies must have at least one trigger location
   - Fail fast if empty

### Architecture Principles (Long Term)

1. **Entity Initialization Standard (Update CLAUDE.md)**
   - Collections ALWAYS initialize inline: `public List<T> X { get; set; } = new List<T>();`
   - NO ?? operators in parsers (trust entity initialization)
   - Nullable ONLY for genuinely optional relationships

2. **Parser Validation Standard**
   - ALL critical player-path references validated (locations, NPCs, scenes)
   - Throw InvalidDataException for missing required content
   - Fail fast at parse time, not during gameplay

3. **Playability Testing Standard**
   - BEFORE marking work complete: trace player path from start to content
   - Test in browser: start game → follow path → verify content accessible
   - NO "it compiles" without "player can reach it"

---

## SUMMARY STATISTICS

- **Total violations found:** 7 critical/high, 1 medium, 2 patterns requiring review
- **Critical (blocks player):** 3 violations
- **High priority (hides content):** 4 violations
- **Medium priority (degrades experience):** 1 violation

**Estimated Player Impact:**
- Tutorial currently INACCESSIBLE (no route visibility, no scene spawn validation)
- Empty location screens (missing situation validation)
- Non-functional scenes (missing situation assignment validation)

---

## NEXT STEPS

1. Fix 3 critical violations (StartingSpotId, Scene.SituationIds, Emergency.TriggerLocationIds)
2. Document player path from game start to tutorial
3. Test actual player flow in browser
4. Add playability validation to CI/CD pipeline (fail build if starter content missing)
