using System.Runtime.CompilerServices;
using System.Text.Json;

[assembly: InternalsVisibleTo("Wayfarer.Tests")]

/// <summary>
/// Domain Service for generating Scene instance DTOs and activating deferred scenes
///
/// HIGHLANDER COMPLIANCE: Creates entities via PackageLoader (single creation path)
/// Entity creation MUST flow through: DTO → PackageLoader.CreateSingle*() → Entity
///
/// RESPONSIBILITIES:
/// 1. CreateDeferredScene: Generate deferred scene JSON package (NO Situations)
/// 2. ActivateScene: INTEGRATED process - creates Situations AND resolves entities
/// 3. GenerateScenePackageJson: Generates complete package JSON for immediate activation
/// 4. DTO generation: Creates SceneDTO, SituationDTOs, ChoiceTemplateDTOs
///
/// AI GENERATION INTEGRATION: SceneNarrativeService generates narratives during DTO generation
/// Generic text used until AI narrative system implemented
/// </summary>
public class SceneInstantiator
{
    private const int SEGMENTS_PER_DAY = 16;
    private const int SEGMENTS_PER_TIME_BLOCK = 4;

    private readonly GameWorld _gameWorld;
    private readonly SpawnConditionsEvaluator _spawnConditionsEvaluator;
    private readonly SceneNarrativeService _narrativeService;
    private readonly VenueGeneratorService _venueGenerator;
    private readonly PackageLoader _packageLoader;

    public SceneInstantiator(
        GameWorld gameWorld,
        SpawnConditionsEvaluator spawnConditionsEvaluator,
        SceneNarrativeService narrativeService,
        VenueGeneratorService venueGenerator,
        PackageLoader packageLoader)
    {
        _gameWorld = gameWorld;
        _spawnConditionsEvaluator = spawnConditionsEvaluator ?? throw new ArgumentNullException(nameof(spawnConditionsEvaluator));
        _narrativeService = narrativeService ?? throw new ArgumentNullException(nameof(narrativeService));
        _venueGenerator = venueGenerator ?? throw new ArgumentNullException(nameof(venueGenerator));
        _packageLoader = packageLoader ?? throw new ArgumentNullException(nameof(packageLoader));
    }

