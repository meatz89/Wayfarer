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
        [Parameter] public EventCallback<(RouteOption route, TravelMethods transport)> OnTravelWithTransport { get; set; }
        
        // Transport selection state
        protected bool ShowTransportSelector { get; set; }
        protected TravelRouteViewModel SelectedRoute { get; set; }
        protected TravelMethods SelectedTransport { get; set; } = TravelMethods.Walking;
        
        protected void ShowTransportOptions(TravelRouteViewModel route)
        {
            SelectedRoute = route;
            ShowTransportSelector = true;
            StateHasChanged();
        }
        
        protected void CloseTransportSelector()
        {
            ShowTransportSelector = false;
            StateHasChanged();
        }
        
        protected void OnTransportSelected(TravelMethods transport)
        {
            SelectedTransport = transport;
            StateHasChanged();
        }
        
        protected async Task ConfirmTransportSelection()
        {
            if (SelectedRoute != null)
            {
                ShowTransportSelector = false;
                await HandleTravelRouteWithTransport(SelectedRoute, SelectedTransport);
            }
        }
        
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
        
        protected async Task HandleTravelRouteWithTransport(TravelRouteViewModel routeViewModel, TravelMethods transport)
        {
            // Check for deadline impacts
            var adjustedTimeCost = CalculateAdjustedTimeCost(routeViewModel.TimeCost, transport);
            if (adjustedTimeCost > 0)
            {
                var timeImpact = TimeCalculator.CalculateTimeImpact(adjustedTimeCost);
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
            
            // No warning needed, proceed with travel
            await ExecuteTravelWithTransport(routeViewModel, transport);
        }
        
        private int CalculateAdjustedTimeCost(int baseTime, TravelMethods transport)
        {
            var multiplier = transport switch
            {
                TravelMethods.Walking => 1.0,
                TravelMethods.Horseback => 0.5,
                TravelMethods.Cart => 0.8,
                TravelMethods.Carriage => 0.4,
                TravelMethods.Boat => 0.6,
                _ => 1.0
            };
            
            return (int)Math.Ceiling(baseTime * multiplier);
        }
        
        protected async Task HandleTravelRoute(TravelRouteViewModel routeViewModel)
        {
            // Check for deadline impacts
            if (routeViewModel.TimeCost > 0)
            {
                var timeImpact = TimeCalculator.CalculateTimeImpact(routeViewModel.TimeCost);
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
            
            // No warning needed, proceed with travel
            await ExecuteTravel(routeViewModel);
        }
        
        private async Task ExecuteTravelWithTransport(TravelRouteViewModel routeViewModel, TravelMethods transport)
        {
            var routeOption = GetRouteOption(routeViewModel);
            
            // Fire the event with transport selection
            if (OnTravelWithTransport.HasDelegate)
            {
                await OnTravelWithTransport.InvokeAsync((routeOption, transport));
            }
            else if (OnTravelRoute.HasDelegate)
            {
                // Fallback to regular route travel
                await OnTravelRoute.InvokeAsync(routeOption);
            }
        }
        
        private async Task ExecuteTravel(TravelRouteViewModel routeViewModel)
        {
            // Default to walking if no transport selected
            await ExecuteTravelWithTransport(routeViewModel, TravelMethods.Walking);
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
        
        /// <summary>
        /// Called when user confirms travel despite deadline warning
        /// </summary>
        protected async Task ConfirmTravelWithDeadline()
        {
            ShowDeadlineWarning = false;
            StateHasChanged();
            
            if (PendingRoute != null)
            {
                await ExecuteTravel(PendingRoute);
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
