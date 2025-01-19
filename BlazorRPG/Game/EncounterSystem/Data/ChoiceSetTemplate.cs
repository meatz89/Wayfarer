public class ChoiceSetTemplate
{
    public string Name { get; }
    public CompositionPattern CompositionPattern { get; }
    public BasicActionTypes ActionType { get; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; }
    public List<EncounterStateCondition> StateConditions { get; }

    public ChoiceSetTemplate(
        string name,
        CompositionPattern compositionPattern,
        BasicActionTypes actionType,
        List<LocationPropertyCondition> availabilityConditions,
        List<EncounterStateCondition> encounterStateConditions)
    {
        Name = name;
        CompositionPattern = compositionPattern;
        ActionType = actionType;
        AvailabilityConditions = availabilityConditions;
        StateConditions = encounterStateConditions;
    }
}