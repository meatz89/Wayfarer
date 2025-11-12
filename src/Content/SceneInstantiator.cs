using System.Text.Json;
/// <summary>
/// Domain Service for generating Scene instance DTOs and JSON packages from SceneTemplates
///
/// HIGHLANDER COMPLIANCE: Generates DTOs/JSON only - NEVER creates entities directly
/// Entity creation MUST flow through: JSON → PackageLoader → SceneParser → Entity
///
/// RESPONSIBILITIES:
/// 1. GenerateScenePackageJson: Main entry point - generates complete package JSON
/// 2. Placement resolution: Evaluates PlacementFilters to find concrete placements
/// 3. DTO generation: Creates SceneDTO, SituationDTOs, ChoiceTemplateDTOs
/// 4. Dependent resources: Generates LocationDTO/ItemDTO specs for self-contained scenes
///
/// AI GENERATION INTEGRATION: SceneNarrativeService generates narratives during DTO generation
/// Placeholders replaced before serialization to JSON
/// </summary>
public class SceneInstantiator
{
    private const int SEGMENTS_PER_DAY = 16;
    private const int SEGMENTS_PER_TIME_BLOCK = 4;

    private readonly GameWorld _gameWorld;
    private readonly SpawnConditionsEvaluator _spawnConditionsEvaluator;
    private readonly SceneNarrativeService _narrativeService;
    private readonly MarkerResolutionService _markerResolutionService;
    private readonly VenueGeneratorService _venueGenerator;
    private readonly Random _random = new Random();

    public SceneInstantiator(
        GameWorld gameWorld,
        SpawnConditionsEvaluator spawnConditionsEvaluator,
        SceneNarrativeService narrativeService,
        MarkerResolutionService markerResolutionService,
        VenueGeneratorService venueGenerator)
    {
        _gameWorld = gameWorld;
        _spawnConditionsEvaluator = spawnConditionsEvaluator ?? throw new ArgumentNullException(nameof(spawnConditionsEvaluator));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _markerResolutionService = markerResolutionService ?? throw new ArgumentNullException(nameof(markerResolutionService));
        _venueGenerator = venueGenerator ?? throw new ArgumentNullException(nameof(venueGenerator));
    }

    /// <summary>
    /// Generate complete scene package JSON from template
    /// HIGHLANDER: Returns JSON string for ContentGenerationFacade to write, PackageLoaderFacade to load
    /// NEVER creates entities directly - only DTOs and JSON
    /// </summary>
    public string GenerateScenePackageJson(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        // Evaluate spawn conditions (same as old CreateProvisionalScene)
        bool isEligible = template.IsStarter || _spawnConditionsEvaluator.EvaluateAll(
            template.SpawnConditions,
            context.Player,
            placementId: null
        );

        if (!isEligible)
        {
            Console.WriteLine($"[SceneInstantiator] Scene '{template.Id}' REJECTED by spawn conditions");
            return null; // Scene not eligible - return null
        }

        // Resolve placement (categorical → concrete ID)
        PlacementResolution placement = ResolvePlacement(template, spawnReward, context);

        // Generate Scene DTO
        SceneDTO sceneDto = GenerateSceneDTO(template, placement, context);

        // Generate dependent resource DTOs (if self-contained scene)
        List<LocationDTO> dependentLocations = new List<LocationDTO>();
        List<ItemDTO> dependentItems = new List<ItemDTO>();
        Dictionary<string, string> markerResolutionMap = new Dictionary<string, string>();

        if (template.DependentLocations.Any() || template.DependentItems.Any())
        {
            // Generate dependent location DTOs
            foreach (DependentLocationSpec spec in template.DependentLocations)
            {
                LocationDTO locationDto = BuildLocationDTO(spec, sceneDto.Id, context);
                dependentLocations.Add(locationDto);

                // Build marker resolution map
                string marker = $"generated:{spec.TemplateId}";
                markerResolutionMap[marker] = locationDto.Id;
            }

            // Generate dependent item DTOs
            foreach (DependentItemSpec spec in template.DependentItems)
            {
                ItemDTO itemDto = BuildItemDTO(spec, sceneDto.Id, context, dependentLocations);
                dependentItems.Add(itemDto);

                string marker = $"generated:{spec.TemplateId}";
                markerResolutionMap[marker] = itemDto.Id;
            }

            // Store marker map in scene DTO
            sceneDto.MarkerResolutionMap = markerResolutionMap;
            sceneDto.CreatedLocationIds = dependentLocations.Select(dto => dto.Id).ToList();
            sceneDto.CreatedItemIds = dependentItems.Select(dto => dto.Id).ToList();
            sceneDto.DependentPackageId = $"scene_{sceneDto.Id}_dep";
        }

        // Generate Situation DTOs (CRITICAL: This replaces InstantiateSituation())
        List<SituationDTO> situationDtos = GenerateSituationDTOs(template, sceneDto, markerResolutionMap, context);
        sceneDto.Situations = situationDtos;

        // Build complete package
        string packageJson = BuildScenePackage(sceneDto, dependentLocations, dependentItems);

        Console.WriteLine($"[SceneInstantiator] Generated scene package '{sceneDto.Id}' with {situationDtos.Count} situations");

        return packageJson;
    }

