public class ChoiceSetTemplate
{
    public string Name { get; }
    public BasicActionTypes ActionType { get; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; }
    public List<EncounterStateCondition> StateConditions { get; }
    public List<ChoiceTemplate> ChoicePatterns { get; }

    public ChoiceSetTemplate(
        string name,
        BasicActionTypes actionType,
        List<LocationPropertyCondition> availabilityConditions,
        List<EncounterStateCondition> stateConditions,
        List<ChoiceTemplate> choicePatterns)
    {
        Name = name;
        ActionType = actionType;
        AvailabilityConditions = availabilityConditions;
        StateConditions = stateConditions;
        ChoicePatterns = choicePatterns;
    }
}