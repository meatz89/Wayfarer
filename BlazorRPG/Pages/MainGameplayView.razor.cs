using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorRPG.Pages;

public partial class MainGameplayView : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private MessageSystem MessageSystem { get; set; }
    [Inject] private LoadingStateService? LoadingStateService { get; set; }
    [Inject] private CardHighlightService CardRefreshService { get; set; }

    public bool HasApLeft { get; private set; }
    public bool HasNoApLeft
    {
        get
        {
            return !HasApLeft;
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

    public CurrentViews CurrentScreen { get; private set; } = CurrentViews.LocationScreen;
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

    // Navigation State
    public string SelectedLocation { get; private set; }

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

    private Dictionary<SidebarSections, bool> ExpandedSections = new Dictionary<SidebarSections, bool>
    {
        { SidebarSections.skills, false },
        { SidebarSections.resources, false },
        { SidebarSections.inventory, false },
        { SidebarSections.status, false }
    };

    protected override void OnInitialized()
    {
        HasApLeft = PlayerState.CurrentActionPoints() > 0;
        DisplayActionMessages();
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
        return CurrentLocation;
    }

    private async Task HandleSpotSelection(LocationSpot locationSpot)
    {
        await GameManager.MoveToLocationSpot(locationSpot.Id);
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

        ActionImplementation = await GameManager.ExecuteAction(action);

        EncounterManager = GameState.ActionStateTracker.GetCurrentEncounter();
        if (EncounterManager != null)
        {
            CurrentScreen = CurrentViews.EncounterScreen;
        }

        UpdateState();
    }

    private async Task HandleTravelStart(string travelLocationName)
    {
        SelectedLocation = travelLocationName;

        if (travelLocationName == CurrentLocation.Id)
        {
            UpdateState();
            return;
        }

        await GameManager.InitiateTravelToLocation(travelLocationName);

        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    private async Task OnEncounterCompleted(EncounterResult result)
    {
        ActionImplementation actionImplementation = result.ActionImplementation;
        await GameManager.ProcessActionCompletion(actionImplementation);

        EncounterResult = result;
        CurrentScreen = CurrentViews.NarrativeScreen;
        UpdateState();
    }

    private async Task OnNarrativeCompleted()
    {
        EncounterResult = null;

        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    private async Task UseResource(ActionNames actionName)
    {
        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    private async Task HandleCardRefreshed(CardDefinition card)
    {
        await GameManager.RefreshCard(card);
        MessageSystem.AddSystemMessage($"Refreshed {card.Name} card");
        UpdateState();
    }

    private async Task WaitAction()
    {
        // Create a "Wait" action that advances time without other effects
        ActionImplementation waitAction = GameManager.GetWaitAction(CurrentSpot.Id);

        UserActionOption waitOption = new UserActionOption(
            "Wait for one hour", false, waitAction,
            GameState.WorldState.CurrentLocation?.Id ?? "Global",
            GameState.WorldState.CurrentLocationSpot?.Id ?? "Global",
            null, 0, null, null);

        await GameManager.ExecuteAction(waitOption);

        UpdateState();
    }

    public void UpdateState()
    {
        HasApLeft = PlayerState.CurrentActionPoints() > 0;
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