    /// <summary>
    /// Generate SceneDTO from template with concrete placement
    /// Replaces placeholders in display name and intro narrative
    /// </summary>
    private SceneDTO GenerateSceneDTO(SceneTemplate template, PlacementResolution placement, SceneSpawnContext context)
    {
        // Generate unique Scene ID
        string sceneId = $"scene_{template.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

        // Calculate expiration day
        int? expiresOnDay = template.ExpirationDays.HasValue
            ? _gameWorld.CurrentDay + template.ExpirationDays.Value
            : null;

        // Replace placeholders in display name and intro narrative
        string displayName = PlaceholderReplacer.ReplaceAll(template.DisplayNameTemplate, context, _gameWorld);
        string introNarrative = PlaceholderReplacer.ReplaceAll(template.IntroNarrativeTemplate, context, _gameWorld);

        // Parse spawn rules DTO
        SituationSpawnRulesDTO spawnRulesDto = null;
        if (template.SpawnRules != null)
        {
            spawnRulesDto = new SituationSpawnRulesDTO
            {
                Pattern = template.SpawnRules.Pattern.ToString(),
                InitialSituationId = template.SpawnRules.InitialSituationId,
                Transitions = template.SpawnRules.Transitions?.Select(t => new SituationTransitionDTO
                {
                    SourceSituationId = t.SourceSituationId,
                    DestinationSituationId = t.DestinationSituationId,
                    Condition = t.Condition.ToString(),
                    SpecificChoiceId = t.SpecificChoiceId
                }).ToList()
            };
        }

        SceneDTO dto = new SceneDTO
        {
            Id = sceneId,
            TemplateId = template.Id,
            PlacementType = placement.PlacementType.ToString(),
            PlacementId = placement.PlacementId,
            State = "Active", // NEW: Scenes spawn directly as Active (no provisional state)
            ExpiresOnDay = expiresOnDay,
            Archetype = template.Archetype.ToString(),
            DisplayName = displayName,
            IntroNarrative = introNarrative,
            PresentationMode = template.PresentationMode.ToString(),
            ProgressionMode = template.ProgressionMode.ToString(),
            Category = template.Category.ToString(), // Copy from template for A-story tracking
            MainStorySequence = template.MainStorySequence, // Copy from template for A-story sequence tracking
            SpawnRules = spawnRulesDto,
            CurrentSituationId = null, // Will be set to first situation ID after situations generated
            SourceSituationId = context.CurrentSituation?.Id
        };

        return dto;
    }

