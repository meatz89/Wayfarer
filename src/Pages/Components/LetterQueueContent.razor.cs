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
        [Inject] protected ITimeManager TimeManager { get; set; }

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
            var allObligations = ObligationQueueManager.GetAllObligations();
            var currentTime = TimeManager.GetCurrentTime();
            
            ActiveObligations = allObligations
                .Where(o => o.Deadline > currentTime)
                .OrderBy(o => o.Deadline)
                .ToList();
                
            ExpiredObligations = allObligations
                .Where(o => o.Deadline <= currentTime)
                .ToList();
        }

        protected async Task DeliverLetter(DeliveryObligation letter)
        {
            if (!CanDeliver(letter)) return;
            
            var result = await GameFacade.DeliverLetterAsync(letter.Id);
            if (result.IsSuccess)
            {
                RefreshObligations();
                StateHasChanged();
            }
        }

        protected async Task DisplaceLetter(DeliveryObligation letter)
        {
            // TODO: Implement displacement logic
            Console.WriteLine($"[LetterQueueContent] Displacing letter: {letter.Id}");
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
            var npc = GameFacade.GetNPC(letter.RecipientId);
            return npc?.CurrentLocation ?? "Unknown";
        }

        protected string GetLetterUrgencyClass(DeliveryObligation letter)
        {
            var timeRemaining = letter.Deadline - TimeManager.GetCurrentTime();
            
            if (timeRemaining.TotalHours < 1)
                return "critical";
            if (timeRemaining.TotalHours < 3)
                return "urgent";
            if (timeRemaining.TotalHours < 6)
                return "moderate";
                
            return "normal";
        }

        protected string GetDeadlineClass(DeliveryObligation letter)
        {
            var timeRemaining = letter.Deadline - TimeManager.GetCurrentTime();
            
            if (timeRemaining.TotalHours < 1)
                return "critical";
            if (timeRemaining.TotalHours < 3)
                return "warning";
                
            return "";
        }

        protected string GetTimeRemaining(DeliveryObligation letter)
        {
            var timeRemaining = letter.Deadline - TimeManager.GetCurrentTime();
            
            if (timeRemaining.TotalMinutes < 0)
                return "EXPIRED";
                
            if (timeRemaining.TotalHours < 1)
                return $"{(int)timeRemaining.TotalMinutes} minutes";
                
            return $"{(int)timeRemaining.TotalHours}h {timeRemaining.Minutes}m";
        }

        protected async Task ReturnToPreviousView()
        {
            await OnNavigate.InvokeAsync(ReturnView.ToString().ToLower());
        }
    }
}