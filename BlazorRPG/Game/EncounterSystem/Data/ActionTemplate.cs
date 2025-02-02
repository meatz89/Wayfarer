public class ActionTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public bool IsEncounterAction { get; }
    public List<Requirement> Requirements { get; }
    public List<Outcome> Energy { get; }
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }
    public List<LocationPropertyCondition> LocationPropertyConditions { get; } = new();
    public List<LocationSpotPropertyCondition> LocationSpotPropertyConditions { get; } = new();
    public List<WorldStatePropertyCondition> WorldStatePropertyConditions { get; } = new();
    public List<PlayerStatusPropertyCondition> PlayerStatusPropertyConditions { get; } = new();

    public ActionTemplate(
        string name,
        BasicActionTypes actionType,
        bool isEncounterAction,
        LocationPropertyCondition locationPropertyCondition,
        LocationSpotPropertyCondition locationSpotPropertyCondition,
        WorldStatePropertyCondition worldStatePropertyCondition,
        PlayerStatusPropertyCondition playerStatusPropertyCondition
,
        List<Requirement> requirements,
        List<Outcome> energy,
        List<Outcome> costs,
        List<Outcome> rewards)
    {
        Name = name;
        ActionType = actionType;
        IsEncounterAction = isEncounterAction;
        Requirements = requirements;
        Energy = energy;
        Costs = costs;
        Rewards = rewards;

        if (locationPropertyCondition != null) LocationPropertyConditions.Add(locationPropertyCondition);
        if (locationSpotPropertyCondition != null) LocationSpotPropertyConditions.Add(locationSpotPropertyCondition);
        if (worldStatePropertyCondition != null) WorldStatePropertyConditions.Add(worldStatePropertyCondition);
        if (playerStatusPropertyCondition != null) PlayerStatusPropertyConditions.Add(playerStatusPropertyCondition);
    }


}