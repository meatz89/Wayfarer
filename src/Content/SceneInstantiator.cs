using System.Text.Json;
using Wayfarer.Content.DTOs;
using Wayfarer.GameState.Enums;
using Wayfarer.Models;
using Wayfarer.Services;

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
///
/// AI GENERATION INTEGRATION: SceneNarrativeService called at finalization time
/// Generates situation narratives from entity context when template has no hardcoded narrative
/// </summary>
public class SceneInstantiator
{
    private readonly GameWorld _gameWorld;
    private readonly SpawnConditionsEvaluator _spawnConditionsEvaluator;
    private readonly SceneNarrativeService _narrativeService;
    private readonly MarkerResolutionService _markerResolutionService;

    public SceneInstantiator(
        GameWorld gameWorld,
        SpawnConditionsEvaluator spawnConditionsEvaluator,
        SceneNarrativeService narrativeService,
        MarkerResolutionService markerResolutionService)
    {
        _gameWorld = gameWorld;
        _spawnConditionsEvaluator = spawnConditionsEvaluator ?? throw new ArgumentNullException(nameof(spawnConditionsEvaluator));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _markerResolutionService = markerResolutionService ?? throw new ArgumentNullException(nameof(markerResolutionService));
    }

    /// <summary>
    /// Create provisional Scene from template
    /// EAGER CREATION: Called when Situation instantiated for each Choice with SceneSpawnReward
    /// Scene has concrete placement and narrative (not finalized until Choice selected)
    /// </summary>
    /// <param name="sceneTemplate">SceneTemplate to instantiate from</param>
    /// <param name="spawnReward">Scene spawn reward containing placement relation and delay info</param>
    /// <param name="context">Spawn context with current entities for resolution</param>
    /// <returns>Provisional Scene with State = Provisional, stored in gameWorld.Scenes (unified collection)</returns>
    public Scene CreateProvisionalScene(SceneTemplate sceneTemplate, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        // PHASE 2.5: Check spawn conditions BEFORE creating scene
        // Evaluate if this scene is eligible to spawn based on player/world/entity state
        // If conditions fail, return null (scene not eligible - no provisional created)
        bool isEligible = _spawnConditionsEvaluator.EvaluateAll(
            sceneTemplate.SpawnConditions,
            context.Player,
            placementId: null // Placement not resolved yet - will check later if needed
        );

        if (!isEligible)
        {
            Console.WriteLine($"[SceneInstantiator] Scene '{sceneTemplate.Id}' REJECTED by spawn conditions");
            return null; // Scene not eligible - don't create provisional
        }

        // Generate unique Scene ID
        string sceneId = $"scene_{sceneTemplate.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        // Determine concrete placement based on PlacementRelation from spawnReward
        (PlacementType placementType, string placementId) = ResolvePlacement(sceneTemplate, spawnReward, context);

        // SHALLOW PROVISIONAL: Calculate metadata from template WITHOUT instantiating Situations
        int situationCount = sceneTemplate.SituationTemplates?.Count ?? 0;
        string estimatedDifficulty = CalculateEstimatedDifficulty(sceneTemplate);

        // Calculate expiration day from template
        // null = no expiration (scene lasts until completed)
        // value = CurrentDay + ExpirationDays (absolute day number when scene expires)
        int? expiresOnDay = sceneTemplate.ExpirationDays.HasValue
            ? _gameWorld.CurrentDay + sceneTemplate.ExpirationDays.Value
            : null;

        // Create Scene instance from template (SHALLOW - no Situations yet)
        Scene scene = new Scene
        {
            Id = sceneId,
            TemplateId = sceneTemplate.Id,
            Template = sceneTemplate, // Composition - reference template for access
            PlacementType = placementType,
            PlacementId = placementId,
            PresentationMode = sceneTemplate.PresentationMode,
            ProgressionMode = sceneTemplate.ProgressionMode,
            IsForced = sceneTemplate.IsForced,
            State = SceneState.Provisional, // KEY: Provisional state
            SourceSituationId = context.CurrentSituation?.Id, // Track source for cleanup
            ExpiresOnDay = expiresOnDay, // Time-limited content (enforced in ProcessTimeAdvancement)
            Archetype = sceneTemplate.Archetype,
            DisplayName = sceneTemplate.DisplayNameTemplate, // Placeholders NOT replaced until finalization
            IntroNarrative = sceneTemplate.IntroNarrativeTemplate, // Placeholders NOT replaced until finalization
            SpawnRules = sceneTemplate.SpawnRules,
            SituationIds = new List<string>(), // EMPTY - Situations instantiated in FinalizeScene()
            SituationCount = situationCount, // Metadata for perfect information display
            EstimatedDifficulty = estimatedDifficulty, // Metadata for perfect information display
            CurrentSituationId = null // Will be set in FinalizeScene() after Situations created
        };

        // PHASE 1.4: Store in unified Scenes collection (filtered by State property)
        _gameWorld.Scenes.Add(scene);

        Console.WriteLine($"[SceneInstantiator] Created Scene '{scene.Id}'");
        Console.WriteLine($"  State: {scene.State}, Placement: {scene.PlacementType}/{scene.PlacementId}");
        Console.WriteLine($"  Presentation: {scene.PresentationMode}, Situations: {scene.SituationIds.Count}");

        return scene;
    }

