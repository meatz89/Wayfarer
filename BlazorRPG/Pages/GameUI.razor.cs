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
    public int concentrationCurrent => GameState.Player.Concentration;
    public int concentrationMax => GameState.Player.MaxConcentration;
    public int repuationCurrent => GameState.Player.Reputation;
    public int reputationMax => GameState.Player.MaxReputation;
    public int health => GameState.Player.Health;
    public int maxHealth => GameState.Player.MaxHealth;
    public int concentration => GameState.Player.Concentration;
    public int maxConcentration => GameState.Player.MaxConcentration;
    public int reputation => GameState.Player.Reputation;
    public int maxReputation => GameState.Player.MaxReputation;
    public int coins => GameState.Player.Coins;
    public int food => GameState.Player.Inventory.GetItemCount(ResourceTypes.Food);

    public List<Location> Locations => GameManager.GetPlayerKnownLocations();

    private bool showNarrative = false;
    private LocationNames selectedLocation;
    public PlayerState Player => GameState.Player;
    public Location CurrentLocation => GameState.World.CurrentLocation;
    public LocationSpot CurrentSpot => GameState.World.CurrentLocationSpot;
    public Encounter CurrentEncounter => GameState.Actions.CurrentEncounter;
    public EncounterResult EncounterResult => GameState.Actions.EncounterResult;
    public TimeWindows CurrentTime => GameState.World.WorldTime;
    public int CurrentHour => GameState.World.CurrentTimeInHours;
    public bool ShowEncounterResult { get; set; } = false;

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

    private void OnNarrativeCompleted()
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
        if (result.EncounterResults != EncounterResults.Ongoing)
        {
            ShowEncounterResult = true;
        }
        StateHasChanged();
    }

    private void FinishEncounter()
    {
        // Reset Encounter logic
        GameManager.FinishEncounter(EncounterResult.Encounter);
        ShowEncounterResult = false;

        ActionResult result = GameManager.TravelToLocation(CurrentLocation.LocationName);
        StateHasChanged();
    }

    public bool CurrentEncounterOngoing()
    {
        if (CurrentEncounter == null) return false;
        if (EncounterResult == null) return false;
        if (EncounterResult.EncounterResults == EncounterResults.Ongoing) { return true; }
        return false;
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
        else
        {
            // Execute the action immediately
            ActionResult result = GameManager.ExecuteBasicAction(action);
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
        WorldState world = GameState.World;

        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        properties.Add(new(
                GetIconForLocationArchetype(location.LocationArchetype),
                FormatEnumString(location.LocationArchetype.ToString()),
                ""
            ));

        properties.Add(new(
                GetIconForCrowdDensity(location.CrowdDensity),
                FormatEnumString(location.CrowdDensity.ToString()),
                ""
            ));

        properties.Add(new(
                GetIconForOpportunity(location.Opportunity),
                FormatEnumString(location.Opportunity.ToString()),
                ""
            ));

        properties.Add(new(
                GetIconForTimeWindow(world.WorldTime),
                FormatEnumString(world.WorldTime.ToString()),
                ""
            ));

        properties.Add(new(
                GetIconForWeatherType(world.WorldWeather),
                FormatEnumString(world.WorldWeather.ToString()),
                ""
            ));

        return properties;
    }

    // Helper methods to get icons for different property types
    private string GetIconForLocationArchetype(LocationArchetypes type)
    {
        return type switch
        {
            LocationArchetypes.Tavern => "🍺",
            LocationArchetypes.Market => "🛒",
            LocationArchetypes.Forest => "🌲",
            LocationArchetypes.Road => "🛣️",
            LocationArchetypes.Field => "🌾",
            LocationArchetypes.Dock => "⚓",
            LocationArchetypes.Warehouse => "🏭",
            LocationArchetypes.Factory => "🏭",
            LocationArchetypes.Workshop => "🔨",
            LocationArchetypes.Shop => "🛍️",
            LocationArchetypes.Garden => "🌷",
            LocationArchetypes.Library => "📚",
            LocationArchetypes.ConstructionSite => "🚧",
            LocationArchetypes.Docks => "🚢",
            LocationArchetypes.CraftsmanWorkshop => "🛠️",
            LocationArchetypes.Crossroads => "🔀",
            _ => "❓"
        };
    }

    private string GetIconForCrowdDensity(CrowdDensity density)
    {
        return density switch
        {
            CrowdDensity.Deserted => "😶",
            CrowdDensity.Quiet => "🚶",
            CrowdDensity.Bustling => "👥",
            _ => "❓"
        };
    }

    private string GetIconForOpportunity(OpportunityTypes scale)
    {
        return scale switch
        {
            OpportunityTypes.Charitable => "🏠",
            OpportunityTypes.Commercial => "🏘️",
            OpportunityTypes.Healthcare => "🏙️",
            _ => "❓"
        };
    }

    private string GetIconForTimeWindow(TimeWindows time)
    {
        return time switch
        {
            TimeWindows.Midnight => "🌙",
            TimeWindows.Dawn => "🌄",
            TimeWindows.Noon => "☀️",
            TimeWindows.Dusk => "🌆",
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
}
