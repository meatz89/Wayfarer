using System.Text.Json;

/// <summary>
/// Orchestrates loading of game packages, delegating to specialized parsers for conversion.
/// Uses simple sequential loading in dependency order for static content.
///
/// ARCHITECTURAL PRINCIPLES:
/// - GameWorld is the SINGLE SOURCE OF TRUTH: All loaded content goes directly into GameWorld
/// - NO INTERMEDIATE STATE: Parsers convert JSON to domain objects immediately, no pass-through
/// - DEPENDENCY ORDER: Files must be numbered (01_, 02_, etc) to ensure proper loading sequence
/// - STATIC vs DYNAMIC: Static content loaded at startup (no skeletons), dynamic content supports skeletons
/// - PARSERS ARE STATELESS: Used only for conversion, then discarded - no state retention
/// </summary>
public class PackageLoader
{
    private readonly GameWorld _gameWorld;
    private bool _isFirstPackage = true;
    private List<ExchangeCard> _parsedExchangeCards; // HIGHLANDER: Object references ONLY, no wrapper class
    private string _currentPackageId = "unknown"; // Track current package for error reporting

    // Track loaded packages to prevent reloading
    private List<string> _loadedPackageIds = new List<string>();

    private readonly SceneGenerationFacade _sceneGenerationFacade;
    private readonly LocationPlayabilityValidator _locationValidator;
    private readonly HexSynchronizationService _hexSync;

    public PackageLoader(
        GameWorld gameWorld,
        SceneGenerationFacade sceneGenerationFacade,
        LocationPlayabilityValidator locationValidator,
        HexSynchronizationService hexSync)
    {
        _gameWorld = gameWorld;
        _sceneGenerationFacade = sceneGenerationFacade;
        _locationValidator = locationValidator ?? throw new ArgumentNullException(nameof(locationValidator));
        _hexSync = hexSync ?? throw new ArgumentNullException(nameof(hexSync));
    }

    /// <summary>
    /// Load static packages in alphabetical/numerical order
    /// Used at game startup for deterministic content loading
    /// </summary>
    public void LoadStaticPackages(List<string> packageFilePaths)
    {
        // Sort by filename to ensure proper loading order (01_, 02_, etc.)
        List<string> sortedPackages = packageFilePaths
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        // Load each package sequentially
        foreach (string packagePath in sortedPackages)
        {
            string packageFileName = Path.GetFileName(packagePath);
            Console.WriteLine($"[PackageLoader] Loading package: {packageFileName}");

            string json = File.ReadAllText(packagePath);
            Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            // Check if already loaded
            if (package.PackageId != null && _loadedPackageIds.Contains(package.PackageId))
            {
                continue;
            }// Track as loaded
            if (package.PackageId != null)
            {
                _loadedPackageIds.Add(package.PackageId);
            }

            // Load with no skeletons allowed for static content
            LoadPackageContent(package, allowSkeletons: false);
        }

        // HEX-BASED TRAVEL SYSTEM: Sync hex positions ONCE after all packages loaded
        // CRITICAL: Must happen after ALL packages because hex grid and locations may be in different packages
        SyncLocationHexPositions();
        EnsureHexGridCompleteness();

        // CATALOGUE PATTERN: Generate content from loaded entities (ONCE after all packages loaded)
        // Must happen AFTER all packages loaded because catalogues need complete entity lists
        GeneratePlayerActionsFromCatalogue();
        GenerateLocationActionsFromCatalogue(); // ATMOSPHERIC LAYER: Static gameplay actions (Travel/Work/Rest)
        GenerateProceduralRoutes();
        GenerateDeliveryJobsFromCatalogue();

        // PLAYABILITY VALIDATION: Validate locations AFTER routes generated
        // CRITICAL: Reachability validation requires routes to exist
        ValidateAllLocations();

        // Final validation and initialization
        ValidateCrossroadsConfiguration();
        InitializeTravelDiscoverySystem();
        InitializeObligationJournal();
    }

    /// <summary>
    /// Load package content with optional skeleton support
    /// </summary>
    private void LoadPackageContent(Package package, bool allowSkeletons)
    {
        // Set current package ID for error reporting
        if (string.IsNullOrEmpty(package.PackageId))
            throw new InvalidDataException("Package missing required field 'PackageId'");
        _currentPackageId = package.PackageId;

        // Apply starting conditions only from the first package
        if (_isFirstPackage && package.StartingConditions != null)
        {
            ApplyStartingConditions(package.StartingConditions);
            _isFirstPackage = false;
        }

        if (package.Content == null) return;

        // Load in strict dependency order
        // 1. Foundation entities (no dependencies)
        // HEX-BASED TRAVEL SYSTEM: Load hex grid FIRST - fundamental spatial scaffolding
        LoadHexMap(package.Content.HexMap, allowSkeletons);
        LoadPlayerStatsConfiguration(package.Content.PlayerStatsConfig, allowSkeletons);
        LoadListenDrawCounts(package.Content.ListenDrawCounts);
        LoadStates(package.Content.States, allowSkeletons); // Scene-Situation: State definitions
        LoadAchievements(package.Content.Achievements, allowSkeletons); // Scene-Situation: Achievement definitions
        LoadRegions(package.Content.Regions, allowSkeletons);
        LoadDistricts(package.Content.Districts, allowSkeletons);
        LoadItems(package.Content.Items, allowSkeletons);

        // 2. Venues and Locations (may reference regions/districts and hex grid)
        LoadLocations(package.Content.Venues, allowSkeletons);
        LoadLocations(package.Content.Locations, allowSkeletons);
        // NOTE: HexSync, HexGridCompleteness, and Validation moved to LoadStaticPackages
        // These must run AFTER all packages loaded (hex grid might be in different package than locations)

        // NOTE: Action/route/job generation moved to LoadStaticPackages() - runs ONCE after all packages loaded
        // CATALOGUE PATTERN: Generate actions from categorical properties (PARSE TIME ONLY)

        // 3. Cards (foundation for NPCs and conversations)
        LoadSocialCards(package.Content.SocialCards, allowSkeletons);
        LoadMentalCards(package.Content.MentalCards, allowSkeletons);
        LoadPhysicalCards(package.Content.PhysicalCards, allowSkeletons);

        // THREE PARALLEL TACTICAL CHALLENGE SYSTEMS - Decks only, no Types
        LoadSocialChallengeDecks(package.Content.SocialChallengeDecks, allowSkeletons);
        LoadMentalChallengeDecks(package.Content.MentalChallengeDecks, allowSkeletons);
        LoadPhysicalChallengeDecks(package.Content.PhysicalChallengeDecks, allowSkeletons);

        // 3.5 Obligation Templates (strategic multi-phase activities)
        LoadObligations(package.Content.Obligations, allowSkeletons);
        LoadSituations(package.Content.Situations, allowSkeletons);
        LoadSceneTemplates(package.Content.SceneTemplates, allowSkeletons); // NEW: Scene-Situation architecture templates
        LoadScenes(package.Content.Scenes, allowSkeletons); // NEW: Scene runtime instances (dynamically spawned)

        // 3.6 Screen Expansion Systems (conversation trees, observation scenes, emergencies)
        LoadConversationTrees(package.Content.ConversationTrees, allowSkeletons);
        LoadObservationScenes(package.Content.ObservationScenes, allowSkeletons);
        LoadEmergencySituations(package.Content.EmergencySituations, allowSkeletons);

        // 4. NPCs (reference locations, Locations, and cards)
        LoadNPCs(package.Content.Npcs, allowSkeletons);
        LoadStrangers(package.Content.Strangers, allowSkeletons);

        // 5. Routes (reference Locations which now have VenueId set)
        LoadRoutes(package.Content.Routes, allowSkeletons);
        // NOTE: Route generation and job generation moved to LoadStaticPackages() - runs ONCE after all packages loaded

        // 6. Relationship entities (depend on NPCs and cards)
        LoadExchanges(package.Content.Exchanges, allowSkeletons);
        InitializeNPCExchangeDecks(package.Content.DeckCompositions);

        // 7. Complex entities
        LoadDialogueTemplates(package.Content.DialogueTemplates, allowSkeletons);
        LoadStandingObligations(package.Content.StandingObligations, allowSkeletons);
        // NOTE: LocationActions and PlayerActions NO LONGER loaded from JSON
        // Actions are now GENERATED from catalogues at parse time (see GeneratePlayerActionsFromCatalogue/GenerateLocationActionsFromCatalogue)
        // CATALOGUE PATTERN: Actions generated from categorical properties, never from JSON

        // 8. Travel content - HIGHLANDER: Object references ONLY, no wrapper classes
        List<PathCardDTO> pathCardLookup = LoadPathCards(package.Content.PathCards, allowSkeletons);
        List<PathCardDTO> eventCardLookup = LoadEventCards(package.Content.EventCards, allowSkeletons);
        LoadTravelEvents(package.Content.TravelEvents, eventCardLookup, allowSkeletons);
        LoadEventCollections(package.Content.PathCardCollections, pathCardLookup, eventCardLookup, allowSkeletons);

        // 9. V2 Obligation and Travel Systems
        LoadTravelScenes(package.Content.TravelScenes, allowSkeletons);
    }

