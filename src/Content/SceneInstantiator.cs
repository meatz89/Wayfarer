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

        // PLAYABILITY VALIDATION: Scene must have at least one Situation
        if (!scene.SituationIds.Any())
            throw new InvalidOperationException($"Scene '{scene.Id}' (template '{sceneTemplate.Id}') has no SituationIds - player cannot interact!");

        // Set CurrentSituationId to first Situation ID
        scene.CurrentSituationId = scene.SituationIds.First();

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

            case PlacementRelation.Generic:
                // Evaluate PlacementFilter from SceneTemplate to find matching entity
                if (template.PlacementFilter == null)
                    throw new InvalidOperationException($"Generic placement requires PlacementFilter on SceneTemplate '{template.Id}'");

                string placementId = EvaluatePlacementFilter(template.PlacementFilter, context);
                if (string.IsNullOrEmpty(placementId))
                {
                    throw new InvalidOperationException(
                        $"No matching entity found for PlacementFilter on SceneTemplate '{template.Id}'.\n{FormatFilterCriteria(template.PlacementFilter)}"
                    );
                }

                PlacementType placementType = template.PlacementFilter.PlacementType;
                return (placementType, placementId);

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
            Type = template.Type,  // Copy semantic type (Normal vs Crisis) from template
            Description = template.NarrativeTemplate,
            NarrativeHints = template.NarrativeHints,
            State = SituationState.Dormant,  // START dormant - actions created at query time
            IsAutoAdvance = parentScene.Archetype == SpawnPattern.AutoAdvance,  // Copy archetype to Situation

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

    // ==================== GENERIC PLACEMENT FILTER EVALUATION ====================
    // RUNTIME PlacementFilter evaluation for Generic placement relation
    // Replicates GameWorldInitializer.FindStarterPlacement() logic for runtime spawning
    // Supports AI-generated scenes with categorical properties instead of hardcoded IDs

    /// <summary>
    /// Evaluate PlacementFilter to find matching entity at runtime
    /// Returns entity ID or null if no match found
    /// Throws InvalidOperationException if no matching entity (fail fast)
    /// </summary>
    private string EvaluatePlacementFilter(PlacementFilter filter, SceneSpawnContext context)
    {
        Player player = context.Player;

        switch (filter.PlacementType)
        {
            case PlacementType.NPC:
                return FindMatchingNPC(filter, player);

            case PlacementType.Location:
                return FindMatchingLocation(filter, player);

            case PlacementType.Route:
                return FindMatchingRoute(filter, player);

            default:
                throw new InvalidOperationException($"Unknown PlacementType: {filter.PlacementType}");
        }
    }

    /// <summary>
    /// Find NPC matching PlacementFilter criteria
    /// REFERENCE IMPLEMENTATION: Copied from GameWorldInitializer.FindStarterPlacement() (lines 120-142)
    /// Returns first NPC ID matching ALL filter criteria, or null if no match
    /// </summary>
    private string FindMatchingNPC(PlacementFilter filter, Player player)
    {
        NPC matchingNPC = _gameWorld.NPCs.FirstOrDefault(npc =>
        {
            // Check personality type (if specified)
            if (filter.PersonalityTypes != null && filter.PersonalityTypes.Count > 0)
            {
                if (!filter.PersonalityTypes.Contains(npc.PersonalityType))
                    return false;
            }

            // Check bond thresholds (BondStrength stored directly on NPC)
            int currentBond = npc.BondStrength;
            if (filter.MinBond.HasValue && currentBond < filter.MinBond.Value)
                return false;
            if (filter.MaxBond.HasValue && currentBond > filter.MaxBond.Value)
                return false;

            // NPC tags check removed - NPCs don't have Tags property in current architecture
            // TODO: Add filter.NpcTags check when NPC.Tags property implemented

            // Check player state filters (shared across all placement types)
            if (!CheckPlayerStateFilters(filter, player))
                return false;

            return true;
        });

        return matchingNPC?.ID;
    }

    /// <summary>
    /// Find Location matching PlacementFilter criteria
    /// Returns first Location ID matching ALL filter criteria, or null if no match
    /// </summary>
    private string FindMatchingLocation(PlacementFilter filter, Player player)
    {
        Location matchingLocation = _gameWorld.Locations.FirstOrDefault(loc =>
        {
            // Check location properties (if specified)
            if (filter.LocationProperties != null && filter.LocationProperties.Count > 0)
            {
                // Location must have ALL specified properties
                if (!filter.LocationProperties.All(prop => loc.LocationProperties.Contains(prop)))
                    return false;
            }

            // TODO: District/Region filters when Location.DistrictId/RegionId properties implemented
            // if (!string.IsNullOrEmpty(filter.DistrictId))
            // {
            //     if (loc.DistrictId != filter.DistrictId)
            //         return false;
            // }
            // if (!string.IsNullOrEmpty(filter.RegionId))
            // {
            //     if (loc.RegionId != filter.RegionId)
            //         return false;
            // }

            // Location tags check removed - Locations don't have Tags property in current architecture
            // TODO: Add filter.LocationTags check when Location.Tags property implemented

            // Check player state filters (shared across all placement types)
            if (!CheckPlayerStateFilters(filter, player))
                return false;

            return true;
        });

        return matchingLocation?.Id;
    }

    /// <summary>
    /// Find Route matching PlacementFilter criteria
    /// Returns first Route ID matching ALL filter criteria, or null if no match
    /// ⚠️ BLOCKED: Route entity missing TerrainType, Tier, DangerRating properties
    /// Currently returns first route as fallback
    /// </summary>
    private string FindMatchingRoute(PlacementFilter filter, Player player)
    {
        RouteOption matchingRoute = _gameWorld.Routes.FirstOrDefault(route =>
        {
            // TODO: Check terrain types when Route.TerrainType property exists
            // if (filter.TerrainTypes != null && filter.TerrainTypes.Count > 0)
            // {
            //     if (!filter.TerrainTypes.Contains(route.TerrainType))
            //         return false;
            // }

            // TODO: Check route tier when Route.Tier property exists
            // if (filter.RouteTier.HasValue)
            // {
            //     if (route.Tier != filter.RouteTier.Value)
            //         return false;
            // }

            // TODO: Check danger rating when Route.DangerRating property exists
            // if (filter.MinDangerRating.HasValue && route.DangerRating < filter.MinDangerRating.Value)
            //     return false;
            // if (filter.MaxDangerRating.HasValue && route.DangerRating > filter.MaxDangerRating.Value)
            //     return false;

            // Check player state filters (shared across all placement types)
            if (!CheckPlayerStateFilters(filter, player))
                return false;

            return true;
        });

        return matchingRoute?.Id;
    }

    /// <summary>
    /// Validate player state filters (shared across all placement types)
    /// Returns true if player meets all state requirements, false otherwise
    /// </summary>
    private bool CheckPlayerStateFilters(PlacementFilter filter, Player player)
    {
        // Check required states
        if (filter.RequiredStates != null && filter.RequiredStates.Count > 0)
        {
            // Player must have ALL required states
            if (!filter.RequiredStates.All(state => player.ActiveStates.Any(activeState => activeState.Type == state)))
                return false;
        }

        // Check forbidden states
        if (filter.ForbiddenStates != null && filter.ForbiddenStates.Count > 0)
        {
            // Player must have NONE of the forbidden states
            if (filter.ForbiddenStates.Any(state => player.ActiveStates.Any(activeState => activeState.Type == state)))
                return false;
        }

        // Check required achievements
        if (filter.RequiredAchievements != null && filter.RequiredAchievements.Count > 0)
        {
            // Player must have ALL required achievements
            if (!filter.RequiredAchievements.All(achId =>
                player.EarnedAchievements.Any(a => a.AchievementId == achId)))
                return false;
        }

        // Check scale requirements
        if (filter.ScaleRequirements != null && filter.ScaleRequirements.Count > 0)
        {
            foreach (ScaleRequirement scaleReq in filter.ScaleRequirements)
            {
                int scaleValue = GetScaleValue(player, scaleReq.ScaleType);

                if (scaleReq.MinValue.HasValue && scaleValue < scaleReq.MinValue.Value)
                    return false;

                if (scaleReq.MaxValue.HasValue && scaleValue > scaleReq.MaxValue.Value)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get current scale value for player
    /// REFERENCE: Copied from ConsequenceFacade.GetScaleValue() (lines 157-169)
    /// </summary>
    private int GetScaleValue(Player player, ScaleType scaleType)
    {
        return scaleType switch
        {
            ScaleType.Morality => player.Scales.Morality,
            ScaleType.Lawfulness => player.Scales.Lawfulness,
            ScaleType.Method => player.Scales.Method,
            ScaleType.Caution => player.Scales.Caution,
            ScaleType.Transparency => player.Scales.Transparency,
            ScaleType.Fame => player.Scales.Fame,
            _ => 0
        };
    }

    /// <summary>
    /// Format PlacementFilter criteria for diagnostic error messages
    /// Returns human-readable summary of all filter criteria
    /// </summary>
    private string FormatFilterCriteria(PlacementFilter filter)
    {
        List<string> criteria = new List<string>();

        criteria.Add($"PlacementType: {filter.PlacementType}");

        // NPC filters
        if (filter.PersonalityTypes != null && filter.PersonalityTypes.Count > 0)
            criteria.Add($"Personality Types: [{string.Join(", ", filter.PersonalityTypes)}]");
        if (filter.MinBond.HasValue)
            criteria.Add($"MinBond: {filter.MinBond.Value}");
        if (filter.MaxBond.HasValue)
            criteria.Add($"MaxBond: {filter.MaxBond.Value}");
        if (filter.NpcTags != null && filter.NpcTags.Count > 0)
            criteria.Add($"NPC Tags: [{string.Join(", ", filter.NpcTags)}]");

        // Location filters
        if (filter.LocationProperties != null && filter.LocationProperties.Count > 0)
            criteria.Add($"Location Properties: [{string.Join(", ", filter.LocationProperties)}]");
        if (!string.IsNullOrEmpty(filter.DistrictId))
            criteria.Add($"District: {filter.DistrictId}");
        if (!string.IsNullOrEmpty(filter.RegionId))
            criteria.Add($"Region: {filter.RegionId}");

        // Route filters
        if (filter.TerrainTypes != null && filter.TerrainTypes.Count > 0)
            criteria.Add($"Terrain Types: [{string.Join(", ", filter.TerrainTypes)}]");
        if (filter.RouteTier.HasValue)
            criteria.Add($"Route Tier: {filter.RouteTier.Value}");
        if (filter.MinDangerRating.HasValue)
            criteria.Add($"Min Danger: {filter.MinDangerRating.Value}");
        if (filter.MaxDangerRating.HasValue)
            criteria.Add($"Max Danger: {filter.MaxDangerRating.Value}");

        // Player state filters
        if (filter.RequiredStates != null && filter.RequiredStates.Count > 0)
            criteria.Add($"Required States: [{string.Join(", ", filter.RequiredStates)}]");
        if (filter.ForbiddenStates != null && filter.ForbiddenStates.Count > 0)
            criteria.Add($"Forbidden States: [{string.Join(", ", filter.ForbiddenStates)}]");
        if (filter.RequiredAchievements != null && filter.RequiredAchievements.Count > 0)
            criteria.Add($"Required Achievements: [{string.Join(", ", filter.RequiredAchievements)}]");
        if (filter.ScaleRequirements != null && filter.ScaleRequirements.Count > 0)
            criteria.Add($"Scale Requirements: {filter.ScaleRequirements.Count} requirements");

        return string.Join("\n", criteria);
    }
}