    /// <summary>
    /// Generate SituationDTOs from template's SituationTemplates
    /// Replaces InstantiateSituation() - generates DTOs instead of entities
    /// </summary>
    private List<SituationDTO> GenerateSituationDTOs(
        SceneTemplate template,
        SceneDTO sceneDto,
        Dictionary<string, string> markerResolutionMap,
        SceneSpawnContext context)
    {
        List<SituationDTO> situationDtos = new List<SituationDTO>();

        foreach (SituationTemplate sitTemplate in template.SituationTemplates)
        {
            // Generate unique Situation ID
            string situationId = $"situation_{sitTemplate.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            // Resolve markers in required location/NPC IDs
            string resolvedLocationId = _markerResolutionService.ResolveMarker(sitTemplate.RequiredLocationId, markerResolutionMap);
            string resolvedNpcId = _markerResolutionService.ResolveMarker(sitTemplate.RequiredNpcId, markerResolutionMap);

            // Generate narrative (AI or template)
            string description = sitTemplate.NarrativeTemplate;
            if (string.IsNullOrEmpty(description) && sitTemplate.NarrativeHints != null)
            {
                try
                {
                    ScenePromptContext promptContext = BuildScenePromptContext(sceneDto, context);
                    description = _narrativeService.GenerateSituationNarrative(promptContext, sitTemplate.NarrativeHints);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SceneInstantiator] Narrative generation failed for Situation '{situationId}': {ex.Message}");
                    description = "A situation unfolds before you.";
                }
            }

            // Replace placeholders
            description = PlaceholderReplacer.ReplaceAll(description, context, _gameWorld);

            // Build Situation DTO from template
            // Scene-based situations use templates - most DTO properties are for standalone situations
            SituationDTO situationDto = new SituationDTO
            {
                Id = situationId,
                TemplateId = sitTemplate.Id,
                Name = sitTemplate.Name,
                Description = description,
                InteractionType = sitTemplate.Type.ToString(),
                PlacementLocationId = resolvedLocationId,
                PlacementNpcId = resolvedNpcId
            };

            // Copy narrative hints if present
            if (sitTemplate.NarrativeHints != null)
            {
                situationDto.NarrativeHints = new NarrativeHintsDTO
                {
                    Tone = sitTemplate.NarrativeHints.Tone,
                    Theme = sitTemplate.NarrativeHints.Theme,
                    Context = sitTemplate.NarrativeHints.Context,
                    Style = sitTemplate.NarrativeHints.Style
                };
            }

            situationDtos.Add(situationDto);
        }

        // Set CurrentSituationId to first situation
        if (situationDtos.Any())
        {
            sceneDto.CurrentSituationId = situationDtos.First().Id;
        }

