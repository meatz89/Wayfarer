using System.Text.Json;

/// <summary>
/// FACADE: Clean boundary between game code and scene lifecycle/spawning subsystem
///
/// HIGHLANDER COMPLIANCE: Orchestrates JSON generation → File Write → PackageLoader flow
/// NEVER creates entities directly - delegates to proper HIGHLANDER pipeline
///
/// PURPOSE:
/// - Isolates game code from SceneInstantiator internals
/// - Orchestrates dynamic content generation (JSON files + PackageLoader)
/// - Provides clean interface for scene spawning operations
///
/// FLOW:
/// 1. SceneInstantiator.GenerateScenePackageJson() → JSON string
/// 2. ContentGenerationFacade.CreateDynamicPackageFile() → Write to disk
/// 3. PackageLoaderFacade.LoadDynamicPackage() → PackageLoader → SceneParser → Scene entity
///
/// USAGE:
/// - Called from: SceneFacade, RewardApplicationService, SpawnFacade, ObligationActivity
/// - Replaces direct Scene entity creation
/// - Centralizes all scene spawning operations
///
/// TESTABILITY:
/// - Facade is integration layer (thin wrapper)
/// - SceneInstantiator tested separately (pure DTO generation)
/// - Game code tests can mock this facade
/// </summary>
public class SceneInstanceFacade
{
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly ContentGenerationFacade _contentGenerationFacade;
    private readonly PackageLoaderFacade _packageLoaderFacade;
    private readonly HexRouteGenerator _hexRouteGenerator;
    private readonly TimeManager _timeManager;
    private readonly GameWorld _gameWorld;
    private readonly SpawnedScenePlayabilityValidator _playabilityValidator;

    public SceneInstanceFacade(
        SceneInstantiator sceneInstantiator,
        ContentGenerationFacade contentGenerationFacade,
        PackageLoaderFacade packageLoaderFacade,
        HexRouteGenerator hexRouteGenerator,
        TimeManager timeManager,
        GameWorld gameWorld,
        SpawnedScenePlayabilityValidator playabilityValidator)
    {
        _sceneInstantiator = sceneInstantiator ?? throw new ArgumentNullException(nameof(sceneInstantiator));
        _contentGenerationFacade = contentGenerationFacade ?? throw new ArgumentNullException(nameof(contentGenerationFacade));
        _packageLoaderFacade = packageLoaderFacade ?? throw new ArgumentNullException(nameof(packageLoaderFacade));
        _hexRouteGenerator = hexRouteGenerator ?? throw new ArgumentNullException(nameof(hexRouteGenerator));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _playabilityValidator = playabilityValidator ?? throw new ArgumentNullException(nameof(playabilityValidator));
    }

    /// <summary>
    /// TWO-PHASE SPAWNING - PHASE 1: Create deferred scene package (Scene + Situations only)
    /// Returns JSON string with ONLY Scene + Situations (NO dependent resources)
    /// Scene.State = "Deferred" (dependent resources not spawned yet)
    /// Used during startup to create scenes without triggering placement
    /// </summary>
    public string CreateDeferredScenePackage(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        // PHASE 1: Generate JSON with ONLY Scene + Situations (empty lists for dependent resources)
        string packageJson = _sceneInstantiator.CreateDeferredScene(template, spawnReward, context);
        return packageJson;
    }

