public class EncounterChoiceSlot
{
    public string slotName;
    public BasicActionTypes actionType;
    public LocationPropertyCondition locationProperty;
    public LocationSpotPropertyCondition locationSpotProperty;
    public WorldStatePropertyCondition worldStateProperty;
    public PlayerStatusPropertyCondition playerStatusProperty;

    public EncounterStateCondition encounterStateProperty;
    public EncounterChoiceTemplate encounterChoiceTemplate;

    public EncounterChoiceSlot(string name, BasicActionTypes actionType, LocationPropertyCondition locationProperty, LocationSpotPropertyCondition locationSpotProperty, WorldStatePropertyCondition worldStateProperty, PlayerStatusPropertyCondition playerStatusProperty, EncounterStateCondition encounterStateProperty, EncounterChoiceTemplate encounterChoiceTemplate)
    {
        this.slotName = name;
        this.actionType = actionType;
        this.locationProperty = locationProperty;
        this.locationSpotProperty = locationSpotProperty;
        this.worldStateProperty = worldStateProperty;
        this.playerStatusProperty = playerStatusProperty;
        this.encounterStateProperty = encounterStateProperty;
        this.encounterChoiceTemplate = encounterChoiceTemplate;
    }

    public bool IsValidFor(EncounterContext context)
    {
        if (context.ActionType != actionType) return false;
        if (!MeetsLocationPropertyConditions(context.CurrentValues)) return false;
        if (!MeetsLocationSpotPropertyConditions(context.CurrentValues)) return false;
        if (!MeetsWorldStateConditions(context.CurrentValues)) return false;
        if (!MeetsPlayerStateConditions(context.CurrentValues)) return false;
        return true;
    }

    private bool MeetsLocationPropertyConditions(EncounterValues currentValues)
    {
        throw new NotImplementedException();
    }

    private bool MeetsLocationSpotPropertyConditions(EncounterValues currentValues)
    {
        throw new NotImplementedException();
    }

    private bool MeetsWorldStateConditions(EncounterValues currentValues)
    {
        throw new NotImplementedException();
    }

    private bool MeetsPlayerStateConditions(EncounterValues currentValues)
    {
        throw new NotImplementedException();
    }

    private bool MeetsEncounterStateConditions(EncounterValues currentValues)
    {
        throw new NotImplementedException();
    }
}
