using Microsoft.AspNetCore.Components;
namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    #region Injected Services
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private MessageSystem MessageSystem { get; set; }
    #endregion

    #region Player State Properties
    public PlayerState PlayerState => GameState.PlayerState;
    public PlayerState Player => PlayerState; // Alias for compatibility

    // Player Resources
    public int Energy => PlayerState.Energy;
    public int MaxEnergy => PlayerState.MaxEnergy;
    public int Health => PlayerState.Health;
    public int MaxHealth => PlayerState.MaxHealth;
    public int Concentration => PlayerState.Concentration;
    public int MaxConcentration => PlayerState.MaxConcentration;
    public int Confidence => PlayerState.Confidence;
    public int MaxConfidence => PlayerState.MaxConfidence;
    public int Coins => PlayerState.Coins;
    #endregion

    #region World State Properties
    public Location CurrentLocation => GameState.WorldState.CurrentLocation;
    public LocationSpot CurrentSpot => GameState.WorldState.CurrentLocationSpot;
    public TimeWindows CurrentTime => GameState.WorldState.WorldTime;
    public int CurrentHour => GameState.WorldState.CurrentTimeInHours;
    public List<Location> Locations => GameManager.GetPlayerKnownLocations();
    public bool ShowEncounterResult { get; set; } = false;
    public bool OngoingEncounter { get; private set; }
    public EncounterResult EncounterResult => GameState.ActionStateTracker.EncounterResult;
    public ActionImplementation ActionImplementation => GameState.ActionStateTracker.CurrentAction.ActionImplementation;

    #endregion

    #region UI State
    // Navigation State
    private bool showAreaMap = true;
    private bool showNarrative = false;
    private bool needsCharacterCreation = false;
    private string selectedLocation;

    // Tooltip State
    public bool showTooltip = false;
    public UserActionOption hoveredAction;
    private double mouseX;
    private double mouseY;

    // Action Message State
    private bool showActionMessage = false;
    private string actionMessageType = "success";
    private List<string> actionMessages = new List<string>();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        needsCharacterCreation = !PlayerState.IsInitialized;
        if (!needsCharacterCreation)
        {
            await InitializeGame();
        }
    }

    private async Task InitializeGame()
    {
        await GameManager.StartGame();
        showAreaMap = false;
        StateHasChanged();
    }
    #endregion

    #region Navigation and UI Methods
    public void SwitchAreaMap()
    {
        showAreaMap = !showAreaMap;
    }

    public Location GetCurrentLocation()
    {
        return CurrentLocation ?? new Location() { Name = "Default" };
    }

    private async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.Name);
    }

    #endregion

    #region Action and Encounter Methods

    private async Task HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return;

        // Use unified action execution
        await GameManager.ExecuteAction(action);

        // Update UI state based on results
        OngoingEncounter = GameState.ActionStateTracker.IsActiveEncounter;

        // Display messages
        DisplayActionMessages();

        StateHasChanged();
    }

    private async Task HandleTravelStart(string travelLocationName)
    {
        selectedLocation = travelLocationName;

        // If already at this location, just switch to spot view
        if (travelLocationName == CurrentLocation.Name)
        {
            showAreaMap = false;
            StateHasChanged();
            return;
        }

        // Use unified travel initiation
        await GameManager.InitiateTravelToLocation(travelLocationName);

        // Update UI state
        OngoingEncounter = GameState.ActionStateTracker.IsActiveEncounter;
        showAreaMap = false;
        StateHasChanged();
    }

    private string GetTimeOfDayStyle()
    {
        return CurrentTime switch
        {
            TimeWindows.Morning => "time-morning",
            TimeWindows.Afternoon => "time-afternoon",
            TimeWindows.Evening => "time-evening",
            TimeWindows.Night => "time-night",
            _ => ""
        };
    }

    private async Task WaitOneHour()
    {
        // Create a "Wait" action that advances time without other effects
        SpotAction waitAction = new SpotAction
        {
            Name = "Wait",
            ActionId = "Wait",
            ActionType = ActionTypes.Basic,
            BasicActionType = BasicActionTypes.Rest,
            TimeCostHours = 1,
            IsRepeatable = true,
            // Define minimal energy cost for waiting
            Energy = new List<Outcome> { new EnergyOutcome(-1) }
        };

        ActionImplementation waitImpl = GameManager.ActionFactory.CreateActionFromTemplate(waitAction);

        UserActionOption waitOption = new UserActionOption(
            "Wait", "Wait for one hour", false, waitImpl,
            GameState.WorldState.CurrentLocation?.Name ?? "Global",
            GameState.WorldState.CurrentLocationSpot?.Name ?? "Global",
            null, 0, null);

        await GameManager.ExecuteAction(waitOption);
        StateHasChanged();
    }

    private async Task UseResource(ActionNames actionName)
    {
        UserActionOption globalAction = GameState.ActionStateTracker.GlobalActions
            .FirstOrDefault(a => a.ActionId == actionName.ToString());

        if (globalAction != null && !globalAction.IsDisabled)
        {
            await GameManager.ExecuteAction(globalAction);
            
            if (GameState.GameMode == Modes.Tutorial)
            {
                if (actionName == ActionNames.ConsumeFood)
                    GameState.TutorialState.SetFlag(TutorialState.TutorialFlags.UsedFood);

                if (actionName == ActionNames.ConsumeMedicinalHerbs)
                    GameState.TutorialState.SetFlag(TutorialState.TutorialFlags.UsedHerbs);
            }
        }

        DisplayActionMessages(); 
        StateHasChanged();
    }

    private void OnEncounterCompleted(EncounterResult result)
    {
        if (result.EncounterResults != EncounterResults.Ongoing)
        {
            OngoingEncounter = false;
            ShowEncounterResult = true;
        }
        DisplayActionMessages(); 
        StateHasChanged();
    }

    private async Task OnNarrativeCompleted()
    {
        showNarrative = false;
        ShowEncounterResult = false;

        showAreaMap = false;

        ActionImplementation actionImplementation = EncounterResult.Encounter.ActionImplementation;
        await GameManager.ProcessActionCompletion(actionImplementation);

        DisplayActionMessages(); 
        StateHasChanged();
    }

    private async Task HandleCharacterCreated(PlayerState playerState)
    {
        needsCharacterCreation = false;
        await InitializeGame();
    }
    #endregion

    #region UI Display Methods
    private void DisplayActionMessages()
    {
        actionMessages = GetResultMessages();

        if (actionMessages.Any())
        {
            showActionMessage = true;
            actionMessageType = "success";  // Default to success

            // Set message type based on content analysis
            if (actionMessages.Any(m => m.Contains("not enough") || m.Contains("cannot")))
            {
                actionMessageType = "warning";
            }

            // Auto-dismiss after 5 seconds
            Task.Delay(5000).ContinueWith(_ =>
            {
                InvokeAsync(() =>
                {
                    showActionMessage = false;
                    StateHasChanged();
                });
            });
        }
    }

    private void DismissActionMessage()
    {
        showActionMessage = false;
    }

    public List<string> GetResultMessages()
    {
        ActionResultMessages messages = MessageSystem.GetAndClearChanges();
        List<string> list = new();

        if (messages == null) return list;

        // Add outcome descriptions
        foreach (Outcome outcome in messages.Outcomes)
        {
            list.Add(outcome.GetDescription());
        }

        // Add system messages
        foreach (SystemMessage sysMsg in messages.SystemMessages)
        {
            list.Add(sysMsg.Message);
        }

        return list;
    }

    private List<PropertyDisplay> GetLocationProperties(Location location)
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();
        WorldState world = GameState.WorldState;

        // Add time property
        properties.Add(new PropertyDisplay(
            GetIconForTimeWindow(world.WorldTime),
            FormatEnumString(world.WorldTime.ToString()),
            "", "", ""
        ));

        // Add weather property
        properties.Add(new PropertyDisplay(
            GetIconForWeatherType(world.WorldWeather),
            FormatEnumString(world.WorldWeather.ToString()),
            "", "", ""
        ));

        return properties;
    }
    #endregion

    #region Helper Methods
    private string GetIconForTimeWindow(TimeWindows time)
    {
        return time switch
        {
            TimeWindows.Night => "🌙",
            TimeWindows.Morning => "🌄",
            TimeWindows.Afternoon => "☀️",
            TimeWindows.Evening => "🌆",
            _ => "❓"
        };
    }

    private string GetIconForWeatherType(WeatherTypes type)
    {
        return type switch
        {
            WeatherTypes.Clear => "🌤️",
            WeatherTypes.Sunny => "☀️",
            WeatherTypes.Windy => "💨",
            WeatherTypes.Stormy => "⛈️",
            _ => "❓"
        };
    }

    private string FormatEnumString(string value)
    {
        return string.Concat(value
            .Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()))
            .Replace("Type", "")
            .Replace("Types", "");
    }

    private string GetArchetypeIcon(ArchetypeTypes archetype)
    {
        return archetype switch
        {
            ArchetypeTypes.Warrior => "⚔️",
            ArchetypeTypes.Scholar => "📚",
            ArchetypeTypes.Ranger => "🏹",
            ArchetypeTypes.Bard => "🎵",
            ArchetypeTypes.Thief => "🗝️",
            _ => "❓"
        };
    }

    private string GetItemIcon(ItemTypes itemType)
    {
        return itemType switch
        {
            ItemTypes.Sword => "⚔️",
            ItemTypes.Shield => "🛡️",
            ItemTypes.Bow => "🏹",
            ItemTypes.Arrow => "🪶",
            ItemTypes.Dagger => "🔪",
            ItemTypes.Lockpicks => "🗝️",
            ItemTypes.Book => "📚",
            ItemTypes.Scroll => "📜",
            ItemTypes.Lute => "🎵",
            ItemTypes.Rope => "🧶",
            ItemTypes.Rations => "🍖",
            ItemTypes.LeatherArmor => "👕",
            ItemTypes.WritingKit => "✒️",
            ItemTypes.HuntingKnife => "🔪",
            ItemTypes.HealingHerbs => "🍃",
            ItemTypes.FineClothes => "👘",
            ItemTypes.WineBottle => "🍷",
            ItemTypes.ClimbingGear => "⛏️",
            _ => "📦"
        };
    }

    private string FormatItemName(ItemTypes itemType)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            itemType.ToString(),
            "([A-Z])",
            " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    private string GetItemDescription(ItemTypes itemType)
    {
        return itemType switch
        {
            ItemTypes.Sword => "A sturdy steel sword",
            ItemTypes.Shield => "A wooden shield with metal binding",
            ItemTypes.Bow => "A hunting bow made of yew",
            ItemTypes.Arrow => "Sharp arrows with fletching",
            ItemTypes.Dagger => "A small but sharp blade",
            ItemTypes.Lockpicks => "Tools for picking locks",
            ItemTypes.Book => "A tome of knowledge",
            ItemTypes.Scroll => "A rolled parchment with writing",
            ItemTypes.Lute => "A stringed musical instrument",
            ItemTypes.Rope => "Strong hemp rope",
            ItemTypes.Rations => "Dried food for travel",
            ItemTypes.LeatherArmor => "Protective leather garments",
            ItemTypes.WritingKit => "Quill, ink and parchment",
            ItemTypes.HuntingKnife => "A knife for skinning game",
            ItemTypes.HealingHerbs => "Medicinal plants",
            ItemTypes.FineClothes => "Well-made attire suitable for performance",
            ItemTypes.WineBottle => "A bottle of reasonably good wine",
            ItemTypes.ClimbingGear => "Tools for scaling walls",
            _ => "A common item"
        };
    }
    #endregion
}