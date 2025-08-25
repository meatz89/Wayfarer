using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Wayfarer.Pages
{
    public enum ScreenMode
    {
        Location,
        Conversation,
        ObligationQueue,
        Travel
    }

    public class ScreenContext
    {
        public ScreenMode Mode { get; set; }
        public Dictionary<string, object> StateData { get; set; } = new();
        public DateTime EnteredAt { get; set; }
    }

    public partial class GameScreenBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected NavigationCoordinator NavigationCoordinator { get; set; }
        [Inject] protected LoadingStateService LoadingStateService { get; set; }
        [Inject] protected TimeBlockAttentionManager AttentionManager { get; set; }
        [Inject] protected ObligationQueueManager ObligationQueueManager { get; set; }

        // Screen Management
        protected ScreenMode CurrentScreen { get; set; } = ScreenMode.Location;
        protected ScreenMode PreviousScreen { get; set; } = ScreenMode.Location;
        protected int ContentVersion { get; set; } = 0;
        
        private Stack<ScreenContext> _navigationStack = new(10);
        private SemaphoreSlim _stateLock = new(1, 1);
        private HashSet<IDisposable> _subscriptions = new();

        // Resources Display
        protected int Coins { get; set; }
        protected int Health { get; set; }
        protected int Food { get; set; }
        protected int Attention { get; set; }
        protected int MaxAttention { get; set; } = 10;

        // Time Display
        protected string CurrentTime { get; set; } = "";
        protected string TimePeriod { get; set; } = "";
        protected string MostUrgentDeadline { get; set; } = "";

        // Location Display
        protected string CurrentLocationPath { get; set; } = "";
        protected string CurrentSpot { get; set; } = "";

        // Navigation State
        protected ConversationContext CurrentConversationContext { get; set; }
        protected int PendingLetterCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[GameScreen] Initializing unified game screen");
            
            // Initialize resources
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
            await RefreshLocationDisplay();
            
            await base.OnInitializedAsync();
        }

        protected async Task RefreshResourceDisplay()
        {
            var player = GameFacade.GetPlayer();
            if (player != null)
            {
                Coins = player.Coins;
                Health = player.Health;
                Food = player.Food;
            }
            
            var attentionState = GameFacade.GetCurrentAttentionState();
            Attention = attentionState.Current;
            MaxAttention = attentionState.Max;
        }

        protected async Task RefreshTimeDisplay()
        {
            var hour = GameFacade.GetCurrentHour();
            var period = hour switch
            {
                >= 6 and < 10 => "Morning",
                >= 10 and < 14 => "Midday",
                >= 14 and < 18 => "Afternoon",
                >= 18 and < 22 => "Evening",
                >= 22 or < 2 => "Night",
                _ => "Deep Night"
            };
            
            CurrentTime = $"{hour % 12:00}:00 {(hour >= 12 ? "PM" : "AM")}";
            TimePeriod = period;
            
            // TODO: Get most urgent deadline from queue
            MostUrgentDeadline = "";
        }

        protected async Task RefreshLocationDisplay()
        {
            var location = GameFacade.GetCurrentLocation();
            var spot = GameFacade.GetCurrentLocationSpot();
            
            if (location != null)
            {
                // Build location breadcrumb path based on location name
                CurrentLocationPath = BuildLocationPath(location.Name);
                
                if (spot != null)
                {
                    CurrentSpot = spot.Name;
                    if (spot.Properties != null && spot.Properties.Any())
                    {
                        CurrentSpot += $" • {string.Join(", ", spot.Properties)}";
                    }
                }
            }
        }
        
        private string BuildLocationPath(string locationName)
        {
            // Build breadcrumb path based on location patterns
            var path = new List<string>();
            
            // Determine district from location name
            var locationLower = locationName?.ToLower() ?? "";
            
            // Add city/ward level
            path.Add("Lower Wards");
            
            // Add district based on location
            if (locationLower.Contains("market") || locationLower.Contains("merchant"))
            {
                path.Add("Market District");
            }
            else if (locationLower.Contains("noble") || locationLower.Contains("lord"))
            {
                path.Add("Noble District");
            }
            else if (locationLower.Contains("temple") || locationLower.Contains("shrine"))
            {
                path.Add("Temple District");
            }
            else if (locationLower.Contains("gate") || locationLower.Contains("guard"))
            {
                path.Add("Gate District");
            }
            else if (locationLower.Contains("river") || locationLower.Contains("dock"))
            {
                path.Add("Riverside District");
            }
            else if (locationLower.Contains("tavern") || locationLower.Contains("kettle"))
            {
                path.Add("Market District"); // Taverns are often in market areas
            }
            
            // Add the specific location
            path.Add(locationName);
            
            return string.Join(" → ", path);
        }

        protected async Task NavigateToScreen(ScreenMode newMode)
        {
            if (!await _stateLock.WaitAsync(5000))
            {
                Console.WriteLine("[GameScreen] State transition timeout");
                return;
            }

            try
            {
                Console.WriteLine($"[GameScreen] Navigating from {CurrentScreen} to {newMode}");
                
                // Save current state
                var currentContext = new ScreenContext
                {
                    Mode = CurrentScreen,
                    StateData = SerializeCurrentState(),
                    EnteredAt = DateTime.UtcNow
                };

                if (_navigationStack.Count >= 10)
                    _navigationStack.TryPop(out _);

                _navigationStack.Push(currentContext);

                // Transition
                PreviousScreen = CurrentScreen;
                CurrentScreen = newMode;
                ContentVersion++;

                await LoadStateForMode(newMode);
                await InvokeAsync(StateHasChanged);
            }
            finally
            {
                _stateLock.Release();
            }
        }

        private Dictionary<string, object> SerializeCurrentState()
        {
            var state = new Dictionary<string, object>();
            
            // Save screen-specific state
            switch (CurrentScreen)
            {
                case ScreenMode.Conversation:
                    state["NpcId"] = CurrentConversationContext?.NpcId;
                    break;
            }
            
            return state;
        }

        private async Task LoadStateForMode(ScreenMode mode)
        {
            // Load screen-specific state
            switch (mode)
            {
                case ScreenMode.Location:
                    await RefreshLocationDisplay();
                    break;
            }
            
            // Always refresh resources
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
        }

        public async Task HandleNavigation(string target)
        {
            Console.WriteLine($"[GameScreen] HandleNavigation: {target}");
            
            switch (target.ToLower())
            {
                case "location":
                    await NavigateToScreen(ScreenMode.Location);
                    break;
                case "obligationqueue":
                case "obligations":
                case "queue":
                    await NavigateToScreen(ScreenMode.ObligationQueue);
                    break;
                case "travel":
                    await NavigateToScreen(ScreenMode.Travel);
                    break;
            }
        }

        public async Task StartConversation(string npcId, ConversationType type)
        {
            CurrentConversationContext = await GameFacade.CreateConversationContext(npcId, type);
            if (CurrentConversationContext != null && CurrentConversationContext.IsValid)
            {
                CurrentScreen = ScreenMode.Conversation;
                ContentVersion++; // Force re-render
                StateHasChanged();
            }
            else if (CurrentConversationContext != null)
            {
                // Show error message
                Console.WriteLine($"[GameScreen] Cannot start conversation: {CurrentConversationContext.ErrorMessage}");
            }
        }

        protected void HandleNavigationEvent(string eventType, object data)
        {
            Console.WriteLine($"[GameScreen] Navigation event: {eventType}");
            
            switch (eventType)
            {
                case "ConversationEnded":
                    _ = NavigateToScreen(ScreenMode.Location);
                    break;
                    
                case "TravelCompleted":
                    _ = NavigateToScreen(ScreenMode.Location);
                    break;
            }
        }

        protected async Task HandleConversationEnd()
        {
            Console.WriteLine("[GameScreen] Conversation ended");
            CurrentConversationContext = null;
            await NavigateToScreen(ScreenMode.Location);
        }

        protected async Task HandleTravelRoute(string routeId)
        {
            Console.WriteLine($"[GameScreen] Travel route selected: {routeId}");
            // Execute travel via intent system
            var travelIntent = new TravelIntent(routeId);
            await GameFacade.ExecuteIntent(travelIntent);
            await NavigateToScreen(ScreenMode.Location);
        }

        protected string GetCurrentLocation()
        {
            var location = GameFacade.GetCurrentLocation();
            return location?.Name ?? "Unknown";
        }

        protected async Task RefreshUI()
        {
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
            await RefreshLocationDisplay();
            await InvokeAsync(StateHasChanged);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription?.Dispose();
            }
            
            _stateLock?.Dispose();
        }
    }
}