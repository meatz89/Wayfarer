using Microsoft.AspNetCore.Components;
using Wayfarer.GameState.Constants;

namespace Wayfarer.Pages
{

    public class TravelSelectionBase : ComponentBase
    {
        [Inject] public GameWorld GameWorld { get; set; }
        [Inject] public GameWorldManager GameWorldManager { get; set; }
        [Inject] public RouteRepository RouteRepository { get; set; }
        // ItemRepository allowed for read-only UI data binding
        [Inject] public ItemRepository ItemRepository { get; set; }
        [Inject] public ConnectionTokenManager TokenManager { get; set; }
        [Inject] public NPCRepository NPCRepository { get; set; }
        [Parameter] public Location CurrentLocation { get; set; }
        [Parameter] public List<Location> Locations { get; set; }
        [Parameter] public EventCallback<string> OnTravel { get; set; }
        [Parameter] public EventCallback<RouteOption> OnTravelRoute { get; set; }
        [Parameter] public EventCallback<(RouteOption route, TravelMethods transport)> OnTravelWithTransport { get; set; }

        public bool ShowEquipmentCategories => GameWorld.GetPlayer().Inventory.ItemSlots
            .Select(name => ItemRepository.GetItemByName(name))
            .Any(item => item != null && item.Categories.Any());

        public WeatherCondition CurrentWeather => GameWorld.CurrentWeather;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public List<Location> GetTravelableLocations()
        {
            return Locations?.Where(loc => loc.Id != CurrentLocation?.Id &&
                                  GameWorldManager.CanTravelTo(loc.Id)).ToList() ?? new List<Location>();
        }


