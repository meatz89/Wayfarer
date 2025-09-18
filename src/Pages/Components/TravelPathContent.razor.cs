using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.Subsystems.TravelSubsystem;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Travel path cards component that displays active travel session with stamina management
    /// </summary>
    public class TravelPathContentBase : ComponentBase
    {
        [Parameter] public TravelContext TravelContext { get; set; }
        [Parameter] public EventCallback<string> OnNavigate { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected TravelManager TravelManager { get; set; }
        [Inject] protected TravelFacade TravelFacade { get; set; }

        protected List<PathCardInfo> AvailablePathCards { get; set; } = new();

        protected override async Task OnParametersSetAsync()
        {
            LoadPathCards();
        }

        private void LoadPathCards()
        {
            if (TravelContext == null)
            {
                AvailablePathCards = new List<PathCardInfo>();
                return;
            }

            // Get path cards for current segment with discovery state
            AvailablePathCards = TravelFacade.GetAvailablePathCards();
        }

        /// <summary>
        /// Get available path cards for display
        /// </summary>
        protected List<PathCardInfo> GetAvailablePathCards()
        {
            if (TravelContext == null)
                return new List<PathCardInfo>();

            return TravelFacade.GetAvailablePathCards();
        }

        /// <summary>
        /// Get base travel time for route
        /// </summary>
        protected int GetBaseTravelTime()
        {
            return TravelContext?.CurrentRoute?.TravelTimeSegments ?? 0;
        }

        /// <summary>
        /// Get segment count
        /// </summary>
        protected int GetSegmentCount()
        {
            return TravelContext?.CurrentRoute?.Segments?.Count ?? 0;
        }

        /// <summary>
        /// Get current segment number
        /// </summary>
        protected int GetCurrentSegment()
        {
            return TravelContext?.Session?.CurrentSegment ?? 1;
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
        /// </summary>
        protected string GetUnavailableReason(string pathCardId)
        {
            PathCardAvailability availability = TravelFacade.GetPathCardAvailability(pathCardId);
            if (availability != null && !availability.CanPlay)
            {
                return availability.Reason ?? "Cannot select this path";
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
            if (TravelContext?.CurrentRoute == null)
                return "Unknown Destination";

            // Extract destination from route
            string destinationSpot = TravelContext.CurrentRoute.DestinationLocationSpot;
            if (string.IsNullOrEmpty(destinationSpot))
                return "Unknown Destination";

            // Get location spot and location name
            LocationSpot spot = GameFacade.GetLocationSpot(destinationSpot);
            if (spot != null)
            {
                Location location = GameFacade.GetLocationById(spot.LocationId);
                if (location != null)
                {
                    return location.Name;
                }
            }

            return "Unknown Destination";
        }

        /// <summary>
        /// Handle path card selection - all cards now use reveal mechanic
        /// </summary>
        protected async Task SelectPathCard(string pathCardId)
        {
            Console.WriteLine($"[TravelPathContent] SelectPathCard called with: {pathCardId}");

            if (TravelContext?.Session == null)
            {
                Console.WriteLine("[TravelPathContent] No travel session, returning");
                return;
            }

            // Check if player can afford the card
            Console.WriteLine($"[TravelPathContent] Looking for card in {TravelContext.CurrentSegmentCards?.Count ?? 0} cards");
            if (TravelContext.CurrentSegmentCards != null)
            {
                foreach (PathCardDTO c in TravelContext.CurrentSegmentCards)
                {
                    Console.WriteLine($"[TravelPathContent]   Available card: {c.Id}");
                }
            }

            PathCardDTO card = TravelContext.CurrentSegmentCards?.FirstOrDefault(c => c.Id == pathCardId);
            if (card == null)
            {
                Console.WriteLine($"[TravelPathContent] Card not found: {pathCardId}");
                return;
            }

            if (!TravelFacade.CanPlayPathCard(pathCardId))
            {
                Console.WriteLine($"[TravelPathContent] Cannot play card: {pathCardId}");
                return;
            }

            Console.WriteLine($"[TravelPathContent] Calling TravelManager.SelectPathCard for: {pathCardId}");
            // Call TravelManager - all cards now use reveal mechanic (no face-down checks)
            bool success = TravelManager.SelectPathCard(pathCardId);
            Console.WriteLine($"[TravelPathContent] SelectPathCard result: {success}");

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
            if (TravelContext?.Session == null)
                return;

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
            if (TravelContext?.Session == null)
                return;

            bool success = TravelManager.TurnBack();
            if (success)
            {
                // Navigate back to location since journey was cancelled
                await OnNavigate.InvokeAsync("location");
            }
        }

        /// <summary>
        /// Get stamina dots for display
        /// </summary>
        protected List<StaminaDot> GetStaminaDots()
        {
            if (TravelContext?.Session == null)
                return new List<StaminaDot>();

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
            if (TravelContext?.CurrentRoute == null || TravelContext.Session == null)
                return new List<SegmentDot>();

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
            if (TravelContext?.Session == null)
                return "Unknown State";

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
        /// </summary>
        protected bool IsCardDiscovered(string pathCardId)
        {
            return TravelContext?.CardDiscoveries.ContainsKey(pathCardId) == true &&
                   TravelContext.CardDiscoveries[pathCardId];
        }

        /// <summary>
        /// Check if card can be played
        /// </summary>
        protected bool CanPlayCard(string pathCardId)
        {
            return TravelFacade.CanPlayPathCard(pathCardId);
        }

        /// <summary>
        /// Get card CSS classes
        /// </summary>
        protected string GetPathCardClass(PathCardDTO card)
        {
            List<string> classes = new() { "path-card" };

            if (IsCardDiscovered(card.Id))
            {
                classes.Add("face-up");
            }
            else
            {
                classes.Add("face-down");
            }

            if (!CanPlayCard(card.Id))
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
                Player player = GameFacade.GetPlayer();
                foreach (KeyValuePair<string, int> statReq in card.StatRequirements)
                {
                    if (Enum.TryParse<PlayerStatType>(statReq.Key, true, out PlayerStatType statType))
                    {
                        string statName = GetStatDisplayName(statType);
                        int required = statReq.Value;
                        int current = player.Stats.GetLevel(statType);

                        if (current >= required)
                        {
                            requirements.Add($"{statName} {required}+ ✓");
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
                PlayerStatType.Commerce => "Commerce",
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
                    PlayerStatType.Commerce => "commerce",
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
            if (TravelContext?.Session == null)
                return;

            bool success = TravelFacade.ConfirmRevealedCard();
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
                // Navigate back to location screen
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
        /// </summary>
        protected bool IsCardBeingRevealed(string pathCardId)
        {
            return IsRevealingCard() && GetRevealedCardId() == pathCardId;
        }

        /// <summary>
        /// Check if other cards should be disabled during reveal state
        /// </summary>
        protected bool ShouldDisableCard(string pathCardId)
        {
            // If we're revealing a card and this isn't the revealed card, disable it
            return IsRevealingCard() && GetRevealedCardId() != pathCardId;
        }

        /// <summary>
        /// Refresh travel context from backend
        /// </summary>
        private async Task RefreshTravelContext()
        {
            // Get updated context from TravelFacade
            TravelContext = TravelFacade.GetCurrentTravelContext();
            LoadPathCards();

            // Check if journey completed
            if (TravelContext == null)
            {
                // Journey finished, navigate back to location
                await OnNavigate.InvokeAsync("location");
            }
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