using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wayfarer.Pages
{
    public enum ScreenMode
    {
        Location,
        Conversation,
        Exchange,
        ObligationQueue,
        Travel,
        DeckViewer // Dev mode screen for viewing NPC decks
    }

    public class ScreenContext
    {
        public ScreenMode Mode { get; set; }
        public ScreenStateData StateData { get; set; } = new();
        public DateTime EnteredAt { get; set; }
    }

    /// <summary>
    /// Strongly typed state data for screen transitions
    /// </summary>
    public class ScreenStateData
    {
        public string NpcId { get; set; }
        public string LocationId { get; set; }
        public string TravelDestination { get; set; }
        public ConversationType? ConversationType { get; set; }
        public string SelectedCardId { get; set; }
        public int? SelectedObligationIndex { get; set; }
    }

    /// <summary>
    /// Main game screen component that manages the unified UI with fixed header/footer and dynamic content area.
    /// 
    /// CRITICAL: BLAZOR SERVERPRERENDERED CONSEQUENCES
    /// ================================================
    /// This component renders TWICE due to ServerPrerendered mode:
    /// 1. During server-side prerendering (static HTML generation)
    /// 2. After establishing interactive SignalR connection
    /// 
    /// ARCHITECTURAL PRINCIPLES:
    /// - OnInitializedAsync() runs TWICE - all initialization MUST be idempotent
    /// - RefreshResourceDisplay/RefreshTimeDisplay are read-only and safe to run twice
    /// - Services are Singletons and persist state across both renders
    /// - User actions (button clicks, navigation) only occur after interactive phase
    /// 
    /// IMPLEMENTATION REQUIREMENTS:
    /// - Resource/time refresh operations are read-only (safe for double execution)
    /// - Navigation state managed through GameFacade (singleton, persists across renders)
    /// - ConversationContext created atomically before navigation (after interactive)
    /// - All state mutations go through GameFacade which has idempotence protection
    /// </summary>
    public partial class GameScreenBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected LoadingStateService LoadingStateService { get; set; }
        [Inject] protected TimeBlockAttentionManager AttentionManager { get; set; }
        [Inject] protected ObligationQueueManager ObligationQueueManager { get; set; }

        public GameScreenBase()
        {
            Console.WriteLine("[GameScreenBase] Constructor called");
        }

        // Screen Management
        protected ScreenMode CurrentScreen { get; set; } = ScreenMode.Location;
        protected ScreenMode PreviousScreen { get; set; } = ScreenMode.Location;
        protected int ContentVersion { get; set; } = 0;
        protected bool IsTransitioning { get; set; } = false;

        private Stack<ScreenContext> _navigationStack = new(10);
        private SemaphoreSlim _stateLock = new(1, 1);
        private HashSet<IDisposable> _subscriptions = new();

        // Resources Display - Made public for child components to access for Perfect Information principle
        public int Coins { get; set; }
        public int Health { get; set; }
        public int Food { get; set; }
        public int Attention { get; set; }
        public int MaxAttention { get; set; } = 10;

        // Time Display - Made public for child components to access for Perfect Information principle
        public string CurrentTime { get; set; } = "";
        public string TimePeriod { get; set; } = "";
        public string MostUrgentDeadline { get; set; } = "";

        // Location Display
        protected string CurrentLocationPath { get; set; } = "";
        protected string CurrentSpot { get; set; } = "";

        // Navigation State
        protected ConversationContextBase CurrentConversationContext { get; set; }
        protected ExchangeContext CurrentExchangeContext { get; set; }
        protected int PendingLetterCount { get; set; }
        public string CurrentDeckViewerNpcId { get; set; } // For dev mode deck viewer

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[GameScreen] OnInitializedAsync started");

            try
            {
                Console.WriteLine("[GameScreen] Calling RefreshResourceDisplay...");
                await RefreshResourceDisplay();
                Console.WriteLine("[GameScreen] RefreshResourceDisplay completed");

                Console.WriteLine("[GameScreen] Calling RefreshTimeDisplay...");
                await RefreshTimeDisplay();
                Console.WriteLine("[GameScreen] RefreshTimeDisplay completed");

                Console.WriteLine("[GameScreen] Calling RefreshLocationDisplay...");
                await RefreshLocationDisplay();
                Console.WriteLine("[GameScreen] RefreshLocationDisplay completed");

                Console.WriteLine("[GameScreen] Calling base.OnInitializedAsync...");
                await base.OnInitializedAsync();
                Console.WriteLine("[GameScreen] base.OnInitializedAsync completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameScreen] ERROR in OnInitializedAsync: {ex.Message}");
                Console.WriteLine($"[GameScreen] Stack trace: {ex.StackTrace}");
                throw;
            }

            Console.WriteLine("[GameScreen] OnInitializedAsync completed");
        }

        public async Task RefreshResourceDisplay()
        {
            Console.WriteLine("[GameScreen.RefreshResourceDisplay] Starting...");
            Console.WriteLine($"[GameScreen.RefreshResourceDisplay] GameFacade null? {GameFacade == null}");

            if (GameFacade == null)
            {
                Console.WriteLine("[GameScreen.RefreshResourceDisplay] GameFacade is null, skipping");
                return;
            }

            Console.WriteLine("[GameScreen.RefreshResourceDisplay] Getting player...");
            Player? player = GameFacade.GetPlayer();
            Console.WriteLine($"[GameScreen.RefreshResourceDisplay] Player null? {player == null}");

            if (player != null)
            {
                Coins = player.Coins;
                Health = player.Health;
                Food = player.Hunger;
                Console.WriteLine($"[GameScreen.RefreshResourceDisplay] Player resources: Coins={Coins}, Health={Health}, Food={Food}");
            }

            Console.WriteLine("[GameScreen.RefreshResourceDisplay] Getting attention state...");
            AttentionStateInfo attentionState = GameFacade.GetCurrentAttentionState();
            Attention = attentionState.Current;
            MaxAttention = attentionState.Max;
            Console.WriteLine($"[GameScreen.RefreshResourceDisplay] Attention: {Attention}/{MaxAttention}");

            Console.WriteLine("[GameScreen.RefreshResourceDisplay] Completed");
        }

        protected async Task RefreshTimeDisplay()
        {
            // Get segment display from time facade
            var timeInfo = GameFacade.GetTimeInfo();
            
            // Use segment display format: "AFTERNOON ●●○○ [2/4]"
            CurrentTime = timeInfo.SegmentDisplay;
            TimePeriod = timeInfo.CurrentTimeBlock.ToString();

            // Get most urgent deadline from queue
            LetterQueueViewModel queueVM = GameFacade.GetLetterQueue();
            if (queueVM?.QueueSlots != null)
            {
                LetterViewModel mostUrgent = null;
                foreach (QueueSlotViewModel slot in queueVM.QueueSlots)
                {
                    if (slot.IsOccupied && slot.DeliveryObligation != null)
                    {
                        if (mostUrgent == null || slot.DeliveryObligation.DeadlineInSegments_Display < mostUrgent.DeadlineInSegments_Display)
                        {
                            mostUrgent = slot.DeliveryObligation;
                        }
                    }
                }

                if (mostUrgent != null && mostUrgent.DeadlineInSegments_Display > 0)
                {
                    MostUrgentDeadline = $"Next deadline: {mostUrgent.DeadlineInSegments_Display} seg - {mostUrgent.SenderName} → {mostUrgent.RecipientName}";
                }
                else
                {
                    MostUrgentDeadline = "";
                }
            }
            else
            {
                MostUrgentDeadline = "";
            }
        }

        protected async Task RefreshLocationDisplay()
        {
            Location location = GameFacade.GetCurrentLocation();
            LocationSpot spot = GameFacade.GetCurrentLocationSpot();

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
            List<string> path = new List<string>();

            // Determine district from location name
            string locationLower = locationName?.ToLower() ?? "";

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

        protected bool CanNavigateTo(ScreenMode targetMode)
        {
            // Can't navigate while already transitioning
            if (IsTransitioning) return false;

            // Can't navigate to same screen
            if (CurrentScreen == targetMode) return false;

            // Conversation-specific rules
            if (CurrentScreen == ScreenMode.Conversation)
            {
                // Can only exit conversation through proper ending
                // This is handled by HandleConversationEnd
                return false;
            }

            // Travel screen rules
            if (CurrentScreen == ScreenMode.Travel)
            {
                // Can cancel travel to go back to location
                return targetMode == ScreenMode.Location;
            }

            // All other transitions are allowed
            return true;
        }

        public async Task NavigateToScreen(ScreenMode newMode)
        {
            if (!CanNavigateTo(newMode))
            {
                Console.WriteLine($"[GameScreen] Cannot navigate from {CurrentScreen} to {newMode}");
                return;
            }

            if (!await _stateLock.WaitAsync(5000))
            {
                Console.WriteLine("[GameScreen] State transition timeout");
                return;
            }

            try
            {
                IsTransitioning = true;
                Console.WriteLine($"[GameScreen] Navigating from {CurrentScreen} to {newMode}");

                // Save current state
                ScreenContext currentContext = new ScreenContext
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
                IsTransitioning = false;
                _stateLock.Release();
            }
        }

        private ScreenStateData SerializeCurrentState()
        {
            ScreenStateData state = new ScreenStateData();

            // Save screen-specific state
            switch (CurrentScreen)
            {
                case ScreenMode.Conversation:
                    state.NpcId = CurrentConversationContext?.NpcId;
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

            // Always refresh UI after GameFacade action
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();

            if (CurrentConversationContext != null && CurrentConversationContext.IsValid)
            {
                CurrentScreen = ScreenMode.Conversation;
                ContentVersion++; // Force re-render
                await InvokeAsync(StateHasChanged);
            }
            else if (CurrentConversationContext != null)
            {
                // Show error message
                Console.WriteLine($"[GameScreen] Cannot start conversation: {CurrentConversationContext.ErrorMessage}");
                await InvokeAsync(StateHasChanged);
            }
        }

        public async Task NavigateToQueue()
        {
            await NavigateToScreen(ScreenMode.ObligationQueue);
        }

        public async Task StartExchange(string npcId)
        {
            CurrentExchangeContext = await GameFacade.CreateExchangeContext(npcId);

            // Always refresh UI after GameFacade action
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();

            if (CurrentExchangeContext != null)
            {
                CurrentScreen = ScreenMode.Exchange;
                ContentVersion++; // Force re-render
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                Console.WriteLine("[GameScreen] Failed to create exchange context");
            }
        }

        protected async Task HandleExchangeEnd()
        {
            Console.WriteLine("[GameScreen] Exchange ended");
            CurrentExchangeContext = null;

            // Always refresh UI after exchange ends
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();

            await NavigateToScreen(ScreenMode.Location);
        }

        public async Task ReturnToLocation()
        {
            await NavigateToScreen(ScreenMode.Location);
        }

        public async Task NavigateToDeckViewer(string npcId)
        {
            Console.WriteLine($"[GameScreen] Navigating to deck viewer for NPC: {npcId}");

            // Store the NPC ID for the deck viewer to use
            CurrentDeckViewerNpcId = npcId;

            // Store the NPC ID in the context for the deck viewer to use
            ScreenContext context = new ScreenContext
            {
                Mode = ScreenMode.DeckViewer,
                EnteredAt = DateTime.Now,
                StateData = new ScreenStateData
                {
                    NpcId = npcId
                }
            };
            _navigationStack.Push(context);

            await NavigateToScreen(ScreenMode.DeckViewer);
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

            // Always refresh UI after conversation ends
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
            await RefreshLocationDisplay();

            // Special case: allow navigation from conversation when it ends properly
            CurrentScreen = ScreenMode.Location;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }

        protected async Task HandleTravelRoute(string routeId)
        {
            Console.WriteLine($"[GameScreen] Travel route selected: {routeId}");
            // Execute travel via intent system
            TravelIntent travelIntent = new TravelIntent(routeId);
            await GameFacade.ProcessIntent(travelIntent);

            // Refresh UI after action
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
            await RefreshLocationDisplay();

            // Force UI update to show the new time
            await InvokeAsync(StateHasChanged);

            await NavigateToScreen(ScreenMode.Location);
        }

        protected string GetCurrentLocation()
        {
            Location location = GameFacade.GetCurrentLocation();
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
            foreach (IDisposable subscription in _subscriptions)
            {
                subscription?.Dispose();
            }

            _stateLock?.Dispose();
        }
    }
}