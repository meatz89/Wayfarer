using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Phase 3: Load entities that depend on NPCs and locations being loaded.
/// This includes: Routes, DeliveryObligation Templates
/// </summary>
public class Phase3_NPCDependents : IInitializationPhase
{
    public int PhaseNumber => 3;
    public string Name => "NPC-Dependent Entities";
    public bool IsCritical => false; // Game can run without routes and letters

    public void Execute(InitializationContext context)
    {
        // 1. Load Routes (depends on Locations)
        LoadRoutes(context);

        // 2. Load DeliveryObligation Templates (depends on NPCs for validation)
        LoadLetterTemplates(context);

        // 3. Initialize all NPC decks using new JSON-based system
        string contentPath = Path.Combine(Directory.GetCurrentDirectory(), "Content");
        var cardDatabase = CardDatabase.LoadFromJson(contentPath);
        var deckInitializer = new DeckInitializer(cardDatabase);
        deckInitializer.InitializeAllNPCDecks(context.GameWorld);
        
        Console.WriteLine("[Phase3] NPC deck initialization complete using JSON card database.");
    }

    // REMOVED: InitializeNPCDecks - replaced by DeckInitializer.InitializeAllNPCDecks
    private void InitializeNPCDecks_REMOVED(InitializationContext context)
    {
        // This method is no longer used - replaced by DeckInitializer
        Console.WriteLine("[Phase3] Initializing NPC THREE-DECK ARCHITECTURE...");

        List<NPC> npcs = context.GameWorld.WorldState.NPCs;

        if (npcs == null || !npcs.Any())
        {
            Console.WriteLine("[Phase3] No NPCs found to initialize decks for");
            return;
        }

        // Cards are already loaded in GameWorld from Phase0
        GameWorld gameWorld = context.GameWorld;

        foreach (NPC npc in npcs)
        {
            // DECK 1: CONVERSATION DECK (20-30 cards)
            // Load from NPCConversationDeckMappings
            if (gameWorld.NPCConversationDeckMappings.TryGetValue(npc.ID.ToLower(), out List<string>? cardIds))
            {
                npc.ConversationDeck = new CardDeck();
                foreach (string cardId in cardIds)
                {
                    if (gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard? card))
                    {
                        // Clone card with NPC-specific context
                        ConversationCard npcCard = CloneCardForNPC(card, npc);
                        npc.ConversationDeck.AddCard(npcCard);
                    }
                }
                Console.WriteLine($"[Phase3] {npc.Name}: Conversation deck has {npc.ConversationDeck.Count} cards");
            }
            // DECK 2: GOAL DECK (2-8 cards)
            // Load from NPCGoalDecks
            if (gameWorld.NPCGoalDecks.TryGetValue(npc.ID.ToLower(), out List<ConversationCard>? goalCards))
            {
                npc.GoalDeck = new CardDeck();
                foreach (ConversationCard goalCard in goalCards)
                {
                    // Clone goal card with NPC-specific context
                    ConversationCard npcGoal = CloneCardForNPC(goalCard, npc);
                    npc.GoalDeck.AddCard(npcGoal);
                }
                Console.WriteLine($"[Phase3] {npc.Name}: Goal deck has {npc.GoalDeck.Count} cards (Letters: {npc.HasPromiseCards()})");
            }
            // DECK 3: EXCHANGE DECK (5-10 cards, Mercantile only)
            // Only for MERCANTILE personality
            if (npc.PersonalityType == PersonalityType.MERCANTILE)
            {
                if (gameWorld.NPCExchangeDecks.TryGetValue(npc.ID.ToLower(), out List<ConversationCard>? exchangeCards))
                {
                    npc.ExchangeDeck = new CardDeck();
                    foreach (ConversationCard exchangeCard in exchangeCards)
                    {
                        // Clone exchange card with NPC-specific context
                        ConversationCard npcExchange = CloneCardForNPC(exchangeCard, npc);
                        npc.ExchangeDeck.AddCard(npcExchange);
                    }
                    Console.WriteLine($"[Phase3] {npc.Name}: Exchange deck has {npc.ExchangeDeck.Count} cards");
                }
            }

            // Report deck status
            Console.WriteLine($"[Phase3] {npc.Name} deck summary:");
            Console.WriteLine($"  - Conversation: {npc.ConversationDeck?.Count ?? 0} cards");
            Console.WriteLine($"  - Goal: {npc.GoalDeck?.Count ?? 0} cards");
            Console.WriteLine($"  - Exchange: {npc.ExchangeDeck?.Count ?? 0} cards");
            Console.WriteLine($"  - Has Letters: {npc.HasPromiseCards()}");
            Console.WriteLine($"  - Has Burdens: {npc.HasBurdenHistory()} ({npc.CountBurdenCards()} cards)");
            // Crisis system removed - no special cases
        }

