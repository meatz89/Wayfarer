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

**JSON Source:**
```json
{
  "sceneTemplates": [
    {
      "id": "elena_plea",
      "sceneArchetypeId": "plea_for_help",
      "tier": 1,
      "placementFilter": {
        "placementType": "NPC",
        "personalityTypes": ["Trusted", "Desperate"],
        "bondThreshold": 15,
        "selectionStrategy": "HighestBond"
      }
    }
  ]
}
```

**Loading Flow:**
```
Static Package JSON
  ↓ PackageLoader
SceneTemplateParser.ParseSceneTemplate()
  ↓ Catalogue Translation
SceneTemplate entity
  ↓
GameWorld.SceneTemplates.Add()
```

**Key Properties:**
- **Categorical Filters**: PersonalityTypes, LocationProperties, BondThreshold
- **SceneArchetypeId**: References catalogue for SituationTemplates generation
- **Reusable**: One template spawns many scene instances
- **Parse-Time Catalogue**: SceneArchetypeCatalogue generates SituationTemplates

### Phase 2: Instance Spawning (Dynamic Content)

**Scenes** = Runtime instances with concrete placements

**Generation Flow:**
```
SpawnFacade/RewardApplicationService
  ↓ spawn trigger
SceneInstantiator.GenerateScenePackageJson()
  ├─ ResolvePlacement() → concrete NPC/Location ID
  ├─ GenerateSceneDTO() → scene metadata
  ├─ GenerateSituationDTOs() → embedded situations
  └─ GenerateResourceDTOs() → dependent locations/items
  ↓ returns JSON string
ContentGenerationFacade.CreateDynamicPackageFile()
  ↓ writes to Content/Dynamic/{packageId}.json
PackageLoaderFacade.LoadDynamicPackage()
  ↓ JSON → PackageLoader
SceneParser.ConvertDTOToScene()
  ↓ + SituationParser for embedded situations
Scene entity + Situation entities
  ↓
GameWorld.Scenes.Add()
```

**Dynamic Package Structure:**
```json
{
  "packageId": "scene_elena_plea_001_package",
  "content": {
    "scenes": [
      {
        "id": "scene_elena_plea_001",
        "templateId": "elena_plea",
        "placementType": "NPC",
        "placementId": "elena",  // ← Resolved from PlacementFilter
        "state": "Active",
        "displayName": "Elena's Plea for Help",
        "introNarrative": "Elena pulls you aside...",
        "situations": [
          {
            "id": "situation_plea_intro_001",
            "name": "The Request",
            "description": "Elena explains the situation...",
            // ... full SituationDTO with ChoiceTemplateDTOs
          }
        ]
      }
    ],
    "locations": [
      // Dependent resources if scene is self-contained
    ],
    "items": [
      // Dependent resources if scene is self-contained
    ]
  }
}
```

## PlacementFilter Resolution (Categorical → Concrete)

**The Core Pattern:**

1. **SceneTemplate has PlacementFilter** (categorical properties)
   ```csharp
   PlacementFilter {
     PlacementType = PlacementType.NPC,
     PersonalityTypes = [Trusted, Desperate],
     BondThreshold = 15,
     SelectionStrategy = HighestBond
   }
   ```

2. **SceneInstantiator resolves to concrete ID** (spawn time)
   ```csharp
   // FindMatchingNPC() queries GameWorld
   List<NPC> candidates = gameWorld.NPCs
     .Where(n => filter.PersonalityTypes.Contains(n.PersonalityType))
     .Where(n => player.GetBond(n.ID) >= filter.BondThreshold)
     .ToList();

   // ApplySelectionStrategy() chooses ONE
   NPC selected = SelectHighestBondNPC(candidates, player);

   // Returns concrete ID
   return selected.ID; // "elena"
   ```

3. **SceneDTO contains concrete placement** (in JSON)
   ```csharp
   SceneDTO {
     PlacementId = "elena",  // Concrete ID, not categorical
     PlacementType = "NPC"
   }
   ```

