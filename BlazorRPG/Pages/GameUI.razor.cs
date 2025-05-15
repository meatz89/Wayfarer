using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private MessageSystem MessageSystem { get; set; }
    [Inject] private LoadingStateService LoadingStateService { get; set; }

    private Timer _pollingTimer;
    private bool _previousLoadingState;
    private string _previousMessage = string.Empty;
    private int _previousProgress;


    public bool hasApLeft = false;
    public bool hasNoApLeft
    {
        get
        {
            return !hasApLeft;
        }
    }

    public PlayerState PlayerState
    {
        get
        {
            return GameState.PlayerState;
        }
    }

    // Player Resources
    public int Energy
    {
        get
        {
            return PlayerState.CurrentEnergy();
        }
    }

    public int Concentration
    {
        get
        {
            return PlayerState.Concentration;
        }
    }

    public int ActionPoints
    {
        get
        {
            return PlayerState.CurrentActionPoints();
        }
    }

    public int TurnActionPoints
    {
        get
        {
            return PlayerState.MaxActionPoints;
        }
    }


    public int Exhaustion;
    public int Hunger;
    public int MentalLoad;
    public int Isolation;

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

    public TimeWindowTypes CurrentTime
    {
        get
        {
            return GameState.WorldState.CurrentTimeWindow;
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

        _pollingTimer = new Timer(CheckLoadingState, null, 0, 100);
    }

    private void CheckLoadingState(object state)
    {
        // Check if loading state has changed
        bool hasChanged = _previousLoadingState != LoadingStateService.IsLoading ||
            _previousMessage != LoadingStateService.Message ||
            _previousProgress != LoadingStateService.Progress;

        if (hasChanged)
        {
            _previousLoadingState = LoadingStateService.IsLoading;
            _previousMessage = LoadingStateService.Message;
            _previousProgress = LoadingStateService.Progress;

            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        _pollingTimer?.Dispose();
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

    public async Task SwitchAreaMap()
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
        return CurrentLocation;
    }

    private async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.Id);
        ChangeState();
    }

    private async Task StartNewDay()
    {
        await GameManager.StartNewDay();
        ChangeState();
    }

    private async Task HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return;

        ActionImplementation = await GameManager.ExecuteAction(action);

        EncounterManager = GameState.ActionStateTracker.GetCurrentEncounter();
        if (EncounterManager != null)
        {
            CurrentScreen = CurrentViews.EncounterScreen;
        }

        ChangeState();
    }

    private async Task HandleTravelStart(string travelLocationName)
    {
        selectedLocation = travelLocationName;

        if (travelLocationName == CurrentLocation.Id)
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

    private async Task WaitAction()
    {
        // Create a "Wait" action that advances time without other effects
        ActionImplementation waitAction = GameManager.GetWaitAction(CurrentSpot.Id);

        UserActionOption waitOption = new UserActionOption(
            "Wait for one hour", false, waitAction,
            GameState.WorldState.CurrentLocation?.Id ?? "Global",
            GameState.WorldState.CurrentLocationSpot?.Id ?? "Global",
            null, 0, null, CardTypes.Physical);

        await GameManager.ExecuteAction(waitOption);

        ChangeState();
    }

    public void ChangeState()
    {
        hasApLeft = PlayerState.CurrentActionPoints() > 0;
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

    private string GetIconForTimeWindow(TimeWindowTypes time)
    {
        return time switch
        {
            TimeWindowTypes.Night => "🌙",
            TimeWindowTypes.Morning => "🌄",
            TimeWindowTypes.Afternoon => "☀️",
            TimeWindowTypes.Evening => "🌆",
            _ => "❓"
        };
    }

    public string GetArchetypePortrait()
    {
        string portraitPath = $"/images/characters/{PlayerState.Gender.ToString().ToLower()}_{PlayerState.Archetype.ToString().ToLower()}.png";
        return portraitPath;
    }

    private string GetArchetypeIcon(Professions archetype)
    {
        return archetype switch
        {
            Professions.Warrior => "⚔️",
            Professions.Scholar => "📚",
            Professions.Mystic => "🏹",
            Professions.Diplomat => "🎵",
            Professions.Ranger => "🗝️",
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
            TimeWindowTypes.Morning => "time-morning",
            TimeWindowTypes.Afternoon => "time-afternoon",
            TimeWindowTypes.Evening => "time-evening",
            TimeWindowTypes.Night => "time-night",
            _ => ""
        };
    }
}

public enum SidebarSections
{
    skills, resources, inventory, status
}