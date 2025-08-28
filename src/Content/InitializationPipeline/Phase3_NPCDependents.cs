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
        
        // 3. Initialize NPC Conversation Decks (CRITICAL - was missing!)
        InitializeNPCDecks(context);
    }
    
    private void InitializeNPCDecks(InitializationContext context)
    {
        Console.WriteLine("[Phase3] Initializing NPC conversation decks...");
        
        // Get NPCDeckFactory from context (should be registered in services)
        var npcs = context.GameWorld.WorldState.NPCs;
        
        if (npcs == null || !npcs.Any())
        {
            Console.WriteLine("[Phase3] No NPCs found to initialize decks for");
            return;
        }
        
        // Create a deck factory instance (no token manager during initialization)
        var deckFactory = new NPCDeckFactory(null);
        
        foreach (var npc in npcs)
        {
            try
            {
                // Initialize conversation deck for each NPC
                npc.InitializeConversationDeck(deckFactory);
                
                // Debug: Check personality BEFORE initializing exchange deck
                Console.WriteLine($"[Phase3] NPC {npc.Name} (ID: {npc.ID}) has PersonalityType: {npc.PersonalityType}");
                
                // Initialize exchange deck (without spot tags since we don't know location yet)
                // MERCANTILE NPCs will get their exchanges initialized here
                // Other personalities need spot information which comes at conversation time
                npc.InitializeExchangeDeck(null);
                
                // Debug: Check exchange deck contents
                if (npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
                {
                    Console.WriteLine($"[Phase3] Exchange deck for {npc.Name} contains {npc.ExchangeDeck.Count} cards:");
                    foreach (var card in npc.ExchangeDeck)
                    {
                        Console.WriteLine($"  - {card.TemplateType} (Personality: {card.NPCPersonality})");
                    }
                }
                
                // Initialize letter deck for specific NPCs
                InitializeLetterDeckForNPC(npc);
                
                // Note: Crisis letters are added later in Phase8 when meeting obligations are created
                // This ensures proper initialization order
                
                Console.WriteLine($"[Phase3] Initialized decks for NPC: {npc.Name} (ID: {npc.ID})");
            }
            catch (Exception ex)
            {
                context.Warnings.Add($"Failed to initialize decks for NPC {npc.Name}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"[Phase3] Initialized decks for {npcs.Count} NPCs");
    }
    
    private void InitializeLetterDeckForNPC(NPC npc)
    {
        // Initialize letter deck for NPCs that have letters based on mechanical property
        if (npc.HasLetterDeck)
        {
            npc.LetterDeck = LetterCardFactory.CreateElenaLetterDeck(npc.ID);
            Console.WriteLine($"[Phase3] Initialized letter deck for {npc.Name} with {npc.LetterDeck.Count} cards");
            
            foreach (var letterCard in npc.LetterDeck)
            {
                Console.WriteLine($"  - {letterCard.Title} (Requires: {string.Join(", ", letterCard.Eligibility.RequiredTokens.Select(t => $"{t.Value} {t.Key}"))}, States: {string.Join(", ", letterCard.Eligibility.RequiredStates)})");
            }
        }
        // Letter decks are now initialized based on HasLetterDeck property
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
        var routesByOriginLocation = routes.GroupBy(r => 
        {
            var originSpot = spots.FirstOrDefault(s => s.SpotID == r.OriginLocationSpot);
            return originSpot?.LocationId;
        }).Where(g => g.Key != null);

        foreach (var group in routesByOriginLocation)
        {
            Location? originLocation = locations.FirstOrDefault(l => l.Id == group.Key);
            if (originLocation == null)
            {
                context.Warnings.Add($"Route origin location '{group.Key}' not found");
                continue;
            }

            // Group by destination spot's location
            var routesByDestinationLocation = group.GroupBy(r => 
            {
                var destSpot = spots.FirstOrDefault(s => s.SpotID == r.DestinationLocationSpot);
                return destSpot?.LocationId;
            }).Where(g => g.Key != null);

            foreach (var destGroup in routesByDestinationLocation)
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