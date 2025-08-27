using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Wayfarer.Pages;

public class MainGameplayViewBase : ComponentBase, IDisposable
{
    // Single facade injection - THE ONLY SERVICE INJECTION
    [Inject] public GameFacade GameFacade { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }

    // Navigation parameters from parent component
    [Parameter] public CurrentViews CurrentView { get; set; }
    [Parameter] public Action<CurrentViews> OnNavigate { get; set; }

    // Track previous screen for queue navigation
    public CurrentViews PreviousView { get; private set; } = CurrentViews.LocationScreen;

    // UI State Properties
    public int StateVersion = 0;
    // Conversation handled by new ConversationScreen component

    // Navigation State
    public string SelectedLocation { get; set; }
    public string SelectedNpcId => null; // NPC ID is now handled through ConversationContext
    public TimeBlocks CurrentTimeBlock { get; set; }
    public int Stamina { get; set; } = 0;
    public int Concentration { get; set; } = 0;
    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentSpot { get; set; }

    // Player State - using ViewModels instead of domain objects
    public Player PlayerState { get; private set; }

    // Tooltip State
    public bool ShowTooltip = false;
    public double MouseX;
    public double MouseY;

    // Action Message State
    public bool ShowActionMessage = false;
    public string ActionMessageType = "success";
    public List<string> ActionMessages = new List<string>();

    // System Messages
    public List<SystemMessage> SystemMessages = new List<SystemMessage>();

    // Time Planning State
    public bool showFullDayView = false;

    // DeliveryObligation Offer State
    public bool ShowLetterOfferDialog = false;
    public LetterOfferViewModel CurrentLetterOffer = null;
    public string CurrentNPCOfferId = null;

    // Daily Activities State
    public bool ShowDailySummary = false;
    public DailyActivityResult DailyActivityResult = null;
    public TimeBlocks? LastTimeBlock = null;

    // Debug State
    public bool ShowDebugPanel = false;

    // Computed Properties
    public bool HasApLeft { get; set; }
    public bool HasNoApLeft => !HasApLeft;
    public int TurnActionPoints => 0; // ActionPoints system removed
    public CurrentViews CurrentScreen => CurrentView;
    public int CurrentHour { get; private set; }
    public WeatherCondition CurrentWeather { get; private set; }
    public List<TravelDestinationViewModel> TravelDestinations { get; private set; }

    public TimeBlocks CurrentTime => CurrentTimeBlock;

    public Dictionary<SidebarSections, bool> ExpandedSections = new Dictionary<SidebarSections, bool>
    {
        { SidebarSections.skills, false },
        { SidebarSections.resources, false },
        { SidebarSections.strategic, false },
        { SidebarSections.inventory, false },
        { SidebarSections.status, false }
    };

    private GameWorldSnapshot _lastSnapshot;

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("MainGameplayView initialized - starting polling");

        // Verify initial state
        if (IsGameDataReady())
        {
            Console.WriteLine("Game data is ready - running initial verification");
        }

