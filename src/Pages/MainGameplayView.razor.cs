using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Wayfarer.UIHelpers;

namespace Wayfarer.Pages;

public class MainGameplayViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameManager { get; set; }
    // Read-only services allowed for UI state management and display
    [Inject] public MessageSystem MessageSystem { get; set; }
    [Inject] public ItemRepository ItemRepository { get; set; }
    [Inject] public LocationRepository LocationRepository { get; set; }
    [Inject] public LoadingStateService? LoadingStateService { get; set; }
    [Inject] public CardHighlightService CardRefreshService { get; set; }


    public int StateVersion = 0;
    public EncounterManager EncounterManager = null;
    public EncounterResult EncounterResult;

    // Navigation State
    public string SelectedLocation { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }
    public int Stamina { get; set; } = 0;
    public int Concentration { get; set; } = 0;
    public Location CurrentLocation { get; set; }

    public Player PlayerState
    {
        get
        {
            return GameWorld.GetPlayer();
        }
    }

    // Tooltip State
    public bool ShowTooltip = false;
    public UserActionOption HoveredAction;
    public double MouseX;
    public double MouseY;

    // Action Message State
    public bool ShowActionMessage = false;
    public string ActionMessageType = "success";
    public List<string> ActionMessages = new List<string>();
    public ElementReference SidebarRef;


    public bool HasApLeft { get; set; }
    public bool HasNoApLeft
    {
        get
        {
            return !HasApLeft;
        }
    }


    public int TurnActionPoints
    {
        get
        {
            return PlayerState.MaxActionPoints;
        }
    }

    public BeatOutcome BeatOutcome { get; set; }
    public CurrentViews CurrentScreen { get; set; } = CurrentViews.LocationScreen;

    public LocationSpot CurrentSpot
    {
        get
        {
            return LocationRepository.GetCurrentLocationSpot();
        }
    }
    public TimeBlocks CurrentTime
    {
        get
        {
            return GameWorld.TimeManager.GetCurrentTimeBlock();
        }
    }

    public int CurrentHour
    {
        get
        {
            return GameWorld.TimeManager.GetCurrentTimeHours();
        }
    }

    public WeatherCondition CurrentWeather
    {
        get
        {
            return GameWorld.CurrentWeather;
        }
    }

    public List<Location> Locations
    {
        get
        {
            return GameManager.GetPlayerKnownLocations();
        }
    }

    public Dictionary<SidebarSections, bool> ExpandedSections = new Dictionary<SidebarSections, bool>
    {
        { SidebarSections.skills, false },
        { SidebarSections.resources, false },
        { SidebarSections.strategic, false },
        { SidebarSections.inventory, false },
        { SidebarSections.status, false }
    };

    protected override async Task OnInitializedAsync()
    {
        // Set up polling instead of direct timer calls
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await InvokeAsync(() =>
                {
                    PollGameState();
                    StateHasChanged();
                });
                await Task.Delay(50);
            }
        });
    }

    public GameWorldSnapshot oldSnapshot;

    public void PollGameState()
    {
        GameWorldSnapshot snapshot = GameManager.GetGameSnapshot();
        CurrentTimeBlock = snapshot.CurrentTimeBlock;
        Stamina = snapshot.Stamina;
        Concentration = snapshot.Concentration;

        if (oldSnapshot == null || !snapshot.IsEqualTo(oldSnapshot))
        {
            // You need to update EncounterManager state and force StateHasChanged for ALL screens
            if (CurrentScreen == CurrentViews.EncounterScreen)
            {
                StateVersion++;  // Force re-render of EncounterView
            }

            oldSnapshot = snapshot;
        }
    }

    public async Task SwitchToTravelScreen()
    {
        Console.WriteLine($"SwitchToTravelScreen called. Current screen: {CurrentScreen}");
        CurrentScreen = CurrentViews.TravelScreen;
        Console.WriteLine($"New current screen: {CurrentScreen}");
        StateHasChanged();
    }

    public void SwitchToMarketScreen()
    {
        CurrentScreen = CurrentViews.MarketScreen;
        StateHasChanged();
    }

    public void SwitchToRestScreen()
    {
        CurrentScreen = CurrentViews.RestScreen;
        StateHasChanged();
    }

    public void SwitchToContractScreen()
    {
        CurrentScreen = CurrentViews.ContractScreen;
        StateHasChanged();
    }

    public async Task HandleTravelRoute(RouteOption route)
    {
        await GameManager.Travel(route);
        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    public async Task HandleTravelWithTransport((RouteOption route, TravelMethods transport) travelData)
    {
        await GameManager.TravelWithTransport(travelData.route, travelData.transport);
        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    public async Task HandleRestComplete()
    {
        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    public Location GetCurrentLocation()
    {
        return LocationRepository.GetCurrentLocation();
    }

    /// <summary>
    /// Validates that all required game data is ready for UI interaction.
    /// MainGameplayView is the single authority on data readiness.
    /// </summary>
    public bool IsGameDataReady()
    {
        // Check that core game state is initialized
        if (GameWorld?.WorldState == null)
            return false;

        // Check that current location is properly loaded
        Location currentLocation = LocationRepository.GetCurrentLocation();
        if (currentLocation == null || string.IsNullOrEmpty(currentLocation.Id) || string.IsNullOrEmpty(currentLocation.Name))
            return false;

        // Check that items are loaded (required for Market functionality)
        if (ItemRepository.GetAllItems() == null || !ItemRepository.GetAllItems().Any())
            return false;

        // Check that player is initialized
        Player player = GameWorld.GetPlayer();
        if (player == null || !player.IsInitialized)
            return false;

        return true;
    }

    public LocationSpot GetCurrentSpot()
    {
        return LocationRepository.GetCurrentLocationSpot();
    }

    public async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.SpotID);
        UpdateState();
    }

    public async Task StartNewDay()
    {
        await GameManager.StartNewDay();
        UpdateState();
    }

    public async Task HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return;

        await GameManager.OnPlayerSelectsAction(action);

        // Check if an encounter was started
        if (GameWorld.ActionStateTracker.CurrentEncounterManager != null)
        {
            // Simply switch to encounter screen - EncounterView will handle initialization
            CurrentScreen = CurrentViews.EncounterScreen;
        }

        EncounterManager = GameWorld.ActionStateTracker.CurrentEncounterManager;

        UpdateState();
    }

    public async Task OnEncounterCompleted(BeatOutcome result)
    {
        // Process action completion
        await GameManager.ProcessActionCompletion();

        // Store the result for narrative view
        BeatOutcome = result;

        // Switch to narrative screen to show result
        CurrentScreen = CurrentViews.NarrativeScreen;

        UpdateState();
    }

    public async Task HandleTravelStart(string travelDestination)
    {
        List<RouteOption> routes = GameManager.GetAvailableRoutes(LocationRepository.GetCurrentLocation().Id, travelDestination);
        RouteOption routeOption = routes.FirstOrDefault();

        await GameManager.Travel(routeOption);

        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    public async Task OnNarrativeCompleted()
    {
        BeatOutcome = null;

        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    public async Task UseResource(ActionNames actionName)
    {
        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    public async Task HandleCardRefreshed(SkillCard card)
    {
        await GameManager.RefreshCard(card);
        MessageSystem.AddSystemMessage($"Refreshed {card.Name} card");
        UpdateState();
    }

    public void UpdateState()
    {
        DisplayActionMessages();

        StateVersion++;
        StateHasChanged();
    }

    public void DisplayActionMessages()
    {
        ActionMessages = GetResultMessages();

        if (ActionMessages.Any())
        {
            ShowActionMessage = true;
            ActionMessageType = "success";  // Default to success

            // Auto-dismiss after 5 seconds
            Task.Delay(5000).ContinueWith(_ =>
            {
                InvokeAsync(() =>
                {
                    ShowActionMessage = false;
                    StateHasChanged();
                });
            });
        }
    }

    public void DismissActionMessage()
    {
        ShowActionMessage = false;
    }

    public List<string> GetResultMessages()
    {
        ActionResultMessages messages = MessageSystem.GetAndClearChanges();
        List<string> list = new();

        if (messages == null) return list;

        // Add outcome descriptions
        foreach (IMechanicalEffect outcome in messages.Outcomes)
        {
            list.Add(outcome.GetDescriptionForPlayer());
        }

        // Add system messages
        foreach (SystemMessage sysMsg in messages.SystemMessages)
        {
            list.Add(sysMsg.Message);
        }

        return list;
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
    /// Analyze overall strategic status for sidebar display
    /// </summary>
    public PlayerStrategicOverview AnalyzePlayerStrategicStatus()
    {
        PlayerStrategicOverview overview = new PlayerStrategicOverview();

        // Analyze current equipment capabilities
        List<EquipmentCategory> currentEquipment = GetCurrentEquipmentCategories();
        overview.EquipmentCapabilities = currentEquipment.Select(cat => cat.ToString().Replace('_', ' ')).ToList();

        // Analyze route accessibility (simplified for now)
        overview.AccessibleRoutes = 3; // Placeholder - would calculate based on actual routes
        overview.BlockedRoutes = 2; // Placeholder - would calculate based on blocked routes

        // Identify critical missing equipment
        List<EquipmentCategory> allEquipmentCategories = Enum.GetValues<EquipmentCategory>().ToList();
        List<EquipmentCategory> missingCategories = allEquipmentCategories.Where(cat => !currentEquipment.Contains(cat)).ToList();
        overview.CriticalMissingEquipment = missingCategories.Take(3).Select(cat => cat.ToString().Replace('_', ' ')).ToList();

        // Analyze contract readiness (simplified for now)
        overview.ReadyContracts = 2; // Placeholder - would calculate based on actual contracts
        overview.PendingContracts = 1; // Placeholder - would calculate based on pending contracts
        overview.UrgentContracts = 0; // Placeholder - would calculate based on urgent contracts

        return overview;
    }

    /// <summary>
    /// Analyze time awareness and recommendations
    /// </summary>
    public TimeAwarenessAnalysis AnalyzeTimeAwareness()
    {
        TimeAwarenessAnalysis analysis = new TimeAwarenessAnalysis();
        TimeBlocks currentTime = GameWorld.TimeManager.GetCurrentTimeBlock();
        int currentDay = GameWorld.CurrentDay;

        // Analyze current time status
        analysis.CurrentStatus = $"{currentTime.ToString().Replace('_', ' ')} - Day {currentDay}";

        // Generate time-based recommendations
        if (currentTime == TimeBlocks.Dawn || currentTime == TimeBlocks.Morning)
        {
            analysis.Recommendation = "Optimal time for travel and contracts";
        }
        else if (currentTime == TimeBlocks.Afternoon)
        {
            analysis.Recommendation = "Good time for trading and social activities";
        }
        else if (currentTime == TimeBlocks.Evening)
        {
            analysis.Recommendation = "Markets closing soon - consider rest";
        }
        else
        {
            analysis.Recommendation = "Night time - limited activities available";
        }

        return analysis;
    }

    /// <summary>
    /// Get current equipment categories owned by player
    /// </summary>
    private List<EquipmentCategory> GetCurrentEquipmentCategories()
    {
        List<EquipmentCategory> ownedCategories = new List<EquipmentCategory>();

        foreach (string itemName in GameWorld.GetPlayer().Inventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = ItemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    ownedCategories.AddRange(item.Categories);
                }
            }
        }

        return ownedCategories.Distinct().ToList();
    }

    /// <summary>
    /// Check if player can complete a contract based on current status (simplified)
    /// </summary>
    private bool CanCompleteContract(Contract contract)
    {
        // Simplified implementation - would need actual contract checking logic
        return true;
    }
}
