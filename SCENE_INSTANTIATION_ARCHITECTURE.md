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

**Scenes** = Runtime instances with concrete placements

**Generation Flow:**
SpawnFacade or RewardApplicationService triggers spawn. SceneInstantiator.GenerateScenePackageJson() executes: ResolvePlacement() converts categorical filters to concrete NPC/Location ID, GenerateSceneDTO() creates scene metadata, GenerateSituationDTOs() creates embedded situations collection, GenerateResourceDTOs() creates dependent locations and items. Returns complete JSON string. ContentGenerationFacade.CreateDynamicPackageFile() writes JSON to Content/Dynamic folder with unique package identifier. PackageLoaderFacade.LoadDynamicPackage() reads JSON invoking PackageLoader. SceneParser.ConvertDTOToScene() plus SituationParser process embedded situations. Scene entity with embedded Situation entities created. Added to GameWorld.Scenes collection.

**Dynamic Package Structure:**
Package contains: unique package identifier, content object with scenes array. Each scene contains: unique scene identifier, template identifier reference, placement type (NPC/Location/Route), resolved placement identifier (concrete entity ID from filter), scene state (Active/Completed/Expired), display name for UI, intro narrative text, embedded situations array containing complete SituationDTOs with nested ChoiceTemplateDTOs, optional locations array for dependent resources if scene is self-contained, optional items array for dependent resources if scene is self-contained.

## PlacementFilter Resolution (Categorical → Concrete)

**The Core Pattern:**

**Step 1: SceneTemplate has PlacementFilter** (categorical properties)
PlacementFilter contains: PlacementType enum value (NPC/Location/Route), PersonalityTypes array for NPC filtering, BondThreshold numeric value for relationship minimum, SelectionStrategy enum (HighestBond/Random/Nearest) determining selection logic.

**Step 2: SceneInstantiator resolves to concrete ID** (spawn time)
FindMatchingNPC() queries GameWorld.NPCs collection. Filters candidates by PersonalityTypes inclusion, filters by BondThreshold minimum, filters by any additional criteria. ApplySelectionStrategy() applies selection logic: HighestBond selects NPC with maximum bond value, Random selects randomly from candidates, Nearest selects by proximity. Returns concrete entity identifier string.

**Step 3: SceneDTO contains concrete placement**
SceneDTO contains PlacementId property with concrete entity identifier (no longer categorical filter), PlacementType enum value specifying entity type.

**Step 4: SceneParser validates reference** (load time)
PlacementId is string reference to existing entity. UI and navigation query GameWorld.NPCs or GameWorld.Locations using this identifier. No object resolution occurs during parsing - Scene stores only identifier string.

## Composition Pattern: Scene OWNS Situations

**Scenes embed Situations via direct ownership:**
Scene entity contains Situations property as List collection directly owning Situation instances. SceneDTO mirrors this structure with embedded SituationDTO collection property. Situations have no meaning outside Scene context. Scene lifecycle completely controls Situation lifecycle. Scene completion makes all Situations inaccessible automatically. Situations exist ONLY in Scene.Situations collection, NOT in separate GameWorld.Situations collection.

**Why Composition:**
Situations semantically belong to their parent Scene. Deletion of Scene should delete all Situations. Situations cannot be shared between Scenes. Scene completion invalidates all child Situations simultaneously.

**Parsing Flow:**
SceneParser.ConvertDTOToScene iterates SceneDTO.Situations collection. For each SituationDTO invokes SituationParser.ConvertDTOToSituation passing GameWorld context. Adds resulting Situation entity to Scene.Situations collection establishing ownership.

## Self-Contained Scenes Pattern

**Self-contained scenes generate dependent resources:**
SceneTemplate contains DependentLocationSpecs defining: logical identifier with "generated:" prefix, name template with placeholder tokens, venue identifier for spatial placement, placement strategy enum (AdjacentToNPC/AdjacentToLocation/SameVenue).

At spawn time SceneInstantiator generates LocationDTO containing: concrete scene-prefixed identifier, name with placeholders resolved to actual values, venue identifier for grouping, hex position calculated via placement strategy.

Package contains all related entities: scenes array, locations array with dependent locations, items array with dependent items. Single atomic package load.

Scene tracks created resource identifiers in CreatedLocationIds list. MarkerResolutionMap translates logical identifiers ("generated:safe_house") to concrete identifiers ("scene_elena_plea_001_safe_house") enabling template marker resolution.

**Marker Resolution:**
SituationTemplates reference markers using "generated:" prefix. ChoiceTemplate navigation targets use marker syntax. MarkerResolutionMap performs marker-to-ID translation at runtime. Enables template reusability without hardcoding concrete identifiers.

## Complete Data Flow

