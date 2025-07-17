using Wayfarer.Game.MainSystem;
using Wayfarer.GameState;

namespace Wayfarer.GameState
{
    /// <summary>
    /// Manages route unlocking through connection token spending at NPC locations.
    /// Routes can be unlocked by spending tokens with specific NPCs who know the routes.
    /// </summary>
    public class RouteUnlockManager
    {
        private readonly GameWorld _gameWorld;
        private readonly ConnectionTokenManager _connectionTokenManager;
        private readonly NPCRepository _npcRepository;
        private readonly RouteRepository _routeRepository;
        private readonly MessageSystem _messageSystem;

        public RouteUnlockManager(
            GameWorld gameWorld,
            ConnectionTokenManager connectionTokenManager,
            NPCRepository npcRepository,
            RouteRepository routeRepository,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld;
            _connectionTokenManager = connectionTokenManager;
            _npcRepository = npcRepository;
            _routeRepository = routeRepository;
            _messageSystem = messageSystem;
        }

        /// <summary>
        /// Get routes that can be unlocked at the current location
        /// </summary>
        public List<RouteUnlockOption> GetAvailableRouteUnlocks(string locationId)
        {
            var unlockOptions = new List<RouteUnlockOption>();
            
            // Get all NPCs at current location
            var availableNPCs = _npcRepository.GetNPCsForLocation(locationId);
            
            foreach (var npc in availableNPCs)
            {
                // Check if NPC can unlock routes (based on their role/profession)
                var unlockedRoutes = GetRouteUnlocksForNPC(npc);
                unlockOptions.AddRange(unlockedRoutes);
            }

            return unlockOptions;
        }

        /// <summary>
        /// Get route unlocks available from a specific NPC
        /// </summary>
        private List<RouteUnlockOption> GetRouteUnlocksForNPC(NPC npc)
        {
            var unlockOptions = new List<RouteUnlockOption>();
            
            // Get all routes from NPC's location
            var routes = _routeRepository.GetRoutesFromLocation(npc.Location);
            
            foreach (var route in routes)
            {
                // Only show routes that are not yet discovered
                if (!route.IsDiscovered)
                {
                    var unlockCost = CalculateRouteUnlockCost(route, npc);
                    if (unlockCost != null)
                    {
                        unlockOptions.Add(new RouteUnlockOption
                        {
                            RouteId = route.Id,
                            RouteName = route.Name,
                            RouteDescription = route.Description,
                            UnlockingNPC = npc,
                            TokenCost = unlockCost,
                            CanAfford = _connectionTokenManager.HasTokens(unlockCost.TokenType, unlockCost.Amount)
                        });
                    }
                }
            }

            return unlockOptions;
        }

        /// <summary>
        /// Calculate the token cost to unlock a route from a specific NPC
        /// </summary>
        private TokenCost? CalculateRouteUnlockCost(RouteOption route, NPC npc)
        {
            // Route unlock cost depends on route efficiency and NPC profession
            var baseCost = CalculateBaseCost(route);
            var tokenType = GetTokenTypeForNPC(npc);

            if (tokenType == null) return null;

            return new TokenCost
            {
                TokenType = tokenType.Value,
                Amount = baseCost
            };
        }

        /// <summary>
        /// Calculate base cost for route unlock based on route efficiency
        /// </summary>
        private int CalculateBaseCost(RouteOption route)
        {
            // Better routes (faster, less stamina) cost more tokens
            var timeSaving = Math.Max(0, 3 - route.TimeBlockCost); // Assume 3 is standard
            var staminaSaving = Math.Max(0, 3 - route.BaseStaminaCost);
            
            return Math.Max(1, timeSaving + staminaSaving); // Minimum cost of 1 token
        }

        /// <summary>
        /// Get the appropriate token type for an NPC's profession
        /// </summary>
        private ConnectionType? GetTokenTypeForNPC(NPC npc)
        {
            return npc.Profession switch
            {
                Professions.Merchant => ConnectionType.Trade,
                Professions.Courtier => ConnectionType.Noble,
                Professions.Ranger => ConnectionType.Common,
                Professions.Scholar => ConnectionType.Common,
                Professions.Thief => ConnectionType.Shadow,
                Professions.Soldier => ConnectionType.Common,
                _ => ConnectionType.Common // Default to Common tokens
            };
        }

