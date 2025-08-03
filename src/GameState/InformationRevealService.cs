using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service that manages revealing hidden content based on carried Information letters.
/// Information letters reveal NPCs, locations, and routes while carried in the satchel.
/// </summary>
public class InformationRevealService
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly RouteRepository _routeRepository;
    private readonly MessageSystem _messageSystem;

    public InformationRevealService(
        GameWorld gameWorld,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        RouteRepository routeRepository,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _routeRepository = routeRepository;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Check carried Information letters and reveal any hidden content.
    /// Called each turn to update revealed content based on current satchel.
    /// </summary>
    public void ProcessCarriedInformation()
    {
        var player = _gameWorld.GetPlayer();
        var carriedInfoLetters = player.CarriedLetters
            .Where(l => l.SpecialType == LetterSpecialType.Information && !string.IsNullOrEmpty(l.InformationId))
            .ToList();

        foreach (var letter in carriedInfoLetters)
        {
            ProcessInformationLetter(letter);
        }
    }

    /// <summary>
    /// Process a single Information letter and reveal associated content.
    /// </summary>
    private void ProcessInformationLetter(Letter letter)
    {
        var player = _gameWorld.GetPlayer();
        string infoId = letter.InformationId;

        // Parse the information ID to determine what to reveal
        // Format: "npc:merchant_guild" or "location:secret_market" or "route:hidden_pass"
        var parts = infoId.Split(':');
        if (parts.Length != 2) return;

        string infoType = parts[0].ToLower();
        string targetId = parts[1];

        switch (infoType)
        {
            case "npc":
                if (!player.UnlockedNPCIds.Contains(targetId))
                {
                    player.UnlockedNPCIds.Add(targetId);
                    var npc = _npcRepository.GetById(targetId);
                    if (npc != null)
                    {
                        _messageSystem.AddSystemMessage(
                            $"🔓 Information revealed: {npc.Name} can be found at {npc.Location}!",
                            SystemMessageTypes.Success
                        );
                    }
                }
                break;

            case "location":
                if (!player.UnlockedLocationIds.Contains(targetId))
                {
                    player.UnlockedLocationIds.Add(targetId);
                    var location = _locationRepository.GetLocation(targetId);
                    if (location != null)
                    {
                        _messageSystem.AddSystemMessage(
                            $"🔓 Information revealed: {location.Name} is now accessible!",
                            SystemMessageTypes.Success
                        );
                    }
                }
                break;

            case "route":
                // Routes are more complex - they need both endpoints
                // Format: "route:location1-location2" or "route:routeId"
                if (targetId.Contains('-'))
                {
                    // Format: location1-location2
                    var routeParts = targetId.Split('-');
                    if (routeParts.Length == 2)
                    {
                        var route = _routeRepository.GetAll()
                            .FirstOrDefault(r => r.Origin == routeParts[0] && r.Destination == routeParts[1]);
                        if (route != null)
                        {
                            DiscoverRoute(player, route);
                        }
                    }
                }
                else
                {
                    // Format: routeId
                    var route = _routeRepository.GetRouteById(targetId);
                    if (route != null)
                    {
                        DiscoverRoute(player, route);
                    }
                }
                break;

            case "service":
                if (!player.UnlockedServices.Contains(targetId))
                {
                    player.UnlockedServices.Add(targetId);
                    _messageSystem.AddSystemMessage(
                        $"🔓 Information revealed: New service '{targetId}' is now available!",
                        SystemMessageTypes.Success
                    );
                }
                break;
        }
    }

    /// <summary>
    /// Check if a specific NPC is revealed by carried information.
    /// </summary>
    public bool IsNPCRevealed(string npcId)
    {
        var player = _gameWorld.GetPlayer();
        
        // NPCs are revealed if:
        // 1. Already unlocked through other means
        if (player.UnlockedNPCIds.Contains(npcId))
            return true;

        // 2. Revealed by a carried Information letter
        return player.CarriedLetters.Any(l => 
            l.SpecialType == LetterSpecialType.Information && 
            l.InformationId == $"npc:{npcId}");
    }

    /// <summary>
    /// Check if a specific location is revealed by carried information.
    /// </summary>
    public bool IsLocationRevealed(string locationId)
    {
        var player = _gameWorld.GetPlayer();
        
        // Locations are revealed if:
        // 1. Already unlocked through other means
        if (player.UnlockedLocationIds.Contains(locationId))
            return true;

        // 2. Revealed by a carried Information letter
        return player.CarriedLetters.Any(l => 
            l.SpecialType == LetterSpecialType.Information && 
            l.InformationId == $"location:{locationId}");
    }

    /// <summary>
    /// Get all currently revealed NPCs including those from carried information.
    /// </summary>
    public List<string> GetAllRevealedNPCIds()
    {
        var player = _gameWorld.GetPlayer();
        var revealed = new HashSet<string>(player.UnlockedNPCIds);

        // Add NPCs revealed by carried information
        foreach (var letter in player.CarriedLetters.Where(l => l.SpecialType == LetterSpecialType.Information))
        {
            if (letter.InformationId?.StartsWith("npc:") == true)
            {
                revealed.Add(letter.InformationId.Substring(4));
            }
        }

        return revealed.ToList();
    }

    /// <summary>
    /// Get all currently revealed locations including those from carried information.
    /// </summary>
    public List<string> GetAllRevealedLocationIds()
    {
        var player = _gameWorld.GetPlayer();
        var revealed = new HashSet<string>(player.UnlockedLocationIds);

        // Add locations revealed by carried information
        foreach (var letter in player.CarriedLetters.Where(l => l.SpecialType == LetterSpecialType.Information))
        {
            if (letter.InformationId?.StartsWith("location:") == true)
            {
                revealed.Add(letter.InformationId.Substring(9));
            }
        }

        return revealed.ToList();
    }
    
    /// <summary>
    /// Helper method to discover a route and add it to player's known routes
    /// </summary>
    private void DiscoverRoute(Player player, RouteOption route)
    {
        string originKey = route.Origin;
        
        // Check if already known
        if (player.KnownRoutes.ContainsKey(originKey) && 
            player.KnownRoutes[originKey].Any(r => r.Id == route.Id))
        {
            return; // Already discovered
        }
        
        // Mark route as discovered
        route.IsDiscovered = true;
        
        // Add to player's known routes
        player.AddKnownRoute(route);
        
        // Show discovery message
        _messageSystem.AddSystemMessage(
            $"🔓 Information revealed: {route.Name} - Route from {route.Origin} to {route.Destination}!",
            SystemMessageTypes.Success
        );
    }
}