4. **SceneParser validates reference** (load time)
   ```csharp
   // PlacementId is just a string reference
   // UI/Navigation uses it to query GameWorld.NPCs or GameWorld.Locations
   // No object resolution needed - Scene just stores the ID
   ```

## Composition Pattern: Scene OWNS Situations

**Scenes embed Situations** (not references):

```csharp
// Scene entity
public class Scene {
  public List<Situation> Situations { get; set; }  // OWNS
}

// SceneDTO mirrors this
public class SceneDTO {
  public List<SituationDTO> Situations { get; set; }  // Embedded
}
```

**Why Composition:**
- Situations have no meaning outside their Scene context
- Scene lifecycle controls Situation lifecycle
- Scene completion = all Situations become inaccessible
- Situations are NOT in GameWorld.Situations (only in Scene.Situations)

**Parsing:**
```csharp
// SceneParser.ConvertDTOToScene()
foreach (SituationDTO situationDto in dto.Situations)
{
    Situation situation = SituationParser.ConvertDTOToSituation(situationDto, gameWorld);
    scene.Situations.Add(situation);  // Add to Scene's collection
}
```

## Self-Contained Scenes Pattern

**Self-contained scenes generate dependent resources** (locations, items):

```csharp
// SceneTemplate has DependentLocationSpecs
DependentLocationSpec {
  Id = "generated:safe_house",
  NameTemplate = "{NPCName}'s Safe House",
  VenueId = "old_town",
  PlacementStrategy = "AdjacentToNPC"
}

// At spawn time, SceneInstantiator generates LocationDTO
LocationDTO {
  Id = "scene_elena_plea_001_safe_house",  // Concrete ID
  Name = "Elena's Safe House",
  VenueId = "old_town",
  HexPosition = {q: 5, r: -3}  // Adjacent to Elena's location
}

// Same package contains Scene + dependent resources
Package {
  Scenes = [sceneDto],
  Locations = [locationDto],
  Items = [itemDto]
}

// Scene tracks created resources
Scene {
  CreatedLocationIds = ["scene_elena_plea_001_safe_house"],
  MarkerResolutionMap = {
    "generated:safe_house" → "scene_elena_plea_001_safe_house"
  }
}
```

**Marker Resolution:**
- SituationTemplates reference "generated:safe_house" (template marker)
- ChoiceTemplate navigation uses marker: `{target: "generated:safe_house"}`
- MarkerResolutionMap translates marker → actual ID at runtime
- Enables template reuse without hardcoding concrete IDs

## Complete Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│ STATIC CONTENT (One-time at game load)                         │
├─────────────────────────────────────────────────────────────────┤
│ SceneTemplateDTO (JSON)                                         │
│   - Categorical PlacementFilter                                 │
│   - SceneArchetypeId reference                                  │
│   ↓ PackageLoader                                               │
│ SceneTemplateParser                                             │
│   - SceneArchetypeCatalogue generates SituationTemplates        │
│   - PlacementFilter stored as-is (categorical)                  │
│   ↓                                                              │
│ SceneTemplate entity → GameWorld.SceneTemplates                 │
└─────────────────────────────────────────────────────────────────┘
                          ↓ spawn trigger
