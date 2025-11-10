# Scene Instantiation Architecture

## HIGHLANDER Principle: One Instantiation Path

**ALL GameWorld entities MUST flow through JSON → PackageLoader → Parser → Entity**

This applies to:
- ✅ Locations: LocationDTO → JSON → PackageLoader → LocationParser → Location
- ✅ Items: ItemDTO → JSON → PackageLoader → ItemParser → Item
- ✅ NPCs: NPCDTO → JSON → PackageLoader → NPCParser → NPC
- ✅ SceneTemplates: SceneTemplateDTO → JSON → PackageLoader → SceneTemplateParser → SceneTemplate
- ✅ **Scenes: SceneDTO → JSON → PackageLoader → SceneParser → Scene** ← THIS DOCUMENT

## Two-Phase Scene System

### Phase 1: Template Definition (Static Content)

**SceneTemplates** = Immutable blueprints for reusable scene patterns

**JSON Structure:**
Templates contain: unique identifier, scene archetype identifier referencing catalogue, tier for difficulty/progression, placement filter defining categorical criteria (personality types, bond thresholds, location properties, selection strategy).

**Loading Flow:**
Static package JSON loaded by PackageLoader. SceneTemplateParser parses template invoking catalogue translation. Catalogue generates SituationTemplates from archetype definition. SceneTemplate entity created. Added to GameWorld.SceneTemplates collection.

**Key Properties:**
**Categorical Filters:** PersonalityTypes enum values, LocationProperties enum flags, BondThreshold numeric values determine placement eligibility.
**SceneArchetypeId:** References catalogue method returning complete SituationTemplates collection.
**Reusable:** Single template spawns multiple scene instances across gameplay.
**Parse-Time Catalogue:** SceneArchetypeCatalogue generates complete SituationTemplates during parsing, not runtime.

### Phase 2: Instance Spawning (Dynamic Content)

**Principle: JSON as Universal Serialization Boundary**

Even dynamically-generated scenes serialize to JSON before becoming entities. This isn't just consistency for consistency's sake - it serves critical functions:

**Debuggability:** Inspect generated JSON files to verify procedural systems produce correct structures before parsing. Catches malformed generation early.

**Save/Load Parity:** Generated scenes serialize identically to authored scenes. No special cases for "was this authored or generated?" during save/load.

**Validation Uniformity:** Generated content flows through same parser validation as authored content. Can't accidentally bypass validation by constructing entities directly.

**Why This Matters:**
Direct entity construction (bypassing JSON) creates two code paths: one for authored content, one for generated. Bugs in generated path don't surface during authored content testing. JSON serialization forces single path - if parsing works for authored content, it works for generated content.

## Categorical-to-Concrete Resolution Principle

**Why Categorical Filters Instead of Concrete IDs**

Templates describe WHAT KIND of entity ("Trusted NPC with bond ≥15"), not WHICH specific entity ("Elena"). This separation serves fundamental architectural goals:

**Template Reusability:** Same template spawns with different concrete entities based on current game state. "Plea for help" template spawns with Elena on Day 5 (when her bond reaches 15), then spawns again with Marcus on Day 10 (when his bond reaches 15). One template, infinite instances.

**State-Dependent Selection:** Resolution happens at spawn time when game state is known. Templates authored at design time can't know which NPCs exist, what bonds player has formed, or where player is located. Categorical filters defer these decisions until runtime.

**Graceful Degradation:** If no entities match categorical filter, spawn fails cleanly (no scene instance). Better than hardcoding "elena" and crashing when Elena doesn't exist in procedurally-generated worlds.

**Why This Matters:**
Hardcoding concrete entity IDs in templates couples content to specific world states. "Find Elena and ask for help" only works if Elena exists. Categorical version "Find Trusted NPC with bond ≥15 and ask for help" works in any world state with appropriate NPCs. This enables procedural worlds where entity sets differ per playthrough.

## Ownership Through Embedding Principle

**Why Situations Embed in Scenes Instead of Separate Collection**

Situations stored directly in Scene.Situations list (not GameWorld.Situations with ID references) eliminates entire categories of bugs:

**No Orphans:** Scene deletion automatically cascades to embedded Situations. No separate garbage collection pass to find orphaned Situations. No "Situation references deleted Scene" errors.

**No Shared Ownership Confusion:** Situation belongs to exactly one Scene. Can't accidentally reference same Situation from multiple Scenes. No "which Scene owns this Situation?" questions.

**No Reference Consistency Errors:** Scene serialization includes all Situations. Can't have Scene referencing Situation IDs that don't exist in save file. Can't have Situation existing without owning Scene.

**Why This Matters:**
ID-based references create temporal coupling - must load entities in specific order to resolve references. Embedding eliminates ordering constraints. Situations come into existence when Scene does, cease to exist when Scene does. Single atomic operation, impossible to get into inconsistent state.

## Marker Resolution Enables Template-Generated Resources

**The Problem: Templates Can't Reference Resources That Don't Exist Yet**

Scene template defines "secure lodging" scenario requiring private room location. Template written at design time can't know concrete ID of room (doesn't exist until scene spawns). Hardcoding placeholder ID ("private_room_001") fails when same template spawns twice - second instance would reference first instance's room.

**The Solution: Logical Markers Resolved at Spawn Time**

Templates reference logical markers ("generated:private_room") instead of concrete IDs. When scene spawns:
- System generates actual Location with real GUID
- Builds resolution map: "generated:private_room" → "scene_abc_private_room_guid"
- Replaces all marker references with concrete IDs