    /// <summary>
    /// Finalize provisional Scene - instantiate Situations, replace placeholders, activate
    /// Called when player SELECTS Choice that spawned this provisional Scene
    /// CRITICAL CHANGE: This is where Situations are instantiated FOR THE FIRST TIME
    /// Provisional scenes are SHALLOW metadata only - full instantiation happens here
    /// </summary>
    /// <param name="sceneId">Provisional Scene ID to finalize</param>
    /// <param name="context">Spawn context for placeholder replacement</param>
    /// <returns>Finalized Scene with dependent resource specs for orchestrator to load</returns>
    public (Scene scene, DependentResourceSpecs dependentSpecs) FinalizeScene(string sceneId, SceneSpawnContext context)
    {
        // PHASE 1.4: Get provisional Scene from unified collection via LINQ
        Scene scene = _gameWorld.Scenes.FirstOrDefault(s => s.Id == sceneId && s.State == SceneState.Provisional);

        if (scene == null)
        {
            throw new InvalidOperationException($"Provisional Scene '{sceneId}' not found in gameWorld.Scenes");
        }

        // SELF-CONTAINED PATTERN: Generate dependent resource specs (NOT load them)
        // Returns specs to orchestrator who will create JSON files and load via PackageLoader
        // Must happen BEFORE situation instantiation so marker references can be resolved
        DependentResourceSpecs dependentSpecs = DependentResourceSpecs.Empty;
        if (scene.Template.DependentLocations.Any() || scene.Template.DependentItems.Any())
        {
            dependentSpecs = GenerateDependentResourceSpecs(scene, context);

            // Track created resource IDs in scene (for marker resolution)
            scene.CreatedLocationIds.AddRange(dependentSpecs.CreatedLocationIds);
            scene.CreatedItemIds.AddRange(dependentSpecs.CreatedItemIds);
            scene.DependentPackageId = dependentSpecs.PackageId;

            // Build marker resolution map immediately after IDs are available
            // Markers can be resolved from IDs alone - actual entities don't need to be loaded yet
            BuildMarkerResolutionMap(scene);
        }

        // INSTANTIATE SITUATIONS FOR THE FIRST TIME (moved from CreateProvisionalScene)
        // This is the key change - provisional scenes are shallow, finalization creates full object graph
        foreach (SituationTemplate sitTemplate in scene.Template.SituationTemplates)
        {
            Situation situation = InstantiateSituation(sitTemplate, scene, context);
            // HIGHLANDER Pattern A: Add to GameWorld.Situations on finalization only
            _gameWorld.Situations.Add(situation);
            // Track ID in Scene
            scene.SituationIds.Add(situation.Id);

            // MARKER RESOLUTION: Resolve markers in situation template properties
            // Self-contained scenes use markers like "generated:private_room" that must resolve to actual IDs
            // Resolved IDs stored in Situation properties, NOT template (templates are immutable/shared)
            if (scene.MarkerResolutionMap.Count > 0)
            {
                situation.ResolvedRequiredLocationId = _markerResolutionService.ResolveMarker(sitTemplate.RequiredLocationId, scene.MarkerResolutionMap);
                situation.ResolvedRequiredNpcId = _markerResolutionService.ResolveMarker(sitTemplate.RequiredNpcId, scene.MarkerResolutionMap);

                // Resolve NavigationPayload destination if present
                if (situation.NavigationPayload != null)
                {
                    situation.NavigationPayload.DestinationId = _markerResolutionService.ResolveMarker(situation.NavigationPayload.DestinationId, scene.MarkerResolutionMap);
                }
            }

            // AI GENERATION: Generate narrative if template has no hardcoded narrative
            // Check: situation.Description null AND situation.NarrativeHints present
            // This pattern enables procedural content with archetype-driven generation
            if (string.IsNullOrEmpty(situation.Description) && situation.NarrativeHints != null)
            {
                try
                {
                    // Build entity context for AI generation
                    ScenePromptContext promptContext = BuildScenePromptContext(scene, context);

                    // Generate narrative from entity context
                    string generatedNarrative = _narrativeService.GenerateSituationNarrative(promptContext, situation.NarrativeHints);

                    // Set generated narrative
                    situation.Description = generatedNarrative;

                    Console.WriteLine($"[SceneInstantiator] Generated narrative for Situation '{situation.Id}'");
                }
                catch (Exception ex)
                {
                    // Fallback on generation failure
                    Console.WriteLine($"[SceneInstantiator] Narrative generation failed for Situation '{situation.Id}': {ex.Message}");
                    situation.Description = "A situation unfolds before you."; // Ultimate fallback
                }
            }

            // PLACEHOLDER REPLACEMENT: Situation descriptions
            // Happens AFTER AI generation so generated text can contain placeholders
            situation.Description = PlaceholderReplacer.ReplaceAll(situation.Description, context, _gameWorld);
        }

        // PLAYABILITY VALIDATION: Scene must have at least one Situation after instantiation
        if (!scene.SituationIds.Any())
            throw new InvalidOperationException($"Scene '{scene.Id}' (template '{scene.Template.Id}') has no SituationIds after finalization - player cannot interact!");

        // Set CurrentSituationId to first Situation ID
        scene.CurrentSituationId = scene.SituationIds.First();

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

        // PHASE 1.4: Change state to Active (no collection movement needed - unified storage)
        scene.State = SceneState.Active;

        return (scene, dependentSpecs);
    }

