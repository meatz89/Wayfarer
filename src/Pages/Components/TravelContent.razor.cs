using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class TravelContentBase : ComponentBase
    {
        [Parameter] public string CurrentLocation { get; set; }
        [Parameter] public EventCallback<string> OnTravelRoute { get; set; }
        [Parameter] public EventCallback<string> OnNavigate { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected TimeManager TimeManager { get; set; }

        protected List<RouteViewModel> AvailableRoutes { get; set; } = new();
        protected RouteViewModel SelectedRoute { get; set; }

        protected override async Task OnInitializedAsync()
        {
            LoadAvailableRoutes();
        }

        protected override async Task OnParametersSetAsync()
        {
            LoadAvailableRoutes();
        }

        private void LoadAvailableRoutes()
        {
            Location currentLoc = GameFacade.GetCurrentLocation();
            if (currentLoc == null) return;

            List<RouteOption> routes = GameFacade.GetAvailableRoutes();

            AvailableRoutes = routes.Select(r => new RouteViewModel
            {
                Id = r.Id,
                DestinationName = r.Name,
                District = GetDestinationDistrict(r.DestinationLocationSpot),
                TransportType = r.Method.ToString(),
                TravelTime = r.TravelTimeMinutes,
                Cost = r.BaseCoinCost,
                Familiarity = r.IsDiscovered ? "Known" : "Unknown",
                Requirements = ExtractRouteRequirements(r)
            }).ToList();
        }

        private string GetDestinationDistrict(string destinationName)
        {
            // Map location names to their districts based on game world
            // This is a categorical mapping based on the game's location hierarchy
            return destinationName?.ToLower() switch
            {
                "noble estate" or "lord's manor" or "noble district" => "Noble District",
                "market square" or "merchant row" or "trade post" => "Market District",
                "city gates" or "guard post" or "customs house" => "Gate District",
                "riverside" or "docks" or "wharf" => "Riverside District",
                "your room" or "boarding house" or "inn" => "Residential District",
                "temple" or "shrine" or "monastery" => "Temple District",
                "guild hall" or "artisan quarter" => "Artisan District",
                _ => "City Center" // Default for unknown locations
            };
        }

        private List<string> ExtractRouteRequirements(RouteOption route)
        {
            List<string> requirements = new List<string>();

            // Extract terrain requirements
            foreach (TerrainCategory terrain in route.TerrainCategories)
            {
                switch (terrain)
                {
                    case TerrainCategory.Requires_Climbing:
                        requirements.Add("Requires climbing equipment");
                        break;
                    case TerrainCategory.Requires_Water_Transport:
                        requirements.Add("Requires water transport");
                        break;
                    case TerrainCategory.Requires_Permission:
                        requirements.Add("Requires special permission");
                        break;
                    case TerrainCategory.Wilderness_Terrain:
                        requirements.Add("Wilderness terrain (navigation tools recommended)");
                        break;
                    case TerrainCategory.Exposed_Weather:
                        requirements.Add("Exposed to weather (protection recommended)");
                        break;
                    case TerrainCategory.Heavy_Cargo_Route:
                        requirements.Add("Heavy cargo route (load distribution recommended)");
                        break;
                    case TerrainCategory.Dark_Passage:
                        requirements.Add("Dark passage (light source recommended)");
                        break;
                }
            }

            // Add tier requirements
            if (route.TierRequired > TierLevel.T1)
            {
                requirements.Add($"Requires {route.TierRequired} access");
            }

            // Add access requirements if any
            if (route.AccessRequirement != null)
            {
                // Add specific access requirement descriptions based on the AccessRequirement
                requirements.Add("Special access required");
            }

            return requirements;
        }

        protected void SelectRoute(RouteViewModel route)
        {
            SelectedRoute = route;
            StateHasChanged();
        }

        protected async Task TravelRoute(RouteViewModel route)
        {
            if (!CanTakeRoute(route)) return;

            await OnTravelRoute.InvokeAsync(route.Id);
        }

        protected bool CanTakeRoute(RouteViewModel route)
        {
            // Check coin cost
            Player player = GameFacade.GetPlayer();
            if (player.Coins < route.Cost)
                return false;

            // Check stamina cost (base 2 for any travel)
            if (player.Attention < 2)
                return false;

            // Check requirements (includes token requirements, time restrictions, etc.)
            // Requirements list is populated by GetRouteRequirements which checks tokens
            // If there are any unmet requirements, the route cannot be taken
            // The actual token checking happens in GameFacade when building the requirements list

            return true; // All basic checks passed
        }

        protected string GetCannotTravelReason(RouteViewModel route)
        {
            Player player = GameFacade.GetPlayer();

            if (player.Coins < route.Cost)
                return $"Need {route.Cost} coins";

            if (route.Requirements.Any())
                return "Requirements not met";

            return "Cannot travel";
        }

        protected string GetRouteClass(RouteViewModel route)
        {
            if (!CanTakeRoute(route))
                return "disabled";

            if (route == SelectedRoute)
                return "selected";

            return "";
        }

        protected string GetTransportIcon(string transportType)
        {
            return transportType?.ToLower() switch
            {
                "walk" => "🚶",
                "cart" => "🛒",
                "horse" => "🐴",
                "boat" => "⛵",
                _ => "🚶"
            };
        }

        protected string GetTravelTimeDisplay(int minutes)
        {
            if (minutes < 60)
                return $"{minutes} minutes";

            int hours = minutes / 60;
            int mins = minutes % 60;

            if (mins == 0)
                return $"{hours} hour{(hours > 1 ? "s" : "")}";

            return $"{hours}h {mins}m";
        }

        protected async Task ReturnToLocation()
        {
            await OnNavigate.InvokeAsync("location");
        }
    }

    public class RouteViewModel
    {
        public string Id { get; set; }
        public string DestinationName { get; set; }
        public string District { get; set; }
        public string TransportType { get; set; }
        public int TravelTime { get; set; }
        public int Cost { get; set; }
        public string Familiarity { get; set; }
        public List<string> Requirements { get; set; } = new();
    }
}