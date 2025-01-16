public class ChoiceSetTemplate
{
    public string Name { get; }
    public List<CompositionPattern> CompositionPatterns { get; }
    public BasicActionTypes ActionType { get; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; }
    public List<EncounterStateCondition> StateConditions { get; }

    public ChoiceSetTemplate(
        string name,
        List<CompositionPattern> compositionPatterns,
        BasicActionTypes actionType,
        List<LocationPropertyCondition> availabilityConditions,
        List<EncounterStateCondition> encounterStateConditions)
    {
        Name = name;
        CompositionPatterns = compositionPatterns;
        ActionType = actionType;
        AvailabilityConditions = availabilityConditions;
        StateConditions = encounterStateConditions;
    }
}