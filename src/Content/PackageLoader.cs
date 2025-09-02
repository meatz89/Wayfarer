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
                var obligation = ConvertStandingObligationDTOToModel(obligationDto);
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

        // Use existing ConversationCardParser for conversion
        var parser = new ConversationCardParser("content");
        
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
            
            // Note: Conversation deck mappings would be handled elsewhere if needed
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
            _gameWorld.AllCardDefinitions[travelCard.Id] = travelCard;
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
            Tier = dto.Tier,
            TravelHubSpotId = dto.TravelHubSpotId,
            DomainTags = dto.DomainTags ?? new List<string>(),
            LocationType = Enum.TryParse<LocationTypes>(dto.LocationType ?? "Connective", out var locationType) ? locationType : LocationTypes.Connective,
            LocationTypeString = dto.LocationType,
            IsStartingLocation = dto.IsStartingLocation
        };

        // Note: AvailableServices not available in LocationDTO

        return location;
    }

    private LocationSpot ConvertLocationSpotDTOToModel(LocationSpotDTO dto)
    {
        var spot = new LocationSpot(dto.Id, dto.Name)
        {
            LocationId = dto.LocationId,
            DomainTags = dto.DomainTags ?? new List<string>()
        };

        if (dto.SpotProperties != null)
        {
            foreach (var property in dto.SpotProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(property, out var propertyType))
                {
                    spot.SpotProperties.Add(propertyType);
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
            PersonalityDescription = dto.Personality ?? "",
            PersonalityType = Enum.TryParse<PersonalityType>(dto.PersonalityType, out var personalityType) ? personalityType : PersonalityType.STEADFAST,
            Description = dto.Description ?? "",
            Location = dto.LocationId ?? "",
            SpotId = dto.SpotId ?? "",
            Role = dto.Role ?? "",
            Tier = dto.Tier
        };

        if (!string.IsNullOrEmpty(dto.Profession))
        {
            if (Enum.TryParse<Professions>(dto.Profession, out var profession))
            {
                npc.Profession = profession;
            }
        }

        if (dto.Services != null)
        {
            foreach (var service in dto.Services)
            {
                if (Enum.TryParse<ServiceTypes>(service, out var serviceType))
                {
                    npc.ProvidedServices.Add(serviceType);
                }
            }
        }

        return npc;
    }

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

    private Item ConvertItemDTOToModel(ItemDTO dto)
    {
        var item = new Item
        {
            Id = dto.Id,
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            Weight = dto.Weight,
            BuyPrice = dto.BuyPrice,
            SellPrice = dto.SellPrice,
            InventorySlots = dto.InventorySlots
        };

        // Parse size category
        if (!string.IsNullOrEmpty(dto.SizeCategory))
        {
            if (Enum.TryParse<SizeCategory>(dto.SizeCategory, out var size))
            {
                item.Size = size;
            }
        }

        // Parse item categories
        if (dto.Categories != null)
        {
            foreach (var category in dto.Categories)
            {
                if (Enum.TryParse<ItemCategory>(category, out var itemCategory))
                {
                    item.Categories.Add(itemCategory);
                }
            }
        }

        // Parse token generation modifiers
        if (dto.TokenGenerationModifiers != null)
        {
            foreach (var kvp in dto.TokenGenerationModifiers)
            {
                if (Enum.TryParse<ConnectionType>(kvp.Key, out var connectionType))
                {
                    item.TokenGenerationModifiers[connectionType] = kvp.Value;
                }
            }
        }

        // Parse enabled token generation
        if (dto.EnablesTokenGeneration != null)
        {
            foreach (var tokenType in dto.EnablesTokenGeneration)
            {
                if (Enum.TryParse<ConnectionType>(tokenType, out var connectionType))
                {
                    item.EnablesTokenGeneration.Add(connectionType);
                }
            }
        }

        return item;
    }

    private LetterTemplate ConvertLetterTemplateDTOToModel(LetterTemplateDTO dto)
    {
        var template = new LetterTemplate
        {
            Id = dto.Id ?? "",
            Description = dto.Description ?? "",
            MinDeadlineInMinutes = dto.MinDeadlineInMinutes,
            MaxDeadlineInMinutes = dto.MaxDeadlineInMinutes,
            MinPayment = dto.MinPayment,
            MaxPayment = dto.MaxPayment,
            MinTokensRequired = dto.MinTokensRequired ?? 1
        };

        // Parse token type
        if (!string.IsNullOrEmpty(dto.TokenType))
        {
            if (Enum.TryParse<ConnectionType>(dto.TokenType, out var tokenType))
            {
                template.TokenType = tokenType;
            }
        }

        // Parse category
        if (!string.IsNullOrEmpty(dto.Category))
        {
            if (Enum.TryParse<LetterCategory>(dto.Category, out var category))
            {
                template.Category = category;
            }
        }

        // Parse tier level
        if (!string.IsNullOrEmpty(dto.TierLevel))
        {
            if (Enum.TryParse<TierLevel>(dto.TierLevel, out var tier))
            {
                template.TierLevel = tier;
            }
        }

        // Parse special type
        if (!string.IsNullOrEmpty(dto.SpecialType))
        {
            if (Enum.TryParse<LetterSpecialType>(dto.SpecialType, out var specialType))
            {
                template.SpecialType = specialType;
            }
        }

        template.SpecialTargetId = dto.SpecialTargetId ?? "";

        // Parse size
        if (!string.IsNullOrEmpty(dto.Size))
        {
            if (Enum.TryParse<SizeCategory>(dto.Size, out var size))
            {
                template.Size = size;
            }
        }

        return template;
    }

    private StandingObligation ConvertStandingObligationDTOToModel(StandingObligationDTO dto)
    {
        var obligation = new StandingObligation
        {
            ID = dto.ID ?? "",
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            Source = dto.Source ?? "",
            RelatedNPCId = dto.RelatedNPCId,
            ActivationThreshold = dto.ActivationThreshold,
            DeactivationThreshold = dto.DeactivationThreshold,
            IsThresholdBased = dto.IsThresholdBased,
            ActivatesAboveThreshold = dto.ActivatesAboveThreshold,
            ScalingFactor = dto.ScalingFactor,
            BaseValue = dto.BaseValue,
            MinValue = dto.MinValue,
            MaxValue = dto.MaxValue,
            SteppedThresholds = dto.SteppedThresholds ?? new Dictionary<int, float>()
        };

        // Parse related token type
        if (!string.IsNullOrEmpty(dto.RelatedTokenType))
        {
            if (Enum.TryParse<ConnectionType>(dto.RelatedTokenType, out var tokenType))
            {
                obligation.RelatedTokenType = tokenType;
            }
        }

        // Parse scaling type
        if (!string.IsNullOrEmpty(dto.ScalingType))
        {
            if (Enum.TryParse<ScalingType>(dto.ScalingType, out var scalingType))
            {
                obligation.ScalingType = scalingType;
            }
        }

        // Parse benefit effects
        if (dto.BenefitEffects != null)
        {
            foreach (var effect in dto.BenefitEffects)
            {
                if (Enum.TryParse<ObligationEffect>(effect, out var obligationEffect))
                {
                    obligation.BenefitEffects.Add(obligationEffect);
                }
            }
        }

        // Parse constraint effects
        if (dto.ConstraintEffects != null)
        {
            foreach (var effect in dto.ConstraintEffects)
            {
                if (Enum.TryParse<ObligationEffect>(effect, out var obligationEffect))
                {
                    obligation.ConstraintEffects.Add(obligationEffect);
                }
            }
        }

        return obligation;
    }
}