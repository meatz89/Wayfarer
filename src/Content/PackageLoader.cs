using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Helper class for exchange card lookups
/// </summary>
internal class ExchangeCardEntry
{
    public string Id { get; set; }
    public ExchangeCard Card { get; set; }
}

/// <summary>
/// Helper class for path card lookups
/// </summary>
internal class PathCardEntry
{
    public string Id { get; set; }
    public PathCardDTO Card { get; set; }
}

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
    private List<ExchangeCardEntry> _parsedExchangeCards;
    private string _currentPackageId = "unknown"; // Track current package for error reporting

    // Track loaded packages to prevent reloading
    private List<string> _loadedPackageIds = new List<string>();

    public PackageLoader(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
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
            string json = File.ReadAllText(packagePath);
            Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
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
        LoadPlayerStatsConfiguration(package.Content.PlayerStatsConfig, allowSkeletons);
        LoadListenDrawCounts(package.Content.ListenDrawCounts);
        LoadStates(package.Content.States, allowSkeletons); // Scene-Situation: State definitions
        LoadAchievements(package.Content.Achievements, allowSkeletons); // Scene-Situation: Achievement definitions
        LoadRegions(package.Content.Regions, allowSkeletons);
        LoadDistricts(package.Content.Districts, allowSkeletons);
        LoadItems(package.Content.Items, allowSkeletons);

        // 2. Venues and Locations (may reference regions/districts)
        LoadLocations(package.Content.Venues, allowSkeletons);
        LoadLocationSpots(package.Content.Locations, allowSkeletons);

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
        LoadObstacles(package.Content.Obstacles, allowSkeletons);

        // 3.6 Screen Expansion Systems (conversation trees, observation scenes, emergencies)
        LoadConversationTrees(package.Content.ConversationTrees, allowSkeletons);
        LoadObservationScenes(package.Content.ObservationScenes, allowSkeletons);
        LoadEmergencySituations(package.Content.EmergencySituations, allowSkeletons);

        // 4. NPCs (reference locations, Locations, and cards)
        LoadNPCs(package.Content.Npcs, allowSkeletons);
        LoadStrangers(package.Content.Strangers, allowSkeletons);

        // 5. Routes (reference Locations which now have VenueId set)
        LoadRoutes(package.Content.Routes, allowSkeletons);

        // 6. Relationship entities (depend on NPCs and cards)
        LoadExchanges(package.Content.Exchanges, allowSkeletons);
        InitializeNPCExchangeDecks(package.Content.DeckCompositions);

        // 7. Complex entities
        LoadDialogueTemplates(package.Content.DialogueTemplates, allowSkeletons);
        LoadStandingObligations(package.Content.StandingObligations, allowSkeletons);
        LoadLocationActions(package.Content.LocationActions, allowSkeletons);
        LoadPlayerActions(package.Content.PlayerActions, allowSkeletons);

        // 8. Travel content
        List<PathCardEntry> pathCardLookup = LoadPathCards(package.Content.PathCards, allowSkeletons);
        List<PathCardEntry> eventCardLookup = LoadEventCards(package.Content.EventCards, allowSkeletons);
        LoadTravelEvents(package.Content.TravelEvents, eventCardLookup, allowSkeletons);
        LoadEventCollections(package.Content.PathCardCollections, pathCardLookup, eventCardLookup, allowSkeletons);

        // 9. V2 Obligation and Travel Systems
        LoadTravelObstacles(package.Content.TravelObstacles, allowSkeletons);
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
            AllowTrailingCommas = true
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
    public List<string> LoadDynamicPackageFromJson(string json, string packageId)
    {
        Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        });

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

        // Set starting location
        if (!string.IsNullOrEmpty(conditions.StartingSpotId))
        {
            _gameWorld.InitialLocationSpotId = conditions.StartingSpotId;
        }

        // Apply starting obligations
        if (conditions.StartingObligations != null)
        {
            foreach (StandingObligationDTO obligationDto in conditions.StartingObligations)
            {
                // Convert DTO to domain model and add to player's standing obligations
                StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(obligationDto);
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
                NPCTokenEntry tokenEntry = _gameWorld.GetPlayer().NPCTokens.GetNPCTokenEntry(kvp.Key);
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

    private void LoadRegions(List<RegionDTO> regionDtos, bool allowSkeletons)
    {
        if (regionDtos == null) return;

        foreach (RegionDTO dto in regionDtos)
        {
            Region region = new Region
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                DistrictIds = dto.DistrictIds,
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
                Id = dto.Id,
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
        if (situationDtos == null) return;

        foreach (SituationDTO dto in situationDtos)
        {
            // Parse situation using SituationParser
            Situation situation = SituationParser.ConvertDTOToSituation(dto, _gameWorld);

            // Add to centralized GameWorld.Situations storage
            _gameWorld.Situations.Add(situation);// Assign situation to NPC or Location based on PlacementNpcId/PlacementLocationId
            if (!string.IsNullOrEmpty(situation.PlacementNpc?.ID))
            {
                // Social situation - assign to NPC.ActiveSituationIds (reference only, situation lives in GameWorld.Situations)
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == situation.PlacementNpc?.ID);
                if (npc != null)
                {
                    npc.ActiveSituationIds.Add(situation.Id);
                }
                else
                { }
            }
            else if (!string.IsNullOrEmpty(situation.PlacementLocation?.Id))
            {
                // Mental/Physical situation - assign to Location.ActiveSituationIds (reference only, situation lives in GameWorld.Situations)
                Location location = _gameWorld.GetLocation(situation.PlacementLocation?.Id);
                if (location != null)
                {
                    location.ActiveSituationIds.Add(situation.Id);
                }
                else
                { }
            }
        }
    }

    private void LoadObstacles(List<ObstacleDTO> obstacleDtos, bool allowSkeletons)
    {
        if (obstacleDtos == null) return;

        foreach (ObstacleDTO dto in obstacleDtos)
        {
            // Parse obstacle using ObstacleParser
            Obstacle obstacle = ObstacleParser.ConvertDTOToObstacle(dto, _currentPackageId, _gameWorld);

            // Duplicate ID protection - prevent data corruption
            if (!_gameWorld.Obstacles.Any(o => o.Id == obstacle.Id))
            {
                _gameWorld.Obstacles.Add(obstacle);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Duplicate obstacle ID '{obstacle.Id}' found in package '{_currentPackageId}'. " +
                    $"Obstacle IDs must be globally unique across all packages.");
            }
        }
    }

    private void LoadLocations(List<VenueDTO> venueDtos, bool allowSkeletons)
    {
        if (venueDtos == null) return;

        foreach (VenueDTO dto in venueDtos)
        {
            // Check if this venue was previously a skeleton, if so replace it
            Venue? existingSkeleton = _gameWorld.Venues
                .FirstOrDefault(l => l.Id == dto.Id && l.IsSkeleton);

            Venue venue = VenueParser.ConvertDTOToVenue(dto);

            if (existingSkeleton != null)
            {
                _gameWorld.Venues.Remove(existingSkeleton);
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }
            }

            _gameWorld.Venues.Add(venue);
        }
    }

    private void LoadLocationSpots(List<LocationDTO> spotDtos, bool allowSkeletons)
    {
        if (spotDtos == null) return;

        foreach (LocationDTO dto in spotDtos)
        {
            // Check if this location was previously a skeleton, if so replace it
            Location? existingSkeleton = _gameWorld.GetLocation(dto.Id);

            if (existingSkeleton != null && existingSkeleton.IsSkeleton)
            {
                SkeletonRegistryEntry? spotSkeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (spotSkeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(spotSkeletonEntry);
                }

                // Remove from primary Locations dictionary if exists
                _gameWorld.Locations.RemoveSpot(dto.Id);
            }

            Location location = LocationParser.ConvertDTOToLocation(dto, _gameWorld);

            // Add to primary Locations dictionary
            _gameWorld.Locations.AddOrUpdateSpot(location.Id, location);
        }
    }

    private void LoadNPCs(List<NPCDTO> npcDtos, bool allowSkeletons)
    {
        if (npcDtos == null) return;

        foreach (NPCDTO dto in npcDtos)
        {
            // Check if NPC references a Venue that doesn't exist
            if (!string.IsNullOrEmpty(dto.VenueId) &&
                !_gameWorld.Venues.Any(l => l.Id == dto.VenueId))
            {
                // Create skeleton venue
                Venue skeletonVenue = SkeletonGenerator.GenerateSkeletonVenue(
                    dto.VenueId,
                    $"npc_{dto.Id}_reference");

                _gameWorld.Venues.Add(skeletonVenue);
                _gameWorld.SkeletonRegistry.AddSkeleton(dto.VenueId, "Venue");

                // Also create a skeleton location for the location
                string hubSpotId = $"{dto.VenueId}_hub";
                Location hubSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    hubSpotId,
                    dto.VenueId,
                    $"location_{dto.VenueId}_hub");

                _gameWorld.Locations.AddOrUpdateSpot(hubSpotId, hubSpot);
                _gameWorld.SkeletonRegistry.AddSkeleton(hubSpot.Id, "Location");
            }

            // Check if NPC references a location that doesn't exist
            if (!string.IsNullOrEmpty(dto.LocationId) &&
                _gameWorld.GetLocation(dto.LocationId) == null)
            {
                // Create skeleton location
                if (string.IsNullOrEmpty(dto.VenueId))
                    throw new InvalidDataException($"NPC '{dto.Id}' references LocationId '{dto.LocationId}' but VenueId is missing - cannot create skeleton");

                Location skeletonSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    dto.LocationId,
                    dto.VenueId,
                    $"npc_{dto.Id}_spot_reference");

                _gameWorld.Locations.AddOrUpdateSpot(dto.LocationId, skeletonSpot);
                _gameWorld.SkeletonRegistry.AddSkeleton(dto.LocationId, "Location");
            }

            // Check if this NPC was previously a skeleton, if so replace it and preserve persistent decks
            NPC? existingSkeleton = _gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
            if (existingSkeleton != null)
            {// Preserve all cards from the persistent decks
                List<ExchangeCard> preservedExchangeCards = existingSkeleton.ExchangeDeck.ToList();

                int totalPreservedCards = preservedExchangeCards.Count;// Remove skeleton from game world
                _gameWorld.NPCs.Remove(existingSkeleton);
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }

                // Create new NPC from DTO
                NPC npc = NPCParser.ConvertDTOToNPC(dto, _gameWorld);

                // Restore preserved cards to the new NPC's persistent decks
                if (preservedExchangeCards.Any())
                {
                    npc.ExchangeDeck.AddRange(preservedExchangeCards);
                }// Add the new NPC to game world
                _gameWorld.NPCs.Add(npc);
            }
            else
            {
                // No skeleton to replace, just create new NPC normally
                NPC npc = NPCParser.ConvertDTOToNPC(dto, _gameWorld);
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
            Location originSpot = _gameWorld.GetLocation(dto.OriginSpotId);
            if (originSpot == null)
            {
                if (allowSkeletons)
                {
                    if (string.IsNullOrEmpty(dto.OriginVenueId))
                        throw new InvalidDataException($"Route '{dto.Id}' missing OriginVenueId - cannot create skeleton origin location");

                    // Create skeleton location with crossroads property (required for routes)
                    originSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.OriginSpotId,
                        dto.OriginVenueId,
                        $"route_{dto.Id}_origin"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!originSpot.LocationProperties.Contains(LocationPropertyType.Crossroads))
                    {
                        originSpot.LocationProperties.Add(LocationPropertyType.Crossroads);
                    }

                    _gameWorld.Locations.AddOrUpdateSpot(dto.OriginSpotId, originSpot);
                    _gameWorld.SkeletonRegistry.AddSkeleton(dto.OriginSpotId, "Location");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Id}' references missing origin location '{dto.OriginSpotId}'. Ensure Locations are loaded before routes.");
                }
            }

            // Check destination location
            Location destSpot = _gameWorld.GetLocation(dto.DestinationSpotId);
            if (destSpot == null)
            {
                if (allowSkeletons)
                {
                    if (string.IsNullOrEmpty(dto.DestinationVenueId))
                        throw new InvalidDataException($"Route '{dto.Id}' missing DestinationVenueId - cannot create skeleton destination location");

                    // Create skeleton location with crossroads property (required for routes)
                    destSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.DestinationSpotId,
                        dto.DestinationVenueId,
                        $"route_{dto.Id}_destination"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!destSpot.LocationProperties.Contains(LocationPropertyType.Crossroads))
                    {
                        destSpot.LocationProperties.Add(LocationPropertyType.Crossroads);
                    }

                    _gameWorld.Locations.AddOrUpdateSpot(dto.DestinationSpotId, destSpot);
                    _gameWorld.SkeletonRegistry.AddSkeleton(dto.DestinationSpotId, "Location");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Id}' references missing destination location '{dto.DestinationSpotId}'. Ensure Locations are loaded before routes.");
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

    private string GetVenueIdFromSpotId(string LocationId)
    {
        Location location = _gameWorld.GetLocation(LocationId);
        if (location == null)
            throw new InvalidDataException($"Location '{LocationId}' not found when attempting to get VenueId");
        if (string.IsNullOrEmpty(location.VenueId))
            throw new InvalidDataException($"Location '{LocationId}' has no VenueId assigned");
        return location.VenueId;
    }

    private void LoadDialogueTemplates(DialogueTemplates dialogueTemplates, bool allowSkeletons)
    {
        if (dialogueTemplates == null) return; _gameWorld.DialogueTemplates = dialogueTemplates;
    }

    private List<PathCardEntry> LoadPathCards(List<PathCardDTO> pathCardDtos, bool allowSkeletons)
    {
        if (pathCardDtos == null) return new List<PathCardEntry>();

        List<PathCardEntry> pathCardLookup = new List<PathCardEntry>();
        foreach (PathCardDTO dto in pathCardDtos)
        {
            pathCardLookup.Add(new PathCardEntry { Id = dto.Id, Card = dto });
        }
        return pathCardLookup;
    }

    private List<PathCardEntry> LoadEventCards(List<PathCardDTO> eventCardDtos, bool allowSkeletons)
    {
        if (eventCardDtos == null) return new List<PathCardEntry>();

        List<PathCardEntry> eventCardLookup = new List<PathCardEntry>();
        foreach (PathCardDTO dto in eventCardDtos)
        {
            eventCardLookup.Add(new PathCardEntry { Id = dto.Id, Card = dto });
        }
        return eventCardLookup;
    }

    private void LoadTravelEvents(List<TravelEventDTO> travelEventDtos, List<PathCardEntry> eventCardLookup, bool allowSkeletons)
    {
        if (travelEventDtos == null) return;

        foreach (TravelEventDTO dto in travelEventDtos)
        {
            // Embed actual event cards if this event has event card IDs (for JSON loaded events)
            if (dto.EventCardIds != null && dto.EventCards.Count == 0)
            {
                foreach (string cardId in dto.EventCardIds)
                {
                    PathCardEntry? eventCardEntry = eventCardLookup.FirstOrDefault(e => e.Id == cardId);
                    if (eventCardEntry != null)
                    {
                        dto.EventCards.Add(eventCardEntry.Card);
                    }
                }
            }

            _gameWorld.AllTravelEvents.Add(new TravelEventEntry { EventId = dto.Id, TravelEvent = dto });
        }
    }

    private void LoadEventCollections(List<PathCardCollectionDTO> collectionDtos, List<PathCardEntry> pathCardLookup, List<PathCardEntry> eventCardLookup, bool allowSkeletons)
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
                    PathCardEntry? pathCardEntry = pathCardLookup.FirstOrDefault(p => p.Id == cardId);
                    if (pathCardEntry != null)
                    {
                        dto.PathCards.Add(pathCardEntry.Card);
                    }
                }
            }

            // Determine if this is a path collection or event collection based on contents
            bool isEventCollection = (dto.Events != null && dto.Events.Count > 0);
            bool isPathCollection = (dto.PathCards != null && dto.PathCards.Count > 0);

            if (isEventCollection)
            {
                // This is an event collection - contains child events for random selection
                _gameWorld.AllEventCollections.Add(new PathCollectionEntry { CollectionId = dto.Id, Collection = dto });
            }
            else if (isPathCollection)
            {
                // This is a path collection - contains actual path cards for FixedPath segments
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { CollectionId = dto.Id, Collection = dto });
            }
            else
            {
                // Fallback: treat as path collection if no clear indicators
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { CollectionId = dto.Id, Collection = dto });
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
                // Check for NPC-specific deck first
                if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.ID))
                {
                    deckDef = deckCompositions.NpcDecks[npc.ID];
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
                    ExchangeCardEntry? exchangeEntry = _parsedExchangeCards?.FirstOrDefault(e => e.Id == cardId);
                    if (exchangeEntry != null)
                    {
                        ExchangeCard exchangeCard = exchangeEntry.Card;
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
            StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(dto);
            _gameWorld.StandingObligationTemplates.Add(obligation);
        }
    }

    private void LoadLocationActions(List<LocationActionDTO> locationActionDtos, bool allowSkeletons)
    {
        if (locationActionDtos == null) return;

        foreach (LocationActionDTO dto in locationActionDtos)
        {
            LocationAction locationAction = LocationActionParser.ParseLocationAction(dto);
            _gameWorld.LocationActions.Add(locationAction);
        }
    }

    private void LoadPlayerActions(List<PlayerActionDTO> playerActionDtos, bool allowSkeletons)
    {
        if (playerActionDtos == null) return;

        foreach (PlayerActionDTO dto in playerActionDtos)
        {
            PlayerAction playerAction = PlayerActionParser.ParsePlayerAction(dto);
            _gameWorld.PlayerActions.Add(playerAction);
        }
    }

    // Conversion methods that don't have dedicated parsers yet

    private RouteOption ConvertRouteDTOToModel(RouteDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("Route missing required field 'Id'");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"Route '{dto.Id}' missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.OriginSpotId))
            throw new InvalidDataException($"Route '{dto.Id}' missing required field 'OriginSpotId'");
        if (string.IsNullOrEmpty(dto.DestinationSpotId))
            throw new InvalidDataException($"Route '{dto.Id}' missing required field 'DestinationSpotId'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"Route '{dto.Id}' missing required field 'Description'");

        RouteOption route = new RouteOption
        {
            Id = dto.Id,
            Name = dto.Name,
            OriginLocationSpot = dto.OriginSpotId,
            DestinationLocationSpot = dto.DestinationSpotId,
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

                // Set collection properties based on segment type using normalized properties
                if (segmentType == SegmentType.FixedPath)
                {
                    // FixedPath segments use pathCollectionId from JSON
                    segment.PathCollectionId = segmentDto.PathCollectionId;
                    if (!string.IsNullOrEmpty(segment.PathCollectionId))
                    { }
                }
                else if (segmentType == SegmentType.Event)
                {
                    // Event segments use eventCollectionId from JSON
                    segment.EventCollectionId = segmentDto.EventCollectionId;
                    if (!string.IsNullOrEmpty(segment.EventCollectionId))
                    { }
                }
                else if (segmentType == SegmentType.Encounter)
                {
                    // Encounter segments have mandatory obstacle that MUST be resolved
                    segment.MandatoryObstacleId = segmentDto.MandatoryObstacleId;
                    if (!string.IsNullOrEmpty(segment.MandatoryObstacleId))
                    { }
                }

                route.Segments.Add(segment);
            }
        }

        // Parse encounter deck IDs
        if (dto.EncounterDeckIds != null)
        {
            route.EncounterDeckIds.AddRange(dto.EncounterDeckIds);
        }

        // Parse obstacles on this route (bandits, flooding, difficult terrain)
        if (dto.Obstacles != null && dto.Obstacles.Count > 0)
        {
            foreach (ObstacleDTO obstacleDto in dto.Obstacles)
            {
                Obstacle obstacle = ObstacleParser.ConvertDTOToObstacle(obstacleDto, route.Id, _gameWorld);

                // Duplicate ID protection - prevent data corruption
                if (!_gameWorld.Obstacles.Any(o => o.Id == obstacle.Id))
                {
                    _gameWorld.Obstacles.Add(obstacle);
                    route.ObstacleIds.Add(obstacle.Id);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Duplicate obstacle ID '{obstacle.Id}' found in route '{route.Name}'. " +
                        $"Obstacle IDs must be globally unique across all packages.");
                }
            }
        }

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

        // Parse exchanges into ExchangeCard objects and store them
        // These will be referenced when building NPC decks
        _parsedExchangeCards = new List<ExchangeCardEntry>();
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            ExchangeCard exchangeCard = ExchangeParser.ParseExchange(dto, dto.NpcId);
            _parsedExchangeCards.Add(new ExchangeCardEntry { Id = exchangeCard.Id, Card = exchangeCard });
        }
    }

    /// <summary>
    /// Initialize the travel discovery system state after all content is loaded
    /// </summary>
    private void InitializeTravelDiscoverySystem()
    {// Initialize PathCardDiscoveries from cards embedded in collections
        // First from path collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllPathCollections.GetAllCollections())
        {
            foreach (PathCardDTO pathCard in collection.PathCards)
            {
                _gameWorld.PathCardDiscoveries.SetDiscovered(pathCard.Id, pathCard.StartsRevealed);
            }
        }

        // Also initialize discovery states for event cards in event collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllEventCollections.GetAllCollections())
        {
            foreach (PathCardDTO eventCard in collection.EventCards)
            {
                _gameWorld.PathCardDiscoveries.SetDiscovered(eventCard.Id, eventCard.StartsRevealed);
            }
        }

        // Cards are now embedded directly in collections
        // No separate card dictionaries needed

        // Initialize EventDeckPositions for routes with event pools
        foreach (PathCollectionEntry entry in _gameWorld.AllEventCollections)
        {
            string routeId = entry.CollectionId;
            string deckKey = $"route_{routeId}_events";

            // Start at position 0 for deterministic event drawing
            _gameWorld.EventDeckPositions.SetPosition(deckKey, 0);

            int eventCount = entry.Collection.Events?.Count ?? 0;
        }
    }

    /// <summary>
    /// Initialize obligation journal with all obligations as Potential (awaiting discovery triggers)
    /// Called AFTER all packages are loaded to ensure all obligations exist
    /// </summary>
    private void InitializeObligationJournal()
    {
        ObligationJournal obligationJournal = _gameWorld.ObligationJournal;

        obligationJournal.PotentialObligationIds.Clear();
        foreach (Obligation obligation in _gameWorld.Obligations)
        {
            obligationJournal.PotentialObligationIds.Add(obligation.Id);
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
        // Extract Venue IDs from the location IDs for naming
        string originVenueId = GetVenueIdFromSpotId(forwardRoute.OriginLocationSpot);
        string destVenueId = GetVenueIdFromSpotId(forwardRoute.DestinationLocationSpot);

        // Generate reverse route ID by swapping origin and destination
        string[] idParts = forwardRoute.Id.Split("_to_");
        string reverseId = idParts.Length == 2
            ? $"{idParts[1]}_to_{idParts[0]}"
            : $"{destVenueId}_to_{originVenueId}";

        RouteOption reverseRoute = new RouteOption
        {
            Id = reverseId,
            Name = $"Return to {GetLocationNameFromId(originVenueId)}",
            // Swap origin and destination
            OriginLocationSpot = forwardRoute.DestinationLocationSpot,
            DestinationLocationSpot = forwardRoute.OriginLocationSpot,

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
                PathCollectionId = originalSegment.PathCollectionId,
                EventCollectionId = originalSegment.EventCollectionId,
                MandatoryObstacleId = originalSegment.MandatoryObstacleId
            };
            reverseRoute.Segments.Add(reverseSegment);
        }

        // Copy encounter deck IDs
        reverseRoute.EncounterDeckIds.AddRange(forwardRoute.EncounterDeckIds);

        // If the forward route has a route-level event pool, copy it to the reverse route
        PathCollectionEntry? forwardEntry = _gameWorld.AllEventCollections.FindById(forwardRoute.Id);
        if (forwardEntry != null)
        {
            _gameWorld.AllEventCollections.AddOrUpdateCollection(reverseRoute.Id, forwardEntry.Collection);
        }

        return reverseRoute;
    }

    private string GetLocationNameFromId(string venueId)
    {
        // Helper to get friendly Venue name from ID for route naming
        if (string.IsNullOrEmpty(venueId))
            throw new InvalidDataException("GetLocationNameFromId called with null/empty venueId");

        Venue venue = _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);
        if (venue == null)
            return venueId.Replace("_", " ").Replace("-", " "); // Fallback to formatted ID if venue not found
        return venue.Name;
    }

    private void ValidateCrossroadsConfiguration()
    {
        // Group Locations by Venue using tuples
        List<(string VenueId, List<Location> Locations)> spotsByLocation = new List<(string VenueId, List<Location> Locations)>();
        foreach (Location location in _gameWorld.Locations)
        {
            int groupIndex = spotsByLocation.FindIndex(g => g.VenueId == location.VenueId);
            if (groupIndex == -1)
            {
                List<Location> Locations = new List<Location>();
                Locations.Add(location);
                spotsByLocation.Add((location.VenueId, Locations));
            }
            else
            {
                spotsByLocation[groupIndex].Locations.Add(location);
            }
        }

        // Validate each venue has exactly one crossroads location
        foreach (Venue venue in _gameWorld.Venues)
        {
            (string VenueId, List<Location> Locations) locationGroup = spotsByLocation.FirstOrDefault(g => g.VenueId == venue.Id);
            if (locationGroup.VenueId == null)
            {
                throw new InvalidOperationException($"Venue '{venue.Id}' ({venue.Name}) has no Locations defined");
            }

            List<Location> locations = locationGroup.Locations;
            List<Location> crossroadsSpots = locations
                .Where(s => s.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true)
                .ToList();

            if (crossroadsSpots.Count == 0)
            {
                throw new InvalidOperationException($"Location '{venue.Id}' ({venue.Name}) has no Locations with Crossroads property. Every Venue must have exactly one crossroads location for travel.");
            }
            else if (crossroadsSpots.Count > 1)
            {
                string spotsInfo = string.Join(", ", crossroadsSpots.Select(s => $"'{s.Id}' ({s.Name})"));
                throw new InvalidOperationException($"Location '{venue.Id}' ({venue.Name}) has {crossroadsSpots.Count} Locations with Crossroads property: {spotsInfo}. Only one crossroads location is allowed per location.");
            }
        }

        // Validate all route Locations have crossroads property
        List<string> routeSpotIds = new List<string>();
        foreach (RouteOption route in _gameWorld.Routes)
        {
            if (!routeSpotIds.Contains(route.OriginLocationSpot))
                routeSpotIds.Add(route.OriginLocationSpot);
            if (!routeSpotIds.Contains(route.DestinationLocationSpot))
                routeSpotIds.Add(route.DestinationLocationSpot);
        }

        foreach (string LocationId in routeSpotIds)
        {
            Location location = _gameWorld.GetLocation(LocationId);
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

                _gameWorld.Locations.AddOrUpdateSpot(LocationId, location);
                _gameWorld.SkeletonRegistry.AddSkeleton(LocationId, "Location");
            }

            if (!location.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true)
            {
                location.LocationProperties.Add(LocationPropertyType.Crossroads);
            }
        }
    }

    private void LoadTravelObstacles(List<TravelObstacleDTO> obstacleDtos, bool allowSkeletons)
    {
        if (obstacleDtos == null) return;

        TravelObstacleParser parser = new TravelObstacleParser();
        foreach (TravelObstacleDTO dto in obstacleDtos)
        {
            TravelObstacle obstacle = parser.ParseTravelObstacle(dto);
            _gameWorld.TravelObstacles.Add(obstacle);
        }
    }

    private void LoadConversationTrees(List<ConversationTreeDTO> conversationTrees, bool allowSkeletons)
    {
        if (conversationTrees == null) return;

        foreach (ConversationTreeDTO dto in conversationTrees)
        {
            ConversationTree tree = ConversationTreeParser.Parse(dto, _gameWorld);
            _gameWorld.ConversationTrees.Add(tree);
        }
    }

    private void LoadObservationScenes(List<ObservationSceneDTO> observationScenes, bool allowSkeletons)
    {
        if (observationScenes == null) return;

        foreach (ObservationSceneDTO dto in observationScenes)
        {
            ObservationScene scene = ObservationSceneParser.Parse(dto, _gameWorld);
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

}