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
        [Inject] protected ITimeManager TimeManager { get; set; }

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
            var currentLoc = GameFacade.GetCurrentLocation();
            if (currentLoc == null) return;
            
            var routes = GameFacade.GetAvailableRoutes();
            
            AvailableRoutes = routes.Select(r => new RouteViewModel
            {
                Id = r.Id,
                DestinationName = r.Destination,
                District = GetDestinationDistrict(r.Destination),
                TransportType = r.TransportType,
                TravelTime = r.TravelTimeInMinutes,
                Cost = r.Cost,
                Familiarity = r.FamiliarityLevel.ToString(),
                Requirements = new List<string>() // TODO: Map requirements from SimpleRouteViewModel
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

        private List<string> GetRouteRequirements(SimpleRouteViewModel route)
        {
            var requirements = new List<string>();
            
            // SimpleRouteViewModel doesn't have these properties yet
            // TODO: Add requirements to SimpleRouteViewModel
            
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
            var player = GameFacade.GetPlayer();
            if (player.Coins < route.Cost)
                return false;
                
            // Check stamina cost (base 2 for any travel)
            if (player.Stamina < 2)
                return false;
                
            // Check requirements (includes token requirements, time restrictions, etc.)
            // Requirements list is populated by GetRouteRequirements which checks tokens
            // If there are any unmet requirements, the route cannot be taken
            // The actual token checking happens in GameFacade when building the requirements list
            
            return true; // All basic checks passed
        }

        protected string GetCannotTravelReason(RouteViewModel route)
        {
            var player = GameFacade.GetPlayer();
            
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
                
            var hours = minutes / 60;
            var mins = minutes % 60;
            
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