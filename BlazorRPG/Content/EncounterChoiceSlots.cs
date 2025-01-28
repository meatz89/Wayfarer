public static class EncounterChoiceSlots
{
    public static List<EncounterChoiceSlot> EncounterBaseChoiceSlots => new()
    {
        new EncounterChoiceSlotBuilder()
            .WithName("Overhear Merchant Plans")
            .WithActionType(BasicActionTypes.Discuss)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationSpotAccessability(Accessibility.Private)
            .WithEncounterStateCondition(builder => builder
                .WithMinResonance(7))
            .WithEncounterChoice(builder => builder
                .RewardsKnowledge(KnowledgeTypes.WorkOpportunity, LocationNames.Market))
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithName("Overhear Merchant Plans")
            .WithActionType(BasicActionTypes.Discuss)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationSpotAccessability(Accessibility.Private)
            .WithEncounterStateCondition(builder => builder
                .WithMinResonance(7))
            .WithEncounterChoice(builder => builder
                .RewardsKnowledge(KnowledgeTypes.WorkOpportunity, LocationNames.Market))
            .Build(),
    };
}