        Console.WriteLine($"[Phase3] Initialized THREE-DECK ARCHITECTURE for {npcs.Count} NPCs");
    }

    /// <summary>
    /// Clone a card with NPC-specific context
    /// </summary>
    private ConversationCard CloneCardForNPC(ConversationCard original, NPC npc)
    {
        // Create new context with NPC-specific values
        CardContext npcContext = new CardContext
        {
            Personality = npc.PersonalityType,
            EmotionalState = npc.CurrentEmotionalState,
            NPCName = npc.Name,
            NPCPersonality = npc.PersonalityType,
            // Preserve original context properties
            UrgencyLevel = original.Context?.UrgencyLevel ?? "Normal",
            HasDeadline = original.Context?.HasDeadline ?? false,
            MinutesUntilDeadline = original.Context?.MinutesUntilDeadline ?? 0,
            ObservationType = original.Context?.ObservationType,
            LetterId = original.Context?.LetterId,
            TargetNpcId = original.Context?.TargetNpcId,
            ExchangeData = original.Context?.ExchangeData,
            ObservationId = original.Context?.ObservationId,
            ObservationText = original.Context?.ObservationText,
            ObservationDescription = original.Context?.ObservationDescription,
            ExchangeName = original.Context?.ExchangeName,
            ExchangeCost = original.Context?.ExchangeCost,
            ExchangeReward = original.Context?.ExchangeReward,
            ObservationDecayState = original.Context?.ObservationDecayState,
            ObservationDecayDescription = original.Context?.ObservationDecayDescription,
            GeneratesLetterOnSuccess = original.Context?.GeneratesLetterOnSuccess ?? false,
            IsOfferCard = original.Context?.IsOfferCard ?? false,
            GrantsToken = original.Context?.GrantsToken ?? false,
            IsAcceptCard = original.Context?.IsAcceptCard ?? false,
            IsDeclineCard = original.Context?.IsDeclineCard ?? false,
            OfferCardId = original.Context?.OfferCardId,
            CustomText = original.Context?.CustomText,
            LetterDetails = original.Context?.LetterDetails,
            ValidStates = original.Context?.ValidStates ?? new List<EmotionalState>(),
            ExchangeOffer = original.Context?.ExchangeOffer,
            ExchangeRequest = original.Context?.ExchangeRequest,
            ObservationLocation = original.Context?.ObservationLocation,
            ObservationSpot = original.Context?.ObservationSpot
        };

        // Create cloned card with NPC context
        return new ConversationCard
        {
            Id = original.Id,
            Mechanics = original.Mechanics,
            Category = original.Category,
            Context = npcContext,
            Type = original.Type,
            Persistence = original.Persistence,
            Weight = original.Weight,
            BaseComfort = original.BaseComfort,
            SuccessState = original.SuccessState,
            IsStateCard = original.IsStateCard,
            GrantsToken = original.GrantsToken,
            FailureState = original.FailureState,
            IsObservation = original.IsObservation,
            ObservationSource = original.ObservationSource,
            CanDeliverLetter = original.CanDeliverLetter,
            DeliveryObligationId = original.DeliveryObligationId,
            ManipulatesObligations = original.ManipulatesObligations,
            SuccessRate = original.SuccessRate,
            DisplayName = original.DisplayName,
            Description = original.Description,
            IsGoalCard = original.IsGoalCard,
            GoalCardType = original.GoalCardType
        };
    }

    private void InitializeGoalDeckForNPC(NPC npc, InitializationContext context)
    {
        // Get letter cards from the Goal deck repository
        if (context.GoalDeckRepository != null)
        {
            List<PromiseCardConfiguration> letterConfigs = context.GoalDeckRepository.GetPromiseCardsForNPC(npc.ID);
            if (letterConfigs != null && letterConfigs.Any())
            {
                // Convert configurations to ConversationCards  
                List<ConversationCard> goalCards = new List<ConversationCard>();
                foreach (PromiseCardConfiguration config in letterConfigs)
                {
                    // Create a goal card from the promise configuration
                    ConversationCard card = new ConversationCard
                    {
                        Id = $"promise_{npc.ID}_{config.CardId}",
                        Mechanics = CardMechanicsType.Promise,
                        Category = CardCategory.Promise.ToString(),
                        Type = CardType.Goal, // Promise cards are goal cards
                        TokenType = TokenType.Trust,
                        ConnectionType = ConnectionType.Trust,
                        Persistence = PersistenceType.Fleeting, // Use valid persistence type
                        Weight = 2, // Standard weight for promise cards
                        BaseComfort = 0, // No comfort for goal cards
                        IsGoalCard = true,
                        GoalCardType = ConversationType.Promise.ToString(),
                        DisplayName = $"Letter from {npc.Name}",
                        Description = $"Negotiate terms for a letter delivery",
                        Context = new CardContext
                        {
                            NPCName = npc.Name,
                            Personality = npc.PersonalityType,
                            EmotionalState = npc.CurrentEmotionalState
                        }
                    };
                    goalCards.Add(card);
                }

                npc.InitializeGoalDeck(goalCards);
                Console.WriteLine($"[Phase3] Initialized Goal deck for {npc.Name} with {goalCards.Count} cards from letter_decks.json");

                foreach (ConversationCard card in goalCards)
                {
                    Console.WriteLine($"  - {card.DisplayName ?? card.Id} (Weight: {card.Weight})");
                }
            }
        }
    }

    // REMOVED: LoadGoalDecks - replaced by CardDatabase.LoadFromJson
    private void LoadGoalDecks_REMOVED(InitializationContext context)
    {
        // This method is no longer used - goals loaded from goals.json by CardDatabase
        Console.WriteLine("[Phase3] Loading Goal deck configurations...");

        // Create and initialize Goal deck repository
        GoalDeckRepository GoalDeckRepo = new GoalDeckRepository(context.ContentPath);
        GoalDeckRepo.LoadGoalDecks();

        // Store in context for later use
        context.GoalDeckRepository = GoalDeckRepo;

        Console.WriteLine("[Phase3] Goal deck configurations loaded");
    }

    private TierLevel ParseTierLevel(string tierString)
    {
        if (string.IsNullOrEmpty(tierString))
            return TierLevel.T1;

        return tierString.ToUpper() switch
        {
            "T1" => TierLevel.T1,
            "T2" => TierLevel.T2,
            "T3" => TierLevel.T3,
            _ => TierLevel.T1
        };
    }

    private void LoadRoutes(InitializationContext context)
    {
        string routesPath = Path.Combine(context.ContentPath, "routes.json");
        Console.WriteLine($"[LoadRoutes] Looking for routes at: {routesPath}");
        Console.WriteLine($"[LoadRoutes] File exists: {File.Exists(routesPath)}");

        if (!File.Exists(routesPath))
        {
            Console.WriteLine("INFO: routes.json not found, player will use default walking");
            return;
        }

        try
        {
            Console.WriteLine($"[LoadRoutes] About to load routes from {routesPath}");
            List<RouteDTO> routeDTOs = context.ContentLoader.LoadValidatedContent<List<RouteDTO>>(routesPath);
            Console.WriteLine($"[LoadRoutes] Loaded {routeDTOs?.Count ?? 0} route DTOs");

            if (routeDTOs == null || !routeDTOs.Any())
            {
                Console.WriteLine("WARNING: No routes found in routes.json");
                return;
            }

            RouteFactory routeFactory = new RouteFactory();
            List<LocationSpot> spots = context.GameWorld.WorldState.locationSpots;

            foreach (RouteDTO dto in routeDTOs)
            {
                try
                {
                    // Verify spots exist
                    LocationSpot? originSpot = spots.FirstOrDefault(s => s.SpotID == dto.OriginLocationSpot);
                    LocationSpot? destinationSpot = spots.FirstOrDefault(s => s.SpotID == dto.DestinationLocationSpot);

                    if (originSpot == null || destinationSpot == null)
                    {
                        context.Warnings.Add($"Route {dto.Id} has invalid spots: {dto.OriginLocationSpot} -> {dto.DestinationLocationSpot}");
                        continue;
                    }

                    // Parse travel method
                    TravelMethods method = TravelMethods.Walking;
                    if (!string.IsNullOrEmpty(dto.Method))
                    {
                        if (!Enum.TryParse<TravelMethods>(dto.Method, true, out method))
                        {
                            context.Warnings.Add($"Invalid travel method '{dto.Method}' for route {dto.Id}, defaulting to Walking");
                        }
                    }

                    // Parse departure time
                    TimeBlocks? departureTime = null;
                    if (!string.IsNullOrEmpty(dto.DepartureTime))
                    {
                        if (Enum.TryParse<TimeBlocks>(dto.DepartureTime, true, out TimeBlocks time))
                        {
                            departureTime = time;
                        }
                    }

                    // Create route with available data
                    RouteOption route = new RouteOption
                    {
                        Id = dto.Id,
                        Name = dto.Name,
                        OriginLocationSpot = originSpot.SpotID,
                        DestinationLocationSpot = destinationSpot.SpotID,
                        TravelTimeMinutes = dto.TravelTimeMinutes,
                        BaseStaminaCost = dto.BaseStaminaCost,
                        BaseCoinCost = dto.BaseCoinCost,
                        Method = method,
                        IsDiscovered = dto.IsDiscovered,
                        DepartureTime = departureTime,
                        Description = dto.Description ?? $"Route from {originSpot.Name} to {destinationSpot.Name}",
                        TierRequired = ParseTierLevel(dto.TierRequired)
                    };

                    context.GameWorld.WorldState.Routes.Add(route);
                    Console.WriteLine($"  Loaded route: {route.Name} ({route.OriginLocationSpot} -> {route.DestinationLocationSpot})");
                }
                catch (Exception ex)
                {
                    context.Warnings.Add($"Failed to create route {dto.Id}: {ex.Message}");
                }
            }

            Console.WriteLine($"Loaded {context.GameWorld.WorldState.Routes.Count} routes");

            // Connect routes to locations
            ConnectRoutesToLocations(context);
        }
        catch (ContentValidationException ex)
        {
            Console.WriteLine($"[LoadRoutes] ContentValidationException: {ex.Message}");
            foreach (ValidationError error in ex.Errors)
            {
                Console.WriteLine($"[LoadRoutes] Validation error: {error.Message}");
                context.Warnings.Add($"Route validation: {error.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadRoutes] Exception: {ex.Message}");
            Console.WriteLine($"[LoadRoutes] Stack trace: {ex.StackTrace}");
            context.Warnings.Add($"Failed to load routes: {ex.Message}");
        }
    }

    private void LoadLetterTemplates(InitializationContext context)
    {
        string templatesPath = Path.Combine(context.ContentPath, "letter_templates.json");
        Console.WriteLine($"[LoadLetterTemplates] Looking for templates at: {templatesPath}");
        Console.WriteLine($"[LoadLetterTemplates] ContentPath: {context.ContentPath}");
        Console.WriteLine($"[LoadLetterTemplates] File exists: {File.Exists(templatesPath)}");

        if (!File.Exists(templatesPath))
        {
            Console.WriteLine("INFO: letter_templates.json not found, creating basic templates");
            CreateBasicLetterTemplates(context);
            return;
        }

        try
        {
            List<LetterTemplateDTO> templateDTOs = context.ContentLoader.LoadValidatedContent<List<LetterTemplateDTO>>(templatesPath);

            if (templateDTOs == null || !templateDTOs.Any())
            {
                Console.WriteLine("WARNING: No letter templates found, creating basics");
                CreateBasicLetterTemplates(context);
                return;
            }

            // LetterTemplateFactory removed - templates created directly

            foreach (LetterTemplateDTO dto in templateDTOs)
            {
                try
                {
                    // Parse token type
                    if (!Enum.TryParse<ConnectionType>(dto.TokenType, true, out ConnectionType tokenType))
                    {
                        tokenType = ConnectionType.Trust;
                        context.Warnings.Add($"Invalid token type '{dto.TokenType}' for template {dto.Id}, defaulting to Trust");
                    }

                    // Parse letter size
                    SizeCategory letterSize = SizeCategory.Small;
                    if (!string.IsNullOrEmpty(dto.Size))
                    {
                        if (!Enum.TryParse<SizeCategory>(dto.Size, true, out letterSize))
                        {
                            context.Warnings.Add($"Invalid letter size '{dto.Size}' for template {dto.Id}, defaulting to Small");
                        }
                    }

                    // Parse category
                    LetterCategory category = LetterCategory.Basic;
                    if (!string.IsNullOrEmpty(dto.Category))
                    {
                        if (!Enum.TryParse<LetterCategory>(dto.Category, true, out category))
                        {
                            context.Warnings.Add($"Invalid letter category '{dto.Category}' for template {dto.Id}, defaulting to Basic");
                        }
                    }

                    // Parse tier level
                    TierLevel tierLevel = TierLevel.T1;
                    if (!string.IsNullOrEmpty(dto.TierLevel))
                    {
                        if (!Enum.TryParse<TierLevel>(dto.TierLevel, true, out tierLevel))
                        {
                            context.Warnings.Add($"Invalid tier level '{dto.TierLevel}' for template {dto.Id}, defaulting to T1");
                        }
                    }

                    // Parse emotional weight
                    EmotionalWeight emotionalWeight = EmotionalWeight.MEDIUM;
                    if (!string.IsNullOrEmpty(dto.EmotionalWeight))
                    {
                        if (!Enum.TryParse<EmotionalWeight>(dto.EmotionalWeight, true, out emotionalWeight))
                        {
                            context.Warnings.Add($"Invalid emotional weight '{dto.EmotionalWeight}' for template {dto.Id}, defaulting to MEDIUM");
                        }
                    }

                    // Parse stakes
                    StakeType stakes = StakeType.REPUTATION;
                    if (!string.IsNullOrEmpty(dto.Stakes))
                    {
                        if (!Enum.TryParse<StakeType>(dto.Stakes, true, out stakes))
                        {
                            context.Warnings.Add($"Invalid stakes type '{dto.Stakes}' for template {dto.Id}, defaulting to REPUTATION");
                        }
                    }

                    // Create template using DTO
                    LetterTemplate template = new LetterTemplate
                    {
                        Id = dto.Id,
                        Description = dto.Description ?? $"A {tokenType} letter",
                        TokenType = tokenType,
                        MinDeadlineInMinutes = dto.MinDeadlineInMinutes,
                        MaxDeadlineInMinutes = dto.MaxDeadlineInMinutes,
                        MinPayment = dto.MinPayment,
                        MaxPayment = dto.MaxPayment,
                        Category = category,
                        MinTokensRequired = dto.MinTokensRequired ?? 1,
                        Size = letterSize,
                        PossibleSenders = dto.PossibleSenders?.ToArray() ?? new string[0],
                        PossibleRecipients = dto.PossibleRecipients?.ToArray() ?? new string[0],
                        UnlocksLetterIds = dto.UnlocksLetterIds?.ToArray() ?? new string[0],
                        TierLevel = tierLevel,
                        ConsequenceIfLate = dto.ConsequenceIfLate ?? "",
                        ConsequenceIfDelivered = dto.ConsequenceIfDelivered ?? "",
                        EmotionalWeight = emotionalWeight,
                        Stakes = stakes
                    };

                    context.GameWorld.WorldState.LetterTemplates.Add(template);
                    Console.WriteLine($"  Loaded template: {template.Id} ({tokenType})");
                }
                catch (Exception ex)
                {
                    context.Warnings.Add($"Failed to create letter template {dto.Id}: {ex.Message}");
                }
            }

            Console.WriteLine($"Loaded {context.GameWorld.WorldState.LetterTemplates.Count} letter templates");
        }
        catch (ContentValidationException ex)
        {
            foreach (ValidationError error in ex.Errors)
            {
                context.Warnings.Add($"DeliveryObligation template validation: {error.Message}");
            }
            CreateBasicLetterTemplates(context);
        }
        catch (Exception ex)
        {
            context.Warnings.Add($"Failed to load letter templates: {ex.Message}");
            CreateBasicLetterTemplates(context);
        }
    }

    private void CreateBasicLetterTemplates(InitializationContext context)
    {
        // LetterTemplateFactory removed - templates created directly

        // Create one template for each token type
        ConnectionType[] tokenTypes = new[]
        {
            ConnectionType.Trust,
            ConnectionType.Commerce,
            ConnectionType.Status,
            ConnectionType.Shadow
        };

        foreach (ConnectionType tokenType in tokenTypes)
        {
            LetterTemplate template = new LetterTemplate
            {
                Id = $"basic_{tokenType.ToString().ToLower()}",
                Description = $"A standard {tokenType} delivery",
                TokenType = tokenType,
                MinPayment = 2,
                MaxPayment = 8,
                MinDeadlineInMinutes = 1440, // 24 hours in minutes
                MaxDeadlineInMinutes = 4320, // 72 hours in minutes
                Size = SizeCategory.Small,
                Category = LetterCategory.Basic,
                MinTokensRequired = 1
            };

            context.GameWorld.WorldState.LetterTemplates.Add(template);
            Console.WriteLine($"  Created basic template: {template.Id}");
        }
    }

    private void ConnectRoutesToLocations(InitializationContext context)
    {
        Console.WriteLine("Connecting routes to locations...");

        List<Location> locations = context.GameWorld.WorldState.locations;
        List<LocationSpot> spots = context.GameWorld.WorldState.locationSpots;
        List<RouteOption> routes = context.GameWorld.WorldState.Routes;

        // Clear any existing connections first
        foreach (Location location in locations)
        {
            location.Connections.Clear();
        }

        // Group routes by origin spot's location
        IEnumerable<IGrouping<string?, RouteOption>> routesByOriginLocation = routes.GroupBy(r =>
        {
            LocationSpot? originSpot = spots.FirstOrDefault(s => s.SpotID == r.OriginLocationSpot);
            return originSpot?.LocationId;
        }).Where(g => g.Key != null);

        foreach (IGrouping<string?, RouteOption>? group in routesByOriginLocation)
        {
            Location? originLocation = locations.FirstOrDefault(l => l.Id == group.Key);
            if (originLocation == null)
            {
                context.Warnings.Add($"Route origin location '{group.Key}' not found");
                continue;
            }

            // Group by destination spot's location
            IEnumerable<IGrouping<string?, RouteOption>> routesByDestinationLocation = group.GroupBy(r =>
            {
                LocationSpot? destSpot = spots.FirstOrDefault(s => s.SpotID == r.DestinationLocationSpot);
                return destSpot?.LocationId;
            }).Where(g => g.Key != null);

            foreach (IGrouping<string?, RouteOption>? destGroup in routesByDestinationLocation)
            {
                LocationConnection connection = new LocationConnection
                {
                    DestinationLocationId = destGroup.Key,
                    RouteOptions = destGroup.ToList()
                };

                originLocation.Connections.Add(connection);
                Console.WriteLine($"  Connected {originLocation.Id} -> {destGroup.Key} with {connection.RouteOptions.Count} routes");
            }
        }

        Console.WriteLine($"Connected {routesByOriginLocation.Count()} locations with routes");
    }
}