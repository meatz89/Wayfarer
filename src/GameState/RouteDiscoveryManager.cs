using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.GameState;

/// <summary>
/// Manages route discovery through NPC relationships and natural play.
/// Routes are learned from NPCs who know them, not through arbitrary counters.
/// </summary>
public class RouteDiscoveryManager
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _connectionTokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly RouteRepository _routeRepository;
    private readonly RouteDiscoveryRepository _routeDiscoveryRepository;
    private readonly ItemRepository _itemRepository;
    private readonly MessageSystem _messageSystem;
    private readonly NarrativeService _narrativeService;
    private readonly InformationDiscoveryManager _informationManager;

    public RouteDiscoveryManager(
        GameWorld gameWorld,
        TokenMechanicsManager connectionTokenManager,
        NPCRepository npcRepository,
        RouteRepository routeRepository,
        RouteDiscoveryRepository routeDiscoveryRepository,
        ItemRepository itemRepository,
        MessageSystem messageSystem,
        NarrativeService narrativeService,
        InformationDiscoveryManager informationManager)
    {
        _gameWorld = gameWorld;
        _connectionTokenManager = connectionTokenManager;
        _npcRepository = npcRepository;
        _routeRepository = routeRepository;
        _routeDiscoveryRepository = routeDiscoveryRepository;
        _itemRepository = itemRepository;
        _messageSystem = messageSystem;
        _narrativeService = narrativeService;
        _informationManager = informationManager;
    }

    /// <summary>
    /// Get routes that can be discovered from NPCs at the current location
    /// </summary>
    public List<RouteDiscoveryOption> GetAvailableDiscoveries(string locationId)
    {
        List<RouteDiscoveryOption> discoveryOptions = new List<RouteDiscoveryOption>();

        // Get all NPCs at current location
        List<NPC> availableNPCs = _npcRepository.GetNPCsForLocation(locationId);

        foreach (NPC npc in availableNPCs)
        {
            // Get routes this NPC knows about
            List<RouteDiscoveryOption> npcDiscoveries = GetDiscoveriesFromNPC(npc);
            discoveryOptions.AddRange(npcDiscoveries);
        }

        return discoveryOptions;
    }

    /// <summary>
    /// Get route discoveries available from a specific NPC
    /// </summary>
    public List<RouteDiscoveryOption> GetDiscoveriesFromNPC(NPC npc)
    {
        List<RouteDiscoveryOption> discoveryOptions = new List<RouteDiscoveryOption>();
        Player player = _gameWorld.GetPlayer();

        // Get all routes this NPC knows
        List<RouteDiscovery> knownRoutes = _routeDiscoveryRepository.GetRoutesKnownByNPC(npc.ID);

        foreach (RouteDiscovery discovery in knownRoutes)
        {
            // Get the actual route
            RouteOption route = _routeRepository.GetRouteById(discovery.RouteId);
            if (route == null || route.IsDiscovered) continue;

            // Check if player meets requirements
            RouteDiscoveryRequirements meetsRequirements = CheckDiscoveryRequirements(discovery, npc, player);

            // Get total tokens with this NPC
            Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            int totalTokens = npcTokens.Values.Sum();
            bool canAfford = totalTokens >= discovery.RequiredTokensWithNPC;

            discoveryOptions.Add(new RouteDiscoveryOption
            {
                Discovery = discovery,
                Route = route,
                TeachingNPC = npc,
                MeetsRequirements = meetsRequirements,
                PlayerTokensWithNPC = totalTokens,
                CanAfford = canAfford
            });
        }

        return discoveryOptions;
    }

    /// <summary>
    /// Check if player meets all requirements to discover a route
    /// </summary>
    private RouteDiscoveryRequirements CheckDiscoveryRequirements(RouteDiscovery discovery, NPC npc, Player player)
    {
        RouteDiscoveryRequirements requirements = new RouteDiscoveryRequirements();

        // Check relationship trust based on required tokens
        Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
        int totalTokens = npcTokens.Values.Sum();
        requirements.HasEnoughTrust = totalTokens >= discovery.RequiredTokensWithNPC;
        requirements.TrustLevel = totalTokens;

        // Check equipment requirements from the specific NPC context
        requirements.HasRequiredEquipment = true;
        requirements.MissingEquipment = new List<string>();

        // Get context-specific requirements for this NPC
        RouteDiscoveryContext? context = _routeDiscoveryRepository.GetDiscoveryContext(discovery.RouteId, npc.ID);
        if (context != null && context.RequiredEquipment != null)
        {
            foreach (string equipmentId in context.RequiredEquipment)
            {
                if (!PlayerHasItem(player, equipmentId))
                {
                    requirements.HasRequiredEquipment = false;
                    requirements.MissingEquipment.Add(equipmentId);
                }
            }
        }

        // Standing obligations removed from new structure
        requirements.HasRequiredObligation = true;

        requirements.MeetsAllRequirements = requirements.HasEnoughTrust &&
                                           requirements.HasRequiredEquipment &&
                                           requirements.HasRequiredObligation;

        return requirements;
    }

    /// <summary>
    /// Check if player has a specific item
    /// </summary>
    private bool PlayerHasItem(Player player, string itemId)
    {
        return player.Inventory.ItemSlots.Any(slot => slot == itemId);
    }

    /// <summary>
    /// Attempt to discover a route by spending tokens with an NPC
    /// </summary>
    public bool TryDiscoverRoute(string routeId)
    {
        // Find the route
        RouteOption route = _routeRepository.GetRouteById(routeId);
        if (route == null)
        {
            _messageSystem.AddSystemMessage("Route not found.", SystemMessageTypes.Danger);
            return false;
        }

        // Check if route is already discovered
        if (route.IsDiscovered)
        {
            _messageSystem.AddSystemMessage("You already know that route.", SystemMessageTypes.Warning);
            return false;
        }

        // Check all requirements
        Player player = _gameWorld.GetPlayer();

        // Discover the route
        route.IsDiscovered = true;

        // Also register in information discovery system
        string infoId = $"route_{routeId}";
        _informationManager.DiscoverInformation(infoId);

        // Show route details
        _messageSystem.AddSystemMessage($"🗺️ Route Discovered: {route.Name}!", SystemMessageTypes.Success);
        _messageSystem.AddSystemMessage($"📍 {route.Origin} → {route.Destination}", SystemMessageTypes.Info);
        _messageSystem.AddSystemMessage($"⏱️ {route.TravelTimeMinutes} hours, 💪 {route.BaseStaminaCost} stamina", SystemMessageTypes.Info);

        return true;
    }

    /// <summary>
    /// Get a summary of discoverable routes at current location
    /// </summary>
    public string GetDiscoverySummary(string locationId)
    {
        List<RouteDiscoveryOption> availableDiscoveries = GetAvailableDiscoveries(locationId);

        if (!availableDiscoveries.Any())
        {
            return "No new routes to discover at this location.";
        }

        int discoverableCount = availableDiscoveries.Count(d => d.MeetsRequirements.MeetsAllRequirements && d.CanAfford);
        int totalCount = availableDiscoveries.Count;

        return $"Routes to discover: {discoverableCount}/{totalCount} available";
    }

    /// <summary>
    /// Check for newly available route discoveries and notify player
    /// </summary>
    public void CheckForNewDiscoveries(string locationId)
    {
        List<RouteDiscoveryOption> availableDiscoveries = GetAvailableDiscoveries(locationId);
        List<RouteDiscoveryOption> discoverableNow = availableDiscoveries
                .Where(d => d.MeetsRequirements.MeetsAllRequirements && d.CanAfford)
                .ToList();

        if (discoverableNow.Any())
        {
            _messageSystem.AddSystemMessage(
                $"💡 You can discover {discoverableNow.Count} new route(s) from locals!",
                SystemMessageTypes.Info
            );

            foreach (RouteDiscoveryOption? discovery in discoverableNow)
            {
                ConnectionType tokenType = DetermineTokenTypeForRoute(discovery.Route, discovery.Discovery, discovery.TeachingNPC);
                _messageSystem.AddSystemMessage(
                    $"  📍 {discovery.Route.Name} from {discovery.TeachingNPC.Name} " +
                    $"({discovery.Discovery.RequiredTokensWithNPC} {tokenType} tokens)",
                    SystemMessageTypes.Info
                );
            }
        }
    }

    /// <summary>
    /// Determine which token type should be used based on the route context
    /// </summary>
    public ConnectionType DetermineTokenTypeForRoute(RouteOption route, RouteDiscovery discovery, NPC npc)
    {
        // Check route name and description for context clues
        string routeContext = $"{route.Name} {route.Description}".ToLower();

        // Trade routes use Trade tokens
        if (routeContext.Contains("trade") || routeContext.Contains("merchant") ||
            routeContext.Contains("commercial") || routeContext.Contains("toll"))
        {
            return ConnectionType.Commerce;
        }

        // Noble/estate routes use Noble tokens
        if (routeContext.Contains("noble") || routeContext.Contains("estate") ||
            routeContext.Contains("court") || routeContext.Contains("palace"))
        {
            return ConnectionType.Status;
        }

        // Shadow/illegal routes use Shadow tokens
        if (routeContext.Contains("shadow") || routeContext.Contains("smuggl") ||
            routeContext.Contains("hidden") || routeContext.Contains("secret"))
        {
            return ConnectionType.Shadow;
        }

        // Personal/trust routes use Trust tokens
        if (routeContext.Contains("personal") || routeContext.Contains("private") ||
            routeContext.Contains("trust") || routeContext.Contains("friend"))
        {
            return ConnectionType.Trust;
        }

        // Mountain/forest/common routes use Common tokens
        return ConnectionType.Trust;
    }
}

/// <summary>
/// Represents a route that can be discovered from an NPC
/// </summary>
public class RouteDiscoveryOption
{
    public RouteDiscovery Discovery { get; set; }
    public RouteOption Route { get; set; }
    public NPC TeachingNPC { get; set; }
    public RouteDiscoveryRequirements MeetsRequirements { get; set; }
    public int PlayerTokensWithNPC { get; set; }
    public bool CanAfford { get; set; }
}

/// <summary>
/// Requirements check result for route discovery
/// </summary>
public class RouteDiscoveryRequirements
{
    public bool HasEnoughTrust { get; set; }
    public int TrustLevel { get; set; }
    public bool HasRequiredEquipment { get; set; }
    public List<string> MissingEquipment { get; set; } = new List<string>();
    public bool HasRequiredObligation { get; set; }
    public string? RequiredObligation { get; set; }
    public bool MeetsAllRequirements { get; set; }
}
