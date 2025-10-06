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
/// - Navigation state managed through GameFacade (singleton, persists across renders)
/// - ConversationContext created atomically before navigation (after interactive)
/// - All state mutations go through GameFacade which has idempotence protection
/// </summary>
public partial class GameScreenBase : ComponentBase, IAsyncDisposable
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected LoadingStateService LoadingStateService { get; set; }
    [Inject] protected ObligationQueueManager ObligationQueueManager { get; set; }
    [Inject] protected InvestigationActivity InvestigationActivity { get; set; }

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
    public int Hunger { get; set; }

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
    protected ObstacleContext CurrentObstacleContext { get; set; }
    public MentalSession MentalSession { get; set; }
    public PhysicalSession PhysicalSession { get; set; }
    protected int PendingLetterCount { get; set; }
    public string CurrentDeckViewerNpcId { get; set; } // For dev mode deck viewer

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("[GameScreen] OnInitializedAsync started");

        try
        {
            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
            await RefreshLocationDisplay();
            await base.OnInitializedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameScreen] ERROR in OnInitializedAsync: {ex.Message}");
            Console.WriteLine($"[GameScreen] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task RefreshResourceDisplay()
    {
        if (GameFacade == null)
        {
            Console.WriteLine("[GameScreen.RefreshResourceDisplay] GameFacade is null, skipping");
            return;
        }

        Player? player = GameFacade.GetPlayer();
        if (player != null)
        {
            Coins = player.Coins;
            Health = player.Health;
            Hunger = player.Hunger;
        }

    }

    protected async Task RefreshTimeDisplay()
    {
        // Get segment display from time facade
        TimeInfo timeInfo = GameFacade.GetTimeInfo();

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

            MostUrgentDeadline = mostUrgent != null && mostUrgent.DeadlineInSegments_Display > 0
                ? $"Next deadline: {mostUrgent.DeadlineInSegments_Display} seg - {mostUrgent.SenderName} → {mostUrgent.RecipientName}"
                : "";
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
        // Get the current location directly from GameFacade by ID
        Location location = GameFacade.GetCurrentLocation();
        if (location == null) return locationName;

        // Get the district from the location's district ID
        if (string.IsNullOrEmpty(location.District))
            return location.Name;

        District district = GameFacade.GetDistrictById(location.District);
        if (district == null)
            return location.Name;

        // Get the region from the district
        Region region = GameFacade.GetRegionForDistrict(district.Id);

        // Build the breadcrumb path
        List<string> path = new List<string>();

        if (region != null)
            path.Add(region.Name);

        path.Add(district.Name);
        path.Add(location.Name);

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

    protected bool IsDevelopmentMode()
    {
        // Check if running in development environment
        // This could be based on configuration, environment variable, or build mode
#if DEBUG
        return true;
#else
        return false;
#endif
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

            Console.WriteLine($"[GameScreen] CurrentScreen set to: {CurrentScreen}, ContentVersion: {ContentVersion}");

            await LoadStateForMode(newMode);
            Console.WriteLine($"[GameScreen] About to call StateHasChanged, CurrentScreen is: {CurrentScreen}");
            await InvokeAsync(StateHasChanged);
            Console.WriteLine($"[GameScreen] StateHasChanged called, CurrentScreen is: {CurrentScreen}");
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

    public async Task StartConversation(string npcId, string requestId)
    {
        CurrentConversationContext = await GameFacade.CreateConversationContext(npcId, requestId);

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

    protected async Task HandleConversationEnd()
    {
        Console.WriteLine("[GameScreen] Conversation ended");
        CurrentConversationContext = null;

        // Always refresh UI after conversation ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Check for investigation results before returning to location
        await CheckForInvestigationResults();

        // Special case: allow navigation from conversation when it ends properly
        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task StartMentalSession(string engagementTypeId)
    {
        Console.WriteLine($"[GameScreen] Starting Mental session: {engagementTypeId}");

        MentalSession = GameFacade.StartMentalSession(engagementTypeId);

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (MentalSession != null)
        {
            CurrentScreen = ScreenMode.Mental;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine("[GameScreen] Failed to start Mental session");
        }
    }

    public async Task HandleMentalEnd()
    {
        Console.WriteLine("[GameScreen] Mental session ended");
        MentalSession = null;

        // Always refresh UI after mental session ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Check for investigation results before returning to location
        await CheckForInvestigationResults();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task StartPhysicalSession(string engagementTypeId)
    {
        Console.WriteLine($"[GameScreen] Starting Physical session: {engagementTypeId}");

        PhysicalSession = GameFacade.StartPhysicalSession(engagementTypeId);

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (PhysicalSession != null)
        {
            CurrentScreen = ScreenMode.Physical;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine("[GameScreen] Failed to start Physical session");
        }
    }

    public async Task HandlePhysicalEnd()
    {
        Console.WriteLine("[GameScreen] Physical session ended");
        PhysicalSession = null;

        // Always refresh UI after physical session ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Check for investigation results before returning to location
        await CheckForInvestigationResults();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task HandleTravelRoute(string routeId)
    {
        Console.WriteLine($"[GameScreen] Travel route selected: {routeId}");

        // V2 OBSTACLE SYSTEM: Check for obstacles before travel
        RouteOption route = GameFacade.GetRouteById(routeId);
        if (route != null)
        {
            TravelObstacle obstacle = GameFacade.CheckForObstacle(route);
            if (obstacle != null)
            {
                Console.WriteLine($"[GameScreen] Obstacle encountered: {obstacle.Id}");
                // Start obstacle encounter instead of completing travel
                await StartObstacle(obstacle.Id, route);
                return; // Don't execute travel yet - wait for obstacle resolution
            }
        }

        // No obstacle - execute travel via intent system
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

    public async Task StartObstacle(string obstacleId, RouteOption route = null)
    {
        Console.WriteLine($"[GameScreen] Starting obstacle: {obstacleId}");
        CurrentObstacleContext = await GameFacade.CreateObstacleContext(obstacleId, route);

        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (CurrentObstacleContext != null)
        {
            CurrentScreen = ScreenMode.Obstacle;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine("[GameScreen] Failed to create obstacle context");
        }
    }

    protected async Task HandleObstacleEnd(bool success)
    {
        Console.WriteLine($"[GameScreen] Obstacle ended - Success: {success}");

        // If obstacle was successfully overcome, complete the pending travel
        if (success && CurrentObstacleContext?.Route != null)
        {
            string routeId = CurrentObstacleContext.Route.Id;
            Console.WriteLine($"[GameScreen] Completing travel after obstacle success: {routeId}");

            // Clear obstacle context before travel
            CurrentObstacleContext = null;

            // Execute travel via intent system
            TravelIntent travelIntent = new TravelIntent(routeId);
            await GameFacade.ProcessIntent(travelIntent);

            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
            await RefreshLocationDisplay();

            await NavigateToScreen(ScreenMode.Location);
        }
        else
        {
            // Failed obstacle or no route - just return to location
            Console.WriteLine("[GameScreen] Obstacle failed or no route - returning to location");
            CurrentObstacleContext = null;

            await RefreshResourceDisplay();
            await RefreshTimeDisplay();
            await RefreshLocationDisplay();

            await NavigateToScreen(ScreenMode.Location);
        }
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

    // Time/Date Display Helper Methods
    protected string GetDayDisplay()
    {
        TimeInfo timeInfo = GameFacade.GetTimeInfo();
        int journeyDay = timeInfo.CurrentDay;

        string[] dayNames = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        int dayOfWeek = (journeyDay - 1) % 7;

        return $"{dayNames[dayOfWeek]} - Day {journeyDay} of Journey";
    }

    protected string GetTimePeriodName()
    {
        TimeInfo timeInfo = GameFacade.GetTimeInfo();
        return timeInfo.CurrentTimeBlock.ToString().ToUpper();
    }

    protected List<string> GetSegmentDots()
    {
        List<string> dots = new List<string>();
        int currentSegment = GetCurrentSegmentInPeriod();
        int totalSegments = GetTotalSegmentsInPeriod();

        for (int i = 1; i <= totalSegments; i++)
        {
            if (i < currentSegment)
                dots.Add("filled");
            else if (i == currentSegment)
                dots.Add("current");
            else
                dots.Add("empty");
        }

        return dots;
    }

    protected int GetCurrentSegmentInPeriod()
    {
        // Get the actual segment display from TimeInfo
        // The SegmentDisplay is formatted like "MORNING ●●○○ [2/4]"
        // We need to extract the current segment number
        TimeInfo timeInfo = GameFacade.GetTimeInfo();
        string segmentDisplay = timeInfo.SegmentDisplay;

        // Extract the current segment from the display format "[current/total]"
        int startIndex = segmentDisplay.LastIndexOf('[') + 1;
        int endIndex = segmentDisplay.IndexOf('/', startIndex);

        if (startIndex > 0 && endIndex > startIndex)
        {
            string currentStr = segmentDisplay.Substring(startIndex, endIndex - startIndex);
            if (int.TryParse(currentStr, out int current))
                return current;
        }

        return 1; // Default to 1 if parsing fails
    }

    protected int GetTotalSegmentsInPeriod()
    {
        return GameFacade?.GetSegmentsInCurrentPeriod() ?? 4;
    }

    protected string GetStaminaDisplay()
    {
        // For travel, get from the TravelManager if there's an active session
        // Otherwise show player's base stamina
        Player player = GameFacade.GetPlayer();
        if (player != null)
        {
            // Default stamina when not traveling
            int currentStamina = player.Stamina;
            int maxStamina = player.MaxStamina;
            return $"{currentStamina}/{maxStamina}";
        }
        return "0/0";
    }

    protected int GetDeadlineSegments()
    {
        // Get the most urgent deadline from the letter queue
        LetterQueueViewModel queueVM = GameFacade.GetLetterQueue();
        if (queueVM?.QueueSlots != null)
        {
            foreach (QueueSlotViewModel slot in queueVM.QueueSlots)
            {
                if (slot.IsOccupied && slot.DeliveryObligation != null)
                {
                    return slot.DeliveryObligation.DeadlineInSegments_Display;
                }
            }
        }
        return 0;
    }

    // Discovery Journal
    protected bool _showJournal = false;

    protected void ToggleJournal()
    {
        _showJournal = !_showJournal;
        StateHasChanged();
    }

    // Investigation Modals
    protected bool _showInvestigationProgressModal = false;
    protected bool _showInvestigationCompleteModal = false;
    protected InvestigationProgressResult _investigationProgressResult;
    protected InvestigationCompleteResult _investigationCompleteResult;

    protected async Task CheckForInvestigationResults()
    {
        InvestigationProgressResult progressResult = InvestigationActivity.GetAndClearPendingProgressResult();
        if (progressResult != null)
        {
            _investigationProgressResult = progressResult;
            _showInvestigationProgressModal = true;
            await InvokeAsync(StateHasChanged);
            return;
        }

        InvestigationCompleteResult completeResult = InvestigationActivity.GetAndClearPendingCompleteResult();
        if (completeResult != null)
        {
            _investigationCompleteResult = completeResult;
            _showInvestigationCompleteModal = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async Task CloseInvestigationProgressModal()
    {
        _showInvestigationProgressModal = false;
        _investigationProgressResult = null;
        await InvokeAsync(StateHasChanged);

        await CheckForInvestigationResults();
    }

    protected async Task CloseInvestigationCompleteModal()
    {
        _showInvestigationCompleteModal = false;
        _investigationCompleteResult = null;
        await InvokeAsync(StateHasChanged);
    }
}

public enum ScreenMode
{
    Location,
    Conversation,
    Exchange,
    ObligationQueue,
    Travel,
    DeckViewer, // Dev mode screen for viewing NPC decks
    Obstacle, // V2 Travel Obstacles
    Mental, // Mental tactical engagements (investigation/problem-solving)
    Physical // Physical tactical engagements (challenges/obstacles)
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
    public string RequestId { get; set; }
    public string SelectedCardId { get; set; }
    public int? SelectedObligationIndex { get; set; }
}
