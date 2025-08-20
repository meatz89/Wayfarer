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

        if (!File.Exists(routesPath))
        {
            Console.WriteLine("INFO: routes.json not found, player will use default walking");
            return;
        }

        try
        {
            List<RouteDTO> routeDTOs = context.ContentLoader.LoadValidatedContent<List<RouteDTO>>(routesPath);

            if (routeDTOs == null || !routeDTOs.Any())
            {
                Console.WriteLine("WARNING: No routes found in routes.json");
                return;
            }

            RouteFactory routeFactory = new RouteFactory();
            List<Location> locations = context.GameWorld.WorldState.locations;

            foreach (RouteDTO dto in routeDTOs)
            {
                try
                {
                    // Verify locations exist
                    Location? origin = locations.FirstOrDefault(l => l.Id == dto.Origin);
                    Location? destination = locations.FirstOrDefault(l => l.Id == dto.Destination);

                    if (origin == null || destination == null)
                    {
                        context.Warnings.Add($"Route {dto.Id} has invalid locations: {dto.Origin} -> {dto.Destination}");
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
                        Origin = origin.Id,
                        Destination = destination.Id,
                        TravelTimeMinutes = dto.TravelTimeMinutes,
                        BaseStaminaCost = dto.BaseStaminaCost,
                        BaseCoinCost = dto.BaseCoinCost,
                        Method = method,
                        IsDiscovered = dto.IsDiscovered,
                        DepartureTime = departureTime,
                        Description = dto.Description ?? $"Route from {origin.Name} to {destination.Name}",
                        TierRequired = ParseTierLevel(dto.TierRequired)
                    };

                    context.GameWorld.WorldState.Routes.Add(route);
                    Console.WriteLine($"  Loaded route: {route.Name} ({route.Origin} -> {route.Destination})");
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
            foreach (ValidationError error in ex.Errors)
            {
                context.Warnings.Add($"Route validation: {error.Message}");
            }
        }
        catch (Exception ex)
        {
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

            LetterTemplateFactory templateFactory = new LetterTemplateFactory();

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
                        HumanContext = dto.HumanContext ?? "",
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
        LetterTemplateFactory templateFactory = new LetterTemplateFactory();

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
        List<RouteOption> routes = context.GameWorld.WorldState.Routes;

        // Clear any existing connections first
        foreach (Location location in locations)
        {
            location.Connections.Clear();
        }

        // Group routes by origin location
        IEnumerable<IGrouping<string, RouteOption>> routesByOrigin = routes.GroupBy(r => r.Origin);

        foreach (IGrouping<string, RouteOption> group in routesByOrigin)
        {
            Location? originLocation = locations.FirstOrDefault(l => l.Id == group.Key);
            if (originLocation == null)
            {
                context.Warnings.Add($"Route origin location '{group.Key}' not found");
                continue;
            }

            // Group by destination
            IEnumerable<IGrouping<string, RouteOption>> routesByDestination = group.GroupBy(r => r.Destination);

            foreach (IGrouping<string, RouteOption> destGroup in routesByDestination)
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

        Console.WriteLine($"Connected {routesByOrigin.Count()} locations with routes");
    }
}