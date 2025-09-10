using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.TravelSubsystem;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Travel screen component that displays available routes and handles travel between locations.
    /// 
    /// CRITICAL: BLAZOR SERVERPRERENDERED CONSEQUENCES
    /// ================================================
    /// This component renders TWICE due to ServerPrerendered mode:
    /// 1. During server-side prerendering (static HTML generation)
    /// 2. After establishing interactive SignalR connection
    /// 
    /// ARCHITECTURAL PRINCIPLES:
    /// - OnParametersSetAsync() runs TWICE - LoadAvailableRoutes is read-only and safe
    /// - Route discovery state maintained in GameWorld singleton (persists)
    /// - Travel actions only happen after interactive connection (user clicks)
    /// - Cost calculations are display-only (actual costs enforced by backend)
    /// 
    /// IMPLEMENTATION REQUIREMENTS:
    /// - LoadAvailableRoutes() fetches display data only (no mutations)
    /// - Route availability determined by backend (discovery, permits, etc.)
    /// - Travel execution creates TravelIntent (processed by GameFacade)
    /// - Resource costs shown but not deducted in UI (backend validates)
    /// </summary>
    public class TravelContentBase : ComponentBase
    {
        [Parameter] public string CurrentLocation { get; set; }
        [Parameter] public EventCallback<string> OnTravelRoute { get; set; }
        [Parameter] public EventCallback<string> OnNavigate { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected TimeManager TimeManager { get; set; }
        [Inject] protected TravelFacade TravelFacade { get; set; }
        [Inject] protected TravelManager TravelManager { get; set; }

        protected List<RouteViewModel> AvailableRoutes { get; set; } = new();
        protected RouteViewModel SelectedRoute { get; set; }
        protected TravelContext CurrentTravelContext { get; set; }

        protected override async Task OnInitializedAsync()
        {
            LoadTravelState();
        }

        protected override async Task OnParametersSetAsync()
        {
            LoadTravelState();
        }

        private void LoadTravelState()
        {
            // Check if there's an active travel session
            CurrentTravelContext = TravelFacade.GetCurrentTravelContext();
            
            // If no active travel session, load available routes for selection
            if (CurrentTravelContext == null)
            {
                LoadAvailableRoutes();
            }
        }
        
        private void LoadAvailableRoutes()
        {
            Location currentLoc = GameFacade.GetCurrentLocation();
            if (currentLoc == null) return;

            List<RouteOption> routes = GameFacade.GetAvailableRoutes();

            AvailableRoutes = routes.Select(r => new RouteViewModel
            {
                Id = r.Id,
                Name = r.Name,  // Store the actual route name from JSON
                DestinationName = GetDestinationLocationName(r.DestinationLocationSpot),
                DestinationSpotName = GetDestinationLocationSpotName(r.DestinationLocationSpot),
                District = GetDestinationDistrict(r.DestinationLocationSpot),
                TransportType = FormatTransportType(r.Method),
                TravelTime = r.TravelTimeMinutes,
                Cost = r.BaseCoinCost,
                HungerCost = CalculateHungerCost(r),
                RouteType = DetermineRouteType(r),
                Tags = ExtractRouteTags(r),
                Requirements = ExtractRouteRequirements(r)
            }).ToList();
        }

        private string GetDestinationLocationName(string destinationSpotId)
        {
            // Get the actual location spot from GameWorld to find its name
            LocationSpot spot = GameFacade.GetLocationSpot(destinationSpotId);
            Location location = GameFacade.GetLocationById(spot.LocationId);

            if (location != null)
            {
                return location.Name;
            }
            return "Unknown Location";
        }

        private string GetDestinationLocationSpotName(string destinationSpotId)
        {
            // Get the actual location spot from GameWorld to find its name
            LocationSpot spot = GameFacade.GetLocationSpot(destinationSpotId);
            return spot?.Name ?? destinationSpotId;
        }

        private string GetDestinationDistrict(string destinationSpotId)
        {
            // Get all locations from GameFacade
            List<Location> locations = GameFacade.GetAllLocations();
            if (locations == null) return "City Center";

            // Find the location containing this spot
            foreach (Location location in locations)
            {
                if (location.LocationSpotIds.Contains(destinationSpotId))
                {
                    // Get the district for this location
                    District district = GameFacade.GetDistrictForLocation(location.Id);
                    if (district != null)
                    {
                        // Get the region for this district
                        Region region = GameFacade.GetRegionForDistrict(district.Id);
                        if (region != null)
                        {
                            return $"{region.Name} • {district.Name}";
                        }
                        return district.Name;
                    }
                    return "City Center";
                }
            }

            return "City Center";
        }

        private string FormatTransportType(TravelMethods method)
        {
            return method switch
            {
                TravelMethods.Walking => "WALKING",
                TravelMethods.Cart => "CART",
                TravelMethods.Carriage => "CARRIAGE",
                TravelMethods.Boat => "BOAT",
                _ => "WALKING"
            };
        }

        private int CalculateHungerCost(RouteOption route)
        {
            // Base hunger cost from route data
            int hungerCost = route.BaseStaminaCost; // This is the hunger cost in the data

            // Add load penalties
            Player player = GameFacade.GetPlayer();
            int itemCount = player.Inventory.ItemSlots.Count(i => !string.IsNullOrEmpty(i));
            if (itemCount > 3) // Light load threshold
            {
                hungerCost += (itemCount - 3);
            }

            return hungerCost;
        }

        private RouteType DetermineRouteType(RouteOption route)
        {
            // Determine route type based on terrain categories
            foreach (TerrainCategory terrain in route.TerrainCategories)
            {
                if (terrain == TerrainCategory.Requires_Permission)
                    return RouteType.Guarded;
                if (terrain == TerrainCategory.Dark_Passage || terrain == TerrainCategory.Wilderness_Terrain)
                    return RouteType.Dangerous;
                if (terrain == TerrainCategory.Heavy_Cargo_Route)
                    return RouteType.Merchant;
            }
            return RouteType.Common;
        }

        private List<string> ExtractRouteTags(RouteOption route)
        {
            List<string> tags = new List<string>();

            // Add tags based on terrain categories
            foreach (TerrainCategory terrain in route.TerrainCategories)
            {
                switch (terrain)
                {
                    case TerrainCategory.Exposed_Weather:
                        tags.Add("EXPOSED");
                        break;
                    case TerrainCategory.Dark_Passage:
                        tags.Add("DISCRETE");
                        break;
                    case TerrainCategory.Wilderness_Terrain:
                        tags.Add("WILDERNESS");
                        break;
                    case TerrainCategory.Heavy_Cargo_Route:
                        tags.Add("COMMERCIAL");
                        break;
                }
            }

            // Add tags based on access requirements
            if (route.AccessRequirement != null)
            {
                tags.Add("RESTRICTED");
            }

            // Add tags based on transport method
            if (route.Method == TravelMethods.Walking)
            {
                tags.Add("PUBLIC");
            }

            return tags;
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

            // Tier requirements removed - all routes accessible

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

            // Start a path cards journey instead of instant travel
            TravelSession session = TravelManager.StartJourney(route.Id);
            if (session != null)
            {
                // Refresh to show the path cards interface
                LoadTravelState();
                StateHasChanged();
            }
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

        protected string GetRouteTypeClass(RouteViewModel route)
        {
            return route.RouteType switch
            {
                RouteType.Dangerous => "dangerous",
                RouteType.Guarded => "guarded",
                RouteType.Merchant => "merchant",
                _ => "common"
            };
        }

        protected string GetRouteMethodDescription(RouteViewModel route)
        {
            // Use the actual route name from JSON which is already descriptive
            // The route.Name should contain something like "Main Gate via Guard Checkpoint"
            // We just need to extract the part after "via" if it exists, or use the full name

            if (!string.IsNullOrEmpty(route.Name))
            {
                // If the name contains "via", return the full name as is
                if (route.Name.Contains("via"))
                    return route.Name;

                // Otherwise try to make it more descriptive based on tags
                string prefix = route.TransportType switch
                {
                    "Cart" => "Cart: ",
                    "Carriage" => "Carriage: ",
                    "Boat" => "Boat: ",
                    _ => ""
                };

                return prefix + route.Name;
            }

            // Fallback to tag-based description
            if (route.Tags.Contains("DISCRETE"))
                return "Discrete Passage";
            if (route.Tags.Contains("COMMERCIAL"))
                return "Commercial Route";
            if (route.Tags.Contains("WILDERNESS"))
                return "Wilderness Path";
            if (route.Tags.Contains("RESTRICTED"))
                return "Restricted Access";

            return "Common Path";
        }

        protected string GetTagClass(string tag)
        {
            return tag.ToUpper() switch
            {
                "PUBLIC" => "tag-public",
                "DISCRETE" => "tag-discrete",
                "EXPOSED" => "tag-exposed",
                "WILDERNESS" => "tag-wilderness",
                "COMMERCIAL" => "tag-commercial",
                "RESTRICTED" => "tag-restricted",
                _ => "tag-public"
            };
        }

        /// <summary>
        /// Check if there is an active travel session
        /// </summary>
        protected bool HasActiveTravelSession()
        {
            return CurrentTravelContext != null && CurrentTravelContext.Session != null;
        }
    }

    public class RouteViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }  // The route name from JSON
        public string DestinationName { get; set; }  // The actual location name
        public string DestinationSpotName { get; set; }  // The actual location spot name
        public string District { get; set; }
        public string TransportType { get; set; }
        public int TravelTime { get; set; }
        public int Cost { get; set; }
        public int HungerCost { get; set; }
        public RouteType RouteType { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<string> Requirements { get; set; } = new();
    }

    public enum RouteType
    {
        Common,
        Dangerous,
        Guarded,
        Merchant
    }
}