        public void ShowRouteOptions(GameWorld gameWorld, string destinationName, string destinationId)
        {
            List<RouteOption> availableRoutes = GameWorldManager.GetAvailableRoutes(CurrentLocation.Id, destinationId);

            Console.WriteLine($"=== Travel to {destinationName} ===");

            if (availableRoutes.Count == 0)
            {
                Console.WriteLine("No routes available at this time.");
                return;
            }

            int totalWeight = GameWorldManager.CalculateTotalWeight();
            string weightStatus = totalWeight <= GameConstants.LoadWeight.LIGHT_LOAD_MAX ? "Light load" :
                                    (totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX ? "Medium load (+1 stamina cost)" : "Heavy load (+2 stamina cost)");

            Console.WriteLine($"Current load: {weightStatus} ({totalWeight} weight units)");
            Console.WriteLine();

            Console.WriteLine("How do you want to travel?");

            for (int i = 0; i < availableRoutes.Count; i++)
            {
                RouteOption route = availableRoutes[i];

                // Calculate adjusted stamina cost
                int adjustedStaminaCost = route.BaseStaminaCost;
                if (totalWeight > GameConstants.LoadWeight.LIGHT_LOAD_MAX && totalWeight <= GameConstants.LoadWeight.MEDIUM_LOAD_MAX)
                {
                    adjustedStaminaCost += GameConstants.LoadWeight.MEDIUM_LOAD_STAMINA_PENALTY;
                }
                else if (totalWeight > GameConstants.LoadWeight.MEDIUM_LOAD_MAX)
                {
                    adjustedStaminaCost += GameConstants.LoadWeight.HEAVY_LOAD_STAMINA_PENALTY;
                }

                string staminaCostDisplay = route.BaseStaminaCost == adjustedStaminaCost ?
                    $"{adjustedStaminaCost} stamina" :
                    $"{adjustedStaminaCost} stamina (base {route.BaseStaminaCost} + weight penalty)";

                string departureInfo = route.DepartureTime.HasValue ? $", departs at {route.DepartureTime}" : "";
                string affordabilityMarker = GameWorldManager.CanTravelRoute(route) ? "" : " (Cannot afford)";

                Console.WriteLine($"[{i + 1}] {route.Name} - {route.BaseCoinCost} coins, {staminaCostDisplay}{departureInfo}{affordabilityMarker}");

                // Show terrain categories if any
                if (route.TerrainCategories.Count > 0)
                {
                    Console.WriteLine($"    Terrain: {string.Join(", ", route.TerrainCategories.Select(c => c.ToString().Replace('_', ' ')))}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Current resources: {gameWorld.PlayerCoins} coins, {gameWorld.PlayerStamina} stamina");

            // Show equipment categories
            List<string> equipmentItems = new List<string>();
            foreach (string itemName in gameWorld.PlayerInventory.ItemSlots)
            {
                if (itemName != null)
                {
                    Item item = ItemRepository.GetItemByName(itemName);
                    if (item != null && item.Categories.Count > 0)
                    {
                        equipmentItems.Add($"{itemName} ({string.Join(", ", item.Categories.Select(c => c.ToString().Replace('_', ' ')))})");
                    }
                }
            }

            if (equipmentItems.Count > 0)
            {
                Console.WriteLine("Equipment categories:");
                foreach (string itemInfo in equipmentItems)
                {
                    Console.WriteLine($"- {itemInfo}");
                }
            }
        }


        /// <summary>
        /// Get equipment categories that are absolutely required for terrain types
        /// </summary>
        public List<ItemCategory> GetRequiredEquipment(List<TerrainCategory> terrainCategories)
        {
            List<ItemCategory> required = new List<ItemCategory>();

            foreach (TerrainCategory terrain in terrainCategories)
            {
                switch (terrain)
                {
                    case TerrainCategory.Requires_Climbing:
                        required.Add(ItemCategory.Climbing_Equipment);
                        break;
                    case TerrainCategory.Requires_Water_Transport:
                        required.Add(ItemCategory.Water_Transport);
                        break;
                    case TerrainCategory.Requires_Permission:
                        required.Add(ItemCategory.Special_Access);
                        break;
                }
            }

            return required.Distinct().ToList();
        }

        /// <summary>
        /// Get equipment categories that are recommended for terrain types
        /// </summary>
        public List<ItemCategory> GetRecommendedEquipment(List<TerrainCategory> terrainCategories)
        {
            List<ItemCategory> recommended = new List<ItemCategory>();

            foreach (TerrainCategory terrain in terrainCategories)
            {
                switch (terrain)
                {
                    case TerrainCategory.Wilderness_Terrain:
                        recommended.Add(ItemCategory.Navigation_Tools);
                        break;
                    case TerrainCategory.Exposed_Weather:
                        recommended.Add(ItemCategory.Weather_Protection);
                        break;
                    case TerrainCategory.Heavy_Cargo_Route:
                        recommended.Add(ItemCategory.Load_Distribution);
                        break;
                    case TerrainCategory.Dark_Passage:
                        recommended.Add(ItemCategory.Light_Source);
                        break;
                }
            }

            return recommended.Distinct().ToList();
        }

        public List<string> GetWeatherTerrainEffects(List<TerrainCategory> terrainCategories, WeatherCondition weather)
        {
            List<string> effects = new List<string>();

            foreach (TerrainCategory terrain in terrainCategories)
            {
                switch (weather)
                {
                    case WeatherCondition.Rain:
                        if (terrain == TerrainCategory.Exposed_Weather)
                            effects.Add("☔ Exposed terrain unsafe in rain - requires weather protection");
                        break;

                    case WeatherCondition.Snow:
                        if (terrain == TerrainCategory.Exposed_Weather)
                            effects.Add("❄️ Exposed terrain impassable in snow - requires weather protection");
                        if (terrain == TerrainCategory.Wilderness_Terrain)
                            effects.Add("🌨️ Wilderness routes dangerous in snow - requires navigation tools");
                        break;

                    case WeatherCondition.Fog:
                        if (terrain == TerrainCategory.Wilderness_Terrain)
                            effects.Add("🌫️ Cannot navigate wilderness in fog - requires navigation tools");
                        break;

                    case WeatherCondition.Clear:
                        // No special effects for clear weather
                        break;
                }
            }

            return effects.Distinct().ToList();
        }

        public string GetWeatherIcon(WeatherCondition weather)
        {
            return weather switch
            {
                WeatherCondition.Clear => "☀️",
                WeatherCondition.Rain => "🌧️",
                WeatherCondition.Snow => "❄️",
                WeatherCondition.Fog => "🌫️",
                _ => "❓"
            };
        }

        /// <summary>
        /// Get terrain icon for display
        /// </summary>
        public string GetTerrainIcon(TerrainCategory terrain)
        {
            return terrain switch
            {
                TerrainCategory.Requires_Climbing => "🧗",
                TerrainCategory.Exposed_Weather => "🌡️",
                TerrainCategory.Wilderness_Terrain => "🌲",
                TerrainCategory.Requires_Water_Transport => "🌊",
                TerrainCategory.Requires_Permission => "🔐",
                TerrainCategory.Heavy_Cargo_Route => "📦",
                TerrainCategory.Dark_Passage => "🌑",
                _ => "🛤️"
            };
        }

        /// <summary>
        /// Get all equipment categories currently owned by the player
        /// </summary>
        public List<ItemCategory> GetCurrentEquipmentCategories()
        {
            List<ItemCategory> ownedCategories = new List<ItemCategory>();

            foreach (string itemName in GameWorld.GetPlayer().Inventory.ItemSlots)
            {
                if (itemName != null)
                {
                    Item item = ItemRepository.GetItemByName(itemName);
                    if (item != null)
                    {
                        ownedCategories.AddRange(item.Categories);
                    }
                }
            }

            return ownedCategories.Distinct().ToList();
        }

        /// <summary>
        /// Get all routes to a destination, including locked ones
        /// </summary>
        public List<RouteOption> GetAllRoutesToLocation(string locationId)
        {
            // Get all routes from current location
            List<RouteOption> allRoutes = RouteRepository.GetRoutesFromLocation(CurrentLocation.Id);

            // Filter to only routes going to the specified destination
            return allRoutes.Where(r => r.Destination == locationId).ToList();
        }

        /// <summary>
        /// Get token requirements for a route
        /// </summary>
        public Dictionary<string, (int required, int current, string displayName)> GetRouteTokenRequirements(RouteOption route)
        {
            var requirements = new Dictionary<string, (int, int, string)>();
            
            if (route.AccessRequirement == null)
                return requirements;

            // Check type-based requirements
            foreach (var (tokenType, requiredCount) in route.AccessRequirement.RequiredTokensPerType)
            {
                int currentCount = TokenManager.GetTotalTokensOfType(tokenType);
                string displayName = $"{tokenType} tokens (any NPC)";
                requirements[$"type_{tokenType}"] = (requiredCount, currentCount, displayName);
            }

            // Check NPC-specific requirements
            foreach (var (npcId, requiredCount) in route.AccessRequirement.RequiredTokensPerNPC)
            {
                var npcTokens = TokenManager.GetTokensWithNPC(npcId);
                int currentCount = npcTokens.Values.Sum();
                var npc = NPCRepository.GetById(npcId);
                string displayName = $"tokens with {npc?.Name ?? npcId}";
                requirements[$"npc_{npcId}"] = (requiredCount, currentCount, displayName);
            }

            return requirements;
        }

        /// <summary>
        /// Get icon for token type
        /// </summary>
        public string GetTokenIcon(ConnectionType tokenType)
        {
            return tokenType switch
            {
                ConnectionType.Trust => "💝",
                ConnectionType.Trade => "🤝",
                ConnectionType.Noble => "👑",
                ConnectionType.Common => "🏘️",
                ConnectionType.Shadow => "🌑",
                _ => "🎭"
            };
        }

        /// <summary>
        /// Check if player has required tokens for a route
        /// </summary>
        public bool HasRequiredTokens(RouteOption route)
        {
            if (route.AccessRequirement == null)
                return true;

            var tokenReqs = GetRouteTokenRequirements(route);
            return tokenReqs.All(kvp => kvp.Value.current >= kvp.Value.required);
        }

        /// <summary>
        /// Get formatted token requirement display
        /// </summary>
        public string GetTokenRequirementDisplay(int required, int current)
        {
            if (current >= required)
                return $"{required}";
            else
                return $"{required} (have {current})";
        }

    }

} // namespace Wayfarer.Pages
