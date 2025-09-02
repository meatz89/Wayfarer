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
        List<string> packageFiles = Directory.GetFiles(directoryPath, "*.json")
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
            // Phase 1: Cards (foundation - NPCs reference these)
            LoadCards(package.Content.Cards);

            // Phase 2: Locations (NPCs and spots reference these)
            LoadLocations(package.Content.Locations);

            // Phase 3: Location spots (depend on locations)
            LoadLocationSpots(package.Content.Spots);

            // Phase 4: NPCs (depend on locations and cards)
            LoadNPCs(package.Content.Npcs);

            // Phase 5: Routes (depend on locations)
            LoadRoutes(package.Content.Routes);

            // Phase 6: Independent content
            LoadObservations(package.Content.Observations);
            LoadTravelCards(package.Content.TravelCards);
            LoadItems(package.Content.Items);
            LoadLetterTemplates(package.Content.LetterTemplates);
            LoadStandingObligations(package.Content.StandingObligations);
        }
    }

    private void ApplyStartingConditions(PackageStartingConditions conditions)
    {
        // Apply player initial config
        if (conditions.PlayerConfig != null)
        {
            _gameWorld.InitialPlayerConfig = conditions.PlayerConfig;
            
            // Apply immediate values to player
            if (conditions.PlayerConfig.Coins.HasValue)
                _gameWorld.PlayerCoins = conditions.PlayerConfig.Coins.Value;
            if (conditions.PlayerConfig.StaminaPoints.HasValue)
                _gameWorld.PlayerStamina = conditions.PlayerConfig.StaminaPoints.Value;
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
                
                // Also remove from parent location's spots
                var parentLoc = _gameWorld.Locations.FirstOrDefault(l => l.Id == existingSkeleton.LocationId);
                parentLoc?.AvailableSpots.Remove(existingSkeleton);
            }
            
            var spot = LocationSpotParser.ConvertDTOToLocationSpot(dto);
            _gameWorld.WorldState.locationSpots.Add(spot);
            
            // Also add to parent location's spots list
            var parentLocation = _gameWorld.Locations.FirstOrDefault(l => l.Id == spot.LocationId);
            if (parentLocation != null)
            {
                parentLocation.AvailableSpots.Add(spot);
            }
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
                var hubSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    skeletonLocation.TravelHubSpotId,
                    dto.LocationId,
                    $"location_{dto.LocationId}_hub");
                    
                _gameWorld.WorldState.locationSpots.Add(hubSpot);
                skeletonLocation.AvailableSpots.Add(hubSpot);
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
        if (routeDtos == null) return;

        foreach (var dto in routeDtos)
        {
            var route = ConvertRouteDTOToModel(dto);
            _gameWorld.WorldState.Routes.Add(route);
        }
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

    // Conversion methods that don't have dedicated parsers yet

    private RouteOption ConvertRouteDTOToModel(RouteDTO dto)
    {
        return new RouteOption
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
            Description = dto.Description ?? ""
        };
    }

    private ConversationCard ConvertObservationDTOToCard(ObservationDTO dto)
    {
        // Observations become player cards
        return new ConversationCard
        {
            Id = dto.Id,
            DisplayName = dto.DisplayText,
            Category = dto.Category ?? "Observation",
            Type = CardType.Observation,
            Weight = dto.Weight,
            Persistence = PersistenceType.Persistent,
            Mechanics = CardMechanicsType.Standard
        };
    }

    private ConversationCard ConvertTravelCardDTOToModel(TravelCardDTO dto)
    {
        return new ConversationCard
        {
            Id = dto.Id,
            DisplayName = dto.Title ?? dto.DisplayName ?? "Travel Card",
            Category = dto.Category ?? "Travel",
            Type = CardType.Normal,
            Weight = dto.Weight ?? 1,
            BaseComfort = dto.BaseComfort ?? 0,
            Persistence = Enum.TryParse<PersistenceType>(dto.Persistence, out var persistence) ? persistence : PersistenceType.Persistent
        };
    }
}