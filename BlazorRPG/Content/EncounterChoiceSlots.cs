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
                .RewardsCoins(1))
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithName("Overhear Merchant Plans")
            .WithActionType(BasicActionTypes.Discuss)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationSpotAccessability(Accessibility.Private)
            .WithEncounterStateCondition(builder => builder
                .WithMinResonance(7))
            .WithEncounterChoice(builder => builder
                .RewardsCoins(1))
            .Build(),
    };
}