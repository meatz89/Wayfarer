using System.Runtime.CompilerServices;
using System.Text.Json;

[assembly: InternalsVisibleTo("Wayfarer.Tests")]

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
    private readonly VenueGeneratorService _venueGenerator;

    public SceneInstantiator(
        GameWorld gameWorld,
        SpawnConditionsEvaluator spawnConditionsEvaluator,
        SceneNarrativeService narrativeService,
        VenueGeneratorService venueGenerator)
    {
        _gameWorld = gameWorld;
        _spawnConditionsEvaluator = spawnConditionsEvaluator ?? throw new ArgumentNullException(nameof(spawnConditionsEvaluator));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _venueGenerator = venueGenerator ?? throw new ArgumentNullException(nameof(venueGenerator));
    }

    /// <summary>
    /// PHASE 1: Create deferred scene WITHOUT dependent resources
    /// Generates JSON package with ONLY Scene + Situations (NO dependent locations/items)
    /// Scene.State = "Deferred" (dependent resources not spawned yet)
    /// HIGHLANDER: Returns JSON string for ContentGenerationFacade to write, PackageLoaderFacade to load
    /// NEVER creates entities directly - only DTOs and JSON
    /// </summary>
    public string CreateDeferredScene(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        // Evaluate spawn conditions
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

        // System 3: Generate Scene DTO with categorical specifications (NO resolution)
        // CRITICAL: State = "Deferred" (not Active yet)
        SceneDTO sceneDto = GenerateSceneDTO(template, spawnReward, context, isDeferredState: true);

        // Generate Situation DTOs (entities reference by categories, no markers)
        List<SituationDTO> situationDtos = GenerateSituationDTOs(template, sceneDto, context);
        sceneDto.Situations = situationDtos;

        // Build package with ONLY Scene + Situations (empty lists for dependent resources)
        string packageJson = BuildScenePackage(sceneDto, new List<LocationDTO>(), new List<ItemDTO>());

        Console.WriteLine($"[SceneInstantiator] Generated DEFERRED scene package '{sceneDto.Id}' with {situationDtos.Count} situations (State=Deferred, NO dependent resources)");

        return packageJson;
    }

    /// <summary>
    /// PHASE 2: Activate scene by spawning dependent resources for existing deferred scene
    /// Generates JSON package with ONLY dependent locations + items (NO Scene/Situations)
    /// Scene transitions: Deferred → Active
    /// Input: Scene entity (already exists in GameWorld with State=Deferred)
    /// Output: JSON package with dependent resources only
    /// </summary>
    public string ActivateScene(Scene scene, SceneSpawnContext context)
    {
        if (scene.State != SceneState.Deferred)
        {
            throw new InvalidOperationException(
                $"Scene '{scene.TemplateId}' cannot be activated - State is {scene.State}, expected Deferred");
        }

        if (scene.Template == null)
        {
            throw new InvalidOperationException(
                $"Scene '{scene.TemplateId}' cannot be activated - Template reference is null");
        }

        // Generate unique scene ID for dependent resource tagging
        // CRITICAL: Use Scene.TemplateId NOT Scene.Id (Scene has no Id property per HIGHLANDER)
        // Use a deterministic tag based on template + timestamp to bind dependent resources
        string sceneTag = $"{scene.TemplateId}_{Guid.NewGuid().ToString("N")}";

        // Generate dependent resource DTOs (if self-contained scene)
        List<LocationDTO> dependentLocations = new List<LocationDTO>();
        List<ItemDTO> dependentItems = new List<ItemDTO>();

        if (scene.Template.DependentLocations.Any() || scene.Template.DependentItems.Any())
        {
            // Generate dependent location DTOs
            foreach (DependentLocationSpec spec in scene.Template.DependentLocations)
            {
                LocationDTO locationDto = BuildLocationDTO(spec, sceneTag, context);
                dependentLocations.Add(locationDto);
            }

            // Generate dependent item DTOs
            foreach (DependentItemSpec spec in scene.Template.DependentItems)
            {
                ItemDTO itemDto = BuildItemDTO(spec, sceneTag, context, dependentLocations);
                dependentItems.Add(itemDto);
            }
        }

        // Build package with ONLY dependent resources (empty list for Scenes)
        string packageJson = BuildScenePackage(null, dependentLocations, dependentItems);

        Console.WriteLine($"[SceneInstantiator] Generated activation package for scene '{scene.TemplateId}' with {dependentLocations.Count} locations and {dependentItems.Count} items");

        return packageJson;
    }

    /// <summary>
    /// PHASE 2.5: Resolve entity references from filters AFTER dependent resources created
    /// THREE-TIER TIMING: Filters stored at parse (Tier 1), resolved here at activation (Tier 2)
    /// Called by LocationFacade.CheckAndActivateDeferredScenes() after PackageLoader completes
    ///
    /// VENUE-SCOPED CATEGORICAL RESOLUTION:
    /// - ALL situations MUST have explicit filter properties (null filter = BUG)
    /// - EntityResolver searches ONLY within CurrentVenue (no cross-venue teleportation)
    /// - Categorical properties (Profession, Purpose, Privacy, etc.) determine matches
    /// - Creates dependent entities if not found within venue
    ///
    /// Scene template parameterizes archetype filters for context:
    /// - Archetype provides structure (e.g., inn_lodging: 3 situations)
    /// - Template provides filter values (e.g., Profession=Innkeeper, Purpose=Commerce)
    /// - Same archetype reused with different filter parameters in different contexts
    ///
    /// Input: Scene with Situations containing PlacementFilters but NULL entity references
    /// Output: Scene with Situations containing resolved Location/Npc/Route object references
    /// </summary>
    /// <param name="scene">Scene with situations containing filters but null entity references</param>
    /// <param name="context">Spawn context with CurrentVenue for venue-scoped resolution</param>
    public void ResolveSceneEntityReferences(Scene scene, SceneSpawnContext context)
    {
        Console.WriteLine($"[SceneInstantiator] Resolving entity references for scene '{scene.DisplayName}' within venue '{context.CurrentVenue?.Name ?? "NO VENUE"}' with {scene.Situations.Count} situations");

        // Create EntityResolver for venue-scoped categorical matching
        EntityResolver entityResolver = new EntityResolver(_gameWorld, context.Player, _narrativeService, context.CurrentVenue);

        // For each situation, resolve entities from explicit filters (null filter = BUG)
        foreach (Situation situation in scene.Situations)
        {
            // LOCATION RESOLUTION - filter must be explicit
            if (situation.LocationFilter != null)
            {
                situation.Location = entityResolver.FindOrCreateLocation(situation.LocationFilter);
                Console.WriteLine($"[SceneInstantiator]   ✅ Resolved Location '{situation.Location?.Name ?? "NULL"}' for situation '{situation.Name}' via categorical filter");
            }
            else if (situation.Location == null)
            {
                Console.WriteLine($"[SceneInstantiator]   ⚠️ WARNING: Situation '{situation.Name}' has null LocationFilter - THIS IS A BUG");
            }

            // NPC RESOLUTION - filter must be explicit
            if (situation.NpcFilter != null)
            {
                situation.Npc = entityResolver.FindOrCreateNPC(situation.NpcFilter);
                Console.WriteLine($"[SceneInstantiator]   ✅ Resolved NPC '{situation.Npc?.Name ?? "NULL"}' for situation '{situation.Name}' via categorical filter");
            }
            else if (situation.Npc == null)
            {
                Console.WriteLine($"[SceneInstantiator]   ℹ️ Situation '{situation.Name}' has null NpcFilter (solo situation - no NPC needed)");
            }

            // ROUTE RESOLUTION - filter must be explicit
            if (situation.RouteFilter != null)
            {
                situation.Route = entityResolver.FindOrCreateRoute(situation.RouteFilter);
                Console.WriteLine($"[SceneInstantiator]   ✅ Resolved Route '{situation.Route?.Name ?? "NULL"}' for situation '{situation.Name}' via categorical filter");
            }
            else if (situation.Route == null)
            {
                Console.WriteLine($"[SceneInstantiator]   ℹ️ Situation '{situation.Name}' has null RouteFilter (not route-based)");
            }
        }

        Console.WriteLine($"[SceneInstantiator] ✅ Entity resolution complete for scene '{scene.DisplayName}'");
    }

    /// <summary>
    /// Generate complete scene package JSON from template.
    /// Runtime content generation produces JSON packages for PackageLoader (same architecture as authored content).
    /// Returns JSON string containing Scene + Situations + Dependent Resources.
    /// HIGHLANDER: Returns JSON string for ContentGenerationFacade to write, PackageLoaderFacade to load.
    /// NEVER creates entities directly - only DTOs and JSON.
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

        // System 3: Generate Scene DTO with categorical specifications (NO resolution)
        SceneDTO sceneDto = GenerateSceneDTO(template, spawnReward, context, isDeferredState: false);

        // Generate dependent resource DTOs (if self-contained scene)
        // Categories → FindOrGenerate → Concrete IDs stored directly
        List<LocationDTO> dependentLocations = new List<LocationDTO>();
        List<ItemDTO> dependentItems = new List<ItemDTO>();

        if (template.DependentLocations.Any() || template.DependentItems.Any())
        {
            // Generate dependent location DTOs
            foreach (DependentLocationSpec spec in template.DependentLocations)
            {
                LocationDTO locationDto = BuildLocationDTO(spec, sceneDto.Id, context);
                dependentLocations.Add(locationDto);
            }

            // Generate dependent item DTOs
            foreach (DependentItemSpec spec in template.DependentItems)
            {
                ItemDTO itemDto = BuildItemDTO(spec, sceneDto.Id, context, dependentLocations);
                dependentItems.Add(itemDto);
            }
        }

        // Generate Situation DTOs (entities reference by categories, no markers)
        List<SituationDTO> situationDtos = GenerateSituationDTOs(template, sceneDto, context);
        sceneDto.Situations = situationDtos;

        // Build complete package
        string packageJson = BuildScenePackage(sceneDto, dependentLocations, dependentItems);

        Console.WriteLine($"[SceneInstantiator] Generated scene package '{sceneDto.Id}' with {situationDtos.Count} situations");

        return packageJson;
    }

    /// <summary>
    /// Generate SceneDTO from template with categorical specifications (System 3)
    /// Does NOT resolve entities - writes PlacementFilterDTO to JSON
    /// EntityResolver (System 4) will FindOrCreate from these specifications
    /// </summary>
    private SceneDTO GenerateSceneDTO(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context, bool isDeferredState)
    {
        // Generate unique Scene ID (pure identifier, TemplateId property tracks source template)
        string sceneId = Guid.NewGuid().ToString();

        // Calculate expiration day
        int? expiresOnDay = template.ExpirationDays.HasValue
            ? _gameWorld.CurrentDay + template.ExpirationDays.Value
            : null;

        // Use templates directly (AI generates complete text with entity context)
        string displayName = template.DisplayNameTemplate;
        string introNarrative = template.IntroNarrativeTemplate;

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

        // System 3: Write categorical specifications (NOT concrete IDs)
        // Copy activation filters from template to scene instance
        // These determine WHEN scene activates (Deferred → Active transition)

        SceneDTO dto = new SceneDTO
        {
            Id = sceneId,
            TemplateId = template.Id,
            // Activation filters: Copied from template, determine activation trigger
            LocationActivationFilter = ConvertPlacementFilterToDTO(template.LocationActivationFilter),
            NpcActivationFilter = ConvertPlacementFilterToDTO(template.NpcActivationFilter),
            State = isDeferredState ? "Deferred" : "Active", // TWO-PHASE: Deferred or Active based on caller
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
            SourceSituationId = context.CurrentSituation?.TemplateId
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
        SceneSpawnContext context)
    {
        List<SituationDTO> situationDtos = new List<SituationDTO>();

        foreach (SituationTemplate sitTemplate in template.SituationTemplates)
        {
            // Generate unique Situation ID (pure identifier, TemplateId property tracks source template)
            string situationId = Guid.NewGuid().ToString();

            // Use narrative template (narrative generation with resolved entities happens later in System 5)
            string description = sitTemplate.NarrativeTemplate;
            if (string.IsNullOrEmpty(description))
            {
                description = "A situation unfolds before you.";
            }

            // AI generates complete text with entity context (no placeholder replacement needed)

            // Get DeckId from Challenge choice template (set at parse-time from archetype)
            string deckId = sitTemplate.ChoiceTemplates
                .FirstOrDefault(c => c.PathType == ChoicePathType.Challenge)
                ?.DeckId ?? string.Empty;

            // Explicit placement filters (NO CSS-style inheritance)
            // Each situation MUST specify explicit filters in its template
            PlacementFilterDTO effectiveLocationFilter = ConvertPlacementFilterToDTO(sitTemplate.LocationFilter);
            PlacementFilterDTO effectiveNpcFilter = ConvertPlacementFilterToDTO(sitTemplate.NpcFilter);
            PlacementFilterDTO effectiveRouteFilter = ConvertPlacementFilterToDTO(sitTemplate.RouteFilter);

            // Replace DEPENDENT_LOCATION markers with scene-specific tags
            // Marker format: "DEPENDENT_LOCATION:private_room" → "{sceneId}_private_room"
            // This binds situations to dependent locations created by THIS scene
            if (effectiveLocationFilter?.LocationTags != null)
            {
                effectiveLocationFilter.LocationTags = effectiveLocationFilter.LocationTags
                    .Select(tag => tag.StartsWith("DEPENDENT_LOCATION:")
                        ? $"{sceneDto.Id}_{tag.Substring("DEPENDENT_LOCATION:".Length)}"
                        : tag)
                    .ToList();
            }

            // Build Situation DTO from template
            // Scene-based situations use templates - most DTO properties are for standalone situations
            SituationDTO situationDto = new SituationDTO
            {
                Id = situationId,
                TemplateId = sitTemplate.Id,
                Name = sitTemplate.Name,
                Description = description,
                InteractionType = "Instant",  // Scene situations present choices (instant interaction, choice determines next action)
                SystemType = sitTemplate.SystemType.ToString(),
                DeckId = deckId,
                // Hierarchical placement: Situation filters override scene base filters
                LocationFilter = effectiveLocationFilter,
                NpcFilter = effectiveNpcFilter,
                RouteFilter = effectiveRouteFilter
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
    /// TWO-PHASE SUPPORT: sceneDto can be null (Phase 2 activation with only dependent resources)
    /// </summary>
    private string BuildScenePackage(SceneDTO sceneDto, List<LocationDTO> dependentLocations, List<ItemDTO> dependentItems)
    {
        // TWO-PHASE: For Phase 2 (activation), sceneDto is null and we only have dependent resources
        string packageName = sceneDto != null
            ? $"Scene {sceneDto.DisplayName}"
            : "Scene Activation (Dependent Resources)";

        List<SceneDTO> scenes = sceneDto != null
            ? new List<SceneDTO> { sceneDto }
            : new List<SceneDTO>(); // Empty list for Phase 2

        Package package = new Package
        {
            PackageId = $"scene_package_{Guid.NewGuid().ToString("N")}",
            Metadata = new PackageMetadata
            {
                Name = packageName,
                Author = "Scene System",
                Version = "1.0.0"
            },
            Content = new PackageContent
            {
                Scenes = scenes,
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

    // ResolvePlacement method DELETED - placement resolution moved to THREE-TIER TIMING MODEL
    // Tier 1 (Parse): SceneParser stores PlacementFilters, entities NULL
    // Tier 2 (Activation): SceneInstantiator.ResolveSceneEntityReferences() calls EntityResolver AFTER PackageLoader
    // Tier 3 (Query): Actions instantiated with resolved entities

    /// <summary>
    /// Instantiate Situation from SituationTemplate
    /// Creates embedded Situation within Scene in DORMANT state
    /// NO ACTIONS CREATED - actions instantiated at query time by SceneFacade
    /// PLACEMENT INHERITANCE: Situation inherits placement from parent Scene
    /// </summary>
    private Situation InstantiateSituation(SituationTemplate template, Scene parentScene, SceneSpawnContext context)
    {
        // Generate unique Situation ID (pure identifier, TemplateId property tracks source template)
        string situationId = Guid.NewGuid().ToString();

        // Create Situation from template
        Situation situation = new Situation
        {
            Name = template.Name,  // Copy display name from template
            TemplateId = template.Id,
            Template = template,  // CRITICAL: Store template for lazy action instantiation
            Type = template.Type,  // Copy semantic type (Normal vs Crisis) from template
            Description = template.NarrativeTemplate,
            NarrativeHints = template.NarrativeHints,
            InstantiationState = InstantiationState.Deferred,  // START deferred - actions created at query time

            // PARENT SCENE REFERENCE: Enables state machine navigation (Scene.AdvanceToNextSituation)
            // ARCHITECTURAL CHANGE: Situation owns placement directly (not inherited from scene)
            ParentScene = parentScene
        };

        // NO ACTION CREATION HERE - actions instantiated by SceneFacade when player enters context
        // This is the CRITICAL timing change: Instantiation Time (Tier 2) → Query Time (Tier 3)

        return situation;
    }

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
        return selectedNPC?.Name;
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
            // Check location capabilities (if specified)
            if (filter.RequiredCapabilities != LocationCapability.None)
            {
                // Location must have ALL required capabilities (bitwise AND check)
                if ((loc.Capabilities & filter.RequiredCapabilities) != filter.RequiredCapabilities)
                    return false;
            }

            // Check district (accessed via Location.Venue.District)
            // HIGHLANDER: District is object reference, filter.DistrictId is string (DTO boundary)
            if (!string.IsNullOrEmpty(filter.DistrictId))
            {
                if (loc.Venue == null || loc.Venue.District == null || loc.Venue.District.Name != filter.DistrictId)
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
        return selectedLocation?.Name;
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

            // Check difficulty rating range (0-100 scale)
            if (filter.MinDifficulty.HasValue && route.DangerRating < filter.MinDifficulty.Value)
                return false;
            if (filter.MaxDifficulty.HasValue && route.DangerRating > filter.MaxDifficulty.Value)
                return false;

            // Check player state filters (shared across all placement types)
            if (!CheckPlayerStateFilters(filter, player))
                return false;

            return true;
        });

        return matchingRoute?.Name;
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
            // HIGHLANDER: Compare Achievement objects directly
            if (!filter.RequiredAchievements.All(achievement =>
                player.EarnedAchievements.Any(a => a.Achievement == achievement)))
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
        if (filter.RequiredCapabilities != LocationCapability.None)
            criteria.Add($"Required Capabilities: {filter.RequiredCapabilities}");
        if (!string.IsNullOrEmpty(filter.DistrictId))
            criteria.Add($"District: {filter.DistrictId}");
        if (!string.IsNullOrEmpty(filter.RegionId))
            criteria.Add($"Region: {filter.RegionId}");

        // Route filters
        if (filter.TerrainTypes != null && filter.TerrainTypes.Count > 0)
            criteria.Add($"Terrain Types: [{string.Join(", ", filter.TerrainTypes)}]");
        if (filter.RouteTier.HasValue)
            criteria.Add($"Route Tier: {filter.RouteTier.Value}");
        if (filter.MinDifficulty.HasValue)
            criteria.Add($"Min Difficulty: {filter.MinDifficulty.Value}");
        if (filter.MaxDifficulty.HasValue)
            criteria.Add($"Max Difficulty: {filter.MaxDifficulty.Value}");

        // Player state filters
        if (filter.RequiredStates != null && filter.RequiredStates.Count > 0)
            criteria.Add($"Required States: [{string.Join(", ", filter.RequiredStates)}]");
        if (filter.ForbiddenStates != null && filter.ForbiddenStates.Count > 0)
            criteria.Add($"Forbidden States: [{string.Join(", ", filter.ForbiddenStates)}]");
        if (filter.RequiredAchievements != null && filter.RequiredAchievements.Count > 0)
            criteria.Add($"Required Achievements: [{string.Join(", ", filter.RequiredAchievements.Select(a => a.Name))}]");
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
                    .Select(loc => $"  - {loc.Name}: Capabilities={loc.Capabilities}")
                    .ToList();
                if (locationCount > 10)
                    locationSummaries.Add($"  ... and {locationCount - 10} more locations");
                return $"Total Locations: {locationCount}\n{string.Join("\n", locationSummaries)}";

            case PlacementType.NPC:
                int npcCount = _gameWorld.NPCs.Count;
                List<string> npcSummaries = _gameWorld.NPCs
                    .Take(10)
                    .Select(npc => $"  - {npc.Name}: Personality={npc.PersonalityType}, Bond={npc.BondStrength}")
                    .ToList();
                if (npcCount > 10)
                    npcSummaries.Add($"  ... and {npcCount - 10} more NPCs");
                return $"Total NPCs: {npcCount}\n{string.Join("\n", npcSummaries)}";

            case PlacementType.Route:
                int routeCount = _gameWorld.Routes.Count;
                List<string> routeSummaries = _gameWorld.Routes
                    .Take(10)
                    .Select(route => $"  - {route.Name}: Tier={route.Tier}, Danger={route.DangerRating}")
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
            contextInfo.Add($"Current Location: {context.CurrentLocation.Name}");
        else
            contextInfo.Add($"Current Location: null");

        if (context.CurrentNPC != null)
            contextInfo.Add($"Current NPC: {context.CurrentNPC.Name}");
        else
            contextInfo.Add($"Current NPC: null");

        if (context.CurrentRoute != null)
            contextInfo.Add($"Current Route: {context.CurrentRoute.Name}");
        else
            contextInfo.Add($"Current Route: null");

        return string.Join("\n", contextInfo);
    }

    /// <summary>
    /// Apply selection strategy to choose ONE NPC from multiple matching candidates
    /// PHASE 3: Implements 4 strategies (Closest, HighestBond, LeastRecent, Random)
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
            PlacementSelectionStrategy.Random => SelectRandomNPC(candidates),
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
            PlacementSelectionStrategy.HighestBond => SelectRandomLocation(candidates), // N/A for locations
            PlacementSelectionStrategy.LeastRecent => SelectLeastRecentLocation(candidates, player),
            PlacementSelectionStrategy.Random => SelectRandomLocation(candidates),
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
    /// Falls back to Random if no interaction history exists
    /// </summary>
    private NPC SelectLeastRecentNPC(List<NPC> candidates, Player player)
    {
        // Create interaction lookup dictionary for fast access
        // ONE record per NPC (update-in-place pattern) - no GroupBy needed
        // HIGHLANDER: Use NPC object as key, not string ID
        Dictionary<NPC, NPCInteractionRecord> interactionLookup = player.NPCInteractions
            .ToDictionary(interaction => interaction.Npc);

        // Find candidate with oldest interaction (or never interacted)
        NPC leastRecentNPC = null;
        long oldestTimestamp = long.MaxValue;

        foreach (NPC candidate in candidates)
        {
            if (!interactionLookup.ContainsKey(candidate))
            {
                // Never interacted with this NPC - prioritize these
                return candidate;
            }

            NPCInteractionRecord record = interactionLookup[candidate];
            long timestamp = CalculateTimestamp(record.LastInteractionDay, record.LastInteractionTimeBlock, record.LastInteractionSegment);

            if (timestamp < oldestTimestamp)
            {
                oldestTimestamp = timestamp;
                leastRecentNPC = candidate;
            }
        }

        // If all candidates have been interacted with, return least recent
        // If somehow nothing found, fall back to random
        return leastRecentNPC ?? SelectRandomNPC(candidates);
    }

    /// <summary>
    /// Select random NPC from candidates using RNG for unpredictable variety (uniform distribution)
    /// </summary>
    private NPC SelectRandomNPC(List<NPC> candidates)
    {
        int index = Random.Shared.Next(candidates.Count);
        return candidates[index];
    }

    /// <summary>
    /// Select random Location from candidates using RNG for unpredictable variety (uniform distribution)
    /// </summary>
    private Location SelectRandomLocation(List<Location> candidates)
    {
        int index = Random.Shared.Next(candidates.Count);
        return candidates[index];
    }

    /// <summary>
    /// Select Location least recently visited for content variety
    /// Uses Player.LocationVisits timestamp data to find oldest visit
    /// Falls back to Random if no visit history exists
    /// </summary>
    private Location SelectLeastRecentLocation(List<Location> candidates, Player player)
    {
        // Find candidate with oldest visit (or never visited)
        // Use LINQ queries over List<T>, NOT Dictionary (DOMAIN COLLECTION PRINCIPLE)
        Location leastRecentLocation = null;
        long oldestTimestamp = long.MaxValue;

        foreach (Location candidate in candidates)
        {
            // LINQ query: Find visit record for this location
            LocationVisitRecord record = player.LocationVisits
                .FirstOrDefault(visit => visit.Location == candidate);

            if (record == null)
            {
                // Never visited this location - prioritize these
                return candidate;
            }

            long timestamp = CalculateTimestamp(record.LastVisitDay, record.LastVisitTimeBlock, record.LastVisitSegment);

            if (timestamp < oldestTimestamp)
            {
                oldestTimestamp = timestamp;
                leastRecentLocation = candidate;
            }
        }

        // If all candidates have been visited, return least recent
        // If somehow nothing found, fall back to random
        return leastRecentLocation ?? SelectRandomLocation(candidates);
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
    /// Generate dependent resource SPECS for self-contained scene
    /// Returns specs to orchestrator who creates JSON files and loads via PackageLoader
    /// Does NOT load resources itself (pure generation, no infrastructure)
    /// Called from FinalizeScene BEFORE situation instantiation
    /// </summary>
    private DependentResourceSpecs GenerateDependentResourceSpecs(Scene scene, SceneSpawnContext context)
    {
        // Generate unique ID for this package (Scene.Id no longer exists)
        string uniqueSceneId = Guid.NewGuid().ToString();

        // Build lists of DTOs
        List<LocationDTO> locationDtos = new List<LocationDTO>();
        List<ItemDTO> itemDtos = new List<ItemDTO>();

        // Generate LocationDTOs from specifications
        foreach (DependentLocationSpec spec in scene.Template.DependentLocations)
        {
            LocationDTO locationDto = BuildLocationDTO(spec, uniqueSceneId, context);
            locationDtos.Add(locationDto);
        }

        // Generate ItemDTOs from specifications
        foreach (DependentItemSpec spec in scene.Template.DependentItems)
        {
            ItemDTO itemDto = BuildItemDTO(spec, uniqueSceneId, context, locationDtos);
            itemDtos.Add(itemDto);
        }

        // Create Package object
        string packageId = $"scene_{uniqueSceneId}_dep";
        Package package = new Package
        {
            PackageId = packageId,
            Metadata = new PackageMetadata
            {
                Name = $"Scene (template '{scene.TemplateId}') Dependent Resources",
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
                string itemId = $"{spec.TemplateId}_{Guid.NewGuid().ToString("N")}";
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
        string locationId = $"{spec.TemplateId}_{Guid.NewGuid().ToString("N")}";

        // Use patterns directly (AI generates complete text with entity context)
        string locationName = spec.NamePattern;
        string locationDescription = spec.DescriptionPattern;

        // Determine venue ID
        string venueId = DetermineVenueId(spec.VenueIdSource, context);

        // FAIL-FAST BUDGET VALIDATION: Check venue capacity BEFORE creating DTO
        // Since all locations persist forever, budget violations cannot be cleaned up
        Venue venue = _gameWorld.Venues.FirstOrDefault(v => v.Name == venueId);
        if (venue == null)
            throw new InvalidOperationException($"Venue '{venueId}' not found for location '{locationId}'");

        if (!_gameWorld.CanVenueAddMoreLocations(venue))
        {
            // HIGHLANDER: Pass Venue object, not string
            int currentCount = _gameWorld.GetLocationCountInVenue(venue);
            throw new InvalidOperationException(
                $"Venue '{venue.Name}' has reached capacity " +
                $"({currentCount}/{venue.MaxLocations} locations). " +
                $"Cannot add location '{locationId}'. " +
                $"Increase MaxLocations or use different venue.");
        }

        // HIGHLANDER: NO hex placement logic here - ALL placement via LocationPlacementService
        // SceneInstantiator creates LocationDTO with NO hex coordinates
        // PackageLoader.PlaceLocations() handles placement for ALL locations (authored + generated)

        // FAIL-FAST VALIDATION: ALL categorical dimensions REQUIRED from spec
        if (string.IsNullOrEmpty(spec.Privacy))
        {
            throw new InvalidOperationException(
                $"DependentLocationSpec '{spec.TemplateId}' missing required Privacy property. " +
                $"Every location MUST have explicit Privacy. Valid values: Public, SemiPublic, Private, Restricted");
        }
        if (string.IsNullOrEmpty(spec.Safety))
        {
            throw new InvalidOperationException(
                $"DependentLocationSpec '{spec.TemplateId}' missing required Safety property. " +
                $"Every location MUST have explicit Safety. Valid values: Dangerous, Unsafe, Neutral, Safe, Secure");
        }
        if (string.IsNullOrEmpty(spec.Activity))
        {
            throw new InvalidOperationException(
                $"DependentLocationSpec '{spec.TemplateId}' missing required Activity property. " +
                $"Every location MUST have explicit Activity. Valid values: Quiet, Moderate, Busy, Crowded");
        }
        if (string.IsNullOrEmpty(spec.Purpose))
        {
            throw new InvalidOperationException(
                $"DependentLocationSpec '{spec.TemplateId}' missing required Purpose property. " +
                $"Every location MUST have explicit Purpose. Valid values: Transit, Dwelling, Commerce, Work, Government, Education, Entertainment, Religion, Defense, Storage, Agriculture, Manufacturing");
        }

        // Build LocationDTO
        LocationDTO dto = new LocationDTO
        {
            Id = locationId,
            Name = locationName,
            Description = locationDescription,
            DistanceFromPlayer = "near", // Generated locations default to "near" - placement algorithm will select appropriate venue
            Type = "Room", // Default type for generated locations
            CanInvestigate = spec.CanInvestigate,
            CanWork = false, // Generated locations don't support work by default,
            WorkType = "",
            WorkPay = 0,
            // Scene-specific tag for dependent location binding
            // Enables situations to reference this location via PlacementFilter.LocationTags
            DomainTags = new List<string> { $"{sceneId}_{spec.TemplateId}" },
            // FAIL-FAST: Read ALL categorical dimensions from spec (NO defaults)
            Privacy = spec.Privacy,
            Safety = spec.Safety,
            Activity = spec.Activity,
            Purpose = spec.Purpose,
            // FAIL-FAST: ALL gameplay properties REQUIRED (no defaults allowed)
            LocationType = "Inn", // Generated locations are typically indoor private spaces
            ObligationProfile = "Research", // Default profile for generated locations
            IsStartingLocation = false // Generated locations are never starting locations
        };

        // Map capabilities (functional properties)
        if (spec.Properties != null && spec.Properties.Any())
        {
            dto.Capabilities = spec.Properties;
        }

        // SPATIAL CONSTRAINT: All dependent locations spawn in SameVenue as activation location
        // Ensures verisimilitude ("your room at this inn" spawns at this inn, not different venue)
        dto.ProximityConstraint = new ProximityConstraintDTO
        {
            Proximity = "SameVenue",
            ReferenceLocation = "current"
        };

        return dto;
    }

    /// <summary>
    /// Build ItemDTO from DependentItemSpec
    /// Replaces tokens, maps categories, handles inventory placement
    /// </summary>
    private ItemDTO BuildItemDTO(DependentItemSpec spec, string sceneId, SceneSpawnContext context, List<LocationDTO> createdLocations)
    {
        // Generate unique ID
        string itemId = $"{spec.TemplateId}_{Guid.NewGuid().ToString("N")}";

        // Use patterns directly (AI generates complete text with entity context)
        string itemName = spec.NamePattern;
        string itemDescription = spec.DescriptionPattern;

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
                // ADR-007: Use Venue.Name instead of deleted VenueId
                return context.CurrentLocation.Venue.Name;

            case VenueIdSource.GenerateNew:
                // Generate new venue for this location
                VenueTemplate venueTemplate = new VenueTemplate
                {
                    NamePattern = "Generated Venue",
                    DescriptionPattern = "A procedurally generated venue.",
                    Type = VenueType.Merchant,
                    Tier = context.CurrentLocation?.Tier ?? 1,
                    District = context.CurrentLocation?.Venue?.District?.Name ?? "wilderness",
                    MaxLocations = 20,
                    HexAllocation = HexAllocationStrategy.ClusterOf7
                };
                Venue generatedVenue = _venueGenerator.GenerateVenue(venueTemplate, context, _gameWorld);
                return generatedVenue.Name;

            default:
                throw new InvalidOperationException($"Unknown VenueIdSource: {source}");
        }
    }

    // HIGHLANDER: FindAdjacentHex and IsAdjacentToOtherVenue methods DELETED
    // These methods now live in LocationPlacementService (single implementation for ALL location placement)
    // SceneInstantiator no longer performs ANY hex placement logic

    /// <summary>
    /// Convert PlacementFilter domain entity to PlacementFilterDTO for JSON serialization
    /// System 3: Writes categorical specifications to JSON (NO entity resolution)
    /// </summary>
    private PlacementFilterDTO ConvertPlacementFilterToDTO(PlacementFilter filter)
    {
        if (filter == null)
            return null;

        return new PlacementFilterDTO
        {
            PlacementType = filter.PlacementType.ToString(),
            SelectionStrategy = filter.SelectionStrategy.ToString(),
            // NPC filters
            PersonalityTypes = filter.PersonalityTypes?.Select(p => p.ToString()).ToList(),
            Professions = filter.Professions?.Select(p => p.ToString()).ToList(),
            RequiredRelationships = filter.RequiredRelationships?.Select(r => r.ToString()).ToList(),
            MinTier = filter.MinTier,
            MaxTier = filter.MaxTier,
            MinBond = filter.MinBond,
            MaxBond = filter.MaxBond,
            NpcTags = filter.NpcTags,
            // Orthogonal Categorical Dimensions - NPC
            SocialStandings = filter.SocialStandings?.Select(s => s.ToString()).ToList(),
            StoryRoles = filter.StoryRoles?.Select(r => r.ToString()).ToList(),
            KnowledgeLevels = filter.KnowledgeLevels?.Select(k => k.ToString()).ToList(),
            // Location filters
            LocationTypes = filter.LocationTypes?.Select(t => t.ToString()).ToList(),
            Capabilities = ConvertCapabilitiesToStringList(filter.RequiredCapabilities),
            IsPlayerAccessible = filter.IsPlayerAccessible,
            LocationTags = filter.LocationTags,
            DistrictId = filter.DistrictId,
            RegionId = filter.RegionId,
            // Orthogonal Categorical Dimensions - Location
            PrivacyLevels = filter.PrivacyLevels?.Select(p => p.ToString()).ToList(),
            SafetyLevels = filter.SafetyLevels?.Select(s => s.ToString()).ToList(),
            ActivityLevels = filter.ActivityLevels?.Select(a => a.ToString()).ToList(),
            Purposes = filter.Purposes?.Select(p => p.ToString()).ToList(),
            // Route filters
            TerrainTypes = filter.TerrainTypes,
            RouteTier = filter.RouteTier,
            MinDifficulty = filter.MinDifficulty,
            MaxDifficulty = filter.MaxDifficulty,
            RouteTags = filter.RouteTags,
            // Variety control
            ExcludeRecentlyUsed = filter.ExcludeRecentlyUsed,
            // Player state filters
            RequiredStates = filter.RequiredStates?.Select(s => s.ToString()).ToList(),
            ForbiddenStates = filter.ForbiddenStates?.Select(s => s.ToString()).ToList(),
            RequiredAchievements = filter.RequiredAchievements?.Select(a => a.Name).ToList(),
            ScaleRequirements = filter.ScaleRequirements?.Select(r => new ScaleRequirementDTO
            {
                ScaleType = r.ScaleType.ToString(),
                MinValue = r.MinValue,
                MaxValue = r.MaxValue
            }).ToList()
        };
    }

    /// <summary>
    /// Convert LocationCapability flags enum to List of string names.
    /// Extracts individual set flags from combined enum value.
    /// </summary>
    private static List<string> ConvertCapabilitiesToStringList(LocationCapability capabilities)
    {
        if (capabilities == LocationCapability.None)
            return new List<string>();

        List<string> result = new List<string>();
        foreach (LocationCapability flag in Enum.GetValues(typeof(LocationCapability)))
        {
            if (flag != LocationCapability.None && capabilities.HasFlag(flag))
            {
                result.Add(flag.ToString());
            }
        }
        return result;
    }
}
