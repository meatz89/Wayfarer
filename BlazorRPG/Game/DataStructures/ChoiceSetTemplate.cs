public class ChoiceSetTemplate
{
    public string Name { get; }
    public BasicActionTypes ActionType { get; }
    public List<ChoicePatternComposition> CompositionPatterns { get; }
    public List<LocationPropertyCondition> AvailabilityConditions { get; }
    public List<EncounterStateCondition> StateConditions { get; }


    public ChoiceSetTemplate(
        string name,
        BasicActionTypes actionType,
        List<ChoicePatternComposition> compositionPatterns,
        List<LocationPropertyCondition> availabilityConditions,
        List<EncounterStateCondition> stateConditions)
    {
        Name = name;
        ActionType = actionType;
        CompositionPatterns = compositionPatterns;
        AvailabilityConditions = availabilityConditions;
        StateConditions = stateConditions;
    }
}
