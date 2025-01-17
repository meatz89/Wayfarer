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
    public TimeSlots CurrentTime => GameState.World.CurrentTimeSlot;
    public int CurrentHour => GameState.World.CurrentTimeInHours;

    public bool ShowEncounterResult { get; set; } = false;
    public EncounterResults EncounterResult { get; set; }

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
        if (GameManager.HasLocationNarrative(locationName))
        {
            showNarrative = true;
        }
        else
        {
            // If no narrative, proceed as before
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


    private void HandleEncounterCompleted(EncounterResults result)
    {
        EncounterResult = result;

        if (result != EncounterResults.Ongoing)
        {
            ShowEncounterResult = true;
        }

        // Force a re-render of the GameUI component
        StateHasChanged();
    }

    private void ContinueAfterEncounterResult()
    {
        ShowEncounterResult = false;
    }

    public bool HasEncounter()
    {
        return GameState.Actions.CurrentEncounter != null;
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

    public List<LocationPropertyChoiceEffect> GetLocationEffects()
    {
        return GameManager.GetLocationEffects(CurrentLocation.LocationName);
    }

    private string GetPressureIcon(AtmosphereTypes? pressure) => pressure switch
    {
        AtmosphereTypes.Relaxed => "😌",
        AtmosphereTypes.Formal => "⚠️",
        AtmosphereTypes.Tense => "⚔️",
        _ => "❓"
    };

    private string GetResourceIcon(ResourceTypes? resource) => resource switch
    {
        ResourceTypes.Food => "🍖",
        ResourceTypes.Wood => "🪵",
        ResourceTypes.Fish => "🐟",
        ResourceTypes.Herbs => "🌿",
        ResourceTypes.Cloth => "🧵",
        _ => "📦"
    };

    private string GetCrowdIcon(ActivityLevelTypes? crowdLevel) => crowdLevel switch
    {
        ActivityLevelTypes.Deserted => "🕸️",
        ActivityLevelTypes.Quiet => "👤",
        ActivityLevelTypes.Bustling => "👥",
        _ => "❓"
    };

    private string FormatEnumString(string value)
    {
        return string.Concat(value
            .Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()))
            .Replace("Type", "")
            .Replace("Types", "");
    }

    private List<PropertyDisplay> GetLocationPropertiesWithEffects()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();
        LocationProperties loc = CurrentLocation.LocationProperties;

        if (loc.IsArchetypeSet)
        {
            List<string> archetypeEffects = GetEffectDescriptions(LocationPropertyTypes.Archetype);
            properties.Add(new(
                "🏠",
                FormatEnumString(CurrentLocation.LocationProperties.Archetype.ToString()),
                "",
                archetypeEffects
            ));
        }

        if (loc.IsResourceSet && loc.Resource != ResourceTypes.None)
        {
            List<string> resourceEffects = GetEffectDescriptions(LocationPropertyTypes.Resource);
            properties.Add(new(
                GetResourceIcon(loc.Resource),
                FormatEnumString(loc.Resource.ToString()),
                "",
                resourceEffects
            ));
        }

        if (loc.IsActivitySet)
        {
            List<string> crowdEffects = GetEffectDescriptions(LocationPropertyTypes.ActivityLevel);
            properties.Add(new(
                GetCrowdIcon(loc.ActivityLevel),
                FormatEnumString(loc.ActivityLevel.ToString()),
                "",
                crowdEffects
            ));
        }


        if (loc.IsAccessabilitySet)
        {
            List<string> scaleEffects = GetEffectDescriptions(LocationPropertyTypes.Accessibility);
            properties.Add(new(
                "📐",
                FormatEnumString(loc.Accessability.ToString()),
                "",
                scaleEffects
            ));
        }

        if (loc.IsSupervisionSet)
        {
            List<string> legalityEffects = GetEffectDescriptions(LocationPropertyTypes.Supervision);
            properties.Add(new(
                "⚖️",
                FormatEnumString(loc.Supervision.ToString()),
                $"property-{loc.Supervision.ToString().ToLower()}",
                legalityEffects
            ));
        }


        if (loc.IsAtmosphereSet)
        {
            List<string> pressureEffects = GetEffectDescriptions(LocationPropertyTypes.Atmosphere);
            properties.Add(new(
                GetPressureIcon(loc.Atmosphere),
                FormatEnumString(loc.Atmosphere.ToString()),
                $"property-{loc.Space.ToString().ToLower()}",
                pressureEffects
            ));
        }

        if (loc.IsSpaceSet)
        {
            List<string> complexityEffects = GetEffectDescriptions(LocationPropertyTypes.Space);
            properties.Add(new(
                "🧩",
                FormatEnumString(loc.Space.ToString()),
                "",
                complexityEffects
            ));
        }

        if (loc.IsExposureSet)
        {
            List<string> exposureEffects = GetEffectDescriptions(LocationPropertyTypes.Exposure);
            properties.Add(new(
                loc.Exposure == ExposureTypes.Indoor ? "🏗️" : "🌳",
                FormatEnumString(loc.Exposure.ToString()),
                "",
                exposureEffects
            ));
        }

        return properties;
    }

    private List<string> GetEffectDescriptions(LocationPropertyTypes propertyType)
    {
        List<string> effects = new();
        List<LocationPropertyChoiceEffect> locationEffects = GetLocationEffects();

        foreach (LocationPropertyChoiceEffect effect in locationEffects)
        {
            // Skip if property type doesn't match
            if (effect.LocationProperty.GetPropertyType() != propertyType)
                continue;

            string formattedEffect = FormatEffect(effect);
            effects.Add(formattedEffect);
        }

        return effects;
    }

    public string FormatEffect(LocationPropertyChoiceEffect effect)
    {
        return effect.Description;
    }
}

public record struct PropertyDisplay(
    string Icon,
    string Text,
    string CssClass,
    List<string> Effects);
