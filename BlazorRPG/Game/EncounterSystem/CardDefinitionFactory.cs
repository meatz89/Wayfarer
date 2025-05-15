public static class CardDefinitionFactory
{
    public static CardDefinition BuildCard(
        string id,
        string name,
        string description,
        CardTypes type,
        Skills skill,
        int level,
        int cost,
        List<string> tags)
    {
        CardDefinition cardDefinition = new CardDefinition(id, name);

        cardDefinition.Description = description;
        cardDefinition.Type = type;
        cardDefinition.Skill = skill;
        cardDefinition.Level = level;
        cardDefinition.Cost = cost;
        cardDefinition.Tags = tags;

        return cardDefinition;
    }
}
