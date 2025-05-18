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

        return choice;
    }
}
