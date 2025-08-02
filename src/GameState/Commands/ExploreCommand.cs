using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Command that allows players to explore the current location to discover hidden routes
/// </summary>
public class ExploreCommand : BaseGameCommand
{
    private readonly string _locationId;
    private readonly RouteRepository _routeRepository;
    private readonly InformationDiscoveryManager _informationManager;
    private readonly MessageSystem _messageSystem;
    private readonly ITimeManager _timeManager;
    private readonly Random _random = new Random();
    
    public int TimeCost { get; private set; }
    
    // Time cost varies by location size/complexity
    private readonly Dictionary<int, int> _timeCostByTier = new Dictionary<int, int>
    {
        { 1, 2 }, // Small locations: 2 hours
        { 2, 3 }, // Medium locations: 3 hours  
        { 3, 4 }, // Large locations: 4 hours
        { 4, 4 }, // Complex locations: 4 hours
        { 5, 4 }  // Max tier: 4 hours
    };
    
    public ExploreCommand(
        string locationId,
        int locationTier,
        RouteRepository routeRepository,
        InformationDiscoveryManager informationManager,
        MessageSystem messageSystem,
        ITimeManager timeManager)
    {
        _locationId = locationId;
        _routeRepository = routeRepository;
        _informationManager = informationManager;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
        
        // Set time cost based on location tier
        TimeCost = _timeCostByTier.GetValueOrDefault(locationTier, 3);
        Description = $"Explore area thoroughly ({TimeCost} hours)";
    }
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        
        // Check if player has enough time
        if (!_timeManager.CanPerformAction(TimeCost))
        {
            return CommandValidationResult.Failure(
                "Not enough time remaining today",
                true,
                "Rest to start a new day");
        }
        
        // Check if player has enough stamina (exploration is tiring)
        if (player.Stamina < 2)
        {
            return CommandValidationResult.Failure(
                "Too tired to explore",
                true,
                "Need at least 2 stamina");
        }
        
        return CommandValidationResult.Success();
    }
    
    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        
        // Spend time and stamina
        _timeManager.AdvanceTime(TimeCost);
        player.SpendStamina(2);
        
        // Get all routes from this location
        List<RouteOption> allRoutes = _routeRepository.GetRoutesFromLocation(_locationId);
        List<RouteOption> undiscoveredRoutes = allRoutes.Where(r => !r.IsDiscovered).ToList();
        
        if (!undiscoveredRoutes.Any())
        {
            _messageSystem.AddSystemMessage(
                "You explore the area thoroughly but find no new routes.",
                SystemMessageTypes.Info
            );
            
            return CommandResult.Success(
                "Exploration complete - no new discoveries",
                new { RoutesDiscovered = 0, TimeSpent = TimeCost }
            );
        }
        
        // Discover routes based on player's exploration skill/luck
        List<RouteOption> discoveredRoutes = new List<RouteOption>();
        int discoveryChance = 60; // Base 60% chance to discover each route
        
        // Higher tier routes are harder to discover
        foreach (var route in undiscoveredRoutes)
        {
            int adjustedChance = discoveryChance - (route.Tier - 1) * 10;
            if (_random.Next(100) < adjustedChance)
            {
                // Discover the route
                route.IsDiscovered = true;
                discoveredRoutes.Add(route);
                
                // Also register as information discovery
                string infoId = $"route_discovery_{route.Id}";
                _informationManager.DiscoverInformation(infoId);
                
                _messageSystem.AddSystemMessage(
                    $"🗺️ Discovered new route: {route.Name}!",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    $"   {route.Description}",
                    SystemMessageTypes.Info
                );
            }
        }
        
        if (discoveredRoutes.Any())
        {
            _messageSystem.AddSystemMessage(
                $"Your exploration revealed {discoveredRoutes.Count} new route{(discoveredRoutes.Count > 1 ? "s" : "")}!",
                SystemMessageTypes.Success
            );
            
            // Add exploration memory
            player.AddMemory(
                $"explored_{_locationId}_{gameWorld.CurrentDay}",
                $"Thoroughly explored the area and discovered {discoveredRoutes.Count} new routes",
                gameWorld.CurrentDay,
                2
            );
        }
        else
        {
            _messageSystem.AddSystemMessage(
                "You explored carefully but the hidden routes remain elusive. Try again later or ask locals.",
                SystemMessageTypes.Info
            );
        }
        
        return CommandResult.Success(
            $"Exploration complete - discovered {discoveredRoutes.Count} routes",
            new
            {
                RoutesDiscovered = discoveredRoutes.Count,
                RouteNames = discoveredRoutes.Select(r => r.Name).ToList(),
                TimeSpent = TimeCost,
                StaminaSpent = 2
            }
        );
    }
}