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
    public string CurrentTime { get; set; }
    public string TimePeriod { get; set; }

    // Venue Display
    protected string CurrentLocationPath { get; set; }
    protected string CurrentSpot { get; set; }

    // Navigation State
    protected ExchangeContext CurrentExchangeContext { get; set; }
    protected ObstacleContext CurrentObstacleContext { get; set; }
    protected SocialChallengeContext CurrentSocialContext { get; set; }
    protected MentalChallengeContext CurrentMentalContext { get; set; }
    protected PhysicalChallengeContext CurrentPhysicalContext { get; set; }
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
    }

    protected async Task RefreshLocationDisplay()
    {
        Venue venue = GameFacade.GetCurrentLocation();
        Location location = GameFacade.GetCurrentLocationSpot();

        if (venue != null)
        {
            // Build venue breadcrumb path based on venue name
            CurrentLocationPath = BuildLocationPath(venue.Name);

            if (location != null)
            {
                CurrentSpot = location.Name;
                if (location.Properties != null && location.Properties.Any())
                {
                    CurrentSpot += $" • {string.Join(", ", location.Properties)}";
                }
            }
        }
    }

    private string BuildLocationPath(string locationName)
    {
        // Get the current venue directly from GameFacade by ID
        Venue venue = GameFacade.GetCurrentLocation();
        if (venue == null) return locationName;

        // Get the district from the venue's district ID
        if (string.IsNullOrEmpty(venue.District))
            return venue.Name;

        District district = GameFacade.GetDistrictById(venue.District);
        if (district == null)
            return venue.Name;

        // Get the region from the district
        Region region = GameFacade.GetRegionForDistrict(district.Id);

        // Build the breadcrumb path
        List<string> path = new List<string>();

        if (region != null)
            path.Add(region.Name);

        path.Add(district.Name);
        path.Add(venue.Name);

        return string.Join(" → ", path);
    }

    protected bool CanNavigateTo(ScreenMode targetMode)
    {
        // Can't navigate while already transitioning
        if (IsTransitioning) return false;

        // Can't navigate to same screen
        if (CurrentScreen == targetMode) return false;

        // Conversation-specific rules
        if (CurrentScreen == ScreenMode.SocialChallenge
            || CurrentScreen == ScreenMode.PhysicalChallenge)
        {
            // Can only exit challenges through proper ending
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
            case ScreenMode.SocialChallenge:
                state.NpcId = CurrentSocialContext?.NpcId;
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
            case "travel":
                await NavigateToScreen(ScreenMode.Travel);
                break;
        }
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

    public async Task StartConversationSession(string npcId, string goalId)
    {
        CurrentSocialContext = await GameFacade.CreateConversationContext(npcId, goalId);

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (CurrentSocialContext != null && CurrentSocialContext.IsValid)
        {
            CurrentScreen = ScreenMode.SocialChallenge;
            ContentVersion++; // Force re-render
            await InvokeAsync(StateHasChanged);
        }
        else if (CurrentSocialContext != null)
        {
            // Show error message
            Console.WriteLine($"[GameScreen] Cannot start conversation: {CurrentSocialContext.ErrorMessage}");
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async Task HandleConversationEnd()
    {
        Console.WriteLine("[GameScreen] Conversation ended");
        CurrentSocialContext = null;

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

    public async Task StartMentalSession(string deckId, string locationSpotId, string goalId, string investigationId)
    {
        Console.WriteLine($"[GameScreen] Starting Mental session: {goalId}");

        MentalSession session = GameFacade.StartMentalSession(deckId, locationSpotId, goalId, investigationId);

        // Create context parallel to Social pattern
        CurrentMentalContext = new MentalChallengeContext
        {
            IsValid = session != null,
            ErrorMessage = session == null ? "Failed to start Mental session" : string.Empty,
            DeckId = deckId,
            Session = session,
            Venue = GameFacade.GetCurrentLocation(),
            LocationName = GameFacade.GetCurrentLocation()?.Name ?? "Unknown"
        };

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (CurrentMentalContext.IsValid)
        {
            CurrentScreen = ScreenMode.MentalChallenge;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine($"[GameScreen] {CurrentMentalContext.ErrorMessage}");
        }
    }

    public async Task HandleMentalEnd()
    {
        Console.WriteLine("[GameScreen] Mental session ended");
        CurrentMentalContext = null;

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

    public async Task StartPhysicalSession(string deckId, string locationSpotId, string goalId, string investigationId)
    {
        Console.WriteLine($"[GameScreen] Starting Physical session: {deckId}");

        PhysicalSession session = GameFacade.StartPhysicalSession(deckId, locationSpotId, goalId, investigationId);

        // Create context parallel to Social pattern
        CurrentPhysicalContext = new PhysicalChallengeContext
        {
            IsValid = session != null,
            ErrorMessage = session == null ? "Failed to start Physical session" : string.Empty,
            DeckId = deckId,
            Session = session,
            Venue = GameFacade.GetCurrentLocation(),
            LocationName = GameFacade.GetCurrentLocation()?.Name ?? "Unknown"
        };

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (CurrentPhysicalContext.IsValid)
        {
            CurrentScreen = ScreenMode.PhysicalChallenge;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            Console.WriteLine($"[GameScreen] {CurrentPhysicalContext.ErrorMessage}");
        }
    }

    public async Task HandlePhysicalEnd()
    {
        Console.WriteLine("[GameScreen] Physical session ended");
        CurrentPhysicalContext = null;

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

        RouteOption route = GameFacade.GetRouteById(routeId);

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
        Venue venue = GameFacade.GetCurrentLocation();
        return venue?.Name ?? "Unknown";
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

    // Discovery Journal
    protected bool _showJournal = false;

    protected void ToggleJournal()
    {
        _showJournal = !_showJournal;
        StateHasChanged();
    }

    // Investigation Modals
    protected bool _showInvestigationDiscoveryModal = false;
    protected bool _showInvestigationActivationModal = false;
    protected bool _showInvestigationProgressModal = false;
    protected bool _showInvestigationCompleteModal = false;
    protected InvestigationDiscoveryResult _investigationDiscoveryResult;
    protected InvestigationActivationResult _investigationActivationResult;
    protected InvestigationProgressResult _investigationProgressResult;
    protected InvestigationCompleteResult _investigationCompleteResult;

    protected async Task CheckForInvestigationResults()
    {
        InvestigationDiscoveryResult discoveryResult = InvestigationActivity.GetAndClearPendingDiscoveryResult();
        if (discoveryResult != null)
        {
            _investigationDiscoveryResult = discoveryResult;
            _showInvestigationDiscoveryModal = true;
            await InvokeAsync(StateHasChanged);
            return;
        }

        InvestigationActivationResult activationResult = InvestigationActivity.GetAndClearPendingActivationResult();
        if (activationResult != null)
        {
            _investigationActivationResult = activationResult;
            _showInvestigationActivationModal = true;
            await InvokeAsync(StateHasChanged);
            return;
        }

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

    protected async Task CloseInvestigationActivationModal()
    {
        _showInvestigationActivationModal = false;
        _investigationActivationResult = null;
        await InvokeAsync(StateHasChanged);

        await CheckForInvestigationResults();
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

    public void ShowInvestigationDiscoveryModal(InvestigationDiscoveryResult discoveryResult)
    {
        _investigationDiscoveryResult = discoveryResult;
        _showInvestigationDiscoveryModal = true;
        StateHasChanged();
    }

    protected async Task BeginInvestigationIntro()
    {
        _showInvestigationDiscoveryModal = false;

        string investigationId = _investigationDiscoveryResult.InvestigationId;
        _investigationDiscoveryResult = null;

        // Auto-open journal to show discovered investigation
        _showJournal = true;

        await InvokeAsync(StateHasChanged);
    }

    protected async Task DismissInvestigationDiscovery()
    {
        _showInvestigationDiscoveryModal = false;
        _investigationDiscoveryResult = null;

        // Auto-open journal to show discovered investigation
        _showJournal = true;

        await InvokeAsync(StateHasChanged);
    }
}

public enum ScreenMode
{
    Location,
    Exchange,
    Travel,
    SocialChallenge,
    MentalChallenge,
    PhysicalChallenge
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
    public string VenueId { get; set; }
    public string TravelDestination { get; set; }
    public string RequestId { get; set; }
    public string SelectedCardId { get; set; }
    public int? SelectedObligationIndex { get; set; }
}
