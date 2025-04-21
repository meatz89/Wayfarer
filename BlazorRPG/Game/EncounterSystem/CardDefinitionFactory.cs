public static class CardDefinitionFactory
{
    public static CardDefinition BuildCard(
        string id,
        string name,
        string description,
        int tier,
        ApproachTags approach,
        int approachPosition,
        FocusTags focus,
        int focusPosition,
        EffectTypes effectType,
        int effectValue,
        List<TagModification> tagModifications,
        EnvironmentalPropertyEffect strategicEffect,
        List<SkillRequirement> unlockRequirements)
    {
        CardDefinition cardDefinition = new CardDefinition();

        cardDefinition.Id = id;
        cardDefinition.Name = name;
        cardDefinition.Description = description;
        cardDefinition.Tier = tier;
        cardDefinition.Approach = approach;
        cardDefinition.OptimalApproachPosition = approachPosition;
        cardDefinition.Focus = focus;
        cardDefinition.OptimalFocusPosition = focusPosition;
        cardDefinition.EffectType = effectType;
        cardDefinition.EffectValue = effectValue;
        cardDefinition.TagModifications = tagModifications;
        cardDefinition.StrategicEffect = strategicEffect;
        cardDefinition.UnlockRequirements = unlockRequirements;

        return cardDefinition;
    }
}
