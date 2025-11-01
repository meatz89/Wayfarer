using Microsoft.AspNetCore.Components;

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

        protected List<ActiveObligation> GetActiveObligations()
        {
            return GameWorld.ObligationJournal.ActiveObligations.ToList();
        }

        protected List<string> GetCompletedObligationIds()
        {
            return GameWorld.ObligationJournal.CompletedObligationIds.ToList();
        }

        protected List<string> GetDiscoveredObligationIds()
        {
            return GameWorld.ObligationJournal.DiscoveredObligationIds.ToList();
        }

        protected Obligation GetObligationById(string obligationId)
        {
            return GameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
        }

        protected int GetObligationProgress(ActiveObligation activeInv)
        {
            // CompletedSituationIds eliminated - track progress via resolved scenes instead
            // NOTE: Scenes no longer have ObligationId - UI needs redesign to show scene locations
            // For now, return understanding accumulated as progress metric
            return activeInv.UnderstandingAccumulated;
        }

        protected int GetObligationTotalSituations(string obligationId)
        {
            // PhaseDefinitions eliminated - return static understanding requirement for now
            // NOTE: Scenes no longer have ObligationId - UI needs redesign
            // TODO: Add UnderstandingRequired property to Obligation model
            return 10; // Default Understanding requirement for completion
        }

        protected double GetObligationProgressPercent(ActiveObligation activeInv)
        {
            int total = GetObligationTotalSituations(activeInv.ObligationId);
            if (total == 0) return 0;
            // CompletedSituationIds eliminated - use resolved scene count instead
            int resolved = GetObligationProgress(activeInv);
            return ((double)resolved / total) * 100.0;
        }

        protected List<ObligationPhaseDefinition> GetCompletedPhases(ActiveObligation activeInv)
        {
            // PhaseDefinitions and CompletedSituationIds eliminated - obligations no longer have sequential phases
            // Return resolved scenes instead
            return new List<ObligationPhaseDefinition>();
        }

        protected List<ObligationPhaseDefinition> GetActivePhases(ActiveObligation activeInv)
        {
            // PhaseDefinitions and CompletedSituationIds eliminated - obligations no longer have sequential phases
            // Return active scenes instead
            return new List<ObligationPhaseDefinition>();
        }

        protected Dictionary<string, int> GetRemainingSituationsByLocation(ActiveObligation activeInv)
        {
            // NOTE: Obligation phases no longer reference situations directly
            // Situations are now contained within scenes spawned by obligations
            // This UI method needs redesign to show scene locations instead
            // For now, return empty dictionary until scene-based UI is implemented
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
