public class EncounterChoiceSlot
{
    public string slotName;
    private BasicActionTypes actionType;
    private LocationPropertyCondition locationProperty;
    private LocationSpotPropertyCondition locationSpotProperty;
    private WorldStatePropertyCondition worldStateProperty;
    private PlayerStatusPropertyCondition playerStatusProperty;

    private EncounterStateCondition encounterStateProperty;
    private List<EncounterChoiceTemplate> encounterChoiceTemplates;

    public ChoiceSlotPersistence ChoiceSlotPersistence { get; }

    public EncounterChoiceSlot(
        string name,
        BasicActionTypes actionType,
        LocationPropertyCondition locationProperty,
        LocationSpotPropertyCondition locationSpotProperty,
        WorldStatePropertyCondition worldStateProperty,
        PlayerStatusPropertyCondition playerStatusProperty,
        EncounterStateCondition encounterStateProperty,
        List<EncounterChoiceTemplate> encounterChoiceTemplates,
        ChoiceSlotPersistence choiceSlotPersistence
        )
    {
        this.slotName = name;
        this.actionType = actionType;
        this.locationProperty = locationProperty;
        this.locationSpotProperty = locationSpotProperty;
        this.worldStateProperty = worldStateProperty;
        this.playerStatusProperty = playerStatusProperty;
        this.encounterStateProperty = encounterStateProperty;
        this.encounterChoiceTemplates = encounterChoiceTemplates;
        ChoiceSlotPersistence = choiceSlotPersistence;
    }

    public bool IsValidFor(EncounterContext context)
    {
        if (context.ActionType != actionType) return false;
        if (!MeetsLocationPropertyConditions(context.Location)) return false;
        if (!MeetsLocationSpotPropertyConditions(context.LocationSpot)) return false;
        if (!MeetsWorldStateConditions(context.GameState.World)) return false;
        if (!MeetsPlayerStateConditions(context.GameState.Player)) return false;
        return true;
    }

    private bool MeetsLocationPropertyConditions(Location location)
    {
        bool isValid = locationProperty == null || locationProperty.IsMet(location);
        return isValid;
    }

    private bool MeetsLocationSpotPropertyConditions(LocationSpot locationSpot)
    {
        bool isValid = locationSpotProperty == null || locationSpotProperty.IsMet(locationSpot);
        return isValid;
    }

    private bool MeetsWorldStateConditions(WorldState worldState)
    {
        bool isValid = worldStateProperty == null || worldStateProperty.IsMet(worldState);
        return isValid;
    }

    private bool MeetsPlayerStateConditions(PlayerState playerState)
    {
        bool isValid = playerStatusProperty == null || playerStatusProperty.IsMet(playerState);
        return isValid;
    }

    public bool MeetsEncounterStateConditions(EncounterValues encounterValues)
    {
        bool isValid = encounterStateProperty == null || encounterStateProperty.IsMet(encounterValues);
        return isValid;
    }

    public List<EncounterChoiceTemplate> GetChoiceTemplates()
    {
        return encounterChoiceTemplates;
    }
}
