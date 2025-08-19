using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Wayfarer.ViewModels;

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
    public ConversationViewModel CurrentConversation { get; set; }
    public ConversationResult ConversationResult;
    public ConversationBeatOutcome ConversationBeatOutcome { get; set; }

    // Navigation State
    public string SelectedLocation { get; set; }
    public string SelectedNpcId { get; set; }
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

    // Letter Offer State
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

    // Backward compatibility for child components
    public TimeBlocks CurrentTime => CurrentTimeBlock;
    public ConversationManager ConversationManager { get; set; } // For compatibility

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
        (Location location, LocationSpot spot) = GameFacade.GetCurrentLocation();
        CurrentLocation = location;
        CurrentSpot = spot;

        // Pull system messages
        SystemMessages = GameFacade.GetSystemMessages().Where(m => !m.IsExpired).ToList();

        // Check for daily activities
        DailyActivityResult dailyActivities = GameFacade.GetDailyActivities();
        if (dailyActivities != null && dailyActivities.HasEvents && !ShowDailySummary)
        {
            Console.WriteLine("MainGameplayView - Daily activities pending - processing");
            ProcessDailyActivities();
        }

        // Check for pending conversation
        CurrentConversation = GameFacade.GetCurrentConversation();
        Console.WriteLine($"[MainGameplayView.PollGameState] CurrentConversation null? {CurrentConversation == null}");
        if (CurrentConversation != null)
        {
            Console.WriteLine($"[MainGameplayView.PollGameState] CurrentConversation.IsComplete = {CurrentConversation.IsComplete}");
        }

        if (CurrentConversation != null && CurrentConversation.IsComplete == false)
        {
            Console.WriteLine("[MainGameplayView.PollGameState] ConversationPending detected!");
            Console.WriteLine($"[MainGameplayView.PollGameState] OnNavigate null? {OnNavigate == null}");
            Console.WriteLine($"[MainGameplayView.PollGameState] CurrentScreen before invoke: {CurrentScreen}");

            // For backward compatibility, create a ConversationManager if needed
            if (ConversationManager == null)
            {
                // This is a shim for compatibility - ideally child components should use CurrentConversation
                ConversationManager = new ConversationManager(null, null, null, null);
            }

            if (OnNavigate != null && CurrentScreen != CurrentViews.ConversationScreen)
            {
                Console.WriteLine($"[MainGameplayView.PollGameState] Invoking OnNavigate with ConversationScreen...");
                OnNavigate.Invoke(CurrentViews.ConversationScreen);
                Console.WriteLine($"[MainGameplayView.PollGameState] OnNavigate invoked successfully");
            }
            else if (OnNavigate == null)
            {
                Console.WriteLine($"[MainGameplayView.PollGameState] ERROR: OnNavigate is null!");
            }

            Console.WriteLine($"[MainGameplayView.PollGameState] CurrentScreen after invoke: {CurrentScreen}");
            Console.WriteLine($"[MainGameplayView.PollGameState] Calling StateHasChanged...");
            StateHasChanged();
            Console.WriteLine($"[MainGameplayView.PollGameState] StateHasChanged completed");
        }
        else if (CurrentScreen == CurrentViews.ConversationScreen && (CurrentConversation == null || CurrentConversation.IsComplete))
        {
            // Conversation ended - return to LocationScreen
            Console.WriteLine("[MainGameplayView.PollGameState] Conversation ended - returning to LocationScreen");
            if (OnNavigate != null)
            {
                OnNavigate.Invoke(CurrentViews.LocationScreen);
                StateHasChanged();
            }
        }

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
        bool success = await GameFacade.TravelToDestinationAsync(route.Destination, routeId);

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
        (Location location, LocationSpot _) = GameFacade.GetCurrentLocation();
        return location;
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
            (Location location, LocationSpot spot) = GameFacade.GetCurrentLocation();
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
        // Get current conversation result from facade if needed
        ConversationViewModel? conversation = GameFacade.GetCurrentConversation();
        if (conversation != null && conversation.ConversationTopic == "QueueManagement")
        {
            // Handle queue management completion through facade
            // The facade should handle the queue operations internally
            OnNavigate?.Invoke(CurrentViews.LetterQueueScreen);
        }
        else
        {
            // Check for tutorial patronage acceptance
            if (conversation?.NpcId == "patron_intermediary")
            {
                NarrativeStateViewModel narrativeState = GameFacade.GetNarrativeState();
                if (narrativeState.IsTutorialActive)
                {
                    TutorialGuidanceViewModel tutorialGuidance = GameFacade.GetTutorialGuidance();
                    if (tutorialGuidance.StepTitle?.Contains("patronage") == true)
                    {
                        // Patron acceptance is handled by the backend through facade
                        // Messages are already added by backend
                    }
                }
            }

            // Switch to narrative screen to show result
            OnNavigate?.Invoke(CurrentViews.NarrativeScreen);
        }

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
        ConversationBeatOutcome = null;
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
    /// Get current equipment categories owned by player
    /// </summary>
    private List<ItemCategory> GetCurrentEquipmentCategories()
    {
        InventoryViewModel inventory = GameFacade.GetInventory();
        List<ItemCategory> categories = new List<ItemCategory>();

        // This requires extending the facade or ViewModels to include item categories
        // For now, returning empty list for compatibility

        return categories.Distinct().ToList();
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
    public void ShowLetterOfferForNPC(string npcId)
    {
        // Get letter board offers and find the one from this NPC
        LetterBoardViewModel letterBoard = GameFacade.GetLetterBoard();
        List<NPCWithOffersViewModel> npcsWithOffers = GameFacade.GetNPCsWithOffers();
        NPCWithOffersViewModel? npcWithOffer = npcsWithOffers.FirstOrDefault(n => n.NPCId == npcId);

        if (npcWithOffer != null && letterBoard.IsAvailable && letterBoard.Offers != null)
        {
            LetterOfferViewModel? offer = letterBoard.Offers.FirstOrDefault(o => o.SenderName == npcWithOffer.NPCName);
            if (offer != null)
            {
                CurrentLetterOffer = offer;
                CurrentNPCOfferId = npcId;
                ShowLetterOfferDialog = true;
                StateHasChanged();
            }
        }
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

    // Backward compatibility methods for child components
    public List<NPC> GetNPCsAtCurrentSpot()
    {
        // Get NPCs through location actions
        LocationActionsViewModel locationActions = GameFacade.GetLocationActions();
        List<NPC> npcList = new List<NPC>();

        // Extract NPCs from conversation actions
        ActionGroupViewModel? conversationGroup = locationActions.ActionGroups
            .FirstOrDefault(g => g.ActionType == "Conversation");

        if (conversationGroup != null)
        {
            foreach (ActionOptionViewModel action in conversationGroup.Actions)
            {
                // Create NPC object from action data
                // This is a compatibility shim - ideally child components should use ViewModels
                NPC npc = new NPC
                {
                    ID = action.Id.Replace("talk_", ""),
                    Name = action.Description.Replace("Talk to ", ""),
                    SpotId = CurrentSpot?.SpotID
                };
                npcList.Add(npc);
            }
        }

        return npcList;
    }

    public List<NPC> GetAvailableNPCsAtCurrentSpot()
    {
        // For now, return same as GetNPCsAtCurrentSpot
        // Child components should ideally use the LocationActionsViewModel directly
        return GetNPCsAtCurrentSpot();
    }

    // Backward compatibility properties
    public List<Location> Locations => TravelDestinations?.Select(d => new Location(d.LocationId, d.LocationName)).ToList() ?? new List<Location>();

    // These are now delegated through GameFacade but kept for backward compatibility
    public DebugLogger DebugLogger => null; // Stub for compatibility - use Console.WriteLine instead
    public ITimeManager TimeManager => null; // Should use GameFacade.GetTimeInfo() instead
    public ConversationStateManager ConversationStateManager => null; // Should use GameFacade.GetCurrentConversation() instead
    // Narrative system replaced with conversation system
    public FlagService FlagService => null; // Should use GameFacade.IsTutorialActive() instead
    public StandingObligationManager StandingObligationManager => null; // Should use GameFacade methods instead
    public StandingObligationRepository StandingObligationRepository => null; // Should use GameFacade methods instead
    public PatronLetterService PatronLetterService => null; // Should use GameFacade methods instead
    public MessageSystem MessageSystem => null; // Should use GameFacade.GetSystemMessages() instead
    public NPCLetterOfferService NPCLetterOfferService => null; // Should use GameFacade.AcceptLetterOfferAsync() instead
    public LetterQueueManager LetterQueueManager => null; // Should use GameFacade.ExecuteLetterActionAsync() instead
    public TokenMechanicsManager TokenMechanicsManager => null; // Should use GameFacade methods instead
    public NPCRepository NPCRepository => null; // Should use GameFacade.GetLocationActions() instead
    public LocationRepository LocationRepository => null; // Should use GameFacade.GetCurrentLocation() instead
    public ItemRepository ItemRepository => null; // Should use GameFacade.GetInventory() instead
    public LoadingStateService LoadingStateService => null; // No longer needed with facade
    // All operations go through GameFacade
    public GameWorld GameWorld => null; // Never access GameWorld directly - use GameFacade

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

    // Helper method to get NPC by ID for backward compatibility
    public NPC GetNPCById(string npcId)
    {
        // Create a stub NPC for the dialog using data from facade
        List<NPCWithOffersViewModel> npcsWithOffers = GameFacade.GetNPCsWithOffers();
        NPCWithOffersViewModel? npcData = npcsWithOffers.FirstOrDefault(n => n.NPCId == npcId);

        if (npcData != null)
        {
            return new NPC
            {
                ID = npcId,
                Name = npcData.NPCName,
                Role = npcData.Role,
                SpotId = CurrentSpot?.SpotID
            };
        }

        // Fallback
        return new NPC
        {
            ID = npcId,
            Name = CurrentLetterOffer?.SenderName ?? "Unknown",
            SpotId = CurrentSpot?.SpotID
        };
    }

    public void Dispose()
    {
        // Clean architecture - no events to unsubscribe from
    }
}