using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components
{
    public class DiscoveryJournalBase : ComponentBase
    {
        [Inject] protected GameWorld GameWorld { get; set; } = null!;
        [Parameter] public EventCallback OnClose { get; set; }

        protected string CurrentTab { get; set; } = "active";

        protected async Task CloseJournal()
        {
            await OnClose.InvokeAsync();
        }

        protected void SelectTab(string tabName)
        {
            CurrentTab = tabName;
            StateHasChanged();
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

        protected List<ActiveInvestigation> GetActiveInvestigations()
        {
            return GameWorld.InvestigationJournal.ActiveInvestigations.ToList();
        }

        protected List<string> GetCompletedInvestigationIds()
        {
            return GameWorld.InvestigationJournal.CompletedInvestigationIds.ToList();
        }

        protected List<string> GetDiscoveredInvestigationIds()
        {
            return GameWorld.InvestigationJournal.DiscoveredInvestigationIds.ToList();
        }

        protected Investigation GetInvestigationById(string investigationId)
        {
            return GameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
        }

        protected int GetInvestigationProgress(ActiveInvestigation activeInv)
        {
            return activeInv.CompletedGoalIds.Count;
        }

        protected int GetInvestigationTotalGoals(string investigationId)
        {
            Investigation inv = GetInvestigationById(investigationId);
            return inv?.PhaseDefinitions.Count ?? 0;
        }

        protected double GetInvestigationProgressPercent(ActiveInvestigation activeInv)
        {
            int total = GetInvestigationTotalGoals(activeInv.InvestigationId);
            if (total == 0) return 0;
            return ((double)activeInv.CompletedGoalIds.Count / total) * 100.0;
        }

        protected List<InvestigationPhaseDefinition> GetCompletedPhases(ActiveInvestigation activeInv)
        {
            Investigation inv = GetInvestigationById(activeInv.InvestigationId);
            if (inv == null) return new List<InvestigationPhaseDefinition>();

            return inv.PhaseDefinitions
                .Where(p => activeInv.CompletedGoalIds.Contains(p.Id))
                .ToList();
        }

        protected List<InvestigationPhaseDefinition> GetActivePhases(ActiveInvestigation activeInv)
        {
            Investigation inv = GetInvestigationById(activeInv.InvestigationId);
            if (inv == null) return new List<InvestigationPhaseDefinition>();

            return inv.PhaseDefinitions
                .Where(p => !activeInv.CompletedGoalIds.Contains(p.Id))
                .ToList();
        }

        protected Dictionary<string, int> GetRemainingGoalsByLocation(ActiveInvestigation activeInv)
        {
            Dictionary<string, int> locationCounts = new Dictionary<string, int>();
            List<InvestigationPhaseDefinition> activePhases = GetActivePhases(activeInv);

            foreach (InvestigationPhaseDefinition phase in activePhases)
            {
                // Derive location from spot (SpotId is globally unique)
                LocationSpotEntry spotEntry = GameWorld.Spots.FirstOrDefault(s => s.Spot.SpotID == phase.SpotId);
                Location loc = spotEntry != null
                    ? GameWorld.Locations.FirstOrDefault(l => l.Id == spotEntry.Spot.LocationId)
                    : null;

                if (loc != null && spotEntry != null)
                {
                    string locationKey = $"{loc.Name} - {spotEntry.Spot.Name}";
                    if (!locationCounts.ContainsKey(locationKey))
                        locationCounts[locationKey] = 0;
                    locationCounts[locationKey]++;
                }
            }

            return locationCounts;
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
