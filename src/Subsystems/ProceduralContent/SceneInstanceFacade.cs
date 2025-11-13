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
        string packageId = $"scene_{template.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}_package";
        await _contentGenerationFacade.CreateDynamicPackageFile(packageJson, packageId);

        // PHASE 2.3: Load package via PackageLoader (HIGHLANDER: JSON → Parser → Entity)
        await _packageLoaderFacade.LoadDynamicPackage(packageJson, packageId);

        // PHASE 2.4: Retrieve spawned scene from GameWorld
        // Scene was added to GameWorld by PackageLoader
        Scene spawnedScene = _gameWorld.Scenes
            .OrderByDescending(s => s.Id) // Most recently added
            .FirstOrDefault(s => s.TemplateId == template.Id);

        if (spawnedScene == null)
        {
            throw new InvalidOperationException($"Scene from template '{template.Id}' failed to load via PackageLoader");
        }

        // PHASE 2.5: Post-load orchestration (dependent resources)
        if (template.DependentLocations.Any() || template.DependentItems.Any())
        {
            PostLoadOrchestration(spawnedScene, template, context.Player);
        }

        // PHASE 2.6: RUNTIME PLAYABILITY VALIDATION (FAIL-FAST)
        // Validates spawned scene is actually playable by player
        // THROWS InvalidOperationException if scene creates soft-lock condition
        // Architectural principle: "PLAYABILITY OVER COMPILATION" - unplayable scene worse than crash
        _playabilityValidator.ValidatePlayability(spawnedScene);

        Console.WriteLine($"[SceneInstanceFacade] Spawned scene '{spawnedScene.Id}' via HIGHLANDER flow - playability validated");

        return spawnedScene;
    }

    /// <summary>
    /// Post-load orchestration for dependent resources
    /// Sets provenance, generates hex routes, adds items to inventory, builds marker map
    /// </summary>
    private void PostLoadOrchestration(Scene scene, SceneTemplate template, Player player)
    {
        SceneProvenance provenance = new SceneProvenance
        {
            SceneId = scene.Id,
            CreatedDay = _timeManager.CurrentDay,
            CreatedTimeBlock = _timeManager.CurrentTimeBlock,
            CreatedSegment = _timeManager.CurrentSegment
        };

        // Set provenance and generate routes for locations
        foreach (DependentLocationSpec locationSpec in template.DependentLocations)
        {
            string locationId = $"{scene.Id}_{locationSpec.TemplateId}";
            Location location = _gameWorld.GetLocation(locationId);
            if (location != null)
            {
                location.Provenance = provenance;

                if (location.HexPosition.HasValue)
                {
                    List<RouteOption> routes = _hexRouteGenerator.GenerateRoutesForNewLocation(location);
                    foreach (RouteOption route in routes)
                    {
                        _gameWorld.Routes.Add(route);
                    }
                    Console.WriteLine($"[SceneInstanceFacade] Generated {routes.Count} hex routes for location '{location.Name}'");
                }
            }
        }

        // Set provenance and add items to inventory (if specified in spec)
        foreach (DependentItemSpec itemSpec in template.DependentItems)
        {
            string itemId = $"{scene.Id}_{itemSpec.TemplateId}";
            Item item = _gameWorld.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.Provenance = provenance;

                if (itemSpec.AddToInventoryOnCreation)
                {
                    player.Inventory.AddItem(item);
                    Console.WriteLine($"[SceneInstanceFacade] Added item '{item.Name}' to player inventory");
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
    /// Returns list of Situation instances matching context
    /// </summary>
    public List<Situation> GetSituationsAtContext(string locationId, string npcId = null)
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

                // Hierarchical placement: Situation has direct object reference
                string requiredLocationId = situation.Location?.Id;

                // RUNTIME GUARD: Validate required location still exists in GameWorld
                if (situation.Location != null && !_gameWorld.Locations.Contains(situation.Location))
                {
                    Console.WriteLine($"[SceneInstanceFacade] Situation '{situation.Id}' requires deleted location '{situation.Location.Id}' - skipping");
                    continue; // Entity deleted between spawn and interaction - skip situation
                }

                bool locationMatches = string.IsNullOrEmpty(requiredLocationId) || requiredLocationId == locationId;

                // Hierarchical placement: Situation has direct object reference
                string requiredNpcId = situation.Npc?.ID;

                // RUNTIME GUARD: Validate required NPC still exists in GameWorld
                if (situation.Npc != null && !_gameWorld.NPCs.Contains(situation.Npc))
                {
                        Console.WriteLine($"[SceneInstanceFacade] Situation '{situation.Id}' requires deleted NPC '{requiredNpcId}' - skipping");
                        continue; // Entity deleted between spawn and interaction - skip situation
                    }
                }

                bool npcMatches = string.IsNullOrEmpty(requiredNpcId) || requiredNpcId == npcId;

                if (locationMatches && npcMatches)
                {
                    matchingSituations.Add(situation);
                }
            }
        }

        return matchingSituations;
    }

}
