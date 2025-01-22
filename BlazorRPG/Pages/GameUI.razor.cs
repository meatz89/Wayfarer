using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages; 

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    public List<string> ResultMessages => GetResultMessages();

    public int physicalEnergyCurrent => GameState.Player.PhysicalEnergy;
    public int physicalEnergyMax => GameState.Player.MaxPhysicalEnergy;
    public int focusEnergyCurrent => GameState.Player.FocusEnergy;
    public int focusEnergyMax => GameState.Player.MaxFocusEnergy;
    public int socialEnergyCurrent => GameState.Player.SocialEnergy;
    public int socialEnergyMax => GameState.Player.MaxSocialEnergy;
    public int health => GameState.Player.Health;
    public int maxHealth => GameState.Player.MaxHealth;
    public int concentration => GameState.Player.Concentration;
    public int maxConcentration => GameState.Player.MaxConcentration;
    public int reputation => GameState.Player.Reputation;
    public int maxReputation => GameState.Player.MaxReputation;
    public int coins => GameState.Player.Coins;
    public int food => GameState.Player.Inventory.GetItemCount(ResourceTypes.Food);

    public List<Location> Locations => GameManager.GetAllLocations();

    private bool showNarrative = false;
    private LocationNames selectedLocation;
    public PlayerState Player => GameState.Player;
    public Location CurrentLocation => GameState.World.CurrentLocation;
    public LocationSpot CurrentSpot => GameState.World.CurrentLocationSpot;
    public Encounter CurrentEncounter => GameState.Actions.CurrentEncounter;
    public TimeSlots CurrentTime => GameState.World.CurrentTimeSlot;
    public int CurrentHour => GameState.World.CurrentTimeInHours;
    public bool ShowEncounterResult { get; set; } = false;
    public EncounterResult EncounterResult { get; set; }

    // Tooltip Logic
    public bool showAreaMap = true;
    public bool showTooltip = false;
    public UserActionOption hoveredAction = null;

    private double mouseX;
    private double mouseY;

    protected override void OnInitialized()
    {
        GameManager.StartGame();
    }

    private void HandleLocationSelection(LocationNames locationName)
    {
        selectedLocation = locationName;

        // Check if the location has a narrative
        string narrative = GameManager.GetLocationNarrative(locationName);
        if (narrative != string.Empty)
        {
            showNarrative = true;
            showAreaMap = true;
        }
        else
        {
            // If no narrative, proceed as before
            showNarrative = false;
            FinalizeLocationSelection(locationName);
        }
    }

    private void OnNarrativeContinue()
    {
        showNarrative = false;
        FinalizeLocationSelection(selectedLocation);
    }

    private void FinalizeLocationSelection(LocationNames locationName)
    {
        List<UserLocationTravelOption> currentTravelOptions = GameState.World.CurrentTravelOptions;

        bool enterLocation = locationName == GameState.World.CurrentLocation.LocationName;

        ActionResult result;

        if (enterLocation)
        {
            result = GameManager.TravelToLocation(locationName);
            GameManager.TravelToLocation(locationName);
        }
        else
        {
            UserLocationTravelOption location = currentTravelOptions.FirstOrDefault(x => x.Location == locationName);
            GameManager.TravelToLocation(location.Location);
            result = GameManager.TravelToLocation(location.Location);
        }

        if (result.IsSuccess)
        {
            CompleteActionExecution();
            showAreaMap = false;
        }
    }

    private void HandleEncounterCompleted(EncounterResult result)
    {
        EncounterResult = result;

        if (result.encounterResults != EncounterResults.Ongoing)
        {
            ShowEncounterResult = true;
        }

        // Force a re-render of the GameUI component
        StateHasChanged();
    }

    private void ContinueAfterEncounterResult()
    {
        // Reset encounter logic
        ShowEncounterResult = false;
        EncounterResult = null;
    }

    public bool HasEncounter()
    {
        return CurrentEncounter != null;
    }

    private void HandleEncounterCompleted()
    {
        // Force a re-render of the GameUI component
        StateHasChanged();
    }

    public string GetModifierDescription(IGameStateModifier modifier)
    {
        if (modifier is FoodModfier modfier)
        {
            return $"Need additional Food: {modfier.AdditionalFood}";
        }

        return string.Empty;
    }

    public List<string> GetResultMessages()
    {
        ActionResultMessages messages = GameState.Actions.LastActionResultMessages;

        List<string> list = new();
        if (messages == null) return list;

        // Show outcomes with their previews
        foreach (Outcome outcome in messages.Outcomes)
        {
            string description = outcome.GetDescription();
            string preview = outcome.GetPreview(Player);
            list.Add($"{description}");
        }

        foreach (SystemMessage sysMsg in messages.SystemMessages)
        {
            // Add CSS class based on message type
            string cssClass = sysMsg.Type switch
            {
                SystemMessageTypes.Warning => "warning",
                SystemMessageTypes.Danger => "danger",
                SystemMessageTypes.Success => "success",
                _ => "info"
            };

            list.Add($"{sysMsg.Message}");
            //list.Add($"<span class='{cssClass}'>{sysMsg.Message}</span>");
        }

        return list;
    }

    private void HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return; // Prevent action if disabled
        //else if (action.BasicAction.ActionType == BasicActionTypes.Wait)
        //{
        //    GameManager.AdvanceTime();
        //    CompleteActionExecution();
        //}
        else
        {
            // Execute the action immediately
            ActionResult result = GameManager.ExecuteBasicAction(action, action.ActionImplementation);
            if (result.IsSuccess)
            {
                CompleteActionExecution();
            }
        }
    }

    public List<Quest> GetActiveQuests()
    {
        return GameState.Actions.ActiveQuests;
    }

    private void HandleSpotSelection(LocationSpot locationSpot)
    {
        List<UserLocationSpotOption> userLocationSpotOptions = GameState.World.CurrentLocationSpotOptions;
        UserLocationSpotOption userLocationSpot = userLocationSpotOptions.FirstOrDefault(x => x.LocationSpot == locationSpot.Name);

        GameManager.MoveToLocationSpot(userLocationSpot.Location, locationSpot.Name);
    }

    private void CompleteActionExecution()
    {
        GameManager.UpdateState();
    }

    private List<PropertyDisplay> GetLocationProperties(Location location)
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        properties.Add(new(
                "",
                FormatEnumString(location.LocationArchetype.ToString()),
                ""
            ));

        properties.Add(new(
                "",
                FormatEnumString(location.CrowdDensity.ToString()),
                ""
            ));

        properties.Add(new(
                "",
                FormatEnumString(location.LocationScale.ToString()),
                ""
            ));

        return properties;
    }

    private string FormatEnumString(string value)
    {
        return string.Concat(value
            .Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()))
            .Replace("Type", "")
            .Replace("Types", "");
    }
}

public record struct PropertyDisplay(
string Icon,
string Text,
string CssClass);
