using Microsoft.AspNetCore.Components;

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
    [Inject] protected GameWorld GameWorld { get; set; }
    [Inject] protected SceneFacade SceneFacade { get; set; }
    [Inject] protected LoadingStateService LoadingStateService { get; set; }
    [Inject] protected ObligationActivity ObligationActivity { get; set; }

    public GameScreenBase()
    { }

    // Screen Management
    protected ScreenMode CurrentScreen { get; set; } = ScreenMode.Location;
    protected ScreenMode PreviousScreen { get; set; } = ScreenMode.Location;
    protected int ContentVersion { get; set; } = 0;
    protected bool IsTransitioning { get; set; } = false;

    private Stack<ScreenContext> _navigationStack = new(10);
    private SemaphoreSlim _stateLock = new(1, 1);
    private List<IDisposable> _subscriptions = new List<IDisposable>();

    // Resources Display - Made public for child components to access for Perfect Information principle
    public int Coins { get; set; }
    public int MaxCoins { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Hunger { get; set; }
    public int MaxHunger { get; set; }
    public int Focus { get; set; }
    public int MaxFocus { get; set; }

    // Time Display - Made public for child components to access for Perfect Information principle
    public string CurrentTime { get; set; }
    public string TimePeriod { get; set; }

    // Venue Display
    protected string CurrentLocationPath { get; set; }
    protected string CurrentSpot { get; set; }

    // Navigation State
    protected ExchangeContext CurrentExchangeContext { get; set; }
    protected RouteObstacleContext CurrentRouteObstacleContext { get; set; }
    protected SceneContext CurrentSceneContext { get; set; }
    protected SocialChallengeContext CurrentSocialContext { get; set; }
    protected MentalChallengeContext CurrentMentalContext { get; set; }
    protected PhysicalChallengeContext CurrentPhysicalContext { get; set; }
    protected ConversationTreeContext CurrentConversationTreeContext { get; set; }
    protected ObservationContext CurrentObservationContext { get; set; }
    protected EmergencyContext CurrentEmergencyContext { get; set; }
    protected int PendingLetterCount { get; set; }
    public string CurrentDeckViewerNpcId { get; set; } // For dev mode deck viewer

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine($"[GameScreen.OnInitializedAsync] Called. IsGameStarted: {GameWorld.IsGameStarted}");

        // CRITICAL: Don't initialize until parent GameUI has called StartGameAsync()
        // This prevents race condition where GameScreen initializes before player position is set
        if (!GameWorld.IsGameStarted)
        {
            Console.WriteLine("[GameScreen.OnInitializedAsync] ⚠️ Game not started yet - skipping initialization");
            return;
        }

        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Check for active emergency - interrupts normal gameplay
        EmergencySituation activeEmergency = GameFacade.GetActiveEmergency();
        if (activeEmergency != null)
        {
            await StartEmergency(activeEmergency);
            return; // Emergency takes priority, skip normal initialization
        }

        // Check for resumable modal scenes at current context (multi-situation scene resumption)
        // Uses GetResumableScenesAtContext which checks CurrentSituation.RequiredLocationId
        // This enables scenes to span multiple locations as situations progress
        Location currentLocation = GameFacade.GetCurrentLocation();
        if (currentLocation != null)
        {
            List<Scene> resumableScenes = SceneFacade.GetResumableScenesAtContext(currentLocation, null);
            if (resumableScenes.Count > 0)
            {
                Scene modalScene = resumableScenes.First();
                await StartScene(modalScene);
                return; // Modal scene takes priority over normal location display
            }
        }

        await base.OnInitializedAsync();
    }

    public async Task RefreshResourceDisplay()
    {
        if (GameFacade == null)
        {
            throw new InvalidOperationException("GameFacade is required");
        }

        Player player = GameFacade.GetPlayer();
        if (player == null)
        {
            throw new InvalidOperationException("Player not found");
        }

        Coins = player.Coins;
        Health = player.Health;
        Focus = player.Focus;
        MaxFocus = player.MaxFocus;
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
        Venue venue = GameFacade.GetCurrentLocation().Venue;
        Location location = GameFacade.GetCurrentLocation();

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
        // Get the current venue directly from GameFacade
        Venue venue = GameFacade.GetCurrentLocation().Venue;
        if (venue == null) return locationName;

        // Get the district for the venue (object reference, NO ID lookup)
        District district = GameFacade.GetDistrictForLocation(venue);
        if (district == null)
            return venue.Name;

        // Get the region from the district (object reference, NO ID extraction)
        Region region = GameFacade.GetRegionForDistrict(district);

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
            return;
        }

        if (!await _stateLock.WaitAsync(5000))
        {
            return;
        }

        try
        {
            IsTransitioning = true;// Save current state
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
            ContentVersion++; await LoadStateForMode(newMode); await InvokeAsync(StateHasChanged);
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
                state.Npc = CurrentSocialContext?.Npc;
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

    public async Task StartExchange(NPC npc)
    {
        CurrentExchangeContext = await GameFacade.CreateExchangeContext(npc);

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
        { }
    }

    protected async Task HandleExchangeEnd()
    {
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

    public async Task StartConversationSession(NPC npc, Situation situation)
    {
        CurrentSocialContext = await GameFacade.CreateConversationContext(npc, situation);

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
            // Show error messageawait InvokeAsync(StateHasChanged);
        }
    }

    protected async Task HandleConversationEnd()
    {
        // STRATEGIC LAYER: Process challenge outcome and apply rewards
        await GameFacade.ProcessSocialChallengeOutcome();

        CurrentSocialContext = null;

        // Always refresh UI after conversation ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Check for obligation results before returning to location
        await CheckForObligationResults();

        // Special case: allow navigation from conversation when it ends properly
        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task StartMentalSession(MentalChallengeDeck deck, Location location, Situation situation, Obligation obligation)
    {
        MentalSession session = GameFacade.StartMentalSession(deck, location, situation, obligation);

        // Create context parallel to Social pattern
        CurrentMentalContext = new MentalChallengeContext
        {
            IsValid = session != null,
            ErrorMessage = session == null ? "Failed to start Mental session" : string.Empty,
            DeckId = deck?.Id,
            Session = session,
            Venue = location?.Venue,
            LocationName = location?.Name ?? "Unknown"
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
        { }
    }

    public async Task HandleMentalEnd()
    {
        // STRATEGIC LAYER: Process challenge outcome and apply rewards
        await GameFacade.ProcessMentalChallengeOutcome();

        CurrentMentalContext = null;

        // Always refresh UI after mental session ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Check for obligation results before returning to location
        await CheckForObligationResults();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task StartPhysicalSession(PhysicalChallengeDeck deck, Location location, Situation situation, Obligation obligation)
    {
        PhysicalSession session = GameFacade.StartPhysicalSession(deck, location, situation, obligation);

        // Create context parallel to Social pattern
        CurrentPhysicalContext = new PhysicalChallengeContext
        {
            IsValid = session != null,
            ErrorMessage = session == null ? "Failed to start Physical session" : string.Empty,
            DeckId = deck?.Id,
            Session = session,
            Venue = location?.Venue,
            LocationName = location?.Name ?? "Unknown"
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
        { }
    }

    public async Task StartConversationTree(ConversationTree tree)
    {
        CurrentConversationTreeContext = GameFacade.CreateConversationTreeContext(tree.Id);

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (CurrentConversationTreeContext != null && CurrentConversationTreeContext.IsValid)
        {
            CurrentScreen = ScreenMode.ConversationTree;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task StartObservationScene(ObservationScene scene)
    {
        CurrentObservationContext = GameFacade.CreateObservationContext(scene);

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (CurrentObservationContext != null && CurrentObservationContext.IsValid)
        {
            CurrentScreen = ScreenMode.Observation;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task StartEmergency(EmergencySituation emergency)
    {
        CurrentEmergencyContext = GameFacade.CreateEmergencyContext(emergency.Id);

        // Always refresh UI after GameFacade action
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        if (CurrentEmergencyContext != null && CurrentEmergencyContext.IsValid)
        {
            CurrentScreen = ScreenMode.Emergency;
            ContentVersion++;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async Task HandlePhysicalEnd()
    {
        // STRATEGIC LAYER: Process challenge outcome and apply rewards
        await GameFacade.ProcessPhysicalChallengeOutcome();

        CurrentPhysicalContext = null;

        // Always refresh UI after physical session ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Check for obligation results before returning to location
        await CheckForObligationResults();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task HandleConversationTreeEnd()
    {
        CurrentConversationTreeContext = null;

        // Always refresh UI after conversation tree ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task HandleObservationEnd()
    {
        CurrentObservationContext = null;

        // Always refresh UI after observation ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task HandleEmergencyEnd()
    {
        CurrentEmergencyContext = null;

        // Clear the active emergency in GameWorld
        GameFacade.ClearActiveEmergency();

        // Always refresh UI after emergency ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task StartScene(Scene scene)
    {
        if (scene == null)
            return;

        // Get current situation from scene
        Situation currentSituation = scene.CurrentSituation;

        if (currentSituation == null)
            return;

        // Create modal scene context
        Location currentLocation = GameFacade.GetCurrentLocation();
        CurrentSceneContext = new SceneContext
        {
            IsValid = true,
            Scene = scene,
            CurrentSituation = currentSituation,
            Location = currentLocation, // Object reference, NO ID
            LocationName = currentLocation?.Name
        };

        // Always refresh UI before modal scene
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        CurrentScreen = ScreenMode.Scene;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task StartNPCEngagement(NPC npc, Scene scene)
    {
        // Direct scene object passed from UI (HIGHLANDER Pattern B - no lookup needed)
        // Defensive validation: Scene must be active and belong to this NPC
        if (scene.State != SceneState.Active)
        {
            // ADR-007: Use DisplayName or TemplateId for logging (no Id property)
            Console.WriteLine($"[GameScreen] Scene {scene.DisplayName} is not active (state: {scene.State})");
            return;
        }

        // Get current situation from scene
        Situation currentSituation = scene.CurrentSituation;

        // ARCHITECTURAL CHANGE: Placement is per-situation (not per-scene)
        // HIGHLANDER: Object equality, not Name comparison
        if (currentSituation?.Npc == null || currentSituation.Npc != npc)
        {
            Console.WriteLine($"[GameScreen] Scene {scene.DisplayName} current situation does not involve NPC {npc.Name}");
            return;
        }

        if (currentSituation == null)
        {
            Console.WriteLine($"[GameScreen] No current situation found for scene {scene.DisplayName}");
            return;
        }

        // Create modal scene context
        Location currentLocation = GameFacade.GetCurrentLocation();
        CurrentSceneContext = new SceneContext
        {
            IsValid = true,
            Scene = scene,
            CurrentSituation = currentSituation,
            Location = currentLocation, // Object reference, NO ID
            LocationName = currentLocation?.Name
        };

        // Always refresh UI before scene
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();

        CurrentScreen = ScreenMode.Scene;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    public async Task HandleSceneEnd()
    {
        CurrentSceneContext = null;

        // Always refresh UI after modal scene ends
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        CurrentScreen = ScreenMode.Location;
        ContentVersion++;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task HandleSceneEnd(bool success)
    {
        // Clear scene context
        CurrentSceneContext = null;

        // Refresh displays
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();

        // Return to location
        await NavigateToScreen(ScreenMode.Location);
    }

    protected string GetCurrentLocation()
    {
        Venue venue = GameFacade.GetCurrentLocation().Venue;
        return venue?.Name ?? "Unknown";
    }

    protected async Task RefreshUI()
    {
        await RefreshResourceDisplay();
        await RefreshTimeDisplay();
        await RefreshLocationDisplay();
        await CheckForObligationResults();
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
        if (GameFacade == null)
        {
            throw new InvalidOperationException("GameFacade is required");
        }

        return GameFacade.GetSegmentsInCurrentPeriod();
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

    protected DeliveryJob GetActiveDeliveryJob()
    {
        return GameFacade.GetActiveDeliveryJob();
    }

    // Discovery Journal
    protected bool _showJournal = false;

    protected void ToggleJournal()
    {
        _showJournal = !_showJournal;
        StateHasChanged();
    }

    // Hex Map Navigation
    protected async Task ToggleMap()
    {
        if (CurrentScreen == ScreenMode.HexMap)
        {
            await NavigateToScreen(ScreenMode.Location);
        }
        else
        {
            await NavigateToScreen(ScreenMode.HexMap);
        }
    }

    // Obligation Modals
    protected bool _showObligationDiscoveryModal = false;
    protected bool _showObligationIntroModal = false;
    protected bool _showObligationActivationModal = false;
    protected bool _showObligationProgressModal = false;
    protected bool _showObligationCompleteModal = false;
    protected ObligationDiscoveryResult _obligationDiscoveryResult;
    protected ObligationIntroResult _obligationIntroResult;
    protected ObligationActivationResult _obligationActivationResult;
    protected ObligationProgressResult _obligationProgressResult;
    protected ObligationCompleteResult _obligationCompleteResult;

    protected async Task CheckForObligationResults()
    {
        ObligationDiscoveryResult discoveryResult = ObligationActivity.GetAndClearPendingDiscoveryResult();
        if (discoveryResult != null)
        {
            _obligationDiscoveryResult = discoveryResult;
            _showObligationDiscoveryModal = true;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ObligationIntroResult introResult = ObligationActivity.GetAndClearPendingIntroResult();
        if (introResult != null)
        {
            _obligationIntroResult = introResult;
            _showObligationIntroModal = true;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ObligationActivationResult activationResult = ObligationActivity.GetAndClearPendingActivationResult();
        if (activationResult != null)
        {
            _obligationActivationResult = activationResult;
            _showObligationActivationModal = true;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ObligationProgressResult progressResult = ObligationActivity.GetAndClearPendingProgressResult();
        if (progressResult != null)
        {
            _obligationProgressResult = progressResult;
            _showObligationProgressModal = true;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ObligationCompleteResult completeResult = ObligationActivity.GetAndClearPendingCompleteResult();
        if (completeResult != null)
        {
            _obligationCompleteResult = completeResult;
            _showObligationCompleteModal = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async Task CloseObligationActivationModal()
    {
        _showObligationActivationModal = false;
        _obligationActivationResult = null;
        await InvokeAsync(StateHasChanged);

        await CheckForObligationResults();
    }

    protected async Task CloseObligationProgressModal()
    {
        _showObligationProgressModal = false;
        _obligationProgressResult = null;
        await InvokeAsync(StateHasChanged);

        await CheckForObligationResults();
    }

    protected async Task CloseObligationCompleteModal()
    {
        _showObligationCompleteModal = false;
        _obligationCompleteResult = null;
        await InvokeAsync(StateHasChanged);
    }

    public void ShowObligationDiscoveryModal(ObligationDiscoveryResult discoveryResult)
    {
        _obligationDiscoveryResult = discoveryResult;
        _showObligationDiscoveryModal = true;
        StateHasChanged();
    }

    protected async Task BeginObligationIntro()
    {
        _showObligationDiscoveryModal = false;

        Obligation obligation = _obligationDiscoveryResult.Obligation;
        _obligationDiscoveryResult = null;

        if (obligation == null)
            return;

        // Activate obligation and spawn Phase 1 scene
        await ObligationActivity.CompleteIntroAction(obligation);

        // Refresh UI after activation
        await RefreshLocationDisplay();

        // Auto-open journal to show activated obligation
        _showJournal = true;

        // Check for activation modal
        await CheckForObligationResults();

        await InvokeAsync(StateHasChanged);
    }

    protected async Task DismissObligationDiscovery()
    {
        _showObligationDiscoveryModal = false;
        _obligationDiscoveryResult = null;

        // Auto-open journal to show discovered obligation
        _showJournal = true;

        await InvokeAsync(StateHasChanged);
    }

    protected async Task CloseObligationIntroModal()
    {
        _showObligationIntroModal = false;
        _obligationIntroResult = null;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task CompleteObligationIntroAction()
    {
        _showObligationIntroModal = false;

        Obligation obligation = _obligationIntroResult.Obligation;
        _obligationIntroResult = null;

        // Activate obligation and spawn Phase 1 scene
        await GameFacade.CompleteObligationIntro(obligation);

        // Refresh UI after activation
        await RefreshLocationDisplay();

        // Check for activation modal
        await CheckForObligationResults();

        await InvokeAsync(StateHasChanged);
    }
}

public class ScreenContext
{
    public ScreenMode Mode { get; set; }
    public ScreenStateData StateData { get; set; } = new();
    public DateTime EnteredAt { get; set; }
}

/// <summary>
/// Strongly typed state data for screen transitions
/// NOTE: This is UI navigation state, not domain state
/// Object references used where possible, IDs acceptable for serialization needs
/// </summary>
public class ScreenStateData
{
    public NPC Npc { get; set; } // Object reference for current NPC interaction
    public string VenueId { get; set; } // May be needed for venue-based navigation
    public string TravelDestination { get; set; } // May be needed for travel resumption
    public string SelectedCardId { get; set; } // Card instance tracking
    public int? SelectedObligationIndex { get; set; } // Obligation selection tracking
}
