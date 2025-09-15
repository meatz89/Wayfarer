using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Wayfarer.GameState.Enums;

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
/// Uses two-phase loading: parse all packages first, then apply in optimal order.
/// </summary>
public class PackageLoader
{
    private readonly GameWorld _gameWorld;
    private bool _isFirstPackage = true;
    private List<ExchangeCardEntry> _parsedExchangeCards;

    // Track loaded packages to prevent reloading
    private List<LoadedPackage> _loadedPackages = new List<LoadedPackage>();
    private List<string> _loadedPackageIds = new List<string>();

    // Track loaded entity IDs for dependency resolution
    private List<string> _loadedLocationIds = new List<string>();
    private List<string> _loadedSpotIds = new List<string>();
    private List<string> _loadedCardIds = new List<string>();
    private List<string> _loadedNpcIds = new List<string>();
    private List<string> _loadedItemIds = new List<string>();
    private List<string> _loadedRegionIds = new List<string>();
    private List<string> _loadedDistrictIds = new List<string>();

    public PackageLoader(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Two-phase package loading:
    /// Phase 1: Parse all packages
    /// Phase 2: Apply in optimal order
    /// </summary>
    private void LoadPackagesTwoPhase(List<string> packageFilePaths)
    {
        Console.WriteLine("[PackageLoader] Starting two-phase package loading...");

        // Phase 1: Parse all packages
        List<ParsedPackage> parsedPackages = new List<ParsedPackage>();
        foreach (string path in packageFilePaths)
        {
            ParsedPackage parsed = ParsePackageFile(path);
            if (parsed != null)
            {
                parsedPackages.Add(parsed);
            }
        }

        Console.WriteLine($"[PackageLoader] Phase 1 complete: Parsed {parsedPackages.Count} packages");

        // Phase 2: Apply packages in optimal order
        ApplyPackagesInOrder(parsedPackages);

        Console.WriteLine($"[PackageLoader] Phase 2 complete: Applied {parsedPackages.Count} packages");
    }

    /// <summary>
    /// Parse a package file without applying it
    /// </summary>
    private ParsedPackage ParsePackageFile(string packageFilePath)
    {
        try
        {
            string json = File.ReadAllText(packageFilePath);
            Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Check if already loaded
            if (package.PackageId != null && _loadedPackageIds.Contains(package.PackageId))
            {
                Console.WriteLine($"[PackageLoader] Skipping already loaded package: {package.PackageId}");
                return null;
            }

            ParsedPackage parsed = new ParsedPackage
            {
                Package = package,
                SourcePath = packageFilePath
            };

            parsed.CalculateLoadPriority();
            AnalyzeDependencies(parsed);

            Console.WriteLine($"[PackageLoader] Parsed package: {package.PackageId ?? Path.GetFileName(packageFilePath)}");
            return parsed;
        }
        catch (Exception ex)
        {
            throw new Exception($"[PackageLoader] Failed to parse package {packageFilePath}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Analyze dependencies in a parsed package
    /// </summary>
    private void AnalyzeDependencies(ParsedPackage parsed)
    {
        if (parsed.Package.Content == null) return;

        PackageContent content = parsed.Package.Content;
        PackageDependencies deps = parsed.Dependencies;

        // Analyze NPC dependencies
        if (content.Npcs != null)
        {
            foreach (var npc in content.Npcs)
            {
                if (!string.IsNullOrEmpty(npc.LocationId))
                    deps.RequiredLocationIds.Add(npc.LocationId);
                if (!string.IsNullOrEmpty(npc.SpotId))
                    deps.RequiredSpotIds.Add(npc.SpotId);
            }
        }

        // Analyze district dependencies
        if (content.Districts != null)
        {
            foreach (var district in content.Districts)
            {
                if (!string.IsNullOrEmpty(district.RegionId))
                    deps.RequiredRegionIds.Add(district.RegionId);
            }
        }

        // Analyze location dependencies
        // Note: LocationDTO doesn't have DistrictId, locations connect to each other via ConnectedTo
        if (content.Locations != null)
        {
            foreach (var location in content.Locations)
            {
                // Locations reference other locations
                if (location.ConnectedTo != null)
                {
                    foreach (var connectedId in location.ConnectedTo)
                    {
                        deps.RequiredLocationIds.Add(connectedId);
                    }
                }

                // Locations reference spots
                if (location.LocationSpots != null)
                {
                    foreach (var spotId in location.LocationSpots)
                    {
                        deps.RequiredSpotIds.Add(spotId);
                    }
                }
            }
        }

        // Analyze deck composition dependencies
        if (content.DeckCompositions != null)
        {
            // NPC decks (including "default" for player)
            if (content.DeckCompositions.NpcDecks != null)
            {
                foreach (var npcDeck in content.DeckCompositions.NpcDecks.Values)
                {
                    if (npcDeck.ConversationDeck != null)
                    {
                        foreach (var cardId in npcDeck.ConversationDeck.Keys)
                        {
                            deps.RequiredCardIds.Add(cardId);
                        }
                    }
                }
            }
        }

        // Analyze NPC request dependencies
        if (content.NpcRequests != null)
        {
            foreach (var request in content.NpcRequests)
            {
                if (!string.IsNullOrEmpty(request.NpcId))
                    deps.RequiredNpcIds.Add(request.NpcId);
            }
        }

        Console.WriteLine($"[PackageLoader] Dependencies analyzed: {deps.GetDependencyScore()} total dependencies");
    }

    /// <summary>
    /// Apply parsed packages in optimal order to minimize skeleton creation
    /// </summary>
    private void ApplyPackagesInOrder(List<ParsedPackage> parsedPackages)
    {
        // Sort packages by priority and dependencies
        parsedPackages.Sort((a, b) =>
        {
            // First sort by load priority (core, base, expansion, generated)
            int priorityCompare = a.LoadPriority.CompareTo(b.LoadPriority);
            if (priorityCompare != 0) return priorityCompare;

            // Then by dependency count (fewer dependencies first)
            return a.Dependencies.GetDependencyScore().CompareTo(b.Dependencies.GetDependencyScore());
        });

        Console.WriteLine("[PackageLoader] Applying packages in optimized order...");

        // Apply packages
        int loadOrder = 0;
        foreach (ParsedPackage parsed in parsedPackages)
        {
            ApplyParsedPackage(parsed, loadOrder++);
        }

        // After all packages loaded, validate and initialize systems
        ValidateCrossroadsConfiguration();
        InitializeTravelDiscoverySystem();
    }

    /// <summary>
    /// Apply a parsed package to the game world
    /// </summary>
    private void ApplyParsedPackage(ParsedPackage parsed, int loadOrder, bool isDynamic = false)
    {
        Package package = parsed.Package;
        string packageId = package.PackageId ?? Path.GetFileName(parsed.SourcePath);

        Console.WriteLine($"[PackageLoader] Applying package {loadOrder}: {packageId}");

        // Track this package as loaded
        if (package.PackageId != null)
        {
            _loadedPackageIds.Add(package.PackageId);
        }

        LoadedPackage loaded = new LoadedPackage
        {
            PackageId = packageId,
            FilePath = parsed.SourcePath,
            LoadOrder = loadOrder,
            Metadata = package.Metadata,
            IsDynamicContent = isDynamic
        };

        // Apply content in optimal order to minimize skeletons
        ApplyPackageContentOptimized(package, loaded.EntityCounts);

        _loadedPackages.Add(loaded);

        Console.WriteLine($"[PackageLoader] Package applied: {loaded.EntityCounts.TotalEntities} entities loaded");
    }

    /// <summary>
    /// Apply package content in optimal order to minimize skeleton creation
    /// </summary>
    private void ApplyPackageContentOptimized(Package package, EntityCounts counts)
    {
        // Apply starting conditions only from the first package
        if (_isFirstPackage && package.StartingConditions != null)
        {
            ApplyStartingConditions(package.StartingConditions);
            _isFirstPackage = false;
        }

        if (package.Content == null) return;

        // OPTIMAL ORDER TO MINIMIZE SKELETONS:

        // 1. Foundation entities (no dependencies)
        LoadRegions(package.Content.Regions, counts);
        LoadItems(package.Content.Items, counts);
        LoadLetterTemplates(package.Content.LetterTemplates, counts);

        // 2. Cards (referenced by many entities)
        LoadCards(package.Content.Cards, counts);

        // 3. Geographic hierarchy
        LoadDistricts(package.Content.Districts, counts);
        LoadLocations(package.Content.Locations, counts);
        LoadLocationSpots(package.Content.Spots, counts);

        // 4. Routes (depend on locations/spots)
        LoadRoutes(package.Content.Routes, counts);

        // 5. NPCs (may depend on locations/spots/cards)
        LoadNPCs(package.Content.Npcs, counts);

        // 6. Relationship entities (depend on NPCs/cards)
        LoadExchanges(package.Content.Exchanges, counts);
        InitializeNPCConversationDecks(package.Content.DeckCompositions);
        InitializeNPCRequests(package.Content.NpcRequests, package.Content.NpcGoalCards, package.Content.DeckCompositions);
        InitializeNPCExchangeDecks(package.Content.DeckCompositions);

        // 7. Complex entities (may depend on multiple entity types)
        LoadObservations(package.Content.Observations, counts);
        LoadDialogueTemplates(package.Content.DialogueTemplates, counts);
        LoadInvestigationRewards(package.Content.InvestigationRewards, counts);
        var pathCardLookup = LoadPathCards(package.Content.PathCards, counts);
        var eventCardLookup = LoadEventCards(package.Content.EventCards, counts);
        LoadTravelEvents(package.Content.TravelEvents, eventCardLookup, counts);
        LoadEventCollections(package.Content.PathCardCollections, pathCardLookup, eventCardLookup, counts);
        LoadStandingObligations(package.Content.StandingObligations, counts);
        LoadLocationActions(package.Content.LocationActions, counts);
    }

    /// <summary>
    /// Load all packages from a directory with proper ordering
    /// </summary>
    public void LoadPackagesFromDirectory(string directoryPath)
    {
        // First, load game rules if present
        string gameRulesPath = Path.Combine(directoryPath, "game_rules.json");
        if (File.Exists(gameRulesPath))
        {
            string rulesJson = File.ReadAllText(gameRulesPath);
            GameRulesParser.ParseAndApplyRules(rulesJson, GameRules.StandardRuleset);
        }

        List<string> packageFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("game_rules.json")) // Exclude game_rules.json from package loading
            .ToList();

        // Use two-phase loading
        LoadPackagesTwoPhase(packageFiles);
    }

    /// <summary>
    /// Load a dynamic package at runtime (e.g., AI-generated content)
    /// </summary>
    public bool LoadDynamicPackage(string packageFilePath)
    {
        Console.WriteLine($"[PackageLoader] Loading dynamic package: {packageFilePath}");

        // Parse the package
        ParsedPackage parsed = ParsePackageFile(packageFilePath);
        if (parsed == null)
        {
            Console.WriteLine("[PackageLoader] Failed to parse dynamic package");
            return false;
        }

        // Check if already loaded
        if (parsed.PackageId != null && _loadedPackageIds.Contains(parsed.PackageId))
        {
            Console.WriteLine($"[PackageLoader] Package {parsed.PackageId} already loaded");
            return false;
        }

        // Apply the package
        int loadOrder = _loadedPackages.Count;
        ApplyParsedPackage(parsed, loadOrder, isDynamic: true);

        Console.WriteLine($"[PackageLoader] Dynamic package loaded successfully");
        return true;
    }

    /// <summary>
    /// Load a dynamic package from JSON string (e.g., AI-generated content)
    /// </summary>
    public bool LoadDynamicPackageFromJson(string json, string packageId = null)
    {
        Console.WriteLine($"[PackageLoader] Loading dynamic package from JSON: {packageId ?? "unnamed"}");

        try
        {
            Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Set package ID if not present
            if (string.IsNullOrEmpty(package.PackageId))
            {
                package.PackageId = packageId ?? $"dynamic_{DateTime.Now.Ticks}";
            }

            // Check if already loaded
            if (_loadedPackageIds.Contains(package.PackageId))
            {
                Console.WriteLine($"[PackageLoader] Package {package.PackageId} already loaded");
                return false;
            }

            ParsedPackage parsed = new ParsedPackage
            {
                Package = package,
                SourcePath = "<dynamic>"
            };

            parsed.LoadPriority = 999; // Dynamic packages load with lowest priority
            AnalyzeDependencies(parsed);

            // Apply the package
            int loadOrder = _loadedPackages.Count;
            ApplyParsedPackage(parsed, loadOrder, isDynamic: true);

            Console.WriteLine($"[PackageLoader] Dynamic package {package.PackageId} loaded successfully");
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"[PackageLoader] Failed to load dynamic package: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get list of loaded packages for debugging/UI
    /// </summary>
    public List<LoadedPackage> GetLoadedPackages()
    {
        return new List<LoadedPackage>(_loadedPackages);
    }

    private void LoadPackageContent(Package package)
    {
        // Apply starting conditions only from the first package
        if (_isFirstPackage && package.StartingConditions != null)
        {
            ApplyStartingConditions(package.StartingConditions);
            _isFirstPackage = false;
        }

        // Load content in dependency order
        if (package.Content != null)
        {
            // Phase 0: Geographic hierarchy (regions and districts)
            LoadRegions(package.Content.Regions);
            LoadDistricts(package.Content.Districts);

            // Phase 1: Cards (foundation - NPCs reference these)
            LoadCards(package.Content.Cards);

            // Phase 2: Locations (NPCs and spots reference these)
            LoadLocations(package.Content.Locations);

            // Phase 3: Location spots (depend on locations)
            LoadLocationSpots(package.Content.Spots);

            // Phase 4: NPCs (depend on locations and cards)
            LoadNPCs(package.Content.Npcs);

            // Phase 4.5: Load exchanges BEFORE initializing exchange decks
            LoadExchanges(package.Content.Exchanges);

            // Phase 4.6: Initialize all NPC decks (after NPCs, cards, and exchanges are loaded)
            InitializeNPCConversationDecks(package.Content.DeckCompositions);
            InitializeNPCRequests(package.Content.NpcRequests, package.Content.NpcGoalCards, package.Content.DeckCompositions);
            InitializeNPCExchangeDecks(package.Content.DeckCompositions);

            // Phase 5: Routes (depend on locations)
            LoadRoutes(package.Content.Routes);

            // Phase 6: Independent content
            LoadObservations(package.Content.Observations);
            LoadInvestigationRewards(package.Content.InvestigationRewards);
            // Load path cards first, then collections that reference them
            var pathCardLookup = LoadPathCards(package.Content.PathCards);
            
            // Load event system components (normalized structure)
            var eventCardLookup = LoadEventCards(package.Content.EventCards);
            LoadTravelEvents(package.Content.TravelEvents, eventCardLookup);
            
            // Load collections (handles both path collections and event collections)
            LoadEventCollections(package.Content.PathCardCollections, pathCardLookup, eventCardLookup);
            LoadItems(package.Content.Items);
            LoadLetterTemplates(package.Content.LetterTemplates);
            LoadStandingObligations(package.Content.StandingObligations);
            LoadLocationActions(package.Content.LocationActions);
            
            // Initialize travel discovery system after all content is loaded
            InitializeTravelDiscoverySystem();
        }
    }

    private void ApplyStartingConditions(PackageStartingConditions conditions)
    {
        // Apply player initial config
        if (conditions.PlayerConfig != null)
        {
            _gameWorld.InitialPlayerConfig = conditions.PlayerConfig;
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
                _gameWorld.GetPlayer().NPCTokens[kvp.Key] = new Dictionary<ConnectionType, int>
                {
                    [ConnectionType.Trust] = kvp.Value.Trust,
                    [ConnectionType.Commerce] = kvp.Value.Commerce,
                    [ConnectionType.Status] = kvp.Value.Status,
                    [ConnectionType.Shadow] = kvp.Value.Shadow
                };
            }
        }
    }

    private void LoadRegions(List<RegionDTO> regionDtos, EntityCounts counts = null)
    {
        if (regionDtos == null) return;

        foreach (RegionDTO dto in regionDtos)
        {
            Region region = new Region
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                DistrictIds = dto.DistrictIds ?? new List<string>(),
                Government = dto.Government,
                Culture = dto.Culture,
                Population = dto.Population,
                MajorExports = dto.MajorExports ?? new List<string>(),
                MajorImports = dto.MajorImports ?? new List<string>()
            };
            _gameWorld.WorldState.Regions.Add(region);
            _loadedRegionIds.Add(region.Id);
            if (counts != null) counts.Regions++;
        }
    }

    private void LoadDistricts(List<DistrictDTO> districtDtos, EntityCounts counts = null)
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
                LocationIds = dto.LocationIds ?? new List<string>(),
                DistrictType = dto.DistrictType,
                DangerLevel = dto.DangerLevel,
                Characteristics = dto.Characteristics ?? new List<string>()
            };
            _gameWorld.WorldState.Districts.Add(district);
            _loadedDistrictIds.Add(district.Id);
            if (counts != null) counts.Districts++;
        }
    }

