using Microsoft.AspNetCore.Components;
using Wayfarer.GameState.Constants;
using Wayfarer.Services;

namespace Wayfarer.Pages
{

    public class TravelSelectionBase : ComponentBase
    {
        [Inject] public GameFacade GameFacade { get; set; }
        [Inject] public TimeImpactCalculator TimeCalculator { get; set; }
        // ActionExecutionService removed - using GameFacade for all actions
        [Parameter] public Location CurrentLocation { get; set; }
        [Parameter] public List<Location> Locations { get; set; }
        [Parameter] public EventCallback<string> OnTravel { get; set; }
        [Parameter] public EventCallback<RouteOption> OnTravelRoute { get; set; }
        [Parameter] public EventCallback<CurrentViews> OnNavigate { get; set; }

        protected RouteOption GetRouteOption(TravelRouteViewModel viewModel)
        {
            if (viewModel == null) return null;

            return new RouteOption
            {
                Id = viewModel.RouteId,
                Name = viewModel.RouteName,
                Method = viewModel.TransportMethod,
                BaseStaminaCost = viewModel.BaseStaminaCost,
                BaseCoinCost = viewModel.CoinCost,
                TravelTimeHours = viewModel.TimeCost,
                TerrainCategories = viewModel.TerrainCategories ?? new List<TerrainCategory>(),
                Description = viewModel.Description
            };
        }

        protected Player GetPlayer()
        {
            return GameFacade.GetPlayer();
        }


        protected async Task HandleTravelRoute(TravelRouteViewModel routeViewModel)
        {
            // Check for deadline impacts using the route's actual time cost
            if (routeViewModel.TimeCost > 0)
            {
                TimeImpactInfo timeImpact = TimeCalculator.CalculateTimeImpact(routeViewModel.TimeCost);
                if (timeImpact?.LettersExpiring > 0)
                {
                    // Show warning modal
                    PendingRoute = routeViewModel;
                    PendingTimeImpact = timeImpact;
                    ShowDeadlineWarning = true;
                    StateHasChanged();
                    return;
                }
            }

            // No warning needed, proceed with travel using the route's defined transport method
            RouteOption routeOption = GetRouteOption(routeViewModel);
            if (OnTravelRoute.HasDelegate)
            {
                await OnTravelRoute.InvokeAsync(routeOption);
            }
        }
        
        protected async Task HandleBackToLocation()
        {
            Console.WriteLine("[TravelSelection] HandleBackToLocation - returning to LocationScreen without travel");
            if (OnNavigate.HasDelegate)
            {
                await OnNavigate.InvokeAsync(CurrentViews.LocationScreen);
            }
        }


        protected TravelContextViewModel TravelContext { get; set; }
        protected List<TravelDestinationViewModel> Destinations { get; set; }

        // Deadline warning state
        protected bool ShowDeadlineWarning = false;
        protected TimeImpactInfo PendingTimeImpact = null;
        protected TravelRouteViewModel PendingRoute = null;

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
            TravelDestinationViewModel? destination = Destinations?.FirstOrDefault(d => d.LocationId == locationId);
            return destination?.Routes ?? new List<TravelRouteViewModel>();
        }

        /// <summary>
        /// Get token requirements for a route
        /// </summary>
        public Dictionary<string, (int required, int current, string displayName)> GetRouteTokenRequirements(TravelRouteViewModel route)
        {
            Dictionary<string, (int, int, string)> requirements = new Dictionary<string, (int, int, string)>();

            if (route.TokenRequirements == null || !route.TokenRequirements.Any())
                return requirements;

            foreach ((string key, RouteTokenRequirementViewModel req) in route.TokenRequirements)
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
                ConnectionType.Shadow => "🌑",
                _ => "🎭"
            };
        }

        /// <summary>
        /// Get icon for transport method
        /// </summary>
        public string GetTransportIcon(TravelMethods method)
        {
            return method switch
            {
                TravelMethods.Walking => "🚶",
                TravelMethods.Horseback => "🐎",
                TravelMethods.Cart => "🛒",
                TravelMethods.Carriage => "🚌",
                TravelMethods.Boat => "⛵",
                _ => "❓"
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

        /// <summary>
        /// Called when user confirms travel despite deadline warning
        /// </summary>
        protected async Task ConfirmTravelWithDeadline()
        {
            ShowDeadlineWarning = false;
            StateHasChanged();

            if (PendingRoute != null)
            {
                RouteOption routeOption = GetRouteOption(PendingRoute);
                if (OnTravelRoute.HasDelegate)
                {
                    await OnTravelRoute.InvokeAsync(routeOption);
                }
                PendingRoute = null;
                PendingTimeImpact = null;
            }
        }

        /// <summary>
        /// Called when user cancels travel due to deadline warning
        /// </summary>
        protected void CancelTravelWithDeadline()
        {
            ShowDeadlineWarning = false;
            PendingRoute = null;
            PendingTimeImpact = null;
            StateHasChanged();
        }

    }

} // namespace Wayfarer.Pages