    /// <summary>
    /// Delete provisional Scene - cleanup when Choice NOT selected
    /// Called when player selects different Choice in Situation
    /// All unselected Choices' provisional Scenes are deleted
    /// SHALLOW ARCHITECTURE: Only removes Scene from ProvisionalScenes
    /// No Situation cleanup needed (Situations only instantiated in FinalizeScene)
    /// </summary>
    /// <param name="sceneId">Provisional Scene ID to delete</param>
    public void DeleteProvisionalScene(string sceneId)
    {
        // PHASE 1.4: Find and remove from unified Scenes collection via LINQ
        Scene provisionalScene = _gameWorld.Scenes.FirstOrDefault(s => s.Id == sceneId && s.State == SceneState.Provisional);

        if (provisionalScene != null)
        {
            _gameWorld.Scenes.Remove(provisionalScene);
        }
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Calculate estimated difficulty from SceneTemplate metadata
    /// Used for provisional Scene perfect information display
    /// Maps SceneTemplate.Tier to difficulty string
    /// Does NOT instantiate Situations (metadata only)
    /// </summary>
    private string CalculateEstimatedDifficulty(SceneTemplate template)
    {
        return template.Tier switch
        {
            0 => "Safety Net",
            1 => "Low",
            2 => "Standard",
            3 => "High",
            4 => "Climactic",
            _ => "Standard" // Default fallback
        };
    }

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
                return (PlacementType.Location, context.CurrentLocation.Id); // Location.Id (source of truth)

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
    // If needed in future, add to enum and implement here with proper Location → Location mapping

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
            Name = template.Name,  // Copy display name from template
            TemplateId = template.Id,
            Template = template,  // CRITICAL: Store template for lazy action instantiation
            Type = template.Type,  // Copy semantic type (Normal vs Crisis) from template
            Description = template.NarrativeTemplate,
            NarrativeHints = template.NarrativeHints,
            InstantiationState = InstantiationState.Deferred,  // START deferred - actions created at query time
            IsAutoAdvance = (template.ChoiceTemplates == null || template.ChoiceTemplates.Count == 0)
                             && template.AutoProgressRewards != null,  // Detect per-situation auto-advance

            // PLACEMENT INHERITANCE: ParentScene reference enables GetPlacementId() queries
            // Scene is single source of truth for placement (HIGHLANDER Pattern C)
            // Situation queries placement via GetPlacementId(PlacementType) helper
            ParentScene = parentScene
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
    /// PHASE 3: Collects ALL matching NPCs, then applies SelectionStrategy to choose ONE
    /// Returns selected NPC ID, or null if no matches
    /// </summary>
    private string FindMatchingNPC(PlacementFilter filter, Player player)
    {
        // Collect ALL matching NPCs
        List<NPC> matchingNPCs = _gameWorld.NPCs.Where(npc =>
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
        }).ToList();

        // No matches found
        if (matchingNPCs.Count == 0)
            return null;

        // Apply selection strategy to choose ONE from multiple matches
        NPC selectedNPC = ApplySelectionStrategyNPC(matchingNPCs, filter.SelectionStrategy, player);
        return selectedNPC?.ID;
    }

