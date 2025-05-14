public static class CardDefinitionFactory
{
    public static CardDefinition BuildCard(
        string id,
        string name,
        string description,
        int tier,
        EffectTypes effectType,
        int effectValue,
        EnvironmentalPropertyEffect strategicEffect,
        List<SkillRequirement> unlockRequirements)
    {
        CardDefinition cardDefinition = new CardDefinition();

        cardDefinition.Id = id;
        cardDefinition.Id = name;
        cardDefinition.Description = description;
        cardDefinition.Tier = tier;
        cardDefinition.EffectType = effectType;
        cardDefinition.EffectValue = effectValue;
        cardDefinition.StrategicEffect = strategicEffect;
        cardDefinition.UnlockRequirements = unlockRequirements;

        return cardDefinition;
    }
}
