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
                Requirements = GetRouteRequirements(r)
            }).ToList();
        }

        private string GetDestinationDistrict(string destinationName)
        {
            // TODO: Get actual district from location
            return "Market District";
        }

        private List<string> GetRouteRequirements(Route route)
        {
            var requirements = new List<string>();
            
            if (route.RequiredTokenCount > 0)
            {
                requirements.Add($"{route.RequiredTokenCount} {route.RequiredTokenType} tokens");
            }
            
            if (route.TimeRestriction != null)
            {
                requirements.Add($"Only during {route.TimeRestriction}");
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
            var resources = GameFacade.GetPlayerResourceState();
            if (resources.Coins < route.Cost)
                return false;
                
            // Check time restrictions
            // TODO: Check token requirements
            
            return true;
        }

        protected string GetCannotTravelReason(RouteViewModel route)
        {
            var resources = GameFacade.GetPlayerResourceState();
            
            if (resources.Coins < route.Cost)
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