┌─────────────────────────────────────────────────────────────────┐
│ DYNAMIC CONTENT (Every scene spawn)                            │
├─────────────────────────────────────────────────────────────────┤
│ SpawnFacade.CheckAndSpawnEligibleScenes()                      │
│   - Evaluates SpawnConditions (player state, world state)      │
│   - Eligible template → spawn                                   │
│   ↓                                                              │
│ SceneInstantiator.GenerateScenePackageJson(template)           │
│   ├─ ResolvePlacement(template.PlacementFilter)                │
│   │   ├─ FindMatchingNPC/Location/Route()                      │
│   │   │   - Query GameWorld with categorical filters           │
│   │   │   - Returns List<NPC> matching PersonalityTypes        │
│   │   ├─ ApplySelectionStrategy()                              │
│   │   │   - Closest / HighestBond / LeastRecent / Random       │
│   │   │   - Returns ONE NPC from candidates                    │
│   │   └─ Returns PlacementResolution {Type, Id}                │
│   │                                                              │
│   ├─ GenerateSceneDTO(template, placement)                     │
│   │   - Id = "scene_{templateId}_{guid}"                       │
│   │   - PlacementId = placement.Id  (concrete)                 │
│   │   - DisplayName = template with placeholders replaced      │
│   │                                                              │
│   ├─ GenerateSituationDTOs(template.SituationTemplates)        │
│   │   - For each SituationTemplate → SituationDTO              │
│   │   - Embedded ChoiceTemplateDTOs                            │
│   │   - Narrative generation (AI or template)                  │
│   │                                                              │
│   ├─ GenerateResourceDTOs(template.DependentSpecs)            │
│   │   - LocationDTOs for DependentLocationSpecs                │
│   │   - ItemDTOs for DependentItemSpecs                        │
│   │   - Marker → Concrete ID mapping                           │
│   │                                                              │
│   └─ BuildPackage(sceneDto, situationDtos, resourceDtos)       │
│       - Serialize to JSON string                                │
│   ↓                                                              │
│ ContentGenerationFacade.CreateDynamicPackageFile()             │
│   - Write JSON to Content/Dynamic/{packageId}.json             │
│   ↓                                                              │
│ PackageLoaderFacade.LoadDynamicPackage()                       │
│   ↓                                                              │
│ PackageLoader.LoadPackageContent()                             │
│   ├─ LoadScenes(package.Content.Scenes)                        │
│   │   ├─ SceneParser.ConvertDTOToScene()                       │
│   │   │   ├─ Resolve TemplateId → Template object              │
│   │   │   ├─ Parse enums (PlacementType, State, etc.)          │
│   │   │   └─ For each SituationDTO:                            │
│   │   │       └─ SituationParser.ConvertDTOToSituation()       │
│   │   └─ GameWorld.Scenes.Add(scene)                           │
│   │                                                              │
│   ├─ LoadLocations(package.Content.Locations)                  │
│   │   └─ LocationParser → GameWorld.Locations                  │
│   │                                                              │
│   └─ LoadItems(package.Content.Items)                          │
│       └─ ItemParser → GameWorld.Items                          │
│   ↓                                                              │
│ DependentResourceOrchestrationService (post-load)              │
│   ├─ Set Location/Item provenance (SceneId, timestamp)         │
│   ├─ Generate hex routes for new locations                     │
│   └─ Add items to player inventory                             │
└─────────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────────┐
│ RUNTIME STATE                                                   │
├─────────────────────────────────────────────────────────────────┤
│ GameWorld.Scenes ← Scene entity with concrete placement        │
│ GameWorld.Locations ← Dependent locations (if any)              │
│ GameWorld.Items ← Dependent items (if any)                      │
│ Player.Inventory ← Items from scene (if specified)              │
└─────────────────────────────────────────────────────────────────┘
```

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
```
Template: "Spawn plea scene with Trusted NPC with bond ≥15"
  ↓ Game load
SceneTemplate stored with categorical filter
  ↓ Day 5: Player bonds with Elena (16)
Spawn: FindMatchingNPC() → Elena selected
  ↓
Scene instance: PlacementId = "elena"
  ↓ Day 10: Player bonds with Marcus (18)
Spawn SAME template again: FindMatchingNPC() → Marcus selected
  ↓
Scene instance: PlacementId = "marcus"
```

One template, multiple instances, different placements based on CURRENT state.

### 3. Why Embed Situations in Scene JSON?

**Composition Pattern**: Scene owns Situations

**Alternatives Rejected:**
- ❌ Situations in separate collection: Breaks composition, orphan situations possible
- ❌ Situation references only: Requires garbage collection, complicates save/load
- ✓ Embedded in Scene JSON: Clear ownership, atomic load/save, no orphans

**Save/Load:**
```csharp
// Save: Serialize Scene → JSON (situations embedded)
SceneDTO dto = SceneDTO.FromEntity(scene);
string json = JsonSerializer.Serialize(dto);

