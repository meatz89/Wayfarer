using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private MessageSystem MessageSystem { get; set; }


    public PlayerState PlayerState
    {
        get
        {
            return GameState.PlayerState;
        }
    }

    public PlayerState Player
    {
        get
        {
            return PlayerState; // Alias for compatibility
        }
    }

    // Player Resources
    public int Energy
    {
        get
        {
            return PlayerState.Energy;
        }
    }

    public int MaxEnergy
    {
        get
        {
            return PlayerState.MaxEnergy;
        }
    }

    public int Health
    {
        get
        {
            return PlayerState.Health;
        }
    }

    public int MaxHealth
    {
        get
        {
            return PlayerState.MaxHealth;
        }
    }

    public int Concentration
    {
        get
        {
            return PlayerState.Concentration;
        }
    }

    public int MaxConcentration
    {
        get
        {
            return PlayerState.MaxConcentration;
        }
    }

    public int Confidence
    {
        get
        {
            return PlayerState.Confidence;
        }
    }

    public int MaxConfidence
    {
        get
        {
            return PlayerState.MaxConfidence;
        }
    }

    public int Coins
    {
        get
        {
            return PlayerState.Coins;
        }
    }

    public EncounterResult EncounterResult { get; private set; }

    public CurrentViews CurrentScreen = CurrentViews.CharacterScreen;
    public Location CurrentLocation
    {
        get
        {
            return GameState.WorldState.CurrentLocation;
        }
    }

    public LocationSpot CurrentSpot
    {
        get
        {
            return GameState.WorldState.CurrentLocationSpot;
        }
    }

    public TimeWindow CurrentTime
    {
        get
        {
            return GameState.WorldState.TimeWindow;
        }
    }

    public int CurrentHour
    {
        get
        {
            return GameState.WorldState.CurrentTimeHours;
        }
    }

    public List<Location> Locations
    {
        get
        {
            return GameManager.GetPlayerKnownLocations();
        }
    }

    public EncounterManager EncounterManager = null;
    public ActionImplementation ActionImplementation = null;

    private int StateVersion = 0;


    [Inject] private ContentValidator ContentValidator { get; set; }

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

    protected override async Task OnInitializedAsync()
    {
        ContentValidationResult validationResult = ContentValidator.ValidateContent();
        bool missingReferences = validationResult.HasMissingReferences;

        if (missingReferences)
        {
            CurrentScreen = CurrentViews.MissingReferences;
        }
        else if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }
    }

    private async Task ResolvedMissingReferences()
    {
        if (!PlayerState.IsInitialized)
        {
            await InitializeGame();
        }

        ChangeState();
    }

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

    private async Task InitializeGame()
    {
        CurrentScreen = CurrentViews.CharacterScreen;
        ChangeState();
    }

    private async Task HandleCharacterCreated(PlayerState playerState)
    {
        CurrentScreen = CurrentViews.LocationScreen;
        await GameManager.StartGame();
    }



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
        return CurrentLocation ?? new Location("default");
    }

    private async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.Name);
        ChangeState();
    }

    private async Task SaveGame()
    {
        await GameManager.SaveGame();
    }

    private async Task HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return;

        ActionImplementation = await GameManager.ExecuteAction(action);

        EncounterManager = GameState.ActionStateTracker.GetCurrentEncounter();
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

        CurrentScreen = CurrentViews.LocationScreen;
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
        CurrentScreen = CurrentViews.LocationScreen;
        ChangeState();
    }

    private async Task WaitOneHour()
    {
        // Create a "Wait" action that advances time without other effects
        ActionImplementation waitAction = GameManager.GetWaitAction();

        UserActionOption waitOption = new UserActionOption(
            "Wait for one hour", false, waitAction,
            GameState.WorldState.CurrentLocation?.Id ?? "Global",
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


    private void DisplayActionMessages()
    {
        actionMessages = GetResultMessages();

        if (actionMessages.Any())
        {
            showActionMessage = true;
            actionMessageType = "success";  // Default to success

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

    private string GetIconForTimeWindow(TimeWindow time)
    {
        return time switch
        {
            TimeWindow.Night => "🌙",
            TimeWindow.Morning => "🌄",
            TimeWindow.Afternoon => "☀️",
            TimeWindow.Evening => "🌆",
            _ => "❓"
        };
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
            TimeWindow.Morning => "time-morning",
            TimeWindow.Afternoon => "time-afternoon",
            TimeWindow.Evening => "time-evening",
            TimeWindow.Night => "time-night",
            _ => ""
        };
    }
}

public enum SidebarSections
{
    skills, resources, inventory, status
}