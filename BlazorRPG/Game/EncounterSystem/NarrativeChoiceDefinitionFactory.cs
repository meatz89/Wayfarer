public static class NarrativeChoiceDefinitionFactory
{
    public static NarrativeChoice BuildChoice(
        string id,
        string name,
        string description,
        Skills skill,
        int difficulty,
        int reward,
        List<string> tags)
    {
        NarrativeChoice choice = new NarrativeChoice(id, name);

        choice.Description = description;
        choice.Skill = skill;
        choice.Difficulty = difficulty;
        choice.Reward = reward;
        choice.Tags = tags;

        return choice;
    }
}