        // Poll state initially
        PollGameState();
        StateHasChanged();
    }

    public void HandleNavigationChange(CurrentViews previousScreen, CurrentViews newScreen)
    {
        InvokeAsync(() =>
        {
            Console.WriteLine($"Navigation changed from {previousScreen} to {newScreen}");
            StateHasChanged();
        });
    }

    public void RefreshUI()
    {
        Console.WriteLine("[MainGameplayView] RefreshUI called");
        PollGameState();
        StateHasChanged();
    }

    public bool IsTutorialActive()
    {
        return GameFacade.IsTutorialActive();
    }

    public void HandleMessagesExpired()
    {
        // Filter expired messages and update UI
        SystemMessages = SystemMessages.Where(m => !m.IsExpired).ToList();
        StateHasChanged();
    }

    public void PollGameState()
    {
        // Get game snapshot
        GameWorldSnapshot snapshot = GameFacade.GetGameSnapshot();

        // Update time info
        (TimeBlocks timeBlock, int hoursRemaining, int currentDay) = GameFacade.GetTimeInfo();
        CurrentTimeBlock = timeBlock;
        CurrentHour = GameFacade.GetCurrentHour(); // Get actual hour (0-23), not hours remaining

        // Update player state
        PlayerState = GameFacade.GetPlayer();
        Stamina = PlayerState.Stamina;
        Concentration = PlayerState.Concentration;
        CurrentWeather = WeatherCondition.Clear; // Weather should come from facade in future

        // Update location
        CurrentLocation = GameFacade.GetCurrentLocation();
        CurrentSpot = GameFacade.GetCurrentLocationSpot();

        // Pull system messages
        SystemMessages = GameFacade.GetSystemMessages().Where(m => !m.IsExpired).ToList();

        // Check for daily activities
        DailyActivityResult dailyActivities = GameFacade.GetDailyActivities();
        if (dailyActivities != null && dailyActivities.HasEvents && !ShowDailySummary)
        {
            Console.WriteLine("MainGameplayView - Daily activities pending - processing");
            ProcessDailyActivities();
        }

        // Conversations now handled by ConversationScreen component directly

        // Update travel destinations
        TravelDestinations = GameFacade.GetTravelDestinations();

        if (_lastSnapshot == null || !snapshot.IsEqualTo(_lastSnapshot))
        {
            // Force re-render of ConversationView if needed
            if (CurrentScreen == CurrentViews.ConversationScreen)
            {
                StateVersion++;
            }
            _lastSnapshot = snapshot;
        }
    }

    private void ProcessDailyActivities()
    {
        DailyActivityResult = GameFacade.GetDailyActivities();

        if (DailyActivityResult != null && DailyActivityResult.HasEvents)
        {
            ShowDailySummary = true;
            StateHasChanged();
        }
    }

    // Navigation Methods
    public async Task SwitchToTravelScreen()
    {
        Console.WriteLine($"SwitchToTravelScreen called. Current screen: {CurrentScreen}");
        OnNavigate?.Invoke(CurrentViews.TravelScreen);
        Console.WriteLine($"New current screen: {CurrentScreen}");
        RefreshUI();
    }

    public void SwitchToMarketScreen()
    {
        OnNavigate?.Invoke(CurrentViews.MarketScreen);
        RefreshUI();
    }

    public void SwitchToRestScreen()
    {
        OnNavigate?.Invoke(CurrentViews.RestScreen);
        RefreshUI();
    }

    public void SwitchToPlayerStatusScreen()
    {
        OnNavigate?.Invoke(CurrentViews.PlayerStatusScreen);
        RefreshUI();
    }

    public void SwitchToRelationshipScreen()
    {
        OnNavigate?.Invoke(CurrentViews.RelationshipScreen);
        RefreshUI();
    }

    public void SwitchToObligationsScreen()
    {
        OnNavigate?.Invoke(CurrentViews.ObligationsScreen);
        RefreshUI();
    }

    public void SwitchToLetterBoardScreen()
    {
        OnNavigate?.Invoke(CurrentViews.LetterBoardScreen);
        RefreshUI();
    }


    // Travel Handling
    public async Task HandleTravelRoute(RouteOption route)
    {
        Console.WriteLine($"HandleTravelRoute: {route.Name}, Current screen: {CurrentScreen}");

        // Use the route's ID property
        string routeId = route.Id;
        bool success = await GameFacade.TravelToDestinationAsync(route.DestinationLocationSpot, routeId);

        if (success)
        {
            // Navigate back to location screen after travel
            OnNavigate?.Invoke(CurrentViews.LocationScreen);
            Console.WriteLine("HandleTravelRoute: Travel completed, navigating to LocationScreen");
        }
        else
        {
            Console.WriteLine("HandleTravelRoute: Travel failed");
        }

        UpdateState();
    }


    public async Task HandleRestComplete()
    {
        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        UpdateState();
    }

    public Location GetCurrentLocation()
    {
        return GameFacade.GetCurrentLocation();
    }

    /// <summary>
    /// Validates that all required game data is ready for UI interaction.
    /// MainGameplayView is the single authority on data readiness.
    /// </summary>
    public bool IsGameDataReady()
    {
        try
        {
            // Check game state through facade
            GameWorldSnapshot snapshot = GameFacade.GetGameSnapshot();
            if (snapshot == null)
                return false;

            // Check current location
            Location location = GameFacade.GetCurrentLocation();
            LocationSpot spot = GameFacade.GetCurrentLocationSpot();
            if (location == null || string.IsNullOrEmpty(location.Id) || string.IsNullOrEmpty(location.Name))
                return false;

            // Check player
            Player player = GameFacade.GetPlayer();
            if (player == null || !player.IsInitialized)
                return false;

            // Check inventory through facade
            InventoryViewModel inventory = GameFacade.GetInventory();
            if (inventory == null || inventory.Items == null)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        // Use the new intent-based system for movement
        MoveIntent moveIntent = new MoveIntent(locationSpot.SpotID);
        bool success = await GameFacade.ExecuteIntent(moveIntent);

        if (success)
        {
            UpdateState();
        }
    }

    public async Task OnConversationCompleted()
    {
        // Conversations now handled by ConversationScreen component
        // Navigate back to appropriate screen
        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task HandleTravelStart(string travelDestination)
    {
        List<TravelRouteViewModel> routes = GameFacade.GetRoutesToDestination(travelDestination);
        if (routes.Any())
        {
            await GameFacade.TravelToDestinationAsync(travelDestination, routes.First().RouteId);
        }

        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task OnNarrativeCompleted()
    {
        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        UpdateState();
    }

    public void UpdateState()
    {
        StateVersion++;
        PollGameState();
        StateHasChanged();
    }

    public void DismissActionMessage()
    {
        ShowActionMessage = false;
    }

    public string GetArchetypePortrait()
    {
        string portraitPath = $"/images/characters/{PlayerState.Gender.ToString().ToLower()}_{PlayerState.Archetype.ToString().ToLower()}.png";
        return portraitPath;
    }

    public string FormatItemName(Item itemType)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            itemType.ToString(),
            "([A-Z])",
            " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    public async Task ToggleSection(SidebarSections sectionName)
    {
        if (ExpandedSections.ContainsKey(sectionName))
        {
            bool toggleOn = !ExpandedSections[sectionName];

            foreach (KeyValuePair<SidebarSections, bool> section in ExpandedSections)
            {
                ExpandedSections[section.Key] = false;
            }

            ExpandedSections[sectionName] = toggleOn;
            if (toggleOn)
            {
                StateHasChanged();
                await Task.Delay(50);
                await JSRuntime.InvokeVoidAsync("scrollSectionIntoView", sectionName.ToString());
            }
            else
            {
                StateHasChanged();
            }
        }
    }

    /// <summary>
    /// Get weather icon for UI display
    /// </summary>
    public string GetWeatherIcon(WeatherCondition weather)
    {
        return weather switch
        {
            WeatherCondition.Clear => "☀️",
            WeatherCondition.Rain => "🌧️",
            WeatherCondition.Snow => "❄️",
            WeatherCondition.Fog => "🌫️",
            _ => "❓"
        };
    }

    /// <summary>
    /// Get icon for time block display
    /// </summary>
    public string GetTimeBlockIcon(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Dawn => "🌅",
            TimeBlocks.Morning => "☀️",
            TimeBlocks.Afternoon => "🌞",
            TimeBlocks.Evening => "🌇",
            TimeBlocks.Night => "🌙",
            _ => "⏰"
        };
    }

    /// <summary>
    /// Get icon for service type display
    /// </summary>
    public string GetServiceIcon(ServiceTypes service)
    {
        return service switch
        {
            ServiceTypes.Rest => "🛌",
            ServiceTypes.Trade => "🛒",
            ServiceTypes.Healing => "❤️",
            ServiceTypes.Information => "📖",
            ServiceTypes.Training => "⚔️",
            ServiceTypes.EquipmentRepair => "🔨",
            ServiceTypes.FoodProduction => "🍲",
            _ => "⚙️"
        };
    }


    /// <summary>
    /// Accept a letter offer from an NPC by offer ID
    /// </summary>
    public void AcceptLetterOfferId(string offerId)
    {
        if (CurrentLetterOffer == null) return;

        // Use the facade to accept the offer
        Task.Run(async () =>
        {
            bool success = await GameFacade.AcceptLetterOfferAsync(offerId);

            if (success)
            {
                ShowLetterOfferDialog = false;
                CurrentLetterOffer = null;
                CurrentNPCOfferId = null;
                StateHasChanged();
            }
        });
    }

    /// <summary>
    /// Refuse a letter offer from an NPC
    /// </summary>
    public void RefuseLetterOffer()
    {
        ShowLetterOfferDialog = false;
        CurrentLetterOffer = null;
        CurrentNPCOfferId = null;
        StateHasChanged();
    }

    /// <summary>
    /// Handle daily summary continuation
    /// </summary>
    public void HandleDailySummaryContinue()
    {
        ShowDailySummary = false;

        // If letter board is available, switch to it
        if (CurrentTimeBlock == TimeBlocks.Dawn)
        {
            SwitchToLetterBoardScreen();
        }

        StateHasChanged();
    }

    public void HandleNavigation(CurrentViews view)
    {
        PreviousView = CurrentView;
        OnNavigate?.Invoke(view);
        StateHasChanged();
    }

    public void HandleConversationEnd()
    {
        Console.WriteLine("[MainGameplayView] HandleConversationEnd called - navigating to LocationScreen");
        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        StateHasChanged();
    }

    /// <summary>
    /// Debug method to output current game state
    /// </summary>
    public void PrintDebugState()
    {
        string report = "Debug state through GameFacade";
        Console.WriteLine(report);
    }

    public void ToggleDebugPanel()
    {
        ShowDebugPanel = !ShowDebugPanel;
        StateHasChanged();
    }

    /// <summary>
    /// Debug command to start tutorial manually
    /// </summary>
    public void StartTutorialManually()
    {
        try
        {
            // Start game through facade
            Task.Run(async () =>
            {
                await GameFacade.StartGameAsync();

                List<SystemMessage> messages = GameFacade.GetSystemMessages();
                Console.WriteLine($"Tutorial manually started. Messages: {messages.Count}");
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start tutorial: {ex.Message}");
        }
    }

    public void RunVerification()
    {
        List<SystemMessage> messages = GameFacade.GetSystemMessages();
        Console.WriteLine("Verification completed - check console");
    }

    public List<Location> Locations => TravelDestinations?.Select(d => new Location(d.LocationId, d.LocationName)).ToList() ?? new List<Location>();

    // All legacy service getters completely removed - use GameFacade methods exclusively
    // Following architectural principle: DELETE LEGACY CODE ENTIRELY

    // Helper method to calculate total weight
    public int CalculateTotalWeight()
    {
        InventoryViewModel inventory = GameFacade.GetInventory();
        return inventory?.TotalWeight ?? 0;
    }

    // Helper method to get item by name
    public Item GetItemByName(string itemName)
    {
        // Create a stub item for display
        // In a proper implementation, this would come from the facade
        return new Item
        {
            Name = itemName,
            Weight = 1,
            Categories = new List<ItemCategory>(),
            // AllCategoriesDescription is computed
        };
    }


    public void Dispose()
    {
        // Clean architecture - no events to unsubscribe from
    }
}