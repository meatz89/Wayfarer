using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components
{
    public class DiscoveryJournalBase : ComponentBase
    {
        [Inject] protected GameWorld GameWorld { get; set; } = null!;
        [Parameter] public EventCallback OnClose { get; set; }

        protected async Task CloseJournal()
        {
            await OnClose.InvokeAsync();
        }

        protected int GetDiscoveredLocationCount()
        {
            Player player = GameWorld.GetPlayer();
            return player.LocationFamiliarity.Count;
        }

        protected int GetTotalLocationCount()
        {
            return GameWorld.Locations.Count;
        }

        protected List<Location> GetDiscoveredLocations()
        {
            Player player = GameWorld.GetPlayer();
            return GameWorld.Locations
                .Where(l => player.LocationFamiliarity.Any(f => f.EntityId == l.Id))
                .OrderBy(l => l.Name)
                .ToList();
        }

        protected int GetFamiliarity(string locationId)
        {
            return GameWorld.GetPlayer().GetLocationFamiliarity(locationId);
        }

        protected double GetFamiliarityPercent(string locationId, int max)
        {
            if (max == 0) return 0;
            return (double)GetFamiliarity(locationId) / max * 100.0;
        }

        protected int GetCollectedObservationCount()
        {
            return GameWorld.GetPlayer().CollectedObservations.Count;
        }

        protected List<string> GetCollectedObservations()
        {
            return GameWorld.GetPlayer().CollectedObservations;
        }

        protected int GetExploredRouteCount()
        {
            return GameWorld.GetPlayer().KnownRoutes.Sum(kr => kr.Routes.Count);
        }

        protected List<RouteInfo> GetKnownRoutes()
        {
            Player player = GameWorld.GetPlayer();
            List<RouteInfo> routes = new List<RouteInfo>();

            foreach (var entry in player.KnownRoutes)
            {
                foreach (var route in entry.Routes)
                {
                    routes.Add(new RouteInfo
                    {
                        Id = $"{route.OriginLocationSpot}_{route.DestinationLocationSpot}",
                        OriginName = route.OriginLocationSpot,
                        DestinationName = route.DestinationLocationSpot,
                        Familiarity = player.GetRouteFamiliarity($"{route.OriginLocationSpot}_{route.DestinationLocationSpot}")
                    });
                }
            }

            return routes.OrderBy(r => r.OriginName).ToList();
        }

        protected int GetRouteFamiliarity(string routeId)
        {
            return GameWorld.GetPlayer().GetRouteFamiliarity(routeId);
        }
    }

    public class RouteInfo
    {
        public string Id { get; set; } = "";
        public string OriginName { get; set; } = "";
        public string DestinationName { get; set; } = "";
        public int Familiarity { get; set; }
    }
}
