using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Xml.Xsl;
namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    #region Injected Services
    [Inject] private IJSRuntime JSRuntime { get; set; }
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

    public EncounterResult EncounterResult { get; private set; }
    #endregion

    #region World State Properties

    public CurrentViews CurrentScreen = CurrentViews.CharacterScreen;
    public Location CurrentLocation => GameState.WorldState.CurrentLocation;
    public LocationSpot CurrentSpot => GameState.WorldState.CurrentLocationSpot;
    public TimeWindows CurrentTime => GameState.WorldState.WorldTime;
    public int CurrentHour => GameState.WorldState.CurrentTimeInHours;
    public List<Location> Locations => GameManager.GetPlayerKnownLocations();

    public EncounterManager EncounterManager = null;
    public ActionImplementation ActionImplementation = null;

    private int StateVersion = 0;

    #endregion

    #region UI State
    // Navigation State
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
    public ElementReference sidebarRef;

    private Dictionary<SidebarSections, bool> expandedSections = new Dictionary<SidebarSections, bool>
    {
        { SidebarSections.skills, false },
        { SidebarSections.resources, false },
        { SidebarSections.inventory, false },
        { SidebarSections.status, false }
    };

    private async Task ToggleSection(SidebarSections sectionName)
    {
        if (expandedSections.ContainsKey(sectionName))
        {
            bool toggleOn = !expandedSections[sectionName];
            
            foreach (KeyValuePair<SidebarSections, bool> section in expandedSections)
            {
                expandedSections[section.Key] = false;
            }

            expandedSections[sectionName] = toggleOn;
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

    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
    }

    private async Task InitializeGame()
    {
        await GameManager.StartGame();

        CurrentScreen = CurrentViews.CharacterScreen;

        ChangeState();
    }

    private async Task HandleCharacterCreated(PlayerState playerState)
    {
        CurrentScreen = CurrentViews.LocationScreen;
    }

    #endregion

    #region Navigation and UI Methods
    public void SwitchAreaMap()
    {
        if (CurrentScreen == CurrentViews.MapScreen)
        {
            CurrentScreen = CurrentViews.LocationScreen;
        }
        else if (CurrentScreen == CurrentViews.LocationScreen)
        {
            CurrentScreen = CurrentViews.MapScreen;
        }
    }

    public Location GetCurrentLocation()
    {
        return CurrentLocation ?? new Location() { Name = "Default" };
    }

    private async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.Name);
        ChangeState();
    }

    #endregion

    #region Action and Encounter Methods

    private async Task HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return;

        ActionImplementation = await GameManager.ExecuteAction(action);

        EncounterManager = GameManager.EncounterSystem.GetCurrentEncounter();
        if (EncounterManager == null)
        {
            EncounterResult = GameManager.GetEncounterResultFor(action.ActionImplementation);
            await OnEncounterCompleted(EncounterResult);
        }
        else
        {
            CurrentScreen = CurrentViews.EncounterScreen;
        }

        ChangeState();
    }

    private async Task HandleTravelStart(string travelLocationName)
    {
        selectedLocation = travelLocationName;

        if (travelLocationName == CurrentLocation.Name)
        {
            ChangeState();
            return;
        }

        await GameManager.InitiateTravelToLocation(travelLocationName);

        CurrentScreen = CurrentViews.EncounterScreen;
        ChangeState();
    }

    private async Task OnEncounterCompleted(EncounterResult result)
    {
        ActionImplementation actionImplementation = result.ActionImplementation;
        await GameManager.ProcessActionCompletion(actionImplementation);

        EncounterResult = result;
        CurrentScreen = CurrentViews.NarrativeScreen;
        ChangeState();
    }

    private async Task OnNarrativeCompleted()
    {
        EncounterResult = null;

        CurrentScreen = CurrentViews.LocationScreen;
        ChangeState();
    }

    private async Task UseResource(ActionNames actionName)
    {
        UserActionOption globalAction = GameState.ActionStateTracker.GlobalActions
            .FirstOrDefault(a => a.ActionId == actionName.ToString());

        if (globalAction != null && !globalAction.IsDisabled)
        {
            await GameManager.ExecuteAction(globalAction);
        }

        CurrentScreen = CurrentViews.LocationScreen;
        ChangeState();
    }


    private async Task WaitOneHour()
    {
        // Create a "Wait" action that advances time without other effects
        ActionDefinition waitAction = new ActionDefinition("Wait", "Wait", 0, 0, EncounterTypes.Physical, true)
        {
            TimeCost = 1,
        };

        ActionImplementation waitImpl = GameManager.ActionFactory.CreateActionFromTemplate(waitAction);

        UserActionOption waitOption = new UserActionOption(
            "Wait for one hour", false, waitImpl,
            GameState.WorldState.CurrentLocation?.Name ?? "Global",
            GameState.WorldState.CurrentLocationSpot?.Name ?? "Global",
            null, 0, null);

        await GameManager.ExecuteAction(waitOption);

        ChangeState();
    }


    public void ChangeState()
    {
        DisplayActionMessages();

        StateVersion++;
        StateHasChanged();
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
            ArchetypeTypes.Knight => "⚔️",
            ArchetypeTypes.Sage => "📚",
            ArchetypeTypes.Forester => "🏹",
            ArchetypeTypes.Courtier => "🎵",
            ArchetypeTypes.Shadow => "🗝️",
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
            ItemTypes.Snares => "🪶",
            ItemTypes.Dagger => "🔪",
            ItemTypes.Lockpicks => "🗝️",
            ItemTypes.Journal => "📚",
            ItemTypes.Spectacles => "📜",
            ItemTypes.WaxSealKit => "🎵",
            ItemTypes.GrapplingHook => "🧶",
            ItemTypes.FlintAndSteel => "🍖",
            ItemTypes.Chainmail => "👕",
            ItemTypes.QuillAndInk => "✒️",
            ItemTypes.SkinningKnife => "🔪",
            ItemTypes.HerbPouch => "🍃",
            ItemTypes.FineClothes => "👘",
            ItemTypes.WineFlask => "🍷",
            ItemTypes.DarkCloak => "⛏️",
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
            ItemTypes.Snares => "Sharp arrows with fletching",
            ItemTypes.Dagger => "A small but sharp blade",
            ItemTypes.Lockpicks => "Tools for picking locks",
            ItemTypes.Journal => "A tome of knowledge",
            ItemTypes.Spectacles => "A rolled parchment with writing",
            ItemTypes.WaxSealKit => "A stringed musical instrument",
            ItemTypes.GrapplingHook => "Strong hemp rope",
            ItemTypes.FlintAndSteel => "Dried food for travel",
            ItemTypes.Chainmail => "Protective leather garments",
            ItemTypes.QuillAndInk => "Quill, ink and parchment",
            ItemTypes.SkinningKnife => "A knife for skinning game",
            ItemTypes.HerbPouch => "Medicinal plants",
            ItemTypes.FineClothes => "Well-made attire suitable for performance",
            ItemTypes.WineFlask => "A bottle of reasonably good wine",
            ItemTypes.DarkCloak => "Tools for scaling walls",
            _ => "A common item"
        };
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
    #endregion
}

public enum SidebarSections
{
    skills, resources, inventory, status
}