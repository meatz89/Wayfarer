# Codebase Violations - Scene Generation Principles

## Status: ALL VIOLATIONS FIXED

All violations have been resolved. This file now serves as documentation of the implemented design.

---

## Documented Principles (Now Implemented)

### History-Driven Generation (gdd/01 §1.8)
- Selection is based on PAST scene history, not current player state
- Current Resolve, stats, resources NEVER influence selection
- No rotation/sequence-based selection

### HIGHLANDER for Scene Generation (arc42 §8.28)
- Same selection logic for authored and procedural content
- No TargetCategory overrides
- Authored content provides categorical inputs that flow through same logic

### Orthogonal Systems (arc42 §8.26)
- Category = WHAT (Investigation, Social, Confrontation, Crisis)
- ArchetypeIntensity = HOW demanding (Recovery, Standard, Demanding)
- These are INDEPENDENT - selection produces both, not one from the other

---

## Fixes Applied

| Component | Change |
|-----------|--------|
| **SceneSpawnReward** | Removed TargetCategory, ExcludedCategories. Added LocationPrivacyContext, LocationActivityContext, RhythmPhaseContext, TierContext |
| **SceneSpawnRewardDTO** | Mirror of entity changes |
| **SceneTemplateParser** | Added ParseLocationPrivacy, ParseLocationActivity, ParseRhythmPhase |
| **RewardApplicationService** | Removed Sequence, MaxSafeIntensity, TargetCategory, PlayerReadinessService dependency. Added ComputeRhythmPhase |
| **SceneSelectionInputs** | Removed overrides. Added RhythmPhase enum, full categorical inputs |
| **ProceduralAStoryService** | SelectArchetypeCategory now uses rhythm phase + location context |
| **ArchetypeCategorySelector** | Removed SelectCategory (HIGHLANDER violation). Kept only MapArchetypeToCategory, MapArchetypeToIntensity utilities |

---

## Validation Checklist - ALL COMPLETE

- [x] No TargetCategory override anywhere in codebase
- [x] No ExcludedCategories special case
- [x] No MaxSafeIntensity in selection (only in runtime display)
- [x] No Sequence-based rotation
- [x] Authored and procedural use identical SelectArchetypeCategory path
- [x] All selection based on history + location + rhythm phase
- [x] RhythmPhase computed from intensity history
