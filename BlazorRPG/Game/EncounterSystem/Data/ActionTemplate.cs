public class ActionTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<LocationPropertyCondition> LocationPropertyConditions { get; } = new();
    public List<LocationSpotPropertyCondition> LocationSpotPropertyConditions { get; } = new();
    public List<WorldStatePropertyCondition> WorldStatePropertyConditions { get; } = new();
    public List<PlayerStatusPropertyCondition> PlayerStatusPropertyConditions { get; } = new();

    public ActionTemplate(
        string name,
        BasicActionTypes actionType,
        LocationPropertyCondition locationPropertyCondition,
        LocationSpotPropertyCondition locationSpotPropertyCondition,
        WorldStatePropertyCondition worldStatePropertyCondition,
        PlayerStatusPropertyCondition playerStatusPropertyCondition
        )
    {
        Name = name;
        ActionType = actionType;
        if (locationPropertyCondition != null) LocationPropertyConditions.Add(locationPropertyCondition);
        if (locationSpotPropertyCondition != null) LocationSpotPropertyConditions.Add(locationSpotPropertyCondition);
        if (worldStatePropertyCondition != null) WorldStatePropertyConditions.Add(worldStatePropertyCondition);
        if (playerStatusPropertyCondition != null) PlayerStatusPropertyConditions.Add(playerStatusPropertyCondition);
    }


}