using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;

namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; }

    public List<string> ResultMessages => GetResultMessages();

    public int physicalEnergyCurrent => GameState.PlayerState.Energy;
    public int physicalEnergyMax => GameState.PlayerState.MaxEnergy;

    public int health => GameState.PlayerState.Health;
    public int maxHealth => GameState.PlayerState.MaxHealth;

    public int concentration => GameState.PlayerState.Concentration;
    public int maxConcentration => GameState.PlayerState.MaxConcentration;

    public int confidence => GameState.PlayerState.Confidence;
    public int maxConfidence => GameState.PlayerState.MaxConfidence;

    public int coins => GameState.PlayerState.Coins;
    public int food => GameState.PlayerState.Inventory.GetItemCount(ItemTypes.Food);

    public List<Location> Locations => GameManager.GetPlayerKnownLocations();

    private bool showNarrative = false;
    private string selectedLocation;
    public PlayerState Player => GameState.PlayerState;

    public LocationSpot CurrentSpot => GameState.WorldState.CurrentLocationSpot;
    public TimeWindows CurrentTime => GameState.WorldState.WorldTime;
    public int CurrentHour => GameState.WorldState.CurrentTimeInHours;
    public bool ShowEncounterResult { get; set; } = false;
    public bool OngoingEncounter = false;

    public EncounterResult EncounterResult => GameState.Actions.EncounterResult;

    // Tooltip Logic
    public bool showAreaMap = true;
    public bool showTooltip = false;
    public UserActionOption hoveredAction;

    private double mouseX;
    private double mouseY;

    protected override async Task OnInitializedAsync()
    {
        await GameManager.StartGame();
        await GameManager.InitializeLocationSystem();
    }

    public Location GetCurrentLocation()
    {
        Location loc = GameState.WorldState.CurrentLocation;
        if (loc != null)
        {
            return loc;
        }
        return new Location() { Name = "Default" };
    }

    public EncounterManager GetCurrentEncounter()
    {
        return GameManager.GetEncounter();
    }

    private async Task HandleActionSelection(UserActionOption action)
    {
        if (action.IsDisabled) return; // Prevent action if disabled
        else
        {
            // Execute the action immediately
            OngoingEncounter = await GameManager.ExecuteBasicAction(action);
            if (!OngoingEncounter)
            {
                CompleteActionExecution();
            }
        }
    }

    private void HandleEncounterCompleted(EncounterResult result)
    {
        if (result.EncounterResults != EncounterResults.Ongoing)
        {
            OngoingEncounter = false;
            ShowEncounterResult = true;
        }
        StateHasChanged();
    }

    private void HandleLocationSelection(string locationName)
    {
        selectedLocation = locationName;

        // If no narrative, proceed as before
        showNarrative = false;
        FinalizeLocationSelection(locationName);
    }

    private void OnNarrativeCompleted()
    {
        showNarrative = false;
        FinalizeLocationSelection(selectedLocation);

        FinishEncounter();
    }

    private async Task FinalizeLocationSelection(string locationName)
    {
        List<UserLocationTravelOption> currentTravelOptions = GameState.WorldState.CurrentTravelOptions;

        bool enterLocation = locationName == GameState.WorldState.CurrentLocation.Name;
        ActionResult result;

        if (enterLocation)
        {
            showAreaMap = false;
        }
        else
        {
            UserLocationTravelOption location = currentTravelOptions.FirstOrDefault(x => x.Location == locationName);
            await GameManager.TravelToLocation(location.Location);
        }
    }

    private async Task FinishEncounter()
    {
        // Reset Encounter logic
        GameManager.FinishEncounter(EncounterResult.Encounter);
        ShowEncounterResult = false;

        await GameManager.TravelToLocation(GetCurrentLocation().Name);
        StateHasChanged();
    }

    public bool CurrentEncounterOngoing()
    {
        if (GetCurrentEncounter == null) return false;
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

    public List<Quest> GetActiveQuests()
    {
        return GameState.Actions.ActiveQuests;
    }

    private void HandleSpotSelection(LocationSpot locationSpot)
    {
        List<UserLocationSpotOption> userLocationSpotOptions = GameState.WorldState.CurrentLocationSpotOptions;
        UserLocationSpotOption userLocationSpot = userLocationSpotOptions.FirstOrDefault(x => x.LocationSpot == locationSpot.Name);

        GameManager.MoveToLocationSpot(userLocationSpot.Location, locationSpot.Name);
    }

    private void CompleteActionExecution()
    {
        GameManager.UpdateState();
    }

    private List<PropertyDisplay> GetLocationProperties(Location location)
    {
        WorldState world = GameState.WorldState;

        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        properties.Add(new PropertyDisplay(
                GetIconForTimeWindow(world.WorldTime),
                FormatEnumString(world.WorldTime.ToString()),
                "",
                "",
                ""
            ));

        properties.Add(new PropertyDisplay(
                GetIconForWeatherType(world.WorldWeather),
                FormatEnumString(world.WorldWeather.ToString()),
                "",
                "",
                ""
            ));

        return properties;
    }

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
}
