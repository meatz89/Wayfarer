using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Travel path cards component that displays active travel session with stamina management
    /// </summary>
    public class TravelPathContentBase : ComponentBase
    {
        [Parameter] public TravelContext TravelContext { get; set; }
        [Parameter] public EventCallback<string> OnNavigate { get; set; }

        [Inject] protected GameOrchestrator GameOrchestrator { get; set; }
        [Inject] protected TravelManager TravelManager { get; set; }
        [Inject] protected TravelFacade TravelFacade { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected SceneFacade SceneFacade { get; set; }

        protected List<PathCardInfo> AvailablePathCards { get; set; } = new();

        protected override async Task OnParametersSetAsync()
        {
            await LoadPathCardsAsync();
        }

        private async Task LoadPathCardsAsync()
        {
            if (TravelContext == null)
            {
                AvailablePathCards = new List<PathCardInfo>();
                return;
            }

            // Get path cards for current segment with discovery state
            AvailablePathCards = await TravelFacade.GetAvailablePathCardsAsync();
        }

        /// <summary>
        /// Get available path cards for display (cached from last load)
        /// </summary>
        protected List<PathCardInfo> GetAvailablePathCards()
        {
            return AvailablePathCards;
        }

        /// <summary>
        /// Get segment count
        /// </summary>
        protected int GetSegmentCount()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.CurrentRoute == null)
                throw new InvalidOperationException("No current route in travel context");
            if (TravelContext.CurrentRoute.Segments == null)
                throw new InvalidOperationException("Route has no segments");
            return TravelContext.CurrentRoute.Segments.Count;
        }

        /// <summary>
        /// Get current segment number
        /// </summary>
        protected int GetCurrentSegment()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");
            return TravelContext.Session.CurrentSegment;
        }

        /// <summary>
        /// Get route familiarity display
        /// </summary>
        protected string GetRouteFamiliarity()
        {
            // Mock data for now - should come from game state
            return "0/3";
        }

        /// <summary>
        /// Get transport type for current route
        /// </summary>
        protected string GetTransportType()
        {
            if (TravelContext?.CurrentRoute == null)
                return "Walking";

            return TravelContext.CurrentRoute.Method.ToString().Replace("_", " ");
        }

        /// <summary>
        /// Get stamina display string
        /// </summary>
        protected string GetStaminaDisplay()
        {
            if (TravelContext?.Session == null)
                return "0/0";

            return $"{TravelContext.Session.StaminaRemaining}/{TravelContext.Session.StaminaCapacity}";
        }

        /// <summary>
        /// Get CSS class for path card
        /// </summary>
        protected string GetPathCardCssClass(bool isDiscovered, bool isAvailable)
        {
            List<string> classes = new() { "path-card" };

            // Event cards should never get face-down class
            bool isEventSegment = IsCurrentSegmentEventType();

            if (isDiscovered || isEventSegment)
            {
                classes.Add("face-up");
            }
            else
            {
                classes.Add("face-down");
            }

            if (!isAvailable)
            {
                classes.Add("unavailable");
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Get one-time reward status
        /// </summary>
        protected string GetOneTimeStatus(PathCardDTO card)
        {
            // Check if one-time reward has been claimed
            // This should check game state to see if reward was already collected
            return "";  // For now, assume not claimed, or "(already claimed)"
        }

        /// <summary>
        /// Get unavailable reason for path card
        /// HIGHLANDER: Accept PathCardDTO object, not string ID
        /// </summary>
        protected string GetUnavailableReason(PathCardDTO card)
        {
            if (card == null) return "";

            PathCardAvailability availability = TravelFacade.GetPathCardAvailability(card);
            if (availability != null && !availability.CanPlay)
            {
                if (string.IsNullOrEmpty(availability.Reason))
                    return "Cannot select this path";
                return availability.Reason;
            }
            return "";
        }

        /// <summary>
        /// Check if player can rest
        /// </summary>
        protected bool CanRest()
        {
            return TravelContext?.Session != null && TravelContext.Session.CurrentState != TravelState.Fresh;
        }

        /// <summary>
        /// Check if player must turn back
        /// </summary>
        protected bool MustTurnBack()
        {
            // Player must turn back if they can't afford any path cards and can't rest
            if (TravelContext?.Session == null)
                return false;

            List<PathCardInfo> availableCards = GetAvailablePathCards();
            return !availableCards.Any(c => c.CanPlay) && !CanRest();
        }

        /// <summary>
        /// Get destination name (same as existing method)
        /// </summary>
        protected string GetDestinationName()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.CurrentRoute == null)
                throw new InvalidOperationException("No current route in travel context");

            // Get destination from route (object reference)
            Location destination = TravelContext.CurrentRoute.DestinationLocation;
            if (destination == null)
                return "Unknown Destination";

            // Get Venue from destination location (object reference)
            Venue venue = destination.Venue;
            if (venue != null)
            {
                return venue.Name;
            }

            return "Unknown Destination";
        }

        /// <summary>
        /// Handle path card selection - all cards now use reveal mechanic
        /// HIGHLANDER: Accept PathCardDTO object, not string ID
        /// </summary>
        protected async Task SelectPathCard(PathCardDTO card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");

            if (!TravelFacade.CanPlayPathCard(card))
            {
                return;
            }

            // Call TravelManager - all cards now use reveal mechanic (no face-down checks)
            bool success = await TravelManager.SelectPathCard(card);
            if (success)
            {
                // Refresh the context after card selection
                await RefreshTravelContext();
                StateHasChanged();
            }
        }

        /// <summary>
        /// Handle rest action
        /// </summary>
        protected async Task RestAction()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");

            bool success = TravelManager.RestAction();
            if (success)
            {
                await RefreshTravelContext();
                StateHasChanged();
            }
        }

        /// <summary>
        /// Handle turn back action
        /// </summary>
        protected async Task TurnBack()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");

            bool success = TravelManager.TurnBack();
            if (success)
            {
                // Navigate back to Venue since journey was cancelled
                await OnNavigate.InvokeAsync("location");
            }
        }

        /// <summary>
        /// Get stamina dots for display
        /// </summary>
        protected List<StaminaDot> GetStaminaDots()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");

            List<StaminaDot> dots = new();
            int capacity = TravelContext.Session.StaminaCapacity;
            int remaining = TravelContext.Session.StaminaRemaining;

            for (int i = 0; i < capacity; i++)
            {
                dots.Add(new StaminaDot
                {
                    State = i < remaining ? StaminaDotState.Available : StaminaDotState.Spent
                });
            }

            return dots;
        }

        /// <summary>
        /// Get segment progress dots
        /// </summary>
        protected List<SegmentDot> GetSegmentDots()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.CurrentRoute == null)
                throw new InvalidOperationException("No current route in travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");

            List<SegmentDot> dots = new();
            int totalSegments = TravelContext.CurrentRoute.Segments.Count;
            int currentSegment = TravelContext.Session.CurrentSegment;

            for (int i = 1; i <= totalSegments; i++)
            {
                SegmentDotState state = i < currentSegment ? SegmentDotState.Complete :
                                     i == currentSegment ? SegmentDotState.Current :
                                     SegmentDotState.Pending;

                dots.Add(new SegmentDot { State = state });
            }

            return dots;
        }

        /// <summary>
        /// Get display text for travel state
        /// </summary>
        protected string GetTravelStateText()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");

            return TravelContext.Session.CurrentState switch
            {
                TravelState.Fresh => "Fresh State (5 capacity)",
                TravelState.Steady => "Steady State (6 capacity)",
                TravelState.Tired => "Tired State (4 capacity)",
                TravelState.Weary => "Weary State (3 capacity)",
                TravelState.Exhausted => "Exhausted State (0 capacity)",
                _ => "Unknown State"
            };
        }

        /// <summary>
        /// Check if card is discovered (face-up)
        /// HIGHLANDER: Uses object reference, not string ID
        /// </summary>
        protected bool IsCardDiscovered(PathCardDTO card)
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            return TravelContext.CardDiscoveries.Any(d => d.Card == card) &&
                   TravelContext.CardDiscoveries.FirstOrDefault(d => d.Card == card)?.IsDiscovered == true;
        }

        /// <summary>
        /// Check if card can be played
        /// HIGHLANDER: Accept PathCardDTO object, not string ID
        /// </summary>
        protected bool CanPlayCard(PathCardDTO card)
        {
            return TravelFacade.CanPlayPathCard(card);
        }

        /// <summary>
        /// Get card CSS classes
        /// HIGHLANDER: Uses object reference for discovery check
        /// </summary>
        protected string GetPathCardClass(PathCardDTO card)
        {
            List<string> classes = new() { "path-card" };

            if (IsCardDiscovered(card))
            {
                classes.Add("face-up");
            }
            else
            {
                classes.Add("face-down");
            }

            if (!CanPlayCard(card))
            {
                classes.Add("unavailable");
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Get requirements text for card
        /// </summary>
        protected string GetRequirementsText(PathCardDTO card)
        {
            List<string> requirements = new();

            if (card.CoinRequirement > 0)
            {
                requirements.Add($"{card.CoinRequirement} coins");
            }

            if (!string.IsNullOrEmpty(card.PermitRequirement))
            {
                requirements.Add(card.PermitRequirement);
            }

            // Add stat requirements with level display
            if (card.StatRequirements != null && card.StatRequirements.Count > 0)
            {
                Player player = GameOrchestrator.GetPlayer();
                foreach (StatRequirementDTO statReq in card.StatRequirements)
                {
                    if (Enum.TryParse<PlayerStatType>(statReq.Stat, true, out PlayerStatType statType))
                    {
                        string statName = GetStatDisplayName(statType);
                        int required = statReq.Value;
                        int current = statType switch
                        {
                            PlayerStatType.Insight => player.Insight,
                            PlayerStatType.Rapport => player.Rapport,
                            PlayerStatType.Authority => player.Authority,
                            PlayerStatType.Diplomacy => player.Diplomacy,
                            PlayerStatType.Cunning => player.Cunning,
                            _ => 0
                        };

                        if (current >= required)
                        {
                            requirements.Add($"{statName} {required}+ <span class='icon-check'></span>");
                        }
                        else
                        {
                            requirements.Add($"{statName} {required}+ (have {current})");
                        }
                    }
                }
            }

            return requirements.Any() ? $"Requires: {string.Join(", ", requirements)}" : "";
        }

        protected string GetStatDisplayName(PlayerStatType statType)
        {
            return statType switch
            {
                PlayerStatType.Insight => "Insight",
                PlayerStatType.Rapport => "Rapport",
                PlayerStatType.Authority => "Authority",
                PlayerStatType.Diplomacy => "Diplomacy",
                PlayerStatType.Cunning => "Cunning",
                _ => statType.ToString()
            };
        }

        protected string GetStatIconClass(string statKey)
        {
            if (Enum.TryParse<PlayerStatType>(statKey, true, out PlayerStatType statType))
            {
                return statType switch
                {
                    PlayerStatType.Insight => "insight",
                    PlayerStatType.Rapport => "rapport",
                    PlayerStatType.Authority => "authority",
                    PlayerStatType.Diplomacy => "diplomacy",
                    PlayerStatType.Cunning => "cunning",
                    _ => "default"
                };
            }
            return "default";
        }

        /// <summary>
        /// Check if journey is complete
        /// </summary>
        protected bool IsJourneyComplete()
        {
            return TravelContext?.Session == null;
        }

        /// <summary>
        /// Get dynamic title based on segment type
        /// </summary>
        protected string GetPathsTitle()
        {
            return IsCurrentSegmentEventType() ? "Choose Your Response" : "Choose Your Path";
        }

        /// <summary>
        /// Get event narrative for current segment if applicable
        /// </summary>
        protected string GetEventNarrative()
        {
            return TravelFacade.GetCurrentEventNarrative();
        }

        /// <summary>
        /// Check if current segment is Event type
        /// </summary>
        protected bool IsCurrentSegmentEventType()
        {
            return TravelFacade.IsCurrentSegmentEventType();
        }

        /// <summary>
        /// Confirm the revealed card and proceed to next segment
        /// </summary>
        protected async Task ConfirmRevealedCard()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");

            bool success = await TravelFacade.ConfirmRevealedCard();
            if (success)
            {
                await RefreshTravelContext();
                StateHasChanged();
            }
        }

        /// <summary>
        /// Finish the route when ready to complete
        /// </summary>
        protected async Task FinishRoute()
        {
            if (TravelFacade.FinishRoute())
            {
                // Navigate back to Venue screen
                await OnNavigate.InvokeAsync("location");
            }
        }

        /// <summary>
        /// Check if journey is ready to complete
        /// </summary>
        protected bool IsReadyToComplete()
        {
            return TravelFacade.IsReadyToComplete();
        }

        /// <summary>
        /// Check if we're in card reveal state
        /// </summary>
        protected bool IsRevealingCard()
        {
            return TravelFacade.IsRevealingCard();
        }

        /// <summary>
        /// Get the revealed card ID
        /// </summary>
        protected string GetRevealedCardId()
        {
            return TravelFacade.GetRevealedCardId();
        }

        /// <summary>
        /// Check if the specified card is the one being revealed
        /// HIGHLANDER: Accept PathCardDTO object, not string ID
        /// </summary>
        protected bool IsCardBeingRevealed(PathCardDTO card)
        {
            if (card == null) return false;
            return IsRevealingCard() && GetRevealedCardId() == card.Id;
        }

        /// <summary>
        /// Check if other cards should be disabled during reveal state
        /// HIGHLANDER: Accept PathCardDTO object, not string ID
        /// </summary>
        protected bool ShouldDisableCard(PathCardDTO card)
        {
            if (card == null) return false;
            // If we're revealing a card and this isn't the revealed card, disable it
            return IsRevealingCard() && GetRevealedCardId() != card.Id;
        }

        /// <summary>
        /// Refresh travel context from backend
        /// </summary>
        private async Task RefreshTravelContext()
        {
            // Get updated context from TravelFacade
            TravelContext = await TravelFacade.GetCurrentTravelContextAsync();
            await LoadPathCardsAsync();

            // Check if journey completed
            if (TravelContext == null)
            {
                // Journey finished, navigate back to location
                await OnNavigate.InvokeAsync("location");
            }
        }


        // ========== TRAVEL CONTEXT DETECTION (FOR UI CONDITIONAL RENDERING) ==========

        /// <summary>
        /// Get current route segment object (not just the number)
        /// </summary>
        protected RouteSegment GetCurrentRouteSegment()
        {
            if (TravelContext == null)
                throw new InvalidOperationException("No active travel context");
            if (TravelContext.Session == null)
                throw new InvalidOperationException("No active travel session");
            if (TravelContext.CurrentRoute == null)
                throw new InvalidOperationException("No current route in travel context");

            int currentSegmentNum = TravelContext.Session.CurrentSegment;
            if (currentSegmentNum < 1 || currentSegmentNum > TravelContext.CurrentRoute.Segments.Count)
                return null;

            return TravelContext.CurrentRoute.Segments[currentSegmentNum - 1];
        }

        /// <summary>
        /// Check if current segment is a dead-end (all paths blocked/unavailable)
        /// Mockup: travel-dead-end.html
        /// </summary>
        protected bool IsDeadEnd()
        {
            RouteSegment segment = GetCurrentRouteSegment();
            if (segment == null)
                return false;

            // PathCard system
            List<PathCardInfo> availableCards = GetAvailablePathCards();
            if (availableCards.Any())
            {
                // Check if all cards are unplayable
                return availableCards.All(c => !c.CanPlay);
            }

            // No paths available at all
            return true;
        }

        /// <summary>
        /// Check if displaying discovery reveal animation
        /// Mockup: travel-discovery.html
        /// </summary>
        protected bool IsDiscoveryReveal()
        {
            return IsRevealingCard();
        }

        /// <summary>
        /// Check if current segment is Event type (caravan)
        /// Mockup: travel-caravan.html
        /// </summary>
        protected bool IsEventSegment()
        {
            return IsCurrentSegmentEventType();
        }

        /// <summary>
        /// Check if current segment is FixedPath type (walking)
        /// Mockup: travel-walking.html
        /// </summary>
        protected bool IsFixedPathSegment()
        {
            RouteSegment segment = GetCurrentRouteSegment();
            if (segment == null)
                return false;

            return segment.Type == SegmentType.FixedPath;
        }

        /// <summary>
        /// Check if current segment is Encounter type (mandatory challenge)
        /// Renders as challenge screen where player must resolve scene to proceed
        /// </summary>
        protected bool IsEncounterSegment()
        {
            RouteSegment segment = GetCurrentRouteSegment();
            if (segment == null)
                return false;

            return segment.Type == SegmentType.Encounter;
        }

        /// <summary>
        /// Engage the encounter - navigate to scene resolution
        /// Called when player clicks ENGAGE CHALLENGE button
        /// </summary>
        protected async Task EngageEncounter()
        {
            if (TravelContext?.Session?.PendingScene == null)
                return;

            // Navigate to scene screen to resolve the encounter
            await OnNavigate.InvokeAsync("scene");
        }

        /// <summary>
        /// Get all unique encounter situation names for current segment
        /// Aggregates situation names from all scenes on all paths in current segment
        /// </summary>
        protected List<string> GetEncounterSituationsForCurrentSegment()
        {
            List<string> situationNames = new List<string>();
            RouteSegment segment = GetCurrentRouteSegment();
            if (segment == null)
                return situationNames;

            // PathCard system - get scenes from PathCards
            List<PathCardInfo> availableCards = GetAvailablePathCards();
            foreach (PathCardInfo cardInfo in availableCards)
            {
                if (!string.IsNullOrEmpty(cardInfo.Card.SceneId))
                {
                    List<SituationPreviewData> situationPreviews = GetSituationPreviewsForPath(cardInfo.Card.SceneId);
                    foreach (SituationPreviewData preview in situationPreviews)
                    {
                        if (!string.IsNullOrEmpty(preview.Name) && !situationNames.Contains(preview.Name))
                        {
                            situationNames.Add(preview.Name);
                        }
                    }
                }
            }

            return situationNames;
        }

        /// <summary>
        /// Get situation previews for a scene (used for displaying encounters on path cards)
        /// ARCHITECTURE: Query GameWorld.Scenes to find Scene, get Situations from Scene.Situations
        /// </summary>
        protected List<SituationPreviewData> GetSituationPreviewsForPath(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId))
                return new List<SituationPreviewData>();

            // Query GameWorld.Scenes for scene with matching TemplateId
            Scene scene = GameWorld.Scenes.FirstOrDefault(s => s.TemplateId == sceneId);
            if (scene == null)
                return new List<SituationPreviewData>();

            // Query scene.Situations directly (HIGHLANDER: Scene owns Situations)
            List<Situation> situations = scene.Situations.ToList();

            // Convert to preview data
            List<SituationPreviewData> previews = new List<SituationPreviewData>();
            foreach (Situation situation in situations)
            {
                SituationPreviewData preview = new SituationPreviewData
                {
                    Name = situation.Name ?? "",
                    Description = situation.Description ?? "",
                    SystemType = situation.SystemType.ToString()
                };
                previews.Add(preview);
            }

            return previews;
        }

        /// <summary>
        /// Helper class for situation preview data
        /// </summary>
        protected class SituationPreviewData
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string SystemType { get; set; }
        }
    }

    /// <summary>
    /// Helper class for stamina dot display
    /// </summary>
    public class StaminaDot
    {
        public StaminaDotState State { get; set; }
    }

    public enum StaminaDotState
    {
        Available,
        Spent
    }

    /// <summary>
    /// Helper class for segment progress display
    /// </summary>
    public class SegmentDot
    {
        public SegmentDotState State { get; set; }
    }

    public enum SegmentDotState
    {
        Complete,
        Current,
        Pending
    }
}