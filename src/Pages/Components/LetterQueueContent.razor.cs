using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class LetterQueueContentBase : ComponentBase
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
            var allObligations = ObligationQueueManager.GetActiveObligations();
            
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
            var deliverIntent = new DeliverLetterIntent(letter.Id);
            var result = await GameFacade.ExecuteIntent(deliverIntent);
            if (result)
            {
                RefreshObligations();
                StateHasChanged();
            }
        }

        protected async Task DisplaceLetter(DeliveryObligation letter)
        {
            // Delegate to ObligationQueueManager which handles all displacement logic
            var result = await GameFacade.DisplaceLetterInQueue(letter.Id);
            if (!result)
            {
                Console.WriteLine($"[LetterQueueContent] Failed to displace letter: {letter.Id}");
            }
            StateHasChanged();
        }

        protected bool CanDeliver(DeliveryObligation letter)
        {
            // Check if we're at the recipient's location
            var currentLocation = GameFacade.GetCurrentLocation();
            var recipientLocation = GetRecipientLocation(letter);
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
            var minutesRemaining = letter.DeadlineInMinutes;
            
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
            var minutesRemaining = letter.DeadlineInMinutes;
            
            if (minutesRemaining < 60) // Less than 1 hour
                return "critical";
            if (minutesRemaining < 180) // Less than 3 hours
                return "warning";
                
            return "";
        }

        protected string GetTimeRemaining(DeliveryObligation letter)
        {
            var minutesRemaining = letter.DeadlineInMinutes;
            
            if (minutesRemaining <= 0)
                return "EXPIRED";
                
            if (minutesRemaining < 60)
                return $"{minutesRemaining} minutes";
                
            var hours = minutesRemaining / 60;
            var minutes = minutesRemaining % 60;
            return $"{hours}h {minutes}m";
        }

        protected async Task ReturnToPreviousView()
        {
            await OnNavigate.InvokeAsync(ReturnView.ToString().ToLower());
        }
    }
}