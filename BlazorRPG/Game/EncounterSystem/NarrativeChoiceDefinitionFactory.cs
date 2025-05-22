public static class NarrativeChoiceDefinitionFactory
{
    public static EncounterOption BuildChoice(
        string id,
        string name,
        string description,
        SkillTypes skill,
        int difficulty,
        int reward,
        List<string> tags)
    {
        EncounterOption choice = new EncounterOption(id, name);
        choice.Description = description;
        choice.Skill = skill;
        choice.Difficulty = difficulty;
        choice.SuccessProgress = reward;
        choice.FailureProgress = 0;
        choice.Tags = tags;

        // Default Universal Encounter System values
        choice.FocusCost = 1;
        choice.ActionType = UniversalActionType.GenerateForce; // Default
        choice.TokenGeneration = new Dictionary<AspectTokenTypes, int>();
        choice.TokenCosts = new Dictionary<AspectTokenTypes, int>();
        choice.NegativeConsequenceType = NegativeConsequenceTypes.ProgressLoss;

        return choice;
    }

    public static EncounterOption BuildUniversalChoice(
        string id,
        string name,
        string description,
        SkillTypes skill,
        int difficulty,
        int focusCost,
        UniversalActionType actionType,
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
        choice.SuccessProgress = CalculateProgressForActionType(actionType, tokenCosts);
        choice.FailureProgress = 0;

        return choice;
    }

    private static int CalculateProgressForActionType(UniversalActionType actionType, Dictionary<AspectTokenTypes, int> tokenCosts)
    {
        return actionType switch
        {
            UniversalActionType.SafetyOption => 0,
            UniversalActionType.GenerateForce => 0,
            UniversalActionType.GenerateFlow => 0,
            UniversalActionType.GenerateFocus => 0,
            UniversalActionType.GenerateFortitude => 0,
            UniversalActionType.BasicConversion => 3, // 2 tokens → 3 progress
            UniversalActionType.SpecializedConversion => 2, // 2 tokens → 2 progress + bonus
            UniversalActionType.PremiumConversion => 4, // 3 tokens → 4 progress
            _ => 1
        };
    }
}