    /// <summary>
    /// Load all packages from a directory with proper ordering
    /// </summary>
    public void LoadPackagesFromDirectory(string directoryPath)
    {
        List<string> packageFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories)
            .ToList();

        // Use simple static loading for game start
        LoadStaticPackages(packageFiles);
    }

    /// <summary>
    /// Load a dynamic package at runtime (e.g., AI-generated content)
    /// Returns list of skeleton IDs that need completion
    /// </summary>
    public List<string> LoadDynamicPackage(string packageFilePath)
    {
        string json = File.ReadAllText(packageFilePath);
        Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });

        // Check if already loaded
        if (package.PackageId != null && _loadedPackageIds.Contains(package.PackageId))
        {
            return new List<string>();
        }

        // Track as loaded
        if (package.PackageId != null)
        {
            _loadedPackageIds.Add(package.PackageId);
        }

        // Load with skeletons allowed for dynamic content
        LoadPackageContent(package, allowSkeletons: true);

        // Return skeleton IDs for AI completion
        return _gameWorld.SkeletonRegistry.Select(r => r.SkeletonKey).ToList();
    }

    /// <summary>
    /// Load a dynamic package from JSON string (e.g., AI-generated content)
    /// Returns list of skeleton IDs that need completion
    /// </summary>
    public async Task<List<string>> LoadDynamicPackageFromJson(string json, string packageId)
    {
        Package package;
        using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
        {
            package = await JsonSerializer.DeserializeAsync<Package>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            });
        }

        // Set package ID if not present
        if (string.IsNullOrEmpty(package.PackageId))
        {
            if (string.IsNullOrEmpty(packageId))
                throw new InvalidDataException("Package and packageId parameter both missing - cannot identify package");
            package.PackageId = packageId;
        }

        // Check if already loaded
        if (_loadedPackageIds.Contains(package.PackageId))
        {
            return new List<string>();
        }

        // Track as loaded
        _loadedPackageIds.Add(package.PackageId);

        // Load with skeletons allowed for dynamic content
        LoadPackageContent(package, allowSkeletons: true);

        // Regenerate static location actions for dynamic packages
        // Dynamic packages may add locations that need intra-venue movement actions
        // Must regenerate for ALL locations because adjacency relationships may have changed
        GenerateLocationActionsFromCatalogue();

        // Return skeleton IDs for AI completion
        return _gameWorld.SkeletonRegistry.Select(r => r.SkeletonKey).ToList();
    }

    private void ApplyStartingConditions(PackageStartingConditions conditions)
    {
        // Apply player initial config
        if (conditions.PlayerConfig != null)
        {
            // Parse DTO (categorical properties) → Domain Entity (concrete values)
            PlayerInitialConfig parsedConfig = PlayerInitialConfigParser.Parse(conditions.PlayerConfig);
            _gameWorld.InitialPlayerConfig = parsedConfig;
            // Apply the initial configuration to the player immediately
            _gameWorld.ApplyInitialPlayerConfiguration();
        }

        // Set starting location - CRITICAL for playability
        if (string.IsNullOrEmpty(conditions.StartingSpotId))
            throw new InvalidOperationException("StartingSpotId is required in starting conditions - player has no spawn location!");

        // Validate starting location exists in parsed locations
        // HIGHLANDER: LINQ query on already-parsed locations
        Location startingLocation = _gameWorld.Locations.FirstOrDefault(l => l.Name == conditions.StartingSpotId);
        if (startingLocation == null)
            throw new InvalidOperationException($"StartingSpotId '{conditions.StartingSpotId}' not found in parsed locations - player cannot spawn!");


        // Apply starting obligations
        if (conditions.StartingObligations != null)
        {
            foreach (StandingObligationDTO obligationDto in conditions.StartingObligations)
            {
                // Convert DTO to domain model and add to player's standing obligations
                StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(obligationDto, _gameWorld);
                _gameWorld.GetPlayer().StandingObligations.Add(obligation);
            }
        }

        // Apply starting token relationships
        if (conditions.StartingTokens != null)
        {
            foreach (KeyValuePair<string, NPCTokenRelationship> kvp in conditions.StartingTokens)
            {
                // Token relationships will be applied when NPCs are loaded
                // Store for later application
                NPCTokenEntry tokenEntry = _gameWorld.GetPlayer().GetNPCTokenEntry(kvp.Key);
                tokenEntry.Trust = kvp.Value.Trust;
                tokenEntry.Diplomacy = kvp.Value.Diplomacy;
                tokenEntry.Status = kvp.Value.Status;
                tokenEntry.Shadow = kvp.Value.Shadow;
            }
        }

        // Store time initialization for TimeModel (applied after DI initialization)
        if (conditions.StartingDay.HasValue)
        {
            _gameWorld.InitialDay = conditions.StartingDay.Value;
        }

        if (!string.IsNullOrEmpty(conditions.StartingTimeBlock))
        {
            if (Enum.TryParse<TimeBlocks>(conditions.StartingTimeBlock, out TimeBlocks timeBlock))
            {
                _gameWorld.InitialTimeBlock = timeBlock;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid StartingTimeBlock '{conditions.StartingTimeBlock}'. " +
                    $"Valid values: Morning, Midday, Afternoon, Evening");
            }
        }

        if (conditions.StartingSegment.HasValue)
        {
            _gameWorld.InitialSegment = conditions.StartingSegment.Value;
        }
    }

    /// <summary>
    /// Generate universal player actions from PlayerActionCatalog (PARSE TIME ONLY)
    /// Called ONCE after all locations loaded, not per location
    /// CATALOGUE PATTERN: Actions generated from categorical properties, never from JSON
    /// </summary>
    private void GeneratePlayerActionsFromCatalogue()
    {
        // Generate universal player actions (CheckBelongings, Wait, SleepOutside)
        List<PlayerAction> playerActions = PlayerActionCatalog.GenerateUniversalActions();

        // Add to GameWorld (check for duplicates if multiple packages)
        foreach (PlayerAction action in playerActions)
        {
            // Avoid duplicates if multiple packages loaded (object equality check)
            if (!_gameWorld.PlayerActions.Any(a => a == action))
            {
                _gameWorld.PlayerActions.Add(action);
            }
        }
    }

    /// <summary>
    /// Generate location actions from LocationActionCatalog (PARSE TIME ONLY)
    /// Called ONCE after all locations loaded (needs complete location list for intra-venue movement)
    /// CATALOGUE PATTERN: Actions generated from categorical properties, never from JSON
    /// </summary>
    private void GenerateLocationActionsFromCatalogue()
    {
        // Get all locations for intra-venue movement calculation
        List<Location> allLocations = _gameWorld.Locations.ToList();

        // Generate actions for each location based on its categorical properties
        foreach (Location location in allLocations)
        {
            List<LocationAction> generatedActions = LocationActionCatalog.GenerateActionsForLocation(
                location,
                allLocations // Pass all locations for intra-venue movement calculation
            );

            // Add generated actions to GameWorld
            foreach (LocationAction action in generatedActions)
            {
                // Avoid duplicates if multiple packages loaded (semantic deduplication via object references)
                bool isDuplicate = _gameWorld.LocationActions.Any(a =>
                    a.ActionType == action.ActionType &&
                    a.SourceLocation == action.SourceLocation &&
                    a.DestinationLocation == action.DestinationLocation);

                if (!isDuplicate)
                {
                    _gameWorld.LocationActions.Add(action);
                }
            }
        }
    }

    /// <summary>
    /// Generate delivery jobs from DeliveryJobCatalog (PARSE TIME ONLY)
    /// Called ONCE after routes and locations loaded
    /// CATALOGUE PATTERN: Jobs generated procedurally from routes, never from JSON
    /// </summary>
    private void GenerateDeliveryJobsFromCatalogue()
    {
        // Get all routes and locations
        List<RouteOption> allRoutes = _gameWorld.Routes.ToList();
        List<Location> allLocations = _gameWorld.Locations.ToList();

        // Generate delivery jobs from routes
        List<DeliveryJob> generatedJobs = DeliveryJobCatalog.GenerateJobsFromRoutes(allRoutes, allLocations);

        // Add generated jobs to GameWorld
        foreach (DeliveryJob job in generatedJobs)
        {
            // Avoid duplicates if multiple packages loaded (object equality check)
            if (!_gameWorld.AvailableDeliveryJobs.Any(j => j == job))
            {
                _gameWorld.AvailableDeliveryJobs.Add(job);
            }
        }
    }

    private void LoadPlayerStatsConfiguration(PlayerStatsConfigDTO playerStatsConfig, bool allowSkeletons)
    {
        if (playerStatsConfig == null) return;

        // Parse the player stats configuration using PlayerStatParser
        PlayerStatsParseResult parseResult = PlayerStatParser.ParseStatsPackage(playerStatsConfig);

        // Store the configuration in GameWorld
        _gameWorld.PlayerStatDefinitions = parseResult.StatDefinitions;
        _gameWorld.StatProgression = parseResult.Progression;
    }

    private void LoadListenDrawCounts(Dictionary<string, int> listenDrawCounts)
    {
        if (listenDrawCounts == null) return;

        // Convert string keys to ConnectionState enum and create ListenDrawCountEntry list
        List<ListenDrawCountEntry> drawCountEntries = new List<ListenDrawCountEntry>();

        foreach (KeyValuePair<string, int> kvp in listenDrawCounts)
        {
            // Parse connection state from string key
            if (Enum.TryParse<ConnectionState>(kvp.Key.ToUpper(), out ConnectionState state))
            {
                drawCountEntries.Add(new ListenDrawCountEntry
                {
                    State = state,
                    DrawCount = kvp.Value
                });
            }
            else
            {
                throw new Exception($"[PackageLoader] Invalid connection state in listenDrawCounts: '{kvp.Key}'");
            }
        }

        // Apply to GameRules
        GameRules.StandardRuleset.ListenDrawCounts = drawCountEntries;
    }

    private void LoadStrangers(List<StrangerNPCDTO> strangerDtos, bool allowSkeletons)
    {
        if (strangerDtos == null) return;

        foreach (StrangerNPCDTO dto in strangerDtos)
        {
            // Convert DTO to domain model using StrangerParser
            NPC stranger = StrangerParser.ConvertDTOToNPC(dto, _gameWorld);

            // Add stranger to the unified NPCs list
            _gameWorld.NPCs.Add(stranger);
        }
    }

    /// <summary>
    /// Load hex map grid - fundamental spatial scaffolding
    /// HEX-BASED TRAVEL SYSTEM: Loads world hex grid with terrain, danger, and location placement
    /// </summary>
    private void LoadHexMap(HexMapDTO hexMapDto, bool allowSkeletons)
    {
        if (hexMapDto == null) return;

        // Parse hex map using HexParser
        HexMap hexMap = HexParser.ParseHexMap(hexMapDto);

        // Store in GameWorld
        _gameWorld.WorldHexGrid = hexMap;
    }

    /// <summary>
    /// Sync Location.HexPosition from hex grid after locations loaded
    /// HEX-BASED TRAVEL SYSTEM: HIGHLANDER synchronization - Location.HexPosition is source of truth
    /// </summary>
    private void SyncLocationHexPositions()
    {
        Console.WriteLine("[HexSync] SyncLocationHexPositions called");
        Console.WriteLine($"[HexSync] WorldHexGrid is null: {_gameWorld.WorldHexGrid == null}");
        if (_gameWorld.WorldHexGrid != null)
        {
            Console.WriteLine($"[HexSync] WorldHexGrid.Hexes.Count: {_gameWorld.WorldHexGrid.Hexes.Count}");
        }
        Console.WriteLine($"[HexSync] Locations.Count: {_gameWorld.Locations.Count}");

        if (_gameWorld.WorldHexGrid == null || _gameWorld.WorldHexGrid.Hexes.Count == 0)
        {
            Console.WriteLine("[HexSync] ⚠️ Skipping hex sync - no hex grid loaded");
            return; // No hex grid loaded, skip sync
        }

        // Sync Location.HexPosition (source of truth) with Hex.LocationId (derived lookup)
        HexParser.SyncLocationHexPositions(_gameWorld.WorldHexGrid, _gameWorld.Locations);
    }

    /// <summary>
    /// Ensure hex grid completeness - create hexes for positioned locations without them
    /// HOLISTIC FIX: Maintains invariant "every positioned location has a hex"
    /// Critical for dependent locations generated by scenes at runtime
    /// </summary>
    private void EnsureHexGridCompleteness()
    {
        Console.WriteLine("[HexGridCompleteness] Method called");

        if (_gameWorld.WorldHexGrid == null)
        {
            Console.WriteLine("[HexGridCompleteness] WorldHexGrid is null, returning early");
            return;
        }

        Console.WriteLine($"[HexGridCompleteness] Processing {_gameWorld.Locations.Count} locations");
        HexParser.EnsureHexGridCompleteness(_gameWorld.WorldHexGrid, _gameWorld.Locations);
    }

    /// <summary>
    /// Validate all locations for playability after hex positions are synced
    /// CRITICAL: Must run AFTER SyncLocationHexPositions() and EnsureHexGridCompleteness()
    /// Ensures all locations meet playability requirements (hex position, venue, properties, etc.)
    /// </summary>
    private void ValidateAllLocations()
    {
        foreach (Location location in _gameWorld.Locations)
        {
            _locationValidator.ValidateLocation(location, _gameWorld);
        }
    }

    /// <summary>
    /// Generate procedural routes between locations using hex-based pathfinding
    /// HEX-BASED TRAVEL SYSTEM: Creates routes from hex paths with calculated properties
    /// Only generates routes between locations in DIFFERENT venues (same venue = instant travel)
    /// </summary>
    private void GenerateProceduralRoutes()
    {
        if (_gameWorld.WorldHexGrid == null || _gameWorld.WorldHexGrid.Hexes.Count == 0)
            return; // No hex grid, skip procedural generation

        // Create route generator
        HexRouteGenerator generator = new HexRouteGenerator(_gameWorld);

        // Generate all inter-venue routes
        List<RouteOption> generatedRoutes = generator.GenerateAllRoutes();

        // Add generated routes to GameWorld, avoiding duplicates with manually-authored routes
        int addedCount = 0;
        int skippedCount = 0;
        foreach (RouteOption route in generatedRoutes)
        {
            // Check if route already exists (manually-authored routes take precedence)
            // Use Name as natural key for route identity
            bool routeExists = _gameWorld.Routes.Any(r => r.Name == route.Name);
            if (!routeExists)
            {
                _gameWorld.Routes.Add(route);
                addedCount++;
            }
            else
            {
                skippedCount++;
            }
        }
    }

    private void LoadRegions(List<RegionDTO> regionDtos, bool allowSkeletons)
    {
        if (regionDtos == null) return;

        foreach (RegionDTO dto in regionDtos)
        {
            Region region = new Region
            {
                // Region uses Name as natural key (no Id property)
                Name = dto.Name,
                Description = dto.Description,
                DistrictIds = dto.DistrictIds,
                Tier = dto.Tier,
                Government = dto.Government,
                Culture = dto.Culture,
                Population = dto.Population,
                MajorExports = dto.MajorExports,
                MajorImports = dto.MajorImports
            };
            _gameWorld.Regions.Add(region);
        }
    }

    private void LoadDistricts(List<DistrictDTO> districtDtos, bool allowSkeletons)
    {
        if (districtDtos == null) return;

        foreach (DistrictDTO dto in districtDtos)
        {
            District district = new District
            {
                // District uses Name as natural key (no Id property)
                Name = dto.Name,
                Description = dto.Description,
                RegionId = dto.RegionId,
                VenueIds = dto.VenueIds,
                DistrictType = dto.DistrictType,
                DangerLevel = dto.DangerLevel,
                Characteristics = dto.Characteristics
            };
            _gameWorld.Districts.Add(district);
        }
    }

    private void LoadSocialCards(List<SocialCardDTO> cardDtos, bool allowSkeletons)
    {
        if (cardDtos == null) return;

        foreach (SocialCardDTO dto in cardDtos)
        {
            try
            {
                // Use static method from ConversationCardParser
                SocialCard card = SocialCardParser.ParseCard(dto);
                _gameWorld.SocialCards.Add(card);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"FATAL: Failed to parse social card '{dto.Id}'. " +
                    $"Total cards loaded so far: {_gameWorld.SocialCards.Count}. " +
                    $"Error: {ex.Message}", ex);
            }
        }

        // Validate Foundation card rules after all cards are loaded
        List<SocialCard> allCards = _gameWorld.SocialCards.ToList();
        SocialCardParser.ValidateFoundationCardRules(allCards);
    }

    private void LoadMentalCards(List<MentalCardDTO> mentalCards, bool allowSkeletons)
    {
        if (mentalCards == null) return;

        MentalCardParser parser = new MentalCardParser();
        foreach (MentalCardDTO dto in mentalCards)
        {
            MentalCard card = parser.ParseCard(dto);
            _gameWorld.MentalCards.Add(card);
        }
    }

    private void LoadPhysicalCards(List<PhysicalCardDTO> physicalCards, bool allowSkeletons)
    {
        if (physicalCards == null) return;

        PhysicalCardParser parser = new PhysicalCardParser();
        foreach (PhysicalCardDTO dto in physicalCards)
        {
            PhysicalCard card = parser.ParseCard(dto);
            _gameWorld.PhysicalCards.Add(card);
        }
    }

    private void LoadSocialChallengeDecks(List<SocialChallengeDeckDTO> decks, bool allowSkeletons)
    {
        if (decks == null) return;

        foreach (SocialChallengeDeckDTO dto in decks)
        {
            SocialChallengeDeck deck = dto.ToDomain();

            // VALIDATION: Verify every card in deck exists in GameWorld.SocialCards
            // FAIL FAST at initialization, not at runtime when player clicks BEGIN CHALLENGE
            foreach (string cardId in deck.CardIds)
            {
                bool cardExists = _gameWorld.SocialCards.Any(c => c.Id == cardId);
                if (!cardExists)
                {
                    int totalCards = _gameWorld.SocialCards.Count;
                    string allCardIds = string.Join(", ", _gameWorld.SocialCards.Select(c => c.Id));
                    throw new InvalidOperationException(
                        $"Social deck '{deck.Id}' references missing card '{cardId}'. " +
                        $"Ensure card is defined in Content/Core/08_social_cards.json and loads before deck. " +
                        $"Total cards loaded: {totalCards}. " +
                        $"All loaded card IDs: {allCardIds}");
                }
            }

            _gameWorld.SocialChallengeDecks.Add(deck);
        }
    }

    private void LoadMentalChallengeDecks(List<MentalChallengeDeckDTO> decks, bool allowSkeletons)
    {
        if (decks == null) return;

        foreach (MentalChallengeDeckDTO dto in decks)
        {
            MentalChallengeDeck deck = dto.ToDomain();

            // VALIDATION: Verify every card in deck exists in GameWorld.MentalCards
            // FAIL FAST at initialization, not at runtime when player clicks BEGIN CHALLENGE
            foreach (string cardId in deck.CardIds)
            {
                bool cardExists = _gameWorld.MentalCards.Any(c => c.Id == cardId);
                if (!cardExists)
                {
                    throw new InvalidOperationException(
                        $"Mental deck '{deck.Id}' references missing card '{cardId}'. " +
                        $"Ensure card is defined in Content/Core/09_mental_cards.json and loads before deck. " +
                        $"Available cards: {string.Join(", ", _gameWorld.MentalCards.Take(5).Select(c => c.Id))}...");
                }
            }

            _gameWorld.MentalChallengeDecks.Add(deck);
        }
    }

    private void LoadPhysicalChallengeDecks(List<PhysicalChallengeDeckDTO> decks, bool allowSkeletons)
    {
        if (decks == null) return;

        foreach (PhysicalChallengeDeckDTO dto in decks)
        {
            PhysicalChallengeDeck deck = dto.ToDomain();

            // VALIDATION: Verify every card in deck exists in GameWorld.PhysicalCards
            // FAIL FAST at initialization, not at runtime when player clicks BEGIN CHALLENGE
            foreach (string cardId in deck.CardIds)
            {
                bool cardExists = _gameWorld.PhysicalCards.Any(c => c.Id == cardId);
                if (!cardExists)
                {
                    throw new InvalidOperationException(
                        $"Physical deck '{deck.Id}' references missing card '{cardId}'. " +
                        $"Ensure card is defined in Content/Core/10_physical_cards.json and loads before deck. " +
                        $"Available cards: {string.Join(", ", _gameWorld.PhysicalCards.Take(5).Select(c => c.Id))}...");
                }
            }

            _gameWorld.PhysicalChallengeDecks.Add(deck);
        }
    }

    private void LoadObligations(List<ObligationDTO> obligations, bool allowSkeletons)
    {
        if (obligations == null) return;

        ObligationParser parser = new ObligationParser(_gameWorld);
        foreach (ObligationDTO dto in obligations)
        {
            Obligation obligation = parser.ParseObligation(dto);
            _gameWorld.Obligations.Add(obligation);
        }
    }

    private void LoadSituations(List<SituationDTO> situationDtos, bool allowSkeletons)
    {
        // LEGACY CODE PATH - DEPRECATED
        // Standalone situations no longer supported
        // All situations are owned by Scenes and created by SceneInstantiator
        // If a package contains situations, they will be ignored
        if (situationDtos != null && situationDtos.Any())
        {
            Console.WriteLine($"[PackageLoader] WARNING: Package contains {situationDtos.Count} standalone situations - these are IGNORED in new architecture. Situations must be part of SceneTemplates.");
        }
    }

    // NOTE: LoadScenes method removed - OLD equipment-based Scene system deleted
    // NEW Scene-Situation architecture:
    // - Scenes spawn via SceneSpawnReward (not package-level definitions)
    // - Scene templates live in GameWorld.SceneTemplates
    // - Scene instances live in GameWorld.Scenes (spawned dynamically)

    private void LoadLocations(List<VenueDTO> venueDtos, bool allowSkeletons)
    {
        if (venueDtos == null) return;

        foreach (VenueDTO dto in venueDtos)
        {
            // Check if this venue was previously a skeleton - UPDATE IN-PLACE (never remove)
            Venue? existing = _gameWorld.Venues
                .FirstOrDefault(v => v.Name == dto.Name);

            if (existing != null)
            {
                // UPDATE existing venue properties in-place (preserve object identity)
                existing.Name = dto.Name;
                existing.Description = dto.Description;
                existing.District = dto.DistrictId;
                existing.Tier = dto.Tier;

                // Parse LocationType to VenueType enum
                VenueType venueType = VenueType.Wilderness;
                if (!string.IsNullOrEmpty(dto.LocationType))
                {
                    if (!Enum.TryParse<VenueType>(dto.LocationType, true, out venueType))
                    {
                        throw new InvalidDataException($"Venue '{dto.Name}' has invalid LocationType '{dto.LocationType}'. Valid values: {string.Join(", ", Enum.GetNames(typeof(VenueType)))}");
                    }
                }
                existing.Type = venueType;

                // LocationIds: No longer stored on Venue (unidirectional Location → Venue relationship)
                // Locations reference their parent Venue via VenueId property
                // dto.locations field is ignored during venue updates

                existing.IsSkeleton = false;

                // Remove from skeleton registry if present
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FirstOrDefault(x => x.SkeletonKey == dto.Name);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }
            }
            else
            {
                // New venue - add to collection
                Venue venue = VenueParser.ConvertDTOToVenue(dto);
                _gameWorld.Venues.Add(venue);
            }
        }
    }

    private void LoadLocations(List<LocationDTO> spotDtos, bool allowSkeletons)
    {
        if (spotDtos == null) return;

        foreach (LocationDTO dto in spotDtos)
        {
            // Remove skeleton registry entry if location was skeleton (cleanup before update)
            // HIGHLANDER: LINQ query on already-parsed locations
            Location? existingSkeleton = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.Name);
            if (existingSkeleton != null && existingSkeleton.IsSkeleton)
            {
                SkeletonRegistryEntry? spotSkeletonEntry = _gameWorld.SkeletonRegistry.FirstOrDefault(x => x.SkeletonKey == dto.Name);
                if (spotSkeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(spotSkeletonEntry);
                }
            }

            Location location = LocationParser.ConvertDTOToLocation(dto, _gameWorld);

            // CAPACITY VALIDATION: Check venue capacity for ALL locations (authored + generated)
            // Generated: Checked BEFORE DTO creation in SceneInstantiator
            // Authored: Checked AFTER parsing here
            // Ensures capacity model applies uniformly (Catalogue Pattern compliance)
            // NO LOCK NEEDED: Blazor Server is single-threaded (07_deployment_view.md line 26)
            if (location.Venue != null)
            {
                if (!_gameWorld.CanVenueAddMoreLocations(location.Venue))
                {
                    // HIGHLANDER: Pass Venue object, not string
                    int currentCount = _gameWorld.GetLocationCountInVenue(location.Venue);
                    throw new InvalidOperationException(
                        $"Venue '{location.Venue.Name}' has reached capacity " +
                        $"({currentCount}/{location.Venue.MaxLocations} locations). " +
                        $"Cannot add location '{location.Name}'. " +
                        $"Increase MaxLocations in venue definition or reduce authored locations.");
                }

                // POST-PARSING INTEGRATION: Skip validation here - happens after hex sync
                // Validation moved to after SyncLocationHexPositions() call (line 145)
                // Reason: Validator requires Location.HexPosition which is set during hex sync

                // Synchronize hex reference (for ALL locations)
                _hexSync.SyncLocationToHex(location, _gameWorld);

                // AddOrUpdateLocation handles skeleton replacement via in-place property updates
                _gameWorld.AddOrUpdateLocation(location.Name, location);
            }
            else
            {
                // Location has no venue (shouldn't happen, but handle gracefully)
                // Skip validation - happens after hex sync
                _hexSync.SyncLocationToHex(location, _gameWorld);
                _gameWorld.AddOrUpdateLocation(location.Name, location);
            }
        }
    }

    private void LoadNPCs(List<NPCDTO> npcDtos, bool allowSkeletons)
    {
        if (npcDtos == null) return;

        // EntityResolver for categorical entity resolution (DDR-006)
        Player player = _gameWorld.GetPlayer();
        SceneNarrativeService narrativeService = new SceneNarrativeService(_gameWorld);
        EntityResolver entityResolver = new EntityResolver(_gameWorld, player, narrativeService);

        foreach (NPCDTO dto in npcDtos)
        {
            // Check if this NPC already exists - UPDATE IN-PLACE (never remove)
            NPC? existing = _gameWorld.NPCs.FirstOrDefault(n => n.Name == dto.Name);

            if (existing != null)
            {
                // Preserve persistent deck cards (player-earned cards must not be lost)
                List<ExchangeCard> preservedExchangeCards = existing.ExchangeDeck.ToList();

                // Parse full NPC from DTO to get updated properties
                NPC parsed = NPCParser.ConvertDTOToNPC(dto, _gameWorld, entityResolver);

                // UPDATE existing NPC properties in-place (preserve object identity)
                existing.Name = parsed.Name;
                existing.Description = parsed.Description;
                existing.Role = parsed.Role;
                existing.Profession = parsed.Profession;
                existing.Tier = parsed.Tier;
                existing.Level = parsed.Level;
                existing.ConversationDifficulty = parsed.ConversationDifficulty;
                existing.PersonalityDescription = parsed.PersonalityDescription;
                existing.PersonalityType = parsed.PersonalityType;
                existing.ConversationModifier = parsed.ConversationModifier;
                existing.Location = parsed.Location; // Resolved via EntityResolver.FindOrCreate
                existing.IsSkeleton = false;

                // PRESERVE ExchangeDeck: Merge authored initial cards + preserved runtime cards
                existing.ExchangeDeck.Clear();
                existing.ExchangeDeck.AddRange(parsed.ExchangeDeck); // Authored initial cards
                existing.ExchangeDeck.AddRange(preservedExchangeCards); // Runtime cards preserved

                // Remove from skeleton registry if present
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FirstOrDefault(x => x.SkeletonKey == dto.Name);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }
            }
            else
            {
                // New NPC - add to collection
                NPC npc = NPCParser.ConvertDTOToNPC(dto, _gameWorld, entityResolver);
                _gameWorld.NPCs.Add(npc);
            }
        }
    }

    private void LoadRoutes(List<RouteDTO> routeDtos, bool allowSkeletons)
    {
        if (routeDtos == null)
        {
            return;
        }

        // Check for missing Locations and handle based on allowSkeletons
        foreach (RouteDTO dto in routeDtos)
        {
            // Check origin location
            // HIGHLANDER: LINQ query on already-parsed locations
            Location originSpot = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.OriginSpotId);
            if (originSpot == null)
            {
                if (allowSkeletons)
                {
                    if (string.IsNullOrEmpty(dto.OriginVenueId))
                        throw new InvalidDataException($"Route '{dto.Name}' missing OriginVenueId - cannot create skeleton origin location");

                    // Create skeleton location with crossroads property (required for routes)
                    originSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.OriginSpotId,
                        dto.OriginVenueId,
                        $"route_{dto.Name}_origin"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!originSpot.LocationProperties.Contains(LocationPropertyType.Crossroads))
                    {
                        originSpot.LocationProperties.Add(LocationPropertyType.Crossroads);
                    }

                    _gameWorld.AddOrUpdateLocation(dto.OriginSpotId, originSpot);
                    _gameWorld.AddSkeleton(dto.OriginSpotId, "Location");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Name}' references missing origin location '{dto.OriginSpotId}'. Ensure Locations are loaded before routes.");
                }
            }

            // Check destination location
            // HIGHLANDER: LINQ query on already-parsed locations
            Location destSpot = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.DestinationSpotId);
            if (destSpot == null)
            {
                if (allowSkeletons)
                {
                    if (string.IsNullOrEmpty(dto.DestinationVenueId))
                        throw new InvalidDataException($"Route '{dto.Name}' missing DestinationVenueId - cannot create skeleton destination location");

                    // Create skeleton location with crossroads property (required for routes)
                    destSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.DestinationSpotId,
                        dto.DestinationVenueId,
                        $"route_{dto.Name}_destination"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!destSpot.LocationProperties.Contains(LocationPropertyType.Crossroads))
                    {
                        destSpot.LocationProperties.Add(LocationPropertyType.Crossroads);
                    }

                    _gameWorld.AddOrUpdateLocation(dto.DestinationSpotId, destSpot);
                    _gameWorld.AddSkeleton(dto.DestinationSpotId, "Location");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Name}' references missing destination location '{dto.DestinationSpotId}'. Ensure Locations are loaded before routes.");
                }
            }
        }

        // BIDIRECTIONAL ROUTE PRINCIPLE: Routes are defined once in JSON but automatically
        // generate both directions. This eliminates redundancy and ensures consistency.
        // The return journey has segments in reversed order (A->B->C becomes C->B->A).
        // Routes can opt-out via CreateBidirectional=false for internal venue navigation.
        foreach (RouteDTO dto in routeDtos)
        {
            // Create the forward route from JSON
            RouteOption forwardRoute = ConvertRouteDTOToModel(dto);
            _gameWorld.Routes.Add(forwardRoute);// Automatically generate the reverse route if CreateBidirectional is true
            if (dto.CreateBidirectional)
            {
                RouteOption reverseRoute = GenerateReverseRoute(forwardRoute);
                _gameWorld.Routes.Add(reverseRoute);
            }
            else
            { }
        }
    }

    // HIGHLANDER: Accept Location object instead of string LocationId
    private string GetVenueIdFromSpot(Location location)
    {
        if (location == null)
            throw new ArgumentNullException(nameof(location));
        if (location.Venue == null)
            throw new InvalidDataException($"Location '{location.Name}' has no Venue assigned");
        return location.Venue.Name;
    }

    private void LoadDialogueTemplates(DialogueTemplates dialogueTemplates, bool allowSkeletons)
    {
        if (dialogueTemplates == null) return; _gameWorld.DialogueTemplates = dialogueTemplates;
    }

    /// <summary>
    /// Load path cards - HIGHLANDER: Object references ONLY, no wrapper classes
    /// </summary>
    private List<PathCardDTO> LoadPathCards(List<PathCardDTO> pathCardDtos, bool allowSkeletons)
    {
        if (pathCardDtos == null) return new List<PathCardDTO>();
        return pathCardDtos;
    }

    /// <summary>
    /// Load event cards - HIGHLANDER: Object references ONLY, no wrapper classes
    /// </summary>
    private List<PathCardDTO> LoadEventCards(List<PathCardDTO> eventCardDtos, bool allowSkeletons)
    {
        if (eventCardDtos == null) return new List<PathCardDTO>();
        return eventCardDtos;
    }

    /// <summary>
    /// Load travel events - HIGHLANDER: Object references ONLY, lookup by Name not wrapper ID
    /// </summary>
    private void LoadTravelEvents(List<TravelEventDTO> travelEventDtos, List<PathCardDTO> eventCardLookup, bool allowSkeletons)
    {
        if (travelEventDtos == null) return;

        foreach (TravelEventDTO dto in travelEventDtos)
        {
            // Embed actual event cards if this event has event card IDs (for JSON loaded events)
            if (dto.EventCardIds != null && dto.EventCards.Count == 0)
            {
                foreach (string cardId in dto.EventCardIds)
                {
                    PathCardDTO? eventCard = eventCardLookup.FirstOrDefault(e => e.Name == cardId);
                    if (eventCard != null)
                    {
                        dto.EventCards.Add(eventCard);
                    }
                }
            }

            // ADR-007: EventId property deleted - use TravelEvent.Id instead
            _gameWorld.AllTravelEvents.Add(new TravelEventEntry { TravelEvent = dto });
        }
    }

    /// <summary>
    /// Load event collections - HIGHLANDER: Object references ONLY, lookup by Name not wrapper ID
    /// </summary>
    private void LoadEventCollections(List<PathCardCollectionDTO> collectionDtos, List<PathCardDTO> pathCardLookup, List<PathCardDTO> eventCardLookup, bool allowSkeletons)
    {
        if (collectionDtos == null) return;

        foreach (PathCardCollectionDTO dto in collectionDtos)
        {
            // VALIDATION: Fail fast if required 'id' field is missing (common error: using "collectionId" instead of "id")
            if (string.IsNullOrEmpty(dto.Id))
            {
                throw new InvalidOperationException(
                    "PathCardCollection missing required 'id' field. " +
                    "Check JSON - field name must be 'id' (lowercase), not 'collectionId'. " +
                    $"Collection data: PathCards={dto.PathCards?.Count ?? 0}, Events={dto.Events?.Count ?? 0}");
            }

            // Embed actual path cards if this collection has path card IDs (for JSON loaded collections)
            if (dto.PathCards != null && dto.PathCards.Count == 0 && dto.PathCardIds != null)
            {
                foreach (string cardId in dto.PathCardIds)
                {
                    PathCardDTO? pathCard = pathCardLookup.FirstOrDefault(p => p.Name == cardId);
                    if (pathCard != null)
                    {
                        dto.PathCards.Add(pathCard);
                    }
                }
            }

            // Determine if this is a path collection or event collection based on contents
            bool isEventCollection = (dto.Events != null && dto.Events.Count > 0);
            bool isPathCollection = (dto.PathCards != null && dto.PathCards.Count > 0);

            // ADR-007: CollectionId property deleted - use Collection.Id instead
            if (isEventCollection)
            {
                // This is an event collection - contains child events for random selection
                _gameWorld.AllEventCollections.Add(new PathCollectionEntry { Collection = dto });
            }
            else if (isPathCollection)
            {
                // This is a path collection - contains actual path cards for FixedPath segments
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { Collection = dto });
            }
            else
            {
                // Fallback: treat as path collection if no clear indicators
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { Collection = dto });
            }
        }
    }

    /// <summary>
    /// Initialize exchange decks for Mercantile NPCs only
    /// </summary>
    private void InitializeNPCExchangeDecks(DeckCompositionDTO deckCompositions)
    {
        foreach (NPC npc in _gameWorld.NPCs)
        {
            List<ExchangeCard> npcExchangeCards = new List<ExchangeCard>();

            // Check deck compositions for this NPC's exchange deck
            NPCDeckDefinitionDTO deckDef = null;
            if (deckCompositions != null)
            {
                // Check for NPC-specific deck first (use Name as key since NPC.ID deleted)
                if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.Name))
                {
                    deckDef = deckCompositions.NpcDecks[npc.Name];
                }
                // No default deck anymore - NPCs only have specific decks
            }

            // Build exchange deck from composition
            if (deckDef != null && deckDef.ExchangeDeck != null)
            {
                foreach (KeyValuePair<string, int> kvp in deckDef.ExchangeDeck)
                {
                    string cardId = kvp.Key;
                    int count = kvp.Value;

                    // Find the exchange card from the parsed exchange cards
                    // HIGHLANDER: Object references ONLY, lookup by Name not wrapper ID
                    ExchangeCard? exchangeCard = _parsedExchangeCards?.FirstOrDefault(e => e.Name == cardId);
                    if (exchangeCard != null)
                    {
                        // Add the specified number of copies to the deck
                        for (int i = 0; i < count; i++)
                        {
                            // Exchange cards are templates, no need to clone
                            npcExchangeCards.Add(exchangeCard);
                        }
                    }
                    else
                    { }
                }
            }
            else if (npc.PersonalityType == PersonalityType.MERCANTILE)
            {
                // Create default exchanges for mercantile NPCs without specific exchanges
                npcExchangeCards = ExchangeParser.CreateDefaultExchangesForNPC(npc);
                if (npcExchangeCards.Count > 0)
                { }
            }

            // Initialize exchange deck
            npc.InitializeExchangeDeck(npcExchangeCards);

            if (npcExchangeCards.Count > 0)
            { }
        }
    }

    private void LoadItems(List<ItemDTO> itemDtos, bool allowSkeletons)
    {
        if (itemDtos == null) return;

        foreach (ItemDTO dto in itemDtos)
        {
            Item item = ItemParser.ConvertDTOToItem(dto);
            _gameWorld.Items.Add(item);
        }
    }

    private void LoadStandingObligations(List<StandingObligationDTO> obligationDtos, bool allowSkeletons)
    {
        if (obligationDtos == null) return;

        foreach (StandingObligationDTO dto in obligationDtos)
        {
            StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(dto, _gameWorld);
            _gameWorld.StandingObligationTemplates.Add(obligation);
        }
    }

    // NOTE: LoadLocationActions and LoadPlayerActions methods REMOVED
    // Actions are NO LONGER loaded from JSON - they are GENERATED from catalogues at parse time
    // See GeneratePlayerActionsFromCatalogue() and GenerateLocationActionsFromCatalogue() above
    // CATALOGUE PATTERN: Actions generated from categorical properties (LocationPropertyType enums), never from JSON

    // Conversion methods that don't have dedicated parsers yet

    private RouteOption ConvertRouteDTOToModel(RouteDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException("Route missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.OriginSpotId))
            throw new InvalidDataException($"Route '{dto.Name}' missing required field 'OriginSpotId'");
        if (string.IsNullOrEmpty(dto.DestinationSpotId))
            throw new InvalidDataException($"Route '{dto.Name}' missing required field 'DestinationSpotId'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"Route '{dto.Name}' missing required field 'Description'");

        // HIGHLANDER: LINQ queries on already-parsed locations (originSpot and destSpot already resolved above)
        RouteOption route = new RouteOption
        {
            // RouteOption uses Name as natural key (no Id property)
            Name = dto.Name,
            OriginLocation = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.OriginSpotId),
            DestinationLocation = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.DestinationSpotId),
            Method = Enum.TryParse<TravelMethods>(dto.Method, out TravelMethods method) ? method : TravelMethods.Walking,
            BaseCoinCost = dto.BaseCoinCost,
            BaseStaminaCost = dto.BaseStaminaCost,
            TravelTimeSegments = dto.TravelTimeSegments,
            Description = dto.Description,
            MaxItemCapacity = dto.MaxItemCapacity > 0 ? dto.MaxItemCapacity : 3
        };

        // Parse terrain categories
        if (dto.TerrainCategories != null)
        {
            foreach (string category in dto.TerrainCategories)
            {
                if (Enum.TryParse<TerrainCategory>(category, out TerrainCategory terrain))
                {
                    route.TerrainCategories.Add(terrain);
                }
            }
        }

        // Parse travel path cards system properties
        route.StartingStamina = dto.StartingStamina;

        // Parse route segments
        if (dto.Segments != null)
        {
            foreach (RouteSegmentDTO segmentDto in dto.Segments)
            {
                // Parse segment type
                SegmentType segmentType = SegmentType.FixedPath; // Default
                if (!string.IsNullOrEmpty(segmentDto.Type))
                {
                    Enum.TryParse<SegmentType>(segmentDto.Type, out segmentType);
                }

                RouteSegment segment = new RouteSegment
                {
                    SegmentNumber = segmentDto.SegmentNumber,
                    Type = segmentType,
                    NarrativeDescription = segmentDto.NarrativeDescription
                };

                // HIGHLANDER: Resolve IDs to object references at parse-time
                // NO string IDs stored in domain entities - only object references
                if (segmentType == SegmentType.FixedPath)
                {
                    // Resolve PathCollectionId → PathCardCollectionDTO object
                    if (!string.IsNullOrEmpty(segmentDto.PathCollectionId))
                    {
                        segment.PathCollection = _gameWorld.GetPathCollection(segmentDto.PathCollectionId);
                    }
                }
                else if (segmentType == SegmentType.Event)
                {
                    // Resolve EventCollectionId → PathCardCollectionDTO object
                    if (!string.IsNullOrEmpty(segmentDto.EventCollectionId))
                    {
                        segment.EventCollection = _gameWorld.GetPathCollection(segmentDto.EventCollectionId);
                    }
                }
                else if (segmentType == SegmentType.Encounter)
                {
                    // Resolve MandatorySceneId → SceneTemplate object
                    if (!string.IsNullOrEmpty(segmentDto.MandatorySceneId))
                    {
                        segment.MandatorySceneTemplate = _gameWorld.SceneTemplates
                            .FirstOrDefault(t => t.Id == segmentDto.MandatorySceneId);
                    }
                }

                route.Segments.Add(segment);
            }
        }

        // Parse encounter deck IDs
        if (dto.EncounterDeckIds != null)
        {
            route.EncounterDeckIds.AddRange(dto.EncounterDeckIds);
        }

        // NOTE: Old inline scene parsing removed - NEW Scene-Situation architecture
        // Scenes now spawn via Situation spawn rewards (SceneSpawnReward) instead of inline definitions
        // Routes will receive Scene references through the spawning system, not direct parsing

        return route;
    }

    // ObservationCard system eliminated - ConvertObservationDTOToCard removed
    // LocationAction/PlayerAction conversion - replaced by dedicated parsers (LocationActionParser, PlayerActionParser)

    private void LoadExchanges(List<ExchangeDTO> exchangeDtos, bool allowSkeletons)
    {
        if (exchangeDtos == null) return;

        // Store DTOs for reference
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            _gameWorld.ExchangeDefinitions.Add(dto);
        }

        // EntityResolver for categorical entity resolution (DDR-006)
        Player player = _gameWorld.GetPlayer();
        SceneNarrativeService narrativeService = new SceneNarrativeService(_gameWorld);
        EntityResolver entityResolver = new EntityResolver(_gameWorld, player, narrativeService);

        // Parse exchanges into ExchangeCard objects and store them
        // These will be referenced when building NPC decks
        // HIGHLANDER: Object references ONLY, no wrapper class
        _parsedExchangeCards = new List<ExchangeCard>();
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            ExchangeCard exchangeCard = ExchangeParser.ParseExchange(dto, entityResolver);
            _parsedExchangeCards.Add(exchangeCard);
        }
    }

    /// <summary>
    /// Initialize the travel discovery system state after all content is loaded
    /// </summary>
    private void InitializeTravelDiscoverySystem()
    {// Initialize PathCardDiscoveries from cards embedded in collections
     // First from path collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllPathCollections.Select(c => c.Collection))
        {
            foreach (PathCardDTO pathCard in collection.PathCards)
            {
                _gameWorld.SetPathCardDiscovered(pathCard, pathCard.StartsRevealed);
            }
        }

        // Also initialize discovery states for event cards in event collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllEventCollections.Select(c => c.Collection))
        {
            foreach (PathCardDTO eventCard in collection.EventCards)
            {
                _gameWorld.SetPathCardDiscovered(eventCard, eventCard.StartsRevealed);
            }
        }

        // Cards are now embedded directly in collections
        // No separate card dictionaries needed

        // Initialize EventDeckPositions for routes with event pools
        foreach (PathCollectionEntry entry in _gameWorld.AllEventCollections)
        {
            // ADR-007: Use Collection.Id (object property) instead of deleted CollectionId
            string routeId = entry.Collection.Id;
            string deckKey = $"route_{routeId}_events";

            // Start at position 0 for deterministic event drawing
            _gameWorld.SetEventDeckPosition(deckKey, 0);

            int eventCount = entry.Collection.Events?.Count ?? 0;
        }
    }

    /// <summary>
    /// Initialize obligation journal with all obligations as Potential (awaiting discovery triggers)
    /// Called AFTER all packages are loaded to ensure all obligations exist
    /// HIGHLANDER: Object references ONLY, no ID collections
    /// </summary>
    private void InitializeObligationJournal()
    {
        ObligationJournal obligationJournal = _gameWorld.ObligationJournal;

        obligationJournal.PotentialObligations.Clear();
        foreach (Obligation obligation in _gameWorld.Obligations)
        {
            obligationJournal.PotentialObligations.Add(obligation);
        }
    }

    /// <summary>
    /// Validates that crossroads configuration is correct:
    /// 1. Each Venue has exactly one location with Crossroads property
    /// 2. All route origin and destination Locations have Crossroads property
    /// </summary>
    /// <summary>
    /// BIDIRECTIONAL ROUTE GENERATION: Automatically creates the reverse route from a forward route.
    /// This ensures travel is always bidirectional and segments are properly reversed.
    /// For example, a route A->B->C with segments [1,2,3] becomes C->B->A with segments [3,2,1].
    /// </summary>
    private RouteOption GenerateReverseRoute(RouteOption forwardRoute)
    {
        // Extract Venue IDs from the location names for naming
        // HIGHLANDER: Pass Location objects directly, not strings
        string originVenueId = GetVenueIdFromSpot(forwardRoute.OriginLocation);
        string destVenueId = GetVenueIdFromSpot(forwardRoute.DestinationLocation);

        RouteOption reverseRoute = new RouteOption
        {
            Name = $"Return to {GetLocationNameFromId(originVenueId)}",
            // Swap origin and destination locations
            OriginLocation = forwardRoute.DestinationLocation,
            DestinationLocation = forwardRoute.OriginLocation,

            // Keep the same properties for both directions
            Method = forwardRoute.Method,
            BaseCoinCost = forwardRoute.BaseCoinCost,
            BaseStaminaCost = forwardRoute.BaseStaminaCost,
            TravelTimeSegments = forwardRoute.TravelTimeSegments,
            DepartureTime = forwardRoute.DepartureTime,
            MaxItemCapacity = forwardRoute.MaxItemCapacity,
            Description = $"Return journey from {GetLocationNameFromId(destVenueId)} to {GetLocationNameFromId(originVenueId)}",
            // AccessRequirement system eliminated - PRINCIPLE 4: Economic affordability determines access
            RouteType = forwardRoute.RouteType,
            HasPermitUnlock = forwardRoute.HasPermitUnlock,
            StartingStamina = forwardRoute.StartingStamina
        };

        // Copy terrain categories
        reverseRoute.TerrainCategories.AddRange(forwardRoute.TerrainCategories);

        // Copy weather modifications
        foreach (KeyValuePair<WeatherCondition, RouteModification> kvp in forwardRoute.WeatherModifications)
        {
            reverseRoute.WeatherModifications[kvp.Key] = kvp.Value;
        }

        // CRITICAL: Reverse the segments order for the return journey
        // This ensures the path is traversed in reverse (C->B->A instead of A->B->C)
        List<RouteSegment> reversedSegments = forwardRoute.Segments.OrderByDescending(s => s.SegmentNumber).ToList();
        int segmentNumber = 1;
        foreach (RouteSegment? originalSegment in reversedSegments)
        {
            RouteSegment reverseSegment = new RouteSegment
            {
                SegmentNumber = segmentNumber++,
                Type = originalSegment.Type,
                // Keep the same collections - they represent the same physical locations
                PathCollection = originalSegment.PathCollection,
                EventCollection = originalSegment.EventCollection,
                MandatorySceneTemplate = originalSegment.MandatorySceneTemplate
            };
            reverseRoute.Segments.Add(reverseSegment);
        }

        // Copy encounter deck IDs
        reverseRoute.EncounterDeckIds.AddRange(forwardRoute.EncounterDeckIds);

        // If the forward route has a route-level event pool, copy it to the reverse route
        // HIGHLANDER: Use RouteOption.Name (natural key) instead of deleted Id property
        PathCollectionEntry? forwardEntry = _gameWorld.AllEventCollections.FirstOrDefault(x => x.Collection.Id == forwardRoute.Name);
        if (forwardEntry != null)
        {
            PathCollectionEntry reverseEntry = new PathCollectionEntry
            {
                Collection = forwardEntry.Collection
            };
            _gameWorld.AllEventCollections.Add(reverseEntry);
        }

        return reverseRoute;
    }

    private string GetLocationNameFromId(string venueId)
    {
        // Helper to get friendly Venue name from ID for route naming
        if (string.IsNullOrEmpty(venueId))
            throw new InvalidDataException("GetLocationNameFromId called with null/empty venueId");

        Venue venue = _gameWorld.Venues.FirstOrDefault(v => v.Name == venueId);
        if (venue == null)
            return venueId.Replace("_", " ").Replace("-", " "); // Fallback to formatted ID if venue not found
        return venue.Name;
    }

    private class VenueLocationGrouping
    {
        public string VenueId { get; set; }
        public List<Location> Locations { get; set; } = new List<Location>();
    }

    private void ValidateCrossroadsConfiguration()
    {
        // Group Locations by Venue
        List<VenueLocationGrouping> spotsByLocation = new List<VenueLocationGrouping>();
        foreach (Location location in _gameWorld.Locations)
        {
            // ADR-007: Use Venue.Name instead of deleted VenueId
            int groupIndex = spotsByLocation.FindIndex(g => g.VenueId == location.Venue.Name);
            if (groupIndex == -1)
            {
                VenueLocationGrouping group = new VenueLocationGrouping();
                group.VenueId = location.Venue.Name;
                group.Locations.Add(location);
                spotsByLocation.Add(group);
            }
            else
            {
                spotsByLocation[groupIndex].Locations.Add(location);
            }
        }

        // Validate each venue has exactly one crossroads location
        foreach (Venue venue in _gameWorld.Venues)
        {
            VenueLocationGrouping locationGroup = spotsByLocation.FirstOrDefault(g => g.VenueId == venue.Name);
            if (locationGroup == null || locationGroup.VenueId == null)
            {
                throw new InvalidOperationException($"Venue '{venue.Name}' has no Locations defined");
            }

            List<Location> locations = locationGroup.Locations;
            List<Location> crossroadsSpots = locations
                .Where(s => s.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true)
                .ToList();

            if (crossroadsSpots.Count == 0)
            {
                throw new InvalidOperationException($"Venue '{venue.Name}' has no Locations with Crossroads property. Every Venue must have exactly one crossroads location for travel.");
            }
            else if (crossroadsSpots.Count > 1)
            {
                string spotsInfo = string.Join(", ", crossroadsSpots.Select(s => $"'{s.Name}'"));
                throw new InvalidOperationException($"Venue '{venue.Name}' has {crossroadsSpots.Count} Locations with Crossroads property: {spotsInfo}. Only one crossroads location is allowed per location.");
            }
        }

        // Validate all route Locations have crossroads property
        List<string> routeSpotIds = new List<string>();
        foreach (RouteOption route in _gameWorld.Routes)
        {
            if (!routeSpotIds.Contains(route.OriginLocation.Name))
                routeSpotIds.Add(route.OriginLocation.Name);
            if (!routeSpotIds.Contains(route.DestinationLocation.Name))
                routeSpotIds.Add(route.DestinationLocation.Name);
        }

        foreach (string LocationId in routeSpotIds)
        {
            // HIGHLANDER: LINQ query on already-parsed locations
            Location location = _gameWorld.Locations.FirstOrDefault(l => l.Name == LocationId);
            if (location == null)
            {// Create skeleton location with crossroads property (required for routes)
                location = SkeletonGenerator.GenerateSkeletonSpot(
                    LocationId,
                    "unknown_location",
                    $"crossroads_validation_{LocationId}"
                );

                // Ensure skeleton has crossroads property for route connectivity
                if (!location.LocationProperties.Contains(LocationPropertyType.Crossroads))
                {
                    location.LocationProperties.Add(LocationPropertyType.Crossroads);
                }

                _gameWorld.AddOrUpdateLocation(LocationId, location);
                _gameWorld.AddSkeleton(LocationId, "Location");
            }

            if (!location.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true)
            {
                location.LocationProperties.Add(LocationPropertyType.Crossroads);
            }
        }
    }

    private void LoadTravelScenes(List<TravelSceneDTO> sceneDtos, bool allowSkeletons)
    {
        if (sceneDtos == null) return;

        // STUBBED OUT: TravelScene parsing removed (legacy Scene architecture deleted)
        // TravelScene functionality moved to Phase 5 (Scene initialization pipeline)
        // TravelSceneParser parser = new TravelSceneParser();
        // foreach (TravelSceneDTO dto in sceneDtos)
        // {
        //     TravelScene scene = parser.ParseTravelScene(dto);
        //     _gameWorld.TravelScenes.Add(scene);
        // }
    }

    private void LoadConversationTrees(List<ConversationTreeDTO> conversationTrees, bool allowSkeletons)
    {
        if (conversationTrees == null) return;

        // EntityResolver for categorical entity resolution (DDR-006)
        Player player = _gameWorld.GetPlayer();
        SceneNarrativeService narrativeService = new SceneNarrativeService(_gameWorld);
        EntityResolver entityResolver = new EntityResolver(_gameWorld, player, narrativeService);

        foreach (ConversationTreeDTO dto in conversationTrees)
        {
            ConversationTree tree = ConversationTreeParser.Parse(dto, entityResolver);
            _gameWorld.ConversationTrees.Add(tree);
        }
    }

    private void LoadObservationScenes(List<ObservationSceneDTO> observationScenes, bool allowSkeletons)
    {
        if (observationScenes == null) return;

        // EntityResolver for categorical entity resolution (DDR-006)
        Player player = _gameWorld.GetPlayer();
        SceneNarrativeService narrativeService = new SceneNarrativeService(_gameWorld);
        EntityResolver entityResolver = new EntityResolver(_gameWorld, player, narrativeService);

        foreach (ObservationSceneDTO dto in observationScenes)
        {
            ObservationScene scene = ObservationSceneParser.Parse(dto, entityResolver);
            _gameWorld.ObservationScenes.Add(scene);
        }
    }

    private void LoadEmergencySituations(List<EmergencySituationDTO> emergencySituations, bool allowSkeletons)
    {
        if (emergencySituations == null) return;

        foreach (EmergencySituationDTO dto in emergencySituations)
        {
            EmergencySituation emergency = EmergencyParser.Parse(dto, _gameWorld);
            _gameWorld.EmergencySituations.Add(emergency);
        }
    }

    /// <summary>
    /// Load State definitions - metadata about temporary player conditions
    /// Scene-Situation Architecture (Sir Brante integration)
    /// </summary>
    private void LoadStates(List<StateDTO> stateDtos, bool allowSkeletons)
    {
        if (stateDtos == null) return;

        List<State> states = StateParser.ParseStates(stateDtos);
        foreach (State state in states)
        {
            _gameWorld.States.Add(state);
        }
    }

    /// <summary>
    /// Load Achievement definitions - milestone templates with grant conditions
    /// Scene-Situation Architecture (Sir Brante integration)
    /// </summary>
    private void LoadAchievements(List<AchievementDTO> achievementDtos, bool allowSkeletons)
    {
        if (achievementDtos == null) return;

        List<Achievement> achievements = AchievementParser.ParseAchievements(achievementDtos);
        foreach (Achievement achievement in achievements)
        {
            _gameWorld.Achievements.Add(achievement);
        }
    }

    /// <summary>
    /// Load SceneTemplate definitions - immutable archetypes for procedural narrative spawning
    /// Scene-Situation Architecture (Sir Brante integration)
    /// </summary>
    private void LoadSceneTemplates(List<SceneTemplateDTO> sceneTemplateDtos, bool allowSkeletons)
    {
        if (sceneTemplateDtos == null) return;

        SceneTemplateParser parser = new SceneTemplateParser(_gameWorld, _sceneGenerationFacade);
        foreach (SceneTemplateDTO dto in sceneTemplateDtos)
        {
            SceneTemplate template = parser.ParseSceneTemplate(dto);
            _gameWorld.SceneTemplates.Add(template);
        }

        // Only log when templates were actually loaded from this package (reduce log noise)
        if (sceneTemplateDtos.Count > 0)
        {
            Console.WriteLine($"[PackageLoader] Loaded {sceneTemplateDtos.Count} SceneTemplates from this package");
        }
    }

    /// <summary>
    /// Load Scene runtime instances from dynamic packages
    /// HIGHLANDER: JSON → PackageLoader → Parser → Entity (single instantiation path)
    /// Scenes are NEVER in static packages - only in dynamic packages generated at spawn time
    /// </summary>
    private void LoadScenes(List<SceneDTO> sceneDtos, bool allowSkeletons)
    {
        if (sceneDtos == null) return;

        // System 4: Entity Resolver (FindOrCreate)
        Player player = _gameWorld.GetPlayer();
        SceneNarrativeService narrativeService = new SceneNarrativeService(_gameWorld);
        EntityResolver entityResolver = new EntityResolver(_gameWorld, player, narrativeService);

        foreach (SceneDTO dto in sceneDtos)
        {
            // ARCHITECTURAL CHANGE: Entity resolution happens per-situation (not per-scene)
            // SceneParser will iterate through SituationDTOs and resolve entities for each
            // Scene has no placement properties - situations do
            Scene scene = SceneParser.ConvertDTOToScene(dto, _gameWorld, entityResolver);
            _gameWorld.Scenes.Add(scene);
        }

        // Only log when scenes were actually loaded from this package
        if (sceneDtos.Count > 0)
        {
            Console.WriteLine($"[PackageLoader] Loaded {sceneDtos.Count} Scene instances from this package");
        }
    }

}