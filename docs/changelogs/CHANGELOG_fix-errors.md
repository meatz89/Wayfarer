# Changelog: fix-errors Branch

Summary of architectural changes and fixes in this branch.

---

## ceb6c088 - ARCH: Enforce GameWorld DI + Player access pattern

**Purpose:** Establish strict architectural rules for accessing GameWorld and Player state.

**Changes:**
- **GameWorld**: Now ONLY injected via constructor, NEVER passed as method parameter
- **Player**: NEVER injected OR passed as parameter - always accessed via `_gameWorld.GetPlayer()`

**Services Refactored:**
| Service | Change |
|---------|--------|
| SituationChoiceExecutor | Removed GameWorld+Player method params |
| LocationActionExecutor | Removed Player method params |
| TransportCompatibilityValidator | Removed Player method params |
| LocationPlacementService | Removed Player method params |
| LocationPlayabilityValidator | Added GameWorld constructor injection |
| PlayerExertionCalculator | Uses `_gameWorld.GetPlayer()` |
| VenueGeneratorService | Uses `_gameWorld.GetPlayer()`, now fully deterministic |
| MentalEffectResolver | Uses `_gameWorld.GetPlayer()` |
| PhysicalEffectResolver | Uses `_gameWorld.GetPlayer()` |

**Additional Fixes:**
- VenueGeneratorService: Replaced `HashSet` with `List<T>` per TYPE restrictions
- VenueGeneratorService: Removed `Random.Shared` per DETERMINISM principle

**Tests Updated:** ServiceStatelessnessTests now enforces these patterns via reflection.

---

## 706b4d9f - FIX: Add SocialSession to state parameter types

**Purpose:** Fix false positive in architecture test.

**Changes:**
- SocialResourceCalculator methods receive SocialSession (game state) via method parameters
- Updated ServiceStatelessnessTests to recognize SocialSession as a state type alongside Player, GameWorld, NPC, Location

---

## eecd8c09 - FIX: Resolve all test failures + refactor SituationArchetypeCatalog

**Purpose:** Fix 554 failing tests and reduce file size violation.

**Test Fixes:**
- Fix HighlanderSceneGenerationTests constructor with parameterless validators
- Fix RhythmPatternComplianceTests to scan all `*Archetypes.cs` files
- Add missing mainStorySequence to tutorial scene templates
- Add State field to A-story tutorial scenes
- Fix SceneInstantiator to include SceneArchetype in dynamic DTOs
- Add Recovery choice generation for peaceful archetypes with None stats

**File Size Compliance (COMPOSITION over INHERITANCE):**
- Extract 24 archetype definitions to SituationArchetypeDefinitions.cs (591 lines)
- SituationArchetypeCatalog.cs reduced from 1481 to 699 lines
- HIGHLANDER preserved: single entry point via `GetArchetype()`

---

## 543213b2 - FIX: Update test project to align with main codebase

**Purpose:** Synchronize test project with breaking changes in main codebase.

**Changes:**
- Remove Tier property references (Venue, SceneTemplate, Location, NPC)
- Update NPC constructor usage to object initializer
- Replace GenerateChoiceTemplates with GenerateChoiceTemplatesWithContext
- Fix GenerationContext property names (Rhythm, NpcDemeanor, Environment)
- Replace PlacementSelectionStrategy.Random with First (DDR-007 compliance)
- Fix Location constructor to require name parameter
- Remove GameWorld.InitializeHexGrid call (auto-initialized)

**Result:** 0 errors, 523 tests passing

---

## 71afaf67 - FIX: Remove duplicate InitializeTravelDiscoverySystem

**Purpose:** Fix HIGHLANDER violation (duplicate method).

**Changes:**
- Delegated to `_travelLoader.InitializeTravelDiscoverySystem()`
- Deleted local duplicate method from PackageLoader

---

## b148a9d5 - REFACTOR: DOMAIN COLLECTION PRINCIPLE + PackageLoader extraction

**Purpose:** Eliminate Dictionary patterns and reduce PackageLoader file size.

**Phase 1 - DOMAIN COLLECTION PRINCIPLE:**
- SocialCard: Explicit ConnectionType properties (Trust/Diplomacy/Status/Shadow TokenRequirement)
- GameRules: Explicit ConnectionState properties (Disconnected/Guarded/Neutral/Receptive/Trusting DrawCount)
- Deleted TokenRequirementEntry class from CollectionEntries.cs

**Phase 2 - PackageLoader Refactoring (1736 to 796 lines):**
| New File | Lines | Responsibility |
|----------|-------|----------------|
| TravelSystemLoader.cs | ~190 | Route and path loading |
| SceneSituationSystemLoader.cs | ~150 | Scene/situation templates |
| CardSystemLoader.cs | ~260 | Card parsing and loading |
| SpatialHierarchyLoader.cs | ~180 | Venue/location hierarchy |
| SpatialPlacementLoader.cs | ~140 | Hex placement and validation |

---

## afc4d626 - update

**Purpose:** Minor incremental changes (no detailed description).

---

## Summary

This branch enforces architectural principles:

1. **GameWorld/Player Access Pattern** - Consistent state access via DI
2. **DOMAIN COLLECTION PRINCIPLE** - Explicit properties replace Dictionary patterns
3. **File Size Limit (1000 lines)** - Large files split via COMPOSITION
4. **HIGHLANDER** - No duplicate implementations
5. **DETERMINISM** - Random only allowed in Pile.cs
6. **TYPE Restrictions** - List<T> only, no HashSet/Dictionary