    /// <summary>
    /// Spawn scene immediately (HIGHLANDER flow)
    ///
    /// FLOW:
    /// 1. Generate JSON package from template
    /// 2. Write JSON to Content/Dynamic/{packageId}.json
    /// 3. Load package via PackageLoader → SceneParser → Scene entity
    /// 4. Return spawned scene
    ///
    /// Used when scene should spawn immediately (rewards, obligations, auto-spawn)
    /// Returns fully activated Scene instance (State = Active)
    /// </summary>
    public async Task<Scene> SpawnScene(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        // PHASE 2.1: Generate JSON package (DTO generation only - no entity creation)
        string packageJson = _sceneInstantiator.GenerateScenePackageJson(template, spawnReward, context);

        if (packageJson == null)
        {
            // Scene not eligible (spawn conditions failed)
            return null;
        }

        // PHASE 2.2: Write JSON to disk
        // No .Substring() on GUID (forbidden operation per CLAUDE.md)
        string packageId = $"scene_{template.Id}_{Guid.NewGuid().ToString("N")}_package";
        await _contentGenerationFacade.CreateDynamicPackageFile(packageJson, packageId);

        // PHASE 2.3: Load package via PackageLoader (HIGHLANDER: JSON → Parser → Entity)
        // Returns PackageLoadResult with direct object references to created entities
        PackageLoadResult loadResult = await _packageLoaderFacade.LoadDynamicPackage(packageJson, packageId);

        // PHASE 2.4: Get spawned scene from result (HIGHLANDER: direct object reference)
        Scene spawnedScene = loadResult.ScenesAdded.FirstOrDefault();

        if (spawnedScene == null)
        {
            throw new InvalidOperationException($"Scene from template '{template.Id}' failed to load via PackageLoader");
        }

        // PHASE 2.5: Post-load orchestration (dependent resources)
        // Creates dependent locations PER-SITUATION via direct CreateSingleLocation calls
        // Items still come from loadResult (JSON path still used for items)
        PostLoadOrchestration(spawnedScene, loadResult, context.Player, context.CurrentVenue);

        // PHASE 2.5: RUNTIME PLAYABILITY VALIDATION (FAIL-FAST)
        // Validates spawned scene is actually playable by player
        // THROWS InvalidOperationException if scene creates soft-lock condition
        // Architectural principle: "PLAYABILITY OVER COMPILATION" - unplayable scene worse than crash
        _playabilityValidator.ValidatePlayability(spawnedScene);

        Console.WriteLine($"[SceneInstanceFacade] Spawned scene '{spawnedScene.TemplateId}' via HIGHLANDER flow - playability validated");

        // PROCEDURAL CONTENT TRACING: Record runtime scene spawn
        if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
        {
            // Spawn trigger determined by caller context (ChoiceReward most common)
            // Auto-links to parent choice via context stack if available
            SceneSpawnNode sceneNode = _gameWorld.ProceduralTracer.RecordSceneSpawn(
                spawnedScene,
                spawnedScene.TemplateId,
                true, // isProcedurallyGenerated = true (runtime spawned via instantiation)
                SpawnTriggerType.ChoiceReward, // Most common trigger for runtime spawns
                _gameWorld.CurrentDay,
                _gameWorld.CurrentTimeBlock
            );

            // Record all embedded situations as children of this scene
            foreach (Situation situation in spawnedScene.Situations)
            {
                _gameWorld.ProceduralTracer.RecordSituationSpawn(
                    situation,
                    sceneNode,
                    SituationSpawnTriggerType.InitialScene
                );
            }
        }

        return spawnedScene;
    }

    /// <summary>
    /// Post-load orchestration for dependent resources
    /// Creates locations PER-SITUATION via CreateSingleLocation (NO JSON, NO matching)
    /// Sets Origin, Provenance, generates hex routes, adds items to inventory
    ///
    /// HIGHLANDER: Direct creation and binding - no string matching
    /// </summary>
    private void PostLoadOrchestration(Scene scene, PackageLoadResult loadResult, Player player, Venue contextVenue)
    {
        // Build provenance for newly created resources
        SceneProvenance provenance = new SceneProvenance
        {
            Scene = scene,
            CreatedDay = _timeManager.CurrentDay,
            CreatedTimeBlock = _timeManager.CurrentTimeBlock,
            CreatedSegment = _timeManager.CurrentSegment
        };

        // Get template for item processing
        SceneTemplate template = scene.Template ?? _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);

        // STEP 1: Configure dependent items (still from loadResult - items use JSON path)
        foreach (Item item in loadResult.ItemsAdded)
        {
            // Set provenance for forensic tracking
            item.Provenance = provenance;

            // Check if this item should be added to inventory
            DependentItemSpec itemSpec = template?.DependentItems?.FirstOrDefault(s => s.Name == item.Name);
            if (itemSpec != null && itemSpec.AddToInventoryOnCreation)
            {
                player.Inventory.Add(item);
                Console.WriteLine($"[SceneInstanceFacade] Added item '{item.Name}' to player inventory");
            }
        }

