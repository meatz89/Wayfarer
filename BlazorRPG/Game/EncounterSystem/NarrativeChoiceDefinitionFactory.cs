public static class NarrativeChoiceDefinitionFactory
{
    public static EncounterOption BuildChoice(
        string id,
        string name,
        string description,
        SkillTypes skill,
        int difficulty,
        int focusCost,
        UniversalActionTypes actionType,
        NegativeConsequenceTypes negativeConsequence,
        List<string> tags)
    {
        EncounterOption choice = new EncounterOption(id, name);
        choice.Description = description;
        choice.Skill = skill;
        choice.Difficulty = difficulty;
        choice.FocusCost = focusCost;
        choice.ActionType = actionType;
        choice.NegativeConsequenceType = negativeConsequence;
        choice.Tags = tags ?? new List<string>();

        return choice;
    }

}