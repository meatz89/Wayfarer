using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components
{
    public class DiscoveryJournalBase : ComponentBase
    {
        [Inject] protected GameWorld GameWorld { get; set; }
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

        protected List<Venue> GetDiscoveredLocations()
        {
            Player player = GameWorld.GetPlayer();
            return GameWorld.WorldState.venues
                .Where(l => player.LocationFamiliarity.Any(f => f.EntityId == l.Id))
                .OrderBy(l => l.Name)
                .ToList();
        }

        protected int GetFamiliarity(string venueId)
        {
            return GameWorld.GetPlayer().GetLocationFamiliarity(venueId);
        }

        protected double GetFamiliarityPercent(string venueId, int max)
        {
            if (max == 0) return 0;
            return (double)GetFamiliarity(venueId) / max * 100.0;
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

            foreach (KnownRouteEntry entry in player.KnownRoutes)
            {
                foreach (RouteOption route in entry.Routes)
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
            // NOTE: Investigation phases no longer reference goals directly
            // Goals are now contained within obstacles spawned by investigations
            // This UI method needs redesign to show obstacle locations instead
            // For now, return empty dictionary until obstacle-based UI is implemented
            return new Dictionary<string, int>();
        }
    }

    public class RouteInfo
    {
        public string Id { get; set; }
        public string OriginName { get; set; }
        public string DestinationName { get; set; }
        public int Familiarity { get; set; }
    }
}