    /// <summary>
    /// PHASE 1: Create deferred scene WITHOUT Situations or dependent resources
    /// Generates JSON package with ONLY Scene (NO Situations, NO dependent locations/items)
    /// Scene.State = "Deferred" (Situations created at activation time)
    /// CORRECT ARCHITECTURE:
    /// - SceneTemplate has ALL SituationTemplates (blueprint)
    /// - Scene (Deferred) = reference to Template ONLY, Situations = EMPTY
    /// - Scene (Active) = Situation INSTANCES created from SituationTemplates
    /// HIGHLANDER: Returns JSON string for ContentGenerationFacade to write, PackageLoaderFacade to load
    /// NEVER creates entities directly - only DTOs and JSON
    /// </summary>
    public string CreateDeferredScene(SceneTemplate template, SceneSpawnReward spawnReward, SceneSpawnContext context)
    {
        // Evaluate spawn conditions
        // Scenes load as Deferred, activate via LocationActivationFilter when player enters matching location
        bool isEligible = _spawnConditionsEvaluator.EvaluateAll(
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

        // CORRECT ARCHITECTURE: Deferred scenes have NO Situation instances
        // Situations created at activation time from Template.SituationTemplates
        // Empty situations list - player can't interact with deferred scene anyway
        sceneDto.Situations = new List<SituationDTO>();

        // Build package with ONLY Scene (empty situations, empty dependent resources)
        string packageJson = BuildScenePackage(sceneDto, new List<LocationDTO>(), new List<ItemDTO>());

        Console.WriteLine($"[SceneInstantiator] Generated DEFERRED scene package '{sceneDto.Id}' (State=Deferred, NO situations - created at activation)");

        return packageJson;
    }

    /// <summary>
    /// PHASE 2: Activate scene - INTEGRATED PROCESS
    /// Creates Situation instances from SituationTemplates AND resolves entities in one operation.
    /// CORRECT ARCHITECTURE (from arc42/08_crosscutting_concepts.md §8.13-8.14):
    /// - Deferred scene has Situations = EMPTY (just reference to Template)
    /// - Activation creates Situation INSTANCES from Template.SituationTemplates
    /// - For each Situation, resolve entities (find-or-create) via EntityResolver + PackageLoader
    /// - Write entity references directly to Situation instances
    /// Scene transitions: Deferred → Active
    /// </summary>
    public void ActivateScene(Scene scene, SceneSpawnContext context)
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

        // FAIL FAST: SituationTemplates must exist per arc42/08 §8.13
        // "SceneTemplates created with ALL SituationTemplates embedded"
        if (scene.Template.SituationTemplates == null || scene.Template.SituationTemplates.Count == 0)
        {
            throw new InvalidOperationException(
                $"[SceneInstantiator] FAIL FAST: Scene '{scene.DisplayName}' (Template: {scene.TemplateId}) " +
                $"has no SituationTemplates. SceneTemplates must have ALL SituationTemplates embedded. " +
                $"This is a content authoring error.");
        }

        Console.WriteLine($"[SceneInstantiator] Activating scene '{scene.DisplayName}' - creating {scene.Template.SituationTemplates.Count} Situation instances from SituationTemplates");

        // Create EntityResolver for FIND-only (venue-scoped categorical search)
        EntityResolver finder = new EntityResolver(_gameWorld, context.CurrentVenue);

        // Track route for RouteDestination proximity (route resolved earlier, destination used later)
        RouteOption sceneRoute = null;

        // INTEGRATED PROCESS: Create Situation instances AND resolve entities in one loop
        foreach (SituationTemplate sitTemplate in scene.Template.SituationTemplates)
        {
            // Step 1: Create Situation instance from template
            Situation situation = CreateSituationFromTemplate(sitTemplate, scene);
            Console.WriteLine($"[SceneInstantiator]   Created Situation '{situation.Name}' from template '{sitTemplate.Id}'");

            // Step 2: Resolve entities (find-or-create) - INTEGRATED, NOT SEPARATE
            // LOCATION: Find or create (with RouteDestination proximity support)
            if (situation.LocationFilter != null)
            {
                Location location;

                // RouteDestination proximity: use route's destination instead of categorical search
                if (situation.LocationFilter.Proximity == PlacementProximity.RouteDestination)
                {
                    if (sceneRoute == null)
                    {
                        throw new InvalidOperationException(
                            $"[SceneInstantiator] FAIL FAST: Situation '{situation.Name}' has RouteDestination proximity " +
                            $"but no route was resolved in earlier situations of scene '{scene.DisplayName}'. " +
                            $"Route-based situations must precede RouteDestination situations.");
                    }
                    if (sceneRoute.DestinationLocation == null)
                    {
                        throw new InvalidOperationException(
                            $"[SceneInstantiator] FAIL FAST: Route '{sceneRoute.Name}' has null DestinationLocation. " +
                            $"Cannot resolve RouteDestination proximity for situation '{situation.Name}'.");
                    }
                    location = sceneRoute.DestinationLocation;
                    Console.WriteLine($"[SceneInstantiator]     ✅ Using RouteDestination '{location.Name}' from route '{sceneRoute.Name}'");
                }
                else
                {
                    // Standard categorical resolution
                    location = finder.FindLocation(situation.LocationFilter, context.CurrentLocation);

                    if (location == null)
                    {
                        // Not found - create via PackageLoader (HIGHLANDER single creation path)
                        LocationDTO dto = BuildLocationDTOFromFilter(situation.LocationFilter);
                        location = _packageLoader.CreateSingleLocation(dto, context.CurrentVenue);

                        // ADR-012: Mark as SceneCreated for dual-model accessibility
                        // Scene-created locations only accessible when active scene's current situation is there
                        location.Origin = LocationOrigin.SceneCreated;
                        location.Provenance = new SceneProvenance
                        {
                            Scene = scene,
                            CreatedDay = _gameWorld.CurrentDay,
                            CreatedTimeBlock = _gameWorld.CurrentTimeBlock
                        };

                        Console.WriteLine($"[SceneInstantiator]     ✅ CREATED Location '{location.Name}' (SceneCreated)");
                    }
                    else
                    {
                        string resolutionType = situation.LocationFilter.Proximity == PlacementProximity.SameLocation
                            ? "SameLocation proximity"
                            : "categorical filter";
                        Console.WriteLine($"[SceneInstantiator]     ✅ FOUND Location '{location.Name}' via {resolutionType}");
                    }
                }

                situation.Location = location;
            }
            else
            {
                // FAIL FAST: LocationFilter is MANDATORY per gdd/05_content.md §5.8
                throw new InvalidOperationException(
                    $"[SceneInstantiator] FAIL FAST: Situation '{situation.Name}' in scene '{scene.DisplayName}' " +
                    $"has null LocationFilter. LocationFilter is MANDATORY for all situations. " +
                    $"Fix the SituationTemplate to specify a LocationFilter.");
            }

            // NPC: Find or create
            if (situation.NpcFilter != null)
            {
                NPC npc = finder.FindNPC(situation.NpcFilter, context.CurrentLocation);

                if (npc == null)
                {
                    // Not found - create via PackageLoader (HIGHLANDER single creation path)
                    NPCDTO dto = BuildNPCDTOFromFilter(situation.NpcFilter);
                    npc = _packageLoader.CreateSingleNpc(dto, situation.Location);
                    Console.WriteLine($"[SceneInstantiator]     ✅ CREATED NPC '{npc.Name}'");
                }
                else
                {
                    string npcResolutionType = situation.NpcFilter.Proximity == PlacementProximity.SameLocation
                        ? "SameLocation proximity"
                        : "categorical filter";
                    Console.WriteLine($"[SceneInstantiator]     ✅ FOUND NPC '{npc.Name}' via {npcResolutionType}");
                }

                situation.Npc = npc;
            }
            else
            {
                Console.WriteLine($"[SceneInstantiator]     ℹ️ Solo situation (no NPC needed)");
            }

            // ROUTE: Find only (FAIL FAST if not found - route creation not implemented)
            if (situation.RouteFilter != null)
            {
                RouteOption route = finder.FindRoute(situation.RouteFilter, context.CurrentLocation);

                if (route == null)
                {
                    // FAIL FAST: Route expected but not found - this is a content error
                    throw new InvalidOperationException(
                        $"[SceneInstantiator] FAIL FAST: Route not found for situation '{situation.Name}' " +
                        $"in scene '{scene.DisplayName}'. RouteFilter specified but no matching route exists. " +
                        $"Route creation not implemented - ensure routes are pre-authored.");
                }

                string routeResolutionType = situation.RouteFilter.Proximity == PlacementProximity.SameLocation
                    ? "SameLocation proximity"
                    : "categorical filter";
                Console.WriteLine($"[SceneInstantiator]     ✅ FOUND Route '{route.Name}' via {routeResolutionType}");
                situation.Route = route;

                // Track route for RouteDestination proximity (used by later situations like Arrival)
                sceneRoute = route;
            }

            // Step 3: Add to Scene.Situations
            scene.Situations.Add(situation);

            // PROCEDURAL CONTENT TRACING: Record situation spawn during scene activation
            if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
            {
                SceneSpawnNode sceneNode = _gameWorld.ProceduralTracer.GetNodeForScene(scene);
                if (sceneNode != null)
                {
                    _gameWorld.ProceduralTracer.RecordSituationSpawn(
                        situation,
                        sceneNode,
                        SituationSpawnTriggerType.InitialScene
                    );
                }
            }
        }

        // Set CurrentSituationIndex to 0 (first situation)
        scene.CurrentSituationIndex = 0;
        scene.SituationCount = scene.Situations.Count;

        // Transition state: Deferred → Active
        scene.State = SceneState.Active;

        Console.WriteLine($"[SceneInstantiator] ✅ Scene '{scene.DisplayName}' activated with {scene.Situations.Count} Situations (State=Active)");
    }

