public class EncounterChoiceTemplate
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public ChoiceArchetypes ChoiceArchetype { get; set; }
    public ChoiceApproaches ChoiceApproach { get; set; }

    public EncounterChoiceTemplate(
        string name,
        BasicActionTypes actionType,
        ChoiceArchetypes choiceArchetype,
        ChoiceApproaches choiceApproach
        )
    {
        Name = name;
        ActionType = actionType;
        ChoiceArchetype = choiceArchetype;
        ChoiceApproach = choiceApproach;
    }
}