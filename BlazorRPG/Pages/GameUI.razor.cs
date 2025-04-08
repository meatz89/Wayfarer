using Microsoft.AspNetCore.Components;
namespace BlazorRPG.Pages;

public partial class GameUI : ComponentBase
{
    [Inject] private GameState GameState { get; set; }
    [Inject] private GameManager GameManager { get; set; }

    public List<string> ResultMessages => GetResultMessages();

    public PlayerState PlayerState => GameState.PlayerState;

    public int energyCurrent => GameState.PlayerState.Energy;
    public int energyMax => GameState.PlayerState.MaxEnergy;

    public int health => GameState.PlayerState.Health;
    public int maxHealth => GameState.PlayerState.MaxHealth;

    public int concentration => GameState.PlayerState.Concentration;
    public int maxConcentration => GameState.PlayerState.MaxConcentration;

    public int confidence => GameState.PlayerState.Confidence;
    public int maxConfidence => GameState.PlayerState.MaxConfidence;

    public int coins => GameState.PlayerState.Coins;

    public List<Location> Locations => GameManager.GetPlayerKnownLocations();

    private bool showNarrative = false;
    private string selectedLocation;
    public PlayerState Player => GameState.PlayerState;

    public LocationSpot CurrentSpot => GameState.WorldState.CurrentLocationSpot;
    public TimeWindows CurrentTime => GameState.WorldState.WorldTime;
    public int CurrentHour => GameState.WorldState.CurrentTimeInHours;
    public bool ShowEncounterResult { get; set; } = false;

    public EncounterResult EncounterResult => GameState.Actions.EncounterResult;

    public bool OngoingEncounter { get; private set; }

    // Tooltip Logic
    public bool showAreaMap = true;
    public bool showTooltip = false;
    public UserActionOption hoveredAction;

    private double mouseX;
    private double mouseY;

    private bool needsCharacterCreation = false;

    protected override async Task OnInitializedAsync()
    {
        // Check if character has been created
        needsCharacterCreation = !GameState.PlayerState.IsInitialized;
        if (!needsCharacterCreation)
        {
            await InitializeGame();
        }
    }

    private async Task HandleCharacterCreated(PlayerState playerState)
    {
        needsCharacterCreation = false;
        await InitializeGame();
    }

    private async Task InitializeGame()
    {
        await GameManager.StartGame();

        // Ensure the user sees the location spot view initially, not the travel view
        showAreaMap = false;

        // Make sure UI refreshes
        StateHasChanged();
    }

    public void SwitchAreaMap()
    {
        showAreaMap = !showAreaMap;
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
            GameManager.ExecuteBasicAction(action);

            OngoingEncounter = GameState.Actions.IsActiveEncounter;
            if (!OngoingEncounter)
            {
                CompleteActionExecution();
            }
        }
    }

    private void OnEncounterCompleted(EncounterResult result)
    {
        if (result.EncounterResults != EncounterResults.Ongoing)
        {
            OngoingEncounter = false;
            ShowEncounterResult = true;
        }
        StateHasChanged();
    }

    // Modify HandleLocationSelection method
    private void HandleTravelStart(string travelLocationName)
    {
        selectedLocation = travelLocationName;

        // If current location, just switch to spot view
        string currentLocationName = GameState.WorldState.CurrentLocation.Name;
        if (travelLocationName == currentLocationName)
        {
            showAreaMap = false;
            StateHasChanged();
            return;
        }

        // Otherwise initiate travel
        GameManager.InitiateTravelToLocation(travelLocationName);

        OngoingEncounter = true;
        StateHasChanged();
    }

    private async Task OnNarrativeCompleted()
    {
        showNarrative = false;
        ShowEncounterResult = false;

        // Always return to location spot view after any encounter
        showAreaMap = false;

        await OnEncounterCompleted();
        StateHasChanged();
    }

    private async Task OnEncounterCompleted()
    {
        await GameManager.EndEncounter(EncounterResult.Encounter);
        ShowEncounterResult = false;

        StateHasChanged();
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

    private void HandleSpotSelection(LocationSpot locationSpot)
    {
        GameManager.MoveToLocationSpot(locationSpot.Name);
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
        // Convert enum names to readable text
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
}
