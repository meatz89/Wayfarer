using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorRPG.Pages;

public partial class MainGameplayView : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private GameWorld GameWorld { get; set; }
    [Inject] private GameWorldManager GameManager { get; set; }
    [Inject] private MessageSystem MessageSystem { get; set; }
    [Inject] private LoadingStateService? LoadingStateService { get; set; }
    [Inject] private CardHighlightService CardRefreshService { get; set; }


    private int StateVersion = 0;
    public EncounterManager EncounterManager = null;
    public EncounterResult EncounterResult;

    // Navigation State
    public string SelectedLocation { get; private set; }
    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public int Energy { get; private set; } = 0;
    public int Concentration { get; private set; } = 0;
    public Location CurrentLocation { get; private set; }
    public Player PlayerState => GameWorld.GetPlayer();

    // Tooltip State
    public bool ShowTooltip = false;
    public UserActionOption HoveredAction;
    private double MouseX;
    private double MouseY;

    // Action Message State
    private bool ShowActionMessage = false;
    private string ActionMessageType = "success";
    private List<string> ActionMessages = new List<string>();
    public ElementReference SidebarRef;


    public bool HasApLeft { get; private set; }
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
    
    public BeatOutcome BeatOutcome { get; private set; }
    public CurrentViews CurrentScreen { get; private set; } = CurrentViews.LocationScreen;

    public LocationSpot CurrentSpot
    {
        get
        {
            return GameWorld.WorldState.CurrentLocationSpot;
        }
    }
    public TimeWindowTypes CurrentTime
    {
        get
        {
            return GameWorld.WorldState.CurrentTimeWindow;
        }
    }

    public int CurrentHour
    {
        get
        {
            return GameWorld.WorldState.CurrentTimeHours;
        }
    }

    public List<Location> Locations
    {
        get
        {
            return GameManager.GetPlayerKnownLocations();
        }
    }

    private Dictionary<SidebarSections, bool> ExpandedSections = new Dictionary<SidebarSections, bool>
    {
        { SidebarSections.skills, false },
        { SidebarSections.resources, false },
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
                await Task.Delay(100);
            }
        });
    }

    private void PollGameState()
    {
        GameWorldSnapshot snapshot = GameManager.GetGameSnapshot();

        // Update UI state from snapshot, not directly from backend events
        CurrentTimeOfDay = snapshot.CurrentTimeOfDay;
        Energy = snapshot.Energy;
        Concentration = snapshot.Concentration;

        // Update location properties based on time
        if (CurrentScreen == CurrentViews.LocationScreen)
        {
            // Refresh available actions based on time-of-day
            UpdateAvailableActions();
        }
    }

    private void UpdateAvailableActions()
    {
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

        StateHasChanged();
    }

    public Location GetCurrentLocation()
    {
        return GameWorld.CurrentLocation;
    }

    private async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.SpotID);
        UpdateState();
    }

    private async Task StartNewDay()
    {
        await GameManager.StartNewDay();
        UpdateState();
    }

    private async Task HandleActionSelection(UserActionOption action)
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

    private async Task OnEncounterCompleted(BeatOutcome result)
    {
        // Process action completion
        await GameManager.ProcessActionCompletion();

        // Store the result for narrative view
        BeatOutcome = result;

        // Switch to narrative screen to show result
        CurrentScreen = CurrentViews.NarrativeScreen;

        UpdateState();
    }

    private async Task HandleTravelStart(string travelLocationName)
    {
        await GameManager.Travel(travelLocationName);

        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    private async Task OnNarrativeCompleted()
    {
        BeatOutcome = null;

        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    private async Task UseResource(ActionNames actionName)
    {
        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    private async Task HandleCardRefreshed(SkillCard card)
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

    private void DisplayActionMessages()
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

    private void DismissActionMessage()
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

    private string FormatItemName(ItemTypes itemType)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            itemType.ToString(),
            "([A-Z])",
            " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    private async Task ToggleSection(SidebarSections sectionName)
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
}
