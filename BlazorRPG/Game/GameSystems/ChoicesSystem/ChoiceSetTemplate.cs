public class ChoiceSetTemplate
{
    public string Name { get; }
    public BasicActionTypes ActionType { get; }
    public LocationArchetypes LocationArchetype { get; set; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; }
    public List<EncounterStateCondition> StateConditions { get; }
    public List<ChoiceTemplate> ChoicePatterns { get; }

    public ChoiceSetTemplate(
        string name,
        BasicActionTypes actionType,
        LocationArchetypes locationArchetype,
        List<LocationPropertyCondition> availabilityConditions,
        List<EncounterStateCondition> stateConditions,
        List<ChoiceTemplate> choicePatterns)
    {
        Name = name;
        ActionType = actionType;
        LocationArchetype = locationArchetype;
        AvailabilityConditions = availabilityConditions;
        StateConditions = stateConditions;
        ChoicePatterns = choicePatterns;
    }
}