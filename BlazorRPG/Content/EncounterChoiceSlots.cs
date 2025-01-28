public static class EncounterChoiceSlots
{
    public static List<EncounterChoiceSlot> EncounterBaseChoiceSlots => new()
    {
        new EncounterChoiceSlotBuilder()
            .WithName("Spill Drinks")
            .WithActionType(BasicActionTypes.Labor)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationSpotAccessability(Accessibility.Public)
            .WithEncounterStateCondition(encounterStateBuilder => encounterStateBuilder
                .WithMaxMomentum(5))
            .WithEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Mistake)
                .UnlocksModifiedChoiceSlot(modifiedBuilder => modifiedBuilder
                    .WithName("Spill Drinks Result")
                    .WithActionType(BasicActionTypes.Labor)
                    .WithLocationArchetype(LocationArchetypes.Tavern)
                    .WithLocationSpotAccessability(Accessibility.Public)
                    .WithEncounterStateCondition(modifiedEncounterStateBuilder => modifiedEncounterStateBuilder
                        .WithMaxMomentum(5))
                    .WithEncounterChoice(modifiedEncounterChoiceBuilder => modifiedEncounterChoiceBuilder
                        .WithArchetype(ChoiceArchetypes.Physical)
                        .WithApproach(ChoiceApproaches.Aggressive)
                        .EndsEndcounter(EncounterResults.EncounterFailure))))
            .Build(),
    };
}