        return situationDtos;
    }

    /// <summary>
    /// Build complete package JSON with scene, situations, and dependent resources
    /// </summary>
    private string BuildScenePackage(SceneDTO sceneDto, List<LocationDTO> dependentLocations, List<ItemDTO> dependentItems)
    {
        Package package = new Package
        {
            PackageId = $"scene_{sceneDto.Id}_package",
            Metadata = new PackageMetadata
            {
                Name = $"Scene {sceneDto.DisplayName}",
                Author = "Scene System",
                Version = "1.0.0"
            },
            Content = new PackageContent
            {
                Scenes = new List<SceneDTO> { sceneDto },
                Locations = dependentLocations,
                Items = dependentItems
            }
        };

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return JsonSerializer.Serialize(package, jsonOptions);
    }

    private PlacementResolution ResolvePlacement(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        PlacementRelation relation = spawnReward.PlacementRelation;

        switch (relation)
        {
            case PlacementRelation.SameLocation:
                if (context.CurrentLocation == null)
                    throw new InvalidOperationException("SameLocation placement requires CurrentLocation in context");
                return new PlacementResolution(PlacementType.Location, context.CurrentLocation.Id); // Location.Id (source of truth)

            case PlacementRelation.SameNPC:
                if (context.CurrentNPC == null)
                    throw new InvalidOperationException("SameNPC placement requires CurrentNPC in context");
                return new PlacementResolution(PlacementType.NPC, context.CurrentNPC.ID);

            case PlacementRelation.SameRoute:
                if (context.CurrentRoute == null)
                    throw new InvalidOperationException("SameRoute placement requires CurrentRoute in context");
                return new PlacementResolution(PlacementType.Route, context.CurrentRoute.Id);

            case PlacementRelation.SpecificLocation:
                if (string.IsNullOrEmpty(spawnReward.SpecificPlacementId))
                    throw new InvalidOperationException("SpecificLocation placement requires SpecificPlacementId in spawnReward");
                return new PlacementResolution(PlacementType.Location, spawnReward.SpecificPlacementId);

            case PlacementRelation.SpecificNPC:
                if (string.IsNullOrEmpty(spawnReward.SpecificPlacementId))
                    throw new InvalidOperationException("SpecificNPC placement requires SpecificPlacementId in spawnReward");
                return new PlacementResolution(PlacementType.NPC, spawnReward.SpecificPlacementId);

            case PlacementRelation.SpecificRoute:
                if (string.IsNullOrEmpty(spawnReward.SpecificPlacementId))
                    throw new InvalidOperationException("SpecificRoute placement requires SpecificPlacementId in spawnReward");
                return new PlacementResolution(PlacementType.Route, spawnReward.SpecificPlacementId);

            case PlacementRelation.Generic:
                // Evaluate PlacementFilter from SceneTemplate to find matching entity
                if (template.PlacementFilter == null)
                    throw new InvalidOperationException($"Generic placement requires PlacementFilter on SceneTemplate '{template.Id}'");

                string placementId = EvaluatePlacementFilter(template.PlacementFilter, context);
                if (string.IsNullOrEmpty(placementId))
                {
                    throw new InvalidOperationException(
                        $"PLACEMENT FILTER FAILED - No matching entity found\n" +
                        $"SceneTemplate: {template.Id}\n" +
                        $"Category: {template.Category}\n" +
                        $"MainStorySequence: {template.MainStorySequence}\n" +
                        $"\nFILTER CRITERIA:\n{FormatFilterCriteria(template.PlacementFilter)}\n" +
                        $"\nAVAILABLE ENTITIES:\n{FormatAvailableEntities(template.PlacementFilter.PlacementType)}\n" +
                        $"\nCONTEXT:\n{FormatSpawnContext(context)}\n" +
                        $"\nDIAGNOSTIC: This error indicates procedurally generated scene cannot find suitable placement.\n" +
                        $"For A-story scenes, this creates SOFT LOCK (player completed previous scene, expects next scene, gets nothing).\n" +
                        $"Consider: Relaxing PlacementFilter constraints OR ensuring required entities exist in GameWorld."
                    );
                }

                PlacementType placementType = template.PlacementFilter.PlacementType;
                return new PlacementResolution(placementType, placementId);

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
    /// PHASE 5: Complete filtering including district, tags, and location properties
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

            // Check district (accessed via Location.Venue.District)
            if (!string.IsNullOrEmpty(filter.DistrictId))
            {
                if (loc.Venue == null || loc.Venue.District != filter.DistrictId)
                    return false;
            }

            // Check location tags (uses DomainTags property)
            if (filter.LocationTags != null && filter.LocationTags.Count > 0)
            {
                // Location must have ALL specified tags
                if (!filter.LocationTags.All(tag => loc.DomainTags.Contains(tag)))
                    return false;
            }

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

    /// <summary>
    /// Format available entities for diagnostic error messages
    /// Shows what entities exist in GameWorld for given PlacementType
    /// </summary>
    private string FormatAvailableEntities(PlacementType placementType)
    {
        switch (placementType)
        {
            case PlacementType.Location:
                int locationCount = _gameWorld.Locations.Count;
                List<string> locationSummaries = _gameWorld.Locations
                    .Take(10) // Show first 10
                    .Select(loc => $"  - {loc.Id} ({loc.Name}): Properties={string.Join(",", loc.LocationProperties)}")
                    .ToList();
                if (locationCount > 10)
                    locationSummaries.Add($"  ... and {locationCount - 10} more locations");
                return $"Total Locations: {locationCount}\n{string.Join("\n", locationSummaries)}";

            case PlacementType.NPC:
                int npcCount = _gameWorld.NPCs.Count;
                List<string> npcSummaries = _gameWorld.NPCs
                    .Take(10)
                    .Select(npc => $"  - {npc.ID} ({npc.Name}): Personality={npc.PersonalityType}, Bond={npc.BondStrength}")
                    .ToList();
                if (npcCount > 10)
                    npcSummaries.Add($"  ... and {npcCount - 10} more NPCs");
                return $"Total NPCs: {npcCount}\n{string.Join("\n", npcSummaries)}";

            case PlacementType.Route:
                int routeCount = _gameWorld.Routes.Count;
                List<string> routeSummaries = _gameWorld.Routes
                    .Take(10)
                    .Select(route => $"  - {route.Id}: Tier={route.Tier}, Danger={route.DangerRating}")
                    .ToList();
                if (routeCount > 10)
                    routeSummaries.Add($"  ... and {routeCount - 10} more routes");
                return $"Total Routes: {routeCount}\n{string.Join("\n", routeSummaries)}";

            default:
                return $"Unknown PlacementType: {placementType}";
        }
    }

    /// <summary>
    /// Format spawn context for diagnostic error messages
    /// Shows player state and current context when filter evaluation failed
    /// </summary>
    private string FormatSpawnContext(SceneSpawnContext context)
    {
        List<string> contextInfo = new List<string>();

        contextInfo.Add($"Player: {context.Player?.Name ?? "null"}");
        contextInfo.Add($"Player Position: {context.Player?.CurrentPosition}");
        contextInfo.Add($"Player Resolve: {context.Player?.Resolve}");
        contextInfo.Add($"Player States: {(context.Player?.ActiveStates.Count ?? 0)} active");

        if (context.CurrentLocation != null)
            contextInfo.Add($"Current Location: {context.CurrentLocation.Id} ({context.CurrentLocation.Name})");
        else
            contextInfo.Add($"Current Location: null");

        if (context.CurrentNPC != null)
            contextInfo.Add($"Current NPC: {context.CurrentNPC.ID} ({context.CurrentNPC.Name})");
        else
            contextInfo.Add($"Current NPC: null");

        if (context.CurrentRoute != null)
            contextInfo.Add($"Current Route: {context.CurrentRoute.Id}");
        else
            contextInfo.Add($"Current Route: null");

        return string.Join("\n", contextInfo);
    }

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
        int index = _random.Next(candidates.Count);
        return candidates[index];
    }

    /// <summary>
    /// Select random Location from candidates using RNG for unpredictable variety
    /// </summary>
    private Location SelectWeightedRandomLocation(List<Location> candidates)
    {
        int index = _random.Next(candidates.Count);
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

        return (day * SEGMENTS_PER_DAY) + (timeBlockValue * SEGMENTS_PER_TIME_BLOCK) + segment;
    }

    /// <summary>
    /// Build ScenePromptContext for AI narrative generation from SceneDTO and spawn context
    /// Bundles entity objects (NPC, Location, Route) with complete properties for rich context
    /// Called during DTO generation when concrete placement is resolved
    /// </summary>
    private ScenePromptContext BuildScenePromptContext(SceneDTO sceneDto, SceneSpawnContext context)
    {
        // Parse PlacementType from string
        if (!Enum.TryParse<PlacementType>(sceneDto.PlacementType, true, out PlacementType placementType))
        {
            throw new InvalidOperationException($"Invalid PlacementType: {sceneDto.PlacementType}");
        }

        // Get template for archetype/tier
        SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == sceneDto.TemplateId);
        if (template == null)
        {
            throw new InvalidOperationException($"SceneTemplate '{sceneDto.TemplateId}' not found");
        }

        ScenePromptContext promptContext = new ScenePromptContext
        {
            Player = context.Player,
            ArchetypeId = template.Archetype.ToString(),
            Tier = template.Tier,
            SceneDisplayName = sceneDto.DisplayName,
            CurrentTimeBlock = _gameWorld.CurrentTimeBlock,
            CurrentWeather = _gameWorld.CurrentWeather.ToString().ToLower(),
            CurrentDay = _gameWorld.CurrentDay
        };

        // Resolve entity references based on placement type
        switch (placementType)
        {
            case PlacementType.NPC:
                promptContext.NPC = _gameWorld.NPCs.FirstOrDefault(n => n.ID == sceneDto.PlacementId);
                if (promptContext.NPC != null)
                {
                    promptContext.NPCBondLevel = promptContext.NPC.BondStrength;
                    // Also populate location if NPC has one
                    promptContext.Location = promptContext.NPC.Location;
                }
                break;

            case PlacementType.Location:
                promptContext.Location = _gameWorld.Locations.FirstOrDefault(l => l.Id == sceneDto.PlacementId);
                break;

            case PlacementType.Route:
                promptContext.Route = _gameWorld.Routes.FirstOrDefault(r => r.Id == sceneDto.PlacementId);
                break;
        }

        return promptContext;
    }

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
            LocationDTO locationDto = BuildLocationDTO(spec, scene.Id, context);
            locationDtos.Add(locationDto);
        }

        // Generate ItemDTOs from specifications
        foreach (DependentItemSpec spec in scene.Template.DependentItems)
        {
            ItemDTO itemDto = BuildItemDTO(spec, scene.Id, context, locationDtos);
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
    private LocationDTO BuildLocationDTO(DependentLocationSpec spec, string sceneId, SceneSpawnContext context)
    {
        // Generate unique ID
        string locationId = $"{sceneId}_{spec.TemplateId}";

        // Replace tokens in name and description
        string locationName = PlaceholderReplacer.ReplaceAll(spec.NamePattern, context, _gameWorld);
        string locationDescription = PlaceholderReplacer.ReplaceAll(spec.DescriptionPattern, context, _gameWorld);

        // Determine venue ID
        string venueId = DetermineVenueId(spec.VenueIdSource, context);

        // FAIL-FAST BUDGET VALIDATION: Check venue capacity BEFORE creating DTO
        // Since all locations persist forever, budget violations cannot be cleaned up
        Venue venue = _gameWorld.Venues.FirstOrDefault(v => v.Id == venueId);
        if (venue == null)
            throw new InvalidOperationException($"Venue '{venueId}' not found for location '{locationId}'");

        if (!venue.CanAddMoreLocations())
            throw new InvalidOperationException(
                $"Venue '{venue.Id}' ({venue.Name}) has reached capacity " +
                $"({venue.LocationIds.Count}/{venue.MaxLocations} locations). " +
                $"Cannot add location '{locationId}'. " +
                $"Increase MaxLocations or use different venue.");

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
            CanWork = false, // Generated locations don't support work by default,
            WorkType = "",
            WorkPay = 0
        };

        // Map properties
        if (spec.Properties != null && spec.Properties.Any())
        {
            dto.Properties = new LocationPropertiesDTO
            {
                Base = spec.Properties
            };
        }

        return dto;
    }

    /// <summary>
    /// Build ItemDTO from DependentItemSpec
    /// Replaces tokens, maps categories, handles inventory placement
    /// </summary>
    private ItemDTO BuildItemDTO(DependentItemSpec spec, string sceneId, SceneSpawnContext context, List<LocationDTO> createdLocations)
    {
        // Generate unique ID
        string itemId = $"{sceneId}_{spec.TemplateId}";

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
                // Generate new venue for this location
                VenueTemplate venueTemplate = new VenueTemplate
                {
                    NamePattern = "Generated Venue",
                    DescriptionPattern = "A procedurally generated venue.",
                    Type = VenueType.Building,
                    Tier = context.CurrentLocation?.Tier ?? 1,
                    District = context.CurrentLocation?.Venue?.District ?? "wilderness",
                    MaxLocations = 20,
                    HexAllocation = HexAllocationStrategy.ClusterOf7
                };
                Venue generatedVenue = _venueGenerator.GenerateVenue(venueTemplate, context, _gameWorld);
                return generatedVenue.Id;

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