    /// <summary>
    /// Create Situation instance from SituationTemplate
    /// CORRECT ARCHITECTURE: Situation instances created at scene activation (not at parse time)
    /// Copies template data to instance, sets entity references to NULL
    /// Entity resolution happens immediately after in ActivateScene() (INTEGRATED process)
    /// </summary>
    private Situation CreateSituationFromTemplate(SituationTemplate template, Scene parentScene)
    {
        Situation situation = new Situation
        {
            Name = template.Name,
            TemplateId = template.Id,
            Template = template,
            Type = template.Type,
            Description = template.NarrativeTemplate ?? "A situation unfolds before you.",
            NarrativeHints = template.NarrativeHints,
            SystemType = template.SystemType,
            InstantiationState = InstantiationState.Deferred,

            // PARENT SCENE REFERENCE: Enables state machine navigation
            ParentScene = parentScene,

            // Entity references NULL here - resolved immediately after in ActivateScene()
            Location = null,
            Npc = null,
            Route = null,

            // Copy PlacementFilters from template for entity resolution
            LocationFilter = template.LocationFilter,
            NpcFilter = template.NpcFilter,
            RouteFilter = template.RouteFilter,

            // Route segment placement from RouteFilter (geographic specificity)
            SegmentIndex = template.RouteFilter?.SegmentIndex ?? 0,

            // Default values for instance-level tracking
            Lifecycle = new SpawnTracking(),
            InteractionType = SituationInteractionType.Instant,
            SituationCards = new List<SituationCard>()
        };

        return situation;
    }

