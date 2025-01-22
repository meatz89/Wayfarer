public class ChoiceSetTemplate
{
    public string Name { get; set; }
    public CompositionPattern CompositionPattern { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; set; }
    public List<EncounterStateCondition> StateConditions { get; set; }

    public ChoiceSetTemplate()
    {
    }

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