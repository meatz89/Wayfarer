using Wayfarer.GameState.Enums;

namespace Wayfarer.Content;

/// <summary>
/// Domain Service for creating and finalizing Scene instances from SceneTemplates
/// Implements provisional Scene architecture:
/// 1. CreateProvisionalScene: Creates Scene from template (eager creation for perfect information)
/// 2. FinalizeScene: Activate Scene and move to permanent storage (when Choice selected)
/// 3. DeleteProvisionalScene: Cleanup (when Choice NOT selected)
///
/// ARCHITECTURE PRINCIPLE: Scenes created from SceneTemplates with PlacementRelation enum
/// Determines concrete placement (SameLocation, SameNPC, SpecificLocation, etc.) at spawn time
/// </summary>
public class SceneInstantiator
{
    private readonly GameWorld _gameWorld;

    public SceneInstantiator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Create provisional Scene from template
    /// EAGER CREATION: Called when Situation instantiated for each Choice with SceneSpawnReward
    /// Scene has concrete placement and narrative (not finalized until Choice selected)
    /// </summary>
    /// <param name="sceneTemplate">SceneTemplate to instantiate from</param>
    /// <param name="spawnReward">Scene spawn reward containing placement relation and delay info</param>
    /// <param name="context">Spawn context with current entities for resolution</param>
    /// <returns>Provisional Scene with State = Provisional, stored in gameWorld.ProvisionalScenes</returns>
    public Scene CreateProvisionalScene(SceneTemplate sceneTemplate, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        // Generate unique Scene ID
        string sceneId = $"scene_{sceneTemplate.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        // Determine concrete placement based on PlacementRelation from spawnReward
        (PlacementType placementType, string placementId) = ResolvePlacement(sceneTemplate, spawnReward, context);

        // Create Scene instance from template
        Scene scene = new Scene
        {
            Id = sceneId,
            TemplateId = sceneTemplate.Id,
            Template = sceneTemplate, // Composition - reference template for access
            PlacementType = placementType,
            PlacementId = placementId,
            State = SceneState.Provisional, // KEY: Provisional state
            SourceSituationId = context.CurrentSituation?.Id, // Track source for cleanup
            Archetype = sceneTemplate.Archetype,
            DisplayName = sceneTemplate.DisplayNameTemplate,
            IntroNarrative = sceneTemplate.IntroNarrativeTemplate,
            SpawnRules = sceneTemplate.SpawnRules,
            SituationIds = new List<string>() // Will be populated next
        };

        // Create Situations from SituationTemplates - store in GameWorld.Situations, track IDs in Scene
        foreach (SituationTemplate sitTemplate in sceneTemplate.SituationTemplates)
        {
            Situation situation = InstantiateSituation(sitTemplate, scene, context);
            // HIGHLANDER Pattern A: Add to GameWorld.Situations immediately
            _gameWorld.Situations.Add(situation);
            // Track ID in Scene
            scene.SituationIds.Add(situation.Id);
        }

        // Set CurrentSituationId to first Situation ID
        scene.CurrentSituationId = scene.SituationIds.FirstOrDefault();

        // Store in provisional Scenes (temporary storage)
        _gameWorld.ProvisionalScenes[sceneId] = scene;

        return scene;
    }

    /// <summary>
    /// Finalize provisional Scene - activate and move to permanent storage
    /// Called when player SELECTS Choice that spawned this provisional Scene
    /// HIGHLANDER Pattern A: Flatten Situations to GameWorld.Situations for query access
    /// </summary>
    /// <param name="sceneId">Provisional Scene ID to finalize</param>
    /// <param name="context">Spawn context for placeholder replacement</param>
    /// <returns>Finalized Scene with State = Active, stored in gameWorld.Scenes</returns>
    public Scene FinalizeScene(string sceneId, SceneSpawnContext context)
    {
        // Get provisional Scene
        if (!_gameWorld.ProvisionalScenes.TryGetValue(sceneId, out Scene scene))
        {
            throw new InvalidOperationException($"Provisional Scene '{sceneId}' not found in gameWorld.ProvisionalScenes");
        }

        // PLACEHOLDER REPLACEMENT: Convert template text to concrete narrative
        scene.DisplayName = PlaceholderReplacer.ReplaceAll(scene.DisplayName, context, _gameWorld);
        scene.IntroNarrative = PlaceholderReplacer.ReplaceAll(scene.IntroNarrative, context, _gameWorld);

        // Replace placeholders in all Situations (query GameWorld.Situations using scene.SituationIds)
        List<Situation> sceneSituations = _gameWorld.Situations
            .Where(s => scene.SituationIds.Contains(s.Id))
            .ToList();

        foreach (Situation situation in sceneSituations)
        {
            situation.Description = PlaceholderReplacer.ReplaceAll(situation.Description, context, _gameWorld);

            // NO ACTION PLACEHOLDER REPLACEMENT HERE
            // Actions are NOT embedded in Situations (deleted in three-tier timing refactoring)
            // Actions created on-demand by SceneFacade when Situation activates (Dormant → Active)
            // Placeholder replacement for actions happens in SceneFacade at query time
        }

        // Change state to Active
        scene.State = SceneState.Active;

        // Move from provisional to active storage
        _gameWorld.ProvisionalScenes.Remove(sceneId);
        _gameWorld.Scenes.Add(scene);

        return scene;
    }