// Load: Deserialize JSON → Scene (situations recreated)
SceneDTO dto = JsonSerializer.Deserialize<SceneDTO>(json);
Scene scene = SceneParser.ConvertDTOToScene(dto);
```

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

```
/home/user/Wayfarer/src/
├── Content/
│   ├── DTOs/
│   │   ├── SceneTemplateDTO.cs       (Static blueprint)
│   │   ├── SceneDTO.cs                (Runtime instance) ← NEW
│   │   └── SituationDTO.cs            (Embedded in Scene)
│   ├── Parsers/
│   │   └── SceneTemplateParser.cs     (Template parsing)
│   ├── SceneParser.cs                 (Instance parsing) ← NEW
│   ├── SceneInstantiator.cs           (DTO generation) ← REFACTORED
│   └── PackageLoader.cs               (Loads both templates + instances)
├── GameState/
│   ├── SceneTemplate.cs               (Immutable blueprint)
│   ├── Scene.cs                       (Runtime instance)
│   ├── Situation.cs                   (Owned by Scene)
│   └── SceneProvenance.cs             (Resource tracking)
├── Models/
│   ├── Package.cs                     (Container)
│   └── PackageContent.cs              (Has SceneTemplates + Scenes) ← UPDATED
└── Services/
    └── DependentResourceOrchestrationService.cs  (Loads packages)
```

## Migration from Old Architecture

### What Was Deleted

```csharp
// SceneInstantiator.cs - DELETED METHODS
CreateProvisionalScene()           // Direct Scene entity creation
FinalizeScene()                     // Direct Situation entity creation
InstantiateSituation()              // Direct entity creation
DeleteProvisionalScene()            // Provisional state management
CalculateEstimatedDifficulty()      // Provisional metadata
BuildScenePromptContext()           // Moved to generation service
BuildMarkerResolutionMap()          // Now happens in parser
```

### What Was Added

```csharp
// SceneInstantiator.cs - NEW METHODS
GenerateScenePackageJson()          // Main entry point
GenerateSceneDTO()                  // Scene DTO generation
GenerateSituationDTOs()             // Situation DTO generation
GenerateChoiceDTOs()                // Choice DTO generation
BuildScenePackage()                 // Package assembly
```

### What Was Kept (Categorical Queries)

```csharp
// SceneInstantiator.cs - UNCHANGED
ResolvePlacement()                  // Entry point
EvaluatePlacementFilter()           // Dispatcher
FindMatchingNPC/Location/Route()    // Categorical queries
ApplySelectionStrategy*()           // Selection algorithms
CheckPlayerStateFilters()           // Player validation
```

These methods are CORRECT - they perform categorical queries returning IDs, no entity creation.

## Testing Strategy

### Unit Tests

```csharp
[Test]
public void GenerateSceneDTO_WithConcreteNPC_CreatesCorrectDTO()
{
    // Arrange: Template with PlacementFilter
    SceneTemplate template = new SceneTemplate {
        PlacementFilter = new PlacementFilter {
            PlacementType = PlacementType.NPC,
            PersonalityTypes = new List<PersonalityType> { PersonalityType.Trusted }
        }
    };

    // Act: Generate DTO
    string json = _instantiator.GenerateScenePackageJson(template, ...);
    Package package = JsonSerializer.Deserialize<Package>(json);

    // Assert: SceneDTO has concrete placement
    Assert.AreEqual("elena", package.Content.Scenes[0].PlacementId);
    Assert.AreEqual("NPC", package.Content.Scenes[0].PlacementType);
}
```

### Integration Tests

```csharp
[Test]
public void SceneSpawn_EndToEnd_LoadsViaPackageLoader()
{
    // Arrange: Template
    SceneTemplate template = LoadTemplate("elena_plea");

    // Act: Spawn
    string json = _instantiator.GenerateScenePackageJson(template, ...);
    _contentGeneration.CreateDynamicPackageFile(json, "test_package");
    _packageLoader.LoadDynamicPackage(json, "test_package");

    // Assert: Scene in GameWorld
    Scene scene = _gameWorld.Scenes.First(s => s.TemplateId == "elena_plea");
    Assert.IsNotNull(scene);
    Assert.AreEqual("elena", scene.PlacementId);
    Assert.AreEqual(4, scene.Situations.Count);
}
```

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
