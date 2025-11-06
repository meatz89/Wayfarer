using System.Text.Json;
using Wayfarer.Content;
using Wayfarer.Content.DTOs;
using Wayfarer.GameState;
using Wayfarer.Services;

/// <summary>
/// FACADE: Clean boundary between game code and scene lifecycle/spawning subsystem
///
/// PURPOSE:
/// - Isolates game code from SceneInstantiator internals
/// - Provides clean interface for scene lifecycle operations
/// - Wraps provisional scene creation, activation, and cleanup
/// - ORCHESTRATES dynamic content generation (JSON files + PackageLoader)
///
/// USAGE:
/// - Called from: SceneFacade, RewardApplicationService, SpawnFacade, ObligationActivity
/// - Replaces direct SceneInstantiator calls
/// - Centralizes all scene instance operations
///
/// ORCHESTRATION:
/// - SceneInstantiator generates specs (pure domain logic)
/// - Facade creates JSON files and loads via PackageLoader (infrastructure)
/// - Full isolation: generation knows nothing about file I/O
///
/// TESTABILITY:
/// - Facade is integration layer (thin wrapper)
/// - SceneInstantiator tested separately (pure functions)
/// - Game code tests can mock this facade
/// </summary>
public class SceneInstanceFacade
{
    private readonly SceneInstantiator _sceneInstantiator;
    private readonly GameWorld _gameWorld;

    public SceneInstanceFacade(
        SceneInstantiator sceneInstantiator,
        GameWorld gameWorld)
    {
        _sceneInstantiator = sceneInstantiator ?? throw new ArgumentNullException(nameof(sceneInstantiator));
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Create AND activate a scene in one operation (for immediate spawns)
    ///
    /// Flow:
    /// 1. CreateProvisionalScene (eager creation for perfect information)
    /// 2. FinalizeScene (activate and add to GameWorld)
    ///
    /// Used when scene should spawn immediately (rewards, obligations, auto-spawn)
    /// Returns fully activated Scene instance
    /// </summary>
    public Scene SpawnScene(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        Scene provisionalScene = _sceneInstantiator.CreateProvisionalScene(template, spawnReward, context);

        (Scene finalizedScene, DependentResourceSpecs _) = FinalizeScene(provisionalScene.Id, context);

        return finalizedScene;
    }

    /// <summary>
    /// Create provisional scene (eager creation, not yet activated)
    ///
    /// Used when player needs to see scene BEFORE choosing (perfect information pattern)
    /// Scene is created but not added to GameWorld until FinalizeScene() called
    ///
    /// Returns provisional Scene instance
    /// </summary>
    public Scene CreateProvisionalScene(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        return _sceneInstantiator.CreateProvisionalScene(template, spawnReward, context);
    }

    /// <summary>
    /// Activate a provisional scene (scene lifecycle only - NO orchestration)
    /// Calls SceneInstantiator to finalize scene and instantiate situations
    /// Returns Scene with State=Active AND DependentResourceSpecs for orchestrator
    /// </summary>
    public (Scene scene, DependentResourceSpecs dependentSpecs) FinalizeScene(string sceneId, SceneSpawnContext context)
    {
        return _sceneInstantiator.FinalizeScene(sceneId, context);
    }

    /// <summary>
    /// Delete a provisional scene (cleanup unselected choice)
    ///
    /// Called when player selects a different provisional scene
    /// Cleans up resources for unselected scene
    /// </summary>
    public void DeleteProvisionalScene(string sceneId)
    {
        _sceneInstantiator.DeleteProvisionalScene(sceneId);
    }

    /// <summary>
    /// Get all situations that can activate at given context (location + optional NPC)
    ///
    /// Query all active scenes, return situations matching context requirements
    /// Used by UI to determine what choices to show player
    ///
    /// Returns list of Situation instances matching context
    /// </summary>
    public List<Situation> GetSituationsAtContext(string locationId, string npcId = null)
    {
        List<Situation> matchingSituations = new();

        foreach (Scene scene in _gameWorld.Scenes)
        {
            // Query GameWorld.Situations filtered by scene's SituationIds (HIGHLANDER Pattern A)
            List<Situation> sceneSituations = _gameWorld.Situations
                .Where(s => scene.SituationIds.Contains(s.Id))
                .ToList();

            foreach (Situation situation in sceneSituations)
            {
                // Skip if situation already completed
                if (situation.IsCompleted)
                    continue;

                // Resolve location requirement (resolved marker or template)
                string requiredLocationId = situation.ResolvedRequiredLocationId ?? situation.Template?.RequiredLocationId;
                bool locationMatches = string.IsNullOrEmpty(requiredLocationId) || requiredLocationId == locationId;

                // Resolve NPC requirement (resolved marker or template)
                string requiredNpcId = situation.ResolvedRequiredNpcId ?? situation.Template?.RequiredNpcId;
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
