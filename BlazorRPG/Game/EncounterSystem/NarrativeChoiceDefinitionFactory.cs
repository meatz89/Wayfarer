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
        Dictionary<AspectTokenTypes, int> tokenGeneration,
        Dictionary<AspectTokenTypes, int> tokenCosts,
        NegativeConsequenceTypes negativeConsequence,
        List<string> tags)
    {
        EncounterOption choice = new EncounterOption(id, name);
        choice.Description = description;
        choice.Skill = skill;
        choice.Difficulty = difficulty;
        choice.FocusCost = focusCost;
        choice.ActionType = actionType;
        choice.TokenGeneration = tokenGeneration ?? new Dictionary<AspectTokenTypes, int>();
        choice.TokenCosts = tokenCosts ?? new Dictionary<AspectTokenTypes, int>();
        choice.NegativeConsequenceType = negativeConsequence;
        choice.Tags = tags ?? new List<string>();

        // Set progress based on action type
        choice.SuccessProgress = 1;
        choice.FailureProgress = 0;

        return choice;
    }

}