**STATIC CONTENT (One-time at game load):**
SceneTemplateDTO loaded from JSON containing categorical PlacementFilter and SceneArchetypeId reference. PackageLoader invokes SceneTemplateParser. Parser invokes SceneArchetypeCatalogue generating complete SituationTemplates. PlacementFilter stored as-is maintaining categorical properties. SceneTemplate entity added to GameWorld.SceneTemplates collection.

**DYNAMIC CONTENT (Every scene spawn):**
SpawnFacade.CheckAndSpawnEligibleScenes evaluates SpawnConditions against player state and world state. Eligible templates trigger spawn. SceneInstantiator.GenerateScenePackageJson executes multi-step generation: ResolvePlacement converts categorical filter to concrete ID via FindMatchingNPC/Location/Route querying GameWorld, ApplySelectionStrategy (Closest/HighestBond/LeastRecent/Random) selects ONE entity from candidates returning PlacementResolution. GenerateSceneDTO creates scene metadata with unique identifier, concrete placement ID, display name with placeholders resolved. GenerateSituationDTOs converts each SituationTemplate to SituationDTO with embedded ChoiceTemplateDTOs and narrative generation (AI or template). GenerateResourceDTOs creates LocationDTOs and ItemDTOs for dependent resources with marker-to-ID mapping. BuildPackage assembles complete package serializing to JSON string.

ContentGenerationFacade.CreateDynamicPackageFile writes JSON to Content/Dynamic folder with unique package filename. PackageLoaderFacade.LoadDynamicPackage reads JSON invoking PackageLoader.LoadPackageContent which performs: LoadScenes converting SceneDTOs via SceneParser resolving TemplateId reference and parsing enums, iterating embedded SituationDTOs invoking SituationParser, adding Scene to GameWorld.Scenes. LoadLocations converting LocationDTOs via LocationParser adding to GameWorld.Locations. LoadItems converting ItemDTOs via ItemParser adding to GameWorld.Items.

DependentResourceOrchestrationService post-processes: Sets Location/Item provenance metadata (SceneId, timestamp), generates hex routes for newly created locations, adds items to player inventory if specified.

**RUNTIME STATE:**
GameWorld.Scenes contains Scene entities with concrete placement. GameWorld.Locations contains dependent locations if any generated. GameWorld.Items contains dependent items if any generated. Player.Inventory contains items from scene if specified.

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

## File Organization

**Content Folder:**
DTOs subfolder contains: SceneTemplateDTO for static blueprints, SceneDTO for runtime instances, SituationDTO embedded in Scene. Parsers subfolder contains SceneTemplateParser for template parsing. SceneParser handles instance parsing. SceneInstantiator performs DTO generation (refactored). PackageLoader loads both templates and instances.

**GameState Folder:**
SceneTemplate stores immutable blueprints. Scene stores runtime instances. Situation owned by Scene. SceneProvenance tracks resource relationships.

**Models Folder:**
Package serves as container. PackageContent updated to contain both SceneTemplates and Scenes collections.

**Services Folder:**
DependentResourceOrchestrationService handles package loading orchestration.

## Migration from Old Architecture

### What Was Deleted

SceneInstantiator deleted methods: CreateProvisionalScene for direct Scene entity creation, FinalizeScene for direct Situation entity creation, InstantiateSituation for direct entity creation, DeleteProvisionalScene for provisional state management, CalculateEstimatedDifficulty for provisional metadata, BuildScenePromptContext moved to generation service, BuildMarkerResolutionMap now happens in parser.

### What Was Added

SceneInstantiator new methods: GenerateScenePackageJson as main entry point, GenerateSceneDTO for Scene DTO generation, GenerateSituationDTOs for Situation DTO generation, GenerateChoiceDTOs for Choice DTO generation, BuildScenePackage for package assembly.

### What Was Kept (Categorical Queries)

SceneInstantiator unchanged methods: ResolvePlacement as entry point, EvaluatePlacementFilter as dispatcher, FindMatchingNPC/Location/Route for categorical queries, ApplySelectionStrategy methods for selection algorithms, CheckPlayerStateFilters for player validation. These methods correctly perform categorical queries returning identifiers without entity creation.

## Testing Strategy

### Unit Tests

**GenerateSceneDTO Validation:** Arrange template with PlacementFilter containing categorical properties. Act by generating JSON via SceneInstantiator. Deserialize JSON to Package. Assert SceneDTO contains concrete placement identifier (not categorical filter), assert PlacementType enum correct.

### Integration Tests

**End-to-End Scene Spawn:** Arrange by loading SceneTemplate from test data. Act by generating JSON via instantiator, creating dynamic package file via ContentGenerationFacade, loading package via PackageLoader. Assert Scene exists in GameWorld.Scenes collection, assert concrete placement identifier matches expected value, assert Situations collection contains expected count.

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
