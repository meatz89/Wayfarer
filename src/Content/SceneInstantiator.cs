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

            // Resolution metadata for spawn graph visualization
            EntityResolutionMetadata locationResolution = null;
            EntityResolutionMetadata npcResolution = null;
            EntityResolutionMetadata routeResolution = null;

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
                    locationResolution = EntityResolutionMetadata.ForRouteDestination();
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

                        // Build resolution metadata for created location
                        locationResolution = BuildLocationCreatedMetadata(situation.LocationFilter);
                        Console.WriteLine($"[SceneInstantiator]     ✅ CREATED Location '{location.Name}' (SceneCreated)");
                    }
                    else
                    {
                        // Build resolution metadata for discovered location
                        locationResolution = BuildLocationDiscoveredMetadata(situation.LocationFilter, location);
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

                    // Build resolution metadata for created NPC
                    npcResolution = BuildNPCCreatedMetadata(situation.NpcFilter);
                    Console.WriteLine($"[SceneInstantiator]     ✅ CREATED NPC '{npc.Name}'");
                }
                else
                {
                    // Build resolution metadata for discovered NPC
                    npcResolution = BuildNPCDiscoveredMetadata(situation.NpcFilter, npc);
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

                // Build resolution metadata for discovered route
                routeResolution = BuildRouteDiscoveredMetadata(situation.RouteFilter, route);
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
                        SituationSpawnTriggerType.InitialScene,
                        locationResolution,
                        npcResolution,
                        routeResolution
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

    // ==================== RESOLUTION METADATA BUILDERS ====================

    /// <summary>
    /// Build resolution metadata for a discovered location
    /// Tracks which filter properties matched the found location
    /// </summary>
    private EntityResolutionMetadata BuildLocationDiscoveredMetadata(PlacementFilter filter, Location location)
    {
        PlacementFilterSnapshot filterSnapshot = SnapshotFactory.CreatePlacementFilterSnapshot(filter);
        List<string> matchedProperties = new List<string>();

        if (filter.LocationRole.HasValue && location.Role == filter.LocationRole.Value)
            matchedProperties.Add("Role");
        if (filter.Privacy.HasValue && location.Privacy == filter.Privacy.Value)
            matchedProperties.Add("Privacy");
        if (filter.Safety.HasValue && location.Safety == filter.Safety.Value)
            matchedProperties.Add("Safety");
        if (filter.Activity.HasValue && location.Activity == filter.Activity.Value)
            matchedProperties.Add("Activity");
        if (filter.Purpose.HasValue && location.Purpose == filter.Purpose.Value)
            matchedProperties.Add("Purpose");

        return EntityResolutionMetadata.ForDiscovered(filterSnapshot, matchedProperties);
    }

    /// <summary>
    /// Build resolution metadata for a created location
    /// Tracks which properties came from filter vs defaults
    /// </summary>
    private EntityResolutionMetadata BuildLocationCreatedMetadata(PlacementFilter filter)
    {
        PlacementFilterSnapshot filterSnapshot = SnapshotFactory.CreatePlacementFilterSnapshot(filter);
        List<string> filterProvided = new List<string>();
        List<string> generated = new List<string>();

        // Role
        if (filter.LocationRole.HasValue)
            filterProvided.Add("Role");
        else
            generated.Add("Role");

        // Privacy
        if (filter.Privacy.HasValue)
            filterProvided.Add("Privacy");
        else
            generated.Add("Privacy");

        // Safety
        if (filter.Safety.HasValue)
            filterProvided.Add("Safety");
        else
            generated.Add("Safety");

        // Activity
        if (filter.Activity.HasValue)
            filterProvided.Add("Activity");
        else
            generated.Add("Activity");

        // Purpose
        if (filter.Purpose.HasValue)
            filterProvided.Add("Purpose");
        else
            generated.Add("Purpose");

        return EntityResolutionMetadata.ForCreated(filterSnapshot, filterProvided, generated);
    }

    /// <summary>
    /// Build resolution metadata for a discovered NPC
    /// Tracks which filter properties matched the found NPC
    /// </summary>
    private EntityResolutionMetadata BuildNPCDiscoveredMetadata(PlacementFilter filter, NPC npc)
    {
        PlacementFilterSnapshot filterSnapshot = SnapshotFactory.CreatePlacementFilterSnapshot(filter);
        List<string> matchedProperties = new List<string>();

        if (filter.Profession.HasValue && npc.Profession == filter.Profession.Value)
            matchedProperties.Add("Profession");
        if (filter.PersonalityType.HasValue && npc.PersonalityType == filter.PersonalityType.Value)
            matchedProperties.Add("PersonalityType");
        if (filter.SocialStanding.HasValue && npc.SocialStanding == filter.SocialStanding.Value)
            matchedProperties.Add("SocialStanding");
        if (filter.StoryRole.HasValue && npc.StoryRole == filter.StoryRole.Value)
            matchedProperties.Add("StoryRole");

        return EntityResolutionMetadata.ForDiscovered(filterSnapshot, matchedProperties);
    }

    /// <summary>
    /// Build resolution metadata for a created NPC
    /// Tracks which properties came from filter vs defaults
    /// </summary>
    private EntityResolutionMetadata BuildNPCCreatedMetadata(PlacementFilter filter)
    {
        PlacementFilterSnapshot filterSnapshot = SnapshotFactory.CreatePlacementFilterSnapshot(filter);
        List<string> filterProvided = new List<string>();
        List<string> generated = new List<string>();

        // Profession
        if (filter.Profession.HasValue)
            filterProvided.Add("Profession");
        else
            generated.Add("Profession");

        // PersonalityType
        if (filter.PersonalityType.HasValue)
            filterProvided.Add("PersonalityType");
        else
            generated.Add("PersonalityType");

        // SocialStanding
        if (filter.SocialStanding.HasValue)
            filterProvided.Add("SocialStanding");
        else
            generated.Add("SocialStanding");

        // StoryRole
        if (filter.StoryRole.HasValue)
            filterProvided.Add("StoryRole");
        else
            generated.Add("StoryRole");

        // Tier (from MinTier)
        if (filter.MinTier.HasValue)
            filterProvided.Add("Tier");
        else
            generated.Add("Tier");

        return EntityResolutionMetadata.ForCreated(filterSnapshot, filterProvided, generated);
    }

    /// <summary>
    /// Build resolution metadata for a discovered route
    /// Routes are always discovered (find-only, no creation)
    /// </summary>
    private EntityResolutionMetadata BuildRouteDiscoveredMetadata(PlacementFilter filter, RouteOption route)
    {
        PlacementFilterSnapshot filterSnapshot = SnapshotFactory.CreatePlacementFilterSnapshot(filter);
        List<string> matchedProperties = new List<string>();

        if (filter.Terrain.HasValue)
            matchedProperties.Add("Terrain");
        if (filter.MinDifficulty.HasValue)
            matchedProperties.Add("MinDifficulty");
        if (filter.MaxDifficulty.HasValue)
            matchedProperties.Add("MaxDifficulty");

        return EntityResolutionMetadata.ForDiscovered(filterSnapshot, matchedProperties);
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
        if (!string.IsNullOrEmpty(filter.DistrictName))
            criteria.Add($"District: {filter.DistrictName}");
        if (!string.IsNullOrEmpty(filter.RegionName))
            criteria.Add($"Region: {filter.RegionName}");

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

    // Selection strategy methods extracted to PlacementSelectionStrategies.cs
    // Use PlacementSelectionStrategies.ApplyStrategyNPC() and ApplyStrategyLocation()

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
            DistrictName = filter.DistrictName,
            RegionName = filter.RegionName,
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
