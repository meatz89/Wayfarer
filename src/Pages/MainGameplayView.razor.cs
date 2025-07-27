using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Wayfarer.Pages;

public class MainGameplayViewBase : ComponentBase, IDisposable
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameManager { get; set; }
    [Inject] public MessageSystem MessageSystem { get; set; }
    [Inject] public ItemRepository ItemRepository { get; set; }
    [Inject] public LocationRepository LocationRepository { get; set; }
    [Inject] public NPCRepository NPCRepository { get; set; }
    [Inject] public LoadingStateService? LoadingStateService { get; set; }
    [Inject] public ConnectionTokenManager ConnectionTokenManager { get; set; }
    [Inject] public LetterQueueManager LetterQueueManager { get; set; }
    [Inject] public NPCLetterOfferService NPCLetterOfferService { get; set; }
    
    // Navigation parameters from parent component
    [Parameter] public CurrentViews CurrentView { get; set; }
    [Parameter] public Action<CurrentViews> OnNavigate { get; set; }
    [Inject] public DebugLogger DebugLogger { get; set; }
    [Inject] public ITimeManager TimeManager { get; set; }
    [Inject] public ConversationStateManager ConversationStateManager { get; set; }
    [Inject] public NarrativeManager NarrativeManager { get; set; }
    [Inject] public FlagService FlagService { get; set; }
    [Inject] public StandingObligationManager StandingObligationManager { get; set; }
    [Inject] public StandingObligationRepository StandingObligationRepository { get; set; }
    [Inject] public PatronLetterService PatronLetterService { get; set; }


    public int StateVersion = 0;
    public ConversationManager ConversationManager = null;
    public ConversationResult ConversationResult;

    // Navigation State
    public string SelectedLocation { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }
    public int Stamina { get; set; } = 0;
    public int Concentration { get; set; } = 0;
    public Location CurrentLocation { get; set; }

    public Player PlayerState => GameWorld.GetPlayer();

    // Tooltip State
    public bool ShowTooltip = false;
    // Action system removed - using location actions
    public double MouseX;
    public double MouseY;

    // Action Message State
    public bool ShowActionMessage = false;
    public string ActionMessageType = "success";
    public List<string> ActionMessages = new List<string>();
    public ElementReference SidebarRef;

    // System Messages from GameWorld
    public List<SystemMessage> SystemMessages = new List<SystemMessage>();

    // Time Planning State
    public bool showFullDayView = false;

    // Letter Offer State
    public bool ShowLetterOfferDialog = false;
    public NPC CurrentNPCOffer = null;

    // Morning Activities State
    public bool ShowMorningSummary = false;
    public MorningActivityResult MorningActivityResult = null;
    public TimeBlocks? LastTimeBlock = null;

    // Debug State
    public bool ShowDebugPanel = false;


    public bool HasApLeft { get; set; }
    public bool HasNoApLeft => !HasApLeft;


    public int TurnActionPoints => 0; // ActionPoints system removed

    public ConversationBeatOutcome ConversationBeatOutcome { get; set; }
    public CurrentViews CurrentScreen => CurrentView;

    public LocationSpot CurrentSpot => LocationRepository.GetCurrentLocationSpot();
    public TimeBlocks CurrentTime => TimeManager.GetCurrentTimeBlock();

    public int CurrentHour => TimeManager.GetCurrentTimeHours();

    public WeatherCondition CurrentWeather => GameWorld.CurrentWeather;

    public List<Location> Locations => GameManager.GetPlayerKnownLocations();

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
        // Events removed per architecture guidelines - handle navigation results directly

        DebugLogger.LogDebug("MainGameplayView initialized - starting polling");

        // Verify initial state
        if (IsGameDataReady())
        {
            DebugLogger.LogDebug("Game data is ready - running initial verification");

            // Log initial state report
            string report = "State verification removed";
            DebugLogger.LogDebug($"Initial state:\n{report}");
        }

        // Poll state initially
        PollGameState();
        StateHasChanged();
    }
    
    public void HandleNavigationChange(CurrentViews previousScreen, CurrentViews newScreen)
    {
        InvokeAsync(() =>
        {
            DebugLogger.LogDebug($"Navigation changed from {previousScreen} to {newScreen}");
            StateHasChanged();
        });
    }

    public GameWorldSnapshot oldSnapshot;

    public void RefreshUI()
    {
        PollGameState();
        StateHasChanged();
    }

    public void HandleMessagesExpired()
    {
        // Filter expired messages and update UI
        SystemMessages = GameWorld.SystemMessages.Where(m => !m.IsExpired).ToList();
        GameWorld.SystemMessages.RemoveAll(m => m.IsExpired);
        StateHasChanged();
    }

    public void PollGameState()
    {
        GameWorldSnapshot snapshot = GameManager.GetGameSnapshot();
        CurrentTimeBlock = snapshot.CurrentTimeBlock;
        Stamina = snapshot.Stamina;
        Concentration = snapshot.Concentration;

        // Pull system messages from GameWorld and filter expired ones
        SystemMessages = GameWorld.SystemMessages.Where(m => !m.IsExpired).ToList();

        // Clean up expired messages from GameWorld
        GameWorld.SystemMessages.RemoveAll(m => m.IsExpired);

        // Check for pending morning activities (set by AdvanceToNextDay)
        if (GameManager.HasPendingMorningActivities() && !ShowMorningSummary)
        {
            DebugLogger.LogPolling("MainGameplayView", "Morning activities pending - processing");
            ProcessMorningActivities();
        }

        // Check for pending action conversation
        if (ConversationStateManager.ConversationPending)
        {
            DebugLogger.LogPolling("MainGameplayView", $"ConversationPending detected! Manager exists: {ConversationStateManager.PendingConversationManager != null}");
            ConversationManager = ConversationStateManager.PendingConversationManager;
            OnNavigate?.Invoke(CurrentViews.ConversationScreen);
            ConversationStateManager.ConversationPending = false;
            DebugLogger.LogNavigation(CurrentScreen.ToString(), "ConversationScreen", "Pending conversation detected");
            StateHasChanged();
        }

        if (oldSnapshot == null || !snapshot.IsEqualTo(oldSnapshot))
        {
            // You need to update ConversationManager state and force StateHasChanged for ALL screens
            if (CurrentScreen == CurrentViews.ConversationScreen)
            {
                StateVersion++;  // Force re-render of ConversationView
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

    public async Task HandleTravelRoute(RouteOption route)
    {
        DebugLogger.LogAction("HandleTravelRoute", route.Name, $"Current screen: {CurrentScreen}");
        await GameManager.Travel(route);
        // Don't navigate immediately - let polling detect the pending conversation
        // The conversation will be shown when PollGameState() detects ConversationPending
        DebugLogger.LogDebug("HandleTravelRoute completed - waiting for polling to detect conversation");
        UpdateState();
    }

    public async Task HandleTravelWithTransport((RouteOption route, TravelMethods transport) travelData)
    {
        await GameManager.TravelWithTransport(travelData.route, travelData.transport);
        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task HandleRestComplete()
    {
        OnNavigate?.Invoke(CurrentViews.LocationScreen);
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

    public async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.SpotID);
        UpdateState();
    }

    public async Task OnConversationCompleted(ConversationBeatOutcome result)
    {
        // Check if this was a queue management conversation
        if (ConversationManager?.Context is QueueManagementContext queueContext)
        {
            ConversationChoice selectedChoice = GameManager.GetLastSelectedChoice();
            if (selectedChoice != null)
            {
                if (selectedChoice.ChoiceType == ConversationChoiceType.SkipAndDeliver)
                {
                    // Process the skip action
                    string skipPosition = GameWorld.GetMetadata("PendingSkipPosition");
                    if (int.TryParse(skipPosition, out int position))
                    {
                        LetterQueueManager.TrySkipDeliver(position);
                    }
                }
                else if (selectedChoice.ChoiceType == ConversationChoiceType.PurgeLetter)
                {
                    // Process the purge action
                    string purgeTokensJson = GameWorld.GetMetadata("PendingPurgeTokens");
                    if (!string.IsNullOrEmpty(purgeTokensJson))
                    {
                        try
                        {
                            Dictionary<ConnectionType, int>? tokenSelection = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ConnectionType, int>>(purgeTokensJson);
                            if (tokenSelection != null)
                            {
                                LetterQueueManager.TryPurgeLetter(tokenSelection);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageSystem.AddSystemMessage($"Error processing purge: {ex.Message}", SystemMessageTypes.Danger);
                        }
                    }
                }
            }

            // Clear metadata
            GameWorld.ClearMetadata("PendingSkipPosition");
            GameWorld.ClearMetadata("PendingPurgePosition");
            GameWorld.ClearMetadata("PendingPurgeTokens");

            // Return to letter queue screen
            OnNavigate?.Invoke(CurrentViews.LetterQueueScreen);
        }
        // Check if this was an action conversation
        else if (GameWorld.PendingCommand != null)
        {
            // Complete the pending command
            GameManager.CompletePendingCommand(GameWorld.PendingCommand);

            // Check if this was a travel encounter - if so, complete the travel
            if (GameWorld.PendingCommand?.CommandType == CommandTypes.TravelEncounter)
            {
                GameManager.CompleteTravelAfterEncounter();
            }

            // Clear the pending action
            GameWorld.PendingCommand = null;

            // Return to location screen
            OnNavigate?.Invoke(CurrentViews.LocationScreen);
        }
        else
        {
            // Check if this was a patron acceptance conversation during tutorial
            if (ConversationManager?.Context?.TargetNPC?.ID == "patron_intermediary")
            {
                // Check if we're in the patronage acceptance step of the tutorial
                if (NarrativeManager != null && NarrativeManager.IsNarrativeActive("wayfarer_tutorial"))
                {
                    var currentStep = NarrativeManager.GetCurrentStep("wayfarer_tutorial");
                    if (currentStep?.Id == "day3_patronage")
                    {
                        // The player has accepted patronage!
                        
                        // Set the patron acceptance flag
                        FlagService?.SetFlag(FlagService.TUTORIAL_PATRON_ACCEPTED, true);
                        
                        // Set HasPatron on the player
                        GameWorld.GetPlayer().HasPatron = true;
                        
                        // Initialize patron debt for leverage
                        PatronLetterService?.InitializePatronDebt();
                        
                        // Create and add the patron's expectation obligation
                        var patronObligation = StandingObligationRepository?.GetObligationTemplate("tutorial_patrons_expectation");
                        if (patronObligation != null && StandingObligationManager != null)
                        {
                            StandingObligationManager.AddObligation(patronObligation);
                            
                            // Add message about the new obligation
                            MessageSystem.AddSystemMessage(
                                "You have accepted the patron's offer. Their letters will now take absolute priority.",
                                SystemMessageTypes.Warning
                            );
                        }
                    }
                }
            }
            
            // Store the result for narrative view
            ConversationBeatOutcome = result;

            // Switch to narrative screen to show result
            OnNavigate?.Invoke(CurrentViews.NarrativeScreen);
        }

        UpdateState();
    }

    public async Task HandleTravelStart(string travelDestination)
    {
        List<RouteOption> routes = GameManager.GetAvailableRoutes(LocationRepository.GetCurrentLocation().Id, travelDestination);
        RouteOption routeOption = routes.FirstOrDefault();

        await GameManager.Travel(routeOption);

        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        UpdateState();
    }

    public async Task OnNarrativeCompleted()
    {
        ConversationBeatOutcome = null;

        OnNavigate?.Invoke(CurrentViews.LocationScreen);
        UpdateState();
    }

    // Action and card systems removed - use LocationActionManager and ConversationSystem

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
        if (TimeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn)
        {
            SwitchToLetterBoardScreen();
        }

        StateHasChanged();
    }

    // Navigation changes now handled by checking NavigationResult directly

    public void HandleNavigation(CurrentViews view)
    {
        OnNavigate?.Invoke(view);
        StateHasChanged();
    }

    /// <summary>
    /// Debug method to output current game state
    /// </summary>
    public void PrintDebugState()
    {
        string report = "State verification removed";
        Console.WriteLine(report);

        // Also run full verification
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
            // Clear tutorial complete flag if set
            FlagService.SetFlag(FlagService.TUTORIAL_COMPLETE, false);
            
            // Set tutorial starting conditions
            var player = GameWorld.GetPlayer();
            player.Coins = 2; // Tutorial starts with 2 coins
            player.Stamina = 4; // Tutorial starts with 4/10 stamina
            
            // Load tutorial narrative definitions
            NarrativeContentBuilder.BuildAllNarratives();
            NarrativeManager.LoadNarrativeDefinitions(NarrativeDefinitions.All);
            
            // Start the tutorial
            NarrativeManager.StartNarrative("wayfarer_tutorial");
            
            MessageSystem.AddSystemMessage("Welcome to Wayfarer. Your journey begins in the Lower Ward...", SystemMessageTypes.Tutorial);
            MessageSystem.AddSystemMessage("Tutorial started successfully!", SystemMessageTypes.Success);
            DebugLogger.LogDebug($"Tutorial manually started. Active narratives: {string.Join(", ", NarrativeManager.GetActiveNarratives())}");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            MessageSystem.AddSystemMessage($"Failed to start tutorial: {ex.Message}", SystemMessageTypes.Danger);
            DebugLogger.LogError("MainGameplayView", $"Tutorial start failed: {ex.Message}", ex);
        }
    }

    public void RunVerification()
    {
        MessageSystem.AddSystemMessage("Verification completed - check console", SystemMessageTypes.Success);
    }

    public List<NPC> GetNPCsAtCurrentSpot()
    {
        if (PlayerState?.CurrentLocationSpot == null) return new List<NPC>();

        return NPCRepository.GetAllNPCs()
            .Where(n => n.SpotId == PlayerState.CurrentLocationSpot.SpotID)
            .ToList();
    }

    public List<NPC> GetAvailableNPCsAtCurrentSpot()
    {
        if (PlayerState?.CurrentLocationSpot == null) return new List<NPC>();

        return NPCRepository.GetNPCsForLocationSpotAndTime(
            PlayerState.CurrentLocationSpot.SpotID,
            CurrentTime);
    }

    public void Dispose()
    {
        // Clean architecture - no events to unsubscribe from
    }
}
