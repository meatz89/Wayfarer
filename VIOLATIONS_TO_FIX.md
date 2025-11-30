# Codebase Violations - Scene Generation Principles

This document tracks violations of the recently documented principles for scene generation.

## Documented Principles (What SHOULD Be)

### History-Driven Generation (gdd/01 §1.8)
- Selection is based on PAST scene history, not current player state
- Current Resolve, stats, resources NEVER influence selection
- No rotation/sequence-based selection

### HIGHLANDER for Scene Generation (arc42 §8.28)
- Same selection logic for authored and procedural content
- No TargetCategory overrides that bypass selection logic
- Authored content provides hardcoded categorical properties that flow through same logic

### Orthogonal Systems (arc42 §8.26)
- Category = WHAT (Investigation, Social, Confrontation, Crisis)
- ArchetypeIntensity = HOW demanding (Recovery, Standard, Demanding)
- These are INDEPENDENT - selection produces both, not one from the other

---

## Active Violations

### 1. SceneSpawnReward.TargetCategory (OVERRIDE BYPASS)

**Location:** `src/GameState/SceneSpawnReward.cs:38`

**Problem:** TargetCategory is an override that bypasses selection logic. When set, it short-circuits the entire selection process.

**Current:**
```csharp
public string TargetCategory { get; set; }
```

**Should Be:** Removed. Authored content should provide categorical inputs (location context, rhythm phase, etc.) that the selection logic processes identically to procedural.

---

### 2. SceneSpawnReward.ExcludedCategories (SPECIAL CASE)

**Location:** `src/GameState/SceneSpawnReward.cs:55`

**Problem:** ExcludedCategories is a special-case parameter. Anti-repetition should be handled by RecentCategories in SceneSelectionInputs.

**Current:**
```csharp
public List<string> ExcludedCategories { get; set; } = new List<string>();
```

**Should Be:** Removed. Use RecentCategories from history instead.

---

### 3. SceneSpawnRewardDTO.TargetCategory and ExcludedCategories

**Location:** `src/Content/DTOs/SceneSpawnRewardDTO.cs:24,39`

**Problem:** DTO mirrors the entity violations. Must be updated to match new structure.

**Should Be:** Replace with categorical input properties:
- LocationSafety, LocationPurpose, LocationPrivacy, LocationActivity
- RhythmPhase
- IntensityHistory fields (or pre-computed)
- Tier

---

### 4. RewardApplicationService.BuildSelectionInputs uses MaxSafeIntensity

**Location:** `src/Subsystems/Consequence/RewardApplicationService.cs:309`

**Problem:** MaxSafeIntensity is derived from current player Resolve. Current player state should NEVER influence selection.

**Current:**
```csharp
MaxSafeIntensity = _playerReadinessService.GetMaxSafeIntensity(player)
```

**Should Be:** Removed. Field deleted from SceneSelectionInputs.

---

### 5. RewardApplicationService.BuildSelectionInputs sets TargetCategory

**Location:** `src/Subsystems/Consequence/RewardApplicationService.cs:316,324`

**Problem:** Still using override pattern.

**Current:**
```csharp
inputs.TargetCategory = sceneSpawn.TargetCategory;
// or
inputs.TargetCategory = null; // Will use rotation logic
```

**Should Be:** Build categorical inputs from either:
- Authored: Hardcoded location context + rhythm phase in SceneSpawnReward
- Procedural: Derived from GameWorld history

---

### 6. SceneTemplateParser uses TargetCategory and ExcludedCategories

**Location:** `src/Content/Parsers/SceneTemplateParser.cs:794,797`

**Problem:** Parser maps DTO override fields to entity.

**Should Be:** Map new categorical input fields instead.

---

### 7. ProceduralAStoryService comments still mention TargetCategory

**Location:** `src/Subsystems/ProceduralContent/ProceduralAStoryService.cs:55-56,69`

**Problem:** Stale comments from old architecture.

**Should Be:** Update to reflect history-driven selection.

---

### 8. Sequence field was removed but may still be referenced

**Status:** Already removed from SceneSelectionInputs. Check for compilation errors.

---

## PlayerReadinessService - Special Case

**Location:** `src/Services/PlayerReadinessService.cs`

**Status:** GetMaxSafeIntensity() is still valid for RUNTIME filtering of which situations to display. This is different from GENERATION selection.

**Clarification:**
- GENERATION: Never uses current player state (history-driven)
- RUNTIME DISPLAY: Can filter based on player Resolve (prevents showing overwhelming situations to exhausted players)

This is correct per arc42 §8.26 - intensity filtering at runtime is separate from category selection at generation.

---

## Fix Order

1. **SceneSpawnReward** - Add categorical input fields, mark old fields deprecated
2. **SceneSpawnRewardDTO** - Mirror entity changes
3. **SceneTemplateParser** - Parse new fields
4. **RewardApplicationService.BuildSelectionInputs** - Use new fields, compute RhythmPhase
5. **ProceduralAStoryService** - Update comments
6. **Remove old fields** - Final cleanup after all usages migrated

---

## Tutorial Content Migration

Tutorial JSON files will need to be updated. Instead of:

```json
{
  "TargetCategory": "Social"
}
```

Use:

```json
{
  "LocationSafety": "Safe",
  "LocationPurpose": "Civic",
  "RhythmPhase": "Accumulation",
  "Tier": 0
}
```

These inputs flow through selection logic and naturally produce Social category.

---

## Validation Checklist

After fixes:

- [ ] No TargetCategory override anywhere in codebase
- [ ] No ExcludedCategories special case
- [ ] No MaxSafeIntensity in selection (only in runtime display)
- [ ] No Sequence-based rotation
- [ ] Authored and procedural use identical SelectArchetypeCategory path
- [ ] All selection based on history + location + rhythm phase