Each spawn creates independent resource set. Template spawns 10 times, creates 10 different private rooms with 10 different GUIDs.

**Why This Matters:**
Without marker resolution, templates can only reference pre-existing global resources. "Secure lodging" template would need global "private_room" location shared across all instances. First player completes scene and locks room, second player can't access it. Markers enable resource isolation - each scene instance gets its own resources, no cross-contamination between instances.

## Runtime Generation Uses Same Path as Static Content

**The Unifying Principle**

Dynamically-generated scenes (spawned during gameplay) flow through identical JSON→Parser→Entity path as hand-authored scenes (loaded at startup). No special constructors for generated content, no separate validation logic, no "runtime entity creation" bypass.

**Why Single Path Matters:**

Authored content gets thorough testing during development. Generated content might spawn combinations never tested. If generated content uses different code path, bugs in that path remain hidden until runtime. Single path means: if validation works for authored content, it works for generated content.

Save files serialize scenes identically regardless of origin. Player saves game with mix of authored and generated scenes. Load code doesn't need to distinguish which is which - all scenes deserialize through same parser.

**The Trade-Off:**
JSON serialization adds overhead versus direct entity construction. But overhead is negligible compared to debugging cost of maintaining two parallel systems. Architectural uniformity chosen over micro-optimization.

## Key Architectural Decisions

### 1. Why JSON for Scene Instances?

**Consistency**: ALL entities use JSON → PackageLoader → Parser
- Locations: JSON → PackageLoader ✓
- Items: JSON → PackageLoader ✓
- Scenes: JSON → PackageLoader ✓

**Benefits:**
- Single code path (no special cases)
- Parsers handle validation uniformly
- Idempotency via _loadedPackageIds
- Debuggable (inspect JSON files)
- Save/load ready (serialize GameWorld.Scenes → JSON)

### 2. When Does Categorical Resolution Happen?

**At spawn time** (SceneInstantiator.ResolvePlacement)

**Why not at parse time?**
- SceneTemplates are parsed ONCE at game load
- Game state changes constantly (NPCs move, bonds increase, player location changes)
- Categorical queries must evaluate CURRENT game state
- Template → Instance separation enables template reuse

**Example:**
Template specifies "Spawn plea scene with Trusted NPC with bond minimum 15". At game load SceneTemplate stored with categorical filter. On Day 5 player bonds with Elena reaching value 16. Spawn evaluation: FindMatchingNPC queries and returns Elena. Scene instance created with PlacementId equals "elena". On Day 10 player bonds with Marcus reaching value 18. Spawn SAME template again: FindMatchingNPC now returns Marcus. New Scene instance created with PlacementId equals "marcus". One template enables multiple instances with different placements based on CURRENT game state.

### 3. Why Embed Situations in Scene JSON?

**Composition Pattern**: Scene owns Situations

**Alternatives Rejected:**
**Situations in separate collection:** Breaks composition pattern, orphan situations possible after Scene deletion.
**Situation references only:** Requires garbage collection logic, complicates save/load serialization.
**Embedded in Scene JSON:** Clear ownership semantics, atomic load/save operations, no orphans possible.

**Save/Load:**
Save operation: Serialize Scene to SceneDTO including embedded Situations. Convert DTO to JSON string. Load operation: Deserialize JSON string to SceneDTO. Convert DTO to Scene entity via SceneParser recreating embedded Situations.

### 4. Provisional vs Active Scenes

**OLD Pattern (DELETED):**
- CreateProvisionalScene() → lightweight skeleton
- FinalizeScene() → full instantiation
- DeleteProvisionalScene() → cleanup if not finalized

**NEW Pattern (SIMPLIFIED):**
- Scenes spawn directly as Active (no provisional state)
- Perfect information from template metadata (SituationCount, Tier, EstimatedDifficulty)
- No lightweight/heavyweight distinction
- State enum: Active, Completed, Expired (no Provisional)

**Why Simplified:**
- Provisional = optimization for preview
- Preview data available from SceneTemplate directly
- No need to instantiate Scene for preview
- Spawn = commitment (player already decided)

## The Deleted Provisional Pattern and Why

**Old Pattern:** Scenes had "Provisional" state - lightweight skeletons created for preview. Player could inspect basic info (situation count, estimated difficulty) before committing. FinalizeScene() would then create full instance or DeleteProvisionalScene() if player declined.

**Why Deleted:**
Optimization solving the wrong problem. Preview data (situation count, difficulty tier) available directly from SceneTemplate - no need to instantiate Scene at all. Provisional state added complexity (extra state transitions, cleanup logic, partial instance handling) for negligible benefit.

**New Pattern:**
Spawn means commitment. Template metadata provides perfect information for player decision. No need for trial instantiation.

## Summary

**The correct architecture:**
1. SceneTemplates from static packages (categorical filters)
2. PlacementFilter resolution at spawn time (categorical → concrete)
3. SceneInstantiator generates DTOs + JSON (no direct entity creation)
4. ContentGenerationFacade writes JSON to disk
5. PackageLoader loads JSON via SceneParser
6. SceneParser creates Scene entities (HIGHLANDER: one path)
7. Dependent resources loaded via same PackageLoader
8. DependentResourceOrchestrationService handles post-load setup

**Every entity follows this flow. No exceptions. No special cases. HIGHLANDER.**