        // STEP 2: Create and bind locations PER-SITUATION via direct CreateSingleLocation
        // NO JSON serialization - creates Location directly from spec
        // Shared spec instance = shared location (tracked by Dictionary for reference equality)
        Dictionary<DependentLocationSpec, Location> specToLocationMap = new Dictionary<DependentLocationSpec, Location>();

        foreach (Situation situation in scene.Situations)
        {
            DependentLocationSpec spec = situation.Template?.DependentLocationSpec;
            if (spec != null)
            {
                // Check if we've already created a location for this spec instance
                if (specToLocationMap.TryGetValue(spec, out Location existingLocation))
                {
                    // Shared spec instance = shared location (reference equality)
                    situation.Location = existingLocation;
                    Console.WriteLine($"[SceneInstanceFacade] Bound situation '{situation.Name}' to SHARED location '{existingLocation.Name}'");
                }
                else
                {
                    // CREATE location directly from spec (NO JSON, NO matching)
                    // Returns ONE Location reference - direct binding
                    Location createdLocation = _packageLoaderFacade.CreateSingleLocation(spec, contextVenue);

                    // Set Origin and Provenance
                    createdLocation.Origin = LocationOrigin.SceneCreated;
                    createdLocation.Provenance = provenance;

                    // Generate hex routes if location has hex position
                    if (createdLocation.HexPosition.HasValue)
                    {
                        List<RouteOption> routes = _hexRouteGenerator.GenerateRoutesForNewLocation(createdLocation);
                        foreach (RouteOption route in routes)
                        {
                            _gameWorld.Routes.Add(route);
                        }
                        Console.WriteLine($"[SceneInstanceFacade] Generated {routes.Count} hex routes for location '{createdLocation.Name}'");
                    }

                    // DIRECT BINDING: situation.Location = createdLocation (NO matching)
                    situation.Location = createdLocation;
                    specToLocationMap[spec] = createdLocation;

                    Console.WriteLine($"[SceneInstanceFacade] CREATED and bound location '{createdLocation.Name}' for situation '{situation.Name}'");
                }
            }
        }
    }

    /// <summary>
    /// Get all situations that can activate at given context (location + optional NPC)
    ///
    /// Query all active scenes, return situations matching context requirements
    /// Used by UI to determine what choices to show player
    ///
    /// RUNTIME GUARDS: Validates required entities still exist (spawn-to-interaction gap protection)
    ///
    /// ARCHITECTURAL FIX: Accepts object references instead of string IDs
    /// Returns list of Situation instances matching context
    /// </summary>
    public List<Situation> GetSituationsAtContext(Location location, NPC npc = null)
    {
        List<Situation> matchingSituations = new();

        foreach (Scene scene in _gameWorld.Scenes)
        {
            // Skip inactive scenes
            if (scene.State != SceneState.Active)
                continue;

            // Get situations directly from scene (direct object ownership)
            foreach (Situation situation in scene.Situations)
            {
                // Skip if situation already completed
                if (situation.IsCompleted)
                    continue;

                // RUNTIME GUARD: Validate required location still exists in GameWorld
                if (situation.Location != null && !_gameWorld.Locations.Contains(situation.Location))
                {
                    Console.WriteLine($"[SceneInstanceFacade] Situation '{situation.Name}' requires deleted location '{situation.Location.Name}' - skipping");
                    continue; // Entity deleted between spawn and interaction - skip situation
                }

                // Object reference comparison (no ID extraction)
                bool locationMatches = situation.Location == null || situation.Location == location;

                // RUNTIME GUARD: Validate required NPC still exists in GameWorld
                if (situation.Npc != null && !_gameWorld.NPCs.Contains(situation.Npc))
                {
                    Console.WriteLine($"[SceneInstanceFacade] Situation '{situation.Name}' requires deleted NPC '{situation.Npc.Name}' - skipping");
                    continue; // Entity deleted between spawn and interaction - skip situation
                }

                // Object reference comparison (no ID extraction)
                bool npcMatches = situation.Npc == null || situation.Npc == npc;

                if (locationMatches && npcMatches)
                {
                    matchingSituations.Add(situation);
                }
            }
        }

        return matchingSituations;
    }

}
