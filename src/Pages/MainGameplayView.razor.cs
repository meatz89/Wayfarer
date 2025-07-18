using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Wayfarer.UIHelpers;
using Wayfarer.GameState;
using Wayfarer.Services;

namespace Wayfarer.Pages;

public class MainGameplayViewBase : ComponentBase, IDisposable
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameManager { get; set; }
    // Read-only services allowed for UI state management and display
    [Inject] public MessageSystem MessageSystem { get; set; }
    [Inject] public ItemRepository ItemRepository { get; set; }
    [Inject] public LocationRepository LocationRepository { get; set; }
    [Inject] public NPCRepository NPCRepository { get; set; }
    [Inject] public LoadingStateService? LoadingStateService { get; set; }
    [Inject] public CardHighlightService CardRefreshService { get; set; }
    [Inject] public ConnectionTokenManager ConnectionTokenManager { get; set; }
    [Inject] public LetterQueueManager LetterQueueManager { get; set; }
    [Inject] public NavigationService NavigationService { get; set; }
    [Inject] public NPCLetterOfferService NPCLetterOfferService { get; set; }


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

    // Time Planning State
    public bool showFullDayView = false;
    
    // Letter Offer State
    public bool ShowLetterOfferDialog = false;
    public NPC CurrentNPCOffer = null;
    
    // Morning Activities State
    public bool ShowMorningSummary = false;
    public MorningActivityResult MorningActivityResult = null;
    public TimeBlocks? LastTimeBlock = null;


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
            return 0; // ActionPoints system removed
        }
    }

    public BeatOutcome BeatOutcome { get; set; }
    public CurrentViews CurrentScreen => NavigationService.CurrentScreen;

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
        // Subscribe to navigation changes
        NavigationService.OnNavigationChanged += OnNavigationChanged;
        
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

        // Check for pending morning activities (set by StartNewDay)
        if (GameManager.HasPendingMorningActivities() && !ShowMorningSummary)
        {
            ProcessMorningActivities();
        }

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
    
    private void ProcessMorningActivities()
    {
        // Get morning summary from GameWorldManager
        MorningActivityResult = GameManager.GetMorningActivitySummary();
        
        if (MorningActivityResult != null && MorningActivityResult.HasEvents)
        {
            ShowMorningSummary = true;
            StateHasChanged();
        }
    }

    public async Task SwitchToTravelScreen()
    {
        Console.WriteLine($"SwitchToTravelScreen called. Current screen: {CurrentScreen}");
        NavigationService.NavigateTo(CurrentViews.TravelScreen);
        Console.WriteLine($"New current screen: {CurrentScreen}");
        StateHasChanged();
    }

    public void SwitchToMarketScreen()
    {
        NavigationService.NavigateTo(CurrentViews.MarketScreen);
        StateHasChanged();
    }

    public void SwitchToRestScreen()
    {
        NavigationService.NavigateTo(CurrentViews.RestScreen);
        StateHasChanged();
    }


    public void SwitchToPlayerStatusScreen()
    {
        NavigationService.NavigateTo(CurrentViews.PlayerStatusScreen);
        StateHasChanged();
    }

    public void SwitchToRelationshipScreen()
    {
        NavigationService.NavigateTo(CurrentViews.RelationshipScreen);
        StateHasChanged();
    }

    public void SwitchToObligationsScreen()
    {
        NavigationService.NavigateTo(CurrentViews.ObligationsScreen);
        StateHasChanged();
    }

    public void SwitchToLetterBoardScreen()
    {
        NavigationService.NavigateTo(CurrentViews.LetterBoardScreen);
        StateHasChanged();
    }

    public async Task HandleTravelRoute(RouteOption route)
    {
        await GameManager.Travel(route);
        NavigationService.NavigateTo(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task HandleTravelWithTransport((RouteOption route, TravelMethods transport) travelData)
    {
        await GameManager.TravelWithTransport(travelData.route, travelData.transport);
        NavigationService.NavigateTo(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task HandleRestComplete()
    {
        NavigationService.NavigateTo(CurrentViews.LocationScreen);
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
            NavigationService.NavigateTo(CurrentViews.EncounterScreen);
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
        NavigationService.NavigateTo(CurrentViews.NarrativeScreen);

        UpdateState();
    }

    public async Task HandleTravelStart(string travelDestination)
    {
        List<RouteOption> routes = GameManager.GetAvailableRoutes(LocationRepository.GetCurrentLocation().Id, travelDestination);
        RouteOption routeOption = routes.FirstOrDefault();

        await GameManager.Travel(routeOption);

        NavigationService.NavigateTo(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task OnNarrativeCompleted()
    {
        BeatOutcome = null;

        NavigationService.NavigateTo(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task UseResource(ActionNames actionName)
    {
        NavigationService.NavigateTo(CurrentViews.LocationScreen);
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
    /// Get current equipment categories owned by player
    /// </summary>
    private List<ItemCategory> GetCurrentEquipmentCategories()
    {
        List<ItemCategory> ownedCategories = new List<ItemCategory>();

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
    /// Show a direct letter offer from an NPC
    /// </summary>
    public void ShowLetterOffer(NPC npc)
    {
        CurrentNPCOffer = npc;
        ShowLetterOfferDialog = true;
        StateHasChanged();
    }
    
    /// <summary>
    /// Accept a letter offer from an NPC by offer ID
    /// </summary>
    public void AcceptLetterOfferId(string offerId)
    {
        if (CurrentNPCOffer == null) return;
        
        // Use the NPCLetterOfferService to accept the offer
        bool success = NPCLetterOfferService.AcceptNPCLetterOffer(CurrentNPCOffer.ID, offerId);
        
        if (success)
        {
            ShowLetterOfferDialog = false;
            CurrentNPCOffer = null;
            StateHasChanged();
        }
    }
    
    /// <summary>
    /// Refuse a letter offer from an NPC
    /// </summary>
    public void RefuseLetterOffer()
    {
        ShowLetterOfferDialog = false;
        CurrentNPCOffer = null;
        StateHasChanged();
    }
    
    /// <summary>
    /// Handle morning summary continuation
    /// </summary>
    public void HandleMorningSummaryContinue()
    {
        ShowMorningSummary = false;
        
        // If letter board is available, switch to it
        if (GameWorld.TimeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn)
        {
            SwitchToLetterBoardScreen();
        }
        
        StateHasChanged();
    }
    
    private void OnNavigationChanged(CurrentViews newView)
    {
        InvokeAsync(StateHasChanged);
    }
    
    public void HandleNavigation(CurrentViews view)
    {
        // Navigation is already handled by NavigationService
        // This callback is just for any additional logic needed
        StateHasChanged();
    }
    
    public void Dispose()
    {
        NavigationService.OnNavigationChanged -= OnNavigationChanged;
    }
}