    private void LoadCards(List<ConversationCardDTO> cardDtos, EntityCounts counts = null)
    {
        if (cardDtos == null) return;

        foreach (ConversationCardDTO dto in cardDtos)
        {
            // Use static method from ConversationCardParser
            ConversationCard card = ConversationCardParser.ConvertDTOToCard(dto);
            _gameWorld.AllCardDefinitions[card.Id] = card;
            _loadedCardIds.Add(card.Id);
            if (counts != null) counts.Cards++;
        }
    }

    private void LoadLocations(List<LocationDTO> locationDtos, EntityCounts counts = null)
    {
        if (locationDtos == null) return;

        foreach (LocationDTO dto in locationDtos)
        {
            // Check if this location was previously a skeleton, if so replace it
            Location? existingSkeleton = _gameWorld.WorldState.locations
                .FirstOrDefault(l => l.Id == dto.Id && l.IsSkeleton);

            if (existingSkeleton != null)
            {
                _gameWorld.WorldState.locations.Remove(existingSkeleton);
                _gameWorld.Locations.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);
            }

            Location location = LocationParser.ConvertDTOToLocation(dto);
            _gameWorld.Locations.Add(location);
            _gameWorld.WorldState.locations.Add(location);
            _loadedLocationIds.Add(location.Id);
            if (counts != null) counts.Locations++;
        }
    }

    private void LoadLocationSpots(List<LocationSpotDTO> spotDtos, EntityCounts counts = null)
    {
        if (spotDtos == null) return;

        foreach (LocationSpotDTO dto in spotDtos)
        {
            // Check if this spot was previously a skeleton, if so replace it
            LocationSpot? existingSkeleton = _gameWorld.WorldState.locationSpots
                .FirstOrDefault(s => s.SpotID == dto.Id && s.IsSkeleton);

            if (existingSkeleton != null)
            {
                _gameWorld.WorldState.locationSpots.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);

                // Remove from primary spots dictionary if exists
                _gameWorld.Spots.Remove(dto.Id);
            }

            LocationSpot spot = LocationSpotParser.ConvertDTOToLocationSpot(dto);
            _gameWorld.WorldState.locationSpots.Add(spot);

            // Add to primary spots dictionary
            _gameWorld.Spots[spot.SpotID] = spot;
            _loadedSpotIds.Add(spot.SpotID);
            if (counts != null) counts.Spots++;
        }
    }

    private void LoadNPCs(List<NPCDTO> npcDtos, EntityCounts counts = null)
    {
        if (npcDtos == null) return;

        foreach (NPCDTO dto in npcDtos)
        {
            // Check if NPC references a location that doesn't exist
            if (!string.IsNullOrEmpty(dto.LocationId) &&
                !_gameWorld.WorldState.locations.Any(l => l.Id == dto.LocationId))
            {
                // Create skeleton location
                Location skeletonLocation = SkeletonGenerator.GenerateSkeletonLocation(
                    dto.LocationId,
                    $"npc_{dto.Id}_reference");

                _gameWorld.WorldState.locations.Add(skeletonLocation);
                _gameWorld.Locations.Add(skeletonLocation);
                _gameWorld.SkeletonRegistry[dto.LocationId] = "Location";

                // Also create a skeleton spot for the location
                string hubSpotId = $"{dto.LocationId}_hub";
                LocationSpot hubSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    hubSpotId,
                    dto.LocationId,
                    $"location_{dto.LocationId}_hub");

                _gameWorld.WorldState.locationSpots.Add(hubSpot);
                _gameWorld.Spots[hubSpotId] = hubSpot;
                _gameWorld.SkeletonRegistry[hubSpot.SpotID] = "LocationSpot";
            }

            // Check if NPC references a spot that doesn't exist
            if (!string.IsNullOrEmpty(dto.SpotId) &&
                !_gameWorld.WorldState.locationSpots.Any(s => s.SpotID == dto.SpotId))
            {
                // Create skeleton spot
                LocationSpot skeletonSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    dto.SpotId,
                    dto.LocationId ?? "unknown_location",
                    $"npc_{dto.Id}_spot_reference");

                _gameWorld.WorldState.locationSpots.Add(skeletonSpot);
                _gameWorld.SkeletonRegistry[dto.SpotId] = "LocationSpot";
            }

            // Check if this NPC was previously a skeleton, if so replace it and preserve persistent decks
            NPC? existingSkeleton = _gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
            if (existingSkeleton != null)
            {
                Console.WriteLine($"[PackageLoader] Replacing skeleton NPC '{existingSkeleton.Name}' (ID: {existingSkeleton.ID}) with real content");

                // Preserve all cards from the 5 persistent decks
                var preservedProgressionCards = existingSkeleton.ProgressionDeck?.GetAllCards()?.ToList() ?? new List<ConversationCard>();
                var preservedExchangeCards = existingSkeleton.ExchangeDeck?.ToList() ?? new List<ExchangeCard>();
                var preservedObservationCards = existingSkeleton.ObservationDeck?.GetAllCards()?.ToList() ?? new List<ConversationCard>();
                var preservedBurdenCards = existingSkeleton.BurdenDeck?.GetAllCards()?.ToList() ?? new List<ConversationCard>();
                var preservedRequests = existingSkeleton.Requests?.ToList() ?? new List<NPCRequest>();

                int totalPreservedCards = preservedProgressionCards.Count + preservedExchangeCards.Count +
                                        preservedObservationCards.Count + preservedBurdenCards.Count +
                                        preservedRequests.SelectMany(r => r.PromiseCardIds).Count();

                Console.WriteLine($"[PackageLoader] Preserving {totalPreservedCards} cards from persistent decks:");
                Console.WriteLine($"  - Progression: {preservedProgressionCards.Count} cards");
                Console.WriteLine($"  - Exchange: {preservedExchangeCards.Count} cards");
                Console.WriteLine($"  - Observation: {preservedObservationCards.Count} cards");
                Console.WriteLine($"  - Burden: {preservedBurdenCards.Count} cards");
                Console.WriteLine($"  - Requests: {preservedRequests.Count} request objects");

                // Remove skeleton from game world
                _gameWorld.NPCs.Remove(existingSkeleton);
                _gameWorld.WorldState.NPCs.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);

                // Create new NPC from DTO
                NPC npc = NPCParser.ConvertDTOToNPC(dto);

                // Restore preserved cards to the new NPC's persistent decks
                if (preservedProgressionCards.Any())
                {
                    foreach (var card in preservedProgressionCards)
                    {
                        npc.ProgressionDeck.AddCard(card);
                    }
                }

                if (preservedExchangeCards.Any())
                {
                    npc.ExchangeDeck.AddRange(preservedExchangeCards);
                }

                if (preservedObservationCards.Any())
                {
                    foreach (var card in preservedObservationCards)
                    {
                        npc.ObservationDeck.AddCard(card);
                    }
                }

                if (preservedBurdenCards.Any())
                {
                    foreach (var card in preservedBurdenCards)
                    {
                        npc.BurdenDeck.AddCard(card);
                    }
                }

                if (preservedRequests.Any())
                {
                    // Merge preserved requests with new NPC's requests
                    // Note: This assumes no ID conflicts between preserved and new requests
                    npc.Requests.AddRange(preservedRequests);
                }

                Console.WriteLine($"[PackageLoader] Successfully replaced skeleton with real NPC and preserved all persistent deck cards");

                // Add the new NPC to game world
                _gameWorld.NPCs.Add(npc);
                _gameWorld.WorldState.NPCs.Add(npc);
                _loadedNpcIds.Add(npc.ID);
                if (counts != null) counts.NPCs++;
            }
            else
            {
                // No skeleton to replace, just create new NPC normally
                NPC npc = NPCParser.ConvertDTOToNPC(dto);
                _gameWorld.NPCs.Add(npc);
                _gameWorld.WorldState.NPCs.Add(npc);
                _loadedNpcIds.Add(npc.ID);
                if (counts != null) counts.NPCs++;
            }
        }
    }

    private void LoadRoutes(List<RouteDTO> routeDtos, EntityCounts counts = null)
    {
        if (routeDtos == null)
        {
            Console.WriteLine("[PackageLoader] No routes to load - routeDtos is null");
            return;
        }

        Console.WriteLine($"[PackageLoader] Loading {routeDtos.Count} routes...");
        Console.WriteLine($"[PackageLoader] Currently loaded spots: {string.Join(", ", _gameWorld.Spots.Keys)}");

        // Create skeleton spots for any missing references
        foreach (RouteDTO dto in routeDtos)
        {
            // Check origin spot - create skeleton if missing
            LocationSpot originSpot = _gameWorld.GetSpot(dto.OriginSpotId);
            if (originSpot == null)
            {
                Console.WriteLine($"[PackageLoader] Route '{dto.Id}' references missing origin spot '{dto.OriginSpotId}' - creating skeleton");

                // Create skeleton spot with crossroads property (required for routes)
                originSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    dto.OriginSpotId,
                    dto.OriginLocationId ?? "unknown_location",
                    $"route_{dto.Id}_origin"
                );

                // Ensure skeleton has crossroads property for route connectivity
                if (!originSpot.SpotProperties.Contains(SpotPropertyType.Crossroads))
                {
                    originSpot.SpotProperties.Add(SpotPropertyType.Crossroads);
                }

                _gameWorld.WorldState.locationSpots.Add(originSpot);
                _gameWorld.Spots[dto.OriginSpotId] = originSpot;
                _gameWorld.SkeletonRegistry[dto.OriginSpotId] = "LocationSpot";

                Console.WriteLine($"[PackageLoader] Created skeleton spot '{dto.OriginSpotId}' for route '{dto.Id}'");
            }

            // Check destination spot - create skeleton if missing
            LocationSpot destSpot = _gameWorld.GetSpot(dto.DestinationSpotId);
            if (destSpot == null)
            {
                Console.WriteLine($"[PackageLoader] Route '{dto.Id}' references missing destination spot '{dto.DestinationSpotId}' - creating skeleton");

                // Create skeleton spot with crossroads property (required for routes)
                destSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    dto.DestinationSpotId,
                    dto.DestinationLocationId ?? "unknown_location",
                    $"route_{dto.Id}_destination"
                );

                // Ensure skeleton has crossroads property for route connectivity
                if (!destSpot.SpotProperties.Contains(SpotPropertyType.Crossroads))
                {
                    destSpot.SpotProperties.Add(SpotPropertyType.Crossroads);
                }

                _gameWorld.WorldState.locationSpots.Add(destSpot);
                _gameWorld.Spots[dto.DestinationSpotId] = destSpot;
                _gameWorld.SkeletonRegistry[dto.DestinationSpotId] = "LocationSpot";

                Console.WriteLine($"[PackageLoader] Created skeleton spot '{dto.DestinationSpotId}' for route '{dto.Id}'");
            }
        }

        // BIDIRECTIONAL ROUTE PRINCIPLE: Routes are defined once in JSON but automatically
        // generate both directions. This eliminates redundancy and ensures consistency.
        // The return journey has segments in reversed order (A->B->C becomes C->B->A).
        foreach (RouteDTO dto in routeDtos)
        {
            // Create the forward route from JSON
            RouteOption forwardRoute = ConvertRouteDTOToModel(dto);
            _gameWorld.WorldState.Routes.Add(forwardRoute);
            Console.WriteLine($"[PackageLoader] Added route {forwardRoute.Id}: {forwardRoute.OriginLocationSpot} -> {forwardRoute.DestinationLocationSpot}");
            AddRouteToLocationConnections(forwardRoute);
            if (counts != null) counts.Routes++;

            // Automatically generate the reverse route
            RouteOption reverseRoute = GenerateReverseRoute(forwardRoute);
            _gameWorld.WorldState.Routes.Add(reverseRoute);
            Console.WriteLine($"[PackageLoader] Generated reverse route {reverseRoute.Id}: {reverseRoute.OriginLocationSpot} -> {reverseRoute.DestinationLocationSpot}");
            AddRouteToLocationConnections(reverseRoute);
            if (counts != null) counts.Routes++;
        }

        Console.WriteLine($"[PackageLoader] Completed loading {routeDtos.Count} routes. Total routes with bidirectional: {_gameWorld.WorldState.Routes.Count}");
    }

    private void AddRouteToLocationConnections(RouteOption route)
    {
        Console.WriteLine($"[PackageLoader] Adding route {route.Id} to location connections...");

        // Find the origin location for this route
        string originLocationId = GetLocationIdFromSpotId(route.OriginLocationSpot);
        string destinationLocationId = GetLocationIdFromSpotId(route.DestinationLocationSpot);

        Console.WriteLine($"[PackageLoader] Route {route.Id}: Origin spot '{route.OriginLocationSpot}' -> Location '{originLocationId}', Destination spot '{route.DestinationLocationSpot}' -> Location '{destinationLocationId}'");

        if (originLocationId == null || destinationLocationId == null)
        {
            throw new Exception($"[PackageLoader] Route {route.Id} has spots without LocationId. Origin: {route.OriginLocationSpot} -> {originLocationId}, Destination: {route.DestinationLocationSpot} -> {destinationLocationId}");
        }

        // Find the origin location
        Location? originLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == originLocationId);
        if (originLocation == null)
        {
            Console.WriteLine($"[PackageLoader] Warning: Could not find origin location {originLocationId} for route {route.Id}");
            return;
        }

        // Find or create a connection to the destination location
        LocationConnection? connection = originLocation.Connections.FirstOrDefault(c => c.DestinationLocationId == destinationLocationId);
        if (connection == null)
        {
            connection = new LocationConnection
            {
                DestinationLocationId = destinationLocationId,
                RouteOptions = new List<RouteOption>()
            };
            originLocation.Connections.Add(connection);
            Console.WriteLine($"[PackageLoader] Created new connection from {originLocationId} to {destinationLocationId}");
        }
        else
        {
            Console.WriteLine($"[PackageLoader] Found existing connection from {originLocationId} to {destinationLocationId}");
        }

        // Add the route to the connection
        if (!connection.RouteOptions.Any(r => r.Id == route.Id))
        {
            connection.RouteOptions.Add(route);
            Console.WriteLine($"[PackageLoader] Added route {route.Id} to connection. Connection now has {connection.RouteOptions.Count} routes");
        }
        else
        {
            Console.WriteLine($"[PackageLoader] Route {route.Id} already exists in connection");
        }
    }

    private string GetLocationIdFromSpotId(string spotId)
    {
        LocationSpot? spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
        return spot?.LocationId;
    }

    private void LoadObservations(List<ObservationDTO> observationDtos, EntityCounts counts = null)
    {
        if (observationDtos == null) return;

        foreach (ObservationDTO dto in observationDtos)
        {
            // Convert to ConversationCard for the existing system
            ConversationCard observation = ConvertObservationDTOToCard(dto);
            _gameWorld.PlayerObservationCards.Add(observation);

            // Also create Observation domain object and store in GameWorld
            Observation observationDomain = ConvertDTOToObservation(dto);
            _gameWorld.Observations.Add(observationDomain);

            if (counts != null) counts.Cards++;
        }
    }

    private void LoadDialogueTemplates(DialogueTemplates dialogueTemplates, EntityCounts counts = null)
    {
        if (dialogueTemplates == null) return;

        Console.WriteLine($"[PackageLoader] Loading dialogue templates...");
        _gameWorld.DialogueTemplates = dialogueTemplates;
        if (counts != null) counts.Cards++; // Count as one entity
        Console.WriteLine($"[PackageLoader] Dialogue templates loaded successfully");
    }

    private List<PathCardEntry> LoadPathCards(List<PathCardDTO> pathCardDtos, EntityCounts counts = null)
    {
        if (pathCardDtos == null) return new List<PathCardEntry>();

        Console.WriteLine($"[PackageLoader] Loading {pathCardDtos.Count} path cards...");

        var pathCardLookup = new List<PathCardEntry>();
        foreach (PathCardDTO dto in pathCardDtos)
        {
            pathCardLookup.Add(new PathCardEntry { Id = dto.Id, Card = dto });
            Console.WriteLine($"[PackageLoader] Loaded path card '{dto.Id}': {dto.Name}");
            if (counts != null) counts.Cards++;
        }

        Console.WriteLine($"[PackageLoader] Completed loading path cards. Total: {pathCardLookup.Count}");
        return pathCardLookup;
    }

    private List<PathCardEntry> LoadEventCards(List<PathCardDTO> eventCardDtos, EntityCounts counts = null)
    {
        if (eventCardDtos == null) return new List<PathCardEntry>();

        Console.WriteLine($"[PackageLoader] Loading {eventCardDtos.Count} event cards...");

        var eventCardLookup = new List<PathCardEntry>();
        foreach (PathCardDTO dto in eventCardDtos)
        {
            eventCardLookup.Add(new PathCardEntry { Id = dto.Id, Card = dto });
            Console.WriteLine($"[PackageLoader] Loaded event card '{dto.Id}': {dto.Name}");
            if (counts != null) counts.Events++;
        }

        Console.WriteLine($"[PackageLoader] Completed loading event cards. Total: {eventCardLookup.Count}");
        return eventCardLookup;
    }

    private void LoadTravelEvents(List<TravelEventDTO> travelEventDtos, List<PathCardEntry> eventCardLookup, EntityCounts counts = null)
    {
        if (travelEventDtos == null) return;
        
        Console.WriteLine($"[PackageLoader] Loading {travelEventDtos.Count} travel events...");
        
        foreach (TravelEventDTO dto in travelEventDtos)
        {
            // Embed actual event cards if this event has event card IDs (for JSON loaded events)
            if (dto.EventCardIds != null && dto.EventCards.Count == 0)
            {
                foreach (string cardId in dto.EventCardIds)
                {
                    var eventCardEntry = eventCardLookup.FirstOrDefault(e => e.Id == cardId);
                    if (eventCardEntry != null)
                    {
                        dto.EventCards.Add(eventCardEntry.Card);
                    }
                }
            }
            
            _gameWorld.AllTravelEvents[dto.Id] = dto;
            Console.WriteLine($"[PackageLoader] Loaded travel event '{dto.Id}': {dto.Name} with {dto.EventCards.Count} event cards");
            if (counts != null) counts.Events++;
        }
        
        Console.WriteLine($"[PackageLoader] Completed loading travel events. Total: {_gameWorld.AllTravelEvents.Count}");
    }
    
    
    private void LoadEventCollections(List<PathCardCollectionDTO> collectionDtos, List<PathCardEntry> pathCardLookup, List<PathCardEntry> eventCardLookup, EntityCounts counts = null)
    {
        if (collectionDtos == null) return;

        Console.WriteLine($"[PackageLoader] Loading {collectionDtos.Count} collections and embedding cards directly...");

        foreach (PathCardCollectionDTO dto in collectionDtos)
        {
            // Embed actual path cards if this collection has path card IDs (for JSON loaded collections)
            if (dto.PathCards != null && dto.PathCards.Count == 0 && dto.PathCardIds != null)
            {
                foreach (string cardId in dto.PathCardIds)
                {
                    var pathCardEntry = pathCardLookup.FirstOrDefault(p => p.Id == cardId);
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
                _gameWorld.AllEventCollections[dto.Id] = dto;
                Console.WriteLine($"[PackageLoader] Loaded event collection '{dto.Id}': {dto.Name} with {dto.Events.Count} events");
            }
            else if (isPathCollection)
            {
                // This is a path collection - contains actual path cards for FixedPath segments
                _gameWorld.AllPathCollections[dto.Id] = dto;
                Console.WriteLine($"[PackageLoader] Loaded path collection '{dto.Id}': {dto.Name} with {dto.PathCards.Count} path cards");
            }
            else
            {
                // Fallback: treat as path collection if no clear indicators
                _gameWorld.AllPathCollections[dto.Id] = dto;
                Console.WriteLine($"[PackageLoader] Loaded collection '{dto.Id}' as path collection (default): {dto.Name}");
            }
        }

        Console.WriteLine($"[PackageLoader] Completed loading collections. Path collections: {_gameWorld.AllPathCollections.Count}, Event collections: {_gameWorld.AllEventCollections.Count}");
    }

    /// <summary>
    /// Initialize player starter deck and NPC progression decks
    /// </summary>
    private void InitializeNPCConversationDecks(DeckCompositionDTO deckCompositions)
    {
        Console.WriteLine("[PackageLoader] Initializing player starter deck and NPC progression decks...");

        if (deckCompositions == null)
        {
            Console.WriteLine("[PackageLoader] Error: No deck compositions defined in package");
            return;
        }

        // Initialize player's starter deck with default/universal cards
        InitializePlayerStarterDeck(deckCompositions);

        // Initialize NPC progression decks with unique cards
        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                npc.ProgressionDeck = new CardDeck();

                // Only load NPC-specific progression cards, not default deck
                DeckDefinitionDTO deckDef = null;
                if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.ID))
                {
                    deckDef = deckCompositions.NpcDecks[npc.ID];
                    Console.WriteLine($"[PackageLoader] Loading progression cards for {npc.Name}");
                }
                // NPCs no longer get default deck - that's for player only

                if (deckDef?.ProgressionDeck != null)
                {
                    // Add NPC-specific progression cards
                    foreach (KeyValuePair<string, int> kvp in deckDef.ProgressionDeck)
                    {
                        string cardId = kvp.Key;
                        int count = kvp.Value;

                        if (_gameWorld.AllCardDefinitions.ContainsKey(cardId))
                        {
                            ConversationCard cardTemplate = _gameWorld.AllCardDefinitions[cardId] as ConversationCard;
                            // Add all cards defined for this NPC - these are progression cards by definition
                            if (cardTemplate != null)
                            {
                                // MinimumTokensRequired must be set in JSON - no defaults

                                // Add multiple copies as specified
                                for (int i = 0; i < count; i++)
                                {
                                    npc.ProgressionDeck.AddCard(cardTemplate);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[PackageLoader] Warning: Card '{cardId}' not found for deck composition");
                        }
                    }

                    Console.WriteLine($"[PackageLoader] Initialized {npc.Name}'s progression deck with {npc.ProgressionDeck.Count} cards");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[PackageLoader] Failed to initialize conversation deck for NPC {npc.Name}: {ex.Message}", ex);
            }
        }

        Console.WriteLine("[PackageLoader] NPC conversation deck initialization completed");
    }


    /// <summary>
    /// Initialize one-time requests for NPCs from NpcGoalCards and deck compositions
    /// </summary>
    private void InitializeNPCRequests(List<NPCRequestDTO> npcRequestDtos, List<NPCGoalCardDTO> npcGoalCardDtos, DeckCompositionDTO deckCompositions)
    {
        Console.WriteLine("[PackageLoader] Initializing NPC one-time requests...");

        // First load NPCRequest bundles from npcRequests section
        if (npcRequestDtos != null && npcRequestDtos.Count > 0)
        {
            foreach (NPCRequestDTO requestDto in npcRequestDtos)
            {
                try
                {
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == requestDto.NpcId);
                    if (npc == null)
                    {
                        Console.WriteLine($"[PackageLoader] Warning: NPC '{requestDto.NpcId}' not found for request '{requestDto.Id}'");
                        continue;
                    }

                    NPCRequest request = new NPCRequest
                    {
                        Id = requestDto.Id,
                        Name = requestDto.Name,
                        Description = requestDto.Description,
                        NpcRequestText = requestDto.NpcRequestText,
                        Status = RequestStatus.Available
                    };

                    // Load request card IDs (validate they exist but store only IDs)
                    if (requestDto.RequestCards != null)
                    {
                        foreach (string cardId in requestDto.RequestCards)
                        {
                            if (_gameWorld.AllCardDefinitions.ContainsKey(cardId))
                            {
                                request.RequestCardIds.Add(cardId);
                                Console.WriteLine($"[PackageLoader] Added request card ID '{cardId}' to request '{requestDto.Id}'");
                            }
                            else
                            {
                                Console.WriteLine($"[PackageLoader] Warning: Request card '{cardId}' not found in GameWorld.AllCardDefinitions");
                            }
                        }
                    }

                    // Load promise card IDs (validate they exist but store only IDs)
                    if (requestDto.PromiseCards != null)
                    {
                        foreach (string cardId in requestDto.PromiseCards)
                        {
                            if (_gameWorld.AllCardDefinitions.ContainsKey(cardId))
                            {
                                request.PromiseCardIds.Add(cardId);
                                Console.WriteLine($"[PackageLoader] Added promise card ID '{cardId}' to request '{requestDto.Id}'");
                            }
                            else
                            {
                                Console.WriteLine($"[PackageLoader] Warning: Promise card '{cardId}' not found in GameWorld.AllCardDefinitions");
                            }
                        }
                    }

                    if (request.RequestCardIds.Count > 0 || request.PromiseCardIds.Count > 0)
                    {
                        npc.Requests.Add(request);
                        Console.WriteLine($"[PackageLoader] Added request '{requestDto.Id}' to NPC '{npc.Name}' with {request.RequestCardIds.Count} request card IDs and {request.PromiseCardIds.Count} promise card IDs");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"[PackageLoader] Failed to load request '{requestDto.Id}': {ex.Message}", ex);
                }
            }
            
            Console.WriteLine($"[PackageLoader] Loaded {npcRequestDtos.Count} NPC request bundles");
        }
    }


    /// <summary>
    /// Initialize exchange decks for Mercantile NPCs only
    /// </summary>
    private void InitializeNPCExchangeDecks(DeckCompositionDTO deckCompositions)
    {
        Console.WriteLine("[PackageLoader] Initializing NPC exchange decks...");

        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                List<ExchangeCard> npcExchangeCards = new List<ExchangeCard>();

                // Check deck compositions for this NPC's exchange deck
                DeckDefinitionDTO deckDef = null;
                if (deckCompositions != null)
                {
                    // Check for NPC-specific deck first
                    if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.ID))
                    {
                        deckDef = deckCompositions.NpcDecks[npc.ID];
                        Console.WriteLine($"[PackageLoader] Using custom exchange deck for {npc.Name}");
                    }
                    else if (deckCompositions.DefaultDeck != null)
                    {
                        deckDef = deckCompositions.DefaultDeck;
                        Console.WriteLine($"[PackageLoader] Using default exchange deck for {npc.Name}");
                    }
                }

                // Build exchange deck from composition
                if (deckDef != null && deckDef.ExchangeDeck != null)
                {
                    foreach (var kvp in deckDef.ExchangeDeck)
                    {
                        string cardId = kvp.Key;
                        int count = kvp.Value;

                        // Find the exchange card from the parsed exchange cards
                        var exchangeEntry = _parsedExchangeCards?.FirstOrDefault(e => e.Id == cardId);
                        if (exchangeEntry != null)
                        {
                            ExchangeCard exchangeCard = exchangeEntry.Card;
                            // Add the specified number of copies to the deck
                            for (int i = 0; i < count; i++)
                            {
                                // Use the DeepClone method to create a copy
                                ExchangeCard cardCopy = exchangeCard.DeepClone();
                                npcExchangeCards.Add(cardCopy);
                                Console.WriteLine($"[PackageLoader] Added exchange card {cardId} to {npc.Name}'s deck");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[PackageLoader] WARNING: Exchange card {cardId} not found in parsed exchanges for {npc.Name}");
                        }
                    }
                }
                else if (npc.PersonalityType == PersonalityType.MERCANTILE)
                {
                    // Create default exchanges for mercantile NPCs without specific exchanges
                    npcExchangeCards = ExchangeParser.CreateDefaultExchangesForNPC(npc);
                    if (npcExchangeCards.Count > 0)
                    {
                        Console.WriteLine($"[PackageLoader] Created {npcExchangeCards.Count} default exchange cards for {npc.Name}");
                    }
                }

                // Initialize exchange deck
                npc.InitializeExchangeDeck(npcExchangeCards);

                if (npcExchangeCards.Count > 0)
                {
                    Console.WriteLine($"[PackageLoader] Initialized exchange deck for {npc.Name} with {npcExchangeCards.Count} cards");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[PackageLoader] Failed to initialize exchange deck for NPC {npc.Name}: {ex.Message}", ex);
            }
        }

        Console.WriteLine("[PackageLoader] NPC exchange deck initialization completed");
    }

    private void LoadItems(List<ItemDTO> itemDtos, EntityCounts counts = null)
    {
        if (itemDtos == null) return;

        foreach (ItemDTO dto in itemDtos)
        {
            Item item = ItemParser.ConvertDTOToItem(dto);
            _gameWorld.WorldState.Items.Add(item);
            _loadedItemIds.Add(item.Id);
            if (counts != null) counts.Items++;
        }
    }

    private void LoadLetterTemplates(List<LetterTemplateDTO> letterDtos, EntityCounts counts = null)
    {
        if (letterDtos == null) return;

        foreach (LetterTemplateDTO dto in letterDtos)
        {
            LetterTemplate letter = LetterTemplateParser.ConvertDTOToLetterTemplate(dto);
            _gameWorld.WorldState.LetterTemplates.Add(letter);
            if (counts != null) counts.LetterTemplates++;
        }
    }

    private void LoadStandingObligations(List<StandingObligationDTO> obligationDtos, EntityCounts counts = null)
    {
        if (obligationDtos == null) return;

        foreach (StandingObligationDTO dto in obligationDtos)
        {
            StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(dto);
            _gameWorld.WorldState.StandingObligationTemplates.Add(obligation);
            if (counts != null) counts.Obligations++;
        }
    }

    private void LoadLocationActions(List<LocationActionDTO> locationActionDtos, EntityCounts counts = null)
    {
        if (locationActionDtos == null) return;

        foreach (LocationActionDTO dto in locationActionDtos)
        {
            LocationAction locationAction = ConvertLocationActionDTOToModel(dto);
            _gameWorld.LocationActions.Add(locationAction);
            if (counts != null) counts.Cards++; // Location actions are card-like
        }
    }

    // Conversion methods that don't have dedicated parsers yet


    private RouteOption ConvertRouteDTOToModel(RouteDTO dto)
    {
        RouteOption route = new RouteOption
        {
            Id = dto.Id,
            Name = dto.Name ?? "",
            OriginLocationSpot = dto.OriginSpotId ?? "",
            DestinationLocationSpot = dto.DestinationSpotId ?? "",
            Method = Enum.TryParse<TravelMethods>(dto.Method, out TravelMethods method) ? method : TravelMethods.Walking,
            BaseCoinCost = dto.BaseCoinCost,
            BaseStaminaCost = dto.BaseStaminaCost,
            TravelTimeSegments = dto.TravelTimeSegments,
            IsDiscovered = dto.IsDiscovered,
            Description = dto.Description ?? "",
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
                    Type = segmentType
                };
                
                // Set collection properties based on segment type using normalized properties
                if (segmentType == SegmentType.FixedPath)
                {
                    // FixedPath segments use pathCollectionId from JSON
                    segment.PathCollectionId = segmentDto.PathCollectionId;
                    if (!string.IsNullOrEmpty(segment.PathCollectionId))
                    {
                        Console.WriteLine($"[PackageLoader] FixedPath segment {segmentDto.SegmentNumber} uses path collection '{segment.PathCollectionId}'");
                    }
                }
                else if (segmentType == SegmentType.Event)
                {
                    // Event segments use eventCollectionId from JSON
                    segment.EventCollectionId = segmentDto.EventCollectionId;
                    if (!string.IsNullOrEmpty(segment.EventCollectionId))
                    {
                        Console.WriteLine($"[PackageLoader] Event segment {segmentDto.SegmentNumber} uses event collection '{segment.EventCollectionId}'");
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
        
        return route;
    }

    private ConversationCard ConvertObservationDTOToCard(ObservationDTO dto)
    {
        // Observations become player cards
        return new ConversationCard
        {
            Id = dto.Id,
            Description = dto.DisplayText ?? "",
            Focus = dto.Focus,
            TokenType = ConnectionType.Trust,
            Difficulty = Difficulty.Medium,
            CardType = CardType.Observation,
            Persistence = PersistenceType.Thought, // Observations persist through LISTEN
            SuccessType = SuccessEffectType.None,
            FailureType = FailureEffectType.None,
            ExhaustType = ExhaustEffectType.None,
            PersonalityTypes = new List<string>(),
            LevelBonuses = new List<CardLevelBonus>(),
            DialogueFragment = "",
            VerbPhrase = ""
        };
    }

    private Observation ConvertDTOToObservation(ObservationDTO dto)
    {
        // Parse observation type from category
        ObservationType observationType = ObservationType.Normal;
        if (dto.Category != null && Enum.TryParse<ObservationType>(dto.Category, out ObservationType parsedType))
        {
            observationType = parsedType;
        }

        return new Observation
        {
            Id = dto.Id ?? "",
            Text = dto.DisplayText ?? dto.Description ?? dto.Name ?? "",
            Type = observationType,
            AttentionCost = 0, // From DTO or default to 0
            RelevantNPCs = new string[0], // Empty by default - could be populated from properties
            CreatesState = null, // No state creation from DTO
            CardTemplate = dto.Id ?? "", // Use ID as template
            Description = dto.Description ?? dto.DisplayText ?? "",
            ProvidesInfo = null, // Could be derived from properties
            CreatesUrgency = false, // Default false
            Automatic = false, // Default false
            SpotId = null // Will be set by location-specific loading if needed
        };
    }

    private LocationAction ConvertLocationActionDTOToModel(LocationActionDTO dto)
    {
        LocationAction action = new LocationAction
        {
            Id = dto.Id ?? "",
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            Cost = dto.Cost ?? new Dictionary<string, int>(),
            Reward = dto.Reward ?? new Dictionary<string, int>(),
            TimeRequired = dto.TimeRequired,
            Availability = dto.Availability ?? new List<string>(),
            Icon = dto.Icon ?? "💼",
            Priority = dto.Priority,
            ActionType = dto.ActionType ?? ""
        };

        // Parse required properties
        if (dto.RequiredProperties != null)
        {
            foreach (string prop in dto.RequiredProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(prop, true, out SpotPropertyType propertyType))
                {
                    action.RequiredProperties.Add(propertyType);
                }
            }
        }

        // Parse optional properties
        if (dto.OptionalProperties != null)
        {
            foreach (string prop in dto.OptionalProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(prop, true, out SpotPropertyType propertyType))
                {
                    action.OptionalProperties.Add(propertyType);
                }
            }
        }

        // Parse excluded properties
        if (dto.ExcludedProperties != null)
        {
            foreach (string prop in dto.ExcludedProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(prop, true, out SpotPropertyType propertyType))
                {
                    action.ExcludedProperties.Add(propertyType);
                }
            }
        }

        return action;
    }

    private void LoadInvestigationRewards(List<ObservationRewardDTO> investigationRewardDtos, EntityCounts counts = null)
    {
        if (investigationRewardDtos == null) return;

        foreach (ObservationRewardDTO dto in investigationRewardDtos)
        {
            // Find the target location
            Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
            if (location != null)
            {
                // Convert DTO to domain model
                ObservationReward reward = ConvertObservationRewardDTOToModel(dto);
                location.ObservationRewards.Add(reward);
                if (counts != null) counts.Cards++;
            }
            else
            {
                // Create skeleton location if not found
                Location skeletonLocation = SkeletonGenerator.GenerateSkeletonLocation(dto.LocationId, $"investigation_reward_{dto.LocationId}");
                ObservationReward reward = ConvertObservationRewardDTOToModel(dto);
                skeletonLocation.ObservationRewards.Add(reward);
                _gameWorld.Locations.Add(skeletonLocation);
                _gameWorld.SkeletonRegistry[dto.LocationId] = "Location";
                if (counts != null) counts.Cards++;
            }
        }
    }

    private ObservationReward ConvertObservationRewardDTOToModel(ObservationRewardDTO dto)
    {
        return new ObservationReward
        {
            LocationId = dto.LocationId,
            FamiliarityRequired = dto.FamiliarityRequired,
            PriorObservationRequired = dto.PriorObservationRequired,
            ObservationCard = new ObservationCardReward
            {
                Id = dto.ObservationCard.Id,
                Name = dto.ObservationCard.Name,
                TargetNpcId = dto.ObservationCard.TargetNpcId,
                Effect = dto.ObservationCard.Effect,
                Description = dto.ObservationCard.Description
            }
        };
    }

    private void LoadExchanges(List<ExchangeDTO> exchangeDtos, EntityCounts counts = null)
    {
        if (exchangeDtos == null) return;

        // Store DTOs for reference
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            _gameWorld.ExchangeDefinitions.Add(dto);
            Console.WriteLine($"[PackageLoader] Loaded exchange definition: {dto.Id} ({dto.GiveAmount} {dto.GiveCurrency} for {dto.ReceiveAmount} {dto.ReceiveCurrency})");
            if (counts != null) counts.Exchanges++;
        }

        // Parse exchanges into ExchangeCard objects and store them
        // These will be referenced when building NPC decks
        _parsedExchangeCards = new List<ExchangeCardEntry>();
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            ExchangeCard exchangeCard = ExchangeParser.ParseExchange(dto);
            _parsedExchangeCards.Add(new ExchangeCardEntry { Id = exchangeCard.Id, Card = exchangeCard });
            Console.WriteLine($"[PackageLoader] Created ExchangeCard: {exchangeCard.Id} - {exchangeCard.GetExchangeRatio()}");
        }
    }

    /// <summary>
    /// Initialize the travel discovery system state after all content is loaded
    /// </summary>
    private void InitializeTravelDiscoverySystem()
    {
        Console.WriteLine("[PackageLoader] Initializing travel discovery system...");

        // Initialize PathCardDiscoveries from cards embedded in collections
        // First from path collections
        foreach (var collection in _gameWorld.AllPathCollections.Values)
        {
            foreach (var pathCard in collection.PathCards)
            {
                _gameWorld.PathCardDiscoveries[pathCard.Id] = pathCard.StartsRevealed;
                Console.WriteLine($"[PackageLoader] Path card '{pathCard.Id}' discovery state: {(pathCard.StartsRevealed ? "face-up" : "face-down")}");
            }
        }
        
        // Also initialize discovery states for event cards in event collections
        foreach (var collection in _gameWorld.AllEventCollections.Values)
        {
            foreach (var eventCard in collection.EventCards)
            {
                _gameWorld.PathCardDiscoveries[eventCard.Id] = eventCard.StartsRevealed;
                Console.WriteLine($"[PackageLoader] Event card '{eventCard.Id}' discovery state: {(eventCard.StartsRevealed ? "face-up" : "face-down")}");
            }
        }
        
        // Cards are now embedded directly in collections
        // No separate card dictionaries needed

        // Initialize EventDeckPositions for routes with event pools
        foreach (KeyValuePair<string, PathCardCollectionDTO> kvp in _gameWorld.AllEventCollections)
        {
            string routeId = kvp.Key;
            string deckKey = $"route_{routeId}_events";
            
            // Start at position 0 for deterministic event drawing
            _gameWorld.EventDeckPositions[deckKey] = 0;
            
            int eventCount = kvp.Value.Events?.Count ?? 0;
            Console.WriteLine($"[PackageLoader] Initialized event deck position for route '{routeId}' with {eventCount} events");
        }

        // Initialize route discovery states
        // Routes can start discovered based on IsDiscovered property
        foreach (RouteOption route in _gameWorld.WorldState.Routes)
        {
            if (route.IsDiscovered)
            {
                Console.WriteLine($"[PackageLoader] Route '{route.Id}' starts discovered");
            }
            else
            {
                Console.WriteLine($"[PackageLoader] Route '{route.Id}' starts hidden, needs discovery");
            }
        }

        Console.WriteLine($"[PackageLoader] Travel discovery system initialized: {_gameWorld.PathCardDiscoveries.Count} path cards, {_gameWorld.EventDeckPositions.Count} event decks");
    }

    /// <summary>
    /// Validates that crossroads configuration is correct:
    /// 1. Each location has exactly one spot with Crossroads property
    /// 2. All route origin and destination spots have Crossroads property
    /// </summary>
    /// <summary>
    /// BIDIRECTIONAL ROUTE GENERATION: Automatically creates the reverse route from a forward route.
    /// This ensures travel is always bidirectional and segments are properly reversed.
    /// For example, a route A->B->C with segments [1,2,3] becomes C->B->A with segments [3,2,1].
    /// </summary>
    private RouteOption GenerateReverseRoute(RouteOption forwardRoute)
    {
        // Extract location IDs from the spot IDs for naming
        string originLocationId = GetLocationIdFromSpotId(forwardRoute.OriginLocationSpot);
        string destLocationId = GetLocationIdFromSpotId(forwardRoute.DestinationLocationSpot);
        
        // Generate reverse route ID by swapping origin and destination
        string[] idParts = forwardRoute.Id.Split("_to_");
        string reverseId = idParts.Length == 2 
            ? $"{idParts[1]}_to_{idParts[0]}"
            : $"{destLocationId}_to_{originLocationId}";
        
        RouteOption reverseRoute = new RouteOption
        {
            Id = reverseId,
            Name = $"Return to {GetLocationNameFromId(originLocationId)}",
            // Swap origin and destination
            OriginLocationSpot = forwardRoute.DestinationLocationSpot,
            DestinationLocationSpot = forwardRoute.OriginLocationSpot,
            
            // Keep the same properties for both directions
            Method = forwardRoute.Method,
            BaseCoinCost = forwardRoute.BaseCoinCost,
            BaseStaminaCost = forwardRoute.BaseStaminaCost,
            TravelTimeSegments = forwardRoute.TravelTimeSegments,
            DepartureTime = forwardRoute.DepartureTime,
            IsDiscovered = forwardRoute.IsDiscovered,
            MaxItemCapacity = forwardRoute.MaxItemCapacity,
            Description = $"Return journey from {GetLocationNameFromId(destLocationId)} to {GetLocationNameFromId(originLocationId)}",
            AccessRequirement = forwardRoute.AccessRequirement,
            RouteType = forwardRoute.RouteType,
            HasPermitUnlock = forwardRoute.HasPermitUnlock,
            StartingStamina = forwardRoute.StartingStamina
        };
        
        // Copy terrain categories
        reverseRoute.TerrainCategories.AddRange(forwardRoute.TerrainCategories);
        
        // Copy weather modifications
        foreach (var kvp in forwardRoute.WeatherModifications)
        {
            reverseRoute.WeatherModifications[kvp.Key] = kvp.Value;
        }
        
        // Copy unlock condition if present
        if (forwardRoute.UnlockCondition != null)
        {
            reverseRoute.UnlockCondition = forwardRoute.UnlockCondition;
        }
        
        // CRITICAL: Reverse the segments order for the return journey
        // This ensures the path is traversed in reverse (C->B->A instead of A->B->C)
        var reversedSegments = forwardRoute.Segments.OrderByDescending(s => s.SegmentNumber).ToList();
        int segmentNumber = 1;
        foreach (var originalSegment in reversedSegments)
        {
            RouteSegment reverseSegment = new RouteSegment
            {
                SegmentNumber = segmentNumber++,
                Type = originalSegment.Type,
                // Keep the same collections - they represent the same physical locations
                PathCollectionId = originalSegment.PathCollectionId,
                EventCollectionId = originalSegment.EventCollectionId
            };
            reverseRoute.Segments.Add(reverseSegment);
        }
        
        // Copy encounter deck IDs
        reverseRoute.EncounterDeckIds.AddRange(forwardRoute.EncounterDeckIds);
        
        // If the forward route has a route-level event pool, copy it to the reverse route
        if (_gameWorld.AllEventCollections.ContainsKey(forwardRoute.Id))
        {
            _gameWorld.AllEventCollections[reverseRoute.Id] = _gameWorld.AllEventCollections[forwardRoute.Id];
        }
        
        return reverseRoute;
    }
    
    private string GetLocationNameFromId(string locationId)
    {
        // Helper to get friendly location name from ID for route naming
        if (string.IsNullOrEmpty(locationId))
        {
            return "Unknown Location";
        }

        var location = _gameWorld.WorldState.locations?.FirstOrDefault(l => l.Id == locationId);
        return location?.Name ?? locationId.Replace("_", " ").Replace("-", " ");
    }

    private void ValidateCrossroadsConfiguration()
    {
        Console.WriteLine("[PackageLoader] Starting crossroads configuration validation...");
        
        // Group spots by location using tuples
        var spotsByLocation = new List<(string LocationId, List<LocationSpot> Spots)>();
        foreach (LocationSpot spot in _gameWorld.WorldState.locationSpots)
        {
            var groupIndex = spotsByLocation.FindIndex(g => g.LocationId == spot.LocationId);
            if (groupIndex == -1)
            {
                var spots = new List<LocationSpot>();
                spots.Add(spot);
                spotsByLocation.Add((spot.LocationId, spots));
            }
            else
            {
                spotsByLocation[groupIndex].Spots.Add(spot);
            }
        }

        // Validate each location has exactly one crossroads spot
        foreach (Location location in _gameWorld.WorldState.locations)
        {
            var locationGroup = spotsByLocation.FirstOrDefault(g => g.LocationId == location.Id);
            if (locationGroup.LocationId == null)
            {
                throw new InvalidOperationException($"Location '{location.Id}' ({location.Name}) has no spots defined");
            }

            List<LocationSpot> locationSpots = locationGroup.Spots;
            List<LocationSpot> crossroadsSpots = locationSpots
                .Where(s => s.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true)
                .ToList();

            if (crossroadsSpots.Count == 0)
            {
                throw new InvalidOperationException($"Location '{location.Id}' ({location.Name}) has no spots with Crossroads property. Every location must have exactly one crossroads spot for travel.");
            }
            else if (crossroadsSpots.Count > 1)
            {
                string spotsInfo = string.Join(", ", crossroadsSpots.Select(s => $"'{s.SpotID}' ({s.Name})"));
                throw new InvalidOperationException($"Location '{location.Id}' ({location.Name}) has {crossroadsSpots.Count} spots with Crossroads property: {spotsInfo}. Only one crossroads spot is allowed per location.");
            }

            Console.WriteLine($"[PackageLoader] ✓ Location '{location.Id}' has valid crossroads spot: '{crossroadsSpots[0].SpotID}'");
        }

        // Validate all route spots have crossroads property
        List<string> routeSpotIds = new List<string>();
        foreach (RouteOption route in _gameWorld.WorldState.Routes)
        {
            if (!routeSpotIds.Contains(route.OriginLocationSpot))
                routeSpotIds.Add(route.OriginLocationSpot);
            if (!routeSpotIds.Contains(route.DestinationLocationSpot))
                routeSpotIds.Add(route.DestinationLocationSpot);
        }

        foreach (string spotId in routeSpotIds)
        {
            LocationSpot spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
            if (spot == null)
            {
                throw new InvalidOperationException($"Route references spot '{spotId}' which does not exist");
            }

            if (!spot.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true)
            {
                throw new InvalidOperationException($"Route spot '{spotId}' ({spot.Name}) does not have Crossroads property. All route origin and destination spots must be crossroads.");
            }

            Console.WriteLine($"[PackageLoader] ✓ Route spot '{spotId}' has valid Crossroads property");
        }

        Console.WriteLine($"[PackageLoader] Crossroads validation completed successfully. Validated {_gameWorld.WorldState.locations.Count} locations and {routeSpotIds.Count} route spots.");
    }

    /// <summary>
    /// Initialize player's starter conversation deck
    /// </summary>
    private void InitializePlayerStarterDeck(DeckCompositionDTO deckCompositions)
    {
        Player player = _gameWorld.GetPlayer();
        if (player.ConversationDeck == null)
        {
            player.ConversationDeck = new PlayerCardDeck();
        }

        // Use the "default" deck from NpcDecks as the player's starter deck
        if (deckCompositions?.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey("default"))
        {
            var defaultDeck = deckCompositions.NpcDecks["default"];
            if (defaultDeck?.ConversationDeck != null)
            {
                foreach (KeyValuePair<string, int> kvp in defaultDeck.ConversationDeck)
            {
                string cardId = kvp.Key;
                int count = kvp.Value;

                if (_gameWorld.AllCardDefinitions.ContainsKey(cardId))
                {
                    ConversationCard cardTemplate = _gameWorld.AllCardDefinitions[cardId] as ConversationCard;
                    // Add starter cards to player deck as CardInstances (to track XP)
                    if (cardTemplate != null && IsStarterCard(cardTemplate))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            // Create a new CardInstance for each copy of the card
                            CardInstance instance = new CardInstance(cardTemplate, "player_deck");
                            player.ConversationDeck.AddCardInstance(instance);
                        }
                    }
                }
            }

                Console.WriteLine($"[PackageLoader] Initialized player's starter deck with {player.ConversationDeck.Count} cards");
            }
        }
        else
        {
            Console.WriteLine("[PackageLoader] Warning: No default deck defined for player starter cards");
        }
    }

    /// <summary>
    /// Check if a card should be in the player's starter deck
    /// </summary>
    private bool IsStarterCard(ConversationCard card)
    {
        // Exclude special card types that are never starter cards
        if (card.CardType == CardType.Letter ||
            card.CardType == CardType.Promise ||
            card.CardType == CardType.BurdenGoal ||
            card.CardType == CardType.Observation)
        {
            return false;
        }

        // All basic conversation cards from default deck are starter cards
        // NPC-specific cards will have MinimumTokensRequired > 0
        return true;
    }
}