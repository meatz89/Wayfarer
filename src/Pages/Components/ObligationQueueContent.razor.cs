using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Obligation queue screen component that displays and manages the player's letter delivery obligations.
    /// 
    /// CRITICAL: BLAZOR SERVERPRERENDERED CONSEQUENCES
    /// ================================================
    /// This component renders TWICE due to ServerPrerendered mode:
    /// 1. During server-side prerendering (static HTML generation)
    /// 2. After establishing interactive SignalR connection
    /// 
    /// ARCHITECTURAL PRINCIPLES:
    /// - OnParametersSetAsync() runs TWICE - RefreshObligations is read-only and safe
    /// - Queue state maintained in ObligationQueueManager singleton (persists)
    /// - Deliver/reorganize actions only happen after interactive connection
    /// - All mutations go through GameFacade.ProcessIntent (idempotent)
    /// 
    /// IMPLEMENTATION REQUIREMENTS:
    /// - RefreshObligations() fetches display data only (no mutations)
    /// - Queue operations validated by manager before execution
    /// - Delivery attempts create PlayerIntent objects (processed by facade)
    /// - Position 1 enforcement handled by backend (not UI)
    /// </summary>
    public class ObligationQueueContentBase : ComponentBase
    {
        [Parameter] public EventCallback<string> OnNavigate { get; set; }
        [Parameter] public ScreenMode ReturnView { get; set; } = ScreenMode.Location;

        [Inject] protected ObligationQueueManager ObligationQueueManager { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected TimeManager TimeManager { get; set; }

        protected List<DeliveryObligation> ActiveObligations { get; set; } = new();
        protected List<DeliveryObligation> ExpiredObligations { get; set; } = new();
        protected string PreviousViewName => ReturnView.ToString();

        protected override async Task OnInitializedAsync()
        {
            RefreshObligations();
        }

        protected override async Task OnParametersSetAsync()
        {
            RefreshObligations();
        }

        private void RefreshObligations()
        {
            DeliveryObligation[] allObligations = ObligationQueueManager.GetActiveObligations();

            ActiveObligations = allObligations
                .Where(o => o.DeadlineInMinutes > 0)
                .OrderBy(o => o.DeadlineInMinutes)
                .ToList();

            ExpiredObligations = allObligations
                .Where(o => o.DeadlineInMinutes <= 0)
                .ToList();
        }

        protected async Task DeliverLetter(DeliveryObligation letter)
        {
            if (!CanDeliver(letter)) return;

            // Use the ExecuteIntent system to deliver the letter
            DeliverLetterIntent deliverIntent = new DeliverLetterIntent(letter.Id);
            bool result = await GameFacade.ProcessIntent(deliverIntent);
            if (result)
            {
                RefreshObligations();
                StateHasChanged();
            }
        }

        protected async Task DisplaceLetterToPosition(DeliveryObligation letter, int targetPosition)
        {
            // Use the new token-burning displacement system
            bool result = await GameFacade.DisplaceObligation(letter.Id, targetPosition);
            if (result)
            {
                RefreshObligations();
                StateHasChanged();
            }
        }

        protected QueueDisplacementPreview GetDisplacementPreview(DeliveryObligation letter, int targetPosition)
        {
            return GameFacade.GetDisplacementPreview(letter.Id, targetPosition);
        }

        protected int GetLetterPosition(DeliveryObligation letter)
        {
            return ObligationQueueManager.GetQueuePosition(letter);
        }

        protected string GetDisplacementButtonClass(QueueDisplacementPreview preview)
        {
            if (preview.TotalTokenCost == 0)
                return "free-displacement";
            else if (preview.TotalTokenCost <= 2)
                return "low-cost-displacement";
            else if (preview.TotalTokenCost <= 5)
                return "medium-cost-displacement";
            else
                return "high-cost-displacement";
        }

        protected bool CanDeliver(DeliveryObligation letter)
        {
            // Check if we're at the recipient's location
            Location currentLocation = GameFacade.GetCurrentLocation();
            string recipientLocation = GetRecipientLocation(letter);
            return currentLocation?.Name == recipientLocation;
        }

        protected bool CanDisplace(DeliveryObligation letter)
        {
            // Can displace if there are other letters in queue
            return ActiveObligations.Count > 1;
        }

        protected string GetRecipientLocation(DeliveryObligation letter)
        {
            // For now, return the recipient name as the location
            // The actual location would need to be resolved from the NPC repository
            return letter.RecipientName ?? "Unknown";
        }

        protected string GetLetterUrgencyClass(DeliveryObligation letter)
        {
            int minutesRemaining = letter.DeadlineInMinutes;

            if (minutesRemaining < 60) // Less than 1 hour
                return "critical";
            if (minutesRemaining < 180) // Less than 3 hours
                return "urgent";
            if (minutesRemaining < 360) // Less than 6 hours
                return "moderate";

            return "normal";
        }

        protected string GetDeadlineClass(DeliveryObligation letter)
        {
            int minutesRemaining = letter.DeadlineInMinutes;

            if (minutesRemaining < 60) // Less than 1 hour
                return "critical";
            if (minutesRemaining < 180) // Less than 3 hours
                return "warning";

            return "";
        }

        protected string GetTimeRemaining(DeliveryObligation letter)
        {
            int minutesRemaining = letter.DeadlineInMinutes;

            if (minutesRemaining <= 0)
                return "EXPIRED";

            if (minutesRemaining < 60)
                return $"{minutesRemaining} minutes";

            int hours = minutesRemaining / 60;
            int minutes = minutesRemaining % 60;
            return $"{hours}h {minutes}m";
        }

        protected async Task ReturnToPreviousView()
        {
            await OnNavigate.InvokeAsync(ReturnView.ToString().ToLower());
        }

        protected async Task ViewLetterDetails(DeliveryObligation letter)
        {
            // For now, just log the details. In future, could open a modal
            Console.WriteLine($"[ObligationQueue] Viewing details for letter {letter.Id}");
            await Task.CompletedTask;
        }
    }
}