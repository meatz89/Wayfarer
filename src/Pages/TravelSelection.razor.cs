using Microsoft.AspNetCore.Components;
using Wayfarer.GameState.Constants;

namespace Wayfarer.Pages
{

    public class TravelSelectionBase : ComponentBase
    {
        [Inject] public IGameFacade GameFacade { get; set; }
        [Parameter] public Location CurrentLocation { get; set; }
        [Parameter] public List<Location> Locations { get; set; }
        [Parameter] public EventCallback<string> OnTravel { get; set; }
        [Parameter] public EventCallback<RouteOption> OnTravelRoute { get; set; }
        [Parameter] public EventCallback<(RouteOption route, TravelMethods transport)> OnTravelWithTransport { get; set; }
        
        protected async Task HandleTravelRoute(TravelRouteViewModel routeViewModel)
        {
            // Convert TravelRouteViewModel to RouteOption for backward compatibility
            var routeOption = new RouteOption
            {
                Id = routeViewModel.RouteId,
                Name = routeViewModel.RouteName,
                Method = routeViewModel.TransportMethod,
                BaseStaminaCost = routeViewModel.BaseStaminaCost,
                BaseCoinCost = routeViewModel.CoinCost,
                TravelTimeHours = routeViewModel.TimeCost,
                Description = routeViewModel.Description,
                Destination = Destinations.FirstOrDefault(d => d.Routes.Contains(routeViewModel))?.LocationId ?? "",
                TerrainCategories = routeViewModel.TerrainCategories ?? new List<TerrainCategory>(),
                DepartureTime = routeViewModel.DepartureTime,
                IsDiscovered = routeViewModel.IsDiscovered
            };
            
            await OnTravelRoute.InvokeAsync(routeOption);
        }

        protected TravelContextViewModel TravelContext { get; set; }
        protected List<TravelDestinationViewModel> Destinations { get; set; }

        public bool ShowEquipmentCategories => TravelContext?.CurrentEquipmentCategories?.Any() ?? false;

        public WeatherCondition CurrentWeather => TravelContext?.CurrentWeather ?? WeatherCondition.Clear;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            RefreshTravelData();
        }
        
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            RefreshTravelData();
        }
        
        private void RefreshTravelData()
        {
            TravelContext = GameFacade.GetTravelContext();
            Destinations = GameFacade.GetTravelDestinationsWithRoutes();
        }

        public List<TravelDestinationViewModel> GetTravelableLocations()
        {
            return Destinations?.Where(d => d.LocationId != CurrentLocation?.Id && d.CanTravel).ToList() 
                ?? new List<TravelDestinationViewModel>();
        }


        // Removed ShowRouteOptions method - no longer needed with ViewModels


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
            return TravelContext?.CurrentEquipmentCategories ?? new List<ItemCategory>();
        }

        /// <summary>
        /// Get all routes to a destination, including locked ones
        /// </summary>
        public List<TravelRouteViewModel> GetAllRoutesToLocation(string locationId)
        {
            var destination = Destinations?.FirstOrDefault(d => d.LocationId == locationId);
            return destination?.Routes ?? new List<TravelRouteViewModel>();
        }

        /// <summary>
        /// Get token requirements for a route
        /// </summary>
        public Dictionary<string, (int required, int current, string displayName)> GetRouteTokenRequirements(TravelRouteViewModel route)
        {
            var requirements = new Dictionary<string, (int, int, string)>();
            
            if (route.TokenRequirements == null || !route.TokenRequirements.Any())
                return requirements;

            foreach (var (key, req) in route.TokenRequirements)
            {
                requirements[key] = (req.RequiredAmount, req.CurrentAmount, req.DisplayName);
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
                ConnectionType.Commerce => "🤝",
                ConnectionType.Status => "👑",
                ConnectionType.Trust => "🏘️",
                ConnectionType.Shadow => "🌑",
                _ => "🎭"
            };
        }

        /// <summary>
        /// Check if player has required tokens for a route
        /// </summary>
        public bool HasRequiredTokens(TravelRouteViewModel route)
        {
            if (route.TokenRequirements == null || !route.TokenRequirements.Any())
                return true;

            return route.TokenRequirements.All(kvp => kvp.Value.IsMet);
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