    /// <summary>
    /// Find Location matching PlacementFilter criteria
    /// PHASE 3: Collects ALL matching Locations, then applies SelectionStrategy to choose ONE
    /// Returns selected Location ID, or null if no matches
    /// </summary>
    private string FindMatchingLocation(PlacementFilter filter, Player player)
    {
        // Collect ALL matching Locations
        List<Location> matchingLocations = _gameWorld.Locations.Where(loc =>
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
        }).ToList();

        // No matches found
        if (matchingLocations.Count == 0)
            return null;

        // Apply selection strategy to choose ONE from multiple matches
        Location selectedLocation = ApplySelectionStrategyLocation(matchingLocations, filter.SelectionStrategy, player);
        return selectedLocation?.Id;
    }

    /// <summary>
    /// Find Route matching PlacementFilter criteria
    /// Returns first Route ID matching ALL filter criteria, or null if no match
    /// Filters by terrain type, tier, and danger rating
    /// </summary>
    private string FindMatchingRoute(PlacementFilter filter, Player player)
    {
        RouteOption matchingRoute = _gameWorld.Routes.FirstOrDefault(route =>
        {
            // Check terrain types (dominant terrain from TerrainCategories)
            if (filter.TerrainTypes != null && filter.TerrainTypes.Count > 0)
            {
                string dominantTerrain = route.GetDominantTerrainType();
                if (!filter.TerrainTypes.Contains(dominantTerrain))
                    return false;
            }

            // Check route tier (calculated from DangerRating)
            if (filter.RouteTier.HasValue)
            {
                if (route.Tier != filter.RouteTier.Value)
                    return false;
            }

            // Check danger rating range (0-100 scale)
            if (filter.MinDangerRating.HasValue && route.DangerRating < filter.MinDangerRating.Value)
                return false;
            if (filter.MaxDangerRating.HasValue && route.DangerRating > filter.MaxDangerRating.Value)
                return false;

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

    // ==================== PLACEMENT SELECTION STRATEGIES ====================

    /// <summary>
    /// Apply selection strategy to choose ONE NPC from multiple matching candidates
    /// PHASE 3: Implements 4 strategies (Closest, HighestBond, LeastRecent, WeightedRandom)
    /// </summary>
    private NPC ApplySelectionStrategyNPC(List<NPC> candidates, PlacementSelectionStrategy strategy, Player player)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        if (candidates.Count == 1)
            return candidates[0]; // Only one candidate, return it

        return strategy switch
        {
            PlacementSelectionStrategy.Closest => SelectClosestNPC(candidates, player),
            PlacementSelectionStrategy.HighestBond => SelectHighestBondNPC(candidates),
            PlacementSelectionStrategy.LeastRecent => SelectLeastRecentNPC(candidates, player),
            PlacementSelectionStrategy.WeightedRandom => SelectWeightedRandomNPC(candidates),
            _ => candidates[0] // Fallback to first match
        };
    }

    /// <summary>
    /// Apply selection strategy to choose ONE Location from multiple matching candidates
    /// PHASE 3: Implements 4 strategies (Closest works, others fall back to Random)
    /// </summary>
    private Location ApplySelectionStrategyLocation(List<Location> candidates, PlacementSelectionStrategy strategy, Player player)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        if (candidates.Count == 1)
            return candidates[0]; // Only one candidate, return it

        return strategy switch
        {
            PlacementSelectionStrategy.Closest => SelectClosestLocation(candidates, player),
            PlacementSelectionStrategy.HighestBond => SelectWeightedRandomLocation(candidates), // N/A for locations
            PlacementSelectionStrategy.LeastRecent => SelectLeastRecentLocation(candidates, player),
            PlacementSelectionStrategy.WeightedRandom => SelectWeightedRandomLocation(candidates),
            _ => candidates[0] // Fallback to first match
        };
    }

    /// <summary>
    /// Select NPC closest to player's current position using hex grid distance
    /// </summary>
    private NPC SelectClosestNPC(List<NPC> candidates, Player player)
    {
        NPC closest = null;
        int minDistance = int.MaxValue;

        foreach (NPC npc in candidates)
        {
            if (npc.Location?.HexPosition == null)
                continue; // NPC has no position, skip

            int distance = player.CurrentPosition.DistanceTo(npc.Location.HexPosition.Value);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = npc;
            }
        }

        return closest ?? candidates[0]; // Fallback to first if no positions
    }

    /// <summary>
    /// Select Location closest to player's current position using hex grid distance
    /// </summary>
    private Location SelectClosestLocation(List<Location> candidates, Player player)
    {
        Location closest = null;
        int minDistance = int.MaxValue;

        foreach (Location location in candidates)
        {
            if (location.HexPosition == null)
                continue; // Location has no position, skip

            int distance = player.CurrentPosition.DistanceTo(location.HexPosition.Value);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = location;
            }
        }

        return closest ?? candidates[0]; // Fallback to first if no positions
    }

    /// <summary>
    /// Select NPC with highest bond strength
    /// Good for "trusted ally" or "close friend" scenarios
    /// </summary>
    private NPC SelectHighestBondNPC(List<NPC> candidates)
    {
        return candidates.OrderByDescending(npc => npc.BondStrength).First();
    }

    /// <summary>
    /// Select NPC least recently interacted with for content variety
    /// Uses Player.NPCInteractions timestamp data to find oldest interaction
    /// Falls back to WeightedRandom if no interaction history exists
    /// </summary>
    private NPC SelectLeastRecentNPC(List<NPC> candidates, Player player)
    {
        // Create interaction lookup dictionary for fast access
        // ONE record per NPC (update-in-place pattern) - no GroupBy needed
        Dictionary<string, NPCInteractionRecord> interactionLookup = player.NPCInteractions
            .ToDictionary(interaction => interaction.NPCId);

        // Find candidate with oldest interaction (or never interacted)
        NPC leastRecentNPC = null;
        long oldestTimestamp = long.MaxValue;

        foreach (NPC candidate in candidates)
        {
            if (!interactionLookup.ContainsKey(candidate.ID))
            {
                // Never interacted with this NPC - prioritize these
                return candidate;
            }

            NPCInteractionRecord record = interactionLookup[candidate.ID];
            long timestamp = CalculateTimestamp(record.LastInteractionDay, record.LastInteractionTimeBlock, record.LastInteractionSegment);

            if (timestamp < oldestTimestamp)
            {
                oldestTimestamp = timestamp;
                leastRecentNPC = candidate;
            }
        }

        // If all candidates have been interacted with, return least recent
        // If somehow nothing found, fall back to random
        return leastRecentNPC ?? SelectWeightedRandomNPC(candidates);
    }

    /// <summary>
    /// Select random NPC from candidates using RNG for unpredictable variety
    /// </summary>
    private NPC SelectWeightedRandomNPC(List<NPC> candidates)
    {
        Random random = new Random();
        int index = random.Next(candidates.Count);
        return candidates[index];
    }

    /// <summary>
    /// Select random Location from candidates using RNG for unpredictable variety
    /// </summary>
    private Location SelectWeightedRandomLocation(List<Location> candidates)
    {
        Random random = new Random();
        int index = random.Next(candidates.Count);
        return candidates[index];
    }

    /// <summary>
    /// Select Location least recently visited for content variety
    /// Uses Player.LocationVisits timestamp data to find oldest visit
    /// Falls back to WeightedRandom if no visit history exists
    /// </summary>
    private Location SelectLeastRecentLocation(List<Location> candidates, Player player)
    {
        // Create visit timestamp lookup dictionary for fast access
        // ONE record per location (update-in-place pattern) - no GroupBy needed
        Dictionary<string, LocationVisitRecord> visitLookup = player.LocationVisits
            .ToDictionary(visit => visit.LocationId);

        // Find candidate with oldest visit (or never visited)
        Location leastRecentLocation = null;
        long oldestTimestamp = long.MaxValue;

        foreach (Location candidate in candidates)
        {
            if (!visitLookup.ContainsKey(candidate.Id))
            {
                // Never visited this location - prioritize these
                return candidate;
            }

            LocationVisitRecord record = visitLookup[candidate.Id];
            long timestamp = CalculateTimestamp(record.LastVisitDay, record.LastVisitTimeBlock, record.LastVisitSegment);

            if (timestamp < oldestTimestamp)
            {
                oldestTimestamp = timestamp;
                leastRecentLocation = candidate;
            }
        }

        // If all candidates have been visited, return least recent
        // If somehow nothing found, fall back to random
        return leastRecentLocation ?? SelectWeightedRandomLocation(candidates);
    }

    /// <summary>
    /// Calculate timestamp from game time components for chronological comparison
    /// Formula: (Day * 16) + (TimeBlockValue * 4) + Segment
    /// Assumes 4 time blocks per day (Morning, Midday, Afternoon, Evening), 4 segments per block = 16 segments/day
    /// </summary>
    private long CalculateTimestamp(int day, TimeBlocks timeBlock, int segment)
    {
        int timeBlockValue = timeBlock switch
        {
            TimeBlocks.Morning => 0,
            TimeBlocks.Midday => 1,
            TimeBlocks.Afternoon => 2,
            TimeBlocks.Evening => 3,
            _ => 0
        };

        return (day * 16) + (timeBlockValue * 4) + segment;
    }

    /// <summary>
    /// Build ScenePromptContext for AI narrative generation from Scene and spawn context.
    /// Bundles entity objects (NPC, Location, Route) with complete properties for rich context.
    /// Called at finalization when concrete placement is resolved.
    /// </summary>
    private ScenePromptContext BuildScenePromptContext(Scene scene, SceneSpawnContext context)
    {
        ScenePromptContext promptContext = new ScenePromptContext
        {
            Player = context.Player,
            ArchetypeId = scene.Template.Archetype.ToString(),
            Tier = scene.Template.Tier,
            SceneDisplayName = scene.DisplayName,
            CurrentTimeBlock = _gameWorld.CurrentTimeBlock,
            CurrentWeather = _gameWorld.CurrentWeather.ToString().ToLower(),
            CurrentDay = _gameWorld.CurrentDay
        };

        // Resolve entity references based on placement type
        switch (scene.PlacementType)
        {
            case PlacementType.NPC:
                promptContext.NPC = _gameWorld.NPCs.FirstOrDefault(n => n.ID == scene.PlacementId);
                if (promptContext.NPC != null)
                {
                    promptContext.NPCBondLevel = promptContext.NPC.BondStrength;
                    // Also populate location if NPC has one
                    promptContext.Location = promptContext.NPC.Location;
                    // TODO: Populate PriorChoicesWithNPC from Player.ChoiceHistory when implemented
                }
                break;

            case PlacementType.Location:
                promptContext.Location = _gameWorld.Locations.FirstOrDefault(l => l.Id == scene.PlacementId);
                break;

            case PlacementType.Route:
                promptContext.Route = _gameWorld.Routes.FirstOrDefault(r => r.Id == scene.PlacementId);
                break;
        }

        return promptContext;
    }

    // ==================== SELF-CONTAINED SCENE PACKAGE GENERATION ====================

    /// <summary>
    /// Generate dependent resource SPECS for self-contained scene
    /// Returns specs to orchestrator who creates JSON files and loads via PackageLoader
    /// Does NOT load resources itself (pure generation, no infrastructure)
    /// Called from FinalizeScene BEFORE situation instantiation
    /// </summary>
    private DependentResourceSpecs GenerateDependentResourceSpecs(Scene scene, SceneSpawnContext context)
    {
        // Build lists of DTOs
        List<LocationDTO> locationDtos = new List<LocationDTO>();
        List<ItemDTO> itemDtos = new List<ItemDTO>();

        // Generate LocationDTOs from specifications
        foreach (DependentLocationSpec spec in scene.Template.DependentLocations)
        {
            LocationDTO locationDto = BuildLocationDTO(spec, scene, context);
            locationDtos.Add(locationDto);
        }

        // Generate ItemDTOs from specifications
        foreach (DependentItemSpec spec in scene.Template.DependentItems)
        {
            ItemDTO itemDto = BuildItemDTO(spec, scene, context, locationDtos);
            itemDtos.Add(itemDto);
        }

        // Create Package object
        string packageId = $"scene_{scene.Id}_dep";
        Package package = new Package
        {
            PackageId = packageId,
            Metadata = new PackageMetadata
            {
                Name = $"Scene {scene.Id} Dependent Resources",
                Author = "Scene System",
                Version = "1.0.0"
            },
            Content = new PackageContent
            {
                Locations = locationDtos,
                Items = itemDtos
            }
        };

        // Serialize to JSON for orchestrator
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(package, jsonOptions);

        // Build list of created IDs for scene tracking
        List<string> createdLocationIds = locationDtos.Select(dto => dto.Id).ToList();
        List<string> createdItemIds = itemDtos.Select(dto => dto.Id).ToList();

        // Build list of items to add to inventory (orchestrator handles after loading)
        List<string> itemsToAddToInventory = new List<string>();
        foreach (DependentItemSpec spec in scene.Template.DependentItems)
        {
            if (spec.AddToInventoryOnCreation)
            {
                string itemId = $"{scene.Id}_{spec.TemplateId}";
                itemsToAddToInventory.Add(itemId);
            }
        }

        // Return specs to orchestrator (NO LOADING HERE)
        return new DependentResourceSpecs
        {
            Locations = locationDtos,
            Items = itemDtos,
            PackageId = packageId,
            PackageJson = json,
            CreatedLocationIds = createdLocationIds,
            CreatedItemIds = createdItemIds,
            ItemsToAddToInventory = itemsToAddToInventory
        };
    }

    /// <summary>
    /// Build LocationDTO from DependentLocationSpec
    /// Replaces tokens, determines venue, finds hex placement
    /// </summary>
    private LocationDTO BuildLocationDTO(DependentLocationSpec spec, Scene scene, SceneSpawnContext context)
    {
        // Generate unique ID
        string locationId = $"{scene.Id}_{spec.TemplateId}";

        // Replace tokens in name and description
        string locationName = PlaceholderReplacer.ReplaceAll(spec.NamePattern, context, _gameWorld);
        string locationDescription = PlaceholderReplacer.ReplaceAll(spec.DescriptionPattern, context, _gameWorld);

        // Determine venue ID
        string venueId = DetermineVenueId(spec.VenueIdSource, context);

        // Find hex placement (CRITICAL: ALL locations must have hex positions)
        Location baseLocation = context.CurrentLocation;
        if (baseLocation == null)
            throw new InvalidOperationException($"Cannot place dependent location '{locationId}' - context.CurrentLocation is null");

        AxialCoordinates? hexPosition = FindAdjacentHex(baseLocation, spec.HexPlacement);
        if (!hexPosition.HasValue)
            throw new InvalidOperationException($"Failed to find hex position for dependent location '{locationId}' using strategy '{spec.HexPlacement}'");

        Console.WriteLine($"[DependentLocation] Placed '{locationId}' at hex ({hexPosition.Value.Q}, {hexPosition.Value.R}) adjacent to base location '{baseLocation.Id}'");

        // Build LocationDTO
        LocationDTO dto = new LocationDTO
        {
            Id = locationId,
            Name = locationName,
            Description = locationDescription,
            VenueId = venueId,
            Q = hexPosition.Value.Q,
            R = hexPosition.Value.R,
            Type = "Room", // Default type for generated locations
            InitialState = spec.IsLockedInitially ? "Locked" : "Available",
            CanInvestigate = spec.CanInvestigate,
            CanWork = false, // Generated locations don't support work by default
            WorkType = "",
            WorkPay = 0
        };

        // Map properties
        if (spec.Properties != null && spec.Properties.Any())
        {
            dto.DomainTags = spec.Properties;
        }

        return dto;
    }

    /// <summary>
    /// Build ItemDTO from DependentItemSpec
    /// Replaces tokens, maps categories, handles inventory placement
    /// </summary>
    private ItemDTO BuildItemDTO(DependentItemSpec spec, Scene scene, SceneSpawnContext context, List<LocationDTO> createdLocations)
    {
        // Generate unique ID
        string itemId = $"{scene.Id}_{spec.TemplateId}";

        // Replace tokens in name and description
        string itemName = PlaceholderReplacer.ReplaceAll(spec.NamePattern, context, _gameWorld);
        string itemDescription = PlaceholderReplacer.ReplaceAll(spec.DescriptionPattern, context, _gameWorld);

        // Build ItemDTO
        ItemDTO dto = new ItemDTO
        {
            Id = itemId,
            Name = itemName,
            Description = itemDescription,
            BuyPrice = spec.BuyPrice,
            SellPrice = spec.SellPrice,
            InventorySlots = spec.Weight
        };

        // Map categories
        if (spec.Categories != null && spec.Categories.Any())
        {
            dto.Categories = spec.Categories.Select(c => c.ToString()).ToList();
        }

        // Handle placement
        if (spec.AddToInventoryOnCreation)
        {
            // Item will be added to player inventory AFTER parsing
            // Store this intent in a way the parser can understand
            // For now, we'll handle this after package loading
        }

        return dto;
    }

    /// <summary>
    /// Determine venue ID based on VenueIdSource enum
    /// </summary>
    private string DetermineVenueId(VenueIdSource source, SceneSpawnContext context)
    {
        switch (source)
        {
            case VenueIdSource.SameAsBase:
                if (context.CurrentLocation == null)
                    throw new InvalidOperationException("VenueIdSource.SameAsBase requires CurrentLocation in context");
                return context.CurrentLocation.VenueId; // Location.VenueId (source of truth)

            case VenueIdSource.GenerateNew:
                throw new NotImplementedException("VenueIdSource.GenerateNew not yet implemented");

            default:
                throw new InvalidOperationException($"Unknown VenueIdSource: {source}");
        }
    }

    /// <summary>
    /// Find adjacent hex position for new location
    /// Implements HexPlacementStrategy.Adjacent and SameVenue logic (both find unoccupied adjacent hex)
    /// </summary>
    private AxialCoordinates? FindAdjacentHex(Location baseLocation, HexPlacementStrategy strategy)
    {
        switch (strategy)
        {
            case HexPlacementStrategy.SameVenue:
            case HexPlacementStrategy.Adjacent:
                // Both strategies find unoccupied adjacent hex
                // SameVenue = adjacent hex in SAME venue (intra-venue travel)
                // Adjacent = adjacent hex (may be different venue if crossing venue boundary)
                if (!baseLocation.HexPosition.HasValue)
                    throw new InvalidOperationException($"Base location '{baseLocation.Id}' has no HexPosition - cannot find adjacent hex");

                // Get neighbors from hex map
                HexMap hexMap = _gameWorld.WorldHexGrid;
                if (hexMap == null)
                    throw new InvalidOperationException("GameWorld.WorldHexGrid is null - cannot find adjacent hex");

                Hex baseHex = hexMap.GetHex(baseLocation.HexPosition.Value);
                if (baseHex == null)
                    throw new InvalidOperationException($"Base location hex position {baseLocation.HexPosition.Value} not found in HexMap");

                List<Hex> neighbors = hexMap.GetNeighbors(baseHex);

                // Find first unoccupied neighbor
                foreach (Hex neighborHex in neighbors)
                {
                    // Check if any location occupies this hex
                    bool hexOccupied = _gameWorld.Locations.Any(loc => loc.HexPosition.HasValue &&
                                                                       loc.HexPosition.Value.Equals(neighborHex.Coordinates));
                    if (!hexOccupied)
                    {
                        return neighborHex.Coordinates;
                    }
                }

                throw new InvalidOperationException($"No unoccupied adjacent hexes found for location '{baseLocation.Id}'");

            case HexPlacementStrategy.Distance:
            case HexPlacementStrategy.Random:
                throw new NotImplementedException($"HexPlacementStrategy.{strategy} not yet implemented");

            default:
                throw new InvalidOperationException($"Unknown HexPlacementStrategy: {strategy}");
        }
    }

    /// <summary>
    /// Build marker resolution map for self-contained scene using MarkerResolutionService
    /// Maps "generated:{templateId}" markers to actual created resource IDs
    /// Called after GenerateAndLoadDependentResources completes
    /// Enables situations/choices to reference generated resources via markers
    /// PUBLIC: Called by orchestrator after loading dependent resources
    /// </summary>
    public void BuildMarkerResolutionMap(Scene scene)
    {
        scene.MarkerResolutionMap = _markerResolutionService.BuildMarkerResolutionMap(scene);
        Console.WriteLine($"[SceneInstantiator] Built marker resolution map for scene '{scene.Id}' with {scene.MarkerResolutionMap.Count} entries");
    }
}
