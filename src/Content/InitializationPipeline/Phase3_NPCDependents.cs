using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Phase 3: Load entities that depend on NPCs and locations being loaded.
/// This includes: Routes, Letter Templates
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
        
        // 2. Load Letter Templates (depends on NPCs for validation)
        LoadLetterTemplates(context);
    }
    
    private void LoadRoutes(InitializationContext context)
    {
        var routesPath = Path.Combine(context.ContentPath, "routes.json");
        
        if (!File.Exists(routesPath))
        {
            Console.WriteLine("INFO: routes.json not found, player will use default walking");
            return;
        }
        
        try
        {
            var routeDTOs = context.ContentLoader.LoadValidatedContent<List<RouteDTO>>(routesPath);
            
            if (routeDTOs == null || !routeDTOs.Any())
            {
                Console.WriteLine("WARNING: No routes found in routes.json");
                return;
            }
            
            var routeFactory = new RouteFactory();
            var locations = context.GameWorld.WorldState.locations;
            
            foreach (var dto in routeDTOs)
            {
                try
                {
                    // Verify locations exist
                    var origin = locations.FirstOrDefault(l => l.Id == dto.Origin);
                    var destination = locations.FirstOrDefault(l => l.Id == dto.Destination);
                    
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
                        if (Enum.TryParse<TimeBlocks>(dto.DepartureTime, true, out var time))
                        {
                            departureTime = time;
                        }
                    }
                    
                    // Create route with available data
                    var route = new RouteOption
                    {
                        Id = dto.Id,
                        Name = dto.Name,
                        Origin = origin.Id,
                        Destination = destination.Id,
                        TravelTimeHours = dto.TravelTimeHours,
                        BaseStaminaCost = dto.BaseStaminaCost,
                        BaseCoinCost = dto.BaseCoinCost,
                        Method = method,
                        IsDiscovered = dto.IsDiscovered,
                        DepartureTime = departureTime,
                        Description = dto.Description ?? $"Route from {origin.Name} to {destination.Name}"
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
            foreach (var error in ex.Errors)
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
        var templatesPath = Path.Combine(context.ContentPath, "letter_templates.json");
        
        if (!File.Exists(templatesPath))
        {
            Console.WriteLine("INFO: letter_templates.json not found, creating basic templates");
            CreateBasicLetterTemplates(context);
            return;
        }
        
        try
        {
            var templateDTOs = context.ContentLoader.LoadValidatedContent<List<LetterTemplateDTO>>(templatesPath);
            
            if (templateDTOs == null || !templateDTOs.Any())
            {
                Console.WriteLine("WARNING: No letter templates found, creating basics");
                CreateBasicLetterTemplates(context);
                return;
            }
            
            var templateFactory = new LetterTemplateFactory();
            
            foreach (var dto in templateDTOs)
            {
                try
                {
                    // Parse token type
                    if (!Enum.TryParse<ConnectionType>(dto.TokenType, true, out var tokenType))
                    {
                        tokenType = ConnectionType.Common;
                        context.Warnings.Add($"Invalid token type '{dto.TokenType}' for template {dto.Id}, defaulting to Common");
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
                    
                    // Create template using DTO
                    var template = new LetterTemplate
                    {
                        Id = dto.Id,
                        Description = dto.Description ?? $"A {tokenType} letter",
                        TokenType = tokenType,
                        MinDeadline = dto.MinDeadline,
                        MaxDeadline = dto.MaxDeadline,
                        MinPayment = dto.MinPayment,
                        MaxPayment = dto.MaxPayment,
                        Category = category,
                        MinTokensRequired = dto.MinTokensRequired ?? 1,
                        Size = letterSize,
                        PossibleSenders = dto.PossibleSenders?.ToArray() ?? new string[0],
                        PossibleRecipients = dto.PossibleRecipients?.ToArray() ?? new string[0],
                        UnlocksLetterIds = dto.UnlocksLetterIds?.ToArray() ?? new string[0],
                        IsChainLetter = dto.IsChainLetter
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
            foreach (var error in ex.Errors)
            {
                context.Warnings.Add($"Letter template validation: {error.Message}");
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
        var templateFactory = new LetterTemplateFactory();
        
        // Create one template for each token type
        var tokenTypes = new[] 
        { 
            ConnectionType.Common, 
            ConnectionType.Commerce, 
            ConnectionType.Trust,
            ConnectionType.Status,
            ConnectionType.Shadow
        };
        
        foreach (var tokenType in tokenTypes)
        {
            var template = new LetterTemplate
            {
                Id = $"basic_{tokenType.ToString().ToLower()}",
                Description = $"A standard {tokenType} delivery",
                TokenType = tokenType,
                MinPayment = 2,
                MaxPayment = 8,
                MinDeadline = 24,
                MaxDeadline = 72,
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
        
        var locations = context.GameWorld.WorldState.locations;
        var routes = context.GameWorld.WorldState.Routes;
        
        // Clear any existing connections first
        foreach (var location in locations)
        {
            location.Connections.Clear();
        }
        
        // Group routes by origin location
        var routesByOrigin = routes.GroupBy(r => r.Origin);
        
        foreach (var group in routesByOrigin)
        {
            var originLocation = locations.FirstOrDefault(l => l.Id == group.Key);
            if (originLocation == null)
            {
                context.Warnings.Add($"Route origin location '{group.Key}' not found");
                continue;
            }
            
            // Group by destination
            var routesByDestination = group.GroupBy(r => r.Destination);
            
            foreach (var destGroup in routesByDestination)
            {
                var connection = new LocationConnection
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