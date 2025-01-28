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
                .WithChoiceSlotType(ChoiceSlotPersistence.Enduring)
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Aggressive)
                .UnlocksModifiedChoiceSlot(modifiedBuilder => modifiedBuilder
                    .WithEncounterChoice(modifiedEncounterChoiceBuilder => modifiedEncounterChoiceBuilder
                        .WithName("Pay for room")
                        .WithArchetype(ChoiceArchetypes.Physical)
                        .WithApproach(ChoiceApproaches.Strategic)
                        .ExpendsCoins(5)
                        .EndsEndcounter(EncounterResults.EncounterSuccess)
                    )
                )
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .WithEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Ask about area")
                .WithChoiceSlotType(ChoiceSlotPersistence.Enduring)
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .RewardsLocationInformation(LocationNames.Market)
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .WithEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Say Goodbye")
                .WithChoiceSlotType(ChoiceSlotPersistence.Enduring)
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .EndsEndcounter(EncounterResults.EncounterSuccess)
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .WithEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Say Goodbye")
                .WithChoiceSlotType(ChoiceSlotPersistence.Enduring)
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .EndsEndcounter(EncounterResults.EncounterSuccess)
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Labor)
            .WithEncounterStateCondition(encounterState => encounterState
                .WithMaxMomentum(2)
            )
            .WithEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Drop Tray")
                .WithChoiceSlotType(ChoiceSlotPersistence.Fleeting)
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Mistake)
                .RewardsReputation(-1)
            )
            .Build(),
    };
}
