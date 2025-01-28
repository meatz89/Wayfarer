public static class EncounterChoiceSlots
{
    public static List<EncounterChoiceSlot> EncounterBaseChoiceSlots => new()
    {
        new EncounterChoiceSlotBuilder()
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .WithEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Ask about rates")
                .WithChoiceSlotType(ChoiceSlotTypes.Opportunity)
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Aggressive)
                .UnlocksModifiedChoiceSlot(modifiedBuilder => modifiedBuilder
                    .WithEncounterChoice(modifiedEncounterChoiceBuilder => modifiedEncounterChoiceBuilder
                        .WithName("Pay for room")
                        .WithArchetype(ChoiceArchetypes.Physical)
                        .WithApproach(ChoiceApproaches.Aggressive)
                        .ExpendsCoins(5)
                        .EndsEndcounter(EncounterResults.EncounterSuccess))))
            .Build(),
    };
}
public enum ChoiceSlotTypes
{
    Opportunity, Substitution
}