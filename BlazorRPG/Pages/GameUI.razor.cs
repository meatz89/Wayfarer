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
    public int coins => GameState.Player.Coins;
    public int food => GameState.Player.Inventory.GetItemCount(ResourceTypes.Food);

    public List<Location> Locations => GameManager.GetAllLocations();

    public PlayerState Player => GameState.Player;
    public Location CurrentLocation => GameState.World.CurrentLocation;
    public LocationSpot CurrentSpot => GameState.World.CurrentLocationSpot;
    public TimeSlots CurrentTime => GameState.World.CurrentTimeSlot;
    public int CurrentHour => GameState.World.CurrentTimeInHours;

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

    private void HandleLocationSelection(LocationNames locationName)
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

    private void CompleteActionExecution()
    {
        GameManager.UpdateState();
    }

    public List<LocationPropertyChoiceEffect> GetLocationEffects()
    {
        return GameManager.GetLocationEffects(CurrentLocation.LocationName);
    }

    private string GetPressureIcon(PressureStateTypes? pressure) => pressure switch
    {
        PressureStateTypes.Relaxed => "😌",
        PressureStateTypes.Alert => "⚠️",
        PressureStateTypes.Hostile => "⚔️",
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

    private string GetCrowdIcon(CrowdLevelTypes? crowdLevel) => crowdLevel switch
    {
        CrowdLevelTypes.Empty => "🕸️",
        CrowdLevelTypes.Sparse => "👤",
        CrowdLevelTypes.Busy => "👥",
        CrowdLevelTypes.Crowded => "👥👥",
        _ => "❓"
    };

    private string FormatLocationArchetype(LocationArchetypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

    private string FormatScale(ScaleVariationTypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

    private string FormatExposure(ExposureConditionTypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

    private string FormatLegality(LegalityTypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

    private string FormatPressure(PressureStateTypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

    private string FormatComplexity(ComplexityTypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

    private string FormatCrowdLevel(CrowdLevelTypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

    private string FormatResource(ResourceTypes? value)
    {
        if (value == null) return string.Empty;
        return FormatEnumString(value.ToString());
    }

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
                FormatLocationArchetype(CurrentLocation.Archetype),
                "",
                archetypeEffects
            ));
        }

        if (loc.IsScaleSet)
        {
            List<string> scaleEffects = GetEffectDescriptions(LocationPropertyTypes.Scale);
            properties.Add(new(
                "📐",
                FormatScale(loc.Scale),
                "",
                scaleEffects
            ));
        }

        if (loc.IsExposureSet)
        {
            List<string> exposureEffects = GetEffectDescriptions(LocationPropertyTypes.Exposure);
            properties.Add(new(
                loc.Exposure == ExposureConditionTypes.Indoor ? "🏗️" : "🌳",
                FormatExposure(loc.Exposure),
                "",
                exposureEffects
            ));
        }

        if (loc.IsLegalitySet)
        {
            List<string> legalityEffects = GetEffectDescriptions(LocationPropertyTypes.Legality);
            properties.Add(new(
                "⚖️",
                FormatLegality(loc.Legality),
                $"property-{loc.Legality.ToString().ToLower()}",
                legalityEffects
            ));
        }

        if (loc.IsPressureSet)
        {
            List<string> pressureEffects = GetEffectDescriptions(LocationPropertyTypes.Pressure);
            properties.Add(new(
                GetPressureIcon(loc.Pressure),
                FormatPressure(loc.Pressure),
                $"property-{loc.Pressure.ToString().ToLower()}",
                pressureEffects
            ));
        }

        if (loc.IsComplexitySet)
        {
            List<string> complexityEffects = GetEffectDescriptions(LocationPropertyTypes.Complexity);
            properties.Add(new(
                "🧩",
                FormatComplexity(loc.Complexity),
                "",
                complexityEffects
            ));
        }

        if (loc.IsResourceSet && loc.Resource != ResourceTypes.None)
        {
            List<string> resourceEffects = GetEffectDescriptions(LocationPropertyTypes.Resource);
            properties.Add(new(
                GetResourceIcon(loc.Resource),
                FormatResource(loc.Resource),
                "",
                resourceEffects
            ));
        }

        if (loc.IsCrowdLevelSet)
        {
            List<string> crowdEffects = GetEffectDescriptions(LocationPropertyTypes.CrowdLevel);
            properties.Add(new(
                GetCrowdIcon(loc.CrowdLevel),
                FormatCrowdLevel(loc.CrowdLevel),
                "",
                crowdEffects
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
        return effect.RuleDescription;
    }
}

public record struct PropertyDisplay(
    string Icon,
    string Text,
    string CssClass,
    List<string> Effects);