    // NOTE: ResolveSceneEntityReferences() DELETED - logic merged into ActivateScene()
    // Entity resolution now happens as an INTEGRATED process during activation

    /// <summary>
    /// Build LocationDTO from PlacementFilter for runtime creation.
    /// Filter has plural lists (for matching flexibility), DTO has singular strings (for creation).
    /// </summary>
    private LocationDTO BuildLocationDTOFromFilter(PlacementFilter filter)
    {
        return new LocationDTO
        {
            Name = _narrativeService.GenerateLocationName(filter),
            Role = (filter.LocationRole ?? LocationRole.Generic).ToString(),
            Privacy = (filter.Privacy ?? LocationPrivacy.Public).ToString(),
            Safety = (filter.Safety ?? LocationSafety.Neutral).ToString(),
            Activity = (filter.Activity ?? LocationActivity.Moderate).ToString(),
            Purpose = (filter.Purpose ?? LocationPurpose.Generic).ToString(),
            ObligationProfile = ObligationDiscipline.Research.ToString()
        };
    }

    /// <summary>
    /// Build NPCDTO from PlacementFilter for runtime creation.
    /// Filter has plural lists (for matching flexibility), DTO has singular strings (for creation).
    /// </summary>
    private NPCDTO BuildNPCDTOFromFilter(PlacementFilter filter)
    {
        return new NPCDTO
        {
            Id = $"generated_npc_{Guid.NewGuid():N}",
            Name = _narrativeService.GenerateNPCName(filter),
            Profession = (filter.Profession ?? Professions.Commoner).ToString(),
            PersonalityType = (filter.PersonalityType ?? PersonalityType.Neutral).ToString(),
            CurrentState = "Neutral",
            SpawnLocation = new PlacementFilterDTO { PlacementType = "Location" },
            Tier = filter.MinTier ?? 1,
            Role = "Generated NPC",
            Description = "A person you've encountered"
        };
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
        // Evaluate spawn conditions
        // Scenes load as Deferred, activate via LocationActivationFilter when player enters matching location
        bool isEligible = _spawnConditionsEvaluator.EvaluateAll(
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

        // Generate Situation DTOs (entities reference by categories, no markers)
        List<SituationDTO> situationDtos = GenerateSituationDTOs(template, sceneDto, context);
        sceneDto.Situations = situationDtos;

        // Build complete package (locations resolved via EntityResolver, not in package)
        string packageJson = BuildScenePackage(sceneDto, new List<LocationDTO>(), new List<ItemDTO>());

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
        // Copy activation filter from template to scene instance
        // Scenes activate via LOCATION ONLY (player enters location matching filter)

        SceneDTO dto = new SceneDTO
        {
            Id = sceneId,
            TemplateId = template.Id,
            // Activation filter: Copied from template, determines activation trigger
            // Scenes activate via LOCATION ONLY when player enters matching location
            LocationActivationFilter = ConvertPlacementFilterToDTO(template.LocationActivationFilter),
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
                // Explicit placement filters (no inheritance - situations specify their own filters)
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

    // ResolvePlacement method DELETED - entity resolution moved to ActivateScene() INTEGRATED process
    // Deferred Scene (Parse): SceneParser stores PlacementFilters, entities NULL, Situations = EMPTY
    // Active Scene (Activation): ActivateScene() creates Situations AND resolves entities (find-or-create)
    // Query Time: Actions instantiated with resolved entities

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
            if (filter.PersonalityType.HasValue)
            {
                if (npc.PersonalityType != filter.PersonalityType.Value)
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
            // Check district (accessed via Location.Venue.District)
            // HIGHLANDER: District is object reference, filter.DistrictId is string (DTO boundary)
            if (!string.IsNullOrEmpty(filter.DistrictId))
            {
                if (loc.Venue == null || loc.Venue.District == null || loc.Venue.District.Name != filter.DistrictId)
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
            // Check terrain type (dominant terrain from TerrainCategories)
            if (filter.Terrain != null)
            {
                string dominantTerrain = route.GetDominantTerrainType();
                if (dominantTerrain != filter.Terrain.ToString())
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
        if (filter.PersonalityType.HasValue)
            criteria.Add($"Personality Type: {filter.PersonalityType.Value}");
        if (filter.MinBond.HasValue)
            criteria.Add($"MinBond: {filter.MinBond.Value}");
        if (filter.MaxBond.HasValue)
            criteria.Add($"MaxBond: {filter.MaxBond.Value}");
        if (filter.NpcTags != null && filter.NpcTags.Count > 0)
            criteria.Add($"NPC Tags: [{string.Join(", ", filter.NpcTags)}]");

        // Location filters
        if (filter.LocationRole.HasValue)
            criteria.Add($"Location Role: {filter.LocationRole}");
        if (filter.Purpose.HasValue)
            criteria.Add($"Purpose: {filter.Purpose}");
        if (!string.IsNullOrEmpty(filter.DistrictId))
            criteria.Add($"District: {filter.DistrictId}");
        if (!string.IsNullOrEmpty(filter.RegionId))
            criteria.Add($"Region: {filter.RegionId}");

        // Route filters
        if (filter.Terrain != null)
            criteria.Add($"Terrain: {filter.Terrain}");
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
                    .Select(loc => $"  - {loc.Name}: Role={loc.Role}, Purpose={loc.Purpose}")
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
            PersonalityType = filter.PersonalityType?.ToString(),
            Profession = filter.Profession?.ToString(),
            RequiredRelationship = filter.RequiredRelationship?.ToString(),
            MinTier = filter.MinTier,
            MaxTier = filter.MaxTier,
            MinBond = filter.MinBond,
            MaxBond = filter.MaxBond,
            NpcTags = filter.NpcTags,
            // Orthogonal Categorical Dimensions - NPC
            SocialStanding = filter.SocialStanding?.ToString(),
            StoryRole = filter.StoryRole?.ToString(),
            KnowledgeLevel = filter.KnowledgeLevel?.ToString(),
            // Location filters
            Role = filter.LocationRole?.ToString(),
            IsPlayerAccessible = filter.IsPlayerAccessible,
            DistrictId = filter.DistrictId,
            RegionId = filter.RegionId,
            // Orthogonal Categorical Dimensions - Location
            Privacy = filter.Privacy?.ToString(),
            Safety = filter.Safety?.ToString(),
            Activity = filter.Activity?.ToString(),
            Purpose = filter.Purpose?.ToString(),
            // Route filters
            Terrain = filter.Terrain?.ToString(),
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
}