    /// <summary>
    /// Delete provisional Scene - cleanup when Choice NOT selected
    /// Called when player selects different Choice in Situation
    /// All unselected Choices' provisional Scenes are deleted
    /// </summary>
    /// <param name="sceneId">Provisional Scene ID to delete</param>
    public void DeleteProvisionalScene(string sceneId)
    {
        if (_gameWorld.ProvisionalScenes.ContainsKey(sceneId))
        {
            _gameWorld.ProvisionalScenes.Remove(sceneId);
        }
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Resolve concrete placement based on PlacementRelation enum and context
    /// Returns (PlacementType, PlacementId) tuple
    /// Uses SceneSpawnReward.PlacementRelation (enum) + SpecificPlacementId
    /// </summary>
    private (PlacementType, string) ResolvePlacement(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        PlacementRelation relation = spawnReward.PlacementRelation;

        switch (relation)
        {
            case PlacementRelation.SameLocation:
                if (context.CurrentLocation == null)
                    throw new InvalidOperationException("SameLocation placement requires CurrentLocation in context");
                return (PlacementType.Location, context.CurrentLocation.Id);

            case PlacementRelation.SameNPC:
                if (context.CurrentNPC == null)
                    throw new InvalidOperationException("SameNPC placement requires CurrentNPC in context");
                return (PlacementType.NPC, context.CurrentNPC.ID);

            case PlacementRelation.SameRoute:
                if (context.CurrentRoute == null)
                    throw new InvalidOperationException("SameRoute placement requires CurrentRoute in context");
                return (PlacementType.Route, context.CurrentRoute.Id);

            case PlacementRelation.SpecificLocation:
                if (string.IsNullOrEmpty(spawnReward.SpecificPlacementId))
                    throw new InvalidOperationException("SpecificLocation placement requires SpecificPlacementId in spawnReward");
                return (PlacementType.Location, spawnReward.SpecificPlacementId);

            case PlacementRelation.SpecificNPC:
                if (string.IsNullOrEmpty(spawnReward.SpecificPlacementId))
                    throw new InvalidOperationException("SpecificNPC placement requires SpecificPlacementId in spawnReward");
                return (PlacementType.NPC, spawnReward.SpecificPlacementId);

            case PlacementRelation.SpecificRoute:
                if (string.IsNullOrEmpty(spawnReward.SpecificPlacementId))
                    throw new InvalidOperationException("SpecificRoute placement requires SpecificPlacementId in spawnReward");
                return (PlacementType.Route, spawnReward.SpecificPlacementId);

            default:
                throw new InvalidOperationException($"Unknown PlacementRelation: {relation}");
        }
    }

    // NOTE: AdjacentLocation placement removed from PlacementRelation enum
    // If needed in future, add to enum and implement here with proper LocationSpot → Location mapping

    /// <summary>
    /// Instantiate Situation from SituationTemplate
    /// Creates embedded Situation within Scene in DORMANT state
    /// NO ACTIONS CREATED - actions instantiated at query time by SceneFacade
    /// PLACEMENT INHERITANCE: Situation inherits placement from parent Scene
    /// </summary>
    private Situation InstantiateSituation(SituationTemplate template, Scene parentScene, SceneSpawnContext context)
    {
        // Generate unique Situation ID
        string situationId = $"situation_{template.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        // Create Situation from template
        Situation situation = new Situation
        {
            Id = situationId,
            TemplateId = template.Id,
            Template = template,  // CRITICAL: Store template for lazy action instantiation
            Description = template.NarrativeTemplate,
            NarrativeHints = template.NarrativeHints,
            State = SituationState.Dormant,  // START dormant - actions created at query time

            // PLACEMENT INHERITANCE: Inherit from parent Scene
            ParentScene = parentScene,
            PlacementLocation = context.CurrentLocation != null ?
                _gameWorld.Locations.FirstOrDefault(l => l.Venue?.Id == context.CurrentLocation.Id) : null,
            PlacementNpc = context.CurrentNPC,
            PlacementRouteId = context.CurrentRoute?.Id
        };

        // NO ACTION CREATION HERE - actions instantiated by SceneFacade when player enters context
        // This is the CRITICAL timing change: Instantiation Time (Tier 2) → Query Time (Tier 3)

        return situation;
    }

    // ==================== ACTION GENERATION DELETED ====================
    // GenerateActionFromChoiceTemplate() and DetermineNPCActionType() methods REMOVED
    // Actions are NO LONGER created at Scene instantiation (Tier 2)
    // Actions are NOW created at query time (Tier 3) by SceneFacade
    // This is the CRITICAL architectural refactoring for three-tier timing model
    // See situation-refactor/REFACTORING_PLAN.md for complete details
}
