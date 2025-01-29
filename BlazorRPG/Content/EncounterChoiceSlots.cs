public static class EncounterChoiceSlots
{
    public static List<EncounterChoiceSlot> EncounterBaseChoiceSlots => new()
    {
        new EncounterChoiceSlotBuilder()
            .WithChoiceSlotType(ChoiceSlotPersistence.Lasting)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .AddEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Ask about rates")
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Aggressive)
                .UnlocksModifiedChoiceSlot(modifiedBuilder => modifiedBuilder
                    .AddEncounterChoice(modifiedEncounterChoiceBuilder => modifiedEncounterChoiceBuilder
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
            .WithChoiceSlotType(ChoiceSlotPersistence.Lasting)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .AddEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Ask for work")
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .UnlockLocationSpotActions(LocationNames.WaysideInn, BasicActionTypes.Labor)
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithChoiceSlotType(ChoiceSlotPersistence.Lasting)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .AddEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Ask about area")
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .RewardsLocationInformation(LocationNames.Market)
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithChoiceSlotType(ChoiceSlotPersistence.Lasting)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .AddEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Say Goodbye")
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .EndsEndcounter(EncounterResults.EncounterSuccess)
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithChoiceSlotType(ChoiceSlotPersistence.Lasting)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Persuade)
            .AddEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Say Goodbye")
                .WithArchetype(ChoiceArchetypes.Social)
                .WithApproach(ChoiceApproaches.Tactical)
                .EndsEndcounter(EncounterResults.EncounterSuccess)
            )
            .Build(),

        new EncounterChoiceSlotBuilder()
            .WithChoiceSlotType(ChoiceSlotPersistence.Fleeting)
            .WithLocationArchetype(LocationArchetypes.Tavern)
            .WithLocationOpportunity(OpportunityTypes.Commercial)
            .WithActionType(BasicActionTypes.Labor)
            .WithEncounterStateCondition(encounterState => encounterState
                .WithMaxMomentum(2)
            )
            .AddEncounterChoice(encounterChoiceBuilder => encounterChoiceBuilder
                .WithName("Drop Tray")
                .WithArchetype(ChoiceArchetypes.Physical)
                .WithApproach(ChoiceApproaches.Mistake)
                .RewardsReputation(-1)
            )
            .Build(),
    };
}