        /// <summary>
        /// Attempt to unlock a route by spending tokens with an NPC
        /// </summary>
        public bool TryUnlockRoute(string routeId, string npcId)
        {
            var npc = _npcRepository.GetNPCById(npcId);
            if (npc == null)
            {
                _messageSystem.AddSystemMessage("NPC not found.", SystemMessageTypes.Danger);
                return false;
            }

            // Find the route
            var route = FindRouteById(routeId);
            if (route == null)
            {
                _messageSystem.AddSystemMessage("Route not found.", SystemMessageTypes.Danger);
                return false;
            }

            // Check if route is already unlocked
            if (route.IsDiscovered)
            {
                _messageSystem.AddSystemMessage("Route is already unlocked.", SystemMessageTypes.Warning);
                return false;
            }

            // Calculate unlock cost
            var unlockCost = CalculateRouteUnlockCost(route, npc);
            if (unlockCost == null)
            {
                _messageSystem.AddSystemMessage($"{npc.Name} cannot help unlock this route.", SystemMessageTypes.Danger);
                return false;
            }

            // Check if player has enough tokens
            if (!_connectionTokenManager.HasTokens(unlockCost.TokenType, unlockCost.Amount))
            {
                _messageSystem.AddSystemMessage($"Need {unlockCost.Amount} {unlockCost.TokenType} tokens to unlock {route.Name}.", SystemMessageTypes.Danger);
                return false;
            }

            // Spend tokens and unlock route
            _connectionTokenManager.SpendTokens(unlockCost.TokenType, unlockCost.Amount);
            route.IsDiscovered = true;

            // Enhanced success feedback with route details
            _messageSystem.AddSystemMessage($"🗺️ Route Unlocked: {route.Name}!", SystemMessageTypes.Success);
            _messageSystem.AddSystemMessage($"📍 {route.Origin} → {route.Destination}", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage($"⏱️ {route.TimeBlockCost} time blocks, 💪 {route.BaseStaminaCost} stamina", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage($"💰 Cost: {unlockCost.Amount} {unlockCost.TokenType} tokens", SystemMessageTypes.Info);
            
            return true;
        }

        /// <summary>
        /// Find a route by ID across all locations
        /// </summary>
        private RouteOption? FindRouteById(string routeId)
        {
            var allRoutes = _routeRepository.GetAllRoutes();
            return allRoutes.FirstOrDefault(r => r.Id == routeId);
        }

        /// <summary>
        /// Get all unlocked routes from a location
        /// </summary>
        public List<RouteOption> GetUnlockedRoutes(string fromLocationId, string toLocationId)
        {
            var routes = _routeRepository.GetRoutesFromLocation(fromLocationId);
            return routes.Where(r => r.IsDiscovered && r.Destination == toLocationId).ToList();
        }

        /// <summary>
        /// Check if player can afford any route unlocks at current location and provide feedback
        /// </summary>
        public void CheckForAffordableRouteUnlocks(string locationId)
        {
            var availableUnlocks = GetAvailableRouteUnlocks(locationId);
            var affordableUnlocks = availableUnlocks.Where(u => u.CanAfford).ToList();
            
            if (affordableUnlocks.Any())
            {
                _messageSystem.AddSystemMessage($"💡 You can now afford {affordableUnlocks.Count} route unlock(s) at this location!", SystemMessageTypes.Info);
                foreach (var unlock in affordableUnlocks)
                {
                    _messageSystem.AddSystemMessage($"  📍 {unlock.RouteName} ({unlock.TokenCost.Amount} {unlock.TokenCost.TokenType} tokens)", SystemMessageTypes.Info);
                }
            }
        }

        /// <summary>
        /// Get a summary of all locked routes that player could potentially unlock
        /// </summary>
        public string GetRouteUnlockSummary(string locationId)
        {
            var availableUnlocks = GetAvailableRouteUnlocks(locationId);
            
            if (!availableUnlocks.Any())
            {
                return "No route unlocks available at this location.";
            }
            
            var affordableCount = availableUnlocks.Count(u => u.CanAfford);
            var totalCount = availableUnlocks.Count;
            
            return $"Route unlocks available: {affordableCount}/{totalCount} affordable";
        }
    }

    /// <summary>
    /// Represents a route that can be unlocked by spending tokens with an NPC
    /// </summary>
    public class RouteUnlockOption
    {
        public string RouteId { get; set; }
        public string RouteName { get; set; }
        public string RouteDescription { get; set; }
        public NPC UnlockingNPC { get; set; }
        public TokenCost TokenCost { get; set; }
        public bool CanAfford { get; set; }
    }

    /// <summary>
    /// Represents the token cost for unlocking a route
    /// </summary>
    public class TokenCost
    {
        public ConnectionType TokenType { get; set; }
        public int Amount { get; set; }
    }
}