using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Orchestrates loading of game packages, delegating to specialized parsers for conversion
/// </summary>
public class PackageLoader
{
    private readonly GameWorld _gameWorld;
    private bool _isFirstPackage = true;

    public PackageLoader(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Load a single package from a JSON file
    /// </summary>
    public void LoadPackage(string packageFilePath)
    {
        string json = File.ReadAllText(packageFilePath);
        Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        LoadPackageContent(package);
    }

    /// <summary>
    /// Load multiple packages in sequence
    /// </summary>
    public void LoadPackages(List<string> packageFilePaths)
    {
        foreach (string path in packageFilePaths)
        {
            LoadPackage(path);
        }
    }

    /// <summary>
    /// Load all packages from a directory with proper ordering
    /// </summary>
    public void LoadPackagesFromDirectory(string directoryPath)
    {
        List<string> packageFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories)
            .OrderBy(f => 
            {
                // Core packages first (priority 0)
                if (f.Contains("core", StringComparison.OrdinalIgnoreCase)) return 0;
                // Base content next (priority 1)
                if (f.Contains("base", StringComparison.OrdinalIgnoreCase)) return 1;
                // Expansions (priority 2)
                if (f.Contains("expansion", StringComparison.OrdinalIgnoreCase)) return 2;
                // Generated content last (priority 3)
                if (f.Contains("generated", StringComparison.OrdinalIgnoreCase)) return 3;
                // Everything else in between
                return 2;
            })
            .ThenBy(f => f) // Then alphabetical
            .ToList();

        LoadPackages(packageFiles);
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

            // Phase 4.5: Initialize all NPC decks (after NPCs and cards are loaded)
            InitializeNPCConversationDecks();
            InitializeNPCRequestDecks(package.Content.NpcPromiseCards);
            InitializeNPCExchangeDecks();

            // Phase 5: Routes (depend on locations)
            LoadRoutes(package.Content.Routes);

            // Phase 6: Independent content
            LoadObservations(package.Content.Observations);
            LoadTravelCards(package.Content.TravelCards);
            LoadItems(package.Content.Items);
            LoadLetterTemplates(package.Content.LetterTemplates);
            LoadStandingObligations(package.Content.StandingObligations);
            LoadLocationActions(package.Content.LocationActions);
        }
    }

    private void ApplyStartingConditions(PackageStartingConditions conditions)
    {
        // Apply player initial config
        if (conditions.PlayerConfig != null)
        {
            _gameWorld.InitialPlayerConfig = conditions.PlayerConfig;
            
            // Player config is stored for GameFacade to apply to Player object
            // We don't set duplicate fields on GameWorld anymore
        }

        // Set starting location
        if (!string.IsNullOrEmpty(conditions.StartingSpotId))
        {
            _gameWorld.InitialLocationSpotId = conditions.StartingSpotId;
        }

        // Apply starting obligations
        if (conditions.StartingObligations != null)
        {
            foreach (var obligationDto in conditions.StartingObligations)
            {
                // Convert DTO to domain model and add to player's standing obligations
                var obligation = StandingObligationParser.ConvertDTOToStandingObligation(obligationDto);
                _gameWorld.GetPlayer().StandingObligations.Add(obligation);
            }
        }

        // Apply starting token relationships
        if (conditions.StartingTokens != null)
        {
            foreach (var kvp in conditions.StartingTokens)
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

    private void LoadRegions(List<RegionDTO> regionDtos)
    {
        if (regionDtos == null) return;
        
        foreach (var dto in regionDtos)
        {
            var region = new Region
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
        }
    }

    private void LoadDistricts(List<DistrictDTO> districtDtos)
    {
        if (districtDtos == null) return;
        
        foreach (var dto in districtDtos)
        {
            var district = new District
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
        }
    }

    private void LoadCards(List<ConversationCardDTO> cardDtos)
    {
        if (cardDtos == null) return;
        
        foreach (var dto in cardDtos)
        {
            // Use static method from ConversationCardParser
            var card = ConversationCardParser.ConvertDTOToCard(dto);
            _gameWorld.AllCardDefinitions[card.Id] = card;
        }
    }

    private void LoadLocations(List<LocationDTO> locationDtos)
    {
        if (locationDtos == null) return;

        foreach (var dto in locationDtos)
        {
            // Check if this location was previously a skeleton, if so replace it
            var existingSkeleton = _gameWorld.WorldState.locations
                .FirstOrDefault(l => l.Id == dto.Id && l.IsSkeleton);
                
            if (existingSkeleton != null)
            {
                _gameWorld.WorldState.locations.Remove(existingSkeleton);
                _gameWorld.Locations.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);
            }
            
            var location = LocationParser.ConvertDTOToLocation(dto);
            _gameWorld.Locations.Add(location);
            _gameWorld.WorldState.locations.Add(location);
        }
    }

    private void LoadLocationSpots(List<LocationSpotDTO> spotDtos)
    {
        if (spotDtos == null) return;

        foreach (var dto in spotDtos)
        {
            // Check if this spot was previously a skeleton, if so replace it
            var existingSkeleton = _gameWorld.WorldState.locationSpots
                .FirstOrDefault(s => s.SpotID == dto.Id && s.IsSkeleton);
                
            if (existingSkeleton != null)
            {
                _gameWorld.WorldState.locationSpots.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);
                
                // Remove from primary spots dictionary if exists
                _gameWorld.Spots.Remove(dto.Id);
            }
            
            var spot = LocationSpotParser.ConvertDTOToLocationSpot(dto);
            _gameWorld.WorldState.locationSpots.Add(spot);
            
            // Add to primary spots dictionary
            _gameWorld.Spots[spot.SpotID] = spot;
        }
    }

    private void LoadNPCs(List<NPCDTO> npcDtos)
    {
        if (npcDtos == null) return;

        foreach (var dto in npcDtos)
        {
            // Check if NPC references a location that doesn't exist
            if (!string.IsNullOrEmpty(dto.LocationId) && 
                !_gameWorld.WorldState.locations.Any(l => l.Id == dto.LocationId))
            {
                // Create skeleton location
                var skeletonLocation = SkeletonGenerator.GenerateSkeletonLocation(
                    dto.LocationId, 
                    $"npc_{dto.Id}_reference");
                
                _gameWorld.WorldState.locations.Add(skeletonLocation);
                _gameWorld.Locations.Add(skeletonLocation);
                _gameWorld.SkeletonRegistry[dto.LocationId] = "Location";
                
                // Also create a skeleton spot for the location
                var hubSpotId = $"{dto.LocationId}_hub";
                var hubSpot = SkeletonGenerator.GenerateSkeletonSpot(
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
                var skeletonSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    dto.SpotId,
                    dto.LocationId ?? "unknown_location", 
                    $"npc_{dto.Id}_spot_reference");
                    
                _gameWorld.WorldState.locationSpots.Add(skeletonSpot);
                _gameWorld.SkeletonRegistry[dto.SpotId] = "LocationSpot";
            }
            
            // Check if this NPC was previously a skeleton, if so replace it
            var existingSkeleton = _gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
            if (existingSkeleton != null)
            {
                _gameWorld.NPCs.Remove(existingSkeleton);
                _gameWorld.WorldState.NPCs.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);
            }
            
            var npc = NPCParser.ConvertDTOToNPC(dto);
            _gameWorld.NPCs.Add(npc);
            _gameWorld.WorldState.NPCs.Add(npc);
        }
    }

    private void LoadRoutes(List<RouteDTO> routeDtos)
    {
        if (routeDtos == null) 
        {
            Console.WriteLine("[PackageLoader] No routes to load - routeDtos is null");
            return;
        }

        Console.WriteLine($"[PackageLoader] Loading {routeDtos.Count} routes...");
        
        foreach (var dto in routeDtos)
        {
            var route = ConvertRouteDTOToModel(dto);
            _gameWorld.WorldState.Routes.Add(route);
            Console.WriteLine($"[PackageLoader] Added route {route.Id}: {route.OriginLocationSpot} -> {route.DestinationLocationSpot}");
            
            // Also add route to location connections for RouteRepository compatibility
            AddRouteToLocationConnections(route);
        }
        
        Console.WriteLine($"[PackageLoader] Completed loading {routeDtos.Count} routes. Total routes in WorldState: {_gameWorld.WorldState.Routes.Count}");
    }
    
    private void AddRouteToLocationConnections(RouteOption route)
    {
        Console.WriteLine($"[PackageLoader] Adding route {route.Id} to location connections...");
        
        // Find the origin location for this route
        var originLocationId = GetLocationIdFromSpotId(route.OriginLocationSpot);
        var destinationLocationId = GetLocationIdFromSpotId(route.DestinationLocationSpot);
        
        Console.WriteLine($"[PackageLoader] Route {route.Id}: Origin spot '{route.OriginLocationSpot}' -> Location '{originLocationId}', Destination spot '{route.DestinationLocationSpot}' -> Location '{destinationLocationId}'");
        
        if (originLocationId == null || destinationLocationId == null)
        {
            Console.WriteLine($"[PackageLoader] Warning: Could not find location IDs for route {route.Id}. Origin: {route.OriginLocationSpot}, Destination: {route.DestinationLocationSpot}");
            return;
        }
        
        // Find the origin location
        var originLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == originLocationId);
        if (originLocation == null)
        {
            Console.WriteLine($"[PackageLoader] Warning: Could not find origin location {originLocationId} for route {route.Id}");
            return;
        }
        
        // Find or create a connection to the destination location
        var connection = originLocation.Connections.FirstOrDefault(c => c.DestinationLocationId == destinationLocationId);
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
        var spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
        return spot?.LocationId;
    }

    private void LoadObservations(List<ObservationDTO> observationDtos)
    {
        if (observationDtos == null) return;

        foreach (var dto in observationDtos)
        {
            var observation = ConvertObservationDTOToCard(dto);
            _gameWorld.PlayerObservationCards.Add(observation);
        }
    }

    private void LoadTravelCards(List<TravelCardDTO> travelCardDtos)
    {
        if (travelCardDtos == null) return;

        foreach (var dto in travelCardDtos)
        {
            var travelCard = ConvertTravelCardDTOToModel(dto);
            _gameWorld.TravelCards.Add(travelCard);
            _gameWorld.AllCardDefinitions[travelCard.Id] = travelCard;
        }
    }

    /// <summary>
    /// Initialize conversation decks for all NPCs with the universal starter deck
    /// </summary>
    private void InitializeNPCConversationDecks()
    {
        Console.WriteLine("[PackageLoader] Initializing NPC conversation decks with universal starter deck...");

        // Get all cards marked for ALL personalities (the universal starter deck)
        // EXCLUDE promise/request cards (DeliveryEligible) - those belong in RequestDeck only
        List<ConversationCard> starterDeckCards = _gameWorld.AllCardDefinitions.Values
            .OfType<ConversationCard>()
            .Where(card => card.PersonalityTypes != null && 
                          card.PersonalityTypes.Contains("ALL") &&
                          !card.Properties.Contains(CardProperty.DeliveryEligible)) // Exclude promise cards
            .ToList();

        Console.WriteLine($"[PackageLoader] Found {starterDeckCards.Count} universal starter deck cards");

        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                // Clear existing deck and populate with ALL starter deck cards
                npc.ConversationDeck = new CardDeck();
                foreach (ConversationCard card in starterDeckCards)
                {
                    npc.ConversationDeck.AddCard(card);
                }

                Console.WriteLine($"[PackageLoader] Initialized conversation deck for {npc.Name} with {starterDeckCards.Count} universal cards");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PackageLoader] Failed to initialize conversation deck for NPC {npc.Name}: {ex.Message}");
            }
        }

        Console.WriteLine("[PackageLoader] NPC conversation deck initialization completed");
    }

    /// <summary>
    /// Initialize request decks for NPCs from NpcPromiseCards
    /// </summary>
    private void InitializeNPCRequestDecks(List<NPCPromiseCardDTO> npcPromiseCardDtos)
    {
        Console.WriteLine("[PackageLoader] Initializing NPC request decks...");

        // Group promise cards by NPC ID for efficient loading
        Dictionary<string, List<ConversationCard>> npcPromiseCards = new Dictionary<string, List<ConversationCard>>();
        
        // Load promise cards from the new NpcPromiseCards section if it exists
        if (npcPromiseCardDtos != null)
        {
            foreach (NPCPromiseCardDTO promiseCardDto in npcPromiseCardDtos)
            {
                try
                {
                    // Convert DTO to RequestCard using ConversationCardParser
                    RequestCard requestCard = ConversationCardParser.ConvertPromiseCardDTO(promiseCardDto);
                    
                    // Add to NPC's collection
                    if (!npcPromiseCards.ContainsKey(promiseCardDto.NpcId))
                    {
                        npcPromiseCards[promiseCardDto.NpcId] = new List<ConversationCard>();
                    }
                    npcPromiseCards[promiseCardDto.NpcId].Add(requestCard);
                    
                    Console.WriteLine($"[PackageLoader] Loaded promise card '{promiseCardDto.Id}' for NPC '{promiseCardDto.NpcId}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PackageLoader] Failed to load promise card '{promiseCardDto.Id}': {ex.Message}");
                }
            }
        }

        // Initialize request decks for all NPCs
        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                List<ConversationCard> requestCards = new List<ConversationCard>();
                
                // Add NPC-specific promise cards if any exist
                if (npcPromiseCards.ContainsKey(npc.ID))
                {
                    requestCards.AddRange(npcPromiseCards[npc.ID]);
                    Console.WriteLine($"[PackageLoader] Added {npcPromiseCards[npc.ID].Count} promise cards to {npc.Name}'s request deck");
                }
                
                // Initialize request deck
                npc.InitializeRequestDeck(requestCards);

                Console.WriteLine($"[PackageLoader] Initialized request deck for {npc.Name} with {requestCards.Count} cards");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PackageLoader] Failed to initialize request deck for NPC {npc.Name}: {ex.Message}");
            }
        }

        Console.WriteLine("[PackageLoader] NPC request deck initialization completed");
    }


    /// <summary>
    /// Initialize exchange decks for Mercantile NPCs only
    /// </summary>
    private void InitializeNPCExchangeDecks()
    {
        Console.WriteLine("[PackageLoader] Initializing NPC exchange decks for Mercantile NPCs...");

        // Get all exchange cards from the card definitions
        List<ConversationCard> exchangeCards = _gameWorld.AllCardDefinitions.Values
            .OfType<ConversationCard>()
            .Where(card => card.Category == CardCategory.Exchange.ToString())
            .ToList();

        Console.WriteLine($"[PackageLoader] Found {exchangeCards.Count} exchange cards");

        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                // Only Mercantile NPCs get exchange cards
                List<ConversationCard> npcExchangeCards = new List<ConversationCard>();
                if (npc.PersonalityType == PersonalityType.MERCANTILE)
                {
                    npcExchangeCards = exchangeCards.ToList();
                }
                
                // Initialize exchange deck
                npc.InitializeExchangeDeck(npcExchangeCards);

                if (npcExchangeCards.Count > 0)
                {
                    Console.WriteLine($"[PackageLoader] Initialized exchange deck for {npc.Name} (MERCANTILE) with {npcExchangeCards.Count} cards");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PackageLoader] Failed to initialize exchange deck for NPC {npc.Name}: {ex.Message}");
            }
        }

        Console.WriteLine("[PackageLoader] NPC exchange deck initialization completed");
    }

    private void LoadItems(List<ItemDTO> itemDtos)
    {
        if (itemDtos == null) return;

        foreach (var dto in itemDtos)
        {
            var item = ItemParser.ConvertDTOToItem(dto);
            _gameWorld.WorldState.Items.Add(item);
        }
    }

    private void LoadLetterTemplates(List<LetterTemplateDTO> letterDtos)
    {
        if (letterDtos == null) return;

        foreach (var dto in letterDtos)
        {
            var letter = LetterTemplateParser.ConvertDTOToLetterTemplate(dto);
            _gameWorld.WorldState.LetterTemplates.Add(letter);
        }
    }

    private void LoadStandingObligations(List<StandingObligationDTO> obligationDtos)
    {
        if (obligationDtos == null) return;

        foreach (var dto in obligationDtos)
        {
            var obligation = StandingObligationParser.ConvertDTOToStandingObligation(dto);
            _gameWorld.WorldState.StandingObligationTemplates.Add(obligation);
        }
    }

    private void LoadLocationActions(List<LocationActionDTO> locationActionDtos)
    {
        if (locationActionDtos == null) return;

        foreach (var dto in locationActionDtos)
        {
            var locationAction = ConvertLocationActionDTOToModel(dto);
            _gameWorld.LocationActions.Add(locationAction);
        }
    }

    // Conversion methods that don't have dedicated parsers yet

    private RouteOption ConvertRouteDTOToModel(RouteDTO dto)
    {
        var route = new RouteOption
        {
            Id = dto.Id,
            Name = dto.Name ?? "",
            OriginLocationSpot = dto.OriginLocationSpot ?? "",
            DestinationLocationSpot = dto.DestinationLocationSpot ?? "",
            Method = Enum.TryParse<TravelMethods>(dto.Method, out var method) ? method : TravelMethods.Walking,
            BaseCoinCost = dto.BaseCoinCost,
            BaseStaminaCost = dto.BaseStaminaCost,
            TravelTimeMinutes = dto.TravelTimeMinutes,
            IsDiscovered = dto.IsDiscovered,
            Description = dto.Description ?? "",
            MaxItemCapacity = dto.MaxItemCapacity > 0 ? dto.MaxItemCapacity : 3
        };


        // Parse terrain categories
        if (dto.TerrainCategories != null)
        {
            foreach (var category in dto.TerrainCategories)
            {
                if (Enum.TryParse<TerrainCategory>(category, out var terrain))
                {
                    route.TerrainCategories.Add(terrain);
                }
            }
        }

        return route;
    }

    private ConversationCard ConvertObservationDTOToCard(ObservationDTO dto)
    {
        // Observations become player cards
        var card = new ConversationCard
        {
            Id = dto.Id,
            Description = dto.DisplayText ?? "",
            Focus = dto.Focus,
            TokenType = TokenType.Trust,
            Difficulty = Difficulty.Medium
        };
        card.Properties.Add(CardProperty.Observable);
        card.Properties.Add(CardProperty.Persistent);
        return card;
    }

    private ConversationCard ConvertTravelCardDTOToModel(TravelCardDTO dto)
    {
        var card = new ConversationCard
        {
            Id = dto.Id,
            Description = dto.Title ?? dto.DisplayName ?? "Travel Card",
            Focus = dto.Focus ?? 1,
            TokenType = TokenType.Trust,
            Difficulty = Difficulty.Medium
        };
        
        // Parse persistence
        if (dto.Persistence == "Impulse")
            card.Properties.Add(CardProperty.Impulse);
        else if (dto.Persistence == "Opening")
            card.Properties.Add(CardProperty.Opening);
        else
            card.Properties.Add(CardProperty.Persistent);
            
        return card;
    }

    private LocationAction ConvertLocationActionDTOToModel(LocationActionDTO dto)
    {
        var action = new LocationAction
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
            foreach (var prop in dto.RequiredProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(prop, true, out var propertyType))
                {
                    action.RequiredProperties.Add(propertyType);
                }
            }
        }

        // Parse optional properties
        if (dto.OptionalProperties != null)
        {
            foreach (var prop in dto.OptionalProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(prop, true, out var propertyType))
                {
                    action.OptionalProperties.Add(propertyType);
                }
            }
        }

        // Parse excluded properties
        if (dto.ExcludedProperties != null)
        {
            foreach (var prop in dto.ExcludedProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(prop, true, out var propertyType))
                {
                    action.ExcludedProperties.Add(propertyType);
                }
            }
        }

        return action;
    }
}