using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorRPG.Pages;

public partial class MainGameplayView : ComponentBase
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    [Inject] private GameWorld GameWorld { get; set; }
    [Inject] private GameWorldManager GameWorldManager { get; set; }
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

    public Player PlayerState
    {
        get
        {
            return GameWorld.Player;
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
            return GameWorld.WorldState.CurrentLocation;
        }
    }

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
            return GameWorldManager.GetPlayerKnownLocations();
        }
    }

    public EncounterManager EncounterManager = null;
    public LocationAction locationAction = null;

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
        await GameWorldManager.MoveToLocationSpot(locationSpot.SpotID);
        UpdateState();
    }

    private async Task StartNewDay()
    {
        await GameWorldManager.StartNewDay();
        UpdateState();
    }

    private async Task HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return;

        await GameWorldManager.ExecuteAction(action);

        EncounterManager = GameWorld.ActionStateTracker.CurrentEncounterManager ;
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

        await GameWorldManager.Travel(travelLocationName);

        CurrentScreen = CurrentViews.LocationScreen;
        UpdateState();
    }

    private async Task OnEncounterCompleted(EncounterResult result)
    {
        LocationAction locationAction = result.locationAction;
        await GameWorldManager.ProcessActionCompletion(locationAction);

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

    private async Task HandleCardRefreshed(SkillCard card)
    {
        await GameWorldManager.RefreshCard(card);
        MessageSystem.AddSystemMessage($"Refreshed {card.Name} card");
        UpdateState();
    }

    private async Task WaitAction()
    {
        // Create a "Wait" action that advances time without other effects
        LocationAction waitAction = GameWorldManager.GetWaitAction(CurrentSpot.SpotID);

        UserActionOption waitOption = new UserActionOption(
            "Wait for one hour", false, waitAction,
            GameWorld.WorldState.CurrentLocation?.Id ?? "Global",
            GameWorld.WorldState.CurrentLocationSpot?.SpotID ?? "Global",
            null, 0, null, null);

        await GameWorldManager.ExecuteAction(waitOption);

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
