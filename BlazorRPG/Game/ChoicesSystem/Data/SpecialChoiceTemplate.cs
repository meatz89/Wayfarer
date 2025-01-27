public class SpecialChoiceTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<LocationPropertyCondition> LocationPropertyConditions { get; } = new();
    public List<LocationSpotPropertyCondition> LocationSpotPropertyConditions { get; } = new();
    public List<WorldStatePropertyCondition> WorldStatePropertyConditions { get; } = new();
    public List<PlayerStatusPropertyCondition> PlayerStatusPropertyConditions { get; } = new();
    public List<EncounterStateCondition> StateConditions { get; set; } = new();

    public SpecialChoiceTemplate(
        string name,
        BasicActionTypes actionType,
        LocationPropertyCondition locationPropertyConditions,
        LocationSpotPropertyCondition locationSpotPropertyConditions,
        WorldStatePropertyCondition worldStatePropertyConditions,
        PlayerStatusPropertyCondition playerStatusPropertyConditions,
        EncounterStateCondition encounterStateConditions)
    {
        Name = name;
        ActionType = actionType;
        LocationPropertyConditions.Add(locationPropertyConditions);
        LocationSpotPropertyConditions.Add(locationSpotPropertyConditions);
        WorldStatePropertyConditions.Add(worldStatePropertyConditions);
        PlayerStatusPropertyConditions.Add(playerStatusPropertyConditions);
        StateConditions.Add(encounterStateConditions);
    }
}