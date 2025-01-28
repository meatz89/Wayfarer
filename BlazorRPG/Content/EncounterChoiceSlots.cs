public static class EncounterChoiceSlots
{
    public static List<EncounterChoiceSlot> EncounterBaseChoiceSlots => new()
    {
        new EncounterChoiceSlotBuilder()
            .WithName("Spill Drinks")
            .WithActionType(BasicActionTypes.Labor)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationSpotAccessability(Accessibility.Public)
            .WithEncounterStateCondition(builder => builder
                .WithMaxMomentum(5))
            .WithEncounterChoice(builder => builder
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Mistake)
                .EndsEndcounter(EncounterResults.EncounterFailure))
            .Build(),

    };
}