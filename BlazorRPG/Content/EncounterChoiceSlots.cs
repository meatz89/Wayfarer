public static class EncounterChoiceSlots
{
    public static List<EncounterChoiceSlot> EncounterBaseChoiceSlots => new()
    {
        new EncounterChoiceSlotBuilder()
            .WithName("Overhear Merchant Plans")
            .WithActionType(BasicActionTypes.Investigate)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationSpotAccessability(Accessibility.Private)
            .WithEncounterStateCondition(builder => builder
                .WithMinInsight(5))
            .WithEncounterChoice(builder => builder
                .WithArchetype(ChoiceArchetypes.Focus)
                .WithApproach(ChoiceApproaches.Strategic)
                .RewardsCoins(1))
            .Build(),

    };
}