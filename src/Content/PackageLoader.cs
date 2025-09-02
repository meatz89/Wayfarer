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
                // Convert DTO to domain model and add to player's queue
                var obligation = ConvertStandingObligationDTOToModel(obligationDto);
                _gameWorld.GetPlayer().ObligationQueue.AddObligation(obligation);
            }
        }

        // Apply starting token relationships
        if (conditions.StartingTokens != null)
        {
            foreach (var kvp in conditions.StartingTokens)
            {
                // Token relationships will be applied when NPCs are loaded
                // Store for later application
                _gameWorld.GetPlayer().InitialTokenRelationships[kvp.Key] = new Dictionary<ConnectionType, int>
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

        // Use existing ConversationCardParser for conversion
        var parser = new ConversationCardParser(_gameWorld.ContentPath);
        
        foreach (var dto in cardDtos)
        {
            var card = parser.ConvertDTOToCard(dto);
            _gameWorld.AllCardDefinitions[card.Id] = card;
        }
    }

    private void LoadLocations(List<LocationDTO> locationDtos)
    {
        if (locationDtos == null) return;

        foreach (var dto in locationDtos)
        {
            var location = ConvertLocationDTOToModel(dto);
            _gameWorld.Locations.Add(location);
            _gameWorld.WorldState.locations.Add(location);
        }
    }

    private void LoadLocationSpots(List<LocationSpotDTO> spotDtos)
    {
        if (spotDtos == null) return;

        foreach (var dto in spotDtos)
        {
            var spot = ConvertLocationSpotDTOToModel(dto);
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
            var npc = ConvertNPCDTOToModel(dto);
            _gameWorld.NPCs.Add(npc);
            _gameWorld.WorldState.NPCs.Add(npc);
            
            // Map conversation deck if specified
            if (dto.ConversationDeckCardIds != null && dto.ConversationDeckCardIds.Any())
            {
                _gameWorld.NPCConversationDeckMappings[npc.ID] = dto.ConversationDeckCardIds;
            }
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
        }
    }

    private void LoadItems(List<ItemDTO> itemDtos)
    {
        if (itemDtos == null) return;

        foreach (var dto in itemDtos)
        {
            var item = ConvertItemDTOToModel(dto);
            _gameWorld.WorldState.Items.Add(item);
        }
    }

    private void LoadLetterTemplates(List<LetterTemplateDTO> letterDtos)
    {
        if (letterDtos == null) return;

        foreach (var dto in letterDtos)
        {
            var letter = ConvertLetterTemplateDTOToModel(dto);
            _gameWorld.WorldState.LetterTemplates.Add(letter);
        }
    }

    private void LoadStandingObligations(List<StandingObligationDTO> obligationDtos)
    {
        if (obligationDtos == null) return;

        foreach (var dto in obligationDtos)
        {
            var obligation = ConvertStandingObligationDTOToModel(dto);
            _gameWorld.WorldState.StandingObligationTemplates.Add(obligation);
        }
    }

    // Conversion methods - these will be implemented by delegating to parsers or simple mapping

    private Location ConvertLocationDTOToModel(LocationDTO dto)
    {
        var location = new Location(dto.Id, dto.Name)
        {
            Description = dto.Description,
            Tier = dto.Tier ?? 1,
            TravelHubSpotId = dto.TravelHubSpotId,
            DomainTags = dto.DomainTags ?? new List<string>(),
            LocationType = Enum.Parse<LocationTypes>(dto.LocationType ?? "Connective"),
            LocationTypeString = dto.LocationTypeString,
            IsStartingLocation = dto.IsStartingLocation ?? false
        };

        if (dto.AvailableServices != null)
        {
            foreach (var service in dto.AvailableServices)
            {
                if (Enum.TryParse<ServiceTypes>(service, out var serviceType))
                {
                    location.AvailableServices.Add(serviceType);
                }
            }
        }

        return location;
    }

    private LocationSpot ConvertLocationSpotDTOToModel(LocationSpotDTO dto)
    {
        var spot = new LocationSpot(dto.Id, dto.Name)
        {
            LocationId = dto.LocationId,
            Tier = dto.Tier ?? 1,
            ComfortModifier = dto.ComfortModifier ?? 0,
            DomainTags = dto.DomainTags ?? new List<string>(),
            PlayerKnowledge = dto.PlayerKnowledge ?? false
        };

        if (dto.SpotTraits != null)
        {
            foreach (var trait in dto.SpotTraits)
            {
                if (Enum.TryParse<SpotPropertyType>(trait, out var property))
                {
                    spot.SpotProperties.Add(property);
                }
            }
        }

        return spot;
    }

    private NPC ConvertNPCDTOToModel(NPCDTO dto)
    {
        var npc = new NPC
        {
            ID = dto.Id,
            Name = dto.Name,
            PersonalityType = Enum.Parse<PersonalityType>(dto.PersonalityType),
            BasePatience = dto.BasePatience ?? 10,
            CurrentEmotionalState = Enum.Parse<EmotionalState>(dto.InitialEmotionalState ?? "Neutral"),
            HomeLocationId = dto.HomeLocationId,
            CurrentLocationId = dto.CurrentLocationId ?? dto.HomeLocationId,
            HasDeliveryForPlayer = dto.HasUrgentDelivery ?? false
        };

        if (dto.ProfessionType != null)
        {
            npc.Profession = Enum.Parse<Professions>(dto.ProfessionType);
        }

        return npc;
    }

    private RouteOption ConvertRouteDTOToModel(RouteDTO dto)
    {
        return new RouteOption
        {
            Id = dto.Id,
            FromSpotId = dto.FromSpotId,
            ToSpotId = dto.ToSpotId,
            TravelTimeMinutes = dto.TravelTimeMinutes ?? 30,
            StaminaCost = dto.StaminaCost ?? 1,
            Description = dto.Description,
            Requirements = dto.Requirements ?? new List<string>()
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
            Type = CardType.Single,
            Weight = dto.Weight,
            Persistence = PersistenceType.Persistent,
            Mechanics = CardMechanicsType.Standard
        };
    }

    private TravelCard ConvertTravelCardDTOToModel(TravelCardDTO dto)
    {
        return new TravelCard
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Weight = dto.Weight ?? 1,
            Requirements = dto.Requirements ?? new List<string>()
        };
    }

    private Item ConvertItemDTOToModel(ItemDTO dto)
    {
        return new Item
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Value = dto.Value ?? 0,
            Weight = dto.Weight ?? 0,
            Properties = dto.Properties ?? new Dictionary<string, object>()
        };
    }

    private LetterTemplate ConvertLetterTemplateDTOToModel(LetterTemplateDTO dto)
    {
        return new LetterTemplate
        {
            Id = dto.Id,
            FromNpcId = dto.FromNpcId,
            ToNpcId = dto.ToNpcId,
            Content = dto.Content,
            Urgency = dto.Urgency ?? "Normal",
            Payment = dto.Payment ?? 0,
            Weight = dto.Weight ?? 1
        };
    }

    private StandingObligation ConvertStandingObligationDTOToModel(StandingObligationDTO dto)
    {
        return new StandingObligation
        {
            Id = dto.Id,
            Type = dto.Type,
            FromNpcId = dto.FromNpcId,
            ToNpcId = dto.ToNpcId,
            DeadlineDay = dto.DeadlineDay ?? 3,
            Payment = dto.Payment ?? 0,
            PenaltyType = dto.PenaltyType,
            Description = dto.Description
        };
    }
}