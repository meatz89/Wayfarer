public static class SpecialChoicesTemplates
{
    public static List<SpecialChoiceTemplate> SpecialChoiceSet => new()
    {
        new SpecialChoiceTemplateBuilder()
            .WithName("Overhear Merchant Plans")
            .WithActionType(BasicActionTypes.Discuss)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationSpotAccessability(Accessibility.Private)
            .WithEncounterStateCondition(builder =>
                builder.WithMinResonance(7))
            .RewardsKnowledge(KnowledgeTypes.WorkOpportunity, LocationNames.Market)
            .Build(),
    };
}